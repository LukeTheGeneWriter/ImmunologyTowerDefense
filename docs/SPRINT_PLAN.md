# Sprint Plan — Sprint 23: the adaptive and unconventional roster

## Recent sprints

- **Sprint 20** — one palette, two looks; Classic later retired.
- **Sprint 21** — cartoon organs, light UI cards, shop tray, knowledge HUD.
  Director: **"TRANSFORMATIVE."**
- **Sprint 22** — the invasion rework: milling, plaques, secretory IgA,
  in-lumen replication, barrier death, sentinel sloughing. Director played
  it; the blister reads, but **he never perceived IgA**.

## Direction (Director, 2026-10-25)

Open with research, then build. The Director asked for parallel research
agents on proliferation, phagocytosis rates and relative cell speeds
before adding cytotoxic T cells, B cells and NK cells, on the reasoning
that **getting the cells right first lets rounds be designed against a
stable roster**, rather than discovering a game-breaking unit later.

Six research passes ran against PubMed and Consensus
(`docs/research/`, distilled into `docs/CELL_KINETICS_REFERENCE.md`).
Four Director decisions came out of them:

1. **All four cells land this sprint** — CTL, B cell, NK, γδ T. (The head
   session flagged that four thins each mechanic; the Director chose four
   anyway, and the scope below reflects that.)
2. **A CTL is a buyable tower licensed by knowledge** — bought whenever,
   dark until ~10%.
3. **B cells field plasma cells that emit visible antibodies**, with
   tertiary lymphoid organs as a later upgrade.
4. **NK held for the research**, then settled: an IFN-γ macrophage
   amplifier, ungated, named "NK cell (ILC1)".
5. **Rebalance the inverted recognition numbers, and add the cooperation
   bonus and a visible stress meter.**
6. **The MHC-I stealth pathogen is designed, not built** — it goes with
   round design.

## What the research changed

The research did not merely supply numbers; it changed two towers and
found one live defect.

- **NK was the wrong shape.** Intestinal NK/ILC1s are less cytotoxic and
  higher IFN-γ than blood NK; the clearest in vivo result is NK cutting
  *intramacrophage* Salmonella. Built as an amplifier, not a killer.
- **γδ IELs are not hunters.** Interception and barrier repair, with a
  63% invasion reduction behind it.
- **Our innate recognition was ~800× too generous**, and inverted against
  a real CTL. §4b said innate immunity is bad at intracellular infection;
  the number said the opposite.

## Scope — all delivered

1. **Four `UnitKind`s**, each with its own profile, price, sprite, speed
   and emission cadence.
2. **`QuietKill`** — the CTL's real mechanic: the host cell survives.
3. **Recognition as a cumulative, visible, cooperative meter**, replacing
   the hidden per-tick roll; superadditive at 3+ attackers (Halle 2016).
4. **Innate recognition rebalanced** 0.03/0.02 → 0.003/0.002; cytotoxic at
   0.03, a ~10× gap.
5. **IFN-γ aura** — the NK/ILC1 tower's primary output.
6. **γδ wall patrol and repair**, reusing Sprint 22's wall API.
7. **Plasma cells and drifting antibody objects**, pooled, reusing the IgA
   coating state.
8. **Knowledge licensing** — the first mechanic that reads the ledger and
   changes behaviour rather than the HUD.
9. **`Sprint23Verification`** — 50 checks, seeded.
10. **Docs**: `CELL_KINETICS_REFERENCE.md`, `docs/research/`, §4c.

## Two seams that had to be fixed first

- **`ProfileFor`/`PoolFor` were binary ternaries.** A third tissue kind was
  not a compile error — it was a silent wrong answer handing a CTL
  neutrophil stats and a neutrophil sprite. Now a dictionary, with a
  harness check.
- **`IsAdaptive` conflated two properties.** It meant both "node-resident,
  no tissue profile" and "needs the adaptive subsystem". A CTL is adaptive
  *and* tissue-roaming — the first cell that is both. Split into
  `IsNodeResident` and `IsKnowledgeGated`.

## Deviations, all deliberate and noted in code

- **The dendritic cell stayed at speed 3** though the research says 2.
  Sprint 14 raised it so its patrol oscillation reads inside a round, and
  a legibility decision beats a ratio this close to the literature's own
  noise floor (three labs disagree by 3× on the same measurement).
- **γδ T is speed 1, against its real ratio.** It is tissue-*resident*;
  search speed is beside the point and a fast number would read as another
  hunter.
- **Antibody secretion is ~1/s, not ~1,000/s.** The real rate is
  unusable — what survives compression is the *shape*: a factory running
  continuously rather than a fighter landing blows.
- **Recognition became a meter rather than staying a roll.** Expected
  time-to-clear is identical; the change buys legibility and makes
  cooperation expressible.

## A defect found on the way

**The Sprint 22 secretory-IgA rung was never flipped to `Built`.** For a
whole sprint — including the playtest where the Director reported never
perceiving IgA — the HUD labelled a live mechanic "planned". Fixed, and
`Sprint21Verification.RunHonesty` now pins the built set in both
directions so a rung can be neither flipped early nor forgotten.

This does not by itself explain the IgA miss (the mechanic is driven by
`IgaTuning.KnowledgePercent`, not by the flag), but it means the one piece
of UI that would have told him IgA had arrived was saying the opposite.
It belongs alongside `PLAYTEST_LOG.md`'s three candidate causes.

## Stopping point

- [x] All ten scope items in.
- [x] **16 harnesses green, 603 checks**, no warnings. `Sprint23Verification`
      is seeded.
- [x] New sprites checked magnified before a build was spent — **two were
      wrong**: the granules were invisible (white-on-white in an
      alpha-carried silhouette) and the γδ cell read as a **face**, which
      the Director ruled out in Sprint 20. Both fixed before building.
- [ ] WebGL build served for the Director.
- [ ] The Director's eye.

## What to watch for in the playtest

1. **Does the quiet kill read?** The thing to notice is not that
   infections clear — it is that *the tissue survives being cleared*. If
   that does not land, the whole innate/adaptive investment arc is
   invisible.
2. **Does the recognition meter read mid-round** without being studied?
   Same bar the blister had to clear in Sprint 22.
3. **Is the NK cell's buff legible**, or does it feel like nothing? An
   amplifier is inherently harder to perceive than a killer — this is the
   biggest legibility risk in the sprint.
4. **Is innate-only play now too punishing** against intracellular
   infection? The rebalance is tenfold and deliberately harsh.
5. **Are four new towers too many to read at once** in the shop tray?
