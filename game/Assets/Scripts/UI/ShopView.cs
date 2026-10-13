using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Economy;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The shop, **Sprint 21: an always-open tray of tiles in the bottom-right
    /// gutter** (docs/COSMETIC_PASS_SPEC.md §2.5, Director 2026-10-14). It
    /// covers nothing on the board, so it no longer collapses -- Sprint 16's
    /// "buy mid-round" stays, without its cost.
    ///
    /// Each tile is one Button: a composite icon built from the game's own
    /// sprites, the name, a price chip (gold when affordable; grey with a
    /// red number and a "how close am I" bar when not), and a level badge.
    /// Placeholder items -- bought, but no effect yet -- carry the hatched
    /// PLANNED band: "hatched = designed, not built" is one rule across the
    /// UI (spec §4.4). A shared tooltip card explains the hovered tile.
    ///
    /// Feel: hover lifts the tile 4 px with an accent edge; a purchase
    /// pulses it; a chip that just became affordable pops once -- the
    /// "watch the number" cue. Polls each frame and writes only on change.
    /// </summary>
    internal sealed class ShopView
    {
        public readonly VisualElement Root;

        private delegate void IconRecipe(VisualElement box, float size);

        private sealed class Entry
        {
            public ShopItem Item;
            public string Name, Short, Effect;
            public bool Real;
            public IconRecipe Icon;
        }

        private static readonly Entry[] Items =
        {
            new Entry { Item = ShopItem.CytokineSensingUpgrade, Name = "Cytokine sensing", Short = "Cytokine\nsensing", Real = true,
                Effect = "Sharpens every unit's pull toward infection.",
                Icon = (b, s) => UiTheme.Layer(b, SpriteShapes.IconWaves, Palette.CytokineHot, s) },
            new Entry { Item = ShopItem.BarrierMucusTurnover, Name = "Mucus turnover", Short = "Mucus\nturnover",
                Effect = "Flush pathogens stuck at the gut wall back into the lumen.",
                Icon = (b, s) => UiTheme.Layer(b, SpriteShapes.IconDroplet, new Color(0.55f, 0.82f, 0.96f), s) },
            new Entry { Item = ShopItem.HostDsRnaSensor, Name = "Host dsRNA sensor", Short = "dsRNA\nsensor",
                Effect = "Infected host cells self-destruct and call dendritic cells.",
                Icon = (b, s) =>
                {
                    UiTheme.Layer(b, SpriteShapes.HostCellInfectedViral, Palette.HostInfectedViral, s, 0.85f);
                    UiTheme.Layer(b, SpriteShapes.BreachStar, Palette.FxStressKill, s, 0.55f, 0.20f, -0.20f);
                } },
            new Entry { Item = ShopItem.HostReducedViralEntry, Name = "Harden vs viral entry", Short = "Block viral\nentry",
                Effect = "A virion is less likely to get inside a host cell.",
                Icon = (b, s) =>
                {
                    UiTheme.Layer(b, SpriteShapes.HostCell, Palette.HostHealthy, s, 0.85f);
                    UiTheme.Layer(b, SpriteShapes.Virion, Palette.Virion, s, 0.50f, 0.18f, -0.18f);
                    UiTheme.Layer(b, SpriteShapes.IconBlock, new Color(0.85f, 0.12f, 0.16f), s, 0.55f, 0.18f, -0.18f);
                } },
            new Entry { Item = ShopItem.HostBacterialResistance, Name = "Bacterial resistance", Short = "Bacterial\nresistance",
                Effect = "Host cells take less damage from a grazing bacterium.",
                Icon = (b, s) =>
                {
                    UiTheme.Layer(b, SpriteShapes.HostCell, Palette.HostHealthy, s, 0.85f);
                    UiTheme.Layer(b, SpriteShapes.LargeBacterium, Palette.Bacterium, s, 0.50f, 0.18f, -0.18f);
                    UiTheme.Layer(b, SpriteShapes.IconBlock, new Color(0.85f, 0.12f, 0.16f), s, 0.55f, 0.18f, -0.18f);
                } },
            new Entry { Item = ShopItem.Crypt, Name = "Crypt", Short = "Crypt", Real = true,
                Effect = "A stem-cell niche at the gut wall. Nearby tissue regrows faster.",
                Icon = (b, s) =>
                {
                    UiTheme.Layer(b, SpriteShapes.Crypt, Palette.CryptPocket, s);
                    UiTheme.Layer(b, SpriteShapes.GranuleCluster, Palette.Paneth, s, 0.40f, 0f, 0.18f);
                } },
            new Entry { Item = ShopItem.MCellSampling, Name = "M-cell sampling", Short = "M-cell\nsampling", Real = true,
                Effect = "The Peyer's patch samples pathogens in the gut and hands antigen to dendritic cells.",
                Icon = (b, s) =>
                {
                    UiTheme.Layer(b, SpriteShapes.PeyerDome, Palette.PeyerDome, s, 1f, 0f, 0.12f);
                    UiTheme.Layer(b, SpriteShapes.MCell, Palette.MCell, s, 0.42f, 0f, -0.06f);
                } },
        };

        private sealed class TileView
        {
            public Button Root;
            public VisualElement IconBox, Chip, Progress, Badge;
            public Label Price, BadgeText;
            public int LastLevel = -1, LastPrice = -1, LastBalance = int.MinValue;
            public bool LastAffordable;
            public float Pulse = -1f, ChipPop = -1f;
            public bool Hovered;
        }

        private readonly TileView[] tiles = new TileView[Items.Length];
        private readonly VisualElement tooltip;
        private readonly Label tipTitle, tipEffect, tipNext, tipAction;
        private int tipIndex = -1;

        private ShopLedger shop;
        private AtpWallet wallet;

        private const float TileW = 112f, TileH = 120f, IconSize = 56f;

        public ShopView(VisualElement parent)
        {
            Root = UiTheme.Card(8);
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.alignItems = Align.Center;
            Root.style.marginRight = 24;
            Root.style.marginBottom = 14;
            parent.Add(Root);

            var header = new VisualElement();
            header.style.width = 26;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.Center;
            var h = UiTheme.Text("SHOP", 12, UiTheme.InkDim, Weight.Black);
            h.style.rotate = new Rotate(new Angle(-90f, AngleUnit.Degree));
            header.Add(h);
            Root.Add(header);

            for (int i = 0; i < Items.Length; i++) tiles[i] = BuildTile(i);

            // The tooltip floats above the tray, positioned per hovered tile.
            tooltip = UiTheme.Card(8);
            tooltip.style.position = Position.Absolute;
            tooltip.style.width = 270;
            tooltip.style.bottom = Length.Percent(100);
            tooltip.style.marginBottom = 10;
            tooltip.pickingMode = PickingMode.Ignore;
            tipTitle = UiTheme.Text("", 14, UiTheme.Ink, Weight.Black);
            tipEffect = UiTheme.Text("", 12, UiTheme.Ink);
            tipEffect.style.whiteSpace = WhiteSpace.Normal;
            tipEffect.style.marginTop = 4;
            tipNext = UiTheme.Text("", 12, UiTheme.InkDim, Weight.Bold);
            tipNext.style.marginTop = 6;
            tipAction = UiTheme.Text("", 12, UiTheme.Accent, Weight.Black);
            tipAction.style.whiteSpace = WhiteSpace.Normal;
            tooltip.Add(tipTitle); tooltip.Add(tipEffect); tooltip.Add(tipNext); tooltip.Add(tipAction);
            Root.Add(tooltip);
            UiTheme.Show(tooltip, false);
        }

        private TileView BuildTile(int index)
        {
            var e = Items[index];
            var t = new TileView();
            var b = new Button(() => Buy(index)) { text = "" };
            t.Root = b;
            b.style.width = TileW; b.style.height = TileH;
            b.style.marginLeft = 6;
            b.style.backgroundColor = UiTheme.TileFill;
            UiTheme.SetBorderWidth(b, 3);
            b.style.borderBottomWidth = 5;
            UiTheme.SetBorderColor(b, UiTheme.Edge);
            UiTheme.SetRadius(b, 10);
            b.style.paddingTop = 6; b.style.paddingBottom = 6;
            b.style.paddingLeft = b.style.paddingRight = 4;
            b.style.alignItems = Align.Center;
            b.style.justifyContent = Justify.SpaceBetween;
            b.style.overflow = Overflow.Visible;

            t.IconBox = new VisualElement();
            t.IconBox.style.width = IconSize; t.IconBox.style.height = IconSize;
            t.IconBox.pickingMode = PickingMode.Ignore;
            e.Icon(t.IconBox, IconSize);
            b.Add(t.IconBox);

            if (!e.Real)
            {
                var band = UiTheme.PlannedPill();
                band.style.position = Position.Absolute;
                band.style.top = 44;
                b.Add(band);
            }

            var name = UiTheme.Text(e.Short, 11, UiTheme.Ink, Weight.Black);
            name.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.Add(name);

            t.Chip = UiTheme.Chip(out t.Price, out t.Progress);
            t.Chip.style.minWidth = 70;
            b.Add(t.Chip);

            t.Badge = new VisualElement();
            t.Badge.style.position = Position.Absolute;
            t.Badge.style.right = -10; t.Badge.style.top = -10;
            t.Badge.style.width = 26; t.Badge.style.height = 26;
            t.Badge.style.backgroundColor = UiTheme.Accent;
            UiTheme.SetBorderWidth(t.Badge, 2);
            UiTheme.SetBorderColor(t.Badge, UiTheme.Edge);
            UiTheme.SetRadius(t.Badge, 13);
            t.Badge.style.alignItems = Align.Center;
            t.Badge.style.justifyContent = Justify.Center;
            t.Badge.pickingMode = PickingMode.Ignore;
            t.BadgeText = UiTheme.Text("0", 13, Color.white, Weight.Black);
            UiTheme.Outline(t.BadgeText, 1f);
            t.Badge.Add(t.BadgeText);
            b.Add(t.Badge);
            UiTheme.Show(t.Badge, false);

            b.RegisterCallback<PointerEnterEvent>(_ => { t.Hovered = true; ShowTip(index); });
            b.RegisterCallback<PointerLeaveEvent>(_ => { t.Hovered = false; if (tipIndex == index) HideTip(); });

            Root.Add(b);
            return t;
        }

        public void Bind(ShopLedger shop, AtpWallet wallet)
        {
            this.shop = shop;
            this.wallet = wallet;
        }

        private void Buy(int index)
        {
            if (shop == null || wallet == null) return;
            if (shop.TryBuy(Items[index].Item, wallet)) tiles[index].Pulse = 0f;
        }

        /// <summary>Kept for UiController: the tray never collapses now.</summary>
        public void SetExpanded(bool value) { }
        public void OnPhaseChanged(bool frozen) { }

        private void ShowTip(int index)
        {
            tipIndex = index;
            UiTheme.Show(tooltip, true);
            RefreshTip();
            var tile = tiles[index].Root;
            float x = tile.layout.x + tile.layout.width * 0.5f - 135f;
            tooltip.style.left = Mathf.Max(0f, x);
        }

        private void HideTip()
        {
            tipIndex = -1;
            UiTheme.Show(tooltip, false);
        }

        private void RefreshTip()
        {
            if (tipIndex < 0 || shop == null) return;
            var e = Items[tipIndex];
            int lvl = shop.LevelOf(e.Item);
            tipTitle.text = lvl > 0 ? $"{e.Name}  ·  Lv {lvl}" : e.Name;
            tipEffect.text = e.Effect;
            tipNext.text = $"{(lvl > 0 ? "Next level" : "Price")}: {shop.NextPrice(e.Item)} ATP";
            tipAction.text = e.Real ? "Click to buy" : "Planned: buying spends ATP but does nothing yet.";
            tipAction.style.color = e.Real ? UiTheme.Accent : UiTheme.InkDim;
        }

        public void Refresh()
        {
            if (shop == null || wallet == null) return;
            float dt = Time.unscaledDeltaTime;
            int balance = wallet.Balance;
            for (int i = 0; i < tiles.Length; i++)
            {
                var t = tiles[i];
                var e = Items[i];
                int level = shop.LevelOf(e.Item);
                int price = shop.NextPrice(e.Item);
                bool ok = balance >= price;

                if (level != t.LastLevel || price != t.LastPrice || balance != t.LastBalance)
                {
                    UiTheme.SetChip(t.Chip, t.Price, t.Progress, price, balance);
                    t.Root.style.backgroundColor = ok ? UiTheme.TileFill : UiTheme.TileDim;
                    t.IconBox.style.opacity = ok ? 1f : 0.45f;
                    if (level != t.LastLevel)
                    {
                        UiTheme.Show(t.Badge, level > 0);
                        t.BadgeText.text = level.ToString();
                        if (tipIndex == i) RefreshTip();
                    }
                    if (ok && !t.LastAffordable && t.LastBalance != int.MinValue) t.ChipPop = 0f;
                    t.LastLevel = level; t.LastPrice = price; t.LastBalance = balance; t.LastAffordable = ok;
                }

                // Feel: hover lift, purchase pulse, affordable pop.
                t.Root.style.translate = new Translate(0, t.Hovered ? -4 : 0);
                UiTheme.SetBorderColor(t.Root, t.Hovered ? UiTheme.Accent : UiTheme.Edge);
                if (t.Pulse >= 0f)
                {
                    t.Pulse += dt / 0.25f;
                    float s = t.Pulse < 0.4f ? Mathf.Lerp(1f, 0.92f, t.Pulse / 0.4f)
                            : t.Pulse < 0.75f ? Mathf.Lerp(0.92f, 1.06f, (t.Pulse - 0.4f) / 0.35f)
                            : Mathf.Lerp(1.06f, 1f, (t.Pulse - 0.75f) / 0.25f);
                    UiTheme.SetScale(t.Root, s);
                    if (t.Pulse >= 1f) { t.Pulse = -1f; UiTheme.SetScale(t.Root, 1f); }
                }
                if (t.ChipPop >= 0f)
                {
                    t.ChipPop += dt / 0.3f;
                    UiTheme.SetScale(t.Chip, UiTheme.Pop(t.ChipPop, 1.15f));
                    if (t.ChipPop >= 1f) { t.ChipPop = -1f; UiTheme.SetScale(t.Chip, 1f); }
                }
            }
        }
    }
}
