using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// The gut interface -- the epithelial boundary at the lumen/tissue
    /// seam, and the second stage of GAME_DESIGN.md section 1b's invasion
    /// loop. One "position" per lane (<see cref="BoardConfig.CrossLength"/>
    /// of them, 40 on Map 01).
    ///
    /// ## Why this is not TissueGrid
    ///
    /// A colonising pathogen is explicitly "not yet in the tissue, and not
    /// yet the player's problem, but accumulating" (section 1b step 1), and
    /// TissueGrid holds exactly ONE occupant per coarse slot -- it
    /// structurally cannot express a pile. So the interface is its own
    /// container: an unbounded list per position, holding pathogens that
    /// have left the flow but not yet entered tissue.
    ///
    /// ## The breach, and the one way to get it wrong
    ///
    /// Each position rolls its OWN breach chance every
    /// InvasionTuning.BreachRollIntervalSeconds, and the chance rises with
    /// how many pathogens are piled there
    /// (InvasionTuning.PerPathogenBreachChance). When a position's roll
    /// trips, **every pathogen at that position enters tissue in the same
    /// call** (<see cref="Breach"/>).
    ///
    /// SPRINT_PLAN.md item 6 is emphatic about this and it is worth
    /// repeating at the implementation: do NOT refactor this into a
    /// per-pathogen independent invasion roll. It looks equivalent and it is
    /// not -- per-pathogen rolls produce a steady trickle across the whole
    /// wall, which destroys the "pressure visibly builds at one spot, then
    /// bursts" shape that is the entire point of the mechanic.
    ///
    /// Explicit time throughout (<see cref="Tick"/> takes deltaTime, Breach
    /// takes currentTime) so Assets/Editor/MapVerification.cs drives the
    /// real production path with a simulated clock, per this project's
    /// standing convention.
    /// </summary>
    public class GutInterface
    {
        private readonly BoardConfig board;
        private readonly TissueGrid tissueGrid;
        private readonly InvasionTally tally;
        private readonly List<PathogenAgent>[] adheredByPosition;

        /// <summary>Positions that breached on the most recent
        /// <see cref="Tick"/>. A reused buffer -- read it before the next
        /// tick, do not retain it. Exposed so a renderer can play one burst
        /// effect per breach without polling every position.</summary>
        private readonly List<int> breachedThisTick = new List<int>();

        private float rollTimer;

        /// <summary>Fired once per breach, with (position, pathogens
        /// released). An event rather than "poll BreachedThisTick" because
        /// script execution order between PathogenSpawner (which ticks this)
        /// and a renderer's Update() is undefined, so polling would silently
        /// drop roughly half the bursts -- and a burst that isn't drawn is
        /// the one thing SPRINT_PLAN.md item 6 says must be legible.</summary>
        public event System.Action<int, int> Breached;

        public GutInterface(BoardConfig board, TissueGrid tissueGrid, InvasionTally tally)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.tally = tally;
            adheredByPosition = new List<PathogenAgent>[board.CrossLength];
            for (int i = 0; i < adheredByPosition.Length; i++)
                adheredByPosition[i] = new List<PathogenAgent>();
        }

        public int PositionCount => adheredByPosition.Length;

        /// <summary>Total pathogens colonising the wall, across all
        /// positions.</summary>
        public int TotalAdhered { get; private set; }

        public IReadOnlyList<int> BreachedThisTick => breachedThisTick;

        public int AdheredCountAt(int position) =>
            InRange(position) ? adheredByPosition[position].Count : 0;

        public IReadOnlyList<PathogenAgent> AdheredAt(int position) =>
            InRange(position) ? (IReadOnlyList<PathogenAgent>)adheredByPosition[position] : System.Array.Empty<PathogenAgent>();

        /// <summary>The highest adhered count at any position right now --
        /// what the renderer normalizes its "pressure" tint against.</summary>
        public int PeakAdhered
        {
            get
            {
                int peak = 0;
                for (int i = 0; i < adheredByPosition.Length; i++)
                    if (adheredByPosition[i].Count > peak) peak = adheredByPosition[i].Count;
                return peak;
            }
        }

        /// <summary>Adds a pathogen to a boundary position. Called by
        /// PathogenAgent when its proximity-gated adhesion roll succeeds.</summary>
        public bool Adhere(int position, PathogenAgent pathogen)
        {
            if (!InRange(position) || pathogen == null) return false;
            adheredByPosition[position].Add(pathogen);
            TotalAdhered++;
            if (tally != null) tally.Adhesions++;
            return true;
        }

        /// <summary>Removes a pathogen from wherever it is adhered -- for a
        /// pathogen cleared or despawned while still on the wall. Linear in
        /// one position's pile, which is small.</summary>
        public bool Remove(PathogenAgent pathogen)
        {
            if (pathogen == null) return false;
            for (int i = 0; i < adheredByPosition.Length; i++)
            {
                if (adheredByPosition[i].Remove(pathogen))
                {
                    TotalAdhered--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>This position's breach chance for one roll:
        /// `1 - (1 - perPathogen)^n`. Rises with pressure -- see
        /// InvasionTuning.PerPathogenBreachChance for why that matters.</summary>
        public float BreachChanceAt(int position)
        {
            int n = AdheredCountAt(position);
            if (n <= 0) return 0f;
            float perPathogen = Mathf.Clamp01(InvasionTuning.PerPathogenBreachChance);
            return 1f - Mathf.Pow(1f - perPathogen, n);
        }

        /// <summary>
        /// Advances the breach clock and rolls every occupied position when
        /// it comes due. Returns the positions that breached (also available
        /// as <see cref="BreachedThisTick"/>); the pathogens at those
        /// positions have ALREADY been moved into tissue by the time this
        /// returns.
        /// </summary>
        public IReadOnlyList<int> Tick(float deltaTime, float currentTime)
        {
            breachedThisTick.Clear();
            if (TotalAdhered == 0)
            {
                // Keep the clock from banking up while the wall is clean, so
                // the first pathogen to adhere waits a full interval rather
                // than being hit by an instant roll.
                rollTimer = 0f;
                return breachedThisTick;
            }

            rollTimer += deltaTime;
            float interval = Mathf.Max(0.0001f, InvasionTuning.BreachRollIntervalSeconds);
            while (rollTimer >= interval)
            {
                rollTimer -= interval;
                for (int position = 0; position < adheredByPosition.Length; position++)
                {
                    if (adheredByPosition[position].Count == 0) continue;
                    if (Random.value >= BreachChanceAt(position)) continue;
                    Breach(position, currentTime);
                    breachedThisTick.Add(position);
                }
            }
            return breachedThisTick;
        }

        /// <summary>
        /// Releases EVERY pathogen adhered at one position into tissue, in
        /// this single call -- the burst. Also the deterministic entry point
        /// a harness (or a future "the player pops the abscess" ability)
        /// uses to trip a specific position without waiting on a roll.
        /// </summary>
        /// <returns>How many pathogens entered tissue.</returns>
        public int Breach(int position, float currentTime)
        {
            if (!InRange(position)) return 0;
            var pile = adheredByPosition[position];
            if (pile.Count == 0) return 0;

            int released = 0;
            // Iterate a copy-free index walk and compact afterwards: a
            // pathogen that cannot find a slot (whole neighbourhood full)
            // stays on the wall rather than being dropped.
            int writeIndex = 0;
            for (int i = 0; i < pile.Count; i++)
            {
                var pathogen = pile[i];
                if (pathogen != null && TryFindReleaseSlot(position, out var slot))
                {
                    pathogen.EnterTissueAt(slot, currentTime);
                    released++;
                }
                else
                {
                    pile[writeIndex++] = pathogen;
                }
            }
            pile.RemoveRange(writeIndex, pile.Count - writeIndex);

            TotalAdhered -= released;
            if (released > 0)
            {
                if (tally != null)
                {
                    tally.Breaches++;
                    tally.ReleasedIntoTissue += released;
                }
                Breached?.Invoke(position, released);
            }
            return released;
        }

        /// <summary>
        /// Finds a free tissue slot for a pathogen bursting through at
        /// <paramref name="position"/>. Searches outward from the
        /// lumen-edge tissue cell in that lane, cheapest first, so a burst
        /// fans ALONG the wall (lanes) before it punches deeper into tissue
        /// -- a breach should read as a widening hole at one spot, not as a
        /// spear.
        ///
        /// Expressed entirely in the axis frame (BoardConfig.CoarseFromAxis)
        /// -- "one cell deeper" is `axisIndex - 1`, never `Column - 1`.
        /// </summary>
        public bool TryFindReleaseSlot(int position, out CoarseCoord slot)
        {
            int edge = board.TissueLumenEdgeAxisIndex;
            int maxDepth = Mathf.Max(0, InvasionTuning.MaxReleaseAxisDepth);
            int maxSpread = Mathf.Max(0, InvasionTuning.MaxReleaseCrossSpread);

            for (int total = 0; total <= maxDepth + maxSpread; total++)
            {
                for (int depth = 0; depth <= Mathf.Min(total, maxDepth); depth++)
                {
                    int spread = total - depth;
                    if (spread > maxSpread) continue;

                    int axisIndex = edge - depth;
                    if (board.BandAtAxisIndex(axisIndex) != BoardBand.Tissue) continue;

                    for (int sign = -1; sign <= 1; sign += 2)
                    {
                        int cross = position + spread * sign;
                        if (!board.InCrossBounds(cross)) continue;
                        var candidate = board.CoarseFromAxis(axisIndex, cross);
                        if (!tissueGrid.IsOccupantFree(candidate)) continue;
                        slot = candidate;
                        return true;
                    }
                }
            }

            slot = default;
            return false;
        }

        private bool InRange(int position) => position >= 0 && position < adheredByPosition.Length;
    }
}
