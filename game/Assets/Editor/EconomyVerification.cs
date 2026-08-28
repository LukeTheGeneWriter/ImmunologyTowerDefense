using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;

/// <summary>
/// Sprint 7 verification: the ATP economy framework and the round loop
/// (GAME_DESIGN.md §5b/§5d/§6c, SPRINT_PLAN.md). Same drive-the-real-classes,
/// no-Play-Mode philosophy as the five harnesses before it -- AtpWallet,
/// RoundController, PathogenSpawner's batch gating, BoneMarrowManager's
/// placement cost, and the EconomyHooks kill payout, all exercised through
/// their production surfaces.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod EconomyVerification.RunAll
/// </summary>
public static class EconomyVerification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[EconomyVerification] Starting ...");

        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();

        RunWallet();
        RunBatchGating();
        RunRoundLoop();
        RunLifePool();
        RunPlacementCost();
        RunPerKillIncome();
        RunRoundBoundaryClearsUnits();

        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        EconomyHooks.PayForKill = null;

        Debug.Log($"[EconomyVerification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool condition)
    {
        if (condition) { passed++; Debug.Log($"[EconomyVerification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[EconomyVerification] FAIL -- {label}"); }
    }

    // =================================================================
    // Fixtures
    // =================================================================

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public InvasionTally Tally;
        public GutInterface Gut;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++)
                if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static Rig BuildRig(string name)
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject($"EconomyVerification_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Gut = new GutInterface(rig.Board, rig.Grid, rig.Tally);
        return rig;
    }

    private static PathogenSpawner NewSpawner(Rig rig)
    {
        var go = new GameObject("EconomyVerification_Spawner");
        rig.Junk.Add(go);
        var spawner = go.AddComponent<PathogenSpawner>();
        var template = new GameObject("EconomyVerification_PathogenTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        rig.Junk.Add(template);
        spawner.Initialize(rig.Board, rig.Grid, rig.Field, rig.Gut, rig.Tally, template);
        return spawner;
    }

    private static RoundController NewRounds(Rig rig, AtpWallet wallet, PathogenSpawner spawner, BoneMarrowManager marrow)
    {
        var go = new GameObject("EconomyVerification_Rounds");
        rig.Junk.Add(go);
        var rc = go.AddComponent<RoundController>();
        rc.Initialize(wallet, spawner, rig.Tally, marrow);
        return rc;
    }

    private static BoneMarrowManager NewMarrow(Rig rig, AtpWallet wallet)
    {
        var mac = MakeProfile(UnitKind.Macrophage);
        var neu = MakeProfile(UnitKind.Neutrophil);
        var macPool = NewUnitPool(rig, mac);
        var neuPool = NewUnitPool(rig, neu);

        var go = new GameObject("EconomyVerification_Marrow");
        rig.Junk.Add(go);
        var m = go.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[5];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(i * 2f, -6f, 0f);
        m.Initialize(rig.Board, rig.Grid, rig.Field, mac, macPool, neu, neuPool, positions, 1f, wallet);
        return m;
    }

    private static UnitProfile MakeProfile(UnitKind kind) => kind == UnitKind.Macrophage
        ? new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5,
                            MaxActiveChildren = 10, KillLimit = 20 }
        : new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3,
                            MaxActiveChildren = 10, KillLimit = 5, DegranulatesOnDepletion = true, DegranulationBurstMultiplier = 3f };

    private static PrefabPool NewUnitPool(Rig rig, UnitProfile profile)
    {
        var template = new GameObject($"{profile.DisplayName}Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<SearchUnit>();
        template.SetActive(false);
        rig.Junk.Add(template);
        var poolGo = new GameObject($"{profile.DisplayName}Pool");
        rig.Junk.Add(poolGo);
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        return pool;
    }

    /// <summary>Ticks spawner + every live agent + the round controller
    /// until the current round clears (Phase back to Building) or a step
    /// budget runs out. Adhesion is forced to 0 by the caller for a clean
    /// wash-through.</summary>
    private static bool DriveRoundToClear(Rig rig, PathogenSpawner spawner, RoundController rounds, int maxSteps = 2000)
    {
        float t = 0f;
        for (int i = 0; i < maxSteps; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in new List<PathogenAgent>(spawner.Live))
                a.SimulationTick(0.1f, t);
            rounds.Tick(0.1f);
            if (rounds.Phase != RoundPhase.Active) return true;
        }
        return false;
    }

    // =================================================================
    // A. AtpWallet
    // =================================================================
    private static void RunWallet()
    {
        Debug.Log("[EconomyVerification] --- AtpWallet ---");
        var w = new AtpWallet(100);
        Check("Starts at the given balance", w.Balance == 100);
        Check("CanAfford within balance", w.CanAfford(100) && w.CanAfford(0));
        Check("Cannot afford above balance", !w.CanAfford(101));

        Check("TrySpend within balance succeeds and deducts", w.TrySpend(40) && w.Balance == 60);
        Check("TrySpend above balance fails and changes nothing", !w.TrySpend(61) && w.Balance == 60);
        Check("TrySpend of a non-positive cost is a free success", w.TrySpend(0) && w.TrySpend(-5) && w.Balance == 60);

        w.Grant(50);
        Check("Grant adds", w.Balance == 110);
        w.Grant(-10);
        Check("Grant of a non-positive amount is ignored", w.Balance == 110);
        Check("LifetimeEarned tracks grants, not spends", w.LifetimeEarned == 150);

        var neg = new AtpWallet(-20);
        Check("A negative starting balance clamps to 0", neg.Balance == 0);
    }

    // =================================================================
    // B. PathogenSpawner batch gating
    // =================================================================
    private static void RunBatchGating()
    {
        Debug.Log("[EconomyVerification] --- Batch gating ---");
        InvasionTuning.AdhesionChanceAtWall = 0f; // nothing sticks -- clean wash-through
        var rig = BuildRig("Batch");
        var spawner = NewSpawner(rig);

        // Idle before any batch: Tick does not spawn.
        float t = 0f;
        for (int i = 0; i < 100; i++) { t += 0.5f; spawner.Tick(0.5f, t); }
        Check("An un-armed spawner emits nothing", spawner.BatchEmitted == 0 && spawner.LiveCount == 0 && !spawner.BatchComplete);

        spawner.BeginBatch(4);
        Check("BeginBatch sets the target", spawner.BatchTarget == 4 && spawner.BatchEmitted == 0);

        for (int i = 0; i < 400 && spawner.BatchEmitted < 4; i++) { t += 0.5f; spawner.Tick(0.5f, t); }
        Check("The spawner emits exactly the batch target and no more", spawner.BatchEmitted == 4);
        Check("...not complete while pathogens are still in the lumen",
            spawner.LiveCount > 0 && !spawner.BatchComplete);

        // Wash them through (adhesion 0 -> all excreted).
        for (int i = 0; i < 2000 && !spawner.BatchComplete; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in new List<PathogenAgent>(spawner.Live)) a.SimulationTick(0.1f, t);
        }
        Check("Complete once the batch is emitted and nothing is left in lumen/tissue",
            spawner.BatchComplete && spawner.LiveCount == 0);
        Check("All four were excreted", rig.Tally.Excreted == 4);

        spawner.EndBatch();
        Check("EndBatch disarms it (BatchComplete goes false)", !spawner.BatchComplete);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // C. The round loop
    // =================================================================
    private static void RunRoundLoop()
    {
        Debug.Log("[EconomyVerification] --- Round loop ---");
        EconomyTuning.ResetToDefaults();
        InvasionTuning.AdhesionChanceAtWall = 0f;
        var rig = BuildRig("Loop");
        var wallet = new AtpWallet(EconomyTuning.StartingAtp);
        var spawner = NewSpawner(rig);
        var marrow = NewMarrow(rig, wallet);
        var rounds = NewRounds(rig, wallet, spawner, marrow);

        Check("Opens in Building at round 0", rounds.Phase == RoundPhase.Building && rounds.RoundNumber == 0);
        Check("Life pool starts full", rounds.Lives == EconomyTuning.StartingLives && rounds.MaxLives == EconomyTuning.StartingLives);

        int balanceBeforeRound1 = wallet.Balance;
        rounds.StartRound();
        Check("StartRound -> Active, round 1", rounds.Phase == RoundPhase.Active && rounds.RoundNumber == 1);
        Check("StartRound arms the spawner with round 1's batch size",
            spawner.BatchTarget == EconomyTuning.BatchSizeForRound(1));
        Check("StartRound itself pays nothing (the lump is on CLEAR)", wallet.Balance == balanceBeforeRound1);

        bool cleared = DriveRoundToClear(rig, spawner, rounds);
        Check("Round 1 clears", cleared && rounds.Phase == RoundPhase.Building && rounds.RoundsCleared == 1);
        Check("...and the round-start lump sum is granted on the clear",
            wallet.Balance == balanceBeforeRound1 + EconomyTuning.RoundStartLumpSum);

        // Round 2's batch is bigger.
        rounds.StartRound();
        Check("Round 2's batch is larger than round 1's",
            spawner.BatchTarget == EconomyTuning.BatchSizeForRound(2)
            && EconomyTuning.BatchSizeForRound(2) > EconomyTuning.BatchSizeForRound(1));
        DriveRoundToClear(rig, spawner, rounds);
        Check("Round 2 clears; two rounds cleared, round number 2",
            rounds.RoundsCleared == 2 && rounds.RoundNumber == 2 && rounds.Phase == RoundPhase.Building);

        // StartRound is a no-op outside Building.
        rounds.StartRound();
        int rn = rounds.RoundNumber;
        rounds.StartRound(); // second call while Active
        Check("StartRound is a no-op while a round is Active", rounds.RoundNumber == rn);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // D. The 100-life pool (GAME_DESIGN.md §6c)
    // =================================================================
    private static void RunLifePool()
    {
        Debug.Log("[EconomyVerification] --- Life pool ---");
        EconomyTuning.ResetToDefaults();
        var rig = BuildRig("Lives");
        var wallet = new AtpWallet(EconomyTuning.StartingAtp);
        var spawner = NewSpawner(rig);
        var rounds = NewRounds(rig, wallet, spawner, null);

        rounds.StartRound();
        int max = rounds.MaxLives;

        // A breach is InvasionTally.ReachedBase rising. Charge it on the tick.
        rig.Tally.ReachedBase += 3;
        rounds.Tick(0.1f);
        Check("Three breaches cost three lives", rounds.Lives == max - 3 && rounds.Phase == RoundPhase.Active);

        rig.Tally.ReachedBase += 1;
        rounds.Tick(0.1f);
        Check("One more breach, one more life", rounds.Lives == max - 4);

        // Drain the rest -> Defeat.
        rig.Tally.ReachedBase += max; // way over
        rounds.Tick(0.1f);
        Check("Lives hitting 0 -> Defeat, clamped at 0", rounds.Lives == 0 && rounds.Phase == RoundPhase.Defeat);
        Check("Defeat disarms the spawner", !spawner.BatchComplete && spawner.BatchTarget >= 0);

        // Ticks after Defeat do nothing.
        rig.Tally.ReachedBase += 5;
        rounds.Tick(0.1f);
        Check("Ticks after Defeat are inert", rounds.Lives == 0 && rounds.Phase == RoundPhase.Defeat);

        rig.Dispose();

        // Life regeneration: every LifeRegenRounds cleared rounds.
        EconomyTuning.ResetToDefaults();
        EconomyTuning.LifeRegenRounds = 1;   // regen every round for the test
        EconomyTuning.LifeRegenAmount = 2;
        InvasionTuning.AdhesionChanceAtWall = 0f;
        var rig2 = BuildRig("Regen");
        var wallet2 = new AtpWallet(EconomyTuning.StartingAtp);
        var spawner2 = NewSpawner(rig2);
        var rounds2 = NewRounds(rig2, wallet2, spawner2, null);

        rounds2.StartRound();
        rig2.Tally.ReachedBase += 5;
        rounds2.Tick(0.1f);
        int afterBreaches = rounds2.Lives; // max - 5
        DriveRoundToClear(rig2, spawner2, rounds2);
        Check("A cleared round regenerates LifeRegenAmount lives",
            rounds2.Lives == afterBreaches + EconomyTuning.LifeRegenAmount);

        // Regen is capped at MaxLives.
        rounds2.StartRound();
        DriveRoundToClear(rig2, spawner2, rounds2);
        Check("Regen never exceeds MaxLives", rounds2.Lives <= rounds2.MaxLives);

        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        rig2.Dispose();
    }

    // =================================================================
    // E. Placement costs ATP (GAME_DESIGN.md §2a/§5b)
    // =================================================================
    private static void RunPlacementCost()
    {
        Debug.Log("[EconomyVerification] --- Placement cost ---");
        EconomyTuning.ResetToDefaults();
        int neuPrice = EconomyTuning.NeutrophilPrice; // 15
        int macPrice = EconomyTuning.MacrophagePrice; // 40

        // 50 ATP: one macrophage (40) leaves 10; a second macrophage is then
        // unaffordable and must be refused.
        var rig = BuildRig("Buy");
        var wallet = new AtpWallet(50);
        var marrow = NewMarrow(rig, wallet);

        marrow.PlaceTower(0, UnitKind.Macrophage);
        Check("Placing a macrophage deducts its price",
            marrow.GetSlotState(0) == BoneMarrowSlotState.Placed && wallet.Balance == 50 - macPrice);

        Check("With 10 ATP left, a second 40-ATP macrophage is unaffordable", !wallet.CanAfford(macPrice));
        marrow.PlaceTower(1, UnitKind.Macrophage);
        Check("...and that placement is refused -- balance unchanged, slot still empty",
            marrow.GetSlotState(1) == BoneMarrowSlotState.Empty && wallet.Balance == 10);

        marrow.PlaceTower(1, UnitKind.Neutrophil); // 15 -- still too dear (10 left)
        Check("A 15-ATP neutrophil is also refused at 10 ATP",
            marrow.GetSlotState(1) == BoneMarrowSlotState.Empty && wallet.Balance == 10);

        wallet.Grant(20); // -> 30
        marrow.PlaceTower(1, UnitKind.Neutrophil);
        Check("Once affordable, the neutrophil places and deducts",
            marrow.GetSlotState(1) == BoneMarrowSlotState.Placed && wallet.Balance == 30 - neuPrice);

        // A null wallet keeps placement free (the harness path).
        var rigFree = BuildRig("Free");
        var marrowFree = NewMarrow(rigFree, null);
        marrowFree.PlaceTower(0, UnitKind.Macrophage);
        marrowFree.PlaceTower(1, UnitKind.Macrophage);
        Check("A null wallet leaves placement free",
            marrowFree.GetSlotState(0) == BoneMarrowSlotState.Placed && marrowFree.GetSlotState(1) == BoneMarrowSlotState.Placed);

        EconomyTuning.ResetToDefaults();
        rig.Dispose(); rigFree.Dispose();
    }

    // =================================================================
    // F. Per-kill ATP income (GAME_DESIGN.md §5b)
    // =================================================================
    private static void RunPerKillIncome()
    {
        Debug.Log("[EconomyVerification] --- Per-kill income ---");
        EconomyTuning.ResetToDefaults();
        var rig = BuildRig("Kill");
        var wallet = new AtpWallet(0);
        EconomyHooks.PayForKill = () => wallet.Grant(EconomyTuning.AtpPerKill);

        var profile = MakeProfile(UnitKind.Neutrophil);
        var go = new GameObject("EconomyVerification_KillUnit");
        rig.Junk.Add(go);
        go.AddComponent<SpriteRenderer>();
        var unit = go.AddComponent<SearchUnit>();
        var start = new FineCoord(10 * BoardConfig.FineSubdivision, 3 * BoardConfig.FineSubdivision);
        unit.Initialize(rig.Board, rig.Grid, rig.Field, profile, start, UnitLifecycleTuning.FromProfile(profile), -1, null);

        unit.RegisterKill();
        Check("SearchUnit.RegisterKill pays AtpPerKill through the hook",
            unit.Kills == 1 && wallet.Balance == EconomyTuning.AtpPerKill);

        unit.RegisterKill();
        unit.RegisterKill();
        Check("...once per kill", wallet.Balance == EconomyTuning.AtpPerKill * 3);

        EconomyHooks.PayForKill = null;
        int before = wallet.Balance;
        unit.RegisterKill();
        Check("With no hook wired, a kill pays nothing (harness default)", wallet.Balance == before);

        EconomyTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // G. The round boundary despawns fielded units (GAME_DESIGN.md §2)
    // =================================================================
    private static void RunRoundBoundaryClearsUnits()
    {
        Debug.Log("[EconomyVerification] --- Round boundary clears units ---");
        EconomyTuning.ResetToDefaults();
        var rig = BuildRig("Boundary");
        var wallet = new AtpWallet(1000);
        var marrow = NewMarrow(rig, wallet);

        marrow.PlaceTower(0, UnitKind.Neutrophil);
        marrow.PlaceTower(1, UnitKind.Macrophage);

        // Emit a few children.
        for (int i = 0; i < 60; i++) marrow.Tick(1f);
        Check("Towers emitted some units", marrow.TotalActiveUnits > 0);

        marrow.ClearFieldedUnits();
        Check("ClearFieldedUnits despawns every fielded unit", marrow.TotalActiveUnits == 0);
        Check("...but the towers stay placed",
            marrow.GetSlotState(0) == BoneMarrowSlotState.Placed && marrow.GetSlotState(1) == BoneMarrowSlotState.Placed);

        // They re-emit afterwards.
        for (int i = 0; i < 30; i++) marrow.Tick(1f);
        Check("Towers re-emit after the boundary", marrow.TotalActiveUnits > 0);

        EconomyTuning.ResetToDefaults();
        rig.Dispose();
    }
}
