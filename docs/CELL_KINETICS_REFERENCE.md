# Cell kinetics reference — the numbers behind the Sprint 23 roster

**Status: research input, not a design document.** `GAME_DESIGN.md` is the
design; this file is the evidence it was argued from, kept so a later
session can check a number instead of re-deriving it or trusting a comment.
Same role `IGA_REFERENCE.md` plays for the Sprint 22 wall.

Assembled 2026-10-25 from six parallel research passes run against PubMed
and Consensus. Every substantive claim carries a citation; numbers that
could not be resolved are marked **UNVERIFIED** rather than quietly
dropped, and those marks are load-bearing — six of them are things a later
session might otherwise assume we checked.

Attribution: a substantial part of this reference is based on articles
retrieved from **PubMed**, with DOI links given per claim in the sections
below.

## How to read these numbers

**Absolute scale in this game is deliberately broken, and was before this
research started.** One macrophage step is ~24 µm/s, roughly a thousand
times a real macrophage. So the research was briefed to deliver *ratios
between cell types* first and absolute figures second — the ratio is what
the game can express, and the absolute figure is how we know what we
compressed and by how much.

Three places where the honest number would make a worse game, recorded so
nobody "fixes" them toward realism without knowing the cost:

| Quantity | Reality | In game | Why |
|---|---|---|---|
| T-cell precursor frequency | ~1 in 10⁵–10⁶ | ~1 in 7 | At the real rate, reaching 100% knowledge takes ~3 years of node throughput. Biology makes meetings *numerous*, not *likely* — a DC scans 500–5,000 T cells/hour. We cannot show every meeting and also make meetings rare. |
| CD8 clonal expansion | 10⁴–10⁵-fold | `MaxActiveChildren` ≈ 8 | Nothing that could carry 10⁴ would leave a playable board. The honest compression is into *time* (late and decisive), not headcount. |
| CTL in vivo kill rate | 2–16 targets/day | ~1 per 4 s of contact | A faithful rate would mean a CTL kills roughly once per real-time hour. |

## The one correction that changed a tower

The brief assumed NK cells are cytotoxic cells that need no priming, and
that this made them a cheap competitor to the CTL. The literature says
otherwise, and the tower was redesigned around it:

- Human **intestinal** NK cells are published as *less* cytotoxic and
  *higher* IFN-γ-producing than blood NK cells (Chinen 2007).
- The clearest in vivo demonstration of NK function in this setting is NK
  cells cutting **intramacrophage** *Salmonella* load (Lapaque 2009) — the
  measurable contribution is making a phagocyte better, not killing.
- NK effector outputs are separably wired at the receptor level: KIR2DL4
  gives IFN-γ without killing, CD16 gives killing without IFN-γ
  (Rajagopalan 2001).
- "NK responds faster than T" is **wrong**. In MCMV, NK proliferation is
  *neither earlier nor faster* than CD4/CD8, and contraction is monophasic
  with no memory signature (Schlub 2011,
  [DOI](https://doi.org/10.4049/jimmunol.1100416)). The innate advantage is
  zero lead time, not a faster ramp — do not narrate it as speed.
- In the gut the honest cell is an **ILC1**, not a classical NK cell:
  human intestinal epithelium is ILC1-dominant, and ILC1s lack perforin
  and granzyme B (Jaeger 2024). The tower is named "NK cell (ILC1)" by the
  Director's ruling — the familiar name leads, the correct one follows.

## The paper the combat model is built on

Halle 2016, *Immunity*
([DOI](https://doi.org/10.1016/j.immuni.2016.01.010)) — two-photon imaging
of CTLs killing virus-infected cells in live mice. Three results, all of
which became mechanics:

1. **One CTL kills 2–16 infected cells per day.** Our pre-Sprint-23
   macrophage cleared an infected cell in a mean 4.0 s of contact despite
   having no MHC-I-restricted recognition at all — roughly 800× faster
   than a real CTL. §4b had always *said* innate immunity is bad at
   intracellular infection; the number never implemented the sentence.
   Sprint 23 cut innate recognition tenfold and set cytotoxic recognition
   ~10× above it.
2. **Under viral MHC-I downmodulation, CTLs failed entirely** — per-capita
   kill rate to exactly 0.0, contact frequency from 38% to 1%. The CTL's
   weakness is a **cliff, not a slope**. Consequence for design: there
   should be no graded "stealth" stat and no "reduce MHC-I by 30%"
   upgrade, because in reality partial reduction does nothing.
3. **Killing is cooperative.** p(target death) = 0.01 / 0.15 / 0.28 for
   0 / 1 / 2 CTL contacts, significantly superadditive at 3 and 5; median
   3.5 contacts and ~50 min cumulative contact per kill; 40% of contacting
   CTLs deliver nothing. This is why recognition is now a **cumulative
   meter with a cooperation multiplier** rather than a per-tick coin flip.

## Speed ladder — measured, then compressed

Normalised against the tissue macrophage at 1. Measured two-photon
velocities; the game column is integer fine-tiles-per-tick.

| Cell | Measured (µm/min) | Ratio | Game | Note |
|---|---|---|---|---|
| Macrophage (tissue) | ~1 | 1 | 1 | van den Bos 2020 |
| Neutrophil | 7.3 interstitial | 7–10 | **5** | Zinselmeyer 2008. Was 3; the game showed a 3× gap where reality is 7–10×. |
| Dendritic cell | 2–3 | 2–3 | 3 | Miller 2004. Research says 2; **left at 3 deliberately** — Sprint 14 raised it so the patrol oscillation reads inside a round. A legibility decision beats a ratio this close. |
| Helper T | 6.2–11 | 6–11 | 2 | Pre-existing. |
| Cytotoxic T | 6.5 (gut villi) | 6.5 | 3 | Torres 2023, [DOI](https://doi.org/10.7554/eLife.84916) |
| NK / ILC1 | 6–7 | 6–7 | 3 | Garrod 2007 |
| B cell | ~0.4–0.6× T | ~3 | 2 | Motility coefficient 5–6× below T cells (Miller 2002) |
| γδ IEL | **UNVERIFIED** | — | **1** | Deliberately against the ratio: it is tissue-*resident*, already at the wall, so search speed is beside the point. |
| Plasma cell | largely niche-bound | <1 | **0** | Motility is inversely related to secretion capacity (Rakhymzhan 2024) |

**The gut is not a slow compartment.** Torres 2023 measured naive T cells
in lymph node (6.2 µm/min) and effector CD8 in mouse small-intestinal
villi (6.5) with one pipeline — statistically similar, and villi patrol
the same volume per second as the node. Any "sluggish out in the tissue"
difficulty lever must come from geometry or gradient availability, never
from a speed penalty.

**Resolution warning.** Three labs report naive T in mouse lymph node as
6.2, 11 and 18 µm/min. Any ratio finer than ~1.5× is below the
literature's own noise floor, so the integer ladder is not the binding
constraint it looks like. The research recommends spending future
resolution on **directional persistence** (meandering ratio: LN 0.70, gut
villi 0.63, inflamed lung 0.22–0.37) rather than on more speed tiers.

## Things we chose not to build, and why that was a choice

- **γδ IELs came back as the strongest unit in the whole brief** —
  and not as a killer. They converge on the enterocyte *currently* touched
  by bacteria; migration-defective γδ cells are as useless as none at all;
  more intercalation cut invasion by **63%** (Edelblum 2015). They also
  hold tight junctions together (Dalton 2006) and make repair factors. The
  tower is built as patrol + repair for that reason.
- **ILC3 is the best-evidenced innate memory in the gut** — dispensable at
  low pathogen load, *indispensable* at high load (Araujo 2024), and
  genuinely trained for months after one encounter (Serafini 2022). A
  better "innate memory" candidate than NK. Not built; queued.
- **Treg repair is separable from Treg suppression**, and is triggered by
  IL-18/IL-33 alarmins rather than TCR (Arpaia 2015) — i.e. a tower
  powered by *your own loud kills*, which is a natural fibrosis
  counterweight. Not built; queued.
- **pDCs are dispensable for resistance in most viral infections**
  (Ngo 2024). Type I interferon should be a tissue-wide state, not a
  tower.
- **The stress sensor is also the autoimmunity cell.** ER-stressed
  epithelium up-regulates NKG2D ligands and intraepithelial group-1 ILCs
  kill it, causing spontaneous enteritis (Hosomi 2017). A sourced drawback
  for stress-sensor towers: they should cost fibrosis for killing
  stressed-but-uninfected cells. Not built; queued.

## Tertiary lymphoid organs — the Director's B-cell upgrade

Researched because the Director proposed B cells that deploy into tissue
to build a local lymphoid organ. **The mechanic is real and B cells are
genuinely the construction crew**: isolated-lymphoid-follicle formation
requires lymphotoxin-sufficient *B lymphocytes* (Lorenz 2003), and where
LTi cells are absent, B cells are the critical source (Lochner 2011).

One correction, which improves the mechanic: **the site is programmed,
only the maturation is induced.** Cryptopatches form postnatally at crypt
bases even in germ-free mice. So the player should not place a follicle
anywhere — they should *mature a pre-seeded one*. `GutWallFeatures`
already distributes crypts along the wall on a fixed sequence, and that
sequence is the cryptopatch grid.

Three findings that make it a real decision rather than a buff:

1. **The stacking curve is measured.** In Peyer's-patch-deficient mice,
   mature ILFs multiply until their total follicular-dendritic-cell area
   equals that of all Peyer's patches combined, and faecal IgA returns to
   normal (Glaysher 2007). One follicle ≈ one PP *follicle*: weak alone,
   patch-equivalent at ~4–6.
2. **The drawback is mechanical, not flavour.** Gut TLOs form at the
   valves of collecting lymphatic vessels and *obstruct outflow toward the
   mesenteric lymph node*; early intervention restores flow, chronic TLOs
   resist regression permanently (Czepielewski 2021). **The follicle built
   to bypass the shuttle physically blocks it** — removable while young,
   permanent once mature. That is the drawback the upgrade should carry.
3. **Quality penalty, cited.** Responses routed through ILFs instead of
   Peyer's patches gave IgA with delayed somatic mutation and limited
   target specificity (Lécuyer 2014). So a follicle buys *broad knowledge
   fast*; only the DC shuttle yields species-specific high-affinity
   antibody. It competes with the shuttle rather than replacing it.

Timings are consistent across models: innate influx day 3 → T cells day 12
→ organised aggregate ~day 14 → germinal-centre peak day 19. GCs wax and
wane on ~3-week cycles even with antigen present, and depleting the DCs
inside an ILF makes it regress — so the structure should **decay back to a
cryptopatch**, not persist.

## Two unresolved questions this research opened

1. **What is one round, in biological time?** The two passes that answered
   it disagree: the proliferation pass recommends **1 round = 1 day** (CD8
   peaks round 8, enterocytes turn over every 3–4 rounds, IgA arrives
   rounds 14–24); the specificity pass recommends **1 round = 2 days**
   (antigen → first division is 32–36 h, so this round's antigen makes
   next round's cells). Both are defensible and the choice sets the whole
   campaign's length. **Director decision needed** — it belongs with round
   design.
2. **The 8-bit barcode is the binding constraint, not the Hamming
   threshold.** `GAME_DESIGN.md` §5c says to reach for the threshold
   before barcode length; the research says that is backwards. Exact match
   on 8 bits is still ~780× too permissive, and 8 bits ceilings the
   species roster at ~8–10 before tags start teaching two species at once.
   That ceiling matters *now*, because the next sprint adds pathogens.
   16 bits at r≤2 (~1 in 480) is the recommended compromise.

Notably, **Hamming distance itself is better justified than the design doc
claims**: one TCR covers ~10⁶ peptides (Mason 1998), and structural work
shows a conserved "hotspot" with free positions around it — a weighted
Hamming rule. The metric is right; only the radius is wrong.

## Full research passes

The six unabridged passes live in `docs/research/` (3,928 lines, ~170
resolved citations). This file is the distillation; those are the working
notes, kept because re-deriving them costs six agent-hours and because
several carry numbers we did **not** use, with the reasoning for why.

| File | Covers |
|---|---|
| `research_killing.md` | Phagocytosis, neutrophil capacity, CTL/NK serial killing, complement, pathogen replication rates |
| `research_motility.md` | Two-photon velocities, search strategy, chemotaxis, contact durations, cell sizes |
| `research_nk_unconventional.md` | NK vs ILC1, missing-self, ADCC, γδ IELs, ILC3, and a verdict on every other candidate cell |
| `research_proliferation.md` | Clonal expansion, GC cycling, antibody output, emergency granulopoiesis, memory recall |
| `research_specificity.md` | Precursor frequency, repertoire size, cross-reactivity, affinity maturation, antigenic drift |
| `research_tertiary_lymphoid.md` | Isolated lymphoid follicles, TLO formation and pathology, gut IgA class switching |

**They are research inputs, not design.** Where one of them disagrees with
`GAME_DESIGN.md`, the design wins — the design is where the Director's
decisions live, and several of those knowingly depart from the biology.

---
*Consensus search results informed parts of this reference. Consensus asks
that the following be reproduced verbatim: "Create or connect a free
Consensus account to return more than 3 results per search in Claude
Code.: https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code"*
