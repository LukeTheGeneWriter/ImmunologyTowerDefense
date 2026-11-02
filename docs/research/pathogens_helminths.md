# Intestinal helminths — ranked shortlist, and the type-2 arm the game does not have

Research pass, 2026-11-02. Every factual claim below was checked against PubMed
(via the PubMed MCP tool) or Consensus **during this session**; nothing is
written from memory. Where a number is commonly quoted in textbooks but I could
not pull a primary or review source for it in this pass, it is marked
**UNVERIFIED** rather than filled in. Full citations with links are at the
bottom; inline markers are `[n]`.

Attribution: substantially based on articles retrieved from **PubMed** and from
**Consensus**. DOI links are given for the PubMed-sourced records.

**Helminth immunology is overwhelmingly mouse.** Every mouse-model finding below
carries an explicit **(mouse)** tag, and the model species is named
(*Heligmosomoides polygyrus*, *Nippostrongylus brasiliensis*, *Trichuris muris*,
*Trichinella spiralis* in mice, *Litomosoides sigmodontis*). Where a human
datapoint exists it is tagged **(human)**. This labelling is the point of the
pass, not a decoration: the previous pass found that "norovirus infects tuft
cells" is true of *murine* norovirus only, and the same trap is everywhere here.

Read against `docs/GAME_DESIGN.md` §1a (three lateral bands), §1b (adhesion /
mill / plaque / rupture), §1c (host-cell states, two-layer occupancy, debris),
§4a (pathogen classes — note the **parasite** row), §4b (stress signal, contact
stress-sense meter), §4c (the tower roster), §5 (knowledge ladder), §6b (barrier
colonisation and the mucus-turnover upgrade), §7 (two-resolution lattice, scale
honesty).

---

## 1. The shortlist, ranked

Seven ranked, three cut on anatomy or redundancy (§6). Ranked by **how much new
decision-making each forces on the player**, not by fame or lethality — the
roster rule. For each, the organism it would be redundant with is named in its
own block.

| # | Organism | The ONE mechanic nothing else on the board brings | Why it earns a slot |
|---|---|---|---|
| 1 | ***Strongyloides stercoralis*** | **It reproduces the wave.** The autoinfective cycle is real and is the organism's defining feature: rhabditiform larvae shed by parthenogenetic females in the small intestine can develop *in the gut* into infective filariform larvae and re-invade the same host, with no new dose arriving from the lumen [1][2][3]. Chronic infection persists for decades on this loop alone [1][5]; immunosuppression converts it to hyperinfection, where "large numbers of larvae pierce the bowel wall and disseminate" [1]. | Nothing on the board spawns. Every existing enemy's population is bounded by what the lumen delivers, which makes §1b's adhesion roll the game's master valve. An enemy whose *tissue* population feeds its own *luminal* population closes that loop and makes "clear it this round" a different proposition from "clear it eventually". It is also the only enemy for which the player's own successful immunosuppression (a Treg tower, §4c's open candidate) would be catastrophic. |
| 2 | ***Trichinella spiralis*** | **Two, and they compose.** (a) The intestinal L1 is **multi-intracellular**: the larva is ~1 mm long against 10–20 µm epithelial cells, so it "creates a syncytium by occupying a large number of cells at any one time", moving through the epithelium at the crypt–villus junction and leaving trails of dead cells behind it [8]. (b) The newborn larva **leaves the board** — released into the lamina propria, ≥97% travel by hepatic portal blood to striated muscle (rat) [14] — and there **converts a host cell into a permanent protective structure instead of killing it**: the nurse cell is a transformed, cell-cycle-arrested, senescent former myocyte whose "only role is to sustain muscle larva throughout the host's life" (mouse) [9], wrapped in a host-made type IV/VI collagen capsule driven by parasite ES products via TGF-β1/Smad3 (mouse) [10] and by M2 macrophage→myofibroblast activation (mouse) [11][12]. | (a) is §4a's parasite footprint **in the correct anatomy** — unlike *Trichuris*, which does the same thing in the caecum (§6). (b) is the permanent inverse of *Vibrio cholerae*: cholera subverts cells and **decays** when the secretors are cleared; the nurse cell is subversion that never decays, and the capsule is a structure the player's cells cannot enter. Together: an enemy that takes many slots at once, exits the board, and comes back as terrain. |
| 3 | **Hookworm (*Necator americanus*, *Ancylostoma duodenale*)** | **It feeds on the player's base.** The worm attaches to the small-intestinal mucosa with cutting plates (*N. americanus*) or two pairs of teeth (*A. duodenale*) [26], draws mucosa into its buccal capsule, ruptures vessels, digests the indrawn tissue and takes a new bite [25] — while secreting anticoagulant and anti-inflammatory polypeptides at the attachment point [26][27]. Measured blood loss: up to **180 mL and 33 mg iron per day** for a load of 834 *A. duodenale* + 249 *N. americanus*, and 30 mL/day for only 166 worms (human, ⁵¹Cr/⁵⁹Fe) [22]. | §1 makes the base the bloodstream. This is the only candidate anywhere in the research that **attacks the player's economy at the wall** rather than the wall itself — and the anticoagulant/immunomodulatory secretion makes the drain *locally un-stoppable*, a suppression aura with a sourced mechanism. It is also the one with a clean dose→effect curve to tune against [22]. |
| 4 | ***Ascaris lumbricoides*** | **It exits the board and returns changed.** The hepato-tracheal migration is what sets *Ascaris* apart from every other soil-transmitted helminth [38]: swallowed eggs hatch in the intestine, larvae leave it for liver then lung, then return to the intestine via trachea, oesophagus and stomach — a journey "commonly requir[ing] about ten days", during which they grow and undergo developmental change [39], from 0.22 mm to 1.4 mm in rodent models [40] **(mouse/rat)**. | The board currently has one spatial story: right to left. An enemy that leaves the map at the wall and re-enters later, larger, is a new axis — and it is the only sourced reason to draw the lumen→tissue→(off-board)→lumen loop the compartment model (§1) implies but never uses. Also the scale problem in person: female adults 20–40 cm [42], one documented at 20 cm × 5 mm diameter [43]. |
| 5 | ***Hymenolepis nana*** | **Bounded autoinfection — exactly one extra wave, then it stops.** *H. nana* is unique among *Hymenolepis* in completing its entire life cycle inside one mammalian host, developing **tailless cysticercoids in the intestinal tissue** (tailed ones in beetles) [57]. Egg ingestion → cysticercoid **in the intestinal tissue, not the lumen** → adult [58][59]. And the limit is sourced: after direct egg infection "the host becomes immune to reinfection with eggs within a few days", so eggs produced from day 12 onward cannot re-infect the same individual; a massive second generation occurs when the host was infected with *cysticercoids* — "however, in this case, there is no third generation" **(mouse models; human experimental studies are rare)** [59]. | The teaching version of #1. #1's loop is unbounded and therefore has to be a boss; this one is a *single* extra wave with a self-limiting rule, which is a round-sized mechanic rather than a campaign-sized one. The two together express the whole idea cheaply, and the acquired-immunity shut-off is §5's knowledge ladder doing real work on a real organism. |
| 6 | ***Anisakis simplex*** | **The damage is the player's own, and the second encounter is the dangerous one.** The L3 cannot mature in humans; it penetrates the gut wall, dies there, and leaves an eosinophil-rich granuloma — up to small-bowel perforation requiring resection, with "a remnant of an *Anisakis* larva" at the centre of the granulomatous nodule **(human case)** [66]. Re-infection causes systemic allergic reactions, urticaria or anaphylaxis **(human)** [67]; ~20 molecular allergens are characterised, including tropomyosin Ani s 3 **(human)** [94]. In fish the same larva is walled into a granuloma of epithelioid cells, fibroblasts and collagen [68]. | A dead-end invader: it cannot win, and it still costs you. That inverts §1b's whole premise (reaching the base is the threat). Its real mechanic is a **sensitisation counter** — clear it once and the board is fine; meet it again and the player's own type-2 response does the damage. No existing enemy makes the player's response the damage source except A/E *E. coli*, and that one *feeds* on the response rather than being the trigger for self-harm. |
| 7 | ***Taenia saginata*** | **An enemy that attacks income, not tissue.** Exclusively the human small intestine [55]; grows to ~5 m and lives up to **30 years** [54] (6–10 m in one morphology study [42]); scolex + a chain of immature → mature → gravid proglottids [55]. It adheres, does not invade, does not kill a slot, and does not advance. | Only worth a slot **if §5b's ATP income becomes a target**. Otherwise honest answer: it is *Vibrio cholerae*'s "wins without killing" with a bigger sprite — see §6. Named here because "drains the player's economy from a fixed point and cannot be killed by anything in the roster" is a decision no current enemy forces, and the 30-year lifespan is the sourced justification for an enemy that is *never* cleared, only managed. |

**Cut:** *Trichuris trichiura* (**rejected on anatomy — colonic**, exactly the
*C. difficile* precedent, and painfully, it has the single best mechanic found in
this pass); *Enterobius vermicularis* (caecal, and its distinctive behaviour
happens outside the body); *Taenia solium* (its interesting phase is
extra-intestinal); *Schistosoma mansoni* (adults live in mesenteric veins, not
the gut); *Capillaria philippinensis* (mechanically #1 with a better food story
and 10,000× less burden). Reasons in §6.

---

## 2. The scoping question: can a worm be a wave enemy at all?

**No. Not on any honest timescale, and not at any honest size.** But "boss" is
not the right answer either. The honest abstraction is a **two-layer enemy**, and
the game already has the mechanic it needs.

### 2.1 The timescale evidence

`GAME_DESIGN.md` §5d sets `FoodItemTransitSeconds` at 30 s and §1a's peristaltic
cycle at 7.5 s; the brief's working figure for a round is ~45 s. §4c's open list
records that **two research passes disagreed on what one round is in biological
time — 1 day versus 2 days.** Use that conversion, because it is the game's own,
and the comparison becomes arithmetic rather than rhetoric:

| Real event | Sourced duration | Rounds at 1–2 days/round |
|---|---|---|
| *Ascaris* hepato-tracheal migration, egg→back in gut | ~10 days (animal models) [39] | **5–10 rounds** |
| *Capillaria* larva→adult | 10–11 days [63] | 5–11 rounds |
| *H. nana* egg→egg-producing adult | ~12 days (mouse) [59] | 6–12 rounds |
| Hookworm skin contact→eggs in stool | **53 days mean** (human volunteers, 25 subjects) [31] | **27–53 rounds** |
| Hookworm egg output peak | 12–18 months post-infection [31] | 180–1,100 rounds |
| *Ascaris* adult life expectancy in man | **~1 year** (worm-ratio estimate, S. India) [44] | 180–365 rounds |
| *N. americanus* mean lifespan | 3–4 years (field estimate, W Bengal) [32]; ova detectable 64 months post-infection [31] | ~550–1,500 rounds |
| *A. duodenale* | ~1 year [32]; ova to **81 months** in volunteers [31] | — |
| *T. saginata* | up to **30 years** [54] | ~5,500–11,000 rounds |
| *S. stercoralis* | **decades**, via autoinfection [1][5] | whole campaigns |
| *T. spiralis* muscle larvae | months to decades [15]; nurse cell still maintained at 7 months (mouse) [9] | — |

The **fastest** helminth event anyone has measured is still 5 rounds long, and
the fastest *human* one — hookworm patency — is longer than the entire 40-round
script. A helminth has no sub-round behaviour to animate. There is nothing to
make into a wave.

### 2.2 The size evidence, which is worse

§7 pins the coarse slot to roughly one ~20 µm enterocyte. The tissue band is 50
slots ≈ **1 mm**; the whole 100-slot board ≈ **2 mm**. Converted:

| Stage | Real size | Coarse slots | Versus the board |
|---|---|---|---|
| *T. saginata* adult | 5 m [54] | ~250,000 | 2,500× the board |
| *Ascaris* adult female | 20–40 cm [42][43] | 10,000–20,000 | 100–200× the board |
| *Ascaris* adult **diameter** | 5 mm [43] | 250 | **5× the tissue band, in girth alone** |
| *S. stercoralis* parasitic female | 2,236 µm × 27 µm [6] | ~112 × 1.4 | 2× the tissue band |
| *T. spiralis* intestinal L1 | 1 mm [8] | 50 | exactly the tissue band |
| *S. stercoralis* egg | 70 × 44 µm [6] | 3.5 × 2 | fits |
| Larvae in rodent migration | 0.22 → 1.4 mm [40] | 11 → 70 | borderline → does not fit |

**Only eggs and the smallest larvae fit on this board at all.** An adult worm is
not a sprite problem, it is a category error: at board scale an adult *Ascaris*
is scenery the camera cannot contain.

### 2.3 The verdict, and the mechanic it should use

Build a helminth as **two layers**:

1. **Larvae are the wave units.** They are the only stage that fits a 20 µm slot,
   they are the only stage with sourced sub-round behaviour (penetration,
   migration, interception at the tissue by neutrophils and eosinophils —
   *S. ratti*, **mouse** [93]), and they are the stage the immune system
   demonstrably acts on. A "hookworm round" is a wave of L3 filariform larvae;
   the adult is **what you get if you fail**.
2. **The adult is persistent terrain, not a unit.** It does not walk toward the
   base. It anchors at the wall, accrues across rounds, and is removed by
   expulsion rather than damage.

**§6b already legalises this.** "Pathogens persist in the barrier space (depth 1,
mucus/glycocalyx) round to round, on a space-available basis… a standing
population accumulates there if the player doesn't manage it." That is an adult
worm burden, described in a locked section, two years before anyone wrote a worm.
And §1b already implements expulsion: "the adhered population is carried slowly
downstream by the mucus… an adherent that reaches the far end rejoins the flow
and is excreted."

So the honest shape is: **larvae as a wave → an adult that becomes a §6b barrier
resident with a multi-slot anchor → cleared only by turning up the existing
conveyor.** And the autoinfection loop (#1, #5) is then exactly one new rule: a
§6b barrier resident that *re-seeds the larval wave*. That is a spawner — the
thing nothing on this board does — expressed as one edge added to mechanics that
already ship.

A boss is the right frame for *Strongyloides* specifically, because its loop is
unbounded and needs several rounds to read. It is the wrong frame for hookworm,
*Ascaris* and *Taenia*, which are not encounters at all. They are weather.

---

## 3. Numbers

Burden first. **The headline estimates disagree by an order of magnitude for
*Strongyloides*, and that disagreement is itself the finding** — do not pick one.

| Organism | Global burden | High-income / age skew | Source |
|---|---|---|---|
| *A. lumbricoides* | **819.0 M** infected, 2010 (95% CrI 771.7–891.6 M); 22% of STH YLDs | Prevalence greatest in young children; "returning immigrants in industrialised nations" is the named high-income route [45]. Age-intensity profile **convex**, peak worm burden at **5–9 years**, with individuals *predisposed* to re-acquire heavy burdens after cure [44] | [36][44][45] |
| Hookworm (both spp.) | **438.9 M** infected, 2010 (406.3–480.2 M); **65% of all 4.98 M STH YLDs** — the largest single contributor | Rate in high-income settings: **UNVERIFIED** in this pass | [36] |
| *T. trichiura* | **464.6 M** infected, 2010 (429.6–508.0 M); 13% of YLDs | — | [36] |
| All STH (GBD 2021) | **642.72 M** cases, 1.38 M DALYs, ASPR 8,429.89/100k; "an estimated 1.5 billion people" in the framing text | Peak 5–19 y, highest 5–9 y (ASPR 16,263/100k); prevalence falls as SDI rises (r = −0.88) | [37] |
| *S. stercoralis* | **Three incompatible figures in current literature:** 30–100 M [3]; **300–600 M** [1]; "over 600 million" [4] | — | [1][3][4] |
| *E. vermicularis* | **Also contested:** pooled global prevalence in children **12.9%** (95% CI 8.2–17.7, 42 datasets, I² = 99.96%) [52] versus "approximately 30% of children worldwide and up to 60% in some developing countries" in a 2024 review [51]; "about 200 million" infested, children 5–10 y >30% of cases [50] | Highest prevalence **5–14 y** [51]; day-care, pre-school and primary-school settings [50]. 22.4% in Marshall Islands kindergartens, higher in >5 y than ≤5 y [53]. 1.9% in one Iranian kindergarten survey [52-adjacent] — **high-income-country rate specifically: UNVERIFIED** | [50][51][52][53] |
| *H. nana* | Most prevalent cestode worldwide [58] | 7.61% of 14,761 children 2–15 y, N Peru [60]; **27.08%** in displaced children, Mogadishu, with fingernail-biting an independent risk factor (AOR 3.25) [62]; **Australia (high-income): 461 episodes / 68,387 faecal samples over 12 y, median age 3.0 y, 98.9% Indigenous, 44.3/10,000 in Indigenous children <5 y in 2013** [61] | [58][60][61][62] |
| *Trichinella* spp. | **65,818 cases and 42 deaths, 1986–2009, 41 countries**; WHO European Region 87% of cases, Romania alone 50%; regional incidence 1.1–8.5/100,000 [17]. Global DALYs **76 per billion persons/year** (95% CrI 38–129) — "much lower than other foodborne parasitic diseases" [18] | Median age 33.1 y, men ≈ women [17]. A 2026 meta-analysis reports a pooled prevalence of 18% across 127 studies / 425,225 people [19] — **treat with suspicion: it pools symptomatic and high-risk cohorts (military personnel 41.47%), and is irreconcilable with [17] and [18]** | [17][18][19] |
| *C. philippinensis* | 348 patients in 32 studies, 2000–2025; **5.0–23.0% in symptomatic/high-risk groups but <0.01% in community screening** [64] | — | [63][64][65] |
| *T. saginata* / *T. solium* | Global distribution [55]; taeniasis 49.9% of 389 subjects in one co-endemic Sichuan village, *T. solium* higher in children <16 y (8.8%) than older (2.5%) [56] | — | [55][56] |

Life-history numbers for tuning:

| Quantity | Value | Source |
|---|---|---|
| Hookworm blood loss | **0.05–0.2 mL/worm/day**, mean daily loss 26.4 mL in adults with *A. duodenale* [23]; 0.1–0.4 mL/worm/day, up to 250 mL/day in heavy infection [24]; measured maxima 180 mL + 33 mg Fe/day at ~1,080 worms, 30 mL/day at 166 worms [22] | [22][23][24] |
| Hookworm worm loads recorded | up to 850 adult *A. duodenale* and **2,400** *N. americanus* per person [22] | [22] |
| Hookworm species asymmetry | *A. duodenale* bleeds **more** per worm; *N. americanus* is smaller, lays fewer but larger eggs, is more resistant to vermifuges, and sits in the villi; *A. duodenale* can infect orally *and* through skin | [22][30] |
| Larval penetration efficiency (field) | *N. americanus* 17.5%, *A. duodenale* 5.3%; ~252 infective larvae contacted per child per transmission season | [32] |
| *E. vermicularis* eggs per gravid female | up to **15,000**, laid perianally, mainly at night | [50] |
| *E. vermicularis* infestation without autoinfection | **4–6 weeks only** — the short adult lifespan is why recurrence is autoinfection, not persistence | [50][51] |
| *T. spiralis* intestinal L1 size vs host cell | 1 mm larva, 10–20 µm epithelial cells | [8] |
| *S. stercoralis* parasitic female | 2,236.55 µm × 27.25 µm; egg 70.42 × 44.24 µm; **all adults recovered were female** (parthenogenesis), thousands expelled after one 400 mg albendazole dose [6] — the paper explicitly contradicts earlier literature on egg size and male presence, so treat as one observation, not a consensus | [6] |
| Infective dose | **UNVERIFIED for every organism in this pass.** No helminth ID50 equivalent was located. Note that worm burdens are famously **aggregated/overdispersed** with named *predisposition* of individuals to heavy burdens [44], which is a better model than a dose threshold anyway | [44] |
| *Ascaris* fecundity | density-dependent — fecundity falls with worm burden [44] | [44] |
| *T. spiralis* newborn larva size, *Ascaris* prepatent period in humans, *T. trichiura* adult lifespan, hookworm adult length in mm, *H. nana* adult length | **UNVERIFIED** | — |

---

## 4. Per-candidate detail

### 4.1 *Strongyloides stercoralis* — rank 1

**The one mechanic.** In §1b/§4a vocabulary: **invades** (filariform larvae
penetrate the mucosa from inside the gut), does **not** take a host-cell slot as
a disguise — the adult female lives in the small-intestinal mucosa and
reproduces by parthenogenesis, eggs hatching **inside the host** before
rhabditiform larvae are excreted [2]. The defining rule: a fraction of those
rhabditiform larvae develop **directly into infectious filariform larvae in the
gut** and cause autoinfection [2]. Mechanically that is *a §6b barrier resident
that writes into the §1b spawn stream*. **Produces debris** (the duodenal biopsy
shows rhabditiform larvae within mucosal crypts and 45–50 eosinophils per HPF in
the lamina propria **(human)** [7]), **emits no §4b stress signal** — it is not
intracellular, so the contact stress-sense meter has nothing to read. Band:
seeded at the **gut interface**, resident in **tissue**, re-seeding the **lumen**.

Second state, for a boss's second phase: **hyperinfection**. Immunosuppression —
corticosteroids especially, also HTLV-1 and chronic alcohol — accelerates
autoinfection into "massive larval proliferation, dissemination" with larvae in
sputum as well as stool, and death [1][3][4][107]. Mechanically a rate
multiplier on the spawn edge, triggered by the player's own suppression.

**Morphology.** Parasitic female **2,236 µm × 27 µm** = **~112 × 1.4 coarse
slots** [6]. It cannot be drawn as a unit. The silhouette that reads at 20 px is
the **larva**: a smooth, blunt-tailed, almost featureless thread (a capsule with
a slight tail flare), deliberately plain so that its *behaviour* — appearing from
tissue rather than from the lumen — is the readable thing. For the adult, draw
the **lesion**: a multi-slot inflamed burrow at the interface with eosinophil
stipple, with larvae emerging. The egg (70 × 44 µm ≈ 3.5 × 2 slots) is drawable
as a discrete object and is the only life stage that is genuinely sprite-sized.

**Exposure route.** Walking barefoot on soil contaminated with human faeces —
skin penetration by filariform larvae is the route of infection of the definitive
human host [2]. Round callout: *"bare feet, wet ground, a latrine upstream."*
The *second* callout is better and is what makes it a boss: *"the steroids you
prescribed for his rash."*

**Burden.** 30–100 M [3] / 300–600 M [1] / >600 M [4] — the pass's largest
unresolved number. Most prevalent in SE Asia and the Western Pacific; global
prevalence "largely unknown due to limited surveillance" [2]. In high-income
settings it is a **latency** disease: an 86-year-old former tropical physician
with eosinophilic pneumonia decades after exposure [5].

**The player's counter — nothing in the roster can do this, and that is the
point.** Antibody (§4c, §5 ~20%/~70%) coats and pacifies but "contains rather
than kills" (`IGA_REFERENCE.md`'s own conclusion) — correct for a worm and
insufficient. CTLs are gated on MHC-I presentation by an *infected cell*, and
there is no infected cell. The γδ IEL is the closest existing answer because it
patrols the wall and intercepts adherents, and it is the only tower that repairs
the barrier the larvae tear. **What is actually needed is a tower that raises
expulsion rate** — see §5. For the autoinfection loop specifically, the counter
is not killing larvae faster; it is **cutting the re-entry edge**, i.e. the
mucus/turnover stack acting on larvae emerging from tissue, which is a new
direction for an existing mechanic.

**Redundancy check.** Redundant with nothing. The nearest comparison is
*Salmonella*'s brood burst (§4b) — but a brood is bounded by one host cell's
replication count and resolves in seconds; this is an unbounded standing loop.
*H. nana* (#5) overlaps deliberately as the bounded teaching version.

---

### 4.2 *Trichinella spiralis* — rank 2

**The one mechanic (intestinal).** **Multi-intracellular occupancy.** The L1
invades columnar epithelial cells in the **small intestine** and, being 1 mm long
against 10–20 µm cells, occupies a large number of cells simultaneously as a
syncytium, migrating through the epithelium — most often at the crypt–villus
junction — and leaving **trails of dead cells** behind it [8]. In §1c terms this
breaks the two-layer model: the host layer holds one cell-state per position and
the occupant layer is for *extracellular* things, and this organism is inside
many host cells at once. It **kills** as it passes (debris trail), and it moves
**along** the wall rather than toward the base, which is a movement rule no class
has. §1c's own text says the two-layer split exists partly because single-occupant
grids make "the parasite class (multi-slot footprint) hard to add" — this is the
organism that proves two layers are still not enough.

**The one mechanic (tissue).** **Permanent subversion.** Newborn larvae are
released into the lamina propria and ≥97% travel via **hepatic portal blood**
(not lymph: <3% from thoracic duct) to striated muscle **(rat)** [14]. There each
larva converts a single host myocyte into a **nurse cell**: non-muscular in type,
cell-cycle-arrested, expressing senescence markers p16^INK4a, p21^Cip1 and
p57^Kip2, maintained for the host's life — still present and upregulated at a
7-month infection **(mouse)** [9]. Around it, a host-built collagen capsule:
parasite ES products drive type IV and VI collagen via TGF-β1/Smad3 **(mouse,
in vivo + myoblast in vitro, reversed by the TGF-βRI inhibitor SB525334)** [10],
and CD206⁺ M2 macrophages drive α-SMA⁺ myofibroblast activation and type I
collagen; macrophage depletion reduces capsule deposition **(mouse)** [11][12].
Most of the host myocyte transcriptional response happens at **1 day**
post-infection and then quiets, with only 11 genes consistently changed across
all five timepoints to 15 days **(mouse)** [13].

So: **the host cell is not killed, it is repurposed, and the player's own
macrophages build the wall that protects the parasite.** That is a sourced
mechanic in which clearing with innate cells makes the problem permanent — the
same inversion A/E *E. coli* achieved, by a completely different route, and the
roster doc flags A/E's inversion as its best single idea.

Immune evasion worth one line each, all with mechanisms the game already models
or could: larval **Plancitoxin-1 degrades NET DNA**, freeing the larva from NETs
[15] (the roster's "NET degradation" gap — flagged there as having *no* mechanic
to disable); **TsCRT binds C1q** and blocks the classical pathway [15] (bears
directly on the complement body-upgrade re-spec); **TsMIF** mimics CD74
engagement and flips macrophages to tolerant [15]; extracellular vesicles cross
the capsule to suppress Th1/Th17 and expand Th2/Treg systemically [15]. Caveat:
[15] is a 2026 narrative review in a low-profile venue — treat each claim as a
pointer to its primary source, not as established.

**Morphology.** Intestinal L1: **1 mm = 50 coarse slots**, i.e. the exact width
of the tissue band. Draw it as a **threaded worm**: a chain of 20–50 capsule
segments following a path through the cell lattice, with the cells it currently
occupies tinted and a trail of debris behind — the sprite *is* the footprint, and
it reads at 20 px because what you see is the **track**, not the animal. The
muscle stage is the other half and is genuinely iconic at 20 px: a **tight coil
inside a lens-shaped capsule** (two arcs, one spiral), which is also the single
most recognisable helminth silhouette in medicine. If the tissue phase is built,
draw the nurse cell as a host cell that has gone pale and glassy with a coil
inside it — visually a cell that is alive and no longer yours, the cholera
treatment taken to its end state.

**Exposure route.** Undercooked pork historically; now mostly **wild game** —
wild boar is the most frequently implicated species in human outbreaks globally,
with bear meat important in North America and implicated in multi-country
outbreaks through meat importation [20][21]. Round callout: *"the hunter's bear
sausage, cured but never cooked."*

**Burden.** 65,818 cases / 42 deaths across 41 countries 1986–2009; 87% in the
WHO European Region, half of those in Romania in the 1990s; 1.1–8.5 per 100,000
regionally; median age 33.1 y, men ≈ women [17]. Global DALY burden **low** — 76
per billion person-years — and explicitly "in sharp contrast to the high budget
allocated to prevent the disease in many industrialised countries" [18].
Commonest symptoms: myalgia, fever, facial oedema [17][19]. **Rare, with a
high-income footprint, which is exactly the profile of a boss rather than a wave
filler.**

**The player's counter.** Partially answered, and that is good design territory.
Intestinal expulsion in mice requires **mast-cell chymase mMCP-1**: mMCP-1⁻/⁻
mice show significantly delayed *T. spiralis* expulsion and increased muscle
larval deposition **despite normal or increased mast-cell numbers** — and notably
mMCP-1 loss did **not** affect *Nippostrongylus* burdens, so this is
worm-specific, not generically type-2 **(mouse)** [86]. Mast-cell-deficient
W/W^v and mMCP-1⁻/⁻ mice both expel late **and** have less enteropathy — i.e. the
effector that expels the worm is also what damages the gut **(mouse)** [87]. IgE⁻/⁻
mice are slower to clear adults, carry ~2× the muscle larvae, and the larvae are
less often necrotic **(mouse)** [88]. Muc5ac⁻/⁻ mice show significantly delayed
expulsion of *T. spiralis* too **(mouse)** [78].
Against the muscle stage the roster has **nothing**, and should not pretend
otherwise — the capsule is a structure, and §4c's cells have no mechanic for
attacking terrain.

**Redundancy check.** The multi-slot intracellular footprint is redundant with
*Trichuris trichiura* — which is cut on anatomy, leaving this organism the sole
carrier of the mechanic **in the small intestine**. The nurse cell is the inverse
of *V. cholerae*'s subversion, not a copy: cholera's decays, this one does not.
The exit-and-return is weaker than *Ascaris*'s (#4) because it does not come back.

---

### 4.3 Hookworm (*Necator americanus*, *Ancylostoma duodenale*) — rank 3

**The one mechanic.** **Adheres** (never invades, never takes a host-cell slot,
never goes intracellular) at the gut interface and **drains the base**. The
feeding cycle is mechanical and documented frame by frame: mucosa is sucked into
the buccal capsule, detached epithelium and blood from ruptured vessels pass into
the worm's gut, surrounding lamina propria is drawn toward the orifice, the
indrawn tissue is digested, and **a new bite is taken** [25]. Attachment is by
cutting plates (*N. americanus*) or two pairs of teeth (*A. duodenale*), and the
worm simultaneously secretes polypeptides that **prevent clotting and suppress
host inflammation** at the site [26]. Hookworm ES products are a large,
well-catalogued arsenal implicated in invasion, tissue migration, nourishment and
**immune modulation** [27], transcriptionally **upregulated specifically in the
adult intestine exposed to host blood** — 1,951 positively immunoregulated genes
in 19-day adult intestinal tissue versus almost none in young adults or
non-intestinal tissue, comparing normal to immunosuppressed hosts **(hamster,
*A. ceylanicum*)** [28].

Game translation, all from the mechanism: (1) a **moving bite point** — the worm
detaches and re-attaches, and §4c's γδ IEL already "converges on the enterocyte
bacteria are *currently* touching", so a target that moves is a real test of that
behaviour; (2) **a wound that does not close** while the worm is attached
(anticoagulant), i.e. a damage-over-time that repair cannot out-pace, unlike
every other wall damage source; (3) **a local suppression field** around the
attachment, reducing nearby tower effectiveness — and this one is *local and
interruptible*, the shape the roster doc prefers over flat debuffs; (4) **the
drain goes to the player's life/ATP pool**, not the wall, because §1 makes the
base the bloodstream.

**Morphology.** Adult length in mm: **UNVERIFIED** in this pass; sourced only
relatively — *N. americanus* is smaller than *A. duodenale*, produces fewer but
larger eggs, is less virulent, and sits in the villi [22][30]. Even at the
commonly cited ~1 cm that is ~500 coarse slots, so again not a unit. The
silhouette is the best one available: a **hooked anterior** — a body that bends
sharply back on itself at the head end, which is the genus's name and reads at
20 px as a question mark. Two discs for the buccal capsule, a notch for the
cutting plates. What the game should actually draw at board scale is the
**attachment**: a multi-slot anchored head at the interface, a bleeding crater,
and a body that leaves the frame into the lumen. The L3 filariform larva is the
wave unit and is the drawable stage.

**Exposure route.** Barefoot contact with faecally contaminated soil; *A.
duodenale* can also infect **orally** [30], which gives a second callout. Round
callouts: *"barefoot in the wet season, in a village with 31% of households
drawing water from the river"* [32]; or for the oral route, *"unwashed lemon
basil leaves from the fried-chicken stall"* (a sourced STH-on-raw-vegetables
vehicle, albeit the eggs recovered in those surveys were *Ascaris* [and hookworm
was not found], so use it for #4 if strictness matters).

**Burden.** **438.9 M** infected in 2010 and **65% of all soil-transmitted
helminth years-lived-with-disability** — the single largest morbidity
contributor among STH [36]. Peak prevalence 5–19 y across STH, highest 5–9 y
[37], though hookworm classically skews older than *Ascaris*; worm loads to
2,400 *N. americanus* per person [22]. High-income rate: **UNVERIFIED**.

**The player's counter.** **Nothing in the roster answers a blood-feeding
anchor.** A macrophage or neutrophil can deliver combat damage to an
extracellular target, which is what §4a prescribes, but the organism is ~500
slots of cuticle and the immunology says damage is the wrong idiom entirely —
helminths "present to the host's defenses large, non-phagocytosable surfaces",
and cytotoxic lymphocytes are inactive against them [69]. The honest answer is
the expulsion tower in §5, plus the γδ IEL's repair racing the anticoagulant.
Human controlled-infection data say what *does* happen: 50 L3 in four naive
volunteers produced increased plasmacytoid DC frequency with reduced TLR7/8
responsiveness and expanded CD45RA⁺ Tregs **(human)** [34]; a one-year controlled
infection produced transient eosinophilia, serum IL-5 and faecal eosinophil
degranulation markers, with a measurable **fall in small-bowel pH** and no change
in microbial community composition **(human)** [35]; naturally mono-infected
adults show reduced T and B cell percentages, more *activated* CD4⁺ and CD19⁺
cells, lower proliferative responses — especially to L3 antigen — and adult-worm
ES antigen suppressing TNF-α and IL-10 from their PBMCs **(human)** [33].

**Redundancy check.** Its *adhesion-without-invasion* profile is redundant with
*V. cholerae*. Its *drain target* is not: cholera subverts the wall's output;
this takes the player's blood. If the economy is never a target, this collapses
to "a tough adherent" and should be cut — the bigger-in-one-named-dimension
defence applies only if **income** is that dimension.

---

### 4.4 *Ascaris lumbricoides* — rank 4

**The one mechanic.** **It leaves the board.** Swallowed embryonated eggs hatch
in the intestine; larvae do not settle but leave for the liver, then lungs, then
return to the intestine via trachea, oesophagus and stomach, "commonly
requir[ing] about ten days" and growing and developing en route [39]; in rodent
models larvae are in liver and lung by day 2–4, gone from liver after day 5, in
bronchi by ~day 7 and trachea by ~day 8 **(rat/mouse)** [40]. The hepato-tracheal
migration is what distinguishes *Ascaris* from the other STH [38], and the
larvae are **killable during it**: in mice, house-dust-mite pre-sensitisation
drives an eosinophil-rich pulmonary type-2 response that directly arrests larval
development and markedly cuts lung parasite burden, and this is **abolished in
eosinophil-deficient mice** **(mouse)** [46].

Game translation: an enemy that **despawns at the wall and re-enters N rounds
later, one tier larger**, with an **off-board window in which it can be
intercepted if the player has invested in the right thing**. That is a genuinely
new decision — spend now on something that does nothing this round, to kill a
thing you cannot see. Note that the one organism in the roster with a
"disappears and comes back" feel is CMV (reactivation), and that is latency, not
a round trip.

Adult phase in §1b vocabulary: **adheres to nothing** — it lives free in the
lumen. It does not take a slot, does not kill a cell, does not emit a stress
signal. Its pathology is **obstruction and migration** [45], which on this board
is a lumen-flow mechanic, not a tissue mechanic: an enemy that blocks the
channel, which would change the arrival rate of everything else in the round.

**Morphology.** Adult female **20–40 cm** [42], one case **20 cm × 5 mm**
diameter [43] — in coarse slots, **10,000–20,000 long by 250 wide**. The
diameter alone is five times the tissue band. **This cannot be drawn at board
scale in any form.** What the game should draw: the **larva** as the wave unit
(0.22–1.4 mm, 11–70 slots — still oversized, so draw the small end), and the
adult as a **lumen obstruction**: a cream-coloured mass crossing the channel,
tapered at both ends [43], three lips visible as a trefoil at the head on close
inspection [41], thick transverse cuticular striation as the texture [41]. At 20
px: a pale tapered band with a trefoil dot. Accept that the adult is scenery.

**Exposure route.** Ingestion of embryonated eggs from faecally contaminated
soil, including on raw vegetables — *A. lumbricoides* eggs were the **only** STH
recovered from fresh market and food-stall vegetables in several Indonesian
surveys (22% of traditional-market cabbage, 11% of food-stall cabbage; 10.7% of
84 fresh vegetable samples; lemon basil most contaminated) [see §7 "also
consulted"]. Round callout: *"the free side salad, rinsed in the same bucket all
morning."* High-income framing: returning immigrants and travellers [45].

**Burden.** **819.0 M** in 2010, the largest of the three STH by headcount, 22%
of STH YLDs [36]. Peak worm burden at **5–9 years**, aggregated distribution,
and **predisposition**: individuals with heavy burdens re-acquire heavy burdens
after treatment [44]. Fecundity is density-dependent [44]. Life expectancy in
man **~1 year** [44].

**The player's counter.** The migration window is the only place the roster could
act, and the sourced effector there is the **eosinophil** — in mice, and only
under allergic pre-sensitisation [46]. In humans the evidence is association, not
mechanism, but it is real: Nigerian children putatively immune to *Ascaris* had
higher serum ferritin, CRP and **eosinophil cationic protein**, and among
children making anti-ABA-1 IgG, the immune group had significantly more **IgE**
to ABA-1 than the susceptible group — while IgG in all subclasses was high in
most children and "appeared to have no protective function" **(human)** [96]. In
Caracas slum children, high pre-treatment *specific* anti-*Ascaris* IgE predicted
*not* being reinfected, while high **total** IgE predicted reinfection — the
authors' reading being that polyclonal IgE stimulation dilutes effective specific
IgE **(human)** [95]. That is a beautiful game mechanic as it stands: **the
antibody response has a signal-to-noise problem, and the parasite creates the
noise.** It maps onto §5c's barcode and §5's mutation discount without inventing
anything.

**Redundancy check.** The exit-and-return is unique. The scale is unique and is
*not* a mechanic on its own — "it is just bigger" is the cut criterion, and
*Ascaris* survives only because the bigness has a named dimension: it obstructs
the lumen channel, which changes every other enemy's arrival. If lumen
obstruction is not built, rank it below #5.

---

### 4.5 *Hymenolepis nana* — rank 5

**The one mechanic.** **Autoinfection with a sourced off-switch.** Egg ingestion
→ oncosphere invades the **intestinal tissue** (explicitly tissue, not lumen,
"which is very different in terms of the host's immune system") → **tailless
cysticercoid** → adult in the lumen [57][58][59]. *H. nana* is the only
*Hymenolepis* that does this in a mammal; the others only form tailed
cysticercoids in beetles, or in experimentally immunodeficient rodents [57]. And
the bound: after direct **egg** infection the host "becomes immune to reinfection
with eggs within a few days", so eggs produced from day 12 onward cannot
re-infect that host; after **cysticercoid** infection a massive second generation
develops — "however, in this case, there is no third generation" **(mouse models;
human experimental studies rare)** [59].

Game translation: **the wave that spawns exactly one more wave, and whose
off-switch is knowledge.** A §5 knowledge threshold that *mechanically terminates*
a spawn loop is a far better demonstration of the ladder than a stat bump, and
it is literally what the immunology says. It also gives the Director a cheap way
to ship the autoinfection idea before committing to *Strongyloides*'s unbounded
version.

**Morphology.** Adult length **UNVERIFIED** (the name is the only size claim I
can source: "dwarf"). Cestode silhouette at 20 px is the easiest in this
document: a **scolex disc plus a tapering chain of trapezoid proglottids**, each
segment a flat polygon, progressively larger toward the tail — immature, mature
and gravid segments are morphologically distinct stages along the chain [55], so
the sprite can show its own age. For the tissue phase, draw the **cysticercoid
in a villus**: a small sphere inside a host structure, which is also the frame
the game would reuse for *Trichinella*'s nurse cell.

**Exposure route.** Faecal-oral, with the most specific sourced risk factors in
this whole pass: **fingernail-biting** (AOR 3.25), playing on unclean ground
(AOR 3.92), large household (AOR 6.96) and unwashed fruit and vegetables (AOR
8.16) [62]; lack of household water (aPR 2.22) and sanitation (aPR 1.94) [60].
Accidental ingestion of **fleas or beetles** carrying cysticercoids, or of eggs
from rodent faeces or a carrier's hands, often contaminating kitchens [59]. Round
callout: *"a bitten fingernail, after a morning on the ground."*

**Burden.** The most prevalent cestode in the world [58]. 7.61% of 14,761 Peruvian
children 2–15 y, a likely underestimate from single-sample microscopy [60];
27.08% in Mogadishu IDP camps, males > females [62]. **High-income setting, with
an unusual twist:** in Australia's Northern Territory, 461 episodes from 68,387
faecal samples over 12 years, **median age 3.0 y**, 98.9% Indigenous, 18.2%
anaemic and 39.6% eosinophilic — and the rate was **flat across 12 years of
deworming programmes** (44.3/10,000 in Indigenous children <5 y in 2013 versus
46.1 in 2002), because albendazole does not eradicate it [61]. A hard enemy that
is unmoved by the standard answer, in a high-income country, with an age skew to
toddlers.

**The player's counter.** Acquired immunity to the egg stage is the counter and
it is the organism's own documented limit [59] — so this is the one helminth
whose real-world answer is already in the game's vocabulary (§5 knowledge). Note
the open question [59] raises honestly: nobody knows how the life cycle persists
given such a short parasite lifespan, with immunodeficient hosts and longer-lived
arthropod intermediates both proposed.

**Redundancy check.** Deliberately redundant with #1, as the bounded version. If
only one autoinfection enemy ships, ship this one first and *Strongyloides* as
the boss that removes the bound.

---

### 4.6 *Anisakis simplex* — rank 6

**The one mechanic.** **A dead-end invader whose cost is the player's own
response.** The L3 cannot mature in a human. It penetrates the gastric or
intestinal wall, where — if not removed — it provokes "massive infiltration of
eosinophils and formation of granulomas" **(human)** [67]; intestinal
involvement is less common than gastric but can perforate, with resection
required and pathology showing granulomatous tissue, necrosis, eosinophilic
infiltration and the larval remnant at the centre **(human case)** [66]. In fish
the identical outcome: a granuloma of epithelioid cells, fibroblasts and collagen,
with the response *more* intense around calcified (dead) larvae than live ones
[68]. **Re-infection drives systemic allergic reactions — urticaria, anaphylaxis
— making it a hidden allergen in seafood** [67], with ~20 characterised molecular
allergens including tropomyosin Ani s 3 and Ani s troponin [94]. Occupational
exposure alone sensitises, causing dermatitis, rhinoconjunctivitis or asthma
[67].

Game translation, in §1b/§1c terms: **invades** (a true wall breach by a single
organism, not a crowd — which is a deliberate exception to §1b's "ingress is
something a CROWD does"; see §7 corrections), **kills** the slots it crosses,
**dies on its own**, and leaves a **permanent multi-slot lesion** (debris that
does not clear, since the granuloma is organised fibrosis, which is §6's
territory). Then it sets a **sensitisation flag on the species**: the next time
that species appears, the player's own type-2 response does the damage.

That is the first enemy whose threat is *the player's accumulated knowledge*.
§5's whole arc is that knowledge is good. One enemy for which a high knowledge
value is a liability is worth a great deal, and it is honest: this is what an
allergic response is.

**Morphology.** L3 size **UNVERIFIED** in this pass (commonly ~2 cm; not
sourced). The silhouette is a **tight flat coil** — anisakid L3 in fish flesh are
the classic watch-spring spiral — which at 20 px is two or three concentric arcs,
and is distinct from *Trichinella*'s encapsulated coil by having no capsule. Draw
the **granuloma** as the board object: a pale fibrous ring with a dark coil at the
centre, occupying 2–4 slots, permanent.

**Exposure route.** The most vivid in the pass: **raw, pickled or undercooked
marine fish** — sushi, sashimi, pickled anchovies — with incidence rising in
Western countries on the back of Japanese cuisine and shifting parasite
distributions under climate change [66][67]. 88% of 41 Atlantic mackerel from the
Bay of Biscay carried *A. simplex* L3, 5–650 larvae per fish, mean ~87 [68].
Prevention is a freezing rule: −20 °C to the core for 24 h [67]. Round callout:
*"the mackerel was fresh, which is exactly the problem."*

**Burden.** Incidence "increasing in Western countries" [66]; no global figure
located — **UNVERIFIED**. This is the candidate with the clearest **high-income
skew** of any organism in the pass, which is a real roster asset given that
everything else here is a poverty disease.

**The player's counter.** Partially in-roster: the γδ IEL repairs the breach, the
macrophage clears debris. The granuloma is not answerable. The sensitisation is
answerable only by *not escalating* — which is a decision, and the only one of
its kind.

**Redundancy check.** The single-organism breach overlaps *Shigella*'s (cut
list) low-dose invasion, but *Shigella* goes on to spread and this one dies.
The permanent lesion overlaps *Trichinella*'s nurse cell; it differs in being
built by the host **against** the parasite rather than **for** it. The
sensitisation is unique.

---

### 4.7 *Taenia saginata* — rank 7, conditional

**The one mechanic.** **Immortal, inert, and it eats your income.** Adults live
exclusively in the human small intestine [55], reach ~5 m, and live **up to 30
years** [54]. It attaches by a scolex, grows a chain of proglottids, and does
none of the things §1b describes: no invasion, no slot, no kill, no stress
signal, no advance. In §4a vocabulary it is an **adherent that cannot be removed
by any mechanic in the game** and whose only output is consumption.

The conditional: this is a slot only if **§5b's ATP income is a legitimate
target**. If it is, *T. saginata* is the organism that justifies an enemy the
player manages for the entire campaign and never clears — which is a tower-defence
shape (an escalating upkeep) the game does not have. If income is never a target,
cut it: see §6.

**Morphology.** 5 m = ~250,000 coarse slots [54]. Draw the **scolex and the
first few proglottids** anchored at the wall, with the chain receding out of
frame — the honest picture of a worm on this board. At 20 px: a small square head
with four dots (suckers) and a run of flat trapezoids. Gravid proglottids differ
measurably from mature and immature ones in cytoskeletal and glycolytic protein
expression [55], so segment-age shading is defensible.

**Exposure route.** Raw or undercooked **beef** containing cysticerci (<1 cm
bladder worms) [54]. Round callout: *"steak tartare, from a cow nobody
inspected."*

**Burden.** Global distribution, a public-health and livestock-economy problem
[55]. A precise global figure was not located — **UNVERIFIED**. In one co-endemic
Sichuan village 49.9% of 389 subjects had taeniasis across three *Taenia*
species, with *T. solium* skewing to children <16 y (8.8% vs 2.5%) and evidence
that adult *T. solium* is comparatively short-lived [56].

**Redundancy check.** Mechanically redundant with *V. cholerae* — adheres, does
not kill, wins by output — unless the drained resource is the player's **economy**
rather than the wall's function. Flagged, not recommended, pending that decision.

---

## 5. The missing immune arm — what actually clears a worm

### 5.1 The load-bearing fact, and it is old

> "Pathogenic helminths, although extremely diverse in structure and behaviour,
> have one common feature, namely that they present to the host's defenses
> **large, non-phagocytosable surfaces**. Because of this, they are susceptible
> to a range of effector mechanisms differing either quantitatively or
> qualitatively from those that are active against other parasites… As an
> extreme example, **the various types of cytotoxic lymphocyte, with one
> interesting exception, are inactive against helminths.**" — Butterworth 1985
> [69]

The game's tower roster is phagocytes, cytotoxic lymphocytes and antibody. Two
of those three are, per the literature, the wrong idiom for a worm; the third
coats but does not kill. **§4a's parasite row — "cleared by direct combat damage,
more of it (bigger target)" — is not a simplification. It is the opposite of what
the immunology says.** That single line is the strongest justification in this
pass for building a new tower rather than scaling an existing one.

The second load-bearing fact is a gift to the design:

> "We argue that a limited ability of the host immune system to **distinguish
> among different nematode parasites** has led to the evolution of a
> **stereotyped Th2 response** that activates a set of effector mechanisms that
> protects against most intestinal nematode parasites." — Finkelman et al. 2004
> **(mouse: *N. brasiliensis*, *T. spiralis*)** [70]

So the type-2 arm should be **one generic tower, not a per-species ladder**. It
needs no §5c barcode, no knowledge gate, no antigen. That is both biologically
correct and the cheapest possible thing to build.

### 5.2 Which existing mechanics already point at the type-2 arm

Four of them, and one is already shipped and free from round 1.

| Existing mechanic | What it already is | What the type-2 arm adds |
|---|---|---|
| **§1b sentinel goblet sloughing** (built, free, round 1, triggered not timed, local, non-specific) | **This is the "weep" half of weep-and-sweep, already in the game.** IL-4/IL-13 → goblet-cell hyperplasia and mucus is the canonical anti-helminth effector [70][71][72]; von Moltke's own framing names "profound changes in tissue physiology, including hyperplasia of mucus-secreting goblet cells and smooth muscle hypercontractility… known as 'weep and sweep'" [79]. | The arm **amplifies** it: wider patch, faster refill, and crucially **a different mucin**. Muc5ac — not normally expressed in the intestine — is induced de novo in resistant mice, and Muc5ac⁻/⁻ mice cannot expel *T. muris* at all despite strong Th2 responses, and are also delayed against *T. spiralis* and *N. brasiliensis* **(mouse)** [78]. Muc2⁻/⁻ mice are delayed too, and the mucus of resistant mice is **less permeable** while the worms in them have a **lower energy status** **(mouse)** [77]. **Human datapoint: human MUC5AC had a direct detrimental effect on nematode vitality** in [78]. |
| **§6b mucus upgrade = turnover, not thickness** ("categorically different from killing: the player is flushing rather than fighting") | **This is the epithelial escalator, pre-specified.** Cliffe 2005: an increase in epithelial cell turnover "acts like an 'epithelial escalator' to expel *Trichuris*", under the control of **IL-13 and CXCL10** **(mouse, *T. muris*, LARGE intestine)** [75]. | The arm makes the shed rate a **live, cytokine-driven variable** rather than a purchased constant. Caveat, in the house style: **the escalator was demonstrated in the large intestine** [75]. The nearest small-intestine evidence is a different parasite and host — accelerated ileal cell migration (~2×) associated with worm expulsion in rats, versus crypt hyperplasia and chronic infection in mice **(rat/mouse, *Echinostoma caproni*, trematode)** [76]. **Small-intestinal epithelial escalation during nematode infection is plausible and under-evidenced. Mark it as the complement-paper situation: untested here, not confirmed.** |
| **§1a peristalsis** (7.5 s cycle, already drawn, already moves the bolus, already clumps the spawns) | **This is the "sweep" half, already on screen with the right clock.** IL-4 and IL-13 both increase small-intestinal smooth-muscle responses to nerve stimulation, IL-13 more strongly, abolished in **Stat6⁻/⁻** mice; *H. polygyrus* and *N. brasiliensis* infection produce the same hypercontractility, partly Stat6-dependent, via **enteric nerves** **(mouse, jejunum in organ bath)** [73]. IL-9 enhances it and accelerates expulsion **(mouse)** [74]. | The arm **multiplies the existing peristaltic amplitude**. This is the single cheapest high-impact change in the document: the surge is already rendered, already on a clock, and already determines where adherents end up. Turning it up *is* the sweep. |
| **§1b's downstream drift + excretion** ("an adherent that reaches the far end rejoins the flow and is excreted") | **The game already implements worm expulsion.** | The arm is a **multiplier on the conveyor**, not a new system. |

Plus two the game has for other reasons:

- **§4c's "the NK cell is an amplifier, not a competitor"** is the exact template
  for the new tower (see 5.3). The design argument is already made and tested.
- **§5's sIgA and §4c's plasma-cell antibody**: `IGA_REFERENCE.md` concluded that
  IgA "contains rather than kills" and that "easier to deal with means easier to
  *flush*, not easier to *eat*". **That conclusion, reached for bacteria, is
  exactly right for a worm** — and the human data back it: specific anti-ABA-1
  IgE associates with natural immunity to *Ascaris* while IgG in all subclasses
  does not **(human)** [96]; high **specific** IgE predicts non-reinfection while
  high **total** IgE predicts reinfection **(human)** [95]; anti-parasite IgE is
  associated with immunity across a range of helminths and many believe IgE and
  its receptors evolved for metazoan parasites [94].

### 5.3 The one new tower a worm justifies: **ILC2**

Not the eosinophil. The **ILC2**, and the reasoning is the same reasoning §4c
already used to pick NK-as-amplifier over a fourth killer.

**Evidence.** Innate-derived IL-4/IL-13 is *required and sufficient* for
protective type-2 immunity: T-cell-specific IL-4/IL-13⁻/⁻ mice cleared
*N. brasiliensis* **as well as wild-type**, while general IL-4/IL-13⁻/⁻ mice were
impaired — and innate IL-4/IL-13 was the critical source for goblet-cell
hyperplasia, Mucin5ac and RELMβ in the **small intestine** **(mouse)** [71]. ILC2s
are a non-redundant source: in mixed bone-marrow chimeras, mice with
IL-4/IL-13-deficient ILC2s showed reduced small-intestinal goblet-cell hyperplasia
and tuft-cell accumulation, higher egg counts and impaired expulsion **(mouse)**
[72]. ILC2 activation requirements for IL-33, TSLP and IL-25 have been
demonstrated "in mice and humans" per [79], which is the closest thing to a human
anchor located in this pass.

**Why it is a tower and not a stat.** It kills nothing. Its whole output is a
multiplier on mucus, turnover and contractility — i.e. on three mechanics the
game already has. It is **innate and ungated**, like the NK cell and the γδ IEL,
so it needs no knowledge and no barcode (and per [70] it *should* not be
species-specific). It compounds with what the player already owns instead of
competing with it. And it gives the roster the thing it currently lacks
completely: **a tower whose job is expulsion rather than killing.**

**Named "ILC2"** with the familiar gloss, on the §4c precedent ("NK cell (ILC1)").
Suggested shape, all sourced: sessile or near-sessile, sits in the lamina propria
behind the wall, emits an IL-13 field; inside the field, goblet sloughing fires
on a shorter cooldown over a wider patch, dead-wall regrowth and epithelial shed
run faster, and the peristaltic surge amplitude rises. Numbers: **all TBD,
mechanics-first**, per the standing instruction.

**The sensor is a host-cell upgrade, not a second tower.** The tuft cell is the
right fit for §1d (host-cell upgrades), not §4c: tuft cells constitutively express
IL-25 to sustain ILC2 homeostasis at rest, and after helminth infection
tuft-derived IL-25 activates ILC2s to make IL-13, which acts back on **crypt
progenitors** to produce more tuft and goblet cells — a closed circuit **(mouse)**
[79]. Pou2f3⁻/⁻ mice lack tuft cells, fail goblet-cell hyperplasia and cannot
expel worms **(mouse)** [80]. The circuit is chemosensory: SI tuft cells express
the succinate receptor SUCNR1/GPR91, dietary succinate alone triggers the whole
type-2 response through a tuft-, TRPM5- and IL-25-dependent pathway, and
*N. brasiliensis* secretes succinate — **though in vivo sensing of
*N. brasiliensis* was SUCNR1-independent**, so helminths are detected by some
other, unidentified route **(mouse)** [81][82]. Tuft cells also secrete cysteinyl
leukotrienes that co-activate ILC2s rapidly, needed for anti-helminth but not
anti-protist immunity **(mouse)** [83], and release **acetylcholine into the gut
lumen** that acts directly on worm muscarinic receptors to reduce fecundity —
epithelial ChAT deletion raised worm burden, fitness and egg counts with type-2
responses unchanged **(mouse)** [84]. One brake worth knowing before tuning:
excessive IL-25 (tuft *Il17rb* deficiency, or prolonged succinate) drives ILC2
**hypoproliferation**, also seen in chronic helminth infection **(mouse)** [85].

That last one is a real design gift: **the amplifier has a documented
over-stimulation failure mode**, which is a non-arbitrary reason for the tower to
have a ceiling.

Note the existing roster connection: `PATHOGEN_ROSTER.md` records that
**rotavirus does infect tuft cells** (Bomidi 2021) and flags murine tuft-cell
immune privilege as a possible future *terrain* mechanic. A tuft-cell host-cell
upgrade therefore interacts with an organism already on the board — the sensor
the player buys is a cell rotavirus can take out.

### 5.4 Eosinophils — genuinely contested, and the textbook version is wrong

The brief asked for this to be checked. It does not hold.

**The classical case.** Larvae are killed in vitro by eosinophils in the presence
of specific antibody or complement, which "established host defense as the
paradigm for eosinophil function" [91]. Antibody-dependent eosinophil attachment
and killing by IgE, IgG and complement; the eosinophil/IgE/mast-cell axis as a
powerful anti-helminth defence; eosinophils somewhat *less* active than
neutrophils against ensheathed nematodes, and trematodes and exsheathed nematodes
highly susceptible [69].

**The contradiction, and it is direct.** In eosinophil-lineage-ablated mice,
*T. spiralis* **muscle larvae died in large numbers** — parasite death correlated
with raised IFN-γ, reduced IL-4, and was reversed by iNOS inhibitors. The authors'
conclusion: "the long-standing paradigm of eosinophil toxicity in nematode
infection requires reevaluation, as our results suggest that eosinophils may
influence the immune response in a manner that **would sustain chronic infection
and insure worm survival**" **(mouse, two independent eosinophil-null strains)**
[90]. The review that followed is titled, exactly, *"Eosinophils in helminth
infection: defenders and dupes"*, and its framing is that eosinophil-ablation
studies "revealed an expanded repertoire of **immunoregulatory** functions" and
roles in tissue homeostasis "central to the establishment and maintenance of
parasitic worms in their hosts", with **distinct functions during primary versus
secondary exposure** [91].

**And the contradiction of the contradiction.** Eosinophil-deficient mice could
not limit *Ascaris* larval development in the lung under allergic
pre-sensitisation, i.e. eosinophils were required for killing there **(mouse)**
[46]. Eosinophils and neutrophils intercept migrating *S. ratti* larvae in skin
and muscle, with eosinophils **strictly dependent on anti-*S. ratti* antibody**
while neutrophils inhibited motility unaided **(mouse)** [93]. PD-1⁻/⁻ mice
recruit eosinophils poorly to parasite-invaded tissue and show **delayed** adult
expulsion and higher muscle larval burdens **(mouse)** [92].

**The resolution that fits all of it** — and it is the one the design should use:
**eosinophils kill migrating larvae in tissue when antibody is present, and
sustain established adults and encysted larvae when it is not.** Site- and
stage-dependent, not organism-dependent.

**Recommendation.** Do **not** ship the eosinophil as the anti-worm tower. Ship
ILC2 first. Then, if an eosinophil is wanted, build it as **the game's first
double-edged tower**: effective only against migrating larvae and only once the
antibody rungs are up (§5 ~20%/~45%), while *prolonging* any established adult or
encapsulated stage on the board. That is a genuinely new decision, it is
defensible from [90][91][93], and it is the only honest way to build a cell whose
own literature calls it a dupe.

### 5.5 Mast cells — strong evidence, worm-specific, and the effector is also the injury

mMCP-1⁻/⁻ mice show significantly delayed *T. spiralis* expulsion and increased
muscle larval deposition **despite normal or raised mast-cell numbers**, and —
critically — **no effect on *Nippostrongylus*** **(mouse)** [86]. Both
mast-cell-deficient W/W^v and mMCP-1⁻/⁻ mice expel late **and** have
significantly *less* enteropathy, with reduced TNF-α and NO; Th2 responses were
reduced in W/W^v but normal in mMCP-1⁻/⁻, so the protease is an effector rather
than a response-shaper **(mouse)** [87]. IgE⁻/⁻ mice have diminished splenic but
normal jejunal mastocytosis, lower serum mMCP-1, slower adult clearance, ~2× the
muscle larvae and fewer necrotic larvae **(mouse)** [88]. Not everything in the
pathway matters: mast-cell-specific ST2 (IL-33 receptor) knockout changed nothing
about *H. polygyrus* egg or worm burden, degranulation markers, serum IgE or
goblet-cell hyperplasia **(mouse)** [89].

**Game translation:** a mast cell is a **worm-specific expulsion burst that
damages your own wall** — the clean "effector = injury" trade the roster does not
yet have in a tower (it has it in the apoptotic/necrotic debris comparison, which
is about *how a cell dies*, not about a tower's own cost). Build it after ILC2,
and build the collateral.

### 5.6 Antibody against something too large to phagocytose

- Antibody coating a worm cannot kill it and does not need to: `IGA_REFERENCE.md`
  already reached "contains rather than kills", and §4c's `SetCoated` is one code
  path. **Reuse it; do not build a second one.**
- **Specific** IgE associates with human resistance; **total** IgE associates with
  susceptibility, with the proposed mechanism being polyclonal dilution of the
  specific response **(human)** [95]. Natural immunity to *Ascaris* in children
  tracked anti-ABA-1 **IgE**, inflammation markers (ferritin, CRP, ECP) and *not*
  IgG subclasses **(human)** [96]. Known IgE targets in helminths sit in the same
  small set of protein families as environmental allergens (EF-hand proteins,
  tropomyosin, PR-1) [94].
- Eosinophil larval killing is **antibody-dependent** where it works; neutrophil
  inhibition is not **(mouse)** [93].

**Recommendation:** the anti-worm use of antibody is **not a damage source**. It
is (a) a prerequisite that switches on the eosinophil's larval-killing mode, and
(b) a signal-to-noise mechanic — a sourced reason for a late-game enemy that
*raises* the player's antibody output in order to degrade its specificity, which
plugs straight into §5c's barcode and §5's mutation discount.

### 5.7 Helminth immunomodulation — the enemy that makes your towers worse at everything else

This is the half of the brief with the strongest evidence and the biggest design
payoff.

- **Broad-spectrum, deliberate, and documented at review level:** helminths
  modulate "all facets of the host immune response to ensure their persistence",
  with consequences including "**reducing vaccine responses, increasing
  susceptibility to coinfection** and potentially reducing tumor
  immunosurveillance" [98].
- **The hard experiment.** Concurrent *Litomosoides sigmodontis* infection
  reduced both the quantity and quality of antibody responses to seasonal
  influenza vaccination and **drastically impaired** vaccine-induced protection
  against pandemic H1N1 challenge. The killer detail: **impairment persisted when
  vaccination was performed after the previous helminth infection had been
  cleared**, so "individuals in helminth-endemic areas may not always benefit
  from vaccinations, even in the absence of an acute and diagnosable helminth
  infection." Mechanism: systemic, sustained expansion of IL-10-producing
  CD4⁺CD49b⁺LAG-3⁺ **Tr1 cells**, partially reversed by in vivo IL-10R blockade
  **(mouse; filarial nematode, not an intestinal one)** [99].
- **Human side.** Controlled human hookworm infection (50 L3, four naive
  volunteers, two years' follow-up) raised pDC frequency, reduced TLR7/8
  responsiveness and expanded CD45RA⁺ Tregs — in both controlled and natural
  infection — though Treg *functional* markers (ICOS, TNFR2, LAP) and in vitro
  suppressive capacity were higher in natural infections **(human)** [34]. Naturally
  mono-infected adults show lower proliferative responses and adult-worm ES
  antigen suppressing TNF-α and IL-10 from their PBMCs **(human)** [33]. A 2025
  review covers mechanisms "defined in preclinical models **and human trials**"
  and is explicit that effects depend on timing, tropism, genetics and environment
  and can be beneficial *or* detrimental [100].
- **Correction to the brief: ES-62 is not an intestinal helminth molecule.** It is
  secreted by ***Acanthocheilonema viteae*, a rodent filarial nematode** [101][102]
  — a phosphorylcholine-containing tetramer that desensitises lymphocyte antigen
  receptors via SHP-1 and Pac-1 recruitment, blunts macrophage IL-12/IL-6/TNF-α
  responses to IFN-γ + LPS, and matures DCs into a Th2-inducing phenotype
  [102][103]. Excellent mechanics; **wrong organism for this map**. The
  intestinal-helminth equivalents to use instead are the hookworm ES catalogue
  [27][28] and a named, characterised example: **Na-AIP-1**, a netrin-domain
  protein from the *N. americanus* secretome that suppresses colitis, requires
  CD11c⁺ cells, downregulates type-1 inflammation and TNF, and cuts TNF secretion
  from a human M1 macrophage line **(mouse models + human cell line)** [29].

**Game translation, and it is the thing the roster says it has no answer to.** An
enemy that applies a **negative multiplier to the player's effectiveness against
every other species on the board** — and whose multiplier **persists after the
worm itself is cleared** [99]. Nothing in `PATHOGEN_ROSTER.md`'s cut or built list
does this. The nearest is A/E *E. coli* profiting from inflammation, which is
*fuel*, not suppression.

Two further notes the design should record:

1. **This is the best justification found anywhere for the roster's candidate
   Treg tower.** `PATHOGEN_ROSTER.md` names "a **Treg** tower (alarmin-triggered,
   powered by the player's own loud kills)" as a candidate answer to the open
   gaps. The evidence base for an intestinal Treg mechanic is overwhelmingly
   **helminth** [34][98][99][100], not bacterial. Worth recording so the tower is
   justified by the right literature.
2. **It creates the first strategic reason not to win.** Hookworm ES products are
   in human clinical development as anti-inflammatory therapeutics [27][29], and
   controlled human hookworm infection is "well tolerated" and proposed as a
   treatment platform [35]. If a later map ever has an autoimmune or
   over-inflammation fail state, a resident worm is the sourced answer to it.
   Flagged, not scoped.

---

## 6. Cuts, and why

**1. *Trichuris trichiura* — REJECTED ON ANATOMY. This is the *C. difficile*
precedent, and it hurts.**

Whipworms "inhabit **multi-intracellular epithelial tunnels in the large
intestine** of their hosts" [47]. L1 larvae degrade the mucus layer to reach
epithelial cells and are then **completely intracellular, woven through multiple
live dividing cells**, with infection progression producing cell damage and an
expansion of Isg15⁺ enterocytes **(mouse *T. muris* + murine caecaloids)** [47];
syncytial tunnel formation has since been live-imaged in caecal monolayers
**(mouse)** [48]. In experimental trichuriasis the L4 and adult stages sit **in
the cecum**, driving mixed Th1/Th2/Th17 responses and bacterial translocation from
17 d.p.i. onward [49].

**"Woven through multiple live dividing cells" is the single best mechanic in this
pass** — a multi-slot occupant that does not kill its hosts and lives *inside* the
host layer, which is precisely the structure §1c cannot express. It is also,
unambiguously, caecal and colonic. Same ruling as *C. difficile*: **bank it as the
headline whipworm act for a future colon map.** Its burden would have earned a
slot easily (464.6 M, 13% of STH YLDs [36]).

Partial consolation: ***Trichinella*'s intestinal L1 carries the same mechanic in
the small intestine** [8], which is why #2 is ranked where it is.

**2. *Enterobius vermicularis* — cut on anatomy, and on the mechanic happening
off-board.**

"The adult male and female *Enterobius* worms inhabit the **caecum** of the human
large intestine" [50]. Its distinctive behaviour is also outside the body:
gravid females migrate to the **perianal area at night** and lay up to 15,000
eggs; eggs mature in the perianal region and released larvae can migrate back up
through the anus (autoinfection) [50]. Without autoinfection, infestation lasts
only 4–6 weeks because adults are short-lived, which is why recurrence is
reinfection rather than persistence [50][51]. Mechanically it is #1/#5's
autoinfection with the interesting half rendered off-screen, in a compartment the
map does not contain. Burden would have supported it (12.9%–30% of children
[51][52]; the most common intestinal worm of humans [50]) and the exposure route
is the best in the pass — *"a sandbox, a bitten fingernail, and a bedsheet with
93.7% of the eggs on it"* [per the Indonesian bedding study in §7] — but
"a sandbox" is not a mechanic.

**3. *Capillaria philippinensis* — cut as redundant with #1, at 10,000× less
burden.**

"**Autoinfection is part of the life cycle and leads to hyperinfection… The
parasite multiplies**", with larvae becoming adults in 10–11 days and
first-generation females producing larvae that become the second generation [63].
That is #1's mechanic, in the small intestine, with a sharper clock and a better
vehicle: raw freshwater fish, specifically **kinilaw** — raw fish soaked in
vinegar with salt, ginger and lime [65]. But: 348 patients in 25 years of world
literature, and <0.01% in community screening versus 5–23% in symptomatic groups
[64]. Keep as the **alternative skin** for #1 if a food vehicle is wanted instead
of barefoot soil; do not build both.

**4. *Taenia solium* — cut; its interesting phase is not on this board.**

Humans ingesting *T. solium* **eggs** develop cysticerci, but "always in
**extra-intestinal sites**", and those cysticerci never become adults in that
host, so unlike *H. nana* it cannot amplify its intestinal adult population
within one host [58]. Its famous pathology is extra-intestinal. Adult *T. solium*
also appears comparatively short-lived [56]. Everything distinctive about it
happens off the map.

**5. *Schistosoma mansoni* — cut, with regret, and with a note.**

Adults live in mesenteric vessels; the **eggs** must migrate from the vessels
across the intestinal wall into the faeces, and the parasite achieves it by
exerting counter-inflammatory influence on the host, "discreetly compromis[ing]
endothelial and epithelial barriers", and modulating the granuloma around each
transiting egg — a large proportion failing and lodging in tissue, where they
cause the pathology [104]. Intestinal "winner" eggs and liver "loser" eggs differ
substantially in gene expression and immunogenicity, and most of the literature
has studied the losers [105].

**The mechanic is an enemy that travels base → lumen**, i.e. the reverse of
every enemy on the board, which would be genuinely new. It is cut because the
organism is not an intestinal-lumen dweller at all — building it means building a
vascular compartment — and because the roster rule is about forcing decisions, not
about reversing an axis for one species. **Recorded for a future vascular or
hepatic map**, where it is the obvious headline act.

**6. Protozoa (*Giardia* and adjacent) — out of scope**, per the brief.

**7. *Taenia saginata* — ranked 7 but cut unless income is a target**, per §4.7.
Without an economy-attack mechanic it is *V. cholerae* with a bigger sprite, and
"none, it is just bigger" is a cut.

---

## 7. Corrections to standing design

1. **§4a: "Large bacteria and parasites don't hide" is wrong for the two most
   mechanically interesting helminths.** *Trichinella*'s intestinal L1 is
   **multi-intracellular** — a 1 mm larva threading 10–20 µm epithelial cells,
   occupying many at once as a syncytium [8] — and *Trichuris* does the same in
   the caecum, "completely intracellular, woven through multiple live dividing
   cells" [47]. §1c's two-layer model cannot express this: the host layer carries
   one cell-state per position and the occupant layer is specified for
   *extracellular* things. §1c's own justification for two layers was that it
   would make the parasite class easier to add; this is the case that shows two
   is still one short. **A multi-slot occupant that lives inside several host
   cells simultaneously needs a third representation, or the host layer needs to
   admit a shared occupant reference.**

2. **§4a: "Parasite… cleared by direct combat damage, more of it (bigger
   target)" is wrong on the immunology, and the error is load-bearing.**
   Helminths "present to the host's defenses large, non-phagocytosable surfaces"
   and "the various types of cytotoxic lymphocyte, with one interesting
   exception, are inactive against helminths" [69]. The real clearance route is
   **expulsion** — mucus and Muc5ac [77][78], epithelial turnover [75][76],
   smooth-muscle hypercontractility [73][74], mast-cell protease [86][87] — none
   of which is damage. Recommended rewrite of that cell: **"cleared by expulsion,
   not damage: the expulsion stack (mucus turnover, epithelial shed, peristaltic
   amplitude). Combat damage is near-useless."** That change is what makes the
   parasite class the only class the shipped roster genuinely cannot answer,
   which is the honest justification for the ILC2 tower.

3. **§4a: "Kills and directly occupies one coarse slot" is wrong for the
   commonest helminths.** An adult *Ascaris* lives free in the lumen and kills
   nothing [38][45]. An adult hookworm attaches, bleeds the mucosa and digests
   indrawn tissue without taking the slot as its own [25][26]. An adult
   *T. saginata* adheres and does nothing to the wall at all [54][55]. The
   "occupies by killing" assumption is a bacterial assumption that was extended
   to parasites without evidence.

4. **§1b's "ingress is something a CROWD does, never something a pathogen does"
   needs a stated exception, or *Anisakis* cannot be built.** A single L3
   penetrates the wall unaided [66][67]. Note the floor is defended in §1b as the
   minimum infectious dose, and **no helminth ID50 was locatable in this pass**
   (§3) — worm burdens are modelled as aggregated with host predisposition [44],
   not as a dose threshold. So the cleanest resolution is: the crowd rule stands
   for bacteria, and a worm's single-organism breach is a **different ingress
   mechanic belonging to the parasite class** — which does mean the game grows a
   second ingress path, contrary to §1b's "there is **one** ingress mechanic in
   the game and not two". **Director decision needed; do not quietly break the
   invariant.**

5. **§7's "Scale honesty" paragraph is computed at the wrong subdivision.** It
   reads "At 5× subdivision of a ~20 µm enterocyte, a fine tile is roughly 4 µm",
   but the shipped lattice is **7×7** (same section, two paragraphs earlier), so a
   fine tile is **~2.9 µm** and the antibody is ~290× oversized, not ~400×. Minor,
   but it is a stale number inside a LOCKED section, and §2.2 above depends on
   that conversion.

6. **`PATHOGEN_ROSTER.md`'s "Still with no answer in the roster" list should gain
   two entries**, both sourced here:
   - **Expulsion-only targets** — an enemy that no amount of damage removes
     [69]. This is the gap the ILC2 tower fills.
   - **Generalised immunosuppression that outlives the organism** — a debuff on
     the player's response to *every other species*, persisting after the worm is
     cleared [99] **(mouse, filarial)**; Treg/Tr1 and IL-10 mediated
     [34][98][99][100]. The nearest existing idea is A/E *E. coli*'s
     inflammation-as-fuel, which is the opposite sign.
   And one entry should be **re-attributed**: the doc's candidate **Treg tower**
   is justified almost entirely by helminth literature, not bacterial.

7. **`PATHOGEN_ROSTER.md`'s NET-degradation gap has a candidate organism.** The
   doc flags "**NET degradation** (no NET mechanic to disable)" as unanswered.
   *T. spiralis* larvae secrete **Plancitoxin-1**, a nuclease that degrades the
   NET DNA backbone and frees the larva from entanglement [15]. If a NET
   mechanic is ever built, this is the organism that disables it — though [15] is
   a weak secondary source and the primary should be pulled before building.

---

## 8. References

PubMed-sourced entries are cited with DOI links per the PubMed tool's attribution
requirement (*according to PubMed*); the remainder were located through Consensus
and are linked to their Consensus records.

- [1] Buonfrate D. et al. (2023) *Human strongyloidiasis: complexities and pathways forward.* Clin Microbiol Rev. [Consensus record](https://consensus.app/papers/details/663058d84ee55e50bcf3e03ed580a6a2/)
- [2] Gordon C.A. et al. (2024) *Strongyloidiasis.* Nat Rev Dis Primers 10(1):6. [DOI](https://doi.org/10.1038/s41572-023-00490-x)
- [3] Nutman T.B. (2016) *Human infection with Strongyloides stercoralis and other related Strongyloides species.* Parasitology. [Consensus record](https://consensus.app/papers/details/0d21b6a83b6c52b88376d213476a188f/)
- [4] Pillai R.K. et al. (2024) *Understanding S. stercoralis infection and its relationship to chronic alcohol abuse.* Toxicol Rep 13:101754. [DOI](https://doi.org/10.1016/j.toxrep.2024.101754)
- [5] Drent M. et al. (2025) *Strongyloides stercoralis: a neglected and latent threat.* Ned Tijdschr Geneeskd. [Consensus record](https://consensus.app/papers/details/789684bd05775fdb89712ca061f9a711/)
- [6] You L. (2003) *Checking of eggs and worms in a patient severely infected with Strongyloides stercoralis.* J Trop Med. [Consensus record](https://consensus.app/papers/details/dae0a072a1e05ff0b4025f919e18a9c1/)
- [7] Asdaghi Jahromi H. et al. (2026) *Duodenal strongyloidiasis mimicking inflammatory bowel disease.* Clin Med Insights Case Rep 19. [DOI](https://doi.org/10.1177/11795476261474993)
- [8] Baker J.A. *New insights into the intestinal niche of Trichinella spiralis.* [Consensus record](https://consensus.app/papers/details/2d4ed282b97c5601a7dbe5d2b7930c83/)
- [9] Dabrowska M. et al. (2026) *Mammalian senescent nurse cell provides Trichinella spp. muscle larvae with cell growth inhibitory capacity.* Gene 987:150034. [DOI](https://doi.org/10.1016/j.gene.2026.150034)
- [10] Cheng G. et al. (2023) *Trichinella spiralis-secreted products promote collagen capsule formation through the TGF-β1/Smad3 pathway.* Int J Mol Sci. [Consensus record](https://consensus.app/papers/details/fcedda0f203655eba17459350ceacc15/)
- [11] Zhang Z.-F. et al. (2025) *Macrophages activate fibroblasts to promote the collagen capsule formation during Trichinella spiralis infection.* Acta Trop. [Consensus record](https://consensus.app/papers/details/82657561336d5d2a8333a31985cecbb5/)
- [12] Gao M.-Y. et al. (2026) *S100A4 enhances the pro-fibrotic effect of macrophage to promote collagen capsule formation of Trichinella spiralis.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/8538a9f2b8b15f4cb00fcd59009da957/)
- [13] Hu X. et al. (2021) *Time-resolved transcriptional profiling of Trichinella-infected murine myocytes.* Parasit Vectors 14(1):130. [DOI](https://doi.org/10.1186/s13071-021-04624-6)
- [14] Wang C.-H. & Bell R.G. (1986) *Trichinella spiralis: newborn larval migration route in rats reexamined.* Exp Parasitol. [Consensus record](https://consensus.app/papers/details/b087e19bcd495c11b5535db72d2cafbf/)
- [15] Zhang R.-X. (2026) *Immune evasion mechanisms of Trichinella spiralis infection in humans.* Theor Nat Sci. [Consensus record](https://consensus.app/papers/details/1c581cc07adf54618f4310e0c9e7aac2/) — *weak secondary source; pull primaries before building.*
- [16] Park M.-K. et al. (2022) *Trichinella spiralis nurse cell formation is regulated via CCR7⁺ dendritic cells.* Parasite Immunol 44(9):e12938. [DOI](https://doi.org/10.1111/pim.12938)
- [17] Murrell K.D. & Pozio E. (2011) *Worldwide occurrence and impact of human trichinellosis, 1986–2009.* Emerg Infect Dis. [Consensus record](https://consensus.app/papers/details/91910642cde4559cb2829c7ef8f5c9c3/)
- [18] Devleesschauwer B. et al. (2015) *The low global burden of trichinellosis: evidence and implications.* Int J Parasitol. [Consensus record](https://consensus.app/papers/details/4dfe4240df9f543dbf5a57a2481241cc/)
- [19] Wei W. et al. (2026) *Global prevalence and risk factors of human trichinellosis: a systematic review and meta-analysis.* Acta Trop. [Consensus record](https://consensus.app/papers/details/6550f2c5ebfe5cd79f00785654cc6271/)
- [20] Diaz J.H. et al. (2020) *The disease ecology, epidemiology, clinical manifestations and management of trichinellosis linked to consumption of wild animal meat.* Wilderness Environ Med. [Consensus record](https://consensus.app/papers/details/06b16b8b8d2c5cbdacf034ca7e5e6bf9/)
- [21] Malone C.J. et al. (2024) *From wildlife to humans: the global distribution of Trichinella species and genotypes.* Int J Parasitol Parasites Wildl. [Consensus record](https://consensus.app/papers/details/45c3e1db332d5daa93d68dccf238d464/)
- [22] Foy H. et al. (1960) *Hookworms in the aetiology of tropical iron deficiency anaemia: radio-isotope studies.* Trans R Soc Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/a9ee834c8d45589fba8671f8c5ae93d8/)
- [23] Onosakponome E. et al. (2024) *Anaemia a monitoring tool for helminth infection.* Asian J Med Health. [Consensus record](https://consensus.app/papers/details/a200824055f0566f8f69410d78626393/)
- [24] Karadbhajne P. (2021) *A case report on Ancylostoma duodenale infection in a pregnant woman.* Biosci Biotechnol Res Commun. [Consensus record](https://consensus.app/papers/details/f5e0f86abb575563a264e035653bd79b/)
- [25] Kalkofen U.P. (1970) *Attachment and feeding behavior of Ancylostoma caninum.* Z Parasitenkd. [Consensus record](https://consensus.app/papers/details/f12172aff95c5ce6aca7ca6b11d235f1/)
- [26] Siagian F.E. (2024) *Chitin bodies vs teeth of hookworm.* Asian J Res Zool. [Consensus record](https://consensus.app/papers/details/16db48da91cb595c85471182cb1fb642/)
- [27] Abuzeid A.M.I. et al. (2020) *Twenty-five-year research progress in hookworm excretory/secretory products.* Parasit Vectors. [Consensus record](https://consensus.app/papers/details/ca047e4e549951e7bd7965af1f2490dd/)
- [28] Schwarz E.M. et al. (2026) *Hookworm genes encoding intestinal excreted-secreted proteins are transcriptionally upregulated in response to the host's immune system.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/e58430b324f05266ba7eba533ee74ed1/)
- [29] Buitrago G. et al. (2021) *A netrin domain-containing protein secreted by the human hookworm Necator americanus protects against CD4 T cell transfer colitis.* Transl Res. [Consensus record](https://consensus.app/papers/details/f23219651c2756ec8cacbeb46b24e578/)
- [30] Hoagland K.E. & Schad G.A. (1978) *Necator americanus and Ancylostoma duodenale: life history parameters and epidemiological implications.* Exp Parasitol. [Consensus record](https://consensus.app/papers/details/e494e9f6cb46512bb4124d700405b3cf/)
- [31] Kendrick J.F. (1934) *The length of life and the rate of loss of the hookworms, Ancylostoma duodenale and Necator americanus.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/6a93194396165b6d8589290739123650/)
- [32] Schad G.A. et al. (1978) *Population biology of hookworms in children in rural West Bengal. II.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/861f200e514752348664d3752982466f/)
- [33] Geiger S.M. et al. (2007) *Stage-specific immune responses in human Necator americanus infection.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/ccdb7882c99a559f846cc169fb875053/)
- [34] Manurung M.D. et al. (2024) *Controlled human hookworm infection remodels plasmacytoid dendritic cells and regulatory T cells.* Nat Commun. [Consensus record](https://consensus.app/papers/details/7decb5287e02528dad395259b44778c0/)
- [35] Vacca F. et al. (2023) *Immune profiling, microbiome, metabolomics and gut physiology of a 1-year controlled human hookworm infection.* [Consensus record](https://consensus.app/papers/details/d6449c6399055e8d85b970f76b2736da/)
- [36] Pullan R.L. et al. (2014) *Global numbers of infection and disease burden of soil-transmitted helminth infections in 2010.* Parasit Vectors. [Consensus record](https://consensus.app/papers/details/52a187a39a9d56d0843a5646f6635afe/)
- [37] Chen J. et al. (2024) *Global burden of soil-transmitted helminth infections, 1990–2021.* Infect Dis Poverty. [Consensus record](https://consensus.app/papers/details/ff2af9a038115581b94e83152245750c/)
- [38] Holland C. (2021) *The long and winding road of Ascaris larval migration: the role of mouse models.* Parasitology. [Consensus record](https://consensus.app/papers/details/772f8c8ebf0c5031b16c163b739bead9/)
- [39] Ransom B.H. & Foster W.D. (1921) *The course of migration of Ascaris larvae.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/2dc45e16d84b5736a037cb671100ed2d/)
- [40] Stewart F.H. (1917) *On the development of Ascaris lumbricoides Lin. and Ascaris suilla Duj. in the rat and mouse.* Parasitology. [Consensus record](https://consensus.app/papers/details/733b2c1caccc55858b52d5c507046065/)
- [41] Khan A.U. et al. (2024) *Resolving taxonomic ambiguities in Ascaris species: insights from a scanning electron microscopy study.* Microsc Res Tech. [Consensus record](https://consensus.app/papers/details/d5cefcadce4053d1a8fb3a7b6271e626/)
- [42] Yeremiev K. et al. (2017) *Histomorphological characteristics of Ascaris lumbricoides and Taeniarhynchus saginatus.* [Consensus record](https://consensus.app/papers/details/2cc845000b005972ae494eb6fdc93324/)
- [43] Chang C. (2024) *Worm passing from the mouth.* J Clin Health Sci. [Consensus record](https://consensus.app/papers/details/004a620c2ceb557ebbe3cfce9f389f56/)
- [44] Elkins D.B. et al. (1986) *The epidemiology and control of intestinal helminths in the Pulicat Lake region of Southern India. I.* Trans R Soc Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/7cc5100633145e909e803315ca2bcb6a/)
- [45] Hon K.L. et al. (2024) *An update on the current and emerging pharmacotherapy for the treatment of human ascariasis.* Expert Opin Pharmacother. [Consensus record](https://consensus.app/papers/details/5404943cd25b5d6395846764b9d14f57/)
- [46] Gazzinelli-Guimarães P.H. et al. (2019) *Allergen presensitization drives an eosinophil-dependent arrest in lung-specific helminth development.* J Clin Invest. [Consensus record](https://consensus.app/papers/details/a193f10c82045be696db90495cd66fcd/)
- [47] Duque-Correa M.A. et al. (2020) *Defining the early stages of intestinal colonisation by whipworms.* Nat Commun. [Consensus record](https://consensus.app/papers/details/ba52969a09b25e48b19ef1c20a3f6fc2/)
- [48] Hofer M. et al. (2024) *Patterned gastrointestinal monolayers with bilateral access as observable models of parasite gut infection.* Nat Biomed Eng. [Consensus record](https://consensus.app/papers/details/531dd44a2a8254238422be45147d5ab3/)
- [49] de Oliveira D.A. et al. (2025) *Experimental trichuriasis: changes in the immune response and bacterial translocation during acute phase development.* PLoS Negl Trop Dis. [Consensus record](https://consensus.app/papers/details/f9f678d67c91535fabd4674c5c8ebd5d/)
- [50] Gunaratna G. et al. (2020) *Diagnosis of Enterobius vermicularis infestations.* J Paediatr Child Health. [Consensus record](https://consensus.app/papers/details/db5ab060854150bd93939c20c0979373/)
- [51] Leung A.K.C. et al. (2024) *Pinworm (Enterobius vermicularis) infestation: an updated review.* Curr Pediatr Rev. [Consensus record](https://consensus.app/papers/details/6a8223cf692d57aca98abe3a2e51774e/)
- [52] Kia Lashaki E. et al. (2023) *Global prevalence of enterobiasis in young children over the past 20 years: a systematic review and meta-analysis.* Osong Public Health Res Perspect. [Consensus record](https://consensus.app/papers/details/ff5a6d2939845185ba14fa9e4b708e6b/)
- [53] Fan C.K. et al. (2019) *Enterobius vermicularis infection: prevalence and risk factors among preschool children in the Marshall Islands.* BMC Infect Dis. [Consensus record](https://consensus.app/papers/details/ee5b4eb5c6fa554fb1701676ac241e6a/)
- [54] Pawlowski Z.S. (2021) *Tapeworms (cestodes).* Schlossberg's Clinical Infectious Disease. [Consensus record](https://consensus.app/papers/details/57ae16285c925bb48b7448ad8012cae6/)
- [55] Liu K. et al. (2025) *Proteomic and phosphoproteomic analysis of adult Taenia saginata.* J Proteome Res. [Consensus record](https://consensus.app/papers/details/d19f2dfee4f35f63808dffc7240f4bc5/)
- [56] Li T.-Y. et al. (2021) *New insights on the Taenia solium tapeworm using molecular tools.* Pathog Glob Health. [Consensus record](https://consensus.app/papers/details/4502f55ad2025d10a27ee420c5e41e21/)
- [57] Ito A. et al. (2021) *Perspectives on intestinal tapeworm infections: an evaluation of direct and indirect life-cycles with special emphasis on Hymenolepis.* Curr Res Parasitol Vector Borne Dis. [Consensus record](https://consensus.app/papers/details/ed10ed71b1575dde8014f1c8712a84d6/)
- [58] Galán-Puchades M.T. (2015) *Hymenolepis nana vs. Taenia solium life cycle.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/e06c6dcf805a550dbf616815cfe70d55/)
- [59] Ito A. (2015) *Beyond our conceived concepts.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/e99f3a4beb1954e59c0928d6347acddb/)
- [60] Vílchez Barreto P.V. et al. (2017) *Prevalence, age profile and associated risk factors for Hymenolepis nana infection in northern Peru.* Am J Trop Med Hyg. [Consensus record](https://consensus.app/papers/details/d6e77fd7a2db5940a690ce11430657eb/)
- [61] Willcocks B.J. et al. (2015) *Dwarf tapeworm (Hymenolepis nana): characteristics in the Northern Territory 2002–2013.* J Paediatr Child Health. [Consensus record](https://consensus.app/papers/details/ab83b94117c859e98163f7b41492e4d1/)
- [62] Mahamud M.A. et al. (2026) *Infections of dwarf tapeworm Hymenolepis nana among internally displaced children in Mogadishu, Somalia.* J Parasitol Res. [Consensus record](https://consensus.app/papers/details/c5b8f37f5c6e5cbe9e2845dfdefbad24/)
- [63] Cross J.H. (1992) *Intestinal capillariasis.* Clin Microbiol Rev. [Consensus record](https://consensus.app/papers/details/9f71bb42c6f6562cb9992cbf1b5a2a70/)
- [64] Jongthawin J. et al. (2026) *Global distribution, clinical characteristics and outcomes of human intestinal capillariasis, 2000–2025.* Infect Dis Poverty. [Consensus record](https://consensus.app/papers/details/0c6aad51e4a7506f9c0e37abac1fe79a/)
- [65] Belizario V. et al. (2010) *Intestinal capillariasis, Western Mindanao, the Philippines.* Emerg Infect Dis. [Consensus record](https://consensus.app/papers/details/0232a3f76277599285f7f0423c38aff6/)
- [66] Fleury V. et al. (2025) *Small bowel perforation by Anisakis infection due to raw fish consumption.* Eur J Clin Microbiol Infect Dis. [Consensus record](https://consensus.app/papers/details/cb8b17e33f8d59dfbe3525ebc1c32475/)
- [67] Nieuwenhuizen N.E. (2016) *Anisakis — immunology of a foodborne parasitosis.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/ac785055bbce58fbba7ab5a9d58a2b3c/)
- [68] Dezfuli B.S. et al. (2025) *Anisakid larvae in the organs of Atlantic mackerel: host or parasite superiority?* Food Waterborne Parasitol. [Consensus record](https://consensus.app/papers/details/8aacbd453fac53cfb45b135114fdd518/)
- [69] Butterworth A.E. (1985) *Cell-mediated damage to helminths.* Adv Parasitol. [Consensus record](https://consensus.app/papers/details/b36b7fefbc895d729dec16e32a516a8b/)
- [70] Finkelman F.D. et al. (2004) *Interleukin-4- and interleukin-13-mediated host protection against intestinal nematode parasites.* Immunol Rev. [Consensus record](https://consensus.app/papers/details/b5f149fc239f5401b60bd61a4564da97/)
- [71] Oeser K. et al. (2014) *Conditional IL-4/IL-13-deficient mice reveal a critical role of innate immune cells for protective immunity against gastrointestinal helminths.* Mucosal Immunol. [Consensus record](https://consensus.app/papers/details/6a583d2ec64559e196ae4d69bbd8e2fc/)
- [72] Varela F. et al. (2022) *IL-4/IL-13-producing ILC2s are required for timely control of intestinal helminth infection in mice.* Eur J Immunol. [Consensus record](https://consensus.app/papers/details/18ca54b1bdda51a18eba5a8f211e541c/)
- [73] Zhao A. et al. (2003) *Dependence of IL-4, IL-13 and nematode-induced alterations in murine small intestinal smooth muscle contractility on Stat6 and enteric nerves.* J Immunol. [Consensus record](https://consensus.app/papers/details/54e9a03f9c5750c9ab496226db7af42c/)
- [74] Khan W.I. et al. (2003) *Modulation of intestinal muscle contraction by IL-9 or IL-9 neutralization.* Infect Immun. [Consensus record](https://consensus.app/papers/details/946ace38fc905e7aab2e43a20f2b0df1/)
- [75] Cliffe L.J. et al. (2005) *Accelerated intestinal epithelial cell turnover: a new mechanism of parasite expulsion.* Science. [Consensus record](https://consensus.app/papers/details/4edb795cd7af59c5a89c59dd6a0f95a1/)
- [76] Cortés A. et al. (2015) *Differential alterations in the small intestine epithelial cell turnover during acute and chronic infection with Echinostoma caproni.* Parasit Vectors. [Consensus record](https://consensus.app/papers/details/c9a51fef551b53d8b644f5fc055aa22f/)
- [77] Hasnain S.Z. et al. (2010) *Mucin gene deficiency in mice impairs host resistance to an enteric parasitic infection.* Gastroenterology. [Consensus record](https://consensus.app/papers/details/d54a45c8ce245b8b81fcadd3c19275f4/)
- [78] Hasnain S.Z. et al. (2011) *Muc5ac: a critical component mediating the rejection of enteric nematodes.* J Exp Med. [Consensus record](https://consensus.app/papers/details/9a3cea66630552fabb241b33455ae844/)
- [79] von Moltke J. et al. (2015) *Tuft-cell-derived IL-25 regulates an intestinal ILC2–epithelial response circuit.* Nature. [Consensus record](https://consensus.app/papers/details/ebe27fa8d5de5bd7ba690358862e8079/)
- [80] Gerbe F. et al. (2016) *Intestinal epithelial tuft cells regulate type 2 mucosal responses required for expulsion of helminth parasites.* Nature. [Consensus record](https://consensus.app/papers/details/fdb19a1d18eb591890d77450fee07bcb/)
- [81] Schneider C. et al. (2018) *A metabolite-triggered tuft cell–ILC2 circuit drives small intestinal remodeling.* Cell. [Consensus record](https://consensus.app/papers/details/825e458ecf2953fba5a331380b153946/)
- [82] Nadjsombati M.S. et al. (2018) *Detection of succinate by intestinal tuft cells triggers a type 2 innate immune circuit.* Immunity. [Consensus record](https://consensus.app/papers/details/b67fe50890c35cccb633f5bca54d5b53/)
- [83] McGinty J.W. et al. (2020) *Tuft cell-derived leukotrienes drive rapid anti-helminth immunity in the small intestine but are dispensable for anti-protist immunity.* Immunity. [Consensus record](https://consensus.app/papers/details/ad11e40ce00758d0b2dd1b028f357025/)
- [84] Ndjim M.M. et al. (2024) *Tuft cell acetylcholine is released into the gut lumen as an effector and regulator of type 2 immunity, and directly targets helminth parasites.* Immunity. [Consensus record](https://consensus.app/papers/details/760f670861fa57f59abbd87eadc27493/)
- [85] Feng X. et al. (2025) *Tuft cell IL-17RB restrains IL-25 bioavailability and reveals context-dependent ILC2 hypoproliferation.* Nat Immunol. [Consensus record](https://consensus.app/papers/details/77957e0a07855461bc5e47b7b42eb91f/)
- [86] Knight P.A. et al. (2000) *Delayed expulsion of the nematode Trichinella spiralis in mice lacking the mucosal mast cell-specific granule chymase, mouse mast cell protease-1.* J Exp Med. [Consensus record](https://consensus.app/papers/details/01d6cfcad5755a0da9f40185cc2ac2b3/)
- [87] Lawrence C.E. et al. (2004) *Mouse mast cell protease-1 is required for the enteropathy induced by gastrointestinal helminth infection in the mouse.* Gastroenterology. [Consensus record](https://consensus.app/papers/details/f39a8d0706da522f9d46e18a669b3da8/)
- [88] Thornton E.M. et al. *IgE enhances parasite clearance and regulates mast cell responses in mice infected with Trichinella spiralis.* J Immunol. [Consensus record](https://consensus.app/papers/details/c57293bcf5ee58c7bf398b12cfb311db/)
- [89] Ong N.W.P. et al. (2026) *Transcriptomic and proteomic analyses show minimal role for ST2 on MCPT5-expressing mast cells in primary Heligmosomoides polygyrus bakeri infection.* Parasite Immunol. [Consensus record](https://consensus.app/papers/details/dffe0298b6bc55d1a9c5a0cdc38ff31c/)
- [90] Fabre V. et al. (2009) *Eosinophil deficiency compromises parasite survival in chronic nematode infection.* J Immunol. [Consensus record](https://consensus.app/papers/details/4b6207358e5e5fb5ade4cac2b35e8244/)
- [91] Huang L. & Appleton J.A. (2016) *Eosinophils in helminth infection: defenders and dupes.* Trends Parasitol. [Consensus record](https://consensus.app/papers/details/f5e239fcef8d5a37baa0342c07ad92c4/)
- [92] Liu Y. et al. (2024) *PD-1 deficiency impairs eosinophil recruitment to tissue during Trichinella spiralis infection.* Cell Rep. [Consensus record](https://consensus.app/papers/details/79adb09f3dfb57d383a3f2ca040afd60/)
- [93] Ehrens A. et al. (2021) *Eosinophils and neutrophils eliminate migrating Strongyloides ratti larvae at the site of infection in the context of extracellular DNA trap formation.* Front Immunol. [Consensus record](https://consensus.app/papers/details/567c2a192a455b2bbfe72f0748c7548e/)
- [94] Fitzsimmons C.M. et al. (2014) *Helminth allergens, parasite-specific IgE, and its protective role in human immunity.* Front Immunol. [Consensus record](https://consensus.app/papers/details/42ce9bb021fb508288c11b9b1d581a9d/)
- [95] Hagel I. et al. (1993) *Ascaris reinfection of slum children: relation with the IgE response.* Clin Exp Immunol. [Consensus record](https://consensus.app/papers/details/4711d86cfb56509ea2aba4ca9a47b36e/)
- [96] McSharry C. et al. (1999) *Natural immunity to Ascaris lumbricoides associated with IgE antibody to ABA-1 allergen and inflammation indicators in children.* Infect Immun. [Consensus record](https://consensus.app/papers/details/e19870ca80db58238258871f72da1aa3/)
- [97] Mules T.C. et al. (2024) *A small intestinal helminth infection alters colonic mucus and shapes the colonic mucus microbiome.* Int J Mol Sci. [Consensus record](https://consensus.app/papers/details/510d7296aa885af7927634e109c40ef5/) — includes the **human** datapoint that trefoil-factor blood levels rise in people chronically infected with small-intestinal helminths, "highlighting a conserved mucus response between humans and mice".
- [98] Maizels R.M. & McSorley H.J. (2016) *Regulation of the host immune system by helminth parasites.* J Allergy Clin Immunol. [Consensus record](https://consensus.app/papers/details/b7751afa91555cb3a6c361a93ad81c9b/)
- [99] Hartmann W. et al. (2019) *Helminth infections suppress the efficacy of vaccination against seasonal influenza.* Cell Rep. [Consensus record](https://consensus.app/papers/details/349b55e3dacd5882a408b0add526e8b4/)
- [100] Diego J.G. et al. (2025) *Helminth infections affect host immune responses to viral infections and vaccines.* FEMS Microbiol Rev. [Consensus record](https://consensus.app/papers/details/7ed895eb363d5a3cb7ffe5b9be0c80a0d/)
- [101] Pineda M.A. et al. (2014) *ES-62, a therapeutic anti-inflammatory agent evolved by the filarial nematode Acanthocheilonema viteae.* Mol Biochem Parasitol. [Consensus record](https://consensus.app/papers/details/ab59d76a850754ccb668a21ecc6d08c2/)
- [102] Harnett W. et al. (2003) *Structural/functional aspects of ES-62.* Curr Protein Pept Sci. [Consensus record](https://consensus.app/papers/details/85bed5525fa853eaa6488e2d7a498e8b/)
- [103] Al-Riyami L. & Harnett W. (2012) *Immunomodulatory properties of ES-62.* Endocr Metab Immune Disord Drug Targets. [Consensus record](https://consensus.app/papers/details/0945083858e95928a53732d007be2918/)
- [104] Costain A.H. et al. (2018) *Schistosome egg migration: mechanisms, pathogenesis and host immune responses.* Front Immunol. [Consensus record](https://consensus.app/papers/details/14377f8a64ae57b2b9325879fc6ad838/)
- [105] Peterková K. et al. (2024) *Winners vs. losers: Schistosoma mansoni intestinal and liver eggs exhibit striking differences in gene expression and immunogenicity.* PLoS Pathog. [Consensus record](https://consensus.app/papers/details/e86cb06053635ae79041ce8af078eebd/)
- [106] Qu Z. et al. (2019) *Comparative genomic analysis of Trichinella reveals potential mechanisms of adaptive evolution* (muscle cell transformation, cell-cycle arrest, DNA repair during nurse cell formation as positively selected gene categories). Biomed Res Int 2019:2948973. [DOI](https://doi.org/10.1155/2019/2948973)
- [107] Maharatna S. et al. (2026) *Strongyloides hyperinfection complicated by invasive aspergillosis and osmotic demyelination syndrome.* Trop Doct. [DOI](https://doi.org/10.1177/00494755261487768)

**Also consulted** (exposure-vehicle evidence for soil-transmitted helminth eggs
on raw vegetables, cited in §4.4 and §6): Anggraini D.A. et al. (2018)
*Identifikasi telur nematoda usus pada sayuran kubis…* — [Consensus record](https://consensus.app/papers/details/08d21a2a8c3f5a41bd0c3fe6c03229c2/);
Susanto I.K. et al. (2025) *Identifikasi keberadaan soil transmitted helminths
pada sayuran lalapan…* Jurnal Kedokteran Meditek — [Consensus record](https://consensus.app/papers/details/710d4ba9098e51a7a79881573fb6a9e5/);
Nurfadly N. et al. (2023) *Soil-transmitted helminth eggs contamination on fresh
vegetables in Medan, Indonesia.* GMHC — [Consensus record](https://consensus.app/papers/details/45293a0c865e557abc34209c08382e30/);
Sumanto D. et al. (2022) *High case enterobiasis in school children and potential
eggs distribution on the bed.* J Microbiol Exp — [Consensus record](https://consensus.app/papers/details/f84981975d78539e983cd45490191836/).

Sources were retrieved from PubMed and from Consensus.

> Create or connect a free Consensus account to return more than 3 results per search in Claude Code.: https://consensus.app/sign-up/?utm_source=claude_code&auth=claude_code
