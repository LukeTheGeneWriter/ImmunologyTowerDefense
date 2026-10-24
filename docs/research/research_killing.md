# Killing, phagocytosis and per-cell capacity — real numbers for Immunology Tower Defense

Research brief, 2026-10-25. Every number below was pulled from a literature record
retrieved this session via PubMed (`mcp__plugin_bio-research_pubmed__*`) or Consensus
(`mcp__plugin_bio-research_consensus__search`). Nothing is from memory. Anything I could
not pin to a paper is marked **UNVERIFIED** and should not be used as if it were sourced.

Attribution note: records in this document were retrieved from **PubMed** and from
Consensus' index of peer-reviewed literature; DOIs are given as links where the record
carried one.

Game-side conversion constant used throughout: **1 logical tick = 0.12 s**, so a
per-tick probability `p` gives an expected time-to-success of `0.12 / p` seconds
(e.g. `p = 0.03` → 4.0 s of sustained contact; `p = 0.003` → 40 s).

---

## 1. Summary table

| Quantity | Value | Organism / system | in vitro / in vivo | Citation |
|---|---|---|---|---|
| **Neutrophil maximal bactericidal capacity** | **~48 bacteria killed per neutrophil** at saturating challenge (~100 bacteria : 1 PMN); efficiency *falls* above that while ingestion keeps pace | human PMN, *S. aureus* | in vitro | Clawson & Repine 1976, *J Lab Clin Med* 88:316 (PMID 956687) |
| Neutrophil oxidative output scales with prey, not all-or-none | 6.4 → 24.4 amol DCF per cell per bacterium at 15 → 60 min; graded with 0–15 bacteria ingested | human PMN, *S. aureus* | in vitro | Szejda et al. 1984, *J Immunol* 133:3303 (PMID 6491287) |
| **IgG vs complement opsonisation, per-organism burst** | IgG-opsonised organism elicits **>2×** the oxidative response of a serum(C3b)-opsonised one | human PMN | in vitro | Szejda et al. 1984 (PMID 6491287) |
| Superoxide concentration inside the neutrophil phagosome | **~20 µM** superoxide; MPO up to **millimolar**; HOCl is the dominant killing species, and most of it reacts with phagosomal protein | human PMN | in vitro + kinetic modelling | Winterbourn & Kettle 2013, *Antioxid Redox Signal* 18:642 (PMID 22881869) |
| Neutrophil lifespan | **6–8 h in blood; up to 7 days in tissue** | human | in vivo | Delgado-Rizo et al. 2017, *Front Immunol* 8:81 |
| Time to NETosis | **suicidal NETosis 2–4 h**; **vital NETosis 5–60 min** (no loss of membrane) | human PMN | in vitro | Delgado-Rizo et al. 2017 |
| NETs as a killing route | NETs kill bacteria directly, but increased NETosis **did not improve neutrophil killing in isolation** — the gain was in *macrophage* killing (NETs raise macrophage phagocytosis and hand over PMN peptides) | mouse, *S. aureus*, *S. pneumoniae*, *P. aeruginosa* | in vitro + in vivo | Monteith et al. 2021, *Sci Adv* 7:eabj2101; Brinkmann et al. 2004, *Science* 303:1532 |
| Neutrophil collateral damage causes pathology | Elastase instilled into animal lung **produces emphysema**; α1-antitrypsin deficiency (unopposed elastase) causes human emphysema; EV-bound elastase is ~10,000× more potent than soluble and self-propagates damage | human + mouse lung | in vivo | Abboud & Vimalanathan 2008, *Int J Tuberc Lung Dis* 12:361; Baldi et al. 2026, *AJRCCM* (commentary on Margaroli et al.) |
| **Macrophage phagocytosis is saturable** | Both binding *and* internalisation are saturable, with a maximal rate and a **finite capacity**; surface-bound-but-not-internalised particles are a large fraction (ignoring them overestimates rate) | rabbit alveolar macrophage, zymosan | in vitro | Besterman et al. 1982, *Am J Physiol* 242:C339 (PMID 7081427, [DOI](https://doi.org/10.1152/ajpcell.1982.242.5.C339)) |
| **Macrophage killing capacity is finite and is the rate-limiting step** | Macrophages sustain phagocytosis but **fail to sustain killing** under large inocula; phagolysosome acidification and cathepsin D activation fall; viable bacteria accumulate, macrophage lyses, bacteria are re-internalised by the next macrophage | human/mouse macrophage, *S. aureus* (Newman, SH1000, USA300) | in vitro + mouse lung in vivo | Jubrail et al. 2015, *Cell Microbiol* 18:80 (PMID 26248337, [DOI](https://doi.org/10.1111/cmi.12485)) |
| Macrophages eat several prey types at once | 51% of macrophages ate *both* NTHI and *S. pneumoniae*; 19% ate all three prey offered; apoptotic-cell uptake alongside bacteria **suppressed** CXCL8/IL-6/TNF | human monocyte-derived macrophage | in vitro (4 h) | Belchamber et al. 2021, *Thorax* 76:A33 |
| **Time to internalise one particle — FcγR vs CR3** | **FcγR cup: 2–3 min.** **CR3 "sinking" mode: 10–25 min.** (~5× slower) | mouse peritoneal F4/80+ macrophage, opsonised RBC | in vitro, real-time 3D | Walbaum et al. 2021, *J Biol Chem* 297:101256 |
| Engulfment time depends on shape more than size | Ellipsoids (e=0.954) taken up **5× slower** than spheres of larger surface area | macrophage | in vitro | Paul et al. 2013, *Biophys J* 105:1143 |
| Phagosome maturation → acidification → proteolysis | Internalisation, microtubule transport, proteolysis **and** acidification all complete within **~6–7 min** of phagosome sealing | *Dictyostelium* (professional phagocyte model) | in vitro, single-phagosome | Clarke et al. 2006, *Eur J Cell Biol* 85:1001 |
| Phagosome acidification, mammalian | pH **4.0–4.5 within 60 min** | mouse J774 macrophage, *Brucella suis* | in vitro | Porte et al. 1999, *Infect Immun* 67:4041 |
| **Opsonisation fold-effect on uptake (the "Fc receptor upgrade" number)** | Post-vaccination serum raised macrophage uptake **2.3×** by flow, **2–9.5×** by imaging, and cut intracellular *survival* by up to **50%** | human macrophage, *S.* Typhi | in vitro, human vaccinee sera | Lindow et al. 2011, *Infect Immun* 79:3188 (PMID 21628517, [DOI](https://doi.org/10.1128/IAI.05081-11)) |
| **CTL serial killing, in vivo** | **2–16 infected cells killed per CTL per day.** Medians by model: 4.8 (MCMV-3D-ΔvRAP, LN), 4.2 (MVA-OVA, LN), **16.0 / 12.5 (skin)**; intralymphatic-transfer modelling gave 2.0–10.4 | mouse, MCMV + MVA | **in vivo**, two-photon + maths | Halle et al. 2016, *Immunity* 44:233 (PMID 26872694, [DOI](https://doi.org/10.1016/j.immuni.2016.01.010)) |
| CTL killing, other in vivo estimates | **0.7–3 infected cells per CTL per day** | human HIV-1 | in vitro + in vivo modelling | Wick et al. 2005, *J Virol* 79:13579 (PMID 16227278, [DOI](https://doi.org/10.1128/JVI.79.21.13579-13586.2005)) |
| Target half-life under CTL pressure (peak response) | **2 min** (NP396) / **14 min** (GP276) at peak; **48 min / 2.8 h** in immune mice | mouse LCMV, peptide-pulsed splenocytes | in vivo | Ganusov & De Boer 2008, *J Virol* 82:11749 (PMID 18815293, [DOI](https://doi.org/10.1128/JVI.01128-08)) |
| CTL killing rates across 5 viruses | BLV and HTLV-1 estimates sit at the **low end** of the published range; high-end estimates come from the most manipulated systems and represent *potential*, not realised, efficiency | sheep BLV, human HTLV-1, + HIV/LCMV/FV literature | in vivo | Elemans et al. 2014, *PLoS Comput Biol* 10:e1003534 (PMID 24699260, [DOI](https://doi.org/10.1371/journal.pcbi.1003534)) |
| **CTL contact duration before detaching** | Individual contacts **~8.5–9.0 min** (median) whether or not the target died; CTLs stay motile at 2–4 µm/min for 10–15 min — **"kinapses", not static synapses** | mouse, in vivo | in vivo | Halle et al. 2016 |
| **CTL killing is cumulative, not one-hit (in vivo)** | Killed targets took a median of **3.5 distinct CTL contacts** and **50 min cumulative contact time**; death 20–60 min after first contact, rarely <10 min | mouse, in vivo | in vivo | Halle et al. 2016 |
| **CTL cooperativity, measured** | p(death) = **0.01** with 0 contacts, **0.15** with 1, **0.28** with 2, and **significantly above the independent-events prediction at 3 and 5 contacts** (n = 660 targets) | mouse, in vivo | in vivo | Halle et al. 2016 |
| CTL functional heterogeneity | 40% of CTLs in contact triggered **no** lethal Ca²⁺ flux; only 10% triggered 3–4; Ca²⁺ flux began **480 s (median)** after contact and lasted 80 s | mouse, in vivo | in vivo | Halle et al. 2016 |
| CTL killing, in vitro (fast picture) | Perforin permeabilises target **within ~30 s** of recognition; pore repair starts at 20 s and finishes by 80 s; caspase-dependent target rounding **within 2 min** | human primary CTL/NK | in vitro | Lopez et al. 2013, *Blood* 121:2659 (PMID 23377437) |
| CTL synapse duration and the detach signal | Mean synapse **~8 min** for competent CTL/NK; **>40 min (5×) when killing fails**; the detachment signal comes **from the dying target** and is caspase-dependent | human + mouse CTL/NK | in vitro, hundreds of synapses | Jenkins et al. 2015, *J Exp Med* 212:307 (PMID 25732304) |
| CTL killing in vitro, per-cell spread | Clonal CTLs sustain population-level killing via **highly heterogeneous** individual performance; a substantial subset are high-rate "burst" killers, and burst activity is **delayed** and needs strong antigenic stimulation | human CTL clones, 12 h single-cell | in vitro | Vasconcelos et al. 2015, *Cell Rep* 11:1474 (PMID 26027932, [DOI](https://doi.org/10.1016/j.celrep.2015.05.002)) |
| Multiple-hitting is consistent with the data | Stochastic/spatial simulation shows multi-hit requirement fully explains the observed late-onset burst kinetics; Poisson single-hit does not | simulation of in vitro data | model | Beck et al. 2020, *PLoS Comput Biol* 16:e1007972 |
| **Peptide-MHC-I complexes needed to trigger killing** | **As few as 200 complexes per target cell** (<0.08% of surface Db) sensitise for CTL lysis | mouse EL4 thymoma, H-2Db, influenza NP peptide | in vitro | Christinck et al. 1991, *Nature* 352:67 (PMID 2062379, [DOI](https://doi.org/10.1038/352067a0)) |
| Same quantity, range across systems | Requirement varies from **several thousand down to fewer than ten** complexes per target, depending on peptide-MHC/CTL pair | human A2, mouse Kb, Ld | in vitro | Kageyama et al. 1995, *J Immunol* 154:567 |
| TCR-level minimum | **3–5** TCR·MHC·peptide ternary complexes per T cell (physico-chemical model fit) | model of in vitro IFN-γ release | model | Brower et al. 1994, *Mol Immunol* 31:1285 |
| **MHC-I downmodulation abolishes CTL killing** | With viral MHC-I evasion intact, per-capita killing rate = **0.0**; CTL contact frequency fell from 38% → 8% → 1% | mouse MCMV | in vivo | Halle et al. 2016 |
| **NK serial killing capacity** | **~4 targets per IL-2-activated NK cell on average**; serial killing consumes perforin and granzyme B; **most NK cells survive** and IL-2 restores "exhausted" cells | human NK | in vitro | Bhat & Watzl 2007, *PLoS One* 2:e326 (PMID 17389917) |
| NK serial killing, upper bound | **up to 10 targets in a 6 h assay**; **the first kill is slower than subsequent kills**, especially on poor targets; sequential kills are usually spatially adjacent | human NK, HeLa/MCF-7 | in vitro | Choi & Mitchison 2013, *PNAS* 110:6488 (PMID 23576740, [DOI](https://doi.org/10.1073/pnas.1221312110)) |
| **NK mechanism switch = "exhausts sooner"** | NK cells use **fast granzyme-B death on first kills, then switch to slow death-receptor (CD95L) killing** on later targets; intracellular GrzB/perforin drop while surface CD95L rises | human NK | in vitro | Prager et al. 2019, *J Exp Med* 216:2113 (PMID 31270246) |
| NK detachment is gated on success | Detachment is **directly coupled to a successful kill**; failed killing → defective detachment, prolonged contact, sustained Ca²⁺, cytokine hypersecretion | human NK | in vitro | Anft et al. 2019, *Cell Mol Immunol* 17:347 |
| **Missing-self threshold** | Cells expressing MHC-I **well below half** the host level are still **tolerated**; hemizygous (one-allele) expression downmodulates Ly49 as well as homozygous | mouse | **in vivo** assay | Brodin et al. 2010, *PLoS One* 5:e13174 (PMID 20957233) |
| Missing-self is context-, not dose-, gated | **Global** MHC-I down-regulation → NK **tolerance**; down-regulation on a **small fraction** of cells → missing-self **reactivity**; inflammation promotes reactivity | mouse, floxed B2m | in vivo | Bern et al. 2018, *J Exp Med* 216:99 |
| ADCC vs direct killing | Rituximab **raises killing frequency** of both resting and IL-2-activated NK cells; CD16 variants / afucosylated IgG that speed **disengagement** raise serial-killing throughput; CD16 shedding is required for detachment | human NK | in vitro | Bhat & Watzl 2007; Maskalenko et al. 2024, *Cancer Immunol Res*; Karampatzakis et al. 2021, *Front Immunol* 12:641521 |
| **MAC pores needed to lyse a Gram-negative** | Bacteria **initially survive hundreds of randomly distributed MACs**; lysis follows larger-scale OM disruption, swelling and stiffening, then IM permeation. **Lysis is an indirect, mechanical consequence of poration** | *E. coli*, live-cell AFM | in vitro | Benn et al. 2024, *EMBO J* 43:6152 (PMID 39402327, [DOI](https://doi.org/10.1038/s44318-024-00266-3)) |
| MAC must be assembled *in situ* | Pre-formed C5b6-derived MACs perforate artificial and mammalian membranes but are **not bactericidal**; killing needs surface-bound C5 convertase and immediate C5b67 insertion | *E. coli* / Gram-negatives | in vitro | Heesterbeek et al. 2019, *EMBO J* 38:e99852 |
| MAC assembly rate-limiting step | **Insertion of the first C9** is the kinetic bottleneck; C9 oligomerisation then completes the pore rapidly | reconstituted, rapid AFM | in vitro | Parsons et al. 2018, *Nat Commun* 9:2066 |
| Gram-positive vs Gram-negative susceptibility | Gram-positives are **inherently resistant to direct MAC lysis** (thick peptidoglycan), yet C5b-9 still deposits — at the **division septum** in *S. pyogenes*, at the **poles** in *B. subtilis*, unlike random C3b deposition. Several Gram-positives *also* secrete MAC inhibitors, so peptidoglycan alone is not the whole story | *S. pyogenes*, *B. subtilis*, review | in vitro | Berends et al. 2013, *Cell Microbiol* 15:1955; Bjånes & Nizet 2021, *Microbiol Mol Biol Rev* 85:e00177-20 |
| **C3b deposition rate on a bacterial surface** | **Massive C3b deposition within 20–30 min** in 30% serum via the classical pathway; **slower and less complete** via the alternative pathway (EGTA-serum), starting at discrete sites (cell poles) and **spreading** across the surface | *Yersinia enterocolitica* | in vitro | Acker & Brade 1980, *Zentralbl Bakteriol A* 248:210 (PMID 7223121) |
| **Antibodies per virion needed to neutralise** | Potent mAbs neutralise with **~30 of 180** accessible envelope proteins bound (~17% occupancy); weak mAbs need near-complete occupancy. Neutralisation is a **multiple-hit** process | West Nile virus | in vitro | Pierson et al. 2007, *Cell Host Microbe* 1:135 |
| Same, picornavirus | **~4 bound antibody molecules per virion** to reduce infectivity to 1/e; max binding capacity ~30 molecules per virion | poliovirus, mAb | in vitro | Icenogle et al. 1983, *Virology* 127:412 |
| Antibodies per bacterium to block adhesion | **UNVERIFIED** — I found no paper giving a per-bacterium antibody stoichiometry for adhesion blockade. Adhesion-blocking is reported qualitatively/by titre, not by molecules per cell. Do not invent this number. | — | — | — |
| **Gut bacterium doubling, in vitro** | *E. coli* K-12 lab ceiling **~20 min** (20.3 min ancestor; 15.9 min after 1,000 generations of directed selection) | *E. coli* K-12 MG1655 | in vitro | Hamrick et al. 2026, bioRxiv (preprint — flag as such) |
| **Gut/enteric bacterium doubling, in vivo** | *E. coli* in **human urine during active UTI: mean doubling 22.4 min** (peak-to-trough sequencing); murine UTI growth matched or exceeded in vitro rates | uropathogenic *E. coli* | **in vivo, human** | Forsyth et al. 2018, *mBio* 9:e00186-18 |
| ***Salmonella* in vivo division time** | **2.86 h in susceptible BALB/c spleen; 5.02 h in resistant (B10×A/J)F1** (superinfecting-phage dilution method); death rate low and higher in the *susceptible* strain | *S.* Typhimurium M526, mouse spleen | **in vivo** | Hormaeche 1980, *Immunology* 41:973 (PMID 7007218) |
| *Salmonella* growth is heterogeneous in tissue | Host tissue diversifies *Salmonella* into **non-dividing, moderately growing and well-nourished fast-growing** subsets simultaneously; antibiotic killing tracks single-cell division rate | *S.* Typhimurium, mouse | in vivo, single-cell reporter | Claudi et al. 2014, *Cell* 158:722 (PMID 25126781, [DOI](https://doi.org/10.1016/j.cell.2014.06.045)) |
| *Salmonella*: macrophage vs epithelial niche | In **macrophages**, replication is nutrient- and T3SS-gated and occurs in only a **subset** of infected cells. In **epithelial cells**, a fraction lyse the vacuole and **hyper-replicate in the cytosol**, reaching **~half the total intracellular population** in HeLa and Caco-2 by late infection | *S.* Typhimurium | in vitro, single-cell/live-cell | Knodler et al. 2014, *PLoS One* 9:e84681 (PMID 24400108, [DOI](https://doi.org/10.1371/journal.pone.0084681)); McQuate et al. 2016, *Cell Microbiol* 19:e12641 (PMID 27376507); Garcia-Rodriguez et al. 2026, *PLoS Pathog* |
| **Enteric virus eclipse period** | Rotavirus: **progeny genome first detected 2–3 h p.i. (latent period)**; host protein synthesis shut off by 4 h; **maximum infectious yield at 10–12 h** at 37 °C | bovine rotavirus, BSC-1 cells | in vitro, one-step | McCrae & Faulkner-Valle 1981, *J Virol* 39:490 |
| **Viral burst size (generic reference figures)** | Influenza A: **10¹–10⁴ per cell**, mean **709 (H3N2) / 358 (H1N1)**; **10% of infected cells make 40–60% of all virions**. HSV-1 in keratinocytes: spans **3 orders of magnitude**, most cells low, rare "super-producers" dominate | influenza A, HSV-1 | in vitro, single-cell | Zath et al. 2024, *PLoS Pathog* 20:e1011597; Villani et al. 2025, *J Virol* |
| Rotavirus / norovirus **exact** burst size | **UNVERIFIED** — no single-cell burst-size measurement found for rotavirus or norovirus. Rotavirus kinetics (above) and rotavirus R₀ ≫ 1 (González-Parra et al. 2018, *Virus Res* 244:53) are the closest sourced figures. Use influenza/HSV-1 as the stand-in and say so. | — | — | — |

---

## 2. Per-quantity notes and caveats

### 2.1 Macrophage phagocytosis

The honest headline is that **"bacteria ingested per macrophage per unit time" does not have a
single value** — it is saturable in both binding and internalisation, with a maximal rate and
a hard capacity ceiling (Besterman 1982), and the ceiling that actually matters is not the
eating but the **killing**.

Jubrail 2015 is the key paper for the game. Macrophages keep eating under a large inoculum but
**stop killing**: phagolysosome acidification fails, cathepsin D activation drops, live bacteria
accumulate intracellularly, the macrophage eventually lyses, and the released live bacteria are
taken up by the *next* macrophage. That is a cycle, not a clean "retire quietly." It is the single
most mechanically interesting macrophage finding here and it is **the biology behind `KillLimit`** —
except that in reality exceeding the limit is *worse than useless*, because the macrophage becomes
a bacterial incubator and shuttle.

Internalisation time has a real, usable number and it is receptor-dependent: **FcγR cups close in
2–3 min; CR3-mediated sinking takes 10–25 min** (Walbaum 2021). Phagosome maturation to a
proteolytic, acidified state is fast once sealed — ~6–7 min in *Dictyostelium* single-phagosome
imaging (Clarke 2006), pH 4.0–4.5 within 60 min in mammalian macrophages (Porte 1999).

Caveats: Besterman used zymosan, not bacteria; Clarke used an amoeba; Walbaum used opsonised
erythrocytes in mouse peritoneal macrophages. Nobody has published "N *Salmonella* per intestinal
macrophage per minute." Target *shape* matters more than size (Paul 2013: 5× for ellipsoids),
which is a free, defensible reason for rod-shaped enemies to be slower to clear than cocci.

### 2.2 Neutrophil killing

**~48 bacteria per neutrophil** (Clawson & Repine 1976) is the number to carry. It was measured
by deliberately stressing human PMN across ratios of 1.25:1 to 400:1 and finding the peak of
total killing at ~100:1. Above that, ingestion kept pace but **bactericidal efficiency fell** —
the same saturation shape as the macrophage.

The oxidative burst is **graded, not all-or-none** (Szejda 1984): output rose monotonically with
0–15 ingested bacteria and with time (6.4 → 24.4 amol DCF/cell/bacterium from 15 to 60 min). Inside
the phagosome, superoxide reaches **~20 µM** against **millimolar MPO**, and the lethal species is
HOCl — of which much is consumed by phagosomal protein rather than the bacterium
(Winterbourn & Kettle 2013). MPO-deficient people rarely get serious infections, which Winterbourn
reads as HOCl being a fast frontline that non-oxidative mechanisms can substitute for at low
burden but not at high.

Extracellular vs phagocytic split: I could not find a clean percentage, and the most careful paper
argues the framing is wrong. Monteith 2021 found that **boosting suicidal NETosis did not improve
the neutrophil's own killing at all** — the benefit was handing bacteria and neutrophil
antimicrobial peptides to macrophages. Treat NETs as a **hand-off / area-denial** mechanic, not as
neutrophil DPS. Timing: suicidal NETosis 2–4 h, vital NETosis 5–60 min (Delgado-Rizo 2017).

**Collateral damage is well evidenced as the cause of pathology, not a side-effect.** Elastase
instilled into animal lung produces emphysema; α1-antitrypsin deficiency (i.e. unopposed elastase)
causes human emphysema (Abboud 2008). More recently, neutrophil-derived extracellular vesicles
carrying surface-bound elastase are **resistant to α1-antitrypsin, ~10,000× more potent than
soluble enzyme, and induce emphysema within days of transfer** — and they self-propagate by
liberating the chemoattractant PGP, recruiting more neutrophils, which release more vesicles
(Baldi 2026, on Margaroli et al.). That is a positive feedback loop and it is exactly the shape
of the game's fibrosis mechanic.

Neutrophil lifespan: **6–8 h in blood, up to 7 days in tissue** (Delgado-Rizo 2017). Note this
contradicts the common "neutrophils live a few hours" shorthand *for tissue* neutrophils, and it
weakens the case for making the game's neutrophil extremely short-lived on arrival.

### 2.3 CTL serial killing

This is where in vitro and in vivo diverge most, and the gap is not subtle.

**In vivo: 2–16 kills per CTL per day** (Halle 2016), **0.7–3 per day** for HIV-specific CTL
(Wick 2005), with Elemans 2014 placing BLV/HTLV-1 at the low end of the published spread and
arguing that high-end figures come from the most manipulated systems and reflect *potential*
rather than realised efficiency. Halle's figure is robust across two virus families, two organs
(lymph node ~4–5/day, skin 12–16/day), transgenic and polyclonal CTLs, and two independent methods
(direct counting and mathematical modelling).

**In vitro the picture is minutes, not hours.** Perforin permeabilises the target within ~30 s of
recognition; the pore is repaired within 80 s; that window is still enough to deliver lethal
granzyme; the target rounds within 2 min (Lopez 2013). Vasconcelos 2015 shows clonal CTLs sustain
killing for 12 h with wildly heterogeneous per-cell performance and delayed "burst" killers.

**Attachment and detachment.** Individual in vivo contacts last **~8.5–9 min**, and crucially the
CTL stays *motile* — 2–4 µm/min for 10–15 min, a "kinapse", not a static synapse (Halle 2016).
In vitro, competent CTL/NK synapses average **~8 min**, extending to **>40 min when killing fails**,
and **the detach signal comes from the dying target and is caspase-dependent** (Jenkins 2015).
So: successful kill → prompt release; failed kill → the killer gets stuck and hypersecretes
cytokine. That is a mechanic, not a footnote.

**One-hit or cumulative?** In vivo, clearly **cumulative and cooperative**. Killed targets took a
median of **3.5 CTL contacts** and **50 min cumulative contact**; death came 20–60 min after first
contact and rarely inside 10 min. Measured per-target death probability: p(0)=0.01, p(1)=0.15,
p(2)=0.28, and **significantly above the independent-events prediction at 3 and 5 contacts** —
real cooperativity, n=660 targets (Halle 2016). 40% of contacting CTLs triggered no lethal
calcium flux at all. Beck 2020 showed by simulation that multi-hitting explains the in vitro
burst kinetics that Poisson single-hit cannot.

**Peptide-MHC-I threshold — the famous small number.** **200 complexes per target cell**, i.e.
<0.08% of surface Db molecules (Christinck 1991, *Nature*). But be precise about the spread:
Kageyama 1995 measured the requirement across 16 peptides × 3 MHC-I proteins × several CTL clones
and found it ranges from **several thousand down to fewer than ten**, depending on the pair. The
TCR-side minimum from physico-chemical modelling is **3–5 ternary complexes** (Brower 1994). So
"200" is the canonical headline figure, "<10 to a few thousand, depending on the pair" is the
honest range, and "3–5 TCRs" is the floor.

### 2.4 NK cells, and how they differ from a CTL

**Serial capacity is lower than a CTL's potential but comparable in practice: ~4 targets per
IL-2-activated NK cell on average** (Bhat & Watzl 2007), **up to 10 in 6 h** at the top end
(Choi 2013).

**The "kills faster but exhausts sooner" claim is, with a correction, confirmed.**
- *Faster*: Prager 2019 showed NK cells use **fast granzyme-B-mediated death on their first kills**
  and then **switch to slow death-receptor (CD95L) killing** on subsequent targets, as intracellular
  GrzB and perforin fall and surface CD95L rises. So the speed advantage is real but it is
  *front-loaded*, not intrinsic.
- *Correction*: Choi 2013 found the **first kill is the slowest**, with subsequent kills faster
  (they propose the NK integrates signals from the previous and current target). These are not in
  conflict — Choi measures time-to-granzyme-activity per engagement in a low-E:T assay; Prager
  measures which death pathway is used. Report both; the clean statement is *"the first engagement
  is slow to commit, the next few are fast and granzyme-driven, and later ones get slow again as
  granules deplete."*
- *Exhausts*: serial killing consumes perforin and granzyme B, but **most NK cells survive**, and
  **IL-2 replenishes the granules and restores the exhausted cell's cytotoxicity** (Bhat & Watzl 2007).
  So NK exhaustion is **reversible and resupply-gated**, not terminal. That is a different
  depletion model from the neutrophil's one-way degranulation.

**Missing self, quantitatively — and this is the surprise.** NK cells **tolerate** cells expressing
MHC-I **well below half** the host level; hemizygous (single-allele) expression is tolerated and
still downmodulates Ly49 receptors as effectively as homozygous (Brodin 2010, in vivo). Bern 2018
adds that *how* MHC-I is lost matters more than *how much*: **global** down-regulation induces NK
**tolerance**, whereas down-regulation on a **small fraction** of cells triggers missing-self
**reactivity**, and inflammation pushes toward reactivity. So missing-self is not a dose threshold
you can tune — it is close to all-or-nothing, and it is **contextual**.

**ADCC.** Rituximab raised the killing frequency of both resting and IL-2-activated NK cells
(Bhat & Watzl 2007). The mechanism by which ADCC boosts *throughput* is instructive: CD16 variants
and afucosylated IgG that make the NK **kill and disengage faster** produce more serial killing,
and blocking CD16 shedding reduces detachment and serial killing (Maskalenko 2024;
Karampatzakis 2021). ADCC is therefore better modelled as **faster engagement cycling**
(attack speed / retarget speed) than as raw damage.

### 2.5 Complement

**"How many MAC pores to lyse a target" has a genuinely surprising answer: more than you'd think,
and the pore count is not the trigger.** Live-cell AFM on *E. coli* showed bacteria **surviving
hundreds of randomly distributed MACs**; death only followed larger-scale outer-membrane
disruption — propagating defects and fractures — plus overall swelling and stiffening, which then
let turgor pressure permeate the inner membrane. **Lysis is an indirect, mechanical consequence of
poration** (Benn 2024).

Two further constraints: the MAC must be assembled **in situ** by a surface-bound C5 convertase —
pre-formed C5b6-derived MACs lyse artificial and mammalian membranes but are **not bactericidal**
(Heesterbeek 2019); and the **rate-limiting step is insertion of the first C9**, after which C9
oligomerisation completes the pore quickly (Parsons 2018). That first-C9 bottleneck is also the
window in which CD59 protects host cells — i.e. there is a built-in, real "friendly fire guard."

**Gram-positive vs Gram-negative.** The textbook reason is correct but incomplete: Gram-positives
resist direct MAC lysis because of the thick peptidoglycan layer (Bjånes & Nizet 2021). But C5b-9
**does** deposit on them, in specific locations — the **division septum** in *S. pyogenes*, the
**poles** in *B. subtilis* — unlike C3b, which deposits randomly (Berends 2013). And many
Gram-positives additionally secrete MAC inhibitors (e.g. streptococcal inhibitor of complement
blocking the C5b-7 insertion site), which would be pointless if peptidoglycan alone sufficed.

**C3b deposition rate.** Massive C3b deposition on *Y. enterocolitica* **within 20–30 min** in 30%
serum via the classical pathway; the alternative pathway is slower and less complete, **starts at
discrete sites (cell poles) and spreads** outward across the surface (Acker & Brade 1980). The
spreading-from-a-seed geometry is a gift for a visual effect.

Caveat: Acker & Brade is 1980 and the numbers are serum-concentration-specific. I did not find a
modern molecules-of-C3b-per-µm²-per-second figure; treat the 20–30 min figure as order-of-magnitude.

### 2.6 Antibody neutralisation and opsonisation

**Per virion: ~30 of 180 accessible envelope proteins** for potently neutralising mAbs against
West Nile virus (~17% occupancy), rising to near-complete occupancy for weakly neutralising mAbs;
the authors frame this explicitly as a **"multiple hits"** model where the outcome depends on
affinity × epitope accessibility (Pierson 2007). For poliovirus, **~4 bound antibodies per virion**
reduce infectivity to 1/e, with a maximum binding capacity of ~30 (Icenogle 1983). So the range
across virus families is roughly **single digits to tens of antibodies per virion** — small
numbers, same spirit as the 200-pMHC-I figure.

**Per bacterium, to block adhesion: UNVERIFIED.** Searched and not found. Do not fabricate it.

**Opsonisation fold-effect — one careful number.** Human post-typhoid-vaccine sera raised macrophage
uptake of wild-type *S.* Typhi **2.3×** by flow cytometry and **2–9.5×** by imaging, and separately
cut *intracellular survival* by up to **50%** (Lindow 2011). Purified immunoglobulin recapitulated
the uptake effect, so it is antibody-driven. Add Szejda 1984's **>2×** greater oxidative burst per
IgG-opsonised organism versus a C3b-opsonised one, and Walbaum 2021's **~5× faster internalisation**
for the FcγR route versus the CR3 route. Three independent, modest effects on three different axes.

### 2.7 Pathogen side

**Gut bacteria, in vitro vs in vivo.** *E. coli* K-12's laboratory doubling floor is **~20 min**
(20.3 min for the ancestor in the only quantitative record I found; 15.9 min after directed
selection — preprint, flag it). In vivo, uropathogenic *E. coli* in **human urine during active
UTI doubles every 22.4 min on average** (Forsyth 2018) — i.e. for a pathogen in its preferred
niche, in vivo growth matches or exceeds in vitro.

***Salmonella* is the opposite case and the more relevant one.** In vivo division time in mouse
spleen is **2.86 h (susceptible BALB/c)** vs **5.02 h (resistant F1)** — roughly a **9–15× slowdown**
relative to broth — and the host-genetic resistance locus acts by controlling **division rate**,
probably inside macrophages, not by killing faster; measured death rates were low and actually
higher in the susceptible strain (Hormaeche 1980). Claudi 2014 adds that in tissue the population is
simultaneously non-dividing, moderately growing and fast-growing, with antibiotic susceptibility
tracking per-cell division rate.

**Macrophage vs epithelial niche.** In macrophages, replication is restricted, T3SS- and
nutrient-gated, and happens in only a **subset** of infected cells (Garcia-Rodriguez 2026;
McQuate 2016). In epithelial cells, a sizeable fraction of bacteria **lyse the vacuole and
hyper-replicate in the cytosol**, reaching **~half of the total intracellular population** in HeLa
and Caco-2 by late infection; SPI-1 is required for escape, SPI-2 is not (Knodler 2014;
McQuate 2016). **So the epithelial cell is the fast, explosive niche and the macrophage is the slow,
persistent one** — the opposite of the intuition that the immune cell is the dangerous place to hide.
For a small-intestine board, that is a directly usable asymmetry.

**Enteric virus eclipse and burst.** Rotavirus one-step growth: progeny genome first detected at
**2–3 h p.i.** (the latent/eclipse period), host protein synthesis shut off by 4 h, **maximum
infectious yield at 10–12 h** (McCrae 1981). Burst size per cell for rotavirus or norovirus:
**UNVERIFIED**. The best-characterised single-cell figures are influenza A — **10¹–10⁴ per cell,
mean 709 (H3N2) / 358 (H1N1), with 10% of cells making 40–60% of all virions** (Zath 2024) — and
HSV-1 in human keratinocytes, spanning **three orders of magnitude** with rare "super-producers"
dominating output (Villani 2025). The transferable lesson is not the mean, it is the
**extreme right-skew**: most infected cells are nearly harmless, a few are catastrophic.

---

## 3. What this means for the game

1. **The game's innate stress-sense roll is already ~800× faster than a real CTL's, so the
   innate-vs-cytotoxic gap is currently inverted, not just small.** Macrophage
   `StressSenseChancePerTick = 0.03` means a mean **4.0 s** of contact to clear an infected cell
   (6.0 s for the neutrophil at 0.02). The real in vivo CTL kills with **p = 0.15 per ~9-minute
   contact**, which is a per-tick equivalent of about **3.6 × 10⁻⁵**, and needs a median of **3.5
   contacts / 50 minutes cumulative** (Halle 2016). An innate cell with **no** MHC-I-restricted
   recognition should not be beating that by nearly three orders of magnitude. Fix the innate side
   down before raising the cytotoxic side up.

2. **Recommended gap: ~10–15×, achieved by lowering innate, not raising cytotoxic — plus one
   categorical cliff.** Concretely: innate **0.03 → 0.003** (macrophage) and **0.02 → 0.002**
   (neutrophil), i.e. a mean **40 s / 60 s** of sustained contact — a real, felt cost for a cell
   that also has a `KillLimit` and somewhere else to be. Cytotoxic cells (γδ T / CTL / NK) at
   **0.03–0.05**, i.e. **2.4–4.0 s**, reliable within a single pass. That reads at the table as
   *"innate can get there eventually if nothing else needs doing; stress sensors just do it,"*
   which is the design intent in §4b, and it makes the Sprint 6 "my macrophages can't touch this"
   moment actually land. **Do not go beyond ~20×** — at a 100× gap the innate roll is cosmetic and
   players will stop reading it as a mechanic at all.

3. **A pure number gap undersells the real biology; the gap's true shape is a cliff, and the cliff
   is the better mechanic.** Viral MHC-I downmodulation drove the measured CTL per-capita kill rate
   to **exactly 0.0**, and dropped CTL contact frequency from 38% to 1% (Halle 2016). Meanwhile NK
   cells **tolerate** cells with MHC-I well below half of normal (Brodin 2010) and respond to *who
   lost it* rather than *how much was lost* (Bern 2018). So: give one pathogen species an MHC-I
   evasion trait that **zeroes** CTL recognition and is answered **only** by NK missing-self.
   That is a free rock-paper-scissors triangle with real citations behind every edge, and it is
   worth more than any tuning of a single float.

4. **Model the cytotoxic kill as cumulative hits with cooperativity, not one roll — the real
   numbers are already game-shaped.** Halle measured p(0)=0.01, p(1)=0.15, p(2)=0.28, with 3 and 5
   contacts **beating the independent-events prediction**. Implement: a per-contact-episode hit
   that accrues on the infected cell, a visible "stress" meter, and an explicit bonus when ≥3
   cytotoxic cells have hit the same target. 40% of real contacting CTLs deliver nothing, which
   justifies a miss chance without feeling arbitrary. This is the single most legible, most
   visible-feedback-friendly finding in the whole brief, and the Director's taste is for visible
   feedback.

5. **`KillLimit` 20 (macrophage) vs 5 (neutrophil) is backwards as a measure of killing power, and
   right as a measure of lifespan.** The measured **maximal bactericidal capacity of a human
   neutrophil is ~48 bacteria** (Clawson & Repine 1976), and the macrophage's killing — not its
   eating — is the step that saturates and fails first (Jubrail 2015). Per burst, the neutrophil is
   the better killer. Either (a) keep 20/5 and relabel it in the docs and UI as **lifetime capacity**,
   with the neutrophil's advantage expressed through speed, burst and degranulation (which the game
   already does), or (b) go to roughly **neutrophil 12 / macrophage 20** and make the neutrophil
   genuinely shorter-lived. (a) is the smaller change and is defensible; (b) is more accurate. Do
   not leave the current numbers with the current framing, which quietly teaches the wrong thing.

6. **Macrophage overload should make things *worse*, not merely stop — this is free, dramatic, and
   real.** An overwhelmed macrophage keeps phagocytosing, stops killing, accumulates **live**
   bacteria, **lyses**, and releases them to be taken up by the next macrophage (Jubrail 2015).
   That is a better retirement than "clean removal": a macrophage that hits `KillLimit` while
   enemies are still in contact should burst and release a fraction of its kills as live pathogens.
   It gives the macrophage a failure state, punishes overcommitting one tower, and is the exact
   mechanism in the literature.

7. **The "Fc receptor" upgrade is honestly worth 2–10×, not 100× — so build it as three stacked
   small effects on three different axes.** Antibody opsonisation raised *S.* Typhi uptake
   **2.3× (flow) / 2–9.5× (imaging)** and cut intracellular survival by up to **50%**
   (Lindow 2011); IgG-opsonised organisms elicit **>2×** the oxidative burst of C3b-opsonised ones
   (Szejda 1984); the FcγR route internalises in **2–3 min** versus **10–25 min** for the
   complement route, ~**5×** faster (Walbaum 2021). Price it as: **~3× engagement rate (attack
   speed), ~2× damage per hit, and +50% effective `KillLimit`**. Three real 2–5× effects stacked
   *feel* like a big upgrade and every component has a paper. A single "×10 damage" would be a lie
   with no paper behind it.

8. **Where the real ratio would make a boring or broken game — and what to do instead.**
   - **CTL at 2–16 kills/day is one kill every 1.5–12 hours.** A round is tens of seconds, so at
     true scale a CTL kills **zero** things per round and the entire adaptive half of the game is
     an expensive no-op. *Instead:* keep the **ordering** (cytotoxic ≫ innate on infected cells),
     the **shape** (multiple contacts, cooperativity, heterogeneous performance, ~9 s of contact
     per engagement scaled from 9 min), and the **cliff** (MHC-I evasion → 0). Throw the absolute
     rate away and say so in `GAME_DESIGN.md` so nobody "corrects" it later.
   - **Neutrophil at 48 kills would solo a wave** and make the degranulation payoff — the whole
     point of the unit — almost never fire. Keep it at 5–12.
   - **The 200-pMHC-I threshold (0.08% of surface MHC-I) is so sensitive there is no partial
     detection to model.** Do not build a graded "stealth" stat for adaptive recognition. Build a
     **binary**: MHC-I present → recognised; MHC-I downmodulated → invisible to CTL. Same for NK
     missing-self: **no "reduce MHC-I by 30%" upgrade**, because in reality that does nothing
     (Brodin 2010).
   - **"One MAC pore = one kill" is wrong and would make complement a trivially strong tower.**
     Bacteria survive **hundreds** of MACs; lysis is an indirect mechanical collapse under turgor
     (Benn 2024). *Instead:* complement is a **stacking debuff with a threshold** — C3b deposition
     is fast opsonisation (and the real thing **spreads from the poles**, which is a great VFX),
     MAC stacks toward a lysis threshold, and **Gram-positive enemies are immune to the lysis
     component but fully susceptible to the opsonisation component**. That makes one tower
     meaningfully good against one enemy class without a damage-type table.
   - **Bacterial doubling at in vitro rates would lose the board in seconds.** *Salmonella* in vivo
     divides every **2.86 h** in a susceptible host versus ~20 min in broth — a ~9× slowdown — and
     the host-resistance gene acts on **division rate**, not on killing (Hormaeche 1980). Use the
     in vivo figure as the reference, and note that this gives an impeccably real mechanism for a
     "host resistance" upgrade that **slows enemy replication** rather than dealing damage.

9. **Two asymmetries worth stealing wholesale, both free and both counter-intuitive.**
   - **The epithelial cell is the dangerous hiding place, not the macrophage.** *Salmonella*
     hyper-replicates in the epithelial **cytosol** after lysing its vacuole, reaching ~half the
     intracellular population; inside macrophages replication is restricted and happens in only a
     subset of cells (Knodler 2014; McQuate 2016; Garcia-Rodriguez 2026). An intracellular
     bacterium in an epithelial slot should tick toward a brood **much** faster than one that a
     macrophage has eaten. This inverts the player's expectation, and it is correct.
   - **Burst size is extremely right-skewed.** 10% of influenza-infected cells produce 40–60% of all
     virions (Zath 2024); HSV-1 output spans three orders of magnitude with rare super-producers
     (Villani 2025). *Instead of* a fixed brood size, roll a heavy-tailed one: most infected cells
     release a trickle, a rare one releases a swarm. That single change turns "clear infections
     promptly" from a background chore into a risk-management decision the player can feel, and it
     is better biology than the mean.

10. **Successful killing is what makes a killer let go — so make a failed kill *stick* the unit.**
    Competent CTL/NK synapses average ~8 min and stretch to **>40 min (5×) when killing fails**,
    and the detach signal is issued **by the dying target**, caspase-dependently (Jenkins 2015;
    Anft 2019 for NK). Stuck killers also hypersecrete inflammatory cytokine. In-game: a cytotoxic
    unit that fails its roll should be **held in contact** (and should feed DAMP/fibrosis) rather
    than freely retargeting. That makes evasive pathogens punishing in a way that is mechanically
    distinct from "more HP," and it is a measured 5× effect, not an invention.

11. **NK depletion should be resupply-gated, not terminal — a third depletion model the game does
    not yet have.** Serial killing drains NK perforin and granzyme B, but **most NK cells survive
    and IL-2 restores their cytotoxicity** (Bhat & Watzl 2007), and the cell meanwhile **switches
    from fast granzyme killing to slow death-receptor killing** (Prager 2019). So NK sits between
    the neutrophil (one-way degranulation) and the macrophage (quiet retirement): it **downgrades**
    to a slower attack when its granules run out, and a cytokine purchase **refills** it. That is a
    genuinely new tower behaviour, it is cleanly sourced, and it gives the eventual cytokine
    economy something concrete to buy.

12. **Treat NETs as hand-off and area denial, not neutrophil DPS.** Increasing suicidal NETosis did
    **not** improve the neutrophil's own killing; the whole benefit was raising **macrophage**
    phagocytosis and transferring neutrophil antimicrobial peptides to them (Monteith 2021). A NET
    should slow/hold pathogens in a zone and **buff co-located macrophages**, which makes
    macrophage + neutrophil a real combo rather than two independently-scaling DPS sources — and
    it is the literature's own conclusion, not a game-design liberty.

---

## 4. Explicitly unresolved (do not fill these in from memory)

- **Antibody molecules per bacterium required to block adhesion** — no source found. Per-*virion*
  stoichiometry is well established (~4 for poliovirus, ~30/180 for WNV); the bacterial-adhesion
  equivalent appears not to be reported as molecules per cell.
- **Rotavirus / norovirus single-cell burst size** — not found. Rotavirus eclipse (2–3 h) and
  one-step cycle (10–12 h) *are* sourced (McCrae 1981); use influenza/HSV-1 for the burst
  distribution and label the substitution.
- **Bacteria ingested per macrophage per minute** — no clean rate exists; saturation and finite
  capacity are documented (Besterman 1982; Jubrail 2015) but not a per-minute figure for bacteria.
- **Exact number of MAC pores required for lysis** — Benn 2024 shows *hundreds are survivable* and
  that pore count is not the trigger; a threshold number is therefore not the right question.
- **Fraction of neutrophil antimicrobial activity that is extracellular vs phagocytic** — no
  percentage found; Monteith 2021 argues the split is the wrong framing.
- **Modern quantitative C3b deposition rate** (molecules·µm⁻²·s⁻¹) — only the 1980
  20–30 min / serum-concentration-dependent figure was found.
