# Enteric viruses — ranked shortlist for Immunology Tower Defense

Research pass, 2026-10-25. Every factual claim below was checked against PubMed
(via the PubMed MCP tool) or Consensus during this session; nothing is written
from memory. Where a number is commonly quoted in textbooks but I could not pull
a primary or review source for it in this pass, it is marked **UNVERIFIED**
rather than filled in. Full citations with links are at the bottom; inline
markers are `[n]`.

Read against `docs/GAME_DESIGN.md` §1b (adhesion / plaque / breach), §4a
(pathogen classes), §4b (stress signal, contact stress-sense roll, loud kill),
§5 (knowledge ladder; CTL at ~10%, neutralizing Ab ~20%, sIgA ~70%).

---

## 1. The shortlist, ranked

Six. Ranked by *how much new decision-making each one forces on the player*, not
by how famous or how deadly it is. Anything that would play as "norovirus but
slightly different" was cut (see §5).

| # | Virus | The ONE mechanic nothing else brings | Why it earns a slot |
|---|---|---|---|
| 1 | **Rotavirus A** | **Positional tropism.** Infects *mature enterocytes at the villus tip only*; the Lgr5+ crypt stem cell is never infected, and the crypt responds by hyper-proliferating to repave the villus [1][2]. | This is the one virus whose damage has a *geometry*. The board heals from the crypt upward whatever you do, so the question becomes "can you hold the tip long enough for the crypt to resupply it" — a tower-defense question, not a damage-race one. Also brings NSP4, a genuine viral enterotoxin: damage leaks into *uninfected* neighbours [3][4]. |
| 2 | **Human cytomegalovirus (CMV colitis)** | **Total, non-graded MHC-I blindness.** Four independent, redundant viral proteins (US2, US3, US6, US11) each sufficient alone to strip MHC-I from the surface [5][6][7]. | The stealth boss. §4b/§5 want a *cliff* — CTL damage exactly zero, not a stealth percentage — and CMV is the only enteric candidate where the biology justifies a cliff rather than a slider. Also the slowest clock available (~96 h lytic cycle [8]) and it sits in gut *endothelium and stroma*, not epithelium [9][10] — i.e. already behind the wall, untouchable by anything that acts in the lumen. |
| 3 | **Human norovirus (GII.4)** | **Antigenic replacement as a scheduled event.** GII.4 is periodically and near-completely re-antigenised; monoclonals and human sera that blockade the 2009 variant lose reactivity against the 2012 variant outright [11][12][13]. | §5's "pathogen mutation applies a discrete step-change discount to knowledge" is not an abstraction for norovirus — it is literally what GII.4 does, on a ~2–4 year beat. Pair that with the lowest practical infectious dose [14][15], weeks of shedding at 10⁵–10⁹/g including from people with no symptoms [16][17], and disinfectant resistance [18][19], and you have the lumen-pressure virus: the one that never stops arriving. |
| 4 | **Human astrovirus** | **Breach without infection.** The capsid alone, with no replication at all — UV-killed virus, or purified recombinant capsid VLPs — opens tight junctions and raises epithelial permeability, with *no cell death and no inflammation* [20][21][22]. | Nothing else on this list can damage the wall while emitting no stress signal and no DAMP. In §4b terms, astrovirus is an infection the contact stress-sense roll can never find, because the cell it's hurting isn't infected. It drives the §1b plaque charge directly and leaves the player's whole detection stack with nothing to bite on. |
| 5 | **Enteric adenovirus F41** | **The anti-stealth: NK-blind, CTL-visible.** Species F adenoviruses are the ones that *lack* E3-19K [23][24], so they do not do the classic adenoviral MHC-I retention — but F41 induces MICA/MICB and then holds MICB *inside* the cell, keeping the NKG2D stress ligand off the surface [25]. | Exact inversion of CMV, from real biology. If CMV is the enemy only NK/stress-sensors can kill, F41 is the enemy only CTLs can kill. Two bosses that teach the same lesson from opposite ends is worth far more than two bosses that teach it twice. Bonus: F41 is *not* neutralised by the Paneth-cell α-defensin HD5, while respiratory Ad5 is [26] — it walks past the crypt's chemical defence. |
| 6 | **Hepatitis A virus** | **Antibody-proof cloaked egress.** HAV leaves cells non-lytically wrapped in host membrane ("eHAV"); the quasi-enveloped particle is fully infectious and *resistant to neutralising antibody* [27][28]. | This is a hard counter aimed squarely at what the game just shipped. Sprint 22 built sIgA coating/cross-linking and Sprint 23 built plasma-cell antibody coating; HAV is the real organism that those two towers cannot touch. Also the longest fuse in enteric virology: mean 28-day incubation, maximal shedding *before* symptoms [29][30]. |

**Cut, and why:** sapovirus (plays as norovirus with lower numbers), hepatitis E
(same quasi-envelope trick as HAV, same hepatotropism — redundant with #6),
enterovirus/coxsackievirus B (strong candidate, see §5; held back only because
its two best mechanics partly overlap #4 and #2), rotavirus-adjacent Aichi
virus (1.45% global prevalence [31] — too rare to carry a slot).

---

## 2. Numbers

| Virus | Infectious dose (ID50) | Incubation | Burst size (virions/cell) | Eclipse / latent period | Source |
|---|---|---|---|---|---|
| **Rotavirus A** | **~1 focus-forming unit.** Human volunteers infected at doses down to the limit of culture detection across 9×10⁻³–9×10⁴ ffu [32]; 1 PFU sufficed in gnotobiotic piglets [33] | ~2 days [34] | **UNVERIFIED** for human RV; per-cell reproduction number modelled as ≫1 [35] | **~2–3 h** to first progeny genome; peak yield 10–12 h (bovine RV, BSC-1) [36] | [32][33][34][35][36] |
| **Human norovirus GI.1 (Norwalk)** | **HID50 1320–2800 genome equivalents** [14]; statistically consistent with the earlier high-infectivity (~18-particle aggregate-corrected) model — the difference was statistical method, not biology [15] | **UNVERIFIED** in this pass (commonly 12–48 h); shedding median ~10 d, mean duration range 8–60 d, peak 10⁵–10⁹/g faeces, symptomatic ≈ asymptomatic [16][17] | **UNVERIFIED** | **UNVERIFIED** | [14][15][16][17] |
| **Human norovirus GII.2 (Snow Mountain)** | **5.1×10⁵ genome-equivalent copies** (controlled human challenge, 44 subjects) [37] | as above | **UNVERIFIED** | **UNVERIFIED** | [37] |
| **Human norovirus GII.4** | **2400–3400 RNA copies** (gnotobiotic pig; diarrhoea dose 21,000–38,000) [38] | as above | **UNVERIFIED** | **UNVERIFIED** | [38] |
| **Human astrovirus** | **UNVERIFIED** | **UNVERIFIED** | **UNVERIFIED** | **UNVERIFIED** — note the pathogenic mechanic does not require any replication at all [20][21] | [20][21] |
| **Enteric adenovirus F41** | **UNVERIFIED** | **UNVERIFIED** | **UNVERIFIED** for species F | **UNVERIFIED** for species F; replicates productively in human enteroids and is restricted by type I *and* type III IFN there [26] | [26] |
| **Hepatitis A virus** | **UNVERIFIED** | **Mean 28 days (range 15–50)**; infectious from ~2 weeks before symptoms, peak shedding pre-jaundice [29][30] | **UNVERIFIED** | **UNVERIFIED** | [29][30] |
| **Human CMV** | n/a — gut disease is reactivation, not an ingested dose | n/a | **UNVERIFIED** | **Whole lytic cycle ~96 h**; true-late virion genes peak ~72 h p.i. [8][39] | [8][39] |

### Anchors for burst size and skew (use these, not guesses)

Because per-cell burst numbers are genuinely missing for most enteric viruses, two
measured anchors are worth more than invented per-species values:

- **Poliovirus in monkey kidney cells (the best-characterised picornavirus
  one-step curve):** >95% of adsorbed particles enter eclipse; **eclipse 3–4 h**;
  release begins after ~10% of total progeny has formed; final yield
  **700–1400 infectious particles per cell** over 12–14 h [40]. Sub-optimal pH
  or temperature *prolongs the eclipse* (6 h at pH 6.0–6.5, >9 h at pH 5.0–5.5)
  without abolishing it — a real, usable "hostile environment slows the fuse"
  mechanic.
- **Burst size is violently skewed, not fixed.** Single-cell measurement across
  tens of thousands of influenza-infected cells gives a range of **10¹–10⁴**
  virions per cell, with **~10% of infected cells producing ~40–60% of all
  progeny** [41]. Mean burst was 709 (H3N2) / 358 (H1N1) — i.e. the mean is
  nowhere near the mode. Formally, with an eclipse phase and constant release
  the per-cell reproduction number is **negative-binomially** distributed [42].

---

## 3. Per-virus detail

### 3.1 Rotavirus A — rank 1

**Tropism.** Mature enterocytes, villus tip, sparing the crypt. Single-cell
sequencing of rotavirus-infected intestinal epithelium confirms damage is at the
villus tip and does *not* directly damage the intestinal stem cell; specific
Lgr5+ subsets then cycle faster, the crypt expands, and immature enterocytes are
pushed up to repave the villus [1]. Unexpectedly, that same study found
**rotavirus also infects tuft cells**, which respond through interferon pathways
[1]. In human intestinal enteroids, human rotavirus infects **differentiated
enterocytes and enteroendocrine cells** [2]; review-level consensus is "primarily
mature enterocytes, causing villus atrophy, enhanced turnover and apoptosis" [3].

**Dose and timing.** Effectively the lowest dose of anything here: in 62 adult
volunteers given 9×10⁻³ to 9×10⁴ ffu, the amount required to infect was
comparable to the minimum detectable in primary monkey kidney culture — i.e.
roughly one culturable unit [32]. 1 PFU infected colostrum-deprived piglets [33].
Pre-existing serum neutralising antibody did not correlate with protection in the
1986 study [32]; a follow-up found serum rotavirus IgG and jejunal neutralising
antibody *were* associated with protection, and that probability of infection was
unrelated to dose over the range tested [43]. Incubation ~2 days [34].

**Spread and kinetics.** Latent period ~2–3 h to first progeny genome, peak yield
at 10–12 h, host protein synthesis shut off by 4 h (bovine RV in BSC-1) [36]. Per-cell
reproduction number ≫1 [35]. Per-cell burst count for human RV: **UNVERIFIED**.

**Immune evasion.** Two separate, well-sourced hits on the game's own mechanics:
- **NSP1 is a broad IRF antagonist** — it targets IRF3, IRF5 and IRF7 (and in
  some strains β-TrCP) for proteasomal degradation [44][45], via a BC-box motif
  recruiting Elongin BC [46]. Human RV strains lean on IRF5/IRF7; animal strains
  hit IRF3 as well [45].
- **Partial MHC-I suppression.** In HT-29 intestinal cells, MHC-I is upregulated
  on *bystander* cells but not on infected ones, because RV blocks IFN-induced
  STAT1 nuclear translocation — plus an additional non-transcriptional mechanism
  [47]. In porcine cells, RV degrades β2-microglobulin through the ERAD-proteasome
  pathway, cutting surface MHC-I [48].
  **Caveat that matters for the game:** this is *partial*. CD8 T cells still
  demonstrably contribute to clearance in vivo (below). Rotavirus is emphatically
  **not** the stealth virus.

**Clearance — genuinely redundant, and genuinely contested.** β2m-knockout mice
(no MHC-I) still completely resolve primary infection, only ~2 days late, and are
fully resistant on rechallenge; B-cell-deficient mice mostly clear it too, but
B-cell-deficient mice *depleted of CD8* become chronically infected [49].
A second group reached the same place from the other side and concluded a
**third, unidentified effector mechanism** also contributes [50]. Adoptive
transfer of immune CD8 T cells clears chronic infection in SCID mice with no
antibody present [51]. **Reading:** no single effector is required; CD8 T cells
and intestinal IgA are each sufficient-ish and partially redundant, and antibody
is what prevents *re*infection [49]. Trust the mouse here — it is the only system
with the genetics, and the three studies agree.

**Structurally odd.** NSP4 is a true viral enterotoxin — the secretory-diarrhoea
component is pharmacological, not cytopathic [3][4]. And the villus-tip/crypt
split is the single most game-legible fact in this whole document.

---

### 3.2 Human cytomegalovirus (CMV colitis) — rank 2, and the stealth recommendation

**Tropism — not epithelial, and that is the point.** In 32 HIV/AIDS patients with
GI CMV, inclusion bodies were found in **epithelial cells and mesenchymal cells
(histiocytes, smooth muscle, fibroblasts, endothelium)**, with mesenchymal cells
significantly *outnumbering* epithelial cells everywhere except the duodenum,
where the ratio flips [9]. An older dual-label study found 35–60% of infected
cells in GI resections were factor-VIII-positive endothelium, with infected cells
clustered around vessels, concluding GI CMV is "primarily vasculitic" [10]. CMV
end-organ disease also occurs in previously immunocompetent critically ill ICU
patients, involving endothelial, stromal and/or epithelial cells [52].

**Immune evasion — the reason it gets the stealth slot.**
- **Four independent MHC-I mechanisms, each sufficient alone.** US2, US3, US6 and
  US11 "each independently sufficient for causing the down-regulation of MHC class
  I molecules on the cell surface" [5]. Mechanistically distinct: US3 retains
  class I in the ER, US2 and US11 drive heavy-chain destruction (US2 via the E3
  ligase TRC8) [6][7]. Co-expression is cooperative [6].
- **The paper that names the design directly:** this down-regulation "allows
  infected cells to evade recognition by cytotoxic T cells **but leaves them
  susceptible to NK cells, which lyse cells that lack class I molecules**" [5].
  That is the game's intended mechanic, stated in the literature.
- **Honest complication.** CMV then fights the NK answer too. Immediate-early
  genes *upregulate* NKG2D ligands (IE1→ULBP2, IE2→MICA/B), and the virus
  suppresses them again with UL16 (ULBP1/2, MICB), UL142 (MICA), US18+US20 (MICA
  via lysosomal degradation) and miR-UL112-1 (MICB) [53][54][55]. It also carries
  UL18, an MHC-I homolog that is *resistant* to US2/3/6/11 and plausibly damps NK
  "missing-self" [5].

**Kinetics.** Protracted: full lytic replication ~96 h [8], true-late virion genes
not accumulating until ~72 h p.i. [39].

**Clearance.** Not resolved in this pass beyond the strong clinical signal that
CMV gut disease is a disease of *T-cell* deficiency (AIDS, transplant, high-dose
immunosuppression) [9][52] — **UNVERIFIED** as a mechanistic statement.

---

### 3.3 Human norovirus GII.4 — rank 3

**Tropism — this is the one that was revised, and the revision is mouse-specific.**
Get this right in the design doc, because the famous answer is the wrong species:

- **Murine norovirus (MNV): tuft cells.** CD300lf is the MNV receptor, and
  intestinal tuft cells — a rare chemosensory epithelial cell — express it and are
  the target cell in the mouse intestine [56]. Type-2 cytokines that expand tuft
  cells *promote* infection [56]. The persistent strain CR6 lives exclusively in
  tuft cells [57].
- **Human norovirus (HuNoV): enterocytes, plus enteroendocrine cells in vivo.**
  HuNoV replicates in **enterocytes** in stem-cell-derived human intestinal
  enteroid monolayers; **bile is required** for strain-dependent replication, and
  lack of histo-blood-group-antigen expression blocks it [58][59]. In four
  immunocompromised patients, negative-sense (replicating) viral RNA was
  predominantly in intestinal epithelial cells, with **chromogranin-A-positive
  enteroendocrine cells** identified as a permissive type [60]. A zebrafish-larva
  model additionally identifies intestinal **macrophages** as hosts (~23% of host
  cells) [61] — interesting, non-mammalian, treat as unsettled.
- **M cells matter for entry regardless of target cell.** M-cell-depleted mice
  show reduced MNV *and* reovirus replication, despite the two viruses replicating
  in different cell types [62]. So "M cell as the doorway" is defensible for the
  game independent of which cell ends up infected.

**Dose.** See §2. The honest summary: Norwalk GI.1 HID50 is ~1300–2800 genome
equivalents [14], and the apparent conflict with the older "~18 particles" figure
is a statistical-model artefact, not a biological disagreement — the two datasets
pool without loss of fit [15]. Different genogroups differ enormously: GII.2
Snow Mountain needed 5.1×10⁵ GEC to hit ID50 [37].

**Immune evasion.** MNV reduces surface MHC-I via NS3 (endocytic recycling plus
some proteasomal degradation) and thereby blocks CD8 T-cell activation [63]; MNV
also downregulates antigen-presentation and proteasome-subunit transcripts [64]
and antagonises **B-cell** antigen presentation via VP2 [65]. All mouse. All
*partial*. Not a stealth virus.

**Clearance — human and mouse diverge; trust the human.** Mouse work says T cells
prevent persistence; the mouse *also* says tuft cells are an immune-privileged
niche in which JEDI CD8 T cells partially fail to kill even a virus-independent
antigen, with the T cells adopting a low-cytotoxicity resident-memory phenotype,
so CTLs neither clear nor prevent MNV^CR6 in the colon [57]. In **humans**, two
patients with inborn errors of immunity and chronic norovirus cleared the
infection on reconstitution of polyfunctional norovirus-specific CD4 **and** CD8
T cells after HSCT, with **no B-cell reconstitution and no new antibody** — the
authors conclude cellular immunity is sufficient [66]. Humans also carry
norovirus-specific CD8 T cells at seven immunodominant GII.4 epitopes,
most abundantly in the intestinal lamina propria [67]. Reviews of chronic
norovirus in the immunocompromised agree T-cell function is the key [68].
**Reading: in humans, T cells clear norovirus.** Do not import the mouse tuft-cell
privilege result into a human-gut game as if it were settled for HuNoV — it is a
property of mouse tuft cells and a mouse-specific receptor.

**Structurally odd.** (a) Antigenic replacement [11][12][13]; (b) weeks of
shedding at high titre from asymptomatic people [16][17]; (c) environmental
toughness — detectable 60–728 days in water, resistant on carpet, Formica,
stainless steel, PVC and ceramic, through shellfish depuration and freeze-thaw;
hydrogen peroxide, quaternary ammonium and most ethanol antiseptics are
ineffective at normal concentrations [18], and HuNoV appears *more* resistant to
heat and free chlorine than its cultivable surrogates [19]; (d) secretor status —
HBGA expression gates susceptibility [58].

---

### 3.4 Human astrovirus — rank 4

**The mechanic, in one line:** the capsid is an enterotoxin.

In differentiated Caco-2 monolayers, astrovirus infection increases barrier
permeability, disrupts the tight-junction protein occludin and reduces actin
stress fibres **in the absence of cell death**. Crucially the same effect is
produced by **UV-inactivated virus** and by **purified recombinant HAstV-1 capsid
VLPs** — so permeability loss is independent of viral replication and is driven by
the capsid protein, "a property apparently unique to astroviruses" [20].
In vivo: oral administration of purified recombinant turkey astrovirus-2 capsid
protein alone causes dose- and time-dependent acute diarrhoea in turkey poults,
again with **no inflammation and no histological change**, and with NHE3
redistributed from membrane to cytoplasm [21]. The field review states the same:
astrovirus infection shows few histological changes and little inflammation, and
the capsid acts as an enterotoxin disrupting the gut epithelial barrier [22].

**Tropism, dose, burst, eclipse, clearance:** **UNVERIFIED** in this pass. The
reviews are explicit that human astroviruses are "among the least characterised
enteric RNA viruses" and that better animal models are needed [22]. Build it for
the barrier mechanic, and do not invent numbers for the rest.

---

### 3.5 Enteric adenovirus F41 — rank 5

**The correction worth having.** The famous adenoviral CTL-evasion protein is
E3-19K, which binds MHC-I and TAP and retains class I in the ER, protecting
infected cells from CTL lysis [69][70]. **Species F — the enteric adenoviruses 40
and 41 — do not encode E3-19K** [23][24]. Subgenus A (e.g. Ad12) instead represses
MHC-I heavy chain, TAP and LMP transcription via E1A [23]. Species F carries two
unique, largely uncharacterised E3 genes (E3-19.4K, E3-31.6K) in a markedly
divergent E3 region [24][25], and recent phylogeny shows an emerging F41 Lineage 3
with significant evolution in the short-fibre and E3 regions [71]. So: **do not
build Ad41 as an MHC-I stealth virus.** The evidence points the other way.

**What it does do.** HAdV-F41 infection of human intestinal HCT116 cells
**upregulates MICA and MICB** expression, but MICB is **largely sequestered
intracellularly** rather than reaching the surface — a plausible NK/NKG2D evasion
route in the gut [25]. (The authors flag this as preliminary and did not see the
same for MICA.)

**Other properties.** Both prototype and clinical isolates of enteric and
non-enteric HAdVs replicate productively in human enteroids; HAdV-41 is sensitive
to type I **and** type III interferon in enteroid monolayers but not in A549 cells;
HAdV-5 preferentially infects goblet cells and is potently neutralised by the
Paneth-cell α-defensin **HD5**, whereas **HAdV-41 is not** [26].

**Dose, incubation, burst, eclipse:** **UNVERIFIED** for species F.

---

### 3.6 Hepatitis A virus — rank 6

**The mechanic.** HAV released from cells is **cloaked in host-derived membrane**
("eHAV"): exosome-like, ESCRT-dependent (VPS4B, ALIX), fully infectious,
chloroform-sensitive, present in the blood of infected humans — and **protected
from antibody-mediated neutralisation** [27]. Anti-capsid antibody still restricts
replication *after* eHAV infection, which is why post-exposure prophylaxis works
[27]. The quasi-envelope is now an established part of the HAV life cycle [28].
Hepatitis E does the same thing with ORF3 [72] — which is why HEV is cut as
redundant.

**Innate evasion, with a caveat.** HAV 3ABC protease cleaves human MAVS, and this
is a conserved hepatovirus attribute under diversifying selection [73][74]; in
knock-in mice whose MAVS is made cleavable, cleavage boosts replication and
*reduces* liver inflammation but is not by itself enough to cross the species
barrier [73]. However, a second group found TRIF and MAVS are only **partially**
cleaved by HAV 3ABC/3CD, not enough to abrogate signalling, and that HAV does
induce ISGs via MDA5/LGP2 [75]. Call the MAVS story **contested in degree**, not
in existence.

**Numbers.** Mean incubation **28 days** (range 15–50); infectious from ~2 weeks
before symptom onset, with maximal stool virus concentration pre-jaundice
[29][30]. Shed in high numbers by symptomatic and asymptomatic people alike [76].
ID50, burst size and eclipse: **UNVERIFIED**.

---

## 4. What this means for the game

- **Build CMV as the stealth virus. Nothing else qualifies.** It is the only
  candidate where MHC-I loss is driven by **four independently sufficient
  mechanisms** [5][6][7], which is exactly the biological justification §4b/§5
  wanted for a cliff rather than a stealth slider. The literature even states the
  design for you: class-I-negative infected cells evade CTLs "but [are] left
  susceptible to NK cells" [5]. Everything else on this list has only *partial*
  MHC-I suppression (rotavirus via blocked STAT1 translocation and β2m
  degradation [47][48]; murine norovirus via NS3 [63]) — and in all those cases
  CD8 T cells still clearly contribute to clearance in vivo [49][50][51][66].
  That is the strongest possible evidence for the Director's existing instinct
  that **partial MHC-I reduction should do nothing mechanically**: in reality, it
  doesn't.
- **Tell the truth about CMV's second layer, but don't build it yet.** CMV also
  strips the NKG2D ligands NK cells would use (UL16, UL142, US18/US20,
  miR-UL112-1 [53][54][55]) after its own IE genes raise them. A clean v1 ships
  the MHC-I cliff with NK as the answer; a later tier can introduce
  "it's learning to hide from your NK cells too" as a genuine, sourced escalation
  rather than a difficulty knob.
- **Ship enteric adenovirus F41 as the mirror boss.** Species F lacks E3-19K
  [23][24] so it is fully CTL-visible, but sequesters MICB intracellularly [25]
  so it is NK/NKG2D-blind. One pair of bosses, two opposite lessons, both sourced.
  It also walks past the Paneth-cell α-defensin HD5 that stops Ad5 [26].
- **Rotavirus is the virus that makes the villus geometry mean something.** Infect
  the tip, never the crypt; the crypt visibly hyper-proliferates and repaves from
  below [1]. That is a self-healing board the player races against, and it is real.
- **Astrovirus is the cheapest big idea here.** A pathogen that raises the §1b
  plaque charge while producing *no* stress signal, *no* DAMP and *no* infected
  cell — because the capsid alone does the damage, with UV-killed virus working
  just as well [20][21]. It defeats the entire detection stack by not being
  detectable, which is a fair and legible kind of hard.
- **Norovirus should be the knowledge-decay enemy, not a damage enemy.** GII.4's
  periodic re-antigenisation is §5's "discrete step-change discount" happening in
  the real world [11][12][13], and its asymptomatic high-titre shedding [16][17]
  plus environmental toughness [18][19] make it the natural source of never-ending
  lumen pressure in §1b.
- **Do not give human norovirus tuft-cell tropism.** The tuft-cell result is
  *murine* norovirus via the CD300lf receptor [56][57]. Human norovirus replicates
  in **enterocytes** (bile-dependent, HBGA-gated) [58][59] and in **enteroendocrine
  cells** in vivo [60]. If you want tuft cells on the board, note that *rotavirus*
  also infects them [1] — and the murine tuft-cell "immune privilege" result [57]
  is a better fit as a future *terrain* mechanic (a cell type CTLs can't work in)
  than as a norovirus property.
- **Use measured burst-size shape, not invented per-species means.** Per-cell burst
  is simply not published for most of these. Use the poliovirus one-step anchor
  (eclipse 3–4 h, 700–1400 particles, release starting after ~10% of progeny is
  made, eclipse *lengthened* by hostile pH) [40], and make burst **heavily skewed**
  — measured single-cell data spans 10¹–10⁴ with ~10% of cells making ~half of all
  progeny [41], formally negative-binomial [42]. A fixed burst number would be the
  least realistic choice available.
- **Boring/unreadable on a ~250-cell board — flagging three:**
  1. **Hepatitis A.** Mean 28-day incubation [29][30] and the real target organ is
     the liver, which isn't on the board. Its quasi-envelope (antibody-proof egress
     [27][28]) is a superb counter to the sIgA and plasma-cell-antibody towers the
     last two sprints shipped — so **keep the mechanic, abstract the timeline
     hard**, or it is a pathogen that does nothing for the entire campaign.
  2. **CMV.** A 96-hour lytic cycle [8] is, honestly, one infected cell sitting
     still for most of a round. That is *fine for a boss* — "slow, unkillable by
     your best tower, already behind the wall" reads as menace — but it must never
     be a wave enemy, and the 250-cell board can only carry a handful of foci
     before the slowness reads as a bug rather than a threat.
  3. **Rotavirus's real dose.** ID50 ≈ 1 infectious unit [32][33] with no
     dose-dependence detectable over four logs in volunteers [43]. Modelling that
     faithfully means every single particle that touches the wall infects, which
     deletes the §1b proximity-gated adhesion roll entirely. Keep rotavirus's
     *tropism* literal and its *dose* abstracted.
- **One cut worth revisiting later: coxsackievirus B.** Two real mechanics the
  shortlist doesn't cover — (a) **non-lytic egress**, with infectious virus leaving
  in autophagosome/mitophagosome-derived extracellular vesicles rather than by
  rupturing the cell [77][78][79], i.e. spread that produces no debris for the §5a
  DC shuttle to learn from; and (b) **picornaviral 3A blocking ER→Golgi traffic**,
  which cuts secretion of IFN-β, IL-6 and IL-8 *and* delivery of newly made MHC-I
  to the surface [80][81] — the only candidate that would silence §7/§9's
  recruitment cytokine at source. Notably this 3A activity is **not** general:
  it is effective for poliovirus and the closely related coxsackievirus B3, but
  **not** for HAV, EV-71, rhinovirus 14, FMDV or Theiler's virus [80]. If a
  seventh virus is ever wanted, this is the one.

---

## 5. References

PubMed-sourced entries (per the PubMed tool's attribution requirement, these are
cited with DOI links; *according to PubMed*):

- [1] Bomidi C. et al. (2021) *Single-cell sequencing of rotavirus-infected intestinal epithelium reveals cell-type specific epithelial repair and tuft cell infection.* PNAS. [Consensus record](https://consensus.app/papers/details/ff947a8afb775187ba41002c1536aeb6/)
- [2] Saxena K. et al. (2015) *Human Intestinal Enteroids: a New Model To Study Human Rotavirus Infection, Host Restriction, and Pathophysiology.* J Virol. [Consensus record](https://consensus.app/papers/details/e95cbe1f99a654b29d7e6cd6bb5c0a0d/)
- [3] Amimo J. et al. (2021) *Rotavirus Interactions With Host Intestinal Epithelial Cells.* Front Immunol. [Consensus record](https://consensus.app/papers/details/07c0e848089f50c3a63db51f06d4f8f3/)
- [4] Pawłuszkiewicz K. et al. (2025) *Rotavirus Infections: Pathophysiology, Symptoms, and Vaccination.* Pathogens. [Consensus record](https://consensus.app/papers/details/6910e14c7b5556d48c89baab6fc4866a/)
- [5] Park B. et al. (2002) *The MHC Class I Homolog of Human Cytomegalovirus Is Resistant to Down-Regulation Mediated by US2, US3, US6, and US11.* J Immunol. [Consensus record](https://consensus.app/papers/details/7d6cb32a485e5ee69d25cd4ad3faf331/)
- [6] Noriega V.M. et al. (2012) *Human cytomegalovirus US3 modulates destruction of MHC class I molecules.* Mol Immunol. [Consensus record](https://consensus.app/papers/details/e24d5753316350ed95ab880212705605/)
- [7] Hsu J.-L. et al. (2015) *Plasma Membrane Profiling Defines an Expanded Class of Cell Surface Proteins Selectively Targeted for Degradation by HCMV US2 in Cooperation with UL141.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/2b680ab3e9ab5dedac5069d54727680c/)
- [8] Monti C.E. et al. (2022) *Mathematical Modeling of Protracted HCMV Replication…* FASEB J. [Consensus record](https://consensus.app/papers/details/3aa185c5816950b892de98d2a70ab35d/)
- [9] Sun L. et al. (2022) *Cytomegalovirus cell tropism and clinicopathological characteristics in gastrointestinal tract of patients with HIV/AIDS.* Diagn Pathol. [Consensus record](https://consensus.app/papers/details/3e49339a50345b459be8c6d945ec4056/)
- [10] Roberts W.H. et al. (1989) *Cytomegalovirus infection of gastrointestinal endothelium…* Arch Pathol Lab Med. [Consensus record](https://consensus.app/papers/details/dd61d65e05795217bfd2f6587514e78b/)
- [11] Lindesmith L. et al. (2010) *Norovirus GII.4 Strain Antigenic Variation.* J Virol. [Consensus record](https://consensus.app/papers/details/41624c5219985993b1771d1036aa6fd7/)
- [12] Debbink K. et al. (2013) *Emergence of new pandemic GII.4 Sydney norovirus strain correlates with escape from herd immunity.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/fe39279777a55415b8eebac92b9fea3c/)
- [13] Kendra J. et al. (2021) *Antigenic cartography reveals complexities of genetic determinants… among pandemic GII.4 noroviruses.* PNAS. [Consensus record](https://consensus.app/papers/details/07443e8e4732589e8d43d0c9a8581416/)
- [14] Atmar R.L. et al., as summarised in Kirby A. et al. (2014) — HID50 1320–2800 gEq.
- [15] Kirby A. et al. (2014) *Two human challenge studies confirm high infectivity of Norwalk virus.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/a28067cc47dd52318c66eb43b0f04264/)
- [16] Teunis P. et al. (2014) *Shedding of norovirus in symptomatic and asymptomatic infections.* Epidemiol Infect. [Consensus record](https://consensus.app/papers/details/0b292020569857b18051b33169f85644/)
- [17] Wu Q.-S. et al. (2019) *Norovirus shedding among symptomatic and asymptomatic employees in outbreak settings in Shanghai.* BMC Infect Dis. [Consensus record](https://consensus.app/papers/details/2dffdcbfd15458718a2779256e65edec/)
- [18] Cook N. et al. (2016) *Persistence and Elimination of Human Norovirus in Food and on Food Contact Surfaces: A Critical Review.* J Food Prot. [Consensus record](https://consensus.app/papers/details/247c8910106c51888313a72579222f8d/)
- [19] Knight A. et al. (2016) *A systematic review of human norovirus survival…* Int J Food Microbiol. [Consensus record](https://consensus.app/papers/details/93d24c55a7675d25b63fcaa681c3ad3f/)
- [20] Moser L. et al. (2007) *Astrovirus Increases Epithelial Barrier Permeability Independently of Viral Replication.* J Virol. [Consensus record](https://consensus.app/papers/details/4aa5cf9af5145893b458f26b1b8c46f9/)
- [21] Meliopoulos V.A. et al. (2016) *Oral Administration of Astrovirus Capsid Protein Is Sufficient To Induce Acute Diarrhea In Vivo.* mBio. [Consensus record](https://consensus.app/papers/details/f528328603ed56dfb58a6658f45388e5/)
- [22] Cortez V. et al. (2017) *Astrovirus Biology and Pathogenesis.* Annu Rev Virol. [Consensus record](https://consensus.app/papers/details/e2dbe3624a4e5928a41b447b74ea2245/)
- [23] Blair G. et al. (1998) *Human Adenoviruses: Evading Detection by Cytotoxic T Lymphocytes.* Semin Virol. [Consensus record](https://consensus.app/papers/details/02a3892acbe15983894eed8327abf0cc/)
- [24] Bouvier M. et al. (2018) *Subversion of antigen presentation by Adenoviruses.* J Immunol. [Consensus record](https://consensus.app/papers/details/b29c1ccb18e95fb3af75c1240a8bb38b/)
- [25] Oliveira E.R.A. et al. (2021) *Intracellular Sequestration of the NKG2D Ligand MIC B by Species F Adenovirus.* Viruses. [Consensus record](https://consensus.app/papers/details/88364e4a206c5406b30c846cb7601174/)
- [26] Holly M.K. & Smith J.G. (2018) *Adenovirus Infection of Human Enteroids Reveals Interferon Sensitivity and Preferential Infection of Goblet Cells.* J Virol. [DOI](https://doi.org/10.1128/JVI.00250-18)
- [27] Feng Z. et al. (2013) *A pathogenic picornavirus acquires an envelope by hijacking cellular membranes.* Nature. [Consensus record](https://consensus.app/papers/details/a37d40b820e85148ac60a6644d98bef8/)
- [28] Zhao Y. et al. (2023) *The role of quasi-enveloped hepatitis A virus in hepatitis A virus infection.* Future Virol. [Consensus record](https://consensus.app/papers/details/4dd30438f38d5d89843869a5caabec10/)
- [29] Barreiro P. et al. (2018) *Hepatitis A Outbreaks in European Homosexual Men.* AIDS Rev. [Consensus record](https://consensus.app/papers/details/61c504e29779571b97592f1445bc3b4c/)
- [30] Ntouva A. et al. (2019) *Hepatitis A in primary care…* Br J Gen Pract. [Consensus record](https://consensus.app/papers/details/dbc9594835c95ad697ca49728750aaaf/)
- [31] Abdelqader R. et al. (2024) *Global epidemiology, genotype distribution and coinfection rate of Human Aichi virus: A systematic review.* J Infect Chemother. [DOI](https://doi.org/10.1016/j.jiac.2024.09.012)
- [32] Ward R.L. et al. (1986) *Human rotavirus studies in volunteers: determination of infectious dose and serological response to infection.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/a074c23ff02953e78f625560b1301572/)
- [33] Graham D. et al. (1987) *Minimal infective dose of rotavirus.* Arch Virol. [Consensus record](https://consensus.app/papers/details/92ae9658bdb45869bddd8254805603fe/)
- [34] Sadeq T. et al. (2021) *Effects of Rota Vaccine on Severity and Duration of Rotavirus Infection…* (incubation ~2 days). [Consensus record](https://consensus.app/papers/details/f4c2f50ad67d50269fe970dd6f9b6ab3/)
- [35] González-Parra G.C. et al. (2018) *Quantifying rotavirus kinetics in the REH tumor cell line using in vitro data.* Virus Res. [Consensus record](https://consensus.app/papers/details/53e7af11729654109e2b5b7367b3ce25/)
- [36] McCrae M. et al. (1981) *Molecular biology of rotaviruses. I. Characterization of basic growth parameters…* J Virol. [Consensus record](https://consensus.app/papers/details/f22143e4a0085454bfe54d10d7b9682a/)
- [37] Rouphael N. et al. (2022) *Dose-Response of a Norovirus GII.2 Controlled Human Challenge Model Inoculum.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/0cd82429feb35f9a994c38e0a030dc97/)
- [38] Ramesh A.K. et al. (2020) *Evaluation of the 50% Infectious Dose of Human Norovirus Cin-2 in Gnotobiotic Pigs.* Viruses. [Consensus record](https://consensus.app/papers/details/d1767a6eefc553e9ab83743d0530824f/)
- [39] Omoto S. et al. *Transcription of True Late (γ2) Cytomegalovirus Genes Requires UL92…* [Consensus record](https://consensus.app/papers/details/ac016b3635ad51c38a0f4c3d5394efa3/)
- [40] Furness G. (1961) *The effect of environment on the replication of poliovirus in monkey kidney cells.* J Gen Microbiol. [Consensus record](https://consensus.app/papers/details/a8bcc10b25935bee812edfda4ee850cd/)
- [41] Zath G.K. et al. (2024) *Influenza A viral burst size from thousands of infected single cells using droplet quantitative PCR.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/054cedbcac465a1b9f9f94c1f3e622a3/)
- [42] Williams B. et al. (2024) *The reproduction number and its probability distribution for stochastic viral dynamics.* J R Soc Interface. [Consensus record](https://consensus.app/papers/details/3711d6a5b8a453cea12579d6c74bb21e/)
- [43] Ward R. et al. (1989) *Effects of antibody to rotavirus on protection of adults challenged with a human rotavirus.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/87c19cc054ea5a0e8aa8094510c4b08b/)
- [44] Barro M. et al. (2007) *Rotavirus NSP1 Inhibits Expression of Type I Interferon by Antagonizing IRF3, IRF5, and IRF7.* J Virol. [Consensus record](https://consensus.app/papers/details/9da00a1806ce5da080a44ba39b8b3e77/)
- [45] Arnold M. et al. (2010) *Diversity of Interferon Antagonist Activities Mediated by NSP1 Proteins of Different Rotavirus Strains.* J Virol. [Consensus record](https://consensus.app/papers/details/91a0841d5a8b5899b31cd6a4f14bb206/)
- [46] Zeng Q. et al. (2026) *A conserved mechanism for stabilization of viral innate immune antagonists via interaction with Elongin BC.* PNAS. [Consensus record](https://consensus.app/papers/details/d90f95bd45bf585fa0aa4140b04273f8/)
- [47] Holloway G. et al. (2018) *MHC class I expression in intestinal cells is reduced by rotavirus infection and increased in bystander cells lacking rotavirus antigen.* Sci Rep. [Consensus record](https://consensus.app/papers/details/f250c1635cba5217a21f6f0758f2e4a5/)
- [48] Jing Z. et al. (2024) *Rotavirus infection inhibits SLA-I expression on the cell surface by degrading β2M via ERAD-proteasome pathway.* Vet Microbiol. [Consensus record](https://consensus.app/papers/details/8042035ef3a359ffa8ed20b4fd509495/)
- [49] Franco M.A. et al. (1995) *Role of B cells and cytotoxic T lymphocytes in clearance of and immunity to rotavirus infection in mice.* J Virol. [Consensus record](https://consensus.app/papers/details/52994436454e5246a6fd5037bccd3b4c/)
- [50] McNeal M. et al. (1995) *Effector functions of antibody and CD8+ cells in resolution of rotavirus infection…* Virology. [Consensus record](https://consensus.app/papers/details/a17de4055a6455f084b4b4fac5cdaefa/)
- [51] Dharakul T. et al. (1990) *Recovery from chronic rotavirus infection in SCID mice: virus clearance mediated by adoptive transfer of immune CD8+ T lymphocytes.* J Virol. [Consensus record](https://consensus.app/papers/details/3d4114d129e45ec6a77d6ff7ac638812/)
- [52] Siciliano R. et al. (2014) *Cytomegalovirus colitis in immunocompetent critically ill patients.* Int J Infect Dis. [Consensus record](https://consensus.app/papers/details/afdb00564cce551788fdf039a727914f/)
- [53] Fielding C. et al. (2014) *Two Novel Human Cytomegalovirus NK Cell Evasion Functions Target MICA for Lysosomal Degradation.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/dd84c09319f9567ea0ad5a51de8a32e9/)
- [54] Chalupny N. et al. (2006) *Down-regulation of the NKG2D ligand MICA by the human cytomegalovirus glycoprotein UL142.* BBRC. [Consensus record](https://consensus.app/papers/details/9e9b8f20ef415369b5d500da8ef9e20c/)
- [55] Stern-Ginossar N. et al. (2007) *Host Immune System Gene Targeting by a Viral miRNA* (hcmv-miR-UL112 → MICB). Science. [Consensus record](https://consensus.app/papers/details/baa496907b815c20811a12679a53a541/)
- [56] Wilen C.B. et al. (2018) *Tropism for tuft cells determines immune promotion of norovirus pathogenesis.* Science. [DOI](https://doi.org/10.1126/science.aar3799)
- [57] Strine M.S. et al. (2024) *Intestinal tuft cell immune privilege enables norovirus persistence.* Sci Immunol. [Consensus record](https://consensus.app/papers/details/a397724a31bf562585ad7a3d858ff90b/)
- [58] Ettayebi K. et al. (2016) *Replication of human noroviruses in stem cell-derived human enteroids.* Science. [DOI](https://doi.org/10.1126/science.aaf5211)
- [59] Murakami K. et al. (2020) *Bile acids and ceramide overcome the entry restriction for GII.3 human norovirus replication in human intestinal enteroids.* PNAS. [DOI](https://doi.org/10.1073/pnas.1910138117)
- [60] Green K. et al. (2020) *Human norovirus targets enteroendocrine epithelial cells in the small intestine.* Nat Commun. [Consensus record](https://consensus.app/papers/details/bfde5d44eb1359078ce476a906fdc2c7/)
- [61] Roux E. et al. (2024) *Transcriptional profiling of zebrafish intestines identifies macrophages as host cells for human norovirus infection.* Gut Microbes. [Consensus record](https://consensus.app/papers/details/2647d47fcd8b53bfa1479f3bef1ac7e8/)
- [62] Gonzalez-Hernandez M.B. et al. (2014) *Efficient norovirus and reovirus replication in the mouse intestine requires microfold (M) cells.* J Virol. [DOI](https://doi.org/10.1128/JVI.00204-14)
- [63] Fritzlar S. et al. (2018) *Mouse Norovirus Infection Reduces the Surface Expression of MHC Class I Proteins and Inhibits CD8+ T Cell Recognition and Activation.* J Virol. [Consensus record](https://consensus.app/papers/details/3439a983f6885ba0ba25cccda7275f8f/)
- [64] Enosi Tuipulotu D. et al. (2017) *RNA Sequencing of Murine Norovirus-Infected Cells…* Front Immunol. [Consensus record](https://consensus.app/papers/details/3b85004069c05efb97198f3bc7878ae4/)
- [65] Zhu S. et al. (2016) *Norovirus Antagonism of B cell Antigen Presentation Results in Impaired Control of Acute Infection.* Mucosal Immunol. [Consensus record](https://consensus.app/papers/details/1340f664ea9558499eefded99a8c7863/)
- [66] Durkee-Shock J. et al. (2024) *Reconstitution of norovirus-specific T cell responses following HSCT in patients with inborn errors of immunity and chronic norovirus infection.* J Infect Dis. [Consensus record](https://consensus.app/papers/details/7d5a2bfde4165ec3a08286484d70fbf9)
- [67] Pattekar A. et al. (2021) *Norovirus-Specific CD8+ T Cell Responses in Human Blood and Tissues.* CMGH. [Consensus record](https://consensus.app/papers/details/9f26490cb90251818c44f0baf8b8beed/)
- [68] Smielewska A. et al. (2025) *Chronic persistent Norovirus in the immune-compromised host.* Curr Opin Infect Dis. [Consensus record](https://consensus.app/papers/details/1a8c292363c751e38f565cb25f35e69d/)
- [69] Hermiston T.W. et al. (1993) *Deletion mutation analysis of the adenovirus type 2 E3-gp19K protein…* J Virol. [Consensus record](https://consensus.app/papers/details/0150f082a4bb5d1eb7888970bd707a03/)
- [70] Fu J. et al. (2010) *Determinants of the ER lumenal domain of the Adenovirus serotype 2 E3-19K protein…* Mol Immunol. [Consensus record](https://consensus.app/papers/details/193bbaab21dc55b49ac957aa1a36fbef/)
- [71] Götting J. et al. (2022) *Molecular phylogeny of human adenovirus type 41 lineages.* Virus Evol. [Consensus record](https://consensus.app/papers/details/bb75cd8caa095b1db65e3721ed42ce6c/)
- [72] Maurer L. et al. (2022) *Induction of Hepatitis E Virus Anti-ORF3 Antibodies…* Viruses. [Consensus record](https://consensus.app/papers/details/a81ef6f838e15ef691b0edcfdc6f0bfb/)
- [73] Sun L. et al. (2022) *Viral protease cleavage of MAVS in genetically modified mice with hepatitis A virus infection.* J Hepatol. [Consensus record](https://consensus.app/papers/details/710db0ae397957aba6e548f6a4217975/)
- [74] Feng H. et al. (2019) *Hepatovirus 3ABC proteases and evolution of MAVS.* J Hepatol. [Consensus record](https://consensus.app/papers/details/ef4583951b6c567c83081b3585ba25fd/)
- [75] Colasanti O. et al. (2022) *Comparison of HAV and HCV infections in vivo and in vitro reveals distinct patterns of innate immune evasion and activation.* [Consensus record](https://consensus.app/papers/details/67449df64f3b5e2a9e9767e2b9227ca7/)
- [76] Sánchez G. (2013) *Summary and Future Directions* (HAV shedding and 28-day incubation). [Consensus record](https://consensus.app/papers/details/90d16aaa57325c38a354e03858de224d/)
- [77] Sin J. et al. (2017) *Coxsackievirus B Escapes the Infected Cell in Ejected Mitophagosomes.* J Virol. [Consensus record](https://consensus.app/papers/details/2119935bbd3a56d7985b54e7efe653d7/)
- [78] Sawaged S. et al. (2022) *TBK1 and GABARAP family members suppress Coxsackievirus B infection…* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/6215534ac8eb5ceaaebb37fa00ecb902/)
- [79] Jorfi S. et al. (2023) *A Coxsackievirus B1-mediated nonlytic Extracellular Vesicle-to-cell mechanism of virus transmission…* J Gen Virol. [Consensus record](https://consensus.app/papers/details/7e69e5d2f1bb5aa0bebe63a5b94b3355/)
- [80] Choe S.S. et al. (2005) *Inhibition of cellular protein secretion by picornaviral 3A proteins.* Virology. [Consensus record](https://consensus.app/papers/details/88252e2fe03a5a399feedff5c21be991/)
- [81] Deitz S. et al. (2000) *MHC I-dependent antigen presentation is inhibited by poliovirus protein 3A.* PNAS. [Consensus record](https://consensus.app/papers/details/98dd461feba159b6b26d2dfad15f1e3d/)

Also consulted: Ingle H. et al. (2023) *IFN-λ derived from nonsusceptible enterocytes acts on tuft cells to limit persistent norovirus*, Sci Adv — [DOI](https://doi.org/10.1126/sciadv.adi2562); Kennedy E.A. et al. (2023) *Age-associated features of norovirus infection analysed in mice*, Nat Microbiol — [DOI](https://doi.org/10.1038/s41564-023-01383-1); Nolan L.S. & Baldridge M.T. (2022) *Advances in understanding interferon-mediated immune responses to enteric viruses in intestinal organoids*, Front Immunol — [DOI](https://doi.org/10.3389/fimmu.2022.943334); Freeman M.C. et al. (2021) *Respiratory and intestinal epithelial cells exhibit differential susceptibility and innate immune responses to contemporary EV-D68 isolates*, eLife — [DOI](https://doi.org/10.7554/eLife.66687).

Sources were retrieved from PubMed and from Consensus.

> Create or connect a free Consensus account to return more than 3 results per search in Claude Code.: https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code
