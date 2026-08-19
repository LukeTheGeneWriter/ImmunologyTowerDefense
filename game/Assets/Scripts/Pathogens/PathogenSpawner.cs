using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;

namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Periodically spawns pathogens from a pooled template
    /// (GAME_DESIGN.md section 8 -- no raw Instantiate/Destroy). Recomputes
    /// the cytokine field on a timer (not just when the adhered set
    /// changes -- Sprint 1 closing task: infected cells now secrete
    /// continuously, ramping strength over time per
    /// TissueGrid.GetSecretionStrength, so the field itself keeps changing
    /// even with a static set of infected slots), and releases
    /// transited-through pathogens back to the pool when they exit the
    /// right edge.
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
        /// infected-source set. Cheap (<=200 coarse cells x a few dozen
        /// sources) so a short interval is fine; fast enough that the
        /// heatmap visual cue and the movement bias both track secretion
        /// ramp-up smoothly rather than in visible jumps.</summary>
        private const float FieldRecomputeIntervalSeconds = 0.4f;

        private readonly List<PathogenAgent> live = new List<PathogenAgent>();
        private float spawnTimer;
        private float fieldRecomputeTimer;

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
