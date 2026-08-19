# Agent Handbook

Curated and evergreen, distilled from `TEAM_RETRO.md` by the head session
(or a dispatched Feedback agent). See `WORKFLOW.md` Section 6.2 for the
intended format. Every brief to a dispatched agent should point at this
file alongside `CLAUDE.md` and its own relevant status doc.

## Tips & tricks

- **Always invoke .ps1 scripts as `powershell -ExecutionPolicy Bypass -File <path>`.**
  Direct invocation (`.\script.ps1` or `& "path"`) hits this machine's
  default execution policy and fails -- and running as administrator does
  NOT fix it, that's a separate setting from elevation.
- **Unity 6 Hub calls the WebGL module "Web Build Support,"** not "WebGL
  Build Support." Check for that exact label before assuming it's missing.
- **Don't trust `$LASTEXITCODE` after a batchmode `& Unity.exe ...` call**
  on this setup -- it can come back null even on success. Check a real
  output artifact (e.g. `ProjectSettings/ProjectVersion.txt`, or grep the
  log for "Exiting batchmode successfully") instead.
- **A quiet-looking WebGL build log usually isn't stuck.** First WebGL
  builds are slow (30+ min: IL2CPP -> Emscripten -> wasm) and the Editor
  log goes quiet well before the build actually finishes. Check the log's
  mtime and for newer files under `Library/Bee/artifacts` before assuming
  it crashed.
- **Local WebGL testing needs `tools/serve_webgl.ps1`, not a generic static
  server.** `file://` URLs can't load Unity's fetch-based loader, and even
  `python -m http.server` doesn't set the `Content-Encoding: gzip` header
  Unity's compressed build output needs.

## Known points of difficulty

- **(Retired 2026-08-19) Device-bridge git commands strand lock files.**
  Applied only to the old Claude desktop device-bridge setup, which is no
  longer how this project is worked on — sessions now run natively with a
  real shell and real `rm`. Left here as a historical note in case a
  device-bridge-style connection is ever reintroduced; the `_to_delete/`
  convention it required is otherwise obsolete.

## Dispatch practices

As of 2026-08-19, `WORKFLOW.md` was rewritten around a single head session
that dispatches focused Code/Design/Feedback subagents directly (via the
`Agent` tool) rather than several separate, persistent Claude Code sessions
coordinating through a device bridge and per-role worktrees — see
`WORKFLOW.md` Section 1 for why. Practices to carry forward:

- Brief every dispatched agent like a cold start — point it at `CLAUDE.md`,
  this file, and whichever status docs are relevant to its task. It has no
  memory of prior sprints even if "the same kind of agent" did similar work
  before.
- Dispatched agents generally can't message each other directly (no shared
  context) — cross-cutting questions get resolved by the head, same
  practical effect as the old "route through the Producer" default, just
  now structural rather than a convention.
- No exceptions established yet beyond that — there isn't enough history
  under the new model to justify one.
