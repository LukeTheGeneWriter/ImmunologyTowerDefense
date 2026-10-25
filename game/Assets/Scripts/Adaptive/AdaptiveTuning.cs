using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Every number in the dendritic-cell shuttle and the antigen barcode,
    /// in one place. Mutable statics with <see cref="ResetToDefaults"/>, the
    /// same pattern as <c>InvasionTuning</c> / <c>EconomyTuning</c> /
    /// <c>TissueTuning</c> -- a harness overrides a value for one scenario
    /// and resets after, and a future tuning pass is a set of field writes.
    ///
    /// **Every value here is a placeholder** (Sprint 8 is a framework pass,
    /// same standard as Sprints 6-7). GAME_DESIGN.md §5a (the shuttle loop)
    /// and §5c (the 8-bit barcode) are the design; the intent of these
    /// defaults is only "the loop is legible on screen", not "the loop is
    /// balanced".
    /// </summary>
    public static class AdaptiveTuning
    {
        // -- The barcode (GAME_DESIGN.md §5c) --

        /// <summary>
        /// Barcode length. **Sprint 29: 8 -> 10 bits** (Director).
        ///
        /// Eight bits could not hold the roster honestly. For a helper-T tag
        /// within radius r of one species to be within r of AT MOST one,
        /// species tags must be at least 2r+1 apart -- 5 at the shipped
        /// r=2. An exhaustive search of the 8-bit space finds at most FOUR
        /// tags that far apart, and there are six species, so the property
        /// the code claimed was not merely unmet but unreachable. At 8 bits
        /// 21.9% of helper-T tags were teaching two species by accident.
        ///
        /// Ten bits fits all six at a minimum distance of SIX, so at r=2
        /// cross-teaching is exactly zero and cross-reactivity becomes a
        /// thing the player can buy (the avidity row takes r to 3) rather
        /// than an accident of tag choice.
        /// </summary>
        public const int BarcodeBits = 10;

        /// <summary>
        /// A DC:helper-T pairing teaches iff the Hamming distance between
        /// the two tags is at most this.
        ///
        /// At 10 bits, r=2 means 56/1024 ≈ **5.47%** of random pairings
        /// teach, and -- because the species tags sit 6 apart -- a teaching
        /// pairing teaches exactly one species. The avidity upgrade raises
        /// it to 3 (17.19%, and 25.4% of tags then teach two species),
        /// which is cross-reactivity bought deliberately.
        ///
        /// **Correction to §5c:** that section says the Hamming threshold
        /// is the dial to reach for before changing barcode length. That is
        /// backwards -- length is the binding constraint, and no threshold
        /// could have fixed the 8-bit roster.
        /// </summary>
        public static int MatchMaxHammingDistance = 2;

        // -- Knowledge (GAME_DESIGN.md §5) --

        /// <summary>
        /// Percentage points a single matching pairing adds to that
        /// species' knowledge.
        ///
        /// **Sprint 29: 3 -> 8.** Widening the barcode to 10 bits made a
        /// match 2.64x rarer (14.45% -> 5.47%), which would have slowed the
        /// whole adaptive ramp by the same factor. Scaling the reward by
        /// that ratio keeps the pace the game was tuned around, and the
        /// side effect is an improvement: teaching is now a rarer, larger
        /// event, which reads far better on the HUD than a constant drip.
        /// </summary>
        public static float KnowledgePerMatch = 8f;

        /// <summary>Knowledge is a 0..this percentage.</summary>
        public static float KnowledgeMax = 100f;

        // -- Sprint 29: memory recall (Director) ------------------------

        /// <summary>
        /// **Memory T cells, as the game can honestly express them.**
        ///
        /// A helper-T pairing that would otherwise MISS gets a second
        /// chance, with probability `bonus x (knowledge of that species /
        /// KnowledgeMax)`. So the better the system already knows an
        /// organism, the more readily it recognises it again -- knowledge
        /// stands in for "how many times we have seen this before", which
        /// is the Director's framing and the only proxy the game has.
        ///
        /// Why this shape and not a burst of cells: three independent
        /// research passes found the ~30% memory rung's "spawn a burst of
        /// CTLs" specification backwards. Day-8 effector numbers are
        /// invariant to how much memory you start with, and the onset of
        /// division is indistinguishable from naive. Memory's real
        /// advantages are a far larger starting precursor pool and zero
        /// lead time -- which is exactly "you recognise it immediately",
        /// not "you make more of it".
        ///
        /// 0 disables it. Only the memory-T upgrade raises a tower's share
        /// of this; the base is 0 so an un-upgraded node behaves exactly as
        /// it did before.
        /// </summary>
        public static float MemoryRecallBonus = 0.55f;

        // -- Antigens are per SPECIES (Sprint 24), and live on the
        //    species roster rather than here, because a tag is a property of
        //    an organism rather than a tuning knob. The separation rule the
        //    old per-class tags followed still holds and still matters: tags
        //    are kept >=4 bits apart, so a helper-T cell within Hamming
        //    distance 2 of one species is within 2 of at most one, and
        //    "knowledge is per species" stays meaningful. The MATCH RATE
        //    (37/256) depends only on the threshold, not the values. --

        public static int AntigenForSpecies(PathogenSpecies s) => SpeciesRoster.Get(s).Antigen;

        // -- The dendritic cell (GAME_DESIGN.md §5a) --

        /// <summary>How many helper-T pairings one antigen cargo is good for
        /// before the DC has to go back to tissue for more (§5a step 4,
        /// "the DC eventually loses its cargo").</summary>
        public static int DcPresentationsPerCargo = 4;

        /// <summary>Debris a DC eats from a pile each time it samples it.
        /// Non-zero on purpose: a DC that samples a pile is also clearing it,
        /// so it competes with macrophage efferocytosis for the same debris
        /// (§1c -- "a macrophage clearing debris is removing what a DC would
        /// have sampled"). ~3 samples drains a full pile.</summary>
        public static float DcDebrisSamplePerBite = 0.34f;

        /// <summary>DC fine-tiles-per-tick. **Sprint 14** raised this 2 -> 3
        /// (neutrophil speed): a DC now spends its whole tissue life pacing
        /// the band, so the pace has to be fast enough that the
        /// oscillation reads inside a ~30s round.</summary>
        public static int DcFineTilesPerTick = 3;

        /// <summary>While in tissue a DC biases its random walk **away from
        /// other DCs, along the CROSS (lane) axis only** (Director,
        /// 2026-08-29) -- the threat axis is base↔lumen, so repelling along
        /// the perpendicular keeps DCs spread evenly across the lanes.
        /// Higher = spreads harder. 0 disables it. Works at FINE-tile
        /// granularity every step (fixed Sprint 12) and now runs the DC's
        /// whole tissue life, not just a brief patrol phase (Sprint 14).</summary>
        public static float DcLaneRepelStrength = 0.8f;

        /// <summary>Only other DCs within this many coarse cells along the
        /// THREAT axis count toward a DC's crowding -- so DCs at opposite
        /// ends of the tissue band don't push on each other.</summary>
        public static int DcLaneRepelAxisRange = 12;

        /// <summary>A DC biases its THREAT-axis steps toward its current
        /// patrol heading. **Sprint 14:** the heading oscillates between the
        /// tissue-band edges the *entire time the DC is in tissue* (not just
        /// a short patrol state) -- it forces toward the base only while
        /// carrying antigen (to deliver), otherwise it paces base↔lumen and
        /// flips at each edge. Raised 1.0 -> 1.8 so the oscillation is
        /// unmistakable. 0 = a plain random walk in the threat axis.</summary>
        public static float DcPatrolSweepBias = 1.8f;

        // -- The helper-T cell / lymph node (GAME_DESIGN.md §5c) --

        /// <summary>A helper-T cell despawns this long after emission, and
        /// the progenitor emits a fresh one with a new random tag -- this is
        /// what makes the barcode repertoire TURN OVER (§5c step 6), so a
        /// player with no current match is not permanently stuck.</summary>
        public static float LymphocyteLifespanSeconds = 20f;

        public static int LymphocyteFineTilesPerTick = 2;

        /// <summary>Both a DC and a helper-T freeze for this long when they
        /// pair (§5c step 5) -- the cost that makes a mismatched pairing an
        /// actual loss.</summary>
        public static float PairingSeconds = 1.5f;

        /// <summary>A DC and a helper-T pair when within this many node
        /// fine-tiles of each other (Chebyshev), same "radius not exact
        /// tile" reasoning as SearchUnit contact.</summary>
        public static int NodePairingContactFineTiles = 3;

        // -- The co-localisation cytokine field (GAME_DESIGN.md §5c step 4).
        //    A DIFFERENT signal from the infection cytokine: it exists only
        //    inside the node and pulls DCs and helper-T cells together so
        //    meetings reliably happen instead of relying on two random walks
        //    intersecting. Built as a real CytokineField (see LymphNode) so
        //    node movement runs the exact Chemotaxis path. --

        /// <summary>Strength of the fixed central attractant source.</summary>
        public static float NodeColocalisationSourceStrength = 18f;

        /// <summary>Strength each resident lymphocyte contributes as its own
        /// weak source, so a DC drifts toward where the T cells actually
        /// are, not just the geometric centre.</summary>
        public static float NodeLymphocyteSourceStrength = 6f;

        // -- Emission (shares the BoneMarrowManager cadence model) --

        public static float DcEmissionIntervalSeconds = 5f;
        public static float LymphocyteEmissionIntervalSeconds = 3.5f;

        // -- Sprint 23 roster. Intervals, not rates, to match the existing
        //    cadence model. See docs/CELL_KINETICS_REFERENCE.md.

        /// <summary>CD8 effector and germinal-centre B cells are the two
        /// fastest-dividing cells in the body and are within ~15% of each
        /// other (6.2 h vs 6-7 h), so the CTL and B-cell towers deliberately
        /// SHARE a base interval. If one is ever retuned without the other,
        /// that shared basis is the thing being broken.</summary>
        public static float CytotoxicTEmissionIntervalSeconds = 4.5f;
        public static float BCellEmissionIntervalSeconds = 4.5f;

        /// <summary>Slower, and deliberately NOT upgradeable by clonal
        /// expansion: human NK proliferation is *unchanged* during acute
        /// infection (Zhang 2007) and homeostatic doubling is ~16 days. NK
        /// cells are early because they are already present, not because
        /// they ramp fast -- the common "NK responds faster than T" framing
        /// is contradicted by Schlub 2011, which found NK proliferation
        /// neither earlier nor faster than CD4/CD8.</summary>
        public static float NaturalKillerEmissionIntervalSeconds = 6f;

        /// <summary>Tissue-RESIDENT: this tower is topping up a standing
        /// epithelial population, not mounting a response, so it is the
        /// slowest producer on the board.</summary>
        public static float GammaDeltaTEmissionIntervalSeconds = 7f;

        /// <summary>Per-progenitor ceiling on simultaneously-alive children,
        /// same role as UnitLifecycleTuning.MaxActiveChildren for the innate
        /// towers.</summary>
        /// <summary>Sprint 25: the Director's board-legibility pass cut
        /// every progenitor's standing population. DCs 4 -> 3.</summary>
        public static int DcMaxActiveChildren = 3;

        /// <summary>Sprint 25: 8 -> 4, and this one is the important change.
        /// A helper-T niche's steady state is lifespan / interval = 20 / 3.5
        /// ~= 5.7, so a cap of 8 **never bound** and clonal expansion's cap
        /// component bought literally nothing (the Sprint 18 note spotted
        /// this and compensated by raising the emission RATE instead). At 4
        /// the cap binds, so the upgrade now buys both halves of what it
        /// claims to. General rule, restated: check a cap binds at default
        /// numbers, or the upgrade is decoration.</summary>
        public static int LymphocyteMaxActiveChildren = 4;

        public static void ResetToDefaults()
        {
            MatchMaxHammingDistance = 2;
            SpeciesRoster.ResetToDefaults();
            KnowledgePerMatch = 8f;
            MemoryRecallBonus = 0.55f;
            KnowledgeMax = 100f;
            DcPresentationsPerCargo = 4;
            DcDebrisSamplePerBite = 0.34f;
            DcFineTilesPerTick = 3;
            DcLaneRepelStrength = 0.8f;
            DcLaneRepelAxisRange = 12;
            DcPatrolSweepBias = 1.8f;
            LymphocyteLifespanSeconds = 20f;
            LymphocyteFineTilesPerTick = 2;
            PairingSeconds = 1.5f;
            NodePairingContactFineTiles = 3;
            NodeColocalisationSourceStrength = 18f;
            NodeLymphocyteSourceStrength = 6f;
            DcEmissionIntervalSeconds = 5f;
            LymphocyteEmissionIntervalSeconds = 3.5f;
            CytotoxicTEmissionIntervalSeconds = 4.5f;
            BCellEmissionIntervalSeconds = 4.5f;
            NaturalKillerEmissionIntervalSeconds = 6f;
            GammaDeltaTEmissionIntervalSeconds = 7f;
            DcMaxActiveChildren = 3;
            LymphocyteMaxActiveChildren = 4;
        }
    }
}
