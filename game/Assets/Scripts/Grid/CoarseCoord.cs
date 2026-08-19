using System;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// A coordinate on the coarse occupancy grid (one host cell or one
    /// adhered pathogen per slot -- GAME_DESIGN.md section 7). Column runs
    /// along the lumen (0 = leftmost, entry side); Row is a coarse depth
    /// band (0 = shallowest / nearest the lumen). This sprint only builds
    /// the tissue compartment and does not implement the full bone
    /// marrow/lymph/blood/tissue depth-5 model -- see docs/INTERFACE.md.
    /// </summary>
    public readonly struct CoarseCoord : IEquatable<CoarseCoord>
    {
        public readonly int Column;
        public readonly int Row;

        public CoarseCoord(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public bool Equals(CoarseCoord other) => Column == other.Column && Row == other.Row;
        public override bool Equals(object obj) => obj is CoarseCoord other && Equals(other);
        public override int GetHashCode() => (Column * 397) ^ Row;
        public override string ToString() => $"Coarse({Column},{Row})";

        public static bool operator ==(CoarseCoord a, CoarseCoord b) => a.Equals(b);
        public static bool operator !=(CoarseCoord a, CoarseCoord b) => !a.Equals(b);
    }
}
