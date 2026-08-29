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

## Director decisions (2026-08-29) — all as the agent recommended

1. **Palette: nudge the flagged ones only.** Neutrophil amber
   `0.95,0.78,0.25` → gold `~0.93,0.74,0.30`; free virus particle → a
   colder purple-maroon `~0.40,0.16,0.34` (so virus ≠ bacterium at a
   glance). Cytokine-heat orange and the knowledge / efferocytosis greens
   **stay** (load-bearing, legible). Everything else unchanged.
2. **Per-instance variation: subtle.** One-time-at-spawn random rotation
   on non-round shapes, ±8% non-uniform scale jitter, a tiny hue nudge.
   No per-frame cost.
3. **Flashes: five distinct silhouettes** (stipple burst / jagged red
   star / soft bloom / shockwave ring / thin ring) + per-shape timing,
   via an internal `switch` on `burstColor` so every `Play(...)` call
   site is unchanged. **And the concurrent-flash cap goes in** (`GAME_DESIGN.md`
   §8).
4. **Compartments + infection: full.** Lymph node → bean/ellipse with
   follicle zones; bone marrow → trabecular texture; gut-wall bar →
   row-of-cells epithelial skin. Infected cells get a **texture split** —
   swollen inclusion (viral) vs. granular purulent (bacterial) — on top
   of the violet / yellow-green hue.

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
