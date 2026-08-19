using System.Collections.Generic;
using UnityEngine;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Rung-2 cytokine gradient (GAME_DESIGN.md sections 2a/7/9's
    /// chemotaxis table). Computed on the COARSE grid only, per section 7's
    /// implementation note -- diffusing across thousands of fine tiles per
    /// tick is called out there as the likely frame-budget trap. The fine
    /// lattice samples this coarse field via bilinear interpolation instead
    /// of diffusing directly.
    ///
    /// Implementation choice: rather than iterating a literal diffusion PDE
    /// every tick, the field is recomputed from scratch whenever the set of
    /// adhered pathogens changes, as an inverse-Manhattan-distance falloff
    /// from every adhered pathogen's coarse slot. For a mostly-static set
    /// of sources this reads the same as a settled diffusion field and is
    /// far cheaper at this board scale (<=200 coarse cells). Recorded here
    /// and in docs/INTERFACE.md as a documented simplification.
    /// </summary>
    public class CytokineField
    {
        private readonly BoardConfig board;
        private float[,] field;

        private const float SourceStrength = 10f;

        public CytokineField(BoardConfig board)
        {
            this.board = board;
            field = new float[board.Columns, BoardConfig.Rows];
        }

        public void Recompute(IEnumerable<CoarseCoord> sources)
        {
            var sourceList = new List<CoarseCoord>(sources);
            var next = new float[board.Columns, BoardConfig.Rows];
            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < BoardConfig.Rows; row++)
                {
                    float value = 0f;
                    foreach (var s in sourceList)
                    {
                        int dist = System.Math.Abs(s.Column - col) + System.Math.Abs(s.Row - row);
                        value += SourceStrength / (1f + dist);
                    }
                    next[col, row] = value;
                }
            }
            field = next;
        }

        private float ValueAt(int col, int row)
        {
            col = Mathf.Clamp(col, 0, board.Columns - 1);
            row = Mathf.Clamp(row, 0, BoardConfig.Rows - 1);
            return field[col, row];
        }

        /// <summary>Bilinear-interpolated field value at a fine-grid
        /// coordinate, sampled from the surrounding coarse cells.</summary>
        public float SampleFine(FineCoord fine)
        {
            float coarseColF = (fine.Column + 0.5f) / BoardConfig.FineSubdivision - 0.5f;
            float coarseRowF = (fine.Row + 0.5f) / BoardConfig.FineSubdivision - 0.5f;

            int c0 = Mathf.FloorToInt(coarseColF);
            int r0 = Mathf.FloorToInt(coarseRowF);
            float tx = coarseColF - c0;
            float ty = coarseRowF - r0;

            float v00 = ValueAt(c0, r0);
            float v10 = ValueAt(c0 + 1, r0);
            float v01 = ValueAt(c0, r0 + 1);
            float v11 = ValueAt(c0 + 1, r0 + 1);

            float top = Mathf.Lerp(v00, v10, tx);
            float bottom = Mathf.Lerp(v01, v11, tx);
            return Mathf.Lerp(top, bottom, ty);
        }
    }
}
