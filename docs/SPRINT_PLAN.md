# Sprint Plan — Sprint 21: organs, shop, board frame, knowledge HUD

## Recent sprints

- **Sprint 19** — peristalsis, velvet villi, Peyer's patch, crypts.
- **Sprint 20** — one palette, two looks (Classic / Daylight) and F10.
- **Sprint 21 opener** — the Director chose Daylight ("That looks great.
  Excellent call on no faces." 2026-10-14) and **Classic was retired**.

## Direction (Director, 2026-10-14)

> "The next cosmetic items should be the bone marrow and the lymph nodes,
> the shop, the borders of the game board, and the pathogen knowledge HUD
> which doesn't exist yet."

A dispatched Design agent wrote `docs/COSMETIC_PASS_SPEC.md`, with mocks
in `docs/art/cosmetic_*.png`. The Director approved all five of its
decisions:
1. light cream UI cards instead of dark glass;
2. all permanent UI in the gutters above and below the board, with the
   shop as an always-open tray;
3. reached-but-unbuilt knowledge rungs and placeholder shop items shown
   honestly with a "planned" hatch;
4. bundle Nunito (SIL OFL);
5. the 90% capstone as a star on every ladder.

## Scope (the spec's order)

1. **Tokens and sprites**: `UiTheme` rewritten for light cards; Nunito
   Regular / Bold / Black; `SpriteShapes.Ui.cs` (icons, hatch, build pad,
   select ring, plus glyph, 9-sliced plate frame).
2. **Frame**: `FitCamera` reserves 15% strips; the plate frame; band
   tabs; the dock reshuffle.
3. **Restyle**: HUD, shop tray, tower picker, upgrade panel.
4. **Knowledge HUD**: `KnowledgeView` + the pure `KnowledgeWatcher`;
   `KnowledgeLadder.Rung.Built` and `CapstonePercent`.
5. **Organs**: `OrganShapes` (spongy-bone marrow with cavities, a
   structured lymph node with lymphatics), steel pads with glyphs, drop
   shadows, tags.
6. **Verify + docs**.

## Deviations from the spec (all small, all noted in code)

- **Plate frame**: window inset 16 px with a 32 px corner, not 48 / 40.
  The spec's geometry put the corner arc across the 9-slice border and
  collapsed the window.
- **Marrow**: cavities are capped at the pad pitch, and the block keeps
  0.30 pitch of end margin, not 0.15. As specified, the cavities merged
  into one channel that broke through the bone.
- **Medullary cords**: four thin cords, not three fat ones; the three
  read as a big "<" arrow.
- **Card lift**: a thicker bottom border instead of a shadow sibling
  (same read, no extra elements).

## Not in scope

- The two bacterial classes look the same on the board (the knowledge
  cards made this obvious). `BACKLOG.md`.
- Leukocyte rolling / extravasation; Peyer's patch refinements; a DC
  antigen dot; idle wobble.

## Stopping point

- [x] All four items built per spec (with the deviations above).
- [x] 14 harnesses green (`Sprint21Verification`, 22 new checks), no warnings.
- [x] Organs and icons checked magnified (`SpriteShapeDump` now includes organs).
- [x] WebGL build (9,204,833 bytes, 0 errors) served for the Director.
- [ ] **The Director's eye**: the whole screen, especially the shop tray,
      the knowledge strip mid-round (tokens, ribbons), and the organs at
      real size. Nothing UI-side is headlessly visible.
