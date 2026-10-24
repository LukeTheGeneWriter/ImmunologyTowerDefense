# Immune cell motility, search, and contact — measured values and game conversions

Research for Immunology Tower Defense. Every number below was looked up against the
literature this session using the PubMed and Consensus MCP tools. **According to PubMed**
and **Consensus** (search engine over Semantic Scholar / OpenAlex / PubMed); DOI links are
given for PubMed-sourced articles as required by that tool. Anything I could not resolve to
a primary measurement is marked **UNVERIFIED**.

**Conversion constants used throughout**
- 1 tick = 0.12 s → 500 ticks/min
- 1 fine tile = 2.9 µm; 1 coarse slot = 7 fine tiles ≈ 20 µm
- **1 fine tile/tick = 24.2 µm/s = 1450 µm/min**
- So a game speed of 1 is already ~1450× a real tissue macrophage and ~230× a real T cell.
  Absolute scale is broken by design; only the *ratios* in §2 are meaningful.

---

## 1. Summary table — measured speeds

| Cell type | Speed (µm/min) | Tissue / compartment | Species | Method | Citation |
|---|---|---|---|---|---|
| Naive CD4 T | ~11 mean; peaks >25; cycles low/high every ~2 min | Inguinal LN, diffuse cortex | Mouse | Intravital 2-photon | Miller 2003 PNAS, [10.1073/pnas.0636560100](https://doi.org/10.1073/pnas.0636560100) (via Consensus) |
| Naive T | 18 mean (3D-corrected) | LN (uninfected) | Mouse | 2-photon + modelling | Textor 2011 PNAS (Consensus) |
| Naive T | **6.2 median** cell-based; 3.7 displacement; meandering ratio 0.70 | LN | Mouse | 2-photon, 90 s timestep | Torres 2023 eLife, [10.7554/eLife.84916](https://doi.org/10.7554/eLife.84916) |
| T cell (high-velocity subset) | >10, requires LFA-1/ICAM-1 | LN paracortex | Mouse | 2-photon LN slices | Katakai 2013 J Immunol (Consensus) |
| **Effector CD8 T** | **6.5 median** cell-based; 3.3 displacement; meandering 0.63 | **Small-intestinal villi** | Mouse | 2-photon | Torres 2023 eLife, [10.7554/eLife.84916](https://doi.org/10.7554/eLife.84916) |
| Effector CD8 T | 5.2 (influenza) / 4.3 (LPS); meandering 0.22 / 0.37 | Inflamed lung | Mouse | 2-photon | Torres 2023 eLife, same DOI |
| PD1+ T cell | 8.4 and 7.8 (Hodgkin, mantle cell); 3.5 (grey-zone) | Human LN tissue slices, lymphoma | Human | 4D live tissue-slice imaging | Hartmann 2021 Cancers (Consensus) |
| B cell | Motility coefficient **5–6× lower than T cells** in the same node; slower with higher turning angles | LN follicle | Mouse (and human slices) | 2-photon; 4D slices | Miller 2002 Science (Consensus); Hansmann 2024 Hum Pathol (Consensus) |
| NK cell (unmanipulated) | **6–7** | Peripheral LN | Mouse | 2-photon | Garrod 2007 PNAS (Consensus) |
| NK cell (CD49b cross-linked) | 2–3 (artefact of isolation) | Peripheral LN | Mouse | 2-photon | Garrod 2007 PNAS (Consensus) |
| NK cell | "slowly motile", stable long contacts with DC | LN paracortex/medulla | Mouse | Intravital | Bajénoff 2006 JEM (Consensus) |
| Neutrophil (interstitial crawling) | **7.3 median** | Footpad connective tissue, bacterial infection | Mouse | Video-rate 2-photon | Zinselmeyer 2008 Inflamm Res (Consensus) |
| Neutrophil (in capillaries) | 14–45 | Small capillaries, infected footpad | Mouse | 2-photon | Zinselmeyer 2008 (Consensus) |
| Neutrophil (rolling) | 5–60 | Larger venules | Mouse | 2-photon | Zinselmeyer 2008 (Consensus) |
| Neutrophil vs macrophage | Neutrophils "move an order of magnitude faster" than macrophages | 2D in vitro comparison | Mouse | Time-lapse chemotaxis assay | van den Bos 2020 JoVE (Consensus) |
| Macrophage | **~1 µm/min** | 2D surface, resident peritoneal Mφ | Mouse | Time-lapse microscopy | van den Bos 2020 JoVE (Consensus) |
| Macrophage | "significantly more slowly" than neutrophils to the same stimulus; elongated, mesenchymal-like, higher directional persistence, lower speed | Interstitial tissue | Zebrafish | Intravital, dual transgenic | Gray 2011 Thromb Haemost; Barros-Becker 2017 J Cell Sci (Consensus) |
| Dendritic cell (resident, no antigen) | **2–3** ("meandered slowly"), while extending long agile dendrites | LN T-cell zone | Mouse | 2-photon | Miller 2004 PNAS (Consensus) |
| Dermal DC (steady state) | "highly motile, continuously crawling"; Gαi-dependent. No µm/min given — **UNVERIFIED** | Dermis | Mouse | Intravital 2-photon | Ng 2008 PLoS Pathog (Consensus) |
| Langerhans cell (steady state) | **Sessile**; inflammation induces motility and LN emigration | Epidermis | Mouse | Vital imaging, langerin-EGFP | Kissenpfennig 2005 Immunity (Consensus) |
| Patrolling monocyte | **36** intraluminal in carotid — **3× faster than in microvessels** (→ ~12 in microvessels) | Large artery lumen | Mouse | 2-photon intravital | Quintar 2017 Circ Res (Consensus) |
| Monocyte (luminal crawling distance) | crawls 147 µm (unstimulated) vs 62 µm for neutrophils; 67 µm after TNF | Cremaster venules | Mouse | Intravital confocal | Sumagin 2010 J Immunol (Consensus) |
| γδ intraepithelial lymphocyte | Actively migrates along basement membrane and into the lateral intercellular space between adjacent enterocytes; "all within minutes". Speed in µm/min **UNVERIFIED** | Jejunal epithelium | Mouse | In vivo spinning-disk confocal | Edelblum 2012 PNAS, [10.1073/pnas.1112519109](https://doi.org/10.1073/pnas.1112519109) |
| Plasma cell | **Bi-modal** migratory behaviour; motility inversely related to antibody-secretion capacity; motility drops inside clusters | Bone marrow (intact tibia) | Mouse | 3-photon intravital | Rakhymzhan 2024 iScience (Consensus); Jing 2021 (Consensus) |

**The two compartments give different numbers for the same cell.** Torres 2023 is the
single most useful paper for this game because it measured naive T cells in LN *and*
activated CD8 T cells in **small-intestinal villi** with one pipeline: 6.2 vs 6.5 µm/min —
**statistically similar speeds** but different directionality (meandering 0.70 vs 0.63) and
the villi score as well as the LN for volume patrolled per time (9.4 vs 9.4 arbitrary
units/s). Lung is the outlier (4.3–5.2 µm/min, meandering 0.22–0.37, volume patrolled
5.1–5.3). So: **gut tissue is not a "slow" compartment relative to the lymph node.**

Also note the spread between labs for the *same* measurement (naive T in mouse LN: 6.2, 11,
18 µm/min) is a factor of ~3, driven by tracking interval, 2D vs 3D correction and outlier
handling. Any ratio finer than ~1.5× is below the noise floor of the literature.

---

## 2. Derived ratio table and recommended game speeds

Normalised to **macrophage = 1**. Macrophage anchor = ~1 µm/min (van den Bos 2020, 2D in
vitro; the only clean figure found — in vivo tissue macrophages are slower still and often
effectively sessile).

| Cell | Real speed (µm/min) | Ratio vs macrophage | Ideal integer (if ladder were unbounded) | **Recommended fine tiles/tick (1–5 ladder)** | Current |
|---|---|---|---|---|---|
| Macrophage (tissue-resident) | ~1 | **1.0** | 1 | **1** (ideally 1 step every 2–3 ticks, see §8) | 1 |
| Neutrophil | 7.3 interstitial; ~10× Mφ in vitro | **7–10** | 7–10 | **5** | 3 |
| Monocyte (interstitial) | **UNVERIFIED**; 12–36 intravascular | ~3 (interstitial, inferred) | 3 | **2** | — |
| Dendritic cell (in LN / as tissue sentinel) | 2–3 | **2–3** | 2–3 | **2** | 3 |
| Helper T (CD4) | 6.2–11 | **6–11** | 6 | **3** | 2 |
| Cytotoxic T (CD8 effector, gut villi) | 6.5 | **6.5** | 6 | **3** | — |
| NK cell | 6–7 | **6–7** | 6 | **3** | — |
| B cell (follicular) | motility coefficient 1/5–1/6 of T ⇒ speed ≈ 0.4–0.6× T | **~3** | 3 | **2** | — |
| γδ T (intraepithelial) | **UNVERIFIED** µm/min; genuinely motile, not sessile | ~T-like (assumed) | 5 | **1** — see note | — |
| Plasma cell | bi-modal, largely niche-bound | <1 | 0 | **0** (sessile) | — |

**Notes on the two deliberate violations of the real ratio:**
- **γδ T gets 1, not 5.** Its game identity is rung 4 (tissue residency) — it is already
  *there*, so its search speed is irrelevant and a high number would make it read as another
  fast hunter. The literature says it is dynamically motile (Edelblum 2012), so give it a
  small local patrol radius rather than speed 0. That is the honest compromise.
- **Plasma cell gets 0.** It is a factory, not a searcher. Marrow plasma cells *are* motile
  (Rakhymzhan 2024), but motility is **inversely** correlated with antibody secretion — so
  "stops moving = starts producing" is a mechanic the biology actively supports.

---

## 3. Search strategy: is the random walk honest?

**Yes, with one correction: it is a *correlated* random walk, not a Brownian one.**

- Naive T cells in LN follow "a default trafficking program analogous to a random walk";
  cells approach and depart DCs at equal velocities, i.e. they are **not** drawn in by
  chemotaxis (Miller 2003 PNAS, [10.1073/pnas.0636560100](https://doi.org/10.1073/pnas.0636560100)).
- The strongest statistical test: at 18 µm/min mean 3D speed, **uniform directed migration
  of more than 1.7 µm/min is ruled out at 95% confidence** — but a bias of only
  **0.5 µm/min (<3% of cell speed) would double LN transit speed** (Textor 2011 PNAS).
  Random is right; *and* tiny biases are enormously consequential. That is exactly the
  design claim behind the "Cytokine sensing +" upgrade.
- Movement is constrained by the **fibroblastic reticular cell network** (T zone) and the
  **FDC network** (follicles) — the walk is a walk *on a graph*, not in free space
  (Bajénoff 2006 Immunity). Distance moved before turning ≈ 2× the distance between network
  intersections, i.e. a T cell turns onto a new fibre at roughly 50% of junctions
  (Beauchemin 2007 J Immunol). **A lattice is a better model of this than continuous space.**
- Pure Brownian and pure Lévy both fail; T cell motion is best described as a **correlated
  random walk with a lognormal / heavy-tailed step distribution**, short-timescale
  directional persistence, and population heterogeneity (Fricke 2016 PLoS Comput Biol).
- **The Lévy claim has been contested.** Harris 2012 Nature reported generalised Lévy walks
  for CD8 T cells in *Toxoplasma*-infected brain, >1 order of magnitude more efficient at
  finding rare targets than Brownian. Ganusov 2020 and Patel 2023 re-analysed and found
  **correlated random walks, not Lévy**, in liver and brain, and attributed the original
  result to missing time frames; they also showed that the Lévy advantage **disappears once
  search is confined by real tissue structure** (liver sinusoids). Patrolling monocytes have
  been *proposed* to be Lévy-like, but that is a hypothesis paper, not a measurement
  (Moreno-Cañadas 2021 Front Immunol).
- Speed and turning are **anti-correlated**: fast cells make shallow turns, slow cells make
  big turns — seen in both neutrophils and T cells (Read et al., reviewed in Yam 2024 Front
  Immunol, [10.3389/fimmu.2024.1458035](https://doi.org/10.3389/fimmu.2024.1458035)).

**Verdict for the game:** the search ladder is biologically honest. Rung 1 (uniform random
walk) is a cartoon of a correlated random walk, which is a fair simplification for a
tutorial. The one thing the model currently *understates* is directional persistence, which
is the real signature of cell type (see §9).

Quantified search metrics worth copying:
- Meandering ratio (median): LN 0.70, **gut villi 0.63**, inflamed lung 0.22–0.37.
- Volume patrolled per unit time (arbitrary units/s): LN 9.4, villi d8 9.4, villi d5 6.5,
  lung 5.1–5.3 (Torres 2023, [10.7554/eLife.84916](https://doi.org/10.7554/eLife.84916)).
- Theory caveat: optimised simulations find that gradient sensing beats unguided search, but
  **"blind search works surprisingly well, in particular if the immune cells are fast and
  directionally persistent"** (Metzner 2019 Sci Rep). Persistence substitutes for guidance.

---

## 4. Chemotaxis, quantitatively

- **Shallowest detectable gradient: 1% difference in chemoattractant concentration across
  the cell body**, in optimal mean concentrations, for rabbit PMNs orienting to
  N-formyl peptides (Zigmond 1977 J Cell Biol, [10.1083/jcb.75.2.606](https://doi.org/10.1083/jcb.75.2.606)).
  Sensitivity falls off at high mean concentration because of receptor down-regulation
  (Zigmond 1981, [10.1083/jcb.88.3.644](https://doi.org/10.1083/jcb.88.3.644)).
  → For the lattice: a neutrophil is ~8 µm = ~3 fine tiles across, so the biologically
  licensed decision signal is a **~1% difference across ~3 fine tiles**. The current softmax
  on relative neighbour values is, if anything, *conservative* about how much signal real
  cells can extract; the generous `GradientSharpness = 4` is defensible.
- Neutrophils chemotax robustly in shallow and steep IL-8 gradients but **not in a saturated
  gradient** (Halilovic 2015 Biomed Microdevices, [10.1007/s10544-015-9963-8](https://doi.org/10.1007/s10544-015-9963-8)).
  → Gameplay implication: the field should *stop* guiding near a very strong source, which
  is the opposite of greedy ascent. A saturation knee is biologically real.
- **Gradient range in tissue: up to 90 µm.** Endogenous CCL21 forms steeply decaying
  haptotactic gradients in the perilymphatic interstitium, and DCs directionally approach
  lymphatic vessels **from a distance of up to 90 µm** (Weber 2013 Science, via Consensus).
  → 90 µm ≈ **4.5 coarse slots ≈ 31 fine tiles**. That is a concrete, defensible radius for
  "beeline" behaviour, with near-flat field beyond it.
- **Relay extends the radius.** Neutrophil swarming recruits additional cells from
  **>70 µm** away via LTB4 released by "pioneer" neutrophils, which amplifies the local
  death signal (Lämmermann 2013 Nature; Chtanova, reviewed in Yam 2024,
  [10.3389/fimmu.2024.1458035](https://doi.org/10.3389/fimmu.2024.1458035)).
  → A second emitter that *the units themselves* place is biologically canonical.
- **Chemokine raises speed, not only direction.** CCR7-deficient CD4 T cells in wild-type LN
  show **33% lower average velocity and 55% lower motility coefficient**; adding CCL21
  rescues it. CCR7 ligands are described as **chemokinetic** factors for basal motility
  (Worbs 2007 JEM, via Consensus). Similarly, CXCL10 "speeds up the average migration speed
  without changing the nature of the walk statistics" for CD8 T cells in brain
  (Harris 2012 Nature).
  → **The "Cytokine sensing +" upgrade should also give a modest speed bump.** That is
  supported by two independent in vivo studies, and it is currently missing.
- Direct measurement of "how much faster with chemotaxis than without" in a real tissue:
  **UNVERIFIED**. Modelled estimates only — Harris 2012 claims >10× for Lévy vs Brownian
  (contested, §3); Textor 2011 computes 2× faster LN transit from a 0.5 µm/min bias.
- Neutrophils use a "search and run" strategy: slow actin-driven orientation, then rapid
  actin flow (reviewed, Yam 2024, same DOI). Formyl peptides from the pathogen **override**
  the host chemokine gradient, so cells end up in the necrotic focus rather than the general
  inflamed area. → Two-tier gradients (host cytokine vs pathogen-derived) are real.

---

## 5. Extravasation

Cascade: capture → rolling → activation → firm adhesion → luminal crawling → diapedesis →
sub-endothelial crawling, plus reverse transmigration (Margraf 2021 Blood, via Consensus).

- **Rolling time is the rate-limiting parameter.** "Leukocytes integrate chemoattractant
  signals while rolling along the endothelial surface until they reach a critical level of
  activation and become firmly adherent"; the time spent rolling through an inflamed segment
  determines conversion efficiency to firm adhesion (Jung 1998 J Clin Invest, via Consensus).
  Rolling velocities 5–60 µm/min in inflamed venules (Zinselmeyer 2008).
- **Diapedesis is slow and has two bottlenecks.** Neutrophils cross the endothelial layer at
  "hotspot I", then **frequently remain between the endothelial layer and the basement
  membrane for a prolonged period (>20 min)** before crossing the basement membrane and
  pericyte sheath at "hotspot II" (Hyun 2019 Exp Mol Med, via Consensus).
  Transcellular (vs paracellular) emigration adds a further **20–30 min** delay
  (Phillipson 2008 PLoS ONE, via Consensus).
- ~80% of adherent leukocytes crawl before transmigrating; neutrophils crawl ~62 µm,
  monocytes ~147 µm unstimulated (Sumagin 2010).
- **Time from infection start to first neutrophil arrival: "within minutes"** is the
  consistent statement (Yam 2024; Deygas 2025/2026 Sci Immunol). An exact onset figure in
  minutes, from a controlled infection start, is **UNVERIFIED** — the cleanest anchor I found
  is that adoptively transferred neutrophils entered footpad connective tissue "within
  minutes" of transfer in infected tissue (Zinselmeyer 2008). Marginated/tissue-resident
  neutrophils, not newly recruited ones, are the true first responders (De Filippo 2020
  Front Cell Dev Biol, via Consensus).

**Game conversion:** a defensible entry delay is **rolling+adhesion a few seconds, diapedesis
~20–30 min, i.e. ~10,000–15,000 ticks at real scale**. At the game's ~500× time compression
that is ~2–3 s of wall clock — small but non-zero. Representing diapedesis as a brief
visible "stuck in the wall" pause (0.5–2 s) is faithful; representing it as instant is the
one step of the cascade that is *least* faithful.

---

## 6. Contact durations — how long to freeze a cell

| Event | Duration | Source |
|---|---|---|
| T cell ↔ DC, no cognate antigen | **~3 min** per contact, on dendrites at arm's length from the soma | Miller 2004 PNAS (Consensus) |
| Priming phase 1 | **~8 h** of multiple short serial encounters; motility progressively decreases | Mempel 2004 Nature (Consensus) |
| Priming phase 2 | **~12 h** of long-lasting stable conjugates; IL-2/IFN-γ secretion begins | Mempel 2004 |
| Priming phase 3 | From day 2: T cells **resume rapid migration and short contacts**, start proliferating | Mempel 2004 |
| Phase-1 length | Inversely correlated with pMHC density per DC and DC density per LN; below a threshold antigen dose, phase 2 never happens | Henrickson 2008 Nat Immunol (Consensus) |
| Extra priming phase | CXCR3-dependent **hours-long re-engagement** with DCs in subfollicular niches, with CD4 cells pausing briefly to deliver IL-2 | Jobin 2025 Science (Consensus) |
| CTL lethal hit | Target death within **~5 min** of contact; granule reorientation complete by ~10 min, granule fusion by ~4 min | Stinchcombe 2001 Immunity; Yannelli 1986 J Immunol (Consensus) |
| CAR-T in vivo | **Engage, kill and detach within 25 min**; many contacts produce no calcium signal or killing at all | Cazaux 2019 JEM (Consensus) |
| CTL on solid-tumour targets | Often fails on a 1:1 conjugate; needs **3 serial sublethal hits with <50 min decay intervals** from multiple CTLs ("additive cytotoxicity") | Weigelin 2021 Nat Commun (Consensus) |
| NK ↔ allogeneic target | Stable conjugates **>5 min**, motility drops, then target elimination | Garrod 2007 PNAS (Consensus) |
| DC engulfing a pathogen | Dermal DCs **immobilise within minutes** of parasite delivery, engulf via long pseudopods, then retract dendrites | Ng 2008 PLoS Pathog (Consensus) |
| Serial phagocytosis | Macrophages engulf IgG-opsonised 2 µm particles at **~1.5 particles/min** across a 33-cell population; a single cell took up to 50 particles in 2 h | Horonushi 2023 Micromachines (Consensus) |
| Efferocytosis | Transcriptional response begins **within minutes** of corpse encounter; successive corpses processed continuously | Tufan 2024 Nature (Consensus) |

**Game conversion (at 0.12 s/tick):**
- Phagocyte "busy" freeze: real ~30–60 s per particle → **a visible pause of 0.3–1 s** reads
  correctly at the game's compression. A 2–8 tick freeze is fine.
- Killer-cell freeze: real 5–25 min → **should be visibly longer than a phagocyte's**, order
  2–5 s. This is the single contact-duration distinction worth spending screen time on.
- Priming: 8 h + 12 h is **not a freeze, it is a game phase**. Do not try to animate it as a
  pause; it is a timer on the lymph-node tab.
- "Three hits within 50 min" is a ready-made, literature-backed mechanic for why a single
  CTL fails on a tough target and a cluster succeeds.

---

## 7. Cell and particle sizes — the footprint ladder

| Object | Real size | Source |
|---|---|---|
| Circulating leukocytes (mean) | **6–8 µm** diameter; pulmonary capillaries ~5.5 µm, so they must deform to pass | Downey 1990 J Appl Physiol (Consensus) |
| Neutrophil, monocyte | Largest of the circulating leukocytes; retained most in 5 µm filters. Monocytes stiffest, neutrophils softest, lymphocytes intermediate | Downey 1990 |
| Lymphocyte (T, B, NK) | Smallest of the leukocytes; least retained | Downey 1990 |
| Human leukocytes, undeformed | Spherical in isotonic solution with extensive membrane folding, and **significantly smaller than blood-smear measurements suggest**. Per-type diameters are in the paper's tables but not the abstract — **UNVERIFIED** at the digit level here | Schmid-Schönbein 1980 Blood (PMID 6775712, no DOI) |
| Tissue macrophage | Markedly larger and elongated/mesenchymal in tissue vs rounded neutrophils | Barros-Becker 2017 J Cell Sci (Consensus) — numeric diameter **UNVERIFIED** |
| Dendritic cell | Soma plus "long agile dendrites" that extend well beyond it; contacts occur on dendrites at arm's length from the soma | Miller 2004 PNAS — numeric diameter **UNVERIFIED** |
| Enterocyte **height** | **30.9 ± 4.6 µm** (human adults), 27.0 ± 3.4 µm (infants), duodenum | Thompson 1998 JPGN (Consensus) |
| Enterocyte **width** | Several µm (tall columnar); no clean human figure found — **UNVERIFIED**. Canine/avian morphometry confirms width is the small axis | Díaz-Regañón 2023 Vet Sci; Ferrer 1995 Poult Sci (Consensus) |
| Gut bacterium (*E. coli*) | **1–2 µm long × 0.35–0.6 µm wide**, rod | Smith 2021 (Consensus) |
| Enteric virion (norovirus) | **38 nm** VLP (T=3); **27 nm** for the original Norwalk agent in stool; capsid size is polymorphic between genogroups (small/medium/large) | Bertolotti-Ciarlet 2002 J Virol; Riepenhoff-Talty 1983; Jung 2019 PNAS (Consensus) |
| Enteric virion (rotavirus) | ~70–100 nm triple-layered particle — **UNVERIFIED**; sourced only as "larger than the 15–40 nm small round viruses" | Riepenhoff-Talty 1983 (Consensus) |

**Real ratios vs what the grid can show (fine tile = 2.9 µm):**

| | Real | Game footprint | Game size | Error |
|---|---|---|---|---|
| Enterocyte | ~20–31 µm tall, narrow | 1 coarse slot = 7×7 | 20 µm | correct |
| Macrophage | larger than a neutrophil, spread | 5×5 | 14.5 µm | plausible |
| Neutrophil | ~7–8 µm | 3×3 | 8.7 µm | **near-exact** |
| T / B / NK | smaller than neutrophil, ~6–7 µm | 3×3 | 8.7 µm | ~25% oversized — **unreadable difference, do not model it** |
| Bacterium | 1–2 µm | 1×1 | 2.9 µm | ~2–3× oversized |
| Virion | 0.027–0.038 µm | 1×1 | 2.9 µm | **~75–100× oversized** |
| Antibody | ~0.01 µm | 1×1 | 2.9 µm | ~290× oversized |

**Where the real ratio is unreadable:** the entire lymphocyte family (T, B, NK, γδ) is within
±25% of the neutrophil's diameter. On a 7×7 sub-lattice there is no footprint between 3×3
and 5×5, so **all small round cells must be 3×3 and all distinction must come from colour,
sprite and behaviour, not size.** The only size step the grid can honestly carry is
macrophage (5×5) vs everything else (3×3) vs sub-cellular objects (1×1).

---

## 8. Lymph node transit and scanning rates

- **Naive T cell dwell in one LN:** mode **10–20 h**, with a strongly right-skewed
  distribution — a significant fraction stay >70 h (sheep, efferent-lymph cannulation +
  LASSO fit; Thomas 2012 PLoS ONE, via Consensus). A pure random-walk simulation predicts
  ~16 h, in good agreement (Textor 2011 PNAS). A whole-organism model gives mean residence
  **~10 h** in major LNs and Peyer's patches, ~2.5 h in spleen, <1 min in lung capillaries
  and liver sinusoids (rat; Ganusov 2014 PLoS Comput Biol, via Consensus).
- **CD4 vs CD8 differ:** CD4 T cells recirculate faster, home more efficiently, **traverse
  the LN twice as quickly**, and spend ~1/3 of transit time interacting with MHC-II on DCs;
  CD8 transit is unaffected by host MHC-I (Mandl 2012 PNAS, via Consensus).
  → A real, cheap, legible asymmetry: **helper T cycles the node faster than cytotoxic T.**
- **Scanning rates:** one DC can interact with **up to 5000 T cells per hour** (Miller 2004
  PNAS); a simulation calibrated to 2-photon data estimates **~2000 T cells/hour per DC and
  ~100 different DCs/hour per T cell** (Beltman et al., via Consensus).
  → "One T cell scans ~100 DCs/hour" is the number to use for a lymph-node throughput bar.
- **DC gut → mesenteric LN:** labelled DCs injected into rat small-intestinal subserosa
  accumulate in MLN with **maximum at 48 h for immature lymph DCs and 24 h for
  cytokine-matured DCs**; CCR7 desensitisation blocks mature DC migration, CCR6 blocks
  immature (Kobayashi 2004 J Leukoc Biol, via Consensus). Under homeostasis, up to **a
  quarter of intestinal cDCs turn over daily**, with **almost complete daily replacement of
  the migratory cDC compartment in the mesenteric LN** (Hager 2025 Nat Commun, via Consensus).
  Only **CD103+** DCs actually migrate in lymph; CX3CR1+ cells are gut-resident and do not
  (Schulz 2009 JEM, via Consensus).
  → Courier delay of **24–48 h** is well supported, and "faster when activated" is real.
  → The game's DC should be **CD103+** by implication; a CX3CR1+ variant that samples but
  never leaves is a legitimate second unit.

---

## 9. What this means for the game — blunt bullets

1. **Raise the neutrophil and lower the dendritic cell.** The neutrophil:macrophage ratio is
   the single largest real speed gap in the roster (7–10×) and the game currently shows 3×.
   Go to macrophage 1 / neutrophil 5. The DC is a 2–3 µm/min meanderer, slower than any
   lymphocyte — it should be **2**, not 3. Its value is the journey it makes, not its pace.

2. **Integer 1–5 can carry exactly two speed tiers plus a floor.** Tier A = macrophage,
   γδ T, plasma cell (sessile/slow, 0–1). Tier B = all lymphocytes and DC (2–3). Tier C =
   neutrophil (5). Everything finer than that is below the literature's own noise floor
   (naive T in mouse LN is reported as 6.2, 11 and 18 µm/min by three groups).

3. **Sacrifice these distinctions, they are not expressible and not real enough to defend:**
   helper T vs cytotoxic T vs NK vs effector T speed (all 6–7 µm/min); naive-in-LN vs
   effector-in-villi (6.2 vs 6.5, statistically similar); T vs B *speed* (the real 5–6×
   difference is in the **motility coefficient**, which is speed × persistence, not speed
   alone, so B = T − 1 is the honest integer).

4. **Spend the saved resolution on directional persistence instead.** Meandering ratio is
   measured, differs sharply by cell and tissue (LN 0.70, gut villi 0.63, inflamed lung
   0.22–0.37), and is *visible from across the room* on a lattice — a persistent cell keeps
   its heading, a meandering one backtracks. One extra float per `UnitProfile`
   (`TurnProbability` / heading inertia) buys more legible unit identity than any speed tweak.
   It also fixes the single biggest honesty gap: real cells perform **correlated** random
   walks, the game performs a memoryless one.

5. **The search ladder survives contact with the literature.** T cell migration in LN really
   is random (directed migration >1.7 µm/min ruled out at 95% confidence, Textor 2011), and
   the Lévy-walk alternative has been largely overturned for liver and brain (Ganusov 2020,
   Patel 2023). Rung 1 is defensible. The caveat to put in GAME_DESIGN.md is that the real
   walk is correlated and constrained to a **stromal fibre network** — which, conveniently,
   is a better argument for a lattice than for continuous space.

6. **The sensing upgrade should raise speed as well as sharpness.** CCR7 ligands are
   chemokinetic: knocking them out cuts T cell velocity 33% and motility coefficient 55%
   (Worbs 2007), and CXCL10 raises CD8 speed in brain without changing walk statistics
   (Harris 2012). A +10–20% speed rider on "Cytokine sensing +" is biologically earned, and
   it makes a repeatable purchase feel like it is doing something.

7. **Give the cytokine field a finite reach of ~4–5 coarse slots and a saturation knee.**
   Real haptotactic CCL21 gradients guide DCs from **up to 90 µm** (≈31 fine tiles ≈ 4.5
   coarse slots; Weber 2013), and neutrophils **fail to chemotax in a saturated gradient**
   (Halilovic 2015). "Beelines within ~4 slots, wanders beyond, and mills around once
   practically on top of the source" is both the measured behaviour and better gameplay than
   a board-wide pull.

8. **Add an LTB4-style unit-emitted relay before adding any new tower.** Pioneer neutrophils
   amplify a local death signal and recruit from **>70 µm** away (Lämmermann 2013). This is
   a second emitter placed by the units themselves — it turns the player's existing neutrophils
   into gradient infrastructure, which is exactly the "sensing improves entry *and* movement"
   double payoff §2a already wants, at zero new art cost.

9. **Freeze durations should separate eaters from killers.** Phagocytosis is ~1.5 particles
   per minute (Horonushi 2023) and DC immobilisation on uptake is "within minutes" (Ng 2008);
   a CTL engages, kills and detaches **within 25 min** (Cazaux 2019) and often needs **three
   hits within 50 min** from different cells to finish a tough target (Weigelin 2021). So:
   phagocyte pause ≈ 0.3–1 s, killer pause ≈ 2–5 s, and "one CTL can't finish a boss, a
   cluster can" is a literature-backed boss mechanic rather than a balance fudge.

10. **Do not model lymphocyte size differences; do model the macrophage's.** T, B, NK and γδ
    are all within ~25% of a neutrophil's diameter — invisible at 3×3 and unreadable at any
    grid resolution the game will ever ship. The macrophage genuinely is bigger (5×5 is fine)
    and the bacterium/virion are already 3× and ~75–100× oversized respectively, so the
    "hardest difficulty tier is to scale" claim in §7 Scale honesty should stay retired.

11. **Gut tissue is not a slow compartment.** Effector CD8 T cells in small-intestinal villi
    move at 6.5 µm/min and patrol the same volume per second as naive T cells in a lymph node
    (Torres 2023). If the game ever wants a "cells are sluggish out in the tissue" difficulty
    lever, it has to come from board geometry or gradient availability — not from a speed
    penalty, which the one paper that measured exactly this game's tissue contradicts.

12. **Extend the ladder downward, not upward, if more resolution is needed.** The constraint
    is that speed is an integer ≥1. Adding a `TicksPerStep` denominator (move 1 tile every
    N ticks) gives macrophage 1/3, monocyte 1/2, DC 1 without raising the neutrophil's top
    speed or touching the tick budget — and it is the only way to represent a tissue
    macrophage's true ~1 µm/min against a neutrophil's 7–10 without the neutrophil becoming
    a blur.

---

## 10. Open / unresolved

- **UNVERIFIED:** γδ IEL migration speed in µm/min (qualitatively dynamic, Edelblum 2012).
- **UNVERIFIED:** interstitial (not intravascular) monocyte speed in µm/min.
- **UNVERIFIED:** per-type leukocyte diameters at digit precision — Schmid-Schönbein 1980 has
  them in tables not reachable through these tools; only the 6–8 µm range (Downey 1990) and
  the ordering (neutrophil ≈ monocyte > lymphocyte) are confirmed here.
- **UNVERIFIED:** enterocyte *width* in human small intestine; only height (27–31 µm) confirmed.
- **UNVERIFIED:** rotavirus virion diameter at a citable figure.
- **UNVERIFIED:** a measured "time from infection onset to first neutrophil arrival" in minutes;
  the literature says "within minutes" without a controlled onset clock.
- **UNVERIFIED:** an in vivo measurement of how much faster a cell finds a target with
  chemotaxis than without. Only model estimates exist (2× from a 0.5 µm/min bias, Textor 2011;
  >10× for Lévy vs Brownian, Harris 2012 — contested).
- **UNVERIFIED:** dendritic cell and tissue macrophage diameters at digit precision.
