using UnityEngine;

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

        // -- Sprint 23. GAME_DESIGN.md §4: "T and B cells: expensive
        //    early-to-mid-game towers. Priced deliberately high so the step
        //    from innate to adaptive is felt as an investment decision, not
        //    a natural progression." These sit well above the innate pair
        //    (macrophage 40, neutrophil 15) for that reason.

        /// <summary>The headline adaptive purchase, and the most expensive
        /// thing on the board: it is also the only one that is dead weight
        /// until a species reaches ~10% knowledge, which is the cost the
        /// Director wanted the player to feel.</summary>
        public static int CytotoxicTPrice = 70;

        /// <summary>Cheaper than the CTL and needs no knowledge, but its
        /// output is mostly an IFN-gamma buff rather than kills -- so it is
        /// priced as a force multiplier on macrophages you already own, not
        /// as a killer in its own right.</summary>
        public static int NaturalKillerPrice = 45;

        /// <summary>Tissue-resident wall defence. Needs no priming, so it is
        /// available from round 1 -- the expensive innate answer for a
        /// player who does not want to wait for adaptive immunity.</summary>
        public static int GammaDeltaTPrice = 55;

        /// <summary>The B-cell progenitor (it fields plasma cells). Gated at
        /// the ~20% neutralising-antibody rung.</summary>
        public static int BCellPrice = 65;

        // -- Sprint 27: bone marrow capacity (GAME_DESIGN.md §2a's oldest
        //    open question, Director 2026-10-25) --

        /// <summary>Progenitor niches available at the start of a run.
        /// Four, down from five: the Director lost a run on five and valued
        /// having to think about what to spend ATP on, so the opening keeps
        /// that pressure and the first expansion is a real decision rather
        /// than a bonus.</summary>
        public static int MarrowStartingSlots = 4;

        /// <summary>Hard ceiling on niches. The remaining pads are drawn as
        /// FATTY marrow from the first frame, so the player can see the
        /// unused capacity sitting in the bone before they can afford
        /// it.</summary>
        public static int MarrowMaxSlots = 8;

        /// <summary>ATP for the first reconversion. Deliberately above the
        /// most expensive tower (the CTL at 70): buying *room* for a tower
        /// should never be cheaper than buying the tower.</summary>
        public static int MarrowReconversionBasePrice = 80;

        /// <summary>Each further niche costs
        /// `base * (1 + growth * alreadyBought)`. At 0.75 the four
        /// purchasable niches run 80 / 140 / 200 / 260 -- the last one costs
        /// about as much as three towers, which is what stops "just buy
        /// more slots" from being the answer to every problem.</summary>
        public static float MarrowReconversionGrowth = 0.75f;

        /// <summary>Price of the next niche, given how many have already
        /// been bought beyond the starting allowance.</summary>
        public static int MarrowReconversionPrice(int alreadyBought) =>
            Mathf.Max(1, Mathf.RoundToInt(
                MarrowReconversionBasePrice * (1f + MarrowReconversionGrowth * alreadyBought)));

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
            CytotoxicTPrice = 70;
            NaturalKillerPrice = 45;
            GammaDeltaTPrice = 55;
            BCellPrice = 65;
            MarrowStartingSlots = 4;
            MarrowMaxSlots = 8;
            MarrowReconversionBasePrice = 80;
            MarrowReconversionGrowth = 0.75f;
            StartingLives = 100;
            LifeRegenRounds = 2;
            LifeRegenAmount = 1;
            BatchSizeBase = 16;
            BatchSizeGrowthPerRound = 6;
        }
    }
}
