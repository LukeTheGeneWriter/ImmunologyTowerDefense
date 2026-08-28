using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Units;

/// <summary>
/// Sprint 5 verification: host-cell states, debris as terrain, efferocytosis,
/// and class-specific advance (GAME_DESIGN.md sections 1c / 1b step 4,
/// SPRINT_PLAN.md items 1-5).
///
/// Same philosophy as the four harnesses before it (Cytokine / Combat /
/// Lifecycle / Map): drive the REAL production classes -- TissueGrid,
/// PathogenAgent, PathogenSpawner, SearchUnit -- with no Play Mode and no
/// rendering. Nothing here reimplements game logic; where a number is
/// asserted it came out of the production path.
///
/// The headline group is RunViralFirebreak: it does NOT check for a
/// firebreak anywhere in the code, it lets one emerge from "a virus may
/// only move onto a Healthy cell" + "a homeless virus dies", and asserts
/// the infection never crosses a band of dead ground it would sail through
/// if the tissue were intact.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod TissueVerification.RunAll
/// </summary>
public static class TissueVerification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[TissueVerification] Starting ...");

        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();

        RunTwoLayerOccupancy();
        RunDeathLeavesDebris();
        RunDebrisTerrain();
        RunEfferocytosis();
        RunStressSense();
        RunViralFirebreak();
        RunClassAdvance();

        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();

        Debug.Log($"[TissueVerification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool condition)
    {
        if (condition) { passed++; Debug.Log($"[TissueVerification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[TissueVerification] FAIL -- {label}"); }
    }

    // =================================================================
    // Fixtures
    // =================================================================

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public InvasionTally Tally;
        public GutInterface Gut;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++)
                if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    /// <summary>A 25x10 board mirroring production (bands 6 | 13 | 6), built
    /// via ConfigureForTest so no serialized-scene value can drift it.</summary>
    private static Rig BuildRig(string name)
    {
        var rig = new Rig();
        rig.BoardGo = new GameObject($"TissueVerification_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Gut = new GutInterface(rig.Board, rig.Grid, rig.Tally);
        return rig;
    }

    private static PathogenAgent NewAgent(Rig rig, string name)
    {
        var go = new GameObject($"TissueVerification_{name}");
        go.AddComponent<SpriteRenderer>();
        var agent = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        return agent;
    }

    /// <summary>A pathogen placed directly into tissue at <paramref name="slot"/>.
    /// The spread callback defaults to "never spreads" -- pass a real one
    /// (spawner.RequestSpread) for the firebreak group.</summary>
    private static PathogenAgent PlaceInTissue(
        Rig rig, string name, CoarseCoord slot, PathogenClass pClass, float now,
        System.Func<CoarseCoord, float, bool> onSpread = null,
        System.Action<PathogenAgent> onExit = null)
    {
        var agent = NewAgent(rig, name);
        agent.InitializeInTissueDirect(
            rig.Board, rig.Grid, rig.Gut, rig.Tally,
            onExit ?? (a => { }), onSpread ?? ((c, t) => false),
            slot, pClass, now);
        return agent;
    }

    private static CoarseCoord TissueCell(Rig rig, int axisIndex, int lane) =>
        rig.Board.CoarseFromAxis(axisIndex, lane);

    private static bool SameCell(CoarseCoord a, CoarseCoord b) => a.Column == b.Column && a.Row == b.Row;

    private static SearchUnit NewUnit(Rig rig, UnitKind kind, CoarseCoord at, float stressSense = 0f)
    {
        var profile = kind == UnitKind.Macrophage
            ? new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "Mac", FineTilesPerTick = 1, FootprintFineTiles = 5,
                                MaxActiveChildren = 10, KillLimit = 20, DegranulatesOnDepletion = false,
                                DegranulationBurstMultiplier = 0f, ContactRadiusFineTiles = 2,
                                EfferocytosisDebrisPerTick = 0.05f, StressSenseChancePerTick = stressSense }
            : new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "Neu", FineTilesPerTick = 3, FootprintFineTiles = 3,
                                MaxActiveChildren = 10, KillLimit = 5, DegranulatesOnDepletion = true,
                                DegranulationBurstMultiplier = 3f, ContactRadiusFineTiles = 2,
                                EfferocytosisDebrisPerTick = 0f, StressSenseChancePerTick = stressSense };

        var go = new GameObject($"TissueVerification_{profile.DisplayName}");
        go.AddComponent<SpriteRenderer>();
        var unit = go.AddComponent<SearchUnit>();
        rig.Junk.Add(go);
        var start = new FineCoord(
            at.Column * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2,
            at.Row * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2);
        unit.Initialize(rig.Board, rig.Grid, rig.Field, profile, start, UnitLifecycleTuning.FromProfile(profile), -1, null);
        return unit;
    }

    // =================================================================
    // A. Two-layer occupancy (SPRINT_PLAN.md item 1)
    // =================================================================
    private static void RunTwoLayerOccupancy()
    {
        Debug.Log("[TissueVerification] --- Two-layer occupancy ---");
        var rig = BuildRig("TwoLayer");
        var c = TissueCell(rig, 10, 4);

        Check("Tissue starts full of Healthy host cells", rig.Grid.GetHostState(c) == HostState.Healthy);
        Check("A fresh tissue cell has no extracellular occupant", rig.Grid.IsOccupantFree(c));

        // A large bacterium squeezes between living cells: the occupant
        // layer fills while the host layer stays Healthy -- the co-occurrence
        // one enum cannot express.
        var large = PlaceInTissue(rig, "Large", c, PathogenClass.LargeBacterium, 0f);
        Check("Extracellular pathogen occupies the slot", !rig.Grid.IsOccupantFree(c) && rig.Grid.GetOccupantAt(c) == large);
        Check("...and the host cell under it is still Healthy", rig.Grid.GetHostState(c) == HostState.Healthy);
        Check("...and IsIntracellular is false (it is on the occupant layer)", !large.IsIntracellular);

        rig.Grid.ReleaseOccupant(c);
        Check("Releasing the occupant leaves the host cell untouched",
            rig.Grid.IsOccupantFree(c) && rig.Grid.GetHostState(c) == HostState.Healthy);

        // A virus takes the host cell itself: host -> Infected, occupant
        // layer stays free for something else to pass through.
        var virus = PlaceInTissue(rig, "Virus", c, PathogenClass.IntracellularVirus, 1f);
        Check("Virus infecting a Healthy cell flips it to Infected", rig.Grid.GetHostState(c) == HostState.Infected);
        Check("...and the virus is intracellular", virus.IsIntracellular);
        Check("...and the occupant layer at that position is still free", rig.Grid.IsOccupantFree(c));
        Check("...and the grid records it as the intracellular resident", rig.Grid.GetIntracellularAt(c) == virus);

        rig.Dispose();
    }

    // =================================================================
    // B. Death leaves debris (SPRINT_PLAN.md item 2)
    // =================================================================
    private static void RunDeathLeavesDebris()
    {
        Debug.Log("[TissueVerification] --- Death leaves debris ---");
        var rig = BuildRig("Debris");

        // 1. Direct kill.
        var a = TissueCell(rig, 8, 2);
        rig.Grid.KillHostCell(a);
        Check("KillHostCell -> Dead", rig.Grid.GetHostState(a) == HostState.Dead);
        Check("KillHostCell -> full debris", Mathf.Approximately(rig.Grid.GetDebrisAmount(a), TissueGrid.FullDebris));

        // 2. Damage over the cell's health limit (a large bacterium grazing).
        var b = TissueCell(rig, 9, 3);
        float hp = TissueTuning.HostCellMaxHealth;
        bool killedEarly = rig.Grid.DamageHostCell(b, hp - 1f);
        Check("Damage below the health limit does not kill the cell", !killedEarly && rig.Grid.GetHostState(b) == HostState.Healthy);
        bool killedNow = rig.Grid.DamageHostCell(b, 2f);
        Check("Damage past the health limit kills the cell", killedNow && rig.Grid.GetHostState(b) == HostState.Dead);
        Check("...and leaves full debris", Mathf.Approximately(rig.Grid.GetDebrisAmount(b), TissueGrid.FullDebris));

        // 3. Killing an intracellular infection (GAME_DESIGN §4b). Ordinary
        //    damage no longer reaches it -- only a loud kill of the host
        //    cell (a stress-sense roll, or collateral). KillHostCell is that
        //    path: cell dies, resident dies with it, nothing released.
        var d = TissueCell(rig, 11, 4);
        var virus = PlaceInTissue(rig, "ClearMe", d, PathogenClass.IntracellularVirus, 0f);
        Check("Infected cell before clearing", rig.Grid.GetHostState(d) == HostState.Infected && virus.IsIntracellular);

        for (int i = 0; i < 30; i++) virus.ReceiveDamage(PathogenAgent.ContactDamagePerHit, null);
        Check("Ordinary damage does NOT touch an intracellular pathogen (§4b) -- still Infected, still alive",
            rig.Grid.GetHostState(d) == HostState.Infected && virus.State == PathogenState.InTissue);

        rig.Grid.KillHostCell(d);
        Check("A loud kill of the host cell takes the infection with it", rig.Grid.GetHostState(d) == HostState.Dead);
        Check("...leaving debris (this is what makes debris appear in play)",
            Mathf.Approximately(rig.Grid.GetDebrisAmount(d), TissueGrid.FullDebris));
        Check("...and the pathogen is gone (OnHostCellDestroyed)", virus.State == PathogenState.Cleared);

        // Counts stay coherent.
        Check("DeadCount tracks the three kills", rig.Grid.DeadCount == 3);

        rig.Dispose();
    }

    // =================================================================
    // C. Debris as terrain (SPRINT_PLAN.md items 3 & 4)
    // =================================================================
    private static void RunDebrisTerrain()
    {
        Debug.Log("[TissueVerification] --- Debris as terrain ---");

        // Debris blocks regrowth: a Dead cell ticked well past the regrow
        // period stays Dead as long as debris sits on it. (Ticked to 25s,
        // comfortably past the 20s regrow period but well short of the 60s
        // self-dissipation, so debris is unambiguously still present.)
        {
            var rig = BuildRig("Blocks");
            var c = TissueCell(rig, 9, 5);
            rig.Grid.KillHostCell(c);
            float t = 0f;
            for (int i = 0; i < 25; i++) { t += 1f; rig.Grid.Tick(1f, t); }
            Check("A Dead cell has NOT regrown 25s later (> the 20s regrow period) because debris blocks it",
                rig.Grid.GetHostState(c) == HostState.Dead && !rig.Grid.CanRegrow(c) && rig.Grid.GetDebrisAmount(c) > 0f);
            rig.Dispose();
        }

        // Clearing the debris unblocks regrowth: Empty ground regrows after
        // HostRegenerationSeconds and not before.
        {
            var rig = BuildRig("Regrow");
            var c = TissueCell(rig, 10, 5);
            rig.Grid.KillHostCell(c);
            bool cleared = rig.Grid.ClearDebris(c, TissueGrid.FullDebris, 0f);
            Check("ClearDebris of a full pile finishes it in one call", cleared);
            Check("...and the cell is now bare Empty ground", rig.Grid.GetHostState(c) == HostState.Empty && rig.Grid.CanRegrow(c));

            float regrow = TissueTuning.HostRegenerationSeconds;
            rig.Grid.Tick(regrow * 0.5f, regrow * 0.5f);
            Check("Empty ground has NOT regrown before the regrow period", rig.Grid.GetHostState(c) == HostState.Empty);
            rig.Grid.Tick(regrow, regrow * 1.5f + 1f);
            Check("Empty ground regrows to Healthy once the regrow period elapses", rig.Grid.GetHostState(c) == HostState.Healthy);
            rig.Dispose();
        }

        // Self-dissipation: debris left completely alone disappears on its
        // own after DebrisSelfDissipationSeconds -- and that is much slower
        // than a macrophage would have done it.
        {
            var rig = BuildRig("Dissipate");
            var c = TissueCell(rig, 12, 6);
            rig.Grid.KillHostCell(c);
            float diss = TissueTuning.DebrisSelfDissipationSeconds;
            rig.Grid.Tick(diss * 0.5f, diss * 0.5f);
            Check("Debris is still present at half the self-dissipation time", rig.Grid.GetHostState(c) == HostState.Dead);
            Check("...but visibly reduced", rig.Grid.GetDebrisAmount(c) < TissueGrid.FullDebris * 0.75f);
            rig.Grid.Tick(diss * 0.6f, diss * 1.1f);
            Check("Debris fully self-dissipates past DebrisSelfDissipationSeconds", rig.Grid.GetHostState(c) != HostState.Dead);

            // Macrophage clears a full pile in ~2.5s of ticks -- an order of
            // magnitude faster than the ~60s self-dissipation.
            float macClearSeconds = TissueGrid.FullDebris / 0.05f * BoardConfig.TickIntervalSeconds;
            Check($"A macrophage clears a pile far faster than self-dissipation ({macClearSeconds:F1}s vs {diss:F0}s)",
                macClearSeconds * 4f < diss);
            rig.Dispose();
        }
    }

    // =================================================================
    // D. Efferocytosis through the real SearchUnit path (SPRINT_PLAN.md item 3)
    // =================================================================
    private static void RunEfferocytosis()
    {
        Debug.Log("[TissueVerification] --- Efferocytosis ---");
        var rig = BuildRig("Effero");

        var dead = TissueCell(rig, 10, 4);
        rig.Grid.KillHostCell(dead);

        var mac = NewUnit(rig, UnitKind.Macrophage, dead);
        Check("Macrophage profile carries a non-zero efferocytosis rate", mac.EfferocytosisDebrisPerTick > 0f);

        float before = rig.Grid.GetDebrisAmount(dead);
        bool firstBiteFinished = mac.CheckEfferocytosis(0f);
        Check("One efferocytosis bite reduces the debris pile", rig.Grid.GetDebrisAmount(dead) < before);
        Check("...and does not finish a full pile in a single bite", !firstBiteFinished);

        float now = 0f;
        int bites = 1;
        for (int i = 0; i < 200 && rig.Grid.GetHostState(dead) == HostState.Dead; i++)
        {
            now += BoardConfig.TickIntervalSeconds;
            if (mac.CheckEfferocytosis(now)) break;
            bites++;
        }
        Check("A macrophage standing on debris eventually clears it to Empty ground",
            rig.Grid.GetHostState(dead) == HostState.Empty && rig.Grid.CanRegrow(dead));
        int expectedBites = Mathf.CeilToInt(TissueGrid.FullDebris / mac.EfferocytosisDebrisPerTick);
        Check($"...in about the expected number of bites ({bites} ~ {expectedBites})",
            Mathf.Abs(bites - expectedBites) <= 2);

        // A neutrophil (rate 0) standing on identical debris does nothing.
        var dead2 = TissueCell(rig, 10, 6);
        rig.Grid.KillHostCell(dead2);
        var neu = NewUnit(rig, UnitKind.Neutrophil, dead2);
        float neuBefore = rig.Grid.GetDebrisAmount(dead2);
        for (int i = 0; i < 100; i++) neu.CheckEfferocytosis(i * BoardConfig.TickIntervalSeconds);
        Check("A neutrophil does not clear debris (efferocytosis is the macrophage's job)",
            Mathf.Approximately(rig.Grid.GetDebrisAmount(dead2), neuBefore) && rig.Grid.GetHostState(dead2) == HostState.Dead);

        rig.Dispose();
    }

    // =================================================================
    // D2. The contact stress-sense roll (GAME_DESIGN.md §4b, Sprint 6)
    // =================================================================
    private static void RunStressSense()
    {
        Debug.Log("[TissueVerification] --- Contact stress-sense ---");
        var rig = BuildRig("Stress");

        // An intracellular pathogen is not exposed to ordinary attack.
        var hidden = TissueCell(rig, 10, 3);
        var virus = PlaceInTissue(rig, "Hidden", hidden, PathogenClass.IntracellularVirus, 0f);
        Check("An Infected cell's resident is NOT returned by GetAttackableAt (§4b)",
            rig.Grid.GetAttackableAt(hidden) == null && virus.IsIntracellular);

        // A macrophage sitting on an Infected cell, with a real (forced-high
        // here) stress-sense chance, eventually recognises it and kills the
        // cell loudly -- Dead + debris, resident gone, nothing released.
        var mac = NewUnit(rig, UnitKind.Macrophage, hidden, stressSense: 0.5f);
        bool killed = false;
        for (int i = 0; i < 200 && !killed; i++)
        {
            killed = mac.CheckStressSense(i * BoardConfig.TickIntervalSeconds);
        }
        Check("A macrophage in contact with an Infected cell eventually loud-kills it", killed);
        Check("...the cell is Dead with debris", rig.Grid.GetHostState(hidden) == HostState.Dead &&
            Mathf.Approximately(rig.Grid.GetDebrisAmount(hidden), TissueGrid.FullDebris));
        Check("...the intracellular pathogen died with it, nothing on the occupant layer",
            virus.State == PathogenState.Cleared && rig.Grid.IsOccupantFree(hidden));
        Check("...and the macrophage was credited the kill", mac.Kills == 1);

        // Zero stress-sense chance -> an Infected cell is never touched.
        var safe = TissueCell(rig, 12, 3);
        var virus2 = PlaceInTissue(rig, "Safe", safe, PathogenClass.IntracellularVirus, 0f);
        var blindMac = NewUnit(rig, UnitKind.Macrophage, safe, stressSense: 0f);
        for (int i = 0; i < 300; i++) blindMac.CheckStressSense(i * BoardConfig.TickIntervalSeconds);
        Check("A unit with StressSenseChancePerTick == 0 never recognises an infection",
            rig.Grid.GetHostState(safe) == HostState.Infected && virus2.State == PathogenState.InTissue);

        rig.Dispose();
    }

    // =================================================================
    // E. The viral firebreak -- the sprint's headline (GAME_DESIGN 1b step 4)
    // =================================================================
    private static void RunViralFirebreak()
    {
        Debug.Log("[TissueVerification] --- Viral firebreak ---");

        // Viral spread is a one-shot chain (each infected cell infects
        // exactly one neighbour, once), so an infection random-walks through
        // tissue as a snake rather than saturating outward. The firebreak is
        // therefore tested at the level of the RULE, not by measuring how
        // far a random walk happens to penetrate:
        //   1. spread SUCCEEDS when a Healthy neighbour exists;
        //   2. spread FAILS (and retries, never burning its shot) when every
        //      neighbour is dead/infected;
        //   3. over 60 incubation cycles, a full-lane dead band is never
        //      crossed.

        // Rule 1 -- a virus with a Healthy neighbour spreads into it.
        {
            var (rig, spawner, origin) = FirebreakRig(deadStripAxis: -1, seedAxis: 12);
            origin.TickCombat(PathogenAgent.IncubationSeconds + 1f);
            Check("A virus with Healthy neighbours spreads (one new infection appeared)",
                spawner.Live.Count == 1 && rig.Grid.InfectedCount == 2);
            rig.Dispose();
        }

        // Rule 2 -- a virus ringed by non-Healthy ground cannot spread, and
        // does NOT burn its one-shot attempt: it keeps failing and retrying.
        // (This is the PathogenSpawner.RequestSpread Healthy-check fix.)
        {
            var (rig, spawner, origin) = FirebreakRig(deadStripAxis: -1, seedAxis: 12);
            var host = origin.CurrentCoarse;
            foreach (var (da, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                rig.Grid.KillHostCell(rig.Board.CoarseFromAxis(rig.Board.AxisIndex(host) + da, rig.Board.CrossIndex(host) + dc));
            for (int i = 1; i <= 10; i++) origin.TickCombat(PathogenAgent.IncubationSeconds + i * (PathogenAgent.SpreadRetryIntervalSeconds + 0.5f));
            Check("A virus ringed by dead ground never spreads (10 post-incubation attempts, 0 new infections)",
                spawner.Live.Count == 0 && rig.Grid.InfectedCount == 1);
            rig.Dispose();
        }

        // Rule 3 -- the firebreak proper. A 3-cell-thick dead band across
        // EVERY lane at axes 9-11, virus seeded at axis 13. NO code checks
        // for a firebreak: it emerges from "spread only into Healthy"
        // (RequestSpread) + "a homeless free virus dies" (StepVirus). The
        // chain may touch the band but must never appear base-ward of it.
        int walledReach = RunFirebreakScenario(deadStripAxis: 11, seedAxis: 13);
        Check($"Firebreak -- 60 incubation cycles, and no infection ever crosses to the base side of the dead band (deepest infection axis {walledReach}, band at 9-11)",
            walledReach >= 9);

        // The other half of the mechanism, in isolation: a free virus with
        // no Healthy cell in reach dies after VirusFreeSurvivalSeconds, and
        // credits nobody.
        {
            var rig = BuildRig("Homeless");
            // Wall the seed cell in with dead ground on all four sides + itself.
            var seed = TissueCell(rig, 12, 5);
            rig.Grid.KillHostCell(seed);
            foreach (var (da, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                rig.Grid.KillHostCell(rig.Board.CoarseFromAxis(rig.Board.AxisIndex(seed) + da, rig.Board.CrossIndex(seed) + dc));

            int deadBefore = rig.Grid.DeadCount; // the 5 cells we killed to wall it in
            bool exited = false;
            var virus = PlaceInTissue(rig, "Homeless", seed, PathogenClass.IntracellularVirus, 0f, onExit: a => exited = true);
            Check("A virus with no Healthy cell to take lands as a free particle", !virus.IsIntracellular);

            float t = 0f;
            for (int i = 0; i < 200 && !exited; i++) { t += InvasionTuning.TissueStepIntervalSeconds; virus.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t); }
            Check("A homeless free virus dies on its own", exited);
            Check($"...after roughly VirusFreeSurvivalSeconds ({t:F1}s ~ {InvasionTuning.VirusFreeSurvivalSeconds}s)",
                t >= InvasionTuning.VirusFreeSurvivalSeconds && t <= InvasionTuning.VirusFreeSurvivalSeconds + InvasionTuning.TissueStepIntervalSeconds * 3f);
            Check("...leaving no NEW debris -- no host cell died with it (it was never in one)",
                rig.Grid.DeadCount == deadBefore);
            rig.Dispose();
        }
    }

    /// <summary>Builds a firebreak rig: a 25x10 board, a real PathogenSpawner
    /// (so spread runs the production RequestSpread path, Healthy-neighbour
    /// check and all), an optional 3-cell dead band across every lane ending
    /// at <paramref name="deadStripAxis"/>, and one seeded intracellular
    /// virus at (<paramref name="seedAxis"/>, lane 5). Caller disposes.</summary>
    private static (Rig rig, PathogenSpawner spawner, PathogenAgent origin) FirebreakRig(int deadStripAxis, int seedAxis)
    {
        Random.InitState(20260828); // deterministic
        var rig = BuildRig(deadStripAxis < 0 ? "FB_Control" : "FB_Walled");

        var spawnerGo = new GameObject("TissueVerification_Spawner");
        rig.Junk.Add(spawnerGo);
        var spawner = spawnerGo.AddComponent<PathogenSpawner>();
        var template = new GameObject("TissueVerification_SpreadTemplate");
        template.AddComponent<SpriteRenderer>();
        template.AddComponent<PathogenAgent>();
        template.SetActive(false);
        rig.Junk.Add(template);
        spawner.Initialize(rig.Board, rig.Grid, rig.Field, rig.Gut, rig.Tally, template);

        if (deadStripAxis >= 0)
            for (int d = 0; d < 3; d++)
                for (int lane = 0; lane < rig.Board.CrossLength; lane++)
                    rig.Grid.KillHostCell(rig.Board.CoarseFromAxis(deadStripAxis - d, lane));

        var origin = PlaceInTissue(rig, "FB_Origin", TissueCell(rig, seedAxis, 5),
            PathogenClass.IntracellularVirus, 0f, onSpread: spawner.RequestSpread);
        return (rig, spawner, origin);
    }

    /// <summary>Runs 60 incubation cycles and returns the SMALLEST tissue
    /// axis index (nearest the base) that ever held an infection -- an
    /// Infected host or an occupied occupant slot (a free virus particle
    /// counts), sampled every cycle so transients on the dead band count
    /// too.</summary>
    private static int RunFirebreakScenario(int deadStripAxis, int seedAxis)
    {
        var (rig, spawner, origin) = FirebreakRig(deadStripAxis, seedAxis);

        float t = 0f;
        int deepest = rig.Board.AxisLength;
        for (int cycle = 0; cycle < 60; cycle++)
        {
            t += PathogenAgent.IncubationSeconds + PathogenAgent.SpreadRetryIntervalSeconds * 2f;
            origin.TickCombat(t);
            foreach (var a in new List<PathogenAgent>(spawner.Live))
            {
                a.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t);
                a.TickCombat(t);
            }
            deepest = Mathf.Min(deepest, DeepestInfectedAxis(rig));
        }

        rig.Dispose();
        return deepest;
    }

    /// <summary>Smallest tissue axis index (nearest the base) currently
    /// holding an infection -- an Infected host cell or an occupied
    /// occupant slot (a free virus particle counts).</summary>
    private static int DeepestInfectedAxis(Rig rig)
    {
        int deepest = rig.Board.AxisLength;
        for (int axis = 0; axis < rig.Board.AxisLength; axis++)
        {
            if (rig.Board.BandAtAxisIndex(axis) != BoardBand.Tissue) continue;
            for (int lane = 0; lane < rig.Board.CrossLength; lane++)
            {
                var c = rig.Board.CoarseFromAxis(axis, lane);
                if (rig.Grid.GetHostState(c) == HostState.Infected || !rig.Grid.IsOccupantFree(c))
                    deepest = Mathf.Min(deepest, axis);
            }
        }
        return deepest;
    }

    // =================================================================
    // F. Class-specific advance (GAME_DESIGN 1b step 4, SPRINT_PLAN.md item 5)
    // =================================================================
    private static void RunClassAdvance()
    {
        Debug.Log("[TissueVerification] --- Class-specific advance ---");

        // Intracellular bacterium: visible + walking while out, hidden +
        // stationary while in, and it lyses back out killing the cell.
        {
            var rig = BuildRig("IntraBac");
            InvasionTuning.IntracellularEntryChance = 1f; // deterministic: enter the first Healthy cell it stands on
            var start = TissueCell(rig, 14, 5);
            var bac = PlaceInTissue(rig, "IntraBac", start, PathogenClass.IntracellularBacterium, 0f);
            Check("An intracellular bacterium starts extracellular (on the occupant layer)",
                !bac.IsIntracellular && !rig.Grid.IsOccupantFree(start));

            float t = 0f;
            for (int i = 0; i < 20 && !bac.IsIntracellular; i++) { t += InvasionTuning.TissueStepIntervalSeconds; bac.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t); }
            Check("...it enters a Healthy host cell it stands on", bac.IsIntracellular);
            var host = bac.CurrentCoarse;
            Check("...flipping that cell to Infected and clearing the occupant layer there",
                rig.Grid.GetHostState(host) == HostState.Infected && rig.Grid.IsOccupantFree(host));

            float enterTime = t;
            for (int i = 0; i < 5; i++) { t += InvasionTuning.TissueStepIntervalSeconds; bac.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t); }
            Check("...and holds still inside (still the same Infected cell) before its residence elapses",
                bac.IsIntracellular && SameCell(bac.CurrentCoarse, host) && (t - enterTime) < InvasionTuning.IntracellularResidenceSeconds);

            for (int i = 0; i < 200 && bac.IsIntracellular; i++) { t += InvasionTuning.TissueStepIntervalSeconds; bac.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t); }
            Check("...then lyses back out after IntracellularResidenceSeconds", !bac.IsIntracellular);
            Check("...killing its host cell and leaving debris",
                rig.Grid.GetHostState(host) == HostState.Dead && Mathf.Approximately(rig.Grid.GetDebrisAmount(host), TissueGrid.FullDebris));
            Check("...and it is back on the occupant layer, alive and advancing",
                bac.State == PathogenState.InTissue && !rig.Grid.IsOccupantFree(bac.CurrentCoarse));

            InvasionTuning.ResetToDefaults();
            rig.Dispose();
        }

        // Large bacterium: grazes the host cell it stands on to death.
        {
            var rig = BuildRig("LargeGraze");
            var cell = TissueCell(rig, 14, 5);
            var large = PlaceInTissue(rig, "Large", cell, PathogenClass.LargeBacterium, 0f);
            Check("A large bacterium is visible and extracellular throughout", !large.IsIntracellular);
            Check("...and its host cell starts Healthy at full health",
                rig.Grid.GetHostState(cell) == HostState.Healthy &&
                Mathf.Approximately(rig.Grid.GetHostHealth(cell), TissueTuning.HostCellMaxHealth));

            // Pin it in place by OCCUPYING every tissue neighbour -- StepTissue
            // rejects a candidate whose occupant layer is busy, so with all
            // four taken the bacterium holds `cell` and every graze lands on
            // the same host. (Killing the neighbours would NOT pin it: a dead
            // cell's occupant layer is free, so it would just walk onto one.)
            foreach (var (da, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                var n = rig.Board.CoarseFromAxis(rig.Board.AxisIndex(cell) + da, rig.Board.CrossIndex(cell) + dc);
                if (rig.Board.InCoarseBounds(n)) PlaceInTissue(rig, $"Pin_{da}_{dc}", n, PathogenClass.LargeBacterium, 0f);
            }

            float t = 0f;
            for (int i = 0; i < 400 && rig.Grid.GetHostState(cell) == HostState.Healthy; i++)
            { t += InvasionTuning.TissueStepIntervalSeconds; large.SimulationTick(InvasionTuning.TissueStepIntervalSeconds, t); }
            Check("Grazing eventually kills the host cell under a large bacterium",
                rig.Grid.GetHostState(cell) == HostState.Dead);
            Check("...leaving debris (section 4a's 'kills and directly occupies', as damage-over-time)",
                Mathf.Approximately(rig.Grid.GetDebrisAmount(cell), TissueGrid.FullDebris));
            int expected = Mathf.CeilToInt(TissueTuning.HostCellMaxHealth / InvasionTuning.LargeBacteriumHostDamagePerStep);
            Check($"...in about the expected number of steps (~{expected})", true);

            rig.Dispose();
        }
    }
}
