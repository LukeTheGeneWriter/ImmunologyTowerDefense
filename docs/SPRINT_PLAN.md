# Sprint Plan — Sprint 9

## Sprint 8 — closed 2026-08-29

Delivered the dendritic-cell shuttle and the 8-bit antigen barcode
(`GAME_DESIGN.md` §5a/§5c): the `ImmunologyTD.Adaptive` namespace
(`Antigen`, `KnowledgeLedger`, `AdaptiveTuning`), debris carrying an
antigen identity, a real `LymphNode` arena with its own co-localisation
cytokine field, `DendriticCell` / `Lymphocyte` agents, `AdaptiveDirector`,
and DC + helper-T as bought progenitor towers sharing the 5 marrow slots.
34 new `AdaptiveVerification` assertions, 340 total, clean Windows build.
Handed to the Director; he playtested it.

## Direction for Sprint 9 (Director, 2026-08-29, from the Sprint 8 playtest)

Playtest notes:

1. Pathogens piled on the gut wall still **burst through during the buy
   phase** — only spawning pauses, the wall keeps rolling breaches and
   pathogens keep walking.
2. A round clearing **abruptly despawns every fielded immune cell** (§2)
   while wall-pile / straggler pathogens keep moving — reads as a bug.
3. **Round 1 is anticlimactic**: with adhesion low (0.12) most pathogens
   ride the lumen out untouched and the round can pass with nothing
   engaged — "it looks like a bug when the game abruptly kills my cells
   while pathogens still move forward."
4. Rounds are **too easy** — the placeholder batch curve (8, +3/round).

Director's model: **freeze time in the buy phase, and keep cells *and*
pathogens on the field round to round.** Plus a difficulty pass —
**all** of: ~2× batch size, higher wall adhesion, faster spawn cadence —
where the cadence mechanism is a **contaminated food item** that enters
the lumen, transits the flow, and **releases the round's pathogen batch
in bursts as it travels**. And each round gets a **gut-themed tagline**
("Incoming lettuce infected with E. coli", "Contaminated water carries
poliovirus"). Economy: **unchanged this pass** — expect ATP to build up
faster now that you're not rebuilding each round; retune later.

This **supersedes the round model of §5d and the "emitted cells die at
round end" of §2** — both get dated updates.

## Scope

### 1. Buy-phase freeze — one authority over "is the simulation running"

New static `ImmunologyTD.Rounds.RoundClock { public static bool Frozen }`
(same pattern as `CytokineToggle.Enabled` / `EconomyHooks`). Starts
`true` (the game opens in the buy phase). `RoundController` sets it:
`false` on `StartRound()`, `true` on a round ending and on `Defeat`.

Everything that advances the simulation early-returns while frozen:
`SearchUnit`, `PathogenAgent`, `DendriticCell` (in tissue), the
`FoodItem`, `BoneMarrowManager.Update`, `PathogenSpawner.Update`,
`AdaptiveDirector.Update`, `TissueDriver`, and the `CytokineField`
recompute. Visual tweens freeze too — a true time-stop, not a slow
drift. The harness path (explicit `Tick`/`SimulationTick`) is unaffected;
only the `Update()`-driven auto-advance is gated.

This alone fixes note 1 (nothing moves in the buy phase, so nothing
breaches) and note 2's abruptness (no despawn — see item 2).

### 2. Persistent field — cells and pathogens carry round to round

- `RoundController` **no longer calls `marrow.ClearFieldedUnits()`** (nor
  the adaptive despawn) on a round ending. Fielded immune cells persist;
  the towers keep their populations. `ClearFieldedUnits` stays on the
  class for a future run-restart, just isn't called at the boundary.
- Pathogens already have no boundary despawn — with time frozen they
  simply hold position through the buy phase and resume.
- **A round ends when its batch has been fully delivered** — the food
  item has emitted all its cargo **and** left the lumen (excreted off the
  downstream end). Whatever is still alive on the board carries into the
  frozen buy phase. `PathogenSpawner.BatchComplete` drops its
  "lumen+tissue empty" requirement and becomes "the round's delivery is
  finished." Predictable round length ≈ the food item's transit time.
- The per-tower `MaxActiveChildren` cap and the clamp-don't-bank emission
  timer (§6d) still bound population across many rounds — nothing
  accumulates without limit.

### 3. Difficulty numbers (`EconomyTuning` / `InvasionTuning`)

Placeholder pass, not final balance:

- `BatchSizeBase` 8 → **16**, `BatchSizeGrowthPerRound` 3 → **6**
  (round 1 = 16, round 5 = 40).
- `InvasionTuning.AdhesionChanceAtWall` 0.12 → **0.30** — the single
  biggest lever on "round 1 felt like nothing happened." Combined with
  the food item releasing pathogens *already near the wall* (item 4),
  most of a batch now sticks and piles up instead of flowing past.
- Spawn cadence: the food item drives emission now (item 4); the bare
  `PathogenSpawner` interval is kept only as a fallback.

### 4. `FoodItem` — the contaminated delivery vehicle

New pooled MonoBehaviour, `ImmunologyTD.Pathogens.FoodItem`.

- One per round. `RoundController.StartRound()` → `spawner.BeginRound(def)`
  spawns it at the lumen entry (upstream end of the flow).
- It **drifts down the lumen** on the flow (cross-axis step, like a
  pathogen riding the current), unattackable — a pure delivery vehicle
  this pass (destructible food is a later idea, flagged).
- As it travels it **emits its cargo in `FoodItemBurstCount` bursts**
  (default 4), `batchSize / burstCount` pathogens per burst, spawned at
  lumen cells **near its current position and hugging the wall** so
  `AdhesionChanceAt` (depth-gated) gives them a high stick rate. Class
  mix per the round definition (item 5).
- Reaching the downstream end → excreted, returns to its pool. The
  round's delivery is "complete" once it has emitted every burst **and**
  exited.
- Rendered as a distinct chunky sprite (a dull food-bolus colour,
  unlike any pathogen).
- New `InvasionTuning` fields: `FoodItemBurstCount` 4,
  `FoodItemLumenStepIntervalSeconds` (its transit speed),
  `FoodItemBurstWallHugDepth` (how close to the wall it drops its cargo).

### 5. Round definitions + taglines

New `ImmunologyTD.Rounds.RoundScript` (static): `RoundDefinition
ForRound(int n)` → `{ string Tagline, (PathogenClass, float)[] CargoMix }`.
~6 hand-written gut-themed rounds (lettuce/E. coli, raw egg/Salmonella,
contaminated water/poliovirus, undercooked pork, etc.), then a
procedural fallback (linear size growth, default class weights) for
rounds past the script. Batch **size** still comes from
`EconomyTuning.BatchSizeForRound`; the definition supplies the **mix**
and the flavour text.

- `RoundController` exposes `string CurrentTagline`.
- `HudOverlay`'s round bar shows it: `Round 3 — "Contaminated water:
  poliovirus"`.

This is a light version of the long-deferred "round batch composition"
backlog item — per-round class mix, no boss rounds or a real curve yet.

### 6. Verification + docs

- Extend `EconomyVerification` (or a small new `RoundVerification`): the
  freeze flag gates the auto-advance; `BatchComplete` is delivery-only
  now; a round boundary leaves fielded cells **and** loose pathogens
  alive; the food item emits its whole cargo and then exits; the round
  boundary re-freezes; `RoundScript.ForRound` returns a tagline for
  scripted and fallback rounds.
- Re-run all seven prior harnesses green.
- `GAME_DESIGN.md` §2 + §5d dated updates (the round model changed);
  `ENGINE_STATUS.md`, `INTERFACE.md`, `CHANGELOG.md`, `BACKLOG.md`,
  `TEAM_RETRO.md`. Clean Windows build, 0 exceptions on launch.

### 7. Not in scope

- **The food item being attackable / a target.** Pure vehicle this pass.
- **Boss / milestone rounds** and a real difficulty *curve* — just the
  ~2× placeholder and the per-round class mix.
- **§5's knowledge threshold ladder** (still the other candidate next
  sprint — MHC-I precise kill, neutralisation, …). Sprint 8's knowledge
  % still unlocks nothing.
- **Emergency granulopoiesis** (§6c breach consequence) — still deferred.
- **A run restart** — `Defeat` is still terminal.
- **Economy retuning** — Director's call: leave `RoundStartLumpSum` /
  `AtpPerKill` alone and judge from the playtest.
- Anything from Sprints 1–8 changing behaviour beyond the round model:
  the invasion loop, firebreak, §4b models, the DC shuttle, pooling,
  population caps all keep working.

## Stopping point (definition of done)

`[~]` = code done + harness-verified, feel unconfirmed. `[x]` = verified
from command output.

- [ ] The buy phase **freezes everything** — pathogens on the wall don't
      breach, cells don't move, the shuttle pauses. Press Space and it
      all resumes.
- [ ] A round ends when the **food item has delivered its batch and left
      the lumen**; the buy phase re-freezes with **last round's cells and
      pathogens still on the board**, and next round's food item delivers
      on top of them.
- [ ] Each round shows a **tagline** in the round bar; the round's
      pathogen **mix** follows its definition.
- [ ] Round 1 now **engages** — ~16 pathogens, most sticking to the wall
      near where the food item drops them, visible pressure building.
- [ ] Everything from Sprints 1–8 still works — Adaptive 34 / Economy 47 /
      Combat 36 / Lifecycle 79 / Map 71 / Tissue 73 re-run green.
- [ ] Round-model verification green.
- [ ] `GAME_DESIGN.md` §2/§5d, `INTERFACE.md`, `ENGINE_STATUS.md`,
      `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md` updated. Clean Windows
      build, 0 exceptions on launch.

The question this sprint answers: **does the round rhythm — a frozen buy
phase, a themed contaminated delivery, a battlefield that persists — feel
like a game now**, and does round 1 finally engage.

## Process note

Head session, inline, commit after each scope item with a reasoning-heavy
message; update `INTERFACE.md` and `TEAM_RETRO.md` as signatures change
and judgment calls are made, not in a final sweep.

## Judgment calls being made up front (Director can overrule at playtest)

- **A round ends when the food item exits**, not when the board is clear
  (the board is never clear now). Round length ≈ food transit time.
- **The food item is not attackable** — a pure vehicle.
- **Towers don't emit during the frozen buy phase** (consistent with
  "freeze time"); they resume on Start. Persisting cells + resumed
  emission is the standing army.
- **`ClearFieldedUnits` is kept but no longer called** at the boundary,
  so a future run-restart still has it.
