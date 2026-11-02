using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 30 diagnostic, not a test.
///
/// The Sprint 29 research raised a suspicion worth measuring rather than
/// arguing about: the macrophage's "+8 kills before the cell is spent"
/// upgrade row may be **inert**, in the same way the helper-T population
/// cap was for several sprints.
///
/// Innate cells have no lifespan — they die only on reaching KillLimit. So
/// if a macrophage almost never gets to 20 kills before a round ends,
/// raising the ceiling to 28 buys the player nothing at all, while still
/// charging them 40 ATP a level.
///
/// This stands a tower up with a stream of pathogens in front of it and
/// reports the distribution of kills actually achieved.
/// </summary>
public static class KillLimitDiagnostic
{
    [MenuItem("ImmunologyTD/Diagnose/Is the kill-limit row inert?")]
    public static void RunAll()
    {
        Random.InitState(3003);
        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        UnitTuning.ResetToDefaults();

        Debug.Log("[KillLimitDiagnostic] === Do innate cells ever reach their kill limit? ===");

        Measure(UnitKind.Macrophage, killLimit: 20, speed: 1, roundSeconds: 45f);
        Measure(UnitKind.Macrophage, killLimit: 20, speed: 1, roundSeconds: 180f);
        Measure(UnitKind.Neutrophil, killLimit: 5, speed: 5, roundSeconds: 45f);

        // -- Sprint 32 ---------------------------------------------------
        // The Director's call is "make them turn over", which is a question
        // the mean above cannot answer: a limit has to bind for the TYPICAL
        // cell, and a mean of 0.9 is compatible with both "every cell gets
        // about one" and "one cell gets seven and seven get none" -- and
        // those two want opposite limits.
        //
        // So measure the DISTRIBUTION once, with the ceiling lifted out of
        // the way, and read every candidate limit off it. Deliberately one
        // measurement rather than a sweep of limits: a limit changes the
        // dynamics (a spent cell stops killing), so sweeping would conflate
        // "the limit binds" with "the limit cost us throughput".
        Debug.Log("[KillLimitDiagnostic] --- Sprint 32: kills per cell with NO effective ceiling ---");
        MeasureDistribution(UnitKind.Macrophage, speed: 1, roundSeconds: 45f);
        MeasureDistribution(UnitKind.Macrophage, speed: 1, roundSeconds: 135f);   // three rounds
        MeasureDistribution(UnitKind.Neutrophil, speed: 5, roundSeconds: 45f);
        MeasureDistribution(UnitKind.Neutrophil, speed: 5, roundSeconds: 135f);

        Debug.Log("[KillLimitDiagnostic] === end ===");
    }

    /// <summary>
    /// Sprint 32: the kills-per-cell distribution with the ceiling lifted
    /// (limit 999), and the implied "what fraction of cells would a limit of
    /// L actually retire" for each candidate L. A larger population than
    /// Measure's eight, because a distribution needs bodies.
    /// </summary>
    private static void MeasureDistribution(UnitKind kind, int speed, float roundSeconds)
    {
        var go = new GameObject("KLD_Dist_Board");
        var junk = new System.Collections.Generic.List<GameObject>();
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var field = new CytokineField(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            var profile = new UnitProfile
            {
                Kind = kind, DisplayName = kind.ToString(),
                FineTilesPerTick = speed, FootprintFineTiles = 3,
                MaxActiveChildren = 4,
                KillLimit = 999,                 // the ceiling is out of the way
                ContactRadiusFineTiles = 2,
                StressSenseChancePerTick = kind == UnitKind.Macrophage ? 0.003f : 0.002f,
            };

            const int cells = 24;
            var units = new System.Collections.Generic.List<SearchUnit>();
            for (int i = 0; i < cells; i++)
            {
                var ugo = new GameObject("KLD_DU"); ugo.AddComponent<SpriteRenderer>();
                var u = ugo.AddComponent<SearchUnit>(); junk.Add(ugo);
                var start = board.CoarseCenterFine(board.CoarseFromAxis(
                    board.TissueBaseEdgeAxisIndex + (i % board.TissueBandCells),
                    i % board.CrossLength));
                u.Initialize(board, grid, field, profile, start);
                units.Add(u);
            }

            float t = 0f;
            int steps = Mathf.CeilToInt(roundSeconds / 0.12f);
            int spawned = 0;
            for (int step = 0; step < steps; step++)
            {
                t += 0.12f;
                if (step % 10 == 0)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        int ai = board.TissueBaseEdgeAxisIndex + Random.Range(0, board.TissueBandCells);
                        int lane = Random.Range(0, board.CrossLength);
                        var slot = board.CoarseFromAxis(ai, lane);
                        if (!grid.IsOccupantFree(slot)) continue;
                        grid.SeedHostState(slot, HostState.Healthy, t);
                        var pgo = new GameObject("KLD_DP"); pgo.AddComponent<SpriteRenderer>();
                        var p = pgo.AddComponent<PathogenAgent>(); junk.Add(pgo);
                        p.InitializeInTissueDirect(board, grid, wall, tally, x => { }, (c, s, f, tt) => false,
                                                   slot, PathogenClass.LargeBacterium, t,
                                                   PathogenSpecies.GenericLargeBacterium);
                        spawned++;
                    }
                }
                foreach (var u in units) if (u != null) u.SimulationTick(t);
            }

            var kills = new System.Collections.Generic.List<int>();
            foreach (var u in units) if (u != null) kills.Add(u.Kills);
            kills.Sort();

            int total = 0; foreach (var k in kills) total += k;
            float mean = kills.Count > 0 ? total / (float)kills.Count : 0f;
            int median = kills.Count > 0 ? kills[kills.Count / 2] : 0;
            int p90 = kills.Count > 0 ? kills[Mathf.Min(kills.Count - 1, (int)(kills.Count * 0.9f))] : 0;

            var binding = new System.Text.StringBuilder();
            for (int limit = 1; limit <= 8; limit++)
            {
                int retired = 0;
                foreach (var k in kills) if (k >= limit) retired++;
                binding.Append($" L{limit}:{retired * 100 / Mathf.Max(1, kills.Count)}%");
            }

            Debug.Log($"[KillLimitDiagnostic] {kind,-11} {roundSeconds,5:F0}s  targets {spawned,3}  " +
                      $"mean {mean,4:F1}  median {median,2}  p90 {p90,2}  best {kills[kills.Count - 1],2}  " +
                      $"| cells retired by limit L:{binding}");
            Debug.Log($"[KillLimitDiagnostic]   raw kills/cell: {string.Join(",", kills)}");
        }
        finally
        {
            foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
            GutWall.Current = null;
            Object.DestroyImmediate(go);
        }
    }

    private static void Measure(UnitKind kind, int killLimit, int speed, float roundSeconds)
    {
        var go = new GameObject("KLD_Board");
        var junk = new System.Collections.Generic.List<GameObject>();
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var field = new CytokineField(board);
            var tally = new InvasionTally();
            var wall = new GutWall(board, grid, tally);
            GutWall.Current = wall;

            var profile = new UnitProfile
            {
                Kind = kind, DisplayName = kind.ToString(),
                FineTilesPerTick = speed, FootprintFineTiles = 3,
                MaxActiveChildren = 4, KillLimit = killLimit,
                ContactRadiusFineTiles = 2,
                StressSenseChancePerTick = kind == UnitKind.Macrophage ? 0.003f : 0.002f,
            };

            // A standing population of cells, and a constantly-replenished
            // field of extracellular bacteria for them to work through --
            // deliberately generous, so this measures the CEILING rather
            // than a realistic round.
            const int cells = 8;
            var units = new System.Collections.Generic.List<SearchUnit>();
            for (int i = 0; i < cells; i++)
            {
                var ugo = new GameObject("KLD_U"); ugo.AddComponent<SpriteRenderer>();
                var u = ugo.AddComponent<SearchUnit>(); junk.Add(ugo);
                var start = board.CoarseCenterFine(board.CoarseFromAxis(
                    board.TissueBaseEdgeAxisIndex + 2 + (i % 8), 2 + (i % 6)));
                u.Initialize(board, grid, field, profile, start);
                units.Add(u);
            }

            float t = 0f;
            int steps = Mathf.CeilToInt(roundSeconds / 0.12f);
            int spawned = 0;
            for (int step = 0; step < steps; step++)
            {
                t += 0.12f;

                // Keep ~12 targets alive in the tissue band at all times.
                if (step % 10 == 0)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        int ai = board.TissueBaseEdgeAxisIndex + Random.Range(0, board.TissueBandCells);
                        int lane = Random.Range(0, board.CrossLength);
                        var slot = board.CoarseFromAxis(ai, lane);
                        if (!grid.IsOccupantFree(slot)) continue;
                        grid.SeedHostState(slot, HostState.Healthy, t);
                        var pgo = new GameObject("KLD_P"); pgo.AddComponent<SpriteRenderer>();
                        var p = pgo.AddComponent<PathogenAgent>(); junk.Add(pgo);
                        p.InitializeInTissueDirect(board, grid, wall, tally, x => { }, (c, s, f, tt) => false,
                                                   slot, PathogenClass.LargeBacterium, t,
                                                   PathogenSpecies.GenericLargeBacterium);
                        spawned++;
                    }
                }

                foreach (var u in units) if (u != null) u.SimulationTick(t);
            }

            int maxKills = 0, totalKills = 0, atLimit = 0;
            foreach (var u in units)
            {
                if (u == null) continue;
                totalKills += u.Kills;
                maxKills = Mathf.Max(maxKills, u.Kills);
                if (u.Kills >= killLimit) atLimit++;
            }

            float mean = cells > 0 ? totalKills / (float)cells : 0f;
            Debug.Log($"[KillLimitDiagnostic] {kind,-11} limit {killLimit,2}  round {roundSeconds,5:F0}s  " +
                      $"targets offered {spawned,3}  mean kills/cell {mean,5:F1}  best {maxKills,2}  " +
                      $"reached limit {atLimit}/{cells}");
        }
        finally
        {
            foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
            GutWall.Current = null;
            Object.DestroyImmediate(go);
        }
    }
}
