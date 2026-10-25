using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 27. The price of the next bone-marrow niche, pinned to the
    /// fatty pad you would be buying.
    ///
    /// The Director chose to make expanding the marrow a click on the pad
    /// itself rather than a shop row, so that the thing you are buying is
    /// visible on the board. That only works if the price is visible too --
    /// without it, clicking fatty marrow either silently succeeds or
    /// silently does nothing, and the player has to infer both the cost and
    /// the fact that it is purchasable at all.
    ///
    /// So exactly one tag is shown, on the NEXT convertible pad, and it
    /// dims when the player cannot afford it. One tag rather than four
    /// because the bone fills from the top down: only one pad is ever the
    /// next purchase, and four price chips would read as clutter in the
    /// smallest, busiest compartment on the board.
    /// </summary>
    internal sealed class MarrowReconvertTag
    {
        public readonly VisualElement Root;

        private readonly BoneMarrowManager marrow;
        private readonly Label label;

        private int shownPrice = -1;
        private bool shownAffordable;
        private Vector3 worldPosition;

        public MarrowReconvertTag(VisualElement parent, BoneMarrowManager marrow)
        {
            this.marrow = marrow;

            Root = new VisualElement();
            Root.style.position = Position.Absolute;
            Root.style.alignItems = Align.Center;
            Root.pickingMode = PickingMode.Ignore;

            label = UiTheme.Text("", 11, UiTheme.Ink, Weight.Black);
            var tag = UiTheme.Tile();
            tag.style.paddingLeft = tag.style.paddingRight = 6;
            tag.style.paddingTop = tag.style.paddingBottom = 2;
            tag.Add(label);
            Root.Add(tag);

            parent.Add(Root);
        }

        public void Refresh(IPanel panel)
        {
            if (marrow == null || panel == null || Camera.main == null)
            {
                UiTheme.Show(Root, false);
                return;
            }

            if (!marrow.CanReconvert)
            {
                // The bone is full. Nothing left to sell, so say nothing.
                UiTheme.Show(Root, false);
                return;
            }

            int price = marrow.NextReconversionPrice;
            bool affordable = marrow.Balance >= price;

            if (price != shownPrice || affordable != shownAffordable)
            {
                shownPrice = price;
                shownAffordable = affordable;
                label.text = $"{price} ATP";
                label.style.color = affordable ? UiTheme.Ink : UiTheme.Hatch;
                Root.style.opacity = affordable ? 1f : 0.75f;
                worldPosition = marrow.SlotWorldPosition(marrow.UnlockedSlots);
            }

            UiTheme.Show(Root, true);

            Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPosition, Camera.main);
            float w = Root.resolvedStyle.width;
            float h = Root.resolvedStyle.height;
            if (float.IsNaN(w)) return; // first frame, before layout

            Root.style.left = p.x - w * 0.5f;
            Root.style.top = p.y - h * 0.5f;
        }
    }
}
