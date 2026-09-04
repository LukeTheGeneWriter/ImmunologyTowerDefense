using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rounds;
using ImmunologyTD.Units;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 15 (docs/COMPARTMENT_DESIGN.md §2.2): draws the base band as
    /// the bloodstream instead of tinted host-cell quads. A deep-oxblood
    /// plasma field lifting toward a soft endothelial vessel wall at the
    /// tissue seam, erythrocytes streaming in from the outer edge, a
    /// "cell born" puff in the marrow on every real emission, and an acute
    /// red flash where a pathogen crosses into the base (so the §6c
    /// life-loss lands instead of being a silent despawn).
    ///
    /// Cosmetic only. The drifting elements freeze with everything else
    /// while <see cref="RoundClock.Frozen"/>. The two static hooks it
    /// listens on (<see cref="BoneMarrowManager.OnCellEmitted"/>,
    /// <see cref="PathogenAgent.OnReachedBase"/>) are cleared in
    /// <see cref="OnDestroy"/> -- same discipline as
    /// <c>DegranulationFlash.Configure</c>.
    /// </summary>
    public class BaseCompartmentRenderer : MonoBehaviour
    {
        // Tint -- COMPARTMENT_DESIGN.md §2.2. **Sprint 17 (the Director's
        // cartoon pass).** The plasma keeps its oxblood -- the sepsis framing
        // depends on the base reading as blood you do not want anything
        // arriving in -- but the things *in* the vessel brighten. A wall of
        // warm pink cells and saturated corpuscles reads as a living vessel;
        // the muted pair before it read as a diagram of one.
        private static readonly Color PlasmaColor   = new Color(0.33f, 0.11f, 0.15f);
        private static readonly Color WallColor     = new Color(0.78f, 0.50f, 0.52f);
        private static readonly Color ErythroColor  = new Color(0.72f, 0.20f, 0.22f);
        private static readonly Color PuffColor     = new Color(0.70f, 0.62f, 0.50f);
        private static readonly Color BreachColor   = new Color(0.86f, 0.10f, 0.12f);

        private const int ErythrocyteCount = 24;
        private const int BirthPuffCap = 12;
        private const int BreachFlashCap = 6;
        private const float ErythroSpeed = 0.6f;   // world units / s
        private const float PuffSpeed = 0.5f;
        private const float PuffLifetime = 1.5f;
        private const float BreachLifetime = 0.55f;

        private Rect baseRect;
        private Vector3 inwardDir;   // outer edge -> vessel wall, world space
        private Vector3 perpDir;
        private float inwardSpan;
        private float halfWidth;

        private PrefabPool erythroPool;
        private PrefabPool puffPool;
        private PrefabPool breachPool;

        private readonly List<Transform> erythrocytes = new List<Transform>();
        private readonly List<Puff> puffs = new List<Puff>();
        private readonly List<Flash> flashes = new List<Flash>();

        private class Puff { public Transform T; public SpriteRenderer Sr; public float Age; }
        private class Flash { public Transform T; public SpriteRenderer Sr; public float Age; public float StartScale; public float EndScale; }

        public void Bind(BoardConfig board)
        {
            baseRect = board.BandWorldRect(BoardBand.Base);
            if (baseRect.width <= 0f || baseRect.height <= 0f) { enabled = false; return; }

            int mid = board.CrossLength / 2;
            int lastBaseAxis = Mathf.Max(0, board.TissueBaseEdgeAxisIndex - 1);
            Vector3 outer = board.CoarseToWorldCenter(board.CoarseFromAxis(0, mid));
            Vector3 lastBase = board.CoarseToWorldCenter(board.CoarseFromAxis(lastBaseAxis, mid));
            Vector3 firstTissue = board.CoarseToWorldCenter(board.CoarseFromAxis(board.TissueBaseEdgeAxisIndex, mid));
            inwardDir = (lastBase - outer).sqrMagnitude > 1e-6f ? (lastBase - outer).normalized : Vector3.right;
            perpDir = new Vector3(-inwardDir.y, inwardDir.x, 0f);

            var span = new Vector3(baseRect.width, baseRect.height, 0f);
            inwardSpan = Mathf.Abs(Vector3.Dot(span, inwardDir));
            if (inwardSpan <= 0f) inwardSpan = Mathf.Max(baseRect.width, baseRect.height);
            halfWidth = 0.5f * Mathf.Abs(Vector3.Dot(span, perpDir));
            if (halfWidth <= 0f) halfWidth = 0.5f * Mathf.Min(baseRect.width, baseRect.height);

            Vector3 center = new Vector3(baseRect.center.x, baseRect.center.y, 0f);
            bool wallOnRight = firstTissue.x >= center.x;

            // Plasma field -- fills the band, gradient lifting toward the wall.
            var plasma = MakeQuad("BasePlasma", SpriteShapes.PlasmaField, PlasmaColor, 0);
            plasma.position = center;
            plasma.localScale = new Vector3(wallOnRight ? baseRect.width : -baseRect.width, baseRect.height, 1f);

            // Vessel wall -- Sprint 17: a row of endothelial *cells* instead
            // of one stretched bar. Sprint 15 scaled a single quad the whole
            // length of the seam, so any detail drawn into the sprite was
            // stretched with it and the wall could only ever be a smooth
            // line. Tiling keeps each cell's aspect at any board size, which
            // is what lets the boundary read as the thing immune cells
            // squeeze *between*.
            Vector3 seam = (lastBase + firstTissue) * 0.5f;
            BuildVesselWall(board, seam);

            erythroPool = MakePool("Erythrocyte", SpriteShapes.Erythrocyte, 2, 32);
            puffPool = MakePool("BirthPuff", SpriteShapes.BirthPuff, 4, 16);
            breachPool = MakePool("BaseBreachFlash", SpriteShapes.EffeBloom, 3, 8);

            float cell = board.CoarseCellWorldSize;
            for (int i = 0; i < ErythrocyteCount; i++)
            {
                var go = erythroPool.Get();
                go.transform.SetParent(transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                float shade = 1f + Random.Range(-0.1f, 0.1f);
                sr.color = new Color(ErythroColor.r * shade, ErythroColor.g * shade, ErythroColor.b * shade, 0.9f);
                float s = cell * Random.Range(0.18f, 0.30f);
                go.transform.localScale = new Vector3(s, s, 1f);
                go.transform.position = OuterEdgePoint(Random.value);
                erythrocytes.Add(go.transform);
            }

            BoneMarrowManager.OnCellEmitted = HandleCellEmitted;
            PathogenAgent.OnReachedBase = HandleReachedBase;
        }

        private void OnDestroy()
        {
            if (BoneMarrowManager.OnCellEmitted == HandleCellEmitted) BoneMarrowManager.OnCellEmitted = null;
            if (PathogenAgent.OnReachedBase == HandleReachedBase) PathogenAgent.OnReachedBase = null;
        }

        private void HandleCellEmitted(Vector3 slotWorld)
        {
            if (RoundClock.Frozen || puffPool == null || puffs.Count >= BirthPuffCap) return;
            var go = puffPool.Get();
            go.transform.SetParent(transform, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.color = new Color(PuffColor.r, PuffColor.g, PuffColor.b, 1f);
            float s = Random.Range(0.22f, 0.34f);
            go.transform.localScale = new Vector3(s, s, 1f);
            go.transform.position = slotWorld + (Vector3)(Random.insideUnitCircle * 0.18f);
            puffs.Add(new Puff { T = go.transform, Sr = sr, Age = 0f });
        }

        private void HandleReachedBase(Vector3 worldPos)
        {
            if (breachPool == null || flashes.Count >= BreachFlashCap) return;
            var go = breachPool.Get();
            go.transform.SetParent(transform, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.color = new Color(BreachColor.r, BreachColor.g, BreachColor.b, 0.95f);
            float start = baseRect.height * 0.06f;
            go.transform.localScale = new Vector3(start, start, 1f);
            go.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            flashes.Add(new Flash { T = go.transform, Sr = sr, Age = 0f, StartScale = start, EndScale = start * 4.5f });
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Breach flashes always finish -- they are sub-second and fire on
            // a real sim event.
            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                var f = flashes[i];
                f.Age += dt;
                float u = Mathf.Clamp01(f.Age / BreachLifetime);
                float sc = Mathf.Lerp(f.StartScale, f.EndScale, u);
                f.T.localScale = new Vector3(sc, sc, 1f);
                var col = f.Sr.color; col.a = 0.95f * (1f - u); f.Sr.color = col;
                if (f.Age >= BreachLifetime) { breachPool.Release(f.T.gameObject); flashes.RemoveAt(i); }
            }

            if (RoundClock.Frozen) return;

            for (int i = 0; i < erythrocytes.Count; i++)
            {
                var t = erythrocytes[i];
                t.position += inwardDir * (ErythroSpeed * dt);
                float along = Vector3.Dot(t.position - OuterEdgePoint(0f, 0f), inwardDir);
                if (along >= inwardSpan) t.position = OuterEdgePoint(0f);
            }

            for (int i = puffs.Count - 1; i >= 0; i--)
            {
                var p = puffs[i];
                p.Age += dt;
                p.T.position += inwardDir * (PuffSpeed * dt);
                var col = p.Sr.color; col.a = 1f - p.Age / PuffLifetime; p.Sr.color = col;
                if (p.Age >= PuffLifetime) { puffPool.Release(p.T.gameObject); puffs.RemoveAt(i); }
            }
        }

        private Vector3 OuterEdgePoint(float lateral01) =>
            OuterEdgePoint(lateral01, Random.Range(-halfWidth * 0.9f, halfWidth * 0.9f));

        private Vector3 OuterEdgePoint(float alongInward01, float lateralOffset)
        {
            Vector3 outerCenter = new Vector3(baseRect.center.x, baseRect.center.y, 0f)
                                  - inwardDir * (inwardSpan * 0.5f);
            return outerCenter + inwardDir * (alongInward01 * inwardSpan) + perpDir * lateralOffset;
        }

        /// <summary>Tiles <see cref="SpriteShapes.EndothelialCell"/> along the
        /// base/tissue seam. Static geometry, built once: ~11 renderers on
        /// the 25x10 test board, ~44 on the 100x40 Map 01 aspiration. The
        /// alternating size jitter is what stops it reading as a zip.</summary>
        private void BuildVesselWall(BoardConfig board, Vector3 seam)
        {
            float cell = board.CoarseCellWorldSize;
            float thickness = cell * 0.85f;
            float pitch = cell * 0.92f;
            int count = Mathf.Max(1, Mathf.RoundToInt(baseRect.height / pitch));
            float step = baseRect.height / count;

            for (int i = 0; i < count; i++)
            {
                float y = baseRect.yMin + (i + 0.5f) * step;
                var go = new GameObject($"VesselWallCell_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteShapes.EndothelialCell;
                float shade = 1f + Random.Range(-0.06f, 0.06f);
                sr.color = new Color(WallColor.r * shade, WallColor.g * shade, WallColor.b * shade, 1f);
                sr.sortingOrder = 1;
                go.transform.position = new Vector3(seam.x, y, 0f);
                go.transform.localScale = new Vector3(
                    thickness * Random.Range(0.92f, 1.06f), step * 1.06f, 1f);
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

        private PrefabPool MakePool(string label, Sprite sprite, int order, int prewarm)
        {
            var template = new GameObject($"{label}Template");
            template.transform.SetParent(transform, false);
            var sr = template.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            template.SetActive(false);
            var poolGo = new GameObject($"{label}Pool");
            poolGo.transform.SetParent(transform, false);
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            var warm = new GameObject[prewarm];
            for (int i = 0; i < prewarm; i++) warm[i] = pool.Get();
            for (int i = 0; i < prewarm; i++) pool.Release(warm[i]);
            return pool;
        }
    }
}
