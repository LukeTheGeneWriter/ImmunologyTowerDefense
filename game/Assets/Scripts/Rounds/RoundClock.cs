using UnityEngine;

namespace ImmunologyTD.Rounds
{
    /// <summary>
    /// The one authority on "is the simulation running." Sprint 9: the buy
    /// phase **freezes time** (Director, 2026-08-29) — not just spawning, as
    /// §5d originally had it, but every moving thing: pathogens on the wall
    /// stop rolling breaches, immune cells stop walking, the dendritic-cell
    /// shuttle pauses. Press Start and it all resumes exactly where it was.
    ///
    /// Same shape as <c>CytokineToggle.Enabled</c> / <c>EconomyHooks</c> — a
    /// process-global read the <see cref="RoundController"/> writes:
    /// <see cref="Frozen"/> is <c>false</c> only while a round is
    /// <c>Active</c>. It opens <c>true</c> (the game starts in the buy
    /// phase).
    ///
    /// <see cref="Time"/> is a simulation clock that only advances while not
    /// frozen — everything that used to pass <c>UnityEngine.Time.time</c>
    /// into a <c>SimulationTick</c> passes this instead, so infection ramps,
    /// breach roll clocks, and burnout timers do **not** fast-forward across
    /// a buy phase. A <see cref="RoundClockDriver"/> advances it.
    ///
    /// Harnesses never touch this — they drive every `SimulationTick` /
    /// `Tick` with their own explicit clock and never run `Update()`.
    /// </summary>
    public static class RoundClock
    {
        /// <summary>True whenever the simulation should hold still — the buy
        /// phase and the defeat screen. Opens true.</summary>
        public static bool Frozen = true;

        /// <summary>Simulation seconds elapsed. Advances only while
        /// <see cref="Frozen"/> is false.</summary>
        public static float Time { get; private set; }

        /// <summary>Called once per frame by <see cref="RoundClockDriver"/>.</summary>
        public static void Advance(float deltaTime)
        {
            if (!Frozen && deltaTime > 0f) Time += deltaTime;
        }

        /// <summary>Full reset — a harness that pokes the statics can call
        /// this afterwards, and a future run-restart will want it.</summary>
        public static void Reset()
        {
            Frozen = true;
            Time = 0f;
        }
    }

    /// <summary>Three lines: advances <see cref="RoundClock.Time"/> every
    /// frame. Added by <c>GameBootstrap</c>. Kept separate from
    /// <see cref="RoundController"/> so the clock advances even if the
    /// controller is mid-transition.</summary>
    public class RoundClockDriver : MonoBehaviour
    {
        private void Update() => RoundClock.Advance(UnityEngine.Time.deltaTime);
    }
}
