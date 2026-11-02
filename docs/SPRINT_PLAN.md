# Sprint Plan — Sprint 33 (opened and closed 2026-11-02)

Director, straight after Sprint 32 and skipping its playtest: *"make 40
rounds with a bunch of different mixes and matches of pathogens. Reference
Bloons TD round descriptions (and how they show up to the player) if you
need inspiration."*

Sprint 32's plan and outcome are in git (`92ff7bc`) and `CHANGELOG.md`.

---

## Scope

**One strand: the round table.** Forty hand-authored rounds, the curriculum
rules from Sprint 32 holding across all of them, and whatever lever set is
needed to make forty rounds actually differ from one another.

### What reading Bloons TD was actually worth

Forty rounds is Bloons TD 6's Easy length. Four habits of its round table
transfer; one does not.

| Habit | Carried over as |
|---|---|
| A new enemy debuts in a small dose, then recurs in bulk | Debuts at 6/11/15, each back within two rounds, never on the hardest round so far |
| Difficulty is a sawtooth with a rising envelope | 12 of 40 rounds are lighter than the round before; the deepest dip is 17% below the running peak |
| **Grouped rushes** | `RoundDefinition.Bursts` — per-round delivery shape |
| The player sees what is coming | The buy-phase caption names any organism debuting next round |
| ~20 enemy types by round 40 | **Does not transfer.** This game has six organisms, three generic. |

The third one was the find. The bolus already dropped its cargo in bursts,
but the count was a global constant, so every round in the game arrived the
same way. Made per-round, it is the strongest source of variety available
without new content — and it is the honest form of "wall pressure":
organisms per clump, not organisms per round.

### The lever that had to be replaced, one sprint after shipping

Sprint 32's `WallPressure` multiplier on the economy baseline **cannot
carry a 40-round curve**, and this was arithmetic rather than judgment:
baseline (16, +6 a round) × a rising multiplier hits the batch ceiling of 90
at **round 8**, so rounds 8-40 would every one of them have delivered
exactly 90 organisms however the multiplier was tuned. Each round authors
its own `Batch` now. `GAME_DESIGN.md` §5d always said the linear growth was
a placeholder and that a real difficulty curve was out of framework scope;
this is where it went. Pressure still drives the endless tail past 40.

---

## Stopping point — all met

1. 40 scripted rounds, each fully authored (tagline, cast, count, delivery
   shape), no round silently falling back to the economy curve.
2. The curriculum rules hold across all 40, asserted rather than asserted-ish:
   one named debut at a time, three rounds apart, never on the hardest round
   so far, everything recurring at least five times, nothing new in the last
   ten rounds.
3. The envelope rises **and** individual rounds dip — a monotone ramp would
   satisfy half of that, so both halves are checked.
4. Delivery shape measured as a property, through the real spawner: a
   one-burst round lands its organisms in **1 lane** of wall against **7**
   for a seven-burst round.
5. Taglines promise only organisms that exist, and deliver what they name —
   the Sprint 32 drift, pinned.
6. The player is warned before a debut round.
7. `tools/unity.sh verify` green, WebGL builds clean.
8. Docs current: `GAME_DESIGN.md` §5d, `INTERFACE.md`, `CHANGELOG.md`,
   `BACKLOG.md`, `TEAM_RETRO.md`, `AGENT_HANDBOOK.md`, and the handoff.

## What the next sprint should pick up

1. **The Director has now skipped two playtests.** Sprints 32 and 33 are
   harness-measured, not played. Everything about whether the curriculum
   *reads* as teaching, whether IgA is perceptible, whether a one-burst
   round 40 is exciting or just a wall of sprites, is unverified by a human.
2. **Astrovirus**, and then **CMV as the boss for round 40's slot** — the
   back half of the script is a frame waiting for content, and this is the
   content. Round 40 is currently made of concentration because the boss
   that belongs there does not exist.
3. **Norovirus GII.4** is the most interesting *late*-round organism in the
   roster: antigenic replacement on a real 2-4 year cycle would invalidate
   knowledge the player spent a whole run accumulating, which is a back-half
   mechanic by construction.
4. Round length is still a global 30s for all forty rounds
   (`FoodItemTransitSeconds`). Varying it needs the adaptive arm's timers
   checked, not assumed.

## Not in scope, deliberately

- Anything from Sprint 32's open list: astrovirus, the `UnitProfileAudit`
  gap, the degranulation × A/E *E. coli* interaction, the ten-sprint
  `INTERFACE.md` backfill.
- Per-round round *length*, for the reason above.
- Raising the 150-pathogen live ceiling (a performance question).
