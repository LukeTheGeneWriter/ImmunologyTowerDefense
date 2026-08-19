# Handoff — Map 01: Small Intestine, Rounds 1–2

**For:** Claude Design
**Purpose:** Produce screen designs for the first playable slice. This document specifies what the game does; it does not specify how it should look, except where noted under *Art direction*.
**Status:** Round 1 was locked at the time of writing; **corrected 2026-08-19** against `docs/GAME_DESIGN.md` following that day's Director decisions (see the correction notes inline below and in `docs/BACKLOG.md`). Round 2 is directional. Mid/late game is context only — do not design for it yet.

---

## 1. Reference frame

The genre model is Bloons TD 6. Two of its conventions we keep, two we deliberately break. A designer familiar with BTD6 will otherwise mispredict what the screen needs.

**Kept**

- **Round-based economy.** Clearing pathogens pays out currency, which buys units and upgrades between rounds. Currency here is **ATP**.
- **Constrained upgrade paths.** BTD6 gives each tower three upgrade paths but only lets one reach the top tier while a second is capped low, which forces commitment and creates distinct builds. We use the same shape, because it maps exactly onto lineage commitment in T cells — a naive cell that commits to one fate forecloses the others. The constraint is biology, not an arbitrary balance rule, and the UI should say so.

**Broken**

- **Transiting is not a fail state.** In BTD6, a balloon reaching the exit costs a life. Here, an *E. coli* that **transits** the lumen and exits is excreted — no life cost. The threat is not the path — it is enemies that **leave** the path via the *other* end (**breach**, at depth 5). *(Correction, 2026-08-19: this document originally used "leak" for both events; see `docs/GAME_DESIGN.md` §6a for the transit/breach vocabulary now in use everywhere. Transit is still free of life cost, but as of the same decisions it is no longer entirely consequence-free — see the barrier colonisation note under §2 below.)*
- **Units are not static turrets.** Immune cells move. In round 1 they move badly, on purpose. See §4.

---

## 2. Spatial model

The map is a length of small intestine, viewed as a cutaway. Two axes carry different meaning and the design must keep them visually distinct.

**Horizontal — the lumen.** A flow channel running left to right. Pathogens enter left and are carried right by peristalsis. Exiting right = excreted = no penalty.

**Vertical — tissue depth.** Five discrete layers beneath the lumen. Depth is the real threat axis.

| Depth | Layer | Meaning |
|---|---|---|
| 0 | Lumen | Transit only. No damage possible. |
| 1 | Mucus / glycocalyx | Adhesion happens here. Recoverable. |
| 2 | Epithelium | Damage begins accruing. |
| 3 | Lamina propria | Where immune cells actually live and fight. |
| 4 | Submucosa | Heavy damage. Rare in round 1. |
| 5 | Bloodstream | **Systemic breach — costs a life (100-life pool, not an instant loss). See correction below.** |

Villi give the lumen floor its silhouette and create the adhesion sites. They are functional geometry, not decoration: a pathogen adheres *to a villus* and burrows from there.

> **Correction, 2026-08-19:** this depth table is still accurate but is now
> only one of **four compartments** on the full board (bone marrow, lymph
> node, blood, tissue) — see `docs/GAME_DESIGN.md` §1. Depth 5 (blood)
> carries a second meaning beyond "systemic breach": it is also the
> player's base, since progenitor towers placed in the bone marrow emit
> cells that enter tissue *from* blood. Breach costs one life from a
> 100-life pool rather than ending the map outright — see `GAME_DESIGN.md`
> §6c. Depth 1 (mucus/glycocalyx) also now persists state round to round:
> pathogens that adhere there can remain between waves on a
> space-available basis (**barrier colonisation**, `GAME_DESIGN.md` §6b) —
> this is the mechanism that answers open question 2 below (what keeps the
> screen worth watching while the player waits).

---

## 3. Round 1 script

This is the tutorial round and should be designed beat by beat.

**Pre-round.** Player has **100 ATP** and an empty map. Two units are purchasable: **Macrophage (40)** and ~~Cytotoxic T cell (60)~~ **Neutrophil**. *(Correction, 2026-08-19: the neutrophil replaces the cytotoxic T cell as the second round-1 unit — see `docs/GAME_DESIGN.md` §4. Its cost is not yet set, so the "100 = exactly one affordable combination, opening is forced" claim no longer holds as written; a cheap neutrophil likely makes round 1 a genuine choice among several affordable combinations instead. Director decision needed on pricing and whether "forced opening" is still a design goal — tracked in `docs/BACKLOG.md`.)*

**Placement.** ~~Player places both units at depth 3. Placement is the one real decision: they choose *where along the intestinal segment* to station them, without yet knowing where adhesion will occur.~~ *(Correction, 2026-08-19: there is no placement action in tissue at all. The player places **progenitor towers in the bone marrow** — a separate compartment — and the cells they emit enter tissue from blood (depth 5) and find their own way in, via the same random-walk/cytokine-sensing progression that governs movement once inside. See `docs/GAME_DESIGN.md` §2a. Bone marrow real estate, not in-tissue placement, is the constraint. This means round 1 has no in-round player action beyond the initial buy — accepted as fine for a tutorial round; barrier colonisation gives the screen something to show in the meantime.)*

**Wave.** 10 *E. coli* enter the lumen. Roughly 6 transit and exit — visibly, harmlessly, and the player must be shown this is fine. Roughly 4 adhere to villi and begin burrowing to depth 1–2.

**Combat.** The two units perform a **random walk** through the tissue. They find pathogens by collision, not by sight. This is slow. It should feel slow. Round 1's job is to make the player want chemotaxis before we sell it to them.

**Damage.** Self tissue damage accrues wherever combat occurs and wherever pathogens sit. Some damage is unavoidable — there is no perfect clear. This must be legible as *the immune response causing it*, not only the pathogen.

**Round end.** Triggered by clearance, not by a timer. Payout in ATP. Tissue damage persists into the interval and heals at a fixed rate per round.

---

## 4. The search problem (core mechanic)

The random walk in round 1 is the game's central teaching device and the anchor of the entire upgrade economy. The progression is:

1. **Random walk** — no directional information. Round 1 default.
2. **Cytokine sensing** — pathogen sites emit a gradient; units bias toward it. First purchasable upgrade, and it should feel transformative.
3. **Directed chemotaxis** — units path efficiently to the nearest signal.
4. **Tissue residency** — units pre-position at high-risk sites and stop searching entirely.

Every step must be **visible in the movement of the units on screen**, not only in a stat readout. The player should be able to see the difference between a round 1 unit and an upgraded one from across the room. This is the single most important thing for the designer to solve.

*(Correction, 2026-08-19: unaffected in intent. The spatial implementation is now specified — a two-resolution lattice, coarse grid for occupancy, 7×7 fine sub-lattice for movement, four-neighbour movement only. See `docs/GAME_DESIGN.md` §7 for the full mechanics, including why board width rather than fine-lattice subdivision is the real difficulty knob.)*

---

## 5. Round 2

Player now has ATP to spend on one of two axes, and the choice should be presented as genuinely competitive:

- **New units** — Dendritic cell, Helper T cell. These unlock synergies: the dendritic cell samples antigen and licenses the helper, the helper amplifies macrophage and CTL output. Nothing about them is useful alone.
- **Capability upgrades** — cytokine sensing on existing units. Cheaper, immediate, no synergy.

The intended lesson is that coordination beats individual strength, but the player should be able to get it wrong and feel the cost.

---

## 6. Progression context (do not design yet)

Early game teaches innate function. Mid and late game shift to adaptive immunity: more T and B cell types, and **phenotype upgrades on specific clones** — committing a clone to central memory, or to IgA secretion. Pathogens gain health, resistances, and replication. Difficulty scales by demanding synergy rather than by demanding more units.

The relevant implication for the designer now: **the unit panel will eventually hold clones with individual identities and upgrade states, not just unit types.** Do not design a panel that only scales to eight generic buttons.

---

## 7. Screens required

1. **Combat screen.** The full map, both axes readable at once, unit positions, pathogen positions and depth, ATP, tissue damage, round counter, speed control.
2. **Pre-round buy panel.** Round 1 state — two units, 100 ATP, nothing else available.
3. **Unit inspector.** Selected unit, its upgrade paths with the tier constraint visible, and the lineage commitment made explicit.
4. **Round-end summary.** Pathogens cleared, transit vs. adhered counts, tissue damage taken, healing rate, ATP earned.

---

## 8. Art direction

- **Platform:** Steam, desktop first, 16:9. Assume mouse.
- **Palette:** derived from histology, not from sci-fi. Hematoxylin blue-violet for host structure, eosin pink for tissue and damage. Pathogens should read as foreign against that palette rather than as generically evil.
- **Register:** clinical and precise, not cartoon. The nearest visual reference is a well-made histology plate, not a mobile game.
- **Non-negotiable:** depth layers must be legible at a glance. If the player cannot instantly see how deep a pathogen has burrowed, the map has failed.
- **Avoid:** hazard-orange, biohazard iconography, glowing green.

---

## 9. Open questions for the designer

1. How does the lumen show flow direction without animation becoming noise at high speed multipliers?
2. ~~Round 1 combat is slow by design. What keeps the screen worth watching while the player waits?~~ *(Substantially answered, 2026-08-19: barrier colonisation gives depth 1 persistent, ongoing state between waves — see the correction under §2 above and `docs/GAME_DESIGN.md` §6b.)*
3. Tissue damage and pathogen depth are both spatial and both accumulate on the same map. Can they share a visual language without becoming unreadable together?
