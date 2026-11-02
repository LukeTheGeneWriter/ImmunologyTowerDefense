# How the immune system learns about *Giardia duodenalis* — the antigen route

Focused follow-up to `docs/research/pathogens_protozoa.md` §3.2 and its correction
#8 ("§1c's 'debris is the input to adaptive immunity' has a hard counter, and it
may be a softlock"). That pass established that *Giardia* is non-invasive,
non-cytolytic, produces no debris and no inflammation, degrades the recruitment
chemokines and IgA at source, and that anti-VSP antibody is non-cytotoxic and
triggers the antigenic switch instead.

**The Director's question — "figure out how the body learns about giardia" — has
a real answer, and it is not a host cell dying.** The answer is that the parasite
*posts its own antigen into the lumen as particulate, protease-resistant,
TLR4-agonist membrane vesicles*, and that this material is sufficient to prime
both systemic IgG and secretory IgA when delivered into the gut lumen. The
strongest single result is that the shedding event the previous pass already
recorded — antibody-triggered VSP microvesicle release [3] — **is itself the
antigen-delivery event.** The feedback loop the pass found is not only a cost; it
is the learning mechanism.

Every claim below was verified in this session against PubMed or Consensus.
Attribution: substantially based on articles retrieved from **PubMed**, with
DOIs in §10. Mouse/human/other system is labelled on every finding. Claims I
could not source are marked **UNVERIFIED** rather than filled in.

---

## 1. The ranked routes

Ranked on *evidence that this route carries **Giardia** antigen to an APC*, not
on how well-known the route is.

| # | Route | Shown for *Giardia*? | System | Strength | Maps onto built mechanics? |
|---|---|---|---|---|---|
| **1** | **Parasite-shed extracellular vesicles / VSP microvesicles** — the trophozoite sheds its own surface antigen as EVs; the EVs are captured by human immature DCs and mature them; the cargo is VSPs + giardins + tubulin + EF-1α; antibody binding *massively increases* the shedding | **Yes, directly** | *G. intestinalis* WB/GS EVs + **human** monocyte-derived DCs [1][2]; antibody-triggered VSP microvesicle release *in vitro and in animal infections* [3]; EV uptake by **human** Caco-2 [5] | **Strong in vitro / ex vivo; the in-lumen→APC step in a live animal is UNVERIFIED** | **Yes — reuses the DC shuttle with a new cargo source.** No new carrier, no new destination |
| **2** | **Particulate VSP-bearing material crossing the follicle-associated epithelium (M cells / Peyer's patch)** | **Partly** — oral delivery of VSP-coated particles primes protective immunity in mice [4]; *Giardia* antigen-specific Peyer's-patch lymphocyte proliferation tracks clearance [10]; IL-17A is (slightly) raised in PP during infection [11]. **No study localises *Giardia* antigen to an M cell** | Mouse (oral VLP; *G. muris* PP) [4][10][11]; M-cell particulate transcytosis itself is generic [14] | **Moderate — the capability is proven, the localisation is not** | **Yes — the M-cell sampler is built (§1d, Sprint 19) and already parcels antigen for a cargo-less DC** |
| **3** | **Soluble antigen crossing a *Giardia*-loosened barrier** (the damage it causes feeds the response to it) | **Barrier damage: yes. Antigen flux through it: UNVERIFIED** | Barrier loss in **human** duodenal biopsies [22], **human** duodenal organoid monolayers [23], and *in vivo* in immunocompetent **and athymic** mice [21] | **Weak as an antigen route** — nobody has measured antigen translocation through the giardiasis lesion | Partially — would need a new "permeability → antigen leak" term |
| **4** | **Transepithelial dendrites from CX3CR1⁺ mononuclear phagocytes** | **No *Giardia* evidence at all** | Mouse [19]; and the subset that extends them **does not migrate to the lymph node** and is a poor T-cell stimulator [20] | **Weak, and structurally wrong for teaching** | Poorly — the game's learning step *is* migration to the node |
| **5** | **Goblet-cell-associated antigen passages (GAPs)** | **No *Giardia* evidence**; and GAPs pass *low-molecular-weight soluble* antigen, to *tolerogenic* CD103⁺ DCs | Mouse [15][16][18]; bacteria can use them, as **dissemination**, and infection *shuts them* via IL-1β [17] | **Weak, and probably teaches the wrong thing** | Would be a new mechanic, and a **tolerance** mechanic |

**Headline:** route 1 is the answer; route 2 is the vehicle the game already owns
and the one with an oral in-vivo priming result behind it; routes 3–5 should be
recorded and not built.

---

## 2. Route 1 — the parasite ships its own antigen (recommended)

### 2.1 *Giardia* sheds EVs constitutively, and the biogenesis is real

*G. intestinalis* releases both large (plasma-membrane-derived, ~180 nm class)
and small (exosome-like, ~100 nm class) vesicles; the exosome-like population
forms in peripheral vacuoles and requires the ESCRT-associated AAA⁺-ATPase
Vps4a, Rab11 and ceramide — notable because the parasite has lost most ESCRT
components and cannot make ceramide *de novo* [6]. Vesicles are also detectable
inside the excretory-secretory fraction the parasite produces while touching
**human** intestinal epithelial cells, and only ~29% of those secreted proteins
carry a signal peptide, which is why the authors invoke a vesicular route [7].
So the shedding is not an artefact of an antibody challenge; it is a baseline
behaviour of the organism, modulated upward by stimuli.

### 2.2 The shed material is immunogenic, and it matures a *human* APC

- **Human immature dendritic cells capture *Giardia* microvesicles, and this
  results in increased DC activation and allostimulatory capacity** [1]. This is
  the single most load-bearing result for the game: a professional APC takes up
  parasite-derived vesicles and becomes better at stimulating T cells, with no
  host cell having died.
- **Human monocyte-derived DCs exposed to *Giardia* EVs upregulate MHC-II, CD80
  and CD86 and drive Th1-skewed T-cell proliferation**; the same EVs activate
  SAPK/JNK, ERK1/2 and NF-κB in **mouse** macrophages (IκB-α degradation, p65
  nuclear translocation), upregulate *Il1b, Il6, Il4, Ptgs2, Nos2, Tnf*
  (log-fold 3.9–15.8), and raise iNOS protein 28–45% and nitrite 9.6–12.3-fold
  [2].
- **The cargo is the right antigen.** Proteomics of the EVs identified elongation
  factor 1-alpha, α-7.3 giardin, tubulin and **variant surface proteins (VSPs)**
  — "well-established antigens in *Giardia* infection" [2].
- **Honest limit on [2]:** its *in vivo* immunogenicity arm was **subcutaneous**
  vaccination of CD1 mice, three animals per group, days 0/14/28 (read from the
  paper's methods). It proves the cargo is immunogenic. **It does not show that
  luminal EVs reach an APC in a live gut.**

### 2.3 The antibody-triggered shed is the big one — and it is the loop

Antibodies to VSPs are **not cytotoxic**. They cluster VSPs into liquid-ordered
membrane microdomains, trigger a **massive release of microvesicles carrying the
original VSP**, and switch expression to a different VSP by a calcium-dependent
mechanism — demonstrated **both in vitro and in animal infections** [3].

Read that as a design statement rather than as evasion-only: *the moment the
player's antibody binds, the parasite ejects a cloud of its own antigen into the
lumen.* The switch is the cost; the microvesicle cloud is the reward. The
previous pass recorded only the cost.

**Caveat carried forward, unchanged:** [3] (2023) contradicts an earlier
consensus that anti-VSP antibody *is* cytotoxic. `pathogens_protozoa.md`
correction #10 already records this; nothing found this session resolves it.
Treat the non-cytotoxic/shedding model as the current best result, not as
settled.

### 2.4 The epithelium takes the vesicles up — but uptake is not transcytosis

*Giardia*-derived EVs are internalised by **human** Caco-2 cells through a
**dynamin- and clathrin-dependent** pathway, independent of caveolae and lipid
rafts; vesicles from the host–parasite *interaction* are internalised faster
still and additionally use macropinocytosis [5]. **Internalisation into an
enterocyte is not the same as delivery to a lamina-propria APC**, and no paper
found this session follows a *Giardia* EV across an epithelial layer to an APC.
That step is **UNVERIFIED**.

### 2.5 Why VSP-coated particulate is the ideal luminal antigen — and the oral proof

This is the result that makes route 1 plus route 2 into a mechanism rather than a
hope:

- **VSPs survive the gut.** Purified VSP ectodomains resist extreme pH and
  proteolysis by trypsin and by mouse stomach and intestinal extracts, to the
  same degree as native VSPs on live trophozoites, while control antigens
  (influenza HA, GRP78/BiP) are destroyed. Reducing agents or metal chelators
  make VSPs protease-sensitive and metal re-addition restores resistance — the
  stability comes from multiple CXXC motifs cross-linking via metals and
  disulfides [4].
- **VSPs are a PAMP.** VSPs signal through **TLR-4** (strongly, dose-dependently)
  and weakly through TLR-2, in **human** TLR reporter lines and with mouse
  receptors; they upregulate CD40 and CD86 on **mouse** bone-marrow DCs, and this
  activation is **abolished in TLR-4-knockout** BMDCs. The authors propose the
  multiple CXXC motifs as a novel PAMP [4].
- **Oral delivery of VSP-coated particles primes mucosal and systemic immunity.**
  Virus-like particles decorated with VSP and administered **orally** to mice
  produced serum anti-HA IgG1 and IgG2, **secretory IgA in faeces and BAL**,
  HA-specific IFN-γ-producing T cells, and protection against influenza
  challenge. **Plain VLPs did the same thing in none of those readouts**, and the
  response persisted in serum to 125 days. Pre-existing anti-VSP antibody did not
  blunt it [4].

So: particulate, VSP-bearing material placed in the gut lumen is (i) protease-
proof, (ii) self-adjuvanting through TLR4, and (iii) demonstrably capable of
priming secretory IgA and T-cell responses in a live mouse without any infection
and without any host-cell death. That is exactly the shape the game needs.

**What is still UNVERIFIED:** [4] does not identify *which* epithelial route
carries the oral VLPs in. Its discussion names DCs, M cells and epithelial PRRs
as candidates for "antigen transcytosis and presentation"; it ran no Peyer's-
patch or M-cell experiment. The M-cell attribution is therefore plausible,
sourced as a hypothesis by the authors, and **not demonstrated**.

### 2.6 The same packet is also a suppressor — the two-edged cargo

A *Giardia* VSP (VSP<sup>AS7</sup>), present both on the trophozoite surface and
**on its EVs**, suppresses *Giardia*-induced NF-κB and ERK/MAPK activation, cuts
IL-6, IL-12p40 and TNF-α, and inhibits NLRP3-dependent pyroptosis (less GSDMD
cleavage, less caspase-1 activation, less IL-1β and LDH release) in **mouse**
macrophages, by binding mouse NLRP3 directly; the suppression is largely lost in
NLRP3-deficient cells [8].

Note this sits in tension with [2], where whole EVs were *pro*-inflammatory in
mouse macrophages. The honest reading is that the EV is a mixed packet: antigenic
cargo plus at least one surface protein that dampens the inflammasome. For the
game that is a feature, not a problem — see §8.

### 2.7 Side-effects of the EV route worth knowing (not antigen routes)

- *Giardia* EVs carry a heat-stable, RNase-sensitive small-RNA cargo that turns
  commensal *E. coli* and *Enterobacter cloacae* into pathobionts — more motile,
  less biofilm, more adhesion/invasion of small-intestinal epithelial cells —
  **at sites with no trophozoites on them** [9]. An "acts at a distance" lever.
- EVs transfer drug-resistance phenotypes between *Giardia* clones in a
  genotype-specific way [58], and carry Giardiavirus between trophozoites, with
  GLV-infected *Giardia* causing *less* damage in mice [57].

---

## 3. Route 2 — M cells and the Peyer's patch

**What is shown for *Giardia*:**

- **Peyer's-patch lymphocytes from *G. muris*-infected mice develop specific
  proliferative responses to *G. muris* antigen, and that proliferation
  correlates with clearance of the infection** [10]. Mouse, 1990. This is direct
  evidence that *Giardia* antigen is being presented in the Peyer's patch during
  a real infection — it does not say by what route it got there.
- **IL-17A mRNA during *G. muris* infection is strongly up in the small intestine,
  only slightly elevated in the Peyer's patches, and unchanged in the mesenteric
  lymph nodes**; the dominant producer is the CD4⁺ Th cell in the **lamina
  propria**, with a possible supporting role for MHC-II⁺CD11c⁺CD11b⁺CD64⁺F4/80⁺
  macrophage-like APCs. In T-cell-deficient mice both IL-17A and clearance are
  severely impaired [11]. Mouse, 2025.
  **Design consequence: the effector response is a lamina-propria event, not a
  Peyer's-patch event.** The patch is where priming is demonstrable [10]; the
  gut wall is where the output happens [11].
- In **gerbils** — the model closest to human disease — infection produces a
  biphasic cytokine profile (early IFN-γ/IL-1β/IL-6/TNF/IL-17/IL-4, then a shift
  to IL-5) measured in Peyer's patches, mesenteric lymph nodes and spleen, with
  intestinal inflammatory changes; an **oral** whole-VSP-repertoire vaccine
  instead gave high IL-17, IL-6, IL-4 and IL-5 with **no obvious inflammation**,
  and both infected and immunised animals made intestinal S-IgA and serum IgG
  against VSPs [12]. Note the gerbil inflammation finding sits awkwardly against
  the "no inflammation" line in the baseline pass; label it gerbil.
- **Athymic nude mice fail to clear *G. muris*** and are especially depleted of
  L3T4⁺ (CD4) cells; clearance in competent BALB/c correlates with a Peyer's-
  patch L3T4⁺/Ly-2⁺ ratio ≥ 2 [13]. Mouse, 1987.

**What is generic, not *Giardia*-specific:** M cells in the follicle-associated
epithelium "are able to translocate diverse particulates without digesting
them", act as pathways for microorganism invasion, and transcytose intact
particles — which is why they are the standard target for oral vaccine delivery
[14]. The *capability* to carry a 100–200 nm VSP-bearing vesicle is therefore
established in principle.

**What is missing:** no study found this session localises *Giardia* antigen, or
a *Giardia* EV, to an M cell or to the subepithelial dome. **UNVERIFIED.**

---

## 4. Route 3 — soluble antigen through the barrier *Giardia* breaks

The damage is well established, in humans:

- **Human duodenal biopsies, 13 chronic giardiasis patients:** mucosal surface
  area fell to 75% of control, yet epithelial resistance *fell* (19 vs 25 Ω·cm²),
  with **reduced claudin-1 and increased epithelial apoptosis**; Na⁺-glucose
  symport was impaired and electrogenic anion secretion activated. Diarrhoea in
  human chronic giardiasis has leak-flux, malabsorptive and secretory components
  [22].
- **Human duodenal organoid-derived monolayers:** time- and parasite-load-
  dependent collapse of the barrier by leak flux; chloride secretion changes
  come **first**, tight-junction composition/localisation/structure changes come
  **later**; the pathway is cAMP/PKA–CREB [23].
- ***In vivo*, mouse:** *G. muris* raises small-intestinal (not gastric, not
  colonic) permeability, correlating with trophozoite colonisation and resolving
  on clearance; **it does this in athymic mice as well as immunocompetent ones**,
  so the permeability defect is T-cell-independent; mechanistically MLCK-dependent
  with F-actin and ZO-1 reorganisation in human duodenal monolayers [21]. This is
  an important corrective: the *barrier* lesion is not immune-mediated even
  though the *brush-border* lesion is (CD8, per the baseline pass).
- TNF-α-deficient and anti-TNF-treated mice also show the permeability rise, so
  it is not TNF-driven either [33].

**But:** no study found this session measures *antigen* (or EV) flux across the
giardiasis-loosened barrier, or shows that the permeability defect feeds
priming. The route is **UNVERIFIED as an antigen route**, and the baseline fact
that *Giardia* proteases cleave the chemokines and IgA at source argues that
whatever leaks in is landing in a deliberately quietened lamina propria.

---

## 5. Route 4 — transepithelial dendrites (recommend against)

Mouse lamina-propria myeloid DCs depend on CX3CR1 to form transepithelial
dendrites that directly sample luminal antigen, and CX3CR1 also controls
clearance of entero-invasive pathogens [19]. No *Giardia* evidence whatsoever.

More importantly, the route is structurally wrong for the game's learning step:
**CD103⁺ DCs migrate in lymph to the mesenteric lymph nodes and serve classical
DC functions; CX3CR1⁺ cells are a non-migratory, gut-resident population with
slow turnover and poor T-cell-stimulatory capacity in vitro and in vivo, and are
less efficient at generating retinoic acid** [20]. The cell that reaches into the
lumen is not the cell that walks to the node. §5a's whole mechanic is the walk.

---

## 6. Route 5 — goblet-cell-associated antigen passages (recommend against)

GAPs are real and well characterised, in mouse:

- In the steady state, small-intestinal goblet cells form passages that deliver
  **low-molecular-weight soluble** luminal antigen to **CD103⁺CX3CR1⁻**
  lamina-propria DCs — the tolerogenic subset [15].
- Formation is driven by acetylcholine via muscarinic receptor 4; in the colon,
  MyD88-dependent microbial sensing by goblet cells *gates* it, and forcing
  colonic GAPs open recruits neutrophils and APCs and produces inflammatory
  cytokines [16].
- **GAPs are actively shut during infection, by IL-1β.** *Salmonella*
  translocation to the mesenteric lymph node required goblet cells and correlated
  with GAPs; overriding the suppression increased dissemination and worsened
  outcomes independent of luminal burden [17].
- Their function is **tolerance**: they maintain pre-existing lamina-propria
  Tregs, imprint tolerogenic properties on LP-DCs, help LP macrophages make
  IL-10, and tolerance to dietary antigen is impaired without them [18].

Two reasons to leave this out. First, no protozoan evidence: **UNVERIFIED for
*Giardia*, and for any protozoan.** Second — and this is the interesting part —
GAP closure is triggered by **IL-1β**, i.e. by inflammation, and *Giardia*
notoriously causes little or none. The sourced inference (labelled as inference,
not as a finding) is that GAPs would likely stay **open** during giardiasis and
keep delivering soluble antigen to tolerogenic DCs. If the game modelled GAPs
faithfully, a *Giardia* round would generate **tolerance**, not knowledge. That
is a great piece of flavour and a terrible mechanic to build as the answer.

---

## 7. What is actually required for clearance

Mouse unless stated. The field's own summary is that the mechanisms are
**redundant**, which is why single knockouts so often look unimpressive [30].

### 7.1 Necessary (single-intervention failure to clear)

| Component | Finding | System |
|---|---|---|
| **CD4⁺ / αβ T cells** | **The clearest requirement in the literature.** Anti-L3T4 (CD4)-depleted BALB/c mice do **not** clear *G. muris*; anti-Ly-2 (CD8)-depleted mice clear at a normal rate [24]. T-cell-deficient and scid mice fail to control *G. lamblia*; anti-CD4 prevents elimination in wild-type **and in B-cell-deficient** mice; **αβ-TCR T cells are required, γδ-TCR T cells are not**; neither Th1 nor Th2 is absolutely required [25]. Athymic nudes fail [13]. T-cell-deficient mice lose both IL-17A and clearance [11] | Mouse (*G. muris*, *G. lamblia* GS/M) |
| **IL-17 signalling** | In IL-17RA-knockout mice the entire *Giardia*-induced transcriptional programme largely fails to fire: angiogenin 4, α- and β-defensins, mannose-binding lectin 2, and **pIgR — the receptor that exports IgA into the lumen — are all IL-17RA-dependent** [27]. IL-17A itself comes predominantly from lamina-propria CD4⁺ Th cells [11] | Mouse (*G. muris*) |
| **IgA** | **IgA-deficient mice cannot eradicate either *G. muris* or *G. lamblia*** — IgA is required for eradication. Secreted-IgM-deficient mice clear normally. B-cell-deficient mice are *worse* than IgA-deficient ones, so there are also B-cell-dependent/IgA-independent defences, and B-cell-deficient mice retain some residual control [26] | Mouse |
| **Mast cells** | c-kit W/Wv mice make no parasite-specific IgA, mount no mast cell response and do not eliminate the infection; anti-c-kit-treated C57BL/6 have **normal IgA**, no mast cells, reduced small-intestinal IL-6 mRNA, and fail to control within 10 days. IL-9-deficient mice have a reduced mast cell response and still clear within 2 weeks. IL-6-deficient mice have *enhanced* mast cell responses and still fail — so mast-cell-derived IL-6 looks like the operative output [29] | Mouse (*G. lamblia*) |
| **IL-6, via the dendritic cell** | IL-6 is required for control; bone-marrow chimeras localise the requirement to radiation-sensitive bone-marrow-derived cells, not T cells, and **transferring wild-type DCs into IL-6-deficient recipients restores the response** [34] | Mouse |
| **TNF-α** | Neutralised or genetically deficient mice carry much higher burdens for the first 2 weeks, with **unchanged** anti-parasite IgA, mast cell responses and IL-4/IL-6 — so TNF acts through something else [33] | Mouse |

### 7.2 Contributory / redundant (single interventions are weak, combinations are not)

- **Nitric oxide and α-defensins are redundant with each other.** Microarray of
  the mouse small intestine at day 10 found antibody genes the most abundant
  induced transcripts and **mast cell proteases the most highly induced**.
  *Mmp7*-deficient mice (no mature α-defensins) showed only a small increase in
  parasite numbers; **combining *Mmp7* deletion with iNOS deletion, or blocking
  iNOS pharmacologically in *Mmp7*-null mice, produced significant increases**
  [30]. This is the cleanest demonstration in the literature that "present" and
  "necessary" are different questions here.
- **Nitric oxide is cytostatic, not cytotoxic.** NO donors inhibit giardial
  growth without killing trophozoites, and also inhibit encystation and
  excystation. *Giardia* evades it by **consuming arginine**, the substrate
  epithelial NOS needs, in models of **human** intestinal epithelium [31].
- **Complement, via the lectin pathway.** Mannose-binding-lectin-2-deficient and
  C3aR-deficient mice show delayed clearance and reduced mast cell recruitment to
  the intestinal submucosa; C3aR-deficient mice make normal anti-parasite IgA but
  have impaired ex vivo T-cell recall [32]. Consistent with the Director's
  2026-10-25 move of complement out of the knowledge ladder and into a body
  upgrade.
- **The microbiota is permissive.** Antibiotic depletion lengthened infection and
  raised burden for both *G. muris* and *G. duodenalis* **even though IL-17A and
  MBL2 were still upregulated**; what fell was luminal IgA (via its transporter
  and fewer IgA⁺ B cells at the Peyer's patches) and intestinal motility [28].

### 7.3 Immunity to reinfection does not need antibody

Mice primed and drug-cleared, then rechallenged at day 21 or day 60, carried
~50-fold fewer parasites at day 5. **Resistance was intact in B-cell-deficient
µMT mice** and was **not strain-specific**; no measured response (IgA, mast cell,
T cell) correlated with protection [35]. So: antibody is required to *eradicate a
primary* infection [26] but not to *resist a secondary* one [35].

### 7.4 Human

Thin, and almost entirely correlative. The review position is that NO, mast cells
and DCs are the first line, that IL-6 and IL-17 matter, and that IgA and CD4⁺ T
cells are fundamental, with CD4 acting both by driving B cells and through a
**B-cell-independent, probably Th17** route [36]. In **human children** in
Brazil (n = 31, 19 *Giardia*-positive), plasma I-FABP was elevated in infected
children, IL-8 was *decreased*, IL-17 showed a non-significant upward trend, and
I-FABP correlated positively with IL-17 and TNF [41]. In **human** adults,
longitudinal anti-VSP serology after natural infection is described in §9.

**No human intervention or deficiency study establishes necessity for any single
component. UNVERIFIED for humans.** Every "required" above is a mouse statement.

---

## 8. Is there any host-cell death? — resolving the apoptosis question

**Verdict: yes, *Giardia* causes real enterocyte apoptosis. It is caspase-3-
dependent, it is documented in human duodenal tissue in vivo, and it is
conditional on strain, assemblage and local metabolic conditions — which is
exactly why the in vitro literature looks contradictory.**

### 8.1 The evidence that it is real

- **Human duodenal biopsies, in vivo:** 13 patients with chronic giardiasis
  showed **increased epithelial apoptosis** by TUNEL alongside reduced claudin-1
  and a fall in epithelial resistance [22]. This is the strongest single datum
  and it is human and in vivo.
- **Human children:** plasma I-FABP — a marker of enterocyte damage — was
  elevated in *Giardia*-positive children relative to uninfected controls [41].
  Enterocyte damage is happening in human infection, though I-FABP does not
  distinguish apoptosis from other loss.
- **Non-transformed human duodenal epithelial monolayers:** *G. lamblia* strains
  **NF and S2 induced caspase-3-dependent apoptosis; strains WB and PB did
  not**. A caspase-3 inhibitor blocked the apoptosis *and* the ZO-1 disruption
  *and* the permeability rise — i.e. apoptosis is upstream of the barrier defect
  in this system [37].
- **HCT-8 human epithelial cells, assemblage comparison:** trophozoites disrupt
  tight junctions and induce varying degrees of enterocyte apoptosis depending on
  the infecting assemblage (A, B, E); all disruptions were caspase-3-dependent,
  were more pronounced with a non-host-specific assemblage, and **mixed-assemblage
  infections enhanced both** [39].
- **The review literature treats chronic-phase apoptosis as established:** acute
  infection resolves in 2–3 weeks in healthy hosts, whereas the chronically
  infected host shows villus and crypt atrophy, **enterocyte apoptosis**, and
  severe barrier disruption [48].

### 8.2 The evidence that it is not unconditional

- **A direct strain contradiction.** [37] reports WB does **not** induce
  apoptosis in human duodenal monolayers. [38] reports that *G. intestinalis*
  **WB clone C6 does** induce apoptosis in HCT-8 cells, with DNA fragmentation,
  active caspase-3, PARP cleavage, Bcl-2 down and Bax up, via both intrinsic and
  extrinsic pathways. Different cell line (primary-like duodenal monolayer vs an
  ileocaecal adenocarcinoma line), different readouts, same nominal strain.
  **This disagreement is unresolved and should be recorded as unresolved.**
- **It is metabolically gated.** In SGLT-1-transfected Caco-2 cells, *Giardia*
  products induced caspase-3 activation and DNA fragmentation in **low** (5 mM)
  glucose and these were **abolished in high (25 mM) glucose**; a soluble
  proteolytic *Giardia* fraction upregulated SGLT-1-mediated glucose uptake
  (increased Vmax, unchanged Km), and the glucose-competitive inhibitor
  phloridzin removed the protection [40]. So the same parasite kills or does not
  kill the same cell line depending on luminal glucose.
- **The barrier defect does not require apoptosis.** In the human
  organoid-derived duodenal model, barrier breakdown ran through ion transport
  first and tight-junction reorganisation second, via cAMP/PKA–CREB [23]; and
  *in vivo* the permeability rise occurs in athymic mice and in TNF-deficient
  mice [21][33]. Apoptosis contributes [37] but is not the only path in.
- **Rate: no clean number.** Nobody found this session reports a per-cell or
  per-hour apoptotic rate for *Giardia* in vivo. [22] reports an elevated
  apoptotic ratio in human biopsies without a usable rate. **Rate is
  UNVERIFIED.**

### 8.3 What this means for the game

The baseline pass's formulation stands and should be kept: ***Giardia* has no
cell death and no inflammation on the round timescale, and a strain-selectable
apoptosis trait.** What this pass adds is that the trait is better modelled as
**conditional** than as per-strain-binary — strain × assemblage × local
nutrient state — and that the honest apoptosis trickle is a *chronic*-phase
phenomenon [22][48], i.e. a many-round effect, not a wave effect.

**It is not a fix for the softlock.** A trickle of conditional, strain-dependent,
chronic-phase apoptosis cannot be the route by which a player learns about
*Giardia* inside a 30-second round. Route 1 can.

---

## 9. Does *Giardia* suppress antigen acquisition? — yes, extensively, and selectively

Beyond the chemokine and IgA proteolysis the baseline pass already found:

### 9.1 It cripples DC IL-12 while sparing — or boosting — IL-10

- **Mouse** bone-marrow DCs with live parasites or extract raise CD40, raise
  CD80/CD86 less than LPS does, secrete small amounts of IL-6 and TNF-α, and
  **no detectable IL-10 or IL-12 at all**. Co-incubated with TLR ligands,
  *Giardia* extract **increases IL-10 and reduces IL-12**, and lowers MHC-II,
  CD80 and CD86 versus TLR ligand alone. The dominant mechanism is **PI3K**, not
  ERK and not IL-10 receptor signalling [42].
- **Human** DCs: with LPS, *G. duodenalis* lysate reduced IL-12/23p40
  (p = 0.002), IL-12p70 (p = 0.011) and IL-23 (p = 0.004) and **raised IL-10**
  (p = 0.006), while lowering CD25, CD83, CD86 and HLA-DR. **But with TLR2/TLR1
  or TLR2/TLR6 ligands the same cells were more activated than controls** — more
  IL-12/23p40, IL-23 and IL-10 and higher CD25/CD83/CD86 [43]. The suppression is
  **context-specific, not global**, and human and mouse differ.
- **Human** monocyte-derived DCs: *Giardia* products reduced LPS-induced IL-6,
  IL-12p70 and TNF-α and raised IL-10; and *G. duodenalis* **trophozoites induced
  rapid apoptosis in the DCs themselves**, which helminth antigens did not [44].
- **Mouse** peritoneal macrophages: *Giardia* plus LPS raises IL-10 with TNF-α
  unchanged, and the effect requires **macrophage galactose-binding lectin
  (MGL1)** [47].
- Counterpoint worth labelling: in **cattle**, *G. duodenalis* and its
  excretory/secretory products produced only partial MoDC maturation yet the cells
  remained functionally active and induced proliferation of CD4⁺, CD8⁺ and
  γδ T cells; the authors found **no** regulatory or inhibitory effect, though they
  did see a dose-dependent fall in ovalbumin uptake [50]. Species matters.

### 9.2 Arginine consumption is a verified, double-barrelled mechanism

- Against the epithelium: *Giardia* **consumes arginine**, the substrate
  epithelial NOS needs, suppressing NO production in **human** intestinal
  epithelial models [31].
- Against the APC: **giardial arginine deiminase (ADI)**, acting on LPS-stimulated
  **human** monocyte-derived DCs, increased TNF-α, decreased IL-10 and IL-12p40,
  reduced upregulation of **CD83 and CD86** (the molecules that do cell–cell
  interaction), and reduced S6 kinase phosphorylation, implicating mTOR. The
  effects were due to **both** arginine depletion **and** the reaction products,
  particularly **ammonium ions** — and NH₄⁺ (from deiminases) and urea (from
  arginases) had distinct immunomodulatory activities [45].
- ADI additionally acts directly on **human** TLR2 and TLR4 signalling in a way
  that depends on maintaining its 3D structure, with specific effects on
  infant-like DCs [46].

### 9.3 Design reading

There is no sourced evidence that *Giardia* blocks antigen **acquisition**
(uptake, transport). What it does is degrade the recruitment signal, degrade the
effector antibody, and **blunt the APC's ability to turn acquired antigen into a
polarising signal** — IL-12 down, IL-10 up, CD83/CD86 down. In game terms: the
antigen arrives; the *presentation* is worth less per encounter. That is a
knowledge-rate debuff on the DC, not a cargo block.

---

## 10. Timing

**Confirmed.** "Acute infection develops over a period of three weeks, peaking at
eight days post infection. Generally, healthy hosts clear the infection within
2–3 weeks, whereas the occasional chronically infected host shows signs of
villus and crypt atrophy, enterocyte apoptosis, and ultimately severe disruption
of epithelial barrier function" [48]. Human-facing review statement; the baseline
pass's day-8 peak and 2–3-week clearance both stand.

**Kinetics of the specific response:**

- **Mouse, parasite burden:** the vast majority of *G. lamblia* parasites are
  eliminated **between 1 and 2 weeks** post-infection, in wild-type and in
  B-cell-deficient mice alike [25]. TNF's contribution is specifically to the
  **first 2 weeks** [33]. Mast-cell-dependent control is measured as failure to
  control **within 10 days** [29].
- **Mouse, specific response:** antigen-specific Peyer's-patch lymphocyte
  proliferation appears during infection and tracks clearance [10]; the IL-17RA-
  dependent transcriptional programme was sampled at **3 weeks** post-infection
  [27]; the effector transcriptome (antibody genes, mast cell proteases) was
  sampled at **day 10** [30].
- **Mouse, memory:** rechallenge at day 21 **or** day 60 after a drug-cleared
  primary gave ~50-fold fewer parasites by day 5 of the challenge [35].
- **Human, antibody:** in 30 *Giardia*-positive patients (mainly returning
  travellers) sampled before treatment and at 6 weeks, 6 months and 12 months
  after clearance, serum **anti-VSP5 and anti-VSP3 IgA fell rapidly** after
  treatment but stayed well above presumed-unexposed controls at 6 and 12 months;
  **IgG/M fell more gradually** and stayed significantly elevated at all points
  except anti-VSP3 at 12 months. Assemblage A and B responses did not differ
  (considerable cross-reactivity). **Chronic infection (> 8 weeks) was associated
  with significantly *lower* anti-VSP5 IgG/M** [49]. Earlier human serology found
  81% of 59 symptomatic patients had circulating anti-*Giardia* IgG, detectable
  from 2 weeks to 15 months after therapy [59].
- **Onset in humans:** no study found this session dates the *first appearance*
  of a specific anti-*Giardia* response relative to inoculation in humans. The
  earliest human timepoint in [49] is "before treatment", which is not an
  inoculation-anchored clock. **Time-to-first-specific-response in humans is
  UNVERIFIED.**

**Game-usable clock (mouse-anchored, human-consistent):** antigen is being
presented in gut-associated lymphoid tissue well before day 10; effector output
(IgA, mast cell proteases, defensins, NO) is at full tilt around day 10; burden
collapses between weeks 1 and 2; the human equivalent peaks at day 8 and clears
by weeks 2–3. In a game where one round is a day or two, the specific response
should become usable **mid-round-sequence, not within one round.**

---

## 11. *Cryptosporidium* — does the same route explain it?

Short answer: **yes, and better than the debris route does.**

**The debris problem is confirmed and is worse than the baseline pass stated.**
*C. parvum*-infected cells are **less** likely to be apoptotic than bystanders in
the same tissue, and infection confers resistance to chemically induced apoptosis
**to the infected cell specifically and not to its neighbours** [53]. Only ~20%
of infected cells apoptose within 48 h; the majority are actively rescued, via
upregulation of **survivin** (knocking down survivin, but not c-IAP1, c-IAP2 or
XIAP, restored caspase-3/7 activity and apoptosis and **reduced parasite
load**) [54]. Earlier work had the same shape: moderate apoptosis peaking at 24 h,
*increased* when NF-κB is inhibited, and attenuation of apoptosis induced by
strongly pro-apoptotic agents [55].

**But the EV route is there, and it is the same route.**

- **Sporozoite-derived EVs.** Excysting *C. parvum* sporozoites release two
  populations: large EVs (mode 181 nm, formed by budding of the sporozoite outer
  membrane) and small EVs (mode 105 nm). **CpRom1 — a rhomboid protease
  recognised by the host immune system — is on the LEV surface**, and **GP60, the
  immunodominant parasite antigen**, is among the most abundant proteins in both
  populations. 12 of the identified proteins are *Cryptosporidium*-specific [51].
- A 2025 review states that *Cryptosporidium*-infected epithelial cells release
  EVs enriched in GP60 and CpRom1 that stimulate host immune responses via the
  **TLR4/IKK** pathway [52] — note this is a review statement, so treat the
  TLR4/IKK attribution as secondary sourcing, not as verified primary.
- **The APC end is directly shown, in human cells.** Primary **human**
  monocyte-derived DCs exposed to *C. parvum* oocysts and sporozoites upregulated
  HLA-DR, CD1a, CD83, CD40, CD86, CD11b and CD58, produced IL-6 and IL-8,
  increased adhesion and mobility, and showed boosted (time-limited) phagocytosis
  of oocysts and sporozoites — "all hallmarks of successful maturation" [56].

**One important difference from *Giardia*.** *Cryptosporidium* does produce
substantial epithelial **cell loss** — it raises both proliferation and loss,
"speeding up the villus elevator", with single-cell RNA-seq showing a gain of
young and a loss of mature cells. That loss is simply **not apoptosis of infected
cells** [53]; extrusion/shedding is the implied route. So for *Cryptosporidium*
the game has two honest options: feed the debris route from **extrusion rather
than apoptosis**, or feed it from the EV/particulate route. The first is the
cheaper fix and is the one that also repairs the §1d "PRR → immunogenic
apoptosis" upgrade silently doing nothing (see corrections).

---

## 12. Recommendation — one mechanic

### 12.1 The mechanic: **shed antigen**

> **A lumen-dwelling organism that kills nothing still leaks antigen. It sheds
> *shed antigen* — a small, drifting parcel of its own surface coat — and the
> harder the player hits it with antibody, the more it sheds.**

Concretely, in the vocabulary the docs already use:

1. **What produces the antigen.** A *Giardia* trophozoite adhering at the wall
   emits a **shed-antigen parcel** on a slow baseline timer (low rate), and emits
   a **burst** of parcels the moment it is coated by antibody or sIgA. Sourced:
   constitutive EV release [1][6][7]; antibody-triggered massive VSP microvesicle
   release [3]. The burst is the headline: the ~20% and ~70% rungs, which the
   baseline pass found "backfire twice", now also *pay for themselves*.
2. **What carries it.** **Reuse two carriers that already exist, no new ones.**
   - A parcel drifting over the **Peyer's-patch dome** is picked up by the
     already-built **M-cell sampler** (§1d, Sprint 19) and waits in the dome (up
     to 3 parcels) for a cargo-less patrolling DC — exactly the built flow.
     Sourced as a capability: M cells translocate intact particulates [14], and
     oral VSP-coated particles prime serum IgG, faecal and BAL sIgA and IFN-γ
     T cells in mice while bare particles do not [4].
   - A parcel that reaches the **tissue band** can be sampled directly by a
     patrolling **DC** as cargo, alongside debris. Sourced: human immature DCs
     capture *Giardia* microvesicles and are activated and allostimulatory
     afterwards [1]; human Mo-DCs mature on *Giardia* EVs and drive Th1-skewed
     proliferation [2]. **§1c already says antigen presentation is "a spectrum,
     not a DC monopoly" — this is the same spectrum extended one step outward,
     from "debris is not the only thing a DC can carry" to "a dead cell is not
     the only thing that makes cargo."**
3. **What the player sees.** A small pale mote peeling off a trophozoite and
   drifting down-lumen; a visible puff of several motes when a coated organism is
   hit; motes queueing at the patch dome; a DC turning from empty to carrying
   after crossing one. The read the player should get is *"it is leaving pieces of
   itself behind, and I can make it leave more."*
4. **What the player can do to make it happen faster.** Three levers, all
   existing purchases:
   - **Buy the M-cell upgrade** (already a shop upgrade, already level-scaled).
     This is the dedicated answer to a *Giardia* round and makes the Sprint 19
     purchase finally load-bearing.
   - **Buy antibody** (~20%) or **sIgA** (~70%). Coating an organism *increases*
     its shed rate. This converts the parasite's own counter-mechanic into the
     player's accelerator while keeping the antigenic-switch cost intact — pay a
     knowledge discount, receive a shed burst.
   - **Field more DCs.** Unchanged from §5a.
5. **The balancing tension, sourced.** Give shed antigen a **lower
   knowledge-per-encounter** value than debris-derived cargo, because the packet
   is a mixed one: *Giardia* EVs and VSP suppress NF-κB/ERK and NLRP3 and lower
   DC IL-12 while raising IL-10 and dropping CD83/CD86 [8][42][43][45]. So a
   *Giardia* round teaches, but slowly and expensively — which is both the correct
   biology and the right feel for an organism whose whole identity is attacking
   the information systems.

### 12.2 Is this a new mechanic or a reuse?

**Mostly a reuse, with one genuinely new object.** The carrier (M-cell sampler),
the destination (dome parcel → DC cargo → lymph node → encounter), the knowledge
ledger and the coating state all exist. The new object is a **lumen-side,
pathogen-emitted antigen entity** — the first antigen source in the game that is
not a dead host cell. §1c's debris already is a board-state entity with a
species tag and a pile size, so the shed parcel can plausibly be the same type
with a different spawn rule and a shorter life, rather than a new system.

What this does change, and the Director should decide it explicitly: **§1c's
"debris is the input to adaptive immunity" becomes "dead host cells are *one*
input to adaptive immunity."** That is the sentence to amend.

### 12.3 What I recommend against

- **GAPs.** No protozoan evidence; passes soluble antigen to tolerogenic DCs
  [15]; shut down by IL-1β during infection [17]; its established job is oral
  tolerance [18]. Modelled faithfully in a no-inflammation infection it would
  generate tolerance rather than knowledge. Lovely flavour; wrong mechanic.
- **Transepithelial dendrites.** No *Giardia* evidence, and the cell that reaches
  into the lumen is a non-migratory, poorly stimulatory population that does not
  travel to the node [20] — structurally incompatible with §5a's walk.
- **"Barrier damage creates its own antigen route."** Appealing, and the damage is
  real and human-verified [21][22][23], but antigen flux through the giardiasis
  lesion is unmeasured. Building it would be inventing the mechanism, which the
  brief forbids.
- **Leaning on enterocyte apoptosis to break the softlock.** Real [22][37][39],
  but conditional, strain- and nutrient-dependent [37][40], disputed on strain
  WB [37] vs [38], and a chronic-phase phenomenon [22][48]. Too thin and too slow
  to be the learning route in a round.

---

## 13. Corrections to standing design

1. **§1c's "Debris is the input to adaptive immunity" is too strong as written,
   and *Giardia* is the counterexample that forces the edit.** The sentence
   should become "**dead host cells are one input** to adaptive immunity." A
   non-invasive, non-cytolytic lumen dweller sheds its own surface antigen
   constitutively [1][6][7] and in a burst when antibody binds [3]; that material
   matures human DCs and drives antigen-specific T-cell proliferation [1][2]; and
   particulate VSP-bearing material delivered into the gut lumen primes serum IgG,
   secretory IgA and IFN-γ T cells in mice [4]. This supersedes
   `pathogens_protozoa.md` correction #8, which flagged the softlock and left it
   for a Director decision — the biology supplies the answer and it needs no new
   carrier.

2. **§1d's M-cell subsection understates what the patch is for, and its
   "refine later" list is missing the most valuable item.** The subsection says
   the patch "is the first route that teaches from the *lumen side*, before
   damage." That is right, and the *Giardia* case makes it the **only** route for
   an entire class of organism, not an earlier alternative for organisms that also
   produce debris. Worth adding to the subsection explicitly, because it changes
   the upgrade from "nice, buys you tempo" to "required against a whole enemy
   class."

3. **§1d's "PRR → immunogenic apoptosis" host-cell upgrade is silently inert
   against two of the three ranked protozoa, and the reason differs for each.**
   `pathogens_protozoa.md` correction #4 already recorded this for
   *Cryptosporidium* (apoptosis-resistant infected cells). This pass confirms it
   with stronger evidence — infected cells are *less* apoptotic than bystanders
   and the resistance is cell-specific, not tissue-wide [53], mediated by
   survivin [54] — **and adds that for *Giardia* the upgrade is inert for a
   different reason: there is no infected cell at all.** The upgrade needs a
   per-species applicability flag, not just a per-species resistance multiplier.

4. **§4c's apoptosis-is-the-clean-reward framing needs a second named
   counterexample, and it is *Cryptosporidium*'s cell loss.** *Cryptosporidium*
   does destroy epithelium — it raises proliferation *and* loss and shifts the
   population young [53] — but **not by apoptosis of infected cells**. If the
   game's debris generation is gated on apoptosis, a *Cryptosporidium* round
   under-produces debris while visibly wrecking the wall, which will read as a
   bug. Recommend debris also be produced by **extrusion/shedding** of damaged
   cells, which is the sourced mechanism here.

5. **§1b's wall asymmetry note should record that *Giardia*'s barrier damage is
   NOT immune-mediated, even though its brush-border lesion is.** The baseline
   pass correctly attributes microvillous shortening to CD8 cells. But the
   *permeability* defect occurs in **athymic** mice [21] and in **TNF-deficient**
   mice [33], correlates with trophozoite colonisation, and resolves on clearance
   [21]. So the organism has two separable damage channels: a T-cell-independent
   leak it causes itself, and a CD8-mediated brush-border lesion the player
   causes. Treating them as one number loses the most interesting thing about it.

6. **The roster's "no inflammation" line needs a species label.** "Little or no
   mucosal inflammation" is a human and mouse statement. In **gerbils** — the
   model the literature calls closest to human giardiasis — infection produced an
   early Th1/Th17/Th2 cytokine induction in Peyer's patches, mesenteric lymph
   nodes and spleen **with intestinal alterations typical of inflammation**,
   before shifting to a counter-regulatory IL-5-dominant profile [12]. Not a
   contradiction of the human picture, but the roster should not read as though
   the absence of inflammation is universal.

7. **§5's ~70% sIgA rung has a mechanistic dependency the ladder does not
   express: IgA export is downstream of IL-17.** In IL-17RA-knockout mice,
   **pIgR — the receptor that secretes IgA into the lumen — fails to be
   upregulated**, along with angiogenin 4, α/β-defensins and MBL2 [27]; and
   antibiotic microbiota depletion lowered luminal IgA specifically by hitting
   its transporter and the Peyer's-patch IgA⁺ B-cell pool [28]. If a Th17-flavoured
   upgrade is ever added, sIgA is its natural downstream unlock rather than an
   independent rung. Recorded, not a request.

8. **Clarification, not a correction: "required for clearance" in the *Giardia*
   literature means two different things, and the game should pick one.**
   B-cell-deficient mice **eliminate the vast majority** of parasites in 1–2
   weeks [25], yet IgA-deficient mice **cannot eradicate** the infection [26].
   Both are true: T cells do the bulk reduction, antibody finishes the job. And
   resistance to *reinfection* needs neither — it is intact in B-cell-deficient
   µMT mice and is not strain-specific [35]. If the game's knowledge ladder wants
   to model "antibody finishes what T cells started", this is the sourced shape.

---

## 14. The honest gaps (UNVERIFIED)

Listed so a future pass extends rather than re-derives, and so nothing below gets
quoted as established:

1. **No study localises *Giardia* antigen, or a *Giardia* EV, to an M cell, to the
   subepithelial dome, or to a lamina-propria or mesenteric-lymph-node DC in a
   live animal.** Route 1 is proven at both ends (parasite sheds; human DC
   matures) and unproven in the middle.
2. **[4]'s oral priming result does not identify the uptake route.** Its M-cell
   mention is the authors' hypothesis in discussion; no Peyer's-patch experiment
   was done.
3. **[2]'s in vivo immunogenicity arm was subcutaneous, in 3 CD1 mice per group.**
   It is evidence the cargo is immunogenic, not that the luminal route works.
4. **No measurement of antigen or EV flux across the giardiasis-loosened
   barrier.** Route 3 is damage without a demonstrated consequence for priming.
5. **No protozoan GAP evidence of any kind**, for *Giardia* or otherwise. The
   "GAPs stay open under *Giardia* because IL-1β closes them and *Giardia*
   causes no IL-1β" argument in §6 is **my inference** from [17] plus the
   baseline no-inflammation finding, not a published result.
6. **No apoptotic rate for *Giardia* in vivo** — [22] gives an elevated ratio in
   human biopsies, not a rate.
7. **The WB-strain apoptosis contradiction between [37] and [38] is
   unresolved.** Recorded as a disagreement, per the brief.
8. **The cytotoxic-vs-shedding disagreement about anti-VSP antibody is
   unresolved** — [3] (2023) against the prior consensus noted in
   `pathogens_protozoa.md` correction #10.
9. **Human necessity data do not exist for any clearance component.** Every
   "required" in §7 is mouse. Human evidence is correlative serology [49][59]
   and one small paediatric cytokine/I-FABP cohort [41].
10. **Time-to-first-specific-response in humans, anchored to inoculation, is not
    sourced.** The day-8 peak and 2–3-week clearance [48] are burden/symptom
    clocks, not immune-response clocks.
11. **[52]'s TLR4/IKK attribution for *Cryptosporidium* EVs is a review
    statement**, not primary evidence verified here.

---

## 15. References

All retrieved via **PubMed** or **Consensus** in this session.

[1] Evans-Osses I, et al. Microvesicles released from *Giardia intestinalis*
disturb host-pathogen response in vitro. *Eur J Cell Biol* 2017;96(2):131–142.
[10.1016/j.ejcb.2017.01.005](https://doi.org/10.1016/j.ejcb.2017.01.005)

[2] Faria C, et al. Unraveling the potential of *Giardia* extracellular vesicles
as a vaccine candidate. *Pharmaceutics* 2026;18(4):461.
[10.3390/pharmaceutics18040461](https://doi.org/10.3390/pharmaceutics18040461)

[3] Tenaglia AH, et al. Antibodies to variable surface antigens induce antigenic
variation in the intestinal parasite *Giardia lamblia*. *Nat Commun*
2023;14:2537.
[10.1038/s41467-023-38317-8](https://doi.org/10.1038/s41467-023-38317-8)

[4] Serradell MC, et al. Efficient oral vaccination by bioengineering virus-like
particles with protozoan surface proteins. *Nat Commun* 2019;10:361.
[10.1038/s41467-018-08265-9](https://doi.org/10.1038/s41467-018-08265-9)

[5] Sabatke B, et al. Host-pathogen cellular communication: the role of dynamin,
clathrin, and macropinocytosis in the uptake of *Giardia*-derived extracellular
vesicles. *ACS Infect Dis* 2025;11(4):954–962.
[10.1021/acsinfecdis.4c00996](https://doi.org/10.1021/acsinfecdis.4c00996)

[6] Moyano S, et al. Exosome biogenesis in the protozoa parasite *Giardia
lamblia*: a model of reduced interorganellar crosstalk. *Cells* 2019;8(12):1600.
[10.3390/cells8121600](https://doi.org/10.3390/cells8121600)

[7] Ma'ayeh SY, et al. Characterization of the *Giardia intestinalis* secretome
during interaction with human intestinal epithelial cells. *PLoS Negl Trop Dis*
2017;11(12):e0006120.
[10.1371/journal.pntd.0006120](https://doi.org/10.1371/journal.pntd.0006120)

[8] Sun M, et al. *Giardia* VSP<sup>AS7</sup> protein attenuates *Giardia
intestinalis*-induced host macrophage pyroptosis. *Parasit Vectors*
2023;16:359.
[10.1186/s13071-023-05949-0](https://doi.org/10.1186/s13071-023-05949-0)

[9] Siddiq A, et al. A thermo-resistant and RNase-sensitive cargo from *Giardia
duodenalis* extracellular vesicles modifies the behaviour of enterobacteria.
*J Extracell Biol* 2023;2(9):e109.
[10.1002/jex2.109](https://doi.org/10.1002/jex2.109)

[10] Hill DR. Lymphocyte proliferation in Peyer's patches of *Giardia
muris*-infected mice. *Infect Immun* 1990;58(8):2683–2685.
[10.1128/iai.58.8.2683-2685.1990](https://doi.org/10.1128/iai.58.8.2683-2685.1990)

[11] Van Crombrugge CE, Decorte B, Seys LJM, Geldhof P. Unravelling the cellular
sources and location of IL-17A production during a *Giardia* infection. *Sci Rep*
2025;15:38790.
[10.1038/s41598-025-22662-3](https://doi.org/10.1038/s41598-025-22662-3)

[12] Serradell MC, et al. Cytokines, antibodies, and histopathological profiles
during *Giardia* infection and variant-specific surface protein-based
vaccination. *Infect Immun* 2018;86(6):e00773-17.
[10.1128/IAI.00773-17](https://doi.org/10.1128/IAI.00773-17)

[13] Carlson JR, Heyworth MF, Owen RL. T-lymphocyte subsets in nude mice with
*Giardia muris* infection. *Thymus* 1987;9(3):189–196. PMID 3109083 (no DOI
retrieved).

[14] Beloqui A, Brayden DJ, Artursson P, Préat V, des Rieux A. A human intestinal
M-cell-like model for investigating particle, antigen and microorganism
translocation. *Nat Protoc* 2017;12(7):1387–1399.
[10.1038/nprot.2017.041](https://doi.org/10.1038/nprot.2017.041)

[15] McDole JR, et al. Goblet cells deliver luminal antigen to CD103+ dendritic
cells in the small intestine. *Nature* 2012;483(7389):345–349.
[10.1038/nature10863](https://doi.org/10.1038/nature10863)

[16] Knoop KA, et al. Microbial sensing by goblet cells controls immune
surveillance of luminal antigens in the colon. *Mucosal Immunol*
2015;8(1):198–210.
[10.1038/mi.2014.58](https://doi.org/10.1038/mi.2014.58)

[17] Kulkarni DH, et al. Goblet cell associated antigen passages are inhibited
during *Salmonella typhimurium* infection to prevent pathogen dissemination and
limit responses to dietary antigens. *Mucosal Immunol* 2018;11(4):1103–1113.
[10.1038/s41385-018-0007-6](https://doi.org/10.1038/s41385-018-0007-6)

[18] Kulkarni DH, et al. Goblet cell associated antigen passages support the
induction and maintenance of oral tolerance. *Mucosal Immunol*
2020;13(2):271–282.
[10.1038/s41385-019-0240-7](https://doi.org/10.1038/s41385-019-0240-7)

[19] Niess JH, et al. CX3CR1-mediated dendritic cell access to the intestinal
lumen and bacterial clearance. *Science* 2005;307(5707):254–258.
[10.1126/science.1102901](https://doi.org/10.1126/science.1102901)

[20] Schulz O, et al. Intestinal CD103+, but not CX3CR1+, antigen sampling cells
migrate in lymph and serve classical dendritic cell functions. *J Exp Med*
2009;206(13):3101–3114.
[10.1084/jem.20091925](https://doi.org/10.1084/jem.20091925)

[21] Scott KG-E, Meddings JB, Kirk DR, Lees-Miller SP, Buret AG. Intestinal
infection with *Giardia* spp. reduces epithelial barrier function in a myosin
light chain kinase-dependent fashion. *Gastroenterology* 2002;123(4):1179–1190.
[10.1053/gast.2002.36002](https://doi.org/10.1053/gast.2002.36002)

[22] Troeger H, et al. Effect of chronic *Giardia lamblia* infection on epithelial
transport and barrier function in human duodenum. *Gut* 2007;56(3):328–335.
[10.1136/gut.2006.100198](https://doi.org/10.1136/gut.2006.100198)

[23] Holthaus D, et al. Dissection of barrier dysfunction in organoid-derived
human intestinal epithelia induced by *Giardia duodenalis*. *Gastroenterology*
2022;162(3):844–858.
[10.1053/j.gastro.2021.11.022](https://doi.org/10.1053/j.gastro.2021.11.022)

[24] Heyworth MF, Carlson JR, Ermak TH. Clearance of *Giardia muris* infection
requires helper/inducer T lymphocytes. *J Exp Med* 1987;165(6):1743–1748.
[10.1084/jem.165.6.1743](https://doi.org/10.1084/jem.165.6.1743)

[25] Singer SM, Nash TE. T-cell-dependent control of acute *Giardia lamblia*
infections in mice. *Infect Immun* 2000;68(1):170–175.
[10.1128/IAI.68.1.170-175.2000](https://doi.org/10.1128/IAI.68.1.170-175.2000)

[26] Langford TD, et al. Central importance of immunoglobulin A in host defense
against *Giardia* spp. *Infect Immun* 2002;70(1):11–18.
[10.1128/IAI.70.1.11-18.2002](https://doi.org/10.1128/IAI.70.1.11-18.2002)

[27] Paerewijck O, et al. Interleukin-17 receptor A (IL-17RA) as a central
regulator of the protective immune response against *Giardia*. *Sci Rep*
2017;7:8520.
[10.1038/s41598-017-08590-x](https://doi.org/10.1038/s41598-017-08590-x)

[28] Maertens B, et al. Regulatory role of the intestinal microbiota in the immune
response against *Giardia*. *Sci Rep* 2021;11:10601.
[10.1038/s41598-021-90261-z](https://doi.org/10.1038/s41598-021-90261-z)

[29] Li E, Zhou P, Petrin Z, Singer SM. Mast cell-dependent control of *Giardia
lamblia* infections in mice. *Infect Immun* 2004;72(11):6642–6649.
[10.1128/IAI.72.11.6642-6649.2004](https://doi.org/10.1128/IAI.72.11.6642-6649.2004)

[30] Tako EA, Hassimi MF, Li E, Singer SM. Transcriptomic analysis of the host
response to *Giardia duodenalis* infection reveals redundant mechanisms for
parasite control. *mBio* 2013;4(6):e00660-13.
[10.1128/mBio.00660-13](https://doi.org/10.1128/mBio.00660-13)

[31] Eckmann L, et al. Nitric oxide production by human intestinal epithelial
cells and competition for arginine as potential determinants of host defense
against the lumen-dwelling pathogen *Giardia lamblia*. *J Immunol*
2000;164(3):1478–1487.
[10.4049/jimmunol.164.3.1478](https://doi.org/10.4049/jimmunol.164.3.1478)

[32] Li E, Tako EA, Singer SM. Complement activation by *Giardia duodenalis*
parasites through the lectin pathway contributes to mast cell responses and
parasite control. *Infect Immun* 2016;84(4):1092–1099.
[10.1128/IAI.00074-16](https://doi.org/10.1128/IAI.00074-16)

[33] Zhou P, Li E, Shea-Donohue T, Singer SM. Tumour necrosis factor alpha
contributes to protection against *Giardia lamblia* infection in mice. *Parasite
Immunol* 2007;29(7):367–374.
[10.1111/j.1365-3024.2007.00953.x](https://doi.org/10.1111/j.1365-3024.2007.00953.x)

[34] Kamda JD, Nash TE, Singer SM. *Giardia duodenalis*: dendritic cell defects in
IL-6 deficient mice contribute to susceptibility to intestinal infection. *Exp
Parasitol* 2012;130(3):288–291.
[10.1016/j.exppara.2012.01.003](https://doi.org/10.1016/j.exppara.2012.01.003)

[35] Li E, Liu M, Singer SM. Resistance to reinfection in mice as a vaccine model
for giardiasis. *Hum Vaccin Immunother* 2014;10(6):1536–1543.
[10.4161/hv.29116](https://doi.org/10.4161/hv.29116)

[36] Lopez-Romero G, Quintero J, Astiazarán-García H, Velazquez C. Host defences
against *Giardia lamblia*. *Parasite Immunol* 2015;37(8):394–406.
[10.1111/pim.12210](https://doi.org/10.1111/pim.12210)

[37] Chin AC, et al. Strain-dependent induction of enterocyte apoptosis by
*Giardia lamblia* disrupts epithelial barrier function in a caspase-3-dependent
manner. *Infect Immun* 2002;70(7):3673–3680.
[10.1128/IAI.70.7.3673-3680.2002](https://doi.org/10.1128/IAI.70.7.3673-3680.2002)

[38] Panaro MA, et al. Caspase-dependent apoptosis of the HCT-8 epithelial cell
line induced by the parasite *Giardia intestinalis*. *FEMS Immunol Med Microbiol*
2007;51(2):302–309.
[10.1111/j.1574-695X.2007.00304.x](https://doi.org/10.1111/j.1574-695X.2007.00304.x)

[39] Koh WH, et al. *Giardia duodenalis* assemblage-specific induction of
apoptosis and tight junction disruption in human intestinal epithelial cells:
effects of mixed infections. *J Parasitol* 2013;99(2):353–358.
[10.1645/GE-3021.1](https://doi.org/10.1645/GE-3021.1)

[40] Yu LCH, et al. SGLT-1-mediated glucose uptake protects human intestinal
epithelial cells against *Giardia duodenalis*-induced apoptosis. *Int J Parasitol*
2008;38(8-9):923–934.
[10.1016/j.ijpara.2007.12.004](https://doi.org/10.1016/j.ijpara.2007.12.004)

[41] Cascais-Figueiredo T, et al. Giardiasis alters intestinal fatty acid binding
protein (I-FABP) and plasma cytokines levels in children in Brazil. *Pathogens*
2019;9(1):7.
[10.3390/pathogens9010007](https://doi.org/10.3390/pathogens9010007)

[42] Kamda JD, Singer SM. Phosphoinositide 3-kinase-dependent inhibition of
dendritic cell interleukin-12 production by *Giardia lamblia*. *Infect Immun*
2009;77(2):685–693.
[10.1128/IAI.00718-08](https://doi.org/10.1128/IAI.00718-08)

[43] Obendorf J, et al. Increased expression of CD25, CD83, and CD86, and
secretion of IL-12, IL-23, and IL-10 by human dendritic cells incubated in the
presence of Toll-like receptor 2 ligands and *Giardia duodenalis*. *Parasit
Vectors* 2013;6:317.
[10.1186/1756-3305-6-317](https://doi.org/10.1186/1756-3305-6-317)

[44] Summan A, Nejsum P, Williams AR. Modulation of human dendritic cell activity
by *Giardia* and helminth antigens. *Parasite Immunol* 2018;40(5):e12525.
[10.1111/pim.12525](https://doi.org/10.1111/pim.12525)

[45] Banik S, et al. *Giardia duodenalis* arginine deiminase modulates the
phenotype and cytokine secretion of human dendritic cells by depletion of
arginine and formation of ammonia. *Infect Immun* 2013;81(7):2309–2317.
[10.1128/IAI.00004-13](https://doi.org/10.1128/IAI.00004-13)

[46] Fernández-Lainez C, et al. The giardial arginine deiminase participates in
*Giardia*-host immunomodulation in a structure-dependent fashion via Toll-like
receptors. *Int J Mol Sci* 2022;23(19):11552.
[10.3390/ijms231911552](https://doi.org/10.3390/ijms231911552)

[47] Angelova VV, et al. *Giardia* increases macrophage production of the
anti-inflammatory cytokine interleukin-10 in response to lipopolysaccharide via
macrophage galactose binding lectin. *ImmunoHorizons* 2025;9(7):vlaf019.
[10.1093/immhor/vlaf019](https://doi.org/10.1093/immhor/vlaf019)

[48] Černíková L, Faso C, Hehl AB. Five facts about *Giardia lamblia*. *PLoS
Pathog* 2018;14(9):e1007250.
[10.1371/journal.ppat.1007250](https://doi.org/10.1371/journal.ppat.1007250)

[49] Hjøllo T, et al. Longitudinal cohort study of serum antibody responses
towards *Giardia lamblia* variant-specific surface proteins in a non-endemic
area. *Exp Parasitol* 2018;191:66–72.
[10.1016/j.exppara.2018.06.005](https://doi.org/10.1016/j.exppara.2018.06.005)

[50] Grit GH, et al. *Giardia duodenalis* stimulates partial maturation of bovine
dendritic cells associated with altered cytokine secretion and induction of
T-cell proliferation. *Parasite Immunol* 2014;36(4):157–169.
[10.1111/pim.12095](https://doi.org/10.1111/pim.12095)

[51] Bertuccini L, et al. Unveiling *Cryptosporidium parvum* sporozoite-derived
extracellular vesicles: profiling, origin, and protein composition. *Front Cell
Infect Microbiol* 2024;14:1367359.
[10.3389/fcimb.2024.1367359](https://doi.org/10.3389/fcimb.2024.1367359)

[52] Fadaee M, et al. Extracellular vesicles in intestinal protozoa: hidden
mediators of host-parasite communication. *Gut Pathog* 2025;17:69. (Review.)
[10.1186/s13099-025-00747-8](https://doi.org/10.1186/s13099-025-00747-8)

[53] Wallbank BA, Pardy RD, Brodsky IE, Hunter CA, Striepen B. *Cryptosporidium*
impacts epithelial turnover and is resistant to induced death of the host cell.
*mBio* 2024;15(8):e0172024.
[10.1128/mbio.01720-24](https://doi.org/10.1128/mbio.01720-24)

[54] Liu J, et al. Inhibition of apoptosis in *Cryptosporidium parvum*-infected
intestinal epithelial cells is dependent on survivin. *Infect Immun*
2008;76(8):3784–3792.
[10.1128/IAI.00308-08](https://doi.org/10.1128/IAI.00308-08)

[55] McCole DF, Eckmann L, Laurent F, Kagnoff MF. Intestinal epithelial cell
apoptosis following *Cryptosporidium parvum* infection. *Infect Immun*
2000;68(3):1710–1713.
[10.1128/IAI.68.3.1710-1713.2000](https://doi.org/10.1128/IAI.68.3.1710-1713.2000)

[56] Ross R, et al. Human dendritic cell interactions with the zoonotic parasite
*Cryptosporidium parvum* result in activation and maturation. *Front Immunol*
2024;15:1388366.
[10.3389/fimmu.2024.1388366](https://doi.org/10.3389/fimmu.2024.1388366)

[57] Li L, et al. Giardiavirus infection alleviates growth restriction and
intestinal damage caused by the intestinal parasite *Giardia duodenalis*.
*Parasit Vectors* 2025;18:71.
[10.1186/s13071-025-06692-4](https://doi.org/10.1186/s13071-025-06692-4)

[58] Pizarro GL, et al. Genotype-specific small EVs released by *Giardia lamblia*
act as mediators of phenotypic adaptation under metronidazole-induced stress.
*J Extracell Vesicles* 2025;14(9):e70139.
[10.1002/jev2.70139](https://doi.org/10.1002/jev2.70139)

[59] Smith PD, et al. IgG antibody to *Giardia lamblia* detected by
enzyme-linked immunosorbent assay. *Gastroenterology* 1981. Retrieved via
Consensus; **DOI not retrieved this session.**
