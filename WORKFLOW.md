# Immunology Tower Defense — Agent Workflow

This document is the constitution for how this project runs. It was
rewritten 2026-08-19 at the start of Sprint 1, replacing an earlier version
built around several separate, persistent Claude Code sessions (Producer,
Code, UI, HR) coordinating through a device bridge and per-role git
worktrees. That structure is retired. The current model is simpler: **one
head session talks to the Director directly and dispatches focused
subagents for the work that benefits from a fresh, narrowly-briefed
context.** If you're re-reading this because something feels off, re-read
it together with the current `docs/SPRINT_PLAN.md` before assuming the
process itself needs to change again.

Keep this file at the repo root. If you change how the project runs, edit
this file first — it's what every session (head or dispatched) re-reads to
remember how to behave.

**Repository:** [github.com/LukeTheGeneWriter/ImmunologyTowerDefense](https://github.com/LukeTheGeneWriter/ImmunologyTowerDefense)
(public). Being public is fine for an open-in-progress indie project, but
keep an eye on it: no API keys, tokens, or other secrets should ever land
in a commit, even a throwaway one.

---

## 1. Roles

**Director (Luke).** Sets direction, breaks ties on design disagreements,
playtests, and gives feedback in plain language. Talks directly to the head
session — no intermediary needed anymore.

**Head session.** The one Luke talks to. Runs natively on Luke's machine
with real shell access (see `CLAUDE.md`), holds the full picture, and:
- turns direction into a sprint plan,
- does editorial/coordination work itself (updating docs, triaging
  feedback, resolving conflicts between dispatched agents' output),
- dispatches subagents for focused implementation or design work,
- integrates what they produce, verifies it, and reports back to Luke.

**Dispatched agents (Code, Design, Feedback).** Spun up via the `Agent`
tool as needed, not pre-created or kept alive between sprints. Each one:
- starts with **zero memory** of this project — the head must brief it like
  an onboarding doc every time (Section 2), pointing at the exact docs and
  files it needs;
- works from its brief and the shared docs (Section 3);
- self-verifies before reporting done (Section 5);
- can be resumed mid-task by name if still running, but should not be
  assumed to persist sprint-to-sprint.

There's no more standing "UI session" or "HR Claude" — a Design agent gets
dispatched when there's actual visual/UX work to do (none in Sprint 1), and
a Feedback agent gets dispatched periodically to mine `TEAM_RETRO.md` into
`AGENT_HANDBOOK.md`, the same job the old HR Claude role did, just without
needing to be kept running.

---

## 2. Why files, not just chat context, are the real communication channel

Every dispatched agent starts cold. The head session itself may also span a
conversation compaction or a fresh start days later. That means:

- Anything that must survive should live in a **file committed to the
  repo**, not just in a message or a previous turn's context.
- Every brief to a dispatched agent should be written as if the recipient
  has never heard of this project before, with a pointer to the exact files
  that bring it up to speed.
- Git history is the project's long-term memory. Commit often, with
  messages that explain *why*, not just *what*.

---

## 3. Shared documents (the repo is the API between head and dispatched agents)

All under `/docs` in the repo:

| File | Owner | Purpose |
|---|---|---|
| `GAME_DESIGN.md` | Head (reflects Director decisions) | Current vision, mechanics, tower/enemy roster, balance philosophy. The single source of truth all implementation work builds against. |
| `ENGINE_STATUS.md` | Head, informed by Code agents | Architecture overview, key modules, known issues, "state of the engine" as of the last commit. Rewritten at the end of every sprint, not just appended to. |
| `UI_STYLE_GUIDE.md` | Head, informed by Design agents | Visual language, layout conventions, color/asset rules, component inventory. Same rule — kept current, not just appended to. |
| `INTERFACE.md` | Head | The contract between engine and UI/design: data shapes, event names, function signatures either side depends on. Kept current whenever it changes, not discovered stale later. |
| `SPRINT_PLAN.md` | Head | This sprint's scope, the brief(s) given to dispatched agents, and the explicit stopping point (Section 4). Overwritten each sprint; history lives in git log. |
| `PLAYTEST_LOG.md` | Director | Raw, informal notes after each playtest. Dated entries, own words. |
| `CHANGELOG.md` | Head | One entry per sprint: what shipped, what's still rough, what's next. Appended to, never rewritten. |
| `BACKLOG.md` | Head | Ideas and known issues not yet scheduled into a sprint. |
| `TEAM_RETRO.md` | Head or any dispatched agent | Raw, dated log of friction and tips from each sprint (Section 6.1). Appended to, never rewritten. |
| `AGENT_HANDBOOK.md` | Head, or a dispatched Feedback agent | Distilled tips, known points of difficulty, current dispatch practices (Section 6.2). Kept current, not just appended to. |

Rule of thumb: if a fact needs to outlive the turn it was created in, it
belongs in one of these files, not just in conversation.

---

## 4. The sprint cycle

A sprint is the loop from one playtest to the next. Keep sprints short —
small enough that Luke is playing something new every few days.

```
 Director                 Head session               Dispatched agents
 --------                 -----------                 ------------------
 gives direction    -->   writes SPRINT_PLAN.md
                          (scope + stopping point)
                          dispatches Code / Design
                          agents with self-contained
                          briefs                -->    work independently,
                                                        report back or ask
                                                        to be resumed
                          <-- reports done / blocked
                          integrates, verifies,
                          runs the stopping-point
                          checklist itself
                          packages a runnable build
 tests it           <--   hands off build + CHANGELOG entry
 writes PLAYTEST_LOG.md
 (or tells head
 directly)
                          triages feedback,
                          updates BACKLOG.md,
                          proposes next sprint's
                          scope
 approves / redirects --> ... cycle repeats
```

**Step-by-step:**

1. **Direction.** Luke tells the head what matters this sprint. If no
   strong opinion, the head proposes a scope from `BACKLOG.md` and the last
   playtest notes, and Luke approves or redirects.

2. **Sprint plan.** The head writes `SPRINT_PLAN.md`: scope, the brief(s)
   for whichever agents get dispatched, and an explicit **stopping point**
   — a testable definition of done, describing something Luke can actually
   open and play, even if rough.

3. **Dispatch.** The head sends each subagent a self-contained brief:
   what's changed since any relevant prior work, exactly which files to
   touch, what `INTERFACE.md` currently guarantees, and what "done" looks
   like for that piece specifically.

4. **Independent work.** Dispatched agents work against the shared docs.
   If genuinely parallel work risks touching the same files, use a git
   worktree per agent (Section 5.2) — not required by default now that
   dispatch is sequential in practice for a project this size.

5. **Integration.** The head merges any agent output, resolves conflicts,
   and runs the stopping-point checklist (Section 5.3) itself before it
   ever reaches Luke. A build that doesn't run is never handed off.

6. **Handoff.** The head gives Luke the build (how to run it), plus a
   one-paragraph `CHANGELOG.md` entry.

7. **Playtest.** Luke plays it. Notes go in `PLAYTEST_LOG.md`, or straight
   to the head in conversation.

8. **Triage.** The head sorts feedback: code fix, design change, art/UI
   tweak, or "noted, not now" into `BACKLOG.md`. Proposes next sprint's
   scope from this.

9. **Retro synthesis.** The head (or a dispatched Feedback agent, for a
   larger sprint) reads what's been appended to `TEAM_RETRO.md`, updates
   `AGENT_HANDBOOK.md`, and flags anything serious enough to warrant a
   change to this file. Doesn't block the next sprint. Cycle repeats from
   Step 1.

---

## 5. Git model

### 5.1 Branch layout

- `main` — always the last **known-good, playable** build.
- `sprint/<n>` — optional integration branch for a sprint, if the head
  wants a checkpoint before merging to `main`. For small sprints, working
  directly and committing to `main` incrementally is also fine — use
  judgment; the old model's mandatory per-role branching is no longer
  required.

### 5.2 Worktrees (optional, use when it actually helps)

If two dispatched agents need to touch the Unity project at the same time
without stepping on each other, give each its own worktree:

```
git worktree add ../ittd-<purpose> <branch>
```

Note the cost: Unity's `Library/` cache is gitignored and gets rebuilt per
checkout, so a fresh worktree means a slow first import. Prefer running
agents sequentially unless there's a real reason not to.

### 5.3 Stopping-point checklist (the head runs this before handing off)

- [ ] Builds cleanly from the current branch.
- [ ] The game launches and reaches a playable state without crashing.
- [ ] Every item listed in `SPRINT_PLAN.md`'s stopping point is present,
      even if rough.
- [ ] `ENGINE_STATUS.md` and `UI_STYLE_GUIDE.md` are up to date, not stale.
- [ ] `INTERFACE.md` reflects reality.
- [ ] `CHANGELOG.md` has this sprint's entry.
- [ ] Anything a dispatched agent found notable has a line in `TEAM_RETRO.md`.

If any box is unchecked, the sprint isn't done — fix the gap before
involving Luke.

---

## 6. Team memory: retro and handbook

### 6.1 `TEAM_RETRO.md` — the raw log

Appended to whenever something was harder than it should have been, or
easier because of a tip left behind. A few lines per sprint, dated,
unpolished:

```
### Sprint 7 — Code agent
- The brief didn't specify tile-grid coordinate origin (top-left vs
  center); guessed top-left, cost ~20 min. State this explicitly in
  future briefs.
```

Never overwritten, only appended to.

### 6.2 `AGENT_HANDBOOK.md` — the distilled version

Where `TEAM_RETRO.md` is raw and chronological, this is curated and
evergreen: the handful of things that actually recur. Three sections:

- **Tips & tricks** — build quirks, environment gotchas, naming
  conventions worth knowing before touching anything.
- **Known points of difficulty** — recurring friction and the current
  standing fix.
- **Dispatch practices** — the current answer to "how should the head brief
  a Code/Design/Feedback agent, and what should it double-check before
  trusting the result back."

Every brief the head sends a dispatched agent should point it at this file
alongside `CLAUDE.md` and its own relevant status doc.

### 6.3 Keeping it current

Periodically (end of a sprint, or every few sprints once there's enough in
`TEAM_RETRO.md` to be worth reading), the head reads what's accumulated
there and updates `AGENT_HANDBOOK.md` — either directly, or by dispatching
a Feedback agent for a larger backlog of retro notes. If a pattern is
serious enough to change how the project operates, propose an edit to
*this file* rather than letting practice silently drift from what's
written here.

---

## 7. Practical tips

**7.1 Brief like an onboarding doc, every time.** A dispatched agent has no
memory of prior sprints. Restate current state at the top of every brief.

**7.2 Small stopping points beat big ones.** Frequent, cheap feedback loops
beat a long gap between playtests, where design debt quietly accumulates.

**7.3 Self-verification is not optional.** A dispatched agent should
confirm its own piece builds/runs/renders before reporting done. The head
re-verifies at integration. Nothing reaches Luke that hasn't been checked
twice.

**7.4 Route cross-cutting concerns through the head.** Dispatched agents
generally can't talk to each other directly (fresh agents, no shared
context) — the head is the one place keeping the full picture, so design
questions that touch both engine and UI get resolved there.

**7.5 Luke's feedback doesn't need structure.** "The frost tower feels
useless past wave 10" is a complete, useful playtest note. Translating that
into a design/code/art task is the head's job, not his.

**7.6 Re-anchor on this file whenever something feels off.** If a sprint
goes sideways, the fix is usually "re-read `WORKFLOW.md` and the current
`SPRINT_PLAN.md` together," before assuming the process itself needs to
change.

**7.7 The handbook is a force multiplier only if kept current.** Skipping a
retro note "just this once" is how it goes stale — and its value compounds
the more sprints a fresh Code/Design agent can benefit from it.
