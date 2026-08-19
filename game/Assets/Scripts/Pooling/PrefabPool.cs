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

        private void Awake()
        {
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

        public GameObject Get() => pool.Get();

        public void Release(GameObject instance) => pool.Release(instance);

        public int CountActive => pool?.CountActive ?? 0;
        public int CountInactive => pool?.CountInactive ?? 0;
    }
}
