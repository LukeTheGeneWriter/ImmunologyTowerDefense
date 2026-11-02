# Session Handoff — 2026-11-02 (Sprint 34: five research passes, and the rotavirus corrections)

A checkpoint written by the head session. **This is a living file:
overwrite it at the next checkpoint rather than accumulating dated
copies.** It is not one of `WORKFLOW.md` §3's canonical docs; if it
contradicts `SPRINT_PLAN.md`, `ENGINE_STATUS.md` or `INTERFACE.md`, those
win.

## First: which machine are you on?

| | Windows | Linux (CachyOS) |
|---|---|---|
| Clone | `C:\Users\lukef\ImmunologyTowerDefense` | `~/Projects/Biotech/ImmunologyTowerDefense` |
| Shell | PowerShell | fish (scripts are bash, run directly) |
| Unity | `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` | `~/Unity/Hub/Editor/6000.6.4f1/Editor/Unity` |
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` (25 methods) |
| Build / serve WebGL | `BuildScript.BuildWebGL`; `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/unity.sh webgl`; `tools/serve_webgl.py` → http://localhost:8000 |
| Look at sprites | — | `tools/unity.sh SpriteShapeDump.DumpAll` → `_spritedump/` |

**`git pull` before doing anything.** GitHub is the only sync point.
**The Windows machine still needs Unity `6000.6.4f1`** (via the Hub, with
"Web Build Support") before it opens the project.

## What this session was

**Mostly research.** The Director asked for ten more common gut pathogens
with sprites and warning messages, and explicitly sanctioned "some are just
stronger bacteria or viruses". Five research passes ran in parallel; two of
their findings were about an organism already shipped, so those were fixed
in code. **The ten are specified, not built.**

### The deliverables

| What | Where |
|---|---|
| The ten organisms, sprite specs, warning messages, engineering gates | **`docs/ROSTER_EXPANSION_SPEC.md`** — start here |
| Protozoa (a kingdom no earlier pass looked at) | `docs/research/pathogens_protozoa.md` |
| Helminths + the type-2 arm the game has no towers for | `docs/research/pathogens_helminths.md` |
| Common bacteria, re-ranked by prevalence | `docs/research/pathogens_bacteria_common.md` |
| Viruses extended; three deep dives closed | `docs/research/pathogens_viruses_02.md` |
| Cross-cutting burden, so "common" is GBD/GEMS/MAL-ED numbers | `docs/research/enteric_burden.md` |
| How the body learns about *Giardia* (the Director's follow-up) | `docs/research/giardia_antigen_route.md` |

~5,500 lines, every claim PubMed- or Consensus-verified in session, gaps
marked **UNVERIFIED** rather than filled in, mouse findings labelled mouse.

## Two Director rulings landed

1. **Colonic is fine.** The anatomy rule that rejected *C. difficile* was
   quietly costing **four of the most common gut pathogens in the world** —
   *Shigella*, *Campylobacter*, *Entamoeba histolytica* and *Trichuris*.
   Anatomy is now a recorded note, not a veto; §1a's bands are a generic
   mucosal cross-section. Recorded in `PATHOGEN_ROSTER.md`. **This opens a
   new head of the shortlist that no pass has ranked**, led by *Entamoeba*,
   which answers two of the five standing "no answer in the roster" gaps.
2. **Fix rotavirus** (below).

## Sprint 34 — what changed in the engine

Three new `SpeciesProfile` fields, all zero-is-neutral, so an un-opted-in
species behaves exactly as before. Full signatures in `INTERFACE.md`.

| Change | Why |
|---|---|
| `SpeciesProfile.Exit` + the `ViralExit` enum | §4b's "two per-species modes" was a **coin flip** |
| `NonLyticReleaseFraction` | rotavirus does not lyse the cell, so its focus never bursts |
| `PlaqueCrowdWeight` + `SpeciesRoster.CrowdWeightOf` | rotavirus arrives in vesicles: one unit is already a dose |
| `PathogenAgent.VirusBuds` / `.WillBurnOut` (read-only) | so a harness can assert the property, not a proxy |

**Budding had never been per-species — seventh instance of the standing
pattern.** §4b has specified two per-species spread modes since Sprint 5;
`AssignVirusTraits` picked with `Random.value` and its own comment admitted
"there is no species roster yet", which stopped being true in Sprint 24.

**The crowd weight is capped below the ingress floor on purpose.**
Rotavirus's measured vesicle multiplier is ~3.5x. §1b Step 2 property 2 is
locked and load-bearing — "a trickle is arithmetically impossible, not
merely unlikely" — and 3.5 against a floor of 2.0 would let a lone virion
charge a plaque and delete it. The weight is **1.9**, which makes
rotavirus's minimum dose **two organisms where a generic virus needs
three** — the per-species knob §1b Step 2 always said the floor would
become. Mechanism literal, magnitude abstracted.

**Rotavirus is still self-limiting**, but by the crypt repaving under its
villus-tip tropism rather than by burn-out. The two traits it already had
compose, and that is closer to how rotavirus actually resolves — by
epithelial replacement, not by the virus exhausting itself.

## Numbers worth keeping

- Four organisms crowd **1.9x harder as rotavirus** — 7.019 against 3.694 —
  through the real `GutWall`.
- **Two rotavirus clear the ingress floor (3.8 > 2.0); two ordinary virions
  do not (2.0 ≤ 2.0).**
- **No rotavirus focus burns out** (0/100). An ordinary virus still does
  (32/100 ≈ 30%), unchanged.
- A cross-linked rotavirus raft crowds **nothing** — the IgA resolution
  survives the dose multiplier.
- **The antigen barcode holds at most 12 species** at the shipped match
  radius (constructed 12, Hamming ceiling 18). At r=3 it holds exactly
  **two** — a Plotkin bound, not a search limit. A 16-species roster needs
  **12 bits**, and a generated 16-tag set at minimum distance 6 is in the
  spec ready to paste.
- **Norovirus's knowledge knock-back is ×0.3 with a floor at the ~10% CTL
  rung** — only ~30% of antibody blockade survives one variant
  replacement, while the HLA class-I epitopes are conserved.

## Open, in rough priority order

Full detail in `BACKLOG.md` under "Open after Sprint 34".

1. **The Director has now skipped three playtests.** Sprints 32, 33 and 34
   are harness-measured, not played. Nothing is known about whether the
   40-round curriculum *reads* as teaching, whether a one-burst round 40 is
   exciting or a wall of sprites, or whether IgA is perceptible.
2. **Four gates before any new species ships**, all in the spec's §5:
   widen the barcode to 12 bits; build a per-species sprite path (there is
   none — `PathogenAgent` picks from `Class`, two sprites for the whole
   roster); make the knowledge HUD scale past six species; allow a
   pathogen's sprite to change mid-board (four of the ten need it).
3. **"Shed antigen" is designed and unbuilt** — how the body learns about
   *Giardia*. The parasite sheds its own antigen in extracellular vesicles,
   human DCs capture and mature on them, and **antibody binding triggers a
   massive release**, so attacking it is what makes it teachable. Carried by
   either sampler already built. Recorded as an amendment under §1c:
   **debris is one input, not the only one.** Needs a Director decision.
4. **Nontyphoidal *Salmonella* should lead its own sprint** — more common
   than five of the chosen ten, but its mechanic is a change to §4a's
   advance rules (the infected enterocyte is *extruded into the lumen*).
5. **Corrections to standing design** that touch built code: the built
   *V. cholerae* actively swims (non-chemotactic mutants out-compete wild
   type); §4a's "parasites don't hide / cleared by combat damage" is wrong
   for every helminth; adenovirus F41 must not be built as a cliff;
   norovirus's 2–4 year cycle is historical only.
6. **§4a's parasite class is over-specified** — no enteric protozoan needs
   the multi-slot footprint `BACKLOG` calls "more engineering than the
   other three classes combined". Single slot with a size multiplier;
   defer the footprint to helminths.
7. **`docs/INTERFACE.md`** now has a Sprint 34 section, but the ten-sprint
   backfill (Sprints 22–31) is still missing.

## Traps this session hit

- **A harness assertion was wrong, and had been passing on luck.**
  `Sprint32Verification` asserted "the named organisms are the **majority**"
  of the round it finds. That round is 20, whose cast is 2/2/1/2/2/1 — so
  the named organisms are expected to be **exactly half**, and whether a
  58-organism sample lands above or below is a coin flip. A Sprint 34 change
  to how many random numbers a rotavirus spawn consumes moved the RNG stream
  and it read 24 of 58. It now computes the expectation from the round's own
  cast weights and allows 3σ of sampling error at the emitted batch size —
  an observed zero is ~7.6σ away, so it still fails loudly if a named
  organism stops reaching the board. **Third instance of a harness owning a
  number it is not testing.**
- **A vacuity guard earned its keep immediately.** `Sprint34Verification`'s
  first run established 0 of 200 rotavirus infections, because the harness
  had collected host slots at the **crypt** end — which the villus-tip
  tropism correctly forbids. Without the "the test is not vacuous" check it
  would have reported a clean zero burn-out rate from zero infections.
- **A long anchor is a fragile anchor.** Two doc patches missed because the
  anchor text had been re-wrapped since it was read. Short unique anchors,
  verified with `grep -c` first, and a patcher that writes nothing unless
  every anchor matches.
- **Two research passes disagreed about sapovirus, and both were right.**
  The burden pass ranks it #2 in the world; the viral pass says it is not
  significantly attributable in a high-income case–control series and that
  its post-vaccine rise is relative, not absolute. Different populations.
  The resolution was the Director's "some are just stronger" instruction: it
  earns a **tier** slot, not a mechanic slot.
