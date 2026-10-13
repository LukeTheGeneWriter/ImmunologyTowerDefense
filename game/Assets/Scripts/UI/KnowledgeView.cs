using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 21: the pathogen knowledge HUD (docs/COSMETIC_PASS_SPEC.md §4)
    /// -- the first player-facing view of GAME_DESIGN.md §5's knowledge
    /// ladder, which until now only showed in the backtick debug readout.
    ///
    /// **Always on** in the bottom-left gutter, under the lymph node where
    /// knowledge is made: one card per species with its icon (the pathogen's
    /// own sprite and colour), its knowledge %, and a ladder bar -- six
    /// rung pips at their thresholds, the next one pulsing, and the 90%
    /// capstone star. Click a card to expand the full ladder above the
    /// strip (Esc, a second click, or a round starting closes it).
    ///
    /// **Feedback, polled via <see cref="KnowledgeWatcher"/>:**
    ///  - a match sends a "+3%" token flying from the lymph node to the card;
    ///    on arrival the number pops, the bar eases up, the card edge flashes
    ///    knowledge-cyan;
    ///  - a rung unlock bursts its pip, shows a ribbon above the strip and
    ///    plays the knowledge ring at the node.
    ///
    /// **Honesty rule (Director, 2026-10-14):** no rung's capability is
    /// built yet, so a reached rung shows the "designed, not built" hatch
    /// and "learned · planned" -- the same hatch as placeholder shop tiles.
    /// <c>KnowledgeLadder.Rung.Built</c> flips it to solid.
    ///
    /// Everything is pre-built; per frame it writes only left/top/scale/
    /// opacity/width on existing elements. No allocation per frame.
    /// </summary>
    internal sealed class KnowledgeView
    {
        public readonly VisualElement Root;
        public VisualElement Expanded => expanded;

        private const int SpeciesCount = KnowledgeWatcher.Species;
        private const float TrackW = 160f;
        private const int TokenPool = 6;
        private const int RibbonQueue = 8;

        private static readonly string[] Names = { "Virus", "Intracellular bacterium", "Large bacterium" };
        private static readonly string[] PctText = BuildPct();

        private static readonly (string name, string effect)[] RungInfo =
        {
            ("Cytotoxic T cells", "Kill an infected cell cleanly: no collateral."),
            ("Neutralising antibodies", "A known pathogen sticks to the gut wall less."),
            ("Memory T cells", "A burst of killer T cells when it returns."),
            ("Fc receptor", "Antibodies tag it for your innate cells."),
            ("Complement", "Coated pathogens are damaged with no cell present."),
            ("Secretory IgA", "Antibody in the lumen, before it can land."),
        };
        private const string CapstoneName = "Specific sensing (capstone)";
        private const string CapstoneEffect = "Coated pathogens become beacons: search is solved.";

        private static string[] BuildPct()
        {
            var a = new string[101];
            for (int i = 0; i <= 100; i++) a[i] = i + "%";
            return a;
        }

        private sealed class Card
        {
            public VisualElement Root, Fill, Star;
            public Label Pct, Next;
            public VisualElement[] Pips = new VisualElement[KnowledgeLadder.Rungs.Length];
            public float Shown, Target;
            public int LastPctInt = -1, LastUnlocked = -1, NextRung = -2;
            public float NumPop = -1f, Flash = -1f;
            public float[] PipBurst = new float[KnowledgeLadder.Rungs.Length];
        }

        private sealed class Token
        {
            public Label Label;
            public int Species;
            public float T = -1f;
            public Vector2 From;
            public bool Pending;   // a delta waiting to be applied on arrival
            public float Delta;
        }

        private readonly Card[] cards = new Card[SpeciesCount];
        private readonly Token[] tokens = new Token[TokenPool];
        private readonly VisualElement floatLayer;
        private readonly VisualElement expanded;
        private readonly Label expandedTitle;
        private readonly (VisualElement row, VisualElement pip, Label pill)[] expandedRows =
            new (VisualElement, VisualElement, Label)[KnowledgeLadder.Rungs.Length + 1];
        private readonly VisualElement ribbon;
        private readonly VisualElement ribbonStripe;
        private readonly Label ribbonTop, ribbonName;
        private readonly VisualElement ribbonPlanned;

        private readonly (int species, int rung)[] ribbonQueue = new (int, int)[RibbonQueue];
        private int ribbonHead, ribbonCount;
        private float ribbonT = -1f;

        private readonly KnowledgeWatcher watcher = new KnowledgeWatcher();
        private KnowledgeLedger ledger;
        private Vector3 nodeWorld;
        private float flashSize;
        private int expandedSpecies = -1;
        private float clock;

        public KnowledgeView(VisualElement parent, VisualElement floatLayer)
        {
            this.floatLayer = floatLayer;

            Root = UiTheme.Card(8);
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.alignItems = Align.Center;
            Root.style.marginLeft = 24;
            Root.style.marginBottom = 14;
            parent.Add(Root);

            var header = new VisualElement();
            header.style.width = 26;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.Center;
            var h = UiTheme.Text("KNOWLEDGE", 12, UiTheme.InkDim, Weight.Black);
            h.style.rotate = new Rotate(new Angle(-90f, AngleUnit.Degree));
            header.Add(h);
            Root.Add(header);

            for (int s = 0; s < SpeciesCount; s++) cards[s] = BuildCard(s);

            // Expanded ladder: above the strip.
            expanded = UiTheme.Card(10);
            expanded.style.position = Position.Absolute;
            expanded.style.left = 0;
            expanded.style.bottom = Length.Percent(100);
            expanded.style.marginBottom = 8;
            expanded.style.width = 470;
            expandedTitle = UiTheme.Text("", 14, UiTheme.Ink, Weight.Black);
            expandedTitle.style.marginBottom = 6;
            expanded.Add(expandedTitle);
            for (int i = 0; i < expandedRows.Length; i++) expandedRows[i] = BuildExpandedRow(i);
            Root.Add(expanded);
            UiTheme.Show(expanded, false);

            // Unlock ribbon: above the strip.
            ribbon = UiTheme.Card(6);
            ribbon.style.position = Position.Absolute;
            ribbon.style.left = 0;
            ribbon.style.bottom = Length.Percent(100);
            ribbon.style.marginBottom = 8;
            ribbon.style.width = 420;
            ribbon.style.flexDirection = FlexDirection.Row;
            ribbon.style.alignItems = Align.Center;
            ribbon.pickingMode = PickingMode.Ignore;
            ribbonStripe = new VisualElement();
            ribbonStripe.style.width = 10;
            ribbonStripe.style.alignSelf = Align.Stretch;
            UiTheme.SetRadius(ribbonStripe, 4);
            ribbon.Add(ribbonStripe);
            var star = UiTheme.Icon(SpriteShapes.IconStar, UiTheme.Gold, 40);
            star.style.marginLeft = 8; star.style.marginRight = 8;
            ribbon.Add(star);
            var rt = new VisualElement();
            rt.style.flexGrow = 1;
            ribbonTop = UiTheme.Text("", 11, UiTheme.InkDim, Weight.Black, upper: true);
            ribbonName = UiTheme.Text("", 17, UiTheme.Ink, Weight.Black);
            rt.Add(ribbonTop); rt.Add(ribbonName);
            ribbon.Add(rt);
            ribbonPlanned = UiTheme.PlannedPill("Planned: no effect yet", 9);
            ribbonPlanned.style.marginRight = 6;
            ribbon.Add(ribbonPlanned);
            Root.Add(ribbon);
            UiTheme.Show(ribbon, false);

            for (int i = 0; i < TokenPool; i++)
            {
                var l = UiTheme.Text("+3%", 13, Color.white, Weight.Black);
                UiTheme.Outline(l, 1f);
                l.style.position = Position.Absolute;
                l.style.width = 52; l.style.height = 24;
                l.style.unityTextAlign = TextAnchor.MiddleCenter;
                UiTheme.SetBorderWidth(l, 2);
                UiTheme.SetBorderColor(l, UiTheme.Edge);
                UiTheme.SetRadius(l, 12);
                l.pickingMode = PickingMode.Ignore;
                floatLayer.Add(l);
                UiTheme.Show(l, false);
                tokens[i] = new Token { Label = l };
            }
        }

        public void Bind(KnowledgeLedger ledger, Vector3 nodeWorld, float flashSize)
        {
            this.ledger = ledger;
            this.nodeWorld = nodeWorld;
            this.flashSize = flashSize;
            if (ledger != null)
            {
                watcher.Prime(ledger);
                for (int s = 0; s < SpeciesCount; s++)
                    cards[s].Shown = cards[s].Target = ledger.Get((ImmunologyTD.Pathogens.PathogenClass)s);
            }
        }

        // ------------------------------------------------------------------
        // Construction
        // ------------------------------------------------------------------

        private static Color SpeciesColor(int s) => s == 0 ? Palette.Virion : Palette.Bacterium;

        private static VisualElement SpeciesIcon(int s, float size)
        {
            var box = new VisualElement();
            box.style.width = size; box.style.height = size;
            box.pickingMode = PickingMode.Ignore;
            switch (s)
            {
                case 0: UiTheme.Layer(box, SpriteShapes.Virion, Palette.Virion, size); break;
                case 1:
                    // "the one that lives in your cells": a lime infected cell with the bacterium in it
                    UiTheme.Layer(box, SpriteShapes.HostCellInfectedBacterial, Palette.HostInfectedBact, size);
                    UiTheme.Layer(box, SpriteShapes.LargeBacterium, Palette.Bacterium, size, 0.55f);
                    break;
                default: UiTheme.Layer(box, SpriteShapes.LargeBacterium, Palette.Bacterium, size); break;
            }
            return box;
        }

        private Card BuildCard(int s)
        {
            var c = new Card();
            c.Root = UiTheme.Tile();
            c.Root.style.width = 186; c.Root.style.height = 120;
            c.Root.style.marginLeft = 6;
            c.Root.style.paddingLeft = c.Root.style.paddingRight = 8;
            c.Root.style.paddingTop = c.Root.style.paddingBottom = 6;
            c.Root.style.justifyContent = Justify.SpaceBetween;
            int species = s;
            c.Root.RegisterCallback<ClickEvent>(_ => ToggleExpanded(species));

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.alignItems = Align.Center;
            var disc = new VisualElement();
            disc.style.width = 42; disc.style.height = 42;
            disc.style.backgroundColor = UiTheme.PortraitBg;
            UiTheme.SetBorderWidth(disc, 2);
            UiTheme.SetBorderColor(disc, UiTheme.Edge);
            UiTheme.SetRadius(disc, 21);
            disc.style.alignItems = Align.Center;
            disc.style.justifyContent = Justify.Center;
            disc.pickingMode = PickingMode.Ignore;
            disc.Add(SpeciesIcon(s, 36));
            top.Add(disc);
            var name = UiTheme.Text(Names[s], 12, UiTheme.Ink, Weight.Black);
            name.style.whiteSpace = WhiteSpace.Normal;
            name.style.flexShrink = 1;
            name.style.marginLeft = 6;
            name.style.width = 70;
            top.Add(name);
            c.Pct = UiTheme.Text("0%", 26, UiTheme.Ink, Weight.Black);
            c.Pct.style.marginLeft = StyleKeyword.Auto;
            top.Add(c.Pct);
            c.Root.Add(top);

            // Ladder track.
            var track = new VisualElement();
            track.style.width = TrackW; track.style.height = 14;
            track.style.alignSelf = Align.Center;
            track.style.backgroundColor = new Color(0.90f, 0.86f, 0.83f);
            UiTheme.SetBorderWidth(track, 2);
            UiTheme.SetBorderColor(track, UiTheme.Edge);
            UiTheme.SetRadius(track, 7);
            track.pickingMode = PickingMode.Ignore;
            var clip = new VisualElement();
            clip.style.position = Position.Absolute;
            clip.style.left = 0; clip.style.right = 0; clip.style.top = 0; clip.style.bottom = 0;
            clip.style.overflow = Overflow.Hidden;
            UiTheme.SetRadius(clip, 5);
            clip.pickingMode = PickingMode.Ignore;
            c.Fill = new VisualElement();
            c.Fill.style.height = Length.Percent(100);
            c.Fill.style.width = 0;
            c.Fill.style.backgroundColor = SpeciesColor(s);
            c.Fill.pickingMode = PickingMode.Ignore;
            clip.Add(c.Fill);
            track.Add(clip);
            var rungs = KnowledgeLadder.Rungs;
            for (int r = 0; r < rungs.Length; r++)
            {
                var pip = new VisualElement();
                pip.style.position = Position.Absolute;
                pip.style.width = 12; pip.style.height = 12;
                pip.style.top = -1;
                pip.style.left = TrackW * rungs[r].ThresholdPercent / 100f - 8f;
                pip.style.backgroundColor = UiTheme.TileFill;
                UiTheme.SetBorderWidth(pip, 2);
                UiTheme.SetBorderColor(pip, UiTheme.Edge);
                UiTheme.SetRadius(pip, 6);
                pip.pickingMode = PickingMode.Ignore;
                track.Add(pip);
                c.Pips[r] = pip;
            }
            c.Star = UiTheme.Icon(SpriteShapes.IconStar, new Color(0.92f, 0.88f, 0.80f), 18);
            c.Star.style.position = Position.Absolute;
            c.Star.style.top = -4;
            c.Star.style.left = TrackW * KnowledgeLadder.CapstonePercent / 100f - 11f;
            track.Add(c.Star);
            c.Root.Add(track);

            c.Next = UiTheme.Text("", 11, UiTheme.InkDim, Weight.Bold);
            c.Root.Add(c.Next);

            Root.Add(c.Root);
            return c;
        }

        private (VisualElement, VisualElement, Label) BuildExpandedRow(int i)
        {
            bool capstone = i >= KnowledgeLadder.Rungs.Length;
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 44;
            var pip = new VisualElement();
            pip.style.width = 16; pip.style.height = 16;
            pip.style.flexShrink = 0;
            UiTheme.SetBorderWidth(pip, 2);
            UiTheme.SetBorderColor(pip, UiTheme.Edge);
            UiTheme.SetRadius(pip, 8);
            pip.style.backgroundColor = UiTheme.TileFill;
            row.Add(pip);
            float th = capstone ? KnowledgeLadder.CapstonePercent : KnowledgeLadder.Rungs[i].ThresholdPercent;
            var pct = UiTheme.Text(Mathf.RoundToInt(th) + "%", 13, UiTheme.InkDim, Weight.Black);
            pct.style.width = 40; pct.style.marginLeft = 8;
            row.Add(pct);
            var text = new VisualElement();
            text.style.flexGrow = 1; text.style.flexShrink = 1;
            text.Add(UiTheme.Text(capstone ? CapstoneName : RungInfo[i].name, 13, UiTheme.Ink, Weight.Black));
            var eff = UiTheme.Text(capstone ? CapstoneEffect : RungInfo[i].effect, 11, UiTheme.InkDim);
            eff.style.whiteSpace = WhiteSpace.Normal;
            text.Add(eff);
            row.Add(text);
            var pill = UiTheme.Text("", 9, UiTheme.Ink, Weight.Black, upper: true);
            pill.style.paddingLeft = pill.style.paddingRight = 6;
            pill.style.paddingTop = pill.style.paddingBottom = 2;
            UiTheme.SetBorderWidth(pill, 2);
            UiTheme.SetBorderColor(pill, UiTheme.Edge);
            UiTheme.SetRadius(pill, 7);
            pill.style.marginLeft = 6;
            row.Add(pill);
            expanded.Add(row);
            return (row, pip, pill);
        }

        // ------------------------------------------------------------------
        // Interaction
        // ------------------------------------------------------------------

        private void ToggleExpanded(int species)
        {
            expandedSpecies = expandedSpecies == species ? -1 : species;
            UiTheme.Show(expanded, expandedSpecies >= 0);
            if (expandedSpecies >= 0) RefreshExpanded();
        }

        public void CloseExpanded()
        {
            expandedSpecies = -1;
            UiTheme.Show(expanded, false);
        }

        private void RefreshExpanded()
        {
            int s = expandedSpecies;
            if (s < 0) return;
            float pct = cards[s].Target;
            expandedTitle.text = $"{Names[s]}  ·  {PctText[Mathf.Clamp(Mathf.FloorToInt(pct), 0, 100)]} known";
            var rungs = KnowledgeLadder.Rungs;
            int next = NextRungIndex(pct);
            for (int i = 0; i < expandedRows.Length; i++)
            {
                var (row, pip, pill) = expandedRows[i];
                bool capstone = i >= rungs.Length;
                float th = capstone ? KnowledgeLadder.CapstonePercent : rungs[i].ThresholdPercent;
                bool reached = pct >= th;
                bool built = !capstone && rungs[i].Built;
                bool isNext = (!capstone && i == next) || (capstone && next < 0 && !reached);

                pip.style.backgroundImage = StyleKeyword.None;
                pip.style.backgroundColor = reached && built ? SpeciesColor(s) : UiTheme.TileFill;
                if (reached && !built) UiTheme.ApplyHatch(pip, 6f);

                pill.style.backgroundImage = StyleKeyword.None;
                if (reached)
                {
                    pill.text = built ? "LEARNED" : "LEARNED · PLANNED";
                    pill.style.backgroundColor = UiTheme.TileFill;
                    pill.style.color = UiTheme.Ink;
                    if (!built) UiTheme.ApplyHatch(pill, 8f);
                    UiTheme.Show(pill, true);
                }
                else if (isNext)
                {
                    pill.text = "NEXT";
                    pill.style.backgroundColor = SpeciesColor(s);
                    pill.style.color = Color.white;
                    UiTheme.Show(pill, true);
                }
                else UiTheme.Show(pill, false);
                row.style.opacity = reached || isNext ? 1f : 0.6f;
            }
        }

        private static int NextRungIndex(float pct)
        {
            var rungs = KnowledgeLadder.Rungs;
            for (int i = 0; i < rungs.Length; i++) if (pct < rungs[i].ThresholdPercent) return i;
            return -1;
        }

        // ------------------------------------------------------------------
        // Per-frame
        // ------------------------------------------------------------------

        public void Refresh(IPanel panel)
        {
            if (ledger == null) return;
            float dt = Time.unscaledDeltaTime;
            clock += dt;

            int n = watcher.Poll(ledger);
            for (int i = 0; i < n; i++)
            {
                var e = watcher.Events[i];
                var c = cards[e.Species];
                switch (e.Kind)
                {
                    case KnowledgeWatcher.Kind.Match:
                        if (!LaunchToken(panel, e.Species, e.Delta)) c.Target += e.Delta;   // pool full: still update
                        break;
                    case KnowledgeWatcher.Kind.Loss:
                        c.Target += e.Delta;
                        c.Pct.style.color = UiTheme.Bad;
                        c.Flash = 0f;
                        break;
                    case KnowledgeWatcher.Kind.Unlock:
                        c.PipBurst[e.RungIndex] = 0.0001f;
                        EnqueueRibbon(e.Species, e.RungIndex);
                        DegranulationFlash.Play(nodeWorld, flashSize * 2f, DegranulationFlash.KnowledgeMatchColor);
                        break;
                }
            }

            StepTokens(panel, dt);
            for (int s = 0; s < SpeciesCount; s++) StepCard(cards[s], s, dt);
            StepRibbon(dt);
        }

        private bool LaunchToken(IPanel panel, int species, float delta)
        {
            if (panel == null || Camera.main == null) return false;
            for (int i = 0; i < tokens.Length; i++)
            {
                var t = tokens[i];
                if (t.T >= 0f) continue;
                t.Species = species;
                t.T = 0f;
                t.Delta = delta;
                t.Pending = true;
                t.From = RuntimePanelUtils.CameraTransformWorldToPanel(panel, nodeWorld, Camera.main);
                t.Label.text = "+" + Mathf.RoundToInt(delta) + "%";
                t.Label.style.backgroundColor = SpeciesColor(species);
                UiTheme.Show(t.Label, true);
                return true;
            }
            return false;
        }

        private void StepTokens(IPanel panel, float dt)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                var t = tokens[i];
                if (t.T < 0f) continue;
                t.T += dt / 0.6f;
                var target = cards[t.Species].Pct.worldBound.center;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t.T));
                Vector2 p = Vector2.Lerp(t.From, target, s) + new Vector2(0f, -60f * Mathf.Sin(Mathf.PI * s));
                t.Label.style.left = p.x - 26f;
                t.Label.style.top = p.y - 12f;
                UiTheme.SetScale(t.Label, Mathf.Lerp(1f, 0.7f, s));
                if (t.T >= 1f)
                {
                    var c = cards[t.Species];
                    if (t.Pending) { c.Target += t.Delta; t.Pending = false; }
                    c.NumPop = 0f;
                    c.Flash = 0f;
                    t.T = -1f;
                    UiTheme.Show(t.Label, false);
                }
            }
        }

        private void StepCard(Card c, int s, float dt)
        {
            if (Mathf.Abs(c.Target - c.Shown) > 0.01f) c.Shown += (c.Target - c.Shown) * Mathf.Min(1f, 10f * dt);
            else c.Shown = c.Target;

            int pi = Mathf.Clamp(Mathf.FloorToInt(c.Shown + 0.0001f), 0, 100);
            if (pi != c.LastPctInt)
            {
                c.Pct.text = PctText[pi];
                c.Fill.style.width = Length.Percent(Mathf.Clamp(c.Shown, 0f, 100f));
                c.LastPctInt = pi;
                if (expandedSpecies == s) RefreshExpanded();
            }
            else c.Fill.style.width = Length.Percent(Mathf.Clamp(c.Shown, 0f, 100f));

            c.Root.style.opacity = c.Target <= 0f ? 0.55f : 1f;   // naive: nothing known yet

            var rungs = KnowledgeLadder.Rungs;
            int unlocked = KnowledgeLadder.UnlockedCount(c.Target);
            int next = NextRungIndex(c.Target);
            if (unlocked != c.LastUnlocked || next != c.NextRung)
            {
                for (int r = 0; r < rungs.Length; r++)
                {
                    var pip = c.Pips[r];
                    bool reached = c.Target >= rungs[r].ThresholdPercent;
                    pip.style.backgroundImage = StyleKeyword.None;
                    pip.style.backgroundColor = reached && rungs[r].Built ? SpeciesColor(s) : UiTheme.TileFill;
                    if (reached && !rungs[r].Built) UiTheme.ApplyHatch(pip, 6f);
                    bool isNext = r == next;
                    pip.style.width = pip.style.height = isNext ? 18 : 12;
                    pip.style.top = isNext ? -4 : -1;
                    pip.style.left = TrackW * rungs[r].ThresholdPercent / 100f - (isNext ? 11f : 8f);
                    UiTheme.SetRadius(pip, isNext ? 9 : 6);
                    UiTheme.SetBorderColor(pip, isNext ? SpeciesColor(s) : UiTheme.Edge);
                    UiTheme.SetBorderWidth(pip, isNext ? 4 : 2);
                }
                c.Star.style.unityBackgroundImageTintColor =
                    c.Target >= KnowledgeLadder.CapstonePercent ? UiTheme.Gold : new Color(0.92f, 0.88f, 0.80f);
                c.Next.text = next >= 0
                    ? $"Next: {rungs[next].ShortName} at {Mathf.RoundToInt(rungs[next].ThresholdPercent)}%"
                    : (c.Target < KnowledgeLadder.CapstonePercent ? $"Next: capstone at {Mathf.RoundToInt(KnowledgeLadder.CapstonePercent)}%" : "Capstone reached");
                c.LastUnlocked = unlocked; c.NextRung = next;
            }

            // The next pip pulses; a freshly unlocked pip bursts.
            for (int r = 0; r < rungs.Length; r++)
            {
                float scale = 1f;
                if (r == next) scale = 1f + 0.06f * (1f + Mathf.Sin(clock * Mathf.PI * 2f / 1.2f));
                if (c.PipBurst[r] > 0f)
                {
                    c.PipBurst[r] += dt / 0.4f;
                    scale = UiTheme.Pop(c.PipBurst[r], 1.8f);
                    if (c.PipBurst[r] >= 1f) c.PipBurst[r] = 0f;
                }
                UiTheme.SetScale(c.Pips[r], scale);
            }

            if (c.NumPop >= 0f)
            {
                c.NumPop += dt / 0.25f;
                UiTheme.SetScale(c.Pct, UiTheme.Pop(c.NumPop, 1.25f));
                if (c.NumPop >= 1f) { c.NumPop = -1f; UiTheme.SetScale(c.Pct, 1f); }
            }
            if (c.Flash >= 0f)
            {
                c.Flash += dt / 0.3f;
                UiTheme.SetBorderColor(c.Root, c.Pct.style.color == UiTheme.Bad ? UiTheme.Bad : Palette.FxKnowledge);
                if (c.Flash >= 1f)
                {
                    c.Flash = -1f;
                    UiTheme.SetBorderColor(c.Root, UiTheme.Edge);
                    c.Pct.style.color = UiTheme.Ink;
                }
            }
        }

        private void EnqueueRibbon(int species, int rung)
        {
            if (ribbonCount >= RibbonQueue) return;
            ribbonQueue[(ribbonHead + ribbonCount) % RibbonQueue] = (species, rung);
            ribbonCount++;
        }

        private void StepRibbon(float dt)
        {
            if (ribbonT < 0f)
            {
                if (ribbonCount == 0) return;
                var (s, r) = ribbonQueue[ribbonHead];
                ribbonHead = (ribbonHead + 1) % RibbonQueue;
                ribbonCount--;
                var rung = KnowledgeLadder.Rungs[r];
                ribbonStripe.style.backgroundColor = SpeciesColor(s);
                ribbonTop.text = $"Learned · {Names[s]} {Mathf.RoundToInt(rung.ThresholdPercent)}%".ToUpperInvariant();
                ribbonName.text = RungInfo[r].name;
                UiTheme.Show(ribbonPlanned, !rung.Built);
                UiTheme.Show(ribbon, true);
                ribbonT = 0f;
            }
            ribbonT += dt;
            float a = ribbonT < 0.2f ? ribbonT / 0.2f : ribbonT < 3.2f ? 1f : 1f - (ribbonT - 3.2f) / 0.4f;
            ribbon.style.opacity = Mathf.Clamp01(a);
            if (ribbonT >= 3.6f) { ribbonT = -1f; UiTheme.Show(ribbon, false); }
        }
    }
}
