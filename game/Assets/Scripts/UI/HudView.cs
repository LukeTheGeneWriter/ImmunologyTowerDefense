using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Economy;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The minimal HUD (UI_DESIGN.md §2), **Sprint 21: one cream card in the
    /// top gutter** (docs/COSMETIC_PASS_SPEC.md §2.7). ATP with a bolt,
    /// lives with a heart, the round number, and on the right the phase's
    /// one action:
    ///  - Building: a chunky START ROUND n button (Space also works);
    ///  - Active: the wave's progress -- "WAVE b / t", how many are in play,
    ///    and a bar;
    ///  - Defeat: GAME OVER.
    /// A caption line under it says the phase's rule (time frozen, or
    /// buying stays open).
    ///
    /// Lives flash red on a loss and stay red with a pulsing heart below a
    /// quarter. Polls; writes only on change.
    /// </summary>
    internal sealed class HudView
    {
        public readonly VisualElement Root;

        private readonly Label atpValue, roundValue, livesValue, caption;
        private readonly VisualElement heart;
        private readonly Button startButton;
        private readonly VisualElement waveBlock, waveFill;
        private readonly Label waveLabel, inPlayLabel, gameOver;
        private readonly System.Action onStartRound;

        private RoundPhase builtForPhase = (RoundPhase)(-1);
        private int lastLives = int.MinValue, lastAtp = int.MinValue, lastRound = int.MinValue;
        private int lastEmitted = -1, lastTarget = -1, lastLive = -1;
        private float livesFlash, heartClock;

        public HudView(VisualElement parent, System.Action onStartRound)
        {
            this.onStartRound = onStartRound;

            Root = UiTheme.Card(8);
            Root.style.width = 600;
            Root.style.marginTop = 14;
            Root.style.marginRight = 24;
            parent.Add(Root);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            Root.Add(row);

            row.Add(Stat(SpriteShapes.IconBolt, UiTheme.Gold, "ATP", out atpValue, out _));
            row.Add(Stat(SpriteShapes.IconHeart, UiTheme.Heart, "LIVES", out livesValue, out heart));
            row.Add(Stat(null, Color.clear, "ROUND", out roundValue, out _));

            var right = new VisualElement();
            right.style.flexGrow = 1;
            right.style.alignItems = Align.FlexEnd;
            row.Add(right);

            startButton = UiTheme.ChunkyButton("START ROUND 1", () => onStartRound?.Invoke(), SpriteShapes.IconPlay, 16);
            startButton.style.height = 48;
            startButton.style.minWidth = 210;
            right.Add(startButton);

            waveBlock = new VisualElement();
            waveBlock.style.width = 210;
            waveLabel = UiTheme.Text("WAVE 0 / 0", 16, UiTheme.Ink, Weight.Black);
            inPlayLabel = UiTheme.Text("0 in play", 11, UiTheme.InkDim, Weight.Bold);
            var track = new VisualElement();
            track.style.height = 16;
            track.style.marginTop = 3;
            track.style.backgroundColor = UiTheme.TileDim;
            UiTheme.SetBorderWidth(track, 2);
            UiTheme.SetBorderColor(track, UiTheme.Edge);
            UiTheme.SetRadius(track, 8);
            track.style.overflow = Overflow.Hidden;
            waveFill = new VisualElement();
            waveFill.style.height = Length.Percent(100);
            waveFill.style.width = 0;
            waveFill.style.backgroundColor = UiTheme.Accent;
            track.Add(waveFill);
            waveBlock.Add(waveLabel);
            waveBlock.Add(inPlayLabel);
            waveBlock.Add(track);
            right.Add(waveBlock);

            gameOver = UiTheme.Text("GAME OVER", 30, UiTheme.Bad, Weight.Black);
            right.Add(gameOver);

            caption = UiTheme.Text("", 11, UiTheme.InkDim, Weight.Bold);
            caption.style.marginTop = 4;
            caption.style.whiteSpace = WhiteSpace.Normal;
            Root.Add(caption);
        }

        private static VisualElement Stat(Sprite icon, Color tint, string label, out Label value, out VisualElement iconEl)
        {
            var col = new VisualElement();
            col.style.flexDirection = FlexDirection.Row;
            col.style.alignItems = Align.Center;
            col.style.marginRight = 22;
            iconEl = null;
            if (icon != null)
            {
                iconEl = UiTheme.Icon(icon, tint, 30);
                iconEl.style.marginRight = 4;
                col.Add(iconEl);
            }
            var stack = new VisualElement();
            stack.style.alignItems = Align.Center;
            value = UiTheme.Text("0", 30, UiTheme.Ink, Weight.Black);
            stack.Add(value);
            var cap = UiTheme.Text(label, 10, UiTheme.InkDim, Weight.Black, upper: true);
            cap.style.marginTop = -4;
            stack.Add(cap);
            col.Add(stack);
            return col;
        }

        public void Refresh(AtpWallet wallet, RoundController rounds, PathogenSpawner spawner)
        {
            if (wallet == null || rounds == null) return;
            float dt = Time.unscaledDeltaTime;

            if (wallet.Balance != lastAtp) { atpValue.text = wallet.Balance.ToString(); lastAtp = wallet.Balance; }

            int shownRound = Mathf.Max(1, rounds.Phase == RoundPhase.Building ? rounds.RoundNumber + 1 : rounds.RoundNumber);
            if (shownRound != lastRound)
            {
                roundValue.text = shownRound.ToString();
                UiTheme.SetButtonText(startButton, $"START ROUND {rounds.RoundNumber + 1}");
                lastRound = shownRound;
            }

            int lives = rounds.Lives;
            if (lives != lastLives)
            {
                if (lastLives != int.MinValue && lives < lastLives) livesFlash = 0.15f;
                livesValue.text = lives.ToString();
                lastLives = lives;
            }
            bool low = lives < rounds.MaxLives * 0.25f;
            if (livesFlash > 0f) livesFlash -= dt;
            livesValue.style.color = (livesFlash > 0f || low) ? UiTheme.Bad : UiTheme.Ink;
            if (heart != null)
            {
                if (low) { heartClock += dt; UiTheme.SetScale(heart, 1f + 0.12f * Mathf.Abs(Mathf.Sin(heartClock * 4f))); }
                else UiTheme.SetScale(heart, 1f);
            }

            if (rounds.Phase != builtForPhase)
            {
                ApplyPhase(rounds);
                builtForPhase = rounds.Phase;
            }

            if (rounds.Phase == RoundPhase.Active && spawner != null)
            {
                int e = spawner.BatchEmitted, t = spawner.BatchTarget, l = spawner.LiveCount;
                if (e != lastEmitted || t != lastTarget || l != lastLive)
                {
                    waveLabel.text = $"WAVE {e} / {t}";
                    inPlayLabel.text = $"{l} in play";
                    waveFill.style.width = Length.Percent(t > 0 ? 100f * e / t : 0f);
                    lastEmitted = e; lastTarget = t; lastLive = l;
                }
            }
        }

        private void ApplyPhase(RoundController rounds)
        {
            UiTheme.Show(startButton, rounds.Phase == RoundPhase.Building);
            UiTheme.Show(waveBlock, rounds.Phase == RoundPhase.Active);
            UiTheme.Show(gameOver, rounds.Phase == RoundPhase.Defeat);
            lastEmitted = lastTarget = lastLive = -1;

            switch (rounds.Phase)
            {
                case RoundPhase.Building:
                {
                    string tag = RoundScript.ForRound(rounds.RoundNumber + 1).Tagline;
                    caption.text = (string.IsNullOrEmpty(tag) ? "" : $"“{tag}”  ·  ") + "buy phase, time is frozen  ·  Space";
                    break;
                }
                case RoundPhase.Active:
                    // Sprint 16 (Director): buying stays open and the clock
                    // does not stop for it -- said once, here.
                    caption.text = "Buying stays open — the round does not pause.";
                    break;
                case RoundPhase.Defeat:
                    caption.text = $"{rounds.RoundsCleared} round(s) cleared.";
                    break;
            }
        }
    }
}
