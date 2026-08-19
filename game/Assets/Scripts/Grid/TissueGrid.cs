using System.Collections.Generic;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Coarse-grid occupancy. Each slot is either bare host tissue or has
    /// exactly one adhered pathogen -- this sprint doesn't model host cell
    /// health, damage, or multi-depth descent (see SPRINT_PLAN.md's
    /// exclusion list). A pathogen "adheres" by claiming a coarse slot
    /// outright and stays there for the rest of the session; nothing
    /// currently calls ReleaseSlot (no despawn/kill system exists yet),
    /// it's kept for the lifecycle work that comes after this sprint.
    /// </summary>
    public class TissueGrid
    {
        private readonly BoardConfig board;
        private readonly PathogenAgent[,] pathogenBySlot;

        public int AdheredCount { get; private set; }

        public TissueGrid(BoardConfig board)
        {
            this.board = board;
            pathogenBySlot = new PathogenAgent[board.Columns, BoardConfig.Rows];
        }

        public bool IsSlotFree(CoarseCoord c) =>
            board.InCoarseBounds(c) && pathogenBySlot[c.Column, c.Row] == null;

        public bool TryAdhere(CoarseCoord c, PathogenAgent pathogen)
        {
            if (!IsSlotFree(c)) return false;
            pathogenBySlot[c.Column, c.Row] = pathogen;
            AdheredCount++;
            return true;
        }

        public void ReleaseSlot(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return;
            if (pathogenBySlot[c.Column, c.Row] != null)
            {
                pathogenBySlot[c.Column, c.Row] = null;
                AdheredCount--;
            }
        }

        public PathogenAgent GetPathogenAt(CoarseCoord c) =>
            board.InCoarseBounds(c) ? pathogenBySlot[c.Column, c.Row] : null;

        public IEnumerable<CoarseCoord> AdheredCoords()
        {
            for (int col = 0; col < board.Columns; col++)
                for (int row = 0; row < BoardConfig.Rows; row++)
                    if (pathogenBySlot[col, row] != null)
                        yield return new CoarseCoord(col, row);
        }
    }
}
