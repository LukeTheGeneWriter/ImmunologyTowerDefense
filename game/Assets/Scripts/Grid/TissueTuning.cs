namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Every number Sprint 5's HOST LAYER runs on (GAME_DESIGN.md section
    /// 1c): host cell health, debris decay, and regrowth. Pathogen-side
    /// numbers (viral survival, intracellular residence) live in
    /// ImmunologyTD.Pathogens.InvasionTuning instead, and per-unit numbers
    /// (how fast a macrophage eats debris) live on UnitProfile /
    /// UnitLifecycleTuning, because those are per-tower upgrade targets and
    /// these are map constants.
    ///
    /// **All mutable statics, none of them `const`**, same reasoning as
    /// InvasionTuning: a harness needs to be able to override one value for
    /// a group of assertions, and a later tuning pass should not need a code
    /// restructure. Nothing here is balance-tested -- the Director's
    /// standing instruction through Sprint 5 is still mechanics first, and
    /// every value below was chosen for LEGIBILITY inside a short playtest.
    /// See docs/TEAM_RETRO.md for the derivation of each.
    /// </summary>
    public static class TissueTuning
    {
        /// <summary>Health of an intact host cell. Only two things damage a
        /// host cell directly: a large bacterium standing on it, and a
        /// neutrophil's degranulation burst. Clearing an INTRACELLULAR
        /// infection does not go through this -- the pathogen's own health
        /// is damaged and the cell dies with it (GAME_DESIGN.md section 4a:
        /// "the only way an innate cell can clear an infected cell is to
        /// damage the cell itself into destruction, pathogen included"),
        /// which deliberately leaves Sprint 2's clearing times untouched.</summary>
        public static float HostCellMaxHealth = 10f;

        /// <summary>Seconds of bare `Empty` ground before a `Healthy` host
        /// cell regrows there. Debris blocks this entirely (GAME_DESIGN.md
        /// section 1c) -- an `Empty` cell is ground that has already been
        /// cleaned.
        ///
        /// 20s is a judgment call. It is deliberately slower than a
        /// pathogen's ~1 cell/second advance, so ground lost to a front
        /// genuinely stays lost while the front is on it, and fast enough
        /// that a Director watching a cleared pocket sees it fill back in
        /// within one playtest rather than having to take it on trust.</summary>
        public static float HostRegenerationSeconds = 20f;

        /// <summary>Seconds for untended debris to dissipate on its own from
        /// full (1.0) to nothing. GAME_DESIGN.md section 1c requires this to
        /// exist ("a player who never invests in clearance is not
        /// permanently locked out of their own tissue") and to be clearly
        /// worse than efferocytosis.
        ///
        /// 60s against a macrophage's ~1.2s of sustained contact is a ~50x
        /// gap -- unambiguous, and still short enough that a headless
        /// harness (and a patient Director) can watch it happen.</summary>
        public static float DebrisSelfDissipationSeconds = 60f;

        /// <summary>How often the host layer is swept for debris decay and
        /// regrowth. The sweep is O(cells) and allocation-free; at 4 Hz over
        /// Map 01's 4,000 cells that is 16k trivial comparisons a second,
        /// which is nothing next to the 4,000 SpriteRenderers already being
        /// driven. Decay is integrated over the accumulated delta, not per
        /// sweep, so changing this rate does not change any timing.</summary>
        public static float SweepIntervalSeconds = 0.25f;

        public static void ResetToDefaults()
        {
            HostCellMaxHealth = 10f;
            HostRegenerationSeconds = 20f;
            DebrisSelfDissipationSeconds = 60f;
            SweepIntervalSeconds = 0.25f;
        }
    }
}
