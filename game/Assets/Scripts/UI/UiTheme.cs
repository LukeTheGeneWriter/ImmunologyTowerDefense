using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.UI
{
    internal enum Weight { Regular, Bold, Black }

    /// <summary>
    /// The whole visual language of the front end, in one place.
    ///
    /// **Sprint 21: light cards on a dark desk** (docs/COSMETIC_PASS_SPEC.md
    /// §2, Director 2026-10-14). Sprint 16's dark smoked glass was built for
    /// the dark "histology plate" board; on the daylight board a near-black
    /// slab was the loudest value contrast on screen, louder than any
    /// pathogen. Now: the board is a light plate on a dark desk (the camera
    /// clear), and the UI is light cream cards on the same desk -- paper is
    /// light and inked, the desk is dark, units are fill + dark outline.
    /// Everything chunky: 3 px ink edges, a thicker bottom edge as the
    /// "lift" (UI Toolkit has no box-shadow), rounded corners, a rounded
    /// typeface.
    ///
    /// **Font: Nunito** (SIL OFL, `Assets/Resources/Fonts/`, licence beside
    /// it), Regular / Bold / Black as separate faces rather than faux-bold.
    /// Falls back to an OS font if the asset is missing, never to a broken
    /// boot. The debug readout keeps a monospace OS font -- it is an
    /// instrument panel.
    ///
    /// **Symbols are sprites, never glyphs** (▶ ♥ ✕ are missing from the
    /// shipped fonts): see <c>SpriteShapes.Icon*</c> and <see cref="Icon"/>.
    /// </summary>
    internal static class UiTheme
    {
        // ---- tokens (spec §2.2) ----
        public static readonly Color CardFill  = new Color(0.99f, 0.96f, 0.90f);
        public static readonly Color TileFill  = new Color(1f, 1f, 1f);
        public static readonly Color TileDim   = new Color(0.91f, 0.88f, 0.85f);
        public static readonly Color Ink       = new Color(0.20f, 0.12f, 0.16f);
        public static readonly Color Edge      = Ink;
        public static readonly Color InkDim    = new Color(0.48f, 0.39f, 0.41f);
        public static readonly Color Rule      = new Color(0.20f, 0.12f, 0.16f, 0.15f);
        public static readonly Color Accent    = new Color(0.13f, 0.58f, 0.88f);
        public static readonly Color AccentLip = new Color(0.08f, 0.30f, 0.48f);
        public static readonly Color AccentDim = new Color(0.13f, 0.58f, 0.88f, 0.35f);
        public static readonly Color Gold      = new Color(1.00f, 0.78f, 0.22f);
        public static readonly Color ChipDim   = new Color(0.84f, 0.80f, 0.78f);
        public static readonly Color Bad       = new Color(0.80f, 0.16f, 0.16f);
        public static readonly Color Heart     = new Color(0.90f, 0.22f, 0.32f);
        public static readonly Color Hatch     = new Color(0.55f, 0.50f, 0.52f);
        public static readonly Color PortraitBg = new Color(0.96f, 0.94f, 0.92f);

        /// <summary>Base spacing unit, px at the 1920x1080 reference.</summary>
        public const int S = 4;

        // ---- fonts ----
        private static Font regular, bold, black, mono;
        private static bool fontsLoaded;

        private static void LoadFonts()
        {
            if (fontsLoaded) return;
            fontsLoaded = true;
            regular = Resources.Load<Font>("Fonts/Nunito-Regular");
            bold = Resources.Load<Font>("Fonts/Nunito-Bold");
            black = Resources.Load<Font>("Fonts/Nunito-Black");
            if (regular == null) regular = CreateOsFont(new[] { "Segoe UI", "Helvetica Neue", "Arial", "Liberation Sans" }, 14);
            if (bold == null) bold = regular;
            if (black == null) black = bold;
            mono = CreateOsFont(new[] { "Consolas", "Menlo", "Courier New", "Liberation Mono" }, 12);
        }

        public static Font FontFor(Weight w)
        {
            LoadFonts();
            return w == Weight.Black ? black : w == Weight.Bold ? bold : regular;
        }

        private static Font CreateOsFont(string[] names, int size)
        {
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
            LoadFonts();
            var f = mono ? UiTheme.mono : regular;
            if (f != null) e.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(f));
        }

        public static void SetWeight(VisualElement e, Weight w)
        {
            var f = FontFor(w);
            if (f != null) e.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(f));
            e.style.unityFontStyleAndWeight = FontStyle.Normal;
        }

        // ---- text ----

        public static Label Text(string s, int px, Color c, Weight w = Weight.Regular, bool upper = false)
        {
            var l = new Label(upper ? (s ?? "").ToUpperInvariant() : s);
            l.style.fontSize = px;
            l.style.color = c;
            SetWeight(l, w);
            if (upper) l.style.letterSpacing = px * 0.06f;
            l.style.marginTop = 0; l.style.marginBottom = 0;
            l.style.marginLeft = 0; l.style.marginRight = 0;
            l.style.paddingTop = 0; l.style.paddingBottom = 0;
            l.style.paddingLeft = 0; l.style.paddingRight = 0;
            l.pickingMode = PickingMode.Ignore;
            return l;
        }

        /// <summary>Pre-Sprint-21 signature, kept for the debug readout.</summary>
        public static Label Text(string s, int px, Color c, bool bold, bool upper = false) =>
            Text(s, px, c, bold ? Weight.Bold : Weight.Regular, upper);

        /// <summary>A white heading with an ink outline -- button labels, pills.</summary>
        public static void Outline(Label l, float width = 1.5f)
        {
            l.style.unityTextOutlineWidth = width;
            l.style.unityTextOutlineColor = Edge;
        }

        // ---- surfaces ----

        /// <summary>The card every surface is built on: cream, a 3 px ink
        /// edge, a 6 px bottom edge as its lift, radius 12.</summary>
        public static VisualElement Card(int pad = 10)
        {
            var v = new VisualElement();
            v.style.backgroundColor = CardFill;
            SetBorderWidth(v, 3);
            v.style.borderBottomWidth = 6;
            SetBorderColor(v, Edge);
            SetRadius(v, 12);
            v.style.paddingTop = v.style.paddingBottom = pad;
            v.style.paddingLeft = v.style.paddingRight = pad + 2;
            return v;
        }

        /// <summary>Pre-Sprint-21 name: the debug readout's panel.</summary>
        public static VisualElement Panel(bool opaque = false) => Card();

        public static VisualElement Tile()
        {
            var v = new VisualElement();
            v.style.backgroundColor = TileFill;
            SetBorderWidth(v, 3);
            v.style.borderBottomWidth = 5;
            SetBorderColor(v, Edge);
            SetRadius(v, 10);
            return v;
        }

        public static VisualElement Divider()
        {
            var v = new VisualElement();
            v.style.height = 2;
            v.style.backgroundColor = Rule;
            v.style.marginTop = v.style.marginBottom = 2 * S;
            v.pickingMode = PickingMode.Ignore;
            return v;
        }

        /// <summary>A sprite shown as a UI element, tinted.</summary>
        public static VisualElement Icon(Sprite sprite, Color tint, float size)
        {
            var v = new VisualElement();
            v.style.width = size;
            v.style.height = size;
            v.style.flexShrink = 0;
            v.style.backgroundImage = new StyleBackground(sprite);
            v.style.unityBackgroundImageTintColor = tint;
            v.pickingMode = PickingMode.Ignore;
            return v;
        }

        /// <summary>An icon layered inside a box at an offset (fractions of
        /// the box) and scale -- for composite icons and badges.</summary>
        public static VisualElement Layer(VisualElement box, Sprite sprite, Color tint, float boxSize,
                                          float scale = 1f, float dx = 0f, float dy = 0f)
        {
            float s = boxSize * scale;
            var v = Icon(sprite, tint, s);
            v.style.position = Position.Absolute;
            v.style.left = (boxSize - s) * 0.5f + dx * boxSize;
            v.style.top = (boxSize - s) * 0.5f + dy * boxSize;
            box.Add(v);
            return v;
        }

        /// <summary>A unit portrait: its sprite in a pale disc with an ink ring.</summary>
        public static VisualElement Portrait(Sprite sprite, Color tint, float size, out VisualElement image)
        {
            var disc = new VisualElement();
            disc.style.width = size; disc.style.height = size;
            disc.style.flexShrink = 0;
            disc.style.backgroundColor = PortraitBg;
            SetBorderWidth(disc, 2);
            SetBorderColor(disc, Edge);
            SetRadius(disc, (int)(size * 0.5f));
            disc.style.alignItems = Align.Center;
            disc.style.justifyContent = Justify.Center;
            image = Icon(sprite, tint, size * 0.82f);
            disc.Add(image);
            disc.pickingMode = PickingMode.Ignore;
            return disc;
        }

        /// <summary>The price chip: a bolt and a number. Gold when
        /// affordable, grey with a red number when not; an optional thin bar
        /// along its bottom shows how close the balance is to the price.</summary>
        public static VisualElement Chip(out Label price, out VisualElement progress)
        {
            var chip = new VisualElement();
            chip.style.flexDirection = FlexDirection.Row;
            chip.style.alignItems = Align.Center;
            chip.style.justifyContent = Justify.Center;
            chip.style.height = 22;
            chip.style.paddingLeft = 4; chip.style.paddingRight = 8;
            chip.style.backgroundColor = Gold;
            SetBorderWidth(chip, 2);
            SetBorderColor(chip, Edge);
            SetRadius(chip, 10);
            chip.style.overflow = Overflow.Hidden;
            chip.Add(Icon(SpriteShapes.IconBolt, Gold, 16));
            price = Text("0", 13, Ink, Weight.Black);
            price.style.marginLeft = 2;
            chip.Add(price);

            progress = new VisualElement();
            progress.style.position = Position.Absolute;
            progress.style.left = 0; progress.style.bottom = 0;
            progress.style.height = 2;
            progress.style.backgroundColor = new Color(Ink.r, Ink.g, Ink.b, 0.45f);
            progress.style.width = 0;
            progress.pickingMode = PickingMode.Ignore;
            chip.Add(progress);
            chip.pickingMode = PickingMode.Ignore;
            return chip;
        }

        public static void SetChip(VisualElement chip, Label price, VisualElement progress,
                                   int cost, int balance)
        {
            bool ok = balance >= cost;
            price.text = cost.ToString();
            price.style.color = ok ? Ink : Bad;
            chip.style.backgroundColor = ok ? Gold : ChipDim;
            if (progress != null)
                progress.style.width = ok ? Length.Percent(0) : Length.Percent(100f * Mathf.Clamp01(cost > 0 ? (float)balance / cost : 1f));
        }

        /// <summary>The chunky action button: accent fill, ink edge, a dark
        /// "lip" as its bottom edge, white heavy text with an ink outline.</summary>
        public static Button ChunkyButton(string label, System.Action onClick, Sprite icon = null, int px = 13)
        {
            var b = new Button(onClick) { text = "" };
            b.style.flexDirection = FlexDirection.Row;
            b.style.alignItems = Align.Center;
            b.style.justifyContent = Justify.Center;
            b.style.backgroundColor = Accent;
            SetBorderWidth(b, 2);
            b.style.borderBottomWidth = 5;
            SetBorderColor(b, Edge);
            b.style.borderBottomColor = AccentLip;
            SetRadius(b, 10);
            b.style.paddingLeft = b.style.paddingRight = 3 * S;
            b.style.paddingTop = b.style.paddingBottom = S;
            b.style.marginLeft = b.style.marginRight = b.style.marginTop = b.style.marginBottom = 0;
            if (icon != null)
            {
                var i = Icon(icon, Color.white, px + 4);
                i.style.marginRight = 6;
                b.Add(i);
            }
            var t = Text(label, px, Color.white, Weight.Black);
            Outline(t);
            t.name = "label";
            b.Add(t);
            return b;
        }

        public static void SetButtonText(Button b, string text)
        {
            var l = b.Q<Label>("label");
            if (l != null) l.text = text;
        }

        public static void SetButtonEnabled(Button b, bool enabled)
        {
            b.SetEnabled(enabled);
            b.style.backgroundColor = enabled ? Accent : ChipDim;
            b.style.borderBottomColor = enabled ? AccentLip : InkDim;
            b.style.opacity = 1f;
        }

        /// <summary>A small round icon button (close).</summary>
        public static Button IconButton(Sprite icon, System.Action onClick, float size = 26)
        {
            var b = new Button(onClick) { text = "" };
            b.style.width = size; b.style.height = size;
            b.style.backgroundColor = TileFill;
            SetBorderWidth(b, 2);
            SetBorderColor(b, Edge);
            SetRadius(b, (int)(size * 0.5f));
            b.style.paddingLeft = b.style.paddingRight = b.style.paddingTop = b.style.paddingBottom = 0;
            b.style.marginLeft = b.style.marginRight = b.style.marginTop = b.style.marginBottom = 0;
            b.style.alignItems = Align.Center;
            b.style.justifyContent = Justify.Center;
            b.Add(Icon(icon, Ink, size * 0.55f));
            return b;
        }

        /// <summary>A label tag: a card-coloured lozenge, ink edge, heavy caps.</summary>
        public static VisualElement Tag(string text, int px = 11)
        {
            var v = new VisualElement();
            v.style.backgroundColor = CardFill;
            SetBorderWidth(v, 2);
            SetBorderColor(v, Edge);
            SetRadius(v, 8);
            v.style.paddingLeft = v.style.paddingRight = 6;
            v.style.paddingTop = 1; v.style.paddingBottom = 1;
            v.Add(Text(text, px, Ink, Weight.Black, upper: true));
            v.pickingMode = PickingMode.Ignore;
            return v;
        }

        /// <summary>The "designed, not built" hatch, as a tiled background
        /// (spec §4.4) -- one rule across shop tiles, knowledge pips and ribbons.</summary>
        public static void ApplyHatch(VisualElement v, float tile = 12f)
        {
            v.style.backgroundImage = new StyleBackground(SpriteShapes.HatchTile);
            v.style.unityBackgroundImageTintColor = Hatch;
            v.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
            v.style.backgroundSize = new BackgroundSize(tile, tile);
        }

        /// <summary>A small hatched pill with a caption ("PLANNED").</summary>
        public static VisualElement PlannedPill(string text = "PLANNED", int px = 9)
        {
            var p = new VisualElement();
            p.style.backgroundColor = TileFill;
            SetBorderWidth(p, 2);
            SetBorderColor(p, Edge);
            SetRadius(p, 7);
            p.style.paddingLeft = p.style.paddingRight = 5;
            p.style.alignSelf = Align.Center;
            ApplyHatch(p, 8f);
            var t = Text(text, px, Ink, Weight.Black, upper: true);
            p.Add(t);
            p.pickingMode = PickingMode.Ignore;
            return p;
        }

        // ---- level pips ----

        /// <summary>Builds <paramref name="total"/> 12 px pips once.</summary>
        public static VisualElement[] Pips(VisualElement host, int total, float size = 12f)
        {
            var pips = new VisualElement[total];
            for (int i = 0; i < total; i++)
            {
                var p = new VisualElement();
                p.style.width = size; p.style.height = size;
                p.style.marginRight = 3;
                SetBorderWidth(p, 2);
                SetBorderColor(p, Edge);
                SetRadius(p, (int)(size * 0.5f));
                p.style.backgroundColor = TileFill;
                p.pickingMode = PickingMode.Ignore;
                host.Add(p);
                pips[i] = p;
            }
            return pips;
        }

        public static void SetPips(VisualElement[] pips, int filled)
        {
            for (int i = 0; i < pips.Length; i++)
                pips[i].style.backgroundColor = i < filled ? Accent : TileFill;
        }

        // ---- style helpers ----

        public static void SetBorderWidth(VisualElement v, float w)
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

        public static void SetScale(VisualElement v, float s) =>
            v.style.scale = new Scale(new Vector3(s, s, 1f));

        /// <summary>A 0→1→0 "pop" curve over <paramref name="t01"/>, peaking at <paramref name="peak"/>.</summary>
        public static float Pop(float t01, float peak) =>
            t01 <= 0f || t01 >= 1f ? 1f : 1f + (peak - 1f) * Mathf.Sin(Mathf.PI * t01);
    }

    /// <summary>
    /// The upgrade panel's row (Sprint 21 restyle): name, effect, level
    /// pips, price chip, and a chunky BUY -- or MAX when capped.
    ///
    /// A class because the views poll (no events on AtpWallet /
    /// BoneMarrowManager): structure is built once, then re-priced each
    /// frame with an early-out unless something moved.
    /// </summary>
    internal sealed class BuyRow
    {
        public readonly VisualElement Root;

        private readonly VisualElement[] pips;
        private readonly VisualElement chip;
        private readonly Label price;
        private readonly VisualElement progress;
        private readonly Label maxed;
        private readonly Button buy;
        private readonly int cap;

        private int lastLevels = -1, lastPrice = -1, lastBalance = int.MinValue;

        public BuyRow(string name, string effect, int cap, System.Action onBuy)
        {
            this.cap = cap;
            Root = new VisualElement();
            Root.style.marginBottom = UiTheme.S;

            Root.Add(UiTheme.Text(name, 13, UiTheme.Ink, Weight.Bold));
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

            var pipHost = new VisualElement();
            pipHost.style.flexDirection = FlexDirection.Row;
            pips = UiTheme.Pips(pipHost, Mathf.Max(0, cap));
            line.Add(pipHost);

            var right = new VisualElement();
            right.style.flexDirection = FlexDirection.Row;
            right.style.alignItems = Align.Center;
            chip = UiTheme.Chip(out price, out progress);
            chip.style.marginRight = 2 * UiTheme.S;
            right.Add(chip);
            maxed = UiTheme.Text("MAX", 13, UiTheme.InkDim, Weight.Black);
            UiTheme.Show(maxed, false);
            right.Add(maxed);
            buy = UiTheme.ChunkyButton("BUY", onBuy);
            right.Add(buy);
            line.Add(right);
            Root.Add(line);
        }

        public void Refresh(int levels, int cost, int balance)
        {
            if (levels == lastLevels && cost == lastPrice && balance == lastBalance) return;
            bool isMaxed = cap > 0 && levels >= cap;
            if (levels != lastLevels) UiTheme.SetPips(pips, levels);
            UiTheme.Show(chip, !isMaxed);
            UiTheme.Show(buy, !isMaxed);
            UiTheme.Show(maxed, isMaxed);
            if (!isMaxed)
            {
                UiTheme.SetChip(chip, price, progress, cost, balance);
                UiTheme.SetButtonEnabled(buy, balance >= cost);
            }
            lastLevels = levels; lastPrice = cost; lastBalance = balance;
        }
    }
}
