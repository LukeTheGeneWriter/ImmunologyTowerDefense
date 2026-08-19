# Sprint Plan — Sprint 2

## Sprint 1 — closed 2026-08-19

Delivered: the tissue lattice, random-walk search, and a cytokine-sensing
toggle that (after a same-day legibility fix — see `docs/CHANGELOG.md`)
the Director confirmed reads clearly in his own playtest. Full history in
`docs/CHANGELOG.md` and `docs/ENGINE_STATUS.md`.

## Direction for Sprint 2 (Director, 2026-08-19)

Two things, in his words: (1) layout changes so there's a place to place
purchased cells — bone marrow for progenitors, a lymph node for adaptive
immunity — and (2) some way for immune cells to actually interact with
pathogens/infected cells, "so we can see some functionality."

## Scope

Both pieces close the loop Sprint 1 left open on purpose: units currently
debug-spawn at random positions and "contact" is just a visual flash with
no consequence. This sprint replaces both placeholders with the real
mechanics, still without the economy layer.

**1. Bone marrow — real placement, replacing debug spawn.**
- A visually distinct area separate from the tissue board (per
  `GAME_DESIGN.md` §1/§2a — bone marrow is its own compartment).
- A small number of open slots. Clicking an open slot places a progenitor
  tower — Macrophage or Neutrophil (the two Sprint 1 already has). Which
  one is a player choice, however that's simplest to expose (e.g. a
  two-button picker) — doesn't need to be polished.
- A placed tower periodically emits a unit that enters tissue from the
  blood-side edge, replacing `GameBootstrap`'s current random debug spawn.
  Rung-1 entry only (uniform, per `GAME_DESIGN.md` §2a's entry table) —
  cytokine-biased entry location is a later rung, not this sprint.
- **No ATP cost yet.** Placement is free. This sprint is about proving the
  placement → emission → tissue pipeline works, not the economy — that's
  a deliberate, separate piece of scope (see `BACKLOG.md`'s round 1
  economy question, still open).

**2. Combat — units can actually clear an infected cell.**
- When a unit is in contact with an infected cell (the existing
  `SearchUnit.CheckContact`/`PathogenAgent.NotifyContact` hook — already
  fires reliably, per `docs/INTERFACE.md`'s open questions from Sprint 1),
  it now deals damage over time instead of just triggering a flash.
- Once an infected cell's health is depleted, the pathogen clears: the
  coarse slot releases (`TissueGrid.ReleaseSlot`, unused until now) and
  returns to plain host tissue, the pathogen returns to its pool.
- Keep this simple — a flat damage rate is fine. Differentiating
  macrophage vs. neutrophil combat behavior (`GAME_DESIGN.md` §4's role
  split — neutrophils are supposed to be strong DPS with collateral
  damage, macrophages more measured) is a reasonable stretch goal if time
  allows, but not required to hit this sprint's stopping point.

**3. Lymph node — placeholder only, not functional.**
- Full adaptive immunity (`GAME_DESIGN.md` §5 — knowledge percentage,
  dendritic cells, T/B cells, the threshold ladder) is a large system on
  its own and deliberately **not** in this sprint's scope.
- What this sprint does add: a visually reserved space for it in the
  layout (labeled, present, empty) so the compartment model is visible on
  screen and the next sprint has somewhere to build into — not a
  functional lymph node.
- **Flagging this explicitly since it's a scope-narrowing call, not a
  literal reading of "a lymph node for adaptive immunity to take place":**
  if the intent was for something adaptive-immunity-related to actually
  function this sprint, say so and this scope should change before work
  starts.

## Explicitly not in scope

ATP/economy, knowledge percentage/adaptive immunity mechanics, fibrosis,
breach cost/lives, multi-depth pathogen descent/burrowing, dendritic
cell/lymph node travel delay, differentiated per-unit-type combat (stretch
goal only, see above). Everything Sprint 1 already built (configurable
board width, per-unit-type step speed, object pooling, the cytokine
toggle and its heatmap cue) must keep working — this sprint extends it,
doesn't replace it.

## Stopping point (definition of done)

- [ ] Bone marrow is a visually distinct area with clickable open slots.
- [ ] Placing a slot lets the player choose Macrophage or Neutrophil.
- [ ] A placed tower periodically emits that unit type, entering tissue
      from the blood-side edge — no more random debug spawn.
- [ ] Lymph node has a visible, labeled, reserved space in the layout.
- [ ] A unit in contact with an infected cell deals damage over time;
      a sufficiently damaged infected cell clears back to healthy tissue.
- [ ] Board width, per-unit-type step speed, object pooling, and the
      cytokine-sensing toggle (with heatmap cue) all still work.
- [ ] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect the new
      systems.
- [ ] `docs/TEAM_RETRO.md` has at least one new note.

The question this sprint answers for the Director: **does placement feel
like a real decision, and does combat give the search loop a satisfying
payoff?** Once both land, decide what's next — likely the economy layer
(ATP, costs) that both of these were deliberately built without.
