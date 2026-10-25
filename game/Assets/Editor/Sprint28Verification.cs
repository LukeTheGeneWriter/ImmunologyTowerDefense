using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 28 -- crowd desensitisation, and the 2-wide marrow.
///
/// The problem: macrophages and neutrophils all read the same cytokine
/// gradient, so they all walked to the same pathogen and everything else
/// strolled past. One dogpile, a wide-open wall.
///
/// The fix must thread a needle, and that is what most of these checks
/// are for. Push cells apart too little and the swarm returns; push them
/// apart too much and you break the cooperation bonus Sprint 23 built on
/// Halle 2016's finding that killing is superadditive at three or more
/// attackers. Kienle 2021 (Science) found the same trade-off in vivo:
/// disabling the neutrophil's own swarm brake let it scan more tissue but
/// made bacterial containment WORSE. Both extremes are bad; the shape in
/// the middle is the design.
/// </summary>
public static class Sprint28Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 28 -- crowd desensitisation")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2801);

        Debug.Log("[Sprint28Verification] === Crowd desensitisation ===");

        RunCooperationIsFree();
        RunSurplusIsPenalised();
        RunShapeIsForgivingThenSharp();
        RunPenaltyIsLocal();
        RunDisableable();
        RunCrowdedCellsSpreadOut();
        RunMarrowIsTwoWide();

        if (failures > 0)
            Debug.LogError($"[Sprint28Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint28Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint28Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint28Verification] FAIL -- {label}"); }
    }

    private static FineCoord[] Pack(int n, FineCoord at)
    {
        var a = new FineCoord[Mathf.Max(1, n)];
        for (int i = 0; i < n; i++) a[i] = at;
        return a;
    }

    // =================================================================
    // 1. The cooperation allowance -- the thing most likely to be broken
    // =================================================================
    private static void RunCooperationIsFree()
    {
        Debug.Log("[Sprint28Verification] --- Cooperation is free ---");
        UnitTuning.ResetToDefaults();

        var here = new FineCoord(50, 50);

        Check("a lone cell feels no crowding",
            Chemotaxis.CrowdPenaltyAt(here, Pack(0, here), 0) <= 0f);

        // Halle 2016: killing is superadditive at THREE attackers, and
        // Sprint 23 built that bonus. If the free allowance ever drops
        // below two companions, cells stop being able to gang up and that
        // bonus becomes unreachable -- the swarm fix would have quietly
        // broken the cooperation feature.
        int free = UnitTuning.RepulsionFreeCompanions;
        Check($"{free} companions are free, so three cells can converge",
            Chemotaxis.CrowdPenaltyAt(here, Pack(free, here), free) <= 0f);
        Check("the free allowance is at least 2 (the cooperation bonus needs 3 cells)",
            free >= 2);
    }

    // =================================================================
    // 2 + 3. The surplus, and the SHAPE of the response
    // =================================================================
    private static void RunSurplusIsPenalised()
    {
        Debug.Log("[Sprint28Verification] --- Surplus is penalised ---");
        UnitTuning.ResetToDefaults();

        var here = new FineCoord(50, 50);
        int free = UnitTuning.RepulsionFreeCompanions;

        float one = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 1, here), free + 1);
        float two = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 2, here), free + 2);
        float four = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 4, here), free + 4);

        Check("one cell over the allowance is discouraged", one > 0f);
        Check("and it gets worse with every extra body", two > one && four > two);
    }

    private static void RunShapeIsForgivingThenSharp()
    {
        Debug.Log("[Sprint28Verification] --- The shape ---");
        UnitTuning.ResetToDefaults();

        var here = new FineCoord(50, 50);
        int free = UnitTuning.RepulsionFreeCompanions;

        float s1 = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 1, here), free + 1);
        float s2 = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 2, here), free + 2);
        float s4 = Chemotaxis.CrowdPenaltyAt(here, Pack(free + 4, here), free + 4);

        // Superlinear: the fourth cell is barely discouraged, the eighth is
        // strongly pushed off. That is the shape Kienle describes -- the
        // brake engages on HIGH self-signal, not on the first neighbour.
        Check($"the response is superlinear, not a flat wall (1:{s1:F2} 2:{s2:F2} 4:{s4:F2})",
            (s2 / s1) > 2f && (s4 / s2) > 2f);
        Check("the exponent is above 1 (forgiving first, sharp later)",
            UnitTuning.RepulsionExponent > 1f);
    }

    // =================================================================
    // 4. Locality
    // =================================================================
    private static void RunPenaltyIsLocal()
    {
        Debug.Log("[Sprint28Verification] --- Locality ---");
        UnitTuning.ResetToDefaults();

        var here = new FineCoord(50, 50);
        int n = UnitTuning.RepulsionFreeCompanions + 4;
        int far = UnitTuning.RepulsionRadiusFineTiles + 3;

        var distant = new FineCoord[n];
        for (int i = 0; i < n; i++) distant[i] = new FineCoord(50 + far, 50);

        Check("a crowd outside the radius is ignored",
            Chemotaxis.CrowdPenaltyAt(here, distant, n) <= 0f);
        Check("...but the same crowd on top of you is not",
            Chemotaxis.CrowdPenaltyAt(new FineCoord(50 + far, 50), distant, n) > 0f);
    }

    // =================================================================
    // 5. It can be switched off
    // =================================================================
    private static void RunDisableable()
    {
        Debug.Log("[Sprint28Verification] --- Off switch ---");
        UnitTuning.ResetToDefaults();

        var here = new FineCoord(50, 50);
        int n = UnitTuning.RepulsionFreeCompanions + 5;

        UnitTuning.RepulsionStrength = 0f;
        Check("strength 0 disables crowding entirely",
            Chemotaxis.CrowdPenaltyAt(here, Pack(n, here), n) <= 0f);

        UnitTuning.ResetToDefaults();
        Check("...and the default has it on",
            Chemotaxis.CrowdPenaltyAt(here, Pack(n, here), n) > 0f);
    }

    // =================================================================
    // 6. The behaviour that was actually complained about
    // =================================================================
    private static void RunCrowdedCellsSpreadOut()
    {
        Debug.Log("[Sprint28Verification] --- Cells spread instead of piling ---");
        UnitTuning.ResetToDefaults();

        var go = new GameObject("S28_Board");
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
            var field = new CytokineField(board);

            var candidates = new FineCoord[4];
            var weights = new float[4];

            // A pile of eight companions sitting exactly where this cell is.
            var here = new FineCoord(200, 100);
            int pile = 8;
            var crowd = new FineCoord[pile];
            for (int i = 0; i < pile; i++) crowd[i] = here;

            // With no gradient to follow, the only signal is crowding -- so
            // every step should move AWAY from the pile.
            int movedOff = 0;
            for (int trial = 0; trial < 60; trial++)
            {
                var next = Chemotaxis.ChooseNextStep(
                    here, board, field, cytokineEnabled: true,
                    candidates, weights, crowd, pile);
                if (!next.Equals(here)) movedOff++;
            }
            Check($"a cell in a pile keeps moving out of it ({movedOff}/60 steps)",
                movedOff >= 55);

            // And with crowding off, it has no reason to prefer anywhere.
            UnitTuning.RepulsionStrength = 0f;
            var nextNoCrowd = Chemotaxis.ChooseNextStep(
                here, board, field, cytokineEnabled: true, candidates, weights, crowd, pile);
            Check("with crowding off the pile exerts no push at all",
                Chemotaxis.CrowdPenaltyAt(nextNoCrowd, crowd, pile) <= 0f);
        }
        finally
        {
            Object.DestroyImmediate(go);
            UnitTuning.ResetToDefaults();
        }
    }

    // =================================================================
    // 7. The marrow grid
    // =================================================================
    private static void RunMarrowIsTwoWide()
    {
        Debug.Log("[Sprint28Verification] --- 2-wide marrow ---");
        EconomyTuning.ResetToDefaults();

        Check("the bone holds 8 niches", EconomyTuning.MarrowMaxSlots == 8);
        Check("...which divides evenly into a 2-wide grid",
            EconomyTuning.MarrowMaxSlots % 2 == 0);
        // 2x4 means four rows; the starting allowance should fill whole
        // rows so the bone never opens half-lit.
        Check("the starting allowance fills whole rows",
            EconomyTuning.MarrowStartingSlots % 2 == 0);
    }
}
