# Sprint Plan — Sprint 20: one palette, two looks

## Recent sprints

- **Sprint 18** — all 12 progenitor upgrade rows wired, per-tower.
- **Sprint 19** — peristalsis, an opaque bolus, velvet-pile villi,
  Peyer's patch + M-cell sampling, real crypts. Director: "Nice job!"
- **Art direction research** (between 19 and 20) — `docs/ART_DIRECTION.md`
  proposes retiring the dark "histology plate" look for a light,
  outlined "daylight cartoon". Its headline finding was legibility, not
  taste: against the plum lumen the bacterium was 1.13:1 and the virion
  1.04:1, effectively invisible.

## Direction (Director, 2026-10-13)

Asked to have the designer compare the look against broad-appeal TD
games and cross-section diagrams for "something accurate but simple fun
and bright with mass appeal". On the resulting decisions:

- **D1 — toggle first, then decide.** Build both looks and an in-game
  switch; choose after seeing them side by side in a real build.
- **D2 — bacteria green** (knowledge flash moves to cyan).
- **D3 — pale aqua lumen.**
- **D5 — pathogen size floor** (visual only).
- D4 (no faces) and D6 (rose blood) were not asked; the designer's
  recommendations are taken, both reversible in `Palette.cs`.

## Scope (ART_DIRECTION.md §4 items #0–#7)

1. **`Rendering/Palette.cs`**: every world colour in one place, Classic
   (= shipped) and Daylight values. All consumers rerouted.
2. **F10**: flip the look, persist it, rebuild sprites, reload the scene.
3. **Outlines**: `SpriteShapes.OuterStroke` + `Finish` on units,
   pathogens, the bolus, structures. Daylight only.
4. **Pathogen silhouettes + size floor**: a 2:1 bean bacterium, a corona
   virion, drawn at ~0.55–0.6 of a cell. Daylight only.
5. **Daylight grounds**: flat (unstippled) chyme, plasma and marrow; a
   tissue underlay; an opaque empty-ground scar.

## Not in scope

- #8 the DC antigen dot (a child renderer showing what a DC carries).
- #9 idle wobble / jiggle, lane-tone rows. #10 a UI re-check on the light
  board. All in `BACKLOG.md`.
- **Leukocyte rolling / extravasation**: still queued.

## Stopping point

- [x] Classic is the shipped look, exactly (asserted).
- [x] Daylight: pathogen outlines are 13.0:1 (bacterium) and 14.7:1
      (virion) against the lumen, up from 1.13 and 1.04.
- [x] FX colours pairwise distinct in both looks (`ShapeFor` safety).
- [x] Outlined sprites keep their outer extent; checked magnified.
- [x] 13 harnesses green (`Sprint20Verification`, 24 new checks).
- [x] WebGL build (0 errors) served for the Director.
- [ ] **The Director's eye**: play both looks (F10), then pick one, or a
      mix. Whichever loses gets deleted from `Palette.cs` in a later sprint.
