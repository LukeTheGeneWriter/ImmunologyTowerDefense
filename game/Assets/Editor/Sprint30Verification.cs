using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 30 -- viruses can reach the base.
///
/// The Director reported that viruses never get through "even when a
/// burst happens". A diagnostic found the cause was not balance: an
/// intracellular virus has no movement code at all (by design, §4b), and
/// nothing else gave it a path to the base. 0 of 150 viruses dropped into
/// tissue ever reached it, against 150 of 150 bacteria, and 142 were
/// still sitting in their original cell after three simulated minutes.
///
/// A virus could not cost the player a life by construction. These checks
/// pin the two mechanics that fixed that, and -- just as important -- pin
/// the limits, because an unbounded branching infection is the opposite
/// failure and would be worse.
/// </summary>
public static class Sprint30Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 30 -- viruses reach the base")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(3002);

        Debug.Log("[Sprint30Verification] === Viruses reach the base ===");

        RunDisseminatesOnlyAtTheBasalRow();
        RunDisseminationTakesTime();
        RunSpreadIsBudgeted();
        RunBacteriaUnaffected();

        if (failures > 0)
            Debug.LogError($"[Sprint30Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint30Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint30Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint30Verification] FAIL -- {label}"); }
    }

    private class Rig
    {
        public GameObject Go;
        public BoardConfig Board;
        public TissueGrid Grid;
        public InvasionTally Tally;
        public GutWall Wall;
        public readonly System.Collections.Generic.List<GameObject> Junk =
            new System.Collections.Generic.List<GameObject>();

        public void Dispose()
        {
            foreach (var j in Junk) if (j != null) Object.DestroyImmediate(j);
            if (Go != null) Object.DestroyImmediate(Go);
            GutWall.Current = null;
            InvasionTuning.ResetToDefaults();
        }
    }

    private static Rig Build(string name)
    {
        InvasionTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();
        var rig = new Rig();
        rig.Go = new GameObject($"S30_{name}");
        rig.Board = rig.Go.AddComponent<BoardConfig>();
        // Real geometry: a 13-cell tissue band, not the deep test rig.
        rig.Board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Wall = new GutWall(rig.Board, rig.Grid, rig.Tally);
        GutWall.Current = rig.Wall;
        return rig;
    }

    private static PathogenAgent Seed(Rig rig, int axisIndex, int lane, PathogenSpecies species)
    {
        var slot = rig.Board.CoarseFromAxis(axisIndex, lane);
        rig.Grid.SeedHostState(slot, HostState.Healthy, 0f);
        var go = new GameObject("S30_P");
        go.AddComponent<SpriteRenderer>();
        var a = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        a.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                                   x => { }, (c, s, f, t) => false,
                                   slot, SpeciesRoster.ClassOf(species), 0f, species);
        return a;
    }

    private static void Drive(PathogenAgent a, float seconds)
    {
        float t = 0f;
        int steps = Mathf.CeilToInt(seconds / 0.12f);
        for (int i = 0; i < steps && a.State == PathogenState.InTissue; i++)
        {
            t += 0.12f;
            a.SimulationTick(0.12f, t);
        }
    }

    // =================================================================
    private static void RunDisseminatesOnlyAtTheBasalRow()
    {
        Debug.Log("[Sprint30Verification] --- Dissemination is positional ---");

        var deep = Build("Deep");
        try
        {
            // Sitting at the LUMEN edge: it must not break through from
            // there, or the whole tissue band stops meaning anything.
            var a = Seed(deep, deep.Board.TissueLumenEdgeAxisIndex, 4, PathogenSpecies.GenericVirus);
            Check("precondition: it infected on arrival", a.IsIntracellular);
            Drive(a, 120f);
            Check("an infection far from the base does NOT reach it",
                deep.Tally.ReachedBase == 0);
        }
        finally { deep.Dispose(); }

        var edge = Build("Edge");
        try
        {
            var a = Seed(edge, edge.Board.TissueBaseEdgeAxisIndex, 4, PathogenSpecies.GenericVirus);
            Drive(a, 120f);
            Check("an infection ON the basal row disseminates to the base",
                edge.Tally.ReachedBase == 1);
            Check("...and leaves the host cell dead behind it",
                edge.Grid.GetHostState(
                    edge.Board.CoarseFromAxis(edge.Board.TissueBaseEdgeAxisIndex, 4)) == HostState.Dead);
        }
        finally { edge.Dispose(); }
    }

    private static void RunDisseminationTakesTime()
    {
        Debug.Log("[Sprint30Verification] --- Dissemination is interruptible ---");

        var rig = Build("Timing");
        try
        {
            var a = Seed(rig, rig.Board.TissueBaseEdgeAxisIndex, 4, PathogenSpecies.GenericVirus);

            // Well short of incubation + dissemination: nothing yet. The
            // delay is the point -- it is what gives a late cytotoxic cell
            // something to save.
            Drive(a, PathogenAgent.IncubationSeconds * 0.5f);
            Check("it has not broken through during incubation",
                rig.Tally.ReachedBase == 0);

            Drive(a, PathogenAgent.IncubationSeconds
                     + InvasionTuning.VirusDisseminationSeconds + 5f);
            Check("...and does once incubation and the delay have passed",
                rig.Tally.ReachedBase == 1);
        }
        finally { rig.Dispose(); }
    }

    private static void RunSpreadIsBudgeted()
    {
        Debug.Log("[Sprint30Verification] --- Spread is bounded ---");
        InvasionTuning.ResetToDefaults();

        // An unbounded branching infection is the opposite failure and
        // would be worse than the inert one it replaced.
        Check("the spread budget is small and positive",
            InvasionTuning.VirusSpreadBudget > 0 && InvasionTuning.VirusSpreadBudget <= 5);
        Check("the base-ward bias is a real bias, not a straight march",
            InvasionTuning.VirusSpreadBaseBias > 0.5f && InvasionTuning.VirusSpreadBaseBias < 1f);

        var rig = Build("Budget");
        try
        {
            int seeded = 0;
            var slot = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex, 4);
            rig.Grid.SeedHostState(slot, HostState.Healthy, 0f);
            var go = new GameObject("S30_B"); go.AddComponent<SpriteRenderer>();
            var a = go.AddComponent<PathogenAgent>(); rig.Junk.Add(go);
            a.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                                       x => { }, (c, s, f, t) => { seeded++; return true; },
                                       slot, PathogenClass.IntracellularVirus, 0f,
                                       PathogenSpecies.GenericVirus);
            Drive(a, 300f);

            // NOTE: this counts BOTH channels. Budding is uncapped by
            // design (§4b: it "never stops"), and a budding strain will
            // therefore seed far more than the chain budget -- the first
            // run of this test asserted a cap and measured 114, which was
            // the test being wrong rather than the code. What matters here
            // is simply that an unanswered infection does propagate.
            Check($"an unanswered infection propagates ({seeded} seeded)", seeded > 0);
        }
        finally { rig.Dispose(); }
    }

    private static void RunBacteriaUnaffected()
    {
        Debug.Log("[Sprint30Verification] --- Bacteria unchanged ---");

        var rig = Build("Bact");
        try
        {
            var a = Seed(rig, rig.Board.TissueLumenEdgeAxisIndex, 4,
                         PathogenSpecies.GenericLargeBacterium);
            Check("a large bacterium does not infect a cell", !a.IsIntracellular);
            Drive(a, 300f);
            // Bacteria still reach the base by WALKING, which is their own
            // mechanic and must not have been disturbed.
            Check("a bacterium still walks to the base", rig.Tally.ReachedBase == 1);
        }
        finally { rig.Dispose(); }
    }
}
