using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// GAME_DESIGN.md §5's "percentage representing how well the adaptive
    /// system has characterised a pathogen" -- tracked **per pathogen
    /// species, not globally** (§5).
    ///
    /// **Sprint 24 made that true.** The key was <see cref="PathogenClass"/>,
    /// of which there were three, so learning about "intracellular virus"
    /// taught you about every virus that would ever arrive -- and §5's
    /// stated consequence, that "a new species arriving in a late round
    /// meets a naive adaptive system", could not happen. The key is now
    /// <see cref="PathogenSpecies"/>.
    ///
    /// Plain reference type, per run, constructed by GameBootstrap and passed
    /// by reference -- same shape as <c>InvasionTally</c> / <c>AtpWallet</c>.
    /// A headless harness constructs one, drives the shuttle, reads it after.
    ///
    /// **Sprint 8 wires the number, not its consequences.** §5's threshold
    /// ladder (MHC-I precise kill at ~10%, neutralisation at ~20%, ...) is
    /// not built -- <see cref="Get"/> rising past a threshold unlocks
    /// nothing yet. That is the next sprint.
    /// </summary>
    public class KnowledgeLedger
    {
        // Indexed by (int)PathogenSpecies.
        private readonly float[] knowledge = new float[SpeciesRoster.Count];

        /// <summary>Bumped on every change, so a poller (the HUD) can cheaply
        /// tell whether anything moved since last frame.</summary>
        public int Revision { get; private set; }

        /// <summary>Current knowledge of <paramref name="species"/>, 0..
        /// <see cref="AdaptiveTuning.KnowledgeMax"/>.</summary>
        public float Get(PathogenSpecies species) => knowledge[(int)species];

        /// <summary>Adds <paramref name="amount"/> percentage points (may be
        /// negative, e.g. a future mutation discount), clamped to
        /// 0..KnowledgeMax. Returns the new value.</summary>
        public float Add(PathogenSpecies species, float amount)
        {
            int i = (int)species;
            knowledge[i] = Mathf.Clamp(knowledge[i] + amount, 0f, AdaptiveTuning.KnowledgeMax);
            Revision++;
            return knowledge[i];
        }

        /// <summary>The species the adaptive system knows most about, and by
        /// how much -- what a "which threat is characterised?" readout wants.</summary>
        public PathogenSpecies BestKnown(out float percent)
        {
            int best = 0;
            for (int i = 1; i < knowledge.Length; i++)
                if (knowledge[i] > knowledge[best]) best = i;
            percent = knowledge[best];
            return (PathogenSpecies)best;
        }

        public void Reset()
        {
            for (int i = 0; i < knowledge.Length; i++) knowledge[i] = 0f;
            Revision++;
        }
    }
}
