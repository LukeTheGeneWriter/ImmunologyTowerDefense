using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;

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

        /// <summary>Read-only view of currently live pathogens -- exposed so
        /// a headless harness can advance every agent with an explicit
        /// simulated clock (Unity's Update() does not run in Editor
        /// batchmode outside play mode).</summary>
        public IReadOnlyList<PathogenAgent> Live => live;

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
            Tick(Time.deltaTime, Time.time);
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
            spawnTimer += deltaTime;
            if (spawnTimer >= spawnIntervalSeconds && live.Count < maxLivePathogens)
            {
                spawnTimer = 0f;
                SpawnOne();
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

        /// <summary>
        /// Spawns one pathogen of <paramref name="pClass"/> at or beside
        /// <paramref name="source"/>. Two production callers, both through
        /// PathogenAgent's `onSpawnNear` delegate:
        ///
        ///  - **Viral spread** (`IntracellularVirus`) -- from an infected
        ///    cell after incubation. The target must be an in-bounds,
        ///    TISSUE-BAND, **`Healthy`**, occupant-free NEIGHBOUR. The
        ///    Healthy check (Sprint 5) is half of what makes the firebreak
        ///    emerge -- a virus ringed by dead/infected ground gets `false`
        ///    here and retries rather than burning its one-shot spread. The
        ///    band check (Sprint 4) keeps it out of the lumen.
        ///  - **Bacterial brood burst** (`IntracellularBacterium`, Sprint 6)
        ///    -- from a cell just drained to death. The bacterium is
        ///    extracellular, so the target only needs to be an in-bounds,
        ///    TISSUE-BAND, occupant-free cell; `source` itself is tried first
        ///    (the dead cell the brood is bursting from), then its
        ///    neighbours. **No `Healthy` requirement.**
        ///
        /// Neighbour order is shuffled each call so neither favours a
        /// direction.
        /// </summary>
        public bool RequestSpawnNear(CoarseCoord source, PathogenClass pClass, float currentTime)
        {
            if (live.Count >= maxLivePathogens) return false;

            bool needsHealthyHost = pClass == PathogenClass.IntracellularVirus;

            // A brood may land on the source cell itself (a virus never can --
            // that cell is already infected).
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
