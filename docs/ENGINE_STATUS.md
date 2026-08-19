# Engine Status

Rewritten at the end of every sprint by the Code session, not just appended
to. This version reflects the state after Sprint 2 (bone marrow placement,
lymph node placeholder, pathogen classes + viral spread). Sprint 0's
engine/platform decision section is preserved below since it's still
accurate; the "current state" and "build status" sections reflect Sprint 2.
Sprint 1's own history (including its closing cytokine-sensing fix) is in
`docs/CHANGELOG.md`; this file only carries forward what's still true.

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
gameplay authoring** through Sprint 2 — everything is built via batchmode
CLI + code. See "Scene construction" below.

## Current state (post–Sprint 2)

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

### Notable bug found and fixed this sprint: `PrefabPool` didn't initialize outside Play Mode

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

### Notable bug found and fixed this sprint: camera under-zoom from a stale `Camera.aspect`

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

## Build status (Sprint 2)

### Headless verification (`Assets/Editor/CombatVerification.cs`, new)

Same philosophy as Sprint 1 closing task's `CytokineVerification.cs` —
drives the actual production classes (`TissueGrid`, `PathogenAgent`,
`PathogenSpawner`, `BoardRenderer.ShowsAsPathogenItself`,
`BoneMarrowManager`), not a reimplementation, with no play mode and no
rendering. Run via `Unity.exe -batchmode -quit -projectPath <path>
-executeMethod CombatVerification.RunAll`.

**35/35 assertions passed.** Four groups:

1. **Damage → clear → slot release**, for all three pathogen classes:
   confirms occupancy before the last hit, slot-free + `GetPathogenAt ==
   null` + `onExit` fired exactly once after the last hit, for
   `IntracellularVirus` (12 HP), `IntracellularBacterium` (12 HP), and
   `LargeBacterium` (18 HP).
2. **Render classification**: `BoardRenderer.ShowsAsPathogenItself` is
   false for both intracellular classes and for a bare slot (`null`), true
   for `LargeBacterium`.
3. **Viral spread timing** — the flagship result. Three scenarios, all
   driving the real `PathogenSpawner.RequestSpread`:
   - Left uncleared: `AdheredCount` stays 1 right up to incubation, then
     grows past 1 once incubation elapses, and reaches **3** (one origin +
     two chain-spread children) within 43 simulated seconds — confirms
     the spread isn't a one-shot event but chains across generations, each
     with its own independent incubation timer, which is what
     `GAME_DESIGN.md` section 4a's "compounding cost" language actually
     means.
   - Cleared (via `ReceiveDamage`) partway through incubation:
     `AdheredCount` stays 0 for the rest of the simulated window — a
     cleared infection genuinely cannot spread, the direct proof that
     "fast search beats spread."
   - `IntracellularBacterium`, ticked 45 simulated seconds (3x the virus
     incubation window): never spreads.
4. **Bone marrow emission**: a slot starts `Empty`; `PlaceTower` sets it to
   `Placed`; `EmittedCount` is 0 before any `Tick`, reaches ≥2 after 10
   simulated seconds (`EmissionIntervalSeconds = 4`); the last emitted
   unit's row is exactly `board.FineRows - 1` (the blood-adjacent edge)
   and its column is in bounds; placing on an already-placed slot is a
   no-op.

**Regression check**: re-ran `CytokineVerification.RunComparison` (Sprint
1's closing-task harness, untouched this sprint) — numbers are
**identical** to the values recorded in the Sprint 1 closing task (OFF:
2.99/3.14/2.84, ON: 0.20/0.00/0.00 across the three time buckets). The
cytokine-sensing mechanism did not regress from this sprint's changes.

### Real build + runtime verification

`BuildScript.BuildWindows()` succeeded three times over the course of this
sprint (iterating on the two bugs above), final result: `Succeeded, size:
93289832 bytes, errors: 0`.

Launched the built `.exe` and captured window contents via `PrintWindow`
(flag `2`, per Sprint 1's tip). **Important tooling correction made this
sprint**: the first screenshot attempt used `GetWindowRect` from a
PowerShell process that was *not* DPI-aware, which on this machine (a
150%-scaled display) returns window coordinates in a different scale than
Unity's actual render resolution (`Screen.width`/`height` = 2560×1600, vs.
the non-DPI-aware rect's 1707×1067 — exactly a 1.5x mismatch). This
produced a screenshot that looked like a cropped board with no lymph node
visible, which was initially (and incorrectly) diagnosed as a camera
framing bug. Calling `SetProcessDPIAware()` in the capture script before
`GetWindowRect` fixed the capture to match Unity's real resolution, at
which point **the full 30-column board and the lymph node compartment were
both visible and correctly positioned** — the camera math had been correct
the whole time. (The camera refit-next-frame fix above is still kept, on
its own merits, but wasn't the fix for this particular symptom.) Worth
carrying forward: **on a scaled display, screenshot/click automation
against a Windows app needs `SetProcessDPIAware()` before any
`GetWindowRect`/`SetCursorPos` call**, or physical-pixel math will be
wrong by the scale factor.

Confirmed visually, across a sequence of screenshots over one running
session (see `docs/TEAM_RETRO.md` for the full narrative, including that
part of this session's evidence came from the Director apparently
interacting with the live window himself):

- **Bone marrow compartment**: 5 slots below the tissue board, labeled
  "Bone Marrow — click an empty slot to place a tower." Clicking an empty
  slot (verified with programmatically computed screen coordinates,
  derived from a logged camera-position/orthographic-size/aspect
  diagnostic — not eyeballed) opens the two-button picker; clicking
  "Neutrophil" placed a tower, changing that slot's label and color
  immediately. Two other slots were already showing "Macrophage tower" /
  "Neutrophil tower" from earlier interaction.
- **Lymph node compartment**: visible to the right of the tissue board,
  labeled "Lymph Node (reserved — not functional yet)."
- **Units actively spawning and moving**: successive screenshots a few
  seconds apart (with the window focused — see below) show materially
  different unit positions and a growing unit count, consistent with
  multiple placed towers each emitting on their own ~4s timer and units
  then random/cytokine-walking.
- **Pathogen class rendering**: small dark-maroon squares (large bacteria,
  visible as themselves) scattered on an otherwise uniformly
  host-pink-colored board — no visible "tell" for intracellular
  infections beyond the existing heatmap tint, confirming the
  hide-the-small-sprite fix (see above) actually fixed the visual bug it
  was meant to fix.
- **Cytokine sensing / heatmap**: toggle read "ON" during this session
  (again, likely toggled by the Director) with heat-tinted cells visible,
  consistent with Sprint 1's unchanged mechanism.

**One behavior worth knowing for future automated verification, not a
bug**: the built game does **not** tick while its window lacks OS
foreground focus — two screenshots taken several tool-calls apart with no
focus in between were pixel-identical, and resumed changing the moment
focus was re-established via the `AttachThreadInput`/`SetForegroundWindow`
trick. `Application.runInBackground` is at its Unity default (unchecked).
Not changed this sprint — flagged as a real consideration for anyone doing
further scripted verification, and possibly worth revisiting when a real
playtest build is being prepared (an idle/alt-tabbed player might be
surprised the game paused).

**Not re-verified this sprint**: WebGL (same as Sprint 1 — the brief
prioritizes Windows-target verification).

### What was verified vs. what needs the Director's own eyes

Verified directly: the pooled damage/clear/spread mechanics are correct
against the real production code (headless, 35/35); the cytokine mechanism
didn't regress (identical numbers to Sprint 1); the build compiles and
runs without exceptions; bone marrow placement (click → picker → placed
tower → periodic emission at the blood edge) works end-to-end in a real
running build, confirmed with programmatically computed click coordinates,
not just visual inspection; the lymph node placeholder renders and is
labeled; intracellular pathogens do not visually reveal themselves outside
the heatmap.

**Not verified from this session, and can't be**: whether placement
"feels like a real decision" or combat "gives the search loop a
satisfying payoff" — `SPRINT_PLAN.md`'s own framing of what this sprint
answers for the Director is explicitly a felt-experience question, not a
mechanism-correctness one. Also not captured in a screenshot this
session: an actual live viral-spread event (a second infected cell
appearing next to an existing one) — the 15s incubation period didn't
line up with this session's screenshot windows, though the headless
harness proves the mechanism triggers and chains correctly. Worth the
Director specifically watching for it in his own play session (leave
cytokine sensing OFF and don't rush to clear an infection — it should
visibly spread within about 15–20 seconds).

## Known issues

- Scene is still named `Sprint1.unity` (see "Scene" above) — cosmetic
  only, not stale content.
- `Application.runInBackground` is unchecked (Unity default) — the game
  pauses when its window loses focus. Not a bug, just worth knowing before
  assuming a "frozen" build has crashed.
- No multi-depth pathogen descent (still out of scope — adhesion row is
  still chosen randomly at spawn, unchanged from Sprint 1).
- No host cell health/fibrosis as a tracked numeric system — contact
  damage exists now (this sprint), but there's no fibrosis/scarring
  consequence layered on top yet (explicitly out of scope, see
  `SPRINT_PLAN.md`).
- `UnityEngine.UI` is still not installed — bone marrow's picker and the
  compartment labels are IMGUI, same reasoning as Sprint 1's HUD.
- WebGL not re-verified this sprint (same as Sprint 1).
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
