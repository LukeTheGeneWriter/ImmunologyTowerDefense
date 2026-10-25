# Changelog

One entry per sprint, written by the Producer at handoff. Appended to,
never rewritten.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Project pipeline stood up: Unity project builds to desktop and WebGL,
Steam app-ID stubbed, object pooling utility in place. Nothing playable
yet — next sprint starts real gameplay.

-->

## Sprint 21 -- 2026-10-14
The frame around the game got the same treatment as the board.

- **The board is a figure plate**: a rounded, ink-outlined panel with a
  drop shadow, and tabs naming the bloodstream, tissue and gut lumen.
- **All the permanent UI moved off the board** into the strips above
  and below it, as light cream cards in a rounded, friendly typeface
  (Nunito).
  - **Top right:** the HUD, with a big START ROUND button or a wave
    progress bar.
  - **Bottom right:** the shop, now an always-open tray of tiles. Each
    tile has an icon, a price chip (with a little bar showing how close
    you are when you can't afford it yet) and a level badge. Hovering
    explains it; items that don't do anything yet are hatched "planned".
- **New: the knowledge HUD**, bottom left under the lymph node.
  - One card per pathogen, showing how much your immune system knows
    about it and the ladder of what that knowledge unlocks.
  - When a dendritic cell teaches, a "+3%" flies from the lymph node to
    the card.
  - When you cross a rung, a ribbon announces it, marked "planned"
    because none of those abilities exist yet.
  - Click a card for the full ladder.
- **The organs look like organs.**
  - The bone marrow is spongy bone with marrow in its holes, and steel
    build pads you place cells on. A placed cell stands on its pad.
  - The lymph node has a capsule, follicles, and lymph vessels coming
    in from the tissue side.
  - It also fixes a bug where helper-T cells walked off the edge of the
    old oval.

## Sprint 20 -- 2026-10-14
Two looks, one key. Press **F10** to flip.

- **Daylight** (the new default) is the art-direction proposal. The board
  is light and bright: a pale aqua gut, peach tissue, rose blood. Every
  cell has a bold dark outline. Bacteria are green, viruses are violet
  little spiky balls, and both are drawn big enough to see. Before this
  they were nearly invisible against the gut (about 1.1:1 contrast); now
  their outlines are about 13–15:1.
- **Classic** is the game exactly as it looked through Sprint 19, kept so
  you can compare.
- F10 restarts the run when it switches, and it remembers your choice.
  Pick one (or a mix) and the other gets retired.

## Sprint 19 -- 2026-10-13
The gut moves now, and it has anatomy.

- **Peristalsis.** The contaminated food is shoved down the gut in
  surges by waves of contraction, then nearly stops until the next wave.
  The wall visibly squeezes behind it, and the villi just behind it
  stretch and lean downstream as the wave passes. It takes the same 30
  seconds to cross as before, so rounds aren't any longer or shorter.
- **The bolus is solid and rounded.** It was see-through (its texture
  punched holes in itself) and read as spiky. Now it's an opaque, lumpy
  mass with grubby flecks.
- **Velvet-pile villi.** Every villus is redrawn the way textbook
  diagrams draw one: a bright rim of epithelium, a darker core with the
  lacteal running up it, pale goblet cells and a fuzzy brush-border edge.
  A second, darker row behind gives the fringe depth.
- **A Peyer's patch.** Partway down the wall the villi stop, and a low
  dome with M cells sits over a lymphoid follicle. Buy **M-cell sampling**
  and the dome starts sampling pathogens drifting past. The M cells light
  up, and any DC patrolling the follicle collects that antigen. That's
  knowledge without waiting for tissue to die.
- **Crypts are real.** Each one you buy appears at the gut wall as a
  little pocket with pink Paneth granules, and dead ground nearby regrows
  about three times faster.

## Sprint 18 -- 2026-10-13
The upgrade buttons do something now.

- **All twelve progenitor upgrades are live.** Since Sprint 16, buying one
  took your ATP and changed nothing. Every row now does what it says: a
  macrophage clears debris faster or lives longer, a neutrophil's burst
  scars less or it runs faster, a DC presents more per trip or walks
  faster, a helper-T niche fills up or matches more loosely. Upgrades
  apply instantly, including to cells already on the board.
- **Upgrades belong to the niche you bought them on.** Upgrading one DC
  niche used to be going to upgrade *every* DC niche, because the DC's
  numbers were global. Each adaptive tower now carries its own upgrades,
  like the innate ones always did.
- **Clonal expansion was going to be a dud, so it changed.** It was only
  going to raise the helper-T cap, but a niche never reaches its cap: the
  cells age out first. Clonal expansion now also makes the niche produce
  helper-Ts faster, which is what it means biologically. Level 2 doubles
  the standing population (6 to 12, measured).
- **Lumen reference research.** A design agent surveyed textbook-style
  diagrams of the gut wall and wrote `docs/LUMEN_REFERENCE.md`: eight
  ranked ideas for the next lumen pass, from a cheap villus redraw (bright
  epithelial rim, fuzzy brush border, goblet flecks) to crypts and a
  Peyer's patch. Nothing in it is built yet; it's waiting on your pick.

## Sprint 17 -- 2026-09-04
The gut looks like a gut, and the dendritic cells stopped twitching.

- **DC jitter, fixed.** They were snapping most of the way to each new
  tile in a single frame and then crawling -- every tick, all their lives.
  The visual tween had been running on its own clock, seeded to a random
  phase at birth, while the movement ticked on a shared one; the two
  drifted apart immediately. They now glide the whole way. (They still
  wander sideways as they pace -- that part is the walk itself, and it is
  left alone until you have seen the motion without the snapping.)
- **The lumen is no longer poo-inspired.** The channel is a warm mucosal
  plum, the mucus is a pearly sheen instead of a grey-green film, the
  drifting particulate is lighter and thinner on the ground -- and the gut
  wall has grown a fringe of **villi** that sway with the peristalsis.
- **The contaminated food now stands out.** It kept its ochre, so it is
  the only brown thing in the band. That was the actual point: the round's
  delivery vehicle used to be camouflaged against the channel it arrived
  in.
- **The blood vessel reads as a vessel.** The endothelial wall is a row of
  actual cells now rather than one stretched bar -- immune cells visibly
  cross a boundary made of something. Corpuscles are brighter and properly
  biconcave.
- **First agentic playtest.** An agent was pointed at the Sprint 16 build
  and asked to drive it and write up what it found
  (`docs/AGENT_PLAYTEST_01.md`) -- a dry run of whether that is a useful
  way to test this game at all.

## Sprint 16 -- 2026-09-04
The game has a real interface.

A dispatched design agent wrote `docs/UI_DESIGN.md`; this sprint
implements it, with four calls from the Director on top. Every IMGUI
surface is gone -- HUD, shop, marrow picker, upgrade panel, debug dump and
the compartment labels are all UI Toolkit now, built from code.

- **The stat wall is behind a key.** What is always on screen is three
  numbers -- ATP, round, lives -- and Start Round. Everything else moved,
  verbatim, into a bottom-left instrument panel on **backtick**, default
  off. This was the Director's Sprint 15 complaint: "it has so many stats
  I can't get a feel for how the player will interact with the game."
- **Buying is live.** The shop no longer closes when a round starts, and
  the upgrade panels open mid-round. ATP was always paid per kill as the
  kills happened -- watching the number climb and buying the instant you
  can afford it is now a real thing to do, and the round does not pause
  for it. The between-rounds freeze is unchanged. The shop collapses to a
  header strip while a round runs.
- **Clicking a marrow niche floats its panel at it** -- the picker for an
  empty slot, that tower's upgrade roster for a placed one -- with a
  breathing ring on the slot itself so you can see what you are editing.
- **Three named upgrades per progenitor kind** ("Efferocytic capacity",
  "Controlled degranulation", "TCR affinity maturation"...), each with
  effect text, level dots, a real price curve and a level ceiling. They
  are still placeholders -- ATP is spent, a level is bumped, the
  simulation does not change -- but each one names the exact field a wired
  effect would write, so the sprint that makes them real is one line per
  row.
- **Lives is a number, not a bar** -- teal, oxblood under 25%, one brief
  flash when it drops.
- **Everything is quieter.** Smoked-glass panels, hairline rules, one type
  family, gold only for ATP, one slate-blue accent for "this is
  interactive". The UI is meant to sit *under* the tissue in the visual
  hierarchy, not compete with it.
- **Caught in verification:** the first build threw a null reference on
  every text label, every frame -- in the built player only, silent in the
  Editor and in batchmode. UI Toolkit needs a real `PanelSettings` asset
  for text; the project now ships exactly one, created by script.

## Sprint 15 -- 2026-08-30
The compartments look like what they are now.

A dispatched design agent wrote `docs/COMPARTMENT_DESIGN.md`; this sprint
implements it. The lumen and base bands were the tissue cell-grid in a
different tint -- now they're drawn as what they represent.

- **Lumen** -- a `LumenChannelRenderer`: a brown chyme field, a
  translucent mucus layer smeared along the gut wall, and ~40 pooled
  particulate motes drifting down the flow, with a slow ~+/-6%
  peristaltic squeeze. A pathogen riding it now reads as a twig in a
  current behind a membrane.
- **Base** -- a `BaseCompartmentRenderer`: a deep-oxblood plasma field
  lifting toward a soft endothelial **vessel wall** at the tissue seam
  (the line immune cells already cross to reach the fight), ~24 pooled
  erythrocytes streaming in from the outer edge, and the marrow + lymph
  node seated in dark **plasma halos** so they read as organs suspended
  in blood. Leans into the sepsis framing: a pathogen reaching here is
  arriving *in your blood* -- and now there's an acute **red breach
  flash** at the arrival lane instead of a silent despawn.
- **Bone marrow** -- bolder trabecular struts, sinusoid channels, a red
  retint, and a pale **birth-puff** mote on every real emission (one per
  cell actually made, via `BoneMarrowManager.OnCellEmitted`).
- **Lymph node** -- a medullary notch, a third follicle zone, and a
  faint cyan **co-localisation haze** rendered live from the real
  `LymphNode.Coloc` field, tracking where the helper-T cells gather --
  the gradient a dendritic cell climbs, made visible.
- **Perf:** the base and lumen bands left the per-cell `SpriteRenderer`
  grid entirely -- **-110 always-resident renderers at 25x10** (~-1,990
  on the 100x40 Map 01 aspiration), and `BoardRenderer.Refresh` stops
  recolouring ~120 cells every 0.15 s. Retires the scale note open in
  `BoardRenderer` since Sprint 4. Three pre-warmed pools;
  `SpriteShapes.Prewarm()` is finally called (BACKLOG item).
- Verified: all ten harnesses green (**410, 0 failed** -- rendering has
  no headless coverage), Windows build clean, headless launch 0
  exceptions (the ~31 procedural rasters generate during `Awake`). How
  it *looks* in motion is the Director's screenshot.

## Sprint 14 -- 2026-08-30
The dendritic cells pace now.

Third pass at the same playtest note ("they don't oscillate, they don't
spread into lanes"). The mechanic was right but almost never ran: a DC's
oscillation and its lane-repulsion only existed in the *patrol* state,
and in a dense round -- debris on every dead cell -- a DC picked up cargo
within about two ticks and then spent the rest of its life in the
travel-to-node / in-node / return-to-tissue states, none of which paced
or repelled.

- **Four states → two.** `PatrolTissue` and `InNode`; the `TravelToNode`
  and `ReturnToTissue` beeline states (and their straight biased dashes)
  are gone. A DC now paces the tissue band base↔lumen for its **whole
  tissue life**, repelling the other DCs across the lanes the entire
  time. Carrying antigen just pins its heading toward the base; it drops
  into the node on reaching the base band and resumes pacing when the
  cargo is spent.
- **Tuning:** `DcPatrolSweepBias` 1.0 → 1.8 and `DcFineTilesPerTick`
  2 → 3, so a full base↔lumen lap reads inside a ~30 s round. Removed the
  now-unused `DcAxisWalkBiasSharpness`.
- Verified: all ten harnesses green (**Adaptive 40**, 410 total, 0
  failed), Windows player build clean, headless launch clean. The
  repulsion A/B reads 16 co-lane ticks vs. 167 and mean spread 15.4 vs.
  4.1; the sweep spans the full tissue band (axis 6..18) vs. 10..12 for a
  plain walk. How it *looks* in motion is the Director's screenshot.

## Sprint 13 -- 2026-08-29
The board has a face now. Everything was a flat tinted square; a
dispatched design agent wrote the visual-identity spec
(`docs/SPRITE_DESIGN.md`) and this sprint implements it.

**Direction: "a histology plate at a glance, a readable icon up close."**
The tissue reads like a stained section -- packed pink host cells, a
violet bruise where a virus spreads, grey-brown necrotic debris -- and
the mobile agents are the only crisp saturated shapes on top: macrophage
= ruffled blob, neutrophil = lobed-nucleus disc, dendritic cell = spiky
star (bright core when carrying antigen), helper-T = nucleus-heavy
circle, large bacterium = maroon rod, free virus = a small cold-purple
dot, the food item = a lumpy stippled bolus. Each drawn once at boot as
a ~64px procedural texture and tinted per-instance exactly as before, so
every state change (cargo, paired, infected, cytokine heat, contact
flash) still reads.

- **Palette:** only the two flagged colours moved -- neutrophil amber →
  gold (away from hazard-orange), and the free virus particle → its own
  cold purple so virus and bacterium separate at a glance.
- **Host-cell grid:** four distinct sprites for the four states; infected
  cells get a texture split (swollen inclusion for viral, purulent
  stipple for bacterial) on top of the violet / yellow-green hue; a tiny
  deterministic per-cell jitter gives the sheet a histology mottle.
- **Compartments:** the lymph node is a bean with follicle zones, the
  bone marrow a trabecular sponge, the marrow slots recessed sockets,
  the gut wall a row-of-cells epithelial strip (the thicken+heat breach
  animation untouched).
- **The five effect flashes** now have distinct silhouettes AND timings,
  not just colours -- granule stipple, jagged breach star, soft
  efferocytosis bloom, stress-kill shockwave ring, clean knowledge ring
  -- so they stay unmistakable when they overlap and for colour-blind
  players. And a hard cap on concurrent flashes (`GAME_DESIGN.md` §8).
- **F9** in a build fires all five flashes at once for a look.

Verified: all ten harnesses still green (**410 total, 0 failed** --
rendering has no headless coverage), and a headless launch is clean (0
exceptions -- the procedural texture generation runs without throwing).
**Not** verified: how it actually *looks* -- that's the Director's
screenshot, same as every rendering change in this project.

## Sprint 12 -- 2026-08-29
Two fixes from the Sprint 11 playtest.

**Cytokine sensing is on by default now.** You always turned it on, so
it's no longer a choice -- every unit senses. The **improvement** is what
you buy: a new shop item, "Cytokine sensing +", sharpens the gradient
bias for every unit at once (this one's a *real* effect, unlike the other
shop placeholders). The `C` key still toggles it off for a rung-1-vs-2
comparison. The HUD shows the current sharpness multiplier.

**Dendritic cells actually spread across the lanes and pace the band
now.** Sprint 10's lane-repulsion was comparing coarse-cell positions, so
it only nudged about one step in seven and there was no back-and-forth at
all. Now it works at fine-tile granularity every step, and a patrolling
DC also **sweeps the full depth of the tissue band and flips at the
edges** -- so it covers ground instead of loitering near where it
spawned, while still holding its own lane. In a headless test, three DCs
started in one lane now share a lane on 2 of 250 ticks (was 82), and a
lone DC paces the whole tissue depth where a random walk barely moves.

Verified: a new `Sprint12Verification` (9 assertions) + 3 more in
`AdaptiveVerification` (37 -> 40). All ten harnesses green -- **410
total, 0 failed**. Clean Windows build.

## Sprint 11 -- 2026-08-29
Framework pass -- a **shop** and the **knowledge ladder**, both mostly
placeholder.

**The buy phase has a shop now.** During the frozen buy phase a panel
offers: **mucus turnover** for the gut barrier, three **host-cell
upgrades** (a dsRNA sensor, hardening against viral entry, bacterial-
damage resistance), and **crypts**. Clicking a **placed progenitor
tower** opens its own upgrade. Every one is priced (rising per level),
greys out when you can't afford it, and buying it spends ATP and raises a
level -- **and changes nothing about the simulation yet.** The design for
each (especially the dsRNA sensor: a ~20% chance for an infected cell to
self-destruct and release a dendritic-cell-recruiting "eat this debris"
signal) is written up in `GAME_DESIGN.md §1d` for the sprint that builds
it.

**The knowledge ladder is on screen.** The KNOWLEDGE readout is now
per-species with the six rungs -- **Cytotoxic T cells (10%), Neutralizing
antibodies (20%), Memory T cells (30%), Fc receptor (45%), Complement
(60%), Secretory IgA (70%)** -- ticking on as that species' % crosses
each threshold. Still drives nothing; each capability is its own later
sprint. Roster confirmed in `GAME_DESIGN.md §5`.

**One real change: tissue heals inward.** An empty cell regrows faster
the more healthy neighbours it has (`TissueTuning.NeighbourRegrowthBonus`),
so a cleared pocket fills in from its intact edges instead of every dead
cell regrowing on its own 20s clock -- a cell ringed by healthy tissue
comes back in ~7s.

Verified: a new `Sprint11Verification` harness, **26 assertions** (shop
spend/refuse/price-scaling, the per-tower upgrade placeholder leaving the
real tuning untouched, ladder thresholds + monotonicity, and the
regrowth A/B -- surrounded 6.8s vs isolated 20.0s). All nine harnesses
green -- **398 total, 0 failed**. Clean Windows build, 0 exceptions.

## Sprint 10 -- 2026-08-29
Small follow-up after the Sprint 9 playtest ("the new rhythm works
well!"): **dendritic cells now spread across the lanes instead of
clumping.** While patrolling, a DC biases its random walk *away from
other DCs, along the lane axis only* -- the base↔lumen threat axis is
left alone, so DCs patrol back and forth and stay evenly distributed
across the lanes, covering far more ground per cell. This is the answer
to "DC migration capacity" instead of debris homing. `DcLaneRepelStrength`
(1.4) and `DcLaneRepelAxisRange` (12) are tunable; 0 restores the plain
random walk.

Verified: 3 new `AdaptiveVerification` assertions (an A/B -- with
repulsion on, three DCs started in one lane hold a mean pairwise lane
spread of ~12 vs ~6 with it off, and share a lane on far fewer ticks).
All eight harnesses green -- **372 total, 0 failed**. Clean Windows build.

## Sprint 9 -- 2026-08-29
The round has a rhythm now. Reworked after the Sprint 8 playtest.

**The buy phase freezes time.** Not just spawning -- everything. Pathogens
piled on the gut wall stop rolling breaches, your cells stop moving, the
dendritic-cell shuttle pauses. Press **Space** and it all resumes exactly
where it was. (This fixes pathogens bursting through while you shopped.)

**The battlefield persists.** Your immune cells and any pathogens still on
the board carry from one round into the next -- no more the round ending
and abruptly despawning your whole army while stragglers keep walking. A
round now ends when its **batch has been delivered**, not when the board
is clear (it's never clear).

**Rounds are delivered by a contaminated food item.** A bolus enters the
lumen at the start of a round, drifts across the channel, and drops that
round's pathogens in four bursts **right at the gut wall** as it travels
-- so they stick and pile up instead of washing past. It passes through
and is excreted; the round ends. Each round has a **gut-themed tagline**
("Undercooked egg, Salmonella"; "Contaminated water, poliovirus") and its
own pathogen **mix** -- shown in the round bar.

**Rounds hit harder.** Batch size doubled (round 1 = 16, round 5 = 40)
and wall adhesion went 0.12 -> 0.30, so round 1 actually engages now
instead of passing with everything excreted. Economy numbers unchanged --
you're spending less on rebuilding, so expect ATP to accumulate; that's a
later tuning pass.

Verified: a new **RoundVerification** harness, **29 assertions** -- the
freeze flag + gated sim clock, the round-script taglines / class mix, the
food item delivering its full batch and the round ending only once it
exits, and cells + pathogens surviving a round boundary -- plus Adaptive
34 / Economy 47 / Combat 36 / Lifecycle 79 / Map 71 / Tissue 73 all still
green (**369 total, 0 failed**). Clean Windows build, 0 exceptions on
launch. **Not** verified: nobody has played the new rhythm -- a frozen
buy phase, a themed contaminated delivery, a battlefield that persists --
which is the whole question, and the Director's playtest.

## Sprint 8 -- 2026-08-29
The adaptive system can learn a pathogen now. The framework, not the
balance -- **every number is a placeholder**, and knowledge unlocks
nothing yet (that's the next sprint).

**The lymph node is a real place.** It was a labelled backdrop since
Sprint 2; now it's a small arena with its own co-localisation cytokine
signal that pulls its two cell types together.

**Two new progenitor towers** in the bone marrow, sharing the same 5
slots as macrophage/neutrophil (one of each innate + one of each adaptive
is 4 of 5 -- on purpose): **Dendritic** (30 ATP) and **Helper-T** (25
ATP), each priced and greyed-out when you can't afford it.

**The shuttle** (GAME_DESIGN 5a): a dendritic cell patrols tissue; when it
walks onto a **dead cell**, it picks up that cell's **antigen** -- debris
now remembers what killed it -- and eats a bite of the pile (so it
competes with a macrophage for the same debris, 5c). It then carries the
antigen left into the **lymph node**, mills among the helper-T cells, and
**pairs** with them: both freeze for ~1.5s.

**The barcode** (GAME_DESIGN 5c): every helper-T cell is born with a
random **8-bit tag**; the antigen is an 8-bit tag too. If they match
within a **Hamming distance of 2** (>=6 of 8 bits agree -- a tunable
field), the pairing **teaches**: that species' **KNOWLEDGE %** on the HUD
goes up, with a green flash. If not, the freeze still cost both cells
their time. A cargo is good for 4 pairings, then the DC walks back to
tissue for more. Helper-T cells age out on a lifespan and the progenitor
emits fresh tags, so the repertoire keeps turning over -- you're never
permanently stuck without a match. Knowledge persists across rounds; the
round boundary despawns the fielded DCs and lymphocytes like any emitted
cell, and the towers refill next round.

Verified: a new **AdaptiveVerification** harness, **34 assertions** --
the barcode math, the knowledge ledger's clamp, debris carrying an
antigen, a **full simulated shuttle** (a matching pairing raises exactly
one species' knowledge by exactly one increment; a non-matching one
teaches nothing), lymphocyte turnover, and the round boundary -- plus
Economy 47 / Combat 36 / Lifecycle 79 / Map 71 / Tissue 73 all still
green (**340 total, 0 failed**). Clean Windows build, 0 exceptions on
launch. **Not** verified: nobody has watched the shuttle run -- a DC pick
up antigen, cross to the node, pair, and the number move -- or judged
whether the lymph node reads as a second search arena. That's the
Director's playtest, and it's the question this sprint exists to answer.

## Sprint 7 -- 2026-08-28
There is a game loop now. The framework, not the balance -- **every number
is a placeholder**.

The game opens in a **buy phase**: you have 100 ATP, the bone-marrow
picker shows prices (Macrophage 40, Neutrophil 15) and greys out what you
can't afford, and nothing spawns until you press **Space** (or the Start
button). A round then spawns a **finite batch** of pathogens -- round 1 is
8, each round is 3 bigger -- and ends when that batch is resolved
(everything cleared, excreted, or reached the base). Pathogens still
sitting on the gut wall are allowed to carry over into the next round, so
a smouldering wall doesn't hold the round open.

**ATP comes in two ways** (GAME_DESIGN 5b): +3 per pathogen a unit kills,
during a round; and a +80 lump sum when a round clears, shown between
rounds as your budget for the next one. When a round clears it also
**despawns every unit your towers put in the field** -- the towers stay,
and re-emit from scratch next round.

**You have 100 lives.** A breach (pathogen reaches the base) costs one;
one regenerates every two cleared rounds; at 0 it's **GAME OVER**. The
acute "emergency granulopoiesis" punishment from the design is still
deferred, so for now a breach is just the counter.

Verified: a new `EconomyVerification` harness, **47 assertions** across the
wallet, the round state machine, batch completion, the lump sum, per-kill
income, life loss -> defeat, life regen, placement cost, and the
round-boundary unit clearing -- plus Map 71 / Combat 36 / Lifecycle 79 /
Tissue 73 all still green (**306 total, 0 failed**). Clean Windows build.
**Not** verified: nobody has played the loop -- buy, start, survive, get
paid, buy more -- which is the whole question, and the Director's
playtest.

## Sprint 6 -- 2026-08-28
An established intracellular infection is now the thing innate immunity is
bad at, on purpose (`GAME_DESIGN.md` §4b -- the Director's rework after the
Sprint 5 playtest).

**You can't just shoot an infected cell any more.** A macrophage or
neutrophil grinding on a cell with a virus or a bacterium inside does
nothing directly. What it can do, each tick it's in contact, is **roll to
recognise the infection** -- a low chance for innate cells -- and if it
succeeds it kills the cell **loudly** (a big magenta burst), taking
everything inside with it and releasing nothing. That low roll is the
whole point: it's why you'll eventually want the stress-sensing cells (γδ
T and friends) that get a *high* roll. Those aren't built yet -- this
sprint is you feeling the problem.

**Intracellular bacteria** now behave the way you asked: out of a cell
they roam freely with no death timer and take normal damage -- that's your
window. Inside a cell they're untouchable, and they **replicate, draining
the host's health**, until it dies and a **brood of bacteria bursts out**.
Catch the cell with a stress-sense kill before then and the brood never
happens. A bacterium-infected cell now renders a sickly yellow-green, so
you can tell it from a violet virus infection and watch one duck in and
burst back out.

**Viruses** split into two species. Contact-chain ones still snake --
infect one neighbour, done. **Budding** ones emit free virions on a timer
that float around (momentum-biased walk, per-tick chance to enter a
healthy cell), so a budding infection grows as a **disk**. Free virions
can only ever step onto healthy cells, so a budding front still can't
cross dead ground -- the firebreak survives. And a chunk of viral
infections now just **burn out on their own** -- the cell exhausts, dies
loud, spills its virus, no immune action needed.

Verified: `TissueVerification` grew from 53 to 73 assertions (stress-sense
loud-kill / no-burst, exposed-vs-hidden bacterium, drain + brood + caught-
early-no-brood, budding disk, burn-out), plus Combat 36 / Lifecycle 79 /
Map 71 all still green -- **259 total, 0 failed**. **Not** verified: nobody
has watched a macrophage catch an infection, a brood burst, a budding disk
grow, or an infection burn out -- all the Director's playtest. Every
number is an unvalidated mechanics-first default.

## Sprint 5 -- 2026-08-28
Tissue is terrain now. Every cell in the tissue band is a host cell with
its own state -- **Healthy**, **Infected**, **Dead**, or bare **Empty**
ground -- drawn in four distinct colours, and each coarse position now has
**two independent layers**: the host cell, and whatever extracellular
pathogen is squeezing past it. A bacterium can stand on a slot that still
holds a living cell; one enum could never say that.

**Death leaves a corpse.** Killing an infected cell -- by innate immunity,
by a bacterium lysing out, by a large bacterium grazing it down, by
neutrophil collateral -- turns it to **Dead** and drops **debris**, and
debris is real terrain: nothing regrows on a slot until the debris is
gone. A **macrophage clears it** (efferocytosis, its real second job), in
about 2.5 seconds of standing on it, with a calm blue-green pulse when a
pile is finished. Left alone, debris also fades on its own -- but about
20x slower, so clearing it is clearly the better answer. Cleared ground
regrows a healthy cell after ~20s.

**The three pathogen classes finally move differently.** A **virus** only
ever steps onto a Healthy cell and dies within 6s if it can't find one --
so a viral infection advances through intact tissue and **physically
cannot cross ground it has already killed**. Nothing in the code checks
for a firebreak; it falls out of those two rules, and a headless test
drives 60 incubation cycles against a band of dead ground and confirms
nothing ever gets across. An **intracellular bacterium** walks the
base-biased path while exposed, ducks inside a healthy cell it passes,
hides there ~12s, then lyses out -- killing that cell -- and walks on. A
**large bacterium** grazes the host cell under it as it goes, killing it
over a few steps.

**Also:** the bone-marrow strip and lymph-node backdrop were still sized
for the 100x40 map and spilled across the board on the 25x10 one -- they
now fit inside the base band. Two bugs found and fixed while resuming the
interrupted work: an intracellular bacterium was killing itself when it
lysed out, and viral spread could waste its one shot landing a doomed
particle on non-healthy ground instead of stalling against it.

Verified: a new **TissueVerification** harness, 53 assertions across
two-layer occupancy, debris/regrowth/efferocytosis, the firebreak, and
class-specific advance -- plus Sprint 4's 71, Sprint 3's 79, and Sprint
2's 36 all still green. **Not** verified: nobody has watched the firebreak
happen on screen, or a macrophage clear a pile -- both are the Director's
playtest, and the firebreak (a viral front stalling against tissue it
already killed, with no one staging it) is the question this sprint
exists to answer.

Items 1, 2 and the antigen-barcode design doc were committed by a
dispatched Code agent before it lost its network connection mid-item-5;
the head session finished items 3, 5 and 6, found the two bugs, wrote the
harness, and wrote the docs.

## Sprint 4 -- 2026-08-21
The map is real. Instead of one undifferentiated board where pathogens
appeared at random spots, Map 01 is now 100x40 host cells in three lateral
bands: your **base** on the left (bone marrow, lymph node, and the place
pathogens must not reach), 50 cells of **tissue** in the middle, and the
**lumen** on the right. Threat comes from the right and pushes left, Plants
vs. Zombies style.

Pathogens ride the lumen flow downward for free -- reach the bottom and
they are excreted with no penalty, which is deliberate. But the closer one
drifts to the gut wall, the likelier it is to stick to it, and stuck
pathogens **pile up at that spot on the wall**. Each spot's odds of
rupturing rise with the size of its pile, and when it goes, **every
pathogen there floods into the tissue at once.** That build-then-burst is
the sprint's centrepiece: you should be able to watch a dangerous spot
forming before it breaks. Once inside, pathogens make a strongly biased
random walk toward your base.

Advance is specified as "toward the base," never as "leftward" -- the base
is a map property, so a future map can put it anywhere and pathogen
movement follows without a code change. There is a test that runs the same
movement code on a mirrored board and confirms the pathogens walk the other
way.

The HUD now shows where every pathogen is (lumen / wall / tissue), running
counts of adhesions, breaches, excretions and anything that REACHED BASE,
and a live frame-cost readout.

**Two things worth knowing.** First, **cytokine sensing got much weaker on
the bigger map** -- on the old 30x5 board it pulled units onto infections
within a minute; on 100x40 it only trends toward them. Nothing broke; the
gradient is simply flat at 47 cells where it was steep at 3. It is measured
and recorded, not tuned, per the standing "mechanics first" instruction.
Second, the map spent one build genuinely broken in a way nothing caught:
the scene file still carried the old 30-column width, and because the outer
bands clamp to fit, **the tissue band silently became zero cells wide**. It
ran, drew a board, and reported no errors. Fixed, and the game now shouts
if the playfield ever collapses again.

Verified: 71 new map/invasion assertions, plus Sprint 3's 79 and Sprint 2's
36 all still passing; 4,000 cells at 8.35 ms/frame (vsync-capped, so that
is an upper bound); clean build, zero exceptions; the invasion loop visibly
running unattended. **Not** verified: nobody has watched a breach burst
happen -- the counters prove it does, but the sight of it is the Director's
to judge, and it is the question this sprint exists to answer.

Implemented by a dispatched Code agent that hit its usage limit having
committed nothing at all; the head session repaired the tree, wrote the
verification harness, found the zero-tissue bug, and wrote the docs.

## Sprint 3 -- 2026-08-21
Population is bounded. Sprint 2's progenitor towers emitted forever and no
unit ever despawned, so active cell count only ever grew -- the problem
this sprint exists to fix. A tower now stops emitting once 10 of its own
cells are alive, and resumes when one dies. Cells die by doing their job:
a neutrophil that lands 5 kills **degranulates** -- self-destructs with a
visible burst that damages whatever occupies its cell -- while a macrophage
quietly retires after 20 (the Director raised this from a drafted 15). The
two deaths are meant to read as deliberately different, not as units
randomly vanishing; the HUD now shows a live active-unit count and each
marrow slot shows "N/cap alive," so boundedness is something you can watch
rather than take on trust.

Two Sprint 2 gaps were folded in rather than deferred. Kills are now
attributed to exactly one unit -- whoever's hit lands the killing blow --
which is what makes kill-count depletion possible at all. And contact
damage now requires actually being near a pathogen (within 2 fine tiles)
instead of merely sharing its 7x7 cell, which removes an accidental
stacking bonus where every unit in a cell hit it every tick.

**The thing to watch in playtest:** that second change cut contact
frequency to about half of Sprint 2's, measured -- so clearing is roughly
half as fast per unit, arriving at the same moment as a population cap. If
the board starts losing ground, that interaction is the cause and the
contact radius is the knob, not a bug. Every number this sprint
(cap 10, kill limits 5/20, burst 3x, radius 2) is a per-tower tunable
field rather than a constant, on the Director's instruction, so a future
progenitor upgrade can sell "bump this tower's kill count" as a one-line
change.

Verified: 76/76 new lifecycle assertions, Sprint 2's 35/35 combat
assertions still pass, Sprint 1's cytokine numbers unchanged (OFF
2.99/3.14/2.84, ON 0.20/0.00/0.00), Windows build clean at 93.3 MB, launches
with zero exceptions. **Not** verified: placing a tower through the running
build's UI -- scripted clicks couldn't take window focus this session, so
that first click is the Director's to make. Implemented by a dispatched
Code agent that hit its usage limit after committing working code but
before writing any documentation; the head session re-ran all verification
and wrote the docs.

## Sprint 2 -- 2026-08-19
Bone marrow is now a real, clickable placement area: 5 slots, free
placement of Macrophage or Neutrophil progenitor towers, each emitting
units from the blood edge on its own timer. Lymph node exists as a
labeled placeholder space (not functional yet -- adaptive immunity is
still a sprint or two out). Combat is real: pathogens now come in three
classes (intracellular virus, intracellular bacterium, large bacterium),
contact deals damage, and a depleted pathogen clears back to healthy
tissue. Uncleared virus infections spread to a neighboring cell after an
incubation period -- watch a slow (cytokine-off) search let an infection
spread versus a fast one catching it first.

Director playtested the same build directly and confirmed placement,
combat, and cytokine sensing all read well. Also surfaced the next real
problem: progenitors have no population cap, so active cell count grows
unbounded over time -- scoped into Sprint 3.

## Sprint 1 -- 2026-08-19
First playable slice: a tissue lattice (configurable-width coarse grid,
7x7 fine sub-lattice for movement), pathogens that enter and adhere across
the board, and two unit types (macrophage, neutrophil) hunting them via a
pure random walk. Press `C` in the running build to toggle cytokine
sensing on/off and compare a biased search against the blind one -- that
comparison is the entire point of this sprint. Board width, and each
unit's fine-tiles-per-tick speed, are tunable without touching code.

Still rough, on purpose: no ATP/economy, no combat (contact just flashes
the pathogen, nothing dies), no multi-depth burrowing (a pathogen picks a
row and sticks there), no bone-marrow placement (units appear at random
starting spots), no art beyond flat-colored squares. All excluded
deliberately -- see `docs/SPRINT_PLAN.md`. This sprint exists to answer
one question before any of that gets built: does the search itself feel
like something, and does the toggle change that. That's a judgment call
only playtesting can make.

Also folded in the large design pass from 2026-08-19 (`docs/GAME_DESIGN.md`
now has the full compartment model, tower lifespan, fibrosis, breach cost,
and the spatial lattice spec this sprint builds against) and restructured
how the project runs (`WORKFLOW.md`, `CLAUDE.md`) -- see those files if
curious, no impact on what's playable.

**Closing update, same day:** first playtest found the cytokine toggle
imperceptible. Root cause was a legibility bug, not a broken mechanism --
fixed by making adhered pathogens genuinely infect their host cell
(continuous cytokine secretion that ramps over ~20s) and switching the
movement bias to weight each direction relative to the best local option
instead of its raw field value. Also added a visible heatmap tint so the
field itself is on screen, not just inferred from behavior. Director
confirmed via his own playtest that the toggle now reads clearly. **Sprint
1 is closed.**

## Sprint 0 -- 2026-08-18
Project pipeline stood up end to end: Unity 6000.5.8f1 project initialized
in `game/`, object-pooling utility and Steam stub in place, build script
producing both targets. Windows build launches cleanly; WebGL build loads
and runs in-browser via a custom local server (`tools/serve_webgl.ps1`).
Nothing playable yet -- next sprint starts real gameplay. Repo linked to
GitHub throughout; several device-bridge/Unity CLI quirks discovered and
documented (see TEAM_RETRO.md and AGENT_HANDBOOK.md).

## Sprint 22 -- 2026-10-19
The wall stopped being a queue.

- **Pathogens mill instead of queueing.** An adherent now has a
  continuous position in the mucus band, drifts, loops, is carried
  downstream in time with the peristaltic squeeze, and drifts toward its
  neighbours. The orderly per-lane stack is gone.
- **Ingress is proximity-based.** Crowding is measured as concentration
  over a stretch of wall; where it peaks, a **plaque** forms and its
  pressure fills as a **visible charge**. What is drawn is now the actual
  state -- the blister's height and colour ARE the charge -- instead of a
  proxy for a hidden roll that might fire in one second or thirty.
- **A trickle is now arithmetically impossible.** The charge subtracts a
  floor, so one or two adherents accrue nothing forever. Ingress is
  something a crowd does. That floor is also the minimum infectious dose,
  which is real and species-specific.
- **Bacteria replicate on the wall**, damped by nutrient competition from
  neighbours of their own kind.
- **Secretory IgA arrived** at the 70% knowledge rung, and it inverts the
  wall: free crowding charges a plaque, coated crowding cross-links into
  rafts that cannot reach the epithelium and are swept away. A coated
  bacterium still divides at full rate -- and its daughter is born
  attached to it, so replicating faster builds its own cage. Nothing in
  the lumen is killed; everything is flushed.
- **Dead epithelium is a weak spot, not a hole** (Director's call): it
  charges faster but still needs a crowd. And because a virus needs a
  living cell, a dead stretch is a bacterial sieve and a viral dead end
  -- the two threat families now want opposite walls.
- **Crypts defend themselves.** A sentinel goblet cell fires when
  bacteria crowd too close, expelling them. Innate, free, from round 1;
  the mucus-turnover row buys amplification.
- **The player can answer the wall**: towers suppress a charging plaque,
  and a bought repair restores dead epithelium.
- Two research docs underpin it: `INVASION_REWORK_SPEC.md` and
  `IGA_REFERENCE.md`, the latter correcting two of the Director's stated
  claims and one of the head session's.
- 15 harnesses, 550 checks. Sprint22Verification is seeded.

## Sprint 23 -- 2026-10-25
Four new cells, and only one of them is a killer.

The sprint opened with six parallel research passes against PubMed and
Consensus, at the Director's request, on the reasoning that getting the
cells right first lets rounds be designed against a stable roster. The
research changed two of the four towers before a line was written.

- **Cytotoxic T cell** (~10% knowledge). It recognises an infected cell
  and kills it -- by **apoptosis**, which is clean. Innate cells can only
  damage a cell to death, which spills its contents and leaves a mess the
  tissue has to clear before it can regrow. The infected cell dies either
  way; what adaptive immunity buys is that **the tissue recovers instead
  of scarring**. (Shipped first, for a few hours, as "the enterocyte
  survives" -- wrong biology, caught by the Director the same day and
  corrected before the entry was final.)
- **NK cell (ILC1)**, buyable from round 1. Mostly an **IFN-gamma
  amplifier that upgrades the macrophages around it**, with a weak precise
  kill of its own. The research is clear that intestinal NK cells are less
  cytotoxic and higher IFN-gamma than blood NK, and that the clearest in
  vivo demonstration of NK function here is NK cells cutting
  *intramacrophage* Salmonella. So it compounds with your macrophages
  rather than competing with them.
- **gamma-delta T cell (IEL)**, also ungated -- it is tissue-resident and
  needs no priming. **The only tower that repairs.** It patrols the wall,
  holds forming blisters down, and mends dead barrier segments.
- **B cell** (~20% knowledge) fielding sessile **plasma cells that emit
  drifting antibodies**, which stick to pathogens and coat them -- reusing
  the exact coating state secretory IgA already uses, because tissue
  antibody and luminal IgA are one molecule in two compartments.

Underneath:

- **Recognition is now a visible, cumulative, cooperative meter** instead
  of a hidden per-tick roll. An infected cell brightens and takes a collar
  as immune cells work on it. Three cells converging clear it far faster
  than three working apart -- superadditive, as measured.
- **The innate recognition numbers were inverted and are fixed.** A
  macrophage cleared an infected cell in ~4s despite having no
  MHC-I-restricted recognition at all -- roughly 800x faster than a real
  cytotoxic T cell. The design always said innate immunity is bad at this
  on purpose; the number said the opposite. Innate is now ~10x slower than
  cytotoxic rather than ~800x faster.
- **The neutrophil got faster** (3 -> 5 fine tiles/tick): the game showed
  a 3x speed gap over the macrophage where the measured gap is 7-10x.
- **Knowledge now changes behaviour**, not just the HUD. A gated tower is
  buyable whenever you like and sits dark until a species reaches its rung.

Fixed on the way: **the Sprint 22 secretory-IgA rung was never marked
built**, so for a whole sprint -- including the playtest where the Director
reported never perceiving IgA -- the HUD called a live mechanic "planned".

16 harnesses green, 605 checks. WebGL build 9,222,409 bytes, 0 errors.

## Sprint 24 -- 2026-10-25
Pathogens got names, and three of them brought mechanics the game had no
answer for.

Four research passes against PubMed and Consensus produced ranked
shortlists rather than catalogues, on one rule: **a species earns a slot
by forcing a decision no existing enemy forces.** Full roster, including
the shortlisted-but-unbuilt and the rejections, in
`docs/PATHOGEN_ROSTER.md`.

**Species are now a real thing.** Knowledge used to be keyed by pathogen
CLASS -- three of them -- so learning about "intracellular virus" taught
you about every virus that would ever arrive. §5 has always promised the
opposite ("a new species arriving in a late round meets a naive adaptive
system") and could not deliver it. Knowledge is now per named species.

**Rotavirus.** Infects mature villus-tip enterocytes and **never the crypt
stem cell**, so the crypt keeps repaving from below -- a plague that eats
the wall but cannot touch the engine that rebuilds it. It also carries
NSP4, a real viral enterotoxin, so infected cells poison healthy
neighbours they never infect. The two compose: **the toxin reaches cells
the virus is forbidden to enter.**

**Vibrio cholerae.** The first enemy that wins **without killing
anything**. It adheres and secretes; the wall under the field stays alive,
keeps its slots, keeps blocking invasion, and stops being yours. Junctions
loosen so plaques charge faster, and the damage releases lactate the
organism feeds on. Clear the secretors and the subversion fades -- so
clearing it feels like it achieved something, where killing cells would
have left permanent holes.

**A/E E. coli.** Respires the products of your own immune response, so a
neutrophil-heavy clear **makes the survivors faster**. This inverts the
assumption the whole game has run on. It was the one strategy the research
found with no answer anywhere in the roster -- and it still has none,
deliberately.

Also: the antigen-separation check **caught a real collision** at species
six -- rotavirus's first barcode sat 2 bits from the generic intracellular
bacterium, so the two would have taught each other. That is the 8-bit
roster ceiling arriving exactly where the research predicted.

Carried but not acted on: the ~60% complement rung is probably wrong for a
gut map (the gut lacks MAC components), flagged in §5 awaiting a decision.

17 harnesses green, 638 checks.

## Sprint 25 -- 2026-10-25
A refinement pass on the Director's notes. Six items, one of which was a
bug that had already shipped.

**The gamma-delta and NK cards were missing text.** Their labels contained
real Greek characters -- "γδ T cell (IEL)" and "IFN-γ" -- and the shipped
Nunito font has no Greek block at all. UI Toolkit does not warn, does not
error and does not draw a missing-glyph box: the characters are simply
absent. It survived a build and a playtest and was caught by eye. Fixed,
and there is now a harness that walks every player-facing string and fails
if any character cannot actually be drawn.

**The species jump is finished.** Knowledge can no longer be addressed by
pathogen class at all, so nothing can accidentally teach "every virus at
once". This also fixed a live bug the half-migration had left: tower
licensing iterated the three old classes, so learning rotavirus to 100%
would never have switched a cytotoxic T cell progenitor on. The knowledge
HUD and the debug readout now size themselves from the species roster, so
a new organism appears in both the moment it is added.

**Smaller standing armies.** Every progenitor's population cap came down
-- macrophage 10 to 4, neutrophil 10 to 5, cytotoxic T 8 to 3, NK and
gamma-delta 6 to 3, plasma cells 5 to 2, dendritic cells 4 to 3, helper T
8 to 4. Fewer cells on the board means each one is legible and each
upgrade is felt. The helper-T change matters most: a cap of 8 never bound
(the niche's steady state is about 5.7), so clonal expansion's cap half
had been buying literally nothing. At 4 it binds.

**Complement moved from the knowledge ladder to the body shop.** It is
structural, not learned -- gut C3 is made locally by stromal cells and
deposits through the alternative pathway, which antibody does not trigger.
It is also re-specified as what the gut actually does: opsonise and
recruit a phagocyte, rather than lysing a membrane with no cell present,
which does not happen in a gut that lacks membrane-attack-complex
components.

**Pathogens change lane in the lumen.** Depth used to be fixed from spawn
to exit, so adhesion odds -- which fall off sharply with distance from the
wall -- were decided by one spawn roll the organism could never escape.
Near lanes were lethal, far lanes were scenery. Now organisms mix across
the channel, with bacteria mixing more and biasing toward the wall because
they swim, while virions drift where the fluid puts them. Spawn depth is
stratified by a rotating cursor as well, so a wave spreads across the
channel instead of clumping.

18 harnesses green, 667 checks.

## Sprint 26 -- 2026-10-25
Greek, properly this time -- and the gamma-delta cell stopped being a
bacterium.

**The UI font now contains Greek.** Last sprint's fix for the missing
characters was to spell them out ("IFN-gamma"), which was a workaround,
not a fix: immunology is written in Greek and the next cytokine would have
hit the same wall. The shipped face is now `ImmunoSans` -- Nunito with the
whole Greek block and a set of scientific symbols merged in, built by
`tools/merge_greek_font.py` from two SIL OFL donors. Donor glyphs are
scaled to match Nunito's x-height so a gamma sits on the same optical line
as the Latin around it. IFN-gamma and the gamma-delta T cell are written
as IFN-γ and γδ again, and µ, ±, ≥, ≤, ≈, ≠ and the arrows all work too.

The coverage list the audit checks against is now **generated from the
font files** rather than written by hand -- a hand-kept allowlist is how
this went wrong in the first place, with the code believing a character
was fine when the font had never contained it.

**The gamma-delta T cell was a drawing of a bacillus.** An elongated
capsule with a long nucleus down the middle is a rod-shaped bacterium, on
a board whose enemies are bacteria. It is now a squircle -- a cell
flattened between two enterocytes, which is where it actually lives --
with a scalloped edge cut into its silhouette for the face pressed against
the basement membrane.

Its colour was half the problem too: teal-green, sitting beside a green
bacterium and a teal helper-T. It is now steel slate, the only desaturated
unit on the board, so it separates by saturation instead of fighting for a
hue on an already-crowded wheel.

18 harnesses green, 668 checks.

## Sprint 27 -- 2026-10-25
You can expand the bone marrow now.

The Director lost a run and said the interesting part was having to think
about where the ATP went -- so this is built to keep that pressure rather
than relieve it. **You start with four niches instead of five**, and can
buy up to eight.

**Un-bought niches are visible from the first frame**, drawn as fatty
yellow marrow with the price sitting on the next one. You buy by clicking
the pad itself rather than through a shop row, so the thing you are
purchasing is on the board in front of you before you can afford it. The
bone fills top-down, so it reads as one growing column rather than a
scatter of gaps, and a reconversion lands with a warm burst because it is
the most expensive single purchase in the game.

**The price escalates: 80 / 140 / 200 / 260.** Two rules behind those
numbers, both now pinned by tests. The first niche costs more than the
dearest tower, because buying *room* for a tower must never be cheaper
than buying the tower -- otherwise the best opening is always "expand
first". And the last one costs more than three dear towers, so late ATP
still has something to compete with.

This is real biology, not a game abstraction with a biology label stuck on
it: fatty marrow reconverting to haematopoietic marrow under G-CSF, which
shows up on imaging as patchy expansions appearing one region at a time --
which is also why niches arrive one at a time instead of the bone growing.
It is the same signal as the emergency granulopoiesis already in the
design, expressed slower and more permanently.

GAME_DESIGN.md §2a's oldest open question is now closed.

19 harnesses green, 689 checks.

## Sprint 28 -- 2026-10-25
Cells stop dogpiling, the marrow goes 2x4, and the shop moves to the top
left.

**The swarm is fixed, and not by bouncing cells off each other.** Every
innate cell read the same cytokine field, so they all walked to the same
pathogen and everything else strolled past. The fix models what actually
stops a neutrophil swarm in life: cells desensitise to the self-secreted
attractants that drew them in, so a crowded cell stops finding the crowd
attractive. It is subtracted from the gradient reading rather than being a
hard rule, so a cell with nowhere better to go will still go there.

The shape matters more than the strength. **Two companions are free**, so
three cells can still converge on one pathogen -- exactly the number the
cooperation bonus rewards -- and only the surplus past that is pushed
away, rising sharply as the pile grows. The fourth cell is barely
discouraged; the eighth is firmly sent elsewhere. It is same-kind only: a
macrophage and a neutrophil working one target together is fine, eight
neutrophils is not.

Worth knowing before anyone tunes this up: in life, disabling that brake
lets neutrophils cover *more* ground but makes bacterial clearance
*worse*, because they stop containing what they find. Both extremes are
worse than the middle, so the harness pins the free allowance rather than
trusting a future tuning pass.

**The bone marrow is a 2x4 grid**, side by side. Eight niches in one
column had squeezed each pad down to about two thirds of its old size; two
columns of four doubled them instead -- they are now bigger than they ever
were at five slots.

**The body shop moved to the top left**, where the eye lands first. The
debug readout took the vacated bottom-right corner; it is hidden by
default and loses nothing by moving.

20 harnesses green, 705 checks.
