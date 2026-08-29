# Sprite / Visual Identity Design Spec

Status: **first pass, 2026-08-29**, written by a dispatched Design agent.
Companion to `docs/UI_STYLE_GUIDE.md` (the current placeholder language)
and `docs/handoff-map01-intestine.md` §8 (the intended art direction).
Nothing here is built yet — this is the spec the sprite pass implements.
An optional prototype of the shape generators lives at
`game/Assets/Scripts/Rendering/SpriteShapes.cs` (compiles conceptually,
wired nowhere — see §5).

The hard constraints from `UI_STYLE_GUIDE.md` "For the sprite / art pass"
are preserved throughout: the sorting-order table, the fine-tile footprint
sizes, the per-instance tint hooks, the intracellular "no sprite" rule,
and the five mutually-unmistakable flash colours.

---

## 1. Visual direction

**"A histology plate at a glance; a readable icon up close."**

The board should read like a stained tissue section — eosin-pink host
cells packed edge to edge, hematoxylin-dark nuclei, a violet bruise
spreading where a virus is winning, grey-brown necrotic debris where the
line broke. Desaturated, clinical, no hazard-orange and no biohazard
iconography (`handoff` §8). Against that muted, low-contrast field the
**mobile agents are the only crisp, saturated, hard-edged shapes on
screen** — every immune cell and every extracellular pathogen is a single
simple silhouette (disc, rod, lobed blob, dendritic star) with a thin dark
"membrane" rim for figure-ground separation, painted in the hue it already
has in playtest.

Legibility has to survive two things the art direction does not get to
negotiate with: entities render **small** (a 3–5 fine-tile unit is
~0.5–0.8 world units, ~14–22 px at the nominal 28 px/coarse-cell zoom; a
pathogen ~14 px) and late rounds put **hundreds of them on screen at
once** (`GAME_DESIGN.md` §8). So identity is carried by four channels that
still work after aggressive downscaling —

1. **silhouette family** — round vs. rod vs. multi-lobed vs. spiky;
2. **hue** — the playtested per-class colours, kept mutually distinct;
3. **footprint size** — macrophage bigger than neutrophil, etc.;
4. **movement behaviour** — already handled by the sim (random walk vs.
   biased vs. stationary vs. momentum walk).

Interior detail (nuclei, granules, stipple, inclusion bodies) is a
**bonus that only resolves when the entity is large or the camera is
close** — it is never load-bearing. The "player winning vs. losing"
colour logic from the current palette is preserved: cool blues / teals /
greens for the player gaining ground, hot reds and violet bruising for
losing it.

Everything is **one-time procedural** `Texture2D`: ~12 shared shape
sprites generated once at boot, tinted per instance via
`SpriteRenderer.color` exactly as today, zero per-frame cost, no asset
pipeline (§4).

---

## 2. Per-entity sprite spec

Coordinates below are "inscribed in a 64×64 texture with a ~3 px margin"
unless noted. All sprites are drawn in **white** (`rgb 1,1,1`) with the
shape carried in the **alpha channel**, so the existing
`SpriteRenderer.color` multiply produces the class hue and every state
tint keeps working untouched. Immune cells and extracellular pathogens
get a **1–2 px rim darkened to ~0.55×** the fill — the "membrane" — which
is what lifts a 16 px dot off the busy board.

### 2.1 Host-cell background grid (`BoardRenderer`, sorting order 0)

One `SpriteRenderer` per coarse cell (4,000 of them). Today each is a flat
tinted square. The pass gives `BoardRenderer` **three shared sprites** to
choose between per cell, alongside the colour it already computes:

| State | Sprite | Palette (unchanged) | Silhouette / texture | Reads at 28 px because… |
|---|---|---|---|---|
| **Healthy** | `HostCell` | `0.80, 0.62, 0.66` eosin pink | Rounded square filling ~92% of the tile, opaque, a **1 px darker rim** (packed-cell membranes) and a faint off-centre nucleus shade (~0.85×) | Tiling rounded squares read as "epithelium", the rim gives the sheet visible cell boundaries |
| **Infected — viral** | `HostCellInfected` | `0.54, 0.36, 0.60` bruised violet | Same cell shape + a **brighter swollen centre** (inclusion-body / cytopathic look, ~1.15× toward white before tint) + a crisp 2 px inset border | Same value as Healthy so it never reads as a hole; the inset border gives an infected patch a countable edge (front legibility) |
| **Infected — bacterial** | `HostCellInfected` | `0.62, 0.60, 0.26` sickly yellow-green (code value; the guide's "yellow-green" prose) | Same shape + a **granular / stippled interior** (purulent) instead of the smooth inclusion | Hue does the main work; the stipple only resolves up close but disambiguates a still-frame screenshot |
| **Dead — debris** | `Debris` | `0.38, 0.34, 0.28` grey-brown | **Cluster of 6–8 small irregular fragments** with a dark rim, not one cell — rubble, not a substance | Broken silhouette vs. the whole rounded cell is obvious even tiny; browner + lighter than Empty |
| **Empty ground** | `EmptyPit` (or stay flat) | `0.13, 0.11, 0.12` near-black | Mostly transparent with a faint inward vignette — a pit, the *absence* of a cell | Darkest thing on the board, and visibly not a surface |

Cytokine heat tint (`1.00, 0.55, 0.05`, blended up to 65%) is applied by
`BoardRenderer` *after* sprite choice exactly as today — an infected cell
still shades toward orange as it ramps. **Keep the host-cell sprite
opaque** (rim drawn *inside* the opaque area, not as transparent margin) so
4,000 of them add no overdraw versus today's opaque quads.

Per-cell variation: a small deterministic hue/value jitter (±3%) seeded by
`(col,row)` gives the section a histology-plate mottle instead of a flat
wash. Optional — see open question 3.

### 2.2 Macrophage (`SearchUnit`, order 10, footprint **5** fine tiles)

- **Silhouette:** large **amoeboid blob** — a disc with 4 broad shallow
  lobes (`lobeDepth ~0.16`), i.e. a ruffled membrane. Biggest, roundest,
  softest-edged immune cell.
- **Palette:** `0.30, 0.40, 0.80` blue (unchanged). Cool = player's innate
  workhorse; sits well in a hematoxylin register.
- **Interior:** one soft dark nucleus shade near centre (~0.7×). Resolves
  only when large.
- **Per-state:** none on the sprite. Efferocytosis and contact are the
  flash's job (§3), not a sprite swap.
- **Small-size read:** the lobed outline + the largest footprint + the
  slowest movement (`FineTilesPerTick`) already say "macrophage" before
  colour does. Per-instance random rotation so a cluster doesn't look
  stamped.

### 2.3 Neutrophil (`SearchUnit`, order 10, footprint **3** fine tiles)

- **Silhouette:** compact **circle**, hard-edged, with a **multi-lobed
  nucleus** hint — 3 small dark kidney-bean shades inside (the classic
  polymorphonuclear look).
- **Palette:** `0.95, 0.78, 0.25` amber (unchanged). *Flag:* this is close
  to the "avoid hazard-orange" note in `handoff` §8 — see open question 2;
  a nudge to `0.93, 0.74, 0.30` gold keeps it distinct from macrophage
  blue and DC magenta while stepping away from hazard-orange.
- **Interior:** the tri-lobe nucleus is the only detail; at 12–16 px it
  reads as "a granular dot", which is enough against the macrophage's
  irregular blob.
- **Per-state:** none. Degranulation is the yellow-white burst (§3.1).
- **Small-size read:** smallest, roundest, fastest of the two tissue
  units; hue seals it.

### 2.4 Dendritic cell (`DendriticCell`, order 13, footprint **4** fine tiles)

- **Silhouette:** **spiky / dendritic star** — a small core disc with
  ~9 thin radiating processes (`rOuter ≈ 1.6× rInner`). This is *the*
  distinctive shape in the roster; the spike count and radius contrast are
  chosen so it still reads as "spiky, not round" at ~14 px.
- **Palette:** `0.72, 0.30, 0.68` magenta empty → `0.98, 0.62, 0.98`
  brighter/whiter while carrying antigen (both unchanged; the DC already
  lerps `sr.color` between `EmptyColor` and `CargoColor` every tick).
- **Per-state variant:**
  - *Empty* — hollow-ish star (core disc only).
  - *Carrying antigen* — add a **bright filled core dot** (a distinct
    `DendriteStarLoaded` sprite). Guaranteed-safe path is the existing
    tint delta alone; the sprite swap is a one-line optional add in
    `SimulationTick` (`sr.sprite = HasCargo ? loaded : empty`) for the
    head to integrate.
- **In the lymph node** the DC is scaled down (`node.AgentWorldSize *
  1.15`). At that size the star degrades toward a rough blob — acceptable,
  because it is the only magenta thing among teal lymphocytes and context
  carries identity. Keep the spikes few and bold rather than many and
  fine so it degrades gracefully.

### 2.5 Helper-T lymphocyte (`Lymphocyte`, order 12, node-scaled)

- **Silhouette:** small **smooth circle that is mostly nucleus** — a big
  interior shade (~0.72×) with only a thin bright rim of cytoplasm. That
  "nucleus with a sliver of cytoplasm" is the real lymphocyte look and it
  distinguishes it from the neutrophil (multi-lobed nucleus, and in tissue
  not the node) and the DC (star).
- **Palette:** `0.32, 0.72, 0.70` teal roaming → `0.82, 0.94, 0.92`
  near-white while paired (both unchanged; `NodeTick` already lerps).
- **Per-state variant:** roaming vs. paired is the tint (wired). Optional
  nicety — a paired pair could draw a short connector line — is out of
  scope (would touch `LymphNode`).
- **Small-size read:** it lives only in the node, at a fixed small scale,
  among DCs (magenta stars) and nothing else. Circle + teal is
  unambiguous there.

### 2.6 Large bacterium (`PathogenAgent` class `LargeBacterium`, order 20, footprint 3.5)

- **Silhouette:** a **rod / capsule** (stadium shape), ~0.8 of the texture
  long by ~0.28 wide. The only non-round extracellular thing — aspect
  ratio alone identifies it at any size. **Per-instance random rotation**
  (bacteria point every which way).
- **Palette:** `0.42, 0.12, 0.16` dark maroon (`BoardRenderer.PathogenColor`,
  unchanged). Darker, redder and cooler than eosin tissue → reads
  *foreign* against the histology palette without being cartoon-evil.
- **Per-state:** contact flash is the existing yellow lerp in
  `PathogenAgent.LateUpdate` (`FlashColor 0.95,0.85,0.3`) — keep. In the
  lumen and on the wall pile it is drawn as itself (same rod, same maroon).
- **Small-size read:** rod vs. every other pathogen's dot.

### 2.7 Free virus particle (`PathogenAgent` class `IntracellularVirus`, extracellular, order 20)

- **Silhouette:** a **small crisp dot** — a filled disc ~60% of the
  bacterium's footprint, faint rim. Smaller = weaker/transient, which is
  true (it is on a survival clock). A faint hexagonal facet hint is
  optional and won't resolve at this size.
- **Palette:** currently also `PathogenColor` maroon.
  **Recommended refinement:** give the free virion its own colder
  purple-maroon (`~0.40, 0.16, 0.34`) so "virus" and "bacterium" separate
  at a glance while both stay in the foreign dark-red/violet family.
  `ApplyRestColorForCurrentClass` already branches on `IsIntracellular`;
  adding a class branch there is a small call-site change for the head.
- **Why it matters:** a **budding infection** spits a *cloud* of these
  that should read as an expanding stipple of tiny dots (momentum walk,
  `GAME_DESIGN.md` §4b). Small-dot sprite + radial motion delivers that.
- **Small-size read:** tiny dot, colder hue, drifting outward from a
  violet infected patch.

### 2.8 Intracellular pathogens — **no own sprite** (`GAME_DESIGN.md` §4a)

While `IsIntracellular` the agent's `SpriteRenderer` is disabled
(`sr.enabled = false`) — unchanged, and load-bearing: an established
infection must read as *the host cell* until an immune cell senses it. The
infection is conveyed **entirely by the coarse-cell background** (§2.1):

- **Established virus** → that cell's quad is `HostCellInfected` in
  bruised violet, swollen-inclusion texture, cytokine-heat orange ramp on
  top. A **contact-chain** virus paints a *snaking line* of violet cells;
  a **budding** virus paints a *growing disk* of them. That spatial
  pattern is the entire read, and it is legible precisely because the
  infected-cell sprite has a crisp 2 px inset border so the front has an
  edge you can count.
- **Established intracellular bacterium** → the cell's quad is
  `HostCellInfected` in yellow-green with the granular/purulent interior.
  When the bacterium ducks *out* (extracellular) its maroon rod sprite
  re-enables on the occupant layer on top of a now-`Healthy` (pink) cell;
  when it ducks back *in*, the rod vanishes and the cell goes yellow-green
  again. That **flicker between pink+rod and flat yellow-green** is the
  tell the Director asked to be able to watch.
- Sensing it is the **magenta stress-kill flash** (§3.4) — a loud event
  precisely because nothing about the sprite changed until that moment.

### 2.9 Contaminated food item (`PathogenSpawner`, order 22, 1.4× a coarse cell)

- **Silhouette:** a **lumpy asymmetric bolus** — a lobed blob with more
  and deeper lobes than a macrophage (`lobes 6`, `lobeDepth ~0.28`,
  randomised phase), plus a mottled/stippled interior. A chunk of matter,
  not a cell and not a pathogen. Random rotation.
- **Palette:** `0.55, 0.47, 0.28` dull spoiled-food ochre (unchanged) —
  already distinct from maroon pathogens, eosin tissue, and every immune
  hue.
- **Per-state:** none; it just transits the lumen and drops bursts.
- **Size read:** far bigger than anything else in the lumen, slow, ochre.

### 2.10 Compartment backdrops

| Element | Where | Order | Palette (unchanged) | Silhouette / texture |
|---|---|---|---|---|
| **Bone-marrow backdrop** | `GameBootstrap.BuildBoneMarrowBackdrop` | 1 | `0.30, 0.24, 0.16` brown | Filled region with a **spongy trabecular texture** — a few soft lighter struts over darker marrow spaces, so it isn't a flat rectangle |
| **Bone-marrow slot** | `BoneMarrowManager` | 5 | `0.62, 0.56, 0.42` tan empty; recolours to the unit's colour when placed | A **rounded niche / socket** — filled rounded square with a darker inset ring, so an empty slot reads as a receptacle. *Optional:* when filled, draw the placed unit's own shape sprite tinted to its colour, so the player sees *what* is in the slot (call-site change in `BoneMarrowManager` for the head) |
| **Lymph-node backdrop** | `GameBootstrap.BuildLymphNodeBackdrop` | 1 | `0.34, 0.40, 0.28` pale lymphoid green | *Recommended:* a **filled bean / ellipse** rather than a square quad (it is a discrete organ), with 2–3 faint lighter circular zones (follicles / germinal centres). See open question 5 |
| **Gut wall bar** | `GutInterfaceRenderer` | 3 | `0.55, 0.47, 0.40` quiet → `0.95, 0.30, 0.20` alarm | Keep the per-position **thicken + heat** animation exactly (it is load-bearing, `SPRINT_PLAN.md` item 6). Just skin the bar with an **epithelial brick / row-of-cells texture** so the barrier reads as the epithelium. The `Refresh()` `localScale` and colour-lerp maths are untouched by a textured sprite |

---

## 3. The five effect flashes (`DegranulationFlash`, order 30)

Today all five are the **same** shape — a 0.45 s square expanding
`StartScale 0.35 → EndScale 1.6`, fading alpha `1 → 0` — separated only by
colour. **Recommendation: give each event its own silhouette *and*
timing** as well as its colour, so they stay unmistakable when they
overlap, on a screenshot, and for colour-blind players. The colours are
unchanged; one is the player winning (efferocytosis, knowledge), one is
losing (breach), and they must never be confused.

| # | Event | Colour (unchanged) | Shape | Timing / scale | Feel |
|---|---|---|---|---|---|
| 1 | Neutrophil degranulation | `1.00, 0.97, 0.72` granule yellow-white | **Scattered-dot burst** — a stipple ring of granules flung outward | ~0.40 s, `0.35 → 1.6` | granules spilling |
| 2 | Gut-wall breach | `1.00, 0.35, 0.22` hot red | **Jagged spiky starburst** — angular, torn | ~0.35 s (fastest), `0.4 → 1.9` (largest) | violent, the line broke |
| 3 | Efferocytosis (pile cleared) | `0.45, 0.80, 0.68` calm blue-green | **Soft filled bloom** — a gaussian disc, no hard edge | ~0.55 s (slowest), `0.3 → 1.3` (smallest) | quiet recovery |
| 4 | Stress-sense loud kill (§4b) | `0.95, 0.40, 0.80` magenta | **Bold expanding ring / shockwave** + bright core | ~0.45 s, `0.35 → 1.6`, already played 1.5× size | necrotic, deliberate, hard |
| 5 | Knowledge match (§5c) | `0.40, 0.92, 0.45` bright green | **Clean thin ring** with a steady centre dot | ~0.50 s, `0.3 → 1.4` | the long game paying off, in the node |

Ring vs. filled bloom vs. stipple vs. spiky-star = four distinct
silhouettes; #4 and #5 are both rings but fire in different compartments,
at different sizes, in colours that are already far apart. Implementation
is cheap: `DegranulationFlash.Begin` can pick the shape from `burstColor`
via an internal `switch` (the five colours are `static readonly` fields on
the class), so **every existing `Play(...)` call site is unchanged**.
Alternatively add a `Shape` enum parameter with overloads.

*Flag (`GAME_DESIGN.md` §8):* there is no explicit cap on concurrent
cosmetic flashes today — the pool will grow to whatever peak demand hits.
A hard ceiling that drops new requests past N is called for; noted here,
see open question 7.

---

## 4. Implementation recommendation — procedural `Texture2D`

**Recommendation: extend the runtime-sprite approach with a small library
of procedurally-drawn shape textures.** Concretely, a new static class
(`SpriteShapes`) that draws ~12 shapes once at boot into cached `Sprite`
objects, exactly the way `RuntimeSprites.SquareSprite` already caches its
one 4×4 white quad — just more shapes, at higher resolution.

### Why procedural, not authored PNGs

- **No asset pipeline.** The project has no imported art, no
  `com.unity.ugui`, no Editor GUI in the loop — everything is built from
  code at runtime (`AGENT_HANDBOOK.md`). Authored PNGs need import
  settings, sprite-atlas config, `.meta` wrangling and a human in the
  Editor for every tweak. That is a different way of working than the
  whole project.
- **The vocabulary is small and geometric.** Disc, ring, rod, lobed blob,
  spiky star, stipple — a dozen shapes covers every entity. Parametric
  variation (lobe count, spike count, rod aspect) is nearly free in code
  and impossible in a PNG without re-exporting.
- **Perf is a non-issue.** Generation is one-time at boot (~12 textures ×
  64×64 × 4 bytes ≈ 200 KB total). Every instance of a class **shares one
  `Sprite` reference** — 150 pooled pathogens still point at 1–3 sprites —
  so draw-call batching is identical to today (one sprite, one material).
  Per-instance state stays on `SpriteRenderer.color`, zero per-frame
  allocation.
- **It stays headless-friendly.** Like `RuntimeSprites`, the generator is
  a plain static with lazy init; a harness that never touches it pays
  nothing, and it does not depend on `Awake()` (which does not fire in
  batchmode — `AGENT_HANDBOOK.md`).

Authored PNGs only start to win if a real artist joins and the art bar
rises past "clean geometric icons" — at which point the atlas + import
cost is worth paying. Not now.

### Sprite generation parameters

- **Resolution 64×64** per shape. Entities render at 14–40 px, so 64 is
  ample headroom; `filterMode = Bilinear` (not `Point` like the square)
  for clean downscaling; no mipmaps.
- **`Sprite.Create(tex, new Rect(0,0,64,64), new Vector2(0.5f,0.5f),
  64f)`** → a 1×1 world-unit sprite, matching `SquareSprite`'s
  `Sprite.Create(..., 4f)` (4 px @ 4 PPU = 1 unit). Every call site's
  `transform.localScale = worldSize` maths is untouched.
- **Draw in white, shape in alpha.** `SpriteRenderer.color` multiply then
  gives the hue, and every state tint (cargo, paired, infected lerp,
  cytokine heat, contact flash) keeps working with no change.
- **Anti-aliased edges** via 4× supersampled coverage in the fill test.
- **Membrane rim:** an edge pass that multiplies the outer 1–2 px of the
  opaque region by ~0.55 — the single biggest legibility win for a 16 px
  agent on a busy board.
- **Per-instance variation, one-time, no per-frame cost:** at
  `Initialize()` set `transform.rotation = Quaternion.Euler(0, 0,
  Random.value * 360f)` for rods / blobs / stars (not circles), an
  optional ±8% non-uniform scale jitter, and an optional tiny HSV nudge
  on the base colour. All set once. See open question 3 for how much.

### Shape-drawing primitives (sketch — see `SpriteShapes.cs`)

Pure static functions on a `Color[64*64]` buffer (white RGB, coverage in
alpha, `max`-blended); composites call them in sequence.

```
Color[] NewBuffer()                                  // (1,1,1,0) ×4096
Sprite  ToSprite(Color[] buf)                        // Texture2D + Sprite.Create, PPU 64, bilinear

void FillDisc   (buf, cx, cy, r)                     // AA filled circle
void FillRing   (buf, cx, cy, rOuter, rInner)        // annulus (flashes 4/5, slot niche)
void FillCapsule(buf, x0, y0, x1, y1, halfW)         // stadium — large bacterium
void FillLobed  (buf, cx, cy, baseR, lobes, depth, phase)
                                                     // r(θ)=baseR·(1+depth·sin(lobes·θ+phase))
                                                     //   depth 0.16 few  → macrophage / amoeboid
                                                     //   depth 0.28 many → food bolus
void FillStar   (buf, cx, cy, rInner, rOuter, points, phase)   // dendritic cell, breach flash
void FillRounded(buf, cx, cy, halfExtent, corner)    // packed host cell
void Stipple    (buf, seed, density)                 // multiply alpha by blue-noise mask where already opaque
                                                     //   purulent interior, debris, granule burst
void InnerShade (buf, cx, cy, r, mul)               // soft interior darkening — nucleus / inclusion
void RimShade   (buf, widthPx, mul)                 // edge-detect on alpha, multiply rim rgb — the membrane
void Multiply   (buf, r, g, b, a)                   // flat tint of the whole buffer
```

Composites (illustrative):

```
Macrophage       = FillLobed(c, 26, 4, 0.16) ; InnerShade(c, 10, 0.70) ; RimShade(2, 0.55)
Neutrophil       = FillDisc (c, 24) ; 3× InnerShade(offset, 8, 0.60) ; RimShade(2, 0.60)
DendriteStar     = FillStar (c, 12, 30, 9) ; FillDisc(c, 10) ; RimShade(1, 0.60)
DendriteStarLoaded = DendriteStar ; FillDisc(c, 7)            // bright core, full alpha
Lymphocyte       = FillDisc (c, 24) ; InnerShade(c, 17, 0.72) ; RimShade(1, 0.75)
LargeBacterium   = FillCapsule(span 0.78 len, halfW 0.22) ; RimShade(2, 0.50)
Virion           = FillDisc (c, 14) ; RimShade(1, 0.50)
FoodBolus        = FillLobed(c, 28, 6, 0.28, rand) ; Stipple(seed, 0.30) ; RimShade(2, 0.60)
HostCell         = FillRounded(c, 30, 8) ; InnerShade(offset, 9, 0.85) ; RimShade(1, 0.80)
HostCellInfected = FillRounded(c, 30, 8) ; InnerShade(c, 12, 1.12→toward white) ; inset 2px border
Debris           = 6–8× FillDisc(rand small) ; Stipple(seed, 0.5) ; RimShade(2, 0.55)
EmptyPit         = InnerShade(c, 30, 0.0) only            // mostly transparent
GranuleBurst     = FillRing(c, 30, 8) ; Stipple(seed, 0.6)
BreachStar       = FillStar(c, 14, 32, 10) ; hollow centre
EffeBloom        = FillDisc(c, 30) with wide AA falloff
StressRing       = FillRing(c, 30, 20) ; FillDisc(c, 8)
KnowledgeRing    = FillRing(c, 30, 24) ; FillDisc(c, 6)
```

`SpriteShapes.cs` in this commit implements the primitives and these
composites. **It has not been compiled** (no Unity on this machine) — it
follows the project's existing style (explicit null checks, lazy static
caches, `UnityEngine` only) and should be reviewed at integration.

---

## 5. Migration plan

Every step is an isolated sprite swap on a `SpriteRenderer` whose
`sortingOrder`, `localScale`, and `color` assignments are set elsewhere
and **left exactly as they are**. No sorting order changes. No footprint
changes. No tint hook changes.

### 5.0 — Land the generator (no behaviour change)

1. Add `game/Assets/Scripts/Rendering/SpriteShapes.cs` **and its
   `.meta`** (handbook rule). Compiles, referenced nowhere.
2. Leave `RuntimeSprites.SquareSprite` in place as the fallback — do not
   delete it.

### 5.1 — Swap call sites, one entity at a time

Each is independently build-testable (spawn that entity, look at it).
`grep` target list, from `RuntimeSprites.SquareSprite` usages:

| Order | File / method | Change |
|---|---|---|
| a | `Units/UnitProfile.cs` + `Bootstrap/GameBootstrap.cs` profile initializer | Add a `public Sprite Shape;` field to `UnitProfile` (mirrors the existing `public Color Color;`), set it per profile in `GameBootstrap` (macrophage → `SpriteShapes.Macrophage`, neutrophil → `SpriteShapes.Neutrophil`). Keeps `SearchUnit` class-agnostic. |
| b | `Units/SearchUnit.cs:154` | `sr.sprite = profile.Shape;` instead of `RuntimeSprites.SquareSprite`. Footprint still `profile.FootprintFineTiles`. |
| c | `Pathogens/PathogenAgent.cs:377` (`EnsureSprite`) and `:959` (`ApplyRestColorForCurrentClass`) | Pick sprite by `Class` (`LargeBacterium` → rod, `IntracellularVirus` free → virion dot) and keep `sr.enabled = !IsIntracellular` untouched. The 3.5-tile scale line is untouched. If adopting the colder virion hue, add the colour branch here too. |
| d | `Pathogens/PathogenSpawner.cs:319` (`EnsureFoodVisual`) | `sr.sprite = SpriteShapes.FoodBolus;`. Fully isolated. |
| e | `Adaptive/DendriticCell.cs:98` | `sr.sprite = SpriteShapes.DendriteStar;`. Optional: in `SimulationTick`, `sr.sprite = HasCargo ? SpriteShapes.DendriteStarLoaded : SpriteShapes.DendriteStar;` next to the existing `sr.color` line. |
| f | `Adaptive/Lymphocyte.cs:69` | `sr.sprite = SpriteShapes.Lymphocyte;`. Node scaling untouched. |
| g | `Units/BoneMarrowManager.cs:192` | `sr.sprite = SpriteShapes.SlotNiche;`. Optional: on placement, also set the placed unit's shape sprite. |
| h | `Rendering/GutInterfaceRenderer.cs:60` | `sr.sprite = SpriteShapes.EpithelialBar;`. `Refresh()` scale/colour maths untouched. |
| i | `Bootstrap/GameBootstrap.cs:338` (`BuildBoardVisual`) + `Rendering/BoardRenderer.cs` | Pass three shared sprites (`HostCell`, `Debris`, `EmptyPit`) into `BoardRenderer.Bind`, or have `BoardRenderer` pull them from `SpriteShapes`. In `Refresh()` (`:127`), set `views[col,row].sprite` alongside the colour it already computes: infected/healthy → `HostCell`, dead → `Debris`, empty → `EmptyPit`. Biggest change, still local to one method + one bind call. `HostCellInfected` vs `HostCell` can be one sprite (hue does the split) or two if the inclusion/purulent texture split is wanted (open question 4). |
| j | `Bootstrap/GameBootstrap.cs:361, 377` (marrow / lymph backdrops) | Swap to `SpriteShapes.MarrowRegion` / `SpriteShapes.LymphNodeBean`. If the node becomes a bean shape, only the sprite changes — the `localScale` from `layout.LymphSize` still bounds it. |
| k | `Rendering/DegranulationFlash.cs:124` (`Begin`) | Pick the shape sprite from `burstColor` via an internal `switch` over the five `static readonly` colours. **All five `Play(...)` call sites unchanged.** Optionally also vary `DurationSeconds` / `StartScale` / `EndScale` per shape (they are currently `const` — make them instance fields set in `Begin`). |

### 5.2 — Verify

- No headless harness covers rendering (`Update()` does not run in
  batchmode — `AGENT_HANDBOOK.md`), so this is a **build-launch
  screenshot** check, same class as the Sprint 2 intracellular-render bug.
- Suggest a scratch debug key that spawns one of every entity in every
  state and fires all five flashes, for a single-frame visual QA capture
  (`PrintWindow` + `PW_RENDERFULLCONTENT` per the handbook for an
  unfocused window).
- Checklist: (1) sorting still back-to-front per the table; (2) DC
  cargo / lymphocyte paired / pathogen contact-flash / infected-cell +
  cytokine-heat all still visibly change; (3) footprints unchanged
  (macrophage still visibly bigger than neutrophil; food still 1.4×);
  (4) intracellular infection still shows *only* as the host-cell
  background.

### 5.3 — Order of commits

`SpriteShapes.cs` + `.meta` first (5.0), then 5.1 a–k roughly in that
order — units and pathogens first (most watched), board grid (i) and
flashes (k) last since they are the widest touch. Each row is one commit
with a reasoning-heavy message per `AGENT_HANDBOOK.md`.

---

## 6. Open questions for the Director

1. **Palette vs. `handoff` §8.** Two playtested placeholder colours sit
   near things §8 says to avoid: the **cytokine-heat orange**
   (`1.00, 0.55, 0.05`, close to "hazard-orange") and the **knowledge /
   efferocytosis greens** (close to "glowing green"). Keep them (they are
   legible and load-bearing) or re-hue toward the histology palette —
   e.g. heat → deep magenta-red, knowledge → gold or violet?
2. **Neutrophil amber** (`0.95, 0.78, 0.25`) is close to hazard-orange.
   Nudge to a gold (`0.93, 0.74, 0.30`) or leave it (distinct from
   everything, playtested)?
3. **How much per-instance variation?** None (identical cells → a clean
   clinical histology-plate look) or subtle rotation + size + hue jitter
   (more organic, less "stamped")? Recommendation: subtle.
4. **Viral vs. bacterial infection** — is the violet / yellow-green **hue
   split enough** at 28 px, or is the texture split (swollen inclusion
   body vs. purulent stipple) worth the second sprite?
5. **Lymph node and bone marrow** — become non-rectangular (a bean, a
   region silhouette) or stay labelled rectangles for now?
6. **Flash shapes** — adopt the five distinct silhouettes (recommended,
   colour-blind-safe) or keep all five as the expanding square and rely
   on colour + location?
7. **Concurrent-flash cap** (`GAME_DESIGN.md` §8) — implement a hard
   ceiling now, or defer?
8. **Free virion colour** — split it to a colder purple-maroon so "virus"
   and "bacterium" differ at a glance, or keep both on `PathogenColor`
   and let the dot-vs-rod silhouette carry it?
