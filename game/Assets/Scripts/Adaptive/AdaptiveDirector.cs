using System.Collections.Generic;
using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pooling;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Owns the adaptive-immunity agents that <see cref="Units.BoneMarrowManager"/>
    /// does not: the dendritic-cell pool, the helper-T (lymphocyte) pool, and
    /// the <see cref="LymphNode"/>. The bone marrow stays the slot / picker /
    /// placement-cost / population-cap authority; it delegates *emission* of
    /// the two adaptive kinds here (they are not <c>SearchUnit</c>s and one
    /// of them spawns straight into the node, not tissue).
    ///
    /// Runs the whole adaptive arena on one simulated <see cref="Clock"/>: a
    /// single tick gate here sub-steps both the node (<see cref="LymphNode.Step"/>)
    /// and every fielded DC's state machine, so there is no second
    /// accumulator to drift. Update() forwards Time.deltaTime; a harness
    /// calls <see cref="Tick"/> directly.
    /// </summary>
    public class AdaptiveDirector : MonoBehaviour
    {
        private LymphNode node;
        private PrefabPool lymphocytePool;
        private PrefabPool dcPool;

        private BoardConfig tissueBoard;
        private TissueGrid tissueGrid;
        private CytokineField tissueCytokine;

        /// <summary>The arena's simulated clock (seconds), advanced one tick
        /// interval per sub-step inside <see cref="Tick"/>.</summary>
        public float Clock { get; private set; }

        private float tickAccumulator;

        // Fielded agents keyed by the marrow slot that emitted them, so the
        // per-tower MaxActiveChildren cap has a count and the round boundary
        // has a list.
        private readonly Dictionary<int, List<Lymphocyte>> lymphocytesBySlot = new Dictionary<int, List<Lymphocyte>>();
        private readonly Dictionary<int, List<DendriticCell>> dcsBySlot = new Dictionary<int, List<DendriticCell>>();

        // Flat view of every fielded DC, for the per-tick state-machine sweep.
        private readonly List<DendriticCell> allDcs = new List<DendriticCell>();

        public LymphNode Node => node;

        public void Initialize(
            LymphNode node,
            PrefabPool lymphocytePool, PrefabPool dcPool,
            BoardConfig tissueBoard, TissueGrid tissueGrid, CytokineField tissueCytokine)
        {
            this.node = node;
            this.lymphocytePool = lymphocytePool;
            this.dcPool = dcPool;
            this.tissueBoard = tissueBoard;
            this.tissueGrid = tissueGrid;
            this.tissueCytokine = tissueCytokine;
        }

        /// <summary>Real tick. Sub-steps at <see cref="BoardConfig.TickIntervalSeconds"/>
        /// so a harness may pass one big delta and still get deterministic
        /// per-tick behaviour.</summary>
        public void Tick(float deltaTime)
        {
            if (node == null || deltaTime <= 0f) return;

            tickAccumulator += deltaTime;
            while (tickAccumulator >= BoardConfig.TickIntervalSeconds)
            {
                tickAccumulator -= BoardConfig.TickIntervalSeconds;
                Clock += BoardConfig.TickIntervalSeconds;

                node.Step(Clock);
                // Copy-iterate: a DC's SimulationTick can transition and a
                // despawn could mutate the list.
                for (int i = 0; i < allDcs.Count; i++) allDcs[i].SimulationTick(Clock);
            }
        }

        private void Update()
        {
            if (node == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: the shuttle pauses during the buy phase / defeat
            Tick(Time.deltaTime);
        }

        // ==================================================================
        // Emission -- called by BoneMarrowManager for the two adaptive kinds
        // ==================================================================

        /// <summary>Emit one helper-T cell into the node with a fresh random
        /// 8-bit tag. <paramref name="onSlotChildDespawned"/> is the marrow
        /// manager's per-slot callback (slot index, the despawned GameObject).</summary>
        public GameObject EmitLymphocyte(int slotIndex, System.Action<int, GameObject> onSlotChildDespawned)
        {
            if (node == null || lymphocytePool == null) return null;

            var go = lymphocytePool.Get();
            var lym = go.GetComponent<Lymphocyte>();
            if (lym == null) lym = go.AddComponent<Lymphocyte>();

            var list = ListFor(lymphocytesBySlot, slotIndex);

            System.Action<Lymphocyte> despawn = l =>
            {
                list.Remove(l);
                node.UnregisterResident(l);
                l.ResetForPool();
                lymphocytePool.Release(l.gameObject);
                onSlotChildDespawned?.Invoke(slotIndex, l.gameObject);
            };

            lym.Initialize(node, Antigen.RandomTag(), node.RandomInteriorFine(), Clock, despawn);
            node.RegisterResident(lym);
            list.Add(lym);
            return go;
        }

        /// <summary>Emit one dendritic cell into tissue at a random lane on
        /// the tissue base edge (same entry as the innate units).</summary>
        public GameObject EmitDendriticCell(int slotIndex, System.Action<int, GameObject> onSlotChildDespawned)
        {
            if (dcPool == null || tissueBoard == null) return null;

            var go = dcPool.Get();
            var dc = go.GetComponent<DendriticCell>();
            if (dc == null) dc = go.AddComponent<DendriticCell>();

            var list = ListFor(dcsBySlot, slotIndex);

            System.Action<DendriticCell> despawn = d =>
            {
                list.Remove(d);
                allDcs.Remove(d);
                d.ResetForPool();
                dcPool.Release(d.gameObject);
                onSlotChildDespawned?.Invoke(slotIndex, d.gameObject);
            };

            var start = tissueBoard.CoarseCenterFine(tissueBoard.CoarseFromAxis(
                tissueBoard.TissueBaseEdgeAxisIndex, Random.Range(0, tissueBoard.CrossLength)));
            dc.Initialize(tissueBoard, tissueGrid, tissueCytokine, node, start, despawn);
            list.Add(dc);
            allDcs.Add(dc);
            return go;
        }

        public int LymphocyteCount(int slotIndex) =>
            lymphocytesBySlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;

        public int DendriticCellCount(int slotIndex) =>
            dcsBySlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;

        /// <summary>Round boundary (§2): despawn every fielded adaptive cell.
        /// The progenitor towers stay placed and re-emit next round.</summary>
        public void DespawnAllFielded()
        {
            foreach (var kv in lymphocytesBySlot)
                foreach (var l in kv.Value.ToArray()) l.DespawnToPool();
            foreach (var kv in dcsBySlot)
                foreach (var d in kv.Value.ToArray()) d.DespawnToPool();
        }

        private static List<T> ListFor<T>(Dictionary<int, List<T>> map, int key)
        {
            if (!map.TryGetValue(key, out var list)) { list = new List<T>(); map[key] = list; }
            return list;
        }
    }
}
