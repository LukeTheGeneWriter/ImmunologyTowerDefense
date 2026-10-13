using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Rounds;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 19: draws the two gut-wall structures modelled by
    /// <see cref="GutWallFeatures"/> (docs/LUMEN_REFERENCE.md P4, P6).
    ///
    /// **Peyer's patch** -- always present:
    ///  - a low smooth **dome** of epithelium bulging into the lumen where
    ///    the villi stop (order 1, with the villi);
    ///  - three **M cells** on the dome, lavender-grey with a dark basal
    ///    pocket (order 3, over the mucus glaze). Dim while the M-cell
    ///    upgrade is unbought; once bought they brighten, and pulse while
    ///    sampled antigen is waiting for a DC -- the "it is working" cue;
    ///  - the **follicle** under it on the tissue side: a translucent
    ///    lymphocyte-teal haze with a scatter of resting lymphocytes
    ///    (order 2, translucent so the host-cell states beneath still read).
    ///
    /// **Crypts** -- one per shop level, drawn only once bought, so a crypt
    /// on screen always means "you bought this": a U-pocket opening at the
    /// wall and sinking into tissue (order 2), rose Paneth granules at its
    /// base (order 3). Rebuilt only when the crypt set changes.
    ///
    /// Cosmetic. Positions come from the axis frame (the seam between the
    /// tissue's lumen-edge row and the lumen's near-wall row), never a
    /// hardcoded world direction. Not headless-testable; the model it draws
    /// is (Sprint19Verification).
    /// </summary>
    public class GutWallFeatureRenderer : MonoBehaviour
    {
        private static Color DomeColor => Palette.PeyerDome;
        private static Color MCellColor => Palette.MCell;
        private static Color FollicleColor => Palette.Follicle;
        private static Color CryptColor => Palette.CryptPocket;
        private static Color PanethColor => Palette.Paneth;

        private const float DomeHeightCells = 0.55f;   // visible bulge -- under the ±8% squeeze cap's spirit
        private const int MCellCount = 3;

        private BoardConfig board;
        private GutWallFeatures wall;
        private Vector3 intoChannel;
        private float cell;
        private float angle;

        private readonly List<SpriteRenderer> mCells = new List<SpriteRenderer>();
        private readonly List<GameObject> crypts = new List<GameObject>();
        private int builtCryptVersion = -1;
        private float pulse;

        public void Bind(BoardConfig board, GutWallFeatures wall)
        {
            this.board = board;
            this.wall = wall;
            if (board == null || wall == null) { enabled = false; return; }
            cell = board.CoarseCellWorldSize;

            int mid = (wall.PatchCrossStart + wall.PatchCrossEnd) / 2;
            Vector3 t = board.CoarseToWorldCenter(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, mid));
            Vector3 l = board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, mid));
            intoChannel = (l - t).sqrMagnitude > 1e-6f ? (l - t).normalized : Vector3.right;
            angle = Mathf.Atan2(intoChannel.y, intoChannel.x) * Mathf.Rad2Deg - 90f;

            BuildPatch();
        }

        /// <summary>World point on the wall seam at a (possibly fractional) cross lane.</summary>
        private Vector3 Seam(float cross)
        {
            int c0 = Mathf.Clamp(Mathf.FloorToInt(cross), 0, board.CrossLength - 1);
            int c1 = Mathf.Clamp(c0 + 1, 0, board.CrossLength - 1);
            float f = cross - Mathf.Floor(cross);
            Vector3 a = 0.5f * (board.CoarseToWorldCenter(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, c0))
                              + board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, c0)));
            Vector3 b = 0.5f * (board.CoarseToWorldCenter(board.CoarseFromAxis(board.TissueLumenEdgeAxisIndex, c1))
                              + board.CoarseToWorldCenter(board.CoarseFromAxis(board.LumenNearWallAxisIndex, c1)));
            return Vector3.Lerp(a, b, f);
        }

        private void BuildPatch()
        {
            float centre = 0.5f * (wall.PatchCrossStart + wall.PatchCrossEnd);
            int lanes = wall.PatchCrossEnd - wall.PatchCrossStart + 1;
            float width = lanes * cell;
            Vector3 seam = Seam(centre);
            Vector3 along = (Seam(centre + 1f) - seam);
            along = along.sqrMagnitude > 1e-6f ? along.normalized : new Vector3(-intoChannel.y, intoChannel.x, 0f);

            // Dome: the sprite's hump fills the bottom 28/64 of its quad, base
            // at the bottom edge -- size the quad so the hump is DomeHeightCells.
            float quadH = DomeHeightCells * cell * (64f / 28f);
            var dome = MakeSprite("PeyerDome", SpriteShapes.PeyerDome, DomeColor, 1);
            dome.transform.position = seam + intoChannel * (quadH * 0.5f);
            dome.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            dome.transform.localScale = new Vector3(width, quadH, 1f);

            // M cells sit on the dome's surface, following its curve.
            float halfW = width * 0.5f;
            float domeH = DomeHeightCells * cell;
            float mSize = cell * 0.34f;
            for (int i = 0; i < MCellCount; i++)
            {
                float u = (i - (MCellCount - 1) * 0.5f) / Mathf.Max(1, MCellCount - 1) * 0.55f;  // -0.55..0.55 of half width
                float x = u * halfW;
                float h = domeH * Mathf.Sqrt(Mathf.Max(0f, 1f - u * u)) - mSize * 0.35f;
                var m = MakeSprite($"MCell_{i}", SpriteShapes.MCell, MCellColor, 3);
                m.transform.position = seam + along * x + intoChannel * h;
                // Tilt each to the dome's surface normal.
                float tilt = Mathf.Atan2(u, Mathf.Sqrt(Mathf.Max(0.01f, 1f - u * u))) * Mathf.Rad2Deg * 0.6f;
                float sideSign = Mathf.Sign(Vector3.Cross(intoChannel, along).z);
                m.transform.rotation = Quaternion.Euler(0f, 0f, angle - sideSign * tilt);
                m.transform.localScale = new Vector3(mSize, mSize, 1f);
                mCells.Add(m);
            }

            // Follicle: a translucent teal haze in the tissue under the dome,
            // reaching FollicleDepthCells deep, plus a few resting lymphocytes.
            float depth = GutWallTuning.FollicleDepthCells * cell;
            Vector3 fc = seam - intoChannel * (depth * 0.5f);
            var haze = MakeSprite("Follicle", SpriteShapes.NodeColocGlow, WithAlpha(FollicleColor, Palette.FollicleAlpha), 2);
            haze.transform.position = fc;
            haze.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            haze.transform.localScale = new Vector3(width * 1.15f, depth * 1.2f, 1f);

            var rng = new System.Random(1919);   // fixed scatter: a landmark, not noise
            for (int i = 0; i < 7; i++)
            {
                float ax = ((float)rng.NextDouble() - 0.5f) * width * 0.8f;
                float ad = (float)rng.NextDouble() * depth * 0.85f + cell * 0.1f;
                var lym = MakeSprite($"FollicleLymphocyte_{i}", SpriteShapes.Lymphocyte, WithAlpha(FollicleColor, 0.55f), 2);
                lym.transform.position = seam + along * ax - intoChannel * ad;
                float s = cell * 0.26f;
                lym.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        private void RebuildCrypts()
        {
            builtCryptVersion = wall.CryptVersion;
            var lanes = wall.CryptLanes;

            // Reuse what exists; add or hide the difference.
            while (crypts.Count < lanes.Count) crypts.Add(BuildCrypt());
            for (int i = 0; i < crypts.Count; i++)
            {
                bool on = i < lanes.Count;
                crypts[i].SetActive(on);
                if (!on) continue;
                Vector3 seam = Seam(lanes[i]);
                float quadH = cell * 0.78f;
                crypts[i].transform.position = seam - intoChannel * (quadH * 0.5f);
                crypts[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private GameObject BuildCrypt()
        {
            var root = new GameObject("Crypt");
            root.transform.SetParent(transform, false);
            var pocket = MakeSprite("CryptPocket", SpriteShapes.Crypt, CryptColor, 2);
            pocket.transform.SetParent(root.transform, false);
            pocket.transform.localScale = new Vector3(cell * 0.75f, cell * 0.78f, 1f);
            var gran = MakeSprite("PanethGranules", SpriteShapes.GranuleCluster, PanethColor, 3);
            gran.transform.SetParent(root.transform, false);
            // The U's floor sits ~22/64 up the quad; granules just above it.
            gran.transform.localPosition = new Vector3(0f, cell * 0.78f * ((24f - 32f) / 64f), 0f);
            float g = cell * 0.24f;
            gran.transform.localScale = new Vector3(g, g, 1f);
            return root;
        }

        private void Update()
        {
            if (wall == null) return;
            if (wall.CryptVersion != builtCryptVersion) RebuildCrypts();   // a purchase can land in the frozen buy phase

            // Tint updates even while frozen, so an M-cell purchase in the buy
            // phase lights the dome at once; only the pulse waits for play.
            if (!RoundClock.Frozen) pulse += Time.deltaTime;

            // M cells: dormant (dim) until bought; bright once bought; a slow
            // pulse while antigen waits in the dome for a DC.
            float k;
            if (wall.MCellLevel <= 0) k = 0.55f;
            else if (wall.PendingAntigen > 0) k = 0.85f + 0.15f * Mathf.Sin(pulse * 5f);
            else k = 0.95f;
            // "Unbought" is half-transparent rather than darkened -- on a
            // light ground, darker reads as louder (Sprint 20).
            var c = new Color(MCellColor.r, MCellColor.g, MCellColor.b,
                wall.MCellLevel <= 0 ? 0.5f : Mathf.Lerp(0.75f, 1f, k));
            for (int i = 0; i < mCells.Count; i++) mCells[i].color = c;
        }

        private SpriteRenderer MakeSprite(string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
