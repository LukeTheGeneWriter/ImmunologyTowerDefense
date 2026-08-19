using UnityEngine;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// Small reusable IMGUI label anchored to a world-space point --
    /// projected via Camera.main.WorldToScreenPoint every OnGUI call, since
    /// this project has no uGUI (no com.unity.ugui package -- see
    /// docs/ENGINE_STATUS.md) to anchor a real UI element to a world
    /// transform instead. Used for compartment headings (bone marrow, lymph
    /// node) so the four-compartment layout (GAME_DESIGN.md section 1)
    /// reads clearly on screen without writing near-identical OnGUI
    /// boilerplate for each one.
    /// </summary>
    public class CompartmentLabel : MonoBehaviour
    {
        private Vector3 worldPosition;
        private string text;
        private Vector2 size = new Vector2(240, 40);
        private GUIStyle style;

        public void Initialize(Vector3 worldPosition, string text, Vector2? sizeOverride = null)
        {
            this.worldPosition = worldPosition;
            this.text = text;
            if (sizeOverride.HasValue) size = sizeOverride.Value;
        }

        private void OnGUI()
        {
            if (Camera.main == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            var screen = Camera.main.WorldToScreenPoint(worldPosition);
            if (screen.z < 0f) return; // behind the camera -- shouldn't happen for this project's fixed orthographic setup, but cheap to guard
            var guiPos = new Vector2(screen.x, Screen.height - screen.y);
            GUI.Label(new Rect(guiPos.x - size.x / 2f, guiPos.y - size.y / 2f, size.x, size.y), text, style);
        }
    }
}
