# Sprint Plan — Sprint 22: the invasion rework

## Recent sprints

- **Sprint 19** — peristalsis, velvet villi, Peyer's patch, crypts.
- **Sprint 20** — one palette, two looks; Classic later retired.
- **Sprint 21** — cartoon organs, light UI cards, shop tray, plate frame,
  knowledge HUD. Director: **"TRANSFORMATIVE."** Closed; the bone marrow
  second pass is queued in `BACKLOG.md`, not reopened.

## Direction (Director, 2026-10-14, extended 2026-10-18)

The orderly queue at the wall goes away. Pathogens should **mill around a
breach spot**, secretory **IgA** should trap them there, and that opens
**proximity-based ingress**, **spatial mucus sloughing** and **dead barrier
cells that pass bacteria but block viruses**.

On 2026-10-18 he added **in-lumen bacterial replication**, damped by
density and by IgA, and resolved the IgA/crowding tension himself:
secretory IgA is two-headed, so it **ties one pathogen to another**, and
the resulting bulk cannot reach the epithelium.

**`GAME_DESIGN.md` §1b is LOCKED**, which is why this sprint designed
before it built.

## What the design pass produced

- `docs/INVASION_REWORK_SPEC.md` (1,848 lines) + six mocks in
  `docs/art/invasion_*.png`. §12 holds the Director's decisions and
  **supersedes** the spec's own fork list.
- `docs/IGA_REFERENCE.md` (880 lines, 36 DOIs) — the immunology, checked
  against the literature rather than against memory. It **corrected two of
  the Director's five stated claims** and the head session's own
  recommendation on sloughing.

## Scope

1. **`GutWall`** replacing `GutInterface`: continuous `(s, d)` positions in
   the mucus band, milling, a triangular-kernel crowd field, and **plaques**
   that own members, charge visibly, and rupture.
2. **In-lumen replication** (bacteria only), damped by nutrient competition
   from crowding. **No IgA rate brake** — see §12.1 D1.
3. **Secretory IgA** at the 70% knowledge rung: two-stage *coated* →
   *cross-linked*, with daughters **born enchained**, so crowding above 70%
   flips from threat to sink.
4. **Barrier-cell death** as a **weak spot, not a hole** (§12.1 D2), with
   the virus asymmetry intact: no living cell means nothing to infect.
5. **Sentinel-goblet-cell sloughing** — automatic, local, triggered by the
   crowd field at a crypt, spreading to neighbours, expelling not killing
   (§12.1 D4).
6. **Player agency on the wall**: tower-targetable plaques, purchasable
   repair (§12.1 D3).
7. **`Sprint22Verification`** (12 checks, including an explicit anti-trickle
   regression), plus the deliberate revision of `MapVerification`.
8. **Docs**: rewrite §1b's steps 1–2, and `INTERFACE.md`.

## Not in scope

- A **player-aimed** purge: that is the type-2 "weep-and-sweep" response,
  which is adaptive and belongs on the knowledge ladder. `BACKLOG.md`.
- IgA blocking plasmid/resistance transfer; IgA1 protease vs. IgA2;
  commensal colonisation resistance — all real, all queued
  (`IGA_REFERENCE.md` §4).
- Bone marrow second pass; leukocyte rolling; the DC antigen dot.
- The `SimulationDriver` refactor and the headless strategy harness.

## Corrections this sprint is carrying

- **The board is 25 × 10, not 100 × 40** — 10 interface lanes, not 40.
  `INTERFACE.md` stated the wrong number as fact and `BoardConfig`'s own
  comments contradicted its defaults; both fixed 2026-10-18. This is
  *why* a lane is too coarse a unit for "a stretch of wall".
- **sIgA does not make pathogens easier to phagocytose** in the lumen; it
  is non-inflammatory and the lumen has no patrolling phagocytes. The gut's
  answer is **flush, not eat**, so the bonus moves tissue-side.
- **Crowding damps replication through nutrient competition**, not quorum
  sensing. Same number, correct reason.

## Stopping point

- [ ] All seven build items in, per spec + §12.
- [ ] 15 harnesses green (14 existing + `Sprint22Verification`), no warnings.
- [ ] New sprites checked magnified before a build is spent.
- [ ] WebGL build served for the Director.
- [ ] **The Director's eye**: does pressure still read, and does IgA feel
      like a reward rather than a wall of text?
