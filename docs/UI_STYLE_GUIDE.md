# UI & Visual Style Guide

Status: **updated after Sprint 17** (the cartoon pass on the lumen and the
vessel), on top of Sprint 16 (the UI pass), Sprint 15 (the compartment
visual pass) and Sprint 13 (the entity sprite / visual-identity pass).
This is "what is on screen now";
`docs/SPRITE_DESIGN.md` (entities), `docs/COMPARTMENT_DESIGN.md`
(compartments) and `docs/UI_DESIGN.md` (the HUD and buy screens) are the
specs it implements and the rationale for every choice. Rewritten (not
appended) as visual decisions land.

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

## Sorting order (z-layering, back to front)

| Order | What |
|---|---|
| 0 | **Tissue** host-cell grid (`BoardRenderer`, one `SpriteRenderer` per *tissue-band* cell only); base **plasma field**; lumen **chyme field** |
| 1 | Base **vessel-wall cells** (*Sprint 17: a tiled row, was one stretched bar*); **organ halos** (behind the backdrops); lumen **villi** *(new, Sprint 17)* |
| 2 | Compartment backdrops (bone marrow, lymph node); lumen **mucus sheen** (*Sprint 17: moved up from 1, so it glazes the villi*); **erythrocyte streamers** |
| 3 | Gut-wall bar (`GutInterfaceRenderer`); node **co-localisation haze**; base **breach flash** |
| 4 | Marrow **birth-puff motes**; lumen **flow motes** (*Sprint 17: moved up from 2, so a mote at the seam isn't half-swallowed by the wall bar*) |
| 5 | Bone-marrow slots |
| 10 | Immune cells (`SearchUnit`) |
| 12 | Lymphocytes (in the node) |
| 13 | Dendritic cells |
| 20 | Pathogens |
| 22 | The contaminated food item |
| 30 | Effect flashes (`DegranulationFlash`) |

The **base and lumen bands are no longer part of the per-cell grid** — Sprint
15 draws them with dedicated field/mote renderers (`BaseCompartmentRenderer`,
`LumenChannelRenderer`), which removed ~110 always-resident `SpriteRenderer`s
at 25×10 and stopped `BoardRenderer.Refresh` recolouring them every 0.15 s.

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
| Bone-marrow backdrop | `MarrowRegion` | **`0.34,0.22,0.18` red marrow** *(Sprint 15)* | trabecular sponge, bolder struts + sinusoid channels |
| Bone-marrow slot | `SlotNiche` | `0.62,0.56,0.42` tan → unit colour when placed | recessed rounded socket |
| Lymph-node backdrop | `LymphNodeBean` | `0.34,0.40,0.28` lymphoid green | bean + **3** follicle zones + a medullary notch *(Sprint 15)* |
| Gut-wall bar | `EpithelialBar` | **`0.50,0.46,0.37` quiet** *(nudged toward mucus, Sprint 15)* → `0.95,0.30,0.20` alarm | row-of-cells strip + goblet flecks; thicken+heat animation unchanged |

### Compartment fields & motes (Sprint 15, **re-painted Sprint 17** — `docs/COMPARTMENT_DESIGN.md`)

The Sprint 17 pass is a legibility fix, not decoration: the contaminated
food bolus is ochre and lumpy on purpose, and Sprint 15 painted the
channel it travels down in the same browns. **The bolus is now the only
brown thing in the lumen.**

| Element | `SpriteShapes` | Colour (0–1 RGB) | Notes |
|---|---|---|---|
| Lumen chyme field | `ChymeField` | **`0.36,0.22,0.24` mucosal plum** *(was brown-olive)* | one quad over the lumen band, alpha fades down-flow; ±6% peristaltic squeeze |
| Lumen **villus** | **`Villus`** *(new)* | **`0.80,0.50,0.47` coral ±7%** | fringe along the gut wall, ~0.42×1.15 cells, spaced ~0.78, sways ±5° on its own phase |
| Lumen mucus sheen | `MucusBand` | **`0.88,0.84,0.76` pearly @ 0.20α** *(was grey-green @ 0.5)* | glazes the villi — that glaze is the "velvety" read |
| Flow mote | `FlowMote` | **`0.60,0.52,0.40` pale cream ±8%** | **~28** pooled *(was ~40)*, drift down-flow, recycle at the excretion edge |
| Base plasma field | `PlasmaField` | `0.33,0.11,0.15` oxblood | one quad over the base band, alpha lifts toward the wall |
| **Vessel wall cell** | **`EndothelialCell`** *(new)* | **`0.78,0.50,0.52` warm pink ±6%** | a tiled row at the base/tissue seam, ~0.92 cells apart — replaces the single stretched `VesselWallBar` quad (the sprite stays, unused) |
| Organ halo | `OrganHalo` | `0.20,0.06,0.09` deep plasma | dark ring + bright rim behind the marrow / lymph backdrops |
| Erythrocyte | `Erythrocyte` | **`0.72,0.20,0.22` arterial red** @ 0.9α | ~24 pooled, drift outer-edge → wall; deeper central dip so it reads as a biconcave dish |
| Birth puff | `BirthPuff` | `0.70,0.62,0.50` pale marrow | one per real emission (`BoneMarrowManager.OnCellEmitted`), fades over 1.5 s |
| Base breach flash | `EffeBloom` | `0.86,0.10,0.12` red | expanding fade at the arrival lane on `PathogenAgent.OnReachedBase`, ~0.55 s |
| Node co-loc haze | `NodeColocGlow` | `0.55,0.85,0.85` cyan-white, ≤35%α | tracks the value-weighted centroid of `LymphNode.Coloc` |

All compartment field/mote elements -- villi included -- freeze with
`RoundClock.Frozen` (except an in-flight breach flash, which finishes --
it is sub-second).

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

## HUD / panels — UI Toolkit, built from code (Sprint 16)

**There is no `OnGUI` left in the project.** Every screen-space surface is
UI Toolkit assembled in C# against `ImmunologyTD.UI.UiTheme` — no `.uxml`,
no `.uss`, no UI Builder. **One** asset:
`Assets/Resources/ITD_PanelSettings.asset` (`ScaleWithScreenSize`,
1920×1080 reference, `sortingOrder` 100, `clearColor` false), created by
script via `UiAssetSetup.CreatePanelSettings` and loaded in
`GameBootstrap.BuildUiRoot`. It is not optional: a `PanelSettings` made at
runtime carries no text settings, and a *player build* then throws in the
text shaper on every label — silently fine in the Editor. No package was
added: `com.unity.modules.uielements`
was already in the manifest — `com.unity.ugui` is still absent and still
not wanted. Full spec and rationale: `docs/UI_DESIGN.md`.

**Direction: the chart clipped to the specimen, not a cockpit.** The board
is a stained section and the agents are the only saturated things on it,
so the UI is quieter than both: flat translucent glass, hairline rules,
one type family, 3 px corners, no glow / bevel / gradient. Colour is spent
only where meaning must survive a glance.

| Token | Value | Used for |
|---|---|---|
| `PanelBg` | `0.059,0.055,0.067` @ 0.86 α | every panel (0.94 for the debug readout — it's an instrument) |
| `Ink` | `0.914,0.898,0.855` | primary text |
| `InkDim` | `0.545,0.522,0.478` | labels, effect text, compartment headings |
| `Rule` | `Ink` @ 0.14 α | 1 px borders and dividers |
| `Atp` | `0.796,0.722,0.471` | the ATP numeral — deliberately duller than neutrophil gold |
| `LivesOk` / `LivesLow` | `0.510,0.663,0.627` / `0.753,0.361,0.263` | lives; oxblood under 25% of max |
| `Accent` | `0.431,0.561,0.690` | "this is interactive" — buy bars, level dots, the board's selection rim |
| `Defeat` | `0.604,0.231,0.173` | GAME OVER |

Type scale: 26 px bold (stat numerals), 13 px (row names, buttons), 11 px
(effect text, cost, headings), 10 px uppercase + 0.10em tracking (panel
titles), 12 px monospace (debug readout only). Spacing is a 4 px unit.
Fonts come from `Font.CreateDynamicFontFromOSFont` and are assigned
explicitly at the root; the panel's default theme is a floor, not the
look — every element sets its own colour, border, radius and padding.

Where things are:

- **HUD** — top-right, always on. ATP / round / lives, the phase block and
  Start Round. A readout only earns a permanent spot if the player decides
  something from it every few seconds.
- **Shop** — right, under the HUD. Open **during rounds too**; collapses
  to a header strip while a round runs, one click to reopen.
- **Upgrade panel / tower picker** — float at the clicked marrow slot,
  mutually exclusive, with a breathing `Accent` rim on the slot itself
  (sorting order 6).
- **Debug readout** — bottom-left, monospace, **backtick to toggle,
  default off**. Everything the old top-left dump showed.
- **Compartment headings** — world-anchored UITK labels ("Bone marrow",
  "Lymph node"), 11 px `InkDim` over a hairline rule, projected each frame
  with `RuntimePanelUtils.CameraTransformWorldToPanel`.

The UITK panel is its own layer above the whole sprite sorting table
above; it does not participate in `sortingOrder`. The root and the dock
containers are `PickingMode.Ignore`, so a click that isn't on a panel
falls through to the physics raycaster — **check that first if marrow slot
clicks ever stop registering.**
