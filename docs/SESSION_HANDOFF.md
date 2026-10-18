# Session Handoff — 2026-10-19 (Sprint 22 shipped the invasion rework; next: playtest, rounds, pathogens)

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

## Where Sprint 22 left things (2026-10-18/19, Linux machine)

**Sprint 22 shipped the invasion rework.** `GAME_DESIGN.md` §1b is
rewritten and **re-locked** -- steps 1 and 2 replaced, steps 3-5
untouched. Read that section first; it is the source of truth now.

Two docs underpin it and are worth reading before touching the wall:

- `docs/INVASION_REWORK_SPEC.md` -- the design. **Its §12 carries the
  Director's decisions and supersedes its own fork list**, so read §12
  before trusting anything earlier in the file.
- `docs/IGA_REFERENCE.md` -- the immunology, 36 DOIs, every citation
  resolved against PubMed. It corrected two of the Director's five
  stated claims and one of the head session's recommendations.

### The one idea the whole wall rests on

Under IgA, **a bacterium's own replication is what traps it**. IgA does
not slow bacteria (Moor 2017, Nature 544:498-502): it cross-links
DAUGHTER cells as they divide, because classical agglutination needs
densities real infections never reach. Links break over time (Bansept
2019), so it is a race -- divide faster than your links break and you
build your own cage.

What that buys: **nothing in the game ever decides which pathogen
deserves IgA.** Fast replicators trap themselves hardest; the physics
does the targeting. There is deliberately NO field anywhere for an IgA
penalty on division rate. If a future session adds one, the observable
(free numbers stop growing) will survive and the reason will be a lie.

### Other invariants a later session must not "simplify"

- **`IngressCrowdFloor`.** Charge accrues at `(members - floor) * rate`,
  so one or two adherents accrue nothing forever. A trickle is
  arithmetically impossible, not merely unlikely. `MapVerification.
  RunNoTrickle` exists to stop anyone removing it.
- **One ingress mechanic, not two.** The Director chose "dead wall is a
  weak spot, not a hole": it charges faster, it does not let bacteria
  walk through.
- **No killing in the lumen, anywhere.** IgA excludes, sloughing expels.
  Secretory IgA is non-inflammatory, inhibits IgG-driven complement, and
  the lumen has no patrolling phagocytes. The gut's answer is flush, not
  eat. The "easier to deal with" bonus lives in `ReceiveDamage`, behind
  the wall.
- **Seed the harness.** `Sprint22Verification` calls `Random.InitState`.
  Several mechanics are races, and an unseeded race gives a harness that
  passes alone and fails in the suite -- which is how two real bugs were
  found.

### What is queued next

1. **The Director's playtest of Sprint 22** -- see `SPRINT_PLAN.md`'s
   "what to watch for". The open question is whether the blister reads
   mid-round and whether earning IgA feels like a reward.
2. **Round planning and new pathogens**, which the rework was blocking.
   Both now have a wall model to design against, and the barrier
   asymmetry gives a mixed round a real decision.
3. **The `SimulationDriver` refactor and the headless strategy harness**
   (§2a below, unchanged). Sprint 22 makes this more valuable: the wall
   now has numbers worth sweeping (coat rate, crowd floor, charge rate),
   and several are explicitly flagged as playtest-set.
4. **Bone marrow second pass**, still queued from Sprint 21.
5. `BACKLOG.md` carries eight further real IgA mechanics the research
   surfaced -- blocking plasmid transfer between enemy lineages, IgA1
   protease vs. IgA2, antigenic escape, and more.

## State of the code (one-liners; details in `ENGINE_STATUS.md` / `INTERFACE.md`)

- **All colours** are in `Rendering/Palette.cs` (Daylight only). New
  colours go there.
- **UI** is UI Toolkit from code (`UI/`): light cards via `UiTheme`, the
  Nunito font in `Assets/Resources/Fonts/`, and screen-gutter docks.
  `KnowledgeView` + `KnowledgeWatcher` drive the knowledge HUD.
- **Organs** are `Rendering/OrganShapes` canvases; UI and marrow sprites
  are in `Rendering/SpriteShapes.Ui.cs`.
- **The wall** is `Grid/GutWall` (Sprint 22; `GutInterface` is deleted):
  the mill, the crowd field, plaques, barrier segments, IgA and luminal
  replication. Numbers live in `InvasionTuning`, `IgaTuning` and
  `BarrierTuning`. `Grid/GutWallFeatures` still owns the Peyer's patch,
  M cells and crypts; `Pathogens/Peristalsis` the squeeze.
- **The board is 25 x 10, not 100 x 40** -- ten interface lanes. Corrected
  in `INTERFACE.md` and `BoardConfig`'s comments on 2026-10-18; both had
  asserted 40 for several sprints. `MapVerification` builds its own
  100x40 rig, which is why nothing caught it.
- **15 harnesses, 550 checks**; `tools/unity.sh verify` green as of
  Sprint 22's final commit. `Sprint22Verification` is seeded.

## For whichever session picks this up

Read `CLAUDE.md` and `WORKFLOW.md`, then this file, then `PLAYTEST_LOG.md`
(the newest entries) and `BACKLOG.md` (Sprints 18–21 sections). Suggested
opening move: **ask the Director how Sprint 22 played**, against
`SPRINT_PLAN.md`'s "what to watch for" list, before building anything
new on top of the wall.
