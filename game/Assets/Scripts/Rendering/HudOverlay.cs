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

        public void Bind(BoardConfig board, int macrophageCount, int macrophageSpeed, int neutrophilCount, int neutrophilSpeed)
        {
            this.board = board;
            infoLine =
                "Immunology TD -- Sprint 1 search prototype\n" +
                $"Board: {board.Columns} x {BoardConfig.Rows} coarse cells, " +
                $"{BoardConfig.FineSubdivision}x{BoardConfig.FineSubdivision} fine per cell\n" +
                $"Macrophage: {macrophageCount} units, {macrophageSpeed} fine-tiles/tick\n" +
                $"Neutrophil: {neutrophilCount} units, {neutrophilSpeed} fine-tiles/tick";
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

            GUI.Label(new Rect(16, 12, 720, 90), infoLine, style);

            string toggleLine = $"Cytokine sensing: {(CytokineToggle.Enabled ? "ON" : "OFF")}   (press C to toggle)";
            GUI.Label(new Rect(16, 100, 720, 30), toggleLine, style);
        }
    }
}
