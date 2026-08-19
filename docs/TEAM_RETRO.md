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

### Sprint 1 closing task — Code agent (2026-08-19, cytokine-sensing fix)
- **The actual bug wasn't the bias math, it was what fed it.** The
  original linear weighting (`1 + k * absoluteFieldValue`) looked
  reasonable in isolation, but the field is bilinear-interpolated smoothly
  across each coarse cell, so any four fine-tile neighbours have nearly
  identical absolute values — no `k` fixes that, because the thing being
  weighted barely varies. The fix that actually mattered was switching to
  a softmax over each candidate's value *relative to the best candidate*,
  which is only sensitive to the (small but real) local difference that
  exists. Tip for next time a "the math should work but nothing visible
  happens" bug shows up: check whether the SIGNAL being weighted actually
  varies meaningfully between the choices being compared, before assuming
  the weighting formula itself is wrong.
- **Made `TissueGrid`/`CytokineField`/the new `Chemotaxis` static class
  take simulated time and randomness as explicit inputs rather than
  reading `UnityEngine.Time`/implicit state internally**, specifically so
  a headless Editor-batchmode script (`CytokineVerification.cs`) could
  drive the real production algorithm and print real before/after numbers
  without needing play mode or any `GameObject`s. This paid off
  immediately — it's also what let a same-process parameter sweep
  (`Chemotaxis.GradientSharpness` from 2 to 20) run in one Unity launch
  instead of one launch per value. Worth defaulting to this pattern
  (explicit time/random inputs on core simulation classes) going forward,
  not just when a verification harness is already planned — it's cheap to
  do up front and expensive to retrofit.
- **`Add-Type`-defined PowerShell types (the Win32 `PrintWindow`/
  `AttachThreadInput` P/Invoke wrappers) do not persist across separate
  PowerShell tool calls** in this environment (shell state resets between
  calls, per the tool's own description — this is expected behavior, not
  a bug, but easy to forget mid-task). Redefine the `Add-Type` block in
  every call that uses it, or do the whole screenshot/input sequence in
  one call.
- **First `PrintWindow` capture attempt of this build returned a solid
  black frame with flag `0`**; needed flag `2` (`PW_RENDERFULLCONTENT`)
  to actually capture this build's D3D12-rendered content, consistent
  with the tip already recorded above from Sprint 1 proper — re-confirming
  it here since it's easy to reach for flag `0` as the "default" and lose
  a round trip.

### Sprint 2 — Code agent (2026-08-19, bone marrow placement + pathogen classes/combat)

**Judgment calls made this sprint, consolidated here** (each is also noted
inline in code comments and in `docs/INTERFACE.md`/`docs/ENGINE_STATUS.md`
at the relevant spot — this is the one-stop list):

- **Bone marrow slot count: 5** (`GameBootstrap.BoneMarrowSlotCount`).
  `SPRINT_PLAN.md` says "a small number," no specific figure. Picked so a
  mixed macrophage/neutrophil strategy (2-3 of each) is possible without
  the strip visually dominating the layout next to a 30-column tissue
  board.
- **Bone marrow emission interval: 4 seconds per placed tower**
  (`BoneMarrowManager.EmissionIntervalSeconds`). Not specified anywhere.
  Chosen slower than `PathogenSpawner`'s 2.5s spawn interval on the
  reasoning that a player can place several towers at once (each emitting
  independently), so per-tower cadence should be a bit more conservative
  than the single pathogen spawner's — but this is a guess, not a balance
  pass, and there's currently no cap on total standing unit population
  (see `docs/INTERFACE.md` open question 6) so a fully-built-out bone
  marrow (5 towers) produces one new unit roughly every 0.8s on average,
  which may prove too fast or too slow once there's an actual economy
  constraining how many towers a player can afford.
- **Pathogen class weights: 45% virus / 25% bacterium / 30% large
  bacterium** (`PathogenAgent.VirusChance`/`BacteriumChance`). Not
  specified. Weighted toward virus specifically because
  `SPRINT_PLAN.md`/`GAME_DESIGN.md` both call viral spread "the sprint's
  most important piece" — an equal three-way split risked the Director not
  seeing much of it in a short playtest session purely by chance.
- **Combat numbers: 1 flat damage per contact-tick; 12 HP for both
  intracellular classes, 18 HP for large bacteria; 15s incubation before
  an uncleared virus infection attempts to spread; 1s retry cadence if a
  spread attempt is blocked.** All flat/simple per `SPRINT_PLAN.md`'s
  explicit request, all picked to be "legible within a short playtest"
  rather than balanced — same standard Sprint 1 used for
  `Chemotaxis.GradientSharpness` and `InfectionRampSeconds`. The 15s
  incubation number specifically was chosen relative to Sprint 1's own
  measured search speeds (cytokine sensing reaches an infected cell in
  ~4.5s on average; a rung-1 random walk doesn't reliably converge within
  2.5 simulated minutes on a 30-wide board) so that fast search
  comfortably beats the incubation window and slow search visibly doesn't
  — this is the whole point of the mechanic, so the number wasn't picked
  arbitrarily even though it's still a guess in absolute terms.
- **Large bacteria and intracellular pathogens clear via the exact same
  HP-depletion mechanic; only the rendering (and framing) differs.** The
  design doc frames these as different in kind ("collateral damage to the
  host cell" vs. "direct damage to the pathogen"), but building two
  parallel combat systems for a difference that's currently only
  cosmetic (no host-cell-health/fibrosis system exists yet to make
  "collateral" mean something numerically distinct from "direct") would
  have been speculative complexity for no observable payoff this sprint.
  Flagging this explicitly since it's a real interpretation call, not an
  oversight — when fibrosis/host-cell-health lands, this is the natural
  point to actually split the two mechanics if they need to diverge
  numerically.
- **The scene file was not renamed from `Sprint1.unity` to
  `Sprint2.unity`**, breaking the pattern Sprint 1 itself established
  (`Sprint0.unity` → `Sprint1.unity`). Deliberate: the file's actual
  content is just one `GameObject` with default-value serialized fields —
  all real state is runtime-code-generated — so a rename would have been
  purely cosmetic and cost a rebuild/re-verify cycle for zero functional
  benefit. Flagged here so a future session doesn't read "Sprint1.unity"
  as meaning stale/wrong content, and can rename it later in a lower-risk
  moment (e.g. alongside an unrelated scene change) if keeping the naming
  convention matters enough to the Director.

**Two real bugs found this sprint, both worth remembering:**

- **`MonoBehaviour.Awake()` is not guaranteed to fire from `AddComponent()`
  outside Play Mode.** Assumed (wrongly) that it would, based on general
  Unity folklore and the fact that Sprint 1's `CytokineVerification.cs`
  never hit a counterexample (its dummy `GameObject`s never needed
  `Awake()`-driven state). `PrefabPool.Awake()` builds the pool; a
  headless `CombatVerification.cs` run hit a `NullReferenceException` in
  `Get()` immediately. Fixed with a lazy `EnsurePool()` guard called from
  `Get()`/`Release()` too, not just `Awake()`. **Tip for next time**: any
  headless harness that `AddComponent`s something and expects its
  `Awake()`-built state to be ready should either call an explicit init
  method directly, or verify the component doesn't rely on `Awake()`
  firing outside Play Mode before trusting it.
- **A screenshot that looks like a camera-framing bug might be a DPI-scaling
  bug in the screenshot tool instead.** The first real-build screenshot
  this sprint appeared to show the right ~25% of the board cropped off and
  the entire lymph node compartment missing. Spent real effort adding a
  one-frame-later camera refit (`GameBootstrap.RefitCameraNextFrame`) to
  fix what looked like a stale-`Camera.aspect`-at-Awake bug — a real and
  worthwhile fix on its own merits, but it turned out **not** to be the
  cause: the actual problem was that `GetWindowRect`, called from a
  PowerShell process that hadn't called `SetProcessDPIAware()`, returned
  window dimensions scaled down by this machine's 150% display scaling
  (1707×1067 instead of Unity's real 2560×1600 render resolution). The
  screenshot bitmap was allocated at the wrong (smaller) size, so
  `PrintWindow` produced a visibly cropped/scaled capture even though the
  game itself was rendering the full board correctly the whole time. Confirmed
  by cross-checking a `Debug.Log`'d `Screen.width`/`height` (`2560x1600`)
  against the un-aware `GetWindowRect` result. **Tip for next time**: on a
  scaled Windows display, call `SetProcessDPIAware()` (or an equivalent
  per-monitor-DPI-awareness call) *before* any `GetWindowRect`/
  `SetCursorPos` call used for screenshot or input automation, or the
  physical-pixel math will be off by the scale factor — and don't
  necessarily trust a first-look "this looks cropped" diagnosis without
  cross-checking the game's own reported `Screen.width`/`height` first.

**A build's window not updating between screenshots isn't necessarily
frozen/crashed — it might just not have OS focus.** Mid-verification, two
screenshots taken several tool-calls apart came back pixel-identical
(same unit positions to the pixel). Turned out the build's window had lost
foreground focus, and this project's build has `Application.runInBackground`
at Unity's default (off) — so the game simply stops ticking while
unfocused, and resumes the instant focus returns (confirmed: forcing
foreground via the `AttachThreadInput`/`SetForegroundWindow` trick made
units visibly resume moving in the very next screenshot). Worth knowing
before assuming a stale screenshot means something crashed.

**Real, useful accident: it appears the Director interacted with the live
build's window himself during this session** — this session never sent any
click/key input until deliberately doing so near the end for a controlled,
reproducible test, yet an early screenshot showed two bone marrow towers
already placed (one Macrophage, one Neutrophil) and cytokine sensing
toggled ON, in a pattern very consistent with a person clicking around out
of curiosity (this machine has a real, visible desktop — see `CLAUDE.md`).
If that's what happened: genuinely great unscripted evidence that the
click-to-place UI works for an actual human, better than anything a
synthetic click could prove. This session also performed its own fully
controlled click-through afterward (open an empty slot's picker, choose
Neutrophil, confirm the slot's label/color changed) using screen
coordinates computed analytically from a logged
camera-position/orthographic-size/aspect diagnostic rather than eyeballed
— both landed correctly, which is good independent confirmation the
placement math (`docs/ENGINE_STATUS.md`'s post-refit diagnostic line) is
right, not just that a human happened to click near the right spot.

**Headless verification continues to earn its keep.** `CombatVerification.cs`
(35/35 assertions) caught the `PrefabPool.Awake()` bug immediately, before
any build was attempted — cheaper by a wide margin than finding it via a
build+screenshot cycle. It also proved the viral spread mechanic chains
across generations (an origin infection spreading to a child, which itself
later spreads to a grandchild) well before any build existed to watch it
happen live, which mattered here since this sprint's real-build screenshot
session didn't happen to catch a live spread event on camera (the 15s
incubation window didn't line up with the screenshot timing) — without the
headless proof, this sprint's evidence for the single most important
mechanic in the brief would have been weaker than it should be.
