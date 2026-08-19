using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Periodically spawns pathogens from a pooled template
    /// (GAME_DESIGN.md section 8 -- no raw Instantiate/Destroy). Recomputes
    /// the cytokine field on a timer, and releases transited-through or
    /// combat-cleared pathogens back to the pool via OnPathogenExit.
    ///
    /// Sprint 2 addition: RequestSpread is the production implementation of
    /// GAME_DESIGN.md section 4a's viral spread -- the callback an adhered
    /// PathogenAgent invokes (via PathogenAgent.TickCombat) once its
    /// incubation period elapses. Also spawns new pathogen instances
    /// through the same pool as normal spawns (still no raw
    /// Instantiate/Destroy), so a spreading infection is just as pooled as
    /// an ordinary one.
    /// </summary>
    public class PathogenSpawner : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private PrefabPool pool;

        [SerializeField] private float spawnIntervalSeconds = 2.5f;
        [SerializeField] private int maxLivePathogens = 40;

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

        /// <summary>Read-only view of currently live pathogens -- exposed
        /// for Assets/Editor/CombatVerification.cs, which needs to advance
        /// every live agent's combat tick (including spread-created
        /// children) with an explicit simulated time rather than relying on
        /// Unity's Update() loop, which doesn't run in Editor batchmode
        /// outside play mode.</summary>
        public IReadOnlyList<PathogenAgent> Live => live;

        public void Initialize(BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField, GameObject pathogenTemplate)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;

            pool = gameObject.AddComponent<PrefabPool>();
            pool.SetPrefab(pathogenTemplate);
        }

        private void Update()
        {
            if (board == null) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnIntervalSeconds && live.Count < maxLivePathogens)
            {
                spawnTimer = 0f;
                SpawnOne();
            }

            fieldRecomputeTimer += Time.deltaTime;
            if (fieldRecomputeTimer >= FieldRecomputeIntervalSeconds)
            {
                fieldRecomputeTimer = 0f;
                cytokineField.Recompute(tissueGrid.InfectedSources(Time.time));
            }
        }

        private void SpawnOne()
        {
            var go = pool.Get();
            var agent = go.GetComponent<PathogenAgent>();
            agent.Initialize(board, tissueGrid, OnPathogenExit, RequestSpread);
            live.Add(agent);
        }

        /// <summary>
        /// Attempts to spread a virus infection from <paramref name="source"/>
        /// into one free, in-bounds, coarse-grid von Neumann neighbour.
        /// Neighbour order is shuffled each call so spread doesn't visibly
        /// favor one direction. Public (not just called by PathogenAgent)
        /// so Assets/Editor/CombatVerification.cs can drive the exact same
        /// production method a real infection would call.
        /// </summary>
        public bool RequestSpread(CoarseCoord source, float currentTime)
        {
            if (live.Count >= maxLivePathogens) return false;

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
                if (!board.InCoarseBounds(candidate)) continue;
                if (!tissueGrid.IsSlotFree(candidate)) continue;

                var go = pool.Get();
                var child = go.GetComponent<PathogenAgent>();
                child.InitializeAdheredDirect(board, tissueGrid, OnPathogenExit, RequestSpread, candidate, PathogenClass.IntracellularVirus, currentTime);
                live.Add(child);
                return true;
            }

            return false;
        }

        private void OnPathogenExit(PathogenAgent agent)
        {
            live.Remove(agent);
            agent.ResetForPool();
            pool.Release(agent.gameObject);
        }
    }
}
