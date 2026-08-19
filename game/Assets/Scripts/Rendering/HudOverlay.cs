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
    /// </summary>
    public class HudOverlay : MonoBehaviour
    {
        private BoardConfig board;
        private string infoLine;
        private GUIStyle style;

        public void Bind(BoardConfig board, int macrophageSpeed, int neutrophilSpeed)
        {
            this.board = board;
            infoLine =
                "Immunology TD -- Sprint 2 placement + combat prototype\n" +
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
        }
    }
}
