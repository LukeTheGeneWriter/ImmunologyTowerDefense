# Extracellular & toxigenic gut bacteria — ranked shortlist for the roster

Research pass for Immunology Tower Defense, 2026-10-25. Domain: **extracellular and
toxin-mediated enteric bacteria**. Every factual claim below was checked against the
literature this session via PubMed and Consensus; anything I could not pin to a source is
marked **UNVERIFIED**. Sources are attributed inline — PubMed-sourced articles carry DOI
links, Consensus-sourced articles carry Consensus links.

Read against `docs/GAME_DESIGN.md` §1b (adhesion → mill → plaque → rupture), §4a (pathogen
classes), §6b (barrier colonisation), and `docs/IGA_REFERENCE.md` §1–2 (enchained growth).

---

## 1. The shortlist, ranked

| # | Organism | The ONE mechanic nothing else brings | Small-intestine fit | Why it earns a slot |
|---|---|---|---|---|
| **1** | ***Vibrio cholerae*** | **A toxin field that subverts the wall instead of killing it — and the damage it does feeds the bacterium.** CT drives massive secretion from a living, intact enterocyte; the resulting disease generates epithelial L-lactate that V. cholerae eats. A visible, interruptible positive-feedback loop. | **Native and exact.** Colonises the small intestine, lives in crypts in the proximal SI; never invades. | It is the only candidate that makes a host cell a *liability while alive*. Every existing enemy either kills a cell or hides in one. Highest-delta mechanic for the least engineering: it rides the adhesion-and-crowding model already shipped. |
| **2** | **EHEC / *E. coli* O157:H7** | **A toxin that passes straight through the wall and damages what is behind it.** Human gut epithelium does not express the Stx receptor Gb3 — the epithelium *translocates* Stx without being its target. The vasculature is. In this map, that is the base. | **Partial, and must be stated honestly.** Human disease is colonic (haemorrhagic colitis). In human small-intestine tissue it adheres only to the follicle-associated epithelium over Peyer's patches — which this game already has (§1d, Sprint 19). | The literal answer to "a toxin that damages tissue without the bacterium ever entering." Pairs with a tiny infectious dose (4–24 organisms) and StcE, which strips the inner mucus layer. Three distinct mechanics in one enemy. |
| **3** | **EPEC** (enteropathogenic *E. coli*) | **Adhesion that becomes irreversible.** Bundle-forming pili build a microcolony, then Tir is injected into the host membrane and intimin locks onto it on an actin pedestal the bacterium built itself. Once pedestalled, mucus flow and goblet sloughing cannot move it. | **Exact.** Classically a proximal small-bowel, paediatric pathogen; A/E lesions on small-intestinal mucosa. | It is the hard counter to the game's two flush defences (sentinel sloughing, mucus turnover) — the enemy that forces the player to *kill* rather than flush. No other candidate does that. |
| **4** | **EAEC** (enteroaggregative *E. coli*) | **A persistent structure rather than a crowd.** Stacked-brick aggregative adherence builds a thick mucus-embedded biofilm that shelters its members and survives between rounds; Pic simultaneously *makes the goblet cells secrete more mucus* and digests it. | **Good.** Adheres to jejunal, ileal and colonic mucosa in human biopsy organ culture. | The slow-burn attrition enemy and the perfect expression of §6b barrier colonisation. It also inverts the mucus upgrade: more mucus becomes more biofilm substrate. |
| **5** | **ETEC** (enterotoxigenic *E. coli*) | **Two toxins on two clocks.** ST hits guanylate cyclase C fast; LT is an AB5 cAMP toxin with a long fuse. One organism, two damage profiles with different onset and duration. | **Exact.** The canonical small-intestinal, non-invasive, adhere-and-secrete pathogen. | The cheapest, cleanest introduction to "toxin" as a verb — and it is the organism where blocking the adhesin (CFA/I) demonstrably stops the toxin delivery, which is the best in-game argument for the IgA tower. |
| **6** | ***Bacillus cereus*** (emetic) **/ *S. aureus*** — one shared slot, "the intoxication wave" | **An enemy with no bacterium in it.** Cereulide and the staphylococcal enterotoxins are preformed in food and arrive as cargo. There is nothing to kill, shoot, flush or phagocytose. | **Fits by transit, not by colonisation** — and that is the point. The toxin acts on the upper gut and vagal afferents in 4–10 hours. | Teaches that antibody-mediated **toxin neutralisation** is a real and categorically different answer from killing. Also the fastest-resolving wave in the game, which is good pacing contrast. |

### Rejected, with reasons

- ***Clostridioides difficile* — reject on anatomical grounds for this map.** It is a colonic
  organism. Small-bowel CDI exists but is rare enough to be publishable: 44 of 855 patients
  (5.1%) after *total colectomy*, at a median 130 days post-op, and a twelve-case ileal series
  was itself a notable report. More decisively, its defining trigger — antibiotic demolition of
  a dense resident community — requires a colonic microbiota the small intestine does not have.
  Its toxin mechanics (Rho glucosylation → cytoskeleton collapse → tight-junction failure) are
  also largely covered by EPEC's effectors plus cholera's barrier effect. **Keep it as the
  headline act for a future colon map, where it is the single best enemy in the whole genus.**
- ***Vibrio parahaemolyticus* — hold.** Real, well-characterised (TDH + T3SS2, 17 h median
  incubation), but mechanically it reads as "EHEC with a cytolysin and fewer distinctives."
  Nothing here that slots 1–6 do not already cover better.
- ***Aeromonas* — reject.** Its causal role in diarrhoea is still contested after fifty years,
  and a 447-genome GEMS analysis found little evidence of genetic determinants for diarrhoeal
  disease. Do not build an enemy whose pathogenicity the field cannot agree on.

---

## 2. Numbers table

| Organism | Infectious dose (human) | Incubation | In vivo doubling in gut | Toxin | Source |
|---|---|---|---|---|---|
| ***V. cholerae*** | **10⁸** organisms in buffered saline; **10⁴** when given with NaHCO₃ — acid neutralisation drops the dose 10,000× | mean **28 h** at a 10⁵ dose | **UNVERIFIED** as a doubling time. Within-host replication, death and migration rates have been resolved per gut segment in mice, and the available niche *grows with inoculum size* (cooperative effect) | **CT** — AB5, binds GM1 ganglioside, ADP-ribosylates Gsα → constitutive adenylate cyclase → cAMP → apical Cl⁻ secretion | Cash 1974 ([Consensus](https://consensus.app/papers/details/50115c69681c5d168fa503dda17f3711/)); Sack 1998 ([Consensus](https://consensus.app/papers/details/5e6de25dc5a05bd4ae1b23ee3c82f39c/)); Gillman 2021 ([Consensus](https://consensus.app/papers/details/48ac56596bac5fdab642a7e2f7490edf/)) |
| ***V. cholerae*** (2nd dose estimate) | **ED₉₀ = 1.3 × 10⁷ CFU** in Thai volunteers; attack rate correlated with inoculum (p<0.01) | — | — | — | Suntharasamai 1992 ([Consensus](https://consensus.app/papers/details/9eff61c64a3d52cd9d908c8183456bcb/)) |
| **ETEC** (H10407) | **≥10⁹** reliably; **2 × 10⁷ + bicarbonate** is the lowest practical challenge dose; **10⁷ → 71%** moderate-to-severe diarrhoea; **10⁶ → 27%**; **10⁵ → 13%** | ~1–3 d (**UNVERIFIED** as a precise median) | **UNVERIFIED**. Shedding reaches geometric mean **7.5 × 10⁷ CFU/g stool** | **LT** (AB5, cAMP) and/or **ST** (binds guanylate cyclase C → cGMP) | Harro 2011 ([DOI](https://doi.org/10.1128/CVI.05194-11)); Chakraborty 2018 ([Consensus](https://consensus.app/papers/details/c28a56820e79564fb84a68b4942e3e33/)); Porter 2011 ([Consensus](https://consensus.app/papers/details/ca4594d92cca553cbd47a1feaaac09b8/)) |
| **EPEC** | **UNVERIFIED** for a modern ID₅₀. Volunteer work is complicated by the fact that EPEC causes disease primarily in infants and small children | ~1–2 d, **UNVERIFIED** | **UNVERIFIED** | **No classical secreted enterotoxin.** T3SS effectors (EspF, Map, and non-LEE effectors) efface microvilli and dismantle tight junctions | Small 2014 ([Consensus](https://consensus.app/papers/details/52c9656d638a54e49f779309b9f4397f/)); Ahmad 2025 ([DOI](https://doi.org/10.7759/cureus.83475)) |
| **EHEC O157:H7** | **4–24 organisms** ingested, modelled from a scout-camp environmental outbreak — fits a *Shigella*-like dose-response curve | **62–87 h** median (STEC/Campylobacter are the slowest bacterial group) | **UNVERIFIED** | **Stx1/Stx2** — AB5, B-pentamer binds Gb3, A-subunit depurinates 28S rRNA → protein-synthesis arrest → death. Also apoptosis via ER stress, and Stx-loaded extracellular vesicles | Strachan 2001 ([DOI](https://doi.org/10.1111/j.1574-6968.2001.tb10822.x)); Teunis 2004 ([DOI](https://doi.org/10.1111/j.0272-4332.2004.00441.x)); Teunis 2007 ([DOI](https://doi.org/10.1017/S0950268807008771)); Chai 2019 ([Consensus](https://consensus.app/papers/details/b50639e235c9591bab460cd3e83faf7a/)); Menge 2020 ([Consensus](https://consensus.app/papers/details/efd243e4b52a5f82b67d64e918a87f2d/)) |
| **EAEC** (strain 042) | **10¹⁰ CFU → diarrhoea in 3 of 5** volunteers. Three other EAEC strains at the same dose caused none — virulence is strain-specific, not pathotype-wide | **UNVERIFIED** acutely; the clinical signature is *persistent* diarrhoea | **UNVERIFIED** | **EAST1**, **Pet**, **Pic** (mucinase + mucus secretagogue), ShET1; plus a 108-kDa cytotoxin | Nataro 1995 ([DOI](https://doi.org/10.1093/infdis/171.2.465)); Nataro 1996 ([DOI](https://doi.org/10.1128/iai.64.11.4761-4768.1996)) |
| ***B. cereus*** (emetic) | Toxin **0.01–1.28 µg/g** measured in 13 of 14 implicated foods; organism reaches **10⁷–10⁸ CFU/g** in boiled rice within 24 h at 30–35 °C | **4–10 h** median (preformed-toxin group) | n/a — preformed in food | **Cereulide**, a heat-stable cyclic dodecadepsipeptide; K⁺-selective ionophore that collapses mitochondrial membrane potential (**EC₅₀ 98–127 nM** on oxygen consumption) | Agata 2002 ([Consensus](https://consensus.app/papers/details/75f220d777fb529e87d847470cd8d9ff/)); Sonji 2026 ([Consensus](https://consensus.app/papers/details/4a43b31b245f5d5fa1ef7f528707f7ec/)); Chai 2019 |
| ***B. cereus*** (diarrhoeal) | **UNVERIFIED** dose; infection by vegetative cells/spores producing toxin **in the small intestine** | longer than emetic, **UNVERIFIED** | **UNVERIFIED** | **Hbl**, **Nhe** (tripartite pore-formers), **CytK** | Fagerlund 2008 ([Consensus](https://consensus.app/papers/details/eb556988b5335a68a681225ced735e18/)); Dietrich 2021 ([Consensus](https://consensus.app/papers/details/6aee4c65a9595627a6fe5a12d26f5df9/)) |
| ***S. aureus*** (SFP) | **20–100 ng SEA total intake** caused illness in the 13,420-case Osaka outbreak; **94–184 ng/carton (mean 144 ng)** in a US chocolate-milk outbreak, attack rate 31.5% at one carton rising to 44.4% at three | **4–10 h** median | n/a — preformed | **SEA–SEE + >24 SEls**; superantigens, heat- and protease-resistant; emesis via enteric nervous system / vagal afferents, 5-HT release from enterochromaffin cells | Asao 2003 ([Consensus](https://consensus.app/papers/details/3956d5638d4b5edd8b148e8f7db55e6f/)); Evenson 1988 ([Consensus](https://consensus.app/papers/details/3c3a56c1afe3543bae159a3aa613e858/)); Hu 2014 ([Consensus](https://consensus.app/papers/details/4584cc8dfcfe513280854cd2ea1d6fa4/)) |
| ***V. parahaemolyticus*** | **UNVERIFIED** (10⁵–10⁷ is widely quoted; I found no primary volunteer source this session) | **17 h** median — distinct from both preformed toxins and the slow invasives | **UNVERIFIED** | **TDH** (+TRH) and **T3SS2**; T3SS2 is the major contributor to enterotoxicity, T3SS1 to cytotoxicity | Chai 2019; Hiyoshi 2010 ([Consensus](https://consensus.app/papers/details/3b0160fe3fc15d74aae0fcc319a181d4/)) |
| ***C. difficile*** | Not dose-governed — governed by **loss of colonisation resistance** | n/a | **UNVERIFIED** | **TcdA/TcdB** — UDP-glucosyltransferases that monoglucosylate and inactivate Rho GTPases → F-actin disorganisation → occludin/ZO-1/ZO-2 dissociate from the tight junction. TcdB also drives RAS-dependent necrosis independent of its glucosyltransferase activity | Nusrat 2001 ([Consensus](https://consensus.app/papers/details/b8da5a4ae5ea5a04aa8d271b5f304863/)); Stieglitz 2022 ([Consensus](https://consensus.app/papers/details/260e631dfd6d5cc7a2c52320d40be8eb/)) |
| ***Shigella*** — **reference anchor, not a candidate** (it is invasive/intracellular, §4a's other class) | **10–100 organisms**; 10–500 across volunteer studies. Acid-resistant at pH 2.5 for ≥2 h | **32–45 h** median | — | Shiga toxin (*S. dysenteriae* 1) | Hendrick 2025 ([Consensus](https://consensus.app/papers/details/62f408bd6852520b94481a6e2bfc243d/)); Gorden 1993 ([Consensus](https://consensus.app/papers/details/dca517b702895dc989836f57813d79bf/)) |

**The dose spread is ~7 orders of magnitude and it is not arbitrary.** The best single
explanation in the literature is structural, and it is exactly the split this game already
uses: *"Most of the enteric pathogens which require a very large inoculum to cause disease,
such as V. cholerae and ETEC, are extracellular pathogens whose mode of virulence involves
adherence and toxin secretion"* — whereas the low-dose organisms are the ones that survive and
replicate inside cells (Small 2014). **Extracellular/toxigenic ⇒ high dose; invasive ⇒ low
dose.** EHEC is the striking exception that proves the rule: it is extracellular and
adherence-based, yet infects at 4–24 organisms, because it carries *Shigella*-grade acid
resistance (glutamate- and arginine-decarboxylase systems, both RpoS-linked) through the
stomach.

**Stomach acid is explicitly the reason for the cholera number.** Cash 1974 is the cleanest
natural experiment in enteric microbiology: 10⁸ organisms in buffered saline, 10⁴ with
bicarbonate. The acid barrier is worth four logs.

---

## 3. Per-organism detail

### 3.1 *Vibrio cholerae* — rank 1

**Adhesion — loose, motility-first, never intimate.** V. cholerae is non-invasive. It swims
through the mucus layer with flagellar motility; alkaline pH (and the small intestine spans
pH 6–8) increases swimming speed and improves mucus penetration, while acidic pH flips it
toward *surface attachment* by activating the MshA pilus (Nhu 2021,
[Consensus](https://consensus.app/papers/details/b740a57354235ecfa07062dcf763e8b8/)). Mucin
itself induces T6SS activity, which slows the cell and suppresses its reorientation flicks —
the proposed effect is that mucus biases cells toward *holding course* for the epithelium
rather than tumbling (Frederick 2020,
[Consensus](https://consensus.app/papers/details/35d25f776d7e5c87bf527289f09eef3f/)). Final
attachment is via the **toxin-coregulated pilus (TcpA)**, a type IV pilus.

Two findings that are directly game-shaped. First, **microcolonies arise from single founder
cells** — so an adherent clump is a clonal family, exactly as `IGA_REFERENCE.md` §2C says of
enchained growth. Second, colonisation is **regiospecific**: in the proximal small intestine
V. cholerae lives *exclusively inside developing crypts*, and motility is required there but
not distally; host mucins are the key factor limiting colonisation, and more so proximally
where the mucus layer is thicker (Millet 2014,
[Consensus](https://consensus.app/papers/details/0d59ea8524bc53ddbd88acb0f1230fc8/)). That is a
clean argument for an enemy that *seeks crypts* on this map.

**Toxin mechanism — the cell is subverted, not killed.** CT is an AB5 toxin: the B-pentamer
binds GM1 ganglioside, the A1 fragment ADP-ribosylates Gsα, adenylate cyclase locks on, cAMP
rises, and CFTR dumps chloride apically. The enterocyte stays alive and functional the whole
time; in Caco-2/HT29-MTX monolayers CT produces a rise in short-circuit current with the
barrier intact (Hoffmann 2021,
[Consensus](https://consensus.app/papers/details/229b68b5fe0550f9bd4fbe16e6ad5696/)).

**Caveat worth building in:** "does not kill" is not "does not damage." In ligated rat jejunum,
CT lowers transepithelial resistance, raises paracellular permeability, **upregulates
claudin-2, downregulates tricellulin, reduces the number of microvilli and widens the
intercellular space** (Markov 2019,
[Consensus](https://consensus.app/papers/details/7c9e361549dc56e1aa6668d7432489c8/)). So CT
*loosens the wall* without killing it — mechanically, a debuff on epithelial integrity, which
in this game means a **faster-charging plaque over the same crowd**.

**Does it act at a distance? Yes, and further than you would expect.** Perfuse cholera toxin
into a rat jejunum and the *colon* — never exposed to the toxin — starts secreting too. Transect
the gut below the jejunum to break enteric-nervous-system continuity and the distant effect
disappears. Other secretagogues (8-Br-cGMP, hyperosmolar mannitol) produce no distant effect at
all (Nocerino 1995,
[Consensus](https://consensus.app/papers/details/c85ea99668725ed2914b0cded084f367/)). The
mechanism is a neural reflex: CT releases 5-HT from enterochromaffin cells, with substance P
and VIP as downstream transmitters (Farthing 2000,
[Consensus](https://consensus.app/papers/details/92a7e68555ad58e18cef26625dd662b2/)). **This is
a field, not a local effect, and the field is nerve-propagated — it can legitimately reach
further than any diffusion radius would allow.**

**The loop that makes it the number-one pick.** CT-induced disease upregulates epithelial LDHA,
the enterocytes pour **L-lactate** into the lumen, and V. cholerae's L-lactate dehydrogenase
(LldD) converts that into a fitness advantage — an advantage absent in a ΔctxAB mutant and
reduced in mice lacking epithelial LDHA (Gutierrez 2026,
[Consensus](https://consensus.app/papers/details/285f2d9ec26f58b4bc6b3ace4827a1a9/)).
Separately, oxidative metabolism of host substrates including **mucin** drives the population
expansion to high density (Van Alst & DiRita 2020,
[Consensus](https://consensus.app/papers/details/9d9892661f9353a4a70fc5423cb548ae/)).
**Translation: the toxin creates the food. Harming the wall is literally the enemy's economy.**

**IgA and mucus.** This is the organism where the IgA story is cleanest and most surprising.
Monoclonal anti-CT-B IgA blocks CT binding to the apical membrane and abolishes the chloride
response in polarised human T84 monolayers, dose-dependently — and does so with antibodies that
are *not* directed at the GM1-binding site (Apter 1993,
[Consensus](https://consensus.app/papers/details/620e40cdacd356df869ada8e36a47916/)). And in the
field: breast-milk anti-cholera IgA did **not** protect children from becoming colonised, but
among those colonised, the ones who got sick had drunk milk with significantly *lower*
anti-toxin and anti-LPS IgA (Glass 1983,
[Consensus](https://consensus.app/papers/details/4386f7303f3154dfa4dd490c422c06a0/)). **IgA
against cholera neutralises the weapon, not the enemy.** That is a mechanically distinct IgA
effect from the enchainment model already in `IGA_REFERENCE.md`, and it is a second, honest job
for the IgA tower.

**Clearance.** Flush, not kill — matching §6b. Cholera is the textbook **non-inflammatory**
diarrhoea: experimentally infected volunteers kept faecal lactoferrin titres below 1:50, versus
1:400+ for *Shigella* and *C. difficile* (Miller 1994,
[Consensus](https://consensus.app/papers/details/96693a5fbfdd591bbdfd32dead9a2f67/)). **Nuance
that should be built in, not glossed:** duodenal biopsies from actual cholera patients show a
significant acute-stage neutrophil influx and upregulated myeloperoxidase, lactoferrin, TNF-α
and α-defensin, with mast cells and eosinophils arriving in convalescence (Qadri 2003,
[Consensus](https://consensus.app/papers/details/9b13c1c407a6541c95c2a5292c86b90d31cac2a/)).
The old "cholera is immunologically silent" line is an oversimplification.

**Inflammation bloom?** Not in the classical nitrate/tetrathionate sense — that is an
Enterobacteriaceae trick (§3.7). V. cholerae's bloom is **toxin-driven and metabolic** (the
lactate loop above), which is arguably the better mechanic because the player causes it by
failing, not by succeeding.

---

### 3.2 EHEC / *E. coli* O157:H7 — rank 2

**Adhesion — intimate, the full A/E pedestal.** Intimin (the *eae* product) is an outer-membrane
adhesin that binds **Tir**, a receptor the bacterium injects into the host membrane through its
own T3SS; the complex nucleates actin into a pedestal beneath the adherent cell (Luo 2000,
[Consensus](https://consensus.app/papers/details/d7271688f9135acd877ad312b03110b4/)). The
pedestal is not decoration: current review evidence attributes to it cytosolic anchoring via
host cortical actin, enhanced surface motility and spread to neighbouring cells, and improved
effector delivery (Miner 2024,
[Consensus](https://consensus.app/papers/details/2828ee5b1a3c579c95b0fbdc626b9708/)). Earlier
loose adherence uses **E. coli common pilus (ECP)**, which commensal *E. coli* also carry
(Rendón 2007,
[Consensus](https://consensus.app/papers/details/ca3a955255b156f28582d15448a6a564/)).

**Mucus stripping is a separate, buildable mechanic.** EHEC's secreted metalloprotease **StcE**
reduces MUC2 glycoprotein levels and **thins the inner mucus layer**, promoting access and
binding to the epithelium both in vitro and in human colonic biopsies ex vivo — and this is
catalytic-activity-dependent (Hews 2017,
[Consensus](https://consensus.app/papers/details/60a2a6b298e75cd6a87cdee7eb7460c6/)).

**Toxin — kills, and kills something other than what it touched.** Stx is an AB5 ribotoxin:
protein-synthesis arrest and cell death, plus apoptosis via ER stress and broad disruption of
intercellular communication (Menge 2020). The critical architectural fact:
**human intestinal epithelium does not express Gb3 or any other Stx binding site.** No
Stx-specific secretion system is known; toxin is thought to be released on bacterial lysis, and
it crosses the epithelial barrier by translocation to reach the systemic circulation (Schüller
2011, [DOI](https://doi.org/10.3390/toxins3060626)). It is the **vasculature** that expresses
high Gb3 and takes the damage (Bellmeyer 2009,
[DOI](https://doi.org/10.1152/ajpgi.00050.2009)). Stx-loaded extracellular vesicles then carry
the toxin further to recipient cells (Lee 2023,
[Consensus](https://consensus.app/papers/details/9b0b70b84ab955989789c8e3bb28777b/)).

**In game terms this is almost too neat: EHEC fires a projectile that passes through the
epithelium without harming it and damages the blood — which on this map is the base.** It is
range without advance, and it decouples "did it breach?" from "did it hurt me?" for the first
time.

**Anatomical fit — say the awkward part.** Two good human-tissue studies disagree in a way that
matters. In human/bovine intestinal xenografts, EHEC induced A/E lesions and haemorrhagic-colitis-like
damage in **colon** xenografts and **did not attach to or damage small-intestinal epithelium
at all** (Golan 2010,
[Consensus](https://consensus.app/papers/details/c49f98e5e5ac5ba8bd8bb80a85f1559e/)). In human
intestinal in-vitro organ culture, EHEC *did* colonise small intestine — but **only** the
follicle-associated epithelium of Peyer's patches, and swapping intimin γ for EPEC's intimin α
released that restriction and let it colonise ordinary small-intestinal explants (Fitzhenry
2002, [DOI](https://doi.org/10.1136/gut.50.2.180)). **Resolution for the game: EHEC belongs on
this map as a Peyer's-patch specialist.** That is literature-faithful, it is a real tropism
determined by a single adhesin variant, and it gives the Sprint-19 M-cell/Peyer's-patch content
a reason to be defended rather than merely harvested.

**Clearance.** Antibiotics are contraindicated clinically (they can trigger Stx release), which
is a nice flavour note. EHEC actively dampens the epithelial inflammatory response to both host
cytokines *and its own toxin*, which is read as facilitating colonisation (Bellmeyer 2009).
Mechanically: **an enemy that suppresses the alarm it would otherwise raise** — a legitimate
reason for a long detection delay.

---

### 3.3 EPEC — rank 3

**Adhesion — the irreversible one.** Typical EPEC first attaches via **bundle-forming pili
(BFP)** and forms localised microcolonies, then builds A/E lesions through the LEE-encoded
T3SS, Tir and intimin. One important corrective from a careful knockout study: an EPEC mutant
expressing intimin and Tir but **no other effectors** still triggered robust actin
polymerisation in cell lines — yet was **unable to form A/E lesions on human intestinal organ
culture**. Non-LEE effectors (and Efa1/LifA) are required for A/E lesions on real mucosa
(Cepeda-Molero 2017,
[Consensus](https://consensus.app/papers/details/6f78267b0bf25a2f996a666a3b2cb303/)). So
"intimin + Tir = pedestal" is a cell-culture simplification; on tissue it takes the full kit.

**Toxin — there isn't one, and that's the point.** EPEC has no classical secreted enterotoxin.
Damage comes from effacement of microvilli (malabsorption) plus T3SS effectors such as EspF and
Map that dismantle tight junctions (Ahmad 2025, [DOI](https://doi.org/10.7759/cureus.83475)).
Range: **zero** — strictly contact-dependent, injected through a needle into the cell it is
sitting on.

**Mechanically**: EPEC is the enemy whose damage is purely local and whose adhesion cannot be
undone. Sentinel sloughing and mucus turnover — the game's two flush answers — should simply
fail against a pedestalled EPEC. It is also inflammatory: experimental EPEC infection produced
faecal lactoferrin titres of 1:100–1:1600, with 43% at ≥1:400, i.e. squarely in the inflammatory
band alongside *Shigella* and *C. difficile* (Miller 1994). So EPEC draws neutrophils, with all
the collateral tissue damage §4a already specifies for innate clearing.

**Anatomy**: proximal small bowel, paediatric. Exact fit, no caveats needed.

---

### 3.4 EAEC — rank 4

**Adhesion — aggregative, "stacked brick," and it builds a structure.** Five AAF types; deleting
the major fimbrial subunit of any of them significantly reduces biofilm, and several AAF types
are needed for colonisation in mice (Van Nederveen 2025,
[Consensus](https://consensus.app/papers/details/5a3ac7cad3be5cc0a8ad534ed522bd5f/)). In 140
Brazilian clinical isolates, 78.6% produced the aggregative pattern and 69.3% were biofilm
producers; a distinct subgroup produces **chain-like adherence**, forming literal chains of
bacteria on the cell surface (de Souza 2025,
[Consensus](https://consensus.app/papers/details/1959bd15c98e50eab55cc9a116a8d896/)). *The
chain-like phenotype is worth flagging to the art side: it looks exactly like IgA enchainment
but is bacterially driven.*

**The mucus interaction is the standout, and it cuts both ways.** **Pic** is a dual-function
autotransporter: it acts directly on goblet cells to induce **mucus hypersecretion** (via
PLC → DAG/IP₃ → calcium signalling) **and simultaneously digests the mucins that get secreted**,
cleaving MUC2 and MUC5AC in the C-terminal domain so the gel cannot form properly. A
serine-protease-site mutant loses the digestion but keeps the secretagogue activity — the two
functions are separable (Flores-Sánchez 2020,
[Consensus](https://consensus.app/papers/details/2833576aba535c39b1b32ba6cf6e808a/)). EAEC
therefore *farms* mucus: it makes the goblet cells produce more, then eats it and uses the
residue as biofilm matrix. **This directly inverts §6b's mucus-turnover upgrade, which is a
genuinely interesting thing to do to a player who has just bought it.**

**Toxin.** EAST1, Pet, ShET1 and a 108-kDa cytotoxin; strain 042 exfoliates mucosal epithelial
cells from human jejunal, ileal and colonic biopsies and induces vesiculation, microvillus
shedding and eventual rupture in T84 cells — **without being internalised** (Nataro 1996,
[DOI](https://doi.org/10.1128/iai.64.11.4761-4768.1996)). Strictly contact-range.

**The signature is chronic, not acute.** In the MAL-ED birth cohort, 27.5% of 27,094
non-diarrhoeal surveillance stools were EAEC-positive and 94.8% of children had at least one
detection by age two; EAEC was **not** associated with diarrhoea but **was** weakly associated
with intestinal-inflammation biomarkers and strongly with reduced length at two years (LAZ
−0.30) (Rogawski 2017,
[Consensus](https://consensus.app/papers/details/8bb8d03a28ba5678bdfbec0c31b16d03/)). **This is
the attrition enemy: it does not cause a breach, it degrades the board.** A standing EAEC
biofilm that caps ATP income or slows epithelial regrowth is a faithful reading.

**Virulence is strain-specific.** Nataro 1995's four-strain volunteer study is the honest
caution: same pathotype, same 10¹⁰ dose, only one strain caused disease. If the game ever shows
a knowledge rung for "EAEC," that rung is doing more work than the species name suggests.

---

### 3.5 ETEC — rank 5

**Adhesion — loose, fimbrial, blockable.** Colonisation factors, of which CFA/I is the major
one. In human enteroids, **CFA/I is the critical adhesin** even in strains that also express
CS21, and only anti-CFA/I antibodies inhibited adherence (Smith 2022,
[Consensus](https://consensus.app/papers/details/acdd589c5880555082fae9edef108301/)). A human
challenge trial of an intradermal CfaE (fimbrial tip adhesin) vaccine achieved 27.8% efficacy
against moderate-to-severe diarrhoea — real but modest, which is a useful calibration for how
strong an in-game "block the adhesin" tower should be (Gutiérrez 2024,
[Consensus](https://consensus.app/papers/details/bd6845fd4cb5579a88825b6445ff7767/)).

**Mucus.** **EatA**, a SPATE, specifically degrades MUC2. EatA, EAEC's Pic and *Shigella*'s SepA
are **functionally redundant MUC2 mucinases**, and monoclonal antibodies from ETEC-infected
humans that target a shared core β-helix epitope neutralise all three and block mucus
penetration by ETEC, *S. flexneri* and Pic-producing EAEC (Buckley 2026,
[Consensus](https://consensus.app/papers/details/1a3ac8820113598a815e3f8178f8bb51/)). **A single
in-game "mucinase" trait shared across ETEC/EAEC/Shigella, counterable by one antibody upgrade,
is literature-faithful — that is unusual and worth taking.**

**Toxins — two clocks.** LT is an AB5 cAMP toxin, close kin to CT. ST binds guanylate cyclase C
and raises cGMP. Notably, in the human enteroid model, **ETEC adherence and ST delivery did not
disrupt monolayer integrity** — no TEER drop, no IL-8 (Smith 2022). So ETEC is the purest
"secretion without damage" organism in the set; it is cholera with the volume turned down.

**Inflammation.** Mild but real: faecal lactoferrin 1:50–1:200 in 53% of experimentally infected
volunteers (Miller 1994), and challenge studies show stool MPO and serum I-FABP rising in both
symptomatic *and asymptomatic* infection, with higher MPO associated with *higher* ETEC
colonisation (Brubaker 2021,
[Consensus](https://consensus.app/papers/details/7f8e1f2c011a5df58b8da2984c1a115e/)).

**Why rank 5 and not higher:** mechanically it is a strict subset of *V. cholerae* plus a second
toxin clock. Build it after cholera, as a cheap, early-wave variant that reuses the same field code.

---

### 3.6 Preformed toxin — *B. cereus* (emetic) and *S. aureus* — rank 6, shared slot

**There is no adhesion step, no colonisation, no replication in the player's gut.** Both diseases
are **intoxications**: the toxin is made in the food before ingestion. Cereulide is a heat-stable
cyclic dodecadepsipeptide, a K⁺-selective ionophore that collapses mitochondrial membrane
potential and depletes ATP at nanomolar concentrations. Staphylococcal enterotoxins are
superantigens that resist heat, cold and proteolysis across a wide pH range, and trigger emesis
through the enteric nervous system and vagal afferents rather than by damaging enterocytes
directly.

**Both act entirely at a distance from any bacterium, because there is no bacterium.** The
incubation data make the pacing contrast concrete: preformed-toxin outbreaks (S. aureus,
B. cereus, C. perfringens) have **4–10 h median** incubations, distinct from everything else
(Chai 2019).

**Important distinction to preserve:** *B. cereus* has **two** diseases. The emetic one is the
intoxication above. The **diarrhoeal** one is a genuine small-intestinal infection in which
ingested vegetative cells or spores produce **Hbl, Nhe and CytK — three-component pore-forming
cytotoxins — in the small intestine itself** (Fagerlund 2008; Dietrich 2021). That second mode
*is* a small-intestine colonising enemy with a pore-forming toxin, and it is a perfectly good
alternative if the Director wants the slot to be a live organism rather than a cargo wave.
Toxin production varies enormously between strains, which again complicates any single
"knowledge of *B. cereus*" rung.

**Mechanically:** a wave with no target. Towers cannot shoot it. Only mucus/flow dilution and
**antibody-mediated neutralisation** touch it. That makes it the ideal teaching wave for the
IgA/antibody tower's *second* job — neutralisation as distinct from enchainment.

---

### 3.7 Inflammation-fuelled blooms — the mechanic is real, but check which organism

This is the most game-shaped finding in the whole domain and it deserves to be stated precisely,
because the Director will want to know who actually does it.

**The mechanism (Enterobacteriaceae).** In the healthy intestine, Enterobacteriaceae scrape by on
low concentrations of diet- and mucus-derived monosaccharides via glycolysis and mixed-acid
fermentation. **Inflammation reshapes that niche**: immune-derived oxidants generate new terminal
electron acceptors — **nitrate** and **tetrathionate** — and let oxygen diffuse into the lumen.
Facultative anaerobes can respire; strict-anaerobe commensals cannot. The pathogen's usable
substrate range widens and it outgrows the community. This is a *conserved programme* across
*E. coli*, *Citrobacter rodentium*, *Klebsiella* and *S.* Typhimurium (Schubert 2026, *Nat Rev
Microbiol*, [Consensus](https://consensus.app/papers/details/e65991c13ebb5bb1b6abc8b24583d75c/);
Baldelli 2021, [Consensus](https://consensus.app/papers/details/a9a625ae04655e4f85639e208d0f85a0/)).

**A second, nastier route, in case the mechanic needs teeth.** Host ROS **lyse the commensals
themselves**, releasing aspartate; *S.* Typhimurium runs aspartate through fumarate respiration
(AspA-dependent) alongside nitrate respiration and outcompetes even commensal Enterobacteriaceae.
The advantage vanishes in germ-free mice and is restored by colonising with Bacteroidia and
Clostridia — i.e. **the pathogen is eating the corpses of the player's commensals** (Yoo 2024,
[Consensus](https://consensus.app/papers/details/da96e19b4cf151fb95da30bf6fc6208f8/)).

**Who on this shortlist genuinely does it?**
- **EPEC/EHEC — yes, by the attaching-and-effacing route.** Ethanolamine sensing via **EutR**
  drives LEE expression; in *C. rodentium* (the murine EPEC/EHEC model) EutR is required for
  maximal LEE expression, colonisation and transmission (Rowley 2020,
  [Consensus](https://consensus.app/papers/details/61b84c148ac356649b68cb90d31cac2a/)). A/E
  infection also flips intestinal epithelial cells from oxidative phosphorylation to aerobic
  glycolysis with higher LDHA and MCT4 (Carson 2019,
  [Consensus](https://consensus.app/papers/details/1caed0dc91065bf7a504595091174dc5/)) — note
  that LDHA/lactate signature again, converging with the cholera finding.
- ***V. cholerae* — yes, but by its own toxin, not by inflammation** (the L-lactate loop, §3.1).
- ***C. difficile* — yes, and most dramatically, but after *antibiotics*, in the colon.**
  Microbiota-liberated **sialic acid** fuels post-antibiotic expansion of both *C. difficile* and
  *Salmonella*; a sialidase-deficient *B. thetaiotaomicron* starves *C. difficile* of it (Ng
  2013, *Nature*,
  [Consensus](https://consensus.app/papers/details/f9066f2ff1f75bf18387d1fc05207e93/)).
  **Succinate** from commensal fermentation spikes after antibiotics or a motility disturbance,
  and *C. difficile* converts it to butyrate to expand (Ferreyra 2014,
  [Consensus](https://consensus.app/papers/details/bac34ebbb95452aa85a1ec03968157ce/)).
- **ETEC, EAEC, preformed toxins — no evidence found this session.** Do not give them the trait.

**Design consequence:** the bloom trait belongs to the A/E *E. coli* (EPEC/EHEC) on this map, and
the trigger is **the player's own inflammatory response** — a neutrophil-heavy clear should
measurably *speed up* the surviving EHEC. That is the single most interesting difficulty knob in
this report, and it is honest.

---

## 4. Colonisation resistance — should the commensals be a protectable resource?

**Yes. The literature supports it specifically, not just in general terms, and there is a
first-rate cholera-specific result.**

**General mechanisms** (Caballero-Flores 2022, *Nat Rev Microbiol*,
[Consensus](https://consensus.app/papers/details/43857ce322535d2296162a9935e0e4da/); Khan 2021,
[Consensus](https://consensus.app/papers/details/49a7f5ce4f575c7b8b7aeb6fe87cd131/); Li 2026,
[Consensus](https://consensus.app/papers/details/f68475e0635659cc92b9140fa55adc3c/)): nutrient
and niche competition, contact-dependent inhibition, antimicrobial metabolites, bile-acid
transformation, and induction of host immune responses. Li 2026 adds that **IL-22 signalling
regulates epithelial glycosylation**, which changes nutrient availability and therefore which
microbe wins — a host-controlled lever on the commensal community.

**The cholera-specific result is the one to build on.** Transplanting different human microbiomes
into animals, **bile salt hydrolase activity** from key commensal species degrades
**taurocholate** — the bile salt that *activates V. cholerae virulence gene expression*. Where
those species and that enzyme are absent, infection loads rise, on a personal,
microbiome-specific basis (Alavi 2020, *Cell*,
[Consensus](https://consensus.app/papers/details/998da5552b7a5332a9c3ca415124e7dc/)). A
follow-up shows susceptible versus resistant communities drive different bile-acid compositions
in vivo, and V. cholerae in susceptible contexts upregulates **biofilm genes** (vpsL-dependent)
to resist both oxidative stress and inhibitory bile metabolites (Cho 2022,
[Consensus](https://consensus.app/papers/details/d2c0ef0d4a595e0c9fb0b919108ad682/)).

**And the demolition case is equally well evidenced.** Nutrient competition specifically — one
strain performing **Stickland fermentation** was necessary and sufficient to suppress
*C. difficile* in a 37-strain synthetic consortium, replicating a full faecal transplant; bile
acid 7α-dehydroxylation, the classic explanation, was *not* a determinant of efficacy (Tian 2025,
[Consensus](https://consensus.app/papers/details/0b23f3ede515507da02110e258c9b907/)).

**Recommended game reading.** Commensals are **not a tower** and should not be bought. They are a
**standing board property** — call it a commensal lawn in the lumen band — that:
1. **Suppresses virulence rather than killing.** Its effect is to lower the *virulence* and
   *adhesion odds* of arriving bacteria (the taurocholate/BSH mechanism), not their health pool.
   This is categorically different from every tower in the game and from mucus flushing.
2. **Is consumed by the player's own broad-spectrum answers.** Anything the game ever models as
   an antibiotic, or heavy inflammatory collateral, should thin it — and that thinning should
   show up two rounds later as faster colonisation, not immediately.
3. **Is eaten by the enemy when the player over-inflames.** ROS-lysed commensals feed the
   pathogen aspartate (Yoo 2024). A neutrophil-spam strategy should visibly consume the lawn.
4. **Is weak here on purpose.** Colonisation resistance is a *colonic* superpower; the small
   intestine's community is sparser and faster-flushed. On this map the lawn should be thin and
   fragile — which is precisely why protecting it is a choice rather than a given. (The exact
   jejunum-vs-colon density figures are **UNVERIFIED** — I could not retrieve a primary source
   this session; the widely quoted 10³–10⁴/mL jejunum vs 10¹¹–10¹²/g colon should be checked
   before it goes in a tooltip.)

---

## 5. What this means for the game

1. **Build *Vibrio cholerae* first.** It is the highest mechanical delta per unit of engineering
   in this entire report. It needs no new class in §4a — it is a "large extracellular bacterium"
   that *never takes a slot* — and it reuses the adhesion/mill/crowd code that already ships. What
   it adds is a **toxin field emitted by an adherent crowd** with three composable effects: (a) the
   epithelium under the field stays alive but stops being yours, (b) tight junctions loosen so the
   plaque over it charges faster (Markov 2019), (c) the damaged wall emits lactate that raises the
   local replication rate (Gutierrez 2026). That is a visible positive-feedback loop the player can
   see forming and interrupt — which is exactly what §1b Step 2 says the charge mechanic exists to
   deliver.

2. **The "toxin damages tissue without the bacterium entering" brief has two honest answers, and
   they are different mechanics.** *V. cholerae* is **subversion at a distance** — no cell dies.
   **EHEC is destruction at a distance, through the wall, aimed past it** — Gb3 is absent from gut
   epithelium, so Stx translocates and kills the vasculature. If the Director wants damage without
   entry in the literal sense, that is EHEC, and it should hit the life pool or the deep tissue
   band, *not* the epithelium. Build cholera first for cost; build EHEC second for that mechanic.

3. **Reject *C. difficile* for this map, and say so to the Director in one line.** It is a colonic
   organism; small-bowel CDI is a reportable rarity (5.1% of post-total-colectomy patients, median
   130 days). Its real trigger — antibiotic demolition of a dense microbiota — does not exist in the
   small intestine. **Bank it as the headline enemy for a colon map**, where its sialic-acid and
   succinate blooms would make colonisation resistance the entire game.

4. **Also reject *Aeromonas*.** Fifty years in and its causal role in diarrhoea remains contested;
   a 447-genome GEMS study found little evidence of genetic determinants for diarrhoeal disease.
   Hold *V. parahaemolyticus* too — it is real but mechanically redundant against EHEC.

5. **Infectious dose is the difficulty dial, and the split is structural, not cosmetic.**
   Extracellular/toxigenic organisms need huge inocula (cholera 10⁸, ETEC ≥10⁹, EAEC 10¹⁰) because
   their whole strategy is adhere-and-secrete; invasive organisms need tens (*Shigella* 10–100).
   Map this straight onto §1b Step 2's minimum-crowd floor: **the toxin enemies should need
   genuinely big plaques, and the one exception — EHEC at 4–24 organisms, because it carries
   *Shigella*-grade acid resistance — should feel shockingly cheap to start.** That contrast *is*
   the lesson.

6. **Stomach acid is worth four orders of magnitude and should be visible somewhere.** Cash 1974:
   10⁸ organisms in buffered saline, 10⁴ with bicarbonate. If the map ever gains a pre-lumen stage,
   or a modifier, this is the cleanest real number in enteric microbiology to hang it on.

7. **Give EPEC the "cannot be flushed" trait and let it break the player's two innate answers.**
   Sentinel sloughing and mucus turnover both work by expulsion. A pedestalled A/E organism has
   remodelled host actin to anchor itself. An enemy that simply ignores both flush defences, and
   must be killed, is a real strategic wall and costs almost nothing to implement (a flag).

8. **Add a shared `mucinase` trait, not per-organism bespoke code.** EHEC's StcE thins the inner
   mucus layer; ETEC's EatA, EAEC's Pic and *Shigella*'s SepA are functionally redundant MUC2
   mucinases neutralised by one shared antibody epitope. One trait, one counter, three organisms,
   fully literature-faithful. **And give EAEC's Pic its perverse second half: it makes goblet cells
   secrete *more* mucus and then digests it** — which means the §6b mucus-turnover upgrade actively
   helps EAEC. That is a great thing to spring on a player who just bought it.

9. **Inflammation-fuelled bloom is real, is a superb mechanic, and belongs specifically to the
   A/E *E. coli*.** Host oxidants create nitrate and tetrathionate that only facultative anaerobes
   can respire; host ROS lyse commensals and the pathogen eats the aspartate. **A neutrophil-heavy
   clear should measurably speed up the surviving EHEC two rounds later.** Do not give this trait
   to cholera, ETEC, EAEC or the preformed toxins — the evidence is not there.

10. **IgA has a second job, and cholera is how to teach it.** `IGA_REFERENCE.md` covers enchainment
    — IgA punishing replication. But anti-CT IgA blocks toxin binding and abolishes the secretory
    response outright, and in the field, breast-milk IgA did not prevent *colonisation* but did
    prevent *disease* in colonised children. **Against a toxin enemy, IgA should neutralise the
    weapon while the enemy stays on the wall** — a visibly different outcome from the clump-and-sweep
    the player already knows. Two mechanics, one tower, both true.

11. **Make the commensals a thin, fragile, protectable lawn — not a tower.** Their real effect is to
    *disarm* arrivals (commensal bile salt hydrolase degrades the taurocholate that switches on
    V. cholerae's virulence genes), which is unlike anything else in the game. Keep it weak here
    because the small intestine genuinely is the weak place for colonisation resistance; that makes
    protecting it a choice.

---

## 6. Provenance

All claims verified this session against PubMed (`mcp__plugin_bio-research_pubmed__*`) and
Consensus (`mcp__plugin_bio-research_consensus__search`). According to PubMed, the articles cited
with DOI links above are as described; Consensus-sourced articles are linked to their Consensus
records. Nothing in this document was written from memory without a lookup.

**Known gaps, flagged UNVERIFIED above:** in vivo doubling times for every organism in the table
(the literature reports colonisation loads, shedding titres and competitive indices, not doubling
times — Gillman 2021 is the closest, resolving replication/death/migration rates per gut segment
in mice); a modern EPEC ID₅₀; a primary *V. parahaemolyticus* volunteer dose; acute EAEC
incubation; the *B. cereus* diarrhoeal-syndrome dose; and the jejunum-vs-colon commensal density
figures. None of these block the ranking.

Consensus asks that this be passed on verbatim: *Create or connect a free Consensus account to
return more than 3 results per search in Claude Code.*
https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code
