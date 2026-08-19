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

        /// <summary>
        /// Picks the next fine-tile step among the in-bounds von Neumann
        /// neighbours of <paramref name="current"/>. Uniform when
        /// <paramref name="cytokineEnabled"/> is false (rung 1). Buffers
        /// are caller-owned (length >= 4) so this allocates nothing per
        /// call -- callers reuse the same arrays every tick/step.
        /// </summary>
        public static FineCoord ChooseNextStep(
            FineCoord current,
            BoardConfig board,
            CytokineField cytokineField,
            bool cytokineEnabled,
            FineCoord[] candidateBuffer,
            float[] weightBuffer)
        {
            int validCount = 0;
            float maxValue = float.NegativeInfinity;

            foreach (var offset in FineCoord.VonNeumannOffsets)
            {
                var candidate = current.Add(offset);
                if (!board.InFineBounds(candidate)) continue;

                candidateBuffer[validCount] = candidate;
                float value = cytokineEnabled ? cytokineField.SampleFine(candidate) : 0f;
                weightBuffer[validCount] = value; // raw value for now; exponentiated below
                if (value > maxValue) maxValue = value;
                validCount++;
            }

            if (validCount == 0) return current; // shouldn't happen on any board >= 2x2 fine tiles

            float totalWeight = 0f;
            for (int i = 0; i < validCount; i++)
            {
                weightBuffer[i] = Mathf.Exp(GradientSharpness * (weightBuffer[i] - maxValue));
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
