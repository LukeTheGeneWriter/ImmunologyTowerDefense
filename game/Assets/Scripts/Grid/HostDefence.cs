using UnityEngine;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Sprint 29. The body shop's host-side upgrades, in one place.
    ///
    /// **Four of the shop's eight rows were taking the player's ATP and
    /// doing nothing** — hatched "planned" in the UI, which was honest, but
    /// it meant half the body shop was scenery. This is where they become
    /// real. Levels are pushed in from `UiController` exactly as the
    /// working rows already are; nothing here reaches into the ledger.
    ///
    /// Process-global mutable statics with a reset, the same shape as
    /// `Chemotaxis.SensingUpgradeLevel` and the tuning classes — a harness
    /// sets a level, drives the real code path, and resets after.
    /// </summary>
    public static class HostDefence
    {
        // -- Levels, pushed from the shop ------------------------------

        public static int ViralEntryLevel;      // IFITM restriction
        public static int GlycocalyxLevel;      // MUC17
        public static int DsRnaSensorLevel;     // OAS/RNase L
        public static int ComplementLevel;      // local C3 opsonisation

        // -- Tuning ----------------------------------------------------

        /// <summary>
        /// Fractional cut in per-tick viral entry chance per level.
        ///
        /// Deliberately modest. IFITM restriction is real but the measured
        /// human effect sizes are small (odds ratios in the 1.7–2.7 range),
        /// and it is **not** universal — for at least one coronavirus IFITM
        /// expression *enhances* entry rather than blocking it. A row that
        /// made viruses bounce off the epithelium would be a much bigger
        /// claim than the evidence supports.
        /// </summary>
        public static float ViralEntryCutPerLevel = 0.18f;

        /// <summary>
        /// Fractional cut in grazing damage per level, in HEALTHY tissue.
        ///
        /// The honest mechanism is the MUC17 glycocalyx, not "a tougher
        /// enterocyte" — no support was found for the latter. MUC17 is
        /// cell-intrinsic, small-intestine-specific, and knockout mice are
        /// specifically prone to small-bowel infection and translocation.
        /// </summary>
        public static float GlycocalyxCutPerLevel = 0.14f;

        /// <summary>
        /// Extra cut applied in proportion to how INFLAMED the local wall
        /// is. MUC17 is upregulated by TNFα, which makes this the first
        /// host defence in the game that gets *stronger* in damaged tissue
        /// — and therefore the natural counter to the attaching-effacing
        /// E. coli, which feeds on inflammation. The two mechanics were
        /// found by different research passes and happen to answer each
        /// other.
        /// </summary>
        public static float GlycocalyxInflammationBonus = 0.30f;

        /// <summary>
        /// Per-tick chance an infected host cell destroys itself, per level.
        ///
        /// **The death is APOPTOTIC (clean).** The sensor is RIG-I/MDA5 —
        /// explicitly NOT PKR for rotavirus — and the executioner that
        /// matters here is OAS/RNase L, which kills through caspase-3;
        /// RNase L-null cells are profoundly impaired at dying. So this
        /// reuses the cytotoxic path rather than the necrotic one, which
        /// means the tissue recovers rather than scarring.
        ///
        /// There IS a messy branch in the biology (ZBP1 → MLKL necroptosis,
        /// a potent neutrophil activator) and it is a good future upgrade
        /// fork — buy the fast death, pay in debris. Not built; the clean
        /// branch is the one the design already has machinery for.
        /// </summary>
        public static float DsRnaSelfDestructPerLevelPerTick = 0.004f;

        /// <summary>
        /// Bonus multiplier on an innate cell's contact damage per level —
        /// opsonisation making a phagocyte's job easier.
        ///
        /// **Not lysis.** The gut lacks membrane-attack-complex components,
        /// which is precisely how commensals survive C3 deposition, and gut
        /// C3 deposits through the alternative pathway rather than being
        /// antibody-triggered. So complement here marks targets for a cell
        /// to finish; it never kills on its own. It deliberately does NOT
        /// set the IgA coating flag either — that flag drives daughter
        /// cross-linking, and complement has no business enchaining
        /// anything.
        /// </summary>
        public static float OpsonisationDamageBonusPerLevel = 0.22f;

        // -- Queries ---------------------------------------------------

        /// <summary>Multiplier on per-tick viral entry chance.</summary>
        public static float ViralEntryMultiplier =>
            Mathf.Clamp01(1f - ViralEntryCutPerLevel * Mathf.Max(0, ViralEntryLevel));

        /// <summary>Multiplier on an innate cell's contact damage.</summary>
        public static float OpsonisationDamageMultiplier =>
            1f + OpsonisationDamageBonusPerLevel * Mathf.Max(0, ComplementLevel);

        /// <summary>Per-tick self-destruct chance for an infected cell.</summary>
        public static float DsRnaSelfDestructChance =>
            DsRnaSelfDestructPerLevelPerTick * Mathf.Max(0, DsRnaSensorLevel);

        /// <summary>
        /// Multiplier on grazing damage at <paramref name="coarse"/>.
        /// Stronger where the wall in front of it is inflamed.
        /// </summary>
        public static float GrazeDamageMultiplier(BoardConfig board, CoarseCoord coarse)
        {
            int level = Mathf.Max(0, GlycocalyxLevel);
            if (level <= 0 || board == null) return 1f;

            float cut = GlycocalyxCutPerLevel * level;

            var wall = GutWall.Current;
            if (wall != null)
            {
                float inflamed = wall.LocalInflammation(wall.SOfLane(board.CrossIndex(coarse)));
                cut += GlycocalyxInflammationBonus * cut * inflamed;
            }

            return Mathf.Clamp(1f - cut, 0.25f, 1f);
        }

        public static void ResetToDefaults()
        {
            ViralEntryLevel = 0;
            GlycocalyxLevel = 0;
            DsRnaSensorLevel = 0;
            ComplementLevel = 0;
            ViralEntryCutPerLevel = 0.18f;
            GlycocalyxCutPerLevel = 0.14f;
            GlycocalyxInflammationBonus = 0.30f;
            DsRnaSelfDestructPerLevelPerTick = 0.004f;
            OpsonisationDamageBonusPerLevel = 0.22f;
        }
    }
}
