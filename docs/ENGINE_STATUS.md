# Engine Status

Rewritten at the end of every sprint, not just appended to. This version
reflects the state after **Sprint 5** (host-cell states, debris as terrain,
efferocytosis, and class-specific pathogen advance). Sprint 0's
engine/platform decision section is preserved below since it is still
accurate. Earlier sprints' histories are in `docs/CHANGELOG.md`; this file
only carries forward what is still true.

**Sprints 3, 4 and 5 were all implemented by dispatched Code agents that
were interrupted mid-task** (usage limit, usage limit, dropped network).
Sprint 3's agent committed working code but no docs; Sprint 4's committed
nothing (~1,600 lines of non-compiling tree); Sprint 5's committed items
1–2 and a design doc with good messages, plus most of item 5 sitting
uncommitted **but compiling and passing all prior harnesses** — the first
clean interrupted hand-off, because this sprint's brief said "commit after
each scope item." In every case the head session finished, verified, and
documented the work. Anything below that says "verified" was verified by
the head session directly, from actual command output — see "Build status
(Sprint 5)" and `docs/TEAM_RETRO.md`.

## Engine & platform decision

**Unity**, chosen 2026-08-18 over Godot and a web-canvas-first stack.
Rationale (full research in project chat history):

- Closest match to Bloons TD 6's own strategy (Steam-primary, web as public
  face, single codebase exporting to both).
- Full C# support in WebGL builds; mature first-party Steamworks
  integration.
- Unity's Runtime Fee controversy is resolved — canceled Sept 2024. Unity
  Personal (free) covers projects under $200k annual revenue/funding.

Tradeoff accepted knowingly: Unity's WebGL builds are heavier than Godot's
— acceptable given the above.

## Local dev environment

Unity Hub, the Unity Editor (`6000.5.8f1` at `C:\Program
Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe`), the Unity CLI, and an
activated Personal license are installed on the Director's machine. As of
`WORKFLOW.md`'s 2026-08-19 rewrite, this is where the head session and
dispatched Code agents run natively with real shell access — no device
bridge, no sandbox. **Still no interactive Editor GUI session used for
gameplay authoring** through Sprint 3 — everything is built via batchmode
CLI + code. See "Scene construction" below.

## Current state (post–Sprint 4)

### Scene

Still `Assets/Scenes/Sprint1.unity` — **not renamed this sprint**, a
deliberate scope call (see `docs/TEAM_RETRO.md`): the file's content is
just the single `GameBootstrap` object with default-value serialized
fields, all real gameplay is code-driven at runtime, so renaming it would
have been cosmetic-only and added a rebuild/re-verify cycle for zero
functional benefit. Flagging clearly here so nobody reads "Sprint1.unity"
as meaning the file is stale.

`Assets/Editor/SceneSetup.cs` and `BuildScript.cs` are unchanged from
Sprint 1 (still point at `Sprint1.unity`).

### Gameplay systems

Builds on Sprint 1's two-resolution lattice, cytokine field, and random/
biased-walk search. Sprint 2 additions, per `SPRINT_PLAN.md`'s three scope
items:

**1. Bone marrow placement (replaces Sprint 1's debug spawn).**
- **`Assets/Scripts/Units/BoneMarrowSlot.cs`** (new) — one clickable slot.
  Click detection is Unity's legacy `OnMouseDown` message via a
  `BoxCollider2D` + the main camera's physics raycast — no uGUI/
  EventSystem needed (still no `com.unity.ugui` package this project).
- **`Assets/Scripts/Units/BoneMarrowManager.cs`** (new) — owns
  `BoneMarrowSlotCount` (5, a judgment call — see `docs/TEAM_RETRO.md`)
  slots below the tissue board. An empty slot click opens an IMGUI
  two-button picker (Macrophage/Neutrophil); `PlaceTower(index, kind)` is
  free (no ATP cost, per `SPRINT_PLAN.md`) and starts a per-slot emission
  timer. `Tick(float deltaTime)` — not an implicit `Update()` reading
  `UnityEngine.Time` — is the real emission logic, matching the
  `TissueGrid`/`CytokineField`/`Chemotaxis` pattern of taking explicit time
  so a headless harness can drive it directly (see Verification below).
  `EmissionIntervalSeconds = 4f` is a judgment call, not a balanced number.
  A placed tower emits into tissue at a uniformly random column along the
  **blood-adjacent edge** (the deepest fine row, `board.FineRows - 1`) —
  rung-1 entry per `GAME_DESIGN.md` section 2a ("cells extravasate at
  random points along the vessel").
- **`GameBootstrap`** no longer spawns any units at startup. It builds the
  (empty) macrophage/neutrophil `PrefabPool`s and hands them to
  `BoneMarrowManager`, which is now the sole source of new units. Nothing
  is on the board until the player places at least one tower — an
  accepted, `GAME_DESIGN.md`-flagged consequence ("round 1 becomes
  buy-then-observe").

**2. Lymph node placeholder.**
- A labeled, reserved, non-functional compartment built directly in
  `GameBootstrap` (a background `SpriteRenderer` + a
  `Assets/Scripts/Rendering/CompartmentLabel.cs` text label), positioned to
  the right of the tissue board. No behavior — exactly what
  `SPRINT_PLAN.md` scoped ("visually reserved space... not a functional
  lymph node").
- **`CompartmentLabel.cs`** (new) — small reusable IMGUI label anchored to
  a world-space point via `Camera.main.WorldToScreenPoint`, used for both
  the lymph node's caption and the bone marrow strip's heading, to avoid
  writing near-identical `OnGUI` boilerplate twice.

**3. Pathogen classes + combat (`GAME_DESIGN.md` section 4a).**
- **`PathogenClass` enum** (new, in `PathogenAgent.cs`) —
  `IntracellularVirus`, `IntracellularBacterium`, `LargeBacterium`.
  Parasites (multi-slot footprint) remain out of scope, per
  `SPRINT_PLAN.md`. Assigned per adhesion event via weighted random
  (`VirusChance = 0.45`, `BacteriumChance = 0.25`, remainder
  `LargeBacterium` — a judgment call, see `docs/TEAM_RETRO.md`, weighted
  toward virus since spread is "the sprint's most important piece").
- **Rendering split** — `BoardRenderer.ShowsAsPathogenItself(PathogenAgent)`
  (new static, side-effect-free predicate, same extraction pattern as
  Sprint 1's `Chemotaxis.ChooseNextStep`) decides per-cell background
  color: `LargeBacterium` reads as itself (dark maroon); intracellular
  classes read as bare host tissue (pink). A `PathogenAgent`'s own small
  sprite is **disabled entirely** for intracellular classes (see "Notable
  bug found and fixed" below for why an earlier version that tinted it
  flat instead of hiding it was wrong) — the coarse-cell background
  (heat-tinted per the existing cytokine mechanism) is the *only*
  representation of an intracellular infection, exactly matching
  `GAME_DESIGN.md`'s "visible as the host cell, not itself, until sensed."
- **Combat** — `PathogenAgent.ReceiveDamage(float)` (new), called by
  `SearchUnit.CheckContact` every tick a unit's fine tile falls in an
  occupied coarse slot (`PathogenAgent.ContactDamagePerHit = 1f`, flat, per
  `SPRINT_PLAN.md`'s "keep damage numbers simple"). `MaxHealth` is 12 for
  both intracellular classes, 18 for `LargeBacterium` (judgment calls —
  see `docs/TEAM_RETRO.md`). Reaching zero calls `TissueGrid.ReleaseSlot`
  (unused since Sprint 1, now exercised) and returns the pathogen to its
  pool via the existing `onExit` callback path — the same mechanism
  Sprint 1's transit-and-exit case already used.
- **Viral spread** — `PathogenAgent.TickCombat(float currentTime)` (new,
  explicit-time like the rest of this sprint's testable surfaces) checks,
  once `IncubationSeconds` (15f, judgment call) have elapsed on an
  uncleared virus infection, whether to spread to a free adjacent coarse
  slot via `PathogenSpawner.RequestSpread(CoarseCoord, float)` (new,
  public — shuffles the four coarse von Neumann neighbours and spawns a
  child `PathogenAgent` directly into `Adhered` state via
  `PathogenAgent.InitializeAdheredDirect`, through the same
  `PrefabPool` as ordinary spawns). Retries every `SpreadRetryIntervalSeconds`
  (1f) if blocked, rather than giving up permanently. Bacterial
  intracellular infections and large bacteria never spread (virus-specific
  per `GAME_DESIGN.md`). Verified headlessly to chain across multiple
  generations (see Verification below) — this is what gives search speed
  its "compounding cost."

### Sprint 3 additions — unit lifecycle and population homeostasis

The problem Sprint 3 exists to fix: Sprint 2's towers emitted forever and
**no `SearchUnit` was ever despawned** — `PrefabPool.Release` was never
called for one, so there was no return path at all. Active unit count grew
without bound. Most of this sprint is building that path and then bounding
it. Full design rationale in `GAME_DESIGN.md` §6d.

**1. `Assets/Scripts/Units/UnitLifecycleTuning.cs` (new) — the
parameterization layer.** A plain reference type holding
`MaxActiveChildren`, `KillLimit`, `DegranulatesOnDepletion`,
`DegranulationBurstMultiplier`, `ContactRadiusFineTiles`. **Nothing here is
a `const`, deliberately** — the Director's ruling (2026-08-21) is that
progenitor upgrades will eventually sell "bump this tower's kill count," so
an upgrade must be a write to one tower's field and nothing else.
Per-*kind* defaults live on `UnitProfile` (serialized fields, not consts);
`BoneMarrowManager.PlaceTower` copies them into a fresh instance per tower
via `FromProfile`. Verified: two towers of the same kind get genuinely
independent instances, and upgrading one touches neither the other tower
nor the shared `UnitProfile` default.

**2. Per-tower population cap (`BoneMarrowManager`).** Each slot keeps a
`List<SearchUnit> Children` (a list, not a counter, so the HUD and harness
can reach them; bounded by the cap so the O(n) remove is trivially cheap)
and stops emitting at `MaxActiveChildren` (default 10) even when its timer
has elapsed. **The emission timer is clamped at the interval while blocked
rather than banking up** — that detail is what keeps the two caps genuinely
independent per §6d: a tower whose whole population dies at once refills at
one cell per 4s instead of dumping ~40s of owed emissions in one frame.
Verified explicitly (1 cell immediately after a mass-death event, still 1
at 3.875s, exactly 2 at 7.875s, back to cap only after ~40s).

**3. Depletion and the despawn path (`SearchUnit`).** New public,
harness-callable surface: `Kills`, `RegisterKill()`, `IsDepletionDue`,
`ResolveDepletionIfDue()`, `ResetForPool()`, `OwnerSlotIndex`, plus
`SimulationTick()`/`CheckContact()` which read no `UnityEngine.Time` — the
project's standing convention (`TissueGrid`/`CytokineField`/`Chemotaxis`)
that anything worth verifying takes time/state explicitly.
- **Neutrophil (`KillLimit` 5, `DegranulatesOnDepletion` true)** —
  degranulates: a `ContactDamagePerHit × DegranulationBurstMultiplier` (3×)
  burst to whatever occupies its own coarse slot, applied as ordinary
  combat damage, then despawns. A `depleting` guard stops a kill landed by
  the burst itself from recursing into a second depletion. Per
  `SPRINT_PLAN.md` item 3, bare host tissue takes nothing — there is no
  host-cell-health/fibrosis system yet, and that is the intended state.
- **Macrophage (`KillLimit` 20 — Director-confirmed 2026-08-21,
  `DegranulatesOnDepletion` false)** — quiet retirement, no collateral
  damage. Verified: an occupant at the retirement site is still at full 18
  HP afterwards.
- Both free their tower's population slot and return to the `PrefabPool`
  (verified inactive, not destroyed), with kill count cleared for reuse.
- **A unit holds a live reference to its tower's tuning** (Director,
  2026-08-21), so an upgrade applies instantly to that tower's
  already-fielded children as well as its future ones. Sprint 3 first
  shipped a value snapshot (future children only); the Director overruled
  it — an ATP purchase should change the board immediately. Isolation still
  holds: no cross-tower leakage, and the shared `UnitProfile` defaults are
  never mutated.

**4. Kill attribution (`PathogenAgent.ReceiveDamage(float, SearchUnit)`).**
Signature changed to carry the attacker. **Exactly one** unit is ever
credited — whoever's hit crosses zero; earlier hits credit nothing and
later same-tick hits no-op at the `State == Cleared` guard. Credit is
applied *before* `ClearFromCombat()`, since clearing can return the
pathogen to its pool. A `null` source stays legal and always will (viral
spread, collateral damage, harness fixtures) and simply means nobody is
credited.

**5. Contact detection: coarse slot → fine-tile proximity.** Resolves
`INTERFACE.md` open question 3. A unit now damages a pathogen only within
`ContactRadiusFineTiles` (default 2, **Chebyshev**) of the pathogen's own
stored fine coordinate, instead of anywhere in its 7×7 coarse slot.
Chebyshev over Manhattan because it matches the square footprints these
units already render as; the Manhattan diamond covers half as many tiles
and would have halved contact frequency again. **Deliberately a radius, not
an exact-tile test** — exact coincidence across 49 fine tiles would have
made a random-walking unit almost never connect, i.e. combat would stop
working. See "Contact-rate change" under Build status for the measured
cost, which is real and matters for balance.

**6. `Assets/Scripts/Rendering/DegranulationFlash.cs` (new, pooled).** A
0.45s pale-yellow expanding burst at the degranulation site — the visible
difference between the two depletion paths, so a depleting neutrophil reads
as an event rather than a unit silently vanishing. Pooled like everything
else per `GAME_DESIGN.md` §8.

**7. HUD (`HudOverlay`, `BoneMarrowManager.OnGUI`).** Total active units
shown against the theoretical ceiling, and each marrow slot label shows
"N/cap alive". Added so the Director can *watch* population stay bounded
rather than be asked to trust it — this sprint's headline claim is a
number, and now that number is on screen.

### Sprint 4 additions — Map 01 geometry and the invasion loop

Implements `GAME_DESIGN.md` §1a/§1b. This is the sprint that gave the game
a real map: three lateral bands with the threat axis running right-to-left,
replacing Sprints 1–3's single tissue-only board where pathogens adhered at
a uniformly random column.

**1. `BoardConfig` — an axis frame and data-driven bands.** `Rows` stopped
being a `const 5` and the `[Range(24,40)]` clamp on columns is gone; Map 01
is **100 × 40 coarse cells**, still 7×7 fine subdivision.

The important part is the **axis frame**, which is how
`SPRINT_PLAN.md` item 3's "advance toward the base, never leftward"
requirement is actually enforced:
- `ThreatAxis` (Horizontal/Vertical) and `BaseEnd` (Negative/Positive) are
  configuration.
- `AxisIndex(coord)` returns **distance from the base**, flipping
  internally when `BaseEnd` is Positive. Axis index 0 is always the
  outermost base cell whichever world side that is.
- `CoarseFromAxis`, `OffsetInAxisFrame`, `InAxisBounds`, `CrossIndex`,
  `CrossLength` complete the frame. `OffsetInAxisFrame(c, -1, 0)` is the
  *only* sanctioned way to step toward the base.
- Bands are cell counts (`BaseBandCells` 25, `LumenBandCells` 25,
  `TissueBandCells` derived), with `BandAtAxisIndex`/`BandOf` classifying,
  and named edges: `TissueBaseEdgeAxisIndex`, `TissueLumenEdgeAxisIndex`,
  `LumenNearWallAxisIndex`, `LumenDepthFromInterface`.
- Lumen flow direction is its own axis end (`FlowCrossStep`,
  `LumenEntryCrossIndex`, `IsExcretedCrossIndex`).
- `ConfigureForTest(...)` builds non-default geometry from code; it exists
  so `MapVerification` can prove the abstraction on a mirrored board.

**2. `GutInterface` (new) — the wall, and the burst.** One pile per lane
(`PositionCount == CrossLength`). `Adhere` adds to a position; `Remove`
handles a pathogen cleared while still on the wall; `AdheredCountAt`,
`AdheredAt`, `TotalAdhered`, `PeakAdhered` expose pressure.
- `BreachChanceAt(position)` is `1 - (1 - perPathogen)^n`, so **a position's
  breach odds rise with the pile on it** — pressure builds toward its own
  release rather than being memoryless.
- `Tick(deltaTime, currentTime)` advances a roll clock and rolls every
  *occupied* position when due. The clock is held at zero while the wall is
  clean so the first pathogen to adhere waits a full interval.
- `Breach(position, currentTime)` releases **every** pathogen at that
  position in one call — the mechanic `SPRINT_PLAN.md` item 6 insists on. A
  pathogen that cannot find a release slot stays on the wall rather than
  being dropped. Public, so a harness (or a future "pop the abscess"
  ability) can trip a position deterministically.
- A `Breached(position, count)` **event** rather than polling, because
  script execution order between `PathogenSpawner` (which ticks this) and a
  renderer's `Update()` is undefined; polling would silently drop about half
  the bursts, and an undrawn burst is the one thing item 6 says must be
  legible.

**3. `PathogenAgent` — largely rewritten.** States are now `Lumen`,
`AtInterface`, `InTissue`, `Cleared`.
- **Lumen:** enters at the upstream end of the flow at a random distance
  from the wall, steps along the cross axis every
  `LumenStepIntervalSeconds`, and is **excreted with no penalty** off the
  downstream end. Unattackable while flowing.
- **Adhesion:** after each step, one roll against
  `AdhesionChanceAt(depth)` = `AdhesionChanceAtWall * exp(-depth / falloff)`
  — a static, side-effect-free function, the same extraction pattern as
  `Chemotaxis.ChooseNextStep`. On success `AdhereToInterface(position)`
  **moves the pathogen to the wall**, per §1b step 1.
- **Tissue:** `StepTissue` picks among toward-base / two lateral / away
  candidates by weight (0.70 / 0.13 each / 0.04), consulting the axis frame
  only. Reaching a `Base`-band cell despawns the pathogen and increments
  `InvasionTally.ReachedBase`.
- `InitializeAdheredDirect` became `InitializeInTissueDirect` — "adhered"
  now means the gut wall, not tissue.

**4. `InvasionTuning` (new) — every invasion number in one place**, all
mutable statics with `ResetToDefaults()`: lumen/tissue step intervals,
`AdhesionChanceAtWall` 0.12, `AdhesionFalloffCells` 5, breach roll cadence
1s, `PerPathogenBreachChance` 0.012, release spread limits, advance weights.

**5. `InvasionTally` (new)** — `Adhesions`, `Breaches`,
`ReleasedIntoTissue`, `Excreted`, `ReachedBase`. Drives the HUD.

**6. Compartments moved into the base band**, and units now enter at
`TissueBaseEdgeAxisIndex` on a random lane — expressed in the axis frame, so
moving the base moves the entry line with it. Placement UX is unchanged.

**7. `CytokineField` is now allocation-free.** Sprints 1–3 allocated a fresh
`float[,]` plus a source `List` every recompute — 150 floats on the old
board, 4,000 on Map 01, ~2.5 allocations/second of steadily growing garbage
against `GAME_DESIGN.md` §8. Both buffers are owned and cleared in place.

**8. HUD** shows the band layout, pathogen counts by band, the invasion
tally including `REACHED BASE`, and a live frame-cost readout. A dimming
panel sits behind it because the base band is now underneath the text.

### Sprint 5 additions — host-cell states, debris, class-specific advance

Implements `GAME_DESIGN.md` §1c and §1b step 4. Full signature-level detail
is in `docs/INTERFACE.md` ("Occupancy state" and "Sprint 5 changes").

**1. `TissueGrid` is two independent layers per coarse position.** A host
layer (`enum HostState { Empty, Healthy, Infected, Dead }`, plus per-cell
health, debris amount, intracellular pathogen ref, infection start time)
and an occupant layer for extracellular pathogens. Sprints 1–4's
one-`PathogenAgent`-per-slot model is gone. The tissue band seeds full of
`Healthy`; base/lumen are permanently `Empty`. Cytokine secretion sources
are now `Infected` hosts **or** occupied occupant slots (earliest start
wins) — dropping the occupant sources would have deleted every
`LargeBacterium` from the field in a sprint told to keep the mechanism
working.

**2. Debris is terrain.** `KillHostCell` (the single chokepoint every death
funnels through) → `Dead` + `FullDebris` (`1f`). Debris blocks regrowth
(only `Empty` ground regrows). `TissueGrid.Tick(dt, now)` — driven by a new
three-line `TissueDriver` MonoBehaviour, **not** the pathogen spawner —
owns debris self-dissipation (`DebrisSelfDissipationSeconds` 60) and
regrowth (`HostRegenerationSeconds` 20). `TissueTuning` holds the numbers
(mutable statics, `ResetToDefaults()`).

**3. Efferocytosis.** `SearchUnit.CheckEfferocytosis(currentTime)` — a unit
with `EfferocytosisDebrisPerTick > 0` (a per-tower field on
`UnitProfile`/`UnitLifecycleTuning`, macrophage default `0.05` ≈ 2.5s per
full pile, neutrophil `0`) standing on a `Dead` cell eats one bite of
debris via `TissueGrid.ClearDebris`. Opportunistic — own coarse slot only.
A finished pile plays a calm blue-green `DegranulationFlash`
(`EfferocytosisColor`). `SearchUnit.SimulationTick()` grew a `currentTime`
parameter (only caller is `Update()`).

**4. Class-specific advance** (`PathogenAgent.StepInTissue` dispatches by
class):
- **Virus** — free particles step only onto `Healthy` hosts and die after
  `VirusFreeSurvivalSeconds` (6s) if homeless; intracellular ones are
  stationary (spread is `TickCombat`'s job). The **firebreak is emergent**
  from those two rules plus `PathogenSpawner.RequestSpread` now requiring
  `IsHealthyHost` — no firebreak check exists in the code.
- **Intracellular bacterium** — base-biased walk when exposed, enters a
  `Healthy` cell it stands on (`IntracellularEntryChance` 0.5), hides
  `IntracellularResidenceSeconds` (12s), lyses out killing the cell and
  keeps walking.
- **Large bacterium** — unchanged walk, grazes its host cell for
  `LargeBacteriumHostDamagePerStep` (2.5) each step.
- Rendering is now driven by `PathogenAgent.IsIntracellular`, not `Class`,
  so an intracellular bacterium out of a cell and a free virus particle
  are both drawn as themselves.

**5. Base-band layout** (`GameBootstrap.BuildLayout`) rebuilt as fractions
of the base band's world rect — the marrow strip and lymph backdrop were
still sized for 100×40 and spilled across the 25×10 board.

### Notable bug found and fixed in Sprint 2: `PrefabPool` didn't initialize outside Play Mode

`PrefabPool.Awake()` builds the underlying `ObjectPool<GameObject>`. The
assumption (mine, going in) that Unity calls `Awake()` synchronously on
`AddComponent()` even outside Play Mode turned out to be wrong — it only
fires reliably once the player loop is actually running (Play Mode, or a
real build). A headless verification harness that `AddComponent`s a
`PrefabPool` and calls `Get()` directly hit this as a
`NullReferenceException`. Fixed by making pool construction lazy
(`EnsurePool()`, called from `Awake()` **and** from `Get()`/`Release()`) —
harmless in normal gameplay (`Awake()` already ran by the time anything
calls `Get()`), and it's what let `Assets/Editor/CombatVerification.cs`
drive the real `PathogenSpawner`/pool path headlessly. See
`docs/TEAM_RETRO.md` for the full story.

### Notable bug found and fixed in Sprint 2: camera under-zoom from a stale `Camera.aspect`

`GameBootstrap.BuildCamera` computes `orthographicSize` from
`Camera.aspect` at `Awake()` time (frame 0), before the actual runtime
window/render-target size has necessarily settled. In this sprint's first
real-build screenshot the right ~25% of the board (and the entire lymph
node compartment) was cropped out of frame. Fixed by refitting the camera
a second time one frame later via a coroutine
(`RefitCameraNextFrame`) — `FitCamera` is now a separate, reusable method
called once immediately (a reasonable frame-0 fallback) and again after
`yield return null`. **Turned out not to be the actual cause of the
cropping seen in the first screenshot** (see Build verification below —
that was a DPI-scaling mismatch in the screenshot *capture tooling*, not
the game), but the fix is real, cheap, and strictly more correct than
relying on a single frame-0 aspect read, so it's kept.

## Build status (Sprint 5)

All run by the **head session**, resuming after the dispatched agent's
network dropped mid-item-5. Numbers copied from actual output.

### Headless verification

| Harness | Result |
|---|---|
| `TissueVerification.RunAll` (**new**, Sprint 5) | **53 passed, 0 failed** |
| `MapVerification.RunAll` (Sprint 4) | 71 passed, 0 failed |
| `LifecycleVerification.RunAll` (Sprint 3) | 79 passed, 0 failed |
| `CombatVerification.RunAll` (Sprint 2) | 36 passed, 0 failed |

**239 assertions, 0 failed**, on a clean working tree after every Sprint 5
commit.

`TissueVerification` covers two-layer occupancy (a `Healthy` host and an
extracellular occupant at one coord at once), death→debris on every kill
path, debris-as-terrain (blocks regrowth / `ClearDebris` unblocks /
self-dissipation ~20× slower than a macrophage), efferocytosis through the
real `SearchUnit` path, the viral firebreak, and class-specific advance.
Two groups carry most of the weight:

- **The firebreak.** Tested at the level of the *rule*, not by measuring
  how far a random walk penetrates (viral spread is a one-shot chain, so
  it snakes rather than saturates): (a) a virus with a `Healthy` neighbour
  spreads into it; (b) a virus ringed by dead/infected ground never
  spreads across 10 post-incubation attempts and does **not** burn its
  one-shot `hasSpread`; (c) over 60 incubation cycles against a 3-cell
  full-lane dead band, no infection ever appears on the base side; (d) a
  homeless free virus dies after `VirusFreeSurvivalSeconds` leaving no new
  debris. No code path checks for a firebreak — it emerges from those
  local rules.
- **Intracellular bacterium in/out.** Enters a `Healthy` cell it stands on,
  hides (sprite off, `Infected`), lyses out after the residence window
  killing the cell + leaving debris, and is back on the occupant layer
  **alive and walking** — the assertion that caught the self-kill bug
  (`KillHostCell` was notifying the very pathogen that called it).

Two bugs `TissueVerification` caught while being written, both in the
resumed item-5 code: the intracellular-bacterium self-kill on lysis (fixed
with `ReleaseIntracellular` before `KillHostCell`), and
`PathogenSpawner.RequestSpread` spreading onto non-`Healthy` ground (fixed
with an `IsHealthyHost` check — latent since Sprint 2). Details in
`docs/TEAM_RETRO.md`.

### Sprint 4 verification (unchanged, re-run green)

`MapVerification` covers band layout and boundaries, axis-frame
round-tripping, lumen flow and excretion, proximity-gated adhesion, the
breach burst, base-directed advance, and the reached-base event. Two groups
carry most of the weight:

- **The burst.** Six pathogens piled at one wall position; one `Breach`
  call releases all six into the tissue band, empties that position, fires
  **one** event carrying the count, and leaves a second pile at another
  position completely untouched. Asserted directly because item 6 warns
  that degrading this into a trickle destroys the mechanic.
- **The mirrored map.** The same advance code runs on a board configured
  with the base on the opposite end. The pathogen still closes on the base
  in the axis frame, which *there* means its world column **increases**.
  That is the real test of item 3 — advance follows configuration, not a
  hardcoded direction.

Adhesion proximity is proven end-to-end rather than by re-deriving the
curve: cohorts of 400 pathogens run the full channel with a wall-only
falloff versus a depth-blind one, and the depth-blind curve adheres several
times as many. Closing the gate entirely (`AdhesionChanceAtWall = 0`) yields
exactly zero adhesions, proving the gate is actually wired into the
production path.

`CombatVerification` gained one assertion (35 → 36): its "units enter at the
blood-adjacent deepest fine row" check tested a contract item 8 retired, and
was rewritten in the axis frame as two checks.

### Cytokine sensing is much weaker at map scale — measured

| Board | OFF (0–1m / 1–2m / 2–2.5m) | ON |
|---|---|---|
| 30×5 (Sprints 1–3) | 2.99 / 3.14 / 2.84 | 0.20 / 0.00 / 0.00 |
| 100×40 (Map 01) | 46.93 / 46.83 / 47.05 | 45.29 / 40.42 / 37.38 |

**Not a regression.** The mechanism works — ON closes steadily while OFF
stays flat — but it no longer converges within the window. `CytokineField`
computes `strength / (1 + distance)` with no cutoff: a steep gradient across
the old board's ~3-cell separations, nearly flat across Map 01's ~47-cell
average. Sprint 1 called this mechanic "should feel transformative"; at map
scale, right now, it isn't. **Deliberately not tuned** — the Director's
standing instruction is mechanics first. Logged in `BACKLOG.md`.
**Sprint 5 note:** not re-measured — no cytokine code changed — and the
table above still stands.

### Windows build (Sprint 5)

`BuildScript.BuildWindows()` — **Succeeded, 93,317,440 bytes, 0 errors.**
Launched headlessly for ~18s: **0 exceptions / 0 errors** in `Player.log`.
Bootstrap diagnostic confirms the 25×10 board (base axis 0–5, tissue 6–18,
lumen 19–24) and the **fixed base-band layout**: `BoneMarrowSlot[0]` at
world y 4.60 (top of the base band, inside it), `LymphNode` at y −3.47,
with a clean gap between the marrow strip's bottom edge and the lymph
backdrop's top — no overlap, nothing spilling into the tissue band.
Frame cost not re-measured (still vsync-capped, unchanged renderer path).

### What was NOT verified

- **Nobody has watched the firebreak happen**, or a macrophage clear a
  debris pile, or an intracellular bacterium duck in and lyse back out.
  The 53-assertion harness proves the mechanics; the *sight* of a viral
  front stalling against dead ground, with no one staging it, is the
  question this sprint exists to answer and it is the Director's playtest.
- **The four host-state colours have not been eyeballed in a build** —
  `BoardRenderer.HostStateColor` returns four distinct values (asserted),
  but "can the Director tell them apart at a glance" (SPRINT_PLAN item 3)
  is a screenshot/playtest question.
- **Placement / clicking was not exercised through the running build's
  UI**, same as Sprints 3–4: scripted clicks can't take foreground focus
  while the machine is in use. Unchanged code path.
- **WebGL** not re-verified (unchanged since Sprint 1).

## Known issues

- **(New, Sprint 4) A stale serialized field silently deleted the
  playfield — fixed, but the class of bug is live.** `Sprint1.unity` still
  carried Sprint 1's `columns: 30`, which beat Map 01's `columns = 100`
  default, and because the outer bands clamp against axis length the whole
  shortfall hit the middle: 25 base + 5 lumen + **0 tissue**. It ran, drew,
  and threw nothing. `GameBootstrap.WarnOnDegenerateBands` now logs an
  error, but note **`MapVerification` cannot catch this class of bug** — it
  builds boards via `ConfigureForTest` and never loads the scene.
- **(New, Sprint 4) Cytokine sensing is much weaker at map scale.** See
  Build status. Measured, not tuned, per the mechanics-first instruction.
- **(New, Sprint 4) Frame cost is vsync-capped and therefore unmeasured.**
  8.35 ms is the refresh interval, not necessarily the cost.
- **(Sprint 4, partly addressed) The base band's marrow strip / lymph
  backdrop no longer overlap or spill** (Sprint 5 item 6 — `BuildLayout`
  rebuilt as fractions of the base band rect). The HUD dimming panel and
  its overprint against the marrow labels is **not** touched — still wants
  a real layout / Design-agent pass.
- **(New, Sprint 4) All invasion numbers are unvalidated defaults** —
  `InvasionTuning`'s adhesion chance/falloff, breach cadence and
  per-pathogen chance, release spread limits, and advance weights. Grouped
  in one file with `ResetToDefaults()` precisely so a tuning pass is cheap.
- **(New, Sprint 5) All Sprint 5 numbers are unvalidated defaults too** —
  `TissueTuning` (host health 10, regrowth 20s, self-dissipation 60s),
  `InvasionTuning`'s four class-advance knobs (`VirusFreeSurvivalSeconds`
  6, `IntracellularEntryChance` 0.5, `IntracellularResidenceSeconds` 12,
  `LargeBacteriumHostDamagePerStep` 2.5), and the macrophage's
  `EfferocytosisDebrisPerTick` 0.05. All mutable, all grouped.
- **(New, Sprint 5) Viral spread is a one-shot chain, not a front.**
  `hasSpread` per cell → an infection snakes rather than saturates. The
  firebreak still emerges and the design ("chains across generations")
  arguably intends this, but flagged for whoever tunes viral behaviour.
  Logged in `BACKLOG.md`.
- **(New, Sprint 5) A 1-cell dead gap is hoppable** by a transient free
  virus particle; ≥2 cells / a full lane is a hard wall. Consistent with
  §1a's "slipping past one or two cells." `TissueVerification` uses a
  3-cell band.
- Scene is still named `Sprint1.unity` — cosmetic only. It carries real
  serialized state that matters (`columns: 25`, the 5 partially-serialized
  `UnitProfile` fields — see `docs/TEAM_RETRO.md`).
- `Application.runInBackground` is unchecked (Unity default) — the game
  pauses when its window loses focus. Not a bug, but it means scripted
  input/screenshot verification needs the machine otherwise idle.
- `UnityEngine.UI` is still not installed — HUD, marrow picker and
  compartment labels are IMGUI.
- WebGL not re-verified (unchanged since Sprint 1).
- Sprint 3's contact-rate reduction (~50% of Sprint 2's) and all Sprint 3
  lifecycle numbers are unchanged and still unvalidated.
- `Chemotaxis.GradientSharpness = 4f` and the infection-ramp constants in
  `TissueGrid.cs` are unchanged from Sprint 1 — and `GradientSharpness` is
  now a candidate lever for the cytokine-range problem above.

## Addendum: what batchmode CLI can and can't do (still true)

No interactive Editor GUI access this sprint either. Everything built via
runtime code (`GameBootstrap`) or Editor scripts driven by
`-executeMethod` in batchmode. One correction to Sprint 1's own note on
this topic: **`MonoBehaviour.Awake()` is not guaranteed to fire just from
`AddComponent()` outside Play Mode** (see the `PrefabPool` bug above) —
Sprint 1's closing task got lucky that none of its dummy `GameObject`s
needed `Awake()`-driven initialization. Any future headless harness that
needs a component's `Awake()`-built state should either call an explicit
init method directly or use a lazy-init guard like `PrefabPool.EnsurePool()`
now does, not assume `AddComponent` alone is enough.
