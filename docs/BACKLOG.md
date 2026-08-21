# Backlog

Ideas and known issues not yet scheduled into a sprint. Owned by the head
session; feedback lands here when it's noted but not urgent.

## Open design questions (from Director decisions, 2026-08-19)

These block specific parts of `docs/GAME_DESIGN.md` from being buildable,
but none of them block Sprint 1 (see `docs/SPRINT_PLAN.md` — Sprint 1 is
scoped to the search problem only, no economy/compartments beyond tissue).

- **Round 1 economy.** The original handoff justified a 100 ATP budget by
  making the opening forced (Macrophage 40 + Cytotoxic T cell 60 = 100,
  exactly one affordable combination). Swapping in the neutrophil (likely
  ~20 ATP) breaks that: 100 ATP buys one macrophage + three neutrophils,
  or two macrophages + one neutrophil, or five neutrophils — a genuine
  choice. Arguably better (buying neutrophils in numbers is biologically
  right), but it contradicts the original intent that the player learn the
  archetypes before they learn choice. Needs a Director call on price
  points and whether "forced opening" is still a goal.
- **Grid dimensions.** Director has confirmed the board should be
  substantially larger than an early ~6×5 wireframe. Working figure:
  **24–40 coarse columns × 5 rows** (3,000–5,000 fine tiles at 7×7
  subdivision). Needs playtesting to fix, not deriving — Sprint 1 builds
  this as a configurable parameter for exactly that reason.
- **Fibrosis decay rate.** "Very slowly" needs a number. Determines whether
  innate-only play is a trap or merely a plateau.
- **Lymph node arrival delay.** A dendritic cell sampling antigen in
  tissue must travel to the lymph node before anything adaptive comes
  online — the mechanistic reason adaptive immunity lags. Needs a duration
  and a visual representation.
- **Transit cost — additional or not.** Barrier colonisation (see
  `GAME_DESIGN.md` §6b) already supplies a cost for transit: a pathogen
  passing through may colonise the mucus and become next round's problem.
  Open question is whether transit needs any cost *on top of* that.
  Candidates, retained for reference:
  1. Barrier attrition — transiting pathogens thin the mucus/glycocalyx at
     depth 1, raising adhesion rate in later rounds. Complements barrier
     colonisation rather than duplicating it.
  2. Luminal burden — uncleared pathogens that exit contribute to the
     colonising population, raising next round's spawn count.
  3. Knowledge cost — a pathogen that leaves unsampled is one the adaptive
     system never characterised, so transits slow knowledge accrual.
  (1) and (2) stack naturally and point at the same fiction. Director
  decision needed.
- **Bone marrow capacity.** Starting slot count and the ATP cost curve for
  expansion. This is the game's primary economic constraint since it caps
  tower count directly.
- **Emergency granulopoiesis numbers.** Needs: duration in rounds,
  magnitude of the forced neutrophil surge, effectiveness and
  collateral-damage modifiers on immature cells, size of the ATP income
  penalty, and the life regeneration rate. Balance figures to be found by
  playtest.
- **Memory erosion (deferred, not scoped).** For pathogen species not
  encountered in many rounds, applied to T/B cell knowledge. Distinct from
  mutation (a discrete step-change discount) and additive to it if ever
  built.

## Delivered in Sprint 2 (closed 2026-08-19)

Bone marrow/lymph node placement and pathogen-class combat, both
originally scheduled here from the Sprint 1 playtest — see
`docs/CHANGELOG.md` for what shipped.

## Scheduled into Sprint 3 (see `docs/SPRINT_PLAN.md`)

Director's direction from the Sprint 2 playtest: progenitors have no
population cap, so active cell count grows unbounded. Now written up as a
formal sprint plan (`docs/GAME_DESIGN.md` §6d has the full design):

- **Per-progenitor population cap** — deliberately not systemic/global,
  see §6d for why. Two independent caps: max active children per tower,
  and the existing emission-rate timer.
- **Neutrophil kill-count depletion → degranulation** (self-destruct +
  collateral damage burst) and **macrophage kill-count depletion → quiet
  retirement** (higher threshold, no damage) — both free their tower's
  population slot.
- Upgrade hooks (reduced degranulation damage, player-triggered timed
  self-destruct, and — added by the Director 2026-08-21 — raising a
  specific progenitor's kill counts) are named as the eventual payoff but
  explicitly deferred until an upgrade system exists. Not Sprint 3 scope,
  **except** that Sprint 3 must keep every lifecycle number as per-tower
  mutable state rather than a hardcoded const, so an upgrade is later just
  a write to one tower's field.
- Starting numbers (max active children = 10, neutrophil kill limit = 5,
  macrophage kill limit = **20** — raised from a drafted 15 by the
  Director 2026-08-21, degranulation burst = 3x contact damage, contact
  radius = 2 fine tiles) are working defaults, not balance-tested. The
  macrophage limit is now Director-confirmed; the rest are not.
- Two Sprint 2 gaps folded into Sprint 3 (2026-08-21) rather than
  deferred: **kill attribution** (`PathogenAgent.ReceiveDamage` takes no
  source, so nothing knows which unit landed a killing blow — required
  infrastructure for kill-count depletion) and **coarse-slot contact
  detection** (`INTERFACE.md` open question 3 — every unit in a 7×7 slot
  damages the pathogen every tick; becoming a fine-tile proximity test
  with a tunable radius).

## Deferred out of Sprint 2

- **Parasites (multi-coarse-slot pathogen footprint).** Per
  `GAME_DESIGN.md` §4a, the fourth pathogen class — larger, occupies
  multiple coarse slots at once. Needs real structural work to
  `TissueGrid`'s current one-pathogen-per-slot occupancy model (partial
  clearing, footprint claiming, etc.) — more engineering than the other
  three classes combined, so deliberately not folded into Sprint 2.

## Everything else

(Nothing else triaged yet.)

## Triaged from the Sprint 3 playtest (Director, 2026-08-21)

- **Immune cells overwhelmingly win. Deliberately not being fixed yet** —
  the Director's call: focus on mechanics now, tune balance later. Do not
  "helpfully" rebalance damage, kill limits, spawn rates, or the contact
  radius in the meantime; the numbers are all tunable fields precisely so
  this can be a deliberate pass once the mechanics are settled. Note this
  is the *opposite* direction from Sprint 3's measured ~50% contact-rate
  reduction, which was expected to make things harder — worth understanding
  why before tuning anything.
- **Neutrophil degranulation still unwatched.** The Director closed the
  build before seeing one fire. Mechanism is verified headlessly; the
  visual (`DegranulationFlash`) has never been seen by a human. Carried
  forward to the next playtest.
- **Progenitor upgrades apply instantly to living cells** — decided and
  implemented same day, see `GAME_DESIGN.md` §6d. No longer open.
- **Pathogen movement: no more skipping across the board** — scoped as the
  next sprint's main work, see below.

## Next up: pathogen battlefront (Director, 2026-08-21)

Current behavior, which the Director objects to: a pathogen enters at fine
column 0, marches right at 2 fine tiles/tick, and adheres at a **uniformly
random target column** anywhere on the board, passing straight through
everything in between. So infections appear at arbitrary depth with no
relationship to where the immune cells are.

Wanted instead: pathogens **push a battlefront**. They advance until they
meet resistance and largely stop there, so the infection has a *frontier*
that advances when they win ground and recedes when the immune cells clear
it. Slipping past one or two cells should be possible but occasional — the
Director's fiction for it is cracks and vessels — not the default.

Open sub-question the head session flagged, needs a Director call before
this is buildable: **which axis is the front?** Pathogens currently transit
along columns (lateral), but `GAME_DESIGN.md` §1's depth model and
`BoneMarrowManager`'s own row convention treat *rows* as depth, with immune
cells entering at the blood-adjacent deepest row. Those are orthogonal, and
that mismatch is arguably the root of the "flying across the board" feel.
See `docs/INTERFACE.md` open question 1, which has flagged this since
Sprint 1.

## Answered 2026-08-21 (Director)

- **Debris behavior** — blocks host-cell regeneration until cleared;
  macrophages clear it (efferocytosis); it also dissipates slowly on its
  own; dendritic cells can carry it to the lymph node to learn from it.
  Now written up in `GAME_DESIGN.md` §1c.
- **Lymph node arrival delay — partially answered.** The open question
  above asked what a dendritic cell samples and where. **Debris is the
  answer** (`GAME_DESIGN.md` §1c). Still open: the actual travel duration
  and how it is shown on screen.
- **Round-1 economy, grid dimensions** — grid is settled by Map 01's
  layout (`GAME_DESIGN.md` §1a): 100×40 host cells, three lateral bands.
  The "24–40 coarse columns × 5 rows" working figure above is retired.

## New, opened 2026-08-21

- **Debris vs. sampling competition.** A macrophage clearing debris
  removes what a dendritic cell would have sampled, so tissue recovery and
  adaptive learning compete for the same resource. Whether that tension
  stays sharp or gets softened (instant sampling vs. slow clearance) is a
  balance question for whenever adaptive immunity is built. Flagged
  because it is a genuinely interesting design pressure, not a problem:
  a perfectly clean defence learns nothing.
- **Host-cell regeneration rate.** Regeneration now has a blocker (debris)
  but no rate. Needed before §6 tissue recovery is buildable.
- **Debris dissipation rate.** "Slowly" needs a number — slow enough that
  macrophage clearance is clearly the better answer.

## Flagged for later, opened 2026-08-21 (Director)

- **"Don't eat me" signals as the clearance/sampling tuning lever.**
  Apoptotic cells display signals that suppress phagocytic clearance.
  Mechanically this is the natural knob on the macrophage-clearance vs.
  DC-sampling ratio (`GAME_DESIGN.md` §1c): debris that resists being eaten
  persists longer, biasing its eventual fate toward sampling rather than
  silent disposal. Two obvious future uses — a player-side upgrade, and a
  pathogen trait (a pathogen inducing strong "don't eat me" signalling
  would clog tissue with debris nobody can clear). **Explicitly flagged for
  later by the Director; not scoped, not costed.**
- **Macrophage presentation efficiency vs. DC.** Macrophages can sample and
  present debris but inefficiently, so the player manages a *ratio* rather
  than choosing between unit types. The actual efficiency gap is a number
  nobody has picked, and it decides whether a macrophage-only defence
  learns slowly or effectively not at all.
- **Passive lymphatic drainage rate.** Debris drains to the lymph node on
  its own and is silently deleted there by resident macrophages — no
  knowledge gained. The drainage rate therefore sets how much antigen is
  *lost* to inaction, which is the pressure that makes deliberate shuttling
  worth paying for. Needs a number, and needs to be visible enough that a
  player understands why their knowledge is not accruing.

## Opened by Sprint 4 (2026-08-21)

- **Cytokine sensing is much weaker at Map 01 scale.** Measured, not
  suspected: on the 30×5 board sensing drove unit-to-infection distance to
  0.20/0.00/0.00 while OFF sat at ~3; on 100×40 it goes 45.29/40.42/37.38
  while OFF sits flat at ~47. The mechanism works — ON closes steadily,
  OFF does not — but it no longer converges. Cause: `CytokineField` is
  `strength / (1 + distance)` with no cutoff, which is a steep gradient at
  3 cells and nearly flat at 47. Sprint 1 called this mechanic "should feel
  transformative"; at map scale it currently isn't. Candidate fixes (all
  tuning, none applied): steeper falloff (1/r²), a finite radius with a
  stronger local gradient, or a higher `Chemotaxis.GradientSharpness`.
  **Deliberately not tuned** — the Director's standing instruction is
  mechanics first.
- **Frame cost is vsync-capped, so unmeasured.** 4,000 cells report 8.35
  ms/frame, which is exactly the 120 Hz refresh interval. True cost is at
  most that; actual headroom is unknown. Re-measure with vsync disabled
  before trusting it, especially before adding host-cell state rendering in
  Sprint 5.
- **Base-band layout is crowded.** Bone marrow slots, the lymph node
  placeholder, and the HUD all occupy the same corner and overprint each
  other. A dimming panel behind the HUD makes it readable, but the marrow
  strip's own labels still collide with it. Needs a real layout pass —
  plausibly the first genuine job for a dispatched Design agent, which this
  project has never used.
- **Nothing reaches the base yet in a short session.** With no towers
  placed, tissue crossing takes ~70s+ at a 1s step interval and 0.7
  toward-base weight, so a 60s observation showed 0 reached base despite 12
  pathogens in tissue. Expected, not a bug — but it means the endzone
  counter needs a longer sitting to demonstrate, and it is worth checking
  whether the pacing is right once the Director plays it.

## Decided 2026-08-21 (Director), and what they open

- **ATP income: a lump sum on starting a round, plus per-pathogen-kill
  income.** Interim ("for now"), see `GAME_DESIGN.md` §5b. Supersedes the
  round-1 economy question above only in part — unit *prices* are still
  unset.
- **A round loop is now needed, and is the blocker under the above.** The
  game runs as one continuous session with no waves or round boundaries, so
  "starting a round" has nothing to attach to. Director has called it out
  as needed soon. Until it exists, round-start ATP can only be a one-off
  grant at session start. **This is the most likely candidate for Sprint 6
  to absorb alongside the buying tab.**
- **Knowledge is earned by DC:lymphocyte encounters, not a timer**
  (`GAME_DESIGN.md` §5a). This answers the long-open "lymph node arrival
  delay" question above: the delay is not a number to pick, it is however
  long the DC's walk takes. Opens instead: cargo capacity, per-encounter
  probability, increment size, how many T/B cells populate the node and
  where they come from, and whether a spent DC dies or returns empty.
- **The lymph node becomes a second search arena.** DC-finds-lymphocyte is
  the same random-walk-and-collide dynamic as tissue, so node crowding
  becomes a real variable — a sparse node learns slowly however much
  antigen arrives. Worth watching for whether that is interesting or just
  another thing to wait on.
