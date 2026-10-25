using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Every world colour in the game, in one place (docs/ART_DIRECTION.md
    /// §3.2, the "daylight cartoon" direction).
    ///
    /// Sprint 20 shipped this with two looks -- Classic (the dark
    /// "histology plate" the game had through Sprint 19) and Daylight -- and
    /// an F10 toggle, so the Director could choose side by side. **He chose
    /// Daylight (2026-10-14: "That looks great"), and Sprint 21 retired
    /// Classic.** The old values are in git history (Sprint 20's commit) if
    /// they are ever wanted.
    ///
    /// Rules that come with the look (UI_STYLE_GUIDE.md): light,
    /// low-saturation grounds; saturated units with a baked dark outline
    /// (<c>SpriteShapes.OuterStroke</c>); pathogens the loudest things on
    /// the board. Values are sRGB as written (Gamma colour space).
    /// Properties, so a consumer never caches a stale value; each is a
    /// struct return, no allocation.
    ///
    /// **`DegranulationFlash.ShapeFor` picks a flash's shape by exact colour
    /// match**, so the five FX colours must stay pairwise distinct.
    /// Sprint20Verification asserts it. New colours go HERE, never inline.
    /// </summary>
    public static class Palette
    {
        private static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

        // ---- frame ------------------------------------------------------
        public static Color CameraClear   => C(0.18f, 0.13f, 0.17f);
        /// <summary>A quad under the tissue grid so the seams
        /// between rounded host cells read as pink mortar, not black lines.</summary>
        public static Color TissueUnderlay => C(0.93f, 0.70f, 0.70f);

        // ---- host cells (BoardRenderer) ---------------------------------
        public static Color HostHealthy       => C(0.99f, 0.86f, 0.82f);
        public static Color HostInfectedViral => C(0.80f, 0.66f, 0.95f);
        public static Color HostInfectedBact  => C(0.88f, 0.96f, 0.50f);
        public static Color Debris            => C(0.66f, 0.58f, 0.60f);
        public static Color EmptyGround       => C(0.42f, 0.26f, 0.32f);
        public static Color BaseBand          => C(0.94f, 0.50f, 0.52f);
        public static Color LumenBand         => C(0.80f, 0.91f, 0.95f);
        public static Color CytokineHot       => C(1.00f, 0.55f, 0.05f);

        // ---- units ------------------------------------------------------
        public static Color Macrophage   => C(0.22f, 0.48f, 0.96f);
        public static Color Neutrophil   => C(1.00f, 0.80f, 0.12f);
        public static Color Dendritic    => C(0.92f, 0.22f, 0.72f);
        /// <summary>A loaded DC: the hot body lifted a touch (lighter is
        /// quieter on a light ground; the loaded sprite's shape carries the state).</summary>
        public static Color DendriticLoaded => C(1.00f, 0.42f, 0.86f);
        public static Color HelperT       => C(0.10f, 0.74f, 0.68f);
        public static Color HelperTPaired => C(0.45f, 0.95f, 0.85f);

        // ---- pathogens + bolus ------------------------------------------
        /// <summary>Bacteria (both classes). Director D2: maroon → germ green.</summary>
        public static Color Bacterium     => C(0.36f, 0.76f, 0.12f);
        public static Color Virion        => C(0.58f, 0.20f, 0.92f);
        public static Color PathogenHit   => C(0.95f, 0.85f, 0.30f);
        public static Color FoodBolus     => C(0.64f, 0.44f, 0.20f);

        // ---- base band, marrow, node --------------------------------------
        /// <summary>Sprint 21 organs (COSMETIC_PASS_SPEC.md §1.1).</summary>
        public static Color Bone           => C(1.00f, 0.94f, 0.80f);
        public static Color MarrowSpace    => C(1.00f, 0.62f, 0.52f);
        public static Color PadSteel       => C(0.80f, 0.85f, 0.93f);
        public static Color SelectionRim   => C(0.431f, 0.561f, 0.690f);
        public static Color NodeBody       => C(0.80f, 0.93f, 0.78f);
        /// <summary>Lymph-node B-cell follicles (apricot) -- not the Peyer's
        /// patch follicle haze, which is <see cref="Follicle"/>.</summary>
        public static Color NodeFollicle   => C(1.00f, 0.84f, 0.62f);
        public static Color Lymph          => C(1.00f, 0.95f, 0.70f);
        /// <summary>Cartoon drop shadows under the organs (replaces the halo).</summary>
        public static Color OrganShadow    => C(0.50f, 0.12f, 0.20f, 0.40f);
        public static Color Plasma         => C(0.94f, 0.50f, 0.52f);
        public static Color VesselWall     => C(1.00f, 0.80f, 0.78f);
        public static Color Erythrocyte    => C(0.86f, 0.12f, 0.18f, 1f);
        public static Color BirthPuff      => C(1.00f, 1.00f, 1.00f, 0.8f);
        public static Color BreachFlash    => C(0.80f, 0.04f, 0.10f);
        public static Color NodeColocGlow  => C(0.20f, 0.80f, 0.80f);

        // ---- gut wall + lumen --------------------------------------------
        public static Color GutWallQuiet   => C(0.98f, 0.68f, 0.60f);
        public static Color GutWallAlarm   => C(0.95f, 0.30f, 0.20f);
        public static Color Chyme          => C(0.80f, 0.91f, 0.95f);
        /// <summary>Includes alpha.</summary>
        public static Color Mucus          => C(1.00f, 1.00f, 1.00f, 0.35f);

        /// <summary>Sprint 22: secretory IgA. The mucus family pushed COOL,
        /// so a coated organism reads against both the green bacterium and
        /// the violet virion without colliding with any reserved hue.</summary>
        public static Color IgaCoat        => C(0.86f, 0.94f, 1.00f, 0.85f);

        /// <summary>Sprint 22: a slough wave -- the mucus glaze at full
        /// brightness, because a sentinel goblet cell dumping its whole load
        /// is the glaze arriving all at once.</summary>
        public static Color MucusSlough    => C(1.00f, 1.00f, 1.00f, 0.75f);

        /// <summary>Sprint 22: epithelium that has been killed. Desaturated
        /// and darkened from GutWallQuiet rather than given a new hue -- dead
        /// wall is the same tissue, just gone.</summary>
        public static Color GutWallDead    => C(0.55f, 0.43f, 0.42f);
        public static Color FlowMote       => C(0.55f, 0.74f, 0.82f, 0.7f);
        public static Color Villus         => C(1.00f, 0.62f, 0.57f);
        /// <summary>Includes alpha. The back row is LIGHTER than the front --
        /// atmospheric depth on a light ground.</summary>
        public static Color VillusBack     => C(0.98f, 0.78f, 0.75f, 1f);
        public static Color PeyerDome      => C(1.00f, 0.70f, 0.64f);
        public static Color MCell          => C(0.88f, 0.82f, 0.98f);
        public static Color Follicle       => C(0.10f, 0.74f, 0.68f);
        public static float FollicleAlpha  => 0.25f;
        public static Color CryptPocket    => C(0.96f, 0.74f, 0.74f);
        public static Color Paneth         => C(1.00f, 0.45f, 0.55f);

        /// <summary>Sprint 23: a free antibody drifting in tissue.</summary>
        public static Color Antibody       => C(0.72f, 0.84f, 1.00f);

        /// <summary>Sprint 27: fatty (yellow) marrow -- a niche that has not
        /// been reconverted to haematopoietic marrow yet. Real yellow
        /// marrow is yellow because it is fat, so here the accurate colour
        /// and the readable one happen to be the same: dormant capacity,
        /// clearly not a steel build pad and clearly not damage.</summary>
        public static Color MarrowFat      => C(0.95f, 0.86f, 0.55f);

        // ---- FX (keep pairwise distinct -- see class comment) -------------
        public static Color FxDegranulation => C(1.00f, 0.78f, 0.10f);
        public static Color FxBreach        => C(1.00f, 0.30f, 0.15f);
        public static Color FxEfferocytosis => C(0.55f, 0.95f, 0.85f);
        public static Color FxStressKill    => C(1.00f, 0.25f, 0.70f);
        /// <summary>Sky cyan -- off green, which is the bacterium.</summary>
        public static Color FxKnowledge     => C(0.15f, 0.80f, 1.00f);

        /// <summary>Sprint 23: an APOPTOTIC kill -- a cytotoxic cell
        /// inducing programmed death in an infected cell. Pale
        /// lavender-white, deliberately the calmest burst on the board: the
        /// visual opposite of <see cref="FxStressKill"/>'s violent magenta.
        /// The contrast between the two is the innate/adaptive distinction
        /// as the player experiences it -- the same event, one of them
        /// tidy and one of them a mess.</summary>
        public static Color FxApoptosis     => C(0.86f, 0.82f, 1.00f);

        /// <summary>Sprint 23: an NK/ILC1 cell's IFN-gamma pulse activating
        /// the macrophages around it. Warm amber-white -- it is a BUFF, so
        /// it must not read as a kill; every other burst on the board is
        /// something dying.</summary>
        public static Color FxIfnGamma      => C(1.00f, 0.88f, 0.55f);

        /// <summary>Sprint 23: an antibody landing on a pathogen. Close to
        /// <see cref="IgaCoat"/> deliberately -- tissue antibody and luminal
        /// IgA are the same molecule doing the same job in two compartments,
        /// and the player should read them as one idea.</summary>
        public static Color FxOpsonisation  => C(0.80f, 0.90f, 1.00f);

        /// <summary>Sprint 23: a gamma-delta IEL mending the barrier. Fresh
        /// green -- the only burst on the board that means something was
        /// BUILT rather than destroyed.</summary>
        public static Color FxRepair        => C(0.45f, 0.90f, 0.55f);

        /// <summary>Sprint 29: a pairing rescued by MEMORY rather than by
        /// barcode luck. Deeper and warmer than the ordinary knowledge
        /// flash, because "we have met this before" should feel different
        /// from "we got lucky".</summary>
        public static Color FxMemoryRecall  => C(0.55f, 0.45f, 1.00f);
    }
}
