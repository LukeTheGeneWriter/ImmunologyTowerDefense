# Sprint Plan — Sprint 32 (opened 2026-11-02)

Director's call this session: **rounds and new pathogens** as the main
body, the **edge bug** first because it is a confirmed bug, and **yes to
innate turnover** — make the kill limits bind rather than deleting the
rows.

Written by the head session after reading `WORKFLOW.md`, the Sprint 23–31
handoff, and the round/spawn path. One finding below moved the centre of
gravity of the sprint; it is item C1 and it comes before any new species.

---

## The finding that reshaped the scope

**No named species can spawn in play.** Read the path:

- `PathogenSpawner.SpawnFromFood` → `agent.Initialize(..., foodDef.RollClass())`,
  i.e. a `PathogenClass`.
- `PathogenAgent.Initialize` sets
  `Species = speciesOverride ?? (classOverride.HasValue ? SpeciesRoster.GenericFor(class) : PickRandomSpecies())`.
- `PickRandomSpecies()` returns `GenericVirus` /
  `GenericIntracellularBacterium` / `GenericLargeBacterium` and nothing else.
- Nothing in `Assets/Scripts` ever passes a `speciesOverride` except
  `RequestSpawnNear`, which **inherits the parent's species** — so
  replication cannot introduce a named organism either.

Therefore rotavirus, *Vibrio cholerae* and A/E *E. coli* — the whole
content of Sprints 24 and 25, with working traits in `PathogenAgent` and
rows in the knowledge HUD — have never been on the board in a playtest.
`RoundScript` still speaks the pre-Sprint-24 language of three classes,
and its taglines name *Salmonella*, poliovirus and norovirus, none of
which are in the roster.

This is the **fifth** instance of the pattern the last session named: a
feature that is charged for and never binds. The previous four were an
upgrade ceiling that never bound (×3) and a barcode guarantee that was
mathematically unreachable. The habit that catches them — assert the
property, not a proxy — is why C1 ships with a harness that enumerates
what actually spawns rather than checking that the spawner was called.

**Consequence for scope:** the cheapest route to "new pathogens on the
board" is not a new species. It is making the three already paid for
reachable, and giving rounds a species-level curriculum. Astrovirus goes
in last, onto rails that work, and is the one item allowed to slip.

---

## Scope

### A. The edge bug — immune cells walk out of the tissue into the blood

Director-reported, diagnosed in `BACKLOG.md`, not fixed. Nothing
constrains an immune cell to the tissue band: `Chemotaxis.ChooseNextStep`
rejects a candidate only on `board.InFineBounds`, the whole board.
Pathogens have the discipline (`StepTissue` checks `BandAtAxisIndex`);
immune cells never got it. It shows at round start because §2a rung-1
entry puts cells at the tissue's base-side edge with no gradient yet.

**Fix shape:** a band permit in the candidate loop of
`Chemotaxis.ChooseNextStep`, so every mover inherits it — including
`Lymphocyte` and `DendriticCell`, which call the same static.

Two things to get right rather than assume:

- **Per-kind, not blanket.** The lymph node is *in* the base band, so
  dendritic cells and lymphocytes legitimately travel there. The permit
  is a serialized field on `UnitProfile` — band layout is already DATA on
  `BoardConfig` and this follows it, no magic numbers in movement code.
- **No stranding.** A cell already outside its permitted bands must be
  able to walk back in. Hard-filtering alone would freeze whoever is
  already in the blood, so the fallback admits steps that strictly reduce
  the distance to the permitted region, and never returns an empty
  candidate set.

### B. Innate turnover — make the kill limits bind (Director: yes)

Measured inert: a macrophage averages **0.9 kills** in a 45s round
against a ceiling of 20, a neutrophil **1.0** against 5. So
"+8 kills before the cell is spent" costs 40 ATP and writes nothing.

**Measure before tuning, again.** Re-run `KillLimitDiagnostic` first and
take the distribution, not the mean, because the limit has to bind for
the *typical* cell and not only the lucky one. Then pick limits from the
measurement and re-point the two upgrade rows at the new base.

Two consequences to check rather than discover in playtest:

- **Neutrophils degranulate on depletion.** Turnover means more terminal
  bursts, so more collateral damage and more debris — which is exactly
  what A/E *E. coli* respires once C1 makes it reachable. These two
  strands interact and the sprint has to look at them together.
- **It thins the board further**, on top of Sprint 31's dispersal. Check
  the standing population, not just the rate.

### C. Rounds and pathogens

**C1 — species actually spawn (the keystone).** `RoundDefinition` carries
species weights, not three class weights; `SpawnFromFood` rolls a species
and passes it through as `speciesOverride`; class stays derived, so every
call site that still speaks class keeps working. Scripted rounds rewritten
so the tagline names what actually arrives.

**C2 — a round curriculum.** Species are learned independently (§5c), so
round order *is* the teaching order: introduce one new organism at a time
against a known board. This is also the answer to the **Sprint 22 IgA
miss**, which diagnosed as a round-design problem rather than an IgA
problem — the player never saw IgA because wall pressure never built, and
a defence against a problem you never have is invisible by construction.
Rounds need to ask something of the wall.

**C3 — astrovirus (allowed to slip).** The ranked standout in
`PATHOGEN_ROSTER.md`: capsid alone opens tight junctions, so it breaches
with **no infection and no stress signal** — it defeats the whole
detection stack by not being detectable. Gated on re-running the barcode
separation search for the **whole seven-species set** at 10 bits, not on
picking a tag that looks far enough away; that shortcut is precisely how
the 8-bit set ended up cross-teaching 21.9% of the time.

---

## Stopping point (testable definition of done)

1. Immune cells do not leave the tissue band at round start; dendritic
   cells and lymphocytes still reach the lymph node; a cell placed in the
   blood walks back into tissue rather than being stranded. Asserted by
   enumeration over the real `ChooseNextStep`, not by eye.
2. A playthrough of the scripted rounds puts **named species** on the
   board, in an order the taglines match, provable by a harness that
   tallies what spawned.
3. Innate cells turn over at a measured rate, and both kill-limit upgrade
   rows change that rate when bought.
4. `tools/unity.sh verify` green (21 harnesses + the new one), WebGL
   builds clean.
5. Docs current: `ENGINE_STATUS.md`, `INTERFACE.md`, `GAME_DESIGN.md`,
   `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md`, and the handoff.

## Not in scope, deliberately

- `SimulationDriver` refactor + headless strategy harness — the Director
  chose rounds over it this sprint; still queued from Sprint 22.
- Epithelial extrusion and the messy-death fork — the other option on the
  table, still designed-not-built.
- The ~60% complement rung re-spec (opsonisation + phagocyte recruitment)
  — a Director call, not implementation, and it belongs with the upgrade
  pass rather than here.
- Virus march speed (~185s, incubation-bound) — left for the Director's
  read now that viruses can actually land.

---

## Outcome (closed 2026-11-02)

**A, B and C1/C2 shipped. C3 (astrovirus) did not, as the plan allowed.**

| Item | Outcome |
|---|---|
| A. Edge bug | Fixed. Measured 43% of steps outside the tissue under the old rule (34,402 of 80,000), **0** now. Per-kind permit; a stray cell homes in 38 steps worst case; still diffuses (mean depth 2.2 of 13 cells). |
| B. Innate turnover | Macrophage 20 → 5, neutrophil 5 → 3, upgrade steps resized (+2/lvl, +1/lvl). Both rows now change the retirement rate. |
| C1. Species spawn | Rounds carry a cast of species. Round 5 verified end-to-end through the real spawner: 32 A/E *E. coli*, 11 intracellular, 9 rotavirus, 8 gut bacteria. |
| C2. Curriculum | Six scripted rounds, one new organism at a time, organisms recur, wall pressure 1.0 → 1.7 (procedural tail capped at 2.5). |
| Bonus | **A round-hang fixed before it shipped.** Wall pressure ran the batch into the spawner's 150-pathogen live ceiling; a food round never completes if its batch cannot be emitted. Raw curve crosses at round 24, with pressure at round 11. Batch now clamped to 90; no scripted round is clipped. |
| C3. Astrovirus | **Not built.** It needs a non-infecting virus path in `PathogenAgent`, a barrier interaction, a sprite pass and the 7-tag barcode search — a sprint of its own. Scoped in `BACKLOG.md`. |

### Corrections this sprint produced

- **The neutrophil kill-limit row was never inert.** At 5 it bound across
  rounds, just not inside one; the standing claim came from a single-round
  measurement. Only the macrophage's ceiling of 20 was unreachable. So
  5 → 3 is a rebalance, not a repair.
- **Every harness in the repo leaks units into `SearchUnit`'s static
  registry**, which made a measurement move from 4 to 3 depending on how
  many runs preceded it. `Sprint32Verification` drains it and asserts
  run-to-run determinism.
- **`docs/INTERFACE.md` was ten sprints stale** (last per-sprint section:
  Sprint 21). A `WORKFLOW.md` §5.3 checkbox was being ticked without being
  true.
- **Two existing harnesses owned numbers they had no reason to own** —
  `Sprint18Verification` asserted `20 -> 28` and `5 -> 7` as literals, so
  the turnover change broke tests that were not about turnover. Restated
  against their own fixture base and the live per-level constants.

### What the next sprint should pick up

1. **Astrovirus**, on rails that now work — and the knowledge-HUD question
   it raises: a species that produces no debris and no stress signal can
   never be learned about, so what does its HUD row say?
2. **The Director's playtest of this build** — specifically whether the
   curriculum reads as teaching, whether IgA is perceptible now that wall
   pressure climbs, and whether the board still feels populated with
   innate turnover live.
3. **The structural gap**: no harness can read the shipped per-kind unit
   numbers, which is why three dead upgrade rows shipped. A
   `UnitProfileAudit` or a `ScriptableObject` home closes it.
4. **The degranulation × A/E *E. coli* interaction**, which two separate
   Sprint 32 changes just put in contact with each other, unmeasured.
