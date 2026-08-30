# Engine Status

Rewritten at the end of every sprint, not just appended to. This version
reflects the state after **Sprint 15** (the compartment visual pass — the
lumen and base bands leave the per-cell grid and are drawn as an open
fluid channel and a bloodstream; the lymph node and bone marrow gain
interiors). Sprint 14 was the DC-pacing rework (the shuttle collapsed
from four states to two so a DC paces the tissue band its entire life);
Sprint 13 was the entity sprite / visual-identity pass
(procedurally-drawn shape sprites replace the flat white quad for every
entity); Sprint 12 was two playtest fixes (cytokine
sensing on-by-default + a buyable sharpen; a first DC patrol movement
fix); Sprint 11 was a framework pass (placeholder shop, knowledge ladder
as data, neighbour regrowth). Recent structural state is **Sprint 9**
(the reworked round model — a frozen buy phase, a persistent battlefield,
food-item delivery) and **Sprint 10** (DC patrol lane-repulsion). Sprint 0's
engine/platform decision section is preserved below since it is still
accurate. Earlier sprints' histories are in `docs/CHANGELOG.md`; this file
only carries forward what is still true.

**Sprints 3–5 were implemented by dispatched Code agents interrupted
mid-task**; the head session finished each. **Sprints 6–9 were done
entirely by the head session**, inline. Anything below that says
"verified" was verified from actual command output — see "Build status
(Sprint 9)" and `docs/TEAM_RETRO.md`.

**Sprint 9 reworked the round model** (§5d / §2 got dated updates) after
the Sprint 8 playtest: the buy phase now freezes the whole simulation, the
battlefield persists round to round, and each round is delivered by a
contaminated food item that transits the lumen. Difficulty numbers roughly
doubled — still placeholder. **Sprint 8's adaptive knowledge % still
unlocks nothing** (§5's threshold ladder is a candidate next sprint).

## Engine & platform decision

**Unity**, chosen 2026-08-18 over Godot and a web-canvas-first stack.
Rationale (full research in project chat history):

- Closest match to Bloons TD 6's own strategy (Steam-primary, web as public
  face, single codebase exporting to both).
- Full C# support in WebGL builds; mature first-party Steamworks
  integration.
- Unity's Runtime Fee controversy is resolved — canceled Sept 2024. Unity
  Personal (free) covers projects under $200k annual revenue/funding.

Tradeoff accepted knowingly: Unity's WebGL builds are heavier than Godot's
— acceptable given the above.

## Local dev environment

Unity Hub, the Unity Editor (`6000.5.8f1` at `C:\Program
Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe`), the Unity CLI, and an
activated Personal license are installed on the Director's machine. As of
`WORKFLOW.md`'s 2026-08-19 rewrite, this is where the head session and
dispatched Code agents run natively with real shell access — no device
bridge, no sandbox. **Still no interactive Editor GUI session used for
gameplay authoring** through Sprint 3 — everything is built via batchmode
CLI + code. See "Scene construction" below.

## Current state (post–Sprint 4)

### Scene

Still `Assets/Scenes/Sprint1.unity` — **not renamed this sprint**, a
deliberate scope call (see `docs/TEAM_RETRO.md`): the file's content is
just the single `GameBootstrap` object with default-value serialized
fields, all real gameplay is code-driven at runtime, so renaming it would
have been cosmetic-only and added a rebuild/re-verify cycle for zero
functional benefit. Flagging clearly here so nobody reads "Sprint1.unity"
as meaning the file is stale.

`Assets/Editor/SceneSetup.cs` and `BuildScript.cs` are unchanged from
Sprint 1 (still point at `Sprint1.unity`).

### Gameplay systems

Builds on Sprint 1's two-resolution lattice, cytokine field, and random/
biased-walk search. Sprint 2 additions, per `SPRINT_PLAN.md`'s three scope
items:

**1. Bone marrow placement (replaces Sprint 1's debug spawn).**
- **`Assets/Scripts/Units/BoneMarrowSlot.cs`** (new) — one clickable slot.
  Click detection is Unity's legacy `OnMouseDown` message via a
  `BoxCollider2D` + the main camera's physics raycast — no uGUI/
  EventSystem needed (still no `com.unity.ugui` package this project).
- **`Assets/Scripts/Units/BoneMarrowManager.cs`** (new) — owns
  `BoneMarrowSlotCount` (5, a judgment call — see `docs/TEAM_RETRO.md`)
  slots below the tissue board. An empty slot click opens an IMGUI
  two-button picker (Macrophage/Neutrophil); `PlaceTower(index, kind)` is
  free (no ATP cost, per `SPRINT_PLAN.md`) and starts a per-slot emission
  timer. `Tick(float deltaTime)` — not an implicit `Update()` reading
  `UnityEngine.Time` — is the real emission logic, matching the
  `TissueGrid`/`CytokineField`/`Chemotaxis` pattern of taking explicit time
  so a headless harness can drive it directly (see Verification below).
  `EmissionIntervalSeconds = 4f` is a judgment call, not a balanced number.
  A placed tower emits into tissue at a uniformly random column along the
  **blood-adjacent edge** (the deepest fine row, `board.FineRows - 1`) —
  rung-1 entry per `GAME_DESIGN.md` section 2a ("cells extravasate at
  random points along the vessel").
- **`GameBootstrap`** no longer spawns any units at startup. It builds the
  (empty) macrophage/neutrophil `PrefabPool`s and hands them to
  `BoneMarrowManager`, which is now the sole source of new units. Nothing
  is on the board until the player places at least one tower — an
  accepted, `GAME_DESIGN.md`-flagged consequence ("round 1 becomes
  buy-then-observe").

**2. Lymph node placeholder.**
- A labeled, reserved, non-functional compartment built directly in
  `GameBootstrap` (a background `SpriteRenderer` + a
  `Assets/Scripts/Rendering/CompartmentLabel.cs` text label), positioned to
  the right of the tissue board. No behavior — exactly what
  `SPRINT_PLAN.md` scoped ("visually reserved space... not a functional
  lymph node").
- **`CompartmentLabel.cs`** (new) — small reusable IMGUI label anchored to
  a world-space point via `Camera.main.WorldToScreenPoint`, used for both
  the lymph node's caption and the bone marrow strip's heading, to avoid
  writing near-identical `OnGUI` boilerplate twice.

**3. Pathogen classes + combat (`GAME_DESIGN.md` section 4a).**
- **`PathogenClass` enum** (new, in `PathogenAgent.cs`) —
  `IntracellularVirus`, `IntracellularBacterium`, `LargeBacterium`.
  Parasites (multi-slot footprint) remain out of scope, per
  `SPRINT_PLAN.md`. Assigned per adhesion event via weighted random
  (`VirusChance = 0.45`, `BacteriumChance = 0.25`, remainder
  `LargeBacterium` — a judgment call, see `docs/TEAM_RETRO.md`, weighted
  toward virus since spread is "the sprint's most important piece").
- **Rendering split** — `BoardRenderer.ShowsAsPathogenItself(PathogenAgent)`
  (new static, side-effect-free predicate, same extraction pattern as
  Sprint 1's `Chemotaxis.ChooseNextStep`) decides per-cell background
  color: `LargeBacterium` reads as itself (dark maroon); intracellular
  classes read as bare host tissue (pink). A `PathogenAgent`'s own small
  sprite is **disabled entirely** for intracellular classes (see "Notable
  bug found and fixed" below for why an earlier version that tinted it
  flat instead of hiding it was wrong) — the coarse-cell background
  (heat-tinted per the existing cytokine mechanism) is the *only*
  representation of an intracellular infection, exactly matching
  `GAME_DESIGN.md`'s "visible as the host cell, not itself, until sensed."
- **Combat** — `PathogenAgent.ReceiveDamage(float)` (new), called by
  `SearchUnit.CheckContact` every tick a unit's fine tile falls in an
  occupied coarse slot (`PathogenAgent.ContactDamagePerHit = 1f`, flat, per
  `SPRINT_PLAN.md`'s "keep damage numbers simple"). `MaxHealth` is 12 for
  both intracellular classes, 18 for `LargeBacterium` (judgment calls —
  see `docs/TEAM_RETRO.md`). Reaching zero calls `TissueGrid.ReleaseSlot`
  (unused since Sprint 1, now exercised) and returns the pathogen to its
  pool via the existing `onExit` callback path — the same mechanism
  Sprint 1's transit-and-exit case already used.
- **Viral spread** — `PathogenAgent.TickCombat(float currentTime)` (new,
  explicit-time like the rest of this sprint's testable surfaces) checks,
  once `IncubationSeconds` (15f, judgment call) have elapsed on an
  uncleared virus infection, whether to spread to a free adjacent coarse
  slot via `PathogenSpawner.RequestSpread(CoarseCoord, float)` (new,
  public — shuffles the four coarse von Neumann neighbours and spawns a
  child `PathogenAgent` directly into `Adhered` state via
  `PathogenAgent.InitializeAdheredDirect`, through the same
  `PrefabPool` as ordinary spawns). Retries every `SpreadRetryIntervalSeconds`
  (1f) if blocked, rather than giving up permanently. Bacterial
  intracellular infections and large bacteria never spread (virus-specific
  per `GAME_DESIGN.md`). Verified headlessly to chain across multiple
  generations (see Verification below) — this is what gives search speed
  its "compounding cost."

### Sprint 3 additions — unit lifecycle and population homeostasis

The problem Sprint 3 exists to fix: Sprint 2's towers emitted forever and
**no `SearchUnit` was ever despawned** — `PrefabPool.Release` was never
called for one, so there was no return path at all. Active unit count grew
without bound. Most of this sprint is building that path and then bounding
it. Full design rationale in `GAME_DESIGN.md` §6d.

**1. `Assets/Scripts/Units/UnitLifecycleTuning.cs` (new) — the
parameterization layer.** A plain reference type holding
`MaxActiveChildren`, `KillLimit`, `DegranulatesOnDepletion`,
`DegranulationBurstMultiplier`, `ContactRadiusFineTiles`. **Nothing here is
a `const`, deliberately** — the Director's ruling (2026-08-21) is that
progenitor upgrades will eventually sell "bump this tower's kill count," so
an upgrade must be a write to one tower's field and nothing else.
Per-*kind* defaults live on `UnitProfile` (serialized fields, not consts);
`BoneMarrowManager.PlaceTower` copies them into a fresh instance per tower
via `FromProfile`. Verified: two towers of the same kind get genuinely
independent instances, and upgrading one touches neither the other tower
nor the shared `UnitProfile` default.

**2. Per-tower population cap (`BoneMarrowManager`).** Each slot keeps a
`List<SearchUnit> Children` (a list, not a counter, so the HUD and harness
can reach them; bounded by the cap so the O(n) remove is trivially cheap)
and stops emitting at `MaxActiveChildren` (default 10) even when its timer
has elapsed. **The emission timer is clamped at the interval while blocked
rather than banking up** — that detail is what keeps the two caps genuinely
independent per §6d: a tower whose whole population dies at once refills at
one cell per 4s instead of dumping ~40s of owed emissions in one frame.
Verified explicitly (1 cell immediately after a mass-death event, still 1
at 3.875s, exactly 2 at 7.875s, back to cap only after ~40s).

**3. Depletion and the despawn path (`SearchUnit`).** New public,
harness-callable surface: `Kills`, `RegisterKill()`, `IsDepletionDue`,
`ResolveDepletionIfDue()`, `ResetForPool()`, `OwnerSlotIndex`, plus
`SimulationTick()`/`CheckContact()` which read no `UnityEngine.Time` — the
project's standing convention (`TissueGrid`/`CytokineField`/`Chemotaxis`)
that anything worth verifying takes time/state explicitly.
- **Neutrophil (`KillLimit` 5, `DegranulatesOnDepletion` true)** —
  degranulates: a `ContactDamagePerHit × DegranulationBurstMultiplier` (3×)
  burst to whatever occupies its own coarse slot, applied as ordinary
  combat damage, then despawns. A `depleting` guard stops a kill landed by
  the burst itself from recursing into a second depletion. Per
  `SPRINT_PLAN.md` item 3, bare host tissue takes nothing — there is no
  host-cell-health/fibrosis system yet, and that is the intended state.
- **Macrophage (`KillLimit` 20 — Director-confirmed 2026-08-21,
  `DegranulatesOnDepletion` false)** — quiet retirement, no collateral
  damage. Verified: an occupant at the retirement site is still at full 18
  HP afterwards.
- Both free their tower's population slot and return to the `PrefabPool`
  (verified inactive, not destroyed), with kill count cleared for reuse.
- **A unit holds a live reference to its tower's tuning** (Director,
  2026-08-21), so an upgrade applies instantly to that tower's
  already-fielded children as well as its future ones. Sprint 3 first
  shipped a value snapshot (future children only); the Director overruled
  it — an ATP purchase should change the board immediately. Isolation still
  holds: no cross-tower leakage, and the shared `UnitProfile` defaults are
  never mutated.

**4. Kill attribution (`PathogenAgent.ReceiveDamage(float, SearchUnit)`).**
Signature changed to carry the attacker. **Exactly one** unit is ever
credited — whoever's hit crosses zero; earlier hits credit nothing and
later same-tick hits no-op at the `State == Cleared` guard. Credit is
applied *before* `ClearFromCombat()`, since clearing can return the
pathogen to its pool. A `null` source stays legal and always will (viral
spread, collateral damage, harness fixtures) and simply means nobody is
credited.

**5. Contact detection: coarse slot → fine-tile proximity.** Resolves
`INTERFACE.md` open question 3. A unit now damages a pathogen only within
`ContactRadiusFineTiles` (default 2, **Chebyshev**) of the pathogen's own
stored fine coordinate, instead of anywhere in its 7×7 coarse slot.
Chebyshev over Manhattan because it matches the square footprints these
units already render as; the Manhattan diamond covers half as many tiles
and would have halved contact frequency again. **Deliberately a radius, not
an exact-tile test** — exact coincidence across 49 fine tiles would have
made a random-walking unit almost never connect, i.e. combat would stop
working. See "Contact-rate change" under Build status for the measured
cost, which is real and matters for balance.

**6. `Assets/Scripts/Rendering/DegranulationFlash.cs` (new, pooled).** A
0.45s pale-yellow expanding burst at the degranulation site — the visible
difference between the two depletion paths, so a depleting neutrophil reads
as an event rather than a unit silently vanishing. Pooled like everything
else per `GAME_DESIGN.md` §8.

**7. HUD (`HudOverlay`, `BoneMarrowManager.OnGUI`).** Total active units
shown against the theoretical ceiling, and each marrow slot label shows
"N/cap alive". Added so the Director can *watch* population stay bounded
rather than be asked to trust it — this sprint's headline claim is a
number, and now that number is on screen.

### Sprint 4 additions — Map 01 geometry and the invasion loop

Implements `GAME_DESIGN.md` §1a/§1b. This is the sprint that gave the game
a real map: three lateral bands with the threat axis running right-to-left,
replacing Sprints 1–3's single tissue-only board where pathogens adhered at
a uniformly random column.

**1. `BoardConfig` — an axis frame and data-driven bands.** `Rows` stopped
being a `const 5` and the `[Range(24,40)]` clamp on columns is gone; Map 01
is **100 × 40 coarse cells**, still 7×7 fine subdivision.

The important part is the **axis frame**, which is how
`SPRINT_PLAN.md` item 3's "advance toward the base, never leftward"
requirement is actually enforced:
- `ThreatAxis` (Horizontal/Vertical) and `BaseEnd` (Negative/Positive) are
  configuration.
- `AxisIndex(coord)` returns **distance from the base**, flipping
  internally when `BaseEnd` is Positive. Axis index 0 is always the
  outermost base cell whichever world side that is.
- `CoarseFromAxis`, `OffsetInAxisFrame`, `InAxisBounds`, `CrossIndex`,
  `CrossLength` complete the frame. `OffsetInAxisFrame(c, -1, 0)` is the
  *only* sanctioned way to step toward the base.
- Bands are cell counts (`BaseBandCells` 25, `LumenBandCells` 25,
  `TissueBandCells` derived), with `BandAtAxisIndex`/`BandOf` classifying,
  and named edges: `TissueBaseEdgeAxisIndex`, `TissueLumenEdgeAxisIndex`,
  `LumenNearWallAxisIndex`, `LumenDepthFromInterface`.
- Lumen flow direction is its own axis end (`FlowCrossStep`,
  `LumenEntryCrossIndex`, `IsExcretedCrossIndex`).
- `ConfigureForTest(...)` builds non-default geometry from code; it exists
  so `MapVerification` can prove the abstraction on a mirrored board.

**2. `GutInterface` (new) — the wall, and the burst.** One pile per lane
(`PositionCount == CrossLength`). `Adhere` adds to a position; `Remove`
handles a pathogen cleared while still on the wall; `AdheredCountAt`,
`AdheredAt`, `TotalAdhered`, `PeakAdhered` expose pressure.
- `BreachChanceAt(position)` is `1 - (1 - perPathogen)^n`, so **a position's
  breach odds rise with the pile on it** — pressure builds toward its own
  release rather than being memoryless.
- `Tick(deltaTime, currentTime)` advances a roll clock and rolls every
  *occupied* position when due. The clock is held at zero while the wall is
  clean so the first pathogen to adhere waits a full interval.
- `Breach(position, currentTime)` releases **every** pathogen at that
  position in one call — the mechanic `SPRINT_PLAN.md` item 6 insists on. A
  pathogen that cannot find a release slot stays on the wall rather than
  being dropped. Public, so a harness (or a future "pop the abscess"
  ability) can trip a position deterministically.
- A `Breached(position, count)` **event** rather than polling, because
  script execution order between `PathogenSpawner` (which ticks this) and a
  renderer's `Update()` is undefined; polling would silently drop about half
  the bursts, and an undrawn burst is the one thing item 6 says must be
  legible.

**3. `PathogenAgent` — largely rewritten.** States are now `Lumen`,
`AtInterface`, `InTissue`, `Cleared`.
- **Lumen:** enters at the upstream end of the flow at a random distance
  from the wall, steps along the cross axis every
  `LumenStepIntervalSeconds`, and is **excreted with no penalty** off the
  downstream end. Unattackable while flowing.
- **Adhesion:** after each step, one roll against
  `AdhesionChanceAt(depth)` = `AdhesionChanceAtWall * exp(-depth / falloff)`
  — a static, side-effect-free function, the same extraction pattern as
  `Chemotaxis.ChooseNextStep`. On success `AdhereToInterface(position)`
  **moves the pathogen to the wall**, per §1b step 1.
- **Tissue:** `StepTissue` picks among toward-base / two lateral / away
  candidates by weight (0.70 / 0.13 each / 0.04), consulting the axis frame
  only. Reaching a `Base`-band cell despawns the pathogen and increments
  `InvasionTally.ReachedBase`.
- `InitializeAdheredDirect` became `InitializeInTissueDirect` — "adhered"
  now means the gut wall, not tissue.

**4. `InvasionTuning` (new) — every invasion number in one place**, all
mutable statics with `ResetToDefaults()`: lumen/tissue step intervals,
`AdhesionChanceAtWall` 0.12, `AdhesionFalloffCells` 5, breach roll cadence
1s, `PerPathogenBreachChance` 0.012, release spread limits, advance weights.

**5. `InvasionTally` (new)** — `Adhesions`, `Breaches`,
`ReleasedIntoTissue`, `Excreted`, `ReachedBase`. Drives the HUD.

**6. Compartments moved into the base band**, and units now enter at
`TissueBaseEdgeAxisIndex` on a random lane — expressed in the axis frame, so
moving the base moves the entry line with it. Placement UX is unchanged.

**7. `CytokineField` is now allocation-free.** Sprints 1–3 allocated a fresh
`float[,]` plus a source `List` every recompute — 150 floats on the old
board, 4,000 on Map 01, ~2.5 allocations/second of steadily growing garbage
against `GAME_DESIGN.md` §8. Both buffers are owned and cleared in place.

**8. HUD** shows the band layout, pathogen counts by band, the invasion
tally including `REACHED BASE`, and a live frame-cost readout. A dimming
panel sits behind it because the base band is now underneath the text.

### Sprint 5 additions — host-cell states, debris, class-specific advance

Implements `GAME_DESIGN.md` §1c and §1b step 4. Full signature-level detail
is in `docs/INTERFACE.md` ("Occupancy state" and "Sprint 5 changes").

**1. `TissueGrid` is two independent layers per coarse position.** A host
layer (`enum HostState { Empty, Healthy, Infected, Dead }`, plus per-cell
health, debris amount, intracellular pathogen ref, infection start time)
and an occupant layer for extracellular pathogens. Sprints 1–4's
one-`PathogenAgent`-per-slot model is gone. The tissue band seeds full of
`Healthy`; base/lumen are permanently `Empty`. Cytokine secretion sources
are now `Infected` hosts **or** occupied occupant slots (earliest start
wins) — dropping the occupant sources would have deleted every
`LargeBacterium` from the field in a sprint told to keep the mechanism
working.

**2. Debris is terrain.** `KillHostCell` (the single chokepoint every death
funnels through) → `Dead` + `FullDebris` (`1f`). Debris blocks regrowth
(only `Empty` ground regrows). `TissueGrid.Tick(dt, now)` — driven by a new
three-line `TissueDriver` MonoBehaviour, **not** the pathogen spawner —
owns debris self-dissipation (`DebrisSelfDissipationSeconds` 60) and
regrowth (`HostRegenerationSeconds` 20). `TissueTuning` holds the numbers
(mutable statics, `ResetToDefaults()`).

**3. Efferocytosis.** `SearchUnit.CheckEfferocytosis(currentTime)` — a unit
with `EfferocytosisDebrisPerTick > 0` (a per-tower field on
`UnitProfile`/`UnitLifecycleTuning`, macrophage default `0.05` ≈ 2.5s per
full pile, neutrophil `0`) standing on a `Dead` cell eats one bite of
debris via `TissueGrid.ClearDebris`. Opportunistic — own coarse slot only.
A finished pile plays a calm blue-green `DegranulationFlash`
(`EfferocytosisColor`). `SearchUnit.SimulationTick()` grew a `currentTime`
parameter (only caller is `Update()`).

**4. Class-specific advance** (`PathogenAgent.StepInTissue` dispatches by
class):
- **Virus** — free particles step only onto `Healthy` hosts and die after
  `VirusFreeSurvivalSeconds` (6s) if homeless; intracellular ones are
  stationary (spread is `TickCombat`'s job). The **firebreak is emergent**
  from those two rules plus `PathogenSpawner.RequestSpread` now requiring
  `IsHealthyHost` — no firebreak check exists in the code.
- **Intracellular bacterium** — base-biased walk when exposed, enters a
  `Healthy` cell it stands on (`IntracellularEntryChance` 0.5), hides
  `IntracellularResidenceSeconds` (12s), lyses out killing the cell and
  keeps walking.
- **Large bacterium** — unchanged walk, grazes its host cell for
  `LargeBacteriumHostDamagePerStep` (2.5) each step.
- Rendering is now driven by `PathogenAgent.IsIntracellular`, not `Class`,
  so an intracellular bacterium out of a cell and a free virus particle
  are both drawn as themselves.

**5. Base-band layout** (`GameBootstrap.BuildLayout`) rebuilt as fractions
of the base band's world rect — the marrow strip and lymph backdrop were
still sized for 100×40 and spilled across the 25×10 board.

### Sprint 6 additions — the intracellular-infection rework

Implements `GAME_DESIGN.md` §4b. Signature-level detail is in
`docs/INTERFACE.md` ("Sprint 6 changes"). Replaces Sprint 5's placeholder
class-advance model.

**1. An intracellular infection is unreachable by ordinary innate damage.**
`TissueGrid.GetAttackableAt` returns the extracellular occupant only;
`PathogenAgent.ReceiveDamage` is a no-op while `IsIntracellular`. The
Sprint 2–5 "grind it down through the cell" path is gone.

**2. The contact stress-sense roll** (`SearchUnit.CheckStressSense`). An
immune cell in contact with an `Infected` cell rolls
`StressSenseChancePerTick` (a per-tower `UnitProfile`/`UnitLifecycleTuning`
field — macrophage `0.03`, neutrophil `0.02`) each tick; on success →
`KillHostCell`, a loud necrotic kill of the cell + all contents, nothing
released, credited to the sensing unit, with a 1.5× magenta
`DegranulationFlash` (`StressKillColor`). The not-yet-built γδ T / CTL / NK
sensors will carry a high value in the same field — that gap is the
innate↔adaptive bridge. `Degranulate` also `DamageHostCell`s its slot now,
so neutrophil collateral still reaches an infected cell.

**3. Intracellular bacterium, real model.** Extracellular: no death clock,
roams (`IntracellularEntryChance` 0.5 → 0.12), takes ordinary damage.
Inside: immune; replicates every
`IntracellularReplicationIntervalSeconds` (3s), draining
`IntracellularDrainPerReplication` (2.5) and growing `broodCount`. Drain-
death → `BurstBrood`: the cell dies loud, this bacterium survives as the
first of the brood, up to `IntracellularMaxBrood` (6) total via
`onSpawnNear`. Killed any other way first → no brood.
`BoardRenderer.InfectedColorFor` draws a bacterium-infected cell sickly
yellow-green vs. the viral violet.

**4. Virus budding + burn-out.** Per-spawn trait
(`VirusBuddingSpeciesChance` 0.5): a **budding** infection emits a free
virion every `VirusBuddingIntervalSeconds` (2.5s), a **chain** one hops
once. Free virions (`StepVirus`) roll `VirusEntryChancePerTick` (0.20) to
enter a `Healthy` current cell, else do a momentum-biased walk (`lastHeading`
3× weight) **restricted to `Healthy`, occupant-free cells** — which is the
firebreak, preserved through the rework. `TickCombat` also rolls a one-time
`VirusBurnoutChance` (0.30): a hit infection self-terminates
8–25s later (`BurnOut`) — its cell dies loud and it spills back out as a
free virion + one more.

**5. Callback generalised.** `PathogenAgent.onSpreadRequested`
(`Func<CoarseCoord,float,bool>`) → `onSpawnNear`
(`Func<CoarseCoord,PathogenClass,bool,float,bool>`), and
`PathogenSpawner.RequestSpread` → `RequestSpawnNear(source, pClass,
asFreeParticle, currentTime)`. One path for viral spread and the bacterial
brood.

### Sprint 7 additions — the ATP economy and the round loop

Design: `GAME_DESIGN.md` §5b (ATP income), §5d (round loop, new), §6c
(life pool), §2 (towers persist, emitted cells die at round end).
Signature-level detail in `docs/INTERFACE.md` ("Sprint 7 changes").

**1. `ImmunologyTD.Economy`.** `EconomyTuning` (every number, mutable
statics, `ResetToDefaults()`), `AtpWallet` (plain reference type —
`Balance` / `TrySpend` / `Grant` / `CanAfford`), and `EconomyHooks`
(`static PayForKill`, a one-line bridge from `SearchUnit.RegisterKill` to
the wallet so a kill can add ATP without threading a wallet ref through
the unit tree).

**2. `ImmunologyTD.Rounds.RoundController`** — a MonoBehaviour with an
explicit-time `Tick(dt)`. `RoundPhase { Building, Active, Defeat }`. Opens
in `Building` at round 0. `StartRound()` (Space / a HUD button) sizes the
batch, arms the spawner, → `Active`. Each `Active` tick charges new
`InvasionTally.ReachedBase` against the 100-life pool (0 → `Defeat`), and
on `spawner.BatchComplete` clears the round — grants the lump sum, regen a
life every `LifeRegenRounds`, `marrow.ClearFieldedUnits()`, → `Building`.

**3. `PathogenSpawner` stops free-running.** `BeginBatch(n)` / `EndBatch()`;
`BatchComplete` = the batch emitted **and** nothing in the lumen or tissue
(a gut-WALL pile is allowed to persist per §6b — without that exception a
single stuck adherer holds a round open for a minute). `LiveCount` /
`BatchTarget` / `BatchEmitted` exposed.

**4. Placement costs ATP.** `BoneMarrowManager.Initialize` gains a nullable
`wallet`; `PlaceTower` spends `PriceFor(kind)` first (null wallet → free,
the harness path); the IMGUI picker shows prices and greys out the
unaffordable. New `ClearFieldedUnits()` for the round boundary — despawns
every fielded child, the towers stay.

**5. HUD + bootstrap.** `HudOverlay` draws a top-right economy bar (ATP,
lives, round + phase, batch progress, buy-phase prompt + Start button,
GAME OVER). `GameBootstrap` wires the wallet, the kill hook, the
`RoundController`; the game opens in a buy phase with the spawner un-armed.

### Sprint 8 additions — the dendritic-cell shuttle and the antigen barcode

Design: `GAME_DESIGN.md` §5a (the DC shuttle loop), §5c (the 8-bit
barcode, pairing, turnover), §1c (debris is the antigen source), §5
(knowledge is a per-species %). Signature-level detail in
`docs/INTERFACE.md` ("Sprint 8 changes"). **Framework pass — a rising
knowledge % unlocks nothing yet; §5's threshold ladder is next.**

**1. `ImmunologyTD.Adaptive` (new namespace).**

- **`Antigen`** — the 8-bit barcode as a plain `byte`, side-effect-free
  like `Chemotaxis`: `RandomTag`, `HammingDistance` (manual 8-iter
  popcount of `a ^ b`), `IsMatch` (distance ≤
  `AdaptiveTuning.MatchMaxHammingDistance`), `ForClass`.
- **`KnowledgeLedger`** — `float[3]` keyed by `(int)PathogenClass` (the
  species key until a roster exists). `Get` / `Add` (clamped
  `0..KnowledgeMax`) / `Reset`, `Revision` for cheap HUD change-detection.
  Per run, constructed by `GameBootstrap`, passed by reference.
- **`AdaptiveTuning`** — mutable statics + `ResetToDefaults()`, every value
  placeholder: `MatchMaxHammingDistance` 2, `KnowledgePerMatch` 3,
  `KnowledgeMax` 100, per-class antigens (≥4 bits apart),
  `DcPresentationsPerCargo` 4, `DcDebrisSamplePerBite` 0.34,
  `DcFineTilesPerTick` 3 (Sprint 14, was 2), `DcLaneRepelStrength` 0.8,
  `DcLaneRepelAxisRange` 12, `DcPatrolSweepBias` 1.8 (Sprint 14, was 1.0;
  `DcAxisWalkBiasSharpness` removed Sprint 14),
  `LymphocyteLifespanSeconds` 20, `LymphocyteFineTilesPerTick` 2,
  `PairingSeconds` 1.5, `NodePairingContactFineTiles` 3,
  `NodeColocalisationSourceStrength` 18, `NodeLymphocyteSourceStrength` 6,
  DC/lymphocyte emission intervals and `MaxActiveChildren`.

**2. Debris carries an antigen (`TissueGrid`).** New per-cell
`PathogenClass? debrisAntigen`, set on death, cleared with the pile
(`BecomeEmpty`). `KillHostCell` / `DamageHostCell` gained an optional
trailing `PathogenClass? antigen`; when null and the cell has an
intracellular resident, the resident's `Class` is used — so the
stress-sense loud kill and neutrophil collateral record the right antigen
with no caller change. `BurstBrood` / `BurnOut` detach before killing, so
they pass `Class` explicitly. New read `GetDebrisAntigen(coord)`.

**3. `LymphNode` (plain class, like `TissueGrid`).** Its own 6×6
`BoardConfig` (42×42 fine) and a `CytokineField` — the **co-localisation
signal** of §5c step 4, recomputed each step from a fixed central source
plus every resident lymphocyte as a weak source, so a DC drifts toward
where the T cells actually are. Node movement runs the **exact
`Chemotaxis.ChooseNextStep` path** the tissue units use (the reuse the
Director asked for); the small grid keeps `strength / (1 + distance)`
steep, so Sprint 4's flat-at-scale finding doesn't bite here.
`NodeToWorld` maps node fine tiles into the lymph backdrop rect.
`Step(now)` recomputes the field, resolves + forms DC:helper-T pairings
(`INodeVisitor` interface stands in for `DendriticCell`), moves residents,
ages them out. The tick gate lives one level up in `AdaptiveDirector`.

**4. `Lymphocyte` (MonoBehaviour agent).** Born with a random 8-bit
`Tag`, wanders the node via `Chemotaxis` against the co-localisation
field, frozen while paired, aged out at `LymphocyteLifespanSeconds` — the
progenitor re-emits a fresh tag, which is §5c's barcode turnover. Tween in
`Update()`; `NodeTick` driven by `LymphNode.Step`.

**5. `DendriticCell` (MonoBehaviour agent, implements `INodeVisitor`).**
State machine `PatrolTissue → InNode` (Sprint 14 — was a four-state
`PatrolTissue → TravelToNode → InNode → ReturnToTissue`; see the Sprint
14 section below). The DC paces the tissue band its whole tissue life via
`RepelledPatrolStep` (fine-grained lane repulsion + a base↔lumen sweep);
on a `Dead` cell with an antigen an empty DC **samples** (picks up the
antigen, eats one `DcDebrisSamplePerBite` — so it competes with
efferocytosis, §1c) and its sweep heading pins toward the base. Reaching
the `Base` band with cargo it enters the node, wanders the co-localisation
gradient there; `OnPairingResolved` spends one presentation whether or not
it taught; at zero the cargo is spent and `LeaveNode` drops it back at the
tissue base edge **empty** (it does not die — the round trip is the cost;
§5a's open "dies or returns empty" resolved this way) to resume pacing.
`Update()` tween deliberately slides across the tissue↔node gap so "the DC
went to the node" reads.

**6. `AdaptiveDirector` (MonoBehaviour).** Owns the DC pool, the
lymphocyte pool and the `LymphNode`, runs the whole arena on one
simulated `Clock`: a single tick gate sub-steps `LymphNode.Step` and every
fielded DC's `SimulationTick`. `EmitDendriticCell` (tissue base edge,
random lane) / `EmitLymphocyte` (into the node) keyed by marrow slot;
`DendriticCellCount` / `LymphocyteCount` for the cap; `DespawnAllFielded`
for the round boundary.

**7. Bone-marrow integration.** `UnitKind` gains `DendriticCell`,
`HelperT`. `EconomyTuning` gains `DendriticCellPrice` 30 / `HelperTPrice`
25. `BoneMarrowManager` stays the slot / picker / cost / cap /
round-boundary authority; `Initialize` gained an optional trailing
`AdaptiveDirector` (null in the innate-only harnesses — placing an
adaptive kind is then refused). A placed adaptive slot's `Tuning` carries
only `MaxActiveChildren`; its interval is `IntervalFor(kind)`; its live
count is `GetActiveChildren(i)` over `AdaptiveChildren`. `Emit` branches
to `EmitAdaptive` → the director; `OnAdaptiveChildDespawned` drops the
tracking ref when an agent pools itself. `ClearFieldedUnits` calls
`adaptive.DespawnAllFielded()`. The picker shows four priced, grey-out
buttons (the last two only when a director is wired).

**8. HUD + bootstrap.** `HudOverlay.Bind` gained optional
`KnowledgeLedger` + `AdaptiveDirector`; new KNOWLEDGE line (per-species %
+ node population). `GameBootstrap` resets `AdaptiveTuning`, builds the
ledger, the `LymphNode` (world rect inset inside the lymph backdrop), the
two agent pools, and the `AdaptiveDirector`; the lymph-node label drops
"reserved — not functional yet".

### Sprint 9 additions — the reworked round model

Design: `GAME_DESIGN.md` §5d (reworked) / §2 (dated update). Full
signature detail in `docs/INTERFACE.md` ("Sprint 9 changes"). Framework
pass — difficulty numbers are placeholder.

**1. `ImmunologyTD.Rounds.RoundClock` (new static) + `RoundClockDriver`.**
The one authority on "is the simulation running." `bool Frozen` (opens
`true`), and `float Time` — a sim clock that only advances while not
frozen. `RoundController` sets `Frozen = false` on `StartRound`, `true`
on a round ending and on `Defeat`. Every `Update()`-driven system
early-returns while frozen and passes `RoundClock.Time` (not
`UnityEngine.Time.time`) into its `SimulationTick` / `Tick`, so infection
ramps, the gut breach clock, and burnout timers do **not** fast-forward
across a buy phase: `PathogenAgent`, `PathogenSpawner`, `SearchUnit`,
`TissueDriver`, `BoneMarrowManager`, `AdaptiveDirector`, `DendriticCell`,
`Lymphocyte`. `RoundClockDriver` (3 lines, added by `GameBootstrap`)
advances the clock. Harnesses never touch it — they drive every tick
explicitly and never run `Update()`.

**2. Persistent battlefield.** `RoundController` **no longer calls
`marrow.ClearFieldedUnits()`** at a round boundary — fielded immune cells
and loose pathogens persist into the frozen buy phase and the next round.
`RoundController.DespawnAllFieldedUnits()` is a new public passthrough
that keeps the method reachable for a future run-restart. A round ends
when its **batch is delivered** (see 3), not when the board is clear.

**3. `PathogenSpawner` — the contaminated food item.** New
`BeginRound(int count, RoundDefinition def)`: sets a food round, spawns
one food item at the lumen entry, and — in `Tick` via `AdvanceFood` —
crawls it along the flow over `InvasionTuning.FoodItemTransitSeconds`,
dropping the batch in `FoodItemBurstCount` evenly-spaced bursts at
wall-hugging lumen cells near its position (`SpawnFromFood`, class per
`def.RollClass()`). A food excreted off the downstream end
force-delivers any remaining cargo, then retires. `BatchComplete` under a
food round = **batch emitted AND `foodExited`** (the field is no longer
required clear); the old `BeginBatch(int)` path keeps its
emitted+lumen/tissue-clear rule for the harnesses. The food item is a
non-pooled single `GameObject` the spawner shows/hides/moves (dull ochre,
`sortingOrder` 22). `PathogenAgent.Initialize` gained optional
`lumenCellOverride` / `classOverride`.

**4. `ImmunologyTD.Rounds.RoundScript` (new static).**
`struct RoundDefinition { string Tagline; float VirusWeight /
BacteriumWeight / LargeBacteriumWeight; PathogenClass RollClass(); }`.
`RoundScript.ForRound(int)` — ~6 hand-written gut-themed rounds, then a
procedural "spoiled leftovers, day N" fallback. `RoundController` exposes
`string CurrentTagline`; `HudOverlay`'s round bar shows the tagline plus
"Time is frozen" / "a contaminated food item is delivering."

**5. Difficulty (all placeholder).** `EconomyTuning.BatchSizeBase` 8 →
**16**, `BatchSizeGrowthPerRound` 3 → **6**.
`InvasionTuning.AdhesionChanceAtWall` 0.12 → **0.30**. New
`FoodItemTransitSeconds` 30 / `FoodItemBurstCount` 4 /
`FoodItemWallHugDepth` 1. Economy untouched.

### Sprint 10 addition — DC patrol lane-repulsion

Design: `GAME_DESIGN.md` §5a note (Director, 2026-08-29). The Sprint 8 DC
patrolled on a plain random walk and clumped. Instead of debris homing
(the deferred `BACKLOG.md` option), a patrolling DC now biases its walk
**away from other fielded DCs along the cross (lane) axis only** — the
base↔lumen threat axis is left unbiased, so DCs sweep back and forth and
spread evenly across the lanes.

`DendriticCell.RepelledPatrolStep` replaces the `Chemotaxis.ChooseNextStep`
call in `TickPatrol`. It sums a cross-axis crowd gradient
(`sign(myCross − otherCross) / (1 + |Δcross|)`) over other non-InNode DCs
within `AdaptiveTuning.DcLaneRepelAxisRange` (12) coarse cells along the
threat axis, then softmax-weights the two cross-direction candidates by
`exp(DcLaneRepelStrength · dir · crowd)` (`DcLaneRepelStrength` 1.4;
threat-axis candidates stay weight 1). `AdaptiveDirector` hands each DC
its live `allDcs` list as the cohort (`DendriticCell.Initialize` gained an
optional `IReadOnlyList<DendriticCell> cohort`); `DendriticCell.DebugPlaceForTest`
is a new test seam.

### Sprint 11 addition — placeholder shop, knowledge ladder, inward regrowth

Design: `GAME_DESIGN.md` §1d (host-cell upgrades), §5 (ladder roster
confirmed), §6b (mucus turnover). Full signatures in `INTERFACE.md`
("Sprint 11 changes"). Framework pass — the shop and the ladder drive
nothing; only regrowth is a real change.

**1. `ImmunologyTD.Economy.ShopLedger` + `ShopTuning` + `ShopItem`.**
Per-run purchase ledger. `enum ShopItem { BarrierMucusTurnover,
HostDsRnaSensor, HostReducedViralEntry, HostBacterialResistance, Crypt }`.
`ShopLedger.TryBuy(item, wallet)` spends `ShopTuning.PriceFor(item, level)`
(`base · (1 + PriceGrowthPerLevel·level)`, `PriceGrowthPerLevel` 0.6) and
increments the level — **no side effect beyond the ledger + wallet**.
`LevelOf` / `Owns` / `NextPrice` / `CanBuy` / `Reset` / `Revision`.
`ShopTuning` is mutable statics + `ResetToDefaults()`, all placeholder.

**2. Per-tower progenitor upgrade (`BoneMarrowManager`).** Clicking a
**placed** slot (was a no-op) opens an upgrade panel. `Slot.UpgradeLevel`,
`bool UpgradeTower(int)` (spends `ShopTuning.ProgenitorUpgradePrice(level)`,
bumps the level), `int GetUpgradeLevel(int)`. The tower's
`UnitLifecycleTuning` is **not** touched — §6d's real-upgrade wiring
exists; this just doesn't call it. The slot label shows "+N".

**3. `ImmunologyTD.Adaptive.KnowledgeLadder` + `KnowledgeCapability`.**
The six §5 rungs as `readonly struct Rung { KnowledgeCapability; float
ThresholdPercent; string ShortName }`, an ordered `Rungs[]`,
`IsUnlocked(cap, pct)`, `UnlockedCount(pct)`. Thresholds 10/20/30/45/60/70,
still placeholder. Display-only.

**4. `HudOverlay`.** `Bind` gained an optional `ShopLedger`. The KNOWLEDGE
block is now per-species (`% [x]CTL [x]NeutAb [ ]MemT …`); a **SHOP panel**
(left side, `Building` phase only) lists the five `ShopItem` rows, priced,
grey-out when broke, wired to `ShopLedger.TryBuy`. Debug panel grew to
392px.

**5. Neighbour-accelerated regrowth (`TissueGrid` / `TissueTuning`) — the
one real change.** An `Empty` host-ground cell's regrow time is
`HostRegenerationSeconds / (1 + TissueTuning.NeighbourRegrowthBonus ·
healthyVonNeumannNeighbours)`, so tissue heals inward from its intact
edges. `NeighbourRegrowthBonus` 0.5 (0 restores the old per-cell clock).
Measured: a cell ringed by 4 healthy neighbours regrows in ~6.8s vs the
20s base. `TissueVerification`'s regrow sub-test pins the bonus to 0.

### Sprint 12 additions — cytokine sensing on-by-default + buyable; the DC movement fix

Design: `GAME_DESIGN.md` §9 note (cytokine), §5a note (DC patrol).
Signatures in `INTERFACE.md` ("Sprint 12 changes").

**1. Cytokine sensing.** `CytokineToggle.Enabled` now defaults **true**
(the `C` key is a debug OFF-toggle). `Chemotaxis` gained
`static int SensingUpgradeLevel` (0 = base), `static float
SensingUpgradePerLevel` (0.6), and `static float EffectiveSharpness =>
GradientSharpness * (1 + SensingUpgradeLevel * SensingUpgradePerLevel)`,
which `ChooseNextStep` now uses instead of `GradientSharpness` directly.
New `ShopItem.CytokineSensingUpgrade` (`ShopTuning.CytokineSensingUpgradeBasePrice`
35) — **the one shop item that is a real effect**: `HudOverlay.Update`
pushes `shop.LevelOf(CytokineSensingUpgrade)` into
`Chemotaxis.SensingUpgradeLevel` each frame (a one-liner bridge; the
`ShopLedger` stays a pure spend+level ledger).

**2. `BoardConfig.FineCrossIndex` / `FineAxisIndex`** — fine-tile
analogues of `CrossIndex` / `AxisIndex` (axis-frame correct, flipped for
a Positive base end). Movement code can now bias at fine granularity
instead of only at coarse-cell boundaries.

**3. `DendriticCell.RepelledPatrolStep` reworked.** The Sprint 10 version
compared *coarse* cross/axis indices, so a fine step changed the index
only ~1 time in 7 — the lane-repulsion fired ~1/7 of steps and there was
no threat-axis behaviour. Now:
- **lane repulsion** runs every step against other DCs' *fine* lane
  positions (distances in coarse-cell units); `DcLaneRepelStrength`
  lowered 1.4 → **0.8** (a gentler per-step push for the same visible
  spread);
- **a back-and-forth sweep** — threat-axis steps bias toward a
  `patrolHeading` (±1) that flips at `TissueLumenEdgeAxisIndex` /
  `TissueBaseEdgeAxisIndex`, so a DC paces the full band depth.
  `AdaptiveTuning.DcPatrolSweepBias` 1.0 (0 = plain random walk).
Measured: three DCs from one lane now share a lane on **2 of 250** ticks
(Sprint 10: 82) with mean pairwise lane spread **16.7** (of ~18 max); a
lone swept DC covers tissue axis **7..17** where a random walk covers
12..14.

### Sprint 13 additions — the sprite / visual-identity pass

Design: `docs/SPRITE_DESIGN.md` (written by a dispatched design agent — a
first for this project) + `docs/UI_STYLE_GUIDE.md` (rewritten to "what's
on screen now"). Signatures in `INTERFACE.md` ("Sprint 13 changes").

**1. `game/Assets/Scripts/Rendering/SpriteShapes.cs` (new, from the design
agent).** ~20 procedurally-drawn 64×64 shape sprites, generated once at
first access and cached in lazy statics (the `RuntimeSprites` pattern),
drawn **white with the silhouette in the alpha channel** so the existing
per-instance `SpriteRenderer.color` multiply still produces every hue and
state tint. Raster primitives: `FillDisc` / `FillRing` / `FillCapsule` /
`FillLobed` / `FillStar` / `FillRounded` + `InnerShade` / `RimShade` /
`Stipple` / `Multiply`, 4× supersampled coverage. Shapes: `Macrophage`
(amoeboid), `Neutrophil` (lobed nucleus), `DendriteStar` /
`DendriteStarLoaded`, `Lymphocyte`, `LargeBacterium` (rod), `Virion`
(dot), `FoodBolus`, `HostCell`, `HostCellInfectedViral` (opaque inclusion
disc) / `HostCellInfectedBacterial` (purulent stipple), `Debris`,
`EmptyPit`, `SlotNiche`, `EpithelialBar`, `MarrowRegion`, `LymphNodeBean`,
and the five flash silhouettes. `RuntimeSprites.SquareSprite` kept as the
fallback.

**2. Call-site swaps** — each an isolated `sr.sprite = SpriteShapes.X`
with **no** `sortingOrder` / `localScale`-magnitude / `color`-hook
change:
- `UnitProfile` gains `[NonSerialized] Sprite Shape`; `GameBootstrap.Awake`
  assigns `SpriteShapes.Macrophage` / `.Neutrophil`; `SearchUnit` reads
  `profile.Shape` (fallback to the square) + a subtle one-time
  per-instance random spin and ±8% non-uniform scale jitter.
- `PathogenAgent` — sprite + colour by `Class` (virion dot + the new
  `BoardRenderer.VirionColor` cold purple / bacterium rod), in
  `Initialize` and `ApplyRestColorForCurrentClass`. `sr.enabled =
  !IsIntracellular` untouched (§4a no-sprite rule).
- `PathogenSpawner` food item → `FoodBolus` + random rotation.
- `DendriticCell` → `DendriteStar` / `DendriteStarLoaded` (on `HasCargo`);
  `Lymphocyte` → `Lymphocyte`.
- `BoardRenderer.Refresh` picks the host-state sprite per cell (Healthy /
  viral-infected / bacterial-infected / Dead / Empty) alongside the
  colour it already computes; `GameBootstrap.BuildBoardVisual` seeds every
  cell as `HostCell`. New `CellJitter(col,row)` — a deterministic ±3%
  per-cell value multiplier for the histology mottle.
- `BoneMarrowManager` slot → `SlotNiche`; `GutInterfaceRenderer` bar →
  `EpithelialBar` (its `Refresh` scale/colour maths untouched);
  marrow / lymph backdrops → `MarrowRegion` / `LymphNodeBean`.

**3. `DegranulationFlash`.** `ShapeFor(color)` picks a silhouette + a
per-instance `durationSeconds` / `startScale` / `endScale` keyed off the
five `static readonly` burst colours, so **every `Play(...)` call site is
unchanged**. `const DurationSeconds` stays as the external reference; the
per-instance fields default to the old values. New `static int
MaxConcurrent` (24) + `static int active` — `Play` drops a request past
the cap (`GAME_DESIGN.md` §8).

**4. `HudOverlay`** — F9 fires all five flashes across the tissue band
for visual QA (no headless coverage of rendering).

### Notable bug found and fixed in Sprint 2: `PrefabPool` didn't initialize outside Play Mode

`PrefabPool.Awake()` builds the underlying `ObjectPool<GameObject>`. The
assumption (mine, going in) that Unity calls `Awake()` synchronously on
`AddComponent()` even outside Play Mode turned out to be wrong — it only
fires reliably once the player loop is actually running (Play Mode, or a
real build). A headless verification harness that `AddComponent`s a
`PrefabPool` and calls `Get()` directly hit this as a
`NullReferenceException`. Fixed by making pool construction lazy
(`EnsurePool()`, called from `Awake()` **and** from `Get()`/`Release()`) —
harmless in normal gameplay (`Awake()` already ran by the time anything
calls `Get()`), and it's what let `Assets/Editor/CombatVerification.cs`
drive the real `PathogenSpawner`/pool path headlessly. See
`docs/TEAM_RETRO.md` for the full story.

### Notable bug found and fixed in Sprint 2: camera under-zoom from a stale `Camera.aspect`

`GameBootstrap.BuildCamera` computes `orthographicSize` from
`Camera.aspect` at `Awake()` time (frame 0), before the actual runtime
window/render-target size has necessarily settled. In this sprint's first
real-build screenshot the right ~25% of the board (and the entire lymph
node compartment) was cropped out of frame. Fixed by refitting the camera
a second time one frame later via a coroutine
(`RefitCameraNextFrame`) — `FitCamera` is now a separate, reusable method
called once immediately (a reasonable frame-0 fallback) and again after
`yield return null`. **Turned out not to be the actual cause of the
cropping seen in the first screenshot** (see Build verification below —
that was a DPI-scaling mismatch in the screenshot *capture tooling*, not
the game), but the fix is real, cheap, and strictly more correct than
relying on a single frame-0 aspect read, so it's kept.

### Sprint 14 additions — the DC-pacing rework (four states → two)

Design: `GAME_DESIGN.md` §5a ("Fixed again — Sprint 14").
Signatures in `INTERFACE.md` ("Sprint 14 changes").

**Why.** Third pass at the playtest note "DCs don't oscillate, don't
spread into lanes." The Sprint 12 fix was correct but only lived in the
`PatrolTissue` state — and in a dense round (debris on every dead cell) a
DC picked up cargo within ~2 ticks, then spent the rest of its life in
`TravelToNode` / `InNode` / `ReturnToTissue`, none of which paced or
repelled. So the mechanic almost never ran where a player could see it.

**1. `DendriticCellState` → `{ PatrolTissue, InNode }`.** `TravelToNode`
and `ReturnToTissue` deleted, along with `TickTravel` / `TickReturn` /
`BiasedAxisStep` (the straight axis-frame dashes). `SimulationTick`'s
switch is two cases.

**2. `TickPatrol` runs the DC's whole tissue life.** It sets
`patrolHeading` *before* stepping: `-1` (toward the base) whenever
`HasCargo`, otherwise it oscillates — `+1` until the lumen-edge axis
index, `-1` until the base-edge axis index, flipping at each. Then it
takes `DcFineTilesPerTick` `RepelledPatrolStep`s (unchanged: fine-grained
cross-axis softmax repulsion vs. the other non-`InNode` DCs, plus the
`DcPatrolSweepBias * axisDir * patrolHeading` sweep term). Reaching the
`Base` band with cargo → `EnterNode`. Empty on a `Dead` cell with a
debris antigen → sample (as before), which just flips the heading toward
the base; no state change.

**3. `LeaveNode`** (was the `ReturnToTissue` transition) drops the DC at
a random lane on the tissue base edge, `HasCargo = false`,
`patrolHeading = 1`, `State = PatrolTissue`.

**4. `AdaptiveTuning`:** `DcFineTilesPerTick` 2 → 3, `DcPatrolSweepBias`
1.0 → 1.8 (both so a full base↔lumen lap reads in a ~30 s round);
`DcAxisWalkBiasSharpness` removed (it only fed the deleted dashes).

**5. `AdaptiveVerification`** (still 40, no new count): `RunShuttleEndToEnd`
asserts the DC is pacing again (`PatrolTissue && !HasCargo`) instead of
the removed `ReturnToTissue`; `DriveOneShuttle` seeds debris two tiles
off the base edge and drives until the DC has visited the node and come
back to an empty patrol. `RunDcLaneSpread` / `RunDcPatrolSweep` unchanged
and still green — repulsion A/B **16 co-lane ticks vs. 167**, mean spread
**15.4 vs. 4.1**; swept axis span **6..18** vs. **10..12** for a plain
walk.

### Sprint 15 additions — the compartment visual pass

Design: `docs/COMPARTMENT_DESIGN.md` (a dispatched design agent, like
Sprint 13's `SPRITE_DESIGN.md`) + `docs/UI_STYLE_GUIDE.md` (updated).
Signatures in `INTERFACE.md` ("Sprint 15 changes"). **Rendering-only — no
simulation, tuning, or harness surface changed.**

**1. `SpriteShapes` grew** by 3 alpha primitives (`AxisGradient`,
`EdgeGradient`, `RadialGradient` — O(n)/pixel, no `Coverage`
supersampling) and 9 accessors (`ChymeField`, `MucusBand`, `FlowMote`,
`PlasmaField`, `VesselWallBar`, `OrganHalo`, `Erythrocyte`, `BirthPuff`,
`NodeColocGlow`); `MarrowRegion` / `LymphNodeBean` / `EpithelialBar`
revised in place. `Prewarm()` covers them and is now **called** from
`GameBootstrap.Awake` (BACKLOG item) — ~31 rasters at one boot point.

**2. `LumenChannelRenderer` (new).** Draws the lumen band as an open
channel: a `ChymeField` quad + a `MucusBand` strip at the gut-wall seam +
a `PrefabPool` of ~40 `FlowMote` quads drifting along the flow. Flow and
cross directions are derived from the axis frame
(`LumenEntryCrossIndex` / `FlowCrossStep` / `CoarseFromAxis`), no
hardcoded world direction. `Update()` early-returns on `RoundClock.Frozen`;
a `Mathf.Sin` phase drives a ±6% cross-section squeeze + in-phase mote
speed (Option B; `peristalsisAmplitude = 0` → Option A).

**3. `BaseCompartmentRenderer` (new).** Draws the base band as
bloodstream: a `PlasmaField` quad (alpha lifts toward the wall), a
`VesselWallBar` strip at `TissueBaseEdgeAxisIndex`, a `PrefabPool` of ~24
`Erythrocyte` streamers drifting outer-edge → wall, a `BirthPuff` pool
(cap 12) fed by `BoneMarrowManager.OnCellEmitted`, and a breach-flash
pool (cap 6, `EffeBloom` tinted red) fed by `PathogenAgent.OnReachedBase`.
Drifting elements freeze with `RoundClock.Frozen`; an in-flight breach
flash finishes. Both hooks are cleared in `OnDestroy`.

**4. Two new cosmetic static hooks.** `BoneMarrowManager.OnCellEmitted`
(`Action<Vector3>`, fired in `Emit` / `EmitAdaptive` with the slot world
position) and `PathogenAgent.OnReachedBase` (`Action<Vector3>`, fired in
`ReachBase()` with the arrival cell centre). Null in harnesses,
process-global — same shape as `EconomyHooks.PayForKill`.

**5. `LymphNodeFieldRenderer` (new).** One `NodeColocGlow` quad whose
position tracks the value-weighted centroid of `LymphNode.Coloc` and
whose alpha rises with the field peak (ref = central source + 4×
per-lymphocyte source; `≤35%` alpha). Re-samples every 0.15 s; holds on
`RoundClock.Frozen`. Wired from `GameBootstrap.BuildAdaptiveDirector`.

**6. Base + lumen leave the per-cell grid.** `GameBootstrap.BuildBoardVisual`
only creates a `SpriteRenderer` for `BandOf == Tissue` cells now (the
`views` array stays full-size, non-tissue entries `null`);
`BoardRenderer.Refresh` skips null views. Host cells only ever exist in
the tissue band (`TissueGrid.IsHostGround`), so the host layer is
unaffected. **−110 always-resident renderers at 25×10** (the base+lumen
grid was 120 of 250 cells; +10 field/wall/halo/glow quads), and
`Refresh` stops touching 120 cells every 0.15 s. On the 100×40 Map 01
aspiration the delta is ≈ **−1,990**. This retires the scale note in
`BoardRenderer`'s class comment (open since Sprint 4).

**7. Backdrop layer + tint.** Marrow / lymph backdrops moved
`sortingOrder 1 → 2`; an `OrganHalo` quad sits at 1 behind each. Marrow
retinted `0.30,0.24,0.16 → 0.34,0.22,0.18` (red marrow).
`GutInterfaceRenderer.WallColor` nudged `0.55,0.47,0.40 → 0.50,0.46,0.37`
toward the mucus tint. No `GutInterface` maths touched.

**Not done (deferred to BACKLOG):** the food-bolus channel wake (would
need `LumenChannelRenderer` to hold a ref to the food GameObject for a
barely-visible effect); the 3×3 co-loc haze grid (single blob shipped).

## Build status (Sprint 13 / Sprint 14 / Sprint 15)

All run by the **head session**. Numbers copied from actual output.

**Sprint 15** is rendering-only: the ten harnesses re-run **green (410
total, 0 failed)**; `BuildScript.BuildWindows()` **Succeeded, 0 errors,
93,378,880 bytes**; headless launch **0 exceptions** — the ~31 procedural
rasters (incl. the 9 new compartment shapes) generate during
`GameBootstrap.Awake` via `Prewarm()` and the bootstrap completes clean,
both compartment renderers + the node-field renderer bind without
throwing. **Not verified: how it looks** — rendering has no headless
coverage, so the lumen reading as a flowing channel, the base reading as
blood with the organs seated in it, the peristalsis not strobing at the
speed-up control, the co-loc haze tracking the T-cell cluster, and every
palette choice are the Director's screenshot / playtest.

**Sprint 14** touched only the DC shuttle: the ten harnesses re-run
**green (Adaptive 40, 410 total, 0 failed)**; `BuildScript.BuildWindows()`
**Succeeded, 0 errors** (`Assembly-CSharp.dll` rebuilt); headless launch
**0 exceptions**, bootstrap diagnostic clean. How the pacing *looks* in
motion is the Director's screenshot.

**Sprint 13 is rendering-only** — the ten harnesses below all re-run
**green, 410 total**, untouched. `BuildScript.BuildWindows()` —
**Succeeded, 93,367,104 bytes, 0 errors**; headless launch **0
exceptions** (the new `SpriteShapes` procedural generation now runs
during `GameBootstrap.Awake` and the bootstrap completes clean). **Not
verified: how it looks** — rendering has no headless coverage, so the
four host states reading apart, agent legibility at ~14–22 px, the
distinct flash shapes, and the palette nudges are all the Director's
screenshot (F9 in a build previews the flashes). The scripted screenshot
tooling did not cooperate this sprint; the handoff is the build.

### Headless verification

| Harness | Result |
|---|---|
| `Sprint12Verification.RunAll` (Sprint 12) | 9 passed, 0 failed |
| `Sprint11Verification.RunAll` (Sprint 11) | 26 passed, 0 failed |
| `RoundVerification.RunAll` (Sprint 9) | 29 passed, 0 failed |
| `AdaptiveVerification.RunAll` (Sprint 8, +3 Sprint 10, +3 Sprint 12) | **40 passed, 0 failed** |
| `EconomyVerification.RunAll` (Sprint 7) | 47 passed, 0 failed |
| `TissueVerification.RunAll` (Sprint 5, grown Sprint 6) | 73 passed, 0 failed |
| `MapVerification.RunAll` (Sprint 4) | 71 passed, 0 failed |
| `LifecycleVerification.RunAll` (Sprint 3) | 79 passed, 0 failed |
| `CombatVerification.RunAll` (Sprint 2) | 36 passed, 0 failed |

**410 assertions, 0 failed** (Sprint 12 added 9 in `Sprint12Verification`
— cytokine sensing default-on, `EffectiveSharpness` scaling with the
upgrade level, the `ShopLedger` tracking `CytokineSensingUpgrade`, and a
higher level biasing `ChooseNextStep` harder toward a source: 3997 vs
3041 of 4000 picks — plus 3 in `AdaptiveVerification` for the DC patrol
sweep), on a clean working tree after every commit.
`CytokineVerification` (the Sprint 1 tuning diagnostic, `RunComparison` /
sweep methods, no PASS/FAIL) still runs clean.

`RoundVerification` drives `RoundClock` (opens frozen; `Advance` is a
no-op while frozen, accumulates while running; re-freezing holds the
clock), `RoundScript` (scripted rounds 1–6 distinct; procedural fallback
names the round; a weighted mix respects its weights; all-zero doesn't
divide by zero), and the real `PathogenSpawner` / `RoundController`
food-round path: `StartRound` arms a food round + unfreezes + sets the
tagline; the food emits the whole batch as it travels; `BatchComplete`
stays false while the food is still in the lumen even after the last
burst; the round ends once the food exits; clear re-freezes + grants the
lump; and — with adhesion forced to 0.95 so the batch sticks — fielded
immune cells **and** loose pathogens both survive the round boundary,
with `DespawnAllFieldedUnits` still clearing the field for a restart. The
`Update()`-only freeze *gate* (every agent's `if (RoundClock.Frozen)
return`) can't run headlessly and is covered by the build launch.
`MapVerification` 4c was repinned to `AdhesionChanceAtWall` 0.03 (it
tests falloff shape, and both the new 0.30 default and the old 0.12
saturated a depth-blind channel at 400/400).

`AdaptiveVerification` drives the real `Antigen` math (popcount / Hamming
/ `IsMatch` boundary / threshold-0 exact-match), `KnowledgeLedger`
(per-species, clamped both ends, `Revision`, `Reset`), debris carrying an
antigen (`KillHostCell(coord, class)` → `GetDebrisAntigen` → cleared by
`ClearDebris`), a **full simulated shuttle** through the real
`AdaptiveDirector` / `LymphNode` / `DendriticCell` / `Lymphocyte` — seed a
tissue-wide debris pile of one species, emit one DC + one lymphocyte,
drive `Tick` until the DC returns; a **matching** pairing (threshold
forced to 8) raises exactly that species' knowledge by exactly
`KnowledgePerMatch` and nothing else's, a **non-matching** one (threshold
−1) still freezes + spends the cargo but teaches 0 — lymphocyte turnover
(a resident past its lifespan despawns, fires its callback, the progenitor
re-populates), and the round boundary (place a DC + a helper-T tower via
the real `BoneMarrowManager`, tick to emit, `ClearFieldedUnits` despawns
every fielded agent, both towers stay `Placed` and re-emit).

`EconomyVerification` drives the real `AtpWallet` (arithmetic, overspend,
non-positive edge cases), `PathogenSpawner` batch gating (un-armed emits
nothing; emits exactly the target; completes only when lumen+tissue are
clear; a wall pile is allowed to persist), the `RoundController` state
machine (Building → StartRound → Active → drive-to-clear → Building; lump
sum on clear; batch grows; `StartRound` a no-op outside Building), the
§6c life pool (breach → life; 0 → Defeat clamped; ticks after Defeat
inert; regen every N cleared rounds capped at Max), placement cost
(deduct / refuse when broke / null wallet = free), per-kill income through
the real `SearchUnit.RegisterKill`, and the round boundary despawning
fielded units while towers stay placed and re-emit.

`TissueVerification` now also covers §4b: the contact stress-sense roll
(`GetAttackableAt` returns null for a hidden resident; `ReceiveDamage`
no-ops on an intracellular pathogen; a macrophage with a real stress-sense
chance eventually loud-kills an `Infected` cell, credited, nothing
released; a 0-chance unit never does), the real intracellular bacterium
(damageable while exposed; immune + draining while inside; **no voluntary
exit**; drain-death bursts a brood; a loud kill mid-replication leaves no
brood), the **budding disk** (180s of budding puts established infections
on both sides of the seed and 0 base-ward of a 3-cell dead band), and
**burn-out** (a hit infection kills its own host cell loud and spills the
virus back out).

The firebreak is preserved by the free-virion walk being restricted to
`Healthy` occupant-free cells — asserted both at the rule level (chain) and
end-to-end (budding, 180s, dead band never crossed).

### Sprint 4 verification (unchanged, re-run green)

`MapVerification` covers band layout and boundaries, axis-frame
round-tripping, lumen flow and excretion, proximity-gated adhesion, the
breach burst, base-directed advance, and the reached-base event. Two groups
carry most of the weight:

- **The burst.** Six pathogens piled at one wall position; one `Breach`

### Sprint 4 verification (unchanged, re-run green)

`MapVerification` covers band layout and boundaries, axis-frame
round-tripping, lumen flow and excretion, proximity-gated adhesion, the
breach burst, base-directed advance, and the reached-base event. Two groups
carry most of the weight:

- **The burst.** Six pathogens piled at one wall position; one `Breach`
  call releases all six into the tissue band, empties that position, fires
  **one** event carrying the count, and leaves a second pile at another
  position completely untouched. Asserted directly because item 6 warns
  that degrading this into a trickle destroys the mechanic.
- **The mirrored map.** The same advance code runs on a board configured
  with the base on the opposite end. The pathogen still closes on the base
  in the axis frame, which *there* means its world column **increases**.
  That is the real test of item 3 — advance follows configuration, not a
  hardcoded direction.

Adhesion proximity is proven end-to-end rather than by re-deriving the
curve: cohorts of 400 pathogens run the full channel with a wall-only
falloff versus a depth-blind one, and the depth-blind curve adheres several
times as many. Closing the gate entirely (`AdhesionChanceAtWall = 0`) yields
exactly zero adhesions, proving the gate is actually wired into the
production path.

`CombatVerification` gained one assertion (35 → 36): its "units enter at the
blood-adjacent deepest fine row" check tested a contract item 8 retired, and
was rewritten in the axis frame as two checks.

### Cytokine sensing is much weaker at map scale — measured

| Board | OFF (0–1m / 1–2m / 2–2.5m) | ON |
|---|---|---|
| 30×5 (Sprints 1–3) | 2.99 / 3.14 / 2.84 | 0.20 / 0.00 / 0.00 |
| 100×40 (Map 01) | 46.93 / 46.83 / 47.05 | 45.29 / 40.42 / 37.38 |

**Not a regression.** The mechanism works — ON closes steadily while OFF
stays flat — but it no longer converges within the window. `CytokineField`
computes `strength / (1 + distance)` with no cutoff: a steep gradient across
the old board's ~3-cell separations, nearly flat across Map 01's ~47-cell
average. Sprint 1 called this mechanic "should feel transformative"; at map
scale, right now, it isn't. **Deliberately not tuned** — the Director's
standing instruction is mechanics first. Logged in `BACKLOG.md`.
**Sprint 5 note:** not re-measured — no cytokine code changed — and the
table above still stands.

### Windows build (Sprint 12)

`BuildScript.BuildWindows()` — **Succeeded, 93,354,304 bytes, 0 errors.**
Launched headlessly for ~20s: **0 exceptions / 0 errors** — the bootstrap
diagnostic prints clean (25×10 layout). The game opens in the **frozen**
buy phase, so a passive launch sits genuinely still — the round-model
code paths (freeze, food delivery, persistence, re-freeze) are covered by
`RoundVerification`, not this launch. The `Update()` freeze gate is
exercised here only in the sense that nothing moves.

### What was NOT verified

- **The shop / upgrade / ladder UI on screen.** `Sprint11Verification`
  drives `ShopLedger`, `UpgradeTower`, `KnowledgeLadder` and the regrowth
  directly; nobody has clicked the shop panel, opened a tower's upgrade
  panel, or watched a ladder rung tick on in a running build. Whether the
  panels fit / read well is a Director eyeball.
- **Whether "placeholder" reads as broken.** Every shop purchase and the
  progenitor upgrade take ATP and change a number and *nothing else*.
  That's intended (framework pass) but could feel like a bug to a
  playtester — flagged.
- **Nobody has played the Sprint 9 round rhythm.** A frozen buy phase, a
  contaminated food item transiting the lumen and dropping its bursts at
  the wall, a battlefield that persists into the next round, the tagline
  — carried forward, still unplayed. So is whether the doubled difficulty
  (16-batch round 1, adhesion 0.30) makes round 1 *engage* without
  overwhelming, and whether ATP now accumulates too fast with a
  persistent army.
- **Nobody has watched the Sprint 8 shuttle** either (DC sample → node →
  pair → KNOWLEDGE % moves), carried forward — and it now pauses in the
  frozen buy phase, which is new behaviour to eyeball.
- **Placement / clicking** through the running build's UI, same as every
  sprint since 3.
- **A `Time.time`-based timer's behaviour on the FIRST tick after
  unfreeze** — infection ramps and burnout windows read `RoundClock.Time`
  now so they don't fast-forward, but `GutInterface`'s roll clock sees a
  `currentTime` that jumped, so it rolls once immediately on unfreeze.
  One roll, not a flood; unverified in a build.
- Sprint 6's §4b visuals and Sprint 5's infected-cell colours — still
  unwatched, carried forward.
- **WebGL** not re-verified (unchanged since Sprint 1).

## Known issues

- **(New, Sprint 4) A stale serialized field silently deleted the
  playfield — fixed, but the class of bug is live.** `Sprint1.unity` still
  carried Sprint 1's `columns: 30`, which beat Map 01's `columns = 100`
  default, and because the outer bands clamp against axis length the whole
  shortfall hit the middle: 25 base + 5 lumen + **0 tissue**. It ran, drew,
  and threw nothing. `GameBootstrap.WarnOnDegenerateBands` now logs an
  error, but note **`MapVerification` cannot catch this class of bug** — it
  builds boards via `ConfigureForTest` and never loads the scene.
- **(New, Sprint 4) Cytokine sensing is much weaker at map scale.** See
  Build status. Measured, not tuned, per the mechanics-first instruction.
- **(New, Sprint 4) Frame cost is vsync-capped and therefore unmeasured.**
  8.35 ms is the refresh interval, not necessarily the cost.
- **(Sprint 4, partly addressed) The base band's marrow strip / lymph
  backdrop no longer overlap or spill** (Sprint 5 item 6 — `BuildLayout`
  rebuilt as fractions of the base band rect). The HUD dimming panel and
  its overprint against the marrow labels is **not** touched — still wants
  a real layout / Design-agent pass.
- **(New, Sprint 4) All invasion numbers are unvalidated defaults** —
  `InvasionTuning`'s adhesion chance/falloff, breach cadence and
  per-pathogen chance, release spread limits, and advance weights. Grouped
  in one file with `ResetToDefaults()` precisely so a tuning pass is cheap.
- **(Sprint 5–6) Every host/infection number is an unvalidated default.**
  `TissueTuning` (host health 10, regrowth 20s, self-dissipation 60s),
  `EfferocytosisDebrisPerTick` 0.05, and — Sprint 6 —
  `StressSenseChancePerTick` (mac 0.03 / neu 0.02) plus the eight
  `InvasionTuning` virus/bacterium knobs (`VirusEntryChancePerTick`,
  budding chance/interval, burn-out chance/window, replication
  interval/drain, brood cap; `IntracellularEntryChance` now 0.12). The
  **stress-sense chance** is the load-bearing one — it is the dial the
  innate↔adaptive bridge turns on. All mutable, all grouped.
- **(New, Sprint 6) Budding vs. chain is a per-spawn coin flip.**
  `VirusBuddingSpeciesChance` 0.5, rolled per agent — a budding
  infection's established children independently re-roll. Placeholder
  until a pathogen-species system exists (`BACKLOG.md`). Any harness test
  that watches a virus over time forces this and `VirusBurnoutChance` to
  0 or 1.
- **(New, Sprint 6) `GetAttackableAt` is occupant-only.** An intracellular
  infection is reached only by `SearchUnit.CheckStressSense` → `KillHostCell`,
  by the infection running its course, or (not built) by the stress-sensor
  / adaptive units. Do not restore the "damage the resident directly"
  path.
- **(New, Sprint 7) Every economy/round number is a placeholder.**
  `EconomyTuning` — prices, lump sum, per-kill, starting ATP, life pool,
  regen cadence, batch curve. Framework, not balance, on the Director's
  instruction. All mutable, all grouped.
- **(New, Sprint 7) Round-complete ignores the gut wall on purpose.**
  `PathogenSpawner.BatchComplete` doesn't wait on wall-pile pathogens
  (§6b — a pile persists round to round; otherwise one stuck adherer
  holds a round open for a minute). Revisit if the wall ever becomes
  attackable.
- **(New, Sprint 7) The kill payout is a process-global static hook**
  (`EconomyHooks.PayForKill`). Fine for one scene / one wallet; a second
  board would need an instance path.
- **(New, Sprint 7) `RoundController` only ticks state while `Active`, and
  snapshots its breach baseline in `Initialize`.** A run restart needs a
  fresh controller (or `InvasionTally.Reset()`), not a phase flip. A
  buy-phase timer or defeat animation has to be driven elsewhere.
- **(New, Sprint 7) The acute breach consequence (emergency
  granulopoiesis, §6c) is not built.** A breach is currently "just the
  counter", which §6c itself warns reads as a cushion. Flagged for the
  playtest.
- **(New, Sprint 8) Every adaptive number is a placeholder, and knowledge
  unlocks nothing.** `AdaptiveTuning` — match threshold, knowledge per
  match, per-class antigens, cargo capacity, debris-sample bite, pairing
  time, lymphocyte lifespan, node field strengths, emission cadence /
  caps. A rising KNOWLEDGE % is a HUD number only; §5's threshold ladder
  (MHC-I precise kill, neutralisation, complement, IgA, specific sensing)
  is the next sprint.
- **(New, Sprint 8) Species key = `PathogenClass` (3 values), not a
  roster.** Each class has one fixed antigen barcode in `AdaptiveTuning`.
  A real pathogen-species system (also needed for §4b's budding-vs-chain
  trait) makes knowledge key off species id and each species roll its own
  antigen.
- **(New, Sprint 8) The whole adaptive arena runs on `AdaptiveDirector`'s
  own `Clock`,** advanced one tick per sub-step in `Tick`, not aligned to
  the tissue board's `Time.time`. Fine for lifespan / pairing (they only
  need it internally consistent); a future feature that needs the two
  clocks in lockstep would have to thread one through.
- **(New, Sprint 8) `AdaptiveDirector` ticks the node every frame
  regardless of round phase** — the lymph node keeps milling during the
  buy phase and during `Defeat`. Cosmetic; a real pause/freeze would gate
  it on `RoundController.Phase`.
- **(New, Sprint 8) A spent DC returns to tissue empty, it does not die.**
  §5a's "does the DC die or return empty" is resolved as *return* for
  now; the travel time is the only cost of a spent cargo.
- **(New, Sprint 8) The DC:helper-T pairing is a timed freeze only.** No
  §5c "pair for a few *turns*" per-turn cost model beyond the single
  `PairingSeconds` window, and a helper-T's tag is not banked toward
  memory. B cells are not built (helper-T only).
- **(New, Sprint 8) Passive lymphatic drainage (§1c's third debris fate —
  a knowledge sink) is not built.** Unsampled debris still just
  self-dissipates; there is no "drains to the node and is deleted with
  nothing learned" path yet.
- **(New, Sprint 9) Every round-model number is a placeholder.**
  `EconomyTuning` batch curve (16/6), `InvasionTuning.AdhesionChanceAtWall`
  0.30, `FoodItemTransitSeconds` 30 / `FoodItemBurstCount` 4 /
  `FoodItemWallHugDepth` 1. Difficulty roughly doubled on the Director's
  say-so; not a balance pass.
- **(New, Sprint 9) The economy is un-retuned against a persistent army.**
  `RoundStartLumpSum` 80 and `AtpPerKill` 3 are unchanged, but the player
  no longer rebuilds each round, so ATP should accumulate faster than
  before. Deliberate — judge from the playtest.
- **(New, Sprint 9) `RoundClock` is a process-global static** (like
  `CytokineToggle` / `EconomyHooks`). Fine for one scene / one round loop;
  a second board would need an instance path. A harness that pokes it
  must `RoundClock.Reset()` afterwards.
- **(New, Sprint 9) `GutInterface`'s roll clock sees a time jump on
  unfreeze.** The spawner passes `RoundClock.Time` (which doesn't
  advance while frozen) but `GutInterface.Tick` compares `now -
  lastRollTime`; after a long buy phase that delta is large, so every
  occupied wall position rolls once on the first unfrozen tick. One roll,
  not a flood, but it means a breach can fire in the first frame of a
  round. Revisit if it reads badly.
- **(New, Sprint 9) The food item is not attackable.** A pure delivery
  vehicle — no health, no way to intercept it before it drops its cargo.
  Destructible food (interrupt the delivery) is a plausible later
  mechanic; the `FoodItem*` tuning and the single-`GameObject` visual are
  where it would attach.
- **(New, Sprint 9) A round ends when the food EXITS, not when the board
  is clear** (the board is never clear now). `PathogenSpawner.BatchComplete`
  branches on `foodRound`. If the food ever becomes destructible or the
  round model changes again, that branch is the place to look.
- **(New, Sprint 9) The buy phase / defeat freeze the adaptive arena and
  tissue healing too.** Intended ("freeze time"), but it means debris
  doesn't dissipate and cells don't regrow while you shop — a long buy
  phase is genuinely paused, not just spawn-paused.
- **(New, Sprint 11) Every shop purchase and the progenitor upgrade are
  placeholders.** `ShopLedger` levels and `Slot.UpgradeLevel` rise and
  spend ATP; **nothing reads them**. The intended mechanics are in
  `GAME_DESIGN.md` §1d / §5 / §6b. Prices (`ShopTuning`,
  `PriceGrowthPerLevel` 0.6) are placeholder.
- **(New, Sprint 11) The knowledge ladder unlocks nothing.**
  `KnowledgeLadder` is data + a HUD readout; crossing a threshold changes
  only the display. All six capabilities (CTL / NeutAb / MemT / FcR /
  Compl / IgA) are unbuilt. Thresholds are §5's placeholder values.
- **(New, Sprint 11) `ShopLedger` / `ShopTuning` are per-run / process
  statics** like the other ledgers/tunings — a harness that pokes
  `ShopTuning` must `ResetToDefaults()`. No `ShopLedger` on the round
  boundary or a run restart yet (it just persists for the run, which is
  correct).
- **(New, Sprint 11) Neighbour-accelerated regrowth uses the *current*
  healthy-neighbour count each sweep**, so a cell's effective regrow time
  changes as its neighbours die/regrow around it. Intended (it's why
  tissue "fills in"), but it means the regrow clock isn't a fixed
  countdown — a pocket that loses its healthy border mid-heal slows back
  down.
- **(New, Sprint 12) The cytokine-sensing upgrade is player-wide, not
  per-tower.** `Chemotaxis.SensingUpgradeLevel` is one global static
  affecting every unit at once (unlike the §6d per-tower
  `UnitLifecycleTuning`). Fine for a "you upgrade your immune system"
  framing; if it ever needs to be per-tower it moves onto
  `UnitLifecycleTuning` like the rest.
- **(New, Sprint 12) The `C` debug OFF-toggle still exists** and still
  flips *all* sensing off — a player who hits C by accident loses rung 2
  until they hit it again. It's kept deliberately (the rung-1-vs-2
  contrast is worth being able to show) but it's a player-reachable
  footgun.
- **(New, Sprint 12) `CytokineToggle.Enabled` defaults `true` as a field
  initializer**, and `Chemotaxis.SensingUpgradeLevel` as a mutable
  static — a harness that pokes either must restore it
  (`Sprint12Verification` does; `CytokineVerification` never touches
  `CytokineToggle`).
- **(New, Sprint 13) The sprites are unverified visually.** No headless
  check covers rendering; the procedural texture pixels aren't asserted.
  `SpriteShapes.cs` came from a dispatched agent and "compiles + launches
  clean" but the actual shapes / legibility / palette are the Director's
  screenshot. `docs/SPRITE_DESIGN.md` §6 also lists open questions the
  Director hasn't ruled on (per-cell mottle amount, flash timing values,
  whether the marrow slot should show the placed unit's shape).
- **(New, Sprint 13) `SpriteShapes` generation is one-time but not
  cheap** — ~20 textures × 64×64, several with an O(n²) rim pass and
  per-pixel closures, run lazily on first access. First touch is in
  `GameBootstrap.Awake` (macrophage/neutrophil) and the rest on first
  spawn of each entity — a small startup / first-of-kind hitch, not a
  per-frame cost. `SpriteShapes.Prewarm()` exists to move it all to a
  chosen point but isn't called.
- **(New, Sprint 13) `DegranulationFlash.ShapeFor` matches on the burst
  colour** (`Same()` within 0.001 per channel). If two events are ever
  given near-identical colours they'd get the same shape/timing — the
  five are currently far apart. A `Shape` enum param would be more
  robust; the colour match keeps every `Play(...)` call site unchanged.
- **(New, Sprint 13) `DegranulationFlash.active` is a process-global
  counter.** It only decrements when a flash runs to completion in
  `Update` — which they always do (flashes aren't despawned externally,
  and Sprint 9's freeze deliberately doesn't gate them). If that ever
  changes, `active` drifts and the cap gets conservative;
  `Mathf.Max(0, …)` floors it.
- **(Sprint 5) A 1-cell dead gap is hoppable** by a transient free virus
  particle; ≥2 cells / a full lane is a hard wall. Consistent with §1a's
  "slipping past one or two cells." `TissueVerification` uses a 3-cell
  band. (Sprint 6's budding front is subject to the same emergent rule and
  the same limitation.)
- Scene is still named `Sprint1.unity` — cosmetic only. It carries real
  serialized state that matters (`columns: 25`, the 5 partially-serialized
  `UnitProfile` fields — see `docs/TEAM_RETRO.md`).
- `Application.runInBackground` is unchecked (Unity default) — the game
  pauses when its window loses focus. Not a bug, but it means scripted
  input/screenshot verification needs the machine otherwise idle.
- `UnityEngine.UI` is still not installed — HUD, marrow picker and
  compartment labels are IMGUI.
- WebGL not re-verified (unchanged since Sprint 1).
- Sprint 3's contact-rate reduction (~50% of Sprint 2's) and all Sprint 3
  lifecycle numbers are unchanged and still unvalidated.
- `Chemotaxis.GradientSharpness = 4f` and the infection-ramp constants in
  `TissueGrid.cs` are unchanged from Sprint 1 — and `GradientSharpness` is
  now a candidate lever for the cytokine-range problem above.

## Addendum: what batchmode CLI can and can't do (still true)

No interactive Editor GUI access this sprint either. Everything built via
runtime code (`GameBootstrap`) or Editor scripts driven by
`-executeMethod` in batchmode. One correction to Sprint 1's own note on
this topic: **`MonoBehaviour.Awake()` is not guaranteed to fire just from
`AddComponent()` outside Play Mode** (see the `PrefabPool` bug above) —
Sprint 1's closing task got lucky that none of its dummy `GameObject`s
needed `Awake()`-driven initialization. Any future headless harness that
needs a component's `Awake()`-built state should either call an explicit
init method directly or use a lazy-init guard like `PrefabPool.EnsurePool()`
now does, not assume `AddComponent` alone is enough.
