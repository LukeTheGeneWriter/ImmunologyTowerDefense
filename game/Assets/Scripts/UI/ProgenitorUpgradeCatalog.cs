using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The per-kind progenitor upgrade roster (docs/UI_DESIGN.md §5).
    ///
    /// **These are placeholders that buy nothing yet** -- GAME_DESIGN.md
    /// §6d's rule stands: buying spends ATP and bumps a level and the
    /// simulation does not change. What makes them worth shipping in that
    /// state is <see cref="Row.FieldLabel"/>: every row names the exact
    /// field an effect would write, so the wiring sprint is one line per
    /// row rather than a design conversation per row. The field name is
    /// never shown to the player -- only in the debug readout while the
    /// panel is open (§3).
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
                    "FineTilesPerTick 3 → +1/lvl (needs the field moved onto UnitLifecycleTuning)", 45, 2),
        };

        private static readonly Row[] DendriticCell =
        {
            new Row("Macropinocytosis rate",
                    "Each trip to the node is worth more presentations before the cell goes back for antigen.",
                    "AdaptiveTuning.DcPresentationsPerCargo 4 → +2/lvl", 35, 3),
            new Row("CCR7 expression",
                    "The cell migrates faster — antigen reaches the node sooner.",
                    "AdaptiveTuning.DcFineTilesPerTick 3 → +1/lvl", 40, 3),
            new Row("Antigen-sparing sampling",
                    "Takes a smaller bite of each debris pile — leaves more for macrophage clearance.",
                    "AdaptiveTuning.DcDebrisSamplePerBite 0.34 → −0.08/lvl, floor 0.1", 30, 3),
        };

        private static readonly Row[] HelperT =
        {
            new Row("Clonal expansion",
                    "More helper-T cells resident in the node at once — a busier node teaches faster.",
                    "per-tower MaxActiveChildren 8 → +4/lvl", 35, 3),
            new Row("TCR affinity maturation",
                    "A near-miss barcode still teaches — more pairings count.",
                    "AdaptiveTuning.MatchMaxHammingDistance 2 → +1/lvl, cap 4", 45, 2),
            new Row("Rapid recirculation",
                    "Shorter helper-T lifespan — the repertoire refreshes faster, so you are less often stuck with no match.",
                    "AdaptiveTuning.LymphocyteLifespanSeconds 20 → −4/lvl, floor 6", 30, 3),
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
                default: return "node resident · barcode match";
            }
        }

        public static string DisplayName(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return "Macrophage";
                case UnitKind.Neutrophil: return "Neutrophil";
                case UnitKind.DendriticCell: return "Dendritic cell";
                default: return "Helper-T cell";
            }
        }
    }
}
