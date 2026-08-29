using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Adaptive
{
    public enum DendriticCellState { PatrolTissue, TravelToNode, InNode, ReturnToTissue }

    /// <summary>
    /// The dendritic-cell shuttle of GAME_DESIGN.md §5a. A DC emitted by its
    /// progenitor:
    ///
    ///  1. **Patrols tissue** (plain random walk -- debris homing is a
    ///     deferred BACKLOG item). Standing on a `Dead` cell whose debris
    ///     carries an antigen, it **samples**: picks up that antigen as
    ///     cargo and eats one bite of the pile (competing with macrophage
    ///     efferocytosis, §1c).
    ///  2. **Travels to the node** -- an axis-frame biased walk toward the
    ///     base (never a hardcoded direction), then enters the lymph node.
    ///  3. **In the node** it wanders the co-localisation gradient among the
    ///     helper-T cells (<see cref="LymphNode"/> runs the pairing). Each
    ///     pairing spends one presentation whether or not the barcodes
    ///     matched.
    ///  4. When the cargo is spent it **returns to tissue** empty (§5a's
    ///     "the DC eventually loses its cargo and must return" -- it does not
    ///     die; the travel time is the cost) and patrols again.
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

        private static readonly Color EmptyColor = new Color(0.72f, 0.30f, 0.68f);  // dendritic magenta
        private static readonly Color CargoColor = new Color(0.98f, 0.62f, 0.98f);  // brighter -- carrying antigen

        private Vector3 tweenStart;
        private Vector3 tweenEnd;
        private float tweenTimer;

        public void Initialize(
            BoardConfig tissueBoard, TissueGrid tissueGrid, CytokineField tissueCytokine,
            LymphNode node, FineCoord tissueStart, System.Action<DendriticCell> onDespawn)
        {
            this.tissueBoard = tissueBoard;
            this.tissueGrid = tissueGrid;
            this.tissueCytokine = tissueCytokine;
            this.node = node;
            this.onDespawn = onDespawn;

            State = DendriticCellState.PatrolTissue;
            Current = tissueStart;
            HasCargo = false;
            Frozen = false;
            presentationsLeft = 0;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
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
                case DendriticCellState.TravelToNode: TickTravel(); break;
                case DendriticCellState.InNode: TickInNode(); break;
                case DendriticCellState.ReturnToTissue: TickReturn(); break;
            }

            tweenEnd = WorldPos();
            if (sr != null) sr.color = HasCargo ? CargoColor : EmptyColor;
        }

        private void TickPatrol(float currentTime)
        {
            for (int i = 0; i < AdaptiveTuning.DcFineTilesPerTick; i++)
            {
                Current = Chemotaxis.ChooseNextStep(
                    Current, tissueBoard, tissueCytokine, false, candidateBuffer, weightBuffer);
            }

            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            if (tissueGrid.GetHostState(coarse) != HostState.Dead) return;
            var antigen = tissueGrid.GetDebrisAntigen(coarse);
            if (!antigen.HasValue) return;

            // Sample: pick up the antigen and eat a bite of the pile (§1c --
            // a sampling DC is also a clearing DC, so it competes with
            // macrophage efferocytosis for the same debris).
            Cargo = Antigen.ForClass(antigen.Value);
            CargoClass = antigen.Value;
            HasCargo = true;
            presentationsLeft = AdaptiveTuning.DcPresentationsPerCargo;
            tissueGrid.ClearDebris(coarse, AdaptiveTuning.DcDebrisSamplePerBite, currentTime);
            State = DendriticCellState.TravelToNode;
        }

        private void TickTravel()
        {
            for (int i = 0; i < AdaptiveTuning.DcFineTilesPerTick; i++)
                Current = BiasedAxisStep(Current, -1); // -1 = toward the base

            if (tissueBoard.BandOf(Current.ToCoarse(BoardConfig.FineSubdivision)) == BoardBand.Base)
                EnterNode();
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

        private void TickReturn()
        {
            for (int i = 0; i < AdaptiveTuning.DcFineTilesPerTick; i++)
                Current = BiasedAxisStep(Current, +1); // +1 = away from the base, into tissue

            if (tissueBoard.BandOf(Current.ToCoarse(BoardConfig.FineSubdivision)) == BoardBand.Tissue)
                State = DendriticCellState.PatrolTissue;
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
            State = DendriticCellState.ReturnToTissue;
            ApplyScale();
        }

        /// <summary>Von Neumann fine step, softmax-biased along the axis
        /// frame. <paramref name="dir"/> -1 = toward the base, +1 = away.
        /// Uses the axis frame, never a world direction -- same architectural
        /// rule as PathogenAgent.StepTissue.</summary>
        private FineCoord BiasedAxisStep(FineCoord from, int dir)
        {
            int n = 0;
            float best = float.NegativeInfinity;
            foreach (var off in FineCoord.VonNeumannOffsets)
            {
                var cand = from.Add(off);
                if (!tissueBoard.InFineBounds(cand)) continue;
                float score = dir * tissueBoard.AxisIndex(cand.ToCoarse(BoardConfig.FineSubdivision));
                candidateBuffer[n] = cand;
                weightBuffer[n] = score;
                if (score > best) best = score;
                n++;
            }
            if (n == 0) return from;

            float total = 0f;
            for (int i = 0; i < n; i++)
            {
                weightBuffer[i] = Mathf.Exp(AdaptiveTuning.DcAxisWalkBiasSharpness * (weightBuffer[i] - best));
                total += weightBuffer[i];
            }
            float pick = Random.value * total;
            float running = 0f;
            for (int i = 0; i < n; i++)
            {
                running += weightBuffer[i];
                if (pick <= running) return candidateBuffer[i];
            }
            return candidateBuffer[n - 1];
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
            HasCargo = false;
            Frozen = false;
            presentationsLeft = 0;
            tweenTimer = 0f;
            if (sr != null) { sr.color = EmptyColor; sr.enabled = true; }
        }
    }
}
