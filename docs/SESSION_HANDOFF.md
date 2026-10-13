# Session Handoff — 2026-10-14 (Sprints 18–21 shipped; next: invasion rework, pathogens, rounds, agent playtests)

A checkpoint written by the head session. **This is a living file:
overwrite it at the next checkpoint rather than accumulating dated
copies.** It is not one of `WORKFLOW.md` §3's canonical docs; if it
contradicts `SPRINT_PLAN.md`, `ENGINE_STATUS.md` or `INTERFACE.md`, those
win.

It is written to be read cold, on **either** machine.

## First: which machine are you on?

| | Windows | Linux (CachyOS) |
|---|---|---|
| Clone | `C:\Users\lukef\ImmunologyTowerDefense` | `~/Projects/Biotech/ImmunologyTowerDefense` |
| Shell | PowerShell | fish (scripts are bash, run directly) |
| Unity | `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` | `~/Unity/Hub/Editor/6000.6.4f1/Editor/Unity` |
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` (14 harnesses) |
| Build / serve WebGL | `BuildScript.BuildWebGL`; `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/unity.sh webgl`; `tools/serve_webgl.py` → http://localhost:8000 |
| Look at sprites | — | `tools/unity.sh SpriteShapeDump.DumpAll` → `_spritedump/` (now includes the organ canvases) |

**`git pull` before doing anything.** GitHub is the only sync point.
**The Windows machine still needs Unity `6000.6.4f1`** (via the Hub, with
"Web Build Support") before it opens the project. Opening it in 6000.5.8f1
would try to downgrade the project.

## What shipped this session (2026-10-13 → 14, all on the Linux machine)

| Sprint | What | Director's verdict |
|---|---|---|
| — | First WebGL build on 6000.6.4f1 (the upgrade and `tools/unity.sh webgl` proven) | — |
| 18 | All 12 progenitor upgrade rows wired; adaptive ones per-tower; clonal expansion also raises the rate (the cap alone was inert) | (moved on) |
| 19 | Peristalsis (surge-and-pause bolus, same transit time); opaque round bolus; velvet villi; Peyer's patch + buyable M-cell sampling; real crypts | "Nice job!" |
| 20 | `Palette.cs`; the "daylight cartoon" look from `docs/ART_DIRECTION.md`; baked outlines; pathogen size floor; F10 A/B | "Looks great. Excellent call on no faces." |
| 21 | Classic retired; cartoon organs; light UI cards in screen gutters; shop tray; plate frame + band tabs; **knowledge HUD** (`docs/COSMETIC_PASS_SPEC.md`) | **"TRANSFORMATIVE."** Loves the flying knowledge tokens; the node looks great; **the bone may need a second pass** |

The process that produced 19–21 is now written up in
`AGENT_HANDBOOK.md` → Dispatch practices: design agent → spec + PIL
mocks → Director decisions via `AskUserQuestion` → inline build, sprites
checked magnified before building. **Reuse it for any visual work.**

## The Director's direction for what's next (2026-10-14, verbatim in `PLAYTEST_LOG.md`)

### 1. Invasion rework: pathogens shouldn't queue in an orderly line at the wall

Today an adhered pathogen sits at one `GutInterface` position, and the
adhered pile is drawn as an orderly stack
(`PathogenAgent.InterfaceStackWorldPosition`). Breach is per
**lane / position** (`GAME_DESIGN.md` §1b, step 2). The Director wants:

- pathogens **floating around an area near a breach spot**, not lined up;
- **secretory IgA** possibly trapping them there (it's §5's ~70% rung,
  not built yet);
- which opens:
  - **proximity-based ingress** in place of lane-based;
  - **mucus sloughing**: §6b's turnover upgrade, made spatial;
  - **barrier-cell death**: a dead epithelial cell lets bacteria through
    but still **blocks viruses**, which need a living cell.

Keep §1b step 2's best property: *pressure builds visibly, then
bursts*. §1b now carries an "under revision" note. **This is a design
change to a LOCKED section, so design it before building it.** Write the
model (what replaces per-position piles; how crowding and breach work
spatially; how IgA, sloughing and dead barrier cells plug in), put the
genuine forks to the Director, then build. Harnesses that lean on the
current model: `MapVerification`, `RoundVerification`, `Sprint19`'s
M-cell sampling (`IsOverDome` uses the near-wall rows) and
`TissueVerification`. Expect to revise them deliberately.

### 2. Then, the next sprints' focus

- **Adding pathogens.** The roster is three classes (`PathogenClass`).
  `GAME_DESIGN.md` §4a describes the class model. Note that the board
  already can't tell the two bacterial classes apart (`BACKLOG.md`,
  Sprint 21), so any new pathogen needs its own silhouette.
- **Planning rounds.** `Rounds/RoundScript.cs` holds the per-round
  definitions and taglines. Design a real progression around the new
  pathogens.
- **Agent playtests to find "broken strategies"**, then sort each into
  "a true biological strategy" (keep it, maybe teach it) or "needs work".
  **Recommended approach: a headless strategy harness**, written up
  below, rather than pixel-driving agents.

### 2a. The headless strategy harness: design notes

**Why not pixel-driving agents.** The Sprint 17 agentic playtest
(`docs/AGENT_PLAYTEST_01.md`) drove the built game through Win32 mouse
calls. That doesn't port to Linux / Wayland. It spent most of a session
fighting input, and it plays one game at real-time speed. Broken-strategy
hunting needs the opposite: **many full runs, fast, reproducible, with
numbers**.

**Why it's feasible.** Every simulation class already takes explicit time
(`AGENT_HANDBOOK.md`: "Explicit-time simulation surfaces"), and the
harnesses already play rounds headlessly. `RoundVerification` ticks
`spawner.Tick`, each `PathogenAgent.SimulationTick` and
`rounds.Tick` in a loop. Every player action is already a plain method:
- `BoneMarrowManager.PlaceTower(slot, kind)`
- `UpgradeTower(slot, row, base, cap)`
- `ShopLedger.TryBuy(item, wallet)`
- `RoundController.StartRound()`

There is no UI in the way.

**The one real piece of work: a single `SimulationStep(dt)`.** Unity
doesn't run `Update()` in batchmode, and today the game's tick is
scattered across **19 `Update()` methods**. Some are cosmetic renderers;
the ones that matter are:
- `RoundClock` (the frozen-aware clock)
- `PathogenSpawner`, `PathogenAgent`, `SearchUnit`
- `BoneMarrowManager`
- `AdaptiveDirector`, `DendriticCell`, `Lymphocyte`
- `TissueDriver`, `RoundController`

There are also two behaviour bridges hiding in the UI:
`UiController.Update` pushes shop levels into `Chemotaxis` and
`GutWallFeatures`. A harness that copies that order by hand will drift
from the real game the first time someone adds a system. So:

1. **Extract one world ticker** (e.g. `Bootstrap/SimulationDriver`) that
   advances every simulation system in a fixed, documented order for a
   given `dt`, including the shop → effect bridges, moved out of the UI.
2. **The game's `Update()`s stop ticking the simulation.** One
   `SimulationDriver.Update()` calls `SimulationStep(Time.deltaTime)`
   while `!RoundClock.Frozen`. The per-object `Update()`s keep only their
   visual tweens.
3. **The harness calls the same `SimulationStep(0.1f)`** in a tight
   loop. Same code path, no drift. `BootstrapSmoke` already builds the
   whole game headlessly, so the harness can boot through `GameBootstrap`
   rather than re-wiring systems.

This refactor has its own value. It makes the tick order explicit,
which today is an emergent property of Unity's `Update()` order. It also
needs the full harness suite plus a played build to confirm nothing
moved.

**Determinism.** 13 scripts call `UnityEngine.Random`. Seed it
(`Random.InitState(seed)`) at the start of each run, and run each
strategy over **N seeds** (say 20). A "broken strategy" must win across
seeds, not get lucky once. If something still varies with the same seed,
that is a determinism bug worth finding anyway.

**Strategies as data.** A strategy is a small script of actions keyed to
game state: "R1: macrophage in slot 0; whenever ATP ≥ 40, buy crypt;
from R3, DC in slot 2 and max CCR7…". Start with a handful of hand-written
archetypes:
- all-neutrophil;
- all-macrophage;
- turtle on crypts + mucus;
- rush the adaptive side (DC + helper-T + M cells);
- never buy upgrades;
- dump everything into one upgrade row.

Then add a **greedy / random-search** player that tries action sequences
and keeps the ones that win. That is what finds the strategies nobody
thought of.

**What to measure per run:**
- rounds survived and lives left;
- ATP earned and spent, by category;
- kills by unit kind;
- breaches by position;
- peak tissue damage and debris;
- knowledge % per species, and the round each rung was reached;
- the time each round took.

Write one CSV/JSON row per run under `game/Logs/strategies/`, plus a
summary table. The Director reads the summary; the raw rows are for
digging.

**What "broken" means.** Flag any strategy that does one of these:
- (a) survives far longer than the median;
- (b) wins while ignoring a whole system (never fields an adaptive cell,
  never buys an upgrade);
- (c) has a degenerate signature: one purchase repeated 20 times, or zero
  breaches ever.

**Each flag goes to the Director to classify:**
- **Real biology:** e.g. a neutrophil swarm early is genuinely how innate
  immunity wins. Keep it, maybe make it a lesson.
- **Needs work:** e.g. infinite crypts make tissue immortal. Rebalance or
  cap it.

That classification is his call; the harness only surfaces candidates.

**Order of work:**
1. The `SimulationDriver` refactor (verify: all harnesses plus a played
   build).
2. Seeding.
3. One strategy end to end.
4. The archetypes.
5. The search player.
6. The report.

It pairs naturally with **round planning**. The harness is also how you
check that a planned round progression is beatable and not trivial.

**Cheap extra from the old agent's suggestion:** debug-only keyboard
shortcuts for common actions (place kind N in slot M, buy item K). They
help the Director's own testing, and any future pixel-level agent.

### 3. Smaller

- **Bone marrow second pass.** Technically correct, but not loved. The
  node is the bar to match. `OrganShapes.BuildMarrow`; mocks and
  reasoning are in `COSMETIC_PASS_SPEC.md` §1.2.
- Still queued from earlier: leukocyte rolling and extravasation (the
  Sprint 18 section of `BACKLOG.md`); Peyer's patch refinements
  (`GAME_DESIGN.md` §1d); the DC antigen dot; idle wobble.

## State of the code (one-liners; details in `ENGINE_STATUS.md` / `INTERFACE.md`)

- **All colours** are in `Rendering/Palette.cs` (Daylight only). New
  colours go there.
- **UI** is UI Toolkit from code (`UI/`): light cards via `UiTheme`, the
  Nunito font in `Assets/Resources/Fonts/`, and screen-gutter docks.
  `KnowledgeView` + `KnowledgeWatcher` drive the knowledge HUD.
- **Organs** are `Rendering/OrganShapes` canvases; UI and marrow sprites
  are in `Rendering/SpriteShapes.Ui.cs`.
- **Gut wall**: `Grid/GutWallFeatures` (Peyer's patch, M cells, crypts),
  `Pathogens/Peristalsis`.
- 14 harnesses; `tools/unity.sh verify` is green as of `515af53`, plus
  this handoff's docs commit.

## For whichever session picks this up

Read `CLAUDE.md` and `WORKFLOW.md`, then this file, then `PLAYTEST_LOG.md`
(the newest entries) and `BACKLOG.md` (Sprints 18–21 sections). Suggested
opening move: **propose the invasion-rework design** in conversation (or
dispatch a design agent if the Director wants mocks of the new wall
behaviour), get the forks answered, and only then touch `GutInterface`.
