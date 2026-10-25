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
        /// <summary>
        /// The state of one stretch of epithelium.
        ///
        /// Deliberately NOT the same vocabulary as TissueGrid's host states
        /// (Healthy / Infected / Dead), although the ideas overlap. Section 1c
        /// is a Director-set section describing the LATTICE's host layer; the
        /// barrier is 20 segments on a seam with its own geometry, its own
        /// regrowth rule and its own occupancy. The names differ on purpose so
        /// that a later session cannot conflate the two structures -- the same
        /// reasoning the old GutInterface gave for not living on TissueGrid.
        /// </summary>
        public enum BarrierState { Intact, Eroded, Dead }

        public struct BarrierSegment
        {
            public BarrierState State;
            public float Health;
            public float RegrowAt;
        }

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
        private readonly BarrierSegment[] barrier;

        /// <summary>Sprint 24: per-segment cholera-toxin load, 0..1. Parallel
        /// to <see cref="barrier"/> -- a separate array rather than a field
        /// on BarrierSegment because subversion is orthogonal to the
        /// Intact/Eroded/Dead state machine: a perfectly healthy segment can
        /// be fully subverted, and that is the whole mechanic.</summary>
        private readonly float[] toxinLoad;
        private readonly float[] cryptReadyAt;

        /// <summary>Scratch buffer for a rupture's membership, reused so a
        /// burst allocates nothing. Read it inside the Ruptured handler or
        /// not at all.</summary>
        private readonly List<PathogenAgent> ruptureBuffer = new List<PathogenAgent>();

        /// <summary>Members per IgA clump, indexed by clump id. A clump is
        /// never destroyed, only emptied -- ids are stable for the renderer.
        /// </summary>
        private readonly List<int> clumpSize = new List<int>();

        /// <summary>Reads knowledge per species, so the wall can tell whether
        /// IgA against a given class is online. Null until the adaptive side
        /// is wired, in which case no coating ever happens -- which is
        /// exactly right for a harness that has not bought any knowledge.
        /// </summary>
        private ImmunologyTD.Adaptive.KnowledgeLedger knowledge;

        /// <summary>How many organisms IgA has excluded from the wall --
        /// agglutinated, swept downstream and lost. Counted separately from
        /// Excreted because "it never stuck" and "we pushed it off" are
        /// different things for the strategy harness to read.</summary>
        public int IgaExcluded { get; private set; }

        /// <summary>How many organisms have been born on the wall. With
        /// enchained growth this is NOT purely bad news for the player --
        /// under IgA a division is what builds the attacker's own cage.
        /// </summary>
        public int LumenDivisions { get; private set; }

        /// <summary>Fired once per rupture, with (centre s, pathogens
        /// released, footprint in cells). Replaces GutInterface.Breached.
        ///
        /// An event rather than a poll for the same reason as before: script
        /// execution order between PathogenSpawner (which ticks this) and a
        /// renderer's Update() is undefined, so polling would silently drop
        /// about half the bursts -- and a burst that isn't drawn is the one
        /// thing the mechanic cannot afford to lose.</summary>
        public event System.Action<float, int, float> Ruptured;

        /// <summary>Gives the wall the knowledge ledger, so IgA can come
        /// online at the 70% rung. Optional: without it, nothing is ever
        /// coated.</summary>
        public void BindKnowledge(ImmunologyTD.Adaptive.KnowledgeLedger ledger) => knowledge = ledger;

        /// <summary>Sprint 23: the live wall, so a gamma-delta IEL standing
        /// in tissue can reach the stretch of barrier in front of it.
        /// Process-global and null-safe, the same pattern as
        /// <see cref="GutWallFeatures.Current"/> -- a harness that never
        /// builds a wall simply has gamma-delta cells with nothing to
        /// patrol, rather than a crash.</summary>
        public static GutWall Current;

        /// <summary>Spawns a daughter. Set by PathogenSpawner, which owns the
        /// pool -- the wall asks, it does not allocate.</summary>
        public System.Func<PathogenAgent, float, PathogenAgent> SpawnDaughter;

        public GutWall(BoardConfig board, TissueGrid tissueGrid, InvasionTally tally)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.tally = tally;
            crowd = new float[Mathf.Max(1, board.CrossLength * Mathf.Max(1, InvasionTuning.CrowdBucketsPerCell))];
            plaques = new Plaque[Mathf.Max(1, InvasionTuning.MaxPlaques)];

            barrier = new BarrierSegment[Mathf.Max(1, board.CrossLength * Mathf.Max(1, BarrierTuning.SegmentsPerLane))];
            toxinLoad = new float[barrier.Length];
            for (int i = 0; i < barrier.Length; i++)
                barrier[i] = new BarrierSegment
                {
                    State = BarrierState.Intact,
                    Health = BarrierTuning.SegmentMaxHealth,
                    RegrowAt = float.PositiveInfinity,
                };
            cryptReadyAt = new float[board.CrossLength];
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
            if (pathogen.IsCrossLinked) LeaveClump(pathogen);
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

            // IgA and replication run AFTER the plaque pass, so that within a
            // tick the player's reading of the wall and the simulation agree:
            // what charged is what was gripping at the top of the tick.
            TickIga(deltaTime, currentTime);
            TickReplication(currentTime);
            TickBarrier(deltaTime, currentTime);
            TickSentinels(currentTime);
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

                // A raft outruns the rest of the mill: it has let go of the
                // wall and the mucus is taking it.
                float s = p.WallS + creep * (p.IsCrossLinked ? IgaTuning.ClearCreepMultiplier : 1f);
                // A clumped organism is held out by SweepClumps, not settling
                // in; everything else comes in to land.
                float d = p.IsCrossLinked
                    ? p.WallD
                    : Mathf.MoveTowards(p.WallD, InvasionTuning.SettleDepthCells, settleStep);

                // A clumped organism has nothing left to adhere WITH -- its
                // adhesins are spent on its neighbours -- so it does not seek
                // a crowd. A merely coated one is slower about it.
                if (!p.IsCrossLinked)
                {
                    float target = CohesionTarget(s);
                    if (!float.IsNaN(target))
                        s = Mathf.MoveTowards(s, target, cohesionStep * p.MotilityScale);
                }

                // Off the downstream end: the mucus has carried it out of the
                // gut. It rejoins the flow and is excreted -- no penalty,
                // exactly as section 1b step 1 says of a pathogen that never
                // adheres. This is what keeps the wall population bounded
                // without making it immortal.
                if (s >= WallLengthCells)
                {
                    if (p.IsCrossLinked) { IgaExcluded++; LeaveClump(p); }
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

            // A virus over a DEAD stretch has nothing left to infect, so it
            // contributes nothing to breaking in there. This is the other
            // half of the Director's asymmetry, and it is what makes a
            // battered wall a bacterial sieve and a viral dead end.
            if (BarrierTuning.VirusesNeedLivingWall
                && p.Class == PathogenClass.IntracellularVirus
                && StateAt(p.WallS) == BarrierState.Dead) return -1;
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
                    // A battered stretch charges faster -- the board
                    // remembers where it was breached -- but it still takes a
                    // CROWD. The Director chose "a weak spot, not a hole":
                    // one ingress mechanic, not two.
                    float rate = (plaques[i].MemberCount - floor)
                               * InvasionTuning.ChargeRatePerCrowdUnit
                               * BarrierChargeMultiplierAt(plaques[i].CentreS)
                               // Sprint 24: loosened tight junctions under a
                               // cholera toxin field let a plaque charge faster.
                               * (1f + BarrierTuning.ToxinChargeBonus * ToxinLoadAt(plaques[i].CentreS));
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

            // The rupture kills the epithelium it came through. That stretch
            // now charges faster until it regrows -- and is dead ground to
            // viruses while it does.
            KillBarrier(centre, footprint, currentTime);

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

        // -- Secretory IgA (INVASION_REWORK_SPEC.md section 4) ---------

        /// <summary>
        /// Coats eligible organisms, bridges coated neighbours of the same
        /// class into clumps, breaks old links, and sweeps rafts away.
        ///
        /// **The resolution of the IgA trap lives here**, and it is worth
        /// stating because a plausible implementation of this rung is
        /// actively harmful: trapping organisms AT the wall concentrates
        /// them, and under proximity ingress concentration is what breaches.
        /// So a naive IgA would charge the player 70% of a species' knowledge
        /// to make that species MORE dangerous.
        ///
        /// It is closed four times over, on purpose:
        ///  1. a clumped organism contributes 0 to the crowd field;
        ///  2. it is held beyond the grip depth, and FURTHER OUT the bigger
        ///     its clump -- so density increases the exclusion;
        ///  3. it is not a plaque member, so a rupture beside it leaves it;
        ///  4. cross-linking ACCELERATES with density, because bridging
        ///     needs a partner.
        ///
        /// (4) is the one that makes the inversion real rather than merely
        /// safe: the denser a coated patch, the faster it rafts up and
        /// leaves.
        /// </summary>
        private void TickIga(float deltaTime, float currentTime)
        {
            if (knowledge == null) return;

            float coatP = Probability(IgaTuning.CoatChancePerSecond, deltaTime);
            float breakP = Probability(IgaTuning.LinkBreakChancePerSecond, deltaTime);

            for (int i = 0; i < adhered.Count; i++)
            {
                var p = adhered[i];
                if (p == null) continue;

                if (!p.IsCoated
                    && p.WallD <= IgaTuning.ReachDepthCells
                    && knowledge.Get(p.Species) >= IgaTuning.KnowledgePercent
                    && Random.value < coatP)
                {
                    p.SetCoated(true);
                }

                // Links break over time -- the other half of Bansept's race,
                // and why a stalled chain slowly comes apart while a dividing
                // one grows.
                //
                // A break SPLITS a chain; it does not dissolve one. Only the
                // last pair can actually come apart, because an organism in
                // the middle of a chain is still held by its other neighbour.
                // Letting any member walk free instead cost an afternoon: at
                // 0.02/s every member of a 9-chain escapes within a couple of
                // minutes, and a free COATED organism still grips and still
                // charges a plaque, so the wall leaked exactly as if IgA were
                // not there.
                if (p.IsCrossLinked && Random.value < breakP) DislocateFrom(p);
            }

            CrossLinkNeighbours(deltaTime);
            SweepClumps(deltaTime);
        }

        /// <summary>Bridges coated organisms of the same class. The rate is
        /// per NEIGHBOUR, so it rises with local density -- which is the
        /// whole promise that density flips sign above the rung.</summary>
        private void CrossLinkNeighbours(float deltaTime)
        {
            float radius = IgaTuning.CrossLinkRadiusCells;

            for (int i = 0; i < adhered.Count; i++)
            {
                var a = adhered[i];
                if (a == null || !a.IsCoated) continue;

                int neighbours = 0;
                PathogenAgent partner = null;
                for (int j = 0; j < adhered.Count; j++)
                {
                    if (i == j) continue;
                    var b = adhered[j];
                    // Same class only: knowledge is per-species, and IgA ties
                    // an organism to another MATCHING one. A mixed round is
                    // therefore harder to clear by IgA alone, which is
                    // correct and is a nice piece of teaching.
                    if (b == null || !b.IsCoated || b.Class != a.Class) continue;
                    if (Mathf.Abs(b.WallS - a.WallS) > radius) continue;
                    neighbours++;
                    if (partner == null || b.ClumpId >= 0) partner = b;
                }
                if (neighbours == 0 || partner == null) continue;

                float p = Probability(
                    IgaTuning.CrossLinkChancePerSecondPerNeighbour * neighbours, deltaTime);
                if (Random.value >= p) continue;

                Bridge(a, partner);
            }
        }

        /// <summary>Joins two organisms into one clump, merging the smaller
        /// into the larger when both already have one.</summary>
        public void Bridge(PathogenAgent a, PathogenAgent b)
        {
            if (a == null || b == null || a == b) return;

            if (a.ClumpId < 0 && b.ClumpId < 0)
            {
                int id = NewClump();
                JoinClump(a, id);
                JoinClump(b, id);
                return;
            }
            if (a.ClumpId < 0) { JoinClump(a, b.ClumpId); return; }
            if (b.ClumpId < 0) { JoinClump(b, a.ClumpId); return; }
            if (a.ClumpId == b.ClumpId) return;

            int from = clumpSize[a.ClumpId] < clumpSize[b.ClumpId] ? a.ClumpId : b.ClumpId;
            int into = from == a.ClumpId ? b.ClumpId : a.ClumpId;

            for (int i = 0; i < adhered.Count; i++)
                if (adhered[i] != null && adhered[i].ClumpId == from) JoinClump(adhered[i], into);
        }

        private int NewClump()
        {
            for (int i = 0; i < clumpSize.Count; i++)
                if (clumpSize[i] == 0) return i;
            clumpSize.Add(0);
            return clumpSize.Count - 1;
        }

        /// <summary>Puts an organism in a chain. Always succeeds:
        /// membership is unbounded and the BULK saturates instead -- see
        /// ClumpRadiusCells for why the spec's membership cap was dropped.
        /// </summary>
        private bool JoinClump(PathogenAgent p, int id)
        {
            if (p.ClumpId == id) return true;
            if (p.ClumpId >= 0) clumpSize[p.ClumpId]--;
            p.SetClump(id);
            clumpSize[id]++;
            return true;
        }


        /// <summary>One link gives way. A pair comes apart; a longer chain
        /// splits, and the organism stays enchained to the piece it leaves
        /// with.</summary>
        private void DislocateFrom(PathogenAgent p)
        {
            if (p.ClumpId < 0) return;
            if (ClumpMembers(p.ClumpId) <= 2)
            {
                // The last pair: both are genuinely free again, and will
                // re-bridge quickly if they are still touching.
                int id = p.ClumpId;
                for (int i = 0; i < adhered.Count; i++)
                    if (adhered[i] != null && adhered[i].ClumpId == id) LeaveClump(adhered[i]);
                return;
            }
            JoinClump(p, NewClump());
        }

        private void LeaveClump(PathogenAgent p)
        {
            if (p.ClumpId < 0) return;
            int id = p.ClumpId;
            clumpSize[id]--;
            p.SetClump(-1);

            // A chain of one is linked to nothing. Free the straggler rather
            // than leaving it "cross-linked" and therefore excluded by a bond
            // that does not exist.
            if (clumpSize[id] != 1) return;
            for (int i = 0; i < adhered.Count; i++)
                if (adhered[i] != null && adhered[i].ClumpId == id) { clumpSize[id]--; adhered[i].SetClump(-1); break; }
        }

        public int ClumpCount => clumpSize.Count;

        public int ClumpMembers(int id) =>
            id >= 0 && id < clumpSize.Count ? clumpSize[id] : 0;

        /// <summary>
        /// Bulk of a chain, in cells. sqrt so a 4-chain is twice the base
        /// radius and a 9-chain three times -- visibly chunky.
        ///
        /// **Deviation from INVASION_REWORK_SPEC.md section 4.3**, found in
        /// testing. The spec capped chain MEMBERSHIP at 9 and said "past this
        /// a new clump forms instead". Every way of implementing that cap
        /// leaked: refusing the daughter left it free, coated and gripping
        /// (an armed wall then breached MORE than a naive one); giving it a
        /// chain of its own produced one-member "chains" linked to nothing,
        /// which dislocated on the next roll; and moving the parent out to
        /// pair with it shredded established chains as members kept leaving.
        ///
        /// Membership is therefore unbounded and the BULK saturates instead.
        /// That is what the cap was for -- keeping the drawn raft under a
        /// cell and sqrt() bounded -- and it is physically truer: a long
        /// chain really does exist, its hold-depth effect simply stops
        /// growing. The invariant that matters survives intact: an organism
        /// is either free, or attached to at least one other organism.
        /// </summary>
        public float ClumpRadiusCells(int id) =>
            IgaTuning.ClumpBaseRadiusCells
            * Mathf.Sqrt(Mathf.Clamp(ClumpMembers(id), 1, IgaTuning.ClumpMaxMembers));

        /// <summary>Pushes rafts off the wall and carries them downstream.
        /// Nothing is killed: everything leaves. That is the compartment's
        /// rule -- the gut's answer is flush, not eat.</summary>
        private void SweepClumps(float deltaTime)
        {
            float push = IgaTuning.PushOutSpeedCellsPerSecond * deltaTime;

            for (int i = adhered.Count - 1; i >= 0; i--)
            {
                var p = adhered[i];
                if (p == null || !p.IsCrossLinked) continue;

                // Held at the larger of the floor and its own bulk, so a
                // growing raft pushes ITSELF further out of reach.
                float hold = Mathf.Max(IgaTuning.HoldDepthCells, ClumpRadiusCells(p.ClumpId));
                p.SetWallPosition(p.WallS, Mathf.MoveTowards(p.WallD, hold, push));
            }
        }

        // -- In-lumen replication (INVASION_REWORK_SPEC.md section 4a) --

        /// <summary>
        /// Divides bacteria on the wall, damped by local density.
        ///
        /// **A daughter of a cross-linked parent is born INTO its parent's
        /// clump.** That is enchained growth (Moor 2017), and it is why there
        /// is no IgA penalty on the division rate anywhere in this method:
        /// IgA does not slow bacteria down, it exploits their division. Under
        /// IgA, replicating faster is what traps you.
        ///
        /// The consequence is the best property in this design: **fast
        /// replicators trap themselves hardest, and nothing here ever decides
        /// which pathogen deserves it.** The physics does the targeting.
        /// </summary>
        private void TickReplication(float currentTime)
        {
            if (SpawnDaughter == null) return;

            int count = adhered.Count;
            for (int i = 0; i < count; i++)
            {
                var p = adhered[i];
                if (p == null || !p.CanReplicateInLumen) continue;

                if (float.IsPositiveInfinity(p.DivideAt))
                {
                    p.SetDivideAt(currentTime + NextDivideInterval(p));
                    continue;
                }
                if (currentTime < p.DivideAt) continue;

                var daughter = SpawnDaughter(p, currentTime);
                p.SetDivideAt(currentTime + NextDivideInterval(p));
                if (daughter == null) continue;

                LumenDivisions++;
                daughter.SetCoated(p.IsCoated);

                // **Enchained growth is what CREATES a chain**, not just what
                // extends one. A coated parent keeps its daughter whether or
                // not it was already in a clump -- that is the whole finding
                // (Moor 2017): clumps form from division, because classical
                // agglutination needs densities real infections never reach.
                //
                // Requiring the parent to be pre-clumped, as this first did,
                // leaves every first-generation daughter free and coated, and
                // free coated organisms still grip and still charge plaques.
                if (p.IsCoated && p.ClumpId < 0)
                {
                    int chain = NewClump();
                    JoinClump(p, chain);
                    JoinClump(daughter, chain);
                }
                else if (p.IsCrossLinked)
                {
                    // Born enchained: the daughter never separates from its
                    // parent, so the clump grows by exactly the mechanism
                    // that was supposed to be the attacker's win condition.
                    // If the parent's chain is already at its bulk cap, the
                    // daughter starts a NEW chain -- it must never be left
                    // free.
                    JoinClump(daughter, p.ClumpId);
                }
                else
                {
                    daughter.SetDivideAt(currentTime + NextDivideInterval(daughter));
                }
            }
        }

        /// <summary>Division interval, damped by how crowded it is here.
        ///
        /// The damper is nutrient competition between metabolically
        /// overlapping neighbours -- your own kind compete with you hardest,
        /// because they eat exactly what you eat. It is NOT quorum sensing,
        /// which governs virulence and biofilm rather than growth rate.
        /// </summary>
        private float NextDivideInterval(PathogenAgent p)
        {
            float interval = IgaTuning.LumenDivideIntervalSeconds;
            if (p.Class == PathogenClass.IntracellularBacterium)
                interval /= Mathf.Max(0.01f, IgaTuning.IntracellularLumenRateMultiplier);

            float localCrowd = CrowdAt(p.WallS);
            interval *= 1f + IgaTuning.CrowdDamperStrength * localCrowd;

            // Sprint 24 -- **inflammation as fuel.** The attaching-effacing
            // E. coli respire what the host's own response produces: the
            // oxidative burst generates nitrate and tetrathionate, which
            // only a facultative anaerobe can use, so an inflamed stretch of
            // wall is a better place to live than a quiet one.
            //
            // This inverts the assumption the whole game has run on so far
            // -- that more immune response is always better -- and it is the
            // one mechanic found in the Sprint 24 research with NO answer
            // anywhere in the current roster. A neutrophil-heavy clear makes
            // the survivors faster.
            //
            // Species that do not feed on inflammation have a factor of 0
            // and are untouched, so no call site needs a species check.
            float fuel = p.Profile.InflammationFuelFactor;
            if (fuel > 0f)
            {
                interval /= 1f + fuel * LocalInflammation(p.WallS);
            }

            return Mathf.Max(0.5f, interval);
        }

        /// <summary>The project's standard per-second-to-per-tick conversion,
        /// the same `1 - (1-p)^dt` shape GutWallFeatures.TrySample uses.
        /// </summary>
        /// <summary>
        /// Sprint 24 -- **cholera toxin.** A crowd of secretors subverts the
        /// stretch of wall in front of it instead of eating it.
        ///
        /// Three composable effects, all from the real mechanism and none of
        /// them a kill:
        ///
        ///  1. The segments under the field become **Subverted**: alive,
        ///     still occupying their slots, still blocking invasion -- but
        ///     secreting, so they are no longer doing the player's work.
        ///  2. Tight junctions loosen (claudin-2 up, tricellulin down), so a
        ///     plaque over subverted wall **charges faster**.
        ///  3. The damage releases lactate the organism feeds on, so the
        ///     field quietly **raises local replication** -- a visible,
        ///     interruptible feedback loop rather than a flat debuff.
        ///
        /// The enterocyte lives through all of it. That is the point: this
        /// is the one enemy on the board that wins without killing anything,
        /// and the player's instinct to look for dying cells will miss it.
        /// </summary>
        public void ApplyToxinField(float centre, float strength, float radiusCells, float deltaTime)
        {
            if (strength <= 0f || deltaTime <= 0f) return;
            int lo = SegmentIndexAt(centre - radiusCells);
            int hi = SegmentIndexAt(centre + radiusCells);
            for (int i = lo; i <= hi; i++)
            {
                // A dead segment has nothing left to subvert.
                if (barrier[i].State == BarrierState.Dead) continue;
                toxinLoad[i] = Mathf.Min(1f, toxinLoad[i] + strength * deltaTime);
            }
        }

        /// <summary>How subverted the wall is at <paramref name="s"/>, 0..1.
        /// The renderer tints by this and the charge rate scales with it.</summary>
        public float ToxinLoadAt(float s) => toxinLoad[SegmentIndexAt(s)];

        /// <summary>Toxin decays when the secretors are cleared -- the
        /// subversion is reversible, which is what makes clearing a cholera
        /// crowd feel like it accomplished something.</summary>
        private void DecayToxin(float deltaTime)
        {
            for (int i = 0; i < toxinLoad.Length; i++)
            {
                if (toxinLoad[i] <= 0f) continue;
                toxinLoad[i] = Mathf.Max(0f, toxinLoad[i] - BarrierTuning.ToxinDecayPerSecond * deltaTime);
            }
        }

        /// <summary>
        /// Sprint 24: how inflamed the wall is around <paramref name="s"/>,
        /// 0..1. Read from the host tissue immediately behind that stretch
        /// -- dead and infected cells are the signal, because those are what
        /// an oxidative burst and a necrotic kill leave behind.
        ///
        /// Deliberately reads the tissue rather than a cytokine value: the
        /// thing the organism respires is the *product of damage*, not the
        /// recruitment signal, and a player who clears messily should feed
        /// it even if no cytokine is left.
        /// </summary>
        public float LocalInflammation(float s)
        {
            if (tissueGrid == null) return 0f;
            int lane = LaneOf(s);
            int edge = board.TissueLumenEdgeAxisIndex;

            int looked = 0, inflamed = 0;
            for (int d = 0; d < InflammationSampleDepth; d++)
            {
                int axis = edge - d;
                if (!board.InAxisBounds(axis)) break;
                var c = board.CoarseFromAxis(axis, lane);
                if (!tissueGrid.IsHostGround(c)) continue;
                looked++;
                var st = tissueGrid.GetHostState(c);
                if (st == HostState.Dead || st == HostState.Infected) inflamed++;
            }
            return looked == 0 ? 0f : inflamed / (float)looked;
        }

        /// <summary>How deep behind the wall the inflammation sample looks.</summary>
        private const int InflammationSampleDepth = 4;

        private static float Probability(float perSecond, float deltaTime)
        {
            float p = Mathf.Clamp01(perSecond);
            return 1f - Mathf.Pow(1f - p, Mathf.Max(0f, deltaTime));
        }

        // -- The barrier (INVASION_REWORK_SPEC.md section 6) ------------

        public int SegmentCount => barrier.Length;

        public float SegmentsPerCell => Mathf.Max(1, BarrierTuning.SegmentsPerLane);

        public int SegmentIndexAt(float s) =>
            Mathf.Clamp(Mathf.FloorToInt(s * SegmentsPerCell), 0, barrier.Length - 1);

        public BarrierSegment SegmentAt(int index) =>
            index >= 0 && index < barrier.Length ? barrier[index] : default;

        public BarrierState StateAt(float s) => barrier[SegmentIndexAt(s)].State;

        public float SegmentCentreS(int index) => (index + 0.5f) / SegmentsPerCell;

        /// <summary>How much faster a plaque charges over this stretch, given
        /// what the wall there has already been through.</summary>
        public float BarrierChargeMultiplierAt(float s)
        {
            switch (StateAt(s))
            {
                case BarrierState.Dead: return BarrierTuning.DeadChargeMultiplier;
                case BarrierState.Eroded: return BarrierTuning.ErodedChargeMultiplier;
                default: return 1f;
            }
        }

        /// <summary>Grazing damage from gripping organisms, plus regrowth.
        /// Crypts speed regrowth within reach -- the organ that regenerates
        /// the lattice regenerates the wall too, which is what crypts
        /// actually do.</summary>
        private void TickBarrier(float deltaTime, float currentTime)
        {
            DecayToxin(deltaTime);

            for (int i = 0; i < adhered.Count; i++)
            {
                var p = adhered[i];
                // A rafted organism is held away from the cells and pacified
                // besides, so it does no damage. Exclusion is not a fight.
                if (p == null || !p.IsGrippingWall || p.IsCrossLinked) continue;

                // **Only bacteria graze.** A virus has no machinery to chew
                // through epithelium from the outside -- its whole problem is
                // getting INTO a cell. Letting virions erode the wall also
                // produced a genuinely silly loop in testing: a crowd of
                // viruses destroyed the epithelium in four seconds and
                // thereby locked itself out, because a dead stretch has no
                // living cell to infect.
                if (p.Class == PathogenClass.IntracellularVirus) continue;

                // Sprint 24 -- **the toxin secretors do not graze.**
                // Vibrio cholerae never eats the wall and never takes a
                // slot: it sits on the epithelium and secretes, and the
                // cells underneath stay alive while ceasing to be the
                // player's. Routing it through DamageBarrier would make it
                // just another chewer and lose the entire distinction.
                float toxin = p.Profile.ToxinFieldStrength;
                if (toxin > 0f)
                {
                    ApplyToxinField(p.WallS, toxin, p.Profile.ToxinFieldRadiusCells,
                                    p.VirulenceScale * deltaTime);
                    continue;
                }

                DamageBarrier(p.WallS, BarrierTuning.GrazeDamagePerSecond * p.VirulenceScale * deltaTime, currentTime);
            }

            for (int i = 0; i < barrier.Length; i++)
            {
                if (barrier[i].State != BarrierState.Dead)
                {
                    if (barrier[i].Health < BarrierTuning.SegmentMaxHealth)
                    {
                        float heal = BarrierTuning.SegmentMaxHealth / BarrierTuning.RegrowSeconds
                                   * CryptBoostAt(SegmentCentreS(i)) * deltaTime;
                        barrier[i].Health = Mathf.Min(BarrierTuning.SegmentMaxHealth, barrier[i].Health + heal);
                    }
                    barrier[i].State = barrier[i].Health < BarrierTuning.ErodedBelowHealth
                        ? BarrierState.Eroded : BarrierState.Intact;
                    continue;
                }

                if (currentTime < barrier[i].RegrowAt) continue;
                barrier[i].State = BarrierState.Eroded;
                barrier[i].Health = BarrierTuning.ErodedBelowHealth;
                barrier[i].RegrowAt = float.PositiveInfinity;
            }
        }

        private float CryptBoostAt(float s)
        {
            var features = GutWallFeatures.Current;
            if (features == null) return 1f;
            var lanes = features.CryptLanes;
            for (int i = 0; i < lanes.Count; i++)
                if (Mathf.Abs(SOfLane(lanes[i]) - s) <= BarrierTuning.CryptReachCells)
                    return BarrierTuning.CryptRegrowMultiplier;
            return 1f;
        }

        public void DamageBarrier(float s, float amount, float currentTime)
        {
            int index = SegmentIndexAt(s);
            if (barrier[index].State == BarrierState.Dead) return;
            barrier[index].Health -= amount;
            if (barrier[index].Health > 0f) return;
            barrier[index].Health = 0f;
            barrier[index].State = BarrierState.Dead;
            barrier[index].RegrowAt = currentTime + BarrierTuning.RegrowSeconds;
        }

        private void KillBarrier(float centre, float footprint, float currentTime)
        {
            int lo = SegmentIndexAt(centre - footprint);
            int hi = SegmentIndexAt(centre + footprint);
            for (int i = lo; i <= hi; i++)
            {
                barrier[i].State = BarrierState.Dead;
                barrier[i].Health = 0f;
                barrier[i].RegrowAt = currentTime + BarrierTuning.RegrowSeconds;
            }
        }

        public int DeadSegmentCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < barrier.Length; i++) if (barrier[i].State == BarrierState.Dead) n++;
                return n;
            }
        }

        // -- Player agency on the wall (section 12.1 D3) -----------------

        /// <summary>Knocks charge off any plaque near a point -- a tower
        /// firing on a forming blister. A visible blister has to be something
        /// the player can answer, not only watch.</summary>
        public float SuppressPlaques(float s, float radiusCells, float deltaTime)
        {
            float removed = 0f;
            for (int i = 0; i < plaques.Length; i++)
            {
                if (!plaques[i].Live) continue;
                if (Mathf.Abs(plaques[i].CentreS - s) > radiusCells) continue;
                float before = plaques[i].Charge;
                plaques[i].Charge = Mathf.Max(0f, before - BarrierTuning.SuppressionPerSecond * deltaTime);
                removed += before - plaques[i].Charge;
            }
            return removed;
        }

        /// <summary>Restores wall around a point. The bought repair.</summary>
        public int RepairBarrier(float s)
        {
            int lo = SegmentIndexAt(s - BarrierTuning.RepairRadiusCells);
            int hi = SegmentIndexAt(s + BarrierTuning.RepairRadiusCells);
            int repaired = 0;
            for (int i = lo; i <= hi; i++)
            {
                if (barrier[i].State == BarrierState.Intact) continue;
                barrier[i].State = BarrierState.Intact;
                barrier[i].Health = BarrierTuning.RepairHealthRestored;
                barrier[i].RegrowAt = float.PositiveInfinity;
                repaired++;
            }
            return repaired;
        }

        // -- Sentinel sloughing (section 12.1 D4) ------------------------

        /// <summary>Level of the mucus-turnover shop row. Zero is fine: the
        /// sentinel response is INNATE and works from round 1. The row buys
        /// amplification, not the mechanic.</summary>
        public int MucusTurnoverLevel { get; set; }

        /// <summary>Organisms expelled by sloughing. They are returned to the
        /// flow, not killed.</summary>
        public int Sloughed { get; private set; }

        /// <summary>Fired when a crypt's sentinel dumps its mucus, with
        /// (centre s, patch half-width, organisms expelled).</summary>
        public event System.Action<float, float, int> Sloughs;

        /// <summary>
        /// The sentinel goblet cell response (Birchenough 2016). Each crypt
        /// watches the stretch of wall in front of it; when pathogens crowd
        /// close enough, it dumps its whole mucus load and its neighbours
        /// follow through gap junctions, expelling whatever is caught.
        ///
        /// Triggered, not timed. Local, not global. Non-specific, so it needs
        /// no knowledge and works from round 1. And it EXPELS rather than
        /// kills, like everything else in this compartment.
        /// </summary>
        private void TickSentinels(float currentTime)
        {
            var features = GutWallFeatures.Current;
            if (features == null) return;

            var lanes = features.CryptLanes;
            float cooldown = Mathf.Max(1f,
                BarrierTuning.SentinelCooldownSeconds
                - BarrierTuning.SentinelCooldownPerLevel * MucusTurnoverLevel);

            for (int i = 0; i < lanes.Count; i++)
            {
                int lane = lanes[i];
                if (lane < 0 || lane >= cryptReadyAt.Length) continue;
                if (currentTime < cryptReadyAt[lane]) continue;

                float centre = SOfLane(lane);
                if (CrowdAt(centre) < BarrierTuning.SentinelTriggerCrowd) continue;

                cryptReadyAt[lane] = currentTime + cooldown;
                Slough(centre, currentTime);
            }
        }

        /// <summary>
        /// One slough event. Everything in the patch goes back to the flow,
        /// and every plaque centred there loses its charge outright --
        /// pressure is physically removed, not merely reduced.
        ///
        /// The barrier is neither healed nor damaged here. A slough is a
        /// mucus-and-shed-cell event; repairing epithelium is the crypt's
        /// other job. Keeping them separate is what stops the mucus row from
        /// quietly becoming the wall-repair row as well.
        /// </summary>
        public int Slough(float centre, float currentTime)
        {
            float half = BarrierTuning.SentinelPatchCells
                       + BarrierTuning.SentinelPatchPerLevel * MucusTurnoverLevel;

            int expelled = 0;
            for (int i = adhered.Count - 1; i >= 0; i--)
            {
                var p = adhered[i];
                if (p == null) { adhered.RemoveAt(i); continue; }
                if (Mathf.Abs(p.WallS - centre) > half) continue;

                // Rafts go too, whole. Mucus turnover is the transport half
                // of immune exclusion, so the two compound rather than
                // overlapping.
                if (p.IsCrossLinked) LeaveClump(p);
                adhered.RemoveAt(i);
                p.ReturnToFlow(LaneOf(p.WallS), currentTime + BarrierTuning.SloughRefractorySeconds);
                expelled++;
            }

            for (int i = 0; i < plaques.Length; i++)
            {
                if (!plaques[i].Live) continue;
                if (Mathf.Abs(plaques[i].CentreS - centre) > half) continue;
                plaques[i].Charge = 0f;
                plaques[i].MemberCount = 0;
            }

            Sloughed += expelled;
            if (expelled > 0) Sloughs?.Invoke(centre, half, expelled);
            return expelled;
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
