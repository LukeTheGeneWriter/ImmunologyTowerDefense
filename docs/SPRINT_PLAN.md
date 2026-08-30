# Sprint Plan — Sprint 15: the compartment visual pass

## Recent sprints — closed 2026-08-29 / 30

- **Sprint 12** — two Sprint 11 playtest fixes: cytokine sensing ON by
  default + a real buyable sharpen (`ShopItem.CytokineSensingUpgrade`);
  a first DC patrol movement fix (fine-grained lane-repulsion + a
  threat-axis sweep). `Sprint12Verification` 9.
- **Sprint 13** — the entity sprite / visual-identity pass:
  procedurally-drawn `SpriteShapes` replace the flat white quad for every
  entity (white + alpha-silhouette so every per-instance tint still
  works); four host-state sprites with a viral/bacterial infection
  texture split; five distinct flash silhouettes + a concurrent cap.
- **Sprint 14** — the DC-pacing rework: the shuttle collapsed from four
  states to two so a dendritic cell paces the tissue band base↔lumen its
  entire tissue life, repelling its neighbours across the lanes the whole
  time (both were confined to a patrol state it rarely occupied).
- **410 assertions total, 0 failed** across all ten harnesses.

## Direction for Sprint 15 (Director, 2026-08-30)

"The DCs behave correctly now — I watched them collect debris and teach
the helper-T cells, incrementing pathogen knowledge. Let's improve the
visuals and UI: make the lumen look like a lumen instead of other cells
in another colour; make the base, lymph node and progenitor bank look
better; and create a UI for progenitor upgrades."

Split into two sprints (Director's call): **Sprint 15 = the compartment
visual pass** (procedural, extends Sprint 13); **Sprint 16 = the UI pass**
(install UI Toolkit, build the progenitor upgrade panel as the first real
screen). A design agent was dispatched for the Sprint 15 spec, same
recipe as Sprint 13 — it delivered `docs/COMPARTMENT_DESIGN.md`.

Director decisions folded into the spec brief:
- Base compartment: **literal vascular / blood**.
- Lumen motion: agent's call → it recommended and shipped **Option B**
  (flow + mucus + a slow ±6% peristaltic squeeze; `peristalsisAmplitude`
  = 0 reverts to static Option A).
- Marrow birth-puffs: **one per real emission** (a hook, not ambient).
- Base breach: **fold a minimal acute visual into S15**.

## Scope — done

1. **`SpriteShapes` +3 alpha primitives, +9 accessors, 3 revisions**;
   `Prewarm()` finally called from `GameBootstrap.Awake`.
2. **`LumenChannelRenderer`** — chyme field + mucus wall band + ~40
   pooled `FlowMote`s drifting down the axis-frame flow, ±6% peristaltic
   squeeze. `RoundClock`-gated.
3. **`BaseCompartmentRenderer`** — oxblood `PlasmaField` lifting to a
   `VesselWallBar` at the tissue seam; ~24 pooled `Erythrocyte`
   streamers; marrow birth-puffs via `BoneMarrowManager.OnCellEmitted`
   (new cosmetic static hook); an acute red breach flash via
   `PathogenAgent.OnReachedBase` (new cosmetic static hook).
4. **Base + lumen leave the per-cell grid** — `BuildBoardVisual` only
   builds `SpriteRenderer`s for `BandOf == Tissue`; `BoardRenderer.Refresh`
   skips null views. **−110 always-resident renderers at 25×10**
   (≈ −1,990 on the 100×40 aspiration); retires the Sprint-4 scale note.
5. **`LymphNodeFieldRenderer`** — one `NodeColocGlow` quad tracking the
   value-weighted centroid of `LymphNode.Coloc`, alpha rising with the
   peak: the co-localisation gradient made visible.
6. **Backdrops** — organ halos behind marrow / lymph, `sortingOrder
   1 → 2`, marrow retinted to red marrow; gut-wall quiet colour nudged
   toward the mucus tint.

## Not in scope

- **The progenitor upgrade UI** — Sprint 16, in UI Toolkit.
- The tissue band's host-cell look (Sprint 13); agents / pathogens /
  flashes (Sprint 13).
- The food-bolus channel wake and the 3×3 co-loc haze grid — deferred to
  BACKLOG (see `COMPARTMENT_DESIGN.md` status note).
- Any gameplay / tuning / mechanics change. `RoundClock`, the sim, and
  the harness surface are untouched.

## Stopping point (definition of done) — status 2026-08-30

- [x] Lumen drawn as an open channel (chyme + mucus + pooled motes +
      peristalsis), not tinted cells.
- [x] Base drawn as bloodstream (plasma + vessel wall + erythrocytes +
      organ halos); marrow + node read as embedded organs.
- [x] Marrow birth-puffs (one per real emission) + an acute base breach
      flash.
- [x] Lymph-node interior + a live co-localisation haze.
- [x] Base + lumen cells removed from the per-cell `SpriteRenderer` grid;
      `BoardRenderer` guards null views. Renderer count down ~110 at
      25×10.
- [x] All ten harnesses green — **410, 0 failed** (rendering-only, no
      harness path touched).
- [x] Clean Windows build (0 errors, 93,378,880 bytes); headless launch
      0 exceptions (~31 rasters generate in `Awake`).
- [x] `COMPARTMENT_DESIGN.md` / `UI_STYLE_GUIDE.md` / `ENGINE_STATUS.md` /
      `INTERFACE.md` / `CHANGELOG.md` / `BACKLOG.md` / `TEAM_RETRO.md` /
      this file updated.
- [ ] **How it looks in motion.** No headless coverage of `Update()`; the
      handoff is the build. The Director's playtest.

**Handed to the Director for playtest.**

## Next: Sprint 16 — the UI pass

- Install **UI Toolkit** (a deliberate network step, like `com.unity.ugui`
  in `TEAM_RETRO.md` Sprint 1).
- Build the **progenitor upgrade panel** as the first real screen: which
  progenitor is selected, its kind + level, the per-kind upgrade options
  (named placeholders are fine — "Macrophage: faster efferocytosis",
  "DC: cargo capacity +1", "Progenitor: emit rate +"), cost, effect text.
  The selected-slot rim highlight hook noted in `COMPARTMENT_DESIGN.md
  §2.4` gets built here.
- Then port the shop / HUD off IMGUI.

## Process note

Bugfix-style sprints (14) run inline; visual/spec sprints (13, 15) run
via a dispatched design agent that produces a spec + optional uncompiled
prototype, with the head integrating. Commit per coherent chunk with a
reasoning-heavy message; the Director's screenshot is the rendering QA.
EOF
