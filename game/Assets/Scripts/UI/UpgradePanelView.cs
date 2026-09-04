using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The first real screen (docs/UI_DESIGN.md §4): click a placed marrow
    /// slot, get that tower's roster.
    ///
    /// **It floats at the slot** rather than docking to the right edge, which
    /// is where the spec landed. The Director's call (docs/SPRINT_PLAN.md,
    /// decision 2): a panel that appears *at the thing you clicked* is worth
    /// more than a panel that is always in the same rectangle, and the
    /// board's own selection rim is not a substitute for proximity. The cost
    /// -- a floating panel can cover other slots -- is paid down by anchoring
    /// it to the side of the marrow column that has the whole tissue band in
    /// it, and by clamping it inside the screen.
    ///
    /// Everything it buys is a placeholder (GAME_DESIGN.md §6d): ATP is
    /// spent, a level is bumped, the simulation is untouched. What makes it
    /// worth building now is that the rows are the *real* roster -- each one
    /// names a mechanic this unit already has and the field an effect would
    /// write (ProgenitorUpgradeCatalog), so the wiring sprint is one line
    /// per row.
    /// </summary>
    internal sealed class UpgradePanelView
    {
        public readonly VisualElement Root;

        private readonly Label title;
        private readonly VisualElement portrait;
        private readonly Label kindName;
        private readonly Label nicheDots;
        private readonly Label fielded;
        private readonly VisualElement rowHost;

        private readonly System.Collections.Generic.List<BuyRow> rows =
            new System.Collections.Generic.List<BuyRow>();

        private BoneMarrowManager marrow;
        private int slotIndex = -1;
        private UnitKind builtForKind;
        private bool built;

        public UpgradePanelView(VisualElement parent, System.Action onClose)
        {
            Root = UiTheme.Panel();
            Root.style.position = Position.Absolute;
            Root.style.width = 320;
            parent.Add(Root);

            title = UiTheme.Text("", 10, UiTheme.InkDim, bold: true, upper: true);
            Root.Add(title);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginTop = 2 * UiTheme.S;

            // A real portrait for free: the same procedural sprite the unit
            // is drawn with on the board, tinted the same colour. No new art,
            // and the panel can never drift out of sync with the units.
            portrait = new VisualElement();
            portrait.style.width = 40;
            portrait.style.height = 40;
            portrait.style.marginRight = 3 * UiTheme.S;
            header.Add(portrait);

            var headerText = new VisualElement();
            kindName = UiTheme.Text("", 13, UiTheme.Ink);
            nicheDots = UiTheme.Text("", 11, UiTheme.Accent);
            nicheDots.style.marginTop = UiTheme.S / 2;
            headerText.Add(kindName);
            headerText.Add(nicheDots);
            header.Add(headerText);
            Root.Add(header);

            fielded = UiTheme.Text("", 11, UiTheme.InkDim);
            fielded.style.marginTop = UiTheme.S;
            Root.Add(fielded);

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

        /// <summary>Points the panel at a slot. Row *structure* is rebuilt
        /// only when the kind changes -- re-selecting between two macrophage
        /// niches keeps the same elements and just re-prices them.</summary>
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
            portrait.style.backgroundImage = new StyleBackground(SpriteFor(kind));
            portrait.style.unityBackgroundImageTintColor = marrow.GetKindColor(kind);
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

            nicheDots.text = $"niche level {UiTheme.Dots(marrow.GetUpgradeLevel(slotIndex), 6)}";
            fielded.text = $"{marrow.GetActiveChildren(slotIndex)} / {marrow.GetTuning(slotIndex).MaxActiveChildren} cells fielded";

            for (int i = 0; i < rows.Count && i < catalog.Length; i++)
            {
                int level = marrow.GetUpgradeLevel(slotIndex, i);
                int price = ShopTuning.ProgenitorUpgradePrice(catalog[i].BasePrice, level);
                rows[i].Refresh(level, price, marrow.CanAfford(price));
            }
        }

        private static Sprite SpriteFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return ImmunologyTD.Rendering.SpriteShapes.Macrophage;
                case UnitKind.Neutrophil: return ImmunologyTD.Rendering.SpriteShapes.Neutrophil;
                case UnitKind.DendriticCell: return ImmunologyTD.Rendering.SpriteShapes.DendriteStar;
                default: return ImmunologyTD.Rendering.SpriteShapes.Lymphocyte;
            }
        }
    }
}
