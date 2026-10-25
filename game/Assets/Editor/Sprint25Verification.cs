using ImmunologyTD.Adaptive;
using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.UI;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 25 -- the Director's refinement pass: finish the species jump,
/// fix the unrenderable shop labels, shrink the standing armies, move
/// complement off the knowledge ladder, and let pathogens change lane.
///
/// The font check is the one worth keeping longest. A UI string containing
/// a character the shipped font lacks does not warn, does not error and
/// does not draw a missing-glyph box -- the text is simply absent. That is
/// how "γδ T cell (IEL)" and "IFN-γ" shipped, survived a build and a
/// playtest, and had to be spotted by eye.
/// </summary>
public static class Sprint25Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 25 -- refinement pass")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2501);

        Debug.Log("[Sprint25Verification] === Refinement pass ===");

        RunEveryUiStringIsRenderable();
        RunSpeciesJumpIsComplete();
        RunKnowledgeLicensingSeesNamedSpecies();
        RunPopulationsAreSmallAndCapsBind();
        RunComplementIsStructuralNotLearned();
        RunLumenLaneChanging();
        RunSpawnSpreadsAcrossTheChannel();

        if (failures > 0)
            Debug.LogError($"[Sprint25Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint25Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint25Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint25Verification] FAIL -- {label}"); }
    }

    // =================================================================
    // 1. Nothing player-facing can silently vanish
    // =================================================================
    private static void RunEveryUiStringIsRenderable()
    {
        Debug.Log("[Sprint25Verification] --- Font coverage ---");

        var bad = UiTextAudit.FindUnrenderable();
        foreach (var o in bad) Debug.LogError($"[Sprint25Verification]   {o}");
        Check($"every player-facing string renders in the shipped font ({bad.Count} bad)",
            bad.Count == 0);

        // And prove the guard itself works, or it is worthless.
        Check("the guard rejects a Greek character", !UiTextAudit.IsRenderable("IFN-γ"));
        Check("the guard accepts the middot the blurbs use", UiTextAudit.IsRenderable("a · b"));
    }

    // =================================================================
    // 2. Class is an archetype; species is the identity
    // =================================================================
    private static void RunSpeciesJumpIsComplete()
    {
        Debug.Log("[Sprint25Verification] --- Species jump ---");

        var ledger = typeof(KnowledgeLedger);
        bool classKeyed = false;
        foreach (var m in ledger.GetMethods())
        {
            if (m.Name != "Get" && m.Name != "Add") continue;
            foreach (var par in m.GetParameters())
                if (par.ParameterType == typeof(PathogenClass)) classKeyed = true;
        }
        // The bridges are gone, so there is no longer any way to address
        // knowledge by class and accidentally teach "every virus at once".
        Check("KnowledgeLedger can no longer be keyed by class", !classKeyed);

        Check("the HUD sizes itself from the species roster",
            KnowledgeWatcher.Species == SpeciesRoster.Count);
        Check("...which is more than the three old classes",
            SpeciesRoster.Count > 3);
    }

    // =================================================================
    // 3. The licensing bug the half-migration left behind
    // =================================================================
    private static void RunKnowledgeLicensingSeesNamedSpecies()
    {
        Debug.Log("[Sprint25Verification] --- Licensing sees named species ---");

        var ledger = new KnowledgeLedger();
        KnowledgeLicence.Ledger = ledger;
        try
        {
            Check("a CTL tower starts unlicensed",
                !KnowledgeLicence.IsTowerLicensed(UnitKind.CytotoxicT));

            // Rotavirus is ordinal 3. The old code iterated PathogenClassCount
            // (3) and cast to PathogenClass, so NOTHING past index 2 could
            // ever license a tower -- learning rotavirus to 100% would have
            // left the CTL progenitor dark forever.
            ledger.Add(PathogenSpecies.Rotavirus, 100f);
            Check("learning a NAMED species licenses the CTL tower",
                KnowledgeLicence.IsTowerLicensed(UnitKind.CytotoxicT));

            Check("...and a CTL recognises that species",
                KnowledgeLicence.RecognisesSpecies(UnitKind.CytotoxicT, PathogenSpecies.Rotavirus));
            Check("...but still not an uncharacterised one",
                !KnowledgeLicence.RecognisesSpecies(UnitKind.CytotoxicT, PathogenSpecies.VibrioCholerae));
        }
        finally { KnowledgeLicence.ResetToDefaults(); }
    }

    // =================================================================
    // 4. Smaller armies, and caps that actually bind
    // =================================================================
    private static void RunPopulationsAreSmallAndCapsBind()
    {
        Debug.Log("[Sprint25Verification] --- Populations ---");
        AdaptiveTuning.ResetToDefaults();

        // The Sprint 18 rule, restated as a test: a cap that never binds is
        // an upgrade that buys nothing. A helper-T niche's steady state is
        // lifespan / interval.
        float steadyState = AdaptiveTuning.LymphocyteLifespanSeconds
                            / AdaptiveTuning.LymphocyteEmissionIntervalSeconds;
        Check("the lymphocyte cap BINDS at default numbers (so clonal expansion buys something)",
            AdaptiveTuning.LymphocyteMaxActiveChildren < steadyState);

        Check("clonal expansion's step is in proportion to the smaller base",
            ProgenitorUpgrades.ClonalCapPerLevel
                < AdaptiveTuning.LymphocyteMaxActiveChildren);

        Check("DC population is small enough to read", AdaptiveTuning.DcMaxActiveChildren <= 3);
    }

    // =================================================================
    // 5. Complement is structural
    // =================================================================
    private static void RunComplementIsStructuralNotLearned()
    {
        Debug.Log("[Sprint25Verification] --- Complement ---");

        foreach (var rung in KnowledgeLadder.Rungs)
            Check($"the knowledge ladder does not carry complement ({rung.ShortName})",
                rung.ShortName != "Compl");

        bool inShop = false;
        foreach (ShopItem it in System.Enum.GetValues(typeof(ShopItem)))
            if (it == ShopItem.ComplementBaseline) inShop = true;
        Check("complement is a body upgrade in the shop", inShop);
        Check("...and is priced", ShopTuning.BasePriceFor(ShopItem.ComplementBaseline) > 0);

        // Ordinals of the pre-existing items must not have moved, or saved
        // shop state silently means something else.
        Check("existing shop ordinals are undisturbed",
            (int)ShopItem.BarrierMucusTurnover == 0 && (int)ShopItem.MCellSampling == 6);
    }

    // =================================================================
    // 6. Lane changing
    // =================================================================
    private static void RunLumenLaneChanging()
    {
        Debug.Log("[Sprint25Verification] --- Lane changing ---");

        InvasionTuning.ResetToDefaults();
        var go = new GameObject("S25_Board");
        var board = go.AddComponent<BoardConfig>();
        board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);

        var agents = new System.Collections.Generic.List<GameObject>();
        try
        {
            // Hold a population in the lumen and watch whether depth moves.
            int moved = 0, total = 24;
            for (int i = 0; i < total; i++)
            {
                var ago = new GameObject($"S25_P{i}");
                ago.AddComponent<SpriteRenderer>();
                var a = ago.AddComponent<PathogenAgent>();
                agents.Add(ago);
                a.Initialize(board, grid, wall, tally, x => { }, (c, s, f, t) => false,
                             classOverride: PathogenClass.LargeBacterium);

                int startAxis = board.AxisIndex(a.CurrentCoarse);
                for (int step = 0; step < 12 && a.State == PathogenState.Lumen; step++)
                {
                    a.SimulationTick(InvasionTuning.LumenStepIntervalSeconds + 0.001f, step * 0.5f);
                }
                if (a.State == PathogenState.Lumen
                    && board.AxisIndex(a.CurrentCoarse) != startAxis) moved++;
            }

            // Before Sprint 25 this was exactly zero, always.
            Check("organisms change lane while drifting in the lumen", moved > 0);
            Check("...and most of them do over a dozen steps", moved > total / 3);
        }
        finally
        {
            foreach (var a in agents) if (a != null) Object.DestroyImmediate(a);
            Object.DestroyImmediate(go);
        }
    }

    // =================================================================
    // 7. Spawn spread
    // =================================================================
    private static void RunSpawnSpreadsAcrossTheChannel()
    {
        Debug.Log("[Sprint25Verification] --- Spawn spread ---");

        var go = new GameObject("S25_SpawnBoard");
        var board = go.AddComponent<BoardConfig>();
        board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);

        PathogenAgent.ResetSpawnPattern();
        var agents = new System.Collections.Generic.List<GameObject>();
        try
        {
            int lo = board.LumenNearWallAxisIndex;
            int span = board.AxisLength - lo;
            var seen = new System.Collections.Generic.HashSet<int>();

            // One wave's worth.
            int n = Mathf.Min(span, 10);
            for (int i = 0; i < n; i++)
            {
                var ago = new GameObject($"S25_S{i}");
                ago.AddComponent<SpriteRenderer>();
                var a = ago.AddComponent<PathogenAgent>();
                agents.Add(ago);
                a.Initialize(board, grid, wall, tally, x => { }, (c, s, f, t) => false,
                             classOverride: PathogenClass.LargeBacterium);
                seen.Add(board.AxisIndex(a.CurrentCoarse));
            }

            // A rotating cursor should touch most distinct depths; uniform
            // random over 10 draws routinely collides far more than this.
            Check($"a wave of {n} covers most distinct depths ({seen.Count})",
                seen.Count >= n - 2);
        }
        finally
        {
            foreach (var a in agents) if (a != null) Object.DestroyImmediate(a);
            Object.DestroyImmediate(go);
        }
    }
}
