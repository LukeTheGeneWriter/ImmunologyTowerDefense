using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Adaptive
{
    /// <summary>**Sprint 14:** collapsed from four states to two. A DC's
    /// whole tissue life is one <see cref="PatrolTissue"/> oscillation
    /// base↔lumen (the old separate TravelToNode / ReturnToTissue beelines
    /// are gone -- they were why the DC never visibly paced the band): it
    /// paces out toward the lumen and back, biases toward the base only
    /// while carrying antigen, enters the node at the base edge, and
    /// resumes pacing after.</summary>
    public enum DendriticCellState { PatrolTissue, InNode }

    /// <summary>
    /// The dendritic-cell shuttle of GAME_DESIGN.md §5a. A DC emitted by its
    /// progenitor:
    ///
    ///  1. **Patrols tissue** its entire life -- one continuous oscillation
    ///     base↔lumen (<see cref="TickPatrol"/>), with cross-axis repulsion
    ///     from the other DCs so the cohort spreads across the lanes. Debris
    ///     homing is a deferred BACKLOG item. Standing on a `Dead` cell whose
    ///     debris carries an antigen, an empty DC **samples**: picks up that
    ///     antigen as cargo and eats one bite of the pile (competing with
    ///     macrophage efferocytosis, §1c).
    ///  2. **Carrying antigen**, the same patrol walk biases toward the base
    ///     (never a hardcoded direction). Reaching the base band it enters
    ///     the lymph node.
    ///  3. **In the node** it wanders the co-localisation gradient among the
    ///     helper-T cells (<see cref="LymphNode"/> runs the pairing). Each
    ///     pairing spends one presentation whether or not the barcodes
    ///     matched.
    ///  4. When the cargo is spent it **returns to tissue** empty (§5a's
    ///     "the DC eventually loses its cargo and must return" -- it does not
    ///     die; the round trip is the cost) and resumes pacing the band.
    ///
    /// **Sprint 14** collapsed the old four-state machine (PatrolTissue /
    /// TravelToNode / InNode / ReturnToTissue) to two -- the separate
    /// travel/return beelines were straight dashes with no oscillation and no
    /// repulsion, and with debris everywhere in a dense round a DC spent
    /// almost its whole life in them, so it never visibly paced the band.
    ///
    /// Explicit-time <see cref="SimulationTick"/>, driven by
    /// <see cref="AdaptiveDirector"/>. Update() only tweens -- and the tween
    /// deliberately slides the sprite across the gap between the tissue edge
    /// and the node when it changes space, so "the DC went to the lymph
    /// node" reads on screen.
    /// </summary>
    public class DendriticCell : MonoBehaviour, INodeVisitor
    {
        private const int TissueFootprintFineTiles = 4;

        private BoardConfig tissueBoard;
        private TissueGrid tissueGrid;
        private CytokineField tissueCytokine;
        private LymphNode node;
        private SpriteRenderer sr;
        private System.Action<DendriticCell> onDespawn;

        /// <summary>Live, read-only view of every fielded DC (AdaptiveDirector
        /// owns the list). Read during patrol for lane-repulsion; never
        /// mutated here.</summary>
        private IReadOnlyList<DendriticCell> cohort;

        private readonly FineCoord[] candidateBuffer = new FineCoord[4];
        private readonly float[] weightBuffer = new float[4];

        public DendriticCellState State { get; private set; }

        /// <summary>Tissue-space fine position (all states except InNode).</summary>
        public FineCoord Current { get; private set; }

        /// <summary>Node-local fine position (InNode).</summary>
        public FineCoord NodePos { get; private set; }

        public byte Cargo { get; private set; }
        public PathogenClass CargoClass { get; private set; }
        public bool HasCargo { get; private set; }
        public bool Frozen { get; set; }

        private int presentationsLeft;

        /// <summary>+1 = sweeping toward the lumen edge, -1 = back toward the
        /// base edge. Flipped at the tissue-band edges so the DC paces the
        /// full depth of the band (Sprint 12); Sprint 14 runs this the DC's
        /// whole tissue life, and pins it to -1 while carrying antigen so a
        /// loaded DC heads to the node.</summary>
        private int patrolHeading = 1;

        private static readonly Color EmptyColor = new Color(0.72f, 0.30f, 0.68f);  // dendritic magenta
        private static readonly Color CargoColor = new Color(0.98f, 0.62f, 0.98f);  // brighter -- carrying antigen

        private Vector3 tweenStart;
        private Vector3 tweenEnd;
        private float tweenTimer;

        public void Initialize(
            BoardConfig tissueBoard, TissueGrid tissueGrid, CytokineField tissueCytokine,
            LymphNode node, FineCoord tissueStart, System.Action<DendriticCell> onDespawn,
            IReadOnlyList<DendriticCell> cohort = null)
        {
            this.tissueBoard = tissueBoard;
            this.tissueGrid = tissueGrid;
            this.tissueCytokine = tissueCytokine;
            this.node = node;
            this.onDespawn = onDespawn;
            this.cohort = cohort;

            State = DendriticCellState.PatrolTissue;
            Current = tissueStart;
            HasCargo = false;
            Frozen = false;
            presentationsLeft = 0;
            patrolHeading = 1;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteShapes.DendriteStar; // Sprint 13
            sr.color = EmptyColor;
            sr.enabled = true;
            sr.sortingOrder = 13;
            ApplyScale();

            tweenStart = tweenEnd = tissueBoard.FineToWorld(Current);
            transform.position = tweenStart;
            tweenTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds);
        }

        // -- INodeVisitor --
        void INodeVisitor.OnPairingResolved(bool taught)
        {
            presentationsLeft--;
            if (presentationsLeft <= 0) HasCargo = false; // spent -> will leave on its next InNode tick
        }

        /// <summary>One logical tick of the shuttle state machine.</summary>
        public void SimulationTick(float currentTime)
        {
            if (tissueBoard == null) return;
            tweenStart = transform.position;

            switch (State)
            {
                case DendriticCellState.PatrolTissue: TickPatrol(currentTime); break;
                case DendriticCellState.InNode: TickInNode(); break;
            }

            tweenEnd = WorldPos();
            if (sr != null)
            {
                sr.color = HasCargo ? CargoColor : EmptyColor;
                sr.sprite = HasCargo ? SpriteShapes.DendriteStarLoaded : SpriteShapes.DendriteStar; // Sprint 13
            }
        }

        private void TickPatrol(float currentTime)
        {
            // Set the sweep heading BEFORE stepping. Carrying antigen ->
            // head to the base to deliver. Otherwise oscillate: flip at
            // each tissue-band edge so the DC paces the full depth.
            int axisC = tissueBoard.AxisIndex(Current.ToCoarse(BoardConfig.FineSubdivision));
            if (HasCargo) patrolHeading = -1;
            else if (axisC >= tissueBoard.TissueLumenEdgeAxisIndex) patrolHeading = -1;
            else if (axisC <= tissueBoard.TissueBaseEdgeAxisIndex) patrolHeading = 1;

            for (int i = 0; i < AdaptiveTuning.DcFineTilesPerTick; i++)
                Current = RepelledPatrolStep(Current);

            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);

            // Carrying antigen and back at the base -> into the node.
            if (HasCargo && tissueBoard.BandOf(coarse) == BoardBand.Base)
            {
                EnterNode();
                return;
            }

            // Empty and standing on debris -> sample. No state change: the
            // DC keeps pacing, now heading back toward the base to deliver.
            if (HasCargo || tissueGrid.GetHostState(coarse) != HostState.Dead) return;
            var antigen = tissueGrid.GetDebrisAntigen(coarse);
            if (!antigen.HasValue) return;

            // (§1c -- a sampling DC is also a clearing DC, so it competes
            // with macrophage efferocytosis for the same debris.)
            Cargo = Antigen.ForClass(antigen.Value);
            CargoClass = antigen.Value;
            HasCargo = true;
            presentationsLeft = AdaptiveTuning.DcPresentationsPerCargo;
            tissueGrid.ClearDebris(coarse, AdaptiveTuning.DcDebrisSamplePerBite, currentTime);
        }

        private void TickInNode()
        {
            if (!HasCargo) { LeaveNode(); return; }
            if (Frozen) return;

            for (int i = 0; i < AdaptiveTuning.LymphocyteFineTilesPerTick; i++)
            {
                NodePos = Chemotaxis.ChooseNextStep(
                    NodePos, node.NodeBoard, node.Coloc, true, candidateBuffer, weightBuffer);
            }
        }

        private void EnterNode()
        {
            node.Admit(this);
            NodePos = node.RandomInteriorFine();
            State = DendriticCellState.InNode;
            ApplyScale();
        }

        private void LeaveNode()
        {
            node.Release(this);
            Current = tissueBoard.CoarseCenterFine(tissueBoard.CoarseFromAxis(
                tissueBoard.TissueBaseEdgeAxisIndex, Random.Range(0, tissueBoard.CrossLength)));
            HasCargo = false;      // cargo was spent in the node
            patrolHeading = 1;     // head back out to pace the band
            State = DendriticCellState.PatrolTissue;
            ApplyScale();
        }

        /// <summary>One fine step of the DC's tissue walk. A random walk
        /// with two biases, both in the axis frame so no world direction is
        /// hardcoded:
        ///
        ///  - **lane repulsion** — CROSS-axis steps are biased away from the
        ///    local density of other tissue DCs, so DCs spread across the
        ///    lanes;
        ///  - **the sweep** — THREAT-axis steps are biased toward
        ///    <see cref="patrolHeading"/> (set by <see cref="TickPatrol"/>:
        ///    toward the base while carrying antigen, otherwise oscillating
        ///    between the band edges).
        ///
        /// Both work at FINE-tile granularity (fixed Sprint 12 — the earlier
        /// version compared coarse-cell indices, so a bias fired only ~1
        /// step in 7). `DcLaneRepelStrength` / `DcPatrolSweepBias` = 0
        /// disable each independently.</summary>
        private FineCoord RepelledPatrolStep(FineCoord from)
        {
            int myCross = tissueBoard.FineCrossIndex(from);
            int myAxis = tissueBoard.FineAxisIndex(from);

            // Cross-axis crowd gradient from other patrolling DCs, distances
            // in coarse-cell units. > 0 => the crowd is at lower lanes,
            // drift to higher ones; < 0 the reverse.
            float crowd = 0f;
            if (cohort != null && AdaptiveTuning.DcLaneRepelStrength != 0f)
            {
                float axisRangeFine = AdaptiveTuning.DcLaneRepelAxisRange * BoardConfig.FineSubdivision;
                for (int i = 0; i < cohort.Count; i++)
                {
                    var o = cohort[i];
                    if (o == null || o == this || o.State == DendriticCellState.InNode) continue;
                    if (Mathf.Abs(tissueBoard.FineAxisIndex(o.Current) - myAxis) > axisRangeFine) continue;
                    float d = (myCross - tissueBoard.FineCrossIndex(o.Current)) / (float)BoardConfig.FineSubdivision;
                    crowd += Mathf.Abs(d) < 0.001f
                        ? (Random.value < 0.5f ? 0.5f : -0.5f) // exactly stacked -- break the tie
                        : Mathf.Sign(d) / (1f + Mathf.Abs(d));
                }
            }

            int n = 0;
            foreach (var off in FineCoord.VonNeumannOffsets)
            {
                var cand = from.Add(off);
                if (!tissueBoard.InFineBounds(cand)) continue;
                int crossDir = tissueBoard.FineCrossIndex(cand) - myCross; // +-1 for a lane step, 0 for an axis step
                int axisDir = tissueBoard.FineAxisIndex(cand) - myAxis;    // +-1 for an axis step, 0 for a lane step
                candidateBuffer[n] = cand;
                weightBuffer[n] = crossDir != 0
                    ? Mathf.Exp(AdaptiveTuning.DcLaneRepelStrength * crossDir * crowd)
                    : Mathf.Exp(AdaptiveTuning.DcPatrolSweepBias * axisDir * patrolHeading);
                n++;
            }
            if (n == 0) return from;

            float total = 0f;
            for (int i = 0; i < n; i++) total += weightBuffer[i];
            float pick = Random.value * total;
            float running = 0f;
            for (int i = 0; i < n; i++)
            {
                running += weightBuffer[i];
                if (pick <= running) return candidateBuffer[i];
            }
            return candidateBuffer[n - 1];
        }

        /// <summary>Test seam (Assets/Editor/AdaptiveVerification.cs): drop a
        /// patrolling DC onto a chosen tissue tile. Not called by
        /// production -- same role as TissueGrid.SeedHostState.</summary>
        public void DebugPlaceForTest(FineCoord tissuePos)
        {
            Current = tissuePos;
            State = DendriticCellState.PatrolTissue;
            HasCargo = false;
            Frozen = false;
            patrolHeading = 1;
            tweenStart = tweenEnd = tissueBoard.FineToWorld(Current);
            transform.position = tweenStart;
        }

        private Vector3 WorldPos() => State == DendriticCellState.InNode
            ? node.NodeToWorld(NodePos)
            : tissueBoard.FineToWorld(Current);

        private void ApplyScale()
        {
            float s = State == DendriticCellState.InNode
                ? node.AgentWorldSize * 1.15f
                : BoardConfig.FineTileWorldSize * TissueFootprintFineTiles;
            transform.localScale = new Vector3(s, s, 1f);
        }

        private void Update()
        {
            if (tissueBoard == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: freeze mid-glide during the buy phase
            tweenTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tweenTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tweenStart, tweenEnd, t);
            if (tweenTimer >= BoardConfig.TickIntervalSeconds) tweenTimer -= BoardConfig.TickIntervalSeconds;
        }

        public void DespawnToPool()
        {
            var cb = onDespawn;
            onDespawn = null;
            if (State == DendriticCellState.InNode && node != null) node.Release(this);
            if (cb != null) cb(this);
            else { ResetForPool(); gameObject.SetActive(false); }
        }

        public void ResetForPool()
        {
            tissueBoard = null;
            tissueGrid = null;
            tissueCytokine = null;
            node = null;
            onDespawn = null;
            cohort = null;
            HasCargo = false;
            Frozen = false;
            presentationsLeft = 0;
            patrolHeading = 1;
            tweenTimer = 0f;
            if (sr != null) { sr.color = EmptyColor; sr.enabled = true; }
        }
    }
}
