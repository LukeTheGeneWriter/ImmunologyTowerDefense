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
  Prior art: `docs/AGENT_PLAYTEST_01.md`, plus the input recipe in
  `AGENT_HANDBOOK.md` ("Driving the built game from a shell"). That recipe
  is **Win32-only**, so on Linux either build an equivalent or, likely
  better, add a **headless strategy harness** that plays rounds through
  the real simulation APIs (place towers, buy, tick) and reports
  outcomes. The explicit-time simulation makes that possible, and it is
  far cheaper than driving pixels. The old agent also suggested
  debug-only keyboard shortcuts for common actions.

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
