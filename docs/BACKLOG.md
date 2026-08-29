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

### Sprite / art pass — a future sprint (Director, 2026-08-29)

Everything on screen is still a flat-coloured `RuntimeSprites.SquareSprite`
quad (see `docs/UI_STYLE_GUIDE.md` for the full current palette and the
sorting-order / footprint contract to preserve). A dedicated sprint to
design and wire real sprites:

- **Scope**: host-cell states, the immune cells (macrophage / neutrophil /
  dendritic cell / helper-T lymphocyte), the pathogen classes, the
  contaminated food item, the effect flashes, and the compartment
  backdrops. Keep the per-instance tint hook so state (cargo, paired,
  infected, cytokine heat) still reads.
- **Constraints from `UI_STYLE_GUIDE.md`**: the sorting-order table, the
  fine-tile footprint sizes, the "intracellular infection shows as the
  host cell, not itself" rule (§4a — no own sprite), and the five
  mutually-unmistakable flash colours (winning vs. losing).
- **Art direction**: `docs/handoff-map01-intestine.md` §8 already has a
  working answer (histology palette, clinical register) for the first
  playable slice — start there.
- **Likely paired with**: a real buy UI (the point at which installing
  `com.unity.ugui` or committing to UI Toolkit is a conscious call — see
  `TEAM_RETRO.md` Sprint 1), since the shop / picker / HUD are all IMGUI
  placeholders too.
- **Probably wants a Design agent** dispatched for the actual asset
  design, with the head wiring them in — this project has never used one.

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

## Opened by the barcode design (2026-08-21)

`GAME_DESIGN.md` §5c fixes barcode length at 8 bits. Still unchosen, all
Sprint 7 or later:

- **Match rule.** Exact 8-bit match is **1 in 256 per pairing**, which
  combined with pairing downtime could make learning glacial. A Hamming
  threshold (≥6 of 8, say) is the dial to reach for before changing barcode
  length. Needs a decision informed by how many T cells are in the node and
  how fast DCs cycle.
- **Pairing duration** ("a few turns") — the cost of a mismatch.
- **Knowledge increment** per successful match, and how it maps onto §5's
  threshold ladder.
- **Helper T cell lifespan**, which sets the barcode turnover rate. This is
  the safety valve that stops a player being permanently unable to match a
  pathogen, so it needs to be short enough to matter.
- **T cell population in the node**, and whether the helper T progenitor
  shares the bone marrow's slot budget or has its own.
- **The second cytokine's range and decay.** It is a different signal from
  the infected-tissue cytokine and needs its own field; note the existing
  field is `strength / (1 + distance)` with no cutoff, which was measured
  as too flat at large scale (see the Sprint 4 entry above).
- **Whether a spent DC dies or returns empty** (carried over from §5a).

## Opened by Sprint 5 (2026-08-28)

- **Viral spread is a one-shot chain, not a front.** `PathogenAgent.hasSpread`
  means each infected cell infects exactly one neighbour, ever, so an
  infection random-walks through tissue as a snake rather than spreading
  outward as a blob. The firebreak still emerges and `CombatVerification`
  already called this "chains across generations," so it may be intended —
  but whoever tunes viral behaviour should consciously decide whether a
  real front is wanted (options: allow multiple simultaneous spreads per
  cell, or drop `hasSpread` and rate-limit differently). Not a bug, not
  scoped.
- **A 1-cell-thick dead gap is hoppable.** The firebreak is emergent: a
  spread event can drop a transient free virus particle *on* a single dead
  cell, and it can then step to the healthy cell on the far side before
  its `VirusFreeSurvivalSeconds` (6s) timer kills it. Two or more dead
  cells, or a full-lane band, is a hard wall. Consistent with
  `GAME_DESIGN.md` §1a's "slipping past one or two cells is allowed and
  occasional," so probably fine — recorded so it isn't mistaken for a bug
  later.
- **Every Sprint 5 number is an unvalidated default.** `TissueTuning`
  (`HostCellMaxHealth` 10, `HostRegenerationSeconds` 20,
  `DebrisSelfDissipationSeconds` 60), the four new `InvasionTuning`
  class-advance knobs (`VirusFreeSurvivalSeconds` 6,
  `IntracellularEntryChance` 0.5, `IntracellularResidenceSeconds` 12,
  `LargeBacteriumHostDamagePerStep` 2.5), and the macrophage's
  `EfferocytosisDebrisPerTick` 0.05. All mutable statics/fields grouped
  for a tuning pass. Mechanics-first per the Director's standing
  instruction — flagged, not touched.
- **The intracellular-bacterium lysis exit is invented.** `GAME_DESIGN.md`
  §1b step 4 says an intracellular bacterium is "hidden when in" but does
  not say *how* it leaves a cell. Sprint 5 gave it a residence timer
  (`IntracellularResidenceSeconds`) after which it lyses out, killing the
  cell. If the Director wants a different exit condition (e.g. only on
  being attacked, or never — it just stays until the cell is cleared),
  this is the knob.
- **Efferocytosis vs. antigen-sampling competition is now half-real.**
  Macrophages clear debris (this sprint). The other half — a dendritic
  cell sampling that same debris for knowledge (`GAME_DESIGN.md` §1c) — is
  Sprint 7. When it lands, the "clearing removes what a DC would have
  sampled" tension (already flagged under "New, opened 2026-08-21") gets
  its real numbers: `EfferocytosisDebrisPerTick` vs. DC sample rate.

## Sprint 6 planning notes (Director, 2026-08-28)

The virus + intracellular-bacterium rework (`GAME_DESIGN.md` §4b) is
**Sprint 6** — it jumped ahead of the ATP economy and the DC shuttle after
the Sprint 5 playtest. Remaining queue, order not finally fixed: ATP
economy + round loop, and the DC shuttle (§5a — the Director's most-wanted).

Deferred out of Sprint 6, needed eventually:

- **Dedicated stress-sensor units — γδ T cell, CTL, NK-like.** §4b's
  contact stress-sense roll is *low* for innate cells (all Sprint 6 ships).
  These units get a *high* roll — reliable recognition of an intracellular
  infection — and are the concrete bridge from innate to adaptive. The γδ T
  cell is already sketched in §4 (tissue-resident, no classical priming).
  Not costed, not scheduled. Sprint 6 leaves the per-unit-kind stress-sense
  field on `UnitProfile` so these slot in without a data-model change.
- **A patrol movement pattern for stress-sensor cells** (Director's note).
  Distinct from the §7/§9 search-ladder chemotaxis — a stress sensor is not
  chasing a gradient, it is walking a beat and checking cells by contact.
  Ships with the units above.
- **Macrophage homing on debris (efferocytosis chemotaxis).** Sprint 5's
  efferocytosis is opportunistic — a macrophage clears debris it happens to
  stand on. The Director wants macrophages to *sense and move toward*
  debris ("find-me" signalling). Good fit for a later combat-feel pass;
  explicitly **not** required by Sprint 6. Note it interacts with the
  loud/quiet death axis: a necrotic (loud) death should pull harder than a
  quiet one.

Partly addressed by Sprint 6:

- **~~Viral spread is a one-shot chain, not a front~~** — §4b adds
  *budding* as a second per-species spread mode (a growing disk), alongside
  the chain. The chain stays for non-budding species. Spontaneous burn-out
  also added.

## Opened / updated by Sprint 6 (2026-08-28) — the §4b rework shipped

- **The innate stress-sense chance is the dial the game turns on.**
  `StressSenseChancePerTick` (macrophage 0.03, neutrophil 0.02) decides
  whether an established intracellular infection is "a problem you chip at"
  or "a wall you can't get past." Too high and a macrophage line
  trivialises intracellular infection before the stress sensors exist; too
  low and it reads as "my units do nothing." First real target for a
  tuning pass, and it wants the Director's playtest specifically.
- **`VirusBuddingSpeciesChance` is a placeholder for a species roster.**
  Budding vs. contact-chain is currently a per-spawn coin flip, so a
  budding infection's established children independently re-roll (some
  snake). When a pathogen-species system lands (needed anyway for a real
  enemy roster, `GAME_DESIGN.md` TBD list) this becomes a fixed per-species
  trait, and so does "buds / burns out / drains at rate X".
- **~~Viral spread is a one-shot chain, not a front~~ — partly addressed.**
  §4b's *budding* species is a growing disk (repeated emission, no
  `hasSpread`); the *chain* species still snakes. Both are now real. The
  open part: whether the chain species should exist at all, or whether all
  viruses should bud, is a species-design question for later.
- **Dedicated stress-sensor units (γδ T / CTL / NK) + their patrol
  pattern** — still deferred (see the Sprint 6 planning notes above). §4b
  ships only the low innate roll; these are the high roll, and the bridge
  to adaptive. The `StressSenseChancePerTick` field is already on
  `UnitProfile` so they slot in without a data-model change.
- **Macrophage homing on debris (efferocytosis chemotaxis)** — still
  flagged, still not scoped. Sprint 6 didn't touch it. Note it now also
  interacts with the loud/quiet death axis: a necrotic (stress-sense or
  burn-out) death should pull harder than a quiet one.
- **"Loud" is only a flash right now.** §4b says a loud necrotic kill
  broadcasts a strong DAMP that recruits more innate cells and feeds
  fibrosis (§6). Neither recruitment nor fibrosis exists, so a stress-sense
  kill and a burn-out are "loud" only in the visual. The hooks are
  commented in `SearchUnit.TryStressSenseAt` / `PathogenAgent.BurnOut`.

## Opened / updated by Sprint 7 (2026-08-28) — the economy + round framework shipped

The framework is in; the balance is not attempted. What a real economy /
difficulty pass needs, none scoped:

- **All of `EconomyTuning`.** Tower prices, the round-start lump, per-kill
  ATP, starting ATP, the life pool size, regen cadence, and the batch-size
  curve (`BatchSizeBase` + linear growth). The Director's playtest of the
  loop is the input.
- **Round batch composition.** Right now round N is just "N pathogens,
  linearly bigger" with the normal class weights. A real curve wants
  per-round class mixes, harder pathogens introduced over time, and
  probably milestone / boss rounds. Depends on a pathogen-species roster
  (also needed for §4b's budding-vs-chain trait — see the Sprint 6 note).
- **`GAME_DESIGN.md` §2a's "round 1 is buy-then-observe" risk.** The buy
  phase is now real, but round 1 still has no in-round player action once
  you press Start. §2a's suggested fix (a manually placed cytokine signal
  or similar) is unbuilt.
- **Emergency granulopoiesis (§6c).** The acute breach punishment — a
  forced surge of immature neutrophils, bone-marrow capacity consumed for
  several rounds, an ATP income penalty. Deferred with its numbers; the
  life pool is wired without it, so a breach is currently just the
  counter.
- **Bone-marrow slot expansion (§2a).** Slots are fixed at 5 and free.
  §2a wants a starting count + an ATP cost curve for buying more — the
  game's primary economic constraint, per that section.
- **Tower upgrades with ATP.** The §6d/§5c upgrade hooks (bump a tower's
  kill count, reduce degranulation damage, the helper-T barcode line) have
  no purchase path — there is no upgrade system, only the framework a
  future one would spend from.
- **A run restart.** `RoundController` has no reset — `Defeat` is
  terminal, and its breach baseline is snapshotted in `Initialize`. A
  "play again" needs a fresh controller (and `InvasionTally.Reset()`).
- **`EconomyHooks.PayForKill` as an instance path** if the game ever runs
  two boards / two wallets at once (currently a process-global static).

## Opened / updated by Sprint 8 (2026-08-29) — the DC shuttle + barcode shipped

The shuttle loop and the 8-bit barcode are built and harness-verified.
What's flagged, none scoped:

- **§5's threshold ladder — the whole point of the knowledge number.**
  Sprint 8 wires the % and shows it; it unlocks nothing. Next: MHC-I
  precise kill (~10%), neutralisation / reduced adhesion (~20%), recall
  speed (~30%), weak opsonisation (~45%), complement (~60%), secretory
  IgA at depth 0 (~70%), specific sensing / the search-problem capstone
  (~90%). This is the natural next sprint.
- **All of `AdaptiveTuning`.** Match threshold (Hamming 2), knowledge per
  match, per-class antigens, cargo capacity, debris-sample bite, pairing
  time, lymphocyte lifespan, node field strengths, DC/helper-T emission
  cadence and caps. The Director's playtest of the shuttle is the input —
  especially whether the match rate (≈14.5% per pairing) plus the walk
  times makes knowledge accrue at a watchable rate or a glacial one.
- **A real pathogen-species roster.** `PathogenClass` (3 values) is the
  species key; each has one fixed antigen. A roster makes knowledge key
  off species id, each species roll its own antigen (and carry its
  budding-vs-chain trait, §4b — same dependency the Sprint 6/7 notes
  flag). Mutation's "discrete step-change discount" to a species'
  knowledge (§5) has nowhere to attach until then.
- **B cells.** §5c is helper-T only; B cells (antibody, neutralisation,
  the §5 ladder's antibody-driven rungs) are unbuilt.
- **Passive lymphatic drainage as a knowledge sink (§1c).** Only the
  DC-shuttle debris fate is built. Unsampled debris still just
  self-dissipates; there's no "drains to the node, deleted by resident
  macrophages, nothing learned" path — which §1c says is what makes
  deliberate shuttling worth paying for.
- **~~DC homing on debris~~ / macrophage homing on debris ("find-me"
  signalling).** **DC patrol got Sprint 10's lane-repulsion instead** —
  DCs spread across the lanes rather than chasing debris, which covers
  the "DCs don't get around" concern without a gradient. **Macrophage**
  debris homing (efferocytosis chemotaxis) is still open — Sprint 5's
  efferocytosis is opportunistic (a macrophage clears debris it happens
  to stand on); the Director wants macrophages to sense and move toward
  it, and a necrotic/loud death should pull harder than a quiet one.
- **The adaptive arena keeps ticking during the buy phase and Defeat.**
  ~~`AdaptiveDirector.Update` doesn't gate on `RoundController.Phase`.~~
  **Fixed in Sprint 9** — it gates on `RoundClock.Frozen` now, along with
  everything else.
  Cosmetic; a real freeze on GAME OVER would gate it.
- **`AdaptiveDirector` runs its own `Clock`,** not aligned to the tissue
  board's `Time.time`. Fine for now (lifespan / pairing only need it
  internally consistent); a feature needing both clocks in lockstep would
  have to thread one through.
- **The "don't eat me" signal / clearance-vs-sampling ratio knob** (§1c,
  already flagged) now has a concrete second consumer: a DC sampling a
  pile also clears a bite of it, so `DcDebrisSamplePerBite` vs.
  `EfferocytosisDebrisPerTick` is the real tension to tune once balance
  starts.

## Opened / updated by Sprint 9 (2026-08-29) — the round model was reworked

Buy-phase freeze, persistent battlefield, and food-item delivery are in.
What's flagged, none scoped:

- **A real difficulty curve.** Sprint 9 just doubled the batch and adhesion
  placeholders and added a per-class mix (`RoundScript`). A real curve
  wants milestone / boss rounds, harder pathogen behaviours introduced
  over time, and pacing tuned against the persistent-army economy. Still
  depends on a pathogen-species roster.
- **The economy vs. a persistent army.** `RoundStartLumpSum` /
  `AtpPerKill` are unchanged; with no per-round rebuild, ATP should
  accumulate faster. The Director wants to judge from a playtest before
  retuning — likely the lump sum drops or per-kill carries more of the
  weight.
- **`GutInterface`'s roll clock jumps on unfreeze.** The spawner passes
  `RoundClock.Time` (frozen-aware) but `GutInterface.Tick` compares
  `now - lastRollTime`, so every occupied wall position rolls once on the
  first unfrozen tick — a breach can fire in round-frame 1. One roll, not
  a flood; revisit if it reads badly. The clean fix is a per-position
  roll clock that also freezes.
- **A destructible food item.** Right now it's a pure vehicle — you can't
  interrupt a delivery. Making it attackable (shoot the bolus before it
  drops its cargo, or reduce the burst it delivers) is a plausible
  mechanic; `FoodItem*` tuning + the single-GameObject visual are where it
  attaches.
- **The buy phase freezes tissue healing and the adaptive shuttle too.**
  Intended, but a long buy phase genuinely pauses debris dissipation,
  regrowth, and DC travel. If that feels wrong, those could be exempted
  from the freeze (heal/learn between rounds) while combat stays frozen.
- **§2a's "round 1 is buy-then-observe" is softer but not solved.** A
  persistent field means the screen is never empty, but round 1 still has
  no in-round player action once you press Start.
- **`RoundClock` as an instance path** if the game ever runs two boards.
- **A run restart.** Still unbuilt — `Defeat` is terminal. Now there's
  `RoundController.DespawnAllFieldedUnits()` as one piece of the reset;
  it still needs a fresh `RoundController` (breach baseline) + `RoundClock.
  Reset()` + `InvasionTally.Reset()` + despawning loose pathogens.

## Opened / updated by Sprint 11 (2026-08-29) — the shop + ladder framework

The shop, per-tower upgrades, and the knowledge ladder are on screen but
drive nothing. Each purchase's real mechanic (design in GAME_DESIGN.md
§1d / §5 / §6b):

- **Barrier: mucus turnover (§6b).** Raise the shed rate — return a
  fraction of gut-wall residents to the lumen each round to be excreted.
  Categorically "flushing not fighting"; sequences before secretory IgA.
- **Host: dsRNA sensor → immunogenic apoptosis.** An infected upgraded
  cell has a ~20% chance to self-destruct (cell + resident, clean), and
  releases a **DC-recruiting "eat this debris" cytokine** — a *third*
  cytokine field, distinct from the recruitment and co-localisation
  signals. This is the piece that most needs a new field. Other sensor
  variants (dsDNA/LPS/flagellin) follow.
- **Host: reduced viral entry / bacterial resistance.** Per-cell (or
  global-while-owned) scalars on `VirusEntryChancePerTick` /
  `LargeBacteriumHostDamagePerStep`.
- **Crypts.** Placeable; faster regrowth in a radius — the spatial
  version of Sprint 11's neighbour-regrowth. Ties to §6's crypt model.
- **Progenitor upgrades (§6d).** The wiring exists (`UnitLifecycleTuning`
  is per-tower mutable, a `SearchUnit` holds a live ref). Sprint 11's
  `UpgradeTower` just needs to actually write a field: bump `KillLimit`,
  cut `DegranulationBurstMultiplier`, raise `StressSenseChancePerTick`,
  etc. — one write per upgrade kind. Probably wants distinct upgrade
  *kinds* per slot rather than one opaque level.

- **The §5 knowledge-ladder capabilities.** All six are unbuilt:
  - **CTL (~10%)** — a new unit; the quiet precise-kill path
    (`TissueGrid.ReleaseIntracellular` is reserved for exactly this).
  - **Neutralizing antibodies (~20%)** — an adhesion multiplier for a
    known species; cheapest to build.
  - **Memory T cells (~30%)** — on re-encounter, spawn a CTL burst; needs
    the CTL unit + a "seen this species before" flag.
  - **Fc receptor (~45%)** — antibody entities that stick to innate
    cells / opsonise pathogens.
  - **Complement (~60%)** — passive damage tick to an antibody-coated
    pathogen, no cell needed.
  - **Secretory IgA (~70%)** — antibody acting in the lumen before
    adhesion; the only lumen-side capability.
  - The **~90% capstone** (antibody-coated pathogens as chemotactic
    beacons) is what Fc receptor grows into.

- **Shop prices vs. the persistent-army economy** (still the open Sprint 9
  question). `ShopTuning` prices are blind placeholders.
- **`ShopLedger` has no round-boundary / restart handling** — it just
  persists for the run (correct for now).

## Opened / updated by Sprint 12 (2026-08-29)

- **Cytokine sensing is on by default; the *upgrade* is buyable** (a real
  shop effect via `Chemotaxis.SensingUpgradeLevel`). Rung-1 (pure random
  walk) is now only reachable via the `C` debug toggle. If the toggle is
  a player footgun, hide it behind a debug flag.
- **The sensing upgrade is player-wide (one global static), not
  per-tower.** If per-tower cytokine sensing is ever wanted it moves onto
  `UnitLifecycleTuning` like the §6d numbers.
- **~~DC patrol wasn't spreading / sweeping~~ — fixed Sprint 12.** The
  Sprint 10 lane-repulsion compared coarse indices (fired ~1/7 of steps);
  now fine-grained + a threat-axis band sweep. `DcLaneRepelStrength` /
  `DcPatrolSweepBias` want a playtest tuning pass.
- **DCs still spend little time patrolling** — with 16-pathogen rounds,
  debris is everywhere, so a DC samples within a couple of ticks and
  spends most of its life shuttling. If the patrol behaviour still reads
  as under-used after Sprint 12, consider letting a DC sample several
  piles before heading to the node (a cargo capacity > 1).
