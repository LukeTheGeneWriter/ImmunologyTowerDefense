using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// A hunting unit performing a random walk on the fine lattice (rung 1
    /// of GAME_DESIGN.md sections 7/9's search ladder), or a
    /// cytokine-biased walk (rung 2) when CytokineToggle.Enabled is on. The
    /// actual per-step choice is delegated to Chemotaxis.ChooseNextStep
    /// (Units/Chemotaxis.cs) -- pulled out to a static method during the
    /// Sprint 1 closing task so it could be verified headlessly, see that
    /// file and Assets/Editor/CytokineVerification.cs. Movement is
    /// four-neighbour (von Neumann) only.
    ///
    /// Movement/collision simplification (see docs/INTERFACE.md for the
    /// full note): units co-occupy fine tiles with host cells AND with
    /// adhered pathogens -- pathogens still do not block unit movement.
    ///
    /// Sprint 3 changes two things about the combat/lifecycle surface:
    ///
    ///  1. **Contact is now fine-tile proximity, not coarse-slot presence.**
    ///     Sprint 2 fired contact damage whenever a unit's fine tile fell
    ///     anywhere inside an occupied 7x7 coarse slot, so every unit in
    ///     that slot damaged the pathogen every tick (docs/INTERFACE.md open
    ///     question 3). Contact now requires Chebyshev distance <=
    ///     tuning.ContactRadiusFineTiles between this unit's fine tile and
    ///     the pathogen's own stored fine tile. Deliberately a RADIUS, not
    ///     an exact-tile match -- see CheckContact.
    ///  2. **Units now have a lifecycle.** A unit belongs to the bone
    ///     marrow tower that emitted it, counts its own kills, and on
    ///     reaching its kill limit either degranulates (neutrophil: a
    ///     visible burst that deals collateral damage at its own slot) or
    ///     retires quietly (macrophage), then despawns via onDespawn --
    ///     which is what frees a slot in its tower's max-active-children
    ///     count. This is the whole point of Sprint 3 (GAME_DESIGN.md
    ///     section 6d): before it, nothing ever despawned a unit and the
    ///     active population grew without bound.
    ///
    /// SimulationTick(currentTime) is public and reads no UnityEngine.Time
    /// itself -- Update() passes Time.time in, a harness passes a simulated
    /// clock -- per this project's convention that anything worth verifying
    /// is callable by a headless harness (see BoneMarrowManager.Tick,
    /// PathogenAgent.TickCombat, Chemotaxis.ChooseNextStep). Update() only
    /// handles the visual tween and the tick clock.
    /// </summary>
    public class SearchUnit : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private UnitProfile profile;
        private SpriteRenderer sr;

        /// <summary>A LIVE REFERENCE to this unit's tower's lifecycle
        /// numbers -- not a copy (Director, 2026-08-21). Sprint 3 originally
        /// handed each unit a value snapshot at emission time, so upgrading
        /// a tower only improved its future children. The Director ruled
        /// against that: an upgrade should make an INSTANT difference the
        /// moment ATP is spent, so it applies to every one of that
        /// progenitor's currently-fielded children as well as future ones.
        /// Writing to a tower's UnitLifecycleTuning is therefore immediately
        /// visible to all of its live units, by design -- do not "fix" this
        /// back into a copy.
        ///
        /// fallbackTuning is used only when a unit is created outside the
        /// tower path (headless harness fixtures pass no tower tuning), so
        /// the accessors below never null-dereference and pooled reuse still
        /// allocates nothing.</summary>
        private UnitLifecycleTuning tuning;
        private readonly UnitLifecycleTuning fallbackTuning = new UnitLifecycleTuning();

        private System.Action<SearchUnit> onDespawn;

        public FineCoord Current { get; private set; }

        /// <summary>Kills credited to this unit -- incremented only by
        /// PathogenAgent.ReceiveDamage, and only for the single hit that
        /// actually crossed zero (SPRINT_PLAN.md item 6: exactly one unit
        /// gets credit, no splitting).</summary>
        public int Kills { get; private set; }

        /// <summary>Index of the bone marrow slot that emitted this unit,
        /// or -1 for a unit created outside the tower path (headless
        /// harness fixtures). Purely informational -- the despawn
        /// notification travels through onDespawn, not this.</summary>
        public int OwnerSlotIndex { get; private set; } = -1;

        public int KillLimit => tuning.KillLimit;
        public bool DegranulatesOnDepletion => tuning.DegranulatesOnDepletion;
        public int ContactRadiusFineTiles => tuning.ContactRadiusFineTiles;
        public float EfferocytosisDebrisPerTick => tuning.EfferocytosisDebrisPerTick;

        /// <summary>True once this unit has started (or finished) its
        /// depletion sequence. Guards re-entrancy: a degranulation burst can
        /// itself land a killing hit, which calls back into RegisterKill on
        /// the very unit that is mid-despawn.</summary>
        private bool depleting;

        private Vector3 tickStartWorld;
        private Vector3 tickEndWorld;
        private float tickTimer;

        // Reused every StepOnce call so Chemotaxis.ChooseNextStep allocates
        // nothing per step; one buffer pair per unit instance.
        private readonly FineCoord[] candidateBuffer = new FineCoord[4];
        private readonly float[] weightBuffer = new float[4];

        /// <summary>
        /// Sprint 3 signature: adds the tower's lifecycle tuning, the
        /// emitting slot index, and the despawn callback (the return path
        /// that frees a slot in the tower's max-active-children count and
        /// releases this instance back to its PrefabPool). Passing
        /// <paramref name="towerTuning"/> null or <paramref name="onDespawn"/>
        /// null is legal -- a unit with no tuning falls back to the
        /// profile's own defaults, and a unit with no despawn callback
        /// simply deactivates itself instead of returning to a pool. Both
        /// exist so a headless harness can build a unit without a whole
        /// bone marrow rig.
        /// </summary>
        public void Initialize(
            BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField,
            UnitProfile profile, FineCoord start,
            UnitLifecycleTuning towerTuning = null, int ownerSlotIndex = -1,
            System.Action<SearchUnit> onDespawn = null)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.profile = profile;
            this.onDespawn = onDespawn;
            Current = start;

            // Hold the tower's OWN instance, so an upgrade bought mid-round
            // reaches this unit immediately (Director, 2026-08-21). Only a
            // tower-less unit (harness fixture) gets a private fallback.
            if (towerTuning != null)
            {
                tuning = towerTuning;
            }
            else
            {
                fallbackTuning.CopyFromProfile(profile);
                tuning = fallbackTuning;
            }
            OwnerSlotIndex = ownerSlotIndex;
            Kills = 0;
            depleting = false;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.color = profile.Color; // full reset: a recycled unit must not carry a flash tint or faded alpha
            sr.enabled = true;
            sr.sortingOrder = 10;
            float worldSize = BoardConfig.FineTileWorldSize * Mathf.Max(1, profile.FootprintFineTiles);
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            tickStartWorld = tickEndWorld = board.FineToWorld(Current);
            transform.position = tickStartWorld;
            tickTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds); // desync visual ticks between units
        }

        private void Update()
        {
            if (board == null) return; // not yet initialized, or already returned to the pool

            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: the buy phase freezes movement mid-glide

            tickTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tickTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tickStartWorld, tickEndWorld, t);

            if (tickTimer >= BoardConfig.TickIntervalSeconds)
            {
                tickTimer -= BoardConfig.TickIntervalSeconds;
                SimulationTick(ImmunologyTD.Rounds.RoundClock.Time);
            }
        }

        /// <summary>
        /// One whole logical tick: move, check contact, then resolve any
        /// depletion the contact triggered. Public and free of
        /// UnityEngine.Time reads so a headless harness drives the real
        /// production path (Assets/Editor/LifecycleVerification.cs), matching
        /// BoneMarrowManager.Tick / PathogenAgent.TickCombat.
        ///
        /// Depletion is resolved at the END of the tick rather than inside
        /// RegisterKill, because RegisterKill is called from deep inside
        /// PathogenAgent.ReceiveDamage, which is itself called from
        /// CheckContact -- despawning there would tear down the object
        /// mid-call.
        ///
        /// <paramref name="currentTime"/> is threaded through only for
        /// <see cref="CheckEfferocytosis"/> (a cleared debris pile stamps a
        /// regrowth clock). Update() passes Time.time; a harness passes a
        /// simulated clock, same convention as the rest of the project.
        /// </summary>
        public void SimulationTick(float currentTime)
        {
            if (board == null) return;

            tickStartWorld = transform.position;
            for (int i = 0; i < profile.FineTilesPerTick; i++)
            {
                StepOnce();
            }
            tickEndWorld = board.FineToWorld(Current);

            CheckContact();
            CheckStressSense(currentTime);
            CheckEfferocytosis(currentTime);
            ResolveDepletionIfDue();
        }

        /// <summary>
        /// The contact stress-sense roll (GAME_DESIGN.md §4b). While this
        /// unit is in contact with an `Infected` host cell it rolls, once per
        /// tick, to recognise the infection. On success it kills the cell
        /// **loudly** -- `KillHostCell` takes the cell to `Dead` + debris and
        /// its intracellular resident dies with it, releasing nothing (the
        /// bacterial brood only bursts on a drain-death, never here). This is
        /// the ONLY way an innate cell reaches an intracellular infection
        /// now; `GetAttackableAt` no longer returns the resident.
        ///
        /// `StressSenseChancePerTick` is low for macrophage/neutrophil --
        /// generic damage sensing, no antigen specificity. The future stress
        /// sensors (γδ T / CTL / NK) carry a high value; that gap is the
        /// point. Kinds with a 0 chance fall out here with no kind check.
        ///
        /// Contact uses the same range test as <see cref="CheckContact"/>,
        /// measured against the resident's stored fine tile (the coarse-cell
        /// centre). Public and explicit-time for the harness.
        /// </summary>
        /// <returns>True if a loud kill fired this call.</returns>
        public bool CheckStressSense(float currentTime)
        {
            if (tissueGrid == null || tuning.StressSenseChancePerTick <= 0f) return false;

            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            if (TryStressSenseAt(coarse)) return true;

            if (tuning.ContactRadiusFineTiles < BoardConfig.FineSubdivision / 2 + 1) return false;

            for (int i = 0; i < FineCoord.VonNeumannOffsets.Length; i++)
            {
                var off = FineCoord.VonNeumannOffsets[i];
                if (TryStressSenseAt(new CoarseCoord(coarse.Column + off.Column, coarse.Row + off.Row))) return true;
            }
            return false;
        }

        private bool TryStressSenseAt(CoarseCoord coarse)
        {
            if (tissueGrid.GetHostState(coarse) != HostState.Infected) return false;
            var resident = tissueGrid.GetIntracellularAt(coarse);
            if (resident == null || !WithinContactRange(resident.Current)) return false;
            if (Random.value >= tuning.StressSenseChancePerTick) return false;

            // Recognised. Loud necrotic kill: cell + everything inside, no
            // pathogen released. KillHostCell notifies the resident via
            // OnHostCellDestroyed. Credit this unit -- it did the work.
            // (The DAMP that a loud death should broadcast -- extra innate
            // recruitment, fibrosis feed -- is GAME_DESIGN.md §6, not built;
            // for now "loud" is the flash.)
            var worldCenter = board.CoarseToWorldCenter(coarse);
            tissueGrid.KillHostCell(coarse);
            RegisterKill();
            DegranulationFlash.Play(
                worldCenter,
                BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision * 1.5f,
                DegranulationFlash.StressKillColor);
            return true;
        }

        /// <summary>
        /// Efferocytosis: a macrophage standing on a dead cell eats its
        /// debris, one <see cref="EfferocytosisDebrisPerTick"/> bite per
        /// logical tick (GAME_DESIGN.md section 1c, SPRINT_PLAN.md item 3).
        ///
        /// Opportunistic, not directed -- the unit clears whatever debris it
        /// happens to walk over while hunting, nothing more. That is enough
        /// for this sprint: the design's "competing demand" between clearing
        /// and killing is about how the PLAYER allocates macrophages, not
        /// per-unit AI. A macrophage that seeks debris is a later concern.
        ///
        /// Kinds with EfferocytosisDebrisPerTick == 0 (neutrophils) fall out
        /// here with no kind check. Public and explicit-time so a headless
        /// harness drives the real path, exactly like CheckContact.
        /// </summary>
        /// <returns>True if this call finished a debris pile off.</returns>
        public bool CheckEfferocytosis(float currentTime)
        {
            if (tissueGrid == null || tuning.EfferocytosisDebrisPerTick <= 0f) return false;

            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            if (!tissueGrid.ClearDebris(coarse, tuning.EfferocytosisDebrisPerTick, currentTime)) return false;

            // Pile finished -- the ground is bare and its regrowth clock has
            // started. Show it, so recovering ground reads as an event.
            DegranulationFlash.Play(
                board.CoarseToWorldCenter(coarse),
                BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision,
                DegranulationFlash.EfferocytosisColor);
            return true;
        }

        private void StepOnce()
        {
            Current = Chemotaxis.ChooseNextStep(
                Current, board, cytokineField, CytokineToggle.Enabled, candidateBuffer, weightBuffer);
        }

        /// <summary>
        /// Sprint 3: fine-tile PROXIMITY contact, replacing Sprint 2's
        /// "anywhere in the same coarse slot" test (docs/INTERFACE.md open
        /// question 3). Damage lands only if this unit's fine tile is within
        /// tuning.ContactRadiusFineTiles (Chebyshev) of the pathogen's own
        /// stored fine tile.
        ///
        /// **Chebyshev, not Manhattan** -- a judgment call
        /// (SPRINT_PLAN.md item 7 leaves it to the implementer). Chebyshev
        /// radius r is a square (2r+1)^2 neighbourhood, which matches both
        /// the square sprites and the square footprints these units already
        /// have; the Manhattan diamond at the same r covers barely half the
        /// tiles and would have roughly halved contact frequency again on
        /// top of the change this item already makes.
        ///
        /// **Deliberately a radius, not an exact-tile match.** With 49 fine
        /// tiles per coarse slot, requiring exact coincidence would make a
        /// random-walking unit essentially never connect and combat would
        /// stop working. If time-to-clear feels wrong in playtest, tune the
        /// radius (it is a per-tower field) -- do not revert to coarse-slot
        /// detection.
        ///
        /// A pathogen sits at the CENTRE of its coarse slot (local fine
        /// 3,3), so at any radius below FineSubdivision/2 + 1 == 4 only the
        /// pathogen in the unit's own coarse slot can possibly be in range;
        /// that is the default-radius hot path, one grid lookup and no
        /// allocation. The neighbour scan below only runs if someone raises
        /// the radius far enough for it to matter, so a future upgrade that
        /// widens contact range stays correct instead of silently clipping
        /// at the slot boundary.
        /// </summary>
        /// <returns>True if contact damage was dealt this call.</returns>
        public bool CheckContact()
        {
            if (tissueGrid == null) return false;

            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            if (TryDamageAt(coarse)) return true;

            if (tuning.ContactRadiusFineTiles < BoardConfig.FineSubdivision / 2 + 1) return false;

            for (int i = 0; i < FineCoord.VonNeumannOffsets.Length; i++)
            {
                var off = FineCoord.VonNeumannOffsets[i];
                if (TryDamageAt(new CoarseCoord(coarse.Column + off.Column, coarse.Row + off.Row))) return true;
            }
            return false;
        }

        private bool TryDamageAt(CoarseCoord coarse)
        {
            var pathogen = tissueGrid.GetAttackableAt(coarse);
            if (pathogen == null) return false;
            if (!WithinContactRange(pathogen.Current)) return false;
            pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, this);
            return true;
        }

        private bool WithinContactRange(FineCoord target)
        {
            int dc = Mathf.Abs(target.Column - Current.Column);
            int dr = Mathf.Abs(target.Row - Current.Row);
            return Mathf.Max(dc, dr) <= tuning.ContactRadiusFineTiles;
        }

        /// <summary>
        /// Credits one kill to this unit. Called by PathogenAgent only for
        /// the single hit that crossed zero health -- never split between
        /// units that damaged the same pathogen on the same tick
        /// (SPRINT_PLAN.md item 6). Increments the counter unconditionally
        /// (so a kill landed by a degranulation burst is still counted and
        /// visible) but never re-enters the depletion sequence.
        /// </summary>
        public void RegisterKill()
        {
            Kills++;
            // GAME_DESIGN.md §5b: per-kill ATP income. The single chokepoint
            // for "a unit got a kill" -- reached from PathogenAgent.ReceiveDamage
            // on the fatal hit and from TryStressSenseAt's loud kill. Brood
            // burst / burn-out / drain-death do not come through here, so
            // they correctly pay nothing.
            ImmunologyTD.Economy.EconomyHooks.ReportKill();
        }

        /// <summary>True once this unit has earned enough kills to deplete
        /// and has not yet done so.</summary>
        public bool IsDepletionDue => !depleting && board != null && Kills >= tuning.KillLimit;

        /// <summary>
        /// Runs the depletion sequence if this unit has hit its kill limit:
        /// a neutrophil-style degranulation (visible burst + collateral
        /// damage to whatever occupies its current coarse slot) or a
        /// macrophage-style quiet retirement (nothing but the despawn), then
        /// despawn either way. Public and explicit so a headless harness
        /// drives the real path; SimulationTick calls it at the end of every
        /// tick.
        /// </summary>
        /// <returns>True if the unit depleted and despawned on this call.</returns>
        public bool ResolveDepletionIfDue()
        {
            if (!IsDepletionDue) return false;
            depleting = true;

            if (tuning.DegranulatesOnDepletion)
            {
                Degranulate();
            }

            Despawn();
            return true;
        }

        /// <summary>
        /// GAME_DESIGN.md section 6d: a depleted neutrophil does not simply
        /// vanish, it self-destructs and dumps its granule contents into
        /// wherever it happens to be standing. Mechanically: a burst equal
        /// to ContactDamagePerHit * DegranulationBurstMultiplier applied to
        /// whatever is in this unit's current coarse slot -- both an
        /// extracellular occupant (ordinary combat damage) **and the host
        /// cell itself** (`DamageHostCell`). §6d's wording is "whatever host
        /// cell or infected cell is there," and hitting the cell is how the
        /// burst reaches an intracellular infection (which `GetAttackableAt`
        /// no longer exposes, §4b) -- a loud, indiscriminate kill, exactly
        /// the neutrophil's "high collateral tissue damage" trait. If the
        /// burst kills a `Healthy`/`Infected` cell it leaves debris and
        /// feeds fibrosis later (§6).
        ///
        /// The burst is attributed to this unit, so a kill it lands still
        /// counts toward Kills -- the `depleting` guard stops that from
        /// recursing into a second depletion.
        /// </summary>
        private void Degranulate()
        {
            DegranulationFlash.Play(transform.position, BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision);

            if (tissueGrid == null) return;
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            float burst = PathogenAgent.ContactDamagePerHit * tuning.DegranulationBurstMultiplier;

            var occupant = tissueGrid.GetAttackableAt(coarse);
            if (occupant != null) occupant.ReceiveDamage(burst, this);

            // Collateral to the host cell -- kills an infected or damaged
            // cell outright at the 3x burst, which is the point.
            tissueGrid.DamageHostCell(coarse, burst);
        }

        /// <summary>
        /// The return path that did not exist before Sprint 3. Notifies the
        /// emitting tower (which decrements its active-children count and
        /// releases this instance back to its PrefabPool -- see
        /// BoneMarrowManager.OnChildDespawned); with no callback wired (a
        /// harness fixture), just deactivates. Pooling is non-negotiable
        /// here per GAME_DESIGN.md section 8 -- nothing about this path may
        /// ever become a Destroy().
        /// </summary>
        private void Despawn()
        {
            var callback = onDespawn;
            if (callback != null)
            {
                callback(this);
            }
            else
            {
                ResetForPool();
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Mirrors PathogenAgent.ResetForPool -- called just before this
        /// instance goes back to its PrefabPool so a recycled unit carries
        /// no stale state (kill count, tower back-reference, board refs,
        /// sprite tint). Nulling `board` also makes Update() a no-op while
        /// the instance sits inactive in the pool.
        /// </summary>
        public void ResetForPool()
        {
            board = null;
            tissueGrid = null;
            cytokineField = null;
            profile = null;
            onDespawn = null;
            tuning = fallbackTuning; // drop the tower reference; Initialize reassigns before use
            Kills = 0;
            depleting = false;
            OwnerSlotIndex = -1;
            tickTimer = 0f;
            if (sr != null)
            {
                sr.color = Color.white;
                sr.enabled = true;
            }
        }
    }
}
