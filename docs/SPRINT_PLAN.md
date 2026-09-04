# Sprint Plan — Sprint 16: the UI pass

## Recent sprints — closed 2026-08-29 / 30

- **Sprint 13** — the entity sprite / visual-identity pass:
  procedurally-drawn `SpriteShapes` replace the flat white quad for every
  entity; four host-state sprites; five distinct flash silhouettes.
- **Sprint 14** — the DC-pacing rework: the shuttle collapsed from four
  states to two so a dendritic cell paces the tissue band base↔lumen its
  entire tissue life.
- **Sprint 15** — the compartment visual pass: the lumen drawn as an open
  channel (chyme + mucus + pooled flow motes + peristalsis), the base as a
  bloodstream (plasma + vessel wall + erythrocytes + organ halos), marrow
  birth-puffs, an acute breach flash, a live lymph-node co-localisation
  haze. Base + lumen left the per-cell `SpriteRenderer` grid (−110
  renderers at 25×10). **Handed off; not yet playtested.**
- **410 assertions total, 0 failed** across all ten harnesses.

## Direction for Sprint 16 (Director, 2026-08-30 / 09-04)

The original ask, verbatim:

> "Remove the status overlay on the top left corner — it has so many stats
> I can't get a feel for how the player will interact with the game."

A design agent was dispatched for the spec (same recipe as Sprints 13 and
15) and delivered **`docs/UI_DESIGN.md`** — direction, palette, type
scale, the minimal HUD, the debug toggle, the progenitor upgrade panel,
a per-kind upgrade roster, and a 9-commit migration plan. It left nine
open questions. The Director answered the four that change what gets
built (2026-09-04); the head took the spec's recommendation on the other
five.

### Director decisions

1. **Everything migrates off IMGUI this sprint** — HUD, debug readout,
   upgrade panel, tower picker *and* the buy-phase shop, in one pass. The
   buy row component is shared, so the shop comes nearly free once the
   upgrade panel exists; leaving it behind guarantees a second migration
   and a buy phase that looks like two different games. **Nothing IMGUI
   survives this sprint.**

2. **The upgrade panel floats by the slot**, not a fixed right dock —
   anchored to the clicked marrow slot via
   `RuntimePanelUtils.CameraTransformWorldToPanel`. Tighter spatial
   coupling: the panel is *at* the thing you clicked. Consequence, per
   `UI_DESIGN.md` §10 Q3: **the shop vacates the left edge** and docks
   right, under the HUD (head's call — see "Shop placement" below).

3. **3 upgrade rows per kind, no inflammasome row.** `GAME_DESIGN.md` §4b
   makes the innate stress-sense roll deliberately bad so "my macrophages
   can't touch this" is what pushes the player toward adaptive sensors;
   selling that away cheaply undercuts the bridge the game is built on.
   Left out of the first pass entirely, recorded in `BACKLOG.md`.

4. **Buying is live during a round — no pause, no freeze advantage.**
   Director, verbatim:

   > "You can buy during the round, but there will be no time freezing
   > advantage. Much like Bloons, if you're close to having enough cash
   > for an upgrade then it forces the user to pay attention and move fast
   > when they can finally afford it to survive a wave they may have
   > otherwise been overwhelmed by."

   This is a **design change, not just a UI one** — see below.

### Spec recommendations taken as-is (§10 Q1, Q2, Q7, Q8, Q9)

- Debug readout toggles on **backtick** (`KeyCode.BackQuote`), default OFF.
- **Lives as a plain integer** — teal, turning oxblood below 25% of max,
  with a flash on each decrement. No pips (100 pips is unreadable, 10
  pips-of-10 lies about the granularity).
- **No `ThemeStyleSheet` asset.** Accept the one boot warning; every
  element is explicitly styled and Unity's default control chrome is not
  wanted. Fallback if unstyled text renders wrong: one tiny
  `DefaultRuntimeTheme.tss` — **pre-authorised**, but only if it bites.
- **Adaptive upgrades stay global-per-kind** (`AdaptiveTuning` statics).
  Placeholder rows either way; the rows and prices don't change. Recorded
  for the wiring sprint.
- **Compartment labels** trim to "Bone marrow" / "Lymph node", restyled to
  the new type scale, ported to UITK with the rest.

## What live buying actually costs (head's code audit, 2026-09-04)

Cheaper than the spec assumed:

- **The marrow picker and upgrade panel are already un-gated** —
  `BoneMarrowManager.OnGUI` draws both regardless of phase. Only
  `HudOverlay.DrawShopPanel` gates on
  `rounds.Phase != RoundPhase.Building`.
- **ATP already accrues live during a round**: `EconomyTuning.AtpPerKill`
  (3) is paid through `EconomyHooks.PayForKill` on every kill, as it
  happens. The exact dynamic the Director described — watch the number
  climb mid-wave, buy the instant it clears the price — **works today with
  no economy change**.
- **`RoundClock.Frozen` is untouched.** The between-rounds buy phase still
  freezes the field; live buying only means the shop is *also* open while
  a round is Active. Placing a tower mid-round works already
  (`EmissionTimer = 0`, emission resumes next unfrozen `Update`).

So the change is: **drop the shop's phase gate, rewrite the HUD's "time is
frozen" copy, and make sure the buy surfaces are reachable in one click
while a round runs.** No `RoundClock` change, no `RoundController` change.

### Shop placement (head's call, following from decision 2)

The floating upgrade panel can land anywhere on the board, so the shop
can't hold the left edge. It **docks right, under the HUD**. Because live
buying puts it on screen during rounds too, it is **collapsible**:
expanded by default in the buy phase, collapsed to a header strip while a
round is Active, one click to open. Keeps `UI_DESIGN.md` §1's "quieter
than the tissue" rule during play without putting the buy one screen away
when the Director needs it fast.

## Scope

1. **UITK bootstrap.** `PanelSettings` + `UIDocument` created at runtime in
   `GameBootstrap`, tree built from code, no `.uxml` / `.uss` / UI Builder.
   `UiTheme` static holds the palette, type scale, spacing and the shared
   `Panel()` / `Divider()` / `Row()` components. Root and dock containers
   `PickingMode.Ignore` so world clicks still reach `BoneMarrowSlot`.
2. **`HudView`** — ATP, round, lives, the Start Round control, GAME OVER.
   Top-right. Deletes `HudOverlay.DrawRoundBar`.
3. **`DebugReadoutView`** — everything else from the top-left dump,
   monospace, bottom-left, backtick to toggle, **default OFF**. Deletes the
   top-left panel. This is the Director's original complaint, closed.
4. **`BoneMarrowManager` selection API** — `SelectedSlotIndex` +
   `SelectSlot` / `ClearSelection` replacing the two `pending*Index`
   fields, so the UI layer owns presentation and the manager owns state.
5. **Selected-slot rim highlight** on the board (`COMPARTMENT_DESIGN.md`
   §2.4's hook).
6. **`UpgradePanelView` + `TowerPickerView` + the upgrade catalog** —
   floating, anchored to the slot. 3 rows per kind from `UI_DESIGN.md` §5,
   inflammasome omitted. Deletes both `BoneMarrowManager.OnGUI` panels.
7. **`ShopView`** — right dock under the HUD, collapsible, **no phase
   gate**. Deletes `HudOverlay.DrawShopPanel`.
8. **`CompartmentLabel`s** ported and trimmed.
9. **Docs** — `UI_STYLE_GUIDE.md` rewritten, `INTERFACE.md`,
   `ENGINE_STATUS.md`, `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md`,
   `GAME_DESIGN.md` (the live-buy rule).

## Not in scope

- **Wiring the upgrades to the simulation.** `GAME_DESIGN.md` §6d's rule
  holds: buying spends ATP, bumps a level, changes nothing in the sim. The
  rows name the exact field they will eventually write
  (`UI_DESIGN.md` §5) so the wiring sprint is a one-field change per row.
  The one existing exception stays exceptional: `CytokineSensingUpgrade`
  really does push `Chemotaxis.SensingUpgradeLevel`.
- Making adaptive upgrades per-tower (`AdaptiveDirector` change).
- Moving `FineTilesPerTick` onto `UnitLifecycleTuning` (needed by the
  neutrophil "Rapid chemokinesis" row when it is wired, not now).
- Any balance / tuning / mechanics change beyond the live-buy gate.

## Stopping point (definition of done)

- [x] The top-left stat dump is **gone** by default; backtick brings it
      back as a monospace instrument panel.
- [x] A minimal HUD reads ATP / round / lives and offers Start Round.
- [x] Clicking an empty marrow slot floats a picker at it; clicking a
      placed one floats its upgrade panel with 3 real-named rows, cost,
      level dots and effect text.
- [x] The selected slot is rimmed on the board.
- [x] The shop is a right-docked UITK panel, buyable **during a round** --
      collapsible, since it is now on screen during play.
- [x] No `OnGUI` left in the project (CompartmentLabel ported too).
- [x] All ten harnesses green, unedited (410 assertions), plus the new
      `BootstrapSmoke.RunAll` — the batchmode-bootstrap-with-0-exceptions
      signal this line asked for. UI still has no other headless coverage.
- [x] Clean Windows build (94,050,392 bytes, 0 errors); headless launch
      0 exceptions -- on the **second** attempt: the first build threw per
      label per frame in the player because a runtime-created
      PanelSettings has no text settings. Fixed by shipping one
      `PanelSettings` asset. See TEAM_RETRO.md.
- [x] Docs updated (scope item 9).
- [ ] **How it feels to use.** No headless coverage of `Update()`; the
      handoff is the build. The Director's playtest — including the
      Sprint 15 compartment visuals, still unplayed.

## Process note

Spec-then-implement, same as Sprints 13 and 15: a dispatched design agent
produced `UI_DESIGN.md` + an uncompiled prototype sketch
(`UiPrototype.PROTOTYPE.cs`, behind an undefined `#if`, wired nowhere);
the head integrates it into real files inline. Commit per coherent chunk
with a reasoning-heavy message; the Director's playtest is the QA.
