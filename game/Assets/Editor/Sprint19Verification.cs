using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rounds;

/// <summary>
/// Sprint 19 verification: peristalsis, the Peyer's patch, and crypts.
///
///  1. Peristalsis' surge profile has mean speed 1 (so total transit time is
///     unchanged), is monotonic, and really surges (push >> drift).
///  2. A food round's bolus still exits at ~FoodItemTransitSeconds, but
///     covers far more lanes per second in the push window than between waves.
///  3. The Peyer's patch sits inside the board, at the configured width.
///     M cells sample nothing until the upgrade is bought, then sample only
///     over the dome, up to capacity. A DC collects the parcel only inside
///     the follicle, and the end-to-end DC tick picks it up as cargo.
///  4. Crypts: placement is stable as more are bought, avoids the patch,
///     and Empty ground near one regrows measurably faster.
///
/// Run (Linux): tools/unity.sh Sprint19Verification.RunAll
/// </summary>
public static class Sprint19Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint19Verification] Starting ...");
        InvasionTuning.ResetToDefaults();
        Peristalsis.ResetToDefaults();
        GutWallTuning.ResetToDefaults();
        AdaptiveTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        GutWallFeatures.Current = null;

        RunPeristalsisProfile();
        RunBolusTransit();
        RunPeyersPatch();
        RunDcCollectsFromFollicle();
        RunCrypts();

        GutWallFeatures.Current = null;
        InvasionTuning.ResetToDefaults();
        Peristalsis.ResetToDefaults();
        Debug.Log($"[Sprint19Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint19Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint19Verification] FAIL -- {label}"); }
    }

    private static BoardConfig NewBoard(string name, out GameObject go)
    {
        go = new GameObject(name);
        var board = go.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        return board;
    }

    // =================================================================
    // 1. The surge profile
    // =================================================================

    private static void RunPeristalsisProfile()
    {
        float T = InvasionTuning.FoodItemTransitSeconds;
        Check("travel reaches exactly 1 at the transit time", Mathf.Abs(Peristalsis.Travel01(T) - 1f) < 1e-3f);
        Check("travel is 0 at the start", Peristalsis.Travel01(0f) == 0f);

        bool monotonic = true;
        float prev = 0f, sumSpeed = 0f;
        int n = 3000;
        for (int i = 1; i <= n; i++)
        {
            float t = T * i / n;
            float v = Peristalsis.Travel01(t);
            if (v < prev - 1e-6f) monotonic = false;
            prev = v;
            sumSpeed += Peristalsis.SpeedFactor(t);
        }
        Check("travel never goes backwards", monotonic);
        Check("mean speed over a transit is 1 (pacing unchanged)", Mathf.Abs(sumSpeed / n - 1f) < 0.02f);

        float P = Peristalsis.Period;
        float peak = Peristalsis.SpeedFactor(P * Peristalsis.PushFraction * 0.5f);
        float drift = Peristalsis.SpeedFactor(P * 0.8f);
        Debug.Log($"[Sprint19Verification] surge peak x{peak:F2}, drift x{drift:F2}, period {P:F2}s");
        Check("it surges: peak push is >10x the drift between waves", peak > 10f * drift);
        Check("contraction is on during the push and off between waves",
            Peristalsis.Contraction(P * Peristalsis.PushFraction * 0.5f) > 0.9f && Peristalsis.Contraction(P * 0.8f) == 0f);

        // Travel derivative matches the speed profile (closed form is the integral).
        float dt = 0.001f, at = P * 0.1f;
        float numeric = (Peristalsis.Travel01(at + dt) - Peristalsis.Travel01(at)) / dt * T;
        Check("closed-form travel integrates the speed profile", Mathf.Abs(numeric - Peristalsis.SpeedFactor(at)) < 0.05f);
    }

    // =================================================================
    // 2. The bolus moves in surges and still exits on time
    // =================================================================

    private static void RunBolusTransit()
    {
        var board = NewBoard("S19_TransitBoard", out var boardGo);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);
        var tally = new InvasionTally();
        var gut = new GutInterface(board, grid, tally);
        var junk = new List<GameObject>();

        var go = new GameObject("S19_Spawner"); junk.Add(go);
        var spawner = go.AddComponent<PathogenSpawner>();
        var template = new GameObject("S19_PathogenTemplate"); junk.Add(template);
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        spawner.Initialize(board, grid, field, gut, tally, template);

        var def = new RoundDefinition { VirusWeight = 1f, BacteriumWeight = 0f, LargeBacteriumWeight = 0f };
        spawner.BeginRound(4, def);

        const float step = 0.05f;
        float t = 0f, exitAt = -1f;
        float P = Peristalsis.Period;
        float pushTravel = 0f, driftTravel = 0f;
        Vector3 lastPos = Peristalsis.BolusWorldPosition;
        for (int i = 0; i < 2000 && exitAt < 0f; i++)
        {
            t += step;
            spawner.Tick(step, t);
            if (!spawner.FoodActive) { exitAt = t; break; }
            // Measure travel in the first full cycle: push window vs drift.
            Vector3 pos = Peristalsis.BolusWorldPosition;
            if (t <= P)
            {
                float d = (pos - lastPos).magnitude;
                if (t % P < P * Peristalsis.PushFraction) pushTravel += d; else driftTravel += d;
            }
            lastPos = pos;
        }
        float T = InvasionTuning.FoodItemTransitSeconds;
        float laneTime = T / board.CrossLength;
        Debug.Log($"[Sprint19Verification] bolus exit at {exitAt:F2}s (transit {T}s); first-cycle travel push {pushTravel:F2} vs drift {driftTravel:F2}");
        Check("the bolus exits at the transit time (within one lane's worth)",
            exitAt > 0f && Mathf.Abs(exitAt - T) <= laneTime + step);
        Check("the bolus covers most of each cycle's ground during the push", pushTravel > 3f * driftTravel);
        Check("the bolus hook is cleared once it exits", !Peristalsis.BolusActive);

        foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
        Object.DestroyImmediate(boardGo);
    }

    // =================================================================
    // 3. Peyer's patch + M cells
    // =================================================================

    private static void RunPeyersPatch()
    {
        var board = NewBoard("S19_PatchBoard", out var boardGo);
        var wall = new GutWallFeatures(board);

        Check("patch is inside the board and the configured width",
            wall.PatchCrossStart >= 0 && wall.PatchCrossEnd < board.CrossLength &&
            wall.PatchCrossEnd - wall.PatchCrossStart + 1 == GutWallTuning.PatchWidthLanes);

        int lane = wall.PatchCrossStart + 1;
        var overDome = board.CoarseFromAxis(board.LumenNearWallAxisIndex, lane);
        var offDome = board.CoarseFromAxis(board.LumenNearWallAxisIndex, wall.PatchCrossEnd + 3);
        var midLumen = board.CoarseFromAxis(board.LumenNearWallAxisIndex + 4, lane);

        Check("level 0: M cells sample nothing (a landmark only)",
            !wall.TrySample(overDome, PathogenClass.IntracellularVirus, 1f, 0f) && wall.PendingAntigen == 0);

        wall.MCellLevel = 1;
        Check("bought: a pathogen over the dome is sampled", wall.TrySample(overDome, PathogenClass.IntracellularVirus, 1f, 0f));
        Check("...but not one off the patch", !wall.TrySample(offDome, PathogenClass.IntracellularVirus, 1f, 0f));
        Check("...nor one out in mid-channel", !wall.TrySample(midLumen, PathogenClass.IntracellularVirus, 1f, 0f));
        Check("a failed roll samples nothing", !wall.TrySample(overDome, PathogenClass.IntracellularVirus, 0.016f, 0.99f));

        for (int i = 0; i < 10; i++) wall.TrySample(overDome, PathogenClass.LargeBacterium, 1f, 0f);
        Check("antigen waiting in the dome is capped", wall.PendingAntigen == GutWallTuning.MCellAntigenCapacity);

        var follicle = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - 1, lane);
        var deepTissue = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - GutWallTuning.FollicleDepthCells - 1, lane);
        Check("a DC deep in tissue can't collect", !wall.TryTakeAntigen(deepTissue, out _));
        Check("a DC in the follicle collects the oldest parcel first",
            wall.TryTakeAntigen(follicle, out var cls) && cls == PathogenClass.IntracellularVirus);

        int rate1 = CountSamples(board, 1), rate3 = CountSamples(board, 3);
        Check($"higher M-cell level samples faster ({rate1} -> {rate3} in 200 rolls)", rate3 > rate1);

        Object.DestroyImmediate(boardGo);
    }

    private static int CountSamples(BoardConfig board, int level)
    {
        var w = new GutWallFeatures(board) { MCellLevel = level };
        var at = board.CoarseFromAxis(board.LumenNearWallAxisIndex, w.PatchCrossStart);
        var rng = new System.Random(7);
        int n = 0;
        for (int i = 0; i < 200; i++)
        {
            if (w.TrySample(at, PathogenClass.IntracellularVirus, 0.1f, (float)rng.NextDouble())) n++;
            w.TryTakeAntigen(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, w.PatchCrossStart), out _);  // keep it under capacity
        }
        return n;
    }

    private static void RunDcCollectsFromFollicle()
    {
        var board = NewBoard("S19_DcBoard", out var boardGo);
        var grid = new TissueGrid(board);
        var field = new CytokineField(board);
        var wall = new GutWallFeatures(board) { MCellLevel = 1 };
        GutWallFeatures.Current = wall;

        int lane = (wall.PatchCrossStart + wall.PatchCrossEnd) / 2;
        wall.TrySample(board.CoarseFromAxis(board.LumenNearWallAxisIndex, lane), PathogenClass.IntracellularBacterium, 1f, 0f);

        var dcGo = new GameObject("S19_DC");
        dcGo.AddComponent<SpriteRenderer>();
        var dc = dcGo.AddComponent<DendriticCell>();
        var node = new LymphNode(new KnowledgeLedger(), new Rect(-3f, -3f, 6f, 6f));
        dc.Initialize(board, grid, field, node, board.CoarseCenterFine(board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 1, lane)), null);

        // Outside the follicle first: nothing.
        dc.SimulationTick(0.1f);
        Check("a DC patrolling outside the follicle collects nothing", !dc.HasCargo && wall.PendingAntigen == 1);

        dc.DebugPlaceForTest(board.CoarseCenterFine(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - 1, lane)));
        dc.SimulationTick(0.2f);
        Check("a DC in the follicle picks up the M-cell parcel as cargo (no debris needed)",
            dc.HasCargo && dc.CargoClass == PathogenClass.IntracellularBacterium && wall.PendingAntigen == 0);

        GutWallFeatures.Current = null;
        Object.DestroyImmediate(dcGo);
        Object.DestroyImmediate(boardGo);
    }

    // =================================================================
    // 4. Crypts
    // =================================================================

    private static void RunCrypts()
    {
        var board = NewBoard("S19_CryptBoard", out var boardGo);
        var wall = new GutWallFeatures(board);

        wall.SetCryptCount(1);
        int first = wall.CryptLanes[0];
        wall.SetCryptCount(4);
        Check("buying more crypts never moves the first", wall.CryptLanes.Count == 4 && wall.CryptLanes[0] == first);
        bool clearOfPatch = true, distinct = true;
        for (int i = 0; i < wall.CryptLanes.Count; i++)
        {
            int l = wall.CryptLanes[i];
            if (l >= wall.PatchCrossStart - 1 && l <= wall.PatchCrossEnd + 1) clearOfPatch = false;
            for (int j = 0; j < i; j++) if (wall.CryptLanes[j] == l) distinct = false;
        }
        Check("crypts stay off the Peyer's patch", clearOfPatch);
        Check("no two crypts share a lane", distinct);

        var near = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - 1, first);
        var far = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex, first);
        Check("ground beside a crypt gets the regrowth bonus", wall.RegrowthMultiplier(near) > 1f);
        Check("ground beyond its reach does not", Mathf.Approximately(wall.RegrowthMultiplier(far), 1f));

        float plain = SecondsToRegrow(board, null, near);
        wall.SetCryptCount(1);
        float boosted = SecondsToRegrow(board, wall, near);
        Debug.Log($"[Sprint19Verification] regrow beside the wall: {plain:F1}s without a crypt, {boosted:F1}s with");
        Check("a cleared cell beside a crypt regrows measurably faster", boosted > 0f && plain > 0f && boosted < plain * 0.6f);

        GutWallFeatures.Current = null;
        Object.DestroyImmediate(boardGo);
    }

    private static float SecondsToRegrow(BoardConfig board, GutWallFeatures wall, CoarseCoord c)
    {
        GutWallFeatures.Current = wall;
        var grid = new TissueGrid(board);
        grid.KillHostCell(c);
        grid.ClearDebris(c, TissueGrid.FullDebris, 0f);
        float t = 0f;
        for (int i = 0; i < 4000 && grid.GetHostState(c) != HostState.Healthy; i++)
        {
            t += 0.25f;
            grid.Tick(0.25f, t);
        }
        GutWallFeatures.Current = null;
        return grid.GetHostState(c) == HostState.Healthy ? t : -1f;
    }
}
