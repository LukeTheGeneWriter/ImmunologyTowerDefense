# Sprint Plan — Sprint 6

## Sprint 5 — closed 2026-08-28

Delivered `GAME_DESIGN.md` §1c: the two-layer `TissueGrid` (host +
occupant), four host-cell states with four distinct colours, debris as
terrain (blocks regrowth; macrophage efferocytosis; ~60s self-dissipation;
~20s regrowth), and a first pass at class-specific advance. **The Director
playtested it:** the cell colours read, the viral firebreak reads, and a
macrophage clearing a debris pile reads. Verified by a new
`TissueVerification` harness (53 assertions) plus all three prior harnesses
— 239 total, 0 failed.

Playtest feedback that shaped Sprint 6:

- The **intracellular bacterium** did not read — it rendered identically to
  a viral infection and mostly just looked like a pathogen walking through
  walls. More to the point, the Director replaced the model (see below).
- The **macrophage** cleared debris but did not *home in* on it —
  efferocytosis is currently opportunistic only.
- Intracellular bacteria should be **vulnerable when out, protected when
  in**, replicating at the host cell's expense, and only reliably killed by
  a stress-sensing cell.

Those decisions are now written into **`GAME_DESIGN.md` §4b** (Director,
2026-08-28). This sprint builds §4b.

## Direction for Sprint 6 (Director, 2026-08-28)

Do the **virus and bacteria reworks** now — they are the natural
continuation of Sprint 5's tissue/infection work and they are what the
playtest asked for. The ATP economy + round loop, and the DC shuttle
(`§5a`), stay queued after this; the Director has flagged the DC shuttle as
the one he is most keen on, exact order to be set at Sprint 7 planning.

**Read `GAME_DESIGN.md` §4b before anything else.** It is the authority for
this sprint; this document is the implementation brief for it. §1c (host
states / debris) and §1b (invasion loop) are the substrate it builds on.

## Scope

### 1. The stress signal + the contact stress-sense roll

Per §4b. Every `Infected` host cell (viral or bacterial) carries a **stress
signal**. It is **read on contact only** — it is *not* the recruitment
cytokine field and must not be folded into it.

- An immune cell **in contact with an `Infected` cell** rolls **each tick**
  to recognise the infection. Contact = the same fine-tile proximity test
  `SearchUnit.CheckContact` already uses.
- On success: a **loud kill** — the host cell dies necrotically (`Dead` +
  debris, a loud death per §1c), and **every pathogen inside dies with it.
  Nothing is released.**
- The per-tick probability is a per-unit-kind field (`UnitProfile` /
  `UnitLifecycleTuning`, never a const — §6d pattern). **Macrophage and
  neutrophil get a LOW value this sprint.** Dedicated stress sensors (γδ T
  etc.) are **out of scope** — leave the field on the profile so they slot
  in later, but do not build the units or a patrol pattern.
- A loud kill should be **visibly louder** than a quiet death — reuse /
  extend `DegranulationFlash` (a distinct colour, bigger) so the Director
  can see "a macrophage just caught one."

### 2. Intracellular bacterium — the real model

Replaces Sprint 5's enter → 12s timer → lyse placeholder.

- **Extracellular:** no death clock (unlike a virus — it survives out
  there). Base-biased walk, *more* inclined to wander than the current
  version. **Fully vulnerable to ordinary innate contact damage.**
- **Enters** a `Healthy` host cell it is standing on — a per-tick roll
  (`InvasionTuning`).
- **Intracellular:** **immune to ordinary innate contact damage.** A
  macrophage/neutrophil touching the cell does nothing except roll the
  stress-sense (item 1). While inside, the bacterium **replicates on a
  timer, draining `hostHealth`**. No voluntary exit.
- **Host cell drained to death:** loud death, debris, and a **burst of N
  extracellular bacteria** — `N` scales with incubation time (replication
  count). Released onto the dead cell and its free neighbours.
- **Caught by a stress-sense kill first: no burst.**
- **Rendering:** an intracellular-bacterium cell must be tellable at a
  glance from a virus-infected cell, from healthy, and from dead. The "went
  in / came out" beat must read. Consider a marker on the cell, or a
  distinct infected-cell tint per infecting class.

### 3. Virus — budding + burn-out

Keep the Sprint 5 contact-chain spread as one **per-species** mode. Add:

- **Budding (some species):** the infected cell **periodically emits a free
  virion** that does a **momentum-biased random walk** (slight bias toward
  its last heading → roughly radial expansion) and **rolls a per-tick entry
  chance** against the `Healthy` cell it is on. Each budded virion has its
  **own survival clock** (`VirusFreeSurvivalSeconds`). A budding infection
  should read as a growing **disk**, visibly unlike a chain virus's line.
- **Spontaneous burn-out:** a **fraction of viral infections** deplete and
  die on their own — loud death, debris, and their virions spilled into the
  tissue — with no immune action. Self-limiting even if ignored.
- Budding virions still only establish in **`Healthy`** cells, so the
  firebreak survives — a `TissueVerification` assertion must prove a budding
  front still cannot cross dead ground.
- "Which species buds" is a `PathogenClass`-adjacent trait; a simple
  per-spawn flag is fine this sprint (no species roster yet).

### 4. Keep everything from Sprints 1–5 working

The firebreak, host states, debris/efferocytosis, the breach burst,
base-directed advance, population caps, kill-count depletion, cytokine
sensing + heatmap, pooling. In particular the innate contact-damage path
for **extracellular** pathogens (large bacterium, extracellular
intracellular-bacterium, exposed virion) is unchanged — only the
*intracellular* case gains protection.

### 5. Explicitly not in scope

Dedicated stress-sensor units (γδ T / CTL / NK) and their patrol pattern —
`BACKLOG.md`. Macrophage *homing* on debris (efferocytosis chemotaxis) —
the Director raised it; it is a good fit for a later pass and is **not**
required here. ATP economy, prices, round loop — the sprint after. DC
shuttle, T/B cells, knowledge accrual — after that. Parasites. Balance
tuning — mechanics first, still.

## Stopping point (definition of done) — status 2026-08-28

Mechanics all landed and harness-covered (`TissueVerification` 53 → 73);
the "watch it happen" halves are the Director's playtest. `[x]` = done,
`[~]` = code done + harness-verified, visual unconfirmed.

- [~] An **intracellular bacterium** enters a host cell (cell turns a
      distinct yellow-green, `InfectedColorFor`), is **untouchable by a
      macrophage** while inside, drains the cell, and **bursts a brood** on
      drain-death. Harness: `RunClassAdvance` bacterium block. Watching it
      is the playtest.
- [~] A **macrophage recognises an infected cell on contact** and kills it
      **loudly** (1.5× magenta flash), **nothing released**. Harness:
      `RunStressSense`. Whether the 0.03/tick rate *feels* right is the
      playtest.
- [~] A **budding infection grows as a disk**, a **chain one snakes** —
      harness confirms established infections on both sides of a budding
      seed. The two side by side is the playtest.
- [~] **Infections burn out on their own**, spilling the virus + debris.
      Harness: burn-out block.
- [x] An **exposed** intracellular-bacterium still takes ordinary
      macrophage/neutrophil damage. Harness: `RunClassAdvance` (`mac.CheckContact`
      lands before it hides).
- [x] A **budding front cannot cross dead ground** — harness-asserted end
      to end (180s of budding, 0 established infections base-ward of a
      3-cell band).
- [x] Everything from Sprints 1–5 still works — Combat 36 / Lifecycle 79 /
      Map 71 all re-run green.
- [x] `TissueVerification` grown to cover stress-sense, exposed/hidden,
      replication + brood + caught-early-no-brood, budding vs chain,
      burn-out — **73 passed, 0 failed**.
- [x] `docs/GAME_DESIGN.md` §4b, `SPRINT_PLAN.md`, `ENGINE_STATUS.md`,
      `INTERFACE.md`, `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md` all
      updated.

**Handed to the Director for playtest.**

The question this sprint answers for the Director: **does an established
intracellular infection feel like something innate immunity struggles
with** — something you can only catch by luck or by burning the tissue
down — so that the eventual stress-sensor and adaptive units have an
obvious job?

## A process note for whoever is dispatched

Sprints 3, 4 and 5 were all implemented by dispatched Code agents that were
interrupted mid-task. Sprint 5's hand-off was the first clean one, because
the brief said **commit after each scope item, even if incomplete or
ugly** — and the agent did. Keep doing that. Update `docs/INTERFACE.md` as
each signature changes and append to `docs/TEAM_RETRO.md` as each judgment
call is made, not in a final sweep. A verbose, reasoning-heavy commit
message is the recovery artifact when the author disappears mid-sprint —
it has been, four times now.
