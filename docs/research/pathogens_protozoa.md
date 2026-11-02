# Enteric protozoa and microsporidia — ranked shortlist for Immunology Tower Defense

Research pass, 2026-11-02. The fifth pathogen pass, and the first to open the
**eukaryotic kingdom**: four previous passes covered viruses, invasive bacteria,
extracellular bacteria and evasion, and the roster that came out of them
(`docs/PATHOGEN_ROSTER.md`) contains three bacteria and one virus and **not one
eukaryote**.

Every factual claim below was checked against PubMed (via the PubMed MCP tool)
or Consensus during this session; nothing is written from memory. Where a number
is commonly quoted but I could not pull a primary or review source for it in
this pass, it is marked **UNVERIFIED** rather than filled in — including several
sprite-relevant dimensions, which is annoying and is recorded honestly.
Attribution: substantially based on articles retrieved from **PubMed**; full
citations with DOI links at the bottom; inline markers are `[n]`.

Read against `docs/GAME_DESIGN.md` §1a (the three lateral bands), §1b (adhesion
/ plaque / breach), §1c (host-cell states, occupancy, debris), §4a (pathogen
classes), §4b (the stress signal and the contact stress-sense roll), §4c (the
tower roster), §5 (the knowledge ladder).

**Companion pass.** `docs/research/enteric_burden.md` was written in parallel on
the same day as a cross-kingdom *epidemiology* pass. The two were produced
independently and agree on the two conclusions that matter here —
***Cryptosporidium* is a top-rank burden organism** and ***Entamoeba histolytica*
is anatomically disqualified on the small-bowel map**, which that pass reaches
from the disease name (amebic colitis) while this one reaches it from the
colonisation site. It also independently flags *Entamoeba*'s burden as its
weakest row, which is why §2a here refuses to quote a single number. Prefer that
document for burden *ranking* across kingdoms and this one for protozoan
*mechanics*. **One caveat it raises that this pass did not:
*Cryptosporidium*'s infected intestinal segment is host-species dependent
(ileum/caecum/colon in neonatal mice, duodenum/jejunum in neonatal rats) and is
therefore partly UNVERIFIED for humans.** This document places *Cryptosporidium*
in the small-intestinal interface column on the strength of its being a leading
cause of small-bowel diarrhoeal disease in children [18][19]; the segment itself
should be treated as an open question, not as settled the way *Giardia*'s
duodenum/jejunum is.

**The one-line finding.** The eukaryotes do not bring *more* of anything the
board already has. They bring four things it has no vocabulary for at all: an
infected host cell that is **harder to kill than a healthy one**; a pathogen that
causes **no inflammation, no cell death and no debris** and whose damage is
delivered by **the player's own CTL tower**; a **ranged** infector that never
adheres; and an **antigenic switch that the player's antibody tower triggers**.

---

## 1. The shortlist, ranked

Six, ranked by *how much new decision-making each forces on the player* — not by
fame, prevalence or lethality. Two of the most prevalent organisms in the whole
kingdom are in the cut list (§5) precisely because prevalence is not a slot.

| # | Organism | The ONE mechanic nothing else on the board brings | Why it earns a slot |
|---|---|---|---|
| 1 | ***Cryptosporidium hominis* / *parvum*** | **The infected cell is harder to kill than its neighbours.** *Cryptosporidium*-infected enterocytes are *less* likely to be apoptotic than bystander cells in the same tissue, and infection confers resistance to chemically induced apoptosis **to the host cell but not to bystanders** [8]. It also lives **inside the cell but outside the cytoplasm**, in a vacuole whose wall is built from the enterocyte's own brush-border membrane and proteins (villin, ezrin) [5][6], sealed off from the apical cytoplasm by an electron-dense layer [4]. | This is a direct, sourced attack on §4c's entire adaptive payoff. The reward for buying cytotoxic cells is the *apoptotic* kill — clean death, ~30% debris, fast regrowth. Against the single leading protozoan cause of moderate-to-severe infant diarrhoea [19], that route is the one the parasite specifically defends. Three further mechanics compose on top: a **hard-coded generation clock** (exactly 3 asexual generations, then one obligate sexual generation, "independent of environmental stimuli", whole cycle in <3 days in one host) [10][11]; **the host cell is the sensor** (enterocyte-intrinsic NLRP6 inflammasome → IL-18 → ILC IFN-γ → back onto the enterocyte) [12][13]; and it **speeds up the villus elevator**, raising both proliferation and cell loss so the epithelium shifts young and immature [8]. |
| 2 | ***Giardia duodenalis*** | **No invasion, no cytolysis, no inflammation, no debris — and the damage is dealt by the player's own CTL tower.** It adheres to the epithelium and never enters the mucosa, and causes "little or no mucosal inflammation" [31][33]. The microvillous shortening, disaccharidase loss and raised crypt/villus ratio that *are* the disease are transferred into naive mice by **CD8⁺ T cells from infected donors, and not by CD4⁺ cells** [43][44][45]. | The roster's most valuable single find to date was A/E *E. coli* — "the enemy that profits from your response", which inverted the assumption that more immune response is better. *Giardia* is that inversion aimed one tier higher: at the **adaptive** half. It also attacks the player's two information systems directly — its secreted cysteine proteases degrade CXCL1/2/3, IL-8, CCL2 and CCL20 (the §7/§9 recruitment cytokine, at source) and strip tight junctions [38], and cleave **IgA, IgG and the Paneth α-defensin HD6 and β-defensin HD1** in vitro [39]. And antibody *against* it does not kill it: anti-VSP antibody is non-cytotoxic and instead **triggers the antigenic switch** [46]. |
| 3 | ***Entamoeba histolytica*** — **mechanically rank 1, anatomically disqualified; see §6** | **Trogocytosis: it kills by nibbling, it kills your towers, and it wears your cells' surface.** Amoebae ingest *bites* of living human cells, which raises intracellular calcium and kills the cell; the amoeba then detaches and **rejects the corpse** [55]. It requires acidified amoebic lysosomes, so it is a distinct process from phagocytosis [58]. Separately and much older: amoebae **killed neutrophils, PBMC, monocytes and monocyte-derived macrophages, "without loss of parasite viability"** [61]. And through trogocytosis it **acquires and displays human membrane proteins**, which protects it from lysis by human serum — only after nibbling *live* cells, not after phagocytosing dead ones [59]. | Three of the roster's five "still with no answer" gaps, in one organism: **killing the phagocyte outright** ("we have no 'tower dies from what it ate'"), **NET degradation** (*E. histolytica* induces NETs, which immobilise and kill it [62] — here is the reason to build the mechanic), and a fourth kind of kill the board does not have: a damage-over-time contact kill that leaves a corpse the attacker refuses to eat. **But the trophozoite colonises the large intestine** [64][66] — excystation happens in the small intestine, colonisation does not. This is *exactly* the shape of the *C. difficile* rejection, and it should get the same treatment. |
| 4 | ***Enterocytozoon bieneusi*** (+ ***Encephalitozoon intestinalis*** as its mirror) | **A ranged infector that never adheres, and the anti-*Cryptosporidium*: naked in the cytoplasm.** A 1.5 × 0.5 µm spore [67] fires a hollow **polar tube 50–500 µm long at ~300 µm/s**, "acting as a harpoon-like organelle", to inject its whole cytoplasm into a host cell [70][71]. Once in, *E. bieneusi* develops **in direct contact with the host cytoplasm — no parasitophorous vacuole and no sporophorous vesicle at all** [67]. Its sister *Encephalitozoon (Septata) intestinalis* does the opposite: a vacuole with a septate honeycomb network, and it **disseminates** into lamina propria macrophages, fibroblasts and endothelium [68]. | §1b's whole ingress model is proximity-gated: adhere, crowd, charge a plaque, rupture. A harpoon that reaches 50–500 µm — **20–300× a bacterium's body length** — from a spore that never adheres is the first thing in five passes that bypasses that model rather than exploiting it. Two sister species also give a sourced pair at opposite ends of the niche axis: naked-in-cytoplasm (fully exposed to cytosolic sensing) versus vacuole-wrapped and disseminating. Honest caveat: the ranged infection is also the one mechanic here that could *break* §1b rather than enrich it. |
| 5 | ***Cyclospora cayetanensis*** | **An enemy with no feedback loop: the only organism here that cannot re-seed itself from your board.** Oocysts are shed **unsporulated** and must mature **7–10 days in the environment** before they are infectious, so direct person-to-person transmission is unlikely [76][80]. | Every pressure source in the game is a loop: adherents replicate on the wall, a brood bursts from a dying cell, a breach makes the wall charge faster. *Cyclospora*'s pressure is **purely exogenous and purely scripted** — nothing that happens on the board can increase next round's arrivals. That is the exact mirror of *Cryptosporidium*, whose entire cycle including sex completes in one host in <3 days [11]. The pair teaches autoinfection by contrast, the way CMV/Ad41 teaches MHC-I by contrast. Also the most vivid callout in the kingdom: **imported raspberries, fresh basil and cilantro** [76][81]. |
| 6 | ***Cystoisospora belli*** | **It only arrives once you are already losing.** Found worldwide but "almost always in individuals with an immuno-compromised status" [82]; on histopathology it may be seen **invading beyond the epithelium**, with eosinophils [82]; it disseminates to the biliary tree in AIDS [83]; and relapse and chronic infection occur **even after immune reconstitution** [82]. | The honest answer is that this is a **round modifier, not a species** — see §4. Keeping it on the list is worth one line because it is the only candidate whose defining property is a *state of the player*, and because "relapses after immune reconstitution" is a sourced argument for a pathogen that survives the player fixing the thing that let it in. |

**Cut, and why:** *Blastocystis* spp. (colonises the **large** intestine [85]; the
most prevalent gut protist in humans [84] and the clinical significance is
genuinely unresolved [86][87]); *Dientamoeba fragilis* (**32.5% of 36,008
children PCR-positive** with no clinical association detectable [88][89] — the
single clearest demonstration in this pass that prevalence is not a slot);
*Entamoeba dispar* (non-pathogenic commensal, exists only as a diagnostic
confound [64]); *Encephalitozoon cuniculi* (disseminating and latent, not an
enteric primary [75]). Full reasoning in §5.

---

## 2. Numbers

### 2a. Burden

| Organism | Global / endemic burden | High-income setting | Age skew | Source |
|---|---|---|---|---|
| ***Cryptosporidium*** | One of the **four** pathogens accounting for most attributable moderate-to-severe diarrhoea (MSD) in GEMS, alongside rotavirus, ST-ETEC and *Shigella* [19]. Cryptosporidium-attributable MSD incidence **3.48 / 100 child-years (Africa) and 3.18 (Asia) in infants**, 1.41 / 1.36 in toddlers; **~2.9 M + 4.7 M attributable cases/yr** in sub-Saharan Africa and South Asia under 24 months; **~202,000 attributable deaths/yr** and ~59,000 *excess* deaths over Cryptosporidium-negative cases [18]. In toddlers 12–23 months it was one of only three pathogens associated with increased risk of case death (HR 2.3) [19]. | **444 US outbreaks / 7,465 cases 2009–2017**; treated recreational water = 35.1% of outbreaks and **56.7% of cases**; cattle contact 14.6%; childcare 12.8%; outbreak count rising ~13%/yr [21]. 2015–2019: *Cryptosporidium* caused **49% of US treated-recreational-water outbreaks with a confirmed etiology and 84% of the cases** [22]. **92% of all global waterborne *Cryptosporidium* outbreaks 2017–2022 were recreational water / swimming pools** [23]. | **Infants.** Significantly associated with MSD and less-severe diarrhoea only **below 24 months** [18]. Also strongly linked to stunting, wasting and being underweight at 60-day follow-up (aOR 1.65 / 2.04 / 2.08) [20]. | [18][19][20][21][22][23] |
| ***Giardia duodenalis*** | **280 million** people annually with acute watery diarrhoea / giardiasis [50]; a second review puts it third behind rotavirus and *Cryptosporidium* at "over 300 million reported cases per annum" in under-fives per WHO figures [51]. **~28.2 M cases/yr attributable to food** specifically [52]. Africa-wide pooled human prevalence **8.8% by microscopy, 14.3% copro-antigen, 19.5% molecular** [96]. | **2–3% prevalence in industrialised countries, up to 30% in low-income countries** [51]. **17.1% (71/416)** of global waterborne protozoan outbreaks 2017–2022; a quarter of those were recreational water [23]. | Asymptomatic infection is "equally common" as symptomatic [50]; no sharp age skew sourced in this pass — **UNVERIFIED**. | [50][51][52][96][23] |
| ***Entamoeba histolytica*** | **Three mutually inconsistent figures were found and none can be preferred on this pass**: "infects 10% of the world's population… 100,000 deaths/year" [63]; "affects approximately 90 million people each year" [65]; "34–50 million symptomatic infections annually" [66]. Third leading cause of human death among parasite infections [62]. | Seen in immigrant populations and returning travellers; no incidence figure sourced — **UNVERIFIED** [66]. | **UNVERIFIED**. | [62][63][65][66] |
| ***Enterocytozoon bieneusi*** | First global meta-analysis: **6.59% (2,291 / 31,644)** across 75 studies in endemic regions. **16.45% in patients with diarrhoea**, **11.49% in HIV-infected**, 6.49% asymptomatic. Rural 7.58% > urban. **69 genotypes in humans** [72]; >500 genotypes described overall [73]. Independent Chinese series: 5.6% in diarrhoea vs 1.9% asymptomatic [72 ref. ctx]. | Not a developing-world-only organism: the **highest prevalence by climate was the temperate marine zone (13.55%)**, and the review explicitly flags Europeans in temperate marine climates [72]. | **Teenagers 10.97%**, the highest age band [72]. | [72][73] |
| ***Cyclospora cayetanensis*** | Independently the cause of only **0.72% (3/416)** of global waterborne protozoan outbreaks 2017–2022 [23] — it is a **produce** pathogen, not a water one. Humans are the only known host [77]. | "An important cause of foodborne outbreaks of enteric disease in **many developed countries**, mostly associated with consumption of contaminated fresh produce" [76]. | In endemic countries: children, foreigners, immunocompromised. **In industrialised countries it affects people of any age** [76]. | [23][76][77] |
| ***Cystoisospora belli*** | No population figure sourced — **UNVERIFIED**. Worldwide, infects **only humans** [82]. | Rare; case-report literature only [83]. | **UNVERIFIED**; the skew is by immune status, not age [82]. | [82][83] |
| ***Blastocystis*** spp. (cut) | Pooled prevalence from 137 survey units / 47 countries / **562,327 participants: 17.9% (95% CI 13.9–22.6)**, crude participant-level positivity 4.7%, I² = 99.7%. Regional: **Africa 44.2%, Americas 36.7%, Asia 8.4%, Europe 7.2%**. ST3, ST1, ST2 predominant [84]. >1 billion people colonised [87]. | Europe 7.2% [84]; 19.2% of 27,918 adult stool PCRs in one Israeli cohort [89]. | — | [84][87][89] |
| ***Dientamoeba fragilis*** (cut) | **32.5% of 36,008 children** and **22.3% of 27,918 adults** PCR-positive in two Israeli cohorts — and in both, positives had **no higher odds of symptoms, referrals, treatment or antibiotics** than all-negative patients [88][89]. | As above — these *are* high-income data. | Children 32.5% > adults 22.3% [88][89]. | [88][89] |

### 2b. Infectious dose, timing, morphology

| Organism | Infectious dose | Timing | Size (and vs a 1–3 µm bacterium) |
|---|---|---|---|
| ***Cryptosporidium*** | **ID50 132 oocysts** (Iowa isolate, 29 seronegative healthy adults) [27]. **Isolate-dependent over two logs**: maximum-likelihood ID50s of **12.1 (TAMU), 132 (IOWA), 2,066 (UCP)** [26]. **Pre-existing anti-*C. parvum* serum IgG raises ID50 to 1,880 — >20-fold** [28]. | Incubation mean 9 d / median 6.5 d, and **dose did not substantially change incubation length**, though higher doses infected sooner and lasted longer [27]. Whole life cycle **<3 days in a single host**; **3 asexual generations then 1 sexual generation**, intrinsically programmed [11]. | Oocyst **4–6 µm**, nearly spherical, obscure internal structure [29]; 4–4.5 µm with **endogenous stages 2–6 µm** [6]. **Four naked sporozoites per oocyst** [78]. → oocyst ≈ **2–4×** a bacterium; the intracellular stage is **bacterium-sized**. |
| ***Giardia*** | **UNVERIFIED for humans in this pass.** Rendtorff's 1954 human capsule challenge is the source of the classic low-dose figure but the abstract was not retrievable [53]. Measured in beavers by FACS-sorted viable cysts: **between <50 and <500 viable cysts** required [54]. | Acute infection develops over ~3 weeks, **peaking at day 8**; healthy hosts clear in 2–3 weeks [51]. **VSP switching every 6.5 to 12–13 generations**, spontaneous, no environmental trigger needed [47]. | Trophozoite **15.0 ± 1.2 µm long × 7.5 ± 0.8 µm wide** (n = 50) [37]; inverted-pear / teardrop, concave ventral surface, **two nuclei, 8 flagella in 4 pairs**, a median body [34][37]. Cyst dimensions **UNVERIFIED** (oval, smaller than the trophozoite, immediately infectious on excretion [33]). → trophozoite ≈ **5–15×** a bacterium's length, and the single largest sprite in the lumen band. |
| ***Entamoeba histolytica*** | **UNVERIFIED.** | **UNVERIFIED.** | Trophozoite **10–60 µm**, motile; mature cyst **~20 µm with 4 nuclei**, chitin wall, detergent-resistant [64][65]. → trophozoite **up to 20–60×** a bacterium. The biggest thing in the kingdom by a wide margin. |
| ***Enterocytozoon bieneusi*** | **UNVERIFIED.** | **UNVERIFIED.** | Mature spore **1.5 × 0.5 µm**, spore wall "very thin", endospore absent or poorly differentiated; **polar tube with 5–6 coils** inside the spore, differentiated *before* the sporogonial plasmodium divides [67]. Extruded polar tube **50–500 µm long, fired at ~300 µm/s** [70]. → the spore is **smaller than a bacterium**; the harpoon is **20–300×** a bacterium's length. |
| ***Cyclospora cayetanensis*** | **UNVERIFIED.** | **Sporulation 5–13 days** at 25–32 °C in potassium dichromate [78]; 7–10 days quoted for the environment [80]. Endogenous stages <10 µm in histological section [77]. | Oocyst size **UNVERIFIED** — described contemporaneously as "a large *Cryptosporidium*", with *Eimeria acervulina* the closest size surrogate [77][79]. Internal structure is the distinguishing feature: **two sporocysts, each with two sporozoites**, versus *Cryptosporidium*'s four naked sporozoites [78]. |
| ***Cystoisospora belli*** | **UNVERIFIED.** | **UNVERIFIED.** | "Characteristic **elliptical** oocysts" on modified acid-fast stain [82]; exact dimensions **UNVERIFIED**. Asexual (schizont) and sexual (gamont) stages in biliary-intestinal epithelium [82]. |

### 2c. The cyst/oocyst stage — the environmental-persistence numbers

The brief asked whether the dormant, chlorine-resistant, environmentally
transmitted stage is a mechanic a bacterium or virus cannot express. The
numbers say yes, and they say the resistance is **species-specific by two orders
of magnitude**, which makes it a tuning knob rather than a flat trait.

- ***Cryptosporidium* is the chlorine-resistance champion, with a measured
  number.** Ct values for a **3-log reduction in oocyst viability at pH 7.5 were
  10,400 (Iowa isolate) and 15,300 (Maine isolate)** mg·min/L free chlorine —
  *higher* than the CDC's then-recommended Ct = 9,600 for remediating a pool
  after a diarrhoeal accident [24]. This is the mechanistic reason 92% of global
  waterborne *Cryptosporidium* outbreaks are recreational water [23]; the UK
  water industry treats it as the main waterborne public-health threat
  specifically because of "its resistance to chlorine disinfection and ability
  to cause large-scale outbreaks" [30].
- **Ordering, not just a label.** Ozone inactivation efficiency decreases in the
  order **viruses ≫ *Giardia* cysts > *C. parvum* oocysts** [25]. So *Giardia*
  cysts are tough but *Cryptosporidium* oocysts are tougher, and the oocyst's
  advantage over a virion is large.
- ***Cryptosporidium*'s resistance also covers drugs, for a structural reason.**
  "The intracellular but extracytoplasmic domain the parasite occupies is in
  itself a significant barrier to drug entry" — any agent must cross the
  multiple apical membranes that camouflage it, or come through the host cell
  [3]. That is the same geometry argument the game would use for why cytosolic
  effectors miss it, and a 2025 review states the niche plainly as "intracellular
  but extracytoplasmic within the parasitophorous vacuole", which "facilitates
  immune evasion and complicates host-pathogen interactions" [94].
- ***Cyclospora*'s oocyst is the opposite kind of tough**: not disinfectant-proof
  but *time-locked*. It is shed non-infectious and matures over 7–10 days [80],
  and even UV-treated oocysts on raspberries still produced shedding (though
  asymptomatic) birds, while high-hydrostatic-pressure treatment abolished both
  [81].

---

## 3. Per-candidate detail

### 3.1 *Cryptosporidium hominis* / *C. parvum* — rank 1

**1. The one mechanic, in §1b/§4a/§4b vocabulary.**
It **invades** and **takes a host-cell slot** (host layer `Healthy → Infected`),
sits in the interface column of the tissue band at the brush border — "a unique
intracellular niche at the brush border of intestinal epithelial cells" [7] — and
**does not kill the cell it occupies** on its own schedule. What is new is three
properties the board has no word for:

- **The infected cell resists the player's best kill.** Infected cells are *less*
  apoptotic than bystanders in the same tissue, and infection confers resistance
  to chemically induced apoptosis **specifically to the infected cell and not to
  its neighbours** [8]; in culture, infection also *attenuates* apoptosis
  induced by strongly proapoptotic agents, while itself inducing only moderate
  apoptosis peaking at 24 h [9]. Mechanically: a per-species multiplier on the
  §4c apoptotic route — the CTL/NK/γδ recognition meter fills, and the kill
  resolves slowly or falls back to the necrotic path.
- **A hard-coded generation clock, immune to player interference.** Exactly
  **three asexual generations, then one obligate sexual generation**, and the
  authors state it is "an intrinsic program… independent of environmental
  stimuli"; the progeny of each meront is collectively committed to asexual or
  sexual fate [11]. Fertilisation does not occur in culture and the infection
  falters there; in mice it does, and leads to meiosis and sporulation [10]. In
  game terms: a visible generation counter on each focus that the player can
  only beat by clearing before it reaches 4 — not slow down.
- **The host cell is the sensor, and the answer is already on the tower list.**
  Innate control runs through an **enterocyte-intrinsic inflammasome**: caspase-1
  deletion *solely in enterocytes* phenocopies whole-body knockout, it works in
  germ-free mice, and **NLRP6 was the only NLR required**; the output is IL-18,
  and recombinant IL-18 rescues parasite control in caspase-1 KO mice [12]. That
  IL-18 synergises with IL-12 to drive **ILC** IFN-γ, which acts back on the
  **enterocyte** via STAT1 — and loss of STAT1 in enterocytes, but *not* in
  dendritic cells or macrophages, antagonised control [13].

**2. Morphology for a sprite.**
Oocyst **4–6 µm, nearly spherical, with deliberately obscure internal
structure** — the review notes there are "few or no morphometric features" to
tell species apart [29]. The distinguishing silhouette is therefore **four naked
sporozoites inside a near-perfect circle** [78] — four comma/banana shapes in a
disc, no inner sporocyst walls (that absence is exactly what separates it from
*Cyclospora*). The intracellular stage is **2–6 µm** [6] and should be drawn as a
small bulge **sitting on top of** the brush border rather than buried in the
cell: the parasitophorous vacuole "stays in the brush border area and disturbs
the distribution of microvilli", and its wall carries the host's own villin and
ezrin [5]. Secreted effectors from four distinct organelle types (micronemes,
rhoptries, dense granules and an original secretory organelle) assemble at the
interface into structures "including a **ring** that anchors the parasite into
its unique epicellular niche" [2][1] — so if the sprite needs one more cue, a
dark anchoring ring at the base of the blister is the sourced one. **Size vs a bacterium: oocyst ~2–4×, intracellular stage ~1–3× —
i.e. the one eukaryote that reads at bacterial scale.** At 20 px the only
readable cue is *four dots in a circle*; use that, and reserve a distinct
"blister on the wall" silhouette for the intracellular stage.

**3. Exposure route / vehicle.**
A **swimming pool or water playground** — specifically one that somebody with
diarrhoea got into, because chlorine does not fix it (§2c). 35.1% of US
outbreaks and 56.7% of cases came from treated recreational water, and the
outbreak count was climbing 13%/yr [21]; 49% of 2015–2019 US treated-recreational
outbreaks with a confirmed cause, and 84% of the cases [22]. The two other vivid
routes: **touching a calf** (14.6% of outbreaks [21]) and **a daycare room**
(12.8% [21]). Boiling drinking water and using rainwater were protective in
Kenya [20]. Note for the callout writer: 77.8% of GEMS *Cryptosporidium*-positive
MSD cases were ***C. hominis***, and **92% of the *C. parvum* isolates tested
were anthroponotic genotypes** [18] — so "you got it from a cow" is the
*minority* story even though the cow route is real.

**4. Burden.** See §2a. The headline: one of the four pathogens responsible for
most attributable MSD globally [19], ~202,000 attributable deaths/yr under 24
months [18], and the only protozoan on the GEMS case-fatality list (HR 2.3 in
12–23-month-olds) [19]. Strongly infant-skewed — significant only below 24
months [18] — and it leaves stunting behind [20].

**5. The player's counter.**
Unusually, **the roster already has the right answer and does not know it.** The
NK cell (ILC1) tower was built as an *amplifier* — an IFN-γ aura that upgrades
nearby macrophages rather than a killer (§4c). The *Cryptosporidium* literature
is the strongest validation of that decision found in any pass: innate control
is **IL-18 from the infected enterocyte → ILC IFN-γ → STAT1 back on the
enterocyte** [12][13], i.e. the ILC's output acts on *host cells*, not on the
parasite. Resolution requires **CD4⁺ T cells** [14][17] — in mice, anti-CD4
permitted chronic infection, **anti-CD8 alone did not permit infection at all**,
and anti-CD4 + anti-IFN-γ was the worst combination [15]. But human CD8⁺ T cells
*can* do it: antigen-expanded sensitised human CD8⁺ T cells **reduced parasite
load in and lysed infected intestinal epithelial cells**, apparently via
cytotoxic granules, requiring presentation through both HLA-A and HLA-B [16].
So: the CTL tower works but is not the critical path; the ILC1 is; and the
apoptosis-resistance trait is what makes the CTL's *clean* kill unreliable
here. Also sourced and worth a shop line: **prior infection protects far better
than vaccination** — pre-existing serum IgG raised the ID50 >20-fold [28], and
live priming restored mucosal Th1 effectors (CD3⁺CD8⁺CD103⁺) even under ongoing
protein malnutrition where vaccination failed [91].

**6. Redundancy check.**
Closest built organism: **Rotavirus** — also intracellular, also epithelial,
also interacts with villus geometry. Not redundant, and the contrast is the
point. Rotavirus's geometry is *static tropism* (tip only, never the Lgr5⁺ crypt
stem cell; the crypt repaves from below). *Cryptosporidium*'s is *kinetic*: it
raises **both** proliferation and cell loss, "speeding up the villus elevator",
and the single-cell result is a **gain of young and loss of mature cells** [8].
One enemy makes the board heal from one end; the other makes the board churn.
And no built or shortlisted organism has an infected cell that is harder to kill
than a healthy one.

### 3.2 *Giardia duodenalis* — rank 2

**1. The one mechanic.**
It **adheres and never invades** — "resides in the lumen and attaches to the
epithelium and overlying mucus layers but **does not invade the mucosa** and
causes **little or no mucosal inflammation**" [31]; the human mucosa in
giardiasis "is devoid of signs of overt intestinal inflammation", reproduced in
animal models [32]; and a 2022 *Trends in Parasitology* primer states flatly
that "in contrast to most other intestinal infections, it causes low level or no
inflammation" [33]. It **takes no host-cell slot**, lives **only in the lumen
band and at the gut interface**, never enters the tissue band, **replicates
extracellularly by binary fission** on the wall [33], and **produces no debris**.
In §4b terms it emits **no stress signal**, because there is no infected cell to
emit one.

Then the damage arrives from the player's own side of the board. The lesion that
*is* giardiasis — diffuse microvillous shortening, lost brush-border surface
area, impaired sucrase, raised crypt/villus ratio — was reproduced by adoptive
transfer of **enriched CD8⁺ T cells and whole lymphocytes from infected donors,
and not by CD4⁺ T cells** [43]; both subsets drove IEL influx, but only CD8
caused the injury [43][44]. The pathophysiology review makes the chain explicit:
barrier dysfunction → CD8 activation → microvillous shortening → malabsorption
[45].

Three further composable effects, all sourced:

- **It eats the recruitment signal at source.** Secreted cysteine proteases
  CP14019 / CP16160 / CP16779 degrade **CXCL1, CXCL2, CXCL3, IL-8, CCL2 and
  CCL20** — the chemokines the epithelium upregulates during the interaction —
  and cleave or re-localise claudin-1/-4, occludin, JAM-1, β-catenin and
  E-cadherin [38]. A fourth protease, **giardipain-1** (cathepsin-B-like,
  25 kDa, on the surface and flagella), independently degrades occludin and
  claudin-1, drops TER, and activates caspase-3 [40].
- **It eats the ~70% rung.** An *in silico* screen of the human intestinal
  proteome followed by *in vitro* cleavage found **IgA and IgG, and the defensins
  α-HD6 and β-HD1**, are substrates of the same proteases [39].
- **The grip gets stronger the harder you pull.** Single-cell force spectroscopy
  on live trophozoites gave **7.7 ± 4.2 nN at 1 µm/s and 28.7 ± 10.5 nN at
  10 µm/s**, with a retract-force curve unlike any other eukaryote measured —
  consistent with a **ligand-independent suction** mechanism rather than
  adhesion molecules [36].

**And the antigenic switch is triggered by the player's antibody.** Antibodies
to VSPs are **not cytotoxic**; instead they cluster VSPs into liquid-ordered
membrane microdomains, trigger a massive release of microvesicles carrying the
original VSP, and **switch expression to a different VSP by a calcium-dependent
mechanism**, both *in vitro* and in animal infections [46]. Baseline switching is
spontaneous at **every 6.5 to 12–13 generations** with no environmental trigger,
from a repertoire of **~270 *vsp* genes** in the WB isolate (≥150 estimated
earlier), of which exactly one is expressed at a time, by an epigenetic
RNAi-based mechanism with no gene movement [47][48][49].

**2. Morphology for a sprite.**
**Inverted-pear / teardrop, 15.0 ± 1.2 µm long × 7.5 ± 0.8 µm wide** (n = 50
measured) [37]. Rounded anterior, convex dorsal surface, **concave ventral
surface whose anterior two-thirds is the adhesive disc, divided into two lobes
with a median groove** [34]. **Two nuclei**, one dorsal to each disc lobe
[34][37]. **Eight flagella in four pairs**, arising from basal bodies between the
anterior poles of the nuclei, with a long caudal pair trailing [34][37]. A
**median body** — a unique oval aggregate of curved microtubules between the
posterior nuclear poles [34]. The disc itself is a **spiral layer of microtubules
and microribbons** with cross-bridges, and is non-uniform, with a distinct margin
and ventral groove [35].

For a 64×64 procedural sprite at ~20 px, the readable set is: **one teardrop
capsule, two dots (nuclei), one concave arc on the ventral side (the disc), two
trailing lines (caudal flagella)**. Drop the other six flagella at display size
and keep them for the inspector art.

**Size vs a bacterium: 5–15× in length, and the game's largest lumen sprite.**
This is itself a gameplay fact. A *Giardia* trophozoite is comparable in size to
one enterocyte, so a knot of them physically occludes the mucus band in a way a
knot of bacteria does not — and §1b's crowding-as-concentration measure over "a
cell and a half of wall" will behave very differently when the organisms are
each a third of a cell wide.

**3. Exposure route / vehicle.**
**Drinking untreated water from a backcountry stream** — the classic, and the
reason the organism's folk name is a beaver's. Verified route data: 17.1% of 416
global waterborne protozoan outbreaks 2017–2022, with 25.3% of those recreational
water [23]; **cysts are immediately infectious the moment they are shed** [33],
so no environmental maturation delay is needed. Second route, less famous and
better sourced than people expect: **food**, at ~28.2 M cases/yr [52]. Third:
**a puppy or a calf** — it is a zoonosis with the broadest host range in the
genus [51], pooled 13.6% prevalence across 4.9 M non-human mammals, highest in
rodents (28%) and artiodactyls (17%) [95].

**4. Burden.** See §2a. **280 million symptomatic infections annually** [50],
third globally behind rotavirus and *Cryptosporidium* in under-fives per WHO
figures [51], and a **sharp high-income split: 2–3% versus up to 30%** [51].
Asymptomatic infection is as common as symptomatic [50], and — the detail most
worth a shop blurb — recent data suggest *Giardia* infection can be
**protective against other diarrhoeal diseases** [50][32]. The cost that is not
diarrhoea: chronic infection causes a **malabsorption syndrome leading to failure
to thrive and stunted growth**, especially in children in developing countries
[92] — so its real-world damage model is attrition, not a spike, which is exactly
the right shape for a wave enemy that never kills a cell.

**5. The player's counter.**
**The honest answer is that most of the roster cannot touch it, and one tower
makes it worse.**

- **CTL (~10%): actively harmful.** Buying it is what produces the lesion [43].
- **NK (ILC1), γδ IEL, macrophage, neutrophil: no target.** All three cytotoxic
  routes in §4c act on an *infected cell*. There isn't one.
- **γδ IEL is the exception worth building around.** Its two shipped effects are
  interception of forming plaques and **repair of dead barrier segments** — and
  a brush border shortened by the player's own CD8 cells is precisely a
  barrier-repair problem rather than a killing problem. This is the first enemy
  for which the repair tower is the *primary* answer.
- **Plasma cell / sIgA (~20% / ~70%): the designed answer, and it backfires
  twice.** Secretory IgA has "a central role in anti-giardial defence" [31] — but
  the parasite's proteases cleave IgA [39], and anti-VSP antibody **triggers the
  antigenic switch** [46], which is §5's knowledge discount fired by the
  player's own tower.
- **B-cell-independent mechanisms exist** and are the honest gap in the real
  biology too: candidate effectors named are **nitric oxide, Paneth-cell
  α-defensins and lactoferrin**, with their relative importance "poorly
  understood" [31]. Note the parasite cleaves α-HD6 and β-HD1 as well [39].

**Flag for the Director: this organism is the strongest argument yet for a
non-cytotoxic, barrier-and-mucus-focused answer that the shop does not have.**

**6. Redundancy check.**
Closest built organism: ***Vibrio cholerae*** — "never enters a cell, never takes
a slot, never kills. It adheres and secretes." Genuinely close, and the
difference has to be stated precisely or this is a cut.

*Vibrio* subverts **living host cells** into not doing the player's work, with
three visible, decaying effects (claudin-2 up, tricellulin down, plaque charges
faster, lactate feedback loop). Its lesson is "look for cells that are alive and
no longer yours."

*Giardia* shares the no-invasion/no-kill/no-slot profile and nothing else. Its
three distinct dimensions:
1. **The damage comes from the player's towers, not from the pathogen.** *Vibrio*
   does its own damage. *Giardia*'s lesion is CD8-mediated [43].
2. **It attacks the information systems.** It degrades the §7/§9 recruitment
   chemokines at source [38] and the §5 ~70% rung's molecule [39]. *Vibrio* does
   neither.
3. **Antibody triggers its switch instead of killing it** [46]. Nothing built or
   shortlisted does this — norovirus GII.4 re-antigenises on a *calendar*, which
   is a scheduled event; *Giardia* re-antigenises **in response to the player
   investing in antibody**, which is a feedback loop.

Keep. Build it *after* the knowledge ladder's ~20% rung is live, or the third
mechanic has nothing to push against.

**Correction to its own entry, stated honestly: "no cell death" is not
unqualified.** Two things cut against the clean profile and must go into any
build:
- **Apoptosis is strain-dependent.** *G. lamblia* strains **NF and S2 induced
  caspase-3-dependent enterocyte apoptosis in human duodenal monolayers; strains
  WB and PB did not**, and a caspase-3 inhibitor blocked both the apoptosis and
  the ZO-1 disruption and permeability rise [41]. Assemblage-specific too, and
  **mixed infections enhance it** [42].
- **Chronic infection does produce damage.** Chronically infected hosts show
  villus and crypt atrophy, enterocyte apoptosis and severe barrier disruption
  [51], and villus blunting and leaky gut are listed as chronic outcomes [33].

So the game-honest statement is: ***Giardia* has no cell death and no
inflammation on the round timescale, and a strain-selectable apoptosis trait.**
That is better than a flat trait — it is the knob that distinguishes a
"commensal-ish" wave enemy from a late-game variant, and it is sourced.

### 3.3 *Entamoeba histolytica* — rank 3 on mechanics, disqualified on anatomy

**1. The one mechanic.**
**Trogocytosis.** It **adheres** to a host cell, then "bites off and ingests
distinct pieces of living human cells, resulting in intracellular calcium
elevation and eventual cell death"; it **requires ingestion of fragments** for
killing; and then — the detail that makes it a mechanic rather than a flavour —
**"after cell killing, amoebae detach and cease ingestion"** and reject the
corpse [55] — "a stark contrast to the established model of dead cell clearance
in multicellular organisms" [55], and reviewed as the likely principal driver of
tissue destruction in amoebiasis [56][57]. It is mechanistically separable from phagocytosis: blocking
amoebic lysosomal acidification with ammonium chloride or concanamycin A
**blocks continued trogocytosis and killing while leaving the first bite
intact**, and phagocytosis-defective mutants still trogocytose normally
[58][59].

In §1b/§4a terms it is the **Parasite** class done properly: large bacterium-like
in that it **does not hide and takes a slot outright**, but with a kill that is
*incremental and contact-sustained* rather than instantaneous, and which
**produces a corpse the attacker will not clear**. Trogocytosis also contributes
to **tissue invasion** [55], so it is simultaneously the kill and the breach.

Two more, both of which hit the tower roster rather than the wall:

- **It kills the player's towers.** "Amoebae killed neutrophils, PBMC, monocytes
  and monocyte-derived macrophages (P < 0.001), **without loss of parasite
  viability**", and adding immune serum neither helped the leukocytes kill nor
  protected them [61]. The reverse also holds and is the only answer found:
  **lymphokine-activated** macrophages killed 55% of amoebae by 3 h by a
  contact-dependent, antibody-independent, partly oxidative mechanism — but
  **40% of those activated macrophages died in the same window** [61]. That is a
  sourced, numerically specified mutual-destruction trade, which is a far better
  mechanic than a damage stat.
- **It wears your cells' surface.** Through trogocytosis it **acquires and
  displays human cell membrane proteins**, and this protects it from lysis by
  human serum — **only after nibbling live cells, never after phagocytosing dead
  ones** [59]. Design note from the same group's review: trogocytosis is not only
  the amoeba's weapon — immune cells use it to kill parasites, and some
  intracellular bacteria appear to **exploit host-cell trogocytosis to spread from
  cell to cell** [60], which is a second, independent candidate mechanism for the
  actin-tail-style spread the roster already wants for *Shigella* and *Listeria*.
  In game terms: an enemy whose resistance to the complement body
  upgrade is *earned by killing your tissue*, and which can be denied by
  stopping the nibbling rather than by out-damaging it.

It also dissolves the mucus layer first — cysteine protease cleavage of the
**MUC2 C-terminus** — and only then binds and kills epithelium [63], which is
§6b's mucus upgrade being specifically defeated.

**2. Morphology for a sprite.**
Trophozoite **10–60 µm, motile, amoeboid**, with pseudopods and (classically)
ingested erythrocytes [55][64]. Cyst **~20 µm, round, chitin cell wall, four
nuclei, detergent-resistant** [64][65] — the four nuclei and round chitin wall
are the four verified "hallmarks" [65]. For a silhouette: **one irregular blob
with one or two extended pseudopods and visible dark inclusions**; for the cyst,
**a circle with four dots** (and note the collision with *Cryptosporidium*'s four
sporozoites — differentiate by size, 20 µm vs 4–6 µm, and by the inclusions
being round nuclei rather than comma-shaped sporozoites).

**Size vs a bacterium: 10–60× at the trophozoite stage**, up to the largest
single sprite the game would ever draw in this kingdom. At 20 px this is the one
organism that should visibly *not fit* in a lane.

**3. Exposure route / vehicle.**
Ingesting cysts in food, water or on plants; **person-to-person contact**;
exposure in an endemic area; and **swimming in contaminated water** [64]. Cysts
survive weeks in the environment and resist disinfectants, and wastewater
treatment is reported to be poor at removing them (stabilisation ponds beat
activated sludge) [64]. The most vivid real callout: it is the parasite of
**inadequate sewage facilities** [64].

**4. Burden.** The three sourced figures do not reconcile — 10% of the world's
population and 100,000 deaths/yr [63], ~90 million people/yr [65], 34–50 million
symptomatic infections/yr [66] — and the discrepancy is itself worth recording:
it is the third leading cause of human death among parasitic infections [62],
and the modern figures are confounded because *E. histolytica* was only recently
separated from the morphologically identical non-pathogenic *E. dispar*, which
is distinguishable only molecularly or immunologically [64]. **Do not quote a
single number for this organism.**

**5. The player's counter.**
- **Macrophage: yes, but only activated, and it dies doing it** [61]. This is the
  mechanic — the NK(ILC1) IFN-γ aura that activates nearby macrophages is
  *exactly* the sourced requirement ("lymphokine-activated"), and the 40%
  macrophage mortality is the cost.
- **Neutrophil: no — it is food** [61]. But neutrophils **launch NETs** around
  amoebae "in an explosive manner", covering them in DNA and cell aggregates
  where the parasites are immobilised and killed [62]. The roster lists "NET
  degradation" as an unanswered gap with no NET mechanic to disable. Here is the
  organism that justifies building the mechanic in the first place.
- **Antibody: no.** Heat-inactivated immune serum neither enabled leukocyte
  killing nor protected the leukocytes [61], and the amoeba's displayed human
  membrane proteins defeat serum lysis [59].
- **CTL / γδ / NK direct kill: no target** — it is extracellular throughout.

**6. Redundancy check.**
Closest shortlisted organism: ***Yersinia***, "the anti-tower unit, Yop-injects
phagocytes." Not redundant. *Yersinia* **disables** phagocytes by injection;
*Entamoeba* **eats** them, at contact, and the numbers for the exchange are
published [61]. And **AIEC** "replicates in an acidified phagolysosome without
killing the macrophage — leaves an infected but still-functional tower":
*Entamoeba* is the opposite end of that same axis, the one that destroys the
tower outright. The roster explicitly names that as having no answer. Nothing
else in any pass trogocytoses, and nothing else acquires host membrane proteins
to defeat complement.

**The anatomy problem, stated plainly.** The cyst excysts **in the small
intestine** and the trophozoite then **colonises the large intestine** [64];
amebic colitis, "although it may involve any part of the bowel, is predominantly
seen in the **cecum and ascending colon**" [66], and amebomas form in the cecum
or ascending colon [66]. This is the *C. difficile* situation almost exactly:
the right mechanic on the wrong map.

**Recommendation.** Give it the *C. difficile* treatment — **bank it as a
headline act for the colon map**, and record loudly that it is the organism that
answers three of the roster's five open gaps, so that whoever scopes the colon
map knows what it is worth. If the Director wants the "tower dies from what it
ate" mechanic on Map 01 sooner than the colon map exists, the honest route is
to port the mechanic onto a small-bowel organism and say so — **not** to relocate
*Entamoeba*, because the one thing a player can verify by looking it up is where
it lives.

### 3.4 *Enterocytozoon bieneusi* / *Encephalitozoon intestinalis* — rank 4

**1. The one mechanic.**
**A ranged infector that never adheres.** The spore sits in the lumen,
immobile, and on appropriate stimulation "the long hollow polar tube (ranging
from **50 to 500 µm** in length) is extruded at **ultra-fast speeds (300 µm/s)**
from the spore acting as a **harpoon-like organelle** to transport and deliver
the infectious material or sporoplasm into the host cell" [70]. The transition
from the coiled spring-state inside the spore to the extended needle is
"explosive" [71]. PTP4 and PTP7 localise to the **tip** and are implicated in
engaging host receptors [70]. The older description is still the clearest
statement of the mechanic: all microsporidia respond to stimulus the same way,
the tube discharges from the anterior pole "in an explosive reaction", and "**if
the polar tube is discharged next to a cell, it can pierce the cell and transfer
its sporoplasm into the cell**" [69]. These organisms also have the **smallest
known eukaryotic genomes** and are phylogenetically related to **fungi**, not
protozoa [69] — worth a codex line, because "protozoan" is simply wrong for them
and the title of this document is therefore a convenience, not a taxonomy.
The whole cytoplasm, nucleus included, travels
down the tube; the posterior vacuole and plasma membrane stay behind in the
empty spore coat [71].

In §1b terms this **bypasses the entire proximity-gated adhesion → crowding →
plaque → rupture chain**: no adhesion, no mill, no crowd, no charge. One spore,
one shot, one infected cell, potentially many lanes away.

Then the niche is the exact inverse of *Cryptosporidium*'s. *E. bieneusi*
develops with **neither a sporophorous vesicle nor a parasitophorous vacuole —
all stages in direct contact with the host cell cytoplasm** [67]. Its sister
species *Encephalitozoon (Septata) intestinalis* develops **inside a
parasitophorous vacuole partitioned by a septate, honeycomb-like,
parasite-secreted fibrillar network**, and it **disseminates** — into lamina
propria macrophages, fibroblasts and endothelial cells, and onward to kidney and
gall bladder [68], with documented cholecystitis, cystitis and **small-bowel
perforation** [74].

So the pair is a designed-by-nature niche axis: **naked in the cytosol (fully
exposed to cytosolic sensing, gut-restricted) ↔ wrapped in a septate vacuole
(shielded, and it leaves the epithelium)**. The game's §4b currently has one
intracellular niche. This is two more, with sources.

**2. Morphology for a sprite.**
Spore **1.5 × 0.5 µm**, ovoid, with a **very thin spore wall** (endospore absent
or poorly differentiated) and a **polar tube of 5–6 coils** visible inside [67].
At rest: a tiny capsule with a coiled spiral inside — think a watch spring in a
rice grain. On firing: **a single long straight line** from the spore to a cell,
which at the game's scale is the entire visual and should be drawn as a
one-frame harpoon with a lingering thread.

**Size vs a bacterium: the spore is *smaller* than a bacterium (1.5 µm vs
1–3 µm) — the only candidate here that is — while its polar tube is 20–300× a
bacterium's length.** That ratio is the art direction and the gameplay in one
number: an almost invisible dot with the longest reach on the board. At 20 px
the spore is 1–2 px; it must be drawn with a glow or a reticle, not scaled up.

**3. Exposure route / vehicle.**
Water and food, with documented **foodborne and waterborne outbreaks** [73];
independently the cause of 0.24% (1/416) of global waterborne protozoan
outbreaks 2017–2022 [23]. The specific, vivid route the meta-analysis points at
is **contact with animals** — it infects "a broad array of hosts, from wild to
domestic", has strong zoonotic potential, and the review's own advice is caution
"during contact process with animals" [72][73]. Occupational/rural: rural
prevalence 7.58% > urban [72].

**4. Burden.** See §2a. **6.59% pooled across endemic regions, 16.45% in people
with diarrhoea, 11.49% in HIV-infected, teenagers highest at 10.97%** [72]. The
politically useful fact: it is "responsible for intestinal illness in **both
non- and immunocompromised** patients" [73], and the highest prevalence by
climate was **temperate marine (13.55%)** [72] — this is not an
AIDS-only-and-tropical organism, which is how the 1980s literature framed it.

**5. The player's counter.**
**Mostly unanswered, and the gap is specific.**
- **Against the ranged infection: nothing in the roster.** Every defence that
  acts in the lumen — sentinel goblet sloughing, sIgA coating and cross-linking
  — works by acting on an organism that is *trying to reach the wall*. A spore
  that never approaches is not flushed by sloughing and gains nothing from being
  coated unless coating specifically blocks polar-tube firing, which is
  **UNVERIFIED** in this pass. The γδ IEL's interception suppresses *plaque
  charge*; there is no plaque.
- **Against *E. bieneusi* once inside: the cytosolic route should work, and that
  is the design payoff.** With no vacuole at all [67], this is the one organism
  for which an infected cell's stress display should be *maximal* — it is the
  mechanical opposite of *Cryptosporidium*. Build them as a sourced pair.
- **Against *Encephalitozoon intestinalis*: it leaves.** It disseminates past the
  epithelium into lamina propria macrophages, fibroblasts and endothelium [68].
  The board has no vocabulary for a pathogen that exits the tissue band by any
  route other than reaching the base.
- **One treatment fact worth a mechanic:** in a treated AIDS patient's resected
  bowel there was "no enterocyte microsporidian infection at that time, **only
  spores undergoing macrophage digestion**" [74] — so the macrophage *can* clear
  the spore stage. And in immunocompetent mice *E. cuniculi* persisted
  undetectably and **reactivated on immunosuppression** [75], which is a
  latency mechanic with a trigger.

**6. Redundancy check.**
Closest built or shortlisted: **nothing, in two independent dimensions.** No
built or shortlisted organism infects at range without adhering; no built or
shortlisted organism develops naked in the cytosol (the game's intracellular
bacterium is in a vacuole-ish abstraction and its virus is cytosolic but spreads
by contact-chain or budding, not by injection). The *Encephalitozoon*
dissemination half does overlap conceptually with **EHEC**'s "destruction aimed
past the wall", but EHEC exports a *toxin* past the barrier while
*Encephalitozoon* exports *itself*.

**Recommendation.** Build **one** of the pair. If the aim is to enrich §4b, build
*E. bieneusi* and use the no-vacuole fact to justify a high stress-display
species, which is cheap and composes with everything. If the aim is to challenge
§1b, build the ranged infection — but scope it as a **real** change to the
ingress model, not a parameter, and expect it to interact badly with crowd
desensitisation (§7) and with the plaque's "ingress is something a CROWD does,
never something a pathogen does" invariant, which a harpoon violates by
construction.

### 3.5 *Cyclospora cayetanensis* — rank 5

**1. The one mechanic.**
**No feedback loop.** "Because oocysts are excreted **unsporulated** and need to
sporulate in the environment, **direct person-to-person transmission is
unlikely**" [76]; oocysts mature for **7–10 days** before sporulating [80], and
complete sporulation took **5–13 days** at 25–32 °C in the original
characterisation [78]. Humans are the only known host [77].

Every other pressure source in the game loops back through the board: adherents
replicate on the wall (§1b), a dying infected cell bursts a brood (§4b), a
breached stretch charges faster (§1b). *Cyclospora* cannot: nothing it does on
the board can increase the next wave. It is **invasive and takes a host-cell
slot** (asexual schizonts and sexual gamonts in biliary-intestinal epithelium,
stages <10 µm in section [77]) and then its progeny leave and are *inert*.

Mechanically this is a **round-design lever** more than a tower-design lever: a
species whose arrival schedule is authoritative and unmodifiable, and which can
never re-seed from a breach. It is also, as of Sprint 33's 40-round curriculum,
the only honest way to present a *purely scripted* pressure without lying about
why.

**2. Morphology for a sprite.**
Described at discovery as resembling "a **large *Cryptosporidium***" [79]; exact
oocyst dimensions **UNVERIFIED in this pass** (the literature's chosen size
surrogate is *Eimeria acervulina*, which "most closely resembles *C.
cayetanensis* in size" [77]). The distinguishing internal structure *is* verified
and is the whole silhouette: **two sporocysts, each containing two sporozoites**,
versus *Cryptosporidium*'s **four naked sporozoites** [78]. So: a circle
containing **two circles, each containing two commas** — against
*Cryptosporidium*'s circle containing **four bare commas**. That difference is
drawable at 20 px as *two inner rings vs none*, and it is the correct way to
distinguish the two sprites, because their outer outlines are deliberately
similar. Male microgamonts have **two flagella**; female macrogametes contain
**wall-forming bodies** [77] — both available if a life-cycle inspector is ever
drawn. **Size vs a bacterium: UNVERIFIED, but larger than *Cryptosporidium*'s
4–6 µm**, so >2–4×.

**3. Exposure route / vehicle.**
The best callout in the kingdom, and all of it verified: **imported fresh
produce** — the US and Canadian outbreaks were associated with contaminated
**raspberries and basil** [81], and the risk in developed countries is "travel
to endemic areas and the domestic consumption of contaminated food, **mainly
fresh produce imported from endemic regions**" [76]. **Soil contact** is
correlated with infection, and water and soil may be reservoirs as well as
vehicles [80]. Infection is "remarkably **seasonal** worldwide" [76] — which is a
free round-schedule hook.

**4. Burden.** See §2a. Low as a waterborne cause (0.72% of outbreaks [23]) and
important as a **foodborne** one in developed countries [76]. The age pattern
flips by setting: children/foreigners/immunocompromised in endemic countries,
**any age in industrialised countries** [76]. Self-limiting in most
immunocompetent patients, but can be severe, protracted or chronic, and can
colonise extra-intestinal organs in the immunocompromised [76].

**5. The player's counter.**
Nothing specific is required, which is the point: it is answered by the same
tools as *Cryptosporidium* (it is a coccidian, phylogenetically close to
*Eimeria* and *Isospora* [79]) and it **cannot be answered at the source**,
because the source is off-board. The one thing a player can do that *does* work
is a lumen-stage intervention before adhesion — i.e. the ~70% sIgA rung — and the
one thing that does *not* work is any amount of cleanliness on the board.
Research note: much of its biology is genuinely unknown, there is no infection
model and no readily available oocysts [81], so **do not invent numbers for
it**; the per-stage detail in this pass is thinner than for any other ranked
candidate.

**6. Redundancy check.**
Closest candidate: ***Cryptosporidium***, and they are nearly the same organism
morphologically and taxonomically. **Cut it as a standalone threat; keep it as
*Cryptosporidium*'s contrast.** The pair earns one slot between them because the
contrast is the lesson — single-host amplification in <3 days with obligate sex
[11] against obligate environmental maturation with no autoinfection [76] — and
because the sprites are *designed* to be confusable, which makes "read the
internal bodies" a real skill. On its own, with the "no feedback loop" mechanic
subtracted, *Cyclospora* is just a *Cryptosporidium* with a slower clock, and the
selection rule cuts it.

### 3.6 *Cystoisospora belli* — rank 6, recommended as a round modifier

**1. The one mechanic.**
**It is defined by the player already losing.** Found worldwide, infects only
humans, and is "**almost always** in individuals with an immuno-compromised
status" [82]. Three sourced properties that are not elsewhere on the board:
- On histopathology it may be found **invading beyond the epithelium**, with
  **eosinophils** present [82] — an immune cell type the roster has no tower for.
- It **disseminates** to the biliary tree in AIDS, with extra-intestinal spread
  [83].
- **Relapse and chronic infection develop even after an adequate initial
  response and even after immune reconstitution** [82], which is a sourced
  argument for an enemy that survives the player repairing the weakness that let
  it in.

**2. Morphology for a sprite.** "Characteristic **elliptical** oocysts" on
modified acid-fast or other special stains [82]; exact dimensions
**UNVERIFIED**. Asexual schizonts and sexual gamonts in biliary-intestinal
epithelium [82]. A long ellipse distinguishes it from *Cryptosporidium*'s circle
and *Cyclospora*'s ringed circle; **relative size vs a bacterium: UNVERIFIED**.

**3. Exposure route / vehicle.** **UNVERIFIED in this pass** beyond faecal-oral;
no vehicle-specific outbreak data was retrieved. Do not write a callout for it
from recollection.

**4. Burden.** No population figure sourced — **UNVERIFIED**. Case-report
literature; multiple stool collections may be needed to find it [82], so the
true rate is probably understated and that understatement is not quantified.

**5. The player's counter.** None in the roster, and the honest reading is that
**none should be**: the organism's real-world counter is
trimethoprim-sulfamethoxazole plus antiretroviral therapy [82] — i.e. fixing the
immune system, which in game terms is the player's whole build rather than one
tower.

**6. Redundancy check.** Against the selection rule: **cut as a species.** Its
"one mechanic" is a *precondition*, not a decision. Nothing about it forces a
choice the player would not already be making. **Recommendation: implement it as
a late-round modifier** ("opportunists" — a wave that only spawns if the player's
tissue health or tower count is below a threshold), which is where its one real
idea lives, and which costs no sprite, no barcode slot (the ladder has a hard
10-bit roster ceiling per §5) and no new class.

---

## 4. What this means for the game

- **Build *Cryptosporidium* first, and build the apoptosis-resistance trait with
  it.** It is the leading protozoan cause of severe infant diarrhoea [18][19], it
  is the only enteric eukaryote that matches the built roster's scale of burden,
  and its headline mechanic is a **sourced attack on the reward structure §4c
  just shipped**. `TissueTuning.ApoptoticDebrisFraction` is described in §4c as
  "the whole reward in one number"; it is not, if a species can make the
  apoptotic route itself unreliable [8]. One new per-species float
  (`ApoptosisResistance`) buys the mechanic.
- **The epicellular niche is real, and the design conclusion is the opposite of
  the obvious one.** The parasite is **inside the cell and outside the
  cytoplasm**, behind a vacuole wall built from the host's own brush-border
  membrane and carrying the host's own villin and ezrin [5], separated from the
  apical cytoplasm by an electron-dense layer [4], and the niche is explicitly a
  barrier to anything entering from the cytosol [3]. But that does **not** make it
  a stealth enemy: innate detection works *beautifully*, because the sensor is
  the **host cell itself** — enterocyte-intrinsic NLRP6 → caspase-1 → IL-18,
  functional in germ-free mice, and enterocyte-only caspase-1 deletion
  phenocopies whole-body KO [12]. And human CD8⁺ T cells **do** lyse infected
  IECs and reduce parasite load, via HLA-A *and* HLA-B [16]. So the sourced
  design is: **highly visible, hard to finish.** That is a much better enemy than
  a stealth one, and it is the opposite of what "hidden behind a host membrane"
  suggests. Do not build it as stealth.
- **The "no inflammation, no cell death" profile exists, and *Giardia* has it.**
  Three independent reviews state it [31][32][33]. It defeats the §4b contact
  stress-sense roll by having no infected cell, defeats the §1c debris→DC→
  knowledge loop by producing no debris, and defeats §7/§9 chemotaxis by
  degrading the chemokines at source [38]. This is the astrovirus idea — "breach
  with no infection and no stress signal" — arriving a second time from a
  completely different kingdom, which is strong evidence it is a real category
  and worth a mechanic rather than a one-off.
- **Trogocytosis is real, it is the best single mechanic in the kingdom, and it
  is on the wrong map.** Verified in *Nature* [55] with the mechanism separated
  from phagocytosis [58], the immune-evasion consequence published [59], and the
  tower-killing published in 1985 with numbers [61]. And the trophozoite
  colonises the **large intestine** [64][66]. Bank it for the colon map, next to
  *C. difficile*, and record what it is worth so the colon map gets scoped.
- **The oocyst/cyst stage is a mechanic a bacterium or virus cannot express, and
  it has a measured spread.** *Cryptosporidium* needs **Ct 10,400–15,300**
  mg·min/L free chlorine for 3-log inactivation [24], ozone efficiency orders
  as **viruses ≫ *Giardia* cysts > *C. parvum* oocysts** [25], and *Cyclospora*'s
  oocyst is time-locked rather than chemical-proof [80][81]. So the stage is not
  a flat "environmental" trait: it is **per-species resistance to the lumen's
  hostility plus a per-species maturation delay**, and both are sourced. If the
  lumen band ever gets a chemical/flush hazard, this is the species-differentiating
  knob for it — and the one trait that makes "a pathogen that ignores the lumen's
  defences entirely" honest rather than arbitrary.
- **The eukaryotes do NOT need §4a's multi-slot Parasite footprint.** See
  Corrections (2). This is the single most actionable finding in the pass,
  because `BACKLOG.md` currently treats the footprint as "more engineering than
  the other three classes combined" — and it is not on the critical path for any
  organism here.
- **Do not build a protozoan because it is common.** *Dientamoeba fragilis* was
  found in **32.5% of 36,008 children** with no detectable clinical consequence
  [88], and *Blastocystis* colonises over a billion people with the clinical
  question still open [84][87]. They are the kingdom's two most prevalent
  members and the two clearest cuts. Conversely *Cyclospora* causes 0.72% of
  waterborne outbreaks [23] and brings a mechanic nothing else has. The
  selection rule holds.
- **Three of the ranked candidates break §1b rather than enriching it, and that
  should be a deliberate choice.** The microsporidian harpoon bypasses
  proximity-gated adhesion entirely [70]; *Giardia*'s suction grip strengthens
  under load [36], so sloughing and IgA-raft sweeping should work *worse* the
  harder they pull; and *Giardia* trophozoites are a third the width of an
  enterocyte, so §1b's "crowding measured over a cell and a half of wall" does
  not mean the same thing for them as for bacteria. Pick which of these to
  honour; do not honour all three by accident.

---

## 5. Cuts, and why

| Organism | Prevalence | Why cut |
|---|---|---|
| ***Blastocystis*** spp. | **The most common gastrointestinal protist in humans** — pooled 17.9% across 562,327 participants; Africa 44.2% [84]; >1 billion people colonised [87] | **Anatomy, then evidence.** It "**colonizes the large intestine**" [85] — the *C. difficile* rejection again. And even setting that aside, the mechanics are *in vitro* only and the authors say so: it secretes cysteine proteases that "contribute to pathogenesis through degradation of secretory IgA, Rho/ROCK-mediated tight-junction compromise, NF-κB-mediated secretion of inflammatory cytokines, and host cell apoptosis" — immediately followed by "**It is currently unknown whether this occurs in vivo**" [85]. Most microbiota studies find it a **normal constituent of the healthy gut associated with higher bacterial richness**, with long-term asymptomatic carriage common [85][86], and it is *rarer* in IBD patients [85]. One subtype-level hook if it is ever revisited: **ST4** is associated with IBS and inflammation but **protective in murine colitis**, while **ST3** is typical of healthy people with diverse microbiota [87]. |
| ***Dientamoeba fragilis*** | **32.5% of 36,008 children** and **22.3% of 27,918 adults** PCR-positive [88][89] | **No mechanic, and arguably no disease.** In two large retrospective cohorts, positives had **no higher odds** of pre-test symptoms or of any post-test outcome than all-negative patients, and PCR cycle threshold did not predict outcome either [88][89]. The honest counter-argument is recorded: a 2026 review argues the commensal evidence may itself be contaminated by **false positives in at least one widely used real-time PCR assay**, and that there is **no consensus even on the infective stage** (*Enterobius* ova as vector, versus a rarely found cyst) [90]. So the cut is "no mechanic to build", not "proven harmless". Keep as the roster's standing example that prevalence is not a slot. |
| ***Entamoeba dispar*** | Common | Non-pathogenic commensal, morphologically identical to *E. histolytica* and separable only molecularly or immunologically [64]. Its only game-relevant property is that it **confounds the burden figures** for *E. histolytica*, which is why §2a refuses to quote one number. |
| ***Encephalitozoon cuniculi*** | Latent in immunocompetent hosts [75] | Not an enteric primary. Its one interesting mechanic — undetectable persistence followed by **reactivation on immunosuppression** [75] — is better carried by *Cystoisospora* as a round modifier (§3.6) or by CMV, which the viruses pass already shortlisted as the stealth boss. |
| ***Encephalitozoon hellem*** | 11 non-HIV cases in France [73 ref. ctx] | Real and newly delineated as a gut-restricted disease in non-HIV immunocompromised patients, but it brings nothing *E. bieneusi* and *E. intestinalis* do not already cover between them. |
| ***Balantioides coli***, ***Toxoplasma*** gut stages, free-living amoebae | — | Out of scope: colonic, non-enteric, or not gut pathogens. *Toxoplasma* and *Naegleria* appear in the waterborne-outbreak survey [23] but neither is a small-intestinal enemy. |
| ***Goussia janae*** | Fish | Not a human pathogen — included only because it is the comparison that sharpens the *Cryptosporidium* niche: *G. janae* is also epicellular but **lacks the feeder organelle**, so it cannot scavenge nutrients directly from the host [93]. That contrast is what makes *Cryptosporidium*'s feeder organelle a real, distinguishing structure rather than a label. |

---

## 6. Corrections to standing design

1. **§4a's class table has no row that fits *Cryptosporidium*, and both halves of
   the row it would need are non-obvious.** It is **intracellular but
   extracytoplasmic** [3][4]: it takes a host-cell slot and flips the host layer
   to `Infected`, but it sits in a vacuole whose wall is made of the enterocyte's
   own brush-border membrane and proteins [5][6] and is sealed from the apical
   cytoplasm by an electron-dense layer [4]. §4a's "Cleared by" column would
   predict a stealth enemy; the literature says the opposite in both directions —
   the **host cell is the sensor** (enterocyte-intrinsic NLRP6/caspase-1/IL-18,
   germ-free-independent, enterocyte-only deletion phenocopying whole-body KO
   [12]) and **human CD8⁺ T cells do lyse infected IECs** [16], while in mice
   anti-CD8 alone permits no infection at all and CD4 + IFN-γ are the required
   pair [15]. §4a needs either a fourth intracellular niche or an explicit note
   that the niche axis and the detectability axis are independent.

2. **§4a's "Parasite" class is specified wrongly for this entire kingdom, and
   fixing it unblocks all of it cheaply.** The row reads "Multiple coarse slots
   at once… Direct combat damage, **more of it (bigger target)**", and
   `BACKLOG.md` treats the multi-slot footprint as "more engineering than the
   other three classes combined". **Not one enteric protozoan needs it.** The
   largest organism in the pass is an *Entamoeba* trophozoite at 10–60 µm
   [64] — comparable to one to three enterocytes — and the ranked candidates are
   a 15 × 7.5 µm *Giardia* trophozoite [37], a 4–6 µm *Cryptosporidium* oocyst
   whose intracellular stage is 2–6 µm [6][29], and a **1.5 × 0.5 µm**
   microsporidian spore that is *smaller than the game's bacterium* [67]. The
   class should be re-specified as **"a single-slot organism 5–20× a bacterium's
   linear size, cleared by direct damage"**, with the multi-slot footprint
   deferred to helminths — which are not protozoa, are not in this pass, and
   are the only things that actually span cells. A parallel
   `docs/research/pathogens_helminths.md` pass exists as of the same day and is
   where that footprint's requirements should be sourced from. This is a specification change that
   removes a blocker rather than adding work.

3. **§4c's CTL framing needs a named counterexample, and giardiasis is it.** §4c
   says "the adaptive payoff is that the tissue recovers instead of scarring" and
   treats the apoptotic route as strictly better. In giardiasis the CD8 cells
   **are the lesion**: adoptive transfer of enriched CD8⁺ (and whole) lymphocytes
   from infected donors reproduced diffuse microvillous shortening, lost brush
   border surface area, impaired sucrase and raised crypt/villus ratios, and
   **CD4⁺ transfer did not** [43][44][45]. The roster's existing note that "the
   player's intuitive 'build a killer T cell' answer is wrong for most of the
   bacterial roster — which is a good thing, and worth teaching deliberately"
   now has its strongest case: an organism where the CTL is not merely
   ineffective but **actively harmful**.

4. **§4c's apoptosis table assumes the apoptotic route is always available.** It
   is not. *Cryptosporidium*-infected cells are **less apoptotic than bystanders**
   and are **specifically protected against induced apoptosis while bystanders
   are not** [8], and infection *attenuates* strongly proapoptotic agents [9].
   So `ApoptoticDebrisFraction` is not "the whole reward in one number" — it is
   the reward *when the species permits the apoptotic route*. Without a
   per-species resistance factor, the §1d "PRR → immunogenic apoptosis" host-cell
   upgrade also silently does nothing against the leading protozoan cause of
   severe infant diarrhoea.

5. **§1b's wall asymmetry is a two-way split and a eukaryote breaks it.** §1b
   says bacteria replicate on the wall while "viruses do not replicate out here:
   no host, no replication", and concludes "a battered wall is a bacterial sieve
   and a viral dead end". *Giardia* **replicates extracellularly on the wall by
   binary fission and never needs a host cell at all** [33], so it behaves like a
   bacterium for replication while needing a **living, mucus-covered** wall for
   grip. It gains nothing from a dead stretch *and* is not stopped by one, which
   is a third position the asymmetry has no slot for.

6. **§1b's "ingress is something a CROWD does, never something a pathogen does"
   is violated by a real organism.** That invariant is listed as load-bearing and
   "must survive any future change". A microsporidian spore infects **one cell,
   alone, from 50–500 µm away, at 300 µm/s, with no adhesion and no crowd**
   [70][71]. The invariant is still the right call for bacteria, but it is a
   *bacterial* invariant, not a universal one, and the doc should say so rather
   than letting a future pass discover the conflict.

7. **§1b's two flush defences should work *worse* under load against *Giardia*,
   and the measurement exists.** Sentinel sloughing and IgA cross-linked rafts
   both work by pulling an organism off the wall. *Giardia*'s attachment is
   ligand-independent suction with a retract-force curve unlike any other
   eukaryote measured, and the adhesion force **rose from 7.7 ± 4.2 nN at 1 µm/s
   to 28.7 ± 10.5 nN at 10 µm/s** [36]. A flush that gets *less* effective the
   faster it is applied is a sourced, legible, counter-intuitive trait and a
   much better differentiator than a flat "resistant to sloughing" flag.

8. **§1c's "debris is the input to adaptive immunity" has a hard counter, and it
   may be a softlock.** §1c establishes that all antigen comes from dead host
   cells, and that of debris's three fates only DC shuttling teaches anything.
   *Giardia* kills no cells on the round timescale and causes no inflammation
   [31][32][33]. A *Giardia*-only round therefore generates **no debris, no DC
   cargo and no knowledge** — so the ~20% and ~70% rungs that are its real
   counters can never be reached from that round. This is either an outstanding
   hard counter to the §5a shuttle (and the natural companion to astrovirus) or a
   dead end that cannot be played out of. **Needs a Director decision**, and it
   needs it before *Giardia* is built, not after.

9. **The roster's "Still with no answer" list can be updated on two entries, and
   the news is mixed.** **"Killing the phagocyte outright (we have no 'tower dies
   from what it ate')"** — found, with 1985 numbers: amoebae killed neutrophils,
   PBMC, monocytes and macrophages without losing viability, while activated
   macrophages killed 55% of amoebae at the cost of 40% of themselves [61].
   **"NET degradation (no NET mechanic to disable)"** — the justification for
   *building* the NET mechanic is found: *E. histolytica* induces NETs that
   immobilise and kill it [62]. Both findings are in the **colonic** organism. The
   list should record that the answers exist and are banked with the colon map,
   rather than continuing to read as though nothing in the literature does this.

10. **§5's ~70% secretory-IgA rung now has named enteric counters, and one of them
    inverts it.** The rung's stated effect is that coating and cross-linking act
    on a species *before adhesion is possible*. *Giardia*'s secreted cysteine
    proteases cleave **IgA and IgG** (and α-HD6 and β-HD1) [39]; *Blastocystis*
    independently degrades secretory IgA *in vitro* [85]. More interesting:
    anti-VSP antibody is **non-cytotoxic** and instead **triggers the antigenic
    switch** [46], so investing in the ~20% rung actively fires §5's own
    "discrete step-change discount" against the species being learned. Honest
    caveat, and this is a genuine disagreement in the literature rather than a
    settled fact: an earlier review states that **antibodies to the VSP amino
    terminus *are* cytotoxic in vitro** and drive immune selection and
    replacement in vivo [47]. Build the switch-on-antibody mechanic if desired,
    but record that it rests on a 2023 result that contradicts the prior
    consensus [46][47], and do not present it as settled.

11. **A clarification worth recording, not a correction: the game's "parasite"
    word is doing two jobs.** §4a's Parasite class means "multi-slot, big,
    dumb". Every organism in this pass is a eukaryotic parasite and not one of
    them is that. If the class keeps its name, the roster will eventually have
    "parasites" that are not Parasites, which is exactly the kind of naming rot
    §1c was written to avoid. Suggest renaming the class **Macroparasite** (or
    deferring it to a helminth pass) and leaving the protozoa in the existing
    intracellular/extracellular classes with a size multiplier.

---

## 7. Still with no answer in the roster — additions from this pass

Flagged so a future tower or mechanic can be justified by a real gap rather than
invented, in the roster's existing format:

- **An infected cell that resists the player's kill.** *Cryptosporidium* [8][9].
  No tower and no upgrade addresses it; §4c's whole reward structure assumes it
  cannot happen.
- **Ranged infection with no adhesion.** The microsporidian polar tube [70][71].
  Every lumen-stage defence in the game acts on an organism approaching the wall.
- **A pathogen that leaves the tissue band other than by reaching the base.**
  *Encephalitozoon intestinalis* into lamina propria macrophages, fibroblasts and
  endothelium [68], onward to kidney and gall bladder [74].
- **Damage delivered by the player's own adaptive tower.** *Giardia* via CD8
  [43][44][45]. The roster has inflammation-as-fuel (A/E *E. coli*, innate); this
  is the adaptive-tier version and there is no mitigation for it anywhere.
- **Degradation of the recruitment cytokine at source.** *Giardia* cysteine
  proteases on CXCL1/2/3, IL-8, CCL2, CCL20 [38]. The viruses pass flagged
  coxsackievirus B3's 3A protein as the only candidate for this; here is a second,
  independent one, which makes it a category rather than a quirk.
- **A hard-coded, player-proof developmental clock.** *Cryptosporidium*'s three
  asexual generations then obligate sex, "independent of environmental stimuli"
  [10][11]. Everything the player can currently do changes a rate; nothing is
  immune to intervention.
- **Eosinophils.** *Cystoisospora belli* histology shows them [82] and the roster
  has no eosinophil tower, no eosinophil mechanic and no reason to build one
  until a helminth pass exists. Recorded, not advocated.

Candidate answers named by this pass: a **mucus/barrier-focused non-cytotoxic
tower** (the only plausible answer to *Giardia*, and the γδ IEL's repair is the
closest existing thing); **IL-18 / epithelial-sensor plumbing** that lets an
*infected host cell* call the ILC1 rather than requiring a tower to find it
[12][13] — which would also give the built NK(ILC1) amplifier a second job; and
a **NET mechanic**, now justified by a real organism [62].

---

## 8. References

PubMed-sourced entries are cited with DOI links per the PubMed tool's
attribution requirement; *according to PubMed*. Entries retrieved via Consensus
link to the Consensus record where no DOI was returned in this session.

- [1] Zhang Y, Jiang P, Yin J, Zhu G. (2025) *Pathogenicity and virulence of Cryptosporidium.* Virulence 16(1):2597642. [DOI](https://doi.org/10.1080/21505594.2025.2597642)
- [2] Guérin A, Strelau KM, Barylyuk K, et al. (2023) *Cryptosporidium uses multiple distinct secretory organelles to interact with and modify its host cell.* Cell Host Microbe 31(4):650-664.e6. [DOI](https://doi.org/10.1016/j.chom.2023.03.001)
- [3] Tzipori S, Griffiths JK. (1998) *Natural history and biology of Cryptosporidium parvum.* Adv Parasitol 40:5-36. [DOI](https://doi.org/10.1016/s0065-308x(08)60116-5)
- [4] Beĭer TV, Sidorenko NV. (1991) *Electron microscopic research on cryptosporidia. III. Parasite-host relations.* Tsitologiia 33(1):18-23. PMID 1866791 (no DOI).
- [5] Bonnin A, et al. (1999) *Immunodetection of the microvillous cytoskeleton molecules villin and ezrin in the parasitophorous vacuole wall of Cryptosporidium parvum.* Eur J Cell Biol. [Consensus record](https://consensus.app/papers/details/72bc8613d6ac5f43ab70550c2cf93d14/)
- [6] Angus KW. (1983) *Cryptosporidiosis in man, domestic animals and birds: a review.* J R Soc Med. [Consensus record](https://consensus.app/papers/details/ef461676c3a45dbba7c0cc354ad598dc/)
- [7] Guérin A, Striepen B. (2020) *The biology of the intestinal intracellular parasite Cryptosporidium.* Cell Host Microbe. [Consensus record](https://consensus.app/papers/details/4dd38d87c2fd5f61a5e08e3f0fc50407/)
- [8] Wallbank BA, et al. (2024) *Cryptosporidium impacts epithelial turnover and is resistant to induced death of the host cell.* mBio. [Consensus record](https://consensus.app/papers/details/b8129eeb9e885e60b2bd178e6fb50f9b/)
- [9] McCole DF, et al. (2000) *Intestinal epithelial cell apoptosis following Cryptosporidium parvum infection.* Infect Immun. [Consensus record](https://consensus.app/papers/details/072fbf8449255b59b5427e283ccadebd/)
- [10] Tandel J, et al. (2019) *Lifecycle progression and sexual development of the apicomplexan parasite Cryptosporidium parvum.* Nat Microbiol. [Consensus record](https://consensus.app/papers/details/b7b205b2dea85ec58d18ee110ed2d273/)
- [11] English ED, et al. (2022) *Live imaging of the Cryptosporidium parvum life cycle reveals direct development of male and female gametes from type I meronts.* PLoS Biol. [Consensus record](https://consensus.app/papers/details/bd81631247ea5a4bbce408c6c07de6cc/)
- [12] Sateriale A, et al. (2020) *The intestinal parasite Cryptosporidium is controlled by an enterocyte intrinsic inflammasome that depends on NLRP6.* PNAS. [Consensus record](https://consensus.app/papers/details/12196e10bd845c5a889c50230e2e4c35/)
- [13] Gullicksrud JA, et al. (2021) *Enterocyte–innate lymphoid cell crosstalk drives early IFN-γ-mediated control of Cryptosporidium.* Mucosal Immunol. [Consensus record](https://consensus.app/papers/details/815b279788ed5389a263bed5401777ec/)
- [14] Pardy RD, et al. (2023) *Immunity to Cryptosporidium: insights into principles of enteric responses to infection.* Nat Rev Immunol. [Consensus record](https://consensus.app/papers/details/0aff7f9b0a6f5f80b90545cd57743099/)
- [15] Ungar BL, et al. (1991) *Cryptosporidium infection in an adult mouse model. Independent roles for IFN-gamma and CD4+ T lymphocytes in protective immunity.* J Immunol. [Consensus record](https://consensus.app/papers/details/d5c59ffc4d6d5051a6bda4cdcfe23690/)
- [16] Pantenburg B, et al. (2010) *Human CD8(+) T cells clear Cryptosporidium parvum from infected intestinal epithelial cells.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/2709c404e9925fe388529481fd851a40/)
- [17] Cohn IS, et al. (2024) *Intestinal cDC1s provide cues required for CD4+ T cell–mediated resistance to Cryptosporidium.* J Exp Med. [Consensus record](https://consensus.app/papers/details/854d312d91b75ca685e6ab69759ebe07/)
- [18] Sow SO, et al. (2016) *The burden of Cryptosporidium diarrheal disease among children < 24 months of age in moderate/high mortality regions of sub-Saharan Africa and South Asia, utilizing data from GEMS.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/5898623aedd9553e9b446750c3f2f11a/)
- [19] Kotloff KL, et al. (2013) *Burden and aetiology of diarrhoeal disease in infants and young children in developing countries (GEMS): a prospective, case-control study.* Lancet. [Consensus record](https://consensus.app/papers/details/4de7ae0a038850bcb6926d4385c0c5e6/)
- [20] Omore R, et al. (2018) *Clinical, environmental, and behavioral characteristics associated with Cryptosporidium infection among children with moderate-to-severe diarrhea in rural western Kenya (GEMS).* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/7ab5773b03ed574eb11885ac30d89c6f/)
- [21] Gharpure R, et al. (2019) *Cryptosporidiosis outbreaks — United States, 2009–2017.* MMWR. [Consensus record](https://consensus.app/papers/details/9b2218dbff5759dfaf4ee9f9cb0141aa/)
- [22] Hlavsa MC, et al. (2021) *Outbreaks associated with treated recreational water — United States, 2015–2019.* [Consensus record](https://consensus.app/papers/details/9e69e8110cc65b00865e768a782a75d1/)
- [23] Bourli P, et al. (2023) *Waterborne transmission of protozoan parasites: a review of worldwide outbreaks — an update 2017-2022.* J Water Health. [Consensus record](https://consensus.app/papers/details/26a5ff0a7ae452159a723c08e0ff1021/)
- [24] Shields JM, et al. (2008) *Inactivation of Cryptosporidium parvum under chlorinated recreational water conditions.* J Water Health. [Consensus record](https://consensus.app/papers/details/64029bf4e8f15c27bbfc6239ed5c483c/)
- [25] Morrison CM, et al. (2022) *Ozone disinfection of waterborne pathogens and their surrogates: a critical review.* Water Res. [Consensus record](https://consensus.app/papers/details/f776926be841589289d81e0ed2cdea2f/)
- [26] Messner MJ, et al. (2001) *Risk assessment for Cryptosporidium: a hierarchical Bayesian analysis of human dose response data.* Water Res. [Consensus record](https://consensus.app/papers/details/89bf1cb76d3657d296987dc42d7ff493/)
- [27] DuPont HL, Chappell CL, Sterling CR, Okhuysen PC, Rose JB, Jakubowski W. (1995) *The infectivity of Cryptosporidium parvum in healthy volunteers.* N Engl J Med 332:855-9; summarised in J Pediatr Gastroenterol Nutr. [Consensus record](https://consensus.app/papers/details/a3806aca180057feab2da377b2a54731/)
- [28] Chappell CL, et al. (1999) *Infectivity of Cryptosporidium parvum in healthy adults with pre-existing anti-C. parvum serum immunoglobulin G.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/7405e0035f1f5a62853281cb15564ed0/)
- [29] Fayer R, et al. (2000) *Epidemiology of Cryptosporidium: transmission, detection and identification.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/553d6425d52c5cee86402cfd054d5538/)
- [30] Robinson G, Elwin K, Jones M, Chalmers RM. (2023) *A comparison of qPCR and microscopy for the detection and enumeration of Cryptosporidium oocysts from drinking water.* J Med Microbiol 72(6). [DOI](https://doi.org/10.1099/jmm.0.001715)
- [31] Eckmann L. (2003) *Mucosal defences against Giardia.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/c2fc111715305d70b1643469b95b22f4/)
- [32] Cotton JA, et al. (2015) *Disruptions of host immunity and inflammation by Giardia duodenalis: potential consequences for co-infections in the gastro-intestinal tract.* Pathogens. [Consensus record](https://consensus.app/papers/details/3ec186bf90a55a56832445f8ebb306ca/)
- [33] Rojas-López L, Marcial-Quino J, Svärd SG, et al. (2022) *Giardia duodenalis.* Trends Parasitol. [Consensus record](https://consensus.app/papers/details/0e4d5910d7985a99a6b0d4e538c17908/)
- [34] Friend DS. (1966) *The fine structure of Giardia muris.* J Cell Biol. [Consensus record](https://consensus.app/papers/details/2dc8a64da1d65e01a0b9bce5eb8373e0/)
- [35] Gadelha AP, et al. (2022) *Nanoarchitecture of the ventral disc of Giardia intestinalis as revealed by high-resolution scanning electron microscopy and helium ion microscopy.* Histochem Cell Biol. [Consensus record](https://consensus.app/papers/details/5bcf5443628d55108eaad4073c08e90a/)
- [36] Gunaratnam G, et al. (2024) *Characterization of a unique attachment organelle: single-cell force spectroscopy of Giardia duodenalis trophozoites.* Nanoscale. [Consensus record](https://consensus.app/papers/details/4c8b0d513d605818849b12ea553d2bd1/)
- [37] Yang H, et al. (2025) *First characterization and zoonotic potential evaluation of Giardia duodenalis in ferrets in China.* Transbound Emerg Dis. [Consensus record](https://consensus.app/papers/details/8aa021a4a9c7576ca2f1b6e732958a76/)
- [38] Liu J, Ma'ayeh S, Peirasmaki D, Lundström-Stadelmann B, Hellman L, Svärd SG. (2018) *Secreted Giardia intestinalis cysteine proteases disrupt intestinal epithelial cell junctional complexes and degrade chemokines.* Virulence 9(1):879-894. [DOI](https://doi.org/10.1080/21505594.2018.1451284)
- [39] Liu J-Y, et al. (2018) *Cleavage specificity of recombinant Giardia intestinalis cysteine proteases: degradation of immunoglobulins and defensins.* Mol Biochem Parasitol. [Consensus record](https://consensus.app/papers/details/dbee1ab9134856228caa4219255a3c28/)
- [40] Ortega-Pierres G, et al. (2018) *Giardipain-1, a protease secreted by Giardia duodenalis trophozoites, causes junctional, barrier and apoptotic damage in epithelial cell monolayers.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/67f271f898005d44bf7fd072f2d3bb6f/)
- [41] Chin AC, et al. (2002) *Strain-dependent induction of enterocyte apoptosis by Giardia lamblia disrupts epithelial barrier function in a caspase-3-dependent manner.* Infect Immun. [Consensus record](https://consensus.app/papers/details/c46829febc94563caf756ba265b0f43a/)
- [42] Koh WH, et al. (2013) *Giardia duodenalis assemblage-specific induction of apoptosis and tight junction disruption in human intestinal epithelial cells: effects of mixed infections.* [Consensus record](https://consensus.app/papers/details/744357e0f06f563d879169098db3312a/)
- [43] Scott KG-E, et al. (2004) *Role of CD8+ and CD4+ T lymphocytes in jejunal mucosal injury during murine giardiasis.* Infect Immun. [Consensus record](https://consensus.app/papers/details/86e7b4a7da185f048005a027bf759f09/)
- [44] Buret AG. (2005) *Immunopathology of giardiasis: the role of lymphocytes in intestinal epithelial injury and malfunction.* Mem Inst Oswaldo Cruz. [Consensus record](https://consensus.app/papers/details/fa2ce8c1ef365bfb957ad7e1f5e8f9e5/)
- [45] Cotton JA, et al. (2011) *Host parasite interactions and pathophysiology in Giardia infections.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/7e725bf006835c85a1f3d0d8c880ed7b/)
- [46] Tenaglia AH, Luján LA, Ríos DN, et al. (2023) *Antibodies to variable surface antigens induce antigenic variation in the intestinal parasite Giardia lamblia.* Nat Commun 14:2537. [DOI](https://doi.org/10.1038/s41467-023-38317-8)
- [47] Nash TE. (2011) *Antigenic variation in Giardia.* [Consensus record](https://consensus.app/papers/details/0dbd3f7aa95a5096973be8fb78a7ac5b/)
- [48] Adam RD. (2006) *Antigenic variation of the VSP genes of Giardia lamblia.* [Consensus record](https://consensus.app/papers/details/ef8cf461d55959a383bab682f0181aae/)
- [49] Rodríguez-Walker M, Molina CR, Luján LA, et al. (2022) *Comprehensive characterization of Cysteine-rich protein-coding genes of Giardia lamblia and their role during antigenic variation.* Genomics 114(5):110462. [DOI](https://doi.org/10.1016/j.ygeno.2022.110462)
- [50] Einarsson E, Ma'ayeh S, Svärd SG. (2016) *An up-date on Giardia and giardiasis.* Curr Opin Microbiol. [Consensus record](https://consensus.app/papers/details/298b363000875ecaaffd615d0aadcfaa/)
- [51] Černíková L, et al. (2018) *Five facts about Giardia lamblia.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/220d5a7bae3351d5ac8d4cd05ba97fd9/)
- [52] Ryan U, et al. (2018) *Giardia: an under-reported foodborne parasite.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/9ff5fa2ba6ff5c9ab3a13d9ed51d6c83/)
- [53] Rendtorff RC. (1954) *The experimental transmission of human intestinal protozoan parasites. II. Giardia lamblia cysts given in capsules.* Am J Hyg 59:209-220. [Consensus record](https://consensus.app/papers/details/9a1e1deca68558b6afb96a5fba6d7ea1/) — **abstract not retrievable in this pass; the human minimum infective dose is therefore UNVERIFIED here.**
- [54] Erlandsen SL, et al. (1988) *Cross-species transmission of Giardia spp.: inoculation of beavers and muskrats with cysts of human, beaver, mouse, and muskrat origin.* Appl Environ Microbiol. [Consensus record](https://consensus.app/papers/details/a1371a5e20b75f9aaab131b0695f1e3f/)
- [55] Ralston KS, Solga MD, Mackey-Lawrence NM, Somlata, Bhattacharya A, Petri WA. (2014) *Trogocytosis by Entamoeba histolytica contributes to cell killing and tissue invasion.* Nature 508(7497):526-30. [DOI](https://doi.org/10.1038/nature13242)
- [56] Ralston KS. (2015) *Chew on this: amoebic trogocytosis and host cell killing by Entamoeba histolytica.* Trends Parasitol 31(9):442-52. [DOI](https://doi.org/10.1016/j.pt.2015.05.003)
- [57] Ralston KS. (2015) *Taking a bite: amoebic trogocytosis in Entamoeba histolytica and beyond.* Curr Opin Microbiol 28:26-35. [DOI](https://doi.org/10.1016/j.mib.2015.07.009)
- [58] Gilmartin AA, Ralston KS, Petri WA. (2017) *Inhibition of amebic lysosomal acidification blocks amebic trogocytosis and cell killing.* mBio 8(4). [DOI](https://doi.org/10.1128/mBio.01187-17)
- [59] Miller HW, Suleiman RL, Ralston KS. (2019) *Trogocytosis by Entamoeba histolytica mediates acquisition and display of human cell membrane proteins and evasion of lysis by human serum.* mBio 10(2). [DOI](https://doi.org/10.1128/mBio.00068-19)
- [60] Bettadapur A, Miller HW, Ralston KS. (2020) *Biting off what can be chewed: trogocytosis in health, infection, and disease.* Infect Immun 88(7). [DOI](https://doi.org/10.1128/IAI.00930-19)
- [61] Salata RA, et al. (1985) *Interaction of human leukocytes and Entamoeba histolytica. Killing of virulent amebae by the activated macrophage.* J Clin Invest. [Consensus record](https://consensus.app/papers/details/1761fe973bc553c8aea43bf70f8357d5/)
- [62] Rosales C. (2021) *Neutrophils vs. amoebas: immunity against the protozoan parasite Entamoeba histolytica.* J Leukoc Biol. [Consensus record](https://consensus.app/papers/details/0aa9295b48c6590eb55cc5f8528f982d/)
- [63] Begum S, Quach J, Chadee K. (2015) *Immune evasion mechanisms of Entamoeba histolytica: progression to disease.* Front Microbiol 6:1394. [DOI](https://doi.org/10.3389/fmicb.2015.01394)
- [64] Ben Ayed L, Sabbahi S. (2018) *Entamoeba histolytica.* Global Water Pathogen Project. [Consensus record](https://consensus.app/papers/details/4ffd91c44ed3501cb22d755b02d79eb9/)
- [65] Wesel J, et al. (2021) *Encystation of Entamoeba histolytica in axenic culture.* Microorganisms. [Consensus record](https://consensus.app/papers/details/e674bf475cb6532b95b292bbf15d5797/)
- [66] Matin MM, et al. (2023) *Entamoeba histolytica associated colitis.* Am J Gastroenterol. [Consensus record](https://consensus.app/papers/details/6a74f5512ccd5f5aa81627a2552ae50a/) — and Nhoung HK, et al. (2023) *A rare case of ameboma.* Am J Gastroenterol. [Consensus record](https://consensus.app/papers/details/0d618d6cc6d3563a8a9f40c4a44f6759/)
- [67] Desportes I, et al. (1985) *Occurrence of a new microsporidan: Enterocytozoon bieneusi n.g., n. sp., in the enterocytes of a human patient with AIDS.* J Protozool. [Consensus record](https://consensus.app/papers/details/130bf2a759b95f5c812c9bc457ee079f/)
- [68] Cali A, et al. (1993) *Septata intestinalis n. g., n. sp., an intestinal microsporidian associated with chronic diarrhea and dissemination in AIDS patients.* J Eukaryot Microbiol. [Consensus record](https://consensus.app/papers/details/16a14c9376e956cb945e2d72b4d6d392/)
- [69] Weiss LM. (2001) *Microsporidia: emerging pathogenic protists.* Acta Trop 78(2):89-102. [DOI](https://doi.org/10.1016/s0001-706x(00)00178-9)
- [70] Fayet M, et al. (2024) *New insights into Microsporidia polar tube function and invasion mechanism.* J Eukaryot Microbiol. [Consensus record](https://consensus.app/papers/details/a4cfaf036e8f5ae38d38adba4f6b8961/)
- [71] Sharma H, et al. (2024) *Ultrastructural insights into the microsporidian infection apparatus reveal the kinetics and morphological transitions of polar tube and cargo during host cell invasion.* PLoS Biol. [Consensus record](https://consensus.app/papers/details/04125fd8e6b15e64a8ea10106ca898fd/)
- [72] Wang Y, et al. (2024) *Global prevalence and risk factors of Enterocytozoon bieneusi infection in humans: a systematic review and meta-analysis.* Parasite. [Consensus record](https://consensus.app/papers/details/40be5ebf7a8a527e9d086b25cc0f8fac/)
- [73] Nourrisson C, et al. (2024) *Enterocytozoon bieneusi, a human pathogen.* Emerg Microbes Infect. [Consensus record](https://consensus.app/papers/details/11733d9939475fca8c9282589a890a30/)
- [74] Soule JB, et al. (1997) *A patient with AIDS and untreated Encephalitozoon (Septata) intestinalis microsporidiosis leading to small bowel perforation. Response to albendazole.* Arch Pathol Lab Med. [Consensus record](https://consensus.app/papers/details/c6f51efdc263504fb8b38de46dc4b34f/)
- [75] Kotková M, et al. (2013) *Latent microsporidiosis caused by Encephalitozoon cuniculi in immunocompetent hosts.* PLoS ONE. [Consensus record](https://consensus.app/papers/details/2cb857aecdef524994044269f4ad412e/)
- [76] Almería S, et al. (2019) *Cyclospora cayetanensis and cyclosporiasis: an update.* Microorganisms. [Consensus record](https://consensus.app/papers/details/05f25693233c53648caa0a02af2d4502/)
- [77] Dubey JP, et al. (2022) *Life cycle and transmission of Cyclospora cayetanensis: knowns and unknowns.* Microorganisms. [Consensus record](https://consensus.app/papers/details/7fb56ca40b6a5e0998b8e21e61720834/)
- [78] Ortega YR, et al. (1993) *Cyclospora species — a new protozoan pathogen of humans.* N Engl J Med. [Consensus record](https://consensus.app/papers/details/a1617066bf055f9c8b86cb8b7fc620ce/)
- [79] Soave R. (1996) *Cyclospora: an overview.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/9c51436cb0965c61854f6f270c358b94/)
- [80] Onstad NH, et al. (2019) *Cyclospora cayetanensis presence in the environment — a case study in the Chicago metropolitan area.* Environments. [Consensus record](https://consensus.app/papers/details/dde6ef019dd05be695e523d9c12af65d/)
- [81] Kniel KE, Shearer AEH, Cascarino JL, Wilkins GC, Jenkins MC. (2007) *High hydrostatic pressure and UV light treatment of produce contaminated with Eimeria acervulina as a Cyclospora cayetanensis surrogate.* J Food Prot 70(12):2837-42. [DOI](https://doi.org/10.4315/0362-028x-70.12.2837)
- [82] Bartelt L, et al. (2020) *Cystoisospora belli (syn. Isospora belli).* [Consensus record](https://consensus.app/papers/details/a9e56022078e5021a6a7c222a1cf099d/)
- [83] Qasim MT, et al. (2022) *Molecular identification of Cystoisospora belli in patients infected with HIV.* [Consensus record](https://consensus.app/papers/details/a97fa1818cdc57748b2864c91fec816c/)
- [84] Li H, et al. (2026) *Blastocystis sp. in human populations: pooled prevalence and subtype distribution from a systematic review and meta-analysis.* Parasitol Int. [Consensus record](https://consensus.app/papers/details/14fcfa87bfa95ad4bed871824a373a33/)
- [85] Stensvold CR, Clark CG. (2020) *Blastocystis.* Trends Parasitol. [Consensus record](https://consensus.app/papers/details/e768dbf7c4a95ece8fc97de36f4f6d5b/)
- [86] Aykur M, et al. (2024) *Blastocystis: a mysterious member of the gut microbiome.* Microorganisms. [Consensus record](https://consensus.app/papers/details/d98cc940330f5b10b20e32971a3233dd/)
- [87] Heneberg P. (2025) *Commensal, pathogen, or passenger? Rethinking the role of Blastocystis in human health.* One Health. [Consensus record](https://consensus.app/papers/details/43ab28a38ac35befacdda3df9bf7b7f5/)
- [88] Shasha D, et al. (2024) *High rates of Dientamoeba fragilis and Blastocystis species in children's stool but minor clinical significance.* J Infect. [Consensus record](https://consensus.app/papers/details/7d4bb46b6940507297034946d566b2fe/)
- [89] Shasha D, et al. (2023) *The clinical significance of Dientamoeba fragilis and Blastocystis in human stool — retrospective cohort study.* Clin Microbiol Infect. [Consensus record](https://consensus.app/papers/details/36b45be2dbdf56f581c44bd55c1a8abc/)
- [90] Hall LM, et al. (2026) *Dientamoeba fragilis: a story of contradictions.* J Med Microbiol. [Consensus record](https://consensus.app/papers/details/f486d57969e45f9aaa6d472618e8784d/)
- [91] Bartelt LA, Bolick DT, Kolling GL, et al. (2016) *Cryptosporidium priming is more effective than vaccine for protection against cryptosporidiosis in a murine protein malnutrition model.* PLoS Negl Trop Dis 10(7):e0004820. [DOI](https://doi.org/10.1371/journal.pntd.0004820)
- [92] Solaymani-Mohammadi S. (2022) *Mucosal defense against Giardia at the intestinal epithelial cell interface.* Front Immunol. [Consensus record](https://consensus.app/papers/details/7e099c71612d510f9b0c9a446122af31/)
- [93] Dogga SK, Bartošová-Sojková P, Lukeš J, Soldati-Favre D. (2015) *Phylogeny, morphology, and metabolic and invasive capabilities of epicellular fish coccidium Goussia janae.* Protist 166(6):659-76. [DOI](https://doi.org/10.1016/j.protis.2015.09.003)
- [94] Ali M, et al. (2025) *Host immune response to Cryptosporidium spp.: insights and perspectives for vaccine development.* Animals and Zoonoses. [Consensus record](https://consensus.app/papers/details/ec00eeedcc6e5ac69a46d090b8c7cbd8/)
- [95] Hatam-Nahavandi K, et al. (2025) *Global prevalence of Giardia infection in nonhuman mammalian hosts: a systematic review and meta-analysis of five million animals.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/bedeb64b75fa5f1aabe6bed3c59a0b51/)
- [96] Tawana M, et al. (2023) *A 'One Health' perspective of Africa-wide distribution and prevalence of Giardia species in humans, animals and waterbodies.* Parasitology. [Consensus record](https://consensus.app/papers/details/dddbadc3b19f55b09570ca82905eb506/)
