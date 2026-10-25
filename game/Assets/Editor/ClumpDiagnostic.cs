using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 31 diagnostic. The Director reports macrophage/neutrophil
/// clumping is STILL a serious problem after Sprint 28's crowd
/// desensitisation.
///
/// Hypothesis to test: the penalty is computed over a radius of 6 fine
/// tiles, but a cell only steps ONE fine tile per move. All four candidate
/// steps therefore sit inside almost the same neighbourhood and get almost
/// the same penalty — and a constant offset across all candidates
/// **cancels out of a softmax entirely**. If so, the feature has been
/// doing nothing at all, and turning its strength up would change nothing
/// either.
///
/// Measures clumping with repulsion off, on, and at 10x strength. If all
/// three are the same, the hypothesis holds.
/// </summary>
public static class ClumpDiagnostic
{
    [MenuItem("ImmunologyTD/Diagnose/Why do cells still clump?")]
    public static void RunAll()
    {
        Debug.Log("[ClumpDiagnostic] === Clumping ===");

        Measure("OFF", 0f, 1.6f);
        Measure("SHIPPED defaults", -1f, -1f);

        Debug.Log("[ClumpDiagnostic] === end ===");
    }

    private static void Measure(string label, float strength, float exponent, int freeCompanions = -1, int radius = -1, float falloff = -1f)
    {
        Random.InitState(3101);
        UnitTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        if (strength >= 0f) UnitTuning.RepulsionStrength = strength;
        if (exponent > 0f) UnitTuning.RepulsionExponent = exponent;
        if (freeCompanions >= 0) UnitTuning.RepulsionFreeCompanions = freeCompanions;
        if (radius > 0) UnitTuning.RepulsionRadiusFineTiles = radius;
        if (falloff > 0f) UnitTuning.RepulsionFalloffPower = falloff;

        var go = new GameObject("CD_Board");
        var junk = new System.Collections.Generic.List<GameObject>();
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var field = new CytokineField(board);

            // One strong infection source, so every cell has the same place
            // to want to be — the situation that produces the dogpile.
            var source = board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 6, board.CrossLength / 2);
            grid.SeedHostState(source, HostState.Infected, 0f);

            var profile = new UnitProfile
            {
                Kind = UnitKind.Macrophage, DisplayName = "M",
                FineTilesPerTick = 1, FootprintFineTiles = 3,
                MaxActiveChildren = 12, KillLimit = 9999,
                ContactRadiusFineTiles = 2, StressSenseChancePerTick = 0f,
            };

            const int n = 12;
            var units = new System.Collections.Generic.List<SearchUnit>();
            for (int i = 0; i < n; i++)
            {
                var ugo = new GameObject("CD_U"); ugo.AddComponent<SpriteRenderer>();
                var u = ugo.AddComponent<SearchUnit>(); junk.Add(ugo);
                int lane = 1 + (i % (board.CrossLength - 2));
                var start = board.CoarseCenterFine(
                    board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex + 1, lane));
                u.Initialize(board, grid, field, profile, start);
                units.Add(u);
            }

            // Drive the field and the units for a while.
            float t = 0f;
            for (int step = 0; step < 1200; step++)
            {
                t += 0.12f;
                grid.Tick(0.12f, t);
                // Drive a guaranteed, static gradient from one point. The
                // clumping the Director sees is many cells converging on one
                // signal, and this is that situation at its purest.
                if (step == 0)
                    field.Recompute(new[] { (source, TissueGrid.MaxSecretionStrength) });
                foreach (var u in units) if (u != null) u.SimulationTick(t);
            }

            // Clumping metrics: mean pairwise distance, and the worst
            // single coarse cell's occupancy.
            float sum = 0f; int pairs = 0;
            var occupancy = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var a in units)
            {
                if (a == null) continue;
                var ca = a.Current.ToCoarse(BoardConfig.FineSubdivision);
                string key = $"{ca.Column},{ca.Row}";
                occupancy[key] = occupancy.TryGetValue(key, out var c) ? c + 1 : 1;
                foreach (var b in units)
                {
                    if (b == null || a == b) continue;
                    sum += Mathf.Max(Mathf.Abs(a.Current.Column - b.Current.Column),
                                     Mathf.Abs(a.Current.Row - b.Current.Row));
                    pairs++;
                }
            }
            int worst = 0;
            foreach (var kv in occupancy) worst = Mathf.Max(worst, kv.Value);
            float mean = pairs > 0 ? sum / pairs : 0f;

            var srcFine = board.CoarseCenterFine(source);
            float atSource = field.SampleFine(srcFine);
            float oneAway = field.SampleFine(new FineCoord(srcFine.Column + 1, srcFine.Row));
            Debug.Log($"[ClumpDiagnostic]   (gradient check: {atSource:F2} at source, {oneAway:F2} one tile away)");

            Debug.Log($"[ClumpDiagnostic] {label,-26} mean pairwise distance {mean,6:F2} fine tiles,  " +
                      $"most cells in one coarse slot {worst,2}/{n},  distinct slots {occupancy.Count,2}");
        }
        finally
        {
            foreach (var j in junk) if (j != null) Object.DestroyImmediate(j);
            Object.DestroyImmediate(go);
            UnitTuning.ResetToDefaults();
        }
    }
}
