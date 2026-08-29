using UnityEngine;
using ImmunologyTD.Economy;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;

namespace ImmunologyTD.Rounds
{
    public enum RoundPhase
    {
        /// <summary>Between rounds. Spawning is idle; the player buys; a
        /// player action starts the next round.</summary>
        Building,

        /// <summary>A round is running: the batch is spawning / in flight.</summary>
        Active,

        /// <summary>The life pool hit 0. Everything frozen.</summary>
        Defeat,
    }

    /// <summary>
    /// The round loop state machine (GAME_DESIGN.md §5d) and the §6c life
    /// pool. Explicit-time <see cref="Tick"/> per the project convention
    /// (TissueGrid / PathogenSpawner / BoneMarrowManager all take time
    /// rather than reading <c>UnityEngine.Time</c>), so
    /// <c>Assets/Editor/EconomyVerification.cs</c> drives the real state
    /// machine with a simulated clock. <see cref="Update"/> only forwards
    /// the real clock and reads the "start round" keypress.
    ///
    /// It does not own the pieces it drives -- the wallet, the spawner, the
    /// tally, the bone marrow -- it coordinates them:
    ///  - <see cref="StartRound"/> tells the spawner how big this round's
    ///    batch is and flips to Active.
    ///  - each Active tick it watches <c>InvasionTally.ReachedBase</c> for
    ///    breaches (→ life loss → possibly Defeat) and the spawner for the
    ///    batch being resolved (→ round clears: lump sum, life regen,
    ///    despawn fielded units, → Building).
    /// </summary>
    public class RoundController : MonoBehaviour
    {
        private AtpWallet wallet;
        private PathogenSpawner spawner;
        private InvasionTally tally;
        private BoneMarrowManager marrow;

        public RoundPhase Phase { get; private set; } = RoundPhase.Building;

        /// <summary>Rounds started. 0 in the opening buy phase; the first
        /// <see cref="StartRound"/> makes it 1.</summary>
        public int RoundNumber { get; private set; }

        /// <summary>Rounds fully cleared -- drives life regeneration.</summary>
        public int RoundsCleared { get; private set; }

        public int Lives { get; private set; }
        public int MaxLives { get; private set; }

        /// <summary>Sprint 9: the current round's gut-themed flavour line,
        /// shown in the HUD round bar ("Contaminated water: poliovirus").
        /// Empty before the first round.</summary>
        public string CurrentTagline { get; private set; } = "";

        /// <summary>Baseline for the per-tick breach delta -- breaches are
        /// counted globally on <c>InvasionTally</c>, so we track how many we
        /// have already charged for.</summary>
        private int breachesCharged;

        /// <summary>The key that starts the next round from a real build.
        /// The HUD also draws a button.</summary>
        public const KeyCode StartRoundKey = KeyCode.Space;

        public void Initialize(AtpWallet wallet, PathogenSpawner spawner, InvasionTally tally, BoneMarrowManager marrow)
        {
            this.wallet = wallet;
            this.spawner = spawner;
            this.tally = tally;
            this.marrow = marrow;

            MaxLives = EconomyTuning.StartingLives;
            Lives = MaxLives;
            RoundNumber = 0;
            RoundsCleared = 0;
            breachesCharged = tally != null ? tally.ReachedBase : 0;
            Phase = RoundPhase.Building;
            RoundClock.Frozen = true; // Sprint 9: the game opens in a frozen buy phase
        }

        /// <summary>Begins the next round: pays nothing (the lump sum is
        /// granted on a round CLEAR, §5d), sizes the batch, arms the
        /// spawner, → Active. No-ops unless in <see cref="RoundPhase.Building"/>.</summary>
        public void StartRound()
        {
            if (Phase != RoundPhase.Building) return;

            RoundNumber++;
            int batch = EconomyTuning.BatchSizeForRound(RoundNumber);

            // Sprint 9: the round is delivered by a contaminated food item
            // whose cargo mix and flavour text come from the round script.
            var def = RoundScript.ForRound(RoundNumber);
            CurrentTagline = def.Tagline;
            spawner?.BeginRound(batch, def);

            RoundClock.Frozen = false; // time runs
            Phase = RoundPhase.Active;
        }

        public void Tick(float deltaTime)
        {
            if (Phase != RoundPhase.Active) return;

            ChargeBreaches();
            if (Phase == RoundPhase.Defeat) return;

            if (spawner != null && spawner.BatchComplete)
            {
                ClearRound();
            }
        }

        /// <summary>Every new breach since we last looked costs one life;
        /// hitting 0 ends the run.</summary>
        private void ChargeBreaches()
        {
            if (tally == null) return;
            int reached = tally.ReachedBase;
            if (reached <= breachesCharged) return;

            Lives -= reached - breachesCharged;
            breachesCharged = reached;

            if (Lives <= 0)
            {
                Lives = 0;
                Phase = RoundPhase.Defeat;
                spawner?.EndBatch();
                RoundClock.Frozen = true; // GAME OVER -- everything stops
            }
        }

        private void ClearRound()
        {
            RoundsCleared++;

            // §5b: the round-start lump sum, granted here and framed as the
            // budget for starting the next round.
            wallet?.Grant(EconomyTuning.RoundStartLumpSum);

            // §6c: convalescence.
            if (EconomyTuning.LifeRegenRounds > 0
                && RoundsCleared % EconomyTuning.LifeRegenRounds == 0)
            {
                Lives = Mathf.Min(MaxLives, Lives + EconomyTuning.LifeRegenAmount);
            }

            // Sprint 9 (Director, 2026-08-29): the battlefield PERSISTS.
            // §2's "the cells they emit die at the end of the round" is
            // retired -- fielded immune cells and loose pathogens both carry
            // into the (frozen) buy phase and the next round delivers on top
            // of them. `marrow.ClearFieldedUnits()` is kept on the class for
            // a future run-restart but is no longer called here.
            spawner?.EndBatch();
            RoundClock.Frozen = true; // freeze the field for the buy phase
            Phase = RoundPhase.Building;
        }

        /// <summary>Sprint 9: no longer called at the round boundary — the
        /// battlefield persists — but kept for a future run-restart, which
        /// wants a clean field. Despawns every fielded immune cell; the
        /// towers stay placed.</summary>
        public void DespawnAllFieldedUnits() => marrow?.ClearFieldedUnits();

        private void Update()
        {
            if (wallet == null) return; // not initialized
            Tick(Time.deltaTime);

            if (Phase == RoundPhase.Building && Input.GetKeyDown(StartRoundKey))
            {
                StartRound();
            }
        }
    }
}
