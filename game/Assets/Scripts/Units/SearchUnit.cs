using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// A hunting unit performing a random walk on the fine lattice (rung 1
    /// of GAME_DESIGN.md sections 7/9's search ladder), or a
    /// cytokine-biased walk (rung 2) when CytokineToggle.Enabled is on. The
    /// actual per-step choice is delegated to Chemotaxis.ChooseNextStep
    /// (Units/Chemotaxis.cs) -- pulled out to a static method during the
    /// Sprint 1 closing task so it could be verified headlessly, see that
    /// file and Assets/Editor/CytokineVerification.cs. Movement is
    /// four-neighbour (von Neumann) only.
    ///
    /// Movement/collision simplification (see docs/INTERFACE.md for the
    /// full note): units co-occupy fine tiles with host cells AND with
    /// adhered pathogens -- pathogens still do not block unit movement.
    /// "Collision" is detected as the unit's current tile falling in the
    /// same COARSE slot as an adhered pathogen, which as of Sprint 2 deals
    /// real flat contact damage (see CheckContact/PathogenAgent.ReceiveDamage)
    /// on top of the existing visual flash -- clearing an infected/occupied
    /// slot is a real, visible consequence now, not just a flash.
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

        // Reused every StepOnce call so Chemotaxis.ChooseNextStep allocates
        // nothing per step; one buffer pair per unit instance.
        private readonly FineCoord[] candidateBuffer = new FineCoord[4];
        private readonly float[] weightBuffer = new float[4];

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

        private void StepOnce()
        {
            Current = Chemotaxis.ChooseNextStep(
                Current, board, cytokineField, CytokineToggle.Enabled, candidateBuffer, weightBuffer);
        }

        private void CheckContact()
        {
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            var pathogen = tissueGrid.GetPathogenAt(coarse);
            // Sprint 2: contact deals flat damage (GAME_DESIGN.md section 4a
            // -- collateral damage to the whole infected cell for
            // intracellular pathogens, direct damage to the pathogen for a
            // large bacterium; mechanically the same call either way, see
            // PathogenAgent.ReceiveDamage). Fires once per tick a unit's
            // fine tile falls in the pathogen's coarse slot, same detection
            // as Sprint 1's flash-only contact.
            pathogen?.ReceiveDamage(PathogenAgent.ContactDamagePerHit);
        }
    }
}
