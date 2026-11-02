# Session Handoff — 2026-11-03 (after Sprint 34 and the Director's playtest)

A checkpoint written by the head session. **This is a living file:
overwrite it at the next checkpoint rather than accumulating dated
copies.** It is not one of `WORKFLOW.md` §3's canonical docs; if it
contradicts `SPRINT_PLAN.md`, `ENGINE_STATUS.md` or `INTERFACE.md`, those
win.

## START HERE — what the next sprint is

**The Director played the build and the game said nothing to him.** That is
the whole top of the queue, and it outranks the ten-organism expansion that
Sprint 34 spent its time specifying.

> "I gave it a play test, and did not see any messages for the rounds or
> showcases of a new pathogen and what it does. I didn't ask for that last
> part."

1. **Fix the round messages.** Sprint 33 shipped a buy-phase caption that
   previews the next round's tagline and names any debuting organism.
   **41 harness checks are green on it and the player sees neither.**
2. **Then build the pathogen showcase** — designed in
   `ROSTER_EXPANSION_SPEC.md` §4.2–4.4, unbuilt. The Director noted he had
   not asked for it, so it is a feature rather than a regression, but it is
   the surface **eight of the ten proposed organisms depend on**.

Order matters: a showcase built on a caption that does not display would be
invisible too.

**What is already ruled out, so you do not spend the time twice:** the
obvious cause — `ApplyPhase` never firing on the opening buy phase — is
**wrong**. `builtForPhase` is initialised to `(RoundPhase)(-1)`, so the
first frame always triggers it. Candidates, to be **measured and not
assumed**: `caption`'s visibility/layout inside `topRight` (it is added to
`Root` but never passed to `UiTheme.Show`); a later `ApplyPhase`
overwriting the text in the same frame; HUD scale clipping the line in the
WebGL build.

**The deeper fix is a harness capability that does not exist.** Nothing in
`Assets/Editor` asserts on a UI element's state. A check that drives a real
`RoundController` through Building → Active → Building and reads
`caption.text` would have caught this. **This is the eighth instance of the
standing pattern and the first on the UI side of the boundary**: the
harness asserts a value is *returned*, nothing asserts it is *displayed*. A
verified string written into a `Label` the player never reads is the same
defect as a value never read at all.

## Which machine are you on?

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

**State as of this handoff:** `main` at Sprint 34, 25 verify methods / 889
checks green, WebGL Succeeded at 9,319,617 bytes with 0 errors. **The local
web server is stopped** — restart it with `tools/serve_webgl.py` when you
need to hand over a playable build.

## What Sprint 34 was

**Mostly research.** The Director asked for ten more common gut pathogens
with sprites and warning messages, and sanctioned *"some are just stronger
bacteria or viruses"* — which closed a real hole, because the roster's
mechanics-only rule had been excluding the world's #2 and #5 enteric
pathogens for being ordinary.

**Nothing new is built.** The ten are specified, with four engineering
gates in front of them.

| What | Where |
|---|---|
| The ten, their sprites, their warnings, the gates | **`docs/ROSTER_EXPANSION_SPEC.md`** |
| An index of all 19 research passes + the findings that changed built code | **`docs/research/README.md`** |
| The passes themselves | `docs/research/` |

### The code that did change

Rotavirus, which the game had been playing as an ordinary lytic virus
arriving at an ordinary dose. Both corrections are real biology:

- **Non-lytic exit** — ~100% release with no permeability change at 72 h,
  so an ignored focus never bursts. It stays self-limiting via the crypt
  repaving under its villus-tip tropism, which is closer to how rotavirus
  actually resolves clinically.
- **Vesicle dose** — one arriving unit is already a dose.
  `PlaqueCrowdWeight` is capped at **1.9**, below the ingress floor of 2.0,
  because §1b Step 2 property 2 is locked and load-bearing and the measured
  ~3.5× would have let a lone virion charge a plaque. Rotavirus's minimum
  dose is now **two organisms where a generic virus needs three** — exactly
  the per-species knob §1b Step 2 said the floor would become.
- **And budding had never been per-species** — a coin flip for 29 sprints,
  with the code's own comment admitting "there is no species roster yet",
  untrue since Sprint 24. Seventh instance of the pattern.

## Two Director rulings to respect

1. **Colonic is fine** (2026-11-02). The anatomy rule had been quietly
   costing four of the most common gut pathogens in the world. It is a
   recorded note now, not a veto, and §1a's bands are a generic mucosal
   cross-section. **This reopens the shortlist**, and the organisms it
   unlocks are unranked because the ruling landed after the ten were
   chosen: *Entamoeba histolytica* first (its trogocytosis answers **two**
   of the five standing "no answer in the roster" gaps), then *Shigella*,
   *Campylobacter*, *Trichuris*, *C. difficile*, ETBF.
2. **"Some are just stronger" is sanctioned.** Two of the ten are explicit
   tier slots (sapovirus, ETEC) rather than organisms with invented powers.

## Numbers worth keeping

- Four organisms crowd **1.9× harder as rotavirus** — 7.019 against 3.694 —
  through the real `GutWall`. **Two rotavirus clear the ingress floor
  (3.8 > 2.0); two ordinary virions do not (2.0 ≤ 2.0).**
- **No rotavirus focus burns out** (0/100). An ordinary virus still does
  (32/100 ≈ 30%), unchanged.
- **The antigen barcode holds at most 12 species** at the shipped match
  radius (constructed 12, Hamming ceiling 18). At r=3 it holds exactly
  **two** — a Plotkin bound. A 16-species roster needs **12 bits**, and a
  generated 16-tag set at minimum distance 6 is in the spec ready to paste.
- **Norovirus's knock-back is ×0.3 with a floor at the ~10% CTL rung** —
  only ~30% of antibody blockade survives a variant replacement, while the
  HLA class-I epitopes are conserved.

## Open, in priority order

Full detail in `BACKLOG.md`, which now opens with "Next sprint — Director's
playtest, 2026-11-03".

1. **The round messages** and then **the showcase** (top of this file).
2. **The three playtest questions are still unanswered** — the session went
   on the messaging gap. Does the 40-round curriculum read as teaching; is
   a one-burst round 40 exciting or a wall of sprites; is IgA perceptible
   now that wall pressure climbs.
3. **The four gates** before any new organism ships (spec §5): widen the
   barcode to 12 bits; build a per-species sprite path (there is none —
   `PathogenAgent` picks from `Class`, two sprites for the whole roster, so
   rotavirus has rendered as a generic ball since Sprint 24); make the
   knowledge HUD scale past six species; let a pathogen's sprite change
   mid-board.
4. **"Shed antigen" is designed and unbuilt** — how the body learns about
   *Giardia*. The parasite sheds its own antigen in extracellular vesicles
   and **antibody binding triggers a massive release**, so attacking it is
   what makes it teachable. Both carriers already exist. §1c is amended:
   **debris is one input, not the only one.** Needs a Director decision.
5. **Nontyphoidal *Salmonella* should lead its own sprint** — more common
   than five of the chosen ten, but its mechanic changes §4a's advance
   rules (the infected enterocyte is *extruded into the lumen*).
6. **Corrections that touch built code**: the built *V. cholerae* actively
   swims; §4a's "parasites don't hide / cleared by combat damage" is wrong
   for every helminth; adenovirus F41 must not be a cliff; norovirus's 2–4
   year cycle is historical only.
7. **`docs/INTERFACE.md`** has Sprint 32/33/34 sections, but the backfill
   for Sprints 22–31 is still missing — a `WORKFLOW.md` §5.3 checkbox that
   was ticked without being true.

## Traps worth carrying forward

- **A green harness is not a working feature.** Sprint 33's caption had 41
  checks and reached no pixel. **Ask where the value is consumed, and
  whether anything asserts on the consumer.** That question has now found
  eight defects in this project.
- **A harness assertion whose threshold sits near its expected value is a
  coin flip, not a test.** `Sprint32Verification` asserted that named
  organisms are the *majority* of round 20, whose cast makes them exactly
  **half**. It passed on luck for two sprints until a Sprint 34 change to
  RNG consumption moved the stream. It now derives the expectation from the
  round's own cast weights with a 3σ tolerance at the emitted batch size.
  Fourth instance of a harness owning a number it is not testing.
- **Put a vacuity guard next to any count-based assertion.**
  `Sprint34Verification`'s first run established 0 of 200 infections,
  because the harness collected host slots at the **crypt** end — which
  rotavirus's tropism correctly forbids. Without the guard it would have
  reported a clean zero burn-out rate from zero infections.
- **A long patch anchor is a fragile anchor**, and inserting after a
  *paragraph* in a Markdown list walks past sibling bullets. Short unique
  anchors, `grep -c` first, and a patcher that writes nothing unless every
  anchor matched.
- **Two research passes disagreeing can be a finding.** Sapovirus is #2 in
  the world by one metric and not significantly attributable by another;
  both are right about different populations. Smoothing it over would have
  destroyed the information.
