using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Colours each coarse cell's background quad.
    ///
    /// **Sprint 4** made the base colour depend on the cell's BAND
    /// (GAME_DESIGN.md section 1a) rather than being host pink everywhere:
    /// base / tissue / lumen have to be visually distinct or the map does
    /// not read as three bands. Occupant colouring (section 4a's
    /// intracellular-vs-large-bacterium split) still applies on top, and
    /// only inside tissue.
    ///
    /// Also blends in a faint warm "heatmap" tint proportional to the
    /// cytokine field at that cell, so the field is visible rather than
    /// inferred. Deliberately independent of CytokineToggle -- see the
    /// original comment history in docs/ENGINE_STATUS.md.
    ///
    /// **Scale note (SPRINT_PLAN.md item 2).** This class was written for
    /// 150 cells and now drives 4,000, one SpriteRenderer each. The band
    /// lookup per cell is cached at Bind time so Refresh is a flat array
    /// walk; measured frame cost is recorded in docs/ENGINE_STATUS.md.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private SpriteRenderer[,] views;
        private Color[,] baseColors;
        private float refreshTimer;

        public static readonly Color HostColor = new Color(0.80f, 0.62f, 0.66f); // eosin-ish pink, healthy (and intracellular-infected) tissue
        public static readonly Color PathogenColor = new Color(0.42f, 0.12f, 0.16f); // dark maroon -- pathogens visible as themselves

        /// <summary>The base band: the player's blood/production compartment
        /// and the endzone. A cold desaturated blue-violet, deliberately far
        /// from tissue pink and from the warm lumen so the three bands
        /// separate at a glance even on a small screenshot.</summary>
        public static readonly Color BaseBandColor = new Color(0.20f, 0.21f, 0.34f);

        /// <summary>The lumen channel: dark, warm, organic -- gut contents,
        /// not tissue.</summary>
        public static readonly Color LumenBandColor = new Color(0.19f, 0.16f, 0.11f);

        private static readonly Color HotColor = new Color(1.00f, 0.55f, 0.05f); // warm glow, cytokine signal

        /// <summary>How far the tint can push toward HotColor at full
        /// (normalized 1.0) field strength.</summary>
        private const float HeatmapBlendMax = 0.65f;

        public void Bind(BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField, SpriteRenderer[,] views)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.views = views;

            baseColors = new Color[board.Columns, board.Rows];
            for (int col = 0; col < board.Columns; col++)
                for (int row = 0; row < board.Rows; row++)
                    baseColors[col, row] = BandColor(board.BandOf(new CoarseCoord(col, row)));

            Refresh();
        }

        public static Color BandColor(BoardBand band)
        {
            switch (band)
            {
                case BoardBand.Base: return BaseBandColor;
                case BoardBand.Lumen: return LumenBandColor;
                default: return HostColor;
            }
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
            int columns = board.Columns;
            int rows = board.Rows;
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    var coord = new CoarseCoord(col, row);
                    Color baseColor = baseColors[col, row];

                    var pathogen = tissueGrid.GetPathogenAt(coord);
                    if (ShowsAsPathogenItself(pathogen)) baseColor = PathogenColor;

                    float intensity = Mathf.Clamp01(cytokineField.CoarseValueAt(coord) / TissueGrid.MaxSecretionStrength);
                    views[col, row].color = Color.Lerp(baseColor, HotColor, intensity * HeatmapBlendMax);
                }
            }
        }

        /// <summary>GAME_DESIGN.md section 4a's occupant/render split, as a
        /// static side-effect-free predicate so a headless harness can
        /// assert it without a bound SpriteRenderer array. True only for a
        /// LargeBacterium; false for both intracellular classes and for
        /// null.</summary>
        public static bool ShowsAsPathogenItself(PathogenAgent pathogen) =>
            pathogen != null && pathogen.Class == PathogenClass.LargeBacterium;
    }
}
