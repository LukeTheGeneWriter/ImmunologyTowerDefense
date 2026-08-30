# Sprint Plan — Sprint 14: the DC-pacing rework

## Recent sprints — closed 2026-08-29 / 30

- **Sprint 11** — placeholder buy-phase shop (`ShopLedger` / `ShopTuning`,
  per-tower `UpgradeTower`), the §5 knowledge ladder as data + a
  per-species HUD readout, and one real change: neighbour-accelerated
  regrowth (`TissueTuning.NeighbourRegrowthBonus`). `Sprint11Verification`
  26.
- **Sprint 12** — two Sprint 11 playtest fixes: cytokine sensing is ON by
  default with a real buyable sharpen (`ShopItem.CytokineSensingUpgrade`
  → `Chemotaxis.SensingUpgradeLevel` → `EffectiveSharpness`; `C` = debug
  off), and a first DC patrol movement fix (fine-grained lane-repulsion +
  a threat-axis band sweep; `BoardConfig.FineCrossIndex` / `FineAxisIndex`;
  `DcPatrolSweepBias`). `Sprint12Verification` 9.
- **Sprint 13** — the sprite / visual-identity pass: procedurally-drawn
  `SpriteShapes` replace the flat white quad for every entity (white +
  alpha-silhouette, so every per-instance tint still works); four
  host-state sprites with a viral/bacterial infection texture split; five
  distinct flash silhouettes + a concurrent-flash cap. No gameplay
  surface. **410 assertions total, 0 failed.**

## Direction for Sprint 14 (Director, 2026-08-30)

"The sprites look GREAT. I think we still need to work on DCs though —
they still don't oscillate noticeably, nor do they separate into
pseudo-lanes. Remember we want them to move back and forth between the
base and the lumen and we also want them to push away from each other on
the perpendicular axis."

Third pass at this note (Sprint 10 added the repulsion, Sprint 12 fixed
its granularity + added the sweep). This time the diagnosis is **not the
mechanic** — it's that the DC barely occupies the state the mechanic
lives in. Both the oscillation and the lane-repulsion run only in
`PatrolTissue`, and a dense round (16-pathogen batches, debris on every
dead cell) means a DC picks up cargo within ~2 ticks and then spends the
rest of its life in `TravelToNode` / `InNode` / `ReturnToTissue`, none of
which pace or repel.

## Scope — done

### 1. Collapse the shuttle from four states to two

`DendriticCellState` → `{ PatrolTissue, InNode }`. Delete `TravelToNode`,
`ReturnToTissue`, and their handlers (`TickTravel`, `TickReturn`,
`BiasedAxisStep` — the straight axis-frame dashes).

- **`TickPatrol` runs the DC's whole tissue life.** Set `patrolHeading`
  before stepping: `-1` (toward the base) while `HasCargo`, otherwise
  oscillate — `+1` until `TissueLumenEdgeAxisIndex`, `-1` until
  `TissueBaseEdgeAxisIndex`, flipping at each edge. Step
  `DcFineTilesPerTick` × `RepelledPatrolStep` (unchanged: fine-grained
  cross-axis softmax repulsion vs. the other non-`InNode` DCs + the
  `DcPatrolSweepBias` sweep term).
- Reaching the `Base` band with cargo → `EnterNode`. Empty on a `Dead`
  cell with a debris antigen → sample (as before); no state change, the
  heading just flips toward the base.
- **`LeaveNode`** (was the `ReturnToTissue` transition): `node.Release`,
  reposition to a random lane on the tissue base edge, `HasCargo = false`,
  `patrolHeading = 1`, `State = PatrolTissue`.

### 2. Tuning so the pace reads in a ~30 s round

`AdaptiveTuning`: `DcFineTilesPerTick` 2 → 3, `DcPatrolSweepBias`
1.0 → 1.8. Remove `DcAxisWalkBiasSharpness` (it only fed the deleted
dashes).

### 3. Harness

`AdaptiveVerification` stays at 40 — the mechanic got simpler, not
bigger. `RunShuttleEndToEnd`'s two assertions move from `ReturnToTissue`
to `PatrolTissue && !HasCargo` (same meaning). `DriveOneShuttle` drives
until it has observed a node visit *and* the return to an empty patrol,
and seeds debris two tiles off the base edge so the DC has to pace to
reach it. `RunDcLaneSpread` / `RunDcPatrolSweep` unchanged.

## Not in scope

- **Cargo capacity > 1 / a spatial node approach / a straighter loaded
  path** — all noted in `BACKLOG.md` (Sprint 14 section) as balance
  levers, not needed for legibility.
- **Knowledge-ladder mechanics, real shop effects, a real buy UI,
  economy retune** — the standing big-ticket items, untouched.
- **Any rendering change.** The sprites shipped last sprint.

## Stopping point (definition of done) — status 2026-08-30

- [x] `DendriticCellState` is two values; `TravelToNode` / `ReturnToTissue`
      and their handlers deleted.
- [x] A DC paces the tissue band base↔lumen its entire tissue life, with
      lane-repulsion active the whole time; a loaded DC biases to the
      base and enters the node on reaching the `Base` band.
- [x] `DcFineTilesPerTick` 3, `DcPatrolSweepBias` 1.8,
      `DcAxisWalkBiasSharpness` removed (field + `ResetToDefaults`).
- [x] All ten harnesses green — **Adaptive 40, 410 total, 0 failed**.
      Repulsion A/B 16 co-lane vs. 167, spread 15.4 vs. 4.1; swept axis
      span 6..18 vs. 10..12 for a plain walk.
- [x] Clean Windows build (0 errors), headless launch 0 exceptions.
- [x] `GAME_DESIGN.md` §5a, `ENGINE_STATUS.md`, `INTERFACE.md`,
      `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md`, this file updated.
- [ ] **How the pacing looks in motion.** No headless coverage of
      `Update()`; the handoff is the build. The Director's playtest.

**Handed to the Director for playtest.**

## Process note

Bugfix sprint from a direct playtest note, done inline by the head
session — no dispatched agent. The lesson (`TEAM_RETRO.md` Sprint 14):
when a harness-green mechanic keeps reading as absent, check how much
wall-clock the entity spends in the state the mechanic lives in before
strengthening the mechanic again.
