using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Sprint 23. A single secreted antibody, drifting in tissue until it
    /// finds an extracellular pathogen to coat.
    ///
    /// **Why this is a real object and not a status effect.** The Director's
    /// ruling (2026-10-25) was that the B-cell tower should put something on
    /// the board you can watch: plasma cells that emit visible antibodies
    /// which drift, stick, and coat. Secretory IgA (Sprint 22) is already an
    /// invisible mechanic -- the Director played a whole session and never
    /// perceived it -- so a second invisible antibody system would have
    /// repeated the one failure this project has actually measured.
    ///
    /// **What coating does is deliberately NOT new.** It calls the same
    /// <see cref="PathogenAgent.SetCoated"/> the IgA work already built, so a
    /// coated organism is pacified, slowed, and takes more damage in tissue
    /// by exactly one code path. Tissue-side antibody and luminal IgA are the
    /// same molecule doing the same thing in two compartments, and the game
    /// should not have two implementations of that.
    ///
    /// Pooled from first implementation, per GAME_DESIGN.md §8 -- the
    /// non-negotiable performance requirement. Follows the
    /// <see cref="DegranulationFlash"/> static-pool idiom: one process-global
    /// pool configured at boot, null-safe so a headless harness that never
    /// configures it simply gets no antibodies rather than a crash.
    /// </summary>
    public class Antibody : MonoBehaviour
    {
        private BoardConfig board;
        private TissueGrid tissueGrid;
        private SpriteRenderer sr;

        private Vector3 velocity;
        private float dieAt;
        private bool active;

        private static PrefabPool pool;

        /// <summary>Configured once at boot. Null = antibodies are disabled,
        /// which is the correct behaviour in a harness that never built a
        /// pool for them.</summary>
        public static void Configure(PrefabPool antibodyPool) => pool = antibodyPool;

        /// <summary>Live count, for the verification harness.</summary>
        public static int ActiveCount { get; private set; }

        public static void ResetStatics()
        {
            pool = null;
            ActiveCount = 0;
        }

        /// <summary>
        /// Secrete one antibody at <paramref name="worldPosition"/>, heading
        /// in a random direction. Returns false (harmlessly) when no pool is
        /// configured.
        /// </summary>
        public static bool Secrete(Vector3 worldPosition, BoardConfig board, TissueGrid tissueGrid,
            float lifespanBonusSeconds = 0f)
        {
            if (pool == null || board == null) return false;

            var go = pool.Get();
            if (go == null) return false;

            var ab = go.GetComponent<Antibody>();
            if (ab == null) { pool.Release(go); return false; }

            ab.Launch(worldPosition, board, tissueGrid, lifespanBonusSeconds);
            return true;
        }

        private void Launch(Vector3 worldPosition, BoardConfig boardConfig, TissueGrid grid,
            float lifespanBonusSeconds)
        {
            board = boardConfig;
            tissueGrid = grid;

            transform.position = worldPosition;
            float size = BoardConfig.FineTileWorldSize * AntibodyTuning.SizeFineTiles;
            transform.localScale = new Vector3(size, size, 1f);

            float angle = Random.value * Mathf.PI * 2f;
            velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f)
                       * AntibodyTuning.DriftWorldUnitsPerSecond;

            dieAt = ImmunologyTD.Rounds.RoundClock.Time
                    + AntibodyTuning.LifespanSeconds + Mathf.Max(0f, lifespanBonusSeconds);

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = SpriteShapes.Antibody;
                sr.color = Palette.Antibody;
                sr.enabled = true;
            }

            if (!active) { active = true; ActiveCount++; }
        }

        private void Update()
        {
            if (!active || board == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // buy phase freezes everything

            float now = ImmunologyTD.Rounds.RoundClock.Time;
            transform.position += velocity * Time.deltaTime;

            if (TryCoatHere() || now >= dieAt) Retire();
        }

        /// <summary>
        /// Coat the first uncoated extracellular pathogen in this antibody's
        /// coarse cell.
        ///
        /// Note it deliberately cannot touch an INTRACELLULAR pathogen: an
        /// antibody cannot reach inside a host cell, which is precisely why
        /// the game needs cytotoxic cells for that job and why the ~10% and
        /// ~20% rungs answer different threats rather than stacking.
        /// </summary>
        private bool TryCoatHere()
        {
            if (tissueGrid == null) return false;

            var coarse = board.WorldToCoarse(transform.position);
            if (!board.InCoarseBounds(coarse)) return true; // drifted off-board: spend it

            var target = tissueGrid.GetAttackableAt(coarse);
            if (target == null || target.IsCoated) return false;

            // Species specificity: antibody is raised against what the
            // adaptive system has actually characterised.
            if (!KnowledgeLicence.RecognisesSpecies(
                    ImmunologyTD.Units.UnitKind.PlasmaCell, target.Species))
            {
                return false;
            }

            target.SetCoated(true);
            DegranulationFlash.Play(
                transform.position,
                BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision * 0.7f,
                DegranulationFlash.OpsonisationColor);
            return true;
        }

        private void Retire()
        {
            if (!active) return;
            active = false;
            ActiveCount--;

            if (sr != null) sr.enabled = false;
            board = null;
            tissueGrid = null;

            if (pool != null) pool.Release(gameObject);
            else gameObject.SetActive(false);
        }

        /// <summary>Mirrors the other pooled types' reset contract.</summary>
        public void ResetForPool()
        {
            if (active) { active = false; ActiveCount--; }
            board = null;
            tissueGrid = null;
            velocity = Vector3.zero;
            if (sr != null) sr.enabled = false;
        }
    }
}
