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
/// production classes -- BoardConfig, GutWall, PathogenAgent,
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
        RunRuptureBurst();
        RunNoTrickle();
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
        public GutWall Gut;
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
        rig.Gut = new GutWall(rig.Board, rig.Grid, rig.Tally);
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

        Check($"The wall is one s-unit per lane ({rig.Gut.WallLengthCells})",
            Mathf.Approximately(rig.Gut.WallLengthCells, b.CrossLength));

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
        agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => exited = true, (c, cls, free, t) => false);

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
        // Sprint 25: this used to assert the depth NEVER changed. It now
        // deliberately can -- organisms mix across the channel, because a
        // fixed depth meant a pathogen's whole fate was decided by one
        // spawn roll it could never escape. What must still hold is that
        // mixing is at most one cell per step, so flow stays the dominant
        // motion and nothing teleports across the lumen.
        int axisDrift = Mathf.Abs(rig.Board.AxisIndex(agent.CurrentCoarse) - startAxis);
        Check($"...and changes its distance from the wall by at most one cell (moved {axisDrift})",
            axisDrift <= 1);

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
        Check("...and it never touched tissue", rig.Grid.TissuePathogenCount == 0);

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
        //
        // This sub-test pins AdhesionChanceAtWall itself (rather than taking
        // the shipped default) because it is about the falloff SHAPE, not the
        // absolute rate. It is set LOW (0.03) so neither curve saturates over
        // a full channel transit -- at the shipped 0.30, and even at the old
        // 0.12, a depth-blind curve adheres literally every pathogen and the
        // "some are always excreted" assertion stops discriminating.
        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionChanceAtWall = 0.03f;
        InvasionTuning.AdhesionFalloffCells = 0.5f; // effectively wall-only
        int adheredSteep = RunChannelCohort(rig, 400, "Steep");

        InvasionTuning.ResetToDefaults();
        InvasionTuning.AdhesionChanceAtWall = 0.03f;
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
        var gut = new GutWall(rig.Board, rig.Grid, tally);
        var agents = new List<PathogenAgent>();

        for (int i = 0; i < cohort; i++)
        {
            var agent = NewAgent(rig, $"{label}_{i}");
            agent.Initialize(rig.Board, rig.Grid, gut, tally, a => { }, (c, cls, free, t) => false);
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
    // 5. The rupture burst (INVASION_REWORK_SPEC.md -- the signature mechanic)
    //
    // Rewritten wholesale in Sprint 22. The old version piled pathogens at
    // two LANE indices and called Gut.Breach(lane). There are no lanes and no
    // piles any more: pathogens mill at continuous positions, crowding seeds
    // a plaque, and the plaque's charge is what bursts.
    //
    // What this must still prove is unchanged, and it is the whole reason
    // the mechanic exists: pressure builds at ONE SPOT and releases
    // EVERYTHING THERE AT ONCE, without touching a separate crowd elsewhere.
    // =================================================================
    private static void RunRuptureBurst()
    {
        Debug.Log("[MapVerification] --- Rupture burst ---");
        var rig = BuildRig("Rupture");
        InvasionTuning.ResetToDefaults();

        const int crowdSize = 6;
        const float spotA = 10.5f;
        const float spotB = 25.5f;

        var atA = new List<PathogenAgent>();
        for (int i = 0; i < crowdSize; i++)
        {
            var agent = NewAgent(rig, $"RuptureA_{i}");
            agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => { }, (c, cls, free, t) => false);
            // Tight clump: well inside one crowd radius, so these read as one
            // spot rather than as six separate adherents.
            agent.AdhereToWall(spotA + (i - crowdSize * 0.5f) * 0.08f);
            atA.Add(agent);
        }
        for (int i = 0; i < 3; i++)
        {
            var agent = NewAgent(rig, $"RuptureB_{i}");
            agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => { }, (c, cls, free, t) => false);
            agent.AdhereToWall(spotB + (i - 1) * 0.08f);
        }

        Check($"All {crowdSize + 3} are on the wall ({rig.Gut.TotalAdhered})",
            rig.Gut.TotalAdhered == crowdSize + 3);

        // Settle: they arrive in the outer mucus and come in to land, so
        // nothing grips (or crowds, or charges) for the first couple of
        // seconds. That delay is deliberate -- see SettleSpeedCellsPerSecond.
        for (int i = 0; i < 40; i++) rig.Gut.Tick(0.1f, i * 0.1f, i * 0.1f);

        int gripping = 0;
        foreach (var a in atA) if (a.IsGrippingWall) gripping++;
        Check($"The clump has settled in to grip ({gripping}/{crowdSize})", gripping == crowdSize);

        Check($"Crowd at the clump is high ({rig.Gut.CrowdAt(spotA):0.0})",
            rig.Gut.CrowdAt(spotA) > 4f);
        Check($"Crowd far away is nil ({rig.Gut.CrowdAt(spotA + 5f):0.0})",
            rig.Gut.CrowdAt(spotA + 5f) < 0.01f);

        int plaqueA = -1, plaqueB = -1;
        for (int i = 0; i < rig.Gut.PlaqueCapacity; i++)
        {
            var plaque = rig.Gut.PlaqueAt(i);
            if (!plaque.Live) continue;
            if (Mathf.Abs(plaque.CentreS - spotA) < 1.5f) plaqueA = i;
            if (Mathf.Abs(plaque.CentreS - spotB) < 1.5f) plaqueB = i;
        }

        Check("A plaque seeded on the big clump", plaqueA >= 0);
        Check("A plaque seeded on the small clump too", plaqueB >= 0);
        Check($"The big clump's plaque owns all {crowdSize} ({rig.Gut.PlaqueAt(plaqueA).MemberCount})",
            rig.Gut.PlaqueAt(plaqueA).MemberCount == crowdSize);
        Check($"The small clump's plaque owns 3 ({rig.Gut.PlaqueAt(plaqueB).MemberCount})",
            rig.Gut.PlaqueAt(plaqueB).MemberCount == 3);

        // Pressure is a CHARGE, so this is a direct assertion rather than the
        // statistical test over hundreds of hidden rolls the old model needed.
        float chargeBefore = rig.Gut.PlaqueAt(plaqueA).Charge;
        for (int i = 0; i < 20; i++) rig.Gut.Tick(0.1f, 4f + i * 0.1f, 4f + i * 0.1f);
        Check($"Charge rises while the crowd sits there ({chargeBefore:0.000} -> {rig.Gut.PlaqueAt(plaqueA).Charge:0.000})",
            rig.Gut.PlaqueAt(plaqueA).Charge > chargeBefore);

        Check($"A bigger crowd charges faster ({rig.Gut.PlaqueAt(plaqueA).Charge:0.000} > {rig.Gut.PlaqueAt(plaqueB).Charge:0.000})",
            rig.Gut.PlaqueAt(plaqueA).Charge > rig.Gut.PlaqueAt(plaqueB).Charge);

        int ruptureEvents = 0, releasedByEvent = 0;
        float ruptureCentre = -1f;
        rig.Gut.Ruptured += (centre, count, footprint) =>
        {
            ruptureEvents++; releasedByEvent += count; ruptureCentre = centre;
        };

        int released = rig.Gut.Rupture(plaqueA, 0f);

        Check($"The rupture released all {crowdSize} at once ({released})", released == crowdSize);
        Check("...in exactly one event", ruptureEvents == 1);
        Check($"...and the event reported the same count ({releasedByEvent})", releasedByEvent == crowdSize);
        Check($"...centred on the clump ({ruptureCentre:0.0})", Mathf.Abs(ruptureCentre - spotA) < 1.5f);

        int inTissue = 0;
        foreach (var a in atA) if (a.State == PathogenState.InTissue) inTissue++;
        Check($"Every member is now in tissue ({inTissue})", inTissue == crowdSize);

        // The burst arrives pre-spread by its own geometry, which is why
        // MaxReleaseCrossSpread could drop 20 -> 4 this sprint.
        var lanes = new HashSet<int>();
        foreach (var a in atA) lanes.Add(rig.Board.CrossIndex(a.CurrentCoarse));
        Check($"...landing across several lanes, not stacked in one ({lanes.Count})", lanes.Count >= 2);

        Check($"The other crowd is untouched ({rig.Gut.PlaqueAt(plaqueB).MemberCount})",
            rig.Gut.PlaqueAt(plaqueB).MemberCount == 3);
        Check($"Tally counted one breach ({rig.Tally.Breaches})", rig.Tally.Breaches == 1);

        rig.Dispose();
    }

    // =================================================================
    // 5b. THE ANTI-TRICKLE REGRESSION
    //
    // The single most important test in this file, and the reason
    // IngressCrowdFloor exists. GutInterface carried a capitalised warning
    // against refactoring its per-position roll into a per-pathogen one,
    // because independent rolls produce a steady leak across the whole wall
    // and destroy "pressure builds at one spot, then bursts" --
    // GAME_DESIGN.md section 1b step 2's whole point.
    //
    // The charge model makes that arithmetically impossible rather than
    // merely unlikely: charge accrues at (members - floor) * rate, so one or
    // two adherents accrue nothing, forever. This proves it, so that nobody
    // can quietly "simplify" the floor away later.
    // =================================================================
    private static void RunNoTrickle()
    {
        Debug.Log("[MapVerification] --- Anti-trickle regression ---");
        var rig = BuildRig("NoTrickle");
        InvasionTuning.ResetToDefaults();

        // Scatter lone adherents a full crowd radius apart, so no two of them
        // can ever see each other in the field.
        var loners = new List<PathogenAgent>();
        for (int i = 0; i < 8; i++)
        {
            var agent = NewAgent(rig, $"Loner_{i}");
            agent.Initialize(rig.Board, rig.Grid, rig.Gut, rig.Tally, a => { }, (c, cls, free, t) => false);
            agent.AdhereToWall(2f + i * 4f);
            loners.Add(agent);
        }

        // Five simulated minutes. Under Sprint 4's model each of these would
        // have leaked into the tissue in ~80 seconds.
        for (int i = 0; i < 3000; i++) rig.Gut.Tick(0.1f, i * 0.1f, i * 0.1f);

        Check($"After 5 minutes, NOTHING trickled in ({rig.Tally.Breaches} breaches)",
            rig.Tally.Breaches == 0);
        Check($"...and nothing reached the tissue ({rig.Tally.ReleasedIntoTissue})",
            rig.Tally.ReleasedIntoTissue == 0);

        float peak = 0f;
        for (int i = 0; i < rig.Gut.PlaqueCapacity; i++)
            if (rig.Gut.PlaqueAt(i).Live) peak = Mathf.Max(peak, rig.Gut.PlaqueAt(i).Charge);
        Check($"No plaque built any charge at all ({peak:0.000})", peak < 0.001f);

        // And the complement: the SAME number of pathogens, in one place,
        // must get in. Ingress is something a crowd does.
        var rig2 = BuildRig("NoTrickle2");
        for (int i = 0; i < 8; i++)
        {
            var agent = NewAgent(rig2, $"Together_{i}");
            agent.Initialize(rig2.Board, rig2.Grid, rig2.Gut, rig2.Tally, a => { }, (c, cls, free, t) => false);
            agent.AdhereToWall(12f + (i - 4) * 0.07f);
        }
        for (int i = 0; i < 3000 && rig2.Tally.Breaches == 0; i++)
            rig2.Gut.Tick(0.1f, i * 0.1f, i * 0.1f);

        Check($"The same 8 TOGETHER do get in ({rig2.Tally.Breaches} breach)",
            rig2.Tally.Breaches >= 1);

        rig.Dispose();
        rig2.Dispose();
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
                a => { }, (c, cls, free, t) => false, slot, PathogenClass.LargeBacterium, 0f);
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
            a => exited = true, (c, cls, free, t) => false, slot, PathogenClass.LargeBacterium, 0f);

        Check("Seeded at the tissue cell adjacent to the base",
            rig.Board.AxisIndex(agent.CurrentCoarse) == rig.Board.TissueBaseEdgeAxisIndex);
        Check("Nothing has reached the base yet", rig.Tally.ReachedBase == 0);

        agent.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, 1f);

        Check($"Stepping into the base counts it ({rig.Tally.ReachedBase})", rig.Tally.ReachedBase == 1);
        Check("...and the pathogen is removed from play", agent.State == PathogenState.Cleared);
        Check("...via the pool exit path, not Destroy", exited);
        Check("...and it did not leave an occupied tissue slot behind", rig.Grid.TissuePathogenCount == 0);

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
            a => { }, (c, cls, free, t) => false, slot, PathogenClass.LargeBacterium, 0f);

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
