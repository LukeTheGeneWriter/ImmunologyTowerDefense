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
"Collision" is detected as the unit's current tile falling in the same
*coarse* slot as an adhered pathogen (`SearchUnit.CheckContact`, called
once per tick after the unit's fine-tiles-per-tick steps resolve).

**Sprint 2 change: contact now deals real damage, not just a flash.**
`CheckContact` calls `pathogen?.ReceiveDamage(PathogenAgent.ContactDamagePerHit)`
(see Pathogens section below) instead of Sprint 1's flash-only
`NotifyContact()`. The 0.25s color-flash-toward-a-highlight-color visual is
still there (now inside `ReceiveDamage`), but reaching zero health now has
a real, visible, persistent consequence: the slot clears and the pathogen
returns to its pool. Every unit in the same coarse slot as an occupied
slot deals damage every tick it's there, so multiple units clear a
pathogen faster — not a designed stacking mechanic, just a natural
consequence of the detection being coarse-slot-level (see open question 3
below, unchanged from Sprint 1).

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
  - **`ReceiveDamage(float amount)`** (new) — flat per-hit damage
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

## Open questions for whoever builds on this next

1. **Coarse `Row` vs. the four-compartment depth-5 model.** This sprint's
   `CoarseCoord.Row` (`0..4`) is a tissue-internal coarse band, not
   `GAME_DESIGN.md` section 1's depth-5 blood/breach axis. When bone
   marrow/blood/lymph node get built, something has to reconcile "tissue
   row 0..4" with "compartment depth 0..5" — they're not currently the
   same numbering and nothing enforces a relationship between them.
2. **Does "adhesion" mean depth-1 specifically?** See the pathogen section
   above — this sprint scatters adhesion across all coarse rows for search
   variety, which may not match the Director's mental model of "adhering
   to the mucus layer."
3. **Contact detection is coarse-slot-level, not fine-tile-level — and now
   deals real damage, not just a flash.** Still true as of Sprint 2:
   `SearchUnit.CheckContact` fires (and now calls
   `PathogenAgent.ReceiveDamage`) once per tick a unit's fine tile falls in
   the same coarse slot as an occupied one, not the pathogen's exact fine
   tile. A consequence worth flagging now that damage is real: **every**
   unit sharing that coarse slot deals damage that tick, so a crowded
   coarse cell clears noticeably faster than a lone unit would — not a
   designed stacking bonus, just what the coarse-level detection produces
   once contact has a real effect. `PathogenAgent`'s stored `Current` fine
   coordinate still has what's needed to tighten this to fine-tile-level
   if that stacking behavior ever needs fixing.
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
6. **(New, Sprint 2) Bone marrow emission has no population cap beyond
   `PathogenSpawner.maxLivePathogens`-style throttling on the unit side —
   there isn't one.** Each placed tower emits every
   `BoneMarrowManager.EmissionIntervalSeconds` (4s) indefinitely; with all
   5 slots filled that's a steadily growing standing unit population,
   since nothing despawns units (no "cells die at end of round" lifecycle
   exists yet — `GAME_DESIGN.md` section 2's round model isn't built).
   Not a bug this sprint (there's no round loop for it to violate yet),
   but whoever builds the round/economy layer should expect to add either
   a cap or an end-of-round despawn here.
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
