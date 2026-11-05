using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Names for the UI elements anything outside this assembly needs to
    /// ask about. Shared constants rather than string literals in two
    /// assemblies: a harness that queries "hud-caption" while the view
    /// names it "hudCaption" passes vacuously, which is the failure mode
    /// this whole file exists to stop.
    /// </summary>
    public static class UiNames
    {
        public const string HudCard = "hud-card";
        public const string HudCaption = "hud-caption";
        public const string HudStartButton = "hud-start";
        public const string HudWaveBlock = "hud-wave";
        public const string HudGameOver = "hud-gameover";

        public const string RoundBanner = "round-banner";
        public const string RoundBannerVehicle = "round-banner-vehicle";
        public const string RoundBannerName = "round-banner-name";
        public const string RoundBannerWarning = "round-banner-warning";
        public const string RoundBannerPortrait = "round-banner-portrait";

        public const string SightingBanner = "sighting-banner";
        public const string SightingBannerName = "sighting-banner-name";

        public const string KnowledgeCard = "knowledge-card";
        public const string KnowledgeViewport = "knowledge-viewport";
        public const string KnowledgeStrip = "knowledge-strip";
        public const string KnowledgeFadeRight = "knowledge-fade-right";
        public const string KnowledgeFadeLeft = "knowledge-fade-left";

        /// <summary>One species card in the knowledge ribbon, by species
        /// index -- "knowledge-species-3".</summary>
        public static string KnowledgeSpecies(int species) => "knowledge-species-" + species;
    }

    /// <summary>
    /// **Sprint 35: the harness can see the screen.**
    ///
    /// ## Why this exists
    ///
    /// Sprint 33 shipped a buy-phase caption that previews the next round
    /// and names any debuting organism. It had **41 green checks** and the
    /// Director saw neither line in play. Every one of those checks asked
    /// whether the string was *returned*; nothing asked whether it was
    /// *displayed*. That is the eighth instance of this project's standing
    /// pattern (a value produced and never consumed) and the first on the
    /// UI side of the boundary -- and a verified string written into a
    /// Label the player never reads is the same defect as a value nothing
    /// reads at all.
    ///
    /// So this is the missing half of the harness: the live visual tree,
    /// addressable by name from the Editor assembly, with the two questions
    /// a UI check actually needs to ask -- *what does this element say*,
    /// and *would the player see it*.
    ///
    /// ## What it can and cannot prove
    ///
    /// **Can:** that an element exists, that it is reachable from the panel
    /// root, that no ancestor hides it (the `UiTheme.Show` mistake: add a
    /// child and never show its parent), that its text is non-empty, and
    /// that the shipped font can draw every character of it.
    ///
    /// **Cannot:** pixels. In Editor batchmode there is no panel and no
    /// frame, so UITK never runs a layout pass and `worldBound` is all
    /// zeroes -- `resolvedStyle` is reported where it resolves and
    /// otherwise falls back to the inline style this project always sets.
    /// A line that is drawn but illegibly small, or clipped by HUD scale in
    /// the WebGL build, is **not** in reach of this probe; that still needs
    /// an eye on a build. <see cref="Dump"/> exists for exactly that gap:
    /// it prints the declared geometry of the whole tree so a human can
    /// read what the layout engine would be given.
    ///
    /// Registered by <see cref="UiController.Build"/>; a no-op cost of one
    /// static reference in the player.
    /// </summary>
    public static class UiProbe
    {
        /// <summary>The panel root the live UI was built into, or null
        /// before <see cref="UiController.Build"/> has run.</summary>
        public static VisualElement Root { get; private set; }

        public static void Register(VisualElement root) => Root = root;

        /// <summary>Forgotten between harness runs, so a stale tree from a
        /// destroyed bootstrap cannot answer for a live one.</summary>
        public static void Clear() => Root = null;

        /// <summary>Depth-first by name, from the registered root. Null if
        /// nothing carries the name.</summary>
        public static VisualElement Find(string name)
        {
            if (Root == null || string.IsNullOrEmpty(name)) return null;
            return FindIn(Root, name);
        }

        private static VisualElement FindIn(VisualElement e, string name)
        {
            if (e.name == name) return e;
            for (int i = 0; i < e.childCount; i++)
            {
                var hit = FindIn(e[i], name);
                if (hit != null) return hit;
            }
            return null;
        }

        public static bool Exists(string name) => Find(name) != null;

        /// <summary>How many elements carry the name. More than one is a
        /// bug in the naming, and a query that silently answers for the
        /// wrong one is worse than a failure.</summary>
        public static int Count(string name)
        {
            if (Root == null) return 0;
            int n = 0;
            CountIn(Root, name, ref n);
            return n;
        }

        private static void CountIn(VisualElement e, string name, ref int n)
        {
            if (e.name == name) n++;
            for (int i = 0; i < e.childCount; i++) CountIn(e[i], name, ref n);
        }

        /// <summary>The text of a named <see cref="TextElement"/> (Label,
        /// Button). Null when the element is missing or carries no text at
        /// all; the empty string when it is there and says nothing, which
        /// is a different failure and must read differently.</summary>
        public static string TextOf(string name)
        {
            var e = Find(name);
            if (e == null) return null;
            if (e is TextElement t) return t.text ?? "";
            var inner = FirstTextIn(e);
            return inner?.text;
        }

        private static TextElement FirstTextIn(VisualElement e)
        {
            for (int i = 0; i < e.childCount; i++)
            {
                if (e[i] is TextElement t) return t;
                var deeper = FirstTextIn(e[i]);
                if (deeper != null) return deeper;
            }
            return null;
        }

        /// <summary>Whether the player would be shown this element: it
        /// exists, and nothing from it up to the root is display:none,
        /// invisible or fully transparent.</summary>
        public static bool IsDisplayed(string name) => HiddenBecause(name) == null;

        /// <summary>Why <see cref="IsDisplayed"/> is false, phrased for a
        /// failing check's message -- "hidden by ancestor 'hud-wave'
        /// (display:none)". Null when the element is displayed.</summary>
        public static string HiddenBecause(string name)
        {
            var e = Find(name);
            if (e == null) return $"no element named '{name}' in the tree";

            for (var at = e; at != null; at = at.parent)
            {
                if (DisplayOf(at) == DisplayStyle.None)
                    return at == e
                        ? $"'{name}' is display:none"
                        : $"hidden by ancestor '{Describe(at)}' (display:none)";
                if (!at.visible)
                    return at == e ? $"'{name}' is not visible" : $"hidden by ancestor '{Describe(at)}' (not visible)";
                float o = at.resolvedStyle.opacity;
                if (!float.IsNaN(o) && o <= 0.001f)
                    return at == e ? $"'{name}' is fully transparent" : $"hidden by ancestor '{Describe(at)}' (opacity 0)";
            }

            // Reaching null without passing the registered root means the
            // element is in a detached fragment -- built, styled, and
            // parented to nothing. That renders exactly as nothing.
            return IsUnderRoot(e) ? null : $"'{name}' is not attached to the panel root";
        }

        private static bool IsUnderRoot(VisualElement e)
        {
            for (var at = e; at != null; at = at.parent) if (at == Root) return true;
            return false;
        }

        /// <summary>Display as the player's build would compute it: the
        /// inline style when this project set one (it sets one on
        /// everything it shows or hides), else the resolved style.</summary>
        private static DisplayStyle DisplayOf(VisualElement e)
        {
            var inline = e.style.display;
            if (inline.keyword == StyleKeyword.Undefined || inline.keyword == StyleKeyword.None)
                return inline.value;
            return e.resolvedStyle.display;
        }

        private static string Describe(VisualElement e) =>
            string.IsNullOrEmpty(e.name) ? e.GetType().Name : e.name;

        /// <summary>Every name in the tree, for a harness that wants to
        /// assert on a set rather than on one element.</summary>
        public static List<string> Names()
        {
            var all = new List<string>();
            if (Root != null) Collect(Root, all);
            return all;
        }

        private static void Collect(VisualElement e, List<string> into)
        {
            if (!string.IsNullOrEmpty(e.name)) into.Add(e.name);
            for (int i = 0; i < e.childCount; i++) Collect(e[i], into);
        }

        /// <summary>
        /// The tree as text: name, type, declared size, display, and text.
        /// This is the diagnostic half -- batchmode runs no layout pass, so
        /// what a human can usefully read is what the layout engine would
        /// be *given*, which is exactly the inline geometry printed here.
        /// </summary>
        public static string Dump(int maxDepth = 12)
        {
            var sb = new StringBuilder();
            if (Root == null) { sb.Append("[UiProbe] no root registered"); return sb.ToString(); }
            DumpInto(Root, sb, 0, maxDepth);
            return sb.ToString();
        }

        private static void DumpInto(VisualElement e, StringBuilder sb, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            sb.Append(' ', depth * 2);
            sb.Append(string.IsNullOrEmpty(e.name) ? e.GetType().Name : e.name);

            var w = e.style.width;
            var h = e.style.height;
            if (w.keyword == StyleKeyword.Undefined) sb.Append("  w=").Append(w.value.ToString());
            if (h.keyword == StyleKeyword.Undefined) sb.Append("  h=").Append(h.value.ToString());
            if (DisplayOf(e) == DisplayStyle.None) sb.Append("  DISPLAY:NONE");
            var op = e.style.opacity;
            sb.Append("  op=").Append(op.keyword == StyleKeyword.Undefined
                ? op.value.ToString("0.00") : "unset/" + e.resolvedStyle.opacity.ToString("0.00"));
            if (!e.visible) sb.Append("  INVISIBLE");

            if (e is TextElement t && !string.IsNullOrEmpty(t.text))
            {
                sb.Append("  fs=").Append(t.style.fontSize.value.ToString());
                sb.Append("  \"").Append(t.text.Length > 90 ? t.text.Substring(0, 90) + "..." : t.text).Append('"');
                if (!UiTheme.IsRenderable(t.text))
                    sb.Append("  UNRENDERABLE U+").Append(((int)UiTheme.FirstUnrenderable(t.text)).ToString("X4"));
            }
            sb.Append('\n');

            for (int i = 0; i < e.childCount; i++) DumpInto(e[i], sb, depth + 1, maxDepth);
        }

        /// <summary>
        /// Whether the named element, or anything under it, draws
        /// <paramref name="sprite"/> as its background image.
        ///
        /// This is how a check asks "is the rotavirus card actually
        /// drawing rotavirus" -- the question that matters, as against
        /// "does a rotavirus sprite exist", which is what a check on the
        /// sprite table alone would really be asserting.
        /// </summary>
        public static bool HasBackgroundSprite(string name, Sprite sprite)
        {
            var e = Find(name);
            return e != null && sprite != null && DrawsSprite(e, sprite);
        }

        private static bool DrawsSprite(VisualElement e, Sprite sprite)
        {
            var bg = e.style.backgroundImage;
            if (bg.keyword == StyleKeyword.Undefined && bg.value.sprite == sprite) return true;
            for (int i = 0; i < e.childCount; i++) if (DrawsSprite(e[i], sprite)) return true;
            return false;
        }

        /// <summary>The element's own opacity as this project declares it
        /// (inline), or NaN when it is missing. Dimming is a *statement*
        /// here -- the knowledge ribbon dims an organism the player has
        /// never met -- so it is worth being able to assert.</summary>
        public static float OpacityOf(string name)
        {
            var e = Find(name);
            if (e == null) return float.NaN;
            var o = e.style.opacity;
            return o.keyword == StyleKeyword.Undefined ? o.value : e.resolvedStyle.opacity;
        }

        /// <summary>The element's laid-out rectangle in panel space, or
        /// <see cref="Rect.zero"/> when UITK has not run a layout pass --
        /// which in Editor batchmode is the usual answer. A harness must
        /// therefore treat zero as "unknown", never as "off screen".</summary>
        public static Rect RectOf(string name)
        {
            var e = Find(name);
            if (e == null) return Rect.zero;
            var r = e.worldBound;
            return float.IsNaN(r.width) || float.IsNaN(r.height) ? Rect.zero : r;
        }

        /// <summary>Whether a layout pass has run at all, so a check can
        /// say "not measured" instead of asserting on zeroes.</summary>
        public static bool HasLayout =>
            Root != null && Root.worldBound.width > 1f && Root.worldBound.height > 1f;

        /// <summary>Exposed so a harness can prove the guard itself works:
        /// a probe that can only ever answer "displayed" is not a
        /// probe.</summary>
        public static bool IsRenderable(string text) => UiTheme.IsRenderable(text);
    }
}
