using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;

/// <summary>
/// Sprint 18 verification: the twelve progenitor upgrade rows are wired to
/// the simulation (GAME_DESIGN.md §6d). Before this sprint buying a row
/// spent ATP and bumped a counter and nothing else.
///
///  1. Every row of every kind moves the field it names, by the right amount,
///     and buying twice is cumulative (recomputed from base + level).
///  2. Upgrades are per-TOWER -- including the adaptive kinds, whose base
///     numbers are process-wide statics. Upgrading one DC niche leaves a
///     second DC niche, and the AdaptiveTuning statics, untouched.
///  3. Upgrades are live: a DC / helper-T already fielded holds its tower's
///     tuning by reference and sees the upgrade without being re-emitted.
///  4. Clonal expansion actually changes the standing helper-T population
///     (the cap alone would not -- see ProgenitorUpgrades.ClonalRatePerLevel).
///  5. A wider TCR tolerance changes what the node counts as a match.
///
/// Run (Linux): tools/unity.sh Sprint18Verification.RunAll
/// </summary>
public static class Sprint18Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint18Verification] Starting ...");
        AdaptiveTuning.ResetToDefaults();

        RunInnateRows();
        RunAdaptiveRowsPerTower();
        RunLiveReference();
        RunClonalExpansion();
        RunMatchTolerance();

        AdaptiveTuning.ResetToDefaults();
        Debug.Log($"[Sprint18Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint18Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint18Verification] FAIL -- {label}"); }
    }

    // ------------------------------------------------------------------
    // Rig: a board, a node + director, and a marrow with five free slots.
    // ------------------------------------------------------------------

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public LymphNode Node;
        public AdaptiveDirector Director;
        public BoneMarrowManager Marrow;
        public UnitProfile Mac, Neu;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++) if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static Rig BuildRig()
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject("S18_Board");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Node = new LymphNode(new KnowledgeLedger(), new Rect(-3f, -3f, 6f, 6f));

        var dirGo = new GameObject("S18_Director");
        rig.Junk.Add(dirGo);
        rig.Director = dirGo.AddComponent<AdaptiveDirector>();
        rig.Director.Initialize(rig.Node, NewPool<Lymphocyte>(rig, "Lym"), NewPool<DendriticCell>(rig, "DC"),
            rig.Board, rig.Grid, rig.Field);

        // Same defaults as GameBootstrap, so "base + level" is checked
        // against the real starting numbers.
        rig.Mac = new UnitProfile
        {
            Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5,
            MaxActiveChildren = 10, KillLimit = 20, ContactRadiusFineTiles = 2, EfferocytosisDebrisPerTick = 0.05f,
        };
        rig.Neu = new UnitProfile
        {
            Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3,
            MaxActiveChildren = 10, KillLimit = 5, DegranulatesOnDepletion = true,
            DegranulationBurstMultiplier = 3f, ContactRadiusFineTiles = 2,
        };

        var marrowGo = new GameObject("S18_Marrow");
        rig.Junk.Add(marrowGo);
        rig.Marrow = marrowGo.AddComponent<BoneMarrowManager>();
        var positions = new Vector3[5];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(i * 2f, -8f, 0f);
        rig.Marrow.Initialize(rig.Board, rig.Grid, rig.Field, rig.Mac, NewPool<SearchUnit>(rig, "Mac"),
            rig.Neu, NewPool<SearchUnit>(rig, "Neu"), positions, 1f, null, rig.Director);
        return rig;
    }

    private static PrefabPool NewPool<T>(Rig rig, string label) where T : Component
    {
        var template = new GameObject($"S18_{label}Template");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<T>();
        template.SetActive(false);
        rig.Junk.Add(template);
        var poolGo = new GameObject($"S18_{label}Pool");
        rig.Junk.Add(poolGo);
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.SetPrefab(template);
        return pool;
    }

    /// <summary>Buy a row with no wallet (free) and no cap.</summary>
    private static bool Buy(Rig rig, int slot, int row) => rig.Marrow.UpgradeTower(slot, row, 10, 0);

    private static void Tick(Rig rig, float seconds)
    {
        int n = Mathf.RoundToInt(seconds / 0.12f);
        for (int i = 0; i < n; i++) { rig.Marrow.Tick(0.12f); rig.Director.Tick(0.12f); }
    }

    // =================================================================
    // 1. Innate rows write the tower's UnitLifecycleTuning
    // =================================================================

    private static void RunInnateRows()
    {
        var rig = BuildRig();
        rig.Marrow.PlaceTower(0, UnitKind.Macrophage);
        rig.Marrow.PlaceTower(1, UnitKind.Neutrophil);
        var mac = rig.Marrow.GetTuning(0);
        var neu = rig.Marrow.GetTuning(1);

        Buy(rig, 0, 0); Buy(rig, 0, 0);
        Check("macrophage efferocytic capacity: 0.05 -> 0.11 at level 2",
            Mathf.Approximately(mac.EfferocytosisDebrisPerTick, 0.05f + 2 * ProgenitorUpgrades.EfferocytosisPerLevel));
        Buy(rig, 0, 1);
        Check("macrophage tissue residency: kill limit 20 -> 28", mac.KillLimit == 28);
        Buy(rig, 0, 2);
        Check("macrophage pseudopod reach: contact radius 2 -> 3", mac.ContactRadiusFineTiles == 3);

        Buy(rig, 1, 0);
        Check("neutrophil controlled degranulation: burst 3 -> 2.5", Mathf.Approximately(neu.DegranulationBurstMultiplier, 2.5f));
        for (int i = 0; i < 5; i++) Buy(rig, 1, 0);
        Check("neutrophil degranulation burst floors at 1", Mathf.Approximately(neu.DegranulationBurstMultiplier, 1f));
        Buy(rig, 1, 1);
        Check("neutrophil extended lifespan: kill limit 5 -> 7", neu.KillLimit == 7);
        Buy(rig, 1, 2);
        Check("neutrophil rapid chemokinesis: 3 -> 4 fine tiles per tick", neu.FineTilesPerTick == 4);
        Check("...and the shared neutrophil profile is untouched", rig.Neu.FineTilesPerTick == 3);

        // Per-tower: a second neutrophil tower starts at the defaults.
        rig.Marrow.PlaceTower(2, UnitKind.Neutrophil);
        var neu2 = rig.Marrow.GetTuning(2);
        Check("a second neutrophil tower is unaffected by the first's upgrades",
            neu2.KillLimit == 5 && neu2.FineTilesPerTick == 3 && Mathf.Approximately(neu2.DegranulationBurstMultiplier, 3f));

        // The legacy single-upgrade shim applies row 0 as well.
        rig.Marrow.PlaceTower(3, UnitKind.Macrophage);
        rig.Marrow.UpgradeTower(3);
        Check("legacy UpgradeTower(int) applies row 0's effect",
            rig.Marrow.GetTuning(3).EfferocytosisDebrisPerTick > 0.05f + 0.001f);

        Check("an innate tower has no adaptive upgrade layer", rig.Marrow.GetAdaptiveTuning(0) == null);
        rig.Dispose();
    }

    // =================================================================
    // 2. Adaptive rows are per-tower, and never write the statics
    // =================================================================

    private static void RunAdaptiveRowsPerTower()
    {
        AdaptiveTuning.ResetToDefaults();
        var rig = BuildRig();
        rig.Marrow.PlaceTower(0, UnitKind.DendriticCell);
        rig.Marrow.PlaceTower(1, UnitKind.DendriticCell);
        rig.Marrow.PlaceTower(2, UnitKind.HelperT);
        var a = rig.Marrow.GetAdaptiveTuning(0);
        var b = rig.Marrow.GetAdaptiveTuning(1);
        var t = rig.Marrow.GetAdaptiveTuning(2);

        Buy(rig, 0, 0);
        Check("DC macropinocytosis: presentations 4 -> 6", AdaptiveUnitTuning.PresentationsPerCargo(a) == 6);
        Buy(rig, 0, 1);
        Check("DC CCR7: 3 -> 4 fine tiles per tick", AdaptiveUnitTuning.DcFineTilesPerTick(a) == 4);
        for (int i = 0; i < 4; i++) Buy(rig, 0, 2);
        Check("DC antigen-sparing bite floors at 0.1",
            Mathf.Approximately(AdaptiveUnitTuning.DcDebrisSamplePerBite(a), AdaptiveUnitTuning.DcDebrisBiteFloor));

        Check("the second DC niche is untouched",
            AdaptiveUnitTuning.PresentationsPerCargo(b) == 4 && AdaptiveUnitTuning.DcFineTilesPerTick(b) == 3 &&
            Mathf.Approximately(AdaptiveUnitTuning.DcDebrisSamplePerBite(b), 0.34f));
        Check("the AdaptiveTuning statics are untouched",
            AdaptiveTuning.DcPresentationsPerCargo == 4 && AdaptiveTuning.DcFineTilesPerTick == 3 &&
            Mathf.Approximately(AdaptiveTuning.DcDebrisSamplePerBite, 0.34f) &&
            AdaptiveTuning.MatchMaxHammingDistance == 2 && Mathf.Approximately(AdaptiveTuning.LymphocyteLifespanSeconds, 20f));

        Buy(rig, 2, 0);
        Check("helper-T clonal expansion: cap 8 -> 12 and rate x1.5",
            rig.Marrow.GetTuning(2).MaxActiveChildren == 12 && Mathf.Approximately(t.EmissionRateMultiplier, 1.5f));
        for (int i = 0; i < 3; i++) Buy(rig, 2, 1);
        Check("helper-T TCR affinity: match distance 2 -> 4, ceiling holds at 4",
            AdaptiveUnitTuning.MatchMaxHammingDistance(t) == AdaptiveUnitTuning.MatchDistanceCeiling);
        for (int i = 0; i < 5; i++) Buy(rig, 2, 2);
        Check("helper-T rapid recirculation: lifespan floors at 6s",
            Mathf.Approximately(AdaptiveUnitTuning.LymphocyteLifespanSeconds(t), AdaptiveUnitTuning.LymphocyteLifespanFloor));

        // Base numbers still flow through: a static override is seen under the bonus.
        AdaptiveTuning.DcPresentationsPerCargo = 10;
        Check("the bonus rides on the CURRENT static, not a snapshot", AdaptiveUnitTuning.PresentationsPerCargo(a) == 12);
        AdaptiveTuning.ResetToDefaults();
        rig.Dispose();
    }

    // =================================================================
    // 3. Fielded adaptive cells see an upgrade instantly
    // =================================================================

    private static void RunLiveReference()
    {
        AdaptiveTuning.ResetToDefaults();
        AdaptiveTuning.DcEmissionIntervalSeconds = 0.5f;
        AdaptiveTuning.LymphocyteEmissionIntervalSeconds = 0.5f;
        var rig = BuildRig();
        rig.Marrow.PlaceTower(0, UnitKind.DendriticCell);
        rig.Marrow.PlaceTower(1, UnitKind.HelperT);
        Tick(rig, 2f);

        var dcs = Object.FindObjectsByType<DendriticCell>();
        var lyms = Object.FindObjectsByType<Lymphocyte>();
        DendriticCell dc = null;
        foreach (var d in dcs) if (d.Tuning != null) { dc = d; break; }
        Lymphocyte lym = null;
        foreach (var l in lyms) if (l.Tuning != null) { lym = l; break; }
        Check("a DC and a helper-T were fielded before the upgrade", dc != null && lym != null);

        if (dc != null && lym != null)
        {
            Check("the fielded DC holds its tower's tuning by reference", ReferenceEquals(dc.Tuning, rig.Marrow.GetAdaptiveTuning(0)));
            Buy(rig, 1, 1);
            Check("the fielded helper-T's tolerance rose without re-emission", lym.MatchMaxHammingDistance == 3);
            Buy(rig, 1, 2);
            Check("the fielded helper-T's lifespan fell without re-emission", Mathf.Approximately(lym.LifespanSeconds, 16f));
        }
        rig.Dispose();
        AdaptiveTuning.ResetToDefaults();
    }

    // =================================================================
    // 4. Clonal expansion raises the standing population
    // =================================================================

    private static void RunClonalExpansion()
    {
        AdaptiveTuning.ResetToDefaults(); // real 20s lifespan, real 3.5s interval, cap 8
        var rig = BuildRig();
        rig.Marrow.PlaceTower(0, UnitKind.HelperT);
        rig.Marrow.PlaceTower(1, UnitKind.HelperT);
        Buy(rig, 1, 0); Buy(rig, 1, 0); // level 2: cap 16, rate x2

        Tick(rig, 60f); // three lifespans -- well past steady state
        int plain = rig.Director.LymphocyteCount(0);
        int expanded = rig.Director.LymphocyteCount(1);
        Debug.Log($"[Sprint18Verification] steady helper-T population: base {plain}, clonal x2 {expanded}");
        Check("base tower sits under its cap of 8 (why the cap alone was inert)", plain < 8);
        Check("clonal expansion level 2 holds clearly more helper-T cells", expanded >= plain + 3);
        rig.Dispose();
    }

    // =================================================================
    // 5. TCR tolerance
    // =================================================================

    private static void RunMatchTolerance()
    {
        AdaptiveTuning.ResetToDefaults();
        byte a = 0b00000000, b = 0b00000111; // Hamming distance 3
        Check("distance-3 tags do not match at the default tolerance", !Antigen.IsMatch(a, b));
        Check("...but do at a helper-T's upgraded tolerance of 3", Antigen.IsMatch(a, b, 3));
    }
}
