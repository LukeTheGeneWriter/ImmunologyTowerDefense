# Session Handoff — 2026-10-03 (second dev machine stood up, Unity upgraded)

A checkpoint written by the head session. **This is a living file —
overwrite it at the next checkpoint rather than accumulating dated copies.**
Not one of `WORKFLOW.md` §3's canonical docs; if it contradicts
`SPRINT_PLAN.md`, `ENGINE_STATUS.md`, or `INTERFACE.md`, those win.

Written to be read cold by a session on **either** machine.

## First: which machine are you on?

The project now has two first-class dev machines, both the Director's
(`CLAUDE.md` → "The essentials"):

| | Windows | Linux (CachyOS) |
|---|---|---|
| Clone | `C:\Users\lukef\ImmunologyTowerDefense` | `~/Projects/Biotech/ImmunologyTowerDefense` |
| Shell | PowerShell | fish (scripts are bash, run directly) |
| Unity | `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` | `~/Unity/Hub/Editor/6000.6.4f1/Editor/Unity` |
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` |
| Serve WebGL | `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/serve_webgl.py` |

**`git pull` before doing anything.** GitHub is the only sync point; the
other machine may have pushed since you last looked.

## What happened this session (commit `3b8cec1`)

No gameplay changed. This was infrastructure only:

1. **Linux machine set up.** Cloned, Unity Hub (AUR `unityhub`), .NET SDK,
   Microsoft VS Code (`visual-studio-code-bin` — the Arch `code` package is
   Code-OSS, which can't install the C# Dev Kit / Unity extensions),
   with C# Dev Kit + Unity extensions installed.
2. **Linux tooling added.** `tools/unity.sh` (one Editor launch per
   `-executeMethod`, logs to `game/Logs/<Method>.log`, pass/fail by log
   grep — never the exit code; shorthands `verify` and `webgl`) and
   `tools/serve_webgl.py` (gzip `Content-Encoding`, same as the `.ps1`).
3. **Unity upgraded `6000.5.8f1` → `6000.6.4f1`.** The Hub doesn't offer
   `6000.5.8f1` on Linux; the Director chose to upgrade rather than pin.
   Tested on a throwaway copy first: **all 410 assertions + BootstrapSmoke
   green, no compile warnings.** The diff is Unity bookkeeping only
   (`multiplayer.center` 1.0.1 → 2.0.1, new built-in `tetgen` module, a
   reordered ProjectAuditor key, `ProjectVersion.txt`).

## Open items from the move

- **The Windows machine is not upgraded yet.** Before opening the project
  there: install `6000.6.4f1` via the Hub (with "Web Build Support"),
  then `git pull`. Opening it in `6000.5.8f1` would try to *downgrade* the
  project — don't.
- **No player build has been made on `6000.6.4f1` yet**, on either
  machine. Harnesses are green, but a WebGL (and on Windows, a desktop)
  build + headless launch should be the first verification on the new
  editor. First WebGL build on Linux will be the slow full IL2CPP →
  Emscripten pass (30+ min; quiet logs aren't a hang).
- **Windows-only harness tooling stays Windows-only.** The agentic
  playtest in `AGENT_PLAYTEST_01.md` drove input via Win32
  (`SetCursorPos`, DPI awareness); none of that ports to Linux/Wayland as
  is. Run agentic playtests from the Windows machine until someone builds
  a Linux equivalent.
- `tools/unity.sh` was exercised against the harnesses but **not yet with
  `webgl`** — treat the first Linux build as also testing the script.

## Where the game itself stands

Unchanged since 2026-09-04 — see `SPRINT_PLAN.md` (Sprint 17) and
`CHANGELOG.md`:

- **Sprint 17 is code-complete; only the Director's playtest remains**
  (smooth DCs, villi/velvety lumen, cartoon vessel, bolus contrast).
  Nothing in it is headlessly testable.
- **The obvious Sprint 18** is wiring the upgrade rows to the simulation —
  they are still `GAME_DESIGN.md` §6d placeholders, the widest gap in the
  game. Also waiting on the Director's eye: the DC's lateral zigzag walk
  (a design question, deliberately left alone — `BACKLOG.md`).

## For whichever session picks this up

Read `CLAUDE.md` and `WORKFLOW.md` first as always, then the living docs
it lists. Suggested opening move: pull, build WebGL on `6000.6.4f1`, serve
it, and hand the Director the URL for the Sprint 17 playtest — that
settles both the upgrade and the sprint in one sitting.
