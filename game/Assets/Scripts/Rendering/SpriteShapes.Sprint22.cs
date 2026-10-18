using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Sprint 22's shapes: the antibody, the agglutinated raft, the plaque
    /// blister and the slough wave (INVASION_REWORK_SPEC.md section 7.2).
    ///
    /// Same contract as the rest of SpriteShapes: 64px, drawn WHITE with the
    /// silhouette in alpha, so the per-instance SpriteRenderer.color tint does
    /// all the colouring.
    /// </summary>
    public static partial class SpriteShapes
    {
        private static Sprite igaY;
        private static Sprite agglomerate;
        private static Sprite plaqueBlister;
        private static Sprite sloughWave;

        /// <summary>
        /// The antibody: three capsules in a Y.
        ///
        /// Drawn as a glyph rather than a blob because it is the single
        /// clearest piece of teaching in the whole rework -- between two
        /// cross-linked organisms the Y is drawn SPANNING the gap, one arm on
        /// each, which makes the Director's "two-headed, so it ties one
        /// organism to another" literal and needs no tooltip. A Y is the
        /// universal antibody glyph in every diagram.
        ///
        /// (Worth knowing while reading it: real secretory IgA is a dimer
        /// with FOUR binding arms, not two -- directly counted by AFM, Suzuki
        /// 2015 PNAS. "Two-headed" is the Director's shorthand for two joined
        /// bodies. At 0.3 of a cell on screen, two arms per end is what
        /// actually reads; the span is the point, not the arm count.)
        /// </summary>
        public static Sprite IgaY => igaY ??= Build(b =>
        {
            FillCapsule(b, C, C - 20f, C, C + 2f, 5f);       // stem
            FillCapsule(b, C, C + 2f, C - 16f, C + 20f, 5f); // left arm
            FillCapsule(b, C, C + 2f, C + 16f, C + 20f, 5f); // right arm
        }, 3);

        /// <summary>The raft a clump sits in: a soft lobed blob, drawn UNDER
        /// its members so they read as embedded in it rather than beside
        /// it.</summary>
        public static Sprite Agglomerate => agglomerate ??= Build(b =>
            FillLobed(b, C, C, 22f, 7, 0.10f, 0.4f), 2, 0.12f);

        /// <summary>
        /// The blister on a charging plaque. Height and colour carry the
        /// charge, so the shape is deliberately plain: it is a gauge, and a
        /// gauge should not have personality competing with its reading.
        ///
        /// A plain disc, NOT an arc. The first attempt used FillArc and came
        /// out as a crescent opening sideways -- caught by dumping it
        /// magnified before spending a build, which is exactly what that step
        /// is for. The renderer scales x and y independently, so a disc
        /// becomes the ellipse that reads as a swelling, and unlike a
        /// hand-drawn dome it stays correct whichever way "outward" points on
        /// a given map.</summary>
        public static Sprite PlaqueBlister => plaqueBlister ??= Build(b =>
            FillDisc(b, C, C, 26f), 3);

        /// <summary>A slough wave: a crescent sheet with a soft leading edge,
        /// sweeping the patch a sentinel goblet cell has just dumped its mucus
        /// over.</summary>
        public static Sprite SloughWave => sloughWave ??= Build(b =>
            FillArc(b, C - 16f, C, 40f, 4.5f, 0f, 62f * Mathf.Deg2Rad), 1, 0.08f);
    }
}
