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

        Debug.Log("[KillLimitDiagnostic] === end ===");
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
