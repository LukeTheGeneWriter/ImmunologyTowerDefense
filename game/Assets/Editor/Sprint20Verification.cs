using UnityEngine;
using ImmunologyTD.Rendering;

/// <summary>
/// Sprint 20 verification: the palette and the Classic / Daylight looks
/// (docs/ART_DIRECTION.md).
///
///  1. Classic IS the shipped look: spot-checks against the pre-Sprint-20
///     literals, so the A/B toggle compares against the real thing.
///  2. The five FX colours are pairwise distinct in both looks --
///     DegranulationFlash.ShapeFor picks a flash's shape by exact colour.
///  3. Daylight fixes the legibility problem the art pass measured: in
///     Classic the bacterium and virion are ~1.1:1 against the lumen; in
///     Daylight their outline is far above that, and their fill differs
///     from the lumen in hue.
///  4. Daylight sprites carry a dark outline with the SAME outer extent as
///     Classic (the shape shrinks to make room), and Classic sprites
///     have none.
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
        var saved = Palette.Current;

        RunClassicIsShipped();
        RunFxDistinct(Look.Classic);
        RunFxDistinct(Look.Daylight);
        RunContrast();
        RunOutlines();

        Palette.Current = saved;
        SpriteShapes.ClearCache();
        Debug.Log($"[Sprint20Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint20Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint20Verification] FAIL -- {label}"); }
    }

    private static bool Eq(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 1e-4f && Mathf.Abs(a.g - b.g) < 1e-4f && Mathf.Abs(a.b - b.b) < 1e-4f;

    private static void RunClassicIsShipped()
    {
        Palette.Current = Look.Classic;
        Check("Classic macrophage is the shipped blue", Eq(Palette.Macrophage, new Color(0.30f, 0.40f, 0.80f)));
        Check("Classic DC is the shipped magenta", Eq(Palette.Dendritic, new Color(0.72f, 0.30f, 0.68f)));
        Check("Classic bacterium is the shipped maroon", Eq(Palette.Bacterium, new Color(0.42f, 0.12f, 0.16f)));
        Check("Classic chyme is the shipped plum", Eq(Palette.Chyme, new Color(0.36f, 0.22f, 0.24f)));
        Check("Classic knowledge flash is the shipped green", Eq(Palette.FxKnowledge, new Color(0.40f, 0.92f, 0.45f)));
        Check("Classic camera clear is the shipped near-black", Eq(Palette.CameraClear, new Color(0.05f, 0.05f, 0.07f)));
        Check("Classic has no outlines and no size floor", !Palette.Outlines && !Palette.PathogenSizeFloor);
        Check("the routed statics read the palette (BoardRenderer.PathogenColor)", Eq(BoardRenderer.PathogenColor, Palette.Bacterium));
        Check("...and DegranulationFlash.KnowledgeMatchColor", Eq(DegranulationFlash.KnowledgeMatchColor, Palette.FxKnowledge));
    }

    private static void RunFxDistinct(Look look)
    {
        Palette.Current = look;
        var fx = new[] { Palette.FxDegranulation, Palette.FxBreach, Palette.FxEfferocytosis, Palette.FxStressKill, Palette.FxKnowledge };
        bool distinct = true;
        for (int i = 0; i < fx.Length; i++)
            for (int j = i + 1; j < fx.Length; j++)
                if (Mathf.Abs(fx[i].r - fx[j].r) < 0.01f && Mathf.Abs(fx[i].g - fx[j].g) < 0.01f && Mathf.Abs(fx[i].b - fx[j].b) < 0.01f)
                    distinct = false;
        Check($"{look}: the five FX colours are pairwise distinct (ShapeFor matches by colour)", distinct);
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
        Palette.Current = Look.Classic;
        float cb = Contrast(Palette.Bacterium, Palette.Chyme), cv = Contrast(Palette.Virion, Palette.Chyme);

        Palette.Current = Look.Daylight;
        float db = Contrast(Outline(Palette.Bacterium), Palette.Chyme);
        float dv = Contrast(Outline(Palette.Virion), Palette.Chyme);
        float dbol = Contrast(Outline(Palette.FoodBolus), Palette.Chyme);
        Debug.Log($"[Sprint20Verification] pathogen vs lumen: Classic bacterium {cb:F2}, virion {cv:F2}; Daylight outline bacterium {db:F1}, virion {dv:F1}, bolus {dbol:F1}");
        Check("Classic reproduces the measured problem (both pathogens < 1.3:1 on the lumen)", cb < 1.3f && cv < 1.3f);
        Check("Daylight: the bacterium's outline is >= 7:1 on the lumen", db >= 7f);
        Check("Daylight: the virion's outline is >= 7:1 on the lumen", dv >= 7f);
        Check("Daylight: the bolus's outline is >= 7:1 on the lumen", dbol >= 7f);

        Color.RGBToHSV(Palette.Bacterium, out float hb, out float sb, out _);
        Color.RGBToHSV(Palette.Virion, out float hv, out float sv, out _);
        Color.RGBToHSV(Palette.Chyme, out _, out float sc, out _);
        Check("Daylight pathogens are far more saturated than the lumen", sb - sc > 0.5f && sv - sc > 0.5f);
        Check("Daylight bacterium is green (the germ convention, D2)", hb > 0.2f && hb < 0.4f);
    }

    private static void RunOutlines()
    {
        Palette.Current = Look.Classic;
        SpriteShapes.ClearCache();
        var classic = Measure(SpriteShapes.Macrophage);
        var classicNeu = Measure(SpriteShapes.Neutrophil);

        Palette.Current = Look.Daylight;
        SpriteShapes.ClearCache();
        var day = Measure(SpriteShapes.Macrophage);
        var dayNeu = Measure(SpriteShapes.Neutrophil);
        var vir = Measure(SpriteShapes.Virion);

        Debug.Log($"[Sprint20Verification] macrophage extent {classic.extent:F1} -> {day.extent:F1}, edge value {classic.edgeValue:F2} -> {day.edgeValue:F2}");
        Check("Classic macrophage has no dark outline (edge value > 0.4)", classic.edgeValue > 0.4f);
        Check("Daylight macrophage has a dark outline (edge value < 0.25)", day.edgeValue < 0.25f);
        Check("Daylight neutrophil has a dark outline", dayNeu.edgeValue < 0.25f);
        Check("Daylight virion has a dark outline", vir.edgeValue < 0.25f);
        Check("outlining keeps the macrophage's outer extent (shape shrinks to make room)",
            Mathf.Abs(day.extent - classic.extent) <= 2.5f);
        Check("...and the neutrophil's", Mathf.Abs(dayNeu.extent - classicNeu.extent) <= 2.5f);
        Check("nothing clips the texture edge", day.extent < 31.5f && vir.extent < 31.5f);
        SpriteShapes.ClearCache();
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
