using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 15 (docs/COMPARTMENT_DESIGN.md §2.3): makes the lymph node's
    /// co-localisation field visible. One soft glow quad whose position
    /// tracks the value-weighted centroid of <see cref="LymphNode.Coloc"/>
    /// and whose alpha rises with the field's peak -- so the haze sits where
    /// the helper-T cells have gathered and a dendritic cell is visibly
    /// drifting toward the light (the §5c "second cytokine is load-bearing,
    /// not decoration" made legible).
    ///
    /// Cosmetic only; re-samples every ~0.15 s; holds still while
    /// <see cref="RoundClock.Frozen"/>. Not headless-testable.
    /// </summary>
    public class LymphNodeFieldRenderer : MonoBehaviour
    {
        private static readonly Color GlowColor = new Color(0.55f, 0.85f, 0.85f); // cool cyan-white
        private const float MaxAlpha = 0.35f;
        private const float RefreshInterval = 0.15f;

        private LymphNode node;
        private SpriteRenderer sr;
        private Rect rect;
        private float referenceValue;
        private float timer;

        public void Bind(LymphNode node)
        {
            this.node = node;
            rect = node.WorldRect;

            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteShapes.NodeColocGlow;
            sr.sortingOrder = 3;
            sr.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, 0f);

            transform.position = new Vector3(rect.center.x, rect.center.y, 0f);
            transform.localScale = new Vector3(rect.width * 0.72f, rect.height * 0.72f, 1f);

            // Baseline is the fixed central source alone; the haze lifts from
            // there as resident lymphocytes add their own weak sources.
            referenceValue = AdaptiveTuning.NodeColocalisationSourceStrength
                             + 4f * AdaptiveTuning.NodeLymphocyteSourceStrength;
            Refresh();
        }

        private void Update()
        {
            if (node == null || RoundClock.Frozen) return;
            timer += Time.deltaTime;
            if (timer < RefreshInterval) return;
            timer = 0f;
            Refresh();
        }

        private void Refresh()
        {
            var nb = node.NodeBoard;
            float total = 0f, sx = 0f, sy = 0f, peak = 0f;
            for (int c = 0; c < nb.Columns; c++)
            {
                for (int r = 0; r < nb.Rows; r++)
                {
                    float v = node.Coloc.CoarseValueAt(new CoarseCoord(c, r));
                    total += v;
                    sx += v * (c + 0.5f);
                    sy += v * (r + 0.5f);
                    if (v > peak) peak = v;
                }
            }

            if (total > 0.001f)
            {
                float u = sx / total / nb.Columns;   // 0..1 across
                float w = sy / total / nb.Rows;      // 0..1 down
                transform.position = new Vector3(
                    rect.xMin + u * rect.width,
                    rect.yMax - w * rect.height, 0f);
            }

            float a = referenceValue > 0f ? Mathf.Clamp01(peak / referenceValue) * MaxAlpha : 0f;
            var col = sr.color;
            col.a = a;
            sr.color = col;
        }
    }
}
