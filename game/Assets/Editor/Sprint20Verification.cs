using UnityEngine;
using ImmunologyTD.Rendering;

/// <summary>
/// Sprint 20 verification, revised Sprint 21 when the Director picked
/// Daylight and Classic was retired (docs/ART_DIRECTION.md).
///
///  1. The old public colour statics forward to the palette.
///  2. The five FX colours are pairwise distinct --
///     DegranulationFlash.ShapeFor picks a flash's shape by exact colour.
///  3. The legibility fix the art pass measured: under the old (Classic)
///     palette the bacterium and virion were 1.13:1 and 1.04:1 against the
///     lumen. Now their outlines are far above that, and their fill differs
///     from the lumen in saturation and hue.
///  4. Sprites carry a dark outline at the pre-outline outer extent (the
///     shape shrinks to make room; the macrophage was 30.0 px).
///
/// Run (Linux): tools/unity.sh Sprint20Verification.RunAll
/// </summary>
public static class Sprint20Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint20Verification] Starting ...");

        RunForwarding();
        RunFxDistinct();
        RunContrast();
        RunOutlines();

        Debug.Log($"[Sprint20Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint20Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint20Verification] FAIL -- {label}"); }
    }

    private static bool Eq(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 1e-4f && Mathf.Abs(a.g - b.g) < 1e-4f && Mathf.Abs(a.b - b.b) < 1e-4f;

    private static void RunForwarding()
    {
        Check("BoardRenderer.PathogenColor reads the palette", Eq(BoardRenderer.PathogenColor, Palette.Bacterium));
        Check("DegranulationFlash.KnowledgeMatchColor reads the palette", Eq(DegranulationFlash.KnowledgeMatchColor, Palette.FxKnowledge));
    }

    private static void RunFxDistinct()
    {
        var fx = new[] { Palette.FxDegranulation, Palette.FxBreach, Palette.FxEfferocytosis, Palette.FxStressKill, Palette.FxKnowledge };
        bool distinct = true;
        for (int i = 0; i < fx.Length; i++)
            for (int j = i + 1; j < fx.Length; j++)
                if (Mathf.Abs(fx[i].r - fx[j].r) < 0.01f && Mathf.Abs(fx[i].g - fx[j].g) < 0.01f && Mathf.Abs(fx[i].b - fx[j].b) < 0.01f)
                    distinct = false;
        Check("the five FX colours are pairwise distinct (ShapeFor matches by colour)", distinct);
    }

    private static float Lum(Color c)
    {
        float F(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * F(c.r) + 0.7152f * F(c.g) + 0.0722f * F(c.b);
    }

    private static float Contrast(Color a, Color b)
    {
        float x = Lum(a), y = Lum(b);
        return (Mathf.Max(x, y) + 0.05f) / (Mathf.Min(x, y) + 0.05f);
    }

    private static Color Outline(Color tint) => new Color(tint.r * 0.18f, tint.g * 0.18f, tint.b * 0.18f);

    private static void RunContrast()
    {
        // The retired Classic values, kept only as the baseline the fix is measured against.
        var oldChyme = new Color(0.36f, 0.22f, 0.24f);
        float cb = Contrast(new Color(0.42f, 0.12f, 0.16f), oldChyme), cv = Contrast(new Color(0.40f, 0.16f, 0.34f), oldChyme);

        float db = Contrast(Outline(Palette.Bacterium), Palette.Chyme);
        float dv = Contrast(Outline(Palette.Virion), Palette.Chyme);
        float dbol = Contrast(Outline(Palette.FoodBolus), Palette.Chyme);
        Debug.Log($"[Sprint20Verification] pathogen vs lumen: was bacterium {cb:F2}, virion {cv:F2}; now outline bacterium {db:F1}, virion {dv:F1}, bolus {dbol:F1}");
        Check("the baseline reproduces the measured problem (< 1.3:1)", cb < 1.3f && cv < 1.3f);
        Check("the bacterium's outline is >= 7:1 on the lumen", db >= 7f);
        Check("the virion's outline is >= 7:1 on the lumen", dv >= 7f);
        Check("the bolus's outline is >= 7:1 on the lumen", dbol >= 7f);

        Color.RGBToHSV(Palette.Bacterium, out float hb, out float sb, out _);
        Color.RGBToHSV(Palette.Virion, out _, out float sv, out _);
        Color.RGBToHSV(Palette.Chyme, out _, out float sc, out _);
        Check("pathogens are far more saturated than the lumen", sb - sc > 0.5f && sv - sc > 0.5f);
        Check("the bacterium is green (the germ convention, D2)", hb > 0.2f && hb < 0.4f);
    }

    private static void RunOutlines()
    {
        var mac = Measure(SpriteShapes.Macrophage);
        var neu = Measure(SpriteShapes.Neutrophil);
        var vir = Measure(SpriteShapes.Virion);
        Debug.Log($"[Sprint20Verification] macrophage extent {mac.extent:F1}, edge value {mac.edgeValue:F2}");
        Check("the macrophage has a dark outline (edge value < 0.25)", mac.edgeValue < 0.25f);
        Check("the neutrophil has a dark outline", neu.edgeValue < 0.25f);
        Check("the virion has a dark outline", vir.edgeValue < 0.25f);
        Check("outlining kept the macrophage's pre-outline extent (30.0 px)", Mathf.Abs(mac.extent - 30f) <= 2.5f);
        Check("nothing clips the texture edge", mac.extent < 31.5f && vir.extent < 31.5f && neu.extent < 31.5f);
    }
    /// <summary>Outer extent (max distance from centre of a pixel with
    /// alpha > 0.5) and the mean RGB value of the outermost opaque ring.</summary>
    private static (float extent, float edgeValue) Measure(Sprite s)
    {
        var tex = s.texture;
        var px = tex.GetPixels();
        int res = tex.width;
        float c = res * 0.5f, extent = 0f;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
                if (px[y * res + x].a > 0.5f)
                    extent = Mathf.Max(extent, Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c)));
        float sum = 0f; int n = 0;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                var p = px[y * res + x];
                if (p.a <= 0.9f) continue;
                float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                if (d < extent - 3f) continue;
                sum += (p.r + p.g + p.b) / 3f; n++;
            }
        return (extent, n > 0 ? sum / n : 1f);
    }
}
