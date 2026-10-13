# Art Direction — "Daylight Cartoon"

Status: **proposal, 2026-10-13.** Nothing here is implemented. It answers
the Director's brief: the look should be *accurate, but simple, fun and
bright, with mass appeal.* It builds on `LUMEN_REFERENCE.md`, which
already surveyed the biology diagrams, and doesn't repeat that survey. It
amends `UI_STYLE_GUIDE.md`, `SPRITE_DESIGN.md` §1 and
`COMPARTMENT_DESIGN.md` §1 only if the Director accepts it.

**Headline.** The current direction is "a histology plate at a glance":
dark ground, desaturated, clinical. It is the opposite of what every
mass-appeal reference does, and it is also the main reason the
pathogens are hard to see. This doc recommends a **substantial shift,
not a nudge**:

- **Light, low-saturation grounds.**
- **Saturated, mid-value figures**, each with a **dark outline**.
- **Threats get the loudest colours on the board.**

The biology is unaffected. Every textbook cartoon of the gut wall
(Servier, OpenStax, BioRender-style) is already drawn this way: on
white, with outlined, flat-coloured cells.

---

## 1. What the reference games (and diagrams) do

Viewed this pass: gameplay screenshots of Bloons TD 6, Kingdom Rush,
Plants vs. Zombies and Osmos, and the cover of *Cells at Work!* vol. 1
(all from Wikipedia file pages; non-free, viewed for research only), plus
the science cartoons S3, S4, S5 and S10 from `LUMEN_REFERENCE.md`. Cell
to Singularity and Cytus were **not** viewed, so no claims are made
about them. Developer art-direction interviews for BTD6, KR and PvZ
don't seem to exist publicly. The closest primary source on readability
is Valve's (V below).

Sources:
- **[BTD]** [Bloons TD 6 gameplay](https://en.wikipedia.org/wiki/File:Bloons_TD6_Gameplay_Screenshot.png).
  Wikipedia's [article](https://en.wikipedia.org/wiki/Bloons_TD_6)
  records that its "cartoonish visuals... were praised for [their]
  appeal for a wider audience". The
  [Family Gaming Database](https://www.familygamingdatabase.com/accessibility/Bloons+TD+6)
  rates it "generally well contrasting and bright".
- **[KR]** [Kingdom Rush gameplay](https://en.wikipedia.org/wiki/File:Kingdom_Rush_Gameplay.jpg).
- **[PvZ]** [Plants vs. Zombies gameplay](https://en.wikipedia.org/wiki/File:Plants_vs_Zombies_Gameplay.png).
- **[OSM]** [Osmos screenshot](https://en.wikipedia.org/wiki/File:Osmos4a.PNG).
- **[CAW]** [*Cells at Work!* vol. 1 cover](https://en.wikipedia.org/wiki/File:Cells_at_Work_v1_cover.jpg).
- **[V]** ["How Valve makes art to enhance gameplay"](https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay)
  (TF2 / Left 4 Dead).
- **[ID]** [Immune Defense](https://www.molecularjig.com/?p=742), Molecular Jig.
  This is the nearest biology tower defense. Third-party listings call
  it "bright, cartoon-style". No screenshots were reviewed.

**The principles, each one transferable to this game:**

| # | Principle | Where it is seen | For us |
|---|---|---|---|
| 1 | **Light or mid-value ground, saturated figures** | BTD's mid-green grass, KR's painted earth, PvZ's bright lawn; the science cartoons (S4 Servier, S10) put pastel tissue on **white**. Osmos is the one dark-ground reference: a minimalist ambient game with a handful of objects, a different job from 200 agents | the core shift, §3.1 |
| 2 | **A dark outline on everything interactive** | KR, BTD and PvZ ink every unit and enemy; Servier and BioRender ink every cell | lets a *yellow* unit read on a *pale* ground by moving separation from hue onto value |
| 3 | **Separate the route from the battlefield by temperature** | KR's blue river and tan road against green; BTD's pale track; PvZ's blue pool lane | a cool lumen against warm tissue |
| 4 | **The loudest saturation goes to what matters** | [V]: Left 4 Dead pushes everything below a saturation threshold "towards gray" so health packs and exits stand out. [OSM]: red motes are danger, blue are safe | pathogens are today the *darkest, least saturated* agents, the inverse |
| 5 | **Role-coded "costume" colour** | [CAW]: the red blood cell in red, the neutrophil all in white. [V]: a whole palette per team | keep one hue per unit kind |
| 6 | **Silhouette first; small things drawn larger than life** | [V]: silhouettes are "a building block of character design". PvZ plants differ by outline. BTD's monkeys are huge next to the track; every cartoon virus is a big spiky ball | a pathogen size floor (§3.3) |
| 7 | **Low-frequency ground, high-frequency figures** | KR's ground is broad soft strokes and the crisp edges belong to units. [V]: detail goes "where players look" | remove the stipple from the fields |
| 8 | **Show the grid and lanes cheaply** | PvZ's two-tone checker lawn | optional ±3% row tone |
| 9 | **Cheerful, not gory** | BTD balloons pop; PvZ zombies lose their heads comically; [CAW] bacteria are cartoon villains | damage = the bright field **darkening and scarring**, not blood |

## 2. Honest critique of the current look

Evidence: `/tmp/claude-1000/artpass/mock_compare*.png` and
`units_true_scale.png` (§5). These are tinted `_spritedump` sprites laid
out per the sorting table. They are rough, but the colour relationships
are exact.

**What reads well:**
- **Host-cell states.** The violet viral patch is the clearest thing on
  the board.
- **Unit silhouettes.** The macrophage blob, the neutrophil disc and the
  DC star are distinct.
- **The villus fringe.** It reads, and the Peyer's gap reads as a
  landmark.
- **The bolus** is large and lumpy, and against the plum lumen it is
  unmistakable.

**What is muddy (numbers are WCAG luminance contrast, fill against ground):**

| Pair | Contrast | Read |
|---|---|---|
| Bacterium on lumen | **1.13 : 1** | Effectively invisible. It differs by hue only, and that hue is darker plum. |
| Virion on lumen | **1.04 : 1** | Invisible. On infected (violet) tissue it is 2.0 : 1. |
| Neutrophil on tissue | 1.32 : 1 | Survives on saturation alone. |
| Bolus on tissue | 1.85 : 1 | Fine in the lumen. It is the only one of these that is fine. |

- **The rims don't exist at play scale.**
  - `RimShade` draws 1–2 px at 64 px, which is **0.3–0.6 px** on a
    14–22 px sprite.
  - The rim is also only ×0.55–0.80 of the tint, so it is a slightly
    darker shade, not a line.
  - So "a thin dark membrane rim for figure-ground separation"
    (`SPRITE_DESIGN.md` §1) is not happening.
- **Pathogens are under the size floor.**
  - The `Virion` dot fills about ⅓ of its texture: **~5 px on screen**
    at the 100×40 scale.
  - `LargeBacterium` is a thin capsule of **~11×3.5 px**.
  - The threats are the smallest *and* darkest agents.
- **The whole board reads "bruise".**
  - The grounds are oxblood plasma (L 0.03), plum lumen (L 0.05) and a
    near-black clear colour, with dark seams between the tiles.
  - Even the tissue is a dusky pink at value 0.80, saturation 0.23.
  - The mood is clinical or forensic. [V]'s point about dark sides
    "getting lost into the background" applies to the whole board here.
  - Mass-appeal TDs are daylight scenes.
- **Losses read as holes in the render, not as damage.** `EmptyPit` is
  near-black and mostly transparent. Debris is grey-brown on black.
- **Stipple everywhere.** `ChymeField`, `PlasmaField`, `MarrowRegion`,
  `HostCellInfectedBacterial` and `FoodBolus` all use alpha or value
  speckle. At 28 px per cell it becomes grain, the opposite of
  principle 7.
- **It fights the mass-appeal brief.** The direction was written for
  "histology plate". That was a coherent choice, but it is not the
  current brief, and a nudge won't get there. Lightening the plum to a
  lighter plum keeps the bruise.

## 3. Proposed direction

### 3.1 The rules

**Option chosen: a light ground with saturated, outlined figures.** The
alternative was a dark ground with neon figures (the Osmos / Geometry
Wars register). It was rejected for four reasons:

- It keeps the clinical-night mood.
- Neon on dark relies on glow, and glow means extra renderers or
  additive passes.
- None of the mass-appeal TDs use it.
- The science cartoons we lean on for accuracy are all on white.

**Value structure. Every sprite must obey this:**

| Layer | Value (HSV V) | Saturation | Edge |
|---|---|---|---|
| Grounds (tissue, lumen) | 0.90–1.00 | ≤ 0.20 | soft, no stipple |
| Home band (blood) | ~0.90, darker than tissue in luminance (L≈0.35) | ~0.45 | soft |
| Structures (villi, wall, organs, slots) | 0.85–1.00 | 0.20–0.50 | 2–3 px dark outline |
| Immune units | 0.70–1.00 | ≥ 0.75 | **≥ 1.5 px dark outline on screen** |
| Pathogens + bolus | 0.65–0.95 | **≥ 0.70, the loudest on the board** | ≥ 1.5 px dark outline, the thickest |
| Damage (debris, pit) | **darker** than the ground | low | — |

**Winning stays bright and losing goes dark.** That is the whole board's
emotional read, and it needs no gore.

**The outline mechanism costs zero renderers.**
- Add an **`OuterStroke(buf, px, value≈0.18)`** primitive to
  `SpriteShapes`. It dilates the alpha mask outward by `px` and fills
  the ring at value 0.18.
- The multiply tint then produces a **dark line in the unit's own hue**
  (e.g. blue → `0.04,0.09,0.17`), which is the coloured-lineart
  convention.
- Stroke width at 64 px: **5 px** for units drawn at 18 px or more on
  screen, and **6 px** for anything drawn under 16 px (pathogens,
  neutrophil).
- Shapes that nearly fill their texture (`Macrophage`, `LargeBacterium`,
  `FoodBolus`) must be drawn ~15% smaller inside the 64 px, with the
  visual scale raised to match.
- After the change, the outline gives 12–15 : 1 against every ground,
  so the fill only has to separate by hue and saturation.

**Fields lose their stipple.** `ChymeField`, `PlasmaField`,
`MarrowRegion` (bigger, cleaner pores) and the bacterial-infected cell's
interior (stipple contrast ×0.6 → ×0.85) all change.

### 3.2 Palette — current against proposed (0–1 sRGB)

Every element in `UI_STYLE_GUIDE.md`. **Bold** = a hue-family change
rather than a value or saturation shift. "+S" = gains `OuterStroke`.

| Element | Current | Proposed | Note |
|---|---|---|---|
| Camera clear | `0.05,0.05,0.07` | `0.18,0.13,0.17` | warm dark frame around a lit board |
| *Tissue underlay* (new quad, order −1) | — (clear shows in seams) | `0.93,0.70,0.70` | seams read as pink mortar, not black lines; +1 renderer |
| Host — healthy | `0.80,0.62,0.66` | `0.99,0.86,0.82` | pale peach cobblestone |
| Host — infected viral | `0.54,0.36,0.60` | `0.80,0.66,0.95` | lilac; same family as the virion, on purpose |
| Host — infected bacterial | `0.62,0.60,0.26` | `0.88,0.96,0.50` | pale lime, same family as the new green bacterium; soften stipple |
| Host — dead (`Debris`) | `0.38,0.34,0.28` | `0.66,0.58,0.60` +S thin | grey-mauve. Drops brown, so brown means food board-wide |
| Host — empty (`EmptyPit`) | `0.13,0.11,0.12` | `0.42,0.26,0.32` **opaque** | a dark rose scar, not a hole in the render |
| Macrophage | `0.30,0.40,0.80` | `0.22,0.48,0.96` +S | brighter royal blue |
| Neutrophil | `0.93,0.74,0.30` | `1.00,0.80,0.12` +S(6) | saturated gold; the outline carries it on peach (fill 1.17 : 1) |
| Dendritic cell | `0.72,0.30,0.68` | `0.92,0.22,0.72` +S | hot magenta |
| DC carrying antigen | `0.98,0.62,0.98` | body stays `0.92,0.22,0.72`; **antigen dot in the pathogen's colour** | lighter-is-louder fails on a light ground. See §4 #8 |
| Helper T | `0.32,0.72,0.70` | `0.10,0.74,0.68` +S | |
| Helper T paired | `0.82,0.94,0.92` | `0.45,0.95,0.85` | pale → bright mint, which keeps it visible on the pale node |
| **Large bacterium** | `0.42,0.12,0.16` maroon | **`0.36,0.76,0.12` germ green** +S(6) | **Director decision D2** |
| Free virion | `0.40,0.16,0.34` | `0.58,0.20,0.92` electric violet +S(6) | new corona silhouette, §3.3 |
| Food bolus | `0.55,0.47,0.28` | `0.64,0.44,0.20` +S | richer ochre-brown; still the only brown |
| Marrow backdrop | `0.34,0.22,0.18` | `1.00,0.88,0.80` | bone-cream sponge, declutter |
| Marrow slot (empty) | `0.62,0.56,0.42` | `0.80,0.85,0.93` +S | cool "build pad", like KR's plots |
| Lymph-node backdrop | `0.34,0.40,0.28` | `0.82,0.92,0.76` +S(2) | pale mint island |
| Gut-wall bar, quiet | `0.50,0.46,0.37` | `0.98,0.68,0.60` | epithelium coral |
| Gut-wall bar, alarm | `0.95,0.30,0.20` | unchanged | pops harder on light |
| Chyme field | `0.36,0.22,0.24` | **`0.80,0.91,0.95` pale aqua** | **D3**. Flat; ≤10% deepening mid-stream |
| Villus (front) | `0.80,0.50,0.47` | `1.00,0.62,0.57` +S(3) | saturated coral |
| Villus (back row) | `0.62,0.40,0.42` @0.70 | `0.98,0.78,0.75` @1.0 | **lighter** than the front: atmospheric depth on light (S3) |
| Peyer's dome | `0.86,0.58,0.55` | `1.00,0.70,0.64` +S(2) | |
| M cell | `0.78,0.74,0.82` | `0.88,0.82,0.98` | "unbought" = **α 0.5**, not ×0.55 darkening |
| Follicle haze | `0.32,0.72,0.70` @0.30 | `0.10,0.74,0.68` @0.25 | |
| Crypt pocket / Paneth | `0.86,0.68,0.70` / `0.95,0.60,0.66` | `0.96,0.74,0.74` +S / `1.00,0.45,0.55` | |
| Mucus sheen | `0.88,0.84,0.76` @0.20 | `1.00,1.00,1.00` @0.35 | a white glaze |
| Flow motes | `0.60,0.52,0.40` | `0.55,0.74,0.82` @0.7 | darker aqua specks; no cream or brown |
| Base plasma | `0.33,0.11,0.15` | `0.94,0.50,0.52` | **D6**. Rose; the darkest band by luminance, flat |
| Vessel wall cell | `0.78,0.50,0.52` | `1.00,0.80,0.78` +S(3) | |
| Organ halo | `0.20,0.06,0.09` | `0.60,0.20,0.26` @0.6 | a soft drop shadow, not a black ring |
| Erythrocyte | `0.72,0.20,0.22` @0.9 | `0.86,0.12,0.18` @1.0 +S(4) | |
| Birth puff | `0.70,0.62,0.50` | `1.00,1.00,1.00` @0.8 | |
| Base breach flash | `0.86,0.10,0.12` | `0.80,0.04,0.10` | darker than rose plasma, so it reads |
| Node co-loc haze | `0.55,0.85,0.85` | `0.20,0.80,0.80` ≤35% | must be saturated to show on mint |
| Cytokine heat tint | `1.00,0.55,0.05` ≤65% | unchanged | reads *better* on peach |
| FX: degranulation | `1.00,0.97,0.72` | `1.00,0.78,0.10` | white-ish vanishes on light |
| FX: breach starburst | `1.00,0.35,0.22` | `1.00,0.30,0.15` | |
| FX: efferocytosis | `0.45,0.80,0.68` | `0.55,0.95,0.85` | mint "clean" bloom |
| FX: stress-sense | `0.95,0.40,0.80` | `1.00,0.25,0.70` | |
| FX: knowledge match | `0.40,0.92,0.45` green | `0.15,0.80,1.00` sky cyan | frees green for bacteria |
| Pathogen hit flash | `0.95,0.85,0.30` | unchanged | |
| UI tokens (`UiTheme`) | dark glass | **unchanged for now** | dark panels over a bright board are the BTD/KR norm. Re-test `Accent` as the board's selection rim |

**Downstream conflicts of the hue moves:**
- **Green bacterium.**
  - The knowledge flash (fixed above).
  - The efferocytosis blue-green, which moves to mint and is separated
    by shape and timing anyway.
  - The bacterial-infected tint, which becomes intentionally the same
    family.
  - The lymph node's pale mint. Bacteria don't sit on it, and the
    saturation differs by 0.6.
  - `LivesOk` sage in the HUD, which is UI only.
- **Violet virion.**
  - The DC magenta. The hue gap is about 40°, and the silhouettes differ:
    the DC has long thin arms and is larger, while the virion is a
    compact knobbed ball.
  - The stress-sense flash, which differs by shape.
- **Pale aqua lumen against macrophage blue.** Macrophages never enter
  the lumen, and the two differ by 0.6 in saturation.
- **`DegranulationFlash.ShapeFor` picks the flash shape by exact colour
  equality (`Same()`).** The five flash colours must stay pairwise
  distinct and must be changed *at the constants*, never inline.
- **Colour space.** The project is in Gamma colour space
  (`m_ActiveColorSpace: 0`), so these sRGB values land as written, as
  they do in the mock.

### 3.3 Shape language

| Agent | Shape | Note |
|---|---|---|
| Macrophage | Big soft ruffled amoeba, the largest unit | "The eater". Keep the lumpy outline; the stroke makes the ruffles read. |
| Neutrophil | Small round disc, trefoil nucleus | At 12 px the nucleus already reads as a tiny face. Keep it subtle, not a face. |
| Dendritic cell | Thin star, long arms | The only long-armed shape. Never give the virion long spikes. |
| Helper T | Round, nucleus-heavy | Node only. |
| Bacterium | **Chunky 2:1 rounded bean** (was a 4:1 rod) | Visual size **0.55 cells**. Drop flagella: they vanish under 20 px. |
| Virion | **Knobbed corona ball**: a disc plus 8 short knobbed spokes | Visual size **0.6 cells** (sim footprint unchanged). It is drawn larger than life, the cartoon-virus convention (principle 6). |
| Food bolus | Lumpy, opaque, brown | Already right; add the stroke. |
| Host cell | Soft cobblestone | Fill colour carries state; interior detail is optional. |

Personality comes from motion, not faces. Options are a tiny idle
"breathing" scale wobble on units and a jiggle on pathogens, both as
transform writes in the existing update loops with no allocation.
**Faces and eyes** (the CAW / PvZ route) are left as Director decision
**D4**. The recommendation is **no**, because it costs accuracy and adds
interior detail that doesn't resolve at 14–22 px.

### 3.4 How each compartment should read

- **Tissue: "the bright field you defend".** The lightest, calmest
  warm area. Infection shows as colour patches (lilac, lime). Loss
  shows as darkening (grey debris, dark-rose scars).
  - Optional PvZ lane cue: alternate rows ±3% value in
    `BoardRenderer.CellJitter`.
- **Lumen: "a cool river".** Pale aqua, separated from the tissue by
  temperature, by the coral villus fringe and by the wall bar.
  - Pathogens and the bolus are the only saturated, outlined things in
    it. Everything else is pale and soft.
  - Commensals (`LUMEN_REFERENCE` P7), if ever built, should be pale
    sage at low α, unoutlined.
- **Base: "your warm blood, with two islands".** Rose plasma is the
  deepest band, which frames home.
  - Marrow (bone-cream) and node (mint) sit in it as outlined islands
    with soft shadows.
  - Slots are cool steel pads: the build zone, as in KR.
  - Red outlined erythrocytes stream past.
- **Lymph node.** A pale mint bean with lighter follicles. Teal T cells
  and magenta DCs pop on it; the co-loc haze is a saturated cyan.
- **Gut wall.** Coral epithelium with an outline, continuous with the
  villi. Its alarm red is now the most alarming thing on the board,
  which is correct.

## 4. Prioritised changes (payoff ÷ effort)

| # | Change | Touches | Effort |
|---|---|---|---|
| 0 | **Centralise the palette** in one `Rendering/Palette.cs`, then add a debug key (beside F9) that swaps *Classic ↔ Daylight* so the Director can A/B in a build. Colours are currently duplicated (DC magenta is in `DendriticCell.cs` *and* `BoneMarrowManager.ColorForKind`; teal is in 3 files). A swap applies at spawn or refresh, with no per-frame cost. | `GameBootstrap` (unit profiles, backdrops), `BoardRenderer`, `DendriticCell`, `Lymphocyte`, `BoneMarrowManager`, `GutWallFeatureRenderer`, `LumenChannelRenderer`, `BaseCompartmentRenderer`, `DegranulationFlash`, `PathogenSpawner` | low–med; makes everything below cheap and reversible |
| 1 | **Lumen + pathogens recolour.** Aqua chyme, white mucus, aqua motes, coral villi, green bacteria, violet virions. **The biggest single legibility win:** bacterium-on-lumen goes 1.13 → 1.8 : 1 on fill, and ~13 : 1 with the outline. | `LumenChannelRenderer`, `BoardRenderer.PathogenColor/VirionColor`, `SpriteShapes.ChymeField` (destipple) | low |
| 2 | **`OuterStroke` on every agent and structure** marked +S in §3.2, with the shapes shrunk inside the 64 px | `SpriteShapes` (new primitive; `Macrophage`, `Neutrophil`, `DendriteStar(Loaded)`, `Lymphocyte`, `LargeBacterium`, `Virion`, `FoodBolus`, `Erythrocyte`, `Villus/VillusAlt`, `EndothelialCell`, `SlotNiche`, `LymphNodeBean`, `Crypt`, `PeyerDome`, `Debris`) | low–med, texture-only |
| 3 | **Pathogen silhouettes and size floor.** Corona virion and chunky bacterium; per-class *visual* scale (0.6 / 0.55 cells) | `SpriteShapes.Virion/LargeBacterium`, `PathogenAgent.EnsureSprite` | low |
| 4 | **Tissue to daylight.** New host-state colours, an underlay quad (+1 renderer, order −1), `EmptyPit` opaque, softer bacterial stipple, grey debris | `BoardRenderer.HostStateColor`, `SpriteShapes.EmptyPit/HostCellInfectedBacterial`, new quad in `BoardRenderer` or `GameBootstrap` | low |
| 5 | **Unit tints** (trivial after #0) | unit profiles, `DendriticCell`, `Lymphocyte`, `BoneMarrowManager` | trivial |
| 6 | **Base band.** Rose plasma (destippled), red outlined erythrocytes, endothelium, halos as shadows, decluttered marrow, mint node, steel slots, and a re-tested selection rim | `BaseCompartmentRenderer`, `SpriteShapes.PlasmaField/MarrowRegion`, `GameBootstrap`, `BoneMarrowManager` | low–med |
| 7 | **Effect colours** (kept pairwise distinct for `Same()`) | `DegranulationFlash` | trivial |
| 8 | **DC antigen dot.** A child renderer, tinted the colour of the pathogen whose antigen the DC carries, enabled only while loaded. It shows *what* the DC carries: a teaching win. | `DendriticCell` (+1 renderer per DC, pooled with it) | medium |
| 9 | Polish: lane-tone rows; idle wobble or jiggle; camera clear colour | `BoardRenderer`, `SearchUnit`, `PathogenAgent`, `GameBootstrap` | low |
| 10 | UI re-check against the light board. Probably a no-op, apart from `Accent` on the board | `UiTheme` | later |

**Sequencing.** Do #0 first. Then land #1 to #4 together as one sprint,
because the outline (#2) is what makes the light grounds (#1, #4) work.
A light ground without outlines would make the neutrophil (fill 1.17 : 1
on peach) *worse*. Verify each step with `SpriteShapeDump` and
`tools/unity.sh verify`. As always, only the Director's eye can sign it
off.

**Director decisions needed:**
- **D1. Retire "histology plate" for "daylight cartoon".** This is the
  headline. It rewrites `SPRITE_DESIGN.md` §1, `COMPARTMENT_DESIGN.md`
  §1 and the `UI_STYLE_GUIDE.md` direction.
- **D2. Bacteria maroon → green.** This is the germ convention, and
  textbook rods are green (S10, S12). The conflicts are listed in §3.2.
  The fallback is a saturated tomato red, but that collides with blood,
  erythrocytes and the breach red.
- **D3. Lumen pale aqua.** This is the fluid convention, and it reads as
  a river. The more literal alternative is a textbook white or cream
  lumen. It also works by value, but it separates less from the peach
  tissue.
- **D4. No faces.** Recommended: no.
- **D5. Accept larger-than-life pathogen visuals** (the size floor,
  #3).
- **D6. Accept a rose blood band** in place of oxblood. The sepsis
  framing survives, because it is still red and the darkest band.

## 5. Mock images

The two comparison mocks are committed under `docs/art/`; they contain only
this project's own sprites. The rest stayed in scratch (`/tmp/claude-1000/artpass/`)
and the reference sheets are deliberately not committed (non-free images). Scripts: `mock.py
<current|proposed> <px/cell> <out>` and `lineup.py`.

- `mock_compare.png`: the shipping 25×10 board at 48 px/cell. **Top:**
  the current palette. **Bottom:** the proposed daylight palette with
  outlines, recoloured pathogens and the tissue underlay.
- `mock_compare_28px.png`: the same pair at 28 px/cell, the 100×40
  target scale. The current lumen pathogens nearly vanish, while the
  proposed ones are the first thing you see.
- `units_true_scale.png`: the six agents at true on-screen size (12–39
  px), shown ×3 nearest, on tissue, lumen and infected grounds. Top
  three rows are current; bottom three are proposed.
- `true_scale_current.png`: the first diagnostic, current sprites only.
- `refs_sheet.png`: BTD6, CAW, KR, Osmos and PvZ side by side (research
  only; non-free).
- `sci_sheet.png`: S10, S4, S5 and S3 from `LUMEN_REFERENCE.md`. All are
  on white with outlined cells.

**Caveats on the mocks:**
- Positions are approximate.
- The mock assumes the clear colour shows between host tiles, as the
  rounded `HostCell` alpha implies.
- PIL Lanczos stands in for Unity's bilinear filtering.
- The proposed `EmptyPit` in the mock is still the old mostly-transparent
  sprite, so it reads weaker than the opaque version proposed in §3.2.
