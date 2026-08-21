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
    /// every tick, the field is recomputed from scratch on a timer (see
    /// PathogenSpawner) as an inverse-Manhattan-distance falloff from every
    /// infected coarse slot, weighted by that slot's own secretion strength
    /// (TissueGrid.InfectedSources -- continuous, ramping per infected
    /// cell, not a flat per-source constant). For a mostly-static set of
    /// source LOCATIONS this reads the same as a settled diffusion field
    /// and is far cheaper at this board scale (<=200 coarse cells); the
    /// per-source STRENGTH still changes continuously as infections ramp,
    /// which is why this is now recomputed periodically rather than only
    /// when the adhered set changes (Sprint 1 closing task -- see
    /// docs/INTERFACE.md). Recorded here and in docs/INTERFACE.md as a
    /// documented simplification.
    /// </summary>
    public class CytokineField
    {
        private readonly BoardConfig board;
        private readonly float[,] field;

        /// <summary>Reused across Recompute calls. Sprint 1-3 allocated a
        /// fresh float[,] plus a fresh source List every recompute, which
        /// was 150 floats on a 30x5 board and is 4,000 on Map 01 -- 2.5
        /// allocations a second of steadily-growing garbage, against
        /// GAME_DESIGN.md section 8's no-per-frame-allocation rule. Both
        /// buffers are now owned and cleared in place.</summary>
        private readonly List<(CoarseCoord Coord, float Strength)> sourceBuffer =
            new List<(CoarseCoord Coord, float Strength)>(64);

        public CytokineField(BoardConfig board)
        {
            this.board = board;
            field = new float[board.Columns, board.Rows];
        }

        public void Recompute(IEnumerable<(CoarseCoord Coord, float Strength)> sources)
        {
            sourceBuffer.Clear();
            foreach (var s in sources) sourceBuffer.Add(s);

            int columns = board.Columns;
            int rows = board.Rows;
            int sourceCount = sourceBuffer.Count;

            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    float value = 0f;
                    for (int i = 0; i < sourceCount; i++)
                    {
                        var s = sourceBuffer[i];
                        int dist = System.Math.Abs(s.Coord.Column - col) + System.Math.Abs(s.Coord.Row - row);
                        value += s.Strength / (1f + dist);
                    }
                    field[col, row] = value;
                }
            }
        }

        private float ValueAt(int col, int row)
        {
            col = Mathf.Clamp(col, 0, board.Columns - 1);
            row = Mathf.Clamp(row, 0, board.Rows - 1);
            return field[col, row];
        }

        /// <summary>Raw coarse-cell field value, no interpolation --
        /// used by BoardRenderer for the heatmap visual cue, which paints
        /// per coarse cell rather than per fine tile.</summary>
        public float CoarseValueAt(CoarseCoord c) => ValueAt(c.Column, c.Row);

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
