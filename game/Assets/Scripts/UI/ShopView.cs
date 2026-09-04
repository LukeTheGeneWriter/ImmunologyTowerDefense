using UnityEngine.UIElements;
using ImmunologyTD.Economy;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The shop (docs/UI_DESIGN.md §6), docked under the HUD on the right.
    ///
    /// **Two Sprint 16 changes, both the Director's:**
    ///
    /// 1. **No phase gate.** Sprint 11's shop only existed while the buy
    ///    phase had the clock frozen. It is now open during a round as well
    ///    (docs/SPRINT_PLAN.md, decision 3) -- ATP accrues per kill as the
    ///    kills happen (EconomyTuning.AtpPerKill via EconomyHooks), so
    ///    watching the number climb and buying the instant it clears a price
    ///    is a real thing to do mid-wave, and the round does not pause for
    ///    it. The between-rounds freeze is untouched.
    /// 2. **Collapsible.** Because it is now on screen during play it has to
    ///    be able to get out of the way: expanded while the clock is frozen,
    ///    collapsed to its header strip while a round runs, one click either
    ///    way. That keeps §1's "quieter than the tissue" rule without
    ///    putting the buy a screen away when it is wanted fast.
    ///
    /// Five of the six items are placeholders. CytokineSensingUpgrade is the
    /// one real effect (it drives Chemotaxis.SensingUpgradeLevel through
    /// UiController) and it is marked with an Accent dot rather than the old
    /// "(REAL)" text tag.
    /// </summary>
    internal sealed class ShopView
    {
        public readonly VisualElement Root;

        private static readonly (ShopItem item, string name, string effect)[] Items =
        {
            (ShopItem.CytokineSensingUpgrade,  "Cytokine sensing +",       "Sharpens every unit's gradient bias — the one upgrade that already works."),
            (ShopItem.BarrierMucusTurnover,    "Mucus turnover",           "Barrier: flush adherent pathogens back into the lumen."),
            (ShopItem.HostDsRnaSensor,         "Host dsRNA sensor",        "Infected host cells self-destruct and call for clearance."),
            (ShopItem.HostReducedViralEntry,   "Harden vs viral entry",    "Lower the per-tick chance a virion gets inside a host cell."),
            (ShopItem.HostBacterialResistance, "Bacterial resistance",     "Host cells take less damage from a grazing large bacterium."),
            (ShopItem.Crypt,                   "Crypt (local regrowth)",   "A stem-cell niche — nearby tissue regrows faster."),
        };

        private readonly VisualElement body;
        private readonly Button header;
        private readonly BuyRow[] rows = new BuyRow[Items.Length];

        private ShopLedger shop;
        private AtpWallet wallet;
        private bool expanded = true;

        public ShopView(VisualElement parent)
        {
            Root = UiTheme.Panel();
            Root.style.width = 320;

            header = UiTheme.TextButton("", ToggleExpanded, 10);
            header.style.color = UiTheme.InkDim;
            header.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            header.style.letterSpacing = 1f;
            header.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
            Root.Add(header);

            body = new VisualElement();
            Root.Add(body);

            for (int i = 0; i < Items.Length; i++)
            {
                int index = i;
                var entry = Items[i];
                rows[i] = new BuyRow(entry.name, entry.effect, cap: 0,
                    onBuy: () => Buy(Items[index].item),
                    accentDot: entry.item == ShopItem.CytokineSensingUpgrade,
                    showLevelCount: true);
                if (i > 0) body.Add(UiTheme.Divider());
                body.Add(rows[i].Root);
            }

            parent.Add(Root);
            SetExpanded(true);
        }

        public void Bind(ShopLedger shop, AtpWallet wallet)
        {
            this.shop = shop;
            this.wallet = wallet;
        }

        private void Buy(ShopItem item)
        {
            if (shop == null || wallet == null) return;
            shop.TryBuy(item, wallet);
        }

        private void ToggleExpanded() => SetExpanded(!expanded);

        public void SetExpanded(bool value)
        {
            expanded = value;
            UiTheme.Show(body, expanded);
            header.text = expanded ? "SHOP    ▾" : "SHOP    ▸  (click to buy mid-round)";
        }

        /// <summary>Collapse on the way into a round, expand on the way back
        /// out. Not a lock -- the player can always click it open again
        /// while the round runs, which is the whole point of decision 3.</summary>
        public void OnPhaseChanged(bool frozen) => SetExpanded(frozen);

        public void Refresh()
        {
            if (shop == null || wallet == null || !expanded) return;
            for (int i = 0; i < rows.Length; i++)
            {
                int level = shop.LevelOf(Items[i].item);
                int price = shop.NextPrice(Items[i].item);
                rows[i].Refresh(level, price, wallet.CanAfford(price));
            }
        }
    }
}
