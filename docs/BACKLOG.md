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

## Scheduled into Sprint 2 (see `docs/SPRINT_PLAN.md`)

Director's direction from the Sprint 1 playtest, now written up as a
formal sprint plan:

- **Bone marrow and lymph node as real, placeable spaces.** Per
  `GAME_DESIGN.md` §1/§2a, progenitor towers are bought and placed in the
  bone marrow, not in tissue — Sprint 1 has no such place, units just
  debug-spawn. Needs actual layout/compartment work: a UI space for the
  player to place purchased cells, and a lymph node area for adaptive
  immunity coordination (dendritic cells, naive/activated T and B cells).
  This is the first real placement decision the player gets.
- **Immune cell / pathogen interaction ("combat").** Sprint 1's "contact"
  is a visual flash only — nothing dies, nothing is damaged. Director
  wants some functional interaction so there's something to actually see
  happen, not just search-and-flash.
- Scope, sequencing, and exact mechanics (does combat mean an instant
  kill? A damage-over-time tick? Does it differ by unit type already, per
  `GAME_DESIGN.md` §4's neutrophil/macrophage role split?) still need to
  be worked out before writing the Sprint 2 brief.

## Deferred out of Sprint 2

- **Parasites (multi-coarse-slot pathogen footprint).** Per
  `GAME_DESIGN.md` §4a, the fourth pathogen class — larger, occupies
  multiple coarse slots at once. Needs real structural work to
  `TissueGrid`'s current one-pathogen-per-slot occupancy model (partial
  clearing, footprint claiming, etc.) — more engineering than the other
  three classes combined, so deliberately not folded into Sprint 2.

## Everything else

(Nothing else triaged yet.)
