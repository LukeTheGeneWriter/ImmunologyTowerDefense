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

**Update (Director, 2026-08-21).** Two changes on top of the above: the
macrophage kill limit goes to **20** (15 read as too low), and all
lifecycle numbers must be **parameterized rather than hardcoded**, so a
future progenitor upgrade can offer "bump this tower's kill counts" as a
purchasable option — see scope items 4 and 5. Two gaps the head session
surfaced when reading the Sprint 2 code (kill attribution doesn't exist;
contact detection is coarse-slot-level, not per-unit) were also folded
into this sprint's scope rather than deferred — items 6 and 7.

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
macrophage just despawns cleanly and frees its tower's slot. **Limit:
20 kills** (Director, 2026-08-21 — he judged the previously drafted 15
"a little low"; 20 is four times the neutrophil's, keeping the
"longer lived, less prone to a terminal burst" contrast from
`GAME_DESIGN.md` §6d intact).

**5. All lifecycle numbers must be parameterized, not hardcoded consts
(Director, 2026-08-21).** Max active children, neutrophil kill limit,
macrophage kill limit, and degranulation burst multiplier are all
**tuning fields, not `const`s** — the explicit reason is that progenitor
upgrades will later offer "bump this tower's kill count" as a purchasable
option, so the values have to be per-tower mutable state from the start.
Concretely:
- Defaults live on `UnitProfile` (per unit kind), the existing home for
  per-kind tuning.
- Each bone marrow slot/tower holds its **own** current values, seeded
  from the profile defaults at placement. A future upgrade mutates the
  tower's copy; nothing else needs to change.
- An emitted unit is given its tower's current values **at emission
  time** and keeps them for life. A tower upgraded mid-round therefore
  improves its *future* children, not the ones already in the field —
  simpler, and it reads correctly (a cell doesn't retroactively gain
  granules). Flagged as a judgment call in case the Director wants
  retroactive upgrades instead.
- No upgrade UI or purchase path this sprint — only the parameterization
  that makes one buildable later.

**6. Kill attribution (new — required infrastructure, folded in
2026-08-21).** Items 3 and 4 are unbuildable without it:
`PathogenAgent.ReceiveDamage(float)` currently takes a bare amount, so
nothing knows *which* unit landed a killing hit. Change it to carry the
attacking `SearchUnit` (e.g. `ReceiveDamage(float amount, SearchUnit
source)`), and when health crosses zero, credit exactly one kill to that
source. Exactly one: if several units damage the same pathogen on the
same tick, only the hit that actually crosses zero counts — no split or
shared credit. Update `SearchUnit.CheckContact` and
`Assets/Editor/CombatVerification.cs` accordingly. A `null` source must
stay legal (spread/environmental damage, and the existing harness
assertions that call `ReceiveDamage` directly).

**7. Contact detection: coarse slot → fine-tile proximity (new, folded in
2026-08-21).** Documented as open question 3 in `docs/INTERFACE.md`:
contact currently fires whenever a unit's fine tile falls anywhere in an
occupied *coarse* slot, so **every** unit in that 7×7 slot damages the
pathogen every tick — an accidental stacking bonus, and now that kills
are attributed it would also scatter kill credit semi-randomly among
units that were never actually near the target.

Replace with a proximity test against the pathogen's stored `Current`
fine coordinate: a unit deals contact damage only if it is within
`ContactRadiusFineTiles` of it (Chebyshev/Manhattan distance on the fine
lattice — implementer's call, state which was chosen). **Starting default:
2 fine tiles**, a tunable field, not a const.

**Do not make this an exact-tile test.** With 49 fine tiles per coarse
slot, requiring exact coincidence would make a random-walking unit almost
never connect, and combat would effectively stop working. The radius is
the point: close enough to touch, loose enough to actually happen. If the
resulting time-to-clear feels wrong in playtest, tune the radius — don't
revert to coarse-slot detection.

Consequence to expect and verify: clearing gets somewhat slower across
the board (fewer simultaneous attackers per tick), and Sprint 2's
combat-timing figures no longer hold. That's intended, but it interacts
with the population cap — if clears get much slower while emission stays
capped, pathogens may outpace the player. Report the observed change
rather than silently re-tuning other numbers to hide it.

**8. Explicitly not in scope.** Any upgrade system (reducing degranulation
damage, player-triggered timed self-destruct — both named in
`GAME_DESIGN.md` §6d as the eventual payoff, neither buildable without an
upgrade system that doesn't exist yet). ATP/economy. Real fibrosis
accounting. Parasites. Adaptive immunity. Everything Sprint 1/2 already
built (lattice, search, cytokine sensing + heatmap, placement, pathogen
classes, viral spread) must keep working unchanged.

## Stopping point (definition of done)

- [x] A tower placed and left alone stops emitting once it hits its
      max-active-children cap, and resumes once a child dies.
- [x] A tower whose entire population dies at once still only refills at
      its emission-rate cap, not instantly.
- [ ] A neutrophil that reaches its kill limit visibly degranulates
      (self-destructs with a visible effect) and deals collateral damage
      if something occupies its slot.
- [x] A macrophage that reaches its (higher) kill limit quietly retires,
      no collateral damage.
- [x] Both depletion paths correctly free their tower's population slot.
- [x] Kill limits, max-active-children, degranulation burst, and contact
      radius are all mutable tuning fields (per-tower where the design
      calls for it), not `const`s — an upgrade system could bump a single
      tower's kill limit without touching any other code.
- [x] Kill credit goes to exactly one unit — the one whose hit crossed
      zero — and `ReceiveDamage` with a `null` source still works.
- [x] Contact damage requires fine-tile proximity, not just sharing a
      coarse slot; a unit at the far corner of a 7×7 slot no longer
      damages a pathogen at the opposite corner.
- [ ] Total active unit count visibly stays bounded over an extended play
      session instead of growing indefinitely — the actual problem this
      sprint exists to fix.
- [x] Everything from Sprint 1/2 still works: board width, per-unit step
      speed, pooling, cytokine toggle + heatmap, placement, pathogen
      classes, viral spread.
- [x] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect the new
      systems.
- [x] `docs/TEAM_RETRO.md` has at least one new note.

The question this sprint answers for the Director: **does population
finally stay under control, and do the two depletion behaviors (neutrophil
burst vs. macrophage quiet exit) read as intentionally different from each
other, not just as a bug where units randomly vanish?**

## Verification result — head session, 2026-08-21

Two boxes above are deliberately left unticked. Both are the same gap:
**nothing was confirmed through the running build's UI this session**,
because scripted clicks could not take window focus (`SetForegroundWindow`
refused; the build doesn't tick unfocused, so two captures 75s apart came
back pixel-identical). The build itself launches clean, renders, ticks, and
shows the new HUD readout.

- *"A neutrophil that reaches its kill limit **visibly** degranulates"* —
  the mechanism, the collateral burst, the freed slot, and the pooled
  return are all verified headlessly, and `DegranulationFlash` exists and
  is distinct in code. Whether the flash **reads** as an event has not been
  seen by anyone yet.
- *"Total active unit count **visibly** stays bounded over an extended play
  session"* — verified in simulation (5 towers, 300 simulated seconds,
  peak 50 ≤ 50, never exceeded at any point, against 375 uncapped) and the
  HUD line that would show it renders correctly at zero. Not yet watched
  climbing to a cap and holding there in a real session.

Everything else passed: 76/76 new lifecycle assertions, Sprint 2's 35/35
combat assertions, Sprint 1's cytokine numbers identical (OFF
2.99/3.14/2.84, ON 0.20/0.00/0.00), Windows build succeeded (93,295,368
bytes, 0 errors), zero runtime exceptions. Full detail in
`docs/ENGINE_STATUS.md`.

**Flagged for the Director, not silently absorbed** (per item 7's own
instruction): proximity contact cut hit frequency to ~50% of the Sprint 2
rate — macrophage 50.0%, neutrophil 49.2%, measured over 200k simulated
ticks. Clearing is about half as fast per unit, at the same time as a
population cap. Nothing else was re-tuned to compensate.
