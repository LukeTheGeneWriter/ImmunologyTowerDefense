using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Sprint 19: the numbers for the two gut-wall structures. Mutable
    /// statics with <see cref="ResetToDefaults"/>, the project's tuning
    /// pattern. All placeholder -- mechanics first.
    /// </summary>
    public static class GutWallTuning
    {
        /// <summary>Peyer's patch: wall lanes it spans, and where along the
        /// flow its centre sits (0 = entry, 1 = exit).</summary>
        public static int PatchWidthLanes = 4;
        public static float PatchCenter01 = 0.6f;
        /// <summary>How deep into the tissue (cells from the lumen edge) the
        /// follicle reaches -- the zone where a DC can collect M-cell antigen.</summary>
        public static int FollicleDepthCells = 3;
        /// <summary>Per second, per lumen pathogen over the dome, per level
        /// of the M-cell upgrade: the chance it gets sampled.</summary>
        public static float MCellSampleChancePerSecond = 0.08f;
        /// <summary>Antigen parcels the dome can hold waiting for a DC.</summary>
        public static int MCellAntigenCapacity = 3;

        /// <summary>Crypt: regrowth speed-up on Empty ground within reach,
        /// as an extra divisor term (1 + this), and the reach in cells
        /// (Chebyshev, from the crypt's mouth at the wall).</summary>
        public static float CryptRegrowthBonus = 2f;
        public static int CryptRadiusCells = 3;

        public static void ResetToDefaults()
        {
            PatchWidthLanes = 4;
            PatchCenter01 = 0.6f;
            FollicleDepthCells = 3;
            MCellSampleChancePerSecond = 0.08f;
            MCellAntigenCapacity = 3;
            CryptRegrowthBonus = 2f;
            CryptRadiusCells = 3;
        }
    }

    /// <summary>
    /// Sprint 19: the Peyer's patch and the crypts -- the two gut-wall
    /// structures the Director asked for after the lumen reference research
    /// (docs/LUMEN_REFERENCE.md P4, P6).
    ///
    /// **Peyer's patch.** A fixed stretch of the wall (no villi there -- the
    /// renderer reads <see cref="IsPatchLane"/>) over a lymphoid follicle in
    /// the tissue. Always drawn. Its job, M-cell sampling, is a **shop
    /// upgrade** (Director, 2026-10-13: "M cell functionality as an upgrade
    /// not straight out of the box"): at <see cref="MCellLevel"/> 0 it is
    /// a landmark only. Once bought, pathogens drifting over the dome are
    /// sampled -- M cells transcytose antigen, the pathogen is not harmed --
    /// and the antigen waits in the dome for a DC patrolling the follicle
    /// to collect. That is knowledge from the lumen side, without waiting
    /// for tissue to die and leave debris (§5a), which is the real reason
    /// Peyer's patches exist.
    ///
    /// **Crypts.** Bought from the shop (repeatable). Each sits at the wall
    /// -- real crypts are invaginations of the epithelium, never free in the
    /// tissue (Director's call: snap to the wall) -- and speeds regrowth of
    /// Empty ground within <see cref="GutWallTuning.CryptRadiusCells"/>.
    /// Positions come from a fixed, well-spread sequence, so buying a second
    /// crypt never moves the first.
    ///
    /// Explicit-time model like the rest of the simulation; no
    /// UnityEngine.Time. Reached via the null-safe static
    /// <see cref="Current"/> (the EconomyHooks / RoundClock pattern), so
    /// TissueGrid, DendriticCell and the spawner need no new references
    /// threaded through, and a harness that never sets it sees Sprint 18
    /// behaviour exactly.
    /// </summary>
    public class GutWallFeatures
    {
        public static GutWallFeatures Current;

        private readonly BoardConfig board;
        private readonly List<int> cryptLanes = new List<int>();
        private readonly Queue<PathogenClass> antigen = new Queue<PathogenClass>();

        public int PatchCrossStart { get; }
        public int PatchCrossEnd { get; }   // inclusive

        /// <summary>Levels of the M-cell upgrade. 0 = landmark only.</summary>
        public int MCellLevel { get; set; }

        public IReadOnlyList<int> CryptLanes => cryptLanes;
        public int PendingAntigen => antigen.Count;
        /// <summary>Lifetime count, for the debug readout and harnesses.</summary>
        public int AntigenSampled { get; private set; }
        public int AntigenCollected { get; private set; }

        /// <summary>Bumped whenever the crypt set changes, so a renderer
        /// can rebuild only then.</summary>
        public int CryptVersion { get; private set; }

        public GutWallFeatures(BoardConfig board)
        {
            this.board = board;
            int lanes = board.CrossLength;
            int width = Mathf.Clamp(GutWallTuning.PatchWidthLanes, 1, Mathf.Max(1, lanes - 2));
            // Along-the-flow position, measured from the ENTRY end so a
            // mirrored board puts the patch at the same point in the transit.
            int fromEntry = Mathf.RoundToInt(GutWallTuning.PatchCenter01 * (lanes - 1)) - width / 2;
            fromEntry = Mathf.Clamp(fromEntry, 0, lanes - width);
            int a = board.LumenEntryCrossIndex + board.FlowCrossStep * fromEntry;
            int b = a + board.FlowCrossStep * (width - 1);
            PatchCrossStart = Mathf.Min(a, b);
            PatchCrossEnd = Mathf.Max(a, b);
        }

        public bool IsPatchLane(int cross) => cross >= PatchCrossStart && cross <= PatchCrossEnd;

        // ------------------------------------------------------------------
        // Crypts
        // ------------------------------------------------------------------

        /// <summary>Set how many crypts exist (the shop level). Grows or
        /// shrinks the list along a fixed van der Corput sequence over the
        /// non-patch lanes, so existing crypts never move.</summary>
        public void SetCryptCount(int count)
        {
            count = Mathf.Max(0, count);
            if (count == cryptLanes.Count) return;
            if (count < cryptLanes.Count)
            {
                cryptLanes.RemoveRange(count, cryptLanes.Count - count);
                CryptVersion++;
                return;
            }
            int lanes = board.CrossLength;
            for (int k = 1; cryptLanes.Count < count && k < 4096; k++)
            {
                int lane = Mathf.Clamp(Mathf.FloorToInt(VanDerCorput(k) * lanes), 0, lanes - 1);
                // Keep crypts off the patch and its immediate shoulders, and
                // never stack two on one lane.
                if (lane >= PatchCrossStart - 1 && lane <= PatchCrossEnd + 1) continue;
                if (cryptLanes.Contains(lane)) continue;
                cryptLanes.Add(lane);
            }
            CryptVersion++;
        }

        /// <summary>Divisor term for TissueGrid's regrowth: 1 + bonus when
        /// <paramref name="c"/> is within reach of any crypt, else 1. Crypt
        /// coverage is spatial, not stacking -- more crypts cover more wall,
        /// they do not make one spot regrow faster.</summary>
        public float RegrowthMultiplier(CoarseCoord c)
        {
            if (cryptLanes.Count == 0) return 1f;
            int depth = board.TissueLumenEdgeAxisIndex - board.AxisIndex(c);  // 0 = the wall-side row
            if (depth < 0) return 1f;
            int cross = board.CrossIndex(c);
            int r = GutWallTuning.CryptRadiusCells;
            for (int i = 0; i < cryptLanes.Count; i++)
                if (depth <= r && Mathf.Abs(cross - cryptLanes[i]) <= r)
                    return 1f + GutWallTuning.CryptRegrowthBonus;
            return 1f;
        }

        // ------------------------------------------------------------------
        // Peyer's patch / M cells
        // ------------------------------------------------------------------

        /// <summary>Is <paramref name="c"/> a lumen-side cell over the dome
        /// (the wall row or the one beside it) -- where M cells can reach?</summary>
        public bool IsOverDome(CoarseCoord c)
        {
            int axis = board.AxisIndex(c);
            return IsPatchLane(board.CrossIndex(c))
                   && axis >= board.LumenNearWallAxisIndex && axis <= board.LumenNearWallAxisIndex + 1;
        }

        /// <summary>Is <paramref name="c"/> inside the follicle -- tissue
        /// under the dome, within FollicleDepthCells of the lumen edge?</summary>
        public bool IsInFollicle(CoarseCoord c)
        {
            if (!IsPatchLane(board.CrossIndex(c))) return false;
            int depth = board.TissueLumenEdgeAxisIndex - board.AxisIndex(c);
            return depth >= 0 && depth < GutWallTuning.FollicleDepthCells;
        }

        /// <summary>One roll for one pathogen sitting at
        /// <paramref name="at"/> for <paramref name="deltaTime"/> seconds.
        /// <paramref name="roll01"/> is the caller's random draw (so a
        /// harness can force it). Returns true when antigen was taken.</summary>
        public bool TrySample(CoarseCoord at, PathogenClass cls, float deltaTime, float roll01)
        {
            if (MCellLevel <= 0) return false;
            if (antigen.Count >= GutWallTuning.MCellAntigenCapacity) return false;
            if (!IsOverDome(at)) return false;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(GutWallTuning.MCellSampleChancePerSecond * MCellLevel), deltaTime);
            if (roll01 >= p) return false;
            antigen.Enqueue(cls);
            AntigenSampled++;
            return true;
        }

        /// <summary>A cargo-less DC standing in the follicle collects the
        /// oldest waiting parcel.</summary>
        public bool TryTakeAntigen(CoarseCoord dcAt, out PathogenClass cls)
        {
            cls = default;
            if (antigen.Count == 0 || !IsInFollicle(dcAt)) return false;
            cls = antigen.Dequeue();
            AntigenCollected++;
            return true;
        }

        private static float VanDerCorput(int n)
        {
            float v = 0f, denom = 1f;
            while (n > 0) { denom *= 2f; v += (n & 1) / denom; n >>= 1; }
            return v;
        }
    }
}
