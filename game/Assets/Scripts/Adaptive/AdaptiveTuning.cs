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

        /// <summary>Barcode length is fixed at 8 bits by the Director
        /// (§5c). Not a tuning knob -- here as a named constant so callers
        /// don't sprinkle literal 8s / 255s around.</summary>
        public const int BarcodeBits = 8;

        /// <summary>A DC:helper-T pairing teaches iff the Hamming distance
        /// between the two 8-bit tags is at most this. 2 => "at least 6 of 8
        /// bits agree" => 37/256 ≈ 14.5% of random pairings teach. §5c names
        /// a Hamming threshold as the dial to reach for before changing
        /// barcode length; 0 would be exact-match (1/256).</summary>
        public static int MatchMaxHammingDistance = 2;

        // -- Knowledge (GAME_DESIGN.md §5) --

        /// <summary>Percentage points a single matching pairing adds to that
        /// species' knowledge. Placeholder -- §5's threshold ladder isn't
        /// wired this sprint, so this only drives the HUD number.</summary>
        public static float KnowledgePerMatch = 3f;

        /// <summary>Knowledge is a 0..this percentage.</summary>
        public static float KnowledgeMax = 100f;

        // -- Per-class antigens (species key = PathogenClass for now; there
        //    is no species roster yet -- see SPRINT_PLAN.md item 1). The
        //    three values are >=4 bits apart so a helper-T within distance 2
        //    of one is within distance 2 of at most one -- keeps "knowledge
        //    is per species" meaningful. The MATCH RATE (37/256) does not
        //    depend on the specific values, only on the threshold. --

        public static byte VirusAntigen = 0b01011100;          // 92
        public static byte BacteriumAntigen = 0b10100011;       // 163
        public static byte LargeBacteriumAntigen = 0b11110000;  // 240

        public static byte AntigenForClass(PathogenClass c)
        {
            switch (c)
            {
                case PathogenClass.IntracellularVirus: return VirusAntigen;
                case PathogenClass.IntracellularBacterium: return BacteriumAntigen;
                default: return LargeBacteriumAntigen;
            }
        }

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

        /// <summary>Per-progenitor ceiling on simultaneously-alive children,
        /// same role as UnitLifecycleTuning.MaxActiveChildren for the innate
        /// towers.</summary>
        public static int DcMaxActiveChildren = 4;
        public static int LymphocyteMaxActiveChildren = 8;

        public static void ResetToDefaults()
        {
            MatchMaxHammingDistance = 2;
            KnowledgePerMatch = 3f;
            KnowledgeMax = 100f;
            VirusAntigen = 0b01011100;
            BacteriumAntigen = 0b10100011;
            LargeBacteriumAntigen = 0b11110000;
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
            DcMaxActiveChildren = 4;
            LymphocyteMaxActiveChildren = 8;
        }
    }
}
