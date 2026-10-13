using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Grid;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 21 (docs/COSMETIC_PASS_SPEC.md §3.3): three folder tabs on the
    /// plate's top edge naming the bands -- BLOODSTREAM, TISSUE, GUT LUMEN
    /// (with a down arrow: the flow direction) -- each with a legend disc in
    /// the band's own colour. Pinned to the board with
    /// CameraTransformWorldToPanel like WorldLabelView; never pickable.
    /// </summary>
    internal sealed class BandTabsView
    {
        private readonly (VisualElement root, Vector3 anchor)[] tabs;

        public BandTabsView(VisualElement parent, BoardConfig board)
        {
            tabs = new[]
            {
                Make(parent, board, BoardBand.Base, "Bloodstream", Palette.Plasma, false),
                Make(parent, board, BoardBand.Tissue, "Tissue", Palette.HostHealthy, false),
                Make(parent, board, BoardBand.Lumen, "Gut lumen", Palette.Chyme, true),
            };
        }

        private static (VisualElement, Vector3) Make(VisualElement parent, BoardConfig board, BoardBand band,
                                                     string text, Color legend, bool arrow)
        {
            var r = board.BandWorldRect(band);
            var anchor = new Vector3(r.xMin, r.yMax, 0f);

            var tab = new VisualElement();
            tab.style.position = Position.Absolute;
            tab.style.flexDirection = FlexDirection.Row;
            tab.style.alignItems = Align.Center;
            tab.style.height = 32;
            tab.style.paddingLeft = 10; tab.style.paddingRight = 12;
            tab.style.backgroundColor = UiTheme.CardFill;
            UiTheme.SetBorderWidth(tab, 3);
            tab.style.borderBottomWidth = 0;
            UiTheme.SetBorderColor(tab, UiTheme.Edge);
            tab.style.borderTopLeftRadius = tab.style.borderTopRightRadius = 9;
            tab.pickingMode = PickingMode.Ignore;

            var disc = new VisualElement();
            disc.style.width = 14; disc.style.height = 14;
            disc.style.backgroundColor = legend;
            UiTheme.SetBorderWidth(disc, 2);
            UiTheme.SetBorderColor(disc, UiTheme.Edge);
            UiTheme.SetRadius(disc, 7);
            disc.style.marginRight = 6;
            disc.pickingMode = PickingMode.Ignore;
            tab.Add(disc);
            tab.Add(UiTheme.Text(text, 13, UiTheme.Ink, Weight.Black, upper: true));
            if (arrow)
            {
                var a = UiTheme.Icon(SpriteShapes.IconArrowDown, UiTheme.Ink, 16);
                a.style.marginLeft = 4;
                tab.Add(a);
            }
            parent.Add(tab);
            return (tab, anchor);
        }

        public void Refresh(IPanel panel)
        {
            if (panel == null || Camera.main == null) return;
            foreach (var (root, anchor) in tabs)
            {
                Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(panel, anchor, Camera.main);
                float h = root.resolvedStyle.height;
                if (float.IsNaN(h)) continue;
                // 14 px in from the band's left edge, sitting on the plate's
                // top edge (just over its ink ring).
                root.style.left = p.x + 14f;
                root.style.top = p.y - h + 2f;
            }
        }
    }
}
