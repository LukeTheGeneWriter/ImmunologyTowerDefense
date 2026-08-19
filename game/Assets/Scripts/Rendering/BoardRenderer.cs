using UnityEngine;
using ImmunologyTD.Grid;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Colours each coarse cell's background quad by occupant: plain host
    /// tissue vs. an adhered pathogen. Deliberately a flat colour swap, no
    /// damage/fibrosis states -- those are out of scope this sprint. This
    /// is what makes "host-cell occupancy readable at a glance"
    /// (SPRINT_PLAN.md stopping point) actually true.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private SpriteRenderer[,] views;
        private float refreshTimer;

        private static readonly Color HostColor = new Color(0.80f, 0.62f, 0.66f); // eosin-ish pink, healthy tissue
        private static readonly Color PathogenColor = new Color(0.42f, 0.12f, 0.16f); // adhered site, dark maroon

        public void Bind(BoardConfig board, TissueGrid tissueGrid, SpriteRenderer[,] views)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
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
                    bool occupied = tissueGrid.GetPathogenAt(new CoarseCoord(col, row)) != null;
                    views[col, row].color = occupied ? PathogenColor : HostColor;
                }
            }
        }
    }
}
