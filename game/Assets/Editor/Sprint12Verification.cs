using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;

/// <summary>
/// Sprint 12 verification: cytokine sensing is on by default and the
/// *improvement* is a buyable shop upgrade (`ShopItem.CytokineSensingUpgrade`
/// → `Chemotaxis.SensingUpgradeLevel`). The DC patrol movement rework is
/// covered in `AdaptiveVerification` (RunDcLaneSpread / RunDcPatrolSweep).
///
/// The HudOverlay bridge (Update() pushes `ShopLedger.LevelOf` into
/// `Chemotaxis`) isn't headlessly testable -- Update() doesn't run in
/// batchmode -- so this drives the two halves: the ledger tracks the item,
/// and Chemotaxis responds to the level.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod Sprint12Verification.RunAll
/// </summary>
public static class Sprint12Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint12Verification] Starting ...");

        ShopTuning.ResetToDefaults();
        Chemotaxis.SensingUpgradeLevel = 0;

        RunDefaultOn();
        RunEffectiveSharpness();
        RunLedgerTracksUpgrade();
        RunStrongerBias();

        ShopTuning.ResetToDefaults();
        Chemotaxis.SensingUpgradeLevel = 0;

        Debug.Log($"[Sprint12Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint12Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint12Verification] FAIL -- {label}"); }
    }

    private static void RunDefaultOn()
    {
        Check("cytokine sensing is ON by default (no key press)", CytokineToggle.Enabled);
    }

    private static void RunEffectiveSharpness()
    {
        Chemotaxis.SensingUpgradeLevel = 0;
        Check("level 0: effective sharpness == base GradientSharpness",
            Mathf.Approximately(Chemotaxis.EffectiveSharpness, Chemotaxis.GradientSharpness));

        Chemotaxis.SensingUpgradeLevel = 2;
        float expected = Chemotaxis.GradientSharpness * (1f + 2f * Chemotaxis.SensingUpgradePerLevel);
        Check($"level 2: effective sharpness scales ({Chemotaxis.EffectiveSharpness:F2} == {expected:F2})",
            Mathf.Approximately(Chemotaxis.EffectiveSharpness, expected));
        Check("more levels -> strictly sharper", Chemotaxis.EffectiveSharpness > Chemotaxis.GradientSharpness);

        Chemotaxis.SensingUpgradeLevel = 0;
    }

    private static void RunLedgerTracksUpgrade()
    {
        ShopTuning.ResetToDefaults();
        var shop = new ShopLedger();
        var wallet = new AtpWallet(1000);

        Check("upgrade unowned at start", shop.LevelOf(ShopItem.CytokineSensingUpgrade) == 0);
        int p0 = shop.NextPrice(ShopItem.CytokineSensingUpgrade);
        Check("buy raises the level and spends", shop.TryBuy(ShopItem.CytokineSensingUpgrade, wallet)
            && shop.LevelOf(ShopItem.CytokineSensingUpgrade) == 1 && wallet.Balance == 1000 - p0);
        Check("the second level costs more", shop.NextPrice(ShopItem.CytokineSensingUpgrade) > p0);

        // The bridge HudOverlay runs each frame, reproduced here:
        Chemotaxis.SensingUpgradeLevel = shop.LevelOf(ShopItem.CytokineSensingUpgrade);
        Check("bridging the ledger level into Chemotaxis sharpens sensing",
            Chemotaxis.EffectiveSharpness > Chemotaxis.GradientSharpness);
        Chemotaxis.SensingUpgradeLevel = 0;
    }

    /// <summary>A hand-built one-source gradient: a higher upgrade level
    /// makes ChooseNextStep pick the toward-source neighbour more often.</summary>
    private static void RunStrongerBias()
    {
        var boardGo = new GameObject("S12_Board");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var field = new CytokineField(board);
        field.Recompute(new[] { (new CoarseCoord(12, 5), 30f) });

        var start = board.CoarseCenterFine(new CoarseCoord(9, 5)); // a few cells from the source
        var cand = new FineCoord[4];
        var wgt = new float[4];

        int TowardCount(int level, int trials)
        {
            Chemotaxis.SensingUpgradeLevel = level;
            Random.InitState(4242);
            int toward = 0;
            for (int i = 0; i < trials; i++)
            {
                var next = Chemotaxis.ChooseNextStep(start, board, field, true, cand, wgt);
                // "toward the source" == the step that increases column (source is at col 12 > 9).
                if (next.Column > start.Column) toward++;
            }
            Chemotaxis.SensingUpgradeLevel = 0;
            return toward;
        }

        int low = TowardCount(0, 4000);
        int high = TowardCount(4, 4000);
        Debug.Log($"[Sprint12Verification] toward-source picks / 4000 -- level 0: {low}, level 4: {high}.");
        Check($"a higher sensing upgrade biases harder toward the source ({high} > {low})", high > low);

        Object.DestroyImmediate(boardGo);
        Chemotaxis.SensingUpgradeLevel = 0;
    }
}
