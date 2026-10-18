using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Draws the gut wall: the epithelium heating where pathogens crowd, a
    /// **blister** swelling on every charging plaque, and a burst flash at
    /// the moment one ruptures.
    ///
    /// **Sprint 22 rewrote this** (docs/INVASION_REWORK_SPEC.md). It used to
    /// draw one bar per lane, scaled by that lane's pile. Both halves of that
    /// are gone: there are no piles, and a lane is far too coarse a unit --
    /// on this board one lane is a tenth of the whole wall.
    ///
    /// The requirement it still serves is the original one, and it got
    /// stricter rather than looser: pressure must VISIBLY build at a spot and
    /// then break, so the player can see danger forming and decide to answer
    /// it. What changed is that the drawing is now honest. The old bar showed
    /// a proxy for a hidden memoryless roll -- a "hot" lane might go in one
    /// second or in thirty, and nothing was accumulating. **The blister's
    /// height and colour ARE the plaque's charge**, so what the player reads
    /// is the actual state of the simulation, and an intervention that drains
    /// a plaque visibly deflates it.
    ///
    /// Three cues, strongest first:
    ///  - **the blister** on a charging plaque -- height and colour are the
    ///    charge, and it pulses in its last 15%;
    ///  - **the wall's heat**, sampled from the crowd field, which reads at a
    ///    glance along the whole seam and shows where mucus is being stripped;
    ///  - **the knot of pathogens itself**, milling, which gives the exact
    ///    count when you look closely.
    ///
    /// Heat is normalised against a FIXED reference, never against the
    /// current peak: a lone adherent must not look like a crisis just because
    /// nothing worse exists yet.
    /// </summary>
    public class GutWallRenderer : MonoBehaviour
    {
        /// <summary>Crowd value at which a stretch of wall is drawn at full
        /// heat. Set just above the plaque seeding threshold so "fully hot"
        /// genuinely means "a plaque is forming here."</summary>
        public static float FullAlarmCrowd = 3.5f;

        /// <summary>Fraction of charge above which the blister pulses -- the
        /// tell that a rupture is imminent. Honest about "nearly due" without
        /// promising "exactly when," which the threshold jitter withholds.
        /// </summary>
        public const float TellFraction = 0.85f;

        private static Color WallColor => Palette.GutWallQuiet;
        private static Color AlarmColor => Palette.GutWallAlarm;

        private BoardConfig board;
        private GutWall wall;
        private SpriteRenderer[] bars;
        private SpriteRenderer[] blisters;
        private float refreshTimer;

        public void Bind(BoardConfig board, GutWall wall)
        {
            this.board = board;
            this.wall = wall;

            var barContainer = new GameObject("GutWallSegments").transform;
            barContainer.SetParent(transform, false);

            bars = new SpriteRenderer[board.CrossLength];
            for (int lane = 0; lane < bars.Length; lane++)
            {
                var go = new GameObject($"GutWall_{lane}");
                go.transform.SetParent(barContainer, false);
                go.transform.position = board.InterfaceWorldCenter(lane);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteShapes.EpithelialBar;
                sr.sortingOrder = 3; // above the cell quads (0), below pathogens (20)
                bars[lane] = sr;
            }

            // One pooled blister per plaque slot, hidden until used --
            // nothing is allocated when a plaque seeds (CLAUDE.md's pooling
            // rule applies to effects, not only to agents).
            var blisterContainer = new GameObject("GutWallPlaques").transform;
            blisterContainer.SetParent(transform, false);

            blisters = new SpriteRenderer[wall.PlaqueCapacity];
            for (int i = 0; i < blisters.Length; i++)
            {
                var go = new GameObject($"Plaque_{i}");
                go.transform.SetParent(blisterContainer, false);
                var sr = go.AddComponent<SpriteRenderer>();
                // Placeholder shape: the dedicated blister sprite is specified
                // in INVASION_REWORK_SPEC.md section 7.2 and deferred to this
                // sprint's sprite pass, which checks every new sprite magnified
                // before a build is spent.
                sr.sprite = SpriteShapes.EffeBloom;
                sr.color = AlarmColor;
                sr.sortingOrder = 4; // above the wall, below the pathogens on it
                sr.enabled = false;
                blisters[i] = sr;
            }

            wall.Ruptured += OnRuptured;
            Refresh();
        }

        private void OnDestroy()
        {
            if (wall != null) wall.Ruptured -= OnRuptured;
        }

        private void OnRuptured(float centreS, int released, float footprintCells)
        {
            if (board == null || wall == null) return;
            // Scale the burst with how much came through, so a ten-pathogen
            // rupture is unmistakably bigger than a three-pathogen one.
            float size = board.CoarseCellWorldSize * (1.2f + 0.25f * Mathf.Min(released, 10));
            DegranulationFlash.Play(wall.WallToWorld(centreS, 0f), size, DegranulationFlash.BreachBurstColor);
        }

        private void Update()
        {
            if (board == null) return;

            // Blisters every frame: a charge the player is watching fill must
            // not step. The wall's heat is a slower read and stays on the
            // cheap refresh.
            RefreshBlisters();

            refreshTimer += Time.deltaTime;
            if (refreshTimer < 0.15f) return;
            refreshTimer = 0f;
            Refresh();
        }

        private void Refresh()
        {
            float cell = board.CoarseCellWorldSize;
            bool horizontalThreat = board.ThreatAxis == BoardAxis.Horizontal;

            for (int lane = 0; lane < bars.Length; lane++)
            {
                float crowd = wall.CrowdAt(wall.SOfLane(lane));
                float t = Mathf.Clamp01(crowd / Mathf.Max(0.01f, FullAlarmCrowd));

                // Thickness runs ACROSS the threat axis; length runs along
                // the lane. Derived from ThreatAxis rather than assumed, so
                // this still draws correctly on a vertically-banded map.
                float thickness = cell * Mathf.Lerp(0.16f, 0.85f, t);
                float length = cell;
                bars[lane].transform.localScale = horizontalThreat
                    ? new Vector3(thickness, length, 1f)
                    : new Vector3(length, thickness, 1f);
                bars[lane].color = Color.Lerp(WallColor, AlarmColor, t);
            }
        }

        private void RefreshBlisters()
        {
            float cell = board.CoarseCellWorldSize;
            float time = ImmunologyTD.Rounds.RoundClock.Time;

            for (int i = 0; i < blisters.Length; i++)
            {
                var plaque = wall.PlaqueAt(i);
                if (!plaque.Live || plaque.Charge <= 0.001f)
                {
                    blisters[i].enabled = false;
                    continue;
                }

                float fill = Mathf.Clamp01(plaque.Charge / Mathf.Max(0.01f, plaque.Threshold));

                // The blister bulges OUT of the wall into the channel, so it
                // reads as the wall giving way rather than as a marker
                // floating on top of it.
                float bulge = Mathf.Lerp(0.10f, 0.55f, fill);
                var position = wall.WallToWorld(plaque.CentreS, bulge * 0.5f);

                float pulse = 1f;
                if (fill >= TellFraction)
                {
                    // 3 Hz in the last 15%: the tell. Deliberately a size
                    // pulse rather than a colour flash, because colour is
                    // already carrying the charge.
                    pulse = 1f + 0.08f * Mathf.Sin(time * Mathf.PI * 6f);
                }

                float along = InvasionTuning.BurstFootprintCells * cell * 0.9f;
                float out_ = bulge * cell * 2f * pulse;
                bool horizontalThreat = board.ThreatAxis == BoardAxis.Horizontal;

                blisters[i].enabled = true;
                blisters[i].transform.position = position;
                blisters[i].transform.localScale = horizontalThreat
                    ? new Vector3(out_, along, 1f)
                    : new Vector3(along, out_, 1f);
                blisters[i].color = Color.Lerp(
                    new Color(AlarmColor.r, AlarmColor.g, AlarmColor.b, 0.45f),
                    AlarmColor,
                    fill);
            }
        }
    }
}
