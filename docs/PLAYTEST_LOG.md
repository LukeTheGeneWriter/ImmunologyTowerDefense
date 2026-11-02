# Playtest Log

Your raw notes after each playtest, dated, in your own words. No structure
required — the Producer translates these into triaged tasks.

<!-- Example entry format:

## Sprint 0 — 2026-08-25
Empty window opened fine on both desktop and the web build. Web version
took a few seconds to load, felt a bit long for a "quick demo" first
impression.

-->

## Sprint 16 — 2026-09-04 (Director)
Buy panels and upgrades are a great framework.

Two things for next:
- Dendritic cell motion is **super jittery**.
- Cartoonify the blood vessel so it's visually appealing, and the lumen
  too — more velvety, with villi, instead of just being poo-inspired.
  That would also help it contrast with the contaminated food sources
  flowing by (those can stay poo-ey).

## Sprint 17 — 2026-10-13 (Director, in conversation)
DCs are smooth now. Start Sprint 18 on the upgrades. Get design
inspiration from scientific-cartoon diagrams of the lumen's
microstructure. The bolus is good for now; fix its motion next. Maybe
introduce new cells flowing in from the blood, rolling along until they
pop through the vessel into the tissue, like they would physiologically.

## Sprint 18 — 2026-10-13 (Director, in conversation)
The bolus should move via gut peristalsis, be opaque and not so spiky.
Great info from the designer. Implement the top lumen ideas. Yes on a
Peyer's patch, now and refined later, because it's important
immunologically. M-cell sampling should be an upgrade, not out of the box.

## Sprint 19 — 2026-10-13 (Director)
"Nice job!" Then: have the designer take another pass, comparing against
other TD games and cross-section diagrams, for something accurate but
simple, fun and bright, with mass appeal.

## Sprint 20 — 2026-10-14 (Director)
"That looks great. Excellent call on no faces." (Daylight chosen.) Next
cosmetic items: bone marrow and lymph nodes, the shop, the borders of the
game board, and a pathogen knowledge HUD.

## Sprint 21 — 2026-10-14 (Director)
"This was TRANSFORMATIVE. The design agent did such a great job."
- Particularly likes the **pathogen knowledge tokens flying**.
- **The node looks great.** The **bone may need a second pass**, though
  it is technically correct.
- **Fix the placeholder mechanism of pathogens lining up at a slot to
  burst through the lumen wall.** Make it more realistic: they float
  around an area close to a wall breach spot (and possibly get stuck by
  IgA), not lined up in an orderly fashion. That may open the door to:
  - **proximity-based ingress** instead of lane-based ingress;
  - **mucus sloughing**;
  - **barrier-cell-is-dead mechanics**: a dead barrier cell lets bacteria
    invade but still acts as a viral barrier.
- Next sprints should focus on **adding pathogens, planning rounds, and
  setting up agent playtests to find "broken strategies"**, so we can tell
  which are true biological strategies and which need work.

## Sprint 22 — 2026-10-19 (Director)
The invasion rework. In his words:

> "I can tell the blister is there now. I don't think I perceived IgA,
> it's overshadowed by the relative strength of the macrophages. I think
> the next step will be for me to suggest new cell types and upgrades.
> 3-5 are all good."

Against `SPRINT_PLAN.md`'s five watch-items:

1. **Does the blister read?** — **Yes.** The charge model's whole case was
   that what is drawn should be the actual state, and it landed.
2. **Does earning IgA feel like a reward?** — **No: he never perceived it
   at all.** This is the sprint's one miss, and it is NOT the same
   complaint as "IgA is too weak." See below.
3. **Is the mill alive or noise?** — good.
4. **Is the wall too safe once IgA is up?** — good (no complaint).
5. **Does round 1 feel different with replication?** — good.

### The IgA miss — diagnose before tuning

"Overshadowed by the relative strength of the macrophages" has at least
three distinct causes and they want opposite fixes. **Check them in this
order before touching `IgaTuning`:**

1. **Did knowledge ever reach 70% for any species in that session?** If
   not, IgA never switched on and there is nothing to perceive. The rung
   is `KnowledgeCapability.SecretoryIgA` at 70%; the HUD pip going solid
   is the tell. This is the cheapest check and the most likely answer.
2. **If it did fire, was there ever a crowd at the wall to clear?** IgA
   acts in the lumen. If macrophages clear the tissue so efficiently that
   wall pressure never builds, the player never sees the thing IgA
   prevents. A defence against a problem you never have is invisible by
   construction — that is a ROUND-DESIGN problem, not an IgA problem.
3. **Only if 1 and 2 are fine** is this a strength question, and the lever
   is `IgaTuning.CoatChancePerSecond`.

Note (2) is the interesting possibility, because it says the wall rework
is working and the rounds are not yet asking anything of it.

### Direction for next session (Director)

He will **suggest new cell types and upgrades**, and wants the current
upgrade behaviour reviewed alongside. Open the next session by asking him
about both.

## Sprint 34 — 2026-11-03 (Director)

**The first playtest in three sprints.** Sprints 32, 33 and 34 had all been
handed off harness-measured but unplayed, which was flagged each time; this
is the session that closed that gap, and it immediately found something no
harness had.

### What the Director reported, verbatim

> "I gave it a play test, and did not see any messages for the rounds or
> showcases of a new pathogen and what it does. I didn't ask for that last
> part."

Two separate items, and he separated them himself.

### 1. The round messages do not reach the player — A BUG

**Sprint 33 shipped a buy-phase caption** that previews the next round's
tagline ("Raw oysters, *Vibrio cholerae*") and names any organism debuting
in that round ("NEW: *Vibrio cholerae*"). `Sprint33Verification` has 41
checks green on it, including the debut callouts. **The Director saw
neither.**

This is the Sprint 32/33 pattern again, one layer further out: the harness
asserts that `RoundScript.DebutLabelFor` and `ForRound().Tagline` return the
right strings, and **nothing asserts that the string reaches a pixel.**
A verified value written into a `Label` that the player never reads is the
same class of defect as a value never read at all — the eighth instance,
and the first one on the UI side of the boundary.

**Ruled out already:** the obvious cause, that `ApplyPhase` never fires on
the opening buy phase, is **wrong** — `builtForPhase` is initialised to
`(RoundPhase)(-1)`, so the first frame always triggers it. The cause is
further in and was deliberately not chased, because the Director asked for
this recorded rather than fixed. Candidates for whoever picks it up:
`caption`'s visibility/layout inside `topRight` (it is added to `Root` but
never passed to `UiTheme.Show`), whether its text is being overwritten by a
later `ApplyPhase` in the same frame, and whether the WebGL build's HUD
scale clips the line. **Do not assume it is one of these** — measure which
of the three it actually is before changing anything.

### 2. A pathogen showcase — NOT a bug, and not previously asked for

The Director noted this himself: *"I didn't ask for that last part."* The
three-line debut card (vehicle → name + sprite at large size → the one thing
that will not work) is **designed in
`docs/ROSTER_EXPANSION_SPEC.md` §4.2–4.4 and unbuilt**. It is the right
answer to "what does this new pathogen do", and it is also the surface most
of the ten proposed organisms depend on, since eight of them have a special
power the player cannot infer from a silhouette.

**Both are next-sprint material.** Fix (1) first: a showcase built on top of
a caption that does not display would be invisible too.

### What was NOT learned, and is still open

The three playtest questions carried since Sprint 32 remain unanswered,
because the session was spent on the messaging gap:

- Does the 40-round curriculum **read** as teaching?
- Is a one-burst round 40 exciting, or a wall of sprites?
- Is IgA perceptible now that wall pressure climbs? (The Sprint 22 miss was
  diagnosed as round design, and the 40-round script is the fix being
  tested.)
