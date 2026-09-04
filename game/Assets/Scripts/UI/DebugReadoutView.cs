using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The instrument panel (docs/UI_DESIGN.md §3): everything that used to
    /// live in HudOverlay's always-on top-left dump, moved behind the
    /// backtick key and **off by default**.
    ///
    /// The content is deliberately unchanged -- the same lines, the same
    /// numbers, built by the same code, moved file. What changed is who
    /// asks for it: the Director's Sprint 15 note was that the stat wall
    /// made it impossible to feel the *game*, not that the stats were
    /// wrong. They are still exactly one keypress away, and they are still
    /// what a mid-sprint engine question gets answered with.
    ///
    /// Bottom-left, ~440 px, monospace, scrolling if it overflows. It sits
    /// over the marrow column on purpose: when you are reading this you are
    /// not clicking towers, and the tissue band -- the thing you are
    /// debugging *against* -- stays clear.
    ///
    /// Refresh() is only called while the panel is visible, so the string
    /// building costs nothing in normal play.
    /// </summary>
    internal sealed class DebugReadoutView
    {
        public readonly VisualElement Root;

        private readonly Label body;
        private readonly System.Text.StringBuilder sb = new System.Text.StringBuilder(1400);

        private BoardConfig board;
        private BoneMarrowManager boneMarrow;
        private GutInterface gutInterface;
        private InvasionTally tally;
        private PathogenSpawner spawner;
        private KnowledgeLedger knowledge;
        private AdaptiveDirector adaptive;
        private string infoLine;

        private float smoothedFrameMs;

        public DebugReadoutView(VisualElement parent)
        {
            Root = UiTheme.Panel(opaque: true);
            Root.style.width = 440;
            Root.style.maxHeight = Length.Percent(62);
            parent.Add(Root);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            Root.Add(scroll);

            body = UiTheme.Text("", 12, UiTheme.Ink);
            body.style.whiteSpace = WhiteSpace.Normal;
            UiTheme.ApplyFont(body, mono: true);
            scroll.Add(body);

            UiTheme.Show(Root, false);
        }

        public void Bind(
            BoardConfig board, int macrophageSpeed, int neutrophilSpeed,
            BoneMarrowManager boneMarrow, GutInterface gutInterface, InvasionTally tally,
            PathogenSpawner spawner, KnowledgeLedger knowledge, AdaptiveDirector adaptive)
        {
            this.board = board;
            this.boneMarrow = boneMarrow;
            this.gutInterface = gutInterface;
            this.tally = tally;
            this.spawner = spawner;
            this.knowledge = knowledge;
            this.adaptive = adaptive;

            // Verbatim from HudOverlay.Bind -- the strings did not change,
            // only where they are drawn.
            infoLine =
                $"Board: {board.Columns} x {board.Rows} coarse cells " +
                $"(base {board.BaseBandCells} | tissue {board.TissueBandCells} | lumen {board.LumenBandCells}), " +
                $"{BoardConfig.FineSubdivision}x{BoardConfig.FineSubdivision} fine per cell\n" +
                $"Macrophage speed: {macrophageSpeed} fine-tiles/tick   Neutrophil speed: {neutrophilSpeed} fine-tiles/tick\n" +
                "Buy 4 progenitor kinds in the base band (they share 5 slots); SPACE starts the round. DCs carry antigen from debris to the lymph node. Every number is a placeholder.";
        }

        /// <summary>Frame cost is smoothed every frame whether or not the
        /// panel is visible -- otherwise the first reading after a toggle is
        /// a spike from the frame that built the panel.</summary>
        public void TickFrameCost()
        {
            float frameMs = Time.unscaledDeltaTime * 1000f;
            smoothedFrameMs = smoothedFrameMs <= 0f ? frameMs : Mathf.Lerp(smoothedFrameMs, frameMs, 0.05f);
        }

        /// <param name="selectedSlot">The slot the upgrade panel is open on,
        /// or -1. UI_DESIGN.md §3: while both are up, the readout echoes the
        /// target-field names the visible rows are destined to write, so
        /// "ready to wire" is something the Director can check rather than
        /// take on faith.</param>
        public void Refresh(int selectedSlot)
        {
            if (board == null) return;

            sb.Clear();
            sb.Append("IMMUNOLOGY TD — DEBUG READOUT  (` to hide)\n\n");
            sb.Append(infoLine).Append('\n');
            sb.Append(BuildToggleLine()).Append('\n');
            sb.Append("Orange tint on host cells = cytokine field strength (always visible; only pulls units when sensing is ON)\n\n");
            sb.Append(BuildPopulationLine()).Append('\n');
            sb.Append(BuildPathogenLine()).Append('\n');
            sb.Append(BuildInvasionLine()).Append("\n\n");
            sb.Append(BuildKnowledgeHeader()).Append('\n');
            sb.Append(BuildLadderLine(PathogenClass.IntracellularVirus, "virus")).Append('\n');
            sb.Append(BuildLadderLine(PathogenClass.IntracellularBacterium, "bacterium")).Append('\n');
            sb.Append(BuildLadderLine(PathogenClass.LargeBacterium, "large-bac")).Append("\n\n");
            sb.Append(BuildPerformanceLine());

            string wiring = BuildWiringLine(selectedSlot);
            if (!string.IsNullOrEmpty(wiring)) sb.Append("\n\n").Append(wiring);

            body.text = sb.ToString();
        }

        private static string BuildToggleLine() =>
            CytokineToggle.Enabled
                ? $"Cytokine sensing: ON (always) — sharpness x{Chemotaxis.EffectiveSharpness / Chemotaxis.GradientSharpness:F1} (buy 'Cytokine sensing +')   [C = debug off]"
                : "Cytokine sensing: OFF (debug) — press C to restore";

        /// <summary>The rows the open upgrade panel would write if the
        /// wiring sprint landed today. Player-invisible on purpose (§4).</summary>
        private string BuildWiringLine(int selectedSlot)
        {
            if (boneMarrow == null || selectedSlot < 0 || selectedSlot >= boneMarrow.SlotCount) return string.Empty;
            if (boneMarrow.GetSlotState(selectedSlot) != BoneMarrowSlotState.Placed) return string.Empty;

            var kind = boneMarrow.GetSlotKind(selectedSlot);
            var rows = ProgenitorUpgradeCatalog.RowsFor(kind);
            var s = new System.Text.StringBuilder();
            s.Append($"SLOT {selectedSlot} ({kind}) — rows are placeholders; each names the field it will write:\n");
            for (int i = 0; i < rows.Length; i++)
                s.Append($"  [{boneMarrow.GetUpgradeLevel(selectedSlot, i)}/{rows[i].Cap}] {rows[i].Name} -> {rows[i].FieldLabel}\n");
            return s.ToString();
        }

        private string BuildKnowledgeHeader()
        {
            if (knowledge == null) return string.Empty;
            string node = adaptive == null || adaptive.Node == null
                ? string.Empty
                : $"      lymph node: DC {adaptive.Node.VisitorCount}  helper-T {adaptive.Node.ResidentCount}";
            return $"KNOWLEDGE per species -- ladder rungs unlock nothing yet, display only:{node}";
        }

        private string BuildLadderLine(PathogenClass species, string label)
        {
            if (knowledge == null) return string.Empty;
            float pct = knowledge.Get(species);
            var s = new System.Text.StringBuilder();
            s.Append($"  {label,-10} {pct,3:F0}%  ");
            foreach (var rung in KnowledgeLadder.Rungs)
                s.Append(pct >= rung.ThresholdPercent ? $"[x]{rung.ShortName} " : $"[ ]{rung.ShortName} ");
            return s.ToString();
        }

        private string BuildPerformanceLine()
        {
            float ms = smoothedFrameMs;
            return ms <= 0f
                ? string.Empty
                : $"Frame: {ms:F2} ms ({(1000f / Mathf.Max(0.01f, ms)):F0} fps)   " +
                  $"cells rendered: {board.Columns * board.Rows}";
        }

        private string BuildPathogenLine()
        {
            if (spawner == null) return string.Empty;

            spawner.CountByState(out int inLumen, out int atInterface, out int inTissue);
            string peak = gutInterface == null
                ? string.Empty
                : $"   (worst wall position right now: {gutInterface.PeakAdhered})";
            return $"Pathogens -- lumen: {inLumen}   adhered at gut wall: {atInterface}   in tissue: {inTissue}{peak}";
        }

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
