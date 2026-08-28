using UnityEngine;

namespace ImmunologyTD.Units
{
    public enum UnitKind { Macrophage, Neutrophil }

    /// <summary>
    /// Per-unit-type tuning. Each type gets its own configurable speed in
    /// fine tiles per tick, per GAME_DESIGN.md section 7 ("per-cell step
    /// length... required by the 7x7 choice") -- deliberately not a single
    /// shared constant, since migration speed genuinely differs by cell
    /// type (neutrophils are among the fastest migrating leukocytes,
    /// macrophages markedly slower).
    ///
    /// Sprint 3 addition: this class is also the home for the per-KIND
    /// DEFAULTS of the lifecycle numbers (GAME_DESIGN.md section 6d).
    /// They are Inspector-serialized fields, never consts -- a bone marrow
    /// tower copies them into its own mutable UnitLifecycleTuning at
    /// placement time, and a future progenitor upgrade mutates that copy.
    /// See UnitLifecycleTuning for the full reasoning.
    /// </summary>
    [System.Serializable]
    public class UnitProfile
    {
        public UnitKind Kind;
        public string DisplayName;
        [Min(1)] public int FineTilesPerTick = 1;
        [Min(1)] public int FootprintFineTiles = 3;
        public Color Color = Color.white;

        [Header("Lifecycle defaults (seed a tower's own mutable copy)")]

        /// <summary>Default max simultaneously-alive children per tower of
        /// this kind. SPRINT_PLAN.md item 1's starting default is 10 -- the
        /// Director's own example number, flagged there as tunable.</summary>
        [Min(1)] public int MaxActiveChildren = 10;

        /// <summary>Default kill count before a unit of this kind depletes.
        /// SPRINT_PLAN.md: neutrophil 5, macrophage 20 (Director,
        /// 2026-08-21 -- an earlier drafted 15 read "a little low").</summary>
        [Min(1)] public int KillLimit = 5;

        /// <summary>Neutrophils degranulate on depletion; macrophages retire
        /// quietly (GAME_DESIGN.md section 6d).</summary>
        public bool DegranulatesOnDepletion = false;

        /// <summary>Degranulation collateral burst, as a multiple of
        /// PathogenAgent.ContactDamagePerHit. SPRINT_PLAN.md item 3's
        /// starting default is 3x.</summary>
        [Min(0f)] public float DegranulationBurstMultiplier = 3f;

        /// <summary>Contact range in fine tiles (Chebyshev). SPRINT_PLAN.md
        /// item 7's starting default is 2.</summary>
        [Min(0)] public int ContactRadiusFineTiles = 2;

        /// <summary>Per-tick chance, while this unit is in contact with an
        /// `Infected` host cell, of recognising the infection and killing
        /// the cell **loudly** (necrotic, cell + all contents, nothing
        /// released) -- the contact stress-sense roll of GAME_DESIGN.md §4b.
        /// **Low for innate cells** (macrophage/neutrophil): they only do
        /// generic stress/damage sensing. The not-yet-built stress sensors
        /// (γδ T / CTL / NK) will carry a much higher value -- that gap is
        /// the innate↔adaptive bridge. Default 0 = this kind cannot sense
        /// intracellular infection at all. Per-tower mutable, never a const
        /// (§6d), so an upgrade or a new unit kind is a one-field write.</summary>
        [Min(0f)] public float StressSenseChancePerTick = 0f;

        /// <summary>Debris cleared per logical tick while a unit of this kind
        /// stands on a dead cell -- efferocytosis, the macrophage's real
        /// second job (GAME_DESIGN.md section 1c, SPRINT_PLAN.md item 3).
        /// **Default 0 = this kind does not clear debris at all**, which is
        /// how "only macrophages clear it" falls out without a kind check in
        /// SearchUnit. The macrophage default (set in GameBootstrap) clears a
        /// full pile in ~2.5s of standing on it -- far faster than the ~60s
        /// self-dissipation, so macrophage clearance is clearly the better
        /// answer, per the design's own wording. A per-tower field, never a
        /// const, so a future macrophage upgrade can raise it.</summary>
        [Min(0f)] public float EfferocytosisDebrisPerTick = 0f;
    }
}
