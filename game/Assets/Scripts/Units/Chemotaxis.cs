using UnityEngine;
using ImmunologyTD.Grid;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// The per-step neighbour-choice algorithm for both rung 1 (uniform
    /// random walk) and rung 2 (cytokine-biased walk) of GAME_DESIGN.md
    /// sections 7/9's search ladder. Pulled out of SearchUnit as a static,
    /// side-effect-free function -- rather than inlined in
    /// SearchUnit.StepOnce -- specifically so it can be driven directly by
    /// a headless verification harness (Assets/Editor/CytokineVerification.cs)
    /// without needing GameObjects or play mode. See docs/ENGINE_STATUS.md
    /// for what that harness measured.
    ///
    /// Sprint 1 closing task: the original rung-2 weighting was linear in
    /// each candidate's ABSOLUTE field value (`1 + k * value`). That
    /// produced a real but imperceptible drift, because the field is
    /// bilinear-interpolated smoothly across each coarse cell -- the four
    /// von Neumann neighbours of any fine tile have nearly identical
    /// absolute values, so a linear weighting on that absolute value barely
    /// distinguishes them even with a large k. This version instead uses a
    /// softmax over each candidate's value RELATIVE to the best candidate
    /// among the four -- only sensitive to the (often small) local
    /// difference that actually exists, and GradientSharpness amplifies
    /// that difference. Net effect: near-deterministic "beelining" within a
    /// few coarse cells of an infected site, where the local gradient is
    /// genuinely steep, fading to a near-uniform walk far from any source,
    /// where the field is nearly flat -- exactly the "preserves the
    /// wandering but adds visible drift" behaviour GAME_DESIGN.md section 7
    /// calls for, and the "units visibly beeline toward nearby infected
    /// cells" the Sprint 1 closing task asked for.
    /// </summary>
    public static class Chemotaxis
    {
        /// <summary>Empirically tuned against CytokineVerification's ON/OFF
        /// distance-to-nearest-infected-cell comparison (see
        /// docs/ENGINE_STATUS.md for the measured numbers -- with this
        /// value, simulated units reach a coarse cell holding an infected
        /// site in ~4.5s of simulated time on average with sensing ON,
        /// vs. staying scattered at a ~3.0-coarse-cell average distance
        /// with sensing OFF over the same multi-minute window). Chosen
        /// as the smallest sharpness tried that still produces a dramatic,
        /// fast difference -- higher values (tested up to 20) converge
        /// even faster but make each individual step read as closer to
        /// deterministic pathfinding, which risks blurring rung 2
        /// (probabilistic gradient bias) into rung 3 (greedy pathfinding)
        /// per GAME_DESIGN.md section 7's chemotaxis table. Deliberately a
        /// mutable static field, not a const -- CytokineVerification's
        /// sweep methods override this value to find the tuning before
        /// settling on the shipped default, and a const can't be
        /// overridden at runtime for that sweep.</summary>
        public static float GradientSharpness = 4f;

        /// <summary>Sprint 12: a **buyable, player-wide** upgrade that
        /// sharpens cytokine sensing for every unit at once (the Director
        /// always turned sensing on, so it is now default-on and the
        /// *improvement* is what you buy). `ShopItem.CytokineSensingUpgrade`
        /// raises this level; `HudOverlay` pushes it here from the
        /// `ShopLedger` each frame. 0 = base sensing.</summary>
        public static int SensingUpgradeLevel = 0;

        /// <summary>Each upgrade level multiplies the effective gradient
        /// sharpness by this much again (level 2 → ×2.2 at 0.6).</summary>
        public static float SensingUpgradePerLevel = 0.6f;

        /// <summary>The gradient sharpness actually used, after the buyable
        /// upgrade. `GradientSharpness · (1 + SensingUpgradeLevel · perLevel)`.</summary>
        public static float EffectiveSharpness =>
            GradientSharpness * (1f + SensingUpgradeLevel * SensingUpgradePerLevel);

        /// <summary>
        /// Picks the next fine-tile step among the in-bounds von Neumann
        /// neighbours of <paramref name="current"/>. Uniform when
        /// <paramref name="cytokineEnabled"/> is false (rung 1). Buffers
        /// are caller-owned (length >= 4) so this allocates nothing per
        /// call -- callers reuse the same arrays every tick/step.
        /// </summary>
        /// <summary>
        /// Sprint 28 -- **crowd desensitisation**, the anti-swarm.
        ///
        /// The problem it solves: macrophages and neutrophils all read the
        /// same cytokine gradient, so they all walked to the same pathogen
        /// and everything else strolled past untouched. The player saw one
        /// dogpile and a wide-open wall.
        ///
        /// What it is NOT: physical repulsion. Cells are not billiard
        /// balls, and a bounce would also break the cooperation the game
        /// just built. What this models is the real mechanism -- neutrophil
        /// swarming is self-limiting through GPCR desensitisation, a
        /// negative feedback that stops migration once a cell senses high
        /// concentrations of self-secreted attractant (Kienle 2021,
        /// Science). A crowded cell stops finding the crowd attractive.
        ///
        /// The shape matters and is deliberately forgiving at the start:
        /// <see cref="UnitTuning.RepulsionFreeCompanions"/> same-kind
        /// neighbours cost nothing, so three cells can converge on one
        /// target -- which is exactly the number Sprint 23's cooperation
        /// bonus rewards -- and only the surplus beyond that is penalised,
        /// rising as `surplus^exponent`.
        ///
        /// Pure and static so the shape can be asserted directly by a
        /// harness without a board or any GameObjects.
        /// </summary>
        public static float CrowdPenaltyAt(FineCoord at, FineCoord[] crowd, int count)
            => CrowdPenaltyAt(at, crowd, null, count);

        /// <summary>
        /// Crowd penalty with per-neighbour weights (1 for own kind, less
        /// for another kind). Pass null weights for all-own-kind.
        /// </summary>
        public static float CrowdPenaltyAt(FineCoord at, FineCoord[] crowd, float[] weights, int count)
        {
            if (crowd == null || count <= 0) return 0f;
            if (UnitTuning.RepulsionStrength <= 0f) return 0f;

            float radius = Mathf.Max(1f, UnitTuning.RepulsionRadiusFineTiles);

            // **Distance-weighted, not a head count.**
            //
            // Sprint 28 counted neighbours inside a radius and penalised the
            // surplus. That saturated: a cell one tile away counted exactly
            // as much as one six tiles away, so once a cell had stepped out
            // of the bucket there was nothing pushing it further, and the
            // four candidate steps of a single move all sat inside nearly
            // the same neighbourhood. Measured, raising the strength a
            // HUNDRED-fold changed nothing past a certain point -- cells
            // settled into tight triples and stopped.
            //
            // A falling kernel gives a real gradient at the scale of one
            // step: moving a single tile away measurably lowers the sum, so
            // cells keep easing apart instead of locking at a threshold.
            float crowding = 0f;
            for (int i = 0; i < count && i < crowd.Length; i++)
            {
                float dc = crowd[i].Column - at.Column;
                float dr = crowd[i].Row - at.Row;
                float d = Mathf.Sqrt(dc * dc + dr * dr);
                if (d >= radius) continue;

                float falloff = Mathf.Pow(1f - d / radius,
                                          Mathf.Max(0.05f, UnitTuning.RepulsionFalloffPower));
                float w = (weights != null && i < weights.Length) ? weights[i] : 1f;
                crowding += w * falloff;
            }

            // The free allowance is still what keeps cooperation alive: a
            // couple of close companions cost nothing, so three cells can
            // converge on one target -- the number Sprint 23's bonus
            // rewards -- and only the excess is pushed away.
            float surplus = crowding - Mathf.Max(0f, UnitTuning.RepulsionFreeCompanions);
            if (surplus <= 0f) return 0f;

            return UnitTuning.RepulsionStrength
                   * Mathf.Pow(surplus, Mathf.Max(0.01f, UnitTuning.RepulsionExponent));
        }

        public static FineCoord ChooseNextStep(
            FineCoord current,
            BoardConfig board,
            CytokineField cytokineField,
            bool cytokineEnabled,
            FineCoord[] candidateBuffer,
            float[] weightBuffer,
            FineCoord[] crowdBuffer = null,
            int crowdCount = 0,
            float[] crowdWeights = null)
        {
            int validCount = 0;
            float maxValue = float.NegativeInfinity;

            foreach (var offset in FineCoord.VonNeumannOffsets)
            {
                var candidate = current.Add(offset);
                if (!board.InFineBounds(candidate)) continue;

                candidateBuffer[validCount] = candidate;
                float value = cytokineEnabled ? cytokineField.SampleFine(candidate) : 0f;
                // Sprint 28: crowding is subtracted BEFORE the softmax, in
                // the same units as the cytokine reading, so a crowded
                // direction simply looks less attractive than an empty one
                // rather than being forbidden. A cell with nowhere better to
                // go will still go there.
                value -= CrowdPenaltyAt(candidate, crowdBuffer, crowdWeights, crowdCount);
                weightBuffer[validCount] = value; // raw value for now; exponentiated below
                if (value > maxValue) maxValue = value;
                validCount++;
            }

            if (validCount == 0) return current; // shouldn't happen on any board >= 2x2 fine tiles

            float totalWeight = 0f;
            float sharpness = EffectiveSharpness; // Sprint 12: base × the buyable upgrade
            for (int i = 0; i < validCount; i++)
            {
                weightBuffer[i] = Mathf.Exp(sharpness * (weightBuffer[i] - maxValue));
                totalWeight += weightBuffer[i];
            }

            float pick = Random.value * totalWeight;
            int chosen = validCount - 1;
            float running = 0f;
            for (int i = 0; i < validCount; i++)
            {
                running += weightBuffer[i];
                if (pick <= running) { chosen = i; break; }
            }

            return candidateBuffer[chosen];
        }
    }
}
