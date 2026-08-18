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

- Unity project created at `game/` — Unity 6000.5.8f1, confirmed via
  `ProjectSettings/ProjectVersion.txt`.
- `Assets/Scripts/Pooling/PrefabPool.cs` — generic pooled spawner wrapping
  `UnityEngine.Pool.ObjectPool<GameObject>`. Not wired into any gameplay
  yet (there isn't any); ready for enemies/projectiles once combat starts.
- `Assets/Scripts/Platform/SteamStub.cs` — placeholder only, no real SDK
  dependency. Real Steamworks.NET/Facepunch.Steamworks integration is a
  follow-up, not Sprint 0 scope.
- `Assets/Editor/BuildScript.cs` — `BuildScript.BuildWindows()` and
  `BuildScript.BuildWebGL()`, each auto-creates an empty scene if needed
  and builds to `Builds/` (gitignored). Not yet run — that's the next step.

## Known issues

None yet — first real build hasn't been attempted. WebGL build support
may not be installed as a Unity Hub module; if `BuildWebGL()` fails on
that, it needs to be added via Unity Hub (a Director/GUI step — see the
device-bridge addendum above).

## Addendum: what the device bridge can and can't do (found during Sprint 0)

The Claude desktop device bridge (used to reach the Director's machine from
a Cowork session) can read and write files in a connected folder — that's
how this repo got linked and how docs land here — but its shell is an
isolated Linux sandbox, *not* the Director's actual Windows shell. It
cannot execute `Unity.exe`, Unity Hub, or any other Windows binary. Any
step that needs the Unity CLI or Editor has to be run by the Director
directly in their own terminal, or by a local Claude Code session running
natively on their machine. File authoring (C# scripts, project config,
docs) can still happen through the device bridge either way.
