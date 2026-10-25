# Sprint Plan — Sprints 23–31 (closed 2026-10-25)

Nine sprints in one session. This file records what they were and what
the next one should pick up; the detail is in `CHANGELOG.md` per sprint,
and the open items are in `BACKLOG.md` under "Open after Sprint 31".

## What they were

| Sprint | Direction (Director) | Outcome |
|---|---|---|
| 23 | "Add cytotoxic T cells, B cells, NK cells — research the numbers first" | Four new towers, each with a distinct job |
| 24 | "Add more viruses and bacteria" | Named species; rotavirus, *Vibrio*, A/E *E. coli* |
| 25 | "Full jump to species; fewer cells; complement is structural; lane changing" | All five delivered |
| 26 | "Get the Greek letters. The γδ look like bacteria" | ImmunoSans font; γδ redrawn |
| 27 | "Allow expanding the bone marrow slots" | Fatty niches, bought by clicking |
| 28 | "Repulsion so they don't all swarm one pathogen" | Crowd desensitisation; 2×4 marrow; shop top-left |
| 29 | "Refine the upgrades and work through the shop" | Four empty towers filled, four dead shop rows woken |
| 30 | "Allow viruses in — they never get through" | They *couldn't*: 0/150. Fixed |
| 31 | "Clumping is still a serious problem" | Measured, re-shaped, 0.74 → 10.94 tile spacing |

## The method that made the difference

Every sprint from 23 on opened with **research or measurement before
implementation**, at the Director's request. That inverted the usual
failure: four times, a reported "balance problem" turned out to be
structural, where tuning would have deepened the hole.

It also produced a steady stream of corrections to things already
shipped — wrong tooltips, an unreachable mathematical guarantee, three
upgrades that charged ATP and did nothing. The rule that emerged:
**a correction is worth more than a confirmation**, and both the research
briefs and the harnesses now say so explicitly.

## Known state

- **21 harnesses, 768 checks**, green. WebGL builds clean, ~9.3 MB.
- Three by-hand diagnostics under `ImmunologyTD/Diagnose`, deliberately
  outside `verify`.
- 11 research passes archived in `docs/research/`.

## What the next sprint should pick up

1. **The edge bug first** — immune cells walk out of the tissue into the
   blood at round start. Diagnosed in `BACKLOG.md`; the trap is that the
   lymph node sits in the base band, so the fix must be per-kind rather
   than a blanket clamp.
2. **Round design and new pathogens.** This is what Sprints 23–31 were
   clearing the way for: the roster is stable, the upgrades are real, and
   the economy has teeth (the Director lost a run and valued it). The
   pathogen shortlist is ranked and cited in `PATHOGEN_ROSTER.md`.
3. **The kill-limit decision** — measured inert, but fixing it is a call
   about whether innate cells should turn over, not a number change.

## Not in scope, deliberately

- `SimulationDriver` refactor and the headless strategy harness, still
  queued from Sprint 22. The diagnostics added this session cover the
  immediate "measure it" need.
- Bone marrow second pass from Sprint 21 — superseded by Sprint 27/28's
  2×4 grid, which rebuilt that geometry anyway.
