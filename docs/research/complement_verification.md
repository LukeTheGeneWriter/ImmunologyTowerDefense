# Verification: "gut C3 is local, gut lacks MAC" (Wu 2024, *Cell*)

Research pass, 2026-10-25. **According to PubMed** (plus PMC full text, Consensus, and the
Human Protein Atlas), all sources below were retrieved live during this pass and every DOI was
checked against the record rather than reconstructed. Nothing here is written from memory.

---

## 1. Verdict (read this first)

**Confirmed, with two corrections that matter more than the headline.** The paper is real and
says almost exactly what was reported: Wu M, Zheng W, Song X, *et al.* "Gut complement induced
by the microbiota combats pathogens and spares commensals." *Cell* 187(4):897–913.e18, 2024,
[doi:10.1016/j.cell.2023.12.036](https://doi.org/10.1016/j.cell.2023.12.036) (PMID 38280374,
PMC10922926). Its own abstract states "the local synthesis of complement component 3 (C3) in
the gut, primarily by stromal cells" and "The absence of membrane attack complex (MAC)
components in the gut ensures that C3 deposition does not result in the lysis of commensals."
**Correction 1: the evidence is colonic, not small-intestinal.** The words "small intestine,"
"ileum," "jejunum" and "duodenum" do not appear anywhere in the paper; every mouse experiment
is colon/feces, and the human figure legend says "human adult colon samples." **Correction 2:
the mechanism is not classical/antibody-driven.** Wu shows luminal C3 deposition on bacteria is
unchanged in C1q-deficient and MBL-deficient mice — it is alternative-pathway, and C1q is
*dispensable* for luminal pathogen defence. So the game's framing, "antibody destroys the
target's membrane directly, no immune cell present," is wrong twice over for a gut setting: the
lytic end of the cascade isn't locally made, and the antibody trigger isn't the gut's route to
C3 anyway. **Recommendation: re-specify the ~60% rung as opsonisation → phagocyte recruitment.**
Antibody + C3 coats the pathogen; the kill is delivered by a recruited neutrophil/macrophage.
That is literally the model Wu's title describes ("combats pathogens and spares commensals"),
it keeps the rung distinct from plain antibody binding, and it is defensible in the small
intestine even though Wu didn't test it there. A no-cell-present direct-lysis effect is only
honestly available against Gram-negatives in a plasma-exposed compartment, which the gut lumen
is not at homeostasis.

---

## 2. Evidence, by question

### Q1 — Does the paper exist and does it say this?

**Yes, verbatim.** Wu *et al.*, *Cell* 2024;187(4):897–913.e18,
[doi:10.1016/j.cell.2023.12.036](https://doi.org/10.1016/j.cell.2023.12.036). Kasper lab,
Harvard Medical School (Benoist, Mekalanos, Chiu co-authors). 85+ citations as of this pass.
It was reproducible by PubMed search — the earlier failure to find it was probably a query
problem, not a phantom citation.

Exact sentences from the full text (PMC10922926):

> "The genes for proteins downstream of C3, including complement MAC components C5, C6, C7, C8
> and C9 showed very low or undetectable levels of transcriptional expression."

> "Since components of the MAC (C5–C9) are not upregulated or secreted by cells in the colon
> during infection, we examined the role of opsonophagocytosis…"

> "However, the proteins forming the MAC are not expressed at a detectable level in the gut,
> neither in homeostasis nor infection. This suggests that bactericidal lysis by the MAC is not
> a function of the gut complement system, and this may be beneficial to spare the commensals
> microorganisms in the gut. Instead, the gut complement system appears to use
> complement-mediated phagocytosis as the major downstream pathway to clear pathogens.
> Phagocytes are not generally present in the gut lumen and neutrophils are only recruited to
> the gut [during infection]."

So the original claim was accurate as a report of the paper. The garbling, if any, is in the
scope the claim was given, not its content.

### Q2 — Is complement locally produced in the intestine, and by whom?

**Yes, and the question of which cell is genuinely contested.**

- **Wu 2024** ([doi:10.1016/j.cell.2023.12.036](https://doi.org/10.1016/j.cell.2023.12.036)):
  C3 is made locally, **primarily by stromal cells** in both mouse and human, with epithelium
  and CD45⁺ cells contributing less. They call this finding "unexpected." Luminal C3 is induced
  by commensal colonisation (germ-free mice lack it), is microbiota-composition-dependent, and
  varies between healthy human donors across a ~20-fold range (33–710 ng/g feces). Transplanting
  a high-C3 donor's community into germ-free mice transfers the high-C3 phenotype.
- **Other components:** C1q is high in gut **myeloid** cells; *Cfp* (properdin) and *C4b* are
  the only other highly-expressed components at homeostasis. In the human scRNA-seq, *CFH* and
  *CFD* are high in mesenchymal cells.
- **Competing attribution — epithelium:** Sünderhauf *et al.*, *Mol Immunol* 2017;90:227-238,
  [doi:10.1016/j.molimm.2017.08.003](https://doi.org/10.1016/j.molimm.2017.08.003) — mouse
  intestinal **epithelial** cell lines constitutively express high C3 mRNA, secrete and activate
  it, and upregulate it on LPS. Directly relevant regional note: **colonic primary IECs express
  more C3 than ileal primary IECs.** Satyam *et al.*, *J Immunol* 2017;198(2):788-797,
  [doi:10.4049/jimmunol.1502287](https://doi.org/10.4049/jimmunol.1502287) — Caco-2 intestinal
  epithelial cells make C3 and release C3a under hypoxia/LPS.
- **Hepatic/systemic spillover:** the canonical liver-plus-plasma system is still there in the
  *lamina propria* interstitium, which is perfused by plasma. Wu's claim is about the **lumen**
  and about local transcription, not about the submucosa. Keep that distinction.

**Bottom line for Q2:** local production is well supported and multi-group. Stromal-vs-epithelial
is unsettled, and it does not affect the game.

### Q3 — THE LOAD-BEARING ONE: is terminal complement absent in the gut?

Split the three possibilities the brief asked for:

**(a) Absent local gene expression — SUPPORTED, for the colon.**
Wu's mouse bulk RNA-seq of sorted epithelial, stromal and CD45⁺ colonic populations: *C5, C6,
C7, C8, C9* "very low or undetectable." Wu's human intestinal scRNA-seq (from the Human Gut
Cell Survey, Elmentaite *et al.*, *Nature* 2021,
[doi:10.1038/s41586-021-03852-1](https://doi.org/10.1038/s41586-021-03852-1)): **C7 *is*
detected, in mesenchymal and endothelial cells**; C5, C6, C8 and C9 are not. That is a real
asterisk on "lacks MAC components entirely" — one of the five is there in humans. C7 alone
cannot lyse anything, so the functional conclusion survives, but the absolute phrasing does not.

*Independent corroboration of (a):* the Human Protein Atlas consensus RNA classification lists
**C9 as "Tissue enriched (Liver)," Tau 0.98, "Detected in some"** and **C5 as "Tissue enriched
(Liver)," Tau 0.80, "Detected in many."** This is a separate dataset and a separate group, and
it agrees that terminal components are a hepatic product, not a gut product. Note C5's
"Detected in many" is weaker than C9's — consistent with Wu finding C5 undetectable in gut but
not with calling terminal expression zero everywhere outside liver.

**(b) Present but inhibited — ALSO TRUE, and Wu does not discuss it.**
Wu never mentions CD59, CD55/DAF, clusterin or vitronectin. But the gut epithelium is known to
carry terminal-pathway brakes:
- Uesu *et al.*, *Lab Invest* 1995;72(5):587-91 (PMID 7538183): in **normal** colonic epithelium
  DAF (CD55) and CD59/HRF20 are present and **confined to the apical domain** — i.e. facing the
  lumen; MCP (CD46) is basolateral. In active UC both DAF and CD59 are markedly **upregulated**
  in proportion to inflammation severity and spread to the basolateral membrane.
- Kiso *et al.*, *Histopathology* 2002,
  [doi:10.1046/j.1365-2559.2002.01350.x](https://doi.org/10.1046/j.1365-2559.2002.01350.x):
  DAF and CD59 intensely apical on intestinal-metaplasia and intestinal-type epithelium.

So the mucosa's protection of **host** cells against MAC is the classic regulator story, layered
on top of Wu's "the components aren't there" story. Two independent safety mechanisms.

**(c) Present and functional but the targets resist — TRUE for a large fraction of gut flora.**
See Q6. Gram-positives are MAC-resistant regardless of what the host expresses.

**The counter-evidence Wu does not cite, and it is the most important thing in this document:**
Halstensen, Mollnes, Garred, Fausa & Brandtzaeg demonstrated **terminal complement complex
(C5b-9) deposited on the luminal face of gut surface epithelium in vivo**, with
neoepitope-specific monoclonal antibodies:
- *Gastroenterology* 1990;98(5 Pt 1):1264-71,
  [doi:10.1016/0016-5085(90)90343-y](https://doi.org/10.1016/0016-5085%2890%2990343-y) — active
  ulcerative colitis: activated C3b apical on surface epithelium in 21/23 patients, with
  striking co-localisation of IgG1, C3b and TCC.
- *Immunol Res* 1991;10(3-4):485-92,
  [doi:10.1007/BF02919746](https://doi.org/10.1007/BF02919746) — C3b in 91% and **cytolytic TCC
  in 81%** of UC patients.
- *Gut* 1992;33(7):902-8, [doi:10.1136/gut.33.7.902](https://doi.org/10.1136/gut.33.7.902) —
  Crohn's: C3b and TCC granular at the luminal face of surface epithelium in 10/18 colitis
  patients, and in **8/14 with terminal ileitis** — i.e. **in the small intestine**.

Reconciling the two: Wu is about **homeostasis and acute enteric infection in the colon**,
where there is no local transcription of C5–C9 and no plasma leak. Halstensen is about
**chronically inflamed mucosa with a broken barrier**, where liver-derived terminal components
reach the epithelial surface from plasma and assemble. Both can be true. The correct statement
is therefore not "the gut has no MAC" but **"the healthy gut lumen has no locally made terminal
complement; MAC appears there only when the barrier fails and plasma leaks in, and when it does
it attacks the host epithelium, not the commensals."** That is a better fact than the original
claim and, incidentally, a better game beat.

### Q4 — How do commensals survive complement?

**Opsonisation-without-lysis is the accepted picture, and Wu's direct evidence for it is good:**
- C3-deficient mice vs WT littermates: **no difference in microbiota composition**, α- or
  β-diversity. Basal luminal C3 does not cull the flora.
- Commensals are nonetheless C3-coated; during *Citrobacter* infection, GFP⁺ bacteria in feces
  were demonstrably C3-coated by day 7.
- The coating is **alternative-pathway**: C3 deposition was unchanged in **C1qa-deficient** and
  **MBL-deficient** mice, and C1q-deficient mice were **not** more susceptible to enteropathogens.
- The effector is recruited cells: "Phagocytes are not generally present in the gut lumen and
  neutrophils are only recruited to the gut" on infection. Pathogen clearance in this model is
  opsonophagocytosis, not lysis.
- Independent, and **small-intestine-specific**: Vitari *et al.*, *Gut Microbes Rep* 2025,
  [doi:10.1080/29933935.2025.2527628](https://doi.org/10.1080/29933935.2025.2527628) — in
  morphine-induced dysbiosis, C1q and C3 protein and IgM/IgG rise in **ileal luminal content**
  within 24 h, microbiome-dependently, and the downstream event they observe is **neutrophil
  infiltration and activation**. Different lab (Roy, Miami), different model, same effector
  logic, and it is the ileum rather than the colon. Note this one *does* implicate the classical
  pathway, unlike Wu.
- Surface structure also matters: Ohno *et al.*, *PLoS Pathog* 2020,
  [doi:10.1371/journal.ppat.1008928](https://doi.org/10.1371/journal.ppat.1008928) — the LPS
  O-antigen of adherent-invasive *E. coli* confers complement resistance; a *wzy* mutant becomes
  complement-sensitive and is engulfed and killed by phagocytes. Again: phagocyte-delivered kill,
  C3-dependent.

### Q5 — Independent corroboration

Honest answer: **the specific "no MAC in the gut" finding rests substantially on Wu 2024 alone,
and the field has not yet replicated it.** What exists around it:

| Direction | Source | What it adds |
|---|---|---|
| Supports | Human Protein Atlas consensus RNA | C9 and C5 are liver-tissue-enriched; independent dataset, independent method |
| Supports (effector logic, ileum) | Vitari 2025, [doi:10.1080/29933935.2025.2527628](https://doi.org/10.1080/29933935.2025.2527628) | Local ileal complement → neutrophil recruitment, not lysis |
| Context, pre-dates Wu | Sina *et al.*, *Semin Immunol* 2018, [doi:10.1016/j.smim.2018.02.008](https://doi.org/10.1016/j.smim.2018.02.008) | Reviews intestinal complement in IBD; frames enterocyte complement as barrier-function machinery |
| Partly contradicts (source of C3) | Sünderhauf 2017 [doi:10.1016/j.molimm.2017.08.003](https://doi.org/10.1016/j.molimm.2017.08.003); Satyam 2017 [doi:10.4049/jimmunol.1502287](https://doi.org/10.4049/jimmunol.1502287) | Epithelium, not stroma, as a C3 source; ileal IECs < colonic IECs |
| **Contradicts the absolute claim** | Halstensen 1990 / 1991 / 1992 | C5b-9 TCC demonstrably deposits on gut epithelium in UC *and in terminal ileitis* |

No second group has repeated Wu's C5–C9 expression survey. Treat the claim as **a single strong
2024 paper from a top lab, consistent with liver-centric expression data, with 30-year-old
immunohistochemistry showing the exception case in inflamed tissue.**

### Q6 — Gram-positives and MAC; the Joiner 1983 figure

**Confirmed precisely, including the number.** Joiner K, Brown E, Hammer C, Warren K, Frank M.
"Studies on the mechanism of bacterial resistance to complement-mediated killing. III. C5b-9
deposits stably on rough and type 7 *S. pneumoniae* without causing bacterial killing."
*J Immunol* 1983;130(2):845-9, PMID 6848598 (pre-DOI; no DOI assigned). From the abstract:

> "Approximately 10,000 molecules of C7 and C9 bound per organism during the same period of
> incubation."

(Conditions: 5 × 10⁸ rough or type 7 pneumococci, 1 h in 10% pooled normal human serum. So the
figure is ~10,000 **each** of C7 and C9, not 10,000 total.) And the interpretation, in the
authors' own words:

> "Serum resistance in *S. pneumoniae* does not represent a failure to form C5b-9 on the
> bacterial cell wall but apparently reflects a failure of the bound complex to penetrate the
> thick peptidoglycan layer."

The previous pass's citation and its reading were both correct.

**Independent corroboration, two further groups:**
- Berends *et al.*, *Cell Microbiol* 2013,
  [doi:10.1111/cmi.12170](https://doi.org/10.1111/cmi.12170) — "Gram-positive bacteria are
  protected from MAC-dependent lysis by their thick peptidoglycan layer." They show SDS-stable
  polymeric C9 (true ring-form C5b-9) deposits on *S. pyogenes* and *B. subtilis* at specific
  sites (septum, poles) **without killing them** — non-lytic C5b-9.
- Bjånes & Nizet, *Microbiol Mol Biol Rev* 2021,
  [doi:10.1128/MMBR.00177-20](https://doi.org/10.1128/MMBR.00177-20) — review: "Gram-positive
  bacteria are inherently resistant to the direct bactericidal activity of complement due to
  their thick layer of cell wall peptidoglycan," while complement still flags them for
  opsonisation and phagocyte killing. The review is literally titled "More than a Pore."
- Dissent exists but is narrow: Alves & Freires, *J Dent Res* 2016;95(4):476 letter,
  [doi:10.1177/0022034515625747](https://doi.org/10.1177/0022034515625747), notes Jeanneau 2015
  reported pulp-fibroblast-derived MAC killing *S. mutans* under **serum-free** conditions, and
  flags it as discordant with Lambris 2008 / Berends 2013 / Merle 2015. One contested
  serum-free dental result does not move the consensus.

**Design-relevant extra:** the small-intestinal lumen is not Gram-negative territory. Jejunal
and ileal communities are dominated by streptococci and other oral-type **Gram-positives**
(*Streptococcus mitis* / *sanguinis* groups, *Granulicatella*, *Gemella*, *Veillonella*,
*Schaalia*), with Enterobacteriaceae as the main Gram-negative presence — Sundin *et al.*,
*BMC Microbiol* 2017, [doi:10.1186/s12866-017-1059-6](https://doi.org/10.1186/s12866-017-1059-6);
Villmones *et al.*, *Sci Rep* 2022,
[doi:10.1038/s41598-022-05723-9](https://doi.org/10.1038/s41598-022-05723-9); Yersin & Vonaesch,
*Trends Microbiol* 2024, [doi:10.1016/j.tim.2024.02.013](https://doi.org/10.1016/j.tim.2024.02.013).
Bacterial density is also low there — ~10³–10⁵ CFU/ml in the jejunum vs 10¹¹–10¹² in the colon.
So even if you handed the small intestine a full terminal cascade, most of what it met would be
structurally immune to it.

---

## 3. What is still uncertain

1. **Small intestine specifically: untested.** Wu's dataset is colonic. No paper found in this
   pass measures C5–C9 expression in healthy human duodenum, jejunum or ileum. The Human Gut
   Cell Survey that Wu drew on *does* cover up to 11 gut regions including small intestine, so
   the data to answer this exists — Wu just didn't report it by region, and the figure legend
   says colon. **This is a gap, not a confirmation.** It would be wrong to tell the Director
   "proven for the small intestine."
2. **C7 in humans.** Wu detects *C7* in human mesenchymal and endothelial cells. "Lacks MAC
   components entirely" is therefore not literally true in humans. Functionally irrelevant
   (C7 alone does nothing) but the absolute phrasing should be retired.
3. **Not replicated.** No independent group has repeated the C5–C9 gut expression survey since
   January 2024. Single-paper findings in high-profile journals sometimes narrow on replication.
4. **Protein vs transcript.** Wu's MAC-absence evidence is transcriptional (bulk RNA-seq and
   scRNA-seq). No mass spectrometry or western blot of luminal contents for C5–C9 appears in the
   paper (the word "proteomic" does not occur). Absence of transcript in three sorted populations
   is good evidence but is not the same as absence of protein in the lumen, where plasma-derived
   protein could in principle arrive without local transcription.
5. **Classical vs alternative in the small bowel.** Wu (colon) finds C1q and MBL dispensable;
   Vitari (ileum, dysbiosis) finds classical-pathway genes and C1q protein rising. These may
   genuinely differ by region or by stimulus. If the game ever wants an antibody-*triggered*
   complement beat in the ileum, Vitari is the citation that gets closest — but even there the
   output is neutrophil recruitment, not lysis.
6. **Regulators not profiled by Wu.** CD59/CD55/clusterin/vitronectin in the gut come from older
   immunohistochemistry (1995–2002), not from modern region-resolved transcriptomics. Their
   quantitative contribution relative to simple absence of C5–C9 is unmeasured.

---

## 4. One-line answer to the question the game actually asked

**In the small-intestinal mucosa and lumen, can antibody-triggered complement lyse a bacterium
with no immune cell involved?** On current evidence: **no, not as a normal mechanism.** The
terminal components aren't made there, the dominant commensals are MAC-proof Gram-positives
anyway, the gut's own C3 deposition is alternative-pathway rather than antibody-driven, and
every documented complement kill in the gut literature is delivered by a recruited phagocyte.
The one setting where MAC does appear on gut epithelium — chronically inflamed mucosa with a
leaking barrier — is MAC attacking the *host*, which is a different mechanic entirely.
