using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// What an *empty* marrow slot opens (docs/UI_DESIGN.md §6): the four
    /// progenitor kinds, their price, and one line each on what they do.
    /// Floats at the slot, same as the upgrade panel it swaps with.
    ///
    /// The rows are the same <see cref="BuyRow"/> component the upgrade
    /// panel and the shop use, with the level dots switched off (`cap 0`) --
    /// placing is a one-shot, not a ladder. That reuse is the reason §6
    /// argued for migrating all three surfaces in one sprint instead of
    /// leaving this one in IMGUI.
    ///
    /// The two adaptive kinds are hidden when there is no AdaptiveDirector
    /// to emit into -- the same rule the old IMGUI picker had, kept because
    /// offering a button that silently cannot work is worse than offering
    /// two buttons.
    /// </summary>
    internal sealed class TowerPickerView
    {
        public readonly VisualElement Root;

        private readonly Label title;
        private readonly VisualElement rowHost;
        private readonly System.Collections.Generic.List<(UnitKind kind, BuyRow row)> rows =
            new System.Collections.Generic.List<(UnitKind, BuyRow)>();

        private BoneMarrowManager marrow;
        private int slotIndex = -1;
        private bool builtWithAdaptive;
        private bool built;

        public TowerPickerView(VisualElement parent, System.Action onClose)
        {
            Root = UiTheme.Panel();
            Root.style.position = Position.Absolute;
            Root.style.width = 300;
            parent.Add(Root);

            title = UiTheme.Text("", 10, UiTheme.InkDim, bold: true, upper: true);
            Root.Add(title);
            Root.Add(UiTheme.Divider());

            rowHost = new VisualElement();
            Root.Add(rowHost);

            var footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.Add(UiTheme.TextButton("close  ✕", onClose));
            Root.Add(footer);

            UiTheme.Show(Root, false);
        }

        public int SlotIndex => slotIndex;

        public void SetTarget(BoneMarrowManager marrow, int slotIndex)
        {
            this.marrow = marrow;
            this.slotIndex = slotIndex;
            if (marrow == null || slotIndex < 0) return;

            title.text = $"Place progenitor · slot {slotIndex + 1}".ToUpperInvariant();

            if (!built || builtWithAdaptive != marrow.AdaptiveAvailable)
            {
                BuildRows(marrow.AdaptiveAvailable);
                builtWithAdaptive = marrow.AdaptiveAvailable;
                built = true;
            }
        }

        private void BuildRows(bool adaptiveAvailable)
        {
            rowHost.Clear();
            rows.Clear();

            AddRow(UnitKind.Macrophage);
            AddRow(UnitKind.Neutrophil);
            if (adaptiveAvailable)
            {
                AddRow(UnitKind.DendriticCell);
                AddRow(UnitKind.HelperT);
            }
        }

        private void AddRow(UnitKind kind)
        {
            var row = new BuyRow(
                ProgenitorUpgradeCatalog.DisplayName(kind),
                ProgenitorUpgradeCatalog.PickerBlurb(kind),
                cap: 0,
                onBuy: () => marrow?.PlaceTower(slotIndex, kind));
            if (rows.Count > 0) rowHost.Add(UiTheme.Divider());
            rowHost.Add(row.Root);
            rows.Add((kind, row));
        }

        public void Refresh()
        {
            if (marrow == null || slotIndex < 0) return;
            for (int i = 0; i < rows.Count; i++)
            {
                int price = BoneMarrowManager.PriceFor(rows[i].kind);
                rows[i].row.Refresh(0, price, marrow.CanAfford(price));
            }
        }
    }
}
