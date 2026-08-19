using UnityEngine;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Board dimensions and world-space scale. Board width (coarse
    /// columns) is this sprint's headline configurable parameter --
    /// GAME_DESIGN.md section 7 identifies it as the primary difficulty
    /// knob ("search cost rises quadratically with board width"). Adjust
    /// the "Columns" field on the GameBootstrap object's Inspector; the
    /// 24-40 range needs no code change (the Range clamp is a sanity rail
    /// for this sprint's scope, not a hard engine limit).
    /// </summary>
    public class BoardConfig : MonoBehaviour
    {
        [Tooltip("Coarse columns spanning the lumen. GAME_DESIGN.md section 7: the primary difficulty knob. Sprint 1 default 30, usable 24-40 without code changes.")]
        [Range(24, 40)]
        [SerializeField] private int columns = 30;

        public const int Rows = 5;
        public const int FineSubdivision = 7;
        public const float FineTileWorldSize = 0.16f;

        /// <summary>Shared simulation tick length for both units and
        /// pathogens. "Fine tiles per tick" (SearchUnit, PathogenAgent) is
        /// measured against this.</summary>
        public const float TickIntervalSeconds = 0.12f;

        public int Columns => columns;
        public int FineColumns => columns * FineSubdivision;
        public int FineRows => Rows * FineSubdivision;

        public bool InFineBounds(FineCoord c) =>
            c.Column >= 0 && c.Column < FineColumns && c.Row >= 0 && c.Row < FineRows;

        public bool InCoarseBounds(CoarseCoord c) =>
            c.Column >= 0 && c.Column < columns && c.Row >= 0 && c.Row < Rows;

        /// <summary>World-space position of a fine tile's centre. The
        /// board is centred on the world origin.</summary>
        public Vector3 FineToWorld(FineCoord c)
        {
            float x = (c.Column + 0.5f) * FineTileWorldSize - BoardWorldWidth * 0.5f;
            float y = BoardWorldHeight * 0.5f - (c.Row + 0.5f) * FineTileWorldSize;
            return new Vector3(x, y, 0f);
        }

        public Vector3 CoarseToWorldCenter(CoarseCoord c)
        {
            var centerFine = new FineCoord(
                c.Column * FineSubdivision + FineSubdivision / 2,
                c.Row * FineSubdivision + FineSubdivision / 2);
            return FineToWorld(centerFine);
        }

        public float BoardWorldWidth => FineColumns * FineTileWorldSize;
        public float BoardWorldHeight => FineRows * FineTileWorldSize;
    }
}
