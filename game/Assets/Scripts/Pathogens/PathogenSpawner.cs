using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Drives the pathogen half of the simulation: spawns into the lumen
    /// from a pooled template (GAME_DESIGN.md section 8 -- no raw
    /// Instantiate/Destroy), advances the gut interface's breach clock,
    /// recomputes the cytokine field on a timer, and returns excreted /
    /// cleared / base-reached pathogens to the pool.
    ///
    /// **Sprint 4** moved the spawn point from "fine column 0, march right"
    /// to "a uniformly random position across the width of the lumen band,
    /// at the upstream end of the flow" (GAME_DESIGN.md section 1a), and
    /// made this the owner of GutInterface.Tick -- the per-position breach
    /// rolls have to be driven by something, and this class already owns the
    /// pathogen lifecycle.
    /// </summary>
    public class PathogenSpawner : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private GutInterface gutInterface;
        private InvasionTally tally;
        private PrefabPool pool;

        /// <summary>Seconds between spawns into the lumen.
        ///
        /// **Raised from Sprint 3's 2.5s, and the live cap from 40.** This
        /// is a SCALE correction, not balance tuning (SPRINT_PLAN.md item 10
        /// forbids the latter): the board went from 150 coarse cells to
        /// 4,000, the lumen alone is 1,000 cells across 40 lanes, and a
        /// pathogen now spends ~14s riding the flow before it can even
        /// adhere. At Sprint 3's rate the channel would read as empty and
        /// the Director could not judge whether the invasion loop works at
        /// all. Flagged in docs/TEAM_RETRO.md as the one number this sprint
        /// moved for reasons other than mechanics.</summary>
        [SerializeField] private float spawnIntervalSeconds = 0.8f;
        [SerializeField] private int maxLivePathogens = 150;

        /// <summary>How often the field is recomputed from the current
        /// infected-source set.</summary>
        private const float FieldRecomputeIntervalSeconds = 0.4f;

        private static readonly (int dc, int dr)[] CoarseNeighborOffsets =
        {
            (1, 0), (-1, 0), (0, 1), (0, -1),
        };

        private readonly List<PathogenAgent> live = new List<PathogenAgent>();
        private float spawnTimer;
        private float fieldRecomputeTimer;

        // -- Round batching (Sprint 7, GAME_DESIGN.md §5d) --
        //
        // The spawner no longer free-runs. RoundController.BeginBatch(n)
        // arms it to spawn exactly n pathogens on the existing interval;
        // it then idles until the next BeginBatch. Gut-interface and
        // cytokine ticking keep running throughout (they no-op with
        // nothing live).
        private bool inBatch;
        private int batchTarget;
        private int batchEmitted;

        // -- Sprint 9: the contaminated food item --
        //
        // BeginRound(count, def) sets foodRound = true and spawns a single
        // food item that transits the lumen, releasing the batch in bursts
        // near the wall. Under a food round the round ends when the food has
        // EXITED, not when the field is clear (the field persists now). The
        // plain BeginBatch(count) path keeps its old field-clear completion,
        // for the harnesses.
        private bool foodRound;
        private bool foodActive;
        private bool foodExited;
        private RoundDefinition foodDef;
        private int foodAxisIndex;   // distance-from-base index, held near the wall
        private int foodCrossIndex;  // position along the flow -- this is what advances
        private float foodStepTimer;
        private int burstsDone;
        private GameObject foodVisual;

        public bool FoodActive => foodActive;

        /// <summary>Read-only view of currently live pathogens -- exposed so
        /// a headless harness can advance every agent with an explicit
        /// simulated clock (Unity's Update() does not run in Editor
        /// batchmode outside play mode).</summary>
        public IReadOnlyList<PathogenAgent> Live => live;

        public int LiveCount => live.Count;
        public int BatchTarget => batchTarget;
        public int BatchEmitted => batchEmitted;

        /// <summary>Arms the spawner to emit <paramref name="count"/>
        /// pathogens on the bare spawn interval, then stop -- **no food
        /// item**. Kept for the verification harnesses; the game uses
        /// <see cref="BeginRound"/>.</summary>
        public void BeginBatch(int count)
        {
            inBatch = true;
            foodRound = false;
            foodActive = false;
            foodExited = false;
            batchTarget = count < 0 ? 0 : count;
            batchEmitted = 0;
            spawnTimer = 0f;
        }

        /// <summary>Sprint 9: begins a round delivered by a contaminated food
        /// item. It enters the lumen, transits the flow over
        /// <see cref="InvasionTuning.FoodItemTransitSeconds"/>, and drops the
        /// <paramref name="count"/> pathogens in
        /// <see cref="InvasionTuning.FoodItemBurstCount"/> bursts near the
        /// wall, class mix per <paramref name="def"/>. The round ends when
        /// the food has exited.</summary>
        public void BeginRound(int count, RoundDefinition def)
        {
            inBatch = true;
            foodRound = true;
            foodActive = true;
            foodExited = false;
            foodDef = def;
            batchTarget = count < 0 ? 0 : count;
            batchEmitted = 0;
            burstsDone = 0;
            foodStepTimer = 0f;

            foodAxisIndex = Mathf.Clamp(
                board.LumenNearWallAxisIndex + Mathf.Max(0, InvasionTuning.FoodItemWallHugDepth),
                board.LumenNearWallAxisIndex, board.AxisLength - 1);
            foodCrossIndex = board.LumenEntryCrossIndex;

            EnsureFoodVisual();
            foodVisual.SetActive(true);
            PositionFoodVisual();
        }

        /// <summary>Disarms the spawner (round over / defeat). Live
        /// pathogens and the persistent field are untouched; the food item
        /// (if any) is hidden.</summary>
        public void EndBatch()
        {
            inBatch = false;
            foodActive = false;
            if (foodVisual != null) foodVisual.SetActive(false);
        }

        /// <summary>The round's delivery is finished.
        ///
        /// **Sprint 9:** under a food round (the game's path) that means the
        /// batch is fully emitted **and the food item has exited the lumen**
        /// -- the field itself is no longer required to be clear, because it
        /// persists round to round now. The plain <see cref="BeginBatch"/>
        /// path keeps the old rule (emitted + nothing loose in lumen/tissue;
        /// a gut-WALL pile still doesn't count, §6b).</summary>
        public bool BatchComplete
        {
            get
            {
                if (!inBatch || batchEmitted < batchTarget) return false;
                if (foodRound) return foodExited;
                CountByState(out int inLumen, out _, out int inTissue);
                return inLumen == 0 && inTissue == 0;
            }
        }

        public void Initialize(
            BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField,
            GutInterface gutInterface, InvasionTally tally, GameObject pathogenTemplate)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.gutInterface = gutInterface;
            this.tally = tally;

            pool = gameObject.AddComponent<PrefabPool>();
            pool.SetPrefab(pathogenTemplate);
        }

        private void Update()
        {
            if (board == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: the buy phase freezes the spawner, the breach clock, everything
            Tick(Time.deltaTime, ImmunologyTD.Rounds.RoundClock.Time);
        }

        /// <summary>
        /// Real spawn / breach / field-recompute logic, taking time
        /// explicitly rather than reading UnityEngine.Time -- this project's
        /// standing convention, so a headless harness can drive the
        /// production path. Update() forwards the real clock.
        ///
        /// Note this does NOT tick the individual agents: PathogenAgent has
        /// its own Update() in a real build, and a harness advances the
        /// agents it cares about itself via Live/SimulationTick.
        /// </summary>
        public void Tick(float deltaTime, float currentTime)
        {
            if (foodRound)
            {
                if (foodActive) AdvanceFood(deltaTime, currentTime);
            }
            else
            {
                spawnTimer += deltaTime;
                if (inBatch && batchEmitted < batchTarget
                    && spawnTimer >= spawnIntervalSeconds && live.Count < maxLivePathogens)
                {
                    spawnTimer = 0f;
                    SpawnOne();
                    batchEmitted++;
                }
            }

            if (gutInterface != null) gutInterface.Tick(deltaTime, currentTime);

            fieldRecomputeTimer += deltaTime;
            if (fieldRecomputeTimer >= FieldRecomputeIntervalSeconds)
            {
                fieldRecomputeTimer = 0f;
                cytokineField.Recompute(tissueGrid.InfectedSources(currentTime));
            }
        }

        private void SpawnOne()
        {
            var go = pool.Get();
            var agent = go.GetComponent<PathogenAgent>();
            agent.Initialize(board, tissueGrid, gutInterface, tally, OnPathogenExit, RequestSpawnNear);
            live.Add(agent);
        }

        // ------------------------------------------------------------------
        // Sprint 9: the contaminated food item
        // ------------------------------------------------------------------

        /// <summary>Advances the food item one slice: crawl it along the
        /// flow, drop a burst when it has travelled far enough for the next
        /// one, and retire it once it leaves the channel -- force-emitting
        /// any cargo it didn't get to, so a round always delivers its full
        /// count.</summary>
        private void AdvanceFood(float deltaTime, float currentTime)
        {
            int lanes = Mathf.Max(1, board.CrossLength);
            float stepInterval = Mathf.Max(0.01f, InvasionTuning.FoodItemTransitSeconds / lanes);

            foodStepTimer += deltaTime;
            while (foodStepTimer >= stepInterval && foodActive)
            {
                foodStepTimer -= stepInterval;
                foodCrossIndex += board.FlowCrossStep;

                if (!board.InCrossBounds(foodCrossIndex))
                {
                    // Off the downstream end -- excreted. Deliver whatever is
                    // left at the last valid lane, then retire.
                    foodCrossIndex -= board.FlowCrossStep;
                    while (batchEmitted < batchTarget && live.Count < maxLivePathogens)
                        SpawnFromFood();
                    foodActive = false;
                    foodExited = true;
                    if (foodVisual != null) foodVisual.SetActive(false);
                    return;
                }

                PositionFoodVisual();
            }

            // Burst schedule: bursts k = 1..FoodItemBurstCount fire at
            // travelled-fractions k / (FoodItemBurstCount + 1), so they land
            // through the middle of the transit rather than at the ends.
            int burstCount = Mathf.Max(1, InvasionTuning.FoodItemBurstCount);
            float travelled = Mathf.Abs(foodCrossIndex - board.LumenEntryCrossIndex) / (float)lanes;
            while (burstsDone < burstCount
                   && travelled >= (burstsDone + 1) / (float)(burstCount + 1))
            {
                burstsDone++;
                int remainingBursts = burstCount - burstsDone + 1;
                int perBurst = Mathf.CeilToInt((batchTarget - batchEmitted) / (float)Mathf.Max(1, remainingBursts));
                for (int i = 0; i < perBurst && batchEmitted < batchTarget && live.Count < maxLivePathogens; i++)
                    SpawnFromFood();
            }
        }

        /// <summary>Drops one pathogen into the lumen at a wall-hugging cell
        /// near the food item's current position, class per the round
        /// definition. It then rides the flow and rolls adhesion like any
        /// other lumen pathogen -- but starting at the wall, so it mostly
        /// sticks.</summary>
        private void SpawnFromFood()
        {
            int axis = Mathf.Clamp(
                board.LumenNearWallAxisIndex + Random.Range(0, Mathf.Max(1, InvasionTuning.FoodItemWallHugDepth + 1)),
                board.LumenNearWallAxisIndex, board.AxisLength - 1);
            int cross = Mathf.Clamp(foodCrossIndex, 0, board.CrossLength - 1);
            var cell = board.CoarseFromAxis(axis, cross);

            var go = pool.Get();
            var agent = go.GetComponent<PathogenAgent>();
            agent.Initialize(board, tissueGrid, gutInterface, tally, OnPathogenExit, RequestSpawnNear,
                cell, foodDef.RollClass());
            live.Add(agent);
            batchEmitted++;
        }

        private void EnsureFoodVisual()
        {
            if (foodVisual != null) return;
            foodVisual = new GameObject("ContaminatedFoodItem");
            var sr = foodVisual.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.SpriteShapes.FoodBolus; // Sprint 13: lumpy stippled bolus
            sr.color = new Color(0.55f, 0.47f, 0.28f); // dull spoiled-food ochre, unlike any pathogen
            sr.sortingOrder = 22; // above pathogens (20)
            float s = board.CoarseCellWorldSize * 1.4f;
            foodVisual.transform.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
            foodVisual.transform.localScale = new Vector3(s, s, 1f);
            foodVisual.SetActive(false);
        }

        private void PositionFoodVisual()
        {
            if (foodVisual == null) return;
            int cross = Mathf.Clamp(foodCrossIndex, 0, board.CrossLength - 1);
            foodVisual.transform.position = board.CoarseToWorldCenter(board.CoarseFromAxis(foodAxisIndex, cross));
        }

        /// <summary>
        /// Spawns one pathogen of <paramref name="pClass"/> at or beside
        /// <paramref name="source"/>. Three production callers, all through
        /// PathogenAgent's `onSpawnNear` delegate:
        ///
        ///  - **Viral CONTACT-CHAIN spread** (`IntracellularVirus`,
        ///    `asFreeParticle` false) -- the target must be an in-bounds,
        ///    TISSUE-BAND, **`Healthy`**, occupant-free NEIGHBOUR. The
        ///    Healthy check (Sprint 5) is half of what makes the firebreak
        ///    emerge -- a virus ringed by dead/infected ground gets `false`
        ///    here and retries rather than burning its one-shot spread. The
        ///    virion establishes instantly (SettleIntoTissue infects it).
        ///  - **Viral BUDDING / burn-out spill** (`IntracellularVirus`,
        ///    `asFreeParticle` true) -- drops a **free virion** on `source`
        ///    itself (the infected cell, so SettleIntoTissue can't infect it
        ///    and it lands on the occupant layer) or, if that is taken, an
        ///    occupant-free tissue neighbour of any host state. It then
        ///    floats via `PathogenAgent.StepVirus`. **No `Healthy`
        ///    requirement**, but the virion can still only ever ESTABLISH in
        ///    a Healthy cell, so the firebreak holds.
        ///  - **Bacterial brood burst** (`IntracellularBacterium`) -- from a
        ///    cell just drained to death; occupant-free tissue cell, `source`
        ///    first then neighbours, no `Healthy` requirement.
        ///
        /// Neighbour order is shuffled each call so none favours a direction.
        /// </summary>
        public bool RequestSpawnNear(CoarseCoord source, PathogenClass pClass, bool asFreeParticle, float currentTime)
        {
            if (live.Count >= maxLivePathogens) return false;

            bool needsHealthyHost = pClass == PathogenClass.IntracellularVirus && !asFreeParticle;

            // A brood or a budded free virion may land on the source cell
            // itself; a contact-chain virus never can (that cell is infected).
            if (!needsHealthyHost && TrySpawnAt(source, pClass, needsHealthyHost, currentTime)) return true;

            var order = new[] { 0, 1, 2, 3 };
            for (int i = 3; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = order[i];
                order[i] = order[j];
                order[j] = tmp;
            }

            foreach (var idx in order)
            {
                var (dc, dr) = CoarseNeighborOffsets[idx];
                var candidate = new CoarseCoord(source.Column + dc, source.Row + dr);
                if (TrySpawnAt(candidate, pClass, needsHealthyHost, currentTime)) return true;
            }

            return false;
        }

        private bool TrySpawnAt(CoarseCoord candidate, PathogenClass pClass, bool needsHealthyHost, float currentTime)
        {
            if (!board.InCoarseBounds(candidate)) return false;
            if (board.BandOf(candidate) != BoardBand.Tissue) return false;
            if (needsHealthyHost && !tissueGrid.IsHealthyHost(candidate)) return false;
            if (!tissueGrid.IsOccupantFree(candidate)) return false;

            var go = pool.Get();
            var child = go.GetComponent<PathogenAgent>();
            child.InitializeInTissueDirect(
                board, tissueGrid, gutInterface, tally, OnPathogenExit, RequestSpawnNear,
                candidate, pClass, currentTime);
            live.Add(child);
            return true;
        }

        /// <summary>Single exit path for every way a pathogen leaves play --
        /// excreted out the bottom of the lumen, killed in tissue, or
        /// despawned on reaching the base. Also detaches it from the gut
        /// interface if it was still colonising, so a pile never holds a
        /// pooled instance.</summary>
        private void OnPathogenExit(PathogenAgent agent)
        {
            live.Remove(agent);
            if (gutInterface != null) gutInterface.Remove(agent);
            agent.ResetForPool();
            pool.Release(agent.gameObject);
        }

        /// <summary>How many live pathogens are currently in each stage of
        /// the invasion loop -- HUD copy, and a cheap sanity read for a
        /// harness.</summary>
        public void CountByState(out int inLumen, out int atInterface, out int inTissue)
        {
            inLumen = atInterface = inTissue = 0;
            for (int i = 0; i < live.Count; i++)
            {
                switch (live[i].State)
                {
                    case PathogenState.Lumen: inLumen++; break;
                    case PathogenState.AtInterface: atInterface++; break;
                    case PathogenState.InTissue: inTissue++; break;
                }
            }
        }
    }
}
