using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Economy;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The minimal player HUD (docs/UI_DESIGN.md §2): ATP, round, lives,
    /// and the one control the player needs -- start the round. Top-right.
    ///
    /// This is the direct answer to the Director's Sprint 15 note ("it has
    /// so many stats I can't get a feel for how the player will interact
    /// with the game"). Everything that used to sit in the top-left dump is
    /// now in DebugReadoutView, behind a key, off by default. The rule for
    /// what earns a place here: a *readout* only qualifies if the player
    /// makes a decision from it every few seconds. ATP (can I buy?), lives
    /// (am I losing?), round (how deep am I?) qualify; population counts,
    /// per-band pathogen tallies, the knowledge ladder and the frame cost
    /// do not.
    ///
    /// Built once. Refresh() only rewrites Label text and the lives colour,
    /// and rebuilds the lower block on an actual phase change -- the views
    /// poll every frame (there are no model events), so nothing here may
    /// allocate per frame.
    /// </summary>
    internal sealed class HudView
    {
        public readonly VisualElement Root;

        private readonly Label atpValue, roundValue, livesValue;
        private readonly VisualElement lowerBlock;

        private RoundPhase builtForPhase = (RoundPhase)(-1);
        private Label batchLabel;
        private int lastLives = int.MinValue;
        private int livesFlashFrames;

        public HudView(VisualElement parent, System.Action onStartRound)
        {
            Root = UiTheme.Panel();
            Root.style.minWidth = 300;
            parent.Add(Root);

            var stats = new VisualElement();
            stats.style.flexDirection = FlexDirection.Row;
            stats.style.justifyContent = Justify.SpaceBetween;
            stats.Add(Stat("ATP", out atpValue, UiTheme.Atp));
            stats.Add(Stat("ROUND", out roundValue, UiTheme.Ink));
            stats.Add(Stat("LIVES", out livesValue, UiTheme.LivesOk));
            Root.Add(stats);

            Root.Add(UiTheme.Divider());

            lowerBlock = new VisualElement();
            Root.Add(lowerBlock);

            this.onStartRound = onStartRound;
        }

        private readonly System.Action onStartRound;

        private static VisualElement Stat(string label, out Label value, Color valueColor)
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;
            col.style.flexGrow = 1;
            col.Add(UiTheme.Text(label, 10, UiTheme.InkDim, bold: true, upper: true));
            value = UiTheme.Text("0", 26, valueColor, bold: true);
            value.style.marginTop = UiTheme.S / 2;
            col.Add(value);
            return col;
        }

        public void Refresh(AtpWallet wallet, RoundController rounds, PathogenSpawner spawner)
        {
            if (wallet == null || rounds == null) return;

            atpValue.text = wallet.Balance.ToString();

            int shownRound = Mathf.Max(1,
                rounds.Phase == RoundPhase.Building ? rounds.RoundNumber + 1 : rounds.RoundNumber);
            roundValue.text = shownRound.ToString();

            int lives = rounds.Lives;
            if (lives != lastLives)
            {
                // The HUD is otherwise motionless, so one brief flash on a
                // decrement is enough to make a life loss impossible to miss
                // without any moving chrome.
                if (lastLives != int.MinValue && lives < lastLives) livesFlashFrames = 9;
                livesValue.text = lives.ToString();
                lastLives = lives;
            }
            bool low = lives < rounds.MaxLives * 0.25f;
            if (livesFlashFrames > 0) livesFlashFrames--;
            livesValue.style.color = livesFlashFrames > 0
                ? UiTheme.Ink
                : (low ? UiTheme.LivesLow : UiTheme.LivesOk);

            if (rounds.Phase != builtForPhase)
            {
                RebuildLower(rounds);
                builtForPhase = rounds.Phase;
            }
            else if (rounds.Phase == RoundPhase.Active && batchLabel != null && spawner != null)
            {
                batchLabel.text = $"batch {spawner.BatchEmitted} / {spawner.BatchTarget} · {spawner.LiveCount} in play";
            }
        }

        private void RebuildLower(RoundController rounds)
        {
            lowerBlock.Clear();
            batchLabel = null;

            switch (rounds.Phase)
            {
                case RoundPhase.Building:
                {
                    lowerBlock.Add(UiTheme.Text("BUY PHASE · TIME IS FROZEN", 10, UiTheme.InkDim, bold: true, upper: true));
                    string tag = RoundScript.ForRound(rounds.RoundNumber + 1).Tagline;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        var t = UiTheme.Text($"“{tag}”", 11, UiTheme.InkDim);
                        t.style.whiteSpace = WhiteSpace.Normal;
                        t.style.marginTop = UiTheme.S;
                        lowerBlock.Add(t);
                    }
                    var start = UiTheme.FlatButton($"Start Round {rounds.RoundNumber + 1}   (Space)", onStartRound);
                    start.style.marginTop = 2 * UiTheme.S;
                    lowerBlock.Add(start);
                    break;
                }

                case RoundPhase.Active:
                {
                    lowerBlock.Add(UiTheme.Text("ROUND IN PROGRESS", 10, UiTheme.InkDim, bold: true, upper: true));
                    batchLabel = UiTheme.Text("batch – / –", 11, UiTheme.InkDim);
                    batchLabel.style.marginTop = UiTheme.S;
                    lowerBlock.Add(batchLabel);
                    // Sprint 16 (Director): buying stays open mid-round and
                    // the clock does not stop for it. Saying so once, here,
                    // is what turns "I couldn't afford it" into "I have to
                    // watch the number and move."
                    var live = UiTheme.Text("Buying stays open — the round does not pause.", 11, UiTheme.InkDim);
                    live.style.whiteSpace = WhiteSpace.Normal;
                    live.style.marginTop = UiTheme.S;
                    lowerBlock.Add(live);
                    break;
                }

                case RoundPhase.Defeat:
                {
                    var over = UiTheme.Text("GAME OVER", 26, UiTheme.Defeat, bold: true);
                    lowerBlock.Add(over);
                    lowerBlock.Add(UiTheme.Text($"{rounds.RoundsCleared} round(s) cleared.", 11, UiTheme.InkDim));
                    break;
                }
            }
        }
    }
}
