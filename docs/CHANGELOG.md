# Changelog

One entry per sprint, written by the Producer at handoff. Appended to,
never rewritten.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Project pipeline stood up: Unity project builds to desktop and WebGL,
Steam app-ID stubbed, object pooling utility in place. Nothing playable
yet — next sprint starts real gameplay.

-->

## Sprint 4 -- 2026-08-21
The map is real. Instead of one undifferentiated board where pathogens
appeared at random spots, Map 01 is now 100x40 host cells in three lateral
bands: your **base** on the left (bone marrow, lymph node, and the place
pathogens must not reach), 50 cells of **tissue** in the middle, and the
**lumen** on the right. Threat comes from the right and pushes left, Plants
vs. Zombies style.

Pathogens ride the lumen flow downward for free -- reach the bottom and
they are excreted with no penalty, which is deliberate. But the closer one
drifts to the gut wall, the likelier it is to stick to it, and stuck
pathogens **pile up at that spot on the wall**. Each spot's odds of
rupturing rise with the size of its pile, and when it goes, **every
pathogen there floods into the tissue at once.** That build-then-burst is
the sprint's centrepiece: you should be able to watch a dangerous spot
forming before it breaks. Once inside, pathogens make a strongly biased
random walk toward your base.

Advance is specified as "toward the base," never as "leftward" -- the base
is a map property, so a future map can put it anywhere and pathogen
movement follows without a code change. There is a test that runs the same
movement code on a mirrored board and confirms the pathogens walk the other
way.

The HUD now shows where every pathogen is (lumen / wall / tissue), running
counts of adhesions, breaches, excretions and anything that REACHED BASE,
and a live frame-cost readout.

**Two things worth knowing.** First, **cytokine sensing got much weaker on
the bigger map** -- on the old 30x5 board it pulled units onto infections
within a minute; on 100x40 it only trends toward them. Nothing broke; the
gradient is simply flat at 47 cells where it was steep at 3. It is measured
and recorded, not tuned, per the standing "mechanics first" instruction.
Second, the map spent one build genuinely broken in a way nothing caught:
the scene file still carried the old 30-column width, and because the outer
bands clamp to fit, **the tissue band silently became zero cells wide**. It
ran, drew a board, and reported no errors. Fixed, and the game now shouts
if the playfield ever collapses again.

Verified: 71 new map/invasion assertions, plus Sprint 3's 79 and Sprint 2's
36 all still passing; 4,000 cells at 8.35 ms/frame (vsync-capped, so that
is an upper bound); clean build, zero exceptions; the invasion loop visibly
running unattended. **Not** verified: nobody has watched a breach burst
happen -- the counters prove it does, but the sight of it is the Director's
to judge, and it is the question this sprint exists to answer.

Implemented by a dispatched Code agent that hit its usage limit having
committed nothing at all; the head session repaired the tree, wrote the
verification harness, found the zero-tissue bug, and wrote the docs.

## Sprint 3 -- 2026-08-21
Population is bounded. Sprint 2's progenitor towers emitted forever and no
unit ever despawned, so active cell count only ever grew -- the problem
this sprint exists to fix. A tower now stops emitting once 10 of its own
cells are alive, and resumes when one dies. Cells die by doing their job:
a neutrophil that lands 5 kills **degranulates** -- self-destructs with a
visible burst that damages whatever occupies its cell -- while a macrophage
quietly retires after 20 (the Director raised this from a drafted 15). The
two deaths are meant to read as deliberately different, not as units
randomly vanishing; the HUD now shows a live active-unit count and each
marrow slot shows "N/cap alive," so boundedness is something you can watch
rather than take on trust.

Two Sprint 2 gaps were folded in rather than deferred. Kills are now
attributed to exactly one unit -- whoever's hit lands the killing blow --
which is what makes kill-count depletion possible at all. And contact
damage now requires actually being near a pathogen (within 2 fine tiles)
instead of merely sharing its 7x7 cell, which removes an accidental
stacking bonus where every unit in a cell hit it every tick.

**The thing to watch in playtest:** that second change cut contact
frequency to about half of Sprint 2's, measured -- so clearing is roughly
half as fast per unit, arriving at the same moment as a population cap. If
the board starts losing ground, that interaction is the cause and the
contact radius is the knob, not a bug. Every number this sprint
(cap 10, kill limits 5/20, burst 3x, radius 2) is a per-tower tunable
field rather than a constant, on the Director's instruction, so a future
progenitor upgrade can sell "bump this tower's kill count" as a one-line
change.

Verified: 76/76 new lifecycle assertions, Sprint 2's 35/35 combat
assertions still pass, Sprint 1's cytokine numbers unchanged (OFF
2.99/3.14/2.84, ON 0.20/0.00/0.00), Windows build clean at 93.3 MB, launches
with zero exceptions. **Not** verified: placing a tower through the running
build's UI -- scripted clicks couldn't take window focus this session, so
that first click is the Director's to make. Implemented by a dispatched
Code agent that hit its usage limit after committing working code but
before writing any documentation; the head session re-ran all verification
and wrote the docs.

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
