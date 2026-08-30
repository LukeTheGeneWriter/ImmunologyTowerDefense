using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.Units
{
    public enum BoneMarrowSlotState { Empty, Placed }

    /// <summary>
    /// Sprint 2's real bone-marrow placement, replacing Sprint 1's
    /// random-fine-coord debug spawn (GameBootstrap.SpawnUnits, now gone --
    /// see docs/TEAM_RETRO.md's Sprint 1 note on that being an explicit
    /// stand-in, not a design decision). A small number of slots
    /// (BoneMarrowSlotCount) sit in their own visually distinct compartment
    /// below the tissue board (GAME_DESIGN.md section 1). An empty slot is
    /// clickable (BoneMarrowSlot.OnMouseDown); clicking opens a two-button
    /// IMGUI picker (Macrophage / Neutrophil -- no uGUI package this
    /// project). Once placed, a slot is a persistent progenitor tower that
    /// periodically emits a unit of that kind from the blood-side edge
    /// (GAME_DESIGN.md section 2a's rung-1 uniform entry: "cells
    /// extravasate at random points along the vessel") -- no ATP cost this
    /// sprint (SPRINT_PLAN.md: placement is free).
    ///
    /// **Sprint 3: population homeostasis (GAME_DESIGN.md section 6d).**
    /// Sprint 2's tower emitted forever and nothing ever despawned, so the
    /// active unit count grew without bound -- the problem Sprint 3 exists
    /// to fix. A tower is now bounded two ways at once, deliberately, each
    /// doing a different job:
    ///
    ///  - **Emission rate** (EmissionIntervalSeconds, unchanged from
    ///    Sprint 2) -- one new cell per interval at most, which is the
    ///    tower's DPS cap. Kept as an independent second cap specifically so
    ///    a tower whose whole population just died cannot burst back to
    ///    full instantly.
    ///  - **Max active children** (new) -- a hard ceiling on how many of a
    ///    tower's OWN children are alive at once. At the ceiling the tower
    ///    stops emitting even if its timer has elapsed, until a child dies.
    ///    Per-tower, NOT systemic: an explicit break from real
    ///    hematopoiesis (which is G-CSF-regulated body-wide), made because
    ///    it is what gives a future per-tower upgrade something to attach
    ///    to. Do not "fix" this back toward biological accuracy without
    ///    reading GAME_DESIGN.md section 6d first.
    ///
    /// Each tower owns a mutable UnitLifecycleTuning seeded from its kind's
    /// UnitProfile defaults at placement; GetTuning(index) hands it out so a
    /// future upgrade is one field write and nothing more.
    ///
    /// Tick(float deltaTime) -- not an implicit Update() reading
    /// UnityEngine.Time -- is the actual emission-timer logic, matching the
    /// project's established pattern (TissueGrid/CytokineField/Chemotaxis)
    /// of taking explicit time so a headless verification harness can drive
    /// the real production method (see Assets/Editor/LifecycleVerification.cs).
    /// Update() just calls Tick(Time.deltaTime).
    /// </summary>
    public class BoneMarrowManager : MonoBehaviour
    {
        private class Slot
        {
            public BoneMarrowSlotState State;
            public UnitKind Kind;
            public float EmissionTimer;
            public SpriteRenderer Visual;
            public Vector3 WorldPosition;

            /// <summary>This tower's own mutable lifecycle numbers, seeded
            /// from the kind's UnitProfile defaults at PlaceTower time. A
            /// future upgrade writes here and nothing else.</summary>
            public UnitLifecycleTuning Tuning;

            /// <summary>This tower's currently-alive children. A list rather
            /// than a bare counter so the HUD, the verification harness, and
            /// any future "recall your cells" mechanic can actually reach
            /// them. Bounded by Tuning.MaxActiveChildren (10 by default), so
            /// the O(n) Remove on despawn is trivially cheap.</summary>
            public readonly List<SearchUnit> Children = new List<SearchUnit>();

            /// <summary>Sprint 8: for the two ADAPTIVE kinds, children are
            /// DendriticCell / Lymphocyte GameObjects owned by
            /// AdaptiveDirector, not SearchUnits. Tracked here only so the
            /// per-tower MaxActiveChildren cap and the round boundary have a
            /// count / a list. Empty for innate towers.</summary>
            public readonly List<GameObject> AdaptiveChildren = new List<GameObject>();

            /// <summary>Cached despawn callback handed to every child this
            /// tower emits -- built once at placement so emission allocates
            /// no closure per unit (GAME_DESIGN.md section 8).</summary>
            public System.Action<SearchUnit> OnChildDespawned;

            /// <summary>Sprint 11: placeholder per-tower upgrade level. Spends
            /// ATP, bumps this, does nothing else yet (§6d).</summary>
            public int UpgradeLevel;
        }

        /// <summary>The two adaptive kinds emit their own agent types via
        /// <see cref="AdaptiveDirector"/> rather than a SearchUnit.</summary>
        public static bool IsAdaptive(UnitKind k) =>
            k == UnitKind.DendriticCell || k == UnitKind.HelperT;

        /// <summary>Sprint 15: fired with the emitting slot's world position
        /// each time a progenitor emits a child, so the base compartment
        /// renderer can bud a "cell born" mote in the marrow (one puff per
        /// real emission, per the Director). Cosmetic, process-global,
        /// null-safe in harnesses -- same pattern as EconomyHooks.PayForKill.</summary>
        public static System.Action<Vector3> OnCellEmitted;

        /// <summary>Seconds between emissions from a placed tower. A
        /// judgment call, not specified by SPRINT_PLAN.md -- see
        /// docs/TEAM_RETRO.md. Chosen slower than PathogenSpawner's 2.5s
        /// spawn interval since a player can place several towers at once
        /// (each one emitting independently), but fast enough that placing
        /// a tower and watching it work reads within a few seconds.
        ///
        /// Sprint 3 keeps this exactly as it was, per SPRINT_PLAN.md item 2
        /// -- it is the second, independent cap.</summary>
        public const float EmissionIntervalSeconds = 4f;

        private static readonly Color EmptySlotColor = new Color(0.62f, 0.56f, 0.42f); // pale bone-ish tan

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;

        private UnitProfile macrophageProfile;
        private PrefabPool macrophagePool;
        private UnitProfile neutrophilProfile;
        private PrefabPool neutrophilPool;

        /// <summary>The player's ATP -- placement spends from it (Sprint 7,
        /// GAME_DESIGN.md §5b/§2a). **Nullable:** a headless harness passes
        /// null and placement stays free, exactly as it was before the
        /// economy existed.</summary>
        private ImmunologyTD.Economy.AtpWallet wallet;

        /// <summary>Emission target for the two adaptive kinds. Null in a
        /// harness that only exercises the innate towers -- placing an
        /// adaptive kind is then refused.</summary>
        private AdaptiveDirector adaptive;

        private readonly List<Slot> slots = new List<Slot>();
        private int? pendingChoiceIndex;
        private int? pendingUpgradeIndex;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        /// <summary>Testable hooks for headless verification (see
        /// Assets/Editor/LifecycleVerification.cs) -- the same reasoning as
        /// Chemotaxis/TissueGrid exposing explicit-time methods rather than
        /// only being observable through rendered output.</summary>
        public int EmittedCount { get; private set; }
        public FineCoord LastEmittedStart { get; private set; }
        public UnitKind LastEmittedKind { get; private set; }
        public SearchUnit LastEmittedUnit { get; private set; }

        /// <summary>Total units alive across every tower. Sprint 3's headline
        /// observable: this is the number that used to grow without bound.
        /// Shown in the HUD (HudOverlay) so the Director can watch it stay
        /// bounded rather than being asked to trust it.</summary>
        public int TotalActiveUnits
        {
            get
            {
                int total = 0;
                for (int i = 0; i < slots.Count; i++) total += slots[i].Children.Count;
                return total;
            }
        }

        public int SlotCount => slots.Count;

        public void Initialize(
            BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField,
            UnitProfile macrophageProfile, PrefabPool macrophagePool,
            UnitProfile neutrophilProfile, PrefabPool neutrophilPool,
            Vector3[] slotWorldPositions, float slotWorldSize,
            ImmunologyTD.Economy.AtpWallet wallet = null,
            AdaptiveDirector adaptive = null)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.macrophageProfile = macrophageProfile;
            this.macrophagePool = macrophagePool;
            this.neutrophilProfile = neutrophilProfile;
            this.neutrophilPool = neutrophilPool;
            this.wallet = wallet;
            this.adaptive = adaptive;

            for (int i = 0; i < slotWorldPositions.Length; i++)
            {
                var slotGo = new GameObject($"BoneMarrowSlot_{i}");
                slotGo.transform.SetParent(transform, false);
                slotGo.transform.position = slotWorldPositions[i];
                slotGo.transform.localScale = new Vector3(slotWorldSize, slotWorldSize, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = ImmunologyTD.Rendering.SpriteShapes.SlotNiche; // Sprint 13 -- recessed socket
                sr.sortingOrder = 5;
                sr.color = EmptySlotColor;

                var col = slotGo.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;

                var comp = slotGo.AddComponent<BoneMarrowSlot>();
                comp.Init(this, i);

                slots.Add(new Slot
                {
                    State = BoneMarrowSlotState.Empty,
                    Visual = sr,
                    WorldPosition = slotWorldPositions[i],
                });
            }
        }

        public void OnSlotClicked(int index)
        {
            if (index < 0 || index >= slots.Count) return;
            if (slots[index].State == BoneMarrowSlotState.Empty)
            {
                pendingChoiceIndex = index;
                pendingUpgradeIndex = null;
            }
            else
            {
                // Sprint 11: clicking a PLACED tower opens its upgrade panel.
                pendingUpgradeIndex = index;
                pendingChoiceIndex = null;
            }
        }

        /// <summary>Sprint 11: a placeholder per-tower upgrade (GAME_DESIGN.md
        /// §6d says an upgrade is "a write to one tower's field" -- this
        /// spends the ATP and bumps a level counter, but does **not** touch
        /// the tower's <see cref="UnitLifecycleTuning"/> yet). Public for the
        /// harness.</summary>
        public bool UpgradeTower(int index)
        {
            if (index < 0 || index >= slots.Count) return false;
            var slot = slots[index];
            if (slot.State != BoneMarrowSlotState.Placed) return false;

            int price = ImmunologyTD.Economy.ShopTuning.ProgenitorUpgradePrice(slot.UpgradeLevel);
            if (wallet != null && !wallet.TrySpend(price)) return false;
            slot.UpgradeLevel++;
            return true;
        }

        public int GetUpgradeLevel(int index) =>
            index >= 0 && index < slots.Count ? slots[index].UpgradeLevel : 0;

        /// <summary>Places a tower -- public so both the IMGUI picker
        /// buttons and a headless verification harness call the same real
        /// placement path. No-ops on an already-placed slot (no upgrade/
        /// replace mechanic this sprint).
        ///
        /// Sprint 3: this is also where the tower's own mutable lifecycle
        /// numbers are seeded from the kind's UnitProfile defaults, and
        /// where its per-child despawn callback is built once.</summary>
        public void PlaceTower(int index, UnitKind kind)
        {
            if (index < 0 || index >= slots.Count) return;
            var slot = slots[index];
            if (slot.State != BoneMarrowSlotState.Empty) return;

            // An adaptive kind needs the AdaptiveDirector to emit into.
            if (IsAdaptive(kind) && adaptive == null) return;

            // Sprint 7: placement costs ATP (GAME_DESIGN.md §2a/§5b). A
            // null wallet (harness) keeps placement free. If the player
            // can't afford it, nothing happens -- the picker button is also
            // greyed out, this is belt-and-braces.
            if (wallet != null && !wallet.TrySpend(PriceFor(kind))) return;

            slot.State = BoneMarrowSlotState.Placed;
            slot.Kind = kind;
            slot.EmissionTimer = 0f;
            slot.Children.Clear();
            slot.AdaptiveChildren.Clear();
            // Innate towers seed a full lifecycle-tuning copy from their
            // profile; adaptive towers only need the MaxActiveChildren
            // ceiling (their agents have no kill-count depletion).
            slot.Tuning = IsAdaptive(kind)
                ? new UnitLifecycleTuning { MaxActiveChildren = AdaptiveCapFor(kind) }
                : UnitLifecycleTuning.FromProfile(ProfileFor(kind));
            int capturedIndex = index;
            slot.OnChildDespawned = unit => OnChildDespawned(capturedIndex, unit);
            slot.Visual.color = ColorForKind(kind);

            if (pendingChoiceIndex == index) pendingChoiceIndex = null;
        }

        public BoneMarrowSlotState GetSlotState(int index) => slots[index].State;
        public UnitKind GetSlotKind(int index) => slots[index].Kind;

        /// <summary>How many of this tower's own children are alive right
        /// now. Compared against GetTuning(index).MaxActiveChildren. For an
        /// adaptive tower this counts DendriticCell / Lymphocyte agents.</summary>
        public int GetActiveChildren(int index) =>
            IsAdaptive(slots[index].Kind)
                ? slots[index].AdaptiveChildren.Count
                : slots[index].Children.Count;

        /// <summary>This tower's live, mutable tuning instance -- null until
        /// the slot is placed. Handed out by reference on purpose: a future
        /// progenitor upgrade is meant to be
        /// `manager.GetTuning(i).KillLimit += 5` and nothing else
        /// (GAME_DESIGN.md section 6d, Director 2026-08-21).</summary>
        public UnitLifecycleTuning GetTuning(int index) => slots[index].Tuning;

        /// <summary>Read-only view of one tower's live children, for the
        /// HUD and for Assets/Editor/LifecycleVerification.cs.</summary>
        public IReadOnlyList<SearchUnit> GetChildren(int index) => slots[index].Children;

        /// <summary>Real emission-timer tick, taking deltaTime explicitly
        /// rather than reading UnityEngine.Time -- see class comment.
        /// Update() calls this with Time.deltaTime every frame; a headless
        /// harness calls it directly with a simulated step.</summary>
        public void Tick(float deltaTime)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.State != BoneMarrowSlotState.Placed) continue;

                float interval = IntervalFor(slot.Kind);
                slot.EmissionTimer += deltaTime;
                if (slot.EmissionTimer < interval) continue;

                // At the max-active-children ceiling: hold the timer AT the
                // interval instead of letting it bank up. This is what makes
                // the two caps genuinely independent (SPRINT_PLAN.md item 2 /
                // GAME_DESIGN.md section 6d): a tower whose entire population
                // dies at once has at most one emission ready to go, and then
                // has to wait a full interval for each subsequent cell --
                // it refills at the emission rate, it does not burst back to
                // full. Without the clamp, a tower blocked for 40s would bank
                // ten emissions and dump them all on the tick a child died.
                if (GetActiveChildren(i) >= slot.Tuning.MaxActiveChildren)
                {
                    slot.EmissionTimer = interval;
                    continue;
                }

                slot.EmissionTimer -= interval;
                Emit(i, slot);
            }
        }

        /// <summary>ATP price for a tower of this kind (GAME_DESIGN.md §5b).
        /// Public so the HUD / picker can show it and grey out what the
        /// player can't afford.</summary>
        public static int PriceFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return ImmunologyTD.Economy.EconomyTuning.MacrophagePrice;
                case UnitKind.Neutrophil: return ImmunologyTD.Economy.EconomyTuning.NeutrophilPrice;
                case UnitKind.DendriticCell: return ImmunologyTD.Economy.EconomyTuning.DendriticCellPrice;
                default: return ImmunologyTD.Economy.EconomyTuning.HelperTPrice;
            }
        }

        private static float IntervalFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.DendriticCell: return AdaptiveTuning.DcEmissionIntervalSeconds;
                case UnitKind.HelperT: return AdaptiveTuning.LymphocyteEmissionIntervalSeconds;
                default: return EmissionIntervalSeconds;
            }
        }

        private static int AdaptiveCapFor(UnitKind kind) =>
            kind == UnitKind.DendriticCell
                ? AdaptiveTuning.DcMaxActiveChildren
                : AdaptiveTuning.LymphocyteMaxActiveChildren;

        private Color ColorForKind(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return macrophageProfile.Color;
                case UnitKind.Neutrophil: return neutrophilProfile.Color;
                case UnitKind.DendriticCell: return new Color(0.72f, 0.30f, 0.68f); // dendritic magenta
                default: return new Color(0.32f, 0.72f, 0.70f);                     // helper-T teal
            }
        }

        private static string KindLabel(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.Macrophage: return "Macrophage";
                case UnitKind.Neutrophil: return "Neutrophil";
                case UnitKind.DendriticCell: return "Dendritic";
                default: return "Helper-T";
            }
        }

        /// <summary>Despawns every fielded immune cell of every tower --
        /// the round boundary (GAME_DESIGN.md §2: "the cells they emit ...
        /// die at the end of the round"). The towers stay placed; their
        /// emission timers reset so each re-emits from scratch next round.
        /// Called by RoundController when a round clears.</summary>
        public void ClearFieldedUnits()
        {
            // One call clears every fielded adaptive agent (DCs + lymphocytes)
            // of every adaptive tower; each agent's despawn callback drops it
            // from its slot's AdaptiveChildren list.
            adaptive?.DespawnAllFielded();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.State != BoneMarrowSlotState.Placed) continue;

                // Iterate a copy -- OnChildDespawned mutates slot.Children.
                var children = slot.Children.ToArray();
                for (int c = 0; c < children.Length; c++)
                {
                    OnChildDespawned(i, children[c]);
                }
                slot.AdaptiveChildren.Clear(); // belt-and-braces if a callback was missed
                slot.EmissionTimer = 0f;
            }
        }

        private UnitProfile ProfileFor(UnitKind kind) =>
            kind == UnitKind.Macrophage ? macrophageProfile : neutrophilProfile;

        private PrefabPool PoolFor(UnitKind kind) =>
            kind == UnitKind.Macrophage ? macrophagePool : neutrophilPool;

        private void Emit(int slotIndex, Slot slot)
        {
            if (IsAdaptive(slot.Kind))
            {
                EmitAdaptive(slotIndex, slot);
                return;
            }

            var profile = ProfileFor(slot.Kind);
            var pool = PoolFor(slot.Kind);

            // Rung-1 entry per GAME_DESIGN.md section 2a: "cells
            // extravasate at random points along the vessel."
            //
            // **Sprint 4 (SPRINT_PLAN.md item 8) moved this.** Through
            // Sprint 3 units entered at the deepest fine ROW, which was the
            // "blood-adjacent edge" back when the board was tissue-only and
            // depth was vertical. Map 01 makes the base a lateral BAND, so
            // the vessel is the tissue's base-side edge: units now enter at
            // a uniformly random lane along that edge and walk outward into
            // the contested middle, directly opposing the pathogen front.
            //
            // Expressed in BoardConfig's axis frame, so moving the base in
            // config moves the entry line with it -- the same architectural
            // rule PathogenAgent.StepTissue follows.
            var entryCell = board.CoarseFromAxis(
                board.TissueBaseEdgeAxisIndex,
                Random.Range(0, board.CrossLength));
            var start = board.CoarseCenterFine(entryCell);

            var go = pool.Get();
            var unit = go.GetComponent<SearchUnit>();
            // The unit receives this tower's LIVE tuning instance, not a
            // copy (Director, 2026-08-21): upgrading a progenitor applies
            // instantly to every one of its currently-fielded children as
            // well as its future ones, because spending ATP should make an
            // immediate difference. See GAME_DESIGN.md section 6d.
            unit.Initialize(board, tissueGrid, cytokineField, profile, start,
                slot.Tuning, slotIndex, slot.OnChildDespawned);
            slot.Children.Add(unit);
            OnCellEmitted?.Invoke(slot.WorldPosition); // Sprint 15: marrow birth-puff

            EmittedCount++;
            LastEmittedStart = start;
            LastEmittedKind = slot.Kind;
            LastEmittedUnit = unit;
        }

        /// <summary>Sprint 8: emit a dendritic cell (into tissue) or a
        /// helper-T cell (into the lymph node) via AdaptiveDirector, and
        /// track the returned GameObject against this tower's cap.</summary>
        private void EmitAdaptive(int slotIndex, Slot slot)
        {
            GameObject go = slot.Kind == UnitKind.DendriticCell
                ? adaptive.EmitDendriticCell(slotIndex, OnAdaptiveChildDespawned)
                : adaptive.EmitLymphocyte(slotIndex, OnAdaptiveChildDespawned);
            if (go == null) return;

            slot.AdaptiveChildren.Add(go);
            OnCellEmitted?.Invoke(slot.WorldPosition); // Sprint 15: marrow birth-puff
            EmittedCount++;
            LastEmittedKind = slot.Kind;
        }

        /// <summary>An adaptive agent (DC / lymphocyte) returned to its pool
        /// -- by lifespan expiry, cargo spend cycling, or the round boundary.
        /// AdaptiveDirector has already released it; this just drops the
        /// marrow's tracking reference so the cap frees up.</summary>
        public void OnAdaptiveChildDespawned(int slotIndex, GameObject go)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            slots[slotIndex].AdaptiveChildren.Remove(go);
        }

        /// <summary>
        /// The other half of Sprint 3's lifecycle: a child that depleted
        /// (degranulated or retired) calls back here. Frees its slot in this
        /// tower's max-active-children count and returns the instance to its
        /// PrefabPool -- the despawn path that simply did not exist before
        /// this sprint (nothing ever called PrefabPool.Release for a
        /// SearchUnit). Public so a harness can exercise it directly.
        /// </summary>
        public void OnChildDespawned(int slotIndex, SearchUnit unit)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || unit == null) return;
            var slot = slots[slotIndex];
            slot.Children.Remove(unit);

            var pool = PoolFor(slot.Kind);
            unit.ResetForPool();
            pool.Release(unit.gameObject);
        }

        private void Update()
        {
            if (board == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: towers don't emit during the frozen buy phase
            Tick(Time.deltaTime);
        }

        private void OnGUI()
        {
            if (board == null || Camera.main == null) return;
            EnsureStyles();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var screen = WorldToGui(slot.WorldPosition);
                // Sprint 3: a placed tower shows "children alive / cap" so
                // the max-active-children ceiling is observable in seconds of
                // play rather than being something the player has to trust.
                string label = slot.State == BoneMarrowSlotState.Empty
                    ? "empty\n(click)"
                    : $"{KindLabel(slot.Kind)}{(slot.UpgradeLevel > 0 ? $" +{slot.UpgradeLevel}" : "")}\n{GetActiveChildren(i)}/{slot.Tuning.MaxActiveChildren} alive";
                GUI.Label(new Rect(screen.x - 45, screen.y - 40, 90, 40), label, labelStyle);
            }

            if (pendingChoiceIndex.HasValue)
            {
                var screen = WorldToGui(slots[pendingChoiceIndex.Value].WorldPosition);
                bool adaptiveAvailable = adaptive != null;
                float panelW = 210;
                float panelH = adaptiveAvailable ? 152 : 92;
                var panelRect = new Rect(screen.x - panelW / 2f, screen.y + 15, panelW, panelH);
                GUI.Box(panelRect, "Place progenitor tower");

                float y = panelRect.y + 28;
                DrawBuyButton(new Rect(panelRect.x + 10, y, panelW - 20, 26), UnitKind.Macrophage, "Macrophage"); y += 30;
                DrawBuyButton(new Rect(panelRect.x + 10, y, panelW - 20, 26), UnitKind.Neutrophil, "Neutrophil"); y += 30;
                if (adaptiveAvailable)
                {
                    DrawBuyButton(new Rect(panelRect.x + 10, y, panelW - 20, 26), UnitKind.DendriticCell, "Dendritic"); y += 30;
                    DrawBuyButton(new Rect(panelRect.x + 10, y, panelW - 20, 26), UnitKind.HelperT, "Helper-T");
                }
            }

            // Sprint 11: the per-tower upgrade panel (placeholder -- spends
            // ATP, bumps the level, no mechanical effect yet).
            if (pendingUpgradeIndex.HasValue)
            {
                int idx = pendingUpgradeIndex.Value;
                var slot = slots[idx];
                var screen = WorldToGui(slot.WorldPosition);
                float panelW = 220, panelH = 84;
                var panelRect = new Rect(screen.x - panelW / 2f, screen.y + 15, panelW, panelH);
                GUI.Box(panelRect, $"{KindLabel(slot.Kind)}  --  upgrade Lv {slot.UpgradeLevel}");

                int price = ImmunologyTD.Economy.ShopTuning.ProgenitorUpgradePrice(slot.UpgradeLevel);
                bool affordable = wallet == null || wallet.CanAfford(price);
                bool wasEnabled = GUI.enabled;
                GUI.enabled = affordable;
                if (GUI.Button(new Rect(panelRect.x + 10, panelRect.y + 28, panelW - 20, 24),
                        $"Upgrade -> Lv {slot.UpgradeLevel + 1}   {price} ATP", buttonStyle) && affordable)
                {
                    UpgradeTower(idx);
                }
                GUI.enabled = wasEnabled;
                if (GUI.Button(new Rect(panelRect.x + 10, panelRect.y + 54, panelW - 20, 22), "close", buttonStyle))
                    pendingUpgradeIndex = null;
            }
        }

        private void DrawBuyButton(Rect rect, UnitKind kind, string label)
        {
            int price = PriceFor(kind);
            bool affordable = wallet == null || wallet.CanAfford(price);
            bool wasEnabled = GUI.enabled;
            GUI.enabled = affordable;
            if (GUI.Button(rect, $"{label}   {price} ATP", buttonStyle) && affordable)
            {
                PlaceTower(pendingChoiceIndex.Value, kind);
            }
            GUI.enabled = wasEnabled;
        }

        private Vector2 WorldToGui(Vector3 worldPos)
        {
            var screen = Camera.main.WorldToScreenPoint(worldPos);
            return new Vector2(screen.x, Screen.height - screen.y);
        }

        private void EnsureStyles()
        {
            if (labelStyle != null) return;
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13 };
        }
    }
}
