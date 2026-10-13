using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Pathogens;
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
        /// <summary>Sprint 19 (LUMEN_REFERENCE.md P2): the back row of the
        /// "velvet pile" -- darker, translucent, so depth reads without
        /// lifting the background value behind pathogens.</summary>
        private static readonly Color BackVillusColor = new Color(0.62f, 0.40f, 0.42f, 0.70f);

        private const int MoteCount = 28;           // Sprint 17: was 40 -- a cleaner channel
        private const float MucusDepthCells = 1.6f;
        private const float MoteBaseSpeed = 1.1f;   // world units / s
        /// <summary>Sprint 19: how far behind the bolus the contraction
        /// ring reaches along the wall, in cells; and how much the villi in
        /// it lengthen and lean downstream at full contraction.</summary>
        private const float RingLengthCells = 2.5f;
        private const float RingStretch = 0.28f;
        private const float RingLeanDegrees = 14f;

        /// <summary>Villus geometry, in coarse cells. Height is kept under
        /// the mucus depth on purpose: villi are wall decoration, and a
        /// pathogen adhered at the wall must stay readable in front of them
        /// (it draws at sorting order 20, they draw at 1, so it is only ever
        /// a question of how much they clutter the lane).</summary>
        // Widened and packed after looking at the first build: at 0.42 cells
        // wide on a 0.78 pitch they read as a row of thorns rather than a
        // velvety fringe. Overlapping neighbours is the point -- the gaps
        // between them were what made them look like spikes.
        private const float VillusHeightCells = 1.05f;
        private const float VillusWidthCells = 0.72f;
        private const float VillusSpacingCells = 0.55f;
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
        private float cellSize;

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
            public float Along;        // Sprint 19: distance along the flow from the entry end
        }

        private readonly System.Collections.Generic.List<Villus> villi =
            new System.Collections.Generic.List<Villus>();

        private Vector3 flowStartWall;   // the wall point at the entry end
        private float leanSign;          // which rotation sense tips a villus downstream
        private float idleClock;         // peristalsis keeps its rhythm between rounds

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
            // Sprint 19: chyme drops to -1 so the back row of villi (0) can
            // sit between it and the front row (1) -- UI_STYLE_GUIDE sorting.
            chymeQuad = MakeQuad("LumenChyme", SpriteShapes.ChymeField, ChymeColor, -1);
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

            cellSize = board.CoarseCellWorldSize;
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

            flowStartWall = flowStart;
            // Rotating a villus counter-clockwise swings its tip toward the
            // CCW perpendicular of intoChannel; downstream is +1 if that
            // perpendicular points along the flow.
            leanSign = Vector3.Dot(new Vector3(-intoChannel.y, intoChannel.x, 0f), flowDir) >= 0f ? 1f : -1f;

            // Sprint 19: the Peyer's patch stretch of wall grows no villi.
            float patchFrom = float.MaxValue, patchTo = float.MinValue;
            var wallFeatures = GutWallFeatures.Current;
            if (wallFeatures != null)
            {
                for (int lane = wallFeatures.PatchCrossStart; lane <= wallFeatures.PatchCrossEnd; lane++)
                {
                    Vector3 p = board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, lane));
                    float d = Vector3.Dot(p - flowStart, flowDir);
                    patchFrom = Mathf.Min(patchFrom, d - cell * 0.5f);
                    patchTo = Mathf.Max(patchTo, d + cell * 0.5f);
                }
            }

            float height = VillusHeightCells * cell;
            float width = VillusWidthCells * cell;
            float spacing = VillusSpacingCells * cell;
            int count = Mathf.Max(1, Mathf.FloorToInt(flowSpan / spacing));
            float angle = Mathf.Atan2(intoChannel.y, intoChannel.x) * Mathf.Rad2Deg - 90f;

            // Back row first (P2): half a spacing offset, ~80% height, darker.
            for (int i = 0; i < count; i++)
            {
                float along = (i + 1f) * (flowSpan / count);
                if (along >= flowSpan) continue;
                if (along > patchFrom - width * 0.5f && along < patchTo + width * 0.5f) continue;
                AddVillus($"VillusBack_{i}", along, height * 0.8f * Random.Range(0.85f, 1.1f), width * 0.95f,
                    BackVillusColor, 0, angle, intoChannel, flowStart, 0.35f, 0.6f);
            }
            for (int i = 0; i < count; i++)
            {
                float along = (i + 0.5f) * (flowSpan / count);
                if (along > patchFrom - width * 0.3f && along < patchTo + width * 0.3f) continue;
                // Per-villus height jitter so the row is a fringe, not a comb.
                AddVillus($"Villus_{i}", along, height * Random.Range(0.82f, 1.16f), width,
                    VillusColor, 1, angle, intoChannel, flowStart, 0.5f, 0.9f);
            }
        }

        private void AddVillus(string name, float along, float h, float width, Color tint, int order,
            float angle, Vector3 intoChannel, Vector3 flowStart, float swayMin, float swayMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Random.value < 0.5f ? SpriteShapes.Villus : SpriteShapes.VillusAlt;
            float shade = 1f + Random.Range(-0.07f, 0.07f);
            sr.color = new Color(tint.r * shade, tint.g * shade, tint.b * shade, tint.a);
            sr.sortingOrder = order;   // back row 0, front row 1; the mucus sheen (2) glazes both

            go.transform.position = flowStart + flowDir * along + intoChannel * (h * 0.5f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            var scale = new Vector3(width, h, 1f);
            go.transform.localScale = scale;

            villi.Add(new Villus
            {
                T = go.transform,
                BaseAngle = angle,
                SwayPhase = Random.Range(0f, Mathf.PI * 2f),
                SwayFreq = Random.Range(swayMin, swayMax),
                BaseScale = scale,
                Along = along,
            });
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

            // Sprint 19: the squeeze follows the SAME surge-and-pause rhythm
            // that moves the bolus (Peristalsis), and while a bolus is in the
            // channel it is read off the bolus's own clock, so the wall
            // visibly contracts as the bolus surges. Between rounds it keeps
            // its rhythm on an idle clock.
            float t;
            if (Peristalsis.BolusActive) { t = Peristalsis.BolusElapsed; idleClock = t; }
            else { idleClock += dt; t = idleClock; }
            float contraction = Peristalsis.Contraction(t);
            float squeeze = 1f - peristalsisAmplitude + 2f * peristalsisAmplitude * contraction;
            float speedFactor = Peristalsis.SpeedFactor(t);

            // The contraction ring sits just BEHIND the bolus.
            float bolusAlong = Peristalsis.BolusActive
                ? Vector3.Dot(Peristalsis.BolusWorldPosition - flowStartWall, flowDir)
                : float.NegativeInfinity;
            float ringLen = RingLengthCells * cellSize;

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
                float behind = bolusAlong - v.Along;
                float ring = behind > 0f && behind < ringLen ? contraction * (1f - behind / ringLen) : 0f;
                v.T.rotation = Quaternion.Euler(0f, 0f,
                    v.BaseAngle + s * VillusSwayDegrees + leanSign * RingLeanDegrees * ring);
                var vs = v.BaseScale; vs.y *= squeeze * (1f + RingStretch * ring); v.T.localScale = vs;
            }

            for (int i = 0; i < motes.Length; i++)
            {
                var m = motes[i];
                if (m == null) continue;
                float wobble = Mathf.Sin(phase * m.WobbleFreq + m.WobblePhase) * 0.35f;
                // Motes surge with the wave too, but only halfway -- they are
                // fluid, not a bolus gripped by the wall.
                m.T.position += flowDir * (m.Speed * Mathf.Lerp(1f, speedFactor, 0.5f) * dt) + perpDir * (wobble * dt);

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
