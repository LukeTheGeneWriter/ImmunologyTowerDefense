# Common enteric bacteria — re-ranked by prevalence, and extended past the last pass

Research pass for Immunology Tower Defense, **2026-11-02**. Status: **research
reference, not a design decision.** `docs/GAME_DESIGN.md` wins on any conflict.

**Brief.** The Director asked for *common* gut pathogens. The existing bacterial
shortlist in `docs/PATHOGEN_ROSTER.md` was assembled purely on mechanics, and
nobody had checked it against burden. This pass does two things: it re-ranks the
existing shortlist by how common each organism actually is, and it extends the
set with the common organisms the two earlier bacterial passes
(`pathogens_invasive_bacteria.md`, `pathogens_extracellular.md`) never covered.

**Read against:** `GAME_DESIGN.md` §1a (three lateral bands), §1b (adhesion →
mill → plaque charge → rupture), §1c (host-cell states, occupant layer, debris),
§4a (pathogen classes), §4b (intracellular infection, stress-sense meter), §4c
(the tower roster), §5 (knowledge ladder).

**Provenance.** Every factual claim below was verified in this session against
**PubMed** (`mcp__plugin_bio-research_pubmed__*`) or **Consensus**
(`mcp__plugin_bio-research_consensus__search`). According to PubMed, the
articles cited with DOI links are as described; Consensus-sourced articles are
linked to their Consensus records. Nothing here was written from memory.
**UNVERIFIED** means no defensible published number was found *in this pass* —
not that none exists. Inline markers are `[n]`; the reference list is at the
bottom.

**Relationship to the two earlier passes** is marked explicitly throughout as
**CONFIRMING**, **EXTENDING** or **CORRECTING**.

---

## 0. The headline, in four sentences

The roster was built on mechanics and it is *mechanically* excellent, but it is
**not a list of common organisms**. Two of the three most common bacterial
enteric pathogens in a high-income setting — ***Campylobacter*** and
***Clostridium perfringens*** — are, respectively, shortlisted-but-unbuilt and
**entirely absent from every previous pass**. Meanwhile three shortlisted
organisms (*Listeria*, AIEC, MAC) are collectively rarer than any single
addition in this document. The prevalence lens does not overturn the roster, but
it changes **which four organisms get built next**.

---

## 1. Prevalence-ranked table — the whole bacterial candidate set

Two burden columns, because they disagree violently and the disagreement is
itself a design input: a **global/endemic** view (GBD 2021 and WHO) and a
**high-income surveillance** view (US). The US column is the best per-organism
breakdown that exists for organisms global estimates do not model at all.

**The surveillance trap, stated once.** FoodNet tracks exactly eight pathogens
[3]. *C. perfringens*, *S. aureus*, *B. cereus*, ETEC, EPEC and EAEC are **not
notifiable and not in FoodNet**, so their absence from the famous incidence
table is an artefact of what is counted, not evidence of rarity [2][3]. Scallan
2011 is the estimate that includes them, built from outbreak surveillance with a
25.5× underreporting multiplier [2]. Do not rank from FoodNet alone.

| # | Organism | US domestic foodborne illnesses/yr [2] | High-income incidence (FoodNet 2022, per 100 000) [3] | Global / endemic signal | Rhythm | On an earlier pass? |
|---|---|---|---|---|---|---|
| **1** | ***Campylobacter* spp.** | 845 024 | **19.2 — the highest of any pathogen** [3] | 96 million foodborne illnesses globally, 2010 [6]; among the top three hazards for foodborne DALYs in 2021 [4] | **endemic**, sporadic; outbreaks are rare (<1% outbreak-associated) [3] | shortlisted (invasive pass #5) |
| **2** | **Non-typhoidal *Salmonella*** | **1 027 561 — the single largest** | 16.3 [3] | highest foodborne DALY burden of 22 foodborne diseases (4.07 M DALYs, 2010) [6]; leading cause of foodborne deaths [5] | **both** — endemic baseline + 9.1% of cases outbreak-associated [3] | **built-adjacent**: shortlisted (invasive pass #2) |
| **3** | ***Clostridium perfringens*** (type F) | **965 958 — #2 bacterial cause of US food poisoning** [2][11] | **not tracked** [3] | **absent from GBD's 13 aetiologies and from WHO's 14 hazards** [1][4] | **outbreak**, institutional, explosive | **NO — new** |
| **4** | ***Shigella* spp.** | 131 254 | 4.9 [3] | **under-5 fatal PAF 24.0%** — the largest of any bacterium [1]; top-three foodborne DALYs [4] | **both**; 5.5% outbreak-associated, 30.6% hospitalised [3] | shortlisted (invasive pass #1) |
| **5** | **ST-ETEC** | 17 894 (mostly travel-related) | not tracked | **under-5 fatal PAF 13.4%** [1]; one of GEMS's top four attributable causes [9] | **endemic** (LMIC), traveller's in high-income | shortlisted (extracellular #5) |
| **6** | **tEPEC** | ~11 982 (modelled as ETEC-equal) [2] | not tracked | **under-5 fatal PAF 13.6%** [1]; **highest per-case death hazard in infants of any GEMS pathogen, HR 2.6 (1.6–4.1)** [9] | **endemic**, paediatric | shortlisted (extracellular #3) |
| **7** | ***Staphylococcus aureus*** (SFP) | 241 148 | not tracked | not modelled in GBD or the WHO diarrhoeal-hazard set [1][4] | **outbreak**, fast, self-limiting | shared slot (extracellular #6) |
| **8** | **STEC / EHEC** | 175 905 (non-O157 112 752 + O157 63 153) | 5.7, **rising (IRR 1.18)** [3] | not in GBD's 13; in WHO's 14 hazards [4] | **outbreak**-heavy | shortlisted (extracellular #2) |
| **9** | ***Yersinia enterocolitica*** | 97 656 | 2.0, **more than doubled vs 2016–18 (IRR 2.41)** [3] | not modelled | endemic, youth-skewed | shortlisted (invasive pass #4) |
| **10** | ***Bacillus cereus*** (both syndromes) | 63 400 | not tracked | not modelled | **outbreak**, very fast | shared slot (extracellular #6) |
| **11** | ***Vibrio parahaemolyticus*** | 34 664 | 1.0 (all *Vibrio*), **rising (IRR 1.57)** [3] | 3.11% of 79 075 diarrhoeal samples across 11 Chinese provinces; 8.1% of 6 951 in eastern China; 383 outbreaks / 4 382 cases in Zhejiang 2010–22 [37] | **outbreak**, seasonal, warming-driven | **held** (extracellular pass) |
| **12** | **EAEC** | within the 11 982 | not tracked | one of WHO's 14 foodborne diarrhoeal hazards [4] | **endemic**, persistent | shortlisted (extracellular #4) |
| **13** | ***Listeria monocytogenes*** | **1 591 — 600× rarer than NTS** | 0.3 [3] | modelled in GBD's foodborne-bacteria set [81] | **outbreak**, rare and catastrophic: **94.1% hospitalised, 22.1% died** [3] | shortlisted (invasive pass #3) |
| **14** | ***Aeromonas* spp.** | not estimated | not tracked (would be missed: the Singapore panel cannot detect it) [47] | **one of GBD 2021's 13 modelled diarrhoeal aetiologies** [1]; named by GEMS as site-importantly attributable [9]; 0.7% of 299 Singapore AGE stools [47]; *A. sobria* 3% of diarrhoeic stools, Cape Town [48] | endemic, site-specific | **rejected** by the extracellular pass |
| **15** | ***Plesiomonas shigelloides*** | not estimated | not tracked | 0.3% of 299 Singapore AGE stools [47]; **9% of diarrhoeic stools and 17% of surface water** in a Cape Town informal settlement [48]; 71% of a 30-case series had recent tropical travel [45] | endemic/travel, low | **NO — new** |
| **16** | **Enterotoxigenic *B. fragilis*** | not estimated | not tracked | **~20% of humans are asymptomatic carriers** [50] | endemic carriage | **NO — new** |
| **17** | ***Vibrio cholerae*** (toxigenic) | **84** | within the 1.0 | epidemic; fastest pre-pandemic regional rise of any pathogen (Caribbean EAPC +47.21) [7] | **epidemic** | **BUILT** |
| **18** | **AIEC, *M. avium* complex** | no incidence figure exists (pathobiont / chronic colonisation) | — | — | chronic | shortlisted (invasive pass #6, #7) |
| — | ***Clostridioides difficile*** | not foodborne-estimated | not tracked | **the dominant diarrhoeal pathogen of high-SDI regions (ASDR 0.53) and the only one positively correlated with SDI (ρ = 0.63)** [7]; ~15 600 global deaths, 13 100 of them in high-income regions, mostly ≥70 y [1]; the fastest-growing of GBD's 13 (ASDR 0.19 → 0.43, 1990–2019) [8] | endemic, geriatric, iatrogenic | **REJECTED on anatomy** |

### What the prevalence lens actually says

1. **The single most common bacterial enteric pathogen in a high-income gut is
   *Campylobacter*, and it is not built.** 19.2 per 100 000 [3], 96 million
   foodborne illnesses globally [6], top-three foodborne DALYs [4]. It was ranked
   #5 on mechanics. **CORRECTING the invasive pass's ranking:** on prevalence it
   is #1, and §4 below shows it also has two traits with no answer in the roster.
2. ***C. perfringens* is the largest single gap in the whole roster.** It is the
   **second most common bacterial cause of food poisoning in the United States**
   [11][12] at ~966 000 illnesses a year [2] — more than *Campylobacter* — and it
   appears in **none** of the four earlier research passes.
3. **Prevalence and lethality are near-orthogonal.** *Listeria* is 600× rarer
   than NTS [2] and kills 22.1% of the people it hospitalises [3]. *C.
   perfringens* is ~966 000 cases and 26 deaths [2]. Any single "threat" number
   that collapses these is lying to the player.
4. **The famous-and-rare trap is real and the roster partly fell into it.**
   Toxigenic *V. cholerae*, which **is built**, is 84 US cases a year [2];
   *Listeria*, shortlisted, is 1 591 [2]. Both earn their slots on mechanics —
   that is the rule and it is the right rule — but the roster should not be
   described to the Director as a list of common organisms, because it is not.

---

## 2. Mechanic-ranked table — the roster's own rule

> **A species earns a slot by forcing a decision no existing enemy forces.**

Ranked by *mechanical delta against what is already built or shortlisted*, with
the one dimension named. The three built bacteria are the baseline:
**V. cholerae** (adheres, secretes, subverts living cells, feeds on the damage),
**A/E *E. coli*** (respires the products of inflammation; pedestal adhesion
resists flushing), and the **generic gut bacterium**.

| # | Organism | The ONE mechanic nothing else brings | Does the roster have an answer? |
|---|---|---|---|
| **1** | ***C. perfringens*** (type F) | **The toxin is built *in situ* by sporulation, and the fastest-growing cell on the board.** CPE is not preformed in food — it is produced by cells **sporulating in the small-intestinal lumen** [11][12], i.e. the organism converts itself into a weapon and a survival capsule in the same act. Generation time **7.1 min at 41 °C** [15], 6.3–8.7 min at 43 °C [16] — ~6× faster than anything else considered. | **No.** Nothing on the board turns a defensive form into an attack, and nothing replicates at this rate. §1b's density-damped wall replication has never been stressed by a number like this. |
| **2** | ***Campylobacter jejuni*** | **It swims *better* the thicker the mucus gets** — swimming velocity *increases* with external viscosity [60], and in high-viscosity media it wraps its leading flagellum around its own body to do it [59]. Plus **phase-variable capsule decoration that blocks pre-existing antibody and is positively selected by serum exposure** [65]. | **No, twice.** §6b's mucus-turnover upgrade would *help* it, the way EAEC's Pic inverts the same upgrade. And §5's IgA/antibody rungs would *select for* the variant that evades them. |
| **3** | **Preformed toxin — *S. aureus* / emetic *B. cereus*** | **An attack that already happened.** The toxin is made in the food; there is no organism to kill, flush, phagocytose or learn. Cereulide is **not inactivated by any heat treatment used in the food industry** [29] and loss of activity is *reversible* [29]; SEs resist heat, cold, proteolysis and a wide pH range [35]. Onset **4–10 h** [10], as little as ~3 h in a documented outbreak [36]. | **No.** The only mechanic that could touch it is antibody-mediated **neutralisation**, which is categorically different from the coating/enchainment `SetCoated` path §4c and §5 already implement. **CONFIRMING** the extracellular pass, with the onset numbers it marked incomplete. |
| **4** | ***Yersinia enterocolitica*** | **It kills the phagocyte — but only the macrophage, and doing so costs it.** YopJ/YopP kills ~70% of infected macrophages in 8 h by apoptosis [53], **human neutrophils are resistant** [54], and the more cytotoxic variant is *less* virulent in vivo [55]. Separately, **YadA and Ail defeat the complement body upgrade outright**, binding C4BP and factor H and cutting C5b-9 deposition [56][57][58]. | **No** on both. §4b has no "the tower dies from what it ate", and the complement body upgrade (§5's note, re-specified as opsonise-and-recruit) has no stated counter. |
| **5** | ***Shigella*** | Actin-tail cell-to-cell spread with **no extracellular phase**, lowest infectious dose on the board. | **No.** Still the strongest intracellular addition. **CONFIRMING** the invasive pass. |
| **6** | ***Salmonella* Typhimurium** | **Two replication rates in two host-cell types**, and an escape that routes the brood **back into the lumen** rather than into tissue. Plus **PhoPQ/PmrAB lipid-A remodelling** that walks it past cationic antimicrobial peptides [73][74][75]. | Partly. **EXTENDING** the invasive pass with the AMP-resistance trait — see §5's honest limit on it below. |
| **7** | ***V. parahaemolyticus*** | **It arms itself on a host chemical cue.** T3SS2 expression is **induced by bile** [41], and Vp-PAI senses the intestinal environment and host-cell contact [42]; T3SS2 is the major contributor to enterotoxicity, T3SS1 to cytotoxicity [40]. A 17 h median incubation sits in a band of its own [10]. | **No** — and this is a **CORRECTION**: the extracellular pass held it as "EHEC with a cytolysin and fewer distinctives". A pathogen that is inert in the lumen and arms on entry is not EHEC with a hemolysin. |
| **8** | **EHEC** | Destruction aimed **past** the wall — human gut epithelium lacks Gb3, so Stx translocates. | **No.** **CONFIRMING.** |
| **9** | ***Listeria*** | A **named door** (goblet cells / shedding sites), the canonical CD8 payoff — **and it arrives non-motile.** Flagellar genes are shut off at 37 °C by the MogR/GmaR thermosensor; `mogR` deletion costs **250-fold virulence** [68][67][69]. | Partly. The non-motility is **EXTENDING** and it is the cheapest possible implementation of a real trait: it is the *default* drift the game already does. |
| **10** | **EAEC** | A persistent biofilm structure, and Pic inverts the mucus upgrade. | **No.** **CONFIRMING.** |
| **11** | **EPEC** | Pedestal adhesion defeats both flush defences. | Covered by built A/E *E. coli*. |
| **12** | **ETEC** | Two toxins on two clocks. | Partly — cholera covers "toxin as a verb". |
| **13** | **AIEC** | An infected but still-functional macrophage. | **No**, but unrecognisable and no dose figure. |
| **14** | **MAC** | ~7-day in vivo generation time; the stalemate. | **No**, but it is a scenario, not a wave. |
| — | ***Aeromonas***, ***Plesiomonas***, **ETBF** | see §5 — cut, each for a different and specific reason. | — |

---

## 3. Where the two rankings disagree — and what that means for round design

| Organism | Prevalence rank | Mechanic rank | The disagreement |
|---|---|---|---|
| ***C. perfringens*** | 3 | **1** | **Agrees.** It is the only candidate that is top-3 on both axes. Build it next. |
| ***Campylobacter*** | **1** | **2** | **Agrees.** Also top-3 on both. Build it next. |
| ***V. cholerae*** | 17 | built | **Maximal disagreement.** 84 US cases a year [2], and the best mechanic in the report. Keep it; stop calling it common. |
| ***Listeria*** | 13 | 9 | Mild. 1 591 US cases [2]. It earns its slot as the *CD8 teaching organism*, not as a common threat. |
| **tEPEC / ST-ETEC** | 6 / 5 | 12 / 12 | **Inverted.** The two biggest *global paediatric killers* among the *E. coli* are the two whose mechanics are most covered by what is built. |
| ***Salmonella*** | **2** | 6 | Mild — the one organism that is near the top of both. |
| ***C. difficile*** | **#1 in high-SDI** [7] | banked | **The sharpest.** The organism the roster rejected on anatomy is the leading diarrhoeal killer of the world the player probably lives in. The rejection is still right (§5), but it should be stated as *"right for this map"*, never as *"not important"*. |

### What this does to §5c's 40-round curriculum

1. **The curriculum should open on prevalence and close on mechanics.** Early
   rounds teach with organisms the player has actually heard of and that the
   world actually contains — *Campylobacter* (chicken), NTS (eggs),
   *C. perfringens* (the buffet) — and the famous-but-rare mechanics specialists
   (*V. cholerae*, *Listeria*, EHEC) land later as named events. That ordering is
   also the honest one: the common organisms are the simple ones.
2. **Two rhythms, and they should sound different.** The burden columns separate
   cleanly into **endemic** (a baseline trickle every round: *Campylobacter*,
   NTS, EAEC, ST-ETEC, tEPEC) and **foodborne outbreak** (nothing for ten rounds,
   then one bolus that is almost entirely one species: *C. perfringens*,
   *S. aureus*, *B. cereus*, *Listeria*, *V. parahaemolyticus*). §1a's bolus +
   peristaltic surge already implements the outbreak rhythm perfectly. **The
   endemic rhythm is the one the game does not have** — an organism that is
   *always* present at low level and never the headline.
3. **The incubation spread is a free, sourced pacing dial and it spans 170×.**
   From one US outbreak dataset [10]: preformed toxins **4–10 h**;
   *V. parahaemolyticus* **17 h**; norovirus / *Salmonella* / *Shigella*
   **32–45 h**; *Campylobacter* and STEC **62–87 h**; hepatitis A **672 h**.
   *C. perfringens* sits at **8–24 h** [13] (reported range 2–36 h [14]) — and
   the reason it is slower than the preformed toxins is mechanistically exact: it
   has to sporulate first. **Use this, not invented numbers, to set how long
   after a bolus arrives its consequence lands.**
4. **A round whose callout names a real food is a better round.** The vehicles in
   §4 are all sourced to specific outbreaks: a slow-braised "lu" hamburger held
   warm [14], fried rice balls at 8×10⁶ CFU/g carrying 37 µg/g cereulide [30],
   ham-and-cheese sandwiches at a family lunch [36], Australian-grown oysters
   [38].

---

## 4. Per-candidate blocks

Additions first, then the existing shortlist entries this pass is correcting or
extending.

---

### 4.1 *Clostridium perfringens* type F — **the strongest addition in this pass**

**The one mechanic.** *Adheres and drifts; never invades; takes no host-cell
slot; kills epithelium without ever entering it; leaves debris; lumen and gut
interface only; **drifting, never motile**.* Its distinguishing act: **the
adherent crowd arms itself by sporulating.** CPE is **sporulation-dependent** —
produced by cells sporulating **in the small-intestinal lumen** [11][12], not
preformed in food. So the sequence is: a huge bolus arrives → the crowd mills on
the wall (§1b) → *and then the crowd converts itself into spores + toxin*. In
game terms this is a **charging mechanic the pathogen owns**, the mirror image of
§1b's plaque charge, which the wall owns. Clear the crowd before it commits and
nothing happens; let it commit and the toxin lands whether or not you then kill
the cells. A fraction of the crowd should also be left behind as **spores: an
inert form that nothing on the board can kill** — which is the honest reading of
why reheating fails.

**The numbers a build needs.**
- **Generation time: 7.1 min at 41 °C** (strain NCTC 8238, constant
  temperature); mean generation time 13–30 min across beef-cookery heating rates;
  fastest observed ~8 min [15]. Independently: **6.3–8.7 min at 43 °C** in
  thioglycollate, **7.1 min at 41 °C** and 9.2 min at 46 °C in autoclaved ground
  beef [16]. **These are in-food / in-vitro numbers. No in vivo intestinal
  generation time was found → UNVERIFIED.** The in-vitro/in-vivo caution the
  invasive pass set for *M. avium* applies here with equal force.
- **Onset: 8–24 h** after ingesting food containing large numbers of vegetative
  cells [13]; a 98-case Beijing outbreak reports the typical range as **2–36 h**
  [14]. **CORRECTING** the extracellular pass's grouping of *C. perfringens* into
  the 4–10 h "preformed toxin" band — see §6.
- **Infectious dose:** "great numbers of vegetative cells and spores" [27],
  "large numbers of vegetative cells" [13]. **A defensible CFU figure was not
  found in this pass → UNVERIFIED.** This matters: it is exactly §1b Step 2's
  minimum-crowd floor, and it should be set *high*, consistent with the
  extracellular pass's structural finding that adhere-and-secrete organisms need
  big inocula.
- **Toxin action:** CPE binds **claudin** receptors, ~6 small complexes
  oligomerise into a prepore, then a β-barrel pore; Ca²⁺ influx activates
  calpain. **Low dose → caspase-3 apoptosis; high dose → necroptosis** [11]. In a
  mouse intestinal-loop model, 100–200 µg CPE caused significant lethality within
  **2 h** while 50 µg did not, and CPE was absorbed into the circulation and
  bound liver and kidney [12]. A **dose-dependent switch between a clean death
  and a messy one** maps directly onto §4c's apoptotic-vs-necrotic debris table —
  except here *the pathogen* chooses which, not the player.

**Resistance / evasion traits — three, all mechanical.**
1. **Sporulation that beats cooking.** Spores of chromosomal-*cpe* isolates are
   **~30× more heat resistant** than plasmid-*cpe* spores, and their vegetative
   cells ~2× [17]; most food-poisoning isolates carry chromosomal *cpe* [11][17].
   French outbreak strains: **δ₉₅ ᵒC ≈ 49 min** for c-*cpe*, ≈ 9.5 min for
   *cpe*-negative strains of the same clade, versus δ₈₅ ᵒC = 12 min for p-*cpe*
   [18]. *Provenance note, recorded because it matters:* the original 2000 paper
   reporting this was **retracted**, and [17] is the 2024 replication that
   reproduced it. Cite the 2024 paper, not the 2000 one.
2. **It kills the phagocyte.** Perfringolysin O production correlates with human
   **PBMC cytotoxicity** and with IL-6/IL-8 induction, more strongly than
   α-toxin does [23]. α-toxin + PFO together cause cell death within **30 min**
   and measurable microvascular perfusion loss within **60 min**, and depleting
   neutrophils or platelets *reduces* the perfusion loss because those cells are
   what physically obstructs the vessels [22]. Separately, lecithinase
   (phospholipase C) enters LAMP1⁺ vesicles and **destabilises the lysosomal
   membrane**, and PFO and lecithinase are **non-redundant** NLRP3 activators
   [24]. **This is a better fit for "the tower dies from what it ate" than
   *Yersinia* is** (§4.5), because it is a secreted field rather than an injected
   effector, and because the recruited cells are what makes it worse — which
   rhymes with A/E *E. coli*'s inflammation-as-fuel without duplicating it.
3. **It cannot swim, at all.** *C. perfringens* **lacks flagella** [20] and moves
   only by **type IV pilus-dependent gliding on surfaces**, in long filaments of
   cells attached end to end [19][21]. A *sagA* mutant cannot form the filaments
   and cannot glide [21]. So it is a pure drifter in the lumen — which is exactly
   what the game already does — and its only "movement" is along a surface it has
   already reached. **Build it as the organism that proves drift is a real
   property, not an engine limitation.**

**Morphology for a sprite.** Large Gram-positive rod. **Rod dimensions in µm:
UNVERIFIED in this pass** (described only as "medium sized rods" in a clinical
isolate report [84]); the **spore** is sized at **2.1–3.3 µm**, from a
size-fractionated airborne-particle study whose distribution peaks at the spore
size of *Clostridium* spp. [82] — a plausible figure, but it is an aerosol
particle-size distribution, not microscopy, and should be re-sourced before it is
used as a sprite spec. **What makes it distinguishable from a plain bean:** two
things, and both are the biology. (i) **A bright oval spore inside a blunt rod**
— a capsule-within-a-capsule, trivially drawable at 64×64 and readable at 20 px,
and it is the organism's actual weapon-arming state, so the sprite can *change*
when the crowd commits. (ii) **End-to-end chains**, because that is literally how
it moves [19][21]. A short line of three blunt rods with one spore glowing inside
the middle one is unmistakable against a drifting bean.

**One variant worth banking, not building.** *C. perfringens* **type C** causes
**enteritis necroticans ("pig-bel")**, a severe necrotising disease of the small
intestine driven by beta toxin (CPB) — and in CPE-positive type C strains, CPB
and CPE act **synergistically** in rabbit small-intestinal loops, each at a
concentration that alone does nothing [25]. It is uncommon in high-income
settings and associated with contaminated pork [27]. Mechanically it is "type F
plus a second toxin", i.e. an **elite variant** of the same unit, not a second
unit — the same call the invasive pass made for *S.* Typhi. Also worth recording
for tuning: the species secretes **more than 20 identified toxins**, is an
anaerobe that nonetheless **survives in the presence of oxygen**, and carries
many of its virulence genes on **mobile genetic elements** [26] — so "which
toxins this strain has" is genuinely strain-level, which complicates any single
§5 knowledge rung for it in exactly the way *B. cereus* does (§4.3).

**Exposure route / vehicle.** Meat or poultry **cooked, then held warm or cooled
slowly** — the 15–55 °C window is where it doubles every seven minutes [14][15].
Specific and sourced: a **slow-braised "lu" Chinese hamburger** at an elderly
care centre in Beijing, 98 cases, where the slow cooling of the braising
technique was identified as the mechanism [14]. For a Western callout: a
rotisserie chicken held under a warming lamp, or a school/institution gravy.
Classically an institutional-kitchen organism: the severe historical outbreaks
with deaths occurred in **psychiatric institutions** [12].

**Burden.** **965 958 US domestic foodborne illnesses/yr (90% CrI 192 316 –
2 483 309), 438 hospitalisations, 26 deaths** [2]. Described in the primary
literature as the **second most commonly identified bacterial foodborne illness
in the United States** [12] and "one of the most common types of foodborne
gastroenteritis in the US" [27]. **Not tracked by FoodNet** [3] and **not modelled
in GBD 2021 or in WHO's 14-hazard set** [1][4], so its global burden is
**UNVERIFIED** — a real gap, not a small one. **Age skew:** UNVERIFIED as a
distribution; the outbreak literature is institutional (an elderly care centre,
ages 22–99 [14]; psychiatric institutions [12]). **Rhythm: foodborne outbreak,
emphatically** — point-source epidemic curves [14].

**The player's counter.** Partly answered, partly not. *Answered:* sentinel
goblet sloughing and IgA cross-linking act on the crowd **before** it commits,
and the γδ IEL's plaque-charge suppression (§4c) is the right shape for
suppressing a *pathogen-owned* charge too. *Not answered:* once CPE is released
nothing neutralises it; the spores cannot be killed by anything on the board; and
PFO killing macrophages has no mechanic at all. **The honest summary: the
player's only winning line is to break up the crowd early, which is precisely the
lesson §1b Step 2 exists to teach.**

**Redundancy check.** Not *V. cholerae*: cholera subverts living cells and never
kills, CPE **kills** them by pore formation [11]. Not A/E *E. coli*: no pedestal,
no inflammation respiration, and it is non-adherent-by-pedestal. Not the generic
bacterium: it neither takes a slot nor fights. It is **new on three axes at once**
— self-arming, unkillable dormant form, and phagocyte-killing. The single
dimension in which it is "stronger" than anything built is **replication rate**,
and that is a named dial, not a vibe.

---

### 4.2 *Campylobacter jejuni* — **re-ranked from #5 to the top, on prevalence AND mechanics**

**This block is a CORRECTION of rank, not a new candidate.** The invasive pass
ranked it #5 and framed it as "the gate-opener". Both the burden and two traits
it did not surface argue for promotion.

**The one mechanic — revised.** *Adheres at the gut interface, then crosses
**paracellularly** and invades from the basolateral face; **motile under its own
power, and faster in mucus than in water**; opens the wall for bystanders.* The
gate-opening remains its headline (HtrA junction cleavage lets non-invasive
bystanders transmigrate with it — the invasive pass's finding, **CONFIRMED as
its distinguishing trait**). What this pass adds is that it is the **only
organism in the candidate set that actively beats the lumen flow model**.

**The numbers, extended.**
- **Infectious dose:** 8×10² to 2×10⁹ all caused illness in 111 volunteers;
  infection rate rose with dose but illness did not (invasive pass, retained).
- **Incubation: 62–87 h median** — with STEC, the **slowest of any bacterium** in
  a 15-year US outbreak dataset [10]. This is a *big* pacing fact that pairs
  badly with the fact that it is the *most common* organism: the commonest enemy
  is also the one whose consequence lands last.
- **In vivo intracellular doubling time: UNVERIFIED** (unchanged from the
  invasive pass — it survives but barely replicates intracellularly).

**Resistance / evasion traits — four, three of them new here.**
1. **Motility that improves in mucus.** It rotates a flagellum at **each pole**,
   its motors produce much higher torque than Enterobacteriaceae, and in
   high-viscosity media it **wraps the leading filament around its own cell body**
   to coordinate the two [59]. Its swimming velocity **increases as external
   viscosity increases** — the opposite of most flagellates — and this is sensed
   through viscous drag on the filament by a flagellar "V-ring" complex that also
   retunes metabolic transcription [60]. Shorter filaments slow swimming and
   hinder intestinal colonisation [61]. **Game consequence: it should not drift.
   It should cross the lumen band under its own power, and §6b's mucus upgrade
   should make it *faster*.** §5e's lane-changing (Sprint 25) is the existing hook.
2. **Phase-variable capsule decoration that blocks antibody — and that the
   player's own antibody pressure selects for.** O-methyl phosphoramidate
   (MeOPN) modifications on the capsule are non-stoichiometric because of phase
   variation at homopolymeric tracts in the transferase genes; MeOPN **blocks
   binding of naturally occurring antibodies to the capsule**, preventing
   activation of the classical complement arm; and **exposure to normal human
   serum selects for cells expressing the transferase that adds MeOPN at the
   critical position** [65]. **This is §5's mutation discount with a real
   mechanism and a real trigger: the discount should fire in proportion to how
   hard the player has been leaning on antibody.** Nothing in the roster does
   this; norovirus GII.4's antigenic replacement is on a *calendar*, not on the
   player's behaviour.
3. **Two different polysaccharides for two different defences.** Capsule is
   **essential** for serum (complement) resistance and lipo-oligosaccharide plays
   no substantial role; conversely the LOS mutant is markedly more sensitive to
   polymyxin B, **α-defensins**, cathelicidins and BPI [64]. So one coat answers
   the complement body upgrade and the other answers Paneth-cell defensins — a
   clean two-slot structure if a per-organism "armour" model is ever wanted.
   (Capsule effects are strain- and model-specific [66]; do not over-generalise.)
4. **Ganglioside mimicry** — phase-variable surface sugars that mimic host
   structures (invasive pass, retained).

**Morphology for a sprite.** A **helical / curved rod with a single flagellum at
each pole** [59]. **Dimensions in µm: UNVERIFIED in this pass.** The corkscrew
shape is **load-bearing biology, not decoration**: deleting the peptidoglycan
DL-carboxypeptidase *pgp1* makes the cell a straight rod, and that rod is
**deficient in host colonisation by more than three orders of magnitude** and
elicits *more* IL-8 than wild type [62]; a second enzyme, *pgp2*, provides *pgp1*'s
substrate and its deletion also straightens the cell and costs fitness [63].
**What makes it distinguishable from a plain bean:** everything. A single-period
corkscrew with a whip at each end is the most visually distinct silhouette in the
entire candidate set, reads instantly at 20 px, and — uniquely — the sprite's
shape *is* its virulence, so a "straightened" debuff would be a legible and
literature-exact visual.

**Exposure route / vehicle.** **Chicken**, overwhelmingly: poultry meat has been
the most commonly identified source in many countries for many years and is the
estimated most common US source [3]. Also unpasteurised milk and a puppy with
diarrhoea. The player-facing callout writes itself: *undercooked chicken, or the
cutting board it touched.*

**Burden.** **The highest incidence of any pathogen in FoodNet: 19.2 per 100 000
in 2022**, against a Healthy People 2030 target of 10.9 that is not being met
(domestic incidence 17.4, IRR 1.07 vs 2016–18, i.e. **rising**) [3]. 845 024 US
domestic foodborne illnesses/yr, 8 463 hospitalisations, 76 deaths [2]. Globally,
**96 million foodborne illnesses in 2010** [6], one of the most frequent causes
of foodborne illness worldwide [5], and among the **top three hazards for
foodborne DALYs in 2021** [4]. Under-5 fatal PAF is not separately resolvable
from the GBD text available in this pass → **UNVERIFIED**. **Rhythm: endemic and
sporadic** — fewer than 1% of cases are outbreak-associated [3], which makes it
the *only* strong candidate for the "always present, never the headline" rhythm
the game lacks.

**The player's counter.** The γδ IEL is close (it patrols the wall and repairs
it, which answers the junction damage), and the macrophage answers it well
because *Campylobacter* is delivered to lysosomes and **rapidly killed** in
macrophages (invasive pass). But **nothing answers the mucus-favouring motility
and nothing answers serum-selected antigenic variation.**

**Redundancy check.** Not *V. cholerae*: it invades and kills; cholera does
neither. Not A/E *E. coli*: no pedestal, and it crosses *between* cells rather
than remodelling one. Not *Shigella*: *Shigella* enters cells and stays;
*Campylobacter* barely replicates intracellularly and its point is the **hole it
leaves for everyone else**. The single dimension in which it is stronger:
**mobility in the mucus layer** — and it is stronger there specifically, while
being *slower* in everything to do with onset.

---

### 4.3 Preformed toxin — *S. aureus* and emetic *B. cereus* — **CONFIRMED, with the real onset numbers, and one species split in two**

**The one mechanic.** *Nothing adheres. Nothing invades. No slot, no stress
signal, no debris from an organism — because there is no organism.* The wave is
**cargo**: it arrives already active, acts on the upper gut, and leaves. **The
extracellular pass's shared-slot proposal is CONFIRMED as real for both
organisms, and this pass supplies the onset and stability numbers it marked
incomplete.**

**The numbers.**
- **Onset: 4–10 h median** for the whole preformed-toxin group in 15 years of US
  outbreak surveillance — the shortest band of any aetiology group, distinct from
  *V. parahaemolyticus* at 17 h and from *Salmonella*/*Shigella* at 32–45 h [10].
  A documented Italian family outbreak reported onset at **~3 h**, "which
  suggests that the food contained preformed toxins", with staphylococcal
  enterotoxin D at **0.649 ng/g** in the implicated cheese [36].
- **Cereulide stability:** heat-inactivation tested at 100, 121 and 150 °C under
  alkaline pH; stability "remarkable"; **"the inability of any heat treatment
  used in the food industry to inactivate cereulide"**, and — the detail worth
  building — **loss of activity was reversible** once the alkaline buffer was
  removed [29]. It is also resistant to heat and acid during cooking [30] and
  "unlikely to be inactivated by food processing" [31].
- **Staphylococcal enterotoxins:** small globular proteins combining
  superantigenic and emetic activity, **resistant to heat, to low temperature and
  to proteolytic enzymes, and tolerant of a wide pH range** [35]; >24 well-described
  SE genes plus newly described ones [35]; symptoms of **rapid onset, nausea and
  violent vomiting** [34]; SEA is the most common cause worldwide [34].
- **Dose:** retained from the extracellular pass (20–100 ng SEA total intake in
  the 13 420-case Osaka outbreak; cereulide 0.01–1.28 µg/g in implicated foods) —
  not re-verified here. One new data point: **8×10⁶ CFU/g and 37 µg/g cereulide**
  in a leftover rice ball, "one of the highest cereulide contaminations reported
  so far", in five immunocompetent adults of whom the index patient developed
  **acute liver failure and acute tubular necrosis** [30].

**The *B. cereus* split is sharper than the earlier pass stated, and it is
genetic.** The two diseases are **intoxication via cereulide (emetic)** and
**toxicoinfection via multiple enterotoxins (diarrhoeal)** [28] — the first is
cargo, the second is a live small-intestinal infection with tripartite
pore-forming cytotoxins (Hbl, Nhe, CytK). **EXTENDING:** cereulide synthetase is
**plasmid-encoded (the *ces* cluster)**, emetic *B. cereus* is **not clonal**,
and acquisition and loss of *ces* is "a dynamic, ongoing process that occurs
across lineages", with two independent gains and three losses documented inside
sequence type 26 alone — including losses in isolates that then caused
**diarrhoeal** illness [28]. **So the same species, even the same lineage,
oscillates between the two enemy types, and which one you face is carried on a
plasmid.** That is a genuinely unusual design object: one species tag, two
completely different units, and a §5 knowledge rung that cannot honestly be
shared between them.

**Morphology for a sprite.**
- ***B. cereus*:* a **large, square-ended rod, 4–5 µm long × 1.0–1.5 µm wide,
  with the spore located centrally or paracentrally** [32]; it grows in **chains
  of rod-shaped cells**, and cell width in the type strain measured 1.19 µm
  (±CI95% 0.03) [33]. **Distinguishable from a plain bean:** strongly — it is
  2–2.5× the length of the existing ~2 µm bean, has *square* ends rather than
  round, and carries a central spore. A long brick with blunt corners and a
  bright middle, in chains.
- ***S. aureus*:* spherical cells in grape-like clusters. **Diameter in µm:
  UNVERIFIED in this pass.** **Distinguishable:** yes, categorically — it is the
  only *coccus* in the candidate set, so a tight tetrad or cluster of small discs
  is unlike every rod on the board. But see the design note below: for the emetic
  wave, **the organism should probably not be drawn at all.**
- **The sprite problem, stated plainly.** For a preformed-toxin wave there is no
  organism in the gut. Drawing *S. aureus* or *B. cereus* cells in the lumen
  would be a lie that teaches the player the wrong thing — that there is
  something to shoot. **Draw the toxin: a drifting field or cargo particle with no
  cell in it.** This is the one candidate where the right answer is *not* a
  bacterium sprite.

**Exposure route / vehicle.** *B. cereus* emetic: **fried rice balls**, with the
measured 37 µg/g [30] — rice cooked, left out, then fried. *S. aureus*:
**ham-and-cheese sandwiches at a family lunch**, eight people, onset three hours
[36]; classically processed meat and dairy contaminated by handling and then held
warm [34]. A cake with cream left out is the same story.

**Burden.** *S. aureus* **241 148 US domestic foodborne illnesses/yr, 1 064
hospitalisations, 6 deaths**; *B. cereus* **63 400, 20 hospitalisations, 0
deaths** [2]. **Neither is tracked by FoodNet** [3] **nor modelled in GBD 2021 or
WHO's 14-hazard diarrhoeal set** [1][4] — so global burden for both is
**UNVERIFIED**, and their US figures come from outbreak surveillance scaled by a
25.5× underreporting multiplier [2], which is the weakest method in that paper.
Age skew: UNVERIFIED. **Rhythm: foodborne outbreak, the fastest in the game** —
and *the only wave that can resolve before the player has done anything.*

**The player's counter. Nothing in the roster can touch it, and that is the
point.** No tower can target it, both flush defences act on organisms, the
stress-sense meter needs an infected cell, and complement needs a surface.
**Antibody-mediated toxin neutralisation is the only honest answer**, and it is
*not* the `SetCoated` path §4c and §5's IgA rung already implement — coating
pacifies an organism, neutralisation destroys a molecule's function. **This wave
is the argument for building neutralisation as a second, distinct job of the
antibody tower.**

**Redundancy check.** Nothing to compare it to: it is the only candidate with no
organism in it. Explicitly **not** ETEC or cholera — those secrete a toxin *from
a living adherent crowd the player can kill*, which is a completely different
decision.

---

### 4.4 *Vibrio parahaemolyticus* — **CORRECTING the "hold, mechanically redundant" verdict**

**The extracellular pass held it as "EHEC with a cytolysin and fewer
distinctives". That reading misses the trait that makes it interesting.**

**The one mechanic.** *Adheres, then **arms itself on a host chemical cue**.*
T3SS2 — the system that actually causes enterotoxicity — has its gene expression
**induced by bile exposure** [41], and the Vp pathogenicity island carries
"sophisticated mechanisms for sensing the intestinal environment and host cell
contact" [42]. So it is inert cargo in the food and in the early lumen, and it
becomes a weapon only after it tastes something only the small intestine
contains. **Nothing on the board has a trigger that is a property of the
anatomy rather than a property of the player.** That is a different kind of
difficulty: a wave that is harmless until it reaches a particular band, and
*cannot* be pre-empted by killing it early in the lumen — only by not letting it
get there.

**The numbers.** **Incubation 17 h median — in a band entirely of its own**,
between the 4–10 h preformed toxins and the 32–45 h invasives, in the same
15-year dataset [10]. Division of labour between its weapons is measured:
**T3SS1 → cytotoxicity** against all cell lines tested; **T3SS2 → the major
contributor to enterotoxicity** (confirmed by deletion and complementation
[41]); **TDH + T3SS1 → lethality** in mice [40]. **Infectious dose: UNVERIFIED**
(10⁵–10⁷ is widely quoted; no primary volunteer source found in this pass or the
last). **In vivo doubling: UNVERIFIED.**

**Morphology for a sprite.** Halophilic Gram-negative curved rod with a polar
flagellum. **Dimensions in µm: UNVERIFIED in this pass.** **Distinguishable from
a plain bean:** *weakly* — it is close to a comma, which is what *V. cholerae*
already is. **If it is built, it should be a palette-and-silhouette sibling of
cholera, not a new shape**, and the honest thing is to say so: the visual budget
is better spent elsewhere.

**Exposure route / vehicle.** **Raw oysters in summer.** Sourced and specific: a
nationwide Australian outbreak, **268 people, 97% of whom had eaten
Australian-grown oysters**, traced to one growing region in South Australia and
ending in a national recall [38]. Pooled prevalence in seafood is **19.6% (95%
CI 13.7–27.4)** for *V. parahaemolyticus*, of which roughly **one in five
positive samples carries pathogenic strains** [39]. Temperature and salinity are
the dominant environmental drivers and occurrence is expected to **increase with
coastal warming** [39] — a seasonal round modifier with a real basis.

**Burden.** **34 664 US domestic foodborne illnesses/yr, 100 hospitalisations, 4
deaths** [2]; all-*Vibrio* FoodNet incidence **1.0 per 100 000 in 2022, up
significantly on 2016–18 (IRR 1.57, 95% CrI 1.37–1.81)** [3]. In China: detected
in **3.11% of 79 075 diarrhoeal samples across 11 provinces** and **8.1% of 6 951
acute-diarrhoea samples in eastern China**; Zhejiang alone documented **383
outbreaks and 4 382 cases between 2010 and 2022** [37]. It is described as **a
leading cause of seafood-associated gastroenteritis worldwide** [37][39][40].
**Rhythm: foodborne outbreak, seasonal, and growing.**

**The player's counter.** The same answers as any adherent extracellular
organism — flush, IgA, direct damage — but all of them must be applied **before
it reaches the band where bile arms it**, which is a genuinely new timing
decision rather than a new tower.

**Redundancy check.** Against **EHEC**: EHEC's mechanic is a toxin aimed *past*
the wall at the vasculature; this is a T3SS aimed *at* the epithelium, and its
distinguishing feature is the **trigger**, not the payload. Against **ETEC**: ETEC
is two toxins on two clocks, always on. Against ***V. cholerae***: cholera
subverts without killing. **Verdict: it earns a slot on the bile trigger alone,
and on nothing else.** If the bile cue is not going to be built, the earlier
pass's "hold" is correct and it should stay held.

---

### 4.5 *Yersinia enterocolitica* — **CORRECTING the "anti-tower unit" framing**

The invasive pass's headline — "it attacks the towers, not the wall" — is **half
right, and the half that is wrong changes how it must be built.**

**What is confirmed.** YopJ/YopP-mediated phagocyte killing is real and fast:
wild-type *Y. pseudotuberculosis* kills **~70% of infected macrophages in an 8 h
infection**, the death is **apoptosis**, attachment via invasin or YadA is
required, and YopJ is **necessary** — a mutant lacking it causes no host-cell
death, and the phenotype is restored by supplying *yopJ* in trans [53].

**Three corrections.**
1. **It kills macrophages, not neutrophils.** YopJ and YopP similarly induce
   apoptosis in human monocyte-derived macrophages, but **human neutrophils
   interacting with virulence-plasmid-containing *Yersinia* showed no caspase
   activation, no LDH release and no loss of membrane integrity above control**
   [54]. Worse for the simple reading: neutrophils facing the
   plasmid-*deficient* strain *did* die, because robust PMN ROS production killed
   them, and the T3SS **protects** the neutrophil by suppressing its ROS burst
   [54]. **So in game terms it does not kill "the tower" — it kills the
   macrophage and *pacifies* the neutrophil.** Those are two different debuffs
   and the roster has neither.
2. **Killing the phagocyte is not straightforwardly good for it.** Reverting
   YopJ to the ancestral, *more* cytotoxic variant produced a strain with
   enhanced macrophage cytotoxicity that was **completely avirulent** in a murine
   model; strains lacking functional YopJ **failed to kill macrophages but
   regained virulence**; and pharmacologically blocking the apoptosis *enhanced*
   virulence [55]. Codon 177 shows positive selection, and the direction of
   selection has been **toward diminished cytotoxicity and increased virulence**
   [55]. **Design consequence: build it as a trade, not a buff.** An enemy whose
   tower-killing attack makes it *weaker* is a far more interesting object than a
   strictly-better enemy — and it is what the literature says.
3. **Its best-sourced trait is not Yop at all — it is complement resistance.**
   **YadA is the single most important determinant of *Y. enterocolitica* serum
   resistance**; YadA and Ail both bind **C4b-binding protein**, the inhibitor of
   the classical and lectin pathways, and the bound C4bp stays functionally
   active in factor-I-mediated C4b degradation [56]. YadA also recruits C3b and
   iC3b **directly**, without an active cascade, and uses them to attract factor
   H, limiting terminal-complex formation [58]; it inhibits complement at **both
   the C3 and the C9 level**, so less C5b-9 is deposited, and it additionally
   protects against the lytic action of whatever complex does form [57]. Full
   trimeric stability of YadA is essential [58]. **Given that complement left the
   §5 knowledge ladder and became a stacking body upgrade, *Yersinia* is the
   organism that specifically defeats that purchase.** That is a better and
   cheaper mechanic than the Yop injection, and the roster currently has no enemy
   that answers a *body upgrade*.

**The numbers.** Infectious dose **UNVERIFIED** (unchanged). Incubation 4–6 days
typically, 1–2 weeks range; shedding up to 40 days (retained from the invasive
pass). Macrophage killing: **~70% in 8 h** [53].

**Burden — and it is rising fastest of anything FoodNet tracks.** 97 656 US
domestic foodborne illnesses/yr, 533 hospitalisations, 29 deaths [2]. FoodNet
2022: **2.0 per 100 000, against a 2016–18 average of 0.9 — IRR 2.41 (95% CrI
2.03–2.88), the largest increase of any bacterium in the table** [3]. Note the
surveillance caveat the same table supplies: 91% of *Yersinia* diagnoses were by
culture-independent test and only **24% of reflex cultures yielded an isolate**,
down from 48% — so some of the rise is better detection [3]. **Rhythm: endemic,
with the steepest trend.**

**Morphology for a sprite.** Gram-negative rod. **Dimensions in µm: UNVERIFIED.**
**Distinguishable from a plain bean: barely.** Its only drawable distinctives are
behavioural, not morphological — **extracellular microcolonies** (so draw it as a
tight self-adherent clump rather than as individuals, which is also what YadA
autoaggregation does [58]) and the needle it injects. A rod that only ever
appears as a cluster is at least a different *silhouette at the group level*.

**The player's counter.** Macrophages are the wrong answer by construction;
**neutrophils are the right answer and the literature says so** — they resist the
Yop apoptosis [54] and *Shigella*-style protection generalises: the invasive
pass already found that for *Shigella* protection is IgA/IgG licensing
neutrophils. For the complement evasion there is **no counter in the roster**.

**Redundancy check.** Against the generic large bacterium: it does not take a
slot and does not fight the wall. Against A/E *E. coli*: no inflammation
respiration. The dimension in which it is unique: **it is the only candidate
whose primary target is the player's units rather than the player's tissue** —
with the correction that the targeting is macrophage-specific and self-costly.

---

### 4.6 *Listeria monocytogenes* — **EXTENDING: it arrives non-motile, and the Paneth gate actually works on it**

Two additions to an entry that stands otherwise. Both are cheap to build.

**It is non-motile at body temperature.** *L. monocytogenes* produces a flagellum
**at or below 30 °C outside the host** and **halts flagellar expression at 37 °C
inside the human host, to evade the flagellum-induced immune response** [67].
The mechanism is a protein thermometer: GmaR is a thermosensing anti-repressor
that neutralises the master flagellar repressor MogR at ≤30 °C and **aggregates
and is degraded at 37 °C**, de-repressing MogR [67][69]. Flagellar motility
genes are transcriptionally down-regulated at 37 °C [68], and the repression is
not merely incidental — **deleting *mogR* costs a 250-fold decrease in
virulence** in vivo, and MogR represses *flaA* during intracellular infection
independently of temperature [68].

**Game consequence, and it is nearly free.** Listeria should **drift**, not swim.
The food it rode in on was refrigerated, where it was motile; by the time it is
in the lumen it has switched the engine off. That is the game's existing default
behaviour, so the implementation cost is a flag that reads "never motile", and
the *fiction* is a real and slightly startling fact. It also sharpens the
contrast with *Campylobacter* (§4.2), which swims harder the deeper it gets.
Note the honest nuance: flagellar motility does contribute to Listeria's
**initial** host-cell invasion [69], so the trait is "non-motile in the lumen",
not "motility is irrelevant".

**The Paneth-cell gate works on it, and that is a teachable asymmetry.**
Recombinant human intestinal defensin **HD-5** — the Paneth α-defensin of the
small-intestinal crypt — is microbicidal against *L. monocytogenes*, and
crucially **remains bactericidal to *Listeria* at every salt concentration
tested (0–150 mM NaCl) and its activity against *Listeria* is not pH-dependent**
[70]. Against *S.* Typhimurium, by contrast, HD-5 activity is **abolished at
100 mM NaCl** and diminished at pH 5.5 — and that acid-induced resistance is
**phoP-mediated** [70]. **So the same host peptide reliably kills one shortlisted
organism and is conditionally defeated by another.** HD-5's in vivo weight is
established: transgenic mice expressing it in Paneth cells were **markedly
resistant to oral challenge with virulent *S.* Typhimurium** [71]. One further
wrinkle if a Paneth-cell mechanic is ever built: in human duodenal fluid HD-5 is
**proteolysed into many still-active fragments** — incubating it with mucus
produced 255–8 000 new antimicrobial combinations — while **HD-6 stays intact and
forms nanonets** [72]. So the gut's two Paneth defensins are two different
mechanics: a diversifying soluble killer and a physical trap.

---

### 4.7 *Salmonella* Typhimurium — **EXTENDING: antimicrobial-peptide resistance, with its real limit**

The invasive pass covered its two niches and its extrusion route. The missing
evasion trait is **cationic antimicrobial-peptide resistance**, and it is the
organism that defines the mechanism.

**The mechanism.** The **PhoP–PhoQ** and **PmrA–PmrB** two-component systems
remodel **lipid A** — adding aminoarabinose and other substituents that reduce
the LPS's net negative charge and so lower AMP binding affinity [75]. PmrAB is
activated in vivo and regulates the LPS modifications that let *Salmonella*
survive innate killing at intestinal mucosal surfaces as well as inside
macrophages [73]. PhoP–PhoQ governs resistance to structurally unrelated peptides
— α-helical magainin 2, β-sheet **defensins**, and polymyxin B — through *different*
determinants: *ugtL* (monophosphorylated lipid A) is needed for magainin and
polymyxin resistance, *yqjA* for magainin only, and *pmrA* for polymyxin only
[74].

**The honest limit, which matters more than the headline.** In the same study,
**deleting *ugtL* did not affect resistance to the human neutrophil defensin
HNP-1**, and *yqjA* was likewise not required for it [74] — i.e. "different
PhoP-mediated modifications are necessary for resistance to different
antimicrobial peptides", and the defensin case is not covered by the ones
measured. And as §4.6 records, **HD-5 kills both wild-type and *phoP*-mutant
*S.* Typhimurium** [70]; what *phoP* buys against HD-5 is **acid-dependent**
resistance at pH 5.5, not blanket immunity [70]. **Do not build a flat "immune to
Paneth defensins" flag for *Salmonella*.** The defensible build is
**conditional**: its AMP resistance should depend on local conditions (the
literature's conditions are salt and pH), which is a more interesting mechanic
anyway and is exactly what the sources support.

---

### 4.8 *Shigella* — **CONFIRMED without change**

Nothing in this pass alters the invasive pass's rank-1 verdict, and the
prevalence lens strengthens it: **the under-5 fatal population-attributable
fraction of *Shigella* is 24.0% (95% UI 15.2–37.9) — second only to rotavirus
(35.2%) and ahead of adenovirus (23.8%) across all 13 modelled aetiologies**
[1], and *Shigella* is among the top three hazards for foodborne DALYs globally
in 2021 [4]. In the US it is 131 254 illnesses/yr [2] at 4.9 per 100 000, with
**30.6% of cases hospitalised — the highest of any bacterium in FoodNet except
*Listeria*** [3]. It is the one organism that is top-tier on prevalence,
lethality *and* mechanics. **Build it.**

---

## 5. Cuts, and exactly why

The rule is the roster's: *a species earns a slot by forcing a decision no
existing enemy forces.* Four cuts, four different reasons.

### 5.1 *Aeromonas* spp. — **cut on the mechanic rule, NOT on causality. This CORRECTS the previous pass's reasoning.**

The extracellular pass rejected it on the grounds that "its causal role in
diarrhoea is still contested after fifty years, and a 447-genome GEMS analysis
found little evidence of genetic determinants for diarrhoeal disease."
**The second half is accurately quoted but the conclusion drawn from it is too
strong, and two sources contradict the framing:**

- ***Aeromonas* is one of the thirteen diarrhoeal aetiologies GBD 2021 models**
  [1]. It is not a fringe organism in the burden literature; it is in the
  canonical list alongside rotavirus, *Shigella* and *Campylobacter*.
- **GEMS itself names it.** Kotloff 2013 states that beyond the four leading
  attributable causes, "other pathogens were important in selected sites (eg,
  ***Aeromonas***, *Vibrio cholerae* O1, *Campylobacter jejuni*)" [9].
- The 447-genome study is more nuanced than "no determinants": alongside its
  stated finding of little evidence for determinants, it reports that **three
  gene loci were significantly associated with moderate-to-severe diarrhoea in at
  least one cohort group**, and that polar- and lateral-flagellar components
  showed a weak association with gastroenteritis isolates [43].

**So: the organism is real, it causes diarrhoea, and it is site-importantly
common. It is still cut — but for the right reason: it has no ONE mechanic.** Its
own genomics paper's headline finding is the *absence of a stable virulence
signature* [43], which in game terms means there is nothing to build. An enemy
whose defining property is "heterogeneous and hard to characterise" is a
*knowledge-system* idea, not a unit. **Banked** with a note: if §5 ever wants an
organism whose knowledge percentage is intrinsically hard to raise because the
species is not one thing, this is the literature-exact candidate.

### 5.2 *Plesiomonas shigelloides* — **cut on anatomy and burden**

**Anatomy.** In the cleanest clinical series available, **78% of *P.
shigelloides*-infected patients had findings suggestive of colitis** [45]. That
is the same disqualification the roster applied to *C. difficile*, and it should
be applied consistently.

**Burden.** It is genuinely rare: **1 of 299 stools (0.3%)** in a Singapore acute
gastroenteritis panel [47]; **22 published cases of bacteraemia worldwide, all
single-patient case reports** (case fatality 27.3%) [46]; and it is described as
a cause of "sporadic cases of gastroenteritis" and "low-incidence disease" [44].
The one high number is local and striking — **9% of diarrhoeic stools and 17% of
surface water** in a Cape Town informal settlement [48] — but that is a setting
signal, not a global one. In a 30-case series, **71% had recent tropical travel**
and the other 29% acquired it locally from seafood or untreated water [45].

**Mechanic.** No distinctive one was found in this pass. **Vehicle worth keeping
in the writers' bank, because it is excellent and sourced:** freshwater and
marine sources and their inhabitants — **fish, shellfish, amphibia and reptiles**
[44]. If a "pet turtle" round callout is ever wanted, this is the organism it
belongs to.

### 5.3 Enterotoxigenic *Bacteroides fragilis* — **cut on anatomy. Bank it with *C. difficile*.**

**It is a colonic organism, and the sources are unambiguous about it.** BFT binds
a **colonic epithelial cell receptor** — identified by genome-wide CRISPR screen
as **claudin-4** — and cleaves the **E-cadherin ectodomain**, disrupting the
barrier and driving inflammation and c-Myc-dependent hyperproliferation [49];
the disease associations are **colitis and colorectal cancer** [49][50], and the
mouse work is in **colonic** epithelial lines [85]. ETBF strains are associated
with diarrhoeal disease in children [51], and **~20% of the human population are
asymptomatic carriers** [50] — real prevalence, wrong compartment.

**Two things worth banking rather than discarding.**
1. **It is the best candidate for a colon-map "commensal that turns"**: *B.
   fragilis* is a ubiquitous commensal, *Bacteroides* is ~30% of the gut
   microbiota, and a subset carries one mobile genetic element encoding one
   toxin — "one toxin away from disease" [50]. The game's commensal lawn
   (`pathogens_extracellular.md` §4) becoming an enemy is a far better mechanic
   than another toxin.
2. **A real convergence worth noting:** **BFT and *C. perfringens* enterotoxin
   both use claudin-4 as their receptor** [49][11]. Two unrelated toxins, the same
   door. If the game ever wants a shared "tight-junction receptor" trait rather
   than per-organism bespoke toxin code — the way the earlier pass proposed a
   shared `mucinase` trait — this is the sourced basis for it.

**One honest flag on the antibody-degradation brief:** ETBF's *B. fragilis* group
isolates from children with diarrhoea were **all capsulated and serum-resistant**
[52], which is a complement-evasion trait, not an antibody-degradation one. See
§7.

### 5.4 *Clostridioides difficile* — **the rejection stands, and the burden makes it more important to say why**

**CONFIRMING the roster's rejection, and adding the number that makes it
uncomfortable.** *C. difficile* is **the dominant diarrhoeal pathogen of
high-SDI regions (age-standardised death rate 0.53) and the only one of the 13
modelled aetiologies positively correlated with socio-demographic index
(ρ = 0.63)** [7]; GBD 2021 attributes ~**15 600 deaths globally, 13 100 of them
in high-income regions, predominantly in people aged ≥70** [1]; and it is the
fastest-growing of the 13, ASDR rising **0.19 → 0.43 between 1990 and 2019**,
correlated with national antibiotic consumption [8].

**It is still wrong for this map** — colonic, and its trigger requires a dense
microbiota the small intestine does not have — but **"rejected on anatomy" must
never be shortened to "not a major pathogen"**, because for a player in a
high-income country it is *the* one. The roster's framing ("banked as the
headline act for a future colon map") is exactly right and the burden data makes
the colon map more attractive, not less. Its spore-based transmission [83] would
also pair with *C. perfringens*'s sporulation if both are ever on the same board.

---

## 6. Corrections to standing design

1. ***C. perfringens* is not a preformed-toxin organism, and the one place it
   appears in our docs says it is.** `pathogens_extracellular.md` §3.6 writes:
   "preformed-toxin outbreaks (*S. aureus*, *B. cereus*, *C. perfringens*) have
   4–10 h median incubations". The grouping is quoted faithfully from the
   incubation-period paper, which does label that group "preformed bacterial
   toxins" [10] — **but it is mechanistically wrong for *C. perfringens*, which is
   the exact opposite case.** CPE is **sporulation-dependent and produced by cells
   sporulating in the small-intestinal lumen** [11][12]; onset is **8–24 h** [13]
   (range 2–36 h [14]), *slower* than the true preformed toxins precisely because
   the organism has to get there and sporulate first. Shipping it in the
   preformed-toxin slot would delete the most interesting thing about it. **Fix
   the sentence in §3.6 and give it its own slot.**

2. **§1b Step 1's "bacteria replicate while on the wall, damped by local
   density" has never been tested against a real replication rate, and
   *C. perfringens* is 6× faster than anything the tuning assumed.** 7.1 min per
   generation at 41 °C [15][16] against *Salmonella*'s in vivo 2.86–5.02 h
   (invasive pass). That is a ~40× spread within the same mechanic. **Either the
   density damper is doing nearly all the work, in which case the published rates
   are decorative and the doc should say so, or the per-species rate is a real
   dial and this organism will break the plaque floor.** Measure before tuning:
   this is a test, not a guess.

3. **§1b models adherents as drifting in the lumen flow and the mucus band, and
   that is wrong for exactly one strong candidate — in the direction that matters
   most.** *Campylobacter jejuni*'s **swimming velocity increases as external
   viscosity increases** [60], it wraps a flagellum round its body to achieve that
   in high-viscosity media [59], and shorter filaments both slow it and reduce
   colonisation [61]. **So the mucus layer is the one place it is *fastest*, and
   §6b's mucus-turnover upgrade should make it worse, not better.** Conversely the
   doc's generic "motile bacterium" is wrong for two others: *C. perfringens*
   **lacks flagella entirely** and glides only on surfaces [19][20][21], and
   *L. monocytogenes* **shuts flagellar expression off at 37 °C** [67][68][69].
   **Motility should be a per-species property with three settings — swims, drifts,
   never swims — and the sources exist for all three.**

4. ***V. cholerae* — which is already built — swims, and the thing that makes it
   infectious is net displacement, not steering.** *V. cholerae* motility and
   chemotaxis both matter for colonisation: *degS*/*rpoE* deletion cuts both and
   cripples colonisation [77], and strains locked into either the "off" or the
   "on" form of the flagellar regulator FlrB are **10- to 40-fold** defective for
   intestinal colonisation in infant mice [78]. But the counterintuitive result is
   the one that bears on §1b: **non-chemotactic mutants greatly *out-compete*
   wild type during small-intestinal infection** — and that advantage requires
   **counterclockwise-biased flagellar rotation and smooth straight runs**, while
   a non-chemotactic mutant with clockwise bias was confined to small-scale
   movement and was **attenuated**. Counterclockwise-biased mutants are
   **aberrantly distributed throughout the small intestine**, exploiting niches
   the steering wild type does not reach [76]. **Design consequence: cholera's
   real behaviour is not "steer toward the wall" but "run straight and get
   everywhere", and the game's model of a pathogen that drifts and then rolls a
   proximity-gated adhesion chance understates it.** §5e's lane-changing (Sprint
   25) is the right place to express it: cholera should spread itself across the
   channel faster than anything else, which raises its effective adhesion odds
   without touching the adhesion roll itself.

5. **§4c's complement body upgrade (opsonise + recruit a phagocyte) has an enemy
   that defeats it, and the roster does not say so.** *Y. enterocolitica*'s
   **YadA is its single most important serum-resistance determinant**; YadA and
   Ail bind **C4b-binding protein** and keep it functionally active [56]; YadA
   recruits C3b/iC3b directly and uses them to pull in **factor H**, limiting
   terminal-complex formation [58]; and it inhibits complement at **both C3 and
   C9**, reducing C5b-9 deposition and additionally surviving what does deposit
   [57]. **An enemy that answers a *body upgrade* rather than a tower is a shape
   the game does not yet have.**

6. **The "anti-tower unit" framing of *Yersinia* needs three corrections** — it
   kills **macrophages but not human neutrophils** [54], the T3SS actually
   **protects** the neutrophil by suppressing its ROS burst [54], and the more
   cytotoxic the YopJ variant the **less** virulent the organism is in vivo
   [55]. Build it as a **trade**, and make the neutrophil the answer.

7. **The §5 mutation discount has a sourced, behaviour-triggered mechanism and it
   is not in a virus.** *Campylobacter*'s phase-variable MeOPN capsule
   modifications **block pre-existing antibody from reaching the capsule** and
   **exposure to normal human serum selects for the evasive variant** [65].
   `PATHOGEN_ROSTER.md` currently assigns antigenic replacement to norovirus
   GII.4 on a 2–4 year calendar. **A discount that fires in proportion to the
   player's own antibody investment is a better mechanic and it is a bacterium.**

8. ***Aeromonas* should not be described as having a contested causal role full
   stop.** It is one of GBD 2021's thirteen modelled diarrhoeal aetiologies [1]
   and GEMS names it as site-importantly attributable [9]. The cut is correct; the
   stated reason is not. See §5.1.

9. ***V. parahaemolyticus* is not "EHEC with a cytolysin".** Its T3SS2 — the
   system that causes enterotoxicity — is **induced by bile** [41], a host-anatomy
   trigger that nothing in the roster has. See §4.4.

10. **The roster should not be presented to the Director as a list of common
   organisms.** Toxigenic *V. cholerae*, which is built, is **84 US cases a year**
   [2]; *Listeria*, shortlisted, is **1 591** [2]; *C. perfringens*, absent from
   all four passes, is **~966 000** [2]. Every one of those choices is defensible
   under the mechanic rule — **the rule is right** — but the *description* needs a
   sentence admitting that mechanical rank and prevalence rank are different
   orderings, so that "the Director asked for common gut pathogens" produces a
   real re-ranking rather than a re-justification of the existing list.

---

## 7. Still with no answer in the roster — additions to that list

Flagged so a future tower or mechanic can be justified by a real gap.

- **A toxin that is already in the food.** No tower can target it, both flush
  defences act on organisms, and `SetCoated` pacifies a cell rather than
  neutralising a molecule. **Antibody-mediated toxin neutralisation** is the only
  honest answer and it is a *second, distinct job* for the antibody tower (§4.3).
- **A dormant form nothing can kill.** Spores. *C. perfringens* c-*cpe* spores at
  δ₉₅ ᵒC ≈ 49 min [18], ~30× the heat resistance of p-*cpe* spores [17]. The
  board has no concept of an organism that is present, inert, and immune.
- **A pathogen that gets faster in mucus.** *Campylobacter* [59][60]. The mucus
  upgrade currently has one perverse interaction (EAEC's Pic); this is a second,
  different one.
- **Antigenic variation selected by the player's own antibody pressure** [65].
- **An enemy that defeats a body upgrade rather than a tower** — *Yersinia*'s
  C4BP/factor-H recruitment [56][57][58].
- **A trigger that is a property of the anatomy, not the player** — bile-induced
  T3SS2 arming [41][42].
- **"The tower dies from what it ate"** — still unanswered, and this pass changes
  *who* should carry it: **secreted PFO/α-toxin from *C. perfringens*** [22][23]
  is a better fit than *Yersinia*'s injected Yops, because it kills human
  mononuclear cells as a field effect [23] and because **recruiting more
  neutrophils makes the damage worse** [22].
- **Antibody degradation — and the surprise is who does it.** The brief asked
  whether any of these organisms degrades antibody. **In this pass, the only
  gut IgA protease found belongs to a *commensal*, not a pathogen**: a
  sub-lineage of ***Sutterella wadsworthensis*** encodes a protease that **digests
  human IgA1 and IgA2** (but not mouse IgA), and its activity **altered the
  trajectory of *Campylobacter jejuni* infection in human epithelial cells and
  phagocytosis by primary neutrophils** [79]. Carriers are detectable globally
  and have a measurably different microbiome [79]. The counterpart finding is the
  same shape from the other direction: *Paraprevotella* strains degrade luminal
  trypsin and thereby **protect IgA from degradation**, improving oral vaccine
  efficacy [80]. **So in this anatomy, whether the player's §5 ~70% IgA rung works
  at all is partly a property of the commensal lawn** — which turns
  `pathogens_extracellular.md` §4's "protectable commensal lawn" from flavour
  into a prerequisite for the game's most important adaptive rung. That is the
  single most game-shaped finding in this pass after *C. perfringens*.

---

## 8. What I would build next, in order

1. ***C. perfringens*** — top-3 on both rankings, three mechanics the board has
   no answer to, the best sprite distinctive in the set, and it fills the
   roster's largest prevalence hole.
2. ***Campylobacter jejuni*** — the most common enteric bacterium in a
   high-income gut, the most visually distinct silhouette available, and two
   traits that invert existing upgrades.
3. **The preformed-toxin wave** (*S. aureus* + emetic *B. cereus*, one slot, two
   skins) — cheap, fast, and the argument for building toxin neutralisation.
4. ***Shigella*** — unchanged; highest under-5 fatal PAF of any bacterium and the
   strongest intracellular mechanic.

Then *Salmonella*, then *Yersinia* rebuilt as a trade. *V. parahaemolyticus* only
if the bile trigger is built. *Listeria* whenever the CD8 rung needs its teaching
organism, with the non-motility flag set.

---

## 9. References

PubMed-sourced articles carry DOI links; Consensus-sourced articles carry
Consensus record links.

**Burden and surveillance**

1. GBD 2021 Diarrhoeal Diseases Collaborators (Kyu H, et al.). Global, regional,
   and national age-sex-specific burden of diarrhoeal diseases, their risk
   factors, and aetiologies, 1990–2021. *Lancet Infect Dis* 2025;25(5):519-536.
   PMID 39708822. [DOI](https://doi.org/10.1016/S1473-3099(24)00691-1) ·
   [Consensus](https://consensus.app/papers/details/72054795006551b49ae7c2a39fcc89cd/)
2. Scallan E, et al. Foodborne illness acquired in the United States — major
   pathogens. *Emerg Infect Dis* 2011;17(1):7-15. PMID 21192848.
   [DOI](https://doi.org/10.3201/eid1701.P11101)
3. Delahoy MJ, et al. Preliminary incidence and trends of infections caused by
   pathogens transmitted commonly through food — FoodNet, 10 U.S. sites, 2022.
   *MMWR* 2023;72(26):701-706. PMID 37384552.
   [DOI](https://doi.org/10.15585/mmwr.mm7226a1)
4. Majowicz SE, et al. WHO estimates of the global, regional, and national burden
   of 14 foodborne diarrhoeal enteric hazards, 2000–21. *Lancet Glob Health* 2026.
   [Consensus](https://consensus.app/papers/details/2f911b511c625151861ee131750c1610/)
5. Havelaar AH, et al. WHO global estimates and regional comparisons of the
   burden of foodborne disease in 2010. *PLoS Med* 2015.
   [Consensus](https://consensus.app/papers/details/5b48b471b77e507cb84c0c3d029ce1cc/)
6. Kirk M, et al. WHO estimates of the global and regional disease burden of 22
   foodborne bacterial, protozoal, and viral diseases, 2010. *PLoS Med* 2015.
   [Consensus](https://consensus.app/papers/details/9e20b08dd1735bc1ab0b7d576b2594dc/)
7. Sang S, et al. The GBD 2021 perspective: COVID-19's impact on diarrheal
   mortality and etiological trends, 1990–2021. *Front Cell Infect Microbiol*
   2025;15:1668444. PMID 41346363.
   [DOI](https://doi.org/10.3389/fcimb.2025.1668444) ·
   [Consensus](https://consensus.app/papers/details/3b9d474233255cfd992e1313f4ef540c/)
8. Chen Y-H, et al. The global burden and trend of *Clostridioides difficile* and
   its association with world antibiotic consumption, 1990–2019. *J Glob Health*
   2024.
   [Consensus](https://consensus.app/papers/details/30394c27576352cbb0fe43915e9d9c6b/)
9. Kotloff KL, et al. Burden and aetiology of diarrhoeal disease in infants and
   young children in developing countries (GEMS). *Lancet* 2013.
   [Consensus](https://consensus.app/papers/details/4de7ae0a038850bcb6926d4385c0c5e6/)
10. Chai SJ, et al. Incubation periods of enteric illnesses in foodborne
    outbreaks, United States, 1998–2013. *Epidemiol Infect* 2019.
    [Consensus](https://consensus.app/papers/details/b50639e235c9591bab460cd3e83faf7a/)
81. Zhou Z, et al. Food system drivers of global foodborne bacterial disease
    burden: pathogen-specific attribution using 1990–2021 GBD 2021 and FAO food
    system databases. *Int J Food Microbiol* 2026;459:111883. PMID 42241982.
    [DOI](https://doi.org/10.1016/j.ijfoodmicro.2026.111883)
83. Buddle JE, Fagan RP. Pathogenicity and virulence of *Clostridioides
    difficile*. *Virulence* 2022.
    [Consensus](https://consensus.app/papers/details/6a1b1f069fd15883b4c04dbbe50e641c/)

***Clostridium perfringens***

11. Shrestha A, Mehdizadeh Gohari I, Li J, Navarro M, Uzal FA, McClane BA. The
    biology and pathogenicity of *Clostridium perfringens* type F. *Microbiol Mol
    Biol Rev* 2024;88(3):e0014023. PMID 38864615.
    [DOI](https://doi.org/10.1128/mmbr.00140-23)
12. Caserta JA, et al. Development and application of a mouse intestinal loop
    model to study the in vivo action of *Clostridium perfringens* enterotoxin.
    *Infect Immun* 2011;79(8):3020-7. PMID 21628512.
    [DOI](https://doi.org/10.1128/IAI.01342-10)
13. Stringer M. *Clostridium perfringens* type A food poisoning.
    [Consensus](https://consensus.app/papers/details/fd8eb221d883540d8b8b2898eac4ae0f/)
14. Zhang D-r, et al. A *Clostridium perfringens* related foodborne diarrhea
    outbreak in an elderly care center — Beijing, May 2024. *China CDC Weekly*
    2025.
    [Consensus](https://consensus.app/papers/details/68ebcde32982506ba27b4d6be20de179/)
15. Willardsen RR, et al. Growth and survival of *Clostridium perfringens* during
    constantly rising temperatures. *J Food Sci* 1978.
    [Consensus](https://consensus.app/papers/details/d53a11f54f7b5e02a52a1e4591b315e9/)
16. Labbé RG, et al. Generation times and modeling of enterotoxin-positive and
    enterotoxin-negative strains of *Clostridium perfringens* in laboratory media
    and ground beef. *J Food Prot* 1995.
    [Consensus](https://consensus.app/papers/details/3450c6dbde065c7e9df73644e36c27fc/)
17. Mehdizadeh Gohari I, et al. Heat resistance differences are common between
    both vegetative cells and spores of *C. perfringens* type F isolates carrying
    a chromosomal vs plasmid-borne enterotoxin gene. *Appl Environ Microbiol*
    2024. (Replication of a retracted 2000 paper — cite this one.)
    [Consensus](https://consensus.app/papers/details/98997ef07ed85f20bb2f8f620a2b9675/)
18. Firmesse O, et al. Sporulation abilities and heat resistance of *Clostridium
    perfringens* strains isolated from French foodborne outbreaks. *Foods* 2025.
    [Consensus](https://consensus.app/papers/details/de0933cb234e596a948ecad7ec79a57e/)
19. Varga JJ, et al. Type IV pili-dependent gliding motility in the Gram-positive
    pathogen *Clostridium perfringens* and other Clostridia. *Mol Microbiol* 2006.
    [Consensus](https://consensus.app/papers/details/2ae4d90e01fc577fb592036db9b41ba3/)
20. Soncini SR, et al. Changes in the expression of genes encoding type IV
    pili-associated proteins when *C. perfringens* is grown in liquid or on
    surfaces. *BMC Genomics* 2020.
    [Consensus](https://consensus.app/papers/details/6f0f2a1040cb52a1befc02b6ea05b62a/)
21. Liu H-L, et al. Use of a mariner-based transposon mutagenesis system to
    isolate *C. perfringens* mutants deficient in gliding motility. *J Bacteriol*
    2012.
    [Consensus](https://consensus.app/papers/details/2aa13e720dcd53949362279507d9783f/)
22. Hickey MJ, et al. Molecular and cellular basis of microvascular perfusion
    deficits induced by *Clostridium perfringens* and *Clostridium septicum*.
    *PLoS Pathog* 2008.
    [Consensus](https://consensus.app/papers/details/756d28a908a15853a344938aca66a2ff/)
23. Suzaki A, et al. Pathogenic characterization of *Clostridium perfringens*
    strains isolated from patients with massive intravascular hemolysis. *Front
    Microbiol* 2021.
    [Consensus](https://consensus.app/papers/details/5d3bfb35dcdc5c49be87582314ef6e4b/)
24. Mathur A, et al. *Clostridium perfringens* virulence factors are
    nonredundant activators of the NLRP3 inflammasome. *EMBO Rep* 2023.
    [Consensus](https://consensus.app/papers/details/3b3c6852184859308f71143478104228/)
25. Ma M, et al. Synergistic effects of *Clostridium perfringens* enterotoxin and
    beta toxin in rabbit small intestinal loops. *Infect Immun* 2014;82(7):2958-70.
    PMID 24778117. [DOI](https://doi.org/10.1128/IAI.01848-14)
26. Grenda T, et al. *Clostridium perfringens* — opportunistic foodborne pathogen,
    its diversity and epidemiological significance. *Pathogens* 2023.
    [Consensus](https://consensus.app/papers/details/f45c34729a3a5bc288bcba193d6f0d63/)
27. Kharel K, et al. Preventing foodborne illness associated with *Clostridium
    perfringens*. *EDIS* 2024.
    [Consensus](https://consensus.app/papers/details/4ba70585d50c517fbe2449ed1547fe58/)
82. Seki M, et al. Size distribution and pathogenic potential of culturable
    airborne *Clostridium* spp. in a suburb of Toyama City, Japan. *Microbes
    Environ* 2025.
    [Consensus](https://consensus.app/papers/details/8a8db85686d35560a51f8a47ceff6458/)
84. Ali HM, et al. Molecular identification of different toxinogenic strains of
    *Clostridium perfringens* and histopathological observations of camels died of
    peracute enterotoxaemia. *Heliyon* 2024.
    [Consensus](https://consensus.app/papers/details/55278e3434035780abed839e4a95ec2b/)

**Preformed toxin: *B. cereus* and *S. aureus***

28. Carroll LM, Wiedmann M. Cereulide synthetase acquisition and loss events
    within the evolutionary history of group III *Bacillus cereus* facilitate the
    transition between emetic and diarrheal foodborne pathogens. *mBio*
    2020;11(4). PMID 32843545. [DOI](https://doi.org/10.1128/mBio.01263-20)
29. Rajković A, et al. Heat resistance of *Bacillus cereus* emetic toxin,
    cereulide. *Lett Appl Microbiol* 2008.
    [Consensus](https://consensus.app/papers/details/2815ed6a30f55290b1877c6703ae9712/)
30. Schreiber N, et al. Acute liver failure after ingestion of fried rice balls: a
    case series of *Bacillus cereus* food poisonings. *Toxins* 2021.
    [Consensus](https://consensus.app/papers/details/0bf79148da585f928d103d991223d966/)
31. Yang S, et al. Cereulide and emetic *Bacillus cereus*: characterizations,
    impacts and public precautions. *Foods* 2023.
    [Consensus](https://consensus.app/papers/details/3273745e005b5d169cf14ca7ecddb68a/)
32. De Jesus RD, et al. Conventional identification of *Bacillus cereus* sensu
    lato isolates from soil and milk powder samples. 2019.
    [Consensus](https://consensus.app/papers/details/0ca383c7345951f6a4d0cba11c5bcb05/)
33. Huillet E, et al. The CodY-dependent *clhAB2* operon is involved in cell
    shape, chaining and autolysis in *Bacillus cereus* ATCC 14579. *PLoS ONE* 2017.
    [Consensus](https://consensus.app/papers/details/b85a65d73d8a58298da9e55c21d3d7de/)
34. Argudín MÁ, et al. Food poisoning and *Staphylococcus aureus* enterotoxins.
    *Toxins* 2010.
    [Consensus](https://consensus.app/papers/details/1762d6e9e460557bae26f8aafa55e943/)
35. Rubio Cieza MY, et al. Staphylococcal enterotoxins: description and importance
    in food. *Pathogens* 2024.
    [Consensus](https://consensus.app/papers/details/efb0c0c632845d38bfad02d325302722/)
36. Bianchi D, et al. A foodborne outbreak caused by staphylococcal enterotoxins
    in cheese sandwiches in northern Italy. *Ital J Food Sci* 2024.
    [Consensus](https://consensus.app/papers/details/91e0cf3312405d6390581efbc091df2d/)

***Vibrio parahaemolyticus***

37. Sánchez-Rojas IC, et al. *Vibrio parahaemolyticus* in a changing world: a One
    Health perspective. *Pathogens* 2026.
    [Consensus](https://consensus.app/papers/details/3fd7c4ba48f35aaea9cd742f08234b60/)
38. Fearnley E, et al. *Vibrio parahaemolyticus* foodborne illness associated with
    oysters, Australia, 2021–2022. *Emerg Infect Dis* 2024.
    [Consensus](https://consensus.app/papers/details/e392ce8b18cd574199c5e4e25a32f194/)
39. Koutsoumanis K, et al. Public health aspects of *Vibrio* spp. related to the
    consumption of seafood in the EU. *EFSA J* 2024.
    [Consensus](https://consensus.app/papers/details/d81e56fbd72b5f18b17cebc67ff6674e/)
40. Hiyoshi H, et al. Contribution of *Vibrio parahaemolyticus* virulence factors
    to cytotoxicity, enterotoxicity, and lethality in mice. *Infect Immun* 2010.
    [Consensus](https://consensus.app/papers/details/3b0160fe3fc15d74aae0fcc319a181d4/)
41. Akyeh ML, et al. Virulence potential of nonclinical *Vibrio parahaemolyticus*
    isolates from Vietnam: evidence for functional T3SS2-mediated enterotoxicity.
    *Microbiol Immunol* 2025.
    [Consensus](https://consensus.app/papers/details/7d4fc43a702b5b0987d4037e5e3f5e43/)
42. Matsuda S, et al. Advances on *Vibrio parahaemolyticus* research in the
    postgenomic era. *Microbiol Immunol* 2019.
    [Consensus](https://consensus.app/papers/details/706bc49397665008bb62a014a9534e35/)

***Aeromonas*, *Plesiomonas*, ETBF**

43. Klemm EJ, et al. Genomic analysis of clinical *Aeromonas* isolates reveals
    genetic diversity but little evidence of genetic determinants for diarrhoeal
    disease. *Microb Genom* 2024;10(3). PMID 38451244.
    [DOI](https://doi.org/10.1099/mgen.0.001211)
44. Janda JM. *Aeromonas* and *Plesiomonas*. 2002.
    [Consensus](https://consensus.app/papers/details/07ecbccb28b95a6d849637f9c81d2a64/)
45. Kain KC, Kelly MT. Clinical features, epidemiology, and treatment of
    *Plesiomonas shigelloides* diarrhea. *J Clin Microbiol* 1989.
    [Consensus](https://consensus.app/papers/details/1d40c1d77e675cc6a899826675490d18/)
46. Romli NIA, et al. *Plesiomonas shigelloides* bacteremia: a scoping review.
    *Pathogens* 2026.
    [Consensus](https://consensus.app/papers/details/f389caac6d065434952028896f2b6db6/)
47. Koo SH, et al. Evaluation of the clinical sensitivity and specificity of the
    BD Max Enteric Bacterial Panel in the Singaporean population. *J Microbiol
    Methods* 2022;197:106478. PMID 35504368.
    [DOI](https://doi.org/10.1016/j.mimet.2022.106478)
48. Kalule JB, et al. Prevalence and antibiotic susceptibility patterns of enteric
    bacterial pathogens in human and non-human sources in an urban informal
    settlement in Cape Town. *BMC Microbiol* 2019;19:244. PMID 31694551.
    [DOI](https://doi.org/10.1186/s12866-019-1620-6)
49. White MT, et al. A pro-carcinogenic bacterial toxin binds claudin-4 to cleave
    E-cadherin. *Nature* 2026.
    [Consensus](https://consensus.app/papers/details/b2ded6e8ed5251249a574db6c02ca8dd/)
50. Valguarnera E, Wardenburg JB. Good gone bad: one toxin away from disease for
    *Bacteroides fragilis*. *J Mol Biol* 2019.
    [Consensus](https://consensus.app/papers/details/84248caf045c5c19a71a16990962632b/)
51. Scotto d'Abusco AS, et al. The alleles of the *bft* gene are distributed
    differently among enterotoxigenic *Bacteroides fragilis* strains from human
    sources. *J Clin Microbiol* 2000;38(2):607-12. PMID 10655354.
    [DOI](https://doi.org/10.1128/JCM.38.2.607-612.2000)
52. Nakano V, Avila-Campos MJ. Virulence markers and antimicrobial susceptibility
    of bacteria of the *Bacteroides fragilis* group isolated from stool of children
    with diarrhea in São Paulo. *Mem Inst Oswaldo Cruz* 2004;99(3):307-12.
    PMID 15273805. [DOI](https://doi.org/10.1590/s0074-02762004000300012)
85. Kim W-S, et al. *Bacteroides fragilis* toxin induces sequential proteolysis of
    E-cadherin and inflammatory response in a mouse intestinal epithelial cell
    line. *Microorganisms* 2025.
    [Consensus](https://consensus.app/papers/details/bf5f40f1d2af5dfd9d932f2634cb6786/)

***Yersinia***

53. Monack DM, et al. *Yersinia* signals macrophages to undergo apoptosis and YopJ
    is necessary for this cell death. *PNAS* 1997.
    [Consensus](https://consensus.app/papers/details/2076ee6347f35a82ad189bf269c0178e/)
54. Spinner JL, et al. Neutrophils are resistant to *Yersinia* YopJ/P-induced
    apoptosis and are protected from ROS-mediated cell death by the type III
    secretion system. *PLoS ONE* 2010.
    [Consensus](https://consensus.app/papers/details/f8cd0133327656018a0ed8145f4645c4/)
55. Mares CA, et al. Heightened virulence of *Yersinia* is associated with
    decreased function of the YopJ protein. *Infect Immun* 2021.
    [Consensus](https://consensus.app/papers/details/80f1678ef4b35124890d85d33c37a163/)
56. Kirjavainen V, et al. *Yersinia enterocolitica* serum resistance proteins YadA
    and Ail bind the complement regulator C4b-binding protein. *PLoS Pathog* 2008.
    [Consensus](https://consensus.app/papers/details/a787670cf014563baeeb797b10838c54/)
57. Pilz D, et al. Mechanism of YadA-mediated serum resistance of *Yersinia
    enterocolitica* serotype O3. *Infect Immun* 1992.
    [Consensus](https://consensus.app/papers/details/1c41065df9b75c94a81bf8e55b97e7ce/)
58. Schindler MKH, et al. *Yersinia enterocolitica* YadA mediates complement
    evasion by recruitment and inactivation of C3 products. *J Immunol* 2012.
    [Consensus](https://consensus.app/papers/details/02a87ae8c78d5b5d87092f3942041d23/)

***Campylobacter***

59. Cohen EJ, et al. *Campylobacter jejuni* motility integrates specialized cell
    shape, flagellar filament, and motor, to coordinate action of its opposed
    flagella. *PLoS Pathog* 2020.
    [Consensus](https://consensus.app/papers/details/eaa00d39ecbc5054aee9465b4f46d05c/)
60. Ribardo DA, et al. The *Campylobacter jejuni* flagellar V-ring discerns
    viscosity levels to alter swimming velocity, metabolic gene expression, and
    fitness. *PNAS* 2026.
    [Consensus](https://consensus.app/papers/details/8efee192a72e530f8c8061894c01b567/)
61. Waller AA, et al. Impact of flagellar filament length on *Campylobacter
    jejuni* for colonization and flagellar-dependent phenotypes. *J Bacteriol* 2025.
    [Consensus](https://consensus.app/papers/details/a863c75564a456d58f9070475900b212/)
62. Frirdich E, et al. Peptidoglycan-modifying enzyme Pgp1 is required for helical
    cell shape and pathogenicity traits in *Campylobacter jejuni*. *PLoS Pathog*
    2012.
    [Consensus](https://consensus.app/papers/details/a54db3f7fefb57fa8ff0115280ab1ca9/)
63. Frirdich E, et al. Peptidoglycan LD-carboxypeptidase Pgp2 influences
    *Campylobacter jejuni* helical cell shape and pathogenic properties. *J Biol
    Chem* 2014.
    [Consensus](https://consensus.app/papers/details/c063f118a5e357868a7e10b873e1c75a/)
64. Keo T, et al. *Campylobacter* capsule and lipooligosaccharide confer
    resistance to serum and cationic antimicrobials. *Virulence* 2011.
    [Consensus](https://consensus.app/papers/details/fbc7309ab44e5c04b61ae6dade86a9e5/)
65. Pequegnat B, et al. Phase-variable changes in the position of O-methyl
    phosphoramidate modifications on the polysaccharide capsule of *Campylobacter
    jejuni* modulate serum resistance. *J Bacteriol* 2017.
    [Consensus](https://consensus.app/papers/details/85e90e11d7855fa1809e7c5499c10d87/)
66. Myles M, et al. Effect of the polysaccharide capsule and its heptose on the
    resistance of *Campylobacter jejuni* to innate immune defenses.
    *MicrobiologyOpen* 2024.
    [Consensus](https://consensus.app/papers/details/9998c4da54515386b12773cde49ef06a/)

***Listeria* motility, antimicrobial peptides, *Salmonella* AMP resistance**

67. So Y-O, et al. Structural basis of flagellar motility regulation by the MogR
    repressor and the GmaR antirepressor in *Listeria monocytogenes*. *Nucleic
    Acids Res* 2022.
    [Consensus](https://consensus.app/papers/details/9acca45efd1b5bb8b271088f03032dca/)
68. Gründling A, et al. *Listeria monocytogenes* regulates flagellar motility gene
    expression through MogR, a transcriptional repressor required for virulence.
    *PNAS* 2004.
    [Consensus](https://consensus.app/papers/details/569ead2d0f425cdc843d901b808c1bc3/)
69. Kamp HD, Higgins DE. A protein thermometer controls temperature-dependent
    transcription of flagellar motility genes in *Listeria monocytogenes*. *PLoS
    Pathog* 2011.
    [Consensus](https://consensus.app/papers/details/58f8dbb773a35cc9b7bb6fe4978c8dda/)
70. Porter EM, et al. Broad-spectrum antimicrobial activity of human intestinal
    defensin 5. *Infect Immun* 1997.
    [Consensus](https://consensus.app/papers/details/05114bf3adf15eb6ab5eb745a23dca71/)
71. Salzman NH, et al. Protection against enteric salmonellosis in transgenic mice
    expressing a human intestinal defensin. *Nature* 2003.
    [Consensus](https://consensus.app/papers/details/86606c99053b5e139e9e1f0651187794/)
72. Ehmann D, et al. Paneth cell α-defensins HD-5 and HD-6 display differential
    degradation into active antimicrobial fragments. *PNAS* 2019.
    [Consensus](https://consensus.app/papers/details/41e0d824c78153348dcb7298d2ed26d0/)
73. Gunn JS. The *Salmonella* PmrAB regulon: lipopolysaccharide modifications,
    antimicrobial peptide resistance and more. *Trends Microbiol* 2008.
    [Consensus](https://consensus.app/papers/details/45d7de3df0465dffbb0b31dbdf2d255a/)
74. Shi Y, et al. PhoP-regulated *Salmonella* resistance to the antimicrobial
    peptides magainin 2 and polymyxin B. *Mol Microbiol* 2004.
    [Consensus](https://consensus.app/papers/details/c231628131335b3db02b396568ce85d8/)
75. Kumar R, et al. Antimicrobial peptide resistance in *Salmonella* AMR: the role
    of surface binding and lipopolysaccharide remodelling. *World J Microbiol
    Biotechnol* 2026.
    [Consensus](https://consensus.app/papers/details/84fde8892de15173b1c0c2b49ccff0d0/)

**Motility, chemotaxis, and IgA proteases**

76. Butler SM, Camilli A. Both chemotaxis and net motility greatly influence the
    infectivity of *Vibrio cholerae*. *PNAS* 2004.
    [Consensus](https://consensus.app/papers/details/d411a49e59cb5c45b04d14fa1c61a369/)
77. Zou M, et al. DegS protease regulates the motility, chemotaxis, and
    colonization of *Vibrio cholerae*. *Front Microbiol* 2023.
    [Consensus](https://consensus.app/papers/details/a69fbe87d3515dbbbf46c2443f562516/)
78. Stanton V, et al. The PAS domain of the polarly localized histidine kinase
    FlrB in *Vibrio cholerae* controls class III flagellar transcription and
    contributes to intestinal colonization. *mBio* 2025.
    [Consensus](https://consensus.app/papers/details/7e4e327fde095a7a898bc7192fbbb5ff/)
79. Majzoub ME, et al. Immunoglobulin A protease from *Sutterella wadsworthensis*
    modifies outcome of infection with *Campylobacter jejuni* and is associated
    with microbiome diversity. *Gut Microbes* 2026.
    [Consensus](https://consensus.app/papers/details/9b09f1d142625545b0ec9d28e862dd00/)
80. Li Y, et al. Identification of trypsin-degrading commensals in the large
    intestine. *Nature* 2022.
    [Consensus](https://consensus.app/papers/details/f74f5594a62156f6b3e063e282e4c4b0/)

---

## 10. Known gaps, flagged UNVERIFIED above

- **Global burden for *C. perfringens*, *S. aureus* and *B. cereus*** — none is
  modelled in GBD 2021's 13 aetiologies or WHO's 14 foodborne diarrhoeal hazards
  [1][4]. Their only quantitative home is national foodborne estimates [2].
- **Infectious dose for *C. perfringens*** — only "large numbers of vegetative
  cells" [13][27]. This is §1b Step 2's crowd floor and it needs a number.
- **In vivo generation time for *C. perfringens*** — all available figures are in
  food or broth [15][16].
- **Cell dimensions in µm** for *C. perfringens*, *Campylobacter*, *S. aureus*,
  *V. parahaemolyticus* and *Yersinia*. Found: *B. cereus* 4–5 × 1.0–1.5 µm [32]
  and *Clostridium* spore 2.1–3.3 µm from an aerosol size distribution [82],
  which is suggestive rather than definitive.
- **Age skew** as a distribution for every organism except where GBD resolves it
  (<5 y and ≥70 y) [1][7] and except *Listeria*'s hospitalisation/death fractions
  [3].
- **Two GBD 2021 under-5 fatal PAFs (7.7% and 7.4%)** could not be assigned to a
  named pathogen, because the retrieved full text had its italicised organism
  names stripped. They are deliberately left unassigned rather than guessed.
- **Infectious dose and in vivo doubling for *V. parahaemolyticus*** — still
  unsourced after two passes.
