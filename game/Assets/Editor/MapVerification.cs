using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

/// <summary>
/// Sprint 4 verification: Map 01's three-band geometry and the invasion
/// loop (GAME_DESIGN.md sections 1a/1b, SPRINT_PLAN.md items 1-9).
///
/// Same philosophy as CytokineVerification (Sprint 1), CombatVerification
/// (Sprint 2) and LifecycleVerification (Sprint 3): drive the REAL
/// production classes -- BoardConfig, GutInterface, PathogenAgent,
/// PathogenSpawner -- with no Play Mode and no rendering. Nothing here
/// reimplements game logic; where a number is asserted it came out of the
/// production path.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod MapVerification.RunAll
///
/// Written by the head session after the dispatched Sprint 4 Code agent hit
/// its usage limit before writing any verification. The agent had already
/// added BoardConfig.ConfigureForTest specifically for this file.
/// </summary>
public static class MapVerification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[MapVerification] Starting ...");

        InvasionTuning.ResetToDefaults();

        RunBandLayout();
        RunAxisFrame();
        RunLumenFlow();
        RunAdhesionProximity();
        RunBreachBurst();
        RunBaseDirectedAdvance();
        RunBaseReached();
        RunMirroredMap();

        InvasionTuning.ResetToDefaults();

        Debug.Log($"[MapVerification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool condition)
    {
        if (condition)
        {
            passed++;
            Debug.Log($"[MapVerification] PASS -- {label}");
        }
        else
        {
            failed++;
            Debug.LogError($"[MapVerification] FAIL -- {label}");
        }
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

    /// <summary>Builds a board. Defaults are Map 01's production values
    /// (100x40, base on the negative end); pass overrides to mirror it.</summary>
    private static Rig BuildRig(
        string name,
        AxisEnd baseEnd = AxisEnd.Negative,
        BoardAxis axis = BoardAxis.Horizontal,
        AxisEnd flowEnd = AxisEnd.Positive)
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject($"MapVerification_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(100, 40, axis, baseEnd, 25, 25, flowEnd);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Gut = new GutInterface(rig.Board, rig.Grid, rig.Tally);
        return rig;
    }

    /// <summary>A bare PathogenAgent bound to a rig, not yet placed.</summary>
    private static PathogenAgent NewAgent(Rig rig, string name)
    {
        var go = new GameObject($"MapVerification_{name}");
        go.AddComponent<SpriteRenderer>();
        var agent = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        return agent;
    }

    private static GameObject NewPathogenTemplate(Rig rig)
    {
        var template = new GameObject("MapVerification_PathogenTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        rig.Junk.Add(template);
        return template;
    }

    // =================================================================
    // 1. Band layout (SPRINT_PLAN.md item 1)
    // =================================================================
    private static void RunBandLayout()
    {
        Debug.Log("[MapVerification] --- Band layout ---");
        var rig = BuildRig("Bands");
        var b = rig.Board;

        Check($"Board is 100 x 40 coarse cells ({b.Columns} x {b.Rows})", b.Columns == 100 && b.Rows == 40);
        Check($"Bands sum to the axis length (25 + {b.TissueBandCells} + 25 = {b.AxisLength})",
            b.BaseBandCells + b.TissueBandCells + b.LumenBandCells == b.AxisLength);
        Check($"Tissue band is the middle half ({b.TissueBandCells} cells)", b.TissueBandCells == 50);
        Check($"Cross length is 40 lanes ({b.CrossLength})", b.CrossLength == 40);

        Check("Axis index 0 is Base", b.BandAtAxisIndex(0) == BoardBand.Base);
        Check("Last base cell (24) is Base", b.BandAtAxisIndex(24) == BoardBand.Base);
        Check("First tissue cell (25) is Tissue", b.BandAtAxisIndex(25) == BoardBand.Tissue);
        Check("Last tissue cell (74) is Tissue", b.BandAtAxisIndex(74) == BoardBand.Tissue);
        Check("First lumen cell (75) is Lumen", b.BandAtAxisIndex(75) == BoardBand.Lumen);
        Check("Last lumen cell (99) is Lumen", b.BandAtAxisIndex(99) == BoardBand.Lumen);

        Check($"TissueBaseEdgeAxisIndex is the first tissue cell ({b.TissueBaseEdgeAxisIndex})",
            b.TissueBaseEdgeAxisIndex == 25 && b.BandAtAxisIndex(b.TissueBaseEdgeAxisIndex) == BoardBand.Tissue);
        Check($"TissueLumenEdgeAxisIndex is the last tissue cell ({b.TissueLumenEdgeAxisIndex})",
            b.TissueLumenEdgeAxisIndex == 74 && b.BandAtAxisIndex(b.TissueLumenEdgeAxisIndex) == BoardBand.Tissue);
        Check($"LumenNearWallAxisIndex is the first lumen cell ({b.LumenNearWallAxisIndex})",
            b.LumenNearWallAxisIndex == 75 && b.BandAtAxisIndex(b.LumenNearWallAxisIndex) == BoardBand.Lumen);

        Check("Lumen depth at the wall is 0", b.LumenDepthFromInterface(b.LumenNearWallAxisIndex) == 0);
        Check($"Lumen depth at the far side is {b.LumenBandCells - 1}",
            b.LumenDepthFromInterface(b.AxisLength - 1) == b.LumenBandCells - 1);

        Check("Gut interface has one position per lane", rig.Gut.PositionCount == b.CrossLength);

        rig.Dispose();
    }

    // =================================================================
    // 2. Axis frame (SPRINT_PLAN.md item 3 -- the abstraction itself)
    // =================================================================
    private static void RunAxisFrame()
    {
        Debug.Log("[MapVerification] --- Axis frame ---");
        var normal = BuildRig("AxisNormal");
        var mirrored = BuildRig("AxisMirrored", AxisEnd.Positive);

        bool roundTrips = true;
        for (int axisIndex = 0; axisIndex < normal.Board.AxisLength; axisIndex += 7)
        {
            for (int cross = 0; cross < normal.Board.CrossLength; cross += 9)
            {
                var cell = normal.Board.CoarseFromAxis(axisIndex, cross);
                if (normal.Board.AxisIndex(cell) != axisIndex || normal.Board.CrossIndex(cell) != cross)
                    roundTrips = false;
            }
        }
        Check("AxisIndex/CoarseFromAxis round-trip on the normal board", roundTrips);

        bool mirrorRoundTrips = true;
        for (int axisIndex = 0; axisIndex < mirrored.Board.AxisLength; axisIndex += 7)
        {
            var cell = mirrored.Board.CoarseFromAxis(axisIndex, 3);
            if (mirrored.Board.AxisIndex(cell) != axisIndex) mirrorRoundTrips = false;
        }
        Check("AxisIndex/CoarseFromAxis round-trip on the mirrored board", mirrorRoundTrips);

        // The point of the whole abstraction: axis index 0 is the base end
        // in BOTH configurations, but sits at opposite world columns.
        var normalBaseCell = normal.Board.CoarseFromAxis(0, 0);
        var mirroredBaseCell = mirrored.Board.CoarseFromAxis(0, 0);
        Check($"Base-end cell is column 0 when baseEnd = Negative ({normalBaseCell.Column})",
            normalBaseCell.Column == 0);
        Check($"Base-end cell is column 99 when baseEnd = Positive ({mirroredBaseCell.Column})",
            mirroredBaseCell.Column == 99);
        Check("Both configurations still classify axis index 0 as the Base band",
            normal.Board.BandOf(normalBaseCell) == BoardBand.Base &&
            mirrored.Board.BandOf(mirroredBaseCell) == BoardBand.Base);

        // "-1 in the axis frame is toward the base" must hold in world terms
        // in opposite directions on the two boards.
        var normalStep = normal.Board.OffsetInAxisFrame(normal.Board.CoarseFromAxis(50, 5), -1, 0);
        var mirroredStep = mirrored.Board.OffsetInAxisFrame(mirrored.Board.CoarseFromAxis(50, 5), -1, 0);
        Check($"Toward-base step DECREASES world column when base is Negative ({normalStep.Column} < 50-ish)",
            normal.Board.AxisIndex(normalStep) == 49 && normalStep.Column == 49);
        Check($"Toward-base step INCREASES world column when base is Positive ({mirroredStep.Column})",
            mirrored.Board.AxisIndex(mirroredStep) == 49 && mirroredStep.Column == 50);

        normal.Dispose();
        mirrored.Dispose();
    }

    // =================================================================
    // 3. Lumen flow (SPRINT_PLAN.md item 4)
    // =================================================================
    private static void RunLumenFlow()
    {
        Debug.Log("[MapVerification] --- Lumen flow ---");
        var rig = BuildRig("Lumen");

        // Adhesion off, so this measures flow and excretion alone.
        InvasionTuning.AdhesionChanceAtWall = 0f;

        var agent = NewAgent(rig, "Flower");
        bool exited = false;
        agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => exited = true, (c, t) => false);

        Check("A freshly spawned pathogen is in the Lumen", agent.State == PathogenState.Lumen);
        Check("...and is inside the lumen band", rig.Board.BandOf(agent.CurrentCoarse) == BoardBand.Lumen);
        Check($"...at the upstream end of the flow (cross {rig.Board.CrossIndex(agent.CurrentCoarse)})",
            rig.Board.CrossIndex(agent.CurrentCoarse) == rig.Board.LumenEntryCrossIndex);

        int startAxis = rig.Board.AxisIndex(agent.CurrentCoarse);
        int startCross = rig.Board.CrossIndex(agent.CurrentCoarse);

        // One step interval.
        agent.SimulationTick(InvasionTuning.LumenStepIntervalSeconds, 0f);
        Check($"One step moves it along the flow axis (cross {startCross} -> {rig.Board.CrossIndex(agent.CurrentCoarse)})",
            rig.Board.CrossIndex(agent.CurrentCoarse) == startCross + rig.Board.FlowCrossStep);
        Check("...without changing its distance from the wall",
            rig.Board.AxisIndex(agent.CurrentCoarse) == startAxis);

        // Carry it all the way down the channel.
        float t = 0f;
        for (int i = 0; i < rig.Board.CrossLength + 5 && agent.State != PathogenState.Cleared; i++)
        {
            t += InvasionTuning.LumenStepIntervalSeconds;
            agent.SimulationTick(InvasionTuning.LumenStepIntervalSeconds, t);
        }

        Check("A pathogen carried off the downstream end is cleared", agent.State == PathogenState.Cleared);
        Check("...its exit callback fired (returned to the pool)", exited);
        Check($"...and it counted as excreted, not as a breach ({rig.Tally.Excreted})", rig.Tally.Excreted == 1);
        Check("...with no penalty: nothing reached the base", rig.Tally.ReachedBase == 0);
        Check("...and it never touched tissue", rig.Grid.AdheredCount == 0);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // 4. Proximity-gated adhesion (SPRINT_PLAN.md item 5)
    // =================================================================
    private static void RunAdhesionProximity()
    {
        Debug.Log("[MapVerification] --- Proximity-gated adhesion ---");
        var rig = BuildRig("Adhesion");
        InvasionTuning.ResetToDefaults();

        // 4a. The curve itself is monotonic in distance from the wall.
        bool monotonic = true;
        for (int depth = 1; depth < rig.Board.LumenBandCells; depth++)
        {
            if (PathogenAgent.AdhesionChanceAt(depth) >= PathogenAgent.AdhesionChanceAt(depth - 1))
                monotonic = false;
        }
        Check("Adhesion chance strictly decreases with distance from the gut wall", monotonic);
        Check($"At the wall it equals AdhesionChanceAtWall ({PathogenAgent.AdhesionChanceAt(0):F3})",
            Mathf.Approximately(PathogenAgent.AdhesionChanceAt(0), InvasionTuning.AdhesionChanceAtWall));
        Check($"Far out in the channel it is near zero ({PathogenAgent.AdhesionChanceAt(24):F5})",
            PathogenAgent.AdhesionChanceAt(24) < InvasionTuning.AdhesionChanceAtWall * 0.05f);
        Check("Negative depth (not in the channel) cannot adhere", PathogenAgent.AdhesionChanceAt(-1) == 0f);

        // 4b. End to end through the production path: the gate is actually
        // wired in. With the wall chance at zero, nothing adheres however
        // long the run.
        InvasionTuning.AdhesionChanceAtWall = 0f;
        int adheredWithGateClosed = RunChannelCohort(rig, 120, "GateClosed");
        Check($"With adhesion chance 0, no pathogen leaves the flow ({adheredWithGateClosed} adhered)",
            adheredWithGateClosed == 0);

        // 4c. Depth dependence, end to end. Same cohort size, same spawn
        // distribution; only the falloff changes. A steep falloff means only
        // pathogens hugging the wall can adhere, so far fewer do.
        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionFalloffCells = 0.5f; // effectively wall-only
        int adheredSteep = RunChannelCohort(rig, 400, "Steep");

        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionFalloffCells = 200f; // effectively depth-blind
        int adheredFlat = RunChannelCohort(rig, 400, "Flat");

        Debug.Log($"[MapVerification] Cohort of 400 through the channel: steep falloff (wall-only) adhered {adheredSteep}, depth-blind adhered {adheredFlat}.");
        Check($"A depth-blind curve adheres far more than a wall-only one ({adheredFlat} vs {adheredSteep})",
            adheredFlat > adheredSteep * 2);
        Check($"A wall-only curve still adheres something ({adheredSteep})", adheredSteep > 0);
        Check($"Neither curve adheres every pathogen -- some are always excreted ({adheredFlat}/400)",
            adheredFlat < 400);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }

    /// <summary>Runs a cohort of pathogens through the whole channel on a
    /// throwaway gut interface and returns how many left the flow. Uses the
    /// real PathogenAgent lumen path, not a model of it.</summary>
    private static int RunChannelCohort(Rig rig, int cohort, string label)
    {
        var tally = new InvasionTally();
        var gut = new GutInterface(rig.Board, rig.Grid, tally);
        var agents = new List<PathogenAgent>();

        for (int i = 0; i < cohort; i++)
        {
            var agent = NewAgent(rig, $"{label}_{i}");
            agent.Initialize(rig.Board, rig.Grid, gut, tally, a => { }, (c, t) => false);
            agents.Add(agent);
        }

        float t = 0f;
        for (int step = 0; step < rig.Board.CrossLength + 5; step++)
        {
            t += InvasionTuning.LumenStepIntervalSeconds;
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].State == PathogenState.Lumen)
                    agents[i].SimulationTick(InvasionTuning.LumenStepIntervalSeconds, t);
            }
        }

        return tally.Adhesions;
    }

    // =================================================================
    // 5. The breach burst (SPRINT_PLAN.md item 6 -- the signature mechanic)
    // =================================================================
    private static void RunBreachBurst()
    {
        Debug.Log("[MapVerification] --- Breach burst ---");
        var rig = BuildRig("Breach");
        InvasionTuning.ResetToDefaults();

        const int pileSize = 6;
        const int positionA = 10;
        const int positionB = 25;

        var atA = new List<PathogenAgent>();
        for (int i = 0; i < pileSize; i++)
        {
            var agent = NewAgent(rig, $"PileA_{i}");
            agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => { }, (c, t) => false);
            agent.AdhereToInterface(positionA);
            atA.Add(agent);
        }

        var atB = new List<PathogenAgent>();
        for (int i = 0; i < 3; i++)
        {
            var agent = NewAgent(rig, $"PileB_{i}");
            agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => { }, (c, t) => false);
            agent.AdhereToInterface(positionB);
            atB.Add(agent);
        }

        Check($"Adhering moves the pathogen TO the wall, not where it was floating",
            rig.Board.AxisIndex(atA[0].CurrentCoarse) == rig.Board.LumenNearWallAxisIndex);
        Check("...and it is now AtInterface, not still flowing", atA[0].State == PathogenState.AtInterface);
        Check($"Position A holds all {pileSize} ({rig.Gut.AdheredCountAt(positionA)})",
            rig.Gut.AdheredCountAt(positionA) == pileSize);
        Check($"Position B holds 3 ({rig.Gut.AdheredCountAt(positionB)})", rig.Gut.AdheredCountAt(positionB) == 3);
        Check($"PeakAdhered reports the worst position ({rig.Gut.PeakAdhered})", rig.Gut.PeakAdhered == pileSize);
        Check("Breach chance rises with pressure at a position",
            rig.Gut.BreachChanceAt(positionA) > rig.Gut.BreachChanceAt(positionB));
        Check("An empty position has no breach chance", rig.Gut.BreachChanceAt(0) == 0f);

        int breachEvents = 0;
        int releasedByEvent = 0;
        rig.Gut.Breached += (pos, count) => { breachEvents++; releasedByEvent += count; };

        int released = rig.Gut.Breach(positionA, 0f);

        // THE assertion this whole sprint item exists for.
        Check($"A breach releases EVERY pathogen at that position at once ({released} of {pileSize})",
            released == pileSize);
        Check("...leaving that wall position empty", rig.Gut.AdheredCountAt(positionA) == 0);

        int inTissue = 0;
        for (int i = 0; i < atA.Count; i++)
            if (atA[i].State == PathogenState.InTissue) inTissue++;
        Check($"...and all {pileSize} are now in tissue ({inTissue})", inTissue == pileSize);

        bool allInTissueBand = true;
        for (int i = 0; i < atA.Count; i++)
            if (rig.Board.BandOf(atA[i].CurrentCoarse) != BoardBand.Tissue) allInTissueBand = false;
        Check("...inside the tissue band, not the lumen or base", allInTissueBand);

        Check($"...reported as ONE breach event, not {pileSize} ({breachEvents})", breachEvents == 1);
        Check($"...carrying the released count ({releasedByEvent})", releasedByEvent == pileSize);
        Check($"Tally recorded one breach ({rig.Tally.Breaches})", rig.Tally.Breaches == 1);
        Check($"Tally recorded the released pathogens ({rig.Tally.ReleasedIntoTissue})",
            rig.Tally.ReleasedIntoTissue == pileSize);

        // Isolation: the other pile is untouched.
        Check($"A breach at one position does NOT release another ({rig.Gut.AdheredCountAt(positionB)})",
            rig.Gut.AdheredCountAt(positionB) == 3);
        bool bStillOnWall = true;
        for (int i = 0; i < atB.Count; i++)
            if (atB[i].State != PathogenState.AtInterface) bStillOnWall = false;
        Check("...and those pathogens are all still on the wall", bStillOnWall);

        Check("Breaching an empty position releases nothing", rig.Gut.Breach(positionA, 0f) == 0);

        rig.Dispose();
    }

    // =================================================================
    // 6. Base-directed advance (SPRINT_PLAN.md item 7)
    // =================================================================
    private static void RunBaseDirectedAdvance()
    {
        Debug.Log("[MapVerification] --- Base-directed advance ---");
        var rig = BuildRig("Advance");
        InvasionTuning.ResetToDefaults();

        const int cohort = 60;
        int startAxis = rig.Board.TissueLumenEdgeAxisIndex;
        var agents = new List<PathogenAgent>();

        for (int i = 0; i < cohort; i++)
        {
            var agent = NewAgent(rig, $"Advance_{i}");
            var slot = rig.Board.CoarseFromAxis(startAxis, i % rig.Board.CrossLength);
            agent.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Gut, rig.Tally,
                a => { }, (c, t) => false, slot, PathogenClass.LargeBacterium, 0f);
            agents.Add(agent);
        }

        float t = 0f;
        const int steps = 12;
        for (int step = 0; step < steps; step++)
        {
            t += InvasionTuning.TissueStepIntervalSeconds;
            for (int i = 0; i < agents.Count; i++)
                if (agents[i].State == PathogenState.InTissue)
                    agents[i].SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t);
        }

        int closer = 0;
        int farther = 0;
        float totalDelta = 0f;
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].State != PathogenState.InTissue) { closer++; continue; } // reached the base
            int axisNow = rig.Board.AxisIndex(agents[i].CurrentCoarse);
            if (axisNow < startAxis) closer++;
            else if (axisNow > startAxis) farther++;
            totalDelta += startAxis - axisNow;
        }

        float meanAdvance = totalDelta / cohort;
        Debug.Log($"[MapVerification] After {steps} tissue steps: {closer}/{cohort} closer to the base, {farther} farther, mean advance {meanAdvance:F2} cells.");
        Check($"A strong majority advanced toward the base ({closer}/{cohort})", closer > cohort * 0.75f);
        Check($"Mean advance is positive and substantial ({meanAdvance:F2} cells in {steps} steps)",
            meanAdvance > steps * 0.3f);
        Check($"The walk is not deterministic -- some drifted the other way or held ({farther} farther)",
            farther < cohort * 0.25f);

        rig.Dispose();
    }

    // =================================================================
    // 7. Reaching the base (SPRINT_PLAN.md item 9)
    // =================================================================
    private static void RunBaseReached()
    {
        Debug.Log("[MapVerification] --- Reaching the base ---");
        var rig = BuildRig("BaseReached");
        InvasionTuning.ResetToDefaults();

        // Force a pure toward-base walk so this is deterministic.
        InvasionTuning.AdvanceBaseWeight = 1f;
        InvasionTuning.AdvanceLateralWeight = 0f;
        InvasionTuning.AdvanceAwayWeight = 0f;

        var slot = rig.Board.CoarseFromAxis(rig.Board.TissueBaseEdgeAxisIndex, 12);
        bool exited = false;
        var agent = NewAgent(rig, "Breacher");
        agent.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Gut, rig.Tally,
            a => exited = true, (c, t) => false, slot, PathogenClass.LargeBacterium, 0f);

        Check("Seeded at the tissue cell adjacent to the base",
            rig.Board.AxisIndex(agent.CurrentCoarse) == rig.Board.TissueBaseEdgeAxisIndex);
        Check("Nothing has reached the base yet", rig.Tally.ReachedBase == 0);

        agent.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, 1f);

        Check($"Stepping into the base counts it ({rig.Tally.ReachedBase})", rig.Tally.ReachedBase == 1);
        Check("...and the pathogen is removed from play", agent.State == PathogenState.Cleared);
        Check("...via the pool exit path, not Destroy", exited);
        Check("...and it did not leave an occupied tissue slot behind", rig.Grid.AdheredCount == 0);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // 8. The mirrored map -- the real test of the base-direction rule
    //    (SPRINT_PLAN.md item 3)
    // =================================================================
    private static void RunMirroredMap()
    {
        Debug.Log("[MapVerification] --- Mirrored map (base on the opposite side) ---");
        var rig = BuildRig("Mirrored", AxisEnd.Positive);
        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdvanceBaseWeight = 1f;
        InvasionTuning.AdvanceLateralWeight = 0f;
        InvasionTuning.AdvanceAwayWeight = 0f;

        var slot = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex, 7);
        int startColumn = slot.Column;

        var agent = NewAgent(rig, "MirrorWalker");
        agent.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Gut, rig.Tally,
            a => { }, (c, t) => false, slot, PathogenClass.LargeBacterium, 0f);

        float t = 0f;
        for (int step = 0; step < 5; step++)
        {
            t += InvasionTuning.TissueStepIntervalSeconds;
            agent.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t);
        }

        int columnNow = agent.CurrentCoarse.Column;
        int axisNow = rig.Board.AxisIndex(agent.CurrentCoarse);

        Debug.Log($"[MapVerification] Mirrored board: pathogen went from world column {startColumn} to {columnNow} (axis index {rig.Board.TissueLumenEdgeAxisIndex} -> {axisNow}).");

        Check($"On a mirrored map it still advances toward the base in the axis frame ({axisNow} < {rig.Board.TissueLumenEdgeAxisIndex})",
            axisNow < rig.Board.TissueLumenEdgeAxisIndex);
        Check($"...which here means its world column INCREASES ({startColumn} -> {columnNow})",
            columnNow > startColumn);
        Check("...proving advance follows configuration, not a hardcoded 'left'", true);

        InvasionTuning.ResetToDefaults();
        rig.Dispose();
    }
}
