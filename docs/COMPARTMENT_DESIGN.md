# Compartment Visual Design Spec

Status: **implemented in Sprint 15** (2026-08-30). Written by a dispatched
Design agent; the head integrated it. Deviations from this spec as
shipped: the food-bolus channel wake (§7 Q2) was **deferred** to BACKLOG
(needs a live ref to the food GameObject for a barely-visible effect); the
co-localisation haze (§2.3, §7 Q3) shipped as the **single blob** tracking
the field centroid, not the 3×3 grid; peristalsis (§7 Q4) is
**whole-channel** at ±6%. Everything else landed as written. Companion to
`docs/SPRITE_DESIGN.md` (the Sprint 13 entity pass) and
`docs/UI_STYLE_GUIDE.md` ("what's on screen now").
This spec extends the same procedural `SpriteShapes` system to the three
compartments Sprint 13 left as flat tinted quads or minimal silhouettes:
the **lumen**, the **base (blood)**, and the interiors of the **lymph node**
and **bone marrow**. The tissue band is done and liked (`SPRITE_DESIGN.md`
§1) — this spec does not touch it.

An optional uncompiled prototype of the new shape generators lives at
`game/Assets/Scripts/Rendering/SpriteShapesCompartments.PROTOTYPE.cs` —
clearly marked, wired nowhere, for the head to review and fold into
`SpriteShapes.cs` (see §3, §4).

The hard constraints carried through: **procedural only** (no PNGs, no new
packages — `SPRITE_DESIGN.md` §4); **white RGB, silhouette in the alpha
channel** so `SpriteRenderer.color` stays the hue/state channel
(`UI_STYLE_GUIDE.md` "How sprites work" — and "RGB brightening is a no-op on
a white sprite," which bit a Sprint 13 shape, `TEAM_RETRO.md` Sprint 13);
**everything that moves is pooled from line one** (`GAME_DESIGN.md` §8,
`PrefabPool.cs`); **`Update()` is cosmetic only** — no spec here changes
simulation behaviour, and none of it is headless-testable
(`SPRITE_DESIGN.md` §5.2).

---

## 0. The board as it actually runs

`GAME_DESIGN.md` §1a describes Map 01 as 100×40 coarse cells. **The scene
that runs today is 25×10** — `Sprint1.unity` overrides `columns: 25` and
`BoardConfig`'s code defaults give `rows 10`, `baseBandCells 6`,
`lumenBandCells 6`, horizontal threat axis, base at the negative (left) end,
lumen flow toward the positive (down-screen) cross end. So:

```
 cols  0        5 | 6                      18 | 19          24
      +-----------+--------------------------+---------------+
 row0 |   BASE    |         TISSUE           |     LUMEN     |  flow
  ..  |  (blood)  |   eosin host-cell        |  (gut         |   |
  ..  |  marrow   |   lattice -- UNTOUCHED   |   channel)    |   v
  ..  |  + lymph  |   Sprint 13 stained      |              | excreted
 row9 |   node    |   section                |              |  off-screen
      +-----------+--------------------------+---------------+
       6x10 = 60      13x10 = 130 cells         6x10 = 60
       grid cells     (KEEP per-cell)           grid cells
```

Coarse cell = `7 * 0.16 = 1.12` world units. Board = `28.0 x 11.2` world
units. Cross axis = the 10 rows = lanes; lumen flow runs **down** the rows
(`FlowCrossStep = +1`), excreting off the bottom.

The renderer-count argument in the brief scales with the map: at 25×10 the
base + lumen bands are **120 of 250** grid `SpriteRenderer`s (48%); on the
100×40 aspiration they are **2,000 of 4,000** (50%). Pulling them out of the
per-cell grid is the spine of this spec (§4 commit 3, §5).

---

## 1. Visual direction — one image per compartment

**Lumen — "a slow brown river behind a pane of mucus."** Not cells in a
different colour: a continuous open channel of chyme, darkest mid-stream,
with a translucent pale mucus layer smeared along the gut wall and fine
particulate visibly *drifting down the lanes* toward the excretion end. A
pathogen riding it is a twig in a current — clearly on the far side of a
membrane, clearly just passing through, clearly safe. The channel breathes:
a slow peristaltic squeeze passes along it every few seconds (recommended —
§2.1, open question 1). `handoff-map01-intestine.md` §8's register holds:
clinical, desaturated, no hazard colour.

**Base — "your blood, and the two organs suspended in it."** A deep venous
red plasma field, darkest at the outer (left) edge and lifting toward a
soft **vessel wall** at the base/tissue seam — endothelium, the line your
cells cross to reach the fight. Erythrocytes stream in from the left edge
and drift toward that wall. The bone marrow and the lymph node sit *in*
this fluid, each ringed by a darker plasma halo and a faint bright rim so
they read as organs embedded in the bloodstream, not silhouettes floating
on the near-black camera clear colour. This compartment is your production
*and* your death (`GAME_DESIGN.md` §1: "the base and the lose condition are
the same compartment... this is what sepsis is") — the blood framing is the
point: a pathogen arriving here is arriving *in your blood*.

**Lymph node — "a kidney bean with lit follicles and a haze where the cells
gather."** The Sprint 13 bean silhouette keeps its shape but gains an
interior: a darker medullary notch on the concave side, two or three
brighter round germinal-centre zones in the cortex, and — the part the
Director asked to be legible — a faint cool haze rendered from the node's
real co-localisation `CytokineField`, brightest where the helper-T cells
have clustered, so you can *see* why a dendritic cell drifts where it does.
The magenta DC and teal T-cell dots still pop crisply on top.

**Bone marrow — "red marrow in a bone sponge, budding cells toward the
blood."** The Sprint 13 trabecular texture stays but sharpens: lighter bone
struts over reddish hematopoietic spaces (red marrow, tying it to the blood
it sits in), a couple of darker sinusoid channels draining toward the
vessel wall, and small pale motes that bud near an occupied progenitor slot
and drift toward the marrow's blood-side exit before fading — a visible
sense of cells being made. The five clickable slot niches are unchanged in
function; a selected-slot rim highlight is left as a hook for the Sprint 16
buy UI (out of scope — brief).

**Gut wall — "keep it, make it continuous with the mucus."** The
`GutInterfaceRenderer` thicken-and-heat + breach-flash animation is
load-bearing (`SPRINT_PLAN.md` history, `SPRITE_DESIGN.md` §2.10) and is
untouched. Only the skin changes: the quiet-state colour is pulled toward
the new lumen mucus band so the wall reads as the epithelium *under* that
mucus rather than a separate stripe, and the bar sprite gains sparse goblet
-cell flecks. Least-broken of the four; lowest priority.

---

## 2. Per-compartment spec

Sorting orders below extend `UI_STYLE_GUIDE.md`'s table. Nothing moves an
existing entry; new layers slot into gaps.

| Order | What | New? |
|---|---|---|
| 0 | Tissue host-cell grid; **base plasma field**; **lumen chyme field** | field quads new |
| 1 | **Vessel wall bar**; **organ halos**; **lumen mucus band** | new |
| 2 | Compartment backdrops (marrow, lymph) — **was 1**, bumped | moved |
| 2 | **Flow motes; erythrocyte streamers** (pooled) | new |
| 3 | Gut-wall bar (unchanged); **node co-localisation haze** | haze new |
| 4 | **Marrow birth-puff motes** (pooled) | new |
| 5 | Bone-marrow slots (unchanged) | — |
| 10+ | Immune cells / lymphocytes / DCs / pathogens / food / flashes | unchanged |

The two backdrops move 1 → 2 so the new halos (1) can sit behind them and
the plasma/chyme fields (0) behind everything. No entity layer changes.

---

### 2.1 Lumen

**Structure (replaces 60 per-cell quads with 2 field quads + a pooled mote
set):**

```
   base/tissue        gut wall (order 3,          far channel edge
      seam           GutInterfaceRenderer,          (excretion end,
        |             thicken+heat UNCHANGED)          off-screen)
        |                   |                              |
  TISSUE  | mucus band |         chyme field                | -> fade to black
  (eosin) |  (order 1) |          (order 0)                  |
          |<~~ opaque  |  . drifting flow motes (order 2) .  |
          |   at wall, |  .    pooled, ~40, tween down    .  |
          |   feathers |  .        the lanes @ ~1.1 u/s   .  |
          |   into ch. |                                     |
                             ^ slow peristaltic squeeze (opt)
```

- **Chyme field** — one `SpriteShapes.ChymeField` quad spanning
  `board.BandWorldRect(BoardBand.Lumen)`, `sortingOrder 0`. The sprite is a
  full-bleed rounded rectangle with an **alpha gradient along the cross
  axis** (see `AxisGradient`, §3): ~0.92 alpha at the wall end, easing to
  ~0.55 at the excretion end, plus a light `Stipple` (density 0.85) so it
  isn't a flat wash. Tinted `0.22, 0.17, 0.10` warm brown-olive at the wall
  end; `BoardRenderer` is not involved — a tiny `LumenChannelRenderer`
  (§4 commit 2) sets `sr.color` once. A second, darker copy tint
  `0.12, 0.10, 0.07` could be layered at the excretion end for the
  "exits = gone" read, or (cheaper) let the alpha gradient over the
  near-black camera clear do that work. Recommend the latter.
- **Mucus band** — one `SpriteShapes.MucusBand` quad, a strip ~1.6 coarse
  cells deep running the full lane length along the wall seam,
  `sortingOrder 1`. The sprite is opaque at the wall edge and feathers to
  transparent across ~65% of its depth (`EdgeGradient`, §3). Tint
  `0.40, 0.40, 0.30` — a desaturated grey-green, semi-transparent
  (`sr.color.a ≈ 0.5`). This is the glycocalyx / mucus layer of
  `handoff-map01-intestine.md` depth 1, and it visually explains where
  adhesion happens: pathogens in `AtInterface` state stack *against* this
  band (`PathogenAgent.InterfaceStackWorldPosition` already pushes them
  outward into the channel — nothing to change).
- **Flow motes** — pooled particulate. `PrefabPool` of
  `SpriteShapes.FlowMote` quads (a 5px 3-lobe blob + stipple, §3), ~40
  concurrent, `sortingOrder 2`, tint `0.42, 0.34, 0.22` dull chyme-ochre
  with ±8% per-instance value jitter and random spin. A
  `LumenChannelRenderer.Update()` (cosmetic, gated on
  `!RoundClock.Frozen` exactly like every other `Update` driver —
  `RoundClock.cs`) advances each mote along `board.FlowCrossStep` at
  ~1.1 world units/s ± 25% per instance, with a small per-frame lateral
  wobble; a mote past the excretion edge is recycled to a random lane at
  the wall end. **Pooled from the first line** — never "add pooling
  later" (`GAME_DESIGN.md` §8). Cost: §5.

**Motion — two options, as the brief requires. Recommendation flagged.**

- **Option A — directional flow + mucus, static channel geometry.**
  Exactly what's above: the two field quads never move, the ~40 motes tween
  down the lanes. Flow direction is legible from the mote streaks alone.
  *Per-frame cost:* ~40 transform writes + 40 float compares in one
  `Update()`; zero allocation (pooled); no `SpriteRenderer.color` churn.
  Well under 0.05 ms. *Pooling:* one `PrefabPool`, pre-warmed to 48.
  *Renderer delta:* −60 (lumen grid cells) + 2 (field quads) + ~40 pooled
  motes.

- **Option B — Option A plus a slow peristaltic squeeze.** A single
  `Mathf.Sin` phase in `LumenChannelRenderer.Update()` drives a ±6%
  (hard-capped ±8%) scale modulation on the **cross-axis dimension** of the
  chyme + mucus quads, period ~7–9 s, and multiplies the mote velocity in
  phase (motes visibly speed up as the squeeze passes). Optionally the
  squeeze is a travelling band rather than a whole-channel pulse — a phase
  offset per lane group — but a whole-channel pulse is cheaper and reads
  fine at this scale. *Per-frame cost over Option A:* one `Sin`, ~4
  transform-scale writes, one float multiply. Negligible — call it
  +0.005 ms. *Pooling:* unchanged. *Renderer delta:* identical to A.
  *Risk:* the squeeze is purely visual — pathogen adhesion/collision still
  runs on fixed coarse cells — so the amplitude cap matters: at >±8% the
  drawn mucus band would visibly desync from the `AtInterface` pile line.
  Also `handoff-map01-intestine.md` open question 1 ("flow direction
  without animation becoming noise at high speed multipliers") — at the
  game's speed-up control a big squeeze could strobe.

**Recommendation: Option B, at the conservative ±6% amplitude.** The
lumen's entire job on screen is to say "this is a living gut channel and
what's in it is just transiting" — and a slow squeeze sells *peristalsis
carried it out* (the excretion-is-not-a-fail-state read, `GAME_DESIGN.md`
§6a) far better than uniform drift, which risks looking like static
geometry with confetti on it. The cost gap between A and B is rounding
error. Option A is the clean fallback if the squeeze reads as noise when
the Director runs the speed control up — it's a one-field revert (§4
commit 9 is deliberately last and isolated for exactly this).

**Palette check (constraint 5).** Chyme brown-olive `0.22,0.17,0.10` and
mote ochre `0.42,0.34,0.22` vs. the load-bearing palette: food bolus ochre
is `0.55,0.47,0.28` — the motes are deliberately darker and greyer so the
bolus stays the biggest, brightest thing in the channel; bacterial-infected
host is `0.62,0.60,0.26` yellow-green — the mucus `0.40,0.40,0.30` is
darker, greyer, semi-transparent, and lives in the lumen band, never on a
tissue cell. Cytokine-heat orange `1.00,0.55,0.05` is far brighter and more
saturated than anything here. For colour-blind players the separation is
**lightness, not hue**: lumen sits mid-dark-warm, tissue is light pink,
base is dark red — a clean value ladder that survives deuteranopia and
protanopia.

---

### 2.2 Base (blood)

Director decision (brief): *literal vascular / blood direction. Deep-red
plasma field, a vessel-wall boundary at the base/tissue seam, immune cells
visibly originating from the left edge, marrow + lymph node reading as
organs embedded in the bloodstream.*

**Structure (replaces 60 per-cell quads with 1 field quad + a wall strip +
2 halos + a pooled streamer set):**

```
  outer edge                                    base/tissue seam
  (venous, darkest)                             (endothelium)
     |                                                |
     | . erythrocyte streamers (order 2, pooled) ->   |
     |   ~24, enter left, drift toward the wall        |
     |                                                 |
     |     [ MARROW ]        [ LYMPH NODE ]            | ) VESSEL WALL BAR
     |   halo + backdrop    halo + backdrop            |   (order 1) soft
     |   (halo order 1,     + coloc haze (order 3)     |   endothelial band,
     |    backdrop order 2)                            |   faint seams
     |                                                 |
        plasma field (order 0): radial-ish gradient,
        dark oxblood at the edge -> lifting to the wall
```

- **Plasma field** — one `SpriteShapes.PlasmaField` quad over
  `board.BandWorldRect(BoardBand.Base)`, `sortingOrder 0`. Full-bleed
  rounded rectangle, alpha ~0.95 throughout (it's opaque blood, not a
  vignette on black), with a gentle `AxisGradient` **brightening toward the
  wall** and a very light `Stipple` (density 0.9). Two-tint approach via a
  single sprite + the renderer: base tint **`0.26, 0.08, 0.11`** deep
  oxblood at the outer edge, and the near-wall lift is carried in the alpha
  silhouette so a lighter plasma tint `0.40, 0.12, 0.15` shows through a
  second thin quad at the seam — or, simpler, one quad tinted
  `0.30, 0.10, 0.13` and let the gradient + wall bar do the depth read.
  Recommend the one-quad version first; add the second if it reads flat.
- **Vessel wall bar** — one `SpriteShapes.VesselWallBar` strip at
  `board.TissueBaseEdgeAxisIndex`, ~1 coarse cell deep, full lane length,
  `sortingOrder 1`. Sprite: full-bleed rounded rect with **sparse, faint**
  darker seams (an endothelium is smoother than the gut epithelium — fewer
  seams, lower contrast than `EpithelialBar`). Tint `0.66, 0.44, 0.46` — a
  muted red-pink, deliberately between the base's oxblood and the tissue's
  eosin `0.80,0.62,0.66` so it reads as the membrane *between* the two
  bands. This is also where `SearchUnit`s already spawn
  (`BoneMarrowManager.Emit` enters at `TissueBaseEdgeAxisIndex` on a random
  lane) — so "cells originate from the left edge and cross the wall" is
  already true in the sim; this bar just makes the crossing visible.
- **Organ halos** — one `SpriteShapes.OrganHalo` quad behind the marrow
  backdrop and one behind the lymph backdrop, each ~1.15× the backdrop
  size, `sortingOrder 1` (behind the backdrops at 2). Sprite: a soft radial
  alpha falloff (a `RadialGradient`, §3) — darker plasma tint
  `0.20, 0.06, 0.09` in the ring, so each organ sits in a pool of deeper
  blood; a 1px bright rim (`0.55, 0.20, 0.22`) at the backdrop edge sells
  "suspended in fluid." Built in `BuildBoneMarrowBackdrop` /
  `BuildLymphNodeBackdrop` (§4 commit 5).
- **Erythrocyte streamers** — pooled. `PrefabPool` of
  `SpriteShapes.Erythrocyte` (a 10px disc with a dark biconcave centre
  shade, §3), ~24 concurrent, `sortingOrder 2`, tint `0.55, 0.16, 0.18`
  brighter arterial red (distinct from the dark field by *value*, and
  moving). A `BaseCompartmentRenderer.Update()` (cosmetic, `RoundClock`
  -gated) spawns them at the outer edge on random lanes and drifts them
  toward the wall at ~0.6 u/s with lateral wobble; recycled on arrival.
  **Pooled from line one.** ~24 is a trickle, not a crowd — the compartment
  should read as calm until something breaches it.

**Marrow / lymph "embedded" read.** The halos do most of the work. The
backdrops keep their existing tints (`0.30,0.24,0.16` brown /
`0.34,0.40,0.28` green) which are already chosen to sit apart from the
base's colour (`BoardRenderer` comments) — against oxblood plasma with a
dark halo they'll read as solid organs, not floating cards.

**Palette check (constraint 5).** The danger is the **large-bacterium
maroon `0.42, 0.12, 0.16`**. The plasma field is darker (`0.26–0.30` R,
`0.08–0.10` G) and, critically, is a *large flat field in the base band*
while the bacterium is a small rimmed **rod in the tissue band** — they are
never co-located (adhesion and kills happen in tissue; a bacterium that
reaches the base despawns via `InvasionTally.ReachedBase`). The
erythrocytes at `0.55,0.16,0.18` are brighter and are small moving discs,
not rods. The free-virion cold purple `0.40,0.16,0.34` has real blue in it;
plasma has almost none. Vessel-wall `0.66,0.44,0.46` vs. eosin tissue
`0.80,0.62,0.66` — same hue family on purpose, separated by value (the wall
is ~15% darker) and by being a single thin horizontal band. Colour-blind:
again a **value ladder** — base darkest, lumen mid, tissue lightest — plus
the base is the only band with a hard directional gradient.

---

### 2.3 Lymph node

Sprint 13 gave it `LymphNodeBean` (a 2-lobe blob + two brightened follicle
shades + rim) tinted `0.34, 0.40, 0.28`. Keep the silhouette; deepen the
interior and add the field haze.

- **Revised `LymphNodeBean` sprite** (alpha silhouette only — brightness
  lives here, not in RGB):
  - Cortex: the bean body, unchanged outline.
  - **Medullary notch** — one `InnerShade` at ~0.72× on the concave side of
    the bean, so it reads as the hilum where vessels enter.
  - **Germinal centres** — 3 (was 2) brightened `InnerShade` discs at
    ~1.20×, varied radius, in the cortex ring. These are where B-cell
    follicles would be; here they're just structure.
  - Keep the 1px rim.
- **Co-localisation haze** — the interesting one. `LymphNode` already owns a
  real `CytokineField coloc` (the §5c step-4 signal, recomputed every node
  tick from a central source + every resident lymphocyte as a weak source)
  and exposes it (`LymphNode.Coloc`), plus `NodeToWorld(FineCoord)` and
  `WorldRect`. A small `LymphNodeFieldRenderer` (or a few lines folded into
  `AdaptiveDirector`, which holds the `LymphNode` ref and already gates on
  `RoundClock.Frozen`) tints **one soft `SpriteShapes.NodeColocGlow` quad**
  (a `RadialGradient`, `sortingOrder 3`, covering `WorldRect`) — or, for a
  legible gradient *shape*, a 3×3 mini-grid of glow quads sampled from
  `coloc.CoarseValueAt` — every ~0.15 s, alpha `= clamp01(value / ref) *
  0.35`, tint a cool cyan-white **`0.55, 0.85, 0.85`**. Brightest where the
  T cells have gathered, so a DC drifting up the gradient is visibly
  drifting toward the light. This is the mechanic the Director asked to be
  legible (`GAME_DESIGN.md` §5c: "the second cytokine is load-bearing, not
  decoration"). Recommend the single quad (cheap, 1 renderer, slightly
  blurry) with a 3×3 as the fallback if a single blob doesn't show the
  gradient — open question 3.
- The magenta DC (`DendriteStar`, order 13) and teal helper-T
  (`Lymphocyte`, order 12) render on top unchanged; the paired-freeze tint
  lerp and the green `KnowledgeMatchColor` match-flash (order 30) are
  untouched.

**Palette check.** Haze cyan-white `0.55,0.85,0.85` vs. helper-T teal
`0.32,0.72,0.70` — the haze is lighter, lower-saturation, a static diffuse
field at ≤35% alpha; the T cell is a crisp saturated moving dot. Vs. the
green knowledge flash `0.40,0.92,0.45` — different hue (cyan vs. green),
different shape (diffuse field vs. thin expanding ring), different duration
(persistent vs. 0.5 s). Vs. efferocytosis blue-green `0.45,0.80,0.68` —
that fires only in *tissue*, never the node. Node green backdrop
`0.34,0.40,0.28` is far darker and browner than the haze.

---

### 2.4 Bone marrow

Sprint 13 gave it `MarrowRegion` (rounded region + 5 brightened trabecular
`InnerShade`s + heavy stipple) tinted `0.30, 0.24, 0.16`. Keep the idea;
sharpen the contrast and tie it to the blood, and add the production motes.

- **Revised `MarrowRegion` sprite** (alpha silhouette):
  - Struts: raise the trabecular `InnerShade`s to ~1.35× and add 2–3 more,
    thinner, so the sponge reads at a glance rather than only up close.
  - **Sinusoid channels** — 2 `FillCapsule` strokes at ~0.75× alpha running
    roughly toward the blood-side (wall-side) edge — the route emitted
    cells and the birth-puff motes travel. Subtle.
  - Keep the stipple but drop its density slightly (0.7 → 0.62) so the
    struts aren't lost in noise.
  - Retint in the renderer to **`0.34, 0.22, 0.18`** — a redder brown (*red*
    marrow), so it visually belongs to the blood compartment it's embedded
    in rather than reading as dry bone. Still clearly distinct from the base
    plasma (browner, lighter) and from the lymph node (green).
- **Birth-puff motes** — pooled. `PrefabPool` of `SpriteShapes.BirthPuff`
  (a 6px soft disc, wide AA, no rim, §3), tint a pale marrow-cell
  `0.70, 0.62, 0.50`, `sortingOrder 4` (above the backdrop at 2, below the
  slots at 5). A puff buds near an **occupied** progenitor slot, drifts
  toward the marrow's blood-side edge over ~1.5 s, fading alpha `1 → 0`,
  then recycles. **Pooled from line one.** Keep it sparse — cap ~12
  concurrent.
  - *Coupling choice (open question 7):* either an **ambient trickle** (one
    puff every ~0.8 s from a random occupied slot, no engine coupling,
    purely `BaseCompartmentRenderer`-owned) or **one puff per real
    emission** — a 1-line `Action` hook fired from `BoneMarrowManager.Emit`
    (`BoneMarrowManager.cs` ~L471, right after `slot.Children.Add(unit)` /
    in `EmitAdaptive`). The hook ties the visual to the mechanic (you see a
    cell made *because* a cell was made) at the cost of touching one live
    file; ambient is zero-coupling. Recommend the hook if the head is
    comfortable with the one-liner, ambient otherwise.
- **Selected-slot highlight** — not built (Sprint 16, brief). Note only:
  the Sprint 16 buy UI will want a way to draw a bright rim on the clicked
  `SlotNiche`; the cleanest hook is a `bool selected` on the slot that
  multiplies `sr.color` toward white or swaps to a `SlotNicheSelected`
  sprite. No layout, no work now.

**Palette check.** Red-marrow `0.34,0.22,0.18` vs. large-bacterium maroon
`0.42,0.12,0.16` — marrow is lighter and much browner (G `0.22` vs.
`0.12`), and it's a big textured region in the base band behind the slot
column, not a rod. Birth-puff `0.70,0.62,0.50` vs. neutrophil gold
`0.93,0.74,0.30` — the puff is paler, desaturated, larger, slow, fading,
and lives inside the marrow backdrop; the neutrophil is a saturated crisp
disc out in the tissue. Vs. food ochre `0.55,0.47,0.28` — puff is lighter
and greyer and never in the lumen.

---

### 2.5 Gut wall (refinement only)

`GutInterfaceRenderer` — the per-position thicken (`localScale` lerp
`0.16→0.85` of a cell) + heat (`WallColor 0.55,0.47,0.40` →
`AlarmColor 0.95,0.30,0.20`) + the `Breached` burst flash — is **untouched**
(`SPRITE_DESIGN.md` §2.10, `SPRINT_PLAN.md` history: load-bearing). Two
skin tweaks only:

- Swap `SpriteShapes.EpithelialBar` for a revised version with sparse
  brighter goblet-cell flecks (a few `InnerShade` at ~1.2× scattered in the
  bar alpha) — mucus-secreting cells, which ties the wall to the new mucus
  band conceptually.
- Nudge `WallColor` toward the mucus tint so quiet wall + lumen mucus band
  read continuous. This is a one-line change to a `static readonly Color`
  in `GutInterfaceRenderer.cs` (L37) — flag for the Director, it's a taste
  call, and it's the only place this spec proposes changing a shipped
  colour value rather than adding new ones.

---

## 3. New `SpriteShapes` primitives / entries

Sketched in `SPRITE_DESIGN.md` §4 style — signatures + a sentence. All
operate on the existing `Color[Res*Res]` buffer, white RGB, **coverage /
detail in the alpha channel**, max-blended. Reuse the existing
`FillDisc` / `FillRing` / `FillCapsule` / `FillLobed` / `FillRounded` /
`InnerShade` / `RimShade` / `Stipple` / `Multiply` / `Coverage` / `ForBox`
wherever possible. Prototype in
`SpriteShapesCompartments.PROTOTYPE.cs`.

**New primitives (3):**

```
void AxisGradient(buf, bool alongX, float aStart, float aEnd)
    // Multiply every opaque pixel's alpha by a linear ramp from aStart at
    // one edge to aEnd at the other, along X (alongX) or Y. The chyme-depth
    // fade, the plasma near-wall lift, the mucus feather's coarse shape.

void EdgeGradient(buf, int edge /*0=left 1=right 2=top 3=bottom*/, float featherPx)
    // Multiply alpha down from 1 to 0 within featherPx of one named border.
    // The mucus band (opaque at the wall edge, gone by mid-channel) and a
    // soft-edged backdrop that must not hard-cut against its neighbour band.

void RadialGradient(buf, float cx, float cy, float rInner, float rOuter,
                    float aInner, float aOuter)
    // Set alpha = lerp(aInner, aOuter, smoothstep(rInner..rOuter, dist)).
    // Unlike InnerShade this WRITES alpha (not RGB) and works on an empty
    // buffer. The plasma vignette, the organ halos, the node coloc glow,
    // the soft birth-puff.
```

**New sprite entries (~8), each a lazy cached `Sprite` like every existing
accessor:**

```
ChymeField      = FillRounded(full-bleed, corner 2) ; AxisGradient(alongCross,
                  0.92 -> 0.55) ; Stipple(seed, 0.85)
MucusBand       = FillRounded(full-bleed) ; EdgeGradient(wall edge, feather
                  ~42px of 64) ; Stipple(seed, 0.9)
FlowMote        = FillLobed(c, 5, lobes 3, depth 0.3, rand phase) ;
                  Stipple(seed, 0.7) ; RimShade(1, 0.7)
PlasmaField     = FillRounded(full-bleed, corner 2) ; AxisGradient(alongAxis,
                  0.86 -> 1.0 toward wall) ; Stipple(seed, 0.9)
VesselWallBar   = FillRounded(full-bleed) ; a FEW faint vertical seams
                  (x % 18 == 0 -> *0.82) ; RimShade(1, 0.85)
OrganHalo       = RadialGradient(c, c, rInner ~18, rOuter ~31, aInner 0.0,
                  aOuter 0.85) ; FillRing(c, c, 20, 18) at full alpha  // bright rim
Erythrocyte     = FillDisc(c, 12) ; InnerShade(c, 8, 0.68) ;  // biconcave dip
                  RimShade(1, 0.8)
BirthPuff       = RadialGradient(c, c, 0, 12, 1.0, 0.0)        // soft, rimless
NodeColocGlow   = RadialGradient(c, c, 4, 31, 0.9, 0.0)        // one soft blob
```

Plus the two **revisions** to existing entries (same file, same pattern):
`MarrowRegion` (stronger struts + 2 `FillCapsule` sinusoids + lower stipple)
and `LymphNodeBean` (add a medullary-notch `InnerShade`, a third germinal
centre). `EpithelialBar` gains a few goblet flecks.

All of these are 64×64, white+alpha, generated once, shared. Total added
texture memory ≈ 8 × 16 KB ≈ **128 KB one-time**, shared across every
instance (a 40-mote pool points at *one* `FlowMote` sprite).

**Style note for the integrator:** the new primitives are O(n) per pixel —
cheaper than the existing O(n²) `RimShade`. The gradient sprites don't need
`Coverage` supersampling (a gradient has no hard edge to alias); a direct
per-pixel write is fine and faster.

---

## 4. Migration plan — ordered, independently landable commits

Every step is additive or a localised swap; `sortingOrder` /
`localScale`-magnitude / `color`-hook assignments elsewhere stay as they
are, except the two backdrop `sortingOrder 1 → 2` bumps (commit 5) and the
grid-cell reduction (commit 3). No sim behaviour changes anywhere.

Each new `.cs` file needs its `.meta` committed alongside it
(`AGENT_HANDBOOK.md` rule — the repo has bitten itself on this).

### Commit 1 — land the new generators (no behaviour change)

Fold `SpriteShapesCompartments.PROTOTYPE.cs` into `SpriteShapes.cs`: the 3
new primitives, the ~8 new accessors, the 3 revised accessors. Add the new
accessors to `SpriteShapes.Prewarm()` (`SpriteShapes.cs` L458). Compiles,
referenced nowhere. Verify: batchmode compile clean.

### Commit 2 — `LumenChannelRenderer` (new), lumen still on the grid underneath

New `game/Assets/Scripts/Rendering/LumenChannelRenderer.cs` (+ `.meta`).
`Bind(BoardConfig)` builds the `ChymeField` + `MucusBand` quads over
`board.BandWorldRect(BoardBand.Lumen)`; owns a `PrefabPool` of `FlowMote`
(pre-warm 48); `Update()` — early-returns on `RoundClock.Frozen`
(`RoundClock.cs`) — drifts motes along `board.FlowCrossStep`, recycles at
the excretion edge. `GameBootstrap.BuildLumenChannel()` called from `Awake`
after `BuildBoardVisual` (`GameBootstrap.cs` ~L177). The lumen grid cells
still draw under it for now — verify the channel reads on top, motes flow
the right way (down-screen), no z-fighting with pathogens (order 2 vs. 20).

### Commit 3 — drop lumen + base cells from the per-cell grid (the renderer win)

- `GameBootstrap.BuildBoardVisual` (`GameBootstrap.cs` L330–353): in the
  `col,row` loop (L336–348), only `AddComponent<SpriteRenderer>` when
  `board.BandOf(new CoarseCoord(col,row)) == BoardBand.Tissue`; leave
  `views[col,row]` null otherwise. (The array stays full-size so every
  index is valid.)
- `BoardRenderer.Bind` (`BoardRenderer.cs` L92) and `Refresh` (L133–177):
  first line inside the `col,row` loop, `if (views[col,row] == null)
  continue;`. `baseColors` / `isHostGround` can still be filled for all
  cells (cheap) or skipped for null views. The `BandColor` path for
  non-host-ground cells (L148–149) becomes dead for base/lumen — leave it,
  it's harmless and keeps the method total for a hypothetical future map
  that puts host cells in another band.
- Net: **−120 `SpriteRenderer`s at 25×10** (−48% of the grid); the
  `Refresh` loop stops touching 120 cells every 0.15 s. Verify: tissue band
  renders identically, no `NullReferenceException`, HUD frame-cost readout
  drops.

### Commit 4 — `BaseCompartmentRenderer` (new)

New `game/Assets/Scripts/Rendering/BaseCompartmentRenderer.cs` (+ `.meta`).
`Bind(BoardConfig)` builds the `PlasmaField` quad over
`board.BandWorldRect(BoardBand.Base)` (order 0) and the `VesselWallBar`
strip at `board.TissueBaseEdgeAxisIndex` (order 1, full lane length via
`board.CoarseFromAxis` + `CoarseCellWorldSize`); owns a `PrefabPool` of
`Erythrocyte` (pre-warm 32); `Update()` — `RoundClock`-gated — spawns/drifts
/recycles ~24 streamers left→wall. `GameBootstrap.BuildBaseCompartment()`
from `Awake` (~L179, before `BuildBoneMarrowBackdrop`). Verify: base band
reads as blood with a directional gradient, wall bar sits between the bands,
`SearchUnit`s still visibly spawn at and cross the wall line.

### Commit 5 — seat the organs (halos + backdrop layer bump)

`GameBootstrap.BuildBoneMarrowBackdrop` (L362–376) and
`BuildLymphNodeBackdrop` (L378–391): before creating the backdrop
`GameObject`, create an `OrganHalo` quad at the same centre, ~1.15× the
backdrop `localScale`, `sortingOrder 1`, tint `0.20,0.06,0.09`. Change the
backdrop `sr.sortingOrder` from `1` to `2` in both methods (L370, L386).
Update `UI_STYLE_GUIDE.md`'s sorting table. Verify: marrow + lymph read as
solid organs in fluid, not floating cards; nothing else moved layer.

### Commit 6 — marrow interior + birth puffs

Revised `MarrowRegion` already in `SpriteShapes` from commit 1 — it swaps in
automatically (`BuildBoneMarrowBackdrop` L368 already assigns
`SpriteShapes.MarrowRegion`); just add the retint
`sr.color = new Color(0.34f,0.22f,0.18f)` (L369 currently sets
`0.30,0.24,0.16`). Add a `BirthPuff` `PrefabPool` + emitter to
`BaseCompartmentRenderer` (it already owns the base-band `Update`); ambient
trickle from occupied slots, OR wire the 1-line hook in
`BoneMarrowManager.Emit` / `EmitAdaptive` (`BoneMarrowManager.cs` ~L471 /
~L488) — open question 7. Verify: struts read; puffs bud near occupied
slots and fade toward the wall; sparse.

### Commit 7 — lymph node interior + co-localisation haze

Revised `LymphNodeBean` swaps in automatically (`BuildLymphNodeBackdrop`
L384). Add `NodeColocGlow`: new `LymphNodeFieldRenderer.cs` (+ `.meta`) OR
~15 lines in `AdaptiveDirector` — one `NodeColocGlow` quad over
`node.WorldRect`, `sortingOrder 3`, re-tinted every ~0.15 s from
`node.Coloc.CoarseValueAt` (max alpha 0.35, tint `0.55,0.85,0.85`), gated
on `RoundClock.Frozen` (`AdaptiveDirector.Update` already has this gate,
L85). Wire from `GameBootstrap.BuildAdaptiveDirector` (L443–459) — it
already builds the `LymphNode` and has `node.WorldRect`. Verify: haze
brightens where T cells cluster; DC drifts up it; DC/T dots still crisp on
top.

### Commit 8 — gut wall skin (lowest priority)

`GutInterfaceRenderer.cs` L60: revised `EpithelialBar` (goblet flecks) from
commit 1 swaps in automatically. Optionally nudge `WallColor` (L37) toward
the mucus tint — flag to Director. `Refresh()` maths (L92–112) untouched.

### Commit 9 — peristalsis (only if Option B adopted; deliberately last)

Add the `Mathf.Sin` phase + ±6% cross-axis scale modulation on the
`ChymeField` + `MucusBand` quads and the in-phase mote-velocity multiplier
to `LumenChannelRenderer.Update()`. One `[SerializeField] float
peristalsisAmplitude = 0.06f` (0 = Option A) so it's a one-value revert.
Verify at the game's speed-up control that it doesn't strobe.

---

## 5. Performance analysis

**Renderer count (25×10 scene that runs today):**

| | Δ renderers |
|---|---|
| Remove lumen grid cells (6×10) | **−60** |
| Remove base grid cells (6×10) | **−60** |
| + chyme field, mucus band | +2 |
| + plasma field, vessel wall bar | +2 |
| + 2 organ halos | +2 |
| + node coloc glow (1 quad) | +1 |
| **Always-resident subtotal** | **−111** |
| + flow motes (pooled, live only during `Active`) | +~40 |
| + erythrocyte streamers (pooled, `Active` only) | +~24 |
| + birth-puff motes (pooled, `Active` only) | +~12 |
| **Full-round total** | **−35** |

So: **≈ −111 always-on `SpriteRenderer`s (−44% of the whole board grid)**,
and during a running round **≈ −35** counting every pooled mote. On the
100×40 Map 01 aspiration the always-resident delta is **≈ −1,990** (the
base+lumen grid is 2,000 cells; the field/halo/wall quads don't scale with
map size). This directly addresses the standing scale note in
`BoardRenderer.cs`'s class comment and the Sprint 4 "frame cost is
vsync-capped and unmeasured" `BACKLOG` item — there is simply less to draw
and less to recolour.

**Per-frame cost of the motion:**

- **Flow motes (Option A):** one `Update()`, ~40 iterations, each a
  `transform.position +=`, a bounds compare, an occasional recycle. No
  allocation (pooled). **< 0.05 ms.**
- **Peristalsis (Option B adds):** one `Mathf.Sin`, ~4 `transform`
  scale writes, one float multiply into the mote loop. **< 0.005 ms.**
- **Erythrocyte streamers:** ~24 iterations, same shape as flow motes, plus
  an alpha write on the few mid-fade. **< 0.03 ms.**
- **Birth puffs:** ≤12 iterations + alpha fade. **< 0.02 ms.**
- **Node coloc haze:** one quad, `sr.color` rewritten every ~0.15 s from
  one `CoarseValueAt` read (or 9 reads for a 3×3). **Negligible.**
- **Removed:** the `BoardRenderer.Refresh` inner body for 120 cells every
  0.15 s — a `CoarseValueAt` sample, a `Color.Lerp`, a host-state enum pick,
  a sprite assignment, per cell. That's the real steady-state saving; the
  ~76 pooled motes that replace those 120 cells are each cheaper per frame.

**Pooling plan.** Three `PrefabPool`s, all pre-warmed at `Bind` time so no
runtime `Instantiate`:

| Pool | Prefab (1 shared sprite) | Pre-warm | Owner |
|---|---|---|---|
| flow motes | `FlowMote` | 48 | `LumenChannelRenderer` |
| erythrocytes | `Erythrocyte` | 32 | `BaseCompartmentRenderer` |
| birth puffs | `BirthPuff` | 16 | `BaseCompartmentRenderer` |

Each is a `GameObject` with one `SpriteRenderer`, exactly like the existing
`DegranulationFlash` / pathogen / unit pools built in `GameBootstrap`. A
mote past its lifetime is `Release`d, not destroyed. This is spec'd from
line one per `GAME_DESIGN.md` §8 — there is no "retrofit pooling later"
path here.

**Boot cost.** ~8 new 64×64 textures. The new gradient primitives are O(n),
cheaper than the shipped O(n²) `RimShade`. `SpriteShapes.Prewarm()` still
isn't called (`BACKLOG` Sprint 13 item) — this spec's new base/lumen
sprites are generated during `GameBootstrap.Awake` (the renderers touch
them at `Bind`), so **calling `Prewarm()` in `Awake` is now worth doing**
to move the whole cost to one known point instead of a first-of-kind hitch
mid-round. Recommend adding that one line in commit 1.

---

## 6. Verification

**Headless: nothing.** Every line of this spec is `Update()`-driven cosmetic
rendering or `SpriteRenderer` configuration. `Update()` does not run in
batchmode; no harness asserts rendered output (`SPRITE_DESIGN.md` §5.2,
`ENGINE_STATUS.md` Sprint 13 notes, `AGENT_HANDBOOK.md`). The pooled motes
carry no simulation state and feed nothing back into the sim. There is
nothing here for `CombatVerification` / `MapVerification` / any editor
harness to check, and that is correct — same as the Sprint 13 pass.

**The one automatable signal:** `GameBootstrap.Awake` now triggers
generation of ~8 new procedural rasters (via `Prewarm()` and/or the new
renderers' `Bind`). So **"a batchmode scene load / bootstrap completes with
0 exceptions"** covers "no new `SpriteShapes` primitive threw" (an index
slip in `AxisGradient` / `RadialGradient`, a bad `ForBox` range). If there's
a scene-load smoke `-executeMethod` it should be run after each commit;
if not, one is cheap and worth adding (it also would have caught the
Sprint 4 degenerate-band bug).

**The Director must eyeball (screenshot / live):**

1. Three bands read apart at a glance and on a still frame — the
   value ladder (base darkest, lumen mid, tissue lightest) survives.
2. The **lumen reads as a flowing channel**, not tinted cells: flow
   direction obvious from the mote streaks, mucus layer visible along the
   wall, and it still reads at the game's speed-up control (Option B: the
   squeeze doesn't strobe — `handoff` open question 1).
3. The **base reads as bloodstream** with the marrow and lymph node
   sitting *in* it (halos doing their job), not on the near-black clear
   colour. Erythrocytes visibly enter from the left and drift to the wall.
4. The **sepsis read** — when a pathogen reaches the base, it's visibly
   arriving in your blood (see open question 8 about making the life-loss
   itself acute).
5. **Bone marrow** shows production — struts legible, puffs budding near
   occupied slots.
6. **Lymph node** — follicles visible, and the co-localisation haze is
   legible and tracks where the T cells are; DC/T pairing still reads
   crisply on top.
7. **Tissue band pixel-identical** to pre-Sprint-15.
8. **Gut wall** thicken + heat + breach flash **unchanged**.
9. No z-fighting at any seam: plasma/chyme 0, wall/halos/mucus 1, backdrops
   2, motes 2, gut wall / node haze 3, birth puffs 4, slots 5, agents 10+.
10. HUD frame-cost readout is **lower** than before (commit 3).

---

## 7. Open questions for the Director

1. **Lumen motion — pick.** Recommendation: **Option B — directional flow +
   translucent mucus wall + a slow peristaltic squeeze at ±6% amplitude,
   ~7–9 s period.** Option A (same, minus the squeeze, static channel
   geometry) is the fallback and is a one-value revert. The squeeze sells
   "peristalsis moved it through" and makes the channel read as alive
   rather than as static geometry with drifting specks; the cost gap
   between A and B is rounding error (§2.1, §5). Approve B, or take A if you
   want to see the static version first. *[flagged — this is the pick you
   asked for]*

2. **How much should the food bolus disturb the channel?** The
   `ContaminatedFoodItem` transits the lumen every round and drops its
   batch at the wall (`GAME_DESIGN.md` §5d, `PathogenSpawner`). Should it
   visibly shove the channel — deflect nearby flow motes, drag a short
   wake, smear the mucus where it wall-hugs? Cheap (motes within ~1 cell
   get a push vector; no new renderers). Recommend a subtle local wake.
   How strong — barely-there, or "you can see the contamination pushing
   through"?

3. **Co-localisation gradient — drawn field or implied?** Recommend a
   **real** faint overlay sampled from `LymphNode.Coloc` (it's the mechanic
   §5c calls load-bearing, and the node's whole "second search arena" only
   makes sense if the player can see the gradient). Sub-questions if yes:
   (a) one soft blob quad (1 renderer, blurry) or a 3×3 tint grid (9
   renderers, shows the gradient's shape) — recommend the blob first; (b)
   max alpha — proposed ≤35%; (c) tint — proposed cyan-white
   `0.55,0.85,0.85`, distinct from helper-T teal and the green match-flash.

4. **Peristalsis cadence & amplitude** (if Option B). Proposed: period
   7–9 s, cross-axis scale ±6% (hard cap ±8% — beyond that the drawn mucus
   band visibly desyncs from the fixed coarse-cell adhesion line, since the
   squeeze is cosmetic only). Whole-channel pulse (cheaper) or a travelling
   band down the lanes (slightly more organic, a bit more cost)? Recommend
   whole-channel.

5. **Blood-cell streamers in the base — erythrocytes only, or also pale
   leukocyte motes?** Recommend **erythrocytes only** — pale motes would be
   confused with the real `SearchUnit`s that already enter from the left
   edge, and those already sell "your cells originate here." Occasional
   platelet flecks are a harmless maybe.

6. **Base field hue.** Proposed deep oxblood `0.26,0.08,0.11` at the outer
   edge lifting to `0.40,0.12,0.15` near the wall. The concern is the
   large-bacterium maroon `0.42,0.12,0.16` — I've argued they never
   co-locate (bacterium is a rimmed rod in the *tissue* band; plasma is a
   flat field in the *base* band) and the plasma is darker, but if you want
   more daylight between them the plasma can go cooler/more crimson
   (`0.24,0.07,0.13`). Confirm the dark oxblood is enough, or push it.

7. **Marrow birth-puffs — ambient trickle or one-per-emission?** Ambient =
   zero engine coupling, purely cosmetic. One-per-emission = a 1-line
   `Action` hook in `BoneMarrowManager.Emit` so a puff fires *because* a
   cell was actually made (tighter feedback, touches one live file).
   Recommend the hook if you're fine with the one-liner.

8. **Should a breach at the base get an acute visual?** Once base cells
   leave the grid (commit 3), a pathogen "reaching the base" just despawns
   and ticks `InvasionTally.ReachedBase`. `GAME_DESIGN.md` §6c warns the
   100-life pool "reads as a difficulty cushion rather than a threat"
   without an acute consequence — and the emergency-granulopoiesis
   mechanic that's meant to supply that is still deferred (`BACKLOG`). A
   red plasma flash / a spreading clot at the arrival lane would at least
   make the *moment* land. Out of scope to build in Sprint 15, but
   `BaseCompartmentRenderer` is where it would hang — want it noted as a
   fast follow?

9. **Gut wall quiet colour.** This spec proposes nudging
   `GutInterfaceRenderer.WallColor` (`0.55,0.47,0.40`) toward the lumen
   mucus tint so the wall reads continuous with the mucus band. It's the
   only shipped colour value this spec would change (everything else is
   additive). Yes, or leave the wall exactly as Sprint 13 shipped it?

10. **`SpriteShapes.Prewarm()` — call it now?** `BACKLOG` (Sprint 13) flags
    it's never called, causing a first-of-kind hitch. This spec adds ~8
    more sprites generated during `Awake`. Recommend finally calling
    `Prewarm()` in `GameBootstrap.Awake` (commit 1) to move the whole
    one-time cost to a single known point. Any objection to that
    unrelated-but-adjacent cleanup riding along?

---

# Sprint 17 revision — the cartoon pass (Director, 2026-09-04)

Sprint 15 built everything above and it shipped. The Director's note after
the first playtest of it:

> "Our next step would be to cartoonify the blood vessel a bit so it's
> visually appealing. Same thing for the lumen. It can look more velvety
> and feature villi instead of just being poo-inspired. That would also
> help contrast with the contaminated food sources flowing by (those can
> be poo-ey)."

That last sentence is the actual argument, and it is a *legibility*
argument, not a taste one. §2.1 painted the lumen brown-olive with ochre
particulate — and the contaminated food bolus that delivers every round is
ochre and lumpy by design (`GAME_DESIGN.md` §5d). The channel and the
threat travelling down it were in one colour family, so the thing the
player most needs to notice arriving was camouflaged against its own
background.

## What changed

### Lumen (§2.1 amended)

| Element | Sprint 15 | Sprint 17 |
|---|---|---|
| Chyme field | `0.22,0.17,0.10` brown-olive | **`0.36,0.22,0.24` mucosal plum** |
| Mucus band | `0.40,0.40,0.30` grey-green @ 0.50 α, order 1 | **`0.88,0.84,0.76` pearly @ 0.20 α, order 2** — a sheen, not a film |
| Flow motes | ochre `0.42,0.34,0.22`, 40 of them, order 2 | **`0.60,0.52,0.40` pale cream, 28, order 4** |
| — | — | **Villi (new)** — `0.80,0.50,0.47` coral, order 1 |
| Food bolus | ochre `0.55,0.47,0.28` | **unchanged — now the only brown thing in the band** |

**Villi** (`SpriteShapes.Villus`, `LumenChannelRenderer.BuildVilli`) are a
fringe of mucosal fingers along the gut wall: one `SpriteRenderer` each,
~0.42 cells wide × ~1.15 cells tall with per-instance height jitter,
spaced ~0.78 cells along the flow, each swaying ±5° on its own phase and
lengthening with the peristaltic squeeze. Direction comes from the axis
frame (the perpendicular pointing away from the tissue band), so a map
with the lumen on the other side still grows them into the channel.

Height is deliberately under the mucus depth: villi are decoration, and a
pathogen adhered at the wall has to stay readable. They draw *under* the
mucus sheen (1 vs 2) so the sheen glazes them — that glaze is what makes
the wall read as wet and velvety rather than as a row of pink fingers.

Lumen sorting order within the band is now **0** chyme, **1** villi,
**2** mucus sheen, **4** motes (above the gut-wall bar at 3, so a mote
that drifts to the seam is never half-swallowed by it).

### Base / vessel (§2.2 amended)

| Element | Sprint 15 | Sprint 17 |
|---|---|---|
| Plasma field | `0.30,0.10,0.13` | `0.33,0.11,0.15` — marginally richer; the oxblood stays, the sepsis framing depends on it |
| Vessel wall | one `VesselWallBar` quad stretched the length of the seam, `0.66,0.44,0.46` | **a tiled row of `EndothelialCell` sprites**, `0.78,0.50,0.52` |
| Erythrocytes | `0.55,0.16,0.18`, shallow dip | `0.72,0.20,0.22`, **deeper dip + darker rim** — a biconcave dish that reads at a glance |

The wall change is the structural one. A single quad stretched across the
seam stretches any detail drawn into it, so that wall could only ever be a
smooth bar — §2.2 said "smoother than `EpithelialBar`: a few faint seams
only" and that was a consequence of the geometry, not a choice. Tiling
one cell sprite per ~0.92 coarse cells keeps the aspect at any board size,
which is what lets the boundary read as *cells* — the things an immune
cell squeezes between on its way out of the vessel.

`VesselWallBar` is left in `SpriteShapes` (unused by the renderer now) —
it costs nothing and a future map may want a smooth vessel somewhere.

### Cost

Villi: ~13 renderers at 25×10, ~51 at 100×40. Wall cells: ~11 and ~44.
Motes: −12. Net ≈ **+12 renderers at 25×10**, all static except the villi
rotation, against the ~110 per-cell renderers this band gave up in Sprint
15. The villi sway loop is one `Quaternion.Euler` and one scale write per
villus per frame, and it early-returns with everything else while
`RoundClock.Frozen`.

### Still not verified by anything automated

Same as every rendering pass: `BootstrapSmoke` proves the shapes generate
and the renderers bind without throwing. Whether the lumen now looks
velvety, whether the villi read as villi rather than as teeth, and whether
the bolus pops against them are the Director's eye.
