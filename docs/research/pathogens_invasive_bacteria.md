# Invasive & Intracellular Gut Bacteria — Research Shortlist for Immunology Tower Defense

Status: **research reference, 2026-10-25.** Written for the pathogen-roster
sprint. Nothing here is a design decision — it is the biology the design
should be checked against. Every literature claim was reached through the
PubMed and Consensus MCP tools in this session; **according to PubMed** and
the DOIs/links given inline. No repo file was edited.

Conventions used throughout:

- **in vitro** / **in vivo** is marked on every replication rate. Where only
  a cell-culture number exists, it says so — the gap is often 10–100×.
- **UNVERIFIED** means I could not find a defensible published number in this
  session, not that one does not exist.
- "Mouse caveat" boxes flag where the animal literature actively misleads
  about human disease. This is acute for *S.* Typhi, *Shigella* and *Listeria*.

---

## 1. The shortlist, ranked

Ranked by **what each brings mechanically that none of the others does**, not
by clinical importance. Five would be a lean roster; seven is the ceiling.

| # | Organism | The ONE mechanic it brings that nothing else does | Why it earns a slot |
|---|---|---|---|
| **1** | ***Shigella flexneri* / *sonnei*** | **Spreads cell-to-cell through the wall and never comes back out.** Escapes the vacuole into raw cytosol, grows an actin tail, and pushes directly into the next enterocyte. There is no extracellular phase to shoot at, ever. | Turns §4b's "innate is bad at this" from a dice-roll into a *spreading* problem. Also the lowest infectious dose of any bacterium in the game (10–100 cells), which is a difficulty dial nothing else can hit. Human-only, invades the wall from the **basolateral** side — which the Peyer's patch and the plaque-breach model already express. |
| **2** | ***Salmonella* Typhimurium** | **Two speeds in two different host cells.** Fast, explosive growth in the epithelial **cytosol**; slow-to-dormant growth in the macrophage **vacuole**. Same organism, opposite behaviour, depending on which tower or tile ate it. | The game already treats epithelial cells and macrophages differently; this is the only organism whose *replication rate* reads that difference. And its escape route — the host cell is **extruded into the lumen** with the brood inside — hands the brood back to the lumen/adhesion loop instead of into tissue. |
| **3** | ***Listeria monocytogenes*** | **A named door.** It does not breach anywhere; it enters at **goblet cells** and at sites where enterocytes are *already* being shed, because that is where its receptor (E-cadherin) is exposed. | A targetable, drawable weak point on the wall — the player can learn where the holes are. Also the fastest actin motility measured (~12 µm/min, 2.5× *Rickettsia*) and the canonical **CD8-cleared** pathogen, so it is the clean payoff for the adaptive ladder. |
| **4** | ***Yersinia enterocolitica*** | **It attacks the towers, not the wall.** Crosses M cells, then *refuses* to go intracellular: it sits in **extracellular microcolonies** and injects Yop effectors that shut down phagocyte superoxide, degranulation and inflammasome. | The only candidate that is an anti-tower unit rather than a hiding unit. Fills the "large extracellular bacterium" class with something that has a real mechanic, and gives the Peyer's patch a second, different consequence. |
| **5** | ***Campylobacter jejuni*** | **It opens the gate for everyone else.** Secretes HtrA protease, cleaves tight/adherens junctions, walks through **paracellularly**, then invades from the basolateral face — and in doing so lets ordinarily non-invasive bacteria through with it. | A support/enabler enemy: it raises the breach rate of the whole wave. Nothing else on the list damages the barrier *as its primary effect*. Bonus: phase-variable surface sugars that mimic host gangliosides — a legible "disguise" mechanic. |
| **6** | **Adherent-invasive *E. coli* (AIEC, LF82)** | **It turns your macrophage into an incubator.** Replicates inside a fully mature, acidified **phagolysosome** — the one place that is supposed to be lethal — and does it **without killing the macrophage**, which keeps patrolling. | The purest expression of "innate immunity made the problem worse." No other candidate leaves a functional, infected tower on the board. Weakest on recognisability and has no ID50/incubation, so it is a late addition, not a launch unit. |
| **7** | ***Mycobacterium avium* complex (MAC)** | **The stalemate.** In-vivo generation time of about **7 days** — against 3.4 h for the same organism in broth. It is not trying to win the round; it is trying to still be there in round 40. | The explicit reason to build an IFN-γ/Th1 rung. Punishes innate-only play by being unkillable rather than by being fast. Also the one that *defies* the M-cell expectation: it enters via plain **enterocytes**. Late-game / immunocompromised-scenario unit. |

**Cut, with reasons:**

- ***Salmonella* Typhi** — mechanically it is Typhimurium **plus a stealth
  coat** (Vi capsule → less complement deposition, less gut inflammation) and
  a 7–14 day fuse. That is an *elite variant* of unit #2, not a seventh unit.
  Shipping it as a Typhimurium skin with "low aggro, long timer, systemic
  payload" is the right call and costs almost nothing.
- ***S.* Paratyphi A / *Y. pseudotuberculosis*** — near-duplicates of #2/#4.
- ***Shigella sonnei* vs *flexneri*** — one unit, with *sonnei* as a tuning
  note (its double O-antigen layer means **less** uptake, **less** vacuole
  escape and **less** macrophage pyroptosis than *flexneri*; see §3.1).

---

## 2. The numbers table

All human numbers unless stated. Replication is marked **in vivo** or **in vitro**.

| Organism | Infectious dose (human) | Incubation | Doubling / generation time | Intracellular niche | Key citations |
|---|---|---|---|---|---|
| ***Shigella* spp.** | **10–100 organisms** — the lowest on this list. Challenge models use 1,500 CFU of *S. flexneri* 2a 2457T for ~60% attack rate | ~1–3 days; in CHIM, onset usually within 24–72 h (protocol treats at 120 h if undiagnosed). *A single pooled median incubation estimate was not found — treat the precise median as* **UNVERIFIED** | **41 min** in HeLa cytosol (**in vitro**); no in vivo figure found → **UNVERIFIED** | **Free cytosol**, after T3SS/IpaB-mediated vacuole rupture | [Hendrick 2025 *Lancet*](https://consensus.app/papers/details/62f408bd6852520b94481a6e2bfc243d/?utm_source=claude_code); [Kothary 2001](https://consensus.app/papers/details/196e67ba1b4f557a94c42efddd95e1fe/?utm_source=claude_code); [Porter 2012](https://consensus.app/papers/details/653c85572ee1525787e3039d166b34c6/?utm_source=claude_code); [MacLennan 2019](https://consensus.app/papers/details/732680f9156550c59ace6166e79171f7/?utm_source=claude_code); [Talaat 2021](https://consensus.app/papers/details/608ba7658a2250aaabbdc107b656bfdd/?utm_source=claude_code); [Yu 1998 *Infect Immun*](https://consensus.app/papers/details/50b60fda151c566e9aa1b450c841fce7/?utm_source=claude_code) |
| ***S.* Typhimurium (NTS)** | Challenge studies generally **>10⁵**; outbreak data (chocolate, cheese, high-fat vehicles) show illness at far lower doses. Pooled dose-response over 6 challenge studies + 44 outbreaks puts Typhimurium **less** infectious than Enteritidis | Median **45 h**; usual range **12–96 h**; >4–6 days "not unusual", 7–9 days occurs (725 outbreak cases, Minnesota 2000–2015) | **In vivo**, mouse spleen: division time **2.86 h** (susceptible BALB/c) vs **5.02 h** (resistant B10×A/J F1). Epithelial cytosol: hyper-replication; SCV: much slower; macrophage: heterogeneous, including a **non-growing persister** subset formed immediately on uptake | **Bimodal**: *Salmonella*-containing vacuole (SCV) **and** free cytosol in epithelium; vacuole only in macrophages/fibroblasts | [Teunis 2022 *Epidemics*](https://consensus.app/papers/details/c85511dab94053c88889552145ff5d50/?utm_source=claude_code); [Eikmeier 2018 *Epidemiol Infect*](https://consensus.app/papers/details/f8defce3d67f552dac0fc90647647b75/?utm_source=claude_code); [Hormaeche 1980 *Immunology*](https://consensus.app/papers/details/b86f5c1ca1b25210afbdf1c5444d2320/?utm_source=claude_code); [Helaine 2014 *Science*](https://consensus.app/papers/details/09e709f9f1c058b382762a5c95f8cf28/?utm_source=claude_code); [Luk 2022](https://consensus.app/papers/details/69d7bce3f39f5e73a84d31a9e9bb87d6/?utm_source=claude_code) |
| ***S.* Typhi** | **10³ CFU → 55% attack rate; 10⁴ CFU → 65%** in bicarbonate buffer (modern Oxford CHIM). Older milk-vehicle studies needed 10⁵–10⁹ | Mean onset **8.4 d** at 10³ CFU, **6.3 d** at 10⁴ CFU (range 4–13 d); within-host model estimates **9.6 d**; classic clinical teaching 7–14 d | **UNVERIFIED** in humans. Mouse surrogates are not *S.* Typhi (see caveat) | SCV in macrophages; **Vi capsule** damps the host response | [Waddington 2014 *CID*](https://consensus.app/papers/details/7ad6bccf324c5c6ab8810f4cad5aa76b/?utm_source=claude_code); [Waddington 2012](https://consensus.app/papers/details/9e9a76c409ac5e31b05162087dfc431f/?utm_source=claude_code); [Hornick 1970 *NEJM*](https://consensus.app/papers/details/aadf3062c1a35335921b7863f4437761/?utm_source=claude_code); [Awofisayo-Okuyelu 2019](https://consensus.app/papers/details/497c0909f5fe5e2abb03c3f9d234c169/?utm_source=claude_code) |
| ***L. monocytogenes*** | **No human challenge studies** (ethics). Dose-response modelling says most invasive cases follow ingestion of **medium-to-high** contamination; host comorbidity matters more than strain virulence (RR ~22–24 between "less" and "more" virulent strains, vs much larger comorbidity effects) | **Strongly form-dependent.** Gastroenteritis: median **24 h** (6–240 h). Invasive: median **8 d** (1–67 d) — bacteraemia median **2 d**, CNS median **9 d**, pregnancy-associated median **27.5 d** (17–67 d). US simulation median **11 d**, 90% within 28 d | **<40 min** in J774/HEp-2 cytosol (**in vitro**); **~90 min** in Caco-2 enterocytes (**in vitro**). In vivo per-cell rate **UNVERIFIED**; recent imaging shows growth slows as a focus exhausts one cell's nutrients, and **spreading to a fresh cell is how it escapes that limit** | **Free cytosol**, after listeriolysin-O vacuole lysis | [Pouillot 2015](https://consensus.app/papers/details/0127ebfafeec566f9c16fc7dc9362202/?utm_source=claude_code); [Pouillot 2024 *Foods*](https://consensus.app/papers/details/ebeb7a7a348f523aace25f7d6e35ce1b/?utm_source=claude_code); [Goulet 2013 *BMC Infect Dis*](https://consensus.app/papers/details/f2b947e498f251b6b75ce4677b710fd4/?utm_source=claude_code); [Angelo 2016 *CID*](https://consensus.app/papers/details/cd25d5525b515dcd97f6f50a4f44d39b/?utm_source=claude_code); [Slaghuis 2004 *JID*](https://consensus.app/papers/details/e8080e121cf45139a7e824e376802d7a/?utm_source=claude_code); [Gaillard 1987 *Infect Immun*](https://consensus.app/papers/details/8f88c337513e57f297d25a2bd6f74531/?utm_source=claude_code); [Radhakrishnan 2025 bioRxiv](https://consensus.app/papers/details/9a86cf2732e15cb0b1c143315b18fe5b/?utm_source=claude_code) |
| ***Y. enterocolitica*** | **UNVERIFIED** — no modern human challenge study found. Commonly quoted as high (~10⁸–10⁹) but I could not source that in this session | **4–6 days** typically; range **1–2 weeks**. Illness lasts 1–3 weeks; stool shedding up to **40 days** after symptoms resolve | **Extracellular** in tissue, so "intracellular doubling time" is the wrong question. In vivo rate **UNVERIFIED** | **None — extracellular microcolonies** in Peyer's patches and mesenteric nodes, surrounded by pyogranulomas | [Quion 2021 *Pediatr Rev*](https://consensus.app/papers/details/8073fa03eac45f5aaaea7b548334ff3c/?utm_source=claude_code); [Bliska 2021 *EcoSal Plus*, PMID 34910573, [DOI](https://doi.org/10.1128/ecosalplus.ESP-0014-2021)]; [Autenrieth 1996 *J Med Microbiol*](https://consensus.app/papers/details/57269fbcc59052b58e819ff28b2a8640/?utm_source=claude_code) |
| ***C. jejuni*** | Doses from **8×10² to 2×10⁹** all caused illness in 111 volunteers; **infection** rate rose with dose, **illness** did not show a clean dose relation. Later modelling: **very high susceptibility to infection at low doses**, with a separate conditional dose-response for *symptoms* | **1–3 days** typically (up to ~7) | **Survives but barely replicates** intracellularly; persists ~72 h in the cell. No credible in vivo intracellular doubling time → **UNVERIFIED** | ***Campylobacter*-containing vacuole (CCV)** — diverts off the endocytic path in epithelium and **avoids lysosomes**; in macrophages it is delivered to lysosomes and **rapidly killed** | [Black 1988 *JID*](https://consensus.app/papers/details/9908c434f9295758802fddf94396676c/?utm_source=claude_code); [Teunis 2018 *Epidemics*](https://consensus.app/papers/details/51938ef6489352d8b347b2e90a71029d/?utm_source=claude_code); [Watson 2008 *PLoS Pathog*](https://consensus.app/papers/details/63586a28d9d6541b93c39603952606bd/?utm_source=claude_code) |
| **AIEC (LF82)** | **N/A** — a pathobiont, not a defined-dose pathogen. No ID50 exists | **N/A** — chronic colonisation, not an acute incubation | **2.2- to 74.2-fold increase over 48 h** inside J774 macrophages across 15 CD isolates (**in vitro**). In vivo **UNVERIFIED** | **Mature, acidified, cathepsin-D-positive phagolysosome** — and acidity is *required* for its replication. Forms intracellular bacterial communities in a biofilm-like matrix | [Glasser 2001 *Infect Immun*](https://consensus.app/papers/details/c6c0d1f9040f5568ace3473938613ab4/?utm_source=claude_code); [Bringer 2006 *Cell Microbiol*](https://consensus.app/papers/details/0723560090895a78a614f022ac5ce63c/?utm_source=claude_code); [Bruder 2025 *Gut Microbes*](https://consensus.app/papers/details/a6eb901a80335606a4305573500548f9/?utm_source=claude_code) |
| **MAC (*M. avium*)** | **UNVERIFIED** — no human ID50. Disease in AIDS follows prolonged GI colonisation, not a single dose | Not an "incubation period": **months to years** of colonisation before dissemination | **Max in vitro doubling 3.4 h** (nitrogen-limited continuous culture) vs **~7-day generation time in vivo** in mouse mesenteric lymph nodes. **This 50× gap is the single most in-vivo-vs-in-vitro-divergent number on the page** | Phagosome held immature / remodelled; in human primary macrophages a fraction re-establishes in a LAMP1⁻/LAMP2⁻/CD63⁻ compartment where it also escapes TLR sensing | [McCarthy 1987 *Microbios*](https://consensus.app/papers/details/877a64ee780f5e8ab5d8490652e873f1/?utm_source=claude_code); [Gidon 2017 *PLoS Pathog*](https://consensus.app/papers/details/0ac6556fe4455f28b5eb22936d4dd57d/?utm_source=claude_code); [Kelley 2003 *MBoC*](https://consensus.app/papers/details/36fe76f523cf58209577bb5acbc8d5b2/?utm_source=claude_code); [Horsburgh 1999 *JID*](https://consensus.app/papers/details/0a5953c623065cbd9d999f4918b90fdd/?utm_source=claude_code) |

### Actin motility speeds (for the cell-to-cell spread mechanic)

| Organism | Speed | Conditions |
|---|---|---|
| *L. monocytogenes* | **12.0 ± 3.1 µm/min** (≈0.20 µm/s) | GFP-actin Vero cells, confocal time-lapse, **in vitro** ([Heinzen 1999 *Infect Immun*](https://consensus.app/papers/details/b31a198d183553a98542315538ce2613/?utm_source=claude_code)) |
| *S. flexneri* | **~0.09–0.3 µm/s** depending on cell and perturbation; roughly the same order as *Listeria*. In *Xenopus* extract, IcsA-expressing *E. coli* moved **~2× faster** than *Listeria* with longer tails | microinjection studies, **in vitro** ([Zeile 1996 *JCB*](https://consensus.app/papers/details/870817541f4f5a9cab153f87d9b9911c/?utm_source=claude_code); [Kocks 1995 *Mol Microbiol*](https://consensus.app/papers/details/5ac77aec532451f29febfd786e938bac/?utm_source=claude_code)) |
| *Rickettsia rickettsii* (reference) | 4.8 ± 0.6 µm/min | 2.5× slower than *Listeria*, same study |

**For the game:** treat *Listeria* and *Shigella* as the same speed band
(~0.1–0.3 µm/s, i.e. a bacterial body-length every second or two) and
differentiate them on *where they enter*, not how fast they crawl. All of
these are in-vitro numbers; no in vivo tail-speed measurement was found.

---

## 3. Per-organism detail

### 3.1 *Shigella flexneri* / *sonnei* — the one that never comes back out

**Route of entry.** Classically, **M cells of the follicle-associated
epithelium** are the major portal; rabbit ligated-loop work showed invasive
*S. flexneri* colonising the FAE and destroying it within 8 h
([Sansonetti 1996](https://consensus.app/papers/details/365610f275185c90b8486c95cca860a6/?utm_source=claude_code),
[Sansonetti 1999](https://consensus.app/papers/details/ab63b1f932b054c886c052e2536f0adc/?utm_source=claude_code)).
The modern human-enteroid numbers are the ones worth encoding:

- **Apical invasion is very limited.**
- Differentiating the enteroid to contain **M cells raises apical invasion ~10×**.
- **Basolateral invasion is ≥2 log₁₀ (≥100×) more efficient than apical.**
  ([Ranganathan 2019 *Infect Immun*](https://consensus.app/papers/details/8e3b16206106558cba02d4b523e88e3b/?utm_source=claude_code))

So *Shigella* is, quantitatively, a **back-door** pathogen: it needs the
Peyer's patch (or an already-broken barrier) to get behind the wall, and only
then does it invade efficiently. That maps exactly onto the game's plaque →
breach → tissue sequence.

A 2020 study complicates the textbook story usefully: rather than being
politely transcytosed to the immune compartment, *S. flexneri* inside M cells
shows a **bimodal lifestyle** — rapid transcytosis, *or* delayed vacuole
rupture followed by **direct actin-based propagation into neighbouring
enterocytes**. The authors argue M-cell-to-enterocyte spread is the *major*
dissemination route and lets the bacterium **avoid the basolateral immune
compartment altogether** — a proposed mechanism for why shigellosis elicits
such poor protective immunity
([Rey 2020 *PLoS Pathog*](https://consensus.app/papers/details/db19b39d004b5391b9ed85bf4bc2bb5e/?utm_source=claude_code)).
*Listeria* does the same thing in that system.

**Niche and replication.** Escapes the vacuole into **free cytosol**, where it
doubles every **41 min** in HeLa cells (in vitro; the comparison strain in
that paper, a *dsbA* mutant, barely grew at all)
([Yu 1998](https://consensus.app/papers/details/50b60fda151c566e9aa1b450c841fce7/?utm_source=claude_code)).
Vacuole lysis and rapid cytoplasmic growth are plasmid-encoded and tightly
coupled ([Sansonetti 1986](https://consensus.app/papers/details/9f8c896a552552b7a18c921e41d4795f/?utm_source=claude_code)).

**Cell-to-cell spread.** IcsA/VirG polarised at one pole → Arp2/3-dependent
actin comet → protrusion into the neighbour → double-membrane vacuole lysed →
repeat. **No extracellular phase required.**

**Phagocyte subversion — it kills the macrophage rather than living in it.**
IpaB oligomerises into cation-selective channels in membranes, lets K⁺ into
the endolysosome, destabilises it, and triggers caspase-1 — **pyroptosis**
([Šenerović 2012 *Cell Death Dis*](https://consensus.app/papers/details/e71cc516285f52d18677d33f12f970ee/?utm_source=claude_code)).
Caspase-1 activation runs through **Ipaf/NLRC4 + ASC** and, unlike
*Salmonella*, is **flagellin-independent**; autophagy partially protects the
macrophage ([Suzuki 2007 *PLoS Pathog*](https://consensus.app/papers/details/d5f5897d53985a66af9fa8376dde70fb/?utm_source=claude_code)).
**Design note:** a macrophage that eats *Shigella* should die loudly — that
is a DAMP event, not a kill.

> ***flexneri* vs *sonnei* is a real tuning axis.** *S. sonnei* carries its
> O-antigen in both LPS **and** a capsule; that extra coat means **less
> uptake, less vacuole escape, fewer cytosolic bacteria and substantially
> less pyroptosis** in human macrophages — it behaves more extracellularly
> ([Watson 2019 *mBio*](https://consensus.app/papers/details/42d2825691d856da8ab726cd38f02481/?utm_source=claude_code)).
> If two *Shigella* variants are ever wanted: *flexneri* = the cytosolic
> spreader, *sonnei* = the armoured, quieter one.

**Immune evasion relevant to the ladder.** Early in infection *Shigella*
**switches off the host's own antimicrobial peptides** — LL-37 and human
β-defensin-1 expression is reduced or abolished in biopsies from patients
with bacillary dysentery and in infected epithelial/monocyte cultures;
*Shigella* plasmid DNA is implicated as a mediator
(Islam et al. 2001 *Nat Med* 7:180–5, PMID 11175848,
[DOI](https://doi.org/10.1038/84627)). That is a **debuff on the wall's
innate defence**, applied before the breach — a genuinely novel mechanic and
exactly the kind of thing a player can be shown.

**How it is actually cleared — and where a player would guess wrong.**
Not by CD8 killing. Protection is **antibody-driven and serotype-specific**:
in an endemic cohort, **O-specific-polysaccharide-specific, FcαR-binding IgA**
was associated with resistance, and depleting IgA removed the
neutrophil-activating function of resistant serum
([Bernshtein 2023](https://consensus.app/papers/details/00f9f38b06445f39b806fbfb6f07ace0/?utm_source=claude_code)).
In the CHIM, baseline **LPS-specific serum IgA and IgA memory B cells**
predicted reduced risk
([Clarkson 2020 *mSphere*](https://consensus.app/papers/details/7c999626c7cd5e4db63277b0c147b692/?utm_source=claude_code));
LPS-specific IgG after bioconjugate vaccination associated with protection
([Clarkson 2021](https://consensus.app/papers/details/ebded593b2e250668568d4eb9bba966c/?utm_source=claude_code),
[Talaat 2021](https://consensus.app/papers/details/608ba7658a2250aaabbdc107b656bfdd/?utm_source=claude_code)).
The effector arm is **neutrophils**, licensed by IgA.

> **Mouse caveat — severe.** Wild-type mice are **fully resistant** to oral
> *Shigella*. Susceptibility is restored only by deleting **NAIP–NLRC4**, and
> the protective activity sits specifically in **intestinal epithelial
> cells** — the authors propose that humans are susceptible precisely because
> *our* IECs lack that inflammasome response
> ([Mitchell 2020 *eLife*](https://consensus.app/papers/details/e58ccc7fbda9540da28e9d78e760c0ba/?utm_source=claude_code);
> review: [Alphonse 2023](https://consensus.app/papers/details/7558d4d15a1f5bada1b59fc81d1e4c0c/?utm_source=claude_code)).
> **Do not take any pre-2020 mouse statement about shigellosis immunity at
> face value.** In the new model, protection is **CD4-dependent, not
> CD8-dependent**, and cross-protective between serotypes — preliminary
> ([Babirye 2025 *J Immunol* abstract](https://consensus.app/papers/details/6a87c38a808a5380b6cefd9bd14a9670/?utm_source=claude_code)).

---

### 3.2 *Salmonella* Typhimurium — two speeds, two niches

**Route of entry.** M cells over Peyer's patches *and* direct enterocyte
invasion, and the two are mechanistically different:

- **M-cell translocation is fast and, in an in vitro M-cell model,
  independent of both SPI-1 and SPI-2** — mutants lacking multiple SPI-1
  effectors crossed as well as wild type
  ([Martinez-Argudo 2008 *Microbiology*](https://consensus.app/papers/details/e578d1fb690a54a497b248a421d06c1f/?utm_source=claude_code)).
- **Enterocyte invasion in the living mouse gut is NOT the textbook "ruffle".**
  In vivo it is **"discreet invasion"**: modest local microvillar elongation
  driven mainly by **SipA**, preferentially at **apicolateral hot spots at
  cell–cell junctions**, and — importantly for the game —
  **it does not favour cooperative invasion**
  ([Fattinger 2020 *PLoS Pathog*](https://consensus.app/papers/details/cca42f5437f05fb5b7db8145fabdfa76/?utm_source=claude_code);
  [Fattinger 2021](https://consensus.app/papers/details/46e55a4855f65f828c2bba534e6989fd/?utm_source=claude_code)).
  The giant membrane ruffle everyone draws is a **cell-line artefact**. If the
  art team was going to animate a huge ruffle, that is the correction.

**Niche and replication — the mechanic.**

| Where | Behaviour |
|---|---|
| **Epithelial cell, SCV** | Slow, controlled replication |
| **Epithelial cell, cytosol** | **Hyper-replication.** Occurs in <20% of infected cells but accounts for the **majority of net intracellular growth** — population CFU assays therefore *mask* vacuolar defects entirely. Cytosolic escape needs **SPI-1, not SPI-2** |
| **Macrophage, vacuole** | Slow, SPI-2-dependent, heterogeneous; a subset becomes **non-replicating persisters immediately on uptake**, induced by vacuolar acidification and starvation |
| **Fibroblast** | Slower still / dormant |

Citations: [Malik-Kale 2012 *PLoS ONE*](https://consensus.app/papers/details/48edb556f43f59e5b2280de2d2fef981/?utm_source=claude_code);
[Knodler 2014 *PLoS ONE*](https://consensus.app/papers/details/6dd9212715465b6697554ed234ab8fbd/?utm_source=claude_code);
[Knodler 2015 *Curr Opin Microbiol*](https://consensus.app/papers/details/b09bd20150335758bfe413ba4d58861f/?utm_source=claude_code);
[Luk 2022](https://consensus.app/papers/details/69d7bce3f39f5e73a84d31a9e9bb87d6/?utm_source=claude_code);
[Salcedo 2001 *Cell Microbiol*](https://consensus.app/papers/details/e7f19ffd36f355a2940fa331e689df02/?utm_source=claude_code).

In **human** monocyte-derived macrophages the outcome depends on macrophage
polarisation: **M1 macrophages do not permit replication; M2a and M-CSF-only
(M0) macrophages do**
([Lathrop 2015 *Infect Immun*](https://consensus.app/papers/details/c059b86aac745e5e8a0d8dd1ff260bba/?utm_source=claude_code)).
That is a direct, defensible in-game rule: *an activated macrophage contains
it; a resting one feeds it.*

In vivo, phenotypic heterogeneity is extensive: fluorescent growth-rate
reporters in mouse tissue show slow-growing subsets alongside **well-nourished
fast-growing subsets that drive disease progression**
([Claudi 2014 *Cell*](https://consensus.app/papers/details/4215650e005c5abb892545654bce46f9/?utm_source=claude_code)).
Persisters are **not dormant** — they stay metabolically active and use SPI-2
effectors to **reprogram the macrophage toward an anti-inflammatory
phenotype** ([Stapels 2018 *Science*](https://consensus.app/papers/details/91f7a837ea8e500b8054fd1079cc2e52/?utm_source=claude_code)),
and are locked in growth arrest by host **reactive nitrogen species**, resuming
only when RNS production subsides
([Ronneau 2023 *Cell Host Microbe*](https://consensus.app/papers/details/beb81d78efb15fd2a2aae2e5e1607c5b/?utm_source=claude_code)).
**That is a tower-pressure mechanic**: the infection pauses while you keep the
heat on and restarts when you let up.

**Cell-to-cell spread — it goes back to the lumen, not sideways.** Cytosolic
hyper-replicating *Salmonella* are transcriptionally distinct, **induce SPI-1
and grow flagella — they are primed to invade again** — and the laden
epithelial cell is **extruded into the lumen**, releasing invasion-competent
bacteria, with caspase-1 activation and apical IL-18 release
([Knodler 2010 *PNAS*, PMID 20876119, [DOI](https://doi.org/10.1073/pnas.1006098107)]).
Blocking cytosolic replication specifically (an engineered self-destruct
strain) **impairs intestinal expansion and faecal shedding**
([Chong 2021 *Cell Host Microbe*](https://consensus.app/papers/details/770a99b1619150aa9e8b99aed678a27a/?utm_source=claude_code)).

**This is the single best-fitting mechanic for the existing design.** The
brood does not spread to the adjacent tile like the virus does — it is
**spat back into the lumen**, where the game's adhesion/mill/plaque loop
already handles it.

**How it is cleared — and the player-surprise.** The host's *first* answer is
not a tower at all: the **epithelium-intrinsic NAIP/NLRC4 inflammasome expels
the infected enterocyte**. Deleting it (globally or epithelium-specifically)
gives **~100× higher intraepithelial loads** and faster lymph node
colonisation — and the downstream cytokines IL-1α/β and IL-18 appear
**dispensable** for that early restriction
([Sellin 2014 *Cell Host Microbe*](https://consensus.app/papers/details/b174d0f9d0955638b52940284f9d2106/?utm_source=claude_code);
[Rauch 2017 *Immunity*](https://consensus.app/papers/details/f9798d3054fb5f719821713e376413a2/?utm_source=claude_code)).
And it is a **tradeoff, not a free win**: without epithelial NAIP/NLRC4, a
**TNF-driven inflammatory hyper-reaction destroys the barrier** within 72 h
([Fattinger 2021 *Mucosal Immunol*](https://consensus.app/papers/details/28d3cdb580925929a85291f9c10568a0/?utm_source=claude_code)).
The wall shedding its own infected cells is therefore *the* protective
mechanism and *also* the source of enteropathy — which is precisely the kind
of two-edged mechanic the Director likes.

Adaptively, clearance is **CD4 Th1 + IFN-γ, not CD8**: MHC-II-deficient,
TCR-β-deficient and IFN-γR-deficient mice are highly susceptible, while
**β2m-deficient (no conventional CD8) and TCR-δ-deficient mice are as
resistant as controls** — and forcing *Salmonella* into the cytosol with
listeriolysin did **not** create a CD8 dependency
([Hess 1996 *J Immunol*](https://consensus.app/papers/details/12e60c4011f0524fbcb750e08cf82b3b/?utm_source=claude_code)).
A CD8 contribution does appear in some MHC haplotypes
([Labuda 2019](https://consensus.app/papers/details/0a76c295cc6454358036085a45f4dfc0/?utm_source=claude_code)),
but the dominant axis is **IFN-γ-activated macrophages**.

> **Player-intuition mismatch, worth building around:** most players will
> assume "kill the infected cell with a cytotoxic T cell." For *Salmonella*
> the real answers are (a) the **epithelium ejecting itself**, and (b) a
> **cytokine that upgrades your macrophages**, not a new killer unit.

> **Mouse caveat.** The standard "mouse typhoid" model is *S.* Typhimurium in
> Nramp1/Slc11a1-susceptible mice — Hormaeche's division-time difference
> (2.86 h vs 5.02 h) *is* that gene acting early, probably inside macrophages
> ([Hormaeche 1980](https://consensus.app/papers/details/b86f5c1ca1b25210afbdf1c5444d2320/?utm_source=claude_code)).
> The colitis model additionally needs streptomycin pretreatment. Neither is
> human gastroenteritis.

---

### 3.3 *Salmonella* Typhi — a Vi-coated Typhimurium with a long fuse (variant, not a unit)

Kept here because the design doc may want the *stealth* idea even if Typhi is
not its own enemy type.

- **Vi capsule = anti-inflammatory armour.** Introducing the *viaB* locus
  into *S.* Typhimurium **reduced complement deposition in vitro and
  intestinal inflammation in vivo**, and the effect was **markedly stronger**
  once very-long O-antigen chains were removed — *S.* Typhi has inactivated
  *fepE* for exactly that reason, so the capsule can do its job
  ([Crawford 2013 *mBio*, PMID 23860765, [DOI](https://doi.org/10.1128/mBio.00232-13)]).
  Mechanic: **a pathogen that generates less aggro** — fewer recruitment
  cytokines, so your chemotaxis ladder finds it later.
- **Typhoid toxin** contributes to symptoms; a toxin-null mutant actually
  **replicated more** in humanised mice, suggesting a role in establishing
  *persistent* rather than florid infection
  ([Song 2010 *Cell Host Microbe*](https://consensus.app/papers/details/65264efc781852fabc738e7cb19f5360/?utm_source=claude_code)).
- Typhoidal and non-typhoidal serovars share SPI-1/SPI-2 but **~half of
  Typhimurium's effectors are absent or pseudogenes in Typhi**, and T3SS
  regulation differs in response to bile, oxygen and fever temperature
  (Johnson, Mylona & Frankel 2018 *Cell Microbiol* 20:e12939, PMID 30030897,
  [DOI](https://doi.org/10.1111/cmi.12939)).
- Notably, **no anti-Vi antibody response was detected** in challenged
  volunteers who developed typhoid — they made anti-flagellin and anti-LPS
  responses instead ([Waddington 2014](https://consensus.app/papers/details/7ad6bccf324c5c6ab8810f4cad5aa76b/?utm_source=claude_code)).
  The armour is also **poorly immunogenic during natural infection**.
- Prior exposure gives only **partial** protection on re-challenge (44% vs 63%
  attack rate, not statistically significant), and there is **no
  cross-protection between Typhi and Paratyphi A**
  ([Gibani 2020 *PLoS NTD*](https://consensus.app/papers/details/b5f8005c86ba52a793f104cb16530500/?utm_source=claude_code)).

> **Mouse caveat — severe.** *S.* Typhi is **strictly human-restricted**.
> There is no normal mouse model; options are humanised (CD34⁺ Hu-NSG /
> Rag2⁻ᐟ⁻γc⁻ᐟ⁻ + human HSPC) mice
> ([Song 2010](https://consensus.app/papers/details/65264efc781852fabc738e7cb19f5360/?utm_source=claude_code);
> [Stepien 2022](https://consensus.app/papers/details/e395bc7c8b7b586aa8ee25ce55e8d8d9/?utm_source=claude_code)),
> or a **surrogate serovar** — *S.* Paratyphi C expresses Vi, colonises mice
> for 28 days and triggers fever after a **3-day** incubation, versus
> Typhimurium killing C57BL/6 mice "within a few days"
> ([Pham 2026 *mBio*](https://consensus.app/papers/details/cd048c1947e15217810243777078afec/?utm_source=claude_code)).
> Any "typhoid" claim sourced from a Typhimurium mouse study is about an
> organism with **no Vi capsule and no typhoid toxin**.

---

### 3.4 *Listeria monocytogenes* — the named door

**Route of entry — the most *drawable* of all of them.** InlA binds
**E-cadherin**, which normally sits *below* the tight junction and should be
luminally unreachable. *Listeria* solves this by going where the wall is
already open:

- around **mucus-expelling goblet cells**,
- around **extruding enterocytes at villus tips and lateral sides**,
- in **villus epithelial folds**.

It preferentially adheres to accessible E-cadherin on goblet cells, is
internalised, **rapidly transcytosed**, and released into the lamina propria
by exocytosis ([Nikitas 2011 *J Exp Med*](https://consensus.app/papers/details/73852b09d3bc55cf8202e5db12298524/?utm_source=claude_code)).
Live imaging in organoids shows it **hijacks the E-cadherin recycling
pathway** (InlA- and microtubule-dependent, Rab11-dependent) to ride through
the goblet cell in a vacuole
([Kim 2020 *Curr Biol*](https://consensus.app/papers/details/cdab8346954556f88bcf425f28049a3a/?utm_source=claude_code)).
It also uses **M cells**, and a third route via **LAP–Hsp60**, which actively
**opens cell junctions** and may be how InlA reaches E-cadherin in the first
place ([Drolia & Bhunia 2019 *Trends Microbiol*](https://consensus.app/papers/details/65806abd10485d3bb42ee175696294d5/?utm_source=claude_code)).

**Design gift:** the game already draws goblet cells and already has
"sentinel sloughing" / barrier shedding from Sprint 22. *Listeria* enters
**exactly at the tiles the player is already watching shed.** That is a
mechanic and a tutorial in one.

**Niche and replication.** Listeriolysin-O lyses the vacuole; the bacterium
grows in **free cytosol** with a generation time **<40 min** in J774
macrophages and HEp-2 cells, and **~90 min** in Caco-2 enterocytes (both
**in vitro**; [Slaghuis 2004](https://consensus.app/papers/details/e8080e121cf45139a7e824e376802d7a/?utm_source=claude_code),
[Gaillard 1987](https://consensus.app/papers/details/8f88c337513e57f297d25a2bd6f74531/?utm_source=claude_code)).
Within 24 h, **>90% of J774 macrophages infected with a non-spreading
*actA* mutant were dead** — so unlike AIEC, *Listeria* destroys the phagocyte.

**Why it spreads, physiologically.** Recent single-focus imaging shows
intracellular growth **slows as a focus matures**, especially at the centre,
because the bacteria exhaust one host cell's nutrients; the carrying capacity
of a single cytoplasm is finite. **Cell-to-cell spread is how it escapes
that limit** — it leaves for "greener pastures" before the current cell runs
dry ([Radhakrishnan 2025 bioRxiv](https://consensus.app/papers/details/9a86cf2732e15cb0b1c143315b18fe5b/?utm_source=claude_code);
preprint, not peer reviewed).
**That is a crowding/damping rule the game already implements in the lumen**
(§1b nutrient competition) — here it applies *inside a cell* and the escape
valve is spreading, not dying. Excellent symmetry.

**Spread speed.** 12.0 ± 3.1 µm/min, via ActA → Arp2/3; the rate scales
linearly with the number of proline-rich repeats in ActA, ~2–3 µm/min per
repeat ([Smith 1996 *JCB*](https://consensus.app/papers/details/a70e3746ff145eeab164ae22a3191339/?utm_source=claude_code))
and with ActA polarity/intensity
([Rafelski 2009](https://consensus.app/papers/details/6cc5e554ea145845be9aeaf59194adaf/?utm_source=claude_code)).
Like *Shigella*, in a human M-cell model it spreads **M-cell-to-enterocyte by
actin motility and evades transcytosis to the basolateral immune compartment
completely** ([Rey 2020](https://consensus.app/papers/details/db19b39d004b5391b9ed85bf4bc2bb5e/?utm_source=claude_code)).

**How it is cleared.** The textbook answer is the right one here, unusually:
**CD8 T cells**, including unconventional **H2-M3-restricted** ones that can
be primed TAP-independently
([Seaman 2000 *J Immunol*](https://consensus.app/papers/details/34b72ebeaa1d522eb046005da8e7eb70/?utm_source=claude_code);
review [Qiu 2018 *Pathogens*](https://consensus.app/papers/details/4fc91fbcd17f52fdb183f8fb46214abb/?utm_source=claude_code)).
And it must be **live** infection: heat-killed *Listeria* primes a large
memory CD8 population that is **non-protective**, because effector
differentiation never happens
([Lauvau 2001 *Science*](https://consensus.app/papers/details/6b5fbb1b66d55276bd8e42dc9ba5dd91/?utm_source=claude_code)).

> **Mouse caveat — specific and fatal to naive reuse.** **Mouse E-cadherin is
> not a receptor for InlA.** A single residue does it: **Pro16** in human
> E-cadherin. Pro→Glu in the human protein abolishes the interaction;
> Glu→Pro in the mouse protein gives complete gain of function. Mouse and rat
> are therefore **inappropriate models for the intestinal phase**; guinea pig
> is better ([Lecuit 1999 *EMBO J*](https://consensus.app/papers/details/7c16412662185f238407dfc0c27dab50/?utm_source=claude_code)).
> **InlB has a *separate* species restriction** — it does not stimulate
> guinea pig or rabbit Met, so no single small animal does both
> ([Khelef 2006](https://consensus.app/papers/details/c341c8da9a2a5f59be4a5a92d6143ef0/?utm_source=claude_code)).
> Modern oral mouse work uses **"murinised" InlA**
> ([Bergmann 2013 *BMC Microbiol*](https://consensus.app/papers/details/a970e3d3a1e25006a41ad6b6f83df83b/?utm_source=claude_code)).

---

### 3.5 *Yersinia enterocolitica* — the anti-tower unit

**Route of entry — the cleanest M-cell story on the list.** Scanning EM shows
**rapid, selective colonisation of the follicle-associated epithelium**;
*Y. enterocolitica* **invades Peyer's patches via M cells and not via other
FAE cells**, with no invasion of columnar absorptive cells observed
([Autenrieth 1996](https://consensus.app/papers/details/57269fbcc59052b58e819ff28b2a8640/?utm_source=claude_code);
[Grützkau 1990 *Gut*](https://consensus.app/papers/details/654416e83b1254efbb025b5fb060d129/?utm_source=claude_code)).
Molecularly, **invasin binds β1 integrins apically expressed on M-like
cells**; the bacteria are carried in **LAMP-1-negative** vacuoles and released
at the basal surface — i.e. **transcytosed, not resident**
([Schulte 2000 *Cell Microbiol*](https://consensus.app/papers/details/3d5ff5ed0f4952d09dba82d02ba149ed/?utm_source=claude_code)).

**Niche — none; it stays outside.** In mesenteric lymph nodes it forms
**pyogranulomas: neutrophils, inflammatory monocytes and lymphocytes
surrounding extracellular microcolonies**
(Bliska, Brodsky & Mecsas 2021 *EcoSal Plus*, PMID 34910573,
[DOI](https://doi.org/10.1128/ecosalplus.ESP-0014-2021)).

**How it subverts the phagocyte — by disarming it, not hiding in it.** YadA
binds host receptors, which engages the **Ysc–Yop T3SS** to translocate
**seven Yop effectors preferentially into phagocytes**, blocking **superoxide
production, degranulation and inflammasome activation**. The host fights back:
**YopJ triggers caspase-8-dependent apoptosis in monocytes**, which limits
dissemination from the node to the blood — so the same effector set
simultaneously helps the pathogen and the host (same review).

**Game translation, almost literal:** a pathogen that **debuffs any tower
that touches it** (no ROS, no degranulation, no inflammasome) and must be
killed by *massed* attack rather than by the usual phagocytosis. By day 5–7 in
mice, the FAE and parts of the Peyer's patch are **destroyed** by the
neutrophil influx — i.e. the player's own response wrecks the structure.

**Clearance.** Neutrophils and monocytes in pyogranulomas; YopE is processed
as an antigen and drives T and B responses. Clinically **self-limiting in most
cases**, 1–3 weeks, with shedding up to 40 days
([Quion 2021](https://consensus.app/papers/details/8073fa03eac45f5aaaea7b548334ff3c/?utm_source=claude_code)).
**Iron is a real vulnerability axis** — focal liver/spleen abscesses occur in
iron-overloaded patients (sickle cell, β-thalassaemia, deferoxamine), which is
a ready-made scenario modifier.

> **Caveat:** most of the mechanistic detail above is *Y. pseudotuberculosis*
> and mouse mesenteric lymph node. The two species share the pYV virulence
> plasmid and the Yop set, so the mechanic transfers, but the specific
> numbers do not.

---

### 3.6 *Campylobacter jejuni* — the gate-opener

**Route of entry — paracellular first, then basolateral.** It does **not**
invade efficiently from the apical face of polarised Caco-2 cells. Instead the
secreted serine protease **HtrA cleaves tight and adherens junction proteins**
and *C. jejuni* **transmigrates between cells**; on the basolateral side,
**CadF and FlpA bind fibronectin**, triggering membrane ruffling and
invasion ([Kemper & Hensel 2023 *AMB*](https://consensus.app/papers/details/f92015f9c83655028d29248d12b91aef/?utm_source=claude_code);
[Backert 2013 *Cell Commun Signal*](https://consensus.app/papers/details/ad345aba792958128c19223384f87b79/?utm_source=claude_code)).

**The mechanic, stated by the literature almost as a game rule:** HtrA-positive
*C. jejuni* **induced efficient transmigration of ordinarily non-invasive
bacteria** (*E. coli*, *Lactococcus lactis*) into the basal compartment; the
*ΔhtrA* mutant did not
([Sharafutdinov 2022 *Biomolecules*](https://consensus.app/papers/details/13d4d3d4c2fe54809ee0384ab011a297/?utm_source=claude_code)).
**One enemy type that raises the breach probability for every other enemy in
the wave.** Nothing else on this list does that.

**Niche — survives, barely replicates, and the macrophage wins.** The
**CCV** deviates from the canonical endocytic pathway immediately after a
caveolae-dependent entry and **avoids lysosomes** in epithelial cells;
**in macrophages it is delivered to lysosomes and rapidly killed**
([Watson & Galán 2008 *PLoS Pathog*](https://consensus.app/papers/details/63586a28d9d6541b93c39603952606bd/?utm_source=claude_code);
CCV maintenance needs **CiaI**,
[Buelow 2011](https://consensus.app/papers/details/44d5b2faeadf5e2b9d3f011ae52c24ef/?utm_source=claude_code)).
It persists ~72 h in the cell, traffics to the perinuclear space via dynein,
and secretes **cytolethal distending toxin**, which drives the cell into
programmed death
([Kemper 2023](https://consensus.app/papers/details/f92015f9c83655028d29248d12b91aef/?utm_source=claude_code)).

**Immune evasion — the disguise.** **Phase-variable** surface sugars. Homopolymeric
G-tracts in *cgtA* and *wlaN* flip the LOS outer core between **GM1-, GM2- and
GM3-like ganglioside mimics** mid-population
([Guerry 2002 *Infect Immun*](https://consensus.app/papers/details/7906f70dd9a9529db196bc43215c5edb/?utm_source=claude_code);
[Linton 2000 *Mol Microbiol*](https://consensus.app/papers/details/537f092c974a5ce79e7504d111225635/?utm_source=claude_code)),
and the repertoire extends well beyond gangliosides to P blood group,
paragloboside, lacto-N-biose and sialyl-Lewis c
([Houliston 2011 *JBC*](https://consensus.app/papers/details/97b6ad861bfa5a1d90755217b0e821ab/?utm_source=claude_code)).
Two consequences the game can use: **sialylation buys serum resistance** (a
*neuC1* mutant lost sialic acid and lost resistance to normal human serum),
and the mimicry **triggers autoantibodies against host gangliosides** —
Guillain-Barré. A pathogen whose coat is literally made of "self" is the most
legible possible antigenic-variation mechanic.

**Clearance.** Largely **self-limiting**: the inflammatory diarrhoea itself
flushes out mucus containing the organism
([Kemper 2023](https://consensus.app/papers/details/f92015f9c83655028d29248d12b91aef/?utm_source=claude_code)).
The dependence on **antibody** is visible in the clinic: hypogammaglobulinaemic
patients get persistent, recurrent and disseminated campylobacteriosis —
**15.1% stool-positive** in an Ig-substituted cohort, with lower IgA and IgM in
the positive group
([Mechernene 2025 *J Clin Immunol*](https://consensus.app/papers/details/aec9dc5007d652d188ebe024bea90a43/?utm_source=claude_code)),
and case reports of chronic enterocolitis, bacteraemia and even **cellulitis**
([Oulkadi 2026](https://consensus.app/papers/details/62a297eaf3c5574c8c1273141bef1777/?utm_source=claude_code);
[Hopkins 2025 *BMJ Case Rep*](https://consensus.app/papers/details/af86ecf9f67759108b57ce56a75dc67b/?utm_source=claude_code)).
In the 1988 volunteer study, ill volunteers were **protected from subsequent
illness but not from infection** with the same strain
([Black 1988](https://consensus.app/papers/details/9908c434f9295758802fddf94396676c/?utm_source=claude_code)).

> **Player-intuition mismatch:** *Campylobacter* is the most common bacterial
> gastroenteritis worldwide but is **killed easily by macrophages**. Its
> danger is structural (the barrier), not cellular. A player who builds
> phagocytes will find it trivially killable and *still* lose the wall.

---

### 3.7 Adherent-invasive *E. coli* (AIEC, LF82) — your macrophage, their nursery

**Route of entry.** Not M cells. It adheres to the **brush border of primary
ileal enterocytes from Crohn's patients but not controls**, via **type 1 pili
(FimH) binding CEACAM6**, which is abnormally expressed apically in CD — and
AIEC **induces more CEACAM6** via IFN-γ/TNF-α, promoting its own colonisation
([Barnich 2007 *JCI*](https://consensus.app/papers/details/ce504563250b57baac4670d0ea2f7d35ce1b/?utm_source=claude_code)).
AIEC FimH carries **recent pathoadaptive point mutations** that raise
CEACAM binding; swapping LF82's *fimH* for the K-12 version removes colitis in
CEABAC10 transgenic mice
([Dreux 2013 *PLoS Pathog*](https://consensus.app/papers/details/ada15d905c07501f8c5ad60a39d95d92/?utm_source=claude_code);
[Carvalho 2009 *JEM*](https://consensus.app/papers/details/5ac7aae820ff5e5e8db946dcb74f24f7/?utm_source=claude_code)).
**Design hook: the receptor only appears on an inflamed wall.** AIEC becomes
viable as a threat *because* earlier waves inflamed the tissue — a genuine
escalation mechanic.

**Niche — the one that breaks the rules.** It does **not** escape the
endocytic pathway. LF82-containing phagosomes acquire and lose EEA1 and Rab7,
then accumulate LAMP-1/2 and cathepsin D — they become **genuine, acidic
phagolysosomes — and acidity is *required* for replication**
([Bringer 2006](https://consensus.app/papers/details/0723560090895a78a614f022ac5ce63c/?utm_source=claude_code)).
Recent work describes a two-step adaptation: **replication arrest → stress-
tolerant persisters → resumption of growth → intracellular bacterial
communities in a biofilm-like matrix**, with vacuole acidification positively
correlated with persister induction
([Bruder 2025 *Gut Microbes*](https://consensus.app/papers/details/a6eb901a80335606a4305573500548f9/?utm_source=claude_code)).

**And the host cell survives.** All 15 CD strains replicated in J774
macrophages (2.2–74.2-fold over 48 h), and for LF82 there was **no LDH
release, no DNA fragmentation, no annexin-V binding even at 24 h** — but
**2.7× more TNF-α than LPS alone**
([Glasser 2001](https://consensus.app/papers/details/c6c0d1f9040f5568ace3473938613ab4/?utm_source=claude_code)).

**Game translation:** a macrophage that eats AIEC **stays alive, stays on the
board, keeps emitting cytokine, and keeps filling up**. The §4b contact
stress-sense roll is the only thing that finds it, and killing it costs you a
tower. This is the purest "innate immunity made it worse" unit available —
and it links directly to granuloma formation in Crohn's.

**Caveat:** everything above is **in vitro** (J774, THP-1, human MDM) or
transgenic mouse. There is **no ID50 and no incubation period**, and AIEC's
causal role in Crohn's is still argued. It is a strong *mechanic* with a
weaker *evidence base* than #1–#5.

---

### 3.8 *Mycobacterium avium* complex — the stalemate

**Route of entry — and this one defies the M-cell expectation.** Three
independent approaches (intestinal loop EM; bacterial load in Peyer's-patch
vs non-Peyer's-patch ileum; dissemination in B-cell-deficient mice that lack
Peyer's patches) all concluded that ***M. avium* invades the intestinal mucosa
primarily by interacting with enterocytes, not M cells**, preferentially at
the **terminal ileum**
([Sangari 2001 *Infect Immun*](https://consensus.app/papers/details/c27f1577f5745473bad9189ba9024443/?utm_source=claude_code)).
The related *M. avium* subsp. *paratuberculosis* uses **both** M cells and
enterocytes (M cells more efficiently) in lamb loops
([Ponnusamy 2013](https://consensus.app/papers/details/40064bfe640e53e19136fbce2142f826/?utm_source=claude_code)),
and crosses the mucosa in Peyer's-patch-free mice
([Bermudez 2010](https://consensus.app/papers/details/5c59753cd42a561dafa0b7605daf3519/?utm_source=claude_code)).

**Niche and the headline number.** Maximal in vitro growth rate is
**0.206 h⁻¹ → 3.4 h doubling**; the estimated **generation time in
chronically infected mouse mesenteric lymph nodes is ~7 days**
([McCarthy 1987](https://consensus.app/papers/details/877a64ee780f5e8ab5d8490652e873f1/?utm_source=claude_code)).
Over three months the load stayed flat in lung, rose 30× in spleen and
**8,900× in mesenteric lymph nodes**. **If one number from this whole document
goes into the game, make it this one:** the same organism is ~50× slower in
tissue than in culture.

**Phagocyte subversion.** The textbook "arrest at the early endosome" is
partly wrong in human cells. In human primary macrophages *M. avium*
phagosomes **do** mature to a LAMP1⁺/LAMP2⁺/CD63⁺ late endosomal/phagolysosomal
stage, where TLR7/8–MyD88–NF-κB/IRF-1 signalling **does** restrict growth —
and then **a fraction of the mycobacteria re-establish in a LAMP1⁻/LAMP2⁻/CD63⁻
compartment where they escape both killing and TLR recognition**
([Gidon 2017](https://consensus.app/papers/details/0ac6556fe4455f28b5eb22936d4dd57d/?utm_source=claude_code)).
**That is a relocation mechanic:** the tower damages it, and it moves to a
part of the tower the tower cannot see. In mouse macrophages, arrest requires
Rab5 activity and **iron availability** — dominant-negative Rab5 plus iron
blocked it, and iron starvation permitted killing
([Kelley 2003](https://consensus.app/papers/details/36fe76f523cf58209577bb5acbc8d5b2/?utm_source=claude_code)).
**Iron is a shared vulnerability axis with *Yersinia*** — a possible global
resource the player manipulates.

**Clinical shape.** Organisms cross the GI mucosa, are phagocytosed by lamina
propria macrophages **which cannot kill them**, spread through the submucosa,
drain to abdominal lymph nodes and enter blood. **Tissue destruction is rare;
most signs are cytokine-driven**
([Horsburgh 1999](https://consensus.app/papers/details/0a5953c623065cbd9d999f4918b90fdd/?utm_source=claude_code)).
Strains differ markedly in GI invasion and macrophage replication, and
patients can carry distinct or polyclonal strains
([Ohkusu 2004](https://consensus.app/papers/details/3a70f6261e945ef6a73b533a7963476d/?utm_source=claude_code)).

**Clearance.** Requires **IFN-γ-activated macrophages**; disseminated MAC is a
disease of profound CD4 depletion. It is therefore the natural "you must build
the Th1/IFN-γ rung" gate.

---

## 4. What this means for the game

Blunt, in priority order.

1. **Keep *Salmonella* as the intracellular-bacterium exemplar.** The design
   doc's call is right, for a reason the doc does not yet state: *Salmonella*
   is the **only** candidate whose replication rate is a function of **which
   host cell it is in** — fast in the epithelial cytosol, slow-to-dormant in
   the macrophage vacuole — and the game already treats those two differently.
   It is also the most recognisable name on the list by a wide margin.

2. **Fix one detail of the §4a row for it.** The doc says the intracellular
   bacterium "drains the cell to death, bursts out a brood." For *Salmonella*
   the brood does **not** burst into tissue — the infected enterocyte is
   **extruded into the lumen**, releasing invasion-primed, flagellated bacteria
   back into the flow (Knodler 2010, PMID 20876119,
   [DOI](https://doi.org/10.1073/pnas.1006098107)). **Route the brood back to
   the §1b lumen/adhesion/mill loop.** It is more accurate *and* it reuses
   systems you already shipped.

3. **Promote *Shigella* from "alternative" to a second, shipped unit, and give
   it bacterial cell-to-cell spread.** §4a currently reserves adjacent-tile
   spread for viruses — "Intracellular bacteria are not stated to spread this
   way." **That is wrong for *Shigella* and *Listeria*.** Both grow actin
   tails and push straight into the neighbouring cell without ever being
   extracellular. This is the strongest single mechanical addition available,
   and it makes "innate can't touch this" *spread* instead of merely *sit*.

4. **Make the Peyer's patch quantitatively load-bearing, not decorative.**
   Human enteroid data: apical invasion by *Shigella* is very limited; adding
   **M cells raises it ~10×**; **basolateral invasion is ≥100× more efficient
   than apical** (Ranganathan 2019). Encode that as a multiplier. It makes the
   patch a strategic liability the player must defend, which is a better
   reading of the biology than "the patch is where immunity happens."

5. **The wall's best defence against *Salmonella* is the wall shedding
   itself — and it costs you.** Epithelium-intrinsic NAIP/NLRC4 expels the
   infected enterocyte; without it, intraepithelial loads are **~100× higher**
   (Sellin 2014), but *also* without it, TNF-driven inflammation destroys the
   barrier within 72 h (Fattinger 2021). **This is a two-edged mechanic that
   already fits Sprint 22's barrier-death and sentinel-sloughing work** and
   should be wired to it rather than invented fresh.

6. **Stop assuming the cytotoxic-killer answer.** For *Salmonella*, CD8 is
   largely **dispensable** — MHC-II⁻, TCR-β⁻ and IFN-γR⁻ mice are susceptible
   while β2m⁻ and TCR-δ⁻ mice are resistant (Hess 1996). For *Shigella*,
   protection is **IgA/IgG against O-antigen, licensing neutrophils**
   (Bernshtein 2023, Clarkson 2020). **Only *Listeria* is genuinely the
   CD8 pathogen.** If the game wants one organism to justify building CTLs,
   it should be *Listeria*; if it wants one to justify **IFN-γ-activated
   macrophages**, that is *Salmonella* and MAC. These are different towers and
   the roster should say so.

7. **Human macrophage polarisation is a free, defensible tower-upgrade rule:**
   M1 human macrophages **do not permit** *Salmonella* replication; M2a and
   resting M0 do (Lathrop 2015). "An activated macrophage contains it; a
   resting one feeds it" is literature-backed, not hand-waving.

8. **If an enemy that attacks towers is wanted, it is *Yersinia*, and it
   should not be an intracellular unit at all.** It crosses M cells via
   invasin–β1-integrin, then stays **extracellular** in microcolonies and
   injects Yops that block phagocyte superoxide, degranulation and
   inflammasome. That is the "large extracellular bacterium" class with a real
   mechanic instead of a generic blob.

9. **If an enemy that breaks the wall for everyone else is wanted, it is
   *Campylobacter*.** HtrA-dependent junction cleavage lets **non-invasive
   bystander bacteria transmigrate too** (Sharafutdinov 2022). One unit that
   raises the whole wave's breach rate. Secondary mechanic: phase-variable
   LOS that mimics host gangliosides — a "wears your own markings" disguise.

10. **Respect the in-vivo/in-vitro gap explicitly in the tuning table.**
    *M. avium*: **3.4 h in broth vs ~7 days in vivo** (McCarthy 1987).
    *Shigella*: **41 min in HeLa** with no in vivo figure at all.
    *Listeria*: **<40 min in J774, ~90 min in Caco-2**, and growth **slows as
    one cell's nutrients run out** — which is the §1b crowding damper applied
    inside a cell, with spreading as the escape valve. Any rate taken from a
    cell line should be treated as an upper bound.

11. **Three mouse-model landmines, if anyone researches these further.**
    (a) Wild-type mice are **fully resistant** to *Shigella*; everything
    pre-2020 about shigellosis immunity in mice is from a host that cannot
    get the disease (Mitchell 2020). (b) ***S.* Typhi is human-only**; the
    "mouse typhoid model" is Typhimurium, which has **no Vi capsule and no
    typhoid toxin** (Johnson 2018, PMID 30030897,
    [DOI](https://doi.org/10.1111/cmi.12939)). (c) **Mouse E-cadherin is not
    an InlA receptor** — one residue, Pro16 — so mouse studies of *Listeria*
    gut crossing are invalid unless InlA was murinised (Lecuit 1999).

12. **Deferred but noted:** AIEC is the most mechanically original unit on the
    list — it **replicates inside a fully acidified phagolysosome without
    killing the macrophage**, leaving an infected, still-functional tower on
    the board (Glasser 2001, Bringer 2006). It has no ID50, no incubation
    period, and its receptor (CEACAM6) only appears on **already-inflamed**
    epithelium, which makes it a natural late-wave escalation unit rather than
    a launch enemy.

---

*Compiled 2026-10-25. All searches run through the `bio-research` PubMed and
Consensus MCP tools in this session. Nothing in the repo was modified.*
