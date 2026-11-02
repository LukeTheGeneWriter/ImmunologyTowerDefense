# Research index

Every file here is a **research pass**: a dated, source-verified document
written to answer one question, with its claims checked against PubMed or
Consensus *at the time of writing* and its gaps marked **UNVERIFIED** rather
than filled in from memory.

These are **evidence, not design.** `docs/GAME_DESIGN.md` wins on any
conflict, and `docs/PATHOGEN_ROSTER.md` is where a research finding becomes
a roster decision. Read a pass for *why* a number is what it is, and to
check whether a claim you are about to build on was actually sourced.

**Two standing cautions.**
1. **A pass is a snapshot.** It reflects the literature as read on its date.
2. **Mouse is labelled mouse.** Where a pass says mouse, gerbil, enteroid,
   Caco-2 or gnotobiotic pig, that is load-bearing — several corrections in
   this project came from catching a model-system finding quoted as human.

---

## The pathogen roster passes

| File | Date | Question | Headline |
|---|---|---|---|
| `pathogens_viruses.md` | 2026-10-25 | Which enteric viruses earn a slot? | Rotavirus (built), CMV, norovirus GII.4, astrovirus, Ad F41, HAV |
| `pathogens_invasive_bacteria.md` | 2026-10-25 | Which invasive bacteria? | *Shigella*, *Salmonella*, *Listeria*, *Yersinia*, *Campylobacter*, AIEC |
| `pathogens_extracellular.md` | 2026-10-25 | Which extracellular bacteria? | EHEC, EPEC, EAEC, ETEC, a preformed-toxin slot |
| `pathogens_evasion.md` | 2026-10-25 | How do they evade? | The evasion mechanics behind the shortlists |
| `complement_verification.md` | 2026-10-25 | Does the ~60% complement rung hold? | **No** — no MAC in the gut; re-spec as opsonisation + phagocyte recruitment |
| `pathogens_protozoa.md` | 2026-11-02 | The eukaryotes nobody had looked at | *Cryptosporidium*, *Giardia*, *Entamoeba*, *Cyclospora* |
| `pathogens_helminths.md` | 2026-11-02 | Worms, and the type-2 arm we lack | Worms **cannot** be wave enemies; the tower is **ILC2**, not the eosinophil |
| `pathogens_bacteria_common.md` | 2026-11-02 | The common bacteria, by prevalence | ***C. perfringens*** — top-3 on burden *and* mechanics, missed by four passes |
| `pathogens_viruses_02.md` | 2026-11-02 | Burden + the three open viral questions | Norovirus's knock-back quantified; astrovirus and Ad F41 **argued down** |
| `enteric_burden.md` | 2026-11-02 | Which are actually common? | One apples-to-apples table across four kingdoms |
| `giardia_antigen_route.md` | 2026-11-02 | How does the body learn about *Giardia*? | **Parasite-shed extracellular vesicles** — and antibody triggers the release |

## The mechanics passes

| File | Question |
|---|---|
| `research_killing.md` | How immune cells actually kill, and how fast |
| `research_motility.md` | How they move and find targets |
| `research_proliferation.md` | Replication and turnover rates |
| `research_specificity.md` | Specific recognition vs stress sensing |
| `research_nk_unconventional.md` | NK and γδ — the unconventional roster |
| `research_tertiary_lymphoid.md` | Lymphoid structure and trafficking |
| `upgrades_innate.md`, `upgrades_adaptive.md`, `upgrades_gammadelta.md`, `upgrades_shop.md` | What each upgrade row should actually do |

---

## The findings that changed built code or locked design

A short record, so nobody has to re-derive them. Each is argued in the file
named.

### The organisms

- **Rotavirus was never a cell-killer.** It leaves the cell **non-lytically**
  (~100% release, no permeability change at 72 h) and **arrives in vesicles**
  (>10 virions each, 10–45% of stool virus, stripping them cuts infectivity
  72–83%), which defeated §1b's minimum-dose floor. Both fixed in Sprint 34.
  `pathogens_viruses_02.md`
- **Named species could not spawn at all** until Sprint 32 — rounds rolled a
  `PathogenClass`, and a bare class resolves to the *generic* species. Three
  organisms with traits, sprites and HUD rows had never been on the board.
- ***Vibrio cholerae* actively swims**, and non-chemotactic, CCW-biased
  mutants **out-compete** wild type across the small intestine. "Run straight
  and get everywhere" beats "steer at the wall" — truer and cheaper than what
  is implemented. **Still unbuilt.** `pathogens_bacteria_common.md`
- ***C. perfringens* is not a preformed toxin** — the opposite. CPE requires
  sporulation in the lumen, which is why onset is 8–24 h. The crowd **arms
  itself** if left alone, and it doubles every **7.1 min at 41 °C**.
  Corrects `pathogens_extracellular.md` §3.6.
- ***Campylobacter* swims FASTER as viscosity rises**, so §6b's mucus
  upgrade — a defensive buy — helps it. Second organism whose answer is to
  invert a player upgrade, after A/E *E. coli* and inflammation.
- **Adenovirus F41 must not be built as a cliff.** MICB **only** (MICA
  explicitly not blocked), **retention not transcriptional**, one cell line,
  no NK killing assay. `PATHOGEN_ROSTER.md` calls it "the exact inversion of
  CMV"; the evidence does not carry that.
- **Astrovirus is two threats, not one invisible one.** The capsid-alone
  breach is *stronger* than assumed (polarised Caco-2, TER ≥1000 Ω·cm²,
  apical only, TER→0 by 32 h, reproduced by UV-killed virus and VLPs) but
  **"no inflammation" is refuted for replicating virus** and "no cell death"
  rests on trypan blue alone.
- **Norovirus's knowledge knock-back is ×0.3 with a floor at the CTL rung** —
  only ~30% of antibody blockade survives one variant replacement, while the
  HLA class-I epitopes are **conserved**. And the "2–4 year cycle" is
  **historical only**: Sydney 2012 has held 11+ years, so it must be
  stochastic with a long tail, not a metronome.
- **Sapovirus's cut was right on mechanics and wrong on burden**, and two
  passes disagree *correctly* — #2 in the world by MAL-ED attributable
  episodes, not significantly attributable in a high-income case–control
  series. Different populations. Resolved as a **tier** slot.

### The immune system

- **Debris is ONE input to adaptive immunity, not the only one.** *Giardia*
  kills nothing and sheds no debris, which made it unlearnable on §1c's
  rails. The real route is **parasite-shed extracellular vesicles**, and
  **antibody binding triggers a massive release** — attacking it is what
  makes it teachable. Two plausible routes were rejected *on evidence*:
  transepithelial dendrites (CX3CR1⁺ cells do not migrate to the node) and
  goblet-cell antigen passages (they feed *tolerogenic* DCs and are shut by
  IL-1β, which *Giardia* does not induce — a GAP route would teach
  **tolerance**). `giardia_antigen_route.md`
- **§4a's "parasites don't hide" and "cleared by direct combat damage, more
  of it" are both wrong.** Cytotoxic lymphocytes are inactive against
  helminths; clearance is **expulsion**. The player's instinct — build more
  killers — is the wrong answer for the entire group.
- **Do not ship the eosinophil as the anti-worm tower.** Eosinophil ablation
  made *T. spiralis* larvae **die**. The amplifier that fits is **ILC2**,
  which multiplies three mechanics already built (goblet sloughing, mucus
  turnover, peristalsis).
- **The ~60% complement rung does not hold.** No MAC in the gut, the trigger
  is alternative-pathway so antibody is not what sets it off, and
  Gram-positives are structurally MAC-proof. `complement_verification.md`
- **The only gut IgA protease found belongs to a COMMENSAL**
  (*Sutterella wadsworthensis*), and it alters *C. jejuni* infection and
  neutrophil phagocytosis — so whether §5's ~70% IgA rung works may be partly
  a property of the bacterial lawn.
- **§1d's PRR → immunogenic-apoptosis upgrade is inert against two proposed
  organisms**, for different reasons: *Giardia* never infects a cell, and
  *Cryptosporidium*'s infected cells are survivin-protected. It needs an
  **applicability flag**, not a multiplier.
- **CD8 is largely dispensable for *Salmonella***, and for *Giardia* the CTL
  is actively **harmful** — microvillous shortening transfers with CD8⁺
  cells, not CD4⁺. "Build a killer T cell" is the wrong answer for most of
  the bacterial roster and for the protozoa.

### The map

- **The anatomy rule had a price nobody had measured.** Applied consistently
  it banks four of the most common gut pathogens in the world — *Shigella*,
  *Campylobacter*, *Entamoeba*, *Trichuris*. **Relaxed by the Director,
  2026-11-02: colonic is fine.** `enteric_burden.md`
- **STEC earns the small-bowel slot legitimately** — initial colonisation is
  distal-ileal Peyer's-patch follicle-associated epithelium; the pathology is
  colonic.
- **§4a's parasite class is over-specified.** No enteric protozoan needs the
  multi-slot footprint `BACKLOG.md` calls "more engineering than the other
  three classes combined" — a microsporidian spore is *smaller* than the
  game's bacterium. Single slot with a size multiplier; defer the footprint
  to helminths.
- **FoodNet counts no viruses at all**, so reading it as "the US picture"
  puts *Campylobacter* on top when the burden estimate says norovirus is, by
  a factor of three. Two true tables, opposite answers — the methodological
  trap most likely to mislead a future pass.
