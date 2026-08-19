using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Pathogens
{
    public enum PathogenState { Transiting, Adhered, Cleared }

    /// <summary>
    /// A pathogen that enters at the lumen edge, transits rightward for a
    /// random distance, then adheres to a coarse slot near where it
    /// stopped (SPRINT_PLAN.md: "adhesion + presence is enough" -- full
    /// multi-depth descent toward blood is explicitly out of scope this
    /// sprint). A minority never adhere and transit straight across the
    /// board and exit (GAME_DESIGN.md section 6a's transit/breach
    /// vocabulary, and docs/handoff-map01-intestine.md's "roughly 6
    /// transit, 4 adhere" flavour) -- those get released back to the pool
    /// instead of claiming a coarse slot.
    /// </summary>
    public class PathogenAgent : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private System.Action<PathogenAgent> onExit;

        public PathogenState State { get; private set; }
        public FineCoord Current { get; private set; }

        private int targetFineColumn;
        private int targetRow;
        private bool willAdhere;

        private Vector3 tickStartWorld;
        private Vector3 tickEndWorld;
        private float tickTimer;
        private const int TransitFineTilesPerTick = 2;

        private SpriteRenderer sr;
        private float contactFlashTimer;
        private static readonly Color BaseColor = new Color(0.55f, 0.15f, 0.2f); // muted maroon, reads "foreign"
        private static readonly Color FlashColor = new Color(0.95f, 0.85f, 0.3f);

        public void Initialize(BoardConfig board, TissueGrid tissueGrid, System.Action<PathogenAgent> onExit)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.onExit = onExit;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.color = BaseColor;
            sr.sortingOrder = 20;
            float worldSize = BoardConfig.FineTileWorldSize * 1.5f;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            int entryRow = Random.Range(0, BoardConfig.Rows);
            int entryFineRow = entryRow * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2;
            Current = new FineCoord(0, entryFineRow);

            targetRow = Random.Range(0, BoardConfig.Rows);
            willAdhere = Random.value < 0.7f; // ~70% adhere, ~30% transit straight through and exit
            targetFineColumn = willAdhere ? Random.Range(1, board.FineColumns - 1) : board.FineColumns;

            State = PathogenState.Transiting;
            contactFlashTimer = 0f;
            tickStartWorld = tickEndWorld = board.FineToWorld(Current);
            transform.position = tickStartWorld;
            tickTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds);
        }

        private void Update()
        {
            if (board == null || State != PathogenState.Transiting) return;

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

            for (int i = 0; i < TransitFineTilesPerTick && State == PathogenState.Transiting; i++)
            {
                int nextCol = Current.Column + 1;
                if (nextCol >= board.FineColumns)
                {
                    Exit();
                    return;
                }
                Current = new FineCoord(nextCol, Current.Row);

                if (willAdhere && Current.Column >= targetFineColumn)
                {
                    TryAdhereHere();
                    if (State != PathogenState.Transiting) return;
                }
            }

            tickEndWorld = board.FineToWorld(Current);
        }

        private void TryAdhereHere()
        {
            var coarse = Current.ToCoarse(BoardConfig.FineSubdivision);
            int col = coarse.Column;

            // Prefer the row chosen at spawn; otherwise take the nearest
            // free row in the same column.
            var rowsByPreference = new System.Collections.Generic.List<int> { targetRow };
            for (int dr = 1; dr < BoardConfig.Rows; dr++)
            {
                int up = targetRow - dr;
                int down = targetRow + dr;
                if (up >= 0) rowsByPreference.Add(up);
                if (down < BoardConfig.Rows) rowsByPreference.Add(down);
            }

            foreach (var row in rowsByPreference)
            {
                var candidate = new CoarseCoord(col, row);
                if (tissueGrid.TryAdhere(candidate, this, Time.time))
                {
                    Current = new FineCoord(
                        candidate.Column * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2,
                        candidate.Row * BoardConfig.FineSubdivision + BoardConfig.FineSubdivision / 2);
                    State = PathogenState.Adhered;
                    tickEndWorld = board.FineToWorld(Current);
                    transform.position = tickEndWorld;
                    return;
                }
            }

            // Column is full top to bottom -- keep transiting and retry
            // adhesion one coarse cell further along instead of giving up.
            targetFineColumn = Current.Column + BoardConfig.FineSubdivision;
            if (targetFineColumn >= board.FineColumns)
            {
                willAdhere = false;
                targetFineColumn = board.FineColumns;
            }
        }

        private void Exit()
        {
            State = PathogenState.Cleared;
            onExit?.Invoke(this);
        }

        public void NotifyContact()
        {
            contactFlashTimer = 0.25f;
        }

        private void LateUpdate()
        {
            if (sr == null) return;
            if (contactFlashTimer > 0f)
            {
                contactFlashTimer -= Time.deltaTime;
                sr.color = Color.Lerp(BaseColor, FlashColor, contactFlashTimer / 0.25f);
            }
            else if (sr.color != BaseColor)
            {
                sr.color = BaseColor;
            }
        }

        /// <summary>Called by PathogenSpawner just before returning this
        /// instance to the pool (transit/exit case only -- adhered
        /// pathogens never get released this sprint).</summary>
        public void ResetForPool()
        {
            State = PathogenState.Transiting;
            board = null;
            tissueGrid = null;
            contactFlashTimer = 0f;
        }
    }
}
