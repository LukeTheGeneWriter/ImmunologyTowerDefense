# Sprint Plan — Sprint 35 (opened and closed 2026-11-05)

Director, on the outline of everything still outstanding: *"Make the
knowledge hub scrollable with a fade out and have the 5 most recently seen
pathogens occupy the visible spaces. A user can browse this way but still
has the most useful info available. Your other changes are scoped well,
please work them out. This will be a big sprint with everything."*

Sprint 34's plan and outcome are in git (`6ca032b`, `a3575ff`) and
`CHANGELOG.md`.

---

## Scope

**Everything outstanding, in one sprint.** The head of the queue was the
Director's playtest report — *"I gave it a play test, and did not see any
messages for the rounds"* — and behind it the four engineering gates in
front of the ten specified organisms, the live bugs, and the corrections
the Sprint 34 research produced.

| Strand | What |
|---|---|
| A | The harness can see the screen — the capability that did not exist |
| B | The round message, measured and then actually fixed |
| C | The four gates (barcode, sprites, the ribbon, two forms) |
| D | Three live bugs found on the way |
| E | Corrections to built code and to standing design |

## A. The measurement, and the thing it overturned

**The handoff's diagnosis was wrong, and it was worth the hour to find
out.** Sprint 33's caption had 41 green checks and the Director saw
nothing, so the expected cause was a wiring bug — a string written into a
Label that never reached a pixel. The handoff listed three candidates to
measure.

None of them. `UiProbe` + `UiTreeDump` boot the real game, drive a real
`RoundController` through its phases and read the **live visual tree**:
the caption's text was correct, unique, attached to the panel root,
displayed, and drawable in the shipped font, **in every phase**.

The round's identity was **11 px of dim grey in the top-right corner,
under two 30 px numbers, beside a 48 px button, with four facts run
together by middots** — and in a browser window narrower than the 1920 px
reference the panel scales that down further. A first encounter with an
organism the player had never met was given the smallest type on the
screen.

So: **not a wire, a surface.** Which collapses the two top queue items
into one piece of work, because the surface the message needed is exactly
`ROSTER_EXPANSION_SPEC.md` §4.2's debut card — the thing the Director had
flagged as *"I didn't ask for that last part."*

**The capability is the durable half.** `UiProbe` gives the Editor
assembly the live tree by name, with the two questions a UI check needs:
*what does this say*, and *would the player see it*. Eight defects in this
project have now come from asking "is this value consumed" — this is the
first tool that can ask it on the UI side.

Its limit is stated rather than papered over: **batchmode runs no layout
pass**, so `worldBound` is all zeroes and nothing here can prove a line is
legible, unclipped or on top. That still needs an eye on a build.

## B. What the player now gets

- **A round banner**, centred, every buy phase: the round number, the
  vehicle at 22 px, and on a debut round the organism's name, its sprite
  at 86 px, and **the one thing that will not work**, stated as a negative
  (§4.3's rule — a positive hint deletes the puzzle).
- **A first-sighting banner** (§4.5) the first time a species actually
  arrives on the board in a run. Three surfaces, one fact each: the debut
  card teaches, the banner confirms, the HUD tracks.
- **The HUD caption keeps the residue** — what is new, and how to start.
  Four facts in 11 px was one fact nobody read.

## C. The four gates, all closed

| Gate | What shipped |
|---|---|
| Barcode 10 → 12 bits | The six live tags re-cut from the spec's verified 16-tag set; the **ten unbuilt tags held in `SpeciesRoster.ReservedAntigens`**, because the property that matters (120 pairs ≥ 6 apart) is only true of the set as a whole |
| Per-species sprites | `SpriteShapes.ForSpecies` + `Palette.ForSpecies`; rotavirus's wheel, *V. cholerae*'s comma, A/E *E. coli*'s rod, all three of which had rendered as generics since Sprint 24 |
| The knowledge ribbon | The Director's design: five visible slots, the whole roster behind them, ordered most-recently-seen, edges fading under the card |
| Two forms | `PathogenForm`, with **one real shipped user** rather than a mechanism waiting for one |

**The two-form path got a real user on purpose.** Building it for the four
unbuilt organisms that need it would have been a mechanism with no caller
— the dead-feature pattern, pre-committed. A/E *E. coli*'s actin pedestal
is assembled *under an intimately attached bacterium* (intimin–Tir,
microvilli effaced beneath it), so the silhouette now changes exactly when
the fact about the organism changes, and being sloughed costs it the
anchor. True on both sides.

## D. Three live bugs, found on the way

1. **The knowledge ladder has been calling IgA "Complement" since Sprint
   25.** `KnowledgeView` held a positional array of six labels against a
   ladder of five rungs; Sprint 25 moved Complement off the ladder and the
   array was never updated, so every index from there on was one out — and
   the 70% rung, which is Secretory IgA, was labelled **Complement** in
   the expanded ladder *and* in the unlock ribbon.

   This matters more than its size. `PLAYTEST_LOG.md` has carried an open
   question since Sprint 22 about why the Director never perceived IgA,
   and `KnowledgeLadder`'s own header already records that its *built*
   flag was left false for a sprint, so the HUD called IgA "planned" while
   it was running. **The one surface that announces IgA has never once
   said "IgA".** Now derived from the capability, so it cannot drift
   again.

2. **`KnowledgeView.SpeciesIcon` drew rotavirus as a bacterium** —
   `case 0 / case 1 / default`, and rotavirus is species 3. Routed through
   the same per-species dispatch the board uses.

3. **The left fade would have lied.** Caught by its own harness check: the
   cached "what is on screen" started disagreeing with what *was* on
   screen, so the first update never fired and a ribbon at its start would
   have claimed there was more behind you.

## E. Corrections landed

- ***V. cholerae* swims, it does not steer.** Non-chemotactic mutants
  out-compete wild type across the small intestine, so "run straight and
  get everywhere" replaces "steer at the wall" — truer, and cheaper than
  what it replaced. Measured on the board: a steering bacterium drifts
  **+0.75 cells** wallward over its lumen transit, *Vibrio* **−0.30**.
- **An applicability flag, not a multiplier.** *V. cholerae* never enters
  a cell, so the two knowledge rungs that act on an infected cell are
  inert against it and the HUD now says **"no effect on this one"**. The
  §1d shop row inherits the flag when it is built; it also pre-answers
  *Giardia* and *Cryptosporidium*.
- **§4a's parasite class, twice over** (`GAME_DESIGN.md`): re-spec'd as
  single-slot with a size multiplier — not one enteric protozoan needs the
  multi-slot footprint that deferred the whole class since Sprint 2 — and
  its clearance rule corrected from "direct combat damage, more of it" to
  **expulsion**. The player's instinct on meeting this class, build more
  killers, is the wrong answer for the entire group.
- **§1d gets its applicability amendment**; `PATHOGEN_ROSTER.md` loses two
  overstatements (adenovirus F41 is not a cliff; norovirus's 2–4 year
  cycle is historical only).
- **The eukaryote hue is decided** (desaturated plum) and deliberately
  *not* added to `Palette` until something uses it.
- **`docs/INTERFACE.md`'s Sprint 22–31 backfill**, a `WORKFLOW.md` §5.3
  checkbox that had been ticked without being true.

## Stopping points — what was deliberately not done

- **None of the ten organisms is built.** The gates are down; the
  organisms are the next sprint's work, and the shortlist the colonic
  ruling unlocked (*Entamoeba* first) is still unranked.
- **"Shed antigen" is still designed, not built.** It needs one new
  lumen-side entity and it amends a locked section — but its only emitter
  would be *Giardia*, which does not exist yet. Building it now would be a
  mechanism with no caller, which is exactly what the two-form path was
  deliberately *not* allowed to be. It lands in the sprint that builds
  *Giardia*.
- **No layout assertions.** The probe cannot see pixels, and a check that
  pretended otherwise would be the same mistake one level up.

## Verification

`Sprint35Verification`, **94 checks** in eleven parts, including a
negative control on the probe itself — a probe that can only answer
"displayed" is not a probe.

**Suite: 26 methods, 983 checks, all green** (was 25 / 889).
`Sprint29Verification`'s `BarcodeBits == 10` was replaced rather than
edited to 12: it asserted a *decision*, so a correct widening would have
had to edit the test to pass. It now derives the width from the Hamming
sphere-packing bound for the actual roster — a necessary condition, so it
can never falsely fail. **Fifth instance of a harness owning a number it
is not testing.**

Build: WebGL **Succeeded, 9,328,875 bytes, 0 errors**.
