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

### Sprint 3 — Code agent + head session (2026-08-21, population homeostasis)

**Process lesson, the big one: the dispatched Code agent hit its usage
limit mid-sprint.** It had done the hard part — code committed (`8eaca14`),
`LifecycleVerification.cs` written, a successful Windows build — and then
died partway through the doc updates, having written **none** of them. Its
last words were "While that runs, I'll update the docs. Starting with
`INTERFACE.md`."

What saved the sprint: the code was committed with a genuinely explanatory
commit message (every judgment call, with reasoning), and the verification
harness was itself committed and re-runnable. The head session re-ran
everything from scratch and reconstructed all four docs from the commit
message, the code comments, and fresh harness output. Nothing was lost.

**Two practices to carry forward:**
- **Brief dispatched agents to write docs incrementally, not as a final
  step.** Docs written last are the docs that don't get written. Better:
  update `INTERFACE.md` as each signature changes, and append to this file
  when each judgment call is *made*, not in a retrospective sweep.
- **A verbose, reasoning-heavy commit message is cheap insurance.** It was
  the primary recovery artifact here. "Explain why, not just what"
  (`WORKFLOW.md` §2) turned out to matter for a reason nobody anticipated:
  it's what a *successor* reads when the author is gone mid-task.

**Judgment calls made this sprint:**
- **Chebyshev, not Manhattan, for contact radius.** Matches the square
  footprints these units already render as; the Manhattan diamond covers
  half the tiles and would have halved contact frequency a second time on
  top of the intended reduction.
- **The blocked emission timer clamps rather than banks.** Not specified
  anywhere, and it's load-bearing: if a blocked tower accumulated the
  emissions it was "owed," a tower whose population died at once would
  dump ~10 units the instant a slot freed, defeating the second cap
  entirely. `GAME_DESIGN.md` §6d's "neither cap alone is sufficient" only
  holds with the clamp.
- **Units get a value snapshot of their tower's tuning, not a live
  reference.** A mid-round upgrade improves future children only. Flagged
  to the Director, unruled — deliberately left as a one-line change.
- **A `depleting` guard on the degranulation path.** A degranulation burst
  can land a kill, which would otherwise re-trigger depletion on a unit
  already mid-despawn.

**The number worth watching: contact frequency halved.** Proximity contact
dropped hit rate to ~50% of Sprint 2's (macrophage 50.0%, neutrophil 49.2%
over 200k simulated ticks). That is intended and was anticipated in
`SPRINT_PLAN.md` item 7, but it arrived in the same sprint as a population
cap, so two independent nerfs to clearing throughput landed together. The
harness prints this as a no-assertion diagnostic specifically so nobody has
to rediscover it by feel. Resisted the temptation to compensate by bumping
damage or the cap — that would have hidden the interaction the Director
needs to actually judge.

**Windows tip, new: you may not be able to drive the built game's input at
all.** Sprint 2 successfully clicked the running build with computed
coordinates. This session could not: `SetForegroundWindow` was refused
(Windows foreground lock — the Director confirmed afterwards that he was
actively using another window at the time, which is exactly when Windows
refuses the steal; a process that doesn't own the foreground generally
can't steal it), and since `Application.runInBackground` is off, the
unfocused build doesn't tick either. **The tell is a pixel-identical
capture across a long wait** — two shots 75 seconds apart were byte-for-byte
the same, which reads as "frozen build" but is actually "never had focus."
Check for that before diagnosing a hang. `PrintWindow` still captures an
unfocused window fine, so screenshots work even when input doesn't.

**Follow-up on the focus failure (same day):** the Director was working in
another window while the automation ran, which is the ordinary cause — not
a hard environmental limit. **Practical rule: scripted input against the
built game needs the machine otherwise idle.** Either run it when nobody's
using the machine, or just hand the build to the Director and let him
click. Sprint 2's successful click automation was probably a quiet machine
as much as good coordinate math.

### Sprint 4 — Code agent + head session (2026-08-21, Map 01 geometry and the invasion loop)

**The dispatched agent hit its usage limit again — and this time nothing
was committed.** Sprint 3's agent at least left a commit; this one left
~1,600 lines of uncommitted working-tree changes, no verification harness,
no docs, and code that did not compile. Its last words were "Now
generalizing the flash effect and adding the interface renderer."

What the head session found and did, in order:

1. **It didn't compile.** `HudOverlay` had never been updated: `BoardConfig.Rows`
   stopped being a `static const` and `Bind` had grown from 4 args to 7.
2. **Both existing harnesses didn't compile either**, because
   `PathogenAgent.InitializeAdheredDirect` was renamed to
   `InitializeInTissueDirect` and now takes the `GutInterface` and
   `InvasionTally`. Added a shared fixture shim rather than passing nulls.
3. **One Sprint 2 assertion was testing a retired contract** — units
   entering at the "blood-adjacent" deepest fine row. Item 8 moved entry to
   the tissue band's base-side edge. Rewrote it in the axis frame so it
   survives the base being reconfigured.
4. **Wrote `MapVerification.cs` from scratch** (71 assertions). The agent
   had added `BoardConfig.ConfigureForTest` *specifically for this file* and
   referenced it by name in a doc comment — it knew what it owed.
5. **Found the bug that mattered** (below).

**Lesson, stronger than last sprint's: tell dispatched agents to commit
early and often, not just to write docs as they go.** "Write docs
incrementally" was in this sprint's brief and it still produced zero docs,
because the agent batched *everything* to the end — including the commit.
An uncommitted, non-compiling tree is far worse to inherit than a
half-finished committed one. Next brief should say: commit after each scope
item, even if incomplete, even if ugly.

**The bug worth remembering: a stale serialized field silently deleted the
playfield.** `Sprint1.unity` still carried Sprint 1's `columns: 30`. Unity
deserialization applies stored YAML over field initializers, so the new
`columns = 100` default lost to a value written three sprints ago. Because
`BaseBandCells` and `LumenBandCells` are clamped against the axis length,
the shortfall did not distribute — it landed entirely on the tissue band:
**25 base + 5 lumen + 0 tissue.** The game launched, rendered a board, threw
no exceptions, and was utterly unplayable.

Three things to take from it:
- **A new serialized field with a sensible default is not the same as that
  default being used.** Any scene authored before the field existed keeps
  its old value forever. Check the scene asset, not just the C# default.
- **Clamping can concentrate an error instead of spreading it.** Clamping
  the two outer bands was locally reasonable and globally catastrophic,
  because the middle band absorbed 100% of the discrepancy.
- **The harness could not have caught this.** `MapVerification` builds its
  own boards through `ConfigureForTest` and never loads the scene — correct
  for unit-style testing, and precisely why it was blind here. Added
  `GameBootstrap.WarnOnDegenerateBands` so the *runtime* complains. Consider
  a scene-level smoke check if this recurs.
- The only reason it was caught at all is that the HUD prints the band
  layout. Building the readout first, before looking at the board, paid for
  itself within one screenshot.

**Judgment calls made by the head session:**
- **Harness fixtures share one throwaway `GutInterface`/`InvasionTally` per
  `BoardConfig`** rather than passing null, so the production code path is
  exercised as written.
- **Adhesion proximity is proven statistically, end to end** — cohorts of
  400 pathogens run the whole channel with a wall-only falloff versus a
  depth-blind one — rather than by re-deriving the curve in the test. A test
  that recomputes the formula only proves arithmetic.
- **Frame cost is reported on the HUD** rather than measured once and
  written down, so it stays honest as the board grows.

**Numbers observed, for whoever tunes later:**
- 4,000 coarse cells render at **8.35 ms/frame (120 fps)**. Note that is
  *exactly* the display refresh rate, so it is vsync-capped: true cost is
  **at most** 8.35 ms and the real headroom is unknown. Measure with vsync
  off if a number with meaning is needed.
- **Cytokine sensing got dramatically weaker at map scale**, and this is the
  finding most likely to matter. Sprint 1–3 (30×5 board): OFF 2.99/3.14/2.84,
  ON 0.20/0.00/0.00 — sensing converged to zero within a minute. Map 01
  (100×40): OFF 46.93/46.83/47.05, ON 45.29/40.42/37.38. The mechanism still
  works — ON closes steadily while OFF stays flat — but it no longer
  converges within the window. Cause is not a regression: `CytokineField`
  uses `strength / (1 + distance)` with no cutoff, and at the old board's
  ~3-cell separations that gradient was steep, while at Map 01's ~47-cell
  average separation it is nearly flat. **Not tuned, per the Director's
  standing "mechanics first" instruction** — flagged in `BACKLOG.md`.

### Sprint 5 — Code agent + head session (2026-08-28, host states, debris, class advance)

**Third sprint running, third dispatched-agent interruption.** The agent
this time lost its network connection mid-item-5. It had committed items 1
and 2 and the barcode doc with genuinely explanatory messages, plus most of
item 5's class-advance code sitting uncommitted but compiling. The head
session picked it up from there. The verbose-commit-message discipline
(WORKFLOW §2) paid off for the *fourth* time as a recovery artifact —
worth treating as a hard rule now, not a nicety.

**The uncommitted tree compiled and passed all three prior harnesses**
(Map 71, Combat 36, Lifecycle 79), which is a first — the two previous
interrupted sprints left non-compiling trees. Committing after each scope
item (this sprint's brief said so explicitly) is visibly working.

**Two real bugs the new harness caught, both in the resumed item-5 code:**

- **An intracellular bacterium killed itself lysing out.** `StepIntracellularBacterium`
  called `TissueGrid.KillHostCell` to leave debris on exit — but
  `KillHostCell` notifies the cell's intracellular resident via
  `OnHostCellDestroyed`, and that resident *is* the bacterium. So it
  exited to the pool instead of surviving as a motile extracellular
  pathogen (§1b step 4). Fix: `ReleaseIntracellular` (Infected→Healthy,
  drop the resident link) *then* `KillHostCell` (Healthy→Dead+debris, no
  one to notify). Tip: any code that calls `KillHostCell` on a cell whose
  resident should outlive it must detach the resident first.

- **`PathogenSpawner.RequestSpread` let a virus spread onto non-Healthy
  ground.** It checked `IsOccupantFree` but not `IsHealthyHost`, so a
  virus ringed by dead/infected tissue burned its one-shot `hasSpread` on
  a doomed free particle instead of stalling and retrying. §1c is explicit
  ("only into a `Healthy` neighbour") and it is half of what makes the
  firebreak emerge. Added the `IsHealthyHost` check. This had been latent
  since Sprint 2 — CombatVerification only asserts "AdheredCount grew,"
  which a wasted spread still satisfies.

**Viral spread is a one-shot CHAIN, not a spreading front.** `hasSpread`
means each infected cell infects exactly one neighbour, ever, so an
infection random-walks through tissue as a snake rather than saturating
outward. This surprised the harness author mid-write (the first firebreak
test assumed a saturating front and its control/wall contrast was
meaningless because neither penetrated far). It matches CombatVerification's
existing "chains across generations" language and is probably fine, but
whoever tunes viral behaviour should know the mechanic is a path, not a
blob, and decide whether a real front (multiple simultaneous spreads, or
dropping `hasSpread`) is wanted. Recorded in BACKLOG.

**A 1-cell-thick dead gap is hoppable; a full-lane band is not.** The
firebreak is emergent, and a single dead cell between two healthy ones can
be crossed by a spread event that lands a transient free particle on the
dead cell, which then steps to the healthy cell on the far side before its
6s survival timer expires. Two-plus cells of dead ground, or a full-lane
band, is a hard wall. This is consistent with §1a's "slipping past one or
two cells is allowed and occasional" but it means the harness tests the
firebreak with a 3-cell band, not a 1-cell one.

**Serialized `UnitProfile` in `Sprint1.unity` only carries 5 of its
fields** (Kind, DisplayName, FineTilesPerTick, FootprintFineTiles, Color)
— the Sprint 3 lifecycle fields and this sprint's `EfferocytosisDebrisPerTick`
are absent from the YAML, so they take the value from `GameBootstrap`'s
`new UnitProfile { ... }` initializer. This is the *opposite* of the
Sprint 4 `columns` bug (a field present in stale YAML overriding a new
default): here the fields are simply not serialized, so the initializer
wins. New lifecycle fields added to `UnitProfile` are safe as long as
`GameBootstrap`'s initializer sets them; don't rely on the scene.

### Sprint 6 — head session (2026-08-28, the intracellular-infection rework)

Done inline by the head session (no dispatched agent) across one sitting,
straight after Sprint 5's playtest and design conversation. Three code
items, each committed green before the next.

**The contract change rippled exactly as far as expected and no further.**
Making `GetAttackableAt` return occupant-only + `ReceiveDamage` a no-op
while `IsIntracellular` broke three harness assertions, all testing the
now-retired "innate grinds an intracellular infection down through the
cell" path (CombatVerification's clear-before-incubation, LifecycleVerification's
kill-attribution-on-a-virus, TissueVerification's clear-via-ReceiveDamage).
Fixes were mechanical: use `KillHostCell` for "caught early", use
`LargeBacterium` (extracellular) where the test is class-agnostic. Worth
knowing for the next combat-contract change: grep the harnesses for the
old verb, don't just compile.

**A one-shot chain vs. a repeating bud is the whole difference between a
snake and a disk** — no separate "grow radially" code needed. The budding
virus just doesn't set `hasSpread`; it emits every interval. The momentum
walk on the free virion is polish on top (radial vs. blobby), and the
"only step onto Healthy cells" rule is what keeps the firebreak intact
through the rework — same emergent-not-checked principle as Sprint 5.

**Budding-disk test: advance time in 1s steps, not one big jump.** A free
virion dies after `VirusFreeSurvivalSeconds` (6s). A test loop that jumps
`t` by an incubation window per cycle but only calls `SimulationTick(1, t)`
gives the virion one step of movement and then 15+ seconds of "you've been
homeless too long" — it never establishes. Drive the clock at the
granularity the mechanic actually runs at.

**Non-determinism from a per-spawn coin flip.** `VirusBuddingSpeciesChance`
0.5 and `VirusBurnoutChance` 0.30 are rolled per agent off unseeded
`Random`, so any test that seeds a virus and watches it over time is flaky
unless it forces those to 0 (or 1) for the scenario it's actually about.
Every viral-spread test now sets them explicitly and `ResetToDefaults()`
after. Pattern to keep: a stochastic trait needs a harness override, not a
hope.

**`onSpreadRequested` -> `onSpawnNear` -> 4-arg `onSpawnNear`.** Two
signature widenings in two commits (add `PathogenClass`, then add
`asFreeParticle`). ~14 harness lambda callsites each time, all
`(c, ...) => false`. `perl -pi` across `game/Assets/Editor/*.cs` is the
fast path; the only hand-edits are the real callers. If a third parameter
ever shows up here, consider a small struct instead.

### Sprint 7 — head session (2026-08-28, ATP economy + round loop framework)

Head session, inline, straight after Sprint 6. Framework pass — every
number a deliberate placeholder — so the emphasis was on the state machine
being right and legible, not on feel.

**Three up-front decisions, asked before writing a line.** Round model
(wave batch + buy phase vs. timed vs. endless), does placement cost ATP
this pass, wire the life pool now or later. All three genuinely changed
what got built, and each was cheap to ask; guessing the round model wrong
would have thrown away the `RoundController` state machine. `AskUserQuestion`
with a recommended option first and a one-line ASCII preview per choice
was the right tool for this — it's the "blocked on a decision that's the
Director's" case exactly.

**The lump sum is paid on round CLEAR, not on StartRound.** §5b's literal
words are "starting a round pays a lump sum", but paying it the instant
the previous round resolves — framed in the HUD as "the budget for
starting the next round" — feels much better (you see the reward for
surviving, then spend it) and still satisfies the text. Recorded in §5b so
it isn't "corrected" back later.

**Round-complete ignores the gut wall on purpose.** A round ends when its
batch is emitted and nothing is in the lumen or tissue — but pathogens
colonising the wall are allowed to persist (§6b), or a single stuck
adherer with a ~1%/roll breach chance would hold a round open for a
minute+. This is `PathogenSpawner.BatchComplete`, and it's the one place
the round loop leans on a design rule that isn't obvious from the code.

**A static hook for the kill payout.** Threading an `AtpWallet` reference
through `BoneMarrowManager → SearchUnit →` every emitted unit, purely so
`RegisterKill` can add 3 ATP, is a lot of plumbing for a one-liner.
`EconomyHooks.PayForKill` (static `Action`, set by `GameBootstrap`, null
in a harness) matches what the project already does for shared services
(`DegranulationFlash.Configure`, `CytokineToggle`, `RuntimeSprites`). The
harness that tests it just points the hook at a test wallet.

**Nullable wallet = free placement kept the lifecycle harness untouched.**
`BoneMarrowManager.Initialize` grew an optional `wallet` param (default
null); `PlaceTower` only charges when it's non-null. `LifecycleVerification`'s
9-arg call still compiles and still gets free placement, no edit. Optional
params are a clean way to add an economy gate without a harness sweep.

### Sprint 8 — head session (2026-08-29, the DC shuttle + antigen barcode)

Head session, inline, one sitting, straight after Sprint 7. Biggest sprint
yet (a new namespace, two new agent types, a second arena, a `UnitKind`
enum widening) — the commit-green-after-each-item discipline held for the
sixth sprint running, and the full regression (six prior harnesses) was
re-run at least once mid-sprint, not just at the end.

**Five up-front decisions again (`AskUserQuestion` x2).** Scope depth
(slice vs. +barcode vs. +capability), DC as a bought tower vs. auto-emit,
node search (random walk vs. second cytokine field), then the match rule
(exact vs. Hamming) and the slot budget. Same as Sprint 7: each genuinely
changed what got built, each cheap to ask. Director picked +barcode,
bought tower, second cytokine field, Hamming ≤ 2, shared 5 slots.

**An interface stand-in kept every commit compiling across a split
feature.** `LymphNode`'s pairing logic (item 3) needed to reference the
dendritic cell, which didn't exist until item 4. `interface INodeVisitor`
(NodePos / Cargo / HasCargo / Frozen / OnPairingResolved) let the whole
node — pairing, freeze, knowledge increment — be written and committed a
commit early; `DendriticCell : MonoBehaviour, INodeVisitor` just slotted
in. Worth reaching for whenever a feature splits across commits.

**The node arena reused `CytokineField` + `Chemotaxis` wholesale** by
giving `LymphNode` its own tiny `BoardConfig` via `ConfigureForTest(6, 6,
…)`. No new movement code. Sprint 4's flat-`strength/(1+distance)`-at-scale
finding is a non-issue at 6×6 — the gradient is steep across a small grid,
which is the whole reason it worked on the 30×5 board and not Map 01.
`BoardConfig.FineToWorld` is centre-on-origin though, so the node needed
its own `NodeToWorld` mapping fine tiles into the lymph backdrop rect.

**`KillHostCell(coord, PathogenClass? antigen = null)` with a
`antigen ?? resident?.Class` fallback meant the two loud-kill call sites
(stress-sense, degranulation collateral) needed ZERO changes** — the
resident is still attached when they call in, so the fallback picks up the
right antigen. Only the detach-before-kill paths (`BurstBrood`,
`BurnOut`) had to pass `Class` explicitly. Applied Sprint 6's
"grep the harnesses for the old verb" lesson: `grep -n "KillHostCell\|
DamageHostCell"` first, then decide per call site.

**`UnitKind` going 2 → 4 values: the `kind == Macrophage ? X : Y`
ternaries in `BoneMarrowManager` were landmines** (they'd silently treat
DC/HelperT as Neutrophil). Converted `PriceFor`/label/colour to switches,
and guarded `Emit` to branch to the adaptive path *before* it reaches
`ProfileFor`/`PoolFor`. The innate-only harnesses never pass the new
kinds, so they stayed green untouched (optional `AdaptiveDirector` param,
unreachable branches).

**Making the end-to-end shuttle test deterministic without controlling
private state.** A helper-T's `Tag` is `Antigen.RandomTag()` set in
`Initialize` (no setter); the DC's cargo comes from the debris antigen.
Rather than fight that: (1) pre-seed the *entire tissue band* with debris
of the target species so the DC samples on tick 1 wherever its random
lane put it, and (2) force `MatchMaxHammingDistance` to 8 (every pairing
teaches) or −1 (none do) for the two sub-cases. Assert exact knowledge
delta (`== KnowledgePerMatch`) with `DcPresentationsPerCargo = 1` so
there's exactly one pairing. 34/34 first run.

**The adaptive arena runs on its own clock.** `AdaptiveDirector.Tick(dt)`
owns the one tick gate, sub-steps `LymphNode.Step(Clock)` + every fielded
DC's `SimulationTick(Clock)`. `LymphNode` lost its own accumulator (was
`Tick(dt,now)`, became `Step(now)`) so there's no second accumulator to
drift. A harness drives it by calling `Tick(0.12f)` in a loop.

### Sprint 9 — head session (2026-08-29, the round-model rework)

Head session, inline, straight after Sprint 8 was pushed — Director
playtested and sent a bundle of notes that added up to one coherent
rework (frozen buy phase, persistent field, themed food-item delivery,
2x difficulty). Two `AskUserQuestion` rounds up front again; the Director
skipped the "what ends a round" fork and instead proposed the food-item /
tagline idea, which answered it (a round ends when the food finishes
delivering).

**One static did the whole freeze.** `RoundClock { bool Frozen; float
Time }` + a 3-line driver. Every `Update()`-driven sim system got two
lines: `if (RoundClock.Frozen) return;` and `Time.time` -> `RoundClock.
Time`. The clock swap matters — just skipping ticks while `Time.time`
keeps running would fast-forward infection ramps / burnout timers the
moment you unfreeze. `GutInterface`'s roll clock still sees a jump
(flagged), because it's driven through the spawner's `currentTime` rather
than owning a frozen-aware clock — a per-position roll clock is the real
fix, deferred.

**The Update()-only gate is not headlessly testable.** Editor batchmode
never runs `Update()`, so `if (RoundClock.Frozen) return` can't be
asserted in a harness — same limitation as every other `Update()`-only
path in this project (`DegranulationFlash`, the tween loops). Covered by
the build launch sitting genuinely still. What the harness *can* test is
the flag/clock state machine and the `RoundController` transitions that
set it, which `RoundVerification` does.

**Kept `BeginBatch` beside the new `BeginRound`.** The food-item path
(`BeginRound`) changes what "batch complete" means — delivered vs.
field-clear. Rather than rewrite every `EconomyVerification` round-loop
assertion, `BeginBatch(int)` keeps its exact old semantics (no food,
field-clear completion) for the harnesses, and `BatchComplete` branches
on a `foodRound` flag. `EconomyVerification` stayed 47/47 untouched; only
`RoundController` (the game's path) moved to `BeginRound`.

**A tuning change broke a harness assertion sitting on a knife-edge.**
`AdhesionChanceAtWall` 0.12 -> 0.30 made `MapVerification` 4c's
"depth-blind curve doesn't adhere *everything*" assertion fail (400/400).
Turned out it was already borderline at 0.12 (~397/400) — the test pins
the falloff *shape*, not the rate, so the fix was to pin
`AdhesionChanceAtWall` low (0.03) for that sub-test so neither curve
saturates. Grep-the-harness-when-a-number-moves (Sprint 6 lesson) caught
it on the first sweep.

**Food-item delivery test determinism.** The "not complete while the food
is still in the lumen" assertion needs the loop to stop the *exact* tick
`BatchEmitted` hits the target — the last burst lands at ~0.8 of transit,
leaving ~6s where all cargo is out but the food hasn't exited. Overshoot
that window by ticking too long and `foodExited` flips and the assertion
is meaningless. `for (...; BatchEmitted < expected; ...)` stops on the
right tick.

### Sprint 10 — head session (2026-08-29, DC patrol lane-repulsion)

Small follow-up after the Sprint 9 playtest. Director's idea: instead of
debris homing, have DCs repel each other **only along the cross (lane)
axis** so they spread across lanes and sweep back and forth, while the
base↔lumen threat axis stays a plain random walk. ~40 lines: a
`RepelledPatrolStep` that softmax-weights the two cross-direction
candidates by `exp(k · dir · crowd)` where `crowd` is a
`sign/(1+dist)` sum over the other fielded DCs; the cohort list is
`AdaptiveDirector.allDcs` handed in through `Initialize`.

**The A/B metric mattered.** First cut asserted "co-lane ticks drop" —
but with only 3 DCs on 10 lanes, random collisions happen ~28% of ticks
even with repulsion working perfectly, and the initial all-in-one-lane
transient front-loads both arms, so the gap was 82 vs 97 (real but thin).
The decisive signal was **mean pairwise lane spread**: 12.4 with
repulsion vs 6.2 without, on an 18-max scale. Kept both, asserting the
direction of each plus a loose absolute — but if you're A/B-testing a
spreading behaviour, measure the spread, not the collisions.

### Sprint 11 — head session (2026-08-29, placeholder shop + knowledge ladder)

Framework pass: a shop and the §5 ladder, both mostly display-only, plus
one real mechanic (neighbour-accelerated regrowth). Broad but shallow —
the placeholder ledgers (`ShopLedger`, `Slot.UpgradeLevel`) are ~20 lines
each; the value is in `GAME_DESIGN.md §1d` capturing the Director's
dsRNA-sensor design (a sensed cell has a ~20% chance to self-destruct and
release a DC-recruiting cytokine -- which implies a *third* cytokine
field, worth flagging loudly).

**A tuning change broke a harness assertion again -- same shape as Sprint
9.** Neighbour-regrowth (an Empty cell regrows faster with more Healthy
neighbours) failed `TissueVerification`'s "hasn't regrown before the
20s period" -- the test cell had 4 healthy neighbours so it regrew in
~7s. Fix: pin `NeighbourRegrowthBonus = 0` in that sub-test (it's about
the base per-cell clock) and put the neighbour A/B in the new harness.
The pattern holds: **any default that a harness assumes, a later sprint
will move -- either pin it in the test or assert the direction, not the
absolute.**

**"Placeholder" as a first-class state.** The Director explicitly wants
buy options visible and clickable *before* they do anything. `ShopLedger.
TryBuy` returning true + spending ATP + bumping a level, with a code
comment that says "no side effect beyond the ledger + wallet", is the
whole contract. The harness asserts the *negative* -- `UpgradeTower`
leaves `UnitLifecycleTuning.KillLimit` unchanged -- so a future sprint
that wires the real effect will trip that assertion and know to update it.

### Sprint 12 — head session (2026-08-29, cytokine on-by-default + the DC movement fix)

Two Sprint 11 playtest fixes.

**"Always on, buy the improvement" is a clean pattern for a binary
upgrade the player always takes.** Cytokine sensing was an on/off debug
toggle the Director flipped on every game. Making it default-on and
turning the *purchase* into a repeatable sharpen (`Chemotaxis.
SensingUpgradeLevel` → `EffectiveSharpness`) keeps rung 2 in the game
without a dead "do I turn this on" decision. The `C` toggle stays for the
rung-1-vs-2 demo. This one shop item is a REAL effect while the rest of
the Sprint 11 shop is placeholder -- worth a `(REAL)` tag in the UI so a
playtester isn't confused about which do anything.

**A movement bias that compares COARSE indices barely fires.** Sprint
10's DC lane-repulsion looked right in the harness (spread 12.4 vs 6.2)
but was invisible in play. Root cause: it computed `CrossIndex(from.
ToCoarse(7))` for a *fine* step, so the index only changed when a step
crossed a coarse-cell boundary -- ~1 step in 7. The fix was
`BoardConfig.FineCrossIndex` / `FineAxisIndex` (fine-tile analogues) and
comparing those. After the fix the same test reads 16.7 vs 2.9 and
co-lane ticks 2 vs 158. **Lesson: if a fine-lattice walker is supposed to
respond to a gradient/field, compute the response at fine granularity --
a coarse comparison silently throttles it 7x.**

**The harness passed the whole time.** 250 ticks x 2 steps = 500 steps /
7 ≈ 71 boundary crossings was *just* enough signal for the directional
assertions (`coOn < coOff`) to pass on 3 forced-together DCs. A harness
that asserts "A beats B" can be green while the effect is far too weak to
see. Where it matters, assert a magnitude floor too (the sweep test does:
"reaches within 2 of each band edge").
