# Immunology Tower Defense — Multi-Session Agent Workflow

This document is the constitution for how this project runs. It exists because
the five participants in this project — you, and four Claude sessions — do
not share memory with each other. The only things that persist between
sessions and between days are **files in the repo**. Everything in this
workflow exists to make sure the right information ends up in the right file
at the right time, so that a session that hasn't spoken to another session in
a week can still pick up exactly where things stand.

Keep this file at the repo root as `WORKFLOW.md`. If you ever change how the
project runs, edit this file first — it's the thing every session re-reads to
remember how to behave.

**Repository:** [github.com/LukeTheGeneWriter/ImmunologyTowerDefense](https://github.com/LukeTheGeneWriter/ImmunologyTowerDefense)
(public). This is the shared filesystem referenced throughout this document
— every session clones it, works on its own branch, and pushes back to it.
Being public is fine for an open-in-progress indie project, but keep an eye
on it: no API keys, tokens, or other secrets should ever land in a commit,
even a throwaway one, since public history is effectively permanent.

---

## 1. The five roles

**Director (you).** Sets direction, breaks ties on design disagreements,
plays the build once per sprint, and gives feedback from a real player's
perspective. You do not talk to Code or UI directly — you talk to the
Producer, in plain language, the way you'd talk to a human lead.

**Producer (this session, or its successor).** Your direct interface. Holds
no code and draws no art — its job is translation and coordination. It turns
your direction into specs, writes the brief for each sprint, dispatches work
to Code and UI, integrates what comes back, resolves conflicts between the
two, and packages a runnable build for you to test. It also owns triage:
when you give feedback, the Producer decides whether it's a code fix, a
design change, or an art tweak, and routes it accordingly.

**Code session.** Owns the engine and gameplay logic. Works only from the
brief the Producer gives it and the shared docs (Section 3). Self-verifies
before declaring anything done — see Section 5.

**UI/Art session.** Owns visual design, UI layout, and asset direction.
Same rule: works from its brief and the shared docs, self-verifies before
declaring done.

**HR Claude.** A fifth role, dedicated to team process rather than product.
Its job is to make each *new* Code or UI session faster to onboard and less
likely to repeat a predecessor's mistakes, by mining what actually happened
last sprint — where a session got confused, what a brief left out, how long
a cross-team question took to resolve — and turning that into durable tips,
a running list of known friction points, and a clear, current answer to how
the team should actually contact each other (Section 6). HR Claude doesn't
set scope or make design calls; it reports findings to the Producer, who
decides what to fold into the next brief or into this file.

Code and UI do not need to message each other directly. Route
cross-cutting concerns (e.g. "the tower's new stat needs a UI element")
through the Producer, at least at first — it's the one place keeping the
full picture, and it's cheap insurance against the two sides silently
drifting out of sync. If this ever feels like a bottleneck once the project
matures, allow narrow, logged exceptions (Section 8.4) rather than opening
the floodgates — and check `AGENT_HANDBOOK.md` (Section 6.2) first, since HR
Claude may have already spotted this exact pattern and proposed a standing
fix.

---

## 2. Why files, not chat, are the real communication channel

Every session — Producer, Code, UI — is capable of starting cold with zero
memory of anything said in a previous conversation. A session might still be
alive and reachable days later (in which case a direct message works fine
and includes full context), or it might have ended and need to be
re-created from scratch. The workflow has to work either way, which means:

- Anything that must survive should live in a **file that's committed to
  the repo**, not just in a message.
- Every brief should be written as if the recipient has never heard of this
  project before, with a pointer to the exact files that bring them up to
  speed. This costs a little more up front and saves you from silent
  regressions later.
- Git history is the project's long-term memory. Commit often, with
  messages that explain *why*, not just *what*.

---

## 3. Shared documents (the repo is the API between sessions)

All under `/docs` in the repo:

| File | Owner | Purpose |
|---|---|---|
| `GAME_DESIGN.md` | Producer (edits reflect Director decisions) | Current vision, mechanics, tower/enemy roster, balance philosophy. The single source of truth both Code and UI build against. |
| `ENGINE_STATUS.md` | Code session | Architecture overview, key modules, known issues, "state of the engine" as of the last commit. Rewritten at the end of every sprint, not just appended to. |
| `UI_STYLE_GUIDE.md` | UI session | Visual language, layout conventions, color/asset rules, component inventory. Same rule — kept current, not just appended to. |
| `INTERFACE.md` | Producer, with input from both | The contract between engine and UI: data shapes, event names, function signatures either side depends on. Changing something here is a cross-team event and should be flagged by the Producer to both sessions before it happens, not discovered after. |
| `SPRINT_PLAN.md` | Producer | This sprint's scope, the brief for Code, the brief for UI, and the explicit stopping point (Section 4). Overwritten each sprint; history lives in git log. |
| `PLAYTEST_LOG.md` | Director | Your raw, informal notes after each playtest. Dated entries, your own words, no need to structure it — the Producer does the translation. |
| `CHANGELOG.md` | Producer | One entry per sprint: what shipped, what's still rough, what's next. Appended to, never rewritten. |
| `BACKLOG.md` | Producer | Ideas and known issues not yet scheduled into a sprint. Where feedback goes when it's noted but not urgent. |
| `TEAM_RETRO.md` | Any session, jointly | Raw, dated log of friction and tips from each sprint, in each session's own words (Section 6.1). Appended to, never rewritten. |
| `AGENT_HANDBOOK.md` | HR Claude | Distilled tips, known points of difficulty, and the current contact protocol (Section 6.2). Kept current, not just appended to. |

Rule of thumb: if a fact needs to outlive the conversation it was created in,
it belongs in one of these files, not just in a chat message.

---

## 4. The sprint cycle

A sprint is the loop from one playtest to the next. Keep sprints short —
small enough that you're playing something new every few days, not once a
month. A stopping point should almost always be reachable in one sprint.

```
 Director                 Producer                  Code / UI sessions
 --------                 --------                  -------------------
 gives direction    -->   writes SPRINT_PLAN.md
                          (scope + stopping point)
                          dispatches briefs   -->    work independently,
                                                      each in its own
                                                      worktree (Section 5)
                          <-- report done / blocked
                          integrates, merges,
                          runs the stopping-point
                          checklist
                          packages a runnable build
 tests it           <--   hands off build + CHANGELOG entry
 writes PLAYTEST_LOG.md
 (or tells Producer
 directly)
                          triages feedback,
                          updates BACKLOG.md,
                          proposes next sprint's
                          scope
 approves / redirects --> ... cycle repeats
```

**Step-by-step:**

1. **Direction.** You tell the Producer what matters this sprint — a new
   feature, a balance pass, "make the UI less ugly," whatever. If you don't
   have a strong opinion, the Producer proposes a scope based on
   `BACKLOG.md` and your last playtest notes, and you approve or redirect it.

2. **Sprint plan.** The Producer writes `SPRINT_PLAN.md`: scope, the brief
   for Code, the brief for UI, and — critically — an explicit **stopping
   point**: a testable definition of done. Example: *"New frost tower is
   implemented and placeable; UI shows its range and cost; game builds and
   runs; balance is a rough guess, not final."* A stopping point should
   always describe something you can actually open and play, even if it's
   rough.

3. **Dispatch.** The Producer sends each session a self-contained brief
   (via a direct message if the session is still alive and reachable, or as
   a fresh session start otherwise) that includes: what changed since their
   last round, exactly which files to touch, what `INTERFACE.md` currently
   guarantees, and what "done" looks like for their piece specifically.

4. **Independent work.** Code and UI work in separate git worktrees
   (Section 5) so they can't step on each other's files. Each reports back
   to the Producer when done, or when blocked (e.g. "I need a new field in
   `INTERFACE.md` to do this") — and appends a couple of lines to
   `TEAM_RETRO.md` while it's fresh: what was confusing, what a future
   session should know (Section 6.1).

5. **Integration.** The Producer merges both worktrees into the sprint
   branch, resolves conflicts, and runs the stopping-point checklist
   (Section 5.3) itself before it ever reaches you. A build that doesn't
   run is never handed to the Director — that defeats the point of a
   stopping point.

6. **Handoff.** The Producer gives you the build (how to run it), plus a
   one-paragraph `CHANGELOG.md` entry: what's new, what's still rough,
   what to pay attention to.

7. **Playtest.** You play it. Notes go in `PLAYTEST_LOG.md` in your own
   words, or straight to the Producer in conversation — whichever is less
   friction for you. Rough, first-impression notes are exactly what's
   useful here; don't pre-filter them into "proper" bug reports.

8. **Triage.** The Producer reads your feedback and sorts it: code fix,
   design change, art/UI tweak, or "noted, not now" into `BACKLOG.md`. It
   proposes the next sprint's scope from this.

9. **Retro synthesis.** In parallel with (or right after) Triage, HR Claude
   reads what Code and UI appended to `TEAM_RETRO.md` this sprint, updates
   `AGENT_HANDBOOK.md`, and flags anything serious enough to warrant a
   change to this file (Section 6.3). This doesn't block your next sprint —
   it just makes the *next* Code or UI session start a little smarter than
   the last one. The cycle then repeats from Step 1.

---

## 5. Git model: worktrees, branches, and the stopping-point checklist

### 5.1 Branch layout

- `main` — always the last **known-good, playable** build. Only the
  Producer merges into `main`, and only at a sprint's integration step.
- `sprint/<n>` — the integration branch for the current sprint. Created
  from `main` at the start of the sprint; merged back into `main` (and
  deleted) once the stopping-point checklist passes.
- `sprint/<n>/engine` and `sprint/<n>/ui` — working branches for the Code
  and UI sessions respectively, each checked out into its own worktree.

### 5.2 Worktrees

Each session works in its own `git worktree` off the sprint branch, so Code
and UI never touch the same working directory:

```
git worktree add ../ittd-engine sprint/<n>/engine
git worktree add ../ittd-ui     sprint/<n>/ui
```

The Code session lives and works in `../ittd-engine`; the UI session lives
and works in `../ittd-ui`. Neither needs write access to the other's
worktree. The Producer works from the main checkout, pulls both branches,
and merges them into `sprint/<n>`.

This literal `git worktree` setup assumes all three sessions share one
filesystem — true if Code and UI are separate Claude Code sessions running
on the same machine, in the same local checkout. If instead any session
runs in its own isolated cloud sandbox (no shared disk — the likely case if
Code and UI are separate Cowork sessions rather than local Claude Code
sessions), replace local worktrees with the shared remote at
[github.com/LukeTheGeneWriter/ImmunologyTowerDefense](https://github.com/LukeTheGeneWriter/ImmunologyTowerDefense):
have each session clone it fresh and work on its own branch
(`sprint/<n>/engine`, `sprint/<n>/ui`), push when done, and let the Producer
pull both branches into `sprint/<n>` to merge. The branch layout and
stopping-point checklist below are identical either way — only the
transport between sessions changes.

### 5.3 Stopping-point checklist (Producer runs this before handing off)

- [ ] `sprint/<n>` builds cleanly from a fresh clone.
- [ ] The game launches and reaches the main menu / a playable level
      without crashing.
- [ ] Every item listed in `SPRINT_PLAN.md`'s stopping point is present,
      even if rough.
- [ ] `ENGINE_STATUS.md` and `UI_STYLE_GUIDE.md` are up to date, not stale.
- [ ] `INTERFACE.md` reflects reality — no engine/UI mismatch left
      unresolved.
- [ ] `CHANGELOG.md` has this sprint's entry.
- [ ] Code and UI have each appended a short note to `TEAM_RETRO.md`
      (even "nothing notable this sprint" is fine — the point is the habit).

If any box is unchecked, the sprint isn't done — the Producer sends
whichever session owns the gap a follow-up brief before involving you.

---

## 6. Team memory: retro, tips, and contact protocol

Docs like `ENGINE_STATUS.md` capture *what the engine is*. This section is
about capturing *what it's like to work on it* — the tips a departing
session would leave its replacement, and the friction patterns that only
become visible after they've happened more than once.

### 6.1 `TEAM_RETRO.md` — the raw log

Owned jointly: Code, UI, or the Producer appends to it whenever something
was harder than it should have been, or easier because of a tip someone
left behind. A few lines per sprint, dated, unpolished — a log, not a
report:

```
### Sprint 7 — Code
- The brief didn't specify tile-grid coordinate origin (top-left vs
  center); guessed top-left, cost ~20 min. Ask Producer to state this
  explicitly going forward.
- Tip for next Code session: scripts/build.sh needs --headless in this
  container or the build hangs waiting for a display.

### Sprint 7 — UI
- Waited on Code to confirm the tower stat schema before laying out the
  info panel. Would've been faster with a draft schema at sprint start,
  even a rough one that might still change.
```

Never overwritten, only appended to — the value is in the history, not just
the latest entry.

### 6.2 `AGENT_HANDBOOK.md` — the distilled version

Owned by HR Claude. Where `TEAM_RETRO.md` is raw and chronological,
`AGENT_HANDBOOK.md` is curated and evergreen: the handful of things that
actually recur, written once and kept current. Three sections:

- **Tips & tricks**, by role — build quirks, environment gotchas, naming
  conventions worth knowing before touching anything.
- **Known points of difficulty** — recurring friction and the current
  standing fix, e.g. *"engine/UI stat-schema handoffs have caused delays
  three sprints running; standing fix: Producer includes a draft schema in
  the sprint brief even before Code finalizes it."*
- **Contact protocol** — the actual, current answer to "who should talk to
  whom, and when." This starts as Section 8.4's default (route everything
  through the Producer) and evolves as HR Claude notices where that default
  is costing more than it's saving.

Every brief the Producer sends should point the recipient at this file
alongside their own status doc — it's part of onboarding a fresh session,
not optional reading.

### 6.3 What HR Claude actually does, and what it doesn't

At the end of a sprint — it doesn't need to be live *during* one, see
Section 7 — HR Claude:

1. Reads everything appended to `TEAM_RETRO.md` since it last looked.
2. Updates `AGENT_HANDBOOK.md`: adds new tips, retires stale ones, and
   promotes a friction point to "known" once it's shown up more than once.
3. If a pattern is serious enough to change how the team operates — not
   just a tip, but a standing process change — it proposes an edit to
   *this file* (`WORKFLOW.md`) to the Producer, rather than deciding
   unilaterally.

What it doesn't do: set sprint scope, resolve design disagreements, or
overrule the Producer. HR Claude improves the process; the Producer and
Director still own it.

---

## 7. Standing up and reaching the sessions

"Multiple sessions talking to each other" is concrete, not abstract — here's
what it actually takes:

- **Create them once, name them clearly.** Start the Code session (e.g. a
  Claude Code session opened in the engine checkout), the UI session, and
  HR Claude separately, and give each a name you'll recognize later — most
  tools that address a session let you set or infer a name from how/where
  it was started. Write those names into `SPRINT_PLAN.md` so the Producer
  (or a fresh Producer session, if this one has ended) knows who to contact.
- **Code runs where Unity runs.** Unity Hub, the Unity Editor, the Unity
  CLI, and an activated Personal license are all installed on the
  Director's machine ("lukesdecoder") as of 2026-08-18 — see `ENGINE_STATUS.md`. The Code session needs to
  operate against that machine, either as a local Claude Code session
  running there directly, or as a Cowork session reaching it through the
  Claude desktop app's device bridge. Record whichever is chosen in
  `ENGINE_STATUS.md` once decided.
- **HR Claude runs on a slower clock than the others.** It isn't part of
  the critical path from direction to playtest, so it doesn't need to be
  live during a sprint — the Producer can start or resume it once per
  sprint (or every few sprints, once `TEAM_RETRO.md` has enough in it to be
  worth reading) rather than keeping it running continuously.
- **The Producer reaches them by name.** From the Producer's side, listing
  reachable sessions surfaces who's currently alive, and messaging a
  session by name resumes it with full context if it's still running.
- **Reachability isn't guaranteed to persist.** A session only answers if
  it's still alive. If Code or UI has ended since the last sprint, the
  Producer can't resume it — instead, start a new session in the same
  branch/worktree and brief it cold, pointing it at `ENGINE_STATUS.md` (or
  `UI_STYLE_GUIDE.md`) to reconstruct state. This is exactly why Section 2's
  rule — anything durable lives in a file — matters: a brand-new Code
  session should be able to pick up the engine exactly where the last one
  left off, using only the docs and the git history, without ever having
  talked to its predecessor.
- **The Producer is the one constant.** Because you're addressing the
  Producer directly each sprint, it's fine if that session also eventually
  ends and gets recreated — you'll just re-anchor it on this file,
  `SPRINT_PLAN.md`, and `BACKLOG.md`, the same way Code and UI re-anchor on
  their own status docs.

## 8. Practical tips

**8.1 Brief like an onboarding doc, every time.** Even if a session is
"the same one" from last sprint, restate current state at the top of every
brief: what changed, what's expected now. Don't assume continuity you
haven't verified.

**8.2 Small stopping points beat big ones.** A sprint that produces
something playable in three days beats one that produces something
impressive in three weeks — you want frequent, cheap feedback loops, and a
long gap between playtests is where design debt quietly accumulates.

**8.3 Self-verification is not optional.** Both Code and UI sessions should
confirm their own piece builds/runs/renders before telling the Producer
they're done. The Producer re-verifies at integration. Nothing reaches you
that hasn't been checked twice.

**8.4 Direct Code↔UI contact is an exception, not the default.** Once the
project has enough history that `INTERFACE.md` is stable and both sessions
have a track record, you can allow them to message each other directly for
narrow technical questions — but have them log the outcome back to the
Producer (a line in the relevant status doc) so nothing gets decided
off-book. Check `AGENT_HANDBOOK.md` first — HR Claude may have already
turned a recurring version of this into a standing rule.

**8.5 Your feedback doesn't need structure.** "The frost tower feels
useless past wave 10" is a complete, useful playtest note. Translating that
into a design/code/art task is the Producer's job, not yours.

**8.6 Re-anchor on this file whenever something feels off.** If a sprint
goes sideways — wrong scope, missed handoff, stale docs — the fix is
usually "re-read `WORKFLOW.md` and the current `SPRINT_PLAN.md` together,"
before assuming the process itself needs to change.

**8.7 HR Claude is a force multiplier, not a fifth wheel.** It's tempting to
skip it as overhead on a small project, but its entire value is compounding:
the tenth Code session should be measurably easier to onboard than the
first, and that only happens if `TEAM_RETRO.md` and `AGENT_HANDBOOK.md` are
actually kept up. Skipping retro notes "just this once" is how the handbook
goes stale.
