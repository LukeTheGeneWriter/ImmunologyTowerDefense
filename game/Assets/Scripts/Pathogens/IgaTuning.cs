namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Secretory IgA, and bacterial replication in the lumen -- the two
    /// mechanics the Director added to Sprint 22 on 2026-10-18, which the
    /// research pass then showed are **one mechanism**.
    ///
    /// All mutable statics with ResetToDefaults, like InvasionTuning: a
    /// headless harness needs to override a value for a sweep or to force a
    /// deterministic outcome.
    ///
    /// ## The thing to understand before changing any number here
    ///
    /// The Director's instruction was that replication be "hampered by IgA".
    /// It is -- but NOT by a growth-rate penalty, and there is deliberately
    /// no such field in this class.
    ///
    /// Moor et al., Nature 544:498-502 (2017), 10.1038/nature22058: IgA does
    /// not slow bacteria down. Classical agglutination (two free organisms
    /// colliding and being bridged) only works at high density, which real
    /// infections never reach. What actually happens is **enchained growth**
    /// -- IgA cross-links DAUGHTER CELLS as they divide, so a clump grows
    /// from the bacterium's own replication rather than from collisions.
    /// Bansept et al., PLoS Comput Biol 15:e1006986 (2019),
    /// 10.1371/journal.pcbi.1006986, adds the race: links break over time,
    /// so dividing faster than your links break builds your own cage, while
    /// dividing slowly lets daughters snap free.
    ///
    /// That buys the property this whole design rests on: **fast replicators
    /// trap themselves hardest, and nothing in the game ever has to decide
    /// which pathogen deserves IgA.** Bansept's own conclusion is that this
    /// is why a real gut can afford to make antibody against everything it
    /// has ever met. Do not "simplify" this into a rate brake -- the
    /// observable (free numbers stop growing) would survive and the reason
    /// would be a lie.
    ///
    /// See docs/IGA_REFERENCE.md and INVASION_REWORK_SPEC.md section 12.1 D1.
    /// </summary>
    public static class IgaTuning
    {
        // -- Eligibility and coating (SPEC section 4.2) --

        /// <summary>Knowledge of a species, in percent, at which secretory
        /// IgA against it comes online. Fixed by KnowledgeLadder's
        /// SecretoryIgA rung, not a free number.</summary>
        public static float KnowledgePercent = 70f;

        /// <summary>
        /// Per-second chance an eligible organism gets coated. Mean ~3.3s.
        ///
        /// **Raised 0.18 -> 0.30 during Sprint 22's build**, after a race
        /// showed up in testing: a crowd of 8 erodes the wall it is gripping,
        /// eroded wall charges 1.4x faster, and at 0.18 coating only just
        /// beat the plaque -- so the armed wall breached on some runs and not
        /// others.
        ///
        /// The tie-break is the Director's ruling that the 70% rung must be
        /// "an unambiguous reward, never a trap", and the spec's promise that
        /// earning it visibly DISSOLVES an existing threat: the blister
        /// deflates as its members are coated and rafted away. A rung that
        /// sometimes arrives too late to save the wall it was bought for does
        /// not deliver that.
        ///
        /// It is still slow enough that a big round has free organisms
        /// gripping for several seconds. Flagged as a playtest number: this
        /// is the lever if IgA proves too strong, per the spec's own residual
        /// -risk note.</summary>
        public static float CoatChancePerSecond = 0.30f;

        /// <summary>How far out from the seam IgA reaches. Section 5 is
        /// explicit that IgA acts "before adhesion is possible", so this
        /// covers the whole mucus band and the first lumen cell, not just
        /// the gripping depth.</summary>
        public static float ReachDepthCells = 1.8f;

        // -- Cross-linking: the bulk rule (SPEC section 4.3) --

        /// <summary>Per-second chance of bridging, PER coated neighbour of
        /// the same class in radius. With two neighbours a pair bridges in
        /// ~2s, so a dense coated patch agglutinates almost as fast as it
        /// coats. It must be visibly faster in a crowd than alone -- that
        /// acceleration IS the "density flips sign" promise.</summary>
        public static float CrossLinkChancePerSecondPerNeighbour = 0.25f;

        /// <summary>How close two coated organisms must be to bridge. About
        /// two pathogen diameters: they have to look like they are touching
        /// when the antibody spans them.</summary>
        public static float CrossLinkRadiusCells = 0.45f;

        /// <summary>Clump bulk is BaseRadius * sqrt(members), so a 4-clump is
        /// 0.60 cells and a 9-clump 0.90 -- visibly chunky, still under one
        /// cell so a raft does not swamp the wall.</summary>
        public static float ClumpBaseRadiusCells = 0.30f;

        /// <summary>Past this a new clump forms instead of growing this one.
        /// Keeps sqrt bulk bounded and the drawn raft under a cell.</summary>
        public static int ClumpMaxMembers = 9;

        /// <summary>Floor on how far out a clump is held. Beyond
        /// InvasionTuning.GripDepthCells by construction, inside the mucus
        /// band, so a raft sits in the drawn glaze rather than out in open
        /// chyme. The actual hold depth is max(this, clump radius), so
        /// **bigger clumps are held further out** -- density increases the
        /// exclusion.</summary>
        public static float HoldDepthCells = 1.15f;

        /// <summary>How fast a newly clumped organism is pushed off the wall.
        /// ~1.5s: a visible "it lets go" beat rather than a snap.</summary>
        public static float PushOutSpeedCellsPerSecond = 0.6f;

        /// <summary>Multiplier on a clumped organism's downstream creep. A
        /// raft crosses the remaining wall inside a round rather than
        /// loitering -- the rung is a reward, and a clump that hangs around
        /// for five minutes does not feel like one.</summary>
        public static float ClearCreepMultiplier = 3.0f;

        /// <summary>Per-second chance an existing link breaks. **This is the
        /// other half of the enchained-growth race** (Bansept 2019): a clump
        /// only grows if division outruns dislocation. At 0.02/s a link lasts
        /// ~50s on average, which is comfortably longer than the division
        /// interval below -- so a dividing bacterium does build its own cage,
        /// while a stalled one slowly comes apart.
        ///
        /// The literature only ever FITS this rate, never measures it, so the
        /// whole replication-versus-breaking balance is qualitative and this
        /// is a playtest number (IGA_REFERENCE.md section 3).</summary>
        public static float LinkBreakChancePerSecond = 0.02f;

        // -- Slower, pacified, fragile (SPEC section 4.4, rulings 2 and 3) --

        /// <summary>Multiplier on a coated organism's movement -- the mill's
        /// cohesion and loop amplitude, and its tissue step rate. "Much
        /// slower" without stopping it dead.
        ///
        /// Note the honest framing: the literature has no per-cell swimming
        /// speed penalty for antibody binding. Bacteria are TANGLED, not
        /// braked (IGA_REFERENCE.md claim 1). This number stands in for
        /// entanglement, and is applied to coated singles mostly so that a
        /// coated organism which does get through is visibly labouring.
        /// </summary>
        public static float MotilityMultiplier = 0.35f;

        /// <summary>Multiplier on everything a coated organism does TO the
        /// host: wall grazing, host-cell damage, and its chance of getting
        /// inside a cell. "Pacified... losing virulence" -- reduced rather
        /// than zero, so it is still a pathogen that has to be cleared.
        ///
        /// Must NOT be read as "harmless" or "sterile": a coated bacterium
        /// still divides at full rate (IGA_REFERENCE.md section 5).</summary>
        public static float VirulenceMultiplier = 0.25f;

        /// <summary>Damage multiplier against a coated organism, in TISSUE
        /// only.
        ///
        /// **This is the one place the Director's stated biology needed
        /// correcting.** Secretory IgA is the non-inflammatory isotype: it
        /// does not fix complement, it actively inhibits IgG-driven
        /// complement, and the lumen has no patrolling phagocytes for it to
        /// hand anything to. The gut's answer is flush, not eat. So there is
        /// no luminal killing bonus anywhere in this sprint, and the "easier
        /// to deal with" ruling is honoured behind the wall, where
        /// phagocytes actually are. See IGA_REFERENCE.md claim 2.</summary>
        public static float TissueDamageTakenMultiplier = 1.6f;

        // -- In-lumen bacterial replication (SPEC section 4a) --

        /// <summary>Seconds between divisions for a bacterium in the lumen.
        /// The pacing anchor: a bolus transit is 30s and a round is roughly
        /// 30-60s of active time, so ~22s means an unmanaged inoculum roughly
        /// doubles once per round -- felt, not explosive.
        ///
        /// Real gut doubling times run 20-60 MINUTES, so this is a deliberate
        /// game-time compression and is flagged as one.</summary>
        public static float LumenDivideIntervalSeconds = 22f;

        /// <summary>Intracellular bacteria divide at half speed while out in
        /// the lumen; their fast case is inside a host cell, where
        /// InvasionTuning.IntracellularReplicationIntervalSeconds already
        /// gives them 3s. Keeps the two bacterial classes mechanically
        /// distinct. Viruses do not replicate in the lumen at all -- a virus
        /// cannot replicate without a host, which is the same rule as the
        /// free-virion death clock.</summary>
        public static float IntracellularLumenRateMultiplier = 0.5f;

        /// <summary>How hard local density damps division:
        /// interval *= (1 + this * localCrowd). At crowd 5 the interval is
        /// x4; at crowd 10, x7. A dense patch is near-stationary in
        /// population, which is what stops replication compounding with the
        /// plaque mechanic into a runaway.
        ///
        /// **The mechanism is nutrient competition, not quorum sensing.**
        /// The Director's intuition that neighbours slow each other is right;
        /// the usual explanation is not. Quorum sensing governs virulence and
        /// biofilm, and growth rate actually drives QS rather than the
        /// reverse. What really competes is metabolic overlap -- and your own
        /// kind overlap with you most, because they eat exactly what you eat
        /// (Spragge et al., Science 2023, 10.1126/science.adj3502). That
        /// version teaches "the microbiota is a defence" instead of
        /// "bacteria are polite". See IGA_REFERENCE.md claim 4.</summary>
        public static float CrowdDamperStrength = 0.6f;

        /// <summary>Headroom left in the agent pool, so replication can never
        /// starve the spawner that the bolus needs. A blocked division just
        /// re-arms the timer.</summary>
        public static int PoolHeadroom = 10;

        public static void ResetToDefaults()
        {
            KnowledgePercent = 70f;
            CoatChancePerSecond = 0.30f;
            ReachDepthCells = 1.8f;
            CrossLinkChancePerSecondPerNeighbour = 0.25f;
            CrossLinkRadiusCells = 0.45f;
            ClumpBaseRadiusCells = 0.30f;
            ClumpMaxMembers = 9;
            HoldDepthCells = 1.15f;
            PushOutSpeedCellsPerSecond = 0.6f;
            ClearCreepMultiplier = 3.0f;
            LinkBreakChancePerSecond = 0.02f;
            MotilityMultiplier = 0.35f;
            VirulenceMultiplier = 0.25f;
            TissueDamageTakenMultiplier = 1.6f;
            LumenDivideIntervalSeconds = 22f;
            IntracellularLumenRateMultiplier = 0.5f;
            CrowdDamperStrength = 0.6f;
            PoolHeadroom = 10;
        }
    }
}
