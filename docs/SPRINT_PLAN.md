# Sprint Plan — Sprint 1

## Context

Sprint 0 stood up the pipeline (Unity project, object pooling utility,
Steam stub, two build targets) with no gameplay. Since Sprint 0 closed, a
large design pass landed (2026-08-19) — the full compartment model, tower
lifespan, knowledge ladder, fibrosis, breach cost, and — most relevant to
this sprint — the two-resolution spatial lattice, all now merged into
`docs/GAME_DESIGN.md`. `WORKFLOW.md` was also rewritten the same day: this
project is now run by one head session that dispatches focused subagents
(Code, Design, Feedback) rather than several persistent, separately-started
Claude Code sessions. See `CLAUDE.md` for orientation on that.

## Why Sprint 1 is scoped the way it is

Per `docs/GAME_DESIGN.md` §9 and `docs/handoff-map01-intestine.md` §4: the
random walk in round 1 is the game's central teaching device and the
anchor of the entire upgrade economy. Everything downstream — the
cytokine-sensing upgrade, the value of chemotaxis, whether round 1 reads as
"frustrating-but-legible" instead of "broken" — depends on whether this
core movement/search loop actually feels like something. The economy (ATP,
buy panel, towers) is well-understood tower-defense plumbing and low risk;
the search problem is not, and the design explicitly calls it out as the
single most important thing to get right before building anything on top
of it. So: **build the search problem first, build nothing else.**

## Scope

- The tissue lattice: coarse grid (occupancy — one host cell or one
  pathogen per slot) with a 7×7 fine sub-lattice per coarse slot for
  movement (`GAME_DESIGN.md` §7).
- Host-cell occupancy on the coarse grid.
- Pathogen adhesion (entering at a coarse slot, "sticking") and descent
  (moving toward depth 5 over time/rounds is out of scope for this
  sprint's minimal slice — adhesion + presence is enough; full multi-depth
  descent can follow once movement itself is proven out. Use judgment here
  if a minimal single-layer-plus-adhesion version is clearly enough to
  answer the sprint's core question; don't over-build depth mechanics
  before they're needed).
- Two unit types performing a **pure random walk** (uniform probability
  across the four von Neumann neighbours) through the fine lattice,
  co-occupying host-cell slots as they move.
- A **debug toggle** for cytokine sensing (rung 2: neighbour probability
  weighted by a gradient from adhered/pathogen-occupied slots) so the
  "does this feel transformative" question in `GAME_DESIGN.md` §9 can
  actually be tested against the random-walk baseline.
- **Configurable board width** (coarse columns) as a build/inspector
  parameter — not hardcoded. Per `GAME_DESIGN.md` §7, board width is the
  primary difficulty knob and needs playtesting, not deriving. Start
  somewhere in the 24–40 column range (30 is a reasonable default) and
  make it trivially adjustable.
- **Per-cell step length** (fine tiles per tick, varies by unit type) from
  the start — required for 7×7 subdivision to cost nothing in pacing per
  `GAME_DESIGN.md` §7. Don't hardcode one tile per tick.
- Continue honoring the object-pooling requirement (`GAME_DESIGN.md` §8) —
  pathogens and immune cells go through `PrefabPool.cs`, not raw
  Instantiate/Destroy.
- Draft a first pass of `docs/INTERFACE.md` reflecting whatever data shapes
  actually get built (coarse/fine coordinates, unit/pathogen state) — it's
  been an empty stub since Sprint 0 and there's finally something to
  describe.
- Update `docs/ENGINE_STATUS.md` to reflect what actually got built.
- Append notes to `docs/TEAM_RETRO.md` — anything that was confusing or
  worth flagging for next time.

**Explicitly not in scope:** ATP, buy panel, round-end summary, art/visual
polish beyond whatever makes the lattice and movement legible for a
playtest, bone marrow UI, lymph node, adaptive immunity, knowledge
percentage, fibrosis, breach cost, multiple compartments beyond tissue.
Several of those are LOCKED in `GAME_DESIGN.md` and still should not be
built yet — this sprint answers one question first.

## Stopping point (definition of done)

- [ ] A build the Director can open and watch for ten minutes.
- [ ] The lattice is visible: host-cell occupancy at the coarse level,
      readable at a glance.
- [ ] Pathogens enter and adhere somewhere on the board.
- [ ] Two units move via pure random walk and visibly search/collide with
      adhered pathogens.
- [ ] Flipping the cytokine-sensing debug toggle visibly changes movement
      behavior — biased drift toward pathogen sites instead of uniform
      wandering.
- [ ] Board width is adjustable without a code change (inspector field or
      equivalent).
- [ ] Each unit type has its own configurable step length.
- [ ] `docs/INTERFACE.md` has a real first draft.
- [ ] `docs/ENGINE_STATUS.md` reflects reality.
- [ ] `docs/TEAM_RETRO.md` has at least one new note.

The single question this sprint exists to answer for the Director: **does
the random walk read as frustrating-but-legible rather than broken, and
does cytokine sensing feel transformative when toggled on?** If the answer
is no, the difficulty ladder in `GAME_DESIGN.md` §7/§9 needs rethinking
before any economy work is worth doing — so don't build economy work yet
regardless of how this sprint goes.
