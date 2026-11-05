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
    /// ## Sprint 35: it scrolls, and it is ordered by what you have met
    ///
    /// The ribbon was a fixed 420 px window onto a row of 186 px cards.
    /// Six species already overflowed it and the planned sixteen would be
    /// ~3,000 px -- one of the four gates in front of the roster expansion
    /// (`ROSTER_EXPANSION_SPEC.md` §5.2).
    ///
    /// **Director's design (2026-11-05):** "make the knowledge hub
    /// scrollable with a fade out and have the 5 most recently seen
    /// pathogens occupy the visible spaces. A user can browse this way but
    /// still has the most useful info available." So: a five-card viewport,
    /// the whole roster in the strip behind it, ordered most-recently-seen
    /// first, with the edges fading under the card to say *there is more*.
    /// Nothing is hidden and nothing has to be hunted for.
    ///
    /// **Why the order is snapshotted at phase boundaries.** Ranking
    /// strictly by the newest arrival would reshuffle the ribbon on every
    /// spawn -- during a six-species round the cards would churn every few
    /// hundred milliseconds and the player could not keep their eye on one.
    /// The order is therefore recomputed when the phase changes, or when a
    /// genuinely new organism is met (which is worth an interruption).
    /// Within a round it holds still.
    ///
    /// **Why a hand-rolled viewport and not a ScrollView.** Everything in
    /// this front end is explicitly styled from code with no theme chrome
    /// (`UiController`'s header); a `ScrollView` brings Unity's own
    /// scrollers and a content container styled by the default runtime
    /// theme. Wheel, drag and clamping are ~40 lines here, and the offset
    /// is a plain float a headless check can read.
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

        private static int SpeciesCount => KnowledgeWatcher.Species;
        private const float TrackW = 160f;
        private const int TokenPool = 6;
        private const int RibbonQueue = 8;

        /// <summary>Sprint 25: names come from the species roster, so a new
        /// organism appears in the HUD the moment it is added rather than
        /// needing a parallel list kept in sync by hand.</summary>
        private static string NameOf(int s) =>
            ImmunologyTD.Pathogens.SpeciesRoster.NameOf((ImmunologyTD.Pathogens.PathogenSpecies)s);
        private static readonly string[] PctText = BuildPct();

        /// <summary>
        /// **Sprint 35 fixes a live mislabel here, and removes the shape
        /// that caused it.**
        ///
        /// This was a positional array of SIX entries indexed by rung,
        /// against a ladder of FIVE rungs. Sprint 25 moved Complement off
        /// the knowledge ladder and into the shop -- correctly, it is
        /// structural rather than learned -- and this array was not
        /// updated. Every index from Complement on was therefore one out,
        /// so the **70% rung, which is Secretory IgA, was labelled
        /// "Complement"** in the expanded ladder and in the unlock ribbon.
        ///
        /// That is worth sitting with. `PLAYTEST_LOG.md` has carried an
        /// open question since Sprint 22 about why the Director never
        /// perceived IgA, and `KnowledgeLadder`'s own header already notes
        /// that its *built* flag was left false for a sprint, so the HUD
        /// called IgA "planned" while it was running. The same surface was
        /// also calling it by the wrong name. Whatever else is true about
        /// whether IgA reads in play, the one piece of UI that announces
        /// its arrival has never once said "IgA".
        ///
        /// Keyed off <see cref="KnowledgeCapability"/> now, so a rung
        /// cannot be labelled as its neighbour however the ladder is
        /// reordered, and a new capability fails to compile instead of
        /// shifting everything after it.
        /// </summary>
        private static (string name, string effect) RungInfoFor(KnowledgeCapability c)
        {
            switch (c)
            {
                case KnowledgeCapability.CytotoxicTCells:
                    return ("Cytotoxic T cells", "Kill an infected cell cleanly: no collateral.");
                case KnowledgeCapability.NeutralizingAntibodies:
                    return ("Neutralising antibodies", "A known pathogen sticks to the gut wall less.");
                case KnowledgeCapability.MemoryTCells:
                    return ("Memory T cells", "A burst of killer T cells when it returns.");
                case KnowledgeCapability.FcReceptor:
                    return ("Fc receptor", "Antibodies tag it for your innate cells.");
                case KnowledgeCapability.SecretoryIgA:
                    return ("Secretory IgA", "Antibody in the lumen, before it can land.");
                default:
                    return (c.ToString(), "");
            }
        }

        /// <summary>
        /// Whether a rung's capability can do anything at all against this
        /// species -- **an applicability flag, not a multiplier**
        /// (BACKLOG, from the Giardia antigen pass).
        ///
        /// Two rungs act on an INFECTED CELL rather than on the organism:
        /// the cytotoxic-T rung kills one, and the memory-T rung summons
        /// cells to kill one. *Vibrio cholerae* never enters a cell -- it
        /// adheres and secretes, and the enterocyte lives -- so both rungs
        /// are inert against it, and the player can currently spend a whole
        /// run's learning on them and see nothing.
        ///
        /// This is the dead-feature pattern caught one layer earlier than
        /// usual: the row is not broken, it is simply *not applicable*, and
        /// an upgrade that silently does nothing is exactly what this
        /// project keeps finding. Saying so is cheap and honest. It also
        /// pre-answers two of the ten proposed organisms -- *Giardia* never
        /// infects a cell either, and *Cryptosporidium*'s infected cells
        /// are survivin-protected against apoptosis.
        /// </summary>
        private static bool RungApplies(KnowledgeCapability c, int species)
        {
            bool needsInfectedCell =
                c == KnowledgeCapability.CytotoxicTCells || c == KnowledgeCapability.MemoryTCells;
            if (!needsInfectedCell) return true;
            return !ImmunologyTD.Pathogens.SpeciesRoster
                .Get((ImmunologyTD.Pathogens.PathogenSpecies)species).NeverEntersCells;
        }
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

        /// <summary>One card plus its left margin -- the pitch of the
        /// strip, and the unit scrolling moves in.</summary>
        private const float SlotW = 192f;

        /// <summary>The Director's five (2026-11-05).</summary>
        private const int VisibleSlots = 5;

        private readonly Card[] cards = new Card[ImmunologyTD.Pathogens.SpeciesRoster.Count];
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

        // -- Sprint 35: the viewport ------------------------------------
        private readonly VisualElement viewport, strip, fadeLeft, fadeRight;
        private readonly ImmunologyTD.Pathogens.PathogenSpecies[] order =
            new ImmunologyTD.Pathogens.PathogenSpecies[ImmunologyTD.Pathogens.SpeciesRoster.Count];
        private float scroll, scrollTarget;
        private bool dragging;
        private float dragFromX, dragFromScroll;
        private int orderedForPhase = -1, orderedForSeen = -1;
        private bool lastFadeLeft, lastFadeRight;

        public KnowledgeView(VisualElement parent, VisualElement floatLayer)
        {
            this.floatLayer = floatLayer;

            Root = UiTheme.Card(8);
            Root.name = UiNames.KnowledgeCard;
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

            // The viewport clips; the strip slides behind it.
            viewport = new VisualElement();
            viewport.name = UiNames.KnowledgeViewport;
            viewport.style.width = VisibleSlots * SlotW;
            viewport.style.maxWidth = Length.Percent(100);
            viewport.style.height = 132;
            viewport.style.overflow = Overflow.Hidden;
            Root.Add(viewport);

            strip = new VisualElement();
            strip.name = UiNames.KnowledgeStrip;
            strip.style.position = Position.Absolute;
            strip.style.left = 0;
            strip.style.top = 0;
            strip.style.flexDirection = FlexDirection.Row;
            strip.style.alignItems = Align.Center;
            strip.style.height = Length.Percent(100);
            viewport.Add(strip);

            for (int s = 0; s < SpeciesCount; s++) cards[s] = BuildCard(s);

            // The fades sit over the clipped edges, tinted to the card they
            // are cut out of, and are hidden when there is nothing past
            // that edge -- a permanent fade would be decoration, and would
            // say "there is more" when there is not.
            fadeRight = BuildFade(false);
            fadeRight.name = UiNames.KnowledgeFadeRight;
            viewport.Add(fadeRight);
            fadeLeft = BuildFade(true);
            fadeLeft.name = UiNames.KnowledgeFadeLeft;
            viewport.Add(fadeLeft);
            // Hidden, to match lastFadeLeft/lastFadeRight starting false.
            // A cache of "what is on screen" that disagrees with what IS
            // on screen turns into a write that never happens: the left
            // fade was visible from construction and the first StepScroll
            // saw no change to make, so a ribbon sitting at its start
            // would have claimed there was more behind you.
            UiTheme.Show(fadeRight, false);
            UiTheme.Show(fadeLeft, false);

            viewport.RegisterCallback<WheelEvent>(OnWheel);
            viewport.RegisterCallback<PointerDownEvent>(OnPointerDown);
            viewport.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            viewport.RegisterCallback<PointerUpEvent>(OnPointerUp);

            ReorderStrip();

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
                    cards[s].Shown = cards[s].Target = ledger.Get((ImmunologyTD.Pathogens.PathogenSpecies)s);
            }
        }

        // ------------------------------------------------------------------
        // Construction
        // ------------------------------------------------------------------

        /// <summary>Sprint 35: one shade per species, within its family's
        /// hue (<see cref="Palette.ForSpecies"/>). Was per CLASS, which
        /// made every bacterium the same green.</summary>
        private static Color SpeciesColor(int s) =>
            Palette.ForSpecies((ImmunologyTD.Pathogens.PathogenSpecies)s);

        /// <summary>
        /// **Sprint 35 fixes a live bug here.** This switched on
        /// <c>case 0 / case 1 / default</c>, so every species from index 2
        /// up drew as a bacterium -- and rotavirus is index 3. The
        /// knowledge HUD has been showing the player a green rod labelled
        /// "Rotavirus" since Sprint 25.
        ///
        /// It now goes through the same per-species dispatch the board
        /// uses, so the card and the thing on the board are the same
        /// drawing by construction rather than by two lists agreeing.
        /// </summary>
        private static VisualElement SpeciesIcon(int s, float size)
        {
            var box = new VisualElement();
            box.style.width = size; box.style.height = size;
            box.pickingMode = PickingMode.Ignore;
            var species = (ImmunologyTD.Pathogens.PathogenSpecies)s;

            // The one species whose icon is not its board sprite: an
            // intracellular bacterium's whole point is that it is inside a
            // cell, so the card shows the cell with it in it (§4a's "visible
            // as the host cell, not itself").
            if (species == ImmunologyTD.Pathogens.PathogenSpecies.GenericIntracellularBacterium)
            {
                UiTheme.Layer(box, SpriteShapes.HostCellInfectedBacterial, Palette.HostInfectedBact, size);
                UiTheme.Layer(box, SpriteShapes.LargeBacterium, Palette.Bacterium, size, 0.55f);
                return box;
            }

            UiTheme.Layer(box, SpriteShapes.ForSpecies(species), Palette.ForSpecies(species), size);
            return box;
        }

        private Card BuildCard(int s)
        {
            var c = new Card();
            c.Root = UiTheme.Tile();
            c.Root.name = UiNames.KnowledgeSpecies(s);
            c.Root.style.width = 186; c.Root.style.height = 120;
            c.Root.style.flexShrink = 0;
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
            var name = UiTheme.Text(NameOf(s), 12, UiTheme.Ink, Weight.Black);
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

            strip.Add(c.Root);
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
            var info = capstone ? (CapstoneName, CapstoneEffect) : RungInfoFor(KnowledgeLadder.Rungs[i].Capability);
            text.Add(UiTheme.Text(info.Item1, 13, UiTheme.Ink, Weight.Black));
            var eff = UiTheme.Text(info.Item2, 11, UiTheme.InkDim);
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

        private VisualElement BuildFade(bool mirrored)
        {
            var v = new VisualElement();
            v.style.position = Position.Absolute;
            v.style.top = 0; v.style.bottom = 0;
            v.style.width = 34;
            if (mirrored) v.style.left = 0; else v.style.right = 0;
            v.style.backgroundImage = new StyleBackground(SpriteShapes.FadeToRight);
            v.style.unityBackgroundImageTintColor = UiTheme.CardFill;
            // One texture for both edges: the left one is the same ramp,
            // flipped.
            if (mirrored) v.style.scale = new Scale(new Vector3(-1f, 1f, 1f));
            v.pickingMode = PickingMode.Ignore;
            return v;
        }

        // ------------------------------------------------------------------
        // Sprint 35: order, scroll, fade
        // ------------------------------------------------------------------

        /// <summary>How far the strip can slide before its last card is
        /// flush with the viewport's right edge. Zero when everything
        /// fits.</summary>
        private float MaxScroll => Mathf.Max(0f, SpeciesCount * SlotW - VisibleSlots * SlotW);

        /// <summary>
        /// Puts the cards in display order: most recently seen first, never
        /// seen last (<c>SpeciesSightings.OrderForDisplay</c>, which is a
        /// pure function so the ordering can be asserted without building a
        /// UI tree).
        ///
        /// Reordering is a re-parent, not a rebuild -- the Card objects,
        /// their animation state and their pips are untouched, so a card
        /// that moves while its "+3%" token is in flight keeps its number
        /// and its flash.
        /// </summary>
        private void ReorderStrip()
        {
            int n = ImmunologyTD.Pathogens.SpeciesSightings.OrderForDisplay(order);
            if (n == 0) return;
            for (int i = 0; i < n; i++)
            {
                var card = cards[(int)order[i]];
                if (card != null) strip.Add(card.Root);   // Add re-parents to the end
            }
        }

        /// <summary>Recomputes the order only when it is safe to: at a
        /// phase boundary, or when the player has met an organism they had
        /// never met before. See the class header for why this is not done
        /// on every sighting.</summary>
        private void ReorderIfDue(int phase)
        {
            int seen = ImmunologyTD.Pathogens.SpeciesSightings.DistinctSeen();
            if (phase == orderedForPhase && seen == orderedForSeen) return;
            orderedForPhase = phase;
            orderedForSeen = seen;
            ReorderStrip();
        }

        private void OnWheel(WheelEvent e)
        {
            if (MaxScroll <= 0f) return;
            // One card per notch, whichever axis the device reports on --
            // a trackpad's horizontal swipe and a mouse wheel's vertical
            // notch should both walk the ribbon.
            float d = Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y) ? e.delta.x : e.delta.y;
            scrollTarget = Mathf.Clamp(scrollTarget + Mathf.Sign(d) * SlotW, 0f, MaxScroll);
            e.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            if (MaxScroll <= 0f) return;
            dragging = true;
            dragFromX = e.position.x;
            dragFromScroll = scrollTarget;
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!dragging) return;
            scrollTarget = Mathf.Clamp(dragFromScroll - (e.position.x - dragFromX), 0f, MaxScroll);
            scroll = scrollTarget;   // a drag tracks the finger exactly, no easing
        }

        private void OnPointerUp(PointerUpEvent e) => dragging = false;

        private void StepScroll(float dt)
        {
            float max = MaxScroll;
            scrollTarget = Mathf.Clamp(scrollTarget, 0f, max);
            if (Mathf.Abs(scrollTarget - scroll) > 0.5f)
                scroll += (scrollTarget - scroll) * Mathf.Min(1f, 14f * dt);
            else scroll = scrollTarget;

            strip.style.left = -scroll;

            bool showLeft = scroll > 1f;
            bool showRight = scroll < max - 1f;
            if (showLeft != lastFadeLeft) { UiTheme.Show(fadeLeft, showLeft); lastFadeLeft = showLeft; }
            if (showRight != lastFadeRight) { UiTheme.Show(fadeRight, showRight); lastFadeRight = showRight; }
        }

        /// <summary>The species currently in the viewport's slots, in
        /// order, for a harness: the Director's claim is that the five most
        /// recently seen organisms occupy the visible spaces, and this is
        /// the half of it the view owns.</summary>
        public ImmunologyTD.Pathogens.PathogenSpecies[] VisibleSpecies()
        {
            int first = Mathf.Clamp(Mathf.RoundToInt(scroll / SlotW), 0, Mathf.Max(0, SpeciesCount - 1));
            int n = Mathf.Min(VisibleSlots, SpeciesCount - first);
            var a = new ImmunologyTD.Pathogens.PathogenSpecies[Mathf.Max(0, n)];
            for (int i = 0; i < a.Length; i++) a[i] = order[first + i];
            return a;
        }

        /// <summary>Scroll offset in pixels, and how far it can go -- so a
        /// check can prove the ribbon really is scrollable and really does
        /// clamp, rather than that a field exists.</summary>
        public float ScrollOffset => scroll;
        public float ScrollRange => MaxScroll;

        /// <summary>Drives the ribbon as a wheel notch would, for a
        /// harness: UITK events cannot be synthesised headlessly, and a
        /// scroll nothing can exercise is a scroll nobody has tested.</summary>
        public void KnowledgeScrollBySlots(int slots)
        {
            scrollTarget = Mathf.Clamp(scrollTarget + slots * SlotW, 0f, MaxScroll);
            scroll = scrollTarget;
            StepScroll(1f);
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
            expandedTitle.text = $"{NameOf(s)}  ·  {PctText[Mathf.Clamp(Mathf.FloorToInt(pct), 0, 100)]} known";
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
                if (!capstone && !RungApplies(rungs[i].Capability, s))
                {
                    // Not "locked" and not "planned": there is nothing here
                    // for this organism to be used ON.
                    pill.text = "NO EFFECT ON THIS ONE";
                    pill.style.backgroundColor = UiTheme.ChipDim;
                    pill.style.color = UiTheme.Ink;
                    UiTheme.Show(pill, true);
                    row.style.opacity = 0.45f;
                    continue;
                }
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

        /// <summary><paramref name="phase"/> is the round phase as an int
        /// (<c>RoundPhase</c> lives in another namespace and this view has
        /// no other reason to know about rounds). It is the boundary the
        /// ribbon's ordering is snapshotted at -- see the class
        /// header.</summary>
        public void Refresh(IPanel panel, int phase)
        {
            if (ledger == null) return;
            float dt = Time.unscaledDeltaTime;
            clock += dt;

            ReorderIfDue(phase);
            StepScroll(dt);

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

            // Sprint 35: dimmed means NEVER MET, which is what the
            // player actually wants to know. It used to mean "knowledge is
            // zero" -- a different statement, and wrong for any organism
            // fought repeatedly without ever being sampled, which is
            // exactly the case the knowledge ladder is about.
            bool met = ImmunologyTD.Pathogens.SpeciesSightings.HasSeen(
                (ImmunologyTD.Pathogens.PathogenSpecies)s);
            c.Root.style.opacity = met ? 1f : 0.55f;

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
                ribbonTop.text = $"Learned · {NameOf(s)} {Mathf.RoundToInt(rung.ThresholdPercent)}%".ToUpperInvariant();
                ribbonName.text = RungInfoFor(KnowledgeLadder.Rungs[r].Capability).name;
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
