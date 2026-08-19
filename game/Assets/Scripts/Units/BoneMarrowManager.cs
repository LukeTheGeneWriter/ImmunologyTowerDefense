using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

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
    /// Tick(float deltaTime) -- not an implicit Update() reading
    /// UnityEngine.Time -- is the actual emission-timer logic, matching the
    /// project's established pattern (TissueGrid/CytokineField/Chemotaxis)
    /// of taking explicit time so a headless verification harness can drive
    /// the real production method (see Assets/Editor/CombatVerification.cs).
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
        }

        /// <summary>Seconds between emissions from a placed tower. A
        /// judgment call, not specified by SPRINT_PLAN.md -- see
        /// docs/TEAM_RETRO.md. Chosen slower than PathogenSpawner's 2.5s
        /// spawn interval since a player can place several towers at once
        /// (each one emitting independently), but fast enough that placing
        /// a tower and watching it work reads within a few seconds.</summary>
        public const float EmissionIntervalSeconds = 4f;

        private static readonly Color EmptySlotColor = new Color(0.62f, 0.56f, 0.42f); // pale bone-ish tan

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;

        private UnitProfile macrophageProfile;
        private PrefabPool macrophagePool;
        private UnitProfile neutrophilProfile;
        private PrefabPool neutrophilPool;

        private readonly List<Slot> slots = new List<Slot>();
        private int? pendingChoiceIndex;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        /// <summary>Testable hooks for headless verification (see
        /// Assets/Editor/CombatVerification.cs) -- the same reasoning as
        /// Chemotaxis/TissueGrid exposing explicit-time methods rather than
        /// only being observable through rendered output.</summary>
        public int EmittedCount { get; private set; }
        public FineCoord LastEmittedStart { get; private set; }
        public UnitKind LastEmittedKind { get; private set; }

        public void Initialize(
            BoardConfig board, TissueGrid tissueGrid, CytokineField cytokineField,
            UnitProfile macrophageProfile, PrefabPool macrophagePool,
            UnitProfile neutrophilProfile, PrefabPool neutrophilPool,
            Vector3[] slotWorldPositions, float slotWorldSize)
        {
            this.board = board;
            this.tissueGrid = tissueGrid;
            this.cytokineField = cytokineField;
            this.macrophageProfile = macrophageProfile;
            this.macrophagePool = macrophagePool;
            this.neutrophilProfile = neutrophilProfile;
            this.neutrophilPool = neutrophilPool;

            for (int i = 0; i < slotWorldPositions.Length; i++)
            {
                var slotGo = new GameObject($"BoneMarrowSlot_{i}");
                slotGo.transform.SetParent(transform, false);
                slotGo.transform.position = slotWorldPositions[i];
                slotGo.transform.localScale = new Vector3(slotWorldSize, slotWorldSize, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSprites.SquareSprite;
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
            if (slots[index].State != BoneMarrowSlotState.Empty) return;
            pendingChoiceIndex = index;
        }

        /// <summary>Places a tower -- public so both the IMGUI picker
        /// buttons and a headless verification harness call the same real
        /// placement path. No-ops on an already-placed slot (no upgrade/
        /// replace mechanic this sprint).</summary>
        public void PlaceTower(int index, UnitKind kind)
        {
            if (index < 0 || index >= slots.Count) return;
            var slot = slots[index];
            if (slot.State != BoneMarrowSlotState.Empty) return;

            slot.State = BoneMarrowSlotState.Placed;
            slot.Kind = kind;
            slot.EmissionTimer = 0f;
            slot.Visual.color = kind == UnitKind.Macrophage ? macrophageProfile.Color : neutrophilProfile.Color;

            if (pendingChoiceIndex == index) pendingChoiceIndex = null;
        }

        public BoneMarrowSlotState GetSlotState(int index) => slots[index].State;
        public UnitKind GetSlotKind(int index) => slots[index].Kind;

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

                slot.EmissionTimer += deltaTime;
                if (slot.EmissionTimer < EmissionIntervalSeconds) continue;
                slot.EmissionTimer -= EmissionIntervalSeconds;
                Emit(slot.Kind);
            }
        }

        private void Emit(UnitKind kind)
        {
            var profile = kind == UnitKind.Macrophage ? macrophageProfile : neutrophilProfile;
            var pool = kind == UnitKind.Macrophage ? macrophagePool : neutrophilPool;

            // Rung-1 entry per GAME_DESIGN.md section 2a: "cells
            // extravasate at random points along the vessel" -- uniform
            // random column, fixed at the blood-adjacent edge row (the
            // deepest fine row -- CoarseCoord's Row convention is "0 =
            // shallowest/nearest the lumen," so the deepest row is the
            // blood-adjacent edge; see docs/INTERFACE.md open question 1
            // on reconciling this coarse-row axis with the full
            // compartment depth model).
            int col = Random.Range(0, board.FineColumns);
            int row = board.FineRows - 1;
            var start = new FineCoord(col, row);

            var go = pool.Get();
            go.GetComponent<SearchUnit>().Initialize(board, tissueGrid, cytokineField, profile, start);

            EmittedCount++;
            LastEmittedStart = start;
            LastEmittedKind = kind;
        }

        private void Update()
        {
            if (board == null) return;
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
                string label = slot.State == BoneMarrowSlotState.Empty
                    ? "empty\n(click)"
                    : $"{(slot.Kind == UnitKind.Macrophage ? "Macrophage" : "Neutrophil")}\ntower";
                GUI.Label(new Rect(screen.x - 45, screen.y - 40, 90, 40), label, labelStyle);
            }

            if (pendingChoiceIndex.HasValue)
            {
                var screen = WorldToGui(slots[pendingChoiceIndex.Value].WorldPosition);
                float panelW = 190, panelH = 92;
                var panelRect = new Rect(screen.x - panelW / 2f, screen.y + 15, panelW, panelH);
                GUI.Box(panelRect, "Place progenitor tower");

                var macRect = new Rect(panelRect.x + 10, panelRect.y + 28, panelW - 20, 26);
                if (GUI.Button(macRect, "Macrophage", buttonStyle))
                    PlaceTower(pendingChoiceIndex.Value, UnitKind.Macrophage);

                var neuRect = new Rect(panelRect.x + 10, panelRect.y + 58, panelW - 20, 26);
                if (GUI.Button(neuRect, "Neutrophil", buttonStyle))
                    PlaceTower(pendingChoiceIndex.Value, UnitKind.Neutrophil);
            }
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
