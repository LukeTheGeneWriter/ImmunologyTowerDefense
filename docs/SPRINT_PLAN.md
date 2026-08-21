# Sprint Plan — Sprint 3

## Sprint 2 — closed 2026-08-19

Delivered: bone marrow placement, a lymph node placeholder, and
pathogen-class combat with viral spread. Director playtested it directly
and confirmed it works — and surfaced the next real problem: progenitors
have no population cap, so active cell count grows unbounded over time.
Full history in `docs/CHANGELOG.md` and `docs/ENGINE_STATUS.md`.

## Direction for Sprint 3 (Director, 2026-08-19)

Fix the unbounded growth with a population cap **tied to each individual
progenitor**, not a systemic/global signal — a deliberate break from real
hematopoiesis, made because it keeps individual towers (and eventually
their upgrades) meaningful. Full design writeup is in `docs/GAME_DESIGN.md`
§6d — read that first, this is the implementation brief for it.

## Scope

**1. Max active children per tower (new).** Each bone marrow slot/tower
tracks how many of its own emitted units are currently alive. Once at the
cap, the tower stops emitting — even if its emission timer has elapsed —
until one of its children dies and frees a slot. Requires each emitted
unit to know which tower it came from and to notify that tower on death
(depletion or otherwise), so `BoneMarrowManager` can decrement the count.
Starting default: **10** per tower — the Director's own example number,
treat as tunable.

**2. Emission-rate cap (already exists, keep it).**
`BoneMarrowManager.EmissionIntervalSeconds` (Sprint 2, currently 4s)
already throttles how fast a tower can produce a *new* cell. Per
`GAME_DESIGN.md` §6d this is deliberately kept as a *second*, independent
cap — it's what stops a tower whose whole population just died at once
from instantly bursting back to full. No change needed here beyond making
sure it still applies once the max-children cap exists (a tower at zero
children but still mid-cooldown should not emit early).

**3. Neutrophil kill-count depletion → degranulation.** Track kills per
neutrophil (increment when a `ReceiveDamage` call from that unit is the
one that actually clears a pathogen — needs a way for `PathogenAgent` to
report back which unit landed the killing hit, not just that it died).
At the kill-count limit, the neutrophil **degranulates**: destroys itself
and deals a burst of collateral damage to whatever's in its current
coarse slot, then notifies its tower (frees a max-children slot). Starting
default: **5 kills**, collateral burst **3x** `ContactDamagePerHit`. Since
there's no host-cell-health/fibrosis system yet (`GAME_DESIGN.md` §6 is
still unbuilt), "collateral damage" this sprint means: if an infected/
occupied slot is present at the degranulation site, deal the burst damage
to it same as combat damage; if the slot is bare host tissue, there's
nothing to damage yet — that's fine, the mechanism is what matters this
sprint, fibrosis accounting comes later. Make the degranulation event
visibly distinct (a brief flash/effect) so the Director can actually see
it happen, not just watch a unit quietly disappear.

**4. Macrophage kill-count depletion → quiet retirement.** Same
kill-tracking mechanism, higher limit, no collateral damage — the
macrophage just despawns cleanly and frees its tower's slot. Starting
default: **15 kills** (three times the neutrophil's, reflecting "longer
lived, less prone to a terminal burst" per `GAME_DESIGN.md` §6d). This
number is this document's working default, not independently confirmed
by the Director — flag if it should change.

**5. Explicitly not in scope.** Any upgrade system (reducing degranulation
damage, player-triggered timed self-destruct — both named in
`GAME_DESIGN.md` §6d as the eventual payoff, neither buildable without an
upgrade system that doesn't exist yet). ATP/economy. Real fibrosis
accounting. Parasites. Adaptive immunity. Everything Sprint 1/2 already
built (lattice, search, cytokine sensing + heatmap, placement, pathogen
classes, viral spread) must keep working unchanged.

## Stopping point (definition of done)

- [ ] A tower placed and left alone stops emitting once it hits its
      max-active-children cap, and resumes once a child dies.
- [ ] A tower whose entire population dies at once still only refills at
      its emission-rate cap, not instantly.
- [ ] A neutrophil that reaches its kill limit visibly degranulates
      (self-destructs with a visible effect) and deals collateral damage
      if something occupies its slot.
- [ ] A macrophage that reaches its (higher) kill limit quietly retires,
      no collateral damage.
- [ ] Both depletion paths correctly free their tower's population slot.
- [ ] Total active unit count visibly stays bounded over an extended play
      session instead of growing indefinitely — the actual problem this
      sprint exists to fix.
- [ ] Everything from Sprint 1/2 still works: board width, per-unit step
      speed, pooling, cytokine toggle + heatmap, placement, pathogen
      classes, viral spread.
- [ ] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect the new
      systems.
- [ ] `docs/TEAM_RETRO.md` has at least one new note.

The question this sprint answers for the Director: **does population
finally stay under control, and do the two depletion behaviors (neutrophil
burst vs. macrophage quiet exit) read as intentionally different from each
other, not just as a bug where units randomly vanish?**
