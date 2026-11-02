using System.Collections.Generic;
using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rounds;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 32 -- the band permit, and the round curriculum.
///
/// ## Part A: immune cells stay in the tissue
///
/// The Director reported immune cells drifting out of the tissue into the
/// blood at round start. The cause was structural, not balance: the
/// movement candidate loop rejected a step only on
/// <c>BoardConfig.InFineBounds</c> -- the whole board -- so nothing
/// whatsoever held a unit inside the tissue band. Pathogens have always
/// had the equivalent discipline in <c>StepTissue</c>.
///
/// These checks are written to the project's standing rule: **assert the
/// property, not a proxy** (docs/AGENT_HANDBOOK.md). Several tests in this
/// repo have passed for sprints while the thing they described was false,
/// because they checked a proxy a random walk would satisfy anyway. So:
///
///  - the permitted candidate SET is enumerated directly, not sampled;
///  - the no-excursion claim is run over the REAL
///    <c>Chemotaxis.ChooseNextStep</c> for tens of thousands of steps,
///    not over a reimplementation of it;
///  - the stranding guard asserts the thing that would actually hurt --
///    that a cell already in the blood gets *strictly closer* to tissue
///    every step, which a "it eventually gets back" check would not
///    distinguish from luck;
///  - and the base-flipped board is included, because a fix that quietly
///    assumed "leftward" would pass every other check here.
///
/// ## Part B: the named species actually spawn
///
/// Sprints 24-25 added rotavirus, Vibrio cholerae and A/E E. coli. Sprint
/// 32 found that none of them could ever appear in play: rounds rolled a
/// <c>PathogenClass</c>, and a bare class resolves to the GENERIC species
/// of that class. The three organisms existed, had working traits, had
/// rows in the knowledge HUD, and were unreachable. These checks tally
/// what a round would really emit, species by species, so that failure
/// mode cannot come back silently.
/// </summary>
public static class Sprint32Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 32 -- band permit + round curriculum")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(3202);

        Debug.Log("[Sprint32Verification] === Band permit + round curriculum ===");

        // -- Part A: the band permit --
        RunPermittedSetExcludesOtherBands();
        RunTissueOnlyWalkNeverLeavesTissue();
        RunBlindWalkWithoutPermitDoesLeave();
        RunStrandedCellWalksHomeMonotonically();
        RunCommutingKindMayEnterBase();
        RunUnrestrictedIsExactlyTheOldRule();
        RunPermitHoldsOnAFlippedBoard();

        // -- Part B: the named species reach the board --
        RunRoundCastRollsTheSpeciesItNames();
        RunEverySpeciesIsReachableFromTheScript();
        RunSpeciesRecurSoKnowledgeCanAccumulate();
        RunWallPressureRises();
        RunClassOnlyRoundsStillBehaveAsBefore();
        RunRealSpawnerEmitsNamedSpecies();
        RunBatchTargetStaysInsideWhatTheBoardCanHold();

        // -- Part C: innate cells turn over --
        RunTurnoverBindsAtTheNewLimits();
        RunTurnoverRecoversTheStandingPopulation();
        RunTheKillLimitRowChangesTheRate();

        if (failures > 0)
            Debug.LogError($"[Sprint32Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint32Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint32Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint32Verification] FAIL -- {label}"); }
    }

    private static BoardConfig Build(string name, AxisEnd baseEnd = AxisEnd.Negative)
    {
        var go = new GameObject($"S32_{name}");
        var board = go.AddComponent<BoardConfig>();
        // The real shipped geometry: 25x10, 6 base / 13 tissue / 6 lumen.
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, baseEnd, 6, 6, AxisEnd.Positive);
        return board;
    }

    private static void Dispose(BoardConfig board)
    {
        if (board != null) Object.DestroyImmediate(board.gameObject);
    }

    /// <summary>Entry tile: the centre of the tissue cell nearest the base,
    /// which is where BoneMarrowManager really puts an emitted unit and
    /// therefore where the reported bug started from.</summary>
    private static FineCoord EntryTile(BoardConfig board, int lane) =>
        board.CoarseCenterFine(board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex, lane));

    // ----------------------------------------------------------------
    // A1. The permitted set, enumerated -- no sampling, no walk.
    // ----------------------------------------------------------------
    private static void RunPermittedSetExcludesOtherBands()
    {
        var board = Build("permitted_set");
        var buf = new FineCoord[4];

        int offending = 0;
        int examined = 0;
        int withCandidates = 0;

        // EVERY fine tile of the tissue band, not a sample of them.
        for (int axis = board.TissueBaseEdgeAxisIndex; axis <= board.TissueLumenEdgeAxisIndex; axis++)
        {
            for (int lane = 0; lane < board.CrossLength; lane++)
            {
                var centre = board.CoarseCenterFine(board.CoarseFromAxis(axis, lane));
                for (int dc = -3; dc <= 3; dc++)
                {
                    for (int dr = -3; dr <= 3; dr++)
                    {
                        var tile = new FineCoord(centre.Column + dc, centre.Row + dr);
                        if (!board.InFineBounds(tile)) continue;
                        if (board.BandOfFine(tile) != BoardBand.Tissue) continue;

                        examined++;
                        int n = Chemotaxis.CollectCandidates(tile, board, buf, BandPermit.TissueOnly);
                        if (n > 0) withCandidates++;
                        for (int i = 0; i < n; i++)
                            if (board.BandOfFine(buf[i]) != BoardBand.Tissue) offending++;
                    }
                }
            }
        }

        Check($"every fine tile of the tissue band was examined ({examined} tiles)", examined > 5000);
        Check("no tissue-only candidate step leaves the tissue band (enumerated, 0 offending)",
              offending == 0);
        Check("and no tile is a dead end -- every one offers at least one legal step",
              withCandidates == examined);
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A2. The property the Director reported, over the real algorithm.
    // ----------------------------------------------------------------
    private static void RunTissueOnlyWalkNeverLeavesTissue()
    {
        var board = Build("walk_tissue_only");
        var field = new CytokineField(board);
        var cand = new FineCoord[4];
        var wgt = new float[4];

        const int Units = 40;
        const int Steps = 2000;  // 40 x 2000 = 80,000 real steps
        int excursions = 0;
        int baseEdgeVisits = 0;
        long depthSum = 0;       // coarse cells inward from the base edge
        int deepest = 0;

        for (int u = 0; u < Units; u++)
        {
            // Round start: at the base-side edge of tissue, no gradient yet
            // -- exactly the condition the bug appeared under.
            var pos = EntryTile(board, u % board.CrossLength);
            for (int s = 0; s < Steps; s++)
            {
                pos = Chemotaxis.ChooseNextStep(
                    pos, board, field, false, cand, wgt, null, 0, null, BandPermit.TissueOnly);

                var band = board.BandOfFine(pos);
                if (band != BoardBand.Tissue) excursions++;

                int axis = board.FineAxisIndex(pos) / BoardConfig.FineSubdivision;
                int depth = axis - board.TissueBaseEdgeAxisIndex;
                depthSum += depth;
                if (depth > deepest) deepest = depth;
                if (depth == 0) baseEdgeVisits++;
            }
        }

        float meanDepth = depthSum / (float)(Units * Steps);

        Check($"80,000 real steps from the round-start entry tile, zero left the tissue ({excursions} excursions)",
              excursions == 0);
        // The cage check. A constraint that worked by freezing cells against
        // the band edge would also score zero excursions, so this has to
        // prove they still RANGE. Stated as diffusion rather than full
        // traversal on purpose: an unbiased walk covers distance like
        // sqrt(steps), so crossing the 91 fine tiles of the band needs
        // ~8,000 steps and "did anyone reach the far edge" would be a test
        // of the step budget, not of the permit.
        Check($"and they diffuse inward instead of piling on the edge -- mean depth {meanDepth:F1} cells, deepest {deepest} of 13, {baseEdgeVisits * 100f / (Units * Steps):F0}% of steps on the edge cell",
              meanDepth >= 1.5f && deepest >= 6 && baseEdgeVisits < Units * Steps * 0.6f);
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A3. The same walk WITHOUT the permit must fail -- otherwise A2
    //     proves nothing about the fix.
    // ----------------------------------------------------------------
    private static void RunBlindWalkWithoutPermitDoesLeave()
    {
        var board = Build("walk_unpermitted");
        var field = new CytokineField(board);
        var cand = new FineCoord[4];
        var wgt = new float[4];

        int excursions = 0;
        for (int u = 0; u < 40; u++)
        {
            var pos = EntryTile(board, u % board.CrossLength);
            for (int s = 0; s < 2000; s++)   // same budget as A2, so the two numbers compare
            {
                pos = Chemotaxis.ChooseNextStep(pos, board, field, false, cand, wgt);
                if (board.BandOfFine(pos) != BoardBand.Tissue) excursions++;
            }
        }

        Check($"the pre-Sprint-32 rule really does leak cells out of the tissue ({excursions} excursions) -- so A2 is testing the fix",
              excursions > 0);
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A4. Nobody gets stranded, and homing is monotone, not lucky.
    // ----------------------------------------------------------------
    private static void RunStrandedCellWalksHomeMonotonically()
    {
        var board = Build("stranded");
        var field = new CytokineField(board);
        var cand = new FineCoord[4];
        var wgt = new float[4];

        int stranded = 0;
        int nonMonotone = 0;
        int worstSteps = 0;

        // Start in the blood -- the outermost base cell, the far side of
        // where the bug used to leave them.
        for (int lane = 0; lane < board.CrossLength; lane++)
        {
            var pos = board.CoarseCenterFine(board.CoarseFromAxis(0, lane));
            int prev = board.FineDistanceToPermittedBand(pos, BandPermit.TissueOnly);
            int steps = 0;
            bool home = false;

            for (; steps < 400; steps++)
            {
                pos = Chemotaxis.ChooseNextStep(
                    pos, board, field, false, cand, wgt, null, 0, null, BandPermit.TissueOnly);
                int d = board.FineDistanceToPermittedBand(pos, BandPermit.TissueOnly);

                if (d > 0 && d >= prev) nonMonotone++;   // still outside and no closer
                prev = d;
                if (d == 0) { home = true; break; }
            }

            if (!home) stranded++;
            if (steps > worstSteps) worstSteps = steps;
        }

        Check($"a cell placed in the blood always walks back into the tissue (0 stranded of {board.CrossLength} lanes)",
              stranded == 0);
        Check($"and it gets strictly closer every step while outside -- not by luck ({nonMonotone} non-monotone steps)",
              nonMonotone == 0);
        Check($"worst case {worstSteps} steps, i.e. seconds not minutes", worstSteps > 0 && worstSteps <= 60);
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A5. The permit is per-kind. The lymph node is in the base band, so
    //     a commuting kind must still be able to get there.
    // ----------------------------------------------------------------
    private static void RunCommutingKindMayEnterBase()
    {
        var board = Build("commuter");
        var buf = new FineCoord[4];

        // One fine tile inside the base band, adjacent to tissue.
        var tissueEdge = board.CoarseCenterFine(
            board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex, 4));
        var baseSide = board.CoarseCenterFine(
            board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex - 1, 4));

        int tissueOnlyAdmits = 0, commuterAdmits = 0;
        int n = Chemotaxis.CollectCandidates(tissueEdge, board, buf, BandPermit.TissueOnly);
        for (int i = 0; i < n; i++) if (board.BandOfFine(buf[i]) == BoardBand.Base) tissueOnlyAdmits++;

        // Walk the commuter from the tissue edge to the base tile and
        // confirm the base is reachable rather than merely "allowed".
        var field = new CytokineField(board);
        var cand = new FineCoord[4];
        var wgt = new float[4];
        var pos = tissueEdge;
        bool reachedBase = false;
        for (int s = 0; s < 4000 && !reachedBase; s++)
        {
            pos = Chemotaxis.ChooseNextStep(
                pos, board, field, false, cand, wgt, null, 0, null, BandPermit.TissueAndBase);
            if (board.BandOfFine(pos) == BoardBand.Base) reachedBase = true;
            if (board.BandOfFine(pos) == BoardBand.Lumen) commuterAdmits++;   // must stay 0
        }

        Check("a tissue-only kind is offered no step into the base band", tissueOnlyAdmits == 0);
        Check("a commuting kind reaches the base band, where the lymph node is", reachedBase);
        Check("and a commuting kind still never reaches the lumen", commuterAdmits == 0);
        Check("the base tile itself is permitted for a commuter, refused for a tissue-only kind",
              board.BandPermitted(baseSide, BandPermit.TissueAndBase)
              && !board.BandPermitted(baseSide, BandPermit.TissueOnly));
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A6. Zero means unrestricted -- the back-compatibility contract for
    //     node movement and for any profile saved before the field existed.
    // ----------------------------------------------------------------
    private static void RunUnrestrictedIsExactlyTheOldRule()
    {
        var board = Build("unrestricted");
        var buf = new FineCoord[4];
        int mismatches = 0;
        int sampled = 0;

        for (int col = 0; col < board.FineColumns; col += 3)
        {
            for (int row = 0; row < board.FineRows; row += 3)
            {
                var tile = new FineCoord(col, row);
                int inBounds = 0;
                foreach (var off in FineCoord.VonNeumannOffsets)
                    if (board.InFineBounds(tile.Add(off))) inBounds++;

                int n = Chemotaxis.CollectCandidates(tile, board, buf, BandPermit.Unrestricted);
                if (n != inBounds) mismatches++;
                sampled++;
            }
        }

        Check($"Unrestricted admits exactly the in-bounds neighbours, nothing more or less ({sampled} tiles)",
              mismatches == 0 && sampled > 100);
        Dispose(board);
    }

    // ----------------------------------------------------------------
    // A7. The base-direction abstraction. A fix that assumed "leftward"
    //     would pass every check above.
    // ----------------------------------------------------------------
    private static void RunPermitHoldsOnAFlippedBoard()
    {
        var board = Build("flipped", AxisEnd.Positive);
        var field = new CytokineField(board);
        var cand = new FineCoord[4];
        var wgt = new float[4];

        int excursions = 0;
        for (int u = 0; u < 20; u++)
        {
            var pos = EntryTile(board, u % board.CrossLength);
            for (int s = 0; s < 400; s++)
            {
                pos = Chemotaxis.ChooseNextStep(
                    pos, board, field, false, cand, wgt, null, 0, null, BandPermit.TissueOnly);
                if (board.BandOfFine(pos) != BoardBand.Tissue) excursions++;
            }
        }

        // And homing still points the right way with the base on the far side.
        var stray = board.CoarseCenterFine(board.CoarseFromAxis(0, 3));
        int d0 = board.FineDistanceToPermittedBand(stray, BandPermit.TissueOnly);
        var next = Chemotaxis.ChooseNextStep(
            stray, board, field, false, cand, wgt, null, 0, null, BandPermit.TissueOnly);
        int d1 = board.FineDistanceToPermittedBand(next, BandPermit.TissueOnly);

        Check($"with the base end flipped, tissue-only still never leaves the tissue ({excursions} excursions)",
              excursions == 0);
        Check("and a stray cell still homes toward the tissue, not toward a hardcoded direction",
              d0 > 0 && d1 < d0);
        Dispose(board);
    }

    // ================================================================
    // Part B -- the named species actually reach the board.
    // ================================================================

    /// <summary>Tally of what <paramref name="def"/> really rolls, over
    /// enough draws that a missing species is a fact and not variance.</summary>
    private static Dictionary<PathogenSpecies, int> TallyRolls(RoundDefinition def, int draws = 20000)
    {
        var tally = new Dictionary<PathogenSpecies, int>();
        for (int i = 0; i < draws; i++)
        {
            var sp = def.RollSpecies();
            tally.TryGetValue(sp, out int n);
            tally[sp] = n + 1;
        }
        return tally;
    }

    // ----------------------------------------------------------------
    // B1. A round rolls exactly the cast it names -- both directions.
    // ----------------------------------------------------------------
    private static void RunRoundCastRollsTheSpeciesItNames()
    {
        SpeciesRoster.ResetToDefaults();
        int mismatched = 0;
        var report = new System.Text.StringBuilder();

        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            var tally = TallyRolls(def);

            // Everything rolled was cast ...
            foreach (var kv in tally)
            {
                bool inCast = false;
                foreach (var c in def.Cast) if (c.Species == kv.Key) inCast = true;
                if (!inCast) { mismatched++; report.Append($" R{r} rolled uncast {kv.Key};"); }
            }
            // ... and everything cast with weight was rolled.
            foreach (var c in def.Cast)
            {
                if (c.Weight <= 0f) continue;
                if (!tally.ContainsKey(c.Species)) { mismatched++; report.Append($" R{r} never rolled cast {c.Species};"); }
            }
        }

        Check($"every scripted round rolls exactly the organisms it casts, no more and no fewer{report}",
              mismatched == 0);

        // And the class a caller sees is DERIVED, so it cannot disagree with
        // the species -- the exact mismatch that let the taglines drift.
        var r3 = RoundScript.ForRound(3);
        int disagreements = 0;
        for (int i = 0; i < 5000; i++)
        {
            var cls = r3.RollClass();
            bool possible = false;
            foreach (var c in r3.Cast) if (SpeciesRoster.ClassOf(c.Species) == cls) possible = true;
            if (!possible) disagreements++;
        }
        Check("the class a round reports is always the class of an organism in its cast",
              disagreements == 0);
    }

    // ----------------------------------------------------------------
    // B2. THE regression: all six species must be reachable in play.
    // ----------------------------------------------------------------
    private static void RunEverySpeciesIsReachableFromTheScript()
    {
        SpeciesRoster.ResetToDefaults();
        var seen = new HashSet<PathogenSpecies>();
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            foreach (var kv in TallyRolls(RoundScript.ForRound(r), 4000)) seen.Add(kv.Key);

        var missing = new List<string>();
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
            if (!seen.Contains(sp)) missing.Add(SpeciesRoster.NameOf(sp));

        Check($"every species in the roster can appear in the scripted rounds ({seen.Count} of {SpeciesRoster.Count}" +
              (missing.Count > 0 ? "; missing " + string.Join(", ", missing) : "") + ")",
              missing.Count == 0);

        // Named-species-specific, because this is the bug that shipped:
        Check("rotavirus is reachable", seen.Contains(PathogenSpecies.Rotavirus));
        Check("Vibrio cholerae is reachable", seen.Contains(PathogenSpecies.VibrioCholerae));
        Check("A/E E. coli is reachable", seen.Contains(PathogenSpecies.AttachingEffacingEcoli));
    }

    // ----------------------------------------------------------------
    // B3. The curriculum rule: organisms RECUR. Knowledge is per species
    //     and the adaptive rungs are gated on it, so a parade of
    //     novelties would keep every species below its rungs forever.
    // ----------------------------------------------------------------
    private static void RunSpeciesRecurSoKnowledgeCanAccumulate()
    {
        var appearances = new Dictionary<PathogenSpecies, int>();
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            foreach (var c in def.Cast)
            {
                if (c.Weight <= 0f) continue;
                appearances.TryGetValue(c.Species, out int n);
                appearances[c.Species] = n + 1;
            }
        }

        var onceOnly = new List<string>();
        foreach (var kv in appearances)
            if (kv.Value < 2) onceOnly.Add($"{SpeciesRoster.ShortNameOf(kv.Key)}x{kv.Value}");

        Check("no organism appears in only one round -- knowledge can accumulate toward its rungs"
              + (onceOnly.Count > 0 ? " (once-only: " + string.Join(", ", onceOnly) + ")" : ""),
              onceOnly.Count == 0);

        // One new organism at a time, never two.
        var introducedBy = new Dictionary<PathogenSpecies, int>();
        int worstIntroCount = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            int newThisRound = 0;
            foreach (var c in RoundScript.ForRound(r).Cast)
            {
                if (c.Weight <= 0f) continue;
                if (introducedBy.ContainsKey(c.Species)) continue;
                introducedBy[c.Species] = r;
                newThisRound++;
            }
            // Round 1 and 2 establish the three generic classes; the rule
            // being asserted is about NAMED organisms arriving one at a time.
            int newNamed = 0;
            foreach (var kv in introducedBy)
                if (kv.Value == r && (int)kv.Key >= (int)PathogenSpecies.Rotavirus) newNamed++;
            if (newNamed > worstIntroCount) worstIntroCount = newNamed;
        }

        Check($"never more than one NEW named organism in a round (worst round introduces {worstIntroCount})",
              worstIntroCount <= 1);
    }

    // ----------------------------------------------------------------
    // B4. Wall pressure: the lever for the Sprint 22 IgA miss.
    // ----------------------------------------------------------------
    private static void RunWallPressureRises()
    {
        Check("rounds 1 and 2 are unscaled, so the economy harnesses' batch numbers still hold",
              Mathf.Approximately(RoundScript.ForRound(1).EffectiveWallPressure, 1f)
              && Mathf.Approximately(RoundScript.ForRound(2).EffectiveWallPressure, 1f));

        bool monotone = true;
        float prev = 0f;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            float p = RoundScript.ForRound(r).EffectiveWallPressure;
            if (p < prev) monotone = false;
            prev = p;
        }
        Check("wall pressure never falls across the script", monotone);
        // **Restated in Sprint 33.** This check used to assert that the
        // last scripted round carried a pressure multiplier above 1.4x.
        // That was true of the six-round script, where pressure WAS the
        // difficulty curve. The 40-round script authors each round's count
        // outright (RoundDefinition.Batch), because the multiplier times the
        // economy baseline hits the batch ceiling at round 8 and would have
        // flattened rounds 8-40 to an identical 90 organisms. Pressure is
        // now the procedural tail's lever only, and the scripted curve is
        // asserted in Sprint33Verification -- on the envelope of
        // organisms-per-clump, which is what pressure at the wall actually
        // means. Weakening a check silently is how dead features ship, so:
        // the claim moved, it did not disappear.
        int authored = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            if (RoundScript.ForRound(r).Batch > 0) authored++;
        Check($"every one of the {RoundScript.ScriptedRounds} scripted rounds authors its own count, so the pressure multiplier is not the scripted curve any more ({authored} authored)",
              authored == RoundScript.ScriptedRounds);

        // The procedural tail climbs and then stops climbing.
        float at50 = RoundScript.ForRound(50).EffectiveWallPressure;
        float at200 = RoundScript.ForRound(200).EffectiveWallPressure;
        Check($"the procedural tail is capped, not unbounded ({at50:F1}x at round 50, {at200:F1}x at round 200)",
              at50 <= RoundScript.MaxProceduralWallPressure + 0.001f
              && Mathf.Approximately(at50, at200));
        Check("and the tail still carries the full roster, not a reduced mix",
              RoundScript.ForRound(50).HasCast && RoundScript.ForRound(50).Cast.Length >= 6);
    }

    // ----------------------------------------------------------------
    // B5. Back-compat: a class-weighted definition behaves as it did in
    //     Sprint 9, which is what two older harnesses build.
    // ----------------------------------------------------------------
    private static void RunClassOnlyRoundsStillBehaveAsBefore()
    {
        SpeciesRoster.ResetToDefaults();
        var virusOnly = new RoundDefinition { VirusWeight = 1f, BacteriumWeight = 0f, LargeBacteriumWeight = 0f };

        bool allVirusClass = true, allGenericSpecies = true;
        for (int i = 0; i < 2000; i++)
        {
            if (virusOnly.RollClass() != PathogenClass.IntracellularVirus) allVirusClass = false;
            if (virusOnly.RollSpecies() != PathogenSpecies.GenericVirus) allGenericSpecies = false;
        }

        var zero = new RoundDefinition();
        Check("a class-weighted round still rolls that class", allVirusClass);
        Check("and resolves to the generic species of it, exactly as before Sprint 32", allGenericSpecies);
        Check("an all-zero definition still falls back to the large bacterium",
              zero.RollClass() == PathogenClass.LargeBacterium);
        Check("and a definition with no wall pressure set means 1.0, not 0",
              Mathf.Approximately(zero.EffectiveWallPressure, 1f));
    }

    // ----------------------------------------------------------------
    // B6. The end-to-end property, through the REAL spawner. B1-B5 test
    //     the round table; this tests what lands on the board, which is
    //     the thing that was false for eight sprints.
    // ----------------------------------------------------------------
    private static void RunRealSpawnerEmitsNamedSpecies()
    {
        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();
        RoundClock.Reset();

        var boardGo = new GameObject("S32_spawner_board");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);
        var tally = new InvasionTally();
        var gut = new GutWall(board, grid, tally);

        var spawnerGo = new GameObject("S32_spawner");
        var spawner = spawnerGo.AddComponent<PathogenSpawner>();
        var template = new GameObject("S32_pathogen_template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        spawner.Initialize(board, grid, field, gut, tally, template);

        // **Sprint 33: find the round, do not name it.** This check used to
        // hard-code round 5, which was the A/E E. coli round in the
        // six-round script and is a generic rush in the forty-round one --
        // so it failed on a table edit that had nothing to do with what it
        // tests. The same mistake Sprint18Verification made with the
        // upgrade numbers. Pick the first round that casts both of the
        // named organisms this check is about, and pass the round's
        // AUTHORED batch, which is what the game now delivers.
        //
        // Adhesion is off so the batch washes through and nothing leaves
        // `Live` before it is counted.
        InvasionTuning.AdhesionChanceAtWall = 0f;

        int roundNumber = -1;
        for (int r = 1; r <= RoundScript.ScriptedRounds && roundNumber < 0; r++)
        {
            bool hasAeec = false, hasRota = false;
            foreach (var c in RoundScript.ForRound(r).Cast)
            {
                if (c.Weight <= 0f) continue;
                if (c.Species == PathogenSpecies.AttachingEffacingEcoli) hasAeec = true;
                if (c.Species == PathogenSpecies.Rotavirus) hasRota = true;
            }
            if (hasAeec && hasRota) roundNumber = r;
        }

        var def = RoundScript.ForRound(roundNumber);
        int batch = EconomyTuning.BatchForRound(
            roundNumber, def.EffectiveWallPressure, spawner.MaxLivePathogens, def.Batch);
        spawner.BeginRound(batch, def);

        var seen = new Dictionary<PathogenSpecies, int>();
        float t = 0f;
        var counted = new HashSet<PathogenAgent>();
        for (int i = 0; i < 4000 && spawner.BatchEmitted < batch; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in spawner.Live)
            {
                if (!counted.Add(a)) continue;
                seen.TryGetValue(a.Species, out int n);
                seen[a.Species] = n + 1;
            }
        }

        var names = new List<string>();
        foreach (var kv in seen) names.Add($"{SpeciesRoster.ShortNameOf(kv.Key)} {kv.Value}");
        names.Sort();

        int namedOnTheBoard = 0;
        foreach (var kv in seen) if ((int)kv.Key >= (int)PathogenSpecies.Rotavirus) namedOnTheBoard += kv.Value;

        Check($"the real spawner emitted the whole authored batch of round {roundNumber} ({spawner.BatchEmitted} of {batch})",
              roundNumber > 0 && spawner.BatchEmitted == batch && batch == def.Batch);
        Check($"and what it put on the board is named organisms, not generics stamped out of a class: {string.Join(", ", names)}",
              seen.ContainsKey(PathogenSpecies.AttachingEffacingEcoli));
        Check("rotavirus rode along in the same batch", seen.ContainsKey(PathogenSpecies.Rotavirus));
        Check($"and the named organisms are the majority of it ({namedOnTheBoard} of {spawner.BatchEmitted}), as that round's cast weights say",
              namedOnTheBoard > spawner.BatchEmitted / 2);
        Check("and every organism on the board was cast by the round",
              AllCast(seen, def));

        // The pre-Sprint-32 behaviour, for contrast: a class-only round
        // really does put nothing but generics on the board.
        var classOnly = new RoundDefinition { VirusWeight = 0f, BacteriumWeight = 0f, LargeBacteriumWeight = 1f };
        spawner.BeginRound(8, classOnly);
        var seen2 = new HashSet<PathogenSpecies>();
        for (int i = 0; i < 4000 && spawner.BatchEmitted < 8; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in spawner.Live) seen2.Add(a.Species);
        }
        Check("a class-only round still delivers only generics -- the shape of the bug, pinned",
              seen2.Contains(PathogenSpecies.GenericLargeBacterium));

        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
        Object.DestroyImmediate(template);
        Object.DestroyImmediate(spawnerGo);
        Object.DestroyImmediate(boardGo);
    }


    // ----------------------------------------------------------------
    // B7. A round's batch target must stay inside what the board can
    //     hold, or the round never ends.
    //
    //     PathogenSpawner.SpawnFromFood only emits while
    //     `live.Count < maxLivePathogens`, and a food round completes only
    //     once `batchEmitted >= batchTarget` AND the food has exited. A
    //     target above the ceiling is therefore a target never reached --
    //     a hang, not a difficulty spike. This is a PRE-EXISTING hazard
    //     (the raw curve reaches the ceiling on its own) that Sprint 32's
    //     wall pressure would have pulled forward into ordinary play.
    // ----------------------------------------------------------------
    private static void RunBatchTargetStaysInsideWhatTheBoardCanHold()
    {
        EconomyTuning.ResetToDefaults();
        const int LiveCap = 150;   // PathogenSpawner's shipped ceiling
        int ceiling = Mathf.FloorToInt(LiveCap * EconomyTuning.MaxBatchShareOfLiveCap);

        int overCeiling = 0, underOne = 0;
        int firstRawCrossing = -1;
        for (int r = 1; r <= 80; r++)
        {
            var def = RoundScript.ForRound(r);
            int batch = EconomyTuning.BatchForRound(r, def.EffectiveWallPressure, LiveCap, def.Batch);
            if (batch > ceiling) overCeiling++;
            if (batch < 1) underOne++;

            if (firstRawCrossing < 0 && EconomyTuning.BatchSizeForRound(r) > LiveCap) firstRawCrossing = r;
        }

        Check($"no round 1-80 asks for more pathogens than the board can hold (ceiling {ceiling} of a live cap of {LiveCap})",
              overCeiling == 0 && underOne == 0);

        // The hazard, restated for the Sprint 33 shape: the scripted rounds
        // are authored under the ceiling and never need clamping, while the
        // PROCEDURAL TAIL does -- the economy baseline is far past the live
        // cap by round 41, so without the clamp every tail round would set a
        // target the board cannot hold, and a food round that cannot finish
        // emitting never ends.
        var tail = RoundScript.ForRound(RoundScript.ScriptedRounds + 1);
        int tailRaw = Mathf.RoundToInt(
            EconomyTuning.BatchSizeForRound(RoundScript.ScriptedRounds + 1) * tail.EffectiveWallPressure);
        int tailClamped = EconomyTuning.BatchForRound(
            RoundScript.ScriptedRounds + 1, tail.EffectiveWallPressure, LiveCap, tail.Batch);
        Check($"the clamp still earns its place: the first tail round would ask for {tailRaw} against a live cap of {LiveCap}, clamped to {tailClamped}",
              tailRaw > LiveCap && tailClamped == ceiling);
        Check($"and the raw economy curve crosses the live cap on its own at round {firstRawCrossing}, so the hazard was never the multiplier's fault alone",
              firstRawCrossing > 0 && firstRawCrossing <= RoundScript.ScriptedRounds);

        // No scripted round may be clipped -- a clipped round delivers a
        // different count from the one the script authored, which would make
        // the whole 40-round curve a lie.
        int clipped = 0;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
        {
            var def = RoundScript.ForRound(r);
            if (EconomyTuning.BatchForRound(r, def.EffectiveWallPressure, LiveCap, def.Batch) != def.Batch) clipped++;
        }
        Check($"none of the {RoundScript.ScriptedRounds} scripted rounds is clipped by the ceiling, so each delivers exactly the count it authors",
              clipped == 0);

        // Rounds 1-2 must still equal the bare economy number, which is what
        // RoundVerification and EconomyVerification assert.
        // Rounds 1 and 2 are authored to the SAME numbers the economy curve
        // produced (16 and 22), deliberately: RoundVerification and
        // EconomyVerification both assert the unscaled batch for those two
        // rounds, and a tutorial round should not be crowded anyway.
        Check("rounds 1 and 2 still deliver exactly the economy's batch size (16 and 22)",
              EconomyTuning.BatchForRound(1, RoundScript.ForRound(1).EffectiveWallPressure, LiveCap,
                                          RoundScript.ForRound(1).Batch)
                  == EconomyTuning.BatchSizeForRound(1)
              && EconomyTuning.BatchForRound(2, RoundScript.ForRound(2).EffectiveWallPressure, LiveCap,
                                             RoundScript.ForRound(2).Batch)
                  == EconomyTuning.BatchSizeForRound(2));

        Check("a caller that does not know the ceiling (liveCap 0) gets the unclamped number",
              EconomyTuning.BatchForRound(40, 2.5f, 0)
                  == Mathf.RoundToInt(EconomyTuning.BatchSizeForRound(40) * 2.5f));
        Check("and zero wall pressure is read as 1.0, not as zero",
              EconomyTuning.BatchForRound(3, 0f, LiveCap) == EconomyTuning.BatchSizeForRound(3));
    }

    private static bool AllCast(Dictionary<PathogenSpecies, int> seen, RoundDefinition def)
    {
        foreach (var kv in seen)
        {
            bool inCast = false;
            foreach (var c in def.Cast) if (c.Species == kv.Key) inCast = true;
            if (!inCast) return false;
        }
        return true;
    }

    // ================================================================
    // Part C -- innate turnover.
    //
    // The Director's call (2026-11-02) was "yes, make them turn over",
    // against a measured fact: innate cells die ONLY on reaching KillLimit,
    // and KillLimitDiagnostic showed a macrophage landing a median of 2
    // kills in a 45s round against a ceiling of 20. The ceiling never
    // bound, so the 40 ATP "+8 kills" row wrote nothing observable.
    //
    // The limits that ship live on GameBootstrap's serialized profiles,
    // which no harness can reach (a GameBootstrap component builds the
    // whole game in Awake, so a harness cannot stand one up). They are
    // mirrored here deliberately, the way LifecycleVerification already
    // mirrors them -- and the gap is written up in docs/TEAM_RETRO.md,
    // because it is the structural reason dead upgrade rows keep shipping.
    // ================================================================

    private const int ShippedMacrophageKillLimit = 5;   // GameBootstrap, Sprint 32 (was 20)
    private const int ShippedNeutrophilKillLimit = 3;   // GameBootstrap, Sprint 32 (was 5)

    private struct TurnoverResult
    {
        public int Emitted;
        public int Alive;
        public int Retired;
        public int Cap;
    }

    /// <summary>
    /// Stands a real marrow tower up in front of a replenished field of
    /// bacteria and runs it for <paramref name="seconds"/> of simulated
    /// time, through BoneMarrowManager, SearchUnit and PathogenAgent -- no
    /// reimplementation. Returns what the tower actually did.
    /// </summary>
    private static TurnoverResult DriveTurnover(UnitKind kind, int killLimit, int speed, float seconds)
    {
        UnitTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        Random.InitState(3207);

        var junk = new List<GameObject>();
        var boardGo = new GameObject("S32_turnover_board");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);
        GutWall.Current = wall;

        UnitProfile Profile(UnitKind k, int limit, int tilesPerTick) => new UnitProfile
        {
            Kind = k, DisplayName = k.ToString(),
            FineTilesPerTick = tilesPerTick, FootprintFineTiles = 3,
            MaxActiveChildren = 4, KillLimit = limit,
            ContactRadiusFineTiles = 2,
            DegranulatesOnDepletion = k == UnitKind.Neutrophil,
            DegranulationBurstMultiplier = k == UnitKind.Neutrophil ? 3f : 0f,
            StressSenseChancePerTick = k == UnitKind.Macrophage ? 0.003f : 0.002f,
        };

        PrefabPool Pool(UnitProfile pr)
        {
            var template = new GameObject($"S32_{pr.DisplayName}_template");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<SearchUnit>();
            template.SetActive(false);
            junk.Add(template);
            var poolGo = new GameObject($"S32_{pr.DisplayName}_pool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            junk.Add(poolGo);
            return pool;
        }

        var mac = Profile(UnitKind.Macrophage, kind == UnitKind.Macrophage ? killLimit : 20,
                          kind == UnitKind.Macrophage ? speed : 1);
        var neu = Profile(UnitKind.Neutrophil, kind == UnitKind.Neutrophil ? killLimit : 5,
                          kind == UnitKind.Neutrophil ? speed : 5);

        var managerGo = new GameObject("S32_turnover_marrow");
        var manager = managerGo.AddComponent<BoneMarrowManager>();
        manager.Initialize(board, grid, field, mac, Pool(mac), neu, Pool(neu),
                           new[] { new Vector3(0f, -5f, 0f) }, 1f);
        manager.PlaceTower(0, kind);

        float t = 0f;
        int steps = Mathf.CeilToInt(seconds / BoardConfig.TickIntervalSeconds);
        for (int step = 0; step < steps; step++)
        {
            t += BoardConfig.TickIntervalSeconds;
            manager.Tick(BoardConfig.TickIntervalSeconds);

            // A replenished field of targets, the same generous supply
            // KillLimitDiagnostic uses -- this measures whether the limit
            // binds, not how scarce targets are.
            if (step % 10 == 0)
            {
                for (int k = 0; k < 6; k++)
                {
                    int ai = board.TissueBaseEdgeAxisIndex + Random.Range(0, board.TissueBandCells);
                    int lane = Random.Range(0, board.CrossLength);
                    var slot = board.CoarseFromAxis(ai, lane);
                    if (!grid.IsOccupantFree(slot)) continue;
                    grid.SeedHostState(slot, HostState.Healthy, t);
                    var pgo = new GameObject("S32_target"); pgo.AddComponent<SpriteRenderer>();
                    var pa = pgo.AddComponent<PathogenAgent>(); junk.Add(pgo);
                    pa.InitializeInTissueDirect(board, grid, wall, tally, x => { }, (c, sp, f, tt) => false,
                                                slot, PathogenClass.LargeBacterium, t,
                                                PathogenSpecies.GenericLargeBacterium);
                }
            }

            var kids = new List<SearchUnit>(manager.GetChildren(0));
            foreach (var u in kids)
                if (u != null && u.gameObject.activeSelf) u.SimulationTick(t);
        }

        var result = new TurnoverResult
        {
            Emitted = manager.EmittedCount,
            Alive = manager.GetChildren(0).Count,
            Cap = manager.GetTuning(0).MaxActiveChildren,
        };
        result.Retired = result.Emitted - result.Alive;

        // **Drain the static unit registry before tearing the rig down.**
        // SearchUnit keeps a static `live` list for the IFN-gamma aura and
        // for crowd desensitisation, and a unit only leaves it via
        // ResetForPool. A harness that destroys its GameObjects directly
        // therefore leaves phantom entries behind forever, and the NEXT
        // run reads them as neighbours -- which is exactly why these
        // retirement counts moved when the number of preceding runs
        // changed. Measured, not guessed: adding two runs ahead of C3
        // shifted its result from 4 retirements to 3. Every harness in
        // this repo that builds SearchUnits has the same leak; see
        // docs/TEAM_RETRO.md.
        var strays = new List<SearchUnit>(SearchUnit.Live);
        foreach (var u in strays) if (u != null) u.ResetForPool();

        foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
        GutWall.Current = null;
        Object.DestroyImmediate(managerGo);
        Object.DestroyImmediate(boardGo);
        UnitTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        return result;
    }

    // ----------------------------------------------------------------
    // C1. The limits bind now, and demonstrably did not before.
    // ----------------------------------------------------------------
    private static void RunTurnoverBindsAtTheNewLimits()
    {
        const float ThreeRounds = 135f;

        const float OneRound = 45f;

        var macOld = DriveTurnover(UnitKind.Macrophage, 20, 1, ThreeRounds);
        var macNew = DriveTurnover(UnitKind.Macrophage, ShippedMacrophageKillLimit, 1, ThreeRounds);
        var neuOld1 = DriveTurnover(UnitKind.Neutrophil, 5, 5, OneRound);
        var neuOld3 = DriveTurnover(UnitKind.Neutrophil, 5, 5, ThreeRounds);
        var neuNew1 = DriveTurnover(UnitKind.Neutrophil, ShippedNeutrophilKillLimit, 5, OneRound);
        var neuNew3 = DriveTurnover(UnitKind.Neutrophil, ShippedNeutrophilKillLimit, 5, ThreeRounds);

        Check($"at the OLD macrophage ceiling of 20 nothing ever retired, even over three rounds (emitted {macOld.Emitted}, retired {macOld.Retired}) -- that row really was inert",
              macOld.Retired == 0);
        Check($"at the new limit of {ShippedMacrophageKillLimit} macrophages do turn over (emitted {macNew.Emitted}, retired {macNew.Retired})",
              macNew.Retired > 0);

        // **A correction to BACKLOG.md's standing claim**, found by running
        // the measurement over a three-round horizon instead of one round.
        // "The kill-limit rows are inert" is true of the macrophage and NOT
        // true of the neutrophil: at 5 it binds across rounds, just not
        // inside one. So 5 -> 3 is a deliberate rebalance toward
        // within-a-round turnover, not the repair of a dead row -- and
        // saying otherwise would have been a confirmation standing in for a
        // measurement, which is the exact habit this project keeps catching.
        Check($"the OLD neutrophil ceiling of 5 did NOT bind inside one round ({neuOld1.Retired} retirements in 45s) ...",
              neuOld1.Retired == 0);
        Check($"... but DID across three ({neuOld3.Retired} of {neuOld3.Emitted} emitted) -- so it was a rebalance, not a dead row like the macrophage's",
              neuOld3.Retired > 0);
        Check($"at the new limit of {ShippedNeutrophilKillLimit} a neutrophil line turns over inside a single round ({neuNew1.Retired} retirements in 45s, vs {neuOld1.Retired} before)",
              neuNew1.Retired > neuOld1.Retired);
        Check($"and neutrophils cycle faster than macrophages over three rounds, as the biology says (neu {neuNew3.Retired} vs mac {macNew.Retired})",
              neuNew3.Retired > macNew.Retired);
    }

    // ----------------------------------------------------------------
    // C2. Turnover must not quietly empty the board. The handoff's
    //     warning was that this thins the board on top of Sprint 31's
    //     dispersal, so assert the standing population, not the rate.
    // ----------------------------------------------------------------
    private static void RunTurnoverRecoversTheStandingPopulation()
    {
        var mac = DriveTurnover(UnitKind.Macrophage, ShippedMacrophageKillLimit, 1, 135f);
        var neu = DriveTurnover(UnitKind.Neutrophil, ShippedNeutrophilKillLimit, 5, 135f);

        Check($"a macrophage tower still holds a standing population after three rounds of turnover ({mac.Alive} of a cap of {mac.Cap})",
              mac.Alive >= mac.Cap - 1);
        Check($"and so does a neutrophil tower, the faster-cycling one ({neu.Alive} of a cap of {neu.Cap})",
              neu.Alive >= neu.Cap - 2);
        // The marrow refills at the emission rate rather than bursting, so
        // the replacement cost is real but bounded.
        Check($"replacement runs through the marrow at its emission interval, not instantly ({mac.Emitted} emissions over 135s at {BoneMarrowManager.EmissionIntervalSeconds}s)",
              mac.Emitted <= Mathf.CeilToInt(135f / BoneMarrowManager.EmissionIntervalSeconds) + 1);
    }

    // ----------------------------------------------------------------
    // C3. The thing the Director is buying: the row has to change the
    //     rate, not just a number in a tooltip.
    // ----------------------------------------------------------------
    private static void RunTheKillLimitRowChangesTheRate()
    {
        // The mechanical half: the row writes the field it claims to.
        var profile = new UnitProfile { Kind = UnitKind.Macrophage, KillLimit = ShippedMacrophageKillLimit };
        var tuning = new UnitLifecycleTuning();
        ProgenitorUpgrades.Apply(UnitKind.Macrophage, 1, 3, tuning, null, profile, 0);
        int fullyUpgraded = tuning.KillLimit;

        Check($"macrophage row 1 at level 3 writes KillLimit {ShippedMacrophageKillLimit} -> {fullyUpgraded}",
              fullyUpgraded == ShippedMacrophageKillLimit + 3 * ProgenitorUpgrades.MacrophageKillLimitPerLevel);

        var neuProfile = new UnitProfile { Kind = UnitKind.Neutrophil, KillLimit = ShippedNeutrophilKillLimit };
        var neuTuning = new UnitLifecycleTuning();
        ProgenitorUpgrades.Apply(UnitKind.Neutrophil, 1, 4, neuTuning, null, neuProfile, 0);
        Check($"neutrophil row 1 at level 4 writes KillLimit {ShippedNeutrophilKillLimit} -> {neuTuning.KillLimit}",
              neuTuning.KillLimit == ShippedNeutrophilKillLimit + 4 * ProgenitorUpgrades.NeutrophilKillLimitPerLevel);

        // The half that actually matters, and the half that was false for
        // three sprints: buying the row has to change what happens on the
        // board. Same window, same target supply, only the ceiling differs.
        var baseRun = DriveTurnover(UnitKind.Macrophage, ShippedMacrophageKillLimit, 1, 135f);
        var upgraded = DriveTurnover(UnitKind.Macrophage, fullyUpgraded, 1, 135f);

        Check($"and buying it measurably slows turnover -- {baseRun.Retired} retirements at limit {ShippedMacrophageKillLimit}, {upgraded.Retired} at {fullyUpgraded}",
              upgraded.Retired < baseRun.Retired);

        // The guard against re-creating the bug this row was fixed for. At
        // the cap the limit SHOULD rarely bind inside three rounds -- that
        // is what the player paid for -- but if it cannot bind at all, the
        // top level is a ceiling nobody meets, which is precisely the shape
        // of the three dead upgrades found so far. So check it on a horizon
        // long enough to be reachable.
        var upgradedLong = DriveTurnover(UnitKind.Macrophage, fullyUpgraded, 1, 360f);
        Check($"the fully-upgraded limit of {fullyUpgraded} is still reachable over a longer run ({upgradedLong.Retired} retirements over 360s), so the top level is a longer life and not an unmeetable ceiling",
              upgradedLong.Retired > 0);

        // Determinism: the two runs below differ in nothing, so if they
        // disagree the rig is leaking state between runs again.
        var repeatA = DriveTurnover(UnitKind.Macrophage, ShippedMacrophageKillLimit, 1, 45f);
        var repeatB = DriveTurnover(UnitKind.Macrophage, ShippedMacrophageKillLimit, 1, 45f);
        Check($"two identical runs give identical results ({repeatA.Emitted}/{repeatA.Retired} vs {repeatB.Emitted}/{repeatB.Retired}) -- no state leaks between them",
              repeatA.Emitted == repeatB.Emitted && repeatA.Retired == repeatB.Retired);
    }
}
