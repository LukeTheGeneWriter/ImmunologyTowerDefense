# UI & Visual Style Guide

Status: **first pass, 2026-08-29 (end of Sprint 11)**, written by the head
session. Rewritten (not appended) as visual decisions land. This records
the *current* visual language — 11 sprints of placeholder flat-colour
rendering and IMGUI HUD — so the planned sprite/art pass has a baseline to
replace rather than reverse-engineer.

Everything visual today is a **deliberate placeholder**: flat-coloured
quads and IMGUI text. No imported art assets, no uGUI package. The colours
below were each chosen for *legibility and mutual distinctness in a
playtest*, not for a final art direction. `docs/handoff-map01-intestine.md`
§8 has the intended art direction (histology palette, clinical register)
for whenever the real pass happens.

## Rendering primitives

- **`RuntimeSprites.SquareSprite`** — one cached procedurally-generated
  white sprite (4×4 texture), tinted per-instance via
  `SpriteRenderer.color`. Every game object on screen is this quad in a
  different colour/scale. A sprite pass replaces the `sprite` assignment
  (and probably the per-class colour constants) — the *positions, scales,
  and sorting orders* below are the contract to preserve.
- **Two-resolution lattice** (`GAME_DESIGN.md` §7): coarse cell = 7×7 fine
  tiles; `FineTileWorldSize` 0.16 world units; coarse cell = 1.12 world
  units. Units/pathogens sit at coarse-cell centres or walk the fine grid;
  sprites tween between grid positions so movement isn't steppy.
- **Camera**: orthographic, background `(0.05, 0.05, 0.07)` near-black,
  fitted to the board each launch (+ a one-frame-later refit).

## Sorting order (z-layering, back to front)

| Order | What |
|---|---|
| 0 | Host-cell background grid (`BoardRenderer`, one `SpriteRenderer` per coarse cell) |
| 1 | Compartment backdrops (bone marrow, lymph node) |
| 5 | Bone-marrow slots |
| 10 | Immune cells (`SearchUnit`) |
| 12 | Lymphocytes (in the node) |
| 13 | Dendritic cells |
| 20 | Pathogens |
| 22 | The contaminated food item |
| 30 | Effect flashes (`DegranulationFlash`) |

## Colour palette (current placeholders)

### Host-cell grid (`BoardRenderer`)

| Role | RGB | Note |
|---|---|---|
| `HostColor` — healthy | `0.80, 0.62, 0.66` | eosin-ish pink |
| `InfectedHostColor` — viral | `0.54, 0.36, 0.60` | bruised violet |
| `InfectedByBacteriumColor` | sickly yellow-green | tells a bacterial infection from a viral one |
| `DebrisColor` — dead | `0.38, 0.34, 0.28` | grey-brown |
| `EmptyGroundColor` | `0.13, 0.11, 0.12` | near-black bare ground |
| cytokine heat tint | `1.00, 0.55, 0.05` | warm, blended up to 65% at full field strength; always visible |

### Units & agents

| Thing | RGB | Footprint (fine tiles) |
|---|---|---|
| Macrophage | `0.30, 0.40, 0.80` blue | 5 |
| Neutrophil | `0.95, 0.78, 0.25` amber | 3 |
| Dendritic cell | `0.72, 0.30, 0.68` magenta (→ `0.98, 0.62, 0.98` carrying antigen) | 4 |
| Lymphocyte (helper-T) | `0.32, 0.72, 0.70` teal (→ near-white `0.82, 0.94, 0.92` while paired) | small, node-scaled |
| Pathogen (large bacterium) | dark maroon `PathogenColor` | 3.5 |
| Pathogen (intracellular) | **sprite disabled** — the coarse-cell background is the only tell | — |
| Contaminated food item | `0.55, 0.47, 0.28` dull ochre | 1.4× a coarse cell |

### Effect flashes (`DegranulationFlash`) — each event a distinct colour, on purpose

| Event | RGB |
|---|---|
| Neutrophil degranulation | `1.00, 0.97, 0.72` granule yellow-white |
| Gut-wall breach burst | `1.00, 0.35, 0.22` hot red |
| Efferocytosis (pile finished) | `0.45, 0.80, 0.68` calm blue-green |
| Stress-sense loud kill (§4b) | `0.95, 0.40, 0.80` magenta, played 1.5× size |
| Knowledge match (§5c) | `0.40, 0.92, 0.45` bright green |

Burst is a 0.45s expanding, fading square (`StartScale` 0.35 → `EndScale`
1.6 of the passed size), pooled.

### Compartments

- Bone-marrow backdrop `0.30, 0.24, 0.16` brown; empty slot `0.62, 0.56,
  0.42` tan; a placed slot recolours to its unit's colour.
- Lymph-node backdrop `0.34, 0.40, 0.28` pale lymphoid green.

## HUD / panels — IMGUI (`OnGUI`)

No `com.unity.ugui` in the manifest (adding it is a conscious
network-requiring step). Everything is `GUI.Label` / `GUI.Button` /
`GUI.Box` over dark dimming rectangles.

- **Text**: white, `GUI.skin.label`, `fontSize` 18; 24 bold for the
  top-line ATP/Lives readout. Buttons `fontSize` 13.
- **Dimming panels**: black at 0.72–0.78 alpha behind every text block, so
  the readout stays legible over the board.
- **Debug panel** (top-left, ~1180×392): sprint title, board dims,
  cytokine toggle state, population / pathogen / invasion / KNOWLEDGE /
  frame-cost lines.
- **Round bar** (top-right, ~380×150): ATP · Lives, round + phase, the
  round tagline, the buy-phase prompt + Start button / GAME OVER.
- **Shop panel** (left, buy-phase only): one row per `ShopItem`, priced,
  greyed when unaffordable.
- **Marrow picker / upgrade panel**: anchored to the clicked slot's world
  position via `Camera.main.WorldToScreenPoint`.

## For the sprite / art pass

- Replace `RuntimeSprites.SquareSprite` usage (or make it a fallback);
  keep the per-instance tint hook so state changes (cargo, paired,
  infected) still read.
- Preserve the sorting-order table and the fine-tile footprint sizes.
- The **intracellular-pathogen "invisible sprite"** rule matters: an
  established infection must read as the *host cell*, not the pathogen,
  until sensed (`GAME_DESIGN.md` §4a). Don't give it its own sprite.
- The **five flash colours must stay mutually unmistakable** — one is the
  player winning (efferocytosis, knowledge), one is losing (breach).
- IMGUI is fine to keep for the debug HUD; a real buy UI is the point at
  which installing uGUI or committing to UI Toolkit is a conscious call
  (see `docs/TEAM_RETRO.md` Sprint 1).
