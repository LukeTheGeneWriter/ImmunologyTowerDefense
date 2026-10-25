# Adaptive-tower upgrade rows — proposal, evidence and naming corrections

**Scope.** The four adaptive towers: dendritic cell, helper T, cytotoxic T cell,
B cell → plasma cells. Dendritic cell and helper T have three live rows each
(`UI/ProgenitorUpgradeCatalog.cs` + `Units/ProgenitorUpgrades.cs`); the CTL and
the B cell have none. This document reviews the existing nine, proposes a final
set per tower, and lists every place the current naming misuses an
immunological term.

**Method.** Every claim below was checked against the literature this session
using the PubMed and Consensus MCP tools; nothing is written from memory. A
substantial part of this document is based on articles retrieved from
**PubMed**, with DOI links given per claim. Claims that could not be resolved
are marked **UNVERIFIED** rather than quietly dropped.

**Three headline corrections, stated up front:**

1. **"TCR affinity maturation" is wrong and should be renamed to "Functional
   avidity maturation."** T cells do not somatically hypermutate; the published
   term for exactly the effect the game models is *functional avidity*, and it
   is a >50-fold real effect.
2. **"Macropinocytosis rate" is on the wrong row.** Macropinocytosis is antigen
   *uptake*, and it is irreversibly switched **off** when a DC matures and
   leaves for the node — i.e. precisely during the trip the row claims to
   improve. The row that writes `DcPresentationsPerCargo` should be named for
   **pMHC-II complex stabilisation**; the row that writes the debris bite should
   be named for **limited lysosomal proteolysis**.
3. **The ~30% memory-T rung's "burst of CTLs" is backwards, confirmed.** Peak
   secondary magnitude is roughly *invariant* to how much memory you have. What
   memory buys is precursors already in place, effector function within hours,
   and a much gentler contraction — so the rung should make CTLs **persist and
   start instantly**, not spawn extra ones.

A fourth, found while checking whether the caps bind: **at the current species
tags, the "Hamming distance 2" specificity guarantee in `AdaptiveTuning.cs` is
already false**, and the TCR row's level-2 cap (r≤4) switches the barcode off
entirely. Numbers in §5.

---

## 1. Final recommended rows

### 1a. Dendritic cell — 3 rows (all three kept in *effect*, two renamed)

| Row | Player-facing effect | Field written, per level | Price / cap | Citation |
|---|---|---|---|---|
| **pMHC-II stabilisation** <br>*(was "Macropinocytosis rate")* | Each trip to the node is worth more presentations before the cell goes back for antigen. | `AdaptiveUnitTuning.DcPresentationsBonus`: `DcPresentationsPerCargo` 4 → **+2/lvl** (unchanged) | 35 / 3 | Cella 1997 *Nature* — class-II t½ 10 h → **>100 h** on maturation, "capable of stimulating T cells even after several days" [DOI](https://doi.org/10.1038/42030); Lazarski 2005 *Immunity* [DOI](https://doi.org/10.1016/j.immuni.2005.05.009) |
| **CCR7 expression** <br>*(kept, name and field both correct)* | The cell migrates faster — antigen reaches the node sooner. | `AdaptiveUnitTuning.DcFineTilesBonus`: `DcFineTilesPerTick` 3 → **+1/lvl** (unchanged) | 40 / 3 | Riol-Blanco 2005 *J Immunol* — CCR7 drives migratory **speed** through a Rho/Pyk2/cofilin module that is independent of its chemotaxis module [DOI](https://doi.org/10.4049/jimmunol.174.7.4070) |
| **Limited lysosomal proteolysis** <br>*(was "Antigen-sparing sampling")* | Takes a smaller bite of each debris pile and still learns as much — leaves more for macrophage clearance. | `AdaptiveUnitTuning.DcDebrisBiteReduction`: `DcDebrisSamplePerBite` 0.34 → **−0.08/lvl**, floor 0.1 (unchanged) | 30 / 3 | Delamarre 2005 *Science* — macrophages are protease-rich and destroy antigen; DCs are protease-**poor**, degrade slowly, retain antigen for extended periods, and limited proteolysis *favours* presentation [DOI](https://doi.org/10.1126/science.1108003) |

No field change on any DC row. All three are renames or confirmations.

### 1b. Helper T — 3 rows (one kept, one renamed, one re-specced)

| Row | Player-facing effect | Field written, per level | Price / cap | Citation |
|---|---|---|---|---|
| **Clonal expansion** <br>*(kept — correct term, and the cap now binds)* | Proliferates faster and holds more helper-T cells in the node — a busier node teaches faster. | per-tower `MaxActiveChildren` 4 → **+2/lvl**, and `EmissionRateMultiplier` = 1 + 0.5·lvl (unchanged) | 35 / 3 | Yoon 2010 *PLoS ONE* — initial CD8 division as short as **2 h**, and cycle time is set by *the antigenic stimulus the APC provides*, which in turn sets expansion rate and effector numbers [DOI](https://doi.org/10.1371/journal.pone.0015423) |
| **Functional avidity maturation** <br>*(was "TCR affinity maturation" — a misnomer; see §4)* | A near-miss barcode still teaches — more pairings count. | `AdaptiveUnitTuning.MatchDistanceBonus`: `MatchMaxHammingDistance` 2 → **+1/lvl**, **cap 1 (not 2)**, and lower `MatchDistanceCeiling` 4 → 3 | 45 / **1** | Slifka & Whitton 2001 *Nat Immunol* — functional avidity rose **>50-fold** in vivo "without any appreciable change in TCR affinity"; "unlike B cells, T cells lack the capacity to improve the affinity of their antigen receptors by somatic mutation" [DOI](https://doi.org/10.1038/90650); Mason 1998 *Immunol Today* — one TCR productively engages ~10⁶ peptides [DOI](https://doi.org/10.1016/s0167-5699(98)01299-7) |
| **Lymph-node egress** <br>*(was "Rapid recirculation" — name defensible, framing and cost were not)* | Helper-T cells cycle out of the node sooner, so the repertoire refreshes faster — **but the node holds fewer of them.** | `AdaptiveUnitTuning.LymphocyteLifespanReduction`: `LymphocyteLifespanSeconds` 20 → **−4/lvl**, floor 6 (unchanged field and step; the blurb must name the cost) | 30 / 3 | Thomas 2012 *PLoS ONE* — modal LN transit **10–20 h**, strongly right-skewed, many cells >70 h [DOI](https://doi.org/10.1371/journal.pone.0045262); Reichardt 2013 *EMBO J* — cells that egress **promptly** return to the parenchyma less often and prime **less** effectively [DOI](https://doi.org/10.1038/emboj.2013.33); Macallan 2004 *J Exp Med* — naive T cells are the long-lived population (intermitotic time ≈ 1 yr) [DOI](https://doi.org/10.1084/jem.20040341) |

### 1c. Cytotoxic T cell — 4 rows, all new (two use existing fields, two need new ones)

| Row | Player-facing effect | Field written, per level | Price / cap | Citation |
|---|---|---|---|---|
| **Granzyme B loading** | Each second of contact does more to an infected cell — it dies sooner. | `UnitLifecycleTuning.StressSenseChancePerTick` 0.03 → **+0.01/lvl** | 45 / **2** (0.05 = 17× innate; the Director's ceiling is ~20×) | Liadi 2015 *Cancer Immunol Res* — CD4 CARs kill and multi-kill **more slowly than CD8, "likely due to the lower granzyme B content"** [DOI](https://doi.org/10.1158/2326-6066.CIR-14-0195); Weigelin 2021 *Nat Commun* — hits are perforin-dependent and graded, "accelerating damage **magnitude** and frequency may improve immune efficacy" [DOI](https://doi.org/10.1038/s41467-021-25282-3) |
| **Interstitial patrol** | The cell moves between infected cells faster, landing hits on more of them before they recover. | `UnitLifecycleTuning.FineTilesPerTick` 3 → **+1/lvl** | 40 / 2 (→5; measured ratio ≈6×) | Weigelin 2021 — damage is delivered by **migrating CTLs transiting between targets**, and multi-hit killing is most effective "in interstitial tissue where high CTL densities and **swarming** support frequent serial encounters" [DOI](https://doi.org/10.1038/s41467-021-25282-3); Torres 2023 *eLife* — effector CD8 at 6.5 µm/min in small-intestinal villi [DOI](https://doi.org/10.7554/eLife.84916) |
| **Immune-synapse release** | The cell lets go of a dying target sooner and moves to the next one. | **NEW FIELD** `UnitLifecycleTuning.PostKillDetachSeconds` (propose default 1.5 s, **−0.4/lvl**, floor 0.3) | 35 / 3 | Sanchez 2022 *Nat Immunol* — apoptotic cytoskeletal contraction is necessary **and sufficient** for CTL dissociation, and amplifying it "promoted **faster CTL detachment and serial killing**" [DOI](https://doi.org/10.1038/s41590-023-01572-4); counter-direction: Slaats 2022 *Cancer Immunol Res* — adenosine *weakens* pairing and thereby dampens serial hit delivery; stabilising conjugation **accelerated** lethal-hit delivery [DOI](https://doi.org/10.1158/2326-6066.CIR-22-0113). There is an optimum, which is what makes it a decision. |
| **Tissue-resident memory (CD103)** | Some of your cytotoxic cells stay in the gut wall between rounds instead of being replaced. | **NEW FIELD** `UnitLifecycleTuning.ResidentFractionAcrossRounds` (0 → **+0.25/lvl**) — the honest home for the ~30% knowledge rung | 45 / 2 | Casey 2012 *J Immunol* — intestinal-epithelium CD8 TRM are maintained by **TGF-β-driven CD103**, cytokine-driven and **independent of continued cognate antigen** [DOI](https://doi.org/10.4049/jimmunol.1200402); Grayson 2002 *J Immunol* — memory CD8 are 8× more apoptosis-resistant and contract 3–5× vs 10–20× [DOI](https://doi.org/10.4049/jimmunol.169.7.3760) |

**Deliberately not proposed for the CTL, with reasons (see §3):** a kill-limit
row (the CTL's `KillLimit` of 16 is already at the top of the measured in-vivo
range), and an exhaustion-resistance row (exhaustion requires chronic antigen
and does not form in acute infection).

### 1d. B cell → plasma cells — 4 rows, all new

| Row | Player-facing effect | Field written, per level | Price / cap | Citation |
|---|---|---|---|---|
| **Germinal-centre output (IL-21 / APRIL)** | The tower keeps more plasma cells standing — more antibody in the air. | per-tower `MaxActiveChildren` 2 → **+1/lvl** (binds exactly; see §5) | 40 / 2 | Zhang 2018 *J Exp Med* — plasmablast output from the GC is set by Tfh-derived **IL-21** plus stromal **APRIL**, and blocking the APRIL receptor TACI cuts plasmablast numbers [DOI](https://doi.org/10.1084/jem.20160832) |
| **Antibody-secreting-cell maturation** | Each plasma cell pumps out antibody faster. | `UnitLifecycleTuning.AntibodySecretionIntervalSeconds` 1.1 → **×0.8/lvl**, floor 0.5 | 35 / 3 | Nguyen 2025 *Front Immunol* — **mature** ASCs secrete more IgG **per cell** than early-minted blood ASCs, and the same cells raise their rate over 7 days in culture (DOI **UNVERIFIED**; Front Immunol 2025, "Maturation of human early-minted blood antibody-secreting cells is coupled with increased IgG secretion rates"); magnitude: a plasma cell secretes thousands–~10,000 Ig/s (Lam 2017 *Trends Immunol*; Sievert 2025 *Cell Rep* — DOIs **UNVERIFIED**) |
| **Polymeric IgA assembly (J chain)** | Antibodies go out as dimers — a coated organism is pacified much harder. | **NEW FIELD** `AntibodyTuning`-per-tower `DimericAntibody` (bool) or `CoatingPotencyMultiplier` 1 → **+0.5/lvl**; cleanest implementation is to let this tower's coat qualify for the §6b **cross-link** state in *tissue*, reusing the IgA raft code | 45 / 2 | Wang 2020 *Sci Transl Med* — IgA **dimers are on average 15× more potent** than the same monomer [DOI](https://doi.org/10.1126/scitranslmed.abf1555); Sun 2021 *PNAS* — IgA1 dimers up to **240×** over monomer (DOI **UNVERIFIED**) |
| **Somatic hypermutation** | Your antibodies start sticking to *related* species, not just the one they were raised against. | **NEW MECHANIC** — this tower's antibodies also coat species whose 8-bit tag is within Hamming 1 of a licensed species (`Antibody.TryCoatHere` already has the species in hand) | 45 / 1 | Kabbert 2020 *J Exp Med* — human gut IgA binds **phylogenetically unrelated** bacterial species, and this cross-species reactivity "did not correlate with antibody polyreactivity but was **crucially dependent on the accumulation of somatic mutations**" [DOI](https://doi.org/10.1084/jem.20200275). Counterpoint recorded: Bunker 2017 *Science* found homeostatic mouse IgA polyreactive with SHM making little difference (DOI **UNVERIFIED**) — the difference is antigen-driven vs homeostatic, and the game's case is antigen-driven. |

**Deliberately not proposed for the B cell:** a class-switching row (the gut's
terminal isotype *is* IgA and secretory IgA is already the ~70% knowledge rung —
a purchasable switch row would double-book it), a plasma-cell-longevity row
(nothing to write; see §5), and any "affinity maturation ×N damage" row (the
affinity ceiling makes a large multiplier unsupportable; §3).

---

## 2. Evidence and trade-off, row by row

### Dendritic cell

**pMHC-II stabilisation** (renamed from Macropinocytosis rate).
Cella 1997 measured the thing the field actually represents: in an immature DC,
class II turns over with a **half-life of ~10 h**; inflammatory maturation
boosts class-II synthesis transiently *and* pushes the half-life to **over
100 h**, producing "a large number of long-lived peptide-loaded MHC class II
molecules capable of stimulating T cells even after several days"
([DOI](https://doi.org/10.1038/42030)). Lazarski 2005 then showed the half-life
of class-II:peptide is *the* parameter that decides which epitopes dominate —
immunodominant peptides t½ >150 h, cryptic ones <10 h — with a demonstrated
causal link to immunogenicity ([DOI](https://doi.org/10.1016/j.immuni.2005.05.009)).
That is a 10× dynamic range in exactly the quantity `DcPresentationsPerCargo`
stands for, which makes 4 → 10 across three levels proportionate.

*Why it is a decision:* it buys **throughput per trip**, not trips. It pays off
only if the DC can actually reach helper-T cells — i.e. it compounds with CCR7
and with a crowded node, and it is nearly worthless on a board where the node
is empty. It is the "fewer, better trips" half of a pair whose other half
(CCR7) is "more trips."

**CCR7 expression** (kept).
Worth stating that this row survives scrutiny, because the obvious objection —
that CCR7 is a *homing* receptor, not a throttle — is answered directly in the
literature. Riol-Blanco 2005 showed CCR7 runs two **independent** signalling
modules in DCs: a Gi/MAPK module that controls chemotaxis and a Rho/Pyk2/cofilin
module that controls **migratory speed**, and interfering with the latter slows
DCs *even without chemokine present*
([DOI](https://doi.org/10.4049/jimmunol.174.7.4070)). So "CCR7 expression →
`DcFineTilesPerTick`" is the correct field for the correct reason. Braun 2011
*Nat Immunol* adds that CCR7 is absolutely required for directional migration
into the T-cell zone (DOI **UNVERIFIED**).

*Why it is a decision:* speed is the DC's only lever on **travel cost**, and
§5a makes travel time the entire lymph-node delay. It is worth most when the
tissue band is wide or debris is far from the base, and worth least when the DC
is already finding cargo within a tick or two.

**Limited lysosomal proteolysis** (renamed from Antigen-sparing sampling).
Delamarre 2005 is the paper the game's debris-competition mechanic has been
waiting for: APCs split into two strategies — macrophages are **protease-rich**
and "rapidly degraded internalized proteins," whereas DCs (and B cells) are
**protease-poor**, degrade internalised antigen slowly, and consequently
"retained antigen in lymphoid organs for extended periods"; limited proteolysis
"also favored antigen presentation"
([DOI](https://doi.org/10.1126/science.1108003)). The game already models a DC
and a macrophage fighting over the same debris pile; this makes the asymmetry
sourced rather than invented — the macrophage *burns* what the DC *preserves*.

*Why it is a decision:* it is the only DC row that pays the **macrophage**
rather than the DC, so it is worth buying on a board where debris clearance and
tissue recovery are the bottleneck, and worth nothing on a board drowning in
debris. "Antigen-sparing" is also a vaccine dose-sparing term and does not mean
this.

### Helper T

**Clonal expansion** (kept).
The term is used correctly — clonal expansion is exactly what this is. Two
numbers make the rate component (not just the cap) the honest lever: CD8 T cells
complete division cycles in **under 6 h** and can start as fast as **2 h**, and
critically Yoon 2010 found the initial cycle time "is not fixed but is
controlled by the antigenic stimulus provided by the APC in vivo," which "influences
the rate of T cell expansion and the numbers of effector T cells"
([DOI](https://doi.org/10.1371/journal.pone.0015423)). So a DC-quality upgrade
raising a T-cell proliferation rate is biologically the right coupling, and
`ClonalRatePerLevel` is the right shape. Also: the cap now **binds** (§5), so
after Sprint 25 this row genuinely buys both halves of what it claims.

*Why it is a decision:* this is the "more meetings" row. It competes directly
with Functional avidity maturation, which is the "better meetings" row — more
pairings at 14.5% each versus the same pairings at 36%. Which is better depends
on whether the node is DC-starved or T-starved, and the player can see which.

**Functional avidity maturation** (renamed; cap cut 2 → 1).
Slifka & Whitton 2001 settles both the naming and the magnitude. Their opening
sentence is the correction: "Unlike B cells, T cells lack the capacity to improve
the affinity of their antigen receptors by somatic mutation." And their result is
the mechanic: responsiveness to peptide — which they name **functional avidity** —
rose **>50-fold** during early viral infection, "without any appreciable change
in T cell receptor affinity" ([DOI](https://doi.org/10.1038/90650)). Mason 1998
supplies the degeneracy that makes a Hamming radius the right metric at all: one
TCR engages on the order of **10⁶** different peptide-MHC complexes
([DOI](https://doi.org/10.1016/s0167-5699(98)01299-7)).

The cap must come down, and that is a numbers finding rather than a naming one
(§5): at r≤4, **100% of the 256 possible tags are within range of every one of
the six species currently in the roster**, so the barcode stops existing. At
r≤3 the per-pairing teach rate is 36.3% (2.5× the default); at r≤4 it is 63.7%
(4.4×). Cap the row at one level and lower
`AdaptiveUnitTuning.MatchDistanceCeiling` from 4 to 3.

*Why it is a decision:* bought at 45 ATP it converts a specificity problem into
a throughput problem — and in doing so it dilutes the thing the barcode exists
to teach. That is a legitimate player trade (knowledge now vs specificity as a
felt mechanic) and it is the one row where the design should *want* the player
to feel slightly guilty.

**Lymph-node egress** (re-specced from Rapid recirculation).
The question asked was whether lifespan-for-refresh is a real trade-off. Verdict:
**the trade is real, but not in the direction the row implements, and the row's
current blurb describes a pure win that the code does not deliver.**

- Repertoire refresh in a real node happens by **recirculation**, not death.
  Naive T cells enter from blood, transit, and leave alive: modal LN transit
  **10–20 h**, distribution strongly right-skewed with a substantial tail past
  70 h (Thomas 2012, [DOI](https://doi.org/10.1371/journal.pone.0045262)).
- Naive T cells are the **long-lived** compartment, not a fast-turnover one:
  deuterium labelling puts the naive CD4 intermitotic time at **≈1 year**
  (0.2%/day), versus 15 d for effector-memory (Macallan 2004,
  [DOI](https://doi.org/10.1084/jem.20040341)). So "shorter lifespan" is the
  wrong physical quantity; "shorter dwell time before egress" is the right one,
  and S1PR1/LFA-1 are the receptors that set it.
- The cost is measured. Reichardt 2013 compared T cells that egress promptly
  with ones that linger: LFA-1-deficient cells "egressed promptly," had
  significantly decreased dwell times, and returned to the LN parenchyma less
  often, and the authors conclude that returning "amplifies the number of T
  cells returning to the LN parenchyma that **can lead to increased
  effectiveness of T-cell response to antigen**"
  ([DOI](https://doi.org/10.1038/emboj.2013.33)).

So keep the field and the step, rename the row, and make the blurb name the
cost, which the arithmetic in §5 shows is real and already in the build: level 1
is free, levels 2–3 shrink the node's standing population by 14% and 43%.

*Why it is a decision:* it is the only row in the game with a built-in,
quantified downside, and it has an emergent synergy worth protecting — buying
Clonal expansion alongside it shortens the emission interval enough to cancel
the population loss (§5). That is a build, not a stat.

### Cytotoxic T cell

**Granzyme B loading.**
Two independent handles. Liadi 2015 compared CD4 and CD8 CAR T cells
cell-by-cell in nanowells and attributed the CD4s' slower killing and slower
multi-killing to "the lower granzyme B content"
([DOI](https://doi.org/10.1158/2326-6066.CIR-14-0195)) — i.e. granule content
maps onto rate, which is what `StressSenseChancePerTick` is. Weigelin 2021
makes the damage explicitly graded and perforin-dependent and closes with
"accelerating damage **magnitude** and frequency may improve immune efficacy"
([DOI](https://doi.org/10.1038/s41467-021-25282-3)).

The cap is set by the game, not the biology, and that is the interesting part:
`GAME_DESIGN.md` §4c fixes innate recognition at 0.003 and bans a gap wider
than ~20×, so this row can only run to 0.05 before it breaks the stated
invariant. Cap 2. Write that reason into the row's comment, because it is
exactly the kind of ceiling a later tuning pass would raise without knowing.

*Why it is a decision:* it makes a lone CTL competent, which is the opposite of
what the cooperation table rewards. A player who buys granzyme loading is
choosing to spread CTLs thin; a player who leans on `CooperationMultiplier` is
choosing to concentrate them. Both are viable and they want different maps.

**Interstitial patrol.**
Weigelin 2021 is unusually direct about *motility* being part of cytotoxicity
rather than merely transport: "sublethal damage events, delivered by multiple
CTL **transiting between** individual tumor cells," and in vivo "multi-hit
delivery is most effective in interstitial tissue where high CTL densities and
**swarming** support frequent serial CTL–tumor cell encounters"
([DOI](https://doi.org/10.1038/s41467-021-25282-3)). The target number is
available: effector CD8 in mouse small-intestinal villi move at 6.5 µm/min,
~6× the tissue macrophage (Torres 2023, [DOI](https://doi.org/10.7554/eLife.84916)),
and the CTL sits at 3 against a macrophage's 1, so 3 → 5 moves toward the
measured ratio instead of away from it.

*Why it is a decision:* the same paper gives the reason this is not simply
"more": 3 hits must land **within 50 min decay intervals** or the target
recovers. A fast CTL spreads its hits over more targets and may land none of
them lethally; a slow one concentrates. Raising speed is a bet that your board
has clustered infection, which the cytokine field makes legible.

**Immune-synapse release (NEW FIELD).**
This is the best-evidenced axis of the five the brief listed, and it is
genuinely two-sided. Sanchez 2022 found the synapse dissolves because the
*apoptotic target* contracts, that this contraction is "necessary and
sufficient" for the CTL to let go, and that mechanically amplifying it "promoted
faster CTL detachment and **serial killing**"
([DOI](https://doi.org/10.1038/s41590-023-01572-4)). Rothstein 1978 saw the
same thing by time-lapse decades earlier: a CTL "usually separated from the
target cell **before** target cell death," not all contacted targets were
killed, and contact duration did not predict death (DOI **UNVERIFIED**).
Pulling the other way, Slaats 2022 showed adenosine *weakens* one-to-one
pairing and thereby "dampen[s] serial cytotoxic hit delivery," while blocking
it "stabilized and prolonged CTL–target cell conjugation and **accelerated**
lethal hit delivery" ([DOI](https://doi.org/10.1158/2326-6066.CIR-22-0113)).

*Why it is a decision:* detaching too fast wastes partial pressure on a cell
that then recovers; detaching too slowly wastes the cell. There is an optimum
and the literature brackets it from both sides, so this row should be tuned to
sit slightly past the optimum at max level — a cited reason for a row that can
be over-bought.

**Tissue-resident memory, CD103 (NEW FIELD).**
Casey 2012 is the gut-specific result: resident-memory signatures CD69 and
CD103 are induced by **cytokines**, prolonged cognate antigen is "dispensable
for intestinal TRM ontogeny," chronic antigen actually "skewed differentiation
**away** from the canonical intestinal T cell phenotype," and TGF-β-driven CD103
is *required* for maintenance within the intestinal epithelium
([DOI](https://doi.org/10.4049/jimmunol.1200402)). Grayson 2002 supplies the
survival numbers: memory CD8 are **8× less** depleted than naive by an apoptotic
insult, and secondary effectors contract only 3–5× against the primary
response's 10–20× ([DOI](https://doi.org/10.4049/jimmunol.169.7.3760)).

*Why it is a decision:* every other row buys performance **inside** a round;
this one buys state **across** rounds, at a price that does nothing for the
round you buy it in. It is also the mechanically honest home for the ~30%
knowledge rung (§4, memory), so building it once serves two specs.

### B cell → plasma cells

**Germinal-centre output (IL-21 / APRIL).**
Zhang 2018 located plasmablast emergence at the GC–T-zone interface and defined
the two signals that set its rate: Tfh-derived **IL-21** and **APRIL** from
podoplanin⁺CD157^hi fibroblastic reticular cells, with TACI blockade cutting
plasmablast numbers specifically ([DOI](https://doi.org/10.1084/jem.20160832)).
Chu 2014 adds the gut-specific survival layer — eosinophil-deficient mice lose
IgA⁺ plasma cells and secreted IgA, and TGF-β-dependent class switching in
Peyer's patches is disturbed ([DOI](https://doi.org/10.1016/j.immuni.2014.02.014)).
Honest caveat: FitzPatrick 2020 (conference abstract, littermate controls) did
**not** reproduce the eosinophil requirement for lamina-propria IgA plasma cells
(**UNVERIFIED** / contested), so cite Chu for the mechanism and not as settled.

*Why it is a decision:* plasma cells are sessile, so each extra one is a
**placement** decision, not a damage number — a third factory only helps if you
put it where antibody will drift into traffic. And since the plasma-cell cap
binds exactly (§5), this row is the one place in the adaptive set where a cap
raise is guaranteed not to be inert.

**Antibody-secreting-cell maturation.**
The absolute rate is unusable in game — a plasma cell exports on the order of
**thousands** of Ig molecules per second (Lam 2017, *Trends Immunol*,
**UNVERIFIED** DOI), with ~**10,000/s** quoted for a fully differentiated PC
(Sievert 2025, *Cell Rep*, **UNVERIFIED** DOI). What *is* usable is that the
rate is not fixed per cell: Nguyen 2025 measured IgG secretion per cell and
found mature bone-marrow ASCs out-secrete early-minted blood ASCs, and that the
same cells raise their own rate as they mature in culture (**UNVERIFIED** DOI).
So "a plasma cell can be made a better factory" is a real, measured axis rather
than a convenience.

*Why it is a decision:* antibodies have a 4.5 s life and 0.55 world-units/s
drift, so raising the secretion rate increases **local** density around a fixed
point. It is the row for a player who has already placed plasma cells well, and
it is strictly worse than Germinal-centre output for a player who has not.

**Polymeric IgA assembly, J chain (NEW FIELD).**
This is the row to build first, because it is the one place where the honest
number is also the number the game wants. Wang 2020 characterised 149
convalescent donors and found IgA monomers *twofold weaker* than matched IgG,
while **IgA dimers were on average 15× more potent than the same monomer**
([DOI](https://doi.org/10.1126/scitranslmed.abf1555)). Sun 2021 reproduced the
effect with plant-expressed IgA1 and measured up to **240×**, concluding that
"both IgA's structural features and multivalency positively impact NT potency"
(**UNVERIFIED** DOI). Note what the gain is *not*: it is **valency**, not
affinity — the same binding site, assembled in twos.

*Why it is a decision:* it changes the *kind* of effect rather than the amount,
and the cleanest implementation reuses the Sprint 22 cross-link raft in tissue
instead of the lumen — so the player who buys it learns that tissue antibody and
luminal IgA are one molecule, which is a design goal §4c already states.

**Somatic hypermutation (NEW MECHANIC).**
Affinity maturation is real here — this is the one tower where the B-cell
germinal-centre term applies — but the interesting gut finding is not potency,
it is **breadth**. Kabbert 2020 cloned monoclonals from healthy and inflamed
human intestine and found individual antibodies binding **phylogenetically
unrelated** bacterial species; that cross-species reactivity "did not correlate
with antibody polyreactivity but was crucially dependent on the accumulation of
**somatic mutations**," i.e. SHM buys cross-species coverage
([DOI](https://doi.org/10.1084/jem.20200275)). Mapped onto the game's 8-bit tags
that is: antibodies from this tower coat species within Hamming 1 of the
licensed one.

*Why it is a decision:* it is the only B-cell row that is not a number, and it
makes the barcode do work on the antibody side of the loop for the first time.
It is also the row that answers a mixed round, which is precisely where a
potency row does nothing.

---

## 3. Verdicts on the specific questions asked

### Is "TCR affinity maturation" a misnomer? — **Yes, plainly.**

Affinity maturation is a B-cell germinal-centre process driven by somatic
hypermutation and affinity-based selection. T cells do not somatically
hypermutate their receptors; Slifka & Whitton state it as the premise of their
paper ([DOI](https://doi.org/10.1038/90650)). Using the phrase for a T-cell row
teaches the player something false about the one distinction between the two
lymphocyte lineages that the game's own B-cell tower depends on.

**The correct name is "Functional avidity maturation."** It is not a coined
compromise — it is the published term for this exact phenomenon in T cells,
coined in the title of the paper that measured it, and the effect is large
(>50-fold) and occurs *in vivo during a response*, which is what a purchasable
upgrade should represent. Acceptable alternatives, in order:

1. **Functional avidity maturation** — recommended; real term, real magnitude,
   and it is literally "the same ligand now triggers a response it did not
   trigger before," which is what raising the Hamming radius does.
2. **TCR cross-reactivity** (or *degenerate recognition*) — also correct, and
   Mason 1998 is the canonical source
   ([DOI](https://doi.org/10.1016/s0167-5699(98)01299-7)). Slightly worse as an
   upgrade name because cross-reactivity is a constitutive property of a TCR
   rather than something a response acquires.
3. **Avidity maturation** — fine shorthand.

The single word to delete is **affinity**. Keep "maturation."

### "Rapid recirculation" — is the lifespan/refresh trade-off real?

**The trade-off is real; the implementation and the framing are both slightly
wrong, and the row currently advertises a pure win.** Full argument and numbers
in §1b and §2; the three actual figures are:

- **Modal LN transit time 10–20 h**, strongly right-skewed, with a significant
  fraction of cells >70 h (sheep cannulation + model fitting; Thomas 2012,
  [DOI](https://doi.org/10.1371/journal.pone.0045262)).
- **CD4 traverse a node roughly twice as fast as CD8**, home more efficiently,
  and spend ~⅓ of transit in contact with MHC-II on DCs; CD8 transit is
  unaffected by the absence of MHC-I, so the two lineages use asymmetric
  surveillance strategies (Mandl 2012 PNAS,
  [DOI](https://doi.org/10.1073/pnas.1211717109)). The game's node is a
  helper-T node, so the CD4 figures are the relevant ones.
- **Repertoire scanning is a DC property, not a lifespan property.** One DC
  scans **≥500** T cells/hour with no antigen present (Bousso 2003 *Nat
  Immunol*, DOI **UNVERIFIED**) and up to **5,000/hour** with ~3-minute contacts
  (Miller 2004 PNAS, [DOI](https://doi.org/10.1073/pnas.0306407101)).
- **Naive T cells do not turn over fast.** Naive CD4 label at 0.2%/day,
  intermitotic time ≈1 year (Macallan 2004,
  [DOI](https://doi.org/10.1084/jem.20040341)).

So: cells leave the node **alive**, the node's repertoire refreshes by
recirculation, and reducing dwell time measurably **reduces** priming efficiency
(Reichardt 2013, [DOI](https://doi.org/10.1038/emboj.2013.33)). Rename to
**Lymph-node egress** (S1PR1/LFA-1 are the honest molecular handles), keep the
field, and put the cost in the blurb — the cost is already in the build (§5).

One incidental correction to `GAME_DESIGN.md` §5c while we are here: the
co-localisation cytokine that pulls DCs and helper-T cells together is
contradicted at the cellular scale. Miller 2004 found "T cells did not
accumulate around DCs, and their relative velocities approaching and departing
DCs were equivalent, implying that T cells are **not** attracted toward DCs by
chemotactic gradients but rather encounter them **by chance**"
([DOI](https://doi.org/10.1073/pnas.0306407101)). The field should stay — it is
a legibility device and §5c already argues for it on those grounds — but the
doc should not claim it mirrors a real gradient. The real mechanism is a DC
with agile dendrites scanning thousands of randomly walking T cells per hour.

### Cytotoxic T cell axes — which are real and quantified?

| Axis | Real? | Quantified? | Verdict |
|---|---|---|---|
| **Serial killing rate** | Yes | Yes — 2–16 targets/day in vivo (Halle 2016, [DOI](https://doi.org/10.1016/j.immuni.2016.01.010)); serial killing visible by time-lapse since 1978 | **Already at ceiling.** The CTL's `KillLimit` is 16, the top of the measured in-vivo daily range. **Do not add a kill-limit row** — this is the one number in the roster that is already at the honest maximum, and raising it is the first thing a tuning pass would do. |
| **Granzyme / perforin loading** | Yes | Partially — granzyme B content tracks killing rate across cell types (Liadi 2015, [DOI](https://doi.org/10.1158/2326-6066.CIR-14-0195)); hit magnitude graded and perforin-dependent (Weigelin 2021, [DOI](https://doi.org/10.1038/s41467-021-25282-3)). A per-molecule dose–response is **UNVERIFIED** | **Row 1.** Existing field, clean mapping, and the Director's 20× ceiling caps it for a stated reason. |
| **Detachment time after a kill** | Yes | Yes, and from both directions — faster detachment raises serial killing (Sanchez 2022, [DOI](https://doi.org/10.1038/s41590-023-01572-4)); prolonging conjugation accelerates lethal-hit delivery (Slaats 2022, [DOI](https://doi.org/10.1158/2326-6066.CIR-22-0113)) | **Row 3**, and the best *designed* row of the four because the literature brackets an optimum. Needs one new field. |
| **Resistance to exhaustion** | Yes, but out of scope | Yes — and the quantification is what rules it out. Exhaustion is "induced by **high** antigen stimulation" and requires chronic infection (Alfei 2019, [DOI](https://doi.org/10.1038/s41586-019-1326-9)); in *T. cruzi*, exhausted PD-1⁺TOX⁺ CD8s "arise **only** in long-term" infection and blocking co-inhibitory receptors during acute infection had "only a marginal effect" (Gálvez 2022, DOI **UNVERIFIED**) | **Reject.** A ~30 s round is an acute encounter. An anti-exhaustion row would be a purchase against a state the game does not have, and the fix if the game ever gets persistent cross-round infection is to add exhaustion *first*. |
| **Memory differentiation (effector vs central memory)** | Yes | Yes for the gut-resident variant specifically — TGF-β/CD103, cytokine-driven, antigen-independent (Casey 2012, [DOI](https://doi.org/10.4049/jimmunol.1200402)); survival and contraction numbers from Grayson 2002 ([DOI](https://doi.org/10.4049/jimmunol.169.7.3760)) | **Row 4** — but as **tissue-resident** memory (TRM), not Tcm/Tem. The small-intestinal epithelium is the TRM compartment, and that is the map. |

Best three, if only three are wanted: **Granzyme B loading**, **Immune-synapse
release**, **Tissue-resident memory (CD103)** — one existing field, two new, and
the three do not overlap. Interstitial patrol is the fourth because it uses an
existing field and moves a number toward its measured ratio, but it is the one
to cut.

### B cell / plasma cell axes — and the affinity-maturation ceiling

**The "10–15×" figure in the brief: partly confirmed, and the honest statement
is different and better.**

- For **single haptenic epitopes**, affinity rises roughly **100-fold** over
  months: serum antibodies bind ε-DNP-lysine at Ka 10⁵–10⁶ M⁻¹ at 1–2 weeks and
  10⁷–10⁸ M⁻¹ after a few months (Foote & Eisen 1995 PNAS, reviewing Eisen &
  Siskind, [DOI](https://doi.org/10.1073/pnas.92.5.1254)).
- For **whole organisms**, the gain is much smaller or absent. Roost 1995
  panelled neutralising monoclonals against vesicular stomatitis virus taken on
  days 6, 12 and 150: they already bound at **Ka 10⁸–10¹⁰ M⁻¹ with rapid on-rates
  on day 6 of a primary response**, "with **no evidence for further** antigen
  dose- and time-dependent overall improvement of affinity," and the authors say
  this is "probably representative for viruses or bacterial toxins which are
  crucially controlled by neutralizing antibodies"
  ([DOI](https://doi.org/10.1073/pnas.92.5.1257)).
- There is a hard **affinity ceiling** around 10¹⁰ M⁻¹, set by the diffusion
  limit on the on-rate against the off-rate at which a B cell internalises
  antigen (Foote & Eisen 1995, [DOI](https://doi.org/10.1073/pnas.92.5.1254));
  modern work relocates the cause to the mechanical strength of the
  antigen-acquisition tether but keeps the conclusion (Desikan 2021 *BioEssays*,
  DOI **UNVERIFIED**).

**So: "around 10–15×" is the right order of magnitude for a whole-organism
response, but it is not a number anyone measured as a maturation gain** — mark
the 10–15× as **UNVERIFIED as stated**. The defensible version is: *single
epitopes, up to ~100×; whole virus, essentially no further improvement after
the first week; hard ceiling ~10¹⁰ M⁻¹ regardless.* A "×1000 antibody damage"
row is unsupportable, and even ×10 is at the generous end.

**And the figure that *is* 15× is a different quantity entirely: valency.** IgA
dimers are 15× more potent than the matched monomer (Wang 2020,
[DOI](https://doi.org/10.1126/scitranslmed.abf1555)), up to 240× in a
plant-expressed system (Sun 2021, **UNVERIFIED** DOI). That is the row to build:
the honest big number in the B-cell tower comes from **assembling two of the same
antibody**, not from improving one.

Remaining axes:

- **Class switching (IgM→IgG→IgA).** Real, and gut-specific: TGF-β is required
  for both T-dependent and T-independent IgA switching, with retinoic acid
  acting as a more IgA-selective switch factor than TGF-β itself (Seo 2013
  *J Leukoc Biol*, **UNVERIFIED** DOI; Chu 2014 for the Peyer's-patch
  dependency, [DOI](https://doi.org/10.1016/j.immuni.2014.02.014)). **But do not
  make it a row**: in this map the terminal isotype is IgA, and secretory IgA is
  already the ~70% knowledge rung. A purchasable class-switch row would sell the
  player something the ladder already grants, and IgG in the gut is the
  *inflamed*-state isotype (Kabbert 2020,
  [DOI](https://doi.org/10.1084/jem.20200275)) — a drawback, not an upgrade.
- **Plasma-cell longevity.** Spectacularly real — carbon-14 birth dating puts
  human small-intestinal CD45⁻ plasma-cell subsets at median **11 and 22 years**,
  with rotavirus-specific clones maintained for life (Landsverk 2017,
  [DOI](https://doi.org/10.1084/jem.20161590)). **But there is nothing to
  write** (§5): plasma cells have no lifespan field and currently persist until
  the round boundary, so a longevity row would be the Sprint-18 helper-T cap
  failure repeated exactly. If it is wanted, it needs the same new field as the
  CTL's CD103 row and should be built once for both.
- **Antibody secretion rate.** Real, per-cell variable, and upgradeable
  (Nguyen 2025, **UNVERIFIED** DOI); magnitude thousands–10,000 Ig/s. Uses an
  existing per-tower field. **Row 2.**
- **Germinal-centre output.** Real and mechanistically dissected (Zhang 2018,
  [DOI](https://doi.org/10.1084/jem.20160832)). **Row 1**, and the only cap in
  the adaptive set guaranteed to bind.

Context for scale, worth keeping: the gut holds the body's largest plasma-cell
population — on the order of **10¹⁰ plasma cells per metre** of intestine,
exporting **3–5 g of IgA per day** (Pabst 2012 *Nat Rev Immunol*; Spencer 2007,
both DOIs **UNVERIFIED**). The B-cell tower is modelling a factory floor, and
two sessile cells is already a very deep compression.

### The ~30% memory-T rung: "a burst of CTLs on re-encounter"

**Confirmed backwards, with three results.**

1. **Memory does not out-expand naive, and peak magnitude is roughly invariant.**
   Badovinac 2003 transferred trackable memory CD8s and found that "similar
   numbers of Ag-specific CD8⁺ T cells were found 8 days postinfection **no
   matter how many memory cells were present** at the time of infection," and
   that memory presence *inversely* correlated with the magnitude of the naive
   response to the same epitope — memory crowds naive out rather than adding to
   it ([DOI](https://doi.org/10.4049/jimmunol.170.10.4933)).
2. **Memory is quick to respond but not quick to divide.** Whitmire 2008
   measured onset directly: memory cells detect and respond to virus within
   hours (IFN-γ), yet "both memory and naive T cells show an extended, and
   **indistinguishable**, delay in the onset of proliferation" — division did not
   begin until ~day 3 for either ([DOI](https://doi.org/10.1371/journal.ppat.1000041)).
3. **Memory wins on survival and contraction.** Grayson 2002: memory CD8s are
   8× more resistant to an apoptotic insult than naive, and secondary effectors
   contract only **3–5×** versus the primary response's **10–20×**
   ([DOI](https://doi.org/10.4049/jimmunol.169.7.3760)). Badovinac independently
   found secondary contraction "markedly prolonged" relative to primary
   ([DOI](https://doi.org/10.4049/jimmunol.170.10.4933)).

**What the rung should actually do.** Three effects, in priority order, none of
which is a burst:

- **Zero lead time.** On re-encountering a known species, CTLs act *immediately*
  — no priming delay, no re-earning of the species licence, and (if
  `KnowledgeLicence.PerSpeciesRecognition` is on) instant per-species
  recognition. This is the effect the literature supports most strongly and the
  one a player will actually perceive.
- **Precursors already in place.** A standing population of resident CTLs
  survives into the next round rather than being re-emitted from the marrow —
  the same `ResidentFractionAcrossRounds` field the CD103 row wants. "Memory
  wins on precursor numbers" means *present at t=0*, not *multiplying faster*.
- **Gentler contraction.** Whatever CTL population exists when the round ends
  decays less than an unmemorised one. This is the literal Grayson/Badovinac
  result and it is cheap to implement on top of the field above.

If the Director wants a visible event at the rung — and §5's whole argument is
that mechanics must be perceptible — the honest visible version is **CTLs
appearing on the board the instant a known species is detected, with no marrow
round-trip**, which reads as a burst to the player while being the correct
biology. What should *not* happen is extra cells or a higher per-cell expansion
rate. Narrating it as "memory cells multiply faster" would be the NK-speed
error again: a framing the data contradicts, written into the HUD.

---

## 4. Naming corrections

Every existing row whose name misuses a term, with the replacement.

| Tower | Current name | Problem | Recommended name |
|---|---|---|---|
| Helper T | **TCR affinity maturation** | **Wrong.** Affinity maturation is a B-cell germinal-centre process requiring somatic hypermutation; T cells cannot do it (Slifka & Whitton 2001, [DOI](https://doi.org/10.1038/90650)). The row teaches the player the opposite of the one lineage distinction the B-cell tower rests on. | **Functional avidity maturation** (alt: TCR cross-reactivity) |
| Helper T | **Rapid recirculation** | **Half wrong.** Recirculation is the right process, but it refreshes a node by cells *leaving alive*, not by dying — naive T cells are the long-lived compartment (≈1 yr intermitotic time; Macallan 2004, [DOI](https://doi.org/10.1084/jem.20040341)) — and faster egress measurably *reduces* priming (Reichardt 2013, [DOI](https://doi.org/10.1038/emboj.2013.33)), which the current blurb presents as a pure win. | **Lymph-node egress** — and the blurb must name the population cost |
| Dendritic cell | **Macropinocytosis rate** | **Wrong row.** Macropinocytosis is fluid-phase *uptake*, and it is **downregulated irreversibly** when a DC matures — i.e. exactly when it stops sampling and leaves for the node (Sallusto 1995, [DOI](https://doi.org/10.1084/jem.182.2.389)). It cannot be the thing that governs presentations *at* the node. It also contradicts the DC's third row, which *reduces* uptake as a benefit: as written, two of three DC rows push the same biological quantity in opposite directions. | **pMHC-II stabilisation** (alt: DC maturation) — Cella 1997, [DOI](https://doi.org/10.1038/42030) |
| Dendritic cell | **Antigen-sparing sampling** | **Not an immunological term for this.** "Antigen-sparing" is vaccine dose-sparing (less antigen per dose via adjuvant), not a cell taking a smaller bite. The real property is that DCs are protease-poor and preserve what they take while macrophages destroy it (Delamarre 2005, [DOI](https://doi.org/10.1126/science.1108003)). | **Limited lysosomal proteolysis** |
| Dendritic cell | CCR7 expression | **Correct** — and specifically defensible against the objection that CCR7 is only a homing receptor: it runs a separate Rho/Pyk2/cofilin module that sets migratory speed independently of chemotaxis (Riol-Blanco 2005, [DOI](https://doi.org/10.4049/jimmunol.174.7.4070)). No change. |
| Helper T | Clonal expansion | **Correct.** No change. |
| Macrophage / neutrophil (outside this brief, flagged in passing) | "Tissue residency (M2)", "Extended lifespan (GM-CSF priming)", "Controlled degranulation", "Rapid chemokinesis", "Efferocytic capacity", "Pseudopod reach" | Not reviewed here — innate towers are another agent's domain. On a quick read, **"Tissue residency (M2)"** is the one worth a second look: M2 is an *activation state*, not a residency programme, and the row writes a kill limit. | — |

Two further doc-level naming notes:

- `AdaptiveTuning.cs` and `GAME_DESIGN.md` §5c both describe the node's
  co-localisation cytokine as mirroring real DC:T-cell attraction. It does not —
  T cells find DCs **by chance** (Miller 2004,
  [DOI](https://doi.org/10.1073/pnas.0306407101)). Keep the field, fix the
  sentence.
- `ProgenitorUpgradeCatalog.PickerBlurb` still says the CTL is "precise kill ·
  **the cell survives**", which §4c corrected on 2026-10-25 — the host cell
  always dies, and the reward is apoptotic rather than necrotic death. That
  blurb is the last place in the code still telling the player the retracted
  version. Suggested replacement: "precise kill · clean death, tissue recovers".

---

## 5. Do the caps bind? — arithmetic against the current defaults

The brief's rule, applied. All numbers from `AdaptiveTuning.cs`,
`GameBootstrap.cs` and `ProgenitorUpgrades.cs` as committed at `fac9b61`.

**Helper T — Clonal expansion: the cap binds at every level. ✅**
Steady state is `min(cap, lifespan / interval)`. Default: `min(4, 20/3.5)` =
`min(4, 5.71)` = **4**, so the cap binds. At level L the cap is 4+2L and the
interval is 3.5/(1+0.5L), so the lifespan-limited ceiling is 5.71·(1+0.5L):
L=1 → min(6, 8.6) = 6; L=2 → min(8, 11.4) = 8; L=3 → min(10, 14.3) = 10. The
cap binds throughout and the rate component is what makes it reachable. The
Sprint 25 note is correct and the fix worked.

**Helper T — Lymph-node egress: there is already a real, unadvertised cost. ⚠️**
Population = `min(4, (20−4L)/3.5)`: L=0 → 4; **L=1 → min(4, 4.57) = 4
(unchanged)**; **L=2 → 3.43 (−14%)**; **L=3 → 2.29 (−43%)**. The crossover is at
lifespan 14 s, i.e. between level 1 and level 2. So level 1 is a free repertoire
refresh and levels 2–3 trade standing population for it. That is a good
trade-off and it is *accidental* — nothing in the catalog text or the code
comments mentions it. Two consequences: the blurb should say so, and
**buying Clonal expansion first cancels the cost** (at clonal L=2 the interval is
1.75 s, so even egress L=3 gives 8/1.75 = 4.57 > cap 4). That interaction is the
most interesting build decision currently in the adaptive set and should be
preserved deliberately rather than by luck.

**Dendritic cell: `DcMaxActiveChildren` = 3 binds trivially** (a spent DC returns
to tissue rather than dying, so population = cap). No DC row touches it, which
is correct.

**Plasma cell: `MaxActiveChildren` = 2 binds exactly. ✅** `KillLimit` is 9999 and
the cell never kills, so nothing retires a plasma cell within a round;
population = cap = 2 from ~9 s into every round. A cap row here is guaranteed
non-inert, which is why Germinal-centre output is the B-cell tower's safest row.

**Plasma-cell "lifespan" does not exist. ⚠️** `GameBootstrap`'s plasma-cell
profile comments `KillLimit = 9999, // it never kills; depletion is by lifespan`,
but `UnitLifecycleTuning` has **no lifespan field** — only node-resident
lymphocytes have one (`AdaptiveTuning.LymphocyteLifespanSeconds`). A plasma cell
therefore persists until the round boundary clears it. A "plasma-cell longevity"
row today would write nothing, and the comment is the kind of thing a later
session would trust. Fix the comment or add the field.

**CTL `KillLimit` = 16 is at the measured ceiling.** Halle 2016 puts one CTL at
**2–16 infected cells per day**. The tower is at the top of that range already,
so a kill-limit row would be the first off-evidence number in the adaptive set.

**The Hamming-radius guarantee is already false, and the TCR row's cap destroys
the barcode. ⚠️⚠️** `AdaptiveTuning.cs` states that because species tags are kept
≥4 bits apart, "a helper-T cell within Hamming distance 2 of one species is
within 2 of at most one." By the triangle inequality that needs tags ≥**2r+1 = 5**
bits apart; at exactly 4 both matches are possible. Checking the six live tags in
`PathogenSpecies.cs` (`01011100`, `10100011`, `11110000`, `00001111`, `01100110`,
`10011001`): **13 of the 15 pairs sit at exactly 4 bits**, the minimum the
harness allows. Enumerating all 256 tags:

| Radius | Tags matching ≥1 species | Tags matching ≥2 | Most species one tag covers | Per-pairing teach rate |
|---|---|---|---|---|
| **r=2 (default)** | 158 (61.7%) | **56 (21.9%)** | **3** | 37/256 = 14.5% |
| r=3 (row level 1) | 238 (93.0%) | 184 (71.9%) | 3 | 93/256 = 36.3% |
| **r=4 (row level 2, the current cap)** | **256 (100%)** | **256 (100%)** | **6 — every species** | 163/256 = 63.7% |

Knowledge is still credited only to the species the DC is carrying, so this is
**not** a knowledge-leak bug — but it does mean specificity is weaker than the
doc claims even at default, and at the row's level 2 every helper-T cell in the
node is specific for every pathogen in the game. Recommendations:
**cap the row at 1 level; lower `MatchDistanceCeiling` 4 → 3; correct the comment
to "≥5 bits apart"; and treat this as one more argument for the 16-bit barcode
`CELL_KINETICS_REFERENCE.md` already recommends** — at 16 bits, r≤2 gives
~1-in-480 and the separation constraint becomes satisfiable for 8–10 species.

---

## 6. What needs new mechanics

Four new fields and one new behaviour, ordered by cost-to-value. Two of the four
fields serve two rows each.

**A. `UnitLifecycleTuning.PostKillDetachSeconds` (new field).**
*For:* CTL row "Immune-synapse release".
*What must be built:* after a unit's recognition meter fills and the kill
resolves, hold the unit in place (and ineligible to add pressure elsewhere) for
this many seconds before it resumes searching. Default 1.5 s, −0.4/lvl, floor
0.3. Per-tower and mutable like everything else in that class, so the upgrade
stays a one-field write. Also gives the innate towers a free future knob.
*Cost:* small — one timer on `SearchUnit` and one branch in the kill path.
*Evidence:* Sanchez 2022 ([DOI](https://doi.org/10.1038/s41590-023-01572-4)),
Slaats 2022 ([DOI](https://doi.org/10.1158/2326-6066.CIR-22-0113)).

**B. `UnitLifecycleTuning.ResidentFractionAcrossRounds` (new field).**
*For:* CTL row "Tissue-resident memory (CD103)" **and** the ~30% memory-T
knowledge rung **and** (if the Director wants it) B-cell plasma-cell longevity.
*What must be built:* at the round boundary, retain this fraction of a tower's
living children instead of clearing them, counted against the tower's cap.
Needs a decision about whether retained cells keep their accumulated kill count.
*Cost:* moderate — the round-boundary clear is currently unconditional, and the
marrow's child tracking has to survive the transition.
*Why it is the highest-value item here:* it is the only way to implement the
memory rung correctly (precursors present at t=0 and gentler contraction rather
than a bigger burst), it is the CTL's most distinctive row, and it is the one
place a "longevity" row for plasma cells could ever write anything.
*Evidence:* Casey 2012 ([DOI](https://doi.org/10.4049/jimmunol.1200402)),
Grayson 2002 ([DOI](https://doi.org/10.4049/jimmunol.169.7.3760)),
Badovinac 2003 ([DOI](https://doi.org/10.4049/jimmunol.170.10.4933)),
Whitmire 2008 ([DOI](https://doi.org/10.1371/journal.ppat.1000041)),
Landsverk 2017 ([DOI](https://doi.org/10.1084/jem.20161590)).

**C. Per-tower antibody potency — `AntibodyTuning` → per-tower, plus a
`DimericAntibody` flag.**
*For:* B-cell row "Polymeric IgA assembly (J chain)".
*What must be built:* `AntibodyTuning`'s statics are process-global, which is the
exact mistake `AdaptiveUnitTuning` exists to avoid — buying a B-cell upgrade on
one niche would currently change every niche. Needs the same
statics-plus-per-tower-bonus split, carried onto the `Antibody` instance at
secretion time. Then the cheapest honest implementation of the dimer: a coat
applied by this tower's antibody qualifies the organism for the **existing**
Sprint 22 cross-link raft state while still in tissue, so the 15× reads as "these
two are stuck together and going nowhere" rather than as a damage number.
*Cost:* moderate — the per-tower split is the real work; the effect reuses
`SetCoated` and the raft code.
*Evidence:* Wang 2020 ([DOI](https://doi.org/10.1126/scitranslmed.abf1555)),
Sun 2021 (**UNVERIFIED** DOI).

**D. Species-neighbourhood coating (new behaviour, no new tuning field).**
*For:* B-cell row "Somatic hypermutation".
*What must be built:* `Antibody.TryCoatHere` already consults
`KnowledgeLicence` with the target's species. Add a per-tower integer "coating
Hamming radius" (0 by default) and, when >0, allow the coat if the target's tag
is within that radius of **any** licensed species. Reuses
`AdaptiveTuning.AntigenForSpecies` and the existing Hamming helper.
*Cost:* small — one predicate, given item C's per-tower plumbing.
*Caveat:* given §5's tag geometry, radius 1 is the only safe value; at radius 2
on the current 8-bit tags this would coat most of the roster.
*Evidence:* Kabbert 2020 ([DOI](https://doi.org/10.1084/jem.20200275)).

**E. Not required, but unblocks the above cleanly: the 16-bit barcode.**
Both the Functional-avidity cap and the somatic-hypermutation radius are
constrained by the same thing — 8 bits cannot hold 6+ tags at the ≥5-bit
separation that a radius-2 match rule needs. `CELL_KINETICS_REFERENCE.md`
already recommends 16 bits at r≤2; §5 is a second, independent argument for it.
If it lands, the TCR row can safely return to a 2-level cap and the antibody
radius can go to 2.

---

## 7. Citation list

Verified this session via PubMed (DOIs as returned by the PubMed tool) and
Consensus. Items without a DOI link are marked **UNVERIFIED** — found and quoted
via Consensus, but the DOI itself was not confirmed through PubMed.

**Naming and specificity**
- Slifka MK & Whitton JL 2001, *Nat Immunol* 2:711–7 — functional avidity maturation >50× without TCR affinity change. [DOI](https://doi.org/10.1038/90650)
- Mason D 1998, *Immunol Today* 19:395–404 — one TCR engages ~10⁶ peptides. [DOI](https://doi.org/10.1016/s0167-5699(98)01299-7)
- Raué HP & Slifka MK 2009, *Virology* 390:197–204 — immunodominance shifts independently of avidity maturation. [DOI](https://doi.org/10.1016/j.virol.2009.05.021)
- Riley TP et al. 2018, *Nat Chem Biol* — TCR cross-reactivity via pMHC adaptability. **UNVERIFIED**

**Dendritic cell**
- Cella M et al. 1997, *Nature* 388:782–7 — class-II t½ 10 h → >100 h on maturation. [DOI](https://doi.org/10.1038/42030)
- Lazarski CA et al. 2005, *Immunity* 23:29–40 — class-II:peptide half-life dictates immunodominance. [DOI](https://doi.org/10.1016/j.immuni.2005.05.009)
- Delamarre L et al. 2005, *Science* 307:1630–4 — DCs protease-poor, macrophages protease-rich. [DOI](https://doi.org/10.1126/science.1108003)
- Sallusto F et al. 1995, *J Exp Med* 182:389–400 — constitutive macropinocytosis, irreversibly downregulated on maturation. [DOI](https://doi.org/10.1084/jem.182.2.389)
- Riol-Blanco L et al. 2005, *J Immunol* 174:4070–80 — CCR7's speed module is separate from its chemotaxis module. [DOI](https://doi.org/10.4049/jimmunol.174.7.4070)
- Miller MJ et al. 2004, *PNAS* 101:998–1003 — ~3-min contacts, up to 5,000 T cells/h per DC, encounters by chance not chemotaxis. [DOI](https://doi.org/10.1073/pnas.0306407101)
- Bousso P & Robey E 2003, *Nat Immunol* — DCs scan ≥500 T cells/h without antigen. **UNVERIFIED**
- Harshyne LA et al. 2001, *J Immunol* — DCs extract membrane from live cells and cross-present. **UNVERIFIED**
- Braun A et al. 2011, *Nat Immunol* — CCR7 required for directional entry into the T-cell zone. **UNVERIFIED**

**Helper T / recirculation**
- Thomas N et al. 2012, *PLoS ONE* 7:e45262 — modal LN transit 10–20 h, strong skew. [DOI](https://doi.org/10.1371/journal.pone.0045262)
- Mandl JN et al. 2012, *PNAS* 109:18036–41 — CD4 vs CD8 transit asymmetry; ~⅓ of CD4 transit in MHC-II contact. [DOI](https://doi.org/10.1073/pnas.1211717109)
- Reichardt P et al. 2013, *EMBO J* 32:829–43 — prompt egress reduces parenchymal return and priming effectiveness. [DOI](https://doi.org/10.1038/emboj.2013.33)
- Macallan DC et al. 2004, *J Exp Med* 200:255–60 — naive CD4 intermitotic time ≈1 yr. [DOI](https://doi.org/10.1084/jem.20040341)
- Yoon H et al. 2010, *PLoS ONE* 5:e15423 — CD8 cycle time as short as 2 h, set by the APC's stimulus. [DOI](https://doi.org/10.1371/journal.pone.0015423)
- Borghans JAM et al. 2018, *Immunol Rev* — consensus leukocyte lifespans from deuterium labelling. **UNVERIFIED**

**Cytotoxic T cell**
- Halle S et al. 2016, *Immunity* — 2–16 kills/day; MHC-I downmodulation → kill rate 0.0; cooperative killing. [DOI](https://doi.org/10.1016/j.immuni.2016.01.010)
- Weigelin B et al. 2021, *Nat Commun* 12:5217 — additive cytotoxicity; 3 hits within <50 min decay intervals; swarming. [DOI](https://doi.org/10.1038/s41467-021-25282-3)
- Sanchez EE et al. 2022, *Nat Immunol* 24:1434–42 — apoptotic contraction drives release; amplifying it speeds detachment and serial killing. [DOI](https://doi.org/10.1038/s41590-023-01572-4)
- Slaats J et al. 2022, *Cancer Immunol Res* 10:1462–74 — adenosine weakens pairing and dampens serial hits; blockade accelerates lethal hits. [DOI](https://doi.org/10.1158/2326-6066.CIR-22-0113)
- Liadi I et al. 2015, *Cancer Immunol Res* 3:473–82 — multikilling; CD4 slower "due to the lower granzyme B content". [DOI](https://doi.org/10.1158/2326-6066.CIR-14-0195)
- Wabnitz G et al. 2016, *Adv Biol Regul* 63:107–14 — over-stabilised conjugates impair serial killing. [DOI](https://doi.org/10.1016/j.jbior.2016.09.009)
- Casey KA et al. 2012, *J Immunol* 188:4866–75 — TGF-β/CD103 maintains intestinal TRM; antigen-independent. [DOI](https://doi.org/10.4049/jimmunol.1200402)
- Alfei F et al. 2019, *Nature* 571:265–9 — TOX, PD-1, exhaustion requires high/chronic antigen. [DOI](https://doi.org/10.1038/s41586-019-1326-9)
- Torres SNM et al. 2023, *eLife* — effector CD8 6.5 µm/min in small-intestinal villi. [DOI](https://doi.org/10.7554/eLife.84916)
- Khazen R et al. 2021, *EMBO J* — most CTL hits sublethal; CTL functional heterogeneity. **UNVERIFIED**
- Rothstein TL et al. 1978, *J Immunol* — CTL separates before target death; contact duration does not predict death. **UNVERIFIED**
- Gálvez RI & Jacobs T 2022, *Front Immunol* — exhausted PD-1⁺TOX⁺ CD8 only in long-term infection. **UNVERIFIED**

**Memory**
- Grayson JM et al. 2002, *J Immunol* 169:3760–70 — memory 8× more apoptosis-resistant; secondary contraction 3–5× vs 10–20×. [DOI](https://doi.org/10.4049/jimmunol.169.7.3760)
- Badovinac VP et al. 2003, *J Immunol* 170:4933–42 — day-8 numbers invariant to memory input; secondary contraction prolonged. [DOI](https://doi.org/10.4049/jimmunol.170.10.4933)
- Whitmire JK et al. 2008, *PLoS Pathog* 4:e1000041 — memory quick to respond, indistinguishably slow to divide. [DOI](https://doi.org/10.1371/journal.ppat.1000041)

**B cell / plasma cell / IgA**
- Roost HP et al. 1995, *PNAS* 92:1257–61 — anti-VSV neutralising IgG at Ka 10⁸–10¹⁰ by day 6, no further overall improvement. [DOI](https://doi.org/10.1073/pnas.92.5.1257)
- Foote J & Eisen HN 1995, *PNAS* 92:1254–6 — hapten affinities 10⁵–10⁶ → 10⁷–10⁸; affinity ceiling ~10¹⁰ M⁻¹. [DOI](https://doi.org/10.1073/pnas.92.5.1254)
- Wang Z et al. 2020, *Sci Transl Med* 13:eabf1555 — IgA dimers ~15× more potent than monomers. [DOI](https://doi.org/10.1126/scitranslmed.abf1555)
- Kabbert J et al. 2020, *J Exp Med* 217(11) — gut IgA cross-species reactivity depends on somatic mutations. [DOI](https://doi.org/10.1084/jem.20200275)
- Landsverk OJB et al. 2017, *J Exp Med* 214:309–17 — gut plasma-cell subsets median 11 and 22 years. [DOI](https://doi.org/10.1084/jem.20161590)
- Zhang Y et al. 2018, *J Exp Med* 215:1227–43 — GC plasmablast output set by Tfh IL-21 and stromal APRIL/TACI. [DOI](https://doi.org/10.1084/jem.20160832)
- Chu VT et al. 2014, *Immunity* 40:582–93 — eosinophils support IgA⁺ plasma cells and TGF-β-dependent IgA switching. [DOI](https://doi.org/10.1016/j.immuni.2014.02.014)
- Sun L et al. 2021, *PNAS* — IgA1 dimers up to 240× monomer neutralisation. **UNVERIFIED**
- Lam WY & Bhattacharya D 2017, *Trends Immunol* — "some thousands of antibody molecules per second". **UNVERIFIED**
- Sievert E et al. 2025, *Cell Rep* — ~10,000 Ig molecules/second. **UNVERIFIED**
- Nguyen DC et al. 2025, *Front Immunol* — mature ASCs secrete more IgG per cell than early-minted blood ASCs. **UNVERIFIED**
- Bunker JJ et al. 2017, *Science* — homeostatic mouse IgA polyreactive; SHM made little difference. **UNVERIFIED**
- Benckert J et al. 2011, *J Clin Invest* — most human gut IgA/IgG plasmablasts antigen-specific and somatically mutated. **UNVERIFIED**
- Seo GY et al. 2013, *J Leukoc Biol* — retinoic acid an IgA-selective switch factor, cooperating with TGF-β1. **UNVERIFIED**
- Pabst O 2012, *Nat Rev Immunol* / Spencer J et al. 2007 — ~10¹⁰ plasma cells per metre of gut; 3–5 g IgA/day. **UNVERIFIED**
- Desikan R et al. 2021, *BioEssays* — affinity ceiling from the antigen-acquisition tether. **UNVERIFIED**
- FitzPatrick RD et al. 2020 (abstract) — eosinophils *not* required for LP IgA plasma cells in littermate controls. **UNVERIFIED / contested**

---

*Consensus search results informed parts of this document. Consensus asks that
the following be reproduced verbatim: "Create or connect a free Consensus account
to return more than 3 results per search in Claude Code.:
https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code"*
