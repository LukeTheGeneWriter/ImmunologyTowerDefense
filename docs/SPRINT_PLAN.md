# Sprint Plan — Sprint 34 (opened and closed 2026-11-02)

Director, skipping Sprint 33's playtest too: *"Kill the game for a while
longer and launch a research swarm to ID 10 more common gut pathogens in
humans. Design sprites and warning messages. Some are just stronger
bacteria or viruses, others have some special power, resistance, etc."*

Then, after the passes landed: *"Colonic is fine. Figure out how the body
learns about giardia, there is definitely a way we can model here. You can
fix rotavirus, good catch."*

Sprint 33's plan and outcome are in git (`cfde0f7`) and `CHANGELOG.md`.

---

## Scope

**A research sprint with a small code tail.** No new organism is built.

| Strand | What |
|---|---|
| A | Five parallel research passes: protozoa, helminths, common bacteria, viruses, and one cross-cutting burden pass |
| B | A design spec: ten organisms, their sprites, their warning messages, and the engineering that gates them |
| C | The Director's follow-up: how the body learns about *Giardia* |
| D | Fix the two rotavirus findings the passes turned up about an organism already shipped |

### Why a swarm rather than one pass

The roster had been chosen purely on **mechanical novelty** — deliberately,
and the rule was good: *a species earns a slot by forcing a decision no
existing enemy forces.* But the Director asked for **common** pathogens, and
nobody on this project had ever put a burden number next to a candidate. A
virologist's "common" and a parasitologist's "common" are not the same
scale, so one pass had to own the cross-kingdom numbers while four others
went deep. Two kingdoms — protozoa and helminths — had never been looked at
at all.

### The rule the Director relaxed, and why that was right

"Some are just stronger" closed a real hole. **Sapovirus** (#2 most common
enteric pathogen in the world) and **ETEC** (#5) are mechanically redundant
with organisms already built, and the old rule excluded them *for being
ordinary*. A game about the gut that omits the second most common thing in
the gut is sorting on the wrong axis. So the ten are **eight mechanic slots
plus two tier slots**, labelled as tiers rather than given an invented
power. Bloons does the same thing: red → blue → green is one mechanic at
four strengths.

## What shipped

### A + B — the research and the spec

~5,500 lines in `docs/research/`, every claim PubMed- or Consensus-verified
in session, gaps marked **UNVERIFIED** rather than filled in, and every
mouse finding labelled mouse. The ten, with sprites and warnings, are in
**`docs/ROSTER_EXPANSION_SPEC.md`**.

**The two design findings worth more than the organisms:**

- **The tell goes where the player must look.** Three of the ten put their
  marker on something other than the enemy, and that is correct rather than
  a compromise: astrovirus's breach on the **wall** (drawing it on the
  virion would delete the mechanic), *S. aureus*'s toxin on the **bolus**,
  *Cryptosporidium*'s apoptosis resistance on the **infected host cell**.
- **Colour cannot be the species channel.** The palette has two pathogen
  hues and every other hue is already a meaning on this board, so identity
  is silhouette family (kingdom) × one tell (the *mechanic*, not the
  taxonomy) × size — with the eukaryotes legitimately the largest things in
  the lumen, a *Giardia* trophozoite being 15 µm against a 2 µm bacterium.

### C — how the body learns about *Giardia*

§1c's "debris is the input to adaptive immunity" made a whole class of
organism unlearnable: *Giardia* kills nothing and sheds no debris, so the
rungs that answer it were unreachable from a *Giardia* round. The real
route is **parasite-shed extracellular vesicles** — human DCs capture and
mature on *Giardia* microvesicles whose proteome carries VSPs, and
**antibody binding triggers a massive release**, so attacking it is what
makes it teachable. Both carriers already exist (the Sprint 19 M-cell dome,
or a patrolling DC). Recorded as an amendment under §1c: **debris is one
input, not the only one.** Designed, unbuilt.

Two routes were rejected *on evidence* and recorded so they are not
re-proposed: transepithelial dendrites (CX3CR1⁺ cells do not migrate to the
node, so they are structurally wrong for §5a) and goblet-cell antigen
passages (they feed *tolerogenic* DCs and are shut by IL-1β, which *Giardia*
does not induce — a GAP route would teach the immune system **tolerance**).

### D — the rotavirus corrections

Both findings were wrong in the same direction: the game had been playing
rotavirus as an ordinary lytic virus arriving at an ordinary dose.

- **Non-lytic exit.** `SpeciesProfile.NonLyticReleaseFraction` suppresses
  §4b burn-out. An ignored rotavirus focus no longer resolves itself — it
  keeps shedding until the player kills the cell. It stays self-limiting
  via the crypt repaving under its tropism, which is closer to how
  rotavirus actually resolves clinically.
- **Vesicle dose.** `SpeciesProfile.PlaqueCrowdWeight`, read through
  `SpeciesRoster.CrowdWeightOf`. Capped at 1.9, below the ingress floor of
  2.0, **because §1b Step 2 property 2 is locked and load-bearing** — the
  measured multiplier is ~3.5x and would have let a lone virion charge a
  plaque. Rotavirus's minimum dose is now two organisms where a generic
  virus needs three, which is exactly the per-species knob §1b Step 2 said
  the floor would become.
- **And the thing that fell out of fixing them: budding had never been
  per-species.** §4b has said "two per-species modes" since Sprint 5 and
  the code chose with a coin flip, admitting in a comment that "there is no
  species roster yet" — untrue since Sprint 24. Seventh instance of the
  standing pattern.

## Stopping points — what was deliberately not done

- **None of the ten organisms is built.** Four gates come first (spec §5):
  the barcode must widen to 12 bits, a per-species sprite path must exist
  at all, the knowledge HUD must scale past six species, and a pathogen's
  sprite must be able to change mid-board.
- **"Shed antigen" is designed, not built.** One new lumen-side entity, and
  it changes a locked section, so it needs the Director.
- **The anatomy ruling's unlocked organisms are unranked.** *Entamoeba*,
  *Shigella*, *Campylobacter*, *Trichuris*, *C. difficile*, ETBF. The
  ruling landed after the ten were chosen; they are the head of the next
  list, not a late addition to this one.
- **No sprite was drawn.** The spec is primitive-level (which `FillDisc`,
  which `FillStar`, what size, what hue) so that a Code sprint can execute
  it, but nothing was committed to `SpriteShapes`.

## Corrections this sprint produced

Sixteen across the five passes; the ones touching built code or locked
sections are listed in `ROSTER_EXPANSION_SPEC.md` §7 and `BACKLOG.md`. The
two that matter most for whoever builds next:

1. **§4a's "parasites don't hide" and "cleared by direct combat damage,
   more of it" are both wrong.** Cytotoxic lymphocytes are inactive against
   helminths; clearance is expulsion. The player's instinct — build more
   killers — is the wrong answer for the entire group.
2. **The built *V. cholerae* actively swims**, and non-chemotactic mutants
   *out-compete* wild type across the small intestine. "Run straight and
   get everywhere" beats "steer at the wall" — truer *and* cheaper than
   what is implemented.

## Verification

`Sprint34Verification`, **20 checks** in three parts: the spread mode
belongs to the roster, a non-lytic releaser does not burn out, and the
vesicle dose measured through the real `GutWall` rather than a
reimplementation of it.

One pre-existing harness had to be corrected, not accommodated:
`Sprint32Verification` asserted that "the named organisms are the
**majority**" of the round it finds, which is round 20, whose cast makes
them **exactly half** — it had been passing on luck, and a Sprint 34 change
to RNG consumption moved the stream. It now computes the expectation from
the round's own cast weights with a 3σ tolerance at the emitted batch size.
Third instance of a harness owning a number it is not testing.
