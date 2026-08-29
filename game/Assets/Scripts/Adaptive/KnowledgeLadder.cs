using System.Collections.Generic;

namespace ImmunologyTD.Adaptive
{
    /// <summary>The six adaptive capabilities of GAME_DESIGN.md §5's
    /// threshold ladder — the Director's roster (2026-08-29).</summary>
    public enum KnowledgeCapability
    {
        CytotoxicTCells,        // ~10% -- precise kill of an infected cell, no collateral / no DAMP
        NeutralizingAntibodies, // ~20% -- reduced adhesion probability for a known species
        MemoryTCells,           // ~30% -- a burst of CTLs spawns when a known species is seen again
        FcReceptor,             // ~45% -- antibodies affix to innate cells (opsonisation)
        Complement,             // ~60% -- antibody destroys the target membrane, no cell needed
        SecretoryIgA,           // ~70% -- antibody into the lumen, acts before adhesion
    }

    /// <summary>
    /// GAME_DESIGN.md §5's knowledge-threshold ladder, as data
    /// (Sprint 11). Each rung is a capability and the per-species
    /// KNOWLEDGE % that unlocks it.
    ///
    /// **Sprint 11 is display-only.** <see cref="IsUnlocked"/> drives the
    /// HUD readout and nothing else — every capability's real mechanic
    /// (a CTL unit, antibody entities, IgA in the lumen, …) is a later
    /// sprint. The thresholds are §5's proposal values, still placeholder.
    /// </summary>
    public static class KnowledgeLadder
    {
        public readonly struct Rung
        {
            public readonly KnowledgeCapability Capability;
            public readonly float ThresholdPercent;
            public readonly string ShortName;

            public Rung(KnowledgeCapability capability, float thresholdPercent, string shortName)
            {
                Capability = capability;
                ThresholdPercent = thresholdPercent;
                ShortName = shortName;
            }
        }

        /// <summary>The rungs, ascending by threshold.</summary>
        public static readonly Rung[] Rungs =
        {
            new Rung(KnowledgeCapability.CytotoxicTCells,        10f, "CTL"),
            new Rung(KnowledgeCapability.NeutralizingAntibodies, 20f, "NeutAb"),
            new Rung(KnowledgeCapability.MemoryTCells,           30f, "MemT"),
            new Rung(KnowledgeCapability.FcReceptor,             45f, "FcR"),
            new Rung(KnowledgeCapability.Complement,             60f, "Compl"),
            new Rung(KnowledgeCapability.SecretoryIgA,           70f, "IgA"),
        };

        /// <summary>True once <paramref name="knowledgePercent"/> has reached
        /// the rung's threshold.</summary>
        public static bool IsUnlocked(KnowledgeCapability capability, float knowledgePercent)
        {
            for (int i = 0; i < Rungs.Length; i++)
                if (Rungs[i].Capability == capability)
                    return knowledgePercent >= Rungs[i].ThresholdPercent;
            return false;
        }

        /// <summary>How many rungs are unlocked at <paramref name="knowledgePercent"/>.</summary>
        public static int UnlockedCount(float knowledgePercent)
        {
            int n = 0;
            for (int i = 0; i < Rungs.Length; i++)
                if (knowledgePercent >= Rungs[i].ThresholdPercent) n++;
            return n;
        }

        public static IEnumerable<Rung> All()
        {
            for (int i = 0; i < Rungs.Length; i++) yield return Rungs[i];
        }
    }
}
