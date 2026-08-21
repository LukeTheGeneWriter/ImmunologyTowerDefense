# Session Handoff — 2026-08-21 (mid–Sprint 3)

A checkpoint written by the head session because it was approaching its
usage limit mid-sprint. **This is a living file — overwrite it at the next
checkpoint rather than accumulating dated copies.** It is not one of
`WORKFLOW.md` §3's canonical docs; if it contradicts `SPRINT_PLAN.md`,
`ENGINE_STATUS.md`, or `INTERFACE.md`, those win.

## Where the sprint stands

Sprint 3 (per-progenitor population cap + unit depletion) is **in flight,
not finished, and not verified by the head session.**

Commits on `main` this session:

| Commit | What |
|---|---|
| `830e1e5` | "Sprint 3 plans" — the previously staged planning docs, committed unchanged at the Director's request |
| `74a2fb6` | Director's 2026-08-21 revisions layered on top as a separate commit (see below) |
| `8eaca14` | Code agent's implementation commit — **written by the dispatched agent, NOT yet reviewed or verified by the head** |

`830e1e5` and `74a2fb6` are pushed. **`8eaca14` was not pushed** — check
`git status` / `git log origin/main..main` before assuming remote state.

## Director decisions this session (2026-08-21) — all already in the docs

1. **Macrophage kill limit 15 → 20.** Now marked Director-confirmed in
   `GAME_DESIGN.md` §6d; it had been an unconfirmed working default.
2. **Parameterize every lifecycle number.** Defaults on `UnitProfile` per
   unit kind; each tower holds its own mutable copy seeded at placement;
   units receive their tower's values at emission time. The stated reason
   is that a future progenitor upgrade should be "buy a higher kill count
   for this tower" — one field write, no other code change.
3. **Two Sprint 2 gaps folded into Sprint 3** rather than deferred:
   kill attribution and coarse-slot contact detection (scope items 6/7).

## Open question the Director has NOT answered

**Do progenitor upgrades apply retroactively to living cells?** The
implementation currently says no — a unit keeps the tuning values it was
born with, so upgrading a tower improves its *future* children only. That
was the head session's judgment call (simpler; reads correctly — a cell
does not retroactively gain granules), flagged to the Director but not
ruled on. Cheap to change now, much less so once an upgrade UI exists.

## What the head session found reading the Sprint 2 code

Both were folded into the agent's brief; recorded here so they are not
rediscovered a third time:

- **Units had no despawn path at all.** `PrefabPool.Release` was never
  called for a `SearchUnit` — `BoneMarrowManager.Emit` called `pool.Get()`
  and that was the whole lifecycle. Every depletion behavior in this
  sprint depends on that path existing, plus a pool reset so a recycled
  unit does not carry a stale kill count or tower back-reference.
- **A strict exact-fine-tile contact test would break combat.** With 49
  fine tiles per coarse slot a random-walking unit would almost never
  connect. `SPRINT_PLAN.md` item 7 therefore specifies a tunable
  *proximity radius* (default 2 fine tiles), with an explicit warning not
  to "fix" it into an exact-tile test later.

## Immediate next step for whoever picks this up

The head session's **integration and re-verification has not happened**
(`WORKFLOW.md` §5 step 5 / §5.3). Nothing here has reached the Director.
Specifically unverified by the head:

- the headless harnesses (`CombatVerification`, the agent's new
  `LifecycleVerification`) actually pass, and against real production
  classes rather than reimplementations;
- the Sprint 1 cytokine regression numbers still match exactly
  (OFF: 2.99/3.14/2.84, ON: 0.20/0.00/0.00 — a difference is a real
  regression, do not re-baseline it);
- a Windows build compiles, launches, and reaches a playable state;
- **the interaction flagged in `SPRINT_PLAN.md` item 7**: proximity
  contact makes clearing slower while emission stays capped, so pathogens
  may now outpace the player. The agent was told to report the observed
  change rather than quietly re-tuning other numbers to hide it — check
  that it did.
- `ENGINE_STATUS.md`, `INTERFACE.md` (open questions 3 and 6 should now be
  resolved, not still reading as open), and an appended `TEAM_RETRO.md`
  Sprint 3 entry.

Run `WORKFLOW.md` §5.3's stopping-point checklist plus `SPRINT_PLAN.md`'s
own definition of done before any build reaches the Director.

## Environment note

This session's memory directory for the project was **empty** — every fact
above came from the repo, which is `WORKFLOW.md` §2 working as intended.
Keep it that way: put things in the repo, not in session memory.
