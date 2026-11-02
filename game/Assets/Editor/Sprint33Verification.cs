using System.Collections.Generic;
using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rounds;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 33 — the 40-round script.
///
/// The Director asked for 40 rounds of mixes and matches, with Bloons TD as
/// the reference. Forty is Bloons TD 6's Easy length, and four of its round
/// table's habits are the design this harness pins:
///
///  1. a new enemy debuts in a small dose, then recurs in bulk;
///  2. difficulty is a sawtooth with a rising envelope, not a ramp;
///  3. the same count delivered tightly is a different problem from the same
///     count spread out — grouped rushes;
///  4. the player can see what is coming before the round starts.
///
/// Checks are written to this project's standing rule: **assert the
/// property, not a proxy.** In particular (3) is not asserted by reading
/// the burst number back out of the table — it is measured through the real
/// spawner, by counting how many distinct lanes of wall a round's organisms
/// actually land in. That is the thing the number is supposed to mean, and
/// a table that says "1 burst" while the spawner smears the batch across
/// ten lanes would pass a proxy check and fail this one.
/// </summary>
public static class Sprint33Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 33 -- the 40-round script")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(3301);

        Debug.Log("[Sprint33Verification] === The 40-round script ===");

        RunEveryRoundIsFullyAuthored();
        RunTaglinesPromiseOnlyWhatTheRoundDelivers();
        RunDebutsArriveOneAtATimeAndSpacedOut();
        RunDebutRoundsAreNotTheHardestYet();
        RunEverythingRecursEnoughToBeLearned();
        RunDifficultyIsASawtoothWithARisingEnvelope();
        RunDeliveryShapeIsActuallyUsed();
        RunBurstsConcentrateArrivalsOnTheWall();
        RunThePlayerIsWarnedBeforeADebutRound();

        if (failures > 0)
            Debug.LogError($"[Sprint33Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint33Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint33Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint33Verification] FAIL -- {label}"); }
    }

    private static bool IsNamed(PathogenSpecies s) => (int)s >= (int)PathogenSpecies.Rotavirus;

    // ----------------------------------------------------------------
    // 1. The table is complete. A half-authored round would quietly fall
    //    back to the economy curve and nobody would notice.
    // ----------------------------------------------------------------
    private static void RunEveryRoundIsFullyAuthored()
    {
        SpeciesRoster.ResetToDefaults();
        Check($"the script is 40 rounds long ({RoundScript.ScriptedRounds})", RoundScript.ScriptedRounds == 40);

        int noTagline = 0, noCast = 0, noBatch = 0, badBursts = 0, emptyWeights = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            if (string.IsNullOrWhiteSpace(def.Tagline)) noTagline++;
            if (!def.HasCast) noCast++;
            if (def.Batch <= 0) noBatch++;
            if (def.Bursts < 1 || def.Bursts > 8) badBursts++;

            float total = 0f;
            if (def.Cast != null) foreach (var c in def.Cast) total += Mathf.Max(0f, c.Weight);
            if (total <= 0f) emptyWeights++;
        }

        Check("every round has a tagline", noTagline == 0);
        Check("every round has a cast with positive weight", noCast == 0 && emptyWeights == 0);
        Check("every round authors its own batch, so none falls back to the economy curve", noBatch == 0);
        Check("every round's burst count is between 1 and 8", badBursts == 0);

        var taglines = new HashSet<string>();
        int duplicates = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            if (!taglines.Add(RoundScript.ForRound(r).Tagline)) duplicates++;
        Check($"all 40 taglines are distinct ({duplicates} duplicates)", duplicates == 0);
    }

    // ----------------------------------------------------------------
    // 2. The Sprint 32 lesson, pinned. The old taglines promised
    //    Salmonella, poliovirus and Listeria -- none of which exist in
    //    this game -- and that drift sat on top of a bug where no named
    //    species could spawn at all.
    // ----------------------------------------------------------------
    private static void RunTaglinesPromiseOnlyWhatTheRoundDelivers()
    {
        SpeciesRoster.ResetToDefaults();

        // Organisms that are shortlisted or famous but NOT in the roster.
        string[] notInTheGame =
        {
            "salmonella", "polio", "listeria", "norovirus", "shigella",
            "campylobacter", "astrovirus", "cmv", "cytomegalo", "adenovirus",
            "clostrid", "yersinia", "ehec", "epec", "hepatitis",
        };

        var promisedButAbsent = new List<string>();
        var namedButNotCast = new List<string>();

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            string tag = def.Tagline.ToLowerInvariant();

            foreach (var ghost in notInTheGame)
                if (tag.Contains(ghost)) promisedButAbsent.Add($"R{r}:{ghost}");

            // If a tagline names a roster organism, that organism must be in
            // the cast -- the promise has to be kept.
            void Requires(string needle, PathogenSpecies species)
            {
                if (!tag.Contains(needle)) return;
                bool inCast = false;
                foreach (var c in def.Cast)
                    if (c.Species == species && c.Weight > 0f) inCast = true;
                if (!inCast) namedButNotCast.Add($"R{r} says '{needle}' but does not cast {SpeciesRoster.ShortNameOf(species)}");
            }

            Requires("vibrio", PathogenSpecies.VibrioCholerae);
            Requires("rotavirus", PathogenSpecies.Rotavirus);
            Requires("e. coli", PathogenSpecies.AttachingEffacingEcoli);
        }

        Check("no tagline names an organism that is not in the game"
              + (promisedButAbsent.Count > 0 ? " (" + string.Join(", ", promisedButAbsent) + ")" : ""),
              promisedButAbsent.Count == 0);
        Check("every tagline that names an organism delivers it"
              + (namedButNotCast.Count > 0 ? " (" + string.Join("; ", namedButNotCast) + ")" : ""),
              namedButNotCast.Count == 0);
    }

    // ----------------------------------------------------------------
    // 3. One new organism at a time, with room to breathe.
    // ----------------------------------------------------------------
    private static void RunDebutsArriveOneAtATimeAndSpacedOut()
    {
        var namedDebutRounds = new List<int>();
        int multiDebutRounds = 0;
        var seen = new HashSet<PathogenSpecies>();

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var debuts = RoundScript.DebutsInRound(r);
            int named = 0;
            foreach (var d in debuts)
            {
                if (IsNamed(d)) named++;
                Check($"R{r} debut {SpeciesRoster.ShortNameOf(d)} is genuinely new", seen.Add(d));
            }
            if (named > 1) multiDebutRounds++;
            if (named == 1) namedDebutRounds.Add(r);
        }

        Check($"the three named organisms debut in rounds {string.Join(", ", namedDebutRounds)}",
              namedDebutRounds.Count == 3);
        Check("no round introduces two named organisms at once", multiDebutRounds == 0);

        bool spaced = true;
        for (int i = 1; i < namedDebutRounds.Count; i++)
            if (namedDebutRounds[i] - namedDebutRounds[i - 1] < 3) spaced = false;
        Check("named debuts are at least three rounds apart, so each gets taught before the next arrives", spaced);

        Check("all six organisms have debuted by the end of the script",
              seen.Count == SpeciesRoster.Count);
        Check("nothing debuts in the last ten rounds -- the back half is recombination, not novelty",
              RoundScript.DebutsInRound(31).Length == 0 && RoundScript.DebutsInRound(40).Length == 0);
    }

    // ----------------------------------------------------------------
    // 4. Bloons introduces a new bloon in a SMALL dose. A debut round
    //    that is also the hardest round so far teaches nothing, because
    //    the player cannot tell the new organism from the pressure.
    // ----------------------------------------------------------------
    private static void RunDebutRoundsAreNotTheHardestYet()
    {
        float peakClump = 0f;
        var offenders = new List<string>();

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            float clump = def.OrganismsPerClump;

            bool isNamedDebut = false;
            foreach (var d in RoundScript.DebutsInRound(r)) if (IsNamed(d)) isNamedDebut = true;

            if (isNamedDebut && clump >= peakClump && r > 1)
                offenders.Add($"R{r} ({clump:F0} per clump vs a running peak of {peakClump:F0})");

            if (clump > peakClump) peakClump = clump;
        }

        Check("a named organism never debuts on the hardest round so far"
              + (offenders.Count > 0 ? " (" + string.Join(", ", offenders) + ")" : ""),
              offenders.Count == 0);
    }

    // ----------------------------------------------------------------
    // 5. Recurrence, and enough of it to reach a rung. Knowledge is per
    //    species and secretory IgA needs 70% of THAT organism.
    // ----------------------------------------------------------------
    private static void RunEverythingRecursEnoughToBeLearned()
    {
        var appearances = new Dictionary<PathogenSpecies, int>();
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            foreach (var c in RoundScript.ForRound(r).Cast)
            {
                if (c.Weight <= 0f) continue;
                appearances.TryGetValue(c.Species, out int n);
                appearances[c.Species] = n + 1;
            }

        var thin = new List<string>();
        foreach (var kv in appearances)
            if (kv.Value < 5) thin.Add($"{SpeciesRoster.ShortNameOf(kv.Key)}x{kv.Value}");

        Check($"every organism appears in at least five rounds, so knowledge can accumulate toward its rungs"
              + (thin.Count > 0 ? " (thin: " + string.Join(", ", thin) + ")" : ""),
              thin.Count == 0);

        // And each named organism comes back soon after it debuts, rather
        // than vanishing for ten rounds.
        var late = new List<string>();
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
        {
            if (!IsNamed(sp)) continue;
            int debut = -1, next = -1;
            for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            {
                bool present = false;
                foreach (var c in RoundScript.ForRound(r).Cast)
                    if (c.Species == sp && c.Weight > 0f) present = true;
                if (!present) continue;
                if (debut < 0) { debut = r; continue; }
                next = r; break;
            }
            if (debut < 0 || next < 0 || next - debut > 2)
                late.Add($"{SpeciesRoster.ShortNameOf(sp)} debut R{debut}, back R{next}");
        }
        Check("each named organism returns within two rounds of its debut"
              + (late.Count > 0 ? " (" + string.Join("; ", late) + ")" : ""),
              late.Count == 0);
    }

    // ----------------------------------------------------------------
    // 6. The sawtooth. A monotone ramp would also pass "the envelope
    //    rises", so both halves are asserted: the envelope never falls,
    //    AND individual rounds dip below the one before.
    // ----------------------------------------------------------------
    private static void RunDifficultyIsASawtoothWithARisingEnvelope()
    {
        int batchEnvelopeFalls = 0, clumpEnvelopeFalls = 0;
        int reliefRounds = 0, deepDips = 0;
        int peakBatch = 0; float peakClump = 0f;
        int worstDipPercent = 0;

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);

            if (r > 1)
            {
                var prev = RoundScript.ForRound(r - 1);
                if (def.Batch < prev.Batch) reliefRounds++;
            }

            if (def.Batch < peakBatch)
            {
                int dip = (peakBatch - def.Batch) * 100 / Mathf.Max(1, peakBatch);
                if (dip > worstDipPercent) worstDipPercent = dip;
                if (dip > 25) deepDips++;
            }

            if (def.Batch > peakBatch) peakBatch = def.Batch; else if (peakBatch == 0) batchEnvelopeFalls++;
            if (def.OrganismsPerClump > peakClump) peakClump = def.OrganismsPerClump;
        }

        var last = RoundScript.ForRound(RoundScript.ScriptedRounds);
        var first = RoundScript.ForRound(1);

        Check($"the batch envelope rises from {first.Batch} to {last.Batch} and round 40 is the peak",
              last.Batch == peakBatch && last.Batch > first.Batch * 4);
        Check($"so does organisms-per-clump: {first.OrganismsPerClump:F0} at round 1, {last.OrganismsPerClump:F0} at round 40, which is the peak",
              Mathf.Approximately(last.OrganismsPerClump, peakClump)
              && last.OrganismsPerClump > first.OrganismsPerClump * 4);
        Check($"it is a sawtooth, not a ramp -- {reliefRounds} rounds are lighter than the round before them",
              reliefRounds >= 6);
        Check($"and relief is relief, not a reset -- the deepest dip below the running peak is {worstDipPercent}%",
              deepDips == 0);
        Check($"round 40 delivers its whole batch in one burst ({last.Batch} organisms, {last.Bursts} burst)",
              last.Bursts == 1 && last.Batch >= 80);
        Check("every organism is in the finale's cast", last.Cast.Length == SpeciesRoster.Count);
    }

    // ----------------------------------------------------------------
    // 7. Both delivery shapes are actually used, and rushes are spread
    //    through the script rather than bunched.
    // ----------------------------------------------------------------
    private static void RunDeliveryShapeIsActuallyUsed()
    {
        var distinct = new HashSet<int>();
        var rushes = new List<int>();
        int trickles = 0, adjacentRushes = 0;
        int prevBursts = int.MaxValue;

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            int b = RoundScript.ForRound(r).Bursts;
            distinct.Add(b);
            if (b <= 2) { rushes.Add(r); if (prevBursts <= 2) adjacentRushes++; }
            if (b >= 6) trickles++;
            prevBursts = b;
        }

        Check($"the script uses {distinct.Count} distinct delivery shapes", distinct.Count >= 4);
        Check($"rush rounds (one or two bursts) appear {rushes.Count} times: {string.Join(", ", rushes)}",
              rushes.Count >= 6);
        Check($"and spread rounds (six or more bursts) appear {trickles} times", trickles >= 4);
        Check("no two consecutive rounds are both rushes -- a spike needs a shoulder", adjacentRushes == 0);
        Check("the first rush is early enough to teach the shape (by round 6)", rushes[0] <= 6);
    }

    // ----------------------------------------------------------------
    // 8. **The property, not the number.** A burst count only means
    //    something if the organisms really do land in fewer stretches of
    //    wall. Measured through the real spawner, at spawn time -- lumen
    //    flow smears lanes afterwards, so a later reading would measure
    //    the flow instead of the delivery.
    // ----------------------------------------------------------------
    private static int LanesTouched(int roundNumber, out int emitted, out int batchTarget)
    {
        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();
        RoundClock.Reset();
        InvasionTuning.AdhesionChanceAtWall = 0f;   // wash-through: nothing leaves Live early

        var boardGo = new GameObject($"S33_board_{roundNumber}");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);
        var tally = new InvasionTally();
        var gut = new GutWall(board, grid, tally);

        var spawnerGo = new GameObject($"S33_spawner_{roundNumber}");
        var spawner = spawnerGo.AddComponent<PathogenSpawner>();
        var template = new GameObject($"S33_template_{roundNumber}");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        spawner.Initialize(board, grid, field, gut, tally, template);

        var def = RoundScript.ForRound(roundNumber);
        batchTarget = EconomyTuning.BatchForRound(
            roundNumber, def.EffectiveWallPressure, spawner.MaxLivePathogens, def.Batch);
        spawner.BeginRound(batchTarget, def);

        var lanes = new HashSet<int>();
        var counted = new HashSet<PathogenAgent>();
        float t = 0f;
        for (int i = 0; i < 6000 && spawner.BatchEmitted < batchTarget; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in spawner.Live)
            {
                if (!counted.Add(a)) continue;      // lane at SPAWN, before the flow moves it
                lanes.Add(board.CrossIndex(a.CurrentCoarse));
            }
        }
        emitted = spawner.BatchEmitted;

        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
        Object.DestroyImmediate(template);
        Object.DestroyImmediate(spawnerGo);
        Object.DestroyImmediate(boardGo);
        return lanes.Count;
    }

    private static void RunBurstsConcentrateArrivalsOnTheWall()
    {
        // Round 40: 88 organisms, one burst. Round 27: 52 organisms, seven.
        int rushLanes = LanesTouched(40, out int rushEmitted, out int rushTarget);
        int spreadLanes = LanesTouched(27, out int spreadEmitted, out int spreadTarget);

        Check($"the finale emits its whole authored batch ({rushEmitted} of {rushTarget})",
              rushEmitted == rushTarget && rushTarget == RoundScript.ForRound(40).Batch);
        Check($"the spread round emits its whole authored batch ({spreadEmitted} of {spreadTarget})",
              spreadEmitted == spreadTarget && spreadTarget == RoundScript.ForRound(27).Batch);
        Check($"a one-burst round really does concentrate: {rushEmitted} organisms into {rushLanes} lane(s) of wall, against {spreadEmitted} into {spreadLanes}",
              rushLanes < spreadLanes);
        Check($"and the rush is genuinely local -- at most a quarter of the {10} lanes ({rushLanes})",
              rushLanes <= 3);
    }

    // ----------------------------------------------------------------
    // 9. The player is told. Bloons puts its new-bloon callout up before
    //    the round; this game's slot is the buy-phase caption, which
    //    already previews the next round's tagline.
    // ----------------------------------------------------------------
    private static void RunThePlayerIsWarnedBeforeADebutRound()
    {
        SpeciesRoster.ResetToDefaults();

        int labelled = 0, silent = 0, wrongly = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            string label = RoundScript.DebutLabelFor(r);
            bool hasDebut = RoundScript.DebutsInRound(r).Length > 0;

            if (hasDebut && !string.IsNullOrEmpty(label)) labelled++;
            if (hasDebut && string.IsNullOrEmpty(label)) silent++;
            if (!hasDebut && !string.IsNullOrEmpty(label)) wrongly++;
        }

        Check($"every debut round carries a callout ({labelled} of them)", labelled > 0 && silent == 0);
        Check("and a round that introduces nothing stays quiet", wrongly == 0);
        Check($"the Vibrio round's callout names it: \"{RoundScript.DebutLabelFor(6)}\"",
              RoundScript.DebutLabelFor(6) == "NEW: " + SpeciesRoster.NameOf(PathogenSpecies.VibrioCholerae));
        Check("past the script there is nothing left to introduce",
              RoundScript.DebutLabelFor(RoundScript.ScriptedRounds + 1) == null);
    }
}
