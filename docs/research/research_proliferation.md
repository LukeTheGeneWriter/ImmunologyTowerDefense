# Proliferation and production kinetics — real numbers for Immunology Tower Defense

Research pass, 2026-10-25. Every number below was checked against the
literature via PubMed and Consensus; nothing is written from memory. Numbers I
could not tie to a specific paper are marked **UNVERIFIED** rather than
dropped or guessed.

**Reading frame.** Absolute scale in this game is already deliberately broken,
so the load-bearing content is the **ratio** blocks (§1 and §12) and the
conversions in §13. The absolute figures are here so we can see exactly what
we are compressing and by how much.

**Game units used throughout.** 1 tick = 0.12 s. 1 coarse slot = 1 enterocyte
≈ 20 µm = 7×7 fine tiles, so 1 fine tile ≈ 2.9 µm. A round ≈ 30 s of wall
clock. §13 argues for **1 round = 1 day of biological time** and works out
what that implies.

---

## 1. THE RATIOS THAT MATTER (read this first)

Everything else in this file supports these. Each row is derived from cited
numbers in the sections below.

### 1a. Fold-expansion of one antigen-specific clone, naive precursor → peak effector

| Cell type | Fold expansion | Source of figure |
|---|---|---|
| **CD8 T cell** | **5×10⁴ – 1×10⁵** (100–200 precursors → ~10⁷/spleen) | Blattman 2002 (§2) |
| **CD4 T cell** | **~10–20× smaller than CD8** at peak; CD4 clones arrest after a limited number of divisions | De Boer 2003, Foulds 2002 (§4) |
| **NK cell (mouse, MCMV)** | **100× in spleen, 1,000× in liver** | Sun 2009 (§6) |
| **NK cell (human, acute EBV)** | **numbers rise but proliferation rate does not** — recruitment, not division | Zhang 2007 (§6) |
| **GC B cell, founding phase** | **~3,000×** (~3 founders → ~10⁴ cells in 3 days) | MacLennan 1992, Liu 1991 (§5) |
| **Gut IgA plasma cells, locally** | **250–750×** (~100–300 local precursors → ~75,000 plasma cells per ileal biopsy) | Yuvaraj 2009 (§9) |
| **Neutrophil** | not clonal. Output surges **5× (low signal) to 15× (high signal)**; **up to 10×** turnover in severe human infection | Chatta 1994, Hansen 1978 (§7) |

**Ordering to preserve in game: CD8 ≫ CD4 > NK(mouse) ≫ NK(human) ≈ flat.**
The CD8:CD4 gap (~10–20×) and the CD8:NK gap (~100–1,000×) are the two
ratios worth being faithful to.

### 1b. Division speed, fastest → slowest, same cell lineage

| State | Cell cycle length | Ratio vs fastest |
|---|---|---|
| CD8 effector (TE/EMP), antigen present, day 4 | **6.2 h** | 1× |
| CD8 central-memory precursor, antigen present | **8.6 h** | 1.4× |
| CD8 effector, antigen withdrawn at 48 h | **9.7 h** | 1.6× |
| CD8 central-memory precursor, antigen withdrawn | **17.0 h** | 2.7× |
| CD8 resting memory (human, YF-17D) | **>450 d doubling** | **~1,700×** |

All mouse except the last (human). Source: Kretschmer 2020, Akondy 2017 (§2, §3).
**The 1,700× gap between "fighting" and "remembering" is the single biggest
honest ratio in the immune system, and the game currently has no analogue of it.**

### 1c. Phase timing of a primary response (mouse, acute infection)

| Phase | CD8 | CD4 |
|---|---|---|
| Population doubling time, expansion | **8 h** | **11 h** |
| Peak | **day 8** | **day 9** |
| Contraction half-life | **41 h** | **3 d**, then biphasic → 35 d |
| Memory-phase half-life | effectively lifelong | **500 d** |

Source: De Boer, Homann & Perelson 2003 (§4). Human time-to-peak is roughly
**2× slower**: YF-17D CD8 peaks ~day 14 (§4).

### 1d. Primary vs secondary (memory recall) — the counterintuitive one

| Quantity | Primary | Secondary | Direction |
|---|---|---|---|
| Starting precursors (mouse, LCMV GP33) | 100–200 | ~5×10⁵ | **memory ~2,500–5,000× more** |
| Onset of division | lag of days | **detectable within 1 day** (human) | memory faster |
| **Per-cell fold expansion** | **larger** | **smaller** | **naive wins** |
| Contraction after peak | 10–20× loss | **3–5× loss** | memory survives better |

Sources: Blattman 2002, Koutsakos 2023, Martin 2011, Grayson 2002 (§8).
**The memory advantage is almost entirely starting headcount, speed of onset,
and survival — NOT a bigger multiplier per cell.** This is directly load-bearing
for the ~30% knowledge rung's "burst of CTLs."

---

## 2. CD8 clonal expansion — division speed and total magnitude

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| CD8 effector cell cycle length, day 4 post-immunisation, antigen present | **6.2 h** (EMP + TE, "non-CMP") | Mouse, OT-1 → peptide-pulsed DC + *L. monocytogenes* | In vivo nucleoside-analogue pulse (EdU/BrdU) with a 3 h gap to DNA-content measurement — a new absolute cell-cycle-speed assay | Kretschmer 2020, *Nat Commun* 11:113, PMID 31913278, [DOI](https://doi.org/10.1038/s41467-019-13788-w) |
| Central-memory-precursor cell cycle length, same conditions | **8.6 h** | Mouse, same | Same | Kretschmer 2020, PMID 31913278 |
| Cell cycle length after antigen withdrawal at 48 h | **9.7 h** (non-CMP), **17.0 h** (CMP) | Mouse, same | Same, after diphtheria-toxin DC depletion | Kretschmer 2020, PMID 31913278 |
| Divisions per day | **~4** (effector), **~3** (CMP) | Mouse | Derived from the above cycle lengths by the authors | Kretschmer 2020, PMID 31913278 |
| G1 phase length in rapidly cycling CD8 | **1–2 h** | Mouse | Cell-cycle-phase distribution + NA pulse | Kretschmer 2020, PMID 31913278 |
| Population doubling time during expansion | **8 h** (CD8), **11 h** (CD4) | Mouse, LCMV | ODE model fit to tetramer time-course across 6 class-I and 2 class-II epitopes | De Boer, Homann & Perelson 2003, *J Immunol* 171:3928, PMID 14530309, [DOI](https://doi.org/10.4049/jimmunol.171.8.3928) |
| Naive precursor frequency, D^b-GP33 | **1 in 2×10⁵ CD8**; **100–200 cells per mouse** (out of 2–4×10⁷ naive CD8) | Mouse, LCMV | Titration of known numbers of TCR-transgenic cells into naive hosts until the transgenic and endogenous responses matched | Blattman 2002, *J Exp Med* 195:657, PMID 11877489, [DOI](https://doi.org/10.1084/jem.20001021) |
| Human naive precursor frequency | **0.6×10⁻⁶ to 1.3×10⁻⁴** across 6 epitopes (>100-fold spread), but **conserved between individuals** | Human PBMC | Tetramer staining + magnetic-bead enrichment + multiparametric cytometry (direct detection, not estimation) | Alanio 2010, *Blood* 115:3718, PMID 20200354, [DOI](https://doi.org/10.1182/blood-2009-10-251124) |
| Divisions from naive to peak | **>14 divisions in 1 week** | Mouse, LCMV | Inferred from precursor count and peak count | Blattman 2002, PMID 11877489 |
| Total fold expansion, naive → peak effector | **~5×10⁴ – 1×10⁵** (100–200 → ~10⁷/spleen) | Mouse, LCMV | Tetramer enumeration | Blattman 2002, PMID 11877489 |
| Peak effector burden | **2×10⁷ LCMV-specific CD8 per spleen** = **50–70% of all activated CD8** | Mouse, LCMV | MHC-I tetramers + two single-cell IFN-γ assays | Murali-Krishna 1998, *Immunity* 8:177, PMID 9491999, [DOI](https://doi.org/10.1016/s1074-7613(00)80470-7) |
| Fraction of peak population that dies in contraction | **90–95%** | Mouse | Stated as settled in the field; ~5% survival independently derived from tetramer counts | Kretschmer 2020 PMID 31913278 (statement); Blattman 2002 PMID 11877489 (~5% survive) |
| Fraction becoming memory | **~5%** of peak effectors → ~5×10⁵ memory cells, stable for the animal's life | Mouse, LCMV | Tetramer enumeration, longitudinal | Blattman 2002 PMID 11877489; Murali-Krishna 1998 PMID 9491999 (~10⁶/spleen maintained for life) |
| Naive → memory precursor-frequency gain | **>1,000-fold** (2×10² naive → 5×10⁵ memory) | Mouse, LCMV | Same | Blattman 2002 PMID 11877489 |

**Caveats.** The 6.2 h figure is the most trustworthy division-speed number
available, because Kretschmer et al. built an assay for *absolute* in vivo
cycle length rather than inferring it: pulse with a nucleoside analogue, wait
3 h (BrdU's in vivo half-life is only 0.5 h, so no further label gets taken
up), then measure DNA content, and the analogue-positive/2N population is
exactly the cells that divided during the window. They validated it with an
EdU/BrdU triple-label and showed the readout is robust to varying death rates.
The older literature's "4–6 h" claim for the fastest intervals shows up in
their data as the ~3 h labelling window already marking >60% of cells at day 4
— so divisions substantially faster than 6 h do occur — but **6.2 h is the
measured population average and is the number I would build on.**

The 8 h *population* doubling time (De Boer) and the 6.2 h *cell cycle* length
(Kretschmer) are not in conflict: population doubling is net of death, and is
fit to spleen counts, so it is slower than the single-cell cycle. For a game,
the 6.2 h figure is the one to use for "how fast does a tower make a cell,"
and the 8 h figure for "how fast does the army on screen grow."

Blattman's expansion figure is an estimate built on a precursor count that is
itself an estimate — the titration method is indirect. Alanio's direct human
enumeration later showed precursor frequency varies >100-fold *between
epitopes*, so "~1 in 2×10⁵" is one epitope, not a universal constant. The
honest statement is that fold-expansion is **10⁴–10⁵**, not a precise 5×10⁴.

Mouse vs human: expansion magnitude is comparable in order, but **human
kinetics are about half-speed** (§4).

---

## 3. The "programmed response" question — does a tower keep producing after the pathogen is gone?

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Antigen contact needed to commit a naive CTL to expansion | **as little as 2 h** | Mouse naive CTL + engineered APC; confirmed in vitro *and* in vivo | APC system allowing controlled onset/removal of antigenic stimulation | van Stipdonk, Lemmens & Schoenberger 2001, *Nat Immunol* 2:423, PMID 11323696, [DOI](https://doi.org/10.1038/87730) |
| Divisions completed autonomously after commitment | **at least 7**, plus differentiation into effector and memory cells, with no further antigen required | Mouse, adoptive transfer | Antigen withdrawal after priming; progeny tracked | Kaech & Ahmed 2001, *Nat Immunol* 2:415, PMID 11323695, [DOI](https://doi.org/10.1038/87720) |
| Is the programme fully autonomous? | **No — "differential autopilot."** Antigen withdrawal at 48 h did not stop division (all subsets still Ki-67⁺, c-Myc⁺, phospho-Rb⁺) but slowed it: effectors 6.2 → 9.7 h, CMPs 8.6 → 17.0 h | Mouse | Ki-67, BrdU, c-Myc, phospho-Rb(S807/811) + absolute cycle-speed assay | Kretschmer 2020, PMID 31913278 |
| Still dividing at the peak? | **Yes.** ~100% of all responding subsets were Ki-67⁺ at day 8 and BrdU⁺ after a day 6–8 label | Mouse | Ki-67 (degraded within 24–48 h of cycle exit) + multi-pulse BrdU | Kretschmer 2020, PMID 31913278 |
| Consequence of early antigen loss for the *later* recall response | **~10-fold smaller recall** >50 days later, after antigen removal at 48 h. Removing *inflammation* (antibiotic) instead had **no effect** on recall magnitude | Mouse | DTx vs ampicillin arms, rechallenge with recombinant *L. monocytogenes*-OVA or MVA-OVA | Kretschmer 2020, PMID 31913278 |
| Cost of stopping 2 days early | **64-fold fewer** central-memory precursors (while the *fraction* of CMPs rises from 1/16 to 1/4) | Mouse | Authors' arithmetic on their measured cycle lengths | Kretschmer 2020, PMID 31913278 |

**Caveats and the answer to the design question.** The classic "autopilot"
result is real and robust: 2 h of antigen commits the cell (van Stipdonk), and
≥7 divisions follow with antigen absent (Kaech & Ahmed). Both are mouse, and
van Stipdonk's 2 h comes from an engineered-APC system where stimulation can
be cleanly switched off — a cleaner cut than any infection provides, so treat
2 h as a *lower bound on commitment*, not a typical exposure.

But the strict version is wrong, and Kretschmer's correction is the useful one
for the game: **with the pathogen gone, cells keep dividing but get slower, and
the memory-fated subset slows down the most.** So: a tower *should* keep
producing after the pathogen is cleared — that is biologically correct and it
fixes the "my army vanished, looked like a bug" problem the Director already
hit in Sprint 8. The honest version is a **decay of rate, not a cutoff**, and
the thing that should suffer most is what the player banks for later (knowledge /
memory), not what is on screen now. Kretschmer's own framing: if antigen is
cleared swiftly, less memory forms.

The third important claim: **antigen duration, not inflammation duration, sets
memory size.** In game terms, how long the pathogen was *present* should set
banked knowledge; how loud the cytokine field was should not.

---

## 4. CD4 vs CD8 — magnitude and timing

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Expansion-phase doubling time | **CD8 8 h, CD4 11 h** | Mouse, LCMV | Model fit to 6 class-I + 2 class-II epitope time-courses | De Boer 2003, PMID 14530309 |
| Time to peak | **CD8 day 8, CD4 day 9** | Mouse, LCMV | Same | De Boer 2003, PMID 14530309 |
| Contraction half-life | **CD8 41 h; CD4 3 d**, biphasic, approaching a **35-d** half-life from 8 d after peak | Mouse, LCMV | Same | De Boer 2003, PMID 14530309 |
| Memory-phase half-life | **CD4 500 d; CD8 apparently lifelong** (all 6 epitopes) | Mouse, LCMV | Same | De Boer 2003, PMID 14530309 |
| CD4 proliferative programme | CD4 cells **divide a limited number of times and arrest in early divisions**, even at higher infectious dose; CD8 proliferate extensively. The difference persists under uniform anti-CD3 in vitro, so it is **cell-intrinsic** | Mouse, *L. monocytogenes*; plus in vitro | CFSE division tracking, adoptive transfer | Foulds 2002, *J Immunol* 168:1528, PMID 11823476, [DOI](https://doi.org/10.4049/jimmunol.168.4.1528) |
| Human time to peak CD8 | **~day 14** (peak epitope-specific IFN-γ responses ~1% of CD8 by 2 weeks; tetramer⁺ cells detectable day 7–9, before IFN-γ⁺ cells) | Human, YF-17D vaccine | HLA-B35 tetramers + ICS, longitudinal | Co 2009, *Immunology* 128:e718, PMID 19740333 |
| Human dose–response | CD8 magnitude correlates with viral load (R²≈0.63) up to a threshold (peak <225 genomes/mL), then **saturates** | Human, YF-17D, n=80 | Longitudinal sampling days 0–90 + viral genome quantification | Akondy 2015, *PNAS*, PMID 25713354 |
| Human CD8 expansion in acute viral infection | **>10-fold increase** in CD45RO⁺CD27⁺CD28⁻ CD8 T cells, with high Ki-67; in acute EBV a substantial portion of the expanded cells were EBV-peptide-specific | Human, acute EBV (n=14) and acute HIV-1 (n=17) | CD45RO/CD27/CD28 subset flow cytometry + Ki-67 | Roos 2000, *J Infect Dis* 182:451, PMID 10915075, [DOI](https://doi.org/10.1086/315737) |
| CD4 vs CD8 clonal expansion in humans | Large TCR clonal expansions detectable in the **CD8** and not the CD4 subset in acute EBV | Human, acute infectious mononucleosis | Global TCR-heterogeneity / CDR3 clonotype tracking | Maini 2000, *J Immunol* 165:5729, PMID 11067931 |

**Caveats.** A clean, single-paper "CD4 peaks N-fold below CD8" number is
surprisingly hard to find, because the two are rarely enumerated on the same
scale in the same experiment. What is solid is the **direction and mechanism**:
CD4 doubles more slowly (11 vs 8 h), peaks a day later, and — the bigger effect
— stops dividing early and cell-intrinsically (Foulds). Composing the two
doubling times over the respective 8/9-day expansions gives a **~20× gap**,
and the commonly stated ~10× gap is consistent with that; I am presenting
**10–20×** as derived rather than directly measured, and would not defend a
tighter figure.

The CD4 memory half-life of 500 days vs effectively-infinite CD8 is the one
place where CD4 does *better* than CD8 kinetically — CD4 contraction is slower
and biphasic. If helper-T towers in game are meant to feel like a long-term
investment and CTLs like a burst, the biology supports that.

---

## 5. B cells and antibody output

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| **GC centroblast cell-cycle time** | **6–7 h** | Mouse (BALB/c), T-dependent response | In vivo labelling / cell-kinetic analysis of centroblasts | Zhang, MacLennan, Liu & Lane 1988, *Immunol Lett* 18:297, PMID 3263319, [DOI](https://doi.org/10.1016/0165-2478(88)90178-2) |
| Follicular B-blast cycle during GC founding | **~6 h**; ~3 founder cells → **~10⁴ cells in 3 days** | Rat, carrier-primed hapten-protein | Histology + BrdU pulse labelling, clonal counting | MacLennan, Liu & Johnson 1992, *Immunol Rev* 126:143, PMID 1597318, [DOI](https://doi.org/10.1111/j.1600-065x.1992.tb00635.x) |
| Centroblast → centrocyte population renewal | **every 7 h** | Rat spleen | BrdU pulse + in situ hapten-binding histology | Liu 1991, *Eur J Immunol* 21:2951, PMID 1748148, [DOI](https://doi.org/10.1002/eji.1830211209) |
| **Disagreeing value — birth rate in *established* GCs** | **33 cells per 1,000 GC cells per hour** (3.3 %/h), implying **~30 h** mean intermitotic time if every cell cycles | Mouse (C3H/HeN) spleen, SRBC | Stathmokinetic (metaphase-arrest) birth-rate measurement | Hollowood & Macartney 1992, *Eur J Immunol* 22:261, PMID 1730253, [DOI](https://doi.org/10.1002/eji.1830220138) |
| GC cell **loss** rate | minimal **3% per hour**; DZ and LZ about equal in size; "several hundred GCs" form per T-dependent immunisation | Mouse spleen, oxazolone-CSA | Semi-automated confocal analysis of 3,457 GC sections; 3-D reconstruction | Wittenbrink 2011, *J Immunol* 187:6185, PMID 22102720, [DOI](https://doi.org/10.4049/jimmunol.1101440); Wittenbrink 2010, *J Immunol* 184:1339, PMID 20053939, [DOI](https://doi.org/10.4049/jimmunol.0901040) |
| Divisions per dark-zone passage | **1–6 divisions**, scaling with antigen captured; **~2 on average** at steady state | Mouse, NP-OVA / αDEC-OVA | H2B-mCherry dilution (tet-off), EdU→BrdU dual pulse, photoactivation | Gitlin, Shulman & Nussenzweig 2014, *Nature* 509:637, PMID 24805232, [DOI](https://doi.org/10.1038/nature13300); Mesin, Ersching & Victora 2016, *Immunity* 45:471, PMID 27653600, [DOI](https://doi.org/10.1016/j.immuni.2016.09.001) |
| T-cell help shortens the cycle | S phase (the longest phase) is accelerated; ~half of control cells labelled in early S had finished S by 5 h, far more of the selected cells had | Mouse | EdU/BrdU dual pulse + DNA content; replication-timing sequencing; single-molecule DNA fibre | Gitlin 2015, *Science* 349:643, PMID 26184917, [DOI](https://doi.org/10.1126/science.aac4919) |
| Interzonal transit | **50% of DZ cells → LZ in ~4 h**; **<10% of LZ cells → DZ in 6 h** | Mouse | Intravital photoactivation (PA-GFP) + flow cytometry | Mesin 2016, PMID 27653600; Gitlin 2014, PMID 24805232 |
| **Seroconversion: total Ab / IgM / IgG** | **15 / 18 / 20 days post-exposure**, or **9 / 10 / 12 days post-symptom-onset** (medians) | **Human**, SARS-CoV-2, n=80 serial sera | Serial ELISA + immunoassay | Lou 2020, *Eur Respir J* 56:2000763, PMID 32430429, [DOI](https://doi.org/10.1183/13993003.00763-2020) |
| Seroconversion, pooled | **IgG and IgM both ~12 days post-symptom-onset** (range 1–40); detection probability ~10% at onset → 98–100% by day 22 | **Human**, 3,214 datapoints / 516 individuals / 21 studies | Meta-analysis, Bayesian kinetic fitting | Borremans 2020, *eLife* 9:e60122, PMID 32894217, [DOI](https://doi.org/10.7554/eLife.60122) |
| **Secretory IgA onset in gut lumen** | **slow onset, >14 days**; response half-life **>16 weeks** | **Mouse**, germ-free, reversible colonisation with live *E. coli* | Reversible gnotobiotic colonisation + anti-commensal IgA ELISA on gut washes | Hapfelmeier 2010, *Science* 328:1705, PMID 20576892, [DOI](https://doi.org/10.1126/science.1188454) |
| Intestinal IgA vs serum IgG, same animals | serum IgG by **9 dpi**; intestinal IgA by **24 dpi** | **Mouse**, oral Norwalk virus-like particles ± cholera toxin | ELISA on serum and intestinal washes | Ball 1998, *J Virol* 72:1345, PMID 9445035, [DOI](https://doi.org/10.1128/JVI.72.2.1345-1353.1998) |
| Gut-derived IgA ASC after oral vaccination | peak IgA ASC at **day 7** in naive, **day 4–5** in primed | **Human**, oral inactivated ETEC vaccine, Phase I | Antibodies-in-lymphocyte-secretions (ALS) of blood mononuclear cells | Lundgren 2016, *Vaccine* 34:3132, PMID 27133878, [DOI](https://doi.org/10.1016/j.vaccine.2016.04.055) |
| **Antibody per plasma cell — in vivo** | **8,000–20,000 IgM molecules/cell/s, mean 12,000** (≈4.3×10⁷/h) | **Rabbit**, i.v. SRBC stromata, splenic PFC | Absolute serum hemolysin accumulation rate ÷ total body PFC count | Conrad & Ingraham 1974, *J Immunol* 112:17, PMID 4590818 |
| **Antibody per plasma cell — human, ex vivo single-cell** | **37 ± 19 pg/cell/day** (blood ASC) → **63 ± 30** (bone-marrow LLPC); single-cell 7–8 d: blood 56 ± 39, LLPC 81 ± 64; observed range **3–148 pg/cell/day**. Converted: **≈1,700 IgG/s (blood plasmablast) → ≈3,800 IgG/s (BM LLPC)** | **Human**, blood plasmablasts vs BM plasma-cell subsets | BM-mimetic survival culture, ELISpot + ELISA/multiplex; on-chip single-cell secretion imaging | Nguyen 2025, *Front Immunol* 16:1644102, PMID 40963623, [DOI](https://doi.org/10.3389/fimmu.2025.1644102) |
| Published range | **100–10,000 molecules/cell/s** (2–220 pg/cell/day); outliers to 3,334 pg/cell/day | Human | Literature survey | Nguyen 2025, PMID 40963623 |
| Spread across single cells | secretion rates span **up to 3 logs** (affinities 4 logs) across >0.5M single cells | **Mouse**, tetanus-toxoid immunisation | DropMap droplet microfluidics (40-pL droplets), fluorescence-relocation immunoassay | Eyer 2017, *Nat Biotechnol* 35:977, PMID 28892076, [DOI](https://doi.org/10.1038/nbt.3964) |
| In vitro comparator | **233 molecules/s** (≈5 pg/cell/day) | Hybridoma cell line, in vitro | Droplet-microfluidic FRET secretion assay | Rutkauskaite 2022, *iScience* 25:104515, PMID 35733793, [DOI](https://doi.org/10.1016/j.isci.2022.104515) |
| **Short-lived plasma cell** | many splenic plasma cells survive **<3 days**; plasmablast clones divide ~**5 times**; the spleen sustains only ~20–100 plasma cells/mm² and loses the excess | Mouse spleen | BrdU pulse-chase, Ig V-region sequencing, quantitative histology | Sze 2000, *J Exp Med* 192:813, PMID 10993912, [DOI](https://doi.org/10.1084/jem.192.6.813) |
| **Long-lived plasma cell half-life** | **426 days** (wild-type BM LLPC) vs **63 days** when CD28/CD80/CD86 signalling is lost; splenic short-lived cells unaffected | Mouse | Population decay fitting in CD28-, CD80/86-deficient mice | Rozanski 2011, *J Exp Med* 208:1435, PMID 21690252, [DOI](https://doi.org/10.1084/jem.20110040) |
| LLPC secretion without memory B cells | plasma cells secrete **>1 year** in mice with no detectable memory B cells | Mouse, LCMV | Memory-B depletion + adoptive transfer of virus-specific plasma cells | Slifka 1998, *Immunity* 8:363, PMID 9529153, [DOI](https://doi.org/10.1016/s1074-7613(00)80541-5) |
| Plasma cell persistence, primate | BrdU⁺ plasma cells detectable **10 years** after vaccination/BrdU | **Rhesus macaque**, decade-long study | BrdU label retention + anti-CD20 memory-B depletion | Hammarlund 2017, *Nat Commun* 8:1781, PMID 29176567, [DOI](https://doi.org/10.1038/s41467-017-01901-w) |
| **Human gut plasma cell age — direct** | median cell age **11 years** (CD19⁻CD45⁺) and **22 years** (CD19⁻CD45⁻) | **Human small intestine** | Retrospective **¹⁴C birth-dating** of genomic DNA + organ-transplant turnover analysis | Landsverk 2017, *J Exp Med* 214:309, PMID 28104812, [DOI](https://doi.org/10.1084/jem.20161590) |
| Serum antibody half-life (downstream readout, not cell lifespan) | **50 yr** (VZV) to **>200 yr** (measles, mumps); tetanus **11 yr**, diphtheria **19 yr** | **Human**, 45 subjects followed up to 26 yr | Longitudinal serology + limiting-dilution memory-B counts | Amanna 2007, *N Engl J Med* 357:1903, PMID 17989383, [DOI](https://doi.org/10.1056/NEJMoa066092) |
| **Affinity maturation, classic (hapten)** | **K_a 10⁵–10⁶ M⁻¹ at 1–2 weeks → 10⁷–10⁸ M⁻¹ after a few months** (~100–1,000-fold) | **Rabbit**, DNP-protein | Equilibrium dialysis / fluorescence quenching | Foote & Eisen 1995, *PNAS* 92:1254, PMID 7877964, [DOI](https://doi.org/10.1073/pnas.92.5.1254); primary Eisen & Siskind 1964, *Biochemistry* 3:996, PMID 14214095, [DOI](https://doi.org/10.1021/bi00895a027) |
| Affinity maturation, human lineage | **1,000-fold overall**, unmutated ancestor → best antibody, with a "remarkably uniform rate of improvement" | **Human**, experimental influenza infection, anti-HA | Lineage reconstruction + recombinant expression of inferred intermediates, SPR | Kepler 2014, *Front Immunol* 5:170, PMID 24795717, [DOI](https://doi.org/10.3389/fimmu.2014.00170) |
| **Affinity gain within ONE GC over ~10 days** | **3.5–14.5-fold** per clone (median **6.8-fold**); a single V_H mutation worth **7.6-fold** | **Mouse**, CGG-alum, individual GCs at d15 | Single-GC photoactivation + single-cell Ig sequencing; recombinant Fab affinity | Tas 2016, *Science* 351:1048, PMID 26912368, [DOI](https://doi.org/10.1126/science.aad3439) |
| Kinetic (not just thermodynamic) maturation | repertoire shifts to an Ig family with **extremely high on-rate**, independent of equilibrium affinity | Mouse, 2-phenyl-oxazolone | Stopped-flow k_on/k_off on hybridoma mAbs | Foote & Milstein 1991, *Nature* 352:530, PMID 1907716, [DOI](https://doi.org/10.1038/352530a0) |
| **Antigen-geometry ceiling** | monovalent hapten responses mature **up to 1,000-fold**; multivalent virus (VSV) only **10–15-fold** germline → hypermutated | Mouse, VSV vs haptens | Recombinant scFv-Cκ binding + neutralisation | Kalinke 2000, *PNAS* 97:10126, PMID 10963674, [DOI](https://doi.org/10.1073/pnas.97.18.10126) |
| **Per-cycle selection stringency** | **10–30% of B cells arriving in the light zone are selected to re-enter the dark zone** per cycle | Mouse | Mathematical fitting of intravital photoactivation transit rates | Mesin 2016, PMID 27653600, [DOI](https://doi.org/10.1016/j.immuni.2016.09.001) |
| **GC lifetime, mouse** | **~3 weeks** (CGG-alum) | Mouse, popliteal LN | Multicolour fate mapping + multiphoton imaging | Mesin 2016, PMID 27653600; Tas 2016, PMID 26912368 |
| GC lifetime, human | antigen-specific GC B cells persist **≥13 weeks** in 2/7 individuals | **Human**, seasonal influenza vaccine | Serial fine-needle aspiration of draining LN + mAb cloning | McIntire 2024, *J Exp Med* 221:e20240668, PMID 38935072, [DOI](https://doi.org/10.1084/jem.20240668) |
| GC lifetime, extended-delivery prime | GC B cells last **≥6 months**; lineage phylogenies spanning **≥191 days** with no further antigen; still **49-fold above baseline at 29 weeks** | **Rhesus macaque**, HIV Env, 12-day slow-delivery prime | LN FNA, single-cell transcriptomics, SHM lineage tracing | Lee 2022, *Nature* 609:998, PMID 36131022, [DOI](https://doi.org/10.1038/s41586-022-05216-9) |
| B cells per GC | "**up to a few thousand B cells**"; in the early exponential phase ~3 founders → ~10⁴ cells by day 3, filling the follicle by day 4 | Mouse; rat (carrier-primed) | — ; histology + clonal counting | Tas 2016, PMID 26912368; MacLennan 1992, PMID 1597318; Liu 1991, PMID 1748148 |
| **Clones per GC** | **23–46 unique VDJ** among 34–77 sequenced cells; richness estimators give **~50–200 clones/GC** (median Chao1 **102**) at day 6 | Mouse, CGG-alum | In-situ photoactivation of single GCs + single-cell Igh sequencing + Chao1/ACE | Tas 2016, PMID 26912368 |
| Somatic hypermutation rate | ~1 mutation per 10³ bp per division | Mouse | — | Gitlin 2014, PMID 24805232 — **exponent UNVERIFIED** from the retrieved full text (stripped in retrieval) |

**Caveats.**

*GC division time — the famous "fastest cell in the body" number needs an
asterisk.* The 6–7 h figure is a 1988–92 Birmingham-school measurement in rat
and mouse **centroblasts during the exponential founding phase**, not the whole
GC. It genuinely disagrees with stathmokinetic measurement in **established**
GCs (3.3% new cells per 1,000 per hour, implying ~30 h per division if every
cell cycles). The two reconcile if only ~20% of GC cells are in cycle at any
moment and the GC is at steady state — and Wittenbrink's independent **3%/h
loss rate** matches the 3.3%/h birth rate almost exactly, which is strong
evidence that **an established GC is a treadmill, not an expanding
population.** Modern work does not report a single clean hours-per-division;
Gitlin measures *how many* divisions per dark-zone passage (1–6,
antigen-dose-dependent) and that T-cell help shortens S phase. **Defensible
statement: GC B cells in the proliferative phase divide every ~6–7 h, among
the fastest in the body, but a mature GC only holds its size because it kills
~3% of its cells every hour.**

*Time to IgM / IgG / IgA.* The human numbers are SARS-CoV-2 primary infection,
the best-characterised human primary response with both exposure-anchored and
symptom-anchored timing. Use the post-exposure column (IgM 18 d, IgG 20 d) if
the game clock starts at infection. **Note the ordering is much tighter than
the textbook: IgM leads IgG by only ~2 days, and a 21-study meta-analysis finds
them seroconverting at the same ~12 days post-onset.** For gut IgA there is no
clean human luminal day-count; mouse gives >14 days after live colonisation and
24 days after oral non-replicating antigen (vs 9 days for serum IgG in the same
animals). **Secretory IgA is reliably the slowest arm, roughly 2–4 weeks.**

*Antibodies per second spans ~5 logs, mostly for methodological reasons.*
Conrad & Ingraham's in vivo rabbit figure (8,000–20,000/s, mean 12,000) is the
one to trust for "what a plasma cell does in a body": absolute serum antibody
accumulation ÷ total body PFC, in undisturbed cells, with the authors checking
that a cell's ribosome budget can sustain it — though it is IgM in rabbit. The
best modern human number is **≈1,700 IgG/s for a blood plasmablast rising to
≈3,800/s for a bone-marrow LLPC**, which also overturns the older assumption
that plasmablasts are the fast secretors. In vitro cells and hybridomas sit an
order of magnitude lower (233/s). **Where in vitro and in vivo disagree by
10–100×, trust the in vivo / ex vivo primary numbers** — cultured and
transformed lines lack niche support and their ER/Golgi expansion differs.
The widely quoted "10,000/s" is at the top of the credible range.

*Plasma cell lifespan: "short-lived" is a property of the niche, not the cell.*
Sze's central finding is that many splenic plasma cells die in <3 days because
the spleen only sustains ~20–100 plasma cells/mm², not because they were born
with a short clock. For the long end, prefer explicit numbers over "forever":
426-day half-life in mouse BM, >1 year of secretion with zero memory B cells,
10-year BrdU retention in macaque, and the strongest human datum — **median
cell ages of 11 and 22 years by ¹⁴C birth-dating of human *gut* plasma cells**,
which overturns the older belief that gut plasma cells are short-lived. Note
this sits in tension with the mouse figure in §9 (intestinal IgA plasma cell
half-life 4.7 d); the human ¹⁴C data are the more direct measurement of actual
cell age and should win for a human-set game. Amanna's serum half-lives are
antibody persistence, not cell lifespan, and must not be quoted as the latter.

*Affinity maturation: the "1,000-fold" is a hapten number.* Eisen & Siskind's
rabbit anti-DNP goes 100–1,000-fold over weeks-to-months, but Kalinke showed
the same system gives only **10–15-fold** against a multivalent virus, because
germline antibodies already bind well by avidity. **For a game whose enemies
are whole bacteria and viruses, 10–15× is the right ceiling, not 1,000×.**
Within a single GC over ~10 days the gain is **median 6.8-fold** — so the
1,000-fold figure is the product of many GC passages plus serial recall
responses, not one GC's work. There is no published "N cycles to maturity"
count; the one quantified selection stringency is **10–30% of light-zone
arrivals selected per cycle**.

*GC lifetime was revised upward recently and is protocol-dependent.* Classic
mouse is ~3 weeks; human influenza vaccination sustains GCs ≥13 weeks in some
individuals; a 12-day slow-delivery prime in macaques sustains them ≥6 months.
Persistence is a tunable design lever, not a constant. For size, "a few
thousand B cells" and "~10⁴ by day 3" bracket the range, but Wittenbrink's
3,457-section study found GC volumes are broadly distributed and
non-synchronised — **there is explicitly no typical GC size**, though the
cellular composition is conserved. Clonality is **tens to hundreds of clones
per GC** (median ~102), which replaces the older "handful of founder clones."

---

## 6. NK cells

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Antigen-specific NK expansion | **100-fold in spleen, 1,000-fold in liver** | Mouse, MCMV, Ly49H⁺ NK recognising m157 | Adoptive transfer of congenically marked Ly49H⁺ NK into DAP12-deficient hosts + infection | Sun, Beilke & Lanier 2009, *Nature* 457:557, PMID 19136945, [DOI](https://doi.org/10.1038/nature07665) |
| NK timing relative to T cells | **NK proliferation is neither earlier nor faster** than the CD4 or CD8 response; early NK **contraction is slower** than T cells'; NK decay is continuous and monophasic (no biphasic/memory signature) and returns to **below** pre-infection levels | Mouse, MCMV | Kinetic modelling of NK, CD4 and CD8 time-courses from matched datasets (spleen, liver, lung) | Schlub 2011, *J Immunol* 187:1385, PMID 21697462, [DOI](https://doi.org/10.4049/jimmunol.1100416) |
| NK memory | Ly49H⁺ NK persist for months in lymphoid and non-lymphoid organs, self-renew, degranulate and make cytokines rapidly on reactivation, and give a robust secondary expansion on transfer + rechallenge | Mouse, MCMV | Adoptive transfer + viral challenge | Sun 2009, PMID 19136945 |
| **Human** NK proliferation rate, homeostasis | **4.3 ± 2.4 %/day**, i.e. a **doubling time of ~16 days** | Human, healthy young adults (n=5) | 24 h IV 6,6-D₂-glucose label; FACS-sorted CD3⁻CD16⁺ NK; DNA deuterium by GC-MS | Zhang 2007, *Immunology* 121:258, PMID 17346281 |
| Human NK production rate | **15 ± 7.6 ×10⁶ cells/L/day** | Human | Same | Zhang 2007, PMID 17346281 |
| Human NK blood turnover / lifespan | labelled cells cleared at **6.9 ± 4.0 %/day**, half-life **<10 days**; turnover time in blood **~2 weeks** | Human | Same | Zhang 2007, PMID 17346281 |
| **Human NK in acute infection** | In acute infectious mononucleosis (n=5, ~12 d post symptom onset) **NK numbers were increased but kinetics were unaffected** (3.2 → 2.8 %/day; not elevated) | Human, acute EBV | Same | Zhang 2007, PMID 17346281 |
| NK division during acute infection (mouse) | NK turnover rises and the rise is accompanied by NK *division*: 34% of activated NK killing was blast-cell-mediated at day 3; hydroxyurea reduced activated NK lysis to 40% of untreated | Mouse, LCMV | Hydroxyurea block + ³H-thymidine pulse single-cell autoradiography | Biron, Turgiss & Welsh 1983, *J Immunol* 131:1539, PMID 6886424 |
| Peak timing, NK vs CTL | NK numbers peak **before** CTL in MCMV, but **not because NK divide faster** — the NK response starts from a far higher precursor frequency (Ly49H⁺ NK are a sizeable fraction of all NK, vs ~1 in 2×10⁵ for a CD8 epitope) | Mouse, MCMV | Inference across Schlub 2011 + Sun 2009 + Blattman 2002 | Derived; see those PMIDs |

**Caveats, and where mouse and human disagree sharply.** The mouse MCMV system
says NK cells do a genuinely adaptive-looking 100–1,000-fold clonal expansion
(Sun 2009). Human deuterium labelling in acute EBV says NK numbers go up while
the proliferation rate does **not** change at all (Zhang 2007) — i.e. the human
blood NK increase is redistribution and recruitment, not division.

**Which would I trust for which claim?** Both, for different claims. Sun's
mouse result is specific to a receptor–ligand pair (Ly49H/m157) with no human
equivalent of that genetic cleanliness, and it is measured in tissue after
adoptive transfer into a host that cannot respond — a system built to maximise
the visible expansion. Zhang's human numbers are the better guide to *typical*
NK behaviour in a typical infection, and the better guide for this game, which
is set in a human gut. Schlub 2011 is the tiebreaker and it is unambiguous:
**even in the mouse, NK proliferation is not earlier or faster than T cells.**

So the folk claim "NK are the fast innate responders that get there before T
cells" is, as a statement about *proliferation*, not supported. NK are early
because they are **already present in numbers**, not because they ramp quickly.
That is a design-relevant correction.

---

## 7. Innate production baselines

> **Framing correction.** My own brief for this section had the neutrophil
> lifespan controversy backwards. Deuterium labelling (Pillay 2010) challenged
> the lifespan **upward** (5.4 d), which would imply production ~10× *lower*
> than classic. Lahoz-Beneytez 2016 then refuted that model and restored the
> short half-life and classic-scale production. So **the classic production
> figure is the one that survived** — but a 2026 *Blood* paper has reopened
> the lifespan question at ≥6 days. See the caveats.

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Neutrophil marrow production | **0.85 ×10⁹ cells/kg/day** (postmitotic pool 5.59 ± 0.90 ×10⁹/kg ÷ transit time 6.60 ± 0.03 d) | Human, 13 healthy adults | Marrow cellularity from sections + erythron iron turnover; ³H-thymidine autoradiography | Dancey 1976, *J Clin Invest* 58:705, PMID 956397, [DOI](https://doi.org/10.1172/JCI108517) |
| Neutrophil effective production (turnover) | **0.87 ± 0.13 ×10⁹ /kg/day** (³H-thymidine) vs **1.62 ± 0.46 ×10⁹ /kg/day** (DF³²P) — the two methods disagree ~2× in the same subjects | Human | ³H-thymidine in vivo vs DF³²P ex vivo | Dancey 1976, PMID 956397 |
| Independent replication | **0.785 ×10⁹ /kg/day** | Human, 7 healthy volunteers | DF³²P labelling | Dale 1998, *Am J Hematol* 57:7, PMID 9423810, [DOI](https://doi.org/10.1002/(sici)1096-8652(199801)57:1%3C7::aid-ajh2%3E3.0.co;2-0) |
| **Absolute neutrophils per day** | **≈6×10¹⁰/day** (thymidine) to **≈1.1×10¹¹/day** (DF³²P) for a 70 kg adult. **Use 6×10¹⁰ as the central estimate; the famous "10¹¹/day" is the DF³²P upper end** | Human | **DERIVED** from Dancey's per-kg values × 70 kg | derived from PMID 956397 |
| Whole-body cell turnover, for scale | **0.33 ± 0.02 ×10¹² cells/day** total, of which **~90% are blood cells** | Human, reference 70 kg adult | Literature integration of ubiquity × mass × lifespan | Sender & Milo 2021, *Nat Med* 27:45, PMID 33432173, [DOI](https://doi.org/10.1038/s41591-020-01182-9) |
| **Neutrophil blood half-life — classic** | **9.6 ± 1.3 h** (literature range 4–18 h) | Human | DF³²P-labelled autologous cells re-infused (**ex vivo** labelling) | Dale 1998, PMID 9423810 |
| **Neutrophil lifespan — the long claim** | **5.4 days** mean circulatory lifespan, "at least 10× longer than previously reported" | Human, homeostatic | **In vivo** ²H₂O (heavy water) DNA labelling, GC/MS | Pillay 2010, *Blood* 116:625, PMID 20410504, [DOI](https://doi.org/10.1182/blood-2010-01-259028) |
| **Neutrophil half-life — the rebuttal** | **0.79 ± 0.25 d (19 h)** from ²H-glucose; **0.77 ± 0.14 d (18.5 h)** from re-analysed heavy water; key-points box says **13–19 h**. Marrow transit time **5.80 ± 0.42 d** | Human (n=4 + n=9 + n=5 published) | Stable-isotope labelling + new mechanistic model; shows Pillay's model assumed a physiologically untenable blood:mitotic-precursor ratio | Lahoz-Beneytez 2016, *Blood* 127:3431, PMID 27136946, [DOI](https://doi.org/10.1182/blood-2016-03-700336) |
| **Neutrophil lifespan — 2026 reopening** | **≥6 days** for the mature neutrophil; deuterium appears in mature blood neutrophils only from day 6; **myelocytes are post-mitotic**; mature neutrophils "constantly exchange between bone marrow, blood and tissue" | Human | 6 h oral ²H-glucose pulse → marrow aspiration + FACS of each precursor stage, GC/MS | van Grinsven 2026, *Blood*, PMID 42485667, [DOI](https://doi.org/10.1182/blood.2026033801) |
| Marrow pipeline delay | **~5.8–6.6 days** from progenitor division to the cell reaching blood | Human | Above | Dancey 1976 PMID 956397; Lahoz-Beneytez 2016 PMID 27136946 |
| Tissue vs blood neutrophil lifespan | "short but **variable** lifetimes across multiple tissues"; tissues co-opt and reprogram neutrophils en route to elimination (qualitative; per-tissue numbers are in figures) | Mouse, multi-tissue | Neutrophil fate-tracking + receptor/transcriptome/ATAC profiling | Ballesteros 2020, *Cell* 183:1282, PMID 33098771, [DOI](https://doi.org/10.1016/j.cell.2020.10.003) |
| Macaque neutrophil half-life | **1.63 ± 0.16 d** | Rhesus macaque | BrdU pulse-labelling + model | He 2018, *J Immunol* 200:4059, PMID 29728510, [DOI](https://doi.org/10.4049/jimmunol.1800207) |
| Mouse neutrophil blood half-life | **11.4 h** | Mouse | **Secondary citation** inside Yona 2013's discussion | reported in PMID 23273845; primary **UNVERIFIED** |
| **Emergency granulopoiesis — infection** | neutrophil turnover **up to 10× normal** in the early phase; blood transit shortened; then marrow egress **"almost stopped" for up to 50 h** in early convalescence | Human, 8 patients, severe acute bacterial meningitis | DF³²P-labelled cells | Hansen 1978, *Acta Med Scand* 204:407, PMID 717062, [DOI](https://doi.org/10.1111/j.0954-6820.1978.tb08463.x) |
| **Emergency granulopoiesis — G-CSF dose response** | peak ANC **5× baseline at 30 µg/day**, **15× baseline at 300 µg/day**; identical in 20–30 y and 70–80 y volunteers | Human, 38 healthy volunteers, 14 d daily rhG-CSF | Randomised dose-ranging; serial ANC, marrow pool sizing | Chatta 1994, *Blood* 84:2923, PMID 7524759 |
| **Mechanism and speed of the surge** | marrow postmitotic transit **6.4 d → 2.9 d**; mitotic pool expanded; **blood half-life and margination unchanged**; skin-chamber neutrophil accumulation **down ~50%** | Human, 32 young + 19 elderly | Marrow pools + tissue migration at baseline and d5, transit time d6–14 | Price 1996, *Blood* 88:335, PMID 8704192 |
| GM-CSF comparison | ANC **3.5× baseline by day 14**; transit 6.4 → 3.9 d; turnover rate **not** significantly changed; half-life 9.6 → 13.1 h (n.s.) | Human, 7 volunteers | DF³²P kinetics + marrow pools + skin chambers | Dale 1998, PMID 9423810 |
| **Fastest possible onset (marrow reserve dump)** | circulating mononuclear phagocytes **completely lost within 2 h** of i.v. endotoxin; classical monocytes reappear **as early as 4 h**, heavily deuterium-labelled → emergency marrow release, not de-margination; intermediate/non-classical absent until **24 h**; steady state restored by **day 7** | Human, 10 volunteers, 2 ng/kg i.v. endotoxin | Flow cytometry + ²H-glucose pulse 20 h pre-challenge | Patel 2017, *J Exp Med* 214:1913, PMID 28606987, [DOI](https://doi.org/10.1084/jem.20170355) |
| Monocyte marrow kinetics | precursor proliferation **0.42/day**; postmitotic marrow residency **1.64 ± 0.04 d (~38 h)** before release | Human, 4 healthy volunteers | ²H-glucose, FACS of 3 subsets over 30 d, GC/MS, compartment model | Patel 2017, PMID 28606987 |
| **Monocyte circulating lifespan, human** | classical **1.01 ± 0.26 d**; intermediate **4.30 ± 0.36 d**; non-classical **7.41 ± 0.53 d**. Pool shares **87% / 5% / 8%**. Only 1.4% of classical convert to intermediate; 100% of intermediate convert to non-classical | Human | Above | Patel 2017, PMID 28606987 |
| **Monocyte half-life, mouse** | **Ly6C-hi 0.843 d (20 h)**; **Ly6C-lo 2.2 d**, extended to **11 d** when Ly6C-hi precursors are ablated — lifespan is **demand-regulated**, not a fixed property | Mouse, C57BL/6 | BrdU pulse-chase; Cx3cr1-CreER YFP fate map ± anti-CCR2 | Yona 2013, *Immunity* 38:79, PMID 23273845, [DOI](https://doi.org/10.1016/j.immuni.2012.12.001) |
| Monocyte production rate, macaque | classical monocytes **3.09 ×10⁸ cells/L of blood/day**; half-life 1.01 ± 0.15 d; released into blood as early as **1 d** after division (vs 4–5 d marrow residency for neutrophils) | Rhesus macaque | BrdU pulse + model | He 2018, PMID 29728510 |
| Monocyte production rate, human | **No direct measurement exists.** **DERIVED**: ~2×10⁹ classical monocytes/day for a 70 kg adult | Human | Derivation from Patel 2017 pool shares + lifespans | derived; inputs PMID 28606987 |
| **Monocyte arrival in inflamed small intestine** | transferred monocytes detectable in SI lamina propria **by day 1**; both frequency and absolute number significantly higher in inflamed than non-inflamed gut, while homing to liver/spleen/marrow was unaffected | Mouse, intestinal-manipulation inflammation | Competitive adoptive transfer of marrow monocytes into CCR2-deficient recipients + flow + NanoString | Desalegn & Pabst 2019, *Nat Commun* 10:3229, PMID 31324779, [DOI](https://doi.org/10.1038/s41467-019-11148-2) |
| Homing-receptor dependence | in non-inflamed gut, CCR2⁻/⁻ monocytes **4.5-fold** and β7-integrin⁻/⁻ **2.7-fold** less frequent in SI LP than WT competitors; in inflamed gut **the β7 requirement is lost**, CCR2 preserved | Mouse | Competitive adoptive transfer | Desalegn & Pabst 2019, PMID 31324779 |
| **The "monocyte waterfall" timetable** | **day 1** Ly6C⁺MHCII⁻ → **day 2** most cells MHCII⁺, Ly6C-low → **day 5** all Ly6C⁻MHCII-hi, with CX3CR1, CD64, CD11c rising and *Il10* / CD206 acquired. Inflamed-vs-normal transcriptional divergence is **maximal at day 1** (21 genes) and **converges by day 5** (10 genes) | Mouse small intestine | Synchronised adoptive transfer + flow + transcriptomics | Desalegn & Pabst 2019, PMID 31324779 |
| Magnitude of influx in colitis | CCR2-dependent Ly6C-hi monocytes become **the dominant mononuclear cell type** in the lamina propria in acute colitis; later convert to migratory T-cell-priming APCs; ablating them ameliorates inflammation | Mouse, DSS/acute colitis | Flow, anti-CCR2 ablation, transcriptional profiling | Zigmond 2012, *Immunity* 37:1076, PMID 23219392, [DOI](https://doi.org/10.1016/j.immuni.2012.08.026) |
| No local proliferation in colitis | **No increase in macrophage proliferation** during DSS colitis or the repair phase — the influx is entirely recruitment | Mouse colon | Ki67 + BrdU, DSS | Bain 2014, *Nat Immunol* 15:929, PMID 25151491, [DOI](https://doi.org/10.1038/ni.2967) |
| **Gut macrophage replacement — parabiosis** | at 10 wk, blood Ly6C-hi monocytes ~12% non-host and **all three colonic macrophage subsets ~10% non-host** — gut macrophage chimerism tracks blood monocyte chimerism. **Liver Kupffer cells: no exchange** | Mouse parabionts | Parabiosis + flow | Bain 2014, PMID 25151491 |
| Gut macrophage half-life | **~4–6 weeks** for mature colonic macrophages | Mouse colon | Parabiosis + *Ccr2*⁻/⁻ age series | Bain 2014, PMID 25151491 |
| Local proliferation collapses with age | Macrophage Ki67⁺ falls from **~30% at day 2 of life** to near detection limit by **5 weeks**; yolk-sac-derived macrophages ≤1,000 cells per adult intestine | Mouse | Csf1r-mer-iCre-mer and Flt3-Cre fate maps, Ki67 | Bain 2014, PMID 25151491 |
| **The 2018 revision — gut macrophages are NOT uniformly monocyte-replaced** | Three similarly sized subsets. **Tim-4⁺CD4⁺: locally maintained** (<1% donor chimerism at 7 wk, <5% at 16 wk; in colon not replenished at all between 7 and 16 wk). **Tim-4⁻CD4⁺: slow** (~50% of potential chimerism by 16 wk in SI, ~30% in colon). **Tim-4⁻CD4⁻: fast**, reaching near-parity with blood monocyte chimerism by 12 wk. **Only the last has the turnover classically attributed to "gut macrophages"** | Mouse SI + colon | **Abdomen-shielded** marrow chimeras (avoids irradiating the gut) at 7/12/16 wk, normalised to blood monocyte chimerism; + Cx3cr1-CreER YFP pulse-label | Shaw 2018, *J Exp Med* 215:1507, PMID 29789388, [DOI](https://doi.org/10.1084/jem.20180019) |
| **Human gut macrophage turnover** | **All four** human small-intestinal macrophage subsets are completely replaced over time; monocyte-like **Mf1/Mf2 largely replaced within 3 weeks**; mature **Mf3/Mf4 significantly slower** | **Human**, duodenal transplant recipients | Donor/recipient HLA discrimination, flow + transcriptomics on serial biopsies | Bujko 2018, *J Exp Med* 215:441, PMID 29273642, [DOI](https://doi.org/10.1084/jem.20170057) |
| Contrast — microglia, mouse | **0.69%** in S-phase per BrdU pulse; by 2-photon imaging proliferation **0.79%/day**, death 1.23%/day; whole population renews **once every ~96 days**. No monocyte contribution in adult | Mouse | BrdU + Iba1 IHC, retroviral labelling, chronic cranial-window 2-photon | Askew 2017, *Cell Rep* 18:391, PMID 28076784, [DOI](https://doi.org/10.1016/j.celrep.2016.12.041) |
| Contrast — microglia, human | renewal **median 28% per year**; some microglia last **>2 decades** | **Human** | ¹⁴C bomb-pulse birth dating of sorted microglial nuclei | Réu 2017, *Cell Rep* 20:779, PMID 28746864, [DOI](https://doi.org/10.1016/j.celrep.2017.07.004) |
| Contrast — alveolar macrophages | **Negligible** donor replacement at **8 months** in lung-shielded chimeras. The widely quoted **30-day half-life is an irradiation artifact** from unshielded chimeras | Mouse | Marrow chimeras ± lead shielding; PKH26 dye retention | Murphy 2008, *Am J Respir Cell Mol Biol* 38:380, PMID 18192503, [DOI](https://doi.org/10.1165/rcmb.2007-0224RC) |
| Contrast — alveolar/Kupffer/splenic/peritoneal | Established **before birth**, self-maintain through adult life with **no** steady-state monocyte input; post-depletion repopulation is stochastic in-situ proliferation | Mouse | Parabiosis + Cx3cr1-CreER fate mapping; depletion/repopulation | Hashimoto 2013, *Immunity* 38:792, PMID 23601688, [DOI](https://doi.org/10.1016/j.immuni.2013.04.004); Yona 2013, PMID 23273845 |
| **Gut vs self-renewing compartment ratio** | **DERIVED.** Human: monocyte-like gut macrophages fully replaced in **3 wk** (~17×/year) vs human microglia **0.28×/year** → **~60-fold**. Mouse: mature gut macrophage half-life 4–6 wk vs alveolar macrophages essentially no replacement in 8 months → **categorical**, not merely quantitative | Human / mouse | Derivation from the cited primaries | derived from PMIDs 29273642, 28746864, 25151491, 18192503 |
| **Neutrophil : monocyte ratio in blood** | 95% reference intervals: neutrophils 30.1–69.2% and monocytes 1.87–5.60% of WBC → **~10:1 typical**, midpoint ~13:1. Absolute medians elsewhere: 6.74 vs 0.63 ×10⁹/L → **10.7:1** | Human, 433 healthy adults 18–80 y; and 360 healthy pregnant women | CLSI-style reference-interval studies, automated CBC | Abbas 2024, *J Blood Med* 15:513, PMID 39720632, [DOI](https://doi.org/10.2147/JBM.S488050); Gebere 2021, *PLoS One* 16:e0249185, PMID 33831053, [DOI](https://doi.org/10.1371/journal.pone.0249185) |
| **Ratio of production rates** | neutrophils **1.42×10⁹ cells/L/day** vs classical monocytes **3.09×10⁸ cells/L/day** → **4.6 : 1**. *The only study measuring both in the same animals.* | Rhesus macaque | BrdU pulse + model | He 2018, PMID 29728510 |
| Human production ratio | **No measured value exists.** A naive derivation gives ~30–55:1, inconsistent with both the ~10:1 blood ratio and the 4.6:1 macaque measurement — **do not quote it** | Human | Derivation flagged as unreliable | derived from PMIDs 956397, 28606987 |

**Caveats.**

*Neutrophil production.* Dancey's three estimates in the same subjects disagree
by ~2×, and the authors attribute the high DF³²P value to lower recovery and a
shorter apparent t½ of the ³²P label — i.e. the DF³²P number is probably
inflated by labelling damage. The widely repeated "~10¹¹ neutrophils/day" is
essentially that inflated value scaled to 70 kg. **The defensible figure is
0.8–0.9 ×10⁹/kg/day ≈ 6×10¹⁰/day**, independently reproduced by Dale.

*Neutrophil half-life — the order-of-magnitude fight, and which to trust.* The
two camps measure different things with opposite biases. The classic 6–10 h
figures come from cells harvested, DF³²P-labelled **ex vivo** and re-infused:
handling activates neutrophils and the labelled population shows reduced
recovery, so these are biased **short**. Pillay's 5.4 days used **in vivo**
²H₂O with no cell manipulation — unbiased on that axis — but Lahoz-Beneytez
showed that extracting a lifespan from heavy-water data requires knowing the
marrow transit time, which heavy-water data constrain poorly, and that
Pillay's model implicitly assumed a blood:mitotic-precursor ratio that is
physiologically untenable; fixing it collapses the estimate to 13–19 h.
**For this game I would use ~0.8 d (≈19 h) as the blood half-life — roughly a
day in circulation.** But flag that it is not settled: van Grinsven 2026, by
labelling each marrow precursor stage in situ rather than modelling blood data
alone, again lands on ≥6 days and argues the cell shuttles between marrow,
blood and tissue — which would reconcile both camps as short *per-pass* blood
residence inside a long *total* life. Also note the **~6-day marrow pipeline
delay** between a progenitor dividing and a cell reaching blood.

*Emergency granulopoiesis.* Two usable human magnitudes. Infection: up to
**10× baseline turnover** early, followed by a near-total shutdown of marrow
egress for up to 50 h in early convalescence — a surge followed by a
refractory trough, which is a ready-made game mechanic. Caveats: n=8, DF³²P
(same short-bias), some patients on prednisone. G-CSF gives a clean dose
response: **5× at 30 µg/day, 15× at 300 µg/day**. The mechanism is (a) a
bigger mitotic pool and (b) compressing marrow transit from 6.4 → 2.9 days —
**blood half-life and margination do not change**, so the surge is a faster
conveyor with more cells on it, not longer-lived cells. Onset is ~24 h for the
first large rise and 5–14 days to peak with daily dosing; the fastest possible
onset is the pre-formed marrow reserve being dumped within **4 h**. One
trade-off worth building in: surge neutrophils **migrate ~50% worse** into
tissue (true for both G-CSF and GM-CSF) — more cells, each less effective.

*Monocytes.* The human lifespans rest on only **4 subjects** and a
sequential-ontogeny model, but the direction is robust and independently
confirmed by fate-mapping human monocytes into humanised mice: classical
circulate ~1 day and mature classical → intermediate → non-classical with
increasing lifespan (1 → 4.3 → 7.4 d). In mouse, Ly6C-lo lifespan is **not a
fixed property** — ablating the Ly6C-hi precursor pool stretches it from 2.2 to
11 days, apparently via CSF-1 availability. No direct human production rate
exists.

*Monocyte recruitment into inflamed gut.* Desalegn & Pabst solve the usual
problem (endogenous gut monocytes are a continuum of ages) by transferring a
synchronised cohort into CCR2-deficient recipients. Donors are in the lamina
propria **within 1 day** and the waterfall takes **~5 days** end to end. The
headline finding matters for modelling: inflammation does **not** just speed
the same programme up — the inflamed-vs-normal transcriptional divergence is
**maximal on arrival (day 1)** and *converges* by day 5. The environment stamps
the monocyte when it lands. Caveats: mild local inflammation model,
CCR2-deficient recipients, and no retrievable absolute fold-increase for the
influx — treat the magnitude as "becomes the dominant population," not a
number.

*Gut macrophage replacement — the brief's framing is out of date.* Bain 2014
established the exception quantitatively and it holds: gut macrophage
chimerism tracks blood monocyte chimerism while Kupffer cells show zero
exchange, CCR2 is needed for both marrow egress and gut entry, and local
proliferation collapses from ~30% Ki67⁺ at 2 days old to undetectable by 5
weeks. But Shaw 2018, using **abdomen-shielded** chimeras — avoiding exactly
the irradiation artifact that Murphy 2008 exposed for alveolar macrophages —
showed only one of three similarly sized subsets actually has that high
turnover, with Tim-4⁺CD4⁺ **locally maintained**. The honest statement:
*the gut is the one tissue containing a continuously, rapidly
monocyte-replaced resident macrophage population, but it coexists with slow
and self-maintaining populations.* Human data agree and are stronger than
usually appreciated (duodenal transplants: Mf1/Mf2 replaced within 3 weeks,
Mf3/Mf4 much slower, all eventually replaced).

For the contrast compartment, **use microglia, not alveolar macrophages**: the
alveolar "30-day half-life" is an irradiation artifact and the real
steady-state answer is negligible replacement over 8 months, which is harder
to put a number on. Note the two microglia papers are in mild tension —
Askew's human Ki67 (~2%) implies faster renewal than Réu's 28%/year; Réu's
bomb-pulse ¹⁴C method directly measures cell age, so prefer 28%/year for human.

*Neutrophil : monocyte ratio.* The **blood** ratio is ~10:1 and well supported.
The **production** ratio is the weak spot: the only same-animal measurement is
macaque at 4.6:1, ~2× lower than the blood ratio. A human derivation gives
~30–55:1, inconsistent with both, almost certainly reflecting the inflated
DF³²P neutrophil figure. **Quote ~10:1 for blood; quote 4.6:1 as the one
directly measured production ratio, in macaque; do not quote a human
production ratio at all.**

---

## 8. Memory recall — how much faster and larger, quantitatively

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Memory vs naive precursor frequency | **~2,500–5,000× higher** (100–200 naive → ~5×10⁵ memory, same epitope). The paper's own headline: **>1,000-fold increase in precursor frequency** | Mouse, LCMV D^b-GP33 | Tetramer enumeration | Blattman 2002, PMID 11877489 |
| Memory response onset | Memory cells **divided after a shorter lag**, had an **increased division rate**, a **lower loss rate**, and differentiated to effector function faster. Memory cells were **multifunctional** on reactivation (2–3 effector functions per cell) vs naive progeny expressing mutually exclusive single functions | Mouse, equal numbers of TCR-transgenic naive vs memory transferred | In vivo response comparison at matched cell numbers | Veiga-Fernandes 2000, *Nat Immunol* 1:47, PMID 10881174, [DOI](https://doi.org/10.1038/76907) |
| Human recall onset | Spike-specific **CD4 proliferation and extensive activation evident as early as 1 day post symptom onset**; CD8 rapidly activated but with variable expansion | Human, Delta/Omicron breakthrough in vaccinated subjects | 6 pMHC multimers, early and frequent longitudinal sampling | Koutsakos 2023, *Immunity* 56:879, PMID 36958334, [DOI](https://doi.org/10.1016/j.immuni.2023.02.017) |
| Secondary contraction | Secondary effectors contracted **3–5×**; primary effectors **10–20×**. Shown in the same animal, so it is intrinsic to the memory cell, not environmental | Mouse, LCMV | Co-transfer of naive + memory into one host; longitudinal counts | Grayson 2002, *J Immunol* 169:3760, PMID 12244170, [DOI](https://doi.org/10.4049/jimmunol.169.7.3760) |
| Memory resistance to apoptosis | After whole-body irradiation, naive CD8 fell **8× more** than memory CD8 | Mouse | Irradiation + counts | Grayson 2002, PMID 12244170 |
| **Per-cell expansion: naive beats memory** | **Naive CD8 expand more in number per cell than primary memory CD8** after various infections and immunisations, independent of memory number or quality; naive also generate **more long-lived memory per cell**. Driven by naive cells' *sustained* proliferation | Mouse | Co-transfer of physiological numbers of naive and memory cells into the same naive host before antigen | Martin 2011, *J Immunol* 188:1255, PMID 22205031, [DOI](https://doi.org/10.4049/jimmunol.1101579) |
| Secondary response saturates | Similar numbers of antigen-specific CD8 were found at day 8 **no matter how many memory cells were present at the time of infection**; existing memory *suppressed* the primary response to the same epitope | Mouse, *L. monocytogenes* and LCMV | Adoptive transfer of low numbers of trackable non-transgenic memory CD8 | Badovinac 2003, *J Immunol* 170:4933, PMID 12734336, [DOI](https://doi.org/10.4049/jimmunol.170.10.4933) |
| Tissue-resident memory recall is local and lymph-node-independent | Mucosal T_RM were highly motile, **paused and divided in situ** on local antigen challenge; pre-existing T_RM proliferation **dominated** the local recall response and contributed most of the boosted secondary T_RM pool. Reactivation also recruited recirculating memory, which converted to T_RM **antigen-independently** in situ | Mouse, reproductive tract and skin | Two-photon intravital microscopy in live mice | Beura 2018, *Nat Immunol* 19:173, PMID 29311694, [DOI](https://doi.org/10.1038/s41590-017-0029-3) |
| Resting memory division rate (human) | Memory maintained by quiescent cells dividing **less than once a year** — **doubling time >450 days** | Human, YF-17D vaccinees | In vivo deuterium labelling + mass-spec quantification of deuterium dilution in YFV-specific CD8, longitudinal over years | Akondy 2017, *Nature* 552:362, PMID 29236685, [DOI](https://doi.org/10.1038/nature24633) |
| Memory's effector readiness | Long-lived YFV-specific memory CD8 express no effector molecules but retain an **open chromatin profile at effector genes** a decade after vaccination — "poised to respond rapidly" | Human | ATAC-seq / chromatin profiling of sorted YFV-specific memory CD8 | Akondy 2017, PMID 29236685 |

**Caveats, and the important correction.** The textbook claim "the secondary
response is faster and bigger" is true at the level of the *whole organism* and
false at the level of the *individual cell*. Martin 2011 co-transferred
physiological numbers of naive and memory cells into the same host and found
naive cells out-expand memory cells per cell, and even produce more long-lived
memory per cell. Grayson 2002 adds that the secondary population's advantage
in final size comes from **not dying** (3–5× vs 10–20× contraction), not from
dividing more.

So the honest decomposition of the memory advantage is:

1. **~1,000–5,000× more starting cells** (Blattman) — the dominant term.
2. **Days-to-hours faster onset** — human recall is detectable at 1 day
   (Koutsakos) vs a multi-day lag for priming.
3. **~3–5× better survival through contraction** (Grayson).
4. **Worse** per-cell expansion (Martin).

Beura 2018 is the other one worth building on: in mucosa, the recall is
*tissue-autonomous*. The T_RM already sitting in the tissue divide in place and
carry the response; the lymph node is not in the loop. For a gut-set game with
an explicit DC→lymph-node shuttle, that is a meaningful second pathway.

Mouse vs human: the structure is the same; the human onset number (1 day) is
from symptom onset, not infection, so it is if anything an underestimate of how
fast recall starts.

---

## 9. Gut-specific numbers

| Quantity | Value | Organism / system | Method | Citation |
|---|---|---|---|---|
| Gut share of the body's plasma cells | "The adult gut mucosa contains some **80%** of the body's activated B cells — differentiated to plasmablasts and plasma cells." Independently, ">80% of mammalian antibody-secreting cells reside in the gut, of which most express IgA" | Human; mammalian | Field estimate restated in review; no primary measurement exists | Brandtzaeg 2010, *J Pediatr* 156(2 Suppl):S8, PMID 20105666, [DOI](https://doi.org/10.1016/j.jpeds.2009.11.014); Keppler 2021, *Front Immunol* 12:670290, PMID 33936114 |
| Gut IgA plasma cells per mm² | **131 ± 8 IgA⁺ plasma cells/mm²** (normal histology); falling to 108 ± 8 and 89 ± 16 with GvHD stage | Human intestinal biopsy | Immunohistology, 430 biopsies / 115 patients | Scheidler 2023, *Haematologica*, PMID 37259539 |
| Absolute IgA plasma cells per ileal biopsy | **~75,000 IgA-producing cells per ileal biopsy**, derived from only **~100–300 local precursors** | Human ileum | Direct counting + IgA V_H clonal sequence analysis | Yuvaraj 2009, *J Immunol* 183:4871, PMID 19786537, [DOI](https://doi.org/10.4049/jimmunol.0901315) |
| Isotype breakdown of Ig⁺ cells in gut mucosa | Per mucosal tissue unit: **IgA 48.5%**, IgG 29.6%, IgM 12.0%, IgE 8.2%, IgD 1.9% | Human jejunum + ileum, 15 healthy adult men | Peroxidase–antiperoxidase immunohistochemistry | Paniagua 1985, *Anat Anz*, PMID 3913346 |
| Daily IgA secreted into the gut lumen | **~2 g/day** ("nearly two grams"); other reviews say "several grams" | Human adult | Review | Yang & Palm 2020, *Curr Opin Microbiol* 56:89, PMID 32889295, [DOI](https://doi.org/10.1016/j.mib.2020.08.003); Pabst 2012, *Nat Rev Immunol*, PMID 23103985 |
| Total daily production, by isotype | **IgA 66, IgG 34, IgM 7.9, IgD 0.4, IgE 0.02 mg/kg/day** → IgA alone (66) exceeds all other isotypes combined (42.3). ~4.6 g/day total IgA in a 70 kg adult | Human adult | Biosynthesis/catabolism turnover studies, tabulated | McGhee, Mestecky, Elson & Kiyono 1989, *J Clin Immunol* 9:175, PMID 2671008, [DOI](https://doi.org/10.1007/BF00916814) |
| How much IgA reaches a mucosal surface | More IgA is made daily than **IgG and IgM combined**; but **~1/3 of it goes straight into the vascular compartment and never reaches a mucosal surface** | Human | Site-by-site estimation of IgA production | Conley & Delacroix 1987, *Ann Intern Med* 106:892, PMID 3579073, [DOI](https://doi.org/10.7326/0003-4819-106-6-892) |
| Enterocyte lifespan, crypt → tip | **Human 3.48 ± 1.55 d; mouse 2.81 ± 1.14 d; rat 2.37 d.** Human significantly longer than preclinical species | Human, mouse, rat | Meta-analysis of 85 studies, 1,602 subjects, 6 species | Darwich 2014, *Drug Metab Dispos* 42:2016, PMID 25233858, [DOI](https://doi.org/10.1124/dmd.114.058404) |
| Turnover, compartment-resolved | Whole epithelium **59.6 h**; crypt 26.5 h; **villus transit 33.4 h over 67.9 cell positions** | Mouse jejunum | [³H]thymidine radioautography, 1 µm plastic sections | Tsubouchi 1983, *Cell Tissue Kinet* 16:441, PMID 6883429 |
| Crypt output | **420 cell divisions per crypt per day**; 16 stem cells/crypt (T_c 16 h), 4 transit divisions (T_c 11–12 h) | Mouse ileum | Mathematical model fitted to [³H]TdR labelling data | Loeffler 1986, *Cell Tissue Kinet* 19:627, PMID 3802185 |
| Crypt cellularity | **~250 cells per crypt**; up to **60% of crypt cells divide twice daily** | Mouse | Review | Booth & Potten 2000, *J Clin Invest* 105:1493, PMID 10841502 |
| Migration velocity up the villus | **1.05 → 2.04 cell positions/h** (upper crypt → villus base). In distance: **13.8 µm/h** (upper crypt) → **34.2 µm/h** (villus base) → **51.8 µm/h** (upper villus) | Mouse jejunum | [³H]TdR leading-labelled-cell tracking | Tsubouchi 1983, PMID 6883429 |
| Migration velocity, human | — | Human | — | **UNVERIFIED** — no primary human µm/h or cell-positions/h figure located |
| Enterocyte height | **31.3 ± 2.3 µm** (younger adults, n=22); 30.1 ± 1.2 µm (elderly) — not significantly different | Human jejunum | Quantitative morphometry of jejunal biopsies | Corazza 1986, *Gerontology* 32:60, PMID 3512366, [DOI](https://doi.org/10.1159/000212766) |
| Enterocyte width (diameter) | **13.8 µm** constant through crypt; **16.8 µm** lower villus → **25.4 µm** upper villus | Mouse jejunum | Direct measurement, 1 µm plastic sections | Tsubouchi 1983, PMID 6883429 |
| Enterocyte width, human specifically | — | Human | — | **UNVERIFIED** — no explicit human apical-diameter measurement located |
| Peyer's patches, human | **~60** before gestational week 30 → maximum **240 at puberty** → declines after age 25. **≥46%** concentrated in the distal 25 cm of ileum | Human small intestine | Macroscopic counting at autopsy across all ages | Jung, Hugot & Barreau 2010, *Int J Inflam* 2010:823710, PMID 21188221, [DOI](https://doi.org/10.4061/2010/823710), citing Cornes 1965, *Gut* 6:225 / 6:230, PMID 18668776 / 18668777 |
| Peyer's patches, mouse | **13** PPs; **1.8 lymphoid follicles per cm** of intestine | Mouse | Gross dissection + histology | Poskitt 1984, *Aust J Exp Biol Med Sci*, PMID 6743144 |
| Isolated lymphoid follicles, mouse | **100–200 ILFs** along the antimesenteric wall, each with a large B-cell area **including a germinal centre** and overlying M cells | Mouse small intestine | Whole-mount survey, IHC, flow cytometry | Hamada 2002, *J Immunol* 168:57, PMID 11751946, [DOI](https://doi.org/10.4049/jimmunol.168.1.57) |
| pIgR transcytosis speed | Transcytosis of bound dimeric IgA has a **t½ of 30 min**; >90% of newly made receptor reaches the basolateral surface first; transport is ligand-independent | Rabbit pIgR in MDCK monolayers | Quantitative pulse-chase of ¹²⁵I-dIgA, basolateral → apical | Mostov & Deitcher 1986, *Cell* 46:613, PMID 3524859, [DOI](https://doi.org/10.1016/0092-8674(86)90887-1) |
| Luminal secretory-IgA half-life | — | — | — | **UNVERIFIED.** Nearest real data: IgA stability *increases* on polymerisation in simulated intestinal fluid, but all mAbs degrade relatively rapidly (Wallace 2020, *Vaccine*, PMID 33041102); and the field attributes luminal SIgA clearance to **peristalsis and mucus flow, not proteolysis** (Mantis 2011, *Mucosal Immunol*, PMID 21975936) |
| Luminal bacterial density, duodenum + jejunum | **10³–10⁴ /mL** of content (low gastric pH + fast transit). Directly measured jejunum: **5.8×10³ – 8.0×10⁶ CFU/mL** by culture, **1.5×10⁵ – 3.1×10⁷ genomes/mL** by qPCR; paper's own normal range **10³–10⁵ CFU/mL** | Human | Concentration × organ volume integration; enteroscopic luminal sampling with culture + 16S qPCR + MiSeq (n=20) | Sender, Fuchs & Milo 2016, *PLoS Biol* 14:e1002533, PMID 27541692, [DOI](https://doi.org/10.1371/journal.pbio.1002533); Sundin 2017, *BMC Microbiol* 17:160, PMID 28716079 |
| SIBO threshold, duodenum | **≥10³ CFU/mL** is the validated threshold; 385 of 483 subjects were below it | Human duodenal aspirate | Culture + 16S/shotgun sequencing | Leite 2023, *Clin Gastroenterol Hepatol*, PMID 37315761 |
| Luminal bacterial density, ileum | **10⁷–10⁸ /g** wet weight. Mucosal-swab sampling of distal ileum is far lower: 79% of samples <1.6×10⁴ CFU/mL | Human ileal effluent (n=7 ileostomy); human distal ileum (n=150) | Quantitative counts + 16S; surgical mucosal swab + culture + MALDI-TOF | Booijink 2010, *Environ Microbiol*, PMID 20626454; Villmones 2020, *Clin Microbiol Infect*, PMID 32835795 |
| Luminal bacterial density, colon (for contrast) | **0.92×10¹¹ /g wet stool** → **3.8×10¹³ bacteria** in the colon | Human | Meta-analysis of 14 microscopy/DAPI/FISH studies × MRI colon volume | Sender 2016, PMID 27541692 |
| Intestinal IgA plasma cell lifespan | Major intestinal IgA plasma-cell population: **half-life 4.7 d**; maximum lifespan **7–8 weeks** | Mouse (CBA) intestine | [³H]thymidine + immunofluorescence/autoradiography | Mattioli & Tomasi 1973, *J Exp Med*, PMID 4578299 |

**Caveats.**

*The 80% figure has no primary measurement behind it.* It is a field estimate
Brandtzaeg restated across decades of reviews; the 2010 paper is the cleanest
citable statement in his own words. Note the exact claim is 80% of *activated
B cells / plasmablasts + plasma cells*, not of all B-lineage cells. For a real
number, use Scheidler's 131 ± 8 IgA⁺ PC/mm² — though that "normal" group is
transplant patients without histologic GvHD, not healthy volunteers. Yuvaraj's
75,000-cells-per-biopsy is the most concrete absolute count but depends on
biopsy size, which the abstract does not state. **The Yuvaraj ratio is the
game-relevant one: ~100–300 local precursors produce ~75,000 plasma cells, a
250–750× local expansion inside one piece of gut.**

*Grams of IgA per day: the spread is real but reconcilable.* "~2 g/day into the
lumen" and "~4.6 g/day synthesised" are both right, because Conley & Delacroix
found ~1/3 of total IgA never reaches a mucosal surface. The mg/kg/day table is
the number to quote for "IgA is the most abundantly produced isotype," and it
is quantified: 66 for IgA vs 42.3 for everything else combined. Honesty flag:
the McGhee 1989 abstract is not on PubMed, so those five values were read from
the paper's body text via full-text search; the PMID, authors, journal, volume
and pages are confirmed from PubMed metadata, and McGhee cites Conley &
Delacroix for them.

*Epithelial turnover: the human/mouse gap is narrower than usually implied.*
Darwich 2014 is the best source by a distance — 85 studies, 1,602 subjects,
six species, with the difference tested statistically rather than asserted. The
answer is **mouse 2.81 d vs human 3.48 d**, not "mouse 3 d, human much longer."
Two caveats the authors flag: human data were too sparse to resolve regional
(duodenum vs ileum) differences even though mice clearly show them, and the
human figure covers GI epithelia broadly rather than small intestine alone.
Tsubouchi's 59.6 h mouse figure is shorter than Darwich's pooled mouse mean —
one strain, one site, one lab. Loeffler's 420 divisions/crypt/day is a *model
output* fitted to labelling data, not a direct count.

*The game's 20 µm enterocyte checks out — but on the width axis, not the
height.* Human enterocytes are ~30 µm **tall** (Corazza), while the mouse cell
**diameter** runs 13.8 µm in the crypt to 25.4 µm at the villus tip
(Tsubouchi). So 20 µm sits right in the middle of the measured width range,
but it is about two-thirds of the cell's height: a human enterocyte is a tall
column, not a cube. The cells also visibly **widen** as they climb the villus,
which is why Tsubouchi's µm/h migration speed accelerates even where the
cell-positions/h rate is flat. No explicit human apical-diameter measurement
was located, so the human width is inferred from mouse.

*Peyer's patches: 240 is a maximum, not a typical adult count.* Cornes 1965 is
the canonical source but both parts are scanned page images in PMC with no
machine-readable text, so the values come from Jung 2010's full text explicitly
citing Cornes — the standard way these numbers propagate. Adult numbers decline
after age 25, which is why textbooks quote anything from 100 to 250. The mouse
figure (13) comes from a marsupial comparative study where eutherian mice are
the control group — odd provenance for a widely used number, but a real count.
Germinal-centre counts per patch were not available for either species.

*Transcytosis: the 30 min half-time is solid but it is rabbit pIgR in dog
kidney cells.* MDCK was the first system that let anyone measure the rate, and
no better in vivo number has displaced it. Separately, pIgA and pIgM
internalise and transcytose at similar rates, so dIgA's advantage in external
transfer comes from diffusion through the lamina propria and basement membrane,
not from the transcytosis step. **The luminal SIgA half-life is the one number
that genuinely does not exist in the literature** — the field treats SIgA
clearance as transit-driven (peristalsis + mucus turnover) rather than as a
degradation half-life. If the game needs a luminal SIgA persistence value,
anchor it to mucus/transit turnover, not to an invented antibody half-life.

*Luminal bacterial density: the gradient is robust, the small-intestine end is
methodologically fragile.* The headline spans **eight orders of magnitude**:
duodenum/jejunum 10³–10⁴/mL → ileum 10⁷–10⁸/g → colon ~10¹¹/g. But the numbers
disagree by sampling technique: enteroscopic *luminal* jejunal samples ran as
high as 8×10⁶ CFU/mL, while surgical *mucosal swabs* of distal ileum found 79%
below 1.6×10⁴ CFU/mL — Villmones argues explicitly that colonoscopy-based
sampling contaminates and overestimates. Sundin also found viability as low as
0.3% in some samples (qPCR genome counts far exceeding culturable CFU), so
"bacteria present" and "live bacteria" differ by up to two orders of magnitude
in the same sample. For the colon figure, Sender's own stated limitation is
that it is measured in *stool*, which may not represent the proximal colon.
**The design-relevant fact is clean: the small intestine is a sparse,
fast-flowing lumen with thousands-to-millions of bacteria per mL, not the dense
colonic mat — and the ileum is where both the density and the Peyer's patches
climb.**

---

## 10. Where the measurement methods disagree, and which I trust

A short list of the places in this file where the literature does not speak
with one voice, because these are exactly the places a game should not pick a
number blind.

1. **Neutrophil blood lifespan — the big one.** See §7. Classic ex-vivo-labelled
   estimates (~6–8 h half-life) vs in-vivo deuterium labelling (~3.75 days).
   An order of magnitude apart, and the two methods are not measuring the same
   thing: handling and re-injecting neutrophils activates and shortens them.
2. **Cell cycle length vs population doubling time.** 6.2 h (single-cell, in
   vivo, Kretschmer) vs 8 h (population fit, De Boer). Both correct; the first
   is "how often does a cell divide," the second is "how fast does the army
   grow net of losses." Use the first for emission intervals, the second for
   on-screen population growth.
3. **In vitro vs in vivo on whether division ceases.** Elegant in vitro work
   (c-Myc as an inherited division timer) says T cells abruptly stop dividing
   at a preset time. In vivo, Kretschmer found ~100% of all subsets still
   Ki-67⁺/c-Myc⁺/phospho-Rb⁺ at day 6–8. **Trust the in vivo result** for a
   game set in tissue: the in vitro cultures lack the continuous antigenic and
   IL-2 input that keeps cells cycling, which is precisely the variable the
   game models.
4. **NK expansion: mouse 100–1,000× vs human ~0×.** See §6. Both are real
   measurements of different systems. Trust the human kinetics for the question
   "should an NK tower have a clonal-expansion upgrade."
5. **Antibody secretion rate per plasma cell.** See §5; this one spans orders
   of magnitude in the literature.

---

## 11. Recommended biological time per round

**Recommendation: one round = one day.**

The argument is that the two clocks the game most needs to feel right — the
adaptive ramp and the epithelial turnover — both land naturally on a day.

| Real quantity | At 1 round = 1 day |
|---|---|
| CD8 effector cell cycle (6.2 h) | **~4 emissions per round** |
| CD8 time to peak (day 8, mouse) | **round 8** |
| CD8 contraction half-life (41 h) | **~1.7 rounds** |
| CD4 time to peak (day 9) | **round 9** |
| Human time to peak (~day 14) | **round 14** |
| Resting memory doubling (>450 d) | **>450 rounds — never, within a campaign** |
| Memory half-life (CD8, lifelong) | never decays |

At a ~30 s round this is a wall-clock compression of **~2,880×**. In game
units, the 6.2 h CD8 cycle becomes **7.75 s**, or **~65 ticks**.

That is strikingly close to where the tuning already sits:
`LymphocyteEmissionIntervalSeconds = 3.5 s` corresponds to a **2.8 h cycle** —
about **2.2× faster than the fastest division speed ever measured in a mouse
T cell**. Raising it to ~7.75 s would make the helper-T tower literally
one-emission-per-real-division. See §13 for whether that is a good idea.

Alternatives considered and rejected:
- **1 round = 1 hour.** The adaptive ramp would take 200 rounds to peak. Dead.
- **1 round = 1 week.** Epithelial turnover (days) would finish mid-round and
  the whole adaptive arc would be over in round 2. Also dead.

---

## 12. Ratio summary table (game-facing)

The numbers a tuning pass should actually preserve, with everything normalised.

### Division/emission rate, normalised to the fastest cell in the body

| Cell | Real rate | Relative rate |
|---|---|---|
| CD8 effector (antigen present) | 6.2 h/division | **1.00** |
| CD8 CMP (antigen present) | 8.6 h/division | 0.72 |
| CD8 effector (antigen gone) | 9.7 h/division | 0.64 |
| CD8 CMP (antigen gone) | 17.0 h/division | 0.36 |
| Human NK (homeostasis) | 16 d doubling | **0.016** |
| Human resting memory CD8 | >450 d doubling | **0.0006** |
| **GC B cell, proliferative phase** | **6–7 h/division** | **~0.95** |
| GC B cell, established GC (net) | 3.3% birth/h vs 3.0% loss/h | treadmill: ~0 net |

**The headline: the two fastest-dividing cells in the body — the CD8 effector
(6.2 h) and the germinal-centre B cell (6–7 h) — are within ~15% of each
other.** Everything else in the immune system is at least an order of magnitude
slower.

### Fold expansion of one clone

| Cell | Fold | Relative to CD8 |
|---|---|---|
| CD8 | 10⁴–10⁵ | 1 |
| CD4 | ~10³–10⁴ (derived) | ~1/10 – 1/20 |
| NK (mouse, spleen) | 10² | ~1/500 |
| NK (mouse, liver) | 10³ | ~1/50 |
| NK (human, acute) | ~1 | ~1/10⁵ |
| GC B cell (founding) | ~3×10³ | ~1/30 |
| Gut IgA plasma cells (local) | 250–750 | ~1/150 |
| Neutrophil output (emergency) | 5–15 | ~1/10⁴ |

### Survival through contraction

| Population | Loss factor after peak |
|---|---|
| Primary CD8 effectors | 10–20× (90–95% die) |
| Secondary (memory-derived) effectors | 3–5× |
| Splenic short-lived plasma cells | most die <3 d (niche-limited to ~20–100/mm²) |
| Bone-marrow long-lived plasma cells | half-life **426 d**; human gut plasma cells median age **11–22 yr** |

### Time-to-arrival, fastest to slowest (the ordering the campaign should feel)

| Arm | Onset | At 1 round = 1 day |
|---|---|---|
| Marrow neutrophil reserve dump | **4 h** | same round |
| Monocyte arrival in inflamed gut | **1 d** | round 1 |
| Memory T recall (human) | **1 d** | round 1 |
| Monocyte → mature gut macrophage | **5 d** | round 5 |
| CD8 peak (mouse) | **8 d** | round 8 |
| CD4 peak (mouse) | **9 d** | round 9 |
| Serum IgM / IgG (human, post-exposure) | **18 / 20 d** | rounds 18–20 |
| CD8 peak (human) | **14 d** | round 14 |
| **Secretory IgA in gut lumen** | **>14–24 d** | **rounds 14–24 — the slowest arm** |

---

## 13. What this means for the game

Ten blunt bullets. Several of them say "the real number would wreck the game" —
those are the important ones.

**1. Set one round = one day, and say so in `BoardConfig`.** Everything lands:
CD8 peaks round 8, CD4 round 9, the human response peaks round 14, enterocytes
turn over every 3–4 rounds, IgM/IgG arrive rounds 18–20, secretory IgA arrives
rounds 14–28. At ~30 s/round that is a 2,880× wall-clock compression, which is
a number we can defend out loud rather than an unstated fudge. The 6.2 h CD8
cell cycle becomes **7.75 s ≈ 65 ticks**.

**2. `LymphocyteEmissionIntervalSeconds = 3.5 s` is currently 2.2× faster than
the fastest T cell division ever measured in vivo.** At 1 round = 1 day, 3.5 s
implies a 2.8 h cycle; the measured figure is 6.2 h (Kretschmer, §2). Raising
it to ~7.75 s makes one emission equal one real division. I'd do it — the
number becomes meaningful and it leaves headroom for the upgrade.

**3. The two fastest-dividing cells in the body are CD8 effectors (6.2 h) and
germinal-centre B cells (6–7 h) — within ~15% of each other.** So T and B
towers should share a base emission rate. Differentiating them by *rate* is
unbiological; differentiate them by what they emit, how long it persists, and
whether it acts locally or as a field.

**4. The clonal-expansion upgrade is attached to the wrong knob, and the real
ratio makes the current knob boring.** Cycle length between the slow and fast
T-cell subsets differs by only 1.4× (8.6 vs 6.2 h) — far too small a lever to
feel. But Kretschmer's own arithmetic shows **two extra days of dividing is a
64-fold difference in output.** The honest and more legible upgrade is
therefore: *clonal expansion extends how long the tower keeps emitting after
the pathogen is gone*, and raises the cap — not the rate. That also fixes the
existing problem that ×(1+0.5·level) pushes past the physiological maximum at
level 1.

**5. Towers should keep producing after the pathogen is cleared — as a decay,
not a cutoff.** Two hours of antigen commits a naive CTL (van Stipdonk); ≥7
divisions follow with no further antigen (Kaech & Ahmed); and removing antigen
slows the cycle (6.2 → 9.7 h) without stopping it, with ~100% of cells still
Ki-67⁺ at day 8 (Kretschmer). This is the biology backing the Sprint 9 decision
to stop despawning at the round boundary. **The refinement worth building: what
should suffer when the pathogen clears early is *banked knowledge*, not the
army on screen — antigen duration sets memory size, and inflammation duration
explicitly does not.** So pathogen presence should feed the knowledge meter;
cytokine-field loudness should not.

**6. The memory-recall rung (~30% knowledge) is specified backwards relative to
the biology, and the biology gives a better mechanic.** Memory's advantage is
*not* a bigger multiplier — naive cells out-expand memory cells per cell
(Martin 2011), and secondary effectors win on *not dying* (3–5× contraction vs
10–20×, Grayson). The real decomposition is: **~1,000–5,000× more starting
cells, onset within ~1 day instead of a multi-day lag, and better survival.**
So the "burst of CTLs" should be implemented as *a pre-stocked pool that
appears with zero ramp and persists longer* — not as a higher emission rate or
a bigger cap. That is both accurate and more satisfying: the player sees an
army that is already there.

**7. Do not give NK cells a clonal-expansion upgrade.** Human NK proliferation
rate is **completely unchanged** during acute EBV infection even while NK
numbers rise (Zhang 2007), and the homeostatic doubling time is 16 days — 16
rounds, i.e. never, within a campaign. Even in the mouse, NK proliferation is
"neither earlier nor faster" than the CD4 or CD8 response (Schlub 2011). **NK
are early because they are already present, not because they ramp.** Making NK
a standing garrison with no ramp at all is both correct and a genuinely
distinct mechanical identity next to the adaptive towers. (The mouse Ly49H
100–1,000-fold expansion is real but is a single receptor–ligand pair with no
human equivalent; I would not build the human-gut game on it.)

**8. The neutrophil tower has a far better mechanic available than
rate-plus-cap.** The real structure is: a **~6-day marrow pipeline** between a
progenitor dividing and a cell arriving; a surge of **5× (low signal) to 15×
(high signal)** achieved by compressing marrow transit 6.4 → 2.9 days, not by
making cells live longer; onset as fast as **4 h** if there is a pre-formed
reserve to dump; and then a **~50 h near-total shutdown of output in early
convalescence**. Plus the trade-off that surge neutrophils **migrate ~50%
worse** into tissue. That is a bank-it/dump-it/pay-a-trough tower with a
built-in quality-vs-quantity cost, and the game currently models none of it.

**9. Where the real ratio would simply break the game: adaptive expansion
magnitude.** One CD8 clone expands **10⁴–10⁵-fold**; `MaxActiveChildren = 8`
cannot represent that, and anything that could would end the game the moment it
arrived. **Do not compress this into headcount. Compress it into time.** The
adaptive army should read as *late and decisive* — it arrives around round 8,
it does not need to be numerous, and the knowledge ladder (not the population
cap) is where its power lives. The 90% capstone already does this correctly by
solving search rather than adding damage; keep that discipline for the other
rungs. Relatedly, **affinity maturation against whole organisms tops out at
10–15×, not the textbook 1,000×** (Kalinke: the big number is a hapten
artifact), so the antibody rungs should be modest multipliers, not
game-winners.

**10. Two gut numbers are free accuracy the game is not yet spending.** (a)
**Human enterocyte turnover is 3.48 days = 3–4 rounds**, so the entire wall
replaces itself every 3–4 rounds — a ready-made regrowth clock for Sprint 22's
barrier death and sentinel sloughing, with villus-tip cells migrating at
34–52 µm/h (~1.7–2.6 coarse slots/hour) and ~420 divisions per crypt per day
feeding it. (b) **The small-intestinal lumen is sparse — 10³–10⁴ bacteria/mL,
eight orders of magnitude below the colon — and both bacterial density and
Peyer's patch density climb toward the ileum** (≥46% of patches sit in the
distal 25 cm). That is a free, real difficulty gradient along the map axis.
Also: the 20 µm coarse slot checks out as enterocyte **width** (measured range
13.8–25.4 µm), but a human enterocyte is ~30 µm **tall** — it is a tall column,
not a cube, and it widens as it climbs the villus.

**Bonus — the one ratio I'd most want visible on screen.** A fighting CD8 cell
divides every 6.2 hours. A resting memory CD8 cell divides **less than once a
year** (doubling time >450 days, by deuterium labelling in humans). That is a
**~1,700× swing in the same cell between "at war" and "remembering,"** and it
is the single biggest honest ratio in the immune system. The game has no
analogue of it yet. A bone-marrow memory store that is visibly, almost
comically inert between outbreaks and then instantaneous on re-encounter would
be dramatically correct *and* would make the §5 knowledge ladder's payoff
legible.

---

## Sources

According to PubMed and Consensus. Full list with PMIDs and DOIs appears inline
in each table above. Primary sources leaned on most heavily:

- Kretschmer et al. 2020, *Nat Commun* 11:113 — PMID 31913278 — [DOI](https://doi.org/10.1038/s41467-019-13788-w)
- Blattman et al. 2002, *J Exp Med* 195:657 — PMID 11877489 — [DOI](https://doi.org/10.1084/jem.20001021)
- De Boer, Homann & Perelson 2003, *J Immunol* 171:3928 — PMID 14530309 — [DOI](https://doi.org/10.4049/jimmunol.171.8.3928)
- van Stipdonk et al. 2001, *Nat Immunol* 2:423 — PMID 11323696 — [DOI](https://doi.org/10.1038/87730)
- Kaech & Ahmed 2001, *Nat Immunol* 2:415 — PMID 11323695 — [DOI](https://doi.org/10.1038/87720)
- Murali-Krishna et al. 1998, *Immunity* 8:177 — PMID 9491999 — [DOI](https://doi.org/10.1016/s1074-7613(00)80470-7)
- Foulds et al. 2002, *J Immunol* 168:1528 — PMID 11823476 — [DOI](https://doi.org/10.4049/jimmunol.168.4.1528)
- Sun, Beilke & Lanier 2009, *Nature* 457:557 — PMID 19136945 — [DOI](https://doi.org/10.1038/nature07665)
- Schlub et al. 2011, *J Immunol* 187:1385 — PMID 21697462 — [DOI](https://doi.org/10.4049/jimmunol.1100416)
- Zhang et al. 2007, *Immunology* 121:258 — PMID 17346281
- Akondy et al. 2017, *Nature* 552:362 — PMID 29236685 — [DOI](https://doi.org/10.1038/nature24633)
- Akondy et al. 2015, *PNAS* — PMID 25713354
- Co et al. 2009, *Immunology* — PMID 19740333
- Alanio et al. 2010, *Blood* 115:3718 — PMID 20200354 — [DOI](https://doi.org/10.1182/blood-2009-10-251124)
- Grayson et al. 2002, *J Immunol* 169:3760 — PMID 12244170 — [DOI](https://doi.org/10.4049/jimmunol.169.7.3760)
- Martin et al. 2011, *J Immunol* 188:1255 — PMID 22205031 — [DOI](https://doi.org/10.4049/jimmunol.1101579)
- Veiga-Fernandes et al. 2000, *Nat Immunol* 1:47 — PMID 10881174 — [DOI](https://doi.org/10.1038/76907)
- Koutsakos et al. 2023, *Immunity* 56:879 — PMID 36958334 — [DOI](https://doi.org/10.1016/j.immuni.2023.02.017)
- Beura et al. 2018, *Nat Immunol* 19:173 — PMID 29311694 — [DOI](https://doi.org/10.1038/s41590-017-0029-3)
- Reina-Campos et al. 2025, *Nature* — PMID 39843748 (small-intestine T_RM niche structure: differentiated T_RM in the upper villus, progenitor-like T_RM in the lower villus)
