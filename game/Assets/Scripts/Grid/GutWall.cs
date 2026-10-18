using System.Collections.Generic;
using ImmunologyTD.Pathogens;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// The gut wall -- the epithelial boundary at the lumen/tissue seam, and
    /// the second stage of GAME_DESIGN.md section 1b's invasion loop.
    ///
    /// **Sprint 22 replaced GutInterface with this** (docs/INVASION_REWORK_SPEC.md,
    /// Director 2026-10-14). The old model gave each adhered pathogen a LANE
    /// INDEX, piled them in a queue drawn straight out into the channel, and
    /// rolled a hidden per-lane breach chance. The Director's objection was
    /// the queue: pathogens should "float around an area near a breach spot",
    /// not line up. On this board a lane is a TENTH of the wall
    /// (BoardConfig is 25x10, not the 100x40 the design aspires to), which is
    /// far too coarse a unit for "a stretch of wall."
    ///
    /// ## The two objects
    ///
    /// 1. **The mill.** Every adhered pathogen owns a CONTINUOUS position
    ///    (s, d) in the mucus band -- s along the wall from the flow's entry
    ///    end, d out from the seam. One flat list, no per-lane structure.
    /// 2. **The plaque.** Where the crowd field peaks, a plaque seeds: a
    ///    pooled record with a centre, an exclusive membership, and a visible
    ///    CHARGE. At full charge it ruptures, releasing every member at once
    ///    and killing the wall it tore through.
    ///
    /// ## The one way to get this wrong (carried over verbatim in spirit)
    ///
    /// The old file warned, in capitals, against refactoring the per-position
    /// roll into a per-pathogen one: independent rolls produce a steady
    /// trickle across the whole wall and destroy the "pressure builds at one
    /// spot, then bursts" shape that is the entire point of the mechanic.
    /// **That warning still stands and this model enforces it harder.**
    ///
    /// Charge accrues at `(members - IngressCrowdFloor) * rate`, so a plaque
    /// with two or fewer members charges at or below ZERO and never ruptures.
    /// Ingress is not something a pathogen does; it is something a CROWD
    /// does. Sprint22Verification asserts this directly (the anti-trickle
    /// regression) -- do not "simplify" the floor away.
    ///
    /// ## Why a charge instead of a dice roll
    ///
    /// Pressure is now a quantity that fills and then triggers, which is the
    /// Director's requirement made literal. Three gains: what is drawn IS the
    /// state (the old bar showed a proxy for a memoryless roll that might go
    /// in one second or thirty); relieving a spot visibly deflates it, so the
    /// player can see that an intervention worked; and charge(t) is a
    /// deterministic function of membership, so a harness can prove "pressure
    /// rises with crowding" without a statistical test over hundreds of rolls.
    ///
    /// ## Explicit time
    ///
    /// Tick takes deltaTime and Rupture takes currentTime, per this project's
    /// standing convention, so Assets/Editor/*Verification.cs drives the real
    /// production path with a simulated clock. The milling SWIRL is NOT in
    /// here -- it is a closed-form visual tween on PathogenAgent.Update, and
    /// the crowd field reads only the simulated base position. If the swirl
    /// fed the field, a pathogen orbiting a bucket boundary would shuffle
    /// crowd every frame and marginal plaques would blink in and out.
    /// </summary>
    public class GutWall
    {
        /// <summary>One plaque. A struct in a fixed pooled array -- nothing
        /// here allocates per event, per CLAUDE.md's standing rule.</summary>
        public struct Plaque
        {
            public bool Live;
            public float CentreS;
            public float Charge;
            public float Threshold;
            public int MemberCount;
        }

        private readonly BoardConfig board;
        private readonly TissueGrid tissueGrid;
        private readonly InvasionTally tally;

        private readonly List<PathogenAgent> adhered = new List<PathogenAgent>();
        private readonly float[] crowd;
        private readonly Plaque[] plaques;

        /// <summary>Scratch buffer for a rupture's membership, reused so a
        /// burst allocates nothing. Read it inside the Ruptured handler or
        /// not at all.</summary>
        private readonly List<PathogenAgent> ruptureBuffer = new List<PathogenAgent>();

        /// <summary>Fired once per rupture, with (centre s, pathogens
        /// released, footprint in cells). Replaces GutInterface.Breached.
        ///
        /// An event rather than a poll for the same reason as before: script
        /// execution order between PathogenSpawner (which ticks this) and a
        /// renderer's Update() is undefined, so polling would silently drop
        /// about half the bursts -- and a burst that isn't drawn is the one
        /// thing the mechanic cannot afford to lose.</summary>
        public event System.Action<float, int, float> Ruptured;

        public GutWall(BoardConfig board, TissueGrid tissueGrid, InvasionTally tally)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.tally = tally;
            crowd = new float[Mathf.Max(1, board.CrossLength * Mathf.Max(1, InvasionTuning.CrowdBucketsPerCell))];
            plaques = new Plaque[Mathf.Max(1, InvasionTuning.MaxPlaques)];
        }

        /// <summary>Length of the wall in coarse cells. s lives in
        /// [0, WallLengthCells).</summary>
        public float WallLengthCells => board.CrossLength;

        /// <summary>Total pathogens colonising the wall.</summary>
        public int TotalAdhered => adhered.Count;

        public IReadOnlyList<PathogenAgent> Adhered => adhered;

        public int PlaqueCapacity => plaques.Length;

        public Plaque PlaqueAt(int index) =>
            index >= 0 && index < plaques.Length ? plaques[index] : default;

        /// <summary>The highest charge on any live plaque, 0..~1.15 -- what a
        /// HUD normalises "how bad is the wall right now" against. Replaces
        /// PeakAdhered, which counted a pile that no longer exists.</summary>
        public float PeakCharge
        {
            get
            {
                float peak = 0f;
                for (int i = 0; i < plaques.Length; i++)
                    if (plaques[i].Live && plaques[i].Charge > peak) peak = plaques[i].Charge;
                return peak;
            }
        }

        /// <summary>Members of the most-loaded live plaque. The readout the
        /// debug HUD used PeakAdhered for.</summary>
        public int PeakPlaqueMembers
        {
            get
            {
                int peak = 0;
                for (int i = 0; i < plaques.Length; i++)
                    if (plaques[i].Live && plaques[i].MemberCount > peak) peak = plaques[i].MemberCount;
                return peak;
            }
        }

        // -- Crowd field ------------------------------------------------

        public int CrowdBucketCount => crowd.Length;

        public float BucketsPerCell => Mathf.Max(1, InvasionTuning.CrowdBucketsPerCell);

        /// <summary>Crowd at a point on the wall: "how many gripping
        /// pathogens are within about a cell and a half of here, weighted by
        /// how close." A tight clump of five reads ~4; five spread over three
        /// cells reads ~2.5. Crowding is about CONCENTRATION, which is the
        /// whole point of proximity-based ingress.</summary>
        public float CrowdAt(float s)
        {
            int bucket = Mathf.Clamp(Mathf.RoundToInt(s * BucketsPerCell), 0, crowd.Length - 1);
            return crowd[bucket];
        }

        public float CrowdAtBucket(int bucket) =>
            bucket >= 0 && bucket < crowd.Length ? crowd[bucket] : 0f;

        public float BucketToS(int bucket) => bucket / BucketsPerCell;

        // -- Membership -------------------------------------------------

        /// <summary>Puts a pathogen into the mill at a point on the wall.
        /// Called by PathogenAgent when its proximity-gated adhesion roll
        /// succeeds. It arrives in the OUTER mucus and settles in over about
        /// two seconds -- it does not snap to the seam, because that snap was
        /// the "orderly" artefact this rework exists to remove.</summary>
        public bool Adhere(PathogenAgent pathogen, float s)
        {
            if (pathogen == null) return false;
            adhered.Add(pathogen);
            pathogen.SetWallPosition(Mathf.Clamp(s, 0f, WallLengthCells - 0.001f),
                                     InvasionTuning.MillArrivalDepthCells);
            if (tally != null) tally.Adhesions++;
            return true;
        }

        /// <summary>Removes a pathogen from the mill -- for one cleared,
        /// swept away, or despawned while still on the wall.</summary>
        public bool Remove(PathogenAgent pathogen)
        {
            if (pathogen == null) return false;
            return adhered.Remove(pathogen);
        }

        // -- The tick ---------------------------------------------------

        /// <summary>
        /// Advances the mill, rebuilds the crowd field, seeds and charges
        /// plaques, and ruptures any that reach their threshold. Pathogens
        /// released by a rupture have ALREADY been moved into tissue by the
        /// time this returns.
        /// </summary>
        public void Tick(float deltaTime, float currentTime, float roundElapsed)
        {
            MoveMill(deltaTime, roundElapsed);
            RebuildCrowdField();
            // Seed BEFORE assigning membership. The other order looks
            // equivalent and is not: a plaque seeded this tick would still
            // have MemberCount 0 when ChargeAndRupture ran its retire check,
            // so every plaque would be destroyed on the tick it was born and
            // the wall could never breach at all.
            SeedPlaques();
            AssignMembership();
            ChargeAndRupture(deltaTime, currentTime);
        }

        /// <summary>Creep, cohesion and settling -- the simulated half of the
        /// mill. The drawn swirl is a tween on the agent and never lands
        /// here.</summary>
        private void MoveMill(float deltaTime, float roundElapsed)
        {
            // One shared peristalsis factor for the whole population, read
            // once: this is what makes the mill lurch downstream together,
            // in time with the contraction already on screen.
            float creep = InvasionTuning.MucusCreepCellsPerSecond
                        * Peristalsis.SpeedFactor(roundElapsed) * deltaTime;
            float cohesionStep = InvasionTuning.CohesionSpeedCellsPerSecond * deltaTime;
            float settleStep = InvasionTuning.SettleSpeedCellsPerSecond * deltaTime;

            for (int i = adhered.Count - 1; i >= 0; i--)
            {
                var p = adhered[i];
                if (p == null) { adhered.RemoveAt(i); continue; }

                float s = p.WallS + creep;
                float d = Mathf.MoveTowards(p.WallD, InvasionTuning.SettleDepthCells, settleStep);

                float target = CohesionTarget(s);
                if (!float.IsNaN(target)) s = Mathf.MoveTowards(s, target, cohesionStep);

                // Off the downstream end: the mucus has carried it out of the
                // gut. It rejoins the flow and is excreted -- no penalty,
                // exactly as section 1b step 1 says of a pathogen that never
                // adheres. This is what keeps the wall population bounded
                // without making it immortal.
                if (s >= WallLengthCells)
                {
                    adhered.RemoveAt(i);
                    p.SweptOffWall();
                    continue;
                }

                p.SetWallPosition(Mathf.Max(0f, s), d);
            }
        }

        /// <summary>What a milling pathogen drifts toward: the nearest live
        /// plaque centre in reach, else the best crowd bucket in reach, else
        /// nothing. Returns NaN for "nothing" so a caller cannot mistake it
        /// for position zero.</summary>
        private float CohesionTarget(float s)
        {
            float reach = InvasionTuning.CohesionReachCells;
            float best = float.NaN;
            float bestDistance = reach;

            for (int i = 0; i < plaques.Length; i++)
            {
                if (!plaques[i].Live) continue;
                float distance = Mathf.Abs(plaques[i].CentreS - s);
                if (distance < bestDistance) { bestDistance = distance; best = plaques[i].CentreS; }
            }
            if (!float.IsNaN(best)) return best;

            float bestCrowd = 0.01f;
            int lo = Mathf.Max(0, Mathf.FloorToInt((s - reach) * BucketsPerCell));
            int hi = Mathf.Min(crowd.Length - 1, Mathf.CeilToInt((s + reach) * BucketsPerCell));
            for (int b = lo; b <= hi; b++)
            {
                if (crowd[b] <= bestCrowd) continue;
                bestCrowd = crowd[b];
                best = BucketToS(b);
            }
            return best;
        }

        /// <summary>Rebuilds the 1-D crowd field from the GRIPPING pathogens'
        /// simulated positions. Triangular kernel, radius CrowdRadiusCells.
        ///
        /// Triangular rather than box for two reasons: a box kernel's maxima
        /// jitter between equal-valued buckets, which would make plaques
        /// flicker in and out; and a smooth field gives the renderer a free
        /// "how bare is the mucus here" value.</summary>
        private void RebuildCrowdField()
        {
            System.Array.Clear(crowd, 0, crowd.Length);
            float radius = Mathf.Max(0.0001f, InvasionTuning.CrowdRadiusCells);
            float bpc = BucketsPerCell;
            int reach = Mathf.CeilToInt(radius * bpc);

            for (int i = 0; i < adhered.Count; i++)
            {
                var p = adhered[i];
                if (p == null || !p.IsGrippingWall) continue;

                float s = p.WallS;
                // A cross-linked pathogen contributes NOTHING to crowding.
                // That is the whole resolution of the IgA tension: trapping
                // concentrates pathogens, but an agglutinated raft cannot
                // reach the epithelium, so concentration stops meaning
                // danger. See INVASION_REWORK_SPEC.md section 4.
                if (p.IsCrossLinked) continue;

                int centre = Mathf.RoundToInt(s * bpc);
                int lo = Mathf.Max(0, centre - reach);
                int hi = Mathf.Min(crowd.Length - 1, centre + reach);
                for (int b = lo; b <= hi; b++)
                {
                    float weight = 1f - Mathf.Abs(s - BucketToS(b)) / radius;
                    if (weight > 0f) crowd[b] += weight;
                }
            }
        }

        /// <summary>Assigns each gripping, un-cross-linked pathogen to the
        /// NEAREST plaque centre within the footprint, exclusively. Exclusive
        /// so two nearby plaques split a crowd instead of double-counting it,
        /// and so MemberCount is exactly the set a rupture will release --
        /// the number driving the charge is the same number the player can
        /// count on screen.</summary>
        private void AssignMembership()
        {
            for (int i = 0; i < plaques.Length; i++) plaques[i].MemberCount = 0;

            float footprint = InvasionTuning.BurstFootprintCells;
            for (int i = 0; i < adhered.Count; i++)
            {
                var p = adhered[i];
                if (p == null) continue;
                int owner = NearestPlaque(p, footprint);
                p.SetPlaqueIndex(owner);
                if (owner >= 0) plaques[owner].MemberCount++;
            }
        }

        private int NearestPlaque(PathogenAgent p, float footprint)
        {
            if (!p.IsGrippingWall || p.IsCrossLinked) return -1;
            int best = -1;
            float bestDistance = footprint;
            for (int i = 0; i < plaques.Length; i++)
            {
                if (!plaques[i].Live) continue;
                float distance = Mathf.Abs(plaques[i].CentreS - p.WallS);
                if (distance <= bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        /// <summary>Seeds at most one plaque per tick, at the highest crowd
        /// bucket that clears PlaqueSeedCrowd and is far enough from every
        /// live plaque. One per tick is plenty -- crowds do not appear
        /// instantly.</summary>
        private void SeedPlaques()
        {
            int free = -1;
            for (int i = 0; i < plaques.Length; i++)
                if (!plaques[i].Live) { free = i; break; }
            if (free < 0) return;

            int bestBucket = -1;
            float bestCrowd = InvasionTuning.PlaqueSeedCrowd;
            for (int b = 0; b < crowd.Length; b++)
            {
                if (crowd[b] < bestCrowd) continue;
                float s = BucketToS(b);
                if (TooCloseToLivePlaque(s)) continue;
                bestCrowd = crowd[b];
                bestBucket = b;
            }
            if (bestBucket < 0) return;

            float jitter = Mathf.Max(0f, InvasionTuning.ChargeThresholdJitter);
            plaques[free] = new Plaque
            {
                Live = true,
                CentreS = BucketToS(bestBucket),
                Charge = 0f,
                Threshold = 1f + Random.Range(-jitter, jitter),
                MemberCount = 0,
            };
        }

        private bool TooCloseToLivePlaque(float s)
        {
            float separation = InvasionTuning.PlaqueMinSeparationCells;
            for (int i = 0; i < plaques.Length; i++)
                if (plaques[i].Live && Mathf.Abs(plaques[i].CentreS - s) < separation) return true;
            return false;
        }

        /// <summary>Fills or drains each live plaque, tracks its centre, and
        /// ruptures it at threshold.</summary>
        private void ChargeAndRupture(float deltaTime, float currentTime)
        {
            float floor = InvasionTuning.IngressCrowdFloor;
            float lerp = Mathf.Clamp01(InvasionTuning.PlaqueCentreLerpPerSecond * deltaTime);

            for (int i = 0; i < plaques.Length; i++)
            {
                if (!plaques[i].Live) continue;

                if (plaques[i].MemberCount > floor)
                {
                    // The floor subtracts, so two members charge at zero and
                    // one charges NEGATIVE-clamped-to-nothing. The trickle is
                    // arithmetically impossible here, not merely unlikely.
                    float rate = (plaques[i].MemberCount - floor) * InvasionTuning.ChargeRatePerCrowdUnit;
                    plaques[i].Charge += rate * deltaTime;
                    plaques[i].CentreS = Mathf.Lerp(plaques[i].CentreS, MemberCentroid(i), lerp);
                }
                else
                {
                    plaques[i].Charge -= InvasionTuning.ChargeDecayPerSecond * deltaTime;
                }

                if (plaques[i].Charge >= plaques[i].Threshold)
                {
                    Rupture(i, currentTime);
                    continue;
                }

                if (plaques[i].MemberCount == 0 && plaques[i].Charge <= 0f)
                    plaques[i] = default;
                else
                    plaques[i].Charge = Mathf.Max(0f, plaques[i].Charge);
            }
        }

        private float MemberCentroid(int plaqueIndex)
        {
            float sum = 0f;
            int count = 0;
            for (int i = 0; i < adhered.Count; i++)
            {
                var p = adhered[i];
                if (p == null || p.PlaqueIndex != plaqueIndex) continue;
                sum += p.WallS;
                count++;
            }
            return count == 0 ? plaques[plaqueIndex].CentreS : sum / count;
        }

        /// <summary>
        /// Releases EVERY member of a plaque into tissue in this single call
        /// -- the burst. Also the deterministic entry point a harness (or the
        /// player's "pop it" ability) uses to trip a plaque without waiting
        /// for its charge.
        ///
        /// Members are released in order of their own s, each asking for the
        /// tissue cell behind its OWN position first, so a rupture that
        /// formed across three lanes lands across those three lanes. A member
        /// that finds no slot stays on the wall, as the old pile did.
        /// </summary>
        /// <returns>How many pathogens entered tissue.</returns>
        public int Rupture(int plaqueIndex, float currentTime)
        {
            if (plaqueIndex < 0 || plaqueIndex >= plaques.Length) return 0;
            if (!plaques[plaqueIndex].Live) return 0;

            float centre = plaques[plaqueIndex].CentreS;
            float footprint = InvasionTuning.BurstFootprintCells;

            ruptureBuffer.Clear();
            for (int i = 0; i < adhered.Count; i++)
                if (adhered[i] != null && adhered[i].PlaqueIndex == plaqueIndex)
                    ruptureBuffer.Add(adhered[i]);
            ruptureBuffer.Sort((a, b) => a.WallS.CompareTo(b.WallS));

            int released = 0;
            for (int i = 0; i < ruptureBuffer.Count; i++)
            {
                var p = ruptureBuffer[i];
                int lane = LaneOf(p.WallS);
                if (!TryFindReleaseSlot(lane, out var slot)) continue;
                if (!p.EnterTissueAt(slot, currentTime)) continue;
                adhered.Remove(p);
                released++;
            }

            plaques[plaqueIndex].Charge = 0f;
            plaques[plaqueIndex].MemberCount = 0;

            if (released > 0)
            {
                if (tally != null)
                {
                    // One breach per rupture, and ReleasedIntoTissue keeps its
                    // exact meaning, so RoundVerification and the HUD are
                    // unaffected by the rework.
                    tally.Breaches++;
                    tally.ReleasedIntoTissue += released;
                }
                Ruptured?.Invoke(centre, released, footprint);
            }
            return released;
        }

        /// <summary>The lane a point on the wall sits in. Measured from the
        /// flow's ENTRY end in flow units, so it stays correct on a mirrored
        /// or vertically-banded board -- the same discipline GutWallFeatures
        /// already uses for the Peyer's patch.</summary>
        public int LaneOf(float s)
        {
            int lane = board.LumenEntryCrossIndex + board.FlowCrossStep * Mathf.FloorToInt(s);
            return Mathf.Clamp(lane, 0, board.CrossLength - 1);
        }

        /// <summary>Inverse of <see cref="LaneOf"/>: where a lane's centre
        /// sits on the wall.</summary>
        public float SOfLane(int lane) =>
            (lane - board.LumenEntryCrossIndex) * board.FlowCrossStep + 0.5f;

        /// <summary>
        /// Finds a free tissue slot for a pathogen coming through at
        /// <paramref name="lane"/>. Searches outward from the lumen-edge
        /// tissue cell, cheapest first, so a burst fans ALONG the wall before
        /// it punches deeper -- a breach should read as a widening hole at
        /// one spot, not as a spear.
        ///
        /// Expressed entirely in the axis frame -- "one cell deeper" is
        /// `axisIndex - 1`, never `Column - 1`.
        /// </summary>
        public bool TryFindReleaseSlot(int lane, out CoarseCoord slot)
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
                        int cross = lane + spread * sign;
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

        /// <summary>Where a wall point is drawn. The outward direction is
        /// derived from the two cells either side of the seam, so it follows
        /// the map's orientation rather than assuming the lumen is on the
        /// right -- no world direction is hardcoded anywhere in this class.
        /// </summary>
        public Vector3 WallToWorld(float s, float d)
        {
            int lane = LaneOf(s);
            var seam = board.InterfaceWorldCenter(lane);
            var tissueSide = board.CoarseToWorldCenter(
                board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, lane));
            var outward = seam - tissueSide;
            if (outward.sqrMagnitude > 0.0001f) outward.Normalize();

            // Slide along the wall by the fractional part, so a pathogen can
            // sit anywhere on the seam rather than only at a lane centre --
            // which is the whole point of a continuous s.
            var along = new Vector3(-outward.y, outward.x, 0f);
            float frac = s - Mathf.Floor(s);
            float cell = board.CoarseCellWorldSize;
            return seam
                 + along * ((frac - 0.5f) * cell * board.FlowCrossStep)
                 + outward * (d * cell);
        }
    }
}
