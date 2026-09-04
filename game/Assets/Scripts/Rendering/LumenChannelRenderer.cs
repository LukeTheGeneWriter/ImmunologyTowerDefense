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
        // Tint -- COMPARTMENT_DESIGN.md §2.1. **Sprint 17 re-palette (the
        // Director's cartoon pass).** Sprint 15 painted the whole lumen in
        // browns, which put the channel in the same colour family as the
        // contaminated food bolus drifting down it (ochre, lumpy, and
        // deliberately unappetising). Two things that must read as opposites
        // -- living gut, foreign filth -- were reading as one thing.
        //
        // So the channel warms to a mucosal plum, the mucus becomes a pearly
        // sheen instead of a grey-green film, the particulate lightens to a
        // pale cream, and the wall grows villi. The bolus keeps its ochre and
        // is now the only brown thing in the band, which is exactly the
        // contrast an incoming round should have.
        private static readonly Color ChymeColor  = new Color(0.36f, 0.22f, 0.24f);
        private static readonly Color MucusColor  = new Color(0.88f, 0.84f, 0.76f);
        private static readonly Color MoteColor   = new Color(0.60f, 0.52f, 0.40f);
        private static readonly Color VillusColor = new Color(0.80f, 0.50f, 0.47f);

        private const int MoteCount = 28;           // Sprint 17: was 40 -- a cleaner channel
        private const float MucusDepthCells = 1.6f;
        private const float MoteBaseSpeed = 1.1f;   // world units / s
        private const float PeristalsisPeriod = 8f; // s

        /// <summary>Villus geometry, in coarse cells. Height is kept under
        /// the mucus depth on purpose: villi are wall decoration, and a
        /// pathogen adhered at the wall must stay readable in front of them
        /// (it draws at sorting order 20, they draw at 1, so it is only ever
        /// a question of how much they clutter the lane).</summary>
        private const float VillusHeightCells = 1.15f;
        private const float VillusWidthCells = 0.42f;
        private const float VillusSpacingCells = 0.78f;
        private const float VillusSwayDegrees = 5f;

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

        private class Villus
        {
            public Transform T;
            public float BaseAngle;    // degrees; points from the wall into the channel
            public float SwayPhase;
            public float SwayFreq;
            public Vector3 BaseScale;
        }

        private readonly System.Collections.Generic.List<Villus> villi =
            new System.Collections.Generic.List<Villus>();

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
            // Sprint 17: a pearly sheen at 0.20 rather than a 0.50 film, and
            // drawn OVER the villi (order 2) so it glazes them -- that glaze
            // is what makes the wall read as wet and velvety instead of as a
            // row of pink fingers.
            mucusQuad = MakeQuad("LumenMucus", SpriteShapes.MucusBand, WithAlpha(MucusColor, 0.20f), 2);
            mucusQuad.position = new Vector3(
                wallX + (wallOnLeft ? 1f : -1f) * stripDepth * 0.5f, lumenRect.center.y, 0f);
            mucusQuad.localScale = mucusBaseScale =
                new Vector3(wallOnLeft ? stripDepth : -stripDepth, lumenRect.height, 1f);

            BuildVilli(board);

            // Flow motes -- pooled, pre-warmed so no runtime Instantiate.
            var template = new GameObject("FlowMoteTemplate");
            template.transform.SetParent(transform, false);
            var tsr = template.AddComponent<SpriteRenderer>();
            tsr.sprite = SpriteShapes.FlowMote;
            // Sprint 17: particulate drifts in front of the villi (1) and the
            // mucus sheen (2), and above the gut-wall bar (3) so a mote that
            // wanders to the seam is never half-swallowed by it.
            tsr.sortingOrder = 4;
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

        /// <summary>Sprint 17: lines the gut wall with villi.
        ///
        /// One `SpriteRenderer` per villus, built once and never rebuilt --
        /// they are static geometry that only rotates. On the 25x10 test
        /// board that is ~13 renderers; on the 100x40 Map 01 aspiration
        /// ~51. Cheap next to the ~110 per-cell renderers Sprint 15 deleted
        /// from this band, and the reason this is affordable at all.
        ///
        /// Direction is derived from the axis frame, never hardcoded: the
        /// villi grow along the perpendicular *away* from the tissue, so a
        /// map with the lumen on the other side, or a vertical threat axis,
        /// still points them into the channel.</summary>
        private void BuildVilli(BoardConfig board)
        {
            float cell = board.CoarseCellWorldSize;
            Vector3 lumenCenter = new Vector3(lumenRect.center.x, lumenRect.center.y, 0f);

            // Which way is "into the channel"? Toward the lumen centre from
            // the wall -- i.e. the perpendicular that points away from the
            // tissue band.
            var tissueRect = board.BandWorldRect(BoardBand.Tissue);
            Vector3 tissueCenter = new Vector3(tissueRect.center.x, tissueRect.center.y, 0f);
            float side = Vector3.Dot(lumenCenter - tissueCenter, perpDir);
            Vector3 intoChannel = side >= 0f ? perpDir : -perpDir;

            Vector3 wallPoint = lumenCenter - intoChannel * halfWidth;
            Vector3 flowStart = wallPoint - flowDir * (flowSpan * 0.5f);

            float height = VillusHeightCells * cell;
            float width = VillusWidthCells * cell;
            float spacing = VillusSpacingCells * cell;
            int count = Mathf.Max(1, Mathf.FloorToInt(flowSpan / spacing));
            float angle = Mathf.Atan2(intoChannel.y, intoChannel.x) * Mathf.Rad2Deg - 90f;

            for (int i = 0; i < count; i++)
            {
                float along = (i + 0.5f) * (flowSpan / count);
                // Per-villus height jitter so the row is a fringe, not a comb.
                float h = height * Random.Range(0.82f, 1.16f);

                var go = new GameObject($"Villus_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteShapes.Villus;
                float shade = 1f + Random.Range(-0.07f, 0.07f);
                sr.color = new Color(VillusColor.r * shade, VillusColor.g * shade, VillusColor.b * shade, 1f);
                sr.sortingOrder = 1;   // over the chyme, under the mucus sheen

                go.transform.position = flowStart + flowDir * along + intoChannel * (h * 0.5f);
                go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                var scale = new Vector3(width, h, 1f);
                go.transform.localScale = scale;

                villi.Add(new Villus
                {
                    T = go.transform,
                    BaseAngle = angle,
                    SwayPhase = Random.Range(0f, Mathf.PI * 2f),
                    SwayFreq = Random.Range(0.5f, 0.9f),
                    BaseScale = scale,
                });
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

            // Villi sway in the flow, each on its own phase, and lengthen a
            // little as the channel squeezes -- the fringe moves with the
            // peristalsis rather than sitting rigid in front of it.
            for (int i = 0; i < villi.Count; i++)
            {
                var v = villi[i];
                if (v.T == null) continue;
                float s = Mathf.Sin(phase * v.SwayFreq + v.SwayPhase);
                v.T.rotation = Quaternion.Euler(0f, 0f, v.BaseAngle + s * VillusSwayDegrees);
                var vs = v.BaseScale; vs.y *= squeeze; v.T.localScale = vs;
            }

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
