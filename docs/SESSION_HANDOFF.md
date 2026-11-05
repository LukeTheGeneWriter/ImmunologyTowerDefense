# Session Handoff — 2026-11-05 (after Sprint 35)

A checkpoint written by the head session. **This is a living file:
overwrite it at the next checkpoint rather than accumulating dated
copies.** It is not one of `WORKFLOW.md` §3's canonical docs; if it
contradicts `SPRINT_PLAN.md`, `ENGINE_STATUS.md` or `INTERFACE.md`, those
win.

## START HERE — the Director plays first

> **Before any work, before reading the rest of this file: serve the
> build and hand it to Luke.** He asked for this explicitly at the end of
> Sprint 35 (2026-11-05): *"remind me to play first thing next time."*
>
> ```
> tools/serve_webgl.py        # → http://localhost:8000
> ```
>
> The build at `590119e` is already made (WebGL Succeeded, 9,328,875
> bytes, 0 errors), so this is a one-line start, not a 30-minute one. If
> `main` has moved since, rebuild with `tools/unity.sh webgl` first.
>
> **Why this outranks the queue.** Four playtest questions are open and
> three have been open since Sprint 32, because two sprints running went
> somewhere else. Sprint 35 changed a great deal of what the game *says*
> — a new buy-phase card, a first-sighting banner, a rewritten HUD
> caption, per-species sprites, a scrolling knowledge ribbon — and
> **none of it has been seen by a human.** The harness explicitly cannot
> judge any of it: batchmode runs no layout pass, so nothing in the suite
> can tell legible from illegible. That is exactly the gap the Sprint 33
> caption fell through.
>
> **What to watch for, and ask him about:**
> - Do the round messages land now, or do three surfaces crowd each
>   other? (Debut card 16 s, plain round 7 s, first-sighting banner
>   2.6 s — all guesses.)
> - Does the 40-round curriculum read as teaching?
> - Is a one-burst round 40 exciting, or a wall of sprites?
> - **Is IgA perceptible now that the ribbon finally calls it IgA?**
> - Can he tell A/E *E. coli* from a generic gut bacterium at board
>   scale? (Known weak pair; the free form has no tell by design.)

## Then — what the next sprint is

**Build the organisms. The gates are down.**

Sprint 35 closed all four engineering gates that stood in front of
`ROSTER_EXPANSION_SPEC.md`'s ten, fixed the round-messaging complaint, and
cleared the correction backlog. What is left in front of new content is a
Director decision about *which* content, not engineering.

1. **The ten specified organisms** (`ROSTER_EXPANSION_SPEC.md` §1, §3,
   §4.4). Everything they need now exists: a per-species sprite path, a
   12-bit barcode with their tags already reserved and verified, a ribbon
   that scales, a two-form path, and a debut card that can carry a
   portrait and a warning. Their sprite specs are primitive-level and
   their warning lines are written.
2. **Or the shortlist the colonic ruling unlocked**, which is still
   unranked: *Entamoeba histolytica* first (it answers **two** of the five
   standing "no answer in the roster" gaps), then *Shigella*,
   *Campylobacter*, *Trichuris*, *C. difficile*, ETBF.
3. **Or a playtest sprint**, which is the honest alternative — see below.

**The strongest argument for playtesting first:** four questions are open
and three of them have been open since Sprint 32, because two sprints in a
row went on something else. The game now says a great deal more than it
did; nobody has watched a human read any of it.

## Which machine are you on?

| | Windows | Linux (CachyOS) |
|---|---|---|
| Clone | `C:\Users\lukef\ImmunologyTowerDefense` | `~/Projects/Biotech/ImmunologyTowerDefense` |
| Shell | PowerShell | fish (scripts are bash, run directly) |
| Unity | `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` | `~/Unity/Hub/Editor/6000.6.4f1/Editor/Unity` |
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` (26 methods) |
| Build / serve WebGL | `BuildScript.BuildWebGL`; `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/unity.sh webgl`; `tools/serve_webgl.py` → http://localhost:8000 |
| Look at sprites | — | `tools/unity.sh SpriteShapeDump.DumpAll` → `_spritedump/` |
| **Look at the UI tree** | — | `tools/unity.sh UiTreeDump.DumpAll` → `game/Logs/ui_tree.txt` |

**`git pull` before doing anything.** GitHub is the only sync point.
**The Windows machine still needs Unity `6000.6.4f1`** (via the Hub, with
"Web Build Support") before it opens the project.

**State as of this handoff:** `main` at Sprint 35, **26 verify methods /
983 checks green**, WebGL **Succeeded at 9,328,875 bytes, 0 errors**. The
local web server **was running** at http://localhost:8000 when this was
written; restart it with `tools/serve_webgl.py` if it is not.

## What Sprint 35 was

The Director asked for "a big sprint with everything", having first
settled the one open design question himself:

> *"Make the knowledge hub scrollable with a fade out and have the 5 most
> recently seen pathogens occupy the visible spaces. A user can browse
> this way but still has the most useful info available."*

### The thing to carry forward above all else

**The handoff's diagnosis was wrong, and measuring it first is what saved
the sprint.** The previous handoff said the round caption did not reach a
pixel and listed three mechanical candidates. A harness that boots the
real game and reads the live visual tree found the caption **correct in
every phase** — right text, one element, attached, displayed, renderable.

The defect was that the message was **11 px of dim grey in the corner,
under two 30 px numbers, four facts run together**. Drawn perfectly.
Unreadable.

Three things follow, and they are worth more than the code:

1. **The project's standing pattern primed everyone for the wrong
   answer.** Seven previous defects were "a value produced and never
   consumed", so the eighth was assumed to be one too. It was a *design*
   failure wearing the same coat.
2. **The fix and the "feature I didn't ask for" were the same work.** The
   surface the message needed is §4.2's debut card.
3. **The new harness explicitly cannot catch this class of bug.**
   `UiProbe` sees the tree; batchmode runs no layout pass, so nothing
   sees pixels. An eye on a real build is still load-bearing, and the
   next person should not read a green suite as covering it.

### What shipped

- **`UiProbe` + `UiNames`** — the live visual tree by name from the Editor
  assembly: what an element says, and whether anything hides it. The
  missing half of the harness, with its limits documented in
  `INTERFACE.md`.
- **`RoundBannerView`** — every buy phase names the round and its vehicle
  at 22 px; a debut round adds the sprite at 86 px, the name, and **the
  one thing that will not work**, stated as a negative. Plus the §4.5
  first-sighting banner.
- **All four gates**: barcode → 12 bits (the ten unbuilt tags reserved in
  `SpeciesRoster.ReservedAntigens` and verified as part of the whole 16),
  `SpriteShapes.ForSpecies`, the five-slot scrolling ribbon, and
  `PathogenForm`.
- **Three live bugs**, all the same shape — two lists kept in agreement by
  hand. The worst is below.
- **Corrections**: *V. cholerae* swims straight (measured: +0.75 cells
  wallward for a steerer, −0.30 for it), the §1d applicability flag,
  §4a's parasite class, two `PATHOGEN_ROSTER` overstatements, the
  eukaryote hue, and `INTERFACE.md`'s Sprint 22–31 backfill.

### The bug most likely to matter to the Director

**The knowledge ladder has been calling secretory IgA "Complement" since
Sprint 25**, in the expanded ladder and the unlock ribbon both.

Put beside two facts already on file — IgA's `Built` flag stayed false for
a sprint after it shipped, so the HUD called a running mechanic "planned";
and the Sprint 22 "IgA was never perceived" miss was diagnosed as round
design — there is now a third, duller explanation on the table: **the
surface that announces IgA has never once said its name.** That has to be
ruled out before the round-design diagnosis gets another sprint.

## Open, in priority order

Full detail in `BACKLOG.md` → "Open after Sprint 35".

1. **Four playtest questions**, three of them two sprints old: does the
   40-round curriculum read as teaching; is a one-burst round 40 exciting
   or a wall of sprites; is IgA perceptible **now that it is called
   IgA**; and do the three new messaging surfaces land or crowd?
2. **The ten organisms**, or the unranked colonic shortlist.
3. **Nothing asserts on UI layout.** The defect this sprint fixed lives
   exactly in that gap. A play-mode harness or a screenshot diff is a
   sprint of its own; until then, look at the build.
4. **"Shed antigen" is designed, unbuilt, and now has a reason to wait** —
   its only emitter would be *Giardia*. It belongs to the sprint that
   builds *Giardia*.
5. **Nontyphoidal *Salmonella* should lead its own sprint** — it changes
   §4a's advance rules.
6. Tuning guesses that need eyes: the banner dwell times (7 s / 16 s), and
   whether free A/E *E. coli* is too close to the generic bean at 20 px.

## Traps worth carrying forward

- **Measure before you believe a handoff — including this one.** The last
  one named three candidates for a bug and all three were wrong. It was
  written in good faith by a session that had the same information you do.
- **A green harness is not a working feature, and now: a correct tree is
  not a readable screen.** Eight defects have come from asking "is this
  value consumed?". The ninth came from asking it and getting *yes*.
- **`GameBootstrap` creates many ROOT GameObjects.** A harness that
  destroys only its own bootstrap object leaves an entire parallel game in
  the scene, and the next `FindFirstObjectByType` returns the stale one
  while `UiProbe` answers for the newest tree. The tell is a pass/fail
  pattern where the first boot works and every later one does not.
  `Sprint35Verification.Booted` records the scene's roots and takes down
  everything that appeared.
- **A cache of "what is on screen" that starts out disagreeing with what
  IS on screen turns into a write that never happens.** The ribbon's left
  fade was visible from construction with its flag false, so the first
  update saw no change to make. Initialise the cache against reality.
- **Inside a filled shape, the only contrast is darkness** — a white ring
  painted into an opaque body is invisible. And **a tell must be large and
  bright or it is noise at 20 px**. Both were found by dumping the raster
  and looking at it, which took two minutes and should be the default.
- **A harness threshold near its expected value is a coin flip.** The
  *Vibrio* drift check first asserted `|drift| < 0.25` against a measured
  −0.30 and failed on a *correct* implementation; an unbiased walk from a
  near-wall spawn drifts away from the wall, which is the signature, not a
  fault. Assert the claim ("does not aim at the wall"), not a symmetric
  tolerance nobody derived.
- **Replace a harness check that owns a decision; do not edit it.**
  `Sprint29Verification` asserted `BarcodeBits == 10`, so a correct
  widening would have had to edit the test to pass. It now derives the
  width from the Hamming sphere-packing bound — a necessary condition, so
  it cannot falsely fail. Fifth instance.
