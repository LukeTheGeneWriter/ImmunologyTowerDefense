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
        private bool[,] isHostGround;
        private float refreshTimer;

        public static Color HostColor => Palette.HostHealthy; // eosin-ish pink, a HEALTHY host cell
        public static Color PathogenColor => Palette.Bacterium; // dark maroon -- pathogens visible as themselves

        /// <summary>Sprint 13: a free (extracellular) virus particle. A
        /// colder purple-maroon -- same "foreign dark-red/violet" family as
        /// PathogenColor but distinct enough that "virus" (small cold dot)
        /// and "bacterium" (maroon rod) read apart at a glance.</summary>
        public static Color VirionColor => Palette.Virion;

        /// <summary>An INFECTED host cell (GAME_DESIGN.md section 1c). A
        /// bruised violet: recognisably still tissue (same value as
        /// HostColor, so it does not read as a hole) but off-hue enough that
        /// a spreading infection draws a visible shape across the band.
        /// Note the cytokine heat tint also lands here -- infection is the
        /// main thing that secretes -- so in practice an infected cell reads
        /// as violet shading toward orange as it ramps.</summary>
        public static Color InfectedHostColor => Palette.HostInfectedViral;

        /// <summary>An `Infected` host cell whose resident is an intracellular
        /// BACTERIUM rather than a virus (GAME_DESIGN.md §4b). A sickly
        /// yellow-green -- purulent, unmistakably not the viral violet and
        /// not healthy pink -- so the Director can see which kind of
        /// infection a cell is carrying, and see a bacterium duck in and
        /// burst back out. The cytokine heat tint lands here too.</summary>
        public static Color InfectedByBacteriumColor => Palette.HostInfectedBact;

        /// <summary>Sprint 23: what an infected cell brightens toward as
        /// immune cells build recognition pressure on it.</summary>
        public static Color RecognitionColor => Palette.FxApoptosis;

        /// <summary>How far the lerp goes at full pressure. Short of 1 on
        /// purpose -- the cell must still read as infected right up to the
        /// moment it is cleared, or the player loses track of the threat
        /// while watching the meter.</summary>
        public const float RecognitionBlendMax = 0.65f;

        /// <summary>Pressure at which the cell takes the recognition collar.
        /// Half-way, so the collar means "this one is going to be dealt
        /// with" rather than "someone walked past it once".</summary>
        public const float RecognitionCollarAt = 0.5f;

        /// <summary>DEBRIS: a dead host cell. Desaturated grey-brown, darker
        /// and colder than living tissue -- necrotic, not merely empty. It
        /// has to be told apart from bare ground at a glance, which is why
        /// it is browner AND lighter than EmptyGroundColor rather than just
        /// darker than tissue.</summary>
        public static Color DebrisColor => Palette.Debris;

        /// <summary>Bare `Empty` ground -- cleaned, cell-free, regrowing.
        /// Very dark and neutral: a hole in the tissue, and visibly the
        /// absence of something rather than a substance in its own
        /// right.</summary>
        public static Color EmptyGroundColor => Palette.EmptyGround;

        /// <summary>The base band: the player's blood/production compartment
        /// and the endzone. A cold desaturated blue-violet, deliberately far
        /// from tissue pink and from the warm lumen so the three bands
        /// separate at a glance even on a small screenshot.</summary>
        public static Color BaseBandColor => Palette.BaseBand;

        /// <summary>The lumen channel: dark, warm, organic -- gut contents,
        /// not tissue.</summary>
        public static Color LumenBandColor => Palette.LumenBand;

        private static Color HotColor => Palette.CytokineHot; // warm glow, cytokine signal

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
            isHostGround = new bool[board.Columns, board.Rows];
            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    var coord = new CoarseCoord(col, row);
                    baseColors[col, row] = BandColor(board.BandOf(coord));
                    isHostGround[col, row] = tissueGrid.IsHostGround(coord);
                }
            }

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
                    // Sprint 15: base/lumen cells have no SpriteRenderer now
                    // (drawn by the compartment renderers instead).
                    if (views[col, row] == null) continue;

                    var coord = new CoarseCoord(col, row);
                    var hostState = isHostGround[col, row] ? tissueGrid.GetHostState(coord) : HostState.Empty;
                    var resident = isHostGround[col, row] && hostState == HostState.Infected
                        ? tissueGrid.GetIntracellularAt(coord) : null;
                    bool bacterial = resident != null && resident.Class == PathogenClass.IntracellularBacterium;

                    Color baseColor;
                    if (!isHostGround[col, row])
                        baseColor = baseColors[col, row];
                    else if (hostState == HostState.Infected)
                        baseColor = InfectedColorFor(resident); // §4b: viral vs. bacterial tint
                    else
                        baseColor = HostStateColor(hostState);

                    var pathogen = tissueGrid.GetOccupantAt(coord);
                    if (ShowsAsPathogenItself(pathogen)) baseColor = PathogenColor;

                    // Sprint 23: recognition pressure on an infected cell.
                    // Two-stage read -- the cell brightens continuously as
                    // pressure builds, then takes a collar once it is more
                    // than half-recognised. A tint alone was too quiet on a
                    // cell that is already colour-coded for being infected.
                    float stress = hostState == HostState.Infected
                        ? tissueGrid.GetInfectionStress(coord) : 0f;

                    // Sprint 13: the host-state sprite alongside the colour.
                    if (isHostGround[col, row])
                    {
                        views[col, row].sprite =
                            hostState == HostState.Healthy ? SpriteShapes.HostCell :
                            hostState == HostState.Dead    ? SpriteShapes.Debris :
                            hostState == HostState.Empty   ? SpriteShapes.EmptyPit :
                            stress >= RecognitionCollarAt ? SpriteShapes.HostCellRecognised :
                            bacterial ? SpriteShapes.HostCellInfectedBacterial : SpriteShapes.HostCellInfectedViral;
                    }

                    if (stress > 0f)
                    {
                        baseColor = Color.Lerp(baseColor, RecognitionColor, stress * RecognitionBlendMax);
                    }

                    // Sprint 13: a tiny deterministic per-cell value jitter so
                    // the sheet reads as a histology plate, not a flat wash.
                    baseColor *= 1f + CellJitter(col, row);
                    baseColor.a = 1f;

                    float intensity = Mathf.Clamp01(cytokineField.CoarseValueAt(coord) / TissueGrid.MaxSecretionStrength);
                    views[col, row].color = Color.Lerp(baseColor, HotColor, intensity * HeatmapBlendMax);
                }
            }
        }

        /// <summary>
        /// The host layer's four states, as four distinguishable colours
        /// (SPRINT_PLAN.md scope item 3: "if the Director cannot tell them
        /// apart at a glance the model is invisible"). Static and
        /// side-effect-free so a harness can assert the four are actually
        /// distinct without binding a SpriteRenderer array.
        /// </summary>
        /// <summary>Sprint 13: a deterministic ±3% multiplier per coarse
        /// cell, so 4,000 identical tiles get a subtle histology-plate
        /// mottle. Pure function of (col,row); no allocation, no RNG.</summary>
        private static float CellJitter(int col, int row)
        {
            uint h = (uint)(col * 73856093 ^ row * 19349663);
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h & 0xFFFFu) / 65535f - 0.5f) * 0.06f;
        }

        public static Color HostStateColor(HostState state)
        {
            switch (state)
            {
                case HostState.Healthy: return HostColor;
                case HostState.Infected: return InfectedHostColor;
                case HostState.Dead: return DebrisColor;
                default: return EmptyGroundColor;
            }
        }

        /// <summary>The colour for an `Infected` cell, split by what is
        /// hiding in it (GAME_DESIGN.md §4b): viral violet vs. bacterial
        /// yellow-green. Falls back to the viral colour if the resident is
        /// unknown. Side-effect-free for the harness.</summary>
        public static Color InfectedColorFor(PathogenAgent resident) =>
            resident != null && resident.Class == PathogenClass.IntracellularBacterium
                ? InfectedByBacteriumColor
                : InfectedHostColor;

        /// <summary>GAME_DESIGN.md section 4a's occupant/render split, as a
        /// static side-effect-free predicate so a headless harness can
        /// assert it without a bound SpriteRenderer array. True only for a
        /// LargeBacterium; false for both intracellular classes and for
        /// null.</summary>
        public static bool ShowsAsPathogenItself(PathogenAgent pathogen) =>
            pathogen != null && pathogen.Class == PathogenClass.LargeBacterium;
    }
}
