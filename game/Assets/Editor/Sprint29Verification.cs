using ImmunologyTD.Adaptive;
using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.UI;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 29 -- the upgrade pass: a 10-bit barcode, memory recall, four
/// towers that had no upgrades at all, and four shop rows that took ATP
/// and did nothing.
///
/// The theme running through these checks is **an upgrade that writes
/// nothing is worse than no upgrade**, because it still charges. Three
/// have now been caught in this project: the helper-T cap that never
/// bound, the four empty towers, and the four dead shop rows. So several
/// of these tests assert that buying a thing CHANGES a number, rather
/// than trusting that it does.
/// </summary>
public static class Sprint29Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 29 -- upgrades")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2901);

        Debug.Log("[Sprint29Verification] === Upgrades ===");

        RunBarcodeIsHonestNow();
        RunEveryTowerHasUpgrades();
        RunEveryRowWritesSomething();
        RunPlasmaRowsDoNotCollide();
        RunMemoryRecallScalesWithKnowledge();
        RunShopRowsAreLive();
        RunGlycocalyxStrengthensWhenInflamed();

        if (failures > 0)
            Debug.LogError($"[Sprint29Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint29Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint29Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint29Verification] FAIL -- {label}"); }
    }

    // =================================================================
    // 1. The barcode claim, which used to be false
    // =================================================================
    private static void RunBarcodeIsHonestNow()
    {
        Debug.Log("[Sprint29Verification] --- Barcode ---");
        AdaptiveTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();

        Check("the barcode is 10 bits", AdaptiveTuning.BarcodeBits == 10);

        var all = (PathogenSpecies[])System.Enum.GetValues(typeof(PathogenSpecies));
        int worst = int.MaxValue;
        for (int i = 0; i < all.Length; i++)
            for (int j = i + 1; j < all.Length; j++)
                worst = Mathf.Min(worst, Antigen.HammingDistance(
                    Antigen.ForSpecies(all[i]), Antigen.ForSpecies(all[j])));

        int required = 2 * AdaptiveTuning.MatchMaxHammingDistance + 1;
        Check($"tags are >= 2r+1 = {required} apart (worst {worst})", worst >= required);

        // Enumerate the whole space: the property itself, not a proxy.
        int space = 1 << AdaptiveTuning.BarcodeBits;
        int ambiguous = 0;
        for (int tag = 0; tag < space; tag++)
        {
            int n = 0;
            foreach (var sp in all)
                if (Antigen.HammingDistance(tag, Antigen.ForSpecies(sp))
                    <= AdaptiveTuning.MatchMaxHammingDistance) n++;
            if (n > 1) ambiguous++;
        }
        Check($"no tag teaches two species at the default radius ({ambiguous}/{space})",
            ambiguous == 0);

        // Every tag must fit the declared width, or RandomTag can generate
        // values that can never match a species.
        bool inRange = true;
        foreach (var sp in all)
            if (Antigen.ForSpecies(sp) >= space || Antigen.ForSpecies(sp) < 0) inRange = false;
        Check("every species tag fits in the barcode width", inRange);
    }

    // =================================================================
    // 2 + 3. No tower without upgrades; no upgrade without an effect
    // =================================================================
    private static void RunEveryTowerHasUpgrades()
    {
        Debug.Log("[Sprint29Verification] --- Every tower has rows ---");

        foreach (UnitKind k in System.Enum.GetValues(typeof(UnitKind)))
        {
            int n = UpgradeAudit.RowCountFor(k);
            Check($"{k} has upgrade rows ({n})", n > 0);
        }
    }

    private static void RunEveryRowWritesSomething()
    {
        Debug.Log("[Sprint29Verification] --- Every row writes a field ---");

        foreach (UnitKind kind in System.Enum.GetValues(typeof(UnitKind)))
        {
            if (BoneMarrowManager.IsNodeResident(kind)) continue; // adaptive fields, covered elsewhere

            var profile = ProfileFor(kind);
            if (profile == null) continue;

            var rows = UpgradeAudit.RowsFor(kind);
            for (int r = 0; r < rows.Length; r++)
            {
                var baseline = UnitLifecycleTuning.FromProfile(profile);
                var upgraded = UnitLifecycleTuning.FromProfile(profile);
                ProgenitorUpgrades.Apply(kind, r, rows[r].Cap, upgraded, null, profile, 0);

                bool changed =
                    !Mathf.Approximately(baseline.StressSenseChancePerTick, upgraded.StressSenseChancePerTick)
                    || baseline.KillLimit != upgraded.KillLimit
                    || baseline.MaxActiveChildren != upgraded.MaxActiveChildren
                    || baseline.ContactRadiusFineTiles != upgraded.ContactRadiusFineTiles
                    || baseline.FineTilesPerTick != upgraded.FineTilesPerTick
                    || !Mathf.Approximately(baseline.EfferocytosisDebrisPerTick, upgraded.EfferocytosisDebrisPerTick)
                    || !Mathf.Approximately(baseline.DegranulationBurstMultiplier, upgraded.DegranulationBurstMultiplier)
                    || !Mathf.Approximately(baseline.IfnGammaAuraChancePerTick, upgraded.IfnGammaAuraChancePerTick)
                    || baseline.IfnGammaAuraCoarseRadius != upgraded.IfnGammaAuraCoarseRadius
                    || !Mathf.Approximately(baseline.BarrierPatrolRadiusCells, upgraded.BarrierPatrolRadiusCells)
                    || !Mathf.Approximately(baseline.BarrierRepairIntervalSeconds, upgraded.BarrierRepairIntervalSeconds)
                    || !Mathf.Approximately(baseline.AntibodySecretionIntervalSeconds, upgraded.AntibodySecretionIntervalSeconds)
                    || !Mathf.Approximately(baseline.AntibodyLifespanBonusSeconds, upgraded.AntibodyLifespanBonusSeconds);

                Check($"{kind} row {r} ('{rows[r].Name}') actually changes a field", changed);
            }
        }
    }

    private static void RunPlasmaRowsDoNotCollide()
    {
        Debug.Log("[Sprint29Verification] --- Plasma rows are independent ---");

        var profile = ProfileFor(UnitKind.PlasmaCell);
        if (profile == null) { Check("plasma profile exists", false); return; }

        // Buy row 0 then row 1 on the same tower. If they wrote the same
        // field, the second would erase the first -- which is exactly the
        // dead-upgrade failure this sprint exists to remove.
        var t = UnitLifecycleTuning.FromProfile(profile);
        ProgenitorUpgrades.Apply(UnitKind.PlasmaCell, 0, 2, t, null, profile, 0);
        float afterRow0 = t.AntibodySecretionIntervalSeconds;
        ProgenitorUpgrades.Apply(UnitKind.PlasmaCell, 1, 2, t, null, profile, 0);

        Check("buying the second row does not undo the first",
            Mathf.Approximately(t.AntibodySecretionIntervalSeconds, afterRow0));
        Check("...and the second row did take effect",
            t.AntibodyLifespanBonusSeconds > profile.AntibodyLifespanBonusSeconds);
    }

    // =================================================================
    // 4. Memory recall
    // =================================================================
    private static void RunMemoryRecallScalesWithKnowledge()
    {
        Debug.Log("[Sprint29Verification] --- Memory recall ---");
        AdaptiveTuning.ResetToDefaults();

        var t = new AdaptiveUnitTuning();
        Check("an un-upgraded node has no recall at all",
            AdaptiveUnitTuning.MemoryRecallChance(t, 1f) <= 0f);

        t.MemoryRecallLevel = AdaptiveUnitTuning.MemoryRecallCap;
        float unknown = AdaptiveUnitTuning.MemoryRecallChance(t, 0f);
        float half = AdaptiveUnitTuning.MemoryRecallChance(t, 0.5f);
        float known = AdaptiveUnitTuning.MemoryRecallChance(t, 1f);

        // The Director's framing: knowledge stands in for "how many times
        // have we seen this". A species you have never met gets nothing.
        Check("a never-seen species gets no recall bonus", unknown <= 0f);
        Check("recall rises with how well the species is known", known > half && half > 0f);
        Check("recall stays a probability", known <= 1f);
    }

    // =================================================================
    // 5 + 6. The shop
    // =================================================================
    private static void RunShopRowsAreLive()
    {
        Debug.Log("[Sprint29Verification] --- Shop rows are live ---");
        HostDefence.ResetToDefaults();

        Check("viral entry is unchanged at level 0",
            Mathf.Approximately(HostDefence.ViralEntryMultiplier, 1f));
        HostDefence.ViralEntryLevel = 2;
        Check("...and is reduced once bought", HostDefence.ViralEntryMultiplier < 1f);

        Check("no self-destruct at level 0", HostDefence.DsRnaSelfDestructChance <= 0f);
        HostDefence.DsRnaSensorLevel = 2;
        Check("...and infected cells self-destruct once bought",
            HostDefence.DsRnaSelfDestructChance > 0f);

        Check("no opsonisation bonus at level 0",
            Mathf.Approximately(HostDefence.OpsonisationDamageMultiplier, 1f));
        HostDefence.ComplementLevel = 2;
        Check("...and innate damage lands harder once bought",
            HostDefence.OpsonisationDamageMultiplier > 1f);

        HostDefence.ResetToDefaults();

        // And every shop item must be priced, or it cannot be sold.
        bool priced = true;
        foreach (ShopItem it in System.Enum.GetValues(typeof(ShopItem)))
            if (ShopTuning.BasePriceFor(it) <= 0) priced = false;
        Check("every shop item has a price", priced);
    }

    private static void RunGlycocalyxStrengthensWhenInflamed()
    {
        Debug.Log("[Sprint29Verification] --- Glycocalyx vs inflammation ---");
        HostDefence.ResetToDefaults();

        var go = new GameObject("S29_Board");
        try
        {
            var board = go.AddComponent<BoardConfig>();
            board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
            var grid = new TissueGrid(board);
            var wall = new GutWall(board, grid, new InvasionTally());
            GutWall.Current = wall;

            int lane = board.LumenEntryCrossIndex + 3;
            var cell = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, lane);

            Check("no effect before it is bought",
                Mathf.Approximately(HostDefence.GrazeDamageMultiplier(board, cell), 1f));

            HostDefence.GlycocalyxLevel = 2;

            for (int d = 0; d < 4; d++)
            {
                var c = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - d, lane);
                if (grid.IsHostGround(c)) grid.SeedHostState(c, HostState.Healthy, 0f);
            }
            float healthy = HostDefence.GrazeDamageMultiplier(board, cell);
            Check("it blunts grazing in healthy tissue", healthy < 1f);

            for (int d = 0; d < 4; d++)
            {
                var c = board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex - d, lane);
                if (grid.IsHostGround(c)) grid.KillHostCell(c);
            }
            float inflamed = HostDefence.GrazeDamageMultiplier(board, cell);

            // MUC17 is upregulated by TNF-alpha, so this is the one host
            // defence that gets BETTER in damaged tissue -- the counter to
            // the E. coli that feeds on inflammation.
            Check($"...and blunts it harder where the wall is inflamed ({inflamed:F3} < {healthy:F3})",
                inflamed < healthy);
        }
        finally
        {
            GutWall.Current = null;
            HostDefence.ResetToDefaults();
            Object.DestroyImmediate(go);
        }
    }

    // =================================================================

    private static UnitProfile ProfileFor(UnitKind kind)
    {
        // Mirror GameBootstrap's defaults closely enough to exercise Apply.
        switch (kind)
        {
            case UnitKind.Macrophage:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 1, MaxActiveChildren = 4,
                    KillLimit = 20, ContactRadiusFineTiles = 2, EfferocytosisDebrisPerTick = 0.05f,
                    StressSenseChancePerTick = 0.003f };
            case UnitKind.Neutrophil:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 5, MaxActiveChildren = 5,
                    KillLimit = 5, ContactRadiusFineTiles = 2, DegranulatesOnDepletion = true,
                    DegranulationBurstMultiplier = 3f, StressSenseChancePerTick = 0.002f };
            case UnitKind.CytotoxicT:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 3, MaxActiveChildren = 3,
                    KillLimit = 16, ContactRadiusFineTiles = 2, StressSenseChancePerTick = 0.03f,
                    ApoptoticKill = true };
            case UnitKind.NaturalKiller:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 3, MaxActiveChildren = 3,
                    KillLimit = 10, ContactRadiusFineTiles = 2, StressSenseChancePerTick = 0.01f,
                    ApoptoticKill = true, IfnGammaAuraChancePerTick = 0.02f, IfnGammaAuraCoarseRadius = 3 };
            case UnitKind.GammaDeltaT:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 1, MaxActiveChildren = 3,
                    KillLimit = 12, ContactRadiusFineTiles = 3, StressSenseChancePerTick = 0.008f,
                    ApoptoticKill = true, BarrierPatrolRadiusCells = 2f, BarrierRepairIntervalSeconds = 3.5f };
            case UnitKind.PlasmaCell:
                return new UnitProfile { Kind = kind, FineTilesPerTick = 0, MaxActiveChildren = 2,
                    KillLimit = 9999, ContactRadiusFineTiles = 1,
                    AntibodySecretionIntervalSeconds = 1.1f, AntibodyLifespanBonusSeconds = 0f };
            default: return null;
        }
    }
}
