using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;
using ImmunologyTD.Units;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Where a pathogen is in GAME_DESIGN.md section 1b's invasion loop.
    ///
    /// **Sprint 4 replaced the old `{ Transiting, Adhered, Cleared }`
    /// entirely.** The old `Transiting` was a rightward march across the
    /// whole board and the old `Adhered` meant "sitting in a tissue slot,
    /// attackable" -- the behaviour the Director rejected on 2026-08-21
    /// ("pathogens fly across the board skipping places"). The names below
    /// are deliberately different rather than reused, so no Sprint 1-3 call
    /// site can quietly keep compiling with the wrong meaning.
    /// </summary>
    public enum PathogenState
    {
        /// <summary>Riding the lumen flow. Safe, free, and irrelevant to the
        /// player -- cannot be attacked and does not touch tissue.</summary>
        Lumen,

        /// <summary>Colonising the gut interface at one boundary position
        /// (GutInterface). Off the flow, but NOT yet in the tissue and not
        /// yet attackable -- "accumulating," per section 1b step 1.</summary>
        AtInterface,

        /// <summary>Inside the tissue band, occupying a TissueGrid coarse
        /// slot, advancing toward the base, and attackable. This is the
        /// state Sprint 1-3 called `Adhered`.</summary>
        InTissue,

        /// <summary>Gone -- killed, excreted out the bottom of the lumen, or
        /// despawned on reaching the base. Awaiting pool return.</summary>
        Cleared,
    }

    /// <summary>
    /// Pathogen classes, per GAME_DESIGN.md section 4a (Director,
    /// 2026-08-19). Parasites (multi-coarse-slot footprint) remain out of
    /// scope. **All three move identically in Sprint 4** -- the
    /// class-specific advance behaviours of section 1b step 4 need Sprint
    /// 5's host-cell states and are explicitly deferred (SPRINT_PLAN.md item
    /// 10).
    /// </summary>
    public enum PathogenClass { IntracellularVirus, IntracellularBacterium, LargeBacterium }

    /// <summary>
    /// One pathogen, running GAME_DESIGN.md section 1b's four-step invasion
    /// loop:
    ///
    ///  1. **Enter the lumen at the upstream end and be carried downstream**
    ///     (<see cref="StepLumen"/>). Free, safe, unattackable. Carried off
    ///     the downstream end = excreted, no penalty -- the deliberate break
    ///     from Bloons TD 6 that handoff-map01-intestine.md section 1 keeps.
    ///  2. **Roll proximity-gated adhesion on every lumen step**
    ///     (InvasionTuning.AdhesionChanceAtWall / AdhesionFalloffCells). The
    ///     chance falls off exponentially with distance from the gut wall,
    ///     so the lane a pathogen rides genuinely decides its odds of ever
    ///     becoming the player's problem. On success it MOVES TO THE
    ///     BOUNDARY and joins the pile at that position.
    ///  3. **Wait on the wall until its position breaches** -- not its own
    ///     decision; GutInterface rolls per position and releases everyone
    ///     there at once (<see cref="EnterTissueAt"/>).
    ///  4. **Advance toward the base by a strongly biased random walk**
    ///     (<see cref="StepTissue"/>). Reaching the base despawns it and
    ///     increments InvasionTally.ReachedBase.
    ///
    /// ## The base-direction rule
    ///
    /// <see cref="StepTissue"/> is the one piece of movement code the
    /// Director called out personally (GAME_DESIGN.md section 1b step 3):
    /// advance is specified as **toward the base**, never as "leftward."
    /// Every step in this file is expressed through
    /// BoardConfig.OffsetInAxisFrame, where a delta of -1 on the axis always
    /// means "one cell toward the base," whatever direction that is in world
    /// space. **There is no `-1`, no `Column - 1`, and no negative-X
    /// assumption anywhere in this class**, and
    /// Assets/Editor/MapVerification.cs proves it by running the same code
    /// with the base configured on the opposite side.
    ///
    /// ## Explicit time
    ///
    /// <see cref="SimulationTick"/> and <see cref="TickCombat"/> take time
    /// as parameters and read no UnityEngine.Time, per this project's
    /// standing convention (TissueGrid, CytokineField, Chemotaxis,
    /// BoneMarrowManager.Tick, SearchUnit.SimulationTick). Update() only
    /// drives the visual tween and forwards the real clock.
    /// </summary>
    public class PathogenAgent : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private GutInterface gutInterface;
        private InvasionTally tally;
        private System.Action<PathogenAgent> onExit;
        private System.Func<CoarseCoord, float, bool> onSpreadRequested;

        public PathogenState State { get; private set; }

        /// <summary>Fine-lattice position. Pathogens do not walk the fine
        /// lattice -- this is the centre of whatever coarse cell they occupy,
        /// kept because SearchUnit.CheckContact measures Chebyshev fine-tile
        /// distance against it.</summary>
        public FineCoord Current { get; private set; }

        /// <summary>The coarse cell this pathogen occupies (lumen cell while
        /// flowing, tissue slot while invading). Undefined while
        /// AtInterface -- use <see cref="InterfacePosition"/> then.</summary>
        public CoarseCoord CurrentCoarse { get; private set; }

        /// <summary>Which gut-interface position (lane) this pathogen is
        /// colonising, or -1 when it is not AtInterface.</summary>
        public int InterfacePosition { get; private set; } = -1;

        public PathogenClass Class { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        /// <summary>
        /// True when this pathogen is INSIDE a host cell -- on TissueGrid's
        /// host layer (the cell is `Infected` and holds a reference to this
        /// agent), not on its occupant layer, and not rendered as itself
        /// (GAME_DESIGN.md section 4a).
        ///
        /// This is the state that makes the two layers worth having: an
        /// intracellular bacterium flips between here and the occupant layer
        /// repeatedly over its life, and while it is in here the position it
        /// sits at is simultaneously "an infected host cell" and "a free
        /// occupant slot something else can walk through."
        /// </summary>
        public bool IsIntracellular { get; private set; }

        private Vector3 moveStartWorld;
        private Vector3 moveEndWorld;
        private float moveElapsed;
        private float moveDuration = 1f;

        private float stepTimer;

        private SpriteRenderer sr;
        private float contactFlashTimer;
        private Color restColor;
        private static readonly Color FlashColor = new Color(0.95f, 0.85f, 0.3f);

        /// <summary>Reused every advance step so StepTissue allocates
        /// nothing (GAME_DESIGN.md section 8) -- same pattern as
        /// SearchUnit's candidateBuffer/weightBuffer.</summary>
        private readonly CoarseCoord[] advanceCandidates = new CoarseCoord[4];
        private readonly float[] advanceWeights = new float[4];
        private readonly bool[] advanceReachesBase = new bool[4];

        // -- Class assignment weights (judgment call, see docs/TEAM_RETRO.md) --
        // Weighted toward virus, unchanged from Sprint 2: viral spread is
        // still the mechanic most worth surfacing in a short playtest.
        // **Sprint 4 moved the assignment from adhesion time to SPAWN time**
        // -- a pathogen is a virus or a bacterium when it enters the lumen,
        // not when it happens to touch tissue.
        public const float VirusChance = 0.45f;
        public const float BacteriumChance = 0.25f; // remaining 0.30 is LargeBacterium

        // -- Combat numbers (judgment call, see docs/TEAM_RETRO.md) --
        // Unchanged from Sprint 2; SPRINT_PLAN.md item 10 forbids balance
        // tuning this sprint.
        public const float IntracellularMaxHealth = 12f;
        public const float LargeBacteriumMaxHealth = 18f;
        public const float ContactDamagePerHit = 1f;

        // -- Viral spread timing (judgment call, see docs/TEAM_RETRO.md) --
        public const float IncubationSeconds = 15f;
        public const float SpreadRetryIntervalSeconds = 1f;

        private bool hasSpread;

        /// <summary>When THIS infection began -- carried by the pathogen, not
        /// stored only in the slot, because a pathogen now moves between
        /// slots roughly once a second and the cytokine secretion ramp must
        /// not restart on every step. See TissueGrid.TryAdhere's comment.</summary>
        private float infectionStartTime;
        private float lastSpreadAttemptTime = float.NegativeInfinity;

        /// <summary>When this virus last became a FREE particle (occupant
        /// layer, no host). InvasionTuning.VirusFreeSurvivalSeconds after
        /// this, it dies. Half of the firebreak.</summary>
        private float freeSinceTime;

        /// <summary>When an intracellular bacterium entered its current host
        /// cell. It lyses out after
        /// InvasionTuning.IntracellularResidenceSeconds, killing the
        /// cell.</summary>
        private float hostEntryTime;

        /// <summary>Shuffle buffer for the five cell-to-cell spread
        /// candidates (own cell + four von Neumann neighbours), reused so a
        /// virus's per-step host search allocates nothing
        /// (GAME_DESIGN.md section 8).</summary>
        private readonly int[] hostSearchOrder = { 0, 1, 2, 3, 4 };

        /// <summary>The five candidates above as (deltaAxis, deltaCross)
        /// pairs in the AXIS FRAME. Index 0 is "stay here." The four steps
        /// are symmetric and equally weighted, which is what "spreads in all
        /// directions with no base bias" means -- there is deliberately no
        /// toward-base term anywhere in the viral path.</summary>
        private static readonly (int axis, int cross)[] SpreadOffsets =
        {
            (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1),
        };

        // ------------------------------------------------------------------
        // Initialization
        // ------------------------------------------------------------------

        /// <summary>
        /// Spawns a pathogen into the lumen at the upstream end, at a
        /// uniformly random position across the width of the channel. That
        /// uniform choice is what makes proximity-gated adhesion interesting
        /// -- most spawns are far from the wall and simply flow past, and
        /// which lane a pathogen happens to draw genuinely decides whether
        /// it ever becomes the player's problem.
        ///
        /// <paramref name="gutInterface"/> and <paramref name="tally"/> may
        /// be null for harness fixtures that only exercise combat; adhesion
        /// and the base counter then simply do nothing.
        /// </summary>
        public void Initialize(
            BoardConfig board, TissueGrid tissueGrid, GutInterface gutInterface, InvasionTally tally,
            System.Action<PathogenAgent> onExit, System.Func<CoarseCoord, float, bool> onSpreadRequested)
        {
            BindContext(board, tissueGrid, gutInterface, tally, onExit, onSpreadRequested);
            EnsureSprite();

            Class = PickRandomClass();
            SetHealthForClass();

            int axisIndex = Random.Range(board.LumenNearWallAxisIndex, board.AxisLength);
            CurrentCoarse = board.CoarseFromAxis(axisIndex, board.LumenEntryCrossIndex);
            Current = board.CoarseCenterFine(CurrentCoarse);

            State = PathogenState.Lumen;
            InterfacePosition = -1;
            hasSpread = false;
            contactFlashTimer = 0f;
            lastSpreadAttemptTime = float.NegativeInfinity;

            restColor = BoardRenderer.PathogenColor; // visible as itself in the channel, whatever its class
            sr.color = restColor;
            sr.enabled = true;

            SnapTo(board.CoarseToWorldCenter(CurrentCoarse));
            // Desync step clocks so a batch of spawns doesn't move in
            // lockstep down the channel.
            stepTimer = Random.Range(0f, InvasionTuning.LumenStepIntervalSeconds);
        }

        /// <summary>
        /// Places a pathogen directly into tissue at a known coarse slot,
        /// bypassing the lumen and the wall entirely. Two production
        /// callers: PathogenSpawner.RequestSpread (a viral spread event
        /// creates a new infection instantly) and GutInterface.Breach via
        /// <see cref="EnterTissueAt"/>'s sibling path for a fresh agent.
        /// Renamed from Sprint 2's `InitializeAdheredDirect` because
        /// "adhered" now means the gut wall, not tissue.
        /// </summary>
        public void InitializeInTissueDirect(
            BoardConfig board, TissueGrid tissueGrid, GutInterface gutInterface, InvasionTally tally,
            System.Action<PathogenAgent> onExit, System.Func<CoarseCoord, float, bool> onSpreadRequested,
            CoarseCoord slot, PathogenClass pClass, float currentTime)
        {
            BindContext(board, tissueGrid, gutInterface, tally, onExit, onSpreadRequested);
            EnsureSprite();

            Class = pClass;
            SetHealthForClass();
            InterfacePosition = -1;
            hasSpread = false;
            contactFlashTimer = 0f;
            lastSpreadAttemptTime = float.NegativeInfinity;

            infectionStartTime = currentTime;
            CurrentCoarse = slot;
            Current = board.CoarseCenterFine(slot);
            State = PathogenState.InTissue;
            SettleIntoTissue(slot, currentTime);
            SnapTo(board.CoarseToWorldCenter(slot));
            stepTimer = Random.Range(0f, InvasionTuning.TissueStepIntervalSeconds);
        }

        /// <summary>
        /// Decides which of the two layers this pathogen lands on when it
        /// arrives in tissue -- the first place Sprint 5's class split shows
        /// up (GAME_DESIGN.md section 1b step 4).
        ///
        ///  - A **virus** takes the host cell if it is `Healthy`, becoming
        ///    intracellular immediately. If there is no healthy cell here it
        ///    lands as a free particle on the occupant layer and starts
        ///    dying.
        ///  - **Both bacteria** land on the occupant layer, visible and
        ///    walking. An intracellular bacterium may go inside a cell later,
        ///    on one of its own steps.
        /// </summary>
        /// <summary>Only a virus can enter a host cell on arrival, and only
        /// if there is a healthy one here. Used by
        /// <see cref="EnterTissueAt"/> to decide whether a pathogen whose
        /// target position already has an extracellular occupant can still
        /// come off the wall.</summary>
        private bool CanTakeHostAt(CoarseCoord slot) =>
            Class == PathogenClass.IntracellularVirus && tissueGrid.IsHealthyHost(slot);

        private void SettleIntoTissue(CoarseCoord slot, float currentTime)
        {
            if (Class == PathogenClass.IntracellularVirus && tissueGrid.TryInfect(slot, this, currentTime))
            {
                IsIntracellular = true;
                hostEntryTime = currentTime;
            }
            else
            {
                IsIntracellular = false;
                freeSinceTime = currentTime;
                tissueGrid.TryClaimOccupant(slot, this, infectionStartTime);
            }
            ApplyRestColorForCurrentClass();
        }

        private void BindContext(
            BoardConfig board, TissueGrid tissueGrid, GutInterface gutInterface, InvasionTally tally,
            System.Action<PathogenAgent> onExit, System.Func<CoarseCoord, float, bool> onSpreadRequested)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.gutInterface = gutInterface;
            this.tally = tally;
            this.onExit = onExit;
            this.onSpreadRequested = onSpreadRequested;
        }

        private void EnsureSprite()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.sortingOrder = 20;
            float worldSize = BoardConfig.FineTileWorldSize * 3.5f;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);
        }

        // ------------------------------------------------------------------
        // Simulation
        // ------------------------------------------------------------------

        private void Update()
        {
            if (board == null) return;

            SimulationTick(Time.deltaTime, Time.time);

            if (moveDuration > 0f && moveElapsed < moveDuration)
            {
                moveElapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(moveStartWorld, moveEndWorld, Mathf.Clamp01(moveElapsed / moveDuration));
            }
        }

        /// <summary>
        /// One slice of simulated time for this pathogen: lumen flow +
        /// adhesion rolls, or a tissue advance step, plus the viral-spread
        /// check. Reads no UnityEngine.Time -- Update() forwards the real
        /// clock, and Assets/Editor/MapVerification.cs forwards a fake one.
        ///
        /// A `while` rather than an `if` on the step clock so a harness can
        /// legitimately advance in coarse slices (e.g. one call per
        /// simulated second) without silently dropping steps.
        /// </summary>
        public void SimulationTick(float deltaTime, float currentTime)
        {
            if (board == null || State == PathogenState.Cleared) return;

            if (State == PathogenState.Lumen)
            {
                float interval = Mathf.Max(0.0001f, InvasionTuning.LumenStepIntervalSeconds);
                stepTimer += deltaTime;
                while (stepTimer >= interval && State == PathogenState.Lumen)
                {
                    stepTimer -= interval;
                    StepLumen(currentTime);
                }
                return;
            }

            if (State == PathogenState.InTissue)
            {
                float interval = Mathf.Max(0.0001f, InvasionTuning.TissueStepIntervalSeconds);
                stepTimer += deltaTime;
                while (stepTimer >= interval && State == PathogenState.InTissue)
                {
                    stepTimer -= interval;
                    StepInTissue(currentTime);
                }
                TickCombat(currentTime);
            }

            // AtInterface: nothing to do. A colonising pathogen sits still
            // and waits for GutInterface to roll its position's breach --
            // deliberately NOT its own decision (SPRINT_PLAN.md item 6).
        }

        /// <summary>
        /// One step down the channel, then one proximity-gated adhesion
        /// roll (GAME_DESIGN.md section 1b step 1). Carried off the
        /// downstream end = excreted, no penalty.
        ///
        /// Order matters: the pathogen moves first and then rolls, so the
        /// entry cell gets exactly one roll like every other cell rather
        /// than two.
        /// </summary>
        private void StepLumen(float currentTime)
        {
            int nextCross = board.CrossIndex(CurrentCoarse) + board.FlowCrossStep;
            if (board.IsExcretedCrossIndex(nextCross))
            {
                if (tally != null) tally.Excreted++;
                Exit();
                return;
            }

            CurrentCoarse = board.CoarseFromAxis(board.AxisIndex(CurrentCoarse), nextCross);
            Current = board.CoarseCenterFine(CurrentCoarse);
            MoveTo(board.CoarseToWorldCenter(CurrentCoarse), InvasionTuning.LumenStepIntervalSeconds);

            if (gutInterface == null) return;
            int depth = board.LumenDepthFromInterface(CurrentCoarse);
            if (depth < 0) return; // not actually in the channel; nothing to adhere to
            if (Random.value >= AdhesionChanceAt(depth)) return;

            AdhereToInterface(board.CrossIndex(CurrentCoarse));
        }

        /// <summary>Per-step adhesion probability as a function of distance
        /// from the gut wall, in coarse cells. Exponential falloff -- see
        /// InvasionTuning for the shape and why it was chosen.</summary>
        public static float AdhesionChanceAt(int lumenDepthFromInterface)
        {
            if (lumenDepthFromInterface < 0) return 0f;
            float falloff = Mathf.Max(0.0001f, InvasionTuning.AdhesionFalloffCells);
            return Mathf.Clamp01(InvasionTuning.AdhesionChanceAtWall) *
                   Mathf.Exp(-lumenDepthFromInterface / falloff);
        }

        /// <summary>
        /// Leaves the flow and colonises the gut interface at
        /// <paramref name="position"/>. Per section 1b step 1 the pathogen
        /// **moves to the boundary** -- it does not adhere where it was
        /// floating -- so its rendered position snaps to the wall
        /// immediately. Public so a harness can put a pathogen on the wall
        /// deterministically without waiting on a roll.
        /// </summary>
        public bool AdhereToInterface(int position)
        {
            if (gutInterface == null || State == PathogenState.Cleared) return false;
            if (!board.InCrossBounds(position)) return false;

            int stackIndex = gutInterface.AdheredCountAt(position);
            if (!gutInterface.Adhere(position, this)) return false;

            State = PathogenState.AtInterface;
            InterfacePosition = position;
            CurrentCoarse = board.CoarseFromAxis(board.LumenNearWallAxisIndex, position);
            Current = board.CoarseCenterFine(CurrentCoarse);
            restColor = BoardRenderer.PathogenColor;
            if (sr != null) { sr.color = restColor; sr.enabled = true; }
            SnapTo(InterfaceStackWorldPosition(position, stackIndex));
            return true;
        }

        /// <summary>
        /// Where the Nth pathogen in a boundary pile is drawn: at the seam,
        /// pushed back OUT INTO THE CHANNEL by its depth in the pile, so a
        /// dangerous position literally reads as a queue of pathogens
        /// pressing against the wall (SPRINT_PLAN.md item 6: "make an
        /// accumulating boundary position visibly distinct from an empty
        /// one"). Direction is derived from the two cells either side of the
        /// seam, so it follows the map's orientation rather than assuming
        /// the lumen is on the right.
        /// </summary>
        private Vector3 InterfaceStackWorldPosition(int position, int stackIndex)
        {
            var seam = board.InterfaceWorldCenter(position);
            var tissueSide = board.CoarseToWorldCenter(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, position));
            var outward = (seam - tissueSide);
            if (outward.sqrMagnitude > 0.0001f) outward.Normalize();
            const int VisualStackCap = 7; // beyond this the pile stops growing outward and just overlaps
            float step = board.CoarseCellWorldSize * 0.55f;
            return seam + outward * (Mathf.Min(stackIndex, VisualStackCap) * step);
        }

        /// <summary>
        /// Enters the tissue at <paramref name="slot"/>. Called by
        /// GutInterface.Breach for every pathogen at a breaching position,
        /// in one pass, so a burst genuinely lands in a single tick.
        ///
        /// Deliberately does NOT remove itself from the interface pile --
        /// GutInterface owns pile membership and compacts it around the
        /// pathogens that did move, which is what keeps "released
        /// everything at once" a single, auditable operation.
        /// </summary>
        public bool EnterTissueAt(CoarseCoord slot, float currentTime)
        {
            if (board == null || State == PathogenState.Cleared) return false;
            if (board.BandOf(slot) != BoardBand.Tissue) return false;

            // A virus can take the host cell here even when the occupant
            // layer is busy, so the free-slot test is no longer the whole
            // admission check -- SettleIntoTissue decides which layer this
            // pathogen lands on, and only a pathogen that gets NEITHER stays
            // on the wall.
            if (!tissueGrid.IsOccupantFree(slot) && !CanTakeHostAt(slot)) return false;

            infectionStartTime = currentTime;
            State = PathogenState.InTissue;
            InterfacePosition = -1;
            CurrentCoarse = slot;
            Current = board.CoarseCenterFine(slot);
            hasSpread = false;
            lastSpreadAttemptTime = float.NegativeInfinity;
            SettleIntoTissue(slot, currentTime);
            MoveTo(board.CoarseToWorldCenter(slot), InvasionTuning.TissueStepIntervalSeconds * 0.5f);
            stepTimer = Random.Range(0f, InvasionTuning.TissueStepIntervalSeconds);
            return true;
        }

        /// <summary>
        /// One step of the strongly biased random walk toward the base
        /// (GAME_DESIGN.md section 1b step 3).
        ///
        /// The four candidates are built in the axis frame: one step toward
        /// the base, one step away, and the two sideways steps along the
        /// lanes. Weights come from InvasionTuning. A candidate is dropped
        /// if it leaves the board, leaves the tissue band on the lumen side
        /// (a pathogen inside the body does not climb back into the
        /// channel), or is already occupied -- so a blocked front slides
        /// sideways instead of jamming. If nothing is legal the pathogen
        /// holds its ground, which is exactly what a contested line should
        /// look like.
        ///
        /// The one special case: a base-ward step whose destination is in
        /// the BASE band is not a move at all, it is the endzone
        /// (<see cref="ReachBase"/>).
        /// </summary>
        // ------------------------------------------------------------------
        // Class-specific advance (GAME_DESIGN.md section 1b step 4)
        //
        // Deferred out of Sprint 4 because it needs host states; Sprint 5's
        // two-layer TissueGrid is what made it buildable. All three classes
        // moved identically before this.
        // ------------------------------------------------------------------

        /// <summary>
        /// One tissue step, dispatched by class. The three genuinely
        /// different means of reaching the base are what should make a front
        /// look different depending on what is attacking it.
        /// </summary>
        private void StepInTissue(float currentTime)
        {
            switch (Class)
            {
                case PathogenClass.IntracellularVirus:
                    StepVirus(currentTime);
                    break;
                case PathogenClass.IntracellularBacterium:
                    StepIntracellularBacterium(currentTime);
                    break;
                default:
                    StepMotile(currentTime);
                    break;
            }
        }

        /// <summary>
        /// **Viruses: diffusive, self-limiting, no base bias whatsoever.**
        ///
        /// An intracellular virus does not move at all -- it is inside a
        /// cell, and its only way forward is <see cref="TickCombat"/>'s
        /// cell-to-cell spread. A FREE virus (occupant layer, between hosts)
        /// looks for a `Healthy` host in its own cell and its four
        /// neighbours, in shuffled order, and dies if it has been homeless
        /// for InvasionTuning.VirusFreeSurvivalSeconds.
        ///
        /// **The firebreak is emergent and there is no firebreak check.**
        /// A free virus may only ever move ONTO a healthy host cell -- it
        /// cannot traverse dead, empty, or already-infected ground even for
        /// one step -- so a viral front that has killed the tissue behind it
        /// physically cannot cross that ground, and the survival timer
        /// finishes off whatever is stranded against it. Both halves are
        /// plain local rules; the firebreak is what they add up to.
        /// </summary>
        private void StepVirus(float currentTime)
        {
            if (IsIntracellular) return; // hidden inside a cell; spread is TickCombat's job

            if (TryFindAndEnterHost(currentTime)) return;

            if (currentTime - freeSinceTime >= InvasionTuning.VirusFreeSurvivalSeconds)
            {
                // Died without finding a host. Not a kill -- nobody is
                // credited, and it leaves no debris because no host cell
                // died with it.
                State = PathogenState.Cleared;
                tissueGrid.ReleaseOccupant(CurrentCoarse);
                onExit?.Invoke(this);
            }
        }

        /// <summary>
        /// **Intracellular bacteria: biased when out, hidden when in.**
        ///
        /// Outside a cell it is an ordinary base-biased walker, visible as
        /// itself, and each step it may slip inside a healthy host cell it is
        /// standing on (InvasionTuning.IntracellularEntryChance). Inside, it
        /// is invisible and stationary until it lyses out
        /// (InvasionTuning.IntracellularResidenceSeconds), which KILLS the
        /// host cell and leaves debris.
        ///
        /// The lysis exit is this sprint's judgment call -- see
        /// InvasionTuning.IntracellularResidenceSeconds for why an exit had
        /// to exist at all.
        /// </summary>
        private void StepIntracellularBacterium(float currentTime)
        {
            if (IsIntracellular)
            {
                if (currentTime - hostEntryTime < InvasionTuning.IntracellularResidenceSeconds) return;
                // Only lyse out if there is somewhere to stand. If something
                // else is occupying this position, wait -- better a longer
                // residence than a pathogen owned by neither layer.
                if (!tissueGrid.IsOccupantFree(CurrentCoarse)) return;

                // Detach from the host layer FIRST. KillHostCell notifies its
                // resident via OnHostCellDestroyed -- and the resident here is
                // this bacterium. Without the detach, lysing out would kill
                // the bacterium along with the cell, when the whole point of
                // lysis is that it survives and keeps walking (section 1b
                // step 4). ReleaseIntracellular flips Infected -> Healthy and
                // drops the resident link; KillHostCell then takes that
                // Healthy cell to Dead + debris with no one to notify.
                tissueGrid.ReleaseIntracellular(CurrentCoarse);
                tissueGrid.KillHostCell(CurrentCoarse);
                IsIntracellular = false;
                freeSinceTime = currentTime;
                tissueGrid.TryClaimOccupant(CurrentCoarse, this, infectionStartTime);
                ApplyRestColorForCurrentClass();
                return;
            }

            if (tissueGrid.IsHealthyHost(CurrentCoarse) &&
                Random.value < InvasionTuning.IntracellularEntryChance &&
                tissueGrid.TryInfect(CurrentCoarse, this, currentTime))
            {
                tissueGrid.ReleaseOccupant(CurrentCoarse);
                IsIntracellular = true;
                hostEntryTime = currentTime;
                infectionStartTime = currentTime;
                ApplyRestColorForCurrentClass();
                return;
            }

            StepMotile(currentTime);
        }

        /// <summary>
        /// Looks for a `Healthy` host cell at this virus's own position and
        /// its four neighbours, in shuffled order, and moves into the first
        /// one found. Returns false if there is no healthy cell in reach --
        /// which, on ground the infection has already killed, is every time.
        ///
        /// Neighbour offsets are expressed in the AXIS FRAME like everything
        /// else in this class, but symmetrically: there is no toward-base
        /// weighting, per section 1b step 4's "in all directions with no
        /// base bias."
        /// </summary>
        private bool TryFindAndEnterHost(float currentTime)
        {
            for (int i = SpreadOffsets.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = hostSearchOrder[i];
                hostSearchOrder[i] = hostSearchOrder[j];
                hostSearchOrder[j] = tmp;
            }

            for (int i = 0; i < hostSearchOrder.Length; i++)
            {
                var (dAxis, dCross) = SpreadOffsets[hostSearchOrder[i]];
                int axisIndex = board.AxisIndex(CurrentCoarse) + dAxis;
                int crossIndex = board.CrossIndex(CurrentCoarse) + dCross;
                if (!board.InAxisBounds(axisIndex) || !board.InCrossBounds(crossIndex)) continue;
                if (board.BandAtAxisIndex(axisIndex) != BoardBand.Tissue) continue;

                var candidate = board.CoarseFromAxis(axisIndex, crossIndex);
                if (!tissueGrid.IsHealthyHost(candidate)) continue;
                if (!tissueGrid.TryInfect(candidate, this, currentTime)) continue;

                tissueGrid.ReleaseOccupant(CurrentCoarse);
                IsIntracellular = true;
                hostEntryTime = currentTime;
                infectionStartTime = currentTime;
                hasSpread = false;
                lastSpreadAttemptTime = float.NegativeInfinity;
                CurrentCoarse = candidate;
                Current = board.CoarseCenterFine(candidate);
                ApplyRestColorForCurrentClass();
                MoveTo(board.CoarseToWorldCenter(candidate), InvasionTuning.TissueStepIntervalSeconds);
                return true;
            }
            return false;
        }

        private void StepMotile(float currentTime)
        {
            // A large bacterium grazes the ground it stands on
            // (GAME_DESIGN.md section 4a: it "kills and directly occupies").
            // Reconciled with section 1c's two-layer model as damage over
            // time rather than instant replacement -- see
            // InvasionTuning.LargeBacteriumHostDamagePerStep.
            if (Class == PathogenClass.LargeBacterium)
            {
                tissueGrid.DamageHostCell(CurrentCoarse, InvasionTuning.LargeBacteriumHostDamagePerStep);
            }

            StepTissue();
        }

        private void StepTissue()
        {
            int count = 0;
            float total = 0f;

            void Consider(int deltaAxis, int deltaCross, float weight)
            {
                if (weight <= 0f) return;
                int axisIndex = board.AxisIndex(CurrentCoarse) + deltaAxis;
                int crossIndex = board.CrossIndex(CurrentCoarse) + deltaCross;
                if (!board.InAxisBounds(axisIndex) || !board.InCrossBounds(crossIndex)) return;

                var band = board.BandAtAxisIndex(axisIndex);
                bool reachesBase = band == BoardBand.Base;
                if (band == BoardBand.Lumen) return;

                var candidate = board.CoarseFromAxis(axisIndex, crossIndex);
                if (!reachesBase && !tissueGrid.IsOccupantFree(candidate)) return;

                advanceCandidates[count] = candidate;
                advanceWeights[count] = weight;
                advanceReachesBase[count] = reachesBase;
                total += weight;
                count++;
            }

            // -1 on the AXIS is "toward the base," always. Never a column
            // decrement -- see the class comment.
            Consider(-1, 0, InvasionTuning.AdvanceBaseWeight);
            Consider(0, -1, InvasionTuning.AdvanceLateralWeight);
            Consider(0, +1, InvasionTuning.AdvanceLateralWeight);
            Consider(+1, 0, InvasionTuning.AdvanceAwayWeight);

            if (count == 0 || total <= 0f) return; // held -- no legal step

            float pick = Random.value * total;
            int chosen = count - 1;
            float running = 0f;
            for (int i = 0; i < count; i++)
            {
                running += advanceWeights[i];
                if (pick <= running) { chosen = i; break; }
            }

            if (advanceReachesBase[chosen])
            {
                ReachBase();
                return;
            }

            tissueGrid.ReleaseOccupant(CurrentCoarse);
            if (!tissueGrid.TryClaimOccupant(advanceCandidates[chosen], this, infectionStartTime))
            {
                // Should be unreachable (freshness was checked above and this
                // is single-threaded), but never leave a live pathogen
                // unowned: take the old slot back.
                tissueGrid.TryClaimOccupant(CurrentCoarse, this, infectionStartTime);
                return;
            }

            CurrentCoarse = advanceCandidates[chosen];
            Current = board.CoarseCenterFine(CurrentCoarse);
            MoveTo(board.CoarseToWorldCenter(CurrentCoarse), InvasionTuning.TissueStepIntervalSeconds);
        }

        /// <summary>
        /// SPRINT_PLAN.md item 9: a pathogen that reaches the base band
        /// despawns and increments a counter, and that is deliberately ALL
        /// it does. The 100-life pool and the real lose condition are Sprint
        /// 5 (GAME_DESIGN.md section 6c).
        /// </summary>
        private void ReachBase()
        {
            if (tally != null) tally.ReachedBase++;
            tissueGrid.ReleaseOccupant(CurrentCoarse);
            State = PathogenState.Cleared;
            onExit?.Invoke(this);
        }

        private static PathogenClass PickRandomClass()
        {
            float r = Random.value;
            if (r < VirusChance) return PathogenClass.IntracellularVirus;
            if (r < VirusChance + BacteriumChance) return PathogenClass.IntracellularBacterium;
            return PathogenClass.LargeBacterium;
        }

        private void SetHealthForClass()
        {
            MaxHealth = Class == PathogenClass.LargeBacterium ? LargeBacteriumMaxHealth : IntracellularMaxHealth;
            Health = MaxHealth;
        }

        /// <summary>GAME_DESIGN.md section 4a: intracellular pathogens are
        /// "visible as the host cell, not itself"; large bacteria are
        /// "visible as itself, no disguise." Applies only once a pathogen is
        /// IN TISSUE -- in the lumen and on the wall there is no host cell to
        /// hide inside, so every class is drawn as itself there. See
        /// docs/ENGINE_STATUS.md for why intracellular classes disable the
        /// sprite outright rather than tinting it to HostColor.
        ///
        /// **Sprint 5 made this a question about the LAYER, not the class.**
        /// Sprint 2-4 hid both intracellular classes permanently, because
        /// there was no such thing as being outside a cell. There is now: an
        /// intracellular bacterium walking between hosts and a free virus
        /// particle are both extracellular, both on the occupant layer, and
        /// both visible as themselves -- which is exactly what section 1b
        /// step 4's "biased when out, hidden when in" asks for. Hiding is
        /// now driven by <see cref="IsIntracellular"/>.</summary>
        private void ApplyRestColorForCurrentClass()
        {
            restColor = IsIntracellular ? BoardRenderer.InfectedHostColor : BoardRenderer.PathogenColor;
            if (sr == null) return;
            sr.color = restColor;
            sr.enabled = !IsIntracellular;
        }

        /// <summary>
        /// The viral spread check (GAME_DESIGN.md section 4a). No-ops unless
        /// this is an uncleared virus infection sitting in tissue past its
        /// incubation. Called from SimulationTick, and callable directly
        /// with simulated time by a harness.
        /// </summary>
        public void TickCombat(float currentTime)
        {
            if (State != PathogenState.InTissue) return;
            if (Class != PathogenClass.IntracellularVirus) return;
            if (hasSpread) return;
            if (currentTime - infectionStartTime < IncubationSeconds) return;
            if (currentTime - lastSpreadAttemptTime < SpreadRetryIntervalSeconds) return;

            lastSpreadAttemptTime = currentTime;
            if (onSpreadRequested != null && onSpreadRequested(CurrentCoarse, currentTime))
            {
                hasSpread = true;
            }
        }

        /// <summary>
        /// Flat per-contact damage, called by SearchUnit.CheckContact.
        ///
        /// **Sprint 4 narrowed the guard from `Adhered` to
        /// <see cref="PathogenState.InTissue"/>.** A pathogen in the lumen
        /// or colonising the wall cannot be attacked -- GAME_DESIGN.md
        /// section 1a states that outright for the lumen, and the wall is
        /// the epithelial barrier, outside the tissue the immune cells walk.
        /// See docs/TEAM_RETRO.md: making the wall attackable would need
        /// multi-occupancy contact detection, which is Sprint 5's TissueGrid
        /// rewrite, and the mechanic this sprint is proving is the burst,
        /// not the siege.
        ///
        /// Kill attribution is unchanged from Sprint 3: exactly one unit is
        /// ever credited, whoever's hit crosses zero, credited before
        /// clearing (which can pool this instance). A null source stays
        /// legal and always will.
        /// </summary>
        public void ReceiveDamage(float amount, SearchUnit source)
        {
            if (State != PathogenState.InTissue) return;
            contactFlashTimer = 0.25f;
            Health -= amount;
            if (Health > 0f) return;

            if (source != null) source.RegisterKill();
            ClearFromCombat();
        }

        private void ClearFromCombat()
        {
            // State goes first, deliberately: KillHostCell notifies its
            // resident via OnHostCellDestroyed, and that method's
            // already-Cleared guard is what stops this from exiting twice
            // and double-releasing into the pool.
            State = PathogenState.Cleared;

            if (IsIntracellular)
            {
                // Innate clearing of an intracellular infection is
                // destructive -- the only way to reach the pathogen is
                // through the cell, so the cell dies and leaves debris
                // (GAME_DESIGN.md sections 1c and 4a). The non-destructive
                // path (ReleaseIntracellular, cell survives) belongs to
                // adaptive immunity's precise MHC-I killing and is not built.
                IsIntracellular = false;
                tissueGrid.KillHostCell(CurrentCoarse);
            }
            else
            {
                tissueGrid.ReleaseOccupant(CurrentCoarse);
            }

            onExit?.Invoke(this);
        }

        /// <summary>
        /// The host cell this pathogen was living inside has just been
        /// destroyed, so this pathogen dies with it -- an intracellular
        /// infection cannot outlive its host (GAME_DESIGN.md section 4a:
        /// clearing an intracellular infection is destructive by nature,
        /// because the only way to reach it is through the cell).
        ///
        /// Called by TissueGrid.KillHostCell, which has ALREADY detached
        /// this agent from the host layer before calling -- deliberately, so
        /// that nothing here calls back into the grid and finds a
        /// half-updated cell. That is why this does not release anything: it
        /// only tears down its own state and returns to the pool.
        /// </summary>
        public void OnHostCellDestroyed()
        {
            if (State == PathogenState.Cleared) return;
            State = PathogenState.Cleared;
            IsIntracellular = false;
            onExit?.Invoke(this);
        }

        private void Exit()
        {
            State = PathogenState.Cleared;
            onExit?.Invoke(this);
        }

        // ------------------------------------------------------------------
        // Visual helpers
        // ------------------------------------------------------------------

        private void SnapTo(Vector3 world)
        {
            moveStartWorld = moveEndWorld = world;
            moveElapsed = moveDuration = 0f;
            transform.position = world;
        }

        private void MoveTo(Vector3 world, float durationSeconds)
        {
            moveStartWorld = transform.position;
            moveEndWorld = world;
            moveElapsed = 0f;
            moveDuration = Mathf.Max(0.0001f, durationSeconds);
        }

        private void LateUpdate()
        {
            if (sr == null) return;
            if (contactFlashTimer > 0f)
            {
                contactFlashTimer -= Time.deltaTime;
                sr.color = Color.Lerp(restColor, FlashColor, contactFlashTimer / 0.25f);
            }
            else if (sr.color != restColor)
            {
                sr.color = restColor;
            }
        }

        /// <summary>Called by PathogenSpawner just before returning this
        /// instance to the pool. Every exit path funnels through onExit, so
        /// this is the single place stale state is cleared.</summary>
        public void ResetForPool()
        {
            State = PathogenState.Cleared;
            board = null;
            tissueGrid = null;
            gutInterface = null;
            tally = null;
            onSpreadRequested = null;
            onExit = null;
            InterfacePosition = -1;
            contactFlashTimer = 0f;
            hasSpread = false;
            lastSpreadAttemptTime = float.NegativeInfinity;
            stepTimer = 0f;
            moveElapsed = moveDuration = 0f;
            Health = 0f;
            MaxHealth = 0f;
        }
    }
}
