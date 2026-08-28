# Changelog

One entry per sprint, written by the Producer at handoff. Appended to,
never rewritten.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Project pipeline stood up: Unity project builds to desktop and WebGL,
Steam app-ID stubbed, object pooling utility in place. Nothing playable
yet — next sprint starts real gameplay.

-->

## Sprint 7 -- 2026-08-28
There is a game loop now. The framework, not the balance -- **every number
is a placeholder**.

The game opens in a **buy phase**: you have 100 ATP, the bone-marrow
picker shows prices (Macrophage 40, Neutrophil 15) and greys out what you
can't afford, and nothing spawns until you press **Space** (or the Start
button). A round then spawns a **finite batch** of pathogens -- round 1 is
8, each round is 3 bigger -- and ends when that batch is resolved
(everything cleared, excreted, or reached the base). Pathogens still
sitting on the gut wall are allowed to carry over into the next round, so
a smouldering wall doesn't hold the round open.

**ATP comes in two ways** (GAME_DESIGN 5b): +3 per pathogen a unit kills,
during a round; and a +80 lump sum when a round clears, shown between
rounds as your budget for the next one. When a round clears it also
**despawns every unit your towers put in the field** -- the towers stay,
and re-emit from scratch next round.

**You have 100 lives.** A breach (pathogen reaches the base) costs one;
one regenerates every two cleared rounds; at 0 it's **GAME OVER**. The
acute "emergency granulopoiesis" punishment from the design is still
deferred, so for now a breach is just the counter.

Verified: a new `EconomyVerification` harness, **47 assertions** across the
wallet, the round state machine, batch completion, the lump sum, per-kill
income, life loss -> defeat, life regen, placement cost, and the
round-boundary unit clearing -- plus Map 71 / Combat 36 / Lifecycle 79 /
Tissue 73 all still green (**306 total, 0 failed**). Clean Windows build.
**Not** verified: nobody has played the loop -- buy, start, survive, get
paid, buy more -- which is the whole question, and the Director's
playtest.

## Sprint 6 -- 2026-08-28
An established intracellular infection is now the thing innate immunity is
bad at, on purpose (`GAME_DESIGN.md` §4b -- the Director's rework after the
Sprint 5 playtest).

**You can't just shoot an infected cell any more.** A macrophage or
neutrophil grinding on a cell with a virus or a bacterium inside does
nothing directly. What it can do, each tick it's in contact, is **roll to
recognise the infection** -- a low chance for innate cells -- and if it
succeeds it kills the cell **loudly** (a big magenta burst), taking
everything inside with it and releasing nothing. That low roll is the
whole point: it's why you'll eventually want the stress-sensing cells (γδ
T and friends) that get a *high* roll. Those aren't built yet -- this
sprint is you feeling the problem.

**Intracellular bacteria** now behave the way you asked: out of a cell
they roam freely with no death timer and take normal damage -- that's your
window. Inside a cell they're untouchable, and they **replicate, draining
the host's health**, until it dies and a **brood of bacteria bursts out**.
Catch the cell with a stress-sense kill before then and the brood never
happens. A bacterium-infected cell now renders a sickly yellow-green, so
you can tell it from a violet virus infection and watch one duck in and
burst back out.

**Viruses** split into two species. Contact-chain ones still snake --
infect one neighbour, done. **Budding** ones emit free virions on a timer
that float around (momentum-biased walk, per-tick chance to enter a
healthy cell), so a budding infection grows as a **disk**. Free virions
can only ever step onto healthy cells, so a budding front still can't
cross dead ground -- the firebreak survives. And a chunk of viral
infections now just **burn out on their own** -- the cell exhausts, dies
loud, spills its virus, no immune action needed.

Verified: `TissueVerification` grew from 53 to 73 assertions (stress-sense
loud-kill / no-burst, exposed-vs-hidden bacterium, drain + brood + caught-
early-no-brood, budding disk, burn-out), plus Combat 36 / Lifecycle 79 /
Map 71 all still green -- **259 total, 0 failed**. **Not** verified: nobody
has watched a macrophage catch an infection, a brood burst, a budding disk
grow, or an infection burn out -- all the Director's playtest. Every
number is an unvalidated mechanics-first default.

## Sprint 5 -- 2026-08-28
Tissue is terrain now. Every cell in the tissue band is a host cell with
its own state -- **Healthy**, **Infected**, **Dead**, or bare **Empty**
ground -- drawn in four distinct colours, and each coarse position now has
**two independent layers**: the host cell, and whatever extracellular
pathogen is squeezing past it. A bacterium can stand on a slot that still
holds a living cell; one enum could never say that.

**Death leaves a corpse.** Killing an infected cell -- by innate immunity,
by a bacterium lysing out, by a large bacterium grazing it down, by
neutrophil collateral -- turns it to **Dead** and drops **debris**, and
debris is real terrain: nothing regrows on a slot until the debris is
gone. A **macrophage clears it** (efferocytosis, its real second job), in
about 2.5 seconds of standing on it, with a calm blue-green pulse when a
pile is finished. Left alone, debris also fades on its own -- but about
20x slower, so clearing it is clearly the better answer. Cleared ground
regrows a healthy cell after ~20s.

**The three pathogen classes finally move differently.** A **virus** only
ever steps onto a Healthy cell and dies within 6s if it can't find one --
so a viral infection advances through intact tissue and **physically
cannot cross ground it has already killed**. Nothing in the code checks
for a firebreak; it falls out of those two rules, and a headless test
drives 60 incubation cycles against a band of dead ground and confirms
nothing ever gets across. An **intracellular bacterium** walks the
base-biased path while exposed, ducks inside a healthy cell it passes,
hides there ~12s, then lyses out -- killing that cell -- and walks on. A
**large bacterium** grazes the host cell under it as it goes, killing it
over a few steps.

**Also:** the bone-marrow strip and lymph-node backdrop were still sized
for the 100x40 map and spilled across the board on the 25x10 one -- they
now fit inside the base band. Two bugs found and fixed while resuming the
interrupted work: an intracellular bacterium was killing itself when it
lysed out, and viral spread could waste its one shot landing a doomed
particle on non-healthy ground instead of stalling against it.

Verified: a new **TissueVerification** harness, 53 assertions across
two-layer occupancy, debris/regrowth/efferocytosis, the firebreak, and
class-specific advance -- plus Sprint 4's 71, Sprint 3's 79, and Sprint
2's 36 all still green. **Not** verified: nobody has watched the firebreak
happen on screen, or a macrophage clear a pile -- both are the Director's
playtest, and the firebreak (a viral front stalling against tissue it
already killed, with no one staging it) is the question this sprint
exists to answer.

Items 1, 2 and the antigen-barcode design doc were committed by a
dispatched Code agent before it lost its network connection mid-item-5;
the head session finished items 3, 5 and 6, found the two bugs, wrote the
harness, and wrote the docs.

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
