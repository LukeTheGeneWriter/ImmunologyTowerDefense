using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ImmunologyTD.Pooling
{
    /// <summary>
    /// Generic pooled spawner for a single prefab (enemy, projectile, or
    /// hit-effect). Required from Sprint 0 onward per GAME_DESIGN.md's
    /// performance requirement -- late-round entity counts must not
    /// allocate/destroy per-instance. Attach one of these per prefab type,
    /// or manage several from a central spawner that holds one PrefabPool
    /// per enemy/projectile type.
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int defaultCapacity = 32;
        [SerializeField] private int maxSize = 512;

        private ObjectPool<GameObject> pool;

        private void Awake() => EnsurePool();

        /// <summary>
        /// Lazily builds the underlying ObjectPool if Awake() hasn't run
        /// yet. Added during Sprint 2: Awake() is only guaranteed to fire
        /// automatically inside Play Mode / a running build -- it does NOT
        /// fire just from AddComponent() in Editor batchmode outside Play
        /// Mode (a corrected assumption from Sprint 1 -- see
        /// docs/TEAM_RETRO.md), which a headless verification harness
        /// (Assets/Editor/CombatVerification.cs) hit directly as a
        /// NullReferenceException in Get(). Idempotent and safe to call
        /// from Get()/Release() as a defensive guard even in normal
        /// gameplay, where Awake will already have run.
        /// </summary>
        private void EnsurePool()
        {
            if (pool != null) return;
            pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        /// <summary>
        /// Runtime assignment of the pooled prefab, for pools built up
        /// entirely from code (no Inspector-time drag-and-drop available)
        /// -- e.g. Sprint 1's GameBootstrap, which constructs its own
        /// template GameObjects at runtime. Safe to call any time before
        /// the first Get(): the pool's createFunc closes over the `prefab`
        /// field and reads it lazily, not at construction time.
        /// </summary>
        public void SetPrefab(GameObject prefabToUse) => prefab = prefabToUse;

        public GameObject Get() { EnsurePool(); return pool.Get(); }

        public void Release(GameObject instance) { EnsurePool(); pool.Release(instance); }

        public int CountActive => pool?.CountActive ?? 0;
        public int CountInactive => pool?.CountInactive ?? 0;
    }
}
