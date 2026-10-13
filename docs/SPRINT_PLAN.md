# Sprint Plan — Sprint 19: peristalsis and gut-wall anatomy

## Recent sprints

- **Sprint 16** — the UI pass (UI Toolkit, floating buy panels, live buying).
- **Sprint 17** — the cartoon pass (villi, plum lumen, cellular vessel
  wall) and the DC jitter fix. Playtested 2026-10-13: "DCs are smooth now."
- **Sprint 18** — all 12 progenitor upgrade rows wired, per-tower;
  `docs/LUMEN_REFERENCE.md` (design research from scientific cartoons).

## Direction (Director, 2026-10-13)

> "We'll want the bolus to move through the gut via gut peristalsis, and
> we'll also want it to be opaque and not so spikey. Great info from the
> designer. Implement all 3. Also yes on payers patch. That can be done
> now and refined later since it's important immunologically."

"All 3" means the top three lumen proposals: the villus redraw (P1 + P3),
the back row (P2), and the crypt sprite (P4). Answers to the forks asked
up front:

- **Peristalsis:** surge-and-pause. The bolus is pushed in waves, and
  total transit is unchanged.
- **Peyer's patch:** a landmark from the start; **M-cell sampling is a
  shop upgrade**, "not straight out of the box".
- **Crypts:** snapped to the wall, with the regrowth bonus wired for real
  so the sprite never shows a bonus that doesn't exist.

## Scope

1. **Peristalsis** (`Pathogens/Peristalsis`): a closed-form surge profile
   with mean 1, which moves the bolus (spawner) and the lumen squeeze
   (renderer) on one clock. A contraction ring behind the bolus.
2. **Bolus sprite**: opaque (no alpha stipple) and rounded (no deep lobes).
3. **Lumen**: villus redraw (rim / core / lacteal / goblets / brush
   border, two variants), and a darker back row.
4. **Peyer's patch**: dome, M cells, follicle; `ShopItem.MCellSampling`
   samples lumen pathogens over the dome into antigen a follicle DC collects.
5. **Crypts**: `ShopItem.Crypt` is real. Each level adds a wall crypt
   (stable spread, off the patch); Empty ground in reach regrows 3× faster.
6. **Docs**: this file, `GAME_DESIGN.md` (§1a peristalsis, §1d crypts +
   Peyer's patch), `UI_STYLE_GUIDE.md`, `INTERFACE.md`, `ENGINE_STATUS.md`,
   `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md`.

## Not in scope

- **Leukocyte rolling / extravasation**: queued, likely Sprint 20.
- Peyer's patch refinements (DCs seeking the follicle, M cells as a
  pathogen entry route, follicle IgA). See `GAME_DESIGN.md` §1d.
- LUMEN_REFERENCE P5 / P7 / P8. Any balance or price pass.

## Stopping point (definition of done)

- [x] The bolus surges and pauses; transit is still 30 s (measured 30.05 s).
- [x] The bolus is opaque and rounded (checked magnified, SpriteShapeDump).
- [x] Villi have rim, core, lacteal, goblets and brush border, with a back row.
- [x] Peyer's patch drawn; M-cell sampling bought → a follicle DC gets cargo.
- [x] Crypts appear when bought and speed regrowth (6.8 s → 2.3 s measured).
- [x] 12 harnesses green (`Sprint19Verification`, 28 new checks), `BootstrapSmoke` green.
- [x] WebGL build (0 errors) served for the Director.
- [ ] **The Director's eye**: does the peristalsis read as peristalsis? Is
      the dome legible as a place? Do villi read as velvet at board scale?
