# Engine Status

Rewritten at the end of every sprint, not just appended to. This version
reflects the state after **Sprint 3** (per-progenitor population cap, unit
depletion/despawn, kill attribution, fine-tile proximity contact). Sprint
0's engine/platform decision section is preserved below since it's still
accurate. Sprint 1's and Sprint 2's own histories are in
`docs/CHANGELOG.md`; this file only carries forward what's still true.

**Sprint 3 was implemented by a dispatched Code agent that hit its usage
limit partway through**, after committing working, verified code
(`8eaca14`) and a successful build, but before writing any of the docs. The
head session ran the verification itself and wrote this file, `INTERFACE.md`,
`TEAM_RETRO.md`, and `CHANGELOG.md` afterwards. Anything below that says
"verified" was verified by the head session directly, from actual command
output — see "Build status (Sprint 3)" for what that did and did not cover.

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

## Current state (post–Sprint 3)

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
- **A unit gets a value snapshot of its tower's tuning at emission time**,
  not a live reference, so a mid-round upgrade improves that tower's
  *future* children and not the ones already fielded (`SPRINT_PLAN.md` item
  5 — flagged there as a judgment call; making it retroactive is a one-line
  change, hand out `slot.Tuning` directly instead of a snapshot).

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

## Build status (Sprint 3)

All of the following was run by the **head session**, not the Code agent,
after that agent hit its usage limit. Numbers below are copied from actual
command output.

### Headless verification

**`Assets/Editor/LifecycleVerification.cs` (new) — 76 passed, 0 failed.**
Drives the real production classes (`BoneMarrowManager`, `SearchUnit`,
`PathogenAgent`, `UnitLifecycleTuning`, `TissueGrid`) with no Play Mode and
no rendering, same philosophy as Sprints 1–2. Run with:

```
Unity.exe -batchmode -quit -projectPath <repo>\game -executeMethod LifecycleVerification.RunAll
```

Groups, with the results that actually matter:
1. **Max active children** — a tower saturates at 10 after 500 simulated
   seconds and emits exactly 10 units total, where uncapped Sprint 2
   behavior would have emitted **125**; depleting one child frees exactly
   one slot; the tower refills and holds at the cap through another 125s.
2. **Emission-rate cap after mass death** — the independence proof above
   (1 cell immediately, 1 at 3.875s, 2 at 7.875s, cap only after ~40s).
3. **Neutrophil degranulation** — fires at exactly 5 kills, not 4; deals a
   flat 3× burst (an 18 HP occupant → 15); a lethal burst clears the slot
   and `GetPathogenAt` returns null; degranulating on bare tissue is
   harmless and still despawns; the slot is freed and the unit returns to
   the pool inactive with its kill count cleared.
4. **Macrophage retirement** — not due at 19/20, retires at 20, occupant
   still at full 18 HP (no collateral), slot freed, pooled not destroyed.
5. **Kill attribution** — after 11 non-lethal hits from unit A, nobody is
   credited; the unit whose hit crosses zero (B) gets the kill and A gets
   nothing (no split credit); extra same-tick hits credit nobody; a `null`
   source still clears the pathogen with no exception.
6. **Proximity contact** — radius default 2; a unit on the pathogen's own
   tile and one at exactly Chebyshev 2 both connect; a unit at the far
   corner of the *same coarse slot* does **not** (asserted to still be in
   that coarse slot, so this is genuinely a proximity test and not an
   out-of-slot test); neighbouring-slot centre does not connect.
7. **Per-tower independence** — two towers of a kind get separate tuning
   instances; upgrading tower 0 leaves tower 1's cap, tower 1's kill limit,
   and the shared `UnitProfile` default untouched; tower 0 then caps at its
   own new max while tower 1 still caps at 10; a mid-life upgrade does not
   retroactively change an already-fielded unit but the tower's next
   emission does carry it.
8. **Long-run boundedness — the point of the sprint.** 5 towers, 300
   simulated seconds of churn: active count **never exceeded** towers × cap
   at any point (peak 50 ≤ 50), ended at 50, against Sprint 2's unbounded
   375 over the same window — while towers kept genuinely producing
   throughout (283 emitted vs. a 50-unit initial fill).

### Regressions — both clean

- **`CombatVerification.RunAll` (Sprint 2): 35 passed, 0 failed**, with its
  `ReceiveDamage` call sites updated for the new signature.
- **`CytokineVerification.RunComparison` (Sprint 1): numbers identical to
  the Sprint 1 and Sprint 2 recordings** — OFF 2.99 / 3.14 / 2.84, ON 0.20
  / 0.00 / 0.00. Movement and cytokine sensing did not regress.

### Contact-rate change — a real balance consequence, not a rounding error

`LifecycleVerification` prints a no-assertion diagnostic for exactly this,
because it is the thing most likely to bite in playtest:

- 25 of the 49 tiles in a coarse slot are within Chebyshev radius 2 of its
  centre (51.0%).
- Over 200,000 simulated ticks starting from the pathogen's tile, the
  macrophage lands **280 hits where the Sprint 2 rule would have landed 560
  (50.0%)**; the neutrophil lands **612 vs. 1245 (49.2%)**.

**Clearing is therefore roughly half as fast per unit as in Sprint 2**, and
that lands at the same time as a population cap. If pathogens now outpace
the player, this interaction is the first place to look — `SPRINT_PLAN.md`
item 7 anticipated it and asked for it to be reported rather than papered
over by quietly re-tuning other numbers. Nothing else was re-tuned.

### Real build + runtime

`BuildScript.BuildWindows()` — **Succeeded, size 93,295,368 bytes, errors:
0** (Sprint 2 was 93,289,832). Note that Unity's incremental player build
left the on-disk artifacts from the Code agent's own earlier build of the
same commit untouched; the build genuinely ran and reported success.

Launched the built `.exe` and captured it with a DPI-aware `PrintWindow`
script: **0 exceptions in `Player.log`**, board renders (30×5, lymph node
compartment, 5 empty marrow slots), pathogens visible, and the new HUD line
reads `Active units: 0 (no towers placed yet)`. Board state changed between
successive captures, so the simulation is genuinely running.

### What was NOT verified, and why

**Placement was not exercised through the real build's UI this session.**
Scripted clicks did not land: `SetForegroundWindow` was refused because the
Director was actively using another window at the time — Windows declines
the steal in exactly that case, so this is a scheduling problem, not an
environmental limit. Because `Application.runInBackground` is off, an
unfocused build does not tick — two captures 75s apart came back
pixel-identical, which is the tell. The click/picker path itself is
unchanged code that Sprint 2 verified end-to-end with computed coordinates,
and `LifecycleVerification` drives the real `PlaceTower`/`Tick`/emission
path headlessly, so the mechanism is covered — but "click a slot in the
running game and watch the counter stop at 10" is **the Director's to
confirm**, and it is the first thing worth doing in the playtest.

Also unverified, deliberately: whether degranulation *reads* as
intentionally different from a quiet retirement. The flash exists and is
distinct in code; whether it lands as an event or as noise is a
felt-experience question, which is exactly what `SPRINT_PLAN.md` says this
sprint asks the Director.

**Not re-verified this sprint**: WebGL (same as Sprints 1–2 — the brief
prioritizes the Windows target).

## Known issues

- **(New, Sprint 3) Clearing is ~50% slower per unit than in Sprint 2**, as
  a direct and intended consequence of proximity contact — measured, see
  "Contact-rate change" above. Arriving simultaneously with a population
  cap, this is the most likely source of "the board feels like it's losing
  ground." Tune `ContactRadiusFineTiles` (per-tower, default 2) first; do
  not revert to coarse-slot detection.
- **(New, Sprint 3) Placement was not exercised through the running build's
  UI this sprint** — scripted clicks could not take foreground focus. See
  "What was NOT verified" above. Unchanged code path, verified headlessly
  and in Sprint 2's own session, but it is unconfirmed for this build.
- **(New, Sprint 3) A mid-round tower upgrade would not affect already-
  fielded units** (they hold a value snapshot of their tower's tuning).
  Deliberate, flagged in `SPRINT_PLAN.md` item 5, and awaiting a Director
  ruling — no upgrade system exists yet, so nothing depends on it today.
- **(New, Sprint 3) Every Sprint 3 number is a tuning default, not a
  balance result** — `MaxActiveChildren` 10, neutrophil `KillLimit` 5,
  macrophage `KillLimit` 20 (the one Director-confirmed value),
  `DegranulationBurstMultiplier` 3, `ContactRadiusFineTiles` 2. All live on
  `UnitProfile`/`UnitLifecycleTuning` as fields precisely so they can move.
- Scene is still named `Sprint1.unity` (see "Scene" above) — cosmetic
  only, not stale content. Unchanged through Sprint 3 for the same reason.
- `Application.runInBackground` is unchecked (Unity default) — the game
  pauses when its window loses focus. Not a bug, just worth knowing before
  assuming a "frozen" build has crashed.
- No multi-depth pathogen descent (still out of scope — adhesion row is
  still chosen randomly at spawn, unchanged from Sprint 1).
- No host cell health/fibrosis as a tracked numeric system — contact
  damage exists since Sprint 2, but there's no fibrosis/scarring
  consequence layered on top yet (explicitly out of scope, see
  `SPRINT_PLAN.md`).
- `UnityEngine.UI` is still not installed — bone marrow's picker and the
  compartment labels are IMGUI, same reasoning as Sprint 1's HUD.
- WebGL not re-verified this sprint (same as Sprints 1-2).
- Pathogen class weights (`VirusChance`/`BacteriumChance`), combat numbers
  (`ContactDamagePerHit`, `MaxHealth` per class), spread timing
  (`IncubationSeconds`, `SpreadRetryIntervalSeconds`), and bone marrow
  numbers (`BoneMarrowSlotCount`, `EmissionIntervalSeconds`) are all
  judgment calls tuned for legibility within a short playtest, not
  validated against balance — see `docs/TEAM_RETRO.md` for the reasoning
  behind each, and expect all of them to be revisited once the ATP/economy
  layer exists.
- `Chemotaxis.GradientSharpness = 4f` and the infection-ramp constants in
  `TissueGrid.cs` are unchanged from Sprint 1 — same caveats as before.

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
