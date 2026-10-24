using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Procedurally-drawn shape sprites for the visual identity pass -- the
    /// planned successor to <see cref="RuntimeSprites.SquareSprite"/>'s single
    /// white quad. See docs/SPRITE_DESIGN.md for the design rationale and the
    /// migration plan.
    ///
    /// **This class is standalone and wired nowhere.** It is a prototype the
    /// head session integrates: no gameplay type is referenced, nothing here
    /// changes a rendering call site, and it follows the same lazy-static
    /// caching pattern as <see cref="RuntimeSprites"/> so a headless harness
    /// that never touches it pays nothing.
    ///
    /// Every shape is drawn in WHITE with the silhouette carried in the alpha
    /// channel, so the existing per-instance <c>SpriteRenderer.color</c> tint
    /// (cargo, paired, infected lerp, cytokine heat, contact flash) keeps
    /// working unchanged -- a swap is <c>sr.sprite = SpriteShapes.Foo</c> and
    /// nothing else.
    ///
    /// Textures are 64x64, generated once at first access. Roughly 20 shapes
    /// at 16 KB each is ~320 KB of texture memory, all shared -- 150 pooled
    /// pathogens still point at one <see cref="Sprite"/>. Generation does
    /// allocate transiently (a working buffer per shape, a closure per pixel
    /// in the coverage sampler); that is one-time boot cost, not per frame.
    ///
    /// NOTE: written without a Unity compiler available on the authoring
    /// machine -- review at integration.
    /// </summary>
    public static partial class SpriteShapes
    {
        private const int Res = 64;
        private const float C = Res * 0.5f;

        // ------------------------------------------------------------------
        // Public shape accessors -- lazy, cached, shared by every instance.
        // ------------------------------------------------------------------

        private static Sprite macrophage;
        public static Sprite Macrophage
        {
            get
            {
                if (macrophage == null)
                {
                    var b = NewBuffer();
                    FillLobed(b, C, C, 26f, 4, 0.16f, 0.3f);   // broad ruffled membrane
                    InnerShade(b, C + 3f, C - 2f, 10f, 0.70f); // off-centre nucleus
                    RimShade(b, 2, 0.55f);
                    macrophage = Finish(b, 5);
                }
                return macrophage;
            }
        }

        private static Sprite neutrophil;
        public static Sprite Neutrophil
        {
            get
            {
                if (neutrophil == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 24f);
                    InnerShade(b, C - 6f, C + 3f, 7f, 0.60f);  // multi-lobed
                    InnerShade(b, C + 6f, C + 4f, 6f, 0.60f);  // nucleus
                    InnerShade(b, C, C - 7f, 6f, 0.60f);
                    RimShade(b, 2, 0.60f);
                    neutrophil = Finish(b, 6);
                }
                return neutrophil;
            }
        }

        private static Sprite dendriteStar;
        public static Sprite DendriteStar
        {
            get
            {
                if (dendriteStar == null)
                {
                    var b = NewBuffer();
                    FillStar(b, C, C, 11f, 30f, 9, 0f);
                    FillDisc(b, C, C, 10f); // solid core
                    RimShade(b, 1, 0.60f);
                    dendriteStar = Finish(b, 5);
                }
                return dendriteStar;
            }
        }

        private static Sprite dendriteStarLoaded;
        /// <summary>Carrying-antigen variant: the same star with a bright
        /// un-shaded core dot, so a loaded DC reads brighter after tint.</summary>
        public static Sprite DendriteStarLoaded
        {
            get
            {
                if (dendriteStarLoaded == null)
                {
                    var b = NewBuffer();
                    FillStar(b, C, C, 11f, 30f, 9, 0f);
                    FillDisc(b, C, C, 10f);
                    RimShade(b, 1, 0.60f);
                    FillDisc(b, C, C, 7f); // redraw core at full white, no rim
                    dendriteStarLoaded = Finish(b, 5);
                }
                return dendriteStarLoaded;
            }
        }

        private static Sprite lymphocyte;
        public static Sprite Lymphocyte
        {
            get
            {
                if (lymphocyte == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 24f);
                    InnerShade(b, C, C, 17f, 0.72f); // big nucleus, thin cytoplasm rim
                    RimShade(b, 1, 0.78f);
                    lymphocyte = Finish(b, 5);
                }
                return lymphocyte;
            }
        }

        private static Sprite largeBacterium;
        public static Sprite LargeBacterium
        {
            get
            {
                if (largeBacterium == null)
                {
                    var b = NewBuffer();
                    // Sprint 20 (ART_DIRECTION.md §3.3): a chunky 2:1
                    // bean, not a 4:1 rod -- a thin rod vanishes at
                    // board scale. Germ-green tint + thick outline.
                    FillCapsule(b, 19f, C, 45f, C, 13f);
                    InnerShade(b, C - 4f, C + 4f, 8f, 1.15f);   // a soft highlight, not a face
                    largeBacterium = Finish(b, 6);
                }
                return largeBacterium;
            }
        }

        private static Sprite virion;
        public static Sprite Virion
        {
            get
            {
                if (virion == null)
                {
                    var b = NewBuffer();
                    // Sprint 20 (ART_DIRECTION.md §3.3): the cartoon
                    // corona -- a ball with 8 short knobbed spokes. Short
                    // on purpose: long thin arms belong to the DC alone.
                    FillDisc(b, C, C, 15f);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f + 0.2f;
                        float ex = C + Mathf.Cos(a) * 22f, ey = C + Mathf.Sin(a) * 22f;
                        FillCapsule(b, C, C, ex, ey, 2.2f);
                        FillDisc(b, ex, ey, 4.5f);
                    }
                    virion = Finish(b, 6);
                }
                return virion;
            }
        }

        private static Sprite foodBolus;
        public static Sprite FoodBolus
        {
            get
            {
                if (foodBolus == null)
                {
                    var b = NewBuffer();
                    // Sprint 19 (Director: "opaque and not so spikey"). The
                    // Sprint 13 bolus was six deep lobes (depth 0.28 -- read
                    // as spikes at board scale) with an ALPHA stipple that
                    // punched 70% of its pixels down to 0.4 opacity, so the
                    // channel showed through it. Now: a soft lumpy mass --
                    // a shallow-lobed core plus three overlapping rounded
                    // lumps -- fully opaque, its grubbiness carried in VALUE
                    // (darker flecks) instead of holes.
                    FillLobed(b, C, C, 23f, 5, 0.07f, 0.9f);
                    FillDisc(b, C - 9f, C + 8f, 15f);
                    FillDisc(b, C + 10f, C + 5f, 14f);
                    FillDisc(b, C + 2f, C - 11f, 15f);
                    ValueStipple(b, 4242, 0.35f, 0.72f);
                    RimShade(b, 3, 0.62f);
                    foodBolus = Finish(b, 5);
                }
                return foodBolus;
            }
        }

        private static Sprite hostCell;
        /// <summary>Board grid, Healthy / (Infected via tint). Opaque, drawn
        /// nearly to the tile edge with a dark 1px rim -- packed epithelium
        /// with visible cell boundaries, no added overdraw.</summary>
        public static Sprite HostCell
        {
            get
            {
                if (hostCell == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 30f, 8f);
                    InnerShade(b, C + 4f, C - 4f, 9f, 0.85f); // faint nucleus
                    RimShade(b, 1, 0.80f);
                    hostCell = ToSprite(b);
                }
                return hostCell;
            }
        }

        private static Sprite hostCellInfectedViral;
        /// <summary>Board grid, virally infected. A crisp opaque
        /// **inclusion body** disc off-centre + a darker inset perimeter so
        /// the infected patch has a countable edge. Tinted violet by
        /// BoardRenderer. (RGB brightening is a no-op here — the sprite is
        /// white + alpha and the hue comes from the tint — so "swollen"
        /// has to be carried by the alpha silhouette, not brightness.)</summary>
        public static Sprite HostCellInfectedViral
        {
            get
            {
                if (hostCellInfectedViral == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 30f, 8f);
                    RimShade(b, 2, 0.70f);            // inset border
                    FillDisc(b, C + 3f, C - 3f, 12f); // inclusion body -- stays fully opaque
                    hostCellInfectedViral = ToSprite(b);
                }
                return hostCellInfectedViral;
            }
        }

        private static Sprite hostCellInfectedBacterial;
        /// <summary>Board grid, bacterially infected. A **granular / purulent
        /// stipple** across the interior instead of a clean inclusion.
        /// Tinted sickly yellow-green by BoardRenderer.</summary>
        public static Sprite HostCellInfectedBacterial
        {
            get
            {
                if (hostCellInfectedBacterial == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 30f, 8f);
                    ValueStipple(b, 24601, 0.38f, 0.85f);  // purulent, but soft and opaque (Sprint 20)
                    RimShade(b, 2, 0.70f);
                    hostCellInfectedBacterial = ToSprite(b);
                }
                return hostCellInfectedBacterial;
            }
        }

        private static Sprite debris;
        public static Sprite Debris
        {
            get
            {
                if (debris == null)
                {
                    var b = NewBuffer();
                    var rng = new System.Random(90210);
                    int chunks = 7;
                    for (int i = 0; i < chunks; i++)
                    {
                        float ox = C + (float)(rng.NextDouble() - 0.5) * 40f;
                        float oy = C + (float)(rng.NextDouble() - 0.5) * 40f;
                        float rr = 5f + (float)rng.NextDouble() * 6f;
                        FillDisc(b, ox, oy, rr);
                    }
                    ValueStipple(b, 1337, 0.5f, 0.8f);
                    RimShade(b, 2, 0.55f);
                    debris = Finish(b, 2);
                }
                return debris;
            }
        }

        private static Sprite emptyPit;
        /// <summary>Bare ground. Mostly transparent -- a faint pit. Flat
        /// colour (no sprite) is an acceptable alternative; this just gives
        /// the hole a hint of depth.</summary>
        public static Sprite EmptyPit
        {
            get
            {
                if (emptyPit == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 30f);
                    // Opaque (Sprint 20): a dark-rose scar, not a hole in the render.
                    emptyPit = ToSprite(b);
                }
                return emptyPit;
            }
        }

        private static Sprite epithelialBar;
        public static Sprite EpithelialBar
        {
            get
            {
                if (epithelialBar == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 2f); // full bleed
                    // darker seams every ~10px -> a row of epithelial cells
                    for (int y = 0; y < Res; y++)
                    {
                        for (int x = 0; x < Res; x++)
                        {
                            int idx = y * Res + x;
                            if (b[idx].a <= 0f) continue;
                            if (x % 10 == 0 || y % 20 == 0)
                            {
                                b[idx].r *= 0.6f; b[idx].g *= 0.6f; b[idx].b *= 0.6f;
                            }
                        }
                    }
                    // Sprint 15: sparse brighter goblet-cell flecks -- the
                    // mucus-secreting cells, tying the wall to the lumen mucus band.
                    var grng = new System.Random(918);
                    for (int i = 0; i < 5; i++)
                        InnerShade(b, 6f + (float)grng.NextDouble() * 52f,
                                      6f + (float)grng.NextDouble() * 52f, 4f, 1.20f);
                    epithelialBar = ToSprite(b);
                }
                return epithelialBar;
            }
        }

        // ---- Sprint 15: compartment fields, walls, and pooled motes -------
        //   docs/COMPARTMENT_DESIGN.md. White RGB, detail in ALPHA, tinted
        //   per-instance by the owning renderer. The three gradient
        //   primitives they use are at the bottom of the primitives block.

        private static Sprite chymeField;
        /// <summary>Lumen channel backdrop -- one quad over the lumen band.
        /// Alpha ramps down the flow axis + a light stipple so it is not a
        /// flat wash. Tinted warm brown-olive by <c>LumenChannelRenderer</c>.</summary>
        public static Sprite ChymeField
        {
            get
            {
                if (chymeField == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 2f);
                    AxisGradient(b, alongX: false, aStart: 0.92f, aEnd: 0.55f);
                    // Flat, unstippled (Sprint 20 -- grounds are calm).
                    chymeField = ToSprite(b);
                }
                return chymeField;
            }
        }

        private static Sprite mucusBand;
        /// <summary>The mucus / glycocalyx layer along the gut wall. Opaque
        /// at one edge, feathering to transparent across ~65% of its depth.
        /// Semi-transparent grey-green, set by the renderer.</summary>
        public static Sprite MucusBand
        {
            get
            {
                if (mucusBand == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 2f);
                    EdgeGradient(b, edge: 0, featherPx: 42f);
                    Stipple(b, 6029, 0.90f);
                    mucusBand = ToSprite(b);
                }
                return mucusBand;
            }
        }

        private static Sprite flowMote;
        /// <summary>Pooled lumen particulate -- fine chyme drifting down the
        /// lanes. <c>LumenChannelRenderer</c> pools these and tweens them
        /// along <c>BoardConfig.FlowCrossStep</c> while <c>!RoundClock.Frozen</c>.</summary>
        public static Sprite FlowMote
        {
            get
            {
                if (flowMote == null)
                {
                    var b = NewBuffer();
                    FillLobed(b, C, C, 6f, 3, 0.30f, 1.7f);
                    Stipple(b, 6047, 0.70f);
                    RimShade(b, 1, 0.70f);
                    flowMote = ToSprite(b);
                }
                return flowMote;
            }
        }

        private static Sprite plasmaField;
        /// <summary>Base-band blood plasma -- one opaque quad over the base
        /// band, alpha lifting toward the vessel wall + a light stipple.
        /// Deep oxblood tint set by <c>BaseCompartmentRenderer</c>.</summary>
        public static Sprite PlasmaField
        {
            get
            {
                if (plasmaField == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 2f);
                    AxisGradient(b, alongX: true, aStart: 0.86f, aEnd: 1.0f);
                    // Flat, unstippled (Sprint 20 -- grounds are calm).
                    plasmaField = ToSprite(b);
                }
                return plasmaField;
            }
        }

        private static Sprite vesselWallBar;
        /// <summary>The endothelial boundary at the base/tissue seam -- the
        /// line immune cells cross to reach the fight. Smoother than
        /// <see cref="EpithelialBar"/>: a few faint seams only. Muted
        /// red-pink tint set by <c>BaseCompartmentRenderer</c>.</summary>
        public static Sprite VesselWallBar
        {
            get
            {
                if (vesselWallBar == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 3f);
                    for (int y = 0; y < Res; y++)
                        for (int x = 0; x < Res; x++)
                        {
                            int idx = y * Res + x;
                            if (b[idx].a <= 0f) continue;
                            if (x % 18 == 0) { b[idx].r *= 0.82f; b[idx].g *= 0.82f; b[idx].b *= 0.82f; }
                        }
                    RimShade(b, 1, 0.85f);
                    vesselWallBar = ToSprite(b);
                }
                return vesselWallBar;
            }
        }

        private static Sprite erythrocyte;
        /// <summary>Pooled base-band blood-cell streamer -- a biconcave disc.
        /// <c>BaseCompartmentRenderer</c> pools these, spawns them at the
        /// outer edge and drifts them toward the vessel wall while
        /// <c>!RoundClock.Frozen</c>. Bright arterial-red tint.</summary>
        public static Sprite Erythrocyte
        {
            get
            {
                if (erythrocyte == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 14f);
                    // Sprint 17: a deeper, wider central dip and a darker rim
                    // -- the biconcave dish reads as a shape at a glance now
                    // rather than as a slightly uneven dot.
                    InnerShade(b, C, C, 9f, 0.42f);
                    RimShade(b, 2, 0.72f);
                    erythrocyte = Finish(b, 4);
                }
                return erythrocyte;
            }
        }

        private static Sprite birthPuff;
        /// <summary>Pooled marrow "a cell was made" mote -- soft, rimless.
        /// Budded near an occupied slot on <c>BoneMarrowManager.OnCellEmitted</c>,
        /// drifts toward the blood-side edge fading, then recycles.</summary>
        public static Sprite BirthPuff
        {
            get
            {
                if (birthPuff == null)
                {
                    var b = NewBuffer();
                    RadialGradient(b, C, C, 0f, 12f, aInner: 1.0f, aOuter: 0.0f);
                    birthPuff = ToSprite(b);
                }
                return birthPuff;
            }
        }

        private static Sprite nodeColocGlow;
        /// <summary>The lymph-node co-localisation field made visible
        /// (GAME_DESIGN.md §5c). One soft blob over the node rect;
        /// <c>LymphNodeFieldRenderer</c> rewrites <c>sr.color</c> from
        /// <c>LymphNode.Coloc</c> so it brightens where the helper-T cells
        /// have gathered and a DC visibly drifts up it.</summary>
        public static Sprite NodeColocGlow
        {
            get
            {
                if (nodeColocGlow == null)
                {
                    var b = NewBuffer();
                    RadialGradient(b, C, C, 4f, 31f, aInner: 0.90f, aOuter: 0.0f);
                    nodeColocGlow = ToSprite(b);
                }
                return nodeColocGlow;
            }
        }

        // ---- Effect flashes: one distinct silhouette per event -----------

        private static Sprite granuleBurst;
        public static Sprite GranuleBurst
        {
            get
            {
                if (granuleBurst == null)
                {
                    var b = NewBuffer();
                    FillRing(b, C, C, 30f, 10f);
                    Stipple(b, 808, 0.55f); // scattered granules
                    granuleBurst = ToSprite(b);
                }
                return granuleBurst;
            }
        }

        private static Sprite breachStar;
        public static Sprite BreachStar
        {
            get
            {
                if (breachStar == null)
                {
                    var b = NewBuffer();
                    FillStar(b, C, C, 12f, 32f, 10, 0f);
                    ClearDisc(b, C, C, 10f); // hollow -- a rupture ring
                    breachStar = ToSprite(b);
                }
                return breachStar;
            }
        }

        private static Sprite effeBloom;
        public static Sprite EffeBloom
        {
            get
            {
                if (effeBloom == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C, C, 30f);
                    Multiply(b, 1f, 1f, 1f, 0.65f); // soft outer
                    FillDisc(b, C, C, 17f);         // brighter core (max-blend)
                    effeBloom = ToSprite(b);
                }
                return effeBloom;
            }
        }

        private static Sprite stressRing;
        public static Sprite StressRing
        {
            get
            {
                if (stressRing == null)
                {
                    var b = NewBuffer();
                    FillRing(b, C, C, 30f, 19f); // bold shockwave
                    FillDisc(b, C, C, 8f);       // bright core
                    stressRing = ToSprite(b);
                }
                return stressRing;
            }
        }

        private static Sprite knowledgeRing;
        public static Sprite KnowledgeRing
        {
            get
            {
                if (knowledgeRing == null)
                {
                    var b = NewBuffer();
                    FillRing(b, C, C, 30f, 25f); // clean thin ring
                    FillDisc(b, C, C, 6f);
                    knowledgeRing = ToSprite(b);
                }
                return knowledgeRing;
            }
        }

        // ---- Sprint 17: the cartoon pass on the gut wall and the vessel ---
        //   The lumen was drawn as a brown chyme channel, which put it in
        //   the same visual family as the thing flowing down it (the
        //   contaminated food bolus is deliberately ochre and lumpy). Lining
        //   the wall with villi and warming the channel separates the two:
        //   the gut reads as living tissue, and the food that arrives in it
        //   reads as the foreign lump it is.

        private static Sprite villus;
        private static Sprite villusAlt;
        /// <summary>One finger of the gut wall's villous fringe, base at the
        /// bottom edge; <c>LumenChannelRenderer</c> rotates each instance so
        /// it projects from the wall into the channel and sways it with the
        /// peristalsis.
        ///
        /// Sprint 19 redraw (docs/LUMEN_REFERENCE.md P1 + P3), following
        /// how textbook cartoons draw a villus:
        ///  - **Epithelial rim / lamina-propria core.** The outer ~3 px is
        ///    full value; the interior drops to 0.78. Under the coral tint
        ///    that is a peach rim around a darker mauve core -- the single
        ///    cue that turns a pink club into a villus.
        ///  - **The lacteal**, a darker central stripe, kept from Sprint 17.
        ///  - **Brush border**: a soft 2 px alpha fuzz *outside* the
        ///    outline, so the edge reads hairy/velvety under the mucus
        ///    sheen. A 1 px comb would alias into noise at ~12 px wide.
        ///  - **Goblet cells**: two small pale ovals sitting in the rim --
        ///    where the mucus sheen comes from.
        /// One tint, so all of this is VALUE; a second hue would cost a
        /// second renderer per villus. <see cref="VillusAlt"/> places the
        /// goblet flecks differently so the row doesn't visibly repeat.</summary>
        public static Sprite Villus => villus ??= BuildVillus(new[] { (-1f, 27f), (1f, 38f) });
        public static Sprite VillusAlt => villusAlt ??= BuildVillus(new[] { (1f, 22f), (-1f, 34f) });

        private static Sprite BuildVillus((float side, float y)[] goblets)
        {
            var b = NewBuffer();
            // A club, not a capsule: narrow where it joins the wall, swelling
            // to a rounded tip (the Sprint 17 silhouette, checked with
            // SpriteShapeDump). Slightly slimmer to leave room for the fuzz.
            for (int i = 0; i <= 24; i++)
            {
                float t = i / 24f;
                float y = Mathf.Lerp(12f, 45f, t);
                float r = Mathf.Lerp(8.5f, 12.5f, Mathf.SmoothStep(0f, 1f, t));
                FillDisc(b, C, y, r);
            }
            var body = (Color[])b.Clone();

            // Epithelium at 0.9 (so the goblet cells, at 1.0, have something
            // to stand out from -- checked in SpriteShapeDump), lamina
            // propria at 0.9 x 0.84 ≈ 0.76 more than 3 px inside the edge.
            Multiply(b, 0.9f, 0.9f, 0.9f, 1f);
            InteriorShade(b, 3, 0.84f);

            // Lacteal: a darker central stripe up the core.
            for (int y = 14; y < 43; y++)
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (b[idx].a <= 0f) continue;
                    float dx = Mathf.Abs(x + 0.5f - C);
                    if (dx < 2.5f)
                    {
                        float k = Mathf.Lerp(0.80f, 0.92f, dx / 2.5f);
                        b[idx].r *= k; b[idx].g *= k; b[idx].b *= k;
                    }
                }

            // Goblet cells: pale ovals in the rim band, value 1.0 against the
            // 0.78 core -- they only read because the core went darker.
            foreach (var (side, gy) in goblets)
            {
                float halfW = Mathf.Lerp(8.5f, 12.5f, Mathf.SmoothStep(0f, 1f, (gy - 12f) / 33f));
                float gx = C + side * (halfW - 3f);
                for (int y = 0; y < Res; y++)
                    for (int x = 0; x < Res; x++)
                    {
                        int idx = y * Res + x;
                        if (b[idx].a <= 0f) continue;
                        float ex = (x + 0.5f - gx) / 2.6f, ey = (y + 0.5f - gy) / 3.6f;
                        if (ex * ex + ey * ey <= 1f) { b[idx].r = 1f; b[idx].g = 1f; b[idx].b = 1f; }
                    }
            }

            // Brush border: a soft halo just outside the outline.
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (body[idx].a > 0.5f || y < 14) continue;   // only past the edge; the base stays clean at the wall
                    float near = 99f;
                    for (int dy = -2; dy <= 2; dy++)
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || nx >= Res || ny < 0 || ny >= Res) continue;
                            if (body[ny * Res + nx].a > 0.5f) near = Mathf.Min(near, Mathf.Sqrt(dx * dx + dy * dy));
                        }
                    if (near > 2.5f) continue;
                    float fuzz = 0.42f * (1f - near / 2.8f) * (0.7f + 0.3f * Hash01(x, y, 77));
                    if (fuzz > b[idx].a) { b[idx].a = fuzz; b[idx].r = 0.95f; b[idx].g = 0.95f; b[idx].b = 0.95f; }
                }
            return Finish(b, 3, shrink: false);
        }

        private static Sprite peyerDome;
        /// <summary>Sprint 19: the Peyer's patch dome -- a low, broad,
        /// smooth hump of follicle-associated epithelium where the wall has
        /// no villi (docs/LUMEN_REFERENCE.md P6). Base along the bottom edge,
        /// so it is placed and rotated exactly like a villus. Bright rim,
        /// darker interior (the subepithelial dome, where DCs wait).</summary>
        public static Sprite PeyerDome
        {
            get
            {
                if (peyerDome == null)
                {
                    var b = NewBuffer();
                    // Half-ellipse: full width, ~0.45 of the height.
                    ForBox(0, 0, Res, Res, (x, y) =>
                    {
                        float cov = Coverage(x, y, (fx, fy) =>
                        {
                            float ex = (fx - C) / 31f, ey = fy / 28f;
                            return fy >= 0f && ex * ex + ey * ey <= 1f;
                        });
                        Blend(b, x, y, cov);
                    });
                    InteriorShade(b, 3, 0.80f);
                    peyerDome = Finish(b, 2, shrink: false);
                }
                return peyerDome;
            }
        }

        private static Sprite mCell;
        /// <summary>Sprint 19: an M cell on the Peyer's dome -- drawn as the
        /// textbook silhouette: a cell with a deep basal pocket (where
        /// lymphocytes and DCs tuck in to receive what it transcytoses). A
        /// rounded cell with a dark notch scooped from its base.</summary>
        public static Sprite MCell
        {
            get
            {
                if (mCell == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C + 2f, 22f, 12f);
                    // The basal pocket: a dark rounded bay opening downward.
                    for (int y = 0; y < Res; y++)
                        for (int x = 0; x < Res; x++)
                        {
                            int idx = y * Res + x;
                            if (b[idx].a <= 0f) continue;
                            float ex = (x + 0.5f - C) / 11f, ey = (y + 0.5f - (C - 14f)) / 16f;
                            if (ex * ex + ey * ey <= 1f) { b[idx].r *= 0.45f; b[idx].g *= 0.45f; b[idx].b *= 0.45f; }
                        }
                    RimShade(b, 2, 0.75f);
                    mCell = ToSprite(b);
                }
                return mCell;
            }
        }

        private static Sprite crypt;
        /// <summary>Sprint 19: a crypt of Lieberkühn (docs/LUMEN_REFERENCE.md
        /// P4) -- a narrow U-shaped pocket of epithelium, mouth at the TOP
        /// edge (the wall), sinking down into tissue. Drawn as an outline:
        /// a thick U wall around a darker hollow lumen, so the host cells
        /// behind it still read through the hollow.</summary>
        public static Sprite Crypt
        {
            get
            {
                if (crypt == null)
                {
                    var b = NewBuffer();
                    // Outer U: a capsule from the bottom up through the top edge.
                    FillCapsule(b, C, 14f, C, 70f, 15f);
                    // Hollow: an inner capsule, alpha cut down so it is a dim lumen.
                    for (int y = 0; y < Res; y++)
                        for (int x = 0; x < Res; x++)
                        {
                            int idx = y * Res + x;
                            if (b[idx].a <= 0f) continue;
                            if (SegDist(x + 0.5f, y + 0.5f, C, 22f, C, 80f) < 7f)
                            { b[idx].a *= 0.35f; b[idx].r *= 0.7f; b[idx].g *= 0.7f; b[idx].b *= 0.7f; }
                        }
                    RimShade(b, 2, 0.70f);
                    crypt = Finish(b, 5, shrink: false);
                }
                return crypt;
            }
        }

        private static Sprite granuleCluster;
        /// <summary>Sprint 19: Paneth-cell granules -- a tight cluster of four
        /// bright dots, tinted rose and placed at the base of a crypt.</summary>
        public static Sprite GranuleCluster
        {
            get
            {
                if (granuleCluster == null)
                {
                    var b = NewBuffer();
                    FillDisc(b, C - 9f, C - 4f, 8f);
                    FillDisc(b, C + 9f, C - 4f, 8f);
                    FillDisc(b, C, C + 9f, 8f);
                    FillDisc(b, C, C - 14f, 7f);
                    RimShade(b, 2, 0.72f);
                    granuleCluster = ToSprite(b);
                }
                return granuleCluster;
            }
        }

        private static Sprite endothelialCell;
        /// <summary>One cell of the vessel wall, tiled along the base/tissue
        /// seam by <c>BaseCompartmentRenderer</c>.
        ///
        /// Sprint 15 drew that wall as a single quad stretched the whole
        /// length of the seam, which meant any detail drawn into it was
        /// stretched with it -- so it could only ever be a smooth bar. A
        /// tiled cell keeps its aspect at any board size, which is what lets
        /// the wall read as *cells* (the thing immune cells squeeze
        /// between) rather than as a painted line.</summary>
        public static Sprite EndothelialCell
        {
            get
            {
                if (endothelialCell == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 27f, 16f);
                    InnerShade(b, C, C, 10f, 0.55f);  // nucleus -- pushed dark enough to read at ~25 px (SpriteShapeDump)
                    RimShade(b, 2, 0.72f);
                    endothelialCell = Finish(b, 3);
                }
                return endothelialCell;
            }
        }

        /// <summary>Optional: touch every accessor so all generation happens
        /// at a chosen point (e.g. bootstrap) rather than on first use.</summary>
        public static void Prewarm()
        {
            _ = Macrophage; _ = Neutrophil; _ = DendriteStar; _ = DendriteStarLoaded;
            _ = Lymphocyte; _ = LargeBacterium; _ = Virion; _ = FoodBolus;
            _ = HostCell; _ = HostCellInfectedViral; _ = HostCellInfectedBacterial; _ = Debris; _ = EmptyPit;
            _ = EpithelialBar;
            _ = GranuleBurst; _ = BreachStar; _ = EffeBloom; _ = StressRing; _ = KnowledgeRing;
            // Sprint 15 compartment shapes
            _ = ChymeField; _ = MucusBand; _ = FlowMote; _ = PlasmaField; _ = VesselWallBar;
            _ = Erythrocyte; _ = BirthPuff; _ = NodeColocGlow;
            // Sprint 17 cartoon pass
            _ = Villus; _ = EndothelialCell;
            // Sprint 23 roster
            _ = CytotoxicT; _ = NaturalKiller; _ = GammaDeltaT; _ = PlasmaCell;
            _ = Antibody; _ = HostCellRecognised;
            // Sprint 19 gut-wall structures
            _ = VillusAlt; _ = PeyerDome; _ = MCell; _ = Crypt; _ = GranuleCluster;
            // Sprint 21 UI + marrow
            _ = IconWaves; _ = IconDroplet; _ = IconBlock; _ = IconBolt; _ = IconHeart; _ = IconStar;
            _ = IconArrowDown; _ = IconPlay; _ = IconClose; _ = HatchTile; _ = BuildPad; _ = SelectRing;
            _ = PlusGlyph; _ = PlateFrame;
        }

        // ================================================================
        // Raster primitives -- pure, operate on a Color[Res*Res] buffer.
        // White RGB throughout; the silhouette is the alpha channel,
        // max-blended so overlapping fills union cleanly.
        // ================================================================

        private static Color[] NewBuffer()
        {
            var buf = new Color[Res * Res];
            for (int i = 0; i < buf.Length; i++) buf[i] = new Color(1f, 1f, 1f, 0f);
            return buf;
        }

        private static Sprite ToSprite(Color[] buf)
        {
            var tex = new Texture2D(Res, Res, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            for (int i = 0; i < buf.Length; i++)
            {
                var c = buf[i];
                buf[i] = new Color(
                    Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), Mathf.Clamp01(c.a));
            }
            tex.SetPixels(buf);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Res, Res), new Vector2(0.5f, 0.5f), Res);
        }

        /// <summary>Sprint 20: finish a unit / structure sprite with a dark
        /// outline <paramref name="strokePx"/> wide, drawn OUTSIDE the
        /// silhouette (<see cref="OuterStroke"/>). To make room inside the
        /// 64 px texture the shape is first shrunk by exactly the stroke
        /// width, so the outlined sprite has the same outer extent as before
        /// and no caller's scale changes. Edge-anchored shapes (villus,
        /// dome, crypt -- their base sits on the texture edge) pass
        /// <paramref name="shrink"/> false so the base stays put.</summary>
        private static Sprite Finish(Color[] buf, int strokePx, bool shrink = true, float strokeValue = 0.18f)
        {
            if (strokePx > 0)
            {
                if (shrink) Shrink(buf, (C - strokePx - 0.5f) / C);
                OuterStroke(buf, strokePx, strokeValue);
            }
            return ToSprite(buf);
        }

        /// <summary>Scale the whole buffer about its centre by
        /// <paramref name="f"/> (&lt;1 = smaller), bilinear.</summary>
        private static void Shrink(Color[] buf, float f)
        {
            var src = (Color[])buf.Clone();
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    float sx = C + (x + 0.5f - C) / f - 0.5f;
                    float sy = C + (y + 0.5f - C) / f - 0.5f;
                    buf[y * Res + x] = Sample(src, sx, sy);
                }
        }

        private static Color Sample(Color[] src, float x, float y)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            Color Px(int px, int py) =>
                px < 0 || px >= Res || py < 0 || py >= Res ? new Color(1f, 1f, 1f, 0f) : src[py * Res + px];
            var a = Color.Lerp(Px(x0, y0), Px(x0 + 1, y0), fx);
            var b = Color.Lerp(Px(x0, y0 + 1), Px(x0 + 1, y0 + 1), fx);
            return Color.Lerp(a, b, fy);
        }

        /// <summary>Sprint 20 (ART_DIRECTION.md §3.1): dilate the silhouette
        /// outward by <paramref name="px"/> and fill that ring at
        /// <paramref name="value"/>. Under the per-renderer multiply tint
        /// this becomes a dark line in the sprite's OWN hue (blue → navy) --
        /// coloured line art, the BTD / Kingdom Rush convention -- at zero
        /// extra renderers. Anti-aliased on its outer edge.</summary>
        private static void OuterStroke(Color[] buf, int px, float value)
        {
            var src = (Color[])buf.Clone();
            int reach = px + 1;
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (src[idx].a >= 0.5f) continue;
                    float near = float.MaxValue;
                    for (int dy = -reach; dy <= reach; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= Res) continue;
                        for (int dx = -reach; dx <= reach; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= Res || src[ny * Res + nx].a < 0.5f) continue;
                            float d = dx * dx + dy * dy;
                            if (d < near) near = d;
                        }
                    }
                    if (near == float.MaxValue) continue;
                    float a = Mathf.Clamp01(px + 0.5f - Mathf.Sqrt(near) + 0.5f);
                    if (a <= 0f) continue;
                    buf[idx] = new Color(value, value, value, Mathf.Max(a, src[idx].a));
                }
        }

        private static void Blend(Color[] buf, int x, int y, float alpha)
        {
            if (x < 0 || x >= Res || y < 0 || y >= Res || alpha <= 0f) return;
            int idx = y * Res + x;
            if (alpha > buf[idx].a) buf[idx].a = alpha;
        }

        /// <summary>4x supersampled coverage of a pixel against an
        /// inside/outside predicate. Boot-time only -- the per-pixel closure
        /// alloc is acceptable here.</summary>
        private static float Coverage(int px, int py, System.Func<float, float, bool> inside)
        {
            int hits = 0;
            for (int sy = 0; sy < 2; sy++)
            {
                for (int sx = 0; sx < 2; sx++)
                {
                    float fx = px + 0.25f + sx * 0.5f;
                    float fy = py + 0.25f + sy * 0.5f;
                    if (inside(fx, fy)) hits++;
                }
            }
            return hits * 0.25f;
        }

        private static void ForBox(float minX, float minY, float maxX, float maxY,
            System.Action<int, int> body)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(minX));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(minY));
            int x1 = Mathf.Min(Res - 1, Mathf.CeilToInt(maxX));
            int y1 = Mathf.Min(Res - 1, Mathf.CeilToInt(maxY));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    body(x, y);
        }

        private static void FillDisc(Color[] buf, float cx, float cy, float r)
        {
            ForBox(cx - r - 1f, cy - r - 1f, cx + r + 1f, cy + r + 1f, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                    (fx - cx) * (fx - cx) + (fy - cy) * (fy - cy) <= r * r);
                Blend(buf, x, y, cov);
            });
        }

        private static void ClearDisc(Color[] buf, float cx, float cy, float r)
        {
            ForBox(cx - r - 1f, cy - r - 1f, cx + r + 1f, cy + r + 1f, (x, y) =>
            {
                float d2 = (x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy);
                if (d2 <= r * r) buf[y * Res + x].a = 0f;
            });
        }

        private static void FillRing(Color[] buf, float cx, float cy, float rOuter, float rInner)
        {
            ForBox(cx - rOuter - 1f, cy - rOuter - 1f, cx + rOuter + 1f, cy + rOuter + 1f, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    float d2 = (fx - cx) * (fx - cx) + (fy - cy) * (fy - cy);
                    return d2 <= rOuter * rOuter && d2 >= rInner * rInner;
                });
                Blend(buf, x, y, cov);
            });
        }

        private static void FillCapsule(Color[] buf, float ax, float ay, float bx, float by, float halfW)
        {
            float minX = Mathf.Min(ax, bx) - halfW - 1f;
            float maxX = Mathf.Max(ax, bx) + halfW + 1f;
            float minY = Mathf.Min(ay, by) - halfW - 1f;
            float maxY = Mathf.Max(ay, by) + halfW + 1f;
            ForBox(minX, minY, maxX, maxY, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) => SegDist(fx, fy, ax, ay, bx, by) <= halfW);
                Blend(buf, x, y, cov);
            });
        }

        private static float SegDist(float px, float py, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float len2 = dx * dx + dy * dy;
            float t = len2 <= 0f ? 0f : Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / len2);
            float qx = ax + t * dx, qy = ay + t * dy;
            return Mathf.Sqrt((px - qx) * (px - qx) + (py - qy) * (py - qy));
        }

        /// <summary>Radial shape with r(theta) = baseR * (1 + depth *
        /// sin(lobes * theta + phase)). Few shallow lobes -> amoeboid;
        /// many deeper -> rosette / bolus.</summary>
        private static void FillLobed(Color[] buf, float cx, float cy, float baseR,
            int lobes, float depth, float phase)
        {
            float rMax = baseR * (1f + Mathf.Abs(depth)) + 1f;
            ForBox(cx - rMax, cy - rMax, cx + rMax, cy + rMax, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    float ang = Mathf.Atan2(fy - cy, fx - cx);
                    float rr = baseR * (1f + depth * Mathf.Sin(lobes * ang + phase));
                    float d2 = (fx - cx) * (fx - cx) + (fy - cy) * (fy - cy);
                    return d2 <= rr * rr;
                });
                Blend(buf, x, y, cov);
            });
        }

        /// <summary>Spiky star: triangular radius wave between rInner (valley)
        /// and rOuter (tip), <paramref name="points"/> spikes.</summary>
        private static void FillStar(Color[] buf, float cx, float cy, float rInner,
            float rOuter, int points, float phase)
        {
            float rMax = rOuter + 1f;
            ForBox(cx - rMax, cy - rMax, cx + rMax, cy + rMax, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    float ang = Mathf.Atan2(fy - cy, fx - cx) + phase;
                    float seg = points * ang / (2f * Mathf.PI);
                    float frac = seg - Mathf.Floor(seg);
                    float tri = Mathf.Abs(2f * frac - 1f);          // 1 at tip, 0 at valley
                    float rr = Mathf.Lerp(rInner, rOuter, tri);
                    float d2 = (fx - cx) * (fx - cx) + (fy - cy) * (fy - cy);
                    return d2 <= rr * rr;
                });
                Blend(buf, x, y, cov);
            });
        }

        /// <summary>Rounded square (SDF), half-extent + corner radius.</summary>
        private static void FillRounded(Color[] buf, float cx, float cy, float halfExtent, float corner)
        {
            float h = halfExtent - corner;
            ForBox(cx - halfExtent - 1f, cy - halfExtent - 1f, cx + halfExtent + 1f, cy + halfExtent + 1f, (x, y) =>
            {
                float cov = Coverage(x, y, (fx, fy) =>
                {
                    float qx = Mathf.Abs(fx - cx) - h;
                    float qy = Mathf.Abs(fy - cy) - h;
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                                               Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f))
                                    + Mathf.Min(Mathf.Max(qx, qy), 0f) - corner;
                    return outside <= 0f;
                });
                Blend(buf, x, y, cov);
            });
        }

        /// <summary>Multiply RGB toward <paramref name="mul"/> at the centre,
        /// easing to 1 at radius <paramref name="r"/>. Only touches already
        /// opaque pixels. <paramref name="mul"/> &gt; 1 brightens (inclusion
        /// body, follicle, trabecula).</summary>
        private static void InnerShade(Color[] buf, float cx, float cy, float r, float mul)
        {
            ForBox(cx - r - 1f, cy - r - 1f, cx + r + 1f, cy + r + 1f, (x, y) =>
            {
                int idx = y * Res + x;
                if (buf[idx].a <= 0f) return;
                float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                if (d >= r) return;
                float f = Mathf.Lerp(mul, 1f, d / r);
                buf[idx].r *= f; buf[idx].g *= f; buf[idx].b *= f;
            });
        }

        /// <summary>Sprint 19: the inverse of <see cref="RimShade"/> -- darken
        /// every opaque pixel MORE than <paramref name="widthPx"/> inside the
        /// edge, leaving a bright rim (an epithelium around a core).</summary>
        private static void InteriorShade(Color[] buf, int widthPx, float mul)
        {
            var src = (Color[])buf.Clone();
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (src[idx].a <= 0.5f) continue;
                    bool interior = true;
                    for (int dy = -widthPx; dy <= widthPx && interior; dy++)
                        for (int dx = -widthPx; dx <= widthPx; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || nx >= Res || ny < 0 || ny >= Res || src[ny * Res + nx].a <= 0.5f)
                            { interior = false; break; }
                        }
                    if (interior) { buf[idx].r *= mul; buf[idx].g *= mul; buf[idx].b *= mul; }
                }
        }

        /// <summary>Darken the outer <paramref name="widthPx"/> band of the
        /// opaque region -- the "membrane" that lifts a small sprite off the
        /// board.</summary>
        private static void RimShade(Color[] buf, int widthPx, float mul)
        {
            var src = (Color[])buf.Clone();
            for (int y = 0; y < Res; y++)
            {
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (src[idx].a <= 0.5f) continue;
                    bool rim = false;
                    for (int dy = -widthPx; dy <= widthPx && !rim; dy++)
                    {
                        for (int dx = -widthPx; dx <= widthPx; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || nx >= Res || ny < 0 || ny >= Res || src[ny * Res + nx].a <= 0.5f)
                            {
                                rim = true;
                                break;
                            }
                        }
                    }
                    if (rim)
                    {
                        buf[idx].r *= mul; buf[idx].g *= mul; buf[idx].b *= mul;
                    }
                }
            }
        }

        /// <summary>Multiply alpha by a hash-noise mask where already opaque:
        /// a fraction <paramref name="density"/> of pixels stay full, the
        /// rest drop to 0.4 -- purulent / granular / rubble texture.</summary>
        private static void Stipple(Color[] buf, int seed, float density)
        {
            for (int y = 0; y < Res; y++)
            {
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (buf[idx].a <= 0f) continue;
                    buf[idx].a *= Hash01(x, y, seed) < density ? 1f : 0.4f;
                }
            }
        }

        /// <summary>Sprint 19: like <see cref="Stipple"/> but darkens VALUE,
        /// leaving alpha alone -- texture without translucency. A fraction
        /// <paramref name="density"/> of opaque pixels drop to
        /// <paramref name="mul"/> brightness.</summary>
        private static void ValueStipple(Color[] buf, int seed, float density, float mul)
        {
            for (int y = 0; y < Res; y++)
            {
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (buf[idx].a <= 0f) continue;
                    if (Hash01(x / 3, y / 3, seed) < density)   // 3px flecks, not single-pixel noise
                    {
                        buf[idx].r *= mul; buf[idx].g *= mul; buf[idx].b *= mul;
                    }
                }
            }
        }

        private static void Multiply(Color[] buf, float r, float g, float b, float a)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i].r *= r; buf[i].g *= g; buf[i].b *= b; buf[i].a *= a;
            }
        }

        // ---- Sprint 15 gradient primitives -----------------------------
        //   These WRITE / SCALE the alpha channel and are O(n) per pixel
        //   (cheaper than the O(n^2) RimShade). No Coverage supersampling --
        //   a gradient has no hard edge to alias. docs/COMPARTMENT_DESIGN.md §3.

        /// <summary>Multiply every already-opaque pixel's alpha by a linear
        /// ramp: <paramref name="aStart"/> at one edge to
        /// <paramref name="aEnd"/> at the opposite edge, along X when
        /// <paramref name="alongX"/>, else along Y. Call AFTER a fill.</summary>
        private static void AxisGradient(Color[] buf, bool alongX, float aStart, float aEnd)
        {
            for (int y = 0; y < Res; y++)
            {
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (buf[idx].a <= 0f) continue;
                    float t = (alongX ? x : y) / (float)(Res - 1);
                    buf[idx].a *= Mathf.Lerp(aStart, aEnd, t);
                }
            }
        }

        /// <summary>Feather alpha from full to zero within
        /// <paramref name="featherPx"/> of ONE border (0=left,1=right,2=top,
        /// 3=bottom). Pixels further from that border are untouched.</summary>
        private static void EdgeGradient(Color[] buf, int edge, float featherPx)
        {
            featherPx = Mathf.Max(1f, featherPx);
            for (int y = 0; y < Res; y++)
            {
                for (int x = 0; x < Res; x++)
                {
                    int idx = y * Res + x;
                    if (buf[idx].a <= 0f) continue;
                    float d;
                    switch (edge)
                    {
                        case 0: d = x; break;
                        case 1: d = Res - 1 - x; break;
                        case 2: d = y; break;
                        default: d = Res - 1 - y; break;
                    }
                    if (d < featherPx) buf[idx].a *= d / featherPx;
                }
            }
        }

        /// <summary>Overwrite alpha with a smoothstep-eased lerp from
        /// <paramref name="aInner"/> at <paramref name="rInner"/> to
        /// <paramref name="aOuter"/> at <paramref name="rOuter"/> from
        /// (cx,cy). Writes (not max-blends), so it works on an empty
        /// buffer.</summary>
        private static void RadialGradient(Color[] buf, float cx, float cy,
            float rInner, float rOuter, float aInner, float aOuter)
        {
            float rIn = Mathf.Min(rInner, rOuter);
            float rOut = Mathf.Max(rInner, rOuter);
            ForBox(cx - rOut - 1f, cy - rOut - 1f, cx + rOut + 1f, cy + rOut + 1f, (x, y) =>
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float t = rOut <= rIn ? 1f : Mathf.Clamp01((d - rIn) / (rOut - rIn));
                t = t * t * (3f - 2f * t); // smoothstep
                buf[y * Res + x].a = Mathf.Lerp(aInner, aOuter, t);
            });
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 362437);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFFu) / (float)0x1000000;
            }
        }
    }
}
