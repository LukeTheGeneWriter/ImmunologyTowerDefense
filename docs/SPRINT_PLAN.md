# Sprint Plan — Sprint 0

## Scope

Stand up the project skeleton. No gameplay yet — the entire point of this
sprint is to prove the pipeline (repo → build → playable artifact → your
review) works before investing in features.

## Brief: Code session

- Initialize a Unity project in this repo (`/game` or similar — pick a
  clean root and record it in `ENGINE_STATUS.md`).
- Configure two build targets: desktop (Windows, matching eventual Steam
  release) and WebGL.
- Both targets should build and launch to an empty scene without errors.
- Stub Steam integration (Steamworks.NET or Facepunch.Steamworks — your
  call, record which in `ENGINE_STATUS.md`) — app-ID plumbing only, no
  real store page needed yet.
- Add a generic, reusable object-pooling utility class. Nothing needs to
  use it yet, but it needs to exist before any enemy/projectile code gets
  written, per the performance requirement in `GAME_DESIGN.md`.
- Update `ENGINE_STATUS.md` to reflect what you actually built (not just
  this plan).
- Append a note to `TEAM_RETRO.md` — anything about the Unity/repo setup
  that the next Code session (or a fresh instance of you) should know.

## Brief: UI session

- Not much to do yet with no gameplay to skin. Optional: review
  `UI_STYLE_GUIDE.md`'s stub and sketch a rough direction (palette, mood
  references) so Sprint 1 doesn't start from zero. Not a blocker for this
  sprint's stopping point.

## Stopping point (definition of done)

- [ ] Desktop build launches to an empty window without crashing.
- [ ] WebGL build loads and runs in a browser without errors.
- [ ] `ENGINE_STATUS.md` reflects the real, current state of the repo.
- [ ] The `/docs` folder matches the file list in `WORKFLOW.md` Section 3.
- [ ] Code has appended at least one note to `TEAM_RETRO.md`.

This is intentionally a low bar — the goal is a working pipeline you can
point at and say "yes, that's real," not an impressive first build.
