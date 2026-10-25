namespace ImmunologyTD.Units
{
    /// <summary>
    /// Sprint 23. Cross-unit numbers that belong to no single kind, in the
    /// usual mutable-statics-with-a-reset shape (<c>InvasionTuning</c>,
    /// <c>TissueTuning</c>, <c>AdaptiveTuning</c>).
    ///
    /// Per-KIND numbers still live on <see cref="UnitProfile"/> and per-TOWER
    /// copies on <see cref="UnitLifecycleTuning"/>; this is only for values
    /// that describe an interaction BETWEEN kinds.
    /// </summary>
    public static class UnitTuning
    {
        /// <summary>How much better an IFN-gamma-activated macrophage is, as
        /// a multiplier on both its contact damage and its recognition rate.
        ///
        /// 2.5x is chosen from the opsonisation/activation literature's
        /// general shape rather than one headline figure: measured
        /// enhancements of phagocyte killing cluster in the low single
        /// digits (~2-10x depending on assay and readout), and the research
        /// pass flagged explicitly that no defensible single fold-change
        /// exists for this. Deliberately NOT a 10x+ buff -- that would make
        /// the NK tower the obvious purchase and flatten the roster, which
        /// is the failure mode the Director's "amplifier, not competitor"
        /// framing exists to avoid. **Playtest number.**</summary>
        public static float IfnGammaActivationMultiplier = 2.5f;

        /// <summary>How long one activation lasts. Long enough that an
        /// activated macrophage visibly works through a target, short enough
        /// that the NK cell has to stay nearby -- which is what makes NK
        /// placement a positioning decision rather than a global buff.</summary>
        public static float IfnGammaActivationSeconds = 6f;

        // -- Sprint 28: crowd desensitisation (the anti-swarm) ----------

        /// <summary>
        /// How far, in fine tiles, a unit notices others of its OWN kind for
        /// crowding purposes.
        ///
        /// Same-kind only, deliberately. A macrophage and a neutrophil
        /// working the same pathogen is fine and realistic; eight
        /// neutrophils piling onto it while bacteria walk past is the
        /// problem. Keeping the check homotypic also matches the mechanism:
        /// neutrophil swarming is driven by self-secreted attractants, and
        /// it is those the cell desensitises to.
        /// </summary>
        /// Sprint 31: 6 -> 14. Radius turned out to be the dominant lever,
        /// not strength: at radius 6 cells settled two tiles apart and
        /// stopped, and raising the strength a hundredfold did not move
        /// them further. Measured mean spacing went 4.4 -> 10.2 fine tiles
        /// on radius alone.
        public static int RepulsionRadiusFineTiles = 14;

        /// <summary>
        /// How many same-kind companions a unit tolerates **for free**
        /// before crowding starts pushing it away.
        ///
        /// This is the dial that keeps cooperation alive. Halle 2016 found
        /// killing is superadditive at three or more attackers, and Sprint
        /// 23 built that bonus, so a value of 2 means three cells can
        /// converge on one target with no penalty at all -- exactly the
        /// number the cooperation bonus rewards -- and the fourth onward
        /// start looking elsewhere. Set it to 0 and cells refuse to gang up;
        /// set it high and you get the swarm back.
        /// </summary>
        public static int RepulsionFreeCompanions = 2;

        /// <summary>
        /// Penalty applied per surplus companion, in the same units as the
        /// cytokine value the chemotaxis softmax reads. Zero disables
        /// crowding entirely.
        /// </summary>
        public static float RepulsionStrength = 6.4f;

        /// <summary>
        /// The SHAPE of the crowding response: penalty rises as
        /// `strength * surplus^exponent`.
        ///
        /// Above 1 the response is forgiving at first and then sharp -- the
        /// fourth cell is barely discouraged, the seventh is strongly
        /// pushed off. That is the shape the biology suggests: Kienle 2021
        /// (Science) describes swarming as self-limiting through GPCR
        /// desensitisation, a negative feedback that engages once a cell
        /// senses HIGH concentrations of self-secreted attractant, not at
        /// the first sign of a neighbour.
        ///
        /// Worth knowing before tuning this up: the same paper found that
        /// disabling the brake let neutrophils scan more tissue but did NOT
        /// improve bacterial clearance, because containment of proliferating
        /// bacteria got worse. There is a real optimum in the middle, and
        /// both extremes are worse than it.
        /// </summary>
        public static float RepulsionExponent = 1.6f;

        /// <summary>
        /// How much a neighbour of a DIFFERENT kind counts toward crowding,
        /// relative to one of your own.
        ///
        /// Sprint 28 ignored other kinds entirely, on the reasoning that
        /// neutrophil swarming is driven by self-secreted attractants and
        /// it is those a cell desensitises to. That is true of the
        /// signalling, but it left the actual complaint unanswered: a
        /// macrophage would walk straight into a pile of six neutrophils
        /// because none of them were its own kind. Physical space is shared
        /// whoever is standing in it.
        ///
        /// Below 1 so same-kind crowding still dominates -- a macrophage
        /// and a neutrophil working one target together stays cheap, which
        /// is both realistic and the cooperation the game wants.
        /// </summary>
        public static float RepulsionCrossKindWeight = 0.5f;

        /// <summary>
        /// Shape of the distance falloff: contribution is
        /// `(1 - d/R)^RepulsionFalloffPower`.
        ///
        /// This is the dial the Director asked for when he said to play
        /// with the shape, and it matters more than the strength. A high
        /// power makes distant neighbours nearly free, so cells tolerate
        /// sitting a couple of tiles apart and bunch; a LOW power keeps
        /// pressure on out to the full radius and disperses them across
        /// the board. Measured: squared falloff put 12 of 12 cells in one
        /// coarse slot, where a near-flat falloff spreads them over
        /// several.
        /// </summary>
        public static float RepulsionFalloffPower = 1.0f;

        public static void ResetToDefaults()
        {
            RepulsionRadiusFineTiles = 14;
            RepulsionFreeCompanions = 2;
            RepulsionStrength = 6.4f;
            RepulsionExponent = 1.6f;
            RepulsionCrossKindWeight = 0.5f;
            RepulsionFalloffPower = 1.0f;
            IfnGammaActivationMultiplier = 2.5f;
            IfnGammaActivationSeconds = 6f;
        }
    }
}
