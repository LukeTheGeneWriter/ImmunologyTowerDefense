namespace ImmunologyTD.Units
{
    /// <summary>
    /// The mutable, per-tower copy of a unit kind's lifecycle numbers
    /// (GAME_DESIGN.md section 6d, SPRINT_PLAN.md scope item 5). Every value
    /// here is a plain public field on a reference type, deliberately:
    ///
    ///  - Nothing here is a `const`. The Director's explicit ruling
    ///    (2026-08-21) is that progenitor upgrades will eventually offer
    ///    "bump this tower's kill count" / "reduce this tower's
    ///    degranulation damage" as purchasable options, so an upgrade must
    ///    be a write to ONE tower's field and nothing else.
    ///  - Defaults live per unit KIND on UnitProfile (the existing home for
    ///    per-kind tuning). BoneMarrowManager copies them into a fresh
    ///    instance of this class at placement time
    ///    (<see cref="FromProfile"/>), so each tower owns its own numbers.
    ///  - An emitted unit is handed its tower's CURRENT instance at emission
    ///    time and keeps that reference for life. Because this is a
    ///    reference type, a mid-life upgrade to a tower WOULD be seen by
    ///    its already-fielded children -- see CopyFrom/the emission path in
    ///    BoneMarrowManager, which hands out a per-unit snapshot instead, so
    ///    "a tower upgraded mid-round improves its future children, not the
    ///    ones already in the field" (SPRINT_PLAN.md item 5, flagged there
    ///    as a judgment call in case the Director wants it retroactive --
    ///    making it retroactive is a one-line change: hand out
    ///    slot.Tuning directly instead of a snapshot).
    /// </summary>
    public class UnitLifecycleTuning
    {
        /// <summary>Hard ceiling on how many of one tower's own emitted
        /// units may be alive at once. Stops unbounded population growth
        /// outright (GAME_DESIGN.md section 6d). Per-tower, deliberately
        /// NOT a systemic/global cap.</summary>
        public int MaxActiveChildren;

        /// <summary>Kills a unit of this kind may land before it depletes.
        /// Neutrophils deplete fast and violently (degranulation);
        /// macrophages are longer-lived and retire quietly.</summary>
        public int KillLimit;

        /// <summary>True = this kind degranulates on depletion (self-destruct
        /// plus a collateral burst at its own coarse slot). False = quiet
        /// retirement, no collateral damage.</summary>
        public bool DegranulatesOnDepletion;

        /// <summary>Multiplier applied to PathogenAgent.ContactDamagePerHit
        /// for the degranulation burst. Only meaningful when
        /// DegranulatesOnDepletion is true.</summary>
        public float DegranulationBurstMultiplier;

        /// <summary>Contact range in FINE tiles, Chebyshev distance, between
        /// a unit's fine tile and a pathogen's stored fine tile. Replaces
        /// Sprint 2's coarse-slot-level contact test (SPRINT_PLAN.md item 7 /
        /// docs/INTERFACE.md open question 3). Explicitly NOT an exact-tile
        /// test -- with 49 fine tiles per coarse slot a random-walking unit
        /// would essentially never connect.</summary>
        public int ContactRadiusFineTiles;

        public static UnitLifecycleTuning FromProfile(UnitProfile profile) => new UnitLifecycleTuning
        {
            MaxActiveChildren = profile.MaxActiveChildren,
            KillLimit = profile.KillLimit,
            DegranulatesOnDepletion = profile.DegranulatesOnDepletion,
            DegranulationBurstMultiplier = profile.DegranulationBurstMultiplier,
            ContactRadiusFineTiles = profile.ContactRadiusFineTiles,
        };

        /// <summary>Value copy, used to snapshot a tower's current numbers
        /// onto a unit at emission time (see class comment).</summary>
        public void CopyFrom(UnitLifecycleTuning other)
        {
            MaxActiveChildren = other.MaxActiveChildren;
            KillLimit = other.KillLimit;
            DegranulatesOnDepletion = other.DegranulatesOnDepletion;
            DegranulationBurstMultiplier = other.DegranulationBurstMultiplier;
            ContactRadiusFineTiles = other.ContactRadiusFineTiles;
        }
    }
}
