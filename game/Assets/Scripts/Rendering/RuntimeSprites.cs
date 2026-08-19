using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// A single reusable 1x1 white sprite, tinted per-instance via
    /// SpriteRenderer.color. No imported art assets this sprint (see
    /// SPRINT_PLAN.md's exclusion list) -- flat colour quads are the whole
    /// visual language for now.
    /// </summary>
    public static class RuntimeSprites
    {
        private static Sprite squareSprite;

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                    tex.SetPixels32(pixels);
                    tex.filterMode = FilterMode.Point;
                    tex.Apply();
                    squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                }
                return squareSprite;
            }
        }
    }
}
