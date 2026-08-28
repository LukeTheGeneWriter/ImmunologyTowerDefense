# Sprint Plan — Sprint 7

## Sprint 6 — closed 2026-08-28

Delivered `GAME_DESIGN.md` §4b: the contact stress-sense roll (an
established intracellular infection is unreachable by ordinary innate
damage; a low per-tick recognition roll → a loud necrotic kill), the real
intracellular-bacterium model (roam/protected/replicate → brood burst;
caught early = no brood), and virus budding + spontaneous burn-out. 259
harness assertions, clean build. Handed to the Director; the "watch it
happen" halves are his playtest.

## Direction for Sprint 7 (Director, 2026-08-28)

Build the **ATP economy framework** and the **round structure** — a
playable skeleton the Director can exercise, numbers deliberately
placeholder. Three decisions taken up front:

- **Round model: wave batch + buy phase.** Each round spawns a defined
  batch of pathogens. The round ends when that batch is resolved —
  everything either cleared, excreted, or reached the base — **except**
  pathogens colonising the gut wall, which persist round to round per
  §6b. Spawning then pauses; the player buys; a player action starts the
  next round.
- **Placement costs ATP.** A per-kind price, deducted on placement, no
  placement you can't afford.
- **The 100-life pool is wired** (§6c): a breach costs a life, lives
  regenerate slowly, the run ends at 0. The *emergency granulopoiesis*
  consequence (§6c's acute punishment) stays deferred.

Authority: `GAME_DESIGN.md` §5b (ATP income), the new §5d (round loop),
§6c (breach cost), §2 (towers persist, emitted cells die at round end).

## Scope

### 1. `AtpWallet` + income

A plain `AtpWallet` (balance, `TrySpend`, `Grant`, `CanAfford`). Two income
sources, per §5b:

- **Round-start lump sum** — granted when a round clears (framed as the
  budget for starting the next round; the between-rounds "+N ATP" the
  Director sees). Game start seeds `StartingAtp` instead.
- **Per kill** — a flat `AtpPerKill` on every pathogen a unit kills,
  routed through the single `SearchUnit.RegisterKill` chokepoint (covers
  contact kills and stress-sense kills; not brood-burst / burn-out /
  drain-death).

### 2. `RoundController` — the state machine

`RoundPhase { Building, Active, Defeat }`, explicit-time `Tick(dt)` per the
project convention.

- **Building** — spawner idle. A player action (`Space`, or a HUD button)
  → `StartRound()`.
- **`StartRound()`** — `RoundNumber++`, computes this round's batch size
  (`BatchSizeBase + (RoundNumber-1) * BatchSizeGrowthPerRound`), tells the
  spawner to emit exactly that many, → **Active**.
- **Active** — watches two things each tick:
  - `tally.ReachedBase` rising → subtract lives; at 0 → **Defeat**.
  - the batch is resolved (batch fully emitted **and** nothing in the
    lumen or tissue — wall pile is allowed to persist) → round clears:
    grant the lump sum, regen a life every `LifeRegenRounds` rounds,
    **despawn every fielded unit** (§2 — emitted cells die at round end;
    the progenitor towers stay), → **Building**.
- **Defeat** — everything frozen, HUD shows GAME OVER.

### 3. `PathogenSpawner` — batch gating

Stops free-running. `BeginBatch(count)` sets a target; `Tick` spawns on its
existing interval only while `emitted < target` and under
`maxLivePathogens`. `BatchComplete` = emitted the target **and** zero
pathogens in lumen or tissue. `LiveCount` exposed. Gut-interface and
cytokine ticking are unchanged (they no-op with nothing live).

### 4. `BoneMarrowManager` — placement cost

`Initialize` gains the wallet (nullable — a harness passes null and
placement stays free). `PlaceTower` spends `MacrophagePrice` /
`NeutrophilPrice` first, no-ops if unaffordable. The IMGUI picker shows
prices and greys out what you can't afford. New `ClearFieldedUnits()` for
the round boundary.

### 5. HUD

`HudOverlay` shows `ROUND N [phase] · ATP · Lives N/100`, batch progress
during a round, the buy-phase prompt + Start button during Building, and
GAME OVER on defeat. Marrow-slot picker shows prices.

### 6. `EconomyTuning` — every number in one place

Mutable statics, `ResetToDefaults()`, all placeholder mechanics-first:
`StartingAtp` 100, `RoundStartLumpSum` 80, `AtpPerKill` 3,
`MacrophagePrice` 40, `NeutrophilPrice` 15, `StartingLives` 100,
`LifeRegenRounds` 2 / `LifeRegenAmount` 1, `BatchSizeBase` 8 /
`BatchSizeGrowthPerRound` 3.

### 7. Not in scope

- **Emergency granulopoiesis** (§6c's acute breach punishment) — deferred.
- **Bone marrow slot expansion / capacity purchase** (§2a open) — slots
  stay at 5, count fixed.
- **Round batch *composition*** — class weights per round, boss rounds,
  a real difficulty curve. Placeholder linear size growth only.
- **Tower upgrades** with ATP — no upgrade system yet.
- **The DC shuttle / adaptive immunity** — the sprint after.
- Anything that makes round 1 non-inert beyond the buy decision (§2a's
  flagged risk) — noted, not addressed.

Everything from Sprints 1–6 keeps working: the invasion loop, the
firebreak, host states / debris / efferocytosis, the §4b stress-sense and
intracellular models, population caps, pooling.

## Stopping point (definition of done)

The Director can, in a build:

- [ ] Open into a **buy phase** with `StartingAtp`, place towers (each
      deducting its price; can't place when broke), and start round 1 with
      a keypress / button.
- [ ] Watch a round spawn a **finite batch**, run, and **clear** when the
      batch is resolved — then land back in a buy phase with the
      **lump sum** added and last round's units gone.
- [ ] See **ATP rise per kill** during a round.
- [ ] See **Lives drop on a breach**, regenerate slowly between rounds,
      and the run end at **0 → GAME OVER**.
- [ ] See the **round number climb** and the batch get bigger.
- [ ] Wall-pile pathogens **persist** across the buy phase into the next
      round (§6b).
- [ ] Everything from Sprints 1–6 still works.
- [ ] A new `EconomyVerification` harness covers the wallet, the round
      state machine, batch completion, the lump sum, per-kill income,
      life loss → defeat, life regen, placement cost, and round-boundary
      unit clearing. Combat / Lifecycle / Map / Tissue all still green.
- [ ] `GAME_DESIGN.md` §5b/§5d/§6c, `ENGINE_STATUS.md`, `INTERFACE.md`,
      `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md` reflect reality.

The question this sprint answers: **is the loop — buy, start, survive,
get paid, buy more — there and legible**, even with every number wrong?

## A process note

Same as Sprint 6: head session, inline, commit after each scope item with
a reasoning-heavy message, update `INTERFACE.md` and `TEAM_RETRO.md` as
signatures change and judgment calls are made — not in a final sweep.
