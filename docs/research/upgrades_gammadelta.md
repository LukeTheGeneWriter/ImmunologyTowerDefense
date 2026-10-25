# γδ IEL upgrade rows — proposal with evidence

Research pass, 2026-10-25. Nothing in the repo was edited. All literature
reached through the PubMed and Consensus MCP tools; **according to PubMed**
and the cited DOIs for every claim. Mouse vs human is flagged inline.
Anything I could not pin is marked **UNVERIFIED**.

The existing γδ IEL research pass (`docs/research/research_nk_unconventional.md`
§7) is accurate and this builds on it rather than repeating it. Three things
are **new** here and should land in `docs/CELL_KINETICS_REFERENCE.md`:

1. **The γδ IEL speed is no longer UNVERIFIED** — measured at
   **3.4 ± 0.2 µm/min** in enteroid co-culture, rising to 4.5 ± 0.2 under
   IFNα (Fischer/Edelblum, *J Immunol* 2021 abstract). That is a dendritic
   cell's pace, not a macrophage's. Game speed 1 remains a legitimate
   deliberate departure — but the table should say "measured ~3.4,
   deliberately set to 1" rather than "UNVERIFIED".
2. **CD103 is the wrong lever for "covers more wall"** and the repo's
   current framing of the Edelblum 2015 63% result invites the wrong
   upgrade. Occludin is the honest lever. §2.1 below.
3. **The repair magnitude has a number**: γδ-deficient mice lose **22%** of
   epithelial proliferation in mucosal atrophy and **14%** in hypertrophy
   (Yang 2004).

---

## 1. The recommended rows

| # | Name | Player-facing effect | Field written, per level | Price / cap | Key citation |
|---|---|---|---|---|---|
| 1 | **Occludin-dependent intercalation** | "Patrols a wider stretch of wall." | `BarrierPatrolRadiusCells` 2.0 → 2.6 → 3.2 → 3.8 (+0.6/level) | 35 ATP / cap 3 | Edelblum 2012, [DOI](https://doi.org/10.1073/pnas.1112519109) |
| 2 | **KGF-1 (FGF7) delivery** | "Mends broken wall faster." | `BarrierRepairIntervalSeconds` 3.5 → 2.8 → 2.2 → 1.8 s | 40 ATP / cap 3 | Chen 2002, [DOI](https://doi.org/10.1073/pnas.212290499); Yang 2004, [DOI](https://doi.org/10.4049/jimmunol.172.7.4151) |
| 3 | **IL-15 trans-presentation** | "The epithelium keeps one more of these alive. Level 2 inflames the gut." | `MaxActiveChildren` +1 per level — **level 2 carries a real cost (§3)** | 45 ATP / cap 2 | Inagaki-Ohara 1997, [DOI](https://doi.org/10.1002/eji.1830271121); Yu 2006, [DOI](https://doi.org/10.4049/jimmunol.176.10.6180); **drawback:** Yokoyama 2009, [DOI](https://doi.org/10.1073/pnas.0908834106); Abadie 2020, [DOI](https://doi.org/10.1038/s41586-020-2003-8) |
| 4 | **Type I interferon priming** | "Slips between enterocytes faster, checking more of them." | `FineTilesPerTick` ×1.30 per level (2 levels ≈ ×1.7) | 30 ATP / cap 2 | Fischer 2021 (abstract — see caveat); Parthasarathy 2025 (abstract) |

**Rejected on the evidence: a BTNL3/BTNL8 "sharper recognition" row.** It is
real biology and it is *not* a recognition upgrade — see §2.5. The defensible
version belongs on the wall (§1d host-cell upgrades), not on this tower.

Total to max all four: 3×35 + 3×40 + 2×45 + 2×30 = **375 ATP**, which is a
whole-game investment for a tower that never kills anything. That is the
right shape for the only repairer in the roster.

---

## 2. Row by row

### 2.1 Occludin-dependent intercalation → `BarrierPatrolRadiusCells`

**What actually governs how much epithelium one γδ IEL covers.** Four
candidates were on the table; only one survives as an upgrade.

**Occludin — yes, this is the lever.** γδ IELs are not stationary: in vivo
imaging of GFP γδ T-cell transgenic mouse jejunum shows them migrating
actively within the intraepithelial compartment and into the lamina propria,
"as a result, **each γδ IEL contacts multiple epithelial cells**." Occludin
is concentrated at every γδ IEL/epithelial contact, forming a ring around the
IEL; occludin from both the epithelial cell *and* the γδ T cell contributes
to the ring and to migration within epithelial monolayers; and occludin-KO γδ
T cells are defective in **both initial accumulation and migration** in the
compartment (Edelblum et al., *PNAS* 2012, 109:7097–102,
[DOI](https://doi.org/10.1073/pnas.1112519109)). Mouse.

The payoff is quantified in the follow-up. Intravital imaging during
infection: γδ IELs rapidly localise to, and remain near, epithelial cells in
**direct contact with bacteria**. Within 1 hour, mice with migration-defective
occludin-KO γδ T cells had more *T. gondii* or *S. typhimurium* in the mucosa,
and **pathogen invasion in Tcrd-KO mice was quantitatively similar to that in
mice with occludin-deficient γδ T cells** — i.e. a γδ IEL that cannot migrate
is worth exactly as much as no γδ IEL at all. Systemic salmonellosis also
developed faster and more severely (Edelblum et al., *Gastroenterology* 2015,
148:1417–26, [DOI](https://doi.org/10.1053/j.gastro.2015.02.053)). Mouse.

**CD103/E-cadherin — the 63% number is real but it is the wrong lever, and
selling it as an upgrade would be a mistake.** In the same 2015 paper,
CD103-KO mice — whose γδ IELs migrate *more* into the lateral intercellular
space — had pathogen invasion **reduced by 63%**. Tempting. Three things
count against building on it:

- αEβ7 (CD103) is a *retention* receptor binding epithelial E-cadherin.
  Integrin-αE-deficient mice have **reduced** intestinal and vaginal IEL
  numbers (and, unexpectedly, reduced lamina propria T cells), with the
  effect strongest after microbial colonisation — though it hit αβCD8 IELs
  harder than γδ (Schön et al., *J Immunol* 1999, 162:6641–9, PMID
  [10352281](https://pubmed.ncbi.nlm.nih.gov/10352281/) — verified, no DOI).
  So "more intercalation" is bought with fewer cells, and the γδ-specific
  share of that cost is the weakest link in this particular objection.
- Ex vivo it goes the other way: in IEL/organoid co-culture live imaging,
  IELs lacking CD103 showed **aberrant mobility and reduced crawling speed**
  (Enderle et al., *Int J Mol Sci* 2021, 22:5148,
  [DOI](https://doi.org/10.3390/ijms22105148)). In vivo LIS entry up, ex vivo
  crawl speed down — the literature does not agree with itself here.
- Most decisively for this game: the **human** protective subset *is*
  CD103⁺. Human colonic γδ T cells include a phenotypically distinct subset
  co-expressing TCR Vγ4 **and CD103**; it is disproportionately diminished
  and dysregulated in IBD, and its restoration on treatment associates with
  sustained remission (Dart et al., *Science* 2023, 381:eadh0301,
  [DOI](https://doi.org/10.1126/science.adh0301)). An upgrade that removes
  CD103 deletes the cell the tower is modelling.

**Verdict: occludin, not CD103.** If the Director wants the 63% number in a
tooltip it belongs as flavour on the occludin row ("more time between the
cells means less gets through — 63% less, measured"), not as its own
mechanism.

**CCR9 — no.** CCR9/CCL25 is gut *homing*: it gets the cell from the
circulation to the small intestine. The intraepithelial fate work shows
SI-epithelium tropism is set by developmentally regulated α4β7 on recent
thymic emigrants, and that unconventional types (notably Vγ7⁺, the hallmark
of γδ IELs) arrive naive and then proliferate in situ (Guy-Grand et al.,
*J Exp Med* 2013, 210:1839–54, [DOI](https://doi.org/10.1084/jem.20122588)).
In this game the γδ tower is explicitly **not node-resident and not
trafficked** — it is already at the wall — so a homing upgrade has nothing to
act on. Skip.

**LFA-1/ICAM-1 — UNVERIFIED for this cell.** PubMed returns nothing for γδ
IEL surveillance via LFA-1/ICAM-1. Do not write a row on it.

**Why it is a decision, not a bigger number.** Patrol radius buys *coverage*,
never throughput. §1b says a plaque forms where crowding **peaks** over a
~1.5-cell stretch, and this tower suppresses charge, so the real choice is:

- **Row 1** (wider radius) → one cell touches more forming plaques, each
  suppressed at the same rate. Good against several small knots.
- **Row 3** (one more cell) → two independently placeable points of coverage,
  and two cells stacked on one hotspot suppress it twice as hard. Good
  against one big knot.

Wide-and-thin versus deep-and-narrow, on the same tower, at similar prices.
That is the decision.

### 2.2 KGF-1 (FGF7) delivery → `BarrierRepairIntervalSeconds`

**What they actually produce.** KGF/FGF7 is the well-evidenced one, and in
the small intestine it is startlingly exclusive to this cell: "KGF expression
was **confined to γδ-TCR⁺ IELs**", highest in the crypts, less in the lower
villus, markedly lower in the upper villus. In γδ⁻/⁻ mice epithelial cell
proliferation fell in controls and in both models — **22% decline** in
mucosal atrophy (total parenteral nutrition) and **14%** in mucosal
hypertrophy (short bowel syndrome) (Yang et al., *J Immunol* 2004,
172:4151–8, [DOI](https://doi.org/10.4049/jimmunol.172.7.4151)). Mouse.
**That 14–22% is the best-evidenced magnitude for the repair effect** and it
should be what the row's three levels sum to, not more.

**How much the evidence supports accelerated restitution, and over what
timescale.** Strongly, and the timescale is **days**:

- In DSS colitis, large numbers of γδ (not αβ) T cells localise to sites of
  epithelial damage and express KGF. **Both** TCRδ⁻/⁻ and KGF⁻/⁻ mice were
  more prone to injury *and* showed **delayed tissue repair after DSS was
  stopped**; termination of DSS produced vigorous epithelial proliferation in
  wild-type mice but **not** in TCRδ⁻/⁻ or KGF⁻/⁻ mice (Chen et al., *PNAS*
  2002, 99:14338–43, [DOI](https://doi.org/10.1073/pnas.212290499)). Mouse.
  457 citations; this is the canonical result.
- It is **causal and rescuable**. CD100 (semaphorin 4D) on IELs ligating
  plexin B2 on epithelium is required: CD100⁻/⁻ mice get much worse DSS
  colitis because the colon epithelium **fails to mount a proliferative
  response to damage**, their γδ IELs fail to make KGF-1, and
  **administering recombinant KGF-1 ameliorates disease and reverses the
  susceptibility** (Meehan et al., *Mucosal Immunol* 2014, 7:134–42,
  [DOI](https://doi.org/10.1038/mi.2013.32)). Mouse. A clean
  knockout-and-rescue is as good as this evidence gets.
- Depleting them removes the effect in a transplant setting: anti-CD52 mAb
  depleted graft γδ iIELs, and that was accompanied by reduced mucosal KGF
  mRNA and reduced epithelial-cell proliferation (Shen et al.,
  *Transplantation* 2013, 95:663–70,
  [DOI](https://doi.org/10.1097/TP.0b013e31827e6ab3)). Mouse.
- **Beyond KGF**, a genome-wide analysis of the γδ IEL response to DSS
  damage found a coordinate transcriptional program of **cytoprotective,
  immunomodulatory and antibacterial** factors, regulated by the commensal
  microbiota, and TCRδ-deficient mice could not control mucosal penetration
  of commensals immediately after damage (Ismail et al., *J Immunol* 2009,
  182:3047–54, [DOI](https://doi.org/10.4049/jimmunol.0802705), PMID
  19234201). Mouse. So
  "repair factors" plural is correct; KGF is the one with a number on it.
  γδ T cells are also a dominant innate IL-17 source at barrier sites, and
  IL-17 protects against DSS epithelial injury.
- **Human:** in HIV enteropathy the human counterpart is visible but
  *dysregulated* — intestinal-resident intraepithelial T lymphocytes express
  KGF, KGF drives crypt proliferation, **absence of these cells is associated
  with reduced epithelial cell turnover**, and the pathological version
  produces immature enterocytes and malabsorption (Kapembwa et al., *Int J
  Infect Dis* 2023, 139:86–91,
  [DOI](https://doi.org/10.1016/j.ijid.2023.11.037)). Review, human.

**The honest timescale caveat, which the tooltip must not overstate.** Every
restitution result above is measured over **days** (DSS recovery curves,
TPN/SBS remodelling). Nothing in the literature supports a 1.8-second repair
tick. The *rank order* — γδ present repairs, γδ absent does not — is solid;
the *rate* is pure game compression. State that in GAME_DESIGN rather than
implying the number is sourced.

**Why it is a decision.** Repair is reactive and patrol is preventive, and
§1b makes that asymmetry sharp: a dead stretch of wall "charges markedly
faster — the board remembers where it was breached". Repair is the only thing
in the game that un-marks a breached spot. So the row's value is a function
of how much wall the player has already lost: a player holding a clean wall
should buy Row 1, a player on a battered wall should buy Row 2, and the
correct answer changes *during* a run. That is state-dependent, not a
strictly-better number.

One risk worth naming: KGF is a real mitogen with a real failure mode, and
the citation for it is the human one already in hand. In HIV enteropathy,
KGF "induces epithelial proliferation mainly in the crypts, leading to
**functional immaturity of enterocytes**, reduced gut absorptive surface
area and malabsorption" (Kapembwa et al. 2023,
[DOI](https://doi.org/10.1016/j.ijid.2023.11.037)). So if the Director ever
wants a second drawback, **"wall that regrows too fast comes back weak"** is
cited and human. I would not build two drawbacks on one tower — the IL-15
one in §3 is stronger — but this is the spare.

### 2.3 IL-15 trans-presentation → `MaxActiveChildren`

**Is "more IL-15" defensible as a population/longevity upgrade? Yes, and for
γδ IELs specifically it is better evidenced than for their αβ neighbours.**

- γδ i-IELs constitutively express a **high level of IL-15Rα mRNA** and
  proliferate in response to IL-15 **more vigorously than αβ i-IELs**; IL-15
  induces polyclonal expansion (unlike IL-7, which skews the Vγ/Vδ
  repertoire); and IL-15 **efficiently prevents γδ i-IELs from apoptosis
  induced by growth-factor deprivation**, with upregulated Bcl-2
  (Inagaki-Ohara et al., *Eur J Immunol* 1997, 27:2885–91,
  [DOI](https://doi.org/10.1002/eji.1830271121)). Mouse, in vitro.
- IL-15 beats IL-2 for survival of restimulated γδ iIELs: significantly
  more live cells recovered, less activation-induced cell death, and Bcl-xL
  sustained rather than decaying (Chu et al., *J Immunol* 1999, 162:1896–903,
  PMID [9973456](https://pubmed.ncbi.nlm.nih.gov/9973456/) — no DOI; J
  Immunol predates DOI assignment for this volume). Mouse. Same paper notes
  γδ cells are ~half of mouse intestinal IELs.
- The source is the epithelium, which is exactly the game's fiction.
  MyD88⁻/⁻ mice have significantly fewer γδ i-IELs and severely reduced
  epithelial IL-15; introducing an IL-15 transgene **partly restores** γδ
  i-IEL numbers; and bone-marrow chimeras localise the requirement to the
  **parenchymal** (epithelial) compartment, not the haematopoietic one (Yu
  et al., *J Immunol* 2006, 176:6180–5,
  [DOI](https://doi.org/10.4049/jimmunol.176.10.6180)). Mouse. This is why
  the row should be named **trans-presentation** — the wall feeds the tower.

**The nuance that keeps this honest.** For small-intestinal *memory CD8αβ*
IELs, lack of IL-7/IL-15 signalling "affects interferon-γ production by,
more than survival of" the cells — tetramer⁺ cells were still found in
IL-15-deficient mice, and what IL-15 loss really did was narrow the CTL
avidity repertoire (Isakov et al., *Eur J Immunol* 2011, 41:3513–28,
[DOI](https://doi.org/10.1002/eji.201141453)). So "IL-15 = survival" is a
γδ-IEL-specific claim, not a blanket IEL claim. Fine for this tower; do not
reuse the row on the CTL.

**Human:** IL-15 is epithelial and is overexpressed in active coeliac disease
— the human evidence is strong but it is almost entirely *pathological*
(§3). There is no human study showing that raising IL-15 benignly expands a
protective γδ IEL population. Flag the row as mouse-grounded.

**Why it is a decision.** Because at level 2 it is a trade, not a buff. §3.

### 2.4 Type I interferon priming (CD47) → `FineTilesPerTick`

**This is the only cited lever that makes a γδ IEL genuinely faster**, and it
resolves the repo's UNVERIFIED speed entry at the same time.

IFNα given to GFP γδ T-cell reporter mice increased γδ IEL **track speed by
30%** and migration into the lateral intercellular space by **75%**. RNAseq
of sorted γδ IELs showed CD47 up **3.5-fold** (and Jaml 4.7-fold); in
γδ IEL/enteroid co-culture IFNα raised speed from **3.4 ± 0.2 to 4.5 ± 0.2
µm/min** and displacement from 34 ± 3.1 to 47 ± 3.0 µm; and an anti-CD47
blocking antibody **abrogated** the effect while anti-JAML did not (Fischer
et al., *J Immunol* 2021, 206:1 suppl, Consensus record
[link](https://consensus.app/papers/details/5aac2fdae8e254588a71c52ab4ed826f/);
extended in Parthasarathy et al., *Physiology* 2025 abstract, Consensus
record [link](https://consensus.app/papers/details/1c53fa13d3b95097b9df945b7f8f7976/)
— 23% more LIS migration in vitro, 30% faster track speed in vivo,
p<0.0001). Mouse.

> **Evidence-quality caveat, stated plainly: both of these are conference
> abstracts.** The Physiology 2025 record says in so many words that the
> editorial board was not involved in peer review. I could not find a
> peer-reviewed full paper for the IFN→CD47→γδ-IEL-motility result. The
> underlying imaging methodology *is* peer-reviewed (Fischer & Edelblum,
> *Curr Protoc* 2022, [DOI](https://doi.org/10.1002/cpz1.516)). Treat the
> 30% / 75% / 3.4 µm/min numbers as **provisional**, cite them as abstracts,
> and re-check before they go into a locked design doc.

A second, nicely counter-intuitive finding from the 2025 abstract: mice
lacking **epithelial** CD47 showed a **14% increase** in γδ IEL migratory
speed with a trend toward **reduced dwell time in the LIS** — i.e. epithelial
CD47 may regulate *how long* an IEL lingers with one enterocyte. Loss of
epithelial CD47 did not change total jejunal γδ IEL number or IEL subsets.

**Why it is a decision, and why the cap is 2.** The dwell-time finding is the
trade. A faster γδ IEL visits more enterocytes but stays shorter at each,
and in this game "staying" is what suppresses a plaque's charge. So speed
should buy breadth at the cost of depth, which is the opposite trade to Row 1
(which buys breadth at no cost but at a higher price per unit). Capping at 2
is the honest expression of "the measured effect is +30%, not +300%": two
levels ≈ ×1.7, which brackets the measured range. Going to cap 4 would be
inventing data.

Also worth saying out loud: the fiction is that **the player's own antiviral
state speeds up their wall patrol**. If type I interferon ever becomes the
tissue-wide state that `research_nk_unconventional.md` recommends (rather
than a pDC tower), this row should read off that state instead of being a
flat purchase — which would make it the first upgrade in the game that is
*conditional on the board*.

### 2.5 BTNL3/BTNL8 — why I am recommending **against** a recognition row

The brief asks whether "sharper epithelial recognition" is defensible. The
biology is excellent and the framing is wrong.

What the evidence says: murine intraepithelial γδ T cells include distinct
tissue-protective subsets **selected by epithelial butyrophilin-like (BTNL)
heteromers**, and this is **conserved in humans** — a phenotypically distinct
human colonic subset co-expressing TCR Vγ4 and CD103, disproportionately
diminished and dysregulated in IBD, with on-treatment restoration associated
with sustained remission, and germline **BTNL3/BTNL8 hypomorphism is a risk
factor for penetrating Crohn's disease** (Dart et al., *Science* 2023,
381:eadh0301, [DOI](https://doi.org/10.1126/science.adh0301)). Human + mouse.

The operative verb is **selected**, not *sensed*. BTNL3/BTNL8 shapes *which
γδ cells exist and persist in that epithelium*; it is not a target-detection
receptor the cell points at a pathogen. A 2023 review states the detection
side plainly: γδ IEL "activation and effector responses occur
**independently of antigen recognition**" (Alonso & Edelblum, *Discov
Immunol* 2023, [DOI](https://doi.org/10.1093/discim/kyad011)). So a row
written as "recognises threats more sharply" would be a straightforwardly
inaccurate sentence about this cell, and `StressSenseChancePerTick` is the
wrong field twice over — it is also the field that would quietly turn the
game's only non-killer into a killer, which §4c exists to prevent.

**What is defensible instead — and it is a better idea.** BTNL3/BTNL8 is an
**epithelial** gene product. It belongs in §1d (host-cell upgrades), bought
on the *wall*, not on the tower:

> **"BTNL3/BTNL8 expression"** — a wall upgrade that raises the γδ tower's
> `MaxActiveChildren` cap and is a prerequisite for the protective subset
> existing at all.

That makes the epithelium select its own garrison, which is what the paper
says, and it gives the game its first upgrade bought on one object that
improves a different object. It also sets up the drawback in §3 for free,
because the same axis runs backwards: in coeliac disease, gluten-driven
inflammation **depletes** the naturally occurring Vγ4/Vδ1 IELs with innate
cytolytic properties and BTNL3/BTNL8 specificity, **reduces epithelial
BTNL8**, and lets gluten-sensitive IFN-γ-producing Vδ1 IELs expand in their
place — and **excluding gluten restores BTNL8 but does not restore the
physiological Vγ4/Vδ1 subset** (Mayassi et al., *Cell* 2019, 176:967–981,
[DOI](https://doi.org/10.1016/j.cell.2018.12.039)). Human. Stopping the
inflammation repairs the *niche* and does not repair the *garrison*.

### 2.6 Considered and not recommended as rows

- **JAML–CAR costimulation.** Strong biology: JAML binding its ligand CAR
  costimulates epithelial γδ T cells into proliferation and **growth-factor
  production**, and blocking JAML costimulation gives diminished γδ
  activation and **delayed wound closure akin to the absence of γδ T cells
  altogether** (Witherden et al., *Science* 2010, 329:1205–10,
  [DOI](https://doi.org/10.1126/science.1192698) — skin). In the gut, DSS
  upregulates JAML on γδ IELs with a CAR isoform shift on epithelium, and
  JAML⁻/⁻ mice show worse weight loss and epithelial damage, reduced γδ IEL
  activation and impaired IL-17 — not explained by IEL numbers (Johnson
  et al., *J Immunol* 2020 abstract, Consensus record
  [link](https://consensus.app/papers/details/c61a2748e84e54a6a034cb9d8a4d4eb0/)).
  **Not a row** because mechanically it writes the same field as Row 2
  (faster/stronger repair) via a different protein, and four rows should be
  four mechanics, not four names for two. Keep it as the flavour text *on*
  Row 2, or as the gate if repair is ever made conditional on damage being
  recognised.
- **CD100/plexin-B2.** Same objection, same field. It is the better
  *citation* for Row 2 (knockout + rescue) and the worse *row*.
- **A kill row of any kind.** `ApoptoticKill` and `KillLimit` exist on this
  tower's tuning object and should both be left alone. §4c's organising
  decision is that only one of the four new towers is primarily a killer; the
  moment this one kills, it stops being the answer to a question no other
  tower answers.

---

## 3. The cited drawback

**Row 3, level 2: IL-15 is the game's best-evidenced "benefit with a real
cost", and the cost has a location rule, a mechanism and an off switch.**

The benefit is §2.3. The cost:

- **Epithelial IL-15 alone is already enough to cause disease.** Transgenic
  mice expressing human IL-15 **specifically in enterocytes** develop
  **villous atrophy and severe duodeno-jejunal inflammation**, with massive
  accumulation of NK-like CD8⁺ lymphocytes in the affected mucosa (Yokoyama
  et al., *PNAS* 2009, 106:15849–54,
  [DOI](https://doi.org/10.1073/pnas.0908834106)). Mouse, human IL-15
  transgene.
- **It is reversible.** In the same paper, antibody blockade of IL-15
  signalling (TM-β1, anti-IL-2/IL-15Rβ = anti-CD122) **reversed the
  autoimmune intestinal damage**. That is the off switch, and it is cited.
- **There is a location threshold, and it is the best thing here for a game.**
  In the DQ8-D-villin-IL-15 model, **overexpression of IL-15 in *both* the
  epithelium *and* the lamina propria is required for villous atrophy** —
  demonstrating "the location-dependent central role of IL-15". Tissue
  destruction additionally required HLA-DQ8 and gluten, with CD4 T cells and
  DQ8 **licensing** cytotoxic T cells to lyse enterocytes, plus roles for
  IFN-γ and transglutaminase 2 (Abadie et al., *Nature* 2020, 578:600–4,
  [DOI](https://doi.org/10.1038/s41586-020-2003-8); protocol in Abadie,
  Khosla & Jabri, *Curr Protoc* 2022,
  [DOI](https://doi.org/10.1002/cpz1.515)). Mouse carrying human HLA.
- **Quantifying the human risk:** IL-15 overexpression in gut epithelium and
  lamina propria is characteristic of **active coeliac disease**, and the
  refractory form is worse — in refractory coeliac disease type II, elevated
  intestinal IL-15 drives expansion of aberrant TCR-negative IELs, and **in
  up to 50% of RCD-II patients these cells develop into lymphoma** and can
  disseminate (Schmitz et al., *Mol Immunol* 2014, 58:10–6,
  [DOI](https://doi.org/10.1016/j.molimm.2013.10.014)). Human. That is the
  number: the end of the IL-15 road is a 50% lymphoma rate in the refractory
  population, not a vague "inflammation".

**How it should play.** This maps onto the game's existing objects almost
without invention:

| Level | What the player gets | What it costs |
|---|---|---|
| 1 — epithelial IL-15 | `MaxActiveChildren` +1 | nothing. Trans-presented from the wall; this is physiology. |
| 2 — **+ lamina propria IL-15** | `MaxActiveChildren` +1 again | **villous atrophy**: the wall's regrowth slows and its dead segments no longer heal on their own; and cytotoxic towers begin killing stressed-but-uninfected enterocytes. Refundable (anti-CD122). |

Three reasons this is the right drawback for this project rather than a
generic nerf:

1. **The threshold is the citation.** Level 1 safe / level 2 harmful is not a
   balance invention — Abadie 2020 is literally the experiment that showed
   one compartment is not enough and two are.
2. **It punishes the player through the thing they bought the tower for.**
   The γδ IEL is the repairer. Over-buying its population degrades the wall
   it repairs. The upgrade eats its own output.
3. **It is reversible, which almost no drawback in this game is**, and the
   reversal is cited (Yokoyama 2009). An expensive, cited "undo" is a more
   interesting object than a permanent trap — and it lets the Director test
   whether players will actually pay to walk a buff back.

**And a second, permanent drawback already sourced in the repo, which this
row should feed.** Chronic inflammation **permanently** reconfigures the γδ
IEL compartment: gluten-driven inflammation depletes the protective
Vγ4/Vδ1 subset and reduces epithelial BTNL8, gluten exclusion restores BTNL8
but **does not** restore the physiological subset (Mayassi 2019,
[DOI](https://doi.org/10.1016/j.cell.2018.12.039), human). So: if the board
spends long enough above the inflammation threshold, the γδ tower's
`MaxActiveChildren` cap should drop **permanently for the rest of the run**,
and no amount of later repair should bring it back. Pairing that with the
reversible IL-15 cost gives the tower a reversible mistake and an
irreversible one, which is a sharper lesson than either alone. There is a
further honest cost for Row 1's lever specifically: TNF administration causes
epithelial occludin endocytosis and **reduces γδ IEL migration** (Edelblum
2012, [DOI](https://doi.org/10.1073/pnas.1112519109)) — so inflammation
should also suppress the patrol radius the player paid for, temporarily.

---

## 4. Adjacent towers worth considering later

### ILC3 / IL-22 — **yes, and it is the strongest candidate in the game**

Verdict: build it, as a *tolerance* tower, and it comes with the only
well-evidenced innate memory in the roster plus a built-in difficulty hook.

In mice that lack ILC3s but keep endogenous T cells, TH17 and TH22 cells and
secondary lymphoid organs: ILC3s were **dispensable** for clearing a **low**
dose of *Citrobacter rodentium* and the mice survived — but ILC3s and
ILC3-derived IL-22 were still **required to maintain homeostatic functions of
the intestinal epithelium** even with IL-22-producing T cells present, they
promoted **pathogen tolerance** early by activating tissue-protective
pathways, and they were **indispensable for survival after high-dose
infection** (Araujo et al., *Sci Immunol* 2024, Consensus record
[link](https://consensus.app/papers/details/3ef88bcdd5b755cd9d69de93613dd55a/)).
A tower that does nothing on easy rounds and is the difference between
surviving and not on hard ones is a *difficulty curve expressed as a purchase*
— I have not seen a better fit for this game's economy.

Two supports. The topography is a real mechanic: CXCL16–CXCR6 controls where
IL-22-producing ILC3s sit, and Cxcr6⁻/⁻ mice selectively lose NKp46⁺ ILC3s,
lose intestinal IL-22, lose **antimicrobial peptide** expression and cannot
control *C. rodentium* (Satoh-Takayama et al., *Immunity* 2014, 41:776–88,
[DOI](https://doi.org/10.1016/j.immuni.2014.10.007)). And the memory is real:
intestinal ILC3s persist for **months** in an activated state after one
time-restricted *C. rodentium* exposure, and on rechallenge these trained
ILC3s proliferate, show enhanced IL-22 and control infection better than
naive ones (Serafini et al., *Science* 2022, Consensus record
[link](https://consensus.app/papers/details/f41262d887bb50238e48484138a17435/)).
All mouse. Human ILC3 trained immunity: **UNVERIFIED**.

Role separation from the γδ IEL is clean: γδ **mends broken wall**, ILC3
**keeps intact wall working under load** and raises antimicrobial peptide
output. Both defend without killing; neither is the other.

### Treg with alarmin-driven repair — **qualified yes, with a species flag that matters**

Verdict: the mouse result is one of the most elegant mechanics available; the
human translation currently does not hold. Build it only with that stated.

The mouse result is exactly what the repo already noted, and it is clean:
Treg-specific deletion of **amphiregulin** causes severe acute lung damage
and decreased blood oxygen during influenza **with no measurable change in
Treg suppressor function, antiviral immune responses or viral load** — and
this repair modality is mobilised by **IL-18 or the alarmin IL-33, not by TCR
signalling** (Arpaia et al., *Cell* 2015, Consensus record
[link](https://consensus.app/papers/details/884117e81c585026bc827fcb6626b5e8/)).
A tower powered by the *noise* of the player's own necrotic kills — i.e. a
fibrosis counterweight that the player charges by playing badly — is a great
object.

The flags, which are real and both matter:

1. **It is lung, not gut.** No gut equivalent of the AREG result was located.
2. **The human axis is not there.** Human Tregs in blood, tonsil, synovial
   fluid, **colon** and lung did **not express ST2** (the IL-33 receptor);
   AREG was produced by human Tregs but was **neither Treg-selective nor
   upregulated in tissues**; IL-33 did not affect human Treg AREG unless ST2
   was engineered in by lentivirus; and AREG potential was **lost upon TCR
   proliferation and differentiation** (Lam et al., *J Immunol* 2019,
   Consensus record
   [link](https://consensus.app/papers/details/20f08f93051653eda6f8f42e379058f2/)).

So "IL-33 alarm → Treg repairs tissue" is **mouse biology presented as human
physiology** if shipped unflagged. Given the project's standard, ship it as
"Treg (mouse evidence; the human IL-33/ST2 axis is not established)" or pick
a different repairer. The one genuinely human-safe piece is the *conceptual*
one — repair capacity sits in a non-effector, innate-like Treg phenotype and
is lost when the cell proliferates — which argues for a Treg tower whose
repair output **falls** as its population grows. That is an unusual and
honest upgrade curve: the only row in the game where buying more makes each
one worse.

---

## 5. What needs new mechanics

Ordered by how much they are load-bearing for the rows above.

1. **Suppression *strength* is not a field, and two of the four rows need it
   to be.** Rows 1 and 4 both buy coverage; nothing in the listed fields
   changes *how hard* a γδ IEL suppresses a charging plaque. Needs
   `ChargeSuppressionPerSecond` (or equivalent) on the tower's tuning
   object. Without it, Row 1 and Row 4 are two prices for one effect and the
   "wide-and-thin vs deep-and-narrow" decision in §2.1 collapses.
2. **Per-contact dwell time**, for Row 4's cited trade (faster ⇒ shorter
   dwell per enterocyte, Parthasarathy 2025). Currently suppression is
   presumably radius-gated and instantaneous, so speed is a pure buff. The
   build: track which wall segment each γδ IEL is currently *in*, and make
   suppression accrue per second of residence rather than per tick of
   proximity. This also makes the tower read better — the player would see it
   stop at the hot spot instead of orbiting it.
3. **The IL-15 drawback (§3)** needs three things:
   - a tissue-wide `EpithelialIL15Level` / `LaminaPropriaIL15Level` pair, so
     the level-1/level-2 location threshold is a state and not a flag;
   - a villous-atrophy effect on the wall — slower autonomous regrowth and a
     lower ceiling on barrier health — which is a *new* wall state distinct
     from §1b's "dead segment";
   - the reversal: a purchasable **anti-CD122 blockade** that refunds the
     level and clears the atrophy state. Small, and it is the most
     interesting button in the proposal.
   It should also switch on the already-queued Hosomi-2017 behaviour
   (`research_nk_unconventional.md`: stress sensors killing
   stressed-but-uninfected epithelium), since that is the same pathology.
4. **Permanent compartment degradation** (Mayassi 2019): a per-run counter
   of time spent above an inflammation threshold, which permanently lowers
   this tower's `MaxActiveChildren` ceiling and cannot be undone. Needs a
   notion of a tower cap that is *reduced below its purchased value*, which
   the per-progenitor cap model (§6d) does not currently express.
5. **BTNL3/BTNL8 as a wall upgrade** (§2.5): a §1d host-cell upgrade whose
   effect lands on a *different* object's tuning (the γδ tower's cap). No
   existing upgrade crosses objects, so this is a genuine new wiring, and it
   is the piece that makes the epithelium select its own garrison.
6. **Type I interferon as a tissue-wide state** rather than a purchase, so
   Row 4 reads off the board (already recommended in
   `research_nk_unconventional.md` for other reasons). Optional; it would
   make Row 4 the game's first conditional upgrade.

---

## 6. Corrections this pass owes the repo

Not applied — no repo file was edited. Three edits to queue:

- `docs/CELL_KINETICS_REFERENCE.md` motility table: γδ IEL measured speed is
  **~3.4 µm/min** (mouse, enteroid co-culture; 4.5 under IFNα), not
  UNVERIFIED. Game speed 1 stays, as a stated deliberate departure — and the
  note should now read "measured at DC pace; set to 1 because it is already
  at the wall", which is a stronger sentence than the current one.
- `docs/CELL_KINETICS_REFERENCE.md` and `GAME_DESIGN.md` §4c: add the repair
  magnitude (**22% / 14%**, Yang 2004) next to the existing 63% invasion
  figure, so the tower's two jobs each carry a number.
- `docs/research/research_nk_unconventional.md` §7c: the 63% result is from
  **CD103-KO**, and the file's phrasing ("more intercalation cut invasion
  ~63%") reads as an endorsement of CD103 loss as a lever. Add the three
  counter-findings in §2.1 above (Schön 1999, Enderle 2021, Dart 2023) so a
  future session does not build a "knock out CD103" upgrade.

---

## 7. Citation audit

Every DOI and PMID in this file was retrieved from PubMed or Consensus during
this pass, not recalled. Three identifiers I first wrote from memory turned
out to point at unrelated papers (a burn-injury HSV study, a community-
pharmacy survey, and a melanoma hypoxia paper) and were replaced with
verified records. The following are **not** peer-reviewed primary papers and
are labelled as such wherever they appear:

- Fischer et al. 2021 (*J Immunol* suppl) — conference abstract.
- Parthasarathy et al. 2025 (*Physiology*) — conference abstract; the record
  states the editorial board was not involved in peer review.
- Johnson et al. 2020 (*J Immunol* suppl) — conference abstract.

Reviews, flagged as reviews: Alonso & Edelblum 2023; Kapembwa et al. 2023.

Still **UNVERIFIED** after this pass:
- LFA-1/ICAM-1 as a γδ IEL surveillance axis — no supporting record found.
- A peer-reviewed full paper for the type I IFN → CD47 → γδ IEL motility
  result (Row 4's entire evidence base is abstracts).
- Human ILC3 trained immunity (the Serafini result is mouse).
- Any human study showing IL-15 elevation benignly expanding a protective
  γδ IEL population — the human IL-15 literature is pathology.
- The exact γδ fraction of human small-intestinal IELs (as the repo already
  notes).
