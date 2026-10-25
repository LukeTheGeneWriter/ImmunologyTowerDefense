using ImmunologyTD.Economy;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 27 -- expanding the bone marrow.
///
/// GAME_DESIGN.md §2a's oldest open question ("Open: bone marrow
/// capacity"), answered after the Director lost a run and said the
/// interesting part was having to think about where the ATP went.
///
/// The checks that matter most are the economic ones. A capacity upgrade
/// is the easiest thing in a tower defence to get wrong: price it low and
/// "buy more slots" becomes the answer to every problem, which removes the
/// decision the Director actually enjoyed.
/// </summary>
public static class Sprint27Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 27 -- marrow expansion")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(2701);

        Debug.Log("[Sprint27Verification] === Marrow expansion ===");

        RunPriceCurve();
        RunStartsFattyBeyondAllowance();
        RunClickingFattyBuysIt();
        RunFattyIsNotABuildSite();
        RunFillsTopDownAndStops();
        RunCannotAffordDoesNothing();

        if (failures > 0)
            Debug.LogError($"[Sprint27Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint27Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint27Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint27Verification] FAIL -- {label}"); }
    }

    // =================================================================

    private class Rig
    {
        public GameObject BoardGo, MarrowGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public CytokineField Field;
        public BoneMarrowManager Marrow;
        public AtpWallet Wallet;

        public void Dispose()
        {
            if (MarrowGo != null) Object.DestroyImmediate(MarrowGo);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
            EconomyTuning.ResetToDefaults();
        }
    }

    private static Rig BuildRig(string name, int atp)
    {
        EconomyTuning.ResetToDefaults();

        var rig = new Rig();
        rig.BoardGo = new GameObject($"S27_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Field = new CytokineField(rig.Board);
        rig.Wallet = new AtpWallet(atp);

        var mac = new UnitProfile { Kind = UnitKind.Macrophage, DisplayName = "M", FineTilesPerTick = 1,
                                    FootprintFineTiles = 3, MaxActiveChildren = 4, KillLimit = 20,
                                    ContactRadiusFineTiles = 2 };
        var neu = new UnitProfile { Kind = UnitKind.Neutrophil, DisplayName = "N", FineTilesPerTick = 5,
                                    FootprintFineTiles = 3, MaxActiveChildren = 5, KillLimit = 5,
                                    ContactRadiusFineTiles = 2 };

        var positions = new Vector3[EconomyTuning.MarrowMaxSlots];
        for (int i = 0; i < positions.Length; i++) positions[i] = new Vector3(0f, -i, 0f);

        rig.MarrowGo = new GameObject($"S27_Marrow_{name}");
        rig.Marrow = rig.MarrowGo.AddComponent<BoneMarrowManager>();
        rig.Marrow.Initialize(rig.Board, rig.Grid, rig.Field, mac, null, neu, null,
                              positions, 1f, rig.Wallet);
        return rig;
    }

    // =================================================================
    // 1. The price curve -- the thing most likely to break the game
    // =================================================================
    private static void RunPriceCurve()
    {
        Debug.Log("[Sprint27Verification] --- Price curve ---");
        EconomyTuning.ResetToDefaults();

        int first = EconomyTuning.MarrowReconversionPrice(0);
        int second = EconomyTuning.MarrowReconversionPrice(1);
        int last = EconomyTuning.MarrowReconversionPrice(
            EconomyTuning.MarrowMaxSlots - EconomyTuning.MarrowStartingSlots - 1);

        Check($"each niche costs more than the last ({first} < {second} < {last})",
            first < second && second < last);

        // Buying ROOM for a tower must never be cheaper than the tower.
        // Otherwise the optimal line is "expand first, always".
        int dearestTower = Mathf.Max(EconomyTuning.CytotoxicTPrice,
                           Mathf.Max(EconomyTuning.BCellPrice, EconomyTuning.MacrophagePrice));
        Check($"the first niche ({first}) costs more than the dearest tower ({dearestTower})",
            first > dearestTower);

        // And the last one has to hurt, or late ATP has no competing use.
        Check($"the last niche ({last}) costs more than three dear towers",
            last > dearestTower * 3);
    }

    // =================================================================
    // 2. Starting state
    // =================================================================
    private static void RunStartsFattyBeyondAllowance()
    {
        Debug.Log("[Sprint27Verification] --- Starting state ---");

        var rig = BuildRig("Start", 0);
        try
        {
            Check($"starts with {EconomyTuning.MarrowStartingSlots} working niches",
                rig.Marrow.UnlockedSlots == EconomyTuning.MarrowStartingSlots);
            Check($"the bone is laid out for all {EconomyTuning.MarrowMaxSlots}",
                rig.Marrow.SlotCount == EconomyTuning.MarrowMaxSlots);

            bool statesRight = true;
            for (int i = 0; i < rig.Marrow.SlotCount; i++)
            {
                var want = i < EconomyTuning.MarrowStartingSlots
                    ? BoneMarrowSlotState.Empty : BoneMarrowSlotState.Fatty;
                if (rig.Marrow.GetSlotState(i) != want) statesRight = false;
            }
            // The unbought pads EXIST and are visible from frame one -- that
            // is how the mechanic teaches itself.
            Check("every pad beyond the allowance is fatty marrow, not absent", statesRight);
            Check("there is something left to buy", rig.Marrow.CanReconvert);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 3 + 4. Clicking behaviour
    // =================================================================
    private static void RunClickingFattyBuysIt()
    {
        Debug.Log("[Sprint27Verification] --- Clicking fatty marrow ---");

        var rig = BuildRig("Click", 10_000);
        try
        {
            int before = rig.Marrow.UnlockedSlots;
            int price = rig.Marrow.NextReconversionPrice;
            int atp = rig.Marrow.Balance;

            rig.Marrow.OnSlotClicked(before); // the first fatty pad

            Check("clicking fatty marrow reconverts a niche",
                rig.Marrow.UnlockedSlots == before + 1);
            Check("...and charges for it", rig.Marrow.Balance == atp - price);
            Check("...and that pad is now a build site",
                rig.Marrow.GetSlotState(before) == BoneMarrowSlotState.Empty);
            Check("...and it did NOT also select the slot (one click, one action)",
                !rig.Marrow.SelectedSlotIndex.HasValue);
        }
        finally { rig.Dispose(); }
    }

    private static void RunFattyIsNotABuildSite()
    {
        Debug.Log("[Sprint27Verification] --- Fatty is not a build site ---");

        var rig = BuildRig("NoBuild", 10_000);
        try
        {
            int fatty = EconomyTuning.MarrowStartingSlots;
            int atp = rig.Marrow.Balance;

            rig.Marrow.PlaceTower(fatty, UnitKind.Macrophage);

            Check("a tower cannot be placed on fatty marrow",
                rig.Marrow.GetSlotState(fatty) == BoneMarrowSlotState.Fatty);
            Check("...and no ATP was taken for the attempt",
                rig.Marrow.Balance == atp);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 5. Fills top-down, and stops
    // =================================================================
    private static void RunFillsTopDownAndStops()
    {
        Debug.Log("[Sprint27Verification] --- Filling the bone ---");

        var rig = BuildRig("Fill", 10_000);
        try
        {
            // Click the LAST pad every time; it must still convert the next
            // one in order, so the bone fills as one growing column rather
            // than a scatter of gaps.
            int guard = 0;
            while (rig.Marrow.CanReconvert && guard++ < 50)
                rig.Marrow.OnSlotClicked(rig.Marrow.SlotCount - 1);

            Check("the whole bone can be bought out",
                rig.Marrow.UnlockedSlots == EconomyTuning.MarrowMaxSlots);

            bool noGaps = true;
            for (int i = 0; i < rig.Marrow.SlotCount; i++)
                if (rig.Marrow.GetSlotState(i) == BoneMarrowSlotState.Fatty) noGaps = false;
            Check("...with no fatty gaps left behind", noGaps);

            Check("and then there is nothing more to sell", !rig.Marrow.CanReconvert);
            Check("...with no price quoted", rig.Marrow.NextReconversionPrice == 0);

            int atp = rig.Marrow.Balance;
            rig.Marrow.OnSlotClicked(rig.Marrow.SlotCount - 1);
            Check("...and a further click is free and harmless",
                rig.Marrow.Balance == atp
                && rig.Marrow.UnlockedSlots == EconomyTuning.MarrowMaxSlots);
        }
        finally { rig.Dispose(); }
    }

    // =================================================================
    // 6. Poverty
    // =================================================================
    private static void RunCannotAffordDoesNothing()
    {
        Debug.Log("[Sprint27Verification] --- Cannot afford ---");

        var rig = BuildRig("Broke", 5);
        try
        {
            int before = rig.Marrow.UnlockedSlots;
            rig.Marrow.OnSlotClicked(before);

            Check("a click you cannot afford buys nothing",
                rig.Marrow.UnlockedSlots == before);
            Check("...and spends nothing", rig.Marrow.Balance == 5);
            Check("...and leaves the pad fatty",
                rig.Marrow.GetSlotState(before) == BoneMarrowSlotState.Fatty);
        }
        finally { rig.Dispose(); }
    }
}
