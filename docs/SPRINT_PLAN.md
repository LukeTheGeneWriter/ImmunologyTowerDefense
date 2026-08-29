# Sprint Plan — Sprint 13: the sprite / visual identity pass

## Recent sprints — closed 2026-08-29

- **Sprint 11** — placeholder buy-phase shop (`ShopLedger` / `ShopTuning`,
  per-tower `UpgradeTower`), the §5 knowledge ladder as data + a
  per-species HUD readout, and one real change: neighbour-accelerated
  regrowth (`TissueTuning.NeighbourRegrowthBonus`). `Sprint11Verification`
  26.
- **Sprint 12** — two Sprint 11 playtest fixes: cytokine sensing is ON by
  default with a real buyable sharpen (`ShopItem.CytokineSensingUpgrade`
  → `Chemotaxis.SensingUpgradeLevel` → `EffectiveSharpness`; `C` = debug
  off), and the DC patrol movement fix (fine-grained lane-repulsion +
  a threat-axis band sweep; `BoardConfig.FineCrossIndex` / `FineAxisIndex`;
  `DcPatrolSweepBias`). `Sprint12Verification` 9. **410 assertions total,
  0 failed.**

## Direction for Sprint 13 (Director, 2026-08-29)

"It's starting to look good — time to dispatch a design agent and plan
the next sprint." A **design agent was dispatched and delivered
`docs/SPRITE_DESIGN.md`** — a full visual-identity spec (direction,
per-entity sprite spec, the five flashes, a procedural-`Texture2D`
implementation recommendation with a shape library, an 11-call-site
migration plan) plus an uncompiled prototype
`game/Assets/Scripts/Rendering/SpriteShapes.cs`.

Sprint 13 **implements that spec**: replace the single flat white quad
(`RuntimeSprites.SquareSprite`) with real procedurally-drawn shape sprites
for every on-screen entity, so the board reads as a stained tissue
section with crisp icon-like agents on top.

## Open questions — need a Director decision before/at the start (see `SPRITE_DESIGN.md` §6)

1. **Palette direction.** Keep the playtested placeholder colours
   as-is, or let this pass re-hue the ones near `handoff` §8's "avoid"
   list — cytokine-heat orange (`1.00,0.55,0.05`), the knowledge /
   efferocytosis greens, neutrophil amber (`0.95,0.78,0.25`)?
2. **Free virion colour** (§6 Q8) — split it to a colder purple-maroon so
   "virus" and "bacterium" read apart at a glance, or keep both on
   `PathogenColor` and let dot-vs-rod carry it?
3. **Per-instance variation** (§6 Q3) — identical cells (clean clinical
   plate) or subtle rotation / size / hue jitter (organic)?
4. **Flash shapes** (§6 Q6) — five distinct silhouettes (stipple / spiky
   star / soft bloom / shockwave ring / thin ring — colour-blind-safe),
   or keep all five as the expanding square + colour?
5. **Compartment + infection detail** (§6 Q4/Q5) — do the lymph node /
   bone marrow get real organ silhouettes (bean, trabecular region), and
   does viral-vs-bacterial infection get a texture split (swollen
   inclusion vs. purulent stipple) as well as the hue? Or keep rectangles
   + hue-only infection this pass?
6. **Concurrent-flash cap** (§6 Q7, `GAME_DESIGN.md` §8) — add the hard
   ceiling on simultaneous `DegranulationFlash` instances now, or defer?

The agent's own recommendations: keep the load-bearing colours but nudge
neutrophil toward gold and split the virion hue; subtle variation;
distinct flash shapes; organ silhouettes + a texture split are worth it;
add the flash cap.

## Scope

### 1. `SpriteShapes.cs` — review, compile, land

Review the dispatched prototype (`game/Assets/Scripts/Rendering/SpriteShapes.cs`,
uncompiled), fix whatever Unity's compiler flags, confirm it follows the
project's lazy-static-cache style (like `RuntimeSprites`) and references
no gameplay type. Land it + its `.meta`, wired nowhere, with
`RuntimeSprites.SquareSprite` kept as the fallback. Shapes per
`SPRITE_DESIGN.md` §4 (disc, ring, capsule, lobed blob, star, rounded,
stipple, inner/rim shade), 64×64, white with the shape in alpha so every
per-instance tint keeps working.

### 2. Swap the call sites, one entity at a time (`SPRITE_DESIGN.md` §5.1)

Each an isolated `sr.sprite = SpriteShapes.X` with **no change** to
`sortingOrder`, `localScale`, or `color`. Order (each its own commit):

- **a–b.** `UnitProfile` gains a `Sprite Shape` field (mirrors `Color`);
  `GameBootstrap` sets it per profile; `SearchUnit` uses `profile.Shape`
  (macrophage = amoeboid blob, neutrophil = lobed-nucleus disc).
- **c.** `PathogenAgent` picks by `Class` — large bacterium = maroon rod
  (random rotation), free virion = small dot. `sr.enabled = !IsIntracellular`
  untouched (the intracellular "no sprite" rule).
- **d.** `PathogenSpawner` food item = lumpy ochre bolus.
- **e.** `DendriticCell` = dendritic star (optional loaded-core variant on
  `HasCargo`).
- **f.** `Lymphocyte` = nucleus-heavy teal circle.
- **g.** `BoneMarrowManager` slot = rounded niche (optional: show the
  placed unit's shape).
- **h.** `GutInterfaceRenderer` bar = epithelial row-of-cells texture;
  the thicken+heat `Refresh()` maths untouched.
- **i.** `BoardRenderer` + `GameBootstrap.BuildBoardVisual` — the 4,000
  coarse-cell grid picks `HostCell` / `HostCellInfected` / `Debris` /
  `EmptyPit` per state alongside the colour it already computes. Keep the
  host-cell sprite **opaque** (rim drawn inside) so there's no added
  overdraw vs. today's quads. Cytokine heat tint applied after, unchanged.
- **j.** Marrow / lymph backdrops → region / (bean) silhouettes (per Q5).
- **k.** `DegranulationFlash.Begin` picks a shape from `burstColor` via an
  internal `switch` over the five `static readonly` colours — **every
  `Play(...)` call site unchanged**. Per-shape timing/scale if adopted
  (Q4): make the current `const` duration/scale instance fields.

### 3. Concurrent-flash cap (if Q6 = now)

`DegranulationFlash` (or its pool owner) drops new `Play` requests past a
tunable `MaxConcurrent` — `GAME_DESIGN.md` §8's "explicit, tunable cap on
simultaneous cosmetic effects that degrades gracefully". A new
`RenderTuning` (or a field on the class).

### 4. Visual QA

No headless harness covers rendering (`Update()` doesn't run in
batchmode). So:

- A scratch **debug key** that spawns one of every entity in every state
  and fires all five flashes at once, for a single-frame `PrintWindow`
  capture (per `AGENT_HANDBOOK.md`'s screenshot notes).
- Checklist: sorting still back-to-front; DC cargo / lymphocyte paired /
  pathogen contact-flash / infected-cell + cytokine-heat all still
  visibly change; footprints unchanged (macrophage > neutrophil; food
  1.4×); intracellular infection shows **only** as the host-cell
  background; the four host states are four distinct reads.

### 5. Docs

- `docs/UI_STYLE_GUIDE.md` **rewritten** to describe the shipped sprites
  (it currently documents the placeholder quads) — the two docs merge:
  `SPRITE_DESIGN.md` is the spec, `UI_STYLE_GUIDE.md` becomes "what's on
  screen now".
- `ENGINE_STATUS.md`, `INTERFACE.md` (the `UnitProfile.Shape` field, the
  `SpriteShapes` surface, any `PathogenAgent` colour branch), `CHANGELOG.md`,
  `BACKLOG.md`, `TEAM_RETRO.md`. Clean Windows build, 0 exceptions,
  **a screenshot in the handoff**.

## Not in scope

- **Authored PNG art / an asset pipeline / `com.unity.ugui`** — the spec
  explicitly rejects these for now (no Editor in the loop). Everything
  stays procedural.
- **A real buy UI** (uGUI / UI Toolkit) — the shop / picker / HUD stay
  IMGUI. Flagged as the natural companion, but its own sprint.
- **Animation** beyond the existing position tween and the flash
  expand/fade. No sprite-sheet animation.
- **New gameplay, tuning, or mechanics.** Sprint 13 is visual-only —
  `sr.sprite` swaps and the flash cap, nothing else.
- **Camera / zoom / lighting changes.**

## Stopping point (definition of done)

- [ ] `SpriteShapes.cs` compiles and is committed; `RuntimeSprites.SquareSprite`
      still present as the fallback.
- [ ] Every entity (host-cell states, macrophage, neutrophil, DC,
      lymphocyte, large bacterium, free virion, food item, marrow/lymph
      backdrops, gut-wall bar) draws its shape sprite; every per-instance
      tint / state change still reads.
- [ ] The five flashes are distinguishable (by shape if Q4 = yes, by
      colour + location otherwise); the concurrent cap is in if Q6 = now.
- [ ] Intracellular infection still shows **only** as the host-cell
      background (§4a) — verified in a build screenshot.
- [ ] Everything from Sprints 1–12 still works — all ten harnesses re-run
      green (rendering changes shouldn't touch them; confirm).
- [ ] Clean Windows build, 0 exceptions, **a screenshot** in the CHANGELOG
      handoff. `UI_STYLE_GUIDE.md` rewritten; `ENGINE_STATUS.md` /
      `INTERFACE.md` / `CHANGELOG.md` / `BACKLOG.md` / `TEAM_RETRO.md`
      updated.

## Process note

Head session integrates the design agent's output. Commit per call-site
swap with a reasoning-heavy message; screenshot-verify each watched
entity as it lands, not in a final sweep.
