# Body shop ("body upgrades") — mechanism audit and re-spec

Research pass, 2026-10-25. Companion to `docs/PATHOGEN_ROSTER.md` and
`docs/IGA_REFERENCE.md`: this is the evidence the shop design should be
argued from, not the design itself. `GAME_DESIGN.md` wins on any conflict.

**Attribution: substantially based on articles retrieved from PubMed**, with
DOI links inline, plus Consensus for discovery. Nothing in the repo was
edited. Claims I could not pin to a specific paper are marked
**UNVERIFIED**.

The wall model this is written against (§1b): adhesion → milling + crowding
→ plaque charge → rupture → tissue advance, with two flush-not-kill
defences out front (goblet sloughing, secretory IgA) and a debris → DC →
knowledge loop behind.

---

## 1. All eight current items — verdict table

| Item | Verdict | What it should do | Key citation |
|---|---|---|---|
| **Cytokine sensing** (live) | **re-spec** | Stop being one gain scalar. Give the game **two chemoattractant classes** and let the upgrade sharpen the *hierarchy*: end-target signals (from the organism / from C5a) must dominate intermediary ones (host cytokines on the way in). Neutrophils prefer end-target at **1/1000th** the intermediary concentration; blocking the p38 arm inverts the hierarchy and drags cells *out* of an infected site **2–5× harder** toward intermediary signals. | Heit et al. 2002, *J Cell Biol* 159:91–102 — [DOI](https://doi.org/10.1083/jcb.200202114) |
| **Mucus turnover** (live) | **keep, add the cost** | §6b's "turnover, not thickness" is correct. But secreted antimicrobial activity **lives in the mucus layer** (44–49% of extractable activity in mucus, 38–56% in crypts, only **2–19% in luminal content**), so fast shedding also discards the peptide reservoir. That is a free, cited drawback for an item that currently has none. Also flag: the sentinel goblet cell was described in the **colon**, not the small intestine. | Meyer-Hoffert et al. 2008, *Gut* 57:764–71 — [DOI](https://doi.org/10.1136/gut.2007.141481); Birchenough et al. 2016, *Science* 352:1535–42 — [DOI](https://doi.org/10.1126/science.aaf7419) |
| **Crypt** (live) | **keep** | Correct as built. One clarification that creates a second item: the crypt supplies **proliferation**. Sheet closure over a wound is a *separate and faster* process (restitution) that needs **no proliferation at all**. Crypts and restitution are genuinely different mechanics; the game should have both. | Dignass et al. 1994, *J Clin Invest* 94:376–83 — [DOI](https://doi.org/10.1172/JCI117332) |
| **M-cell sampling** (live) | **keep, make it species-selective** | GP2 on M cells is a **transcytotic receptor for FimH⁺ (type-1-piliated) bacteria** — *Salmonella* Typhimurium and *E. coli* among them. Loss of either FimH or GP2 abolishes transcytosis and attenuates the antigen-specific response. So sampling should work on *some* species and not others, and the same door is a real **entry** route. This is the single best upgrade to current behaviour. | Hase et al. 2009, *Nature* 462:226–30 — [DOI](https://doi.org/10.1038/nature08529) |
| **Host dsRNA sensor** (dead) | **re-spec — §2.1** | The sensor is right; "immunogenic apoptosis" is half-right. Split into a **clean** route (OAS1/RNase L → caspase-3) and a **messy** route (ZBP1/Z-RNA → RIPK3/MLKL necroptosis). And in the gut the cell doesn't leave a corpse — it is **extruded into the lumen**. | Boehmer et al. 2021, *Sci Immunol* 6:eabe2550 — [DOI](https://doi.org/10.1126/sciimmunol.abe2550); Zhang et al. 2020, *Cell* 180:1115–29 — [DOI](https://doi.org/10.1016/j.cell.2020.02.050); Sellin et al. 2014, *Cell Host Microbe* 16:237–48 — [DOI](https://doi.org/10.1016/j.chom.2014.07.001) |
| **Harden vs viral entry** (dead) | **split — §2.2** | Defensible but must not be one global knob. IFITM3 is real but **small and virus-specific** (human OR ≈1.7–2.7 for severe influenza) and *backfires* on at least one coronavirus. The big numbers are at the **receptor glycan**: GII.4 norovirus infected **155/155** secretors and zero non-secretors. | Everitt et al. 2012, *Nature* 484:519–23 — [DOI](https://doi.org/10.1038/nature10921); Currier et al. 2015, *Clin Infect Dis* 60:1631–8 — [DOI](https://doi.org/10.1093/cid/civ165) |
| **Bacterial resistance** (dead) | **cut as written, replace — §2.3** | "A tougher enterocyte that takes less damage per graze" has **no support I could find**. The gut's answer is *keep it off the cell* and *kill it in the mucus*. The cell-intrinsic, small-intestine-specific answer is the **membrane mucin MUC17 glycocalyx**. | Layunta et al. 2024, *JCI Insight* 10(3):e181481 — [DOI](https://doi.org/10.1172/jci.insight.181481) |
| **Complement** (dead) | **confirmed, build as specified — §2.4** | Opsonisation + phagocyte recruitment, never lysis — confirmed, and independently corroborated outside the colon paper. Plus one addition nobody in the repo has yet: coated organisms that get *inside* a cell are also **xenophagy targets** via a direct C3–ATG16L1 interaction. | Wu et al. 2024, *Cell* 187:897–913 — [DOI](https://doi.org/10.1016/j.cell.2023.12.036); Sorbara et al. 2018, *Cell Host Microbe* 23:644–52 — [DOI](https://doi.org/10.1016/j.chom.2018.04.008) |

---

## 2. Full specs — the four dead items

### 2.1 Host dsRNA sensor → **"Sense and extrude"**

#### 2.1.1 Which sensor, and what each one actually does

Four systems, and they do **four different jobs**. Treating them as one
"dsRNA sensor" is the current spec's only real error.

| System | Job | Fits "the cell kills itself and signals"? |
|---|---|---|
| **RIG-I / MDA5 → MAVS** | **Detection and the interferon call.** In intestinal epithelium specifically, rotavirus is sensed by **both** RIG-I and MDA5, alone and together; silencing either, or MAVS, cut IFN-β and raised titres; MAVS-null mice made less IFN-β, carried more virus in the epithelium, and **shed more in faeces**. | Partly — it is the *priming* step, not the executioner |
| **OAS1 / RNase L** | **The executioner.** RIG-I ligand binds OAS1, activating RNase L; RNase L-deficient cells are "profoundly impaired" in their ability to undergo apoptosis. Translational arrest + NOXA up → MCL-1 depleted → intrinsic (caspase-3) apoptosis. The authors call it explicitly **a two-step process: RIG-I-dependent priming, RNase L-dependent effector phase.** | **Yes — this is the right molecule for a clean self-destruct** |
| **ZBP1 (Z-RNA)** | **The messy executioner.** Replicating virus makes Z-RNA in the nucleus; ZBP1 → RIPK3 → nuclear MLKL → nuclear envelope disruption, DNA into the cytosol, necroptosis. The dying cell is "a potent activator of neutrophils," and MLKL-deficient mice had less nuclear disruption, fewer recruited neutrophils and **better survival** of lethal influenza. ZBP1 also drives NLRP3, apoptosis, necroptosis *and* pyroptosis in the same infection. | **Yes — and it is the opposite of clean** |
| **PKR** | eIF2α phosphorylation, translational shutdown, apoptosis. **But explicitly not the rotavirus sensor**: rotavirus "did not signal IFN-β through either of the dsRNA sensors TLR3 or dsRNA-activated protein kinase (PKR)," and PKR-null mice were not impaired. | **No — wrong sensor for this roster** |

Citations, in order: Broquet et al. 2011, *J Immunol* 186:1618–26 —
[DOI](https://doi.org/10.4049/jimmunol.1002862); Boehmer et al. 2021,
*Sci Immunol* 6:eabe2550 —
[DOI](https://doi.org/10.1126/sciimmunol.abe2550); Ramnani et al. 2024,
*Viruses* 16:1913 — [DOI](https://doi.org/10.3390/v16121913) (RNase L
cleavage products need DHX15 + RIG-I + MAVS + JNK/p38 for caspase-3
apoptosis, and this is **uncoupled** from the cytokine output — two
separable outputs from one sensor, which is directly useful); Siddiqui et
al. 2015, *Int J Mol Sci* 16:17611–36 —
[DOI](https://doi.org/10.3390/ijms160817611) (RNase L products flip
autophagy → apoptosis by caspase-cleaving Beclin-1); Zhang et al. 2020,
*Cell* 180:1115–29 — [DOI](https://doi.org/10.1016/j.cell.2020.02.050);
Jiao et al. 2020, *Nature* 580:391–5 —
[DOI](https://doi.org/10.1038/s41586-020-2129-8) (ZBP1 sensing of
*endogenous* retroelement dsRNA causes spontaneous colitis in
FADD-deficient intestinal epithelium — i.e. the sensor misfires on the
host's own RNA, which is a drawback with a mechanism);
Kuriakose et al. 2016, *Sci Immunol* 1:aag2045 —
[DOI](https://doi.org/10.1126/sciimmunol.aag2045).

#### 2.1.2 The answer to the Director's question: **necroptosis/pyroptosis, not (only) immunogenic apoptosis — and the distinction is real**

- **Apoptosis is the quiet death.** Efferocytosis of an apoptotic cell is
  "immunologically silent and induces a tissue repair response," and it
  *prevents* the secondary necrosis and inflammation that follow release of
  intracellular contents. Yin & Heit 2021, *Front Immunol* 12:631714 —
  [DOI](https://doi.org/10.3389/fimmu.2021.631714).
- **Necroptosis is the loud death**, and the loudness is the point — though
  the field is honest that the relative potency of DAMPs versus
  conventional cytokines is still not pinned down, and apoptosis is not
  *always* non-inflammatory. Davidovich, Kearney & Martin 2014, *Biol Chem*
  395:1163–71 — [DOI](https://doi.org/10.1515/hsz-2014-0164).
- **"Immunogenic cell death" is a real, named category** with a defined
  DAMP set — surface calreticulin, HSP70/90, released ATP, HMGB1, type I
  IFN, IL-1 family — and it is that set, not the death itself, that
  recruits and licenses antigen presentation. Ahmed & Tait 2020,
  *Mol Oncol* 14:2994–3006 — [DOI](https://doi.org/10.1002/1878-0261.12851).

**So the frame is: the sensor is one, the death is two, and the game should
let the player buy which one.** Clean (RNase L → caspase-3) gives a small
debris pile and a strong, *specific* "eat this" signal. Messy (ZBP1 → MLKL)
gives a full pile, a neutrophil surge, and fibrosis. That maps exactly onto
the axis §4c already uses for the CTL ("what it buys is a clean death").

#### 2.1.3 The mechanic the gut actually uses, and the one the game is missing

A small-intestinal epithelial cell that detects an intracellular invader
does not simply die in place — **it is expelled into the lumen**, and the
neighbours close over the hole.

- *Salmonella* that invades the epithelium replicates there, and that niche
  is "restricted by expulsion of infected enterocytes into the lumen."
  Delete the inflammasome (NAIP1-6, NLRC4, caspase-1/-11) globally **or
  only in the epithelium** and you get **~100-fold higher intraepithelial
  loads** and accelerated lymph-node colonisation. Strikingly, the
  downstream cytokines (IL-1α/β, IL-18) appear **dispensable** — it is the
  *physical removal* that does the work. Sellin et al. 2014, *Cell Host
  Microbe* 16:237–48 —
  [DOI](https://doi.org/10.1016/j.chom.2014.07.001).
- The expulsion is co-ordinated: lytic death plus eicosanoid and IL-18
  release, driven by caspase-1 **and** caspase-8, with "a major actin
  rearrangement in neighboring cells that maintained epithelium integrity."
  Organoids do it with no other cell type present — it is genuinely
  epithelium-intrinsic. Rauch et al. 2017, *Immunity* 46:649–59 —
  [DOI](https://doi.org/10.1016/j.immuni.2017.03.016).
- **Human caveat, important:** in human IEC lines and enteroids the
  NAIP/NLRC4 and NLRP3/ASC routes are **dispensable**; humans use
  **caspase-4** and gasdermin-D pore-forming activity instead. Naseer et
  al. 2022, *Infect Immun* 90:e00663-21 —
  [DOI](https://doi.org/10.1128/iai.00663-21). The mechanic survives; the
  molecule name changes. Call it "extrusion," not "NLRC4."

#### 2.1.4 Game spec

**Name:** *Nucleic-acid sensor* (levels: dsRNA → dsDNA → LPS/flagellin,
each keyed to a class, as §1d already anticipated).

On a host cell flipping `Healthy → Infected`, roll once per level-scaled
interval:

1. **On a hit, the cell is EXTRUDED, not killed in place.**
   - The host-layer slot goes `Infected → Empty` (**not** `Dead`): no
     debris, because the corpse left the board through the lumen.
   - The intracellular resident goes **with it, into §1a's flow band**, at
     the lane above the extrusion point, as a free organism subject to the
     normal adhesion roll. It is not killed. This is correct (*Salmonella*
     is extruded *alive*, which the roster's Correction 1 already records)
     and it is better game: the player gets the cell back fast but the
     organism gets a second chance at the wall.
   - Neighbours "close over": the adjacent host slots take a brief
     cosmetic contraction. Mechanically the gap is closed by the
     **restitution** item (§3.4) if bought, and otherwise by crypt
     regrowth.
2. **The death flavour is a purchasable branch, not a level.**
   | Branch | Debris | DC-attractant | Fibrosis | Side effect |
   |---|---|---|---|---|
   | **Clean** (RNase L) | none on extrusion; a small pile if the cell dies in place | **strong, DC-specific** | none | — |
   | **Messy** (ZBP1) | full pile | strong | **yes** | a local neutrophil surge the player did not buy |
   The clean branch is the one §1d described. The messy branch should be
   *cheaper* and *stronger at killing the resident*, so it is a real
   temptation and not a trap.
3. **A third output, uncoupled.** Ramnani 2024 shows the cytokine and
   cell-death outputs of the same sensor are independent. So sensor level
   should *also* raise local interferon output (feeding §3.1's IFN-λ item)
   even on rolls that do not kill — which makes the item useful before it
   ever fires, and avoids the "I bought it and saw nothing" problem.

**Real numbers.** The honest effect size for *extrusion* is large and well
measured: **~100× intraepithelial load** (Sellin 2014). The honest effect
size for a *dsRNA-triggered* self-destruct is directional only — more virus
in the epithelium and more faecal shedding without MAVS (Broquet 2011) —
with no transferable per-cell probability. **§1d's ~20% self-destruct
chance is a game number, not a measured one: UNVERIFIED.** Keep it, label
it as a design choice, and tune on feel.

**Drawback: yes, strongly cited, two of them.**
- Rauch 2017: "Excessive inflammasome activation in IECs was **sufficient
  to result in diarrhea and pathology**." Over-levelling extrusion should
  cost the player board coverage even with no pathogen present — cells
  leaving faster than crypts replace them.
- Jiao 2020: ZBP1's Zα domains bind **endogenous** dsRNA from
  retroelements, and that constitutive sensing causes colitis in
  FADD-deficient intestinal epithelium. The messy branch should have a
  small per-round misfire chance on *uninfected* cells.
- Zhang 2020: the MLKL death is what brings the neutrophils and what kills
  the mouse. The messy branch should feed **A/E *E. coli*'s
  `LocalInflammation`** directly — the game already has the enemy that
  profits from it, which makes this the most interesting drawback on the
  board.

---

### 2.2 Harden vs viral entry → **split into two items**

#### 2.2.1 IFITM3 — real, and smaller than it looks

**Mechanism.** IFITM3 does not stop the virus reaching the membrane or
even starting to fuse. It lets **hemifusion (lipid mixing) proceed** and
then **blocks formation of the fusion pore**, so viral contents never reach
the cytoplasm; it stabilises the cytoplasmic leaflet of the endosomal
membrane without affecting the lumenal leaflet. The excess cholesterol in
late endosomes of IFITM3-expressing cells is **not** the mechanism. Desai
et al. 2014, *PLoS Pathog* 10:e1004048 —
[DOI](https://doi.org/10.1371/journal.ppat.1004048).

**It is gut-relevant.** IFITM3-overexpressing cells were less susceptible
to rotavirus and IFITM3-knockout cells significantly more so; IFITM3
**delayed rotavirus escape from late endosomes**. First evidence of this,
*in vitro* only. Pang et al. 2022, *Viruses* 14:2407 —
[DOI](https://doi.org/10.3390/v14112407).

**Effect sizes — honest, and modest.**
- *Mouse, large:* `Ifitm3`-null mice get **fulminant viral pneumonia from a
  normally low-pathogenicity influenza virus**, likened to 1918. Everitt et
  al. 2012, *Nature* 484:519–23 —
  [DOI](https://doi.org/10.1038/nature10921).
- *Human, modest:* meta-analysis of 12 studies, 16,263 subjects — rs12252
  C vs T, **OR 1.69 (1.23–2.33)** for severe influenza and 1.46 for mild.
  Prabhu et al. 2018, *Gene* 674:70–9 —
  [DOI](https://doi.org/10.1016/j.gene.2018.06.070). An earlier
  meta-analysis: **OR 2.70 (1.86–3.94)** for severe illness, and **no
  significant association with mild infection** — i.e. it changes
  *severity*, not whether you get infected. Xuan et al. 2015, *Epidemiol
  Infect* 143:2975–84 —
  [DOI](https://doi.org/10.1017/S0950268815000278).
- **I could not find a clean, transferable "IFITM3 reduces entry by N-fold"
  figure** usable as a tuning constant. Overexpression studies report
  virus-by-virus restriction profiles rather than one coefficient:
  **UNVERIFIED**.

**It is virus-specific, not generic.** IFITM1/2/3 restrict influenza A,
Marburg, Ebola and SARS-CoV entry with **distinct patterns** — filoviruses
are *less* restricted by IFITM3 and *more* by IFITM1; mouse Ifitm5/6 do
nothing to influenza but block filoviruses efficiently. Restriction sits at
a late endocytic step, and IFITMs "modulate cellular tropism independently
of viral receptor expression." Huang et al. 2011, *PLoS Pathog* 7:e1001258
— [DOI](https://doi.org/10.1371/journal.ppat.1001258). **So the upgrade
must be per-species, with a published-looking per-species multiplier, not a
global `VirusEntryChancePerTick` scalar.**

**Costs, cited — two, and both are good game.**
1. **It taxes the host's own membrane fusion.** IFITM1/2/3 block
   syncytin-1- and syncytin-2-mediated trophoblast fusion; knocking IFITMs
   down *increased* spontaneous fusion. The authors frame it as "a critical
   balance between these antifusogenic effects and the beneficial antiviral
   effects." Zani et al. 2019, *J Biol Chem* 294:19844–51 —
   [DOI](https://doi.org/10.1074/jbc.AC119.010611). Game translation: any
   host process the game models as membrane fusion gets slower.
2. **It inverts on at least one virus.** HCoV-OC43 **uses** IFN-induced
   IFITM2/IFITM3 as an *entry factor*; all three interferon types
   efficiently **promote** that infection. Zhao et al. 2014, *PNAS*
   111:6756–61 — [DOI](https://doi.org/10.1073/pnas.1320856111). Give one
   roster virus this trait and the item stops being a pure buff.

#### 2.2.2 The much bigger lever: receptor glycans (secretor status)

This is where the large, clean, human numbers are.

- **GII.4 norovirus infected secretors and nothing else.** In US paediatric
  surveillance, norovirus cases were **2.8× more likely to be secretors**
  than controls (p<0.001, adjusted), and **all 155 GII.4 cases and all 21
  GII.4 asymptomatic infections were secretors** — a complete, not partial,
  block. ~70–80% of people carry a functional FUT2. Currier et al. 2015,
  *Clin Infect Dis* 60:1631–8 —
  [DOI](https://doi.org/10.1093/cid/civ165).
- **Rotavirus too, nearly as strongly.** In Brazilian children with acute
  diarrhoea, **97% of rotavirus-infected and 98.5% of
  norovirus-infected children were secretors**. Tonini et al. 2020,
  *Viruses* 12:1084 — [DOI](https://doi.org/10.3390/v12101084).

**This is a binary, per-species, receptor-level block — an order of
magnitude more powerful than IFITM3 — and it is exactly the shape of a
"harden the wall against a virus" upgrade.** It also composes with the
game's existing knowledge system: you have to *know* the species to mask
the right glycan.

**Costs, cited, and serious.**
- Non-secretors have **increased Crohn's disease susceptibility**, and the
  FUT2⁻/⁻ mucosal ecosystem is reprogrammed: enriched carbohydrate/lipid
  and glycan metabolism, depleted amino-acid biosynthesis, novel
  metabolite shifts, and metaproteomics showing **sub-clinical
  inflammation** — replicated in FUT2⁻/⁻ mice. Tong et al. 2014, *ISME J*
  8:2193–206 — [DOI](https://doi.org/10.1038/ismej.2014.64).
- The same non-secretor genotype **raises type 1 diabetes risk**: OR 1.29
  (1.20–1.37) across 8,344 cases / 10,008 controls, combined
  p = 4.3 × 10⁻¹⁸. The authors' own framing is "the intriguing
  relationship between host resistance to infections and susceptibility to
  autoimmune disease." Smyth et al. 2011, *Diabetes* 60:3081–4 —
  [DOI](https://doi.org/10.2337/db11-0638).

#### 2.2.3 Game spec

**Cut** `HostReducedViralEntry` as a single global knob. Replace with:

**(a) *Endosomal brake* (IFITM).** Cheap, repeatable, small. Lowers the
per-tick entry chance by a **per-species** multiplier read off a table, not
a flat amount. One roster virus gets `IfitmInverted = true` and *gains*
entry chance per level. Secondary cost: any modelled host fusion event
(nothing today; the hook exists for later) slows.

**(b) *Mask the receptor* (secretor status / glycan).** Expensive, **not
repeatable per species — bought once per characterised species**, gated
behind a knowledge rung. Effect: that species' adhesion-and-entry roll goes
to **near zero**, which is honest and dramatic. Cost: a standing penalty to
the commensal-health stat (§5), which lowers the complement ceiling
(§2.4) and REG3γ reach (§2.3), plus a small permanent background
"sub-clinical inflammation" that feeds A/E *E. coli*.

That gives the player a genuine choice between a cheap broad shrug and an
expensive species-specific wall with a systemic price — which is a better
shop row than one slider.

**A note the current spec gets right and should keep:** §1b's "nothing
there to infect" phrasing. IFITM and glycan masking both act at a *living*
cell, so neither helps over dead wall — and over dead wall the virus had
nothing anyway. The asymmetry survives untouched.

---

### 2.3 Bacterial resistance → **cut as written; replace with "Glycocalyx and the peptide layer"**

#### 2.3.1 The honest finding first

**I found no evidence for the intended mechanism.** "An upgraded enterocyte
takes less damage per step from a grazing bacterium" has no literature
behind it that I could locate. The small intestine's answer to a
wall-grazing bacterium is not a tougher cell; it is **three spatial
mechanisms that keep the bacterium off the cell, and one that kills it in
the mucus**. The nearest thing to genuine cell-intrinsic damage tolerance
is plasma-membrane repair — cells shed pore-forming toxin in
microvesicles via annexins and ESCRT rather than lysing, so "immediate cell
lysis … appears to be an unlikely event *in vivo*" (Wolfmeier et al. 2016,
*BBA* 1860:2498–509 — [DOI](https://doi.org/10.1016/j.bbagen.2016.07.022)).
That is real, but it is a *toxin* mechanism, not a grazing mechanism, and
it belongs to a future toxin pathogen, not here.

#### 2.3.2 The strongest, most small-intestine-specific answer: **MUC17**

This is the item. It is cell-intrinsic (it sits on the brush border, it is
not secreted into the gel), it is region-specific to the small intestine,
and it does exactly what the Director wanted the upgrade to do.

- **MUC17 "protected small intestinal enterocytes against commensal and
  pathogenic bacteria."** In **non-inflamed Crohn's ileum**, reduced MUC17
  and a compromised glycocalyx barrier allowed **recurrent bacterial
  contact with enterocytes**. `Muc17` deletion made the small intestine
  "particularly prone to atypical bacterial infection **while maintaining
  resistance to colitis**", caused spontaneous deterioration of epithelial
  homeostasis, and produced **extraintestinal translocation of bacteria**.
  Layunta et al. 2024, *JCI Insight* 10(3):e181481 —
  [DOI](https://doi.org/10.1172/jci.insight.181481).
- **It is a sheddable decoy, and TNFα turns it up.** Long TNFα stimulation
  of Caco-2 cells raised MUC17 levels and apical insertion, **followed by
  shedding of MUC17-containing vesicles**; overexpression protected against
  **enteropathogenic *E. coli* adhesion**. The authors propose MUC17 as "a
  second line of defense … preventing attachment of bacteria to the
  epithelial cell glycocalyx in the small intestine." Schneider et al.
  2019, *Biochem J* 476:2281–95 —
  [DOI](https://doi.org/10.1042/BCJ20190180).

That second paper is a gift: the enemy it was tested against is **EPEC**,
and the game's A/E *E. coli* is the one enemy that profits from
inflammation. MUC17 goes **up** with TNFα. So this item is the game's first
host defence that gets *stronger* in inflamed tissue — a direct, cited
counter to the roster's unanswered "inflammation-as-fuel" problem.

#### 2.3.3 The secreted layer, ranked, with real numbers

| Effector | What it does | Real effect size |
|---|---|---|
| **REG3γ** | Maintains a **~50 µm zone physically separating the microbiota from the small-intestinal epithelial surface**. RegIIIγ⁻/⁻ mice lose that segregation: increased bacterial colonisation *of the epithelial surface* and enhanced adaptive activation. Gram-positive-selective. | A **standoff distance**, measured. Vaishnava et al. 2011, *Science* 334:255–8 — [DOI](https://doi.org/10.1126/science.1209791) |
| **α-defensin HD5** | Paneth-cell α-defensin. **HD5-transgenic mice were "markedly resistant" to oral challenge with virulent *S*. Typhimurium** — the first direct in-vivo demonstration of the magnitude of defensin effects. | Large but the **exact fold is UNVERIFIED** (not in the abstract; I did not obtain the figures). Salzman et al. 2003, *Nature* 422:522–6 — [DOI](https://doi.org/10.1038/nature01520) |
| **HD6** | **Not bactericidal.** After stochastic binding to bacterial surface proteins it **self-assembles into fibrils and nanonets that surround and entangle bacteria**, protecting against invasion by diverse enteric pathogens in vitro and in vivo. | Mechanically **identical to IgA enchainment but innate and knowledge-free**. Chu et al. 2012, *Science* 337:477–81 — [DOI](https://doi.org/10.1126/science.1218831) |
| **Lysozyme** | Real, but its gut function is governed by **trafficking, not amount**: commensals recruit Nod2 onto lysozyme dense-core vesicles, which is required for LRRK2 and Rab2a localisation; without them lysozyme is **degraded in lysosomes instead of secreted**. | A *mechanism*, not a dial. Weak as a standalone upgrade. Zhang et al. 2015, *Nat Immunol* 16:918–26 — [DOI](https://doi.org/10.1038/ni.3233) |
| **Mucin O-glycosylation** | The strong secreted-gel genetics are predominantly colonic; the small intestine's glycan barrier is the **membrane** mucin glycocalyx above. | Deprioritise for this map. |

#### 2.3.4 The spatial rule that should govern the whole item

**Secreted antimicrobial activity is retained in the mucus, not the lumen.**
Measured per compartment (crypt / mucus / lumen, % of extractable
activity): *Listeria* 48/44/8; *E. faecalis* 44/49/7; *B. megaterium*
56/42/2; *S. pyogenes* 48/46/6; *E. coli* 46/47/7; *S.* Typhimurium
38/43/19. The mucus-borne activity was "strong and **contact-dependent**."
Meyer-Hoffert et al. 2008, *Gut* 57:764–71 —
[DOI](https://doi.org/10.1136/gut.2007.141481).

So: **this item acts on adherents inside the mucus band and on the wall. It
does nothing to organisms riding the lumen flow.** That is a hard,
cited constraint and it keeps the item from overlapping with mucus
turnover (which acts on the flow) or IgA (which acts on the crowd).

#### 2.3.5 Game spec

**Name:** *Glycocalyx and peptide layer* (replaces
`HostBacterialResistance`). Three composable effects, each a level:

1. **Glycocalyx (MUC17).** Raises the depth threshold of §1b Step 1's
   proximity-gated adhesion roll — organisms must get *closer* to adhere —
   and gives each wall cell a small pool of **shed decoy vesicles**; a
   decoy consumes one adhesion attempt and is consumed with it, refilling
   over time. **Scales up with local inflammation** (TNFα), i.e. it reads
   the same `LocalInflammation` field A/E *E. coli* feeds on, with the
   opposite sign.
2. **Standoff (REG3γ).** Imposes a **minimum depth** the adherent mill
   cannot cross over covered wall, shrinking per level toward the seam.
   Mechanically this *lowers the crowding concentration* a plaque can
   reach without killing anything — which is the single cleanest way to
   slow plaque charge without adding a kill mechanic. Gram-positive
   selective: apply only to species flagged as such.
3. **Peptides (HD5 + HD6).** Per-tick, inside the mucus band only: a small
   kill chance (HD5) **plus** a replication-damping entanglement that
   cross-links neighbouring adherents into a non-charging raft (HD6) —
   i.e. the pre-adaptive version of IgA's sign-flip, available from the
   shop instead of the knowledge ladder.

Keep "less host damage per graze" as a **small** level-4 effect so the
original intent survives, but label it in the code as the weakest-evidenced
leg.

**Drawback: yes, two, both well cited.**
- **α-defensins reshape your own commensals, with an immune cost.** DEFA5-
  expressing mice had significant α-defensin-dependent changes in microbiota
  composition (but *not* total bacterial numbers), and **striking losses of
  segmented filamentous bacteria with fewer IL-17-producing lamina propria
  T cells**. Salzman et al. 2009, *Nat Immunol* 11:76–83 —
  [DOI](https://doi.org/10.1038/ni.1825). Game translation: levelling
  peptides degrades the commensal-health stat *and* down-tunes whatever
  γδ IEL / Th17-flavoured tower the game fields. A defence upgrade that
  weakens a tower line is exactly the shape the Director likes.
- **REG3γ is Gram-positive-selective** (Vaishnava 2011; and Brandl et al.
  2008, *Nature* 455:804–7 — [DOI](https://doi.org/10.1038/nature07250),
  where antibiotic-driven REG3γ loss abolishes in-vivo killing of
  vancomycin-resistant *Enterococcus*). Per the repo's own roster note, the
  jejunal/ileal lumen is **Gram-positive-dominated** — so this leg hits
  commensals hardest and barely touches the Gram-negative half of the
  roster. That asymmetry is free, accurate, and interesting.

---

### 2.4 Complement → **confirmed; build exactly as the ShopTuning comment says, plus one addition**

#### 2.4.1 The framing is confirmed

Wu et al. 2024, *Cell* 187:897–913 —
[DOI](https://doi.org/10.1016/j.cell.2023.12.036): C3 is **locally
synthesised in the gut, primarily by stromal cells**; it is **expressed
upon commensal colonisation** and **regulated by microbiota composition**,
giving each host an **individual luminal C3 level**; "the absence of
membrane attack complex (MAC) components in the gut ensures that C3
deposition does **not** result in the lysis of commensals"; pathogen
infection "triggers the immune system to **recruit neutrophils** to the
infection site for pathogen clearance"; and **"basal C3 levels directly
correlate with protection against enteric infection."**

That last clause is the whole justification for making it a stacking,
repeatable baseline buy — the code comment is already right.

The repo's two standing caveats hold and should stay in the design doc: it
is a **colon** paper, and the **trigger is the alternative pathway, not
antibody**. But both now have independent support beyond that paper:

Valencia-Bacca et al. 2026, *mBio* — complement in *Klebsiella pneumoniae*
gut colonisation. C3 and C4 rise on gut colonisation and **deposit on the
organism in the gut**; complement from rabbit, mouse and human sources
deposits C3b on bacteria grown under **gut-mimetic** conditions **even when
C4-dependent pathways are inhibited** (→ alternative pathway, antibody-
independent, confirmed in a second system); C3 is critical for **recruiting
myeloid cells to the gut** and for preventing lethal spread; depleting
*systemic* C3 showed that **mucosal-derived C3 controls gut colonisation**
specifically; **C4 is dispensable at baseline** and only matters at high
burdens after dysbiosis. Record: paper reached via Consensus, which did not
return a DOI on this tier — **DOI UNVERIFIED**, cite by journal and title.

#### 2.4.2 The addition nobody has recorded yet: coated organisms are also xenophagy targets

**C3 that coats an invader keeps working after the invader gets inside a
cell.** Intracellular C3-positive *Listeria monocytogenes* is targeted by
autophagy through a **direct C3/ATG16L1 interaction**, producing
autophagy-dependent growth restriction; C3-deficient mice showed defective
clearance at the intestinal mucosa after oral *Listeria*. And the counter
is published: ***Shigella flexneri* and *Salmonella* Typhimurium escape by
using outer-membrane proteases that cleave bound C3.** Because C3
"indiscriminately associates with foreign surfaces," the authors propose
this as **a universal mechanism of xenophagy initiation**. Sorbara et al.
2018, *Cell Host Microbe* 23:644–52 —
[DOI](https://doi.org/10.1016/j.chom.2018.04.008).

This is the best single find in this pass for the game, because it gives
complement a **second effect that reaches inside the host cell** — the
compartment §4b says is only addressable by a stress-sense roll or by
waiting — and it does so without any adaptive immunity, without lysis, and
with a published per-species counter.

#### 2.4.3 Game spec

**Name:** *Baseline complement* (`ComplementBaseline`, already enumerated).
Repeatable; level sets a scalar `C3Baseline`.

1. **Coating.** Every organism entering the barrier gets a `Coated`
   probability roll scaled by `C3Baseline`. Coating is **cosmetic damage-
   wise: it never deals a point of damage.** Commensals are never coated in
   a way that harms them — draw them coated and unbothered, which is the
   paper's headline and a good teaching beat.
2. **Recruitment.** A coated organism is an **end-target chemoattractant
   source** (see §1's cytokine-sensing re-spec — C5a is literally one of
   Heit's end-target molecules). Coated crowds therefore pull phagocytes
   *through* intermediary host-cytokine gradients. This is the mechanical
   link between two shop items and is worth building for that alone.
3. **Phagocyte efficiency.** A phagocyte engaging a coated organism kills
   faster / needs fewer contacts.
4. **Xenophagy tag (new).** If a coated organism gets inside a host cell,
   it carries the coat in, and the host cell gains a standing per-tick
   clearance chance against it — no tower, no death, no CTL. This stacks
   with the proposed autophagy item (§3.3) and is its cheapest entry point.
5. **Per-species counter.** Give invasive species a `C3Protease` flag.
   *Shigella* and *Salmonella* strip the coat (cited); *Listeria* does not.
   Visible: the coat shader fades over a second or two and the player can
   see which enemies defeat the upgrade.
6. **Ceiling is microbiota-dependent.** Wu 2024 found luminal C3 is set by
   microbiota composition. So cap `C3Baseline` by the commensal-health stat
   (§5). That makes complement the item that *punishes* over-flushing and
   over-peptiding — a systemic coupling the shop currently lacks entirely.

**Drawback: yes — and it is the one the game most wants.** C3 opsonisation
works by **recruiting neutrophils and myeloid cells into the gut** (Wu
2024; Valencia-Bacca 2026). This game already has §6's
neutrophil-overuse → fibrosis axis and A/E *E. coli*'s
inflammation-as-fuel. Complement should therefore raise local inflammation
per coated kill. Secondary, cited: the C3 ceiling falls if the player has
degraded their commensals.

---

## 3. Proposed new items, ranked

Ranked by (evidence strength × fit with the existing wall model × whether
it does something no current item does).

### 3.1 **Interferon-λ (type III) — the epithelial antiviral alarm** — RANK 1

**Yes: IFN-λ is the honest epithelial antiviral upgrade for a gut map, and
type I is the wrong one.** This is the best-evidenced new item of the lot
and the evidence is unusually clean.

**Why λ and not I.**
- IFN-λ**R**-deficient suckling *and* adult mice were impaired in
  controlling oral rotavirus, while mice lacking functional type I IFN
  receptor "were similar to wild-type." Using Mx1 accumulation as a
  per-cell readout, **intestinal epithelial cells — the prime rotavirus
  target — responded strongly to IFN-λ and only marginally to type I IFN
  *in vivo*.** Systemic IFN-λ repressed rotavirus replication in the gut;
  **type I treatment was not effective.** Pott et al. 2011, *PNAS*
  108:7944–9 — [DOI](https://doi.org/10.1073/pnas.1100552108).
- Genetically, the responsive cell is the **IEC**: lineage-specific
  deletion of `Ifnlr1` in IECs phenocopies the whole-animal knockout for
  **murine norovirus and reovirus** — higher tissue titres, more faecal
  shedding — and IFN-λ's *sterilising* clearance of persistent norovirus
  requires IEC responsiveness. Baldridge et al. 2017, *J Virol* 91:e02079-16
  — [DOI](https://doi.org/10.1128/JVI.02079-16).
- **The reason the epithelium uses λ is damage control, and it is
  measured.** In neonatal mouse IECs, type III gives robust uniform ISG
  expression and blocks rotavirus; type I gives a marginal response and
  does not. In organoids (which upregulate the type I receptor), type I
  gives a strikingly *expanded* response that includes **proapoptotic
  genes and potentiates TNF-α toxicity**; the shared type I/III ISGs have
  **strong ISRE promoter motifs** while the type-I-only proapoptotic set
  has **weak** ones. The authors' conclusion: preferential λ
  responsiveness "confers antiviral protection but **minimizes disruption
  of intestinal homeostasis**." Van Winkle et al. 2020, *J Virol*
  94:e00603-20 — [DOI](https://doi.org/10.1128/JVI.00603-20).
- **It has a published amplifier.** IL-22 from ILC3 and IFN-λ **act
  synergistically** on STAT1 and ISG induction, both receptors
  "preferentially expressed by intestinal epithelial cells," and the
  synergy is needed to curtail rotavirus. Hernández et al. 2015, *Nat
  Immunol* 16:698–707 — [DOI](https://doi.org/10.1038/ni.3180).

**Game effect.** A board-wide, **epithelium-only**, time-limited antiviral
state, bought as a repeatable level that (a) shortens the ramp to the state
and (b) deepens it. While active: virus entry chance down, intracellular
replication slower, and the §4b virus death clock shorter. **It must do
absolutely nothing to any bacterium** — that specificity is the teaching
beat and it is exactly what the literature supports. Trigger it off the
dsRNA sensor's uncoupled cytokine output (§2.1.4 item 3) so the two items
compose. Price it high (≈70 base) and make the ramp long enough that
buying it mid-wave is too late.

**Drawback: outstanding, and it closes a loop with the dsRNA item.**
Jena et al. 2024, *Cell* 187:7533–50 —
[DOI](https://doi.org/10.1016/j.cell.2024.10.010): "**Type III, not type I
or type II, interferons delay epithelial cell regeneration**" by
upregulating **ZBP1**; Z-nucleic acids formed after intestinal damage are
sensed by ZBP1 → caspase-8 → cleaved **gasdermin C** → **pyroptosis**,
delaying repair of the **small intestine after irradiation** and the large
intestine after colitis; the axis is active in IBD patients.

So the honest IFN-λ item is: **buy antiviral protection, pay in slower
regrowth and messier deaths** — through the *same ZBP1 node* as §2.1's
messy branch. That is a drawback with a mechanism, a shared molecule, and a
direct hook into the game's clean/messy and fibrosis systems. If only one
new item gets built, build this one.

### 3.2 **Xenophagy (ATG16L1) — the host cell cleans itself out** — RANK 2

**What no current item does:** clear an intracellular resident **without a
CTL and without the host cell dying**. §4b/§4c currently offer only a
stress-sense roll, a CTL kill, or waiting it out.

**Mechanism and effect size.**
- Epithelium-specific `Atg16l1` deletion (`Atg16l1^f/f × Villin-cre`):
  *Salmonella* no longer colocalised with LC3 in the epithelium; the mice
  also had **fewer Paneth cells with abnormal granule morphology and
  reduced antimicrobial-peptide expression**; and they showed increased
  inflammation and **systemic translocation of bacteria**. CD11c-specific
  deletion did almost nothing — it is the epithelium that matters. Conway
  et al. 2013, *Gastroenterology* 145:1347–57 —
  [DOI](https://doi.org/10.1053/j.gastro.2013.08.035).
- The trigger is mechanistically specified and **has a published pathogen
  counter**: on vacuolar damage, host **V-ATPase recruits ATG16L1** onto the
  bacteria-containing vacuole; *Salmonella*'s T3SS effector **SopF**
  ADP-ribosylates ATP6V0C Gln124 and blocks this, specifically killing
  xenophagy without touching canonical autophagy. **ΔsopF *Salmonella* is
  mostly captured by autophagy**, and inhibiting autophagy promotes
  proliferation in vivo. Xu et al. 2019, *Cell* 178:552–66 —
  [DOI](https://doi.org/10.1016/j.cell.2019.06.007) *(DOI reconstructed
  from the Consensus record — **verify before quoting**)*.
- And it is the downstream half of the complement item: **C3/ATG16L1**
  direct interaction initiates it (Sorbara 2018, above).

**Game effect.** A per-tick clearance chance against an intracellular
resident, resolved by the host cell alone. **Cap it low** — in the real
system a fraction escape and the whole point of the SopF work is that
pathogens target this pathway first. Per-species: *Listeria* is
susceptible, *Salmonella*/*Shigella* carry the counter (effector or C3
protease). Composes with: complement (coating supplies the tag), the dsRNA
sensor (RNase L products flip autophagy → apoptosis — Siddiqui 2015 — so a
high sensor level *converts* xenophagy attempts into deaths, which is a
real and interesting interaction between two shop rows).

**Drawback, cited.** Autophagy in the epithelium is **load-bearing for
Paneth cells**: Conway 2013's epithelial knockouts had abnormal granules
and less AMP. Honest direction of the trade-off is therefore not
"autophagy costs you AMP" but the reverse — so the game's drawback should
be the *dependency*, not a penalty: xenophagy's clearance chance should
**scale with the peptide-layer item (§2.3)**, so the player cannot buy it in
isolation. Secondary, softer: ATG16L1 hypomorphism is a Crohn's risk
allele, so over-reliance is thematically the "chronic inflammation"
pathway — frame as flavour, not mechanics, unless a number turns up.

### 3.3 **Epithelial restitution (trefoil factors) — close the gap, don't grow a cell** — RANK 3

**Why it is a distinct item from the crypt**, and this is the crisp part:
trefoil peptides had **no significant effect on proliferation** in any of
six cell lines, yet produced a **3–6× increase in the rate of epithelial
migration into a wound**, rising to **up to 15×** when combined with mucin
glycoproteins purified from small intestine or colon — and the synergy was
mucin-specific (non-mucin glycoproteins did nothing) and TGF-β-independent.
Dignass et al. 1994, *J Clin Invest* 94:376–83 —
[DOI](https://doi.org/10.1172/JCI117332). Reviewed with the TFF1/2/3
division of labour in Aihara, Engevik & Montrose 2016, *Annu Rev Physiol*
78:309–32 — [DOI](https://doi.org/10.1146/annurev-physiol-021115-105256)
*(DOI reconstructed from the Consensus record — **verify before
quoting**)*: "in response to acute GI mucosal injury, TFF peptides
accelerate cell migration to **seal the damaged area from luminal
contents**."

**Game effect.** On a wall slot becoming `Empty` (or `Dead` once debris is
cleared), neighbours **slide over to cover it** at 3–6× the crypt's
regrowth rate — the slot stops being a *weak spot* (§1b: dead wall charges
markedly faster) long before it is a real cell again. Introduce a fourth
host state, `Covered`: it blocks invasion and charges like intact wall, but
a virus **gains nothing there** (it is not a fresh host cell) and it
reverts to `Healthy` only on normal crypt timing. That single state
resolves a tension the current model has: the player currently has no fast
answer to a dead stretch, and the only answer (crypts) is positional and
slow.

**Drawback: the mucin dependency, and it is cited.** The 15× needs mucin
glycoproteins. So restitution's rate should **scale with local mucus**,
which makes it anti-synergistic with maxed mucus turnover — a player who
flushes aggressively heals slowly. Two existing shop items now pull against
each other for a published reason. The further claim that a `Covered` sheet
is more fragile than real epithelium is plausible but **UNVERIFIED**;
don't build a fragility penalty on it.

### 3.4 **Rapid fucosylation — pacify the colonists** — RANK 4 (most elegant, genuinely two-edged)

**Mechanism, fully specified and fast.** Systemic TLR-ligand exposure
causes **rapid α(1,2)-fucosylation of small-intestinal epithelial cells**
in mice, requiring TLR sensing → dendritic-cell **IL-23** → innate lymphoid
cell activation → **IL-22** → **Fut2** in IECs. Fucosylated proteins are
shed into the lumen, fucose is liberated and metabolised by the microbiota,
and — the key clause — this **"reduces the expression of bacterial
virulence genes"** and improves host tolerance of *Citrobacter rodentium*.
Pickard et al. 2014, *Nature* 514:638–41 —
[DOI](https://doi.org/10.1038/nature13823).

**Game effect.** The game has no other upgrade that **lowers virulence
instead of killing**. Buy it and adherent bacteria's damage-per-step and
plaque charge-rate contribution both drop, without any of them dying — a
third category alongside "kill" and "flush." Short ramp (hours, in the
paper), so it is a reactive buy. Also feeds commensal health (the fucose is
food), which raises the complement ceiling.

**Drawback: the best one on this list, because it is the same molecule.**
Fucose/H-antigen is **the norovirus and rotavirus receptor** — and §2.2.2's
numbers say so in both directions: secretors are the ones who get infected
(155/155 GII.4 cases; Currier 2015), and non-secretors' altered
glycan-metabolising microbiome comes with sub-clinical inflammation (Tong
2014). **So fucosylation pacifies the bacteria and opens the door to the
viruses.** A single, cited, perfectly symmetric trade-off, and it directly
inverts §2.2's "mask the receptor" item — the two items are literally
opposites, which makes a mixed round a real decision.

### 3.5 **Motility / migrating motor complex — buy the flow** — RANK 5 (best board fit, weaker evidence class)

The migrating motor complex is a cyclic fasting motility pattern
interrupted by feeding; phase III can be induced by motilin, erythromycin
or ghrelin (antral origin) or serotonin/somatostatin (duodenal origin);
and **its absence "has been associated with gastroparesis, intestinal
pseudo-obstruction and small intestinal bacterial overgrowth."** Deloose
et al. 2012, *Nat Rev Gastroenterol Hepatol* 9:271–85 —
[DOI](https://doi.org/10.1038/nrgastro.2012.57). Independently: "Bacterial
overgrowth with Gram-negative bacilli is **the consequence of severe small
bowel dysmotility**." Husebye 1999, *Neurogastroenterol Motil* 11:141–61 —
[DOI](https://doi.org/10.1046/j.1365-2982.1999.00147.x).

**Game effect.** The most *legible* upgrade possible on this map: it raises
§1a's lumen flow speed and the downstream drift rate of adherents, so
fewer organisms ever adhere and adherents reach the far end and are
excreted sooner. It plugs straight into Sprint 19's peristaltic surge,
which is already on screen. Nothing new needs drawing.

**Honesty.** The evidence is **associative** (absent MMC ↔ SIBO), not an
intervention study showing "faster transit → N% less colonisation":
treat the magnitude as **UNVERIFIED** and tune freely.

**Drawback.** Faster transit means less contact time for absorption. The
game already has the currency: a standing **ATP income penalty** per level.
That is the honest cost and it needs no new system.

### 3.6 **Bile acid / FXR enteroprotection** — RANK 6 (real, hard to draw)

Obstruction of bile flow causes bacterial proliferation and mucosal injury
in the small intestine and bacterial translocation; **FXR, the nuclear bile
acid receptor, induces enteroprotective genes and inhibits both bacterial
overgrowth and mucosal injury in ileum** after bile-duct ligation, and
**FXR-null mice have increased ileal bacterial levels and a compromised
epithelial barrier**. Inagaki et al. 2006, *PNAS* 103:3920–5 —
[DOI](https://doi.org/10.1073/pnas.0509592103).

**Game effect.** A lumen-wide suppression of adherent bacterial
replication, **strongest at the distal end of the map** because it is an
ileal mechanism — which makes it positionally interesting on a board that
already has a Peyer's patch at ~60% and a crypt sequence. **Rank 6 because
it is a transcriptional *program*, not one effector**, so it is hard to
draw and hard to explain in a tooltip.

### 3.7 **Tight-junction reinforcement — do NOT build as a buff** — RANK 7 / reject

The player-facing instinct is right (the game's *V. cholerae* already
loosens junctions: claudin-2 up, tricellulin down, plaque charges faster),
but the evidence does not support tightening as a free upgrade.

- Occludin and ZO-1 have essential **non-barrier** functions (epithelial
  apoptosis and proliferation, and occludin **facilitates viral entry**),
  and — this is the awkward part — "surprisingly, **neither is required for
  intestinal barrier function or overall health in the absence of
  exogenous stressors**." Kuo et al. 2022, *Ann N Y Acad Sci* 1514:21–33 —
  [DOI](https://doi.org/10.1111/nyas.14798) *(DOI reconstructed from the
  Consensus record — **verify before quoting**)*.
- And the strong small-intestinal genetics run the *other* way: the
  "Na⁺-leaky" claudin-15 is **indispensable** — `Cldn15⁻/⁻` adults had
  abnormally low luminal Na⁺ measured directly in vivo and **impaired
  glucose absorption** on oral glucose tolerance testing. Tamura et al.
  2011, *Gastroenterology* 140:913–23 —
  [DOI](https://doi.org/10.1053/j.gastro.2010.08.006).
- The positive evidence for "tighten the junctions" is largely
  probiotic-conditioned-medium in Caco-2/T84 monolayers (e.g. Alvarez et
  al. 2019, *BMC Microbiol* 19:166 —
  [DOI](https://doi.org/10.1186/s12866-019-1534-3) *(DOI reconstructed —
  **verify**)*) — in vitro, strain-specific, and about *preventing
  pathogen-induced loss*, not about exceeding baseline.

**Verdict:** keep tight junctions as a **pathogen lever** (which the game
already does well) and, if a host item is wanted anyway, price it with the
cited absorption cost: paracellular Na⁺ down → glucose absorption down →
**ATP income down**. A tightened wall that starves you is a far better
item than a free one.

### 3.8 **Goblet-cell hyperplasia — do NOT build as a buff** — RANK 8 / reject

The circuit is real and beautifully specified: tuft cells constitutively
express **IL-25** to sustain ILC2s; on helminth infection, tuft-derived
IL-25 drives ILC2 **IL-13**, which acts on crypt progenitors to make more
**tuft and goblet** cells — the "weep and sweep" response. von Moltke et al.
2016, *Nature* 529:221–5 — [DOI](https://doi.org/10.1038/nature16161).

But the cost is now measured and it hits the exact system this game cares
about: **goblet cells mechanically induce tight-junction fractures between
neighbouring enterocytes under homeostatic conditions in vivo**, an effect
**exacerbated by goblet-cell hypertrophy and associated with increased gut
permeability**, arising from a force imbalance (goblet cells press on
adjacent enterocytes; rupture depends on myosin-II-controlled tissue
rheology). Creff et al. 2026, *Nat Commun* 17(1) —
[DOI](https://doi.org/10.1038/s41467-026-76034-0).

**In this game's terms, goblet hyperplasia buys mucus and pays in faster
plaque charge.** That is not a bad item — it is a *cost* that should live on
the existing mucus row, or a trait for a mucus-farming enemy (EAEC is
already shortlisted for exactly this inversion). Don't give it its own buy.

### 3.9 **Colonisation resistance from commensals** — not an item; make it a **stat**

The concept is solid — a normal microbiota "can generate conditions in the
gut that disfavor colonization of enteric pathogens," transiently disrupted
by antibiotics, diet, probiotics and disease, with proteobacterial blooms
as the signature of a disturbed community (Stecher 2015, *Microbiol Spectr*
3(3) — [DOI](https://doi.org/10.1128/microbiolspec.MBP-0008-2014); Gart et
al. 2016, *Front Microbiol* 7:1827 —
[DOI](https://doi.org/10.3389/fmicb.2016.01827)) — but it is **not a thing
the host buys**. It is the substrate several of the above items spend.

**Build it as `CommensalHealth`** (§5), not as a shop row: it is set by
colonisation, it gates the complement ceiling (Wu 2024), it is degraded by
peptides (Salzman 2009), by glycan masking (Tong 2014) and by
over-flushing, and it is fed by fucosylation (Pickard 2014). Every item
above touches it. That is a better use of the idea than a button.

---

## 4. Drawbacks worth building

Ranked by evidence quality, with the game system each one lands in.

| # | Drawback | Mechanism and effect | Lands in | Citation |
|---|---|---|---|---|
| 1 | **IFN-λ delays repair and makes the deaths messy** | Type III (not I or II) IFN upregulates **ZBP1**; Z-nucleic acids from damage → caspase-8 → cleaved **gasdermin C** → pyroptosis; delays small-intestinal repair after irradiation and colonic repair after colitis; active in IBD patients. | §6 regrowth + fibrosis; shares the ZBP1 node with the dsRNA item | Jena 2024, *Cell* — [DOI](https://doi.org/10.1016/j.cell.2024.10.010) |
| 2 | **Over-active epithelial extrusion is itself a disease** | "Excessive inflammasome activation in IECs was **sufficient** to result in diarrhea and pathology." | Levelled extrusion costs board coverage with no pathogen present | Rauch 2017, *Immunity* — [DOI](https://doi.org/10.1016/j.immuni.2017.03.016) |
| 3 | **Necroptosis recruits neutrophils and makes outcomes worse** | Nuclear MLKL death is "a potent activator of neutrophils"; MLKL-deficient mice had less nuclear disruption, **decreased neutrophil recruitment and increased survival** of lethal influenza. | The messy branch feeds fibrosis **and** A/E *E. coli*'s `LocalInflammation` | Zhang 2020, *Cell* — [DOI](https://doi.org/10.1016/j.cell.2020.02.050) |
| 4 | **Defensins reshape your own commensals and cost you Th17** | DEFA5-expressing mice: significant microbiota compositional change (not total load), **striking loss of segmented filamentous bacteria, fewer IL-17⁺ lamina propria T cells**. | `CommensalHealth` down; down-tunes the Th17/γδ-flavoured tower line | Salzman 2009, *Nat Immunol* — [DOI](https://doi.org/10.1038/ni.1825) |
| 5 | **Glycan masking buys autoimmunity and a reprogrammed microbiome** | Non-secretors: Crohn's risk; perturbed microbial energy metabolism (enriched glycan/lipid, depleted amino-acid biosynthesis) with **metaproteomic sub-clinical inflammation**, replicated in FUT2⁻/⁻ mice. Plus T1D **OR 1.29**, p = 4.3 × 10⁻¹⁸. | Permanent `CommensalHealth` penalty + background inflammation | Tong 2014, *ISME J* — [DOI](https://doi.org/10.1038/ismej.2014.64); Smyth 2011, *Diabetes* — [DOI](https://doi.org/10.2337/db11-0638) |
| 6 | **A tighter wall starves you** | `Cldn15⁻/⁻`: low luminal Na⁺ in vivo, **impaired glucose absorption**. | Direct **ATP income** penalty — the game's existing currency | Tamura 2011, *Gastroenterology* — [DOI](https://doi.org/10.1053/j.gastro.2010.08.006) |
| 7 | **More goblet cells crack the junctions** | Goblet cells mechanically fracture TJs between neighbours; worse with hypertrophy; **increased gut permeability**. | Plaque charges faster — exactly the mucus item's cost | Creff 2026, *Nat Commun* — [DOI](https://doi.org/10.1038/s41467-026-76034-0) |
| 8 | **Anti-entry hardening backfires on one species** | IFITM2/IFITM3 are **entry factors** for HCoV-OC43; all three IFN types **promote** that infection. | One roster virus gains entry chance per IFITM level | Zhao 2014, *PNAS* — [DOI](https://doi.org/10.1073/pnas.1320856111) |
| 9 | **…and it taxes the host's own fusion** | IFITM1/2/3 block syncytin-1/-2-mediated trophoblast fusion; IFITM knockdown *increases* spontaneous fusion. The authors name the "critical balance" explicitly. | Any modelled host fusion event slows (hook for later) | Zani 2019, *J Biol Chem* — [DOI](https://doi.org/10.1074/jbc.AC119.010611) |
| 10 | **Why type I IFN must never be the gut item** | In IEC organoids, type I induces an expanded ISG set **including proapoptotic genes** and **potentiates TNF-α toxicity**; the type-I-only genes have weak ISRE motifs. | Design constraint, not a shop row: offer λ, not I | Van Winkle 2020, *J Virol* — [DOI](https://doi.org/10.1128/JVI.00603-20) |
| 11 | **Flushing throws away your peptides** | Only **2–19%** of secreted antimicrobial activity is in luminal content; **38–56%** sits in crypts and **42–49%** in the mucus layer, where it is strong and contact-dependent. | Mucus turnover vs the peptide-layer item, directly opposed | Meyer-Hoffert 2008, *Gut* — [DOI](https://doi.org/10.1136/gut.2007.141481) |
| 12 | **Losing commensals costs you a real effector** | Antibiotics markedly downregulate intestinal REG3γ and **decrease in vivo killing of VRE**; re-stimulating epithelial TLR4 with oral LPS restores it. | The rebound path for a wrecked `CommensalHealth` — a *recovery* mechanic, not just a penalty | Brandl 2008, *Nature* — [DOI](https://doi.org/10.1038/nature07250) |

**Which are the best-evidenced?** #1, #2, #3 and #4 — all four are
primary-paper findings stated as the paper's own conclusion, all four land
on systems the game already has (regrowth, fibrosis, neutrophil surge,
tower lines), and #1/#3 share a molecule (ZBP1/MLKL-family) with the dsRNA
item, which is the kind of coupling the shop currently lacks entirely.

---

## 5. What needs new mechanics

Ordered by how much the specs above depend on it.

1. **Epithelial extrusion — a host cell leaving the board through the
   lumen.** The single biggest new mechanic, and §2.1 is built on it. Needs:
   a `Healthy/Infected → Empty` transition that produces **no debris**, and
   a path from the host layer back into §1a's flow band carrying the
   intracellular resident alive. Nothing in the current model lets tissue
   return something to the lumen.
2. **A clean/messy flag on every host-cell death.** Debris quantity,
   DC-attractant strength and fibrosis contribution should all key off one
   enum (`Apoptotic | Pyroptotic | Necroptotic | Extruded`). §4c's CTL
   already needs this ("what it buys is a clean death"); §2.1 and §3.1
   both need it; §6's fibrosis should read it.
3. **An `Opsonised` flag on organisms, readable by towers, with per-species
   `C3Protease`.** §2.4. Visible coat shader that fades when stripped.
4. **A standoff depth field at the wall.** REG3γ's ~50 µm zone becomes a
   per-lane minimum depth the adherent mill cannot cross. §2.3. This is the
   cleanest way to slow plaque charge without a kill, and the mill already
   has a depth coordinate to clamp.
5. **An intracellular clearance route that is neither a tower nor a
   death.** §3.2. §4b currently offers only the stress-sense roll, a CTL,
   or waiting; xenophagy is a fourth outcome resolved by the host cell.
6. **Two chemoattractant classes with a hierarchy.** End-target (organism,
   C5a) must dominate intermediary (host cytokine) at very low relative
   concentration, and the cytokine-sensing upgrade sharpens the *hierarchy*
   rather than a gain. §1 table. This is also what makes complement's
   recruitment leg mean anything.
7. **A per-species receptor/glycan compatibility flag.** §2.2.2. Binary,
   knowledge-gated, and the exact inverse of the fucosylation item.
8. **`CommensalHealth`** — one scalar, set by colonisation, gating the
   complement ceiling and REG3γ reach, degraded by peptides / glycan
   masking / over-flushing, fed by fucosylation, recoverable via the
   TLR-restimulation route (#12 above). §3.9. Several items are
   uninteresting without it.
9. **A `Covered` host state** — covered by a migrated sheet: blocks
   invasion, charges like intact wall, is not a viral host, reverts to
   `Healthy` on crypt timing. §3.3.
10. **Virulence as a modifiable per-organism property**, so damage-per-step
    and charge contribution can be lowered without killing. §3.4. The game
    has kill and flush; this is the third verb.
11. **Lumen flow speed as a purchasable board parameter**, coupled to ATP
    income. §3.5.
12. **An `IfitmInverted` / "this upgrade helps me" per-species trait.**
    Needed by #8 in the drawbacks table and by the fucosylation item; worth
    generalising into a per-species "upgrade interaction" table rather than
    special-casing each.

---

## 6. Loose ends and honesty log

- **Salzman 2003's HD5 protection magnitude: UNVERIFIED.** The abstract says
  only "markedly resistant"; I did not obtain the figures. Do not quote a
  fold or an LD50 shift.
- **No transferable IFITM3 entry-reduction coefficient exists** that I could
  find. The usable numbers are the mouse phenotype and the human odds
  ratios (1.69; 2.70 for severe), both in §2.2.1.
- **No transferable sIgA adhesion-reduction coefficient exists either** —
  already recorded in `IGA_REFERENCE.md` §3 and unchanged by this pass.
- **The sentinel goblet cell is colonic** (Birchenough 2016). The small
  intestine has no comparable thick inner mucus layer. §1b's sentinel
  sloughing is an extrapolation and the design doc should say so. The
  *small-intestinal* equivalents it can legitimately lean on are the mucus-
  retained peptide layer (Meyer-Hoffert 2008) and the MUC17 glycocalyx
  (Layunta 2024).
- **Human vs mouse on epithelial extrusion**: mice use NAIP/NLRC4, humans
  use caspase-4/gasdermin-D (Naseer 2022). Name the mechanic "extrusion."
- **Four DOIs in §3 were reconstructed from Consensus records** rather than
  returned by PubMed and are flagged inline: Xu 2019 (*Cell*, SopF),
  Aihara 2016 (*Annu Rev Physiol*, TFF review), Kuo 2022 (*Ann N Y Acad
  Sci*, occludin/ZO-1), Alvarez 2019 (*BMC Microbiol*, EcN OMVs). Verify
  before any of these reaches `GAME_DESIGN.md`.
- **Valencia-Bacca 2026 (*mBio*) has no DOI in the record I retrieved** —
  cite by journal and title until confirmed.
- **Wu 2024's caveats stand unchanged** from
  `docs/research/complement_verification.md`: colon-only, alternative
  pathway not antibody, no replication of the C5–C9 survey, and
  Gram-positives are structurally MAC-proof anyway.
