using UnityEngine;
using UnityEngine.UIElements;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The whole visual language of the front-end, in one place
    /// (docs/UI_DESIGN.md §1). Palette, type scale, spacing unit, and the
    /// handful of element factories every view is assembled from.
    ///
    /// **Direction: "the chart clipped to the specimen, not a cockpit."**
    /// The board is a stained histology section and the mobile agents are
    /// the only saturated things on it, so the UI has to be quieter than
    /// both -- flat translucent glass, hairline rules, one type family at a
    /// few sizes, 3 px corners, no glow / bevel / gradient. Colour is spent
    /// only where meaning has to survive a glance: ATP gold, lives teal
    /// (oxblood when low), one slate-blue accent for "this is interactive."
    /// Everything else is off-white ink and taupe dim-ink.
    ///
    /// Colours are deliberately offset from the board palettes in
    /// UI_STYLE_GUIDE.md rather than reused: the ATP gold is duller and
    /// darker than neutrophil gold so a HUD numeral is never mistaken for a
    /// unit, lives-low is the muted oxblood of base plasma rather than the
    /// hot breach flash, and the accent is macrophage blue dragged most of
    /// the way to slate.
    ///
    /// **Fonts are assigned explicitly, not inherited from a theme.**
    /// PanelSettings.themeStyleSheet is null by choice (UI_DESIGN.md §7 --
    /// a .tss is an editor-imported asset and this project builds its UI
    /// from code), which means there is no default font either. Rather than
    /// gamble on what an unstyled Label renders as, the root gets an OS
    /// font here and every child inherits it.
    /// </summary>
    internal static class UiTheme
    {
        // ---- palette (docs/UI_DESIGN.md §1) ----
        public static readonly Color PanelBg   = new Color(0.059f, 0.055f, 0.067f, 0.86f);
        public static readonly Color PanelBgSolid = new Color(0.059f, 0.055f, 0.067f, 0.94f);
        public static readonly Color Ink       = new Color(0.914f, 0.898f, 0.855f);
        public static readonly Color InkDim    = new Color(0.545f, 0.522f, 0.478f);
        public static readonly Color Rule      = new Color(0.914f, 0.898f, 0.855f, 0.14f);
        public static readonly Color Atp       = new Color(0.796f, 0.722f, 0.471f);
        public static readonly Color LivesOk   = new Color(0.510f, 0.663f, 0.627f);
        public static readonly Color LivesLow  = new Color(0.753f, 0.361f, 0.263f);
        public static readonly Color Accent    = new Color(0.431f, 0.561f, 0.690f);
        public static readonly Color AccentDim = new Color(0.431f, 0.561f, 0.690f, 0.35f);
        public static readonly Color Defeat    = new Color(0.604f, 0.231f, 0.173f);

        /// <summary>Base spacing unit, px at the 1920x1080 reference.</summary>
        public const int S = 4;

        private static Font uiFont;
        private static Font monoFont;

        /// <summary>The interface sans. Created from an OS font because no
        /// .ttf ships with the project and no theme supplies one.</summary>
        public static Font UiFont
        {
            get
            {
                if (uiFont == null)
                    uiFont = CreateOsFont(new[] { "Segoe UI", "Helvetica Neue", "Arial", "Liberation Sans" }, 14);
                return uiFont;
            }
        }

        /// <summary>The debug readout's face. It is an instrument panel and
        /// is allowed to look like one (UI_DESIGN.md §1).</summary>
        public static Font MonoFont
        {
            get
            {
                if (monoFont == null)
                    monoFont = CreateOsFont(new[] { "Consolas", "Menlo", "Courier New", "Liberation Mono" }, 12);
                return monoFont;
            }
        }

        private static Font CreateOsFont(string[] names, int size)
        {
            // Never throw out of a style helper -- a missing OS font must
            // degrade to "UITK picks something", not to a broken boot.
            try
            {
                var f = Font.CreateDynamicFontFromOSFont(names, size);
                if (f != null) return f;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UiTheme] OS font lookup failed ({e.GetType().Name}); falling back to the UITK default.");
            }
            return null;
        }

        public static void ApplyFont(VisualElement e, bool mono = false)
        {
            var f = mono ? MonoFont : UiFont;
            if (f != null) e.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(f));
        }

        // ---- element factories ----

        /// <summary>One text run at a size and colour from the §1 type
        /// scale. <paramref name="upper"/> also applies the 0.10em tracking
        /// the uppercase label styles carry.</summary>
        public static Label Text(string s, int px, Color c, bool bold = false, bool upper = false)
        {
            var l = new Label(upper ? (s ?? "").ToUpperInvariant() : s);
            l.style.fontSize = px;
            l.style.color = c;
            l.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            if (upper) l.style.letterSpacing = px * 0.10f;
            l.style.marginTop = 0; l.style.marginBottom = 0;
            l.style.marginLeft = 0; l.style.marginRight = 0;
            l.style.paddingTop = 0; l.style.paddingBottom = 0;
            return l;
        }

        /// <summary>A slab of smoked glass: the one panel treatment every
        /// surface in the game uses.</summary>
        public static VisualElement Panel(bool opaque = false)
        {
            var v = new VisualElement();
            v.style.backgroundColor = opaque ? PanelBgSolid : PanelBg;
            SetBorderWidth(v, 1);
            SetBorderColor(v, Rule);
            SetRadius(v, 3);
            v.style.paddingTop = v.style.paddingBottom = 3 * S;
            v.style.paddingLeft = v.style.paddingRight = 4 * S;
            v.style.marginTop = v.style.marginRight = v.style.marginLeft = v.style.marginBottom = 3 * S;
            return v;
        }

        public static VisualElement Divider()
        {
            var v = new VisualElement();
            v.style.height = 1;
            v.style.backgroundColor = Rule;
            v.style.marginTop = v.style.marginBottom = 2 * S;
            return v;
        }

        /// <summary>The one interactive treatment: flat, with a 2 px accent
        /// bar down the left edge. No pill, no gradient, no rounding --
        /// UI_DESIGN.md §1 rules those out explicitly.</summary>
        public static Button FlatButton(string label, System.Action onClick, int px = 13)
        {
            var b = new Button(onClick) { text = label };
            b.style.fontSize = px;
            b.style.color = Ink;
            b.style.backgroundColor = PanelBg;
            SetBorderWidth(b, 0);
            b.style.borderLeftWidth = 2;
            b.style.borderLeftColor = Accent;
            SetRadius(b, 0);
            b.style.paddingLeft = b.style.paddingRight = 3 * S;
            b.style.paddingTop = b.style.paddingBottom = 2 * S;
            b.style.marginLeft = b.style.marginRight = 0;
            b.style.marginTop = b.style.marginBottom = 0;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            return b;
        }

        /// <summary>The small text-only affordance used for "close" and the
        /// shop's collapse header -- no accent bar, no chrome.</summary>
        public static Button TextButton(string label, System.Action onClick, int px = 11)
        {
            var b = new Button(onClick) { text = label };
            b.style.fontSize = px;
            b.style.color = InkDim;
            b.style.backgroundColor = Color.clear;
            SetBorderWidth(b, 0);
            SetRadius(b, 0);
            b.style.paddingLeft = b.style.paddingRight = S;
            b.style.paddingTop = b.style.paddingBottom = S;
            b.style.marginLeft = b.style.marginRight = 0;
            b.style.marginTop = b.style.marginBottom = 0;
            return b;
        }

        /// <summary>Level pips: filled per level owned, hollow per level
        /// remaining. Text rather than elements -- it is a value readout,
        /// not a control, and one Label is cheaper than N VisualElements
        /// rebuilt on every purchase.</summary>
        public static string Dots(int filled, int total)
        {
            if (total <= 0) return string.Empty;
            var sb = new System.Text.StringBuilder(total);
            for (int i = 0; i < total; i++) sb.Append(i < filled ? '●' : '○');
            return sb.ToString();
        }

        public static void SetBorderWidth(VisualElement v, int w)
        {
            v.style.borderTopWidth = v.style.borderBottomWidth =
                v.style.borderLeftWidth = v.style.borderRightWidth = w;
        }

        public static void SetBorderColor(VisualElement v, Color c)
        {
            v.style.borderTopColor = v.style.borderBottomColor =
                v.style.borderLeftColor = v.style.borderRightColor = c;
        }

        public static void SetRadius(VisualElement v, int r)
        {
            v.style.borderTopLeftRadius = v.style.borderTopRightRadius =
                v.style.borderBottomLeftRadius = v.style.borderBottomRightRadius = r;
        }

        public static void Show(VisualElement v, bool visible)
        {
            if (v == null) return;
            v.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>
    /// The buy row shared by the upgrade panel, the shop and the tower
    /// picker (docs/UI_DESIGN.md §7): name, effect text, level dots, cost,
    /// and a buy control with three states -- affordable, unaffordable, and
    /// maxed.
    ///
    /// It is a class rather than a factory function because the views poll
    /// (there are no events on AtpWallet / ShopLedger / BoneMarrowManager,
    /// and adding them is out of scope) and a row therefore has to be
    /// *re-priced every frame* while its structure is built exactly once.
    /// Rebuilding the elements each frame would churn the layout engine for
    /// no reason and drop the click that lands on the frame it rebuilds.
    /// </summary>
    internal sealed class BuyRow
    {
        public readonly VisualElement Root;

        private readonly Label dots;
        private readonly Label cost;
        private readonly Label maxed;
        private readonly Button buy;
        private readonly int cap;

        private readonly bool showLevelCount;

        private bool lastAffordable = true;
        private int lastLevels = -1;
        private int lastPrice = -1;

        /// <param name="cap">Level ceiling; 0 means "not a levelled row"
        /// (the tower picker's place-a-tower rows), which hides the dots.</param>
        /// <param name="showLevelCount">For uncapped repeatable rows (the
        /// shop's items, which have no ceiling): print "Lv N" where a capped
        /// row would print its dots. Dots can't say "seven" legibly; a
        /// number can.</param>
        public BuyRow(string name, string effect, int cap, System.Action onBuy,
                      bool accentDot = false, bool showLevelCount = false)
        {
            this.cap = cap;
            this.showLevelCount = showLevelCount;

            Root = new VisualElement();
            Root.style.marginBottom = UiTheme.S;

            var nameLine = new VisualElement();
            nameLine.style.flexDirection = FlexDirection.Row;
            nameLine.style.alignItems = Align.Center;
            if (accentDot)
            {
                // The one shop row that really does something keeps a mark,
                // but a dot rather than the old "(REAL)" text tag.
                var d = UiTheme.Text("● ", 11, UiTheme.Accent);
                nameLine.Add(d);
            }
            nameLine.Add(UiTheme.Text(name, 13, UiTheme.Ink));
            Root.Add(nameLine);

            if (!string.IsNullOrEmpty(effect))
            {
                var e = UiTheme.Text(effect, 11, UiTheme.InkDim);
                e.style.whiteSpace = WhiteSpace.Normal;
                e.style.marginBottom = UiTheme.S;
                Root.Add(e);
            }

            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            line.style.alignItems = Align.Center;
            line.style.justifyContent = Justify.SpaceBetween;

            dots = UiTheme.Text(UiTheme.Dots(0, cap), 11, UiTheme.Accent);
            line.Add(dots);

            var right = new VisualElement();
            right.style.flexDirection = FlexDirection.Row;
            right.style.alignItems = Align.Center;

            cost = UiTheme.Text("", 11, UiTheme.InkDim);
            cost.style.marginRight = 2 * UiTheme.S;
            right.Add(cost);

            maxed = UiTheme.Text("MAX", 13, UiTheme.AccentDim, bold: true);
            UiTheme.Show(maxed, false);
            right.Add(maxed);

            buy = UiTheme.FlatButton("BUY", onBuy);
            right.Add(buy);

            line.Add(right);
            Root.Add(line);
        }

        /// <summary>Called every frame by the owning view. Cheap: it early-
        /// outs unless the affordability, level or price actually moved.</summary>
        public void Refresh(int levels, int price, bool affordable)
        {
            if (levels == lastLevels && price == lastPrice && affordable == lastAffordable) return;

            bool isMaxed = cap > 0 && levels >= cap;

            if (levels != lastLevels)
            {
                dots.text = showLevelCount
                    ? (levels > 0 ? $"Lv {levels}" : string.Empty)
                    : UiTheme.Dots(levels, cap);
                dots.style.color = isMaxed ? UiTheme.AccentDim : UiTheme.Accent;
            }

            cost.text = isMaxed ? "" : $"{price} ATP";
            cost.style.color = affordable ? UiTheme.InkDim : UiTheme.LivesLow;

            UiTheme.Show(maxed, isMaxed);
            UiTheme.Show(buy, !isMaxed);
            if (!isMaxed)
            {
                // Unaffordable reads as a dash rather than a dead "BUY":
                // the label itself says you can't, not just the colour, so
                // it survives a colour-blind read (UI_DESIGN.md §1).
                buy.text = affordable ? "BUY" : "–";
                buy.style.color = affordable ? UiTheme.Ink : UiTheme.AccentDim;
                buy.style.borderLeftColor = affordable ? UiTheme.Accent : UiTheme.AccentDim;
                buy.SetEnabled(affordable);
            }

            lastLevels = levels;
            lastPrice = price;
            lastAffordable = affordable;
        }
    }
}
