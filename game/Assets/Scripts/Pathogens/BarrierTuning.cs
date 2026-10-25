namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// The epithelial barrier itself: how it is worn down, what a dead
    /// stretch does, how it is shed, and how it comes back
    /// (INVASION_REWORK_SPEC.md sections 5 and 6, as amended by the
    /// Director's decisions in section 12).
    ///
    /// All mutable statics with ResetToDefaults, like InvasionTuning.
    /// </summary>
    public static class BarrierTuning
    {
        // -- Segments --

        /// <summary>Barrier segments per coarse lane. Two gives a 0.5-cell
        /// segment (37px on Map 01) -- fine enough that a rupture's footprint
        /// kills a readable patch rather than a whole lane, coarse enough
        /// that the wall is 20 objects and not 200.</summary>
        public static int SegmentsPerLane = 2;

        /// <summary>Health of an intact segment, and the threshold below
        /// which it reads as eroded rather than intact.</summary>
        public static float SegmentMaxHealth = 10f;
        public static float ErodedBelowHealth = 6f;

        /// <summary>Damage per second a gripping organism does to the segment
        /// it is holding. Scaled by the organism's virulence, so a coated one
        /// does a quarter of it. At 0.35/s a lone adherent takes ~30s to
        /// erode a segment and would need ~95s to kill it -- slow enough that
        /// grazing is a background pressure rather than a second way to
        /// breach.</summary>
        public static float GrazeDamagePerSecond = 0.35f;

        // -- Dead stretches: the Director's asymmetry (D2) --

        /// <summary>
        /// How much faster a plaque charges over DEAD wall.
        ///
        /// **The Director chose "a weak spot, not a hole" (section 12.1 D2)**,
        /// against the head session's recommendation of an open gap. So a
        /// dead stretch does NOT let bacteria walk through unopposed: it
        /// still takes a crowd, and everything still goes through the single
        /// plaque-and-charge path. There is one ingress mechanic in this
        /// game, not two.
        ///
        /// x2.5 makes a previously-breached stretch meaningfully dangerous --
        /// the board remembers where it was hurt -- without making the first
        /// breach decide the round.</summary>
        public static float DeadChargeMultiplier = 2.5f;

        /// <summary>Same, for a merely eroded segment. Mild: erosion is a
        /// warning, not a wound.</summary>
        public static float ErodedChargeMultiplier = 1.4f;

        /// <summary>
        /// **The other half of the asymmetry, and the reason it is a
        /// strategic axis rather than a penalty.** A virus needs a LIVING
        /// cell: over a dead stretch there is nothing left to infect, so
        /// viruses contribute nothing to charging there.
        ///
        /// Phrase this as "nothing there to infect", never "cannot cross" --
        /// a virion can physically drift through a gap, it simply finds no
        /// host (docs/IGA_REFERENCE.md section 2H).
        ///
        /// The consequence is that bacteria and viruses want OPPOSITE walls:
        /// a battered wall is a bacterial sieve and a viral dead end, a
        /// pristine wall is the reverse. The player cannot optimise for both.
        /// </summary>
        public static bool VirusesNeedLivingWall = true;

        // -- Regrowth and repair --

        /// <summary>Seconds for a dead segment to regrow on its own. Slow:
        /// the Director's fork 7 answer was that an unmanaged wall self-heals
        /// but slowly, so one bad round is recoverable and a neglected wall
        /// still degrades.</summary>
        public static float RegrowSeconds = 45f;

        /// <summary>Multiplier on regrowth within reach of a crypt. Crypts
        /// are already the game's tissue-regeneration organ
        /// (GutWallTuning.CryptRegrowthBonus); this makes them repair the
        /// WALL too, which is what crypts actually do.</summary>
        public static float CryptRegrowMultiplier = 3f;
        public static float CryptReachCells = 3f;

        /// <summary>What a bought repair restores, in health, per segment in
        /// range, and how wide that is. The Director bought this as an
        /// explicit ability (D3), so it is deliberately strong on impact and
        /// limited by price rather than by effect.</summary>
        public static float RepairHealthRestored = 10f;
        public static float RepairRadiusCells = 1.0f;

        // -- Player agency on a plaque (D3) --

        /// <summary>Charge removed per second of sustained tower fire on a
        /// plaque. At 0.05/s a plaque at half charge is talked down in ~10s
        /// of focus -- an answer, not a cancel button.</summary>
        public static float SuppressionPerSecond = 0.05f;

        // -- Sentinel sloughing (D4) --

        /// <summary>
        /// Crowd value at a crypt that trips the sentinel response.
        ///
        /// **This mechanic exists because the Director asked a question.**
        /// Offered three options for who aims mucus sloughing, he declined
        /// all three and asked whether intestines really slough automatically
        /// near a pathogen. They do, by a mechanism that fits this game
        /// unusually well: Birchenough, Nystrom, Johansson and Hansson,
        /// "A sentinel goblet cell guards the colonic crypt by triggering
        /// Nlrp6-dependent Muc2 secretion", Science 352(6293):1535-42 (2016),
        /// 10.1126/science.aaf7419.
        ///
        /// A sentinel goblet cell at the CRYPT ENTRANCE non-specifically
        /// endocytoses bacterial TLR ligands; on detection it fires the Nlrp6
        /// inflammasome, dumps its whole Muc2 load by compound exocytosis,
        /// and signals through GAP JUNCTIONS to its neighbours, which dump
        /// theirs -- and that "expels bacteria".
        ///
        /// Every property of this is already in the game: the trigger is
        /// pathogens getting too close (our crowd field), the response is
        /// local and spreads to neighbours (our patch), it needs no knowledge
        /// of the species, and it expels rather than kills.</summary>
        public static float SentinelTriggerCrowd = 2.5f;

        /// <summary>Cooldown per crypt after it fires. The cell has to refill
        /// its granules before it can dump them again.</summary>
        public static float SentinelCooldownSeconds = 18f;

        /// <summary>Half-width of the patch a sentinel clears, before the
        /// gap-junction spread below. Deliberately about one plaque
        /// footprint: a wave that cleared a third of the wall would make the
        /// wall uninteresting.</summary>
        public static float SentinelPatchCells = 1.0f;

        /// <summary>Extra half-width per level of the mucus-turnover upgrade,
        /// and how much each level shortens the cooldown.
        ///
        /// The baseline response is INNATE -- it costs nothing and works from
        /// round 1, which is what the research describes. The shop row buys
        /// AMPLIFICATION, which is section 6b's "the upgrade raises the shed
        /// rate" read spatially.</summary>
        public static float SentinelPatchPerLevel = 0.35f;
        public static float SentinelCooldownPerLevel = 3.5f;

        /// <summary>How long a sloughed organism cannot re-adhere, while it
        /// drifts clear of the wall it was just shed from.</summary>
        public static float SloughRefractorySeconds = 1.5f;

        // -- Sprint 24: cholera toxin --

        /// <summary>How much faster a plaque charges over fully-subverted
        /// wall. Tight junctions loosen under cholera toxin (claudin-2 up,
        /// tricellulin down), so the barrier is mechanically easier to get
        /// through without a single cell having died. Playtest number.</summary>
        public static float ToxinChargeBonus = 0.8f;

        /// <summary>Fraction of toxin load shed per second once the
        /// secretors are gone. Subversion is REVERSIBLE -- that is what
        /// makes clearing a cholera crowd feel like it achieved something,
        /// where killing cells would have left permanent holes.</summary>
        public static float ToxinDecayPerSecond = 0.12f;

        public static void ResetToDefaults()
        {
            ToxinChargeBonus = 0.8f;
            ToxinDecayPerSecond = 0.12f;
            SegmentsPerLane = 2;
            SegmentMaxHealth = 10f;
            ErodedBelowHealth = 6f;
            GrazeDamagePerSecond = 0.35f;
            DeadChargeMultiplier = 2.5f;
            ErodedChargeMultiplier = 1.4f;
            VirusesNeedLivingWall = true;
            RegrowSeconds = 45f;
            CryptRegrowMultiplier = 3f;
            CryptReachCells = 3f;
            RepairHealthRestored = 10f;
            RepairRadiusCells = 1.0f;
            SuppressionPerSecond = 0.05f;
            SentinelTriggerCrowd = 2.5f;
            SentinelCooldownSeconds = 18f;
            SentinelPatchCells = 1.0f;
            SentinelPatchPerLevel = 0.35f;
            SentinelCooldownPerLevel = 3.5f;
            SloughRefractorySeconds = 1.5f;
        }
    }
}
