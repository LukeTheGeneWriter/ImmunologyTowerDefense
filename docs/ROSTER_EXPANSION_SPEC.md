# Roster expansion — ten organisms, their sprites, and their warnings

Director's ask, 2026-11-02: *"ID 10 more common gut pathogens in humans.
Design sprites and warning messages. Some are just stronger bacteria or
viruses, others have some special power, resistance, etc."*

Assembled from five parallel research passes run the same day
(`docs/research/enteric_burden.md`, `pathogens_protozoa.md`,
`pathogens_helminths.md`, `pathogens_bacteria_common.md`,
`pathogens_viruses_02.md`). Those are the evidence; this is the design.
`GAME_DESIGN.md` wins on any conflict. **Nothing here is built** — it is a
spec awaiting the Director decisions in §6.

Companion to `PATHOGEN_ROSTER.md`, which stays the record of *why each
organism earns a slot*. This doc adds the two things that one never had:
**what each organism looks like** and **what the player is told before it
arrives**.

## 0. What changed about the selection rule

The roster's rule is:

> **A species earns a slot by forcing a decision no existing enemy forces.**

The Director has now relaxed it deliberately: *"some are just stronger
bacteria or viruses."* That is a real design instruction and it closes a
hole the rule had. Two of the world's most common enteric pathogens —
**sapovirus** (#2 globally by attributable episodes) and **ETEC** (#5) —
are mechanically redundant with organisms already built, and the old rule
excluded them *because* they are ordinary. A game about the gut that omits
the second most common thing in the gut has its priorities from the wrong
axis.

So this set is **eight mechanic slots plus two tier slots**, and the tier
slots are labelled as such rather than given an invented special power.
This is also the Bloons habit: red → blue → green is not four mechanics,
it is one mechanic at four strengths, and the game is better for it.

## 1. The ten

| # | Organism | Kind | The slot it earns | Burden |
|---|---|---|---|---|
| 1 | **Norovirus GII.4** | virus | **Your knowledge is not permanent.** ×0.3 to that species' knowledge on a variant replacement, hard floor at the ~10% CTL rung | #1 high-income: 5.5M US illnesses/yr; GBD PAF 10.2% of all-age diarrhoeal deaths |
| 2 | **Astrovirus** | virus | **Two threats, one organism** — a visible infected cell and an invisible apical breach that works with no replication at all, and keeps working on virions the player's antibody has already neutralised | MAL-ED 15.0 attributable episodes/100 child-yr |
| 3 | **Sapovirus** | virus | **TIER — a stronger generic virus.** No new mechanic, by design | **#2 globally**, 22.8/100 child-yr, above norovirus's 15.4 in the same study |
| 4 | ***Clostridium perfringens* type F** | bacterium | **The crowd arms itself if you let it sit.** CPE is not preformed — it must sporulate in the lumen first. Generation time 7.1 min at 41 °C. PFO kills phagocytes | 966k US illnesses/yr, the US's #2 bacterial food poisoning |
| 5 | ***Staphylococcus aureus*** enterotoxin | bacterium | **The attack already happened.** The toxin is in the food; killing the organism achieves nothing | ~241k US illnesses/yr |
| 6 | **STEC O157:H7** | bacterium | **Damage aimed past the wall** — human gut epithelium lacks Gb3, so Shiga toxin translocates and hits what is behind it | FoodNet 5.3/100k lab-confirmed |
| 7 | **ETEC** | bacterium | **TIER — a stronger generic bacterium.** Adheres harder, invades nothing | #5 globally, 18.8/100 child-yr |
| 8 | ***Giardia duodenalis*** | protozoan | **No debris, no inflammation — and the player's own CTL deals the damage.** Microvillous shortening transfers with CD8⁺ cells, not CD4⁺. Antibody *triggers* the antigenic switch rather than killing | the most common protozoan in the small bowel; magnitude of the quoted "300M" is UNVERIFIED |
| 9 | ***Cryptosporidium hominis*** | protozoan | **The infected cell is harder to kill than its neighbours**, and the clock cannot be changed — exactly 3 asexual generations, then obligate sex, independent of environment | ID50 132 oocysts; 35.1% of US outbreaks from treated recreational water |
| 10 | ***Cyclospora cayetanensis*** | protozoan | **No feedback loop** — oocysts are shed unsporulated and need 7–10 days outside a host, so nothing the player does can change next round's arrivals. The exact mirror of #9 | seasonal, imported-produce outbreaks |

**Three kingdoms, and the eukaryotes are new to this game entirely.** The
roster had no protozoan and no helminth; four earlier passes never looked.

### What this set deliberately does not include

- **Nontyphoidal *Salmonella*** — the top runner-up, and more common than
  five organisms on the list (1.03M US illnesses, highest foodborne DALYs
  of 22 diseases). Held back because its mechanic ("two speeds in two
  niches", plus the roster's own correction #1: the infected enterocyte is
  *extruded into the lumen* rather than bursting into tissue) is a change
  to §4a's advance rules, not a species addition. It should lead its own
  sprint.
- ***Strongyloides stercoralis*** — autoinfection is real, and it would be
  **the board's first spawner**: an enemy that re-seeds the wave with no
  new luminal dose. Held back because helminths need larvae-as-waves plus
  adult-as-terrain (`pathogens_helminths.md` §2), which is a sprint, not a
  slot.
- ***Vibrio parahaemolyticus*** (bile-induced T3SS2 — the host's own
  anatomy arms it) and **microsporidia** (the polar tube fires at
  300 µm/s and ingresses *alone*, which violates §1b's "ingress is
  something a crowd does"). Both are good; both need a Director ruling
  first.
- **Formerly banked on anatomy:** *Shigella*, *Campylobacter*, *Entamoeba
  histolytica*, *Trichuris*. **No longer excluded** — the Director relaxed
  the anatomy rule on 2026-11-02 (§6.1). They are not in this ten because
  the ruling came after the set was chosen; they are the natural head of the
  *next* list, and *Entamoeba* in particular answers two standing gaps.

## 2. The sprite system

### 2.1 The problem

`PathogenAgent.cs:390` and `:1330` select the sprite from **class**:

```csharp
sr.sprite = Class == PathogenClass.IntracellularVirus
    ? SpriteShapes.Virion : SpriteShapes.LargeBacterium;
```

Two sprites for the whole roster. Rotavirus, *Vibrio cholerae* and A/E
*E. coli* have rendered as the generic ball and the generic bean since
Sprint 24. **A per-species sprite path does not exist and has to be built
before any of this is visible** — a `SpriteShapes.ForSpecies(PathogenSpecies)`
dispatch plus a per-species palette entry, and the three shipped organisms
need sprites too, not only the ten new ones.

`KnowledgeView.SpeciesIcon` has the same shape of bug today: it switches
`case 0 / case 1 / default`, so **rotavirus draws as a bacterium** in the
knowledge HUD, tinted virion-violet.

### 2.2 Three channels, because colour is already full

The palette has exactly two pathogen hues (`Palette.Bacterium` green,
`Palette.Virion` violet). Blue, yellow, magenta, teal, lime, lilac,
orange, red, aqua and brown are all already *meanings* on this board.
**Colour cannot be the species channel.** Identity is carried by:

1. **Silhouette family = the kingdom**, i.e. what kind of answer applies.
   - **Knobbed ball** — virus, visual size 0.6 cells (unchanged).
   - **Chunky 2:1 bean** — bacterium, 0.55 cells (unchanged).
   - **Large lobed/teardrop body** — eukaryote, **0.7–1.1 cells**. A
     *Giardia* trophozoite is 15.0 ± 1.2 µm long against a ~2 µm
     bacterium, so it is legitimately **the largest thing in the lumen**.
     Size is the strongest channel available and it costs nothing.
2. **One "tell" = the mechanic, not the taxonomy.** A silhouette modifier
   that survives ~20 px. The player needs to read *"the obvious answer
   will not work on this one"* in a crowd, and that is a mechanical fact,
   not a phylogenetic one.
3. **Hue varies only within the family** — violet family for viruses,
   green family for bacteria, and one new hue for eukaryotes (§6.3).

ART_DIRECTION forbids the easy routes and is right to: **flagella vanish
under 20 px** (§3.3, "drop flagella"), and **long thin arms belong to the
dendritic cell alone**. Every tell below is a disc, a ring, a pip, a
notch or a star.

### 2.3 The tell alphabet

Eight symbols, each meaning one thing everywhere it appears:

| Tell | Means | Primitive |
|---|---|---|
| **Additive bright star** | acts without infecting anything | `FillStar(points=6)` light over the body |
| **Subtractive pits, ragged rim** | calicivirus family | `InnerShade` discs + lobed rim |
| **Bright inner pip** | carries a toxin that acts on its own | `InnerShade(..., mul>1)` |
| **Broken halo ring (one gap)** | your antibody or knowledge does not hold | `FillRing` + erase an arc |
| **Solid dark anchor ring** | fixed in place, resists removal | `FillRing` dark at the base |
| **Cluster of small bodies** | the organism is not the threat | several `FillDisc` |
| **Chain / doubled body** | it multiplies fast | two `FillCapsule` end to end |
| **Counted inner bodies** | eukaryote cyst stage; the count is the species | `FillCapsule` commas in a disc |

### 2.4 The rule that falls out: the tell goes where the player must look

Three of these ten put their tell on something **other than the
organism**, and that is correct rather than a compromise:

- **Astrovirus's breach** is invisible by construction — it is drawn on
  the **wall** (an apical leak shimmer), never on the virion. Drawing it
  on the enemy would delete the mechanic.
- ***S. aureus*'s toxin** is already in the food — it is drawn on the
  **bolus**. The round's warning is a property of the meal, not of a
  creature.
- ***Cryptosporidium*'s resistance** belongs to the **infected host
  cell** (a hardened double outline), because that is the thing the
  player's CTL will fail against.

## 3. Per-organism sprite specs

Drawn at 64×64, white silhouette in alpha, tinted per instance by
`SpriteRenderer.color` (so hue is free and shape is not). `C` = 32.

**1. Norovirus GII.4** — `FillLobed` body r≈15 with a slightly **feathery
rim**, three `InnerShade` round pits, and a **dark-centred six-point
star**. Madeley's 1979 side-by-side is the source and the discriminators
are exact: calicivirus = feathery edge, **dark**-centred star, **round**
hollows. Tell: **broken halo ring** (knowledge slips). Hue: violet.
Size 0.6.

**2. Astrovirus** — a **clean, unbroken circular rim** (not lobed, not
knobbed: that is the discriminator against norovirus) with an **additive
bright six-point star, white at the centre**, plus **triangular** surface
hollows. `FillDisc` + `FillStar(points=6)` light fill. Size 0.6, hue
violet shifted blue. **Recorded deliberately:** the star is the
alkaline-treated, *minority* EM appearance; the cryo-EM infectious virion
is a 30-spiked ball ~44 nm across, and 30 spikes at 20 px read as
identical to 8 knobs. The game is choosing the icon over the structure
knowingly — `pathogens_viruses_02.md` §2 logs this as a correction to the
art brief, and it belongs in `ART_DIRECTION.md` §3.3 as a decision rather
than an error.

**3. Sapovirus** (tier) — same calicivirus family as norovirus, drawn as
the **plain** version: feathery rim and the dark-centred star, **no pits
and no tell**. It should read as "the virus you know, more of it." Hue:
deeper violet. Size 0.6.

**4. *C. perfringens* type F** — a **blunt rod in end-to-end chains**
(two `FillCapsule` in line, ~3:1 overall), with a **bright spore pip** at
one pole. It really does grow in chains, and the spore is the mechanic:
the pip appears **only once the organism has settled**, so the sprite
*changes on the board* when it arms. Size 0.75 long. Hue: olive-green,
away from the neutrophil's yellow.

**5. *S. aureus*** — a **cluster of 4–5 small discs** (grape-like cocci,
0.5–1.5 µm each), instantly not-a-bean at 20 px. No tell on the organism;
the tell is the **bolus toxin halo**. Hue: desaturated sage. Size 0.6.

**6. STEC O157:H7** — bean plus a **wedge pointing away from the wall**
(damage aimed past it) and a **bright inner pip** (Shiga toxin). Size
0.58, hue green.

**7. ETEC** (tier) — the generic bean at **0.62** with a thicker stroke
and a **solid dark anchor ring** at one pole. Beefier and stickier, and
nothing else. Hue: deeper green.

**8. *Giardia duodenalis*** — **inverted-pear / teardrop**, 15.0 × 7.5 µm,
drawn with the **ventral disc as a large inner ring** (`InnerShadeRing`),
which reads as a suction cup at 20 px. Two nuclei as two small inner
discs if they survive the scale; the 8 flagella do not and are dropped
per ART_DIRECTION. Tell: **broken halo ring** — antibody clusters VSPs and
triggers the switch rather than killing. **Size 1.1 cells: the largest
pathogen on the board.** Second form: the **cyst**, an oval with a thick
double outline.

**9. *Cryptosporidium hominis*** — oocyst as **four comma/banana
sporozoites inside a near-perfect circle**, with *no* inner sporocyst
walls (that absence is what separates it from #10). At 20 px the only
readable cue is *four dots in a circle*, so that is the cue. Size 0.7.
Second form: the **intracellular stage as a blister sitting on top of the
brush border** — not buried in the cell, because the parasitophorous
vacuole stays in the brush-border zone and its wall carries the host's own
villin and ezrin — with a **dark anchor ring** at its base (sourced: a
secreted ring anchors the parasite into its epicellular niche). The
apoptosis-resistance tell goes on the **host cell**, per §2.4.

**10. *Cyclospora cayetanensis*** — same oocyst family, distinguished by
**two sporocysts each holding two sporozoites** (two pods, not four
commas). Described contemporaneously as "a large *Cryptosporidium*", so
draw it **larger**: size 0.8. Oocyst dimensions are UNVERIFIED; the
internal structure is the sourced discriminator.

## 4. Warning messages

### 4.1 What exists

`HudView` already shows, in the buy phase, the next round's tagline plus
`RoundScript.DebutLabelFor` → `"NEW: Vibrio cholerae"`. That is enough for
an organism whose threat is legible, and not enough for any of these ten.

### 4.2 The three-line debut card

Shown in the buy phase, the round before the organism first arrives:

```
   Imported raspberries, basil and cilantro
   ┌──────┐
   │ (••) │   NEW — Cyclospora cayetanensis
   └──────┘
   Nothing you do here changes how many arrive next time.
```

1. **The vehicle** — the real-world thing a person eats, drinks or
   touches. Vivid and specific, and it is also the round tagline.
2. **The name, with the sprite at large size.** This is the only place the
   player sees the silhouette big enough to learn it; afterwards they must
   recognise it at 20 px in a crowd, which is what §2 is for.
3. **The one thing that will not work** — stated as a negative.

### 4.3 Why the third line is a negative

Every organism in this set that has a special power has it *against an
answer the player already owns*. A positive hint ("use IgA") deletes the
puzzle. A negative warning ("your antibodies will slip") keeps the puzzle
and still plays fair — it tells the player their habit is about to fail
without telling them the replacement. This is exactly the Bloons
convention for the lead bloon: the game says *sharp will not pop this*,
and lets the player find the rest.

The tier organisms get a negative too, and theirs is honest about being
ordinary.

### 4.4 The lines

| Organism | Vehicle (tagline) | The warning |
|---|---|---|
| Norovirus GII.4 | "A vomiting incident in a restaurant dining room" | "What you know about it will slip. What your killer T cells know will not." |
| Astrovirus | "A daycare changing table in autumn" | "The wall will leak where nothing looks sick." |
| Sapovirus | "A catered lunch, the same as last week" | "Nothing new here. Just more of it." |
| *C. perfringens* | "A rotisserie chicken held warm since noon" | "Harmless until it settles. Then it arms itself — and your phagocytes die of what they eat." |
| *S. aureus* | "A cream cake left out at a church picnic" | "The toxin is already in the food. Killing these will not stop it." |
| STEC O157:H7 | "An undercooked burger, pink in the middle" | "This one is not aiming at the wall." |
| ETEC | "Tap water in a town you are visiting" | "A familiar shape, in unfamiliar numbers." |
| *Giardia* | "A mouthful of stream water on a hiking trip" | "No debris, no fever — and your T cells will do the damage for it." |
| *Cryptosporidium* | "A swimming pool somebody got into while ill" | "The cells it takes will refuse to die." |
| *Cyclospora* | "Imported raspberries, basil and cilantro" | "Nothing you do here changes how many arrive next time." |

### 4.5 First sighting, in round

A small banner the first time a species is seen on the board in a run,
carrying the sprite and the name only. The knowledge HUD already holds one
card per species, so the debut card teaches, the banner confirms, and the
HUD tracks — three surfaces, one fact each.

## 5. The engineering gates

Measured this session, not estimated. Each one blocks the build.

### 5.1 The antigen barcode must widen to 12 bits

Greedy construction with randomised restarts, against the Hamming
sphere-packing ceiling:

| bits | d=5 (r=2) constructed | ceiling | d=7 (r=3) constructed | ceiling |
|---|---|---|---|---|
| 10 | **12** | 18 | 2 | 5 |
| 11 | 16 | 30 | 4 | 8 |
| 12 | **20** | 51 | 4 | 13 |
| 14 | 64 | 154 | 16 | 34 |

Ten bits admits **12** species at the shipped match radius. The roster
becomes **16**, which does not fit. Eleven bits is the minimum and
**12 is the choice** (20 slots, room to keep going). This must land
*before* any new species gets a tag, or we repeat the 8-bit mistake where
21.9% of helper-T tags taught two species by accident.

**A generated 16-tag set at 12 bits, minimum pairwise distance 6** — the
same separation the shipped 6-species set has, verified across all 120
pairs:

```csharp
GenericVirus                  0b000011011001
GenericIntracellularBacterium 0b000110110111
GenericLargeBacterium         0b001000010100
Rotavirus                     0b010000100001
VibrioCholerae                0b010110001100
AttachingEffacingEcoli        0b011011000111
NorovirusGII4                 0b011100111010
Astrovirus                    0b100000101110
Sapovirus                     0b100101000101
ClostridiumPerfringens        0b101001110011
StaphAureusToxin              0b101110100000
StecO157                      0b101111011110
Etec                          0b110010010010
Giardia                       0b110111101011
Cryptosporidium               0b111001001000
Cyclospora                    0b111010111101
```

**Correction to a standing comment.** `SpeciesProfile.Antigen` claims the
shipped set "leaves headroom for the avidity upgrade to raise r to 3". It
does not — the set sits 6 apart and r=3 needs 7. At 10 bits, r=3 admits
exactly **two** species, and that is a Plotkin bound, not a search limit.
`AdaptiveTuning.MatchMaxHammingDistance`'s own comment has this right and
says the cross-teaching is deliberately bought. The two comments
contradict each other; the `Antigen` one is wrong. **The 12-bit set above
inherits the same property** — it is fine at r=2 and cross-teaches at r=3,
which is now a stated consequence of buying avidity rather than an
accident.

### 5.2 The knowledge HUD does not scale

`KnowledgeView` builds a fixed 420 px ribbon of 186 px cards. Six species
already overflow it; sixteen would be ~3,000 px. Recommendation: **the
ribbon shows only species seen this run**, which also teaches better — the
player's ledger grows as their run does.

### 5.3 Per-species sprites need a dispatch

See §2.1. `SpriteShapes.ForSpecies` + a per-species palette entry, and the
three shipped organisms get sprites in the same pass.

### 5.4 Two-form organisms are new

Four of the ten change silhouette on the board: *C. perfringens* (arms
itself), *Giardia* (trophozoite / cyst), *Cryptosporidium* (oocyst /
blister), and arguably astrovirus (virion / wall leak). Today a pathogen's
sprite is set once at spawn. This is a small change and worth doing once,
deliberately, rather than four times.

## 6. Director decisions needed

### 6.1 The anatomy rule — DECIDED: colonic is fine (Director, 2026-11-02)

*C. difficile* was rejected for being colonic, and applied consistently the
same rule cost **four of the world's most common gut pathogens**: *Shigella*
(#1 on three of five global metrics, 24% of under-five diarrhoeal deaths),
*Campylobacter* (#1 in FoodNet at 20.6/100k), *Entamoeba histolytica*
(trogocytosis — it nibbles live cells, kills them, rejects the corpse, and
kills neutrophils and macrophages without losing viability) and *Trichuris*
(the best helminth mechanic found). Two of the roster's five standing "no
answer in the roster" gaps — killing the phagocyte outright, and a
justification for a NET mechanic — are answered by the colonic *Entamoeba*.

**The Director's ruling: colonic is fine.** Anatomy is no longer a veto; it
is a recorded note. §1a's three bands become a *generic* mucosal
cross-section rather than a jejunal one, nothing already built changes, and
the shortlists in `PATHOGEN_ROSTER.md` are no longer filtered by
compartment. Recorded there too.

**What that unlocks, in rough order of value:** *Entamoeba histolytica*
(two standing gaps at once), *Shigella* (the highest-burden bacterium
anywhere, plus actin-tail spread and PMN-opened invasion), *Campylobacter*
(it swims *faster* as viscosity rises, so the mucus upgrade helps it, and
its phase-variable capsule blocks antibody and is selected by serum
exposure), *Trichuris*, *C. difficile* itself, and ETBF. None of them is in
the ten below — this ruling landed after the set was chosen — so the next
roster pass should start here rather than from the mechanics shortlist.

### 6.2 *Giardia*-only rounds — ANSWERED: shed antigen (research 2026-11-02)

§1c says debris is the input to adaptive immunity, and *Giardia* produces
none, so on current rails the rungs that counter it are unreachable from a
*Giardia* round. The Director asked for the real route rather than an
invented one, and there is one:
**`docs/research/giardia_antigen_route.md`.**

**The parasite sheds its own antigen in extracellular vesicles**, human
dendritic cells capture and mature on them, and **antibody binding triggers
a massive VSP-microvesicle release** — so attacking it is what makes it
teachable. The proposed mechanic is a wall-adherent trophozoite emitting a
drifting antigen parcel on a slow timer and in a burst when coated, carried
by either of the two samplers already built (the Sprint 19 M-cell dome, or
a patrolling DC). Knowledge per encounter is deliberately lower than
debris, because the same packet suppresses (VSP^AS7 binds NLRP3; DC IL-12,
CD83 and CD86 fall, IL-10 rises).

Recorded as an amendment under §1c: **debris is one input, not the only
one.** Unbuilt — it needs one new lumen-side entity and the Director's
sign-off.

Two routes were rejected on evidence and should not be re-proposed:
transepithelial dendrites (no *Giardia* data; CX3CR1⁺ cells do not migrate
to the node, so they are structurally wrong for §5a) and goblet-cell
antigen passages (they feed *tolerogenic* DCs and are shut by IL-1β, which
*Giardia* does not induce — a GAP route would teach **tolerance**).

### 6.3 The eukaryote hue

Recommendation: a **desaturated plum**, distinguished from the virion's
saturated violet by saturation and from debris grey by value — with
**size carrying the real weight** (0.7–1.1 cells against 0.55–0.6).
Alternatives all collide: amber with the neutrophil, orange with cytokine
heat, teal with the helper T, brown with the bolus.

### 6.4 §4a's parasite class is over-specified, and fixing it unblocks this

`BACKLOG.md` calls the parasite class's multi-slot footprint "more
engineering than the other three classes combined". **Not one enteric
protozoan needs it.** The largest is a 10–60 µm *Entamoeba*; a
microsporidian spore at 1.5 × 0.5 µm is *smaller* than the game's
bacterium. Re-spec the class as **single-slot with a size multiplier** and
defer the footprint to helminths, where it is genuinely required.

## 7. Corrections to standing design, from the five passes

Logged in full in the research docs; these are the ones that touch built
code or locked sections.

1. **The built rotavirus has bulk transmission** — >10 virions per
   vesicle, 10–45% of stool virus, removing vesicles cuts infectivity
   72–83% — which defeats §1b's minimum-dose floor. It also has a
   **non-lytic exit** (~100% release, no permeability change at 72 h).
   We had been playing rotavirus as a cell-killer; it substantially is
   not one. **FIXED in Sprint 34** (Director, 2026-11-02): per-species
   crowd weight capped below the ingress floor, and
   `NonLyticReleaseFraction` suppressing burn-out. The "§1c has no
   host-cell state for released-and-still-alive" part of this finding was
   **wrong** — §4b's *budding* mode already was that state. The real defect
   was that budding had never been per-species: a coin flip chose it for 29
   sprints, while §4b said "per-species modes" the whole time. Seventh
   instance of the standing pattern. `Sprint34Verification`, 20 checks.
2. **The built *V. cholerae* actively swims**, and non-chemotactic,
   CCW-biased mutants *out-compete* wild type across the small intestine —
   so "run straight and get everywhere" beats "steer at the wall", which
   is both truer and cheaper than the intuitive implementation.
3. **§4a's "parasites don't hide" and "cleared by direct combat damage,
   more of it" are both wrong.** Cytotoxic lymphocytes are inactive
   against helminths; clearance is expulsion. The player's instinct —
   build more killers — is the wrong answer for the entire group.
4. ***C. perfringens* is not a preformed toxin**, as
   `pathogens_extracellular.md` §3.6 has it. It is the opposite: onset
   8–24 h *because* it must sporulate first.
5. ***Campylobacter* swims faster as viscosity rises**, so §6b's mucus
   upgrade — a defensive buy — helps it. Second organism found whose
   answer is to invert a player upgrade, after A/E *E. coli*.
6. **Adenovirus F41 must not be built as a cliff.** Confirmed but
   narrowed: **MICB only** (MICA explicitly not blocked), **retention not
   transcriptional**, one cell line, preliminary, no NK killing assay.
   `PATHOGEN_ROSTER.md` calls it "the exact inversion of CMV"; the
   evidence does not carry that weight.
7. **Norovirus's "2–4 year cycle" is historical only** — Sydney 2012 has
   held 11+ years. Implement stochastic with a long tail, not a metronome.
8. **Sapovirus's cut was right on mechanics and wrong on burden**, and the
   two passes that examined it disagree in a useful way — see §8.
9. **The only IgA protease found in the gut belongs to a commensal**
   (*Sutterella wadsworthensis*), and it measurably alters *C. jejuni*
   infection and neutrophil phagocytosis. Whether §5's ~70% IgA rung works
   may be partly a property of the bacterial lawn, not the player's B
   cells.
10. **§7's scale note is computed at 5× but the lattice is 7×7.**
11. **§4c's CTL reward needs a counterexample**, and §4c's apoptosis table
    assumes a route a species can deny: `ApoptoticDebrisFraction` is not
    "the whole reward in one number" without a per-species
    apoptosis-resistance factor, and §1d's immunogenic-apoptosis upgrade
    silently does nothing against *Cryptosporidium*.

## 8. Where two passes disagreed, and the resolution

`enteric_burden.md` ranks **sapovirus #2 in the world** and calls it the
best anatomical fit available (replicates in duodenal, jejunal and ileal
enteroids; explicitly not colon). `pathogens_viruses_02.md` says the cut
stands and that **the burden argument fails too** — in a high-income
case-control series its case-vs-control gap is not significant (7.0% vs
3.0%, P=.07), and its post-vaccine rise is *relative, not absolute*
(Finland 1.4%→5.5% while "the absolute number did not increase").

**Both are right about different populations**, and the burden pass's own
"detection ≠ causation" caution applies to its own headline finding. The
resolution is §0: sapovirus does not earn a **mechanic** slot, and it does
earn a **tier** slot. That satisfies the burden evidence without inventing
a special power the literature does not support.

## 9. What is not answered

- **Astrovirus's "no inflammation" is refuted for replicating virus**
  (IFN-dominated response in human enteroids), and "no cell death" rests
  on trypan blue alone. The *capsid-alone* breach is stronger than the
  roster assumed — polarised filter-grown Caco-2 at TER ≥1000 Ω·cm²,
  apical only, TER to zero by 32 h, reproduced by UV-killed virus and by
  VLPs. So it is not the undetectable enemy; it is two threats in one
  organism.
- ***Cryptosporidium*'s infected human segment is host-species dependent
  and partly UNVERIFIED** — duodenum/jejunum in neonatal rats, ileum,
  caecum and colon in neonatal mice. Its small-bowel placement is an open
  question in a way *Giardia*'s is not.
- **Anti-VSP antibody's effect on *Giardia* is genuinely contested** — a
  2023 *Nat Commun* paper has it clustering VSPs, shedding them as
  microvesicles and triggering the switch; a 2011 review has
  anti-N-terminal VSP antibody being cytotoxic. Recorded as unresolved
  rather than picked.
- **Infectious doses are missing** for *C. perfringens* (which is §1b
  Step 2's crowd floor), *Cyclospora*, *Cystoisospora*, *Entamoeba*, the
  microsporidia, and every helminth. *Giardia*'s human dose rests on a
  1954 paper whose abstract could not be retrieved.
- ***Entamoeba*'s global burden has three mutually inconsistent sourced
  figures**, so no pass would quote one.
