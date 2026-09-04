namespace ImmunologyTD.Economy
{
    /// <summary>
    /// The purchasable items in the buy-phase shop (Sprint 11). These are
    /// **placeholders** — buying one spends ATP and raises its level in the
    /// <see cref="ShopLedger"/> and does nothing else. The intended
    /// mechanics are recorded in <c>GAME_DESIGN.md</c> (§5b/§6b and the
    /// host-cell-upgrades subsection) for the sprint that builds them.
    /// </summary>
    public enum ShopItem
    {
        /// <summary>§6b mucus-turnover upgrade — raise the barrier shed rate,
        /// flush adherent pathogens back to the lumen.</summary>
        BarrierMucusTurnover,

        /// <summary>Host cells sense a pathogen signature (e.g. dsRNA) and
        /// gain a chance to self-destruct when infected, releasing a strong
        /// DC-specific "eat this debris" cytokine (immunogenic apoptosis).</summary>
        HostDsRnaSensor,

        /// <summary>Host cells harden against a virus getting inside — lower
        /// per-tick viral entry chance.</summary>
        HostReducedViralEntry,

        /// <summary>Host cells take less damage per step from a grazing
        /// large bacterium.</summary>
        HostBacterialResistance,

        /// <summary>A stem-cell niche — tissue near a crypt regrows faster
        /// (§6's crypt-based recovery). Repeatable count.</summary>
        Crypt,

        /// <summary>Sprint 12: sharpens cytokine sensing for every unit
        /// (rung 2 of the search ladder). **This one is a real effect** —
        /// it raises <c>Chemotaxis.SensingUpgradeLevel</c>. Repeatable.</summary>
        CytokineSensingUpgrade,
    }

    /// <summary>
    /// Every number the shop runs on, in one place — mutable statics with
    /// <see cref="ResetToDefaults"/>, the same pattern as
    /// <c>EconomyTuning</c> / <c>InvasionTuning</c>. **All placeholder**
    /// (Sprint 11 is a framework pass: the shop exists, the purchases do
    /// nothing yet).
    /// </summary>
    public static class ShopTuning
    {
        /// <summary>Base ATP price of the first level of each item.</summary>
        public static int BarrierMucusTurnoverBasePrice = 30;
        public static int HostDsRnaSensorBasePrice = 45;
        public static int HostReducedViralEntryBasePrice = 40;
        public static int HostBacterialResistanceBasePrice = 40;
        public static int CryptBasePrice = 25;
        public static int CytokineSensingUpgradeBasePrice = 35;

        /// <summary>Each subsequent level of the same item costs
        /// <c>basePrice * (1 + PriceGrowthPerLevel * currentLevel)</c>.</summary>
        public static float PriceGrowthPerLevel = 0.6f;

        /// <summary>Price of a per-tower progenitor upgrade at
        /// <paramref name="currentLevel"/> (0-based).</summary>
        public static int ProgenitorUpgradeBasePrice = 35;

        public static int BasePriceFor(ShopItem item)
        {
            switch (item)
            {
                case ShopItem.BarrierMucusTurnover: return BarrierMucusTurnoverBasePrice;
                case ShopItem.HostDsRnaSensor: return HostDsRnaSensorBasePrice;
                case ShopItem.HostReducedViralEntry: return HostReducedViralEntryBasePrice;
                case ShopItem.HostBacterialResistance: return HostBacterialResistanceBasePrice;
                case ShopItem.CytokineSensingUpgrade: return CytokineSensingUpgradeBasePrice;
                default: return CryptBasePrice;
            }
        }

        /// <summary>ATP price of the next level of <paramref name="item"/>,
        /// given how many are already owned.</summary>
        public static int PriceFor(ShopItem item, int currentLevel)
        {
            int b = BasePriceFor(item);
            int lvl = currentLevel < 0 ? 0 : currentLevel;
            return Mathf_RoundToInt(b * (1f + PriceGrowthPerLevel * lvl));
        }

        public static int ProgenitorUpgradePrice(int currentLevel) =>
            ProgenitorUpgradePrice(ProgenitorUpgradeBasePrice, currentLevel);

        /// <summary>Sprint 16: the same curve, but with the base price
        /// supplied per row. Sprint 11 had one unnamed upgrade per tower at
        /// one flat base price; the roster
        /// (<c>UI.ProgenitorUpgradeCatalog</c>) gives each row its own base,
        /// because "pseudopod reach" and "extended lifespan" should not cost
        /// the same just because they hang off the same niche.</summary>
        public static int ProgenitorUpgradePrice(int basePrice, int currentLevel)
        {
            int lvl = currentLevel < 0 ? 0 : currentLevel;
            return Mathf_RoundToInt(basePrice * (1f + PriceGrowthPerLevel * lvl));
        }

        // Tiny local rounder so this file has no UnityEngine dependency —
        // it's pure economy data, like EconomyTuning.
        private static int Mathf_RoundToInt(float v) => (int)System.Math.Round(v, System.MidpointRounding.AwayFromZero);

        public static void ResetToDefaults()
        {
            BarrierMucusTurnoverBasePrice = 30;
            HostDsRnaSensorBasePrice = 45;
            HostReducedViralEntryBasePrice = 40;
            HostBacterialResistanceBasePrice = 40;
            CryptBasePrice = 25;
            CytokineSensingUpgradeBasePrice = 35;
            PriceGrowthPerLevel = 0.6f;
            ProgenitorUpgradeBasePrice = 35;
        }
    }
}
