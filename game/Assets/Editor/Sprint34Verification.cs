using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;

/// <summary>
/// Sprint 34 -- **the rotavirus corrections**, from
/// `docs/research/pathogens_viruses_02.md`.
///
/// Two facts about the organism the game has shipped since Sprint 24 turned
/// out to be wrong, and both were wrong in the same direction: we had been
/// playing rotavirus as an ordinary lytic virus arriving at an ordinary dose.
///
/// 1. **It leaves the cell non-lytically** -- ~100% of progeny released with
///    no measured permeability change at 72 h -- so an ignored focus does not
///    burst and resolve. It is still self-limiting, but by the crypt repaving
///    under its villus-tip tropism, not by §4b's burn-out.
/// 2. **It arrives in vesicles** -- 10-45% of stool virus is membrane-bound,
///    >10 virions per vesicle, and stripping the vesicles cuts infectivity
///    72-83%. One arriving unit is already a dose, which is the per-species
///    "minimum infectious dose" knob §1b Step 2 promised its floor would
///    become.
///
/// Fixing (1) exposed a third thing: §4b says a virus's spread mode is
/// "per-species", and `AssignVirusTraits` chose it with a **coin flip**,
/// admitting in a comment that "there is no species roster yet". There has
/// been one since Sprint 24. That is the seventh instance of this project's
/// standing pattern -- a value written on one side of a boundary and never
/// read as that value on the other.
///
/// Every check below asserts the PROPERTY (this organism does not burn out;
/// this crowd charges and that one cannot) rather than a proxy (the field is
/// set), which is the habit `AGENT_HANDBOOK.md` asks for.
/// </summary>
public static class Sprint34Verification
{
    private static int checks;
    private static int failures;

    public static void RunAll()
    {
        checks = 0; failures = 0;
        Debug.Log("[Sprint34Verification] === Sprint 34: the rotavirus corrections ===");

        SpeciesRoster.ResetToDefaults();
        InvasionTuning.ResetToDefaults();

        RunSpreadMode();
        RunNonLyticExit();
        RunVesicleDose();

        Debug.Log($"[Sprint34Verification] === {checks} checks, {failures} failures ===");
        if (failures > 0) Debug.LogError($"[Sprint34Verification] {failures} FAILURES");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint34Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint34Verification] FAIL -- {label}"); }
    }

    // =================================================================
    // A. The spread mode belongs to the roster, not to a coin flip
    // =================================================================
    private static void RunSpreadMode()
    {
        Debug.Log("[Sprint34Verification] --- A. Spread mode is per-species ---");

        Check("Rotavirus is declared a budding species",
              SpeciesRoster.Get(PathogenSpecies.Rotavirus).Exit == ViralExit.Budding);
        Check("The generic virus leaves its exit UNSPECIFIED (back-compat)",
              SpeciesRoster.Get(PathogenSpecies.GenericVirus).Exit == ViralExit.Unspecified);

        // The property: EVERY rotavirus buds, not about half of them.
        Random.InitState(34_001);
        int rotaBuds = 0, n = 300;
        var junk = new List<GameObject>();
        var boardGo = new GameObject("Sprint34_BoardA");
        var board = boardGo.AddComponent<BoardConfig>();
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);

        for (int i = 0; i < n; i++)
        {
            var a = NewAgent(junk, board, grid, wall, tally, $"Rota_{i}",
                             PathogenSpecies.Rotavirus);
            if (a.VirusBuds) rotaBuds++;
        }
        Check($"All {n} rotavirus spawns bud ({rotaBuds}/{n})", rotaBuds == n);

        // Back-compat: the generic virus still rolls the old coin, so the
        // three generics behave exactly as they did before Sprint 34.
        int genBuds = 0;
        for (int i = 0; i < n; i++)
        {
            var a = NewAgent(junk, board, grid, wall, tally, $"Gen_{i}",
                             PathogenSpecies.GenericVirus);
            if (a.VirusBuds) genBuds++;
        }
        float genRate = genBuds / (float)n;
        Check($"The generic virus still splits about evenly ({genBuds}/{n} = {genRate:P0})",
              genRate > 0.38f && genRate < 0.62f);

        // A bacterium is not a virus and never buds, whatever the roster says.
        int bactBuds = 0;
        for (int i = 0; i < 40; i++)
        {
            var a = NewAgent(junk, board, grid, wall, tally, $"Bact_{i}",
                             PathogenSpecies.GenericLargeBacterium);
            if (a.VirusBuds) bactBuds++;
        }
        Check($"No bacterium ever buds ({bactBuds}/40)", bactBuds == 0);

        // Determinism: the same seed gives the same answer, twice.
        int first = CountRotaBuds(junk, board, grid, wall, tally, 34_002, 60);
        int second = CountRotaBuds(junk, board, grid, wall, tally, 34_002, 60);
        Check($"Two identical runs agree ({first} == {second})", first == second);

        Cleanup(junk, boardGo);
    }

    private static int CountRotaBuds(List<GameObject> junk, BoardConfig board,
        TissueGrid grid, GutWall wall, InvasionTally tally, int seed, int n)
    {
        Random.InitState(seed);
        int buds = 0;
        for (int i = 0; i < n; i++)
        {
            var a = NewAgent(junk, board, grid, wall, tally, $"Det_{seed}_{i}",
                             PathogenSpecies.Rotavirus);
            if (a.VirusBuds) buds++;
        }
        return buds;
    }

    // =================================================================
    // B. A non-lytic releaser does not burn out
    // =================================================================
    private static void RunNonLyticExit()
    {
        Debug.Log("[Sprint34Verification] --- B. Non-lytic exit ---");

        Check("Rotavirus releases non-lytically (fraction 1)",
              Mathf.Approximately(
                  SpeciesRoster.Get(PathogenSpecies.Rotavirus).NonLyticReleaseFraction, 1f));
        Check("The generic virus is ordinary and lytic (fraction 0)",
              SpeciesRoster.Get(PathogenSpecies.GenericVirus).NonLyticReleaseFraction <= 0f);

        // The §4b burn-out roll happens on the first tick past establishment,
        // BEFORE the incubation gate, so one tick is enough to read it.
        Random.InitState(34_100);
        int rotaBurn = Established(PathogenSpecies.Rotavirus, 100, out int rotaEstablished);
        Check($"Rotavirus infections established in tissue ({rotaEstablished}/100) -- the test is not vacuous",
              rotaEstablished >= 90);
        Check($"No rotavirus focus ever burns out on its own ({rotaBurn}/{rotaEstablished})",
              rotaBurn == 0);

        Random.InitState(34_101);
        int genBurn = Established(PathogenSpecies.GenericVirus, 100, out int genEstablished);
        float rate = genEstablished > 0 ? genBurn / (float)genEstablished : 0f;
        Check($"An ordinary virus still burns out at about {InvasionTuning.VirusBurnoutChance:P0}"
              + $" ({genBurn}/{genEstablished} = {rate:P0}) -- unchanged by Sprint 34",
              rate > 0.20f && rate < 0.42f);

        // Why removing burn-out does not break §4b's "self-limiting if wholly
        // ignored": the limit moved to the tropism rotavirus already had.
        var rota = SpeciesRoster.Get(PathogenSpecies.Rotavirus);
        Check("Rotavirus is still self-limiting by geometry -- tropism confines it "
              + $"to {rota.VillusTipTropismCells} cells of the lumen, so the crypt repaves beneath it",
              rota.VillusTipTropismCells > 0);
    }

    /// <summary>Stands up n infections of one species directly in tissue,
    /// ticks each once past establishment, and returns how many rolled a
    /// spontaneous burn-out.</summary>
    private static int Established(PathogenSpecies species, int n, out int established)
    {
        var junk = new List<GameObject>();
        var boardGo = new GameObject($"Sprint34_BoardB_{species}");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);

        // Collect real healthy host slots rather than assuming where the
        // tissue band is -- the band is DATA on BoardConfig. They must also
        // sit inside rotavirus's VILLUS-TIP TROPISM WINDOW, because the first
        // run of this harness collected slots at the CRYPT end and rotavirus
        // established 0 of 200 -- the tropism doing exactly its job. The
        // window is read from the roster, never hard-coded: a harness must
        // not own a number it is not testing.
        int window = Mathf.Max(1,
            SpeciesRoster.Get(PathogenSpecies.Rotavirus).VillusTipTropismCells);
        var slots = new List<CoarseCoord>();
        for (int col = 0; col < board.Columns && slots.Count < n; col++)
            for (int row = 0; row < board.Rows && slots.Count < n; row++)
            {
                var c = new CoarseCoord(col, row);
                if (!grid.IsHealthyHost(c)) continue;
                int depthFromLumen = board.TissueLumenEdgeAxisIndex - board.AxisIndex(c);
                if (depthFromLumen < 0 || depthFromLumen >= window) continue;
                slots.Add(c);
            }

        int burn = 0; established = 0;
        for (int i = 0; i < n && i < slots.Count; i++)
        {
            var go = new GameObject($"Sprint34_Inf_{species}_{i}");
            go.AddComponent<SpriteRenderer>();
            var a = go.AddComponent<PathogenAgent>();
            junk.Add(go);
            // One slot per agent, so no two infections contend for a cell.
            var slot = slots[i];
            a.InitializeInTissueDirect(board, grid, wall, tally, _ => { },
                                       (c, s, free, t) => false, slot,
                                       PathogenClass.IntracellularVirus, 0f,
                                       speciesOverride: species);
            if (!a.IsIntracellular) continue;
            established++;
            a.TickCombat(0.1f);
            if (a.WillBurnOut) burn++;
        }
        Cleanup(junk, boardGo);
        return burn;
    }

    // =================================================================
    // C. The vesicle dose, measured through the real GutWall
    // =================================================================
    private static void RunVesicleDose()
    {
        Debug.Log("[Sprint34Verification] --- C. Vesicle dose at the wall ---");

        float w = SpeciesRoster.CrowdWeightOf(PathogenSpecies.Rotavirus);
        Check($"Rotavirus carries a crowd weight above one ({w:0.##})", w > 1f);
        Check("An un-opted-in species still counts as exactly one organism",
              Mathf.Approximately(SpeciesRoster.CrowdWeightOf(PathogenSpecies.GenericVirus), 1f)
              && Mathf.Approximately(
                  SpeciesRoster.CrowdWeightOf(PathogenSpecies.GenericLargeBacterium), 1f));

        // The LOCKED property of §1b Step 2, which this change must not break:
        // "ingress is something a CROWD does, never something a pathogen
        // does... a trickle is arithmetically impossible, not merely
        // unlikely." So one vesicle-packed virion must still accrue nothing.
        Check($"One rotavirus still sits below the ingress floor "
              + $"({w:0.##} < {InvasionTuning.IngressCrowdFloor:0.##}) -- a lone organism can NEVER charge",
              w < InvasionTuning.IngressCrowdFloor);

        float rotaPeak = PeakCrowd(PathogenSpecies.Rotavirus, 4, out float rotaCharge);
        float genPeak = PeakCrowd(PathogenSpecies.GenericVirus, 4, out float genCharge);

        Check($"Four generic virions make a measurable crowd ({genPeak:0.###}) -- the test is not vacuous",
              genPeak > 0.01f);
        float ratio = genPeak > 0f ? rotaPeak / genPeak : 0f;
        Check($"The same four organisms crowd {ratio:0.##}x harder as rotavirus "
              + $"({rotaPeak:0.###} vs {genPeak:0.###}), matching its weight of {w:0.##}",
              Mathf.Abs(ratio - w) < 0.08f);

        // The gameplay consequence, stated as the thing a player would feel:
        // rotavirus's minimum dose is two organisms where a generic virus
        // needs three.
        float rota2 = PeakCrowd(PathogenSpecies.Rotavirus, 2, out _);
        float gen2 = PeakCrowd(PathogenSpecies.GenericVirus, 2, out _);
        Check($"Two rotavirus clear the ingress floor ({rota2:0.###} > {InvasionTuning.IngressCrowdFloor:0.##})",
              rota2 > InvasionTuning.IngressCrowdFloor);
        Check($"Two ordinary virions do not ({gen2:0.###} <= {InvasionTuning.IngressCrowdFloor:0.##})",
              gen2 <= InvasionTuning.IngressCrowdFloor);

        // The IgA resolution must survive the dose multiplier: a cross-linked
        // raft cannot reach the epithelium, so it contributes NOTHING however
        // many virions its vesicles hold.
        float crossLinked = PeakCrowd(PathogenSpecies.Rotavirus, 4, out _, crossLink: true);
        Check($"A cross-linked rotavirus raft still crowds nothing ({crossLinked:0.###})",
              crossLinked <= 0.0001f);
    }

    /// <summary>Clumps n organisms of one species at one spot on a real
    /// GutWall, lets them settle in to grip, and returns the peak crowd the
    /// wall itself reports.</summary>
    private static float PeakCrowd(PathogenSpecies species, int n, out float charge,
                                   bool crossLink = false)
    {
        var junk = new List<GameObject>();
        var boardGo = new GameObject($"Sprint34_BoardC_{species}_{n}_{crossLink}");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var wall = new GutWall(board, grid, tally);

        const float centre = 10f;
        var list = new List<PathogenAgent>();
        for (int i = 0; i < n; i++)
        {
            var a = NewAgent(junk, board, grid, wall, tally, $"Clump_{species}_{i}", species);
            a.AdhereToWall(centre + (i - n * 0.5f) * 0.07f);
            list.Add(a);
        }

        // Gripping takes about two seconds by design; 6 s is comfortably past
        // it and short enough that nothing has ruptured.
        for (int i = 0; i < 60; i++) wall.Tick(0.1f, i * 0.1f, i * 0.1f);
        if (crossLink) foreach (var a in list) a.SetClump(1);
        for (int i = 0; i < 5; i++) wall.Tick(0.1f, 6f + i * 0.1f, 6f + i * 0.1f);

        float peak = 0f;
        for (int b = 0; b < wall.CrowdBucketCount; b++)
            peak = Mathf.Max(peak, wall.CrowdAtBucket(b));

        charge = 0f;
        for (int i = 0; i < wall.PlaqueCapacity; i++)
            charge = Mathf.Max(charge, wall.PlaqueAt(i).Charge);

        Cleanup(junk, boardGo);
        return peak;
    }

    // -----------------------------------------------------------------

    private static PathogenAgent NewAgent(List<GameObject> junk, BoardConfig board,
        TissueGrid grid, GutWall wall, InvasionTally tally, string name,
        PathogenSpecies species)
    {
        var go = new GameObject($"Sprint34_{name}");
        go.AddComponent<SpriteRenderer>();
        var a = go.AddComponent<PathogenAgent>();
        junk.Add(go);
        a.Initialize(board, grid, wall, tally, _ => { }, (c, s, free, t) => false,
                     speciesOverride: species);
        return a;
    }

    private static void Cleanup(List<GameObject> junk, GameObject boardGo)
    {
        for (int i = 0; i < junk.Count; i++)
            if (junk[i] != null) Object.DestroyImmediate(junk[i]);
        junk.Clear();
        if (boardGo != null) Object.DestroyImmediate(boardGo);
    }
}
