using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The per-kind progenitor upgrade roster (docs/UI_DESIGN.md §5).
    ///
    /// **Sprint 18: every row is live.** The effect itself lives in the
    /// model layer (<see cref="ProgenitorUpgrades"/>, row indices matching
    /// this table); this class keeps the player-facing text, the price and
    /// the cap. <see cref="Row.FieldLabel"/> names the field each row writes
    /// -- never shown to the player, only in the debug readout while the
    /// panel is open (§3). Keep it in step with ProgenitorUpgrades' numbers.
    ///
    /// Three rows per kind, the Director's call from the §10 questions.
    /// The spec offered an optional fourth per kind; the macrophage's
    /// (inflammasome priming) is deliberately **omitted** -- §5's own note
    /// argues a cheap upgrade that buys away the weak innate stress-sense
    /// roll undercuts the innate-to-adaptive bridge the whole knowledge
    /// ladder exists to make you want.
    ///
    /// Prices use the same curve as everything else in the economy:
    /// base * (1 + ShopTuning.PriceGrowthPerLevel * level).
    /// </summary>
    internal static class ProgenitorUpgradeCatalog
    {
        internal readonly struct Row
        {
            /// <summary>Immunology display name, shown to the player.</summary>
            public readonly string Name;
            /// <summary>What it does in the player's terms -- not the field.</summary>
            public readonly string Effect;
            /// <summary>The field an effect would write. Debug readout only.</summary>
            public readonly string FieldLabel;
            public readonly int BasePrice;
            public readonly int Cap;

            public Row(string name, string effect, string fieldLabel, int basePrice, int cap)
            {
                Name = name; Effect = effect; FieldLabel = fieldLabel;
                BasePrice = basePrice; Cap = cap;
            }
        }

        private static readonly Row[] Macrophage =
        {
            new Row("Efferocytic capacity",
                    "Clears debris markedly faster — frees dead ground to regrow.",
                    "UnitLifecycleTuning.EfferocytosisDebrisPerTick 0.05 → +0.03/lvl", 30, 3),
            new Row("Tissue residency (M2)",
                    "+8 kills before the cell retires quietly. A longer-lived line.",
                    "UnitLifecycleTuning.KillLimit 20 → +8/lvl", 40, 3),
            new Row("Pseudopod reach",
                    "The cell touches — and clears — pathogens one tile further out.",
                    "UnitLifecycleTuning.ContactRadiusFineTiles 2 → 3", 45, 1),
        };

        private static readonly Row[] Neutrophil =
        {
            new Row("Controlled degranulation",
                    "The terminal burst does less collateral damage — less scarring from your own defence.",
                    "UnitLifecycleTuning.DegranulationBurstMultiplier 3 → −0.5/lvl, floor 1", 40, 4),
            new Row("Extended lifespan (GM-CSF priming)",
                    "+2 kills before the cell degranulates.",
                    "UnitLifecycleTuning.KillLimit 5 → +2/lvl", 30, 4),
            new Row("Rapid chemokinesis",
                    "The cell covers ground faster on its way to a signal.",
                    "UnitLifecycleTuning.FineTilesPerTick 3 → +1/lvl", 45, 2),
        };

        private static readonly Row[] DendriticCell =
        {
            new Row("Macropinocytosis rate",
                    "Each trip to the node is worth more presentations before the cell goes back for antigen.",
                    "AdaptiveUnitTuning.DcPresentationsBonus: DcPresentationsPerCargo 4 → +2/lvl", 35, 3),
            new Row("CCR7 expression",
                    "The cell migrates faster — antigen reaches the node sooner.",
                    "AdaptiveUnitTuning.DcFineTilesBonus: DcFineTilesPerTick 3 → +1/lvl", 40, 3),
            new Row("Antigen-sparing sampling",
                    "Takes a smaller bite of each debris pile — leaves more for macrophage clearance.",
                    "AdaptiveUnitTuning.DcDebrisBiteReduction: DcDebrisSamplePerBite 0.34 → −0.08/lvl, floor 0.1", 30, 3),
        };

        private static readonly Row[] HelperT =
        {
            new Row("Clonal expansion",
                    "Proliferates faster and holds more helper-T cells in the node — a busier node teaches faster.",
                    "per-tower MaxActiveChildren 4 → +2/lvl, emission rate ×(1 + 0.5·lvl)", 35, 3),
            new Row("TCR affinity maturation",
                    "A near-miss barcode still teaches — more pairings count.",
                    "AdaptiveUnitTuning.MatchDistanceBonus: MatchMaxHammingDistance 2 → +1/lvl, cap 4", 45, 2),
            new Row("Rapid recirculation",
                    "Shorter helper-T lifespan — the repertoire refreshes faster, so you are less often stuck with no match.",
                    "AdaptiveUnitTuning.LymphocyteLifespanReduction: LymphocyteLifespanSeconds 20 → −4/lvl, floor 6", 30, 3),
        };

        private static readonly Row[] None = new Row[0];

        public static Row[] RowsFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return Macrophage;
                case UnitKind.Neutrophil: return Neutrophil;
                case UnitKind.DendriticCell: return DendriticCell;
                case UnitKind.HelperT: return HelperT;
                default: return None;
            }
        }

        public static int RowCountFor(UnitKind kind) => RowsFor(kind).Length;

        /// <summary>One-line descriptor under each kind in the tower picker
        /// (UI_DESIGN.md §6). Short enough to read while a round runs.</summary>
        public static string PickerBlurb(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return "ruffled scavenger · clears debris";
                case UnitKind.Neutrophil: return "fast · high collateral";
                case UnitKind.DendriticCell: return "antigen shuttle to the node";
                case UnitKind.CytotoxicT: return "precise kill · the cell survives";
                case UnitKind.NaturalKiller: return "IFN-gamma · upgrades your macrophages";
                case UnitKind.GammaDeltaT: return "patrols the wall · repairs it";
                case UnitKind.PlasmaCell: return "antibody factory · coats pathogens";
                default: return "node resident · barcode match";
            }
        }

        /// <summary>The unit's own board sprite -- portraits stay in sync with the units.</summary>
        public static UnityEngine.Sprite SpriteFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return ImmunologyTD.Rendering.SpriteShapes.Macrophage;
                case UnitKind.Neutrophil: return ImmunologyTD.Rendering.SpriteShapes.Neutrophil;
                case UnitKind.DendriticCell: return ImmunologyTD.Rendering.SpriteShapes.DendriteStar;
                case UnitKind.CytotoxicT: return ImmunologyTD.Rendering.SpriteShapes.CytotoxicT;
                case UnitKind.NaturalKiller: return ImmunologyTD.Rendering.SpriteShapes.NaturalKiller;
                case UnitKind.GammaDeltaT: return ImmunologyTD.Rendering.SpriteShapes.GammaDeltaT;
                case UnitKind.PlasmaCell: return ImmunologyTD.Rendering.SpriteShapes.PlasmaCell;
                default: return ImmunologyTD.Rendering.SpriteShapes.Lymphocyte;
            }
        }

        /// <summary>Sprint 23: what a knowledge-gated tower's card says
        /// while its rung is still out of reach. The card stays BUYABLE --
        /// the Director's model is "spend now, it switches on later" -- so
        /// this text is the only thing telling the player the tower will sit
        /// dark, and it names the threshold so the spend can be planned.</summary>
        public static string LockedBlurb(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.CytotoxicT: return "dark until a species hits 10% knowledge";
                case UnitKind.PlasmaCell: return "dark until a species hits 20% knowledge";
                default: return PickerBlurb(kind);
            }
        }

        public static string DisplayName(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return "Macrophage";
                case UnitKind.Neutrophil: return "Neutrophil";
                case UnitKind.DendriticCell: return "Dendritic cell";
                case UnitKind.CytotoxicT: return "Cytotoxic T cell";
                // Both names on purpose (Director, 2026-10-25): players know
                // "NK cell", but the cell that actually dominates the human
                // small-intestinal epithelium is an ILC1.
                case UnitKind.NaturalKiller: return "NK cell (ILC1)";
                case UnitKind.GammaDeltaT: return "Gamma-delta T (IEL)";
                case UnitKind.PlasmaCell: return "B cell";
                default: return "Helper-T cell";
            }
        }
    }
}
