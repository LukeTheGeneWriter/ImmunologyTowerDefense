using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// GAME_DESIGN.md §5's "percentage representing how well the adaptive
    /// system has characterised a pathogen" -- tracked **per pathogen
    /// species, not globally** (§5). Species key is <see cref="PathogenClass"/>
    /// for now (no roster yet).
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
        // Indexed by (int)PathogenClass. Three classes today; a switch to a
        // real roster makes this a Dictionary keyed by species id.
        private readonly float[] knowledge = new float[3];

        /// <summary>Bumped on every change, so a poller (the HUD) can cheaply
        /// tell whether anything moved since last frame.</summary>
        public int Revision { get; private set; }

        /// <summary>Current knowledge of <paramref name="species"/>, 0..
        /// <see cref="AdaptiveTuning.KnowledgeMax"/>.</summary>
        public float Get(PathogenClass species) => knowledge[(int)species];

        /// <summary>Adds <paramref name="amount"/> percentage points (may be
        /// negative, e.g. a future mutation discount), clamped to
        /// 0..KnowledgeMax. Returns the new value.</summary>
        public float Add(PathogenClass species, float amount)
        {
            int i = (int)species;
            knowledge[i] = Mathf.Clamp(knowledge[i] + amount, 0f, AdaptiveTuning.KnowledgeMax);
            Revision++;
            return knowledge[i];
        }

        public void Reset()
        {
            for (int i = 0; i < knowledge.Length; i++) knowledge[i] = 0f;
            Revision++;
        }
    }
}
