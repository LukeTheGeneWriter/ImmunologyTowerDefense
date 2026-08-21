# Session Handoff — 2026-08-21 (Sprint 3 built and verified, awaiting playtest)

A checkpoint written by the head session. **This is a living file —
overwrite it at the next checkpoint rather than accumulating dated copies.**
It is not one of `WORKFLOW.md` §3's canonical docs; if it contradicts
`SPRINT_PLAN.md`, `ENGINE_STATUS.md`, or `INTERFACE.md`, those win.

## Status: Sprint 3 is code-complete, verified, and documented

Everything in `SPRINT_PLAN.md`'s scope is built. All verification was run
by the head session directly (see `ENGINE_STATUS.md` → "Build status
(Sprint 3)" for the actual numbers). **The one thing left is the Director's
playtest.**

Commits on `main`, all pushed:

| Commit | What |
|---|---|
| `830e1e5` | "Sprint 3 plans" — previously staged planning docs, committed unchanged |
| `74a2fb6` | Director's 2026-08-21 revisions (macrophage 20, parameterization, items 6–7 folded in) |
| `8eaca14` | Code agent's implementation |
| *(this)* | Head session's verification pass + all four doc updates |

## What happened to the Code agent

It hit its usage limit mid-sprint, after committing working code and a
successful build but **before writing any documentation**. The head session
re-ran every verification from scratch and wrote `ENGINE_STATUS.md`,
`INTERFACE.md`, `TEAM_RETRO.md`, and `CHANGELOG.md` from the commit
message, code comments, and fresh harness output. Nothing was lost. The
process lesson is recorded in `TEAM_RETRO.md`: **brief agents to write docs
incrementally, not as a final step.**

## The two things the Director needs to look at

1. **Does population visibly stay bounded, and do the two deaths read as
   different?** That's `SPRINT_PLAN.md`'s own question. Place towers, watch
   the HUD's active-unit count climb and stop. A neutrophil hitting 5 kills
   should flash and burst; a macrophage hitting 20 should just go.
2. **Clearing is ~50% slower per unit than Sprint 2** — measured, intended,
   and flagged rather than compensated for. Proximity contact and the
   population cap landed in the same sprint, so if the board starts losing
   ground, that interaction is why. The knob is `ContactRadiusFineTiles`
   (per-tower, default 2). **Do not** revert to coarse-slot detection.

## Still-open question the Director has not answered

**Do progenitor upgrades apply retroactively to living cells?** Currently
no — a unit holds a value snapshot of its tower's tuning from emission
time, so upgrading a tower improves only its future children. Head
session's judgment call, flagged but unruled. A one-line change today; not
one once an upgrade UI exists.

## Known gap in verification

**Placement was never exercised through the running build's UI this
session.** Scripted clicks couldn't take foreground focus
(`SetForegroundWindow` refused — the Director was using another window at the
time; an unfocused build doesn't tick, so two
captures 75s apart came back pixel-identical — that's the tell, not a
hang). The click/picker path is unchanged code that Sprint 2 verified, and
the real `PlaceTower`/emission path is driven headlessly by
`LifecycleVerification`, so the mechanism is covered. But the first real
click belongs to the Director.

## Next sprint's likely scope (not yet proposed to the Director)

Depends entirely on how the playtest goes. If population and pacing feel
right, `BACKLOG.md`'s open design questions are the queue — the ATP/economy
layer is the biggest unblocked one, and most of Sprint 2–3's numbers are
explicitly waiting on it before they can be balanced rather than guessed.
