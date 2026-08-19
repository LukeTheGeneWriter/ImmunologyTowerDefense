using System;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// A coordinate on the fine movement lattice, in GLOBAL fine-tile
    /// units (not local to one coarse cell) -- global fine column =
    /// coarseColumn * FineSubdivision + localColumn, same for row. This is
    /// the layer immune cells and pathogens actually walk on
    /// (GAME_DESIGN.md section 7).
    /// </summary>
    public readonly struct FineCoord : IEquatable<FineCoord>
    {
        public readonly int Column;
        public readonly int Row;

        public FineCoord(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public CoarseCoord ToCoarse(int subdivision) =>
            new CoarseCoord(FloorDiv(Column, subdivision), FloorDiv(Row, subdivision));

        private static int FloorDiv(int a, int b) => a >= 0 ? a / b : -(((-a) + b - 1) / b);

        public FineCoord Add(FineCoord offset) => new FineCoord(Column + offset.Column, Row + offset.Row);

        public bool Equals(FineCoord other) => Column == other.Column && Row == other.Row;
        public override bool Equals(object obj) => obj is FineCoord other && Equals(other);
        public override int GetHashCode() => (Column * 397) ^ Row;
        public override string ToString() => $"Fine({Column},{Row})";

        public static bool operator ==(FineCoord a, FineCoord b) => a.Equals(b);
        public static bool operator !=(FineCoord a, FineCoord b) => !a.Equals(b);

        /// <summary>Von Neumann (four-neighbour) step offsets -- movement
        /// is explicitly not eight-directional, per GAME_DESIGN.md section 7
        /// ("legal diagonals make vertical progress cost the same as
        /// horizontal").</summary>
        public static readonly FineCoord[] VonNeumannOffsets =
        {
            new FineCoord(1, 0),
            new FineCoord(-1, 0),
            new FineCoord(0, 1),
            new FineCoord(0, -1),
        };
    }
}
