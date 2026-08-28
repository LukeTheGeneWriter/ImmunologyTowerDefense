namespace ImmunologyTD.Economy
{
    /// <summary>
    /// The player's ATP balance. A plain reference type (not a
    /// MonoBehaviour, not statics), constructed by <c>GameBootstrap</c> and
    /// passed by reference to whoever earns or spends -- the same shape as
    /// <c>InvasionTally</c> / <c>TissueGrid</c>, so a headless harness makes
    /// one, hands it to the real classes, and reads it back with no
    /// GameObjects.
    ///
    /// Income (GAME_DESIGN.md §5b): a round-start lump sum (<see cref="Grant"/>
    /// from <c>RoundController</c> on a round clearing) and a flat amount per
    /// kill (<see cref="Grant"/> via <c>EconomyHooks.PayForKill</c> from
    /// <c>SearchUnit.RegisterKill</c>). Spending: tower placement
    /// (<see cref="TrySpend"/> from <c>BoneMarrowManager.PlaceTower</c>).
    /// </summary>
    public class AtpWallet
    {
        public int Balance { get; private set; }

        /// <summary>Total granted over the wallet's life -- diagnostics /
        /// HUD only, never spent from.</summary>
        public int LifetimeEarned { get; private set; }

        public AtpWallet(int startingBalance)
        {
            Balance = startingBalance < 0 ? 0 : startingBalance;
            LifetimeEarned = Balance;
        }

        public bool CanAfford(int cost) => cost <= 0 || Balance >= cost;

        /// <summary>Deducts <paramref name="cost"/> if affordable. Returns
        /// whether it spent. A non-positive cost always succeeds and changes
        /// nothing.</summary>
        public bool TrySpend(int cost)
        {
            if (cost <= 0) return true;
            if (Balance < cost) return false;
            Balance -= cost;
            return true;
        }

        public void Grant(int amount)
        {
            if (amount <= 0) return;
            Balance += amount;
            LifetimeEarned += amount;
        }

        /// <summary>Hard reset to a known balance -- for a run restart or a
        /// harness reusing a wallet across scenarios.</summary>
        public void Reset(int toBalance)
        {
            Balance = toBalance < 0 ? 0 : toBalance;
            LifetimeEarned = Balance;
        }
    }
}
