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
    /// SimulationTick() is public and takes no UnityEngine.Time reads, per
    /// this project's convention that anything worth verifying is callable
    /// by a headless harness (see BoneMarrowManager.Tick,
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

            tickTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tickTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tickStartWorld, tickEndWorld, t);

            if (tickTimer >= BoardConfig.TickIntervalSeconds)
            {
                tickTimer -= BoardConfig.TickIntervalSeconds;
                SimulationTick();
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
        /// </summary>
        public void SimulationTick()
        {
            if (board == null) return;

            tickStartWorld = transform.position;
            for (int i = 0; i < profile.FineTilesPerTick; i++)
            {
                StepOnce();
            }
            tickEndWorld = board.FineToWorld(Current);

            CheckContact();
            ResolveDepletionIfDue();
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
            var pathogen = tissueGrid.GetPathogenAt(coarse);
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
        /// whatever occupies this unit's current coarse slot, as ordinary
        /// combat damage.
        ///
        /// Scope note (SPRINT_PLAN.md item 3): there is no host-cell-health
        /// or fibrosis system yet, so if the slot is bare host tissue there
        /// is simply nothing to damage. That is the intended state this
        /// sprint -- the mechanism is what matters; fibrosis accounting
        /// comes later.
        ///
        /// The burst is attributed to this unit, so a kill it lands still
        /// counts toward Kills (visible in the HUD/debug) -- the `depleting`
        /// guard is what stops that from recursing back into a second
        /// depletion.
        /// </summary>
        private void Degranulate()
        {
            DegranulationFlash.Play(transform.position, BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision);

            if (tissueGrid == null) return;
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            var occupant = tissueGrid.GetPathogenAt(coarse);
            if (occupant == null) return;
            occupant.ReceiveDamage(PathogenAgent.ContactDamagePerHit * tuning.DegranulationBurstMultiplier, this);
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
