using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// **Sprint 35: the game tells the player what is coming.**
    ///
    /// ## What was actually wrong
    ///
    /// The Director played Sprint 33's build and reported seeing no round
    /// messages at all. Sprint 33 had shipped them, with 41 green checks,
    /// and the obvious conclusion was a wiring bug: a string written into a
    /// Label that never reached a pixel.
    ///
    /// **It was measured instead of assumed, and it was not that.**
    /// `UiTreeDump` boots the real game, drives a real `RoundController`
    /// through its phases and reads the live visual tree: the caption's
    /// text was correct, unique, attached to the panel root, displayed, and
    /// drawable in the shipped font, in every phase. Nothing was broken.
    ///
    /// The round's identity was simply **11 px of dim grey, in the
    /// top-right corner, under two 30 px numbers, beside a 48 px button,
    /// with four facts run together by middots** -- and in a browser window
    /// narrower than the 1920 px reference, the panel scales that line down
    /// further. A first encounter with an organism the player has never met
    /// was given the smallest type on the screen. The message was there;
    /// nobody was ever going to read it.
    ///
    /// So the fix is not a wire, it is a surface. This is that surface, and
    /// it is `ROSTER_EXPANSION_SPEC.md` §4.2's three-line debut card --
    /// which means the regression and the feature the Director flagged as
    /// "I didn't ask for that part" are one piece of work, not two.
    ///
    /// ## The three lines (§4.2), and why the third is a negative
    ///
    ///  1. **The vehicle** -- the real-world thing a person eats, drinks or
    ///     touches. This is the round's tagline, which already exists for
    ///     all 40 rounds, so there is one source of truth for it.
    ///  2. **The name, with the sprite big.** The only place the player
    ///     ever sees the silhouette large enough to learn it; afterwards
    ///     they must recognise it at 20 px in a crowd, which is what the
    ///     sprite channels in §2 are for.
    ///  3. **The one thing that will not work**, stated as a negative
    ///     (`SpeciesProfile.Warning`). A positive hint ("use IgA") deletes
    ///     the puzzle; a negative one tells the player their habit is about
    ///     to fail without handing them the replacement. It is the Bloons
    ///     lead-bloon convention: *sharp will not pop this*, find the rest
    ///     yourself.
    ///
    /// ## Two banners, one fact each
    ///
    /// §4.5 asks for three surfaces: **the debut card teaches** (here, in
    /// the buy phase, before the organism arrives), **a first-sighting
    /// banner confirms** (here too, smaller, the first time a species
    /// actually appears on the board in a run), and **the knowledge HUD
    /// tracks** (`KnowledgeView`).
    ///
    /// It floats over the board rather than living in a gutter, because it
    /// is temporary and because the gutters are where the permanent
    /// surfaces live (`COSMETIC_PASS_SPEC.md` §2.3). It auto-dismisses --
    /// longer for a debut, since that card is the teaching one and the buy
    /// phase has time frozen anyway -- and a round start always clears it.
    /// </summary>
    internal sealed class RoundBannerView
    {
        public readonly VisualElement Root;
        public readonly VisualElement Sighting;

        /// <summary>How long a plain round's banner stays up. Short: it is
        /// a reminder, and the HUD caption carries the same rule quietly
        /// for the rest of the phase.</summary>
        private const float PlainSeconds = 7f;

        /// <summary>A debut card stays much longer. It is the only place
        /// the player meets the organism at a readable size, time is frozen
        /// in the buy phase, and a click dismisses it early.</summary>
        private const float DebutSeconds = 16f;

        private const float SightingSeconds = 2.6f;
        private const float FadeSeconds = 0.35f;

        private readonly Label roundLine, vehicleLine, nameLine, warningLine;
        private readonly VisualElement portrait, portraitImage, newTag;
        private readonly Label sightingName;
        private readonly VisualElement sightingPortrait, sightingImage;

        private float life = -1f, span;
        private float sightingLife = -1f;
        private RoundPhase lastPhase = (RoundPhase)(-1);
        private int builtForRound = -1;
        private readonly bool[] announced = new bool[SpeciesRoster.Count];

        public RoundBannerView(VisualElement floatLayer)
        {
            Root = UiTheme.Card(14);
            Root.name = UiNames.RoundBanner;
            Root.style.position = Position.Absolute;
            Root.style.width = 620;
            Root.style.maxWidth = Length.Percent(92);
            // Centred horizontally, high enough to clear the board's own
            // detail but below the HUD card's 48 px row.
            Root.style.left = Length.Percent(50);
            Root.style.translate = new Translate(Length.Percent(-50), 0);
            Root.style.top = 132;
            Root.RegisterCallback<ClickEvent>(_ => Dismiss());
            floatLayer.Add(Root);
            UiTheme.Show(Root, false);

            roundLine = UiTheme.Text("", 11, UiTheme.InkDim, Weight.Black, upper: true);
            Root.Add(roundLine);

            vehicleLine = UiTheme.Text("", 22, UiTheme.Ink, Weight.Black);
            vehicleLine.name = UiNames.RoundBannerVehicle;
            vehicleLine.style.whiteSpace = WhiteSpace.Normal;
            vehicleLine.style.marginTop = 2;
            Root.Add(vehicleLine);

            // The debut half: portrait, NEW tag + name, warning. Hidden
            // entirely on a round that introduces nothing, so a quiet round
            // stays quiet (the Sprint 33 rule, kept).
            var debut = new VisualElement();
            debut.style.flexDirection = FlexDirection.Row;
            debut.style.alignItems = Align.Center;
            debut.style.marginTop = 10;
            Root.Add(debut);

            portrait = UiTheme.Portrait(SpriteShapes.LargeBacterium, Palette.Bacterium, 86, out portraitImage);
            portrait.name = UiNames.RoundBannerPortrait;
            portrait.style.marginRight = 12;
            debut.Add(portrait);

            var text = new VisualElement();
            text.style.flexGrow = 1;
            text.style.flexShrink = 1;
            debut.Add(text);

            var nameRow = new VisualElement();
            nameRow.style.flexDirection = FlexDirection.Row;
            nameRow.style.alignItems = Align.Center;
            text.Add(nameRow);
            newTag = UiTheme.Tag("NEW");
            newTag.style.marginRight = 8;
            nameRow.Add(newTag);
            nameLine = UiTheme.Text("", 19, UiTheme.Ink, Weight.Black);
            nameLine.name = UiNames.RoundBannerName;
            nameLine.style.whiteSpace = WhiteSpace.Normal;
            nameLine.style.flexShrink = 1;
            nameRow.Add(nameLine);

            warningLine = UiTheme.Text("", 14, UiTheme.Ink, Weight.Bold);
            warningLine.name = UiNames.RoundBannerWarning;
            warningLine.style.whiteSpace = WhiteSpace.Normal;
            warningLine.style.marginTop = 4;
            text.Add(warningLine);

            this.debutBlock = debut;

            // ---- the in-round first-sighting banner (§4.5) -------------
            Sighting = UiTheme.Card(8);
            Sighting.name = UiNames.SightingBanner;
            Sighting.style.position = Position.Absolute;
            Sighting.style.left = Length.Percent(50);
            Sighting.style.translate = new Translate(Length.Percent(-50), 0);
            Sighting.style.top = 132;
            Sighting.style.flexDirection = FlexDirection.Row;
            Sighting.style.alignItems = Align.Center;
            Sighting.pickingMode = PickingMode.Ignore;
            floatLayer.Add(Sighting);
            UiTheme.Show(Sighting, false);
            sightingPortrait = UiTheme.Portrait(SpriteShapes.LargeBacterium, Palette.Bacterium, 44, out sightingImage);
            sightingPortrait.style.marginRight = 10;
            Sighting.Add(sightingPortrait);
            var stack = new VisualElement();
            stack.Add(UiTheme.Text("FIRST SIGHTING", 9, UiTheme.InkDim, Weight.Black, upper: true));
            sightingName = UiTheme.Text("", 16, UiTheme.Ink, Weight.Black);
            sightingName.name = UiNames.SightingBannerName;
            stack.Add(sightingName);
            Sighting.Add(stack);
        }

        private readonly VisualElement debutBlock;

        /// <summary>Clears the banner now -- a click on it, Esc, or a round
        /// starting.</summary>
        public void Dismiss()
        {
            life = -1f;
            UiTheme.Show(Root, false);
        }

        /// <summary>
        /// Polled, like every other view here.
        ///
        /// The phase edge is detected inside the view rather than pushed in
        /// by `UiController`, so that driving `RefreshViews()` from a
        /// harness reproduces exactly what a frame does -- a view that only
        /// updates when something external remembers to tell it is a view
        /// no headless check can speak for.
        /// </summary>
        public void Refresh(RoundController rounds)
        {
            if (rounds == null) return;

            // **Clamped, and not only for the harness.** One slow frame --
            // a shader compile, a WebGL hitch, the first frame after a
            // scene loads -- can report several seconds of unscaled delta,
            // and an unclamped timer would eat a 16-second debut card
            // whole. A message the player never gets is the exact defect
            // this view exists to fix; it does not get to happen here by
            // accident.
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);

            if (rounds.Phase != lastPhase)
            {
                if (rounds.Phase == RoundPhase.Building) ShowForRound(rounds.RoundNumber + 1);
                else Dismiss();
                lastPhase = rounds.Phase;
            }

            if (life >= 0f)
            {
                life += dt;
                float a = life < FadeSeconds ? life / FadeSeconds
                        : life < span - FadeSeconds ? 1f
                        : 1f - (life - (span - FadeSeconds)) / FadeSeconds;
                Root.style.opacity = Mathf.Clamp01(a);
                if (life >= span) Dismiss();
            }

            StepSighting(rounds, dt);
        }

        /// <summary>Builds the card for the round that is about to be
        /// bought for. Idempotent per round: re-entering the same buy phase
        /// (a harness driving several refreshes) does not restart the
        /// timer, but a new round always does.</summary>
        private void ShowForRound(int roundNumber)
        {
            var def = RoundScript.ForRound(roundNumber);
            var debuts = RoundScript.DebutsInRound(roundNumber);

            roundLine.text = $"Round {roundNumber}  ·  buy phase, time is frozen";
            vehicleLine.text = string.IsNullOrEmpty(def.Tagline)
                ? "Something is coming."
                : def.Tagline;

            bool hasDebut = debuts.Length > 0;
            UiTheme.Show(debutBlock, hasDebut);
            if (hasDebut)
            {
                // One organism at a time is the round script's own rule
                // (Sprint 33), so the card speaks for the first debut and
                // names any others after it rather than pretending to
                // portrait two.
                var first = debuts[0];
                nameLine.text = debuts.Length == 1
                    ? SpeciesRoster.NameOf(first)
                    : SpeciesRoster.NameOf(first) + ", " + SpeciesRoster.NameOf(debuts[1]);
                warningLine.text = SpeciesRoster.WarningOf(first);
                UiTheme.Show(warningLine, !string.IsNullOrEmpty(warningLine.text));
                SetPortrait(portraitImage, first);
            }

            builtForRound = roundNumber;
            span = hasDebut ? DebutSeconds : PlainSeconds;
            life = 0f;
            Root.style.opacity = 0f;
            UiTheme.Show(Root, true);
        }

        private static void SetPortrait(VisualElement image, PathogenSpecies species)
        {
            image.style.backgroundImage = new StyleBackground(SpriteShapes.ForSpecies(species));
            image.style.unityBackgroundImageTintColor = Palette.ForSpecies(species);
        }

        /// <summary>
        /// §4.5: the first time a species actually arrives on the board in
        /// this run, confirm it. The debut card promised it a round ago;
        /// this is the "there it is", and it carries the sprite and the
        /// name only -- one fact per surface.
        ///
        /// Driven by <see cref="SpeciesSightings"/> rather than by the
        /// spawner, because the question "has the player ever met this"
        /// outlives any one spawn and three surfaces now ask it.
        /// </summary>
        private void StepSighting(RoundController rounds, float dt)
        {
            if (sightingLife >= 0f)
            {
                sightingLife += dt;
                float a = sightingLife < FadeSeconds ? sightingLife / FadeSeconds
                        : sightingLife < SightingSeconds - FadeSeconds ? 1f
                        : 1f - (sightingLife - (SightingSeconds - FadeSeconds)) / FadeSeconds;
                Sighting.style.opacity = Mathf.Clamp01(a);
                if (sightingLife >= SightingSeconds) { sightingLife = -1f; UiTheme.Show(Sighting, false); }
                return;
            }

            // Deliberately NOT gated on SpeciesSightings.Revision. A
            // revision early-out would strand the last species of a run: if
            // nothing else ever spawns, the revision never moves again and
            // the pending announcement is never picked up. Scanning 16
            // bools on a poll costs nothing, and "cheap" is not worth a
            // surface that silently skips its final case.
            if (rounds.Phase != RoundPhase.Active) return;

            for (int s = 0; s < announced.Length; s++)
            {
                var species = (PathogenSpecies)s;
                if (announced[s] || !SpeciesSightings.HasSeen(species)) continue;
                announced[s] = true;
                sightingName.text = SpeciesRoster.NameOf(species);
                SetPortrait(sightingImage, species);
                sightingLife = 0f;
                Sighting.style.opacity = 0f;
                UiTheme.Show(Sighting, true);
                return;   // one at a time; the rest keep their turn
            }
        }

        /// <summary>Which round the card on screen was built for, or -1.
        /// Read by the harness: "the banner says what the next round is" is
        /// the claim, and the round number is half of it.</summary>
        public int BuiltForRound => builtForRound;
    }
}
