using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;
using ImmunologyTD.Units;

namespace ImmunologyTD.Pathogens
{
    public enum PathogenState { Transiting, Adhered, Cleared }

    /// <summary>
    /// Pathogen classes, per GAME_DESIGN.md section 4a (Director, 2026-08-19).
    /// Only three of the design doc's four classes exist this sprint --
    /// parasites (multi-coarse-slot footprint) are explicitly deferred, see
    /// SPRINT_PLAN.md.
    /// </summary>
    public enum PathogenClass { IntracellularVirus, IntracellularBacterium, LargeBacterium }

    /// <summary>
    /// A pathogen that enters at the lumen edge, transits rightward for a
    /// random distance, then either adheres to a coarse slot near where it
    /// stopped or transits straight across and exits (unchanged from
    /// Sprint 1 -- see class comment history in docs/ENGINE_STATUS.md).
    ///
    /// Sprint 2 addition: on adhesion, a pathogen is assigned one of three
    /// classes (GAME_DESIGN.md section 4a). Intracellular pathogens (virus,
    /// bacterium) infect the slot's host cell without replacing it -- the
    /// slot keeps reading as host tissue (see restColor/HideAsHostCell
    /// below); a large bacterium kills and occupies the slot outright,
    /// visible as itself. All three clear the same way mechanically (flat
    /// per-contact damage to Health, see ReceiveDamage) -- the
    /// "collateral damage to the host cell" vs. "direct damage to the
    /// pathogen" distinction from the design doc is a rendering/identity
    /// difference on top of one shared HP-depletion mechanic, not two
    /// separate combat systems; see docs/TEAM_RETRO.md for why that's a
    /// reasonable scope call for this sprint (flat damage numbers requested
    /// explicitly by SPRINT_PLAN.md).
    ///
    /// Virus-class infections left uncleared through an incubation period
    /// spread to one adjacent uninfected coarse slot (TickCombat below) --
    /// the sprint's most important piece, see GAME_DESIGN.md section 4a.
    /// </summary>
    public class PathogenAgent : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private System.Action<PathogenAgent> onExit;
        private System.Func<CoarseCoord, float, bool> onSpreadRequested;

        public PathogenState State { get; private set; }
        public FineCoord Current { get; private set; }
        public PathogenClass Class { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        private int targetFineColumn;
        private int targetRow;
        private bool willAdhere;

        private Vector3 tickStartWorld;
        private Vector3 tickEndWorld;
        private float tickTimer;
        private const int TransitFineTilesPerTick = 2;

        private SpriteRenderer sr;
        private float contactFlashTimer;
        private Color restColor; // what the sprite shows when not mid-flash -- PathogenColor while transiting/large-bacterium, HostColor while an intracellular infection sits still and unnoticed
        private static readonly Color FlashColor = new Color(0.95f, 0.85f, 0.3f);

        // -- Class assignment weights (judgment call, see docs/TEAM_RETRO.md) --
        // Not specified by SPRINT_PLAN.md/GAME_DESIGN.md. Weighted so virus
        // is the most common class -- viral spread is explicitly "the
        // sprint's most important piece" (SPRINT_PLAN.md), so it needs to
        // be the class the Director actually sees most often in a short
        // playtest, not an equal three-way split that might not surface it.
        public const float VirusChance = 0.45f;
        public const float BacteriumChance = 0.25f; // remaining 0.30 is LargeBacterium

        // -- Combat numbers (judgment call, see docs/TEAM_RETRO.md) --
        // SPRINT_PLAN.md asks for "simple, flat" numbers, not balanced
        // ones. A neutrophil (3 fine-tiles/tick, so it re-enters an
        // infected coarse slot on almost every tick while wandering
        // through it) clears a 12-HP intracellular infection in roughly a
        // couple of seconds of sustained presence; a large bacterium's
        // higher HP is meant to read as "tankier, stands alone" rather
        // than anything precisely tuned.
        public const float IntracellularMaxHealth = 12f;
        public const float LargeBacteriumMaxHealth = 18f;
        public const float ContactDamagePerHit = 1f;

        // -- Viral spread timing (judgment call, see docs/TEAM_RETRO.md) --
        // 15s incubation: long enough that clearing it with cytokine
        // sensing (which finds an infected cell in ~4.5s on average per
        // docs/ENGINE_STATUS.md) comfortably beats it, short enough that a
        // rung-1 random walk (which does not reliably converge within
        // 2.5 simulated minutes on a 30-wide board, same source) visibly
        // fails to beat it in a short playtest -- this gap is the whole
        // point (GAME_DESIGN.md section 4a: "makes search speed matter in
        // a way the player can watch happen").
        public const float IncubationSeconds = 15f;
        public const float SpreadRetryIntervalSeconds = 1f; // if the incubation elapses but every neighbour is occupied, keep trying at this cadence rather than giving up permanently

        private bool hasSpread;
        private float infectionStartTime;
        private float lastSpreadAttemptTime = float.NegativeInfinity;

        public void Initialize(BoardConfig board, TissueGrid tissueGrid, System.Action<PathogenAgent> onExit, System.Func<CoarseCoord, float, bool> onSpreadRequested)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.onExit = onExit;
            this.onSpreadRequested = onSpreadRequested;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.sortingOrder = 20;
            float worldSize = BoardConfig.FineTileWorldSize * 1.5f;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            int entryRow = Random.Range(0, BoardConfig.Rows);
            int entryFineRow = entryRow * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2;
            Current = new FineCoord(0, entryFineRow);

            targetRow = Random.Range(0, BoardConfig.Rows);
            willAdhere = Random.value < 0.7f; // ~70% adhere, ~30% transit straight through and exit
            targetFineColumn = willAdhere ? Random.Range(1, board.FineColumns - 1) : board.FineColumns;

            State = PathogenState.Transiting;
            contactFlashTimer = 0f;
            hasSpread = false;
            restColor = BoardRenderer.PathogenColor; // visible as itself while transiting, regardless of eventual class
            sr.color = restColor;
            sr.enabled = true;
            tickStartWorld = tickEndWorld = board.FineToWorld(Current);
            transform.position = tickStartWorld;
            tickTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds);
        }

        /// <summary>
        /// Places a pathogen directly into the Adhered state at a known
        /// coarse slot, bypassing the transit walk entirely. Used for
        /// viral spread (a new infection appears in an adjacent slot the
        /// instant an incubated infection spreads, not by "walking" there)
        /// -- see PathogenSpawner.RequestSpread, the only production
        /// caller. currentTime is explicit (not read via UnityEngine.Time)
        /// so this stays headlessly testable, matching the rest of this
        /// sprint's simulation classes -- see
        /// Assets/Editor/CombatVerification.cs.
        /// </summary>
        public void InitializeAdheredDirect(
            BoardConfig board, TissueGrid tissueGrid,
            System.Action<PathogenAgent> onExit, System.Func<CoarseCoord, float, bool> onSpreadRequested,
            CoarseCoord slot, PathogenClass pClass, float currentTime)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.onExit = onExit;
            this.onSpreadRequested = onSpreadRequested;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.sortingOrder = 20;
            float worldSize = BoardConfig.FineTileWorldSize * 1.5f;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            Class = pClass;
            SetHealthForClass();

            tissueGrid.TryAdhere(slot, this, currentTime);

            Current = new FineCoord(
                slot.Column * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2,
                slot.Row * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2);
            State = PathogenState.Adhered;
            infectionStartTime = currentTime;
            hasSpread = false;
            lastSpreadAttemptTime = float.NegativeInfinity;
            contactFlashTimer = 0f;
            ApplyRestColorForCurrentClass();
            tickStartWorld = tickEndWorld = board.FineToWorld(Current);
            transform.position = tickEndWorld;
        }

        private void Update()
        {
            if (board == null) return;

            if (State == PathogenState.Adhered)
            {
                TickCombat(Time.time);
                return;
            }

            if (State != PathogenState.Transiting) return;

            tickTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tickTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tickStartWorld, tickEndWorld, t);

            if (tickTimer >= BoardConfig.TickIntervalSeconds)
            {
                tickTimer -= BoardConfig.TickIntervalSeconds;
                DoTick();
            }
        }

        private void DoTick()
        {
            tickStartWorld = transform.position;

            for (int i = 0; i < TransitFineTilesPerTick && State == PathogenState.Transiting; i++)
            {
                int nextCol = Current.Column + 1;
                if (nextCol >= board.FineColumns)
                {
                    Exit();
                    return;
                }
                Current = new FineCoord(nextCol, Current.Row);

                if (willAdhere && Current.Column >= targetFineColumn)
                {
                    TryAdhereHere();
                    if (State != PathogenState.Transiting) return;
                }
            }

            tickEndWorld = board.FineToWorld(Current);
        }

        private void TryAdhereHere()
        {
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            int col = coarse.Column;

            // Prefer the row chosen at spawn; otherwise take the nearest
            // free row in the same column.
            var rowsByPreference = new System.Collections.Generic.List<int> { targetRow };
            for (int dr = 1; dr < BoardConfig.Rows; dr++)
            {
                int up = targetRow - dr;
                int down = targetRow + dr;
                if (up >= 0) rowsByPreference.Add(up);
                if (down < BoardConfig.Rows) rowsByPreference.Add(down);
            }

            foreach (var row in rowsByPreference)
            {
                var candidate = new CoarseCoord(col, row);
                if (tissueGrid.TryAdhere(candidate, this, Time.time))
                {
                    Current = new FineCoord(
                        candidate.Column * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2,
                        candidate.Row * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2);
                    State = PathogenState.Adhered;
                    Class = PickRandomClass();
                    SetHealthForClass();
                    infectionStartTime = Time.time;
                    hasSpread = false;
                    lastSpreadAttemptTime = float.NegativeInfinity;
                    ApplyRestColorForCurrentClass();
                    tickEndWorld = board.FineToWorld(Current);
                    transform.position = tickEndWorld;
                    return;
                }
            }

            // Column is full top to bottom -- keep transiting and retry
            // adhesion one coarse cell further along instead of giving up.
            targetFineColumn = Current.Column + BoardConfig.FineSubdivision;
            if (targetFineColumn >= board.FineColumns)
            {
                willAdhere = false;
                targetFineColumn = board.FineColumns;
            }
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
        /// "visible as the host cell, not itself" -- large bacteria are
        /// "visible as itself, no disguise." Large bacteria keep their own
        /// small sprite visible (and flash on hit, see LateUpdate).
        /// Intracellular pathogens instead disable their own sprite
        /// entirely: an early version tried tinting it to flat HostColor
        /// and leaving it enabled, but BoardRenderer's coarse-cell
        /// background is HEAT-BLENDED (host color lerped toward orange by
        /// local cytokine strength -- see BoardRenderer.Refresh) while a
        /// flat-tinted small sprite sitting on top of it is not, so the
        /// unblended square was visibly a different shade than its
        /// surroundings -- an accidental "tell" that defeated the whole
        /// point of "not visible as itself, until sensed." Disabling the
        /// sprite outright removes that artifact at the cost of losing the
        /// small per-hit flash for intracellular combat specifically (see
        /// docs/TEAM_RETRO.md) -- large bacteria still flash normally.</summary>
        private void ApplyRestColorForCurrentClass()
        {
            bool isLargeBacterium = Class == PathogenClass.LargeBacterium;
            restColor = isLargeBacterium ? BoardRenderer.PathogenColor : BoardRenderer.HostColor;
            sr.color = restColor;
            sr.enabled = isLargeBacterium;
        }

        /// <summary>
        /// Per-frame check (Update calls this with Time.time; a headless
        /// harness can call it directly with simulated time -- see
        /// Assets/Editor/CombatVerification.cs) for whether an uncleared
        /// virus infection should spread to an adjacent coarse slot.
        /// Bacterial intracellular infections and large bacteria never
        /// spread (GAME_DESIGN.md section 4a: virus-specific). Retries on
        /// SpreadRetryIntervalSeconds if the attempt fails (every neighbour
        /// occupied) rather than giving up after one try, so a
        /// still-uncleared infection keeps pressing outward as
        /// opportunities open up.
        /// </summary>
        public void TickCombat(float currentTime)
        {
            if (State != PathogenState.Adhered) return;
            if (Class != PathogenClass.IntracellularVirus) return;
            if (hasSpread) return;
            if (currentTime - infectionStartTime < IncubationSeconds) return;
            if (currentTime - lastSpreadAttemptTime < SpreadRetryIntervalSeconds) return;

            lastSpreadAttemptTime = currentTime;
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            if (onSpreadRequested != null && onSpreadRequested(coarse, currentTime))
            {
                hasSpread = true;
            }
        }

        /// <summary>
        /// Flat per-contact damage (SPRINT_PLAN.md: "keep damage numbers
        /// simple, flat rates are fine"), called by SearchUnit each tick a
        /// unit comes within contact range of this pathogen's fine tile
        /// (Sprint 3 tightened that from "shares this pathogen's coarse
        /// slot" -- see SearchUnit.CheckContact). Reaching zero health
        /// clears the infection/pathogen back to bare host tissue --
        /// releases the TissueGrid slot (unused since Sprint 1) and returns
        /// this instance to its pool via onExit, exactly like the existing
        /// transit-and-exit path.
        ///
        /// **Sprint 3: kill attribution (SPRINT_PLAN.md item 6).**
        /// <paramref name="source"/> is the unit that landed this hit.
        /// EXACTLY ONE unit is ever credited with a kill: whoever's hit
        /// crosses zero. If several units damage the same pathogen on the
        /// same tick, the earlier hits credit nothing and the later ones
        /// no-op entirely (State is already Cleared by then, so this method
        /// returns at the first line) -- no split or shared credit. That
        /// single credit is what drives the depleting-unit lifecycle in
        /// GAME_DESIGN.md section 6d.
        ///
        /// A NULL source stays legal and always will: viral spread,
        /// environmental/collateral damage, and harness fixtures all pass
        /// null. It simply means the kill is credited to nobody.
        /// </summary>
        public void ReceiveDamage(float amount, SearchUnit source)
        {
            if (State != PathogenState.Adhered) return;
            contactFlashTimer = 0.25f;
            Health -= amount;
            if (Health > 0f) return;

            // Credit before clearing: ClearFromCombat can return this
            // instance to the pool via onExit, and the credited unit should
            // not depend on anything this object still holds afterwards.
            if (source != null) source.RegisterKill();
            ClearFromCombat();
        }

        private void ClearFromCombat()
        {
            State = PathogenState.Cleared;
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            tissueGrid.ReleaseSlot(coarse);
            onExit?.Invoke(this);
        }

        private void Exit()
        {
            State = PathogenState.Cleared;
            onExit?.Invoke(this);
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
        /// instance to the pool (transit/exit case, or a combat-cleared
        /// pathogen -- both funnel through the same onExit callback).</summary>
        public void ResetForPool()
        {
            State = PathogenState.Transiting;
            board = null;
            tissueGrid = null;
            onSpreadRequested = null;
            contactFlashTimer = 0f;
            hasSpread = false;
            Health = 0f;
            MaxHealth = 0f;
        }
    }
}
