using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// A hunting unit performing a random walk on the fine lattice (rung 1
    /// of GAME_DESIGN.md sections 7/9's search ladder), or a
    /// cytokine-biased walk (rung 2) when CytokineToggle.Enabled is on.
    /// Movement is four-neighbour (von Neumann) only.
    ///
    /// Movement/collision simplification (see docs/INTERFACE.md for the
    /// full note): units co-occupy fine tiles with host cells AND with
    /// adhered pathogens -- pathogens do not block unit movement this
    /// sprint, since there's no combat/kill system yet to make blocking
    /// meaningful. "Collision" is instead detected as the unit's current
    /// tile falling in the same COARSE slot as an adhered pathogen, which
    /// triggers a visible contact flash on the pathogen.
    /// </summary>
    public class SearchUnit : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private UnitProfile profile;
        private SpriteRenderer sr;

        public FineCoord Current { get; private set; }

        private Vector3 tickStartWorld;
        private Vector3 tickEndWorld;
        private float tickTimer;

        private const float CytokineBiasStrength = 4f;

        public void Initialize(BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField, UnitProfile profile, FineCoord start)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.profile = profile;
            Current = start;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.color = profile.Color;
            sr.sortingOrder = 10;
            float worldSize = BoardConfig.FineTileWorldSize * Mathf.Max(1, profile.FootprintFineTiles);
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            tickStartWorld = tickEndWorld = board.FineToWorld(Current);
            transform.position = tickStartWorld;
            tickTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds); // desync visual ticks between units
        }

        private void Update()
        {
            if (board == null) return; // not yet initialized this frame

            tickTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tickTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tickStartWorld, tickEndWorld, t);

            if (tickTimer >= BoardConfig.TickIntervalSeconds)
            {
                tickTimer -= BoardConfig.TickIntervalSeconds;
                DoTick();
            }
        }

        private void DoTick()
        {
            tickStartWorld = transform.position;
            for (int i = 0; i < profile.FineTilesPerTick; i++)
            {
                StepOnce();
            }
            tickEndWorld = board.FineToWorld(Current);
            CheckContact();
        }

        private static readonly FineCoord[] candidates = new FineCoord[4];
        private static readonly float[] weights = new float[4];

        private void StepOnce()
        {
            int validCount = 0;
            foreach (var offset in FineCoord.VonNeumannOffsets)
            {
                var candidate = Current.Add(offset);
                if (!board.InFineBounds(candidate)) continue;
                candidates[validCount] = candidate;
                float bias = CytokineToggle.Enabled ? cytokineField.SampleFine(candidate) : 0f;
                weights[validCount] = 1f + CytokineBiasStrength * bias;
                validCount++;
            }

            if (validCount == 0) return; // shouldn't happen on any board >= 2x2 fine tiles

            float totalWeight = 0f;
            for (int i = 0; i < validCount; i++) totalWeight += weights[i];

            float pick = Random.value * totalWeight;
            int chosen = validCount - 1;
            float running = 0f;
            for (int i = 0; i < validCount; i++)
            {
                running += weights[i];
                if (pick <= running) { chosen = i; break; }
            }

            Current = candidates[chosen];
        }

        private void CheckContact()
        {
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            var pathogen = tissueGrid.GetPathogenAt(coarse);
            pathogen?.NotifyContact();
        }
    }
}
