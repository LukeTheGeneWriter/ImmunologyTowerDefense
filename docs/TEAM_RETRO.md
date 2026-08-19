# Team Retro Log

Raw, dated notes from Code, UI, or the Producer — anything that was harder
than it should have been, or easier because of a tip left behind. A few
lines per sprint. Never overwritten, only appended to. See `WORKFLOW.md`
Section 6.1 for the intended format.

(Empty — Sprint 0 hasn't run yet.)

### Sprint 0 — Producer
- Discovered the Claude desktop device bridge's shell (`device_bash`) runs in
  an isolated Linux sandbox, not the real Windows OS — it can read/write
  files in a connected folder (git add/commit worked fine there) but cannot
  execute Windows binaries like Unity.exe. Tip for next Code session: any
  Unity CLI/batchmode step needs to be run by the Director directly, or by
  a local Claude Code session running natively on the Director's machine —
  not assumed to be scriptable through the device bridge.

### Sprint 0 — Producer (cont.)
- Unity project creation via `-batchmode -createProject` genuinely worked
  (confirmed via the log and ProjectVersion.txt), but $LASTEXITCODE came
  back empty/null in PowerShell rather than 0, making it look like a
  failure. Fixed setup_unity_project.ps1 to check for
  ProjectSettings\ProjectVersion.txt instead of trusting the exit code.
  Tip for future build/CLI scripts: don't trust $LASTEXITCODE for Unity
  batchmode calls on this setup -- check a real output artifact or grep
  the log for "Exiting batchmode successfully" instead.

### Sprint 0 -- Producer (closing out)
- PowerShell blocks direct .ps1 execution by default (not an admin-rights
  issue -- running as admin does NOT fix it). Every .ps1 in this repo
  needs to be invoked as `powershell -ExecutionPolicy Bypass -File <path>`,
  not run directly or via `& <path>`. Forgot this once for
  serve_webgl.ps1 and cost a round trip -- should have been consistent
  with setup_unity_project.ps1's invocation from the start.
- Unity 6 Hub labels the WebGL module "Web Build Support," not "WebGL
  Build Support" -- don't assume it's missing just because that exact
  string isn't in the modules list.
- First WebGL build was slow (30+ min: IL2CPP -> Emscripten -> wasm) with
  a near-silent-looking log. Don't conclude a build is stuck without
  checking log mtime / newer files under Library/Bee first.
- Local WebGL testing needs a real HTTP server (file:// won't load the
  fetch-based loader) AND correct Content-Encoding: gzip handling for
  Unity's compressed output -- plain `python -m http.server` doesn't set
  that header either, so it's not just a "no Python" problem. Use
  `tools/serve_webgl.ps1`.
- Sprint 0 complete: both builds launch-confirmed by the Director.

### Sprint 1 — Code agent
- **No `UnityEngine.UI` package installed.** First compile attempt used
  `UnityEngine.UI.Text`/`Canvas`/`CanvasScaler` for the debug HUD and
  failed with `CS0234` (`UnityEngine.UI` namespace not found). Unity 6
  split legacy uGUI into its own package (`com.unity.ugui`), and it isn't
  in `game/Packages/manifest.json`. Rather than add a package mid-sprint
  (needs network access, and per the existing Steamworks precedent in
  `SteamStub.cs` that's treated as a deliberate Editor-GUI/Director step,
  not something a Code agent should do unprompted), rebuilt the HUD with
  IMGUI (`OnGUI`) instead — works fine for a debug overlay, needs nothing
  extra. Tip for next session: if real UI work starts (buy panel, unit
  inspector), installing `com.unity.ugui` or evaluating UI Toolkit
  (`com.unity.modules.uielements`, already present) is a conscious step to
  take then — don't assume uGUI is available.
- **Judgment call: units debug-spawn at randomized fine-grid positions.**
  Bone-marrow placement and blood-entry/extravasation are out of scope this
  sprint per `SPRINT_PLAN.md`. Rather than build any placeholder version of
  entry mechanics, `GameBootstrap.SpawnUnits` just picks a uniformly random
  `FineCoord` across the whole board for each unit at start. Stated
  explicitly here per the brief's own request, so it isn't later mistaken
  for an actual design decision about how units enter the tissue.
- **Judgment call: pathogen adhesion row is random, not depth-1-only.**
  `GAME_DESIGN.md` describes adhesion as happening at the mucus/glycocalyx
  layer (depth 1) specifically, with burrowing to deeper layers over time.
  Full multi-depth descent is explicitly out of scope this sprint
  (`SPRINT_PLAN.md`: "adhesion + presence is enough"). Rather than pin
  every pathogen to one coarse row (which would make the search problem
  effectively one-dimensional and not exercise board width the way
  `GAME_DESIGN.md` section 7 says width should matter), pathogens pick a
  random coarse row at spawn and adhere there directly. This is a
  reasonable reading of "adhesion + presence is enough to give the two
  hunting units something to search for," but it's a real interpretation
  call, not a re-statement of the design doc — flagged in
  `docs/INTERFACE.md`'s open questions too, in case the Director's mental
  model specifically meant "adhesion happens at depth 1."
- **Judgment call: pathogens don't block unit movement.** Units co-occupy
  fine tiles with adhered pathogens exactly like they co-occupy with host
  cells; "collision" is detected as the unit's current tile falling in the
  same *coarse* slot as an adhered pathogen (triggers a visible flash), not
  as a literal walkable/blocked distinction on the fine lattice. Chosen
  because there's no combat/kill system yet to make blocking meaningful —
  a unit "stuck" against a pathogen it can't interact with would just look
  broken. Worth revisiting once combat exists.
- **No interactive Unity Editor session was available this sprint** (same
  situation Sprint 0's Producer notes describe for the old device bridge,
  but for a different reason — this session had a real native shell, just
  never opened the Editor's GUI window). That meant no drag-and-drop
  prefab assignment, no hand-placing GameObjects in the Scene view.
  Worked around by making the entire scene construct itself at runtime
  from a single bootstrap `GameObject` (`GameBootstrap.Awake()` builds
  camera, grid, units, pathogens, HUD all in code) and by adding a small
  `PrefabPool.SetPrefab(GameObject)` method so pools built from runtime
  templates could still go through the existing pooling utility instead of
  raw `Instantiate`. An Editor script (`Assets/Editor/SceneSetup.cs`, run
  via `-executeMethod` in batchmode) creates and saves the one-object
  scene. This worked well and is probably the right default approach for
  future sprints too, given how this project is being run — but a future
  session with real Editor GUI access should know this is a workaround,
  not a load-bearing architectural preference.
- **Verifying a Windows build without watching it:** `SetForegroundWindow`
  called on a just-launched background process did not reliably steal
  focus from whatever window was already in the foreground (Windows'
  focus-stealing prevention) — a naive screenshot attempt captured an
  unrelated foreground window instead of the game. Fix: the Win32
  `PrintWindow` API (`PW_RENDERFULLCONTENT` flag) captures a window's
  contents directly by handle, regardless of focus or z-order, and worked
  reliably. To send input (e.g. testing the cytokine-sensing toggle's `C`
  key) an `AttachThreadInput` trick was needed before `SetForegroundWindow`
  would actually succeed. Tip for future sessions needing to verify a
  build visually without a human watching: `PrintWindow` for screenshots,
  `AttachThreadInput` + `SetForegroundWindow` + `SendKeys` if input needs
  to reach a specific window.
