# Agent Handbook

Curated and evergreen, distilled from `TEAM_RETRO.md` by the head session.
See `WORKFLOW.md` Section 6.2 for the intended format. Every brief to a
dispatched agent should point at this file alongside `CLAUDE.md` and its
own relevant status doc.

Last distilled: end of **Sprint 11** (2026-08-29), covering Sprints 1–11.

## Tips & tricks

### Environment / build (properties of this machine)

- **Always invoke .ps1 scripts as `powershell -ExecutionPolicy Bypass -File <path>`.**
  Direct invocation (`.\script.ps1` or `& "path"`) hits this machine's
  default execution policy and fails — running as administrator does NOT
  fix it, that's a separate setting.
- **Unity 6 Hub calls the WebGL module "Web Build Support,"** not "WebGL
  Build Support."
- **Don't trust `$LASTEXITCODE` / `$?` after a batchmode `Unity.exe` call.**
  It can come back null/false on a clean success. Check a real artifact:
  grep the log for `Exiting batchmode successfully`, or check the build
  output size / `errors: 0` line the harness/build script prints.
- **A quiet WebGL build log usually isn't stuck** — first WebGL builds are
  30+ min (IL2CPP → Emscripten → wasm) and go quiet early. Check log mtime
  / `Library/Bee` before assuming a crash.
- **Local WebGL testing needs `tools/serve_webgl.ps1`** — `file://` can't
  load Unity's fetch loader and `python -m http.server` doesn't send
  `Content-Encoding: gzip`.
- **`.meta` files must be committed** for every new script and folder
  (Unity generates them on import). Add them in the same commit as the
  `.cs` — a run that imports the project generates any that are missing.
- **`Application.runInBackground` is off (Unity default)** — an unfocused
  build stops ticking. A pixel-identical screenshot across a long wait
  means "lost focus," not "crashed." Scripted input to a running build
  needs the machine otherwise idle (Windows foreground lock). `PrintWindow`
  + `PW_RENDERFULLCONTENT` captures an unfocused window; call
  `SetProcessDPIAware()` before any `GetWindowRect` on this 150%-scaled
  display or the pixel math is off.

### Architecture patterns that have paid off (follow these in new code)

- **Explicit-time simulation surfaces.** Every core class takes
  `deltaTime` / `currentTime` as parameters and reads **no
  `UnityEngine.Time`** — `TissueGrid.Tick`, `CytokineField`,
  `Chemotaxis.ChooseNextStep`, `BoneMarrowManager.Tick`,
  `PathogenAgent.SimulationTick` / `TickCombat`, `SearchUnit.SimulationTick`,
  `GutInterface.Tick`, `RoundController.Tick`, `PathogenSpawner.Tick`,
  `AdaptiveDirector.Tick`, `LymphNode.Step`. `Update()` only forwards the
  clock and drives the visual tween. **This is the single pattern that
  makes headless verification possible** — new sim code must follow it.
  (Sprint 9: `RoundClock.Time` is the frozen-aware clock the `Update()`s
  now pass instead of `Time.time`, so a buy-phase pause doesn't
  fast-forward infection ramps.)
- **The axis frame** (`BoardConfig.OffsetInAxisFrame` / `AxisIndex` /
  `CrossIndex` / `CoarseFromAxis`). No movement code hardcodes a world
  direction — "toward the base" is `dAxis -1`, whatever that means in
  world space. `MapVerification` proves it on a mirrored board. New
  movement code (pathogen advance, DC travel, DC lane-repulsion) all uses
  it.
- **Tuning = mutable statics grouped per system**, never `const`:
  `InvasionTuning`, `TissueTuning`, `EconomyTuning`, `AdaptiveTuning`,
  `ShopTuning`, plus `Chemotaxis.GradientSharpness` and the per-tower
  `UnitLifecycleTuning`. Each has `ResetToDefaults()`. A harness that
  overrides one **must reset after** (leaks into the next group otherwise).
- **Optional trailing parameters** add a capability without a harness
  sweep: nullable `wallet`, `adaptive`, `cohort`, `shop`, `antigen`,
  `lumenCellOverride`, `classOverride`. Prefer this to widening every call
  site. Every prior harness keeps compiling.
- **Static hooks for shared services** rather than threading a reference
  through the whole tree: `DegranulationFlash.Configure`,
  `CytokineToggle.Enabled`, `EconomyHooks.PayForKill`, `RoundClock`. All
  null-safe / default-safe so a harness skips them.
- **An interface stand-in unblocks a feature split across commits**:
  `INodeVisitor` let `LymphNode` pairing be written and committed a commit
  before `DendriticCell` existed.

## Known points of difficulty (recurring — with the standing fix)

- **A harness assertion that leans on a tuning default breaks when a later
  sprint moves that default.** Hit at least 3× (Sprint 3 contact radius,
  Sprint 9 `AdhesionChanceAtWall` 0.12→0.30, Sprint 11
  `NeighbourRegrowthBonus`). **Fix:** in a test that's about a *different*
  thing, pin the moved value explicitly (`InvasionTuning.AdhesionChanceAtWall
  = 0.12f`); and where you can, assert the *direction* of an effect (A vs
  B), not an absolute number sitting on a knife-edge.
- **When a contract / method verb changes, grep every harness for the old
  name** — don't just recompile and trust it. Sprint 6 (`GetAttackableAt`
  went occupant-only), Sprint 8 (`KillHostCell` gained an arg) both had
  harness assertions testing the retired behaviour that still compiled.
- **`MonoBehaviour.Awake()` does not fire from `AddComponent()` outside
  Play Mode.** A headless harness that `AddComponent`s something and
  expects `Awake()`-built state gets a `NullReferenceException`. Use a
  lazy-init guard (`PrefabPool.EnsurePool()`, called from `Get()` too) or
  an explicit init method.
- **`Update()` never runs in Editor batchmode** — so anything that only
  happens in `Update()` is NOT headlessly testable: visual tweens, the
  `RoundClock.Frozen` early-returns, IMGUI panels (the shop, the marrow
  picker, the HUD), `DegranulationFlash`. Those are the build-launch's
  job. Write the *logic* behind them as an explicit-time method the
  harness can call, and gate/tween in `Update()`.
- **The scene is still `Assets/Scenes/Sprint1.unity`** and carries real
  serialized state (`columns: 25`; only 5 of `UnitProfile`'s fields are in
  the YAML, the rest come from `GameBootstrap`'s initializer). **A new
  serialized field's C# default is NOT used if the scene predates the
  field** — a stale `columns: 30` once silently zeroed the tissue band
  (Sprint 4). Check the scene asset, not just the C# default.
- **Dispatched Code agents hit their usage limit mid-sprint** on Sprints
  3, 4 and 5 — one left an uncommitted, non-compiling tree. See Dispatch
  practices below; the current answer is "the head does it inline."

## Process practices (proven across 11 sprints)

- **Ask the genuine forks up front with `AskUserQuestion`** — recommended
  option first, a one-line ASCII preview per choice. Sprints 7/8/9/11 each
  opened this way and *every* answer changed what got built. Cheap to ask,
  expensive to guess wrong (a wrong round-model guess would have thrown
  away a state machine).
- **Commit after each scope item — even incomplete, even ugly.** The
  three interrupted-agent sprints were survived only because code was
  committed with **verbose "explain why" commit messages**, which were the
  primary recovery artifact. Treat both as hard rules.
- **Update docs incrementally**, not in a final sweep: `INTERFACE.md` as a
  signature changes, `TEAM_RETRO.md` when a judgment call is made. "Docs
  written last are the docs that don't get written."
- **A framework/placeholder pass is a legitimate sprint shape** (Sprints
  7, 8, 11): build the state machine / data / UI, every number a
  deliberate placeholder, and record the intended mechanics in
  `GAME_DESIGN.md` for the sprint that implements them. Assert the
  *negative* in the harness ("the placeholder upgrade leaves
  `UnitLifecycleTuning` untouched") so the implementing sprint trips it and
  knows to update.
- **One implementation commit that compiles beats five that don't.** When
  a feature genuinely can't be split into compiling pieces (Sprint 9's
  round model touched everything at once), commit it whole with a
  thorough message rather than forcing broken intermediate commits.

## Dispatch practices

`WORKFLOW.md` (rewritten 2026-08-19) describes a head session that
dispatches focused Code/Design/Feedback subagents. **In practice, Sprints
6–11 were all done inline by the head session, and the dispatched agents
on Sprints 3–5 each hit a usage limit mid-task** (Sprint 4's left an
uncommitted non-compiling tree). For a project this size the head-does-it-
inline approach has a much better track record.

If a sprint *is* dispatched:

- Brief the agent as a cold start — point it at `CLAUDE.md`, this file,
  and the relevant status docs. It has zero memory of prior sprints.
- Tell it explicitly: **commit after each scope item** (even incomplete),
  with a reasoning-heavy message; **write docs incrementally**.
- Dispatched agents can't talk to each other — cross-cutting design
  questions come back to the head.
- The head re-verifies everything at integration and runs the
  stopping-point checklist before it reaches the Director.
