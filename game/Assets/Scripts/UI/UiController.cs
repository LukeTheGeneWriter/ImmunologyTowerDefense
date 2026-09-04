using UnityEngine;
using UnityEngine.UIElements;
using ImmunologyTD.Grid;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// The whole front end, on one GameObject (docs/UI_DESIGN.md §7).
    /// Sprint 16 replaces every IMGUI surface in the project -- HudOverlay's
    /// round bar, its stat dump and its shop, and BoneMarrowManager's picker
    /// and upgrade panels -- with UI Toolkit built from code: no .uxml, no
    /// .uss, no UI Builder, no asset of any kind. PanelSettings is created
    /// with ScriptableObject.CreateInstance at boot.
    ///
    /// **Why UITK and not uGUI:** uGUI needs com.unity.ugui, a package add;
    /// UI Toolkit's runtime module (com.unity.modules.uielements) is already
    /// in the manifest. **Why from code and not assets:** every other visual
    /// in this project is procedural (SpriteShapes rasters its own sprites,
    /// SceneSetup builds the scene from a script) and a .uxml would be the
    /// first hand-authored asset a text diff can't review.
    ///
    /// **The one asset.** UI_DESIGN.md §7 planned for a null
    /// `themeStyleSheet` and accepted the boot warning. That turned out to
    /// be wrong for the wrong reason: a `PanelSettings` created at runtime
    /// also has no *text settings*, and in a **player build** the text
    /// shaper then throws a NullReferenceException on every label, every
    /// frame -- while the Editor and batchmode stay perfectly quiet. So the
    /// project ships exactly one UI asset,
    /// `Assets/Resources/ITD_PanelSettings.asset`, created by script
    /// (Assets/Editor/UiAssetSetup.cs) and loaded here. Everything else is
    /// still code. Every element is styled explicitly regardless, so the
    /// default theme it carries is a floor, not the look.
    ///
    /// **Picking:** the root and the dock containers are PickingMode.Ignore,
    /// so a click that is not on a panel falls straight through to the
    /// physics raycaster and BoneMarrowSlot.OnMouseDown still works. This is
    /// the thing to check first if slot clicks ever stop registering.
    ///
    /// It polls. There are no events on AtpWallet / RoundController /
    /// ShopLedger / BoneMarrowManager and adding them was out of scope, so
    /// Update() reads a handful of int getters each frame and every view
    /// early-outs unless something it draws actually moved.
    /// </summary>
    public sealed class UiController : MonoBehaviour
    {
        private BoardConfig board;
        private AtpWallet wallet;
        private RoundController rounds;
        private PathogenSpawner spawner;
        private BoneMarrowManager marrow;
        private KnowledgeLedger knowledge;
        private AdaptiveDirector adaptive;
        private ShopLedger shop;
        private GutInterface gutInterface;
        private InvasionTally tally;
        private int macrophageSpeed, neutrophilSpeed;

        private UIDocument doc;
        private VisualElement root;
        private VisualElement floatLayer;

        private HudView hud;
        private ShopView shopView;
        private UpgradePanelView upgrade;
        private TowerPickerView picker;
        private DebugReadoutView debug;
        private readonly System.Collections.Generic.List<WorldLabelView> worldLabels =
            new System.Collections.Generic.List<WorldLabelView>();
        private readonly System.Collections.Generic.List<(Vector3 pos, string text)> pendingLabels =
            new System.Collections.Generic.List<(Vector3, string)>();

        private bool built;
        private bool debugVisible;
        /// <summary>Which slot the floating panel is currently built for,
        /// and whether it was placed at the time -- the two facts that decide
        /// whether a re-target is needed. -1 = no panel up.</summary>
        private int shownSlot = -1;
        private bool shownPlaced;
        private RoundPhase lastPhase = (RoundPhase)(-1);

        public void Bind(
            BoardConfig board, AtpWallet wallet, RoundController rounds, PathogenSpawner spawner,
            BoneMarrowManager marrow, KnowledgeLedger knowledge, AdaptiveDirector adaptive,
            ShopLedger shop, GutInterface gutInterface, InvasionTally tally,
            int macrophageSpeed, int neutrophilSpeed)
        {
            this.board = board;
            this.wallet = wallet;
            this.rounds = rounds;
            this.spawner = spawner;
            this.marrow = marrow;
            this.knowledge = knowledge;
            this.adaptive = adaptive;
            this.shop = shop;
            this.gutInterface = gutInterface;
            this.tally = tally;
            this.macrophageSpeed = macrophageSpeed;
            this.neutrophilSpeed = neutrophilSpeed;
        }

        private void OnEnable() => Build();

        /// <summary>Builds the tree. Public and idempotent because
        /// GameBootstrap calls it straight after Bind -- OnEnable fires
        /// before the refs exist when the component is added, and in Editor
        /// batchmode it does not fire at all.
        ///
        /// In batchmode there is no live panel, so rootVisualElement is
        /// null; the views are then built into a detached element instead.
        /// That is on purpose: constructing every view is exactly the part
        /// of this file a headless bootstrap smoke run can prove doesn't
        /// throw (§9), and it costs nothing to keep that path alive.</summary>
        public void Build()
        {
            if (built || board == null) return;

            doc = GetComponent<UIDocument>();
            root = doc != null ? doc.rootVisualElement : null;
            if (root == null) root = new VisualElement();   // headless / batchmode

            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;
            UiTheme.ApplyFont(root);

            var topRight = Dock(root, Align.FlexEnd, Justify.FlexStart);
            var bottomLeft = Dock(root, Align.FlexStart, Justify.FlexEnd);
            floatLayer = Dock(root, Align.FlexStart, Justify.FlexStart);

            // The HUD and the shop share the right column so the shop hangs
            // directly under the HUD rather than floating free of it.
            hud = new HudView(topRight, StartRound);
            shopView = new ShopView(topRight);
            shopView.Bind(shop, wallet);

            upgrade = new UpgradePanelView(floatLayer, ClearSelection);
            picker = new TowerPickerView(floatLayer, ClearSelection);

            debug = new DebugReadoutView(bottomLeft);
            debug.Bind(board, macrophageSpeed, neutrophilSpeed, marrow, gutInterface, tally,
                       spawner, knowledge, adaptive);

            foreach (var (pos, text) in pendingLabels)
                worldLabels.Add(new WorldLabelView(floatLayer, pos, text));
            pendingLabels.Clear();

            built = true;
        }

        /// <summary>Adds a compartment heading pinned to a board position.
        /// Queued if the tree isn't built yet, since GameBootstrap lays the
        /// organs out before it builds the UI.</summary>
        public void AddWorldLabel(Vector3 worldPosition, string text)
        {
            if (built) worldLabels.Add(new WorldLabelView(floatLayer, worldPosition, text));
            else pendingLabels.Add((worldPosition, text));
        }

        private static VisualElement Dock(VisualElement parent, Align h, Justify v)
        {
            var d = new VisualElement();
            d.style.position = Position.Absolute;
            d.style.left = 0; d.style.right = 0; d.style.top = 0; d.style.bottom = 0;
            d.style.alignItems = h;
            d.style.justifyContent = v;
            d.pickingMode = PickingMode.Ignore;
            parent.Add(d);
            return d;
        }

        private void StartRound()
        {
            if (rounds == null || rounds.Phase != RoundPhase.Building) return;
            rounds.StartRound();
        }

        private void ClearSelection() => marrow?.ClearSelection();

        private void Update()
        {
            if (!built) Build();
            if (!built) return;

            ReadKeys();
            HandleClickAway();

            // Sprint 12's one real shop effect, bridged here now that
            // HudOverlay is gone. ShopLedger stays a pure spend-and-level
            // ledger; this is the only line that turns a purchase into
            // behaviour anywhere in the project.
            if (shop != null)
                Chemotaxis.SensingUpgradeLevel = shop.LevelOf(ShopItem.CytokineSensingUpgrade);

            hud.Refresh(wallet, rounds, spawner);
            shopView.Refresh();

            if (rounds != null && rounds.Phase != lastPhase)
            {
                // Live buying (SPRINT_PLAN.md decision 3) means the shop
                // stays reachable in Active -- collapsed, not gone. The
                // selection is cleared on a phase change either way: a panel
                // left floating over a slot from the previous phase is
                // stale, not useful.
                shopView.OnPhaseChanged(rounds.Phase == RoundPhase.Building);
                marrow?.ClearSelection();
                lastPhase = rounds.Phase;
            }

            RefreshSelection();

            for (int i = 0; i < worldLabels.Count; i++) worldLabels[i].Refresh(root.panel);

            debug.TickFrameCost();
            if (debugVisible) debug.Refresh(marrow != null && marrow.SelectedSlotIndex.HasValue
                ? marrow.SelectedSlotIndex.Value : -1);
        }

        private void ReadKeys()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                debugVisible = !debugVisible;
                UiTheme.Show(debug.Root, debugVisible);
            }

            if (Input.GetKeyDown(KeyCode.Escape)) ClearSelection();

            // Sprint 13's flash preview, moved off HudOverlay unchanged: one
            // frame that fires all five effect flashes across the tissue
            // band for a visual QA of the shapes.
            if (board != null && Input.GetKeyDown(KeyCode.F9)) PlayFlashPreview();
        }

        /// <summary>A click that hits neither a panel nor a slot clears the
        /// selection (§4). OnMouseDown runs *before* Update in the same
        /// frame, so a click that just selected a slot is recognised by its
        /// frame stamp rather than by re-raycasting.</summary>
        private void HandleClickAway()
        {
            if (marrow == null || !Input.GetMouseButtonDown(0)) return;
            if (marrow.LastSelectionFrame == Time.frameCount) return;
            if (!marrow.SelectedSlotIndex.HasValue) return;
            if (PointerOverUi(Input.mousePosition)) return;
            marrow.ClearSelection();
        }

        /// <summary>Whether a screen point is over one of the visible
        /// panels. Panel space is the screen scaled uniformly by
        /// PanelSettings, so one ratio converts between them; the Y axis
        /// flips (UITK's origin is top-left, Input's is bottom-left).</summary>
        private bool PointerOverUi(Vector3 screenPos)
        {
            var p = ScreenToPanel(screenPos);
            return Over(hud.Root, p) || Over(shopView.Root, p)
                || Over(upgrade.Root, p) || Over(picker.Root, p)
                || (debugVisible && Over(debug.Root, p));
        }

        private static bool Over(VisualElement e, Vector2 panelPoint) =>
            e != null && e.resolvedStyle.display != DisplayStyle.None && e.worldBound.Contains(panelPoint);

        private Vector2 ScreenToPanel(Vector3 screenPos)
        {
            float width = root.resolvedStyle.width;
            float scale = Screen.width > 0 && width > 0f ? width / Screen.width : 1f;
            return new Vector2(screenPos.x * scale, (Screen.height - screenPos.y) * scale);
        }

        /// <summary>Shows the picker for an empty selected slot, the upgrade
        /// panel for a placed one, neither for no selection -- and keeps
        /// whichever is up pinned to its slot as the panel resizes.</summary>
        private void RefreshSelection()
        {
            if (marrow == null) return;

            int? sel = marrow.SelectedSlotIndex;

            if (!sel.HasValue)
            {
                if (shownSlot >= 0)
                {
                    UiTheme.Show(upgrade.Root, false);
                    UiTheme.Show(picker.Root, false);
                    shownSlot = -1;
                }
                return;
            }

            bool placed = marrow.GetSlotState(sel.Value) == BoneMarrowSlotState.Placed;

            // Re-target on a new slot, and on the slot the picker is open on
            // becoming placed -- buying a tower should swap straight to its
            // roster rather than making the player click the slot again.
            if (sel.Value != shownSlot || placed != shownPlaced)
            {
                if (placed) upgrade.SetTarget(marrow, sel.Value);
                else picker.SetTarget(marrow, sel.Value);
                shownSlot = sel.Value;
                shownPlaced = placed;
            }

            UiTheme.Show(upgrade.Root, placed);
            UiTheme.Show(picker.Root, !placed);

            var panel = placed ? upgrade.Root : picker.Root;
            if (placed) upgrade.Refresh(); else picker.Refresh();
            AnchorToSlot(panel, marrow.GetSlotWorldPosition(sel.Value));
        }

        /// <summary>Floats a panel beside its slot (SPRINT_PLAN.md decision
        /// 2), clamped inside the screen. Anchored to the slot's *right*
        /// because the marrow column hugs the left edge of the base band and
        /// the tissue band -- the half of the screen with room in it -- is
        /// to its right.</summary>
        private void AnchorToSlot(VisualElement panel, Vector3 worldPos)
        {
            if (root.panel == null || Camera.main == null) return;

            Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, worldPos, Camera.main);

            float w = panel.resolvedStyle.width;
            float h = panel.resolvedStyle.height;
            if (float.IsNaN(w) || w <= 0f) return;   // first frame, before layout

            float pad = 4 * UiTheme.S;
            float x = p.x + pad + 24f;
            float y = p.y - h * 0.5f;

            float maxX = root.resolvedStyle.width - w - pad;
            float maxY = root.resolvedStyle.height - h - pad;
            panel.style.left = Mathf.Clamp(x, pad, Mathf.Max(pad, maxX));
            panel.style.top = Mathf.Clamp(y, pad, Mathf.Max(pad, maxY));
        }

        private void PlayFlashPreview()
        {
            float sz = BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision;
            int mid = board.Rows / 2;
            int c0 = board.Columns / 2 - 6;
            DegranulationFlash.Play(board.CoarseToWorldCenter(new CoarseCoord(c0 + 0, mid)), sz, DegranulationFlash.GranuleBurstColor);
            DegranulationFlash.Play(board.CoarseToWorldCenter(new CoarseCoord(c0 + 3, mid)), sz, DegranulationFlash.BreachBurstColor);
            DegranulationFlash.Play(board.CoarseToWorldCenter(new CoarseCoord(c0 + 6, mid)), sz, DegranulationFlash.EfferocytosisColor);
            DegranulationFlash.Play(board.CoarseToWorldCenter(new CoarseCoord(c0 + 9, mid)), sz * 1.5f, DegranulationFlash.StressKillColor);
            DegranulationFlash.Play(board.CoarseToWorldCenter(new CoarseCoord(c0 + 12, mid)), sz, DegranulationFlash.KnowledgeMatchColor);
        }
    }
}
