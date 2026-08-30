using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 15 (docs/COMPARTMENT_DESIGN.md §2.1): draws the lumen as an
    /// open fluid channel instead of tinted host-cell quads. A chyme field
    /// quad + a translucent mucus band hugging the gut wall + a pool of
    /// particulate motes drifting down the flow, plus a slow peristaltic
    /// squeeze (Option B -- <see cref="peristalsisAmplitude"/> = 0 reverts to
    /// the static-geometry Option A).
    ///
    /// Cosmetic only. <see cref="Update"/> early-returns while
    /// <see cref="RoundClock.Frozen"/>, exactly like every other Update()
    /// driver -- the channel holds still in the buy phase. Nothing here feeds
    /// the simulation and none of it is headless-testable.
    /// </summary>
    public class LumenChannelRenderer : MonoBehaviour
    {
        // Tint -- COMPARTMENT_DESIGN.md §2.1, palette-checked there.
        private static readonly Color ChymeColor = new Color(0.22f, 0.17f, 0.10f);
        private static readonly Color MucusColor = new Color(0.40f, 0.40f, 0.30f);
        private static readonly Color MoteColor  = new Color(0.42f, 0.34f, 0.22f);

        private const int MoteCount = 40;
        private const float MucusDepthCells = 1.6f;
        private const float MoteBaseSpeed = 1.1f;   // world units / s
        private const float PeristalsisPeriod = 8f; // s

        /// <summary>±fraction the channel cross-section squeezes, and the
        /// in-phase mote-speed boost. 0 = Option A (no squeeze). Kept low --
        /// the squeeze is cosmetic and adhesion still runs on fixed coarse
        /// cells, so above ~0.08 the drawn mucus visibly desyncs from the
        /// pile line.</summary>
        [SerializeField] private float peristalsisAmplitude = 0.06f;

        private Transform chymeQuad;
        private Transform mucusQuad;
        private Vector3 chymeBaseScale;
        private Vector3 mucusBaseScale;

        private PrefabPool motePool;
        private readonly Mote[] motes = new Mote[MoteCount];

        private Rect lumenRect;
        private Vector3 flowDir;     // world-space, from the axis frame
        private Vector3 perpDir;
        private float flowSpan;      // channel length along the flow
        private float halfWidth;     // channel half-extent across the flow
        private float phase;

        private class Mote
        {
            public Transform T;
            public float Speed;
            public float WobblePhase;
            public float WobbleFreq;
        }

        public void Bind(BoardConfig board)
        {
            lumenRect = board.BandWorldRect(BoardBand.Lumen);
            if (lumenRect.width <= 0f || lumenRect.height <= 0f) { enabled = false; return; }

            // Flow direction in world space, derived from the axis frame so
            // no world direction is hardcoded (same discipline as
            // PathogenAgent / BoneMarrowManager.Emit).
            int entryCross = board.LumenEntryCrossIndex;
            int nextCross = Mathf.Clamp(entryCross + board.FlowCrossStep, 0, board.CrossLength - 1);
            Vector3 a = board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, entryCross));
            Vector3 c = board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, nextCross));
            flowDir = (c - a).sqrMagnitude > 1e-6f ? (c - a).normalized : Vector3.down;
            perpDir = new Vector3(-flowDir.y, flowDir.x, 0f);

            var span = new Vector3(lumenRect.width, lumenRect.height, 0f);
            flowSpan = Mathf.Abs(Vector3.Dot(span, flowDir));
            if (flowSpan <= 0f) flowSpan = Mathf.Max(lumenRect.width, lumenRect.height);
            halfWidth = 0.5f * Mathf.Abs(Vector3.Dot(span, perpDir));
            if (halfWidth <= 0f) halfWidth = 0.5f * Mathf.Min(lumenRect.width, lumenRect.height);

            Vector3 center = new Vector3(lumenRect.center.x, lumenRect.center.y, 0f);

            // Chyme field -- fills the band.
            chymeQuad = MakeQuad("LumenChyme", SpriteShapes.ChymeField, ChymeColor, 0);
            chymeQuad.position = center;
            chymeQuad.localScale = chymeBaseScale = new Vector3(lumenRect.width, lumenRect.height, 1f);

            // Mucus band -- a strip along the tissue seam, running the full
            // flow length. The sprite is opaque at its texture-left edge; a
            // negative X scale flips it so the opaque side faces the wall when
            // the wall is on the right.
            var tissueRect = board.BandWorldRect(BoardBand.Tissue);
            bool wallOnLeft = tissueRect.center.x <= lumenRect.center.x;
            float stripDepth = MucusDepthCells * board.CoarseCellWorldSize;
            float wallX = wallOnLeft ? lumenRect.xMin : lumenRect.xMax;
            mucusQuad = MakeQuad("LumenMucus", SpriteShapes.MucusBand, WithAlpha(MucusColor, 0.5f), 1);
            mucusQuad.position = new Vector3(
                wallX + (wallOnLeft ? 1f : -1f) * stripDepth * 0.5f, lumenRect.center.y, 0f);
            mucusQuad.localScale = mucusBaseScale =
                new Vector3(wallOnLeft ? stripDepth : -stripDepth, lumenRect.height, 1f);

            // Flow motes -- pooled, pre-warmed so no runtime Instantiate.
            var template = new GameObject("FlowMoteTemplate");
            template.transform.SetParent(transform, false);
            var tsr = template.AddComponent<SpriteRenderer>();
            tsr.sprite = SpriteShapes.FlowMote;
            tsr.sortingOrder = 2;
            template.SetActive(false);
            var poolGo = new GameObject("FlowMotePool");
            poolGo.transform.SetParent(transform, false);
            motePool = poolGo.AddComponent<PrefabPool>();
            motePool.SetPrefab(template);

            float cell = board.CoarseCellWorldSize;
            for (int i = 0; i < MoteCount; i++)
            {
                var go = motePool.Get();
                go.transform.SetParent(transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                float shade = 1f + Random.Range(-0.08f, 0.08f);
                sr.color = new Color(MoteColor.r * shade, MoteColor.g * shade, MoteColor.b * shade, 1f);
                float s = cell * Random.Range(0.14f, 0.26f);
                go.transform.localScale = new Vector3(s, s, 1f);
                go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                var m = new Mote
                {
                    T = go.transform,
                    Speed = MoteBaseSpeed * Random.Range(0.75f, 1.25f),
                    WobblePhase = Random.Range(0f, Mathf.PI * 2f),
                    WobbleFreq = Random.Range(0.6f, 1.4f),
                };
                motes[i] = m;
                m.T.position = RandomChannelPoint(Random.value);
            }
        }

        private Vector3 RandomChannelPoint(float alongFlow01)
        {
            Vector3 entryCenter = new Vector3(lumenRect.center.x, lumenRect.center.y, 0f)
                                  - flowDir * (flowSpan * 0.5f);
            return entryCenter
                   + flowDir * (alongFlow01 * flowSpan)
                   + perpDir * Random.Range(-halfWidth * 0.9f, halfWidth * 0.9f);
        }

        private void Update()
        {
            if (chymeQuad == null || RoundClock.Frozen) return;
            float dt = Time.deltaTime;
            phase += dt;

            float squeeze = 1f + peristalsisAmplitude * Mathf.Sin(phase * (2f * Mathf.PI / PeristalsisPeriod));

            // Cross-section squeeze on the two field quads.
            var cs = chymeBaseScale; cs.x *= squeeze; chymeQuad.localScale = cs;
            var ms = mucusBaseScale; ms.y *= squeeze; mucusQuad.localScale = ms;

            for (int i = 0; i < motes.Length; i++)
            {
                var m = motes[i];
                if (m == null) continue;
                float wobble = Mathf.Sin(phase * m.WobbleFreq + m.WobblePhase) * 0.35f;
                m.T.position += flowDir * (m.Speed * squeeze * dt) + perpDir * (wobble * dt);

                Vector2 p = m.T.position;
                float along = Vector3.Dot((Vector3)p - (new Vector3(lumenRect.center.x, lumenRect.center.y, 0f)
                                          - flowDir * (flowSpan * 0.5f)), flowDir);
                float across = Mathf.Abs(Vector3.Dot((Vector3)p - new Vector3(lumenRect.center.x, lumenRect.center.y, 0f), perpDir));
                if (along >= flowSpan || across > halfWidth * 1.05f)
                    m.T.position = RandomChannelPoint(0f);
            }
        }

        private Transform MakeQuad(string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
