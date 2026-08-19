# Changelog

One entry per sprint, written by the Producer at handoff. Appended to,
never rewritten.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Project pipeline stood up: Unity project builds to desktop and WebGL,
Steam app-ID stubbed, object pooling utility in place. Nothing playable
yet — next sprint starts real gameplay.

-->

## Sprint 1 -- 2026-08-19
First playable slice: a tissue lattice (configurable-width coarse grid,
7x7 fine sub-lattice for movement), pathogens that enter and adhere across
the board, and two unit types (macrophage, neutrophil) hunting them via a
pure random walk. Press `C` in the running build to toggle cytokine
sensing on/off and compare a biased search against the blind one -- that
comparison is the entire point of this sprint. Board width, and each
unit's fine-tiles-per-tick speed, are tunable without touching code.

Still rough, on purpose: no ATP/economy, no combat (contact just flashes
the pathogen, nothing dies), no multi-depth burrowing (a pathogen picks a
row and sticks there), no bone-marrow placement (units appear at random
starting spots), no art beyond flat-colored squares. All excluded
deliberately -- see `docs/SPRINT_PLAN.md`. This sprint exists to answer
one question before any of that gets built: does the search itself feel
like something, and does the toggle change that. That's a judgment call
only playtesting can make.

Also folded in the large design pass from 2026-08-19 (`docs/GAME_DESIGN.md`
now has the full compartment model, tower lifespan, fibrosis, breach cost,
and the spatial lattice spec this sprint builds against) and restructured
how the project runs (`WORKFLOW.md`, `CLAUDE.md`) -- see those files if
curious, no impact on what's playable.

## Sprint 0 -- 2026-08-18
Project pipeline stood up end to end: Unity 6000.5.8f1 project initialized
in `game/`, object-pooling utility and Steam stub in place, build script
producing both targets. Windows build launches cleanly; WebGL build loads
and runs in-browser via a custom local server (`tools/serve_webgl.ps1`).
Nothing playable yet -- next sprint starts real gameplay. Repo linked to
GitHub throughout; several device-bridge/Unity CLI quirks discovered and
documented (see TEAM_RETRO.md and AGENT_HANDBOOK.md).
