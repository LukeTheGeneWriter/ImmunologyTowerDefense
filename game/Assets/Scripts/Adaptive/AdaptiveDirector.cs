using System.Collections.Generic;
using UnityEngine;
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
    /// Runs its own simulated <see cref="Clock"/>, advanced only in
    /// <see cref="Tick"/> -- Update() forwards Time.deltaTime, the harness
    /// calls Tick(dt) directly. It does not need to align with the tissue
    /// board's Time.time; lifespan and pairing only need it internally
    /// consistent.
    ///
    /// **Sprint 8 item 3 scope:** lymphocytes + the node tick. The dendritic
    /// cell and its tissue->node->tissue shuttle land in item 4; this class
    /// grows a DC pool and a fielded-DC tick then.
    /// </summary>
    public class AdaptiveDirector : MonoBehaviour
    {
        private LymphNode node;
        private PrefabPool lymphocytePool;

        /// <summary>The director's simulated clock (seconds). Advanced in
        /// <see cref="Tick"/>.</summary>
        public float Clock { get; private set; }

        // Fielded lymphocytes, keyed by the marrow slot that emitted them, so
        // BoneMarrowManager's per-tower MaxActiveChildren cap has a count to
        // read and the round boundary has a list to despawn.
        private readonly Dictionary<int, List<Lymphocyte>> lymphocytesBySlot = new Dictionary<int, List<Lymphocyte>>();

        public LymphNode Node => node;

        public void Initialize(LymphNode node, PrefabPool lymphocytePool)
        {
            this.node = node;
            this.lymphocytePool = lymphocytePool;
        }

        /// <summary>Real tick -- advances the clock and the node
        /// (co-localisation field, pairing, lymphocyte movement, lifespan).</summary>
        public void Tick(float deltaTime)
        {
            if (node == null || deltaTime <= 0f) return;
            Clock += deltaTime;
            node.Tick(deltaTime, Clock);
        }

        private void Update()
        {
            if (node == null) return;
            Tick(Time.deltaTime);
        }

        /// <summary>Emit one helper-T cell into the node with a fresh random
        /// 8-bit tag. <paramref name="onSlotChildDespawned"/> is the marrow
        /// manager's per-slot child-despawned callback (slot index, the
        /// despawned GameObject).</summary>
        public GameObject EmitLymphocyte(int slotIndex, System.Action<int, GameObject> onSlotChildDespawned)
        {
            if (node == null || lymphocytePool == null) return null;

            var go = lymphocytePool.Get();
            var lym = go.GetComponent<Lymphocyte>();
            if (lym == null) lym = go.AddComponent<Lymphocyte>();

            if (!lymphocytesBySlot.TryGetValue(slotIndex, out var list))
            {
                list = new List<Lymphocyte>();
                lymphocytesBySlot[slotIndex] = list;
            }

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

        /// <summary>Live helper-T count for a marrow slot (the cap check).</summary>
        public int LymphocyteCount(int slotIndex) =>
            lymphocytesBySlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;

        /// <summary>Round boundary (§2): despawn every fielded adaptive cell.
        /// The progenitor towers stay placed and re-emit next round.</summary>
        public void DespawnAllFielded()
        {
            foreach (var kv in lymphocytesBySlot)
            {
                var copy = kv.Value.ToArray();
                for (int i = 0; i < copy.Length; i++) copy[i].DespawnToPool();
            }
        }
    }
}
