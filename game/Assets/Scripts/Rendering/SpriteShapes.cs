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
    public static class SpriteShapes
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
                    macrophage = ToSprite(b);
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
                    neutrophil = ToSprite(b);
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
                    dendriteStar = ToSprite(b);
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
                    dendriteStarLoaded = ToSprite(b);
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
                    lymphocyte = ToSprite(b);
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
                    FillCapsule(b, 16f, C, 48f, C, 8f); // horizontal rod, caller rotates per instance
                    RimShade(b, 2, 0.50f);
                    largeBacterium = ToSprite(b);
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
                    FillDisc(b, C, C, 13f); // small -- ~60% of the bacterium footprint
                    RimShade(b, 1, 0.50f);
                    virion = ToSprite(b);
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
                    FillLobed(b, C, C, 27f, 6, 0.28f, 0.9f); // lumpier than a macrophage
                    Stipple(b, 4242, 0.30f);
                    RimShade(b, 2, 0.60f);
                    foodBolus = ToSprite(b);
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
                    Stipple(b, 24601, 0.62f);         // purulent
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
                    Stipple(b, 1337, 0.5f);
                    RimShade(b, 2, 0.55f);
                    debris = ToSprite(b);
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
                    Multiply(b, 1f, 1f, 1f, 0.18f);
                    emptyPit = ToSprite(b);
                }
                return emptyPit;
            }
        }

        private static Sprite slotNiche;
        public static Sprite SlotNiche
        {
            get
            {
                if (slotNiche == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 30f, 6f);
                    InnerShade(b, C, C, 24f, 0.62f); // recessed
                    RimShade(b, 2, 0.80f);
                    slotNiche = ToSprite(b);
                }
                return slotNiche;
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

        private static Sprite marrowRegion;
        /// <summary>Sprint 15: struts raised and multiplied so the sponge
        /// reads at a glance (not only up close), two faint sinusoid
        /// channels toward the blood-side edge (the route emitted cells and
        /// birth-puff motes take), lighter stipple so the struts survive.
        /// Retinted to red marrow in <c>GameBootstrap</c>.</summary>
        public static Sprite MarrowRegion
        {
            get
            {
                if (marrowRegion == null)
                {
                    var b = NewBuffer();
                    FillRounded(b, C, C, 31f, 4f);
                    var rng = new System.Random(7);
                    for (int i = 0; i < 5; i++) // bold lighter trabecular struts
                    {
                        float ox = C + (float)(rng.NextDouble() - 0.5) * 44f;
                        float oy = C + (float)(rng.NextDouble() - 0.5) * 44f;
                        InnerShade(b, ox, oy, 12f, 1.35f);
                    }
                    for (int i = 0; i < 3; i++) // thinner secondary struts
                    {
                        float ox = C + (float)(rng.NextDouble() - 0.5) * 46f;
                        float oy = C + (float)(rng.NextDouble() - 0.5) * 46f;
                        InnerShade(b, ox, oy, 7f, 1.30f);
                    }
                    // sinusoid channels draining toward the wall-side (x = Res) edge
                    FillCapsule(b, 40f, 20f, 62f, 26f, 2.4f);
                    FillCapsule(b, 38f, 44f, 62f, 40f, 2.4f);
                    InnerShade(b, 52f, 23f, 12f, 0.80f);
                    InnerShade(b, 52f, 42f, 12f, 0.80f);
                    Stipple(b, 55, 0.62f);
                    marrowRegion = ToSprite(b);
                }
                return marrowRegion;
            }
        }

        private static Sprite lymphNodeBean;
        /// <summary>Sprint 15: keeps the Sprint 13 bean outline; adds a
        /// darker medullary notch on the concave side (the hilum where
        /// vessels enter) and a third germinal-centre zone.</summary>
        public static Sprite LymphNodeBean
        {
            get
            {
                if (lymphNodeBean == null)
                {
                    var b = NewBuffer();
                    FillLobed(b, C, C, 26f, 2, 0.18f, 0.4f); // rounded bean
                    InnerShade(b, C - 9f, C + 5f, 8f, 1.18f); // follicles
                    InnerShade(b, C + 10f, C - 4f, 7f, 1.18f);
                    InnerShade(b, C + 2f, C + 9f, 6f, 1.15f); // third germinal centre
                    InnerShade(b, C - 2f, C - 10f, 9f, 0.72f); // medullary notch / hilum
                    RimShade(b, 1, 0.85f);
                    lymphNodeBean = ToSprite(b);
                }
                return lymphNodeBean;
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
                    Stipple(b, 6011, 0.85f);
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
                    Stipple(b, 6067, 0.90f);
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

        private static Sprite organHalo;
        /// <summary>Seats the marrow / lymph backdrops in the plasma: an
        /// annulus of darker plasma (clear centre so the backdrop shows) with
        /// a bright rim at the organ edge. <c>GameBootstrap</c> builds one per
        /// organ, <c>sortingOrder 1</c>, behind the backdrops (which move to 2).</summary>
        public static Sprite OrganHalo
        {
            get
            {
                if (organHalo == null)
                {
                    var b = NewBuffer();
                    ForBox(0f, 0f, Res, Res, (x, y) =>
                    {
                        float d = Mathf.Sqrt((x + 0.5f - C) * (x + 0.5f - C) + (y + 0.5f - C) * (y + 0.5f - C));
                        float a = d < 19f
                            ? Mathf.Lerp(0f, 0.78f, Mathf.Clamp01((d - 8f) / 11f))
                            : Mathf.Lerp(0.78f, 0f, Mathf.Clamp01((d - 19f) / 12f));
                        b[y * Res + x].a = a;
                    });
                    FillRing(b, C, C, 21f, 19f); // bright rim where organ meets fluid
                    organHalo = ToSprite(b);
                }
                return organHalo;
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
                    erythrocyte = ToSprite(b);
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
        /// <summary>One intestinal villus -- the finger of mucosa that makes
        /// a gut lumen look like a gut and not a pipe. Drawn pointing **+Y**
        /// with its base at the bottom edge; <c>LumenChannelRenderer</c>
        /// rotates each instance so it projects from the wall into the
        /// channel, and sways it with the peristalsis phase.
        ///
        /// The darker central seam is the lacteal / capillary core -- it is
        /// what stops a row of these reading as a row of identical blobs,
        /// and it costs one loop.</summary>
        public static Sprite Villus
        {
            get
            {
                if (villus == null)
                {
                    var b = NewBuffer();
                    // A club, not a capsule: narrow where it joins the wall,
                    // swelling to a rounded tip. Drawn as a stack of discs
                    // because the taper is the whole silhouette -- a uniform
                    // capsule reads as a finger of anything, and a row of
                    // them reads as a comb. (Checked with
                    // Assets/Editor/SpriteShapeDump.cs, which is why this is
                    // the second version of the shape.)
                    for (int i = 0; i <= 24; i++)
                    {
                        float t = i / 24f;
                        float y = Mathf.Lerp(12f, 46f, t);
                        float r = Mathf.Lerp(6.5f, 12.5f, Mathf.SmoothStep(0f, 1f, t));
                        FillDisc(b, C, y, r);
                    }
                    // Central core, slightly darker than the body.
                    for (int y = 0; y < Res; y++)
                        for (int x = 0; x < Res; x++)
                        {
                            int idx = y * Res + x;
                            if (b[idx].a <= 0f) continue;
                            float dx = Mathf.Abs(x + 0.5f - C);
                            if (dx < 3f && y > 12 && y < 44)
                            {
                                float k = Mathf.Lerp(0.84f, 1f, dx / 3f);
                                b[idx].r *= k; b[idx].g *= k; b[idx].b *= k;
                            }
                        }
                    RimShade(b, 1, 0.76f);
                    // Velvet: the tip feathers a little, the base stays solid
                    // where it meets the wall.
                    AxisGradient(b, alongX: false, aStart: 1f, aEnd: 0.90f);
                    villus = ToSprite(b);
                }
                return villus;
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
                    endothelialCell = ToSprite(b);
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
            _ = SlotNiche; _ = EpithelialBar; _ = MarrowRegion; _ = LymphNodeBean;
            _ = GranuleBurst; _ = BreachStar; _ = EffeBloom; _ = StressRing; _ = KnowledgeRing;
            // Sprint 15 compartment shapes
            _ = ChymeField; _ = MucusBand; _ = FlowMote; _ = PlasmaField; _ = VesselWallBar;
            _ = OrganHalo; _ = Erythrocyte; _ = BirthPuff; _ = NodeColocGlow;
            // Sprint 17 cartoon pass
            _ = Villus; _ = EndothelialCell;
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
