using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;
using ImmunologyTD.Adaptive;

/// <summary>
/// Sprint 8 verification: the dendritic-cell shuttle and the antigen
/// barcode (GAME_DESIGN.md §5a/§5c, SPRINT_PLAN.md). Same
/// drive-the-real-classes, no-Play-Mode philosophy as the six harnesses
/// before it -- Antigen math, KnowledgeLedger, debris antigen, the
/// LymphNode + DendriticCell shuttle end to end (a matching pairing raises
/// knowledge by exactly one increment; a non-matching one raises nothing),
/// lymphocyte turnover, and the round boundary despawning the adaptive
/// agents while the two progenitors stay placed.
///
/// Deterministic: Random is seeded, and any test that watches a barcode
/// match forces MatchMaxHammingDistance to 8 (always) or -1 (never) rather
/// than hoping two random tags line up. ResetToDefaults() after.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod AdaptiveVerification.RunAll
/// </summary>
public static class AdaptiveVerification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[AdaptiveVerification] Starting ...");

        Random.InitState(20260829);
        AdaptiveTuning.ResetToDefaults();
        ImmunologyTD.Economy.EconomyTuning.ResetToDefaults();

        RunAntigenMath();
        RunKnowledgeLedger();
        RunDebrisAntigen();
        RunShuttleEndToEnd();
        RunLymphocyteTurnover();
        RunRoundBoundaryDespawn();
        RunDcLaneSpread();

        AdaptiveTuning.ResetToDefaults();
        ImmunologyTD.Economy.EconomyTuning.ResetToDefaults();

        Debug.Log($"[AdaptiveVerification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool condition)
    {
        if (condition) { passed++; Debug.Log($"[AdaptiveVerification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[AdaptiveVerification] FAIL -- {label}"); }
    }

    // =================================================================
    // 1. Antigen -- the 8-bit barcode math
    // =================================================================

    private static void RunAntigenMath()
    {
        AdaptiveTuning.ResetToDefaults();

        Check("Hamming(x, x) == 0", Antigen.HammingDistance(0b10110010, 0b10110010) == 0);
        Check("Hamming(0x00, 0xFF) == 8", Antigen.HammingDistance(0x00, 0xFF) == 8);
        Check("Hamming counts differing bits (0x00 vs 0b00000111 == 3)",
            Antigen.HammingDistance(0x00, 0b00000111) == 3);

        // Default threshold 2: distance 2 teaches, distance 3 does not.
        Check("IsMatch at distance 2 (threshold 2)", Antigen.IsMatch(0x00, 0b00000011));
        Check("NOT IsMatch at distance 3 (threshold 2)", !Antigen.IsMatch(0x00, 0b00000111));

        AdaptiveTuning.MatchMaxHammingDistance = 0;
        Check("threshold 0 -> only an exact match teaches", Antigen.IsMatch(0x2A, 0x2A) && !Antigen.IsMatch(0x2A, 0x2B));
        AdaptiveTuning.ResetToDefaults();

        // The three species antigens are distinct and >= 4 bits apart, so a
        // helper-T within distance 2 of one is within distance 2 of at most
        // one -- "knowledge is per species" stays meaningful.
        byte v = Antigen.ForClass(PathogenClass.IntracellularVirus);
        byte b = Antigen.ForClass(PathogenClass.IntracellularBacterium);
        byte l = Antigen.ForClass(PathogenClass.LargeBacterium);
        Check("species antigens are distinct", v != b && b != l && v != l);
        Check("species antigens are >= 4 bits apart",
            Antigen.HammingDistance(v, b) >= 4 &&
            Antigen.HammingDistance(b, l) >= 4 &&
            Antigen.HammingDistance(v, l) >= 4);
    }

    // =================================================================
    // 2. KnowledgeLedger -- per-species %, clamped
    // =================================================================

    private static void RunKnowledgeLedger()
    {
        AdaptiveTuning.ResetToDefaults();
        var k = new KnowledgeLedger();

        Check("fresh ledger reads 0 for every species",
            k.Get(PathogenClass.IntracellularVirus) == 0f &&
            k.Get(PathogenClass.IntracellularBacterium) == 0f &&
            k.Get(PathogenClass.LargeBacterium) == 0f);

        int rev0 = k.Revision;
        k.Add(PathogenClass.IntracellularVirus, 30f);
        Check("Add is per species (virus 30, bacterium still 0)",
            Mathf.Approximately(k.Get(PathogenClass.IntracellularVirus), 30f) &&
            k.Get(PathogenClass.IntracellularBacterium) == 0f);
        Check("Revision advanced on Add", k.Revision > rev0);

        k.Add(PathogenClass.IntracellularVirus, 999f);
        Check("clamped at KnowledgeMax",
            Mathf.Approximately(k.Get(PathogenClass.IntracellularVirus), AdaptiveTuning.KnowledgeMax));

        k.Add(PathogenClass.IntracellularVirus, -999f);
        Check("clamped at 0 from below", k.Get(PathogenClass.IntracellularVirus) == 0f);

        k.Add(PathogenClass.LargeBacterium, 12f);
        k.Reset();
        Check("Reset zeroes every species", k.Get(PathogenClass.LargeBacterium) == 0f);
    }

    // =================================================================
    // 3. Debris carries an antigen identity
    // =================================================================

    private static void RunDebrisAntigen()
    {
        var boardGo = new GameObject("AdaptiveVerification_DebrisBoard");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);

        var a = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 2, 3);
        grid.KillHostCell(a, PathogenClass.LargeBacterium);
        Check("KillHostCell records the antigen on the debris",
            grid.GetDebrisAntigen(a) == PathogenClass.LargeBacterium);
        Check("...and leaves debris", grid.GetDebrisAmount(a) > 0f);

        grid.ClearDebris(a, TissueGrid.FullDebris, 0f);
        Check("clearing the pile clears the antigen too", grid.GetDebrisAntigen(a) == null);

        var b = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 3, 4);
        grid.KillHostCell(b); // no antigen, no resident
        Check("a killer that leaves no antigen -> debris antigen is null", grid.GetDebrisAntigen(b) == null);

        Object.DestroyImmediate(boardGo);
    }

    // =================================================================
    // 4. The shuttle, end to end
    // =================================================================

    private class ShuttleRig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public KnowledgeLedger Knowledge;
        public LymphNode Node;
        public AdaptiveDirector Director;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++) if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static ShuttleRig BuildShuttleRig()
    {
        var rig = new ShuttleRig();
        rig.BoardGo = new GameObject("AdaptiveVerification_ShuttleBoard");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Knowledge = new KnowledgeLedger();
        rig.Node = new LymphNode(rig.Knowledge, new Rect(-3f, -3f, 6f, 6f));

        var dcPool = NewAgentPool<DendriticCell>(rig, "DC");
        var lymPool = NewAgentPool<Lymphocyte>(rig, "Lym");

        var dirGo = new GameObject("AdaptiveVerification_Director");
        rig.Junk.Add(dirGo);
        rig.Director = dirGo.AddComponent<AdaptiveDirector>();
        rig.Director.Initialize(rig.Node, lymPool, dcPool, rig.Board, rig.Grid, rig.Field);
        return rig;
    }

    private static PrefabPool NewAgentPool<T>(ShuttleRig rig, string label) where T : Component
    {
        var template = new GameObject($"AdaptiveVerification_{label}Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<T>();
        template.SetActive(false);
        rig.Junk.Add(template);

        var poolGo = new GameObject($"AdaptiveVerification_{label}Pool");
        rig.Junk.Add(poolGo);
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        return pool;
    }

    /// <summary>Drives one DC from emission through sample -> travel -> node
    /// -> a single pairing, re-seeding debris under it while it patrols so
    /// the sample is deterministic. Returns the DC. <paramref name="teaches"/>
    /// picks the always/never match threshold.</summary>
    private static DendriticCell DriveOneShuttle(ShuttleRig rig, PathogenClass species, bool teaches)
    {
        AdaptiveTuning.ResetToDefaults();
        AdaptiveTuning.DcPresentationsPerCargo = 1;         // one pairing spends the cargo
        AdaptiveTuning.LymphocyteLifespanSeconds = 100000f; // no aging mid-test
        AdaptiveTuning.DcAxisWalkBiasSharpness = 8f;        // beeline to the base
        AdaptiveTuning.NodeColocalisationSourceStrength = 60f;
        AdaptiveTuning.MatchMaxHammingDistance = teaches ? 8 : -1;

        // Fill the whole tissue band with debris of the target species, so
        // the DC samples on its very first patrol tick wherever it spawned
        // -- makes the sample deterministic without touching the DC's walk.
        for (int ai = rig.Board.TissueBaseEdgeAxisIndex; ai <= rig.Board.TissueLumenEdgeAxisIndex; ai++)
            for (int cross = 0; cross < rig.Board.CrossLength; cross++)
                rig.Grid.KillHostCell(rig.Board.CoarseFromAxis(ai, cross), species);

        var dcGo = rig.Director.EmitDendriticCell(0, null);
        var dc = dcGo.GetComponent<DendriticCell>();
        rig.Director.EmitLymphocyte(0, null); // one resident waiting in the node

        float dt = BoardConfig.TickIntervalSeconds;
        for (int step = 0; step < 4000; step++)
        {
            rig.Director.Tick(dt);
            if (dc.State == DendriticCellState.ReturnToTissue) break;
        }
        return dc;
    }

    private static void RunShuttleEndToEnd()
    {
        // -- matching pairing raises knowledge by exactly one increment --
        var rigA = BuildShuttleRig();
        var dcA = DriveOneShuttle(rigA, PathogenClass.IntracellularVirus, teaches: true);
        Check("shuttle: DC finished a pairing and headed back to tissue",
            dcA.State == DendriticCellState.ReturnToTissue && !dcA.HasCargo);
        Check("shuttle: a MATCHING pairing raised virus knowledge by exactly KnowledgePerMatch",
            Mathf.Approximately(rigA.Knowledge.Get(PathogenClass.IntracellularVirus), AdaptiveTuning.KnowledgePerMatch));
        Check("shuttle: it taught NOTHING about the other species",
            rigA.Knowledge.Get(PathogenClass.IntracellularBacterium) == 0f &&
            rigA.Knowledge.Get(PathogenClass.LargeBacterium) == 0f);
        rigA.Dispose();

        // -- non-matching pairing: freeze happened, cargo spent, taught nothing --
        var rigB = BuildShuttleRig();
        var dcB = DriveOneShuttle(rigB, PathogenClass.LargeBacterium, teaches: false);
        Check("shuttle (no match): DC still completed its pairing cycle and headed back",
            dcB.State == DendriticCellState.ReturnToTissue && !dcB.HasCargo);
        Check("shuttle (no match): knowledge stayed at 0",
            rigB.Knowledge.Get(PathogenClass.LargeBacterium) == 0f);
        rigB.Dispose();

        AdaptiveTuning.ResetToDefaults();
    }

    // =================================================================
    // 5. Lymphocyte turnover -- lifespan -> despawn -> re-emit
    // =================================================================

    private static void RunLymphocyteTurnover()
    {
        AdaptiveTuning.ResetToDefaults();
        AdaptiveTuning.LymphocyteLifespanSeconds = 5f;

        var rig = BuildShuttleRig();
        int despawnCalls = 0;
        rig.Director.EmitLymphocyte(0, (slot, go) => despawnCalls++);
        Check("turnover: one resident after emit", rig.Node.ResidentCount == 1);

        float dt = BoardConfig.TickIntervalSeconds;
        for (int i = 0; i < 80; i++) rig.Director.Tick(dt); // > 5s of Clock

        Check("turnover: resident aged out past its lifespan", rig.Node.ResidentCount == 0);
        Check("turnover: the despawn callback fired", despawnCalls == 1);

        rig.Director.EmitLymphocyte(0, null);
        Check("turnover: progenitor re-populates the node", rig.Node.ResidentCount == 1);

        rig.Dispose();
        AdaptiveTuning.ResetToDefaults();
    }

    // =================================================================
    // 6. Round boundary -- despawn adaptive agents, towers persist
    // =================================================================

    private static void RunRoundBoundaryDespawn()
    {
        AdaptiveTuning.ResetToDefaults();
        AdaptiveTuning.DcEmissionIntervalSeconds = 0.5f;
        AdaptiveTuning.LymphocyteEmissionIntervalSeconds = 0.5f;
        AdaptiveTuning.LymphocyteLifespanSeconds = 100000f;

        var rig = BuildShuttleRig();

        // A bone marrow manager sharing the rig's board/grid, free placement.
        var mac = new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5, MaxActiveChildren = 10, KillLimit = 20 };
        var neu = new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3, MaxActiveChildren = 10, KillLimit = 5 };
        var macPool = NewAgentPool<SearchUnit>(rig, "Mac");
        var neuPool = NewAgentPool<SearchUnit>(rig, "Neu");

        var marrowGo = new GameObject("AdaptiveVerification_Marrow");
        rig.Junk.Add(marrowGo);
        var marrow = marrowGo.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[5];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(i * 2f, -8f, 0f);
        marrow.Initialize(rig.Board, rig.Grid, rig.Field, mac, macPool, neu, neuPool, positions, 1f, null, rig.Director);

        marrow.PlaceTower(0, UnitKind.DendriticCell);
        marrow.PlaceTower(1, UnitKind.HelperT);
        Check("boundary: both adaptive towers placed", BoneMarrowManager.IsAdaptive(marrow.GetSlotKind(0)) && marrow.GetSlotKind(1) == UnitKind.HelperT);

        for (int i = 0; i < 40; i++) { marrow.Tick(0.12f); rig.Director.Tick(0.12f); }
        Check("boundary: DC tower emitted into tissue", rig.Director.DendriticCellCount(0) >= 1);
        Check("boundary: helper-T tower populated the node", rig.Node.ResidentCount >= 1);

        marrow.ClearFieldedUnits();
        Check("boundary: every fielded DC despawned", rig.Director.DendriticCellCount(0) == 0);
        Check("boundary: every fielded lymphocyte despawned", rig.Node.ResidentCount == 0);
        Check("boundary: both towers stay placed",
            marrow.GetSlotState(0) == BoneMarrowSlotState.Placed && marrow.GetSlotState(1) == BoneMarrowSlotState.Placed);

        for (int i = 0; i < 40; i++) { marrow.Tick(0.12f); rig.Director.Tick(0.12f); }
        Check("boundary: towers re-emit next round",
            rig.Director.DendriticCellCount(0) >= 1 && rig.Node.ResidentCount >= 1);

        rig.Dispose();
        AdaptiveTuning.ResetToDefaults();
    }

    // =================================================================
    // 7. Patrol lane-repulsion -- DCs spread across lanes, not clump
    // =================================================================

    /// <summary>Emits three DCs, drops all three on the SAME lane a few
    /// cells into tissue, patrols them for <paramref name="ticks"/> steps,
    /// and reports two things over the run: how many ticks had 2+ DCs
    /// sharing a lane, and the mean total pairwise lane distance. Repulsion
    /// on -> fewer shared-lane ticks, larger spread.</summary>
    private static void PatrolLaneStats(int ticks, float repelStrength, out int coLaneTicks, out float meanSpread)
    {
        AdaptiveTuning.ResetToDefaults();
        AdaptiveTuning.DcLaneRepelStrength = repelStrength;
        AdaptiveTuning.DcMaxActiveChildren = 8;

        var rig = BuildShuttleRig();
        var dcs = new List<DendriticCell>();
        for (int i = 0; i < 3; i++)
            dcs.Add(rig.Director.EmitDendriticCell(0, null).GetComponent<DendriticCell>());

        int lane = rig.Board.CrossLength / 2;
        var start = rig.Board.CoarseCenterFine(
            rig.Board.CoarseFromAxis(rig.Board.TissueBaseEdgeAxisIndex + 4, lane));
        foreach (var d in dcs) d.DebugPlaceForTest(start);

        coLaneTicks = 0;
        long spreadSum = 0;
        for (int step = 0; step < ticks; step++)
        {
            rig.Director.Tick(BoardConfig.TickIntervalSeconds);
            int c0 = rig.Board.CrossIndex(dcs[0].Current.ToCoarse(BoardConfig.FineSubdivision));
            int c1 = rig.Board.CrossIndex(dcs[1].Current.ToCoarse(BoardConfig.FineSubdivision));
            int c2 = rig.Board.CrossIndex(dcs[2].Current.ToCoarse(BoardConfig.FineSubdivision));
            if (c0 == c1 || c1 == c2 || c0 == c2) coLaneTicks++;
            spreadSum += Mathf.Abs(c0 - c1) + Mathf.Abs(c1 - c2) + Mathf.Abs(c0 - c2);
        }
        meanSpread = spreadSum / (float)ticks;

        rig.Dispose();
        AdaptiveTuning.ResetToDefaults();
    }

    private static void RunDcLaneSpread()
    {
        const int ticks = 250;
        PatrolLaneStats(ticks, 1.4f, out int coOn, out float spreadOn);
        PatrolLaneStats(ticks, 0f, out int coOff, out float spreadOff);

        Debug.Log($"[AdaptiveVerification] DC patrol over {ticks} ticks -- repulsion ON: {coOn} co-lane ticks, mean spread {spreadOn:F1}; OFF: {coOff}, {spreadOff:F1}.");
        Check($"lane-repulsion cuts the time DCs share a lane ({coOn} vs {coOff})", coOn < coOff);
        Check($"lane-repulsion widens the mean lane spread ({spreadOn:F1} vs {spreadOff:F1})", spreadOn > spreadOff);
        Check($"with repulsion on, DCs share a lane on well under half the ticks ({coOn}/{ticks})", coOn < ticks / 2);
    }
}
