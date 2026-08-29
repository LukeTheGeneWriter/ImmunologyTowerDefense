using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Something a dendritic cell is, from the lymph node's point of view --
    /// so <see cref="LymphNode"/> can run pairing without depending on the
    /// concrete <c>DendriticCell</c> type (which lands a commit later).
    /// </summary>
    public interface INodeVisitor
    {
        /// <summary>Node-local fine-tile position.</summary>
        FineCoord NodePos { get; }

        /// <summary>The antigen barcode this DC is carrying.</summary>
        byte Cargo { get; }

        /// <summary>The species that antigen belongs to -- the knowledge key.</summary>
        PathogenClass CargoClass { get; }

        /// <summary>False once the cargo is spent; a spent DC does not pair.</summary>
        bool HasCargo { get; }

        /// <summary>Set by the node while locked in a pairing.</summary>
        bool Frozen { get; set; }

        /// <summary>Node tells the DC a pairing just resolved.
        /// <paramref name="taught"/> is whether the barcodes matched. The DC
        /// decrements its remaining presentations either way (§5a step 4).</summary>
        void OnPairingResolved(bool taught);
    }

    /// <summary>
    /// The lymph node as a real arena (GAME_DESIGN.md §5a/§5c) -- a
    /// non-functional backdrop since Sprint 2, now a small bounded space
    /// where dendritic cells and helper-T cells move around and collide.
    ///
    /// ## Its own coordinate space, reusing the tissue machinery
    ///
    /// The node owns a small dedicated <see cref="BoardConfig"/> (6x6 coarse
    /// = 42x42 fine) and its own <see cref="CytokineField"/> -- the
    /// **co-localisation signal** of §5c step 4, a *different* signal from
    /// the infection cytokine. Both DCs and helper-T cells bias toward it via
    /// the exact same <see cref="Units.Chemotaxis"/> path the tissue units
    /// use, so meetings reliably happen instead of relying on two random
    /// walks intersecting. The field is recomputed every node tick from a
    /// fixed central source plus each resident lymphocyte as a weak source,
    /// so a DC drifts toward where the T cells actually are. The field is
    /// small enough that <c>strength / (1 + distance)</c> is steep across it
    /// -- the flat-at-scale problem measured in Sprint 4 is a large-map
    /// problem.
    ///
    /// ## Explicit time
    ///
    /// <see cref="Tick"/> takes deltaTime + currentTime, driven by
    /// <see cref="AdaptiveDirector"/> in a build and by a simulated clock in
    /// the harness -- same convention as TissueGrid / BoneMarrowManager /
    /// GutInterface. Agents' Update() only tween.
    /// </summary>
    public class LymphNode
    {
        private const int NodeCoarseSize = 6;

        private readonly BoardConfig nodeBoard;
        private readonly CytokineField coloc;
        private readonly KnowledgeLedger knowledge;

        private readonly List<Lymphocyte> residents = new List<Lymphocyte>();
        private readonly List<INodeVisitor> visitors = new List<INodeVisitor>();

        private struct ActivePair
        {
            public INodeVisitor Visitor;
            public Lymphocyte Resident;
            public float ResolveAt;
        }
        private readonly List<ActivePair> pairs = new List<ActivePair>();

        // Reused each Step so Recompute's source enumeration allocates nothing.
        private readonly List<(CoarseCoord Coord, float Strength)> sourceBuffer =
            new List<(CoarseCoord, float)>(16);

        /// <summary>World-space rectangle the node is drawn inside (the lymph
        /// backdrop). Agents render here via <see cref="NodeToWorld"/>.</summary>
        public Rect WorldRect { get; private set; }

        public BoardConfig NodeBoard => nodeBoard;
        public CytokineField Coloc => coloc;

        public int ResidentCount => residents.Count;
        public int VisitorCount => visitors.Count;

        public IReadOnlyList<Lymphocyte> Residents => residents;

        public LymphNode(KnowledgeLedger knowledge, Rect worldRect)
        {
            this.knowledge = knowledge;
            WorldRect = worldRect;

            var go = new GameObject("LymphNodeBoard");
            nodeBoard = go.AddComponent<BoardConfig>();
            // All-tissue-band 6x6; the node does not use bands, it just needs
            // a bounded fine lattice and FineToWorld-free coordinates.
            nodeBoard.ConfigureForTest(
                NodeCoarseSize, NodeCoarseSize,
                BoardAxis.Horizontal, AxisEnd.Negative, 0, 0, AxisEnd.Positive);

            coloc = new CytokineField(nodeBoard);
        }

        /// <summary>Approx world size of one node agent's square.</summary>
        public float AgentWorldSize =>
            Mathf.Min(WorldRect.width / nodeBoard.FineColumns,
                      WorldRect.height / nodeBoard.FineRows) * 2.6f;

        /// <summary>Maps a node-local fine tile into <see cref="WorldRect"/>.
        /// Row 0 is the top edge, matching BoardConfig.FineToWorld.</summary>
        public Vector3 NodeToWorld(FineCoord fine)
        {
            float u = (fine.Column + 0.5f) / nodeBoard.FineColumns;
            float v = (fine.Row + 0.5f) / nodeBoard.FineRows;
            return new Vector3(
                WorldRect.xMin + u * WorldRect.width,
                WorldRect.yMax - v * WorldRect.height,
                0f);
        }

        /// <summary>A random fine tile a few tiles in from the node edge, for
        /// spawning an agent.</summary>
        public FineCoord RandomInteriorFine()
        {
            int margin = BoardConfig.FineSubdivision / 2;
            return new FineCoord(
                Random.Range(margin, nodeBoard.FineColumns - margin),
                Random.Range(margin, nodeBoard.FineRows - margin));
        }

        public void RegisterResident(Lymphocyte l) { if (l != null && !residents.Contains(l)) residents.Add(l); }

        public void UnregisterResident(Lymphocyte l)
        {
            residents.Remove(l);
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                if (pairs[i].Resident != l) continue;
                pairs[i].Visitor.Frozen = false;
                pairs.RemoveAt(i);
            }
        }

        public void Admit(INodeVisitor v) { if (v != null && !visitors.Contains(v)) visitors.Add(v); }

        public void Release(INodeVisitor v)
        {
            visitors.Remove(v);
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                if (pairs[i].Visitor != v) continue;
                if (pairs[i].Resident != null) pairs[i].Resident.Frozen = false;
                pairs.RemoveAt(i);
            }
        }

        /// <summary>
        /// One logical node step (the tick gate lives in
        /// <see cref="AdaptiveDirector"/>, which also sub-steps the DCs, so
        /// there is one clock for the whole arena): recompute the
        /// co-localisation field, resolve and form pairings, move residents,
        /// age residents out. DCs are moved by their own
        /// <c>SimulationTick</c> (their state machine also runs a tissue-side
        /// walk); this reads their <see cref="INodeVisitor.NodePos"/> and
        /// sets their <see cref="INodeVisitor.Frozen"/>.
        /// </summary>
        public void Step(float currentTime)
        {
            RecomputeColoc();
            ResolvePairs(currentTime);
            FormPairs(currentTime);

            for (int i = 0; i < residents.Count; i++) residents[i].NodeTick(currentTime);

            AgeOutResidents(currentTime);
        }

        private void RecomputeColoc()
        {
            sourceBuffer.Clear();
            int c = NodeCoarseSize / 2;
            sourceBuffer.Add((new CoarseCoord(c, c), AdaptiveTuning.NodeColocalisationSourceStrength));
            for (int i = 0; i < residents.Count; i++)
            {
                var rc = residents[i].Node.ToCoarse(BoardConfig.FineSubdivision);
                sourceBuffer.Add((rc, AdaptiveTuning.NodeLymphocyteSourceStrength));
            }
            coloc.Recompute(sourceBuffer);
        }

        private void ResolvePairs(float currentTime)
        {
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                if (currentTime < pairs[i].ResolveAt) continue;
                var p = pairs[i];
                pairs.RemoveAt(i);

                bool taught = p.Visitor != null && p.Resident != null &&
                              p.Visitor.HasCargo &&
                              Antigen.IsMatch(p.Visitor.Cargo, p.Resident.Tag);

                if (p.Visitor != null) { p.Visitor.Frozen = false; p.Visitor.OnPairingResolved(taught); }
                if (p.Resident != null) p.Resident.Frozen = false;

                if (taught)
                {
                    knowledge.Add(p.Visitor.CargoClass, AdaptiveTuning.KnowledgePerMatch);
                    DegranulationFlash.Play(
                        NodeToWorld(p.Resident.Node),
                        AgentWorldSize * 2.2f,
                        DegranulationFlash.KnowledgeMatchColor);
                }
            }
        }

        private void FormPairs(float currentTime)
        {
            int r = AdaptiveTuning.NodePairingContactFineTiles;
            for (int vi = 0; vi < visitors.Count; vi++)
            {
                var v = visitors[vi];
                if (v.Frozen || !v.HasCargo) continue;

                for (int ri = 0; ri < residents.Count; ri++)
                {
                    var res = residents[ri];
                    if (res.Frozen) continue;

                    int dc = Mathf.Abs(res.Node.Column - v.NodePos.Column);
                    int dr = Mathf.Abs(res.Node.Row - v.NodePos.Row);
                    if (Mathf.Max(dc, dr) > r) continue;

                    v.Frozen = true;
                    res.Frozen = true;
                    pairs.Add(new ActivePair
                    {
                        Visitor = v,
                        Resident = res,
                        ResolveAt = currentTime + AdaptiveTuning.PairingSeconds,
                    });
                    break; // this visitor is busy now
                }
            }
        }

        private void AgeOutResidents(float currentTime)
        {
            for (int i = residents.Count - 1; i >= 0; i--)
            {
                var res = residents[i];
                if (res.Frozen) continue; // don't yank a cell mid-pairing
                if (currentTime - res.BornAt < AdaptiveTuning.LymphocyteLifespanSeconds) continue;
                res.DespawnToPool(); // its onDespawn calls UnregisterResident + pool release
            }
        }
    }
}
