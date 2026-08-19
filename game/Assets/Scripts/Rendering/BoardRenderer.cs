using UnityEngine;
using ImmunologyTD.Grid;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Colours each coarse cell's background quad by occupant: plain host
    /// tissue vs. an adhered pathogen. No damage/fibrosis states -- those
    /// are out of scope this sprint. This is what makes "host-cell
    /// occupancy readable at a glance" (SPRINT_PLAN.md stopping point)
    /// actually true.
    ///
    /// Sprint 1 closing task: also blends in a faint warm "heatmap" tint
    /// proportional to the cytokine field's strength at that cell, so the
    /// field itself is visible on screen -- not just inferred from unit
    /// behaviour. Deliberately independent of CytokineToggle: the field
    /// exists and is shown regardless of whether sensing is on, mirroring
    /// GAME_DESIGN.md 2a's fiction that cytokines are secreted regardless
    /// of whether a given cell type can sense them yet. This is what lets
    /// the Director watch cause (a hot cell, always visible) and effect
    /// (units drifting toward it, only when the toggle is on) side by
    /// side, per the closing task's "watchable side by side" requirement.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private SpriteRenderer[,] views;
        private float refreshTimer;

        private static readonly Color HostColor = new Color(0.80f, 0.62f, 0.66f); // eosin-ish pink, healthy tissue
        private static readonly Color PathogenColor = new Color(0.42f, 0.12f, 0.16f); // adhered site, dark maroon
        private static readonly Color HotColor = new Color(1.00f, 0.55f, 0.05f); // warm glow, cytokine signal

        /// <summary>How far the tint can push toward HotColor at full
        /// (normalized 1.0) field strength -- kept below 1 so a fully "hot"
        /// cell still visibly reads as host tissue or pathogen underneath,
        /// not a solid orange square.</summary>
        private const float HeatmapBlendMax = 0.65f;

        public void Bind(BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField, SpriteRenderer[,] views)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.views = views;
            Refresh();
        }

        private void Update()
        {
            if (board == null) return;
            refreshTimer += Time.deltaTime;
            if (refreshTimer < 0.15f) return;
            refreshTimer = 0f;
            Refresh();
        }

        private void Refresh()
        {
            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < BoardConfig.Rows; row++)
                {
                    var coord = new CoarseCoord(col, row);
                    bool occupied = tissueGrid.GetPathogenAt(coord) != null;
                    Color baseColor = occupied ? PathogenColor : HostColor;

                    float intensity = Mathf.Clamp01(cytokineField.CoarseValueAt(coord) / TissueGrid.MaxSecretionStrength);
                    views[col, row].color = Color.Lerp(baseColor, HotColor, intensity * HeatmapBlendMax);
                }
            }
        }
    }
}
