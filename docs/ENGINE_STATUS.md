# Engine Status

Rewritten at the end of every sprint by the Code session. This is the seed
version, written before Sprint 0 — the next Code session should treat this
as its starting brief, not as an accurate description of existing code
(there isn't any yet).

## Engine & platform decision

**Unity**, chosen 2026-08-18 over Godot and a web-canvas-first stack.
Rationale (full research in project chat history — worth asking the
Producer to summarize if you need it):

- Closest match to Bloons TD 6's own strategy (Steam-primary, web as public
  face, single codebase exporting to both) — Ninja Kiwi explicitly moved
  away from maintaining a separate web-only codebase when they built BTD6.
- Full C# support in WebGL builds; mature first-party Steamworks
  integration.
- Unity's Runtime Fee controversy is resolved — canceled Sept 2024. Unity
  Personal (free) covers projects under $200k annual revenue/funding, which
  comfortably covers this project for the foreseeable future. Re-check this
  threshold if the project ever starts generating real revenue.

Tradeoff accepted knowingly: Unity's WebGL builds are heavier than Godot's
(~7.7–10.7MB baseline for 2D vs. ~5MB) — acceptable given the above.

## Local dev environment (resolved 2026-08-18)

Unity Hub, the Unity Editor, the Unity CLI, and an activated Personal
license are all installed on the Director's machine ("lukesdecoder"). This
is where Unity Editor work actually happens — the Code session's builds and CLI invocations
should target this machine, either via a local Claude Code session running
there directly, or via the Claude desktop app's device bridge from a
Cowork session. Record which one ends up being used once it's decided —
this line should get more specific as soon as that's settled.

## Current state

Environment is ready; nothing built yet. Sprint 0's job is to produce:

- A Unity project initialized in this repo, building successfully to a
  Windows/desktop target and a WebGL target, each launching an empty scene.
- A pooling utility (generic object pool) ready to back enemies and
  projectiles once combat exists — see the performance requirement in
  `GAME_DESIGN.md`.
- Steam integration stubbed (e.g. via Steamworks.NET or Facepunch.Steamworks
  — pick one and record the choice here), even if it's just app-ID
  plumbing with no real store presence yet.

## Known issues

None yet — nothing exists to have issues.
