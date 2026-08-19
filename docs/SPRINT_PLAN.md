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

**2. Combat — pathogen classes, not one undifferentiated type.**
Per `GAME_DESIGN.md` §4a (new — Director direction, 2026-08-19). Two of
the four classes there are in scope this sprint; the third (large
bacteria) is a light add-on; the fourth (parasites) is deferred.

- **Intracellular pathogens (virus + bacterium, e.g. *Salmonella*).**
  Replace Sprint 1's generic "adhered pathogen" with an infected-cell
  model: the host cell isn't replaced, it's infected and keeps rendering
  as host tissue (already effectively true — `TissueGrid`'s slot doesn't
  currently distinguish, so this is mostly a rendering/identity change,
  not new occupancy logic). No adaptive immunity yet, so units can't target
  the pathogen precisely — contact instead deals damage to the **whole
  infected cell**, and once destroyed the slot clears back to bare host
  tissue (this doubles as tissue damage, worth tracking even if fibrosis
  itself isn't built yet — see `GAME_DESIGN.md` §6/§6a).
  - **Viral spread**: give virus-class infections an incubation timer;
    if not cleared before it elapses, the infection spreads to one
    adjacent uninfected coarse slot. This is the sprint's most important
    piece for validating the whole game's thesis — it's what makes search
    speed (rung 1 vs. rung 2, Sprint 1's whole point) have a visible,
    escalating cost. Bacterial intracellular infections do not spread this
    sprint (virus-specific per the design doc).
- **Large bacteria.** Simpler, and close to what Sprint 1 already built:
  kill-and-occupy a slot outright, visible as itself (no host-cell
  disguise), cleared by direct damage to the pathogen rather than the
  collateral-damage mechanic above. Include if it doesn't meaningfully
  slow down the two items above — this is the smallest addition of the
  three classes in scope.
- **Not this sprint: parasites** (multi-coarse-slot footprint). Real
  structural work to `TissueGrid`'s one-pathogen-per-slot model — tracked
  in `BACKLOG.md` for a later sprint rather than folded in here.
- Keep damage numbers simple (flat rates are fine). Differentiating
  macrophage vs. neutrophil combat behavior (`GAME_DESIGN.md` §4's role
  split) is a reasonable stretch goal, not required for this sprint's
  stopping point.

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

ATP/economy, knowledge percentage/adaptive immunity mechanics (including
precise MHC-I-restricted killing — everything is collateral-damage clearing
this sprint), fibrosis as a real system (tracking tissue-damage numbers is
fine, decay/gameplay consequences are not in scope), breach cost/lives,
multi-depth pathogen descent/burrowing, dendritic cell/lymph node travel
delay, parasites (multi-slot footprint — see `BACKLOG.md`), differentiated
per-unit-type combat (stretch goal only, see above). Everything Sprint 1
already built (configurable board width, per-unit-type step speed, object
pooling, the cytokine toggle and its heatmap cue) must keep working — this
sprint extends it, doesn't replace it.

## Stopping point (definition of done)

- [ ] Bone marrow is a visually distinct area with clickable open slots.
- [ ] Placing a slot lets the player choose Macrophage or Neutrophil.
- [ ] A placed tower periodically emits that unit type, entering tissue
      from the blood-side edge — no more random debug spawn.
- [ ] Lymph node has a visible, labeled, reserved space in the layout.
- [ ] Intracellular pathogens (virus + bacterium) infect a cell without
      replacing it visually; a unit in contact deals damage to the whole
      infected cell over time; a sufficiently damaged one clears back to
      healthy tissue.
- [ ] An uncleared virus spreads to an adjacent cell after its incubation
      period — visibly, so the Director can watch it happen.
- [ ] (If time allows) large bacteria kill-and-occupy a slot visibly and
      clear via direct damage to the pathogen instead of the host cell.
- [ ] Board width, per-unit-type step speed, object pooling, and the
      cytokine-sensing toggle (with heatmap cue) all still work.
- [ ] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect the new
      systems.
- [ ] `docs/TEAM_RETRO.md` has at least one new note.

The question this sprint answers for the Director: **does placement feel
like a real decision, does combat give the search loop a satisfying
payoff, and does viral spread make search speed feel like it actually
matters?** Once these land, decide what's next — likely the economy layer
(ATP, costs) that both placement and combat were deliberately built
without, and/or parasites' multi-slot footprint.
