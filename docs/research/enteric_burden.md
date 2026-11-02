# Enteric-pathogen burden — one ranked, cross-kingdom table

Research pass, 2026-11-02. Cross-cutting **epidemiology** pass, commissioned
after the Director asked for "10 more common gut pathogens in humans."
The four previous passes (`pathogens_viruses.md`,
`pathogens_invasive_bacteria.md`, `pathogens_extracellular.md`,
`pathogens_evasion.md`) selected organisms purely on **mechanical novelty**,
by `PATHOGEN_ROSTER.md`'s rule — *"a species earns a slot by forcing a
decision no existing enemy forces."* That rule is good and this pass does
not replace it. This pass supplies the one thing nobody on the project has
ever put next to a candidate: **a burden number with a source and a unit.**

Every factual claim below was checked against **PubMed** (via the PubMed MCP
tool) or **Consensus** during this session; nothing is written from memory.
Where a figure is commonly quoted but I could not pull a source for it in
this pass, it is marked **UNVERIFIED** rather than filled in. Inline markers
are `[n]`; full citations with DOIs at the bottom. *According to PubMed* for
all PubMed-sourced entries.

Read against `GAME_DESIGN.md` §1a (**the map is small intestine** —
duodenum/jejunum/ileum, three lateral bands, no colon) and
`PATHOGEN_ROSTER.md` (the built roster, the shortlist, and the
*C. difficile* anatomy rejection).

---

## 0. The honest preamble: "same units" is partly impossible, and that is a finding

The brief asked for one apples-to-apples table. **There is no single study
or estimate that ranks viruses, bacteria, protozoa *and* helminths in one
unit.** Not because nobody tried, but because:

- **The multi-pathogen diarrhoea studies (GEMS, MAL-ED, VIDA, GPDS) measure
  *attributable diarrhoea*.** Helminths barely cause diarrhoea, so they
  score ~zero in every one of them while infecting a billion people. Their
  burden is **chronic morbidity measured as prevalence and YLDs**, a
  different ruler entirely.
- **GBD and WHO FERG both report DALYs, but over different causes.** GBD
  2021's diarrhoea paper apportions the *diarrhoeal* envelope across 13
  pathogens [1]. WHO's 2026 FERG update apportions only the
  **foodborne-transmitted share** across 14 hazards [2]. Neither covers
  helminths in the same table.
- **High-income surveillance (FoodNet, EU One Health) counts
  laboratory-confirmed bacterial and parasitic cases and barely counts
  viruses at all** [12][16] — so a naive read of FoodNet says
  *Campylobacter* is the commonest enteric pathogen in the USA, when in
  fact **norovirus causes roughly three times as many illnesses** [13].

So this pass gives **four tables in four declared units**, and says plainly
where the orderings disagree (§7.3). The headline table is the one study
that gets closest to a single cross-kingdom ruler.

**GBD iteration — checked, not assumed.** The brief said to verify whether
GBD 2021 or later is published. Searched PubMed this session for a GBD 2023
diarrhoea-aetiology paper; **none is indexed.** GBD 2023 capstones exist for
other causes (e.g. breast cancer, Lancet Oncol 2026 [PMID 41785894]), but
the most recent **diarrhoeal-disease aetiology** iteration remains **GBD
2021**, published in *Lancet Infect Dis* 2025;25:519-536 [1]. Use that.

---

## 1. Headline ranked table — global, community, one unit, one study

**Unit: attributable diarrhoea episodes per 100 child-years.**
**Population: 1,715 children aged 0–24 months, eight sites (Dhaka,
Bangladesh; Vellore, India; Bhaktapur, Nepal; Naushero Feroze, Pakistan;
Venda, South Africa; Haydom, Tanzania; Fortaleza, Brazil; Loreto, Peru).
Enrolled 2009–2012, qPCR reanalysis published 2018.** Source: MAL-ED qPCR
reanalysis [6].

This is the headline because it is **one study, one unit, one denominator,
covering viruses + bacteria + protozoa together**, in the *community* rather
than at a hospital door — so it is not biased toward whatever is severe
enough to be brought in. Ten pathogens accounted for **95.7%** of all
attributable diarrhoea [6].

| # | Organism | Class | Attributable episodes / 100 child-yr (95% CI) | Compartment it actually infects | Right for a small-intestine map? |
|---|---|---|---|---|---|
| 1 | ***Shigella*** | bacterium | **26.1** (23.8–29.9) | **Colon and rectum** [21][22] | ✗ **No** — see §7.2 |
| 2 | **Sapovirus** | virus | **22.8** (18.9–27.5) | Duodenum, jejunum, ileum — **and explicitly not colon** [25] | ✓✓ **Yes, strictly** |
| 3 | **Rotavirus** | virus | **20.7** (18.8–23.0) | Mature villus-tip enterocytes, small intestine (roster §Built) | ✓ Yes — already built |
| 4 | **Adenovirus 40/41** | virus | **19.0** (16.8–23.0) | Replicates productively in small-intestinal enteroids [33]; enteric F-types have a tissue tropism structurally distinct from respiratory types [34] | ✓ Yes |
| 5 | **Enterotoxigenic *E. coli* (ETEC)** | bacterium | **18.8** (16.5–23.8) | Small-intestinal epithelium; LT drives brush-border and microvillus derangement there [29] | ✓ Yes |
| 6 | **Norovirus** | virus | **15.4** (13.5–20.1) | Duodenal and jejunal enterocytes (replication markers RdRp + VPg) [23]; enteroendocrine cells of the small intestine [24] | ✓ Yes |
| 7 | **Astrovirus** | virus | **15.0** (12.0–19.5) | **UNVERIFIED** in this pass — no compartment source pulled | ? |
| 8 | ***Campylobacter jejuni*/*C. coli*** | bacterium | **12.1** (8.5–17.2) | In humans presents as **colitis**, with immunohistochemical and ultrastructural evidence of **colonic mucosal invasion** [28]; murine models show stable stomach/duodenum/ileum colonisation too [28-adjacent, see §7.2] | ✗/~ **Mixed, leans colon** |
| 9 | ***Cryptosporidium*** | protozoan | **5.8** (4.3–8.3) | Unique intracellular niche at the **brush border of intestinal epithelial cells** [26]; the *segment* is host- and species-dependent (ileum/caecum/colon in neonatal mice, duodenum/jejunum in neonatal rats) [27] | ~ Probably yes, **segment partly UNVERIFIED for humans** |
| 10 | **Typical enteropathogenic *E. coli* (tEPEC)** | bacterium | **5.4** (2.8–9.3) | Small intestine (A/E pedestal adhesion; roster already models this) | ✓ Yes |

**Context numbers from the same study, same population** [6]:
- Total diarrhoea incidence **273.8 episodes per 100 child-years** across
  both years (304.9 in year 1, 242.7 in year 2).
- **64.9%** (95% CI 62.6–71.2) of episodes attributable to *any* aetiology by
  qPCR — vs **32.8%** by the original culture/EIA microbiology. A third of
  diarrhoea still has **no identified cause at all**.
- **Viral diarrhoea 36.4%** of overall incidence > bacterial 25.0% >
  parasitic 3.5%.
- Second year of life is far more attributable than the first (82.8% vs
  50.5%) — the bacterial and parasitic share rises with age.

**Year-1 vs year-2 reordering** (same source, same unit) — this is real
round-design material:

| Rank | First year of life | Second year of life |
|---|---|---|
| 1 | Rotavirus (23.9) | ***Shigella*** (41.3) |
| 2 | Adenovirus 40/41 (20.9) | Sapovirus (27.8) |
| 3 | Sapovirus (18.1) | ETEC (22.8) |
| 4 | Norovirus (16.4) | Rotavirus (17.7) |
| 5 | ETEC (14.9) | Adenovirus 40/41 (17.2) |

---

## 2. The same global ranking in three other metrics — and it moves

### 2a. Attributable fraction of **moderate-to-severe** diarrhoea at a health centre

**Unit: adjusted population attributable fraction (AF) of MSD.
Population: 5,304 case/control stool pairs from GEMS, children <5 y, 7 sites
in sub-Saharan Africa and South Asia, enrolled 2007–2011; qPCR reanalysis
published 2016.** Source: GEMS qPCR reanalysis [4].

Top six attributable pathogens, **in descending order**: ***Shigella*** >
**rotavirus** > **adenovirus 40/41** > **ST-ETEC** > ***Cryptosporidium*** >
***Campylobacter***. Those six account for **77.8%** (74.6–80.9) of all
attributable diarrhoea; total attributable burden **89.3%** (83.2–96.0) with
qPCR vs **51.5%** (48.0–55.0) in the original 2013 GEMS analysis [4].

**The original GEMS 2013 analysis is a different list and must not be quoted
as if it were this one.** With culture/EIA, the four leading attributable
causes of MSD in 9,439 cases vs 13,129 controls were **rotavirus,
*Cryptosporidium*, ST-ETEC, and *Shigella*** [3] — adenovirus 40/41 and
*Campylobacter* were both badly undercounted by the older methods [4].
Note that GEMS reports **attributable**, not raw detection: the AF is
derived from case-control odds ratios in conditional logistic regression,
adjusted for age, site and other pathogens [9], precisely so that a
commensal-like carriage signal does not inflate a pathogen's rank.

GEMS also ran a **less-severe diarrhoea (LSD)** arm (6 sites, 2011–2012,
2,368 MSD + 3,174 LSD cases). The leading attributable pathogens were the
same four — rotavirus, *Cryptosporidium*, ST-ETEC, *Shigella* — but
**attributable incidence was several-fold higher for LSD than MSD** (e.g.
rotavirus 22.3 vs 5.5 per 100 child-years in infants) [7]. Severity-gated
sampling understates everything.

### 2b. Attributable fraction of **hospitalised** diarrhoea, 2017–2022

**Unit: AF of hospitalised diarrhoea; attributable incidence per 1,000
child-years. Population: 16,458 qPCR-tested stools from 70,750 children <5 y
hospitalised with diarrhoea, 38 sentinel sites in 31 low- and
middle-income countries, 2017–2022.** Source: WHO Global Pediatric Diarrhea
Surveillance network (GPDS) [10].

| Organism | AF, 2017–2022 (95% CI) | Attributable incidence, 2021–22 (per 1,000 child-yr) | Raw weighted *detection* prevalence |
|---|---|---|---|
| **Rotavirus** | **32.5%** (27.4–37.6) | **2.3** (1.8–3.0) | 30.4% |
| ***Shigella*** | **9.8%** (8.4–11.3) | **0.9** (0.7–1.1) | 14.1% |
| **Adenovirus 40/41** | **8.6%** (6.3–10.8) | **0.6** (0.4–0.8) | 19.1% |
| **Norovirus** | **6.7%** (5.6–7.7) | **0.6** (0.4–0.8) | 17.9% |
| ***Campylobacter jejuni/coli*** | not in top 4 | — | 8.6% |

Note how far **detection** diverges from **attribution** here: adenovirus
40/41 is detected in 19.1% of hospitalised cases but attributed only 8.6%.
That gap is §7.1 in a single row.

### 2c. Deaths — GBD 2021 fatal population-attributable fractions

**Unit: fatal PAF (%) of diarrhoeal deaths. Population: 204 countries and
territories, year 2021, all ages and children <5 y.** Source: GBD 2021
diarrhoeal diseases [1].

Envelope: diarrhoeal diseases caused **1.17 million (0.793–1.62) deaths**
globally in 2021 — a 60.3% fall since 1990 — and **59.0 million (47.2–73.2)
DALYs**, of which **30.9 million (23.1–42.0)** were in children <5 [1].

| Organism | Fatal PAF, **all ages** | Fatal PAF, **children <5 y** |
|---|---|---|
| **Rotavirus** | **15.2%** (11.4–20.1) | **35.2%** (28.7–43.0) |
| **Norovirus** | **10.6%** (2.3–17.0) | — |
| ***Cryptosporidium*** | **10.2%** (7.03–14.3) | >10% (stated, value not in abstract) |
| ***Shigella*** | — | **24.0%** (15.2–37.9) |
| **Adenovirus** | — | **23.8%** (14.8–36.3) |
| **tEPEC** | — | >10% (stated) |
| **ST-ETEC** | — | >10% (stated) |

**Weakness flag:** the per-pathogen *non-fatal* and *DALY* PAFs from GBD 2021
were not extractable from the abstract in this session and are therefore
**UNVERIFIED**. Only the fatal PAFs above are sourced. PAFs here are
counterfactual and **do not sum to 100%**.

### 2d. Foodborne DALYs — WHO FERG, the 2026 update

**Unit: foodborne illnesses, deaths, DALYs. Population: 194 countries,
2000–2021, 14 diarrhoeal foodborne hazards.** Source: WHO FERG update [2].

In 2021 the 14 diarrhoeal hazards caused **666 million (483–884) illnesses,
265,000 (196,000–351,000) deaths, and 15.2 million (11.6–19.1) DALYs** from
**foodborne transmission alone**. Ranked by foodborne DALYs the top three
are ***Shigella* spp > *Campylobacter* > rotavirus** [2].

This is a **third ordering** — and it is a legitimately different question
("what does *food* carry?"), not an error. Greatest regional burden: the
African region, **773.5 DALYs (559.7–1033.3) per 100,000**. Global
foodborne DALY rate fell from 692.3 per 100,000 (2000) to 193.6 (2021), but
**not uniformly across hazards** [2].

---

## 3. The organisms the diarrhoea studies cannot see: helminths and the chronic protozoa

These rank near-zero in every table above and infect on the order of a
**billion people**. Their unit is prevalence, not attributable diarrhoea.

| Organism | Burden | Unit, year, population | Compartment | Right for this map? |
|---|---|---|---|---|
| ***Ascaris lumbricoides*** | **819.0 million** (771.7–891.6) infected | prevalence, 2010, global; from Global Atlas of Helminth Infection + GBD 2010 framework [17] | Larvae penetrate gut mucosa, migrate via liver and lungs, then adults dwell **luminally in the small intestine** [20] | ✓✓ **Yes** |
| ***Trichuris trichiura*** | **464.6 million** (429.6–508.0) infected | as above [17] | Larvae moult **within intestinal epithelial cells**; adults take up a **partially intracellular residency in the large intestine** [20] | ✗ **No — colon** |
| **Hookworm** (*Necator*, *Ancylostoma*) | **438.9 million** (406.3–480.2) infected; **65% of all 4.98 million STH YLDs** | as above [17] | Small intestine — **UNVERIFIED segment** in this pass | ~ likely yes |
| **All STH, re-estimated** | **642.72 million cases**, **1.38 million DALYs**; age-standardised prevalence **8,429.89 per 100,000** (7,697–9,362) | prevalence + DALYs, **2021**, 204 countries, GBD 2021 data [18] | — | — |
| ***Giardia duodenalis*** | **>300 million cases/yr** (WHO figure as cited in review); prevalence **2–3% in industrialised countries, up to 30% in low-income** | annual cases + prevalence, review published 2018 [45] | Proliferates **extracellularly and non-invasively in the small intestine** [45] | ✓✓ **Yes, and it is textbook duodenum/jejunum** |
| ***Entamoeba histolytica*** | A leading cause of severe diarrhoeal disease worldwide; **~10% of infections turn invasive**; **most infections are asymptomatic** | qualitative + proportion, reviews 2022 and 2026 [46][47] | **Amebic colitis** — trophozoites invade **colonic** mucosa [46][47] | ✗ **No — colon** |

**Three cautions on this block, all from the sources:**
1. **The STH numbers are 2010 vintage and the trend is steeply down.** The
   GBD 2021 re-estimate gives an **estimated annual percentage change of
   −4.03%** (95% CI −4.13 to −3.93) in age-standardised STH prevalence
   [18]. A 2014 paper's 819 million *Ascaris* is not a 2026 number.
2. ***Entamoeba* burden is the weakest row in this entire document.** I could
   not source a current global case or DALY count for amoebiasis in this
   session; the reviews are explicit that molecular diagnostics are
   "underutilised in many endemic settings" and that *E. histolytica* is
   morphologically identical to non-pathogenic *Entamoeba* species [46].
   Treat as **UNVERIFIED for magnitude**, verified for anatomy.
3. ***Giardia*'s "300 million"** is a WHO figure quoted inside a review [45],
   not a primary modelled estimate I pulled. Lower confidence than the STH
   or GBD rows.

---

## 4. High-income table — a different world, and the brief was right that it matters

### 4a. Community incidence, UK

**Unit: cases per 1,000 person-years in the community. Population: 6,836
prospectively followed UK participants registered with 88 GP practices,
April 2008 – August 2009 (IID2).** Source: Tam et al. [15].

| Organism | Community rate (per 1,000 py) | GP-consultation rate (per 1,000 py) |
|---|---|---|
| **All infectious intestinal disease** | **274** (254–296) | **17.7** (14.4–21.8) |
| **Norovirus** — commonest organism | **47** | **2.1** |
| ***Campylobacter*** — commonest bacterium | **9.3** | **1.3** |

**The single most important surveillance number in this pass:** there were
**147 community cases and 10 GP consultations for every one case reported to
national surveillance** [15]. Any national notification table understates
true incidence by roughly two orders of magnitude.

### 4b. Foodborne illness burden, USA

**Unit: estimated annual domestically acquired foodborne illnesses /
hospitalisations / deaths, adjusted for underreporting and underdiagnosis.
Population: USA, year 2019, 7 major pathogens.** Source: Scallan Walter et
al. [13].

| Organism | Illnesses | Hospitalisations | Deaths |
|---|---|---|---|
| **Norovirus** | **≈5.5 million** | 22,400 | — |
| ***Campylobacter*** spp. | **1.9 million** | 13,000 | — |
| **Nontyphoidal *Salmonella*** | **1.3 million** | 12,500 | **238 — leading cause of death** |
| *(all 7 pathogens)* | ≈9.9 M (5.9–15.4 M) | 53,300 (35,700–74,500) | 931 (530–1,460) |

The earlier 31-pathogen estimate (USA, methods of 2011) put **norovirus at
58% of all foodborne illnesses**, ahead of nontyphoidal *Salmonella* (11%),
*Clostridium perfringens* (10%) and *Campylobacter* (9%) — while the leading
causes of *death* were nontyphoidal *Salmonella* (28%), *Toxoplasma gondii*
(24%), *Listeria monocytogenes* (19%) and norovirus (11%) [14]. **The two
sets of estimates are not comparable to each other** (different methods,
different pathogen counts); the authors of the 2011 paper say so explicitly
[14].

### 4c. Laboratory-confirmed incidence, USA

**Unit: crude cases per 100,000 persons, domestically acquired. Population:
FoodNet historic catchment, 51.0 million persons (15% of the US population),
year 2023.** Source: FoodNet / MMWR [12].

| Organism | 2023 crude incidence /100,000 | Model-estimated (95% CrI) | vs 2016–2018 baseline (IRR) |
|---|---|---|---|
| ***Campylobacter*** | **20.6** | 19.32 (18.30–20.41) | **↑ 1.22** (1.15–1.30) |
| ***Salmonella*** | **14.2** | 13.92 (13.10–14.81) | stable 0.94 (0.87–1.01) |
| **STEC** | **5.3** | 5.15 (4.63–5.75) | **↑ 1.25** (1.10–1.42) |
| ***Shigella*** | **4.6** | 4.21 (3.42–5.26) | stable 1.02 (0.81–1.29) |
| ***Yersinia*** | **2.7** | 2.49 (2.18–2.85) | **↑ 3.47** (2.97–4.08) |
| ***Vibrio*** | **1.0** | 1.02 (0.90–1.16) | **↑ 1.64** (1.43–1.90) |
| ***Cyclospora*** | **0.7** | 1.25 (0.73–2.87) | **↑ 5.06** (2.65–12.43) |
| ***Listeria*** | **0.31** | 0.29 (0.26–0.34) | stable 1.13 (0.98–1.32) |

**Two large caveats, both from the source itself:**
1. **FoodNet does not count enteric viruses.** It monitors eight bacterial
   and parasitic pathogens plus paediatric HUS [12]. Reading this table as
   "the commonest US gut pathogens" is the single easiest way to get the
   high-income picture wrong — compare §4b, where norovirus is three times
   *Campylobacter*.
2. **The increases are substantially a diagnostics artefact.** In 2023,
   **78% of all bacterial infections were diagnosed by culture-independent
   diagnostic tests (CIDTs)**, 46% by CIDT only; reflex-culture success
   fell, so speciation collapsed (e.g. *Shigella* speciated 65% → 41%,
   *Campylobacter* 33% → 26%). CDC states the lack of progress toward
   reduction goals "might reflect changing diagnostic practices rather than
   an actual increase in incidence" [12]. **Rising surveillance counts are
   not rising biology.**

### 4d. Europe

**Unit: rank by reported human cases. Population: EU/EEA, 27 Member States
plus UK (Northern Ireland), year 2024.** Source: EFSA/ECDC EU One Health
Zoonoses Report [16].

**Campylobacteriosis #1 > salmonellosis #2 > STEC #3 > listeriosis #4** (and
listeriosis the most severe, with the highest hospitalisation percentage and
case-fatality rate). Cases rose significantly over the previous five years
for campylobacteriosis, salmonellosis, listeriosis and STEC [16]. The 2023
report gives the same top three with *Yersinia enterocolitica* and
*L. monocytogenes* next [16-2023]. Campylobacteriosis is estimated at
**around 9 million cases annually in the EU** [38].

### 4e. The one organism whose incidence is the *same* everywhere

**Norovirus.** A 233-country simulation model found **essentially identical
incidence in low/middle-income and high-income countries: 10,148 vs 9,935
illnesses per 100,000 persons**, while high-income countries generated 62%
of global *health-system costs* [44]. Global burden $4.2 billion
(3.2–5.7 bn) direct health-system cost and $60.3 billion (44.4–83.4 bn)
societal cost per year; **$39.8 bn of the societal cost falls on children
under 5** [44].

**This is the global/high-income split in one line.** Almost every other
organism in this document is hugely skewed by income. Norovirus is not. If
the game wants one organism that a player in Berlin and a player in Dhaka
both genuinely meet at the same rate, it is norovirus.

---

## 5. Age skew and seasonality

### 5a. Age

| Finding | Number | Source |
|---|---|---|
| Diarrhoeal DALYs concentrated in infancy | 30.9 M of 59.0 M global diarrhoeal DALYs (2021) are in children <5 | [1] |
| Foodborne enteric hazards, <5 vs ≥5 | **mortality rates 7.1× higher** and **DALY rates 18.9× higher** in children <5 than in people ≥5 (2021, 194 countries) | [2] |
| *Shigella* is a **second-year** organism | 26.1 episodes/100 child-yr overall but **41.3 in year 2** vs 1st-year rank outside the top 5; "dominant etiology among 12–23 and 24–35-month-olds, causing approximately one-third to one-half of MSD" | [6][11] |
| Rotavirus is a **first-year** organism | leading aetiology among 6–11-month-olds pre-vaccine, ~22–28% of MSD | [11] |
| Norovirus GII peaks at **6–11 months** | across all eight MAL-ED sites | [41] |
| Norovirus is near-universal by 2 years | **89% of children experienced ≥1 norovirus infection before 24 months**; 22.7% of all diarrhoeal stools norovirus-positive (1,457-child birth cohort, 8 countries) | [41] |
| Shigellosis is **not** an infant disease in practice | MAL-ED prediction-score derivation: diarrhoeal stools in children ≤6 months were **never** *Shigella*-positive | [6] |
| **High income is bimodal, not infant-skewed** | FoodNet 2023: children ≤4 y were 11.9% of cases but 5.4% of the catchment population; adults ≥60 y were 29.2% of cases vs 23.5% of the population | [12] |
| Norovirus cost per illness is highest in the **old** | highest cost per illness among adults ≥55 y in some WHO regions | [44] |

**Round-design read:** the global curve is a steep infant spike that decays.
The high-income curve is a **U** — toddlers and the elderly, with a trough
in between. Those are two different difficulty curves.

### 5b. Seasonality

| Organism | Pattern | Number | Source |
|---|---|---|---|
| **Norovirus** | **Winter, strongly** (temperate northern hemisphere) | **78.9% of cases and 71.0% of outbreaks occur in cool months**; 52.7% of cases in winter months proper (systematic review, 38 case + 29 outbreak series, 1997–2011) | [35] |
| **Norovirus, USA** | Season **October–May**, median duration **24 weeks** | 24,995 single-state outbreaks, Aug 2009 – Jul 2019; west-to-east gradient — earliest onset (Oct) and latest offset (May) in the west, latest onset (Dec) and earliest offset (Apr) in the northeast | [36] |
| **Norovirus, England & Wales** | Driven by **cold and dry** | rate ratio **0.85** (0.83–0.86) per 1 °C rise in mean temperature over the previous 7 weeks; RR 0.980 per 1% relative-humidity rise over 5 weeks; **new GII.4 variant emergence RR 1.16** (1.10–1.22) | [37] |
| ***Campylobacter*** | **Early-to-mid summer**, strikingly | Nordic countries peak **weeks 29–32**; UK, Ireland, Hungary, Slovakia peak **week 24**; positively associated with temperature and (less) precipitation (TESSy, Europe, 2008–2016) | [38] |
| ***Campylobacter***, earlier series | Same shape, stable for decades | peak week 22 (Wales), 26 (Scotland), 30 (Finland), 32 (Denmark), 33 (Sweden) | [39] |
| ***Campylobacter***, England & Wales | Late-spring rise tracks temperature with a **2-week lag**; +0.175 cases/100,000/week for weeks 17–24; temperature explains **33.3%** of expected cases | [38-adj] |
| **Rotavirus** | **Drier, cooler months — and in antiphase with the bacteria** | across all 7 GEMS sites, rotavirus peaked in drier "winter" months and was **out of phase with bacterial pathogens**, which peaked in hotter/rainier monsoon or summer seasons | [40] |
| **Everything, caveat** | **No generalisation holds across sites** | "a complex epidemiology of pathogens that does not reveal an easy generalization that is consistent across all sites… the necessity of local data" | [40] |

**The one clean, universal, usable rhythm: viruses and bacteria are in
antiphase.** A winter wave and a summer wave are genuinely different
enemies, and that is sourced [40], not invented.

---

## 6. The rotavirus-vaccine effect: who moved up

This is the best-documented aetiology shift in the field, and it has four
independent confirmations.

**1. MAL-ED, within one study, vaccinated vs unvaccinated sites** (2009–2012;
rotavirus vaccine already in national programmes in Brazil, Peru, South
Africa) [6]:
- Where rotavirus vaccine **had not** been introduced: rotavirus was
  **8.9%** of all attributable diarrhoea — **second** highest.
- Where it **had**: **4.2%** — **eighth** highest.
- The earlier non-qPCR MAL-ED analysis saw the same thing: rotavirus had the
  highest AF at unvaccinated sites and only the **fifth** highest at
  vaccinated sites [5].

**2. GPDS, the same countries over time** (38 sites, 31 LMICs, 2017–2022)
[10]:
- Rotavirus AF fell **36.7%** (28.7–46.7) in 2017–18 → **26.7%**
  (20.7–34.1) in 2021–22.
- Norovirus AF rose slightly, **6.2%** (4.7–7.7) → **7.3%** (5.0–9.2).
- *Shigella* global burden **stable**; adenovirus 40/41 **volatile**, peaking
  at 11.9% (5.9–17.5) in 2019–20.
- **In the subset of countries that had introduced rotavirus vaccine**, the
  2021–22 leaders were rotavirus **18.4%** (15.8–21.5) and ***Shigella*
  16.4%** (11.8–21.1) — i.e. *Shigella* closes to within a few points.
- Regionally in 2021–22: **norovirus predominated in Central and South
  America**; ***Shigella* was the leading aetiology in South Asia**.

**3. Harmonised reanalysis across five multisite studies** (GEMS, MAL-ED,
VIDA, ABCD, EFGH; children 6–35 months with acute MSD) [11]:
- Pre-vaccine, rotavirus had the highest incidence and was the leading
  aetiology among 6–11-month-olds, **~22–28% of MSD**.
- Post-introduction, rotavirus average AF fell to **10–11%** in both Africa
  and Asia.
- ***Shigella*** became "the dominant etiology among 12–23 and
  24–35-month-olds, causing approximately one-third to one-half of MSD."
- Authors' own summary: **"burden shifting toward *Shigella* following
  rotavirus vaccine introduction."**
- Also, directly relevant to class design: **case-fatality ratios were
  higher for bacterial (2.52%) and protozoal (3.55%) than viral (1.42%)
  pathogens**; 90-day mortality fell from 2.21% (GEMS) to 0.30% (EFGH).

**4. High-income: norovirus took the top slot outright.**
- Active surveillance in three US counties, 2009–2010, children <5 with
  acute gastroenteritis: norovirus in **21%** (278/1,295) vs rotavirus
  **12%** (152/1,295). Conclusion: **"Since the introduction of rotavirus
  vaccines, norovirus has become the leading cause of medically attended
  acute gastroenteritis in U.S. children"** — ~1 million healthcare visits
  and >$273 M treatment cost annually [42].
- New Vaccine Surveillance Network, US children <2 y, 2012: norovirus 23.0%
  of AGE cases vs rotavirus **3.0%**; norovirus "the leading AGE-causing
  pathogen" [43].

**Answer to the brief's question, in one sentence: globally *Shigella* moved
up (and adenovirus 40/41 and sapovirus were revealed as large by better
diagnostics); in high-income countries norovirus moved up.** Those are two
different winners, and neither of them is in the Built roster.

---

## 7. Three methodological cautions, with evidence

### 7.1 Detection ≠ causation — and here is the size of the gap

**The number to quote.** In MAL-ED, across 6,625 diarrhoeal and **30,968
non-diarrhoeal surveillance stools** from 1,715 children aged 0–24 months,
tested by qPCR for 29 enteropathogens:

> **Diarrhoeal stools contained a mean of 3.4 pathogens (SD 2.0).
> Non-diarrhoeal stools contained a mean of 2.5 (SD 1.8)** (p<0.001) [6].

A healthy, asymptomatic child in these cohorts carries, on average,
**two and a half simultaneous detectable enteropathogens.** Detection is
close to the baseline state of the gut, not evidence of disease. Supporting
figures:

- In the earlier MAL-ED analysis, "pathogen detection was **common in
  non-diarrhoeal stools** but was higher with diarrhoea" — which is exactly
  why every one of these studies reports **attributable** fractions derived
  from a case/control or quantity model rather than raw prevalence [5].
- 22,846 non-diarrhoeal stools from 1,253 MAL-ED children were screened for
  a panel of **40 enteropathogens**; the cohort is characterised as having
  "frequent enteric infections and high levels of both intestinal and
  systemic inflammation" **in the absence of diarrhoea** [49].
- Recent **asymptomatic** enteropathogen infections were independently
  associated with decreased mannitol excretion and higher lactulose:mannitol
  ratios — i.e. **measurable barrier damage with no symptoms** [50].
- Even with a stringent per-episode quantitative cutoff, **only 33.9% of
  episodes had one identified pathogen and 9.7% had two or more** — so over
  half of episodes get no confident per-case aetiology at all [6]. In GEMS,
  **42.5% of cases had one diarrhoea-associated pathogen and 38.9% had two
  or more** [4].
- Healthy-control detection rates, measured directly: norovirus in **4%** of
  493 healthy US controls [42]; adenovirus **16.0%**, parechovirus A
  **16.0%**, enterovirus **9.0%**, norovirus **6.6%**, sapovirus 3.0% in 272
  age-matched healthy US controls [43]; rotavirus in **15%** (160/1,046) of
  GEMS Mozambique community controls vs 44% of MSD cases [48].

**Design consequence.** A wave built from "detection prevalence" would put
the wrong organisms on the board and would be mechanically dishonest: the
game's premise is that an organism *does something to tissue*. Use
**attributable** numbers (§1, §2a, §2b) throughout. If a future round wants
a "colonised but not yet harmful" state, §7.1 is the real-biology warrant
for it — and `GAME_DESIGN.md` §6b barrier colonisation is already close to
the right shape for it.

### 7.2 Small bowel vs colon — the *C. difficile* rejection generalises, and it bites hard

`PATHOGEN_ROSTER.md` rejected *C. difficile* because it is colonic and the
map is small intestine. **Applied consistently, that rule disqualifies or
demotes some of the highest-burden organisms on earth.**

**Correctly small-intestinal (map-legal):**

| Organism | Evidence |
|---|---|
| **Sapovirus** | Replicates in differentiated human intestinal enteroids from **jejunum, duodenum and ileum, but not from the colon**; requires bile acids. The authors state the tropism "is restricted to the small intestine, does not occur in the colon" [25] |
| **Norovirus** | VP1 detected throughout all segments of the small intestine; **RdRp and VPg (active-replication markers) with VP1 in duodenal and jejunal enterocytes** [23]; negative-sense viral RNA predominantly in intestinal epithelium, with **chromogranin-A⁺ enteroendocrine cells** permissive, in jejunal and ileal tissue [24] |
| **Adenovirus 40/41** | Prototype and clinical isolates of enteric HAdV replicate productively in human **enteroids** (small-intestinal primary epithelium); HAdV-41p is **not** neutralised by the Paneth α-defensin HD5 while respiratory Ad5 is [33]; F41's capsid exterior is structurally adapted to "a fundamentally different tissue tropism" from respiratory types [34] |
| **ETEC** | Heat-labile toxin "drives enteropathic changes in **small intestinal epithelia**", suppressing HNF4/SMAD4 and disrupting microvillus architecture [29] |
| ***Salmonella* Typhimurium** | "Penetrating the intestinal epithelium of the **small bowel**"; within 30 min invasive *S.* Typhimurium **exclusively entered M cells of the Peyer's-patch follicle-associated epithelium** [30]; non-invasive mutants are avirulent precisely because they cannot enter and destroy **ileal** Peyer's-patch M cells [31-adj] |
| ***Giardia duodenalis*** | Proliferates **extracellularly and non-invasively in the small intestine**; chronic infection produces villus and crypt atrophy, enterocyte apoptosis and barrier disruption [45] |
| ***Ascaris lumbricoides*** | Larvae penetrate the mucosa and migrate via liver and lungs, then return as **luminal-dwelling small-intestinal adults** [20] |
| **Rotavirus, *V. cholerae*, A/E *E. coli*** | Already built; anatomy established in the prior passes. **Not re-verified in this pass.** |

**Burden is real, anatomy is wrong — flag these:**

| Organism | Its burden rank | The anatomy problem |
|---|---|---|
| ***Shigella*** | **#1 global community (26.1/100 child-yr) [6]; #1 GEMS MSD [4]; #1 WHO foodborne DALYs [2]; 24.0% fatal PAF in <5s [1]** | **Colonic and rectal.** "Shigellae have the capacity to invade the **colonic and rectal** epithelium in humans"; after oral ingestion the bacteria "pass through the stomach and the **small intestine** before reaching the **colon** where they invade the mucosa" [21]. *Shigella* "invades the **colonic mucosa** but does not penetrate further" [22]. Crucially, **"the basis for organ specificity of shigellosis to the rectal and colonic mucosae is not understood"** [21] — so there is no mechanism to hand-wave it onto the jejunum with. **This is the single largest burden/anatomy conflict in the document.** |
| ***Campylobacter jejuni*** | #8 global community [6]; **#1 high-income bacterium in the UK community (9.3/1,000 py) [15], the USA (20.6/100,000 [12]; 1.9 M foodborne illnesses [13]) and the EU (#1 reported zoonosis [16])** | **Presents as colitis in humans.** 22 patients with *C. jejuni*-positive stools and colitis showed the histology of acute infectious colitis, and immunohistochemistry plus electron microscopy demonstrated **invasion of the colonic mucosa** [28]. Murine models do show stable colonisation of stomach, duodenum and ileum — but the colonic load is where disease correlates [28-adj]. **Mixed; leans colon.** |
| ***Entamoeba histolytica*** | A leading cause of severe diarrhoeal disease worldwide [46] | **Amebic colitis.** Trophozoites must "cross mucus, epithelium, connective tissue and possibly blood" of the **colon** [47]; the named disease is colitis [46]. ✗ |
| ***Trichuris trichiura*** | **464.6 million infected** [17] | Adults take up "a partially intracellular residency in the **large intestine**" [20]. ✗ |
| **STEC / EHEC** | #3 in both FoodNet (5.3/100,000) [12] and the EU [16] | **Genuinely two-compartment, and the ileal half is good news for this map.** Human in-vitro organ culture across a panel of strains found tropism for the **follicle-associated epithelium of distal ileal Peyer's patches**, plus ileal villous surfaces in four of six strains, with **minimal adhesion to the transverse colon** — colonic colonisation in only 4 of 113 IVOCs (3.5%) [31]. But colonic biopsies *do* support typical A/E lesion formation, and "pathology is most pronounced in the **colon**" [32]. **Reading: entry in the terminal ileum, pathology in the colon.** The roster's EHEC shortlist entry survives if it is staged as an **ileal-band entry event**. |
| ***Clostridioides difficile*** | — | Already rejected on exactly this ground. The rejection was correct and this pass does not reopen it. |

**One substantive caution on *Cryptosporidium*.** It occupies "a unique
intracellular niche at the **brush border** of intestinal epithelial cells"
[26] — mechanically excellent for this game, since the brush border *is* the
wall. But the **segment is host- and species-dependent**: in neonatal mice
both *C. hominis* and *C. parvum* primarily infected **ileum, caecum and
colon**, whereas in neonatal rats they primarily colonised **duodenum and
jejunum** [27]. I did **not** source a human-tissue segment determination in
this pass. **UNVERIFIED for the human small bowel specifically** — worth one
targeted lookup before it is built.

### 7.3 Burden measured as what? — where the orderings disagree

Same organisms, five metrics, five different #1s. **This is not noise; each
metric answers a different question, and a round script should pick one
deliberately.**

| Metric | Unit | Year / population | **#1** | **#2** | **#3** | Source |
|---|---|---|---|---|---|---|
| **Community attributable incidence** | episodes / 100 child-yr | 2009–12, 8 LMIC sites, 0–24 mo | ***Shigella*** | **Sapovirus** | Rotavirus | [6] |
| **MSD attributable fraction** | AF of moderate-to-severe diarrhoea | 2007–11, 7 LMIC sites, <5 y | ***Shigella*** | Rotavirus | Adenovirus 40/41 | [4] |
| **Hospitalised attributable fraction** | AF of hospitalised diarrhoea | 2017–22, 31 LMICs, <5 y | **Rotavirus** | *Shigella* | Adenovirus 40/41 | [10] |
| **Deaths** | fatal PAF, all ages | 2021, 204 countries | **Rotavirus** (15.2%) | Norovirus (10.6%) | *Cryptosporidium* (10.2%) | [1] |
| **Foodborne DALYs** | DALYs, foodborne share only | 2021, 194 countries | ***Shigella*** | *Campylobacter* | Rotavirus | [2] |
| **High-income illnesses** | annual illnesses, USA | 2019, USA | **Norovirus** (5.5 M) | *Campylobacter* (1.9 M) | *Salmonella* (1.3 M) | [13] |
| **High-income lab-confirmed incidence** | cases / 100,000 | 2023, USA 51.0 M | ***Campylobacter*** (20.6) | *Salmonella* (14.2) | STEC (5.3) | [12] |
| **Global prevalence (chronic)** | people infected | 2010 (trending down 4%/yr) | ***Ascaris*** (819 M) | *Trichuris* (465 M) | Hookworm (439 M) | [17][18] |

Four specific disagreements worth naming:

1. **Severity inverts rank.** Rotavirus climbs as you move from community →
   MSD → hospitalised → deaths. *Shigella* dominates the community and
   foodborne-DALY views. Within MAL-ED alone, ***Shigella* had the highest
   attributable burden of moderate-to-severe diarrhoea while rotavirus had
   the highest burden of *severe* diarrhoea** by modified Vesikari score
   [6]. One study, one dataset, two different winners by severity threshold.
2. **Outbreak counts rank differently again.** FoodNet 2023: *Listeria*
   caused only 163 infections but **97.5% were hospitalised and 22.1% died**
   [12]. *Cyclospora* was **7.6% outbreak-associated**; *Campylobacter* only
   **0.3%** [12]. An outbreak-count metric would surface organisms that
   almost nobody gets.
3. **Lab-confirmed incidence is a diagnostics instrument, not a biology
   instrument** — §4c, and IID2's 147:1 community-to-surveillance ratio
   [15].
4. **Helminths are invisible in four of the eight metrics** and #1 in the
   eighth, because the metrics measure acute diarrhoea and helminths cause
   chronic morbidity.

---

## 8. Closing recommendation

### 8a. The burden-first shortlist: 12 organisms

Ranked by the strongest defensible claim to "common," **with the
small-intestine anatomy filter applied** (because the map is §1a's
duodenum/jejunum/ileum, and the project already enforced that filter once).

| # | Organism | Why it is on this list | Compartment |
|---|---|---|---|
| 1 | **Rotavirus** | #1 by deaths globally (fatal PAF 15.2% all ages, 35.2% in <5s) [1]; still #1 hospitalised aetiology in LMICs even post-vaccine (AF 32.5%) [10] | ✓ small intestine |
| 2 | **Norovirus** | **The only organism with the same incidence in rich and poor countries** (10,148 vs 9,935 /100,000) [44]; #1 illness cause in the USA (5.5 M/yr) [13] and #1 organism in the UK community (47/1,000 py) [15]; 89% of children infected by 24 months [41]; #2 by global diarrhoeal deaths (10.6%) [1] | ✓ duodenum/jejunum [23][24] |
| 3 | **Sapovirus** | **#2 in the entire global community ranking (22.8/100 child-yr)** [6] and almost nobody has heard of it; **strictly small-intestinal, explicitly not colonic** [25] | ✓✓ confirmed, strictest fit |
| 4 | **Adenovirus 40/41** | #3 GEMS MSD [4], #3 hospitalised AF [10], #4 global community [6]; 23.8% fatal PAF in <5s [1]; detection 19.1% vs attribution 8.6% makes it the perfect teaching case for §7.1 [10] | ✓ small-intestinal enteroids [33] |
| 5 | **ETEC** | #5 global community (18.8) [6], #4 GEMS MSD [4], >10% fatal PAF in <5s [1]; LT acts directly on the small-intestinal brush border [29] | ✓ |
| 6 | **Astrovirus** | #7 global community (15.0/100 child-yr) [6] — **far higher burden than the mechanics-first pass assumed**, where it ranked 4th of 6 viruses on novelty alone | **compartment UNVERIFIED** |
| 7 | ***Cryptosporidium*** | #3 by global diarrhoeal deaths (10.2% all-ages fatal PAF) [1]; highest protozoal case-fatality class (3.55%) [11]; brush-border niche [26] | ~ segment UNVERIFIED for humans [27] |
| 8 | ***Giardia duodenalis*** | >300 M cases/yr [45]; the **textbook duodenum/jejunum organism**, extracellular and non-invasive, causing villus atrophy [45]; associated with growth faltering in MAL-ED **without** any inflammation-biomarker mediation [49] | ✓✓ |
| 9 | ***Salmonella*** Typhimurium | #2 high-income on every measure (1.3 M US illnesses, 238 deaths — **the leading foodborne cause of death** [13]; #2 EU zoonosis [16]; 14.2/100,000 FoodNet [12]); enters at **ileal Peyer's-patch M cells** [30] | ✓ |
| 10 | ***Ascaris lumbricoides*** | **819 M people** [17]; **luminal small-intestinal adult** [20]; zero representation in the current roster and zero representation of helminths at all | ✓✓ |
| 11 | **Hookworm** | **439 M people and 65% of all 4.98 M STH YLDs** — the single largest helminth *disability* burden [17] | ~ segment UNVERIFIED |
| 12 | **STEC / EHEC** | #3 in both FoodNet (5.3/100,000, rising) [12] and the EU [16]; **initial colonisation is distal-ileal FAE with minimal colonic adhesion ex vivo** [31] | ✓ for entry, ✗ for pathology [32] |

**Two more if 14 is wanted:** ***Campylobacter jejuni*** — overwhelmingly
the #1 bacterium in every high-income dataset [12][15][16] and #8 globally
[6], but see the colitis problem [28]; and **tEPEC** (#10 global community,
>10% fatal PAF in <5s [1][6]), though the roster's built A/E *E. coli*
already covers its mechanics.

### 8b. Where this disagrees with the mechanics-first roster — plainly

The brief said disagreement is a finding, not a problem. Here it is.

**1. The biggest disagreement: sapovirus is absent from the project entirely,
and it is the #2 organism in the world by community attributable
incidence.** It does not appear in `PATHOGEN_ROSTER.md`'s built list, its
shortlist, or its cut list. The viruses pass cut sapovirus with one line —
*"plays as norovirus with lower numbers"* — on mechanics. The numbers are
not lower. **22.8 attributable episodes per 100 child-years vs norovirus's
15.4, in the same study, same children, same unit** [6]; second only to
*Shigella* across both years and second-ranked in the second year of life.
It was missed historically because **it was not even tested for in the
original MAL-ED microbiology** [6]. And it is the *best* anatomical fit of
any candidate: it replicates in duodenal, jejunal and ileal enteroids and
**does not replicate in the colon** [25]. The mechanics argument may still
be right — but it was made against a burden figure nobody had, and the
figure is large.

**2. *Shigella* is the roster's star shortlist bacterium and it is the
roster's worst anatomy violation.** It is #1 on three of five global
metrics [2][4][6] and 24.0% of under-5 diarrhoeal deaths [1]. It is also
**colonic and rectal, with the organ specificity explicitly unexplained in
the literature** [21]. The project rejected *C. difficile* on precisely
this ground. **Consistency requires one of two decisions from the Director:
either *Shigella* is reclassified alongside *C. difficile* as banked for a
colon map, or the anatomy rule is formally relaxed to "the small intestine
is where we show it, not where it clinically dominates."** The second is a
defensible game decision — *Shigella* does transit the small intestine
before reaching the colon [21], and its actin-tail cell-to-cell spread is
genuinely unanswered in the roster — but it should be made explicitly, not
by accident. The same decision covers *Campylobacter* [28], *Entamoeba*
[46][47] and *Trichuris* [20].

**3. The roster has no helminths and no protozoa at all.** Four passes
produced a roster of viruses and bacteria. Meanwhile **~1.7 billion STH
infections** [17] and **>300 million annual *Giardia* cases** [45] sit
outside it, and *Ascaris* and *Giardia* are both **better anatomical fits
for a small-intestine map than anything currently shortlisted**. *Giardia*
in particular is an extracellular, non-invasive, villus-atrophy organism
[45] — mechanically unlike every single enemy on the board, including
*V. cholerae* (which subverts without killing but does not strip villi).
Burden and novelty point the same way here.

**4. CMV is the roster's #2 virus and it does not belong on this list on
burden grounds.** It earns its slot as a *boss* via four-mechanism MHC-I
blindness — a design argument this pass does not contest. But it appears in
**none** of GBD 2021 [1], WHO FERG [2], GEMS [3][4], MAL-ED [5][6], VIDA
[8], GPDS [10], FoodNet [12] or the EU zoonoses report [16]. It is a
reactivation disease of the immunosuppressed. **A boss can be rare. A wave
enemy cannot.** The roster already treats it this way ("it must be a boss
and never a wave enemy") — this pass simply confirms that framing is the
only one the epidemiology supports.

**5. The roster's rotavirus abstraction choice is now doubly justified.** It
was already the #1 virus on mechanics. It is also #1 by deaths on every
global metric [1][10] — so it is correctly the first thing a player meets.

**6. Two corrections to the shortlist's stated rationale, on burden grounds.**
- **Astrovirus was ranked 4th of 6 on novelty.** On burden it is **#7
  globally at 15.0 attributable episodes per 100 child-years** [6] — above
  *Campylobacter*, *Cryptosporidium* and tEPEC, and essentially level with
  norovirus. Its rank should rise.
- **Hepatitis A and *Vibrio cholerae* are the two roster entries with the
  weakest burden support in this pass.** *V. cholerae* O1 appears in GEMS
  only as important "in selected sites" [3] and is one of 14 WHO FERG
  hazards without a top-three DALY position [2]; hepatitis A appears in none
  of the diarrhoea-aetiology studies at all, because it is not a diarrhoeal
  disease. Both are defensible on mechanics. Neither is defensible as
  "common."

### 8c. What to do with seasonality and age, concretely

- **Viruses and bacteria are in antiphase** across all seven GEMS sites [40]
  — so a **winter virus wave** and a **summer bacterium wave** are
  biologically honest round themes, not flavour.
- **The age curve differs by income setting**: a steep infant spike globally
  [1][2][11], a **U-shaped toddler-and-elderly** curve in high-income
  countries [12][44]. If the game ever wants two campaigns, that is the
  real difference between them.
- **Rotavirus is the year-1 enemy; *Shigella* is the year-2 enemy**
  [6][11] — a genuine, sourced curriculum ordering that `GAME_DESIGN.md`
  §5's "round is a CURRICULUM" rework could use directly.

### 8d. Weakest rows in this document — read these with suspicion

Ordered worst first.

1. ***Entamoeba histolytica* magnitude** — no current global case or DALY
   estimate sourced. Anatomy verified, burden **UNVERIFIED** [46][47].
2. **Astrovirus compartment** — burden is solid [6], anatomy was not sourced
   this pass. Needs one lookup before it can be map-placed.
3. ***Cryptosporidium* human segment** — brush-border niche verified [26],
   but the ileum-vs-duodenum question resolved only in rodents, and
   **oppositely in mice and rats** [27].
4. **Hookworm segment** — small intestine assumed, not sourced.
5. ***Giardia*'s ">300 million cases/yr"** — a WHO figure quoted inside a
   review [45], not a primary modelled estimate.
6. **GBD 2021 per-pathogen non-fatal PAFs and DALYs** — only the fatal PAFs
   were extractable from the abstract. Everything else from GBD in this
   document is the envelope, not the split [1].
7. **Reference [11] (Quesada et al., harmonised five-study reanalysis)** —
   sourced via Consensus, **journal and DOI not resolved on PubMed** this
   session; the record shows 2026, 0 citations. Its content is the single
   cleanest statement of the rotavirus-vaccine shift, and it is corroborated
   independently by [6] and [10], so the *finding* is safe even if this
   particular citation is not yet firm.
8. **The 2010-vintage STH numbers** [17] against a −4.03%/yr trend [18] —
   use the 2021 GBD aggregate (642.72 M cases, 1.38 M DALYs) for anything
   presented as current.
9. **Rotavirus, *V. cholerae* and A/E *E. coli* compartments** — taken from
   the prior passes and **not re-verified here**. They are almost certainly
   right; they are simply outside this pass's evidence.

---

## 9. References

PubMed-sourced entries are cited with DOI links per the PubMed tool's
attribution requirement (*according to PubMed*). Entries retrieved via
Consensus and not resolved on PubMed carry a Consensus record link instead.

- [1] GBD 2021 Diarrhoeal Diseases Collaborators (2025) *Global, regional, and national age-sex-specific burden of diarrhoeal diseases, their risk factors, and aetiologies, 1990–2021, for 204 countries and territories: a systematic analysis for the Global Burden of Disease Study 2021.* Lancet Infect Dis 25(5):519-536. [DOI](https://doi.org/10.1016/S1473-3099(24)00691-1)
- [2] Majowicz S.E. et al. (2026) *WHO estimates of the global, regional, and national burden of 14 foodborne diarrhoeal enteric hazards, 2000–21: an updated data synthesis.* Lancet Glob Health 14(8):103997. [DOI](https://doi.org/10.1016/j.langlo.2026.103997)
- [3] Kotloff K.L. et al. (2013) *Burden and aetiology of diarrhoeal disease in infants and young children in developing countries (the Global Enteric Multicenter Study, GEMS): a prospective, case-control study.* Lancet 382(9888):209-22. [DOI](https://doi.org/10.1016/S0140-6736(13)60844-2)
- [4] Liu J., Platts-Mills J.A. et al. (2016) *Use of quantitative molecular diagnostic methods to identify causes of diarrhoea in children: a reanalysis of the GEMS case-control study.* Lancet 388(10051):1291-301. [DOI](https://doi.org/10.1016/S0140-6736(16)31529-X)
- [5] Platts-Mills J.A. et al. (2015) *Pathogen-specific burdens of community diarrhoea in developing countries: a multisite birth cohort study (MAL-ED).* Lancet Glob Health 3(9):e564-75. [DOI](https://doi.org/10.1016/S2214-109X(15)00151-5)
- [6] Platts-Mills J.A. et al. (2018) *Use of quantitative molecular diagnostic methods to assess the aetiology, burden, and clinical characteristics of diarrhoea in children in low-resource settings: a reanalysis of the MAL-ED cohort study.* Lancet Glob Health 6(12):e1309-e1318. [DOI](https://doi.org/10.1016/S2214-109X(18)30349-8)
- [7] Kotloff K.L. et al. (2019) *The incidence, aetiology, and adverse clinical consequences of less severe diarrhoeal episodes among infants and children residing in low-income and middle-income countries: a 12-month case-control study as a follow-on to GEMS.* Lancet Glob Health 7(5):e568-e584. [DOI](https://doi.org/10.1016/S2214-109X(19)30076-2)
- [8] Buchwald A.G. et al. (2023) *Etiology, Presentation, and Risk Factors for Diarrheal Syndromes in 3 Sub-Saharan African Countries After the Introduction of Rotavirus Vaccines From the Vaccine Impact on Diarrhea in Africa (VIDA) Study.* Clin Infect Dis 76(Suppl 1):S12-S22. [DOI](https://doi.org/10.1093/cid/ciad022)
- [9] Powell H. et al. (2023) *A Description of the Statistical Methods for the Vaccine Impact on Diarrhea in Africa (VIDA) Study.* Clin Infect Dis 76(Suppl 1):S5-S11. [DOI](https://doi.org/10.1093/cid/ciac968)
- [10] Soeters H.M. et al. (2026) *Etiology and incidence of diarrhea requiring hospitalization in children under 5 years of age in 31 low- and middle-income countries: Findings from the Global Pediatric Diarrhea Surveillance network, 2017-2022.* PLOS Glob Public Health 6(10):e0006655. [DOI](https://doi.org/10.1371/journal.pgph.0006655)
- [11] Quesada M.G. et al. (2026) *Geographic and temporal trends in etiology-specific diarrhea burden among children in low-resource settings* (harmonised reanalysis of GEMS, MAL-ED, VIDA, ABCD and EFGH). **Journal and DOI UNVERIFIED in this session.** [Consensus record](https://consensus.app/papers/details/f820017b48075a60821606a27ec66532/)
- [12] Shah H.J. et al. (2024) *Reported Incidence of Infections Caused by Pathogens Transmitted Commonly Through Food: Impact of Increased Use of Culture-Independent Diagnostic Tests — Foodborne Diseases Active Surveillance Network, 1996–2023.* MMWR Morb Mortal Wkly Rep 73(26):584-593. [DOI](https://doi.org/10.15585/mmwr.mm7326a1)
- [13] Scallan Walter E.J. et al. (2025) *Foodborne Illness Acquired in the United States — Major Pathogens, 2019.* Emerg Infect Dis 31(4):669-677. [DOI](https://doi.org/10.3201/eid3104.240913)
- [14] Scallan E. et al. (2011) *Foodborne Illness Acquired in the United States — Major Pathogens.* Emerg Infect Dis 17(1):7-15. [Consensus record](https://consensus.app/papers/details/9801cfd413d7513ea36f8ab858579de5/)
- [15] Tam C.C. et al. (2012) *Longitudinal study of infectious intestinal disease in the UK (IID2 study): incidence in the community and presenting to general practice.* Gut 61(1):69-77. [DOI](https://doi.org/10.1136/gut.2011.238386)
- [16] EFSA and ECDC (2025) *The European Union One Health 2024 Zoonoses Report.* EFSA Journal. [Consensus record](https://consensus.app/papers/details/b461ef1a973e57f9b7638c28824e41b4/) — and [16-2023] EFSA/ECDC (2024) *The European Union One Health 2023 Zoonoses report.* [Consensus record](https://consensus.app/papers/details/acc6d43999965e9081fd5f67f7005e68/)
- [17] Pullan R.L., Smith J.L., Jasrasaria R., Brooker S.J. (2014) *Global numbers of infection and disease burden of soil transmitted helminth infections in 2010.* Parasit Vectors 7:37. [DOI](https://doi.org/10.1186/1756-3305-7-37)
- [18] Chen J. et al. (2024) *Global burden of soil-transmitted helminth infections, 1990–2021.* Infect Dis Poverty. [Consensus record](https://consensus.app/papers/details/ff2af9a038115581b94e83152245750c/)
- [19] Jourdan P.M. et al. (2018) *Soil-transmitted helminth infections.* Lancet. [Consensus record](https://consensus.app/papers/details/efd9e56218245cc5b03a2b83e25450ee/)
- [20] Else K.J. et al. (2020) *Whipworm and roundworm infections.* Nat Rev Dis Primers. [Consensus record](https://consensus.app/papers/details/6d480e4013d85254aac87ef573376a8e/)
- [21] Sansonetti P.J. (2001) *Rupture, invasion and inflammatory destruction of the intestinal barrier by Shigella…* FEMS Microbiol Rev. [Consensus record](https://consensus.app/papers/details/77064276b5ca5fcea91096311685aa5c/)
- [22] Carayol N. & Tran Van Nhieu G. (2013) *The inside story of Shigella invasion of intestinal epithelial cells.* Cold Spring Harb Perspect Med. [Consensus record](https://consensus.app/papers/details/5bc90d01ab625cd1b62b2af01ba789e0/)
- [23] Karandikar U.C. et al. (2016) *Detection of human norovirus in intestinal biopsies from immunocompromised transplant patients.* J Gen Virol. [Consensus record](https://consensus.app/papers/details/cc345ac946175f59a9164322d7d4c311/)
- [24] Green K.Y. et al. (2020) *Human norovirus targets enteroendocrine epithelial cells in the small intestine.* Nat Commun. [Consensus record](https://consensus.app/papers/details/bfde5d44eb1359078ce476a906fdc2c7/)
- [25] Euller-Nicolas G. et al. (2023) *Human Sapovirus Replication in Human Intestinal Enteroids.* J Virol. [Consensus record](https://consensus.app/papers/details/7a85a0672f615d7387482f81963c0c01/)
- [26] Guérin A. & Striepen B. (2020) *The Biology of the Intestinal Intracellular Parasite Cryptosporidium.* Cell Host Microbe. [Consensus record](https://consensus.app/papers/details/4dd38d87c2fd5f61a5e08e3f0fc50407/)
- [27] Sun L.-B. et al. (2025) *Genetically Tractable Rodent Models for Cryptosporidium hominis and C. parvum Infections: Identifying Differences in Infection Sites Between Host Species.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/5a768fbb5abc581bafcb8282300f88ab/)
- [28] van Spreeuwel J.P. et al. (1985) *Campylobacter colitis: histological immunohistochemical and ultrastructural findings.* Gut. [Consensus record](https://consensus.app/papers/details/eac218569d2c545784c669bfad1780d9/) — and [28-adj] Schmidt A.-M. et al. (2019) *Immunopathological properties of the Campylobacter jejuni flagellins and the adhesin CadF…* Gut Pathog (stomach/duodenum/ileum colonisation in the murine model). [Consensus record](https://consensus.app/papers/details/47597942ebf25a21a68be35353aeb913/)
- [29] Sheikh A. et al. (2022) *Enterotoxigenic Escherichia coli heat-labile toxin drives enteropathic changes in small intestinal epithelia.* Nat Commun. [Consensus record](https://consensus.app/papers/details/212ff98817ef59dab769686c7dd56f80/)
- [30] Jones B.D. et al. (1994) *Salmonella typhimurium initiates murine infection by penetrating and destroying the specialized epithelial M cells of the Peyer's patches.* J Exp Med. [Consensus record](https://consensus.app/papers/details/f285ebda95245635831bdad0a1c0bc85/)
- [31] Chong Y. et al. (2007) *Human intestinal tissue tropism in Escherichia coli O157:H7 — initial colonization of terminal ileum and Peyer's patches and minimal colonic adhesion ex vivo.* Microbiology. [Consensus record](https://consensus.app/papers/details/7d8dfda7492a5913a9def33bdb107e0d/) — and [31-adj] Mathur N. et al. (1997) *Non-invasive Salmonella typhimurium mutants are avirulent because of an inability to enter and destroy M cells of ileal Peyer's patches.* Mol Microbiol. [Consensus record](https://consensus.app/papers/details/36b67b454c04597a82650ac966e8898b/)
- [32] Lewis S.B. et al. (2015) *Enterohemorrhagic Escherichia coli Colonization of Human Colonic Epithelium In Vitro and Ex Vivo.* Infect Immun. [Consensus record](https://consensus.app/papers/details/b81dcd4654cb54199da12c6ca8fa882d/)
- [33] Holly M.K. & Smith J.G. (2018) *Adenovirus Infection of Human Enteroids Reveals Interferon Sensitivity and Preferential Infection of Goblet Cells.* J Virol. [DOI](https://doi.org/10.1128/JVI.00250-18)
- [34] Rafie K. et al. (2021) *The structure of enteric human adenovirus 41 — a leading cause of diarrhea in children.* Sci Adv. [Consensus record](https://consensus.app/papers/details/6ec7d2af7f0d59cca6a97d9278378684/)
- [35] Ahmed S.M. et al. (2013) *A Systematic Review and Meta-Analysis of the Global Seasonality of Norovirus.* PLoS One. [Consensus record](https://consensus.app/papers/details/aa7e79cd1a525be6b58410d374a8755b/)
- [36] Kambhampati A.K. et al. (2022) *Spatiotemporal Trends in Norovirus Outbreaks in the United States, 2009–2019.* Clin Infect Dis. [Consensus record](https://consensus.app/papers/details/03364ff10caa55d0a7816c7e0f2dc97a/)
- [37] Lopman B. et al. (2009) *Host, Weather and Virological Factors Drive Norovirus Epidemiology: Time-Series Analysis of Laboratory Surveillance Data in England and Wales.* PLoS One. [Consensus record](https://consensus.app/papers/details/3c6b431079345d258b06b60c02496b24/)
- [38] Lake I.R. et al. (2019) *Exploring Campylobacter seasonality across Europe using The European Surveillance System (TESSy), 2008 to 2016.* Eurosurveillance. [Consensus record](https://consensus.app/papers/details/b5a6dab1a1dc5e61b71c92e7c44bd12a/) — and [38-adj] Djennad A. et al. (2019) *Seasonality and the effects of weather on Campylobacter infections.* BMC Infect Dis. [Consensus record](https://consensus.app/papers/details/3f6ad9b16b4051debde07bbc9aa09928/)
- [39] Nylen G. et al. (2002) *The seasonal distribution of campylobacter infection in nine European countries and New Zealand.* Epidemiol Infect. [Consensus record](https://consensus.app/papers/details/213186782f1e5cbb8bfbaa58e15a7f8e/)
- [40] Chao D.L. et al. (2019) *The seasonality of diarrheal pathogens: A retrospective study of seven sites over three years.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/38d6a7fde8835d958d715c9fd1bd56a3/)
- [41] Rouhani S. et al. (2016) *Norovirus Infection and Acquired Immunity in 8 Countries: Results From the MAL-ED Study.* Clin Infect Dis 62(10):1210-7. [DOI](https://doi.org/10.1093/cid/ciw072)
- [42] Payne D.C. et al. (2013) *Norovirus and Medically Attended Gastroenteritis in U.S. Children.* N Engl J Med. [Consensus record](https://consensus.app/papers/details/4cce28ba97185554a7ffcdd895604019/)
- [43] Hassan F. et al. (2018) *Viral Etiology of Acute Gastroenteritis in <2-Year-Old US Children in the Post-Rotavirus Vaccine Era.* J Pediatric Infect Dis Soc. [Consensus record](https://consensus.app/papers/details/1db2a5ea096050c68dcd5203d680ac8c/)
- [44] Bartsch S.M. et al. (2016) *Global Economic Burden of Norovirus Gastroenteritis.* PLoS One. [Consensus record](https://consensus.app/papers/details/fa4beeafb7905ca3974d41116833c269/)
- [45] Černíková L. et al. (2018) *Five facts about Giardia lamblia.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/220d5a7bae3351d5ac8d4cd05ba97fd9/)
- [46] Shirley D.T. et al. (2026) *Amebiasis as a neglected tropical disease: Current knowledge gaps and future directions.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/9dbf511669285bfc91ba2e9376913420/)
- [47] Guillén N. (2022) *Pathogenicity and virulence of Entamoeba histolytica, the agent of amoebiasis.* Virulence. [Consensus record](https://consensus.app/papers/details/b40b84c195e85bc9bab1b58bb8ff921d/)
- [48] Acácio S. et al. (2021) *Rotavirus disease burden pre-vaccine introduction in young children in Rural Southern Mozambique, an area of high HIV prevalence.* PLoS One 16(4):e0249714. [DOI](https://doi.org/10.1371/journal.pone.0249714)
- [49] Kosek M.N. et al. (2017) *Causal Pathways from Enteropathogens to Environmental Enteropathy: Findings from the MAL-ED Birth Cohort Study.* EBioMedicine 18:109-117. [DOI](https://doi.org/10.1016/j.ebiom.2017.02.024)
- [50] Lee G.O. et al. (2017) *Infant Nutritional Status, Feeding Practices, Enteropathogen Exposure, Socioeconomic Status, and Illness Are Associated with Gut Barrier Function As Assessed by the Lactulose Mannitol Test in the MAL-ED Birth Cohort.* Am J Trop Med Hyg 97(1):281-290. [DOI](https://doi.org/10.4269/ajtmh.16-0830)
