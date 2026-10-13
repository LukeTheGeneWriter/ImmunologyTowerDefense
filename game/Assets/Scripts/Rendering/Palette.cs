using UnityEngine;

namespace ImmunologyTD.Rendering
{
    public enum Look { Classic, Daylight }

    /// <summary>
    /// Sprint 20: every world colour in the game, in one place, in two looks
    /// (docs/ART_DIRECTION.md §3.2, §4 #0).
    ///
    /// **Classic** is the game as it shipped through Sprint 19: dark
    /// grounds and muted units, the "histology plate". **Daylight** is the
    /// art-direction proposal: light, low-saturation grounds; saturated
    /// units with dark outlines; pathogens the loudest things on the board.
    /// The Director chose "toggle first, then decide", so both live here.
    /// F10 in a build flips the look and reloads the scene
    /// (<see cref="ToggleAndReload"/>).
    ///
    /// Before this the same colour was typed in several files (DC magenta
    /// twice, helper-T teal three times). Every consumer now reads a
    /// property here. Values are sRGB as written, since the project is in
    /// Gamma colour space. Properties, not fields, so a look change needs no
    /// re-initialisation; each is a struct return with no allocation.
    ///
    /// **`DegranulationFlash.ShapeFor` picks a flash's shape by exact colour
    /// match**, so the five FX colours must stay pairwise distinct in BOTH
    /// looks. Sprint20Verification asserts it.
    /// </summary>
    public static class Palette
    {
        private const string PrefKey = "ImmunologyTD.Look";

        /// <summary>The active look. Default Daylight: it is the thing being
        /// evaluated, and Classic is one key away.</summary>
        public static Look Current = Look.Daylight;
        public static bool Daylight => Current == Look.Daylight;

        /// <summary>Daylight sprites carry a baked dark outline
        /// (<c>SpriteShapes.OuterStroke</c>); Classic keeps the old thin rims.</summary>
        public static bool Outlines => Daylight;

        /// <summary>Daylight draws pathogens at a minimum on-screen size
        /// (Director D5); positions and footprints are unchanged.</summary>
        public static bool PathogenSizeFloor => Daylight;

        private static Color P(Color classic, Color daylight) => Daylight ? daylight : classic;
        private static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

        /// <summary>Read the persisted look (call before any sprite is built).</summary>
        public static void Load()
        {
            try { Current = PlayerPrefs.GetInt(PrefKey, (int)Look.Daylight) == (int)Look.Classic ? Look.Classic : Look.Daylight; }
            catch { Current = Look.Daylight; }
        }

        /// <summary>F10: flip the look, persist it, drop every cached
        /// sprite (they bake the outline), and reload the scene so every
        /// renderer is rebuilt from scratch. Loses the current run -- it is a
        /// comparison tool, not a setting.</summary>
        public static void ToggleAndReload()
        {
            Current = Daylight ? Look.Classic : Look.Daylight;
            try { PlayerPrefs.SetInt(PrefKey, (int)Current); PlayerPrefs.Save(); } catch { }
            SpriteShapes.ClearCache();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // ---- frame ------------------------------------------------------
        public static Color CameraClear   => P(C(0.05f, 0.05f, 0.07f), C(0.18f, 0.13f, 0.17f));
        /// <summary>Daylight only: a quad under the tissue grid so the seams
        /// between rounded host cells read as pink mortar, not black lines.</summary>
        public static Color TissueUnderlay => C(0.93f, 0.70f, 0.70f);

        // ---- host cells (BoardRenderer) ---------------------------------
        public static Color HostHealthy       => P(C(0.80f, 0.62f, 0.66f), C(0.99f, 0.86f, 0.82f));
        public static Color HostInfectedViral => P(C(0.54f, 0.36f, 0.60f), C(0.80f, 0.66f, 0.95f));
        public static Color HostInfectedBact  => P(C(0.62f, 0.60f, 0.26f), C(0.88f, 0.96f, 0.50f));
        public static Color Debris            => P(C(0.38f, 0.34f, 0.28f), C(0.66f, 0.58f, 0.60f));
        public static Color EmptyGround       => P(C(0.13f, 0.11f, 0.12f), C(0.42f, 0.26f, 0.32f));
        public static Color BaseBand          => P(C(0.20f, 0.21f, 0.34f), C(0.94f, 0.50f, 0.52f));
        public static Color LumenBand         => P(C(0.19f, 0.16f, 0.11f), C(0.80f, 0.91f, 0.95f));
        public static Color CytokineHot       => C(1.00f, 0.55f, 0.05f);

        // ---- units ------------------------------------------------------
        public static Color Macrophage   => P(C(0.30f, 0.40f, 0.80f), C(0.22f, 0.48f, 0.96f));
        public static Color Neutrophil   => P(C(0.93f, 0.74f, 0.30f), C(1.00f, 0.80f, 0.12f));
        public static Color Dendritic    => P(C(0.72f, 0.30f, 0.68f), C(0.92f, 0.22f, 0.72f));
        /// <summary>A loaded DC. Classic brightens it; on a light ground
        /// lighter is quieter, so Daylight keeps the hot body (the loaded
        /// sprite's shape carries the state) and only lifts it a touch.</summary>
        public static Color DendriticLoaded => P(C(0.98f, 0.62f, 0.98f), C(1.00f, 0.42f, 0.86f));
        public static Color HelperT       => P(C(0.32f, 0.72f, 0.70f), C(0.10f, 0.74f, 0.68f));
        public static Color HelperTPaired => P(C(0.82f, 0.94f, 0.92f), C(0.45f, 0.95f, 0.85f));

        // ---- pathogens + bolus ------------------------------------------
        /// <summary>Bacteria (both classes). Director D2: maroon → germ green.</summary>
        public static Color Bacterium     => P(C(0.42f, 0.12f, 0.16f), C(0.36f, 0.76f, 0.12f));
        public static Color Virion        => P(C(0.40f, 0.16f, 0.34f), C(0.58f, 0.20f, 0.92f));
        public static Color PathogenHit   => C(0.95f, 0.85f, 0.30f);
        public static Color FoodBolus     => P(C(0.55f, 0.47f, 0.28f), C(0.64f, 0.44f, 0.20f));

        // ---- base band, marrow, node --------------------------------------
        public static Color MarrowBackdrop => P(C(0.34f, 0.22f, 0.18f), C(1.00f, 0.88f, 0.80f));
        public static Color MarrowSlotEmpty => P(C(0.62f, 0.56f, 0.42f), C(0.80f, 0.85f, 0.93f));
        public static Color SelectionRim   => C(0.431f, 0.561f, 0.690f);
        public static Color NodeBackdrop   => P(C(0.34f, 0.40f, 0.28f), C(0.82f, 0.92f, 0.76f));
        public static Color OrganHalo      => P(C(0.20f, 0.06f, 0.09f), C(0.60f, 0.20f, 0.26f, 0.6f));
        public static Color Plasma         => P(C(0.33f, 0.11f, 0.15f), C(0.94f, 0.50f, 0.52f));
        public static Color VesselWall     => P(C(0.78f, 0.50f, 0.52f), C(1.00f, 0.80f, 0.78f));
        /// <summary>Includes alpha (Classic 0.9).</summary>
        public static Color Erythrocyte    => P(C(0.72f, 0.20f, 0.22f, 0.9f), C(0.86f, 0.12f, 0.18f, 1f));
        public static Color BirthPuff      => P(C(0.70f, 0.62f, 0.50f), C(1.00f, 1.00f, 1.00f, 0.8f));
        public static Color BreachFlash    => P(C(0.86f, 0.10f, 0.12f), C(0.80f, 0.04f, 0.10f));
        public static Color NodeColocGlow  => P(C(0.55f, 0.85f, 0.85f), C(0.20f, 0.80f, 0.80f));

        // ---- gut wall + lumen --------------------------------------------
        public static Color GutWallQuiet   => P(C(0.50f, 0.46f, 0.37f), C(0.98f, 0.68f, 0.60f));
        public static Color GutWallAlarm   => C(0.95f, 0.30f, 0.20f);
        public static Color Chyme          => P(C(0.36f, 0.22f, 0.24f), C(0.80f, 0.91f, 0.95f));
        /// <summary>Includes alpha.</summary>
        public static Color Mucus          => P(C(0.88f, 0.84f, 0.76f, 0.20f), C(1.00f, 1.00f, 1.00f, 0.35f));
        public static Color FlowMote       => P(C(0.60f, 0.52f, 0.40f), C(0.55f, 0.74f, 0.82f, 0.7f));
        public static Color Villus         => P(C(0.80f, 0.50f, 0.47f), C(1.00f, 0.62f, 0.57f));
        /// <summary>Includes alpha. Daylight's back row is LIGHTER than the
        /// front -- atmospheric depth on a light ground.</summary>
        public static Color VillusBack     => P(C(0.62f, 0.40f, 0.42f, 0.70f), C(0.98f, 0.78f, 0.75f, 1f));
        public static Color PeyerDome      => P(C(0.86f, 0.58f, 0.55f), C(1.00f, 0.70f, 0.64f));
        public static Color MCell          => P(C(0.78f, 0.74f, 0.82f), C(0.88f, 0.82f, 0.98f));
        public static Color Follicle       => P(C(0.32f, 0.72f, 0.70f), C(0.10f, 0.74f, 0.68f));
        public static float FollicleAlpha  => Daylight ? 0.25f : 0.30f;
        public static Color CryptPocket    => P(C(0.86f, 0.68f, 0.70f), C(0.96f, 0.74f, 0.74f));
        public static Color Paneth         => P(C(0.95f, 0.60f, 0.66f), C(1.00f, 0.45f, 0.55f));

        // ---- FX (keep pairwise distinct -- see class comment) -------------
        public static Color FxDegranulation => P(C(1.00f, 0.97f, 0.72f), C(1.00f, 0.78f, 0.10f));
        public static Color FxBreach        => P(C(1.00f, 0.35f, 0.22f), C(1.00f, 0.30f, 0.15f));
        public static Color FxEfferocytosis => P(C(0.45f, 0.80f, 0.68f), C(0.55f, 0.95f, 0.85f));
        public static Color FxStressKill    => P(C(0.95f, 0.40f, 0.80f), C(1.00f, 0.25f, 0.70f));
        /// <summary>Daylight moves this off green, which is now the bacterium.</summary>
        public static Color FxKnowledge     => P(C(0.40f, 0.92f, 0.45f), C(0.15f, 0.80f, 1.00f));
    }
}
