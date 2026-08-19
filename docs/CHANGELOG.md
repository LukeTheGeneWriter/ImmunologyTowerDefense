# Changelog

One entry per sprint, written by the Producer at handoff. Appended to,
never rewritten.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Project pipeline stood up: Unity project builds to desktop and WebGL,
Steam app-ID stubbed, object pooling utility in place. Nothing playable
yet — next sprint starts real gameplay.

-->

## Sprint 2 -- 2026-08-19
Bone marrow is now a real, clickable placement area: 5 slots, free
placement of Macrophage or Neutrophil progenitor towers, each emitting
units from the blood edge on its own timer. Lymph node exists as a
labeled placeholder space (not functional yet -- adaptive immunity is
still a sprint or two out). Combat is real: pathogens now come in three
classes (intracellular virus, intracellular bacterium, large bacterium),
contact deals damage, and a depleted pathogen clears back to healthy
tissue. Uncleared virus infections spread to a neighboring cell after an
incubation period -- watch a slow (cytokine-off) search let an infection
spread versus a fast one catching it first.

Director playtested the same build directly and confirmed placement,
combat, and cytokine sensing all read well. Also surfaced the next real
problem: progenitors have no population cap, so active cell count grows
unbounded over time -- scoped into Sprint 3.

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

**Closing update, same day:** first playtest found the cytokine toggle
imperceptible. Root cause was a legibility bug, not a broken mechanism --
fixed by making adhered pathogens genuinely infect their host cell
(continuous cytokine secretion that ramps over ~20s) and switching the
movement bias to weight each direction relative to the best local option
instead of its raw field value. Also added a visible heatmap tint so the
field itself is on screen, not just inferred from behavior. Director
confirmed via his own playtest that the toggle now reads clearly. **Sprint
1 is closed.**

## Sprint 0 -- 2026-08-18
Project pipeline stood up end to end: Unity 6000.5.8f1 project initialized
in `game/`, object-pooling utility and Steam stub in place, build script
producing both targets. Windows build launches cleanly; WebGL build loads
and runs in-browser via a custom local server (`tools/serve_webgl.ps1`).
Nothing playable yet -- next sprint starts real gameplay. Repo linked to
GitHub throughout; several device-bridge/Unity CLI quirks discovered and
documented (see TEAM_RETRO.md and AGENT_HANDBOOK.md).
