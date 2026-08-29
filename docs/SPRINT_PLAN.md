# Sprint Plan — Sprint 11 (+ Sprint 12 follow-up); next: Sprint 13 (sprite pass)

## Sprint 12 — closed 2026-08-29

Two Sprint 11 playtest fixes. **(1)** Cytokine sensing is ON by default
(the `C` key is now a debug OFF-toggle); the *improvement* is a real
buyable shop item, `ShopItem.CytokineSensingUpgrade` →
`Chemotaxis.SensingUpgradeLevel` → `EffectiveSharpness`. **(2)** The DC
patrol movement fix — Sprint 10's lane-repulsion compared coarse indices
(fired ~1/7 of steps, no back-and-forth); now fine-grained every step
plus a threat-axis band sweep (`DcPatrolSweepBias`; `DcLaneRepelStrength`
1.4 → 0.8). New `Sprint12Verification` (9) + `AdaptiveVerification` +3
(→ 40). 410 total, 0 failed. Sprite/art pass = Sprint 13 (design agent
dispatched 2026-08-29; `docs/SPRITE_DESIGN.md`).

## Sprint 10 — closed 2026-08-29

DC patrol lane-repulsion: patrolling dendritic cells bias their random
walk away from each other **along the lane axis only** (the base↔lumen
threat axis stays unbiased), so they spread evenly across the lanes and
sweep back and forth instead of clumping. `AdaptiveTuning.DcLaneRepelStrength`
1.4 / `DcLaneRepelAxisRange` 12. `AdaptiveVerification` 34 → 37. 372
total, 0 failed.

## Direction for Sprint 11 (Director, 2026-08-29)

Two things: a **shop with placeholder buy options**, and the **§5
knowledge-ladder roster** made concrete.

### 1. Shop — placeholder buy options (no mechanical effect yet)

The Director's framing: "These are placeholders for now, no actual
changes during an upgrade." Build the shop *framework* — purchases cost
ATP, register as owned / a level, and show in the UI — and record the
intended design for each so the mechanics sprint has a spec. Categories:

- **Lumen barrier** — the §6b mucus-turnover upgrade (raise the shed
  rate; flush barrier residents back to the lumen). Repeatable level.
- **Host-cell upgrades** — the Director's design (all placeholder):
  - **Pattern-recognition receptor → immunogenic apoptosis.** Upgrade a
    cell to sense a pathogen signature (example: **dsRNA**). An infected
    cell then has a ~20% chance to **self-destruct** when infected by the
    matching class (an RNA virus for dsRNA), dying cleanly and releasing
    a **strong "eat this debris" cytokine that pulls in dendritic cells
    specifically** — a *third* signal, distinct from the §7/§9
    recruitment cytokine and the §5c co-localisation signal. This is
    intrinsic antiviral defence + immunogenic cell death, and it is the
    host cell actively feeding the adaptive loop.
  - **Reduced viral entry** — lower the per-tick chance a virus gets
    inside a healthy cell.
  - **Bacterial-damage resistance** — a large bacterium grazing the cell
    does less host-cell damage per step.
  - **Crypts** — placeable stem-cell niches; tissue near a crypt regrows
    faster (§6's crypt-based recovery). Repeatable count.
- **Progenitor upgrades** — per-tower (§6d: "an upgrade is a write to one
  tower's field"). Clicking a *placed* marrow slot opens its upgrade
  options (kill count, degranulation collateral, …), each a placeholder
  that spends ATP and bumps that tower's upgrade level.

### 2. Knowledge ladder — define + show, no mechanics

The Director's roster (his mechanic notes in parentheses), mapped onto
§5's existing threshold proposals:

| % | Capability | Mechanic (for the later sprint) |
|---|---|---|
| ~10 | **Cytotoxic T cells** | precise kill of an infected cell — no collateral, no necrotic DAMP (the quiet version of §4b's stress-sense kill) |
| ~20 | **Neutralizing antibodies** | reduced adhesion probability for a known species |
| ~30 | **Memory T cells** | on re-encountering a known species, a quick **burst of CTLs** spawns to hunt it |
| ~45 | **Fc receptor** | antibodies affix to innate cells — opsonisation / antibody-guided targeting |
| ~60 | **Complement activation** | antibody destroys the target's membrane directly, no immune cell needed — passive damage to a coated pathogen |
| ~70 | **Secretory IgA** | antibody exported into the lumen, acting on a species **before adhesion is possible** |

This sprint: a `KnowledgeLadder` data table + a per-species HUD readout of
which rungs are unlocked at the current KNOWLEDGE %. **Crossing a
threshold changes the display and nothing else** — every capability's
real mechanic is a later sprint (each is a substantial piece: CTL is a
new unit, antibodies are a new entity, IgA is a new lumen mechanic).

### 3. One real change: neighbour-accelerated regrowth

Not a placeholder — the Director asked to "tune repairing so that healthy
cells can fill in neighbouring empty space more quickly." An `Empty`
host-ground cell's regrowth time scales down with its count of `Healthy`
von Neumann neighbours, so tissue heals inward from its intact edges
rather than every dead cell regrowing on an independent clock. New
`TissueTuning.NeighbourRegrowthBonus` (0 restores the old behaviour).

## Scope

### A. `ImmunologyTD.Economy.ShopLedger` + `ShopTuning`

- **`ShopItem` enum** — `BarrierMucusTurnover`, `HostDsRnaSensor`,
  `HostReducedViralEntry`, `HostBacterialResistance`, `Crypt`.
- **`ShopLedger`** — plain reference type, per run. `int LevelOf(ShopItem)`,
  `bool CanBuy(ShopItem, AtpWallet)`, `bool TryBuy(ShopItem, AtpWallet)`
  (spends `ShopTuning.PriceFor(item, currentLevel)` and increments the
  level), `void Reset()`. **No side effect beyond the ledger + the
  wallet.**
- **`ShopTuning`** — mutable statics, `ResetToDefaults()`. A base price
  per item and a per-level multiplier (so repeat buys cost more). All
  placeholder.

### B. Per-tower progenitor upgrades (`BoneMarrowManager`)

- `Slot` gains `int UpgradeLevel` (and optionally a small `int[]` per
  upgrade kind — keep it one level for now).
- Clicking a **placed** slot (currently a no-op) opens an IMGUI upgrade
  panel: one "Upgrade → Lv N+1" button, priced from `ShopTuning`,
  greyed-out when unaffordable. `UpgradeTower(index)` spends ATP and
  bumps the level. **No mechanical effect** — the `UnitLifecycleTuning`
  is untouched (the §6d wiring for real upgrades already exists; this
  just doesn't call it yet).
- `int GetUpgradeLevel(int)` for the HUD / harness.

### C. `ImmunologyTD.Adaptive.KnowledgeLadder` + HUD

- **`KnowledgeCapability` enum** — the six above.
- **`KnowledgeLadder`** (static) — `struct Rung { KnowledgeCapability
  Capability; float ThresholdPercent; string ShortName; }`, an ordered
  `Rung[] Rungs`, `bool IsUnlocked(KnowledgeCapability, float pct)`, and
  `IEnumerable<Rung>` iteration for the HUD.
- **`HudOverlay`** — the KNOWLEDGE line becomes a small block: per
  species, `Name pct%` then the six rungs as `[x] 10 CTL  [ ] 20 NeutAb
  …`. Still drives nothing.

### D. Neighbour-accelerated regrowth (`TissueGrid` / `TissueTuning`)

- `TissueTuning.NeighbourRegrowthBonus` (new, default `0.5`).
- `TissueGrid.Tick`'s `Empty → Healthy` branch: `effectiveRegen =
  HostRegenerationSeconds / (1 + NeighbourRegrowthBonus * healthyNeighbourCount)`,
  clamped so 4 healthy neighbours ≈ 3× faster. Isolated `Empty` ground
  regrows at the old rate.

### E. Shop panel (`HudOverlay` or a new `ShopPanel` MonoBehaviour)

- Drawn only during `RoundPhase.Building` (the frozen buy phase). IMGUI
  panel, left side, clear of the debug panel. One row per `ShopItem`:
  name, "Lv N", price, a Buy button greyed-out when broke. Wired to
  `ShopLedger.TryBuy`.
- The Director should be able to open the game, see the shop, click
  every option, and watch ATP go down and levels go up.

### F. Verification + docs

- **`ShopVerification`** (new, or fold into `EconomyVerification`):
  `ShopLedger` spend / refuse / level-up and price scaling; per-tower
  `UpgradeTower` spend + level; `KnowledgeLadder.IsUnlocked` at boundary
  percentages (9.9 vs 10.0, etc.) and rung ordering; neighbour-regrowth
  (an `Empty` cell ringed by `Healthy` regrows measurably faster than an
  isolated one).
- Re-run all eight prior harnesses green.
- `GAME_DESIGN.md` §5 (roster promoted to Director-confirmed, mechanic
  notes) + a new host-cell-upgrades subsection (the dsRNA-sensor design
  in full, the DC-attractant third signal, reduced viral entry, bacterial
  resistance, crypts); `ENGINE_STATUS.md`, `INTERFACE.md`, `CHANGELOG.md`,
  `BACKLOG.md`, `TEAM_RETRO.md`. Clean Windows build, 0 exceptions.

## Not in scope

- **Every knowledge-ladder capability's real mechanic** — CTL as a unit,
  antibody entities, IgA in the lumen, complement damage, Fc/opsonisation,
  the memory-CTL burst. All defined this sprint, built later.
- **Every shop purchase's real effect** — mucus turnover, the host-cell
  receptor upgrades, crypts, progenitor upgrades. Framework only.
- **The DC-attractant third cytokine field** — described in the design,
  not built.
- **A real economy pass** (prices are placeholder; the persistent-army
  ATP question from Sprint 9 is still open).
- Anything from Sprints 1–10 changing behaviour, except neighbour-regrowth
  (scope D).

## Stopping point (definition of done) — status 2026-08-29

`[~]` = code done + harness-verified. `[x]` = verified from command output.

- [~] A **shop** is drawn in the frozen buy phase with buy options for
      the barrier, host-cell upgrades, and crypts; each priced, greying
      out when broke, buying spends ATP and raises a level and **nothing
      about the simulation changes**. (`RunShopLedger`; the OnGUI panel
      itself is the build launch.)
- [~] Clicking a **placed progenitor tower** offers an upgrade that
      spends ATP and bumps the tower's level, `UnitLifecycleTuning`
      untouched. (`RunProgenitorUpgrade`.)
- [~] The HUD KNOWLEDGE block shows the **six-rung ladder per species**,
      rungs ticking on as the % crosses their thresholds, driving
      nothing. (`RunKnowledgeLadder`; the HUD lines are the build launch.)
- [~] Tissue **fills in from its healthy edges faster** than isolated
      dead cells regrow. (`RunNeighbourRegrowth`: surrounded 6.8s vs
      isolated 20.0s.)
- [x] Everything from Sprints 1–10 still works — eight prior harnesses
      re-run green.
- [x] `Sprint11Verification` — **26 passed, 0 failed**. 398 total across
      nine harnesses.
- [x] `GAME_DESIGN.md` §5 + §1d, `INTERFACE.md`, `ENGINE_STATUS.md`,
      `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md` updated. Clean Windows
      build (93,353,280 bytes, 0 errors), 0 exceptions on launch.

**Handed to the Director for playtest.**

## Candidate next sprints (Director's call)

- **§5 knowledge-ladder mechanics** — start wiring the rungs the ladder
  now displays. Cheapest first: neutralizing antibodies (~20%, an
  adhesion multiplier) and complement (~60%, a passive damage tick).
  Cytotoxic T cells (~10%) is a new unit and the headline payoff.
- **Shop mechanics** — make the Sprint 11 placeholders real, starting
  with the progenitor upgrades (the §6d wiring exists, `UpgradeTower`
  just needs to write a field) and the mucus-turnover barrier upgrade.
- **The dsRNA-sensor host-cell upgrade + its third cytokine field**
  (`GAME_DESIGN.md` §1d) — the DC-recruiting "eat this debris" signal.
- **Sprite / art pass** — `BACKLOG.md` "Sprite / art pass" +
  `UI_STYLE_GUIDE.md`. Everything on screen is still a flat quad; this is
  the first likely job for a dispatched Design agent, probably paired
  with a real buy UI.
- **Economy retune** against the persistent army (open since Sprint 9).

## Process note

Head session, inline, commit after each scope item with a reasoning-heavy
message; update `INTERFACE.md` / `TEAM_RETRO.md` as signatures change and
calls are made, not in a final sweep.
