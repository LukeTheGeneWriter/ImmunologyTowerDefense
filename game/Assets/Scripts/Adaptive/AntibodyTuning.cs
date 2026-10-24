namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Sprint 23. Numbers for plasma-cell antibody secretion, in the usual
    /// mutable-statics-with-a-reset shape.
    ///
    /// **All playtest numbers.** The literature gives a real secretion rate
    /// -- a plasma cell exports on the order of a thousand antibody molecules
    /// per second -- and that figure is useless here for the obvious reason:
    /// one visible Y per molecule would be millions of sprites, and the
    /// board is 250 coarse cells. What survives the compression is the SHAPE
    /// of the fact, which is that a plasma cell is a factory running
    /// continuously rather than a fighter landing blows. Hence: a short
    /// interval, a cheap object, and no kill of its own.
    /// </summary>
    public static class AntibodyTuning
    {
        /// <summary>Seconds between one plasma cell's secretions.</summary>
        public static float SecretionIntervalSeconds = 1.1f;

        /// <summary>How long a secreted antibody drifts before it is spent.
        /// Short on purpose: it makes plasma-cell POSITION matter, which is
        /// the only decision the player gets to make about them.</summary>
        public static float LifespanSeconds = 4.5f;

        /// <summary>Drift speed in world units per second.</summary>
        public static float DriftWorldUnitsPerSecond = 0.55f;

        /// <summary>Rendered size, in fine tiles. GAME_DESIGN.md §7 puts an
        /// antibody at 1x1 -- the smallest thing on the board.</summary>
        public static float SizeFineTiles = 1.6f;

        /// <summary>Pool prewarm. Several plasma cells at ~1/s each with a
        /// 4.5s life means a few dozen in flight at the worst moment.</summary>
        public static int PoolPrewarm = 48;

        public static void ResetToDefaults()
        {
            SecretionIntervalSeconds = 1.1f;
            LifespanSeconds = 4.5f;
            DriftWorldUnitsPerSecond = 0.55f;
            SizeFineTiles = 1.6f;
            PoolPrewarm = 48;
        }
    }
}
