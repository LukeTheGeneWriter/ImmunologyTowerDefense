# Immunology Tower Defense — Game Design

Status: core mechanics locked as of 2026-08-19 (Director decisions from a
chat-surface conversation, merged in from the now-deleted
`DESIGN_DECISIONS_2026-08-19.md`). Kept current as further Director
decisions land; all implementation work builds against this file. Sections
marked **LOCKED** are settled; sections marked **TBD** or **open** need a
Director decision before they can be built — see `docs/BACKLOG.md` for the
tracked list.

Later Director decisions with their own dated markers: §1a–§1c, §5a–§5c,
§6b–§6d (2026-08-21); **§4b — intracellular infection / the innate↔adaptive
bridge (2026-08-28), which supersedes §1b step 4 and the intracellular rows
of §4a in detail.**

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

**Rendered distinctly — Sprint 15 (Director, 2026-08-30).** The three
bands are no longer the same host-cell grid in three tints. The **lumen**
is drawn as an open fluid channel (chyme field, a mucus layer on the gut
wall, drifting particulate, a slow peristaltic squeeze); the **base** as
the bloodstream it is per §1 (oxblood plasma, an endothelial vessel wall
at the tissue seam, erythrocytes streaming in, the marrow and lymph node
seated in it as organs) — and a pathogen reaching it now triggers an
acute breach flash instead of a silent despawn. Only the tissue band
keeps a per-cell renderer. Spec: `docs/COMPARTMENT_DESIGN.md`.

### Scale note

At the existing 7×7 fine sub-lattice and 0.12s tick, a 50-cell-wide tissue
band works out to roughly **14 seconds for a neutrophil to cross laterally
and ~42 for a macrophage** — a good spread, and slow enough that holding a
line matters. The whole 100×40 field fits a 16:9 screen at about 28px per
coarse cell. The numbers are workable as stated; they are recorded here so
a later change to fine subdivision or unit speed is understood to move
them.

### Peristalsis — the bolus moves in surges (Director, 2026-10-13, Sprint 19)

The contaminated food bolus no longer crawls down the lumen at a constant
rate. A peristaltic wave pushes it forward over ~35% of each 7.5 s cycle
and it barely drifts between waves. The profile's mean speed is exactly
the old constant speed, and the period divides the transit evenly, so the
total transit (`FoodItemTransitSeconds`, 30 s) and round pacing are
unchanged. The lumen's drawn squeeze runs on the same clock, so the wall
visibly contracts behind the bolus as it surges. Burst timing still keys
off the bolus's lane position, so pathogens now drop in clumps that
follow the surges.

## 1b. Invasion, breach, and pathogen advance — LOCKED (Director, 2026-08-21)

How a pathogen gets from the lumen into the tissue, and how it moves once
there. Replaces Sprints 1–3's behavior entirely: those spawned a pathogen
at the board edge, marched it across, and adhered it at a **uniformly
random column**, which is what produced the "flying across the board"
problem this section exists to fix.

> **Revised and re-locked (Director, 2026-10-18/19, Sprint 22).** Steps 1
> and 2 below are the REWRITTEN versions. The orderly per-lane
> colonisation stack is gone; so is the hidden per-position breach roll.
> The design is in `docs/INVASION_REWORK_SPEC.md` (its §12 carries the
> Director's decisions and supersedes its own fork list), and the
> immunology it rests on is checked against the literature in
> `docs/IGA_REFERENCE.md`. Steps 3–5 are unchanged.

### Step 1 — Adhesion is proximity-gated, and adherents MILL

A pathogen riding the lumen flow has a **chance to adhere that depends on
its distance from the gut interface**. Close to the boundary, it likely
adheres; far out in the channel, it likely does not and simply flows past.

This makes lateral position in the lumen meaningful rather than decorative,
and it means the flow itself does real work: a pathogen's lane through the
lumen determines its odds of ever becoming the player's problem. A pathogen
that never adheres rides the flow out the bottom and is excreted, with no
penalty — the deliberate break from Bloons TD 6 that this layout preserves.

**On adhering, the pathogen joins the mill.** It takes a continuous
position in the mucus band — distance *along* the wall, and depth *out*
from it — arrives in the outer mucus and **settles in over about two
seconds**. It does not snap to the seam and it does not queue: it drifts,
loops and crowds. The adhered population is carried slowly **downstream**
by the mucus, in time with the peristaltic contraction already on screen,
and an adherent that reaches the far end rejoins the flow and is excreted.
So the barrier population accumulates across rounds (§6b) without being
immortal.

Adherents **drift toward each other**. Colonisation is a social process,
and mechanically this is what turns several scattered clumps into one
dangerous spot over the course of a round.

**Bacteria replicate while on the wall**, damped by local density. The
damper is **nutrient competition between metabolically overlapping
neighbours** — your own kind compete with you hardest, because they eat
exactly what you eat. It is *not* quorum sensing, which governs virulence
and biofilm rather than growth rate. Viruses do not replicate out here:
no host, no replication.

### Step 2 — Ingress is proximity-based, and pressure is a visible charge

**Breach is no longer per lane.** Crowding is measured as *concentration*
over a stretch of wall about a cell and a half wide — a tight knot reads as
dangerous, the same organisms spread thin do not. Where that crowding
peaks, the wall forms a **plaque**: a patch that owns the organisms inside
it and carries a **charge**.

The charge **fills while the plaque holds more than a minimum number of
organisms**, and when it fills, the plaque **ruptures**: every member
enters the tissue at once, across whatever lanes the crowd had spread over,
and the epithelium it tore through **dies**.

Three properties are load-bearing and must survive any future change:

1. **Pressure builds visibly at a spot, then bursts.** This is the original
   requirement and the reason the section exists. What is *drawn* is now
   the actual state: the blister's height and colour are the charge. The
   old model drew a proxy for a memoryless roll — a "hot" lane might go in
   one second or in thirty, nothing accumulated, and a player could never
   see that an intervention had worked.
2. **Ingress is something a CROWD does, never something a pathogen does.**
   The charge rate subtracts a floor, so one or two adherents accrue
   nothing, *forever*. A trickle is arithmetically impossible, not merely
   unlikely. This floor is also the **minimum infectious dose**, which is
   real and famously species-specific, so it is the knob that will later
   distinguish one species from another.
3. **A rupture is one event releasing everything at once**, not a stream of
   independent invasions.

**A dead stretch of wall is a weak spot, not a hole** (Director,
2026-10-18). It charges markedly faster — the board remembers where it was
breached — but it still takes a crowd, so there is **one** ingress
mechanic in the game and not two. Dead wall regrows slowly on its own, and
faster near a crypt.

**The asymmetry: bacteria and viruses want opposite walls.** A virus needs
a *living* cell, so over a dead stretch there is nothing left to infect and
viruses gain nothing there. (Phrase this as "nothing there to infect",
never "cannot cross" — a virion can physically drift through a gap, it
simply finds no host.) A battered wall is therefore a bacterial sieve and a
viral dead end, and a pristine wall is the reverse. The player cannot
optimise for both, and a mixed round forces the choice.

**Two defences act out here, and both flush rather than kill.**

- **Sentinel sloughing (innate, free, from round 1).** A sentinel goblet
  cell at a crypt entrance detects bacteria crowding too close, dumps its
  whole mucus load, and signals its neighbours to do the same, expelling
  what is caught back into the flow. Triggered, not timed; local, not
  global; non-specific, so it needs no knowledge. §6b's mucus-turnover
  upgrade buys **amplification** of this — a wider patch and a faster
  refill — not the mechanic itself.
- **Secretory IgA (adaptive, at §5's ~70% rung).** See §5. Its effect on
  this step is that **crowding flips sign**: free crowding charges a
  plaque, coated crowding cross-links into rafts that cannot reach the
  epithelium and are swept away. Above the rung, a dense patch of a
  characterised species is not a threat forming — it is a clearance event
  forming.

**The player can answer the wall.** Towers can suppress a charging plaque,
and a bought repair restores dead epithelium. A visible blister has to be
something the player can act on, not only watch.

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

> **Refined by the Director, 2026-08-28** — after the Sprint 5 playtest.
> The virus / intracellular-bacterium behaviour above is superseded in
> detail by **§4b (Intracellular infection — the innate↔adaptive
> bridge)**. Headlines: intracellular bacteria roam *freely* when
> extracellular (no death clock — they have the chassis to survive, unlike
> a virus) and **replicate while inside**, draining the host cell, then
> **burst out as a brood** when it dies; some virus species **bud** free
> virions rather than only chain cell-to-cell; a fraction of viral
> infections **burn out on their own**; and both classes, while
> intracellular, are only cleared by a **contact stress-sense roll** (low
> for innate, high for the not-yet-built stress sensors) or by running
> their course. Sprint 5 shipped a placeholder (enter → 12s timer → lyse);
> Sprint 6 replaces it with §4b.

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

## 1d. Host-cell upgrades — design (Director, 2026-08-29), placeholder in Sprint 11

Bought in the buy-phase shop. **Sprint 11 shipped the shop framework
only** — purchasing spends ATP and raises a level, nothing about the
simulation changes. The design each will implement:

### Pattern-recognition receptor → immunogenic apoptosis

Upgrade host cells to sense a pathogen molecular signature. The Director's
worked example: **a dsRNA sensor**. A cell so upgraded, once infected by
the matching class (an **RNA virus** for dsRNA), has a **~20% chance to
self-destruct** — a clean apoptotic death, cell + resident, releasing
**a strong "eat this debris" cytokine that recruits *dendritic cells
specifically***. This is:

- **intrinsic antiviral defence** — the cell kills itself and the virus
  with it, denying the infection before it buds or bursts;
- **immunogenic cell death** — the debris it leaves is a loud call to the
  adaptive-sampling loop, not a quiet corpse. It actively *feeds*
  knowledge accrual (§5a).

It implies a **third cytokine signal**, distinct from the §7/§9
recruitment cytokine (chemotaxis for innate cells toward infection) and
the §5c co-localisation signal (DC↔helper-T in the node): a
**DC-attractant** in the tissue. Other sensor variants (dsDNA, LPS, flagellin)
are the obvious follow-ons, each keyed to a class.

### Reduced viral entry

Lower the per-tick chance (`InvasionTuning.VirusEntryChancePerTick`) that
a free virion sitting on an upgraded `Healthy` cell gets inside it — the
cell is harder to infect, so a viral front advances more slowly through
upgraded tissue.

### Bacterial-damage resistance

A large bacterium grazing an upgraded cell does less host-cell damage per
step (`InvasionTuning.LargeBacteriumHostDamagePerStep`) — upgraded tissue
survives a bacterial crawl-through longer.

### Crypts

Placeable stem-cell niches (§6's crypt-based recovery). Tissue within a
radius of a crypt regrows faster — a spatial version of Sprint 11's
neighbour-accelerated regrowth, giving the player a reason to care *where*
combat happens. Repeatable buy (more crypts = more coverage).

**Built Sprint 19 — snapped to the wall (Director, 2026-10-13).** Real
crypts are invaginations of the epithelium and never sit free in the
tissue, so each crypt sits at the gut wall and its reach (3 cells,
Chebyshev) extends into the tissue. Empty ground in reach regrows 3×
faster. Coverage is spatial, not stacking. The player doesn't choose
where a crypt goes yet: each purchase takes the next lane of a fixed,
well-spread sequence that avoids the Peyer's patch. If players want to
choose, the natural extension is a click-a-wall-lane placement mode,
still snapped to the wall.

### Peyer's patch and M-cell sampling — built Sprint 19 (Director, 2026-10-13)

A fixed stretch of the gut wall, about 4 lanes at ~60% of the way down
the flow, is a **Peyer's patch**: no villi, a low dome of epithelium,
M cells on the dome, and a lymphoid follicle in the tissue beneath. It is
**always drawn**, a landmark the Director called "important
immunologically".

Its function, **M-cell sampling, is a shop upgrade, not a default**
(Director: "M cell functionality as an upgrade not straight out of the
box"). Once bought, a pathogen drifting over the dome or sitting at the
wall there may be sampled, at a rate that scales with the level. M cells
transcytose *antigen*, so the pathogen itself is unharmed and still a
threat. The antigen waits in the dome (up to 3 parcels) until a
cargo-less DC patrolling the follicle collects it as cargo.

Why it matters: until now the **only** route to knowledge (§5a) was
debris, so the player learned about a pathogen only after it had killed
tissue. The patch is the first route that teaches from the *lumen side*,
before damage. That is what Peyer's patches are for in reality, and it
starts §5c's adaptive loop earlier. Sequenced as an upgrade, it is a
choice between spending ATP on fighting now and on learning sooner.

**Refine later (flagged, not built):**
- DCs don't seek the follicle; they collect only when their patrol
  crosses it. Making the follicle a DC attractant is the obvious next lever.
- M cells are also a real *entry* route: Salmonella and others exploit
  them. A pathogen class that breaches preferentially at the dome would
  make the patch a two-edged structure.
- A dedicated follicle lymphocyte population (local priming, IgA class
  switching) ties to the ~70% secretory-IgA rung (§5).

## 2. Tower / unit lifespan model — LOCKED

Adopts the Bloons division between a persistent tower and a transient thing
it emits, with a real biological referent here.

- **Progenitor towers** are purchased in the bone marrow and persist round
  to round.
- **The cells they emit** enter the tissue and die at the end of the round.
- At full pressure during a round, emission and consumption roughly
  balance, so the player experiences a standing population, not an
  accumulating one.

> **Superseded in part — Sprint 9 (Director, 2026-08-29), after the
> Sprint 8 playtest.** Despawning every fielded cell at the round boundary
> "looked like a bug" — the screen froze the player's army while
> straggler pathogens kept moving. **Emitted cells now PERSIST round to
> round.** The buy phase **freezes time** instead (see §5d), so the
> standing army carries seamlessly into the next round. Population is
> still bounded — the per-tower `MaxActiveChildren` cap and the
> clamp-don't-bank emission timer (§6d) do that work; the round-end
> despawn was never the backstop. Towers do not emit during the frozen
> buy phase; they resume on Start.

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

### Bone marrow capacity — LOCKED (Director, 2026-10-25)

**Answered after the Director lost a run** and said the interesting part
was having to think about where the ATP went. The design goal is therefore
to *preserve* that pressure, not relieve it.

- **Start with 4 niches, expand to 8.** One fewer than the game shipped
  with, so the opening keeps the pressure that produced the good run, and
  the first expansion is a real decision rather than a bonus.
- **Escalating price: 80 / 140 / 200 / 260 ATP** (`base * (1 + 0.75 n)`).
  Two rules the numbers have to satisfy, both pinned by
  `Sprint27Verification`:
  1. **The first niche costs more than the dearest tower.** Buying *room*
     for a tower must never be cheaper than buying the tower, or the
     optimal opening is always "expand first".
  2. **The last niche costs more than three dear towers.** Otherwise late
     ATP has no competing use and the end of a run stops being a choice.
- **You buy it by clicking the fatty pad**, not from the shop. The bone is
  laid out at full capacity from the first frame, and un-bought niches are
  drawn as **fatty (yellow) marrow** with their price on them. The thing
  you are buying is visible on the board before you can afford it, which
  is how the mechanic teaches itself.
- **The bone fills top-down.** Clicking any fatty pad converts the *next*
  one in order, so the marrow reads as a single growing column and the
  player cannot leave gaps.

**The biology is real and worth keeping straight.** This is marrow
*reconversion*: fatty marrow turning back into haematopoietic marrow under
demand. G-CSF "stimulates marrow mitotic activity and induces marrow
reconversion from fatty to cellular", appearing on imaging as "patchy
expansions of baseline hematopoietic marrow" (Gu 2018, *Skeletal
Radiology*, [DOI](https://doi.org/10.1007/s00256-018-3035-0)) — which is
also why niches appear one at a time rather than the bone growing. It ties
directly to §6c's emergency granulopoiesis: same signal, slower and more
permanent expression of it.

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

> **Built in Sprint 23 (2026-10-25) — see §4c**, which supersedes the
> paragraph above for the γδ cell specifically: it ships as a *wall patrol
> and repair* tower, not an expensive hunter, because the research says
> that is what the cell does.

## 4a. Pathogen classes — LOCKED in principle (Director, 2026-08-19)

Four classes, distinguished by where they sit relative to a host cell and
how that determines what can clear them. This split is what gives combat
real texture instead of one undifferentiated "pathogen" type, and it
plugs directly into mechanics already in this document rather than adding
new ones.

| Class | Example | Occupies | Visible as | Cleared by |
|---|---|---|---|---|
| **Intracellular — virus** | generic virus | Hides inside a host cell; the cell is not replaced | The host cell, not itself (until sensed) | **§4b:** contact stress-sense roll (innate low / stress sensors high) → loud kill of the cell; or the infection burns out on its own; precise MHC-I killing (adaptive, ~10% knowledge, §5) is the quiet version |
| **Intracellular — bacterium** | *Salmonella* (or *Listeria*/*Shigella* — genuinely gut-invasive, unlike *C. diff*, which is extracellular and toxin-mediated, not intracellular) | Roams freely when extracellular; hides inside a host cell (cell not replaced), replicating | Itself when extracellular; the host cell when inside | **§4b:** ordinary combat damage *only while extracellular*; while inside, the same contact stress-sense roll as the virus, else it runs its course (drains the cell to death, bursts out a brood) |
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

> **Viruses can reach the base as of Sprint 30 — before it, they could
> not, at all.** An intracellular virus has no movement code by design
> (its spread is §4b's job, not a walk), and nothing gave it any other
> path inward. A diagnostic found **0 of 150** viruses dropped into tissue
> ever reaching the base against 150 of 150 bacteria, with 142 still in
> their original cell after three simulated minutes. A virus could not
> cost the player a life by construction.
>
> Two mechanics fix it, and neither makes virions walk:
> - **Spread is budgeted, not one-shot.** It was a single hop *ever*, so a
>   focus reached two cells and stopped. Each infection may now seed
>   `VirusSpreadBudget` daughters, biased base-ward
>   (`VirusSpreadBaseBias`), so an unanswered focus marches inward.
> - **Dissemination.** An infection sitting in the base-most tissue row
>   crosses out of the epithelium after `VirusDisseminationSeconds`. That
>   delay is the point: it gives a late cytotoxic cell something to save.
>
> Measured on real geometry: one unanswered focus crosses the 13-cell
> tissue band and reaches the base in **~185 s**, seeding ~110 infections
> on the way. That time is almost exactly 12 hops × the 15 s incubation,
> so **the march is incubation-bound** — that is the lever if it should be
> faster, not the spread budget.

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
is a virus-specific mechanic unless/until said otherwise.) **§4b adds a
second, per-species spread mode — *budding* — and a spontaneous
burn-out fraction.**

**Parasites' multi-slot footprint is a real structural change**, not just a
bigger number — `TissueGrid`'s occupancy model (§7, and
`docs/INTERFACE.md`) currently assumes exactly one pathogen per coarse
slot. Building this properly (footprint claiming multiple slots, partial
clearing, etc.) is more engineering than the other three classes combined.
See `docs/SPRINT_PLAN.md` for which of these four classes land in which
sprint — not all four need to ship at once.

## 4b. Intracellular infection — the innate↔adaptive bridge — LOCKED (Director, 2026-08-28)

Sprint 5 shipped a placeholder for the two intracellular classes (walk in,
sit on a 12-second timer, lyse back out). The Director playtested it and
replaced the model. This section is the replacement; Sprint 6 builds it.
The point of the rework: **an established intracellular infection is the
thing innate immunity is *bad at*, on purpose** — it is what the adaptive
half of the game exists to answer, and the mechanics here are the seam
between the two.

### The stress signal, and why sensing needs contact

Every infected cell — viral or bacterial — emits a **stress signal**. It is
**not** the recruitment cytokine of §7/§9: that one says "something is
wrong over here, come look," and drives chemotaxis from a distance. The
stress signal is different — it is read **on contact**, and it answers
"*this* cell, the one I am touching, is infected and needs to die."

The Director's reasoning for making recognition contact-gated: a cell
stressed enough to be screaming chemokine has *already* tried to kill
itself (apoptosis). Whether an immune cell then finishes the job is a
close-range decision — a handshake, not a broadcast.

### The contact stress-sense roll

An immune cell **in contact with an infected cell** rolls, **each tick**,
to recognise the infection. On success it performs a **loud kill**:

- The host cell dies **necrotically** — `Dead` + debris, and a **strong
  DAMP** (a loud death per §1c) that recruits more innate cells and feeds
  fibrosis (§6) later.
- **Everything inside dies with it. No pathogen is released** — not a
  virion, not a bacterium. Catching an infection early is how you deny the
  brood (see bacterial replication below).

The roll probability is **pathogen-class-specific** and, more importantly,
**cell-type-specific**:

| Sensing cell | Per-tick recognition | Built? |
|---|---|---|
| Innate (macrophage, neutrophil) | **Low** — they lack antigen-specific recognition; this is generic stress/damage sensing | Sprint 6 |
| Dedicated stress sensors (γδ T cell, CTL, NK-like) | **High** — they know what they are looking for; this is the payoff for investing past innate | Later — see below |

This is the bridge: innate can clear an intracellular infection, but only
by luck and only loudly. The stress sensors clear it reliably. Adaptive
MHC-I killing (§5, ~10% knowledge) is the *quiet* precise version of the
same recognition — no necrotic DAMP, minimal collateral.

**Dedicated stress-sensor units are deferred** (Director, 2026-08-28 —
"we can wait to build gamma deltas… for a moment"). Sprint 6 ships only the
innate (low) roll, so the Director feels the problem — "my macrophages
can't touch this" — before the answer is purchasable. When those units
land they will also want a **patrol movement pattern** distinct from the
search-ladder chemotaxis (Director's note); that is deferred with them, in
`BACKLOG.md`.

### Virus — revised

- **A free virion dies after `VirusFreeSurvivalSeconds` outside a host.**
  Unchanged — the "needs a new host *now*" pressure, and half of the
  firebreak (§1c).
- **Spread has two per-species modes:**
  - *Contact-chain* (Sprint 5, kept): an infected cell infects **one**
    adjacent healthy cell after an incubation period. Produces a snaking
    path.
  - *Budding* (new, some species only): the infected cell **periodically
    emits a free virion** that (a) does a **momentum-biased random walk** —
    a slight bias toward its last heading, so a cloud of budded virions
    expands roughly **radially** from the source rather than diffusing
    formlessly — and (b) **rolls a per-tick "entry" chance** against
    whatever healthy cell it is currently on. Each budded virion carries
    **its own survival clock**. A budding infection reads as a growing
    *disk* of infected cells, visibly different from a chain virus's line.
- **Spontaneous burn-out:** a **fraction of viral infections deplete and
  die on their own** — the cell exhausts, dies loud, and spills its
  virions + debris with no immune action. Makes a viral infection
  self-limiting even if wholly ignored, and seeds fresh free virions into
  the tissue as it goes.
- Infected cells still spread only into **`Healthy`** neighbours, so a
  budding front still cannot cross dead ground — the firebreak survives the
  rework.

### Intracellular bacterium — revised

- **Extracellular: no death clock.** Unlike a virus, it has the chassis to
  survive in the interstitium indefinitely. It roams on the base-biased
  walk and is **fully vulnerable to ordinary innate contact damage** — this
  is the window to kill it cheaply. Give it *more* inclination to wander
  than a virus (the Director's words: "more of a chance to move around
  outside the cells").
- **Enters a healthy host cell it is standing on** — a per-tick roll.
- **Intracellular: immune to ordinary innate contact.** A macrophage or
  neutrophil touching the cell does nothing *except* roll the contact
  stress-sense (low). While inside, the bacterium **replicates on a timer,
  draining the host cell's health**. No voluntary exit.
- **When the drain kills the cell:** loud death, debris, and a **burst of N
  bacteria** released as extracellular — `N` scales with how long the
  infection incubated (replication count). Option A from the design
  conversation.
- **If a stress-sense roll kills the cell first: no burst.** The brood dies
  with the host. This is what makes the low innate roll still worth
  something — every tick a macrophage sits on an infected cell is a chance
  to abort the whole thing.

### Numbers — all TBD, mechanics-first

Innate stress-sense per-tick probability, bacterial replication interval
and host-drain rate, brood size `N` as a function of incubation, budding
interval, per-tick virion entry chance, momentum-bias weight, spontaneous
burn-out fraction and its timing. None chosen; all belong in
`InvasionTuning`/`TissueTuning` as mutable fields. The Director's standing
instruction is still mechanics first.

## 4c. The adaptive and unconventional roster — LOCKED (Director, 2026-10-25)

Sprint 23 added four towers. The evidence behind every number is in
`docs/CELL_KINETICS_REFERENCE.md`, with the unabridged passes in
`docs/research/`.

**The organising decision, and the one most worth protecting:** only one
of these four is primarily a killer. The obvious design — four cytotoxic
cells at four price points — was available and is wrong on the biology,
and it would have made the roster a single stat line with four names on
it. Each tower here answers a threat the others cannot.

| Tower | Gated? | Speed | Its actual job |
|---|---|---|---|
| **Cytotoxic T cell** | ~10% knowledge | 3 | Recognises and kills an infected cell by **apoptosis** — clean, so the tissue recovers |
| **NK cell (ILC1)** | no — innate | 3 | IFN-γ: **upgrades nearby macrophages**; weak precise kill of its own |
| **γδ T cell (IEL)** | no — tissue-resident | 1 | Patrols the wall: intercepts adherents, **repairs the barrier** |
| **B cell** → plasma cells | ~20% knowledge | 0 (sessile) | Secretes drifting **antibodies** that coat extracellular pathogens |

### The CTL kills the cell — what it buys is a clean death

**Corrected 2026-10-25 (Director).** An earlier version of this section
said a cytotoxic T cell excises the pathogen and leaves the enterocyte
alive. That is wrong. A CTL recognises peptide-MHC-I on an infected cell
and **kills the cell** — perforin and granzymes, inducing apoptosis.
Nothing sterilises a cell and walks away: the cell is the infection's
shell, and killing it is the only route to anything living inside.

So every route to an intracellular infection kills the host cell. What
differs is **how it dies**, and that is a real and consequential
difference rather than a cosmetic one:

| | Apoptotic (CTL, NK, γδ) | Necrotic (macrophage, neutrophil) |
|---|---|---|
| Trigger | Specific recognition — peptide-MHC-I, or stress ligands | Generic damage until the cell fails |
| Cell contents | Packaged into membrane-bound bodies | Spilled |
| Debris left | ~30% of a full pile | A full pile |
| Neighbours | Untouched | Injured; DAMPs released |
| Tissue outcome | Regrows quickly | Slow, and feeds fibrosis (§6) |

**So the adaptive payoff is that the tissue recovers instead of scarring**
— not that the cell survives. That is the observable a player should learn
to read: after a cytotoxic sweep the ground comes back; after an innate
one it is a field of debris.

`TissueTuning.ApoptoticDebrisFraction` is the whole reward in one number.
If it is ever raised to 1.0 the two paths become indistinguishable, every
test about infections clearing still passes, and the case for buying
cytotoxic cells silently evaporates.
`Sprint23Verification.RunNecroticKillLeavesAMess` pins the comparison.

**Apoptotic death does not cost the player antigen.** Apoptotic cells are
the classic substrate for cross-presentation, so the debris still carries
the species tag for a dendritic cell to sample. (An earlier draft claimed
precision costs you what you learn from; it does not.)

### Specific recognition vs stress sensing — two routes, same outcome

The three cytotoxic kinds reach an infected cell by different means, and
that is what keeps them from being redundant:

- **Cytotoxic T cell — specific recognition.** Reads a peptide presented
  on MHC-I, so it acts only on species the adaptive system has
  characterised. This is the route that matters against **viruses** and,
  in the wider biology, tumours.
- **NK/ILC1 and γδ IEL — stress sensing.** Read induced stress ligands
  (NKG2D ligands such as MICA/MICB and the ULBPs) and missing-self. No
  antigen, no priming, no knowledge required. Stress display is strongly
  associated with viral infection, which is why these two also skew
  antiviral despite being innate.

The gap between them is the game's hard counter: a virus that
**downregulates MHC-I** blinds the CTL completely — measured per-capita
kill rate of *exactly* zero — while stress sensing still sees it. That
pathogen is designed and not yet built.

### Recognition is a visible cumulative meter, not a hidden roll

Replaced the per-tick coin flip. An immune cell in contact with an
infected cell adds pressure to a 0..1 meter on that cell; the kill fires
when it fills; an unattended cell sheds pressure. Expected time to clear
is unchanged (a roll at *p* per tick and an accumulator at *p* per tick
both resolve in 1/*p* ticks), so this is a legibility change, not a
balance one.

Two reasons it is worth the extra state:

1. **Sprint 22 already proved the point.** The plaque blister read because
   it replaced a hidden roll with something that visibly fills. This is
   the same move for the same reason.
2. **It makes cooperation expressible**, which a single roll cannot.
   Halle 2016 measured p(death) = 0.01 / 0.15 / 0.28 for 0 / 1 / 2 CTL
   contacts, superadditive at 3+. Three cytotoxic cells converging clear a
   cell far faster than three working separately — so letting a cytokine
   gradient concentrate your cells is now a real tactic rather than a
   cosmetic one.

### The innate recognition numbers were inverted, and are now fixed

`StressSenseChancePerTick` was 0.03 (macrophage) / 0.02 (neutrophil) — a
mean 4.0 s of contact to clear an infected cell. A real CTL, which *has*
MHC-I-restricted recognition, kills with p≈0.15 per ~9-minute contact. Our
macrophage, which has none, was ~800× faster than that.

§4b has always said established intracellular infection is "the thing
innate immunity is *bad at*, on purpose." The number never implemented the
sentence. Now: innate 0.003/0.002 (~40–60 s of contact), cytotoxic 0.03
(~4 s) — about a 10× gap, which was the Director's explicit ceiling.
**Not more than ~20×**: past that the innate roll stops reading as a
mechanic at all and becomes a flat "innate cannot do this".

### The NK cell is an amplifier, not a competitor

Buyable from round 1, no knowledge required. Its main output is an
IFN-γ aura that activates nearby macrophages (×2.5 damage and recognition,
6 s, radius 3). It has a weak quiet kill — a third of the CTL's.

This is the shape the literature supports and it also solves a design
problem: an ungated cytotoxic tower would compete with the CTL and cheapen
the ~10% rung. An amplifier compounds with the macrophages the player
already owns instead.

**Named "NK cell (ILC1)"** (Director's call): the familiar name leads so
players can place it, the correct one follows because in human small
intestine the epithelial population is ILC1, which lacks perforin and
granzyme B entirely.

**Do not give it a clonal-expansion upgrade.** Human NK proliferation is
unchanged during acute infection; homeostatic doubling is ~16 days. NK
cells are early because they are already present, not because they ramp
fast. The common "NK is the fast responder" framing is contradicted by the
data (Schlub 2011) and should not be narrated anywhere in the game.

### The γδ IEL repairs — the only tower that does

Needs no priming and is tissue-resident, so it is gated by neither
knowledge nor the lymph node. Speed **1**, deliberately against its real
ratio: it is already at the wall, so search speed is beside the point and
a fast number would read as "another hunter".

Two effects, both reusing the wall API Sprint 22 built:

- **Interception** — suppresses charge on forming plaques within 2 cells.
- **Repair** — restores dead barrier segments nearby every 3.5 s.

Sourced rather than invented: γδ IELs converge on the enterocyte bacteria
are *currently* touching, migration-defective γδ cells are as useless as
having none, and more intercalation cut invasion ~63% (Edelblum 2015).

### B cells field plasma cells that emit real antibodies

The Director's ruling was that antibody must be **visible**: plasma cells
(sessile — motility is inversely related to secretion capacity) secreting
drifting 1×1 antibody objects that stick to extracellular pathogens and
coat them.

Coating calls the **same `SetCoated`** secretory IgA already uses, so a
coated organism is pacified, slowed and more fragile in tissue by exactly
one code path. Tissue antibody and luminal IgA are one molecule in two
compartments and the game must not grow two implementations of that.

An antibody **cannot reach inside a host cell** — which is why the ~10%
and ~20% rungs answer genuinely different threats instead of stacking.

Queued, not built: **B cells deploying to form a tertiary lymphoid organ**
(Director's idea, 2026-10-25). The research backs it — see
`CELL_KINETICS_REFERENCE.md`. Three things it must carry when built: the
player *matures a pre-seeded cryptopatch* rather than placing a follicle
anywhere; it saturates at ~4–6 follicles ≈ one Peyer's patch; and its
drawback is that gut TLOs **obstruct lymphatic outflow toward the node** —
the follicle built to bypass the DC shuttle physically blocks it, and
resists regression once mature.

### Licensing: buy early, switch on later

A knowledge-gated tower is **purchasable at any time** and emits nothing
until a species clears its rung. The Director chose this over hiding the
card so the spend can be planned in advance; the card stays enabled and
its blurb names the threshold.

`IsNodeResident` and `IsKnowledgeGated` are **orthogonal** and must stay
two predicates. They used to be one (`IsAdaptive`), which worked only
because no cell was both adaptive and tissue-roaming. A CTL is both. A γδ
cell is neither.

### Open — needs a Director decision

- **Per-species recognition.** A CTL currently only acts on species that
  have themselves cleared 10% (MHC-I restriction is species-specific), so
  your CTLs walk past a novel pathogen. Biologically right and makes mixed
  rounds a real decision, but harsher. `KnowledgeLicence.PerSpeciesRecognition`
  flips it in one write.
- **What one round is in biological time** — two research passes
  disagreed (1 day vs 2 days). Belongs with round design.
- **The MHC-I stealth pathogen.** Designed, not built (Director's call:
  not this sprint). Under real MHC-I downmodulation CTL killing goes to
  *exactly zero*, which makes a sourced hard counter that only the NK cell
  answers. Build it as a cliff — there must be no graded stealth stat,
  because partial MHC-I reduction does nothing in reality.

## 5. Pathogen knowledge — LOCKED

A percentage representing how well the adaptive system has characterised a
pathogen, accumulated by investing in APC, T cell, and B cell towers.
Thresholds unlock capabilities rather than granting linear stat increases.

**Tracked per pathogen species, not globally.** A new species arriving in
a late round meets a naive adaptive system.

> **True as of Sprint 24.** Until then the ledger was keyed by
> `PathogenClass` -- three of them -- so learning about "intracellular
> virus" taught you about every virus that would ever arrive, and the
> sentence above could not happen. The key is now `PathogenSpecies`
> (`docs/PATHOGEN_ROSTER.md`). `Sprint24Verification.RunKnowledgeIsPerSpecies`
> pins it: rotavirus and the generic virus share a CLASS, and teaching one
> must not teach the other.
>
> **Roster ceiling.** The 8-bit barcode (§5c) requires species tags >= 4
> bits apart, and hand-picking separable tags already failed once at
> species six -- rotavirus's first tag sat 2 bits from the generic
> intracellular bacterium and the harness rejected it. Past roughly 8-10
> species the barcode must grow to 16 bits. Note §5c's claim that the
> Hamming threshold is the dial to reach for before barcode length is
> **backwards**: length is the binding constraint.

> **The barcode is 10 bits, not 8 (Sprint 29).** For a helper-T tag within
> radius *r* of one species to be within *r* of at most one, species tags
> must be at least 2*r*+1 apart — 5 at the shipped *r*=2. An exhaustive
> search shows **8 bits admits at most four such tags**, and there are six
> species, so the property the code claimed was not merely unmet but
> unreachable: 21.9% of helper-T tags were teaching two species by
> accident. Ten bits fits all six at distance 6, so cross-teaching at the
> default radius is exactly zero, and **cross-reactivity becomes something
> the player buys** (the avidity row takes *r* to 3) rather than an
> accident of tag choice.
>
> Knowledge-per-match rose 3 → 8 to absorb the 2.64× drop in match rate.
> **§5c's claim that the Hamming threshold is the dial to reach for before
> barcode length is backwards** — length is the binding constraint, and no
> threshold could have fixed the 8-bit roster.

**Pathogen mutation applies a discrete step-change discount** to that
species' knowledge percentage. An event, not continuous erosion. This is
the abstraction's main payoff: antigenic change becomes a single legible
number dropping, and the game never has to represent phylogenetics, drift
versus shift, or epitope-level specificity.

### The knowledge HUD and "planned" rungs (Sprint 21, Director 2026-10-14)

Knowledge is now player-facing: the bottom-left strip shows each species'
% and its ladder. **No rung's capability is built yet**, but the % is
really earned. So a reached rung shows as *learned · planned*, with the
UI's "designed, not built" hatch, rather than being hidden or shown as
live. `KnowledgeLadder.Rung.Built` flips each to solid as its mechanic
lands. **Flip the flag in the same commit that builds the mechanic.**
Sprint 22 shipped secretory IgA and did not flip its flag, so for a whole
sprint — including the playtest in which the Director reported never
perceiving IgA — the HUD went on calling a live mechanic *planned*.
`Sprint21Verification.RunHonesty` now pins the exact built set in both
directions so neither kind of drift can recur. The ~90% capstone (`KnowledgeLadder.CapstonePercent`) is a star at
the end of every ladder, permanently planned for now: it names the
campaign's goal.

### Threshold ladder — **roster confirmed (Director, 2026-08-29)**

The Director confirmed the six-rung roster below (thresholds still
placeholder, mechanically distinct from each other). Sprint 11 built it
as **data + a HUD readout only** (`KnowledgeLadder` /
`KnowledgeCapability`) — crossing a threshold changes the display and
nothing else. Each capability's real mechanic is its own later sprint.

| Knowledge | Capability | Mechanic (Director's note) |
|---|---|---|
| ~10% | **Cytotoxic T cells** | MHC-I-restricted recognition, then an **apoptotic** kill of the infected cell — no spill, no necrotic DAMP, little debris, so the tissue recovers instead of scarring. The cell still dies; the *manner* is the reward. **Built in Sprint 23.** |
| ~20% | **Neutralizing antibodies** | reduced adhesion probability for a known species (low-affinity antibody blocking adhesins). **Built in Sprint 23** as plasma-cell-secreted antibody that coats pathogens in tissue, reusing the §6b coating state. |
| ~30% | **Memory T cells** | **Re-specified in Sprint 29.** A burst of CTLs is the wrong shape: three independent research passes found day-8 effector numbers are invariant to how much memory you start with, and the onset of division is indistinguishable from naive. Memory's real advantages are a far larger standing precursor pool and **zero lead time**. Built as a helper-T upgrade row: a pairing that misses on barcode alone is rescued with a probability scaled by how well that species is already known — knowledge standing in for "how many times have we seen this before" (Director, 2026-10-25). |
| ~45% | **Fc receptor** | antibodies affix to innate cells — opsonisation / antibody-guided targeting, no specific sensing yet. |
| ~70% | **Secretory IgA** | antibody exported into the lumen, acting on a species **before adhesion is possible** (depth 0). The only capability that acts in the lumen. **Built in Sprint 22.** Two stages: a *coated* organism is slower, pacified and more fragile behind the wall; two coated organisms of the same species are **cross-linked** into a raft that cannot reach the epithelium and is swept away. Crucially, a coated bacterium still divides at full rate and its **daughter is born attached to it**, so under IgA replication is what traps it. Nothing out here is killed. |

> **Complement left this ladder (Director, 2026-10-25).** It is now a
> **body upgrade in the shop**, because it is *structural rather than
> learned*: gut C3 is made locally by stromal cells and deposits through
> the **alternative** pathway, which antibody does not trigger (Wu 2024,
> *Cell* — luminal C3 deposition is unchanged in C1q-deficient mice). A
> per-species knowledge ladder said the opposite, that you learn your way
> to complement one pathogen at a time.
>
> The move also fixes a wording that was wrong for this map. The gut lacks
> membrane-attack-complex components, so "antibody destroys the membrane
> with no immune cell present" does not happen here; as a body upgrade it
> is re-specified as what the gut actually does — **opsonise, and recruit a
> phagocyte to finish it**. Wu 2024 found basal C3 correlates directly with
> protection, which is exactly the shape of a stacking body upgrade.
>
> Residual uncertainty, recorded honestly: that paper is a **colon** study,
> the small intestine is untested, and nobody has replicated its C5–C9
> survey. Full write-up in `docs/PATHOGEN_ROSTER.md`. If a direct-lysis
> beat is ever wanted, its honest home is inflamed, barrier-breached
> tissue, where MAC does appear — and attacks the host epithelium.

The **~90% "specific sensing" capstone** (antibody-coated pathogens
become chemotactic beacons) is not in the Director's six — treat it as the
end state that Fc receptor grows into, still the capstone the balance
should protect (see below).

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

## 5e. Lumen traffic — lane changing and spawn spread (Sprint 25)

**Organisms mix across the channel as they drift.** Before Sprint 25 a
pathogen's depth was fixed from spawn to exit: it entered in a lane and
rode that lane to the excretion end. Because adhesion probability falls
off exponentially with distance from the wall (§1b step 1), that meant an
organism's entire fate was decided by a single spawn roll it could never
escape — near lanes were lethal, far lanes were scenery, and the player
could affect neither. It also *looked* wrong: the channel visibly
stratified into packed rows and empty ones.

Real luminal contents are not laminar at this scale. Peristalsis actively
mixes the bolus — which the game already models as surges (§1a) — and that
mixing is what repeatedly re-presents organisms to the mucus surface
instead of letting the mid-channel ones coast past untouched.

Two differences fall out of it for free, and both are real:

- **Motile organisms mix more and bias toward the wall.** Bacteria are
  flagellated and swim toward the mucus layer; virions are passive
  particles that go where the fluid puts them. So a bacterium works its
  way to the epithelium and a virion only arrives there by luck.
- **Mixing is at most one cell per step**, so flow stays the dominant
  motion. `MapVerification` pins that bound — the old assertion that depth
  *never* changes was replaced by one that it changes by at most one.

**Spawn depth is stratified, not random.** A rotating cursor walks every
depth in turn, so a wave spreads across the channel by construction.
Uniform random is even *in expectation*, but a wave is a handful of
organisms and at that sample size random clumps badly — which is the other
half of what made the channel look stratified. There is deliberately **no
jitter** on top of the cursor: the first version added one and the harness
caught it undoing the very spread it existed to vary (a wave of ten
covered six depths). Variety between waves comes free, because the cursor
runs continuously and a wave rarely divides the channel depth evenly.

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

**Built — Sprint 8 (Director, 2026-08-29).** The shuttle is built: a
bought **dendritic-cell progenitor** emits DCs that patrol tissue, sample
antigen off a dead cell's **debris** (which now carries the class of
whatever killed it — §1c), carry it left into a now-real **lymph node**
compartment, and **pair** with the helper-T cells there. The lymph node
is a real bounded arena with its own **co-localisation cytokine signal**
(§5c step 4) driving both cell types together via the existing chemotaxis
code. Implementation choices recorded: a spent DC **returns to tissue
empty, it does not die** (the travel time is the cost); a **cargo is good
for `DcPresentationsPerCargo` (4) pairings**; the DC **eats a bite of the
debris pile when it samples**, so it competes with macrophage
efferocytosis for the same debris; **B cells are not built** (helper-T
only); the species key is `PathogenClass` until a roster exists. Every
number is a placeholder, and **a rising knowledge % unlocks nothing yet**
— §5's threshold ladder is the next sprint. See `ENGINE_STATUS.md` and
`SPRINT_PLAN.md`.

**Refined — Sprint 10 (Director, 2026-08-29).** DC patrol was a plain
random walk and the cells clumped. Rather than debris homing, patrolling
DCs now **repel each other along the lane axis only** (the base↔lumen
threat axis is untouched), so they patrol back and forth and stay evenly
spread across the lanes — the answer to "DC migration capacity."
Tunable (`DcLaneRepelStrength`); macrophage debris homing is still an
open `BACKLOG.md` item.

**Fixed — Sprint 12.** The Sprint 10 version wasn't visible in play — it
compared *coarse* cell indices, so the bias fired only ~1 step in 7 and
there was no back-and-forth at all. Now the lane-repulsion works at
fine-tile granularity every step, and a patrolling DC also **sweeps the
full depth of the tissue band, flipping at the edges**
(`DcPatrolSweepBias`), so it genuinely paces the band and covers ground
rather than loitering near where it spawned.

**Fixed again — Sprint 14.** Still not visible: the oscillation and the
lane-repulsion both lived only in the *patrol* state, and with debris
everywhere in a dense round a DC sampled cargo within ~2 ticks and then
spent the rest of its life in the travel / node / return states, which
had neither. The four-state shuttle (`PatrolTissue → TravelToNode →
InNode → ReturnToTissue`) collapsed to **two** (`PatrolTissue`,
`InNode`): a DC now paces the band its **entire tissue life** — repelling
its neighbours across the lanes the whole time — and simply biases toward
the base while carrying antigen, entering the node when it reaches the
base band and resuming the pace afterward. Sweep and step rate bumped
(`DcPatrolSweepBias` 1.0→1.8, `DcFineTilesPerTick` 2→3) so a full
base↔lumen lap is legible inside a ~30 s round.

## 5b. ATP income — LOCKED for now (Director, 2026-08-21)

Two sources:

- **Starting a round** pays a lump sum.
- **Killing a pathogen** pays per kill.

Explicitly an interim answer — the Director's words were "for now" — chosen
so the progenitor buying tab has something real to spend before the full
economy exists.

Note the two sources pull in different directions, which is probably
healthy: the round-start lump rewards surviving to the next round, while
per-kill income rewards active clearing over letting pathogens transit.
Whether per-kill income makes a passive "let them through" strategy
non-viable — or whether it should — is a balance question for when the
economy is real.

**Round loop built — Sprint 7 (Director, 2026-08-28).** The "there is no
round loop" caveat this section carried is resolved; see **§5d**. Two
implementation choices worth recording here: the round-start lump is
granted **when a round clears** (framed as the budget for the next round —
what the player sees as the between-rounds "+N ATP"), with `StartingAtp`
seeding the first buy phase instead; and per-kill income is a flat amount
routed through the single `SearchUnit.RegisterKill` chokepoint, so it
covers ordinary contact kills and §4b stress-sense kills but **not**
brood-burst / burn-out / drain-death (those are not the player's kills).

## 5d. Round loop — LOCKED (Director, 2026-08-28)

The round model, decided at the top of Sprint 7. **Wave batch + buy
phase**, close to Bloons TD's rhythm:

- **A round is a defined batch of pathogens.** Round `N` spawns
  `BatchSizeBase + (N−1)·BatchSizeGrowthPerRound` of them into the lumen on
  the normal spawn cadence, then stops.
- **The round ends when the batch is resolved** — every pathogen in it has
  cleared, been excreted, or reached the base — **except** pathogens
  colonising the gut wall, which are *allowed to persist* into the buy
  phase and the next round, exactly as §6b's barrier colonisation
  specifies. A smouldering wall pile does not hold the round open.
- **On clear:** the round-start lump sum is granted, a life regenerates
  every `LifeRegenRounds` rounds (§6c convalescence), and **every fielded
  immune cell despawns** — §2's "the cells they emit die at the end of the
  round." The progenitor **towers** stay placed; each re-emits from
  scratch next round.
- **The buy phase** pauses spawning. The player places / can eventually
  upgrade towers, then a **player action** (a keypress or a button) starts
  the next round. No timer on the buy phase.
- **`RoundPhase { Building, Active, Defeat }`.** Defeat is entered when the
  life pool hits 0 (§6c); everything freezes.

Numbers (`EconomyTuning`, all placeholder mechanics-first): `StartingAtp`
100, `RoundStartLumpSum` 80, `AtpPerKill` 3, `MacrophagePrice` 40,
`NeutrophilPrice` 15, `BatchSizeBase` 8, `BatchSizeGrowthPerRound` 3.

**Deliberately not in the framework:** round batch *composition* (class
weights per round, boss rounds, a real difficulty curve — placeholder is
linear size growth), bone-marrow slot expansion (§2a, slots fixed at 5),
ATP tower upgrades (no upgrade system yet), and anything addressing §2a's
"round 1 is buy-then-observe with no in-round action" risk.

### Reworked — Sprint 9 (Director, 2026-08-29, after the Sprint 8 playtest)

The round model above was replaced after playtest. Three complaints:
breaches fired during the buy phase (only spawning paused); the round-end
despawn of the player's army "looked like a bug"; round 1 was
anticlimactic (adhesion 0.12 → almost everything excreted).

- **The buy phase FREEZES TIME** — not just spawning. Pathogens on the
  wall stop rolling breaches, immune cells stop, the DC shuttle pauses.
  Press Start and everything resumes exactly where it was. (`RoundClock`.)
- **The battlefield PERSISTS.** Fielded immune cells and loose pathogens
  both carry into the frozen buy phase and the next round delivers on top
  of them. The "emitted cells die at round end" of §2 is retired.
- **A round is delivered by a CONTAMINATED FOOD ITEM** — a bolus that
  enters the lumen, transits the flow, and releases the round's batch in
  a few bursts *at the wall* as it travels (so the batch adheres instead
  of washing through — this is also the "faster cadence"). It is a pure
  delivery vehicle, not attackable. **The round ends when the food has
  delivered its batch and exited the lumen**, not when the board is clear
  (the board is never clear now). Predictable round length ≈ transit time.
- **Each round has a gut-themed tagline and a per-class mix** — a light
  version of "round batch composition" above ("Undercooked egg,
  Salmonella"; "Contaminated water, poliovirus"). `RoundScript`, ~6
  hand-written rounds then a procedural fallback.
- **Difficulty, placeholder:** `BatchSizeBase`/`GrowthPerRound` 8/3 →
  **16/6**; `AdhesionChanceAtWall` 0.12 → **0.30**. New `InvasionTuning`
  `FoodItemTransitSeconds` 30 / `FoodItemBurstCount` 4 /
  `FoodItemWallHugDepth` 1. Economy (`RoundStartLumpSum`, `AtpPerKill`)
  **unchanged** — expect ATP to build up faster now that you don't
  rebuild each round; retune later.
- **Still deferred:** emergency granulopoiesis (§6c), a real difficulty
  curve / boss rounds, the food item being destructible, a run restart.
  §2a's "round 1 buy-then-observe" risk is unaddressed but softer now (a
  persistent field means the screen is never empty).

### Buying is LIVE — Sprint 16 (Director, 2026-09-04)

**Every buy surface stays open while a round is running.** The shop, the
tower picker and the progenitor upgrade panel are no longer gated on
`Phase == Building`. In the Director's words: if you can't afford an
upgrade, that forces you to pay attention and move fast when you finally
can — buying one to survive a wave that would otherwise have overwhelmed
you.

The dynamic this creates already existed in the economy and was simply
invisible: `EconomyTuning.AtpPerKill` is paid through
`EconomyHooks.PayForKill` on every kill *as it happens*, so the balance
climbs during a wave. Watching the number and spending it the instant it
clears a price is now a real thing to do.

**What this does NOT change:** the between-rounds buy phase still freezes
time (`RoundClock.Frozen`, above) — live buying means the shop is *also*
open while a round is Active, not that the freeze went away. Placing a
tower mid-round works exactly as it does frozen (the emission timer
resets and emission resumes on the next unfrozen `Update`). No prices, no
income rates, and no round pacing changed with it.

**Consequence to watch at the next tuning pass:** income per round is now
spendable *within* that round, which makes ATP worth marginally more than
it was and makes a rich mid-wave save meaningfully different from a poor
one. If rounds start feeling trivially recoverable, this is the first
lever to look at — not `AtpPerKill` itself.

## 5c. Antigen specificity — the 8-bit barcode — LOCKED (Director, 2026-08-21)

§5a says knowledge is earned when a DC and a helper T cell meet. This
section says **what decides whether a given meeting teaches anything**, and
the answer is antigen specificity, modelled as a binary barcode.

### The abstraction, and what it stands in for

Real T cell receptor:MHC recognition is based on a **9mer of amino acids
with 20 possibilities per position** — 20⁹, around half a trillion
combinations. That is far too large to be a game mechanic. The Director's
simplification: **an 8-bit binary barcode** (e.g. `01011100`), 256
possible values.

That is small enough to actually match by chance in a game, and large
enough that a match *feels* like specificity rather than a formality. It
keeps the essential fiction — a T cell recognises **one** antigen, not
antigens in general — which is the thing the game needs the player to
understand.

### The loop

1. **A helper T cell progenitor** (fictional — real helper T cells mature
   in the thymus, not from a marrow progenitor; this is a deliberate
   simplification for playability) emits a helper T cell. **Each one is
   born with a random 8-bit barcode.**
2. **A DC picking up debris auto-generates a barcode** from that debris —
   the antigen it is now carrying.
3. **The DC migrates to the lymph node**, where helper T cells are.
4. **Both DCs and helper T cells home in on a shared, unique cytokine
   signal** — a *different* cytokine from the one infected tissue secretes.
   This is what makes them co-localise instead of relying on two random
   walks happening to intersect in an open space.
5. **When an antigen-loaded DC touches a helper T cell they pair for a few
   turns and stop moving.** The pairing is visible and costs both of them
   time.
6. **If their barcodes match, pathogen knowledge increments.** If not, the
   pairing simply ends and both move on.

### Why the details matter

- **The second cytokine is load-bearing, not decoration.** Two random
  walkers finding each other in an open compartment is a much worse search
  problem than a unit finding a stationary infection, and it would make
  learning feel arbitrary. A shared chemotactic signal turns the lymph node
  from "wait and hope" into a place where meetings reliably happen — and it
  reuses the cytokine machinery the game already has.
- **Pairing that freezes both units for a few turns is the cost model.**
  Without it, a DC could brush past every T cell in the node in seconds and
  specificity would be free. The pause is what makes a mismatched pairing
  an actual loss.
- **Helper T cells get a lifespan, and the progenitor keeps emitting new
  ones.** This is what creates **barcode turnover**: the population of
  barcodes in the node keeps changing, so a player who currently has no T
  cell matching the pathogen they are facing is not permanently stuck —
  wait, and the repertoire refreshes. It also means a long-lived infection
  is eventually matched, which is the right outcome.

### Numbers not yet chosen

Barcode length is fixed at 8 by the Director. Still open: how many bits
must match (exact match, or a threshold like ≥6 of 8?), pairing duration,
knowledge increment per successful match, helper T cell lifespan, how many
T cells populate the node, and the second cytokine's range and decay.

**An exact-match rule on 8 bits means a 1-in-256 chance per pairing**,
which combined with pairing time could make learning glacial. A Hamming
threshold is the obvious dial if that proves too slow — flagged here so
whoever tunes it knows the lever exists rather than reaching for barcode
length.

**Built — Sprint 8 (Director, 2026-08-29).** The 8-bit barcode is built.
Decisions taken: the match rule is a **Hamming threshold of 2** (≥ 6 of 8
bits agree ≈ 14.5% of random pairings teach) — `MatchMaxHammingDistance`
is a mutable tuning field, so exact-match (0) or looser is one write away;
**pairing is a single timed freeze** of `PairingSeconds` on both cells,
not a per-turn cost; a helper-T cell's tag is **not** banked toward
memory; the repertoire turns over via a **helper-T lifespan** plus
re-emission (a fresh random tag each time) *and* the round boundary. The
"second cytokine" co-localisation field is real (its own small
`CytokineField` inside the node). Every number is a placeholder. See
`ENGINE_STATUS.md` and `SPRINT_PLAN.md`.

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

**Wired — Sprint 7 (Director, 2026-08-28).** The life pool, the per-breach
decrement (off `InvasionTally.ReachedBase` rising), the per-round
regeneration (`LifeRegenAmount` every `LifeRegenRounds` cleared rounds),
and the `0 → Defeat` lose condition are built (`RoundController`). The
**acute consequence below (emergency granulopoiesis) is *not* built** —
still deferred with its numbers. So for now a breach is "just the counter,"
which §6c itself warns reads as a cushion; the Director's playtest of the
economy framework is expected to confirm that and the granulopoiesis work
follows.

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

**Sprint 18 (2026-10-13): the upgrades are live.** Every row of every
kind writes its field (`Units/ProgenitorUpgrades`). Two decisions made
while wiring:

- **Adaptive upgrades are per-tower too**, through `AdaptiveUnitTuning`
  (per-tower bonuses on the shared `AdaptiveTuning` base, held by
  reference). The Sprint 16 note had accepted global-per-kind. Doing it
  per-tower cost little, and it keeps this section's "an upgrade is a
  write to one tower" true for every kind.
- **Clonal expansion raises the production rate as well as the cap.**
  The cap alone was inert: a helper-T niche's steady state is lifespan ÷
  interval ≈ 20 / 3.5 ≈ 6, under its cap of 8. Clonal expansion is
  proliferation, so the rate (×(1 + 0.5·level)) is the honest lever and
  the cap stays the ceiling. **General rule for cap-style upgrades:**
  check the cap binds at default numbers, or the upgrade buys nothing.
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

### Crowd desensitisation — the anti-swarm (Director, 2026-10-25)

**The problem.** Every innate cell reads the same cytokine field, so they
all walked to the same pathogen. The Director's report: macrophages and
neutrophils "move in a swarm and let bacteria through all the time" — one
dogpile and a wide-open wall.

**What it is not: physical repulsion.** Cells are not billiard balls, and
a bounce would have broken the cooperation Sprint 23 built. What is
modelled instead is the real mechanism. Neutrophil swarming is
*self-limiting*: GPCR desensitisation acts as negative feedback that stops
migration once a cell senses high concentrations of the self-secreted
attractants that started the swarm (Kienle 2021, *Science*). **A crowded
cell stops finding the crowd attractive.**

Implemented as a penalty subtracted from each candidate step's cytokine
reading *before* the chemotaxis softmax, so a crowded direction merely
looks less attractive than an empty one — a cell with nowhere better to go
will still go there. Same-kind only: a macrophage and a neutrophil working
one pathogen is fine and realistic; eight neutrophils on it is the bug.

**The shape is the design**, and it has to thread a needle:

| Dial | Default | What it does |
|---|---|---|
| `RepulsionFreeCompanions` | 2 | Companions tolerated for free. **Two, so three cells can converge** — exactly the number Sprint 23's cooperation bonus rewards. |
| `RepulsionExponent` | 1.6 | Penalty rises as `surplus^exponent`. Above 1 = forgiving at first, sharp later: the fourth cell is barely discouraged, the eighth is strongly pushed off. |
| `RepulsionStrength` | 0.55 | Scale, in cytokine units. 0 disables it. |
| `RepulsionRadiusFineTiles` | 6 | How far a cell notices its own kind. |

**Both extremes are worse than the middle, and that is measured, not
assumed.** Kienle found that disabling the swarm brake let neutrophils
scan *more* tissue but made bacterial clearance **worse**, because
containment of proliferating bacteria broke down. Turning
`RepulsionStrength` up until cells never cluster would reproduce exactly
that failure in the game: better coverage, worse killing.
`Sprint28Verification` pins the free allowance at ≥2 so a future tuning
pass cannot quietly make the cooperation bonus unreachable.

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

**Revised — Sprint 12 (Director, 2026-08-29).** In playtest the Director
always turned cytokine sensing on immediately, so step (2) is no longer a
binary on/off purchase: **sensing is ON from the start** (rung 1 as the
pure-random-walk baseline is now only reachable via the `C` debug
toggle), and the *purchasable upgrade* is **"Cytokine sensing +"** — a
repeatable buy that sharpens the gradient bias for every unit
(`Chemotaxis.EffectiveSharpness`). Rungs (3) directed chemotaxis and (4)
tissue residency are unbuilt and remain the later steps of the ladder.

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
