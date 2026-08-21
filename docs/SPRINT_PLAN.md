# Sprint Plan — Sprint 4

## Sprint 3 — closed 2026-08-21

Delivered: per-progenitor population cap, kill-count depletion (neutrophil
degranulation / macrophage retirement), kill attribution, and fine-tile
proximity contact. Verified 79/79 headless, both prior harnesses clean,
Windows build good. The Director playtested and reported: population is
under control, immune cells overwhelmingly win, and **balance tuning is
explicitly deferred** — mechanics first. Full history in
`docs/CHANGELOG.md` and `docs/ENGINE_STATUS.md`.

Same-day follow-up, already shipped: progenitor upgrades now apply
instantly to a tower's living children as well as its future ones
(`GAME_DESIGN.md` §6d).

## Direction for Sprint 4 (Director, 2026-08-21)

**Build Map 01's real geometry.** The Director specified the map, which
resolved the axis ambiguity that had been open since Sprint 1 — read
`GAME_DESIGN.md` **§1a, §1b, and §1c** before anything else; they are the
authority for this sprint and this document is the implementation brief for
them.

The short version: a 100×40 grid of host cells in three lateral bands —
**base** (leftmost 25 columns: bone marrow, lymph node, immune cell entry,
and the lose condition), **tissue** (middle 50), **lumen** (rightmost 25).
Pathogens flow top-to-bottom down the lumen for free, adhere to the gut
interface with a proximity-gated chance, accumulate there, and **burst into
the tissue when a boundary position's breach roll trips**. In tissue they
make a strongly biased random walk **toward the base**.

## Scope split — why this is Sprint 4 and not one giant sprint

The Director's 2026-08-21 design covers more than one sprint's worth of
work. It is split so something is playable at the end of each:

- **Sprint 4 (this one): the map and the invasion loop.** Geometry, bands,
  lumen flow, proximity adhesion, boundary accumulation, per-position
  breach, base-directed advance. All three pathogen classes move the same
  way for now.
- **Sprint 5 (next): the tissue state model.** Host cells as
  healthy/infected/dead with two-layer lattice occupancy (`GAME_DESIGN.md`
  §1c), which then enables the class-specific advance behaviors of §1b step
  4 — viral diffusion that dies without a host, intracellular bacteria
  entering and leaving cells — plus debris and the real life pool.

**Do not pull Sprint 5's work forward.** §1c's two-layer occupancy is a
`TissueGrid` rewrite (it currently holds exactly one pathogen per coarse
slot and has no host-cell concept whatsoever), and doing it at the same
time as the geometry change would make both unreviewable.

## Scope

**1. Board geometry: 100×40 with three bands.** `BoardConfig` currently
hardcodes `Rows = 5` as a const and clamps columns to 24–40. Both go. The
board becomes 100 coarse columns × 40 coarse rows, still 7×7 fine
subdivision, with a **band concept**: which columns are base, tissue, and
lumen. Bands must be data, not magic numbers scattered through the code — a
later map will use different proportions.

Scale is confirmed by the Director: **100×40 counts host cells, not
sub-lattice tiles.** So the tissue band is 50×40 = 2,000 host cells, and the
full field is 4,000 — roughly 10× Sprint 3's board. See `GAME_DESIGN.md`
§1a's scale note: at the existing 0.12s tick this puts a neutrophil at ~14s
to cross the tissue laterally and a macrophage at ~42s, which is the
intended spread. **Do not change `TickIntervalSeconds`, `FineSubdivision`,
or unit speeds to "fix" that** — it is the design.

**2. Camera and rendering at the new scale.** The field is 2.5:1; it fits
16:9 at roughly 28px per coarse cell. **This is the sprint's main
performance risk** — 4,000 coarse cells rendered, plus a cytokine field over
all of them, against a hard project requirement that everything is pooled
from first implementation (`GAME_DESIGN.md` §8, restated in `CLAUDE.md`).
Sprint 1–3's `BoardRenderer` was written for 150 cells. Measure the frame
cost and report it as a number; if per-cell `SpriteRenderer`s do not hold
up, say so with measurements rather than silently redesigning.

**3. "Direction of the base" as a map property.** The Director was explicit
that pathogen advance is specified as *toward the base*, **not** as
*leftward*, so later maps can put the base anywhere without touching
pathogen code. **No movement code may hardcode a leftward or negative-X
assumption.** Expose the base direction (or the base region) from the map /
board config and have movement consult it. This is an architectural
requirement, not a style preference — call it out in `INTERFACE.md`.

**4. Lumen flow.** Pathogens enter at the top of the lumen band and are
carried **downward**. A pathogen that reaches the bottom is excreted —
despawned, no penalty, per `handoff-map01-intestine.md` §1's
deliberately-kept "transit is not a fail state." While in the lumen a
pathogen cannot be attacked and does not interact with tissue.

**5. Proximity-gated adhesion.** Per `GAME_DESIGN.md` §1b step 1: a
pathogen's chance to adhere depends on **its distance from the gut
interface** — near the boundary it likely adheres, far out in the channel it
likely does not. On adhering it **moves to the boundary** and stays there,
colonising the interface. Curve shape is yours to choose; state what you
chose and why, and make it tunable.

**6. Per-position breach that releases everything at once.** Per §1b step 2:
**each boundary position has its own breach chance**, rolled per tick or
every X ticks. When it trips, **every pathogen adhered at that position is
released into the first tissue column at once.**

The burst is the point, not an implementation detail — pressure must
*visibly build* at a position and then break. Do not "simplify" this into
per-pathogen independent invasion rolls; that produces a trickle and
destroys the mechanic. Rolling less often with a correspondingly larger
chance is fine (and cheaper across 40 boundary positions) — rolling
per-pathogen is not.

Make an accumulating boundary position visibly distinct from an empty one,
so the player can see danger forming.

**7. Base-directed advance in tissue.** Per §1b step 3: a pathogen in tissue
performs a **strongly biased random walk toward the base**. All three
classes move this way this sprint — the class-specific behaviors (§1b step
4) need Sprint 5's host-cell states and are explicitly deferred. Bias
strength is tunable, not a const.

**8. Compartments move into the base band.** Bone marrow and the lymph node
placeholder currently render below and to the right of the board as
free-floating strips. They belong in the base band now. Immune cells enter
tissue at the **base-side edge of the tissue band**, not at the old
"blood-adjacent deepest fine row." Keep placement working exactly as it does
today — click an empty slot, pick a kind — this is a relocation, not a
redesign.

**9. Reaching the base is a real event, minimally.** A pathogen that reaches
the base band despawns and increments a visible counter in the HUD. **The
100-life pool and the actual lose condition are Sprint 5** — this sprint
only needs the endzone to exist and register, so the loop is observable end
to end.

**10. Explicitly not in scope.** Host cell states / two-layer occupancy
(§1c). Debris. Class-specific advance (§1b step 4). The life pool and lose
condition. Balance tuning of any kind — **the Director's standing
instruction is mechanics first, and immune cells currently winning easily is
known and accepted** (`BACKLOG.md`). Any upgrade system, ATP, economy.
Parasites. Adaptive immunity.

Everything Sprints 1–3 built must keep working: per-tower population caps,
kill-count depletion and degranulation, kill attribution, proximity contact,
cytokine sensing + heatmap, pooling, the `C` toggle.

## Stopping point (definition of done)

- [ ] The board is 100×40 host cells in three visually distinct bands, and
      the whole field is legible on screen at once.
- [ ] Pathogens flow down the lumen and are excreted at the bottom with no
      penalty.
- [ ] Adhesion probability visibly depends on distance from the interface —
      pathogens hugging the boundary adhere far more often than ones out in
      the channel.
- [ ] Adhered pathogens accumulate at boundary positions, and an
      accumulating position looks different from an empty one.
- [ ] A breach releases **every** pathogen at that position simultaneously,
      as a visible burst, not a trickle.
- [ ] Pathogens in tissue advance toward the base by a biased random walk,
      and **nothing in the movement code hardcodes "left"** — moving the
      base in config moves the advance direction with it.
- [ ] Bone marrow and lymph node sit in the base band; placement still
      works; emitted cells enter at the tissue's base-side edge.
- [ ] A pathogen reaching the base despawns and increments a visible
      counter.
- [ ] Frame cost at 4,000 cells is measured and reported as a number.
- [ ] Everything from Sprints 1–3 still works.
- [ ] `docs/ENGINE_STATUS.md` and `docs/INTERFACE.md` reflect reality, and
      `docs/TEAM_RETRO.md` has a new note. **Write these as you go, not at
      the end** — Sprint 3's agent hit its usage limit before writing any
      docs and the head session had to reconstruct all four.

The question this sprint answers for the Director: **does the invasion loop
read?** Can he watch pressure build at a spot on the gut wall, see it burst,
and then see the immune response converge on the breach — and does that feel
like defending a front rather than watching pathogens teleport?
