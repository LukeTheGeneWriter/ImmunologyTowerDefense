# Sprint Plan — Sprint 8

## Sprint 7 — closed 2026-08-28

Delivered the ATP economy framework and the round loop (`GAME_DESIGN.md`
§5b/§5d/§6c): `AtpWallet`, `EconomyTuning` (every number a placeholder),
`RoundController` (`Building → Active → Defeat`, wave batch + buy phase),
`PathogenSpawner` batch gating, placement cost, the 100-life pool with a
`0 → GAME OVER` lose condition, and a HUD economy bar. 47 new
`EconomyVerification` assertions, 306 total, clean Windows build. Handed to
the Director; nobody has *played* the loop — that's his playtest.

## Direction for Sprint 8 (Director, 2026-08-29)

Build the **dendritic-cell shuttle and the antigen barcode** — the
mechanism by which the adaptive system actually learns a pathogen
(`GAME_DESIGN.md` §5a + §5c, with §1c's "debris is the antigen source").
This is the Director's long-standing most-wanted item and "the sprint
after" in the Sprint 7 plan.

Framework pass, same standard as Sprints 6–7: **the loop is built and
legible, every number is a deliberate placeholder, no balance attempted.**

### Five decisions taken up front (Director, 2026-08-29)

1. **Scope: the shuttle + the 8-bit barcode.** DC picks up antigen from
   tissue debris → carries it to a now-real lymph node → collides with
   helper-T cells there → a **Hamming-distance barcode match** teaches the
   adaptive system (per-species knowledge %). **No threshold capabilities
   this sprint** — knowledge goes up and is shown on the HUD, but unlocks
   nothing yet (§5's ladder is next).
2. **DC and helper-T are bought bone-marrow progenitor towers**, sharing
   the existing **5 slots** with macrophage/neutrophil. Marrow real estate
   is the constraint (§1c/§2a) — one of each innate + one of each adaptive
   is 4 of 5 slots, on purpose.
3. **The lymph node becomes a real compartment** — a small bounded arena
   with its own coordinate space that DCs and helper-T cells move around
   in and collide in. It was a non-functional backdrop since Sprint 2.
4. **A second cytokine field** — the co-localisation signal of §5c step 4,
   distinct from the infection cytokine — is built for the node, and both
   DCs and helper-T cells bias toward it via the existing `Chemotaxis`
   code so meetings reliably happen instead of relying on two random walks
   intersecting.
5. **Barcode match rule: Hamming distance ≤ 2** (≥ 6 of 8 bits agree ≈
   14.5% per random pairing). `MatchMaxHammingDistance` is a mutable
   tuning field so it can move (exact-match = 0, looser = 3+).

Authority: `GAME_DESIGN.md` §5a (the DC shuttle loop), §5c (the 8-bit
barcode, pairing, turnover), §1c (debris is the antigen source; the
three-fate debris table), §5 (knowledge is a per-species %), §2 (emitted
cells die at round end, progenitor towers persist).

## Scope

### 1. `ImmunologyTD.Adaptive` — the pure-logic core

- **`Antigen`** (static) — the 8-bit barcode. `byte RandomTag()`,
  `int HammingDistance(byte, byte)` (`popcount(a ^ b)`),
  `bool IsMatch(byte, byte)` (distance ≤ `AdaptiveTuning.MatchMaxHammingDistance`),
  `byte TagForClass(PathogenClass)` (a fixed per-class antigen — see the
  species note below).
- **`KnowledgeLedger`** — plain reference type, per run. `float Get(PathogenClass)`
  (0–100), `void Add(PathogenClass, float)` (clamped), `void Reset()`,
  and a `Changed` hook for the HUD. This is §5's "percentage per pathogen
  species."
- **`AdaptiveTuning`** — mutable statics, `ResetToDefaults()`, same pattern
  as `InvasionTuning`/`EconomyTuning`. Every number placeholder:
  `MatchMaxHammingDistance` 2, `KnowledgePerMatch` 3, `KnowledgeMax` 100,
  `DcPresentationsPerCargo` 4, `DcDebrisSamplePerBite` 0.34 (≈3 bites to
  drain a pile — it *does* compete with efferocytosis, §1c), `PairingSeconds`
  1.5, `LymphocyteLifespanSeconds` 20, `NodeColocalisationSourceStrength`
  18, `NodeLymphocyteSourceStrength` 6, DC/helper-T fine-tiles-per-tick,
  emission intervals, `MaxActiveChildren`. Per-class antigens
  (`VirusAntigen` etc.) live here too.

**Species key = `PathogenClass` for now.** There is no pathogen-species
roster yet (a known `BACKLOG.md` item, also needed for §4b's
budding-vs-chain trait). The three `PathogenClass` values stand in as the
three "species," each with a fixed antigen barcode. When a real roster
lands, knowledge keys off species id and each species rolls its own
antigen. Flagged, not blocking.

### 2. Debris carries an antigen identity (`TissueGrid`)

A dead cell's debris has to remember *what killed it* for a DC to sample
it. The host layer gains a per-cell `PathogenClass? DebrisAntigen`;
`KillHostCell` gains an optional `PathogenClass? antigen` argument;
every call site passes the responsible class (intracellular resident's
class on a stress-sense / drain / burn-out death, the grazing large
bacterium's class, `null` for neutrophil collateral on bare tissue).
New read `PathogenClass? GetDebrisAntigen(CoarseCoord)`. `ClearDebris`
clears it with the pile. Existing harness call sites pass no antigen and
keep working (optional arg).

### 3. `LymphNode` — the second arena

Plain reference type (not a MonoBehaviour), constructed by `GameBootstrap`.

- Owns a small dedicated `BoardConfig` (via `ConfigureForTest` to ~a
  6×6 coarse / 42×42 fine box) and its own `CytokineField` — this is the
  reuse the Director asked for: node movement runs through the exact
  `Chemotaxis.ChooseNextStep` path, and the co-localisation field is a
  real `CytokineField` recomputed each node tick from a **static central
  source** plus **each resident lymphocyte as a weak source** (so DCs
  drift toward where the T cells actually are). The node is small enough
  that `strength / (1 + distance)` is steep across it — the flat-at-scale
  problem is a large-map problem.
- Holds `List<Lymphocyte>` residents and `List<DendriticCell>` visitors.
- `Tick(float dt, float now)` — recompute the field, step every resident
  and visitor, run pairing, age out residents past
  `LymphocyteLifespanSeconds`.
- `void Admit(DendriticCell)` / `void Release(DendriticCell)` — a DC
  entering / leaving on its shuttle.
- `FineCoord RandomInteriorFine()` — spawn helper.
- World-space transform helpers so the renderer can draw node agents
  inside the node backdrop rect.

### 4. `Lymphocyte` — the helper-T cell (MonoBehaviour agent)

- `Initialize(LymphNode, byte tag, FineCoord start, float now)`.
- Born with a **random 8-bit `Tag`** (`Antigen.RandomTag()`), set by the
  progenitor at emission.
- Wanders the node via `Chemotaxis` against the co-localisation field
  (`cytokineEnabled: true`). Frozen while `Paired`.
- `float BornAt`; the node ages it out at `LymphocyteLifespanSeconds` →
  despawn to its pool. The progenitor keeps emitting new ones, so the
  barcode repertoire **turns over** within a round (§5c step 6) as well
  as at the round boundary.

### 5. `DendriticCell` — the shuttle (MonoBehaviour agent)

State machine, explicit-time `Tick`/`SimulationTick` like every other
agent:

- **`PatrolTissue`** — emitted at the tissue base edge on a random lane
  (same entry as innate units). Plain random walk (no debris homing —
  that's a deferred `BACKLOG.md` item). Standing on a `Dead` cell with
  debris and no cargo → **sample**: `Cargo = TissueGrid.GetDebrisAntigen`,
  `HasCargo = true`, take one `DcDebrisSamplePerBite` bite of the pile
  (this is the efferocytosis competition of §1c — a DC that samples a
  pile is clearing it too). `presentationsLeft = DcPresentationsPerCargo`.
- **`TravelToNode`** — base-biased walk (axis frame, `dAxis = -1`) until
  it reaches the base band, then `lymphNode.Admit(this)` → `InNode`.
- **`InNode`** — wanders the node via `Chemotaxis` against the
  co-localisation field. On contact with an unpaired `Lymphocyte`: both
  enter a `PairingSeconds` freeze; on resolve, if
  `Antigen.IsMatch(Cargo, other.Tag)` → `knowledge.Add(species,
  KnowledgePerMatch)` and a green `KnowledgeFlash`; either way
  `presentationsLeft--`. At 0 → `HasCargo = false` → `ReturnToTissue`.
- **`ReturnToTissue`** — `lymphNode.Release(this)`, walk back out into
  tissue, → `PatrolTissue` (a spent DC **returns empty, it does not
  die** — travel time is the cost; §5a's "dies or returns empty" open
  question is resolved this way for now).

Round boundary despawns fielded DCs like any emitted cell (§2); knowledge
% persists on the `KnowledgeLedger`.

### 6. Bone-marrow integration

- `UnitKind` gains `DendriticCell` and `HelperT`.
- `EconomyTuning` gains `DendriticCellPrice` (placeholder 30) and
  `HelperTPrice` (placeholder 25); `PriceFor` covers them.
- `BoneMarrowManager` stays the slot / picker / placement-cost / cap /
  `ClearFieldedUnits` authority. The two adaptive kinds don't emit a
  `SearchUnit` — placement and per-interval emission for them delegate to
  an injected `IAdaptiveEmitter` (implemented by a new small
  `AdaptiveDirector` MonoBehaviour that owns the DC pool, the lymphocyte
  pool, and the `LymphNode`). The manager still tracks child counts for
  the `MaxActiveChildren` cap and despawns them on `ClearFieldedUnits`,
  via emitter callbacks.
- The IMGUI picker gets two more buttons (four total), each showing its
  price and greying out when unaffordable — same as Sprint 7.

### 7. HUD + rendering + bootstrap

- `HudOverlay` — a **KNOWLEDGE** readout (per class: `Virus 14% ·
  Bacterium 3% · LargeBac 0%`), plus node population (`DC n · helperT n`)
  and a one-line "DCs carry antigen from debris to the lymph node" hint.
- `DendriticCell` / `Lymphocyte` visuals — flat squares like everything
  else, distinct colours (DC dendritic-magenta, brighter when carrying
  cargo; helper-T teal; a paired agent tinted toward white). Tween
  between tiles like `SearchUnit`.
- `KnowledgeFlash` — reuse the pooled `DegranulationFlash` with a new
  green colour for a successful match.
- Lymph-node backdrop relabelled ("Lymph Node — antigen presentation";
  drop "not functional yet"), enlarged a little so agents are visible.
- `GameBootstrap` wires the `KnowledgeLedger`, the `LymphNode`, the
  `AdaptiveDirector`, the DC/lymphocyte pools, and the new HUD args, and
  resets `AdaptiveTuning`.

### 8. `AdaptiveVerification.cs`

New headless harness, same no-Play-Mode philosophy as the six before it.
Covers: `Antigen` popcount / Hamming / `IsMatch` boundary (distance 2
matches, 3 doesn't), `KnowledgeLedger` clamp at 0 and `KnowledgeMax`,
debris antigen set by `KillHostCell` and read back / cleared by
`ClearDebris`, a **full simulated shuttle** (seed a debris pile of a
known class, drive a DC through patrol → sample → travel → node → force a
matching lymphocyte → assert knowledge rose by exactly `KnowledgePerMatch`
and a non-matching one adds nothing), lymphocyte turnover (a resident
past `LymphocyteLifespanSeconds` is gone; the count refills), and the
round boundary despawning DCs/lymphocytes while the two progenitors stay
placed and re-emit. Deterministic: any test that watches barcodes forces
`RandomTag`/match via explicit tags, and calls `ResetToDefaults()` after.

### 9. Not in scope

- **Threshold capabilities** (§5's ladder — MHC-I precise kill,
  neutralisation, complement, IgA, specific sensing). Knowledge unlocks
  nothing yet.
- **B cells.** §5c is helper-T; B cells wait.
- **A real pathogen-species roster.** `PathogenClass` is the species key.
- **DC homing on debris / macrophage homing on debris** ("find-me"
  signalling) — already a deferred `BACKLOG.md` item; DC patrol is a
  plain walk.
- **Barcode length** — fixed at 8 by the Director; not a variable.
- **Passive lymphatic drainage** as a knowledge sink (§1c's third debris
  fate). Only the DC-shuttle fate is built; unsampled debris still just
  self-dissipates.
- **Mutation / knowledge erosion** (§5's discrete step-change discount).
- **The DC:T pairing consuming a helper-T "slot" / helper-T barcode
  banking toward memory** — pairing is a timed freeze only.
- Anything from Sprints 1–7 changing behaviour: the invasion loop, the
  firebreak, §4b intracellular models, the economy/round loop, pooling,
  population caps all keep working.

## Stopping point (definition of done)

`[~]` = code done + harness-verified, feel unconfirmed (the Director's
playtest). `[x]` = verified from command output.

- [ ] Open into the buy phase; the picker now offers **four** progenitors
      (Macrophage 40 · Neutrophil 15 · Dendritic 30 · HelperT 25), each
      priced and greyed-out when unaffordable, all drawing from the same
      5 slots.
- [ ] A placed **helper-T progenitor** populates the lymph node with teal
      lymphocytes that wander and turn over on a lifespan.
- [ ] A placed **dendritic-cell progenitor** puts DCs in tissue that walk
      to a `Dead` cell, pick up antigen (visibly change colour), carry it
      left into the lymph node, mill among the lymphocytes, and pair with
      them.
- [ ] A **barcode match** (Hamming ≤ 2) on a pairing raises that species'
      **KNOWLEDGE %** on the HUD with a green flash; a mismatch does not.
- [ ] Cargo is spent after `DcPresentationsPerCargo` pairings and the DC
      **walks back to tissue** for more.
- [ ] The round boundary despawns fielded DCs and lymphocytes; the two
      progenitor towers stay placed and refill next round; knowledge %
      persists across rounds.
- [ ] Everything from Sprints 1–7 still works — Economy 47 / Combat 36 /
      Lifecycle 79 / Map 71 / Tissue 73 re-run green.
- [ ] `AdaptiveVerification` — all green.
- [ ] `GAME_DESIGN.md` §5a/§5c status, `INTERFACE.md`, `ENGINE_STATUS.md`,
      `CHANGELOG.md`, `BACKLOG.md`, `TEAM_RETRO.md` updated. Clean Windows
      build, 0 exceptions on launch.

The question this sprint answers: **is the shuttle — sample, carry,
present, match, learn — there and legible on screen**, even with every
number wrong, and does the lymph node read as a second place where a
search problem is playing out?

## Process note

Same as Sprints 6–7: head session, inline, commit after each scope item
with a reasoning-heavy message, update `INTERFACE.md` and `TEAM_RETRO.md`
as signatures change and judgment calls are made — not in a final sweep
(the retro's repeated lesson).
