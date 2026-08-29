using System;

namespace ImmunologyTD.Economy
{
    /// <summary>
    /// What the player has bought in the buy-phase shop, per run (Sprint 11).
    ///
    /// **Placeholder framework.** <see cref="TryBuy"/> spends ATP and raises
    /// the item's level; that is the entire effect. Nothing reads
    /// <see cref="LevelOf"/> to change the simulation yet — the mechanics
    /// sprint wires those. Plain reference type, constructed by
    /// <c>GameBootstrap</c> and passed by reference, same shape as
    /// <c>AtpWallet</c> / <c>KnowledgeLedger</c>.
    /// </summary>
    public class ShopLedger
    {
        // Indexed by (int)ShopItem.
        private readonly int[] levels = new int[Enum.GetValues(typeof(ShopItem)).Length];

        /// <summary>Bumped on every purchase, so a poller (the HUD) can tell
        /// cheaply whether anything changed.</summary>
        public int Revision { get; private set; }

        /// <summary>How many levels of <paramref name="item"/> are owned (0 =
        /// not bought).</summary>
        public int LevelOf(ShopItem item) => levels[(int)item];

        public bool Owns(ShopItem item) => levels[(int)item] > 0;

        /// <summary>Price of the next level of <paramref name="item"/>.</summary>
        public int NextPrice(ShopItem item) => ShopTuning.PriceFor(item, levels[(int)item]);

        public bool CanBuy(ShopItem item, AtpWallet wallet) =>
            wallet != null && wallet.CanAfford(NextPrice(item));

        /// <summary>Spends the next level's price from <paramref name="wallet"/>
        /// and increments the level. False (and no change) if unaffordable or
        /// the wallet is null. **No side effect beyond the ledger + wallet.**</summary>
        public bool TryBuy(ShopItem item, AtpWallet wallet)
        {
            if (wallet == null) return false;
            if (!wallet.TrySpend(NextPrice(item))) return false;
            levels[(int)item]++;
            Revision++;
            return true;
        }

        public void Reset()
        {
            for (int i = 0; i < levels.Length; i++) levels[i] = 0;
            Revision++;
        }
    }
}
