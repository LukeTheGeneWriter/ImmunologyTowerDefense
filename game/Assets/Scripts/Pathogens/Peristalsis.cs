using UnityEngine;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Sprint 19: the gut's peristaltic rhythm, shared by the simulation
    /// (how the food bolus advances, <see cref="PathogenSpawner"/>) and the
    /// cosmetics (how the lumen squeezes, <c>LumenChannelRenderer</c>), so the
    /// contraction the player sees is the push the bolus feels.
    ///
    /// Before this the bolus stepped one lane every
    /// <c>FoodItemTransitSeconds / lanes</c> -- a constant crawl that snapped
    /// from cell to cell. Real chyme moves in **surges**: a ring of smooth
    /// muscle contracts behind the bolus and travels down the gut, shoving it
    /// forward; between waves it barely drifts. The Director's choice
    /// (2026-10-13): surge-and-pause.
    ///
    /// The profile, per wave cycle u ∈ [0,1): a push window
    /// u &lt; <see cref="PushFraction"/> where speed rises and falls as sin²,
    /// and a slow drift (<see cref="DriftSpeed"/> × mean) the rest of the
    /// time. The push amplitude is solved so the cycle's MEAN speed is
    /// exactly 1 -- and the period is the transit divided by a whole number
    /// of waves -- so total transit time is unchanged
    /// (<c>InvasionTuning.FoodItemTransitSeconds</c>) and round pacing does
    /// not move. Pure functions of elapsed time: frame-rate independent and
    /// headlessly testable.
    /// </summary>
    public static class Peristalsis
    {
        /// <summary>Waves per bolus transit. 4 at the 30s default = one wave
        /// every 7.5s, close to the old cosmetic 8s squeeze.</summary>
        public static int WavesPerTransit = 4;
        /// <summary>Fraction of each cycle spent being pushed.</summary>
        public static float PushFraction = 0.35f;
        /// <summary>Speed between waves, as a fraction of the mean.</summary>
        public static float DriftSpeed = 0.15f;

        public static float Period =>
            Mathf.Max(0.01f, InvasionTuning.FoodItemTransitSeconds / Mathf.Max(1, WavesPerTransit));

        private static float PushAmplitude =>
            2f * (1f - DriftSpeed) / Mathf.Max(0.01f, PushFraction);

        /// <summary>Speed multiplier at <paramref name="elapsed"/> seconds
        /// into a wave train (mean 1).</summary>
        public static float SpeedFactor(float elapsed)
        {
            float u = Frac(elapsed / Period);
            if (u >= PushFraction) return DriftSpeed;
            float s = Mathf.Sin(Mathf.PI * u / PushFraction);
            return DriftSpeed + PushAmplitude * s * s;
        }

        /// <summary>0..1 strength of the contraction at
        /// <paramref name="elapsed"/> -- the cosmetic squeeze reads this.</summary>
        public static float Contraction(float elapsed)
        {
            float u = Frac(elapsed / Period);
            if (u >= PushFraction) return 0f;
            float s = Mathf.Sin(Mathf.PI * u / PushFraction);
            return s * s;
        }

        /// <summary>Fraction of the full transit covered after
        /// <paramref name="elapsed"/> seconds: the closed-form integral of
        /// <see cref="SpeedFactor"/>, normalised so it reaches exactly 1 at
        /// <c>FoodItemTransitSeconds</c>. Keeps growing past 1 (the caller
        /// retires the bolus when it leaves the channel).</summary>
        public static float Travel01(float elapsed)
        {
            if (elapsed <= 0f) return 0f;
            float x = elapsed / Period;
            float whole = Mathf.Floor(x);
            float u = x - whole;
            // ∫0^u of (Drift + A·sin²(πu'/W)) du' over the push window, Drift after.
            float w = PushFraction;
            float partial;
            if (u < w)
                partial = DriftSpeed * u + PushAmplitude * (u * 0.5f - w / (4f * Mathf.PI) * Mathf.Sin(2f * Mathf.PI * u / w));
            else
                partial = DriftSpeed * u + PushAmplitude * w * 0.5f;
            // One whole cycle integrates to exactly 1 (mean speed 1).
            return (whole + partial) / Mathf.Max(1, WavesPerTransit);
        }

        public static void ResetToDefaults()
        {
            WavesPerTransit = 4;
            PushFraction = 0.35f;
            DriftSpeed = 0.15f;
        }

        // ---- cosmetic hook --------------------------------------------
        /// <summary>Where the bolus is and how far into its wave train, so
        /// the lumen renderer can contract the wall *behind* it. Written by
        /// <see cref="PathogenSpawner"/>; read-only for everything else.
        /// Same process-global, null-safe pattern as
        /// <c>EconomyHooks.PayForKill</c>.</summary>
        public static bool BolusActive { get; internal set; }
        public static Vector3 BolusWorldPosition { get; internal set; }
        public static float BolusElapsed { get; internal set; }

        private static float Frac(float v) => v - Mathf.Floor(v);
    }
}
