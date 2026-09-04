using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ImmunologyTD.Rendering;

/// <summary>
/// Writes every <see cref="SpriteShapes"/> sprite out as a PNG so a shape
/// can be *looked at* without launching the game.
///
/// Sprint 17 added this while drawing the villi. Every sprite in this
/// project is procedural -- a few dozen lines of fills and shades against a
/// 64x64 alpha buffer -- and until now the only way to see whether a new
/// one came out as intended was to build the player and hunt for it on the
/// board at ~20 px. Dumping the raster is faster, and it is the difference
/// between "the code compiles" and "the shape is a villus".
///
/// The sprites are white with the silhouette in the alpha channel (that is
/// how per-instance tinting works), so a raw PNG is white-on-transparent
/// and invisible against a white background. Each dump therefore also gets
/// a **_check** variant: the alpha composited over a dark grey, which is
/// what the shape will actually look like on the board.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod SpriteShapeDump.DumpAll
/// Output goes to &lt;projectPath&gt;/../_spritedump/ (outside Assets, so Unity
/// never imports it and it never lands in a build).
/// </summary>
public static class SpriteShapeDump
{
    private const string OutDir = "../_spritedump";

    [MenuItem("ImmunologyTD/Dump sprite shapes to PNG")]
    public static void DumpAll()
    {
        Directory.CreateDirectory(OutDir);
        SpriteShapes.Prewarm();

        int written = 0;
        foreach (var prop in typeof(SpriteShapes).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (prop.PropertyType != typeof(Sprite)) continue;
            var sprite = prop.GetValue(null) as Sprite;
            if (sprite == null || sprite.texture == null) continue;

            var tex = sprite.texture;
            File.WriteAllBytes(Path.Combine(OutDir, prop.Name + ".png"), tex.EncodeToPNG());
            File.WriteAllBytes(Path.Combine(OutDir, prop.Name + "_check.png"), OverDarkGrey(tex).EncodeToPNG());
            written++;
        }

        Debug.Log($"[SpriteShapeDump] Wrote {written} shapes (x2 files) to {Path.GetFullPath(OutDir)}");
    }

    /// <summary>Composites the white-on-alpha raster over the board's dark
    /// ground, which is the only way the silhouette is actually visible.</summary>
    private static Texture2D OverDarkGrey(Texture2D src)
    {
        var bg = new Color(0.13f, 0.11f, 0.12f, 1f);   // the empty-pit near-black
        var pixels = src.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float a = pixels[i].a;
            pixels[i] = new Color(
                Mathf.Lerp(bg.r, pixels[i].r, a),
                Mathf.Lerp(bg.g, pixels[i].g, a),
                Mathf.Lerp(bg.b, pixels[i].b, a),
                1f);
        }
        var outTex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        outTex.SetPixels(pixels);
        outTex.Apply();
        return outTex;
    }
}
