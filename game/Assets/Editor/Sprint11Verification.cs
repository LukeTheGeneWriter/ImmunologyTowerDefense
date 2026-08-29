using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;
using ImmunologyTD.Adaptive;

/// <summary>
/// Sprint 11 verification: the placeholder buy-phase shop, per-tower
/// progenitor upgrades, the knowledge-ladder data, and neighbour-
/// accelerated regrowth (the one real mechanic this sprint). Same
/// drive-the-real-classes, no-Play-Mode philosophy as the eight before it.
///
/// The shop UI itself (an OnGUI panel) isn't headlessly testable -- that's
/// the build launch -- but ShopLedger, ShopTuning, BoneMarrowManager.
/// UpgradeTower, KnowledgeLadder, and TissueGrid regrowth all are.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod Sprint11Verification.RunAll
/// </summary>
public static class Sprint11Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint11Verification] Starting ...");

        ShopTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();

        RunShopLedger();
        RunProgenitorUpgrade();
        RunKnowledgeLadder();
        RunNeighbourRegrowth();

        ShopTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();

        Debug.Log($"[Sprint11Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint11Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint11Verification] FAIL -- {label}"); }
    }

    // =================================================================
    // 1. ShopLedger -- spend, refuse, level, price scaling
    // =================================================================

    private static void RunShopLedger()
    {
        ShopTuning.ResetToDefaults();
        var shop = new ShopLedger();
        var wallet = new AtpWallet(1000);

        Check("nothing owned at the start",
            shop.LevelOf(ShopItem.Crypt) == 0 && !shop.Owns(ShopItem.Crypt));

        int p0 = shop.NextPrice(ShopItem.Crypt);
        Check("NextPrice of an unowned item is its base price",
            p0 == ShopTuning.BasePriceFor(ShopItem.Crypt));

        int before = wallet.Balance;
        Check("TryBuy succeeds and spends exactly the price",
            shop.TryBuy(ShopItem.Crypt, wallet) && wallet.Balance == before - p0);
        Check("...and the level went to 1", shop.LevelOf(ShopItem.Crypt) == 1 && shop.Owns(ShopItem.Crypt));

        int p1 = shop.NextPrice(ShopItem.Crypt);
        Check($"the second level costs more ({p1} > {p0})", p1 > p0);

        int rev = shop.Revision;
        shop.TryBuy(ShopItem.Crypt, wallet);
        Check("second buy -> level 2, revision advanced", shop.LevelOf(ShopItem.Crypt) == 2 && shop.Revision > rev);

        var broke = new AtpWallet(1);
        Check("TryBuy with too little ATP fails and changes nothing",
            !shop.TryBuy(ShopItem.BarrierMucusTurnover, broke)
            && shop.LevelOf(ShopItem.BarrierMucusTurnover) == 0
            && broke.Balance == 1);
        Check("CanBuy reflects affordability",
            !shop.CanBuy(ShopItem.BarrierMucusTurnover, broke)
            && shop.CanBuy(ShopItem.BarrierMucusTurnover, wallet));

        Check("TryBuy with a null wallet is a no-op",
            !shop.TryBuy(ShopItem.Crypt, null) && shop.LevelOf(ShopItem.Crypt) == 2);

        shop.Reset();
        Check("Reset zeroes every item", shop.LevelOf(ShopItem.Crypt) == 0);
    }

    // =================================================================
    // 2. Per-tower progenitor upgrade -- placeholder, spends + levels
    // =================================================================

    private static void RunProgenitorUpgrade()
    {
        var boardGo = new GameObject("S11_Board");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);

        var mac = new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5, MaxActiveChildren = 10, KillLimit = 20 };
        var neu = new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3, MaxActiveChildren = 10, KillLimit = 5 };
        var macPool = NewUnitPool("S11_Mac");
        var neuPool = NewUnitPool("S11_Neu");

        var mgrGo = new GameObject("S11_Marrow");
        var marrow = mgrGo.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[5];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(i * 2f, -8f, 0f);
        var wallet = new AtpWallet(1000);
        marrow.Initialize(board, grid, field, mac, macPool, neu, neuPool, positions, 1f, wallet);

        Check("upgrading an empty slot fails", !marrow.UpgradeTower(0) && marrow.GetUpgradeLevel(0) == 0);

        marrow.PlaceTower(0, UnitKind.Macrophage);
        int before = wallet.Balance;
        int price = ShopTuning.ProgenitorUpgradePrice(0);
        Check("upgrading a placed tower spends ATP and bumps the level",
            marrow.UpgradeTower(0) && marrow.GetUpgradeLevel(0) == 1 && wallet.Balance == before - price);
        Check("the next upgrade costs more", ShopTuning.ProgenitorUpgradePrice(1) > price);

        // The placeholder does NOT touch the tower's real tuning.
        var tuning = marrow.GetTuning(0);
        int killLimitBefore = tuning.KillLimit;
        marrow.UpgradeTower(0);
        Check("the upgrade is a placeholder -- UnitLifecycleTuning is untouched",
            marrow.GetTuning(0).KillLimit == killLimitBefore && marrow.GetUpgradeLevel(0) == 2);

        var broke = new GameObject("S11_MarrowBroke").AddComponent<BoneMarrowManager>();
        broke.Initialize(board, grid, field, mac, macPool, neu, neuPool, positions, 1f, new AtpWallet(1));
        broke.PlaceTower(0, UnitKind.Neutrophil);
        Check("upgrade refused when broke", !broke.UpgradeTower(0) && broke.GetUpgradeLevel(0) == 0);

        Object.DestroyImmediate(boardGo);
        Object.DestroyImmediate(mgrGo);
        Object.DestroyImmediate(broke.gameObject);
        Object.DestroyImmediate(macPool.gameObject);
        Object.DestroyImmediate(neuPool.gameObject);
    }

    private static PrefabPool NewUnitPool(string label)
    {
        var template = new GameObject($"{label}_Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<SearchUnit>();
        template.SetActive(false);
        var poolGo = new GameObject($"{label}_Pool");
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        return pool;
    }

    // =================================================================
    // 3. KnowledgeLadder -- data, thresholds, ordering
    // =================================================================

    private static void RunKnowledgeLadder()
    {
        Check("six rungs", KnowledgeLadder.Rungs.Length == 6);

        bool ascending = true;
        for (int i = 1; i < KnowledgeLadder.Rungs.Length; i++)
            if (KnowledgeLadder.Rungs[i].ThresholdPercent <= KnowledgeLadder.Rungs[i - 1].ThresholdPercent)
                ascending = false;
        Check("rungs are ordered ascending by threshold", ascending);

        Check("nothing unlocked at 0%", KnowledgeLadder.UnlockedCount(0f) == 0);
        Check("CTL locked at 9.9%, unlocked at 10%",
            !KnowledgeLadder.IsUnlocked(KnowledgeCapability.CytotoxicTCells, 9.9f)
            && KnowledgeLadder.IsUnlocked(KnowledgeCapability.CytotoxicTCells, 10f));
        Check("at 34% exactly CTL + NeutAb + MemT are unlocked (3)",
            KnowledgeLadder.UnlockedCount(34f) == 3
            && KnowledgeLadder.IsUnlocked(KnowledgeCapability.MemoryTCells, 34f)
            && !KnowledgeLadder.IsUnlocked(KnowledgeCapability.FcReceptor, 34f));
        Check("all six unlocked at 100%", KnowledgeLadder.UnlockedCount(100f) == 6
            && KnowledgeLadder.IsUnlocked(KnowledgeCapability.SecretoryIgA, 100f));

        bool monotonic = true;
        int last = 0;
        for (float p = 0f; p <= 100f; p += 1f)
        {
            int u = KnowledgeLadder.UnlockedCount(p);
            if (u < last) monotonic = false;
            last = u;
        }
        Check("UnlockedCount is monotonic in %", monotonic);
    }

    // =================================================================
    // 4. Neighbour-accelerated regrowth (the real mechanic)
    // =================================================================

    /// <summary>Sim-seconds for a cleared Empty cell to regrow to Healthy,
    /// optionally after also killing its four von Neumann neighbours.</summary>
    private static float SecondsToRegrow(bool killNeighbours)
    {
        var boardGo = new GameObject("S11_RegrowBoard");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);

        var c = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 3, 5);
        grid.KillHostCell(c);
        grid.ClearDebris(c, TissueGrid.FullDebris, 0f); // c is now Empty, emptySince = 0

        if (killNeighbours)
        {
            grid.KillHostCell(new CoarseCoord(c.Column + 1, c.Row));
            grid.KillHostCell(new CoarseCoord(c.Column - 1, c.Row));
            grid.KillHostCell(new CoarseCoord(c.Column, c.Row + 1));
            grid.KillHostCell(new CoarseCoord(c.Column, c.Row - 1));
        }

        float t = 0f;
        for (int i = 0; i < 4000 && grid.GetHostState(c) != HostState.Healthy; i++)
        {
            t += 0.25f;
            grid.Tick(0.25f, t);
        }
        Object.DestroyImmediate(boardGo);
        return grid.GetHostState(c) == HostState.Healthy ? t : -1f;
    }

    private static void RunNeighbourRegrowth()
    {
        TissueTuning.ResetToDefaults(); // NeighbourRegrowthBonus = 0.5
        float surrounded = SecondsToRegrow(killNeighbours: false); // 4 healthy neighbours
        float isolated = SecondsToRegrow(killNeighbours: true);    // 0 healthy neighbours

        Debug.Log($"[Sprint11Verification] regrow time -- surrounded {surrounded:F1}s, isolated {isolated:F1}s (base {TissueTuning.HostRegenerationSeconds}s).");
        Check("a cell ringed by healthy tissue regrows well before the base period",
            surrounded > 0f && surrounded < TissueTuning.HostRegenerationSeconds * 0.6f);
        Check("an isolated empty cell regrows at ~the base period",
            isolated > TissueTuning.HostRegenerationSeconds * 0.75f);
        Check("tissue heals inward: surrounded regrows faster than isolated", surrounded < isolated);

        TissueTuning.NeighbourRegrowthBonus = 0f;
        float sur0 = SecondsToRegrow(false);
        float iso0 = SecondsToRegrow(true);
        Check("with the bonus at 0, neighbour count no longer matters",
            Mathf.Abs(sur0 - iso0) <= 0.5f);

        TissueTuning.ResetToDefaults();
    }
}
