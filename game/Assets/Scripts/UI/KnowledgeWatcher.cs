using ImmunologyTD.Adaptive;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 21 (docs/COSMETIC_PASS_SPEC.md §4.3): the pure half of the
    /// knowledge HUD -- turns the ledger's numbers into the two feedback
    /// moments, by polling (UI_DESIGN.md §7, "poll, don't event"):
    ///  - a **match**: a species' knowledge rose (one DC/helper-T pairing
    ///    taught, +3% today);
    ///  - a **rung unlock**: its UnlockedCount rose;
    ///  - and a **loss** (a future mutation mechanic): it fell.
    ///
    /// No Unity types, no allocation after construction: <see cref="Poll"/>
    /// fills a fixed buffer. Public so Sprint21Verification can drive it
    /// headlessly; KnowledgeView consumes it.
    /// </summary>
    public sealed class KnowledgeWatcher
    {
        public enum Kind { Match, Unlock, Loss }

        public struct Event
        {
            public Kind Kind;
            public int Species;       // (int)PathogenClass
            public float Delta;       // percent points (Match / Loss)
            public int RungIndex;     // KnowledgeLadder.Rungs index (Unlock)
        }

        public const int Species = 3;
        public const int MaxEvents = 16;

        private readonly float[] last = new float[Species];
        private readonly int[] lastUnlocked = new int[Species];
        private int lastRevision = -1;

        public readonly Event[] Events = new Event[MaxEvents];
        public int EventCount { get; private set; }

        /// <summary>Seed from the ledger without firing anything (so a HUD
        /// built mid-run doesn't replay every match).</summary>
        public void Prime(KnowledgeLedger ledger)
        {
            for (int s = 0; s < Species; s++)
            {
                last[s] = ledger.Get((ImmunologyTD.Pathogens.PathogenClass)s);
                lastUnlocked[s] = KnowledgeLadder.UnlockedCount(last[s]);
            }
            lastRevision = ledger.Revision;
            EventCount = 0;
        }

        /// <summary>Diff against the last poll. Cheap early-out on the
        /// ledger's revision. Returns the number of events written.</summary>
        public int Poll(KnowledgeLedger ledger)
        {
            EventCount = 0;
            if (ledger == null) return 0;
            if (lastRevision < 0) { Prime(ledger); return 0; }
            if (ledger.Revision == lastRevision) return 0;
            lastRevision = ledger.Revision;

            for (int s = 0; s < Species; s++)
            {
                float now = ledger.Get((ImmunologyTD.Pathogens.PathogenClass)s);
                float d = now - last[s];
                if (d > 1e-4f) Push(Kind.Match, s, d, -1);
                else if (d < -1e-4f) Push(Kind.Loss, s, d, -1);

                int unlocked = KnowledgeLadder.UnlockedCount(now);
                for (int r = lastUnlocked[s]; r < unlocked; r++) Push(Kind.Unlock, s, 0f, r);

                last[s] = now;
                lastUnlocked[s] = unlocked;
            }
            return EventCount;
        }

        private void Push(Kind k, int species, float delta, int rung)
        {
            if (EventCount >= MaxEvents) return;
            Events[EventCount++] = new Event { Kind = k, Species = species, Delta = delta, RungIndex = rung };
        }
    }
}
