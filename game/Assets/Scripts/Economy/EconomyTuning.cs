namespace ImmunologyTD.Economy
{
    /// <summary>
    /// Every number in the ATP economy and the round loop, in one place.
    /// Mutable statics with <see cref="ResetToDefaults"/>, the same pattern
    /// as <c>InvasionTuning</c> / <c>TissueTuning</c> -- a harness overrides
    /// a value for one scenario and resets after, and a future tuning pass
    /// is a set of field writes, not a code change.
    ///
    /// **Every value here is a placeholder** (Director, 2026-08-28: build
    /// the framework, numbers deliberately wrong). GAME_DESIGN.md §5b/§5d/§6c
    /// are the design; the intent of these defaults is only "the loop is
    /// legible", not "the loop is balanced".
    /// </summary>
    public static class EconomyTuning
    {
        // -- ATP --

        /// <summary>ATP the player has when the game opens, before round 1.
        /// Effectively round 1's buy budget (the round-start lump is granted
        /// on each round CLEAR, not on start -- see §5d).</summary>
        public static int StartingAtp = 100;

        /// <summary>Granted when a round clears, framed as the budget for
        /// starting the next one -- the "+N ATP" the player sees between
        /// rounds (GAME_DESIGN.md §5b).</summary>
        public static int RoundStartLumpSum = 80;

        /// <summary>Flat ATP per pathogen a unit kills. Routed through
        /// SearchUnit.RegisterKill, so it covers contact kills and §4b
        /// stress-sense kills but not brood-burst / burn-out / drain-death
        /// (not the player's kills).</summary>
        public static int AtpPerKill = 3;

        // -- Tower prices (GAME_DESIGN.md §2a: bone-marrow real estate is
        //    the constraint; price is what makes the buy phase a decision) --

        public static int MacrophagePrice = 40;
        public static int NeutrophilPrice = 15;

        /// <summary>Sprint 8: the two adaptive progenitors. Placeholder --
        /// they share the 5 marrow slots with the innate towers, so price is
        /// only half the cost; the real cost is the slot (GAME_DESIGN.md
        /// §1c/§2a).</summary>
        public static int DendriticCellPrice = 30;
        public static int HelperTPrice = 25;

        // -- Lives (GAME_DESIGN.md §6c) --

        public static int StartingLives = 100;

        /// <summary>A life regenerates (`LifeRegenAmount`) every this many
        /// cleared rounds -- convalescence, §6c.</summary>
        public static int LifeRegenRounds = 2;
        public static int LifeRegenAmount = 1;

        // -- Round batch (GAME_DESIGN.md §5d) --

        /// <summary>Round N's batch is <c>BatchSizeBase + (N-1) *
        /// BatchSizeGrowthPerRound</c> pathogens. Linear growth only --
        /// real per-round composition is a light per-class mix in
        /// <c>RoundScript</c>; a real difficulty curve is still out of scope.
        /// **Sprint 9 doubled these (8/3 -> 16/6)** on the Director's note
        /// that rounds were too easy. Round 1 = 16, round 5 = 40. Still a
        /// placeholder.</summary>
        public static int BatchSizeBase = 16;
        public static int BatchSizeGrowthPerRound = 6;

        /// <summary>Batch size for round <paramref name="roundNumber"/>
        /// (1-based). Clamped at 1.</summary>
        public static int BatchSizeForRound(int roundNumber)
        {
            int n = roundNumber < 1 ? 1 : roundNumber;
            int size = BatchSizeBase + (n - 1) * BatchSizeGrowthPerRound;
            return size < 1 ? 1 : size;
        }

        public static void ResetToDefaults()
        {
            StartingAtp = 100;
            RoundStartLumpSum = 80;
            AtpPerKill = 3;
            MacrophagePrice = 40;
            NeutrophilPrice = 15;
            DendriticCellPrice = 30;
            HelperTPrice = 25;
            StartingLives = 100;
            LifeRegenRounds = 2;
            LifeRegenAmount = 1;
            BatchSizeBase = 16;
            BatchSizeGrowthPerRound = 6;
        }
    }
}
