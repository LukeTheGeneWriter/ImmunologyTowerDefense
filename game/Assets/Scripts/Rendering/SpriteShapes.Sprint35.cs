using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// **Sprint 35: pathogens are drawn as themselves.**
    ///
    /// Until now `PathogenAgent` picked its sprite from
    /// <see cref="PathogenClass"/> — two shapes for the whole roster — so
    /// rotavirus, *Vibrio cholerae* and A/E *E. coli* had rendered as
    /// generic balls and beans since Sprint 24, with working traits,
    /// knowledge rows and round scripts behind them. `ROSTER_EXPANSION_SPEC`
    /// §5.3 calls this one of the four gates in front of the ten new
    /// organisms; it is also a live defect for the three already shipped,
    /// which is why they get their sprites in the same pass.
    ///
    /// ## The channels (spec §2.2), and what they cost
    ///
    /// Colour cannot be the species channel — the palette has two pathogen
    /// hues and every other hue is already a meaning on this board. So
    /// identity is **silhouette family (the kingdom) × one tell (the
    /// mechanic) × size**, with hue varying only *within* a family.
    ///
    /// The tells here are drawn from spec §2.3's alphabet, where each
    /// symbol means one thing everywhere it appears:
    ///
    /// - **Additive bright star** — "acts without infecting anything."
    ///   *V. cholerae* exactly: cholera toxin works on a wall the organism
    ///   never enters, so subverted wall looks perfectly healthy.
    /// - **Bright inner pip** — "carries a toxin that acts on its own."
    ///   Rotavirus's NSP4 is the first viral enterotoxin described, and it
    ///   damages neighbouring cells the virus never infects. Round 32's
    ///   "Toxin, and something that feeds on it" is already about this.
    /// - **Solid dark anchor ring** — "fixed in place, resists removal."
    ///   The A/E pedestal. IgA enchainment and goblet sloughing both work
    ///   poorly on an organism that has built itself a foothold.
    ///
    /// **One honest exception.** Rotavirus also keeps its wheel — a rim
    /// with radial spokes. That is a *taxonomic* tell, which §2.2 rule 2
    /// says to avoid in favour of mechanical ones. It is kept because the
    /// organism's mechanics (non-lytic release, a two-organism dose) have
    /// no visual the player can act on in the moment, and because *rota*
    /// IS the wheel: it is the one virion in the gut whose name is its
    /// silhouette. Recorded as an exception rather than pretended away.
    ///
    /// **No flagella.** `ART_DIRECTION.md` §3.3 drops them and is right to:
    /// they vanish under 20 px, and long thin arms belong to the dendritic
    /// cell alone. *V. cholerae* is a comma, not a tadpole — Koch's comma
    /// bacillus, carried by the curve of the body instead of by a tail.
    /// </summary>
    public static partial class SpriteShapes
    {
        // ------------------------------------------------------------------
        // Rotavirus -- virus family (knobbed ball), violet.
        // ------------------------------------------------------------------
        private static Sprite rotavirus;
        public static Sprite Rotavirus
        {
            get
            {
                if (rotavirus == null)
                {
                    var b = NewBuffer();
                    // The wheel: a fat body, a rim, and short radial spokes
                    // reading inward from it. Spokes are drawn as capsules
                    // between two radii so they survive the 6 px stroke
                    // without closing the hub up.
                    FillDisc(b, C, C, 21f);
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i * Mathf.PI / 5f;
                        float ex = C + Mathf.Cos(a) * 25f, ey = C + Mathf.Sin(a) * 25f;
                        FillDisc(b, ex, ey, 3.6f);          // VP4 spikes, knobbed not spiked
                    }
                    // The wheel, as DARKNESS. A white ring painted into an
                    // already-opaque body is invisible -- the Sprint 23
                    // lesson, re-learned here by dumping the raster and
                    // looking at it: the first draft used FillRing and the
                    // rim simply was not there.
                    InnerShadeRing(b, C, C, 18f, 11.5f, 0.50f);
                    InnerShade(b, C, C, 8f, 1.30f);         // NSP4: a toxin that acts on its own
                    RimShade(b, 2, 0.6f);
                    rotavirus = Finish(b, 6);
                }
                return rotavirus;
            }
        }

        // ------------------------------------------------------------------
        // Vibrio cholerae -- bacterium family (green), the comma.
        // ------------------------------------------------------------------
        private static Sprite vibrio;
        public static Sprite Vibrio
        {
            get
            {
                if (vibrio == null)
                {
                    var b = NewBuffer();
                    // A curved body built from three overlapping capsules:
                    // a comma at board scale, with no tail to lose.
                    FillCapsule(b, 20f, C + 11f, 30f, C - 7f, 11f);
                    FillCapsule(b, 30f, C - 7f, 44f, C - 9f, 10.5f);
                    FillDisc(b, 20f, C + 11f, 10.5f);
                    InnerShade(b, 27f, C + 2f, 6f, 1.10f);
                    // The tell: it acts without ever infecting anything.
                    // Large, and bright rather than merely present -- at 20
                    // px a small additive star is a pixel of noise, which
                    // is what the first raster dump showed. It overlaps the
                    // body so the silhouette stays one piece.
                    FillStar(b, 44f, C + 8f, 4.5f, 13f, 6, 0.3f);
                    InnerShade(b, 44f, C + 8f, 9f, 1.45f);
                    vibrio = Finish(b, 6);
                }
                return vibrio;
            }
        }

        // ------------------------------------------------------------------
        // A/E E. coli -- bacterium family, two forms (spec §5.4).
        // ------------------------------------------------------------------
        private static Sprite aeEcoli;
        /// <summary>Free in the lumen: a longer, flatter-based rod than the
        /// generic bean, so the pair read as relatives rather than as the
        /// same organism.</summary>
        public static Sprite AttachingEffacingEcoli
        {
            get
            {
                if (aeEcoli == null)
                {
                    var b = NewBuffer();
                    FillCapsule(b, 17f, C, 47f, C, 12f);
                    InnerShade(b, C - 6f, C - 4f, 7f, 1.12f);
                    aeEcoli = Finish(b, 6);
                }
                return aeEcoli;
            }
        }

        private static Sprite aeEcoliAnchored;
        /// <summary>
        /// Attached: the same rod standing on a dark actin pedestal.
        ///
        /// This is the project's first **mid-board form change**, and it is
        /// real on both sides. Biologically, the pedestal is not something
        /// the organism carries around — it is assembled under an
        /// *intimately attached* bacterium, intimin binding Tir in the host
        /// membrane, effacing the microvilli beneath it. Mechanically it is
        /// the moment the player's flush answers (IgA enchainment, goblet
        /// sloughing) stop working on it. So the silhouette changes exactly
        /// when the fact about it changes.
        /// </summary>
        public static Sprite AttachingEffacingEcoliAnchored
        {
            get
            {
                if (aeEcoliAnchored == null)
                {
                    var b = NewBuffer();
                    FillCapsule(b, 17f, C + 7f, 47f, C + 7f, 11.5f);
                    InnerShade(b, C - 6f, C + 3f, 7f, 1.12f);
                    // The anchor: a dark plinth under the body, as wide as
                    // the organism and dark enough to read at 20 px, where
                    // a subtle one is just a smudge on the underside.
                    FillCapsule(b, C - 17f, C - 11f, C + 17f, C - 11f, 7.5f);
                    InnerShadeCapsule(b, C - 17f, C - 11f, C + 17f, C - 11f, 9f, 0.34f);
                    aeEcoliAnchored = Finish(b, 6);
                }
                return aeEcoliAnchored;
            }
        }

        // ------------------------------------------------------------------
        // The dispatch (spec §5.3)
        // ------------------------------------------------------------------

        /// <summary>
        /// The sprite for one organism, in one of its forms.
        ///
        /// **Falls back to the class generic** for any species without its
        /// own drawing, which is what makes this safe to call from every
        /// render path before the whole roster is drawn — a new species
        /// renders as a plausible member of its kingdom from the moment it
        /// exists, rather than as nothing.
        /// </summary>
        public static Sprite ForSpecies(PathogenSpecies species, PathogenForm form = PathogenForm.Primary)
        {
            switch (species)
            {
                case PathogenSpecies.Rotavirus: return Rotavirus;
                case PathogenSpecies.VibrioCholerae: return Vibrio;
                case PathogenSpecies.AttachingEffacingEcoli:
                    return form == PathogenForm.Alternate ? AttachingEffacingEcoliAnchored : AttachingEffacingEcoli;
            }
            return SpeciesRoster.ClassOf(species) == PathogenClass.IntracellularVirus ? Virion : LargeBacterium;
        }

        // ------------------------------------------------------------------
        // A fade, for the knowledge ribbon's edges.
        // ------------------------------------------------------------------
        private static Sprite fadeToRight;

        /// <summary>
        /// A horizontal alpha ramp: opaque at the left edge, clear at the
        /// right. Tinted to the card fill and laid over the edge of a
        /// clipped strip, it reads as the content fading out under the card
        /// rather than being chopped off -- which is the difference between
        /// "there is more, scroll" and "this is broken".
        ///
        /// Drawn here rather than imported because every other texture in
        /// this project is procedural, and a 64x64 ramp stretched across 30
        /// px of element is indistinguishable from a hand-made one.
        /// Mirrored with a negative scale for the left edge, so there is one
        /// texture, not two.
        /// </summary>
        public static Sprite FadeToRight
        {
            get
            {
                if (fadeToRight == null)
                {
                    var b = NewBuffer();
                    for (int x = 0; x < Res; x++)
                    {
                        // Eased rather than linear: a straight ramp reads as
                        // a grey band, an eased one reads as nothing at all.
                        float t = x / (float)(Res - 1);
                        float a = 1f - t;
                        a *= a;
                        for (int y = 0; y < Res; y++) b[y * Res + x] = new Color(1f, 1f, 1f, a);
                    }
                    fadeToRight = ToSprite(b);
                }
                return fadeToRight;
            }
        }

        /// <summary>Whether this species draws differently in its alternate
        /// form. Exposed so a harness can assert the two-form path is
        /// actually reachable rather than merely implemented, and so the
        /// agent can skip the work for the 15 species out of 16 that have
        /// one shape.</summary>
        public static bool HasAlternateForm(PathogenSpecies species) =>
            ForSpecies(species, PathogenForm.Primary) != ForSpecies(species, PathogenForm.Alternate);
    }
}
