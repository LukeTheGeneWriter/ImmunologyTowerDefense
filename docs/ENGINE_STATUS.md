# Engine Status

Rewritten at the end of every sprint by the Code session, not just appended
to. This version reflects the state after Sprint 1 (the search-problem
prototype). Sprint 0's engine/platform decision section is preserved below
since it's still accurate; the "current state" and "build status" sections
are replaced wholesale.

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
  40)`, default 30), `TissueGrid` (coarse occupancy), `CytokineField`
  (coarse-grid gradient, bilinear-interpolated down to fine resolution per
  `GAME_DESIGN.md` section 7's implementation note).
- **`Assets/Scripts/Units/`** — `UnitProfile` (per-type
  fine-tiles-per-tick speed, footprint, color), `SearchUnit` (the
  random-walk / cytokine-biased-walk mover), `CytokineToggle` (the
  `C`-key runtime debug toggle, works in a standalone build).
- **`Assets/Scripts/Pathogens/`** — `PathogenAgent` (spawn → transit →
  adhere, or transit-and-exit), `PathogenSpawner` (timed spawning through
  `PrefabPool`, triggers `CytokineField` recomputation on adhesion).
- **`Assets/Scripts/Rendering/`** — `RuntimeSprites` (procedural flat-color
  square sprite, no imported art), `BoardRenderer` (coarse-cell
  host-tissue-vs-pathogen coloring), `HudOverlay` (IMGUI debug text —
  deliberately not `UnityEngine.UI`, see below).
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

## Build status (Sprint 1)

Verified via `BuildScript.BuildWindows()` (`Unity.exe -batchmode -quit
-projectPath <path> -executeMethod BuildScript.BuildWindows`):

- **Windows**: `Succeeded, size: 93274792 bytes, errors: 0` →
  `Builds/Windows/ImmunologyTowerDefense.exe`.
- Launched the built `.exe` directly (not the Editor) and confirmed:
  process stayed alive and stable for ~80 seconds of observed runtime, no
  exceptions/errors in `Player.log` (`%LOCALAPPDATA%\..\LocalLow\
  DefaultCompany\game\Player.log`).
  - Captured window contents via the Win32 `PrintWindow` API (screen
    capture via `SetForegroundWindow`/`CopyFromScreen` didn't reliably
    steal focus from the launching process — Windows blocks background
    processes from stealing foreground focus by default; `PrintWindow`
    captures a window's contents directly regardless of focus/z-order and
    worked reliably). Screenshots confirmed: the coarse grid renders with
    host cells (pink) visually distinct from adhered pathogens (dark
    maroon, spread across multiple rows and columns, not clustered at the
    entry edge); two visually distinct unit types (blue macrophages,
    larger; yellow neutrophils, smaller) present and dispersed across the
    board, not clustered at a single spawn point; at least one visible
    contact-flash in progress (a pathogen cell rendering lighter,
    consistent with `PathogenAgent`'s highlight-on-contact behavior).
  - Forced foreground focus via an `AttachThreadInput` trick (plain
    `SetForegroundWindow` alone was refused by Windows) and sent a `C`
    keypress via `SendKeys`; a follow-up `PrintWindow` capture confirmed
    the HUD's "Cytokine sensing: OFF" line flipped to "Cytokine sensing:
    ON" — the runtime debug toggle genuinely works in the packaged build,
    not only in theory.
- **WebGL**: not re-verified this sprint (Sprint 0's WebGL build/serve
  path is untouched by Sprint 1's changes and should still work per
  `tools/serve_webgl.ps1`, but wasn't re-run — the brief only required
  Windows-target verification, which is faster).

### What was verified vs. what needs the Director's own eyes

Verified directly (build compiles, runs without crashing/exceptions,
renders the expected visual elements, toggle mechanically works): all of
the above.

**Not verified, and can't be from this session:** whether the random walk
actually *feels* "frustrating-but-legible rather than broken," and whether
the cytokine-sensing toggle *feels* "transformative" when flipped — the two
questions `SPRINT_PLAN.md` says this sprint exists to answer. Confirming
the toggle changes a visible label and confirming individual pathogen
adhesion sites are spread across the board is not the same as watching ten
minutes of movement and forming a felt judgment about pacing, tedium, or
"aha, that's different now." That's inherently a Director-in-the-loop
question; this session's screenshots are only evidence that the mechanism
is wired up and running, not that it lands.

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
- WebGL not re-verified this sprint (see above) — should still work but
  hasn't been re-confirmed against Sprint 1's code.

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
