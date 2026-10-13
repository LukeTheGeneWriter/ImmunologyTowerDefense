using UnityEngine;
using UnityEngine.UIElements;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// A compartment heading pinned to a point on the board -- "Bone
    /// marrow", "Lymph node" (docs/UI_DESIGN.md §6).
    ///
    /// Replaces Rendering/CompartmentLabel.cs, which did the same job with
    /// IMGUI. The spec argued for leaving it in IMGUI one more sprint since
    /// it is thin text with no chrome; the Director's definition of done
    /// (docs/SPRINT_PLAN.md) says **no OnGUI left in the project**, and that
    /// wins -- one straggling IMGUI surface would mean every future UI
    /// question starts by asking which system a thing lives in.
    ///
    /// It stays *world-anchored* rather than becoming a screen-space panel,
    /// which was the substance of the spec's recommendation: it is an
    /// annotation pointing at an organ, and a caption in a corner would not
    /// be the same thing. RuntimePanelUtils projects the world point into
    /// panel space each frame, which also means it survives any future
    /// camera move for free.
    ///
    /// The copy is trimmed to the organ's name -- the old labels carried
    /// placement instructions, and the selection rim plus the floating
    /// picker teach that far better than a caption.
    /// </summary>
    internal sealed class WorldLabelView
    {
        public readonly VisualElement Root;

        private readonly Vector3 worldPosition;

        public WorldLabelView(VisualElement parent, Vector3 worldPosition, string text)
        {
            this.worldPosition = worldPosition;

            Root = new VisualElement();
            Root.style.position = Position.Absolute;
            Root.style.alignItems = Align.Center;
            Root.pickingMode = PickingMode.Ignore;

            // Sprint 21 (COSMETIC_PASS_SPEC.md §1.4): a tag -- card fill, ink
            // edge, heavy caps -- pinned to the organ's edge.
            Root.Add(UiTheme.Tag(text));

            parent.Add(Root);
        }

        public void Refresh(IPanel panel)
        {
            if (panel == null || Camera.main == null) return;

            Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPosition, Camera.main);
            float w = Root.resolvedStyle.width;
            float h = Root.resolvedStyle.height;
            if (float.IsNaN(w)) return;   // first frame, before layout

            Root.style.left = p.x - w * 0.5f;
            Root.style.top = p.y - h * 0.5f;
        }
    }
}
