# Interface Contract (Engine ↔ UI)

Status: written during Sprint 1 alongside the search prototype; updated
during Sprint 1's closing task (2026-08-19 playtest fix — see
`SPRINT_PLAN.md`'s "Closing task" section) to reflect the infected-cell /
continuous-secretion rework of the cytokine system. Everything below
reflects code that actually exists in `game/Assets/Scripts/` as of this
sprint — it is not aspirational. There is no UI session/agent yet to
consume this contract (Sprint 1 has no UI beyond a debug IMGUI overlay), so
treat this as the engine side declaring its shapes ahead of need. Update
this file whenever any of the below changes; per `WORKFLOW.md` that's a
cross-team event even though "cross-team" is currently just "future Design
agent."

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
- `void ReleaseSlot(CoarseCoord)` — exists for future use (kill/despawn);
  nothing calls it yet, since no combat/lifecycle system exists this
  sprint. Also clears the slot's infection timer.
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
host cells — a pathogen does **not** block unit movement this sprint.
"Collision" is detected as the unit's current tile falling in the same
*coarse* slot as an adhered pathogen (`SearchUnit.CheckContact`, called
once per tick after the unit's fine-tiles-per-tick steps resolve), which
triggers `PathogenAgent.NotifyContact()` — a 0.25s color flash toward a
bright highlight color, visible in a build. There is no damage, no kill, no
removal; `GAME_DESIGN.md`'s "collide by collision, not by sight" is
satisfied at the detection level, but there is nothing yet for a
"collision" to *do* beyond the flash, since combat is out of scope. When
combat lands, expect this to become the hook point (contact already fires
reliably; it just doesn't do anything consequential yet).

## Pathogens (`ImmunologyTD.Pathogens`)

- **`PathogenState`** — `enum { Transiting, Adhered, Cleared }`.
- **`PathogenAgent`** (MonoBehaviour) — `Initialize(BoardConfig, TissueGrid,
  Action<PathogenAgent> onExit)`. Spawns at fine column 0 on a random row,
  transits rightward at a fixed 2 fine-tiles/tick, and either (a) adheres
  (~70% chance) at a randomly chosen target column, preferring its
  originally-picked row but falling back to the nearest free row in the
  same column if that's taken, retrying one coarse-cell further right if
  the whole column is full; or (b) transits straight across and exits
  (~30% chance), calling `onExit` so `PathogenSpawner` can release it back
  to the pool. `NotifyContact()` is the hook `SearchUnit` calls on contact
  (see above). `ResetForPool()` clears state before a release-case
  `PrefabPool.Release` call; adhered pathogens are never released this
  sprint (no despawn/kill system exists yet) so it's currently only
  exercised by the transit-and-exit path.
- **`PathogenSpawner`** (MonoBehaviour) — `Initialize(BoardConfig,
  TissueGrid, CytokineField, GameObject pathogenTemplate)`. Owns the
  `PrefabPool` for pathogens, spawns on a timer (`spawnIntervalSeconds =
  2.5`, capped at `maxLivePathogens = 40` counting both transiting and
  permanently-adhered pathogens — this cap is what throttles spawning once
  the board fills up, since adhered pathogens never leave the `live`
  list).

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

Unchanged from Sprint 0 except one addition: **`public void
SetPrefab(GameObject)`**. Sprint 0's `PrefabPool` only supported assigning
`prefab` via the Inspector's serialized field, which assumes an interactive
Editor session laying out the scene by hand. Sprint 1's whole scene is
built from code at runtime (`GameBootstrap`, see below) with no
hand-authored prefab assets — `SetPrefab` lets a pool be wired up
programmatically. Safe to call any time before the first `Get()` (the
pool's `createFunc` closure reads the `prefab` field lazily, not at
construction time), so call order relative to `AddComponent<PrefabPool>()`
doesn't matter.

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
  (gained the `CytokineField` parameter this sprint). Owns the
  `SpriteRenderer[,]` for the coarse-cell background quads, polls
  `TissueGrid`/`CytokineField` every 0.15s and recolors each cell: host
  tissue (`0.80, 0.62, 0.66` — eosin-ish pink) vs. adhered pathogen
  (`0.42, 0.12, 0.16` — dark maroon), then blends in a warm tint
  (`1.00, 0.55, 0.05`) proportional to `CytokineField.CoarseValueAt(coord)
  / TissueGrid.MaxSecretionStrength`, up to 65% blend at full strength.
  This is what makes "host-cell occupancy readable at a glance"
  (`SPRINT_PLAN.md` stopping point) actually true, plus, as of the Sprint 1
  closing task, what makes the cytokine field itself readable at a
  glance — the visual cue the Director's playtest found missing. **The
  heatmap tint is deliberately independent of `CytokineToggle`** — it's
  always visible, since the field itself always exists in the fiction
  (cytokines are secreted regardless of whether a given cell type can
  sense them yet, per `GAME_DESIGN.md` section 2a). Only unit movement
  responds to the toggle. This is intentional: it lets the Director watch
  a hot cell (always visible) and a unit's response to it (only when
  sensing is ON) side by side, rather than the heatmap itself encoding
  on/off state.
- **`HudOverlay`** (MonoBehaviour) — IMGUI (`OnGUI`) debug text: board
  size, unit counts/speeds, and the live cytokine-sensing toggle state, plus
  (added this sprint) a one-line explainer of the heatmap tint so the
  Director doesn't have to infer what the new orange coloring means.
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

## Scene construction (`ImmunologyTD.Bootstrap.GameBootstrap`)

The entire Sprint 1 scene (`Assets/Scenes/Sprint1.unity`) is a single
`GameObject` named `GameBootstrap`, carrying the `GameBootstrap` and
(via `[RequireComponent]`) `BoardConfig` components. Everything else —
camera, host-cell grid quads, unit/pathogen pools and their templates,
spawns, HUD — is built at runtime in `GameBootstrap.Awake()`. This was a
practical choice, not a stylistic one: this sprint was built without an
interactive Unity Editor session (no way to drag prefabs into an Inspector
field or hand-place GameObjects), so hand-authoring scene YAML was the only
alternative, and that's far more failure-prone than letting Unity's own
`GameObject`/`AddComponent` API build the scene from a script. See
`Assets/Editor/SceneSetup.cs` — an Editor script, run via `-executeMethod
SceneSetup.RebuildSprint1Scene` in batchmode — which creates the single
bootstrap object and saves the scene. `BuildScript.EnsureSceneExists()`
also knows how to recreate this same single-object scene from scratch if
the `.unity` file is ever missing, as a fallback.

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
3. **Contact detection is coarse-slot-level, not fine-tile-level.** If
   future gameplay (e.g. an actual kill) wants tighter precision (unit must
   be on the pathogen's exact fine tile, not just anywhere in its coarse
   slot), `SearchUnit.CheckContact` and `PathogenAgent`'s stored `Current`
   fine coordinate already have what's needed to tighten this — it's a
   small change, just not made this sprint because the looser
   coarse-slot-level version reads better as "found it" for a legibility
   test.
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
