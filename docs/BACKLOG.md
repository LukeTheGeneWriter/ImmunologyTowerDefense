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
