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
- **WebGL builds cleanly on `6000.6.4f1` from Linux** (2026-10-13, via
  `tools/unity.sh webgl`, which is now proven). No Windows desktop build
  on the new editor yet.
- **Windows-only harness tooling stays Windows-only.** The agentic
  playtest in `AGENT_PLAYTEST_01.md` drove input via Win32
  (`SetCursorPos`, DPI awareness); none of that ports to Linux/Wayland as
  is. Run agentic playtests from the Windows machine until someone builds
  a Linux equivalent.
- `tools/unity.sh` was exercised against the harnesses but **not yet with
  `webgl`** — treat the first Linux build as also testing the script.

## Where the game itself stands (updated 2026-10-14)

- **Daylight look chosen; Classic retired** (Sprint 21 opener).
- **Sprint 21 shipped, awaiting the Director's playtest**: cartoon
  organs, light UI cards in the screen gutters, shop tray, plate frame
  with band tabs, and the new knowledge HUD. See `SPRINT_PLAN.md` and
  `docs/COSMETIC_PASS_SPEC.md`.
- **Queued:** leukocyte rolling/extravasation, Peyer's patch refinements,
  telling the two bacterial classes apart on the board (`BACKLOG.md`).

## For whichever session picks this up

Read `CLAUDE.md` and `WORKFLOW.md` first as always, then the living docs
it lists. Suggested opening move: pull, build WebGL on `6000.6.4f1`, serve
it, and hand the Director the URL for the Sprint 17 playtest — that
settles both the upgrade and the sprint in one sitting.
