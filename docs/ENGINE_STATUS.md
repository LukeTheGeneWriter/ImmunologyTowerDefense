# Engine Status

Rewritten at the end of every sprint by the Code session, not just appended
to. This version reflects the state after Sprint 1 (the search-problem
prototype) plus Sprint 1's closing task (2026-08-19 — the cytokine-sensing
legibility fix; see `SPRINT_PLAN.md`'s "Closing task" section and
`docs/INTERFACE.md` for the full data-shape changes). Sprint 0's
engine/platform decision section is preserved below since it's still
accurate; the "current state" and "build status" sections reflect the
state after the closing task, not just the original Sprint 1 drop.

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
activated Personal license are installed on the Director's machine
("lukesdecoder"). As of `WORKFLOW.md`'s 2026-08-19 rewrite, this is where
the head session and dispatched Code agents run natively with real shell
access — no device bridge, no sandbox. **No interactive Editor GUI session
has been used for actual gameplay authoring through Sprint 1** — everything
has been built via batchmode CLI + code, not by hand-placing GameObjects or
dragging assets in the Editor window. See "Scene construction" below; this
is a real constraint that shaped several implementation choices, not
incidental.

## Current state (post–Sprint 1)

### Scene

`Assets/Scenes/Sprint1.unity` (renamed from Sprint 0's `Sprint0.unity`,
which has been deleted). Registered in `EditorBuildSettings`. Contains a
single `GameObject` ("GameBootstrap") carrying `GameBootstrap` and
`BoardConfig`; everything else in the running game — camera, board visual,
unit/pathogen pools and instances, HUD — is constructed at runtime in
`GameBootstrap.Awake()`. See `docs/INTERFACE.md`'s "Scene construction"
section for why (short version: no interactive Editor session was
available to hand-author a scene, so the scene builds itself from code
instead).

`Assets/Editor/SceneSetup.cs` is the Editor-only script that creates and
saves this scene (`SceneSetup.RebuildSprint1Scene`, also exposed as an
Editor menu item `ImmunologyTD/Rebuild Sprint 1 Scene`). `BuildScript.cs`
also knows how to recreate the same minimal scene from scratch as a
fallback if the `.unity` file is ever missing.

### Gameplay systems (all new this sprint)

Implements `SPRINT_PLAN.md`'s Sprint 1 scope — the two-resolution lattice,
host-cell occupancy, pathogen adhesion, two-unit-type random walk, and the
cytokine-sensing debug toggle. Full data-shape documentation lives in
`docs/INTERFACE.md`; summary:

- **`Assets/Scripts/Grid/`** — `CoarseCoord`, `FineCoord`, `BoardConfig`
  (board width is an Inspector field on the bootstrap object, `Range(24,
  40)`, default 30), `TissueGrid` (coarse occupancy, **plus, as of the
  closing task, per-slot infection timers and continuous secretion
  strength** — see `docs/INTERFACE.md`), `CytokineField` (coarse-grid
  gradient, bilinear-interpolated down to fine resolution per
  `GAME_DESIGN.md` section 7's implementation note; now recomputed on a
  timer from weighted infected-source strengths rather than only on
  adhesion-count change).
- **`Assets/Scripts/Units/`** — `UnitProfile` (per-type
  fine-tiles-per-tick speed, footprint, color), `SearchUnit` (the
  random-walk / cytokine-biased-walk mover — the per-step decision itself
  now lives in `Chemotaxis`, see next), `Chemotaxis` (new this closing
  task — static, side-effect-free per-step neighbour-choice algorithm,
  pulled out of `SearchUnit` so a headless verification harness could call
  the real production algorithm), `CytokineToggle` (the `C`-key runtime
  debug toggle, works in a standalone build).
- **`Assets/Scripts/Pathogens/`** — `PathogenAgent` (spawn → transit →
  adhere, or transit-and-exit), `PathogenSpawner` (timed spawning through
  `PrefabPool`; recomputes `CytokineField` on a fixed 0.4s timer as of the
  closing task, not just on adhesion events).
- **`Assets/Scripts/Rendering/`** — `RuntimeSprites` (procedural flat-color
  square sprite, no imported art), `BoardRenderer` (coarse-cell
  host-tissue-vs-pathogen coloring, **plus, as of the closing task, a
  cytokine-field heatmap tint blended in on top** — see below), `HudOverlay`
  (IMGUI debug text — deliberately not `UnityEngine.UI`, see below).
- **`Assets/Scripts/Bootstrap/GameBootstrap.cs`** — builds the whole scene
  at runtime; the closest thing this project has to a "main" entry point
  right now.
- **`Assets/Scripts/Pooling/PrefabPool.cs`** — unchanged from Sprint 0
  except one addition, `public void SetPrefab(GameObject)`, needed because
  Sprint 1's pools are wired up entirely from code (no Inspector
  drag-and-drop available). Both units and pathogens go through this pool,
  not raw `Instantiate`/`Destroy`, per `GAME_DESIGN.md` section 8. Host
  cells (the coarse-grid background quads) do **not** go through the pool
  — they're created once at startup and never churned, which is outside
  what the pooling requirement targets (repeated spawn/destroy of
  enemies/projectiles/effects).
- **`Assets/Scripts/Platform/SteamStub.cs`** — unchanged from Sprint 0,
  still a placeholder.

### Sprint 1 closing task: cytokine-sensing legibility fix (2026-08-19)

First playtest verdict: the random walk read fine and the build was
liked, but toggling cytokine sensing (`C` key) produced **no perceptible
difference**. Diagnosis (see `SPRINT_PLAN.md`'s "Closing task" section):
the field's sources were just "wherever a pathogen happens to be" with no
distinct infected-cell concept and a flat one-shot strength, and the
resulting per-step bias was real but too gradual to notice in a short
session — compounded by there being no visual cue that a field existed at
all. Three changes landed to fix this, all documented in detail in
`docs/INTERFACE.md`:

1. **Infected-cell concept with continuous secretion**
   (`TissueGrid.GetSecretionStrength`) — adhesion now starts a per-slot
   infection timer; secretion ramps from `BaseSecretionStrength` (6) to
   `MaxSecretionStrength` (32) over `InfectionRampSeconds` (20s), rather
   than being a flat value from the moment of adhesion. `CytokineField` is
   recomputed from this on a timer (every 0.4s, `PathogenSpawner`) instead
   of only when the adhered-slot count changes, since the field now keeps
   changing on its own.
2. **Much stronger, differently-shaped bias** — the per-step neighbour
   weighting (now `Chemotaxis.ChooseNextStep`, pulled out of
   `SearchUnit.StepOnce` into its own static class) changed from a linear
   function of each candidate's *absolute* field value to a softmax over
   each candidate's value *relative to the best candidate* among the four,
   which is what actually fixes the imperceptibility problem — a linear
   weighting on absolute value barely distinguished four fine tiles a
   coarse-cell's-width apart from each other, no matter how large the
   coefficient. `Chemotaxis.GradientSharpness = 4f` (empirically tuned,
   see Build status below).
3. **Visible heatmap cue** (`BoardRenderer`) — coarse-cell background
   quads now blend a warm orange tint proportional to local cytokine
   field strength, on top of the existing host/pathogen coloring.
   Deliberately shown regardless of the toggle state (the field exists in
   the fiction whether or not a cell type can sense it yet), so cause (a
   hot cell) and effect (units pulled toward it, only when sensing is ON)
   are both on screen at once.

### Notable implementation choice: no `UnityEngine.UI` (uGUI)

`game/Packages/manifest.json` doesn't include `com.unity.ugui` — Unity 6
split legacy uGUI (`Canvas`, `Text`, `CanvasScaler`, etc.) out to its own
package, and it isn't installed in this project. Adding a package needs
network access and is normally an Editor-GUI/Director step (the same
constraint Sprint 0 noted for Steamworks — see `SteamStub.cs`'s comment).
Sprint 1 hit this directly: the first compile attempt used
`UnityEngine.UI.Text` for the debug HUD and failed with `CS0234`. Rather
than add the package mid-sprint, the HUD was rebuilt with IMGUI (`OnGUI`),
which needs nothing extra and is a reasonable fit for a debug overlay.
**If/when real UI work starts** (buy panel, unit inspector, etc. — all out
of scope so far), installing `com.unity.ugui` (or evaluating UI Toolkit,
which *is* available via `com.unity.modules.uielements`, already in the
manifest) is a deliberate step to take then, not something to route around
again.

## Build status (Sprint 1, including the closing task)

### Headless algorithm verification (closing task's required evidence)

Before touching the build, `Assets/Editor/CytokineVerification.cs`
(`Unity.exe -batchmode -quit -projectPath <path> -executeMethod
CytokineVerification.RunComparison`) drove the actual production
`Chemotaxis.ChooseNextStep`/`TissueGrid`/`CytokineField` classes headlessly
(no `GameObject`s, no play mode) to measure the ON/OFF difference directly,
per the closing task's explicit ask for more than "it compiles and
launches." 10 simulated units (mixed macrophage/neutrophil speed) against
5 infected sites on a 30x5 board, same random seed both runs, average
unit-to-nearest-infected-cell distance in coarse cells (Manhattan):

| Window | OFF (rung 1) | ON (rung 2) |
|---|---|---|
| 0:00–1:00 | 2.99 | 0.20 |
| 1:00–2:00 | 3.14 | 0.00 |
| 2:00–2:30 | 2.84 | 0.00 |

OFF never meaningfully converges within the 2.5-minute simulated window
(consistent with `GAME_DESIGN.md` section 7's own cover-time estimate for a
30-wide board); ON reaches distance ~0 (a unit sitting in the same coarse
cell as an infected site) for effectively the whole population well before
the 1-minute mark. A finer-grained run (`RunFineGrainedSweep`, 10-second
buckets, `Chemotaxis.GradientSharpness = 4f`) showed this isn't an instant
snap: per-unit first-arrival times ranged 0.2s–16.4s (avg 4.5s across all
10 units), and the 10s-bucket average distance stepped down gradually
(1.00 → 0.21 → 0.00 → ...) rather than jumping straight to 0 — i.e. it
reads as fast, visible drift, not teleportation. `GradientSharpness`
itself was chosen by sweeping 2–20 and picking the smallest value that
still produced a dramatic, fast difference (higher values converge even
faster but make individual steps look closer to deterministic pathfinding,
risking rung 2 reading like rung 3).

**Honesty check on what this evidence does and doesn't show:** this
confirms the mechanism produces a large, fast, measurable difference in
the exact algorithm the game runs, using a representative but hand-picked
scenario (5 sources, 10 units, one seed). It is not a claim about how the
Director will perceive it, which the screenshots and build below are
closer to, and even those are still this session's read, not the
Director's.

### Real build + runtime verification

Verified via `BuildScript.BuildWindows()` (`Unity.exe -batchmode -quit
-projectPath <path> -executeMethod BuildScript.BuildWindows`):

- **Windows**: `Succeeded, size: 93277352 bytes, errors: 0` →
  `Builds/Windows/ImmunologyTowerDefense.exe`. (Sprint 1's original build
  was 93274792 bytes; the ~2.5KB difference is consistent with the new
  `Chemotaxis.cs` and `CytokineVerification.cs` — the latter is
  Editor-only and doesn't ship, so this is just the `Chemotaxis`/rework
  code.)
- Launched the built `.exe` directly (not the Editor) and confirmed:
  process stayed alive for the full ~60s observation window (started,
  screenshotted twice ~30s apart, then manually stopped — never crashed on
  its own), and `Player.log`
  (`%LOCALAPPDATA%\..\LocalLow\DefaultCompany\game\Player.log`) has no
  `Exception`/`Error` lines, same clean-launch signature as Sprint 1's
  original verification.
  - Captured window contents via the Win32 `PrintWindow` API. **Note for
    next time:** this sprint's build renders via D3D12 (see `Player.log`),
    and `PrintWindow` with flag `0` (what worked in the original Sprint 1
    verification, presumably a different renderer at the time) returned a
    solid black frame this time — needed flag `2`
    (`PW_RENDERFULLCONTENT`) to actually capture DirectX-composited
    content. Worth trying `2` first if a future session hits the same
    black-frame result. Also, `Add-Type`-defined PowerShell types don't
    persist across separate tool calls — redefine them in the same call
    that uses them.
  - **Screenshot 1** (sensing OFF, ~8s after launch): host cells (pink)
    visually distinct from adhered pathogens (dark maroon), **and** a
    visible warm-orange tint gradient around infected cells — the heatmap
    cue renders correctly and is legible even at this small a board
    region. Units (blue macrophages, yellow neutrophils) scattered with no
    obvious pull toward hot cells.
  - **Screenshot 2** (sensing ON, ~25s after toggling `C` via a
    `SendKeys` call through an `AttachThreadInput`-forced-foreground
    window, ~33s further into the same run): three units — including a
    macrophage and a neutrophil stacked on the same cell — now sitting
    directly on top of infected/hot cells, versus zero doing so in
    Screenshot 1. HUD line confirms "Cytokine sensing: ON". This is the
    single clearest piece of visual evidence gathered this task: the same
    running process, ~30 seconds apart, before/after the toggle, showing
    units visibly relocating onto hot cells.
  - Screenshots saved to `game/screenshot_before_toggle.png` and
    `game/screenshot_after_toggle_on.png` (untracked, not committed — see
    `.gitignore`'s log-file pattern doesn't cover these, they're just
    left as loose evidence files for review).
- **WebGL**: not re-verified this sprint (Sprint 0's WebGL build/serve
  path is untouched by Sprint 1's changes and should still work per
  `tools/serve_webgl.ps1`, but wasn't re-run — the brief only required
  Windows-target verification, which is faster).

### What was verified vs. what needs the Director's own eyes

Verified directly this closing task: the algorithm produces a large,
fast, measurable ON/OFF difference (headless numbers above); the build
compiles and runs without exceptions; the heatmap tint renders and tracks
infection; units visibly relocate onto infected cells within ~30 seconds
of toggling sensing on, in an actual packaged build screenshot comparison.

**Not verified, and can't be from this session:** whether this now reads,
to the Director, as "transformative" rather than merely "measurably
different" — the actual bar `SPRINT_PLAN.md` sets. This session's read of
its own screenshots is that the difference looks strong and immediate, but
that's this session's judgment, not the Director's. Also unverified: taste
questions this fix didn't try to answer — is `GradientSharpness = 4`
*too* strong once real balance/economy exists around it (see
`docs/INTERFACE.md`'s open question 4), does the orange heatmap read
clearly against the pink/maroon board at a glance without the HUD's
explainer line, and whether the original Sprint 1 question ("does the
random walk read as frustrating-but-legible") still holds now that rung 2
is dramatically stronger and easy to compare against.

**Original Sprint 1 verification (still valid, not re-run this task):**
confirming the toggle changes a visible label and confirming individual
pathogen adhesion sites are spread across the board is not the same as
watching ten minutes of movement and forming a felt judgment about pacing,
tedium, or "aha, that's different now." That's inherently a
Director-in-the-loop question; this session's screenshots are only
evidence that the mechanism is wired up and running, not that it lands —
same caveat as noted above for the closing task's own screenshots.

## Known issues

- No multi-depth pathogen descent (out of scope this sprint by design —
  see `docs/INTERFACE.md`'s pathogen section for the specific
  simplification made: adhesion row is chosen randomly at spawn rather
  than reached via burrowing).
- No host cell health/damage/fibrosis (out of scope this sprint).
- Units debug-spawn at randomized fine-grid positions rather than via real
  bone-marrow placement / blood extravasation (out of scope this sprint —
  see `docs/TEAM_RETRO.md`).
- `UnityEngine.UI` is not installed in this project; any future UI work
  needs a conscious package-installation step first (see above).
- WebGL not re-verified this sprint or the closing task (see above) —
  should still work but hasn't been re-confirmed against the current code.
- `Chemotaxis.GradientSharpness = 4f` and the infection-ramp constants in
  `TissueGrid.cs` are tuned for legibility (this closing task's mandate),
  not validated against balance/pacing — flagged as open questions in
  `docs/INTERFACE.md`. Whoever builds the round 2 economy should expect to
  revisit these.
- The heatmap tint's saturation reference is `TissueGrid.MaxSecretionStrength`
  (32), a single infected cell's own fully-ramped strength. Overlapping
  falloff from multiple nearby infected cells can exceed that and clamp to
  "fully hot" sooner than a single isolated cell would — visually fine
  (reads as "very hot area"), just worth knowing if the tint ever looks
  saturated across a wider area than expected.

## Addendum: what batchmode CLI can and can't do (relevant again this sprint)

Same limitation Sprint 0 recorded for the old device-bridge setup, still
true for a native session running without an interactive Editor window:
there is no way to drag a prefab into an Inspector field, hand-place a
GameObject, or otherwise use Editor GUI affordances. Everything has to be
either (a) generated at runtime from code (this sprint's approach — see
`GameBootstrap`), or (b) built via an Editor script driven by
`-executeMethod` in batchmode (this sprint's `SceneSetup.cs`). Both worked
well. Worth carrying forward: don't assume a future Code agent has
interactive Editor access unless told otherwise.
