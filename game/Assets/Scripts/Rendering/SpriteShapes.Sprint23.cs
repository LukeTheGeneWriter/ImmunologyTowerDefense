using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 23 roster sprites. Same conventions as the rest of
    /// <see cref="SpriteShapes"/>: drawn white with the silhouette in alpha,
    /// 64x64, lazily cached.
    ///
    /// Design brief these follow (docs/ART_DIRECTION.md, and the Director's
    /// standing note from Sprint 20 -- "excellent call on no faces"):
    /// **no faces, and each cell's real histological tell is the read.**
    /// All four are lymphoid, so they share the big-nucleus/thin-rim
    /// silhouette and are separated by one honest feature each:
    ///
    ///  - Cytotoxic T -- lytic GRANULES. That is literally what it carries.
    ///  - NK (ILC1)   -- the classical name is "large granular lymphocyte":
    ///                   bigger, and more granules than the CTL.
    ///  - gamma-delta -- ELONGATED, because it lives wedged between
    ///                   enterocytes rather than roaming open tissue.
    ///  - Plasma cell -- the histology-textbook clock-face: eccentric
    ///                   nucleus plus a pale perinuclear hof (the Golgi).
    /// </summary>
    public static partial class SpriteShapes
    {
        /// <summary>
        /// Sprint 23: the capsule counterpart of <c>InnerShade</c> -- shades
        /// an elongated region with the same soft edge, for a nucleus that
        /// is stretched along a cell's long axis rather than round.
        ///
        /// Written because stacking overlapping <c>InnerShade</c> discs to
        /// fake an elongated nucleus multiplies the shading where they
        /// overlap, producing visible banding down the middle of the cell.
        /// </summary>
        private static void InnerShadeCapsule(Color[] buf, float ax, float ay,
            float bx, float by, float halfW, float mul)
        {
            ForBox(Mathf.Min(ax, bx) - halfW - 1f, Mathf.Min(ay, by) - halfW - 1f,
                   Mathf.Max(ax, bx) + halfW + 1f, Mathf.Max(ay, by) + halfW + 1f, (x, y) =>
            {
                int idx = y * Res + x;
                if (buf[idx].a <= 0f) return;
                float d = SegDist(x + 0.5f, y + 0.5f, ax, ay, bx, by);
                if (d >= halfW) return;
                float f = Mathf.Lerp(mul, 1f, d / halfW);
                buf[idx].r *= f; buf[idx].g *= f; buf[idx].b *= f;
            });
        }

        /// <summary>Sprint 23: shades an annulus. Needed because the Fill*
        /// helpers paint WHITE into the alpha channel, so drawing a white
        /// ring inside an already-opaque cell body is invisible -- the only
        /// contrast available inside a filled shape is darkness.</summary>
        private static void InnerShadeRing(Color[] buf, float cx, float cy,
            float rOuter, float rInner, float mul)
        {
            ForBox(cx - rOuter - 1f, cy - rOuter - 1f, cx + rOuter + 1f, cy + rOuter + 1f, (x, y) =>
            {
                int idx = y * Res + x;
                if (buf[idx].a <= 0f) return;
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d >= rOuter || d <= rInner) return;
                buf[idx].r *= mul; buf[idx].g *= mul; buf[idx].b *= mul;
            });
        }

        private static Sprite cytotoxicT;
        /// <summary>Lymphocyte silhouette carrying three bright lytic
        /// granules -- the one feature that separates a killer from the
        /// helper-T it is otherwise identical to.</summary>
        public static Sprite CytotoxicT
        {
            get
            {
                if (cytotoxicT == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 24f);
                    // Nucleus pushed off-centre, leaving a WIDE cytoplasmic
                    // crescent on the lower-right for the granules to sit in.
                    InnerShade(b, C - 5f, C - 4f, 14f, 0.74f);
                    // Lytic granules -- DARKER than the cytoplasm, because
                    // every shape here is drawn white with the silhouette in
                    // alpha, so a "brighter" feature clamps to white and
                    // disappears. Darkness is the only contrast available.
                    InnerShade(b, C + 9f, C + 7f, 3.6f, 0.42f);
                    InnerShade(b, C + 3f, C + 13f, 3.2f, 0.42f);
                    InnerShade(b, C + 14f, C + 1f, 3.0f, 0.42f);
                    RimShade(b, 1, 0.78f);
                    cytotoxicT = Finish(b, 5);
                }
                return cytotoxicT;
            }
        }

        private static Sprite naturalKiller;
        /// <summary>Larger than the CTL and more heavily granulated -- the
        /// classical "large granular lymphocyte". Reads as a cousin of the
        /// CTL at a glance, which is correct: same job description, wholly
        /// different way of choosing a target.</summary>
        public static Sprite NaturalKiller
        {
            get
            {
                if (naturalKiller == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 26f);
                    InnerShade(b, C - 7f, C - 6f, 13f, 0.76f); // eccentric nucleus
                    // Five granules, bigger and spread wider than the CTL's
                    // three -- this is the "large GRANULAR lymphocyte".
                    InnerShade(b, C + 9f, C + 6f, 4.0f, 0.38f);
                    InnerShade(b, C + 2f, C + 14f, 3.6f, 0.38f);
                    InnerShade(b, C + 15f, C - 1f, 3.4f, 0.38f);
                    InnerShade(b, C + 11f, C + 15f, 3.0f, 0.38f);
                    InnerShade(b, C + 15f, C - 11f, 3.0f, 0.38f);
                    RimShade(b, 1, 0.78f);
                    naturalKiller = Finish(b, 5);
                }
                return naturalKiller;
            }
        }

        private static Sprite gammaDeltaT;
        /// <summary>Elongated on the threat axis: this cell lives squeezed
        /// into the lateral space between two enterocytes, and the shape
        /// should say "wedged in the wall", not "hunting in open tissue".</summary>
        public static Sprite GammaDeltaT
        {
            get
            {
                if (gammaDeltaT == null)
                {
                    var b = NewBuffer();
                    FillCapsule(b, C, C - 17f, C, C + 17f, 12f);
                    // An ELONGATED nucleus running with the long axis, not a
                    // round one: this cell is squeezed between two
                    // enterocytes and its nucleus is squeezed with it.
                    //
                    // **Deliberately a single asymmetric feature.** The first
                    // draft gave it two small symmetric processes and the
                    // magnified dump showed it immediately as a FACE -- two
                    // dots on an oval are eyes, and the Director ruled faces
                    // out in Sprint 20. One off-axis process cannot pair.
                    InnerShadeCapsule(b, C, C - 9f, C, C + 7f, 6.5f, 0.70f);
                    InnerShade(b, C + 6f, C - 13f, 3.0f, 0.52f);
                    RimShade(b, 1, 0.74f);
                    gammaDeltaT = Finish(b, 5);
                }
                return gammaDeltaT;
            }
        }

        private static Sprite plasmaCell;
        /// <summary>The clock-face plasma cell: a fat oval with the nucleus
        /// shoved hard to one side and a pale perinuclear hof beside it
        /// (the Golgi, which is enormous in a cell whose whole job is
        /// export). Recognisable to anyone who has looked down a microscope,
        /// and it reads as "factory", not "fighter".</summary>
        public static Sprite PlasmaCell
        {
            get
            {
                if (plasmaCell == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 25f);
                    // Nucleus shoved hard to one side -- the clock face.
                    InnerShade(b, C - 10f, C - 7f, 11f, 0.58f);
                    // The perinuclear hof is PALE, and pale is the one thing
                    // this drawing system cannot add (white is the ceiling).
                    // So it is carved out instead: a shaded crescent on the
                    // far side leaves an unshaded band beside the nucleus,
                    // and that untouched band reads as the hof.
                    InnerShade(b, C + 13f, C + 10f, 12f, 0.74f);
                    RimShade(b, 2, 0.70f);
                    plasmaCell = Finish(b, 5);
                }
                return plasmaCell;
            }
        }

        private static Sprite hostCellRecognised;
        /// <summary>
        /// Sprint 23: an infected host cell that immune cells have built
        /// real recognition pressure on -- drawn with a heavy notched ring
        /// around the cell body, so the player can see WHICH cell is about
        /// to be cleared and tell at a glance that something is working on
        /// it rather than merely standing near it.
        ///
        /// The Sprint 22 lesson applied again: the plaque blister read
        /// because it replaced a hidden roll with a shape that fills. A
        /// colour ramp alone would be too quiet on a cell that is already
        /// tinted for being infected.
        /// </summary>
        public static Sprite HostCellRecognised
        {
            get
            {
                if (hostCellRecognised == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 30f, 8f);
                    RimShade(b, 2, 0.70f);
                    // The recognition collar, as a DARK annulus inset from
                    // the cell border. It has to be dark: a white ring drawn
                    // inside an already-opaque white cell body is invisible,
                    // which is exactly how the first draft of this sprite
                    // came back blank in the magnified dump.
                    InnerShadeRing(b, C, C, 26f, 20f, 0.42f);
                    InnerShade(b, C, C, 11f, 0.62f); // the resident, still there
                    hostCellRecognised = ToSprite(b);
                }
                return hostCellRecognised;
            }
        }

        private static Sprite antibody;
        /// <summary>A single antibody: the Y that everyone already knows.
        /// Drawn at 1x1 fine-tile scale, so it is three strokes and nothing
        /// else -- detail here would just alias away.</summary>
        public static Sprite Antibody
        {
            get
            {
                if (antibody == null)
                {
                    var b = NewBuffer();
                    FillCapsule(b, C, C - 20f, C, C + 2f, 6f);        // stem
                    FillCapsule(b, C, C + 2f, C - 16f, C + 20f, 6f);  // left arm
                    FillCapsule(b, C, C + 2f, C + 16f, C + 20f, 6f);  // right arm
                    antibody = Finish(b, 6);
                }
                return antibody;
            }
        }
    }
}
