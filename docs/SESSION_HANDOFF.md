# Session Handoff — 2026-08-21 (Sprint 4 built and verified, awaiting playtest)

A checkpoint written by the head session. **This is a living file —
overwrite it at the next checkpoint rather than accumulating dated copies.**
Not one of `WORKFLOW.md` §3's canonical docs; if it contradicts
`SPRINT_PLAN.md`, `ENGINE_STATUS.md`, or `INTERFACE.md`, those win.

## Status: Sprint 4 is code-complete, verified, and documented

Map 01's geometry and the invasion loop are built. All verification was run
by the head session — see `ENGINE_STATUS.md` → "Build status (Sprint 4)".
**The remaining step is the Director's playtest.**

## Both dispatched agents so far have hit their usage limits mid-sprint

Sprint 3's agent committed working code but no docs. **Sprint 4's agent
committed nothing at all** — ~1,600 lines of uncommitted, non-compiling
working tree, no verification harness, no docs. The head session repaired
it, wrote `MapVerification.cs` (71 assertions), found a serious bug, and
wrote every doc.

**The standing instruction for the next brief** (recorded in
`TEAM_RETRO.md`): tell the agent to **commit after each scope item**, even
if incomplete. "Write docs as you go" was already in Sprint 4's brief and
still produced nothing, because the agent batched the commit too.

## The two things the Director should look at

1. **Does the invasion loop read?** Watch one spot on the gut wall: pathogens
   should visibly pile up there, then all burst into the tissue at once.
   That build-then-burst is the sprint's whole question and **nobody has
   actually watched it happen** — the counters and the harness prove it
   occurs, but the sight of it is unverified.
2. **Does anything reach the base?** In a 60s unattended run nothing crossed
   the 50-cell tissue band. Expected at a 1s step interval, but it means the
   endzone is unproven live and the pacing is worth a look.

## Findings recorded rather than fixed (mechanics-first instruction)

- **Cytokine sensing is much weaker at map scale.** 30×5: OFF ~3, ON
  converges to 0 within a minute. 100×40: OFF flat at ~47, ON only trends
  45.29 → 37.38 over 2.5 minutes. Not a regression — the 1/r field is steep
  at 3 cells and flat at 47 — but Sprint 1 built the entire upgrade ladder
  on this mechanic feeling transformative, so it needs an answer eventually.
- **The 8.35 ms frame cost at 4,000 cells is vsync-capped**, so it is an
  upper bound, not a measurement. Re-measure with vsync off before Sprint 5
  adds host-cell state rendering.
- **The base band is visually crowded** — marrow slots, lymph node, and HUD
  overprint. Plausibly the first real job for a dispatched Design agent,
  which this project has never used.

## The bug worth knowing about

The scene asset still carried Sprint 1's serialized `columns: 30`, which
overrode Map 01's `columns = 100` default. Because the outer bands clamp
against axis length, the shortfall landed entirely on the middle: **25 base
+ 5 lumen + 0 tissue.** The build ran, rendered, and logged nothing while
being completely unplayable. Fixed, plus `GameBootstrap.WarnOnDegenerateBands`.

**`MapVerification` could not have caught it** — it builds boards via
`ConfigureForTest` and never loads the scene. Worth remembering when the
next "the harness is green so it works" moment arrives.

## Next up: Sprint 5

Already scoped in `SPRINT_PLAN.md`'s split: the tissue state model —
host cells as healthy/infected/dead with two-layer lattice occupancy
(`GAME_DESIGN.md` §1c), which then unlocks §1b step 4's class-specific
advance (viral diffusion that dies without a host, intracellular bacteria
entering and leaving cells), plus debris and the real 100-life pool.
`TissueGrid` still holds exactly one pathogen per coarse slot with no
host-cell concept — that is the rewrite.

Also now designed and waiting: debris rules and the antigen-presentation
spectrum (`GAME_DESIGN.md` §1c) — macrophages clear debris and present it
inefficiently, dendritic cells shuttle it efficiently, passive drainage
into the lymph node is a knowledge sink, and "don't eat me" signals are
flagged in `BACKLOG.md` as the eventual tuning lever.
