# Innate tower upgrades — review and recommendation

**Scope:** Macrophage, Neutrophil, NK cell (ILC1). Nothing in the repo was edited.

**Attribution:** a substantial part of this document is based on articles retrieved
from **PubMed**, with DOI links given per claim. Some searches were run through
**Consensus**, which asks that the following be reproduced verbatim: *"Create or
connect a free Consensus account to return more than 3 results per search in
Claude Code.: https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code"*

**What I measured before proposing anything** (from
`GameBootstrap.cs`, `BoardConfig.cs`, `BoneMarrowManager.cs`, `UnitTuning.cs`):

| Quantity | Value | Consequence for upgrade design |
|---|---|---|
| Tick | 0.12 s | every per-tick chance converts to a real rate |
| Tissue band | 13 × 10 = **130 coarse cells** | an aura of radius 3 covers 49 of them (38%) |
| Innate emission interval | 4 s (`BoneMarrowManager.EmissionIntervalSeconds`) | sustained kill ceiling = `KillLimit ÷ 4 s` |
| Macrophage | cap 4, KillLimit 20, speed 1, contact 2, efferocytosis 0.05, stress 0.003 | **no lifespan — innate cells die only at `KillLimit`** (`SearchUnit.IsDepletionDue`) |
| Neutrophil | cap 5, KillLimit 5, speed **5** (not 3), burst ×3, stress 0.002 | sustained kill ceiling 1.25/s |
| NK/ILC1 | cap 3, KillLimit 10, speed 3, stress 0.01, aura p=0.02/tick, radius 3 | emission 6 s |
| IFN-γ activation | ×2.5 for 6.0 s, macrophages only (`UnitTuning`) | **global, not per-tower** — not a legal one-field write |
| Upgrade price curve | `base × (1 + 0.6 × level)` | a 30-base cap-3 row costs 30+48+66 = 144 ATP total |

Two binding facts that drive most of what follows:

1. **An innate cell has no lifespan in this game.** It dies only on reaching
   `KillLimit`. So `MaxActiveChildren` **always binds** for innate towers
   (population climbs to the cap and sits there), unlike the helper-T case — and
   conversely `KillLimit` only binds when cells actually reach it.
2. **The IFN-γ aura already saturates in time.** p = 0.02/tick at 0.12 s is a mean
   6.0 s between fires, and activation lasts 6.0 s, so a macrophage in range is
   activated with duty cycle 1 − e⁻¹ ≈ **63%**. Doubling the chance buys 63% → 86%;
   tripling buys 95%. Radius, by contrast, binds hard (38% → 62% → 93% of the
   tissue band at radius 3/4/5).

---

## 1. Recommended final rows

### Macrophage — 4 rows (2 kept, 1 replaced, 1 new)

| Name | Player-facing effect | Field written | Price / cap | Citation |
|---|---|---|---|---|
| **Efferocytic capacity** *(kept)* | Clears debris markedly faster — frees dead ground to regrow, but the dendritic cell gets less to sample. | `EfferocytosisDebrisPerTick` 0.05 → +0.03/lvl | 30 / 3 | Greenlee-Wacker 2016 [DOI](https://doi.org/10.1111/imr.12453); Fadok 1998 [DOI](https://doi.org/10.1172/JCI1112); Medeiros 2009 [DOI](https://doi.org/10.1084/jem.20082058) |
| **Filopodial capture** *(kept, renamed from "Pseudopod reach")* | The cell reaches out, grabs and reels in pathogens a tile further away. | `ContactRadiusFineTiles` 2 → +1/lvl | 45 / 2 | Kress 2007 [DOI](https://doi.org/10.1073/pnas.0702449104) |
| **Trained immunity (β-glucan priming)** *(replaces "Tissue residency (M2)")* | A primed marrow fields more macrophages — and keeps fielding them. | `MaxActiveChildren` 4 → +1/lvl | 40 / 2 | Mitroulis 2018 [DOI](https://doi.org/10.1016/j.cell.2017.11.034); Kaufmann 2018 [DOI](https://doi.org/10.1016/j.cell.2017.12.031); Kleinnijenhuis 2012 [DOI](https://doi.org/10.1073/pnas.1202870109) |
| **Pyroptosis (caspase-1)** *(new, optional 4th)* | A spent macrophage now bursts instead of retiring quietly — big damage, and a mess. | `DegranulatesOnDepletion` false → **true** | 45 / 1 | Fadok 1998 [DOI](https://doi.org/10.1172/JCI1112) (contrast: silent clearance); Saffarzadeh 2012 [DOI](https://doi.org/10.1371/journal.pone.0032366); Silk 2017 [DOI](https://doi.org/10.1038/cddis.2017.52) |

### Neutrophil — 4 rows (1 kept, 1 revised, 1 replaced, 1 new)

| Name | Player-facing effect | Field written | Price / cap | Citation |
|---|---|---|---|---|
| **Azurophil granule reserve** *(revised from "Extended lifespan (GM-CSF priming)")* | Each cell carries more granules out of the marrow — more kills before it goes. | `KillLimit` 5 → +2/lvl | **35 / 3** (was 30 / 4) | Cowland & Borregaard 2016 [DOI](https://doi.org/10.1111/imr.12440) |
| **Apoptosis and silent clearance** *(new — replaces "Controlled degranulation")* | Spent cells die quietly for a macrophage to eat, instead of bursting. No collateral at all — and no final burst either. | `DegranulatesOnDepletion` true → **false** | 45 / 1 | Greenlee-Wacker 2016 [DOI](https://doi.org/10.1111/imr.12453); Saffarzadeh 2012 [DOI](https://doi.org/10.1371/journal.pone.0032366) |
| **Rapid chemokinesis** *(kept, cap cut 2 → 1)* | The cell covers ground faster on its way to a signal — and keeps a straighter line. | `FineTilesPerTick` 5 → +1/lvl | 45 / **1** | Maiuri 2015 [DOI](https://doi.org/10.1016/j.cell.2015.01.056); Zinselmeyer 2008 (already in `CELL_KINETICS_REFERENCE.md`) |
| **Neutrophil cannibalism** *(new, optional 4th)* | Your neutrophils start clearing the dead as well as making them. | `EfferocytosisDebrisPerTick` 0 → +0.02/lvl | 30 / 2 | Ramos 2024, *Cell Death Discov* (Consensus; see §2) |

### NK cell (ILC1) — 3 rows (all new)

| Name | Player-facing effect | Field written | Price / cap | Citation |
|---|---|---|---|---|
| **IL-15 priming (trans-presentation)** | The cell signals more often, so nearby macrophages stay activated almost continuously. | `IfnGammaAuraChancePerTick` 0.02 → +0.02/lvl | 40 / 2 | Lucas 2007 [DOI](https://doi.org/10.1016/j.immuni.2007.03.006); Ebert 2005 [DOI](https://doi.org/10.1111/j.1365-2567.2005.02132.x) |
| **IFN-γ field (bystander signalling)** | The signal carries further — one cell can light up a whole stretch of your macrophage line. | `IfnGammaAuraCoarseRadius` 3 → +1/lvl | 45 / 2 | Thibaut 2020 [DOI](https://doi.org/10.1038/s43018-020-0038-2); Nathan 1983 [DOI](https://doi.org/10.1084/jem.158.3.670) |
| **NKG2D expression** | Reads stress on a cell's surface more reliably — it finds hidden infections on its own. | `StressSenseChancePerTick` 0.01 → +0.005/lvl | 40 / **1** | Hosomi 2017 [DOI](https://doi.org/10.1084/jem.20162041); Hüe 2004 [DOI](https://doi.org/10.1016/j.immuni.2004.06.018) |

Deliberately **not** proposed for the NK tower: any `MaxActiveChildren` row (that is
the clonal-expansion shape the Director banned, and the design's own note says this
tower's value scales with *position*, not headcount); any speed row (its job is to
sit beside the macrophage line, and a faster amplifier leaves the line it amplifies);
`ApoptoticKill` (already true).

---

## 2. Evidence and trade-off, row by row

### Macrophage

#### Efferocytic capacity — keep, but *state* the trade-off

The row exists and works. What is missing is that it is already one of the
best-evidenced trade-offs in the game, and nothing in the UI or the docs says so.

- Efferocytosis is a prerequisite for resolution, and specifically **prevents
  neutrophil lysis** (Greenlee-Wacker 2016, *Immunol Rev*,
  [DOI](https://doi.org/10.1111/imr.12453)).
- It is not passive. A macrophage that has ingested apoptotic neutrophils *actively*
  shuts down IL-1β, IL-8, TNF-α, GM-CSF, LTC4 and TXB2 while raising TGF-β1, PGE2 and
  PAF — and adding those three back reproduces the suppression (Fadok 1998,
  *J Clin Invest*, [DOI](https://doi.org/10.1172/JCI1112)).
- The cost is measured in vivo: apoptotic cells **suppress phagocytosis and bacterial
  killing** by alveolar macrophages through a COX–PGE2–EP2–cAMP pathway, and
  intrapulmonary apoptotic cells impaired clearance of *S. pneumoniae* (Medeiros 2009,
  *J Exp Med*, [DOI](https://doi.org/10.1084/jem.20082058)).

**Why it is a decision:** two costs, one biological and one already built. Biologically,
the macrophage you pay to clean faster is the macrophage that gets worse at killing.
In game, debris is the dendritic cell's antigen substrate (§1c, §5a) — and the DC
tower already ships the inverse row, *Antigen-sparing sampling*, "leaves more for
macrophage clearance". So Efferocytic capacity and that DC row are a tug-of-war over
the same resource, which is exactly the shape the game wants. The one change I would
make is to the effect line, so the player can see the fork: *"…frees dead ground to
regrow, but the dendritic cell gets less to sample."*

#### Filopodial capture — keep, rename

"Pseudopod reach" is not wrong, but the mechanism has a better name and a direct
measurement. Macrophage filopodia act as **phagocytic tentacles**: seconds after
binding a particle they retract and pull it toward the cell body, in F-actin-dependent
~36 nm steps, against up to 19 pN, at 600 nm/s unloaded falling to ~40 nm/s above
15 pN (Kress 2007, *PNAS*, [DOI](https://doi.org/10.1073/pnas.0702449104)). "Reaches
out, grabs, and reels in" is literally what happens, and it justifies a *contact
radius* larger than the cell's footprint rather than hand-waving at cell shape.

**Why it is a decision:** at cap 1 it is a pure buff and the weakest-justified row in
the set. I would raise the cap to 2 (radius 2 → 4 fine tiles) and keep the 45 price,
because at radius 4 a speed-1 macrophage covers a meaningful fraction of a coarse cell
without moving — i.e. you are buying **a sentry instead of a searcher**, which is a
real alternative to Rapid chemokinesis on the neutrophil. That is a position-vs-mobility
decision rather than a number.

#### "Tissue residency (M2)" — **cut it.** Three separate errors, and it may be inert.

This is the correction I would most want the Director to see.

**(a) M1/M2 is not a pair of states you can buy.** The field published a consensus
paper for the explicit reason that "many researchers still consider there to be only
two types of activated macrophages, often termed M1 and M2", called the nomenclature
"contentious and confusing", compared it to the Tower of Babel, and recommended naming
macrophages by **the activator used** rather than by M-number (Murray et al. 2014,
*Immunity*, [DOI](https://doi.org/10.1016/j.immuni.2014.06.008)). Martinez & Gordon
2014 is titled "The M1 and M2 paradigm of macrophage activation: **time for
reassessment**" ([DOI](https://doi.org/10.12703/P6-13)). Even the pro-M1 literature now
reports M1 macrophages promoting tumour progression by sustained cytotoxic pressure,
and calls the binary into question (Kovaleva 2025, *Front Immunol*,
[DOI](https://doi.org/10.3389/fimmu.2025.1638102)). A game may absolutely use the terms
as shorthand — but it should not sell "M2" as a purchasable state, and it definitely
should not attach it to the wrong property.

**(b) M1/M2 does not trade lifespan. It trades arginine, and that *is* zero-sum.**
NOS2 metabolises L-arginine to NO + citrulline (killing); ARG1 hydrolyses it to
ornithine + urea, feeding polyamine and proline synthesis (proliferation, repair). The
two pathways **cross-inhibit at the level of their own products**, which is why the
polarisation is self-sustaining (Rath 2014, *Front Immunol*,
[DOI](https://doi.org/10.3389/fimmu.2014.00532)). The functional consequence was
measured early: NO inhibits cell division while ornithine stimulates it via polyamines,
so M-1 and M-2 responses "influence inflammatory reactions in opposite ways"
(Mills 2000, *J Immunol*, [DOI](https://doi.org/10.4049/jimmunol.164.12.6166)). Two
mouse-genetic datasets support the specific M1 → *worse efferocytosis* direction:
M1-shifted macrophages showed increased apoptosis and impaired efferocytosis, while
M2 and AT1-deficient macrophages were more efficient phagocytes (Yamamoto 2011,
*ATVB*, [DOI](https://doi.org/10.1161/ATVBAHA.111.237198)); and Medeiros 2009 above is
the clean in vivo version of the same reciprocity.
**So the real axis is kill-vs-clear, not short-vs-long** — and the game already owns
both ends as separate fields (`StressSenseChancePerTick`, `EfferocytosisDebrisPerTick`).
A genuine polarisation row is therefore a *two-field* write and belongs in §4.

**(c) In the small intestine, tissue residency does not exist.** This is the part that
makes the current name worst-case wrong for *this map*. Embryonic precursors do seed
the intestinal mucosa and proliferate neonatally, but **they do not persist in the
adult**: around weaning they are replaced by CCR2-dependent Ly6Cʰⁱ monocyte influx that
differentiates locally, is driven largely by the microbiota, and "had to be continued
throughout adult life to maintain a normal intestinal macrophage pool" (Bain 2014,
*Nat Immunol*, [DOI](https://doi.org/10.1038/ni.2967)). Small-intestinal macrophages
specifically are maintained by dietary amino acids rather than microbiota, but still by
continuous monocyte replacement (Ochi 2016, *Sci Rep*,
[DOI](https://doi.org/10.1038/srep27634)). The gut macrophage is the *least* resident
tissue macrophage in the body. Selling "tissue residency" here is like selling
"alveolar" on a gut map.

**(d) It may also buy nothing — the same failure as the old helper-T cap.** Innate cells
have no lifespan; they deplete only at `KillLimit`. With `KillLimit` 20 and a 4 s
emission interval, the tower's sustained kill ceiling is already **5 kills/second**. A
speed-1 macrophage with a 2-fine-tile contact radius, on a 130-cell tissue band, will
not come close to that. If most macrophages never reach 20 kills in a round, raising
the limit to 44 changes literally nothing — exactly the pathology
`ProgenitorUpgrades.ClonalRatePerLevel`'s comment documents for the helper-T.
**Before shipping any `KillLimit` row, instrument it:** log the mean kill count at
macrophage retirement and the fraction of macrophages that retire at all per round. If
that fraction is under ~30%, the row is inert.

#### Trained immunity (β-glucan priming) — the replacement, and the right answer to the brief's second question

**Is it solid enough to be an upgrade? Yes — and it is better suited to this game than
to most, because it is a *bone marrow* phenomenon and these are bone-marrow towers.**

*What it is, in humans.* BCG vaccination of healthy volunteers produced a **4–7-fold**
rise in IFN-γ and a **~2-fold** rise in monocyte-derived TNF and IL-1β **in response to
unrelated bacterial and fungal pathogens**, with CD11b and TLR4 up, mediated by NOD2 and
increased H3K4 trimethylation; and BCG protected SCID mice — no T or B cells at all —
from disseminated candidiasis, 100% survival vs 30% (Kleinnijenhuis 2012, *PNAS*,
[DOI](https://doi.org/10.1073/pnas.1202870109)). β-glucan does the same thing through
Dectin-1 and the non-canonical Raf-1 pathway, again with stable H3K4me3 changes
(Quintin 2012, *Cell Host Microbe*, [DOI](https://doi.org/10.1016/j.chom.2012.06.006)),
and the metabolic basis is a switch to aerobic glycolysis via Dectin-1–Akt–mTOR–HIF-1α;
blocking any of those blocks training, and myeloid HIF-1α-deficient mice cannot mount it
(Cheng 2014, *Science*, [DOI](https://doi.org/10.1126/science.1250684)).

*How long it lasts.* ≥**3 months** is firm, for monocytes (Kleinnijenhuis 2012) and
independently for **neutrophils** (Moorlag 2020, *Cell Rep*,
[DOI](https://doi.org/10.1016/j.celrep.2020.108387)). At **1 year** the cytokine effect
is "less pronounced", but pattern-recognition-receptor expression — CD14, TLR4, mannose
receptor — is still elevated and correlates with proinflammatory output to LPS
(Kleinnijenhuis 2013, *J Innate Immun*, via Consensus). So: strong for a quarter,
attenuated-but-present at a year. In game terms it is a **permanent upgrade**, not a
timed buff — which is what a purchasable row is anyway.

*Why it writes `MaxActiveChildren` and not a per-cell stat.* Because the quantitative
discovery that matters here is central, not peripheral. β-glucan **expands myeloid-lineage
progenitors** in the bone marrow, with raised IL-1β and GM-CSF signalling and adapted
glucose and cholesterol metabolism, improving the response to a secondary LPS challenge
and protecting against chemotherapy-induced myelosuppression — "modulation of myeloid
progenitors in the bone marrow is an integral component of trained immunity, which to
date was considered to involve functional changes of mature myeloid cells in the
periphery" (Mitroulis 2018, *Cell*,
[DOI](https://doi.org/10.1016/j.cell.2017.11.034)). BCG that reaches the marrow rewrites
the transcriptional landscape of HSCs and MPPs, expands them locally, and the resulting
macrophages protect against virulent *M. tuberculosis* better than naïve macrophages do
(Kaufmann 2018, *Cell*, [DOI](https://doi.org/10.1016/j.cell.2017.12.031)). The game has
no "cytokine output" stat, so the honest expression of trained immunity on a progenitor
tower is **more and better-primed output**, and the cap is the field for it.

*Why the cap is a legitimate target here where it was not for the helper-T.* Because
innate cells have no lifespan: standing population rises to `MaxActiveChildren` and
stays. 4 is both the steady state and the ceiling. **The cap binds absolutely.** 4 → 6
over two levels is a 50% increase in standing macrophages, and it respects the Sprint 25
decision that took the cap from 10 to 4 for legibility — it does not walk that back.

*Why it is a decision, not just a bigger number.* The sourced drawback is excellent and
specific: BCG-educated HSCs give "enhanced myelopoiesis **at the expense of
lymphopoiesis**" (Kaufmann 2018). A level-2 purchase should therefore cost the player
something adaptive — the cleanest expression is a small penalty to the helper-T or B-cell
tower's production rate, which is one extra field on `AdaptiveUnitTuning` (see §4).
Second, trained immunity has a shadow twin: LPS induces **tolerance**, not training, and
tolerised monocytes fail to deposit active histone marks at the promoters of
**phagocytic-pathway** genes — which β-glucan then partially reverses (Novakovic 2016,
*Cell*, [DOI](https://doi.org/10.1016/j.cell.2016.09.034)). That is a ready-made
"overwhelmed marrow gets worse at eating" mechanic if the game ever wants one.

#### Pyroptosis (caspase-1) — optional 4th, and the only macrophage row that costs you something

`DegranulatesOnDepletion` is already a per-tower bool and is already wired generically
in `SearchUnit`. Flipping it true on a macrophage converts the quiet scavenger into a
cell that goes out with a burst at its own slot.

The biology is the mirror of Fadok 1998: silent, apoptotic, TGF-β-releasing clearance is
one fate; lytic death that spills the arsenal is the other, and extracellular histones
from lysed myeloid cells are directly cytotoxic to epithelial and endothelial cells and
act as DAMPs through TLR2/4/9 and the NLRP3 inflammasome (Saffarzadeh 2012, *PLoS One*,
[DOI](https://doi.org/10.1371/journal.pone.0032366); Silk 2017, *Cell Death Dis*,
[DOI](https://doi.org/10.1038/cddis.2017.52)).

**Why it is a decision:** it is a strict downgrade to the tissue in exchange for damage.
It also stays clear of the objection recorded in `ProgenitorUpgradeCatalog`'s own header
comment — that a cheap macrophage row "buys away the weak innate stress-sense roll" and
undercuts the knowledge ladder — because it fires **on depletion**, not on contact with
an infected cell, and does not touch `StressSenseChancePerTick`.
**Caveat:** I have not verified the degranulation path renders and scores correctly for a
non-neutrophil kind. Worth a harness check before pricing it.

### Neutrophil

The brief asks which neutrophil axes are real. Verdicts first:

| Axis | Real? | Verdict as an upgrade |
|---|---|---|
| **Granule content / reserve** | Real and well-defined | **The honest justification for the existing `KillLimit` row.** Use it. |
| **Apoptosis vs lysis as the terminal fate** | Real, in vivo, and consequential | **The best neutrophil row available.** One bool. |
| **NETs** | Real; *antimicrobial* contribution in vivo is the contested part, the *host damage* is not | Great mechanic, but area denial + host kill = new mechanic (§4) |
| **Oxidative burst** | Real and non-negotiable (CGD) | No field to write; would be a damage stat the game does not have |
| **Reverse migration** | Real but modest, and its published consequence is *dissemination*, not resolution | Interesting; new mechanic; weaker than the apoptosis row |
| **Lifespan extension by GM-CSF** | GM-CSF priming/anti-apoptosis is real; the *lifespan number* is contested | **Overstated. Rename the row.** |

#### Azurophil granule reserve — revise the name and the numbers, keep the field

The field is `KillLimit`: kills before the cell goes. The current name, "Extended
lifespan (GM-CSF priming)", mismatches it on both halves.

- *Lifespan* is the wrong unit — the game never counts time for an innate cell — and the
  underlying number is genuinely disputed. The headline claim is a circulating human
  neutrophil lifespan of **5.4 days** by in vivo ²H₂O labelling, "at least 10 times
  longer than previously reported" (Pillay 2010, *Blood*,
  [DOI](https://doi.org/10.1182/blood-2010-01-259028)); prior ex vivo estimates were
  <1 day and the field has argued over the model ever since. **I could not resolve the
  published rebuttals through these tools — mark the "~5 days" figure UNVERIFIED and
  don't build a row's name on it.**
- *GM-CSF* is real but it is a **priming** and anti-apoptotic signal, not a granule
  signal, so it does not explain a per-cell kill budget either. (BCG, by contrast, does
  produce durable neutrophil functional reprogramming — ≥3 months, H3K4me3-associated,
  with raised activation markers and antimicrobial function, and it is explicitly
  "associated with a bias toward granulopoiesis in bone marrow hematopoietic progenitor
  cells" — Moorlag 2020, *Cell Rep*,
  [DOI](https://doi.org/10.1016/j.celrep.2020.108387). If you want a cytokine name on
  this row, that is the better-evidenced one.)

What *does* explain a fixed per-cell kill budget is the granule arsenal. Granules are
"densely packed with proteins that contribute to microbial killing when liberated to the
phagosome or extracellularly", they come in ordered subsets (azurophil → specific →
gelatinase → secretory vesicles), and their formation "is a process **intimately
connected to maturation of neutrophils from their precursors in the bone marrow**"
(Cowland & Borregaard 2016, *Immunol Rev*, [DOI](https://doi.org/10.1111/imr.12440)).
That is a per-cell, non-replenishable, **marrow-determined** resource — which is
precisely why buying it at a bone-marrow progenitor is the right place and a mid-field
buff would not be.

**Why it is a decision:** keeping a spent neutrophil in the tissue is itself the
failure-of-resolution mechanism — efferocytosis of apoptotic neutrophils is what
prevents their lysis (Greenlee-Wacker 2016). In game the cost is spatial and real: a
long-lived neutrophil holds its slot in the cap-5 ceiling, so the tower emits fewer
fresh cells, and your coverage concentrates instead of spreading.

**Numbers.** I would **raise the base price 30 → 35 and cut the cap 4 → 3.** As it
stands this is the cheapest row the neutrophil has and it does two jobs at once: at level
4 it takes `KillLimit` 5 → 13, which raises the sustained kill ceiling from 1.25/s to
3.25/s **and** cuts degranulation events per kill by 2.6× — i.e. it is simultaneously the
strongest DPS row and a stealth collateral-reduction row competing with the dedicated
one.

#### Apoptosis and silent clearance — new, and I would make this the flagship

One bool: `DegranulatesOnDepletion` true → false.

A spent neutrophil has exactly two fates and they are not interchangeable. Apoptosis
followed by macrophage efferocytosis is non-inflammatory, actively anti-inflammatory,
and **specifically prevents neutrophil lysis** (Greenlee-Wacker 2016, *Immunol Rev*,
[DOI](https://doi.org/10.1111/imr.12453)); the engulfing macrophage then suppresses its
own proinflammatory output (Fadok 1998, [DOI](https://doi.org/10.1172/JCI1112)). Lysis
spills the arsenal, and the spilled material is directly cytotoxic to epithelium —
NET-associated histones killed alveolar epithelial and endothelial cells
dose-dependently, DNase digestion did not reduce it, and anti-histone antibody did
(Saffarzadeh 2012, [DOI](https://doi.org/10.1371/journal.pone.0032366)).

**Why it is a decision and not a buff:** it **removes the terminal burst entirely**. The
burst is real output in this game — ×3 contact damage at the cell's slot — and it is the
thing that clears a clump of adherents. You are trading a guaranteed finisher for zero
self-inflicted scarring. It also creates the cross-tower hook the roster wants: an
apoptotic neutrophil needs a macrophage to come and eat it, so this row's value depends
on owning macrophages with *Efferocytic capacity*. Two innate towers, one decision.

#### "Controlled degranulation" — fold into the above

The existing row writes `DegranulationBurstMultiplier` 3 → −0.5/lvl with a floor of 1, at
40 ATP base and cap 4: 304 ATP to go from ×3 to ×1. Two problems.

1. **A neutrophil does not partially lyse.** There is no published quantity "fraction of
   granule content released on death" that a 3 → 2.5 → 2 ramp corresponds to. The closest
   real thing is *granule-subset ordering* — secretory vesicles exocytose readily,
   azurophil granules (the most histotoxic: MPO, elastase) most reluctantly
   (Cowland & Borregaard 2016) — which is a discrete ordering, not a dial.
2. **It is the third row in a set where two others already reduce total collateral**
   (the `KillLimit` row cuts degranulation frequency; the bool proposal removes it). Three
   rows pulling one lever is not three decisions.

Recommendation: **cut it and ship the bool.** If the Director wants to keep a graded
version, the defensible name is *"Secretory-vesicle-first degranulation"* and the cap
should be 2, not 4 — but it is strictly dominated by the bool and I would not.

#### Rapid chemokinesis — keep, fix the label, cut the cap to 1

Two bookkeeping errors first. The catalog's debug `FieldLabel` says
`FineTilesPerTick 3 → +1/lvl`, but `GameBootstrap` raised the neutrophil to **5** in
Sprint 23 and `ProgenitorUpgrades` recomputes from the profile — so the row actually runs
5 → 6 → 7. The label is stale. And the bootstrap comment calls 5 "the ceiling of the
useful integer ladder"; at 7 fine tiles per tick a neutrophil crosses a whole coarse cell
each tick, which will read as teleporting. **Cap 1.**

The interesting finding is that this row is *better justified than it looks, and has no
sourced cost*. Cell persistence — the straightness of a trajectory — is robustly and
**positively** coupled to migration speed, across cell types and in vivo as well as in
vitro, because actin flows advect polarity cues; the authors propose it as a generic law
of cell migration (Maiuri 2015, *Cell*,
[DOI](https://doi.org/10.1016/j.cell.2015.01.056)). So a faster neutrophil is also a
straighter one, and the row honestly buys both.

**The correction that matters:** do **not** implement speed as a precision trade-off
("faster but sloppier"). The literature says the opposite, and `CELL_KINETICS_REFERENCE.md`'s
recommendation to spend future resolution on directional persistence should be read in
that light — persistence rides along with speed rather than trading against it. The only
honest cost of this row is emergent: a faster neutrophil reaches more of the board, so
its terminal burst lands in more places and farther from where you would have chosen.
Thin, but real — and it disappears entirely if the player also buys the apoptosis row,
which is a nice interaction.

#### Neutrophil cannibalism — new, optional 4th, and a correction to a code comment

`GameBootstrap` says of the neutrophil: `EfferocytosisDebrisPerTick = 0f, //
neutrophils do not clear debris -- macrophage's job (section 1c)`. That is out of date.
Over the last decade it has been shown that during inflammation **neutrophils contribute
significantly to clearing the apoptotic neutrophils that accumulate at sites of tissue
damage**; this "neutrophil cannibalism" is accompanied by inhibition of the respiratory
burst and of NET formation, and efferocytosing neutrophils secrete anti-inflammatory
mediators and mitogens including HGF, FGF2, VEGF and TGF-β (Ramos et al. 2024,
*Cell Death Discovery* — retrieved via Consensus;
[record](https://consensus.app/papers/details/f47e9bf2f78053aea8cf7662d4e39f57/?utm_source=claude_code)).

**Why it is a decision:** the citation hands you the trade-off in the same sentence as
the ability — a neutrophil that cleans up **stops fighting** (burst off, NETs off). As a
single field write today it is only the upside, so price it as a modest utility row and
flag the downside half for §4. It is also a genuinely surprising correct fact that gives
the neutrophil a second job and a reason to exist after the wave is over.

### NK cell (ILC1)

The brief's framing is right and the literature backs it: this tower's job is amplifying
macrophages, so its rows should buy **amount of amplification, reach of amplification,
and self-sufficiency** — in that order.

#### IL-15 priming (trans-presentation) — the "more often" row

NK priming in vivo is not spontaneous. Using inducible DC ablation, in vivo priming of
NK responses to viral and bacterial pathogens **required** CD11cʰⁱ DCs, depended on
type-I-IFN sensing by those DCs, and was mediated by **trans-presentation of IL-15** by
DCs to resting NK cells — necessary *and* sufficient (Lucas 2007, *Immunity*,
[DOI](https://doi.org/10.1016/j.immuni.2007.03.006)).

And the gut-specific version is stronger still: prolonged IL-15 stimulation of **human
intestinal intraepithelial lymphocytes**, taken from jejunal mucosa, converted them to
CD94⁺ cells producing "massive quantities of IFN-γ", with "a steady rate of transcription
with **no feedback regulation**" (Ebert 2005, *Immunology*,
[DOI](https://doi.org/10.1111/j.1365-2567.2005.02132.x)). Unregulated IFN-γ output under
IL-15, in human small intestine, is exactly this row.

**Why it is a decision.** Three reasons, and the first is a numbers fact the player will
feel: the field **saturates**. At p = 0.02/tick with a 6 s activation window the duty
cycle is already 63%; level 1 buys +23 points, level 2 buys +9. Cap it at 2 and let the
diminishing return be visible — the second level should feel like a worse buy than the
first, because it is. Second, the cross-tower hook is sourced rather than invented: IL-15
reaches the NK cell *from a dendritic cell*, so this row is a real reason to own the DC
tower for something other than the knowledge shuttle. Third, the drawback is the same
molecule: in human small intestine, excess IL-15 is the signal that relays MICA onto the
epithelial surface and drives NKG2D-mediated innate-like killing of enterocytes, with
villous atrophy as the outcome (Hüe 2004, *Immunity*,
[DOI](https://doi.org/10.1016/j.immuni.2004.06.018)). At level 2 this tower should start
costing you wall.

**UNVERIFIED / honest caveat:** the IL-15 story is solid for conventional NK cells and
for intestinal IELs, but the repo's own research pass records that intraepithelial ILC1s
develop independently of **IL-15Rα**, which is part of what distinguishes them from
conventional NK cells. Since the tower is named "NK cell (ILC1)", the row's name is
accurate for the NK half and thinner for the ILC1 half. Flag it in the doc rather than
quietly dropping it; the mechanism is right for the familiar cell the player is placing.

#### IFN-γ field (bystander signalling) — the "further" row, and the best-evidenced NK row available

This is the row I would build first, because IFN-γ's *defining* spatial property is that
it acts past its point of release. Secretion requires local antigen recognition at the
synapse, but the cytokine then "diffuses extensively to alter the tumor microenvironment
in distant areas"; intravital imaging with a STAT1-translocation reporter showed sustained
IFN-γ signalling in **remote** cells, and — the part that settles the design — phenotypic
change in targets required **several hours** of exposure, "a feature that disfavored local
IFN-γ activity over diffusion and bystander activity". T cells "act collectively to create
large cytokine fields" (Thibaut 2020, *Nat Cancer*,
[DOI](https://doi.org/10.1038/s43018-020-0038-2)). A field, not a touch. Radius is the
honest lever.

**Calibration the game should keep, from the paper that named the molecule.** IFN-γ was
identified *as* macrophage-activating factor by measuring exactly what this tower does:
recombinant IFN-γ raised human macrophage peroxide-releasing capacity ~**19.8-fold** at
peak and lifted *Toxoplasma* killing from 2.6 ± 1.3% to 54 ± 0.4%, with half-maximal effect
at ~6 pM; the effect peaked 2–4 days after exposure and reversed within ~3 days of removal
(Nathan 1983, *J Exp Med*, [DOI](https://doi.org/10.1084/jem.158.3.670)). Two consequences.
The game's `IfnGammaActivationMultiplier = 2.5` is **conservative by nearly an order of
magnitude** — there is headroom if the Director ever wants the aura to feel decisive. And
the real kinetics are *days*, so the 6-second activation window is where the compression
lives, not in the multiplier. Both of those are worth recording next to the constant.

**Why it is a decision.** It is purely positional and buys nothing if your macrophages are
elsewhere. Radius 3 covers 49 of 130 tissue cells (38%); radius 4 covers 81 (62%); radius 5
covers 121 (93%). So the two NK rows are deliberately orthogonal: IL-15 buys *more of the
time*, IFN-γ field buys *more of the line*. Cap 2 at 45 base (45 + 72 = 117 ATP) is a real
spend for near-total coverage, and it makes NK placement relative to the macrophage line
the thing the player is actually buying — which is what the design doc says this tower is
for.

#### NKG2D expression — the "on its own" row, and the best-sourced drawback in the document

This is the cell's actual recognition route, and in the small intestine it has been
measured end to end. ER stress in intestinal epithelial cells (epithelial *Xbp1* deletion)
up-regulates the NKG2D ligand MULT1 — human orthologue ULBP1 — via CHOP; NKG2D-expressing
intraepithelial group-1 ILCs accumulate; NKG2D blockade suppresses cytolysis of
ER-stressed epithelial cells in vitro **and spontaneous enteritis in vivo**, and depleting
NK1.1⁺ cells improves the enteritis too (Hosomi 2017, *J Exp Med*,
[DOI](https://doi.org/10.1084/jem.20162041)). In human coeliac disease the same axis
destroys villi: MICA is strongly expressed on the epithelial surface in active disease,
induced by gliadin with IL-15 as the relay, and NKG2D engagement triggers innate-like
cytotoxicity toward enterocytes — "villous atrophy in coeliac disease might thus be
ascribed to an IEL-mediated damage to enterocytes involving NKG2D/MICA interaction"
(Hüe 2004, *Immunity*, [DOI](https://doi.org/10.1016/j.immuni.2004.06.018)).

**This is the answer to the brief's last question.** Asked for the best-evidenced upgrade
that should make a tower *worse* at something, this is it: turning this receptor up makes
the cell kill **stressed-but-uninfected** enterocytes, and that claim comes with a
mechanism, a receptor, a ligand, the correct organ, and two disease names. The repo already
has this queued in `CELL_KINETICS_REFERENCE.md` ("the stress sensor is also the autoimmunity
cell"); this is the row it belongs to.

**Cap must be 1, and here is why.** §4c fixed the recognition ladder at innate 0.003 /
cytotoxic 0.03 with the Director's explicit ceiling of ~10× and a hard stop at ~20×. The NK
sits at 0.01, a third of the CTL. A +0.005 step puts it at 0.015 — half the CTL, 5× the
macrophage — which is defensible. A second level would reach 0.02, two-thirds of the CTL,
and start eroding the ~10%-knowledge rung the whole ladder exists to make the player want.
One level only, and say so in the comment.

**As a single field write today it is a pure buff** — the drawback half needs a
false-positive roll against stressed/healthy cells. I would still ship it at cap 1 and
price it at 40, but the row is only *interesting* once §4's stressed-cell mechanic exists.

---

## 3. What I would cut and why

| Row | Verdict | Reason |
|---|---|---|
| **Macrophage — "Tissue residency (M2)"** | **Cut** (replace with Trained immunity) | Wrong on three counts and possibly inert. M1/M2 is a contested binary the field published a consensus paper to discourage (Murray 2014, [DOI](https://doi.org/10.1016/j.immuni.2014.06.008)); what polarisation actually trades is arginine between NOS2 and ARG1, i.e. **killing vs repair, not short vs long** (Rath 2014, [DOI](https://doi.org/10.3389/fimmu.2014.00532)); and **intestinal macrophages are not tissue-resident at all** — the adult pool requires continuous CCR2-dependent monocyte influx (Bain 2014, [DOI](https://doi.org/10.1038/ni.2967)). On top of that, at `KillLimit` 20 with a 4 s emission interval the tower's kill ceiling is already 5/s, which a speed-1 macrophage cannot approach — so the row is a candidate for the same inertness the helper-T cap had. |
| **Neutrophil — "Controlled degranulation"** | **Cut** (replace with the apoptosis bool) | A neutrophil does not partially lyse; there is no published quantity the 3 → 2.5 → 2 → 1 ramp corresponds to. The real structure is discrete granule-subset ordering (Cowland & Borregaard 2016, [DOI](https://doi.org/10.1111/imr.12440)). It is also the third of three rows that all reduce total collateral, which is one lever wearing three hats, and at 304 ATP for the full ramp it is the most expensive thing in the innate shop. |
| **Neutrophil — "Extended lifespan (GM-CSF priming)"** | **Keep the field, cut the name** | The field counts kills, not time; and the neutrophil-lifespan literature is contested — the 5.4-day figure (Pillay 2010, [DOI](https://doi.org/10.1182/blood-2010-01-259028)) is ~10× prior estimates and I could not resolve the rebuttals here (**UNVERIFIED**). GM-CSF is a priming/anti-apoptotic signal, not a granule signal. "Azurophil granule reserve" names the actual non-replenishable, marrow-built resource the field models. |
| **Neutrophil — "Rapid chemokinesis" cap 2** | **Cut to cap 1**, fix the stale label | `FieldLabel` says base 3; the real base is 5 since Sprint 23, so the row runs 5 → 7. The bootstrap calls 5 "the ceiling of the useful integer ladder", and 7 fine tiles/tick is a full coarse cell per tick. |
| **Any NK `MaxActiveChildren` row** | **Never ship it** | It is the clonal-expansion shape the Director banned, and the design's own note says this tower's value scales with position rather than headcount. Human NK proliferation is unchanged in acute infection; homeostatic doubling ~16 days. |
| **Any innate `ApoptoticKill = true` row** | **Never ship it** | One bool would hand an innate tower the entire adaptive payoff. §4c's whole case for buying cytotoxic cells is `TissueTuning.ApoptoticDebrisFraction`; a cheap innate row that sets `ApoptoticKill` quietly evaporates it, and every existing test would still pass. Worth an explicit "do not" comment in `ProgenitorUpgrades`. |
| **Any macrophage `StressSenseChancePerTick` row** | **Agree with the existing omission** | The catalog header already rules out "inflammasome priming" for undercutting the innate→adaptive bridge, and after Sprint 23's tenfold recalibration that reasoning is stronger, not weaker. The pyroptosis row I propose above deliberately fires on depletion instead, so it does not touch this field. |

**One naming nit, not a cut.** What the game calls neutrophil *degranulation* — a terminal
self-destruct that damages the surroundings — is mechanically closer to **lysis / NETosis**
than to classical degranulation, which is exocytosis by a living cell. The player-facing
word is fine; the code comment could note it, since the distinction is exactly what the
apoptosis row is selling.

---

## 4. What needs new mechanics

Ordered by evidence-per-unit-of-work.

### 4.1 Per-tower IFN-γ activation duration → unlocks **Cytokine-induced memory-like NK**
**Cost: one float field, mirroring the two aura fields exactly. ~15 lines.**
`IfnGammaActivationSeconds` and `IfnGammaActivationMultiplier` are statics on
`UnitTuning`, so neither is a legal one-tower write. Adding
`IfnGammaActivationSecondsBonus` to `UnitLifecycleTuning` (seeded 0 from `UnitProfile`,
read at the one call site in `SearchUnit.CheckIfnGammaAura`) makes the best-evidenced
innate-memory mechanic in the literature a one-field row.

Why it is worth it: CIML is the strongest NK-memory evidence base *because it is
clinical*, and its measured shape is precisely an amplifier upgrade. Cytokine-activated
NK cells transferred into naïve hosts, detected 7–22 days later, produced significantly
**more IFN-γ on restimulation** — while **not expressing granzyme B and killing targets
similarly to naïve NK cells** (Cooper 2009, *PNAS*,
[DOI](https://doi.org/10.1073/pnas.0813192106)). Human NK cells given a brief IL-12 +
IL-15 + IL-18 preactivation and a 7–21 day rest showed enhanced IFN-γ on restimulation,
retained in proliferating cells (Romee 2012, *Blood*,
[DOI](https://doi.org/10.1182/blood-2012-04-419283)); the approach reached a first-in-human
phase 1 trial in relapsed/refractory AML with clinical responses in 5 of 9 evaluable
patients including 4 complete remissions (Romee 2016, *Sci Transl Med*,
[DOI](https://doi.org/10.1126/scitranslmed.aaf2341)).

**It is a decision because the biology says the killing does not improve.** A row that
buys longer activation and explicitly nothing else is honest, legible, and the opposite
shape from IL-15 priming (rate) and IFN-γ field (reach) — three NK rows, three axes, no
overlap. It is also cytokine-primed rather than antigen-primed, so it needs no knowledge
rung, which is exactly the innate-memory gap the roster has.

### 4.2 CD16 / ADCC — the NK row that should make the tower worse
**Cost: medium. One new field + a read of the existing `SetCoated` state + a knowledge gate.**
The evidence here is a clean receptor-level dichotomy, measured in resting human NK cells:
KIR2DL4 induces IFN-γ **but not** cytotoxicity, while **CD16 and 2B4 induce cytotoxicity
but not IFN-γ** — "a functional dichotomy (cytokine production vs cytotoxicity) … as
dictated by the signals of individual receptors" (Rajagopalan 2001, *J Immunol*,
[DOI](https://doi.org/10.4049/jimmunol.167.4.1877)).

So the row writes itself: **buy CD16 and the aura weakens while the cell gains a kill that
only works on antibody-coated targets.** Everything it needs already exists — the B-cell
tower's `SetCoated`, and the ~45% Fc-receptor knowledge rung. Two sourced constraints keep
it from scaling forever: ADCC is a coating-density-dependent kill-rate multiplier with a
sharp low-density cutoff below which it does essentially nothing (per the repo's own §5
synthesis), and ADAM-17-mediated CD16 shedding on NK activation measurably impairs ADCC,
with selective ADAM-17 inhibition restoring CD16 (Oliviero 2017, *J Hepatol*,
[DOI](https://doi.org/10.1016/j.jhep.2017.01.032)). **Do not quote a fold-change for
ADCC-over-natural-cytotoxicity** — the repo already marks that UNVERIFIED and it is
assay-dependent.

### 4.3 NKG2D false positives — the drawback half of the NK row
**Cost: medium. Needs a "stressed but uninfected" host-cell state and a false-positive roll.**
Hosomi 2017 and Hüe 2004 (above) both measure the same thing: raising stress-ligand
surveillance makes group-1 ILCs kill *stressed, uninfected* enterocytes, producing
enteritis and villous atrophy. The honest implementation is that NKG2D levels 1+ add a
chance per tick of killing a `Stressed` host cell that has no pathogen in it, feeding
debris and fibrosis. Until that exists, the NKG2D row is a pure buff and should be priced
and capped as one.

### 4.4 Trained immunity's lymphoid cost
**Cost: small. One multiplier on `AdaptiveUnitTuning`, applied by the macrophage row.**
"Enhanced myelopoiesis **at the expense of lymphopoiesis**" (Kaufmann 2018,
[DOI](https://doi.org/10.1016/j.cell.2017.12.031)) is a two-tower effect, so it breaks the
one-field rule by construction. It is also the best cross-tower trade-off in the innate set
and the one that would make the innate/adaptive step feel like a fork rather than a
progression. Suggested shape: level 2 of Trained immunity applies a small production-rate
penalty to the player's helper-T and B-cell towers.

### 4.5 Neutrophil extracellular traps
**Cost: high. A persistent area object with its own lifetime, host-damage tick, and pathogen-immobilisation.**
Both halves are solid. NETs bind Gram-positive and Gram-negative bacteria, degrade
virulence factors, kill bacteria, and are abundant in vivo in experimental dysentery and
human appendicitis — "a form of innate response that binds microorganisms, **prevents them
from spreading**, and ensures a high local concentration of antimicrobial agents"
(Brinkmann 2004, *Science*, [DOI](https://doi.org/10.1126/science.1092385)). And they kill
the host: NETs induce epithelial and endothelial cell death dose-dependently, histone-
mediated, not rescued by DNase (Saffarzadeh 2012,
[DOI](https://doi.org/10.1371/journal.pone.0032366)), with extracellular histones acting as
DAMPs through TLR2/4/9 and NLRP3 (Silk 2017, [DOI](https://doi.org/10.1038/cddis.2017.52)).
In game: a NET is **area denial that scars the ground it holds** — immobilise pathogens in a
patch, damage the host cells under it, decay over time. That is the most mechanically
interesting unbuilt innate idea, and it is also the Director's old request ("trigger
self-destruction deliberately at a chosen moment") made concrete, since NET release is
exactly a deliberate terminal act. Honest caveat: the *antimicrobial* contribution of NETs
in vivo is the contested part of this literature; the *host damage* is not. Build it as
control, not as damage.

### 4.6 Neutrophil swarming / LTB4 relay
**Cost: medium-high. Needs an emitted chemoattractant source and a recruitment-radius effect.**
This is the honest mechanism behind "my neutrophils converge on the fight", and it is
measured: neutrophil-derived LTB4 is an intercellular **signal relay** that "acutely
amplifies local cell death signals to enhance the radius of highly directed interstitial
neutrophil recruitment"; integrins are dispensable for long-range migration but hold dense
clusters together while the swarm physically rearranges the collagen network into a
collagen-free zone, forming a **wound seal** that isolates the site from surrounding viable
tissue (Lämmermann 2013, *Nature*, [DOI](https://doi.org/10.1038/nature12175)). Two
game-shaped consequences: recruitment radius is a *relayed* quantity, not a speed stat; and
a swarm has a positive side (seal, containment) and a negative one (it tears up the
matrix it seals). This is a better long-term home for "the neutrophil gets to the fight"
than the raw speed row.

### 4.7 Macrophage polarisation as a real two-field row
**Cost: small mechanically, large in design.**
The arginine axis is genuinely zero-sum (Rath 2014,
[DOI](https://doi.org/10.3389/fimmu.2014.00532); Mills 2000,
[DOI](https://doi.org/10.4049/jimmunol.164.12.6166)), and the game already owns both ends
as separate fields: `StressSenseChancePerTick` (kill) and `EfferocytosisDebrisPerTick`
(clear). A row that raises one while lowering the other is a textbook polarisation
mechanic. **I am not proposing it**, for one reason: raising innate stress-sensing is the
thing the catalog's own header comment rules out, and Sprint 23's tenfold recalibration
makes that ruling stronger. If the Director ever wants it, the version that respects the
ladder is a **slider, not a ladder** — one row with a level that moves *along* the axis in
both directions, bought once, re-settable — and it would be the first upgrade in the game
that you can turn down as well as up. Which is, biologically, exactly what polarisation is.

### 4.8 Reverse migration
**Cost: medium. Low priority.**
Real but modest, and its published consequence is the opposite of a benefit: real-time
confocal imaging in vivo showed hesitant and reverse transendothelial migration of
neutrophils after ischaemia-reperfusion injury, enhanced by JAM-C blockade or deletion, and
the authors conclude reverse TEM "can contribute to the **dissemination of systemic
inflammation**" (Woodfin 2011, *Nat Immunol*, [DOI](https://doi.org/10.1038/ni.2062)). As a
row it would be "spent neutrophils leave the tissue instead of dying in it" — no collateral,
no burst, and a cost somewhere else. That is strictly weaker than, and overlapping with, the
apoptosis bool in §2, which achieves the same player-facing outcome with one field and
better evidence. Park it.

### 4.9 Neutrophil cannibalism's downside
**Cost: small. Needs a per-cell "currently efferocytosing" state that suppresses killing.**
Ramos 2024 reports the inhibition of respiratory burst and NET formation *as part of* the
cannibalism phenotype. Implementing only the upside makes the row a free buff; a cheap
version is to suppress the cell's contact damage on ticks where it cleared debris.

---

## 5. Instrumentation I would run before shipping any of this

1. **Mean kills at innate retirement, and retirement rate per round**, logged per kind.
   This is the single measurement that decides whether *any* `KillLimit` row is real. If
   under ~30% of macrophages reach 20 kills, the current M2 row has been inert since
   Sprint 18 and the neutrophil row is the only one carrying weight.
2. **Fraction of ticks a macrophage spends IFN-γ-activated, with and without an NK in
   range.** The 63% duty-cycle figure above is derived, not measured; if the real number
   is already near 100% because NK cells cluster, then `IfnGammaAuraChancePerTick` is
   closer to inert than to saturating and IL-15 priming should be cut to cap 1.
3. **Standing innate population vs cap, per round.** Confirms the claim that innate caps
   always bind — the premise of the Trained immunity row.
