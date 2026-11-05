namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// **Sprint 35: what the player has actually met, and in what order.**
    ///
    /// Three surfaces need this one fact and none of them could ask for it
    /// before:
    ///
    ///  - the **knowledge ribbon** shows the five most recently seen
    ///    organisms in its visible slots (Director, 2026-11-05), so the
    ///    useful cards are the ones in front of you and the rest are a
    ///    scroll away;
    ///  - the **first-sighting banner** (`ROSTER_EXPANSION_SPEC.md` §4.5)
    ///    fires once per species per run;
    ///  - and the ribbon's dimming can finally mean "never seen" rather
    ///    than today's proxy, "knowledge is still zero" -- which is a
    ///    different statement, and wrong for any organism the player has
    ///    fought without ever sampling.
    ///
    /// **A sighting is the organism arriving on the board**, not the player
    /// noticing it. That is the honest meaning of the only event available;
    /// the alternative (a sighting means it entered the camera frustum)
    /// would be a lie dressed as precision, since the whole board is on
    /// screen at once.
    ///
    /// Static, pure, and no Unity types -- same shape as
    /// <c>InvasionTally</c>. It is run state, so
    /// <see cref="ResetForNewRun"/> is called from the bootstrap; a harness
    /// that forgets would see the previous test's history, so
    /// <see cref="Revision"/> is there to make staleness visible rather
    /// than silent.
    /// </summary>
    public static class SpeciesSightings
    {
        /// <summary>Monotonic counter: the order of sightings, not a clock.
        /// A frame time would make the data untestable headlessly and would
        /// tie the HUD's ordering to the frame rate.</summary>
        private static int stamp;

        private static readonly int[] lastSeen = new int[SpeciesRoster.Count];
        private static readonly int[] firstSeen = new int[SpeciesRoster.Count];
        private static readonly int[] seenCount = new int[SpeciesRoster.Count];

        /// <summary>Bumped on every change, so a polling view can early-out
        /// (UI_DESIGN.md §7, "poll, don't event").</summary>
        public static int Revision { get; private set; }

        static SpeciesSightings() { ResetForNewRun(); }

        public static void ResetForNewRun()
        {
            stamp = 0;
            for (int i = 0; i < lastSeen.Length; i++) { lastSeen[i] = 0; firstSeen[i] = 0; seenCount[i] = 0; }
            Revision++;
        }

        /// <summary>One organism of this species arrived on the board.</summary>
        public static void Record(PathogenSpecies species)
        {
            int i = (int)species;
            if (i < 0 || i >= lastSeen.Length) return;
            stamp++;
            lastSeen[i] = stamp;
            if (firstSeen[i] == 0) firstSeen[i] = stamp;
            seenCount[i]++;
            Revision++;
        }

        public static bool HasSeen(PathogenSpecies species)
        {
            int i = (int)species;
            return i >= 0 && i < lastSeen.Length && lastSeen[i] > 0;
        }

        /// <summary>The sighting order of this species' most recent arrival.
        /// 0 = never seen. Higher is more recent.</summary>
        public static int LastSeenOrder(PathogenSpecies species)
        {
            int i = (int)species;
            return i >= 0 && i < lastSeen.Length ? lastSeen[i] : 0;
        }

        /// <summary>The sighting order of this species' FIRST arrival, for
        /// "new to this run" ordering. 0 = never seen.</summary>
        public static int FirstSeenOrder(PathogenSpecies species)
        {
            int i = (int)species;
            return i >= 0 && i < firstSeen.Length ? firstSeen[i] : 0;
        }

        /// <summary>How many organisms of this species have arrived this
        /// run. Not displayed anywhere yet; it is here because the banner
        /// needs to fire on the transition 0 -> 1 and a count makes that
        /// testable without reading the banner.</summary>
        public static int CountSeen(PathogenSpecies species)
        {
            int i = (int)species;
            return i >= 0 && i < seenCount.Length ? seenCount[i] : 0;
        }

        public static int DistinctSeen()
        {
            int n = 0;
            for (int i = 0; i < lastSeen.Length; i++) if (lastSeen[i] > 0) n++;
            return n;
        }

        /// <summary>
        /// Every species, ordered for display: **most recently seen first**,
        /// then everything never seen in roster order.
        ///
        /// Writes into <paramref name="into"/> rather than allocating,
        /// because the knowledge HUD calls this from a polled refresh and
        /// the project's UI rule is no allocation per frame. Returns how
        /// many entries were written (always the roster size, but returned
        /// so a caller need not assume it).
        ///
        /// A pure function of the sighting data, deliberately separate from
        /// the view: the ordering is the part worth asserting, and a check
        /// that has to build a UI tree to test a sort is a check nobody
        /// writes.
        /// </summary>
        public static int OrderForDisplay(PathogenSpecies[] into)
        {
            if (into == null || into.Length < lastSeen.Length) return 0;

            int n = 0;
            for (int i = 0; i < lastSeen.Length; i++) into[n++] = (PathogenSpecies)i;

            // Insertion sort: 16 entries, called on a poll, and stable --
            // never-seen species keep roster order behind the seen ones.
            for (int i = 1; i < n; i++)
            {
                var v = into[i];
                int key = lastSeen[(int)v];
                int j = i - 1;
                while (j >= 0 && lastSeen[(int)into[j]] < key)
                {
                    into[j + 1] = into[j];
                    j--;
                }
                into[j + 1] = v;
            }
            return n;
        }
    }
}
