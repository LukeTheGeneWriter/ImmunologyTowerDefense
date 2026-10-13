using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 21 (docs/COSMETIC_PASS_SPEC.md §1.2, §2.4, §3.2): the UI icon
    /// set, the marrow build pad, its selection ring and "+" glyph, and the
    /// board's plate frame. Same conventions as the rest of SpriteShapes:
    /// white RGB, the silhouette in alpha, detail in value, tinted per
    /// renderer / per UI element; outlines via <c>Finish</c>.
    ///
    /// Coordinates in this file are written **y-down** (as in the spec and
    /// its PIL mocks) and flipped with <see cref="Y"/>, because Texture2D row
    /// 0 is the bottom.
    /// </summary>
    public static partial class SpriteShapes
    {
        private static float Y(float yDown) => Res - yDown;

        private static Sprite iconWaves, iconDroplet, iconBlock, iconBolt, iconHeart, iconStar,
                              iconArrowDown, iconPlay, iconClose, hatchTile, buildPad, selectRing,
                              plusGlyph, plateFrame;

        /// <summary>Cytokine sensing: a source dot and three radiating arcs.</summary>
        public static Sprite IconWaves => iconWaves ??= Build(b =>
        {
            FillDisc(b, 17f, Y(32f), 9f);
            foreach (float r in new[] { 16f, 25f, 34f })
                FillArc(b, 17f, Y(32f), r, 2.5f, 0f, 50f * Mathf.Deg2Rad);
        }, 3);

        /// <summary>Mucus turnover: a droplet, with a pale highlight.</summary>
        public static Sprite IconDroplet => iconDroplet ??= Build(b =>
        {
            FillDisc(b, 32f, Y(38f), 16f);
            FillPolygon(b, new[] { new Vector2(18f, Y(32f)), new Vector2(32f, Y(6f)), new Vector2(46f, Y(32f)) });
            InnerShade(b, 26f, Y(40f), 5f, 1.25f);
        }, 3);

        /// <summary>"Blocked": a ring with a diagonal bar.</summary>
        public static Sprite IconBlock => iconBlock ??= Build(b =>
        {
            FillRing(b, C, C, 22f, 15f);
            FillCapsule(b, C - 15f, C - 15f, C + 15f, C + 15f, 3.5f);
        }, 2);

        /// <summary>ATP: a lightning bolt.</summary>
        public static Sprite IconBolt => iconBolt ??= Build(b =>
            FillPolygon(b, new[]
            {
                new Vector2(36f, Y(4f)), new Vector2(12f, Y(36f)), new Vector2(30f, Y(36f)),
                new Vector2(24f, Y(60f)), new Vector2(52f, Y(24f)), new Vector2(34f, Y(24f)), new Vector2(42f, Y(4f)),
            }), 3);

        public static Sprite IconHeart => iconHeart ??= Build(b =>
        {
            FillDisc(b, 21f, Y(25f), 13f);
            FillDisc(b, 43f, Y(25f), 13f);
            FillPolygon(b, new[] { new Vector2(10f, Y(30f)), new Vector2(32f, Y(56f)), new Vector2(54f, Y(30f)) });
        }, 3);

        public static Sprite IconStar => iconStar ??= Build(b => FillStarPolygon(b, C, C, 28f, 12f, 5), 3);

        public static Sprite IconArrowDown => iconArrowDown ??= Build(b =>
        {
            FillCapsule(b, C, Y(8f), C, Y(34f), 6f);
            FillPolygon(b, new[] { new Vector2(12f, Y(32f)), new Vector2(52f, Y(32f)), new Vector2(32f, Y(58f)) });
        }, 3);

        public static Sprite IconPlay => iconPlay ??= Build(b =>
            FillPolygon(b, new[] { new Vector2(16f, Y(8f)), new Vector2(16f, Y(56f)), new Vector2(56f, Y(32f)) }), 3);

        public static Sprite IconClose => iconClose ??= Build(b =>
        {
            FillCapsule(b, 16f, 16f, 48f, 48f, 5f);
            FillCapsule(b, 16f, 48f, 48f, 16f, 5f);
        }, 3);

        /// <summary>The "designed, not built" hatch (spec §4.4): 2 px
        /// diagonal stripes every 6 px at α 0.8, tiled as a UI background.
        /// No outline.</summary>
        public static Sprite HatchTile => hatchTile ??= BuildRaw(b =>
        {
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                    if (((x + y) % 6) < 2) b[y * Res + x] = new Color(1f, 1f, 1f, 0.8f);
        });

        /// <summary>Marrow build pad (spec §1.2): a steel plate with a
        /// recessed face and a lit lip -- the Kingdom Rush "plot".</summary>
        public static Sprite BuildPad => buildPad ??= Build(b =>
        {
            FillRounded(b, C, C, 30f, 15f);
            ShadeRounded(b, C, C, 30f - 0.12f * Res, 11f, 0.88f);
            ShadeRounded(b, C, C - 2f, 30f - 0.16f * Res, 9f, 0.93f / 0.88f);
        }, 4);

        /// <summary>The selected-pad ring (spec §1.2) -- replaces the slate
        /// ring that vanished on the light board.</summary>
        public static Sprite SelectRing => selectRing ??= Build(b =>
        {
            FillRounded(b, C, C, 31f, 18f);
            ClearRounded(b, C, C, 25f, 12f);
        }, 2);

        /// <summary>The empty-pad "+" (spec §1.2), outlined lighter (0.30).</summary>
        public static Sprite PlusGlyph => plusGlyph ??= Build(b =>
        {
            FillCapsule(b, C, C - 18f, C, C + 18f, 4.5f);
            FillCapsule(b, C - 18f, C, C + 18f, C, 4.5f);
        }, 3, strokeValue: 0.30f);

        // ------------------------------------------------------------------
        // The plate frame (spec §3.2) -- one 9-sliced renderer round the board
        // ------------------------------------------------------------------

        private const int PlateRes = 128;
        private const int PlateBorder = 48;
        /// <summary>The window edge sits this far in from the texture edge.
        /// (The spec's inset of 48 put a 40 px corner arc across the slice
        /// border, so the stretched centre collapsed -- the window is inset
        /// 16 instead, which keeps the whole corner arc inside the 48 px
        /// corner slice, and leaves room for the ring and the shadow.)</summary>
        private const int PlateInset = 16;
        private const float PlateCorner = 32f;
        private const float PlateRing = 6f;

        /// <summary>World units the frame extends past the board on each side
        /// (PlateInset at PPU 100). Size the sliced renderer to
        /// board + 2 x this.</summary>
        public const float PlateMarginWorld = PlateInset / 100f;

        /// <summary>A 128 px 9-slice (border 48, PPU 100): the board shows
        /// through a rounded window; outside it is the desk colour (tint it
        /// <c>Palette.CameraClear</c>, so it is invisible against the clear
        /// and only masks the board's square corners); round the window runs
        /// a 6 px ink ring (value 0.5 -> near-black under the tint) with a
        /// baked drop shadow below. Draw with <c>SpriteDrawMode.Sliced</c>
        /// at board size + 2 x <see cref="PlateMarginWorld"/>.</summary>
        public static Sprite PlateFrame
        {
            get
            {
                if (plateFrame != null) return plateFrame;
                var buf = new Color[PlateRes * PlateRes];
                float half = PlateRes * 0.5f - PlateInset;     // inner window half-extent
                float c = PlateRes * 0.5f;
                for (int y = 0; y < PlateRes; y++)
                    for (int x = 0; x < PlateRes; x++)
                    {
                        float d = RoundedDist(x + 0.5f - c, y + 0.5f - c, half, PlateCorner);
                        // Drop shadow: the window's silhouette offset 5 px down, outside the ring.
                        float ds = RoundedDist(x + 0.5f - c, y + 0.5f - c + 5f, half, PlateCorner);
                        Color px;
                        if (d <= 0f) px = new Color(1f, 1f, 1f, 0f);                         // the board shows through
                        else if (d <= PlateRing)
                        {
                            float aa = Mathf.Clamp01(d + 0.5f);                                // soften the inner edge
                            px = new Color(0.5f, 0.5f, 0.5f, aa);
                        }
                        else
                        {
                            float v = ds <= 10f ? Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(ds / 10f)) : 1f;
                            px = new Color(v, v, v, 1f);
                        }
                        buf[y * PlateRes + x] = px;
                    }
                var tex = new Texture2D(PlateRes, PlateRes, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                tex.SetPixels(buf);
                tex.Apply();
                plateFrame = Sprite.Create(tex, new Rect(0, 0, PlateRes, PlateRes), new Vector2(0.5f, 0.5f), 100f,
                    0, SpriteMeshType.FullRect, new Vector4(PlateBorder, PlateBorder, PlateBorder, PlateBorder));
                return plateFrame;
            }
        }

        /// <summary>Signed distance outside a rounded square (0 = on the edge, &lt;0 inside).</summary>
        private static float RoundedDist(float px, float py, float half, float corner)
        {
            float qx = Mathf.Abs(px) - (half - corner);
            float qy = Mathf.Abs(py) - (half - corner);
            float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - corner;
        }

        // ------------------------------------------------------------------
        // Builders and primitives used by this file
        // ------------------------------------------------------------------

        private static Sprite Build(System.Action<Color[]> draw, int stroke, float strokeValue = 0.18f)
        {
            var b = NewBuffer();
            draw(b);
            return Finish(b, stroke, true, strokeValue);
        }

        private static Sprite BuildRaw(System.Action<Color[]> draw)
        {
            var b = NewBuffer();
            draw(b);
            return ToSprite(b);
        }

        /// <summary>Point-in-polygon fill (even-odd), 4x supersampled.</summary>
        private static void FillPolygon(Color[] buf, Vector2[] pts)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in pts)
            {
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }
            ForBox(minX - 1f, minY - 1f, maxX + 1f, maxY + 1f, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    bool inside = false;
                    for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                    {
                        if ((pts[i].y > fy) != (pts[j].y > fy) &&
                            fx < (pts[j].x - pts[i].x) * (fy - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                            inside = !inside;
                    }
                    return inside;
                });
                Blend(buf, x, y, cov);
            });
        }

        /// <summary>A straight-edged n-point star (the spec's IconStar).</summary>
        private static void FillStarPolygon(Color[] buf, float cx, float cy, float rOuter, float rInner, int points)
        {
            var pts = new Vector2[points * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                float a = Mathf.PI * 0.5f + i * Mathf.PI / points;
                float r = (i % 2 == 0) ? rOuter : rInner;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            FillPolygon(buf, pts);
        }

        /// <summary>A ring segment of radius <paramref name="r"/> and
        /// half-width <paramref name="halfW"/>, centred on angle
        /// <paramref name="center"/>, ±<paramref name="halfAngle"/>, round-capped.</summary>
        private static void FillArc(Color[] buf, float cx, float cy, float r, float halfW, float center, float halfAngle)
        {
            ForBox(cx - r - halfW - 1f, cy - r - halfW - 1f, cx + r + halfW + 1f, cy + r + halfW + 1f, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    float dx = fx - cx, dy = fy - cy;
                    float a = Mathf.Atan2(dy, dx) - center;
                    a = Mathf.Repeat(a + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;
                    float clamped = Mathf.Clamp(a, -halfAngle, halfAngle);
                    float px = cx + Mathf.Cos(center + clamped) * r, py = cy + Mathf.Sin(center + clamped) * r;
                    return (fx - px) * (fx - px) + (fy - py) * (fy - py) <= halfW * halfW;
                });
                Blend(buf, x, y, cov);
            });
        }

        /// <summary>Multiply value inside a rounded square.</summary>
        private static void ShadeRounded(Color[] buf, float cx, float cy, float half, float corner, float mul)
        {
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (buf[idx].a <= 0f) continue;
                    if (RoundedDist(x + 0.5f - cx, y + 0.5f - cy, half, corner) > 0f) continue;
                    buf[idx].r *= mul; buf[idx].g *= mul; buf[idx].b *= mul;
                }
        }

        /// <summary>Cut a rounded square out of the alpha (anti-aliased).</summary>
        private static void ClearRounded(Color[] buf, float cx, float cy, float half, float corner)
        {
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    float d = RoundedDist(x + 0.5f - cx, y + 0.5f - cy, half, corner);
                    if (d >= 0.5f) continue;
                    int idx = y * Res + x;
                    buf[idx].a *= Mathf.Clamp01(d + 0.5f);
                }
        }
    }
}
