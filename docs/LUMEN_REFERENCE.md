# Lumen & Gut-Wall Visual Reference

Status: **reference / proposal, 2026-10-13.** Research for the lumen pass
that follows Sprint 17 (`COMPARTMENT_DESIGN.md`, "Sprint 17 revision").
Nothing here is implemented. Every proposal in section 3 is open for the
Director to accept or reject.

Director's constraint: **scientific cartoons and schematics only.** No
H&E, SEM or endoscopy. All the images below were viewed during research.
**None are committed to the repo.** Most are CC BY / CC BY-SA, so they
would need attribution if we ever did ship one, and we don't plan to. We
take shapes, proportions, layering and colour conventions from them, not
pixels.

---

## 1. Sources

| # | Reference | Shows | Licence | Why it is useful |
|---|---|---|---|---|
| S1 | [Intestinal villus simplified.svg](https://commons.wikimedia.org/wiki/File:Intestinal_villus_simplified.svg) (Snow93) | One villus: hatched surface layer, a red capillary loop with cross-rungs, a central lacteal | Public domain | The most reduced form of the villus core. It is close to what a 12 × 32 px sprite can actually hold. |
| S2 | [Villi & microvilli of small intestine.svg](https://commons.wikimedia.org/wiki/File:Villi_%26_microvilli_of_small_intestine.svg) (BallenaBlanca) | Two villi with a crypt between them, a single row of enterocytes, a zoom inset of the brush border | CC BY-SA 4.0 | Clean **villus : crypt proportion** and how the crypt sits *between* villus bases. Brush border drawn as a comb of short ticks. All monochrome pink. |
| S3 | [Small intestine surface.svg](https://commons.wikimedia.org/wiki/File:Small_intestine_surface.svg) (T. Kebert) | A forest of tall villi with a dark outline. One is cut away to show a red artery, a blue vein, a green lacteal and a capillary mesh. Crypts are short pendant pockets at the bases, then the mucosal and muscle layers. | CC BY-SA 4.0 | Best **"velvet pile" depth**: villi overlap, the back ones are lighter and the front ones darker. Also shows that the interior only needs to be drawn on *one* villus to read. |
| S4 | [Digestive system – Intestinal villi](https://commons.wikimedia.org/wiki/File:Digestive_system_-_Intestinal_villi_--_Smart-Servier.png) (Servier Medical Art) | Two broad U-shaped villi with peach enterocytes, **blue goblet cups**, a brush-border fringe, a **mauve lamina propria**, a green lacteal tree on one villus and a red/blue capillary mesh on the other | CC BY-SA 3.0 (Servier is CC BY 4.0 at [smart.servier.com](https://smart.servier.com)) | The closest match to our "cartoon, outlined, flat colour" register. Shows goblet cells as a contrasting cup every ~5–7 cells. |
| S5 | [OpenStax A&P, Histology of the Small Intestine](https://commons.wikimedia.org/wiki/File:Histology_of_the_Small_Intestine_ku.png) (from [OpenStax A&P 2e §23.5](https://openstax.org/books/anatomy-and-physiology-2e/pages/23-5-the-small-and-large-intestines)) | Textbook diagram with a white epithelium, **orange goblet flecks**, a **yellow-green lacteal**, a red arteriole, a blue venule, and a crypt whose base cells are tinted teal | CC BY 4.0 | The textbook-standard colour code: lacteal yellow-green, artery red, vein blue. Shows the crypt is about 1/3 to 1/2 of villus height. |
| S6 | [Intestinal Villus (NIH BioArt 573)](https://commons.wikimedia.org/wiki/File:Intestinal_Villus_(NIH_BioArt_573).png) (NIAID) | One villus, long axis, with teal columnar epithelium, a grey fuzz brush border, **cream goblet ovals**, a coral stromal core, an open lacteal lumen and many immune cells | CC BY 4.0 | Immune-cell-dense core: macrophages, DCs and plasma cells live *inside* the villus. Shows the stylised option of a non-pink epithelium. |
| S7 | [Paneth cells at the crypt base](https://commons.wikimedia.org/wiki/File:Schematic_diagram_showing_the_localisation_of_matured_Paneth_cells_at_the_base_of_the_intestinal_crypt_and_higher_concentration_of_antimicrobial_peptides_at_the_stem_cell_zone.png) (Luvhengo et al.) | Crypt in BioRender style: a brush-border comb lining the crypt lumen, **Paneth cells as granule-packed wedges at the base**, small triangles for antimicrobial peptides, bacteria at the crypt mouth | CC BY 4.0 | The canonical crypt read: a U-tube with granular cells at the bottom firing small particles upward. Directly relevant to the crypt upgrade. |
| S8 | [Crypt structure & lineage](https://commons.wikimedia.org/wiki/File:A_schematic_illustration_of_the_crypt%E2%80%99s_structure_and_cell_lineage_classification.png) (Du, Nie, Holmes) | A U of rounded-rectangle cells. **Paneth and stem cells alternate at the base**, with goblet cells scattered up the walls. | CC BY 4.0 | The simplest possible crypt: about 12 cells a side and a 5-cell base. It can be drawn with existing `FillDisc`-style primitives. |
| S9 | [Intestinal epithelial cells and their regulatory signals](https://commons.wikimedia.org/wiki/File:Intestinal_epithelial_cells_and_their_regulatory_signals.png) (Kayama & Takeda) | Small intestine vs colon. There is a tall villus with a deep crypt in a **pale-pink lamina propria** with grey immune cells, and a grey **microbiota clump floating well above the epithelium**. | CC BY 4.0 | Confirms the convention that **microbiota sits away from the wall**. Lamina propria is shown as a soft pink gradient. |
| S10 | [B-cell activation in the gut / IgA](https://commons.wikimedia.org/wiki/File:B-cell_activation_and_response_in_the_gut_shape_the_gut_microbiota_through_IgA_secretion.png) (Botía-Sánchez et al.) | A **Peyer's patch dome**: the epithelium bulges into the lumen with no villi on it. **M cells** are pale-blue cells with a basal pocket. A pale-green mucus arc runs over the top, there is a follicle of round B/T cells and DCs below, and green rods and orange cocci sit in the lumen. | CC BY 4.0 | The main reference for the Peyer's patch and M cell look, and for the commensal style (simple capsules and dots). |
| S11 | [M Cell (NIH BioArt 875)](https://commons.wikimedia.org/wiki/File:M_Cell_(NIH_BioArt_875_-_798387).png) (NIAID) | M cell icon: a grey cell whose basal side is scooped into an arch, a "pocket" that cradles a lymphocyte | Public domain | The M cell's whole identity is that pocket silhouette, which makes it trivially drawable. |
| S12 | [Amyloid-producing bacteria and the gut](https://commons.wikimedia.org/wiki/File:Potential_areas_of_interaction_between_amyloid-producing_bacteria_and_the_gut.png) (Friedland & Chapman) | Flat-colour strip: a **pale-mint mucus layer** with green rods and blue cocci, yellow enterocytes with a comb brush border, and a Peyer's patch as a purple ellipse under an M cell | CC BY 4.0 | Shows mucus drawn as a translucent band with bacteria *in* it. A strong precedent for our pearly sheen. |

Found but not viewed or used, because they are proprietary or paywalled:
the BioRender "intestinal villus vasculature" icon, Mind the Graph's
villus illustrations, and Nature Reviews figures (PMC blocked automated
fetching). S4, S7 and S10 are already BioRender or Servier in style.

---

## 2. Recurring conventions, structure by structure

**Villi.**
- Shape: a finger with parallel sides and a rounded dome tip, not a
  triangle or a tooth. Every source draws the tip as a semicircle.
- Aspect ratio runs from about 1 : 1.5 (Servier S4, broad U) to about
  1 : 5 (Kebert S3, tall forest). Textbook diagrams sit around 1 : 3.
- Spacing: the gap between neighbours is narrow, about 0.3–0.8 × the
  villus width. Villi are a crowded pile, not a sparse comb.
- Outline: a dark rim (S3, S4) does most of the readability work.
- Depth: when there are many villi, back ones are lighter and lower
  contrast, and front ones overlap them (S3).
- Interior: the vessels are drawn on **one villus in a row**, never on all
  of them (S3, S4). The rest show only a faint core.

**Epithelium / enterocytes.**
- One row of tall rectangles along the villus outline, each with a dark
  nucleus dot near the *basal* side.
- Colour is usually peach or pink (S2, S4). Stylised alternatives are
  teal (S6) and yellow (S12).
- At small scale the row collapses into a lighter **rim band** around a
  darker core. That rim-vs-core value split is the convention that
  survives downscaling.

**Brush border (microvilli).**
- Drawn universally as a **comb of short, dense ticks** on the apical
  (lumen-facing) edge, dark on light (S2, S4, S7, S12).
- In zoomed-out views it becomes a fuzzy or hairy outline (S6).
- It is never drawn as individual microvilli at villus scale. It is a
  texture on the edge.

**Goblet cells.**
- A wine-glass or cup shape in the epithelial row. The rounded mucin
  "theca" sits toward the apex and is filled with pale droplets.
- Signalled by **contrast colour**: blue (S4), cream/white (S6), yellow
  (S8), salmon with droplets (S7), orange (S5).
- Frequency is about 1 in 5–8 epithelial cells, scattered irregularly.
- There is no fixed hue convention. What matters is that the cup is pale
  or contrasting against the enterocytes.

**Mucus.**
- A translucent pale band over the apical surface: pale mint, very light
  green or grey-blue (S10, S12).
- The small intestine has **one loose, patchy layer**. The two-layer
  firm/loose arrangement is a colon convention, so don't draw two layers.
- Bacteria sit *in the outer part or above it*, and the band next to the
  cells is cleaner.

**Lamina propria.**
- The villus core. It is drawn as a flat wash darker or cooler than the
  epithelium: mauve or lavender-grey (S4), soft pink gradient (S9) or
  coral stroma (S6).
- Immune cells are drawn inside it as round dots (S6, S9). That fits our
  story, because tissue is where the fighting happens.

**Capillaries and lacteals.**
- Capillaries: **red arteriole up one side, blue venule down the other,
  with a mesh or ladder between** (S1, S3, S4, S5).
- Lacteal: a **single central blind-ended tube** coloured yellow-green or
  green (S3, S4, S5), cyan (S1), or white/cream in "chyle" framing. The
  capillary loop wraps *around* it.

**Crypts of Lieberkühn.**
- Narrow U-tubes sunk *between* villus bases, into the tissue rather than
  the lumen.
- Depth is about 1/3 to 1/2 of villus height (S2, S5). Width is about
  1/2 of villus width (S2).
- The base is marked by **granular Paneth cells**, drawn as bright
  orange/gold granules or a distinct colour (S7, S8), alternating with
  small stem cells. Antimicrobial peptides are drawn as tiny triangles
  rising up the crypt lumen (S7).

**Peyer's patch and M cells.**
- A **villus-free dome** where the epithelium bulges gently into the
  lumen (S10).
- Under the dome is a cluster of round lymphocytes, with a B-cell
  follicle core and DCs on its flanks.
- M cells sit in the dome epithelium and are drawn as a lighter or
  different-coloured cell with a **pocket scooped out of the basal side**
  holding a lymphocyte (S10, S11). They have no brush border, or a
  stubby one.

**Commensal microbiota.**
- Simple capsules (rods) and dots (cocci) in 2–3 flat colours, typically
  green rods with blue or orange cocci (S7, S10, S12), or a grey clump
  (S9).
- Always several together, sitting in the mucus or the lumen, not on the
  cells.

---

## 3. Proposals for this game (ranked by visual payoff against effort)

### Constraints that shape every proposal

- **Orientation.** The lumen is the right-hand band and flows top to
  bottom. The gut wall (`GutInterfaceRenderer`, order 3) is the seam on
  the lumen's left edge. Villi already grow from it into the channel.
  Crypts belong on the *tissue* side of that seam. The tissue band is
  the per-cell host grid at order 0, and its cells carry game state.
- **Pixel budget.** At the full 100 × 40 board, one coarse cell is about
  28 px. A villus (0.42 × 1.15 cells) is roughly **12 × 32 px**. The
  readability floor in `UI_STYLE_GUIDE.md` is 14–22 px for whole
  entities. Any interior detail must be **at least 2–3 px**, or it
  aliases into noise. A 1 px brush-border comb won't survive.
- **White-multiply sprites.** `SpriteShapes` draws white with the shape
  in alpha, and `sr.color` tints it. Inside one sprite we can only vary
  **value**, not hue: a rim brighter than a core, or a pale spot. Any
  **second hue** (a red capillary on a coral villus) needs a second
  `SpriteRenderer`. That doubles renderer cost for that element.
- **Palette guardrails.**
  - Ochre/brown is reserved for the food bolus (`0.55,0.47,0.28`).
  - Blue means macrophage (`0.30,0.40,0.80`).
  - Gold means neutrophil (`0.93,0.74,0.30`).
  - Maroon means bacterium (`0.42,0.12,0.16`).
  - Cold purple means virion (`0.40,0.16,0.34`).
  - Magenta means DC.
  - Bright green means the knowledge-match flash.
  - So two textbook conventions must be **broken on purpose**: no blue
    venules, and no yellow-green or yellow lacteal. Yellow-green is the
    bacterial-infected host tint (`0.62,0.60,0.26`), and yellow sits next
    to ochre.

### P1. Villus rim / core value split + soft brush-border halo (high payoff, low effort)

- **What:** redraw `SpriteShapes.Villus` so the outer ~2 px band (the
  epithelium) is at full value. The interior (lamina propria) drops to
  about 0.78 value, and the existing darker central stripe (the lacteal
  core) stays.
- **Brush border:** add a soft 1–2 px alpha **feather** outside the
  outline. A fuzz, not a comb, so the edge reads hairy and velvety under
  the mucus sheen.
- **Band and layer:** lumen, order 1 (unchanged).
- **Palette:** unchanged coral tint `0.80,0.50,0.47`. The value split
  produces a peach rim and a darker mauve-coral core, which matches S4.
- **Cost:** texture-only. 0 extra renderers and 0 per-frame cost.
- **Gameplay legibility:** a slight gain. A lit rim separates each finger
  from the plum channel, so adhered pathogens at the wall read against a
  defined edge rather than a blur.

### P2. A back row of villi, the "velvet pile" (high payoff, low effort)

- **What:** a second row of villi offset by half a spacing, about 80%
  of the front row's height. It is darker and lower contrast: tint
  roughly `0.62,0.40,0.42` at 0.7 α. It has its own slower sway phase.
- **Why:** this is S3's depth cue, and the cheapest route to the
  "velvety" quality the Director liked.
- **Proportions:** keep front-row spacing at about 0.78 cells. The gap
  between neighbours stays at most 0.8 × the villus width, per
  convention.
- **Band and layer:** lumen. It must draw between the chyme (0) and the
  front villi (1). Either give it order 1 with a small positive
  `transform.z` behind the front row, or renumber the lumen orders. That
  needs a decision, because the sorting table in `UI_STYLE_GUIDE.md`
  would change.
- **Cost:** +13 renderers at 25 × 10 and +51 at 100 × 40, the same sway
  loop, and frozen with `RoundClock.Frozen`.
- **Gameplay legibility:** neutral, as long as the back row stays darker
  than the front. It must not lift the background value behind pathogens.

### P3. Goblet-cell flecks on villi, tied to the mucus sheen (medium payoff, very low effort)

- **What:** 1–2 small pale oval "cups" (about 3 × 4 px at full scale)
  baked into the villus texture at random-looking heights on the rim
  band, at about 1.0 value against the 0.9 rim.
- **Variants:** add a second texture variant (`Villus` / `VillusAlt`)
  with the flecks placed differently, so the row doesn't repeat.
- **Why:** goblet cells are the source of the mucus sheen, so this links
  the villi to the glaze visually. It echoes the existing goblet flecks
  on `EpithelialBar` (COMPARTMENT_DESIGN §2.5).
- **Band and layer:** lumen, order 1.
- **Palette:** comes out as a pale coral highlight under the tint. If a
  true pearly `0.88,0.84,0.76` is wanted, that is a second renderer, and
  it isn't worth it.
- **Cost:** texture-only.
- **Gameplay legibility:** none.

### P4. Crypt sprite for the crypt upgrade (high payoff, medium effort, and gameplay-serving)

- **What:** a `SpriteShapes.Crypt`, a narrow U-pocket that opens through
  the gut-wall seam and sinks into the tissue side.
- **Proportions:** about 0.35 cells wide × 0.6 cells deep. That is
  roughly half a villus wide and half a villus deep, the S2/S5
  proportion.
- **Drawn on purchase only**, never as ambient decoration, so a crypt on
  screen always means "you bought this".
- **Two renderers:**
  1. The pocket outline, in a light tissue pink close to `HostCell`
     (`0.86,0.68,0.70`).
  2. **Paneth granules**: a 3–4 dot cluster at the base in a bright rose
     `0.95,0.60,0.66`. They are eosinophilic in reality, and this keeps
     them clear of neutrophil gold, ochre and debris brown.
- **Optional activity cue:** while regrowth is accelerating inside the
  crypt's radius, emit 1–2 tiny pale motes upward out of the crypt mouth
  (S7's antimicrobial-peptide triangles). Pool them. This is the "it is
  working" cue.
- **Band and layer:** it straddles the wall. The pocket draws at order 2
  so it sits over tissue cells but under the wall bar (3). It should
  cover at most the outer ~0.4 of the first tissue column, so the state
  colours of those host cells stay visible.
- **Gameplay legibility:** strong. It marks where the regrowth bonus
  lives. Show the regrowth radius as a faint `Accent` ring only on
  hover or selection, never always on.
- **Design question:** GAME_DESIGN §1d says crypts are "placeable" in
  tissue. Biologically, crypts exist only at the epithelial surface. The
  cartoon reads best if placement **snaps to the wall seam**, with a
  radius reaching into tissue.

### P5. Capillary loop in a highlighted villus (medium payoff, medium effort)

- **What:** following S3/S4, draw the vessels in **only 1 villus in
  6–8**, not all of them.
- **Drawing:** a thin (2 px) red hairpin loop as a child
  `SpriteRenderer` up the inside of the rim, tint `0.80,0.30,0.34` at
  0.8 α. The lacteal stays the pale central stripe from P1.
- **No blue venule.** Blue is reserved for the macrophage.
- **Why:** it ties the lumen to the base vessel's red, and it tells the
  "absorbed things go to the blood" story at a glance.
- **Band and layer:** lumen, order 1, as a child of its villus so it
  inherits the sway.
- **Cost:** +2 to +7 renderers.
- **Gameplay legibility:** none.
- **Lacteal colour:** if the lacteal ever gets its own hue, use milky
  white-lavender `0.92,0.90,0.94` for "chyle" rather than the textbook
  yellow-green, for the reasons in the guardrails.

### P6. Peyer's patch dome with M cells (high thematic payoff, high effort, design-coupled)

- **What:** 1–2 stretches of the wall, each about 3–4 cells long, where
  **no villi grow**.
- **The dome:** the wall bulges about 0.5 cells into the lumen there as
  a low dome, drawn as a smooth arc in the epithelium colour.
- **M cells:** 3–4 cells on the dome, shown as notches in a lighter
  lavender-grey `0.78,0.74,0.82`. Each has a darker basal "pocket" dot
  (S11's silhouette).
- **The follicle:** a soft round cluster of pale lymphocyte-teal dots,
  echoing the node's `Lymphocyte` `0.32,0.72,0.70` at low α, on the
  tissue side.
- **Why:** it is the real biological site of antigen sampling. A dome
  is also the best on-board signpost for **where DCs pick up antigen**
  in the DC loop. If DCs ever preferentially sample there, this is the
  place.
- **Band and layer:** dome and M cells in the lumen at order 1. The
  follicle on the tissue side at order 2 must stay translucent, because
  it covers tissue cells.
- **Effort and risk:** high. It needs the wall renderer and
  `PathogenAgent.InterfaceStackWorldPosition` to agree on the bulge.
  The amplitude cap that applies to the peristaltic squeeze (±8%)
  applies here too: the drawn wall must not desync from the adhesion
  line.
- **Recommendation:** make this a decision point for the Director, not
  a decoration task. If it carries no gameplay meaning, a villus-free
  gap with a gentle dome and nothing else is still a nice landmark that
  breaks up the repeating fringe.

### P7. Commensal microbiota in the outer mucus (medium payoff, medium effort, readability risk)

- **What:** about 15–20 pooled tiny shapes. They are rods of about
  0.18 × 0.07 cells (≈ 5 × 2 px) and cocci of ≈ 2–3 px, in small loose
  groups of 2–4.
- **Placement:** they sit in the **outer half of the mucus sheen**,
  never touching the villi (the clear-inner-zone convention). They
  drift at about 1/3 of mote speed with a little jostle.
- **Band and layer:** lumen, order 2 alongside the mucus. They draw
  under the motes (4) and far under pathogens (20).
- **Palette:** desaturated sage-grey `0.60,0.68,0.62` at 0.55 α. This
  is deliberately the lowest-contrast thing in the lumen, cooler and
  greyer than anything on the threat list.
- **Readability risk:** players may read them as enemies. Mitigations:
  - keep them at least 4× smaller than a `LargeBacterium`;
  - keep them low-alpha;
  - keep them clustered;
  - never put them on the tissue side.
- **Optional gameplay hook (needs a design call):** commensal density
  near a wall position could thin as pathogens adhere there. That would
  be a visual "colonisation resistance is failing" cue. Don't build it
  without a mechanic behind it.

### P8. Mucus sheen: a soft outer edge (low payoff, trivial effort)

- **What:** give `MucusBand` a faint brighter line (about 1.08 value) at
  the point where its feather starts. This reads as the outer surface of
  the loose layer and as the line where commensals stop.
- **Keep it one layer.** The small intestine has one mucus layer (§2).
- **Band and layer:** lumen, order 2.
- **Cost:** texture-only.

---

## 4. What to avoid

- **Full textbook detail on every villus.** No per-villus vessels,
  nuclei rows or brush-border combs. Sources draw interiors on one
  villus in a row, and so should we. A uniformly detailed fringe turns
  into a barcode at 28 px per cell and fights the pathogens adhered at
  the wall.
- **Hue drift into reserved colours.** No blue venules (macrophage). No
  yellow, yellow-green or tan lacteals (bacterial-infected tint and the
  ochre bolus). No gold Paneth granules (neutrophil). No magenta goblet
  cells (DC). The lumen should stay a coral, plum and pearl family with
  one red accent.
- **Villi taller than the mucus depth.** Sprint 17 kept villi under it so
  wall-adhered pathogens stay readable. P2's back row and P6's dome must
  respect the same ceiling.
- **Decorative anything on the tissue cells.** The host grid is
  load-bearing state: pink, violet, yellow-green, debris and pit. Crypts
  and the Peyer's follicle may only *edge into* the first column, must
  stay translucent, and should exist only where they mean something (a
  purchase, or the antigen-sampling site).
- **Ambient particles that look like threats.** Commensals and crypt
  peptide motes must be smaller, greyer and lower-alpha than any
  pathogen. The bolus remains the only brown, lumpy, big thing in the
  channel.
- **Two mucus layers, thick slime, or opaque green mucus.** These are
  the colon convention and the "poo-inspired" look the Director moved
  away from.
- **More motion.** Villi already sway and the channel squeezes. New
  elements should be static (crypts, dome, flecks) or very slow
  (commensals), so the speed-up control doesn't strobe
  (`handoff-map01-intestine.md` open question 1).
- **Copying textures or shipping the reference images.** The licences
  above allow it with attribution, but procedural art is the project's
  rule (`SPRITE_DESIGN.md` §4).

## 5. Suggested order

1. **P1 + P3** together: one `Villus` texture rewrite plus a variant.
2. **P2**: the back row, which needs the sorting decision.
3. **P8**: the mucus edge.
4. **P4**: when the crypt upgrade is scheduled, since it is the one with
   a gameplay job.
5. **P5**: vessel highlights.
6. **P7**, then **P6**: both need a design call first.

All of these pass `BootstrapSmoke` trivially. As with Sprint 17, only the
Director's eye can judge them, so `Assets/Editor/SpriteShapeDump.cs`
should be used to check each new silhouette at 1× and at 12 × 32 px
before playtest.
