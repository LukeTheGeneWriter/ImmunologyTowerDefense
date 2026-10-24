# Antigen specificity: real numbers vs. the 8-bit barcode

Research brief for Immunology Tower Defense §5 / §5a / §5c. Every figure below was
checked against the literature via PubMed and Consensus during this session; nothing
is written from memory. Items I could not pin to a primary source are marked
**UNVERIFIED**. Mouse and human are distinguished throughout — the precursor-frequency
field is overwhelmingly mouse tetramer-enrichment work, and the human numbers come
from different assays with different sensitivity floors.

**The game's current state** (`AdaptiveTuning.cs`): 8-bit tags, `MatchMaxHammingDistance = 2`
→ 37/256 = **14.45% of random pairings teach (1 in 6.92)**, `KnowledgePerMatch = 3f`,
`DcPresentationsPerCargo = 4`, `LymphocyteLifespanSeconds = 20f`, `PairingSeconds = 1.5f`.

---

## 1. Summary table

| Quantity | Value | Species | Method | Citation |
|---|---|---|---|---|
| Naive CD4 cells per pMHC-II epitope | **20–200 cells / mouse** (3 epitopes) | mouse | pMHCII tetramer + magnetic enrichment | Moon et al. 2007, *Immunity* |
| Naive CD4 precursors, sequencing estimate | **<200 splenic precursors** | mouse | TCRβ sequencing from tetramer-sorted cells | Bousso et al. 1998, *Immunity* |
| Tetramer-enrichment detection floor (T) | **~5 epitope-specific cells / mouse** | mouse | tetramer + MACS + multiparameter flow | Moon et al. 2009, *Nat Protoc* |
| Naive CD8 precursor frequency (LCMV gp33) | **1 in 2 × 10⁵**; 100–200 cells/mouse out of 2–4 × 10⁷ naive CD8 | mouse | TCR-Tg titration into naive recipients | Blattman et al. 2002, *JEM* |
| Naive CD8 precursors, 6 specificities | **80–1200 cells / mouse** | mouse | pMHC-I tetramer + bead enrichment | Obar et al. 2008, *Immunity* |
| Naive CD8 precursor frequency, 6 epitopes | **0.6 × 10⁻⁶ to 1.3 × 10⁻⁴** (>100-fold spread, conserved across donors) | human | tetramer + bead enrichment + multiparametric cytometry | Alanio et al. 2010, *Blood* |
| Naive CD4 precursor frequency (anthrax PA) | **0.2–10 per 10⁶ naive CD4** | human | pMHCII tetramer, DRB1*01:01 donors | Kwok et al. 2012, *J Immunol* |
| Naive clonal burst on infection | 100–200 naive → ~10⁷ effectors in 1 wk (>14 divisions); ~5 × 10⁵ memory | mouse | as Blattman | Blattman et al. 2002, *JEM* |
| Antigen-specific B cell detection floor | **1 in 10⁸ cells** | human/mouse | antigen tetramer + enrichment | Phelps et al. 2024, *Nat Protoc* |
| Naive TCRβ richness, young adults | **≥10⁸ unique sequences** in naive CD4 + CD8 | human | NGS + nonparametric richness estimation | Qi et al. 2014, *PNAS* |
| Naive clonotype count / mean clone size | ≥10⁸ clonotypes, **~1000 T cells per clonotype** (highly unequal) | human | review of NGS + modelling | Sodi et al. 2026, *Immunol Rev* |
| Total TCR richness (all subsets) | **≥3.8 × 10⁸** (lower bound, from 1.9 × 10⁸ cells) | human | UMI-based TCRα/β RNA-seq | Sun et al. 2022, *JCI* |
| αβ pairing diversity (earlier lower bound) | 9 × 10⁵ β chains, 4.5 × 10⁵ α chains, **≥2.4 × 10⁷ αβ combinations** | human | V-family sampling + sequencing | Arstila / Keşmir 2000, *Science* |
| Splenic αβ repertoire | 5–8 × 10⁵ distinct BV nt sequences; **~2 × 10⁶ clones of ~10 cells each** | mouse | PCR + exhaustive sequencing | Casrouge et al. 2000, *J Immunol* |
| Theoretical TCR diversity | up to **10¹⁵** (vs. ~2 × 10⁶ realised in mouse spleen) | — | combinatorial | Casrouge et al. 2000, *J Immunol* |
| TdT contribution to TCR diversity | **≥90%**; TdT⁻/⁻ repertoire falls to ~10⁵ | mouse | sequencing of Tdt-null mice | Cabaniols et al. 2001, *JEM* |
| Naive antibody repertoire diversity | **≥10¹²** unique antibodies; ~5 × 10⁹ circulating B cells sample a small fraction | human | ~3 × 10⁹ heavy-chain sequences, 10 donors | Briney et al. 2019, *Nature* |
| Peptides recognised by one TCR (estimate) | **~10⁶ MHC-associated epitopes** | — (theory) | coverage argument | Mason 1998, *Immunol Today* |
| Peptides recognised by one TCR (measured) | **>10⁶ distinct decamers** on one MHC-I, for clone 1E6 | human | combinatorial peptide library + maths | Wooldridge et al. 2011, *JBC* |
| Size of the problem cross-reactivity solves | **<10⁸ human TCRs vs. >10¹⁵ distinct pMHCs** | human | as above | Wooldridge et al. 2011, *JBC* |
| Best agonist vs. index peptide | RQFGPDFPTI **>100× more potent**, differs at **7 of 10 positions** | human | library screen of >10⁸ peptides | Wooldridge et al. 2011, *JBC* |
| Structural basis of degeneracy | conserved "hotspot" lock-and-key footprint; substitutions outside it tolerated | human | 7 TCR–pMHC crystal structures | Cole et al. 2016, *JCI* |
| Cross-reactivity without similarity | register shifts / MHC-groove extensions, no molecular mimicry needed | human | structures of DMF5 complexes | Riley et al. 2018, *Nat Chem Biol* |
| T cells scanned by one DC per hour | **up to 5000/h**; contacts ~3 min, mostly on dendrites | mouse | two-photon, intact LN | Miller et al. 2004, *PNAS* |
| T cells scanned by one DC per hour (no Ag) | **≥500/h**; Ag-bearing DC engages **>10 T cells simultaneously** | mouse | two-photon, intact LN | Bousso & Robey 2003, *Nat Immunol* |
| DCs scanned by one T cell per hour | **~100/h** (and 2000 T cells/h per DC) | mouse | spatially explicit model + 2PM | modelling paper, see caveat §4 — **UNVERIFIED attribution** |
| Early T:DC contact duration | **11–12 min** each in the first 2 h after LN entry; serial engagement | mouse | two-photon | Miller et al. 2004 (CD4 imaging) |
| DC positioning strategy | DCs home near **high endothelial venules** and scan all incoming T cells | mouse | imaging of initial activation clusters | Bajénoff et al. 2003, *JEM* |
| Priming phase 1 | first **8 h**: multiple short encounters, motility falls, activation markers up | mouse | two-photon, synchronised dwell | Mempel et al. 2004, *Nature* |
| Priming phase 2 | next **12 h**: long-lasting stable conjugates, IL-2 and IFN-γ secreted | mouse | as above | Mempel et al. 2004, *Nature* |
| Priming phase 3 | **day 2**: motility resumes, short contacts, proliferation begins | mouse | as above | Mempel et al. 2004, *Nature* |
| Stable-contact onset (immunity vs tolerance) | stop-and-cluster at **15–20 h** = immunity; persistent brief contacts = tolerance | mouse | two-photon | Hugues et al. 2004, *Nat Immunol* |
| Contact time required for commitment | **≥6 h cumulative in vivo T:DC interaction** for naive CD4 clonal expansion | mouse | real-time manipulation + 2PM | Celli et al. 2007, *Immunity* |
| Signal integration across contacts | T cells detach before dividing, **re-engage different DCs**, integrate signals | mouse | Ag-recognition history reporter + 2PM | Celli et al. 2005, *JEM* |
| T:APC cluster transit before division | **32–36 h**; subdominant/weak agonist delayed by **6 h** | mouse | ex vivo cluster isolation | Hommel et al. 2003, *JEM* |
| Presentation duration needed for Tfh | T:DC interaction must persist into **stage 3 (>72 h)** | mouse | kinetics of Ag presentation | Benson et al. 2015, *eLife* |
| Presentation without costimulation | transient activation → **deletion/unresponsiveness within 7 d** | mouse | DEC-205-targeted antigen | Hawiger et al. 2001, *JEM* |
| LN transit, CD4 vs CD8 | CD4 traverse LN **~2× faster**, spend **~⅓ of transit** engaging MHC-II on DCs | mouse | intravital imaging + modelling | Mandl et al. 2012, *PNAS* |
| LN dwell-time distribution | mode **10–20 h**, strongly right-skewed, a significant tail **>70 h** | sheep | efferent-lymph cannulation + LASSO fit | Thomas et al. 2012, *PLoS ONE* |
| pMHC-II needed to activate CD4 | **210–340 specific complexes/APC** (0.1% of I-Ak) | mouse | quantitative immunoprecipitation | Harding & Unanue 1990, *Nature* |
| pMHC-II dose–response | **200–400** necessary and sufficient for partial; **>5000** for maximal | mouse | complex-specific mAbs | Reay et al. 2000, *J Immunol* |
| Single-ligand sensitivity (CD4) | **1 agonist pMHC** → transient Ca²⁺ signal; **~10** → immunological synapse; **~30** without CD4 | mouse | 3-D single-molecule fluorescence | Irvine et al. 2002, *Nature* |
| pMHC-I needed for CTL killing | **~3 complexes/target** at half-maximal lysis; **a single complex may suffice** | mouse | ¹²⁵I-peptide of high specific activity | Sykulev et al. 1996, *Immunity* |
| pMHC-I needed for CTL killing (other) | as few as **200 complexes/cell** (<0.08% of surface Db) | mouse | peptide binding on live cells | Christinck et al. 1991, *Nature* |
| pMHC-I requirement, spread | **several thousand to fewer than ten**, depending on pair | mouse/human | 16 peptides × 3 MHC-I | Kageyama et al. 1995, *J Immunol* |
| Lowest affinity that still activates | **K_D ~ 1 mM**; kinetic proofreading delay **2.8 s**, **2.67 steps** | human | low-affinity measurement method | Pettmann et al. 2021, *eLife* |
| Affinity window | activation impaired by mutations that **either decrease or increase** dwell time | mouse | two TCR systems | Kalergis et al. 2001, *Nat Immunol* |
| SHM rate | **~1 × 10⁻³ per bp per cell division** | mouse | GC B cell sequencing + modelling | Merkenschlager et al. 2025, *Nature* |
| SHM rate vs. background | **10⁻⁵–10⁻³ per bp per generation, ~10⁶-fold above normal** | human | BL2 cell SHM system | Poltoratsky et al. 2001, *PNAS* ([DOI](https://doi.org/10.1073/pnas.141222198)) |
| Hypermutation frequency band | **10⁻²–10⁻⁴ per bp** | — | review | Goodman 2016, *Environ Mol Mutagen* ([DOI](https://doi.org/10.1002/em.22023)) |
| SHM per cell per division | **up to one mutation per cell per division** | mouse | review | Shlomchik & Weisel 2012, *Immunol Rev* |
| GC kinetics | forms after several days, **peaks 1.5–2 weeks**, detectable **>8 weeks** | mouse | review | Shlomchik & Weisel 2012, *Immunol Rev* |
| GC death rate | **up to half of all GC B cells die every 6 h**; LZ cells die by default unless positively selected | mouse | apoptosis-indicator mice | Mayer et al. 2017, *Science* |
| DZ→LZ transit | **a few hours**; weak directional bias 0.2–0.3 µm/min on a persistent random walk | mouse | 2PM reanalysis + simulation | Beltman et al. 2011, *PNAS* |
| What T help buys a GC B cell | faster S phase, prolonged DZ dwell, more divisions per passage | mouse | genome seq + single-molecule | Gitlin et al. 2015, *Science* |
| Cyclic re-entry | GC B cells exit cycle hours before entering LZ; re-entry and "refuelling" independently regulated | mouse | Fucci2 reporter | Long et al. 2022, *Sci Immunol* |
| Maximum observed GC duration | **≥6 months (191 days)** of continuous SHM and selection, no further antigen | rhesus | slow-delivery HIV Env prime | Lee et al. 2022, *Nature* |
| Affinity gain from one key mutation | **10×** from a single VH W33L exchange (anti-NP) | mouse | site-directed mutagenesis | Allen et al. 1988, *EMBO J* |
| Overall naive → matured affinity gain | "**orders of magnitude**" on average; precise fold is antigen-specific | mouse/human | review | Eisen 2014, *Cancer Immunol Res*; magnitude as a single number **UNVERIFIED** |
| What limits rare B cell clones | precursor frequency × affinity × avidity, **interdependently** limiting in GC competition | mouse (human BCR knock-in) | adoptive transfer | Abbott et al. 2017, *Immunity*; Huang et al. 2020, *PNAS* |
| H3N2 antigenic evolution shape | antigenic evolution **more punctuated** than genetic; single substitutions can have outsized antigenic effect | human virus | antigenic cartography 1968–2003 | Smith et al. 2004, *Science* ([DOI](https://doi.org/10.1126/science.1097211)) |
| H3N2 antigenic clusters | **13 clusters over 1968–2011** → **~3.3 years per cluster** (derived) | human virus | cartography + full-genome analysis | Westgeest et al. 2013, *J Virol* |
| Transition interval, stated | **every 2–8 years** | human virus | phylodynamic model | Castro et al. 2019, *PLoS Comput Biol* |
| Relative drift rates | H3N2 evolves faster and more punctuated than H1N1, B/Vic, B/Yam; year-to-year drift drives incidence | human virus | antigenic + genetic joint model | Bedford et al. 2013, *eLife* |
| Protection lost to one drift step | VE **7% (95% CI −14 to 23)** vs. drifted A(H1N1)pdm09 clade, vs. **40–60%** historical; 39% overall that season | human | test-negative design, 8845 enrollees | Tenforde et al. 2020, *CID* |
| Imprinting / original antigenic sin | first-ever IAV infection gives **75% protection vs. severe disease, 80% vs. death** against a novel same-group HA, for life | human | all known H5N1/H7N9 cases + modelling | Gostic et al. 2016, *Science* |
| Imprinting, seasonal | primary infection reduces risk of medically attended infection with that subtype **throughout life**; stronger for H1N1 | human | VE studies + statistical model | Arevalo et al. 2019, *eLife* |
| OAS, mechanism | re-infection boosts **stalk** antibodies that **do not bind the boosting strain well** | human + ferret | serology | Arevalo et al. 2020, *PNAS* |
| First gut T cell activation | Salmonella-specific CD4 show activation in **Peyer's patches as early as 3 h** after oral infection | mouse | adoptive transfer tracking | McSorley et al. 2002, *Immunity* |
| Gut effector/Trm generation | oral Listeria → robust intestinal CD8 response; intestinal Trm required for secondary protection | mouse | oral infection model | Sheridan et al. 2014, *Immunity* |
| Gut antibody kinetics | ASC peak **day 5–7**; faecal secretory IgA peak **day 7** after dose | human | oral cholera vaccine, Bangladesh | Akhtar et al. 2017, *Vaccine* |
| Gut IgA character | highly synchronised, **oligoclonal**, affinity-matured, re-uses existing GCs across multiple Peyer's patches — hence repeat dosing is mandatory | mouse | NP-CT oral immunisation, lineage trees | Bergqvist et al. 2012, *Mucosal Immunol* |
| DC division of labour | **resident** DCs trap and alert; **migratory** DCs license expansion | mouse | subcutaneous immunisation | Allenspach et al. 2008, *Immunity* |

---

## 2. Question by question

### 1. Precursor frequency — and the size of the gap

**Mouse, CD4.** Moon et al. tetramer-enriched three different pMHC-II specificities and
found **20–200 naive CD4 cells per mouse** — and, crucially for the game, that naive
population size *predicted* both the magnitude and the TCR diversity of the primary
response. Variation in naive frequency is why some peptides are stronger immunogens
than others. Bousso's earlier sequencing work independently put the figure at
**<200 splenic precursors**, differing in each individual mouse.

**Mouse, CD8.** Blattman's titration method gave **1 in 2 × 10⁵** for LCMV gp33 — 100–200
cells in a mouse holding 2–4 × 10⁷ naive CD8. Obar's direct tetramer enumeration across
six specificities found **80–1200 cells/mouse**, and showed precursor number controls
response kinetics, immunodominance and memory lineage decisions.

**Human.** Different assays, broadly concordant numbers. Alanio measured six CD8 epitopes
at **0.6 × 10⁻⁶ to 1.3 × 10⁻⁴** — a >100-fold spread across epitopes but remarkably
conserved *between people*. Kwok measured CD4 at **0.2–10 per million naive CD4**, and
explicitly noted this is comparable to the murine repertoire.

**B cells.** The field quantifies rarity rather than a canonical frequency: antigen-tetramer
enrichment is pushed to detect B cells **as rare as 1 in 10⁸** (Phelps 2024), and the HIV
germline-targeting literature exists precisely because bnAb-precursor B cells are vanishingly
rare. Abbott and Huang show precursor frequency, affinity and avidity are *interdependently*
limiting for whether a rare clone wins its germinal centre. **A single canonical "naive B
cells specific for antigen X per million" figure is UNVERIFIED** — it is antigen-dependent
and the literature reports it per-immunogen, not as a constant.

**The gap, quantified.** The game's rule (8 bits, Hamming ≤ 2) teaches on **37/256 = 14.45%**
of random pairings — 1 in 6.92. Against the measured frequencies:

| Reality | Frequency | Game is more permissive by |
|---|---|---|
| Mouse CD8, gp33 (Blattman) | 5 × 10⁻⁶ | **~29,000×** |
| Mouse CD4 (~100 cells / 2 × 10⁷) | 5 × 10⁻⁶ | **~29,000×** |
| Human CD4 (Kwok, mid-range) | 1 × 10⁻⁶ | **~145,000×** |
| Human CD8, rarest (Alanio) | 6 × 10⁻⁷ | **~241,000×** |
| Human CD8, commonest (Alanio) | 1.3 × 10⁻⁴ | **~1,100×** |

So: **the game runs 3 × 10⁴ to 2 × 10⁵ times too permissive — four to five orders of
magnitude.** Even dropping to exact match (1/256 = 3.9 × 10⁻³) leaves a ~780× gap to the
mouse number; **barcode length, not the Hamming threshold, is the binding constraint**, which
inverts §5c's stated assumption that the threshold is the dial to reach for first.

**What the gap costs in realism.** Three specific things, in descending order of how much
they matter:

1. **It erases the single most consequential fact about adaptive immunity — that it is slow
   because it is rare.** The 3–7 day lag before adaptive immunity does anything is *entirely*
   a consequence of having to find and then amplify ~100 cells. At 1-in-7 the game has no
   reason for a lag, so the lag has to be imposed artificially by `PairingSeconds` and
   emission rates rather than emerging from specificity.
2. **It makes the clonal burst invisible.** Blattman's 100–200 naive cells divide >14 times
   in a week to reach 10⁷. The game has no expansion step at all: a matching T cell just
   increments a counter and the repertoire turns over. The most dramatic number in the whole
   system — a **>1000-fold increase in precursor frequency from naive to memory** — is
   unrepresented, and it is exactly the kind of visible feedback the design otherwise prizes.
3. **It makes "knowledge is per species" cheap.** The three class antigens are ≥4 bits apart
   so a tag matches at most one, which is sound. But with 14.5% coverage per tag, a node of 8
   lymphocytes has a ~71% chance of containing a match to any given species at any moment
   (1 − 0.855⁸). Specificity is therefore almost never the binding constraint in play; DC
   travel time is. The barcode is decorative at current tuning.

### 2. Repertoire size and diversity — is 8 bits the right length?

**Human TCR.** The modern lower bound is **≥10⁸ distinct naive TCRβ sequences** in young
adults (Qi 2014), restated in a 2026 review as ≥10⁸ naive clonotypes at a mean clone size of
~1000 cells, with very unequal clone sizes. Sun's UMI-based deep sequencing of 1.9 × 10⁸ cells
puts total richness at **≥3.8 × 10⁸**. The older Arstila estimate (**≥2.4 × 10⁷** αβ
combinations, from 9 × 10⁵ β and 4.5 × 10⁵ α chains) is a deliberate lower bound — Keşmir's
comment notes it assumes each β chain pairs with exactly one α.

**Mouse TCR.** Casrouge: 5–8 × 10⁵ distinct BV nucleotide sequences in a whole spleen, and
**~2 × 10⁶ clones of ~10 cells each** — against a theoretical maximum "estimated up to 10¹⁵."
That is a 10⁹-fold shortfall of realised against theoretical, and Casrouge's own conclusion is
that 2 × 10⁶ is nonetheless "large enough to maintain a high functional diversity." Cabaniols
showed TdT alone supplies ≥90% of that diversity.

**B cells.** Briney: naive antibody repertoire diversity **≥10¹²**, while only ~5 × 10⁹ B cells
circulate — so the blood samples a small fraction of the repertoire at any instant, and the
rearranged-receptor "genome" of one person exceeds the human genome by >4 orders of magnitude.

**Converted to bits** (log₂ of realised diversity — i.e. how long a tag must be to name each
distinct receptor once):

| Repertoire | Size | Bits |
|---|---|---|
| Mouse splenic αβ clones | 2 × 10⁶ | **20.9** |
| Human naive TCRβ (Qi) | 10⁸ | **26.6** |
| Human total TCR richness (Sun) | 3.8 × 10⁸ | **28.5** |
| Human naive antibody (Briney) | 10¹² | **39.9** |
| Nonamer peptide space (20⁹) | 5.1 × 10¹¹ | **38.9** |
| pMHC space (Wooldridge) | >10¹⁵ | **~49.8** |
| Theoretical TCR diversity | 10¹⁵ | **~49.8** |

**Direct answer: 8 bits is roughly a third of the honest length.** A mouse needs ~21 bits, a
human ~27 bits, and peptide space itself needs ~39 bits. The design doc's own framing ("20⁹,
around half a trillion") is correct and corresponds to **38.9 bits** — the doc then jumps to 8
without an intermediate. There is a defensible middle: **20–21 bits is the mouse repertoire,
to the bit.** More on this in §3.

### 3. Cross-reactivity — the game's real justification, assessed

This is the strongest ground the design stands on, and it is stronger than §5c currently claims.

**The coverage argument.** Mason's 1998 argument is exactly the game's: a comprehensive
response requires that one T cell reacts productively with **~10⁶ different MHC-associated
epitopes**. Sewell's 2012 review states the compromise explicitly — clonal selection theory
proposed one T cell per antigen, but the αβ TCR repertoire "is dwarfed by the vast array of
potential foreign peptide–MHC complexes, and a comprehensive system requires each T cell to
recognize numerous peptides." Wooldridge measured it: a single human CD8 clone recognises
**>10⁶ distinct decamers** on one MHC-I molecule, framed against **<10⁸ human TCRs vs. >10¹⁵
distinct pMHCs**. Without degeneracy there would be holes in coverage that pathogens would
evolve to exploit.

**Why a distance metric is the right shape, not just a convenient one.** Three findings support
a graded-mismatch rule over exact match:

- The best agonist Wooldridge found, RQFGPDFPTI, was **>100× more potent than the index
  peptide despite differing at 7 of 10 positions**. Recognition is emphatically not
  "all bits must agree."
- Cole's structures explain why: binding runs through a conserved **"hotspot" lock-and-key
  minimal footprint**, and the TCR "tolerates vast numbers of substitutions outside of this
  hotspot." That is a *weighted* Hamming distance — some positions are free, a few are not.
- Riley showed cross-reactivity can even proceed with no physicochemical commonality at all,
  via peptide register shifts and extensions out of the MHC groove.

**Verdict on the analogy.** Hamming distance is a *better* abstraction than §5c gives itself
credit for — it captures the central fact that recognition degrades gracefully with sequence
change, which exact match cannot. The problem is purely the **radius relative to the length**.

| Measure | Fraction of ligand space one receptor covers |
|---|---|
| Mason (10⁶ of 20⁹ nonamers) | 2.0 × 10⁻⁶ (1 in 512,000) |
| Wooldridge (10⁶ of 10¹⁵ pMHC) | 1.0 × 10⁻⁹ (1 in 10⁹) |
| **Game (37 of 256)** | **1.4 × 10⁻¹ (1 in 6.9)** |

Biology's cross-reactivity is enormous *in absolute count* and microscopic *as a fraction*.
The game has it the other way round. **Hamming ≤ 2 is not defensible on 8 bits** — the Hamming
ball swallows 14% of the space. It is very defensible on a longer tag:

| Bits | r ≤ 0 | r ≤ 1 | r ≤ 2 | r ≤ 3 |
|---|---|---|---|---|
| 8 | 3.9e−3 | 3.5e−2 | **1.4e−1** | 3.6e−1 |
| 16 | 1.5e−5 | 2.6e−4 | 2.1e−3 | 1.1e−2 |
| 20 | **9.5e−7** | 2.0e−5 | 2.0e−4 | 1.3e−3 |
| 24 | 6.0e−8 | 1.5e−6 | 1.8e−5 | 1.4e−4 |
| **32** | 2.3e−10 | 7.7e−9 | **1.2e−7** | 1.3e−6 |

**32 bits at Hamming ≤ 2 lands at 1.2 × 10⁻⁷ — squarely inside the measured 10⁻⁶–10⁻⁹
cross-reactivity window**, and 20 bits at exact match gives 9.5 × 10⁻⁷, essentially the
measured human CD4 precursor frequency. Those are the two honest settings, and they differ
from the current one by a factor of ~10⁶.

### 4. How a T cell finds its antigen

**Scanning rates.** Bousso & Robey: a DC scans **≥500 different T cells per hour** in the
absence of antigen; an antigen-bearing DC can **engage more than ten T cells simultaneously**,
for *hours* not minutes. Miller, with ~3 min contacts occurring on extended dendrites rather
than the soma, puts the ceiling at **up to 5000 T cells per hour per DC**. A spatially explicit
model fitted to 2PM data estimated 2000 T cells/h per DC and **~100 different DCs per hour
scanned by a T cell** — I could not resolve that paper's title or authorship through the search
interface (it returned as "Online Supplemental Material"), so treat the 100 DCs/h figure as
**UNVERIFIED attribution**, though it is consistent with the measured DC-side numbers.

**Critically for the game's design: Miller found T cells encounter DCs by chance.** T cells did
not accumulate around DCs, and their velocities approaching and departing were equivalent —
"implying that T cells are not attracted toward DCs by chemotactic gradients but rather
encounter them by chance." What *is* real is **positional** bias, not chemotactic: Bajénoff
showed tissue-derived DCs preferentially home near high endothelial venules, i.e. they park
themselves on the entry route and scan everything coming in. **§5c's "second cytokine"
co-localisation field is the one piece of the barcode design with no direct biological
referent** — the real solution is geometry plus brute-force throughput. (That said, it is
defensible as a *gameplay* stand-in for DC positioning at HEVs; it just shouldn't be described
as modelling a chemokine that pulls T cells to DCs.)

**Time to find antigen.** No single clean "X hours to first cognate contact" figure exists;
the field reports it as transit and dwell statistics. Mandl: CD4 T cells recirculate faster,
home to LNs more efficiently, traverse a LN **twice as fast** as CD8, and spend **~⅓ of their
transit time interacting with MHC-II on DCs**. Thomas: LN dwell time has a **mode of 10–20 h**
with a long tail past 70 h. McSorley provides the fastest in-gut figure: Salmonella-specific
CD4 T cells show signs of activation in Peyer's patches **as early as 3 h after oral infection**.

**Contacts needed before commitment — the answer is a time integral, not a count.** Celli 2007
is decisive: **at least 6 hours of in vivo T cell–DC interaction are required for naive CD4
cells to undergo clonal expansion**; the first encounter is often already long-lived, and
cessation of TCR–MHC signalling is what causes dissociation (so antigen availability on the DC
sets contact length). Celli 2005 adds that T cells detach before dividing and then re-engage
*different* DCs antigen-specifically, **integrating signals across successive contacts**. So
the biological analogue of `DcPresentationsPerCargo` is not "4 presentations then empty" but
"a cumulative 6-hour dose that may be delivered by several DCs."

**Three-phase model** (Mempel 2004, mouse, synchronised LN dwell):

- **Phase 1, first 8 h** — multiple short encounters, progressively falling motility, activation markers up.
- **Phase 2, next 12 h** — long-lasting stable conjugates; IL-2 and IFN-γ secretion begins.
- **Phase 3, day 2 onward** — motility resumes, short contacts return, proliferation starts.

Hugues independently places the stop-and-cluster transition at **15–20 h**, and — important for
a game that might want a failure mode — shows that T cells which *keep* making brief serial
contacts instead become **tolerised**. Jobin 2025 adds a fourth, post-detachment phase of
CXCR3-dependent hours-long re-engagement in subfollicular niches, favouring high-affinity clones.
Hommel's cluster-compartment measurement (**32–36 h** transit before division, with a **6 h
delay** for a subdominant epitope) is the cleanest single number for "antigen arrival → first
division."

**How long a DC stays able to prime.** No sharp expiry; what the literature gives is: Tfh
generation requires presentation to persist and hold T:DC interactions into **stage 3, >72 h**
(Benson 2015); and presentation *without* costimulation produces transient activation followed
by **deletion and unresponsiveness within 7 days** (Hawiger 2001). Allenspach adds a division
of labour the game could use directly — **resident DCs trap and alert the T cell, migratory
DCs license its expansion**. **A specific "DC loses presentation capacity after N hours"
figure is UNVERIFIED**; the honest statement is that priming competence degrades over days,
and that maturation state matters more than elapsed time.

### 5. Signal thresholds

**CD4 / MHC-II.** Harding & Unanue: T cells were activated by APCs expressing as few as
**210–340 specific peptide/class II complexes**, just 0.1% of total I-Ak. Reay's
complex-specific mAbs agree: **200–400 complexes necessary and sufficient** for some
stimulation, **>5000** for maximal — and their analysis concluded that T cell activation is
**a stochastic process**, which is a direct endorsement of the game's per-pairing roll.
Irvine's single-molecule imaging shows the floor is far lower than the population threshold:
**a single agonist pMHC** elicits a transient Ca²⁺ signal, **~10 agonists** suffice to organise
a synapse, and **~30** are needed if CD4 is blocked.

**CTL / MHC-I — the famously small number.** Sykulev: at the peptide concentration causing
half-maximal lysis, **only ~3 peptide molecules on average were bound per target cell**, and
from the distribution of Ld across cells the authors argue **a single peptide–MHC complex per
target can trigger the cytolytic response.** Christinck independently found **as few as 200
complexes** sufficed in another system; Kageyama surveyed 16 peptides × 3 MHC-I and found the
requirement spans **several thousand down to fewer than ten**, depending on the pair. So the
headline "one complex can kill" is real but is the extreme of a wide distribution.

**Affinity range.** Pettmann measured the floor: primary human T cells respond to pMHC with
**K_D as low as ~1 mM** — discrimination is enhanced but *imperfect*, which is why self-pMHC
can drive autoimmunity and anti-tumour responses. The fit gave a kinetic-proofreading delay of
**2.8 s** over **2.67 steps**. Kalergis adds that affinity is not monotonic: activation is
impaired by mutations that **either decrease or increase** the TCR–pMHC binding half-life, so
there is an **optimal dwell-time window**, and both very low and very high affinity clones are
excluded from the expanding population.

**Game implication worth flagging.** Pettmann's 2.8 s proofreading delay is the real-world
analogue of `PairingSeconds = 1.5f`. The game's pairing freeze is biologically well-motivated —
it just currently stands in for the *whole* 6-hour commitment integral rather than for the
seconds-scale discrimination step.

### 6. B cells and affinity maturation

**Cycling.** GC B cells iterate between the dark zone (proliferation + SHM) and the light zone
(selection). DZ→LZ transit takes **a few hours** on a persistent random walk with only a small
directional bias (0.2–0.3 µm/min; Beltman). Cells exit the cell cycle hours before entering the
LZ and must re-enter to continue — "cyclic re-entry" — and Long showed re-entry initiation and
T-cell "refuelling" are independently regulated, giving a graded rather than binary selection.
Gitlin showed what T help actually buys: faster S phase and a **prolonged DZ dwell with more
divisions per passage**, so selected cells both expand more and get more mutation attempts.

**Duration.** GCs form after several days, **peak at 1.5–2 weeks**, and persist **>8 weeks** in
mice (Shlomchik & Weisel). The upper bound is far longer than the textbook: with slow-delivery
priming, Lee observed GC B cells active and accumulating SHM for **at least 191 days with no
further antigen exposure**, with a 186-fold increase in GC B cells at week 10. **"Number of
rounds" is not reported as a count** — the field measures time and death rate, not cycle count.
Dividing an 8-week GC by a ~6–12 h cycle implies roughly 100–200 passages, but that arithmetic
is mine, not a cited figure.

**Selection stringency.** Mayer is the number to use: **up to half of all GC B cells die every
6 hours**, and the two zones kill for different reasons — LZ cells die *by default* unless
positively selected, DZ cells die when AID damages their antigen receptors. This is a brutal
per-cycle survival rate and is the single most game-legible fact in this section.

**SHM rate vs. background.** Merkenschlager states the accepted figure: **~1 × 10⁻³ mutations
per base pair per cell division**, and shows it is not actually fixed — high-affinity cells
shorten G0/G1 and mutate *less* per division, protecting good lineages. Poltoratsky gives both
the range and the comparison: **10⁻⁵–10⁻³ per bp per generation, about 10⁶-fold above normal.**
Goodman's review brackets hypermutation generally at 10⁻²–10⁻⁴ per bp. Shlomchik's framing is
the most intuitive: **as many as one mutation per cell per division**.

**Affinity gain.** Allen's dissection of the anti-NP response found a **single VH W33L exchange
gives 10× higher affinity** and that all other somatic exchanges were irrelevant to selection —
a strikingly discrete result. Eisen's review frames the overall shift as early low-affinity IgM
giving way to IgG/IgA "with, on average, **orders of magnitude** higher affinity." **A single
canonical fold-improvement figure is UNVERIFIED** and probably shouldn't be quoted as one; the
defensible statement is 10× from one good mutation, 10²–10³× overall for a well-matured response.

**Assessment: how well does a rising knowledge % map onto affinity maturation?** Better than it
maps onto anything else in immunology, with two real mismatches:

- **Right:** affinity maturation genuinely is a quantity that climbs monotonically over
  repeated, stochastic, T-cell-gated selection rounds, driven by discrete events. A percentage
  that rises per successful pairing is a fair abstraction of "the response is getting better."
  The game's dependency on *meetings* rather than a clock mirrors the GC's dependence on
  limiting Tfh help, which is the actual rate-limiting resource.
- **Wrong (1): the game's number only goes up.** Real GCs lose half their population every six
  hours; progress is bought against constant attrition. The game has no decay and no failure
  mode — the only thing that *removes* knowledge is the §5 mutation discount.
- **Wrong (2): one number per species conflates affinity with breadth.** Bergqvist's gut data
  shows real intestinal IgA responses are **oligoclonal** and need **repeated oral
  immunisations** precisely because GCs are re-used rather than re-founded — i.e. the response
  gets deeper on a narrow set of epitopes, which is exactly what makes antigenic escape possible.
  A single rising % cannot express "deep but narrow," which is the state that makes §7's
  mutation discount dramatic.

### 7. Antigenic variation and memory

**How much change breaks immunity.** Smith's antigenic cartography is the canonical quantitative
treatment: H3N2 antigenic evolution from 1968 to 2003 was **more punctuated than its genetic
evolution**, and **genetic change sometimes had a disproportionately large antigenic effect** —
i.e. the mapping from mutation to immune escape is lumpy, not linear. That is precisely the
justification for the game modelling mutation as a **discrete step-change** rather than
continuous erosion; §5's claim is literature-supported.

**How often a cluster transition happens.** Westgeest mapped H3N2 evolution to **13 antigenic
clusters between 1968 and 2011** — 43 years / 13 clusters ≈ **one transition every ~3.3 years**
(derivation mine from their cluster count). Castro states the interval directly as **every two
to eight years**. Bedford shows H3N2 drifts faster and more punctuated than H1N1, B/Victoria or
B/Yamagata, and that year-to-year antigenic drift drives incidence within each lineage.

**How much protection is lost.** Tenforde's 2019–20 test-negative study (8845 enrollees) is the
cleanest real figure: against antigenically drifted A(H1N1)pdm09 clade 6B.1A 183P-5A+156K
viruses, **VE was 7% (95% CI −14 to 23)** — statistically indistinguishable from zero — versus
**40–60% historically** and 30% against A(H1N1)pdm09 overall that season. Against the
better-matched B/Victoria it was 45%. **One cluster transition costs roughly 80–100% of
existing protection**, not a shaving.

**Recommended discount size for the game.** The literature supports a **large, sharp drop —
60–85% of accumulated knowledge removed in a single event** — not a 10–20% trim. Two
refinements worth having:

- **Don't zero it.** Tenforde's own conclusion: "even in drifted years, significant protection
  can be observed," and the 2026 subclade-K study found preserved cross-reactivity where
  ferret assays had predicted marked drift. A floor of ~15–25% residual knowledge is both
  defensible and kinder.
- **Make it lumpy and unpredictable.** Smith's central finding is the *decoupling* of genetic
  from antigenic change. A model where most mutations do nothing and occasional ones are
  catastrophic is more accurate than a steady drip — and better drama.

**Imprinting / original antigenic sin — a mechanic the game could genuinely earn.** Gostic is
the strongest number in this whole brief: an individual's **first ever influenza A infection
confers lifelong protection against severe disease from novel HA subtypes in the same
phylogenetic group — 75% against severe infection and 80% against death**, for both H5N1 and
H7N9, and this explains the otherwise-baffling age distributions of those two viruses. Arevalo
2019 shows the same for seasonal subtypes. The flip side is the "sin": Arevalo 2020 found
re-infection boosts **stalk antibodies that do not bind the boosting strain efficiently** —
you are rewarded for your first encounter and partly *mis*-armed against later ones.

Game-legible version: **the first species of a class a player ever learns sets a permanent
group-level floor** (say 20–30% knowledge against any later species sharing its class, for
free, forever), **but new species of that class also partly re-boost the imprinted tag instead
of building their own**. That is a mechanic with a real diminishing-returns tension in it, and
it is one of the few places where accurate immunology hands the designer a clean trade-off
rather than a constraint.

### 8. Timing of the whole adaptive response

| Milestone | Time | Source |
|---|---|---|
| Antigen-specific T activation in Peyer's patches, oral infection | **as early as 3 h** | McSorley 2002 |
| Phase 1: serial short T:DC encounters | **first 8 h** | Mempel 2004 |
| Cumulative contact needed to commit to expansion | **≥6 h** | Celli 2007 |
| Stop-and-cluster / stable conjugates, IL-2 + IFN-γ | **15–20 h** | Mempel 2004; Hugues 2004 |
| Antigen arrival → **first division** | **32–36 h** (+6 h for a subdominant epitope) | Hommel 2003 |
| Proliferation under way, motility resumed | **day 2** | Mempel 2004 |
| Clonal burst: 100–200 naive → ~10⁷ effectors | **~7 days, >14 divisions** | Blattman 2002 |
| Effector/Trm established in gut tissue | **~day 7 and onward**; Trm needed for secondary protection | Sheridan 2014 |
| Antibody-secreting cells detectable (oral vaccine) | **day 5–7** | Akhtar 2017 |
| Faecal secretory IgA peak | **day 7** | Akhtar 2017 |
| GC peak | **1.5–2 weeks** | Shlomchik & Weisel 2012 |
| GC persistence | **>8 weeks**, up to **191 days** with slow delivery | Shlomchik 2012; Lee 2022 |

**Textbook "IgM at 5–7 d, IgG at 10–14 d" for a naive primary response is UNVERIFIED** — I did
not find a primary source for it in this session. The oral-cholera numbers above are from a
two-dose schedule in a partly primed endemic population and should be labelled as such.

**What should one round represent?** Work backwards from the design's own constraints: a round
is **~30 s** (`FoodItemTransitSeconds` 30), knowledge should climb over several rounds, and the
knowledge ladder's first rung (CTLs, ~10%) should land after a handful of rounds.

The biology gives a natural unit. The interval from antigen arriving in a node to the first
T cell division is **32–36 hours**, and effector cells reach gut tissue around **day 7**. If
the first rung is "CTLs exist," it should arrive after ~7 days of in-fiction time. At 3 points
per match and a 10% threshold, that is ~4 teaching events — so **roughly 1.5–2 days per round**.

> **Recommendation: one round = 2 days of biological time.**

That choice is coherent in several directions at once:

- A round (2 d) is slightly longer than the 32–36 h antigen-to-first-division interval, so
  "this round's antigen produces next round's cells" is literally true, and the one-round lag
  the player feels is the real priming delay.
- **Rung 1 (CTLs, ~10%)** lands around round 4 ≈ **day 8** — matching effector T cells reaching
  gut tissue at ~day 7 (Sheridan).
- **Rung 2 (neutralising antibody, ~20%)** lands around round 7 ≈ **day 14** — matching the
  GC peak at 1.5–2 weeks and faecal SIgA becoming detectable in the first week or two.
- **Rung 6 (secretory IgA, ~70%)** lands around round 23 ≈ **day 46** — inside the >8-week GC
  window, which is exactly right for a high-affinity mucosal antibody.
- The **~90% capstone** then sits near **day 60**, i.e. the end of a sustained germinal centre.
  The campaign is one GC reaction, which is a genuinely elegant framing.
- Under this clock `LymphocyteLifespanSeconds = 20f` (two-thirds of a round) reads as ~32 h of
  repertoire turnover, and `PairingSeconds = 1.5f` reads as ~2.5 h of contact — the right order
  of magnitude for Mempel's phase-1 encounters, though short of Celli's 6 h commitment integral.

Say it in the fiction as **"one round is about two days in the gut."** It costs nothing and it
makes every threshold on the ladder land on a real date.

---

## 3. Judging the 8-bit barcode

### Is 8 bits defensible?

**As a number of distinguishable antigens: no. As a game mechanic: yes, with one caveat.**

The honest lengths are **~21 bits** (mouse realised repertoire), **~27 bits** (human), and
**~39 bits** (nonamer peptide space — which is what §5c's own "20⁹, around half a trillion"
actually equals). 8 bits is about a fifth of what the design doc itself identifies as the
target. The doc's reasoning for 8 — "small enough to actually match by chance in a game, and
large enough that a match *feels* like specificity" — is sound game reasoning, but it quietly
assumes barcode length and match probability must be coupled. **They don't have to be.** The
length sets how many distinguishable species the system can hold; the match rule sets how often
pairings teach. Conflating them is the actual design error, not the number 8.

The caveat that *does* bite: 8 bits gives 256 values, and the three class antigens are spaced
≥4 bits apart so a tag is within distance 2 of at most one. That works for 3 classes. It will
not survive the species roster `SPRINT_PLAN.md` item 1 implies — packing more than about **8–10
species** into 8-bit space with ≥5-bit separation becomes impossible, and once separations
shrink, a single T cell starts teaching two species at once and "knowledge is per species"
breaks silently. **8 bits is a roster ceiling, and that is a more urgent reason to widen it
than realism is.**

### Is Hamming ≤ 2 defensible?

**The metric: yes, strongly. The radius: no.**

Hamming distance is a genuinely good model of TCR cross-reactivity — Wooldridge's best agonist
differed from the index peptide at 7 of 10 positions, and Cole's structures show a conserved
hotspot with free positions around it, which is a weighted Hamming rule in all but name. That
is a stronger justification than §5c currently claims for itself; the doc presents the threshold
apologetically, as "the obvious dial if exact match proves too slow," when it is actually the
more accurate rule.

The radius is the problem. **r ≤ 2 of 8 bits covers 14.45% of tag space.** Real single-receptor
coverage is 2 × 10⁻⁶ (Mason) to 10⁻⁹ (Wooldridge). The game is off by **5–7 orders of
magnitude** on the quantity the barcode exists to represent. And note which lever fixes it:
dropping to exact match on 8 bits only gets to 3.9 × 10⁻³, still ~780× too permissive. **Length
is the binding constraint; the threshold is the fine adjustment.** §5c has this backwards.

### What a more honest abstraction looks like

Two defensible settings, both reachable without restructuring anything:

**Option A — 20 bits, exact match.** Teach rate 9.5 × 10⁻⁷, essentially identical to the
measured human CD4 precursor frequency (Kwok: 0.2–10 per 10⁶). Clean to explain: "one T cell,
one antigen." Loses the cross-reactivity story entirely.

**Option B — 32 bits, Hamming ≤ 2.** Teach rate 1.2 × 10⁻⁷, inside the measured 10⁻⁶–10⁻⁹
cross-reactivity window, and keeps the graded-mismatch rule that is the biologically truer one.
**This is the honest version.** It also solves the roster ceiling: 32 bits holds hundreds of
species at comfortable separation.

A fully honest model would add three things the barcode alone can't express: **clonal expansion**
(a matched T cell should *multiply*, not just increment — Blattman's >1000-fold), **affinity**
as a continuous quality distinct from match/no-match (Kalergis's dwell-time window; Pettmann's
1 mM floor), and **cumulative commitment** rather than instantaneous matching (Celli's 6-hour
integral delivered across several DCs).

### Would the more honest version be a worse game? — Yes, as a drop-in. Bluntly.

Do the arithmetic. The node supports ~4 DCs; at `PairingSeconds = 1.5` the node's theoretical
ceiling is ~160 pairings/minute. Reaching 100% knowledge at 3 points per match needs **34
matches**, i.e.:

| Rule | Pairings needed for 100% | At node throughput |
|---|---|---|
| 8 bits, r ≤ 2 (current) | 231 | ~1.4 min of perfect pairing |
| 8 bits, r ≤ 1 | 947 | ~6 min |
| 8 bits, exact | 8,525 | ~53 min |
| **20 bits, exact** | **3.5 × 10⁷** | **~150 days** |
| **32 bits, r ≤ 2** | **2.8 × 10⁸** | **~3.3 years** |

An honest barcode makes the game **unplayable by a factor of ~10⁵**, and no amount of tuning
`KnowledgePerMatch` disguises that — you would be multiplying a number that fires once per
geological age.

**But that is the wrong comparison, and this is the important point of the whole brief.**
Biology does not make each pairing likely. It makes pairings *astronomically numerous*:

- One DC scans **500–5000 T cells per hour** (Bousso; Miller).
- An antigen-bearing DC holds **>10 T cells at once** (Bousso).
- A draining LN holds ~10⁷ T cells with hundreds of antigen-bearing DCs.

At a precursor frequency of 10⁻⁵ and 5000 scans/h, one DC meets a cognate T cell every **20
hours** — but **100 antigen-bearing DCs produce a cognate contact every ~12 minutes node-wide**,
and 1000 produce one every ~1.2 minutes. *That* is how the real system reconciles 1-in-10⁵ with
a response that starts in 32 hours. **The search problem is solved by throughput, not by
permissiveness** — which is, verbatim, the thesis the rest of this game already builds on.

So the real design choice is not "realistic vs. fun." It is:

- **Keep 1-in-7 and ~4 DCs:** fun, fast, but the barcode is decorative and the player learns
  nothing true about specificity.
- **Go to 1-in-10⁵ and scale throughput to match:** honest, but requires hundreds of simultaneous
  DC:T pairings per second, which is a rendering and simulation problem, not a tuning one — and
  individual pairings stop being legible events the player can watch, which destroys §5c's
  best feature (the visible, costly, frozen pairing).

**The pairing freeze and the honest precursor frequency are mutually exclusive.** You cannot
show the player each meeting *and* have meetings be rare enough to matter. Pick one. Given the
Director's stated taste for visible feedback, keeping legible pairings is the right call — but
then the barcode should be sold as **cross-reactivity**, which it genuinely models in *shape*,
rather than as **specificity**, which it misses by five orders of magnitude.

### The middle path I'd actually recommend

**Widen the tag to 16 bits, keep Hamming ≤ 2 (teach rate 2.1 × 10⁻³, ~1 in 480), and raise
throughput ~10–30× to compensate** — more lymphocytes resident in the node, more DCs, a shorter
pairing freeze for *misses* and a long one only for *hits*.

Why this and not Option B:
- 2.1 × 10⁻³ is still ~400× too permissive, but it is **two orders of magnitude closer** than
  today and it keeps the game playable within a round or two of retuning.
- Asymmetric pairing cost (fast rejection, slow commitment) is **more biologically accurate than
  the current symmetric freeze**: Celli showed cessation of TCR–MHC signals causes dissociation,
  so a non-match genuinely *should* break faster than a match. It also reproduces Mempel's
  phase 1 → phase 2 transition for free.
- Making a hit **rare but visibly explosive** — a matched T cell proliferating into a visible
  clone rather than silently adding 3% — converts the clonal burst (>1000-fold, the most
  dramatic number in the literature) into the kind of visible feedback the design wants, and
  lets the knowledge % climb in satisfying jumps from genuinely rare events.
- 16 bits holds a roster of ~50+ species at ≥6-bit separation, which unblocks the species roster
  permanently.

---

## 4. What this means for the game

- **The game is 3 × 10⁴–2 × 10⁵ times too permissive on matching.** 37/256 = 14.5% vs. a
  measured 10⁻⁵–10⁻⁶ in both mouse and human, CD4 and CD8. Four to five orders of magnitude.
- **Length, not the Hamming threshold, is the binding constraint.** §5c says the threshold is
  the dial to reach for before changing barcode length. That is backwards: even exact match on
  8 bits is ~780× too permissive. Widening the tag is the only lever that reaches reality, and
  it is independently forced by the species roster (8 bits ceilings out at ~8–10 species with
  safe separation).
- **The Hamming *metric* is better justified than the doc claims.** Wooldridge's best agonist
  differed at 7 of 10 positions; Cole's structures show a conserved hotspot with tolerated
  substitutions around it. Graded mismatch is the accurate rule — the doc should stop
  apologising for it and own it. One TCR covers ~10⁶ peptides (Mason, Wooldridge) out of >10¹⁵
  pMHCs, and coverage would otherwise have holes pathogens evolve into (Sewell).
- **Biology solves search with throughput, not permissiveness — and the game already believes
  this.** A DC scans 500–5000 T cells/hour and holds >10 at once; 100 antigen-bearing DCs give
  a cognate contact every ~12 min node-wide at 1-in-10⁵. This is the same "the search problem
  is the game" thesis as §5's capstone. Raising node throughput is the realism-positive lever.
- **The "second cytokine" has no direct referent.** Miller showed T cells meet DCs **by chance**
  — equal approach and departure velocities, no accumulation around DCs. What is real is
  **positional**: DCs park at high endothelial venues and scan everything entering (Bajénoff).
  The field is defensible as a stand-in for DC positioning; it shouldn't be described as
  chemotaxis toward DCs.
- **`DcPresentationsPerCargo = 4` models the wrong thing.** Commitment is a **cumulative ≥6 h
  integral** delivered across *several* DCs, with signals integrated between contacts (Celli
  2005, 2007). Cargo as "4 discrete presentations" is fine as a resource; "a T cell needs N
  cumulative seconds of contact, from any DCs" would be both truer and a better mechanic.
- **Make the pairing freeze asymmetric.** A mismatch should break fast (cessation of TCR–MHC
  signalling causes dissociation, Celli 2007); a match should hold long. Free accuracy, free
  drama, and it reproduces Mempel's phase 1 → phase 2 transition.
- **Add clonal expansion — it is the best unused number in the literature.** 100–200 naive cells
  → ~10⁷ effectors in a week, >14 divisions, >1000-fold rise in precursor frequency (Blattman).
  A rare match that visibly *multiplies* is worth more than a common match that increments a
  counter, and it is the honest way to buy back playability after making matches rare.
- **The mutation discount should be big: remove 60–85% of knowledge, floor at ~15–25%.** One
  influenza cluster transition took VE from 40–60% to **7% (CI −14 to 23)** (Tenforde). Transitions
  occur every **2–8 years**, ~3.3 on average (13 clusters, 1968–2011). And Smith's core finding —
  antigenic change is **more punctuated than genetic change** — directly vindicates §5's choice
  of a discrete step over continuous erosion. Make most mutations do nothing and rare ones
  catastrophic.
- **Imprinting is the mechanic worth stealing.** First-ever exposure gives **75% protection
  against severe disease and 80% against death** against novel same-group subtypes, for life
  (Gostic) — with the sting that later strains boost antibodies that bind them poorly (Arevalo).
  A permanent class-level knowledge floor from a species' first encounter, plus later species of
  that class partly re-boosting the imprint instead of building their own, is a real trade-off
  the game can earn.
- **Knowledge should be able to fall, not only rise.** Up to **half of all GC B cells die every
  6 hours** (Mayer); LZ cells die by default unless positively selected. The game's single
  monotonic number has no attrition and therefore no tension between rounds. Real gut IgA is
  **oligoclonal and requires repeated dosing** (Bergqvist) — deep but narrow, which is exactly
  the state that makes antigenic escape dramatic.
- **One round = 2 days of biological time.** Antigen→first division is 32–36 h, so this round's
  antigen produces next round's cells and the one-round lag the player feels *is* the priming
  delay. It puts CTLs (~10%) at ~day 8 (effectors reach gut ~day 7), neutralising antibody
  (~20%) at ~day 14 (GC peak 1.5–2 weeks), secretory IgA (~70%) at ~day 46, and the 90% capstone
  near day 60 — inside the >8-week GC window. **The campaign becomes one germinal centre
  reaction**, which is both accurate and a good story.
