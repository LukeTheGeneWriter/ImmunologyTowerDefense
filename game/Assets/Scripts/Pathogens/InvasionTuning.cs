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
        /// wall (lumen depth 0). **Sprint 9 raised this 0.12 -> 0.30**
        /// (Director, 2026-08-29): at 0.12 a round could pass with almost
        /// everything excreted harmlessly and "nothing happened." Combined
        /// with the food item dropping its cargo *already at the wall*
        /// (see the FoodItem* fields below), most of a batch now sticks and
        /// piles up. Placeholder, not a balance pass.</summary>
        public static float AdhesionChanceAtWall = 0.30f;

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

        // -- The wall: the mill, the crowd field, and plaques --
        // Sprint 22 (INVASION_REWORK_SPEC.md) replaced the per-position
        // breach roll. BreachRollIntervalSeconds and PerPathogenBreachChance
        // are GONE: breach is no longer per lane and no longer a hidden
        // memoryless roll. Pressure is now a visible CHARGE on a plaque,
        // placed by where pathogens actually crowd.

        /// <summary>Depth of the mucus band the adhered population mills in,
        /// in coarse cells out from the seam. Deliberately equal to the band
        /// LumenChannelRenderer already draws, so "in the mill" and "in the
        /// drawn glaze" are the same region.</summary>
        public static float MucusBandDepthCells = 1.6f;

        /// <summary>Within this depth a pathogen is GRIPPING: it counts
        /// toward crowding, can be a plaque member, and can damage a barrier
        /// segment. Beyond it (out to MucusBandDepthCells) it is loose --
        /// visible, still on the wall, counting for nothing. Half a cell, so
        /// a gripping pathogen visibly touches the wall.</summary>
        public static float GripDepthCells = 0.55f;

        /// <summary>Radius, in coarse cells, of the triangular kernel the
        /// crowd field is built with. 1.5 cells is ~111px on Map 01 -- a
        /// "stretch of wall" the eye reads as one spot, about three lanes.
        /// This is the number that makes ingress PROXIMITY-based rather than
        /// lane-based: it is the width of "near each other."</summary>
        public static float CrowdRadiusCells = 1.5f;

        /// <summary>Crowd-field resolution. 4 buckets per coarse cell is
        /// 18.5px on Map 01; finer is wasted at this board scale. The field
        /// is 1-D -- depth is a gate (gripping or not), not a dimension of
        /// the statistic.</summary>
        public static int CrowdBucketsPerCell = 4;

        /// <summary>Crowd value a bucket must reach before it seeds a
        /// plaque. Just above IngressCrowdFloor, so a plaque becomes visible
        /// slightly BEFORE it can charge -- the player gets a beat of warning
        /// that is honest rather than decorative.</summary>
        public static float PlaqueSeedCrowd = 2.5f;

        /// <summary>Minimum separation between plaque centres, in cells.
        /// Equal to the footprint, so plaques tile the wall instead of
        /// stacking on one hot spot.</summary>
        public static float PlaqueMinSeparationCells = 1.5f;

        /// <summary>Hard cap on live plaques. The 738px wall holds about six
        /// non-overlapping footprints; eight is slack. Fixed-size and pooled
        /// -- nothing here allocates per event (CLAUDE.md's pooling rule).
        /// </summary>
        public static int MaxPlaques = 8;

        /// <summary>**The minimum infectious dose, and the single most
        /// important number in the invasion model.** A plaque only charges
        /// while its membership EXCEEDS this, so one or two adherents never
        /// charge anything: they sit, mill, erode a little barrier, and creep
        /// away downstream.
        ///
        /// Mechanically this is what forbids the trickle. Ingress is not
        /// something a pathogen does, it is something a CROWD does -- which
        /// is the property GAME_DESIGN.md section 1b step 2 exists to
        /// protect, carried over from the per-position model intact.
        ///
        /// Biologically it is the infectious-dose threshold, which is real
        /// and famously species-specific (tens of organisms for Shigella,
        /// ~10^5 for Salmonella), so this is also the knob that will later
        /// tell one species from another.</summary>
        public static float IngressCrowdFloor = 2.0f;

        /// <summary>Charge added per second per plaque member above the
        /// floor. This is Sprint 4's PerPathogenBreachChance (0.012) scaled
        /// by 1.5 to compensate for the floor subtracting two members, so a
        /// LOADED spot still ruptures in 5-10s and round pacing does not
        /// move. What changed is the bottom of the curve: a lone adherent
        /// used to leak in after ~80s and now never gets in at all.
        /// Placeholder like everything here -- the headless strategy harness
        /// (SESSION_HANDOFF.md section 2a) is what should eventually set
        /// it.</summary>
        public static float ChargeRatePerCrowdUnit = 0.018f;

        /// <summary>How fast an abandoned plaque's charge drains. ~25s from
        /// full to empty: slow enough that relieving a spot reads as relief
        /// rather than as the threat evaporating.</summary>
        public static float ChargeDecayPerSecond = 0.04f;

        /// <summary>Fractional jitter on a plaque's rupture threshold, drawn
        /// once when it seeds (so threshold is uniform in 1 +/- this).
        ///
        /// A charge is honest -- what is drawn IS the state -- but a
        /// perfectly deterministic fill would make a rupture predictable to
        /// the second. +/-15% keeps "it is nearly due" true while keeping
        /// "exactly when" unknowable.</summary>
        public static float ChargeThresholdJitter = 0.15f;

        /// <summary>How fast a plaque centre chases its membership's
        /// centroid, per second. A plaque follows a swarm drifting
        /// downstream within about half a second instead of being left
        /// behind by it.</summary>
        public static float PlaqueCentreLerpPerSecond = 2.0f;

        /// <summary>Radius, in cells, of what a rupture releases and what it
        /// kills. Equal to CrowdRadiusCells on purpose: what gathered is
        /// exactly what goes in.</summary>
        public static float BurstFootprintCells = 1.5f;

        // -- The mill (INVASION_REWORK_SPEC.md section 2) --

        /// <summary>How fast the mucus layer shears adherents downstream, in
        /// cells per second, BEFORE peristalsis modulation. At 0.035 a
        /// pathogen takes ~285s to travel the whole wall and rejoin the flow
        /// at the downstream end -- so the barrier population still
        /// accumulates across rounds (GAME_DESIGN.md section 6b) without
        /// being immortal.
        ///
        /// Multiplied by Peristalsis.SpeedFactor, which the bolus and the
        /// drawn wall squeeze already use, so the WHOLE adhered population
        /// lurches downstream in time with the contraction the player can
        /// already see. That shared modulation is what makes the mill read
        /// as "stuck in moving slime" rather than as agents twitching
        /// independently.</summary>
        public static float MucusCreepCellsPerSecond = 0.035f;

        /// <summary>How fast a pathogen drifts toward the nearest plaque
        /// centre (or the best crowd bucket in reach). One 1.5 cells from a
        /// forming clump joins it in ~12s.
        ///
        /// Biologically this stands in for co-aggregation on the glycocalyx
        /// -- colonisation genuinely is a social process. Mechanically it is
        /// what turns four scattered bolus clumps into ONE dangerous spot
        /// over a round, which is where the drama comes from.</summary>
        public static float CohesionSpeedCellsPerSecond = 0.12f;

        /// <summary>How far a pathogen looks for something to drift toward.
        /// </summary>
        public static float CohesionReachCells = 2.0f;

        /// <summary>Depth a pathogen settles toward once it joins the mill,
        /// and how fast it gets there. It ARRIVES at the outer mucus and
        /// settles in over ~2s: a pathogen never jumps to the wall, because
        /// the old snap-to-seam is exactly the "orderly" artefact the
        /// Director objected to.</summary>
        public static float SettleDepthCells = 0.25f;
        public static float SettleSpeedCellsPerSecond = 0.5f;
        public static float MillArrivalDepthCells = 1.3f;

        /// <summary>How far into the tissue, along the threat axis, a
        /// breach may look for a free slot when the first tissue layer is
        /// already occupied. Kept small so a burst still reads as "at the
        /// wall."</summary>
        public static int MaxReleaseAxisDepth = 3;

        /// <summary>How far along the lanes a rupture may look for a free
        /// slot. **Sprint 22 dropped this 20 -> 4.** The old value was
        /// compensating for a ten-deep pile in a SINGLE lane, which had to
        /// be fanned across half the board on release. A plaque is already
        /// spread across ~3 lanes by its own geometry, and each member asks
        /// for the cell behind its own position first, so the burst arrives
        /// pre-spread and only needs a little slack.</summary>
        public static int MaxReleaseCrossSpread = 4;

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

        // -- Class-specific advance (SPRINT_PLAN.md Sprint 5 item 5) --
        // GAME_DESIGN.md section 1b step 4 was deferred out of Sprint 4
        // because it needs host states. These are the numbers it needs.

        /// <summary>How long a FREE virus particle -- one on the occupant
        /// layer, between hosts -- survives without finding a healthy cell
        /// before it dies. This is half of the firebreak: a virus can only
        /// move into a `Healthy` neighbour (the other half), so a virus
        /// surrounded by dead ground cannot move at all and this timer is
        /// what finishes it off.
        ///
        /// 6s is a judgment call: at the 1s tissue step it gives a virus
        /// about six chances to find a host, which is enough that a breach
        /// landing next to intact tissue reliably takes hold, and short
        /// enough that a viral front stalling against a dead band visibly
        /// dies out inside a playtest rather than sitting there.
        ///
        /// **Do not implement the firebreak as a check.** It is emergent
        /// from this value plus TissueGrid.IsHealthyHost, exactly as
        /// GAME_DESIGN.md section 1b step 4 requires.</summary>
        public static float VirusFreeSurvivalSeconds = 6f;

        /// <summary>Per-tick chance that a FREE virion sitting on a `Healthy`
        /// host cell gets inside it (GAME_DESIGN.md §4b -- "a random virus
        /// entry number each tick"). Below 1 so a virion visibly lingers and
        /// walks a little before establishing, rather than snapping into the
        /// first cell it touches. Judgment call.</summary>
        public static float VirusEntryChancePerTick = 0.20f;

        /// <summary>Fraction of virus spawns that are a BUDDING species
        /// rather than a contact-chain one (GAME_DESIGN.md §4b). A budding
        /// infection emits free virions on a timer and grows as a disk; a
        /// chain infection infects one neighbour once and snakes. Per-spawn
        /// roll -- there is no species roster yet.</summary>
        public static float VirusBuddingSpeciesChance = 0.5f;

        /// <summary>Seconds between a budding infected cell emitting a free
        /// virion. Each virion then walks (momentum-biased, Healthy cells
        /// only) and rolls VirusEntryChancePerTick to establish.</summary>
        public static float VirusBuddingIntervalSeconds = 2.5f;

        /// <summary>Fraction of viral infections that spontaneously burn out
        /// -- the cell exhausts and dies loud on its own, spilling the virus
        /// back out as a free virion plus debris, with no immune action
        /// (GAME_DESIGN.md §4b). Rolled once when the infection establishes.</summary>
        public static float VirusBurnoutChance = 0.30f;

        /// <summary>Range, in seconds after establishing, over which a
        /// burn-out fires. Wide so burn-outs are scattered rather than
        /// synchronised.</summary>
        public static float VirusBurnoutMinSeconds = 8f;
        public static float VirusBurnoutMaxSeconds = 25f;

        /// <summary>Per-step chance that an intracellular bacterium standing
        /// on a healthy host cell goes inside it (GAME_DESIGN.md §4b:
        /// "vulnerable when out, protected when in"). **Sprint 6 lowered
        /// this from 0.5 to 0.12** on the Director's note that these should
        /// roam more -- they have the chassis to survive extracellularly,
        /// unlike a virus -- so the exposed window where they can be killed
        /// cheaply is longer and more visible.</summary>
        public static float IntracellularEntryChance = 0.12f;

        /// <summary>Seconds between an intracellular bacterium's replication
        /// events while inside a host cell (GAME_DESIGN.md §4b). Each event
        /// drains <see cref="IntracellularDrainPerReplication"/> from the
        /// host cell and adds one to the brood that bursts out when the cell
        /// dies. **No voluntary exit** -- Sprint 5's residence timer is
        /// gone; the bacterium leaves only when the cell dies (from the
        /// drain, or from a stress-sense / collateral kill, which releases
        /// nothing).</summary>
        public static float IntracellularReplicationIntervalSeconds = 3f;

        /// <summary>Host-cell health drained per replication event. Against
        /// TissueTuning.HostCellMaxHealth of 10 and a 3s interval, an
        /// unmolested infection kills its host in ~12s and bursts a brood of
        /// ~4. Judgment call, mechanics-first.</summary>
        public static float IntracellularDrainPerReplication = 2.5f;

        /// <summary>Hard cap on how many bacteria a single drained cell
        /// releases, however long the infection ran. Keeps a long-lived
        /// infection from dumping an unbounded brood.</summary>
        public static int IntracellularMaxBrood = 6;

        /// <summary>Damage a LARGE bacterium does to the host cell it is
        /// <summary>Sprint 9 (Director, 2026-08-29): the contaminated food
        /// item. It enters the lumen at the upstream end, drifts the full
        /// width of the channel over <see cref="FoodItemTransitSeconds"/>,
        /// and releases the round's batch in <see cref="FoodItemBurstCount"/>
        /// evenly-spaced bursts as it travels. Each burst drops its
        /// pathogens at lumen cells within <see cref="FoodItemWallHugDepth"/>
        /// of the wall, near the food's current position -- so they adhere
        /// instead of washing through. A pure delivery vehicle this pass:
        /// not attackable. All placeholder.</summary>
        public static float FoodItemTransitSeconds = 30f;
        public static int FoodItemBurstCount = 4;
        public static int FoodItemWallHugDepth = 1;

        /// <summary>Damage a LARGE bacterium does to the host cell it is
        /// standing on, per tissue step, as it grazes its way toward the
        /// base. Against TissueTuning.HostCellMaxHealth of 10 that is four
        /// steps on one cell to kill it.
        ///
        /// GAME_DESIGN.md section 4a says a large bacterium "kills and
        /// directly occupies one coarse slot"; section 1c's two-layer model
        /// says it passes over ground that still holds living cells. This
        /// number is how those are reconciled -- it kills, but not
        /// instantly, so a bacterium that keeps moving leaves damaged tissue
        /// rather than a scorched trail, while one that gets held up at a
        /// contested line kills what it is standing on. Set it to 0 to
        /// restore Sprint 4's exact behaviour (a bacterium that never
        /// touches the host layer at all).</summary>
        public static float LargeBacteriumHostDamagePerStep = 2.5f;

        /// <summary>Restores every value above. Called by harnesses that
        /// override a value for one group of assertions so they cannot leak
        /// into the next group.</summary>
        public static void ResetToDefaults()
        {
            LumenStepIntervalSeconds = 0.35f;
            AdhesionChanceAtWall = 0.30f;
            AdhesionFalloffCells = 5f;
            FoodItemTransitSeconds = 30f;
            FoodItemBurstCount = 4;
            FoodItemWallHugDepth = 1;
            MucusBandDepthCells = 1.6f;
            GripDepthCells = 0.55f;
            CrowdRadiusCells = 1.5f;
            CrowdBucketsPerCell = 4;
            PlaqueSeedCrowd = 2.5f;
            PlaqueMinSeparationCells = 1.5f;
            MaxPlaques = 8;
            IngressCrowdFloor = 2.0f;
            ChargeRatePerCrowdUnit = 0.018f;
            ChargeDecayPerSecond = 0.04f;
            ChargeThresholdJitter = 0.15f;
            PlaqueCentreLerpPerSecond = 2.0f;
            BurstFootprintCells = 1.5f;
            MucusCreepCellsPerSecond = 0.035f;
            CohesionSpeedCellsPerSecond = 0.12f;
            CohesionReachCells = 2.0f;
            SettleDepthCells = 0.25f;
            SettleSpeedCellsPerSecond = 0.5f;
            MillArrivalDepthCells = 1.3f;
            MaxReleaseAxisDepth = 3;
            MaxReleaseCrossSpread = 4;
            TissueStepIntervalSeconds = 1f;
            AdvanceBaseWeight = 0.70f;
            AdvanceLateralWeight = 0.13f;
            AdvanceAwayWeight = 0.04f;
            VirusFreeSurvivalSeconds = 6f;
            VirusEntryChancePerTick = 0.20f;
            VirusBuddingSpeciesChance = 0.5f;
            VirusBuddingIntervalSeconds = 2.5f;
            VirusBurnoutChance = 0.30f;
            VirusBurnoutMinSeconds = 8f;
            VirusBurnoutMaxSeconds = 25f;
            IntracellularEntryChance = 0.12f;
            IntracellularReplicationIntervalSeconds = 3f;
            IntracellularDrainPerReplication = 2.5f;
            IntracellularMaxBrood = 6;
            LargeBacteriumHostDamagePerStep = 2.5f;
        }
    }
}
