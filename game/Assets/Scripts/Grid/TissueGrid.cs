using System.Collections.Generic;
using UnityEngine;
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
    ///
    /// Sprint 1 closing task: a slot that has a pathogen adhered to it is
    /// also, conceptually, an INFECTED host cell -- and it's the infection,
    /// not "wherever a pathogen happens to be," that secretes cytokines.
    /// That's modelled here (not as a separate class -- the design brief
    /// judged a full split not worth it for one sprint) as a per-slot
    /// secretion timer, distinct from bare occupancy: adhesion starts it,
    /// and GetSecretionStrength ramps continuously from a base level up to
    /// a cap over time, rather than being a fixed one-shot value keyed off
    /// "is a pathogen here." CytokineField reads this, not raw occupancy.
    /// currentTime is passed in by the caller (real gameplay passes
    /// UnityEngine.Time.time) rather than read internally, so this class
    /// stays plain C# and testable headlessly -- see
    /// Assets/Editor/CytokineVerification.cs, which drives it with a fake
    /// clock with no GameObjects/play mode required.
    /// </summary>
    public class TissueGrid
    {
        private readonly BoardConfig board;
        private readonly PathogenAgent[,] pathogenBySlot;
        private readonly float[,] adhesionStartTime;

        /// <summary>Secretion strength (arbitrary field units, same scale
        /// CytokineField's old flat SourceStrength used) at the moment a
        /// cell becomes infected.</summary>
        public const float BaseSecretionStrength = 6f;

        /// <summary>Secretion strength once fully ramped up. Also used as
        /// the normalization reference for the heatmap visual cue
        /// (BoardRenderer) -- a single freshly-ramped infected cell reads
        /// as "fully hot" at its own location.</summary>
        public const float MaxSecretionStrength = 32f;

        /// <summary>Seconds for a newly-adhered cell's secretion to ramp
        /// from Base to Max. Tuned so the Director can watch a site "heat
        /// up" over roughly half a minute, not instantly and not so slowly
        /// it's imperceptible in a short playtest.</summary>
        public const float InfectionRampSeconds = 20f;

        public int AdheredCount { get; private set; }

        public TissueGrid(BoardConfig board)
        {
            this.board = board;
            pathogenBySlot = new PathogenAgent[board.Columns, BoardConfig.Rows];
            adhesionStartTime = new float[board.Columns, BoardConfig.Rows];
            for (int col = 0; col < board.Columns; col++)
                for (int row = 0; row < BoardConfig.Rows; row++)
                    adhesionStartTime[col, row] = -1f;
        }

        public bool IsSlotFree(CoarseCoord c) =>
            board.InCoarseBounds(c) && pathogenBySlot[c.Column, c.Row] == null;

        public bool TryAdhere(CoarseCoord c, PathogenAgent pathogen, float currentTime)
        {
            if (!IsSlotFree(c)) return false;
            pathogenBySlot[c.Column, c.Row] = pathogen;
            adhesionStartTime[c.Column, c.Row] = currentTime;
            AdheredCount++;
            return true;
        }

        public void ReleaseSlot(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return;
            if (pathogenBySlot[c.Column, c.Row] != null)
            {
                pathogenBySlot[c.Column, c.Row] = null;
                adhesionStartTime[c.Column, c.Row] = -1f;
                AdheredCount--;
            }
        }

        public PathogenAgent GetPathogenAt(CoarseCoord c) =>
            board.InCoarseBounds(c) ? pathogenBySlot[c.Column, c.Row] : null;

        /// <summary>Current cytokine secretion strength of the infected
        /// host cell at this slot -- 0 if the slot isn't infected/adhered.
        /// Ramps linearly from BaseSecretionStrength to MaxSecretionStrength
        /// over InfectionRampSeconds of infection age.</summary>
        public float GetSecretionStrength(CoarseCoord c, float currentTime)
        {
            if (!board.InCoarseBounds(c)) return 0f;
            float start = adhesionStartTime[c.Column, c.Row];
            if (start < 0f) return 0f;
            float age = Mathf.Max(0f, currentTime - start);
            float t = Mathf.Clamp01(age / InfectionRampSeconds);
            return Mathf.Lerp(BaseSecretionStrength, MaxSecretionStrength, t);
        }

        /// <summary>Bare coarse coordinates of every adhered slot, with no
        /// secretion-strength data. No longer used internally as of the
        /// Sprint 1 closing task -- CytokineField.Recompute now consumes
        /// InfectedSources instead, since it needs each slot's ramping
        /// strength, not just its location. Kept public in case a future
        /// caller only cares about occupancy (e.g. rendering).</summary>
        public IEnumerable<CoarseCoord> AdheredCoords()
        {
            for (int col = 0; col < board.Columns; col++)
                for (int row = 0; row < BoardConfig.Rows; row++)
                    if (pathogenBySlot[col, row] != null)
                        yield return new CoarseCoord(col, row);
        }

        /// <summary>Every currently-infected coarse slot paired with its
        /// current secretion strength -- what CytokineField.Recompute
        /// consumes. Distinct from AdheredCoords (still kept, e.g. for
        /// rendering) in that it carries the continuous-secretion value,
        /// not just "a pathogen is here."</summary>
        public IEnumerable<(CoarseCoord Coord, float Strength)> InfectedSources(float currentTime)
        {
            for (int col = 0; col < board.Columns; col++)
                for (int row = 0; row < BoardConfig.Rows; row++)
                    if (pathogenBySlot[col, row] != null)
                    {
                        var coord = new CoarseCoord(col, row);
                        yield return (coord, GetSecretionStrength(coord, currentTime));
                    }
        }
    }
}
