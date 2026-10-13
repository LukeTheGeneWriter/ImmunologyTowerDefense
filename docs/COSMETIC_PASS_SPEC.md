# Cosmetic Pass Spec: organs, shop, board frame, knowledge HUD

Status: **proposal, 2026-10-14. Daylight only** (Classic is being retired).
Builds on `ART_DIRECTION.md`. If accepted, amends `UI_DESIGN.md` §1, §2, §6
and the HUD part of `UI_STYLE_GUIDE.md`. Sizes are at the **1920×1080
reference** unless marked "world" or "tex px". Colours are sRGB 0–1, and new
ones go in `Palette.cs` / `UiTheme.cs`, never inline.

**Mocks** (scratch, not committed; each regenerates with `python3 -I <script>`):
- `docs/art/cosmetic_full_screen.png` (`screen.py`): the whole buy
  phase. Plate frame, band tabs, HUD, knowledge strip, shop tray, a "+3%"
  token in flight, a tooltip.
- `docs/art/cosmetic_organs_closeup.png` (`organs_closeup.py`): the
  base band at 2×, current vs proposed.
- `docs/art/cosmetic_ui_states.png` (`states.py`): expanded
  knowledge card, unlock ribbon, match token, shop tile states, picker,
  upgrade panel, in-round HUD, frame corner.

`organs.py` and `icons.py` are PIL ports of the construction steps below,
so the mocks are the spec.

---

## 0. The fact everything hangs on

At 16:9 the 2.5:1 board (28 × 11.2 world) draws **1846 × 738 px**. That
leaves **171 px of empty frame above and below** and 37 px at the sides.
Today the HUD and shop sit *on* the board, over the lumen.

**Proposal:** all permanent UI moves into the two gutters, so nothing
permanent covers the board. Only on-demand surfaces float over it: the
picker, the upgrade panel, the expanded knowledge card and tooltips.

`GameBootstrap.FitCamera` must keep each gutter at ≥ 15% of the visible
height, because 21:9 leaves only 5% today:
`halfHeight = max(h*0.5*1.04, h*0.5/(1-2*0.15), w*0.5*1.04/aspect)`.
This is a no-op at 16:9, where the gutter is already 15.8%.

---

## 1. Bone marrow and lymph node

### 1.1 Shared mechanism

The organs are ~214×453 px (marrow) and ~363×221 px (node) on screen. A
64 px square stretched to that size blurs, and its outline goes uneven.

- **New `Rendering/OrganShapes.cs`** with a `Canvas(w,h)` raster. It ports
  `FillRounded` (rect), `FillEllipse`, `ClearEllipse`, `FillCapsule`,
  `OuterStroke` and `ToSprite`. White RGB, value detail, silhouette in
  alpha, as now. `SpriteShapes` is untouched.
- **Canvas sizes:** marrow 192 × round(192·h/w); node 320 × 200 (the body
  plus vessel padding). PPU = tex width / world width.
- **Cost:** ~1.3 MB, built once at boot. One hue per renderer. These are
  static single instances, so extra renderers are free.
- **The halo is retired.** Each organ gets a cartoon drop shadow instead:
  its base silhouette, offset (+0.06, −0.09) world, tinted `OrganShadow`,
  at order 1. This is the same shadow language as the UI cards.

New `Palette` entries:

| Name | RGB | Note |
|---|---|---|
| `Bone` | `1.00,0.94,0.80` | cream, the textbook spongy-bone colour |
| `MarrowSpace` | `1.00,0.62,0.52` | salmon; warmer and lighter than plasma `0.94,0.50,0.52` |
| `NodeBody` | `0.80,0.93,0.78` | replaces `NodeBackdrop` |
| `Follicle` | `1.00,0.84,0.62` | apricot B-cell zones |
| `Lymph` | `1.00,0.95,0.70` | butter-yellow lymphatics |
| `OrganShadow` | `0.50,0.12,0.20 @0.40` | drop shadows |
| `PadSteel` | `0.80,0.85,0.93` | today's `MarrowSlotEmpty`, renamed |
| `FrameInk` | `0.10,0.07,0.09` | plate outline (§3) |

### 1.2 Bone marrow: spongy bone with build pads

**What the player should see:** a cream block of spongy bone, its rounded
holes full of red marrow, with five steel build pads set into it, like
Kingdom Rush plots.

Reference: [Bone_cross-section.svg](https://commons.wikimedia.org/wiki/File:Bone_cross-section.svg)
(cream struts, salmon marrow; CC BY 3.0).

**Layout.** Slot positions and size are unchanged.
- `MarrowBackdropSize.x = min(slotSize×2.9, baseRect.width×0.50)`, up from
  ×1.35 (≈3.2 world).
- `.y = strip + pitch×0.15`, down from 0.35. This opens ~0.2 world above
  the node.

| Order | Renderer | Construction (canvas W×H) | Tint |
|---|---|---|---|
| 1 | shadow | outer rounded rect (corner 0.30W), offset | `OrganShadow` |
| 2 | marrow space | Rounded rect inset `t=0.07W`, value 1.0. Haematopoietic dots: r 0.025W, value 0.84, 1 per 700 tex px², **only inside side holes**. | `MarrowSpace` |
| 3 | bone | 1. Outer rounded rect, value 1.0. 2. **Cut** one cavity per pad: half-size `slot/2+0.045W`, corner 50%. A salmon rim shows round each pad. 3. **Cut side holes** in each lane, from `t+0.02W` to `cavity−0.03W`. Stack every `2.6r + rand(0,0.05W)`. Each is an ellipse: r = half the lane, × rand(1.0,1.45), aspect rand(0.8,1.2), seed 3. Touching holes are fine. 4. A 2 px value-0.86 ring inside each hole edge. 5. `OuterStroke(0.022W ≈ 4 tex px, 0.18)`. This also inks the hole edges, which is what makes them read. | `Bone` |
| 5 | pad ×5 | New 64 px `SpriteShapes.BuildPad`: `FillRounded(30, corner 15)` at 1.0. Inset 0.12S at 0.88 (recess). Inset 0.16S, +2 px down, at 0.93 (lit lip). `Finish(b,4)`. | `PadSteel` |
| 6 | select ring | New `SelectRing`: rounded-square ring, half-extent 31, corner 18, width 6, `Finish(b,2)`. ×1.22 the pad. Breathes α 0.55–1.0 over 1.4 s. Replaces the slate `KnowledgeRing`, which vanishes on light. | white |
| 7 | glyph ×5 | **Empty:** new `PlusGlyph` (two capsules, width 9, `Finish(b,3)` at stroke value 0.30), ×0.55, α 0.9, ±3% bob at 1 Hz in the buy phase only. **Placed:** the kind's unit sprite at ×0.70, tinted `ColorForKind`. | white / kind |

**The pad stays steel when occupied.** Today `slot.Visual.color =
ColorForKind` paints the whole socket. A unit standing on a neutral pad is
the KR "tower built here" read. So `PlaceTower` swaps only the glyph
renderer, for +2 renderers per slot.

### 1.3 Lymph node: capsule, cortex, paracortex, medulla, lymphatics

**What the player should see:**
- a mint kidney with a dark capsule;
- apricot follicles with pale germinal centres around the convex cortex;
- the lightest central paracortex, where the T cells live;
- pale medullary cords fanning from a hilum notch on the **left**;
- three afferent lymphatics plugging into the **right** with inward
  chevrons, and one efferent leaving the hilum.

Reference: [Illu_lymph_node_structure.png](https://commons.wikimedia.org/wiki/File:Illu_lymph_node_structure.png)
(public domain, NCI SEER).

The orientation teaches: lymph enters on the tissue side, where DCs come
from.

**Fixes a current bug.** The helper-T arena is 86% of the backdrop
*rect*, but the bean is an ellipse. Its corners fall outside, so T cells
walk off the node today (`organs_closeup.png`, left). The new body is a
rounded rect with corner 0.30H, which contains the arena corner
(0.43W, 0.43H). No arena code changes.

Canvas = body W×H padded to 1.24W × 1.18H; the vessels live in the padding.

| Order | Renderer | Construction (W,H = body; origin = body top-left) | Tint |
|---|---|---|---|
| 1 | shadow | body silhouette, offset | `OrganShadow` |
| 1 | vessels | **Afferent ×3:** capsules of width 0.085H. Endpoints `(1.06W,.18H)→(.94W,.26H)`, `(1.07W,.50H)→(.95W,.50H)`, `(1.06W,.82H)→(.94W,.74H)`. **Efferent:** width 0.12H, `(.03W,.5H)→(−.10W,.52H)`. **Chevrons:** value 0.55 mid-vessel, pointing in on the afferents and out on the efferent. `OuterStroke(0.02H)`. Drawn under the body. | `Lymph` |
| 2 | body | 1. Rounded rect, corner 0.30H, value **0.93** (cortex). 2. Subtract the hilum ellipse at (0, 0.5H), radii (0.06W, 0.15H). 3. An inset-0.13H rounded rect at **1.0** (paracortex). 4. Three medullary cords: capsules of width 0.10H from (0.06W, 0.5H) at −35°/0°/+35°, length 0.30W (y×0.7), value **0.90**. 5. `OuterStroke(0.022H)` = the capsule. | `NodeBody` |
| 3 | follicles | 7 discs of r 0.095H at **0.91** (mantle). Each holds a germinal centre of r 0.055H at **1.0**, +0.012H down. They sit at (u,v) (.36,.11) (.60,.10) (.83,.17) (.92,.50) (.83,.83) (.60,.90) (.36,.89). Clip to the body; `OuterStroke(1, 0.45)`; re-clip. | `Follicle` |
| 4 | co-loc haze | The existing `LymphNodeFieldRenderer`, moved **3 → 4** so it glows over the follicles. | `NodeColocGlow` |

T cells (12) and DCs (13) are unchanged and read on mint (see the mocks).

### 1.4 Organ labels

`WorldLabelView` becomes a **tag**: `Card` fill, 2 px `Edge`, radius 8,
11 px bold `Ink` caps. "BONE MARROW" pins to the marrow's top edge and
"LYMPH NODE" to the node's bottom edge.

---

## 2. The shop (with the picker, upgrade panel and HUD)

### 2.1 Light cream cards on the dark frame, not dark glass

**Recommendation: go light.**

1. **Every reference does it.** PvZ has tan seed packets on a brown bar,
   KR has parchment on wood, BTD6 has bright tiles on a brown panel. Dark
   glass is the cockpit look we are leaving.
2. **One rule.** The board is a light plate on a dark desk (§3), and the UI
   becomes light cards on the same desk. Paper is light and inked; the desk
   is dark. Units already follow the same rule: fill plus a dark outline.
3. **Floating panels stop shouting.** A near-black 320×400 slab over a
   light board is the strongest value contrast on screen, louder than any
   pathogen. A cream card with a 3 px ink edge separates from the board
   without that.
4. **Legibility.** Dark ink on cream reads at 11 px. Dim taupe on glass
   over a light board does not.

**The cost:** gutter cards are bright against the dark desk. But they are
small, low-saturation and peripheral.

### 2.2 `UiTheme` tokens (they replace dark glass)

| Token | Value | Use |
|---|---|---|
| `Card` | `0.99,0.96,0.90` | panels |
| `Tile` / `TileDim` | `1,1,1` / `0.91,0.88,0.85` | affordable / unaffordable tile |
| `Edge` = `Ink` | `0.20,0.12,0.16` | 3 px outline; primary text |
| `InkDim` / `Rule` | `0.48,0.39,0.41` / `Ink@0.15` | labels / dividers |
| `Shadow` | `0,0,0 @0.40` | sibling element offset +5 px y (UITK has no box-shadow) |
| `Accent` / `AccentLip` | `0.13,0.58,0.88` / `0.08,0.30,0.48` | buttons, level badges / 3 px lip under a button |
| `Gold` / `ChipDim` | `1.00,0.78,0.22` / `0.84,0.80,0.78` | price chip, affordable / not; ATP bolt |
| `Bad` | `0.80,0.16,0.16` | unaffordable price, lives low, GAME OVER |
| `Heart` / `Hatch` | `0.90,0.22,0.32` / `0.55,0.50,0.52` | lives icon / the "planned" stripe (§4.4) |

**Shape.** Card radius 12 / border 3. Tile 10/3. Chip 10/2, 20 px tall.
Button 10/2 plus a 3 px `AccentLip` underlay, with white 13 px black-weight
text and a 1.5 px `Edge` text outline (`unityTextOutlineWidth/Color`).

**Type.** Numerals 30 px black-weight; names 13 bold; body 11–12; titles
12 bold caps `InkDim`. Spacing stays the 4 px unit.

**Symbols are sprites, never glyphs.** The OS fonts lack ▶ ↓ ♥ ✕; the mock
renderer hit exactly that. Today's `●○` dots and `✕` text are replaced
too.

### 2.3 Where things live

| Surface | Dock | Box |
|---|---|---|
| HUD | top-right gutter | right 24, top 14, 600×110 |
| Band tabs | plate top edge (§3.3) | 32 tall |
| **Shop tray** | **bottom-right gutter** (new `bottomRight` dock) | right 24, bottom 14, 896×140 |
| **Knowledge strip** | **bottom-left gutter** (§4) | left 24, bottom 14, 620×140 |
| Debug readout | moves to **top-left** (off by default) | — |
| Picker / upgrade | float at the slot, as now (Director decision 2) | 400 / 360 wide |

**The shop no longer collapses.** It covers nothing, so it is always there,
in and out of rounds; `SetExpanded` and `OnPhaseChanged` become no-ops.
This keeps Sprint 16's "buy mid-round" and removes its cost.

### 2.4 Icons: procedural, named

New 64 px sprites in `SpriteShapes`, `Finish(b,3)` unless noted:

| Sprite | Construction |
|---|---|
| `IconWaves` | disc r 9 at (17,32) + 3 arcs ±50°, r 16/25/34, width 5 |
| `IconDroplet` | disc r 16 at (32,38) ∪ triangle (18,32)(32,6)(46,32); 0.8 highlight arc |
| `IconBlock` | ring r 22, width 7, + diagonal bar width 7; `Finish(b,2)` |
| `IconBolt` | polygon (36,4)(12,36)(30,36)(24,60)(52,24)(34,24)(42,4) |
| `IconHeart` | discs r 13 at (21,25), (43,25) + triangle (10,30)(32,56)(54,30) |
| `IconStar` | 5 points, r 28/12 |
| `IconArrowDown` / `IconPlay` / `IconClose` | stem + head / triangle / crossed capsules |
| `HatchTile` | diagonal 2 px stripes every 6 px, α 0.8, tiled as `backgroundImage` |

**Composites** stack child `VisualElement`s (`backgroundImage` + tint) in a
56 px box; UI layers are free. A badge is ×0.55 at (+0.20, −0.20).

| Shop item | Layers (sprite → tint) | Real? |
|---|---|---|
| Cytokine sensing | `IconWaves` → `CytokineHot` (the board's inflammation colour) | yes |
| Mucus turnover | `IconDroplet` → `0.55,0.82,0.96` | planned |
| Host dsRNA sensor | `HostCellInfectedViral` → `HostInfectedViral`; badge `BreachStar` → `FxStressKill` | planned |
| Harden vs viral entry | `HostCell` → `HostHealthy`; badge `Virion`; `IconBlock` → `0.85,0.12,0.16` over it | planned |
| Bacterial resistance | as above, with badge `LargeBacterium` → `Bacterium` | planned |
| Crypt | `Crypt` → `CryptPocket`; `GranuleCluster` → `Paneth` ×0.40 at +0.18y | yes |
| M-cell sampling | `PeyerDome` → `PeyerDome` at +0.12y; `MCell` ×0.42 at −0.06y | yes |

Picker and upgrade portraits are the unit sprites in their unit colours,
in a 48 px `0.96,0.94,0.92` disc with a 2 px `Edge` ring.

### 2.5 Shop tray (`ShopView`, built once)

```
tray   Card row 896x140, pad 10 (+Shadow sibling)
├─ header  44 wide: "SHOP" 12px bold InkDim, style.rotate = -90
└─ tile ×7 112x120 Tile/TileDim, Edge 3, r 10 — the whole tile is the Button
   ├─ icon    56px composite, top 6; opacity 0.45 when unaffordable
   ├─ planned placeholders only: 12px HatchTile band at y 46 + centred "PLANNED" pill (9px)
   ├─ name    2 lines 11px bold: Cytokine/sensing · Mucus/turnover · dsRNA/sensor ·
   │          Block viral/entry · Bacterial/resistance · Crypt · M-cell/sampling
   ├─ chip    IconBolt 14 + price 13px black. Gold+Ink, or ChipDim+Bad with a 2px Ink@0.45
   │          bar along its bottom = balance/price ("how close am I")
   └─ badge   28px Accent circle top-right (−8,−8), white level digit; hidden at 0
```

The `Items` table gains a short name, an icon recipe and a `Real` flag.
`Real` replaces the `accentDot` condition.

**States:**
- **Hover:** `Accent` border, translate −4 px y, shadow shows.
- **Pressed:** scale 0.94.
- **Bought:** pulse 1→0.92→1.06→1 over 0.25 s.
- **Just became affordable:** the chip pops ×1.15 once. This is the "watch
  the number" cue.

**Tooltip.** One shared Card, 270 wide, above the hovered tile, with a
12 px pointer. It shows:
- the name and "· Lv N";
- the effect;
- "Next level: N ATP";
- "Click to buy" in `Accent`;
- for placeholders, "Planned: buying spends ATP but does nothing yet."

Wire it with `PointerEnter` / `PointerLeave`, registered once.

**Refresh** keeps today's poll-and-early-out shape. Writes happen only on
change, and there is no per-frame allocation.

### 2.6 Picker and upgrade panel

Same placement as today; the visuals follow `ui_states.png` D and E.

**Picker** (400 wide):
- title "PLACE A PROGENITOR · SLOT n" with a 26 px round `IconClose`;
- a **2×2 grid of unit cards** (178×114): portrait disc, name 13 bold,
  blurb 10;
- each card has a price chip and a **PLACE** button (`Accent`, or
  `ChipDim` when unaffordable).

**Upgrade panel** (360 wide):
- header: a 48 px portrait, the name at 17 px, and "6 / 10 cells fielded ·
  niche level 8";
- each row: name 13 bold, effect 11, then **level pips** (12 px circles:
  `Accent` filled, `Tile` empty, `Edge` 2);
- price chip and **BUY** on the right; "MAX" when maxed.

`BuyRow` builds its pips as elements once per `SetTarget`, replacing
`Dots()` text.

### 2.7 HUD (`HudView`): one row in the top gutter

A 600×110 Card.
- **Stats:** `IconBolt` + ATP, `IconHeart` + lives, then the round. Each is
  30 px over a 10 px caption.
- **Building:** a chunky **START ROUND n** button on the right (`Accent`,
  `IconPlay`, lip). Under it: tagline · "buy phase, time is frozen" ·
  "Space".
- **Active:** the button's slot becomes "WAVE b / t", "n in play" and a
  16 px `Accent` progress bar. Under it: "Buying stays open — the round
  does not pause."
- **Lives:** the 150 ms flash turns `Bad`. Below 25% the number stays
  `Bad` and the heart pulses.
- **Defeat:** "GAME OVER" at 30 px `Bad` replaces the button.

---

## 3. Board borders: a "diagram plate" on a dark desk

### 3.1 Choice

**The board becomes a textbook figure plate:** a rounded rectangle with a
4 px ink outline and a drop shadow, three folder tabs naming the bands, on
the existing dark warm desk (`CameraClear 0.18,0.13,0.17`).

**Rejected:**
- **Vignette:** it darkens exactly where threats appear (the lumen entry
  at the top, the base at the left), plus a full-screen overdraw quad.
- **Petri dish:** round on a 2.5:1 field, and it says "in vitro".
- **Ornate KR bevel:** heavy art that fights the flat line.

**Why the plate wins:** it costs one sliced renderer plus three UI tabs. It
turns the gutters into deliberate UI shelves, and it matches the OpenStax /
Servier register of outlined, labelled figures.

### 3.2 Plate frame (one renderer)

New `SpriteShapes.PlateFrame`:
- **Texture:** 128×128, `Sprite.Create(..., ppu 100, 0,
  SpriteMeshType.FullRect, border (48,48,48,48))`;
  `SpriteRenderer.drawMode = Sliced`.
- **Outside** a rounded rect inset 48, corner 40: value 1.0, α 1, tinted
  `CameraClear`. It is invisible against the clear colour and masks the
  board's square corners.
- **The edge:** a 6 tex px ring at value 0.5 → `FrameInk`, ≈ 4 px on
  screen.
- **Shadow, baked in:** the 12 tex px band below the bottom edge ramps
  0.55 → 1.0, offset 5 px.
- **Inside:** α 0.
- **Placement:** `size = board + (0.96, 0.96)` world, **order 40** (above
  the FX at 30). Corner radius 0.4 world ≈ 26 px. Agents crossing a corner
  are clipped, which is acceptable.

### 3.3 Band tabs (UI)

Three folder tabs on the top edge, each 14 px inside its band:
- **BLOODSTREAM**, chip `Plasma`;
- **TISSUE**, chip `HostHealthy`;
- **GUT LUMEN** + `IconArrowDown` (the flow direction), chip `Chyme`.

Each is a Card with top corners radius 9, a `FrameInk` 3 px border with no
bottom edge, 32 tall, overlapping the plate edge by 6 px. Inside: a 14 px
legend disc and 13 px bold caps. Position them with
`CameraTransformWorldToPanel`, as `WorldLabelView` does;
`PickingMode.Ignore`.

The seams themselves need nothing new: the villus fringe and wall bar mark
tissue|lumen, and the endothelial row marks blood|tissue.

---

## 4. Pathogen knowledge HUD (new): `UI/KnowledgeView.cs`

### 4.1 Placement

**Collapsed (always on):** the bottom-left gutter strip, 620×140, directly
under the lymph node where knowledge is made. Three species cards. It never
covers the board.

**Expanded:** click a card for a 470-wide Card above the strip. Click again
or press Esc to close; it also auto-closes on Building→Active.

### 4.2 Card (186×120, Tile, Edge 3, r 10; built once)

```
card
├─ iconDisc 42px (0.96,0.94,0.92), Edge 2; species icon 40px
├─ name     12px bold, wraps: "Virus" · "Intracellular bacterium" · "Large bacterium"
├─ pct      26px black, right; from static string[101] "0%".."100%"
├─ ladder   160x14 track (0.90,0.86,0.83), Edge 2, r 7
│  ├─ fill  species colour, width = shownPct%
│  ├─ pip×6 left = threshold%. locked: 12px Tile+Edge · learned: Tile+HatchTile (4.4)
│  │        · NEXT: 20px Tile, 4px species-colour ring, pulse scale 1→1.12 / 1.2 s
│  └─ star  IconStar 18px at 90%: (0.92,0.88,0.80), Gold once reached
└─ next     11px bold InkDim "Next: FcR at 45%" (Rung.ShortName)
```

**Species icons:**
- **Virus:** `Virion` in violet.
- **Large bacterium:** `LargeBacterium` in green.
- **Intracellular bacterium:** `HostCellInfectedBacterial` in lime, with
  `LargeBacterium` green ×0.55 inside ("the one that lives in your
  cells").

The bar colour is the icon colour. **At 0% the card sits at opacity 0.55**
(naive).

**Expanded rows** (7 × 44 px): pip, %, name (13 bold), effect (11), and a
pill, either `NEXT` in the species colour or a hatched `LEARNED · PLANNED`:

| % | Name | Effect |
|---|---|---|
| 10 | Cytotoxic T cells | Kill an infected cell cleanly: no collateral. |
| 20 | Neutralising antibodies | Known pathogen sticks to the gut wall less. |
| 30 | Memory T cells | A burst of killer T cells when it returns. |
| 45 | Fc receptor | Antibodies tag it for your innate cells. |
| 60 | Complement | Coated pathogens are damaged with no cell present. |
| 70 | Secretory IgA | Antibody in the lumen, before it can land. |
| 90 | Specific sensing (capstone) | Coated pathogens become beacons: search is solved. |

### 4.3 Feedback moments (poll, don't event)

**Detection.** `Refresh` checks `KnowledgeLedger.Revision`, then diffs
`Get(species)` against a cached `float[3]`.
- `delta > 0` means a match (+3% each today).
- `UnlockedCount` rising means a rung unlock.
- Write it as an explicit-time `Step(dt)`, so a harness can drive it.

**Match (frequent, small):**
1. The existing in-world cyan ring plays at the pairing.
2. **A "+3%" pill flies** from `CameraTransformWorldToPanel(LymphCenter)`
   to the card's numeral.
   - Path: `lerp(a,b,s) + (0, −60·sin πs)`, `s = smoothstep(t/0.6 s)`,
     scale 1→0.7.
   - Look: 48×24, species colour, `Edge` 2, white 13 px text with outline.
   - Pool: **6** pre-built Labels. Overflow skips the token but still
     updates the numbers.
3. **On arrival:**
   - the numeral pops 1→1.25→1 over 0.25 s;
   - `shownPct += (target−shown)·min(1, 10dt)`;
   - the card border flashes `FxKnowledge 0.15,0.80,1.00` for 0.3 s.

**Rung unlock (rare, big):**
1. The pip bursts, 1→1.8→1 over 0.4 s.
2. **A ribbon Card** (420×60) appears 8 px above the strip:
   - a species stripe;
   - a 40 px pip with a Gold `IconStar`;
   - "LEARNED · VIRUS 30%" at 11 px and the capability name at 17 px;
   - while unbuilt, a hatched pill **"PLANNED: NO EFFECT YET"**.

   Fade in 0.2 s, hold 3 s, fade out 0.4 s. Queue up to 8 (ring buffer).
3. `DegranulationFlash.Play(LymphCenter, 2× size, KnowledgeMatchColor)`.

**Negative delta** (a future mutation): the numeral flashes `Bad`, the bar
shrinks, no token.

Everything is `left/top/scale/opacity` writes on pre-built elements, on
`Time.unscaledDeltaTime`. There is **no per-frame allocation**.

### 4.4 ⚑ "Unlocked" for a capability that does nothing yet

**Show it as learned, honestly marked as planned:**
- the pip fills with the **hatch**, not the solid species colour;
- the expanded row says `LEARNED · PLANNED`;
- the ribbon says "no effect yet".

The **same hatch** marks placeholder shop tiles. "Hatched = designed, not
built" becomes one rule across the UI.

**Model change (data only):**
- `KnowledgeLadder.Rung` gains `public readonly bool Built`, false for all
  six today.
- Add `public const float CapstonePercent = 90f`.

Flipping `Built` makes the pip solid, with no UI change. The % really is
earned, but a solid pip would promise an effect that doesn't exist.

---

## 5. Notes for the implementer

**Sorting, net:**

| Order | What |
|---|---|
| 1 | organ shadows, vessels |
| 2 | marrow space, node body |
| 3 | bone, follicles |
| 4 | co-loc haze (was 3) |
| 5 | pads |
| 6 | select ring |
| 7 | pad glyph / unit |
| 40 | plate frame |

Retire `OrganHalo`, `MarrowRegion` and `LymphNodeBean` with Classic.

**`UiController`:**
- add the `bottomRight` dock and `KnowledgeView`, bound to `knowledge` and
  the node centre;
- move the debug readout to `topLeft`;
- extend `Over()` to the strip, tray, tabs, expanded card and tooltip.

**Headless:**
- the `KnowledgeView` diff/unlock logic and the `FitCamera` gutter maths
  are pure; test them in `CosmeticVerification.RunAll`;
- extend `SpriteShapeDump` to the new icons, pad, ring, plate and organ
  canvases.

**⚑ Out of scope but now visible:** both bacterial classes share one green
sprite in the lumen. The strip tells them apart; the board does not.

**On landing, update:**
- `UI_STYLE_GUIDE.md`: palette, HUD, sorting;
- `UI_DESIGN.md` §1/§2/§6: light cards, docks, no shop collapse;
- `GAME_DESIGN.md` §5: `Built`, capstone.

**References** (viewed, not committed):
- *Bone_cross-section.svg*, CC BY 3.0, Pbroks13.
- *Illu_lymph_node_structure.png*, public domain, SEER.
- The BTD6/KR/PvZ shots of `ART_DIRECTION.md` §1: non-free, research only.

---

## 6. Director decisions (recommendation first)

1. **Light cream panels instead of dark glass?** **Yes** (§2.1).
   Fallback: keep the glass, but only in the gutters.
2. **All permanent UI in the gutters?** Shop as an always-open tray
   bottom-right, knowledge bottom-left, HUD top-right; the camera reserves
   the gutters on ultra-wide. **Yes.** It also ends the shop's collapse.
3. **Unbuilt but unlocked capabilities:** show them as "learned ·
   planned" with a hatch, or hide them until built? **Show.** It is
   honest, the number matters now, and the same hatch marks placeholder
   shop items.
4. **Bundle a rounded OFL font** (Nunito Bold/Black, ~300 KB in
   `Resources/`) in place of the OS-font lookup? **Yes.** Most of
   "chunky, friendly" is the typeface. The cost is one asset plus an OFL
   notice.
5. **Show the 90% capstone** as a star on every ladder, permanently
   "planned"? **Yes.** It names the campaign's goal.

## 7. Suggested implementation order

1. **Tokens and sprites:** `UiTheme` light tokens and factories (Card,
   Tile, Chip, ChunkyButton, Pip, Tag, Hatch); the new 64 px icons, pad,
   ring and plate; the font, if decision 4 is yes.
2. **Frame:** `FitCamera` gutter reserve, plate frame, band tabs, dock
   reshuffle.
3. **Restyle:** HUD, shop tray, picker, upgrade panel; `BuyRow` gets pips
   and chips.
4. **Knowledge strip:** `Rung.Built`, the capstone constant, cards,
   expanded view, token, ribbon, plus the headless diff test.
5. **Organs:** `OrganShapes`, marrow layers, pads with glyph and unit,
   select ring, node layers and vessels, layout tweaks, tags, haze → 4.
6. **Verify and ship:** sprite dump, `tools/unity.sh verify`, a WebGL
   build for the Director's eye, then the docs in §5.

Steps 1–4 are UI-only. Step 5 is independent and can run in parallel.
