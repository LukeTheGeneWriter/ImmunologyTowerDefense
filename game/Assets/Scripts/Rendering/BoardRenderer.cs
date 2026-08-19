using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Colours each coarse cell's background quad by occupant. As of
    /// Sprint 2 (GAME_DESIGN.md section 4a), "occupant" is class-dependent:
    /// intracellular pathogens (virus, bacterium) infect the slot's host
    /// cell without replacing it, so the cell still reads as host tissue;
    /// a large bacterium kills and occupies the slot outright and reads as
    /// itself. See ShowsAsPathogenItself below.
    ///
    /// Also blends in a faint warm "heatmap" tint proportional to the
    /// cytokine field's strength at that cell (Sprint 1 closing task), so
    /// the field itself is visible on screen -- not just inferred from unit
    /// behaviour. This is now the primary way an intracellular infection is
    /// visually legible at all, since it no longer changes the cell's base
    /// color. Deliberately independent of CytokineToggle -- see original
    /// comment history in docs/ENGINE_STATUS.md.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private SpriteRenderer[,] views;
        private float refreshTimer;

        public static readonly Color HostColor = new Color(0.80f, 0.62f, 0.66f); // eosin-ish pink, healthy tissue (and, as of Sprint 2, intracellular-infected tissue -- it keeps reading as host cell)
        public static readonly Color PathogenColor = new Color(0.42f, 0.12f, 0.16f); // dark maroon -- transiting pathogens and adhered large bacteria only, as of Sprint 2
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
                    var pathogen = tissueGrid.GetPathogenAt(coord);
                    Color baseColor = ShowsAsPathogenItself(pathogen) ? PathogenColor : HostColor;

                    float intensity = Mathf.Clamp01(cytokineField.CoarseValueAt(coord) / TissueGrid.MaxSecretionStrength);
                    views[col, row].color = Color.Lerp(baseColor, HotColor, intensity * HeatmapBlendMax);
                }
            }
        }

        /// <summary>GAME_DESIGN.md section 4a's occupant/render split, as a
        /// static, side-effect-free predicate (same reasoning as pulling
        /// Chemotaxis.ChooseNextStep out to its own static method in the
        /// Sprint 1 closing task) so Assets/Editor/CombatVerification.cs
        /// can assert this directly for all three classes without needing
        /// a bound BoardRenderer/SpriteRenderer array.</summary>
        public static bool ShowsAsPathogenItself(PathogenAgent pathogen) =>
            pathogen != null && pathogen.Class == PathogenClass.LargeBacterium;
    }
}
