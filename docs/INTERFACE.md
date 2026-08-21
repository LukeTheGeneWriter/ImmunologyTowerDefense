# Interface Contract (Engine ↔ UI)

Status: written during Sprint 1 alongside the search prototype; updated
during Sprint 1's closing task (2026-08-19 playtest fix) to reflect the
infected-cell / continuous-secretion rework of the cytokine system; updated
again during Sprint 2 (2026-08-19) to add bone marrow placement, the lymph
node placeholder, and pathogen classes/combat/viral spread. Everything
below reflects code that actually exists in `game/Assets/Scripts/` as of
this sprint — it is not aspirational. There is no UI session/agent yet to
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

- `bool IsSlotFree(CoarseCoord)`
- `bool TryAdhere(CoarseCoord, PathogenAgent, float currentTime)` — claims
  a slot atomically; false if already occupied. `currentTime` starts that
  slot's infection timer (see below) — real gameplay callers pass
  `Time.time`. Changed this sprint's closing task from a 2-arg to a 3-arg
  signature; the caller (`PathogenAgent.TryAdhereHere`) was updated.
- `void ReleaseSlot(CoarseCoord)` — clears occupancy and the slot's
  infection timer. Unused through Sprint 1; as of Sprint 2, called by
  `PathogenAgent.ClearFromCombat` whenever `ReceiveDamage` brings a
  pathogen's `Health` to zero, for all three pathogen classes (see
  Pathogens section below) — this is the "clears back to bare host tissue"
  half of `GAME_DESIGN.md` section 4a.
- `PathogenAgent GetPathogenAt(CoarseCoord)` — null if the slot is bare
  host tissue.
- `IEnumerable<CoarseCoord> AdheredCoords()` — bare occupancy, no
  secretion data. No longer used internally (see `InfectedSources` below)
  but kept public for callers that only care about occupancy.
- `int AdheredCount` — cheap O(1) counter.

There is currently no separate "host cell" object/state — a coarse slot is
implicitly host tissue whenever `GetPathogenAt` returns null. Host cell
health, damage, and fibrosis are out of scope this sprint (see
`SPRINT_PLAN.md`'s exclusion list) and are **not** represented anywhere in
this data model yet. When they land, expect `TissueGrid` to grow a real
per-slot occupant enum/struct rather than the current
null-means-host-tissue shortcut.

### Infected-cell / continuous secretion (added in the Sprint 1 closing task)

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
13. **(New, Sprint 4) Nothing renders host-cell state, because there isn't
    any.** Sprint 5 adds healthy/infected/dead and two-layer occupancy
    (`GAME_DESIGN.md` §1c), which is a `TissueGrid` rewrite — it still holds
    exactly one pathogen per coarse slot. Do not build on the current
    occupancy model expecting it to survive.
