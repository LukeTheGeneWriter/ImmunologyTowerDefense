# Sprint Plan — Sprint 5

## Sprint 4 — closed 2026-08-21

Delivered Map 01's geometry and the invasion loop: three lateral bands,
lumen flow, proximity-gated adhesion, wall accumulation, the per-position
breach burst, and base-directed advance. Verified 71/71 new assertions plus
both prior harnesses. **The Director playtested and confirmed the rupture
mechanic reads** — the sprint's whole question, answered yes.

Same-day follow-up, already shipped: the board shrank from 100×40 to
**25×10** (bands 6 | 13 | 6, keeping the proportions), which made units
properly readable and got pathogens actually reaching the base within a
short session.

## Direction for Sprint 5 (Director, 2026-08-21)

The Director named three things for the next stretch — **debris, the
progenitor buying tab, and lymphatic migration** — and approved splitting
them across three sprints, because they form a hard dependency chain:

- **Sprint 5 (this one): host-cell states and debris.** Debris *is* a dead
  host cell, and `TissueGrid` currently has no concept of a host cell at
  all, so §1c's state model has to land first.
- **Sprint 6: ATP economy + the progenitor buying tab**, and very likely
  the round loop, since `GAME_DESIGN.md` §5b's "ATP from starting a round"
  has nothing to attach to without one.
- **Sprint 7: lymphatic migration** — the DC shuttle of §5a. Blocked until
  debris exists, because debris is what a DC picks up.

Read `GAME_DESIGN.md` **§1c** before anything else. It is the authority for
this sprint; this document is the implementation brief for it.

## Scope

**1. Two-layer lattice occupancy — the structural change.** Per §1c, each
coarse position holds **two independent slots**:

- **Host layer:** `Healthy` | `Infected` | `Dead` (debris) | `Empty`.
- **Occupant layer:** extracellular things — a large bacterium, an
  intracellular bacterium currently *outside* a cell, a free virus
  particle between hosts.

`TissueGrid` today holds exactly one `PathogenAgent` per coarse slot and
nothing else. That single-occupant model is what this sprint replaces.

**Why two layers and not one enum** (§1c has the full argument, but it is
the thing most likely to get "simplified" away): the states genuinely
co-occur. A bacterium crawling toward the base passes **over ground that
still holds living host cells** — tissue is packed with cells and bacteria
squeeze between them. "Occupied by bacteria" and "occupied by a healthy
cell" are simultaneously true, and one enum cannot say that.

Immune cells are tracked on the fine lattice and are **not** part of either
layer. Do not fold them in.

**2. Host cells exist and can die.** The tissue band starts full of
`Healthy` host cells. An infected cell that is cleared, or a cell damaged
past its limit, becomes `Dead` and leaves debris. Sprint 2's existing
"infected slot" concept (which is currently just "a slot with an
intracellular pathogen in it") should become a real `Infected` host state
that the pathogen occupies rather than replaces.

Keep the existing cytokine behaviour working: infected cells secrete, the
heatmap reads off that, and `Chemotaxis` biases toward it. That mechanism
predates this sprint and must survive it.

**3. Debris behaves as terrain, per §1c's locked rules.**
- **Blocks host-cell regeneration.** A position holding debris cannot
  regrow a host cell until the debris is gone.
- **Macrophages clear it** — efferocytosis, the macrophage's real second
  job. This deliberately puts the same units doing the killing in
  competition with themselves; that tension is the point, not a problem to
  design around.
- **It also dissipates on its own, slowly** — slow enough that macrophage
  clearance is clearly the better answer, but present so a player who never
  invests in clearance is not permanently locked out of their own tissue.
- Make debris **visually distinct** from healthy tissue, infected tissue,
  and bare ground. Four host states now share one cell; if the Director
  cannot tell them apart at a glance the model is invisible.

**4. Host-cell regeneration.** Bare `Empty` ground regrows a `Healthy`
cell over time. Rate is a tuning value nobody has chosen — pick one, state
why, make it a field. Debris blocks it (item 3).

**5. Class-specific advance, now that it is possible.** §1b step 4 was
deferred out of Sprint 4 precisely because it needs host states:
- **Viruses spread cell-to-cell in all directions with no base bias**, and
  **die if they do not find a host quickly.** A virus can only spread into
  a `Healthy` neighbour, so a viral front advances through intact tissue
  and **cannot cross ground it has already killed** — dead tissue is a
  firebreak. That behaviour is emergent from the two rules; do not script
  it.
- **Intracellular bacteria** use the base-biased walk while *outside* a
  cell, and are hidden (occupying an `Infected` host) while inside.
- **Large bacteria** keep the Sprint 4 base-biased walk, visible
  throughout.

**6. Fix the base-band layout.** The 25×10 resize left the bone marrow
strip, its tower boxes, and the lymph node backdrop sized for the old
proportions — they overlap each other and spill across the board. This is
small, visible, and in the Director's way every time he plays.

**7. Explicitly not in scope.** ATP, economy, prices, the buying tab, the
round loop (all Sprint 6). Dendritic cells, lymphatic migration, T/B cells,
knowledge accrual (all Sprint 7). "Don't eat me" signals. Fibrosis as a
distinct system beyond debris. Parasites. Balance tuning — **the
Director's standing instruction is still mechanics first.**

Everything Sprints 1–4 built must keep working: the three bands, lumen flow
and excretion, proximity adhesion, the breach burst, base-directed advance,
per-tower population caps, kill-count depletion and degranulation, kill
attribution, proximity contact, cytokine sensing + heatmap, pooling.

## Stopping point (definition of done)

- [ ] A coarse position can hold a host cell **and** an extracellular
      pathogen at the same time, and the code says so in two slots rather
      than one enum.
- [ ] Host cells are visible, and `Healthy` / `Infected` / `Dead (debris)`
      / `Empty` are distinguishable at a glance.
- [ ] Killing an infected cell leaves debris behind.
- [ ] Debris blocks regeneration; clearing it lets a host cell regrow.
- [ ] A macrophage clears debris, and it is visible that it did.
- [ ] Debris left alone eventually disappears on its own, noticeably slower
      than a macrophage would have done it.
- [ ] A viral infection spreads through healthy tissue and **visibly fails
      to cross a patch of dead ground** — the firebreak.
- [ ] An intracellular bacterium is hidden inside a host cell and visible
      when out of one.
- [ ] The base band's compartments no longer overlap each other or the
      board.
- [ ] Everything from Sprints 1–4 still works.
- [ ] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect reality, and
      `docs/TEAM_RETRO.md` has a new note.

The question this sprint answers for the Director: **does tissue feel like
terrain now?** Does losing ground read as losing something — and does the
firebreak (a viral front stalling against tissue it already killed) show up
on its own, without being staged?

## A process note for whoever is dispatched

**Commit after each scope item, even if it is incomplete or ugly.** Sprint
3's agent hit its usage limit having written no docs; Sprint 4's agent hit
its limit having committed **nothing at all** — ~1,600 lines of
uncompilable working tree, no harness, no docs, all of which the head
session had to repair and reconstruct. Sprint 4's brief already said "write
docs as you go" and that was not enough, because the commit got batched to
the end too.

An incomplete committed tree is recoverable. An uncommitted one is not.
Same for `docs/INTERFACE.md` and `docs/TEAM_RETRO.md`: update them as each
signature changes and as each judgment call is made, not in a final sweep.
