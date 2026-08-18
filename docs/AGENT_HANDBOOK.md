# Agent Handbook

Curated and evergreen, owned by HR Claude — distilled from `TEAM_RETRO.md`.
See `WORKFLOW.md` Section 6.2 for the intended format. Every brief to a
fresh Code or UI session should point at this file alongside their own
status doc.

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

- **Device-bridge git commands strand lock files.** Any `git` command run through the Claude desktop device bridge (`device_bash`) tends to leave a stray `.git/index.lock`, `.git/HEAD.lock`, and/or `.git/objects/maintenance.lock` behind, because the bridge can rename files but can't unlink them, and git sometimes needs a plain unlink to release its own lock. Left in place, this makes the Director's own git client fail with "another git process seems to be running." Standing fix: after any git command sequence run through the bridge, do one final pass moving any `.git/*.lock` (and `.git/objects/maintenance.lock`) into `_to_delete/`, as the *very last* step, with no git command after it (since even `git status` can strand a fresh one).

## Contact protocol

Default, per `WORKFLOW.md` Section 1: Code and UI route cross-cutting
concerns through the Producer rather than messaging each other directly.
No exceptions have been established yet — there isn't enough history to
justify one.
