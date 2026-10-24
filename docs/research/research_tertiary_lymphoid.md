# Tertiary lymphoid organs in the gut — literature brief for the B-cell follicle upgrade

Research compiled 2026-10-25 via PubMed and Consensus (peer-reviewed literature only; nothing
written from memory). Every factual claim below carries a citation. Claims I could not resolve
against a retrieved source are flagged **UNVERIFIED**.

Per the PubMed tool's terms: material retrieved from PubMed is identified as such, and DOI links
are given for every PubMed-sourced article referenced.

---

## 1. Verdict (three sentences)

**Yes — "B cells deploy into tissue and build a local lymphoid organ" is biologically defensible,
and it is one of the few places in immunology where B cells are literally the construction
crew**: isolated-lymphoid-follicle formation in the small intestine is inducible in adulthood and
specifically requires *lymphotoxin-sufficient B lymphocytes*, not just T cells
([Lorenz et al. 2003, J Immunol](https://doi.org/10.4049/jimmunol.170.11.5475)), and in mice that
lack LTi cells entirely, B cells are the critical lymphotoxin source that builds gut lymphoid
tissue from scratch ([Lochner et al. 2011, J Exp Med](https://doi.org/10.1084/jem.20102065)).

**The honest name depends on where the follicle grows.** If it matures from a pre-seeded cluster
in the gut wall it is an **isolated lymphoid follicle (ILF)** — formally still a *secondary*
lymphoid organ, because its precursor cryptopatch is laid down by a programmed, LTi-dependent
step that happens postnatally even in germ-free mice; if it appears de novo at a site with no
such precursor, driven purely by inflammation, it is a genuine **tertiary lymphoid organ (TLO)**
([Buettner & Lochner 2016, Front Immunol](https://doi.org/10.3389/fimmu.2016.00342)).

**The cheapest accurate name for the game is "isolated lymphoid follicle", with "tertiary
lymphoid organ" reserved for the overgrown/pathological late-game version** — which is exactly
the distinction that gives the upgrade a real, literature-backed drawback rather than an invented
one.

---

## 2. Isolated lymphoid follicles in the small intestine

### 2.1 What they are

According to PubMed, Hamada et al. identified **100–200 lymphocyte-packed clusters along the
antimesenteric wall of the mouse small intestine**, each composed of a large B-cell area
*including a germinal centre*, with **M cells in the epithelium overlying them**
([Hamada et al. 2002, J Immunol](https://doi.org/10.4049/jimmunol.168.1.57)). These were the
first description of ILFs in mouse small intestine, and they are anatomically distinct from both
Peyer's patches and cryptopatches.

The modern framing is a **continuum**, not a type: "solitary intestinal lymphoid tissue" (SILT)
runs from small **cryptopatches (CP)** → immature ILF → large mature ILF, and a mature ILF
"can reach the size of a single PP follicle"
([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)). Mature ILFs have:

- **one** big B-cell follicle (PPs have several)
- a germinal centre and a follicular dendritic cell network — FDC presence is the reliable marker
  of ILF maturity ([Glaysher & Mabbott 2007, Immunology](https://doi.org/10.1111/j.1365-2567.2006.02459.x))
- CD11c⁺ DCs, interspersed LTi cells, a few CD4 T cells —
  **but no confined T-cell zone**
- **a follicle-associated epithelium (FAE) containing M cells**
- ~10% of their B cells are IgA⁺ plasma cells
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342))

Pabst et al. add an important caveat on taxonomy: when they measured large coherent areas of
mouse small intestine, **most aggregates did not match the textbook definition of either CP or
ILF**, varying with strain, age and species — these are "dynamic structures with a great deal of
inborn plasticity" ([Pabst et al. 2005, Eur J Immunol](https://doi.org/10.1002/eji.200425936)).

### 2.2 How they form, and what initiates them

Programmed step (cryptopatch), then induced step (maturation into ILF):

| Step | Trigger | Key molecules | Source |
|---|---|---|---|
| LTi cells cluster at crypt bases → **cryptopatch** | postnatal, programmed; happens in germ-free mice | RORγt⁺ IL-7Rα⁺ LTi; LTα₁β₂ → LTβR on VCAM-1⁺ stromal "LTo" cells | [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342); [Taylor et al. 2004, J Immunol](https://doi.org/10.4049/jimmunol.173.12.7183) |
| CP → **immature ILF** (B-cell influx) | Gram-negative peptidoglycan sensed by epithelial **NOD1** | β-defensin 3 + CCL20 → CCR6 on B cells and LTi | [Bouskra et al. 2008, Nature](https://doi.org/10.1038/nature07450) |
| B-cell recruitment / follicle organisation | stromal and DC chemokine output | **CXCL13** (from stromal cells *and* from CD11c⁺ DCs inside the CP/ILF); CXCL13⁻/⁻ or CXCR5⁻/⁻ → no ILF, CP numbers unaffected | [McDonald et al. 2010, Am J Pathol](https://doi.org/10.2353/ajpath.2010.090723) |
| Lymphocyte entry into the forming follicle | vascular addressin | **α4β7 / MAdCAM-1** — essential for the CP→ILF transition, dispensable for CP formation itself | [Wang et al. 2008, J Immunol](https://doi.org/10.4049/jimmunol.181.6.4052) |
| **Small-intestine-specific requirement** | stromal **RANKL (TRANCE)** | RANKL⁻/⁻ mice: 4-fold fewer CPs, **no B cells in CPs, ILF formation completely blocked**, stromal CXCL13 absent. Colonic ILFs form fine without RANKL | [Knoop et al. 2011, Am J Pathol](https://doi.org/10.1016/j.ajpath.2010.10.045) |
| Maturation to large B-cell cluster | subsequent **TLR / MyD88 / NOD2** detection of bacteria | | [Bouskra 2008](https://doi.org/10.1038/nature07450); [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342) |
| ILC3 positioning inside CP/ILF | **oxysterol 7α,25-OHC → GPR183** on LTi-like ILC3s, made by local fibroblastic stromal cells | | [Emgård et al. 2018, Immunity](https://doi.org/10.1016/j.immuni.2017.11.020) |
| Diet also gates it | **AhR ligands** (polyphenols/glucosinolates — broccoli, cauliflower) and **vitamin A** | AhR⁻/⁻ or plant-free synthetic diet → near-total loss of CP/ILF, PPs and LNs unaffected | [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342) |
| Follicle-associated epithelium polarity | RANKL on stromal cells concentrated **just beneath the FAE**, in ILFs *and* PPs; RANKL expression here is LTβR- and NIK-independent | | [Taylor et al. 2007, J Immunol](https://doi.org/10.4049/jimmunol.178.9.5659) |

### 2.3 "Induced on demand" vs "developmentally programmed" — testing the brief's claim hard

**The brief's claim is right in spirit and wrong in detail. Correct it.**

Evidence *for* induction:
- Lorenz et al. explicitly titled it: ILF formation **is inducible**, the LT/LTβR events required
  "can occur **in adulthood**", mature ILF formation "occurs in response to lumenal stimuli,
  including normal bacterial flora", requires TNFRI, and ILF **mass can be expanded in response
  to mucosal challenge** ([Lorenz et al. 2003](https://doi.org/10.4049/jimmunol.170.11.5475)).
- Peptidoglycan from Gram-negative bacteria is **necessary and sufficient** to induce ILF genesis
  ([Bouskra 2008](https://doi.org/10.1038/nature07450)).
- Germ-free mice have **normal numbers of CPs and small immature ILFs with few B cells**;
  transition to mature ILF requires microbiota
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).
- Colonising germ-free mice with commensals converts **pre-existing** small structures into
  larger ones ([Pabst et al. 2006, J Immunol](https://doi.org/10.4049/jimmunol.177.10.6824)).

Evidence *against* pure induction:
- "The development of CP, although initiated after birth, **proceeds in the absence of any
  exogenous inflammatory stimuli**… it is reasonable to consider ILF as **true SLO**"
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).
- ILF formation **strictly depends on LTi cells** — RORγt-deficient mice have none
  (ibid.).

**So: the site is pre-seeded; the structure is induced.** The accurate game statement is "the gut
pre-lays a grid of tiny lymphoid seeds along the wall, and a local microbial signal makes one of
them swell into a working follicle." A true LTi-independent TLO *can* form in gut — but only in
abnormal conditions (RORγt⁻/⁻ mice; DSS colitis; blockade of normal GALT development), and that
version is the one that comes with the pathology (§5).

### 2.4 Human ILFs

- Human GALT = multi-follicular **Peyer's patches** of the ileum, the **appendix**, and
  **numerous ILFs along the whole intestine**
  ([Mörbe et al. 2021, Mucosal Immunol](https://doi.org/10.1038/s41385-021-00389-4)).
- Humans have **two** ILF classes, resolved only recently:
  - **M-ILF** (mucosal): entirely within the lamina propria, **always a single follicle**, with a
    dome-like FAE containing GP-2⁺ M cells and CD45⁺-filled M-cell pockets.
  - **SM-ILF** (submucosal): extends through the muscularis mucosae into the submucosa; its FAE is
    **hidden in clefts between normal crypts** rather than forming a visible dome; M-cell-like
    cells detectable only by scanning EM.
  Both carry MAdCAM-1⁺ high endothelial venules and efferent lymphatics, both have germinal
  centres, and IgA repertoire sequencing shows they **initiate responses in an anatomically
  restricted way** — regional, not global
  ([Fenton et al. 2020, Immunity](https://doi.org/10.1016/j.immuni.2020.02.001)).
- Human ILFs have been documented since at least 1998, together with a distinct structure called
  the **lymphocyte-filled villus** — a villus whose epithelium resembles PP follicle-associated
  epithelium ([Moghaddami et al. 1998, Gastroenterology](https://doi.org/10.1016/s0016-5085\(98\)70020-4)).
  That paper also concluded that "isolated lymphoid follicles and submucosal lymphoid aggregations
  are components of a single structure" — the thing Fenton et al. later split into M-ILF/SM-ILF.
- **Human ILF total count per small intestine: UNVERIFIED.** No retrieved source gives a number.
  The mouse figure (100–200 per small intestine) is solid; do not silently transfer it to humans.

---

## 3. Tertiary lymphoid organs in general

### 3.1 Definition and how they differ from secondary lymphoid organs

> "Tertiary lymphoid structures (TLSs) are ectopic lymphoid tissues that drive antigen-specific
> immune responses at sites of chronic inflammation. **Unlike secondary lymphoid organs such as
> lymph nodes, TLSs lack capsules** and have their own unique characteristics and functions."
> — [Sato et al. 2023, Nat Rev Nephrol](https://doi.org/10.1038/s41581-023-00706-z)

Structural differences, consolidated:

| Feature | Lymph node / Peyer's patch | TLO / ectopic lymphoid structure |
|---|---|---|
| Fibrous capsule | yes | **no** ([Sato 2023](https://doi.org/10.1038/s41581-023-00706-z)) |
| Mantle zone | yes | **absent** ([Alsughayyir et al. 2017, Front Immunol](https://doi.org/10.3389/fimmu.2017.01639)) |
| Site | fixed, developmentally predetermined | wherever inflammation persists |
| Timing | embryonic / before birth | postnatal, on demand |
| T and B zones | distinct | present in *mature* TLS; absent in early ones |
| HEVs | yes | yes in mature TLS |
| Range | uniform | "from tight clusters of B and T cells to highly organized structures that comprise functional germinal centres" ([Pitzalis et al. 2014, Nat Rev Immunol](https://doi.org/10.1038/nri3700)) |
| Mature-ILF special case | — | **one** B follicle, FDC network, **no defined T zone** ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)) |

A useful operational tell for the game: **presence of a defined T-cell area distinguishes a true
inflammation-induced intestinal TLO from an ordinary mature ILF** — DSS-treated mice grow colonic
structures that look like mature ILFs but, uniquely, have a T zone
([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342), reporting Mebius-group work).

### 3.2 The cytokine/chemokine stack, and who supplies what

- **LTα₁β₂ on inducer cells → LTβR on stromal LTo cells** is the master switch for *all* of it:
  LNs, PPs, CPs and ILFs. LTα⁻/⁻ or LTβR⁻/⁻ mice have no cryptopatches at all
  ([Taylor et al. 2004](https://doi.org/10.4049/jimmunol.173.12.7183)). In adults, **B cells are a
  sufficient LT source** ([Lorenz 2003](https://doi.org/10.4049/jimmunol.170.11.5475);
  [Lochner 2011](https://doi.org/10.1084/jem.20102065)).
- **CXCL13** pulls B cells in. Made by stromal cells *and by the CD11c⁺ DCs inside the follicle*;
  depleting DCs causes ILFs to **regress** and CPs to disorganise
  ([McDonald et al. 2010](https://doi.org/10.2353/ajpath.2010.090723)).
- **CCL19 / CCL21** (CCR7 ligands) build the T zone. In the TNF^ΔARE Crohn's-ileitis mouse,
  *aberrant* CCL19/CCL21 production by gut TLOs increases the density of infiltrating
  effector-memory T cells and their retention — perpetuating the ileitis
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342), reporting McNamee et al.).
- **RANKL** and **IL-7** from LTi/LTo cells drive LTαβ upregulation, a positive feedback loop
  (ibid.).
- **IL-22** from ILC3s is required for ILF induction and structural integrity *during infection*
  but not at steady state; in a salivary-gland TLO model, IL-22 loss blocked B-cell recruitment
  and **strongly reduced autoantibody formation** (ibid.).
- **IL-23** promotes colonic ILF maturation; **IL-25** (epithelium, microbiota-induced) suppresses
  it (ibid.). **IL-27 restrains TLS size** — Il27ra⁻/⁻ mice grow larger, more hyperactive ectopic
  structures with more autoimmunity
  ([Lucchesi et al. 2020, Arthritis Rheumatol](https://doi.org/10.1002/art.41289)).
- **Vagal innervation** of the proximal colon was required for DSS-induced TLO formation, via
  stromal CXCL13/CCL20 upregulation
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).

### 3.3 Timescale from stimulus to functioning structure

The best-characterised inducible model (adenovirus into mouse submandibular gland):

| Day post-stimulus | State |
|---|---|
| 3 | neutrophils/eosinophils/monocytes flood in; monocytes → resident macrophages |
| ~3–10 | CD11b⁺ cells upregulate *Cxcl13*, *Ltb*, *Il18*, *April* |
| 12 | **T cells arrive**; B cells shortly after |
| ~14 | SS-like periductal **lymphoid aggregates** established |
| **19** | **GL7⁺ germinal-centre B cells peak**; Tfh infiltration correlates |
| ≤21 | anti-nuclear antibodies in up to 75% of mice; exocrine dysfunction measurable |

Sources: [Coleby et al. 2021, Clin Exp Rheumatol](https://pubmed.ncbi.nlm.nih.gov/34001304/)
(kinetics) and [Bombardieri et al. 2012, J Immunol](https://doi.org/10.4049/jimmunol.1201216)
(2-week aggregates, functional AID⁺/GL7⁺ GCs, ANA in ≤75%, salivary flow drop over 3 weeks).

Gut analogue: germ-free adult mice monoassociated with a commensal develop **germinal centres in
Peyer's patch follicles at ~10–14 days**, and those GCs **wax and wane over about 3 weeks** even
though the bacteria stay at high density
([Cebra et al., Dev Immunol](https://pubmed.ncbi.nlm.nih.gov/9683661/)).

**Headline: ~2 weeks from stimulus to an organised aggregate, ~3 weeks to a peak germinal
centre.** Not hours, not months.

---

## 4. Do they actually work? Local germinal centres, SHM, class switching

### 4.1 Yes — with caveats, and the caveats are the interesting part

**Evidence they work:**

- Mouse mature ILFs **contain germinal centres and serve as inductive sites for antigen-specific
  IgA responses** ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342);
  [Hamada 2002](https://doi.org/10.4049/jimmunol.168.1.57)).
- **The compensation experiment is the strongest single result for the game's premise.** In mice
  lacking Peyer's patches, significantly *more* mature ILFs form, each with GCs and large FDC
  networks; **the total FDC area summed across all mature ILFs equalled the total FDC area of all
  PPs plus ILFs in normal mice**, and ILF maturation **restored faecal IgA to normal levels**
  ([Glaysher & Mabbott 2007](https://doi.org/10.1111/j.1365-2567.2006.02459.x)). A dispersed
  network of small local follicles can carry the same load as one big patch.
- In mice depleted of lymph nodes, PPs *and* CP-derived ILFs by LTβR–Ig, segmented filamentous
  bacteria still induced **small-intestinal TLOs capable of initiating an IgA response**
  ([Lécuyer et al. 2014, Immunity](https://doi.org/10.1016/j.immuni.2014.03.009)).
- Specific IgA responses to an enteric pathogen could be induced in mice with no LNs and no PPs
  but with ILFs ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).
- In lung, iBALT germinal centres have **classical dark/light-zone polarity**, FDCs, Tfh, and
  GC-B transcriptional profiles **conserved with secondary lymphoid organs**, and they
  independently support maturation and selection of B cells
  ([Tan et al. 2019, Front Immunol](https://doi.org/10.3389/fimmu.2019.00611)). Mice lacking
  spleen, LNs and PPs cleared influenza and survived *higher* doses than normal mice via iBALT
  ([Moyron-Quiroz et al. 2004, Nat Med](https://doi.org/10.1038/nm1091)).
- Ectopic GCs in rheumatoid synovium invariably express **AID**, sit surrounded by
  autoantibody-secreting plasma cells, and keep class-switching **after transplant into SCID mice
  with no new immune-cell influx** — i.e. they are genuinely autonomous
  ([Humby et al. 2009, PLoS Med](https://doi.org/10.1371/journal.pmed.0060001)).

**Evidence the quality is worse:**

- iBALT architecture is "**pleiomorphic and less structurally defined**" than SLOs
  ([Tan 2019](https://doi.org/10.3389/fimmu.2019.00611)).
- SFB — which drives vigorous PP germinal centres — nonetheless produced **"only a low frequency
  of specific IgA-secreting cells with delayed accumulation of somatic mutations"**, and when it
  worked through ILFs/TLOs instead of PPs the resulting IgA and Th17 responses displayed
  **"only limited target specificity"** ([Lécuyer et al. 2014](https://doi.org/10.1016/j.immuni.2014.03.009)).
  *This is the cleanest "local is fast but sloppy" citation available.*
- Somatic hypermutation **requires a manifest germinal centre**: T-independent IgA plasma cells
  carried few V-region mutations, while wild-type LP B cells (GC-derived) were heavily and
  clonally mutated ([Bergqvist et al. 2010, J Immunol](https://doi.org/10.4049/jimmunol.0903861)).
- Tumour TLS mouse work notes that persistence "**also enables the accumulation of regulatory cell
  components (regulatory T and B cells)**", making TLS sites of both promotion *and* suppression
  of the response ([Riffard et al. 2026, Immunol Rev](https://doi.org/10.1111/imr.70035)).

### 4.2 Verdict for the game

A local follicle **can** run a real germinal centre with class switching to IgA, without antigen
reaching a distant node. It is **measurably faster to reach and measurably less specific in what
it produces.** That asymmetry is free game design.

---

## 5. The cost side — the most important section

### 5.1 Do TLOs drive pathology or merely accompany it?

**Both, and the field knows it is both.** The cleanest statement of the split:

> "The presence of TLSs correlates with a **favourable** disease course in cancer and infection.
> Conversely, in **autoimmune diseases and chronic age-related inflammatory diseases**, the
> presence of TLSs is associated with a **more severe** disease course."
> — [Sato et al. 2023](https://doi.org/10.1038/s41581-023-00706-z)

**Causal evidence, not just correlation (these are the ones that matter):**

1. **Gut, and directly causal.** RORγt-deficient mice (no LTi, no LNs, no PPs, no ILFs) compensate
   by growing large numbers of B-cell-driven tertiary lymphoid tissues. On epithelial damage they
   develop **severe colitis with neutrophil and IgG⁺ B-cell infiltration, high AID expression in
   the tLTs, and wasting disease**. The pathology was **prevented by blocking lymphoid-tissue
   formation** (LTβR–Ig) or by antibiotics, and reduced by IVIG. The authors' own framing: the
   compensation "comes at a **high cost for the host** and can lead to a **deregulated B cell
   response and aggravated inflammatory pathology**"
   ([Lochner et al. 2011, J Exp Med](https://doi.org/10.1084/jem.20102065)).
   **This is a knockdown causal result in the gut, in B-cell-built lymphoid tissue. It is the
   single best source for the game's drawback.**

2. **Gut, and mechanically obstructive.** In TNF^ΔARE mice (Crohn's ileitis), mesenteric TLOs
   **form at the valves of collecting lymphatic vessels**, **obstruct cellular and molecular
   outflow from the gut**, and are sites of **lymph leakage and backflow**. TNF neutralisation
   started early restored lymph transport; **robustly developed, chronic TLOs resisted regression
   and resisted restoration of flow**
   ([Czepielewski et al. 2021, Immunity](https://doi.org/10.1016/j.immuni.2021.08.025)).
   *A structure that physically blocks the drainage route to the lymph node, and can't be removed
   once mature, is a mechanic sitting on a plate.*

3. **Human Crohn's.** B-cell-rich TLO-like aggregates impinge on the collecting lymphatics that
   enter and exit mesenteric lymph nodes, positioned "in a manner expected to affect delivery of
   lymph to LNs" ([Randolph et al. 2016, Am J Pathol](https://doi.org/10.1016/j.ajpath.2016.07.015)).
   Immunohistochemistry of 24 Crohn's patients found inflammatory lymphoid follicles in **all 24**,
   with perilymphangitis and in some cases granuloma-obstructed lymphatics
   ([Sura et al. 2011, Aliment Pharmacol Ther](https://doi.org/10.1111/j.1365-2036.2011.04573.x)).

4. **Autoantibodies are manufactured locally.** RA synovial ectopic GCs invariably express AID,
   are ringed by ACPA⁺ plasma cells reactive to citrullinated fibrinogen, and keep producing human
   IgG ACPA after transplantation into SCID mice with no new B-cell influx
   ([Humby et al. 2009](https://doi.org/10.1371/journal.pmed.0060001)).

5. **The hardest number in the whole brief — malignancy.** In primary Sjögren's syndrome, ~25% of
   patients have GC-like ectopic structures at diagnosis
   ([Risselada et al. 2013, Semin Arthritis Rheum](https://doi.org/10.1016/j.semarthrit.2013.01.001)).
   Over 1,855 patient-years, **14% of GC-positive vs 0.8% of GC-negative patients developed
   non-Hodgkin lymphoma**, median onset 7 years
   ([Theander et al. 2011, Ann Rheum Dis](https://doi.org/10.1136/ard.2010.135186)); an independent
   cohort put it at a **7.8-fold increased lymphoma hazard (95% CI 1.73–34.86)**
   ([Sène et al. 2018, Arthritis Rheumatol](https://doi.org/10.1002/art.40575)).
   A chronic local germinal centre is a long-running mutation factory, and sometimes it produces a
   tumour.

6. **Lupus nephritis.** Renal TLS presence is associated with severe tubulointerstitial
   inflammation, higher activity and chronicity indices and **poor treatment response**; resolving
   the TLS by several unrelated interventions improves disease
   ([Wang et al. 2023, Front Immunol](https://doi.org/10.3389/fimmu.2023.1204777)).

7. **Autoimmune vasculitis.** Adventitial TLS house **TCF1-high CD4 T stem cells that escape
   exhaustion** and continuously resupply pathogenic effectors — the structure is what makes the
   autoimmunity un-exhaustible
   ([Goronzy et al. 2025, Immunol Rev](https://doi.org/10.1111/imr.70020)).

8. **Transplant.** Post-transplant TLOs are "**most often associated with organ rejection**",
   though a minority contribute to tolerance, and "TLOs can be associated with development of
   lymphomas" ([Ruddle 2023, Transplantation](https://doi.org/10.1097/TP.0000000000004663)).
   A competing review argues they can be regulatory too
   ([Koenig & Thaunat 2016, Front Immunol](https://doi.org/10.3389/fimmu.2016.00646)).

9. **Rheumatoid synovium, clinical.** Lymphoid neogenesis in synovial biopsy was associated with
   poor-prognosis factors and with needing biologic therapy
   ([Azuaga et al. 2024, Arthritis Res Ther](https://doi.org/10.1186/s13075-024-03374-x)).
   Bombardieri et al. are explicit that it is *still unresolved* whether ELSs mark distinct disease
   phenotypes or are "evolutionary manifestations of chronic inflammation"
   ([Bombardieri et al. 2017, Nat Rev Rheumatol](https://doi.org/10.1038/nrrheum.2016.217)).

**In tumours (the contested case):** TLS are "initially described in NSCLC as functional immune
lymphoid structures associated with **better** clinical outcome… found in many other carcinomas,
melanomas and sarcomas, and associated with improved response to immunotherapy"
([Teillaud et al. 2024, Nat Rev Cancer](https://doi.org/10.1038/s41568-023-00619-w)).
Intratumoral TLS in hepatocellular carcinoma predicted lower early relapse (HR 0.46, p=0.005), and
the effect scaled with TLS maturity — **but in the same tumour type, TLS in the *non-tumoural*
liver have been shown to promote growth of malignant hepatocyte progenitors**
([Calderaro et al. 2019, J Hepatol](https://doi.org/10.1016/j.jhep.2018.09.003)).
In breast cancer the effect is described as "controversial"
([Wang et al. 2022, Front Immunol](https://doi.org/10.3389/fimmu.2022.868155)). So: in cancer,
mostly good, context-dependent, and *location within the lesion matters*.

### 5.2 Does the gut literature disagree with the autoimmunity literature?

**Partly, yes — say so plainly.**

- The autoimmunity field's position is confident: TLS sustain autoreactivity and predict worse
  outcomes ([Pipi et al. 2018, Front Immunol](https://doi.org/10.3389/fimmu.2018.01952);
  [Sato 2023](https://doi.org/10.1038/s41581-023-00706-z)).
- The **gut** field is more hedged. Buettner & Lochner, surveying every mouse and human study they
  could find, conclude: intestinal inflammation is consistently associated with lymphoid expansion,
  but **"the contribution of these lymphoid organs to the perpetuation or exacerbation of the
  inflammatory pathology was, in the cases where it was tested, less evident"** — and
  LN/PP-deficient mice in DSS colitis formed follicles with **no correlation to disease severity**
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).
- Also gut-specific and **protective**: ILFs provide the microenvironment where ILC3s and DCs
  interact via LTβR to produce IL-22, which is essential for surviving *Citrobacter rodentium*;
  LTβR-deficient mice die rapidly of it (ibid.). And ILFs harbour commensals that induce IL-10 and
  protect against DSS colitis (ibid.).
- Colonic ILFs are also described as "**indispensable** for normal colonic mucosal regeneration"
  ([Sipos et al. 2009, Pathol Oncol Res](https://doi.org/10.1007/s12253-009-9181-x)); but the same
  group notes ILFs may support tumour growth via VEGFR signalling
  ([Sipos & Muzes 2011, World J Gastroenterol](https://doi.org/10.3748/wjg.v17.i13.1666)).

**Net:** in the gut, follicles are *normal infrastructure* and net-protective at ordinary density.
The pathology appears when they are **too many, too large, or forming where they should not** —
which is exactly the right shape for an upgrade with a stacking penalty.

---

## 6. B-cell follicles and gut IgA specifically

### 6.1 Where IgA class switching actually happens — the state of the argument

This is genuinely contested; here is the argument in order.

**Position A — it happens in situ in the lamina propria.** Fagarasan et al. found LP B220⁺IgA⁺
cells expressing AID *and* circular DNA transcripts (the molecular fingerprint of a switch that
has just happened), and showed LP IgM⁺ B cells are pre-committed to switch, with LP stromal cells
enhancing it — concluding gut IgA⁺ cells are "generated in situ"
([Fagarasan et al. 2001, Nature](https://doi.org/10.1038/35074115)). 456 citations; this is the
paper that started the fight.

**Position B — no, it does not, and the LP result was an artefact of contaminating follicles.**
Bergqvist et al. found **no molecular marker of IgA CSR in gut LP at all**, in either wild-type or
CD40⁻/⁻ mice, and — more surprisingly — "no to low IgA CSR in mesenteric lymph nodes **or isolated
lymphoid follicles**" in wild-type mice
([Bergqvist et al. 2006, J Immunol](https://doi.org/10.4049/jimmunol.177.11.7772)).
Their follow-up refined it: **the Peyer's patch is the dominant IgA CSR site in both CD40⁻/⁻ and
wild-type mice**; "the colon LP was found to host IgA CSR **only when lymphoid follicles were
present**"; CSR happens in PPs at a **pre-germinal-centre stage** (GL7-intermediate B cells), while
**somatic hypermutation requires a manifest GC** (GL7-high)
([Bergqvist et al. 2010, J Immunol](https://doi.org/10.4049/jimmunol.0903861)).

**Position C — ILFs are a major T-independent CSR site.** Tsuji et al. showed ILFs are where AID
induction and IgA class switching happen **in the absence of T cells**, and that ILF formation
needs RORγt⁺ LTi × stromal cell crosstalk through LTβR plus simultaneous TLR sensing of bacteria
([Tsuji et al. 2008, Immunity](https://doi.org/10.1016/j.immuni.2008.06.013), 464 citations).
Buettner & Lochner report this as the mainstream reading: ILFs "have been proposed to be a major
site for T cell-independent B cell class switch to IgA"
([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).

**Position D — more recent refinement.** B-cell *residency* in the gut requires CCR6⁺ LTi cells
(i.e. requires the lymphoid structure), but IgA plasma-cell generation itself proceeded efficiently
without T cells *and without helper ILCs*
([Zheng et al. 2021, PNAS](https://doi.org/10.1073/pnas.2106754118)).

**Honest summary for the design doc:** the field has converged on *organised lymphoid tissue is
required* — PPs dominate, ILFs are a real and sufficient alternative especially for T-independent
responses, and **the free lamina propria by itself is not an inductive site**. Pabst & Slack's
review frames the whole T-dependent-vs-T-independent IgA question as an active, growing
disagreement, not a settled one
([Pabst & Slack 2019, Mucosal Immunol](https://doi.org/10.1038/s41385-019-0227-4)).

### 6.2 T-dependent vs T-independent IgA — speed and quality

| | T-dependent | T-independent |
|---|---|---|
| Site | PP / colonic patch germinal centres; Tfh-driven | ILFs, and PP pre-GC stage |
| Needs CD40? | yes | **no** — CD40⁻/⁻ mice have near-normal total gut IgA and near-normal LP IgA plasma cells, despite having **no germinal centres at all** and failing completely against T-dependent antigens ([Bergqvist 2006](https://doi.org/10.4049/jimmunol.177.11.7772), [2010](https://doi.org/10.4049/jimmunol.0903861)) |
| Somatic hypermutation | extensive, clonally related | **few mutations** ([Bergqvist 2010](https://doi.org/10.4049/jimmunol.0903861)) |
| Human blood counterpart | CD27⁺IgA⁺ | CD27⁻IgA⁺: **weakly mutated, often Igλ, enriched in polyreactive clones recognising many bacterial species** ([Berkowska et al. 2015, J Immunol](https://doi.org/10.4049/jimmunol.1402708)) |
| Target specificity | high-affinity, antigen-specific | broad, cross-reactive anti-commensal cover |

**Implication the brief asked for:** yes, a local B-cell structure can produce secretory IgA
without T help — that is precisely what the T-independent ILF pathway is for. It will be
**broad-spectrum and low-affinity**. High-affinity, species-specific IgA needs a germinal centre
with Tfh help, and the game's DC→T-cell shuttle is the right fiction for that.

### 6.3 One more honest complication

Human GALT is now argued to be unusual: it maintains germinal centres **chronically**, under
permanently high antigen load, which changes the rules for B-cell engagement relative to other
lymphoid tissues, and in humans may support diversification and selection of *systemic* B cells —
not just gut ones ([Bemark et al. 2024, Trends Immunol](https://doi.org/10.1016/j.it.2024.01.006)).
A gut follicle is never "off."

---

## 7. Peyer's patches, for comparison

- **Human PP count.** Cornes (1965) counted 100 normal small intestines from 20 weeks' gestation to
  96 years, defining a PP as ≥5 follicles. Numbers ranged **from 44 at 20 weeks' fetal life to over
  200** in later life, rising with age to a peak then declining
  ([Cornes 1965, Gut](https://doi.org/10.1136/gut.6.3.225) — PubMed holds only a scanned image, so
  the **exact peak count and peak age window are UNVERIFIED from the primary text**; secondary
  sources citing Cornes give ~339 total, and StatPearls citing Cornes puts the peak at 15–25 years).
- **A directly measured, modern, conflicting count:** van Kruiningen et al. opened and
  transilluminated the distal 200 cm of ileum from 55 adults: **mean 29.4 ± 5.4 patches in the
  youngest group falling to 19.0 ± 3.0 in the oldest**, with **46% of all patch tissue in the
  terminal 25 cm**, a "lymphoid ring" in the terminal 10–15 cm, and **total patch area peaking at
  47.4 ± 1.0 cm² at age 21–30**
  ([van Kruiningen et al. 2002, Inflamm Bowel Dis](https://doi.org/10.1097/00054725-200505000-00004)
  — *note: DOI resolution for this 2002 article is uncertain;
  [PubMed 11889315](https://pubmed.ncbi.nlm.nih.gov/11889315/) is authoritative*).
  **The two counts disagree by an order of magnitude because they count different things** (Cornes:
  microscopic ≥5-follicle patches across the whole small intestine; van Kruiningen: macroscopically
  visible patches in the distal 200 cm). Use ~20–30 for "big visible patches in the ileum" and the
  higher figure for "all patches everywhere."
- **Structure.** Human PPs consist of distinct B follicles with T-cell-rich interfollicular
  regions, a subepithelial dome, and an M-cell-containing FAE; **they may comprise several hundred
  follicles**, size peaking in adolescence and declining thereafter; they **constitutively contain
  germinal centres** ([Fenton et al. 2020](https://doi.org/10.1016/j.immuni.2020.02.001)).
- **Do they change with infection/activity?** Patch *number* is largely fixed (prenatally
  programmed) but **size and activity are not**: germ-free and minimally-colonised mice have
  significantly **smaller PP follicles at the same follicle number**
  ([Pačes et al. 2025, Gut Microbes](https://doi.org/10.1080/19490976.2025.2474141)); germ-free
  adults monoassociated with commensals grow GCs in 10–14 days (Cebra, above); SFB colonisation
  "induced an increase in the size and overall cell number in PPs," enhancing naive, follicular and
  GC B cells plus Th17 and Tfh ([Kang et al. 2023, J Immunol](https://doi.org/10.4049/jimmunol.210.Supp.158.03)).
- **Mouse ILF vs PP:** a mature ILF "can reach the size of **a single PP follicle**"
  ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).

**→ The ratio the game needs: one player-built follicle should be worth roughly *one follicle of
the Peyer's patch*, not the whole patch.** Human PPs have up to several hundred follicles; the
game's 4-lane patch should be read as "many follicles." A player-built ILF is the atom; the patch
is the molecule.

---

## 8. Geometry and physical constraints

### 8.1 Where they sit

- Mouse **cryptopatches** are named for their location: **at the bottom of the intestinal crypts**,
  in the lamina propria ([Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342)).
  CP → ILF therefore **grows upward from the crypt base toward the epithelium**.
- Mouse ILFs sit **in the lamina propria** of the small intestine, on the **antimesenteric wall**,
  with **M cells in the epithelium directly overlying them**
  ([Hamada 2002](https://doi.org/10.4049/jimmunol.168.1.57)).
- Human **M-ILF**: entirely within the lamina propria; **dome-like FAE with GP-2⁺ M cells**.
  Human **SM-ILF**: crosses the muscularis mucosae into the submucosa; its FAE is **buried in
  clefts between epithelial crypts**, not a visible dome
  ([Fenton et al. 2020](https://doi.org/10.1016/j.immuni.2020.02.001)).
- RANKL expression in both ILFs and PPs is **concentrated just beneath the FAE** — the
  epithelium-facing pole is the organising pole
  ([Taylor et al. 2007](https://doi.org/10.4049/jimmunol.178.9.5659)).
- Rat ILFs are described as developing at the bottom of **lymphocyte-filled villi**, i.e. inside
  villus bodies ([Hitotsumatsu et al. 2005, J Gastroenterol](https://doi.org/10.1007/s00535-005-1679-8)).

**Yes, they disrupt the epithelium above them.** The FAE is a real, different epithelium: no (or
reduced) villi, a dome, and M cells. That is exactly how the game already renders the Peyer's
patch, so the rendering convention transfers.

### 8.2 Dimensions

| Thing | Measured value | Source |
|---|---|---|
| Human duodenal **villus height**, normal | **396 µm** (median; 248 µm in environmental enteric dysfunction, 208 µm coeliac) | [Ehsan et al. 2024, Am J Clin Nutr](https://doi.org/10.1016/j.ajcnut.2024.02.001) |
| Human duodenal **crypt depth**, normal | **246 µm** (299 µm EED, 365 µm coeliac) | ibid. |
| ⇒ human mucosal thickness (villus + crypt) | **~640 µm** | derived from ibid. |
| Rat small-intestine **mucosal thickness** | 526 ± 17 µm; submucosa 47 ± 2 µm; muscularis 146 ± 7 µm | [Abdreshov et al. 2024, Int J Biol Chem](https://doi.org/10.26577/ijbch.2024v17i1-a3) |
| Human **SM-ILF** | **> 0.75 mm diameter** for the ones large enough to see and dissect under a stereo microscope | [Fenton et al. 2020](https://doi.org/10.1016/j.immuni.2020.02.001) |
| Human **M-ILF** | a single follicle, within the ~640 µm mucosa; no diameter published in retrieved sources | **UNVERIFIED** (bounded above by mucosal thickness) |
| Mouse **ILF** | "can reach the size of a single PP follicle"; no µm figure in retrieved sources | [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342); µm **UNVERIFIED** |
| Mouse **cryptopatch** diameter in µm | — | **UNVERIFIED**; no retrieved source gives one |
| Canine gastric lymphoid follicle (closest measured analogue) | mean **295 µm** diameter | [Negoescu et al. 2024, Animals](https://doi.org/10.3390/ani14060872) |

### 8.3 What this means at 20 µm per coarse slot

| Structure | Real size | Coarse slots (÷20 µm) |
|---|---|---|
| One enterocyte | ~20 µm | 1 |
| Crypt depth | 246 µm | ~12 |
| Villus height | 396 µm | ~20 |
| Full mucosa | ~640 µm | ~32 |
| Lymphoid follicle (~300 µm, canine analogue) | 295 µm | **~15** |
| Human SM-ILF (dissectable) | ≥750 µm | **~38** |

**Flag for the Director: the game's existing geometry is already heavily schematic.** The 100×40
board at 20 µm/slot would be 2 mm × 0.8 mm of real gut — smaller than a single human Peyer's
patch, and the game's 4-lane patch (80 µm) is ~5× *smaller* than a single lymphoid follicle.
Don't try to fix this by scaling; the board is a diagram, not a micrograph. The usable guidance is
**relative**: a follicle is on the order of **the depth of a crypt plus half a villus**, i.e. it
should look like it occupies the full wall-to-mid-tissue depth of one or two lanes — comparable to
`FollicleDepthCells = 3` already in `GutWallFeatures.cs`, and **narrower than the 4-lane Peyer's
patch**, which represents many follicles.

---

## 9. Reversibility

**ILFs: fully reversible in both directions, and they are reversible by *size*, not by existence.**

- SILT in germ-free mice is dominated by **small** structures; colonisation shifts the spectrum
  upward **by converting pre-existing structures into larger ones**, not by making new ones
  ([Pabst et al. 2006](https://doi.org/10.4049/jimmunol.177.10.6824)).
- Germ-free mice have a **significantly smaller SILT volume**; PP follicles are smaller at
  unchanged follicle number ([Pačes et al. 2025](https://doi.org/10.1080/19490976.2025.2474141)).
- Intestinal lymphoid aggregates are explicitly "**dynamic structures with a great deal of inborn
  plasticity**" ([Pabst et al. 2005](https://doi.org/10.1002/eji.200425936)).
- **Depleting the DCs inside an ILF causes the ILF to regress and the cryptopatch to disorganise**
  ([McDonald et al. 2010](https://doi.org/10.2353/ajpath.2010.090723)). The structure needs
  continuous upkeep from the cells inside it.
- Germinal centres inside PPs **wax and wane over ~3 weeks** even with the stimulating bacteria
  still present at high density ([Cebra et al.](https://pubmed.ncbi.nlm.nih.gov/9683661/)).
- Antibiotics **prevented** the TLO-driven pathology in RORγt⁻/⁻ mice
  ([Lochner et al. 2011](https://doi.org/10.1084/jem.20102065)).

**TLOs: reversible early, hard-set late.** This is the sharpest reversibility finding in the brief:

> "TNF neutralization begun at **early** stages of TLO formation restored lymph transport.
> However, **robustly developed, chronic TLOs resisted regression** and restoration of flow after
> TNF neutralization."
> — [Czepielewski et al. 2021](https://doi.org/10.1016/j.immuni.2021.08.025)

In tumours, TLS do disaggregate: "**the disruption of germinal centers and the progressive
disaggregation of TLS following tumor regression and neoantigen clearance**" is noted as a point
where TLS differ from lymph nodes, though "whether a form of local tissue memory exists following
TA-TLS resolution … remain[s an] open question"
([Riffard et al. 2026](https://doi.org/10.1111/imr.70035)). In HCC treated with neoadjuvant
immunotherapy, regressing tumour regions contained **"involuted" TLS** — the B-cell follicle
dispersed while the T-cell zone persisted and gained memory markers
([Shu et al. 2024, Nat Immunol](https://doi.org/10.1038/s41590-024-01865-2)). So resolution is
**partial and ordered**: the B follicle goes first, a T-cell residue stays.

**No retrieved source gives a resolution half-life in days for a gut ILF after stimulus removal
— that figure is UNVERIFIED.** The defensible timescale statement is "same order as formation,
i.e. weeks, and only while it is young."

---

## 10. The numbers table (game-convertible)

Coarse slot ≈ 20 µm ≈ one enterocyte.

| Quantity | Value | Slots / rounds equivalent | Confidence | Source |
|---|---|---|---|---|
| ILFs per **mouse** small intestine | **100–200** | — | high | [Hamada 2002](https://doi.org/10.4049/jimmunol.168.1.57) |
| ILFs per **human** small intestine | "numerous"; no count | — | **UNVERIFIED** | [Mörbe 2021](https://doi.org/10.1038/s41385-021-00389-4) |
| Peyer's patches, human whole small intestine | 44 (fetal) → >200 (adult); ~339 cited secondhand | — | medium (primary text is a scan) | [Cornes 1965](https://doi.org/10.1136/gut.6.3.225) |
| Peyer's patches, human distal 200 cm ileum (macroscopic) | **29.4 ± 5.4** young → **19.0 ± 3.0** old | — | high | [van Kruiningen 2002, PMID 11889315](https://pubmed.ncbi.nlm.nih.gov/11889315/) |
| Total PP area, human, age 21–30 | **47.4 ± 1.0 cm²** | — | high | ibid. |
| Follicles per human Peyer's patch | up to **several hundred** | — | high | [Fenton 2020](https://doi.org/10.1016/j.immuni.2020.02.001) |
| Mature ILF size relative to PP | **= one PP follicle** | — | high | [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342) |
| Human SM-ILF diameter (dissectable) | **>750 µm** | **~38 slots** | high | [Fenton 2020](https://doi.org/10.1016/j.immuni.2020.02.001) |
| Lymphoid follicle diameter (canine gastric analogue) | **~295 µm** | **~15 slots** | medium (wrong organ/species) | [Negoescu 2024](https://doi.org/10.3390/ani14060872) |
| Human villus height | **396 µm** | **~20 slots** | high | [Ehsan 2024](https://doi.org/10.1016/j.ajcnut.2024.02.001) |
| Human crypt depth | **246 µm** | **~12 slots** | high | ibid. |
| Human mucosal thickness | **~640 µm** | **~32 slots** | high (derived) | ibid. |
| First ILFs appear postnatally (mouse) | day **7** (BALB/c) / day **25** (C57BL/6) | — | high | [Hamada 2002](https://doi.org/10.4049/jimmunol.168.1.57) |
| Stimulus → organised lymphoid aggregate | **~12–14 days** | ≈ a mid-game stretch of rounds | high | [Bombardieri 2012](https://doi.org/10.4049/jimmunol.1201216); [Coleby 2021](https://pubmed.ncbi.nlm.nih.gov/34001304/) |
| Stimulus → peak germinal centre | **~19 days** | | high | [Coleby 2021](https://pubmed.ncbi.nlm.nih.gov/34001304/) |
| Stimulus → GCs in Peyer's patch (gut, germ-free colonisation) | **10–14 days** | | high | [Cebra, PMID 9683661](https://pubmed.ncbi.nlm.nih.gov/9683661/) |
| GC wax-and-wane cycle with stimulus still present | **~3 weeks** | → a decaying structure, not a permanent one | high | ibid. |
| Autoantibodies appear in an induced TLO model | **≤3 weeks**, up to **75%** of mice | the drawback's onset delay | high | [Bombardieri 2012](https://doi.org/10.4049/jimmunol.1201216) |
| TLO regression after removing the driver | **possible if early; chronic TLOs resist regression** | ⇒ reversible early, permanent late | high | [Czepielewski 2021](https://doi.org/10.1016/j.immuni.2021.08.025) |
| ILF regression after removing the cells that maintain it | DC depletion → **ILF regression** | ⇒ needs upkeep | high | [McDonald 2010](https://doi.org/10.2353/ajpath.2010.090723) |
| Gut ILF resolution half-life after stimulus removal | — | — | **UNVERIFIED** | — |
| Sjögren's: GC-like ELS prevalence at diagnosis | **~25%** | base rate of the bad outcome | high | [Risselada 2013](https://doi.org/10.1016/j.semarthrit.2013.01.001); [Theander 2011](https://doi.org/10.1136/ard.2010.135186) |
| Sjögren's: lymphoma in GC⁺ vs GC⁻ | **14% vs 0.8%**, median onset **7 yr**; HR **7.8** | the long-run cost | high | [Theander 2011](https://doi.org/10.1136/ard.2010.135186); [Sène 2018](https://doi.org/10.1002/art.40575) |
| IgA⁺ plasma cells as fraction of mature-ILF B cells | **~10%** | the follicle's IgA throughput | high | [Buettner & Lochner 2016](https://doi.org/10.3389/fimmu.2016.00342) |
| Functional parity, ILF network vs PPs | **total FDC area equal**; faecal IgA **restored to normal** | ⇒ N follicles ≈ 1 patch | high | [Glaysher & Mabbott 2007](https://doi.org/10.1111/j.1365-2567.2006.02459.x) |

---

## 11. What this means for the game — 10 blunt bullets

1. **Rename it.** "B cells deploy into tissue and build a lymphoid organ" is real, but the honest
   name for the thing they build in the gut wall is an **isolated lymphoid follicle**, and the
   honest name for the overgrown late version is a **tertiary lymphoid organ**. Having both names
   in the game — the second one unlocked by abuse of the first — is free narrative structure that
   costs no new mechanics.

2. **Don't let the player build it anywhere. Build it on a seed.** The gut pre-lays cryptopatches
   at crypt bases along the whole wall; the player's B cells *mature* one, they don't invent one.
   The repo already has `GutWallFeatures.SetCryptCount` placing crypts on a fixed van der Corput
   sequence at the wall — **the same sequence is the natural cryptopatch grid**. "Place a follicle
   on any crypt you own" is both better biology and a cleaner placement rule than free placement,
   and it preserves the Director's existing snap-to-wall convention. (It also makes the first
   player-placed-in-tissue object less of a departure than it sounds.)

3. **The follicle grows upward from the crypt base and tears the epithelium above it.** FAE is a
   real thing: dome, no villi, M cells. **A matured follicle should give that lane its own M-cell
   sampling for free** — mouse ILFs have M cells over them, as do human M-ILFs. That is a second
   antigen tap on the lumen side, which is a far more interesting payoff than "shorter DC walk."

4. **One follicle ≈ one Peyer's-patch follicle, not one Peyer's patch.** A human PP has up to
   several hundred follicles; a mature ILF reaches the size of exactly one of them. Visually and
   numerically the player-built structure should be **smaller and narrower than the existing
   4-lane patch** — but the Glaysher result says that **enough of them sum to the same total
   capacity** (equal FDC area, faecal IgA fully restored). That is the stacking curve: weak
   individually, patch-equivalent at ~4–6.

5. **Make it slow in the right way: ~2 weeks to organise, ~3 weeks to peak.** Across the best
   inducible models the sequence is identical — innate cells by day 3, chemokine switch, T cells
   day 12, B cells after, germinal centre peaking day 19. A follicle that is a *warm-up timer* with
   visible stages (empty dome → B cluster → FDC network → germinal centre) is both accurate and
   exactly the kind of visible feedback this project already favours.

6. **It is not permanent, and that is canon, not a nerf.** Germinal centres wax and wane on ~3-week
   cycles even with the antigen still present; depleting the DCs inside an ILF makes it regress;
   germ-free guts have small follicles that re-grow on colonisation. **A follicle should decay
   toward a cryptopatch when nothing is being sampled nearby, and re-bloom when pressure returns.**
   Decay-to-seed, not decay-to-nothing.

7. **The quality penalty is real and specific: local IgA is fast, broad and low-affinity.** SFB
   working through ILFs/TLOs instead of Peyer's patches gave IgA with "delayed accumulation of
   somatic mutations" and "limited target specificity"; T-independent IgA plasma cells carry few
   V-region mutations; human T-independent IgA B cells are polyreactive. **Game rule: the follicle
   grants knowledge/IgA against a *broad class* quickly, while the DC→lymph-node shuttle is the
   only route to species-specific, high-knowledge IgA.** The upgrade shortcuts the walk but
   downgrades the product — it does not replace the shuttle, it competes with it.

8. **RECOMMENDED DRAWBACK #1 (the headline, drawn straight from the pathology literature):
   the follicle obstructs the very drainage it was built to bypass.** In the TNF^ΔARE Crohn's
   ileitis model, TLOs form **at the valves of collecting lymphatic vessels, obstruct cellular and
   molecular outflow from the gut toward mesenteric lymph nodes, and cause lymph leakage and
   backflow**; early intervention restores flow, **chronic TLOs resist regression and resist
   restoration of flow** ([Czepielewski 2021](https://doi.org/10.1016/j.immuni.2021.08.025)).
   In game: each follicle **slows DC transit through its lanes** toward the lymph node (and/or
   degrades the node's throughput), the penalty is removable while the follicle is young, and
   **becomes permanent once it matures**. The player trades the shuttle away to get the follicle —
   literally, mechanically, and with a real citation behind it.

9. **RECOMMENDED DRAWBACK #2 (the stacking penalty, also real):
   too many follicles turn protective into pathological.** RORγt⁻/⁻ mice that compensate for
   missing GALT by growing large numbers of B-cell-built tertiary lymphoid tissues get **severe
   colitis, neutrophil and IgG⁺ B-cell infiltration, high AID in the structures and wasting
   disease — and blocking lymphoid-tissue formation prevented the pathology**
   ([Lochner 2011](https://doi.org/10.1084/jem.20102065)). In game: **beyond N follicles, they
   start damaging host cells in their own lanes** (self-inflicted tissue loss, the project's
   existing debris/death pathway, so no new systems). Pair it with the autoantibody flavour —
   ectopic germinal centres manufacture class-switched autoantibodies on site
   ([Humby 2009](https://doi.org/10.1371/journal.pmed.0060001)) and up to 75% of mice in the
   induced-TLO model develop ANAs within 3 weeks
   ([Bombardieri 2012](https://doi.org/10.4049/jimmunol.1201216)). Optional late-game flourish if
   you want a horror beat: ~14% of Sjögren's patients with ectopic germinal centres develop
   lymphoma vs 0.8% without ([Theander 2011](https://doi.org/10.1136/ard.2010.135186)) — a
   follicle left mature and unsupervised for long enough can turn into an enemy.

10. **Be honest about one correction and one disagreement when you write the design doc.**
    (a) ILFs are **not** purely "induced on demand" — the cryptopatch seed is programmed, only the
    maturation is induced; the game should reflect that by seeding sites. (b) The autoimmunity
    literature is confident that TLOs are causal; the **gut** literature is not — Buettner &
    Lochner found that "where it was tested," the contribution of intestinal lymphoid structures to
    worsening pathology was "less evident," and gut ILFs are separately **required** for surviving
    enteric infection (ILC3–DC–IL-22 axis) and for normal mucosal regeneration. So the drawback
    should be **conditional on excess**, not intrinsic to having one. A single follicle is good
    biology and should be a clean buy; the fifth one is where the player starts paying.

---

## Caveats and open items

- **UNVERIFIED:** human ILF count per small intestine; mouse ILF and cryptopatch diameters in µm;
  human M-ILF diameter; gut ILF resolution half-life after stimulus removal; Cornes' exact peak PP
  count and peak age from the primary text (the PMC copy is a scan with no extractable text).
- The 295 µm follicle diameter is from **canine gastric** lymphoid follicles — the closest directly
  measured figure I could retrieve, but the wrong organ and species. Treat it as an order of
  magnitude only.
- Peyer's-patch counts from Cornes (1965) and van Kruiningen (2002) differ by ~10×; this is a
  methodological difference (whole intestine + microscopic definition vs distal 200 cm +
  macroscopic), not a contradiction, but the game should pick one and say which.
- A few DOIs above are reconstructed rather than returned directly by the PubMed tool
  (van Kruiningen 2002, Lorenz 2003, Lochner 2011, Teillaud 2024, Sato 2023 and others came via
  Consensus, which returns Consensus paper URLs rather than DOIs). Where a DOI is uncertain I have
  given the PubMed ID instead. Titles and findings are quoted accurately from the retrieved
  records either way.

*Sources were retrieved from PubMed (metadata and full text) and from Consensus (semantic search
over Semantic Scholar / OpenAlex / PubMed). Per PubMed's terms of use, information drawn from
PubMed is attributed as such and DOI links are provided.*
