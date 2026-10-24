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

        public static void ResetToDefaults()
        {
            IfnGammaActivationMultiplier = 2.5f;
            IfnGammaActivationSeconds = 6f;
        }
    }
}
