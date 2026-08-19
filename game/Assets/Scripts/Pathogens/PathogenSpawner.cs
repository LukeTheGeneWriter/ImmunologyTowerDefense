using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Periodically spawns pathogens from a pooled template
    /// (GAME_DESIGN.md section 8 -- no raw Instantiate/Destroy). Recomputes
    /// the cytokine field whenever the set of adhered pathogens changes,
    /// and releases transited-through pathogens back to the pool when they
    /// exit the right edge.
    /// </summary>
    public class PathogenSpawner : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private PrefabPool pool;

        [SerializeField] private float spawnIntervalSeconds = 2.5f;
        [SerializeField] private int maxLivePathogens = 40;

        private readonly List<PathogenAgent> live = new List<PathogenAgent>();
        private float spawnTimer;
        private int lastAdheredCount = -1;

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

            if (tissueGrid.AdheredCount != lastAdheredCount)
            {
                lastAdheredCount = tissueGrid.AdheredCount;
                cytokineField.Recompute(tissueGrid.AdheredCoords());
            }
        }

        private void SpawnOne()
        {
            var go = pool.Get();
            var agent = go.GetComponent<PathogenAgent>();
            agent.Initialize(board, tissueGrid, OnPathogenExit);
            live.Add(agent);
        }

        private void OnPathogenExit(PathogenAgent agent)
        {
            live.Remove(agent);
            agent.ResetForPool();
            pool.Release(agent.gameObject);
        }
    }
}
