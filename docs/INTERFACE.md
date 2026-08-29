# Interface Contract (Engine ↔ UI)

Status: written Sprint 1; grown each sprint since. Sprint 2 added bone
marrow placement, the lymph-node placeholder, pathogen classes/combat/viral
spread. Sprint 4 rewrote spatial geometry (Map 01 bands, the axis frame,
`GutInterface`) — see the "Map 01 geometry" / "Gut interface" sections.
**Sprint 5 (2026-08-28)** rewrote `TissueGrid` into two layers (host +
occupant) with `HostState`, debris, efferocytosis, and class-specific
advance — see "Occupancy state" and "Sprint 5 changes". **Sprint 6
(2026-08-28)** made an established intracellular infection unreachable by
ordinary innate damage and added the contact stress-sense roll, the real
intracellular-bacterium model (replicate → brood burst), and virus budding
+ burn-out — see "Sprint 6 changes". **Sprint 7 (2026-08-28)** added the
ATP economy (`AtpWallet`, prices, per-kill and lump-sum income) and the
round loop (`RoundController`: wave batch + buy phase, the 100-life pool,
the lose condition) — see "Sprint 7 changes". Everything below reflects
code that actually exists in `game/Assets/Scripts/` as of this sprint — it
is not aspirational. There is no UI session/agent yet to
consume this contract, so treat this as the engine side declaring its
shapes ahead of need. Update this file whenever any of the below changes;
per `WORKFLOW.md` that's a cross-team event even though "cross-team" is
currently just "future Design agent."

## Coordinate types (`ImmunologyTD.Grid`)

Two resolutions, matching `GAME_DESIGN.md` section 7.

- **`CoarseCoord`** — `{ int Column, int Row }`. One slot = one host cell
  or one adhered pathogen. `Column` runs along the lumen (0 = entry side).
  `Row` is a coarse depth band, `0..4` (`BoardConfig.Rows == 5`). This
  sprint's `Row` is *not* the full four-compartment/depth-5 model in
  `GAME_DESIGN.md` section 1/section 2 — it's a coarse row within the
  tissue compartment only. Reconciling the two is not done yet; flagged as
  a real open question below.
- **`FineCoord`** — `{ int Column, int Row }`, in **global** fine-tile
  units, not local to a coarse cell. `globalFineColumn = coarseColumn *
  FineSubdivision + localColumn` (same for row). This is the layer units
  and pathogens actually walk on. `FineCoord.ToCoarse(subdivision)`
  converts down; `FineCoord.VonNeumannOffsets` is the canonical
  four-neighbour offset set (movement is explicitly not eight-directional
  — see `GAME_DESIGN.md` section 7).
- **`BoardConfig`** (MonoBehaviour, lives on the single bootstrap
  GameObject) — owns `Columns` (Inspector-configurable, 24–40, default 30),
  `Rows` (`const 5`), `FineSubdivision` (`const 7`), `FineTileWorldSize`
  (`const 0.16f`), `TickIntervalSeconds` (`const 0.12f`, shared by units
  and pathogens). Provides `FineToWorld`, `CoarseToWorldCenter`,
  `InFineBounds`, `InCoarseBounds`.

## Occupancy state (`ImmunologyTD.Grid.TissueGrid`)

Plain C# class (not a MonoBehaviour), constructed by `GameBootstrap` and
passed by reference to everything that needs it (units, pathogens, the
spawner, the board renderer).

**Sprint 5 rewrote this into two independent layers per coarse position**
(`GAME_DESIGN.md` §1c). Sprints 1–4 held exactly one `PathogenAgent` per
slot and treated "bare tissue" as "no pathogen"; that could not express a
bacterium standing on a slot that still holds a living host cell, which is
the co-occurrence §1c argues is the whole reason for two layers rather than
one enum.

### Layer 1 — the host cell

`enum HostState { Empty, Healthy, Infected, Dead }`. The tissue band seeds
full of `Healthy`; every non-tissue cell is permanently `Empty` and every
host mutator no-ops there (`IsHostGround(CoarseCoord)` gates them).

Reads:
- `HostState GetHostState(CoarseCoord)`
- `float GetHostHealth(CoarseCoord)` — 0..`TissueTuning.HostCellMaxHealth`
- `float GetDebrisAmount(CoarseCoord)` — 0..`FullDebris` (`const 1f`); non-zero exactly when `Dead`
- `PathogenAgent GetIntracellularAt(CoarseCoord)` — the pathogen inside an `Infected` cell, else null
- `bool IsHealthyHost(CoarseCoord)` — `GetHostState == Healthy`. §1c's whole viral rule as one predicate: a virus may only spread into a `Healthy` neighbour.
- `bool CanRegrow(CoarseCoord)` — `IsHostGround && GetHostState == Empty`. Debris blocks regrowth because a `Dead` cell is not `Empty`.
- `int HealthyCount` / `int InfectedCount` / `int DeadCount` — O(1) counters, kept coherent across every transition.

Writes (every host-cell death funnels through `KillHostCell`, so "killing a
cell leaves debris" cannot be true on one path and false on another):
- `bool TryInfect(CoarseCoord, PathogenAgent, float startTime)` — `Healthy → Infected`, records the pathogen as living *inside* the cell (not replacing it). Fails on anything not an uninfected healthy host. `startTime` is what the cytokine ramp measures from.
- `bool KillHostCell(CoarseCoord)` — any state → `Dead`, full debris, intracellular ref dropped. **Notifies the cell's intracellular resident via `PathogenAgent.OnHostCellDestroyed` after detaching it** (so the resident's own clear path finds an already-dead cell and stops). Idempotent on an already-`Dead` cell.
- `bool DamageHostCell(CoarseCoord, float amount)` — direct damage to the cell (a large bacterium grazing, a neutrophil degranulation burst). Reaching zero calls `KillHostCell`. Returns true if this call killed it.
- `bool ReleaseIntracellular(CoarseCoord)` — `Infected → Healthy`, drops the resident link, **does not kill the cell**. Two callers: (a) `PathogenAgent.StepIntracellularBacterium` calls it immediately before `KillHostCell` when a bacterium lyses out, so `KillHostCell` has no resident to notify and the bacterium survives; (b) reserved for adaptive immunity's precise MHC-I killing (§4a, ~10% knowledge), not yet wired.
- `bool ClearDebris(CoarseCoord, float amount, float currentTime)` — efferocytosis. Subtracts `amount`; when the pile hits zero the position becomes `Empty` and its regrowth clock starts. Returns true if this call finished the pile.
- `void SeedHostState(CoarseCoord, HostState, float currentTime)` — test/bootstrap hook (lay out a firebreak fixture without killing cells through combat). Not called by production.

### Layer 2 — the extracellular occupant

Independent of the host layer. Holds a large bacterium, an intracellular
bacterium currently *outside* a cell, or a free virus particle between
hosts. Immune cells are tracked on the fine lattice and are **not** here.

- `bool IsOccupantFree(CoarseCoord)`
- `bool TryClaimOccupant(CoarseCoord, PathogenAgent, float secretionStartTime)` — false if already occupied
- `void ReleaseOccupant(CoarseCoord)`
- `PathogenAgent GetOccupantAt(CoarseCoord)` — null if free
- `PathogenAgent GetAttackableAt(CoarseCoord)` — what a `SearchUnit` hits at this slot: the occupant, or the intracellular resident if the host is `Infected` (innate clearing of an infection is destructive, §4a). Replaces Sprints 1–4's `GetPathogenAt`.
- `int OccupantCount`
- `int TissuePathogenCount => OccupantCount + InfectedCount` — HUD copy / cheap sanity read.

### Host-layer simulation — `void Tick(float deltaTime, float currentTime)`

Debris self-dissipation and host-cell regrowth, the two host-layer
processes that run on their own clock — **not** driven by the pathogen
spawner (the host layer keeps healing whether or not anything is invading).
Wired in `GameBootstrap` via a three-line `TissueDriver` MonoBehaviour that
forwards `Time.deltaTime`/`Time.time`; every harness forwards a simulated
clock. Decay is integrated over the accumulated delta, so
`TissueTuning.SweepIntervalSeconds` is purely a cost knob — a harness may
advance in coarse slices without the numbers drifting.

- Debris on a `Dead` cell loses `elapsed / DebrisSelfDissipationSeconds` per sweep; at zero the cell becomes `Empty`.
- An `Empty` host-ground cell regrows to `Healthy` `HostRegenerationSeconds` after it became empty.

### `TissueTuning` (`ImmunologyTD.Grid`, new Sprint 5)

Mutable statics with `ResetToDefaults()`, same pattern as `InvasionTuning`.
All unvalidated defaults (mechanics-first):
`HostCellMaxHealth` 10, `HostRegenerationSeconds` 20,
`DebrisSelfDissipationSeconds` 60, `SweepIntervalSeconds` 0.25.

### Legacy names removed

`IsSlotFree`, `TryAdhere`, `ReleaseSlot`, `GetPathogenAt`, `AdheredCoords`,
`AdheredCount` are **gone** (Sprint 4 already retired the adhesion model
they belonged to; Sprint 5 removed the shims). `AdheredCount`'s role is now
split across `HealthyCount`/`InfectedCount`/`DeadCount`/`OccupantCount`.

### Infected-cell / continuous secretion (added in the Sprint 1 closing task, still current)

The first playtest found the cytokine-sensing toggle produced no
perceptible difference — root cause: the field's sources were just "wherever
a pathogen happens to be," a flat one-shot value, and the resulting bias
was real but too gradual to notice in a short session (see
`SPRINT_PLAN.md`'s "Closing task" section for the Director's verdict and
the diagnosis). Fix: `TryAdhere` now also starts a per-slot infection
timer, and the slot's *host cell* (still not a separate class — a
deliberate scope call, see below) is what secretes, ramping continuously
rather than emitting a fixed value from the moment of adhesion:

- `public const float BaseSecretionStrength = 6f` — strength the instant a
  cell becomes infected.
- `public const float MaxSecretionStrength = 32f` — strength once fully
  ramped. Also the normalization reference `BoardRenderer` divides by for
  the heatmap tint (see Rendering below).
- `public const float InfectionRampSeconds = 20f` — time to go from Base
  to Max (linear).
- `float GetSecretionStrength(CoarseCoord, float currentTime)` — current
  strength, 0 if the slot isn't infected.
- `IEnumerable<(CoarseCoord Coord, float Strength) InfectedSources(float
  currentTime)` — every infected slot paired with its current strength.
  This is what `CytokineField.Recompute` now consumes (see below), not
  `AdheredCoords`.

**Why this lives on `TissueGrid` rather than a separate `InfectedCell`
class:** the closing task's brief explicitly left this as a judgment call
("doesn't need to be a fully separate class if that's overkill"). A
per-slot timer array plus a couple of methods gave the conceptually
distinct "infection" lifecycle (its own start time, its own ramp, could in
principle exist without a live `PathogenAgent` reference later) without a
new MonoBehaviour/class hierarchy — judged proportionate to a one-sprint
fix. If host cell health/fibrosis lands later and `TissueGrid` grows a
real per-slot occupant struct (see above), infection state is a natural
field to fold into that struct at the same time.

**Why `currentTime` is a parameter, not read internally via
`UnityEngine.Time`:** so `TissueGrid` (and `CytokineField`, `Chemotaxis`)
stay plain, headlessly-testable C# — this is what let
`Assets/Editor/CytokineVerification.cs` (see below) drive a full simulated
session and print real before/after numbers without needing play mode or
any `GameObject`s.

## Cytokine gradient (`ImmunologyTD.Grid.CytokineField`)

- `void Recompute(IEnumerable<(CoarseCoord Coord, float Strength)>
  sources)` — rebuilds the coarse field from scratch as an
  inverse-Manhattan-distance falloff from every source, each weighted by
  its own strength (`strength / (1 + distance)`). Signature changed this
  sprint from `IEnumerable<CoarseCoord>` (flat strength) to carry each
  source's `TissueGrid.GetSecretionStrength` value. Called by
  `PathogenSpawner` on a timer (every 0.4s) rather than only when
  `TissueGrid.AdheredCount` changes — the closing task's continuous
  secretion means the field keeps changing even with a static set of
  infected slots, so the old change-detection optimization no longer
  covers what needs to be recomputed.
- `float SampleFine(FineCoord)` — bilinear interpolation of the coarse
  field at a fine-grid position. This is what `Chemotaxis.ChooseNextStep`
  (see Units below) samples when cytokine sensing is on.
- `float CoarseValueAt(CoarseCoord)` — raw, non-interpolated coarse-cell
  value. Added this sprint for `BoardRenderer`'s heatmap tint, which paints
  per coarse cell rather than per fine tile.

**Documented simplification:** `GAME_DESIGN.md` section 7's implementation
note calls for diffusion on the coarse grid, interpolated down to the fine
lattice, specifically to avoid diffusing across thousands of fine tiles per
tick. This is honored, but the coarse field itself is *not* a literal
diffusion PDE stepped every tick — it's a distance-falloff field recomputed
from the current infected-source set on a timer. For a mostly-static set of
source LOCATIONS (pathogens don't move once adhered) this reads the same
as a settled diffusion field and is far cheaper; the per-source *strength*
still changes continuously as infections ramp, which is exactly why it's
now recomputed periodically instead of only on adhesion events. If
pathogens ever become mobile after adhering (e.g. burrowing deeper over
time), this shortcut should be revisited — a falloff field recomputed from
a moving source set would still work, just less smoothly during the
transition than a true diffused field would.

## Units (`ImmunologyTD.Units`)

- **`UnitKind`** — `enum { Macrophage, Neutrophil }`.
- **`UnitProfile`** — plain serializable class (not a ScriptableObject):
  `{ UnitKind Kind, string DisplayName, int FineTilesPerTick, int
  FootprintFineTiles, Color Color }`. One instance per type, held on
  `GameBootstrap` and shared by reference across every spawned instance of
  that type (so tuning one Inspector value changes all units of that kind
  at once). `FineTilesPerTick` is the required per-cell step length from
  `GAME_DESIGN.md` section 7 — deliberately per-type, not a shared
  constant. Sprint 1 defaults: Macrophage `1` tile/tick, 5×5 footprint;
  Neutrophil `3` tiles/tick, 3×3 footprint.
- **`SearchUnit`** (MonoBehaviour) — `Initialize(BoardConfig, TissueGrid,
  CytokineField, UnitProfile, FineCoord start)`. Exposes `FineCoord
  Current`. Ticks on `BoardConfig.TickIntervalSeconds`; each tick takes
  `profile.FineTilesPerTick` individual steps, each delegated to
  `Chemotaxis.ChooseNextStep` (see below). Visual position is a
  `Vector3.Lerp` between tick-start and tick-end world position, so grid
  logic doesn't produce steppy visuals (per `GAME_DESIGN.md` section 7's
  "sprites tween between coordinates").
- **`Chemotaxis`** (static class, `ImmunologyTD.Units`, added in the
  Sprint 1 closing task) — `FineCoord ChooseNextStep(FineCoord current,
  BoardConfig, CytokineField, bool cytokineEnabled, FineCoord[]
  candidateBuffer, float[] weightBuffer)`. The per-step neighbour-choice
  algorithm, pulled out of `SearchUnit.StepOnce` into a static,
  side-effect-free function specifically so a headless verification
  harness could call the *actual* production algorithm without needing
  `GameObject`s or play mode (see `Assets/Editor/CytokineVerification.cs`
  below). Buffers are caller-owned (length >= 4, reused every call) so
  this allocates nothing per step.

  **Rung-2 weighting changed this sprint.** The original approach weighted
  each candidate linearly by its *absolute* field value
  (`1 + k * value`). That produced a real but imperceptible drift — First
  playtest verdict, confirmed by code review, was "no perceptible
  difference" (see `SPRINT_PLAN.md`'s closing task) — because the field is
  bilinear-interpolated smoothly across each coarse cell, so any four
  adjacent fine tiles have nearly identical absolute values regardless of
  `k`. The fix: a softmax over each candidate's value *relative to the
  best candidate* among the four, via `public static float
  GradientSharpness` (default `4f`, in `Chemotaxis.cs`) —
  `weight_i = exp(GradientSharpness * (value_i - maxValue))`. Only
  sensitive to the (often small) local difference that actually exists,
  amplified by `GradientSharpness`. Net effect, matching `GAME_DESIGN.md`
  section 7's "preserves the wandering but adds visible drift": near a
  steep local gradient (a few coarse cells from an infected site) this is
  close to deterministic "beelining"; far from any source, where the field
  is nearly flat, it's close to a uniform walk. `GradientSharpness` is a
  mutable static field, not a `const`, specifically so
  `CytokineVerification`'s sweep methods can override it at runtime while
  tuning — see that file for the sweep results that led to `4f`.
- **`CytokineToggle`** (MonoBehaviour, one instance on the HUD object) —
  `static bool Enabled { get; }`. Flips on `KeyCode.C` (`Input.GetKeyDown`,
  legacy Input Manager — this project has `activeInputHandler: 0`, no new
  Input System package installed). This is the sprint's debug toggle; it
  is deliberately runtime-toggleable in a standalone build, not just an
  Inspector checkbox, since the Director watches a build, not the Editor.

**Movement/collision simplification, stated explicitly because it's a
judgment call and not obviously implied by the design doc:** units
co-occupy fine tiles with adhered pathogens exactly as they co-occupy with
host cells — a pathogen still does **not** block unit movement.
"Collision" is detected as the unit's fine tile falling **within
`ContactRadiusFineTiles` (Chebyshev, default 2) of the pathogen's own fine
tile** (`SearchUnit.CheckContact`, called once per tick after the unit's
fine-tiles-per-tick steps resolve).

**Sprint 3 change: contact is a proximity test, not a coarse-slot test.**
Through Sprint 2 a unit damaged a pathogen from anywhere in its 7×7 coarse
slot, so every unit in that slot landed a hit every tick (an accidental
stacking bonus — this was open question 3, now resolved). `CheckContact`
now calls `pathogen.ReceiveDamage(PathogenAgent.ContactDamagePerHit, this)`
only within the radius, and passes **itself** as the attacker so the kill
can be attributed (see Pathogens below).

Chebyshev rather than Manhattan because it matches the square footprints
these units render as; the Manhattan diamond covers half the tiles and
would have halved contact frequency again. **It is deliberately a radius,
not an exact-tile test** — with 49 fine tiles per coarse slot, requiring
exact coincidence would mean a random-walking unit almost never connects.
Anyone tempted to "tighten" this further should read `SPRINT_PLAN.md` item
7 first. Measured cost of the change: contact frequency dropped to ~50% of
the Sprint 2 rate (macrophage 50.0%, neutrophil 49.2% — see
`ENGINE_STATUS.md`), i.e. clearing is about half as fast per unit.

The 0.25s color-flash-toward-a-highlight-color visual is still there
(inside `ReceiveDamage`), and reaching zero health still clears the slot
and returns the pathogen to its pool.

## Bone marrow / placement (`ImmunologyTD.Units`, new this sprint)

Replaces Sprint 1's random-fine-coord debug spawn (`GameBootstrap.SpawnUnits`,
removed). Implements `GAME_DESIGN.md` section 2a's placement model and
rung-1 entry table.

- **`BoneMarrowSlot`** (MonoBehaviour) — `Init(BoneMarrowManager, int
  index)`. One clickable slot; detection is Unity's legacy `OnMouseDown`
  message via a `BoxCollider2D` (this project has no uGUI/EventSystem —
  `OnMouseDown` needs nothing beyond that collider and a `MainCamera`-tagged
  camera, both of which `GameBootstrap` already provides). Forwards clicks
  to `BoneMarrowManager.OnSlotClicked(index)`.
- **`BoneMarrowManager`** (MonoBehaviour) —
  `Initialize(BoardConfig, TissueGrid, CytokineField, UnitProfile
  macrophageProfile, PrefabPool macrophagePool, UnitProfile
  neutrophilProfile, PrefabPool neutrophilPool, Vector3[] slotWorldPositions,
  float slotWorldSize)` builds `slotWorldPositions.Length` slots as child
  `GameObject`s (each with its own `SpriteRenderer` + `BoxCollider2D` +
  `BoneMarrowSlot`). Per-slot state is `BoneMarrowSlotState { Empty,
  Placed }`, `Kind` (`UnitKind`), and an emission timer.
  - `OnSlotClicked(int index)` — opens a pending-choice state (drawn by
    `OnGUI` as a two-button "Macrophage"/"Neutrophil" panel anchored to
    the slot's world position) for an `Empty` slot; no-ops otherwise.
  - `PlaceTower(int index, UnitKind kind)` — public (not just reachable via
    the IMGUI buttons) so a headless verification harness can call the
    exact same placement path. No ATP cost (`SPRINT_PLAN.md`: "placement
    is free this sprint"). No-ops on an already-`Placed` slot.
  - `Tick(float deltaTime)` — **not** an implicit `Update()` reading
    `UnityEngine.Time`, matching `TissueGrid`/`CytokineField`/`Chemotaxis`'s
    established pattern so a headless harness can drive the real emission
    logic with simulated time (`Update()` just calls
    `Tick(Time.deltaTime)`). Every `Placed` slot accumulates
    `EmissionTimer`; at `EmissionIntervalSeconds` (`const float = 4f`, a
    judgment call — see `docs/TEAM_RETRO.md`) it emits one unit of that
    slot's kind and resets.
  - **Emission entry point**: `new FineCoord(Random.Range(0,
    board.FineColumns), board.FineRows - 1)` — uniformly random column,
    fixed at the **deepest fine row** (`board.FineRows - 1`). Per
    `CoarseCoord`'s existing convention ("Row 0 = shallowest, nearest the
    lumen"), the deepest row is the blood-adjacent edge — this is
    `GAME_DESIGN.md` section 2a's rung-1 "cells extravasate at random
    points along the vessel," with "the vessel" mapped onto the tissue
    board's existing depth axis rather than a new one (see open question 1
    below, still unresolved — this sprint didn't need to resolve the
    coarse-row-vs-compartment-depth reconciliation, just pick a consistent
    edge, which it did).
  - `EmittedCount`, `LastEmittedStart`, `LastEmittedKind`,
    `GetSlotState(int)`, `GetSlotKind(int)` — public read-only surface
    added specifically for `Assets/Editor/CombatVerification.cs` to assert
    against, same reasoning as `Chemotaxis`/`TissueGrid` exposing enough
    surface for headless testing.

## Pathogens (`ImmunologyTD.Pathogens`)

- **`PathogenState`** — `enum { Transiting, Adhered, Cleared }` (unchanged).
- **`PathogenClass`** (new, `enum { IntracellularVirus,
  IntracellularBacterium, LargeBacterium }`) — `GAME_DESIGN.md` section
  4a's pathogen classes. Parasites (multi-coarse-slot footprint) remain out
  of scope, per `SPRINT_PLAN.md`. Assigned on adhesion via a private
  weighted-random pick (`PathogenAgent.VirusChance = 0.45f`,
  `BacteriumChance = 0.25f`, remainder `LargeBacterium` — a judgment call,
  weighted toward virus since spread is the sprint's flagship mechanic; see
  `docs/TEAM_RETRO.md`).
- **`PathogenAgent`** (MonoBehaviour) —
  `Initialize(BoardConfig, TissueGrid, Action<PathogenAgent> onExit,
  Func<CoarseCoord, float, bool> onSpreadRequested)` — signature grew a
  4th parameter this sprint, the spread-request callback (see below).
  Transit behaviour (spawn at fine column 0 on a random row, transit
  rightward at 2 fine-tiles/tick, ~70% adhere / ~30% transit-and-exit) is
  unchanged from Sprint 1. On successful adhesion, `Class` is assigned,
  `Health`/`MaxHealth` are set (`IntracellularMaxHealth = 12f` for both
  intracellular classes, `LargeBacteriumMaxHealth = 18f` — judgment calls,
  see `docs/TEAM_RETRO.md`), and the sprite's rest color/visibility is set
  per class (see "Class-dependent rendering" below).
  - **`InitializeAdheredDirect(BoardConfig, TissueGrid,
    Action<PathogenAgent> onExit, Func<CoarseCoord, float, bool>
    onSpreadRequested, CoarseCoord slot, PathogenClass pClass, float
    currentTime)`** (new) — places a pathogen directly into `Adhered`
    state at a known coarse slot, bypassing the transit walk. The only
    production caller is `PathogenSpawner.RequestSpread` (a viral spread
    event creates a new infected cell instantly, not by walking there).
    `currentTime` is an explicit parameter (not read via `UnityEngine.Time`)
    so this stays headlessly testable — see
    `Assets/Editor/CombatVerification.cs`.
  - **`ReceiveDamage(float amount, SearchUnit source)`** (signature
    changed in Sprint 3 — was `ReceiveDamage(float)`) — flat per-hit damage
    (`ContactDamagePerHit = 1f`, per `SPRINT_PLAN.md`'s "keep damage
    numbers simple"). No-op unless `State == Adhered`.

    `source` is the unit that landed the hit, and drives kill attribution:
    **exactly one unit is ever credited with a kill — whoever's hit crosses
    zero.** Earlier hits from other units credit nothing, and later hits on
    the same tick no-op at the `State` guard, so there is no split or
    shared credit. Credit is applied *before* `ClearFromCombat()`, because
    clearing can return this instance to its pool. That single credit is
    what drives unit depletion (`SearchUnit.RegisterKill`, below).

    **`source` may be null, and always will be legal** — viral spread,
    degranulation collateral, and harness fixtures all pass an attacker
    where crediting makes no sense or none exists. A null source simply
    means nobody is credited.
    (`ContactDamagePerHit = 1f`, per `SPRINT_PLAN.md`'s "keep damage
    numbers simple"). No-op unless `State == Adhered`. Triggers the
    existing 0.25s flash-toward-highlight visual on every hit (for
    `LargeBacterium`; intracellular classes have their sprite disabled —
    see below, so the flash is invisible for them, a known trade-off, see
    `docs/TEAM_RETRO.md`). `Health <= 0` calls `TissueGrid.ReleaseSlot`
    and invokes `onExit` — same pool-return path Sprint 1's
    transit-and-exit already used.
  - **`TickCombat(float currentTime)`** (new) — the viral spread check.
    Called every frame from `Update()` with `Time.time` while `State ==
    Adhered`; also directly callable with simulated time for headless
    testing. No-ops unless `Class == IntracellularVirus`. Once
    `currentTime - infectionStartTime >= IncubationSeconds` (`15f`, a
    judgment call — see `docs/TEAM_RETRO.md`) and at least
    `SpreadRetryIntervalSeconds` (`1f`) has passed since the last attempt,
    calls `onSpreadRequested(currentCoarseCoord, currentTime)`. On success,
    sets a `hasSpread` flag so this infection only spreads once; on
    failure (every neighbour occupied), keeps retrying every
    `SpreadRetryIntervalSeconds` rather than giving up permanently.
  - **Class-dependent rendering** — `LargeBacterium` keeps its own small
    sprite visible (`BoardRenderer.PathogenColor`, dark maroon) and
    flashes on hit. Intracellular classes have their sprite **disabled
    entirely** (`sr.enabled = false`) once adhered. An earlier version
    tinted the sprite to flat `BoardRenderer.HostColor` and left it
    enabled — wrong, because `BoardRenderer`'s coarse-cell background is
    *heat-blended* toward orange by local cytokine strength while a flat
    small sprite sitting on top isn't, so it was visibly a different shade
    than its surroundings, an accidental "tell" that defeated
    `GAME_DESIGN.md` section 4a's "not visible as itself, until sensed."
    Found via an actual build screenshot, not the headless harness (which
    can't see rendering) — see `docs/TEAM_RETRO.md`.
  - `ResetForPool()` — now also clears `onSpreadRequested`, `hasSpread`,
    `Health`, `MaxHealth`.
- **`PathogenSpawner`** (MonoBehaviour) — `Initialize(BoardConfig,
  TissueGrid, CytokineField, GameObject pathogenTemplate)` (unchanged
  signature). Owns the `PrefabPool` for pathogens, spawns on a timer
  (`spawnIntervalSeconds = 2.5`, capped at `maxLivePathogens = 40`).
  - **`RequestSpread(CoarseCoord source, float currentTime)`** (new,
    `public bool`) — the production implementation of viral spread, and
    the method `PathogenAgent.TickCombat` calls via its
    `onSpreadRequested` delegate. Shuffles the four coarse-grid von Neumann
    neighbour offsets (Fisher-Yates on a 4-element array, so spread
    direction isn't biased), tries each until it finds one that's
    in-bounds and free, then spawns a child `PathogenAgent` through the
    same pool as ordinary spawns
    (`InitializeAdheredDirect(..., PathogenClass.IntracellularVirus,
    currentTime)`) and adds it to `live`. Returns `false` if every
    neighbour is occupied or `live.Count >= maxLivePathogens`. Public (not
    just reachable via `PathogenAgent`) so
    `Assets/Editor/CombatVerification.cs` can seed an origin infection
    manually and drive the exact same production spread path.
  - **`Live`** (new, `IReadOnlyList<PathogenAgent>`) — read-only view of
    the `live` list, added so a headless harness can advance every live
    agent's `TickCombat` with simulated time (Unity's own `Update()` loop
    doesn't run in Editor batchmode outside Play Mode — see
    `docs/ENGINE_STATUS.md`'s `PrefabPool` bug note for a related gotcha).

**Depth/descent simplification, stated explicitly:** this sprint has no
multi-depth descent. A pathogen's coarse `Row` at adhesion is chosen
randomly at spawn time and reached directly — there's no depth-1 →
depth-2 → ... progression, no burrowing, no barrier-colonisation capacity
limit (`GAME_DESIGN.md` section 6b). "Adhesion + presence" per
`SPRINT_PLAN.md` is interpreted here as "reaches *some* coarse row and
stops," not "reaches the mucus layer specifically and stays there." This
was a deliberate scope call to keep pathogens distributed across the full
board (useful for testing search across board width, per section 7's
"board width is the real difficulty knob") rather than clustering them all
at depth 1. Flagging in case the Director's mental model of "adhesion"
specifically meant the mucus/barrier row (depth 1) and not any row — easy
to change (`PathogenAgent.Initialize`'s `targetRow = Random.Range(0,
BoardConfig.Rows)` would become `targetRow = 0` or similar) if so.

## Pooling (`ImmunologyTD.Pooling.PrefabPool`)

Unchanged from Sprint 0 except `SetPrefab(GameObject)` (added Sprint 1) and
one Sprint 2 fix: **`EnsurePool()`, a private lazy-init guard, is now
called from `Get()`/`Release()` as well as `Awake()`**. Previously the
underlying `ObjectPool<GameObject>` was only built in `Awake()`, on the
(wrong) assumption that `Awake()` fires synchronously on
`AddComponent<PrefabPool>()` even outside Play Mode — it doesn't reliably;
`Awake()` needs the player loop actually running (Play Mode, or a real
build). A headless `Assets/Editor/CombatVerification.cs` run hit this
directly as a `NullReferenceException` in `Get()`. The fix is harmless in
normal gameplay (by the time anything calls `Get()`, `Awake()` has already
run) and is what makes `PrefabPool` safely usable from a batchmode Editor
script with no Play Mode, same testability goal as everything else this
sprint. See `docs/ENGINE_STATUS.md` for the full story.

`SetPrefab(GameObject)` (Sprint 1) — Sprint 0's `PrefabPool` only
supported assigning `prefab` via the Inspector's serialized field, which
assumes an interactive Editor session laying out the scene by hand. This
project's whole scene is built from code at runtime with no hand-authored
prefab assets — `SetPrefab` lets a pool be wired up programmatically. Safe
to call any time before the first `Get()` (the pool's `createFunc` closure
reads the `prefab` field lazily, not at construction time), so call order
relative to `AddComponent<PrefabPool>()` doesn't matter.

Both `SearchUnit` and `PathogenAgent` templates are runtime-created
`GameObject`s (inactive, never added to the "real" scene content, just
kept as the clone source), not `.prefab` asset files — see "Scene
construction" below for why.

## Rendering (`ImmunologyTD.Rendering`)

- **`RuntimeSprites.SquareSprite`** — a single cached 1×1 white sprite
  (procedurally generated 4×4 texture), tinted per-instance via
  `SpriteRenderer.color`. No imported art assets this sprint (see
  `SPRINT_PLAN.md`'s exclusion list) — flat colour quads are the entire
  visual language.
- **`BoardRenderer`** (MonoBehaviour, lives on the bootstrap GameObject) —
  `Bind(BoardConfig, TissueGrid, CytokineField, SpriteRenderer[,] views)`
  (unchanged signature). Owns the `SpriteRenderer[,]` for the coarse-cell
  background quads, polls `TissueGrid`/`CytokineField` every 0.15s and
  recolors each cell: host tissue (`HostColor = 0.80, 0.62, 0.66` —
  eosin-ish pink) vs. **as of Sprint 2, occupant class-dependent** (see
  below) rather than "any occupied slot" — then blends in a warm tint
  (`1.00, 0.55, 0.05`) proportional to `CytokineField.CoarseValueAt(coord)
  / TissueGrid.MaxSecretionStrength`, up to 65% blend at full strength,
  unchanged from Sprint 1. **The heatmap tint is still deliberately
  independent of `CytokineToggle`** — see Sprint 1's original reasoning,
  still valid.

  **Sprint 2 change — class-dependent base color**:
  `HostColor`/`PathogenColor` are now `public static readonly` (were
  private) so `PathogenAgent` can reference the same constants for its own
  sprite's rest color (see Pathogens section above). `ShowsAsPathogenItself
  (PathogenAgent)` (new, `public static bool`) is a side-effect-free
  predicate — `GAME_DESIGN.md` section 4a's occupant/render split as
  code: true only for `PathogenClass.LargeBacterium` (reads as itself,
  `PathogenColor`); false for both intracellular classes and for `null`
  (reads as `HostColor`, i.e. bare/infected host tissue are visually
  identical except for the heatmap tint). Extracted as a static method —
  same reasoning as Sprint 1's `Chemotaxis.ChooseNextStep` extraction — so
  `Assets/Editor/CombatVerification.cs` can assert it directly without a
  bound `SpriteRenderer[,]` array.
- **`CompartmentLabel`** (new, MonoBehaviour) —
  `Initialize(Vector3 worldPosition, string text, Vector2? sizeOverride =
  null)`. Small reusable IMGUI label anchored to a world-space point via
  `Camera.main.WorldToScreenPoint` every `OnGUI` call (no uGUI this
  project — see below). Used for the bone marrow strip's heading and the
  lymph node's caption, avoiding near-identical `OnGUI` boilerplate in two
  places.
- **`HudOverlay`** (MonoBehaviour) — IMGUI (`OnGUI`) debug text.
  `Bind(BoardConfig, int macrophageSpeed, int neutrophilSpeed)` — **lost
  the `macrophageCount`/`neutrophilCount` parameters this sprint**, since
  there's no longer a fixed starting unit count (`GameBootstrap` doesn't
  spawn anything at startup — see Scene construction below); the HUD now
  shows per-type speed only, plus a line telling the player to place bone
  marrow towers. Also shows the live cytokine-sensing toggle state and a
  one-line explainer of the heatmap tint (Sprint 1 closing task).
  **Deliberately not `UnityEngine.UI`** — this project's package manifest
  doesn't include `com.unity.ugui` (Unity 6 split uGUI out to its own
  package), and adding a package needs network access and is normally an
  Editor-GUI/Director step (same constraint noted in `ENGINE_STATUS.md`
  for Steamworks). IMGUI needs nothing extra and is a reasonable fit for a
  debug overlay. If real UI ever needs uGUI's layout system, that's a
  network-requiring setup step to do consciously, not something to
  half-adopt via one HUD script.

## Verification harness (`Assets/Editor/CytokineVerification.cs`, added in the Sprint 1 closing task)

An Editor script, not shipped in any build (`Assets/Editor` is
Editor-only). Drives the actual production classes (`TissueGrid`,
`CytokineField`, `Chemotaxis.ChooseNextStep`) directly, with no
`GameObject`s that need play mode and no rendering, made possible by
`TissueGrid`/`CytokineField` taking simulated time and `Chemotaxis` reading
`UnityEngine.Random` (which works outside play mode) rather than depending
on anything play-mode-only. Two entry points, both run via `Unity.exe
-batchmode -quit -projectPath <path> -executeMethod
CytokineVerification.<Method>`:

- `RunComparison()` — the closing task's required self-verification.
  Simulates 10 virtual units (mixed macrophage/neutrophil speed) against 5
  infected sites spread across a 30x5 board for 2.5 simulated minutes,
  once with `cytokineEnabled: false` and once `true` (same random seed),
  logging the average unit-to-nearest-infected-cell distance in 1-minute
  buckets. Measured result (see `ENGINE_STATUS.md`'s Build status section
  for the full numbers): OFF stays flat around 3.0 coarse cells the whole
  run; ON drops to ~0.2 within the first minute and 0.0 (units sitting on
  an infected cell) for the rest.
- `RunFineGrainedSweep()` / `RunSharpnessSweep()` — the tuning tools used
  to pick `Chemotaxis.GradientSharpness = 4f`, not part of the required
  verification. Kept in the repo since retuning this value is a plausible
  future ask (e.g. once real balance passes start) and re-deriving this
  harness from scratch would be wasted effort.

## Verification harness (`Assets/Editor/CombatVerification.cs`, added Sprint 2)

Same philosophy and constraints as `CytokineVerification.cs` above — no
Play Mode, no rendering, drives the real production classes directly. Run
via `Unity.exe -batchmode -quit -projectPath <path> -executeMethod
CombatVerification.RunAll`. One entry point, four groups of assertions
(`RunDamageAndClear`, `RunClassRendering`, `RunViralSpreadTiming`,
`RunBoneMarrowEmission`), each logging `PASS`/`FAIL` per assertion via a
small local `Check(string label, bool condition)` helper plus a final
`X passed, Y failed` summary. **35/35 passed** as of this sprint — see
`docs/ENGINE_STATUS.md` for the specific results, especially the viral
spread chain-reaction numbers.

Notably exercises `PathogenSpawner.RequestSpread` (the real spread
implementation) and `BoneMarrowManager.Tick`/`PlaceTower` (the real
placement/emission implementation) directly via `AddComponent`, which is
what surfaced the `PrefabPool.Awake()`-doesn't-fire-outside-Play-Mode bug
(see `docs/ENGINE_STATUS.md`) — worth knowing if a future harness
`AddComponent`s something that depends on another component's `Awake()`.

## Scene construction (`ImmunologyTD.Bootstrap.GameBootstrap`)

The entire scene (still `Assets/Scenes/Sprint1.unity` — not renamed this
sprint, see `docs/ENGINE_STATUS.md`'s "Scene" note) is a single
`GameObject` named `GameBootstrap`, carrying the `GameBootstrap` and
(via `[RequireComponent]`) `BoardConfig` components. Everything else —
camera, host-cell grid quads, bone marrow/lymph node compartments,
unit/pathogen pools and their templates, HUD — is built at runtime in
`GameBootstrap.Awake()`. This was a practical choice, not a stylistic one:
there is still no interactive Unity Editor session available, so
hand-authoring scene YAML was the only alternative, and that's far more
failure-prone than letting Unity's own `GameObject`/`AddComponent` API
build the scene from a script. See `Assets/Editor/SceneSetup.cs` — an
Editor script, run via `-executeMethod SceneSetup.RebuildSprint1Scene` in
batchmode — which creates the single bootstrap object and saves the scene
(unchanged this sprint). `BuildScript.EnsureSceneExists()` also knows how
to recreate this same single-object scene from scratch if the `.unity`
file is ever missing, as a fallback.

**Sprint 2 additions to `Awake()`**: no longer spawns any units at
startup — builds the (empty) macrophage/neutrophil `PrefabPool`s and hands
them to a new `BoneMarrowManager` instead (see Bone marrow section above).
Computes an overall `Layout` (tissue board bounds + bone marrow strip
position/slots + lymph node position, see `GameBootstrap.BuildLayout`) and
fits the camera to the union of all three compartments' bounds, not just
the tissue board. Camera fitting also gained a **one-frame-later refit**
(`RefitCameraNextFrame`, a coroutine) on top of the original immediate fit,
since `Camera.aspect` read at `Awake()` (frame 0) isn't guaranteed to match
the real runtime window size yet — see `docs/ENGINE_STATUS.md` for the
full story, including that this specific fix wasn't actually what caused
the cropping symptom that prompted it (a screenshot-tooling DPI issue was).
Also logs a one-line diagnostic (`Debug.Log`, lands in `Player.log`) of the
camera's position/orthographic size/aspect and each compartment's world
position, both at `Awake()` and after the refit — added specifically so a
scripted verification pass can compute exact click coordinates from logged
values instead of guessing, and it's cheap enough to leave in permanently.

**If a future session gets real interactive Editor access** (or a Design
agent wants to hand-place things), this code-driven approach can coexist
with hand authoring — `GameBootstrap` doesn't assume it's the only thing in
the scene, it just doesn't currently expect anything else to be there. Get
in touch with whoever's touching the scene before changing that assumption
though, since e.g. `BuildCamera()` unconditionally creates a new Main
Camera regardless of whether one already exists.

## Unit lifecycle and per-tower tuning (`ImmunologyTD.Units`, new Sprint 3)

The contract that bounds population. Design rationale: `GAME_DESIGN.md`
§6d. **Nothing in this section is a `const` — that is the whole point.**

### `UnitLifecycleTuning` (new class)

The mutable, per-tower copy of a unit kind's lifecycle numbers. Plain
public fields on a reference type:

| Field | Default | Meaning |
|---|---|---|
| `MaxActiveChildren` | 10 | Ceiling on one tower's simultaneously-alive children |
| `KillLimit` | 5 neutrophil / **20 macrophage** | Kills before the unit depletes |
| `DegranulatesOnDepletion` | true neutrophil / false macrophage | Burst-and-die vs. quiet retirement |
| `DegranulationBurstMultiplier` | 3 | Collateral burst as a multiple of `ContactDamagePerHit` |
| `ContactRadiusFineTiles` | 2 | Chebyshev contact range |

- `UnitLifecycleTuning.FromProfile(UnitProfile)` — builds a fresh instance
  from the per-*kind* defaults, which live as serialized fields on
  `UnitProfile`.
- `BoneMarrowManager.PlaceTower` calls it once per tower, so **each tower
  owns its own instance**. A future upgrade is a write to one tower's
  field and nothing else — that is the Director's stated requirement
  (2026-08-21), and it is why the cap is per-progenitor rather than
  systemic.
- `BoneMarrowManager.GetTuning(index)` hands a tower's instance out.
- **An emitted unit holds a LIVE REFERENCE to its tower's tuning, not a
  snapshot** (Director, 2026-08-21). An upgrade therefore applies instantly
  to every one of that tower's currently-fielded children as well as its
  future ones — spending ATP is meant to make an immediate difference. It
  does not leak across towers, and never mutates the shared `UnitProfile`
  default. Do not reintroduce snapshot semantics.

### `BoneMarrowManager` additions

- `TotalActiveUnits` — sum of live children across all towers. The
  headline observable; shown in the HUD.
- `SlotCount`, `LastEmittedUnit`, per-slot live-children access for the
  HUD and harness.
- A tower **stops emitting at `MaxActiveChildren`** even when its emission
  timer has elapsed, and resumes when a child dies.
- **The blocked emission timer is clamped at the interval, not banked.**
  This is what keeps the two caps independent per §6d — a tower whose
  whole population dies at once refills at one cell per
  `EmissionIntervalSeconds`, rather than discharging every emission it was
  "owed" while blocked. Do not change this to accumulate.

### `SearchUnit` additions (all harness-callable, none read `UnityEngine.Time`)

- `Kills` / `RegisterKill()` — kill count; called only by
  `PathogenAgent.ReceiveDamage` on the hit that crosses zero.
- `KillLimit`, `DegranulatesOnDepletion`, `ContactRadiusFineTiles` —
  read-through to this unit's snapshot.
- `OwnerSlotIndex` — which tower emitted this unit (`-1` if none).
- `IsDepletionDue` — `Kills >= KillLimit` and not already depleting.
- `ResolveDepletionIfDue()` — the depletion transition. Degranulates first
  if this kind does, then despawns; returns whether it fired. A
  `depleting` guard prevents a kill landed *by* the degranulation burst
  from recursing into a second depletion.
- `SimulationTick()` / `CheckContact()` — explicit-time movement and
  contact, per the project convention.
- `ResetForPool()` — clears kill count, tower back-reference, and transient
  state so a recycled unit carries nothing stale.
- **Despawn returns the unit to its `PrefabPool` and notifies its tower**,
  which decrements its live-children count. This return path did not exist
  before Sprint 3 — `PrefabPool.Release` was never called for a
  `SearchUnit` at all.

### `DegranulationFlash` (`ImmunologyTD.Rendering`, new)

- `Configure(PrefabPool)` — wired once by `GameBootstrap`.
- `Play(Vector3 worldPosition, float worldSize)` — a 0.45s pale-yellow
  expanding burst, pooled like everything else (`GAME_DESIGN.md` §8). It
  exists so the two depletion paths are visually distinguishable; a
  neutrophil's death should read as an event, not as a unit vanishing.

## Map 01 geometry (`ImmunologyTD.Grid.BoardConfig`, rewritten Sprint 4)

Design: `GAME_DESIGN.md` §1a. The board is **100 × 40 coarse cells** in
three lateral bands. `Rows` is no longer a `static const` — it is an
instance property, and the `[Range(24,40)]` clamp on columns is gone.

### The axis frame — read this before writing any movement code

**Constraint, not a convention: no movement code may hardcode a direction.**
Pathogen advance is specified as *toward the base*, and the base is a map
property so later maps can put it anywhere (Director, 2026-08-21).

| Member | Meaning |
|---|---|
| `ThreatAxis` | `Horizontal` or `Vertical` |
| `BaseEnd` | Which end of that axis the base occupies |
| `AxisIndex(CoarseCoord)` | **Distance from the base.** 0 = outermost base cell, always, whichever world side that is |
| `CrossIndex(CoarseCoord)` | Lane index, perpendicular to the threat axis |
| `AxisLength` / `CrossLength` | 100 / 40 on Map 01 |
| `CoarseFromAxis(axis, cross)` | Inverse of the two above |
| `OffsetInAxisFrame(c, dAxis, dCross)` | **The only sanctioned way to step along the threat axis.** `dAxis = -1` is always one cell toward the base |
| `InAxisBounds` / `InCrossBounds` | Bounds in the frame |

`AxisIndex` flips internally when `BaseEnd == Positive`, which is what makes
"−1 is toward the base" true in both configurations while pointing at
opposite world columns. `MapVerification` asserts exactly this on a mirrored
board.

### Bands

`BaseBandCells` (25), `LumenBandCells` (25), `TissueBandCells` (derived,
50). `BandAtAxisIndex(int)` / `BandOf(CoarseCoord)` return
`BoardBand.Base | Tissue | Lumen`. Named edges: `TissueBaseEdgeAxisIndex`
(where units enter), `TissueLumenEdgeAxisIndex`, `LumenNearWallAxisIndex`,
and `LumenDepthFromInterface(...)` (0 = hugging the wall).

**The outer two band sizes are clamped against the axis length, so a board
too small to hold them silently starves the tissue band to zero.** That
shipped once — see `ENGINE_STATUS.md` known issues.
`GameBootstrap.WarnOnDegenerateBands` now logs an error for it.

Lumen flow has its own end: `FlowCrossStep`, `LumenEntryCrossIndex`,
`IsExcretedCrossIndex(int)`.

`ConfigureForTest(columns, rows, axis, baseEnd, baseCells, lumenCells, flowEnd)`
builds non-default geometry from code. Test/bootstrap only.

## Gut interface (`ImmunologyTD.Grid.GutInterface`, new Sprint 4)

One pile of adhered pathogens per lane; `PositionCount == CrossLength`.

- `Adhere(position, pathogen)` / `Remove(pathogen)` — the latter for a
  pathogen cleared while still on the wall.
- `AdheredCountAt(position)`, `AdheredAt(position)`, `TotalAdhered`,
  `PeakAdhered` — pressure, for the renderer and HUD.
- `BreachChanceAt(position)` = `1 - (1 - PerPathogenBreachChance)^n`, so a
  position's odds **rise with the pile on it**.
- `Tick(deltaTime, currentTime)` — advances the roll clock, rolls every
  occupied position when due, returns the positions that breached. The
  clock is held at zero while the wall is clean, so the first pathogen to
  adhere waits a full interval instead of catching a stale roll.
- **`Breach(position, currentTime)` releases EVERY pathogen at that
  position in one call** and returns how many entered tissue. This is
  `SPRINT_PLAN.md` item 6's mechanic: **do not** reshape it into
  per-pathogen rolls. A pathogen with nowhere to go stays on the wall
  rather than being dropped. Public so a harness — or a future
  player-triggered ability — can trip a position deterministically.
- **`event Breached(position, releasedCount)`** — subscribe rather than
  poll `BreachedThisTick`. Script execution order between `PathogenSpawner`
  (which ticks this) and a renderer's `Update()` is undefined, so polling
  drops roughly half the bursts, and an undrawn burst defeats the mechanic.

## Invasion state and tuning (`ImmunologyTD.Pathogens`, new Sprint 4)

**`InvasionTally`** — `Adhesions`, `Breaches`, `ReleasedIntoTissue`,
`Excreted`, `ReachedBase`, plus `Reset()`. Drives the HUD; `ReachedBase` is
this sprint's endzone counter (the 100-life pool is Sprint 5).

**`InvasionTuning`** — every invasion number as a mutable static, with
`ResetToDefaults()`: `LumenStepIntervalSeconds` 0.35,
`AdhesionChanceAtWall` 0.12, `AdhesionFalloffCells` 5,
`BreachRollIntervalSeconds` 1, `PerPathogenBreachChance` 0.012,
`MaxReleaseAxisDepth` 3, `MaxReleaseCrossSpread` 20,
`TissueStepIntervalSeconds` 1, and advance weights
`AdvanceBaseWeight` 0.70 / `AdvanceLateralWeight` 0.13 / `AdvanceAwayWeight`
0.04. **All are unvalidated defaults.** Harnesses that change them must
call `ResetToDefaults()` afterwards.

## Pathogen lifecycle (`PathogenAgent`, largely rewritten Sprint 4)

`PathogenState` is now `Lumen | AtInterface | InTissue | Cleared`.

- `Initialize(board, tissueGrid, gutInterface, tally, onExit, onSpreadRequested)`
  — spawns into the lumen at the upstream end, at a random distance from
  the wall.
- `InitializeInTissueDirect(..., slot, pClass, currentTime)` — **renamed
  from Sprint 2's `InitializeAdheredDirect`**, because "adhered" now means
  the gut wall rather than tissue. Places directly into tissue, bypassing
  lumen and wall; used by viral spread and by harness fixtures.
- `SimulationTick(deltaTime, currentTime)` — explicit-time, harness-callable.
  Lumen: step, then one adhesion roll. Tissue: step, then `TickCombat`.
  `AtInterface`: deliberately does nothing — a colonising pathogen waits for
  `GutInterface` to roll its position, it does not decide its own breach.
- `static AdhesionChanceAt(lumenDepthFromInterface)` —
  `AdhesionChanceAtWall * exp(-depth / falloff)`; side-effect-free so it can
  be asserted directly.
- `AdhereToInterface(position)` — leaves the flow and **moves to the wall**.
  Public for deterministic harness setup.
- `EnterTissueAt(slot, currentTime)` — the wall→tissue transition.
- `CurrentCoarse`, `InterfacePosition` (−1 when not on the wall).

Advance in tissue consults only the axis frame; reaching a `Base`-band cell
despawns the pathogen and increments `InvasionTally.ReachedBase`.

## Sprint 5 changes — host states, debris, class-specific advance

Design: `GAME_DESIGN.md` §1c and §1b step 4. The `TissueGrid` two-layer
rewrite is documented under **Occupancy state** above; the deltas to the
other classes:

### `PathogenAgent`

- **`bool IsIntracellular { get; private set; }`** — true when this pathogen
  is inside a host cell (on `TissueGrid`'s host layer, cell `Infected`), not
  on the occupant layer. **Rendering is now driven by this, not by
  `Class`**: `ApplyRestColorForCurrentClass` hides the sprite iff
  `IsIntracellular`, so an intracellular bacterium walking *between* hosts
  and a free virus particle are both drawn as themselves. Sprints 2–4 hid
  both intracellular classes permanently because "outside a cell" did not
  exist.
- **`SettleIntoTissue(slot, currentTime)`** (private) — decides which layer
  a pathogen lands on when it arrives in tissue. A virus takes the host cell
  if `Healthy` (→ intracellular immediately); everything else, and a virus
  with no healthy cell here, lands on the occupant layer. Called from
  `InitializeInTissueDirect` and `EnterTissueAt`.
- **`EnterTissueAt`** — a virus may now come off the wall onto a `Healthy`
  host even when the occupant layer at that position is busy
  (`CanTakeHostAt`); only a pathogen that gets *neither* layer stays on the
  wall.
- **Class-specific advance** — `SimulationTick`'s tissue branch now calls
  `StepInTissue(currentTime)`, which dispatches by class:
  - **Virus** — an intracellular virus does not move (spread is
    `TickCombat`'s job). A *free* virus (`StepVirus`) steps only onto a
    `Healthy` host in its own cell or a von Neumann neighbour
    (`TryFindAndEnterHost`), and dies after
    `InvasionTuning.VirusFreeSurvivalSeconds` if it finds none. No firebreak
    check exists anywhere — it emerges from these two local rules.
  - **Intracellular bacterium** (`StepIntracellularBacterium`) — base-biased
    walk while extracellular; each step may enter a `Healthy` cell it stands
    on (`InvasionTuning.IntracellularEntryChance`); hidden and stationary
    inside until it lyses out after
    `InvasionTuning.IntracellularResidenceSeconds`, which **calls
    `ReleaseIntracellular` then `KillHostCell`** (detach-before-kill, so the
    bacterium survives and keeps walking) leaving debris.
  - **Large bacterium** (`StepMotile`) — unchanged base-biased walk, but
    grazes the host cell under it for
    `InvasionTuning.LargeBacteriumHostDamagePerStep` each step
    (`DamageHostCell`).
- **`OnHostCellDestroyed()`** — called by `TissueGrid.KillHostCell` on the
  cell's intracellular resident (already detached before the call). The
  pathogen dies with its host: `State = Cleared`, `onExit`. Guards on
  already-`Cleared` so the innate-clear path (which sets `Cleared` *before*
  `KillHostCell`) doesn't double-exit.
- **`ClearFromCombat`** splits by layer: an intracellular pathogen cleared
  by innate immunity takes its host cell with it (`KillHostCell` → `Dead` +
  debris, §4a); an extracellular one just `ReleaseOccupant`s.

New `InvasionTuning` statics (mutable, `ResetToDefaults()`, all unvalidated
mechanics-first defaults): `VirusFreeSurvivalSeconds` 6,
`IntracellularEntryChance` 0.5, `IntracellularResidenceSeconds` 12,
`LargeBacteriumHostDamagePerStep` 2.5.

### `PathogenSpawner.RequestSpread`

Now also requires `tissueGrid.IsHealthyHost(candidate)` — §1c ("a virus can
only spread into a `Healthy` neighbour"). Previously checked only
`IsOccupantFree`, which let a walled-in virus burn its one-shot `hasSpread`
on a doomed free particle. This is the other half of the firebreak.

### `SearchUnit`

- **`SimulationTick()` → `SimulationTick(float currentTime)`.** Only caller
  is `Update()` (passes `Time.time`); no harness calls it. Threaded through
  for the regrowth-clock stamp.
- **`bool CheckEfferocytosis(float currentTime)`** (new, public,
  harness-callable like `CheckContact`) — a unit with
  `tuning.EfferocytosisDebrisPerTick > 0` (macrophage only, by default)
  standing on a `Dead` cell calls `TissueGrid.ClearDebris` for one bite.
  Opportunistic — the unit's own coarse slot only, no seeking. Plays a
  `DegranulationFlash` in `DegranulationFlash.EfferocytosisColor` when a
  pile is finished.
- **`float EfferocytosisDebrisPerTick`** — read-through to
  `tuning.EfferocytosisDebrisPerTick`.

### `UnitProfile` / `UnitLifecycleTuning`

New per-tower mutable field **`float EfferocytosisDebrisPerTick`** (§6d
pattern: never a const, so a future macrophage upgrade is a one-field
write). Default 0 = "this kind does not clear debris" — how "only
macrophages do it" falls out with no kind check. `GameBootstrap` sets the
macrophage to `0.05` (~2.5s per full pile), neutrophil to `0`. Wired
through `FromProfile` / `CopyFromProfile` / `CopyFrom`.

### `BoardRenderer` (`ImmunologyTD.Rendering`)

- **`static Color HostStateColor(HostState)`** — the four host states as
  four distinguishable colours: `HostColor` (pink) / `InfectedHostColor`
  (bruised violet, `0.54,0.36,0.60`) / `DebrisColor` (grey-brown,
  `0.38,0.34,0.28`) / `EmptyGroundColor` (near-black, `0.13,0.11,0.12`).
  Static and side-effect-free so a harness can assert the four are distinct.
- `DebrisColor` / `EmptyGroundColor` are new `public static readonly`;
  `InfectedHostColor` was added with item 1.

### `GameBootstrap.BuildLayout`

Rewritten (item 6). The bone-marrow strip and lymph-node backdrop are now
sized as fractions of `BandWorldRect(BoardBand.Base)` with explicit
non-overlapping vertical budgets (4% margin | marrow 62% | 4% gap | lymph
30%), and slot size is capped against the band width too. The 25×10 resize
had left them sized for 100×40, spilling across the board.

### `Assets/Editor/TissueVerification.cs` (new Sprint 5, grown Sprint 6)

`TissueVerification.RunAll` — **73 assertions** (was 53): two-layer
occupancy, death→debris, debris-as-terrain, efferocytosis, the viral
firebreak, class-specific advance, and Sprint 6's §4b coverage (contact
stress-sense, exposed-vs-hidden bacterium, replication + brood burst +
caught-early-no-brood, budding disk, spontaneous burn-out). Same
drive-the-real-classes, no-Play-Mode philosophy as the four before it.

## Sprint 6 changes — the intracellular-infection rework (`GAME_DESIGN.md` §4b)

An established intracellular infection is no longer reachable by ordinary
innate damage. Full design in §4b; the contract deltas:

### `TissueGrid`

- **`GetAttackableAt(CoarseCoord)` returns the extracellular occupant
  ONLY.** The intracellular resident is never returned — Sprints 2–5
  returned it and let a macrophage grind it down through the cell; that
  path is gone.

### `PathogenAgent`

- **`ReceiveDamage(amount, source)` is a no-op while `IsIntracellular`.**
  Ordinary damage cannot touch a hidden pathogen. `ClearFromCombat` now
  only handles the extracellular case; the intracellular case leaves via
  `OnHostCellDestroyed` when its host cell is killed (stress-sense roll,
  drain-death, or collateral).
- **`onSpreadRequested` → `onSpawnNear`**, type
  `Func<CoarseCoord, PathogenClass, bool, float, bool>` — `(source,
  class, asFreeParticle, currentTime) → spawned?`. One delegate for viral
  spread and the bacterial brood.
- **`TickCombat`** now drives an ESTABLISHED viral infection only
  (`if (!IsIntracellular) return`): a one-time spontaneous burn-out roll
  (`VirusBurnoutChance`, fires `VirusBurnoutMin..MaxSeconds` later →
  `BurnOut`), then spread — **budding** (`virusBuds`: emit a free virion
  every `VirusBuddingIntervalSeconds`, `asFreeParticle:true`, forever) or
  **contact-chain** (one `asFreeParticle:false` hop, `hasSpread`).
- **`StepVirus`** is now the FREE-virion tick: roll `VirusEntryChancePerTick`
  to get inside a `Healthy` current cell, else `TryStepFreeVirion` — a
  momentum-biased walk (3× weight to continue `lastHeading`) that may step
  **only onto `Healthy`, occupant-free cells** (this is the firebreak),
  else die on the `VirusFreeSurvivalSeconds` clock. `TryFindAndEnterHost`
  and `hostSearchOrder` are removed.
- **`StepIntracellularBacterium`** — extracellular: no death clock, roams
  (`IntracellularEntryChance` 0.5 → 0.12), takes ordinary damage. Enters a
  `Healthy` cell (per-tick roll). Intracellular: immune to damage;
  replicates every `IntracellularReplicationIntervalSeconds`, draining
  `IntracellularDrainPerReplication`, `broodCount++`. Drain-death →
  `BurstBrood` (detach, `KillHostCell`, this bacterium survives as the
  first of the brood, up to `IntracellularMaxBrood` total via
  `onSpawnNear`). Killed any other way first → `OnHostCellDestroyed`, no
  brood.
- **`EstablishInfection(currentTime)`** — shared "now an intracellular
  infection" setup (sets `IsIntracellular`, resets bud/burn-out state),
  called by `StepVirus` establishing and `SettleIntoTissue`.
- Removed: `hostEntryTime`, `IntracellularResidenceSeconds` (the Sprint 5
  residence-timer / lyse model).

### `PathogenSpawner`

- **`RequestSpread` → `RequestSpawnNear(source, pClass, asFreeParticle,
  currentTime)`.** Contact-chain virus (`asFreeParticle:false`): needs a
  `Healthy`, occupant-free NEIGHBOUR (the firebreak). Budded virion / burn-
  out spill (`asFreeParticle:true`): a free virion dropped on `source`
  itself or any occupant-free tissue cell — no `Healthy` requirement, but
  it can still only ESTABLISH in a `Healthy` cell. Brood: occupant-free
  tissue cell, no `Healthy` requirement.

### `SearchUnit`

- **`bool CheckStressSense(float currentTime)`** (new, public, harness-
  callable) — while in contact (same range test as `CheckContact`) with an
  `Infected` cell, roll `tuning.StressSenseChancePerTick` once per tick; on
  success `KillHostCell` (a loud necrotic kill of cell + all contents,
  nothing released), credit self a kill, play a 1.5× magenta
  `DegranulationFlash` (`StressKillColor`). Called from `SimulationTick`.
- **`Degranulate`** now also `DamageHostCell`s its slot at the burst
  multiplier (§6d "whatever host cell or infected cell is there"), since
  `GetAttackableAt` no longer exposes the intracellular resident.
- **`float StressSenseChancePerTick`** — read-through to the tuning.

### `UnitProfile` / `UnitLifecycleTuning`

New per-tower mutable field **`float StressSenseChancePerTick`** (§6d
pattern; the future γδ T / CTL / NK sensors carry a high value here).
`GameBootstrap`: macrophage `0.03`, neutrophil `0.02`. Wired through
`FromProfile` / `CopyFromProfile` / `CopyFrom`.

### `BoardRenderer`

- **`static Color InfectedColorFor(PathogenAgent resident)`** — viral
  violet vs. bacterial `InfectedByBacteriumColor` (sickly yellow-green).
  `Refresh` uses it for `Infected` cells so the Director can tell the two
  apart and watch a bacterium duck in / burst out.

### `InvasionTuning` — new statics (all unvalidated defaults, `ResetToDefaults()`)

`VirusEntryChancePerTick` 0.20, `VirusBuddingSpeciesChance` 0.5,
`VirusBuddingIntervalSeconds` 2.5, `VirusBurnoutChance` 0.30,
`VirusBurnoutMinSeconds` 8 / `VirusBurnoutMaxSeconds` 25,
`IntracellularReplicationIntervalSeconds` 3, `IntracellularDrainPerReplication`
2.5, `IntracellularMaxBrood` 6; `IntracellularEntryChance` 0.5 → 0.12.

## Sprint 7 changes — the ATP economy and round loop (`GAME_DESIGN.md` §5b/§5d/§6c)

### `ImmunologyTD.Economy`

- **`EconomyTuning`** — mutable statics, `ResetToDefaults()`, all
  placeholder: `StartingAtp` 100, `RoundStartLumpSum` 80, `AtpPerKill` 3,
  `MacrophagePrice` 40, `NeutrophilPrice` 15, `StartingLives` 100,
  `LifeRegenRounds` 2 / `LifeRegenAmount` 1, `BatchSizeBase` 8 /
  `BatchSizeGrowthPerRound` 3. `int BatchSizeForRound(int)`.
- **`AtpWallet`** — plain reference type. `int Balance`, `bool CanAfford(int)`,
  `bool TrySpend(int)` (false if unaffordable, non-positive cost is a free
  success), `void Grant(int)` (ignores non-positive), `void Reset(int)`,
  `int LifetimeEarned` (diagnostics). Constructed by `GameBootstrap`,
  passed by reference — same shape as `InvasionTally`.
- **`EconomyHooks`** — `static System.Action PayForKill` + `ReportKill()`.
  A one-line bridge from `SearchUnit.RegisterKill` to the wallet without
  threading a wallet ref through the unit tree. `GameBootstrap` sets it in
  Awake; a harness leaves it null (kills pay nothing) or points it at a
  test wallet. Same pattern as `DegranulationFlash.Configure` /
  `CytokineToggle`.

### `ImmunologyTD.Rounds.RoundController` (MonoBehaviour)

The round state machine (§5d) and the §6c life pool. Explicit-time
`Tick(float deltaTime)`; `Update()` forwards `Time.deltaTime` and reads
`StartRoundKey` (`KeyCode.Space`).

- `void Initialize(AtpWallet, PathogenSpawner, InvasionTally, BoneMarrowManager)`
  — the last two may be null for a harness.
- `RoundPhase Phase` — `{ Building, Active, Defeat }`; opens in `Building`.
- `int RoundNumber` (0 in the opening buy phase; first `StartRound` → 1),
  `int RoundsCleared`, `int Lives`, `int MaxLives`.
- `void StartRound()` — no-op unless in `Building`. `RoundNumber++`, sizes
  the batch (`EconomyTuning.BatchSizeForRound`), `spawner.BeginBatch(n)`,
  → `Active`. **Pays nothing** — the lump sum is granted on a round CLEAR.
- `Tick` while `Active`: charges new `InvasionTally.ReachedBase` against
  `Lives` (0 → `Defeat`, `spawner.EndBatch()`); and when
  `spawner.BatchComplete`, clears the round — `wallet.Grant(RoundStartLumpSum)`,
  regen a life every `LifeRegenRounds` cleared rounds (capped at `MaxLives`),
  `marrow.ClearFieldedUnits()` (§2), `spawner.EndBatch()`, → `Building`.

### `PathogenSpawner`

**No longer free-runs.** The spawn gate now also checks a batch:

- `void BeginBatch(int count)` — arm to emit exactly `count`, reset the
  spawn clock.
- `void EndBatch()` — disarm (round over / defeat); live pathogens
  untouched.
- `bool BatchComplete` — emitted the target **and** zero pathogens in the
  lumen or tissue. Pathogens on the GUT WALL are deliberately **not**
  counted (§6b: a barrier pile persists round to round).
- `int LiveCount` / `int BatchTarget` / `int BatchEmitted`.

Gut-interface and cytokine ticking are unchanged (they no-op with nothing
live).

### `SearchUnit`

`RegisterKill()` now also calls `EconomyHooks.ReportKill()` — the single
"a unit got a kill" chokepoint. Contact kills (via
`PathogenAgent.ReceiveDamage`) and §4b stress-sense kills (via
`TryStressSenseAt`) pay; brood-burst / burn-out / drain-death do not.

### `BoneMarrowManager`

- `Initialize` gains a trailing **`AtpWallet wallet = null`**. Null keeps
  placement free (the harness path).
- `PlaceTower` calls `wallet.TrySpend(PriceFor(kind))` first and no-ops on
  failure. `static int PriceFor(UnitKind)` reads `EconomyTuning`.
- `void ClearFieldedUnits()` — despawns every fielded child of every
  placed tower and resets emission timers; the towers stay `Placed`.
  Called by `RoundController` on a round clear (§2).
- The IMGUI picker shows `"{Kind}   {price} ATP"` and greys out
  (`GUI.enabled`) what the wallet can't afford.

### `HudOverlay`

`Bind` gains **`AtpWallet wallet, RoundController rounds`**. A top-right
bar draws ATP, `Lives N / MaxLives`, round number + phase, batch progress
during a round, the "+N ATP · Start Round" prompt + button during
`Building`, and a GAME OVER line on `Defeat`.

### `GameBootstrap`

Awake wires `EconomyTuning.ResetToDefaults()`, `wallet = new AtpWallet(...)`,
`EconomyHooks.PayForKill = () => wallet.Grant(...)`, a `RoundController`
(`BuildRoundController`), and passes the wallet to bone marrow. The game
now opens in a buy phase with the spawner un-armed.

### `Assets/Editor/EconomyVerification.cs` (new)

`EconomyVerification.RunAll` — **47 assertions**: the wallet, batch
gating, the round loop (Building → StartRound → Active → drive-to-clear →
Building, lump on clear, batch growth), the life pool (breach → life, 0 →
Defeat, inert after Defeat, regen), placement cost, per-kill income
through the real `SearchUnit.RegisterKill`, and round-boundary unit
clearing. Same no-Play-Mode philosophy as the five before it.

## Sprint 8 changes — the dendritic-cell shuttle and the antigen barcode (`GAME_DESIGN.md` §5a/§5c)

Design: §5a (the DC shuttle loop), §5c (the 8-bit barcode / pairing /
turnover), §1c (debris is the antigen source), §5 (knowledge is a
per-species %). **Framework pass — a rising knowledge % unlocks nothing
yet.**

### `ImmunologyTD.Adaptive` (new namespace)

- **`Antigen`** (static, side-effect-free) — the 8-bit barcode as a plain
  `byte`. `byte RandomTag()`, `int HammingDistance(byte, byte)`
  (`popcount(a ^ b)`), `bool IsMatch(byte, byte)` (distance ≤
  `AdaptiveTuning.MatchMaxHammingDistance`), `byte ForClass(PathogenClass)`.
- **`KnowledgeLedger`** — plain reference type, per run. `float Get(PathogenClass)`
  (0..`KnowledgeMax`), `float Add(PathogenClass, float)` (clamped, returns
  new value), `void Reset()`, `int Revision` (bumps on change, for cheap
  HUD polling). Backing store is `float[3]` keyed by `(int)PathogenClass`.
- **`AdaptiveTuning`** — mutable statics + `ResetToDefaults()`, all
  placeholder. `const int BarcodeBits = 8` (fixed by the Director).
  Mutable: `MatchMaxHammingDistance` 2, `KnowledgePerMatch` 3,
  `KnowledgeMax` 100, `VirusAntigen`/`BacteriumAntigen`/`LargeBacteriumAntigen`
  (≥4 bits apart), `DcPresentationsPerCargo` 4, `DcDebrisSamplePerBite`
  0.34, `DcFineTilesPerTick` 2, `DcAxisWalkBiasSharpness` 1.6,
  `LymphocyteLifespanSeconds` 20, `LymphocyteFineTilesPerTick` 2,
  `PairingSeconds` 1.5, `NodePairingContactFineTiles` 3,
  `NodeColocalisationSourceStrength` 18, `NodeLymphocyteSourceStrength` 6,
  `Dc/LymphocyteEmissionIntervalSeconds`, `Dc/LymphocyteMaxActiveChildren`.

### `TissueGrid` — debris antigen

- **`PathogenClass? GetDebrisAntigen(CoarseCoord)`** (new read) — the
  class of whatever killed the cell whose debris sits here, or null.
- **`KillHostCell(CoarseCoord, PathogenClass? antigen = null)`** and
  **`DamageHostCell(CoarseCoord, float, PathogenClass? antigen = null)`** —
  optional trailing arg. When null and the cell has an intracellular
  resident, `resident.Class` is used, so the stress-sense loud kill
  (`SearchUnit`) and neutrophil collateral need no change. `BurstBrood` /
  `BurnOut` (which detach the resident first) pass `Class` explicitly.
  Every pre-Sprint-8 caller keeps compiling (optional arg). Cleared with
  the pile in `BecomeEmpty`.

### `LymphNode` (`ImmunologyTD.Adaptive`, plain class)

Constructed by `GameBootstrap` / a harness: `new LymphNode(KnowledgeLedger,
Rect worldRect)`. Owns its own `BoardConfig` (6×6 coarse, built via
`ConfigureForTest`) and a `CytokineField` — the **co-localisation signal**
(§5c step 4), recomputed each `Step` from a fixed central source + every
resident lymphocyte as a weak source.

- `void Step(float currentTime)` — recompute the field, `ResolvePairs` /
  `FormPairs`, move residents (`Lymphocyte.NodeTick`), age residents past
  `LymphocyteLifespanSeconds`. **The tick gate is in `AdaptiveDirector`**,
  not here — one clock for the whole arena.
- `void RegisterResident(Lymphocyte)` / `UnregisterResident(Lymphocyte)`;
  `void Admit(INodeVisitor)` / `Release(INodeVisitor)`.
- `BoardConfig NodeBoard`, `CytokineField Coloc`, `Rect WorldRect`,
  `float AgentWorldSize`, `Vector3 NodeToWorld(FineCoord)`,
  `FineCoord RandomInteriorFine()`.
- `int ResidentCount` / `int VisitorCount`, `IReadOnlyList<Lymphocyte> Residents`.
- **`interface INodeVisitor`** — `FineCoord NodePos`, `byte Cargo`,
  `PathogenClass CargoClass`, `bool HasCargo`, `bool Frozen { get; set; }`,
  `void OnPairingResolved(bool taught)`. Implemented by `DendriticCell`;
  lets pairing be written without the node depending on the concrete type.
- Pairing: a visitor with cargo not already paired, and a resident not
  paired, within `NodePairingContactFineTiles` (Chebyshev) → both
  `Frozen`, resolve at `now + PairingSeconds`. On resolve:
  `taught = HasCargo && Antigen.IsMatch(Cargo, resident.Tag)`; if taught,
  `knowledge.Add(CargoClass, KnowledgePerMatch)` + a
  `DegranulationFlash.KnowledgeMatchColor` burst. Either way both unfreeze
  and `visitor.OnPairingResolved(taught)`.

### `Lymphocyte` (`ImmunologyTD.Adaptive`, MonoBehaviour agent)

`Initialize(LymphNode, byte tag, FineCoord start, float bornAt, System.Action<Lymphocyte> onDespawn)`.
`byte Tag` (fixed at birth), `FineCoord Node`, `float BornAt`, `bool Frozen`.
`NodeTick(float)` — `Chemotaxis.ChooseNextStep` against `node.Coloc`
unless frozen; driven by `LymphNode.Step`. `Update()` tweens only.
`DespawnToPool()` routes through `onDespawn`.

### `DendriticCell` (`ImmunologyTD.Adaptive`, MonoBehaviour agent, `INodeVisitor`)

`Initialize(BoardConfig tissueBoard, TissueGrid, CytokineField, LymphNode,
FineCoord tissueStart, System.Action<DendriticCell> onDespawn)`.
`enum DendriticCellState { PatrolTissue, TravelToNode, InNode, ReturnToTissue }`.
Public: `State`, `Current` (tissue fine), `NodePos`, `Cargo`, `CargoClass`,
`HasCargo`, `Frozen`.

- `void SimulationTick(float currentTime)` — dispatches by state. Patrol:
  `Chemotaxis` random walk (cytokine off), then if standing on a `Dead`
  cell with `GetDebrisAntigen` → sample (`Cargo = Antigen.ForClass(...)`,
  `presentationsLeft = DcPresentationsPerCargo`, `ClearDebris` one bite) →
  `TravelToNode`. Travel/return: softmax biased walk in the **axis frame**
  (`dir * AxisIndex`, `DcAxisWalkBiasSharpness`), enter the node at the
  base band / resume patrol at the tissue band. InNode: `Chemotaxis`
  against `node.Coloc`; when `!HasCargo` → `node.Release` → `ReturnToTissue`.
- `OnPairingResolved(bool)` — `presentationsLeft--`; at 0, `HasCargo = false`.
- `void DespawnToPool()` / `void ResetForPool()`.

### `AdaptiveDirector` (`ImmunologyTD.Adaptive`, MonoBehaviour)

`Initialize(LymphNode, PrefabPool lymphocytePool, PrefabPool dcPool,
BoardConfig tissueBoard, TissueGrid, CytokineField)`.

- `float Clock` — the arena's simulated clock.
- `void Tick(float deltaTime)` — sub-steps at `BoardConfig.TickIntervalSeconds`;
  each sub-step advances `Clock`, calls `LymphNode.Step(Clock)` then every
  fielded DC's `SimulationTick(Clock)`. `Update()` forwards `Time.deltaTime`.
- `GameObject EmitDendriticCell(int slotIndex, System.Action<int,GameObject> onSlotChildDespawned)`
  — tissue base edge, random lane. `GameObject EmitLymphocyte(int slotIndex, …)`
  — into the node with `Antigen.RandomTag()`.
- `int DendriticCellCount(int slotIndex)` / `int LymphocyteCount(int slotIndex)`
  — for `BoneMarrowManager`'s cap. `void DespawnAllFielded()` — round boundary.
- `LymphNode Node`.

### `BoneMarrowManager` — four kinds

- **`enum UnitKind { Macrophage, Neutrophil, DendriticCell, HelperT }`**
  (was two). `static bool IsAdaptive(UnitKind)`.
- `Initialize(…, AtpWallet wallet = null, AdaptiveDirector adaptive = null)`
  — trailing optional. An adaptive kind can't be placed without a director.
- `static int PriceFor(UnitKind)` — switch over four; `EconomyTuning`
  gains `DendriticCellPrice` 30 / `HelperTPrice` 25.
- A placed adaptive slot: `Tuning` carries only `MaxActiveChildren` (from
  `AdaptiveTuning`); its emission interval is `IntervalFor(kind)`; its
  live count is `GetActiveChildren(i)` over the new
  `Slot.AdaptiveChildren` (`List<GameObject>`). `Emit` → `EmitAdaptive` →
  the director; `void OnAdaptiveChildDespawned(int, GameObject)` drops the
  tracking ref. `ClearFieldedUnits` calls `adaptive.DespawnAllFielded()`.
- Picker: four priced, grey-out buttons (last two only when `adaptive != null`).

### `HudOverlay`

`Bind(…, KnowledgeLedger knowledge = null, AdaptiveDirector adaptive = null)`
— trailing optional. New KNOWLEDGE line: per-species % + `lymph node: DC n
helper-T n`.

### `DegranulationFlash`

`static readonly Color KnowledgeMatchColor` (bright green) — a matching
DC:helper-T pairing.

### `Assets/Editor/AdaptiveVerification.cs` (new)

`AdaptiveVerification.RunAll` — **34 assertions**: `Antigen` math,
`KnowledgeLedger` clamp, debris antigen, a full simulated shuttle
(matching pairing → exactly one increment on exactly one species;
non-matching → freeze + cargo spent, teaches 0), lymphocyte turnover, the
round boundary. Same no-Play-Mode philosophy as the six before it;
deterministic (seeded `Random`, forced match thresholds).

## Sprint 9 changes — the reworked round model (`GAME_DESIGN.md` §5d / §2)

The buy phase freezes time; the battlefield persists round to round; each
round is delivered by a contaminated food item. Difficulty numbers are
placeholder.

### `ImmunologyTD.Rounds.RoundClock` (new static) + `RoundClockDriver`

- **`static bool Frozen`** — opens `true`. `false` only while a round is
  `Active`. Every `Update()`-driven sim system reads this and
  early-returns when true.
- **`static float Time { get; }`** — a sim clock; `static void
  Advance(float dt)` adds `dt` only when `!Frozen`; `static void Reset()`
  (→ `Frozen = true`, `Time = 0`). Systems that used to pass
  `UnityEngine.Time.time` into a `SimulationTick` now pass `RoundClock.Time`.
- **`RoundClockDriver` (MonoBehaviour)** — one line: `Update() =>
  RoundClock.Advance(UnityEngine.Time.deltaTime)`. Added by `GameBootstrap`.

Harnesses never touch `RoundClock` (they drive every tick explicitly).

### `RoundController`

- `StartRound()` → `RoundClock.Frozen = false`, `CurrentTagline =
  RoundScript.ForRound(RoundNumber).Tagline`, `spawner.BeginRound(batch,
  def)` (was `BeginBatch`).
- A round ending (`ClearRound`) and `Defeat` → `RoundClock.Frozen = true`.
- **`ClearRound` no longer calls `marrow.ClearFieldedUnits()`** — the
  field persists.
- **`string CurrentTagline { get; }`** — new.
- **`void DespawnAllFieldedUnits()`** — new public passthrough to
  `marrow.ClearFieldedUnits()`, kept for a future run-restart (not called
  at the boundary).
- `Initialize` signature unchanged.

### `PathogenSpawner`

- **`void BeginRound(int count, RoundDefinition def)`** — a food round:
  spawns one food item at the lumen entry, arms `count`, records `def`'s
  class mix. **`void BeginBatch(int count)`** kept unchanged (no food) for
  the harnesses.
- **`bool FoodActive { get; }`** — new.
- **`bool BatchComplete`** — under a food round: `batchEmitted >=
  batchTarget && foodExited`. Under `BeginBatch`: the old
  emitted + lumen/tissue-clear rule (a gut-WALL pile still doesn't count).
- `EndBatch()` also hides the food visual.
- Private: `AdvanceFood(dt, now)` crawls the food along `FlowCrossStep`
  over `InvasionTuning.FoodItemTransitSeconds`, fires
  `FoodItemBurstCount` bursts at travelled-fractions `k/(burstCount+1)`,
  `SpawnFromFood()` drops each pathogen at a wall-hugging lumen cell
  (`LumenNearWallAxisIndex + [0..FoodItemWallHugDepth]`) at the food's
  current cross index, class `def.RollClass()`. A food excreted off the
  end force-delivers any remaining cargo. The food visual is one
  non-pooled `GameObject` (ochre, `sortingOrder` 22).

### `PathogenAgent`

- **`Initialize(..., CoarseCoord? lumenCellOverride = null, PathogenClass?
  classOverride = null)`** — optional trailing args. With no override,
  Sprint 4's random-depth upstream spawn and random class. Every existing
  caller keeps compiling.
- `Update()` early-returns while `RoundClock.Frozen`; passes
  `RoundClock.Time` to `SimulationTick`.

### `ImmunologyTD.Rounds.RoundScript` (new static)

- **`struct RoundDefinition { string Tagline; float VirusWeight,
  BacteriumWeight, LargeBacteriumWeight; PathogenClass RollClass(); }`** —
  `RollClass` normalises the weights; an all-zero mix returns
  `LargeBacterium` (no divide-by-zero).
- **`static RoundDefinition ForRound(int roundNumber)`** — ~6 hand-written
  gut-themed entries, then a procedural `"Spoiled leftovers, day N"`
  fallback with an even mix.

### `HudOverlay`

Round bar shows `rounds.CurrentTagline` (or `RoundScript.ForRound(next)`
during Building) and a "Time is frozen … Buy, then:" / "A contaminated
food item is delivering …" line. Box grew to 380×150.

### `EconomyTuning` / `InvasionTuning`

- `EconomyTuning.BatchSizeBase` 8 → **16**, `BatchSizeGrowthPerRound` 3 →
  **6**.
- `InvasionTuning.AdhesionChanceAtWall` 0.12 → **0.30**; new
  `FoodItemTransitSeconds` 30, `FoodItemBurstCount` 4,
  `FoodItemWallHugDepth` 1 (all in `ResetToDefaults`).

### Other `Update()` freeze gates

`SearchUnit`, `TissueDriver`, `BoneMarrowManager`, `AdaptiveDirector`,
`DendriticCell`, `Lymphocyte` all early-return while `RoundClock.Frozen`;
the first four pass `RoundClock.Time` where they passed `Time.time`.

### `Assets/Editor/RoundVerification.cs` (new)

`RoundVerification.RunAll` — **29 assertions**: `RoundClock` flag +
clock, `RoundScript` taglines / mix, the food-round delivery path through
the real `PathogenSpawner` / `RoundController`, and cells + pathogens
persisting across a boundary. The `Update()`-only freeze gate is left to
the build launch. `MapVerification` 4c repinned to `AdhesionChanceAtWall`
0.03.

## Sprint 10 changes — DC patrol lane-repulsion (`GAME_DESIGN.md` §5a note)

- **`DendriticCell.Initialize(...)`** gained an optional trailing
  `IReadOnlyList<DendriticCell> cohort` — the live fielded-DC list, for
  patrol lane-repulsion. `AdaptiveDirector.EmitDendriticCell` passes its
  `allDcs`.
- **`DendriticCell.RepelledPatrolStep`** (private) replaces the plain
  `Chemotaxis.ChooseNextStep` in `TickPatrol`: a random walk biased away
  from other DCs **along the cross axis only** (threat-axis steps stay
  unbiased). `TickTravel` / `TickInNode` / `TickReturn` unchanged.
- **`DendriticCell.DebugPlaceForTest(FineCoord)`** — new test seam (drop a
  patrolling DC on a chosen tile), same role as `TissueGrid.SeedHostState`.
- **`AdaptiveTuning`** — new `DcLaneRepelStrength` (1.4, softmax sharpness
  on the cross bias; 0 = plain random walk) and `DcLaneRepelAxisRange`
  (12, coarse cells along the threat axis within which another DC counts
  as crowding). Both in `ResetToDefaults`.
- `AdaptiveVerification` grew 3 assertions (34 → 37): an A/B on lane
  spread / shared-lane ticks with repulsion on vs. off.

## Sprint 11 changes — placeholder shop, knowledge ladder, inward regrowth

Framework pass — the shop and the ladder drive nothing; only regrowth is
a real change.

### `ImmunologyTD.Economy` — the shop

- **`enum ShopItem`** — `BarrierMucusTurnover`, `HostDsRnaSensor`,
  `HostReducedViralEntry`, `HostBacterialResistance`, `Crypt`.
- **`ShopLedger`** (plain reference type, per run) — `int LevelOf(ShopItem)`,
  `bool Owns(ShopItem)`, `int NextPrice(ShopItem)`, `bool CanBuy(ShopItem,
  AtpWallet)`, `bool TryBuy(ShopItem, AtpWallet)` (spends + increments;
  false and no change if unaffordable / null wallet), `void Reset()`,
  `int Revision`. **No effect beyond the ledger + wallet.**
- **`ShopTuning`** (mutable statics, `ResetToDefaults()`) — a base price
  per item, `PriceGrowthPerLevel` 0.6 (`PriceFor(item, level) = base ·
  (1 + growth·level)`), `ProgenitorUpgradeBasePrice` 35 /
  `ProgenitorUpgradePrice(level)`. All placeholder.

### `BoneMarrowManager` — per-tower upgrade

- `Slot.UpgradeLevel` (int). `OnSlotClicked` on a **placed** slot now
  opens an upgrade panel (`pendingUpgradeIndex`) instead of no-op.
- **`bool UpgradeTower(int index)`** — spends
  `ShopTuning.ProgenitorUpgradePrice(currentLevel)`, bumps the level.
  Refused on an empty slot / when broke. **Does not touch
  `UnitLifecycleTuning`** — §6d's real path is unchanged, just not called.
- **`int GetUpgradeLevel(int index)`**. Slot label shows "+N".

### `ImmunologyTD.Adaptive.KnowledgeLadder` + `KnowledgeCapability`

- **`enum KnowledgeCapability`** — `CytotoxicTCells`,
  `NeutralizingAntibodies`, `MemoryTCells`, `FcReceptor`, `Complement`,
  `SecretoryIgA`.
- **`KnowledgeLadder`** (static) — `readonly struct Rung { KnowledgeCapability
  Capability; float ThresholdPercent; string ShortName }`, `Rung[] Rungs`
  (ascending: 10/20/30/45/60/70), `bool IsUnlocked(cap, pct)`,
  `int UnlockedCount(pct)`, `IEnumerable<Rung> All()`. **Display-only.**

### `HudOverlay`

- `Bind(...)` gained a trailing optional `ShopLedger shop`.
- The KNOWLEDGE line is now a block: `BuildKnowledgeHeader()` +
  `BuildLadderLine(species, label)` per class (`% [x]CTL [x]NeutAb …`).
- **`DrawShopPanel()`** — left-side IMGUI panel, drawn only while
  `rounds.Phase == RoundPhase.Building`. Five `ShopItem` rows, priced,
  grey-out when broke, wired to `ShopLedger.TryBuy`. Debug panel height
  324 → 392.

### `TissueGrid` / `TissueTuning` — neighbour-accelerated regrowth (real)

- **`TissueTuning.NeighbourRegrowthBonus`** (new, 0.5; 0 = old behaviour).
- `TissueGrid.Tick`'s `Empty → Healthy` branch: `effectiveRegen =
  HostRegenerationSeconds / (1 + NeighbourRegrowthBonus ·
  HealthyNeighbourCount(c))` — a new private von-Neumann `Healthy`
  counter. Tissue heals inward from intact edges.

### `Assets/Editor/Sprint11Verification.cs` (new)

`Sprint11Verification.RunAll` — **26 assertions**: `ShopLedger` spend /
refuse / null-wallet / price scaling / `Reset`; `BoneMarrowManager.
UpgradeTower` placeholder (spends, levels, leaves `UnitLifecycleTuning`
untouched, refused when broke / on an empty slot); `KnowledgeLadder`
thresholds at the boundary, ordering, `UnlockedCount` monotonicity;
neighbour-regrowth A/B (surrounded 6.8s vs isolated 20.0s, and identical
at bonus 0). The OnGUI panels themselves are left to the build launch.
`TissueVerification`'s regrow sub-test pins `NeighbourRegrowthBonus` to 0.

## Verification harness (`Assets/Editor/MapVerification.cs`, new Sprint 4)

`MapVerification.RunAll` — 71 assertions over band layout, axis-frame
round-tripping, lumen flow and excretion, proximity-gated adhesion, the
breach burst, base-directed advance, and the reached-base event. Drives real
production classes; builds its own boards via `ConfigureForTest` and
**never loads the scene**, which is why it could not catch the stale
serialized `columns` bug.

## Open questions for whoever builds on this next

1. **~~Coarse `Row` vs. the four-compartment depth-5 model~~ — RESOLVED by
   Map 01's layout (Director, 2026-08-21).** The two axes are no longer in
   tension because depth is no longer a row. **Columns are the threat
   axis**: lumen on the right, tissue in the middle, base on the left, with
   pathogens advancing right→left. **Rows are lanes**, 40 of them, carrying
   no depth meaning at all. See `GAME_DESIGN.md` §1a. Nothing in the engine
   implements this yet — `BoardConfig` is still 30 columns × 5 rows with no
   band concept — so this is the next sprint's main structural work, but
   the ambiguity itself is settled.
2. **~~Does "adhesion" mean depth-1 specifically?~~ — RESOLVED by the same
   layout.** Adhesion is not a depth-1 event; it is a **lumen→tissue
   invasion** across the gut interface at the lumen/tissue seam. A pathogen
   riding the lumen flow either colonises that interface (`GAME_DESIGN.md`
   §6b barrier colonisation), jumps left into tissue, or is excreted out
   the bottom with no penalty. Scattering adhesion uniformly across the
   board — which is what Sprints 1–3 do — is exactly the behavior the
   Director rejected on 2026-08-21.
3. **~~Contact detection is coarse-slot-level~~ — RESOLVED in Sprint 3.**
   Contact is now a fine-tile proximity test (`ContactRadiusFineTiles`,
   Chebyshev, default 2) against the pathogen's own `Current` coordinate,
   so a unit at the far corner of a pathogen's coarse slot no longer
   damages it and the accidental stacking bonus is gone. What replaced the
   open question: **contact frequency fell to ~50% of the Sprint 2 rate**
   (macrophage 50.0%, neutrophil 49.2%, measured over 200k simulated
   ticks), so clearing is about half as fast per unit — and that landed in
   the same sprint as a population cap. Whether the two together tip the
   board toward the pathogens is a **balance question for playtest**, and
   the radius is the knob (per-tower, tunable). Do not "fix" it by
   reverting to coarse-slot detection or by tightening to an exact-tile
   test — see `SPRINT_PLAN.md` item 7.
4. **`Chemotaxis.GradientSharpness = 4f` is tuned for legibility, not
   balance.** The closing task's own verification numbers show units
   reaching an infected cell in ~4.5 simulated seconds on average once
   sensing is ON — dramatically faster than OFF (which doesn't reliably
   converge within a 2.5-minute simulated window on a 30-wide board). That
   gap is exactly what "should feel transformative" (`GAME_DESIGN.md`
   section 9) asks for, but it also means rung 2 alone, once purchased,
   could make round 1 feel closer to "solved" than "harder-but-manageable"
   once a real economy exists around it — a genuine balance question for
   whoever builds the round 2 buy panel and beyond, not something this
   fix tried to resolve. `CytokineVerification.RunFineGrainedSweep()` is
   there to re-derive this number once real balance criteria exist.
5. **Infection ramp timing (`InfectionRampSeconds = 20f` in
   `TissueGrid.cs`) and the heatmap's saturation reference
   (`MaxSecretionStrength`) were chosen for "visibly ramps up within a
   short playtest," not validated against any particular pacing target.**
   Fine for this sprint's legibility goal; worth a real look once round
   length/pacing is being tuned deliberately.
6. **~~Bone marrow emission has no population cap~~ — RESOLVED in Sprint 3.**
   This was the problem Sprint 3 existed to fix. Population is now bounded
   two independent ways per `GAME_DESIGN.md` §6d: a per-tower
   `MaxActiveChildren` ceiling (10) and the pre-existing
   `EmissionIntervalSeconds` rate cap (4s), with units despawning on
   kill-count depletion. Verified over 300 simulated seconds with all 5
   towers placed: active count never exceeded towers × cap at any point
   (peak 50 ≤ 50), against 375 unbounded. **Still genuinely open**: there
   is no end-of-round despawn, because `GAME_DESIGN.md` §2's round model
   still isn't built — whoever builds the round/economy layer decides
   whether cells persist across rounds or clear at the boundary. The cap
   makes that a design choice rather than a leak.
7. **(New, Sprint 2) All pathogen-class weights, combat numbers, and
   spread/emission timings are judgment calls, not balance-tested** —
   `PathogenAgent.VirusChance`/`BacteriumChance`, `IntracellularMaxHealth`/
   `LargeBacteriumMaxHealth`, `ContactDamagePerHit`, `IncubationSeconds`/
   `SpreadRetryIntervalSeconds`, `BoneMarrowManager.EmissionIntervalSeconds`,
   `GameBootstrap.BoneMarrowSlotCount`. Each is documented inline with the
   reasoning behind its specific value (mostly "legible within a short
   playtest," same standard as Sprint 1's `GradientSharpness`/
   `InfectionRampSeconds`) — see `docs/TEAM_RETRO.md` for the consolidated
   list. All are real candidates for revision once the ATP/economy layer
   exists and real balance criteria can be applied.
8. **(New, Sprint 2) `PathogenAgent`'s own sprite is fully hidden (not
   just recolored) for intracellular classes.** This means intracellular
   combat currently has **no per-hit visual feedback at all** — the flash
   that still works for `LargeBacterium` doesn't render for
   virus/bacterium infections, since their sprite is disabled. This was a
   deliberate trade-off to fix a worse problem (an accidental "tell" — see
   `docs/ENGINE_STATUS.md`/`docs/TEAM_RETRO.md`), but it does mean a
   player fighting an infected cell they can't see gets no confirmation
   they're making progress until it suddenly clears. If that reads as a
   real problem in playtesting, the fix is a flash driven by
   `BoardRenderer` (the coarse-cell background itself, the only thing
   actually visible for that slot) rather than by `PathogenAgent`'s own
   sprite — not attempted this sprint, flagged as a plausible next step.
9. **~~Do progenitor upgrades apply retroactively to living cells?~~ —
   ANSWERED (Director, 2026-08-21): yes, instantly.** An upgrade applies
   to every one of that progenitor's currently-fielded children as well as
   its future ones, because spending ATP should make an immediate visible
   difference. A `SearchUnit` therefore holds a **live reference** to its
   tower's `UnitLifecycleTuning`, not a snapshot — writing
   `manager.GetTuning(i).KillLimit = n` is immediately visible to all of
   tower `i`'s live units. Verified in `LifecycleVerification`: the change
   reaches every live child of that tower, reaches its future emissions,
   and touches neither another tower's children nor the shared
   `UnitProfile` default. Do not reintroduce snapshot semantics.
   results** — `MaxActiveChildren` 10, neutrophil `KillLimit` 5,
   `DegranulationBurstMultiplier` 3, `ContactRadiusFineTiles` 2. Only the
   macrophage `KillLimit` of 20 is Director-confirmed. All are fields on
   `UnitProfile`/`UnitLifecycleTuning`, never consts, so tuning them
   requires no code restructuring.
11. **(New, Sprint 4) Cytokine sensing's range no longer matches the board.**
    `CytokineField` is `strength / (1 + distance)` with no cutoff. That was
    a steep gradient across a 30×5 board and is nearly flat across Map 01's
    ~47-cell average separation, so sensing went from converging to zero
    within a minute to merely trending downward over 2.5 minutes. Measured
    numbers in `ENGINE_STATUS.md`. Not a code regression and deliberately
    not tuned (mechanics first), but Sprint 1 built the whole upgrade ladder
    on sensing feeling transformative, so this needs an answer before that
    ladder is monetised.
12. **(New, Sprint 4) Band sizes clamp inward, concentrating any shortfall
    on the tissue band.** If the axis is too short to hold base + lumen, the
    tissue band silently becomes zero cells and the game is unplayable while
    appearing fine. Guarded at runtime now, but the clamping behavior itself
    is unchanged — consider whether bands should be proportions rather than
    absolute cell counts when a second map exists.
13. **~~Nothing renders host-cell state, because there isn't any.~~ —
    RESOLVED in Sprint 5.** `TissueGrid` is now two layers (host +
    occupant), `HostState` is `{Empty, Healthy, Infected, Dead}`, and
    `BoardRenderer.HostStateColor` draws the four distinctly. See
    **Occupancy state** and **Sprint 5 changes** above.
14. **(New, Sprint 5) Viral spread is a one-shot chain, not a front.**
    `PathogenAgent.hasSpread` means each infected cell infects exactly one
    neighbour, ever, so an infection snakes through tissue rather than
    saturating outward. It matches `CombatVerification`'s "chains across
    generations" and the firebreak still emerges, but whoever tunes viral
    behaviour should decide whether a real front (multiple simultaneous
    spreads, or dropping `hasSpread`) is wanted. Logged in `BACKLOG.md`.
15. **(New, Sprint 5) A 1-cell dead gap is hoppable; ≥2 cells / a full
    lane is a hard wall.** The firebreak is emergent: a spread event can
    land a transient free virus particle *on* a single dead cell, which
    then steps to the healthy cell on the far side before its 6s survival
    timer expires. Consistent with `GAME_DESIGN.md` §1a's "slipping past
    one or two cells is allowed and occasional." `TissueVerification`
    tests the firebreak with a 3-cell band.
16. **(New, Sprint 5) Every Sprint 5 number is an unvalidated default.**
    `TissueTuning` (`HostCellMaxHealth` 10, `HostRegenerationSeconds` 20,
    `DebrisSelfDissipationSeconds` 60), `InvasionTuning`'s class-advance
    knobs, and `UnitProfile.EfferocytosisDebrisPerTick` 0.05 for the
    macrophage. All mutable, all grouped for a tuning pass; mechanics-first
    per the Director's standing instruction.
17. **(New, Sprint 6) The innate stress-sense chance and every §4b number
    are unvalidated.** `StressSenseChancePerTick` (macrophage 0.03,
    neutrophil 0.02), the eight new `InvasionTuning` virus/bacterium knobs.
    The stress-sense chance in particular is the dial the whole
    innate↔adaptive bridge turns on — too high and a macrophage-wall
    trivialises intracellular infection before the stress sensors exist;
    too low and it reads as "nothing works." Needs the Director's
    playtest.
18. **(New, Sprint 6) Budding vs. contact-chain is a per-spawn coin flip,
    not a species roster.** `VirusBuddingSpeciesChance` 0.5 is rolled per
    agent, so a budding infection's established virions independently
    re-roll (some of its children snake). Fine as "no roster yet"; when a
    pathogen-species system lands this becomes a species trait. Any harness
    test that watches a virus over time must force this (and
    `VirusBurnoutChance`) to 0 or 1 for determinism.
19. **(New, Sprint 6) `GetAttackableAt` occupant-only changed the
    degranulation and kill-attribution contracts.** A neutrophil's
    degranulation now reaches an infected cell via `DamageHostCell` rather
    than `ReceiveDamage` on the resident; kill attribution for an
    intracellular infection goes through `SearchUnit.CheckStressSense` →
    `KillHostCell` → `OnHostCellDestroyed` (which credits nobody unless the
    stress-sense path called `RegisterKill` on the sensing unit — it does).
    Any future code that assumed "hit the pathogen, get the kill" for an
    intracellular target needs the stress-sense path instead.
20. **(New, Sprint 7) Every economy/round number is a placeholder.**
    `EconomyTuning` — prices, lump sum, per-kill, starting ATP, life pool,
    regen cadence, batch size curve. The Director asked for the framework,
    not the balance. First real tuning pass follows his playtest of the
    loop.
21. **(New, Sprint 7) Round-complete deliberately ignores the gut wall.**
    `PathogenSpawner.BatchComplete` is true when the batch is emitted and
    nothing is in the lumen or tissue — a wall pile is allowed to persist
    (§6b). Without this, one stuck adherer (~1%/roll breach chance) holds a
    round open for a minute+. If a later change makes the wall attackable
    or clearable, revisit whether a round should wait on it.
22. **(New, Sprint 7) The kill payout is a static hook.**
    `EconomyHooks.PayForKill` is process-global. Fine for a single-scene
    game (`GameBootstrap` sets it every Awake) and null-safe for harnesses,
    but a second concurrent board or a scene with two wallets would need it
    made an instance path.
23. **(New, Sprint 7) `RoundController.Tick` runs only while `Active`.**
    Nothing advances the round state during `Building` or `Defeat` — which
    is intended (spawning is paused; the game is over), but it means a
    future "buy timer" or a defeat animation has to be driven elsewhere.
    The life-pool baseline (`breachesCharged`) is snapshotted in
    `Initialize` from `tally.ReachedBase`, so a run restart must build a
    fresh `RoundController` (or the tally must be `Reset()`), not just flip
    the phase.
