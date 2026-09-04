# Sprint Plan — Sprint 17: the cartoon pass + the DC jitter fix

## Recent sprints

- **Sprint 14** — the DC-pacing rework: the shuttle collapsed to two states
  so a dendritic cell paces the tissue band base↔lumen its whole life.
- **Sprint 15** — the compartment visual pass: the lumen drawn as an open
  channel, the base as a bloodstream, marrow birth-puffs, a breach flash,
  a live lymph-node co-localisation haze. Base + lumen left the per-cell
  `SpriteRenderer` grid (−110 renderers at 25×10).
- **Sprint 16** — the UI pass: every IMGUI surface replaced with UI Toolkit
  built from code. Minimal HUD, debug readout behind backtick, floating
  buy panels at the marrow slot, live buying during a round, 12 named
  placeholder upgrade rows. One `PanelSettings` asset shipped after the
  built player threw on every text label.
- **410 assertions, 0 failed** across the ten harnesses, plus
  `BootstrapSmoke`.

## Direction (Director, 2026-09-04, after playtesting Sprint 16)

> "We still need to do something about the super jittery motion of DCs,
> but the buy panels and upgrades are a great framework! Our next step
> would be to cartoonify the blood vessel a bit so it's visually
> appealing. Same thing for the lumen. It can look more velvety and
> feature villi instead of just being poo-inspired. That would also help
> contrast with the contaminated food sources flowing by (those can be
> poo-ey)."

Two items, and the second one carries an argument worth keeping in front
of us: the lumen re-paint is **legibility**, not decoration. The
contaminated bolus that delivers every round is ochre and lumpy on
purpose; Sprint 15 painted the channel it travels down in the same
browns. The most important arriving object in the game was camouflaged
against its own background.

## Scope

1. **The DC jitter fix.** `DendriticCell.Update` free-ran its tween timer
   from a random spawn phase while the tick fired on `AdaptiveDirector`'s
   shared accumulator, so a tick landing late in the tween cycle snapped
   the cell most of the way to its new tile in one frame and left it
   crawling — every tick. Reset the timer with the tick.
   `Lymphocyte` has the same bug and gets the same fix.
2. **Lumen cartoon pass** — villi along the gut wall (new
   `SpriteShapes.Villus`, swaying with the peristalsis), a mucosal-plum
   channel, a pearly mucus sheen instead of a grey-green film, lighter and
   fewer flow motes. The bolus keeps its ochre and becomes the only brown
   thing in the band.
3. **Vessel cartoon pass** — the endothelial wall becomes a *tiled row of
   cells* (new `SpriteShapes.EndothelialCell`) instead of one stretched
   quad, brighter wall and corpuscle tints, and a deeper biconcave dip on
   the erythrocyte so it reads as a dish rather than a dot.
4. **Docs** — `COMPARTMENT_DESIGN.md` amended with a Sprint 17 revision
   section (the old §2.1/§2.2 stay as the record of what was there and
   why), `UI_STYLE_GUIDE.md` palette + sorting table, `INTERFACE.md`,
   `ENGINE_STATUS.md`, `CHANGELOG.md`, `TEAM_RETRO.md`, `BACKLOG.md`.

## Not in scope

- **The DC's zigzag walk.** Fixing the tween is one thing; the patrol step
  itself is genuinely random (three independent weighted von Neumann steps
  per tick), so the cell will still wander laterally. That is a *design*
  question — directional persistence, fewer longer steps, or smoothing
  only the rendered position — and it should be answered after the
  Director has seen the motion without the snapping.
- Wiring any upgrade row to the simulation (still `GAME_DESIGN.md` §6d
  placeholders; the widest gap in the game right now, and the obvious
  Sprint 18).
- The tissue band's own look, the food bolus (deliberately unchanged), and
  any balance or mechanics change.

## Stopping point (definition of done)

- [ ] DCs move smoothly — no per-tick snap.
- [ ] The lumen reads as gut: villi along the wall, velvety, not brown.
- [ ] The contaminated bolus visibly contrasts with the channel it enters.
- [ ] The vessel wall reads as cells; erythrocytes read as biconcave discs.
- [ ] Ten harnesses green, `BootstrapSmoke` green.
- [ ] Clean Windows build; headless launch 0 exceptions **(launch it — see
      Sprint 16: batchmode being clean proved nothing about the player)**.
- [ ] Docs updated.
- [ ] **The Director's eye.** Nothing here is headlessly testable.

## Process note

Built directly, without a dispatched design agent: the direction was
concrete enough (villi, velvety, contrast with the bolus) that a spec
round would have been ceremony. `COMPARTMENT_DESIGN.md` is amended rather
than rewritten, so the Sprint 15 reasoning stays legible next to what
replaced it.

**Also running this sprint: the first agentic playtest.** An agent was
dispatched to drive the Sprint 16 build and write up its findings
(`docs/AGENT_PLAYTEST_01.md`) — a dry run of whether an agent can test
this game at all, not a replacement for the Director's playtest.
