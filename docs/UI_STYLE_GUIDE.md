# UI & Visual Style Guide

Status: **rewritten after Sprint 13** (the sprite / visual-identity pass).
This is "what's on screen now"; `docs/SPRITE_DESIGN.md` is the spec it
implements and the rationale for every choice. Rewritten (not appended)
as visual decisions land.

## Direction

**"A histology plate at a glance; a readable icon up close."** The tissue
board reads as a stained section — packed eosin-pink host cells, a violet
bruise where a virus spreads, grey-brown necrotic debris, near-black
where the sheet is gone — desaturated and clinical, no hazard iconography.
The mobile agents are the only crisp, saturated, hard-edged shapes on
top: one silhouette family each, with a thin dark "membrane" rim for
figure-ground separation. Identity survives downscaling to ~14–22 px via
four channels: **silhouette family**, **hue**, **footprint size**, and
**movement behaviour** (the sim already does the last one). Interior
detail (nuclei, stipple, inclusion bodies) only resolves up close and is
never load-bearing.

## How sprites work

- **`ImmunologyTD.Rendering.SpriteShapes`** — ~20 procedurally-drawn
  64×64 `Texture2D` shapes, generated once at first access, cached in
  lazy statics (the `RuntimeSprites` pattern). Drawn **white with the
  silhouette in the alpha channel**, so the per-instance
  `SpriteRenderer.color` multiply produces the class hue and every state
  tint (cargo, paired, infected, cytokine heat, contact flash) keeps
  working with no code change. `RuntimeSprites.SquareSprite` remains as a
  fallback.
- A swap is `sr.sprite = SpriteShapes.Foo` and nothing else — no
  `sortingOrder`, no `localScale` magnitude, no `color` hook changes.
- **Per-instance variation** (subtle): a one-time random spin + ±8%
  non-uniform scale jitter on `SearchUnit` / food, and a deterministic
  ±3% per-cell colour jitter (`BoardRenderer.CellJitter`) on the host
  grid. Set once, zero per-frame cost.

## Sorting order (z-layering, back to front) — unchanged

| Order | What |
|---|---|
| 0 | Host-cell background grid (`BoardRenderer`, one `SpriteRenderer` per coarse cell) |
| 1 | Compartment backdrops (bone marrow, lymph node) |
| 3 | Gut-wall bar (`GutInterfaceRenderer`) |
| 5 | Bone-marrow slots |
| 10 | Immune cells (`SearchUnit`) |
| 12 | Lymphocytes (in the node) |
| 13 | Dendritic cells |
| 20 | Pathogens |
| 22 | The contaminated food item |
| 30 | Effect flashes (`DegranulationFlash`) |

## Entity sprites & palette (as shipped)

| Entity | `SpriteShapes` | Colour (0–1 RGB) | Silhouette |
|---|---|---|---|
| Host cell — Healthy | `HostCell` | `0.80,0.62,0.66` eosin pink | rounded opaque tile, dark 1px rim, faint nucleus |
| Host cell — Infected (viral) | `HostCellInfectedViral` | `0.54,0.36,0.60` bruised violet | tile + a crisp opaque **inclusion-body disc** + inset border |
| Host cell — Infected (bacterial) | `HostCellInfectedBacterial` | `0.62,0.60,0.26` sickly yellow-green | tile + **purulent stipple** interior + inset border |
| Host cell — Dead | `Debris` | `0.38,0.34,0.28` grey-brown | cluster of small fragments, not a whole cell |
| Host cell — Empty | `EmptyPit` | `0.13,0.11,0.12` near-black | mostly transparent, faint pit |
| Macrophage | `Macrophage` | `0.30,0.40,0.80` blue | large ruffled amoeboid blob, footprint 5 |
| Neutrophil | `Neutrophil` | **`0.93,0.74,0.30` gold** *(nudged from amber, Sprint 13)* | compact disc, multi-lobed nucleus hint, footprint 3 |
| Dendritic cell | `DendriteStar` / `DendriteStarLoaded` | `0.72,0.30,0.68` magenta → `0.98,0.62,0.98` carrying antigen | spiky ~9-point star; loaded variant adds a bright core |
| Helper-T lymphocyte | `Lymphocyte` | `0.32,0.72,0.70` teal → `0.82,0.94,0.92` paired | nucleus-heavy circle, thin cytoplasm rim (node only) |
| Large bacterium | `LargeBacterium` | `0.42,0.12,0.16` dark maroon | maroon rod / capsule, random rotation, footprint 3.5 |
| Free virus particle | `Virion` | **`0.40,0.16,0.34` cold purple** *(split from `PathogenColor`, Sprint 13)* | small crisp dot |
| Intracellular pathogen | *no sprite* (`sr.enabled=false`) | — | conveyed **only** by the host-cell background (§4a) |
| Contaminated food item | `FoodBolus` | `0.55,0.47,0.28` ochre | lumpy stippled bolus, 1.4× a coarse cell |
| Bone-marrow backdrop | `MarrowRegion` | `0.30,0.24,0.16` brown | trabecular sponge texture |
| Bone-marrow slot | `SlotNiche` | `0.62,0.56,0.42` tan → unit colour when placed | recessed rounded socket |
| Lymph-node backdrop | `LymphNodeBean` | `0.34,0.40,0.28` lymphoid green | bean / ellipse + 2 faint follicle zones |
| Gut-wall bar | `EpithelialBar` | `0.55,0.47,0.40` quiet → `0.95,0.30,0.20` alarm | row-of-cells epithelial strip; thicken+heat animation unchanged |

Cytokine heat tint (`1.00,0.55,0.05`, up to 65% blend) is applied by
`BoardRenderer` **after** sprite/colour selection, unchanged.

## Effect flashes — five distinct silhouettes (`DegranulationFlash`)

Each event now has its own shape **and** timing, not just colour — so
they stay unmistakable overlapping, on a screenshot, and for colour-blind
players. Selected in `ShapeFor(color)` keyed off the burst colour.

| Event | Colour | Shape / timing |
|---|---|---|
| Neutrophil degranulation | `1.00,0.97,0.72` yellow-white | scattered granule stipple, ~0.40s |
| Gut-wall breach | `1.00,0.35,0.22` hot red | jagged spiky starburst, ~0.35s, largest |
| Efferocytosis (pile cleared) | `0.45,0.80,0.68` blue-green | soft filled bloom, ~0.55s, smallest |
| Stress-sense loud kill (§4b) | `0.95,0.40,0.80` magenta | shockwave ring + core, ~0.45s, 1.5× |
| Knowledge match (§5c) | `0.40,0.92,0.45` green | clean thin ring + dot, ~0.50s |

`DegranulationFlash.MaxConcurrent` (24) caps simultaneous flashes
(`GAME_DESIGN.md` §8) — requests past it are dropped.

**F9** in a build fires all five at once for a look.

## HUD / panels — IMGUI (`OnGUI`), unchanged this pass

No `com.unity.ugui`. White text (`fontSize` 18; 24 bold for ATP/Lives;
13 buttons) over dark dimming panels (black 0.72–0.78 α). Debug panel
top-left, round bar top-right, shop panel left (buy phase), marrow
picker/upgrade panel anchored to the clicked slot. A real buy UI (uGUI /
UI Toolkit) is still a future sprint — see `BACKLOG.md`.
