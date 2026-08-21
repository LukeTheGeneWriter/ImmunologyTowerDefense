# Immunology Tower Defense — Game Design

Status: core mechanics locked as of 2026-08-19 (Director decisions from a
chat-surface conversation, merged in from the now-deleted
`DESIGN_DECISIONS_2026-08-19.md`). Kept current as further Director
decisions land; all implementation work builds against this file. Sections
marked **LOCKED** are settled; sections marked **TBD** or **open** need a
Director decision before they can be built — see `docs/BACKLOG.md` for the
tracked list.

## Locked decisions — platform & genre

- **Engine:** Unity (decided 2026-08-18 — see rationale in `ENGINE_STATUS.md`).
- **Platforms:** Steam (primary release) + a web-based build (public-facing
  funnel/demo, in the spirit of Bloons TD's web presence). Mobile not ruled
  out later, not in scope for early sprints.
- **Genre:** Tower defense, immunology-themed — towers as immune cells,
  enemies as pathogens. Round-based economy (currency: **ATP**) and
  constrained upgrade paths (one path reaches top tier, a second is capped
  low) are kept from the Bloons TD model — the upgrade-path constraint maps
  onto real lineage commitment in T cells, a naive cell that commits to one
  fate forecloses the others, and the UI should say so when that work
  happens.
- **Two conventions deliberately broken from Bloons TD:** leaks are not a
  fail state (see Transit vs. breach, below), and units are not static
  turrets — immune cells move, badly at first, on purpose (see The search
  problem, below).

## 1. Compartment model — LOCKED

The board has four compartments. Every immune cell is in exactly one of
them at any time.

| Compartment | Role | Contains |
|---|---|---|
| **Bone marrow** | Source of cells; storage for promoted memory adaptive cells | Progenitor towers, memory reservoir |
| **Lymph node** | Coordination between professional APCs and adaptive cells | Dendritic cells arriving from tissue, naive/activated T and B cells |
| **Blood** | Entry point into the tissue; the player's base; the lose condition | Cells in transit inward |
| **Tissue** | Combat | Host cells, immune cells, pathogens |

Two consequences that shape everything downstream:

**The base and the lose condition are the same compartment.** Blood is
where the player's production reaches the tissue, and a pathogen arriving
there ends the map. Deliberate — this is what sepsis actually is.

**Two fronts converge.** Pathogens enter at the lumen (depth 0) and
descend. Immune cells enter from blood (depth 5) and ascend. The tissue is
the contested middle — a departure from the pure Bloons model (static
towers, fixed enemy path), closer to Plants vs. Zombies in geometry.

## 1a. Map 01 layout — three lateral bands, LOCKED (Director, 2026-08-21)

The abstract compartment model above becomes concrete geometry here. Map 01
is a **Plants vs. Zombies–style lateral map**: threat comes from the right,
the player's base is on the left, and the contested ground is the middle.

**This supersedes `docs/handoff-map01-intestine.md` §2**, which put the
lumen along the top flowing left→right and made depth a *vertical* axis of
five layers. That model is retired. The threat axis is now **horizontal**,
and the old doc should be read as historical for everything spatial (its
round script, art direction, and search-progression sections are still
current).

### The bands

Working figure: a **100 × 40 grid of coarse cells**, split laterally into
three bands, each the full 40 cells tall.

| Columns | Band | Role |
|---|---|---|
| 0–24 (leftmost quarter) | **Base** | Bone marrow + lymph node. Immune cells enter tissue from here. **Pathogens reaching it cost the player health** — this is the endzone. |
| 25–74 (middle half) | **Tissue** | The host-cell lattice already built. All combat happens here. |
| 75–99 (rightmost quarter) | **Lumen** | Pathogens flow **top to bottom** freely. Reaching the bottom = excreted, no penalty. |

### How pathogens behave

- **In the lumen they are safe and free.** They flow down the channel with
  no consequence and no way to be attacked. A pathogen that simply rides
  the flow out of the bottom is excreted — **not** a fail state, which is
  the deliberate break from Bloons TD 6 that `handoff-map01-intestine.md`
  §1 already describes and this layout preserves.
- **From the lumen they choose to invade.** Two ways off the flow:
  colonising the **gut interface** (the epithelial boundary at the
  lumen/tissue seam — this is where `GAME_DESIGN.md` §6b's barrier
  colonisation lives), or **jumping leftward into the tissue** proper.
- **In tissue they push a front leftward toward the base.** This is the
  battlefront the Director asked for (2026-08-21): pathogens should
  *advance and be held*, not teleport to arbitrary positions. Slipping past
  one or two cells is allowed and occasional — the fiction is cracks and
  vessels — but the default is a contested line that advances when the
  pathogens win ground and recedes when immune cells clear it.
- **Reaching the base costs a life**, per §6c's 100-life pool. Base is
  simultaneously the player's production and the lose condition, exactly as
  §1 describes — that is what sepsis is.

### Why this geometry

It puts both sides on **one axis**, which the previous arrangement did not:
pathogens transited laterally (along columns) while immune cells entered
from the bottom edge and wandered upward, so the two never opposed each
other along a shared line and no front could form. That mismatch is the
root of the "pathogens fly across the board" problem the Director
identified, and it is what `docs/INTERFACE.md` open question 1 has been
flagging since Sprint 1. This layout resolves it: **right-to-left is the
threat axis, and the 40 rows are lanes.**

### Scale note

At the existing 7×7 fine sub-lattice and 0.12s tick, a 50-cell-wide tissue
band works out to roughly **14 seconds for a neutrophil to cross laterally
and ~42 for a macrophage** — a good spread, and slow enough that holding a
line matters. The whole 100×40 field fits a 16:9 screen at about 28px per
coarse cell. The numbers are workable as stated; they are recorded here so
a later change to fine subdivision or unit speed is understood to move
them.

## 1b. Invasion, breach, and pathogen advance — LOCKED (Director, 2026-08-21)

How a pathogen gets from the lumen into the tissue, and how it moves once
there. Replaces Sprints 1–3's behavior entirely: those spawned a pathogen
at the board edge, marched it across, and adhered it at a **uniformly
random column**, which is what produced the "flying across the board"
problem this section exists to fix.

### Step 1 — Adhesion is proximity-gated

A pathogen riding the lumen flow has a **chance to adhere that depends on
its distance from the gut interface**. Close to the boundary, it likely
adheres; far out in the channel, it likely does not and simply flows past.

This makes lateral position in the lumen meaningful rather than decorative,
and it means the flow itself does real work: a pathogen's lane through the
lumen determines its odds of ever becoming the player's problem. A pathogen
that never adheres rides the flow out the bottom and is excreted, with no
penalty — the deliberate break from Bloons TD 6 (`handoff-map01-intestine.md`
§1) that this layout preserves.

**On adhering, the pathogen moves to the boundary** and sits there. It is
now colonising the gut interface (`GAME_DESIGN.md` §6b) — not yet in the
tissue, and not yet the player's problem, but accumulating.

### Step 2 — Breach is per-position and releases everything at once

**Each position along the boundary carries its own breach chance**, rolled
per tick (or every X ticks — a tuning decision, and rolling less often with
a correspondingly larger chance is the cheaper implementation on a
40-row boundary).

When a position's breach trips, **every pathogen adhered at that position
is released at once** into the first layer of healthy tissue.

This is the shape that makes the mechanic good, and it should be preserved
even if the numbers move: pressure at a boundary position **builds
visibly** as pathogens pile up, and then **bursts**. The player can see a
dangerous position forming before it breaks, which is what makes defending
it a decision rather than a reaction. A trickle of independent single
invasions would lose that entirely.

### Step 3 — Advance is toward the base, not "leftward"

Once in tissue, a motile pathogen performs a **strongly biased random walk
in the direction of the base**. The fiction: they are chasing the resources
in the blood.

**"Direction of the base" is the specification, not "left"** — the
Director's explicit framing, and an architectural requirement rather than a
wording preference. The base direction is a **property of the map**, so a
later map can put the base anywhere (right side, a corner, the centre)
without touching pathogen movement code. Nothing in the movement
implementation may hardcode a leftward or negative-X assumption.

### Step 4 — Advance differs by pathogen class

The three classes reach the base by genuinely different means, which is
what should make a front look different depending on what is attacking it.

**Viruses — diffusive, self-limiting.** A virus spreads **cell to cell in
all directions**, with no base bias at all. What limits it is that a virus
which does not find a host quickly **dies**. So its only real progress
comes from randomly choosing a direction that happens to hold a healthy
cell. A viral front therefore advances through *intact* tissue and stalls
against ground it has already killed — it is fastest where the tissue is
healthiest, and it cannot cross a gap of dead cells. Note this makes viral
advance genuinely emergent rather than scripted, and it is the mechanic
that most rewards clearing infections early.

**Intracellular bacteria — biased when out, hidden when in.** While
**outside** a host cell they chase the blood on the same base-biased random
walk as anything else motile. Once inside a host cell they are
intracellular: hidden, not visible as themselves (per §4a), and no longer
walking.

**Large bacteria — straightforwardly motile.** Base-biased random walk,
visible as themselves the whole time.

### Step 5 — Reaching the base costs a life

Per §6c's 100-life pool. The base is simultaneously the player's production
and the lose condition, exactly as §1 describes.

## 1c. Host cell states and lattice occupancy — Director-set states, structure proposed (2026-08-21)

The Director's requirement: **a host cell has three states — healthy,
infected, dead — and that state influences the state of the lattice
position** (occupied by a cell, debris, free, occupied by bacteria, …).

This section separates those two ideas explicitly, because conflating them
is what makes occupancy models rot. **The three host-cell states are the
Director's; the two-layer structure below is the head session's proposal
and is the part to push back on.**

### Two layers per lattice position

A coarse position holds **two independent slots**:

**1. The host layer — what the tissue itself is doing here.**

| Host state | Meaning |
|---|---|
| `Healthy` | An intact host cell. What a virus needs to spread into. |
| `Infected` | A host cell harbouring an intracellular pathogen (virus or intracellular bacterium). Renders as the host cell, not as the pathogen (§4a), and secretes cytokines. |
| `Dead` | The cell is gone. The position holds **debris**. |
| `Empty` | No cell and no debris — bare ground, available for regrowth. |

**2. The occupant layer — what is standing here that is not the tissue.**

Extracellular things: a large bacterium, an intracellular bacterium that is
currently *outside* a cell, a free virus particle between hosts. Immune
cells are tracked on the fine lattice and are not part of this layer.

### Why two layers rather than one enum

Because the states genuinely co-occur, and a single enum forces
false choices. A motile bacterium crawling toward the base passes **over
ground that still has living host cells in it** — tissue is packed with
cells and bacteria squeeze between them; "occupied by bacteria" and
"occupied by a healthy cell" are simultaneously true. Sprint 1–3's
`TissueGrid` has exactly one occupant per coarse slot, which cannot express
that, and it is also what makes the parasite class (multi-slot footprint,
`BACKLOG.md`) hard to add.

### What this buys mechanically

- **A virus can only spread into a `Healthy` neighbour.** Combined with
  §1b's "a virus that doesn't find a host dies," a viral front literally
  cannot cross ground it has already killed. Dead tissue is a firebreak —
  emergent, not scripted.
- **An intracellular bacterium entering a cell** flips the host layer
  `Healthy → Infected` and clears its occupant-layer entry. Killing it
  flips the host back toward `Dead` or `Healthy` depending on how much
  damage the cell took.
- **Debris is a real board state**, which gives §6's tissue recovery and
  fibrosis somewhere concrete to live, and gives the macrophage its
  real second job (clearing debris — efferocytosis) beyond killing things.

### Debris — LOCKED (Director, 2026-08-21)

A dead host cell leaves **debris**, and debris is real terrain, not a
decal:

- **Debris blocks healthy-cell regeneration.** A position holding debris
  cannot regrow a host cell until it is cleared. This is what makes
  unattended damage compound: ground you never clean up stays dead, and
  since a virus can only spread into `Healthy` cells (§1b step 4), dead
  ground is simultaneously a firebreak against viral spread and a
  permanent hole in your own tissue. Both consequences are intended.
- **A macrophage clears it.** This is efferocytosis, and it is the
  macrophage's real second job — which means the same units doing the
  killing are also the only fast way to recover ground. That competing
  demand is the point: it is biologically correct and it gives the
  macrophage a role the neutrophil cannot fill.
- **It also dissipates on its own, slowly.** So a player who never
  invests in clearance is not permanently locked out of their own tissue
  — just very slow to recover it. Rate is a tuning value; "slowly" means
  slow enough that macrophage clearance is clearly the better answer.
- **A dendritic cell can carry debris to the lymph node to learn from
  it.** See below — this is the mechanism the knowledge system needed.

### Debris is the input to adaptive immunity (Director, 2026-08-21)

The Director's third debris rule connects two systems that were previously
specified independently, and it is worth stating loudly because it changes
what debris *is*:

§5 (pathogen knowledge) describes a threshold ladder where the adaptive
system accumulates knowledge of a pathogen species, and
`BACKLOG.md` has carried an open question about the **lymph node arrival
delay** — a dendritic cell must travel to the lymph node before anything
adaptive comes online. Neither specified **what the dendritic cell actually
picks up, or where.**

Debris is the answer. Dead host cells are the antigen source. That makes
the loop: pathogens kill host cells → debris accumulates → dendritic cells
sample it and carry it to the lymph node → knowledge accrues → adaptive
immunity unlocks. **Tissue damage is therefore not purely a cost** — it is
also the raw material for the entire adaptive half of the game, which is a
genuinely elegant tension: a perfectly clean defence learns nothing.

It also creates a real competition for the same resource, since a
macrophage clearing debris is removing what a dendritic cell would have
sampled. Whether that tension is left sharp or softened (e.g. sampling
being instant while clearance takes time) is a **balance question for
whenever this is built** — not resolved here, and not in Sprint 4 or 5.


### Antigen presentation is a spectrum, not a DC monopoly (Director, 2026-08-21)

Refinement of the above. Dendritic cells are not the only cell that can
sample and present debris — they are the *efficient* one.

- **Macrophages can also sample and present debris, but very
  inefficiently.** So the player is not choosing between "clearing" and
  "learning" as separate unit types; they are choosing a **ratio**. A
  macrophage-heavy defence still learns, just slowly. A DC-heavy one learns
  fast but does less clearing. Neither extreme is a dead end, which is a
  better shape than a hard either/or.
- **Debris also drains passively into the lymph node** — sweeping there on
  its own rather than being deliberately shuttled by a professional antigen
  presenting cell. **Macrophages in the lymph node silently delete that
  debris**: it is disposed of without anything being learned from it.

That second rule is the important one, because it makes passive drainage a
**knowledge sink rather than a free trickle of learning**. Debris that
nobody deliberately carries is not merely slower to teach you something —
it teaches you nothing at all, and then it is gone. Learning requires
*active* shuttling. Without this, adaptive immunity would eventually come
online on its own no matter how the player played, and the whole antigen
economy would be decorative.

Net effect: three ways debris can end, only one of which produces
knowledge.

| Fate of debris | Tissue recovered | Knowledge gained |
|---|---|---|
| Cleared by a macrophage in tissue | yes | a little (inefficient presentation) |
| Shuttled to the lymph node by a DC | yes | yes, efficiently |
| Drains passively, deleted by lymph node macrophages | yes | **none** |
| Left alone (slow self-dissipation) | eventually | none |

### "Don't eat me" signals — flagged for later, not scoped

Apoptotic cells display **"don't eat me" signals** that suppress
phagocytic clearance. Mechanically this is a lever on the
macrophage-clearance vs. DC-sampling ratio above: debris that resists
being eaten persists longer, which biases its eventual fate toward
sampling rather than silent disposal.

Recorded now because it is the natural tuning knob for that ratio — and a
plausible upgrade or pathogen trait later (a pathogen that induces strong
"don't eat me" signalling would clog tissue with unclearable debris).
**Not scoped, not costed, not in Sprint 4 or 5.** See `BACKLOG.md`.

## 2. Tower / unit lifespan model — LOCKED

Adopts the Bloons division between a persistent tower and a transient thing
it emits, with a real biological referent here.

- **Progenitor towers** are purchased in the bone marrow and persist round
  to round.
- **The cells they emit** enter the tissue and die at the end of the round.
- At full pressure during a round, emission and consumption roughly
  balance, so the player experiences a standing population, not an
  accumulating one.

## 2a. Placement and entry — LOCKED

**The player places progenitor towers in the bone marrow.** Bone marrow
real estate is the constraint on how many towers can exist. There is no
placement action in the tissue; cells emitted by progenitors enter from
blood and find their own way.

### Extravasation is part of the search ladder

Where a cell crosses from blood into tissue is not a player choice, and
isn't arbitrary either — it's governed by the same gradient that governs
movement, mirroring real leukocyte extravasation (selectin-mediated
rolling, firm adhesion, diapedesis at sites of inflamed endothelium):

| Search rung | Entry behaviour |
|---|---|
| 1. Random walk | Cells extravasate at random points along the vessel |
| 2. Cytokine sensing | Cells extravasate preferentially near inflamed tissue |
| 3+. Directed chemotaxis | Entry is tightly localised to the site |

This gives the cytokine sensing upgrade a **double payoff** — it improves
both where cells arrive and how they move once there.

### Known risk

With placement removed, round 1 becomes buy-then-observe with no in-round
player action. Acceptable for a tutorial round — barrier colonisation
(§6b) gives the screen something to show — but the first genuine
interactivity is the round 2 buy panel. If playtesting finds round 1
inert, the likely fix is an in-round player-triggered action (a manually
placed cytokine signal, for instance), not restoring tissue placement. Not
scoped; flagged so it isn't a surprise.

### Open: bone marrow capacity

Starting slot count and the ATP cost curve for expanding it — see
`BACKLOG.md`.

## 3. Trafficking simplification — LOCKED (Director's ruling)

In real immunology, cells follow competing signals toward different
tissues. This board has one tissue. With no competing destination, the
rate at which hematopoiesis generates a cell and the rate at which it
becomes tissue-resident are treated as equal: cells produced are cells
that arrive.

## 4. Unit roster and role design — LOCKED in principle

**Round 1: Macrophage + Neutrophil.** The neutrophil replaces the
cytotoxic T cell as the second round-1 unit — cheapest and most abundant
product of real hematopoiesis, requires no priming.

**Neutrophil role:** cheap, strong DPS, high collateral tissue damage,
weak against pathogens with evasion mechanisms. Good at most jobs,
blanket-effective at none. Tissue damage is disincentivised by fibrosis
(§6), which is what makes innate-only strategies hit a ceiling rather than
being merely suboptimal.

**T and B cells: expensive early-to-mid-game towers.** Priced deliberately
high so the step from innate to adaptive is felt as an investment
decision, not a natural progression. Primary vehicle for teaching the
innate/adaptive distinction.

**Intraepithelial γδ T cell:** an expensive tissue-resident option,
available later. Genuinely resident in small-intestinal epithelium and
responds without classical priming — the one T cell that can exist on the
board before the lymph node is online. Not a round-1 purchase.

Costs are unresolved — see `BACKLOG.md`.

## 4a. Pathogen classes — LOCKED in principle (Director, 2026-08-19)

Four classes, distinguished by where they sit relative to a host cell and
how that determines what can clear them. This split is what gives combat
real texture instead of one undifferentiated "pathogen" type, and it
plugs directly into mechanics already in this document rather than adding
new ones.

| Class | Example | Occupies | Visible as | Cleared by |
|---|---|---|---|---|
| **Intracellular — virus** | generic virus | Hides inside a host cell; the cell is not replaced | The host cell, not itself (until sensed — see below) | Collateral damage to the whole infected cell (innate) or precise MHC-I killing (adaptive, ~10% knowledge, §5) |
| **Intracellular — bacterium** | *Salmonella* (or *Listeria*/*Shigella* — genuinely gut-invasive, unlike *C. diff*, which is extracellular and toxin-mediated, not intracellular) | Same as virus | Same as virus | Same as virus |
| **Large bacterium (extracellular)** | generic gut bacterium | Kills and directly occupies one coarse slot | Itself — no disguise | Direct combat damage to the pathogen |
| **Parasite** | generic multicellular/large parasite | Multiple coarse slots at once | Itself, spanning its footprint | Direct combat damage, more of it (bigger target) |

**Intracellular pathogens are why innate-only clearing is destructive, not
just suboptimal.** Neither macrophages nor neutrophils have MHC-I-restricted
recognition — that's a §5 adaptive capability, unlocked at ~10% knowledge.
Until then, the only way an innate cell can clear an infected cell is to
damage the cell itself into destruction, pathogen included. This is
literally what "high collateral tissue damage" (§4's neutrophil role
already states this) *is* — it isn't flavor text, it's this mechanic. Tissue
damage from clearing intracellular infections is what feeds fibrosis (§6)
once that's built.

**Large bacteria and parasites don't hide.** They've already killed and
taken the slot (or slots) outright, so there's no host cell to collaterally
damage — combat targets the pathogen directly. Mechanically simpler than
the intracellular case, and closest to what Sprint 1's original adhesion
model already does (a pathogen claiming a coarse slot).

**Viral spread.** An intracellular virus left uncleared through an
**incubation period** spreads to an adjacent uninfected coarse slot,
infecting a second cell — genuine cell-to-cell viral spread, not just the
original infection sitting still. This is deliberately the mechanic that
makes search speed matter in a way the player can watch happen: a slow
random-walk search (rung 1) means visible, spreading infection; faster
search (cytokine sensing and beyond, §7/§9) catches infections before they
spread. It's a direct, legible payoff for exactly the search-ladder
progression the game is built around — not a separate difficulty knob
bolted on. (Intracellular bacteria are not stated to spread this way — this
is a virus-specific mechanic unless/until said otherwise.)

**Parasites' multi-slot footprint is a real structural change**, not just a
bigger number — `TissueGrid`'s occupancy model (§7, and
`docs/INTERFACE.md`) currently assumes exactly one pathogen per coarse
slot. Building this properly (footprint claiming multiple slots, partial
clearing, etc.) is more engineering than the other three classes combined.
See `docs/SPRINT_PLAN.md` for which of these four classes land in which
sprint — not all four need to ship at once.

## 5. Pathogen knowledge — LOCKED

A percentage representing how well the adaptive system has characterised a
pathogen, accumulated by investing in APC, T cell, and B cell towers.
Thresholds unlock capabilities rather than granting linear stat increases.

**Tracked per pathogen species, not globally.** A new species arriving in
a late round meets a naive adaptive system.

**Pathogen mutation applies a discrete step-change discount** to that
species' knowledge percentage. An event, not continuous erosion. This is
the abstraction's main payoff: antigenic change becomes a single legible
number dropping, and the game never has to represent phylogenetics, drift
versus shift, or epitope-level specificity.

### Threshold ladder

Director-specified rungs are marked **(Director)**. The remainder are
proposals filling the range; each has a real mechanism behind it and is
mechanically distinct from its neighbours rather than a scaled version of
the same effect.

| Knowledge | Capability | Mechanism |
|---|---|---|
| ~10% **(Director)** | Cytotoxic T cells kill only infected cells — no collateral loss of healthy neighbours, and/or accelerated regrowth into the vacated coordinate | MHC-I restricted recognition |
| ~20% | **Neutralization.** Reduced probability that this pathogen successfully adheres to a villus | Early low-affinity antibody blocking adhesins |
| ~30% | **Recall speed.** Shortened lymph node activation delay (see `BACKLOG.md`) on re-encounter | Memory responses are faster than primary ones |
| ~45% | **Weak opsonization.** Innate cells with the Fc receptor upgrade gain a phagocytosis rate bonus, but no specific sensing | Opsonization is real at moderate titres |
| ~60% | **Complement fixation.** Antibody-coated pathogens take passive damage over time with no immune cell present | Classical complement pathway |
| ~70% | **Secretory IgA.** Antibody exported across the epithelium into the lumen, acting on pathogens at depth 0 before adhesion is possible | Transcytosis of dimeric IgA — the signature adaptive mechanism of small-intestinal mucosa |
| ~90% **(Director)** | **Specific sensing.** Antibody-coated pathogens become chemotactic beacons for suitably upgraded innate cells | Fc receptor engagement |

### Why the 90% rung is the game's capstone

At full knowledge, adaptive immunity *solves the search problem* that
round 1 taught the player to resent. Antibody-coated pathogens stop having
to be found; chemotaxis ceases to be a purchased upgrade and becomes
something the immune system learned on its own. This makes the whole
campaign a single arc rather than two systems bolted together, and should
be protected as such in balancing — nothing else should be allowed to
trivialise search before this point.

The ~70% secretory IgA rung is the other structurally important one: the
only capability that acts at depth 0, and therefore the only mechanistic
answer to "how do you defend the lumen." Also gives the bone marrow's
memory-storage role a visible payoff.

Open question on mutation interacting with banked memory — see `BACKLOG.md`.

## 5a. How knowledge is actually earned — the DC shuttle — LOCKED (Director, 2026-08-21)

§5 says knowledge accrues toward thresholds. This section says **by what
mechanism**, and the answer is deliberately not a timer or a passive drip.

**The loop:**

1. A **dendritic cell in tissue picks up cargo from debris** (§1c — debris
   is the antigen source; dead host cells are what there is to learn from).
2. It **migrates to the lymph node**.
3. Inside the node it **bumps into the T and B cells hanging out there**.
   Every encounter carries a **stochastic chance to increment knowledge**
   of that pathogen species.
4. **The DC eventually loses its cargo** and must return to tissue to get
   more.

**Why encounters rather than a timer.** This mirrors the real DC:T cell
network — antigen shuttled to the lymph node is *presented* to T and B
cells, and antigen-specific learning comes out of those interactions. The
Director's framing: it should be the meeting that teaches, not the clock.

Two consequences worth stating, because they are what make this a mechanic
rather than flavour:

- **The lymph node becomes a second arena with its own search problem.** A
  DC finding lymphocytes is the same random-walk-and-collide dynamic the
  tissue already runs, which means the engine's existing search and contact
  code applies — and it means **how crowded the node is matters**. A node
  with few lymphocytes teaches slowly no matter how much antigen arrives.
- **Cargo depletion forces a round trip**, so a DC is not a
  fire-and-forget upgrade. It is a unit with a job that keeps taking it
  back and forth across the map, and its travel time is a real cost. This
  is also the concrete answer to `BACKLOG.md`'s long-open "lymph node
  arrival delay" question: the delay is not a configured number, it is
  however long the walk takes.

**Numbers not yet chosen:** cargo capacity (how many presentations before
it is spent), per-encounter knowledge probability, how much a successful
encounter increments, how many T/B cells populate the node and where they
come from, and whether a spent DC dies or returns empty. All tuning, none
blocking the sprints before this one.

## 5b. ATP income — LOCKED for now (Director, 2026-08-21)

Two sources:

- **Starting a round** pays a lump sum.
- **Killing a pathogen** pays per kill.

Explicitly an interim answer — the Director's words were "for now" — chosen
so the progenitor buying tab has something real to spend before the full
economy exists.

**This creates a dependency the project has been deferring: there is no
round loop.** The game currently runs as one continuous, unbounded session
— no waves, no round boundaries, no between-round interval. "Starting a
round" has nothing to attach to yet. The Director has flagged a round loop
as needed soon; until it exists, the round-start payment can only be a
one-off grant at session start.

Note the two sources pull in different directions, which is probably
healthy: the round-start lump rewards surviving to the next round, while
per-kill income rewards active clearing over letting pathogens transit.
Whether per-kill income makes a passive "let them through" strategy
non-viable — or whether it should — is a balance question for when the
economy is real.

## 6. Tissue recovery and fibrosis — LOCKED in principle

Host cells refill damaged coordinates between rounds. Damage is inevitable
and cumulative; without recovery the board degrades monotonically and the
run becomes unwinnable by attrition rather than by decision.

**Fibrosis** is the counterweight. Repeatedly damaged regions scar.
Fibrosis wears off very slowly — not strictly permanent, but persists
across many rounds and acts as a structural barrier to innate-only
strategies that lean on neutrophil DPS. The real outcome of repeated
intestinal inflammation; gives the game a persistent-consequence axis
without a lives counter, consistent with the principle that leaks are not
a fail state. Decay rate needs a number — see `BACKLOG.md`.

**Recovery mechanism (proposal, still open):** regeneration originates
from the crypts rather than applying uniformly. Intestinal epithelium
genuinely renews from stem cells at the crypt base, and small-intestinal
turnover is among the fastest in the body. Spatial recovery would mean
damage near a crypt heals quickly while distant damage lingers — giving
the player a reason to care *where* combat happens. If crypts are
load-bearing for recovery they're also a target worth defending.

### Fibrosis as terrain — proposal, strongly recommended

If a fibrotic coarse slot raises the movement cost of its fine tiles or
blocks them outright, fibrosis stops being an abstract penalty and becomes
a *search* penalty. Scarred tissue genuinely impedes leukocyte migration.
This ties the innate-overuse disincentive directly to the game's central
mechanic: neutrophil spam doesn't merely cost health, it degrades the
board the player has to search.

## 6a. Terminology: transit vs. breach — LOCKED

Two different events, previously conflated under "leak."

- **Transit** — a pathogen travels the lumen and exits. Excretion. No life
  cost. But see §6b: transiting pathogens may colonise the barrier on the
  way past.
- **Breach** — a pathogen reaches depth 5, the blood. The costly event.

## 6b. Barrier colonisation — LOCKED

Pathogens persist in the barrier space (depth 1, mucus / glycocalyx)
**round to round, on a space-available basis**. The barrier has finite
occupancy; a standing population accumulates there if the player doesn't
manage it.

Consequences, all intended:

- The top of the board has ongoing state between waves — the barrier is
  where something is always happening, which is what keeps the screen
  worth watching while the player waits.
- The player is incentivised toward barrier strategy, not only toward
  killing things in tissue.
- Unmanaged buildup creates future pressure: when barrier occupancy is at
  capacity, additional adherent pathogens have nowhere to sit and burrow
  deeper instead.

### Mucus upgrade — turnover, not thickness

The barrier upgrade should raise the **shed rate**, not the occupancy
capacity. Adding capacity would just give pathogens more room to sit. Real
mucus is continuously secreted and sloughed, physically expelling adherent
bacteria, so a turnover upgrade returns some fraction of barrier residents
to the lumen each round to be excreted.

Categorically different from killing: the player is flushing rather than
fighting. Sequences correctly against the adaptive answer to the same
problem — mucus turnover is the innate tool available early on, secretory
IgA (§5, ~70%) is the adaptive tool that later prevents adhesion outright.

## 6c. Breach cost — LOCKED in principle

**The player has 100 lives.** A breach costs one. Lives regenerate slowly
over subsequent rounds (convalescence). Losing a life must have an
**acute** consequence beyond the counter, or a 100-life pool reads as a
difficulty cushion rather than a threat.

### Acute consequence: emergency granulopoiesis

A systemic breach forces the bone marrow into emergency output, dumping
immature neutrophils into circulation — what a left shift on a
differential is showing. A punishment disguised as assistance:

- The player receives a surge of neutrophils they didn't buy and didn't
  choose.
- Those neutrophils are immature — lower effectiveness, higher collateral
  tissue damage, therefore more fibrosis (§6).
- The surge consumes bone marrow capacity for several rounds, stalling
  whatever the player was building toward.
- Paired with an **ATP income penalty** for the same duration: systemic
  inflammation is genuinely catabolic, and the metabolic cost is real.

Net effect: a breach costs a life, scars the board, and stalls the
economy — every consequence has a mechanism behind it. Needs numbers — see
`BACKLOG.md`.

## 6d. Unit lifecycle and population homeostasis — LOCKED in principle (Director, 2026-08-19)

Sprint 2 gave each bone marrow progenitor an unbounded emission timer —
placed once, it emits forever, so active cell count only ever grows. This
section is the fix: a progenitor's output is capped two ways at once,
deliberately, and each cap has a different job.

**The cap is per-progenitor, not systemic — an explicit break from
biology.** Real hematopoiesis is regulated by a systemic signal (G-CSF
sensing a body-wide deficiency), not tower-by-tower. This game
deliberately does it per-tower anyway: it's simpler to build, and it's
what makes upgrading a specific tower legible and worth doing — a systemic
cap would make individual towers interchangeable. Noted here so a future
session doesn't "fix" this back toward biological accuracy without
realizing it was a deliberate simplification, not an oversight.

**Two caps, doing different jobs:**
- **Emission rate** (already exists — `BoneMarrowManager.EmissionIntervalSeconds`,
  Sprint 2). A tower can only produce one new cell every *X* ticks,
  regardless of how many of its previous children are still alive or how
  recently one died. This is what actually gives a tower a **DPS cap**:
  even a tower whose entire population just depleted in one moment can
  only refill at this fixed rate, not burst back to full strength
  instantly.
- **Max active children** (new). A tower also has a hard ceiling on how
  many of its own children can be alive at once (e.g. "10"). Once at that
  ceiling, it stops emitting even if the timer has elapsed, until one of
  its children dies and frees a slot. This is the piece that stops
  unbounded growth outright — without it, a tower left alone long enough
  still accumulates population no matter how slow the emission rate is.
- Together: a tower's sustained output is bounded by the emission rate,
  and its standing population is bounded by the max-children cap. Neither
  alone is sufficient — rate-only still grows without bound given enough
  time; cap-only still allows an instant refill burst the moment several
  children die at once.

**Neutrophils deplete via kill count, then degranulate.** A neutrophil
that reaches its kill-count limit doesn't just vanish — it **degranulates**:
self-destructs and deals a burst of collateral damage at its own location
(to whatever host cell or infected cell is there). This isn't a new
mechanic bolted on; it's the same "high collateral tissue damage" trait
§4 already gives neutrophils, now with a concrete trigger and consequence
instead of being pure flavor text. A degranulated neutrophil frees its
slot in its tower's max-active-children count, same as one that never
depleted at all.

**Macrophages retire quietly instead.** Consistent with real macrophages
being longer-lived and less prone to this kind of terminal burst,
macrophages get a higher kill-count threshold before retiring, and
retirement is a clean removal (no collateral damage) rather than a
degranulation event. **Confirmed by the Director 2026-08-21** — the
behavior as written, at a threshold of **20 kills** (four times the
neutrophil's; an earlier drafted 15 read as too low).

**Upgrades are the future payoff, not built yet — but the numbers are
parameterized for them now (Director, 2026-08-21).** The Director's
explicit intent: neutrophil upgrades should eventually let a player
**reduce degranulation's collateral damage**, or **trigger self-destruction
deliberately at a chosen moment** (e.g. before an infection spreads,
rather than waiting for the kill counter) instead of only ever being
forced into it passively. Added to that list: **upgrading a progenitor to
raise its cells' kill counts**, i.e. buying longer-lived output from one
specific tower.
No upgrade system exists yet, so none of these are purchasable. What the
Director *did* rule is that the implementation must not foreclose them:
every lifecycle number (max active children, both kill limits,
degranulation burst) is **per-tower mutable state seeded from a per-unit-kind
default**, never a hardcoded constant. An upgrade, when it lands, should
be a write to one tower's field and nothing more. This is the concrete
reason the cap is per-progenitor rather than systemic (see the top of this
section) — a global cap would have nothing for such an upgrade to attach
to.

**Upgrades apply instantly to living cells — LOCKED (Director, 2026-08-21).**
When a progenitor is upgraded, the change takes effect immediately on
**every one of that progenitor's currently-fielded children as well as all
future ones.** The Director's reasoning is a game-feel one and overrides
the biological reading: spending ATP should make an instant, visible
difference, not a difference that only arrives as old cells cycle out.

Sprint 3 originally shipped the opposite (each unit held a value snapshot
of its tower's numbers taken at emission time, so an upgrade improved only
future children). That was a head-session judgment call — simpler, and
arguably more realistic, since a mature neutrophil doesn't retroactively
gain granules — and the Director overruled it. Implementation: a unit
holds a **live reference** to its tower's `UnitLifecycleTuning`, so writing
to the tower's instance is immediately visible to all of its live units.
Do not "fix" this back into a copy; it is deliberate. Note the mechanism
does **not** leak across towers — each progenitor still owns its own
instance, and the shared per-kind `UnitProfile` defaults are never mutated.
to.

Numbers (max active children, neutrophil kill-count limit, macrophage
kill-count limit, degranulation collateral damage, contact radius) are
tuning values, not derived — see `docs/SPRINT_PLAN.md` for the Sprint 3
starting figures and `BACKLOG.md` for what's still open.

## 7. Spatial representation — LOCKED

**Discrete lattice, two resolutions.** Authoritative state lives on a
grid; sprites tween between coordinates so grid logic doesn't produce
steppy visuals.

### Two-resolution lattice

- **Coarse grid — occupancy.** One slot holds exactly one host cell or one
  pathogen. This is the layer the player reads: tissue integrity, pathogen
  depth, fibrosis.
- **Fine sub-lattice — movement.** Each coarse slot subdivides into
  **7×7** fine tiles. Immune cells, antibodies, and other mobile objects
  walk this layer.

**Subdivision must be odd.** An odd count gives every host cell a true
centre tile, which matters for gradient source placement, burrow anchors,
and rendering. 7×7 additionally gives a clean size ladder in which every
unit is odd and centred: macrophage 5×5, neutrophil 3×3, antibody 1×1. At
5×5 the neutrophil would be a centreless 2×2.

### Search mathematics — how the knobs actually behave

**Subdivision is a time multiplier, not a difficulty knob.** Targets are
coarse-slot sized, so a searcher explores the same number of coarse slots
regardless of subdivision; crossing one slot costs ~s² diffusion steps.
5×5 → 7×7 multiplies search time by 49/25 ≈ 1.96 and changes nothing
structural.

**Board width is the difficulty knob, and it's stronger than it looks.** A
board 30 slots long and 5 deep is not meaningfully two-dimensional: beyond
a few slots of travel the walk is effectively one-dimensional along the
length. Hitting time on a line scales as L², not as *N* log *N*. Search
cost therefore rises **quadratically with board width** — 30 to 40 columns
is roughly a 75% increase, not 33%. Cheap difficulty now; a trap when
scaling later maps.

Order-of-magnitude figures for 30 coarse columns × 5 rows, two units
hunting four adhered pathogens, one fine tile per tick at 60 Hz:

| Subdivision | Fine tiles | ≈ ticks to first contact | ≈ wall clock |
|---|---|---|---|
| 5×5 | 3,750 | ~2,800 | ~45 s |
| 7×7 | 7,350 | ~5,500 | ~90 s |

### Per-cell step length — required by the 7×7 choice

Because 7×7 doubles traversal time, pacing must be decoupled from
subdivision: each cell type gets a **speed in fine tiles per tick** rather
than a fixed one tile per tick. Not a workaround — migration speed
genuinely differs by cell type (neutrophils are among the fastest
migrating leukocytes, macrophages markedly slower), so the parameter is
one the game wants regardless. With it, 7×7 costs nothing in pacing.

### What this buys

1. **Search time decouples from board complexity.** A 30×5 host-cell board
   is 3,750 walkable fine tiles. Cover time scales roughly as *n* log²*n*,
   so round 1 can be genuinely punishing while the player still only reads
   150 host cells.
2. **Cell size becomes a real property.** Macrophages occupy 3×3 fine
   tiles, neutrophils 2×2, antibodies 1×1 (approximate — see the LOCKED
   figures above for the final sizing). Macrophages genuinely are much
   larger, and size trades against maneuverability through crowded tissue.
3. **Multiple immune cells can crowd one host-cell slot**, which is what
   actually happens around an infected cell.

### Scale honesty

The fine lattice fixes *relative* size ordering, not absolute scale. At 5×
subdivision of a ~20 µm enterocyte, a fine tile is roughly 4 µm; an
antibody is about 10 nm, so it remains ~400× oversized. A large
improvement on one-antibody-per-cell-slot and worth having, but the
hardest difficulty tier shouldn't claim the board is to scale.

### Chemotaxis on the lattice

The textbook model of chemotaxis *is* a biased random walk —
run-and-tumble in bacteria, gradient-following in leukocytes, the
Keller-Segel formalism. The lattice implementation is the honest model
rather than a simplification of one:

| Rung | Implementation |
|---|---|
| 1. Random walk | Uniform probability across neighbours |
| 2. Cytokine sensing | Neighbour probability weighted by local gradient |
| 3. Directed chemotaxis | Greedy gradient ascent / pathfinding |
| 4. Tissue residency | No movement; pre-positioned |

**Legibility.** On a lattice the player can see a unit revisit a tile it
just left. Cytokine sensing preserves the wandering but adds visible
drift. In continuous space both modes read as "cell moving smoothly" and
the distinction is lost at high speed multipliers. This difference must be
visible from across the room — the single most important thing to solve
for the search problem (see §10 below and Round 1 script in
`docs/handoff-map01-intestine.md`).

**Movement is four-neighbour (von Neumann), not eight.** Depth is the
threat axis. Legal diagonals make vertical progress cost the same as
horizontal and the two axes stop being distinct. Descending a layer should
remain a discrete, visible, costly event.

**Immune cells co-occupy with host cells; pathogens replace them.**
Without this, immune cells can't move through healthy tissue at all.
Leukocytes genuinely migrate between cells through the interstitium;
pathogens kill and occupy.

### Implementation note

Run cytokine diffusion on the **coarse** grid and interpolate down to the
fine lattice for the biased walk. Diffusing across 3,750 fine tiles per
tick is the operation most likely to become the frame budget problem, and
coarse diffusion is visually indistinguishable once interpolated. The
object pooling utility from Sprint 0 (`PrefabPool.cs`) covers agent churn
but not this.

## 8. Performance requirement (non-negotiable, not just a nice-to-have)

Research into comparable games (Bloons TD 6 specifically) shows late-game
slowdown is driven by raw entity count — enemies, projectiles, tower
effects, and ability animations all stacking up at once — not by engine
choice. This game is expected to reach similarly dense late rounds, so:

- Enemies, projectiles, and effects must be object-pooled from the first
  implementation, not retrofitted later.
- There should be an explicit, tunable cap on simultaneous cosmetic effects
  (particles, hit-flashes, etc.) that degrades gracefully under load rather
  than accumulating unbounded.

This requirement belongs in every relevant sprint brief until the core
combat loop is fully pooled — see `ENGINE_STATUS.md`.

## 9. The search problem (why Sprint 1 exists)

The random walk in round 1 is the central teaching device and the anchor
of the entire upgrade economy. The progression is: (1) random walk — no
directional information, round 1 default; (2) cytokine sensing — pathogen
sites emit a gradient, units bias toward it, first purchasable upgrade,
should feel transformative; (3) directed chemotaxis — units path
efficiently to the nearest signal; (4) tissue residency — units
pre-position at high-risk sites and stop searching entirely.

Every step must be **visible in the movement of units on screen**, not
only in a stat readout. See `docs/handoff-map01-intestine.md` for the full
round 1 script this anchors.

## TBD — still needs a Director decision before it can be built

See `docs/BACKLOG.md` for the tracked list of open numeric/design
questions (round 1 economy, grid dimensions, fibrosis decay rate, lymph
node arrival delay, transit cost stacking, bone marrow capacity, emergency
granulopoiesis numbers). Also still open, lower priority:

- Enemy roster beyond the pathogens named above (boss design, scaling
  curve).
- Art direction / tone (clinical and precise vs. stylized and playful) —
  `docs/handoff-map01-intestine.md` §8 has a working answer (histology
  palette, clinical register) for the first playable slice specifically.
- Meta-progression, if any (persistent unlocks across runs).
