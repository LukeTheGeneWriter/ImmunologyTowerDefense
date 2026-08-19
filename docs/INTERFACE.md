# Interface Contract (Engine ↔ UI)

Status: first real draft, written during Sprint 1 alongside the search
prototype. Everything below reflects code that actually exists in
`game/Assets/Scripts/` as of this sprint — it is not aspirational. There is
no UI session/agent yet to consume this contract (Sprint 1 has no UI beyond
a debug IMGUI overlay), so treat this as the engine side declaring its
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
- `bool TryAdhere(CoarseCoord, PathogenAgent)` — claims a slot atomically;
  false if already occupied.
- `void ReleaseSlot(CoarseCoord)` — exists for future use (kill/despawn);
  nothing calls it yet, since no combat/lifecycle system exists this
  sprint.
- `PathogenAgent GetPathogenAt(CoarseCoord)` — null if the slot is bare
  host tissue.
- `IEnumerable<CoarseCoord> AdheredCoords()`
- `int AdheredCount` — cheap O(1) counter, used by `PathogenSpawner` to
  know when to recompute the cytokine field without a full board scan.

There is currently no separate "host cell" object/state — a coarse slot is
implicitly host tissue whenever `GetPathogenAt` returns null. Host cell
health, damage, and fibrosis are out of scope this sprint (see
`SPRINT_PLAN.md`'s exclusion list) and are **not** represented anywhere in
this data model yet. When they land, expect `TissueGrid` to grow a real
per-slot occupant enum/struct rather than the current
null-means-host-tissue shortcut.

## Cytokine gradient (`ImmunologyTD.Grid.CytokineField`)

- `void Recompute(IEnumerable<CoarseCoord> sources)` — rebuilds the coarse
  field from scratch as an inverse-Manhattan-distance falloff from every
  source (`strength / (1 + distance)`, `strength = 10`). Called by
  `PathogenSpawner` only when `TissueGrid.AdheredCount` changes, not every
  frame.
- `float SampleFine(FineCoord)` — bilinear interpolation of the coarse
  field at a fine-grid position. This is what `SearchUnit` samples when
  cytokine sensing is on.

**Documented simplification:** `GAME_DESIGN.md` section 7's implementation
note calls for diffusion on the coarse grid, interpolated down to the fine
lattice, specifically to avoid diffusing across thousands of fine tiles per
tick. This is honored, but the coarse field itself is *not* a literal
diffusion PDE stepped every tick — it's a static distance-falloff field
recomputed from scratch on every adhesion event. For a mostly-static set of
sources (pathogens don't move once adhered) this reads the same as a
settled diffusion field and is far cheaper. If pathogens ever become
mobile after adhering (e.g. burrowing deeper over time), this shortcut
should be revisited — a falloff field recomputed from a moving source set
would still work, just less smoothly during the transition than a true
diffused field would.

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
  `profile.FineTilesPerTick` individual random-walk steps (uniform among
  in-bounds von Neumann neighbours when `CytokineToggle.Enabled` is false;
  weighted by `CytokineField.SampleFine` at each candidate when true).
  Visual position is a `Vector3.Lerp` between tick-start and tick-end world
  position, so grid logic doesn't produce steppy visuals (per
  `GAME_DESIGN.md` section 7's "sprites tween between coordinates").
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
  owns the `SpriteRenderer[,]` for the coarse-cell background quads,
  polls `TissueGrid` every 0.15s and recolors each cell: host tissue
  (`0.80, 0.62, 0.66` — eosin-ish pink) vs. adhered pathogen (`0.42, 0.12,
  0.16` — dark maroon). This is what makes "host-cell occupancy readable
  at a glance" (`SPRINT_PLAN.md` stopping point) actually true; no other
  system currently changes a coarse cell's visual state (no
  damage/fibrosis yet).
- **`HudOverlay`** (MonoBehaviour) — IMGUI (`OnGUI`) debug text: board
  size, unit counts/speeds, and the live cytokine-sensing toggle state.
  **Deliberately not `UnityEngine.UI`** — this project's package manifest
  doesn't include `com.unity.ugui` (Unity 6 split uGUI out to its own
  package), and adding a package needs network access and is normally an
  Editor-GUI/Director step (same constraint noted in `ENGINE_STATUS.md`
  for Steamworks). IMGUI needs nothing extra and is a reasonable fit for a
  debug overlay. If real UI ever needs uGUI's layout system, that's a
  network-requiring setup step to do consciously, not something to
  half-adopt via one HUD script.

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
