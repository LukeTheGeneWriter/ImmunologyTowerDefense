# Pathogen immune evasion — the counter space for named gut pathogens

**Status: research input, not design.** Companion to `docs/CELL_KINETICS_REFERENCE.md`.
Assembled 2026-10-25 against **PubMed** and **Consensus**. Every substantive claim
carries a citation; claims I could not resolve are marked **UNVERIFIED** and those
marks are load-bearing. Where the textbook story has weak in vivo support I say so —
there are four such corrections below and three of them change a planned mechanic.

Attribution: a substantial part of this document is based on articles retrieved from
**PubMed**, with DOI links given per claim.

Roster assumed: macrophage, neutrophil, dendritic cell, helper T, cytotoxic T cell
(CTL), NK/ILC1, γδ IEL, B cell → plasma cell → antibody. Ladder assumed: §5's six
rungs (10% CTL, 20% neutralising antibody, 30% memory T, 45% Fc receptor,
60% complement, 70% secretory IgA) + the ~90% capstone.

---

## 1. Counter-matrix

| # | Evasion strategy | Real pathogens that use it | Tower / rung that answers it | Is the answer real **in vivo**? | Does the game have an answer today? |
|---|---|---|---|---|---|
| A | **MHC-I downregulation** (CTL → exactly zero) | Rotavirus (IFN/STAT1 block → no MHC-I upregulation + a second non-transcriptional loss, Holloway 2018); *Salmonella*/*Yersinia*/*Klebsiella* (weak, HLA-B27-skewed, Kirveskari 1999) | NK/ILC1 "missing self"; γδ IEL stress sense | **Only proven for MCMV** (Parikh 2020). **Not** demonstrated for any enteric organism. For rotavirus the in vivo rescue is *antibody*, not NK (Franco 1995: β2m-/- mice clear it) | **Yes — two of them.** NK/ILC1 and γδ exist. But the *correct* answer for the gut is the B cell / IgA rung, which also exists |
| B | **Stress-ligand suppression / shedding** (NKG2D ligands cleaved off as soluble decoys) | *H. pylori* (Anthofer 2024); HCMV UL16 retains MICB intracellularly (Wu 2003) — mechanism template, not a gut organism | Nothing that senses by a *different* channel. γδ IELs in human gut are selected by **BTNL3/BTNL8**, not NKG2D (Dart 2023) — so a BTNL-reading γδ is the sourced escape hatch | Partly: *H. pylori* evidence is biopsy + cell culture. Soluble MIC degrading NKG2D on effectors is solid (Groh 2002) | **NO.** Turns off NK *and* γδ at once, and §4b's whole innate stress-sense roll with them. **This is the biggest gap.** |
| C | **IgA protease** | *Thomasclavelia ramosa*, *Bacteroides fragilis* M64 metallopeptidases (Márquez-Moñino 2025); classic IgA1 proteases are *Neisseria / Haemophilus / S. pneumoniae* — **respiratory & genital, not gut** | 70% secretory-IgA rung is what gets disabled. Nothing replaces it at depth 0 | **Yes, but off-target**: the clean in vivo demo is pneumococcus in mouse nasopharynx (Roche 2015). **Gut SIgA is mostly IgA2, whose short hinge resists these proteases** (Gleeson 2024) | **Partially.** Mucus/barrier upgrades and γδ patrol still act at the wall; nothing else acts in the lumen |
| D | **Antigenic variation within one infection** | *Giardia lamblia* — ~190 VSPs, one at a time, RNAi-controlled (Prucca 2008); genuinely required for persistence (Rivero 2010) | Knowledge step-down is the right abstraction. Counter = re-earning knowledge via the DC shuttle; innate towers are unaffected | **Yes, strongest in vivo case in this whole document** | **Yes** — the discount mechanic is already specified in §5 |
| D2 | **Between-infection antigenic drift / reassortment** | Norovirus GII.4 epochal evolution (Lindesmith 2008); rotavirus reassortment; *Campylobacter* phase variation (Skarp 2015); *Salmonella* flagellar phase variation | Memory T (30%) and banked antibody is what gets devalued | Real but operates on **years and populations**, not within one round | Yes, but it should be a *campaign* event, not a within-round one |
| E | **Complement / MAC resistance** | *Salmonella* long O-antigen + sialylated LPS + PgtE/Rck (Futoma-Kołoch 2015); *Yersinia* Ail recruiting C4BP (Ho 2014); **all Gram-positives structurally** (Joiner 1983) | 60% complement rung is what gets nullified; 45% Fc-receptor opsonisation still works | Yes for serum. **But the gut lumen has C3 and essentially no MAC at all** (Wu 2024, *Cell*) | **Yes** — Fc/opsonisation path survives. But see the correction: the 60% rung as written is wrong for this compartment |
| F | **Phagocyte subversion — shelter** | *Salmonella*: SCV biogenesis via SifA/SipA (Brawn 2007); non-replicating persisters formed immediately on macrophage uptake (Helaine 2014); rides CD18+ phagocytes out of the gut (Vazquez-Torres 1999) | NK/ILC1 IFN-γ aura upgrading macrophages — exactly the tower we already built, and the Lapaque 2009 result already in the repo | **Yes, strong** | **Yes** — and it is the roster's best-evidenced counter |
| G | **Phagocyte subversion — kill the phagocyte** | *Shigella* IpaB → caspase-1 → macrophage death (Hilbi 1998); *Listeria* LLO cytosol escape | Nothing. Tower death is a cost, not a counter | n/a | **NO.** The game has no "my tower was killed by the thing it ate" interaction |
| H | **NET / oxidative-burst resistance** | *V. cholerae* Dns + Xds nucleases: double mutant has a colonisation defect **that disappears in neutropenic mice** (Seper 2013) | Neutrophil is what gets blanked | **Yes** — the neutropenic-mouse control is the strong part | **Partially.** Macrophage/CTL still work; no NET mechanic exists to disable |
| I | **Exploiting inflammation — electron acceptors** | *Salmonella* tetrathionate from the neutrophil respiratory burst (Winter 2010); *E. coli* nitrate from iNOS (Winter 2013); *Salmonella* aerobic expansion when butyrate-producing Clostridia are lost (Rivera-Chávez 2016) | **Nothing — the counter is restraint.** Closest real answer: γδ IEL repair, Treg repair (queued), barrier integrity | **Yes, strong and repeated across three labs/axes** | **NO.** The game has no mechanic where immune activity *feeds* a pathogen |
| J | **Exploiting inflammation — neutrophil-opened door** | *Shigella flexneri*: PMN transmigration opens the paracellular route and *enables* apical invasion (Perdomo 1994, Sansonetti 1998) | Same — restraint; or γδ interception at the wall before PMNs arrive | Strong in vitro (T-84 monolayer, CD18-blockable) + consistent with in vivo crypt histology | **NO**, but it is the cheapest of the inflammation mechanics to build |

---

## 2. MHC-I downregulation, and whether NK really rescues it

### Which gut-relevant organisms actually do it

**Rotavirus — yes, and it is the best enteric case.** In HT-29 intestinal epithelial
cells, MHC-I was upregulated in *bystander* cells lacking rotavirus antigen but **not
in infected cells**; MHC-I and NLRC5 mRNA were elevated only in bystanders, and
infected cells additionally lost MHC-I protein by a second, non-transcriptional
route. The transcriptional half is downstream of rotavirus blocking STAT1 nuclear
translocation (Holloway 2018,
[DOI](https://doi.org/10.1038/s41598-017-18464-x)). The upstream IFN antagonism is
well mapped: NSP1 degrades IRF-3/5/7/β-TrCP and is a potent inhibitor of IRF-1 and of
**type III** IFN induction, which is the IFN that matters in intestinal epithelium
(Iaconis 2021, *Viruses*); VP3 degrades MAVS (Dai 2023, *mBio*).

Design note: the authors are explicitly cautious — "it is **possible** that rotavirus
**may** suppress MHCI expression in vivo." The in vivo confirmation is **UNVERIFIED**.

**Enteric bacteria — weak.** The only in vivo-ish report is 16 patients with
*Salmonella*, *Yersinia* or *Klebsiella* infection where MHC-I on PBMCs fell, but
only in HLA-B27-positive patients, with intracellular retention and unchanged mRNA
(Kirveskari 1999, [DOI](https://doi.org/10.1046/j.1365-2567.1999.00803.x)). This is a
reactive-arthritis story, not a general evasion mechanism, and it is on blood
monocytes, not enterocytes. **Do not build a bacterium around it.**

### Does NK actually rescue those infections in vivo? Mostly no — and the real answer surprised me

The textbook triangle (virus hides from CTL → NK sees missing self) **has exactly one
clean in vivo proof, and it is not an enteric organism.** Parikh et al. built mice
lacking specific inhibitory Ly49 receptors and showed MCMV control that is NK
dependent, requires an inhibitory ITIM motif, *and requires MCMV to be capable of
downregulating MHC-I* — when MCMV cannot downregulate MHC-I, CTLs take over and NK
contributes nothing (Parikh 2020,
[DOI](https://doi.org/10.1016/j.celrep.2020.107969)). That is as clean as the
hypothesis ever gets. It is murine cytomegalovirus, in a herpesvirus–Ly49 coevolved
system that **humans do not have** (humans use KIRs, differently).

For enteric viruses the picture is the opposite:

- **Rotavirus is cleared with zero MHC-I.** β2-microglobulin-knockout mice — no
  surface MHC-I, no CD8 T cells — shed rotavirus antigen **two days longer** than
  controls and then **completely resolved** primary infection; anti-CD8 treatment on
  top added one more day. Clearance tracked the appearance of intestinal
  rotavirus-specific **IgA**. B-cell-deficient (JHD) mice mostly cleared it too, but
  JHD + anti-CD8 mice became **chronically infected**, and JHD mice were fully
  susceptible on rechallenge (Franco & Greenberg 1995,
  [DOI](https://doi.org/10.1128/JVI.69.12.7800-7806.1995)).
- NK activity against rotavirus-infected targets in the gut is **in vitro only**, and
  the cleanest demonstration is chicken intraepithelial leukocytes killing
  rotavirus-infected chick kidney cells in a 4-hour chromium release assay — with the
  authors noting the activity was *not induced or altered by infection of the host*
  (Myers 1990, *Vet Immunol Immunopathol*). That is a plate assay, not a rescue.

**Caveat I must state, because it cuts the other way:** β2m-/- mice also have
hyporesponsive, unlicensed NK cells. So Franco & Greenberg does **not** prove "NK did
nothing." What it proves is stronger and simpler for our purposes: **in the gut, an
infection with zero MHC-I is still cleared, and the thing that clears it is
antibody.** CTL is dispensable; B cells are not.

**Verdict for the game.** The planned MHC-I stealth virus is sourced and the CTL cliff
is right (Halle 2016, [DOI](https://doi.org/10.1016/j.immuni.2016.01.010), already in
the repo). But "only the NK cell answers it" is **a tidier triangle than the gut
literature supports**. The honest version is a *two-answer* cliff: the stress sensors
give you a partial, slow answer (and the game's NK is an amplifier, not a killer,
which fits), and the **B-cell / IgA line is the answer that actually finishes it**.
That is a better puzzle anyway — it makes the 20% and 70% rungs matter against a
*virus*, which they currently do not.

---

## 3. Stress ligands — what turns them on, and who turns them off

### What induces NKG2D ligands (MICA/MICB/ULBPs; RAE-1/H60/MULT-1 in mouse)

Four independent inducers, all sourced:

1. **DNA damage response.** ATM/ATR-dependent induction of NKG2D ligands is the
   canonical "genotoxic stress alerts the innate system" pathway (Gasser & Raulet 2006,
   [DOI](https://doi.org/10.1016/j.semcancer.2006.07.004)).
2. **Direct bacterial adhesion — no infection required.** Diffusely adherent
   diarrhoeagenic *E. coli* induce surface MICA on epithelial lines, and the trigger
   is the specific **AfaE adhesin ↔ CD55 (decay-accelerating factor)** interaction. It
   is "extremely rapid after AfaE binding, consistent with a stress-induced signal,"
   and the induced MICA triggered IFN-γ release from an NKG2D+ NK line (Tieng 2002,
   [DOI](https://doi.org/10.1073/pnas.032668099)). **This is the single most
   game-shaped paper in this document**: adhesion itself lights the cell up, which is
   exactly our Step-1 adhesion phase.
3. **Physicochemical / desiccating stress**, via an NKG2D–RAE-1 axis that drives
   IFN-γ and CXCL9/10/11 independently of T cells (Coursey 2014,
   [DOI](https://doi.org/10.4049/jimmunol.1400016)); and epithelial stress ligands
   driving systemic Th2 "lymphoid stress surveillance" through intraepithelial T cells
   (Strid 2011, [DOI](https://doi.org/10.1126/science.1211250)).
4. **Baseline tone is set by the microbiota.** Intestinal epithelial cells
   *constitutively* express NKG2D ligands; germ-free and ampicillin-treated mice had
   **increased** expression, vancomycin and xylooligosaccharides (both expanding
   *Akkermansia muciniphila*) **decreased** it, and IL-10-deficient mice had a
   pronounced increase (Hansen 2013, *Eur J Immunol* — DOI **UNVERIFIED**, retrieved
   via Consensus). So stress-ligand display is a *regulated resting level*, not a
   binary infection flag. In Crohn's tissue, ligands are up on epithelium, monocytes
   and vascular endothelium, highly variable between patients, while NKG2D on the
   lymphocytes themselves is **down** (Vadstrup 2017,
   [DOI](https://doi.org/10.1016/j.yexmp.2017.06.010)).

**Important correction for the γδ tower specifically.** Human colonic γδ IELs are not
primarily NKG2D-driven. The tissue-protective subset is **CD103+ Vγ4+, selected and
maintained by epithelial butyrophilin-like heteromers BTNL3/BTNL8**; germline
BTNL3/BTNL8 hypomorphism depletes that subset and is a risk factor for penetrating
Crohn's (Dart 2023, [DOI](https://doi.org/10.1126/science.adh0301)). So γδ and NK read
*different channels*, and a pathogen that strips NKG2D ligands should **not**
automatically blank the γδ cell. That is a free, sourced design out.

### Who suppresses stress-ligand display — the counter-counter

**The gut-relevant one is *Helicobacter pylori*, and its mechanism is better than
simple hiding: it sheds.** In gastritis biopsies, NKG2D receptor expression was reduced
while NKG2D ligands *accumulated in the lamina propria*. In culture, *H. pylori*
induced both the transcription **and the proteolytic shedding** of NKG2D ligands from
gastric epithelial cells, in a way tied to specific virulence factors; the released
soluble ligands reduced the immunogenic visibility of infected cells **and** attenuated
the cytotoxic activity of effector cells (Anthofer 2024,
[DOI](https://doi.org/10.3389/fimmu.2024.1282680)).

Why shedding is the better mechanic: soluble MIC does not merely fail to be seen, it
**actively disarms the sensor**. Binding of soluble MICA induces **endocytosis and
degradation of NKG2D** on NK and CD8 T cells, with the downregulation measurable
systemically (Groh 2002, [DOI](https://doi.org/10.1038/nature01112)).

The classic viral mechanism, as a template: HCMV **UL16** physically binds newly
synthesised MICB and retains it in the trans-Golgi via a tyrosine motif, so MICB never
reaches the surface and NKG2D stimulation is abrogated (Wu 2003,
[DOI](https://doi.org/10.4049/jimmunol.170.8.4196)). Honest caveat the authors state
themselves: UL16 does **not** retain MICA, which is normally co-expressed, so even
this textbook case is a partial block. **No enteric virus with this mechanism was
found — UNVERIFIED.**

---

## 4. IgA proteases — the mechanic is real, but it mostly belongs to the wrong organisms

### The correction that matters most

Human IgA has two subclasses. **IgA1 has a long, O-glycosylated hinge that classic
bacterial IgA1 proteases cleave; IgA2 has a short hinge and is resistant to the
bacterial proteases prevalent at mucosal sites — and IgA2 is mostly confined to the
intestine** (Gleeson 2024, *Immunol Rev* — DOI **UNVERIFIED**, via Consensus;
corroborated by Canales-Herrerías 2022, *JCI*). The named IgA1-protease producers —
*Neisseria gonorrhoeae*, *N. meningitidis*, *Haemophilus influenzae*, *Streptococcus
pneumoniae* — are **respiratory and genital** colonisers (Plaut 1983, *Annu Rev
Microbiol*), i.e. exactly the compartments where IgA1 dominates. Veterinary analogue:
*Mannheimia haemolytica* (bovine respiratory) carries two (Ayalew 2019,
[DOI](https://doi.org/10.1016/j.vetmic.2019.108487)).

So the naive design — "a gut pathogen that cuts your secretory IgA" — is **pointing at
the wrong compartment**, and a biology-literate player could call it.

### What *does* cleave IgA in the gut

A distinct family: **M64 metallopeptidases of gut bacteria**. ThomasA from
*Thomasclavelia ramosa* cleaves IgA by exclusive recognition of the **Fab** region —
mechanistically distinct from the hinge-targeting classics, and therefore *not*
dodged by the IgA2 short hinge. BF3526 from *Bacteroides fragilis*, by contrast,
targets the N-terminus of **pre-digested** protein and does **not** act on intact IgA
(Márquez-Moñino 2025, [DOI](https://doi.org/10.1038/s44318-025-00518-w)). These are
commensals/pathobionts, not classic enteric pathogens — which is itself an interesting
design seat.

### Does it measurably help colonisation in vivo? Yes — and the experiment is a gift

The cleanest demonstration is in the **nasopharynx**, not the gut, but it maps onto our
mechanic one-for-one. Passive immunisation with monoclonal human IgA1 blocked
colonisation by an **IgA1-protease-deficient** pneumococcal mutant (which was
agglutinated) but **not** by the protease-producing wild-type parent (which was not
agglutinated). Protease-resistant **hIgA2 agglutinated and blocked both.** Protection
required **divalent** binding and was independent of Fc and complement — i.e. the
mechanism is *cross-linking*, not killing (Roche 2015,
[DOI](https://doi.org/10.1038/mi.2014.55)).

That is literally §5's 70% rung: coat → cross-link two organisms into a raft → swept
away, nothing killed. The protease's effect is to convert divalent IgA into monovalent
Fab fragments, so **coating still happens and cross-linking stops**. That is a far
better mechanic than "IgA does nothing": the coat visual survives, the raft never
forms.

**Giardia bonus.** Anti-VSP IgA is cytotoxic to Giardia trophozoites in suckling mice,
and **intestinal proteases** (host, not parasite) shape which variant types survive
(Müller & Gottstein 1998, [DOI](https://doi.org/10.1016/s0020-7519(98)00137-4)).

---

## 5. Antigenic variation — only one gut organism justifies a within-infection step-down

### *Giardia lamblia* — the one that genuinely does it, and the only one I'd build

Giardia carries a repertoire of **~190 variant-specific surface protein (VSP) genes**
and expresses **one at a time**; switching is spontaneous and is controlled by an
RNAi system (RdRP/Dicer/Argonaute) that silences all but the expressed *vsp*
(Prucca 2008, [DOI](https://doi.org/10.1038/nature07585)).

Crucially, there is a **loss-of-function in vivo experiment**: knocking down the RNAi
machinery produces trophozoites expressing many VSPs at once, and infecting gerbils
with those — or immunising with their purified VSPs — **protects against subsequent
Giardia infection**. The authors call it the first experimental evidence that antigenic
variation is *essential* for parasite survival in the host (Rivero 2010,
[DOI](https://doi.org/10.1038/nm.2141)). In vivo variation produces a *complex mixture*
of variant types rather than clean serial waves (Müller & Gottstein 1998,
[DOI](https://doi.org/10.1016/s0020-7519(98)00137-4)).

**Defensible discount:** one switch exposes a surface the host has never seen, out of
~190. A step-down to **near zero** on the antibody-dependent rungs (20/45/60/70) is
defensible for Giardia. It should **not** touch innate recognition, because VSP
switching does not change PAMPs. A **~70–90% knock-down of accumulated knowledge, on
antibody rungs only**, is the supportable number. Switching *frequency* per generation
is **UNVERIFIED** — published estimates vary by orders of magnitude across clones and
I could not pin one.

### Everything else varies between infections, not within one

- **Norovirus GII.4** shows *epochal* evolution: periods of stasis punctuated by rapid
  emergence of replacement strains, with changed HBGA-carbohydrate binding and
  strain-specific blockade responses — i.e. escape from **herd** immunity over
  ~years (Lindesmith 2008, [DOI](https://doi.org/10.1371/journal.pmed.0050031)).
- **Rotavirus** reassortment is segment-level and generates diversity across hosts and
  seasons, not within one patient's infection.
- **Bacterial phase variation** is real but narrow: *Campylobacter jejuni* serum
  resistance tracked phase variation of **wcbK** in the capsule locus (Skarp 2015,
  [DOI](https://doi.org/10.1038/srep17300)); *Salmonella* flagellar H1/H2 switching is
  the textbook invertible-element case. Both change *one* antigen, not the surface.

**Design consequence:** make the within-round step-down **Giardia-shaped and
Giardia-only**. For norovirus and rotavirus, a knowledge discount should fire
**between campaign waves** ("a drifted variant has arrived") — which is also the more
honest framing of the game's "an event, not continuous erosion."

---

## 6. Complement resistance — and the compartment problem

### Active resistance (Gram-negatives)

- ***Yersinia* Ail** binds **C4b-binding protein** and, with factor I, inactivates C4b
  that is both covalently bound to the bacterial surface and non-covalently bound to
  Ail itself — i.e. it recruits a host regulator and turns off the classical/lectin
  pathway at C4 (Ho 2014, PMID 24765656). *Y. pestis* here; Ail is conserved in
  enteropathogenic *Yersinia*.
- ***Salmonella*** — long, smooth O-antigen holds MAC away from the membrane;
  **sialylated LPS** (serogroup O48) reduces C3 deposition in proportion to sialic
  acid content; outer-membrane proteins **PgtE, PagC, Rck** contribute a
  serum-resistant phenotype (Futoma-Kołoch 2015,
  [DOI](https://doi.org/10.1186/s13099-015-0066-0)).

### Structural immunity (Gram-positives) — immune for a reason, and the reason is precise

Gram-positives are **not** immune because complement fails to assemble. On rough and
type-7 *S. pneumoniae* in 10% normal human serum, **~10,000 molecules each of C7 and
C9 bound per organism**, SDS-stable polymeric C9 included, and the organisms still were
not killed. The authors' conclusion: resistance "does not represent a failure to form
C5b-9 on the bacterial cell wall but apparently reflects a **failure of the bound
complex to penetrate the thick peptidoglycan layer**" (Joiner 1983, PMID 6848598).
Later imaging refined it: C5b-9 deposits at *specialised* sites — the division septum
on *S. pyogenes*, the poles on *B. subtilis* — unlike C3b, which deposits randomly
(Berends 2013, [DOI](https://doi.org/10.1111/cmi.12170)). And several Gram-positives
still bother to secrete MAC inhibitors, which argues the peptidoglycan wall is not a
complete shield.

So: **Gram-positives (e.g. *C. difficile*, *Listeria*, enterococci) are intrinsically
MAC-proof for structural reasons — no virulence gene required.** That is a free,
zero-cost, perfectly honest "this tower does nothing here" for an entire morphological
class.

### The correction the 60% rung needs

§5's 60% rung says "antibody destroys the target's membrane **directly**, with no
immune cell present." In the gut that is wrong. C3 is locally synthesised in the
intestine, mainly by stromal cells, induced by commensal colonisation, with
host-specific luminal levels — and **"the absence of membrane attack complex (MAC)
components in the gut ensures that C3 deposition does not result in the lysis of
commensals."** Basal C3 correlates directly with protection against enteric infection,
and the effector step is **recruiting neutrophils to the infection site** (Wu 2024,
*Cell*, [DOI](https://doi.org/10.1016/j.cell.2023.12.036)).

**The gut's complement is opsonic, not lytic.** The 60% rung should be "C3 coating
makes a pathogen a priority target and pulls phagocytes to it," which is also a better
game mechanic than passive damage — it couples a late rung to the towers the player
already owns instead of bypassing them.

---

## 7. Phagocyte subversion — who turns the macrophage into a shelter

### Shelter: *Salmonella*, and it is the best-evidenced item in the whole document

- **The vacuole is built, not survived.** SPI-1 effector **SipA** keeps being delivered
  *after* entry, is exposed on the cytoplasmic face of the Salmonella-containing
  vacuole, and cooperates with SPI-2's **SifA** to position the SCV perinuclearly and
  drive replication in macrophages (Brawn 2007,
  [DOI](https://doi.org/10.1016/j.chom.2007.02.001)).
- **Uptake itself creates a dormant reservoir.** Single-cell imaging showed a
  **non-replicating persister population formed immediately on macrophage uptake**,
  induced by vacuolar acidification and nutrient deprivation — the same cues that
  induce virulence genes — with most of 14 toxin-antitoxin modules contributing. Some
  persisters resumed growth after being phagocytosed by a *naive* macrophage
  (Helaine 2014, [DOI](https://doi.org/10.1126/science.1244705)).
- **The phagocyte is also the vehicle.** *Salmonella* is carried from gut to
  bloodstream by **CD18-expressing phagocytes**, and CD18-deficient mice resist
  dissemination to liver and spleen after oral challenge (Vazquez-Torres 1999,
  [DOI](https://doi.org/10.1038/44593)).

Three distinct, individually citable game behaviours: *eating it doesn't kill it*,
*eating it makes a dormant copy*, *eating it moves it toward your base*.

The roster's counter is already built and already sourced: **NK/ILC1 IFN-γ activating
macrophages** cut intramacrophage *Salmonella* load (Lapaque 2009, in
`CELL_KINETICS_REFERENCE.md`).

### Killing the phagocyte: *Shigella*

**IpaB binds caspase-1 directly** and is sufficient to induce macrophage death;
caspase-1-knockout macrophages are not susceptible although they respond normally to
other apoptotic stimuli; the pathway bypasses upstream caspases entirely
(Hilbi 1998, [DOI](https://doi.org/10.1074/jbc.273.49.32895)). In the mucosal sequence,
*Shigella* enters via M cells, then **kills the macrophages in the follicular dome,
releasing IL-1β, which initiates the inflammation that destabilises the epithelium**
(Sansonetti 1998, [DOI](https://doi.org/10.1007/BF02818608)) — note that this is
item 7's inversion arriving by a different road.

### Resisting NETs: *Vibrio cholerae*, with the right control

*V. cholerae* induces NET formation on contact with neutrophils and in return induces
two extracellular nucleases, **Dns and Xds**, which rapidly degrade the DNA scaffold.
The double-nuclease mutant has a **colonisation defect in immunocompetent mice that is
not evident in neutropenic mice** — the control that makes this an in vivo result and
not a plate artefact. Stated limitation: the authors could not visualise intact NETs in
the intestinal lumen of infected mice (Seper 2013,
[DOI](https://doi.org/10.1371/journal.ppat.1003614)).

Adjacent and useful for tuning: the mucosal chemokine **CCL28** both recruits
neutrophils and **boosts their ROS production and NET formation** via CCR3 — and
CCL28-deficient mice were *highly susceptible* to *Salmonella* gut infection but
*highly resistant* to otherwise lethal *Acinetobacter* lung infection (Walker 2024,
[DOI](https://doi.org/10.7554/eLife.78206)). The same neutrophil knob helps in one
place and kills in another. That is the game's inflammation tension, measured.

---

## 8. Exploiting inflammation — the strongest evidence here, and the game's blind spot

This is the item the brief flagged as most interesting, and the evidence is the most
solid of anything in sections 2–7. Three independent axes, all with genetic controls:

1. **Tetrathionate.** Reactive oxygen species generated during *Salmonella*-induced
   inflammation react with endogenous luminal thiosulphate to form **tetrathionate**, a
   respiratory electron acceptor. The genes for tetrathionate respiration confer a
   **growth advantage over the competing fermenting microbiota** in the inflamed gut
   lumen — i.e. *Salmonella*'s own virulence factors trigger the host to manufacture
   the pathogen's fuel (Winter 2010, [DOI](https://doi.org/10.1038/nature09415); review
   framing in Winter & Bäumler 2011, [DOI](https://doi.org/10.4161/gmic.2.1.14911)).
   The ROS in question come specifically from **neutrophils transmigrating into the
   lumen**.
2. **Nitrate.** Nitrate generated as a by-product of the inflammatory response confers a
   growth advantage on commensal *E. coli*; **iNOS-deficient mice do not support that
   growth**, proving the nitrate is host-derived (Winter 2013,
   [DOI](https://doi.org/10.1126/science.1232467)). This is the mechanism behind the
   Enterobacteriaceae blooms seen in intestinal inflammation generally.
3. **Oxygen.** Depleting butyrate-producing Clostridia raises epithelial oxygenation and
   permits **aerobic** luminal expansion of *Salmonella*; tributyrin restores epithelial
   hypoxia and restricts it. Critically, this happens **without antibiotics** in
   genetically resistant mice too — slower, and requiring functional *Salmonella* T3SS.
   "Salmonella virulence factors and antibiotic treatment promote pathogen expansion
   through the same mechanism" (Rivera-Chávez 2016,
   [DOI](https://doi.org/10.1016/j.chom.2016.03.004)).

And the fourth, mechanical rather than metabolic:

4. **Neutrophils open the door.** *Shigella flexneri* cannot invade a polarised colonic
   monolayer from the apical side. Apical bacteria signal basolateral PMNs to
   transmigrate; after ~1 h of transmigration, shigellae begin invading **in the areas
   of intense PMN infiltration**, and both the transmigration and the invasion are
   blocked by an anti-CD18 antibody (Perdomo 1994,
   [DOI](https://doi.org/10.1172/JCI117015)). The neutrophil response is the entry
   route.

**How solid?** Items 1–3 are Nature/Science/Cell Host & Microbe from one dominant lab
lineage (Bäumler) with mutant-and-knockout controls in each case; the nitrate result
has the cleanest host-side genetic control (iNOS-/-). Item 4 is in vitro but
CD18-blockable and consistent with in vivo crypt histology. **I would build on all
four.** The honest caveat is that 1–3 are mouse-gut-lumen findings about
*Enterobacteriaceae* competing with a *microbiota* the game does not model — so the
game's version has to express "outcompetes commensals" somehow, or convert the benefit
into straight replication-rate gain.

---

## 9. What this means for the game

**The three I would build first, and why:**

1. **Inflammation-feeder (`Salmonella`-shaped).** Build this first. It is the
   best-evidenced strategy in this document (Winter 2010/2013, Rivera-Chávez 2016), it
   is the one strategy the current roster has **no answer to at all**, and it inverts
   the game's core assumption in a way the player *feels* rather than reads: every
   neutrophil kill near it makes it replicate faster. Cheapest honest implementation —
   this species gains replication rate proportional to *recent nearby loud kills /
   neutrophil presence*, visible as the organism brightening when your line fires.
   Pairs with the existing debris/fibrosis loop rather than needing new systems.

2. **Stress-ligand shedder (`H. pylori`-shaped).** This is the real counter-counter and
   the **largest hole in the roster**. Under NKG2D-ligand shedding both the NK/ILC1 and
   §4b's innate stress-sense roll go dark, and soluble ligand **degrades the receptor
   on cells nearby** (Groh 2002) — so the natural mechanic is an *aura that disables
   your stress sensors in a radius*, which is visually and tactically unlike anything
   the game has. The sourced out is that human gut γδ IELs are **BTNL3/BTNL8-selected,
   not NKG2D-driven** (Dart 2023), so the γδ tower can keep working — giving the
   shedder exactly one answer, which is the design shape the Director wanted for the
   MHC-I virus.

3. **IgA-protease colonist — but built as a *cross-link* breaker, not an IgA-canceller.**
   Roche 2015 is the mechanic: protease-cleaved IgA still **coats**, it just cannot
   **agglutinate**, because protection requires divalence. So `SetCoated` fires, the
   visual survives, and the raft never forms. One flag on the existing Sprint 22 code
   path, and the player watches their working mechanic fail at the *second* stage
   rather than vanishing.

**Blunt bullets:**

4. **The planned MHC-I stealth virus needs its counter rewritten.** The cliff is right
   (Halle 2016). "Only NK answers it" is not: the single in vivo missing-self rescue is
   MCMV (Parikh 2020), a herpesvirus in a Ly49 system humans lack, while the enteric
   result is that **β2m-/- mice clear rotavirus anyway, via IgA** (Franco & Greenberg
   1995). Make it a **two-answer cliff**: NK/γδ slow it, B cells/IgA finish it. This
   gives the 20% and 70% rungs a job against viruses that they currently do not have.

5. **The 60% complement rung is wrong for this compartment and should be re-specified
   before anything is built on it.** The gut lumen has locally synthesised C3 and
   **essentially no MAC components** (Wu 2024, *Cell*). Direct antibody-driven lysis
   with no cell present does not happen there. Re-spec as **opsonisation + phagocyte
   recruitment**, which is both correct and a better rung (it compounds with owned
   towers instead of bypassing them).

6. **Gram-positive = MAC-immune, for free, with a crisp citation.** Joiner 1983 shows
   ~10,000 C9 molecules bind per pneumococcus and it still survives: the complex forms
   and cannot reach the membrane. A *C. difficile* or *Listeria* enemy can be "complement
   does literally nothing" with no virulence gene invented and no stat hidden.

7. **Within-infection antigenic variation is a Giardia mechanic and nothing else's.**
   ~190 VSPs, one expressed, RNAi-switched (Prucca 2008), and disrupting it makes the
   parasite *clearable and protective* in gerbils (Rivero 2010). Defensible discount:
   **70–90% knock-down of antibody-dependent rungs only, innate untouched.** Norovirus
   and rotavirus variation is between-season/between-host — fire it as a campaign event,
   not a round event.

8. **Adhesion itself should light a cell up.** Tieng 2002: the AfaE adhesin binding CD55
   induces surface MICA within minutes, no invasion needed, and that triggers IFN-γ from
   NK. The game already has a proximity-gated adhesion phase with a visible charge meter
   — this is a one-line justification for stress-signal emission *during adhesion*, which
   makes the γδ wall patrol's interception sourced rather than asserted.

**Strategies the current roster CANNOT answer (the gaps, ranked):**

- **Exploiting inflammation for fuel** (tetrathionate / nitrate / epithelial oxygen).
  No tower counters it; the counter is *not acting*, which the game has no vocabulary
  for. Candidate future towers: **Treg** (already queued in
  `CELL_KINETICS_REFERENCE.md`, alarmin-triggered, repair-not-suppression — a tower
  powered by your own loud kills) and a **barrier/hypoxia upgrade** standing in for
  butyrate-producing commensals.
- **Stress-ligand shedding.** Blanks NK *and* innate stress sense simultaneously. Partial
  out exists (BTNL-driven γδ), but nothing in the roster *restores* sensing. Candidate
  future tower: an **antibody-guided killer** — the 45% Fc rung as ADCC, which lets the
  NK cell kill by a route that does not need NKG2D at all.
- **Killing the phagocyte** (*Shigella* IpaB/caspase-1). The game has no interaction where
  engaging a pathogen destroys the engaging tower. This is a mechanic the roster cannot
  answer because it is not a *resistance*, it is a *counter-attack*.
- **NET / oxidative-burst degradation.** The neutrophil has no NET mechanic to disable, so
  a nuclease pathogen currently has nothing to switch off. Build NETs first, or skip.
- **Neutrophil-opened invasion route** (*Shigella*/Perdomo). No counter in the roster; γδ
  interception at the wall is the nearest candidate and would need the breach to be
  PMN-triggered, which §1b does not currently model.

---

## 10. Citations

PubMed-sourced, with DOIs as retrieved. Items marked **UNVERIFIED** were located via
Consensus and their DOIs were not resolved against PubMed.

| Claim | Source | Link |
|---|---|---|
| Rotavirus blocks MHC-I upregulation in infected enterocytes | Holloway 2018, *Sci Rep* | [DOI](https://doi.org/10.1038/s41598-017-18464-x) |
| Rotavirus NSP1 inhibits type I/III IFN, IRF-1 | Iaconis 2021, *Viruses* | DOI **UNVERIFIED** |
| Rotavirus VP3 degrades MAVS | Dai 2023, *mBio* | DOI **UNVERIFIED** |
| Enterobacterial infection reduces MHC-I (HLA-B27-skewed) | Kirveskari 1999, *Immunology* | [DOI](https://doi.org/10.1046/j.1365-2567.1999.00803.x) |
| Missing-self NK control of MCMV in vivo requires Ly49 + viral MHC-I downmodulation | Parikh 2020, *Cell Rep* | [DOI](https://doi.org/10.1016/j.celrep.2020.107969) |
| β2m-/- mice clear rotavirus; B cells required for immunity | Franco & Greenberg 1995, *J Virol* | [DOI](https://doi.org/10.1128/JVI.69.12.7800-7806.1995) |
| Chicken IELs kill rotavirus-infected targets in vitro | Myers 1990, *Vet Immunol Immunopathol* | DOI **UNVERIFIED** |
| CTL kill → 0.0 under MHC-I downmodulation (the cliff) | Halle 2016, *Immunity* | [DOI](https://doi.org/10.1016/j.immuni.2016.01.010) |
| DNA damage response induces NKG2D ligands | Gasser & Raulet 2006, *Semin Cancer Biol* | [DOI](https://doi.org/10.1016/j.semcancer.2006.07.004) |
| AfaE adhesin ↔ CD55 induces MICA on enterocytes | Tieng 2002, *PNAS* | [DOI](https://doi.org/10.1073/pnas.032668099) |
| Epithelial stress ligands drive lymphoid stress surveillance | Strid 2011, *Science* | [DOI](https://doi.org/10.1126/science.1211250) |
| Desiccating stress → NKG2D/RAE-1 → IFN-γ, CXCL9/10/11 | Coursey 2014, *J Immunol* | [DOI](https://doi.org/10.4049/jimmunol.1400016) |
| NKG2D ligands up / NKG2D down in Crohn's intestine | Vadstrup 2017, *Exp Mol Pathol* | [DOI](https://doi.org/10.1016/j.yexmp.2017.06.010) |
| Microbiota sets baseline epithelial NKG2D-ligand level | Hansen 2013, *Eur J Immunol* | DOI **UNVERIFIED** |
| Human gut γδ IELs are BTNL3/BTNL8-selected | Dart 2023, *Science* | [DOI](https://doi.org/10.1126/science.adh0301) |
| *H. pylori* sheds NKG2D ligands as soluble decoys | Anthofer 2024, *Front Immunol* | [DOI](https://doi.org/10.3389/fimmu.2024.1282680) |
| Soluble MIC causes NKG2D endocytosis and degradation | Groh 2002, *Nature* | [DOI](https://doi.org/10.1038/nature01112) |
| HCMV UL16 retains MICB intracellularly (but not MICA) | Wu 2003, *J Immunol* | [DOI](https://doi.org/10.4049/jimmunol.170.8.4196) |
| IgA2 short hinge resists mucosal bacterial proteases; IgA2 gut-confined | Gleeson 2024, *Immunol Rev* | DOI **UNVERIFIED** |
| IgA1 proteases are Neisseria/Haemophilus/pneumococcus | Plaut 1983, *Annu Rev Microbiol* | DOI **UNVERIFIED** |
| Gut M64 IgA peptidases (ThomasA Fab-directed; BF3526 not on intact IgA) | Márquez-Moñino 2025, *EMBO J* | [DOI](https://doi.org/10.1038/s44318-025-00518-w) |
| IgA1 protease defeats agglutination; IgA2 blocks both; divalence required | Roche 2015, *Mucosal Immunol* | [DOI](https://doi.org/10.1038/mi.2014.55) |
| *M. haemolytica* IgA1 + IgA2 proteases | Ayalew 2019, *Vet Microbiol* | [DOI](https://doi.org/10.1016/j.vetmic.2019.108487) |
| Giardia VSP switching is RNAi-regulated, ~190 genes | Prucca 2008, *Nature* | [DOI](https://doi.org/10.1038/nature07585) |
| Disrupting Giardia antigenic variation protects gerbils | Rivero 2010, *Nat Med* | [DOI](https://doi.org/10.1038/nm.2141) |
| Giardia in vivo variation; anti-VSP IgA cytotoxic | Müller & Gottstein 1998, *Int J Parasitol* | [DOI](https://doi.org/10.1016/s0020-7519(98)00137-4) |
| Norovirus GII.4 epochal evolution / antigenic drift | Lindesmith 2008, *PLoS Med* | [DOI](https://doi.org/10.1371/journal.pmed.0050031) |
| *C. jejuni* serum resistance tracks wcbK phase variation | Skarp 2015, *Sci Rep* | [DOI](https://doi.org/10.1038/srep17300) |
| C5b-9 binds pneumococci (~10⁴ C9) without killing — peptidoglycan blocks penetration | Joiner 1983, *J Immunol* | PMID 6848598 |
| C5b-9 deposits at specialised sites on Gram-positives | Berends 2013, *Cell Microbiol* | [DOI](https://doi.org/10.1111/cmi.12170) |
| *Yersinia* Ail recruits C4BP → factor-I inactivation of C4b | Ho 2014, *Eur J Immunol* | PMID 24765656 |
| *Salmonella* sialylated LPS + PgtE/PagC/Rck reduce C3 deposition | Futoma-Kołoch 2015, *Gut Pathog* | [DOI](https://doi.org/10.1186/s13099-015-0066-0) |
| Gut C3 is local, microbiota-induced, **MAC-free** | Wu 2024, *Cell* | [DOI](https://doi.org/10.1016/j.cell.2023.12.036) |
| SipA + SifA build and position the SCV | Brawn 2007, *Cell Host Microbe* | [DOI](https://doi.org/10.1016/j.chom.2007.02.001) |
| Macrophage uptake immediately makes *Salmonella* persisters | Helaine 2014, *Science* | [DOI](https://doi.org/10.1126/science.1244705) |
| *Salmonella* disseminates inside CD18+ phagocytes | Vazquez-Torres 1999, *Nature* | [DOI](https://doi.org/10.1038/44593) |
| *Shigella* IpaB binds caspase-1 and kills macrophages | Hilbi 1998, *JBC* | [DOI](https://doi.org/10.1074/jbc.273.49.32895) |
| *Shigella* M-cell entry → dome macrophage apoptosis → IL-1β → inflammation | Sansonetti 1998, *Folia Microbiol* | [DOI](https://doi.org/10.1007/BF02818608) |
| PMN transmigration promotes *Shigella* invasion (CD18-blockable) | Perdomo 1994, *J Clin Invest* | [DOI](https://doi.org/10.1172/JCI117015) |
| *V. cholerae* Dns/Xds degrade NETs; defect absent in neutropenic mice | Seper 2013, *PLoS Pathog* | [DOI](https://doi.org/10.1371/journal.ppat.1003614) |
| CCL28 boosts neutrophil ROS/NETs; helps in gut, harms in lung | Walker 2024, *eLife* | [DOI](https://doi.org/10.7554/eLife.78206) |
| Inflammation-derived tetrathionate lets *Salmonella* outgrow microbiota | Winter 2010, *Nature* | [DOI](https://doi.org/10.1038/nature09415) |
| Review framing of the tetrathionate result | Winter & Bäumler 2011, *Gut Microbes* | [DOI](https://doi.org/10.4161/gmic.2.1.14911) |
| Host iNOS-derived nitrate boosts *E. coli* in inflamed gut | Winter 2013, *Science* | [DOI](https://doi.org/10.1126/science.1232467) |
| Loss of butyrate-producing Clostridia → epithelial O₂ → aerobic *Salmonella* expansion | Rivera-Chávez 2016, *Cell Host Microbe* | [DOI](https://doi.org/10.1016/j.chom.2016.03.004) |

---

*Consensus search results informed parts of this document. Consensus asks that the
following be reproduced verbatim: "Create or connect a free Consensus account to return
more than 3 results per search in Claude Code.:
https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code"*
