using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 30 diagnostic, not a test. The Director reports that viruses
/// "never get through even when a burst happens".
///
/// There are at least four places a virus could be dying, and tuning the
/// wrong one would make things worse:
///   1. it never reaches the wall (lane changing favours motile bacteria);
///   2. it reaches the wall but never contributes to a breach;
///   3. it gets into tissue but never establishes an infection;
///   4. it establishes, but an intracellular infection cannot ADVANCE, so
///      it can never reach the base by design.
///
/// This walks a population through the real production code and counts
/// where they actually end up. Run it before changing a single number.
/// </summary>
public static class VirusDiagnostic
{
    [MenuItem("ImmunologyTD/Diagnose/Where do viruses die?")]
    public static void RunAll()
    {
        Random.InitState(3001);
        InvasionTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();

        Debug.Log("[VirusDiagnostic] === Where do viruses actually die? ===");

        LaneAndAdhesion();
        TissueFate();
        DisseminationAtBaseEdge();
        FocusMarch();

        Debug.Log("[VirusDiagnostic] === end ===");
    }

    // -----------------------------------------------------------------
    // Stage 1: lumen -> wall. Does a virion ever get near the epithelium?
    // -----------------------------------------------------------------
    private static void LaneAndAdhesion()
    {
        var go = new GameObject("VD_Board");
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            foreach (var species in new[] { PathogenSpecies.GenericVirus, PathogenSpecies.GenericLargeBacterium })
            {
                int n = 200, adhered = 0, excreted = 0;
                float depthSum = 0f; int depthSamples = 0;
                var junk = new System.Collections.Generic.List<GameObject>();

                for (int i = 0; i < n; i++)
                {
                    var ago = new GameObject("VD_P");
                    ago.AddComponent<SpriteRenderer>();
                    var a = ago.AddComponent<PathogenAgent>();
                    junk.Add(ago);
                    a.Initialize(board, grid, wall, tally, x => { }, (c, s, f, t) => false,
                                 speciesOverride: species);

                    float t0 = 0f;
                    for (int step = 0; step < 220 && a.State == PathogenState.Lumen; step++)
                    {
                        t0 += InvasionTuning.LumenStepIntervalSeconds + 0.001f;
                        a.SimulationTick(InvasionTuning.LumenStepIntervalSeconds + 0.001f, t0);
                        if (a.State == PathogenState.Lumen)
                        {
                            depthSum += board.LumenDepthFromInterface(a.CurrentCoarse);
                            depthSamples++;
                        }
                    }
                    if (a.State == PathogenState.AtInterface) adhered++;
                    else if (a.State == PathogenState.Cleared) excreted++;
                }

                float meanDepth = depthSamples > 0 ? depthSum / depthSamples : -1f;
                Debug.Log($"[VirusDiagnostic] {species,-28} adhered {adhered,3}/{n}  " +
                          $"excreted {excreted,3}  mean depth from wall {meanDepth:F2} cells");

                foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
            }
        }
        finally { GutWall.Current = null; Object.DestroyImmediate(go); }
    }

    // -----------------------------------------------------------------
    // Stage 3: an infection sitting on the basal row should break through.
    // -----------------------------------------------------------------
    private static void DisseminationAtBaseEdge()
    {
        var go = new GameObject("VD_Board3");
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            int n = 60, through = 0;
            var junk = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < n; i++)
            {
                int lane = 2 + (i % (board.CrossLength - 4));
                var slot = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex, lane);
                grid.SeedHostState(slot, HostState.Healthy, 0f);

                var ago = new GameObject("VD_D");
                ago.AddComponent<SpriteRenderer>();
                var a = ago.AddComponent<PathogenAgent>();
                junk.Add(ago);
                a.InitializeInTissueDirect(board, grid, wall, tally, x => { }, (c, s2, f, t) => false,
                                           slot, PathogenClass.IntracellularVirus, 0f,
                                           PathogenSpecies.GenericVirus);
                float t0 = 0f;
                for (int step = 0; step < 900 && a.State == PathogenState.InTissue; step++)
                {
                    t0 += 0.12f; a.SimulationTick(0.12f, t0);
                }
            }
            through = tally.ReachedBase;
            Debug.Log($"[VirusDiagnostic] infection seeded ON the basal row: {through}/{n} disseminated to base");
            foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
        }
        finally { GutWall.Current = null; Object.DestroyImmediate(go); }
    }

    // -----------------------------------------------------------------
    // Stage 4: does a focus actually MARCH inward when spread works?
    // -----------------------------------------------------------------
    private static void FocusMarch()
    {
        var go = new GameObject("VD_Board4");
        try
        {
            // REAL geometry: 25 x 10, base 6, lumen 6 -> a 13-cell tissue
            // band. The other stages use a deep 100x40 rig, which makes the
            // march look four times longer than it is in the game.
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            // Seed healthy tissue across the whole band.
            for (int ai = board.TissueBaseEdgeAxisIndex; ai <= board.TissueLumenEdgeAxisIndex; ai++)
                for (int cross = 0; cross < board.CrossLength; cross++)
                    grid.SeedHostState(board.CoarseFromAxis(ai, cross), HostState.Healthy, 0f);

            var live = new System.Collections.Generic.List<PathogenAgent>();
            var junk = new System.Collections.Generic.List<GameObject>();

            System.Func<CoarseCoord, PathogenSpecies, bool, float, bool> spawn = null;
            spawn = (at, sp, free, t) =>
            {
                if (live.Count > 400) return false;
                foreach (var off in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    var c = board.OffsetInAxisFrame(at, off.Item1, off.Item2);
                    if (board.BandOf(c) != BoardBand.Tissue) continue;
                    if (!grid.IsHealthyHost(c)) continue;
                    var g2 = new GameObject("VD_M"); g2.AddComponent<SpriteRenderer>();
                    var a2 = g2.AddComponent<PathogenAgent>(); junk.Add(g2);
                    a2.InitializeInTissueDirect(board, grid, wall, tally, x => { }, spawn,
                                                c, PathogenClass.IntracellularVirus, t,
                                                PathogenSpecies.GenericVirus);
                    live.Add(a2);
                    return true;
                }
                return false;
            };

            int lane = board.CrossLength / 2;
            var start = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, lane);
            var g = new GameObject("VD_M0"); g.AddComponent<SpriteRenderer>();
            var seed = g.AddComponent<PathogenAgent>(); junk.Add(g);
            seed.InitializeInTissueDirect(board, grid, wall, tally, x => { }, spawn,
                                          start, PathogenClass.IntracellularVirus, 0f,
                                          PathogenSpecies.GenericVirus);
            live.Add(seed);

            float time = 0f;
            int deepest = board.AxisIndex(start);
            for (int step = 0; step < 4000; step++)
            {
                time += 0.12f;
                for (int i = live.Count - 1; i >= 0; i--)
                {
                    var a = live[i];
                    if (a == null || a.State != PathogenState.InTissue) { live.RemoveAt(i); continue; }
                    a.SimulationTick(0.12f, time);
                    if (a.State == PathogenState.InTissue)
                        deepest = Mathf.Min(deepest, board.AxisIndex(a.CurrentCoarse));
                }
                if (tally.ReachedBase > 0) break;
            }

            int span = board.AxisIndex(start) - board.TissueBaseEdgeAxisIndex;
            Debug.Log($"[VirusDiagnostic] one focus, unanswered: reached base {tally.ReachedBase}, " +
                      $"deepest axis {deepest} (started {board.AxisIndex(start)}, base edge " +
                      $"{board.TissueBaseEdgeAxisIndex}, span {span}), after {time:F0}s, " +
                      $"{junk.Count} infections seeded");
            foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
        }
        finally { GutWall.Current = null; Object.DestroyImmediate(go); }
    }

    // -----------------------------------------------------------------
    // Stage 2: in tissue. Can a virus ever reach the base?
    // -----------------------------------------------------------------
    private static void TissueFate()
    {
        var go = new GameObject("VD_Board2");
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            foreach (var species in new[] { PathogenSpecies.GenericVirus, PathogenSpecies.GenericLargeBacterium })
            {
                int n = 150, infected = 0, reachedBase = 0, cleared = 0, stillGoing = 0;
                var junk = new System.Collections.Generic.List<GameObject>();
                int before = tally.ReachedBase;

                for (int i = 0; i < n; i++)
                {
                    // Drop it in tissue at the lumen edge, as a breach would.
                    int lane = 2 + (i % (board.CrossLength - 4));
                    var slot = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, lane);
                    grid.SeedHostState(slot, HostState.Healthy, 0f);

                    var ago = new GameObject("VD_T");
                    ago.AddComponent<SpriteRenderer>();
                    var a = ago.AddComponent<PathogenAgent>();
                    junk.Add(ago);
                    a.InitializeInTissueDirect(board, grid, wall, tally, x => { }, (c, s, f, t) => false,
                                               slot, SpeciesRoster.ClassOf(species), 0f, species);

                    if (a.IsIntracellular) infected++;

                    float t = 0f;
                    for (int step = 0; step < 1500 && a.State == PathogenState.InTissue; step++)
                    {
                        t += 0.12f;
                        a.SimulationTick(0.12f, t);
                    }
                    if (a.State == PathogenState.InTissue) stillGoing++;
                    else if (a.State == PathogenState.Cleared) cleared++;
                }

                reachedBase = tally.ReachedBase - before;
                Debug.Log($"[VirusDiagnostic] {species,-28} of {n} dropped in tissue: " +
                          $"infected-on-arrival {infected,3}  reached base {reachedBase,3}  " +
                          $"cleared {cleared,3}  still alive {stillGoing,3}");

                foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
            }
        }
        finally { GutWall.Current = null; Object.DestroyImmediate(go); }
    }
}
