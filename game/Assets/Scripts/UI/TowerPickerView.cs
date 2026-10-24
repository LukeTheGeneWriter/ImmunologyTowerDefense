using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Rendering;
using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The tower picker, floated beside the clicked empty marrow pad
    /// (UI_DESIGN.md §4/§6). **Sprint 21 restyle** (COSMETIC_PASS_SPEC.md
    /// §2.6): a cream card with a 2x2 grid of unit cards -- portrait, name,
    /// one-line blurb, price chip and a chunky PLACE button. Adaptive kinds
    /// only appear when the AdaptiveDirector exists (a harness may not
    /// have one). Built once per target; re-priced by polling.
    /// </summary>
    internal sealed class TowerPickerView
    {
        public readonly VisualElement Root;

        private readonly Label title;
        private readonly VisualElement grid;

        private sealed class UnitCard
        {
            public UnitKind Kind;
            public VisualElement Root, Chip, Progress;
            public Label Price;
            public Label Blurb;
            public Button Place;
            public int LastBalance = int.MinValue;
            public bool LastLicensed = true;
        }

        private readonly System.Collections.Generic.List<UnitCard> cards =
            new System.Collections.Generic.List<UnitCard>();

        private BoneMarrowManager marrow;
        private int slotIndex = -1;
        private bool builtWithAdaptive;
        private bool built;

        public TowerPickerView(VisualElement parent, System.Action onClose)
        {
            Root = UiTheme.Card(10);
            Root.style.position = Position.Absolute;
            Root.style.width = 400;
            parent.Add(Root);

            var head = new VisualElement();
            head.style.flexDirection = FlexDirection.Row;
            head.style.alignItems = Align.Center;
            head.style.justifyContent = Justify.SpaceBetween;
            head.style.marginBottom = 8;
            title = UiTheme.Text("", 12, UiTheme.InkDim, Weight.Black, upper: true);
            head.Add(title);
            head.Add(UiTheme.IconButton(SpriteShapes.IconClose, onClose));
            Root.Add(head);

            grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.SpaceBetween;
            Root.Add(grid);

            UiTheme.Show(Root, false);
        }

        public int SlotIndex => slotIndex;

        public void SetTarget(BoneMarrowManager marrow, int slotIndex)
        {
            this.marrow = marrow;
            this.slotIndex = slotIndex;
            if (marrow == null || slotIndex < 0) return;

            title.text = $"Place a progenitor · slot {slotIndex + 1}".ToUpperInvariant();

            if (!built || builtWithAdaptive != marrow.AdaptiveAvailable)
            {
                grid.Clear();
                cards.Clear();
                // Innate, available from round 1.
                AddCard(UnitKind.Macrophage);
                AddCard(UnitKind.Neutrophil);
                // Sprint 23: also innate, also ungated. The NK/ILC1 needs no
                // priming and the gamma-delta IEL is tissue-resident, so
                // neither waits on the lymph node OR on knowledge -- they are
                // the expensive innate answer for a player who does not want
                // to wait for adaptive immunity.
                AddCard(UnitKind.NaturalKiller);
                AddCard(UnitKind.GammaDeltaT);
                if (marrow.AdaptiveAvailable)
                {
                    AddCard(UnitKind.DendriticCell);
                    AddCard(UnitKind.HelperT);
                    // Knowledge-gated: buyable whenever, but dark until the
                    // rung is reached. The card says so rather than hiding,
                    // so the player can plan the spend (Director, 2026-10-25).
                    AddCard(UnitKind.CytotoxicT);
                    AddCard(UnitKind.PlasmaCell);
                }
                builtWithAdaptive = marrow.AdaptiveAvailable;
                built = true;
            }
            foreach (var c in cards) c.LastBalance = int.MinValue;
        }

        private void AddCard(UnitKind kind)
        {
            var c = new UnitCard { Kind = kind };
            c.Root = UiTheme.Tile();
            c.Root.style.width = 178;
            c.Root.style.height = 128;
            c.Root.style.marginBottom = 8;
            c.Root.style.paddingLeft = c.Root.style.paddingRight = 8;
            c.Root.style.paddingTop = c.Root.style.paddingBottom = 6;
            c.Root.style.justifyContent = Justify.SpaceBetween;

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.alignItems = Align.Center;
            top.Add(UiTheme.Portrait(ProgenitorUpgradeCatalog.SpriteFor(kind), marrow.GetKindColor(kind), 44, out _));
            var name = UiTheme.Text(ProgenitorUpgradeCatalog.DisplayName(kind), 13, UiTheme.Ink, Weight.Black);
            name.style.marginLeft = 6;
            name.style.whiteSpace = WhiteSpace.Normal;
            name.style.flexShrink = 1;
            top.Add(name);
            c.Root.Add(top);

            c.Blurb = UiTheme.Text(ProgenitorUpgradeCatalog.PickerBlurb(kind), 10, UiTheme.InkDim, Weight.Bold);
            c.Blurb.style.whiteSpace = WhiteSpace.Normal;
            c.Root.Add(c.Blurb);

            var bottom = new VisualElement();
            bottom.style.flexDirection = FlexDirection.Row;
            bottom.style.alignItems = Align.Center;
            bottom.style.justifyContent = Justify.SpaceBetween;
            c.Chip = UiTheme.Chip(out c.Price, out c.Progress);
            bottom.Add(c.Chip);
            c.Place = UiTheme.ChunkyButton("PLACE", () => marrow?.PlaceTower(slotIndex, kind), null, 12);
            bottom.Add(c.Place);
            c.Root.Add(bottom);

            grid.Add(c.Root);
            cards.Add(c);
        }

        public void Refresh()
        {
            if (marrow == null || slotIndex < 0) return;
            int balance = marrow.Balance;
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];

                // Sprint 23: a knowledge-gated tower's licence can flip
                // mid-round, so its card has to refresh on that too -- not
                // just on the balance changing.
                bool licensed = ImmunologyTD.Adaptive.KnowledgeLicence.IsTowerLicensed(c.Kind);
                if (balance == c.LastBalance && licensed == c.LastLicensed) continue;

                int price = BoneMarrowManager.PriceFor(c.Kind);
                UiTheme.SetChip(c.Chip, c.Price, c.Progress, price, balance);

                // **Buyable while unlicensed, deliberately** (Director,
                // 2026-10-25): you can commit ATP to a CTL progenitor before
                // the rung is reached, and it sits dark until knowledge
                // arrives. So the button stays enabled and the BLURB carries
                // the warning -- hiding the card would remove the ability to
                // plan toward it, which was the point of choosing this model.
                UiTheme.SetButtonEnabled(c.Place, balance >= price);
                if (c.Blurb != null)
                {
                    c.Blurb.text = licensed
                        ? ProgenitorUpgradeCatalog.PickerBlurb(c.Kind)
                        : ProgenitorUpgradeCatalog.LockedBlurb(c.Kind);
                    c.Blurb.style.color = licensed ? UiTheme.InkDim : UiTheme.Hatch;
                }

                c.LastBalance = balance;
                c.LastLicensed = licensed;
            }
        }
    }
}
