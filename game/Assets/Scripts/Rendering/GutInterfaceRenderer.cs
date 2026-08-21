using UnityEngine;
using ImmunologyTD.Grid;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Draws the gut wall: one bar per boundary position, brightening and
    /// thickening as pathogens pile up there, plus a burst flash at the
    /// moment a position breaches.
    ///
    /// This exists because of SPRINT_PLAN.md item 6, which is blunt about
    /// it: "pressure must *visibly* build at a position and then break...
    /// Make an accumulating boundary position visibly distinct from an empty
    /// one, so the player can see danger forming." The mechanic is worthless
    /// if the player cannot read it, and a pile of pathogens alone is not
    /// enough at 28px per cell.
    ///
    /// Two cues, on purpose:
    ///  - **the wall itself** thickens and heats up per position, which is
    ///    readable at a glance across all 40 lanes;
    ///  - **the pathogens themselves** stack outward into the channel
    ///    (PathogenAgent.InterfaceStackWorldPosition), which gives the exact
    ///    count when you look closely.
    ///
    /// Intensity is normalized against a FIXED reference count, not against
    /// the current peak: a lone pathogen must not look like a crisis just
    /// because nothing worse exists yet.
    /// </summary>
    public class GutInterfaceRenderer : MonoBehaviour
    {
        /// <summary>Adhered count at which a position is drawn at full
        /// alarm. A judgment call: with InvasionTuning's defaults a position
        /// holding this many breaches within ~9 seconds, so "fully hot"
        /// genuinely means "about to go."</summary>
        public static int FullAlarmCount = 8;

        private static readonly Color WallColor = new Color(0.55f, 0.47f, 0.40f); // quiet epithelium
        private static readonly Color AlarmColor = new Color(0.95f, 0.30f, 0.20f); // colonised, about to fail

        private BoardConfig board;
        private GutInterface gutInterface;
        private SpriteRenderer[] bars;
        private float refreshTimer;

        public void Bind(BoardConfig board, GutInterface gutInterface)
        {
            this.board = board;
            this.gutInterface = gutInterface;

            var container = new GameObject("GutInterfaceBars").transform;
            container.SetParent(transform, false);

            bars = new SpriteRenderer[gutInterface.PositionCount];
            for (int position = 0; position < bars.Length; position++)
            {
                var go = new GameObject($"GutWall_{position}");
                go.transform.SetParent(container, false);
                go.transform.position = board.InterfaceWorldCenter(position);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSprites.SquareSprite;
                sr.sortingOrder = 3; // above the cell quads (0), below pathogens (20)
                bars[position] = sr;
            }

            gutInterface.Breached += OnBreached;
            Refresh();
        }

        private void OnDestroy()
        {
            if (gutInterface != null) gutInterface.Breached -= OnBreached;
        }

        private void OnBreached(int position, int released)
        {
            if (board == null) return;
            // Scale the burst with how much came through, so a ten-pathogen
            // rupture is unmistakably bigger than a two-pathogen one.
            float size = board.CoarseCellWorldSize * (1.2f + 0.25f * Mathf.Min(released, 10));
            DegranulationFlash.Play(board.InterfaceWorldCenter(position), size, DegranulationFlash.BreachBurstColor);
        }

        private void Update()
        {
            if (board == null) return;
            refreshTimer += Time.deltaTime;
            if (refreshTimer < 0.15f) return;
            refreshTimer = 0f;
            Refresh();
        }

        private void Refresh()
        {
            float cell = board.CoarseCellWorldSize;
            bool horizontalThreat = board.ThreatAxis == BoardAxis.Horizontal;

            for (int position = 0; position < bars.Length; position++)
            {
                int count = gutInterface.AdheredCountAt(position);
                float t = Mathf.Clamp01(count / (float)Mathf.Max(1, FullAlarmCount));

                // Thickness runs ACROSS the threat axis; length runs along
                // the lane. Derived from ThreatAxis rather than assumed, so
                // this still draws correctly on a vertically-banded map.
                float thickness = cell * Mathf.Lerp(0.16f, 0.85f, t);
                float length = cell;
                bars[position].transform.localScale = horizontalThreat
                    ? new Vector3(thickness, length, 1f)
                    : new Vector3(length, thickness, 1f);
                bars[position].color = Color.Lerp(WallColor, AlarmColor, t);
            }
        }
    }
}
