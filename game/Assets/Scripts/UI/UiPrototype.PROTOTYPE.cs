// ============================================================================
//  UiPrototype.PROTOTYPE.cs  --  UNCOMPILED SKETCH, wired nowhere.
//
//  Companion to docs/UI_DESIGN.md (Sprint 16 buy-UI pass). This is the
//  "one view class + the PanelSettings creation + the shared row" sketch
//  the brief asks for -- for the head to review and fold into real
//  game/Assets/Scripts/UI/ files, NOT to ship as-is.
//
//  It has NOT been compiled (no Unity on the authoring machine). It follows
//  the project's from-code style: PanelSettings via ScriptableObject.
//  CreateInstance, the tree assembled in C#, styled with inline style.*
//  against a central UiTheme, no .uxml / .uss asset, no UI Builder.
//
//  Runtime UI Toolkit needs NO package -- com.unity.modules.uielements
//  (1.0.0) is already in game/Packages/manifest.json. The one caveat is a
//  ThemeStyleSheet asset for PanelSettings; see docs/UI_DESIGN.md §7.
//
//  Namespaces used: UnityEngine, UnityEngine.UIElements,
//  ImmunologyTD.Economy, ImmunologyTD.Rounds, ImmunologyTD.Units,
//  ImmunologyTD.Pathogens, ImmunologyTD.Rendering (SpriteShapes).
// ============================================================================
#if UI_PROTOTYPE_ENABLED   // never defined -- keeps this out of every build

using System;
using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.UI.Prototype
{
    // ------------------------------------------------------------------
    //  UiTheme -- the whole visual language, in one place. Hex + notes
    //  in docs/UI_DESIGN.md §1. Colours checked against UI_STYLE_GUIDE.md
    //  board palettes (ATP gold is deliberately duller than neutrophil
    //  gold; lives-low is oxblood, not fire-red; accent is desaturated
    //  macrophage blue).
    // ------------------------------------------------------------------
    internal static class UiTheme
    {
        public static readonly Color PanelBg  = new Color(0.059f, 0.055f, 0.067f, 0.86f);
        public static readonly Color Ink      = new Color(0.914f, 0.898f, 0.855f);
        public static readonly Color InkDim   = new Color(0.545f, 0.522f, 0.478f);
        public static readonly Color Rule     = new Color(0.914f, 0.898f, 0.855f, 0.14f);
        public static readonly Color Atp      = new Color(0.796f, 0.722f, 0.471f);
        public static readonly Color LivesOk  = new Color(0.510f, 0.663f, 0.627f);
        public static readonly Color LivesLow = new Color(0.753f, 0.361f, 0.263f);
        public static readonly Color Accent   = new Color(0.431f, 0.561f, 0.690f);
        public static readonly Color AccentDim = new Color(0.431f, 0.561f, 0.690f, 0.35f);
        public static readonly Color Defeat   = new Color(0.604f, 0.231f, 0.173f);

        public const int S = 4; // spacing unit, px @ 1920x1080 reference

        public static Label Text(string s, int px, Color c, bool bold = false, bool upper = false)
        {
            var l = new Label(upper ? s.ToUpperInvariant() : s);
            l.style.fontSize = px;
            l.style.color = c;
            l.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            if (upper) l.style.letterSpacing = px * 0.10f;
            l.style.marginTop = 0; l.style.marginBottom = 0;
            l.style.marginLeft = 0; l.style.marginRight = 0;
            return l;
        }

        public static VisualElement Panel()
        {
            var v = new VisualElement();
            v.style.backgroundColor = PanelBg;
            v.style.borderTopWidth = v.style.borderBottomWidth =
                v.style.borderLeftWidth = v.style.borderRightWidth = 1;
            v.style.borderTopColor = v.style.borderBottomColor =
                v.style.borderLeftColor = v.style.borderRightColor = Rule;
            v.style.borderTopLeftRadius = v.style.borderTopRightRadius =
                v.style.borderBottomLeftRadius = v.style.borderBottomRightRadius = 3;
            v.style.paddingTop = v.style.paddingBottom = 3 * S;
            v.style.paddingLeft = v.style.paddingRight = 4 * S;
            v.style.marginTop = v.style.marginRight = 3 * S;
            v.style.minWidth = 300;
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

        // The row shared by the upgrade panel, the shop, and (trimmed) the
        // picker: name + effect text + level dots + cost + buy-state button.
        public static VisualElement Row(
            string name, string effect, int levels, int cap,
            int price, bool affordable, Action onBuy)
        {
            var row = new VisualElement();
            row.style.marginBottom = S;

            row.Add(Text(name, 13, Ink, bold: false));
            if (!string.IsNullOrEmpty(effect))
            {
                var e = Text(effect, 11, InkDim);
                e.style.whiteSpace = WhiteSpace.Normal;
                e.style.marginBottom = S;
                row.Add(e);
            }

            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            line.style.alignItems = Align.Center;
            line.style.justifyContent = Justify.SpaceBetween;

            line.Add(Text(Dots(levels, cap), 11, levels >= cap ? AccentDim : Accent));

            var right = new VisualElement();
            right.style.flexDirection = FlexDirection.Row;
            right.style.alignItems = Align.Center;

            bool maxed = levels >= cap;
            var cost = Text(maxed ? "" : price + " ATP", 11, affordable ? InkDim : LivesLow);
            cost.style.marginRight = 2 * S;
            right.Add(cost);

            if (maxed)
            {
                right.Add(Text("MAX", 13, AccentDim, bold: true));
            }
            else
            {
                var buy = new Button(() => { if (affordable) onBuy?.Invoke(); }) { text = "BUY" };
                buy.style.fontSize = 13;
                buy.style.color = affordable ? Ink : AccentDim;
                buy.style.backgroundColor = PanelBg;
                buy.style.borderLeftWidth = 2;
                buy.style.borderLeftColor = affordable ? Accent : AccentDim;
                buy.style.borderTopWidth = buy.style.borderBottomWidth = buy.style.borderRightWidth = 0;
                buy.style.borderTopLeftRadius = buy.style.borderTopRightRadius =
                    buy.style.borderBottomLeftRadius = buy.style.borderBottomRightRadius = 0;
                buy.style.paddingLeft = buy.style.paddingRight = 3 * S;
                buy.style.paddingTop = buy.style.paddingBottom = S;
                buy.SetEnabled(affordable);
                right.Add(buy);
            }

            line.Add(right);
            row.Add(line);
            return row;
        }

        private static string Dots(int filled, int total)
        {
            var sb = new System.Text.StringBuilder(total);
            for (int i = 0; i < total; i++) sb.Append(i < filled ? '●' : '○'); // ● ○
            return sb.ToString();
        }
    }

    // ------------------------------------------------------------------
    //  UiController -- MonoBehaviour on the "UiRoot" GameObject that
    //  GameBootstrap.BuildUiRoot() creates. Owns the UIDocument, builds
    //  the docks + views, polls the model in Update(). Only HudView is
    //  fleshed out here; the other views are the same shape (see
    //  docs/UI_DESIGN.md §7).
    // ------------------------------------------------------------------
    internal sealed class UiController : MonoBehaviour
    {
        // -- refs, handed in by GameBootstrap.BuildUiRoot() --
        private AtpWallet wallet;
        private RoundController rounds;
        private PathogenSpawner spawner;
        private BoneMarrowManager marrow;
        private ShopLedger shop;

        private UIDocument doc;
        private HudView hud;
        // private UpgradePanelView upgrade;   // same pattern
        // private TowerPickerView  picker;    // same pattern
        // private ShopView         shopView;  // reuses UiTheme.Row over ShopLedger
        // private DebugReadoutView debug;     // ScrollView of monospace Labels

        private bool debugVisible;
        private int lastShopRevision = -1;
        private int? lastSelected;

        public void Bind(AtpWallet wallet, RoundController rounds, PathogenSpawner spawner,
                         BoneMarrowManager marrow, ShopLedger shop)
        {
            this.wallet = wallet; this.rounds = rounds; this.spawner = spawner;
            this.marrow = marrow; this.shop = shop;
        }

        // GameBootstrap builds PanelSettings + UIDocument like this:
        //
        //   var ps = ScriptableObject.CreateInstance<PanelSettings>();
        //   ps.scaleMode = PanelScaleMode.ConstantPhysicalSize;
        //   ps.referenceResolution = new Vector2Int(1920, 1080);
        //   ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        //   ps.match = 0.5f;
        //   ps.sortingOrder = 100;
        //   ps.clearColor = false;
        //   // ps.themeStyleSheet left null -- see docs/UI_DESIGN.md §7 caveat
        //   var go = new GameObject("UiRoot");
        //   var doc = go.AddComponent<UIDocument>();
        //   doc.panelSettings = ps;
        //   go.AddComponent<UiController>().Bind(...);

        private void OnEnable()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore; // never eat a world click

            // top-right dock: the always-on minimal HUD
            var topRight = Dock(root, Align.FlexEnd, Justify.FlexStart);
            hud = new HudView(topRight);

            // right dock under the HUD: upgrade / picker / shop (mutually
            // exclusive) -- omitted from this sketch.
            // bottom-left dock: debug readout, hidden by default -- omitted.
        }

        private static VisualElement Dock(VisualElement parent, Align h, Justify v)
        {
            var d = new VisualElement();
            d.style.position = Position.Absolute;
            d.style.left = 0; d.style.right = 0; d.style.top = 0; d.style.bottom = 0;
            d.style.alignItems = h;
            d.style.justifyContent = v;
            d.pickingMode = PickingMode.Ignore;
            parent.Add(d);
            return d;
        }

        private void Update()
        {
            // Sprint 12 bridge moves here from HudOverlay.Update:
            //   Chemotaxis.SensingUpgradeLevel = shop.LevelOf(ShopItem.CytokineSensingUpgrade);
            // F9 flash-preview passthrough also moves here.

            if (Input.GetKeyDown(KeyCode.BackQuote))   // §10 Q1 -- Director confirms the key
            {
                debugVisible = !debugVisible;
                // debug.Root.hidden = !debugVisible;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
                marrow.ClearSelection();               // §4 -- new selection API

            hud.Refresh(wallet, rounds, spawner);

            // selection change -> swap which right-dock view is visible +
            // move the board rim (marrow.SetSelected). Cheap dirty checks:
            var sel = marrow.SelectedSlotIndex;
            if (sel != lastSelected) { /* swap views, move rim */ lastSelected = sel; }
            if (shop.Revision != lastShopRevision) { /* shopView.Refresh() */ lastShopRevision = shop.Revision; }
        }
    }

    // ------------------------------------------------------------------
    //  HudView -- the top-right stat bar (docs/UI_DESIGN.md §2). Built
    //  once; Refresh() only rewrites Label .text and the lives colour,
    //  and rebuilds the lower block on a phase change.
    // ------------------------------------------------------------------
    internal sealed class HudView
    {
        public readonly VisualElement Root;

        private readonly Label atpValue, roundValue, livesValue;
        private readonly VisualElement lowerBlock;
        private RoundPhase builtForPhase = (RoundPhase)(-1);
        private int livesFlashFrames;

        public HudView(VisualElement parent)
        {
            Root = UiTheme.Panel();
            parent.Add(Root);

            // --- the three stats, in a row ---
            var stats = new VisualElement();
            stats.style.flexDirection = FlexDirection.Row;
            stats.style.justifyContent = Justify.SpaceBetween;

            stats.Add(Stat("ATP",   out atpValue,   UiTheme.Atp));
            stats.Add(Stat("ROUND", out roundValue, UiTheme.Ink));
            stats.Add(Stat("LIVES", out livesValue, UiTheme.LivesOk));
            Root.Add(stats);

            Root.Add(UiTheme.Divider());

            lowerBlock = new VisualElement();
            Root.Add(lowerBlock);
        }

        private static VisualElement Stat(string label, out Label value, Color valueColor)
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;
            col.style.flexGrow = 1;
            col.Add(UiTheme.Text(label, 10, UiTheme.InkDim, bold: true, upper: true));
            value = UiTheme.Text("0", 26, valueColor, bold: true);
            col.Add(value);
            return col;
        }

        public void Refresh(AtpWallet wallet, RoundController rounds, PathogenSpawner spawner)
        {
            atpValue.text = wallet.Balance.ToString();

            int shownRound = Mathf.Max(1,
                rounds.Phase == RoundPhase.Building ? rounds.RoundNumber + 1 : rounds.RoundNumber);
            roundValue.text = shownRound.ToString();

            int newLives = rounds.Lives;
            if (livesValue.text != newLives.ToString())
            {
                // a decrement is the only motion in the HUD -- brief flash
                if (int.TryParse(livesValue.text, out int prev) && newLives < prev)
                    livesFlashFrames = 9; // ~150ms @ 60fps
                livesValue.text = newLives.ToString();
            }
            bool low = newLives < rounds.MaxLives * 0.25f;
            Color target = low ? UiTheme.LivesLow : UiTheme.LivesOk;
            livesValue.style.color = livesFlashFrames-- > 0 ? UiTheme.LivesLow : target;

            if (rounds.Phase != builtForPhase)
            {
                RebuildLower(rounds, spawner);
                builtForPhase = rounds.Phase;
            }
            else if (rounds.Phase == RoundPhase.Active && spawner != null)
            {
                // just the batch line
                if (lowerBlock.childCount >= 2 && lowerBlock[1] is Label batch)
                    batch.text = $"batch {spawner.BatchEmitted} / {spawner.BatchTarget} · {spawner.LiveCount} in play";
            }
        }

        private void RebuildLower(RoundController rounds, PathogenSpawner spawner)
        {
            lowerBlock.Clear();
            switch (rounds.Phase)
            {
                case RoundPhase.Building:
                    lowerBlock.Add(UiTheme.Text("BUY PHASE · time is frozen", 11, UiTheme.InkDim));
                    var tag = RoundScript.ForRound(rounds.RoundNumber + 1).Tagline;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        var t = UiTheme.Text($"“{tag}”", 11, UiTheme.InkDim);
                        t.style.unityFontStyleAndWeight = FontStyle.Italic;
                        lowerBlock.Add(t);
                    }
                    var start = new Button(rounds.StartRound) { text = $"Start Round {rounds.RoundNumber + 1}   ⏎" };
                    start.style.fontSize = 13;
                    start.style.marginTop = 2 * UiTheme.S;
                    start.style.color = UiTheme.Ink;
                    start.style.backgroundColor = UiTheme.PanelBg;
                    start.style.borderLeftWidth = 2;
                    start.style.borderLeftColor = UiTheme.Accent;
                    start.style.borderTopWidth = start.style.borderBottomWidth = start.style.borderRightWidth = 0;
                    start.style.paddingTop = start.style.paddingBottom = 2 * UiTheme.S;
                    lowerBlock.Add(start);
                    break;

                case RoundPhase.Active:
                    lowerBlock.Add(UiTheme.Text("ROUND IN PROGRESS", 11, UiTheme.InkDim));
                    lowerBlock.Add(UiTheme.Text("batch – / –", 11, UiTheme.InkDim));
                    break;

                case RoundPhase.Defeat:
                    lowerBlock.Add(UiTheme.Text("GAME OVER", 26, UiTheme.Defeat, bold: true));
                    break;
            }
        }
    }

    // ------------------------------------------------------------------
    //  Portrait helper for UpgradePanelView / TowerPickerView headers:
    //  a real portrait for free from the existing procedural sprites.
    //
    //  var p = new VisualElement { style = { width = 40, height = 40 } };
    //  p.style.backgroundImage = new StyleBackground(
    //      Background.FromSprite(ImmunologyTD.Rendering.SpriteShapes.Macrophage));
    //  p.style.unityBackgroundImageTintColor = kindColor; // BoneMarrowManager.ColorForKind
    // ------------------------------------------------------------------
}

#endif // UI_PROTOTYPE_ENABLED
