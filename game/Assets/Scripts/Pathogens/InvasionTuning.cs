namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Every number Sprint 4's invasion loop (GAME_DESIGN.md section 1b)
    /// runs on, in one place.
    ///
    /// **All mutable static fields, none of them `const`** -- the same
    /// reasoning as Chemotaxis.GradientSharpness and UnitLifecycleTuning:
    /// SPRINT_PLAN.md items 5/6/7 each say "tunable, not a const," and a
    /// headless harness needs to be able to override a value for a sweep or
    /// to force a deterministic outcome. Nothing here is balance-tested --
    /// the Director's standing instruction for Sprint 4 is mechanics first,
    /// and every value below was chosen for LEGIBILITY inside a short
    /// playtest. See docs/TEAM_RETRO.md for the derivation of each.
    ///
    /// Static rather than per-instance because these are map/simulation
    /// constants, not per-tower upgrade targets. If a later sprint sells
    /// "harden the gut wall" as an upgrade, breach numbers should move onto
    /// a per-position instance the way UnitLifecycleTuning moved onto a
    /// per-tower one.
    /// </summary>
    public static class InvasionTuning
    {
        // -- Lumen flow (SPRINT_PLAN.md item 4) --

        /// <summary>Seconds per one-cell step down the lumen channel. 0.35s
        /// x 40 lanes = ~14s for a full transit, which is long enough that
        /// the Director can watch a pathogen ride the flow and short enough
        /// that a wave resolves inside a playtest.</summary>
        public static float LumenStepIntervalSeconds = 0.35f;

        // -- Proximity-gated adhesion (SPRINT_PLAN.md item 5) --

        /// <summary>Per-step adhesion chance for a pathogen hugging the gut
        /// wall (lumen depth 0).</summary>
        public static float AdhesionChanceAtWall = 0.12f;

        /// <summary>Exponential falloff length, in coarse cells, of adhesion
        /// chance with distance from the wall:
        /// `p(d) = AdhesionChanceAtWall * exp(-d / AdhesionFalloffCells)`.
        ///
        /// Exponential rather than linear because the mechanic wants a sharp
        /// "lane matters" signal, not a gentle ramp: at these values a
        /// wall-hugging pathogen adheres with ~99% probability over a full
        /// descent, one 10 cells out with ~48%, and one on the far side of
        /// the channel with ~4%. Monotonic by construction, which is what
        /// Assets/Editor/MapVerification.cs asserts statistically.
        ///
        /// AdhesionChanceAtWall is deliberately NOT near 1.0: a very high
        /// wall chance makes every near-wall pathogen adhere in the first
        /// two or three lanes, which piles the entire invasion at the top of
        /// the map. 0.12 spreads adhesion over roughly the first two thirds
        /// of the descent instead.</summary>
        public static float AdhesionFalloffCells = 5f;

        // -- Per-position breach (SPRINT_PLAN.md item 6) --

        /// <summary>How often each boundary position rolls for a breach.
        /// GAME_DESIGN.md section 1b step 2 explicitly permits rolling less
        /// often with a correspondingly larger chance, and calls it the
        /// cheaper implementation across 40 positions -- 1 roll/second
        /// across 40 positions is nothing, and it makes the numbers below
        /// readable as "per second."</summary>
        public static float BreachRollIntervalSeconds = 1f;

        /// <summary>Each adhered pathogen's independent contribution to its
        /// position's breach chance, per roll. The position's chance is
        /// `1 - (1 - PerPathogenBreachChance)^n`, so it RISES WITH PRESSURE
        /// -- 1 pathogen ~1.2%/s (a breach in ~80s), 5 ~5.9%/s (~17s), 10
        /// ~11.4%/s (~9s).
        ///
        /// The count-dependence is not decoration. With a flat per-position
        /// chance, a position holding one pathogen breaches as often as one
        /// holding ten, so nothing ever accumulates and the "pressure builds
        /// visibly, then bursts" shape GAME_DESIGN.md section 1b step 2
        /// insists on cannot happen. Note this is still ONE roll per
        /// position -- when it trips, EVERY pathogen there is released at
        /// once. It is emphatically not a per-pathogen roll, which would
        /// produce the trickle SPRINT_PLAN.md item 6 warns against.</summary>
        public static float PerPathogenBreachChance = 0.012f;

        /// <summary>How far into the tissue, along the threat axis, a
        /// breach may look for a free slot when the first tissue layer is
        /// already occupied. Kept small so a burst still reads as "at the
        /// wall."</summary>
        public static int MaxReleaseAxisDepth = 3;

        /// <summary>How far along the lanes a breach may look for a free
        /// slot. Generous, because a burst of ten pathogens must land
        /// somewhere -- it fans out along the wall rather than punching
        /// deep.</summary>
        public static int MaxReleaseCrossSpread = 20;

        // -- Base-directed advance (SPRINT_PLAN.md item 7) --

        /// <summary>Seconds per one-cell advance step in tissue. Much slower
        /// than a unit's 0.12s tick on purpose: the tissue band is 50 cells
        /// wide, so even a perfectly-directed pathogen needs ~50s to cross
        /// it, and a biased random walk needs closer to 100s. That is what
        /// makes it a front that can be held rather than a sprint for the
        /// endzone.</summary>
        public static float TissueStepIntervalSeconds = 1f;

        /// <summary>Relative weight of the one step that moves TOWARD THE
        /// BASE. "Strongly biased random walk" (GAME_DESIGN.md section 1b
        /// step 3) -- with the three weights below, ~70% of steps advance,
        /// ~26% slide sideways along the front, ~4% fall back.</summary>
        public static float AdvanceBaseWeight = 0.70f;

        /// <summary>Relative weight of each of the two sideways steps
        /// (perpendicular to the threat axis). Non-zero so a blocked front
        /// spreads laterally instead of jamming.</summary>
        public static float AdvanceLateralWeight = 0.13f;

        /// <summary>Relative weight of the step AWAY from the base. Small
        /// but non-zero -- a pure one-way walk reads as scripted movement
        /// rather than as a random walk.</summary>
        public static float AdvanceAwayWeight = 0.04f;

        /// <summary>Restores every value above. Called by harnesses that
        /// override a value for one group of assertions so they cannot leak
        /// into the next group.</summary>
        public static void ResetToDefaults()
        {
            LumenStepIntervalSeconds = 0.35f;
            AdhesionChanceAtWall = 0.12f;
            AdhesionFalloffCells = 5f;
            BreachRollIntervalSeconds = 1f;
            PerPathogenBreachChance = 0.012f;
            MaxReleaseAxisDepth = 3;
            MaxReleaseCrossSpread = 20;
            TissueStepIntervalSeconds = 1f;
            AdvanceBaseWeight = 0.70f;
            AdvanceLateralWeight = 0.13f;
            AdvanceAwayWeight = 0.04f;
        }
    }
}
