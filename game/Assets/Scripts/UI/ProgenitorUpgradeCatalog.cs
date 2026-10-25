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
            // Sprint 29: was "Tissue residency (M2)". M1/M2 as discrete
            // states is something the field published a consensus paper to
            // discourage, and the row's actual effect is a kill count, which
            // has nothing to do with ontogeny. What polarisation really
            // trades is arginine — NOS2 (killing) against ARG1 (repair).
            new Row("Sustained respiratory burst",
                    "+8 kills before the cell is spent. A longer-working line.",
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
            // Sprint 29: was "Macropinocytosis rate", which is uptake — and
            // uptake is switched OFF when a DC matures and leaves for the
            // node, so it cannot be what makes a trip worth more. It also
            // contradicted this tower's third row, which sells REDUCED
            // uptake as a benefit. What actually extends a trip is how long
            // peptide-MHC-II survives on the surface (10h → >100h on
            // maturation).
            new Row("pMHC-II stabilisation",
                    "Each trip to the node is worth more presentations before the cell goes back for antigen.",
                    "AdaptiveUnitTuning.DcPresentationsBonus: DcPresentationsPerCargo 4 → +2/lvl", 35, 3),
            new Row("CCR7 expression",
                    "The cell migrates faster — antigen reaches the node sooner.",
                    "AdaptiveUnitTuning.DcFineTilesBonus: DcFineTilesPerTick 3 → +1/lvl", 40, 3),
            // Sprint 29: renamed. Dendritic cells are deliberately
            // protease-POOR — they preserve antigen where a macrophage
            // destroys it — which is the real reason the two compete for
            // the same debris.
            new Row("Limited lysosomal proteolysis",
                    "Takes a smaller bite of each debris pile — leaves more for macrophage clearance.",
                    "AdaptiveUnitTuning.DcDebrisBiteReduction: DcDebrisSamplePerBite 0.34 → −0.08/lvl, floor 0.1", 30, 3),
        };

        private static readonly Row[] HelperT =
        {
            new Row("Clonal expansion",
                    "Proliferates faster and holds more helper-T cells in the node — a busier node teaches faster.",
                    "per-tower MaxActiveChildren 4 → +2/lvl, emission rate ×(1 + 0.5·lvl)", 35, 3),
            // Sprint 29: was "TCR affinity maturation", which is wrong —
            // T cells cannot somatically hypermutate; affinity maturation
            // is a B-cell germinal-centre process. The real phenomenon is
            // functional AVIDITY maturation, which rose >50x in vivo with
            // no change in TCR affinity at all.
            //
            // Cap dropped 2 → 1. At 10 bits this takes the match radius
            // from 2 to 3: the rate goes 5.47% → 17.19% AND cross-reactive
            // teaching appears (25.4% of tags then teach two species). A
            // second level would take r to 4, where 100% of tags teach
            // more than one species and specificity collapses entirely.
            new Row("Functional avidity maturation",
                    "A near-miss barcode still teaches — and one meeting can inform two species at once.",
                    "AdaptiveUnitTuning.MatchDistanceBonus: MatchMaxHammingDistance 2 → 3", 45, 1),
            // Sprint 29: was "Rapid recirculation", advertised as a pure
            // win. Nodes refresh because cells LEAVE ALIVE (transit 10–20h),
            // naive T cells are the long-lived compartment, and faster
            // egress measurably REDUCES priming. The copy now states the
            // trade instead of hiding it.
            new Row("Lymph-node egress (S1P)",
                    "Cells cycle out sooner, refreshing the repertoire — but each one has less time to find its match.",
                    "AdaptiveUnitTuning.LymphocyteLifespanReduction: LymphocyteLifespanSeconds 20 → −4/lvl, floor 6", 30, 3),
            // Sprint 29, the Director's memory row.
            new Row("Memory T cells",
                    "The better you already know a pathogen, the more readily a meeting recognises it again.",
                    "AdaptiveUnitTuning.MemoryRecallLevel → +1/lvl (scales with that species' knowledge)", 50, 3),
        };

        // =============================================================
        // Sprint 29: the four towers that shipped with an empty upgrade
        // panel. Every row writes one field that already exists.
        // =============================================================

        private static readonly Row[] CytotoxicT =
        {
            // Granzyme B loading is the real lever on how fast a synapse
            // kills. Deliberately NOT a kill-count row: an in vivo CTL
            // already kills 2–16 targets a day, and the tower is at that
            // ceiling, so raising a kill limit would buy nothing.
            new Row("Granzyme B loading",
                    "Recognises and kills an infected cell faster once it is in contact.",
                    "UnitLifecycleTuning.StressSenseChancePerTick 0.03 → +0.012/lvl", 45, 3),
            new Row("Interstitial patrol",
                    "Covers ground faster between targets.",
                    "UnitLifecycleTuning.FineTilesPerTick 3 → +1/lvl", 40, 2),
            new Row("Immunological synapse reach",
                    "Engages infected cells one tile further out.",
                    "UnitLifecycleTuning.ContactRadiusFineTiles 2 → 3", 45, 1),
        };

        private static readonly Row[] NaturalKiller =
        {
            // Three orthogonal rows: aura RATE, aura RADIUS, own killing.
            // No population row — NK proliferation is unchanged during
            // acute infection, so a clonal-expansion shape would be a lie.
            new Row("IL-15 priming",
                    "Pulses its interferon signal more often.",
                    "UnitLifecycleTuning.IfnGammaAuraChancePerTick 0.02 → +0.008/lvl", 40, 2),
            new Row("IFN-γ diffusion",
                    "The interferon field reaches further — more macrophages caught per pulse.",
                    "UnitLifecycleTuning.IfnGammaAuraCoarseRadius 3 → +1/lvl", 45, 2),
            // Cap 1 on purpose: NKG2D up also means killing stressed-but-
            // UNINFECTED enterocytes, and more than one level would start
            // eroding the ~10% knowledge rung's reason to exist.
            new Row("NKG2D expression",
                    "Senses stressed cells more readily — including some that are merely stressed.",
                    "UnitLifecycleTuning.StressSenseChancePerTick 0.01 → 0.018", 45, 1),
        };

        private static readonly Row[] GammaDeltaT =
        {
            new Row("Occludin-dependent intercalation",
                    "Patrols a wider stretch of wall.",
                    "UnitLifecycleTuning.BarrierPatrolRadiusCells 2.0 → +0.6/lvl", 35, 3),
            new Row("KGF-1 (FGF7) delivery",
                    "Mends dead barrier segments faster.",
                    "UnitLifecycleTuning.BarrierRepairIntervalSeconds 3.5 → −0.55/lvl", 40, 3),
            // The drawback row. Level 1 is physiology; level 2 crosses a
            // cited threshold — villous atrophy needs IL-15 raised in BOTH
            // epithelium and lamina propria, so the second level is exactly
            // where the game should start charging for it.
            new Row("IL-15 trans-presentation",
                    "Holds more γδ cells in the epithelium. Past the first level the wall itself starts to suffer.",
                    "UnitLifecycleTuning.MaxActiveChildren 3 → +1/lvl (level 2 damages the barrier)", 45, 2),
        };

        private static readonly Row[] PlasmaCell =
        {
            // Three rows, three DIFFERENT fields. An earlier draft had two
            // of them cutting the secretion interval, which would have made
            // whichever was bought second silently erase the first --
            // exactly the dead-upgrade failure this sprint exists to fix.
            //
            // Dimeric IgA is the measured ~15x improvement and is the gut's
            // isotype. Affinity maturation against a whole organism, by
            // contrast, shows no further gain after about day 6, so it is
            // deliberately NOT a row here.
            new Row("Polymeric IgA assembly",
                    "Antibody is assembled and exported faster.",
                    "UnitLifecycleTuning.AntibodySecretionIntervalSeconds 1.1 → −0.25/lvl, floor 0.35", 45, 2),
            // Secretory component is the piece of pIgR that stays bound to
            // IgA after transport and protects it from proteolysis in the
            // gut -- the defining feature of the antibody out there.
            new Row("Secretory component",
                    "Each antibody survives longer before it is spent.",
                    "UnitLifecycleTuning.AntibodyLifespanBonusSeconds → +1.5s/lvl", 40, 2),
            // NOT a lifespan row for the CELL: plasma cells have no
            // lifespan field, so one would have been inert on arrival.
            new Row("Eosinophil-supported niche",
                    "Holds more plasma cells in the tissue at once.",
                    "UnitLifecycleTuning.MaxActiveChildren 2 → +1/lvl", 40, 2),
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
                case UnitKind.CytotoxicT: return CytotoxicT;
                case UnitKind.NaturalKiller: return NaturalKiller;
                case UnitKind.GammaDeltaT: return GammaDeltaT;
                case UnitKind.PlasmaCell: return PlasmaCell;
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
                case UnitKind.NaturalKiller: return "IFN-γ · upgrades your macrophages";
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
                case UnitKind.GammaDeltaT: return "γδ T cell (IEL)";
                case UnitKind.PlasmaCell: return "B cell";
                default: return "Helper-T cell";
            }
        }
    }
}
