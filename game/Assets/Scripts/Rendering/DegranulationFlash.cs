using UnityEngine;
using ImmunologyTD.Pooling;

namespace ImmunologyTD.Rendering
{
    /// <summary>
    /// The visible half of a neutrophil degranulating (GAME_DESIGN.md
    /// section 6d, SPRINT_PLAN.md item 3: "make the degranulation event
    /// visibly distinct... so the Director can actually see it happen, not
    /// just watch a unit quietly disappear"). A short expanding, fading
    /// square burst at the dying unit's position.
    ///
    /// This is the ONLY visual difference between the two depletion paths,
    /// on purpose: a macrophage retiring is meant to read as a quiet exit,
    /// so it gets no effect at all. If the two still don't read as
    /// deliberately different in playtest, this is the knob to turn (a
    /// longer/larger burst, or a brief fade-out for the macrophage) -- the
    /// mechanism underneath is identical either way.
    ///
    /// Pooled, per GAME_DESIGN.md section 8's non-negotiable performance
    /// requirement -- effects are named in that section alongside enemies
    /// and projectiles, so this never Instantiates or Destroys. The pool is
    /// a static, set once by GameBootstrap.Configure; Play() is a silent
    /// no-op when it is unset, which is exactly what a headless verification
    /// harness (no rendering, no bootstrap) needs. That means the flash
    /// itself is NOT covered by the headless harness -- the degranulation
    /// *mechanism* is (damage, slot free, despawn), but "did a burst
    /// actually render" is a build-screenshot question, same as Sprint 2's
    /// intracellular-rendering bug.
    ///
    /// Update() drives only the tween (a pure visual, nothing simulation
    /// depends on) -- unlike the lifecycle logic in SearchUnit/
    /// BoneMarrowManager, which takes explicit time so a harness can call it.
    /// </summary>
    public class DegranulationFlash : MonoBehaviour
    {
        /// <summary>Burst lifetime. Long enough to be unmissable at a
        /// glance, short enough not to leave visual litter when several
        /// neutrophils deplete near each other -- a judgment call, see
        /// docs/TEAM_RETRO.md.</summary>
        public const float DurationSeconds = 0.45f;

        /// <summary>Start/end size as a multiple of the size passed to
        /// Play() (one coarse cell, in practice) -- it starts smaller than
        /// the cell and blows out well past it.</summary>
        private const float StartScale = 0.35f;
        private const float EndScale = 1.6f;

        /// <summary>Hot granule yellow-white, deliberately unlike anything
        /// else on the board (host pink, pathogen maroon, macrophage blue,
        /// neutrophil amber, marrow tan, lymph green).</summary>
        private static readonly Color BurstColor = new Color(1f, 0.97f, 0.72f);

        private static PrefabPool pool;

        private SpriteRenderer sr;
        private float age;
        private float baseSize;

        /// <summary>Wires up the shared pool. Called once by GameBootstrap.
        /// Static because threading an effects pool through
        /// BoneMarrowManager -> SearchUnit purely so a dying unit can draw
        /// one square would be three layers of plumbing for a visual; this
        /// project already uses statics for exactly this kind of shared
        /// presentation service (RuntimeSprites.SquareSprite,
        /// CytokineToggle.Enabled, BoardRenderer's color statics).</summary>
        public static void Configure(PrefabPool flashPool) => pool = flashPool;

        /// <summary>Spawns one burst. Silently no-ops if no pool has been
        /// configured (headless harness, or before bootstrap finishes).</summary>
        public static void Play(Vector3 worldPosition, float worldSize)
        {
            if (pool == null) return;
            var go = pool.Get();
            go.transform.position = worldPosition;
            var flash = go.GetComponent<DegranulationFlash>();
            if (flash != null) flash.Begin(worldSize);
        }

        private void Begin(float worldSize)
        {
            baseSize = worldSize;
            age = 0f;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.sortingOrder = 30; // above units (10) and pathogens (20)
            sr.color = BurstColor;
            sr.enabled = true;
            ApplyScale(0f);
        }

        private void Update()
        {
            if (sr == null) return;

            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / DurationSeconds);
            ApplyScale(t);

            var c = BurstColor;
            c.a = 1f - t;
            sr.color = c;

            if (t >= 1f)
            {
                sr = null;
                if (pool != null) pool.Release(gameObject);
                else gameObject.SetActive(false);
            }
        }

        private void ApplyScale(float t)
        {
            float s = baseSize * Mathf.Lerp(StartScale, EndScale, t);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
