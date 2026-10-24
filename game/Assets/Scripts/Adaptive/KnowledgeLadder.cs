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
            /// <summary>Sprint 21: whether the capability's mechanic exists.
            /// The knowledge % is really earned either way; the HUD shows an
            /// unlocked-but-unbuilt rung as "learned · planned" with the
            /// hatch rather than a solid pip, so it never promises an effect
            /// the game doesn't have (COSMETIC_PASS_SPEC.md §4.4). Flipping
            /// this to true when a capability lands is the whole UI change.</summary>
            public readonly bool Built;

            public Rung(KnowledgeCapability capability, float thresholdPercent, string shortName, bool built = false)
            {
                Capability = capability;
                ThresholdPercent = thresholdPercent;
                ShortName = shortName;
                Built = built;
            }
        }

        /// <summary>Sprint 21: the ~90% capstone (GAME_DESIGN.md §5) --
        /// antibody-coated pathogens become chemotactic beacons. Not a Rung
        /// (it has no KnowledgeCapability yet); the HUD draws it as a star at
        /// the end of every ladder, permanently "planned" for now.</summary>
        public const float CapstonePercent = 90f;

        /// <summary>
        /// The rungs, ascending by threshold. The `built` flag is what the
        /// HUD reads to decide whether a reached rung shows solid or with
        /// the "designed, not built" hatch.
        ///
        /// **Sprint 23 flipped three of them, one of which was overdue.**
        /// Secretory IgA shipped in Sprint 22 but its flag was never
        /// flipped, so the HUD went on calling it *planned* even once a
        /// species passed 70% and the mechanic was really running. That is
        /// worth remembering next to `PLAYTEST_LOG.md`'s open question about
        /// why the Director never perceived IgA: whatever else was true, the
        /// one piece of UI that would have told him it had arrived was
        /// saying the opposite.
        ///
        /// If you build a rung's mechanic, flip its flag in the same commit.
        /// `Sprint21Verification.RunHonesty` pins the exact set both ways --
        /// it fails if a built rung is still marked planned AND if an
        /// unbuilt one is marked built.
        /// </summary>
        public static readonly Rung[] Rungs =
        {
            new Rung(KnowledgeCapability.CytotoxicTCells,        10f, "CTL",    built: true),  // Sprint 23
            new Rung(KnowledgeCapability.NeutralizingAntibodies, 20f, "NeutAb", built: true),  // Sprint 23
            new Rung(KnowledgeCapability.MemoryTCells,           30f, "MemT"),
            new Rung(KnowledgeCapability.FcReceptor,             45f, "FcR"),
            new Rung(KnowledgeCapability.Complement,             60f, "Compl"),
            new Rung(KnowledgeCapability.SecretoryIgA,           70f, "IgA",    built: true),  // Sprint 22
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
