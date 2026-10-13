using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The per-tower upgrade panel, floated beside a placed marrow pad
    /// (UI_DESIGN.md §4/§5). **Sprint 21 restyle** (COSMETIC_PASS_SPEC.md
    /// §2.6): a cream card -- a portrait of the unit in its own sprite and
    /// colour, the name, "cells fielded · niche level", then one row per
    /// upgrade with level pips, a price chip and a chunky BUY (MAX when
    /// capped). Every row is live since Sprint 18. Polls; re-prices with an
    /// early-out.
    /// </summary>
    internal sealed class UpgradePanelView
    {
        public readonly VisualElement Root;

        private readonly Label title;
        private readonly VisualElement portraitImage;
        private readonly Label kindName;
        private readonly Label status;
        private readonly VisualElement rowHost;

        private readonly System.Collections.Generic.List<BuyRow> rows =
            new System.Collections.Generic.List<BuyRow>();

        private BoneMarrowManager marrow;
        private int slotIndex = -1;
        private UnitKind builtForKind;
        private bool built;
        private string lastStatus;

        public UpgradePanelView(VisualElement parent, System.Action onClose)
        {
            Root = UiTheme.Card(10);
            Root.style.position = Position.Absolute;
            Root.style.width = 360;
            parent.Add(Root);

            var head = new VisualElement();
            head.style.flexDirection = FlexDirection.Row;
            head.style.alignItems = Align.Center;
            head.style.justifyContent = Justify.SpaceBetween;
            title = UiTheme.Text("", 12, UiTheme.InkDim, Weight.Black, upper: true);
            head.Add(title);
            head.Add(UiTheme.IconButton(ImmunologyTD.Rendering.SpriteShapes.IconClose, onClose));
            Root.Add(head);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginTop = 2 * UiTheme.S;
            header.Add(UiTheme.Portrait(ImmunologyTD.Rendering.SpriteShapes.Macrophage, Color.white, 48, out portraitImage));
            var headerText = new VisualElement();
            headerText.style.marginLeft = 3 * UiTheme.S;
            kindName = UiTheme.Text("", 17, UiTheme.Ink, Weight.Black);
            status = UiTheme.Text("", 11, UiTheme.InkDim, Weight.Bold);
            headerText.Add(kindName);
            headerText.Add(status);
            header.Add(headerText);
            Root.Add(header);

            Root.Add(UiTheme.Divider());

            rowHost = new VisualElement();
            Root.Add(rowHost);

            UiTheme.Show(Root, false);
        }

        public int SlotIndex => slotIndex;

        public void SetTarget(BoneMarrowManager marrow, int slotIndex)
        {
            this.marrow = marrow;
            this.slotIndex = slotIndex;
            if (marrow == null || slotIndex < 0) return;

            var kind = marrow.GetSlotKind(slotIndex);
            title.text = $"Progenitor niche · slot {slotIndex + 1}".ToUpperInvariant();

            if (!built || kind != builtForKind)
            {
                BuildRows(kind);
                builtForKind = kind;
                built = true;
            }

            kindName.text = ProgenitorUpgradeCatalog.DisplayName(kind);
            portraitImage.style.backgroundImage = new StyleBackground(ProgenitorUpgradeCatalog.SpriteFor(kind));
            portraitImage.style.unityBackgroundImageTintColor = marrow.GetKindColor(kind);
            lastStatus = null;
        }

        private void BuildRows(UnitKind kind)
        {
            rowHost.Clear();
            rows.Clear();

            var catalog = ProgenitorUpgradeCatalog.RowsFor(kind);
            for (int i = 0; i < catalog.Length; i++)
            {
                int row = i;                       // captured per row, not per frame
                var entry = catalog[i];
                var buyRow = new BuyRow(entry.Name, entry.Effect, entry.Cap,
                    () => marrow?.UpgradeTower(slotIndex, row, entry.BasePrice, entry.Cap));
                rows.Add(buyRow);
                if (i > 0) rowHost.Add(UiTheme.Divider());
                rowHost.Add(buyRow.Root);
            }
        }

        public void Refresh()
        {
            if (marrow == null || slotIndex < 0) return;
            if (marrow.GetSlotState(slotIndex) != BoneMarrowSlotState.Placed) return;

            var kind = marrow.GetSlotKind(slotIndex);
            var catalog = ProgenitorUpgradeCatalog.RowsFor(kind);

            string s = $"{marrow.GetActiveChildren(slotIndex)} / {marrow.GetTuning(slotIndex).MaxActiveChildren} cells fielded  ·  niche level {marrow.GetUpgradeLevel(slotIndex)}";
            if (s != lastStatus) { status.text = s; lastStatus = s; }

            int balance = marrow.Balance;
            for (int i = 0; i < rows.Count && i < catalog.Length; i++)
            {
                int level = marrow.GetUpgradeLevel(slotIndex, i);
                int price = ShopTuning.ProgenitorUpgradePrice(catalog[i].BasePrice, level);
                rows[i].Refresh(level, price, balance);
            }
        }
    }
}
