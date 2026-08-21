using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;

/// <summary>
/// Sprint 1 closing task's required self-verification: a headless (no
/// GameObjects that need play mode, no rendering) comparison of the
/// average unit-to-nearest-infected-cell distance with cytokine sensing
/// forced OFF vs. forced ON, over a simulated multi-minute window.
///
/// This drives the ACTUAL production classes (TissueGrid, CytokineField,
/// Chemotaxis.ChooseNextStep -- the same static method SearchUnit.StepOnce
/// calls) rather than a reimplementation, so the numbers it prints reflect
/// the real algorithm, not a stand-in. It works outside play mode because
/// TissueGrid/CytokineField/Chemotaxis take simulated time and randomness
/// as explicit inputs rather than reading UnityEngine.Time internally (see
/// TissueGrid.GetSecretionStrength's currentTime parameter).
///
/// Run via:
///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod CytokineVerification.RunComparison
/// Output goes to Debug.Log, which lands in Editor.log in batchmode.
/// </summary>
public static class CytokineVerification
{
    private const float SimulatedSeconds = 150f; // 2.5 minutes -- "a couple minutes of watching"
    private const int UnitCount = 10;

    public static void RunComparison()
    {
        Debug.Log("[CytokineVerification] Starting OFF vs ON comparison ...");
        RunOneCondition("OFF (cytokine sensing disabled -- rung 1 baseline)", cytokineEnabled: false);
        RunOneCondition("ON  (cytokine sensing enabled -- rung 2)", cytokineEnabled: true);
        Debug.Log("[CytokineVerification] Done.");
    }

    /// <summary>Tuning sweep for Chemotaxis.GradientSharpness -- not part
    /// of the final self-verification, just how the shipped default (see
    /// Chemotaxis.cs) was chosen and a tool to re-tune it later. 1-minute
    /// buckets hide HOW convergence happens (a slow visible drift vs. an
    /// instant snap look identical once both have converged by minute 2).
    /// This prints 10-second buckets over the first minute only, plus each
    /// unit's own time-to-first-arrival (distance 0), to judge "visible
    /// drift that still looks like wandering" vs. "looks like teleporting
    /// in a straight line" -- a judgment call the aggregate distance number
    /// alone can't make.</summary>
    public static void RunFineGrainedSweep()
    {
        float original = Chemotaxis.GradientSharpness;
        foreach (float sharpness in new[] { 2f, 4f, 8f })
        {
            Chemotaxis.GradientSharpness = sharpness;
            RunFineGrained($"ON, GradientSharpness={sharpness}", cytokineEnabled: true);
        }
        Chemotaxis.GradientSharpness = original;
        Debug.Log("[CytokineVerification] Fine-grained sweep done.");
    }

    private static void RunFineGrained(string label, bool cytokineEnabled)
    {
        Random.InitState(20260819);

        var boardGo = new GameObject("CytokineVerification_Board");
        var board = boardGo.AddComponent<BoardConfig>();
        var tissueGrid = new TissueGrid(board);
        var cytokineField = new CytokineField(board);

        var infectedCoarse = new[]
        {
            new CoarseCoord(4, 1), new CoarseCoord(11, 3), new CoarseCoord(18, 0),
            new CoarseCoord(23, 4), new CoarseCoord(27, 2),
        };
        var dummyAgentObjects = new List<GameObject>();
        float simTime = 0f;
        foreach (var c in infectedCoarse)
        {
            var agentGo = new GameObject("DummyInfectedSource");
            var agent = agentGo.AddComponent<PathogenAgent>();
            dummyAgentObjects.Add(agentGo);
            tissueGrid.TryClaimOccupant(c, agent, simTime);
        }
        cytokineField.Recompute(tissueGrid.InfectedSources(simTime));

        var positions = new FineCoord[UnitCount];
        var speeds = new int[UnitCount];
        var arrivalTime = new float[UnitCount];
        for (int i = 0; i < UnitCount; i++)
        {
            positions[i] = new FineCoord(Random.Range(0, board.FineColumns), Random.Range(0, board.FineRows));
            speeds[i] = (i % 2 == 0) ? 1 : 3;
            arrivalTime[i] = -1f;
        }

        var candidateBuf = new FineCoord[4];
        var weightBuf = new float[4];
        const float tickSeconds = BoardConfig.TickIntervalSeconds;
        const float fieldRecomputeIntervalSeconds = 0.4f;
        int totalTicks = Mathf.RoundToInt(60f / tickSeconds);

        float fieldTimer = 0f;
        var bucketSums = new float[6]; // six 10s buckets over the first minute
        var bucketCounts = new int[6];

        for (int tick = 0; tick < totalTicks; tick++)
        {
            simTime += tickSeconds;
            fieldTimer += tickSeconds;
            if (fieldTimer >= fieldRecomputeIntervalSeconds)
            {
                fieldTimer = 0f;
                cytokineField.Recompute(tissueGrid.InfectedSources(simTime));
            }

            for (int i = 0; i < UnitCount; i++)
            {
                for (int s = 0; s < speeds[i]; s++)
                {
                    positions[i] = Chemotaxis.ChooseNextStep(
                        positions[i], board, cytokineField, cytokineEnabled, candidateBuf, weightBuf);
                }
                if (arrivalTime[i] < 0f)
                {
                    var coarse = positions[i].ToCoarse(BoardConfig.FineSubdivision);
                    foreach (var inf in infectedCoarse)
                    {
                        if (inf.Column == coarse.Column && inf.Row == coarse.Row) { arrivalTime[i] = simTime; break; }
                    }
                }
            }

            float avgDist = AverageNearestInfectedDistance(positions, infectedCoarse, board);
            int bucket = Mathf.Clamp(Mathf.FloorToInt((tick * tickSeconds) / 10f), 0, 5);
            bucketSums[bucket] += avgDist;
            bucketCounts[bucket]++;
        }

        var bucketStrs = new List<string>();
        for (int b = 0; b < 6; b++)
            bucketStrs.Add(bucketCounts[b] > 0 ? (bucketSums[b] / bucketCounts[b]).ToString("F2") : "n/a");

        float sumArrival = 0f; int arrivedCount = 0;
        var arrivalStrs = new List<string>();
        foreach (var t in arrivalTime)
        {
            arrivalStrs.Add(t >= 0f ? t.ToString("F1") + "s" : "never(60s)");
            if (t >= 0f) { sumArrival += t; arrivedCount++; }
        }

        Debug.Log(
            $"[CytokineVerification] {label}\n" +
            $"  avg distance per 10s bucket (0-10,10-20,...,50-60): {string.Join(", ", bucketStrs)}\n" +
            $"  per-unit first-arrival time: {string.Join(", ", arrivalStrs)}\n" +
            $"  arrived {arrivedCount}/{UnitCount} within 60s, avg arrival {(arrivedCount > 0 ? (sumArrival / arrivedCount).ToString("F1") : "n/a")}s");

        foreach (var go in dummyAgentObjects) Object.DestroyImmediate(go);
        Object.DestroyImmediate(boardGo);
    }

    private static void RunOneCondition(string label, bool cytokineEnabled)
    {
        Random.InitState(20260819); // same seed for both runs -- only the toggle differs

        var boardGo = new GameObject("CytokineVerification_Board");
        var board = boardGo.AddComponent<BoardConfig>(); // default Columns = 30

        var tissueGrid = new TissueGrid(board);
        var cytokineField = new CytokineField(board);

        // Representative infected sites: spread across columns/rows the
        // way PathogenAgent's adhesion actually scatters them (see
        // docs/INTERFACE.md's pathogen section), not clustered at one
        // edge. Adhered at simulated t=0 so both conditions see the same
        // secretion ramp-up over the run.
        var infectedCoarse = new[]
        {
            new CoarseCoord(4, 1), new CoarseCoord(11, 3), new CoarseCoord(18, 0),
            new CoarseCoord(23, 4), new CoarseCoord(27, 2),
        };

        var dummyAgentObjects = new List<GameObject>();
        float simTime = 0f;
        foreach (var c in infectedCoarse)
        {
            var agentGo = new GameObject("DummyInfectedSource");
            var agent = agentGo.AddComponent<PathogenAgent>(); // never Initialize()'d -- only used as a non-null occupancy token
            dummyAgentObjects.Add(agentGo);
            tissueGrid.TryClaimOccupant(c, agent, simTime);
        }
        cytokineField.Recompute(tissueGrid.InfectedSources(simTime));

        // Virtual units: no GameObjects, just FineCoord state driven
        // through the exact same Chemotaxis.ChooseNextStep SearchUnit
        // calls every tick. Half macrophage speed (1 fine-tile/tick), half
        // neutrophil speed (3 fine-tiles/tick), matching GameBootstrap's
        // defaults.
        var positions = new FineCoord[UnitCount];
        var speeds = new int[UnitCount];
        for (int i = 0; i < UnitCount; i++)
        {
            positions[i] = new FineCoord(Random.Range(0, board.FineColumns), Random.Range(0, board.FineRows));
            speeds[i] = (i % 2 == 0) ? 1 : 3;
        }

        var candidateBuf = new FineCoord[4];
        var weightBuf = new float[4];

        const float tickSeconds = BoardConfig.TickIntervalSeconds; // 0.12s, same as real gameplay
        const float fieldRecomputeIntervalSeconds = 0.4f; // matches PathogenSpawner
        int totalTicks = Mathf.RoundToInt(SimulatedSeconds / tickSeconds);

        float fieldTimer = 0f;
        float sumFirstMinute = 0f, sumSecondMinute = 0f, sumThirdHalfMinute = 0f;
        int samplesFirst = 0, samplesSecond = 0, samplesThird = 0;

        for (int tick = 0; tick < totalTicks; tick++)
        {
            simTime += tickSeconds;

            fieldTimer += tickSeconds;
            if (fieldTimer >= fieldRecomputeIntervalSeconds)
            {
                fieldTimer = 0f;
                cytokineField.Recompute(tissueGrid.InfectedSources(simTime));
            }

            for (int i = 0; i < UnitCount; i++)
            {
                for (int s = 0; s < speeds[i]; s++)
                {
                    positions[i] = Chemotaxis.ChooseNextStep(
                        positions[i], board, cytokineField, cytokineEnabled, candidateBuf, weightBuf);
                }
            }

            float avgDist = AverageNearestInfectedDistance(positions, infectedCoarse, board);
            float simSeconds = tick * tickSeconds;
            if (simSeconds < 60f) { sumFirstMinute += avgDist; samplesFirst++; }
            else if (simSeconds < 120f) { sumSecondMinute += avgDist; samplesSecond++; }
            else { sumThirdHalfMinute += avgDist; samplesThird++; }
        }

        Debug.Log(
            $"[CytokineVerification] {label}\n" +
            $"  avg unit-to-nearest-infected-cell distance (coarse cells, Manhattan):\n" +
            $"    0:00-1:00 -> {(sumFirstMinute / samplesFirst):F2}\n" +
            $"    1:00-2:00 -> {(sumSecondMinute / samplesSecond):F2}\n" +
            $"    2:00-2:30 -> {(sumThirdHalfMinute / samplesThird):F2}");

        foreach (var go in dummyAgentObjects) Object.DestroyImmediate(go);
        Object.DestroyImmediate(boardGo);
    }

    private static float AverageNearestInfectedDistance(FineCoord[] positions, CoarseCoord[] infected, BoardConfig board)
    {
        float sum = 0f;
        foreach (var p in positions)
        {
            var coarse = p.ToCoarse(BoardConfig.FineSubdivision);
            int best = int.MaxValue;
            foreach (var inf in infected)
            {
                int d = Mathf.Abs(inf.Column - coarse.Column) + Mathf.Abs(inf.Row - coarse.Row);
                if (d < best) best = d;
            }
            sum += best;
        }
        return sum / positions.Length;
    }
}
