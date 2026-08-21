using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;

/// <summary>
/// Sprint 3's required self-verification for the unit-lifecycle scope
/// (SPRINT_PLAN.md items 1-7 and its stopping-point checklist). Headless --
/// no play mode, no rendering -- and drives the ACTUAL production classes
/// (BoneMarrowManager, SearchUnit, PathogenAgent, TissueGrid, PrefabPool),
/// not a reimplementation. Same philosophy and structure as Sprint 2's
/// Assets/Editor/CombatVerification.cs.
///
/// **Why a sibling file rather than more groups inside CombatVerification:**
/// CombatVerification is Sprint 2's evidence for Sprint 2's claims, and it
/// still passes unchanged (beyond the ReceiveDamage signature update this
/// sprint forced on it) -- which is itself a useful regression signal. A
/// separate file keeps "did Sprint 2 still work" and "does Sprint 3 work"
/// separately runnable and separately readable. The cost is two batchmode
/// invocations instead of one; that is cheap.
///
/// Run via:
///   Unity.exe -batchmode -quit -projectPath &lt;path&gt; -executeMethod LifecycleVerification.RunAll
/// Output goes to Debug.Log/Debug.LogError, which lands in Editor.log in
/// batchmode.
/// </summary>
public static class LifecycleVerification
{
    private static int passCount;
    private static int failCount;

    /// <summary>Simulated step used throughout. 0.125 == 2^-3, so it is
    /// exactly representable as a float and repeated accumulation into
    /// BoneMarrowManager's emission timer stays exact -- which is what lets
    /// these tests assert precise emission counts ("exactly 2 cells 8s after
    /// a mass death") instead of hiding rate bugs behind a tolerance.</summary>
    private const float Dt = 0.125f;

    public static void RunAll()
    {
        passCount = 0;
        failCount = 0;
        Debug.Log("[LifecycleVerification] Starting ...");

        RunMaxActiveChildrenCap();
        RunEmissionRateCapAfterMassDeath();
        RunNeutrophilDegranulation();
        RunMacrophageQuietRetirement();
        RunKillAttribution();
        RunProximityContact();
        RunPerTowerTuningIsMutable();
        RunLongSimulation();
        RunContactRateDiagnostic();

        Debug.Log($"[LifecycleVerification] Done. {passCount} passed, {failCount} failed.");
        if (failCount > 0)
        {
            Debug.LogError($"[LifecycleVerification] {failCount} assertion(s) FAILED -- see log above.");
        }
    }

    /// <summary>True when every unit in the list reports the given kill
    /// limit. Used to prove an upgrade reached ALL of a tower's live
    /// children, not just the first one (Director, 2026-08-21).</summary>
    private static bool AllKillLimitsAre(IReadOnlyList<SearchUnit> units, int expected)
    {
        if (units.Count == 0) return false; // an empty list must not pass vacuously
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].KillLimit != expected) return false;
        }
        return true;
    }


    // ---------------------------------------------------------------
    // Sprint 4 shim: PathogenAgent.InitializeAdheredDirect became
    // InitializeInTissueDirect and now takes the GutInterface and
    // InvasionTally, because "adhered" means the gut wall in Sprint 4's
    // model, not tissue. These harness fixtures don't exercise the wall,
    // so they share one throwaway interface/tally per BoardConfig.
    // ---------------------------------------------------------------
    private static InvasionTally harnessTally;
    private static BoardConfig harnessGutBoard;
    private static GutInterface harnessGut;

    private static InvasionTally HarnessTally =>
        harnessTally ?? (harnessTally = new InvasionTally());

    private static GutInterface HarnessGut(BoardConfig board, TissueGrid grid)
    {
        if (harnessGut == null || harnessGutBoard != board)
        {
            harnessTally = new InvasionTally();
            harnessGut = new GutInterface(board, grid, harnessTally);
            harnessGutBoard = board;
        }
        return harnessGut;
    }

    private static void Check(string label, bool condition)
    {
        if (condition)
        {
            passCount++;
            Debug.Log($"[LifecycleVerification] PASS -- {label}");
        }
        else
        {
            failCount++;
            Debug.LogError($"[LifecycleVerification] FAIL -- {label}");
        }
    }

    // =================================================================
    // Fixtures -- mirror GameBootstrap's real profiles exactly, including
    // this sprint's lifecycle defaults, so the numbers asserted here are
    // the numbers the built game actually runs.
    // =================================================================

    private static UnitProfile MacrophageProfile() => new UnitProfile
    {
        Kind = UnitKind.Macrophage,
        DisplayName = "Macrophage",
        FineTilesPerTick = 1,
        FootprintFineTiles = 5,
        Color = new Color(0.30f, 0.40f, 0.80f),
        MaxActiveChildren = 10,
        KillLimit = 20,
        DegranulatesOnDepletion = false,
        DegranulationBurstMultiplier = 0f,
        ContactRadiusFineTiles = 2,
    };

    private static UnitProfile NeutrophilProfile() => new UnitProfile
    {
        Kind = UnitKind.Neutrophil,
        DisplayName = "Neutrophil",
        FineTilesPerTick = 3,
        FootprintFineTiles = 3,
        Color = new Color(0.95f, 0.78f, 0.25f),
        MaxActiveChildren = 10,
        KillLimit = 5,
        DegranulatesOnDepletion = true,
        DegranulationBurstMultiplier = 3f,
        ContactRadiusFineTiles = 2,
    };

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public BoneMarrowManager Manager;
        public GameObject ManagerGo;
        public UnitProfile Mac;
        public UnitProfile Neu;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            foreach (var go in Junk) if (go != null) Object.DestroyImmediate(go);
            if (ManagerGo != null) Object.DestroyImmediate(ManagerGo);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static Rig BuildRig(string name, int slotCount)
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject($"LifecycleVerification_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Mac = MacrophageProfile();
        rig.Neu = NeutrophilProfile();

        var macPool = BuildUnitPool(rig.Mac, rig.Junk);
        var neuPool = BuildUnitPool(rig.Neu, rig.Junk);

        rig.ManagerGo = new GameObject($"LifecycleVerification_Marrow_{name}");
        rig.Manager = rig.ManagerGo.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[slotCount];
        for (int i = 0; i < slotCount; i++) positions[i] = new Vector3(i * 2f, -5f, 0f);
        rig.Manager.Initialize(rig.Board, rig.Grid, rig.Field, rig.Mac, macPool, rig.Neu, neuPool, positions, 1f);
        return rig;
    }

    private static PrefabPool BuildUnitPool(UnitProfile profile, List<GameObject> junk)
    {
        var template = new GameObject($"{profile.DisplayName}Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<SearchUnit>();
        template.SetActive(false);
        junk.Add(template);

        var poolGo = new GameObject($"{profile.DisplayName}Pool");
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        junk.Add(poolGo);
        return pool;
    }

    /// <summary>Ticks the manager until it emits one more unit (or gives up),
    /// then re-Initializes that unit at a chosen fine tile so a test can put
    /// a real, tower-owned unit next to a real pathogen. Everything about it
    /// stays production: it is still in its tower's Children list, still
    /// holds the tower's tuning snapshot, and still despawns through
    /// BoneMarrowManager.OnChildDespawned.</summary>
    private static SearchUnit EmitAt(Rig rig, int slotIndex, FineCoord at)
    {
        // Ticks in exactly-representable steps (0.25 == 2^-2) so the
        // emission timer's float arithmetic is exact and the tests can
        // assert on precise emission counts rather than tolerances.
        int before = rig.Manager.GetChildren(slotIndex).Count;
        for (int i = 0; i < 400 && rig.Manager.GetChildren(slotIndex).Count == before; i++) rig.Manager.Tick(0.25f);
        var kids = rig.Manager.GetChildren(slotIndex);
        if (kids.Count == before) return null;
        var unit = kids[kids.Count - 1];

        var profile = rig.Manager.GetSlotKind(slotIndex) == UnitKind.Macrophage ? rig.Mac : rig.Neu;
        int captured = slotIndex;
        unit.Initialize(rig.Board, rig.Grid, rig.Field, profile, at,
            rig.Manager.GetTuning(slotIndex), slotIndex, u => rig.Manager.OnChildDespawned(captured, u));
        return unit;
    }

    /// <summary>Drives a unit to depletion through the real production path
    /// -- RegisterKill (what PathogenAgent calls on the killing hit) until
    /// the limit, then ResolveDepletionIfDue (what SimulationTick calls at
    /// the end of every tick).</summary>
    private static void DepleteThroughRealPath(SearchUnit unit)
    {
        int guard = 0;
        while (!unit.IsDepletionDue && guard++ < 1000) unit.RegisterKill();
        unit.ResolveDepletionIfDue();
    }

    private static PathogenAgent SeedPathogen(Rig rig, CoarseCoord slot, PathogenClass pClass)
    {
        var go = new GameObject($"Pathogen_{slot}");
        rig.Junk.Add(go);
        var agent = go.AddComponent<PathogenAgent>();
        agent.InitializeInTissueDirect(rig.Board, rig.Grid, HarnessGut(rig.Board, rig.Grid), HarnessTally, a => { }, (c, t) => false, slot, pClass, 0f);
        return agent;
    }

    private static FineCoord CenterOf(CoarseCoord c) => new FineCoord(
        c.Column * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2,
        c.Row * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2);

    // =================================================================
    // 1. Max active children per tower (SPRINT_PLAN.md item 1).
    // =================================================================
    private static void RunMaxActiveChildrenCap()
    {
        Debug.Log("[LifecycleVerification] --- Max active children per tower ---");
        var rig = BuildRig("cap", 1);
        rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
        int cap = rig.Manager.GetTuning(0).MaxActiveChildren;
        Check($"A freshly placed tower seeds its own tuning from the profile default (cap {cap})", cap == 10);
        Check("A freshly placed tower has 0 active children", rig.Manager.GetActiveChildren(0) == 0);

        // Long enough to emit far more than the cap if nothing stopped it:
        // 500s / 4s = 125 emissions' worth of time. (dt is 0.125 -- exactly
        // representable in binary floating point, so the emission timer's
        // arithmetic is exact and these counts can be asserted precisely.)
        for (int i = 0; i < 4000; i++) rig.Manager.Tick(Dt);

        Check($"Tower stops at its cap: {rig.Manager.GetActiveChildren(0)} alive after 500 simulated seconds (cap {cap})",
            rig.Manager.GetActiveChildren(0) == cap);
        Check($"Tower emitted exactly {cap} units total, not one per interval ({rig.Manager.EmittedCount} emitted)",
            rig.Manager.EmittedCount == cap);
        Debug.Log($"[LifecycleVerification] Uncapped Sprint 2 behaviour would have emitted {(int)(500f / BoneMarrowManager.EmissionIntervalSeconds)} units over the same window.");

        // One child dies -> the tower resumes.
        var victim = rig.Manager.GetChildren(0)[0];
        DepleteThroughRealPath(victim);
        Check($"Depleting one child frees a slot ({rig.Manager.GetActiveChildren(0)} alive)", rig.Manager.GetActiveChildren(0) == cap - 1);

        int emittedBefore = rig.Manager.EmittedCount;
        rig.Manager.Tick(Dt);
        Check("Tower resumes emitting once a slot frees up", rig.Manager.EmittedCount == emittedBefore + 1);
        Check($"Tower is back at its cap and stays there ({rig.Manager.GetActiveChildren(0)} alive)", rig.Manager.GetActiveChildren(0) == cap);

        for (int i = 0; i < 1000; i++) rig.Manager.Tick(Dt);
        Check($"Still exactly at the cap after another 125 simulated seconds ({rig.Manager.GetActiveChildren(0)} alive)",
            rig.Manager.GetActiveChildren(0) == cap);

        rig.Dispose();
    }

    // =================================================================
    // 2. The emission-rate cap survives the max-children cap
    //    (SPRINT_PLAN.md item 2): a tower whose whole population dies at
    //    once refills at the rate cap, it does not burst back to full.
    // =================================================================
    private static void RunEmissionRateCapAfterMassDeath()
    {
        Debug.Log("[LifecycleVerification] --- Emission rate cap after mass death ---");
        var rig = BuildRig("rate", 1);
        rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
        int cap = rig.Manager.GetTuning(0).MaxActiveChildren;

        for (int i = 0; i < 4000; i++) rig.Manager.Tick(Dt);
        Check("Tower is saturated before the mass-death event", rig.Manager.GetActiveChildren(0) == cap);

        // Kill every child on the same "tick".
        foreach (var u in new List<SearchUnit>(rig.Manager.GetChildren(0))) DepleteThroughRealPath(u);
        Check("Whole population is gone", rig.Manager.GetActiveChildren(0) == 0);

        rig.Manager.Tick(Dt);
        Check($"Immediately after mass death the tower has at most one cell, not a full refill ({rig.Manager.GetActiveChildren(0)} alive)",
            rig.Manager.GetActiveChildren(0) <= 1);

        // 3.75s more -- still short of a second full emission interval.
        for (int i = 0; i < 30; i++) rig.Manager.Tick(Dt);
        Check($"Still 1 cell 3.875s after mass death ({BoneMarrowManager.EmissionIntervalSeconds}s interval; {rig.Manager.GetActiveChildren(0)} alive)",
            rig.Manager.GetActiveChildren(0) == 1);

        for (int i = 0; i < 32; i++) rig.Manager.Tick(Dt);
        Check($"Exactly 2 cells 7.875s after mass death -- the refill is rate-limited, not a burst ({rig.Manager.GetActiveChildren(0)} alive)",
            rig.Manager.GetActiveChildren(0) == 2);

        // Refilling all 10 must take roughly cap * interval seconds.
        for (int i = 0; i < 400; i++) rig.Manager.Tick(Dt);
        Check($"Back to the cap only after ~{cap * BoneMarrowManager.EmissionIntervalSeconds}s of refilling ({rig.Manager.GetActiveChildren(0)} alive)",
            rig.Manager.GetActiveChildren(0) == cap);

        rig.Dispose();
    }

    // =================================================================
    // 3. Neutrophil degranulation (SPRINT_PLAN.md item 3).
    // =================================================================
    private static void RunNeutrophilDegranulation()
    {
        Debug.Log("[LifecycleVerification] --- Neutrophil degranulation ---");

        // 3a. Degranulating on an occupied slot damages the occupant.
        {
            var rig = BuildRig("degran", 1);
            rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
            var slot = new CoarseCoord(5, 2);
            var pathogen = SeedPathogen(rig, slot, PathogenClass.LargeBacterium);
            var unit = EmitAt(rig, 0, CenterOf(slot));

            Check("Neutrophil tower's kill limit is 5", unit.KillLimit == 5);
            Check("Neutrophil is flagged as degranulating on depletion", unit.DegranulatesOnDepletion);

            for (int i = 0; i < 4; i++) unit.RegisterKill();
            Check("Neutrophil with 4/5 kills is NOT due to deplete", !unit.IsDepletionDue);
            Check("ResolveDepletionIfDue is a no-op below the limit", !unit.ResolveDepletionIfDue());
            Check("Under-limit neutrophil still occupies its tower slot", rig.Manager.GetActiveChildren(0) == 1);

            unit.RegisterKill();
            Check("Neutrophil with 5/5 kills IS due to deplete", unit.IsDepletionDue);

            float before = pathogen.Health;
            float expectedBurst = PathogenAgent.ContactDamagePerHit * rig.Manager.GetTuning(0).DegranulationBurstMultiplier;
            bool depleted = unit.ResolveDepletionIfDue();
            Check("ResolveDepletionIfDue reports the unit depleted", depleted);
            Check($"Degranulation dealt a {expectedBurst}x-flat collateral burst to the occupied slot ({before} -> {pathogen.Health})",
                Mathf.Approximately(pathogen.Health, before - expectedBurst));
            Check("Occupant survived a non-lethal burst and still holds its slot", !rig.Grid.IsSlotFree(slot));
            Check("Degranulated neutrophil freed its tower's population slot", rig.Manager.GetActiveChildren(0) == 0);
            Check("Despawned unit was reset for the pool (kill count cleared)", unit.Kills == 0);
            Check("Despawned unit's GameObject is inactive (returned to the pool, not destroyed)", !unit.gameObject.activeSelf);

            rig.Dispose();
        }

        // 3b. A lethal burst clears the slot outright, through the same
        //     TissueGrid.ReleaseSlot path ordinary combat uses.
        {
            var rig = BuildRig("degran_lethal", 1);
            rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
            var slot = new CoarseCoord(7, 1);
            var pathogen = SeedPathogen(rig, slot, PathogenClass.LargeBacterium);
            pathogen.ReceiveDamage(pathogen.MaxHealth - 2f, null); // soften to 2 HP, unattributed
            Check($"Softened occupant is at {pathogen.Health} HP and still adhered", pathogen.Health <= 2f && !rig.Grid.IsSlotFree(slot));

            var unit = EmitAt(rig, 0, CenterOf(slot));
            for (int i = 0; i < unit.KillLimit; i++) unit.RegisterKill();
            unit.ResolveDepletionIfDue();

            Check("A lethal degranulation burst clears the slot", rig.Grid.IsSlotFree(slot));
            Check("...and GetPathogenAt is null afterwards", rig.Grid.GetPathogenAt(slot) == null);
            Check("...and the degranulating unit still freed its tower slot", rig.Manager.GetActiveChildren(0) == 0);

            rig.Dispose();
        }

        // 3c. Degranulating on bare host tissue: no crash, nothing to damage
        //     (no fibrosis system yet -- SPRINT_PLAN.md item 3), slot freed.
        {
            var rig = BuildRig("degran_bare", 1);
            rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
            var bare = new CoarseCoord(11, 3);
            var unit = EmitAt(rig, 0, CenterOf(bare));
            for (int i = 0; i < unit.KillLimit; i++) unit.RegisterKill();
            bool ok = unit.ResolveDepletionIfDue();
            Check("Degranulating on bare host tissue is harmless and still despawns", ok && rig.Manager.GetActiveChildren(0) == 0);
            Check("Bare slot stays bare", rig.Grid.IsSlotFree(bare));
            rig.Dispose();
        }
    }

    // =================================================================
    // 4. Macrophage quiet retirement at the higher limit
    //    (SPRINT_PLAN.md item 4).
    // =================================================================
    private static void RunMacrophageQuietRetirement()
    {
        Debug.Log("[LifecycleVerification] --- Macrophage quiet retirement ---");
        var rig = BuildRig("retire", 1);
        rig.Manager.PlaceTower(0, UnitKind.Macrophage);
        var slot = new CoarseCoord(4, 2);
        var pathogen = SeedPathogen(rig, slot, PathogenClass.LargeBacterium);
        var unit = EmitAt(rig, 0, CenterOf(slot));

        Check("Macrophage tower's kill limit is 20 (Director, 2026-08-21)", unit.KillLimit == 20);
        Check("Macrophage is NOT flagged as degranulating", !unit.DegranulatesOnDepletion);
        Check("Macrophage's limit is four times the neutrophil's", unit.KillLimit == 4 * rig.Neu.KillLimit);

        for (int i = 0; i < 19; i++) unit.RegisterKill();
        Check("Macrophage with 19/20 kills is NOT due to deplete", !unit.IsDepletionDue);
        Check("...and is still alive in its tower's count", rig.Manager.GetActiveChildren(0) == 1);

        unit.RegisterKill();
        float before = pathogen.Health;
        bool depleted = unit.ResolveDepletionIfDue();

        Check("Macrophage retires at 20 kills", depleted);
        Check($"Quiet retirement dealt NO collateral damage (occupant still at {pathogen.Health} HP)", Mathf.Approximately(pathogen.Health, before));
        Check("Occupied slot is untouched by a retirement", !rig.Grid.IsSlotFree(slot));
        Check("Retired macrophage freed its tower's population slot", rig.Manager.GetActiveChildren(0) == 0);
        Check("Retired macrophage returned to its pool (inactive), not destroyed", !unit.gameObject.activeSelf);

        rig.Dispose();
    }

    // =================================================================
    // 5. Kill attribution -- exactly one unit is credited
    //    (SPRINT_PLAN.md item 6).
    // =================================================================
    private static void RunKillAttribution()
    {
        Debug.Log("[LifecycleVerification] --- Kill attribution ---");
        var rig = BuildRig("attribution", 2);
        rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
        rig.Manager.PlaceTower(1, UnitKind.Neutrophil);

        var slot = new CoarseCoord(8, 2);
        var pathogen = SeedPathogen(rig, slot, PathogenClass.IntracellularVirus);
        var center = CenterOf(slot);

        var unitA = EmitAt(rig, 0, center);
        var unitB = EmitAt(rig, 1, center);
        Check("Two units emitted from two different towers", unitA != null && unitB != null && unitA != unitB);

        int hitsToKill = Mathf.CeilToInt(pathogen.MaxHealth / PathogenAgent.ContactDamagePerHit);
        for (int i = 0; i < hitsToKill - 1; i++) pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, unitA);
        Check($"After {hitsToKill - 1} non-lethal hits from A, nobody has been credited (A={unitA.Kills}, B={unitB.Kills})",
            unitA.Kills == 0 && unitB.Kills == 0);

        pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, unitB);
        Check("The unit whose hit crossed zero (B) gets the kill", unitB.Kills == 1);
        Check("The unit that did all the earlier damage (A) gets nothing -- no split credit", unitA.Kills == 0);

        // Same-tick pile-on: further hits on an already-cleared pathogen
        // credit nobody, because ReceiveDamage returns at its state guard.
        pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, unitA);
        pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, unitB);
        Check("Extra same-tick hits on an already-cleared pathogen credit nobody (A=0, B=1)", unitA.Kills == 0 && unitB.Kills == 1);
        Check("Cleared pathogen's slot is free", rig.Grid.IsSlotFree(slot));

        // Null source stays legal (spread/environmental damage + harness use).
        var slot2 = new CoarseCoord(9, 2);
        var pathogen2 = SeedPathogen(rig, slot2, PathogenClass.IntracellularVirus);
        pathogen2.ReceiveDamage(pathogen2.MaxHealth, null);
        Check("ReceiveDamage with a null source still clears the pathogen (no exception, credited to nobody)", rig.Grid.IsSlotFree(slot2));

        rig.Dispose();
    }

    // =================================================================
    // 6. Contact requires fine-tile proximity, not just a shared coarse
    //    slot (SPRINT_PLAN.md item 7).
    // =================================================================
    private static void RunProximityContact()
    {
        Debug.Log("[LifecycleVerification] --- Fine-tile proximity contact ---");
        var rig = BuildRig("proximity", 1);
        rig.Manager.PlaceTower(0, UnitKind.Neutrophil);

        var slot = new CoarseCoord(6, 2);
        var pathogen = SeedPathogen(rig, slot, PathogenClass.LargeBacterium);
        var center = CenterOf(slot);
        int radius = rig.Manager.GetTuning(0).ContactRadiusFineTiles;
        Check($"Contact radius default is 2 fine tiles (got {radius})", radius == 2);

        // On top of the pathogen.
        var unit = EmitAt(rig, 0, center);
        float before = pathogen.Health;
        Check("A unit on the pathogen's own fine tile deals contact damage", unit.CheckContact());
        Check("...and the pathogen actually lost health", pathogen.Health < before);

        // Exactly at the radius edge (Chebyshev 2, diagonally).
        before = pathogen.Health;
        unit.Initialize(rig.Board, rig.Grid, rig.Field, rig.Neu, new FineCoord(center.Column + 2, center.Row + 2),
            rig.Manager.GetTuning(0), 0, null);
        Check("A unit exactly at the contact radius (Chebyshev 2) still connects", unit.CheckContact());
        Check("...and the pathogen lost health", pathogen.Health < before);

        // Far corner of the SAME 7x7 coarse slot -- Chebyshev 3 from the
        // centre. This is the case Sprint 2's coarse-slot test got wrong.
        var slotOrigin = new FineCoord(slot.Column * BoardConfig.FineSubdivision, slot.Row * BoardConfig.FineSubdivision);
        before = pathogen.Health;
        unit.Initialize(rig.Board, rig.Grid, rig.Field, rig.Neu, slotOrigin, rig.Manager.GetTuning(0), 0, null);
        Check($"A unit at the far corner of the same coarse slot {slotOrigin} does NOT connect", !unit.CheckContact());
        Check("...and the pathogen took no damage", Mathf.Approximately(pathogen.Health, before));
        Check("Far-corner unit is genuinely in the pathogen's coarse slot (so this is a proximity test, not an out-of-slot test)",
            slotOrigin.ToCoarse(BoardConfig.FineSubdivision) == slot);

        // Opposite far corner too, for symmetry.
        var oppositeCorner = new FineCoord(slotOrigin.Column + BoardConfig.FineSubdivision - 1, slotOrigin.Row + BoardConfig.FineSubdivision - 1);
        before = pathogen.Health;
        unit.Initialize(rig.Board, rig.Grid, rig.Field, rig.Neu, oppositeCorner, rig.Manager.GetTuning(0), 0, null);
        Check($"A unit at the opposite corner {oppositeCorner} does NOT connect either", !unit.CheckContact());
        Check("...and the pathogen took no damage", Mathf.Approximately(pathogen.Health, before));

        // A neighbouring coarse slot is out of range at the default radius.
        before = pathogen.Health;
        unit.Initialize(rig.Board, rig.Grid, rig.Field, rig.Neu, CenterOf(new CoarseCoord(slot.Column + 1, slot.Row)),
            rig.Manager.GetTuning(0), 0, null);
        Check("A unit in the neighbouring coarse slot's centre does not connect", !unit.CheckContact());
        Check("...and the pathogen took no damage", Mathf.Approximately(pathogen.Health, before));

        rig.Dispose();
    }

    // =================================================================
    // 7. Every lifecycle number is per-tower mutable state, not a const
    //    (SPRINT_PLAN.md item 5) -- i.e. an upgrade system could bump ONE
    //    tower's numbers with a single field write.
    // =================================================================
    private static void RunPerTowerTuningIsMutable()
    {
        Debug.Log("[LifecycleVerification] --- Per-tower tuning is mutable ---");
        var rig = BuildRig("tuning", 2);
        rig.Manager.PlaceTower(0, UnitKind.Neutrophil);
        rig.Manager.PlaceTower(1, UnitKind.Neutrophil);

        Check("Two towers of the same kind get INDEPENDENT tuning instances",
            !ReferenceEquals(rig.Manager.GetTuning(0), rig.Manager.GetTuning(1)));

        // The one-line "upgrade" the design doc asks to be possible.
        rig.Manager.GetTuning(0).MaxActiveChildren = 3;
        rig.Manager.GetTuning(0).KillLimit = 9;

        Check("Upgrading tower 0 does not touch tower 1's cap", rig.Manager.GetTuning(1).MaxActiveChildren == 10);
        Check("Upgrading tower 0 does not touch tower 1's kill limit", rig.Manager.GetTuning(1).KillLimit == 5);
        Check("Upgrading tower 0 does not touch the shared UnitProfile default", rig.Neu.KillLimit == 5);

        for (int i = 0; i < 2000; i++) rig.Manager.Tick(Dt);
        Check($"Upgraded tower 0 caps at its OWN new max of 3 ({rig.Manager.GetActiveChildren(0)} alive)", rig.Manager.GetActiveChildren(0) == 3);
        Check($"Tower 1 still caps at 10 ({rig.Manager.GetActiveChildren(1)} alive)", rig.Manager.GetActiveChildren(1) == 10);
        Check("Tower 0's children carry the upgraded kill limit", rig.Manager.GetChildren(0)[0].KillLimit == 9);
        Check("Tower 1's children carry the un-upgraded kill limit", rig.Manager.GetChildren(1)[0].KillLimit == 5);

        // Live-reference semantics (Director, 2026-08-21): an upgrade takes
        // effect INSTANTLY on every one of that progenitor's current
        // children as well as its future ones -- the point being that
        // spending ATP makes an immediate difference. Sprint 3 originally
        // shipped snapshot semantics (future children only); this is the
        // ruling that replaced it.
        var alreadyFielded = rig.Manager.GetChildren(1)[0];
        rig.Manager.GetTuning(1).KillLimit = 40;
        Check("A mid-life upgrade DOES immediately change an already-fielded unit",
            alreadyFielded.KillLimit == 40);
        Check("...and every other already-fielded child of that tower, not just one",
            AllKillLimitsAre(rig.Manager.GetChildren(1), 40));
        Check("...while a DIFFERENT tower's already-fielded children are untouched",
            AllKillLimitsAre(rig.Manager.GetChildren(0), 9));
        Check("...and the shared UnitProfile default is still untouched", rig.Neu.KillLimit == 5);

        DepleteThroughRealPath(alreadyFielded);
        for (int i = 0; i < 100; i++) rig.Manager.Tick(Dt);
        Check("...and the NEXT unit that tower emits also gets the upgraded limit",
            rig.Manager.LastEmittedUnit.KillLimit == 40);

        rig.Dispose();
    }

    // =================================================================
    // 8. The headline claim: population stays bounded over a long run.
    // =================================================================
    private static void RunLongSimulation()
    {
        Debug.Log("[LifecycleVerification] --- Long run, all towers placed ---");

        // 8a. Five towers, five simulated minutes, nothing ever dies.
        {
            var rig = BuildRig("long_static", 5);
            for (int i = 0; i < 5; i++) rig.Manager.PlaceTower(i, i % 2 == 0 ? UnitKind.Macrophage : UnitKind.Neutrophil);

            int ceiling = 0;
            for (int i = 0; i < 5; i++) ceiling += rig.Manager.GetTuning(i).MaxActiveChildren;

            const float dt = Dt;
            const float duration = 300f; // five simulated minutes
            for (int i = 0; i < duration / dt; i++) rig.Manager.Tick(dt);

            int uncappedWouldBe = (int)(duration / BoneMarrowManager.EmissionIntervalSeconds) * 5;
            Debug.Log($"[LifecycleVerification] After {duration}s x 5 towers: {rig.Manager.TotalActiveUnits} active, {rig.Manager.EmittedCount} ever emitted. " +
                      $"Sprint 2 (no cap, no despawn) would have had {uncappedWouldBe} active by now.");
            Check($"Active unit count is bounded by towers x cap ({rig.Manager.TotalActiveUnits} <= {ceiling})",
                rig.Manager.TotalActiveUnits <= ceiling);
            Check($"Active unit count is far below Sprint 2's unbounded {uncappedWouldBe}", rig.Manager.TotalActiveUnits < uncappedWouldBe);
            rig.Dispose();
        }

        // 8b. Same, but with constant churn -- one live unit depleted every
        //     simulated second, so the towers are continuously refilling.
        //     This is the case that would break a naive implementation
        //     (banked emission timers dumping a burst on every death).
        {
            var rig = BuildRig("long_churn", 5);
            for (int i = 0; i < 5; i++) rig.Manager.PlaceTower(i, i % 2 == 0 ? UnitKind.Macrophage : UnitKind.Neutrophil);

            int ceiling = 0;
            for (int i = 0; i < 5; i++) ceiling += rig.Manager.GetTuning(i).MaxActiveChildren;

            const float dt = Dt;
            const float duration = 300f;
            int steps = (int)(duration / dt);
            int peak = 0;
            bool everExceeded = false;
            var rng = new System.Random(20260821);

            for (int i = 0; i < steps; i++)
            {
                rig.Manager.Tick(dt);
                if (i % 10 == 0) // once per simulated second
                {
                    int slot = rng.Next(0, 5);
                    var kids = rig.Manager.GetChildren(slot);
                    if (kids.Count > 0) DepleteThroughRealPath(kids[rng.Next(0, kids.Count)]);
                }
                int now = rig.Manager.TotalActiveUnits;
                if (now > peak) peak = now;
                if (now > ceiling) everExceeded = true;
            }

            Debug.Log($"[LifecycleVerification] Churn run: peak {peak} active (ceiling {ceiling}), ended at {rig.Manager.TotalActiveUnits} active, {rig.Manager.EmittedCount} ever emitted over {duration}s.");
            Check($"Active count NEVER exceeded the ceiling at any point during 300s of churn (peak {peak} <= {ceiling})", !everExceeded);
            Check($"Ended within the ceiling ({rig.Manager.TotalActiveUnits} <= {ceiling})", rig.Manager.TotalActiveUnits <= ceiling);
            Check($"Towers kept producing throughout the churn ({rig.Manager.EmittedCount} emitted > {ceiling} initial fill)",
                rig.Manager.EmittedCount > ceiling);
            rig.Dispose();
        }
    }

    // =================================================================
    // 9. Diagnostic (no assertions): how much did item 7 actually slow
    //    combat down? SPRINT_PLAN.md asks for the observed change to be
    //    reported rather than silently compensated for elsewhere.
    //    Uses the REAL movement algorithm (Chemotaxis.ChooseNextStep) with
    //    no side effects, so it measures the production walk.
    // =================================================================
    private static void RunContactRateDiagnostic()
    {
        Debug.Log("[LifecycleVerification] --- Contact-rate diagnostic (no assertions) ---");
        var boardGo = new GameObject("LifecycleVerification_DiagBoard");
        var board = boardGo.AddComponent<BoardConfig>();
        var field = new CytokineField(board);

        var slot = new CoarseCoord(board.Columns / 2, 2);
        var target = CenterOf(slot);

        var candidates = new FineCoord[4];
        var weights = new float[4];

        // Purely geometric: what fraction of a 7x7 coarse slot's tiles are
        // within Chebyshev 2 of its centre?
        int inRadius = 0;
        for (int dc = -3; dc <= 3; dc++)
            for (int dr = -3; dr <= 3; dr++)
                if (Mathf.Max(Mathf.Abs(dc), Mathf.Abs(dr)) <= 2) inRadius++;
        Debug.Log($"[LifecycleVerification] Geometric: {inRadius}/49 tiles of a coarse slot are within contact radius 2 of its centre ({100f * inRadius / 49f:F1}%).");

        // Behavioural: walk a real neutrophil (3 fine tiles/tick) from the
        // pathogen's tile for a long time and count ticks that WOULD have
        // dealt damage under each rule.
        foreach (int speed in new[] { 1, 3 })
        {
            int coarseHits = 0, proximityHits = 0;
            const int ticks = 200000;
            var cur = target;
            for (int t = 0; t < ticks; t++)
            {
                for (int s = 0; s < speed; s++)
                    cur = Chemotaxis.ChooseNextStep(cur, board, field, false, candidates, weights);
                bool sameSlot = cur.ToCoarse(BoardConfig.FineSubdivision) == slot;
                if (!sameSlot) continue;
                coarseHits++;
                if (Mathf.Max(Mathf.Abs(cur.Column - target.Column), Mathf.Abs(cur.Row - target.Row)) <= 2) proximityHits++;
            }
            string kind = speed == 1 ? "macrophage" : "neutrophil";
            float ratio = coarseHits == 0 ? 0f : 100f * proximityHits / coarseHits;
            Debug.Log($"[LifecycleVerification] {kind} ({speed} tiles/tick), {ticks} ticks from the pathogen's tile: " +
                      $"Sprint 2 rule would have landed {coarseHits} hits, Sprint 3 proximity rule lands {proximityHits} ({ratio:F1}% of the old rate).");
        }

        Object.DestroyImmediate(boardGo);
    }
}
