using UnityEngine;

namespace ImmunologyTD.Grid
{
    /// <summary>Which lateral band of the map a coarse cell belongs to
    /// (GAME_DESIGN.md section 1a). Bands are DATA, not magic numbers --
    /// their extents live on BoardConfig so a later map can use different
    /// proportions without touching any gameplay code.</summary>
    public enum BoardBand { Base, Tissue, Lumen }

    /// <summary>Which grid axis the base -> tissue -> lumen bands are laid
    /// out along, i.e. the axis pathogens advance along. Map 01 is
    /// Horizontal (bands are columns, rows are lanes).</summary>
    public enum BoardAxis { Horizontal, Vertical }

    /// <summary>Which end of an axis something sits at, in raw grid-index
    /// terms. Negative = index 0 (left for columns, top for rows, given
    /// FineToWorld's orientation); Positive = index length-1.</summary>
    public enum AxisEnd { Negative, Positive }

    /// <summary>
    /// Board dimensions, band layout, and world-space scale.
    ///
    /// **Sprint 4 rewrite.** Sprints 1-3 had a 30x5 board with Rows as a
    /// const and no notion of where anything was; the map was flat and
    /// pathogens adhered at a uniformly random column. GAME_DESIGN.md
    /// section 1a replaced that with Map 01: a 100x40 grid of host cells in
    /// three lateral bands (base / tissue / lumen).
    ///
    /// ## The base-direction abstraction (ARCHITECTURAL REQUIREMENT)
    ///
    /// The Director's explicit framing (2026-08-21, GAME_DESIGN.md section
    /// 1b step 3) is that pathogen advance is specified as **toward the
    /// base**, never as "leftward". Nothing in movement code may hardcode a
    /// leftward or negative-X assumption, so a later map can put the base
    /// anywhere by changing config alone.
    ///
    /// The mechanism: every piece of band/advance logic works in an
    /// **axis frame**, not in raw (Column, Row):
    ///
    ///  - **axis index** = distance from the base end, measured along
    ///    <see cref="ThreatAxis"/>. Axis index 0 is always the outermost
    ///    base cell; axis index <see cref="AxisLength"/>-1 is always the far
    ///    side of the lumen channel. Moving toward the base is ALWAYS
    ///    "axis index minus one," whatever that means in world space.
    ///  - **cross index** = the perpendicular index (the 40 lanes on Map
    ///    01), raw, unflipped.
    ///
    /// <see cref="AxisIndex"/> / <see cref="CrossIndex"/> convert a
    /// CoarseCoord into that frame and <see cref="CoarseFromAxis"/> converts
    /// back; <see cref="OffsetInAxisFrame"/> does both around a step. Flip
    /// <see cref="baseEnd"/> to Positive and the base moves to the opposite
    /// side of the map with no code change -- Assets/Editor/MapVerification.cs
    /// asserts exactly that.
    /// </summary>
    public class BoardConfig : MonoBehaviour
    {
        [Header("Board size, in COARSE cells (host cells, not fine tiles)")]
        [Tooltip("Coarse columns. Map 01 (GAME_DESIGN.md section 1a) is 100 columns x 40 rows of HOST CELLS -- the 7x7 fine sub-lattice sits underneath that.")]
        [SerializeField] private int columns = 25;

        [Tooltip("Coarse rows. On Map 01 these are the 40 lanes; they carry no depth meaning (INTERFACE.md open question 1, resolved 2026-08-21).")]
        [SerializeField] private int rows = 10;

        [Header("Band layout (GAME_DESIGN.md section 1a) -- data, not magic numbers")]
        [Tooltip("Which axis the bands run along. Horizontal = bands are columns and rows are lanes (Map 01).")]
        [SerializeField] private BoardAxis threatAxis = BoardAxis.Horizontal;

        [Tooltip("Which end of the threat axis the BASE sits at. Map 01: Negative, i.e. the left columns. Flipping this moves the base -- and therefore every pathogen's advance direction -- with no code change.")]
        [SerializeField] private AxisEnd baseEnd = AxisEnd.Negative;

        [Tooltip("Cells of BASE band, counted from the base end. Map 01: 25.")]
        [SerializeField] private int baseBandCells = 6;

        [Tooltip("Cells of LUMEN band, counted from the far end. Map 01: 25. Tissue is whatever is left in between.")]
        [SerializeField] private int lumenBandCells = 6;

        [Tooltip("Which end of the CROSS axis the lumen flow carries pathogens toward. Map 01: Positive = increasing row = downward on screen (FineToWorld puts row 0 at the top).")]
        [SerializeField] private AxisEnd lumenFlowEnd = AxisEnd.Positive;

        public const int FineSubdivision = 7;
        public const float FineTileWorldSize = 0.16f;

        /// <summary>Shared simulation tick length for both units and
        /// pathogens. "Fine tiles per tick" (SearchUnit) is measured against
        /// this. GAME_DESIGN.md section 1a's scale note is derived from this
        /// value -- do NOT change it to "fix" traversal times.</summary>
        public const float TickIntervalSeconds = 0.12f;

        public int Columns => columns;

        /// <summary>Coarse rows. **Was a `const 5` through Sprint 3** --
        /// now an instance value, because Map 01 is 40 rows and a later map
        /// will be something else. Any `BoardConfig.Rows` static reference
        /// left anywhere is Sprint 1-3 code that has not been updated.</summary>
        public int Rows => rows;

        public int FineColumns => columns * FineSubdivision;
        public int FineRows => rows * FineSubdivision;

        // ------------------------------------------------------------------
        // Axis frame -- see the class comment. Everything band- or
        // advance-related is expressed here, never in raw Column/Row deltas.
        // ------------------------------------------------------------------

        public BoardAxis ThreatAxis => threatAxis;
        public AxisEnd BaseEnd => baseEnd;

        /// <summary>Number of cells along the threat axis (100 on Map 01).</summary>
        public int AxisLength => threatAxis == BoardAxis.Horizontal ? columns : rows;

        /// <summary>Number of lanes perpendicular to the threat axis (40 on
        /// Map 01). Also the number of gut-interface positions.</summary>
        public int CrossLength => threatAxis == BoardAxis.Horizontal ? rows : columns;

        /// <summary>Distance-from-base index of a coarse cell. 0 = the
        /// outermost base cell, AxisLength-1 = the far side of the lumen.</summary>
        public int AxisIndex(CoarseCoord c)
        {
            int raw = threatAxis == BoardAxis.Horizontal ? c.Column : c.Row;
            return baseEnd == AxisEnd.Negative ? raw : AxisLength - 1 - raw;
        }

        /// <summary>Lane index of a coarse cell, perpendicular to the threat
        /// axis. Raw and unflipped -- the flow direction along it is carried
        /// by <see cref="FlowCrossStep"/>, not by a coordinate flip.</summary>
        public int CrossIndex(CoarseCoord c) =>
            threatAxis == BoardAxis.Horizontal ? c.Row : c.Column;

        /// <summary>Fine-tile analogue of <see cref="AxisIndex"/> — distance
        /// from the base end measured in FINE tiles. Flips for a Positive
        /// base end, exactly as the coarse version does. For movement code
        /// that biases at fine granularity (Sprint 12's DC patrol sweep)
        /// rather than only at coarse-cell boundaries.</summary>
        public int FineAxisIndex(FineCoord c)
        {
            int raw = threatAxis == BoardAxis.Horizontal ? c.Column : c.Row;
            int len = threatAxis == BoardAxis.Horizontal ? FineColumns : FineRows;
            return baseEnd == AxisEnd.Negative ? raw : len - 1 - raw;
        }

        /// <summary>Fine-tile analogue of <see cref="CrossIndex"/> — the lane
        /// index in FINE tiles, raw and unflipped.</summary>
        public int FineCrossIndex(FineCoord c) =>
            threatAxis == BoardAxis.Horizontal ? c.Row : c.Column;

        /// <summary>Inverse of AxisIndex/CrossIndex.</summary>
        public CoarseCoord CoarseFromAxis(int axisIndex, int crossIndex)
        {
            int raw = baseEnd == AxisEnd.Negative ? axisIndex : AxisLength - 1 - axisIndex;
            return threatAxis == BoardAxis.Horizontal
                ? new CoarseCoord(raw, crossIndex)
                : new CoarseCoord(crossIndex, raw);
        }

        /// <summary>Steps a coarse coordinate in the axis frame.
        /// <paramref name="deltaAxis"/> of -1 always means "one cell toward
        /// the base," whichever direction that is in world space. This is
        /// the ONLY sanctioned way for movement code to step along the
        /// threat axis.</summary>
        public CoarseCoord OffsetInAxisFrame(CoarseCoord c, int deltaAxis, int deltaCross) =>
            CoarseFromAxis(AxisIndex(c) + deltaAxis, CrossIndex(c) + deltaCross);

        public bool InAxisBounds(int axisIndex) => axisIndex >= 0 && axisIndex < AxisLength;
        public bool InCrossBounds(int crossIndex) => crossIndex >= 0 && crossIndex < CrossLength;

        // ------------------------------------------------------------------
        // Bands
        // ------------------------------------------------------------------

        public int BaseBandCells => Mathf.Clamp(baseBandCells, 0, AxisLength);
        public int LumenBandCells => Mathf.Clamp(lumenBandCells, 0, AxisLength - BaseBandCells);
        public int TissueBandCells => AxisLength - BaseBandCells - LumenBandCells;

        /// <summary>Axis index of the tissue cell nearest the BASE -- where
        /// immune cells enter tissue (SPRINT_PLAN.md item 8) and the last
        /// tissue cell a pathogen occupies before it reaches the base.</summary>
        public int TissueBaseEdgeAxisIndex => BaseBandCells;

        /// <summary>Axis index of the tissue cell nearest the LUMEN -- the
        /// "first layer of healthy tissue" a breach releases into
        /// (GAME_DESIGN.md section 1b step 2).</summary>
        public int TissueLumenEdgeAxisIndex => BaseBandCells + TissueBandCells - 1;

        /// <summary>Axis index of the lumen cell touching the gut interface
        /// (lumen depth 0).</summary>
        public int LumenNearWallAxisIndex => BaseBandCells + TissueBandCells;

        public BoardBand BandAtAxisIndex(int axisIndex)
        {
            if (axisIndex < BaseBandCells) return BoardBand.Base;
            if (axisIndex < BaseBandCells + TissueBandCells) return BoardBand.Tissue;
            return BoardBand.Lumen;
        }

        public BoardBand BandOf(CoarseCoord c) => BandAtAxisIndex(AxisIndex(c));

        /// <summary>How many cells a lumen position sits out from the gut
        /// interface. 0 = hugging the wall, LumenBandCells-1 = far side of
        /// the channel. This is the distance GAME_DESIGN.md section 1b step
        /// 1's adhesion probability is gated on. Meaningless (negative) for
        /// non-lumen cells; callers check the band first.</summary>
        public int LumenDepthFromInterface(int axisIndex) => axisIndex - LumenNearWallAxisIndex;

        public int LumenDepthFromInterface(CoarseCoord c) => LumenDepthFromInterface(AxisIndex(c));

        // ------------------------------------------------------------------
        // Lumen flow
        // ------------------------------------------------------------------

        /// <summary>+1 or -1 in RAW cross-index space: the direction the
        /// lumen flow carries a pathogen (GAME_DESIGN.md section 1a: "top to
        /// bottom" on Map 01).</summary>
        public int FlowCrossStep => lumenFlowEnd == AxisEnd.Positive ? 1 : -1;

        /// <summary>Cross index a pathogen enters the lumen at -- the
        /// upstream end of the flow.</summary>
        public int LumenEntryCrossIndex => lumenFlowEnd == AxisEnd.Positive ? 0 : CrossLength - 1;

        /// <summary>True once a flowing pathogen has been carried off the
        /// downstream end of the channel and is excreted.</summary>
        public bool IsExcretedCrossIndex(int crossIndex) => !InCrossBounds(crossIndex);

        // ------------------------------------------------------------------
        // Bounds + world space
        // ------------------------------------------------------------------

        public bool InFineBounds(FineCoord c) =>
            c.Column >= 0 && c.Column < FineColumns && c.Row >= 0 && c.Row < FineRows;

        public bool InCoarseBounds(CoarseCoord c) =>
            c.Column >= 0 && c.Column < columns && c.Row >= 0 && c.Row < rows;

        /// <summary>World-space position of a fine tile's centre. The
        /// board is centred on the world origin.</summary>
        public Vector3 FineToWorld(FineCoord c)
        {
            float x = (c.Column + 0.5f) * FineTileWorldSize - BoardWorldWidth * 0.5f;
            float y = BoardWorldHeight * 0.5f - (c.Row + 0.5f) * FineTileWorldSize;
            return new Vector3(x, y, 0f);
        }

        public FineCoord CoarseCenterFine(CoarseCoord c) => new FineCoord(
            c.Column * FineSubdivision + FineSubdivision / 2,
            c.Row * FineSubdivision + FineSubdivision / 2);

        public Vector3 CoarseToWorldCenter(CoarseCoord c) => FineToWorld(CoarseCenterFine(c));

        public float CoarseCellWorldSize => FineSubdivision * FineTileWorldSize;

        public float BoardWorldWidth => FineColumns * FineTileWorldSize;
        public float BoardWorldHeight => FineRows * FineTileWorldSize;

        /// <summary>World-space centre of one gut-interface position -- the
        /// seam between the lumen-edge tissue cell and the wall-adjacent
        /// lumen cell, for the given lane.</summary>
        public Vector3 InterfaceWorldCenter(int crossIndex)
        {
            var tissueSide = CoarseToWorldCenter(CoarseFromAxis(TissueLumenEdgeAxisIndex, crossIndex));
            var lumenSide = CoarseToWorldCenter(CoarseFromAxis(LumenNearWallAxisIndex, crossIndex));
            return (tissueSide + lumenSide) * 0.5f;
        }

        /// <summary>World-space bounds of one band, for backdrops and
        /// compartment placement.</summary>
        public Rect BandWorldRect(BoardBand band)
        {
            int start, count;
            switch (band)
            {
                case BoardBand.Base: start = 0; count = BaseBandCells; break;
                case BoardBand.Tissue: start = BaseBandCells; count = TissueBandCells; break;
                default: start = LumenNearWallAxisIndex; count = LumenBandCells; break;
            }
            if (count <= 0) return new Rect(0, 0, 0, 0);

            var a = CoarseToWorldCenter(CoarseFromAxis(start, 0));
            var b = CoarseToWorldCenter(CoarseFromAxis(start + count - 1, CrossLength - 1));
            float half = CoarseCellWorldSize * 0.5f;
            return Rect.MinMaxRect(
                Mathf.Min(a.x, b.x) - half, Mathf.Min(a.y, b.y) - half,
                Mathf.Max(a.x, b.x) + half, Mathf.Max(a.y, b.y) + half);
        }

        /// <summary>Test/bootstrap hook for building a board with
        /// non-default geometry from code (Assets/Editor/MapVerification.cs
        /// uses this to prove the base-direction abstraction by putting the
        /// base on the opposite side). Not called in normal gameplay --
        /// production values come from the serialized fields above.</summary>
        public void ConfigureForTest(
            int newColumns, int newRows, BoardAxis axis, AxisEnd newBaseEnd,
            int newBaseBandCells, int newLumenBandCells, AxisEnd newFlowEnd)
        {
            columns = newColumns;
            rows = newRows;
            threatAxis = axis;
            baseEnd = newBaseEnd;
            baseBandCells = newBaseBandCells;
            lumenBandCells = newLumenBandCells;
            lumenFlowEnd = newFlowEnd;
        }
    }
}
