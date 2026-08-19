# CLAUDE.md — Immunology Tower Defense

Orientation for any Claude Code session opening this repo. This file is
versioned and loads automatically — it replaces the old `PRODUCER_NOTES.md`,
which lived outside the repo in Claude Project context and had to be
hand-fed to every new session. If anything here conflicts with `WORKFLOW.md`
or `/docs`, **the repo wins** — this file is a fast-orientation layer on top
of those, not the source of truth itself.

## The essentials

- **Repo:** [github.com/LukeTheGeneWriter/ImmunologyTowerDefense](https://github.com/LukeTheGeneWriter/ImmunologyTowerDefense) (public)
- **Local clone:** `C:\Users\lukef\ImmunologyTowerDefense`
- **Unity project:** the `game\` subfolder inside the clone — not the repo root
- **Read this first, always:** `WORKFLOW.md` at the repo root — the process spec (roles, sprint cycle, git model).
- **Living state, read fresh each session (don't trust memory of these):**
  `docs/GAME_DESIGN.md`, `docs/ENGINE_STATUS.md`, `docs/SPRINT_PLAN.md`,
  `docs/TEAM_RETRO.md`, `docs/AGENT_HANDBOOK.md`, `docs/BACKLOG.md`,
  `docs/CHANGELOG.md`, `docs/PLAYTEST_LOG.md`, `docs/INTERFACE.md`,
  `docs/UI_STYLE_GUIDE.md`

## Who's who

- **Luke — Director.** Sets direction, playtests, gives feedback in plain
  language. He came into this wanting to be director and playtester, not a
  hands-off client — he asks *why*, appreciates honest narration when
  something breaks rather than being told it's "handled," and clearly
  enjoys the infrastructure side as much as the eventual game. Keep
  explaining reasoning, not just outcomes.
- **This session — the head.** Talks to Luke directly, holds the full
  picture, and dispatches specialized subagents (Code, Design/UI, Feedback)
  for focused work rather than doing everything itself or requiring Luke to
  spin up and name separate sessions manually. See `WORKFLOW.md` for the
  current process — it was rewritten 2026-08-19 to reflect this model; the
  older Producer/Code/UI/HR-as-separate-persistent-sessions structure
  (worktree-per-role, contacting sessions by name, a device bridge) is
  retired. Design conversations happen in chat with Luke; implementation
  happens here.
- **Dispatched agents.** Fresh subagents start with zero memory of this
  project every time — brief them like an onboarding doc, pointing at the
  exact docs they need, every single time. Don't assume continuity just
  because "the Code agent" did similar work last sprint.

## Environment

This session runs natively on Luke's machine with real shell access
(Bash/PowerShell) — there is no device-bridge sandbox, no 45-second call
cap, no inability to delete files, no network restriction. The `_to_delete/`
convention and the git-lock-stranding discipline that the old device bridge
required are **obsolete**; a plain `rm` or `git` command works normally.

## Windows / Unity tips (still live — properties of this machine, not of old tooling)

- **Every `.ps1` script must be invoked as**
  `powershell -ExecutionPolicy Bypass -File <path>`. Direct invocation
  (`.\script.ps1` or `& "path"`) hits this machine's default execution
  policy and fails — running as administrator does **not** fix it, that's a
  separate setting from elevation.
- **Unity install:** Unity Hub + Editor `6000.5.8f1` at
  `C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe`, Personal
  license activated. Confirmed current as of 2026-08-19 — re-verify before
  reusing this path if it's been a while, since Unity updates in place.
- **Unity 6 Hub calls the WebGL module "Web Build Support,"** not "WebGL
  Build Support" — don't assume it's missing just because that exact string
  isn't in the modules list.
- **Don't trust `$LASTEXITCODE`** after a batchmode `& Unity.exe ...` call
  on this machine — it can come back `$null` even on a clean success. Check
  a real artifact instead (e.g. `ProjectSettings/ProjectVersion.txt`
  existing, or grep the log for "Exiting batchmode successfully").
- **A quiet WebGL build log usually isn't a dead build.** First builds are
  slow (IL2CPP → Emscripten → wasm, 30+ min) and the log goes quiet well
  before it actually finishes. Check log mtime / newer files under
  `Library/Bee` before assuming it's stuck.
- **Local WebGL testing needs `tools/serve_webgl.ps1`**, not a generic
  static server. `file://` URLs can't load Unity's fetch-based loader, and
  even `python -m http.server` doesn't set the `Content-Encoding: gzip`
  header Unity's compressed build output needs — that script handles it.

## Performance requirement (non-negotiable — see `docs/GAME_DESIGN.md`)

Enemies, projectiles, and effects must be object-pooled from first
implementation, not retrofitted later. `game/Assets/Scripts/Pooling/PrefabPool.cs`
already exists for this.
