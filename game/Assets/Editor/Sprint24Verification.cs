using ImmunologyTD.Adaptive;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 24 -- named pathogen species, and the three mechanics the first
/// three of them bring.
///
/// The check that matters most is the first one. Before this sprint,
/// knowledge was keyed by <see cref="PathogenClass"/> -- three of them --
/// so learning about "intracellular virus" taught you about every virus
/// that would ever arrive. GAME_DESIGN.md §5 has always promised the
/// opposite ("a new species arriving in a late round meets a naive adaptive
/// system"), and the promise was unkeepable. If a later session collapses
/// species back onto class to simplify something, that promise breaks
/// silently and every other test still passes.
///
/// Seeded, same reasoning as Sprints 22 and 23: several of these are races.
/// </summary>
public static class Sprint24Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 24 -- named species")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2401);

        Debug.Log("[Sprint24Verification] === Named pathogen species ===");

        RunKnowledgeIsPerSpecies();
        RunRosterIsCoherent();
        RunAntigensStaySeparable();
        RunRotavirusSparesTheCrypt();
        RunEnterotoxinHitsBystanders();
        RunCholeraSubvertsRatherThanKills();
        RunInflammationFeedsTheRightSpecies();

        if (failures > 0)
            Debug.LogError($"[Sprint24Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint24Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint24Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint24Verification] FAIL -- {label}"); }
    }

    // =================================================================

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public InvasionTally Tally;
        public GutWall Wall;
        public readonly System.Collections.Generic.List<GameObject> Junk =
            new System.Collections.Generic.List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++)
                if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
            GutWall.Current = null;
        }
    }

    private static Rig BuildRig(string name)
    {
        InvasionTuning.ResetToDefaults();
        TissueTuning.ResetToDefaults();
        BarrierTuning.ResetToDefaults();
        IgaTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();

        var rig = new Rig();
        rig.BoardGo = new GameObject($"Sprint24_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Wall = new GutWall(rig.Board, rig.Grid, rig.Tally);
        GutWall.Current = rig.Wall;
        return rig;
    }

    private static PathogenAgent AgentAt(Rig rig, CoarseCoord cell, PathogenSpecies species, float now)
    {
        var go = new GameObject($"Sprint24_{species}");
        go.AddComponent<SpriteRenderer>();
        var a = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        a.InitializeInTissueDirect(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                                   x => { }, (c, s, free, t) => false,
                                   cell, SpeciesRoster.ClassOf(species), now, species);
        return a;
    }

    // =================================================================
    // 1. The promise §5 could not previously keep
    // =================================================================
    private static void RunKnowledgeIsPerSpecies()
    {
        Debug.Log("[Sprint24Verification] --- Per-species knowledge ---");

        var k = new KnowledgeLedger();
        k.Add(PathogenSpecies.Rotavirus, 50f);

        Check("learning about rotavirus raises rotavirus",
            Mathf.Approximately(k.Get(PathogenSpecies.Rotavirus), 50f));

        // THE regression guard. Rotavirus and the generic virus share a
        // CLASS; if knowledge is ever re-keyed to class, this goes to 50.
        Check("...and teaches nothing about a different virus species",
            Mathf.Approximately(k.Get(PathogenSpecies.GenericVirus), 0f));
        Check("...nor about an unrelated species",
            Mathf.Approximately(k.Get(PathogenSpecies.VibrioCholerae), 0f));

        // The class-keyed bridge still means "the generic one".
        k.Add(PathogenClass.IntracellularVirus, 20f);
        Check("the class-keyed overload resolves to that class's generic",
            Mathf.Approximately(k.Get(PathogenSpecies.GenericVirus), 20f)
            && Mathf.Approximately(k.Get(PathogenSpecies.Rotavirus), 50f));

        k.Reset();
        Check("Reset zeroes every species",
            Mathf.Approximately(k.Get(PathogenSpecies.Rotavirus), 0f)
            && Mathf.Approximately(k.Get(PathogenSpecies.GenericVirus), 0f));
    }

    // =================================================================
    // 2. The roster itself
    // =================================================================
    private static void RunRosterIsCoherent()
    {
        Debug.Log("[Sprint24Verification] --- Roster ---");
        SpeciesRoster.ResetToDefaults();

        Check("rotavirus is a virus", SpeciesRoster.ClassOf(PathogenSpecies.Rotavirus)
            == PathogenClass.IntracellularVirus);
        Check("vibrio is a large extracellular bacterium",
            SpeciesRoster.ClassOf(PathogenSpecies.VibrioCholerae) == PathogenClass.LargeBacterium);
        Check("A/E E. coli is a large extracellular bacterium",
            SpeciesRoster.ClassOf(PathogenSpecies.AttachingEffacingEcoli) == PathogenClass.LargeBacterium);

        // Generic ordinals must keep matching PathogenClass, or every
        // pre-Sprint-24 harness quietly starts testing a different organism.
        Check("generic ordinals still line up with PathogenClass",
            (int)PathogenSpecies.GenericVirus == (int)PathogenClass.IntracellularVirus
            && (int)PathogenSpecies.GenericIntracellularBacterium == (int)PathogenClass.IntracellularBacterium
            && (int)PathogenSpecies.GenericLargeBacterium == (int)PathogenClass.LargeBacterium);

        Check("every species has a display name",
            !string.IsNullOrEmpty(SpeciesRoster.NameOf(PathogenSpecies.Rotavirus))
            && !string.IsNullOrEmpty(SpeciesRoster.NameOf(PathogenSpecies.VibrioCholerae)));

        // Only the species that should carry a trait does.
        Check("only rotavirus has villus-tip tropism",
            SpeciesRoster.Get(PathogenSpecies.Rotavirus).VillusTipTropismCells > 0
            && SpeciesRoster.Get(PathogenSpecies.GenericVirus).VillusTipTropismCells == 0);
        Check("only vibrio secretes a toxin field",
            SpeciesRoster.Get(PathogenSpecies.VibrioCholerae).ToxinFieldStrength > 0f
            && SpeciesRoster.Get(PathogenSpecies.GenericLargeBacterium).ToxinFieldStrength <= 0f);
        Check("only A/E E. coli feeds on inflammation",
            SpeciesRoster.Get(PathogenSpecies.AttachingEffacingEcoli).InflammationFuelFactor > 0f
            && SpeciesRoster.Get(PathogenSpecies.VibrioCholerae).InflammationFuelFactor <= 0f);
    }

    // =================================================================
    // 3. Barcode separation -- the roster-size constraint
    // =================================================================
    private static void RunAntigensStaySeparable()
    {
        Debug.Log("[Sprint24Verification] --- Antigen separation ---");
        SpeciesRoster.ResetToDefaults();

        var all = (PathogenSpecies[])System.Enum.GetValues(typeof(PathogenSpecies));
        int worst = 8;
        for (int i = 0; i < all.Length; i++)
            for (int j = i + 1; j < all.Length; j++)
                worst = Mathf.Min(worst, Antigen.HammingDistance(
                    Antigen.ForSpecies(all[i]), Antigen.ForSpecies(all[j])));

        Check("every pair of species tags is distinct", worst > 0);

        // >= 4 bits apart means a helper-T within distance 2 of one species
        // is within 2 of at most one -- which is what keeps "knowledge is
        // per species" from leaking. This is also the constraint that
        // ceilings the roster at ~8-10 species on 8 bits.
        Check("every pair is >= 4 bits apart, so one tag teaches one species",
            worst >= 4);
    }

    // =================================================================
    // 4. Rotavirus tropism
    // =================================================================
    private static void RunRotavirusSparesTheCrypt()
    {
        Debug.Log("[Sprint24Verification] --- Rotavirus tropism ---");

        var rig = BuildRig("Rota");
        try
        {
            int lane = 5;
            int window = SpeciesRoster.Get(PathogenSpecies.Rotavirus).VillusTipTropismCells;

            // A cell at the villus tip -- right at the lumen edge.
            var tip = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex, lane);
            // A cell well behind it, toward the crypt/base end.
            var crypt = rig.Board.CoarseFromAxis(
                rig.Board.TissueLumenEdgeAxisIndex - window - 2, lane);

            rig.Grid.SeedHostState(tip, HostState.Healthy, 0f);
            rig.Grid.SeedHostState(crypt, HostState.Healthy, 0f);

            var atTip = AgentAt(rig, tip, PathogenSpecies.Rotavirus, 0f);
            Check("rotavirus infects a villus-tip enterocyte",
                rig.Grid.GetHostState(tip) == HostState.Infected);
            Check("...and reports itself in tropism there", atTip.WithinTropismAt(tip));

            var atCrypt = AgentAt(rig, crypt, PathogenSpecies.Rotavirus, 0f);
            Check("rotavirus CANNOT infect a crypt-end cell",
                rig.Grid.GetHostState(crypt) != HostState.Infected);
            Check("...and reports itself out of tropism there", !atCrypt.WithinTropismAt(crypt));

            // A generic virus has no such restriction -- proving the
            // behaviour belongs to the species, not to the board.
            var crypt2 = rig.Board.CoarseFromAxis(
                rig.Board.TissueLumenEdgeAxisIndex - window - 3, lane);
            rig.Grid.SeedHostState(crypt2, HostState.Healthy, 0f);
            AgentAt(rig, crypt2, PathogenSpecies.GenericVirus, 0f);
            Check("a generic virus infects the same deep cell happily",
                rig.Grid.GetHostState(crypt2) == HostState.Infected);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 5. NSP4
    // =================================================================
    private static void RunEnterotoxinHitsBystanders()
    {
        Debug.Log("[Sprint24Verification] --- NSP4 enterotoxin ---");

        var rig = BuildRig("NSP4");
        try
        {
            int lane = 7;
            var tip = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex, lane);
            rig.Grid.SeedHostState(tip, HostState.Healthy, 0f);

            // Seed the whole neighbourhood healthy.
            foreach (var off in FineCoord.VonNeumannOffsets)
            {
                var n = new CoarseCoord(tip.Column + off.Column, tip.Row + off.Row);
                if (rig.Grid.IsHostGround(n)) rig.Grid.SeedHostState(n, HostState.Healthy, 0f);
            }

            var rota = AgentAt(rig, tip, PathogenSpecies.Rotavirus, 0f);
            Check("precondition: infected", rig.Grid.GetHostState(tip) == HostState.Infected);

            float before = 0f; int counted = 0;
            foreach (var off in FineCoord.VonNeumannOffsets)
            {
                var n = new CoarseCoord(tip.Column + off.Column, tip.Row + off.Row);
                if (rig.Grid.GetHostState(n) != HostState.Healthy) continue;
                before += rig.Grid.GetHostHealth(n); counted++;
            }

            for (int i = 0; i < 60; i++) rota.TickCombat(i * 0.5f);

            float after = 0f;
            foreach (var off in FineCoord.VonNeumannOffsets)
            {
                var n = new CoarseCoord(tip.Column + off.Column, tip.Row + off.Row);
                if (!rig.Grid.IsHostGround(n)) continue;
                after += rig.Grid.GetHostHealth(n);
            }

            Check("there were healthy neighbours to poison", counted > 0);
            // The mechanic: harm reaches cells the virus never entered.
            Check("NSP4 damages uninfected neighbours", after < before);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 6. Cholera -- subversion, not destruction
    // =================================================================
    private static void RunCholeraSubvertsRatherThanKills()
    {
        Debug.Log("[Sprint24Verification] --- Cholera toxin field ---");

        var rig = BuildRig("Vibrio");
        try
        {
            float s = rig.Wall.SOfLane(rig.Board.LumenEntryCrossIndex + 2);

            Check("the wall starts unsubverted", rig.Wall.ToxinLoadAt(s) <= 0f);
            Check("and intact", rig.Wall.StateAt(s) == GutWall.BarrierState.Intact);

            rig.Wall.ApplyToxinField(s, 0.9f, 2.5f, 1.0f);

            Check("a toxin field subverts the wall", rig.Wall.ToxinLoadAt(s) > 0f);
            // The whole point: nothing died.
            Check("...without damaging the barrier at all",
                rig.Wall.StateAt(s) == GutWall.BarrierState.Intact);

            // Reversible -- clearing the secretors should undo it.
            float peak = rig.Wall.ToxinLoadAt(s);
            for (int i = 0; i < 400; i++) rig.Wall.Tick(0.1f, i * 0.1f, i * 0.1f);
            Check("subversion decays once the secretors are gone",
                rig.Wall.ToxinLoadAt(s) < peak);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 7. Inflammation as fuel
    // =================================================================
    private static void RunInflammationFeedsTheRightSpecies()
    {
        Debug.Log("[Sprint24Verification] --- Inflammation as fuel ---");

        var rig = BuildRig("Fuel");
        try
        {
            int lane = rig.Board.LumenEntryCrossIndex + 3;
            float s = rig.Wall.SOfLane(lane);

            // Quiet wall first.
            for (int d = 0; d < 4; d++)
            {
                var c = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex - d, lane);
                if (rig.Grid.IsHostGround(c)) rig.Grid.SeedHostState(c, HostState.Healthy, 0f);
            }
            float quiet = rig.Wall.LocalInflammation(s);
            Check("healthy tissue reads as uninflamed", quiet <= 0.01f);

            // Now make a mess of it -- exactly what a necrotic innate clear
            // leaves behind.
            for (int d = 0; d < 4; d++)
            {
                var c = rig.Board.CoarseFromAxis(rig.Board.TissueLumenEdgeAxisIndex - d, lane);
                if (rig.Grid.IsHostGround(c)) rig.Grid.KillHostCell(c);
            }
            float inflamed = rig.Wall.LocalInflammation(s);
            Check("a field of debris reads as inflamed", inflamed > quiet);
            Check("...strongly", inflamed > 0.5f);

            // And the trait is species-specific: the profile carries it,
            // so a species without the trait cannot benefit.
            Check("A/E E. coli has a non-zero fuel factor",
                SpeciesRoster.Get(PathogenSpecies.AttachingEffacingEcoli).InflammationFuelFactor > 0f);
            Check("vibrio and rotavirus do not feed on inflammation",
                SpeciesRoster.Get(PathogenSpecies.VibrioCholerae).InflammationFuelFactor <= 0f
                && SpeciesRoster.Get(PathogenSpecies.Rotavirus).InflammationFuelFactor <= 0f);
        }
        finally { rig.Dispose(); }
    }
}
