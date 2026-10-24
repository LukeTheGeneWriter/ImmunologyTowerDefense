using UnityEngine;

namespace ImmunologyTD.Units
{
    /// <summary>The four progenitor kinds the bone marrow can place. The
    /// first two are innate <see cref="SearchUnit"/> towers; Sprint 8 adds
    /// the two adaptive kinds, which emit their own agent types
    /// (<c>DendriticCell</c> / <c>Lymphocyte</c>) via <c>AdaptiveDirector</c>
    /// rather than a SearchUnit. All four share the same 5 marrow slots
    /// (GAME_DESIGN.md §1c/§2a -- marrow real estate is the constraint).</summary>
    /// <summary>
    /// Sprint 23 appends four kinds; **ordinals of the existing four are
    /// load-bearing** (serialized slots, saved shop state), so new kinds go
    /// on the end, never in the middle.
    ///
    /// The four new ones are deliberately NOT four flavours of "killer" --
    /// the Sprint 23 research (docs/CELL_KINETICS_REFERENCE.md) found that
    /// only the CTL is primarily a killer:
    ///  - CytotoxicT -- precise QUIET kill of an infected cell, knowledge-gated.
    ///  - NaturalKiller -- "NK cell (ILC1)". Mostly an IFN-gamma AMPLIFIER of
    ///    nearby macrophages, with a weak quiet kill of its own. Intestinal
    ///    NK/ILC1s are published as less cytotoxic and higher IFN-gamma than
    ///    blood NK cells, so a straight "cheap CTL" would be wrong.
    ///  - GammaDeltaT -- intraepithelial patroller: barrier integrity and
    ///    repair at the wall, NOT tissue DPS.
    ///  - PlasmaCell -- sessile antibody factory emitted by a B-cell tower.
    /// </summary>
    public enum UnitKind
    {
        Macrophage,
        Neutrophil,
        DendriticCell,
        HelperT,
        CytotoxicT,
        NaturalKiller,
        GammaDeltaT,
        PlasmaCell,
    }

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

        /// <summary>Sprint 13: the procedurally-drawn shape sprite for this
        /// kind (`ImmunologyTD.Rendering.SpriteShapes`). Not serializable
        /// (a runtime-generated `Sprite`), so `GameBootstrap` assigns it in
        /// `Awake` after the profiles exist; `SearchUnit` reads it. Null
        /// falls back to `RuntimeSprites.SquareSprite`.</summary>
        [System.NonSerialized] public Sprite Shape;

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

        /// <summary>Sprint 23. **The CTL's entire reason for existing.**
        /// False = the stress-sense kill is LOUD: the host cell dies with the
        /// pathogen inside it (necrosis, collateral, fibrosis feed). True =
        /// QUIET: the intracellular pathogen is destroyed and **the host cell
        /// survives**, per GAME_DESIGN.md §5's ~10% rung ("precise kill of an
        /// infected cell -- no collateral, no necrotic DAMP").
        ///
        /// Note the mechanic is NOT "more damage" -- it is that the enterocyte
        /// is still standing afterwards. That is what the player is buying
        /// when they invest in adaptive immunity, and it is the only reason
        /// innate-only play hits a tissue-damage ceiling.</summary>
        public bool QuietKill = false;

        /// <summary>Sprint 23. Per-tick chance this unit's IFN-gamma aura
        /// upgrades a macrophage it is near -- the NK/ILC1 tower's PRIMARY
        /// job, not a side effect. 0 = this kind has no aura.
        ///
        /// Research (docs/CELL_KINETICS_REFERENCE.md): human intestinal NK
        /// cells are published as less cytotoxic and higher IFN-gamma than
        /// blood NK cells, and the clearest in vivo demonstration of NK
        /// function anywhere is NK cells cutting *intramacrophage* Salmonella
        /// load. So the honest NK tower makes macrophages better rather than
        /// competing with them.</summary>
        [Min(0f)] public float IfnGammaAuraChancePerTick = 0f;

        /// <summary>Sprint 23. Radius in COARSE cells over which the above
        /// aura reaches. 0 with a non-zero chance means "same cell only".</summary>
        [Min(0)] public int IfnGammaAuraCoarseRadius = 0;

        /// <summary>Sprint 23. How many coarse cells of wall this kind
        /// patrols from, and how hard it holds that wall together. 0 = this
        /// kind does not interact with the barrier at all, which is every
        /// kind except the intraepithelial gamma-delta T cell.
        ///
        /// The research is unusually specific here: gamma-delta IELs
        /// converge on the enterocyte *currently being touched by bacteria*,
        /// migration-defective gamma-delta cells are as useless as having
        /// none at all, and increasing their intercalation cut bacterial
        /// invasion by ~63% (Edelblum 2015). So the mechanic is
        /// INTERCEPTION AND REPAIR at the wall, not damage -- a gamma-delta
        /// tower built as another hunter would have been the obvious game
        /// design and a straightforward misreading of the cell.</summary>
        [Min(0f)] public float BarrierPatrolRadiusCells = 0f;

        /// <summary>Seconds between this kind's barrier repair attempts.
        /// 0 = never repairs.</summary>
        [Min(0f)] public float BarrierRepairIntervalSeconds = 0f;

        /// <summary>Sprint 23. Seconds between antibody secretions for a
        /// unit of this kind. 0 = this kind secretes nothing, which is how
        /// "only plasma cells make antibody" falls out with no kind check in
        /// SearchUnit -- the same trick EfferocytosisDebrisPerTick uses to
        /// keep debris clearance a macrophage job.</summary>
        [Min(0f)] public float AntibodySecretionIntervalSeconds = 0f;

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
