using System.Collections.Generic;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 23 -- the adaptive and unconventional roster: cytotoxic T cells,
/// NK/ILC1, gamma-delta IELs, and B-cell-derived plasma cells.
///
/// Three of these checks exist because of a specific way this sprint could
/// have gone wrong, and those are the ones worth keeping:
///
///  1. **Every tissue kind has its own profile.** Before this sprint,
///     `ProfileFor`/`PoolFor` were binary ternaries -- `kind == Macrophage ?
///     macrophageProfile : neutrophilProfile`. Adding a kind was not a
///     compile error, it was a SILENT WRONG ANSWER: a cytotoxic T cell would
///     have been handed neutrophil stats and a neutrophil sprite, and the
///     game would have looked almost right.
///
///  2. **An apoptotic kill leaves the tissue recoverable.** The infected
///     cell dies either way -- it is the infection's shell, and killing it
///     is the only route to what is inside. What the ~10% rung buys is the
///     MANNER of death: a cytotoxic cell induces apoptosis, leaving a small
///     tidy pile, where an innate cell damages the cell to death and leaves
///     a full necrotic spill that must be cleared before anything regrows.
///     If someone later collapses the apoptotic branch into the necrotic
///     one, the observable (the infection clears) survives and the reason
///     the player bought the tower does not.
///
///  3. **A knowledge-gated tower emits nothing while unlicensed.** This is
///     the first mechanic in the game that reads KnowledgeLedger and changes
///     behaviour rather than just the HUD.
///
/// Seeded, per Sprint 22's lesson: several of these are races, and an
/// unseeded race gives a harness that passes alone and fails in the suite.
/// </summary>
public static class Sprint23Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 23 -- the adaptive roster")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2311);

        Debug.Log("[Sprint23Verification] === The adaptive roster ===");

        RunPredicatesAreOrthogonal();
        RunEveryTissueKindHasItsOwnProfile();
        RunApoptoticKillIsCleanerNotGentler();
        RunNecroticKillLeavesAMess();
        RunStressMeterAccumulates();
        RunCooperationIsSuperadditive();
        RunUnlicensedTowerEmitsNothing();
        RunPerSpeciesRecognition();
        RunAntibodyCoats();
        RunGammaDeltaRepairsTheWall();
        RunIfnGammaActivatesMacrophages();

        if (failures > 0)
            Debug.LogError($"[Sprint23Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint23Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint23Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint23Verification] FAIL -- {label}"); }
    }

    // =================================================================

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public InvasionTally Tally;
        public GutWall Wall;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++)
                if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
            GutWall.Current = null;
            KnowledgeLicence.ResetToDefaults();
        }
    }

    private static Rig BuildRig(string name)
    {
        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        UnitTuning.ResetToDefaults();
        AntibodyTuning.ResetToDefaults();
        KnowledgeLicence.ResetToDefaults();

        var rig = new Rig();
        rig.BoardGo = new GameObject($"Sprint23_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Wall = new GutWall(rig.Board, rig.Grid, rig.Tally);
        GutWall.Current = rig.Wall;
        return rig;
    }

    private static UnitProfile Profile(UnitKind kind, float stressSense, bool apoptotic)
    {
        return new UnitProfile
        {
            Kind = kind,
            DisplayName = kind.ToString(),
            FineTilesPerTick = 0, // stand still: these tests are about contact, not search
            FootprintFineTiles = 3,
            MaxActiveChildren = 10,
            KillLimit = 999,
            ContactRadiusFineTiles = 3,
            StressSenseChancePerTick = stressSense,
            ApoptoticKill = apoptotic,
        };
    }

    private static SearchUnit UnitAt(Rig rig, UnitProfile profile, FineCoord at)
    {
        var go = new GameObject($"Sprint23_Unit_{profile.Kind}");
        go.AddComponent<SpriteRenderer>();
        var unit = go.AddComponent<SearchUnit>();
        rig.Junk.Add(go);
        unit.Initialize(rig.Board, rig.Grid, rig.Field, profile, at);
        return unit;
    }

    private static PathogenAgent InfectedAt(Rig rig, CoarseCoord cell, PathogenClass cls, float now)
    {
        var go = new GameObject($"Sprint23_Path_{cls}");
        go.AddComponent<SpriteRenderer>();
        var agent = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        // Use the production entry point rather than a test-only backdoor:
        // it places the agent AT the cell and settles it into the host
        // layer. Calling TryInfect on a freshly-Initialized agent infects
        // the grid but leaves the agent's own fine coordinate wherever
        // Initialize put it, so every contact test then silently misses --
        // which is exactly how the first run of this harness failed.
        agent.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                                       a => { }, (c, c2, free, t) => false,
                                       cell, cls, now);
        return agent;
    }

    private static FineCoord CentreOf(Rig rig, CoarseCoord c) => rig.Board.CoarseCenterFine(c);

    // =================================================================
    // 1. The two predicates are genuinely independent
    // =================================================================
    private static void RunPredicatesAreOrthogonal()
    {
        Debug.Log("[Sprint23Verification] --- Predicates ---");

        // The whole reason IsAdaptive was split: a CTL is adaptive AND
        // tissue-roaming, so one bool could not answer both questions.
        Check("CTL is knowledge-gated",
            BoneMarrowManager.IsKnowledgeGated(UnitKind.CytotoxicT));
        Check("CTL is NOT node-resident (it works in tissue)",
            !BoneMarrowManager.IsNodeResident(UnitKind.CytotoxicT));

        Check("Dendritic cell is node-resident",
            BoneMarrowManager.IsNodeResident(UnitKind.DendriticCell));
        Check("Dendritic cell is NOT knowledge-gated",
            !BoneMarrowManager.IsKnowledgeGated(UnitKind.DendriticCell));

        // gamma-delta is the case that lands on NEITHER: tissue-resident and
        // unprimed. GAME_DESIGN.md §4 calls it "the one T cell that can
        // exist on the board before the lymph node is online."
        Check("gamma-delta is neither node-resident nor gated",
            !BoneMarrowManager.IsNodeResident(UnitKind.GammaDeltaT)
            && !BoneMarrowManager.IsKnowledgeGated(UnitKind.GammaDeltaT));

        // NK is innate by the Director's 2026-10-25 ruling.
        Check("NK/ILC1 is ungated (innate, needs no priming)",
            !BoneMarrowManager.IsKnowledgeGated(UnitKind.NaturalKiller));
    }

    // =================================================================
    // 2. The silent-wrong-answer guard
    // =================================================================
    private static void RunEveryTissueKindHasItsOwnProfile()
    {
        Debug.Log("[Sprint23Verification] --- Per-kind profiles ---");

        var rig = BuildRig("Profiles");
        try
        {
            var go = new GameObject("Sprint23_Marrow");
            rig.Junk.Add(go);
            var manager = go.AddComponent<BoneMarrowManager>();

            var mac = Profile(UnitKind.Macrophage, 0.003f, false);
            var neu = Profile(UnitKind.Neutrophil, 0.002f, false);
            var ctl = Profile(UnitKind.CytotoxicT, 0.03f, true);
            var nk  = Profile(UnitKind.NaturalKiller, 0.01f, true);

            var positions = new[] { Vector3.zero, Vector3.right, Vector3.up };
            manager.Initialize(
                rig.Board, rig.Grid, rig.Field, mac, null, neu, null,
                positions, 1f, null, null,
                new[]
                {
                    new BoneMarrowManager.TissueKindRegistration { Profile = ctl },
                    new BoneMarrowManager.TissueKindRegistration { Profile = nk },
                });

            var registered = new List<UnitKind>(manager.RegisteredTissueKinds);
            Check("all four registered tissue kinds are present",
                registered.Contains(UnitKind.Macrophage) && registered.Contains(UnitKind.Neutrophil)
                && registered.Contains(UnitKind.CytotoxicT) && registered.Contains(UnitKind.NaturalKiller));

            // The real regression guard: a CTL tower must seed CTL numbers,
            // not whatever the old ternary's fallback branch returned.
            manager.PlaceTower(0, UnitKind.CytotoxicT);
            var tuning = manager.GetTuning(0);
            Check("a CTL tower seeds the CTL's ApoptoticKill, not the neutrophil's",
                tuning != null && tuning.ApoptoticKill);
            Check("a CTL tower seeds the CTL's stress-sense rate",
                tuning != null && Mathf.Approximately(tuning.StressSenseChancePerTick, 0.03f));

            manager.PlaceTower(1, UnitKind.NaturalKiller);
            var nkTuning = manager.GetTuning(1);
            Check("an NK tower seeds NK numbers, distinct from the CTL's",
                nkTuning != null && Mathf.Approximately(nkTuning.StressSenseChancePerTick, 0.01f));
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 3 + 4. Quiet vs loud -- the heart of the sprint
    // =================================================================
    private static void RunApoptoticKillIsCleanerNotGentler()
    {
        Debug.Log("[Sprint23Verification] --- Apoptotic kill ---");

        var rig = BuildRig("Apoptotic");
        try
        {
            var cell = new CoarseCoord(50, 20);
            rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
            InfectedAt(rig, cell, PathogenClass.IntracellularVirus, 0f);

            Check("precondition: the cell is infected",
                rig.Grid.GetHostState(cell) == HostState.Infected);

            var ctl = UnitAt(rig, Profile(UnitKind.CytotoxicT, 2f, true), CentreOf(rig, cell));

            bool fired = false;
            for (int i = 0; i < 20 && !fired; i++) fired = ctl.CheckStressSense(i * 0.12f);

            Check("the apoptotic kill fired", fired);

            // The correction: the infected cell DIES. A cytotoxic T cell
            // recognises and kills the infected cell -- it does not
            // sterilise one and walk away.
            Check("the infected cell dies -- the shell goes too",
                rig.Grid.GetHostState(cell) == HostState.Dead);
            Check("the pathogen dies with it",
                rig.Grid.GetIntracellularAt(cell) == null);

            // ...but cleanly. This fraction is the actual reward.
            float debris = rig.Grid.GetDebrisAmount(cell);
            Check("it leaves only a small tidy pile, not a necrotic spill",
                debris > 0f && debris < TissueGrid.FullDebris * 0.5f);

            // Antigen survives: apoptotic cells are the classic substrate
            // for cross-presentation, so precision does NOT cost the player
            // the thing they learn from.
            Check("antigen is still recorded for a dendritic cell to sample",
                rig.Grid.GetDebrisAntigen(cell) != null);
        }
        finally { rig.Dispose(); }
    }

    private static void RunNecroticKillLeavesAMess()
    {
        Debug.Log("[Sprint23Verification] --- Necrotic kill ---");

        var rig = BuildRig("Necrotic");
        try
        {
            var cell = new CoarseCoord(50, 20);
            rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
            InfectedAt(rig, cell, PathogenClass.IntracellularVirus, 0f);

            var mac = UnitAt(rig, Profile(UnitKind.Macrophage, 2f, false), CentreOf(rig, cell));

            bool fired = false;
            for (int i = 0; i < 20 && !fired; i++) fired = mac.CheckStressSense(i * 0.12f);

            Check("the necrotic kill fired", fired);
            Check("the host cell dies", rig.Grid.GetHostState(cell) == HostState.Dead);
            Check("and leaves a FULL debris pile",
                Mathf.Approximately(rig.Grid.GetDebrisAmount(cell), TissueGrid.FullDebris));

            // The comparison is the mechanic. If these two ever become equal
            // the whole case for cytotoxic towers is gone, silently.
            float necrotic = rig.Grid.GetDebrisAmount(cell);
            float apoptoticExpected = TissueGrid.FullDebris * TissueTuning.ApoptoticDebrisFraction;
            Check("an innate kill leaves meaningfully more mess than a cytotoxic one",
                necrotic > apoptoticExpected * 2f);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 5. The meter is cumulative, not a coin flip
    // =================================================================
    private static void RunStressMeterAccumulates()
    {
        Debug.Log("[Sprint23Verification] --- Stress meter ---");

        var rig = BuildRig("Meter");
        try
        {
            var cell = new CoarseCoord(50, 20);
            rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
            InfectedAt(rig, cell, PathogenClass.IntracellularVirus, 0f);

            // 0.1 per tick => exactly 10 ticks to clear, deterministically.
            var ctl = UnitAt(rig, Profile(UnitKind.CytotoxicT, 0.1f, true), CentreOf(rig, cell));

            Check("the meter starts empty", rig.Grid.GetInfectionStress(cell) <= 0f);

            for (int i = 0; i < 5; i++) ctl.CheckStressSense(i * 0.12f);
            float half = rig.Grid.GetInfectionStress(cell);
            Check("the meter is visibly part-filled after 5 of 10 ticks",
                half > 0.3f && half < 0.7f);
            Check("the cell is still infected mid-fill",
                rig.Grid.GetHostState(cell) == HostState.Infected);

            bool fired = false;
            for (int i = 5; i < 40 && !fired; i++) fired = ctl.CheckStressSense(i * 0.12f);
            Check("the meter fills and the kill fires", fired);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 6. Cooperation (Halle 2016)
    // =================================================================
    private static void RunCooperationIsSuperadditive()
    {
        Debug.Log("[Sprint23Verification] --- Cooperation ---");

        int SoloTicks()
        {
            var rig = BuildRig("Solo");
            try
            {
                var cell = new CoarseCoord(50, 20);
                rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
                InfectedAt(rig, cell, PathogenClass.IntracellularVirus, 0f);
                var u = UnitAt(rig, Profile(UnitKind.CytotoxicT, 0.05f, true), CentreOf(rig, cell));
                for (int i = 0; i < 400; i++)
                    if (u.CheckStressSense(i * 0.12f)) return i + 1;
                return int.MaxValue;
            }
            finally { rig.Dispose(); }
        }

        int TrioTicks()
        {
            var rig = BuildRig("Trio");
            try
            {
                var cell = new CoarseCoord(50, 20);
                rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
                InfectedAt(rig, cell, PathogenClass.IntracellularVirus, 0f);
                var a = UnitAt(rig, Profile(UnitKind.CytotoxicT, 0.05f, true), CentreOf(rig, cell));
                var b = UnitAt(rig, Profile(UnitKind.CytotoxicT, 0.05f, true), CentreOf(rig, cell));
                var c = UnitAt(rig, Profile(UnitKind.CytotoxicT, 0.05f, true), CentreOf(rig, cell));
                for (int i = 0; i < 400; i++)
                {
                    float t = i * 0.12f;
                    if (a.CheckStressSense(t) || b.CheckStressSense(t) || c.CheckStressSense(t)) return i + 1;
                }
                return int.MaxValue;
            }
            finally { rig.Dispose(); }
        }

        int solo = SoloTicks();
        int trio = TrioTicks();

        Check("a lone cytotoxic cell does clear an infection eventually", solo < int.MaxValue);
        Check("three together clear it faster than one", trio < solo);
        // Three attackers each at x3.5 is >3x one attacker's rate, so the
        // speed-up must beat simple addition.
        Check("and faster than three times one (superadditive, per Halle 2016)",
            trio * 3 < solo);
    }

    // =================================================================
    // 7 + 8. Knowledge licensing
    // =================================================================
    private static void RunUnlicensedTowerEmitsNothing()
    {
        Debug.Log("[Sprint23Verification] --- Licensing ---");

        var ledger = new KnowledgeLedger();
        KnowledgeLicence.Ledger = ledger;
        try
        {
            Check("a CTL tower is NOT licensed at 0% knowledge",
                !KnowledgeLicence.IsTowerLicensed(UnitKind.CytotoxicT));
            Check("an NK tower IS licensed at 0% (innate)",
                KnowledgeLicence.IsTowerLicensed(UnitKind.NaturalKiller));
            Check("a gamma-delta tower IS licensed at 0% (tissue-resident, unprimed)",
                KnowledgeLicence.IsTowerLicensed(UnitKind.GammaDeltaT));

            ledger.Add(PathogenClass.IntracellularVirus, 12f);
            Check("one species past 10% licenses the CTL tower",
                KnowledgeLicence.IsTowerLicensed(UnitKind.CytotoxicT));

            // The B cell waits on a higher rung than the CTL.
            Check("the B-cell tower is still dark at 12% (its rung is 20%)",
                !KnowledgeLicence.IsTowerLicensed(UnitKind.PlasmaCell));
            ledger.Add(PathogenClass.IntracellularVirus, 10f);
            Check("and licenses at 22%",
                KnowledgeLicence.IsTowerLicensed(UnitKind.PlasmaCell));
        }
        finally { KnowledgeLicence.ResetToDefaults(); }
    }

    private static void RunPerSpeciesRecognition()
    {
        Debug.Log("[Sprint23Verification] --- Per-species recognition ---");

        var rig = BuildRig("Species");
        var ledger = new KnowledgeLedger();
        KnowledgeLicence.Ledger = ledger;
        try
        {
            // The virus is known; the bacterium is not.
            ledger.Add(PathogenClass.IntracellularVirus, 40f);

            Check("a CTL recognises the characterised species",
                KnowledgeLicence.RecognisesSpecies(UnitKind.CytotoxicT, PathogenClass.IntracellularVirus));
            Check("a CTL is BLIND to an uncharacterised species (MHC-I restriction)",
                !KnowledgeLicence.RecognisesSpecies(UnitKind.CytotoxicT, PathogenClass.IntracellularBacterium));
            Check("an NK cell sees both -- it needs no antigen",
                KnowledgeLicence.RecognisesSpecies(UnitKind.NaturalKiller, PathogenClass.IntracellularBacterium));

            // And prove it in the simulation, not just the predicate.
            var cell = new CoarseCoord(50, 20);
            rig.Grid.SeedHostState(cell, HostState.Healthy, 0f);
            InfectedAt(rig, cell, PathogenClass.IntracellularBacterium, 0f);
            var ctl = UnitAt(rig, Profile(UnitKind.CytotoxicT, 2f, true), CentreOf(rig, cell));

            bool fired = false;
            for (int i = 0; i < 50 && !fired; i++) fired = ctl.CheckStressSense(i * 0.12f);
            Check("a CTL standing on an unknown species does nothing at all", !fired);
            Check("and builds no recognition pressure either",
                rig.Grid.GetInfectionStress(cell) <= 0f);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 9. Antibody
    // =================================================================
    private static void RunAntibodyCoats()
    {
        Debug.Log("[Sprint23Verification] --- Antibody ---");

        var rig = BuildRig("Antibody");
        try
        {
            // The plasma cell's profile must actually carry a secretion
            // interval, or the B-cell tower is an expensive ornament.
            var plasma = Profile(UnitKind.PlasmaCell, 0f, false);
            plasma.AntibodySecretionIntervalSeconds = 1.1f;
            var unit = UnitAt(rig, plasma, CentreOf(rig, new CoarseCoord(50, 20)));

            // No pool configured in the harness, so Secrete returns false --
            // what matters is that the plasma cell TRIES on schedule and
            // that nothing throws when antibodies are disabled.
            bool threw = false;
            try
            {
                for (int i = 0; i < 40; i++) unit.CheckAntibodySecretion(i * 0.12f);
            }
            catch (System.Exception) { threw = true; }
            Check("a plasma cell with no antibody pool degrades quietly", !threw);

            var nonSecretor = Profile(UnitKind.Macrophage, 0.003f, false);
            var mac = UnitAt(rig, nonSecretor, CentreOf(rig, new CoarseCoord(51, 20)));
            Check("a macrophage never secretes antibody (falls out with no kind check)",
                !mac.CheckAntibodySecretion(5f));

            // Coating itself is the IgA path, already proven in Sprint 22 --
            // check here only that tissue coating produces the same
            // observable, so the two are genuinely one mechanic.
            var go = new GameObject("Sprint23_CoatTarget");
            go.AddComponent<SpriteRenderer>();
            var target = go.AddComponent<PathogenAgent>();
            rig.Junk.Add(go);
            target.Initialize(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                              a => { }, (c, c2, free, t) => false,
                              classOverride: PathogenClass.LargeBacterium);
            Check("a fresh pathogen is uncoated", !target.IsCoated);
            target.SetCoated(true);
            Check("coating pacifies it (same field IgA uses)", target.VirulenceScale < 1f);
            Check("and slows it", target.MotilityScale < 1f);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 10. gamma-delta at the wall
    // =================================================================
    private static void RunGammaDeltaRepairsTheWall()
    {
        Debug.Log("[Sprint23Verification] --- gamma-delta IEL ---");

        var rig = BuildRig("GammaDelta");
        try
        {
            var gd = Profile(UnitKind.GammaDeltaT, 0.008f, true);
            gd.BarrierPatrolRadiusCells = 2f;
            gd.BarrierRepairIntervalSeconds = 1f;

            int lane = rig.Board.LumenEntryCrossIndex;
            float s = rig.Wall.SOfLane(lane);

            // Kill a stretch of barrier, then stand a gamma-delta cell on it.
            rig.Wall.DamageBarrier(s, 9999f, 0f);
            bool broken = rig.Wall.StateAt(s) != GutWall.BarrierState.Intact;
            Check("precondition: the barrier is broken", broken);

            // The lumen-edge TISSUE cell: one step base-ward of the first
            // lumen cell, which is where an intraepithelial cell lives.
            var coarse = rig.Board.CoarseFromAxis(rig.Board.LumenNearWallAxisIndex - 1, lane);
            var unit = UnitAt(rig, gd, CentreOf(rig, coarse));

            for (int i = 0; i < 40; i++) unit.CheckBarrierPatrol(i * 0.12f);

            Check("a gamma-delta IEL repairs the wall it stands on",
                rig.Wall.StateAt(s) == GutWall.BarrierState.Intact);

            // And a cell with no patrol radius must not touch the wall.
            var mac = Profile(UnitKind.Macrophage, 0.003f, false);
            var m = UnitAt(rig, mac, CentreOf(rig, coarse));
            Check("a macrophage does not patrol the barrier", !m.CheckBarrierPatrol(10f));
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 11. The NK cell's actual job
    // =================================================================
    private static void RunIfnGammaActivatesMacrophages()
    {
        Debug.Log("[Sprint23Verification] --- IFN-gamma aura ---");

        var rig = BuildRig("Ifn");
        try
        {
            var cell = new CoarseCoord(50, 20);

            var nkProfile = Profile(UnitKind.NaturalKiller, 0.01f, true);
            nkProfile.IfnGammaAuraChancePerTick = 1f; // deterministic for the test
            nkProfile.IfnGammaAuraCoarseRadius = 3;

            var mac = UnitAt(rig, Profile(UnitKind.Macrophage, 0.003f, false), CentreOf(rig, cell));
            var far = UnitAt(rig, Profile(UnitKind.Macrophage, 0.003f, false),
                             CentreOf(rig, new CoarseCoord(70, 20)));
            var nk = UnitAt(rig, nkProfile, CentreOf(rig, cell));

            Check("macrophages start unactivated", !mac.IsActivated(0f));

            bool any = nk.CheckIfnGammaAura(0f);
            Check("the NK/ILC1 pulse reaches a nearby macrophage", any && mac.IsActivated(0.1f));
            Check("but not one across the board", !far.IsActivated(0.1f));
            Check("activation lapses", !mac.IsActivated(1000f));

            // The NK cell is not the one being buffed -- it buffs others.
            Check("the NK cell does not activate itself", !nk.IsActivated(0.1f));

            var macProfileNoAura = Profile(UnitKind.Macrophage, 0.003f, false);
            var plainMac = UnitAt(rig, macProfileNoAura, CentreOf(rig, cell));
            Check("a macrophage emits no aura of its own", !plainMac.CheckIfnGammaAura(0f));
        }
        finally { rig.Dispose(); }
    }
}
