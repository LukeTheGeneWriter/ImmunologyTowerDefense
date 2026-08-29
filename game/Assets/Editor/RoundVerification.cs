using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;

/// <summary>
/// Sprint 9 verification: the reworked round model (SPRINT_PLAN.md) --
/// a frozen buy phase, a battlefield that persists round to round, and a
/// contaminated food item that delivers each round's batch. Same
/// drive-the-real-classes, no-Play-Mode philosophy as the seven before it.
///
/// The Update()-only freeze GATE (every agent's `if (RoundClock.Frozen)
/// return`) can't be exercised headlessly -- Update() doesn't run in
/// batchmode -- so that is covered by the build launch. This harness
/// covers the freeze FLAG state machine, the RoundScript table, and the
/// food-item delivery / persistence behaviour.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod RoundVerification.RunAll
/// </summary>
public static class RoundVerification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[RoundVerification] Starting ...");

        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();

        RunRoundClock();
        RunRoundScript();
        RunFoodDelivery();
        RunFreezeAndPersistence();

        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();

        Debug.Log($"[RoundVerification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool condition)
    {
        if (condition) { passed++; Debug.Log($"[RoundVerification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[RoundVerification] FAIL -- {label}"); }
    }

    // =================================================================
    // 1. RoundClock -- the freeze flag + the gated sim clock
    // =================================================================

    private static void RunRoundClock()
    {
        RoundClock.Reset();
        Check("opens frozen", RoundClock.Frozen);
        Check("clock starts at 0", Mathf.Approximately(RoundClock.Time, 0f));

        RoundClock.Advance(1f);
        Check("Advance while frozen does nothing", Mathf.Approximately(RoundClock.Time, 0f));

        RoundClock.Frozen = false;
        RoundClock.Advance(2f);
        RoundClock.Advance(0.5f);
        Check("Advance while running accumulates", Mathf.Approximately(RoundClock.Time, 2.5f));

        RoundClock.Frozen = true;
        RoundClock.Advance(10f);
        Check("re-freezing holds the clock", Mathf.Approximately(RoundClock.Time, 2.5f));

        RoundClock.Reset();
    }

    // =================================================================
    // 2. RoundScript -- taglines + class mix
    // =================================================================

    private static void RunRoundScript()
    {
        var r1 = RoundScript.ForRound(1);
        var r6 = RoundScript.ForRound(6);
        Check("round 1 has a scripted tagline", !string.IsNullOrEmpty(r1.Tagline));
        Check("round 6 has a scripted tagline", !string.IsNullOrEmpty(r6.Tagline));
        Check("scripted rounds 1 and 6 differ", r1.Tagline != r6.Tagline);

        var r99 = RoundScript.ForRound(99);
        Check("past the script -> a procedural tagline mentioning the round", r99.Tagline.Contains("99"));

        // A virus-only definition never rolls anything else.
        var virusOnly = new RoundDefinition { VirusWeight = 1f, BacteriumWeight = 0f, LargeBacteriumWeight = 0f };
        bool allVirus = true;
        for (int i = 0; i < 200; i++)
            if (virusOnly.RollClass() != PathogenClass.IntracellularVirus) { allVirus = false; break; }
        Check("a virus-only mix only ever rolls virus", allVirus);

        var zero = new RoundDefinition { VirusWeight = 0f, BacteriumWeight = 0f, LargeBacteriumWeight = 0f };
        Check("an all-zero mix still returns a class (no divide-by-zero)",
            zero.RollClass() == PathogenClass.LargeBacterium);
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
            for (int i = 0; i < Junk.Count; i++) if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static Rig BuildRig(string name)
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject($"RoundVerification_Board_{name}");
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
        var go = new GameObject("RoundVerification_Spawner");
        rig.Junk.Add(go);
        var spawner = go.AddComponent<PathogenSpawner>();
        var template = new GameObject("RoundVerification_PathogenTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        rig.Junk.Add(template);
        spawner.Initialize(rig.Board, rig.Grid, rig.Field, rig.Gut, rig.Tally, template);
        return spawner;
    }

    private static RoundController NewRounds(Rig rig, AtpWallet wallet, PathogenSpawner spawner, BoneMarrowManager marrow)
    {
        var go = new GameObject("RoundVerification_Rounds");
        rig.Junk.Add(go);
        var rc = go.AddComponent<RoundController>();
        rc.Initialize(wallet, spawner, rig.Tally, marrow);
        return rc;
    }

    /// <summary>Ticks spawner + every live agent + the round controller with
    /// a fixed step until the round ends (Phase back to Building) or a
    /// budget runs out.</summary>
    private static bool DriveToRoundEnd(Rig rig, PathogenSpawner spawner, RoundController rounds, int maxSteps = 4000)
    {
        float t = 0f;
        for (int i = 0; i < maxSteps; i++)
        {
            t += 0.1f;
            spawner.Tick(0.1f, t);
            foreach (var a in new List<PathogenAgent>(spawner.Live)) a.SimulationTick(0.1f, t);
            rounds.Tick(0.1f);
            if (rounds.Phase != RoundPhase.Active) return true;
        }
        return false;
    }

    // =================================================================
    // 3. Food delivery -- the food transits and delivers the full batch
    // =================================================================

    private static void RunFoodDelivery()
    {
        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionChanceAtWall = 0f; // wash-through, isolate the delivery
        RoundClock.Reset();

        var rig = BuildRig("Food");
        var wallet = new AtpWallet(EconomyTuning.StartingAtp);
        var spawner = NewSpawner(rig);
        var rounds = NewRounds(rig, wallet, spawner, null);

        int expected = EconomyTuning.BatchSizeForRound(1);
        rounds.StartRound();
        Check("StartRound arms a FOOD round", spawner.FoodActive && spawner.BatchTarget == expected);
        Check("StartRound unfreezes", !RoundClock.Frozen);
        Check("StartRound set the round-1 tagline", rounds.CurrentTagline == RoundScript.ForRound(1).Tagline);

        // Part-way through: not complete just because the batch is emitted --
        // the food still has to leave the channel.
        float t = 0f;
        for (int i = 0; i < 600 && spawner.BatchEmitted < expected; i++) { t += 0.1f; spawner.Tick(0.1f, t); }
        Check("the food emits the whole batch as it travels", spawner.BatchEmitted == expected);
        Check("not complete while the food is still in the lumen (the last burst lands before the food exits)",
            spawner.FoodActive && !spawner.BatchComplete);

        bool ended = DriveToRoundEnd(rig, spawner, rounds);
        Check("round ends once the food has exited", ended && rounds.Phase == RoundPhase.Building);
        Check("...and the batch delivered its full count", spawner.BatchEmitted == expected);
        Check("...and every one of them was excreted (adhesion 0)", rig.Tally.Excreted == expected);
        Check("round clear re-freezes the field", RoundClock.Frozen);
        Check("round clear granted the lump sum",
            wallet.Balance == EconomyTuning.StartingAtp + EconomyTuning.RoundStartLumpSum);

        // Round 2 delivers a bigger, differently-themed batch on top.
        rounds.StartRound();
        Check("round 2 is bigger", spawner.BatchTarget == EconomyTuning.BatchSizeForRound(2)
            && EconomyTuning.BatchSizeForRound(2) > expected);
        Check("round 2 has its own tagline", rounds.CurrentTagline == RoundScript.ForRound(2).Tagline);

        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
        rig.Dispose();
    }

    // =================================================================
    // 4. The battlefield persists across a round boundary
    // =================================================================

    private static void RunFreezeAndPersistence()
    {
        EconomyTuning.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionChanceAtWall = 0.95f; // make the batch STICK
        RoundClock.Reset();

        var rig = BuildRig("Persist");
        var wallet = new AtpWallet(EconomyTuning.StartingAtp);
        var spawner = NewSpawner(rig);

        // A bone marrow with one placed, emitting macrophage tower.
        var mac = new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5, MaxActiveChildren = 10, KillLimit = 20 };
        var neu = new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3, MaxActiveChildren = 10, KillLimit = 5 };
        var macPool = NewUnitPool(rig, mac);
        var neuPool = NewUnitPool(rig, neu);
        var marrowGo = new GameObject("RoundVerification_Marrow");
        rig.Junk.Add(marrowGo);
        var marrow = marrowGo.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[5];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(i * 2f, -8f, 0f);
        marrow.Initialize(rig.Board, rig.Grid, rig.Field, mac, macPool, neu, neuPool, positions, 1f, null);
        marrow.PlaceTower(0, UnitKind.Macrophage);

        var rounds = NewRounds(rig, wallet, spawner, marrow);

        rounds.StartRound();
        for (int i = 0; i < 120; i++) marrow.Tick(0.1f); // emit a few cells
        int fieldedBefore = marrow.TotalActiveUnits;
        Check("towers put cells in the field during the round", fieldedBefore >= 1);

        bool ended = DriveToRoundEnd(rig, spawner, rounds);
        Check("round ended", ended && rounds.Phase == RoundPhase.Building);

        Check("fielded immune cells PERSIST across the boundary (not despawned)",
            marrow.TotalActiveUnits == fieldedBefore);
        Check("loose pathogens PERSIST across the boundary",
            spawner.LiveCount > 0);
        Check("the boundary re-froze the field", RoundClock.Frozen);

        // The kept-for-restart escape hatch still works.
        rounds.DespawnAllFieldedUnits();
        Check("DespawnAllFieldedUnits still clears the field for a restart",
            marrow.TotalActiveUnits == 0);

        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
        rig.Dispose();
    }

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
}
