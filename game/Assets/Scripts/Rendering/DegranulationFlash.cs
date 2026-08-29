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
        /// <summary>Default burst lifetime. Sprint 13: per-event timing is
        /// set per-instance in <see cref="Begin"/> (breach fastest, effero
        /// slowest); this stays as the nominal value / external reference.</summary>
        public const float DurationSeconds = 0.45f;

        // Sprint 13: per-instance so each event can have its own timing/size
        // as well as its own colour and shape. Defaulted to the old consts.
        private float durationSeconds = DurationSeconds;
        private float startScale = 0.35f;
        private float endScale = 1.6f;

        /// <summary>GAME_DESIGN.md §8: a hard, tunable ceiling on simultaneous
        /// cosmetic flashes so the pool degrades gracefully under load
        /// rather than growing unbounded. Requests past this are dropped.</summary>
        public static int MaxConcurrent = 24;
        private static int active;

        /// <summary>Hot granule yellow-white, deliberately unlike anything
        /// else on the board (host pink, pathogen maroon, macrophage blue,
        /// neutrophil amber, marrow tan, lymph green).</summary>
        public static readonly Color GranuleBurstColor = new Color(1f, 0.97f, 0.72f);

        /// <summary>Sprint 4: a gut-wall breach burst. Hot pathogen red,
        /// deliberately unlike the neutrophil's granule yellow -- the two
        /// events must never be confused for one another, since one is the
        /// player winning and the other is the player losing ground.</summary>
        public static readonly Color BreachBurstColor = new Color(1f, 0.35f, 0.22f);

        /// <summary>Sprint 5: a macrophage finishing off a debris pile
        /// (efferocytosis). A calm blue-green -- the player RECOVERING
        /// ground, so it must read as unlike both the granule burst and the
        /// breach. Deliberately gentler than either.</summary>
        public static readonly Color EfferocytosisColor = new Color(0.45f, 0.80f, 0.68f);

        /// <summary>Sprint 6: an immune cell recognising an infected cell on
        /// contact and killing it LOUDLY (GAME_DESIGN.md §4b's stress-sense
        /// roll). A harsh magenta-pink, distinct from granule yellow, breach
        /// red, and efferocytosis blue-green -- and this one is played
        /// bigger, because a necrotic kill is meant to read as violent, not
        /// tidy.</summary>
        public static readonly Color StressKillColor = new Color(0.95f, 0.40f, 0.80f);

        /// <summary>Sprint 8: a dendritic cell and a helper-T cell pairing in
        /// the lymph node with MATCHING barcodes -- the adaptive system just
        /// learned something (GAME_DESIGN.md §5c). A clean bright green,
        /// unlike every other burst colour: this one is the player's
        /// long-game investment paying off, and it happens away from the
        /// tissue board so it must read on its own.</summary>
        public static readonly Color KnowledgeMatchColor = new Color(0.40f, 0.92f, 0.45f);

        private Color burstColor = GranuleBurstColor;

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
        public static void Play(Vector3 worldPosition, float worldSize) =>
            Play(worldPosition, worldSize, GranuleBurstColor);

        /// <summary>Same burst, in a caller-chosen colour -- so a second
        /// kind of event (Sprint 4's breach) can reuse the pooled effect
        /// instead of duplicating it.</summary>
        public static void Play(Vector3 worldPosition, float worldSize, Color color)
        {
            if (pool == null) return;
            if (active >= MaxConcurrent) return; // §8 cap -- drop, don't queue
            var go = pool.Get();
            go.transform.position = worldPosition;
            var flash = go.GetComponent<DegranulationFlash>();
            if (flash != null) flash.Begin(worldSize, color);
        }

        private void Begin(float worldSize, Color color)
        {
            baseSize = worldSize;
            burstColor = color;
            age = 0f;
            active++;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            // Sprint 13: shape + timing per event, keyed off the (static
            // readonly) burst colour, so every Play(...) call site is
            // unchanged. Ring vs. bloom vs. stipple vs. spiky-star keeps the
            // five unmistakable when they overlap / on a screenshot / for
            // colour-blind players.
            sr.sprite = ShapeFor(color);
            sr.sortingOrder = 30; // above units (10) and pathogens (20)
            sr.color = burstColor;
            sr.enabled = true;
            ApplyScale(0f);
        }

        private Sprite ShapeFor(Color c)
        {
            if (Same(c, BreachBurstColor))    { durationSeconds = 0.35f; startScale = 0.40f; endScale = 1.9f; return SpriteShapes.BreachStar; }
            if (Same(c, EfferocytosisColor))  { durationSeconds = 0.55f; startScale = 0.30f; endScale = 1.3f; return SpriteShapes.EffeBloom; }
            if (Same(c, StressKillColor))     { durationSeconds = 0.45f; startScale = 0.35f; endScale = 1.6f; return SpriteShapes.StressRing; }
            if (Same(c, KnowledgeMatchColor)) { durationSeconds = 0.50f; startScale = 0.30f; endScale = 1.4f; return SpriteShapes.KnowledgeRing; }
            // default = neutrophil degranulation
            durationSeconds = 0.40f; startScale = 0.35f; endScale = 1.6f;
            return SpriteShapes.GranuleBurst;
        }

        private static bool Same(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.001f && Mathf.Abs(a.g - b.g) < 0.001f && Mathf.Abs(a.b - b.b) < 0.001f;

        private void Update()
        {
            if (sr == null) return;

            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / durationSeconds);
            ApplyScale(t);

            var c = burstColor;
            c.a = 1f - t;
            sr.color = c;

            if (t >= 1f)
            {
                sr = null;
                active = Mathf.Max(0, active - 1);
                if (pool != null) pool.Release(gameObject);
                else gameObject.SetActive(false);
            }
        }

        private void ApplyScale(float t)
        {
            float s = baseSize * Mathf.Lerp(startScale, endScale, t);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
