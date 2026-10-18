# Invasion Rework Spec: the mill, the plaque, and the barrier

Status: **proposal, 2026-10-18.** Written before any code is touched,
because it changes a **LOCKED** section (`GAME_DESIGN.md` §1b, now marked
"under revision") on the Director's direction of 2026-10-14
(`PLAYTEST_LOG.md`, Sprint 21; framing in `SESSION_HANDOFF.md` §1).

If accepted it amends `GAME_DESIGN.md` §1b (steps 1 and 2 rewritten), §5
(the ~70% rung gains a mechanic), §6b (the mucus upgrade becomes spatial),
adds **in-lumen bacterial replication** to `GAME_DESIGN.md` §4a (new
scope, Director 2026-10-18), adds a barrier-cell state model that sits
**beside** §1c rather than inside it (§6 below flags that conflict
explicitly), and rewrites
`GutInterface`, `GutInterfaceRenderer` and the wall half of
`PathogenAgent`. Numbers are in **coarse cells** unless marked world or px;
colours are sRGB 0–1 and live in `Palette.cs`, never inline.

**Mocks** (committed, `docs/art/`; the PIL scripts are scratch and
regenerate with `python3 -I <script>`):

| File | Shows |
|---|---|
| `invasion_today_vs_proposed.png` | the same 13 pathogens, as today's orderly per-lane stacks vs. the proposed mill, with a crowd that straddles two lanes and a pair that is below the dose |
| `invasion_pressure.png` | five frames of one plaque: quiet → a crowd forms → pressure builds → the two-second tell → the rupture, with the charge meter under each |
| `invasion_iga.png` | a 70%-knowledge wall before and twelve seconds after: free crowd charging, then coated, cross-linked, held out and leaving — plus the dimer bridging two organisms at 2× |
| `invasion_replication.png` | the three replication regimes: loose and growing, crowded and stalled, coated and growing uselessly |
| `invasion_slough.png` | a slough wave peeling a colonised 2-cell patch off the wall |
| `invasion_barrier.png` | dead segments: bacteria squeezing through one at a time, a virion turned away, a virion inside a living cell four seconds from lysing it |

All six are drawn from the real `_spritedump/` sprites, tinted with the
real `Palette.cs` values, at the shipped board scale (1 coarse cell =
1.12 world = **73.8 px** at the 1920×1080 reference).

---

## 0. The facts everything hangs on

### 0.1 The board is 25 × 10, not 100 × 40

Every doc still quotes Map 01 as 100 × 40 with 40 lanes. **The shipped
board is 25 × 10.** `BoardConfig`'s C# defaults are `columns = 25`,
`rows = 10`, `baseBandCells = 6`, `lumenBandCells = 6`, the scene
(`Assets/Scenes/Sprint1.unity`) serialises only `columns: 25`, and commit
`0502919` ("Shrink Map 01 to 25x10, keeping the band proportions",
Director 2026-08-21) is where it happened. `MapVerification` still builds
**its own** 100 × 40 board through `ConfigureForTest`, which is why nobody
trips over it.

So the real numbers this design has to live in:

| Quantity | Value |
|---|---|
| Axis (threat) length | 25 cells — base 0–5, tissue 6–18, lumen 19–24 |
| Cross length (lanes) = **gut-interface positions today** | **10** |
| `TissueLumenEdgeAxisIndex` / `LumenNearWallAxisIndex` | 18 / 19 |
| Coarse cell | 1.12 world (7 fine tiles × 0.16) = **73.8 px** |
| Board | 28.0 × 11.2 world = 1846 × 738 px |
| **The wall** | 10 cells = **11.2 world = 738 px** long |
| Lumen channel depth | 6 cells = 6.72 world |
| Drawn mucus band depth | 1.6 cells (`LumenChannelRenderer.MucusDepthCells`) |

**This matters for the rework specifically.** Ten discrete positions is
why today's wall reads as a row of ten bars, and it is why "the lane a
pathogen adheres in" is such a coarse unit: one lane is 74 px of a 738 px
wall. A spatial model has to be **continuous along the wall** and quantise
only where it must; quantising to lanes again would change the drawing and
not the mechanic. Everything below is specified in *fractions of a coarse
cell*, so it is correct on a 10-lane board and on a 40-lane one.

### 0.2 The one property that must survive

`GAME_DESIGN.md` §1b step 2 and the long comment at the head of
`Grid/GutInterface.cs` say the same thing twice, and the comment is blunt
about the failure mode:

> do NOT refactor this into a per-pathogen independent invasion roll. It
> looks equivalent and it is not -- per-pathogen rolls produce a steady
> trickle across the whole wall, which destroys the "pressure visibly
> builds at one spot, then bursts" shape that is the entire point.

This spec takes that as a hard constraint and, where it adds a second
ingress route at all (§6's holes), it bounds that route to **one pathogen
at a time per hole, taking 2 s, visibly queueing** — so even the secondary
route reads as pressure at a spot rather than as a leak everywhere. The
primary route is *more* deterministic than today's, not less: pressure
becomes a visible **charge** that fills, rather than a hidden probability
that might fire at any moment.

### 0.3 What the wall can and cannot do today, and why it is about to matter

- **Adhered pathogens are unattackable.** `PathogenAgent.ReceiveDamage`
  returns immediately unless the state is `InTissue`; the wall is outside
  the tissue the immune cells walk. That stays true here.
- So the player's levers at the wall are, and will be, exactly three, and
  each comes from a different system: **mucus turnover** (innate, shop,
  §5), **secretory IgA** (adaptive, 70% knowledge, §4) and **crypts**
  (terrain, shop — §6 gives them a second job, healing the barrier).
  Before this spec, the count was zero: the wall was weather.
- **Nothing clears the wall between rounds.** The pile persists, which is
  §6b's intent ("a standing population accumulates there if the player
  doesn't manage it"), and `RoundController.ClearRound` only waits on
  `spawner.BatchComplete` (the bolus leaving), never on the field being
  empty. The mill therefore persists round to round for free.
- **The buy phase freezes the whole simulation** (`RoundClock.Frozen`), so
  the mill, charge, barrier healing and slough timers all stop with it.
  That is consistent with the existing "the buy phase freezes tissue
  healing too" note in `BACKLOG.md`, and it is deliberate here: a player
  cannot wait out a blister in the shop.

### 0.4 What a round actually puts on the wall

`EconomyTuning.BatchSizeForRound` is `16 + 6·(N−1)`, and
`PathogenSpawner.AdvanceFood` drops the batch in `FoodItemBurstCount = 4`
clumps at wall-hugging cells (`FoodItemWallHugDepth = 1`) near the bolus,
spread over a 30 s transit. At `AdhesionChanceAtWall = 0.30` most of a
clump sticks. So the design has to look right at:

| Round | Batch | Clump | Typical crowd at one spot |
|---|---|---|---|
| 1 | 16 | ~4 | 3–4 |
| 5 | 40 | ~10 | 7–10 |
| 10 | 70 | ~18 | 12–18 |

`maxLivePathogens = 150` is the hard ceiling, so the mill must be cheap at
~150 agents — see §2.3.

---

## 1. The model: the mill and the plaque

### 1.1 What replaces the per-position pile

Today: `GutInterface` holds `List<PathogenAgent>[CrossLength]` — one
unbounded pile per lane — and each pile rolls its own breach. An adhered
pathogen has an `InterfacePosition` (a lane index) and is drawn at
`InterfaceStackWorldPosition(position, stackIndex)`, i.e. pushed straight
out into the channel by its depth in the queue.

Proposed: **two objects, neither of them per-lane.**

1. **The mill.** Every adhered pathogen owns a **continuous 2-D position
   in the mucus band** — `(s, d)`: `s` along the wall, `d` out from it.
   It is a member of nothing; it just drifts, loops and crowds. `GutWall`
   (the renamed `GutInterface`) holds one flat list of them, plus a
   coarse **crowd field** computed from their `s` values.
2. **The plaque.** Where the crowd field has a local maximum above a
   threshold, the wall spawns a **plaque**: a pooled object with a centre
   `s_c`, a fixed footprint, an **exclusive membership** of the pathogens
   inside it, and a **charge** that fills while the membership is large.
   At full charge it **ruptures**: every member enters tissue in one call,
   and the barrier segments under the footprint die.

The plaque is the per-position pile's successor: the thing that
accumulates, the thing that is drawn as danger, and the thing that
releases everything at once. What changed is that it is **placed by the
pathogens** rather than by the map's lane grid, that it can sit anywhere
along the wall and straddle lane boundaries, and that its pressure is a
**visible quantity** rather than a hidden per-second probability.

### 1.2 Coordinates — the spatial unit, in the board's real frame

```
      TISSUE                      | wall |        LUMEN  (flow: s increasing)
                                  |      |
   axis 18 (TissueLumenEdge)       | seam |   axis 19 (LumenNearWall)  ... axis 24
                                  |      |
   d (cells out from the seam) ->  0    0.55            1.15        1.6
                                  |grip |             IgA hold    mucus edge
```

- **`s`** — distance **along** the wall, in coarse cells, measured from the
  flow's entry end in the flow direction:
  `s = (crossIndex − board.LumenEntryCrossIndex) · board.FlowCrossStep`
  plus the fraction within the cell. `s ∈ [0, CrossLength)`, so 0 ≤ s < 10
  on Map 01. Measuring it **from the entry end in flow units** (rather
  than in raw cross index) is what keeps it correct on a mirrored or
  vertically-banded board, exactly as `GutWallFeatures` already does for
  the Peyer's patch, and it means "downstream" is always `+s`.
  - lane back-conversion, for anything that needs a cell:
    `crossIndex = board.LumenEntryCrossIndex + board.FlowCrossStep · floor(s)`.
- **`d`** — depth **out from the seam** into the channel, in coarse cells.
  `d ∈ [0, MucusBandDepthCells = 1.6]`, which is deliberately the depth of
  the mucus band `LumenChannelRenderer` already draws, so "in the mill"
  and "in the drawn mucus" are the same region. Three bands inside it:
  - `d ≤ GripDepthCells = 0.55` — **gripping**. Counts toward crowding,
    can be a plaque member, can damage a barrier segment.
  - `0.55 < d ≤ 1.6` — **loose**. Visible, still on the wall, counts for
    nothing. Where IgA parks its captures (§4) and where a freshly
    sloughed pathogen passes through (§5).
  - `d > 1.6` — back in the flow (`PathogenState.Lumen`).
- **World conversion** (one helper on `BoardConfig`, so no renderer does
  the arithmetic):
  `WallToWorld(s, d) = InterfaceWorldCenter(lane) + alongAxis·(frac(s)−0.5)·cell + outward·d·cell`,
  where `outward` is derived from the two cells either side of the seam
  exactly as `PathogenAgent.InterfaceStackWorldPosition` already derives
  it today (no world direction is hardcoded).

**Why continuous and not a finer bucket grid.** A pathogen's drawn
position has to move smoothly for the mill to look alive (§2); a bucketed
position either snaps or needs a tween to hide the snap. The buckets below
exist only for the *crowd field*, which is a statistic, not a position.

### 1.3 The crowd field

One `float[]`, rebuilt every simulation tick from the gripping pathogens:

- **Buckets:** `CrowdBucketsPerCell = 4`, so 0.25 cell = 18.5 px per
  bucket, 40 buckets on Map 01 (`CrossLength · 4`). The field is 1-D: `d`
  is a gate (grip or not), not a dimension of the statistic.
- **Kernel:** triangular, radius `CrowdRadiusCells = 1.5` (≈111 px). A
  pathogen contributes `max(0, 1 − |s − s_bucket| / 1.5)` to each bucket in
  reach — 6 buckets either side. Cost is `O(agents × 13)`; at 150 agents
  that is ~2,000 float adds per tick.
- **Read as:** "how many gripping pathogens are within about a cell and a
  half of here, weighted by how close". A tight clump of 5 reads ≈ 4.0; 5
  spread over 3 cells reads ≈ 2.5. Crowding is therefore about
  *concentration*, which is the point.
- Triangular rather than box because a box kernel's maxima jitter between
  equal-valued buckets, which would make plaques flicker in and out; and
  because a smooth field gives the renderer a free "how bare is the mucus
  here" value (§7).

The field is **only** used to decide *where* things happen — where a
plaque seeds, where a slough wave is aimed (§5). It never drives pressure;
that comes from a flat membership count, so that the number driving the
charge is the same number the player can count on screen and the same set
the rupture releases.

### 1.4 The plaque — pressure as a charge, not a dice roll

```
struct Plaque {           // a fixed array of MaxPlaques, pooled; not allocated per event
    float CentreS;        // tracks the member centroid, lagged
    float Charge;         // 0..1, the visible pressure
    float Threshold;      // 1 ± ChargeThresholdJitter, drawn once at seed
    int   MemberCount;    // refreshed each tick
}
```

**Seeding.** Each tick, the highest bucket whose field value ≥
`PlaqueSeedCrowd = 2.5` and which is ≥ `PlaqueMinSeparationCells = 1.5`
from every live plaque centre seeds a new plaque there, up to
`MaxPlaques = 8`. One seed per tick is enough — crowds do not appear
instantly.

**Membership is exclusive.** Each gripping, uncoated pathogen is assigned
to the **nearest** plaque centre within `BurstFootprintCells = 1.5`. So two
plaques 1.5 cells apart split the pathogens between them instead of
double-counting, and `MemberCount` is exactly the set that a rupture will
release. `O(agents × plaques)` with ≤ 8 plaques.

**Charge.**

```
rate = (MemberCount − IngressCrowdFloor) · ChargeRatePerCrowdUnit      [per second]
Charge += rate · dt                     while MemberCount >  IngressCrowdFloor
Charge -= ChargeDecayPerSecond · dt      while MemberCount <= IngressCrowdFloor
```

with `IngressCrowdFloor = 2.0`, `ChargeRatePerCrowdUnit = 0.018 /s`,
`ChargeDecayPerSecond = 0.04 /s`. The centre tracks the membership
centroid at `PlaqueCentreLerpPerSecond = 2.0`, so a plaque follows a swarm
that is drifting downstream instead of being left behind by it.

**A plaque retires** when `MemberCount == 0` and `Charge ≤ 0`.

**Why a charge rather than a per-roll probability.** Three reasons, in
order of weight:

1. **It is the Director's requirement, made literal.** "Pressure builds
   visibly at a spot, then bursts" is exactly a quantity that fills and
   then triggers. Today the player sees a *proxy* for pressure (the bar's
   heat, normalised against a fixed count of 8 in `GutInterfaceRenderer`)
   and the actual event is memoryless: a position at 11%/s might go in one
   second or in thirty. A charge is honest — what is drawn *is* the state.
2. **It makes the player's interventions legible.** Coating a plaque's
   members with IgA (§4) or sloughing them off (§5) makes a drawn blister
   visibly deflate. Under a memoryless roll, "I stopped a breach" is
   invisible, because nothing was ever accumulating.
3. **It is headlessly assertable.** `charge(t)` is a deterministic
   function of membership, so the harness can prove "pressure rises with
   crowding" without a statistical test over hundreds of rolls (which is
   what `MapVerification` has to do today).

The cost is that a rupture becomes predictable to the second, which is
slightly less tense. `ChargeThresholdJitter = 0.15` (threshold drawn
uniformly from 0.85–1.15 when the plaque seeds) buys most of the tension
back while keeping the fill legible — the player knows it is nearly
due, not exactly when. Fork 1 puts the probabilistic alternative to the
Director anyway.

**The floor is a minimum infectious dose.** `IngressCrowdFloor = 2.0`
means a lone adherent *never* charges anything: it sits, mills, erodes a
little barrier if it is a large bacterium, and eventually creeps away
downstream. This is the single most important number in the spec, for two
reasons. Mechanically it is what forbids the trickle: ingress is not
something a pathogen does, it is something a *crowd* does. Biologically it
is the infectious-dose threshold, which is real and famously
species-specific (tens of organisms for *Shigella*, ~10⁵ for *Salmonella*)
— so it is also the knob that will later distinguish one species from
another. Fork 2.

### 1.5 The rupture — what is released, where

When `Charge ≥ Threshold`, in one call (`GutWall.Rupture(plaque, now)`):

1. **Every member enters tissue**, in the same tick, as today
   (`PathogenAgent.EnterTissueAt`). Members are ordered by their own `s`
   and each asks for the tissue cell behind its **own** position first, so
   a rupture that formed across three lanes lands across those three
   lanes. The existing outward search (`TryFindReleaseSlot`) is the
   fallback when a cell is taken; its `MaxReleaseCrossSpread` drops
   **20 → 4**, because the burst is already spread by geometry and no
   longer needs to fan a ten-deep single-lane pile across half the board.
   `MaxReleaseAxisDepth` stays 3. A member that finds no slot stays on the
   wall, as today.
2. **Every barrier segment inside the footprint dies** (§6): `s_c ± 1.5`
   is 3 cells of wall = 6 segments on Map 01. This is the hole the
   Director asked for, and it means the board **remembers** where it was
   breached: that stretch passes bacteria freely (one at a time) until it
   regrows, and blocks viruses while it is dead.
3. **The charge resets to 0** and the plaque survives if any member
   failed to find a slot, otherwise it retires. A spot that keeps being
   fed therefore ruptures repeatedly, with a dead stretch of wall in
   between — which is what chronic colonisation looks like.
4. **One `Ruptured(s_c, released, footprint)` event** fires, replacing
   `Breached(position, released)`. `InvasionTally.Breaches` and
   `ReleasedIntoTissue` keep their current meanings exactly (one breach
   per rupture), so `RoundVerification` and the HUD are unaffected.

### 1.6 Tuning — the full table, and what it does to pacing

New fields on `InvasionTuning` (mutable statics with `ResetToDefaults`,
per the project's pattern), replacing `PerPathogenBreachChance` and
`BreachRollIntervalSeconds`:

| Field | Proposed | Unit | Reasoning |
|---|---|---|---|
| `MucusBandDepthCells` | 1.6 | cells | equals the drawn mucus band, so the mill lives where the glaze is |
| `GripDepthCells` | 0.55 | cells | half a cell; a gripping pathogen visibly touches the wall |
| `CrowdRadiusCells` | 1.5 | cells | 111 px — a "stretch of wall" the eye reads as one spot, ~3 lanes |
| `CrowdBucketsPerCell` | 4 | — | 18.5 px resolution; finer is wasted at this board scale |
| `PlaqueSeedCrowd` | 2.5 | crowd | just above the floor, so a plaque appears slightly before it can charge |
| `PlaqueMinSeparationCells` | 1.5 | cells | = footprint, so plaques tile the wall instead of stacking |
| `MaxPlaques` | 8 | — | the 738 px wall holds ~6 non-overlapping footprints; 8 is slack |
| `IngressCrowdFloor` | 2.0 | members | the infectious dose (§1.4). Fork 2 |
| `ChargeRatePerCrowdUnit` | 0.018 | /s | chosen to reproduce today's pacing at a loaded spot — see below |
| `ChargeDecayPerSecond` | 0.04 | /s | an abandoned blister deflates in ~25 s: slow enough to read as relief |
| `ChargeThresholdJitter` | 0.15 | — | ±15% so "nearly due" is honest and "exactly when" is not |
| `PlaqueCentreLerpPerSecond` | 2.0 | /s | the plaque follows a drifting swarm within ~0.5 s |
| `BurstFootprintCells` | 1.5 | cells | release radius = crowd radius: what gathered is what goes in |
| `MaxReleaseCrossSpread` | 4 (was 20) | cells | the burst is pre-spread; 20 was compensating for a single-lane pile |

Time from "a crowd exists" to rupture, against today's equivalent:

| Members | `rate` | Proposed time to rupture | Today, same count in one lane |
|---|---|---|---|
| 1 | 0 | **never** | ~80 s |
| 2 | 0 | **never** | ~42 s |
| 3 | 0.018/s | 56 s | ~28 s |
| 4 | 0.036/s | 28 s | ~21 s |
| 5 | 0.054/s | 19 s | ~17 s |
| 8 | 0.108/s | 9.3 s | ~11 s |
| 10 | 0.144/s | 7.0 s | ~9 s |
| 14 | 0.216/s | 4.6 s | ~6 s |
| 18 | 0.288/s | 3.5 s | ~5 s |

Deliberately: **the top of the table is unchanged** (a loaded spot still
goes in 5–10 s, so round pacing does not move and the §0.4 round sizes
still work), while **the bottom of the table is gone** (one or two
pathogens no longer make a slow leak). Net effect on round 1, with clumps
of ~4: a rupture every ~28 s if the player ignores the wall, versus a
scatter of single-pathogen leaks today. The invasion gets *later and
louder*, which is the right direction for a tower defence.

`ChargeRatePerCrowdUnit` is literally today's `PerPathogenBreachChance`
(0.012) scaled by 1.5 to compensate for the floor subtracting 2 members.
It is a placeholder like everything else in `InvasionTuning`; the headless
strategy harness (`SESSION_HANDOFF.md` §2a) is what should eventually set
it.

---
## 2. Milling behaviour

### 2.1 The rule: milling is *drawn*, crowding is *simulated*

This is the architectural decision that makes the mill both cheap and
stable. Each adhered pathogen carries a **simulated base position**
`(sBase, dBase)` which changes only on the simulation tick, and a
**drawn offset** which is a closed-form function of time:

```
simulated (tick, ~0.12 s)          drawn (every frame, Update())
  sBase += creep + cohesion          s = sBase + Along · sin(ω t + φ)
  dBase -> settle toward grip        d = dBase + Depth · sin(2ω t + φ)
  crowd field uses sBase ONLY        transform.position = WallToWorld(s, d)
```

The crowd field, plaque membership and every rule in §1 read `sBase`. The
loop never feeds them. Two consequences worth the trouble:

- **Plaques do not flicker.** If the swirl fed the field, a pathogen
  orbiting a bucket boundary would shuffle crowd between buckets every
  frame and a marginal plaque would blink in and out.
- **It is the project's existing convention.** `AGENT_HANDBOOK.md`:
  "`Update()` only forwards the clock and drives the visual tween." The
  mill's swirl *is* a visual tween, and it is therefore not something the
  headless harness has to reproduce or assert.

### 2.2 The motion, and why it does not read as jitter

**Per-agent constants, drawn once when it joins the mill** (so there is no
per-tick RNG and no per-agent allocation — they are four floats on the
existing pooled `PathogenAgent`):

| Constant | Range | Note |
|---|---|---|
| `millPeriod` | 2.5 – 4.5 s | `ω = 2π / millPeriod` |
| `millAlong` | 0.10 – 0.22 cells | 7 – 16 px of travel along the wall |
| `millDepth` | 0.05 – 0.12 cells | 4 – 9 px of travel out from the wall |
| `millPhase` | 0 – 2π | desyncs the swarm |

The depth term runs at **2ω**, so the path is a **lazy figure-eight**, not
a circle and not noise. Three further ingredients, all shared:

1. **Downstream creep, modulated by peristalsis.**
   `sBase += MucusCreepCellsPerSecond · Peristalsis.SpeedFactor(roundElapsed) · dt`
   with `MucusCreepCellsPerSecond = 0.035`. The mucus layer is being
   sheared downstream by the gut; adherents go with it. Mean speed 0.035
   cells/s means a pathogen takes ~**285 s** to travel the whole wall and
   rejoin the flow at the downstream end — so the barrier population still
   accumulates across rounds as §6b requires, but it is not immortal.
   `Peristalsis.SpeedFactor` is already a pure static function used by the
   bolus *and* the drawn wall squeeze, so **the entire adhered population
   lurches downstream in time with the contraction the player can already
   see**. This is the single cheapest thing in the spec and it is what
   makes the mill read as "stuck in moving slime" rather than as
   independent agents twitching.
2. **Weak cohesion.** `sBase` moves toward the nearest plaque centre (or,
   with no plaque in reach, toward the highest crowd bucket within
   `CohesionReachCells = 2.0`) at `CohesionSpeedCellsPerSecond = 0.12`. A
   pathogen 1.5 cells from a forming clump joins it in ~12 s. Biologically
   this stands in for quorum-sensing and co-aggregation on the
   glycocalyx — colonisation genuinely is a social process. Mechanically
   it is what turns four separate bolus clumps into one dangerous spot
   over the course of a round, which is where the drama comes from.
3. **Settling.** On joining the mill a pathogen arrives at
   `dBase = 1.3` (the outer mucus) and settles toward
   `SettleDepthCells = 0.25` at `SettleSpeedCellsPerSecond = 0.5`, so
   there is a visible ~2 s "it is coming in to land" beat. A pathogen
   never *jumps* to the wall: the current `AdhereToInterface` snap is
   exactly the "orderly" artefact the Director objected to.

**No separation pass.** Overlapping agents in a knot are correct for a
mill, and a pairwise push would be the only `O(n²)` thing in the design.
The desynced phases, different loop radii and 0.6–0.9 sprite scales give
the knot its texture without it.

**Why this is not Brownian jitter**, stated plainly because it is the part
the Director will judge by eye: the motion is **low-frequency** (a 3 s
period, not a per-frame random walk), **smooth** (a sine, continuously
differentiable, so nothing twitches), **correlated across the swarm** (one
shared creep and one shared peristaltic rhythm, so the whole patch breathes
together) and **directional** (everything slowly goes downstream). Jitter
is high-frequency, zero-mean and uncorrelated — the opposite of all four.

### 2.3 Cost

Per agent per frame: 2 `sin`, one `Vector3` add, one `transform.position`
write. Per agent per tick: ~4 multiply-adds plus 13 float adds into the
crowd field. At the `maxLivePathogens = 150` ceiling that is ~300 `sin`
calls per frame and ~2,500 float ops per tick — **under a tenth of what
the 250-cell tissue grid already does** every 0.15 s. Zero allocation:
the four mill constants live on the pooled agent, the crowd field is one
reused `float[40]`, plaques are a fixed `Plaque[8]`, and the plaque domes
are 8 pooled `SpriteRenderer`s created at boot. This satisfies
`CLAUDE.md`'s pooling requirement by construction — nothing new is ever
instantiated at runtime.

---

## 3. Proximity-based ingress

### 3.1 The three routes in, and their rank

| Route | Who | Gate | Rate | Reads as |
|---|---|---|---|---|
| **Plaque rupture** (§1.4–1.5) | all classes | a crowd of > 2 holding a stretch of wall | charge fills in 3.5–56 s | a blister swelling, then bursting |
| **Hole squeeze** (§6.4) | bacteria only | a **dead** barrier segment | one at a time, 2 s each | a queue squeezing through a gap |
| **Barrier infection** (§6.5) | virus only | a **living** barrier segment | 0.05/s per adhered virion, then 4 s | one wall cell going violet, then lysing |

The rupture is the mechanic; the other two are consequences of it
(a rupture is what kills barrier segments in the first place) and are
individually bounded so that neither can become the trickle §0.2 forbids.

### 3.2 Adhesion itself is unchanged

`PathogenAgent.AdhesionChanceAt(depth) = AdhesionChanceAtWall ·
exp(−depth / AdhesionFalloffCells)` (0.30 at the wall, falloff 5 cells),
rolled once per lumen step (`LumenStepIntervalSeconds = 0.35`), stays
exactly as it is. It is already "proximity-gated" in the §1b step 1 sense
and the Director has not asked for it to change; `MapVerification`'s
monotonicity and cohort tests keep passing. The only difference is where a
successful roll puts the pathogen: into the mill at `dBase = 1.3` near its
current `s`, rather than snapped to the wall at a lane's stack position.

Two small consequences to keep in mind:

- `InvasionTally.Adhesions` keeps its meaning (one per join), which is
  what `MapVerification.RunChannelCohort` counts.
- A pathogen in the mill is **not** excreted by riding past the end of the
  lumen, because it is no longer in the flow. It leaves the mill only by
  creeping off the downstream end of the wall (§2.2), by being sloughed
  (§5), by being carried away coated (§4), or by entering tissue. This is
  §6b's "persists round to round" and is intended.

### 3.3 Tick cadence

| What | Cadence | Why |
|---|---|---|
| Mill base positions, crowd field, plaque membership + charge | every `GutWall.Tick(dt, now)` call, i.e. every frame in play, driven by `PathogenSpawner.Tick` as today | the charge is continuous; there is no roll to schedule |
| Plaque seeding | once per tick, one seed maximum | crowds form over seconds |
| Barrier damage, healing, hole squeezes, viral transcytosis | every tick, as accumulating floats/timers | same reason |
| Slough events | own timer, `SloughIntervalSeconds` | §5 |
| IgA binding | own roll, `IgaBindChancePerSecond`, applied as `1 − (1 − p)^dt` | §4, the same shape `GutWallFeatures.TrySample` already uses |

`BreachRollIntervalSeconds` (1 s) is **retired**, and with it the
`BACKLOG.md` bug "`GutInterface`'s roll clock jumps on unfreeze — every
occupied wall position rolls once on the first unfrozen tick, so a breach
can fire in round-frame 1". A charge that advances by `dt` cannot bank up
across a freeze, so that defect disappears rather than being fixed.

### 3.4 How the chance scales with crowding — the probabilistic form

Recorded in full because Fork 1 offers it, and because an implementer may
want it as a fallback. Keeping the 1 s roll interval, a plaque with `m`
members would rupture with per-roll probability

```
p(m) = 1 − exp( −(m − IngressCrowdFloor) · ChargeRatePerCrowdUnit )     [m > floor]
```

i.e. 1.8%/s at 3 members, 5.3% at 5, 13.4% at 10, 25% at 18 — the same
mean times as §1.6's table, with the variance of a memoryless process.
Everything else in this spec (membership, footprint, the barrier, IgA,
slough) is unchanged by that swap; only the drawn blister changes meaning,
from "how full" to "how crowded".

---
## 4. Secretory IgA — the §5 ~70% rung

> **The Director settled this section himself, 2026-10-18**, while this
> spec was being written, and his three rulings are **design input, not
> open questions**:
> 1. **Agglutination is the resolution.** IgA is "two-headed" (a dimer)
>    and ties a pathogen to **another matching pathogen**. The resulting
>    object is **larger, and its bulk makes it harder to invade.**
>    Crowding of *free* pathogens must still build pressure and burst;
>    crowding of *cross-linked* pathogens must not. "The ~70% rung has to
>    be an unambiguous reward, never a trap."
> 2. **A bound pathogen is much slower**, and **easier for immune cells to
>    deal with**.
> 3. **Tagged bacteria are pacified** — they lose virulence.
>
> He also added **in-lumen bacterial replication** to this sprint's scope;
> that is §4a. Every number in §4 and §4a is a **labelled tunable with its
> reasoning**, because a parallel research pass (`docs/IGA_REFERENCE.md`)
> may correct the magnitudes — a correction should change a number, not
> this design.

### 4.1 The headline: under IgA, density flips sign

The rung already exists as data: `KnowledgeCapability.SecretoryIgA`,
threshold 70%, short name "IgA", `Built = false`
(`Adaptive/KnowledgeLadder.cs`). §5 calls it "the only capability that acts
in the lumen" and "the only mechanistic answer to *how do you defend the
lumen*". §6b sequences it after mucus turnover: turnover is the innate
flush, IgA is the adaptive one.

The mechanism is **immune exclusion**, and the Director's "two-headed"
framing is the mechanically important half of it. Dimeric secretory IgA has
two antigen-binding ends, so one molecule can bridge **two organisms of the
same species**. Bridge enough of them and you have an aggregate too bulky
to approach the epithelium, which the mucus then carries away. Nothing is
killed; everything leaves.

Cross-linking **needs a partner**, so its rate depends on how many like
organisms are nearby. That is the whole resolution of the trap in one
sentence:

> **Free crowding charges a plaque. Coated crowding cross-links.** The same
> density that makes untreated pathogens dangerous is what makes treated
> ones agglutinate and be swept out. Above 70% knowledge, a dense patch of
> that species is not a threat forming — it is a clearance event forming.

So the mechanic is specified in **two stages**, and the distinction between
them is what makes the Director's three rulings all land in the right
place:

| Stage | Trigger | Can grip the wall? | Counts toward plaque charge? | Can enter tissue? |
|---|---|---|---|---|
| **Coated** (one organism, IgA bound) | a per-second roll | yes, weakly | yes, at `IgaGripWeight` | yes — and in tissue it is slow, pacified and fragile (rulings 2 and 3) |
| **Cross-linked** (≥ 2 coated organisms of the same class bridged) | a per-second roll **per coated neighbour** | **no — bulk** | **no** | **no** |

A lone coated organism on an empty stretch of wall therefore still gets in
occasionally, and when it does it is a feeble version of itself — which is
where rulings 2 and 3 pay off. A *crowd* of them never gets in at all,
which is ruling 1.

### 4.2 Eligibility and coating

A pathogen is **eligible** when `KnowledgeLedger.Get(species) ≥
IgaKnowledgePercent` for its class **and** it is inside the mucus band —
adhered at any depth, **or** still riding the flow at lumen depth 0–1,
because §5 is explicit that IgA acts "before adhesion is possible". Each
tick it rolls `p = 1 − (1 − IgaCoatChancePerSecond)^dt`, the same
`1 − (1−p)^dt` shape `GutWallFeatures.TrySample` already uses.

| Tunable | Proposed | Reasoning for the magnitude |
|---|---|---|
| `IgaKnowledgePercent` | **70** | fixed by `KnowledgeLadder`; not a free number |
| `IgaCoatChancePerSecond` | **0.18 /s** | mean ~5.5 s to coat. Fast enough that the player sees the wall turn pearly within a few seconds of earning the rung (the reward must be legible on arrival), slow enough that a big round still has free organisms gripping for a while |
| `IgaCoatRetentionSeconds` | **∞** (permanent) | real sIgA does wash off, but a decay timer would add a second clock for no mechanical gain, and "it got better" would read as a bug. Revisit only if the literature pass says coating is genuinely transient on a gameplay timescale |

### 4.3 Cross-linking — the bulk rule

A **coated** organism rolls, per second, against each coated neighbour of
the **same `PathogenClass`** within `IgaCrossLinkRadiusCells`:

```
p_link(dt) = 1 − (1 − IgaCrossLinkChancePerSecondPerNeighbour · n_coatedNeighbours)^dt
```

On success the two join one **clump** (or the smaller clump merges into the
larger). The matching rule is the Director's: IgA ties a pathogen to
*another matching pathogen*, and knowledge is per-species, so cross-linking
is within a class only. Mixed rounds are therefore harder to clear by IgA
alone, which is correct and is a nice piece of teaching.

A clump of `k` organisms has **bulk**:

```
clumpRadiusCells = IgaClumpBaseRadiusCells · sqrt(k)
holdDepthCells   = max(IgaHoldDepthCells, clumpRadiusCells)
```

and the bulk is what does the work:

1. **It cannot grip.** A clumped organism contributes **nothing** to the
   crowd field and cannot be a plaque member. Its own adhesins are spent on
   its neighbours, and its bulk holds its surface away from the epithelium.
2. **It is held off the wall**, at `holdDepthCells`, which is outside
   `GripDepthCells` by construction and grows as the clump grows. `dBase`
   lerps there at `IgaPushOutSpeedCellsPerSecond`.
3. **It is swept out.** Its passive creep becomes
   `MucusCreepCellsPerSecond · IgaClearCreepMultiplier`, so it clears the
   wall and rejoins the flow, then is excreted (`InvasionTally.Excreted`,
   plus a new `IgaExcluded` counter for the debug readout and the strategy
   harness).
4. **It is not released by a rupture happening next to it**, because it is
   not a member of the plaque.
5. **It cannot squeeze through a hole** (§6.4) — a hole is one segment
   wide and the clump is wider.
6. **It does no barrier damage** (§6.3): held away from the cells, and
   pacified besides.

| Tunable | Proposed | Reasoning for the magnitude |
|---|---|---|
| `IgaCrossLinkChancePerSecondPerNeighbour` | **0.25 /s** | with 2 coated neighbours a pair bridges in ~2 s, so a *dense* coated patch agglutinates almost as fast as it coats — this is the "density flips sign" promise, and it must be visibly faster in a crowd than alone |
| `IgaCrossLinkRadiusCells` | **0.45** (33 px) | about two pathogen diameters: they must look like they are touching when they bridge |
| `IgaClumpBaseRadiusCells` | **0.30** | a 1-organism "clump" would be 22 px; `sqrt(k)` puts a 4-clump at 0.60 cells (44 px) and a 9-clump at 0.90 (66 px) — a visibly chunky raft, still under one cell so it does not swamp the wall |
| `IgaClumpMaxMembers` | **9** | past this a new clump forms instead. Keeps `sqrt` bulk bounded and keeps the drawn raft under 1 cell |
| `IgaHoldDepthCells` | **1.15** | the floor on hold depth: beyond `GripDepthCells = 0.55`, inside `MucusBandDepthCells = 1.6`, so a clump sits in the drawn mucus and not out in open chyme |
| `IgaPushOutSpeedCellsPerSecond` | **0.6** | ~1.5 s to be pushed off the wall: a visible "it lets go" beat, not a snap |
| `IgaClearCreepMultiplier` | **3.0** | 0.105 cells/s, so a clump crosses the remaining wall in under a round rather than loitering. Ruling 1 says the rung is a reward; a clump that hangs around for five minutes does not feel like one |

### 4.4 Slower, pacified, and easier to kill (rulings 2 and 3)

These apply to **any coated organism**, clumped or not, and they are what
make a coated organism that *does* get into tissue a bad investment for the
attacker.

| Tunable | Proposed | Applies to | Reasoning |
|---|---|---|---|
| `IgaMotilityMultiplier` | **0.35** (Fork 9) | the mill's cohesion speed and loop amplitude; `TissueStepIntervalSeconds` (divided, so steps take longer) once in tissue | "much slower" (ruling 2) without stopping it dead. ×0.35 means a coated bacterium in tissue takes ~2.9 s per cell instead of 1 s — it is visibly labouring, and a front of them can be held by far fewer cells |
| `IgaVirulenceMultiplier` | **0.25** | `WallGrazeDamagePerSecond`, `LargeBacteriumHostDamagePerStep`, `IntracellularEntryChance`, `BarrierInfectChancePerSecond` | "pacified… losing virulence" (ruling 3). Reduced rather than zero, so it is still a pathogen and still has to be cleared; **Fork 11** offers zero |
| `IgaDamageTakenMultiplier` | **1.6** | `PathogenAgent.ReceiveDamage` | "easier for immune cells to deal with" (ruling 2) as a flat damage multiplier: one line, no new state, and it reads on screen as a coated bacterium dying in 2 contacts instead of 3. **Fork 10** offers the alternatives (a contact-rate/to-hit change, or lower effective health) |
| `IgaHoleTransitMultiplier` | **2.0** | `HoleTransitSeconds` | a coated organism squeezing through a hole takes 4 s instead of 2 — slower, per ruling 2, and a longer window to matter |

Note what is **deliberately not** here: no killing, no complement, and no
opsonisation-as-targeting. Killing by antibody is the 60% complement rung
and antibody-guided targeting is the 45% Fc rung; this rung must stay
"exclusion, plus a weaker pathogen", or the ladder's rungs stop being
mechanically distinct (§5's stated requirement for the roster).

### 4.5 The trap, and why it is now closed four times over

The brief's warning was that trapping pathogens *at* the wall concentrates
them, and under proximity ingress concentration is what breaches — so a
naive IgA would charge 70% of a species' knowledge to make that species
*more* dangerous. The Director's agglutination ruling closes it, and the
model closes it redundantly on purpose, because this is the one place where
a plausible implementation is actively harmful:

1. **Bulk cannot grip** (§4.3 #1): a clumped organism contributes 0 to the
   crowd field.
2. **Bulk is held out** (§4.3 #2): beyond `GripDepthCells` by construction,
   and further out the bigger the clump — so density *increases* the
   exclusion.
3. **Bulk is not a plaque member** (§4.3 #4): a rupture beside a raft does
   not take the raft in.
4. **Cross-linking accelerates with density** (§4.3): the denser the
   coated patch, the faster it agglutinates and leaves. This is the one
   that makes the inversion real rather than merely safe.

And the payoff is the mirror image of the failure mode: because coating
rolls against organisms that are **already gripping**, earning the rung
**visibly dissolves an existing threat**. A plaque at 70% charge loses its
members to coating and then to agglutination, its `MemberCount` falls below
`IngressCrowdFloor`, `ChargeDecayPerSecond` takes over, and **the blister
deflates on screen** while the swarm turns pearly, rafts up and drifts
away. That is the best feedback moment in this design and it costs nothing
— it falls out of the charge model (§1.4) rather than being scripted.

Residual risk, stated honestly: IgA makes the wall nearly safe against one
species, and the wall is the only place that species can enter from. At 70%
of one of three classes that is a strong but narrow reward, narrow in the
right way — the other classes are untouched, knowledge is per-species, and
a new species in a later round meets a naive wall. If it proves too strong
the lever is `IgaCoatChancePerSecond`, not the mechanic.

### 4.6 How it looks

- **The coat.** A pale blue-white halo behind the organism, new
  `Palette.IgaCoat = 0.86, 0.94, 1.00 @ 0.85` — the mucus family (the glaze
  is white @0.35) pushed cool so it reads against both the green bacterium
  and the violet virion, and clear of every reserved hue.
- **The antibody.** New 64 px `SpriteShapes.IgaY`: three capsules in a Y,
  `Finish(b, 5)`, drawn at ~0.3 cell. **Between two cross-linked
  organisms the Y is drawn spanning the gap, one head on each** — the
  "two-headed" ruling made literal, and the single clearest piece of
  teaching in the whole rework. A Y is the universal antibody glyph in
  every diagram on the Director's reference list, and it needs no tooltip.
  No faces (standing Director decision).
- **The clump.** One `IgaCoat`-tinted `AggloBlob` per clump, scaled to
  `clumpRadiusCells`, drawn *under* its members so they read as embedded.
  The organisms keep their own lazy-eight loops inside it at
  `IgaMotilityMultiplier` amplitude, so a raft wobbles as one body.
- **The drift.** Rafts visibly outrun the rest of the mill and leave
  downstream. The knowledge HUD's IgA pip goes solid
  (`KnowledgeLadder.Rung.Built = true`) the moment this ships — the first
  of the six rungs to do so, which is itself a reward.

---

## 4a. Bacterial replication in the lumen (new scope, Director 2026-10-18)

> "Bacteria replicate while in the lumen", damped by **density** ("some
> bacteria won't replicate fast when they're close to other members of
> their group") and by **IgA**.

This is the piece that makes the lumen a **living population** instead of a
delivery queue, and it changes what the bolus means: the bolus now seeds an
inoculum rather than delivering a fixed batch. It also gives the player
something to watch and act on *before* anything is at the wall, which is
the thing round 1 has always lacked (`BACKLOG.md`: "round 1 still has no
in-round player action once you press Start").

### 4a.1 Who replicates, and where

| Class | Replicates in the lumen? | Why |
|---|---|---|
| `LargeBacterium` | **yes** | free-living, extracellular, this is what it does |
| `IntracellularBacterium` | **yes, slower** (`×IntracellularLumenRateMultiplier`) | it survives extracellularly (§4b: "no death clock… it has the chassis to survive") but its niche is inside a cell; `IntracellularReplicationIntervalSeconds = 3 s` *inside a host* stays the fast case |
| `IntracellularVirus` | **no** | a virus cannot replicate without a host cell. This is the same rule as §4b's free-virion death clock and it is non-negotiable biology |

A bacterium replicates whether it is **riding the flow** or **milling at
the wall** — both are "in the lumen". Not while gripping inside a plaque
footprint? It still does; the crowd damper below is what handles that case
and it handles it better than a special rule.

### 4a.2 The rate, and the two dampers

Each bacterium carries a `divideAt` time. On reaching it:

```
interval = LumenDivideIntervalSeconds
         / classMultiplier
         × (1 + LumenCrowdDamperStrength · localDensity)     // density damper
         × (coated ? IgaReplicationDamper : 1)               // IgA damper
```

`localDensity` is read **from the crowd field that already exists** (§1.3)
for a milling bacterium, and from a coarse count of same-class bacteria in
the same lumen cell for one still in the flow — no new spatial structure.

On division the parent spawns **one** daughter from the pool at
`IgaClumpBaseRadiusCells`-ish offset: for a miller, the same `(s, d)` with
a small random offset and a fresh set of mill constants; for a flow rider,
the same lumen cell. The daughter inherits class, coat state (see §4a.4)
and health, and both parent and daughter get a fresh `divideAt`.

| Tunable | Proposed | Reasoning for the magnitude |
|---|---|---|
| `LumenDivideIntervalSeconds` | **22 s** | the pacing anchor. A bolus transit is 30 s and a round is roughly 30–60 s of active time, so ~22 s means an unmanaged inoculum roughly **doubles once per round** — felt, not explosive. Real gut doubling times run 20–60 min, so this is a deliberate game-time compression, flagged as such |
| `IntracellularLumenRateMultiplier` | **0.5** | intracellular bacteria divide at half speed in the lumen; the fast case is inside a host cell, where §4b already gives them a 3 s interval. Keeps the classes mechanically distinct |
| `LumenCrowdDamperStrength` | **0.6 per crowd unit** | at crowd 5 the interval is ×4 (88 s); at crowd 10, ×7 (154 s). Chosen so that **a dense patch is near-stationary in population** — which is what keeps replication from compounding with the plaque mechanic into a runaway (see §4a.3) |
| `LumenDivideMaxPopulation` | **`maxLivePathogens` − 10** | a hard ceiling so replication can never starve the pool that the bolus needs; a blocked division just re-arms the timer |
| `IgaReplicationDamper` | **3.0** (i.e. ×3 the interval → ⅓ rate) | the Director's second damper. ⅓ rate is a strong but not absolute brake; a coated population shrinks because clearance (§4.3 #3) outruns a 66-s doubling |

### 4a.3 Why the density damper is load-bearing, not flavour

Without it, replication and the plaque mechanic form a **positive feedback
loop**: more organisms at a spot → faster charge *and* more divisions →
more organisms. The wall would go from quiet to catastrophic with no middle
state, and the pre-burst window the Director insists on would vanish into
an exponential.

With it, the two mechanics occupy different regimes, which is exactly the
shape a tower defence wants:

| Local density | Replication | Plaque charge | Reads as |
|---|---|---|---|
| sparse (crowd < 2) | **fast** (22 s) | **none** (below the floor) | a luminal population quietly growing — the warning |
| moderate (crowd 3–5) | slowing (66–88 s) | charging (56 → 19 s) | a spot forming; act now |
| dense (crowd ≥ 8) | nearly stalled (≥ 135 s) | fast (≤ 9 s) | about to go |

So **growth happens where it is harmless and stops where it is dangerous**,
and the dangerous state is reached by *gathering* (§2.2's cohesion), not by
exponential growth. The biology this stands in for is real and worth one
line in the game's own terms: a dense population is nutrient-limited and
quorum-signalled into a different, stickier mode — it stops multiplying and
starts colonising. Which is, pleasingly, exactly what the mechanic does.

### 4a.4 Replication × cross-linking: does dividing under IgA help or hurt?

It **hurts**, and this is worth stating because the opposite is a plausible
reading of the Director's "a dividing bacterium in a clump makes the clump
bigger".

In this model:

- A daughter of a **coated** parent is **born coated**
  (`IgaDaughterInheritsCoat = true`). Division does not shed the IgA on the
  parent's surface, and the daughter is the same antigen in the same
  mucus full of antibody.
- A daughter born inside a clump is **immediately a cross-link candidate
  with every member of that clump**, so it links in within ~1 s at
  `IgaCrossLinkChancePerSecondPerNeighbour · n`.
- The clump's bulk is `IgaClumpBaseRadiusCells · sqrt(k)`, so it grows,
  and `holdDepthCells` grows with it: **a bigger raft is held further off
  the wall and swept out at the same multiplied creep.**

Net: replicating inside an agglutinated raft converts one excluded organism
into two excluded organisms and pushes the raft further from the
epithelium. Under IgA, replication is the pathogen **digging in the wrong
direction** — population without access. Combined with the ×3 interval
damper, a coated population's net access to the wall falls over time even
as its head-count rises, which is the most honest possible rendering of
"immune exclusion": the organisms are still there, in numbers, and they
cannot get in. That is also a genuinely teachable idea — most of the
microbiota is exactly this.

Without IgA the same interaction is the threat: divisions in a sparse patch
feed the cohesion that builds the next plaque.

### 4a.5 How the player reads a growing luminal population

The danger of this mechanic is that it is invisible until it is a wall
problem. Four cues, cheapest first:

1. **Division is an event with a pop.** On each division play a small
   `BirthPuff` (the sprite already exists, used by the marrow) at the
   parent, 0.25 cell, 0.2 s. A lumen that is quietly doubling **twinkles**.
   One pooled renderer set, reused from the marrow's puff pool.
2. **The daughter is briefly smaller.** Scale 0.75 → 1.0 over 1.5 s, so
   the eye sees new organisms appearing rather than a count changing.
3. **The HUD already has the number.** `PathogenSpawner` tracks
   `live.Count` and the HUD shows "n in play"
   (`COSMETIC_PASS_SPEC.md` §2.7). With replication, that number **rising
   while no bolus is active** is the signal, so the HUD should show it as a
   delta: `n in play ▲`, in `UiTheme.Bad` when it has risen over the last
   5 s. One cached float and one arrow sprite.
4. **The debug readout** (and the strategy harness's CSV) gain
   `divisions`, `luminal population` and `coated fraction`.

### 4a.6 What it does to round pacing, and what must be re-checked

- **Rounds get a tail.** Today a round ends when the bolus leaves
  (`RoundController.ClearRound` on `spawner.BatchComplete`) and the
  leftovers persist into the buy phase, frozen. With replication, the
  leftovers **grow** in the next round rather than merely persisting. That
  is the §6b "unmanaged buildup creates future pressure" promise finally
  having teeth, and it is the main reason this mechanic is worth building.
- **`EconomyTuning.BatchSizeForRound = 16 + 6(N−1)` is now the *inoculum*,
  not the threat.** Linear growth in the seed plus geometric growth in the
  lumen means late rounds could overshoot badly. Two guards: the density
  damper (§4a.3) and `LumenDivideMaxPopulation`. Even so, **the batch
  curve should be re-tuned after this lands**, and that is a job for the
  headless strategy harness (`SESSION_HANDOFF.md` §2a), not for a guess
  here. Flag it in `BACKLOG.md` on landing.
- **The buy phase freeze covers it**, so a player cannot shop while the
  lumen doubles.
- **It gives the mucus-turnover upgrade a second job**: flushing removes
  replicating stock, so turnover now suppresses growth as well as pressure.
  This is the first time that shop row has been worth buying early.

---
## 5. Mucus sloughing — §6b made spatial

### 5.1 What §6b locked, and what "spatial" adds

§6b: the barrier upgrade raises the **shed rate**, not the capacity, and
"returns some fraction of barrier residents to the lumen each round to be
excreted"; it is "categorically different from killing — the player is
flushing rather than fighting", and it sequences before IgA as the innate
answer to the same problem. `ShopItem.BarrierMucusTurnover` exists, is
priced (30 ATP base, ×1.6 per level) and is inert.

"Each round, a fraction" is a number appearing in a counter. The spatial
version is the same thing with a **place and a moment**: discrete waves
that sweep patches of wall, so the player watches their purchase work and
can tell *where* it worked.

### 5.2 A slough event

| Field | Proposed | Note |
|---|---|---|
| `SloughBaseIntervalSeconds` | 18 | interval = `base · 2 / (1 + level)` → L1 18 s, L2 12 s, L3 9 s, L4 7.2 s |
| `SloughPatchCells` | 2.0 | 148 px of wall — bigger than a plaque footprint is *not* wanted; see below |
| `SloughWaveSpeedCellsPerSecond` | 2.0 | crosses its patch in ~1 s; visible, not instant |
| `AdhesionRefractorySeconds` | 1.5 | a sloughed pathogen cannot re-adhere while it drifts clear |

**Cadence.** One wave every `SloughIntervalSeconds`, on its own timer,
only while `shop.LevelOf(BarrierMucusTurnover) ≥ 1`. Nothing at level 0
(Fork 7 asks whether a baseline rate should exist).

**Where.** At the **worst** patch — the centre of mass of the highest
region of the crowd field, equivalently the most-charged live plaque.
Reasoning: the player bought it to solve a problem, so it should solve the
problem; and it is accurate, because mucin secretion really is
upregulated where the epithelium is irritated (goblet-cell hyperplasia).
The alternative — waves that wander down the wall on a fixed schedule,
whether or not anything is there — is more "turnover is constant, not
targeted" and is Fork 8.

**What it does**, in one call, as the wave front passes each pathogen:

1. Every pathogen in the patch (**any** depth in the mucus band, coated or
   not) is **returned to the flow**: `PathogenState` → `Lumen`, placed at
   the near-wall lumen cell for its `s`, `stepTimer` reset, and
   `adhesionRefractoryUntil = now + 1.5`. It then rides the flow and is
   excreted unless it re-adheres downstream. `InvasionTally.Sloughed`
   counts them.
2. Every **plaque** whose centre is in the patch has its `Charge` reset to
   0 and its membership cleared. Pressure is physically removed, not
   merely reduced.
3. Barrier segments in the patch are **not** healed and **not** damaged. A
   slough wave is a mucus-and-shed-cell event; repairing an ulcer is the
   crypt's job (§6.6). Keeping these separate is what stops the mucus
   upgrade from quietly becoming the barrier-repair upgrade too.
4. **IgA rafts in the patch go too** (§4.3), whole, which is the correct
   interaction: mucus turnover is the transport half of immune exclusion,
   so the two upgrades compound rather than overlapping. §6b already says
   they "sequence correctly".
5. **It removes replicating stock** (§4a). Flushing 8 bacteria does not
   just relieve pressure now, it deletes their future divisions — which is
   the first reason in the game's history to buy this row *early* rather
   than when the wall is already bad.

**Why the patch is 2 cells and not 4.** A wave that clears a third of the
wall at once makes the upgrade a round-winner and makes the wall
uninteresting. Two cells clears one plaque's worth of ground and leaves
the rest, so repeated purchases (shorter interval) are how the player
scales it — which is what `ShopTuning`'s repeatable-level model already
assumes.

### 5.3 How it looks

A pale crescent sheet — new `SpriteShapes.SloughWave`, a 64 px arc with a
soft leading edge, tinted `Palette.Mucus` brightened to a new
`Palette.MucusSlough = 1.00, 1.00, 1.00 @ 0.75` — spanning the patch's
2 cells and travelling downstream at 2 cells/s with a ~1.2 s life. Under
it: the local mucus band's alpha pops from 0.35 to 0.6 and falls back over
2 s (freshly secreted mucus), the villi in the patch flatten ~20% and
spring back, and the pathogens it has picked up **tumble with the wave**
for ~0.5 s (their `dBase` pushed out to 1.6 and their loop amplitude
tripled) before they rejoin the flow. The read is unmistakable: a sheet of
slime peels off the wall and takes the bugs with it.

Effect-colour note: this is a *sprite*, not a `DegranulationFlash`, so it
does not enter `ShapeFor`'s exact-colour-match table and cannot collide
with the five FX colours (`Sprint20Verification` asserts their
distinctness).

---

## 6. Barrier-cell death

### 6.1 The asymmetry the Director asked for

> a dead epithelial cell is a hole **bacteria** can use, but it still
> **blocks viruses**, because a virus needs a *living* cell to infect.

This is correct biology and it is a genuinely new strategic axis, because
it makes the two threat families want **opposite** barrier states:

| Barrier state | Bacteria | Viruses |
|---|---|---|
| Intact | must gather and rupture it (§1) | must gather and rupture it, **or** infect a cell (§6.5) |
| Eroded | rupture is nearer | infection is a little likelier |
| **Dead (a hole)** | **walk through, one at a time** (§6.4) | **blocked outright** — there is no living cell to enter and no epithelium left to rupture |

So a wall full of holes is a bacterial sieve and a viral fortress, and a
pristine wall is the reverse. The player cannot optimise for both, which
is exactly the kind of tension `GAME_DESIGN.md` §4a's class model exists to create. Round
4's shipped tagline is already *"Raw oysters, norovirus + Vibrio"* — a
mixed round is the one where this choice bites.

### 6.2 The structure, and the §1c conflict — flagged, not resolved

**Proposed:** a new `Grid/GutWall` class (the renamed, rewritten
`GutInterface`) owns a 1-D array of **barrier segments**:

```
BarrierSegmentsPerLane = 2      →  20 segments on Map 01
segment length = 0.5 cell = 0.56 world = 37 px
struct BarrierSegment { BarrierState State; float Health; float RegrowAt; PathogenAgent Occupant; }
enum  BarrierState { Intact, Eroded, Infected, Dead }
```

**⚑ The conflict with §1c, stated rather than quietly resolved.** §1c is a
Director-set section: *"a host cell has three states — healthy, infected,
dead — and that state influences the state of the lattice position"*, and
the head session's two-layer structure (host layer + occupant layer) lives
on `TissueGrid`. A barrier cell is an epithelial cell with, now, states
that read `Intact / Infected / Dead` — the same three ideas under
different names, on a **different structure**, with a fourth state
(`Eroded`) that §1c's host layer does not have (the host layer carries
damage as a separate `health` float instead).

Three ways to resolve it, and the recommendation is the first:

1. **Keep them separate, and say so in §1c.** `GutWall` owns barrier
   state; `TissueGrid` owns the lattice. The justification is already
   written into the code: `GutInterface`'s class comment explains that the
   interface cannot live on `TissueGrid` because the lattice holds exactly
   one occupant per coarse slot and structurally cannot express a pile.
   The same argument applies to the wall's own cells: they are 20 segments
   on a seam, not 10 lattice positions, they have their own geometry
   (`s`), their own regrowth rule (crypt-driven, §6.6) and their own
   occupant rule (one squeezer at a time). **Cost:** two state
   vocabularies in the game, which a future session must not conflate.
   Mitigated by naming them differently on purpose (`BarrierState` vs.
   `HostState`, `Intact` vs. `Healthy`).
2. **Put the barrier on the lattice** — extend the host layer to the
   near-wall lumen row (axis 19). **Cost:** that row is not drawn as cells
   at all any more (Sprint 15 took the lumen out of `BoardRenderer`; the
   `UI_STYLE_GUIDE.md` sorting table and `COMPARTMENT_DESIGN.md` §2.1 both
   assume a field-plus-motes lumen), the resolution would drop to 10
   segments, and `TissueGrid`'s regrowth, debris, cytokine and
   infected-source loops would all have to learn to skip or special-case
   that row. Rejected on cost, not on principle.
3. **Make it one state set shared by both structures.** Cleanest
   conceptually, the largest diff, and it would drag §1c (LOCKED) open.
   Not now.

Either way this needs a Director-visible line in §1b or §1c, which §8
proposes.

### 6.3 What kills a barrier segment

| Cause | Amount | Note |
|---|---|---|
| **A plaque rupture** | **instant death** of every segment in the ±1.5-cell footprint (6 segments) | the primary cause, and the reason a breach site stays dangerous |
| **Large-bacterium grazing** | `WallGrazeDamagePerSecond = 0.6` per gripping, uncoated `LargeBacterium` over the segment | 17 s for one to kill a segment, ~6 s for three. Mirrors `LargeBacteriumHostDamagePerStep = 2.5` in tissue against `HostCellMaxHealth = 10`; `BarrierSegmentMaxHealth = 10` for the same reason |
| **Viral lysis** | death at the end of transcytosis (§6.5) | so viruses *make* bacterial holes |
| Intracellular bacteria, virions in the mill | **nothing** | class differentiation: `GAME_DESIGN.md` §4a's large bacterium is the one that "kills and directly occupies"; the intracellular classes invade rather than erode |
| Immune cells | **nothing** | they cannot reach the wall (§0.3) |

A segment at `Health < BarrierSegmentMaxHealth` is **`Eroded`**: drawn
thinner and paler, its local mucus alpha reduced, and its stretch takes
`ErodedChargeMultiplier = 1.4` on any plaque charge over it (a damaged
barrier gives way sooner). At `Health ≤ 0` it is **`Dead`**.

### 6.4 A hole: bacterial passage, bounded

A gripping bacterium (either bacterial class) whose `sBase` lies over a
`Dead` segment, and where `segment.Occupant == null`, starts a **squeeze**:

- the segment's `Occupant` is claimed, so **one at a time per segment**;
- it takes `HoleTransitSeconds = 2.0`, during which the pathogen is drawn
  being drawn through: `dBase` lerps from its grip depth to −0.3 (i.e. it
  sinks *into* the seam), its sprite scale narrows to 0.75 on the
  along-wall axis, and a small dark notch is drawn at the segment;
- on completion it enters tissue at the cell behind that segment via the
  ordinary `EnterTissueAt` path, so every downstream rule (§4b infection,
  base-directed advance) is unchanged. If no slot is free, the squeeze
  aborts and the occupant is released.
- `InvasionTally` gains `SqueezedThrough`.

**Why this does not become the trickle.** A hole leaks at most 0.5
pathogens/s and only while it is dead (20 s, or ~7 s near a crypt — §6.6),
holes only exist where a rupture or a virus already made one, and the
queue is the most legible thing on the wall: a line of bacteria waiting at
a visible gap *is* pressure at a spot, in a different register. The
alternative — holes that merely make the local charge faster, with no
direct passage — is Fork 4, and it is the safer answer if the Director
reads this as a leak.

### 6.5 Viral ingress needs a living cell — so it takes one

A virus cannot use a hole; §0.2's shape still has to give it a route other
than waiting for a crowd. The route is the obvious one, and it is also how
norovirus and rotavirus actually get in: **infect the enterocyte**.

- An adhered, uncoated virion over an `Intact` or `Eroded` segment rolls
  `BarrierInfectChancePerSecond = 0.05` (mean 20 s) — on `Eroded`,
  ×1.4 like the charge.
- On success the segment becomes **`Infected`**: drawn in
  `Palette.HostInfectedViral` (`0.80, 0.66, 0.95`), the same lilac as an
  infected host cell, because it is the same event. The virion is hidden
  inside it (`IsIntracellular`-like, removed from the mill and from the
  crowd field).
- After `BarrierTranscytosisSeconds = 4.0` the segment **dies** and the
  virion is delivered to the tissue cell behind it, where
  `SettleIntoTissue` runs as it does today (so it infects a healthy host
  cell if there is one, or lands as a free virion and starts its death
  clock).
- The segment is now `Dead` — **a hole the bacteria can use**. A viral
  round softens the wall for the next bacterial one, which is both a nice
  piece of teaching and a real phenomenon.
- If the segment is sloughed (§5) or ruptured before the 4 s is up, the
  virion dies with it. Catching it early denies the entry, exactly as
  §4b's stress-sense roll denies the brood.

This is the one place where the spec adds a mechanic the Director did not
name, so it is **Fork 5** — the alternative is that viruses simply cannot
enter except in a rupture, which is simpler and makes a holed wall
*absolutely* viral-proof.

### 6.6 Healing, and the crypt's second job

| Field | Proposed | Note |
|---|---|---|
| `BarrierSegmentMaxHealth` | 10 | = `TissueTuning.HostCellMaxHealth`, so damage numbers read the same everywhere |
| `BarrierHealPerSecond` | 0.35 | an `Eroded` segment with nothing grazing it is whole again in ≤ 29 s |
| `BarrierRegrowSeconds` | 20 | a `Dead` segment becomes `Intact` after this, at full health |
| crypt bonus | ÷ `GutWallFeatures.RegrowthMultiplier` = ÷3 at default `CryptRegrowthBonus = 2` | **6.7 s** near a crypt |

The crypt hook is the best thing in this section and it costs almost
nothing: `GutWallFeatures.RegrowthMultiplier(CoarseCoord)` already exists,
already returns `1 + CryptRegrowthBonus` within `CryptRadiusCells = 3` of
a crypt, and is already wired to the shop
(`UiController` → `SetCryptCount`). Feeding the barrier's regrow timer
through it means **crypts repair the gut wall**, which is literally what
crypt stem cells do — the epithelium is renewed from the crypt base, and
it is the fastest-renewing tissue in the body (§6 of `GAME_DESIGN.md`
already proposes crypt-centred recovery for the tissue lattice). It gives
the 25-ATP crypt a visible second job, and it makes "where did I put my
crypts" a real decision about the wall as well as the tissue.

A `Dead` segment also **loses its mucus locally** (the band's alpha drops
to 0.1 over it) and therefore adheres new arrivals slightly more readily;
`DeadSegmentAdhesionBonus = 1.3` on `AdhesionChanceAt` for a lumen step
over a dead segment. Small, and it is the mechanism behind
`BACKLOG.md`'s long-open "barrier attrition" idea (transiting pathogens
thin the barrier, raising later adhesion) without needing a separate
attrition system.

---
## 7. What the player sees — sprites, palette, sorting

### 7.1 The pressure read, in order of strength

The mechanic is worthless if the player cannot read it — the reason
`GutInterfaceRenderer` exists at all. Five cues, each cheap:

1. **The blister.** One pooled dome per live plaque, 3 cells wide
   (221 px), height `lerp(0.10, 0.75) · charge` cells (7 → 55 px), colour
   `lerp(Palette.GutWallQuiet, Palette.GutWallAlarm, charge)`. **The
   blister *is* the charge** — there is no normalisation choice to make,
   which retires `GutInterfaceRenderer.FullAlarmCount = 8` and the comment
   apologising for it ("a lone pathogen must not look like a crisis just
   because nothing worse exists yet"). A lone pathogen now has no plaque
   at all.
2. **The constricting rim.** `SpriteShapes.StressRing` (it already exists)
   around the dome, scale `1.45 → 1.02` as charge goes 0 → 1, alpha
   `0.3 → 1.0`. A ring tightening onto a swelling dome is the "about to
   go" language.
3. **The swarm.** The mill itself: a knot of 8 pathogens at one spot is
   legible without any UI, and its size *is* the number driving the charge.
4. **Bare mucus.** The mucus band's alpha is modulated along the wall by
   the crowd field: `0.35 → 0.12` where crowd ≥ `PlaqueSeedCrowd`. The
   wall looks stripped where it is under attack. Needs the band split into
   `CrossLength` quads, or a per-vertex alpha, or (cheapest) one extra
   "scrape" quad per plaque at the same footprint — recommend the last.
5. **The pre-burst tell.** In the last 15% of charge the dome pulses at
   3 Hz (scale ±6%) and sheds 2–3 flecks per second
   (`SpriteShapes.FlowMote`, tinted `GutWallAlarm`) into the channel. This
   is the ~2-second "act now" window, and it is the difference between
   "defending it is a decision" and "reacting to it afterwards".

And on rupture, unchanged: `DegranulationFlash.Play(..., BreachBurstColor)`
scaled by the released count (`GutInterfaceRenderer.OnBreached` already
does exactly this) — then the footprint's segments go dark and **stay**
dark, which is the first time this board has had a lasting mark of where
it was hurt.

### 7.2 New sprites (procedural, 64 px, white-on-alpha, `OuterStroke`-finished)

| Sprite | Construction | Used by |
|---|---|---|
| `WallBlister` | half-ellipse (flat side on the wall), inner highlight arc at value 0.9 offset 0.1S toward the flat side, `OuterStroke(b, 5, 0.18)` | the dome, §7.1 #1 |
| `IgaY` | three capsules of width 7 meeting at (32,38): stem to (32,60), arms to (12,10) and (52,10); `Finish(b, 5)` | IgA, §4.6 |
| `AggloBlob` | union of 4 discs r 18–24 at jittered positions (seed 7), value 1.0, soft 3 px alpha falloff, no stroke | IgA clumps |
| `SloughWave` | a crescent: disc r 30 minus disc r 30 offset (0, −16); leading edge ramped 1.0 → 0.4 across 8 px | slough, §5.3 |
| `WallNotch` | a rounded trapezoid gap mask, 0.5 cell wide, value 1.0 | dead/squeezing segments, §6.4 |

Reused rather than new: **`BirthPuff`** for a lumen division (§4a.5) —
the marrow's puff pool, a second client for it; and `IgaY` is drawn
**twice over** in practice, once as a free tag on a coated organism and
once spanning the gap between two cross-linked ones (§4.6).

`SpriteShapes.EpithelialBar` is reused per **segment** rather than per
lane; `StressRing`, `FlowMote`, `MucusBand`, `Villus` are unchanged.
`SpriteShapeDump.DumpAll` should be extended to the five new shapes and
each checked magnified **before** a build is spent — `AGENT_HANDBOOK.md`
is emphatic that a few spec numbers are always wrong in ways only a raster
shows.

### 7.3 Palette additions (two, and only two)

| Name | RGB | Why this value |
|---|---|---|
| `IgaCoat` | `0.86, 0.94, 1.00 @ 0.85` | the mucus family (white @0.35) pushed cool. Saturation 0.14 against macrophage blue's 0.77 and M-cell lilac's 0.10-at-a-different-hue; legible over both the green bacterium and the violet virion |
| `MucusSlough` | `1.00, 1.00, 1.00 @ 0.75` | freshly secreted mucus: the existing glaze at double opacity |

Everything else reuses shipped values: the dome lerps
`GutWallQuiet → GutWallAlarm`, an infected segment is
`HostInfectedViral`, a dead segment is `EmptyPit` (`0.42,0.26,0.32` — the
"dark rose scar, not a hole in the render" the Sprint 20 pass chose for
exactly this job), an eroded segment is `GutWallQuiet` at value ×0.88, and
the rupture flash is `FxBreach`. Neither new colour touches
`DegranulationFlash.ShapeFor`'s exact-match table, so
`Sprint20Verification`'s pairwise-distinctness check is unaffected.

### 7.4 Sorting (a delta to `UI_STYLE_GUIDE.md`'s table)

| Order | What | Change |
|---|---|---|
| 2 | lumen mucus sheen | + per-plaque "scrape" quad at the same order |
| 3 | **barrier segments** (`EpithelialBar` × `CrossLength · 2`) | replaces one bar per lane |
| 4 | **plaque blister + constricting rim** | new |
| 5 | **slough wave** | new (above the flow motes at 4) |
| 19 | **IgA agglutination blob** | new, under the pathogens it holds |
| 20 | pathogens | unchanged |
| 21 | **IgA Y marks**; **lumen division puffs** (`BirthPuff`) | new, over the organisms |
| 30 | rupture starburst | unchanged |

**Renderer budget:** 20 segments + 8 domes + 8 rims + 8 scrapes + 1 wave +
12 pooled clump blobs + 24 pooled Y marks ≈ **81**, replacing today's 10
bars. For scale, Sprint 15 freed ~110 renderers by taking the lumen and
base out of the per-cell grid, and the board holds 250 tissue cells. All
81 are created at boot and reused; nothing is instantiated during play.

---

## 8. Migration: locked text, API, harnesses

### 8.1 `GAME_DESIGN.md` §1b — proposed replacement wording

The "under revision" note at the head of §1b is deleted on landing, the
section keeps its LOCKED marker with a new date, steps 3–5 are untouched,
and steps 1 and 2 become:

> ### Step 1 — Adhesion is proximity-gated, and an adhered pathogen joins the mill
>
> A pathogen riding the lumen flow has a **chance to adhere that depends
> on its distance from the gut interface**. Close to the boundary, it
> likely adheres; far out in the channel, it likely does not and simply
> flows past. This makes lateral position in the lumen meaningful rather
> than decorative, and it means the flow itself does real work: a
> pathogen's lane through the lumen determines its odds of ever becoming
> the player's problem. A pathogen that never adheres rides the flow out
> the bottom and is excreted, with no penalty.
>
> **On adhering, the pathogen enters the mucus layer and mills there.** It
> holds a continuous position along the wall, drifting slowly downstream
> with the mucus in time with peristalsis, looping as it goes, and
> gradually **gathering toward other adherents** — colonisation is a
> social process, and a lone organism on a healthy barrier is not an
> invasion. It is now colonising the gut interface (§6b): not in the
> tissue, not attackable, not yet the player's problem, but accumulating.
> It leaves the mucus only by being swept off the downstream end, by being
> sloughed (§6b), by being excluded by secretory IgA (§5), or by getting
> into the tissue (step 2).
>
> **There are no discrete interface positions and no queue.** The pile is
> not per-lane and is never drawn as a stack: the wall is a continuous
> stretch, and *where along it* a crowd forms is decided by the pathogens,
> not by the lane grid.
>
> ### Step 2 — Ingress is proximity-based: pressure gathers on a stretch of wall, then ruptures it
>
> Crowding is measured **spatially**, over a stretch of wall about a cell
> and a half across — not per lane. Where enough adherents crowd one
> stretch, the barrier there comes under **pressure that visibly builds**:
> a blister swells and reddens over that stretch, and it fills faster the
> more pathogens are packed into it. Below a **minimum crowd** nothing
> accumulates at all: one or two organisms on an intact barrier sit there
> and are eventually swept away.
>
> When the pressure fills, the stretch **ruptures, and every pathogen
> crowding it enters the tissue at once**, spread across the lanes behind
> the stretch they had gathered on.
>
> **This is the shape that makes the mechanic good and it must be
> preserved even if the numbers move:** pressure at one spot **builds
> visibly** and then **bursts**. The player can see a dangerous spot
> forming before it breaks, which is what makes defending it a decision
> rather than a reaction. **A trickle of independent single invasions
> loses that entirely** — ingress is something a crowd does, never
> something an individual pathogen does.
>
> **A rupture also kills the epithelial cells it tore through**, leaving a
> hole that behaves asymmetrically: bacteria can squeeze through it, one
> at a time, while it lasts; a virus cannot use it at all, because a virus
> needs a living cell to infect (§1c, §4b). The hole is repaired by the
> epithelium's own renewal, fastest near a crypt (§6).

Four other sections gain text, none of which alters a Director-set
decision — each fills in a mechanic the section already promised:

- **§1c** gains one paragraph recording the deliberate separation of
  `BarrierState` from `HostState` (§6.2 above).
- **§4a (pathogen classes)** gains the **in-lumen replication** rule
  (§4a above): bacteria divide in the lumen, damped by local density;
  viruses do not, for the same reason §4b gives them a free-virion death
  clock. This is the one genuinely *new* mechanic in the sprint and it is
  the Director's own addition (2026-10-18).
- **§5**'s IgA row gains its mechanic — exclusion by agglutination, plus
  slower / pacified / easier to kill — and `KnowledgeLadder`'s IgA rung
  flips `Built = true`, the first of the six to do so.
- **§6b**'s "Mucus upgrade — turnover, not thickness" gains the spatial
  slough event (§5 above), and §6b's "unmanaged buildup creates future
  pressure" stops being a promise: with replication, an unmanaged barrier
  population *grows*.

### 8.2 API changes

| Today | Proposed | Notes |
|---|---|---|
| `class GutInterface` | `class GutWall` (same file role, same owner, new name) | the thing it models is no longer an array of interface *positions*. Renaming forces every call site to be re-read, which is what `AGENT_HANDBOOK.md`'s "when a verb changes, grep every harness" lesson asks for |
| `new GutInterface(board, grid, tally)` | **unchanged signature** | so `RoundVerification`, `TissueVerification` and `Sprint19Verification` keep compiling after a rename |
| `PositionCount`, `AdheredCountAt(int)`, `AdheredAt(int)`, `PeakAdhered`, `BreachChanceAt(int)` | `SegmentCount`, `CrowdAt(float s)`, `PlaqueCount`, `Plaques[i]` (centre, charge, members), `PeakCharge` | `TotalAdhered` stays |
| `Adhere(int position, agent)` | `Join(float s, agent)` | `tally.Adhesions++` preserved |
| `Breach(int position, float now)` | `Rupture(int plaqueIndex, float now)` plus `RuptureAt(float s, float now)` for a harness or a future "pop the abscess" ability | keeps the deterministic entry point the current code deliberately exposes |
| `event Breached(int position, int released)` | `event Ruptured(float centreS, int released, float footprintCells)` | `GutInterfaceRenderer` and the tally are the only subscribers |
| `Tick(dt, now)` → breached positions | `Tick(dt, now)` → ruptures this tick | same explicit-time contract |
| `PathogenAgent.InterfacePosition` (int lane) | `WallS`, `WallDepth` (floats) + `WallMode { Gripping, Loose, Coated, Squeezing, InBarrier }` | `InterfacePosition` has no production consumer today; `PathogenState.AtInterface` keeps its name and meaning |
| `InterfaceStackWorldPosition(position, stackIndex)` | **deleted** — this is the thing the Director asked to remove | replaced by `BoardConfig.WallToWorld(s, d)` |
| (no coat state) | `PathogenAgent.IgaCoated`, `IgaClumpId`, and the `WallMode` above | one bool, one short id; the clump itself is a pooled struct on `GutWall`, not an object per raft |
| (no division) | `PathogenAgent.divideAt` + a new optional trailing delegate `onDivideInLumen` on `Initialize`, served by a new `PathogenSpawner.RequestDivideInLumen(float s, float d, PathogenClass, float now)` | the sibling of the existing `RequestSpawnNear(CoarseCoord, PathogenClass, bool, float)` (`PathogenSpawner.cs:401`), which spawns into *tissue* and is the wrong hook for a lumen division. An **optional trailing parameter** keeps every existing harness call site compiling — the project's standing pattern |
| `InvasionTally` | gains `IgaExcluded`, `Sloughed`, `SqueezedThrough`, `LumenDivisions` | the strategy harness's per-run CSV (`SESSION_HANDOFF.md` §2a) wants all four |
| `InvasionTuning.PerPathogenBreachChance`, `BreachRollIntervalSeconds` | **deleted**, replaced by §1.6's table | both are referenced in `MapVerification` and must be removed from `ResetToDefaults` together |

**One compatibility requirement, easy to miss:** `PathogenAgent.CurrentCoarse`
must stay in sync with `(s, d)` while `AtInterface` — i.e. set to the
coarse cell containing the drawn position, as `AdhereToInterface` does
today. Three things read it in that state:
`PathogenSpawner.Tick`'s M-cell sampling loop (`PathogenSpawner.cs:244`,
`wall.TrySample(a.CurrentCoarse, …)`), `GutWallFeatures.IsOverDome`, and
the debug readout. Keeping it in sync means Sprint 19's Peyer's patch and
the M-cell upgrade keep working **with no changes at all**, which is worth
a line of code.

### 8.3 Harnesses: what breaks, and how each should be rewritten

**`MapVerification.cs` — the only harness with real breakage.**

| Where | Asserts | Fate |
|---|---|---|
| `RunBandLayout`, L162 | "Gut interface has one position per lane" (`PositionCount == CrossLength`) | **rewrite**: `SegmentCount == CrossLength · BarrierSegmentsPerLane`, and the wall's `s` range covers exactly `[0, CrossLength)` |
| `RunAdhesionProximity`, L272–330 | `AdhesionChanceAt` monotonic in depth; a depth-blind curve adheres more than a wall-only one; `tally.Adhesions` counts | **keeps passing** unchanged — §3.2 does not touch adhesion. Note L310–318 already pins `AdhesionChanceAtWall = 0.03` locally, which is the handbook's "pin a value you depend on" pattern; leave it |
| `RunLumenFlow`, L224–263 | flow, excretion, no-penalty transit | **keeps passing** |
| `RunBreachBurst`, L364–443 | the whole per-position model: `AdhereToInterface(position)`, `AdheredCountAt`, `PeakAdhered`, `BreachChanceAt(A) > BreachChanceAt(B)`, `Breach(position)` releases all N in one event, a breach at A does not release B | **rewritten wholesale** — see the replacement list below |
| `RunBaseDirectedAdvance`, `RunBaseReached`, `RunMirroredMap` | tissue-side behaviour | **keep passing** |

**`RoundVerification.cs`** — constructs a `GutInterface` (L141) and hands
it to the spawner (L155). It compiles after the rename and **keeps
passing**: L218's "every one of them was excreted (adhesion 0)" works
because adhesion 0 means nothing joins the mill, and the round-clear
condition is `spawner.BatchComplete` (the bolus leaving), never "the field
is empty" — so a persistent mill cannot hang a round.

**`TissueVerification.cs`** — six construction/plumbing sites (L97, L120,
L483, L549, L594, L676, L731). All compile after the rename; none asserts
anything about the wall. **Keeps passing.**

**`Sprint19Verification.cs`** — constructs a `GutInterface` (L116) and
tests the patch via `CoarseCoord`s (L168–200). **Keeps passing iff** §8.2's
`CurrentCoarse` requirement is honoured. Worth one *new* assertion
there: a pathogen milling over the dome is still sampled, which is the
actual regression risk.

**New `Sprint22Verification.RunAll`**, the replacement for
`RunBreachBurst` and the sprint's own harness. It should assert:

1. **Geometry.** `s ↔ world` round-trips; `s = 0` is the flow's entry end
   on a **mirrored** board too (build it with `ConfigureForTest`, as
   `RunMirroredMap` does — this is the file's whole reason for existing);
   `WallToWorld(s, 0)` equals `InterfaceWorldCenter(lane)` at lane centres.
2. **Crowding is spatial, not per-lane.** Six pathogens straddling a lane
   boundary form **one** plaque; six split 1.5 cells apart form **two**
   — the single assertion that proves the model changed.
3. **Pressure builds and is monotone.** With 5 members held fixed, charge
   after 10 s < charge after 20 s; charge rate at 10 members > at 5
   (the *direction* of the effect, not an absolute — the handbook's
   standing fix for knife-edge assertions).
4. **The floor holds.** Two members for 300 simulated seconds → no plaque
   charge, no rupture, nothing in tissue. **This is the anti-trickle
   regression test** and it should carry a comment saying so.
5. **The burst is still a burst.** A plaque of 8 ruptures once, releases
   all 8 in one call, fires one `Ruptured` event carrying 8, and bumps
   `tally.Breaches` by exactly 1.
6. **Locality.** A rupture at `s = 2` leaves a clump at `s = 6`
   untouched, and the released pathogens land in the lanes behind `s = 2`.
7. **The rupture kills barrier segments**, and those segments pass a
   bacterium (one at a time, `HoleTransitSeconds` apart) and **refuse a
   virion**. The asymmetry, asserted directly.
8. **A crypt in reach shortens `BarrierRegrowSeconds`** (direction, not an
   absolute) — this one will break the day `CryptRegrowthBonus` moves, so
   pin it locally.
9. **IgA.** Above 70% knowledge, coated pathogens leave the crowd field,
   a loaded plaque's charge **falls**, and nothing is killed
   (`tally.Excreted` + `IgaExcluded` accounts for all of them).
10. **Slough.** One wave at the hottest patch returns its pathogens to
    `Lumen` with the refractory set, and zeroes the plaque's charge.
11. **Replication.** A lone bacterium in the lumen divides on schedule; a
    virus **never** divides (assert the negative — it is the biology rule
    most likely to be broken by a later refactor); the same bacterium in a
    crowd of 10 divides measurably slower (direction, not an absolute);
    a coated parent's daughter is **born coated** and cross-links into its
    parent's raft within a second or two; and `LumenDivideMaxPopulation`
    holds the pool.
12. **Pooling.** Running 400 pathogens through the mill allocates no new
    `GameObject`s beyond the pool's high-water mark
    (`BootstrapSmoke`-style count).

Every group that overrides an `InvasionTuning` value must call
`ResetToDefaults()` afterwards — the handbook's leak rule, which has
already bitten this project three times.

**Also needs updating, non-harness:** `DebugReadoutView.cs:185` ("worst
wall position right now: {PeakAdhered}") → worst plaque's charge and
member count, plus the dead-segment count; and `PathogenSpawner.cs:470`'s
`AtInterface` tally line keeps working as-is.

---

---

## 9. Forks for the Director

Twelve real choices. Each is a place where a reasonable designer could
have gone the other way, or where the direction is genuinely ambiguous;
the recommendation is first, with why. **Not** in this list: the IgA /
crowding tension, which the Director resolved himself on 2026-10-18
(§4) — agglutination is settled direction, not a question.

### The core of the mechanic

**1. Pressure: a filling blister, or a hidden chance?**
Should a dangerous spot be a meter the player can watch fill, or a dice
roll that might go at any moment?
- **A) A filling blister, with a slightly random breaking point
  (recommended).** The wall swells over that stretch, you can see roughly
  how close it is, and it breaks somewhere in the last 15%. *Why:* it
  makes "pressure builds visibly, then bursts" literal rather than
  implied; it is the only version where stopping a breach is a visible
  success; and it is the version a test can prove.
- B) Today's dice roll, but spatial — a crowded stretch has a chance each
  second. More suspense, and a crowd can get lucky and hold for a minute;
  but the player can never tell a spot that is about to go from one that
  is not, which is most of what this rework is for.
- C) A blister with a fixed breaking point. Perfectly readable, but the
  player learns the exact timing and it stops being tense.

**2. Minimum infectious dose: can one pathogen ever get in on its own?**
- **A) No — it takes a crowd of three or more (recommended).** One or two
  organisms on an intact barrier sit there, nibble a little, and are
  eventually swept away. *Why:* it is the rule that forbids the steady
  trickle the direction has already rejected once; it makes a quiet wall
  genuinely quiet, so a gathering crowd *means* something; and it is real
  — an infectious dose is a threshold, not a certainty, and a famously
  species-specific one.
- B) Yes, very slowly (today's behaviour: a lone pathogen gets in after
  about 80 seconds). Keeps a slow background cost for ignoring the wall,
  but that background cost is exactly the trickle that blurs the "spot
  forming" read.
- C) Per-species doses: some bugs need 3, some need 15. The best biology
  (*Shigella* needs tens of organisms, *Salmonella* needs about a
  hundred thousand) and a great teaching hook — but it wants the
  pathogen roster that the next sprint is going to design, so it is
  better as a follow-up than as part of this rework.

**3. How much wall does a burst destroy?**
A rupture tears through epithelium, which is how holes get made (§6).
- **A) Scaled by how many came through (recommended):** one dead wall
  segment per two pathogens released, centred on the burst — a 4-pathogen
  burst leaves a nick, a 16-pathogen one leaves a crater. *Why:* the
  damage is proportional to the event, it echoes the burst flash already
  scaling with the released count, and it self-balances (small breaches
  do not perforate the wall).
- B) The whole stretch dies — about three cells of wall, every time. The
  most dramatic, and the clearest "the board remembers", but at round 8+
  the wall could be in holes more often than not.
- C) Only the single segment at the epicentre. Safest, but the hole is so
  small that the bacteria-pass / viruses-blocked mechanic rarely comes up.

### The barrier

**4. Can bacteria walk through a hole, or does a hole only weaken the wall?**
- **A) They walk through — one at a time, taking two seconds each,
  visibly queueing (recommended).** *Why:* it is what the direction
  literally asks for ("a dead barrier cell is a hole bacteria can use");
  the one-at-a-time rule makes a hole a bottleneck rather than a leak;
  and a line of bacteria squeezing through a gap is still "pressure at a
  spot", just in a different register.
- B) No direct passage: a hole simply makes that stretch rupture much
  faster. Strictly safer — it keeps every entry a burst — but it loses
  the asymmetry's best visual and makes a hole feel abstract.
- C) Passage, but only while a crowd is sitting there. A hybrid; the extra
  condition is hard to read on screen.

**5. Can a virus break in through a *living* wall cell?**
The direction says a dead cell blocks viruses because a virus needs a
living host. That implies the opposite route exists; this asks whether to
build it.
- **A) Yes: an adhered virus can infect a wall cell, hide in it for about
  four seconds, then kill it and drop into the tissue behind — leaving a
  hole the bacteria can then use (recommended).** *Why:* it is how
  norovirus and rotavirus actually get in (they infect enterocytes); it
  gives viruses a route that is not just "wait for the bacteria to make a
  crowd"; it makes a viral round *soften the wall* for the next bacterial
  one, which is both real and good drama; and it can be denied by
  sloughing or rupturing that cell first, which rewards attention.
- B) No: viruses only ever come in on a rupture. Simpler, and it makes a
  holed wall *absolutely* virus-proof, which is a cleaner lesson — but it
  makes viruses passengers of bacterial crowds.
- C) Yes, but the cell survives (the virus is passed through intact). Real
  — that is transcytosis — but then viruses never make holes and the
  interaction in A disappears.

**6. Can the player attack pathogens stuck on the wall?**
Today they cannot: the wall is outside the tissue the immune cells walk,
so a milling crowd is untouchable. With the mill, that crowd is on screen,
crowded and dangerous for 20–60 seconds, and a player will try to shoot
it.
- **A) No — keep the wall untouchable (recommended).** *Why:* it is what
  makes the wall a *barrier* rather than another battlefield; it is the
  reason mucus turnover and IgA exist at all (give the player a gun and
  both upgrades become pointless); and it would need multi-occupancy
  contact detection that nothing else in the game needs.
- B) Yes, at short range only: neutrophils in the last tissue row can
  reach into the mucus. Real (neutrophils really do cross into the lumen
  in inflamed gut) and satisfying — but it should then **damage the
  barrier**, because that is what that costs in the body, and it risks
  making the wall just more tissue.
- C) Later, as a purchasable "transmigration" upgrade. Keeps A's clean
  rule now and sells B as a choice later. The only reason not to pick
  this is that it defers a decision the mill makes urgent.

### The player's levers

**7. Does an unmanaged wall slowly clean itself?**
Proposed: everything stuck to the wall creeps downstream with the mucus
and falls off the far end after about five minutes, lurching in time with
each peristaltic squeeze.
- **A) Yes, slowly (recommended).** *Why:* mucus genuinely is a conveyor,
  it is free (the peristalsis function already exists and already drives
  the squeeze the player can see), and it is what makes the whole adhered
  population visibly move as one body instead of jittering. Five minutes
  is slow enough that §6b's "a standing population accumulates if you do
  not manage it" still holds.
- B) No: nothing leaves the wall except by rupture, slough or IgA. A
  purer reading of §6b, and it makes the two upgrades matter more — but
  the wall then only ever gets worse, and a bad early round is permanent.
- C) Only during rounds. It already works this way in effect, since the
  buy phase freezes the simulation.

**8. Mucus sloughing: aimed at the hot spot, on a timer, or aimed by the player?**
- **A) Automatic, at the worst spot (recommended).** Every N seconds a
  sheet of mucus peels off the most crowded stretch and takes everything
  with it. *Why:* the player bought it to solve a problem, so it should
  solve the problem; and it is accurate — mucus output genuinely goes up
  where the epithelium is irritated.
- B) Automatic, wandering down the wall on a schedule regardless of what
  is there. More honest about turnover being constant rather than clever,
  and it sometimes misses, which makes it feel weaker than it is.
- C) The player aims it: click a stretch of wall to shed it, on a
  cooldown. The most agency and the most fun, and it turns a passive
  upgrade into an ability — but it is the only thing in this spec that
  needs a new input mode and new UI, and it makes the wall a
  click-to-win surface if the cooldown is wrong.

### Downstream of the Director's IgA ruling

**9. How much slower is a bound pathogen?** ("much slower", ruling 2)
- **A) About a third of its speed (recommended).** A coated bacterium in
  tissue takes ~3 seconds per cell instead of 1. *Why:* visibly labouring
  but still advancing, so a front of them can be held by far fewer cells
  without the threat disappearing.
- B) Immobile — a bound pathogen stops dead. Maximum payoff, and it makes
  the rung feel enormous; but a frozen pathogen is a decoration, and the
  "it is still a pathogen, go and clear it" tension is lost.
- C) Two thirds speed. Safer, but at that level the player will not
  notice it at all, and an unnoticed reward is a wasted one.

**10. "Easier for immune cells to deal with" — by what mechanism?** (ruling 2)
- **A) It takes more damage per hit — about 1.6× (recommended).** *Why:*
  one line of code, no new state, and the read on screen is immediate (a
  coated bacterium dies in two contacts instead of three).
- B) Immune cells hit it more often (a contact-rate bonus). Closer to
  what antibody actually does, but it overlaps with the 45% Fc-receptor
  rung, whose whole job is antibody-guided targeting — the rungs should
  stay mechanically distinct.
- C) It has less effective health. Identical in play to A but worse to
  read, because the health bar would lie.

**11. "Pacified" — reduced virulence, or none at all?** (ruling 3)
- **A) Reduced to a quarter (recommended):** a coated bacterium still
  grazes the wall, still damages host cells, still invades cells, all at
  a quarter rate. *Why:* it is still a pathogen and still has to be
  cleared, which keeps the rung a strong reward rather than a win button;
  and "pacified" reads better as declawed than as inert.
- B) Zero: a coated bacterium does no damage of any kind. The cleanest
  version of the Director's word, and the strongest possible reward — but
  combined with exclusion and slowing it may make a known species a
  non-event, and a non-event is not fun to watch either.
- C) Zero wall damage, reduced tissue damage. A middle path that matches
  the fiction best (bound organisms are held off the epithelium, so of
  course they cannot graze it) at the cost of one more rule to remember.

**12. How hard does crowding damp replication?** ("some bacteria won't
replicate fast when they're close to other members of their group")
- **A) A smooth brake that bites hard (recommended):** at a loose density
  bacteria double in ~20 s; in a tight crowd that stretches past two
  minutes. *Why:* growth then happens where it is harmless and stalls
  where it is dangerous, which keeps replication and the burst mechanic
  from compounding into an exponential and preserves the "you can see it
  coming" window.
- B) A hard stop above a threshold: below it they divide at full rate,
  above it not at all. Easier to explain and to see, and it stands in for
  quorum sensing more literally — but the cliff means a patch flips
  between doubling and frozen with nothing in between.
- C) No damper: bacteria always divide at the same rate. The simplest, and
  wrong — it makes the wall's worst spot also its fastest-growing one,
  and late rounds become unwinnable by arithmetic rather than by decision.

---

## 10. Suggested implementation order

Each step compiles and is committable on its own, which is the standing
rule after three interrupted sprints (`AGENT_HANDBOOK.md`: "commit after
each scope item — even incomplete, even ugly").

1. **Geometry and the mill, nothing else.** `BoardConfig.WallToWorld`,
   `GutWall` with the `(s, d)` list, the crowd field, the drawn
   lazy-eight + creep + cohesion, `PathogenAgent`'s wall fields.
   **Keep the old per-lane breach running against the new positions** by
   bucketing `s` to a lane — so the game is playable and the only visible
   change is that the queue is gone. This is the step the Director can
   look at soonest, and it is the one his note was actually about.
2. **Plaques and the charge.** Seeding, exclusive membership, charge,
   rupture, the `Ruptured` event. Delete `PerPathogenBreachChance` and
   `BreachRollIntervalSeconds`. Rewrite `MapVerification.RunBreachBurst`
   as `Sprint22Verification` items 1–6 in the same commit — they are the
   proof the mechanic survived.
3. **The wall renderer.** Segments, the blister, the constricting rim, the
   scrape, the pre-burst tell. Check every new sprite magnified via
   `SpriteShapeDump` **before** spending a build.
4. **Barrier segments.** State, grazing damage, rupture death (per fork 3),
   healing, the crypt hook. Harness items 7–8.
5. **The two asymmetric routes.** Bacterial hole squeeze (fork 4), viral
   barrier infection (fork 5).
6. **Mucus sloughing.** The event, the wave, the shop bridge in
   `UiController` beside the existing crypt and M-cell lines.
7. **In-lumen replication.** The division timer, both dampers, the
   `RequestDivideInLumen` hook, the puff, the HUD delta. Harness item 11.
   Deliberately after sloughing, because turnover is the counter-play and
   should exist before the thing it counters.
8. **Secretory IgA.** Coating, cross-linking, bulk, the three debuffs,
   `Rung.Built = true`. Harness item 9. Last because it is the only piece
   gated behind 70% knowledge, so it is also the only piece that cannot
   be playtested without a debug hook — add one (a key that grants 70% of
   a species) while building it.
9. **Verify and ship.** `tools/unity.sh verify` (14 harnesses + the new
   one), a WebGL build, then the docs: `GAME_DESIGN.md` §1b/§1c/§4a/§5/§6b
   per §8.1, `ENGINE_STATUS.md`, `INTERFACE.md`, `UI_STYLE_GUIDE.md`'s
   sorting table, `CHANGELOG.md`, and the `BACKLOG.md` note that
   `EconomyTuning.BatchSizeForRound` now needs re-tuning against
   replication.

Steps 1–3 are the Director's original note. Steps 4–5 are the barrier
asymmetry. Steps 6–8 are the three levers, and they are the ones that
make the wall a place the player acts on rather than watches.

## 11. References

Viewed for this pass; **images were not downloaded into the repo** and
nothing was copied — procedural art is the project's rule
(`SPRITE_DESIGN.md` §4).

- Already catalogued in `docs/LUMEN_REFERENCE.md` §1 and reused here for
  the mucus, dome and commensal conventions: **S10**
  [B-cell activation and response in the gut / IgA secretion](https://commons.wikimedia.org/wiki/File:B-cell_activation_and_response_in_the_gut_shape_the_gut_microbiota_through_IgA_secretion.png)
  (Botía-Sánchez et al., CC BY 4.0) — the main precedent for drawing IgA
  in the lumen at all; **S12**
  [Potential areas of interaction between amyloid-producing bacteria and the gut](https://commons.wikimedia.org/wiki/File:Potential_areas_of_interaction_between_amyloid-producing_bacteria_and_the_gut.png)
  (Friedland & Chapman, CC BY 4.0) — bacteria drawn *inside* a
  translucent mucus band, which is exactly the mill's register; **S8**
  [crypt structure and cell lineage](https://commons.wikimedia.org/wiki/File:A_schematic_illustration_of_the_crypt%E2%80%99s_structure_and_cell_lineage_classification.png)
  (Du, Nie & Holmes, CC BY 4.0) — the crypt-renews-the-epithelium fact
  behind §6.6.
- The immunology in §4 and §4a (immune exclusion, dimeric IgA
  cross-linking, infectious-dose thresholds, density-limited growth) is
  being checked against the literature in parallel by a separate research
  pass writing `docs/IGA_REFERENCE.md`. **Expect the magnitudes in §4.2,
  §4.3, §4.4 and §4a.2 to be corrected there**; they are labelled
  tunables with stated reasoning precisely so that a correction changes a
  number and not this design.
- No new web sources were fetched for this spec.

---

## 12. Director's decisions (2026-10-18) — these supersede §11's forks

Put to the Director by the head session after the spec and
`docs/IGA_REFERENCE.md` were both in hand. **Where a decision contradicts
an earlier section, this section wins**; the earlier text is kept for its
reasoning, not its conclusion.

### 12.1 Decided by the Director

**D1 — IgA × replication: enchainment only, no rate brake.** (Fork 11/12.)
`IgaReplicationDamper` is **removed**. A coated bacterium divides at its
**full** rate; the daughter is **born cross-linked to its parent**. The
Director's original instruction was that replication be "hampered by IgA",
and in the observable sense it still is — the count of *free* bacteria does
not grow — but the mechanism is enchainment, not a growth-rate penalty.

This follows Moor 2017 ([DOI](https://doi.org/10.1038/nature22058)) and
Bansept 2019 ([DOI](https://doi.org/10.1371/journal.pcbi.1006986)): IgA
does not slow bacteria, it *exploits* their division. Links break over
time, so it is a race — divide faster than your links break and you build
your own cage. See `IGA_REFERENCE.md` §1 claim 4 and §2C.

The property this buys, and the reason it beat the simpler option: **fast
replicators trap themselves hardest, with no targeting decision anywhere
in the system.** The player never chooses which pathogen to hit with IgA,
and neither does the simulated immune system — the physics does it. This
is Bansept's own stated conclusion about why real guts work this way.

**D2 — A dead barrier cell is a weak spot, not an open hole.** (Fork 4.)
The Director chose the *gentler* of the two options against the head's
recommendation. A dead wall segment **sharply raises local breach chance
but still requires a crowd to charge a plaque**. Bacteria do not walk
through unopposed.

Consequences, to be honoured in implementation: there is **one** ingress
mechanic, not two (everything still goes through the plaque/charge path),
the failure curve stays gentle, and §6's "a hole bacteria use" wording must
be rewritten as "a stretch where charging is much faster". The
virus asymmetry is **unaffected** and still holds: a dead segment offers a
virus no living cell to infect, so it remains dead ground to viruses
regardless of how fast bacteria charge there. Phrase it "nothing there to
infect", never "cannot cross" (`IGA_REFERENCE.md` §2H).

**D3 — The player can both attack a plaque and repair the wall.** (Fork 6.)
Towers may target a charging plaque to knock its charge down, and a
purchasable repair restores dead wall segments. A visible blister must be
something the player can answer, not only watch — this is the Director's
standing preference for visible, actionable feedback. Repair pricing must
not make wall damage trivial; tune so that repair-spam is affordable only
at the cost of the tower economy.

**D4 — Mucus sloughing is automatic at the hot spot, and the biology
settled it.** (Fork 8.) The Director declined all three offered options and
asked whether intestines really slough automatically in the presence of a
pathogen. **They do**, by a mechanism that fits this game unusually well:

> Birchenough, Nyström, Johansson & Hansson, "A sentinel goblet cell guards
> the colonic crypt by triggering Nlrp6-dependent Muc2 secretion", *Science*
> 352(6293):1535–42, 2016, [DOI](https://doi.org/10.1126/science.aaf7419).

A **sentinel goblet cell** at the crypt entrance non-specifically
endocytoses TLR2/1, TLR4 and TLR5 ligands; on detection it fires the Nlrp6
inflammasome via MyD88 and Nox/Duox ROS, triggering Ca²⁺-dependent
**compound exocytosis** of its whole Muc2 load, then signals through **gap
junctions** to neighbouring goblet cells in the upper crypt, which dump
theirs — and this **expels bacteria**. It guards against intruders "that
have penetrated the inner mucus layer".

Therefore, as built:
- **Trigger:** the existing crowd field crossing a threshold near a crypt —
  *not* a timer, and *not* player-aimed.
- **Footprint:** a patch centred on the detecting crypt and spreading to
  its neighbours, not a uniform sweep. The gap-junction spread is the
  footprint.
- **Specificity:** none. It needs no knowledge and works from round 1.
- **Effect:** expulsion, not damage. Caught pathogens join the downstream
  creep and leave. Nothing is killed — consistent with the compartment's
  "flush, not eat" rule (`IGA_REFERENCE.md` §4.1).
- **Home on the board:** the crypts Sprint 19 already built
  (`Grid/GutWallFeatures`). The visual is a crypt voiding its mucus.

**Deferred, not dropped:** a *player-aimed* purge is the type-2
"weep-and-sweep" response (IL-4/5/13: mucus plus smooth-muscle
contractility, [DOI](https://doi.org/10.1111/imr.70036)), which is
**adaptive**. It belongs on the knowledge ladder as an earned upgrade, not
as the innate baseline. Queued in `BACKLOG.md`, out of Sprint 22 scope.

### 12.2 Set by the head session (stated assumptions, Director may override)

| # | Fork | Call | Why |
|---|---|---|---|
| 1 | Pressure: filling blister vs. hidden roll | **Filling blister**, as mocked | The Director's standing preference is visible, readable feedback; a hidden roll cannot be defended against, which was the whole complaint behind the rework |
| 2 | Minimum infectious dose (crowd floor) | **Keep it** — 2 members contribute nothing | Real, teachable (infectious dose is a genuine concept), and it stops a lone drifting pathogen from ever threatening the wall |
| 3 | How much wall a burst destroys | **The stretch it tore through**, per §3 | Already consistent with D2: dead wall is a weak spot, so a burst makes the next burst there likelier without making it free |
| 5 | Viruses breaking in through a *living* wall cell | **Yes** — viruses need living cells, so they ingress at healthy stretches | This is the asymmetry's other half and it makes viral and bacterial fronts want different wall conditions, which is the point |
| 7 | Does an unmanaged wall self-clean | **Yes, slowly** | D4's sentinel response is the fast local answer; a slow baseline regrowth stops a single bad round from being unrecoverable |
| 9 | How much slower a bound pathogen is | **Tangled, not braked** — cross-linked rafts move at the creep, coated singles at ×0.6 | `IGA_REFERENCE.md` claim 1: the literature has no per-cell swimming-speed penalty, and the real effect is entanglement. Tunable |
| 10 | "Easier to kill" mechanism | **Tissue-side only** — no luminal phagocytosis bonus | `IGA_REFERENCE.md` claim 2, the research pass's one outright correction: sIgA is non-inflammatory, does not fix complement, and the lumen has no patrolling phagocytes. Bonuses move behind the wall, which also makes the two compartments play differently |
| 12 | How hard crowding damps replication | **Keep `LumenCrowdDamperStrength = 0.6`**, but re-document the reason | The damper is right; the *mechanism* was wrong. Not quorum sensing (which governs virulence, not growth rate) but **nutrient competition between metabolically overlapping neighbours** — your own kind compete hardest. `IGA_REFERENCE.md` §2G |

### 12.3 Corrections to fold in from `IGA_REFERENCE.md`

- **Valency:** secretory IgA has **four** Fab arms (dimer), directly counted
  by AFM (Suzuki 2015). "Two-headed" is the Director's shorthand for two
  joined bodies and is fine in UI copy, but the sprite and any tooltip
  should imply four grabbing arms, not two.
- **No killing anywhere in the lumen.** IgA deals no damage; sloughing deals
  no damage. Both convert pathogens into a state the flow removes.
- **"Pacified" must not imply harmless or non-replicating** — a coated
  bacterium still divides (D1); it is less able to adhere and invade.
