using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Debug HUD drawn with IMGUI (OnGUI) rather than uGUI -- this
    /// project's manifest doesn't include com.unity.ugui, and adding a
    /// package needs network access and is normally an Editor-GUI/
    /// Director step (see CytokineToggle.cs for the same note). IMGUI
    /// needs nothing extra and is a reasonable fit for a "debug toggle"
    /// sprint's HUD.
    ///
    /// Sprint 3 adds a live active-unit-count line. That number is the
    /// entire point of the sprint (GAME_DESIGN.md section 6d): before it,
    /// towers emitted forever and nothing despawned, so the count grew
    /// without bound. Showing it -- next to the theoretical ceiling -- is
    /// what turns "the population is capped, trust me" into something the
    /// Director can confirm in ten seconds of play. Per-tower
    /// "children alive / cap" is drawn on the marrow slots themselves
    /// (BoneMarrowManager.OnGUI).
    /// </summary>
    public class HudOverlay : MonoBehaviour
    {
        private BoardConfig board;
        private BoneMarrowManager boneMarrow;
        private GutInterface gutInterface;
        private InvasionTally tally;
        private PathogenSpawner spawner;
        private AtpWallet wallet;
        private RoundController rounds;
        private KnowledgeLedger knowledge;
        private AdaptiveDirector adaptive;
        private string infoLine;
        private GUIStyle style;
        private GUIStyle bigStyle;
        private float smoothedFrameMs;

        public void Bind(
            BoardConfig board, int macrophageSpeed, int neutrophilSpeed, BoneMarrowManager boneMarrow,
            GutInterface gutInterface, InvasionTally tally, PathogenSpawner spawner,
            AtpWallet wallet, RoundController rounds,
            KnowledgeLedger knowledge = null, AdaptiveDirector adaptive = null)
        {
            this.board = board;
            this.boneMarrow = boneMarrow;
            this.gutInterface = gutInterface;
            this.tally = tally;
            this.spawner = spawner;
            this.wallet = wallet;
            this.rounds = rounds;
            this.knowledge = knowledge;
            this.adaptive = adaptive;
            infoLine =
                "Immunology TD -- Sprint 8 dendritic-cell shuttle & antigen barcode\n" +
                $"Board: {board.Columns} x {board.Rows} coarse cells " +
                $"(base {board.BaseBandCells} | tissue {board.TissueBandCells} | lumen {board.LumenBandCells}), " +
                $"{BoardConfig.FineSubdivision}x{BoardConfig.FineSubdivision} fine per cell\n" +
                $"Macrophage speed: {macrophageSpeed} fine-tiles/tick   Neutrophil speed: {neutrophilSpeed} fine-tiles/tick\n" +
                "Buy 4 progenitor kinds in the base band (they share 5 slots); SPACE starts the round. DCs carry antigen from debris to the lymph node. Every number is a placeholder.";
        }

        private void OnGUI()
        {
            if (board == null) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    normal = { textColor = Color.white }
                };
                bigStyle = new GUIStyle(style) { fontSize = 24, fontStyle = FontStyle.Bold };
            }

            DrawRoundBar();

            // Map 01's base band now sits underneath this text, so the HUD
            // needs to stop being transparent white-on-whatever. A dimming
            // panel keeps both the readout and the board readable; without
            // it the bone marrow slot labels and these lines overprint each
            // other into mush.
            var panel = new Rect(0, 0, 1180, 324);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(16, 12, 820, 110), infoLine, style);

            string toggleLine = $"Cytokine sensing: {(CytokineToggle.Enabled ? "ON" : "OFF")}   (press C to toggle)";
            GUI.Label(new Rect(16, 122, 720, 30), toggleLine, style);

            string heatmapLine = "Orange tint on host cells = cytokine field strength (always visible; only pulls units when sensing is ON)";
            GUI.Label(new Rect(16, 150, 900, 30), heatmapLine, style);

            GUI.Label(new Rect(16, 178, 900, 30), BuildPopulationLine(), style);
            GUI.Label(new Rect(16, 206, 1100, 30), BuildPathogenLine(), style);
            GUI.Label(new Rect(16, 234, 1100, 30), BuildInvasionLine(), style);
            GUI.Label(new Rect(16, 262, 1100, 30), BuildKnowledgeLine(), style);
            GUI.Label(new Rect(16, 290, 1100, 30), BuildPerformanceLine(), style);
        }

        /// <summary>Sprint 8: per-species adaptive knowledge % (§5), plus the
        /// live lymph-node population. Knowledge rises when a DC and a
        /// helper-T pair with matching barcodes; it persists across rounds.
        /// It unlocks nothing yet -- §5's threshold ladder is next.</summary>
        private string BuildKnowledgeLine()
        {
            if (knowledge == null) return string.Empty;
            float v = knowledge.Get(PathogenClass.IntracellularVirus);
            float b = knowledge.Get(PathogenClass.IntracellularBacterium);
            float l = knowledge.Get(PathogenClass.LargeBacterium);
            string node = adaptive == null || adaptive.Node == null
                ? string.Empty
                : $"      lymph node: DC {adaptive.Node.VisitorCount}  helper-T {adaptive.Node.ResidentCount}";
            return $"KNOWLEDGE -- virus {v:F0}%   bacterium {b:F0}%   large-bac {l:F0}%{node}";
        }

        /// <summary>The Sprint 7 economy / round readout -- top-right, clear
        /// of the debug panel. ATP, lives, round number and phase, plus the
        /// buy-phase prompt / Start button and the GAME OVER banner.</summary>
        private void DrawRoundBar()
        {
            if (rounds == null || wallet == null) return;

            float w = 380f;
            var box = new Rect(Screen.width - w - 12f, 12f, w, 150f);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = prev;

            float x = box.x + 14f;
            GUI.Label(new Rect(x, box.y + 8f, w - 28f, 30f),
                $"ATP {wallet.Balance}      Lives {rounds.Lives} / {rounds.MaxLives}", bigStyle);

            string phase =
                rounds.Phase == RoundPhase.Active ? "ROUND IN PROGRESS" :
                rounds.Phase == RoundPhase.Defeat ? "GAME OVER" : "BUY PHASE";
            string batch = spawner == null ? "" :
                $"   batch {spawner.BatchEmitted}/{spawner.BatchTarget}, {spawner.LiveCount} in play";
            int shownRound = Mathf.Max(1, rounds.Phase == RoundPhase.Building ? rounds.RoundNumber + 1 : rounds.RoundNumber);
            GUI.Label(new Rect(x, box.y + 42f, w - 28f, 24f),
                $"Round {shownRound} -- {phase}{(rounds.Phase == RoundPhase.Active ? batch : "")}", style);

            string tagline = rounds.Phase == RoundPhase.Building
                ? RoundScript.ForRound(rounds.RoundNumber + 1).Tagline
                : rounds.CurrentTagline;
            if (!string.IsNullOrEmpty(tagline))
                GUI.Label(new Rect(x, box.y + 64f, w - 28f, 24f), $"“{tagline}”", style);

            if (rounds.Phase == RoundPhase.Building)
            {
                GUI.Label(new Rect(x, box.y + 90f, w - 28f, 22f),
                    $"Time is frozen. +{ImmunologyTD.Economy.EconomyTuning.RoundStartLumpSum} ATP. Buy, then:", style);
                if (GUI.Button(new Rect(x, box.y + 114f, w - 28f, 24f), $"Start Round {rounds.RoundNumber + 1}   (Space)"))
                {
                    rounds.StartRound();
                }
            }
            else if (rounds.Phase == RoundPhase.Defeat)
            {
                GUI.Label(new Rect(x, box.y + 90f, w - 28f, 44f),
                    $"Reached the base {tally?.ReachedBase ?? 0} times over {rounds.RoundsCleared} cleared round(s).", style);
            }
            else
            {
                GUI.Label(new Rect(x, box.y + 90f, w - 28f, 44f),
                    "A contaminated food item is delivering\nthis round's batch. Cells & pathogens persist.", style);
            }
        }

        /// <summary>Frame cost, on screen. SPRINT_PLAN.md item 2 asks for a
        /// measured number rather than an assurance: Map 01 renders 4,000
        /// coarse cells where Sprint 1-3 rendered 150, and BoardRenderer
        /// still gives each cell its own SpriteRenderer. Smoothed so it is
        /// readable rather than flickering.</summary>
        private string BuildPerformanceLine()
        {
            float ms = smoothedFrameMs;
            return ms <= 0f
                ? string.Empty
                : $"Frame: {ms:F2} ms ({(1000f / Mathf.Max(0.01f, ms)):F0} fps)   " +
                  $"cells rendered: {board.Columns * board.Rows}";
        }

        private void Update()
        {
            // Exponential smoothing; unscaledDeltaTime so a future pause or
            // speed control doesn't distort the reading.
            float frameMs = Time.unscaledDeltaTime * 1000f;
            smoothedFrameMs = smoothedFrameMs <= 0f
                ? frameMs
                : Mathf.Lerp(smoothedFrameMs, frameMs, 0.05f);
        }

        /// <summary>Where the pathogens currently are, by band. Sprint 4's
        /// invasion loop is only legible if you can see the three
        /// populations move between each other -- riding the lumen flow,
        /// piled up against the gut wall, and loose in the tissue.</summary>
        private string BuildPathogenLine()
        {
            if (spawner == null) return string.Empty;

            spawner.CountByState(out int inLumen, out int atInterface, out int inTissue);
            string peak = gutInterface == null
                ? string.Empty
                : $"   (worst wall position right now: {gutInterface.PeakAdhered})";
            return $"Pathogens -- lumen: {inLumen}   adhered at gut wall: {atInterface}   in tissue: {inTissue}{peak}";
        }

        /// <summary>Running totals for the invasion loop. "Reached base" is
        /// this sprint's endzone counter -- SPRINT_PLAN.md item 9 keeps it a
        /// bare count deliberately; the 100-life pool and the real lose
        /// condition are Sprint 5.</summary>
        private string BuildInvasionLine()
        {
            if (tally == null) return string.Empty;

            return $"Adhesions: {tally.Adhesions}   Breaches: {tally.Breaches} " +
                   $"(released {tally.ReleasedIntoTissue})   Excreted harmlessly: {tally.Excreted}   " +
                   $"REACHED BASE: {tally.ReachedBase}";
        }

        private string BuildPopulationLine()
        {
            if (boneMarrow == null) return string.Empty;

            int active = boneMarrow.TotalActiveUnits;
            int placed = 0;
            int ceiling = 0;
            for (int i = 0; i < boneMarrow.SlotCount; i++)
            {
                if (boneMarrow.GetSlotState(i) != BoneMarrowSlotState.Placed) continue;
                placed++;
                ceiling += boneMarrow.GetTuning(i).MaxActiveChildren;
            }

            return placed == 0
                ? "Active units: 0   (no towers placed yet)"
                : $"Active units: {active} / {ceiling} max   ({placed} tower{(placed == 1 ? "" : "s")} placed; units deplete after their kill limit and free a slot)";
        }
    }
}
