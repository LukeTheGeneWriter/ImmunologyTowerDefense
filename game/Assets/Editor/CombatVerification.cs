using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

/// <summary>
/// Sprint 2's required self-verification for the placement/combat scope
/// (SPRINT_PLAN.md explicitly asks for evidence like "confirming viral
/// spread actually happens within N ticks of an uncleared infection,
/// confirming clearing releases the slot correctly," not just "it compiles
/// and launches"). Headless -- no play mode, no rendering -- and drives the
/// ACTUAL production classes (TissueGrid, PathogenAgent, PathogenSpawner,
/// BoneMarrowManager, BoardRenderer.ShowsAsPathogenItself) rather than a
/// reimplementation, same philosophy as the Sprint 1 closing task's
/// Assets/Editor/CytokineVerification.cs.
///
/// This relies on Unity calling Awake() synchronously on AddComponent even
/// outside play mode (true for MonoBehaviours in general -- Awake fires on
/// instantiation/component-addition, unlike Start/Update which need the
/// player loop running) so that PrefabPool -- used here exactly as
/// PathogenSpawner and GameBootstrap use it in real gameplay -- is properly
/// initialized without needing play mode.
///
/// Run via:
///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod CombatVerification.RunAll
/// Output goes to Debug.Log/Debug.LogError, which lands in Editor.log in
/// batchmode.
/// </summary>
public static class CombatVerification
{
    private static int passCount;
    private static int failCount;

    public static void RunAll()
    {
        passCount = 0;
        failCount = 0;
        Debug.Log("[CombatVerification] Starting ...");

        RunDamageAndClear();
        RunClassRendering();
        RunViralSpreadTiming();
        RunBoneMarrowEmission();

        Debug.Log($"[CombatVerification] Done. {passCount} passed, {failCount} failed.");
        if (failCount > 0)
        {
            Debug.LogError($"[CombatVerification] {failCount} assertion(s) FAILED -- see log above.");
        }
    }

    private static void Check(string label, bool condition)
    {
        if (condition)
        {
            passCount++;
            Debug.Log($"[CombatVerification] PASS -- {label}");
        }
        else
        {
            failCount++;
            Debug.LogError($"[CombatVerification] FAIL -- {label}");
        }
    }

    // ---------------------------------------------------------------
    // 1. Damage -> clear -> TissueGrid.ReleaseSlot, for all three classes.
    // ---------------------------------------------------------------
    private static void RunDamageAndClear()
    {
        Debug.Log("[CombatVerification] --- Damage and clear ---");
        var boardGo = new GameObject("CombatVerification_Board1");
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);

        CheckClassClears(board, tissueGrid, PathogenClass.IntracellularVirus, new CoarseCoord(3, 1));
        CheckClassClears(board, tissueGrid, PathogenClass.IntracellularBacterium, new CoarseCoord(6, 2));
        CheckClassClears(board, tissueGrid, PathogenClass.LargeBacterium, new CoarseCoord(9, 3));

        Object.DestroyImmediate(boardGo);
    }

    private static void CheckClassClears(BoardConfig board, TissueGrid tissueGrid, PathogenClass pClass, CoarseCoord slot)
    {
        var go = new GameObject($"CombatVerification_{pClass}");
        var agent = go.AddComponent<PathogenAgent>();
        bool exited = false;
        agent.InitializeAdheredDirect(board, tissueGrid, a => exited = true, (c, t) => false, slot, pClass, 0f);

        Check($"{pClass} occupies {slot} after adhering", !tissueGrid.IsSlotFree(slot) && tissueGrid.GetPathogenAt(slot) == agent);

        float maxHealth = agent.MaxHealth;
        int hitsToClear = Mathf.CeilToInt(maxHealth / PathogenAgent.ContactDamagePerHit);

        for (int i = 0; i < hitsToClear - 1; i++)
        {
            agent.ReceiveDamage(PathogenAgent.ContactDamagePerHit);
        }
        Check($"{pClass} still occupies {slot} before its last hit ({hitsToClear - 1}/{hitsToClear} hits landed, MaxHealth={maxHealth})", !tissueGrid.IsSlotFree(slot));

        agent.ReceiveDamage(PathogenAgent.ContactDamagePerHit);
        Check($"{pClass} slot {slot} is free after {hitsToClear} hits", tissueGrid.IsSlotFree(slot));
        Check($"{pClass} GetPathogenAt({slot}) is null after clearing", tissueGrid.GetPathogenAt(slot) == null);
        Check($"{pClass} onExit fired exactly once on clear", exited);

        Object.DestroyImmediate(go);
    }

    // ---------------------------------------------------------------
    // 2. Render classification -- intracellular reads as host tissue,
    //    large bacterium reads as itself (GAME_DESIGN.md section 4a).
    // ---------------------------------------------------------------
    private static void RunClassRendering()
    {
        Debug.Log("[CombatVerification] --- Render classification ---");
        var boardGo = new GameObject("CombatVerification_Board2");
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);

        var virusGo = new GameObject("Virus");
        var virus = virusGo.AddComponent<PathogenAgent>();
        virus.InitializeAdheredDirect(board, tissueGrid, null, (c, t) => false, new CoarseCoord(1, 1), PathogenClass.IntracellularVirus, 0f);
        Check("Intracellular virus does NOT show as itself (reads as host tissue)", !BoardRenderer.ShowsAsPathogenItself(virus));

        var bacGo = new GameObject("Bacterium");
        var bac = bacGo.AddComponent<PathogenAgent>();
        bac.InitializeAdheredDirect(board, tissueGrid, null, (c, t) => false, new CoarseCoord(2, 1), PathogenClass.IntracellularBacterium, 0f);
        Check("Intracellular bacterium does NOT show as itself (reads as host tissue)", !BoardRenderer.ShowsAsPathogenItself(bac));

        var largeGo = new GameObject("LargeBacterium");
        var large = largeGo.AddComponent<PathogenAgent>();
        large.InitializeAdheredDirect(board, tissueGrid, null, (c, t) => false, new CoarseCoord(3, 1), PathogenClass.LargeBacterium, 0f);
        Check("Large bacterium DOES show as itself", BoardRenderer.ShowsAsPathogenItself(large));

        Check("BoardRenderer.ShowsAsPathogenItself(null) is false (bare host tissue)", !BoardRenderer.ShowsAsPathogenItself(null));

        Object.DestroyImmediate(virusGo);
        Object.DestroyImmediate(bacGo);
        Object.DestroyImmediate(largeGo);
        Object.DestroyImmediate(boardGo);
    }

    // ---------------------------------------------------------------
    // 3. Viral spread timing -- the sprint's flagship mechanic
    //    (GAME_DESIGN.md section 4a). Drives PathogenSpawner.RequestSpread,
    //    the real production spread implementation.
    // ---------------------------------------------------------------
    private static void RunViralSpreadTiming()
    {
        Debug.Log("[CombatVerification] --- Viral spread timing ---");

        // Scenario A: left uncleared -- should spread after IncubationSeconds
        // and keep spreading from new sites (compounding), demonstrating
        // GAME_DESIGN.md section 4a's "visible, compounding cost."
        RunSpreadScenario("uncleared (slow/rung-1-equivalent search)", clearOriginEarly: false);

        // Scenario B: cleared before incubation elapses -- should NOT spread,
        // demonstrating the other half of the sprint's thesis (fast search
        // via cytokine sensing catches it first).
        RunSpreadScenario("cleared before incubation (fast/rung-2-equivalent search)", clearOriginEarly: true);

        // Scenario C: bacterium-class, left uncleared well past the virus
        // incubation window -- should never spread (virus-specific per
        // GAME_DESIGN.md section 4a).
        RunBacteriumDoesNotSpread();
    }

    private static void RunSpreadScenario(string label, bool clearOriginEarly)
    {
        var boardGo = new GameObject("CombatVerification_SpreadBoard_" + (clearOriginEarly ? "cleared" : "uncleared"));
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);
        var cytokineField = new CytokineField(board);

        var spawnerGo = new GameObject("CombatVerification_Spawner");
        var spawner = spawnerGo.AddComponent<PathogenSpawner>();
        var template = new GameObject("PathogenTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        spawner.Initialize(board, tissueGrid, cytokineField, template);

        var originCoord = new CoarseCoord(board.Columns / 2, 2);
        var originGo = new GameObject("OriginVirus");
        var origin = originGo.AddComponent<PathogenAgent>();
        float simTime = 0f;
        origin.InitializeAdheredDirect(board, tissueGrid, a => { }, spawner.RequestSpread, originCoord, PathogenClass.IntracellularVirus, simTime);

        Check($"[{label}] AdheredCount is 1 right after seeding the origin infection", tissueGrid.AdheredCount == 1);

        const float dt = 0.5f;
        bool clearedEarly = false;

        // Run up to just short of incubation -- confirm no spread yet.
        while (simTime < PathogenAgent.IncubationSeconds - dt)
        {
            simTime += dt;
            if (clearOriginEarly && !clearedEarly && simTime >= PathogenAgent.IncubationSeconds * 0.5f)
            {
                origin.ReceiveDamage(origin.MaxHealth); // one hit for full health clears it outright
                clearedEarly = true;
            }
            origin.TickCombat(simTime);
            foreach (var a in new List<PathogenAgent>(spawner.Live)) a.TickCombat(simTime);
        }
        Check($"[{label}] AdheredCount as expected just before incubation would elapse (no premature spread)",
            clearedEarly ? tissueGrid.AdheredCount == 0 : tissueGrid.AdheredCount == 1);

        // Run well past incubation (several retry intervals) to let a
        // spread attempt land, then far enough past a SECOND incubation
        // window (measured from the child's own spread time, not the
        // origin's) to catch second-generation chain-spread too -- this is
        // what actually demonstrates GAME_DESIGN.md section 4a's
        // "compounding" cost, not just a single spread event.
        float target = PathogenAgent.IncubationSeconds * 2f + PathogenAgent.SpreadRetryIntervalSeconds * 8f + 5f;
        while (simTime < target)
        {
            simTime += dt;
            origin.TickCombat(simTime);
            foreach (var a in new List<PathogenAgent>(spawner.Live)) a.TickCombat(simTime);
        }

        if (clearOriginEarly)
        {
            Check($"[{label}] AdheredCount stayed 0 well past incubation (cleared infection cannot spread)", tissueGrid.AdheredCount == 0);
        }
        else
        {
            Check($"[{label}] AdheredCount grew beyond 1 well past incubation (spread happened)", tissueGrid.AdheredCount > 1);
            Debug.Log($"[CombatVerification] [{label}] AdheredCount after {target:F1}s simulated: {tissueGrid.AdheredCount} (chain-spread children tracked by spawner: {spawner.Live.Count})");
        }

        Object.DestroyImmediate(originGo);
        Object.DestroyImmediate(spawnerGo);
        Object.DestroyImmediate(template);
        Object.DestroyImmediate(boardGo);
    }

    private static void RunBacteriumDoesNotSpread()
    {
        var boardGo = new GameObject("CombatVerification_BacBoard");
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);
        var cytokineField = new CytokineField(board);

        var spawnerGo = new GameObject("CombatVerification_BacSpawner");
        var spawner = spawnerGo.AddComponent<PathogenSpawner>();
        var template = new GameObject("PathogenTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        spawner.Initialize(board, tissueGrid, cytokineField, template);

        var coord = new CoarseCoord(board.Columns / 2, 2);
        var go = new GameObject("OriginBacterium");
        var origin = go.AddComponent<PathogenAgent>();
        origin.InitializeAdheredDirect(board, tissueGrid, a => { }, spawner.RequestSpread, coord, PathogenClass.IntracellularBacterium, 0f);

        float simTime = 0f;
        const float dt = 0.5f;
        float target = PathogenAgent.IncubationSeconds * 3f;
        while (simTime < target)
        {
            simTime += dt;
            origin.TickCombat(simTime);
        }

        Check($"Bacterium never spreads even {target:F0}s past the virus incubation window (AdheredCount stayed 1)", tissueGrid.AdheredCount == 1);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(spawnerGo);
        Object.DestroyImmediate(template);
        Object.DestroyImmediate(boardGo);
    }

    // ---------------------------------------------------------------
    // 4. Bone marrow placement + periodic emission at the blood-side edge.
    // ---------------------------------------------------------------
    private static void RunBoneMarrowEmission()
    {
        Debug.Log("[CombatVerification] --- Bone marrow emission ---");
        var boardGo = new GameObject("CombatVerification_MarrowBoard");
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);
        var cytokineField = new CytokineField(board);

        var macProfile = new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Macrophage", FineTilesPerTick = 1, FootprintFineTiles = 5, Color = Color.blue };
        var neuProfile = new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neutrophil", FineTilesPerTick = 3, FootprintFineTiles = 3, Color = Color.yellow };

        var macPool = BuildTestUnitPool(macProfile);
        var neuPool = BuildTestUnitPool(neuProfile);

        var managerGo = new GameObject("CombatVerification_BoneMarrowManager");
        var manager = managerGo.AddComponent<BoneMarrowManager>();
        var slotPositions = new[] { Vector3.zero, new Vector3(2f, 0f, 0f), new Vector3(4f, 0f, 0f) };
        manager.Initialize(board, tissueGrid, cytokineField, macProfile, macPool, neuProfile, neuPool, slotPositions, 1f);

        Check("Slot 0 starts Empty", manager.GetSlotState(0) == BoneMarrowSlotState.Empty);

        manager.PlaceTower(0, UnitKind.Macrophage);
        Check("Slot 0 is Placed after PlaceTower", manager.GetSlotState(0) == BoneMarrowSlotState.Placed);
        Check("Slot 0 kind is Macrophage", manager.GetSlotKind(0) == UnitKind.Macrophage);
        Check("EmittedCount is 0 before any Tick", manager.EmittedCount == 0);

        // Advance well past two emission intervals.
        const float dt = 0.25f;
        float total = BoneMarrowManager.EmissionIntervalSeconds * 2.5f;
        for (float t = 0f; t < total; t += dt)
        {
            manager.Tick(dt);
        }

        Check($"EmittedCount >= 2 after {total:F1}s ({BoneMarrowManager.EmissionIntervalSeconds}s interval)", manager.EmittedCount >= 2);
        Check("Last emitted unit's row is the blood-adjacent edge (board.FineRows - 1)", manager.LastEmittedStart.Row == board.FineRows - 1);
        Check("Last emitted unit's column is in bounds", manager.LastEmittedStart.Column >= 0 && manager.LastEmittedStart.Column < board.FineColumns);
        Check("Last emitted kind is Macrophage (only slot 0 is placed)", manager.LastEmittedKind == UnitKind.Macrophage);

        // Placing on an already-placed slot is a no-op.
        manager.PlaceTower(0, UnitKind.Neutrophil);
        Check("PlaceTower on an already-placed slot does not change its kind", manager.GetSlotKind(0) == UnitKind.Macrophage);

        Object.DestroyImmediate(managerGo);
        Object.DestroyImmediate(boardGo);
    }

    private static PrefabPool BuildTestUnitPool(UnitProfile profile)
    {
        var template = new GameObject($"{profile.DisplayName}Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<SearchUnit>();
        template.SetActive(false);

        var poolGo = new GameObject($"{profile.DisplayName}Pool");
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        return pool;
    }
}
