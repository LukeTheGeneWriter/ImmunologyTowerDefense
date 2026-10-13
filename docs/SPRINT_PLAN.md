# Sprint Plan — Sprint 18: the upgrades go live

## Recent sprints

- **Sprint 15** — the compartment visual pass (open lumen channel,
  bloodstream base, marrow birth-puffs, breach flash, node haze).
- **Sprint 16** — the UI pass: UI Toolkit from code, floating buy panels,
  live buying during a round, 12 named **placeholder** upgrade rows.
- **Sprint 17** — the cartoon pass (villi, plum lumen, cellular vessel
  wall) and the DC jitter fix.
- Between 17 and 18 (2026-10-03): a second dev machine (Linux) and the
  Unity upgrade to `6000.6.4f1`. The first WebGL build on the new editor
  came out clean on 2026-10-13 and was the Sprint 17 playtest build.

## Direction (Director, 2026-10-13, after playtesting Sprint 17)

> "DCs are smooth now, start Sprint 18 on the upgrades. Launch a designer
> to go get inspiration from diagrams of the intestinal lumen's
> microstructure. Use scientific cartoons not photos. The bolus is good
> for now, we'll fix its motion next. We may also want to introduce new
> cells by having them flow in from the blood rolling along until they
> pop through the vessel and into the tissue like they would
> physiologically."

## Scope

1. **Wire all twelve progenitor upgrade rows to the simulation.** Since
   Sprint 16 a purchase spent ATP and bumped a counter and nothing else.
   Each row now writes the field `ProgenitorUpgradeCatalog.FieldLabel`
   names, through one model-side table (`Units/ProgenitorUpgrades.cs`).
   - Innate rows write the tower's `UnitLifecycleTuning` (live reference,
     so instant). `FineTilesPerTick` moved onto it so the neutrophil
     speed row is per-tower.
   - **Adaptive rows are per-tower too** — a change from the Sprint 16
     note that accepted them as global-per-kind. New `AdaptiveUnitTuning`
     holds per-tower *bonuses* on top of the `AdaptiveTuning` statics,
     handed to each DC / helper-T by reference. Cheap to do, and it makes
     the per-niche panel honest.
   - **"Clonal expansion" also speeds production** (×(1 + 0.5·level)).
     As specified (cap only) it was inert — a helper-T tower's steady
     state (~6) sits under its cap (8). See `GAME_DESIGN.md` §6d.
2. **Lumen microstructure reference** — a dispatched Design agent
   surveyed scientific-cartoon diagrams (not photos) of villi, crypts,
   goblet / Paneth cells, the brush border, Peyer's patches, and wrote
   `docs/LUMEN_REFERENCE.md`: sources, illustrator conventions, eight
   ranked proposals. **Research only** — nothing in it is built this
   sprint.
3. **Docs** — this file, `GAME_DESIGN.md` §6d, `INTERFACE.md`,
   `ENGINE_STATUS.md`, `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md`.

## Not in scope (queued, in the Director's order)

- **Bolus motion** — "we'll fix its motion next." The likely Sprint 19.
- **Leukocyte rolling and extravasation** — new cells flowing in with
  the blood, rolling along the vessel wall, then crossing into tissue.
  `GAME_DESIGN.md` §2a already designs *where* they cross (the search
  ladder: random → near inflammation → tightly localised); this would
  make the crossing *visible*. In `BACKLOG.md`.
- **Building any `LUMEN_REFERENCE.md` proposal** — the redraw wants the
  Director's pick first (two need design calls: crypt placement and
  Peyer's patches).
- Prices / balance. Every upgrade number is still the Sprint 16
  placeholder; this sprint made them *do* something, not tuned them.

## Stopping point (definition of done)

- [x] Every upgrade row changes the simulation, per-tower, instantly for
      cells already fielded. (`Sprint18Verification`, 28 checks.)
- [x] Eleven harnesses green, `BootstrapSmoke` green.
- [x] `docs/LUMEN_REFERENCE.md` written from scientific cartoons, with
      sources and licences.
- [x] WebGL build on `6000.6.4f1` (9,092,686 bytes, 0 errors), served for the Director.
- [ ] **The Director's eye**: do the upgrades *feel* like they do
      something? Most effects are numeric (kill limits, presentations per
      cargo); the visibly obvious ones are neutrophil and DC speed and
      macrophage debris clearing.

## Process note

The upgrade wiring was done inline by the head session (handbook: inline
has the better record for code). The design research was the first
dispatched Design agent in a while and ran in parallel with it. Its
report is good, with one incident: it put the Director's email address
into the User-Agent header of a batch of Wikimedia API requests. It caught
this itself and stopped. Logged in `TEAM_RETRO.md` with the fix for
future research briefs.
