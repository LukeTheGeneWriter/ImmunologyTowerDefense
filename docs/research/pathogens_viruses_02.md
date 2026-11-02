# Enteric viruses, pass 02 — burden, buildable specifics, and what pass 01 left open

Research pass, 2026-11-02. Companion to `docs/research/pathogens_viruses.md`
(pass 01, 2026-10-25) and `docs/PATHOGEN_ROSTER.md`. **This pass does not
redo pass 01's shortlist.** It does the three things pass 01 left undone:
real burden numbers, the buildable specifics for the three organisms that
would actually be implemented next, and the candidates/traits pass 01 did
not cover.

Every factual claim below was checked in this session against PubMed (via
the PubMed MCP tool) or Consensus. Nothing is from memory. Where a number
could not be sourced in this pass it is marked **UNVERIFIED** rather than
filled in. Human vs model system is labelled on every claim. Inline markers
are `[n]`; full references with DOIs where available are at the bottom.

Read against `GAME_DESIGN.md` §1a (three lateral bands), §1b (adhesion /
plaque / breach), §1c (host-cell states and debris), §4a (pathogen
classes), §4b (intracellular infection, stress signal, contact
stress-sense), §4c (tower roster), §5 (knowledge ladder).

The selection rule is quoted from the roster doc and applied literally:

> **A species earns a slot by forcing a decision no existing enemy forces.**
> Not by being famous, not by being dangerous.

---

## 0. What this pass does to pass 01

| Pass 01 claim | This pass |
|---|---|
| Norovirus GII.4 antigenic replacement on "a real 2–4 year cycle" | **Corrected in period, confirmed and now quantified in magnitude.** 2–4 years is the *historical* beat (six named replacements 1995–2012); it has not held since. One variant, Sydney 2012, has dominated 2012–2023 [21][22]. The knock-back size is now sourced: **~70% of human polyclonal blockade titre lost across one 3-year replacement** [15]. |
| Astrovirus: "breach with no infection, no cell death and no inflammation" | **Split verdict.** The capsid-alone breach is real and the monolayers were *better* than the roster feared — polarised, differentiated, Transwell, apical-only [44]. But "no cell death" rests on trypan blue alone, and **"no inflammation" is refuted for replicating astrovirus**: in human intestinal enteroids the host response is IFN-dominated [47][48]. See §3. |
| Adenovirus F41 "induces MICA/MICB and holds MICB inside the cell" | **Confirmed, narrowed, and the mechanism named.** It is **MICB only** (MICA was not shown blocked), and it is **retention, not transcriptional** — expression of both went *up* [68]. One cell line, no NK killing assay, no gene assigned. See §4. |
| Sapovirus cut: "plays as norovirus with lower numbers" | **Cut stands, and the burden argument now also fails.** Newly in GBD 2023 at 18,800 deaths [1], but in a high-income post-vaccine setting its case-vs-control gap is not significant (7.0% vs 3.0%, P=.07) [7]. |
| Coxsackievirus B flagged as "the seventh virus if one is ever wanted" | **Cut on burden.** In US children <2y, enterovirus was 11% of AGE cases and 9% of healthy controls (P=.34) [7]. It is not a gut pathogen in attribution terms. Its two mechanics are now covered by organisms that *are* common — see §6. |
| Hepatitis A as the answer to antibody-resistant egress | **Confirmed and extended** [84]. But see §6.3: the equivalent trick in a *gut* virus is vesicle cloaking, and the antibody-protection half of that is unverified. |
| Rotavirus built as a lytic villus-tip enemy | **Two corrections.** Rotavirus has a substantial **non-lytic** egress route (~100% of releasable virus from human H69 cholangiocytes with no permeability change through 72 h) and is shed as **vesicle-cloaked clusters of >10 particles** [81]. See §6 and §9. |

---

## 1. Prevalence-ranked table

Ranked by burden, as the Director asked, not by mechanics. Global and
high-income are given separately because for enteric viruses they disagree
violently, and the disagreement is itself the most useful fact in this
section.

The global column is dominated by **GBD 2023** [1] (population-attributable
fractions of diarrhoeal deaths, 204 countries, first GBD to include
astrovirus and sapovirus) and the **Global Pediatric Diarrhea Surveillance
network** [2] (qPCR attributable fractions, 5,465 tested specimens, 33
hospitals, 28 LMICs, 2017–18). The high-income column leans on the US **New
Vaccine Surveillance Network** [7][8], which is the cleanest available
case-versus-healthy-control aetiology series in a vaccinated population.

| # | Virus | Global burden | High-income burden | Age skew | Vaccine effect |
|---|---|---|---|---|---|
| **1** | **Norovirus** | **10.2% PAF of all-age diarrhoeal deaths** (95% UI 2.4–17.0), second only to rotavirus [1]. ~1/5 of all diarrhoea cases and >200,000 deaths/yr in developing countries [3]. 6.5% AF of under-5 diarrhoea hospitalisations; 35,914 attributable deaths/yr [2]. **125 million** foodborne illnesses — the single leading foodborne cause worldwide [98b]. 17.7% pooled prevalence in children with gastroenteritis across 45 countries, GII.4 = 59.3% of those [9] | **The leading cause of AGE.** US children <2y, 2012: **23.0% of cases vs 6.6% of healthy controls** (P<.01) — the largest case–control gap of any virus tested [7]. US annual: **900 deaths, 109,000 hospitalisations, 465,000 ED visits, 2.3M clinic visits**, $430–740M in charges [4]. Incidence 68.9/1,000/yr, total cost $10.6bn [5]. **81.8%** of genotyped GII outbreaks in US long-term-care facilities are GII.4; 13,092 outbreaks / 416,284 cases in a decade [12] | **Bimodal.** Highest detection <1 year [9]; but >half the US economic burden falls on adults ≥45 [5] and deaths concentrate in the elderly (LTCF residents' attack rate 29.0% vs staff 10.9%; 21.6 hospitalisations and 2.3 deaths per 1,000 cases) [12] | **It is the beneficiary.** Vietnam pilot: rotavirus 41.6%→22.7%, **norovirus 8.8%→21.8%** (P<.0001) [10]. Norovirus has replaced rotavirus as the commonest cause of healthcare-associated gastroenteritis outbreaks in North America, Europe and Australasia [11] |
| **2** | **Rotavirus** (built) | **Still #1. 16.3% PAF of all-age diarrhoeal deaths** (12.0–21.5); **40.2% of under-5 diarrhoeal deaths** (32.5–48.5) [1]. Leading cause of hospitalised diarrhoea, AF 33.3% (27.7–40.3); **208,009 attributable deaths/yr** [2] | **Collapsed.** NVSN, pre→post vaccine: rotavirus share of AGE hospitalisations **49%→10%**, ED visits **49%→8%** [8]. US <2y 2012: 3.0% of cases vs 0.4% of controls [7] | Under-5, sharply. Post-vaccine the residual cases skew *older* and run a **biennial** pattern [8] | **This is the whole point.** Vaccinated GPDS sites: AF 20.8% vs 42.1% unvaccinated — ~50% lower [2]. Residual genotypes shifted G1P[8] 63%→7%, G12P[8] 17%→40% [8] |
| **3** | **Adenovirus F40/41** | **23.4% PAF of under-5 diarrhoeal deaths** (13.7–34.3) — third behind rotavirus and *Shigella* [1]. 5.5% AF of under-5 hospitalisations; **36,922 attributable deaths/yr** [2] | **Fails attribution.** US <2y: 23.0% of cases but **16.0% of healthy controls, P=.08** [7]. Brazil 2018–20: HAdV in 24.5% of single-detection AGE samples, of which F40/41 were 35.8% [76]; 2021–23: F40/41 in only 3.2% of 1,980 samples [77] | 6–24 months, little seasonality [76] | None (no vaccine). Benefits passively from rotavirus decline [10] |
| **4** | **Astrovirus** | **24,600 deaths in 2023** (6,290–49,000), newly included in GBD 2023, mainly under-5 [1]. Pooled prevalence **3.48%** in 302,423 gastroenteritis patients across 69 countries **vs 2.01% in 11,342 healthy controls, OR 2.16** (1.72–2.71) [56]; 4.2% in children with AGE, OR 2.06 [57]; 2.39% in hospitalised Asian children [58] | **Small but genuinely attributable.** US <2y: **3.0% of cases vs 0.3% of controls, P=.01** [7] — one of only three viruses in that study to reach significance | Highest <5y [56]; Japan, 67.9% of 140 positives were 1–3y [64]. Seroprevalence: HAstV-1 **73%** of US adults [62]; MLB1 reaches **100% by adulthood** with a nadir at 12–23 months [63] — i.e. essentially everyone is infected in childhood | None |
| **5** | **Sapovirus** | **18,800 deaths in 2023** (4,650–44,400), newly included [1] | **Not significantly attributable.** US <2y: 7.0% vs 3.0%, P=.07 [7] | 12–24 months [92] | **Relative share rises, absolute does not.** Finland 1.4%→5.5% pre→post rotavirus vaccine but "the absolute number did not increase" [93]; Japan 4.3%→6.1% (P=.033) [92]; Vietnam 1.9%→3.4% [10] |
| **6** | **Hepatitis A** | Not a diarrhoeal pathogen. Named by WHO FERG as one of the major causes of **foodborne death** alongside non-typhoidal *Salmonella*, *S.* Typhi and *Taenia solium* [98a] | **Outbreak-driven, in defined groups.** 4,096 cases across 17 EU/EEA countries in 12 months; among confirmed cases with data, 92% unvaccinated, **84% MSM**, 43% had travelled to Spain [88]. High-income populations are highly susceptible because sanitation removed childhood exposure [87] | Paradoxically *worse* with age — improving sanitation shifts infection to susceptible adults, where disease is more severe [87] | Vaccine exists and works; most high-income countries do not use it universally, so risk groups stay unprotected [87] |
| **7** | **CMV (colitis)** | Not community-acquired; reactivation disease | **21–34% of acute severe colitis; 33–36% of the steroid-refractory subgroup** [90]. 7.3% of an IBD cohort by PCR (23.3% of the ulcerative colitis subset) [91] | Older age is a risk factor [91] | n/a |
| **8** | **Enterovirus / parechovirus A** | UNVERIFIED as a diarrhoeal attributable fraction | **Passengers.** US <2y: enterovirus 11% cases vs 9% controls (P=.34); **parechovirus A 11% cases vs 16% controls** (P=.09 — *more* common in healthy children) [7] | n/a | n/a |
| **9** | **Aichi virus** | 1.45% global prevalence (pass 01 [31]) | UNVERIFIED | UNVERIFIED | n/a |
| **10** | **Human bocavirus / SARS-CoV-2** | No established diarrhoeal attributable fraction. Both are detectable in stool and wastewater at high rates (HBoV in 55.3% and SARS-CoV-2 in 20.6% of paediatric AGE stools in one Egyptian series) without attribution [96] | SARS-CoV-2 faecal RNA in 49.2% within a week of diagnosis, **12.7% still shedding at 4 months, 3.8% at 7 months**, correlated with GI symptoms [95] | n/a | n/a |

### The one-line reading

**Rotavirus is still the world's biggest enteric virus and is nearly gone
from the places that vaccinate.** Norovirus is the organism that is common
*everywhere*, in *every age group*, and is the one the player's grandmother
would have heard of. If the Director wants a common gut pathogen next, it
is norovirus, and it is not close.

**Adenovirus F41's #3 ranking is load-bearing on global mortality and
breaks in a high-income setting** (23% of cases, 16% of controls, not
significant [7]). It should be built for its *mechanic*, which is excellent,
and the round callout should not claim it is common where the player lives.

---

## 2. Deep dive — norovirus GII.4

### 2.1 The antigenic-replacement mechanic, quantified

This is the number pass 01 could not produce and §5 needs. It exists, in
human polyclonal sera, and it is big.

**Debbink et al. 2013, J Infect Dis** [15] compared GII.4-2012 Sydney
against the two variants it displaced, using VLP–carbohydrate (HBGA)
blockade as the surrogate neutralisation assay.

**Human polyclonal, n = 8 convalescent sera from a GII.4-2009 outbreak:**

| Target VLP | Blockade EC50 (% serum) | vs GII.4-2009 |
|---|---|---|
| GII.4-2009 New Orleans (homotypic) | **0.021%** | — |
| GII.4-2012 Sydney | **0.066%** | **3.1× more serum needed** |
| GII.4-2012 with epitope A reverted to 2009 | 0.048% | 2.3× |

The authors state it directly: **"only approximately 30% of the blockade
against GII.4-2009 is retained against GII.4-2012,"** and that **at least
11% of the decrease is attributable to epitope A changes alone** [15].

**So: ~70% loss of functional blockade titre across a single variant
replacement separated by three years.** That is the sourced size of the
knock-back.

**Mouse monoclonals, same paper** — the loss is not graded, it is a mixture
of complete and partial:

- GII.4-2006-G2 **lost the ability to block GII.4-2012 entirely**; NVB 97
  also failed to block it.
- G6 and G7 blockade EC50s rose **3.9- to 6.3-fold** (significant).
- Binding (EIA, not blockade) fell only **1.8- to 4.5-fold**, and two mAbs
  (G3, G4) **did not bind at all**.

Two independent corroborations of the ~40% figure for a single epitope:

- **Lindesmith et al. 2012, J Virol** [16]: altering epitope A alone
  **changed convalescent human serum reactivity by 40%** — epitope A is
  immunodominant, and it is also the most variable site.
- **Landivar et al. 2025, J Virol** [26]: in a novel epitope-resolved
  blockade assay on polyclonal sera, **36% of cross-blockade was
  attributable to antibodies mapping to antigenic site A**, and
  site-A cross-blocking titres fall progressively with the number of
  capsid mutations.

**And the crucial asymmetry: binding knowledge barely moves, functional
knowledge collapses.** In VLP-immunised mice, **at least half of total
antibody mass targets the conserved S (shell) domain, and fewer than 1% of
VP1-binding antibodies are blocking/neutralising** [28]. Monoclonals
against Sydney 2012 include one (30A11) that cross-reacts with *every*
tested genotype — GI, GII, GIV and GIX — mapping to a conserved shell
peptide, and is **non-neutralising** [—see 26/27 family; the specific mAb is
reported in Ford-Siltz 2024, cited as [99]].

**Design consequence:** the knock-back is not "you forgot the organism." It
is "you still recognise it perfectly and your antibodies no longer work."
That is a far better game beat than amnesia, and it is what the biology
says.

### 2.2 The period — and why "every 2–4 years" needs a caveat

Pass 01's "~2–4 year beat" is the *historical* pattern and should be
labelled as such. The named GII.4 pandemic variants [1 of the chronology is
compiled in 24 and summarised in 21/22/24a]: **Grimsby/US95-96 (1995) →
Farmington Hills (2002) → Hunter (2004) → Yerseke 2006a / Den Haag 2006b →
Apeldoorn (2007) → New Orleans (2009) → Sydney (2012)** — six to seven
replacements in 17 years, so a mean interval of roughly 2.5–3 years.

Then it stopped. **Zheng et al. 2022, Virus Evol** states plainly that
"the pattern of having new GII.4 variants every 2–4 years has been broken"
[21]: GII.4 Sydney has dominated from 2012 to the present, re-assorting its
polymerase (P31 → P16 → P16-2020) rather than replacing its capsid.
**Barclay et al. 2025, J Infect Dis** sequenced 406 VP1 genes from
CaliciNet 2011–2023: Sydney circulated throughout, and three novel GII.4
clusters have emerged since 2017 (San Francisco, Allegany, Wichita) with
**significantly different antigenicity** and major substitutions in epitopes
A–E, G and H — yet they *co-circulate* rather than sweep [22]. Chhabra et al.
2023 report a further novel variant with an amino-acid insertion at the
start of epitope A, from the US, Gabon, South Africa and the UK, 2017–2022
[25].

One more real fact worth building: **Eden et al. 2013** found pre-epidemic
ancestral variants of both New Orleans 2009 and Sydney 2012 circulating
**up to two years before their pandemic emergence** [24].

**Design recommendation.** Do not implement a fixed timer. Implement:

- A **stochastic replacement event with mean ~3 years of in-game time and a
  long right tail** (the biology has both a 2-year gap and an 11-year gap in
  the same dataset).
- A **two-year tell**: the variant is detectable before it sweeps. The HUD
  can surface this as a warning on the norovirus knowledge strip, and it is
  not a gameplay kindness invented for fairness — it is Eden 2013 [24].
- **Co-circulation, not replacement, as the late-game version**: after the
  first sweep, further variants arrive alongside the incumbent [22]. That is
  a mixed-species round built from one species.

### 2.3 What is NOT lost — and this is the part the ladder must respect

**Pattekar et al. 2021, CMGH** screened human PBMCs with an overlapping
norovirus peptide library and defined **7 HLA-restricted immunodominant
class I epitopes that were "highly conserved across pandemic strains from
genogroup II.4"** [29]. Norovirus-specific CD8⁺ T cells with central,
effector and tissue-resident memory phenotypes were present at all sites
and **most abundant in the intestinal lamina propria**. In mice immunised
with VLPs, T-cell epitopes mapped to the S domain, P1 and P2, and a
nine-residue P2 sequence behaved as a **universal genogroup-II CD8 epitope**
[28].

**So the antigenic replacement attacks antibody and leaves the CTL blind
spot alone.** Combined with pass 01's finding that in humans T cells are what
actually clear norovirus (chronic cases cleared after HSCT reconstituted
polyfunctional CD4 *and* CD8 responses with **no B-cell reconstitution and no
new antibody**, pass 01 [66]), the design writes itself:

> **A GII.4 antigenic-replacement event multiplies that species' knowledge
> by ~0.3, with a hard floor at the ~10% cytotoxic-T-cell rung.**

Every term in that sentence is sourced: the 0.3 is Debbink's "approximately
30% of the blockade retained" [15]; the floor at the CTL rung is Pattekar's
conserved class-I epitopes [29]; and the fact that it is the *antibody* rungs
(~20% neutralising, ~45% Fc, ~70% sIgA) that go dark is the direct reading
of a blockade assay.

The game's ladder is a single percentage that only rises. This is the one
organism that knocks it back — **and it must not knock it below 10%**, or
the game will be telling the player something false.

### 2.4 Secretor / HBGA status as host resistance — strong, and not absolute

Pass 01 noted HBGA gating. Here is the quantification and the important
qualification.

**The strong form.** *FUT2* nonsecretors (homozygous 428G→A) are resistant.
In 53 symptomatic and 62 asymptomatic individuals from Swedish nosocomial
and sporadic GGII outbreaks, **the homozygous nonsense genotype segregated
with complete resistance to disease**: 0 of 53 symptomatics were
nonsecretors, against **20% nonsecretors among 104 Swedish blood donors**
(P < 0.0002) and 29% among asymptomatics (P < 0.00001) [30]. Saliva from
secretor-positive symptomatic patients bound the outbreak strain; saliva
from nonsecretors did not [30]. The review position is that nonsecretors
are resistant to several genotypes **including the predominant GII.4** [31],
and nonsecretors have measurably narrower strain susceptibility [32].

**The qualification, which matters for the game.** **Costantini et al. 2015,
CID** prospectively enrolled 43 US long-term-care facilities and found
**symptomatic infections with GII.4 Den Haag and GII.4 New Orleans among
nonsecretor individuals** [14]. Viral load in GII.4 Sydney outbreaks was
significantly *higher* than in other genotypes, and the authors conclude
that "viral load was highest in GII.4 viruses that most recently emerged;
these viruses also infect the nonsecretor population" [14].

**So host resistance erodes along the same axis as antibody resistance.**
That is a genuinely elegant thing to build: the §1d host-cell upgrade
"reduced viral entry" is the natural home for secretor status, and the same
antigenic-replacement event that knocks back knowledge should also
**partially defeat the host-cell upgrade** — because that is what the newest
GII.4 variants demonstrably do.

Honest limit: Thorven and Costantini are both human, both observational,
and they disagree in degree. Do **not** build nonsecretor status as
immunity — build it as a large probability reduction that the newest variant
partly bypasses.

### 2.5 Shedding and dose — pass 01's UNVERIFIED cells, now closed

| Quantity | Value | System |
|---|---|---|
| **Shedding onset** | **18 hours** post-inoculation by RT-PCR | Human challenge, Norwalk GI.1, n=16 [33] |
| **Shedding duration** | **median 28 days** (range **13–56**) by RT-PCR; by antigen ELISA median 7 days | Human challenge [33] |
| **Peak faecal titre** | **median 95 × 10⁹** genome copies/g (range **0.5–1,640 × 10⁹**) | Human challenge [33] |
| **Incubation to symptoms** | **0.8–1.5 days**, dose-dependent: 1.5 d at 4.8 RT-PCR units, 0.8 d at 4,800 | Human challenge, Bayesian reanalysis [34] |
| **Time to peak shedding** | **1.5–2.3 days**, dose-dependent | as above [34] |
| **Total virus shed per infection** | **4.5 × 10¹¹ to 3.4 × 10¹²** genome equivalents in faeces | as above [34] |
| **Total virus shed in vomit** | **6.4 × 10⁵ to 3.0 × 10⁷** genome equivalents | as above [34] |
| **Prolonged shedders** | **47% of symptomatic cases shed for ≥21 days**; cases and controls shed similar amounts | Human, 10 LTCF outbreaks [14] |
| **Clinical course** | GII.4 illness **33 h** vs non-GII.4 **24 h** (P<0.001) | Human, 163 LTCF outbreaks [13] |
| Children, natural infection | Load rises days 2–9, plateaus without a peak, declines after day 9, mostly ceased by day 15; GII.4 Sydney highest mean load | Human, 77 hospitalised children [35] |

The dose figures from pass 01 stand (Norwalk HID50 1,320–2,800 gEq;
GII.2 Snow Mountain 5.1 × 10⁵ GEC; GII.4 Cin-2 2,400–3,400 RNA copies in
gnotobiotic pigs). **Note that Ge 2023 infected volunteers at 4.8 RT-PCR
units** [34] — four logs below the GI.1 HID50 — so the honest game statement
is still "the lowest practical dose of anything on the roster."

### 2.6 Deliverable block — norovirus GII.4

**1. The one mechanic.** Takes a host-cell slot (`Healthy → Infected`,
§1c); replicates in **enterocytes and enteroendocrine cells** (human
enteroids and human in-vivo biopsy, pass 01 [58][60]); **lytic** — pass 01
found no non-lytic exit for human norovirus in enterocytes, though
vesicle-cloaked clusters do appear in patient stool (§6.1) [81]; emits a
stress signal (there is no evidence of MHC-I loss sufficient to blind the
CTL — murine NS3 downregulation is partial and mouse-specific, pass 01
[63]); leaves debris. Band: arrives in the **lumen**, dominates §1b's
adhesion/plaque loop because it never stops arriving, and infects at the
interface. **Its real mechanic is not spread — it is §5.** It is a
**knowledge-decay enemy**: a periodic event that multiplies its own
knowledge by 0.3 with a floor at the 10% rung, and simultaneously degrades
the §1d "reduced viral entry" host upgrade (§2.4). Pair with **never-ending
lumen pressure**: 47% of cases shed ≥21 days [14], asymptomatic shedding at
symptomatic levels [14], and 28 days median shedding at 10¹¹ genome
equivalents [33][34].

**2. Morphology for a sprite.** 38.0 nm, T=3 icosahedral, **90 dimers** of
VP1, arch-like capsomeres at the local and strict two-fold axes, and
**large hollows at the icosahedral 5-fold and 3-fold axes** — that is
12 + 20 = **32 hollows**, which is exactly where the textbook "32
cup-shaped depressions" comes from [41]. The atomic GII.4 structure adds
that the protruding domain **rests on** the shell in the native state and
**rises** on divalent-ion chelation, increasing capsid diameter [42]; in
murine norovirus cryo-EM the protruding domains are independently flexible
while the shell is rigid [—noted in the same literature family].

By negative-stain EM — which is how a cartoon should read it —
**Madeley 1979** is the definitive side-by-side: a calicivirus has a
**feathery edge**, a six-pointed star with a **dark hollow in the centre**
("Star-of-David" appearance), and **round or oval** surface hollows [43].

**What distinguishes it from the generic corona ball:** nothing radiates.
Norovirus has **no spokes at all**. Draw a 20-px disc with a **ragged,
feathery rim** (not a clean circle, not knobs) and **~6 dark subtractive
pits** arranged around a darker centre. The generic virus sprite is
additive (knobs sticking out); norovirus is **subtractive** (pits going in).
That is a legible, buildable, sourced distinction at 64×64 using `FillDisc`
plus small negative discs, and the ragged rim is `FillLobed` at high lobe
count and low depth.

**3. Exposure route / vehicle.** Three, and all three are vivid:

- **A vomiting incident in a restaurant.** One diner vomited at a hotel
  meal; attack rates by table were **inversely related to distance from the
  vomiter**, and nobody in a separate restaurant fell ill [36]. (Honest
  caveat: a 2017 multi-agent reanalysis showed a **fomite** route via
  waiters' serving pathways fits the same attack-rate distribution equally
  well [37]. Use the incident, not the airborne claim.)
- **Raw oysters.** Oysters are not passive filters — they **selectively
  bioaccumulate** norovirus strains using carbohydrate ligands shared with
  humans, which is why shellfish outbreaks are disproportionately GI [40]. A
  2022 multi-jurisdictional oyster outbreak involving hundreds of illnesses
  carried four co-circulating genotypes, consistent with wastewater
  contamination [39].
- **A cruise ship.** 45 outbreaks on 26 ships, 1990–2020; weighted mean
  attack rate **7% of passengers and 2% of crew**; person-to-person was the
  mode in 35 of 45; the strongest risk factor was **having an ill cabin
  mate, OR 38.70** (13.51–110.86) [38].

**4. Burden.** §1 row 1.

**5. The player's counter.** This is the interesting part: **the roster's
best answers are the ones it defeats.** §4c's B-cell/plasma-cell antibody
and §5's ~70% secretory IgA are precisely the capabilities a GII.4
replacement switches off. What *survives* is the **cytotoxic T cell** —
sourced, because the class-I epitopes are conserved [29] and because human
clearance is cellular (pass 01 [66][67]). Also surviving: the **γδ IEL**,
which is tissue-resident, unprimed and repairs the wall, so it answers the
lumen pressure without needing knowledge. **The lesson the organism teaches
is "the tower you bought at 20% and 70% is the one that goes dark; the one
you bought at 10% does not."** That is a real strategic decision and no
existing enemy forces it.

**6. Redundancy check.** Not rotavirus with different numbers. Rotavirus's
mechanic is **positional** (tip-only tropism + a toxin that reaches cells
the virus cannot). Norovirus's mechanic is **temporal and in the
knowledge layer** — it is the only enemy that attacks the player's
*progress* rather than their tissue. They do not overlap at any point.

---

## 3. Deep dive — astrovirus

### 3.1 The experiments, exactly

The roster's worry was that the capsid-alone barrier effect might only have
been shown in a non-polarised cell line. **It was not. The experiment is
better than the roster feared.** Full details from the primary paper
[44], Moser & Schultz-Cherry 2007, *J Virol*:

| | What was actually done |
|---|---|
| **Cell system** | Caco-2, human, **differentiated 15–20 days on 0.3 cm² semipermeable inserts with 1-µm pores** (i.e. polarised monolayers on a filter, with separate apical and basolateral chambers). Only monolayers that reached **TER ≥ 1,000 Ω·cm²** were used. Coverslip cultures, differentiated 5–7 days, were used for immunofluorescence only |
| **Polarity** | **Apical application decreased TER and increased flux. Basolateral application produced no significant change at any timepoint.** The effect is lumen-side only |
| **Permeability readout** | Two: **TER** (every 4–12 h, normalised to each insert's own initial reading) and **paracellular flux of 4 kDa FITC-dextran** apical→basolateral |
| **Magnitude, live virus (MOI 10)** | TER fell significantly by **24 hpi** and reached **~0% of initial by 32 hpi** — complete barrier collapse. FITC-dextran flux rose significantly by 36 hpi (P = 0.0061); no numeric flux magnitude reported |
| **Dose–response** | Yes. MOI 1, 5, 10. MOI 1 produced a comparable drop ~8 h later (32 hpi) and **did not reach zero** |
| **UV-inactivated virus** | ~10⁶ U in 100 µl, UV cross-linker; **complete inactivation first achieved at 100 mJ/cm²**, defined as no capsid expression at 24 hpi and confirmed by serial passage with capsid staining. TER kinetics described as **"nearly identical"** to infectious virus over 0–24 hpi. **The final depth of the drop is not reported** |
| **Recombinant capsid VLPs** | HAstV-1 ORF2 in baculovirus transfer vector 1392, expressed in **Sf9 insect cells** (MOI 5, 4 days), size-exclusion purified, VLPs confirmed by EM. **35 µg protein, trypsin-pretreated (10 µg/ml), applied apically.** The authors state they could not relate that mass to a particle number. TER decreased by **20 hpi**, kinetics "similar" to infectious virus; **depth not reported** |
| **Occludin disruption** | Shown for live infection **and for UV-inactivated virus** (comparable to live at 24 hpi). **Not examined for VLPs** — the VLP arm measured TER only |
| **Cell death** | **Trypan blue exclusion only**, at 4/12/24/36 hpi; no significant difference from mock. For UV-virus and VLPs the authors state no effect on cell death but **the data are not shown** |
| **Inflammatory / IFN readout** | **None. The paper measured no cytokine, no ISG, nothing.** Its "no inflammation" framing is imported from prior turkey and human histology studies |
| **Recovery** | Not a main experiment. Only at **MOI 1** did TER rebound, from **52 hpi** (data not shown). With virus pre-incubated with anti-HAstV-1 antibody TER fell only to ~60% before rebounding |

**In vivo corroboration of the capsid-alone arm, in a bird.**
**Meliopoulos et al. 2016, mBio** [45]: **oral** administration of purified
recombinant **turkey astrovirus 2** capsid protein alone caused acute
diarrhoea in turkey poults, dose- and time-dependently, with increased
intestinal barrier permeability, **no inflammation and no histological
change**, and **NHE3 redistributed from membrane to cytoplasm**. The route
is the natural one — ingestion of intact, non-infectious capsid protein
suffices. **Label: avian, not mammalian, and a different virus from the
HAstV-1 used in vitro.**

Review-level support: astrovirus infection shows few histological changes
and little inflammation, and the capsid acts as an enterotoxin disrupting
the barrier [46].

### 3.2 Verdict on "breach with no infection, no cell death and no inflammation"

**Three separate claims. They do not all survive.**

| Claim | Verdict |
|---|---|
| **Breach with no infection / no replication** | **CONFIRMED, and in a better system than the roster assumed.** Polarised, differentiated, filter-grown Caco-2 at TER ≥1,000 Ω·cm², apical-only, with both UV-killed virus (occludin disruption confirmed) and purified recombinant capsid VLPs reproducing the TER kinetics [44]. Corroborated in vivo by the oral capsid-alone turkey experiment [45]. **The roster's hedge can be removed.** |
| **No cell death** | **WEAKLY SUPPORTED.** Trypan blue exclusion is a membrane-integrity dye; it does not exclude apoptosis, and no LDH release, caspase or TUNEL assay was run. For the UV-virus and VLP arms the death data are **not shown** at all [44]. The claim is plausible — TER collapsed to zero while trypan blue stayed flat, which is itself informative — but it is one assay. Treat as **supported, not established.** |
| **No inflammation / no stress signal** | **REFUTED for replicating astrovirus. Survives only for capsid alone.** Moser 2007 measured no immune readout whatsoever [44]. In the most physiological human system available — **epithelium-only human intestinal enteroids from biopsy-derived crypts** — the host response to astrovirus infection is **"dominated by interferon-mediated innate immune responses," larger in magnitude than in transformed Caco-2**, and the endogenous IFN response curtails growth of strains from all three clades [47]. Single-cell transcriptomics of human intestinal organoids agrees: **interferon controls the infection**, and each epithelial lineage undergoes antiviral reprogramming by upregulating **distinct** ISG sets [48]. In mouse enteroid air–liquid-interface cultures and in vivo, murine astrovirus stimulates **IFN-λ** in infected cells and induces ISGs throughout the epithelium in an IFN-λ-receptor-dependent manner [49]. |

**Mandatory clade caveat.** The no-signal evidence is for **classical
HAstV-1** capsid (and turkey AstV-2); the IFN-dominated evidence is
strongest for **VA1** (the non-canonical VA/HMO clade) with IFN sensitivity
confirmed across all three clades [47], plus "human astrovirus" in organoids
[48] and murine astrovirus in mouse enteroids [49]. The two bodies of
evidence are not strictly the same virus. But they are not in tension
either, because they test different things: **replication is seen; capsid
is not.**

**Design consequence — and it is a better mechanic than the roster's.**
Do not build astrovirus as "an enemy the entire detection stack cannot
see." Build it as **two threats from one organism, only one of which is
visible**:

1. **The infected cell is visible.** It emits a stress signal, it is a
   normal §4b target, the contact stress-sense roll finds it, a CTL can kill
   it. Sourced: IFN-dominated enteroid response [47][48].
2. **The breach is invisible.** Free capsid — including the capsid of
   particles the player's own antibody already neutralised, and of UV-dead
   virions — **drives the §1b plaque charge directly, with no infection, no
   dying cell, no DAMP and nothing for the §5a DC shuttle to sample.** The
   player clears every infected cell and the wall still fails.

That is sharper than a blanket stealth enemy, and every half of it is
sourced. It also gives the organism a trait nothing on the roster has:
**damage that survives its own neutralisation.** §4c's antibody coating
(`SetCoated`) pacifies an organism; it does not disassemble a capsid, and
Moser's own antibody arm showed TER still fell to ~60% with neutralising
antibody present [44].

### 3.3 Does it produce *any* damage-associated signal? Yes — and it also switches one off

**Yes, via interferon, when it replicates** — §3.2 above.

**And here is what pass 01 missed entirely.** The **same** protein that
breaks the barrier is a complement inhibitor.

- **Bonaparte et al. 2007, J Virol** [50]: HAstV-1 virions and purified
  recombinant coat protein (CP) both suppress haemolytic complement
  activity. Depletion/add-back mapped the block to **C1, the first component
  of the classical pathway**. CP binds the **A chain of C1q** and reduces
  **C4b, iC3b and terminal C5b-9** formation. **Inhibition was also
  demonstrated for HAstV serotypes 2, 3 and 4** — a general feature of the
  species, not a quirk of one isolate. First report of a non-enveloped
  icosahedral virus inhibiting the classical pathway at C1.
- **Hair et al. 2009, Mol Immunol** [51]: mechanism. CP prevents cleavage
  of C1s to its active form even against heat-aggregated IgG; it inhibits
  **C5a** generation; it binds C1q's **globular heads and collagen-like
  region**; and adding CP to C1 **dissociates C1s from C1q** — it displaces
  the C1s-C1r-C1r-C1s protease tetramer. CP also binds **MBL** and inhibits
  mannan-mediated **lectin-pathway** activation, and does **not** bind an MBL
  variant lacking Lys55 (the MASP-2 contact). It crosses the species barrier
  to inhibit C3 activation and MAC formation in rat serum.
- **Gronemus et al. 2010, Mol Immunol** [52]: CP residues **79–139** share
  limited homology with human neutrophil defensin-1 — itself a C1q/MBL
  binder — and a 30-aa peptide from that region competitively inhibits
  wild-type CP binding to C1q.

**Why this matters for the game, and the honest caveat that comes with it.**
Complement left §5's knowledge ladder on 2026-10-25 and became a **body
upgrade in the shop**. Astrovirus is the only organism found in any of these
passes that would **switch a shop purchase off** — a new category of counter
entirely, and a very legible one ("the thing you bought stops working while
this is on the board").

**But the pathways do not line up, and this must be said in the design
doc.** The roster's complement work (Wu 2024) established that luminal gut
C3 deposition is **alternative-pathway** — unchanged in C1q- and
MBL-deficient mice. Astrovirus CP inhibits exactly the two pathways that
the gut does *not* use: **classical (at C1q) and lectin (at MBL)** [50][51].
So as written, astrovirus's complement inhibitor is aimed at the wrong
pathway for this map. Options, in order of honesty:

1. **Build it anyway, scoped to the inflamed/breached state.** The roster
   already notes MAC *does* appear in inflamed, barrier-breached tissue.
   Classical-pathway activity is plausible there, and that is also exactly
   where astrovirus has already broken the wall. Self-consistent and the
   scoping is the mechanic.
2. **Build it as an anti-C5a effect** — CP demonstrably inhibits C5a
   generation [51], and C5a is a chemoattractant, so this reads as "the
   recruitment signal gets quieter," which composes with §7/§9 rather than
   with the shop.
3. **Note it and do not build it.** Defensible; it is the weakest-sited of
   the astrovirus facts for *this* map.

All three beat inventing a mechanic.

### 3.4 Recovery time course

**Largely UNVERIFIED, and this is a real gap.** The only data points:

- At **MOI 1** (not MOI 10), TER did not reach zero and **rebounded from
  52 hpi** — "data not shown" [44].
- With virus pre-incubated with neutralising anti-HAstV-1 antibody, TER fell
  only to **~60% of initial** and then rebounded [44].
- At MOI 10, TER reached ~0% by 32 hpi and no recovery is described [44].

**Reading for the game: recovery is dose-dependent and, at a high dose
in vitro, does not happen within the observation window.** A defensible
design is a **permeability debuff that decays on its own over roughly the
time it took to accumulate** (~20 h after nadir at low dose is the one
hint), gated so that a large enough capsid load does not recover at all
within a round. Flag the number as **UNVERIFIED** in whatever tuning field
holds it.

### 3.5 Tropism — and the clean non-redundancy argument against rotavirus

This closes one of pass 01's biggest UNVERIFIED cells.

**Kolawole et al. 2019, PLoS Pathog** [47], in **epithelium-only human
intestinal enteroids derived from biopsy crypts** (human, primary,
non-transformed), from multiple patients and multiple intestinal regions:

- Strains from **all three human clades** (classic, MLB, VA/HMO) infect.
- VA1 replicates robustly **independent of the cellular differentiation
  status**.
- By flow cytometry and immunofluorescence, VA1 infects **several cell
  types, including intestinal progenitor cells and mature enterocytes**.
- Genotypic non-permissiveness of some enteroid lines was **overcome by
  pharmacologic JAK/STAT inhibition** — i.e. the restriction is interferon,
  not receptor.

Corroborating in human organoids: astrovirus **infects all major cell types
and lineages** and induces the proliferation marker MKI67 [48]. In mouse,
murine astrovirus targets **goblet cells and enterocytes**, and depleting
goblet cells from the culture limits infection [49] (murine).

**This is the inverse of the built rotavirus.** Rotavirus is villus-tip
only, never touches the Lgr5⁺ crypt stem cell, and the crypt repaves from
below — that is the whole mechanic. **Astrovirus reaches the progenitors.**
So on a board where "which end of the band can this thing reach" is already
a property the player reads (§1a, §1b), astrovirus is the organism that
**can reach the engine that rebuilds the wall**. Two viruses, opposite
geometry, both sourced, and the second one makes the first one's mechanic
retroactively more interesting.

**Caveat, stated plainly:** the progenitor-infection result is **VA1**, the
divergent clade, in human enteroids. VA1 is also the clade associated with
**meningitis and encephalitis in immunocompromised patients** [65][66][67]
and may use a different receptor [65]. The capsid-enterotoxin result is
**classical HAstV-1**. If the game wants one astrovirus it must pick which
clade's properties to carry, or carry both and say so.

### 3.6 Deliverable block — astrovirus

**1. The one mechanic.** **Two threats, one organism, and only one of them
is visible.** (a) *The infected cell*: takes a host-cell slot, emits a
stress signal, normal §4b target — and critically can occupy **progenitor
cells**, so unlike rotavirus it reaches the crypt end of the band [47].
(b) *The capsid*: a §1b **plaque-charge driver that requires no living cell,
no infection and no replication** — UV-killed virions and purified capsid
work as well as live virus [44][45]. It produces **no infected cell, no
dying cell, no DAMP and no antigen for the §5a DC shuttle**, and it keeps
working on organisms the player's antibody has already neutralised
[44]. Optional third layer: a **complement/C5a suppressor** scoped to
inflamed breached tissue (§3.3). Band: **lumen → interface**, and the capsid
half acts **apically only** [44], which is mechanically perfect — it is a
wall effect, not a tissue effect.

**2. Morphology for a sprite — and a correction to the brief.** The
iconic five-or-six-pointed star is real but it is **not** the native
infectious virion's dominant appearance.

- **Cryo-EM** (human HAstV, icosahedral reconstruction) [53]: T=3, solid
  capsid shell **~350 Å (35 nm)** diameter, globular spikes bringing overall
  diameter to **~440 Å (44 nm)**. The **immature** VP70 particle displays
  **90 dimeric spikes**; the **mature, fully proteolysed, infectious** virion
  displays only **30 spikes**, at the icosahedral two-fold axes. Loss of the
  60 peripentonal spikes is tied to infectivity. The spike's β-barrel core
  is shared with **hepatitis E virus** [53][55].
- **Negative stain** [54]: external diameter **41 nm**, a well-defined layer
  of surface spikes; pentagonal and hexagonal contours occasionally
  visible. **Star-like morphologies and triangular surface hollows were seen
  only after brief pH-10 treatment, and only in dark areas of the
  preparation.** pH 10.5 disassembled the particles. The authors
  explicitly propose an alternative morphological model to the traditional
  star.
- **Madeley 1979** [43], the side-by-side that matters for a cartoon:
  astrovirus has a **round unbroken edge**, a **six-pointed star with a
  WHITE centre**, and **triangular** surface hollows — against the
  calicivirus's feathery edge, dark-centred star and round hollows. And:
  **"only a minority of the virions of both viruses exhibit a completely
  characteristic structure."**

**What distinguishes it from the generic corona ball:** at 64×64 rendering
to ~20 px, 30 spikes on a 44 nm particle is a fuzzy rim and will read as
identical to 8 knobs. **The star is the only thing that reads.** Draw a
clean, unbroken circular rim (not lobed, not knobbed — this is the
discriminator against norovirus) with a **bright, additive six-point star,
white at the centre**, using the existing `FillStar` primitive at `points=6`
with a light fill over the body disc. Norovirus is subtractive pits on a
ragged rim; astrovirus is an additive bright star on a clean rim. **Record
in the art doc that the star is the alkaline-treated / minority EM
appearance and the cryo-EM virion is a 30-spiked ball** — the game is
choosing the icon over the structure deliberately, which is fine, but it
should be a recorded decision rather than an error.

**3. Exposure route / vehicle.** **A daycare changing table in autumn.**
Of 32 reported HAstV outbreaks 1978–2018, the commonest settings were
**nursery centres and kindergartens (31.0%)**, hospitals (17.2%) and
schools (13.8%); **43.8% occurred in autumn**; 11 of 32 were mixed
infections, most often with norovirus (81.8%) or rotavirus (45.5%), and 7 of
the 9 mixed outbreaks with a known route were **waterborne or foodborne**
[59]. A Catalan daycare outbreak gives the household numbers: global attack
rate **20%**, rising to **41.2% in children under 2**; household contacts of
**affected** attenders had an attack rate of **74.3%** versus **2.4%** for
contacts of unaffected attenders; and **asymptomatic infections were found
among the attendees** [60]. A 2025 Chongqing kindergarten outbreak
(HAstV-5) ran an attack rate of **43.2%**, presenting with fever in 100% of
cases and diarrhoea in only 42.1% [61].

That last detail is worth the callout text: **an astrovirus round should
not look like a diarrhoea round.** Fever in everyone, loose stool in fewer
than half.

**4. Burden.** §1 row 4. The honest framing for a round callout: common
enough that **73% of US adults are seropositive for HAstV-1** and MLB1
reaches **100% by adulthood** [62][63], but with a narrow case–control gap
(3.48% vs 2.01%, OR 2.16 [56]) — i.e. **everybody gets it, and it is
frequently found in healthy children too** [7][56].

**5. The player's counter.** The infected-cell half is answered by the
whole §4b/§4c stack: contact stress-sense, CTL, NK, γδ. **The capsid half
has no answer in the roster and this should be said loudly.** Antibody
coating pacifies organisms (`SetCoated`) — it does not neutralise a
free protein, and Moser's antibody arm still lost 40% of TER [44]. The
nearest real answers:

- **The γδ IEL's repair** (§4c) — the only tower that restores dead barrier
  segments, and the only thing on the board that treats the *wall* rather
  than the *pathogen*. Astrovirus is the enemy that makes that tower
  mandatory rather than nice.
- **§6b mucus turnover / sentinel goblet sloughing** — flushes the capsid
  source back into the flow before it reaches the epithelium. Non-specific,
  needs no knowledge, acts at depth 0.
- **Secretory IgA at the ~70% rung**, acting *before* adhesion — but only
  against intact organisms, not shed capsid.

So the player's genuine answer is **"defend the wall, not the pathogen,"**
which is a strategy the game has built towers for and has never had a reason
to teach. That is the strongest argument for building this organism.

**6. Redundancy check.** Not rotavirus with different numbers, on two
independent grounds. First, **opposite tropism geometry**: rotavirus cannot
reach the crypt, astrovirus infects progenitors [47]. Second, and more
importantly, **rotavirus's toxin requires an infected cell to make it
(NSP4 is a non-structural protein, synthesised only during replication),
while astrovirus's toxin is the capsid itself and arrives pre-made in the
lumen.** Rotavirus's damage radiates *outward from* an infection you can
see; astrovirus's damage has no infection behind it at all. Also worth
recording: rotavirus *does* disrupt occludin in polarised Caco-2 with no
change in TEER or LDH release, and does so independently of its F-actin
rearrangements [97] — so tight-junction disruption is **not** unique to
astrovirus. What is unique is that it happens **without replication** [44].

---

## 4. Deep dive — adenovirus F41

### 4.1 E3-19K absence — confirmed, and the game should keep pass 01's correction

Pass 01's correction stands: the famous adenoviral CTL-evasion protein
E3-19K is **absent from species F**, the E3 region of F40/41 is "markedly
different from that of all other HAdV species," and the two unique species-F
E3 genes remain **uncharacterised** [68][72]. Do not build Ad41 as an MHC-I
stealth virus.

### 4.2 MICA or MICB? Retention or transcriptional? — both questions answered

**Oliveira et al. 2021, *Viruses*** [68], the single paper the claim rests
on. HAdV-F41 infection of **HCT116** cells (human, colorectal carcinoma
line):

| Question | Answer |
|---|---|
| **MICA, MICB, or both?** | **Both are upregulated in expression. Only MICB is held back from the surface.** The authors state the MICB response "did not however result in a significant increase of MIC B on the cell surface. Instead, MIC B was largely sequestered intracellularly," and that **"a similar observation could not be made for MIC A in these cells."** So: **MICB only** for the evasion. The roster's "induces MICA/MICB and then holds MICB inside" is correct as written; any shorthand of "sequesters MICA/B" is not. |
| **Retention or transcriptional?** | **Retention.** Expression of both ligands **went up** relative to uninfected cells; the block is on surface delivery, not on transcription. This is the opposite of subgenus A (Ad12), where **E1A represses MHC-I heavy chain, TAP and LMP transcription** (pass 01 [23]). Species F is a trafficking mechanism, species A is a transcriptional one, and the game should not conflate them. |
| **How strong is the evidence?** | **Weak, and the authors say so.** One cell line. The authors call the findings "preliminary." **No NK-cell killing assay** was performed — the NK blindness is inferred from surface ligand levels, not measured as cytotoxicity. **No gene was assigned**: the paper's closing move is that this "paves the way for the future investigation of their unique E3 proteins." |

**The convergence that makes it a better story than "the mirror of CMV."**
Non-enteric adenoviruses achieve **exactly this phenotype** via E3/19K:
wild-type Ad enhances MICA and MICB synthesis while actively suppressing
their surface expression, retaining **both** in the ER as immature
endo-H-sensitive forms; mapping to the E3 region identified **E3/19K** as
the responsible gene; the structural requirements for sequestering MICA/B
and HLA-I are similar; and **deleting E3/19K rendered infected cells more
sensitive to NK recognition** in functional assays [69].

So **species F reaches the same NK-evasion endpoint without the gene that
does it in every other species** — and, because it lacks E3/19K, it pays
the CTL-visibility price that the others do not. That is a genuinely
interesting convergence and it is the honest version of "CTL-visible,
NK-blind."

### 4.3 What this licenses, and what it does not

**Licensed:** F41 as an enemy that **is** a normal CTL target (no MHC-I
evasion gene) but whose **NKG2D stress display is partially suppressed** —
one of two ligands held intracellularly, in one human carcinoma line, with
no killing assay.

**Not licensed:** a **cliff**. §4c's standing rule is that MHC-I stealth
must be a cliff because partial MHC-I reduction does nothing in reality.
The mirror argument does **not** transfer: NKG2D signalling is a summed,
graded input over multiple ligands (MICA, MICB, ULBP1–6), and only MICB was
shown blocked here. **Build F41's NK blindness as a partial reduction, not
an immunity**, and label the tuning field with the fact that it rests on one
preliminary paper.

Also carried over from pass 01 and still the best-evidenced F41 property:
both prototype and clinical HAdVs replicate productively in human enteroids;
**HAdV-41 is sensitive to type I and type III interferon in enteroid
monolayers but not in A549 cells**; and **HAdV-5 is potently neutralised by
the Paneth-cell α-defensin HD5 whereas HAdV-41 is not** (pass 01 [26]). The
HD5 result is the one that should drive the game: **it walks past the
crypt's chemical defence.**

### 4.4 Deliverable block — adenovirus F41

**1. The one mechanic.** Takes a host-cell slot; **lytic** (adenoviruses are
the archetypal lytic DNA virus; no non-lytic route reported for species F in
this pass — **UNVERIFIED**); emits a stress signal that is **partially
muffled** (MICB retained intracellularly, MICA unaffected [68]); leaves
debris. Band: lumen → interface → tissue. Spread: contact-chain and/or
budding — **UNVERIFIED for species F**. **The decision it forces:** it is the
only enemy that inverts the usual tower advice. Against most of the roster
the stress sensors (NK, γδ) are the cheap, ungated answer and the CTL is the
expensive gated one. Against F41 **the gated tower is the better one**, and
the ungated stress sensors are degraded. Paired with a built CMV later, the
two teach the same lesson from opposite ends. Second mechanic, better
sourced than the first: **it is not neutralised by the Paneth-cell
α-defensin HD5** (pass 01 [26]) — so if the §1d crypt upgrade ever carries an
antimicrobial-peptide effect, this is the organism that ignores it.

**2. Morphology for a sprite — the best silhouette on the roster.**
~90 nm icosahedron, by far the largest virion here (~2.4× norovirus,
~2× astrovirus). Cryo-EM structures of F41 exist at **3.8 Å** [70] and
**4 Å** [71]: hexon, penton base and the internal minor coat proteins are
conserved, but the **external minor coat protein IX is arranged unlike any
previously characterised human or non-human mastadenovirus**, the hexon HVR1
sequence and arrangement differ, and there are **partially ordered elements
reinforcing the vertex region**, read as the structural basis for surviving
the gut [70][71][72].

And the detail that makes the sprite: **F40 and F41 are the only human
adenoviruses that carry and express two fibre genes**, giving a **long** and
a **short** fibre [72][73][74]. Measured by EM on CsCl-purified Ad41
virions: the **long fibre is 315 Å** with 22 sixteen-residue shaft repeats,
the **short fibre is 250 Å** with 12 — both with the characteristic terminal
knob — and **only one fibre is incorporated per penton base** [73][74]. So a
single virion carries, across its 12 vertices, a mixture of long and short
antennae of two distinct lengths.

**What distinguishes it from the generic corona ball:** everything. A
**hard-edged hexagonal silhouette** (the icosahedron's projection) instead of
a disc, **noticeably larger** than every other virus sprite, and **12 long
spikes of two visibly different lengths each ending in a dot**, instead of 8
even knobs. Buildable from `FillRounded`/a polygon fill plus 12 `FillCapsule`
calls at two lengths plus 12 small `FillDisc` knobs. At 20 px the read is
"big angular thing with uneven antennae" — unmistakable, and it is the only
thing on the roster whose distinguishing feature is **unique among all human
viruses of its family**, which is a nice piece of trivia for a codex entry.

**3. Exposure route / vehicle.** A **contaminated swimming pool or splash
pad**, or a **paediatric ward** — and the best real hook available:
**the 2022 hepatitis of unknown origin**. By 8 July 2022, **1,010 probable
cases across 35 countries**, more than half in Europe, mostly in children
under 6, with common hepatitis viruses A–E excluded, and **adenovirus 41
detected in most patients** [80]. In the Alabama case series, 8 of 9
children with hepatitis of unknown cause had adenovirus viraemia; sequencing
showed **three distinct HAdV-41 hexon variants** (so not a single-strain
outbreak); liver immunohistochemistry and EM were **negative** for
adenovirus; PCR of liver tissue was positive in 3 of 6; **two children
required liver transplantation** [78]. A separate pathology series found
HAdV-F41 by PCR in liver tissue in **10 of 11** cases, with
immunohistochemistry negative in all, and a histology resembling severe
autoimmune hepatitis — concluding the injury "may represent an
immune-mediated process associated with viral infection" [79]. Causality
remains unproven [80].

**That last point is the most interesting thing available for a round
callout, and it is honest**: the virus was in the gut and the blood, the
damage was in the liver, the virus could not be found in the damaged tissue,
and the damage looked like the immune system. **Destruction aimed past the
wall, delivered by the player's own response.** EHEC already holds the
"destruction aimed past the wall" slot in the roster; this would be the
viral version, and it composes with A/E *E. coli*'s
inflammation-as-fuel lesson rather than repeating it. **Flag clearly that
causality is unproven** — the game should not assert it, but a codex entry
that says "nobody knows" is better content than one that lies.

**4. Burden.** §1 row 3. Use the global number (23.4% PAF of under-5
diarrhoeal deaths [1]; 36,922 attributable deaths [2]) and **do not claim
high-income commonness** — 23.0% of cases vs 16.0% of healthy controls,
P=.08 [7].

**5. The player's counter.** The **cytotoxic T cell at the ~10% rung** —
and it is the only tower that works well. The NK/ILC1 and γδ IEL
stress-sensing route is degraded (partially, per §4.3). Antibody cannot
reach inside a host cell (§4c). **This is the enemy that justifies the 10%
rung existing at all**, and it should arrive after the player has had a
chance to lean on ungated stress sensors and get comfortable.

**6. Redundancy check.** Not rotavirus with different numbers: rotavirus's
mechanic is positional tropism plus a toxin, and its MHC-I/stress profile is
unremarkable. Not CMV with different numbers either — CMV is a **cliff** on
the CTL axis and a slow boss already behind the wall; F41 is **CTL-soluble
and stress-sensor-degraded**, is epithelial, and arrives from the lumen like
anything else. The pair inverts; it does not repeat.

---

## 5. New candidates — the ones pass 01 did not cover

### 5.1 Sapovirus — **cut stands, and now on burden too**

Pass 01 cut it as "norovirus with lower numbers." That was a mechanics
judgement; this pass adds the burden judgement, and it agrees.

**Newly included in GBD 2023 at 18,800 deaths** (4,650–44,400), mainly
under-5 [1] — real, and about three-quarters of astrovirus's. In
high-income post-vaccine settings its *relative* share rises and its
*absolute* share does not: Finland 1.4% → 5.5% of paediatric AGE across the
rotavirus-vaccine introduction, with the authors stating explicitly that
**"the absolute number did not increase"** [93]; Japan 4.3% → 6.1%
(P = 0.033) across 5,697 samples, with the increase attributed mainly to
**co-infections** [92]; Vietnam 1.9% → 3.4% [10]. In US children <2y it does
not reach significance against healthy controls (7.0% vs 3.0%, P=.07) [7].
Age skew 12–24 months [92].

**Verdict: cut.** It is a calicivirus with the same shape, a smaller
burden, and no reported mechanic that norovirus does not already bring. It
is also the organism whose EM appearance *is* the textbook 32-cup
calicivirus (Madeley's "Star-of-David") [43] — so if anything, **sapovirus's
contribution to this project is its morphology, which the norovirus sprite
should borrow.**

### 5.2 Enterovirus / coxsackievirus B / parechovirus A — **cut on burden**

Pass 01 held coxsackievirus B back and flagged it as "the one to revisit."
**This pass cuts it.**

In the cleanest available case–control aetiology series, US children <2y:
**enterovirus 11% of AGE cases vs 9% of healthy controls (P=.34)**, and
**parechovirus A 11% of cases vs 16% of controls (P=.09)** — i.e.
parechovirus was *more* common in healthy children [7]. These are gut
passengers. Building one as a gut enemy would be the "famous, not relevant"
failure mode the roster's organising rule exists to prevent.

Its two good mechanics are **not lost**, because organisms that *are* common
now carry them:

- **Non-lytic egress** → rotavirus and norovirus, with better and more
  directly gut-relevant evidence (§6.2) [81].
- **Picornaviral 3A blocking ER→Golgi traffic**, cutting IFN-β/IL-6/IL-8
  secretion *and* new MHC-I delivery (pass 01 [80][81]) → genuinely
  unreplaced. But pass 01 already recorded that this 3A activity is
  **not general** — effective for poliovirus and CVB3, **not** for HAV,
  EV-71, rhinovirus 14, FMDV or Theiler's virus. It is a poliovirus-family
  trick, and poliovirus is not on this map.

**Verdict: cut, and remove it from the "revisit later" list.** If the
"silence the recruitment cytokine at source" mechanic is wanted, it needs a
different organism, and the roster should record that it currently has none.

### 5.3 Aichi virus — **cut stands**

1.45% global prevalence (pass 01 [31]). Detected in 59.4% of raw sewage and
62.6% of paediatric AGE stools in one Egyptian series [96], but with **no
attributable fraction** — and the same series detected it at high rates in
treated sewage, which is an environmental-persistence observation, not an
aetiological one. **No attributable burden sourced in this pass. Cut.**

### 5.4 Human bocavirus — **cut, new**

Detected in 55.3% of paediatric AGE stools and 45.8% of raw sewage in the
same series [96]. **No attributable diarrhoeal fraction found in this
pass — UNVERIFIED.** Not tested in the NVSN case–control panel [7]. A virus
with a 55% stool detection rate and no established attribution is the
textbook passenger. **Cut.**

### 5.5 SARS-CoV-2 enteric involvement — **cut for this map, but one fact is worth banking**

Real GI involvement: a notable proportion of COVID-19 patients develop GI
symptoms, roughly half have detectable faecal viral RNA, and there is
in vitro and in vivo animal evidence of direct intestinal infection [94]. In
113 people with mild-to-moderate disease, faecal RNA was detected in
**49.2%** within the first week, **12.7% at 4 months and 3.8% at 7 months**,
while oropharyngeal shedding had ceased by 4 months — and faecal shedding
**correlated with GI symptoms** (abdominal pain, nausea, vomiting) [95].

**Cut as an enemy**: no diarrhoeal attributable fraction, and it would read
as a topical gimmick on a small-intestine map. **But bank the trait**: a
pathogen that **persists in the gut long after it has been cleared from its
primary site** is a shape the roster does not have, and it is the honest
seed of a "chronic reservoir" mechanic if one is ever wanted. The better
organism for that slot is already on the shortlist — **chronic norovirus in
the immunocompromised** (pass 01 [68]).

### 5.6 Astrovirus VA1 — **noted, not a separate slot**

Worth recording because it is the clade most of the enteroid work used. The
VA/HMO clade is "the main clade associated with meningitis and encephalitis
in immunocompromised patients" [65]; two paediatric oncology cases were
diagnosed by metagenomic sequencing of CSF [66]; and in three fatal human
cases **neurons were the principal target**, with the highest burden in
cerebellum and brainstem, apparent **trans-synaptic** spread between
interconnected structures, and a host response in which the **innate but not
the adaptive** arm drove defence [67]. Its capsid spike is strikingly
divergent and a competition assay supports a **different receptor** [65].

**Not a slot.** The brain is not on the board. But it is the reason the
astrovirus entry must state which clade it is modelling.

---

## 6. The three roster gaps, answered

The brief asked specifically whether any enteric virus has a trait the
roster has no answer to: **bulk transmission**, **non-lytic exit**, or
**antibody-resistant egress**. All three exist. Two of them belong to an
organism that is **already built**, which makes them corrections as much as
additions.

### 6.1 Bulk transmission — **SOLVED, with numbers, and it belongs to rotavirus and norovirus**

**Santiana et al. 2018, Cell Host & Microbe** [81] is the paper. Rotavirus
and norovirus are shed in stool not only as free particles but as **clusters
of virions enclosed in host-derived extracellular vesicles** (exosomal or
plasma-membrane origin) that survive faecal–oral transit and deliver
**multiple genomes to one cell as a single unit**.

Systems, labelled:

- **Human:** H69 cholangiocytes and Caco-2 (rotavirus SA11 vesicles);
  **stool from paediatric norovirus patients**, assayed on **human
  intestinal enteroids**. Human norovirus was **not** tested in vivo.
- **Murine:** BALB/c suckling pups with murine EDIM rotavirus; MNV-1 in
  RAW264.7 mouse macrophages.
- **Porcine:** gnotobiotic piglets infected with the **human Wa** rotavirus
  strain — a human strain in a non-human host.
- **Monkey:** MA104 cells with SA11.

The numbers:

| Measure | Value |
|---|---|
| **Virions per vesicle, rotavirus stool** (negative-stain EM, n=25) | **>60% of vesicles carried more than 10 particles** |
| **Virions per vesicle, human norovirus stool exosomes** (n=25) | ~90% were <200 nm and carried **1–5 particles** |
| **Virions per vesicle, MNV-1 exosomes** | up to 5 |
| **Vesicle-cloaked share of total stool virus** (pig and mouse rotavirus) | **10–45%**; two pig stools at 35% and 42% |
| **Infectivity lost by removing vesicles** | **72% and 83%** in those two stools |
| **Disease severity** | Mouse pups: the **1× vesicle** group and the **5× free virus** group both had severe diarrhoea within 24 h; the **1× free virus** group only began at day 2. Vesicle-fed pups had significantly higher intestinal viral levels and more infected enterocytes at days 1 and 3. **No exact counts or p-values given in the text** |
| **Environmental persistence** [82] | Murine rotavirus vesicles kept integrity and infectivity after ≥7 days in filtered fresh- and wastewater; **24.5–27.5% still intact at 16 weeks**. **Free chlorine at 13.3 mg·min/L did not decompose them**, and was far less effective at inactivating vesicle-enclosed than free rotavirus (murine) |

**So bulk transmission is real, quantified, and it changes the built
rotavirus.** The mechanic in §1b terms is exact and lovely: **a vesicle is
one adherent that ruptures into ten infections.** The game already has the
vocabulary — §1b step 2's plaque is "one event releasing everything at
once"; a vesicle is the same idea at single-organism scale. And it has a
direct consequence the player can read: **the §1b charge floor (the
minimum-infectious-dose knob, which makes a trickle "arithmetically
impossible") is exactly what a vesicle defeats.** One vesicle arriving at the
wall carries the dose a crowd would have had to assemble.

**What it does NOT do, and this must be stated:** the paper **did not test
neutralising antibody, detergent or trypsin** on vesicle-cloaked versus free
virus. The authors *propose* that the membrane may protect against stool
proteases and mucosal antibodies; **that is explicitly speculation** [81].
Supporting but circumstantial: free stool rotavirus showed degraded
VP5-reactive bands that vesicle-enclosed virus did not. **So "vesicles are
antibody-proof" is UNVERIFIED and must not be built as fact.** Build the
bulk-delivery half, which is measured; leave the antibody half out.

### 6.2 Non-lytic exit that leaves the host cell alive — **SOLVED, same paper**

From the same work [81]:

- In **human H69 cholangiocytes**, plasma-membrane permeability to trypan
  blue showed **no significant change through 72 hours** while **~100% of
  the releasable rotavirus was released**.
- In **RAW264.7 mouse macrophages**, ~100% of the releasable MNV-1 pool
  exited non-lytically.
- In **MA104 monkey kidney cells**, a fraction was released before lysis.

Evidence: membrane-permeability assays, ELISA of released virus,
phosphatidylserine-based vesicle pull-downs, EM confirmation.

**So an enteric virus can empty a host cell without killing it.** In §1c/§4b
terms this is a **host-cell state the game does not have**: a cell that is
`Infected`, is producing and exporting virions, and will not proceed to
`Dead`, so it never becomes debris, never emits a necrotic DAMP, and never
feeds the §5a DC shuttle. It is the viral exact analogue of what *Vibrio
cholerae* already does to the wall — **alive, still occupying its slot, and
no longer the player's** — which is the strongest possible argument that the
shape is good, because the roster already proved that shape reads.

**Caveat on magnitude:** H69 is a human **cholangiocyte** line, not an
enterocyte, and MA104 (monkey) *did* lyse. So "non-lytic" is
cell-type-dependent, and the honest game implementation is a **fraction** of
infections that take the non-lytic route, not a per-species switch.

### 6.3 Antibody-resistant egress — **confirmed for hepatitis A, and still only hepatitis A**

**Das et al. 2023, Nat Rev Microbiol** [84] is the current review position,
and it is unambiguous: hepatitis A and hepatitis E are released
**non-lytically** from hepatocytes as **quasi-enveloped virions cloaked in
host membranes**; these virion types **predominate in the blood** of infected
people; they **lack virally encoded proteins on their surface** and are
therefore **resistant to the neutralising anti-capsid antibodies induced by
infection**; and they nonetheless enter cells efficiently and initiate new
replication. Release is **ESCRT-dependent, through multivesicular
endosomes**, driven by specific capsid peptide sequences. Pass 01's primary
source [85] and the contemporaneous commentary [86] stand.

**Still cut as an enemy, for the reason pass 01 gave** — a mean 28-day
incubation and a target organ that is not on the board. The high-income
exposure vehicle is now well sourced if it is ever built: **4,096 cases
across 17 EU/EEA countries in 12 months, 84% MSM, 92% unvaccinated, 43% with
travel to Spain** [88], and a 2022 Hungarian outbreak that combined sexual
transmission with a **restaurant cluster implicating frozen berries** [89].

**The honest conclusion on this gap: the roster's "antibody-resistant
egress" hole is filled only by an organism it has already cut on anatomy.**
The gut-virus candidate (vesicle cloaking, §6.1) delivers the *bulk*
mechanic but **not** the antibody-resistance mechanic, because that half was
never tested [81]. **The gap should stay open in `PATHOGEN_ROSTER.md`, with
a note that the near-miss is vesicle cloaking and that closing it needs an
experiment nobody has run.**

**Quantities still UNVERIFIED:** number of capsids per eHAV vesicle (pass
01 and this pass both failed to source it); HAV ID50; HAV burst size;
species-F burst size and eclipse.

---

## 7. Closed from pass 01's UNVERIFIED list

| Pass 01 cell | Now |
|---|---|
| Norovirus **incubation** ("UNVERIFIED in this pass; commonly 12–48 h") | **CLOSED. 0.8–1.5 days to symptom onset, dose-dependent** — 1.5 d at 4.8 RT-PCR units, 0.8 d at 4,800. Human challenge, Bayesian mixed-effects reanalysis [34] |
| Norovirus **shedding duration / peak titre** (cited at second hand via Teunis/Wu) | **CLOSED with a primary human challenge source. Onset 18 h; median 28 days (13–56); median peak 95 × 10⁹ copies/g (0.5–1,640 × 10⁹)** [33]. Plus 47% of symptomatic LTCF cases shed ≥21 days, with cases and controls shedding similar amounts [14] |
| Norovirus **total virus shed** | **CLOSED. 4.5 × 10¹¹ – 3.4 × 10¹² GEC in faeces; 6.4 × 10⁵ – 3.0 × 10⁷ in vomit** [34] |
| Astrovirus **tropism** (UNVERIFIED) | **CLOSED for the VA clade in human enteroids: intestinal progenitor cells and mature enterocytes, independent of differentiation status; all three human clades infect** [47]. Human organoids: all major cell types and lineages [48]. Mouse: goblet cells and enterocytes [49] |
| Astrovirus **"no inflammation"** (taken from reviews) | **CLOSED and REVERSED for replicating virus** — IFN-dominated host response in human intestinal enteroids [47][48] and IFN-λ in mouse enteroids and in vivo [49] |
| Astrovirus **barrier-effect magnitude** | **CLOSED for live virus: TER to ~0% of initial by 32 hpi at MOI 10, significant by 24 hpi, dose-dependent; FITC-dextran flux significant by 36 hpi** [44] |
| Astrovirus **age skew / exposure** (absent in pass 01) | **CLOSED. HAstV-1 seroprevalence 73% of US adults; MLB1 100% by adulthood with a nadir at 12–23 months** [62][63]; 67.9% of Japanese positives aged 1–3y [64]; outbreaks in nurseries/kindergartens 31%, autumn 44% [59]; daycare attack rate 20% overall / 41.2% under 2 / 74.3% in household contacts of affected attenders [60] |
| Adenovirus **F41 — which MIC ligand, and how** | **CLOSED. MICB only; retention not transcription; both ligands' expression rises; HCT116; preliminary; no NK killing assay; no gene assigned** [68] |
| Norovirus **antigenic-replacement magnitude** (qualitative in pass 01) | **CLOSED. ~70% of human polyclonal blockade lost across one 3-year replacement (EC50 0.021% → 0.066%, 3.1-fold); ≥11% of that from epitope A; one mAb lost blockade entirely, two reduced 3.9–6.3-fold** [15]; epitope A alone accounts for 40% of serum reactivity [16] and 36% of cross-blockade [26] |
| Norovirus **what survives the replacement** (not asked in pass 01) | **CLOSED. 7 immunodominant HLA class-I epitopes highly conserved across GII.4 pandemic strains** [29]; ≥50% of antibody mass targets the conserved S domain and <1% of VP1-binding antibody is neutralising (mouse) [28] |
| **Burden, all species** (pass 01 ranked on mechanics only) | **CLOSED — §1** |

**Still UNVERIFIED after this pass**, and worth listing so no one fills them
in from recollection:

- **Astrovirus**: ID50, incubation, burst size, eclipse; the **depth** (not
  the onset kinetics) of the capsid-only and UV-virus TER drop; the
  **permeability recovery time course** at a high dose; whether the VLP arm
  disrupts occludin; any death assay beyond trypan blue.
- **Adenovirus F41**: ID50, incubation, burst size, eclipse; lytic
  vs non-lytic exit; spread mode; whether the MICB retention is functionally
  sufficient to reduce NK killing; which E3 gene does it.
- **Norovirus**: burst size per cell; eclipse; whether vesicle-cloaked
  virions resist antibody.
- **Rotavirus**: per-cell burst for human RV (pass 01's gap, unchanged);
  what fraction of real enterocyte infections exit non-lytically.
- **Hepatitis A**: ID50, burst size, eclipse; capsids per eHAV vesicle.
- **Human bocavirus, Aichi virus**: attributable diarrhoeal fraction.

---

## 8. Cuts, and why

| Cut | Why |
|---|---|
| **Sapovirus** | Pass 01's mechanical cut stands; this pass adds that it is not significantly attributable in a high-income case–control series (7.0% vs 3.0%, P=.07) [7] and that its post-vaccine rise is **relative, not absolute** [93]. **Plays as norovirus with lower numbers**, which the roster's rule forbids. Its EM morphology is worth borrowing [43] |
| **Coxsackievirus B / enterovirus** | **Removed from the revisit list.** 11% of AGE cases vs 9% of healthy controls (P=.34) [7]. Not a gut pathogen in attribution terms. Its non-lytic-egress mechanic is now better carried by rotavirus/norovirus [81]; its 3A secretion-block mechanic is a poliovirus-family property that pass 01 itself showed does not generalise |
| **Parechovirus A** | New cut. **More common in healthy children than in cases** (16% vs 11%) [7]. The clearest passenger in the dataset |
| **Aichi virus** | Cut stands. 1.45% prevalence (pass 01), no attributable fraction sourced [96] |
| **Human bocavirus** | New cut. 55% stool detection, zero attribution [96] |
| **SARS-CoV-2** | New cut as an enemy. No diarrhoeal attributable fraction; would read as a gimmick. **Trait banked**: gut persistence long after clearance elsewhere (12.7% at 4 months, 3.8% at 7 months) [95] — but chronic norovirus is the better organism for that slot |
| **Hepatitis A** | Cut stands on anatomy and timeline (pass 01). Its mechanic is the only answer to a real roster gap, so **keep the mechanic on the gap list, not the organism on the roster** (§6.3) |
| **Astrovirus VA1 as its own slot** | Cut. The brain is not on the board [65][66][67]. But the astrovirus entry must name which clade it models |

---

## 9. Corrections to standing design

1. **`PATHOGEN_ROSTER.md`, shortlist line: "norovirus GII.4 (antigenic
   replacement on a real 2–4 year cycle)" needs a qualifier.** 2–4 years is
   the **historical** beat (six to seven named replacements, 1995–2012); it
   has not held since. GII.4 Sydney has dominated 2012–2023, replacing its
   polymerase rather than its capsid, and the literature says the 2–4-year
   pattern "has been broken" [21][22]. **Implement as stochastic with a long
   tail, not a fixed timer**, and note that post-2017 variants
   **co-circulate** rather than sweep [22][25].

2. **The roster's astrovirus line — "breach with no infection and no stress
   signal … it defeats the entire detection stack by not being detectable" —
   is half wrong and should be rewritten.** The capsid-alone breach is real
   and the experimental system is *stronger* than the roster's own hedge
   assumed: polarised, differentiated, filter-grown Caco-2 at
   TER ≥1,000 Ω·cm², apical-only [44]. But **replicating astrovirus produces
   a strong interferon response** in human intestinal enteroids and organoids
   [47][48]. Rewrite as **"a breach with no infection behind it"** — the
   infected cell is a normal target; the wall damage is what has no signal.

3. **"Astrovirus's five-or-six-pointed star" (brief and art direction) is the
   minority / alkaline-treated EM appearance, not the native virion.**
   Cryo-EM gives a 35 nm shell with 44 nm overall diameter and **30**
   globular spikes on the mature infectious particle (90 on the immature one)
   [53]; negative stain gives 41 nm with a spike layer, and star-like
   morphology **only after brief pH-10 treatment** [54]; and Madeley states
   that **only a minority** of virions of either astrovirus or calicivirus
   show the characteristic structure [43]. **Draw the star anyway — it is the
   only thing that reads at 20 px — but record it as a deliberate choice of
   icon over structure.**

4. **"A calicivirus's 32 cup-shaped depressions" is structurally correct but
   is the *sapovirus* negative-stain look, and the astrovirus/norovirus
   silhouette discriminator is not the star.** Prasad's cryo-EM gives
   norovirus 38.0 nm, T=3, 90 dimers, arch-like two-fold capsomeres and
   **large hollows at the 5- and 3-fold axes** — 12 + 20 = 32, which is where
   the figure comes from [41]. But by EM **both** astrovirus and calicivirus
   show a six-pointed star [43]. The real discriminators are: astrovirus =
   **round unbroken edge, white-centred star, triangular hollows**;
   calicivirus = **feathery edge, dark-centred star ("Star-of-David"), round
   or oval hollows** [43]. Build the sprites on edge quality and additive-vs-
   subtractive, not on star-vs-no-star.

5. **The built rotavirus is missing two real mechanics, and one of them
   undercuts §1b's charge floor.** Rotavirus is shed as **vesicle-cloaked
   clusters**, >60% of stool vesicles carrying **more than 10 particles**,
   making up **10–45% of total stool virus**, whose removal cut infectivity
   by **72–83%**, and at a 1× dose producing the same 24-hour severe
   diarrhoea as **5× free virus** [81]. These vesicles are also
   **chlorine-resistant and persist for months in water** (murine) [82]. In
   §1b terms, **one vesicle defeats the minimum-infectious-dose floor** that
   makes a trickle "arithmetically impossible." Either build bulk delivery or
   record explicitly that rotavirus's dose is abstracted in a second way
   beyond the one already recorded.

6. **Rotavirus also has a non-lytic exit, and the game has no host-cell
   state for it.** In human H69 cholangiocytes, ~100% of releasable
   rotavirus was released with **no change in plasma-membrane permeability
   through 72 h** [81]. §1c's host layer has `Healthy / Infected / Dead /
   Empty` and §4b's virus always ends in a death — loud, quiet, or burn-out.
   There is no state for **"infected, exporting, and not going to die,"**
   which is the viral analogue of the *Vibrio* subversion the roster already
   calls its best mechanic. Cell-type-dependent (MA104 monkey cells did
   lyse), so implement as a **fraction** of infections, not a species flag.

7. **§4c's "build the MHC-I stealth pathogen as a cliff" must not be
   mirrored onto F41's NK blindness.** The cliff rule is correct for MHC-I
   because partial MHC-I reduction does nothing in reality. NKG2D is a
   **summed, multi-ligand** input, and the F41 result blocks surface display
   of **MICB only** — MICA was explicitly not shown blocked, in one human
   carcinoma line, with no NK killing assay and no gene assigned [68].
   **Build F41's NK evasion as partial.**

8. **`PATHOGEN_ROSTER.md`'s "still with no answer" list should gain
   `capsid-as-enterotoxin` and keep `antibody-resistant egress` open.**
   The roster has no answer to **damage that survives neutralisation** — a
   free structural protein that drives the §1b plaque charge with no
   organism, no infection and no signal, and that works just as well on
   virions the player's antibody has already coated [44][45]. And the
   antibody-resistant-egress gap is **not** closed by vesicle cloaking,
   because the antibody half of that was never tested [81]; it is closed only
   by hepatitis A, which the roster has already cut on anatomy.

9. **A body-upgrade-disabling enemy is now a sourced category, with a
   pathway mismatch that must be recorded.** Astrovirus coat protein binds
   the C1q A chain and MBL, displaces the C1s-C1r-C1r-C1s tetramer,
   inhibits C1 and C4 activation and C5a generation, and reduces C4b, iC3b
   and C5b-9 — across serotypes 1–4 [50][51][52]. Complement is now a
   **shop body upgrade**, so this would be the first enemy to switch a
   purchase off. **But it inhibits the classical and lectin pathways, and the
   roster's own evidence (Wu 2024) is that gut luminal C3 deposition is
   alternative-pathway and unchanged in C1q- and MBL-deficient mice.** As
   written the counter is aimed at the wrong pathway for this map. See §3.3
   for three honest ways to scope it; do not build it unscoped.

---

## 10. References

Entries retrieved via **PubMed** (per the PubMed tool's attribution
requirement, cited with DOI links; *according to PubMed*) or via
**Consensus**; Consensus-sourced entries without a retrievable DOI carry
their Consensus record link, matching pass 01's format.

**Burden and epidemiology**

- [1] GBD 2023 Enteric Infectious Diseases Collaborators (2026) *Global burden of enteric infectious diseases, diarrhoeal diseases, and corresponding aetiologies, 1990–2023: a systematic analysis for the Global Burden of Disease Study 2023.* Lancet Infect Dis. [DOI](https://doi.org/10.1016/S1473-3099(26)00194-5)
- [2] Global Pediatric Diarrhea Surveillance network (2023) *Aetiology and incidence of diarrhoea requiring hospitalisation in children under 5 years of age in 28 low-income and middle-income countries.* BMJ Glob Health. [DOI](https://doi.org/10.1136/bmjgh-2022-009548)
- [3] Expert group (2016) *The Vast and Varied Global Burden of Norovirus: Prospects for Prevention and Control.* PLoS Med. [DOI](https://doi.org/10.1371/journal.pmed.1001999)
- [4] Burke R. et al. (2020) *The Burden of Norovirus in the United States, as Estimated Based on Administrative Data.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/357891cef40559afab6c18fdee6fd50e/)
- [5] Bartsch S. et al. (2020) *The Clinical and Economic Burden of Norovirus Gastroenteritis in the United States.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/811c0876519156e6bb31a8d0e7077b07/)
- [6] Bartsch S. et al. (2016) *Global Economic Burden of Norovirus Gastroenteritis.* PLoS ONE. [Consensus record](https://consensus.app/papers/details/fa4beeafb7905ca3974d41116833c269/)
- [7] Hassan F. et al. (2018) *Viral Etiology of Acute Gastroenteritis in <2-Year-Old US Children in the Post-Rotavirus Vaccine Era.* J Pediatric Infect Dis Soc. [Consensus record](https://consensus.app/papers/details/1db2a5ea096050c68dcd5203d680ac8c/)
- [8] Staat M. et al. (2020) *Continued Evidence of the Impact of Rotavirus Vaccine in Children Less than 3 Years of Age from the US New Vaccine Surveillance Network, 2006–2016.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/42211df59d665e3a9442f7b92d00774b/)
- [9] Farahmand M. et al. (2021) *Global prevalence and genotype distribution of norovirus infection in children with gastroenteritis: a meta-analysis, 2015–2020.* Rev Med Virol. [Consensus record](https://consensus.app/papers/details/289d9a4cb87257748f09b9ccecd83128/)
- [10] Mai C.T.N. et al. (2023) *Prevalence and Characterization of Gastroenteritis Viruses among Hospitalized Children during a Pilot Rotavirus Vaccine Introduction in Vietnam.* Viruses. [Consensus record](https://consensus.app/papers/details/51c2923b2a9d5d218fdc5a9531918fd1/)
- [11] L'Etoile N. et al. (2025) *Healthcare-Associated Gastroenteritis: Outbreak Report and Systematic Review of the Literature.* J Pediatric Infect Dis Soc. [Consensus record](https://consensus.app/papers/details/94ebd2ad5ea55a42ab34e37d0185b9dd/)
- [12] Calderwood L.E. et al. (2021) *Norovirus Outbreaks in Long-term Care Facilities in the United States, 2009–2018: A Decade of Surveillance.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/ee6a4672c57f578e9f6e67a037c03e5a/)
- [13] Rosenthal N. et al. (2010) *Epidemiological and genetic characteristics of norovirus outbreaks in long-term care facilities, 2003–2006.* Epidemiol Infect. [Consensus record](https://consensus.app/papers/details/c7186b1d14c45ab4b6a5452ae8cb33f2/)
- [14] Costantini V. et al. (2015) *Epidemiologic, Virologic, and Host Genetic Factors of Norovirus Outbreaks in Long-term Care Facilities.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/19401455bf5750259395fb299881dc30/)
- [98a] Havelaar A. et al. (2015) *World Health Organization Global Estimates and Regional Comparisons of the Burden of Foodborne Disease in 2010.* PLoS Med. [DOI](https://doi.org/10.1371/journal.pmed.1001923)
- [98b] Kirk M. et al. (2015) *World Health Organization Estimates of the Global and Regional Disease Burden of 22 Foodborne Bacterial, Protozoal, and Viral Diseases, 2010.* PLoS Med. [DOI](https://doi.org/10.1371/journal.pmed.1001921)
- [98c] WHO (2026) *WHO estimates of the global, regional, and national burden of 14 foodborne diarrhoeal enteric hazards, 2000–21.* Lancet Glob Health. [DOI](https://doi.org/10.1016/j.langlo.2026.103997)

**Norovirus — antigenic variation, immunity, dose, shedding, transmission**

- [15] Debbink K. et al. (2013) *Emergence of new pandemic GII.4 Sydney norovirus strain correlates with escape from herd immunity.* J Infect Dis 208(11):1877-87. [DOI](https://doi.org/10.1093/infdis/jit370)
- [16] Lindesmith L.C. et al. (2012) *Emergence of a norovirus GII.4 strain correlates with changes in evolving blockade epitopes.* J Virol 87(5):2803-13. [DOI](https://doi.org/10.1128/JVI.03106-12)
- [17] Lindesmith L.C. et al. (2010) *Norovirus GII.4 strain antigenic variation.* J Virol 85(1):231-42. [DOI](https://doi.org/10.1128/JVI.01364-10)
- [18] Lindesmith L.C. et al. (2012) *Immunogenetic mechanisms driving norovirus GII.4 antigenic variation.* PLoS Pathog 8(5):e1002705. [DOI](https://doi.org/10.1371/journal.ppat.1002705)
- [19] Lindesmith L.C. et al. (2011) *Monoclonal antibody-based antigenic mapping of norovirus GII.4-2002.* J Virol 86(2):873-83. [DOI](https://doi.org/10.1128/JVI.06200-11)
- [20] Lindesmith L.C. et al. (2019) *Human Norovirus Epitope D Plasticity Allows Escape from Antibody Immunity without Loss of Capacity for Binding Cellular Ligands.* J Virol 93(2). [DOI](https://doi.org/10.1128/JVI.01813-18)
- [21] Zheng G.-Y. et al. (2022) *Evolutionary analyses of emerging GII.2[P16] and GII.4 Sydney[P16] noroviruses.* Virus Evol. [Consensus record](https://consensus.app/papers/details/a7516c2a980f53a1a0895379f634398c/)
- [22] Barclay L. et al. (2025) *Molecular Evolution and Epidemiology of Norovirus GII.4 Viruses in the United States.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/39af05f9afa2586d950f1568eee5f21e/)
- [23] Tohma K. et al. (2019) *Population Genomics of GII.4 Noroviruses Reveal Complex Diversification and New Antigenic Sites Involved in the Emergence of Pandemic Strains.* mBio. [Consensus record](https://consensus.app/papers/details/fdd60537ee075bf587c2784ec9cd9c7b/)
- [24] Eden J. et al. (2013) *The emergence and evolution of the novel epidemic norovirus GII.4 variant Sydney 2012.* Virology. [Consensus record](https://consensus.app/papers/details/86d44c9e21a0559ea647e771a2f5406a/)
- [24a] Oparina S.V. et al. (2026) *Norovirus GII.4 Sydney: origin and global distribution. A systematic review.* J Microbiol Epidemiol Immunobiol. [Consensus record](https://consensus.app/papers/details/0c3f82f103ca53cd9abd94b5e0b002d2/)
- [25] Chhabra P. et al. (2023) *Emergence of Novel Norovirus GII.4 Variant.* Emerg Infect Dis. [Consensus record](https://consensus.app/papers/details/9ae5f7263c6957309a83cef22e0620a1/)
- [26] Landivar M. et al. (2025) *Highly variable antigenic site located at the apex of GII.4 norovirus capsid protein induces cross-reactive blocking antibodies in a variant-specific manner.* J Virol. [Consensus record](https://consensus.app/papers/details/b154afd889e9505da876dfdfcd9796de/)
- [27] Tohma K. et al. (2025) *Manipulation of immunodominant variable epitopes of norovirus capsid protein elicited cross-blocking antibodies to different GII.4 variants.* J Virol. [Consensus record](https://consensus.app/papers/details/82654f4299d95246b866dc6a63ecfbae/)
- [28] Malm M. et al. (2016) *Type-specific and cross-reactive antibodies and T cell responses in norovirus VLP immunized mice are targeted both to conserved and variable domains of capsid VP1 protein.* Mol Immunol. [Consensus record](https://consensus.app/papers/details/57d0cb173a125026be76061666b2cb33f2/)
- [29] Pattekar A. et al. (2021) *Norovirus-Specific CD8+ T Cell Responses in Human Blood and Tissues.* CMGH. [Consensus record](https://consensus.app/papers/details/9f26490cb90251818c44f0baf8b8beed/)
- [30] Thorven M. et al. (2005) *A Homozygous Nonsense Mutation (428G→A) in the Human Secretor (FUT2) Gene Provides Resistance to Symptomatic Norovirus (GGII) Infections.* J Virol. [Consensus record](https://consensus.app/papers/details/0bdc7044784254b39ffcfdcb6ca6026b/)
- [31] Nordgren J. & Svensson L. (2019) *Genetic Susceptibility to Human Norovirus Infection: An Update.* Viruses. [Consensus record](https://consensus.app/papers/details/bcdda09498fc5757acf3aeeef012a4a2/)
- [32] Lindesmith L.C. et al. (2020) *Virus–Host Interactions Between Nonsecretors and Human Norovirus.* CMGH. [Consensus record](https://consensus.app/papers/details/a7d811887b2f54ebbef7f9d594a80408/)
- [33] Atmar R.L. et al. (2008) *Norwalk Virus Shedding after Experimental Human Infection.* Emerg Infect Dis. [Consensus record](https://consensus.app/papers/details/0447846a825b5d9786b9710795f99c11/)
- [34] Ge Y. et al. (2023) *Effect of Norovirus Inoculum Dose on Virus Kinetics, Shedding, and Symptoms.* Emerg Infect Dis. [Consensus record](https://consensus.app/papers/details/6050a2468bf75c37b00241fe43a9cb2f/)
- [35] Cheng H.-Y. et al. (2021) *Viral shedding in gastroenteritis in children caused by variants and novel recombinant norovirus infections.* Medicine. [Consensus record](https://consensus.app/papers/details/f550c2f0e7845fba80ef249eed9c1550/)
- [36] Marks P. et al. (2000) *Evidence for airborne transmission of Norwalk-like virus (NLV) in a hotel restaurant.* Epidemiol Infect. [Consensus record](https://consensus.app/papers/details/fa93772c65b85124af03296df5ca67e3/)
- [37] Xiao S.-L. et al. (2017) *Airborne or Fomite Transmission for Norovirus? A Case Study Revisited.* Int J Environ Res Public Health. [Consensus record](https://consensus.app/papers/details/6f9829f2511f55abb1172fee4cfd1f73/)
- [38] Mouchtouri V. et al. (2024) *Systematic literature review and meta-analysis on preventing and controlling norovirus outbreaks on cruise ships, 1990 to 2020.* Eurosurveillance. [Consensus record](https://consensus.app/papers/details/6dd55fef49eb505c977fab90d5b54e2d/)
- [39] Flint A. et al. (2025) *Genomic characterization of noroviruses from an outbreak associated with oysters.* Microbiol Spectr. [Consensus record](https://consensus.app/papers/details/1ac4590934d7599ca8e11ac230ef88ba/)
- [40] Le Guyader F. et al. (2011) *Transmission of viruses through shellfish: when specific ligands come into play.* Curr Opin Virol. [Consensus record](https://consensus.app/papers/details/00f7deff14eb523cbc09d087c2c14c48/)
- [41] Prasad B.V.V. et al. (1994) *Three-dimensional structure of baculovirus-expressed Norwalk virus capsids.* J Virol. [Consensus record](https://consensus.app/papers/details/62203d3a9a72516c9773a2cc91644700/)
- [42] Hu L.-Y. et al. (2022) *Atomic structure of the predominant GII.4 human norovirus capsid reveals novel stability and plasticity.* Nat Commun. [Consensus record](https://consensus.app/papers/details/2a10f21d7f11510e971220ade52194ea/)
- [99] Ford-Siltz L. et al. (2024) *Characterization of cross-reactive, non-neutralizing monoclonal antibodies against a pandemic GII.4 norovirus variant.* Microbiol Spectr. [Consensus record](https://consensus.app/papers/details/7153ee5a2e445a7790472e5655e35a5c/)

**Astrovirus**

- [43] Madeley C.R. (1979) *Comparison of the features of astroviruses and caliciviruses seen in samples of feces by electron microscopy.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/8fdab8d20d5152b6b1928f34e0268d9a/)
- [44] Moser L.A. & Schultz-Cherry S. (2007) *Astrovirus Increases Epithelial Barrier Permeability Independently of Viral Replication.* J Virol. [DOI](https://doi.org/10.1128/JVI.00942-07) — full text read in this session (PMC2168760) for the experimental detail in §3.1
- [45] Meliopoulos V.A. et al. (2016) *Oral Administration of Astrovirus Capsid Protein Is Sufficient To Induce Acute Diarrhea In Vivo.* mBio. [DOI](https://doi.org/10.1128/mBio.01494-16)
- [46] Cortez V. et al. (2017) *Astrovirus Biology and Pathogenesis.* Annu Rev Virol. [Consensus record](https://consensus.app/papers/details/e2dbe3624a4e5928a41b447b74ea2245/)
- [47] Kolawole A. et al. (2019) *Astrovirus replication in human intestinal enteroids reveals multi-cellular tropism and an intricate host innate immune landscape.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/40f426ac8c65514e8317a021034ce4e9/)
- [48] Triana S. et al. (2021) *Single-cell transcriptomics reveals immune response of intestinal cell types to viral infection.* Mol Syst Biol. [Consensus record](https://consensus.app/papers/details/c9d68f4920d15fa4b104d37ca0c3382a/)
- [49] Ingle H. et al. (2021) *Murine astrovirus tropism for goblet cells and enterocytes facilitates an IFN-λ response in vivo and in enteroid cultures.* Mucosal Immunol. [Consensus record](https://consensus.app/papers/details/9f5e070f7819537b92702da8fca6b8ba/)
- [50] Bonaparte R.S. et al. (2007) *Human Astrovirus Coat Protein Inhibits Serum Complement Activation via C1, the First Component of the Classical Pathway.* J Virol. [Consensus record](https://consensus.app/papers/details/5d5446f245bd529c9a10b11cbb16a58c/)
- [51] Hair P. et al. (2009) *Human astrovirus coat protein binds C1q and MBL and inhibits the classical and lectin pathways of complement activation.* Mol Immunol. [Consensus record](https://consensus.app/papers/details/aa22a209e4185a6abc30303b3a0285e5/)
- [52] Gronemus J.Q. et al. (2010) *Potent inhibition of the classical pathway of complement by a novel C1q-binding peptide derived from the human astrovirus coat protein.* Mol Immunol. [Consensus record](https://consensus.app/papers/details/d6262c0294535ea4aa4ce6e541e078f6/)
- [53] Dryden K.A. et al. (2012) *Immature and Mature Human Astrovirus: Structure, Conformational Changes, and Similarities to Hepatitis E Virus.* J Mol Biol. [Consensus record](https://consensus.app/papers/details/2bdd0e961aa653d28f9da0a2167b6e45/)
- [54] Risco C. et al. (1995) *Ultrastructure of human astrovirus serotype 2.* J Gen Virol. [Consensus record](https://consensus.app/papers/details/241f86fcbee6539ca08c9d254d3f0683/)
- [55] Dong J. et al. (2011) *Crystal structure of the human astrovirus capsid spike.* PNAS. [Consensus record](https://consensus.app/papers/details/415c3beaf5b15391a344502312cdd53d/)
- [56] Farahmand M. et al. (2023) *Worldwide prevalence and genotype distribution of human astrovirus in gastroenteritis patients: A systematic review and meta-analysis.* Microb Pathog. [Consensus record](https://consensus.app/papers/details/a0403f747af758109d5f7414e4ff6440/)
- [57] Razizadeh M.H. et al. (2022) *An annoying enteric virus: A systematic review and meta-analysis of human astroviruses and gastrointestinal complications in children.* Rev Med Virol. [Consensus record](https://consensus.app/papers/details/43c428365331543ab100bd607ea0f934/)
- [58] Prajapati B. et al. (2026) *Prevalence of astrovirus-associated gastroenteritis among hospitalized pediatric patients in Asia: A systematic review and meta-analysis.* Indian J Gastroenterol. [Consensus record](https://consensus.app/papers/details/44f6cf7a4b44553cbdc1f2e82966ff4a/)
- [59] Fu J. et al. (2023) *Acute gastroenteritis outbreaks caused by human astrovirus, 1978–2021: A systematic review.* Biosaf Health. [Consensus record](https://consensus.app/papers/details/53a28dfb51025bfa88124ee03c6dd56b/)
- [60] Parrón I. et al. (2021) *Human Astrovirus Outbreak in a Daycare Center and Propagation among Household Contacts.* Viruses. [Consensus record](https://consensus.app/papers/details/917cfa8d1ef85983aad9535c389dac42/)
- [61] Hu L. et al. (2026) *An outbreak of human astrovirus type 5 in a kindergarten in Chongqing, Southwest China.* BMC Infect Dis. [Consensus record](https://consensus.app/papers/details/f6277ee259345420a80542ddb78e58b3/)
- [62] Meyer L. et al. (2021) *Human Astrovirus 1–8 Seroprevalence Evaluation in a United States Adult Population.* Viruses. [Consensus record](https://consensus.app/papers/details/b6a8591e3f2857b39613ad29a1c01ea4/)
- [63] Holtz L.R. et al. (2014) *Seroepidemiology of Astrovirus MLB1.* Clin Vaccine Immunol. [Consensus record](https://consensus.app/papers/details/d6256d17d82d587fbfef6a48ae392a85/)
- [64] Ushijima H. et al. (2024) *Human Astrovirus Infection and Outbreaks in Japanese Children Before and After the Emergence of COVID-19 Pandemic.* J Med Virol. [Consensus record](https://consensus.app/papers/details/f26fa7884c9a5fb5b763e884c956c13b/)
- [65] Ghosh A. et al. (2024) *Structure and antigenicity of the divergent human astrovirus VA1 capsid spike.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/9a4ed291d4b05e8ba6491d99f70f7bc9/)
- [66] Bami S. et al. (2022) *Human Astrovirus VA1 Encephalitis in Pediatric Patients With Cancer.* J Pediatric Infect Dis Soc. [Consensus record](https://consensus.app/papers/details/ad312388c7265f5e991ca91994a8bbaf/)
- [67] Maximova O. et al. (2023) *Pathogenesis and outcome of VA1 astrovirus infection in the human brain.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/526198615d95510a99c06c4fde2d1e2a/)

**Adenovirus**

- [68] Oliveira E.R.A. et al. (2021) *Intracellular Sequestration of the NKG2D Ligand MIC B by Species F Adenovirus.* Viruses. [Consensus record](https://consensus.app/papers/details/88364e4a206c5406b30c846cb7601174/)
- [69] McSharry B.P. et al. (2008) *Adenovirus E3/19K Promotes Evasion of NK Cell Recognition by Intracellular Sequestration of the NKG2D Ligands MICA and MICB.* J Virol. [Consensus record](https://consensus.app/papers/details/819a75aa58265c2dbd7feb1a23daa437/)
- [70] Rafie K. et al. (2021) *The structure of enteric human adenovirus 41 — A leading cause of diarrhea in children.* Sci Adv. [Consensus record](https://consensus.app/papers/details/6ec7d2af7f0d59cca6a97d9278378684/)
- [71] Pérez-Illana M. et al. (2021) *Cryo-EM structure of enteric adenovirus HAdV-F41 highlights structural variations among human adenoviruses.* Sci Adv. [Consensus record](https://consensus.app/papers/details/9685eaa8d36c5c3ea0deee60688b9c34/)
- [72] Grand R.J. (2023) *Pathogenicity and virulence of human adenovirus F41: Possible links to severe hepatitis in children.* Virulence. [Consensus record](https://consensus.app/papers/details/90bbaacb01315b25809ab6e5132d15d3/)
- [73] Kidd A.H. et al. (1993) *Adenovirus type 40 virions contain two distinct fibers.* Virology. [Consensus record](https://consensus.app/papers/details/e7cbd3bb0853596f91a328a398f9b497/)
- [74] Yeh H.-Y. et al. (1994) *Human adenovirus type 41 contains two fibers.* Virus Res. [Consensus record](https://consensus.app/papers/details/671660833175571b855f8673f6616d8c/)
- [75] Lee B. et al. (2020) *Pediatric acute gastroenteritis due to adenovirus 40/41 in low- and middle-income countries.* Curr Opin Infect Dis. [Consensus record](https://consensus.app/papers/details/770ccc23ca885e559ddbba112539be79/)
- [76] Do Nascimento L.G. et al. (2022) *Human enteric adenovirus F40/41 as a major cause of acute gastroenteritis in children in Brazil, 2018 to 2020.* Sci Rep. [Consensus record](https://consensus.app/papers/details/15cb4583e435579eb807571af2e1ebf1/)
- [77] Mello M.S. et al. (2025) *Molecular Epidemiology of Human Adenovirus from Acute Gastroenteritis Cases in Brazil After the COVID-19 Pandemic Period, 2021–2023.* Viruses. [Consensus record](https://consensus.app/papers/details/1217213f7f6d57fcae49544132bd6ee0/)
- [78] Gutierrez Sanchez L.H. et al. (2022) *A Case Series of Children with Acute Hepatitis and Human Adenovirus Infection.* N Engl J Med. [Consensus record](https://consensus.app/papers/details/7ef11ad37568519cbb8a3c536005679d/)
- [79] Liang J.-C. et al. (2023) *Clinicopathologic Features of Severe Acute Hepatitis Associated With Adenovirus Infection in Children.* Am J Surg Pathol. [Consensus record](https://consensus.app/papers/details/09ca614f0d1c53cf87c4f6646be1e5e7/)
- [80] Namakin K. et al. (2023) *Severe acute hepatitis of unknown etiology in children in 2022: A Narrative Review.* New Microbes New Infect. [Consensus record](https://consensus.app/papers/details/a4fef78213ce5817bcb2a8a58d1647cb/)

**Vesicle cloaking, non-lytic exit, quasi-envelope**

- [81] Santiana M. et al. (2018) *Vesicle-cloaked virus clusters are optimal units for inter-organismal viral transmission.* Cell Host Microbe. [DOI](https://doi.org/10.1016/j.chom.2018.07.006) — full text read in this session (PMC6226266) for the system labels and numbers in §6.1/§6.2
- [82] Zhang M. et al. (2022) *Vesicle-Cloaked Rotavirus Clusters are Environmentally Persistent and Resistant to Free Chlorine Disinfection.* Environ Sci Technol. [Consensus record](https://consensus.app/papers/details/836dd90bf53d505785e9ff31bb5761d0/)
- [83] Mirabelli C. et al. (2018) *All Aboard! Enteric Viruses Travel Together.* Cell Host Microbe. [Consensus record](https://consensus.app/papers/details/9ab2e4626c225553a1024ede3c881f4e/)
- [84] Das A. et al. (2023) *Cell entry and release of quasi-enveloped human hepatitis viruses.* Nat Rev Microbiol. [Consensus record](https://consensus.app/papers/details/f4b1a02ed5e25650a443255fc709150a/)
- [85] Feng Z. et al. (2013) *A pathogenic picornavirus acquires an envelope by hijacking cellular membranes.* Nature. (pass 01 [27]) [Consensus record](https://consensus.app/papers/details/a37d40b820e85148ac60a6644d98bef8/)
- [86] Ramakrishnaiah V. et al. (2014) *Hepatitis virus hijacks shuttle: Exosome-like vesicles provide protection against neutralizing antibodies.* Hepatology. [Consensus record](https://consensus.app/papers/details/918ea9be9651521d921077b0af504fed/)

**Hepatitis A, CMV, sapovirus, other**

- [87] Migueres M. et al. (2021) *Hepatitis A: Epidemiology, High-Risk Groups, Prevention and Research on Antiviral Treatment.* Viruses. [Consensus record](https://consensus.app/papers/details/af1d1e2ab9205cf4bd66e49aa739ef40/)
- [88] Ndumbi P. et al. (2018) *Hepatitis A outbreak disproportionately affecting men who have sex with men in the EU/EEA, June 2016 to May 2017.* Eurosurveillance. [Consensus record](https://consensus.app/papers/details/58abcf62e99f58ea82282c3f098cf29e/)
- [89] Dencs Á. et al. (2024) *Hepatitis A virus subtype IB outbreak among MSM in Hungary with a link to a frozen berry source.* Infect Genet Evol. [Consensus record](https://consensus.app/papers/details/67fd61b72b6057fdbbed53270a570a38/)
- [90] Kandiel A. & Lashner B. (2006) *Cytomegalovirus Colitis Complicating Inflammatory Bowel Disease.* Am J Gastroenterol. [Consensus record](https://consensus.app/papers/details/85457c6d32055a51983afffc8c5e8431/)
- [91] Soni K. et al. (2025) *Cytomegalovirus Colitis in Adult Patients with Inflammatory Bowel Disease.* Viruses. [Consensus record](https://consensus.app/papers/details/8445db0647ae5b8cb7ab8e728aa8f3b8/)
- [92] Hoque S. et al. (2022) *An increasing trend of human sapovirus infection in Japan, 2009 to 2019.* J Infect Public Health. [Consensus record](https://consensus.app/papers/details/d8c4930343285b3aacd3338d0bcb359c/)
- [93] Pitkänen O. et al. (2019) *The role of the sapovirus infection increased in gastroenteritis after national immunisation was introduced.* Acta Paediatr. [Consensus record](https://consensus.app/papers/details/e6c7d867322a5ab1bd933b3946be170c/)
- [94] Guo M. et al. (2021) *Potential intestinal infection and faecal–oral transmission of SARS-CoV-2.* Nat Rev Gastroenterol Hepatol. [Consensus record](https://consensus.app/papers/details/90cfe9e057a5559ab0ca4acc7d433f07/)
- [95] Natarajan A. et al. (2022) *Gastrointestinal symptoms and fecal shedding of SARS-CoV-2 RNA suggest prolonged gastrointestinal infection.* Med. [Consensus record](https://consensus.app/papers/details/e29688d9c2e45e7f9e0f5cbddca07d63/)
- [96] Shaheen M.N. et al. (2024) *Seasonal Prevalence and Detection of Enteric and Respiratory Viruses in Wastewater and Hospitalized Children with Acute Gastroenteritis.* Curr Microbiol. [Consensus record](https://consensus.app/papers/details/dd96ac526ada56b98e8398a9dc411d37/)
- [97] Obert G. et al. (2000) *Rotavirus-Induced Structural and Functional Alterations in Tight Junctions of Polarized Intestinal Caco-2 Cell Monolayers.* J Virol. [Consensus record](https://consensus.app/papers/details/be5de1e8af025a35805b8aeac685a82c/)

Sources were retrieved from PubMed and from Consensus.

> Create or connect a free Consensus account to return more than 3 results per search in Claude Code.: https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code
