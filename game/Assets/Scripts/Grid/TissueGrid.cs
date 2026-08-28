using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// What the tissue itself is doing at a coarse position -- the HOST
    /// LAYER of GAME_DESIGN.md section 1c's two-layer model. Director-set
    /// states; the structure around them is the head session's proposal.
    ///
    /// Outside the tissue band every cell is <see cref="Empty"/> and stays
    /// that way: the base and the lumen are not made of host cells, so
    /// nothing there infects, dies, leaves debris, or regrows.
    /// </summary>
    public enum HostState
    {
        /// <summary>No cell and no debris -- bare ground, available for
        /// regrowth. Also the permanent state of every non-tissue cell.</summary>
        Empty,

        /// <summary>An intact host cell. What a virus needs to spread
        /// into.</summary>
        Healthy,

        /// <summary>A host cell harbouring an intracellular pathogen (virus
        /// or intracellular bacterium). Renders as the host cell, not as the
        /// pathogen (section 4a), and secretes cytokines.</summary>
        Infected,

        /// <summary>The cell is gone. The position holds DEBRIS -- real
        /// terrain, not a decal: it blocks regrowth until cleared.</summary>
        Dead,
    }

    /// <summary>
    /// Coarse-grid occupancy, rewritten in Sprint 5 around GAME_DESIGN.md
    /// section 1c's **two independent layers per position**:
    ///
    ///  - **Host layer** -- <see cref="HostState"/>, plus the host cell's
    ///    health, its debris amount once dead, and (when Infected) the
    ///    intracellular pathogen living in it.
    ///  - **Occupant layer** -- the extracellular thing standing here: a
    ///    large bacterium, an intracellular bacterium currently OUTSIDE a
    ///    cell, or a free virus particle between hosts.
    ///
    /// ## Why two layers and not one enum (do not "simplify" this)
    ///
    /// The states genuinely co-occur. A bacterium crawling toward the base
    /// passes **over ground that still holds living host cells** -- tissue
    /// is packed with cells and bacteria squeeze between them. "Occupied by
    /// bacteria" and "occupied by a healthy cell" are simultaneously true,
    /// and Sprints 1-4's single `PathogenAgent[,]` could not say that. It is
    /// also what makes the future parasite class (one pathogen spanning
    /// several positions) tractable rather than a rewrite.
    ///
    /// **Immune cells are NOT part of either layer.** They are tracked on
    /// the fine lattice by SearchUnit and always were; do not fold them in.
    ///
    /// ## What survived from Sprint 1
    ///
    /// The continuous cytokine secretion ramp. `InfectedSources` still feeds
    /// CytokineField, still ramps from Base to Max over
    /// InfectionRampSeconds, and still takes `currentTime` explicitly so
    /// this class stays plain, headlessly-drivable C#. What changed is only
    /// where a source comes from: an `Infected` host cell OR a position
    /// holding an extracellular occupant. Both secrete -- see
    /// <see cref="SecretionStartTime"/> for why the second one is kept.
    ///
    /// ## Explicit time
    ///
    /// <see cref="Tick"/> takes deltaTime and currentTime, like
    /// PathogenAgent.SimulationTick / GutInterface.Tick /
    /// BoneMarrowManager.Tick. Debris decay and host regrowth live there,
    /// never in an Update(). TissueDriver forwards the real clock; a harness
    /// forwards a fake one.
    /// </summary>
    public class TissueGrid
    {
        private readonly BoardConfig board;

        // -- Host layer --
        private readonly HostState[,] host;
        private readonly float[,] hostHealth;
        private readonly PathogenAgent[,] intracellular;
        private readonly float[,] infectionStartTime;
        private readonly float[,] debrisAmount;
        private readonly float[,] emptySince;

        // -- Occupant layer --
        private readonly PathogenAgent[,] occupant;
        private readonly float[,] occupantStartTime;

        private float sweepAccumulator;

        /// <summary>Debris amount a freshly dead host cell leaves. A
        /// normalized 0..1 quantity rather than a hit-point pool, so
        /// macrophage clearance and self-dissipation are directly
        /// comparable rates against the same number.</summary>
        public const float FullDebris = 1f;

        /// <summary>Secretion strength (arbitrary field units) at the moment
        /// a cell becomes a cytokine source.</summary>
        public const float BaseSecretionStrength = 6f;

        /// <summary>Secretion strength once fully ramped. Also the
        /// normalization reference for BoardRenderer's heatmap tint.</summary>
        public const float MaxSecretionStrength = 32f;

        /// <summary>Seconds for a source's secretion to ramp from Base to
        /// Max. Unchanged since Sprint 1.</summary>
        public const float InfectionRampSeconds = 20f;

        public TissueGrid(BoardConfig board)
        {
            this.board = board;
            int cols = board.Columns, rows = board.Rows;

            host = new HostState[cols, rows];
            hostHealth = new float[cols, rows];
            intracellular = new PathogenAgent[cols, rows];
            infectionStartTime = new float[cols, rows];
            debrisAmount = new float[cols, rows];
            emptySince = new float[cols, rows];
            occupant = new PathogenAgent[cols, rows];
            occupantStartTime = new float[cols, rows];

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    infectionStartTime[col, row] = -1f;
                    occupantStartTime[col, row] = -1f;
                    emptySince[col, row] = -1f;
                }
            }

            SeedHealthyTissue();
        }

        /// <summary>The tissue band starts full of healthy host cells
        /// (SPRINT_PLAN.md scope item 2). Base and lumen cells are left
        /// Empty forever -- see <see cref="IsHostGround"/>.</summary>
        private void SeedHealthyTissue()
        {
            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    var c = new CoarseCoord(col, row);
                    if (!IsHostGround(c)) continue;
                    host[col, row] = HostState.Healthy;
                    hostHealth[col, row] = TissueTuning.HostCellMaxHealth;
                    HealthyCount++;
                }
            }
        }

        /// <summary>True where host cells can exist at all: the tissue band,
        /// and only the tissue band. Every host-layer mutator below silently
        /// no-ops elsewhere, so nothing can accidentally grow a host cell in
        /// the lumen or the player's base.</summary>
        public bool IsHostGround(CoarseCoord c) =>
            board.InCoarseBounds(c) && board.BandOf(c) == BoardBand.Tissue;

        // ==================================================================
        // Host layer -- reads
        // ==================================================================

        public HostState GetHostState(CoarseCoord c) =>
            board.InCoarseBounds(c) ? host[c.Column, c.Row] : HostState.Empty;

        public float GetHostHealth(CoarseCoord c) =>
            board.InCoarseBounds(c) ? hostHealth[c.Column, c.Row] : 0f;

        /// <summary>How much debris is left at this position, 0..1. Non-zero
        /// exactly when the host state is <see cref="HostState.Dead"/>.</summary>
        public float GetDebrisAmount(CoarseCoord c) =>
            board.InCoarseBounds(c) ? debrisAmount[c.Column, c.Row] : 0f;

        /// <summary>The pathogen living inside this position's host cell, or
        /// null. Non-null exactly when the host state is
        /// <see cref="HostState.Infected"/>.</summary>
        public PathogenAgent GetIntracellularAt(CoarseCoord c) =>
            board.InCoarseBounds(c) ? intracellular[c.Column, c.Row] : null;

        /// <summary>GAME_DESIGN.md section 1b step 4's whole viral rule, as
        /// one predicate: a virus may only spread into a HEALTHY neighbour.
        /// The firebreak against dead ground is emergent from this plus the
        /// free-virus death timer -- there is deliberately no firebreak
        /// check anywhere in this codebase.</summary>
        public bool IsHealthyHost(CoarseCoord c) => GetHostState(c) == HostState.Healthy;

        /// <summary>Debris blocks regrowth (GAME_DESIGN.md section 1c). Only
        /// bare, already-cleaned ground regrows.</summary>
        public bool CanRegrow(CoarseCoord c) => IsHostGround(c) && GetHostState(c) == HostState.Empty;

        public int HealthyCount { get; private set; }
        public int InfectedCount { get; private set; }
        public int DeadCount { get; private set; }

        // ==================================================================
        // Host layer -- writes
        // ==================================================================

        /// <summary>
        /// A pathogen enters this position's healthy host cell:
        /// `Healthy -> Infected`, and the pathogen is recorded as living
        /// INSIDE the cell rather than replacing it (GAME_DESIGN.md section
        /// 4a: "hides inside a host cell; the cell is not replaced"). Fails
        /// on anything that is not an uninfected healthy host.
        ///
        /// <paramref name="startTime"/> is when THIS infection began and is
        /// what the cytokine ramp measures from -- an intracellular
        /// bacterium that leaves a cell and re-enters another one carries
        /// its own start time, exactly like Sprint 4's moving pathogen did.
        /// </summary>
        public bool TryInfect(CoarseCoord c, PathogenAgent pathogen, float startTime)
        {
            if (pathogen == null || !IsHostGround(c)) return false;
            if (host[c.Column, c.Row] != HostState.Healthy) return false;
            if (intracellular[c.Column, c.Row] != null) return false;

            host[c.Column, c.Row] = HostState.Infected;
            intracellular[c.Column, c.Row] = pathogen;
            infectionStartTime[c.Column, c.Row] = startTime;
            HealthyCount--;
            InfectedCount++;
            return true;
        }

        /// <summary>
        /// Detaches the intracellular pathogen without killing the cell --
        /// `Infected -> Healthy`. Currently unused by production code and
        /// kept deliberately: GAME_DESIGN.md section 1c says killing an
        /// intracellular pathogen flips the host "toward `Dead` or `Healthy`
        /// depending on how much damage the cell took," and adaptive
        /// immunity's precise MHC-I killing (section 4a, ~10% knowledge) is
        /// the case that will want this. Innate clearing always kills the
        /// cell -- see <see cref="KillHostCell"/>.
        /// </summary>
        public bool ReleaseIntracellular(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return false;
            if (host[c.Column, c.Row] != HostState.Infected) return false;

            host[c.Column, c.Row] = HostState.Healthy;
            intracellular[c.Column, c.Row] = null;
            infectionStartTime[c.Column, c.Row] = -1f;
            InfectedCount--;
            HealthyCount++;
            return true;
        }

        /// <summary>
        /// The cell dies and leaves DEBRIS: any host state -> `Dead`, full
        /// debris, intracellular reference dropped. Idempotent on an
        /// already-dead cell (it does not top the debris back up, so a
        /// half-eaten pile is not refilled by a second kill call).
        ///
        /// This is the single place a host cell can die -- infection
        /// clearing, bacterial lysis, bacterial grazing, and neutrophil
        /// collateral all funnel here, so "killing an infected cell leaves
        /// debris" cannot be true on one path and false on another.
        /// </summary>
        public bool KillHostCell(CoarseCoord c)
        {
            if (!IsHostGround(c)) return false;
            var previous = host[c.Column, c.Row];
            if (previous == HostState.Dead) return false;

            if (previous == HostState.Healthy) HealthyCount--;
            else if (previous == HostState.Infected) InfectedCount--;

            // Whatever was living inside dies with the cell -- GAME_DESIGN.md
            // section 4a's whole point about innate clearing of intracellular
            // infections being destructive. Detach BEFORE notifying, so the
            // pathogen's own clear path (which calls back in here) finds an
            // already-dead cell and stops.
            var resident = intracellular[c.Column, c.Row];

            host[c.Column, c.Row] = HostState.Dead;
            intracellular[c.Column, c.Row] = null;
            infectionStartTime[c.Column, c.Row] = -1f;
            hostHealth[c.Column, c.Row] = 0f;
            debrisAmount[c.Column, c.Row] = FullDebris;
            emptySince[c.Column, c.Row] = -1f;
            DeadCount++;

            if (resident != null) resident.OnHostCellDestroyed();
            return true;
        }

        /// <summary>
        /// Direct damage to the host cell itself (a large bacterium grazing
        /// the ground it crosses, a neutrophil's degranulation burst).
        /// Reaching zero kills the cell and leaves debris.
        /// </summary>
        /// <returns>True if this damage killed the cell.</returns>
        public bool DamageHostCell(CoarseCoord c, float amount)
        {
            if (!IsHostGround(c) || amount <= 0f) return false;
            var state = host[c.Column, c.Row];
            if (state != HostState.Healthy && state != HostState.Infected) return false;

            hostHealth[c.Column, c.Row] -= amount;
            if (hostHealth[c.Column, c.Row] > 0f) return false;
            return KillHostCell(c);
        }

        /// <summary>
        /// Efferocytosis: removes <paramref name="amount"/> of debris.
        /// When the pile reaches zero the position becomes bare `Empty`
        /// ground and its regrowth clock starts -- which is exactly why
        /// debris "blocks regeneration": regrowth is only ever evaluated for
        /// `Empty`, and a `Dead` cell is not `Empty`.
        /// </summary>
        /// <returns>True if this call finished the pile off.</returns>
        public bool ClearDebris(CoarseCoord c, float amount, float currentTime)
        {
            if (!board.InCoarseBounds(c) || amount <= 0f) return false;
            if (host[c.Column, c.Row] != HostState.Dead) return false;

            debrisAmount[c.Column, c.Row] -= amount;
            if (debrisAmount[c.Column, c.Row] > 0f) return false;
            BecomeEmpty(c, currentTime);
            return true;
        }

        private void BecomeEmpty(CoarseCoord c, float currentTime)
        {
            if (host[c.Column, c.Row] == HostState.Dead) DeadCount--;
            else if (host[c.Column, c.Row] == HostState.Healthy) HealthyCount--;
            else if (host[c.Column, c.Row] == HostState.Infected) InfectedCount--;

            host[c.Column, c.Row] = HostState.Empty;
            debrisAmount[c.Column, c.Row] = 0f;
            hostHealth[c.Column, c.Row] = 0f;
            intracellular[c.Column, c.Row] = null;
            infectionStartTime[c.Column, c.Row] = -1f;
            emptySince[c.Column, c.Row] = currentTime;
        }

        private void Regrow(CoarseCoord c)
        {
            host[c.Column, c.Row] = HostState.Healthy;
            hostHealth[c.Column, c.Row] = TissueTuning.HostCellMaxHealth;
            emptySince[c.Column, c.Row] = -1f;
            HealthyCount++;
        }

        /// <summary>Test/bootstrap hook: force a position's host state
        /// directly. Used by the verification harnesses to lay out a band of
        /// dead ground (the firebreak fixture) without having to kill fifty
        /// cells through combat first. Not called by production code.</summary>
        public void SeedHostState(CoarseCoord c, HostState state, float currentTime)
        {
            if (!IsHostGround(c)) return;
            switch (state)
            {
                case HostState.Dead:
                    KillHostCell(c);
                    break;
                case HostState.Empty:
                    BecomeEmpty(c, currentTime);
                    break;
                case HostState.Healthy:
                    BecomeEmpty(c, currentTime);
                    Regrow(c);
                    break;
                default:
                    Debug.LogError($"[TissueGrid] SeedHostState cannot seed {state} -- infection needs a pathogen, use TryInfect.");
                    break;
            }
        }

        // ==================================================================
        // Occupant layer
        // ==================================================================

        /// <summary>True if nothing extracellular is standing here. Note it
        /// says NOTHING about the host layer -- a position with a living
        /// host cell in it is still a free occupant slot, which is the whole
        /// point of the split.</summary>
        public bool IsOccupantFree(CoarseCoord c) =>
            board.InCoarseBounds(c) && occupant[c.Column, c.Row] == null;

        /// <summary>Claims the occupant layer at a position. False if
        /// something extracellular is already there.
        ///
        /// <paramref name="secretionStartTime"/> is carried by the pathogen
        /// rather than reset per step, unchanged from Sprint 4: a walking
        /// pathogen releases one position and claims the next roughly once a
        /// second, and restarting the cytokine ramp each time would pin
        /// every source at BaseSecretionStrength forever.</summary>
        public bool TryClaimOccupant(CoarseCoord c, PathogenAgent pathogen, float secretionStartTime)
        {
            if (pathogen == null || !IsOccupantFree(c)) return false;
            occupant[c.Column, c.Row] = pathogen;
            occupantStartTime[c.Column, c.Row] = secretionStartTime;
            OccupantCount++;
            return true;
        }

        public void ReleaseOccupant(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return;
            if (occupant[c.Column, c.Row] == null) return;
            occupant[c.Column, c.Row] = null;
            occupantStartTime[c.Column, c.Row] = -1f;
            OccupantCount--;
        }

        public PathogenAgent GetOccupantAt(CoarseCoord c) =>
            board.InCoarseBounds(c) ? occupant[c.Column, c.Row] : null;

        /// <summary>Extracellular pathogens currently standing on the
        /// board.</summary>
        public int OccupantCount { get; private set; }

        /// <summary>Every pathogen the tissue is currently holding, either
        /// layer -- what Sprints 1-4 called `AdheredCount`. Kept as the
        /// natural replacement for that counter now that "in tissue" can
        /// mean two structurally different things.</summary>
        public int TissuePathogenCount => OccupantCount + InfectedCount;

        /// <summary>
        /// What an innate immune cell can hit with ordinary contact damage
        /// here: the **extracellular occupant only**.
        ///
        /// **Sprint 6 (GAME_DESIGN.md §4b):** a pathogen *inside* a host
        /// cell is no longer returned. An intracellular infection cannot be
        /// touched by ordinary innate damage at all -- it is reached only by
        /// the contact stress-sense roll (`SearchUnit.CheckStressSense` ->
        /// `KillHostCell`, a loud necrotic kill that takes the cell and
        /// everything in it), by the infection running its course, or by the
        /// not-yet-built stress-sensor / adaptive units. Sprints 2-5
        /// returned the intracellular resident here and let a macrophage
        /// grind it down through the cell; that "innate clearing is
        /// destructive" path is what §4b replaces.
        /// </summary>
        public PathogenAgent GetAttackableAt(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return null;
            return occupant[c.Column, c.Row];
        }

        // ==================================================================
        // Cytokine secretion (Sprint 1 mechanism, preserved)
        // ==================================================================

        /// <summary>
        /// When this position started secreting, or -1 if it is not a
        /// source. An `Infected` host is the section 1c source; a position
        /// holding an extracellular occupant is kept as a source too, and
        /// that is a deliberate carry-forward rather than an oversight --
        /// Sprints 1-4 treated any pathogen-holding slot as inflamed, real
        /// tissue absolutely does signal around an extracellular bacterium,
        /// and dropping it would have quietly deleted ~30% of the field's
        /// sources (every LargeBacterium) in a sprint whose brief says the
        /// cytokine mechanism must survive unchanged.
        ///
        /// Whichever started EARLIER wins, so a cell that was already
        /// inflamed does not have its ramp reset by a bacterium wandering
        /// over it.
        /// </summary>
        public float SecretionStartTime(CoarseCoord c)
        {
            if (!board.InCoarseBounds(c)) return -1f;
            float a = infectionStartTime[c.Column, c.Row];
            float b = occupantStartTime[c.Column, c.Row];
            if (a < 0f) return b;
            if (b < 0f) return a;
            return Mathf.Min(a, b);
        }

        /// <summary>Current cytokine secretion strength at this position --
        /// 0 if it is not a source. Ramps linearly from
        /// BaseSecretionStrength to MaxSecretionStrength over
        /// InfectionRampSeconds. Unchanged behaviour from Sprint 1.</summary>
        public float GetSecretionStrength(CoarseCoord c, float currentTime)
        {
            float start = SecretionStartTime(c);
            if (start < 0f) return 0f;
            float age = Mathf.Max(0f, currentTime - start);
            float t = Mathf.Clamp01(age / InfectionRampSeconds);
            return Mathf.Lerp(BaseSecretionStrength, MaxSecretionStrength, t);
        }

        /// <summary>Every currently-secreting position paired with its
        /// current strength -- what CytokineField.Recompute consumes.
        /// Chemotaxis biases toward this, and BoardRenderer's heat tint
        /// draws it.</summary>
        public IEnumerable<(CoarseCoord Coord, float Strength)> InfectedSources(float currentTime)
        {
            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    var coord = new CoarseCoord(col, row);
                    if (SecretionStartTime(coord) < 0f) continue;
                    yield return (coord, GetSecretionStrength(coord, currentTime));
                }
            }
        }

        // ==================================================================
        // Simulation
        // ==================================================================

        /// <summary>
        /// Debris self-dissipation and host-cell regrowth, the two host-layer
        /// processes that run on their own clock. Explicit time, per this
        /// project's standing convention -- TissueDriver forwards
        /// Time.deltaTime/Time.time in a real build, and every verification
        /// harness forwards a simulated clock.
        ///
        /// Decay is integrated over the ACCUMULATED delta rather than per
        /// sweep, so TissueTuning.SweepIntervalSeconds is purely a cost knob:
        /// changing it changes how often the grid is walked and nothing about
        /// how long debris lasts. That also means a harness may legitimately
        /// advance in coarse slices (one call per simulated minute) without
        /// the numbers drifting.
        /// </summary>
        public void Tick(float deltaTime, float currentTime)
        {
            if (deltaTime <= 0f) return;
            sweepAccumulator += deltaTime;
            float interval = Mathf.Max(0.0001f, TissueTuning.SweepIntervalSeconds);
            if (sweepAccumulator < interval) return;

            float elapsed = sweepAccumulator;
            sweepAccumulator = 0f;

            float dissipation = elapsed / Mathf.Max(0.0001f, TissueTuning.DebrisSelfDissipationSeconds) * FullDebris;
            float regrowAfter = Mathf.Max(0f, TissueTuning.HostRegenerationSeconds);

            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    var c = new CoarseCoord(col, row);
                    switch (host[col, row])
                    {
                        case HostState.Dead:
                            debrisAmount[col, row] -= dissipation;
                            if (debrisAmount[col, row] <= 0f) BecomeEmpty(c, currentTime);
                            break;

                        case HostState.Empty:
                            if (!IsHostGround(c)) break;
                            if (emptySince[col, row] < 0f) { emptySince[col, row] = currentTime; break; }
                            if (currentTime - emptySince[col, row] >= regrowAfter) Regrow(c);
                            break;
                    }
                }
            }
        }
    }
}
