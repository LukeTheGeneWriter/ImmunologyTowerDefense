using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;

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
        private string infoLine;
        private GUIStyle style;

        public void Bind(BoardConfig board, int macrophageSpeed, int neutrophilSpeed, BoneMarrowManager boneMarrow)
        {
            this.board = board;
            this.boneMarrow = boneMarrow;
            infoLine =
                "Immunology TD -- Sprint 3 unit lifecycle prototype\n" +
                $"Board: {board.Columns} x {BoardConfig.Rows} coarse cells, " +
                $"{BoardConfig.FineSubdivision}x{BoardConfig.FineSubdivision} fine per cell\n" +
                $"Macrophage speed: {macrophageSpeed} fine-tiles/tick   Neutrophil speed: {neutrophilSpeed} fine-tiles/tick\n" +
                "Place bone marrow towers (below) to bring units into tissue -- nothing spawns until you do.";
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
            }

            GUI.Label(new Rect(16, 12, 820, 110), infoLine, style);

            string toggleLine = $"Cytokine sensing: {(CytokineToggle.Enabled ? "ON" : "OFF")}   (press C to toggle)";
            GUI.Label(new Rect(16, 122, 720, 30), toggleLine, style);

            string heatmapLine = "Orange tint on host cells = cytokine field strength (always visible; only pulls units when sensing is ON)";
            GUI.Label(new Rect(16, 150, 900, 30), heatmapLine, style);

            GUI.Label(new Rect(16, 178, 900, 30), BuildPopulationLine(), style);
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
