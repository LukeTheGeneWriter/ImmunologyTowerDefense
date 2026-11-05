# Pathogen roster — the named organisms, and why each earns a slot

Companion to `CELL_KINETICS_REFERENCE.md`, same role: this is the evidence
the design was argued from, not the design itself. `GAME_DESIGN.md` wins
on any conflict.

Assembled 2026-10-25 from four parallel research passes plus one targeted
verification pass (`docs/research/pathogens_*.md`,
`docs/research/complement_verification.md`). Attribution: substantially
based on articles retrieved from **PubMed**, with DOIs in the passes.

## The organising rule

**A species earns a slot by forcing a decision no existing enemy forces.**
Not by being famous, not by being dangerous. Four of the shortlisted
organisms were cut for being mechanically identical to one already on the
list, and one famous organism was cut on anatomy (below).

## Built in Sprint 24

| Species | Class | The mechanic |
|---|---|---|
| **Rotavirus** | virus | **Positional tropism** + a viral enterotoxin |
| **Vibrio cholerae** | large bacterium | **Subversion at a distance** — wins without killing |
| **A/E E. coli** | large bacterium | **Feeds on your own inflammation** |

### Rotavirus — the board heals from one end

Rotavirus infects **mature villus-tip enterocytes and never the Lgr5+
crypt stem cell**. The crypt therefore keeps producing replacements
underneath an infection that cannot reach it, and hyper-proliferates to
repave from below.

In game terms: a plague that eats the wall but can never touch the engine
that rebuilds it. It makes the board's *geometry* matter — which end of
the tissue band a thing can reach is suddenly a property worth reading.

It also carries **NSP4, a genuine viral enterotoxin**, so an infected cell
damages healthy neighbours it never infects. The two traits compose:
**the toxin reaches cells the virus is forbidden to enter**, so damage
creeps toward the crypt the infection can never occupy. A player who
clears only the cells that look infected stays a step behind.

Deliberately abstracted: rotavirus's real ID50 is ~1 infectious unit with
no dose-dependence over four logs. Taken literally that deletes §1b's
proximity-gated adhesion roll entirely. **Tropism literal, dose
abstracted.**

**Two corrections, found 2026-11-02 and fixed in Sprint 34**
(`docs/research/pathogens_viruses_02.md`). Both were wrong in the same
direction: the game had been playing rotavirus as an ordinary lytic virus
arriving at an ordinary dose.

1. **It leaves the cell non-lytically** — ~100% of progeny released, with no
   measured permeability change at 72 h. So an ignored focus does not burst
   and resolve. §1c appeared to have no host-cell state for "released its
   virus and is still alive", but §4b's *budding* mode already was that
   state; the real defect was that budding had never been per-species — a
   coin flip chose it, for 29 sprints. Rotavirus remains self-limiting, by
   the crypt repaving beneath its tropism rather than by burn-out.
2. **It arrives in vesicles** — 10-45% of stool virus is membrane-bound,
   >10 virions per vesicle, and stripping the vesicles cuts infectivity
   72-83%. That defeats §1b's minimum-dose floor, because one arriving unit
   is already a dose. Implemented as a per-species crowd weight, capped
   below the floor so §1b Step 2's locked property survives. The dose is
   still abstracted; what changed is that it is now **species-specific**,
   which is exactly what §1b Step 2 said the floor would become.

### Vibrio cholerae — it wins without killing anything

Never enters a cell, never takes a slot, never kills. It adheres and
secretes, and the stretch of wall under the field is **subverted**: alive,
still occupying its slots, still blocking invasion, and no longer doing
the player's work.

Three composable effects, all from the real mechanism:
1. The cells live and stop being yours.
2. Tight junctions loosen (claudin-2 up, tricellulin down), so a plaque
   over subverted wall **charges faster**.
3. The damage releases lactate the organism feeds on — a visible,
   interruptible feedback loop rather than a flat debuff.

Subversion **decays** once the secretors are cleared, which is what makes
clearing a cholera crowd feel like it achieved something. Killing cells
would have left permanent holes instead.

This is the one enemy on the board that the player's instinct — look for
dying cells — will miss entirely.

### A/E E. coli — the enemy that profits from your response

Host oxidants generate nitrate and tetrathionate that **only a facultative
anaerobe can respire**, so an inflamed stretch of wall is a better place
to live than a quiet one. ROS-lysed commensals feed it further.

**This inverts the assumption the whole game has run on**, that more
immune response is always better, and it was the one strategy found in the
research with **no answer anywhere in the roster**. A neutrophil-heavy
clear makes the survivors faster.

Implemented as `LocalInflammation` reading dead and infected tissue behind
the wall — deliberately the *products of damage* rather than a cytokine
level, because what the organism respires is the mess, not the signal. A
player who clears messily feeds it even after the cytokines fade.

It also has **intimate (pedestal) adhesion**, so both of the player's
flush answers — IgA enchainment and goblet sloughing — work poorly on it.

## Shortlisted, not built

**Viruses:** CMV (the stealth boss — four independent MHC-I mechanisms,
already behind the wall, ~96 h lytic cycle, so it must be a boss and never
a wave enemy); norovirus GII.4 (antigenic replacement — literally §5's
mutation discount); astrovirus (**breach with no infection and no stress
signal** — capsid alone opens tight junctions, so it defeats the entire
detection stack by not being detectable); adenovirus F41 (a *partial*
NK-blind, see below); hepatitis A (quasi-enveloped, antibody-resistant).

> **Two corrections to the entries above (Sprint 34 research, recorded
> Sprint 35).** Both were overstatements in the direction of a tidier
> mechanic, which is the direction this document has to be most careful
> about.
>
> - **Norovirus's "real 2–4 year cycle" is historical only.** Sydney 2012
>   has now held for 11+ years. Replacement is **stochastic with a long
>   tail**, not a metronome, so the mechanic must not be built as a timer
>   that fires every N rounds — that would teach the player a schedule
>   that does not exist.
> - **Adenovirus F41 must not be built as a cliff.** "CTL-visible but
>   NK-blind, the mirror of CMV" is more than the evidence carries: the
>   finding is MICB only, by *retention* rather than transcriptional
>   shutdown, in one cell line, with **no NK killing assay**. A
>   partial, single-ligand effect is a modifier, not an immunity.

**Invasive bacteria:** *Shigella* (actin-tail spread, never returns to the
extracellular space, lowest ID of anything); *Salmonella* Typhimurium
(**two speeds in two niches** — hyper-replication in epithelial cytosol,
near-dormant in the macrophage vacuole); *Listeria* (enters at a named
door — goblet cells); *Yersinia* (the anti-tower unit, Yop-injects
phagocytes); *Campylobacter* (the gate-opener, lets bystanders through);
AIEC (replicates in an acidified phagolysosome **without killing the
macrophage** — leaves an infected but still-functional tower).

**Extracellular:** EHEC (**destruction aimed past the wall** — human gut
epithelium lacks Gb3, so Shiga toxin translocates and hits the
vasculature); EPEC; EAEC (**farms mucus** — Pic is both a goblet
secretagogue and a MUC2 protease, inverting the §6b mucus upgrade); ETEC;
a shared preformed-toxin slot.

**~~Rejected: *Clostridioides difficile*, on anatomy.~~ SUPERSEDED — the
anatomy rule is relaxed (Director, 2026-11-02).** The original reasoning
stands as biology: *C. difficile* is colonic, small-bowel CDI is a
reportable rarity, and its antibiotic-demolition trigger needs a dense
microbiota the small intestine does not have.

What changed is the **cost** of the rule, which nobody had priced until the
2026-11-02 burden pass (`docs/research/enteric_burden.md`). Applied
consistently it does not merely bank *C. difficile* — it banks **four of
the most common gut pathogens in the world**: ***Shigella*** (#1 on three
of five global metrics, 24% of under-five diarrhoeal deaths, colonic and
rectal, with the organ specificity explicitly unexplained in the
literature), ***Campylobacter*** (#1 in FoodNet at 20.6/100k; human disease
is colitis), ***Entamoeba histolytica*** (whose trogocytosis answers **two**
of the five standing "no answer in the roster" gaps — killing the phagocyte
outright, and a justification for a NET mechanic) and ***Trichuris*** (the
best helminth mechanic the research found).

**The Director's ruling: colonic is fine.** Anatomy is no longer a veto. It
remains a **note** — an organism's real compartment is recorded, and a
colonic candidate is labelled as such rather than quietly relocated — but
it does not disqualify. The map is "the gut" rather than strictly the
jejunum, and the honest consequence to state is that §1a's three bands are
now a *generic* mucosal cross-section. Nothing already built changes; what
changes is that the shortlist above is no longer filtered by compartment.

## Corrections to standing design

1. **§4a's "the brood bursts out" is wrong for *Salmonella*.** The infected
   enterocyte is **extruded into the lumen** with invasion-primed,
   flagellated bacteria inside — so the brood should route back into §1b's
   adhesion/mill loop rather than appearing in tissue.
2. **§4a reserves adjacent-tile spread for viruses. That is wrong for
   *Shigella* and *Listeria*,** which spread cell-to-cell by actin tail.
3. **Norovirus tuft-cell tropism is *murine* norovirus.** Human norovirus
   replicates in enterocytes and enteroendocrine cells. (Rotavirus, oddly,
   does infect tuft cells.)
4. **Adenovirus E3-19K is absent from exactly the enteric serotypes**, so
   Ad41 must not be built as a stealth virus — it is the opposite.
5. **CD8 is largely dispensable for *Salmonella*** (β2m⁻ mice resist;
   MHC-II⁻ and IFN-γR⁻ do not). The player's intuitive "build a killer T
   cell" answer is wrong for most of the bacterial roster — which is a
   good thing, and worth teaching deliberately.

## The ~60% complement rung — verified, and it needs re-specifying

The rung currently reads "antibody destroys the target's membrane
**directly, with no immune cell present**." A targeted verification pass
confirmed the problem and found two corrections beyond it.

Wu et al., *Cell* 2024;187(4):897-913
([DOI](https://doi.org/10.1016/j.cell.2023.12.036)) states in its abstract:
*"The absence of membrane attack complex (MAC) components in the gut
ensures that C3 deposition does not result in the lysis of commensals."*
C3 is locally made, primarily by stromal cells.

Two caveats that matter, both found by the verification pass rather than
assumed:

- **It is a colon paper.** "Small intestine", "ileum", "jejunum" and
  "duodenum" appear nowhere in it. The small intestine is *untested*, not
  confirmed. No group has replicated the C5–C9 survey.
- **The trigger is wrong too, not just the effector.** Luminal C3
  deposition is unchanged in C1q- and MBL-deficient mice — it is
  alternative-pathway, so *antibody* is not what sets it off either.

Independently: **Gram-positives are structurally MAC-proof.** Joiner 1983
found ~10,000 molecules of C7 and C9 bound per organism without killing
it, concluding the complex fails to penetrate the thick peptidoglycan —
and the jejunal/ileal lumen is dominated by Gram-positive streptococci.

**Recommendation (Director's decision pending):** re-specify the rung as
**opsonisation + phagocyte recruitment** — antibody and C3 coat the
target, a recruited neutrophil delivers the kill. If a direct-lysis beat
is wanted later, its honest home is *inflamed, barrier-breached* tissue,
where MAC does appear — and attacks the host epithelium, which is a
different and arguably better mechanic.

## Still with no answer in the roster

Flagged so a future tower can be justified by a real gap rather than
invented: **inflammation-as-fuel** (now built, still unanswered);
**stress-ligand shedding** (soluble decoys that degrade the receptor on
nearby cells — a sensor-disabling aura); **killing the phagocyte outright**
(we have no "tower dies from what it ate"); **NET degradation** (no NET
mechanic to disable); **neutrophil-opened invasion** (PMN transmigration
is *Shigella*'s route in).

Candidate answers named by the research: a **Treg** tower (alarmin-
triggered, powered by the player's own loud kills) and **ADCC** via the
~45% Fc rung.
