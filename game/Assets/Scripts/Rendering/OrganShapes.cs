using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 21 (docs/COSMETIC_PASS_SPEC.md §1): the bone marrow and the
    /// lymph node, drawn as textbook-cartoon organs.
    ///
    /// They are ~214x453 px and ~363x221 px on screen, so a 64 px
    /// SpriteShapes square stretched to that size blurs and its outline goes
    /// uneven. This draws on a canvas at each organ's own aspect
    /// (<see cref="Canvas"/>), with the same conventions as SpriteShapes --
    /// white RGB, silhouette in alpha, detail in value, one hue per
    /// renderer, outlines via OuterStroke -- and returns one sprite per
    /// layer. Built once at boot (static single instances; ~1.3 MB total).
    /// </summary>
    public static class OrganShapes
    {
        // ------------------------------------------------------------------
        // Canvas: an arbitrary-size raster, y-UP like Texture2D
        // ------------------------------------------------------------------

        public sealed class Canvas
        {
            public readonly int W, H;
            public readonly Color[] Buf;

            public Canvas(int w, int h)
            {
                W = w; H = h;
                Buf = new Color[w * h];
                for (int i = 0; i < Buf.Length; i++) Buf[i] = new Color(1f, 1f, 1f, 0f);
            }

            /// <summary>2x2 supersampled coverage of pixel (x,y).</summary>
            private static float Cov(int x, int y, System.Func<float, float, bool> inside)
            {
                int n = 0;
                if (inside(x + 0.25f, y + 0.25f)) n++;
                if (inside(x + 0.75f, y + 0.25f)) n++;
                if (inside(x + 0.25f, y + 0.75f)) n++;
                if (inside(x + 0.75f, y + 0.75f)) n++;
                return n * 0.25f;
            }

            private void Box(float x0, float y0, float x1, float y1, System.Action<int, int, int> body)
            {
                int a = Mathf.Max(0, Mathf.FloorToInt(x0)), b = Mathf.Min(W - 1, Mathf.CeilToInt(x1));
                int c = Mathf.Max(0, Mathf.FloorToInt(y0)), d = Mathf.Min(H - 1, Mathf.CeilToInt(y1));
                for (int y = c; y <= d; y++)
                    for (int x = a; x <= b; x++)
                        body(x, y, y * W + x);
            }

            public static float RoundedDist(float px, float py, float cx, float cy, float hw, float hh, float r)
            {
                float qx = Mathf.Abs(px - cx) - (hw - r), qy = Mathf.Abs(py - cy) - (hh - r);
                float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
                return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            }

            public void FillRounded(float cx, float cy, float hw, float hh, float r) =>
                Box(cx - hw - 1, cy - hh - 1, cx + hw + 1, cy + hh + 1, (x, y, i) =>
                    Blend(i, Cov(x, y, (fx, fy) => RoundedDist(fx, fy, cx, cy, hw, hh, r) <= 0f)));

            public void ClearRounded(float cx, float cy, float hw, float hh, float r) =>
                Box(cx - hw - 1, cy - hh - 1, cx + hw + 1, cy + hh + 1, (x, y, i) =>
                    Buf[i].a *= 1f - Cov(x, y, (fx, fy) => RoundedDist(fx, fy, cx, cy, hw, hh, r) <= 0f));

            public void ShadeRounded(float cx, float cy, float hw, float hh, float r, float value) =>
                Box(cx - hw - 1, cy - hh - 1, cx + hw + 1, cy + hh + 1, (x, y, i) =>
                {
                    if (Buf[i].a <= 0f) return;
                    if (RoundedDist(x + 0.5f, y + 0.5f, cx, cy, hw, hh, r) <= 0f) SetValue(i, value);
                });

            public void FillEllipse(float cx, float cy, float rx, float ry) =>
                Box(cx - rx - 1, cy - ry - 1, cx + rx + 1, cy + ry + 1, (x, y, i) =>
                    Blend(i, Cov(x, y, (fx, fy) => Sq((fx - cx) / rx) + Sq((fy - cy) / ry) <= 1f)));

            public void ClearEllipse(float cx, float cy, float rx, float ry) =>
                Box(cx - rx - 1, cy - ry - 1, cx + rx + 1, cy + ry + 1, (x, y, i) =>
                    Buf[i].a *= 1f - Cov(x, y, (fx, fy) => Sq((fx - cx) / rx) + Sq((fy - cy) / ry) <= 1f));

            public void ShadeEllipse(float cx, float cy, float rx, float ry, float value) =>
                Box(cx - rx - 1, cy - ry - 1, cx + rx + 1, cy + ry + 1, (x, y, i) =>
                {
                    if (Buf[i].a <= 0f) return;
                    if (Sq((x + 0.5f - cx) / rx) + Sq((y + 0.5f - cy) / ry) <= 1f) SetValue(i, value);
                });

            public void FillCapsule(float ax, float ay, float bx, float by, float hw) =>
                Box(Mathf.Min(ax, bx) - hw - 1, Mathf.Min(ay, by) - hw - 1, Mathf.Max(ax, bx) + hw + 1, Mathf.Max(ay, by) + hw + 1,
                    (x, y, i) => Blend(i, Cov(x, y, (fx, fy) => SegDist(fx, fy, ax, ay, bx, by) <= hw)));

            public void ShadeCapsule(float ax, float ay, float bx, float by, float hw, float value) =>
                Box(Mathf.Min(ax, bx) - hw - 1, Mathf.Min(ay, by) - hw - 1, Mathf.Max(ax, bx) + hw + 1, Mathf.Max(ay, by) + hw + 1,
                    (x, y, i) =>
                    {
                        if (Buf[i].a <= 0f) return;
                        if (SegDist(x + 0.5f, y + 0.5f, ax, ay, bx, by) <= hw) SetValue(i, value);
                    });

            /// <summary>Multiply alpha by another canvas's alpha (clip to a mask).</summary>
            public void ClipTo(Canvas mask)
            {
                for (int i = 0; i < Buf.Length; i++) Buf[i].a *= mask.Buf[i].a;
            }

            /// <summary>Darken opaque pixels within <paramref name="px"/> of a
            /// transparent one to <paramref name="value"/> (an inner ring).</summary>
            public void InnerRing(int px, float value)
            {
                var src = (Color[])Buf.Clone();
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int i = y * W + x;
                        if (src[i].a < 0.5f) continue;
                        bool edge = false;
                        for (int dy = -px; dy <= px && !edge; dy++)
                            for (int dx = -px; dx <= px; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;   // canvas edge is not a hole
                                if (src[ny * W + nx].a < 0.5f) { edge = true; break; }
                            }
                        if (edge) SetValue(i, Mathf.Min(Buf[i].r, value));
                    }
            }

            /// <summary>Dilate outward and fill the ring at
            /// <paramref name="value"/> -- the same coloured-line-art outline
            /// as SpriteShapes.OuterStroke, at any canvas size. Also inks the
            /// inside edge of every hole.</summary>
            public void OuterStroke(float px, float value)
            {
                var src = (Color[])Buf.Clone();
                int reach = Mathf.CeilToInt(px) + 1;
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int i = y * W + x;
                        if (src[i].a >= 0.5f) continue;
                        float near = float.MaxValue;
                        for (int dy = -reach; dy <= reach; dy++)
                        {
                            int ny = y + dy;
                            if (ny < 0 || ny >= H) continue;
                            for (int dx = -reach; dx <= reach; dx++)
                            {
                                int nx = x + dx;
                                if (nx < 0 || nx >= W || src[ny * W + nx].a < 0.5f) continue;
                                float d = dx * dx + dy * dy;
                                if (d < near) near = d;
                            }
                        }
                        if (near == float.MaxValue) continue;
                        float a = Mathf.Clamp01(px + 1f - Mathf.Sqrt(near));
                        if (a <= 0f) continue;
                        Buf[i] = new Color(value, value, value, Mathf.Max(a, src[i].a));
                    }
            }

            public Sprite ToSprite(float worldWidth)
            {
                var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                tex.SetPixels(Buf);
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), W / Mathf.Max(0.01f, worldWidth));
            }

            private void Blend(int i, float a) { if (a > Buf[i].a) Buf[i].a = a; }
            private void SetValue(int i, float v) { Buf[i].r = v; Buf[i].g = v; Buf[i].b = v; }
            private static float Sq(float v) => v * v;

            private static float SegDist(float px, float py, float ax, float ay, float bx, float by)
            {
                float dx = bx - ax, dy = by - ay, len2 = dx * dx + dy * dy;
                float t = len2 <= 0f ? 0f : Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / len2);
                float qx = ax + t * dx - px, qy = ay + t * dy - py;
                return Mathf.Sqrt(qx * qx + qy * qy);
            }
        }

        // ------------------------------------------------------------------
        // Bone marrow (spec §1.2): spongy bone with build-pad cavities
        // ------------------------------------------------------------------

        public struct Marrow
        {
            public Sprite Shadow, Space, Bone;
        }

        /// <summary>Layers for a marrow block <paramref name="worldSize"/>
        /// with pads at <paramref name="padCentersLocal"/> (world offsets from
        /// the block centre) of side <paramref name="padWorld"/>.</summary>
        public static Marrow BuildMarrow(Vector2 worldSize, Vector2[] padCentersLocal, float padWorld, float pitchWorld = 0f)
        {
            int W = 192;
            int H = Mathf.Max(32, Mathf.RoundToInt(W * worldSize.y / worldSize.x));
            float ppw = W / worldSize.x;                      // canvas px per world unit
            float stroke = 0.022f * W;
            float margin = stroke + 1f;                       // room for the outline inside the canvas
            float cx = W * 0.5f, cy = H * 0.5f;
            float hw = W * 0.5f - margin, hh = H * 0.5f - margin;
            float corner = 0.30f * W * 0.5f;
            float t = 0.07f * W;

            // Shadow: the outer silhouette.
            var shadow = new Canvas(W, H);
            shadow.FillRounded(cx, cy, hw, hh, corner);

            // Marrow space: salmon under the bone, with haematopoietic dots
            // (they only show through the holes, which is where they belong).
            var space = new Canvas(W, H);
            space.FillRounded(cx, cy, hw - t * 0.5f, hh - t * 0.5f, corner * 0.8f);
            var rng = new System.Random(3);
            int dots = Mathf.RoundToInt(W * H / 700f);
            float dr = 0.025f * W;
            for (int i = 0; i < dots; i++)
                space.ShadeEllipse((float)rng.NextDouble() * W, (float)rng.NextDouble() * H, dr, dr, 0.84f);

            // Bone: the block, a rounded cavity per pad, and stacked holes in
            // the two side lanes.
            var bone = new Canvas(W, H);
            bone.FillRounded(cx, cy, hw, hh, corner);
            float cav = padWorld * 0.5f * ppw + 0.045f * W;
            // Cap the cavity so a strut of bone bridges between neighbouring
            // pads (uncapped, the cavities ran together into one channel that
            // broke through the block's ends -- caught in the sprite dump).
            if (pitchWorld > 0f) cav = Mathf.Min(cav, pitchWorld * 0.5f * ppw - 3f);
            foreach (var p in padCentersLocal)
                bone.ClearRounded(cx + p.x * ppw, cy + p.y * ppw, cav, cav, cav * 0.5f);

            float laneIn = t + 0.02f * W + margin;
            float laneOut = cx - cav - 0.03f * W;
            float laneW = Mathf.Max(4f, laneOut - laneIn);
            float r = laneW * 0.5f;
            for (int side = -1; side <= 1; side += 2)
            {
                float lx = side < 0 ? laneIn + r : W - laneIn - r;
                float y = margin + t + r;
                while (y < H - margin - t - r * 0.5f)
                {
                    float rr = r * (1f + (float)rng.NextDouble() * 0.45f);
                    float aspect = 0.8f + (float)rng.NextDouble() * 0.4f;
                    bone.ClearEllipse(lx, y, Mathf.Min(rr, r * 1.1f), rr * aspect);
                    y += 2.6f * r + (float)rng.NextDouble() * 0.05f * W;
                }
            }
            bone.InnerRing(2, 0.86f);
            bone.OuterStroke(stroke, 0.18f);

            float worldW = worldSize.x;
            return new Marrow { Shadow = shadow.ToSprite(worldW), Space = space.ToSprite(worldW), Bone = bone.ToSprite(worldW) };
        }

        // ------------------------------------------------------------------
        // Lymph node (spec §1.3)
        // ------------------------------------------------------------------

        public struct Node
        {
            public Sprite Shadow, Vessels, Body, Follicles;
            /// <summary>World size of each layer's quad (body padded for the vessels).</summary>
            public Vector2 CanvasWorld;
        }

        /// <summary>Hilum (and efferent) on the LEFT, afferents on the RIGHT
        /// (tissue side, where DCs come from) when <paramref name="tissueOnRight"/>.</summary>
        public static Node BuildNode(Vector2 bodyWorld, bool tissueOnRight = true)
        {
            // Canvas aspect follows the padded body (the spec's 320x200
            // assumed one node aspect; this keeps PPU square for any).
            int W = 320;
            int H = Mathf.Max(64, Mathf.RoundToInt(W * (bodyWorld.y * 1.18f) / (bodyWorld.x * 1.24f)));
            float bw = W / 1.24f, bh = H / 1.18f;            // body size in canvas px
            float ox = (W - bw) * 0.5f, oy = (H - bh) * 0.5f; // body top-left (y-down) offset

            // Body-relative (u along W, v DOWN along H) -> canvas px (y-up).
            float X(float u) => ox + (tissueOnRight ? u : 1f - u) * bw;
            float Yv(float v) => H - (oy + v * bh);

            float bcx = W * 0.5f, bcy = H * 0.5f;
            float corner = 0.30f * bh;

            var shadow = new Canvas(W, H);
            shadow.FillRounded(bcx, bcy, bw * 0.5f, bh * 0.5f, corner);

            // Vessels (under the body): three afferents into the tissue side,
            // one efferent out of the hilum, with direction chevrons.
            var ves = new Canvas(W, H);
            void Vessel(float u0, float v0, float u1, float v1, float width)
            {
                ves.FillCapsule(X(u0), Yv(v0), X(u1), Yv(v1), width * bh * 0.5f);
            }
            Vessel(1.06f, .18f, .94f, .26f, 0.085f);
            Vessel(1.07f, .50f, .95f, .50f, 0.085f);
            Vessel(1.06f, .82f, .94f, .74f, 0.085f);
            Vessel(.03f, .50f, -.10f, .52f, 0.12f);
            // Chevrons: a short darker capsule pair mid-vessel, pointing the flow.
            void Chevron(float u, float v, float dirU)
            {
                float s = 0.028f * bh;
                ves.ShadeCapsule(X(u - dirU * 0.012f), Yv(v - 0.035f), X(u + dirU * 0.012f), Yv(v), s * 0.5f, 0.55f);
                ves.ShadeCapsule(X(u + dirU * 0.012f), Yv(v), X(u - dirU * 0.012f), Yv(v + 0.035f), s * 0.5f, 0.55f);
            }
            Chevron(1.00f, .22f, -1f); Chevron(1.01f, .50f, -1f); Chevron(1.00f, .78f, -1f);
            Chevron(-0.035f, .51f, -1f);
            ves.OuterStroke(0.02f * bh, 0.18f);

            // Body: cortex 0.93, hilum notch, paracortex 1.0, medullary cords 0.90.
            var body = new Canvas(W, H);
            body.FillRounded(bcx, bcy, bw * 0.5f, bh * 0.5f, corner);
            for (int i = 0; i < body.Buf.Length; i++) { body.Buf[i].r = body.Buf[i].g = body.Buf[i].b = 0.93f; }
            body.ClearEllipse(X(0f), Yv(0.5f), 0.06f * bw, 0.15f * bh);
            body.ShadeRounded(bcx, bcy, bw * 0.5f - 0.13f * bh, bh * 0.5f - 0.13f * bh, corner * 0.6f, 1.0f);
            // Medullary cords: four thin fingers fanning from the hilum. (Three
            // fat ones at ±35° read as one big "<" arrow in the sprite dump.)
            foreach (float deg in new[] { -50f, -18f, 18f, 50f })
            {
                float a = deg * Mathf.Deg2Rad;
                float u1 = 0.07f + Mathf.Cos(a) * 0.22f, v1 = 0.5f + Mathf.Sin(a) * 0.22f * (bw / bh) * 0.7f;
                body.ShadeCapsule(X(0.07f), Yv(0.5f), X(u1), Yv(v1), 0.03f * bh, 0.91f);
            }
            var bodyMask = new Canvas(W, H);
            System.Array.Copy(body.Buf, bodyMask.Buf, body.Buf.Length);
            body.OuterStroke(0.022f * bh, 0.18f);

            // Follicles: apricot mantles with pale germinal centres round the cortex.
            var fol = new Canvas(W, H);
            var spots = new[] { (.36f, .11f), (.60f, .10f), (.83f, .17f), (.92f, .50f), (.83f, .83f), (.60f, .90f), (.36f, .89f) };
            foreach (var (u, v) in spots) fol.FillEllipse(X(u), Yv(v), 0.095f * bh, 0.095f * bh);
            for (int i = 0; i < fol.Buf.Length; i++) { fol.Buf[i].r = fol.Buf[i].g = fol.Buf[i].b = 0.91f; }
            foreach (var (u, v) in spots) fol.ShadeEllipse(X(u), Yv(v + 0.012f), 0.055f * bh, 0.055f * bh, 1.0f);
            fol.ClipTo(bodyMask);
            fol.OuterStroke(1f, 0.45f);
            fol.ClipTo(bodyMask);

            float worldW = bodyWorld.x * 1.24f;
            return new Node
            {
                Shadow = shadow.ToSprite(worldW), Vessels = ves.ToSprite(worldW),
                Body = body.ToSprite(worldW), Follicles = fol.ToSprite(worldW),
                CanvasWorld = new Vector2(worldW, bodyWorld.y * 1.18f),
            };
        }
    }
}
