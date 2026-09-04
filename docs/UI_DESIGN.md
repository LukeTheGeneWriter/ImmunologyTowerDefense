# UI Design Spec — the minimal HUD, the debug toggle, the first real screen

Status: **spec, 2026-08-30**, written by a dispatched Design agent for the
Sprint 16 buy-UI pass. Nothing here is built yet. Companion to
`docs/UI_STYLE_GUIDE.md` ("what's on screen now"), `docs/SPRITE_DESIGN.md`
and `docs/COMPARTMENT_DESIGN.md` (the board's visual language this UI has
to sit inside without clashing), and `docs/INTERFACE.md` (the
engine↔front-end contract — Sprint 11 shop section especially).

The Director's ask, verbatim:

> "Remove the status overlay on the top left corner — it has so many stats
> I can't get a feel for how the player will interact with the game."

So this pass does three things, in priority order:

1. **Replace the top-left debug dump with a minimal player HUD** — ATP,
   round, lives, and the one control the player needs (start the round).
   Everything else moves behind a toggle key, default OFF.
2. **Build the progenitor upgrade panel** — the first screen in the game
   that is a real designed surface rather than `GUI.Button` stacked in a
   corner. Click a *placed* marrow slot, get a panel that says what that
   progenitor is, what you can buy for it, and what each thing costs.
3. **Rule on the rest** — the buy-phase shop and the empty-slot tower
   picker: migrate now or later, and if now, in the same system.

Framework constraint carried through, non-negotiable (`GAME_DESIGN.md`
§6d, Director's Sprint 11 rule): **buying an upgrade spends ATP, bumps a
level, and changes nothing in the simulation.** The rows are real and
grounded — each names a plausible immunology mechanic tied to what that
unit actually does in the sim, and names the `UnitProfile` /
`UnitLifecycleTuning` / `AdaptiveTuning` field it would eventually
write — but the wiring is a later sprint. This spec must not make that
wiring harder; ideally it makes it a one-field change.

**UI framework: UI Toolkit, built from code.** No `.uxml`, no `.uss`
asset, no UI Builder. `PanelSettings` created at runtime, the tree
assembled in C# in the `GameBootstrap`-from-code style, styled with inline
`style.*` against a central palette. One caveat about a theme asset is in
§7; it is the only place "no asset files" bends, and it bends optionally.

---

## 1. Direction

**The chart clipped to the specimen, not a cockpit.**

The board is a stained section — desaturated eosin pinks, a violet bruise,
oxblood plasma, near-black where the sheet is gone (`SPRITE_DESIGN.md` §1,
`COMPARTMENT_DESIGN.md` §1). The mobile agents are the only saturated,
hard-edged things on it. **The UI must be quieter than the agents and
quieter than the tissue.** It reads like the printed margin of a
histology plate or the label taped to a slide: a few measured values in a
clinical hand, on a strip of smoked glass, deferring to the tissue behind
it. No glow, no bevels, no neon, no drop shadows, no rounded "game" pill
buttons with gradients. Hairline rules, one weight of type at two or three
sizes, flat translucent panels, corners barely radiused (3 px — enough to
not look like a `Debug.DrawLine` box, not enough to look playful).

Colour is used **only to carry meaning that has to be read at a glance**:
ATP is a dull marrow-gold, lives are a desaturated teal that turns oxblood
when low, an interactive element gets one clinical slate-blue accent line
and nothing else. Everything else is off-white ink and taupe dim-ink on
black glass. The guiding test: a screenshot of the game with the HUD in it
should still obviously be a screenshot of *a histology plate*, not of a
tower-defense HUD that happens to have cells behind it.

The debug readout is explicitly *not* held to this — it is an instrument
panel for the Director, monospaced and dense, and it is allowed to look
like one because it is off by default and never in a playtest screenshot
unless someone asked for it.

### Palette

Defined once, in code, as `static readonly Color` on a `UiTheme` class
(§7). Hex and 0–1 RGB both given so it can be checked against the
board palettes in `UI_STYLE_GUIDE.md`.

| Token | Hex | 0–1 RGB | Use |
|---|---|---|---|
| `PanelBg` | `#0F0E11` @ 0.86α | `0.059, 0.055, 0.067` | every panel's fill (over the `0.05,0.05,0.07` camera clear) |
| `Ink` | `#E9E5DA` | `0.914, 0.898, 0.855` | primary text; warm off-white, sits in the eosin/bone family |
| `InkDim` | `#8B857A` | `0.545, 0.522, 0.478` | labels, effect text, secondary numbers |
| `Rule` | `#E9E5DA` @ 0.14α | — | hairline dividers and panel borders (1 px) |
| `Atp` | `#CBB878` | `0.796, 0.722, 0.471` | the ATP value only. Dull marrow-gold — **greyer and darker than the neutrophil gold `0.93,0.74,0.30`** so the HUD number is never mistaken for a unit on the board |
| `LivesOk` | `#82A9A0` | `0.510, 0.663, 0.627` | the lives value at ≥ 25% of max. Desaturated teal — the "player holding ground" cool family, duller than helper-T teal `0.32,0.72,0.70` |
| `LivesLow` | `#C05C43` | `0.753, 0.361, 0.263` | the lives value below 25% of max; also the cost text on an unaffordable buy. Oxblood, **not** fire-red — pulled from base plasma `0.30,0.10,0.13` and the muted breach bloom, not the hot breach flash |
| `Accent` | `#6E8FB0` | `0.431, 0.561, 0.690` | the one interactive-affordance colour: a live buy button's left border, the Start-round button's border, the selected-slot rim. Macrophage blue `0.30,0.40,0.80` heavily desaturated toward slate |
| `AccentDim` | `#6E8FB0` @ 0.35α | — | a disabled/maxed control |
| `Defeat` | `#9A3B2C` | `0.604, 0.231, 0.173` | the GAME OVER word only |

Colour-blind check: the only colour that *must* be distinguished is
lives-ok vs. lives-low, and those separate by **value and hue-family**
(light cool teal vs. mid-dark warm oxblood) plus a shape change (§2). ATP
gold vs. lives teal are never adjacent in meaning. Everything load-bearing
is also carried by position and label text.

### Type scale

At a 1920×1080 reference resolution (`PanelSettings.referenceResolution`),
`ConstantPhysicalSize`-ish scaling (§7). System UI font; no custom face
shipped (a `.ttf` would be an asset file — out of scope, and the system
sans is already clinical enough).

| Role | px | weight | treatment |
|---|---|---|---|
| Stat numeral (ATP / lives / round value) | 26 | 600 | tabular figures if the platform font offers them |
| Stat unit label ("ATP", "ROUND", "LIVES") | 10 | 600 | uppercase, `letter-spacing: 0.12em`, `InkDim` |
| Panel title ("PROGENITOR NICHE · SLOT 2") | 12 | 600 | uppercase, `letter-spacing: 0.08em`, `InkDim` |
| Body (upgrade name, button label) | 13 | 500 | `Ink` |
| Fine print (effect text, round tagline, "6 / 10 fielded") | 11 | 400 | `InkDim` |
| Debug readout | 12 | 400 | **monospace** (`ui-monospace`, fallback Consolas/Menlo), `Ink` |

### Spacing

Base unit `s = 4 px`. Panel padding `3s` vertical / `4s` horizontal. Gap
between rows `2s`. Divider is a 1 px `Rule` line with `2s` margin above and
below. Panel border 1 px `Rule`. Panel corner radius 3 px. Panels dock to
the screen edge with a `3s` outset margin.

---

## 2. The minimal HUD

**Contents: ATP, round number, lives — and nothing else that is a
readout.** Plus the one thing that is not a readout but is the player's
only between-rounds action: the **Start Round** control, and the **GAME
OVER** state. The Director's complaint was about *stats*; the round
control is interaction, and removing it would leave the player with only
the `Space` key and no visible affordance. It stays, styled down.

That is the whole HUD. No population line, no pathogen-by-band line, no
invasion tally, no knowledge ladder, no frame-cost line, no cytokine
toggle state, no board-dimensions blurb — all of that is §3.

### Placement

**Top-right, one panel, docked with a `3s` outset.** Not top-left (that
corner is where the debug dump lived and the Director wants it gone; also
the base band and the marrow column are on the left and the HUD should not
sit over the player's own towers). Not top-centre (competes with the
food-item entering the lumen at the top). Top-right is already where the
Sprint 7 round bar lives, so the player's eye is trained there; this
shrinks that bar to its essentials.

Width ~300 px. It is the *only* always-visible UI. In `Building` it grows
downward to include the tagline and the Start button (~150 px tall); in
`Active` and `Defeat` it is shorter.

### ASCII mock

Buy phase (`Building`):

```
                                         ┌──────────────────────────────────┐
                                         │  ATP        ROUND       LIVES     │
                                         │  214          4          96      │
                                         │  ─────────────────────────────── │
                                         │  BUY PHASE · time is frozen      │
                                         │  “Undercooked eggs, Salmonella”  │
                                         │                                  │
                                         │  ┌────────────────────────────┐  │
                                         │  │      Start Round 4  ⏎       │  │   ← Accent left-border, else flat
                                         │  └────────────────────────────┘  │
                                         └──────────────────────────────────┘
```

Round running (`Active`):

```
                                         ┌──────────────────────────────────┐
                                         │  ATP        ROUND       LIVES     │
                                         │  201          4          94      │
                                         │  ─────────────────────────────── │
                                         │  ROUND IN PROGRESS               │
                                         │  batch 22 / 28 · 14 in play      │
                                         └──────────────────────────────────┘
```

Defeat:

```
                                         ┌──────────────────────────────────┐
                                         │  ATP        ROUND       LIVES     │
                                         │  201          9           0      │   ← LIVES in LivesLow, value shown as "0"
                                         │  ─────────────────────────────── │
                                         │  GAME OVER                       │   ← Defeat colour, 26px
                                         │  reached the base 100× over      │
                                         │  8 cleared round(s)              │
                                         └──────────────────────────────────┘
```

### How each value updates, and behaviour across phases

Everything is a poll in `UiController.Update()` (§7) — the sources are
plain refs with cheap getters and there are no events. Each `Label` only
has its `.text` (and sometimes colour) rewritten; no tree rebuilds.

| Value | Source | Notes across phases |
|---|---|---|
| **ATP** | `AtpWallet.Balance` | Always live. Ticks up on kills during `Active` (`EconomyHooks.PayForKill`), jumps by `RoundStartLumpSum` (80) at a round clear, drops on a buy. No animation beyond the number changing — a clinical readout doesn't juggle. |
| **Round** | `RoundController` | Shows `RoundNumber + 1` while `Building` (the round you're about to start), `RoundNumber` while `Active`/`Defeat` — same rule `HudOverlay.DrawRoundBar` uses today, clamped to ≥ 1. |
| **Lives** | `RoundController.Lives` / `.MaxLives` | Always live; decremented in `Active` as `InvasionTally.ReachedBase` rises. Renders as a **plain integer** (see §10 — pips are an open question; 100 pips is unreadable, so the recommendation is the number). Colour: `LivesOk` normally, `LivesLow` below 25% of `MaxLives`. When it changes downward, a single 150 ms flash to `LivesLow` then back (the only motion in the HUD — a breach *should* twitch your eye). |
| **Phase line** | `RoundController.Phase` | `Building` → "BUY PHASE · time is frozen". `Active` → "ROUND IN PROGRESS" + a second fine-print line "batch `BatchEmitted` / `BatchTarget` · `LiveCount` in play" from `PathogenSpawner`. `Defeat` → "GAME OVER" in `Defeat` colour at numeral size, plus the summary line. |
| **Tagline** | `RoundController.CurrentTagline` (or `RoundScript.ForRound(next).Tagline` while `Building`) | Fine print, italic, in quotes. Only shown in `Building` and `Active`. |
| **Start button** | calls `RoundController.StartRound()` | Only present in `Building`. `Space` still works (unchanged, `RoundController.Update`). Button is flat `PanelBg` with a 2 px `Accent` left border and `Ink` label; hover lifts the border to full `Accent` across the top too. On click it's gone next frame (phase flips to `Active`). |

Freeze awareness: the minimal HUD is the **only** surface that is up
during `Active`. It does not itself care about `RoundClock.Frozen` — it
reads `RoundController.Phase`, which is the authority the player sees. The
"time is frozen" text is just a label; the actual freeze is
`RoundClock`'s job.

---

## 3. The debug readout toggle

Everything currently in `HudOverlay.OnGUI`'s big top-left panel, moved
behind a key, **default OFF**.

### The key

**Recommendation: backtick `` ` `` (`KeyCode.BackQuote`).** It is the
universal "dev console / debug overlay" key, it is not bound to anything
in this project, and it is nowhere near the gameplay keys. `F3` is the
fallback if the Director wants a function key (Minecraft-style). **Not**
`F9` (that is the flash-preview, `UI_STYLE_GUIDE.md`), **not** `C` (that
is the cytokine debug toggle), **not** `Tab` or `Esc` (`Esc` dismisses the
upgrade panel — §4). Exact key is an open question for the Director (§10).

The key is read in `UiController.Update()`. It toggles a `bool
debugVisible` (starts `false`), which just sets `debugReadout.hidden` /
`.style.display`. The state is not persisted between runs — a fresh launch
is always clean.

`F9` (fire all five flashes) and `C` (cytokine sensing off) are unchanged
and keep working whether or not the readout is visible — they are engine
debug affordances, not part of this panel.

### What it shows

The exact set of lines `HudOverlay` builds today, verbatim in content
(the code moves, the strings don't):

- **`infoLine`** — board dims (`Columns × Rows`, band split, fine
  subdivision), macrophage/neutrophil fine-tiles-per-tick, and the
  "buy 4 progenitor kinds… SPACE starts the round" blurb.
- **`toggleLine`** — cytokine sensing ON (always) + the effective-sharpness
  multiplier `Chemotaxis.EffectiveSharpness / GradientSharpness`, or
  "OFF (debug) — press C to restore".
- **`heatmapLine`** — "Orange tint on host cells = cytokine field
  strength…".
- **`BuildPopulationLine()`** — active units / theoretical ceiling, tower
  count.
- **`BuildPathogenLine()`** — pathogens in lumen / adhered at wall / in
  tissue, worst wall position.
- **`BuildInvasionLine()`** — adhesions, breaches (released), excreted,
  REACHED BASE.
- **`BuildKnowledgeHeader()`** — "KNOWLEDGE per species…" + live lymph-node
  DC and helper-T counts.
- **`BuildLadderLine()` ×3** — virus / bacterium / large-bac: `%` + the six
  ladder rungs `[x]/[ ]` (`KnowledgeLadder.Rungs`).
- **`BuildPerformanceLine()`** — smoothed frame ms, fps, cells rendered.

Plus one line this spec adds: while the readout is on **and** the upgrade
panel is open, it echoes the selected slot's target-field names (§4) so
the Director can see which `UnitLifecycleTuning` field each row is destined
to write. Invisible in normal play.

### Form

**A corner panel, bottom-left, not a full-screen overlay.** Full-screen
dim would hide the board — but the board is exactly what you are debugging
*against* (does the front advance? do DCs pace the lanes?). ~440 px wide,
`max-height: 62%` of the screen with a `ScrollView` if it overflows,
`PanelBg` at a slightly higher opacity (0.90 — it's an instrument, it can
be more opaque), monospace 12 px, `Ink`. Bottom-left keeps it clear of the
minimal HUD (top-right), the shop / upgrade panel (right, §4/§6), and the
food item entering top-centre. It sits *over* the marrow column, which is
fine — when you're reading the debug panel you are not clicking towers.

---

## 4. The progenitor upgrade panel

The first real screen. Everything about it should feel like it belongs to
the same instrument family as the minimal HUD — same glass, same rules,
same two type sizes — just larger and with more in it.

### Trigger

Click a **placed** bone-marrow slot. The path already exists:
`BoneMarrowSlot.OnMouseDown` → `BoneMarrowManager.OnSlotClicked(i)`, which
today sets `pendingUpgradeIndex`. This spec replaces the two `int?`
pending fields with one public selection concept:

```
BoneMarrowManager:
    int? SelectedSlotIndex { get; }         // null = nothing selected
    void ClearSelection();
    // OnSlotClicked sets SelectedSlotIndex; the kind of panel shown
    // (picker vs. upgrade) is decided by GetSlotState(i).
```

`UiController` polls `SelectedSlotIndex` each frame. When it is non-null
and the slot is `Placed`, the **upgrade panel** is shown for that slot;
when it is non-null and `Empty`, the **tower picker** (§6) is shown in the
same place; when it is null, neither (the shop, §6, takes that space in
`Building`).

### Placement

**A fixed panel docked to the right edge, directly under the minimal
HUD** — *not* floating next to the clicked slot. Reasons, in order:

1. **Click occlusion.** The marrow column and the lymph node are in the
   base band on the **left**. A panel floating there — or a left-docked
   shop — sits on top of the very colliders `OnMouseDown` needs. Docking
   every panel to the right keeps the whole left half of the screen
   click-through to the physics raycaster. (This also decides §6: the shop
   moves to the right too.)
2. **A stable place to look.** The slots are small (`marrowSlotSize` is
   capped at 62% of the narrow base band's width) and stacked five high. A
   panel that jumps to a different Y for each slot is harder to use than
   one that is always in the same rectangle. The **selected-slot
   highlight** on the board is what ties the panel to a specific slot;
   physical proximity isn't needed once that rim is there.
3. **Room.** The right dock can be ~320 px wide and as tall as it needs;
   a slot-anchored popover on the left would collide with the board edge
   and the lymph node backdrop.

Floating-near-the-slot is the alternative if the Director wants the
tighter spatial coupling — it's an open question (§10). If chosen, it
would anchor with `RuntimePanelUtils.CameraTransformWorldToPanel` on the
slot's `WorldPosition` and clamp inside the safe area, and the shop would
still need to move off the left.

### Layout

```
                                         ┌──────────────────────────────────┐
                                         │  ATP 214   ROUND 4   LIVES 96    │   ← minimal HUD (always)
                                         └──────────────────────────────────┘
                                         ┌──────────────────────────────────┐
                                         │  PROGENITOR NICHE · SLOT 2       │
                                         │  ┌──────┐                        │
                                         │  │  ◍   │   MACROPHAGE           │   ← portrait = SpriteShapes.Macrophage,
                                         │  └──────┘   niche level ●●○○     │      tinted the kind colour
                                         │  6 / 10 cells fielded            │
                                         │  ──────────────────────────────  │
                                         │  Efferocytic capacity            │
                                         │  Clears debris markedly faster;  │
                                         │  frees ground to regrow.         │
                                         │  ●●○            42 ATP   [ BUY ] │   ← BUY: Accent border, enabled
                                         │  ──────────────────────────────  │
                                         │  Tissue residency (M2)           │
                                         │  +8 kills before the cell        │
                                         │  retires quietly.               │
                                         │  ●○○            40 ATP   [ BUY ] │
                                         │  ──────────────────────────────  │
                                         │  Inflammasome priming (NLRP3)    │
                                         │  Better odds of sensing an       │
                                         │  infected cell on contact.       │
                                         │  ○○             88 ATP   [  –  ] │   ← unaffordable: dim, 88 in LivesLow
                                         │  ──────────────────────────────  │
                                         │  Expanded niche output           │
                                         │  +2 to this niche's cell cap.    │
                                         │  ●●●●●         MAX               │   ← maxed: AccentDim, no button
                                         │  ──────────────────────────────  │
                                         │                        close  ✕  │
                                         └──────────────────────────────────┘
```

**Header.** Panel title ("PROGENITOR NICHE · SLOT `i`") in the 12 px
uppercase style. A portrait: a `VisualElement` whose
`style.backgroundImage` is the kind's `SpriteShapes` sprite
(`Macrophage` / `Neutrophil` / `DendriteStar` / `Lymphocyte`) and
`style.unityBackgroundImageTintColor` is the kind colour
(`BoneMarrowManager.ColorForKind`, exposed) — a real portrait for free,
no new art. Beside it: the kind name (13 px `Ink`) and **niche level
dots** — one dot per *purchased* upgrade across all rows on this slot,
filled `Accent`, empty `Rule`. Under the header, fine print:
"`GetActiveChildren(i)` / `GetTuning(i).MaxActiveChildren` cells fielded".

**Upgrade rows** (2–4, see §5 — recommendation is 3). Each row:

- **Name** — 13 px `Ink`. The immunology display name.
- **Effect text** — 11 px `InkDim`, up to two lines. What it does, in the
  player's terms, not the field name.
- **Level dots** — `●` per level bought, `○` per level remaining, capped
  at that row's max (§5). `Accent` / `Rule`.
- **Cost** — "`N` ATP", 11 px. `InkDim` when affordable, `LivesLow` when
  not.
- **Buy button** — 13 px label `[ BUY ]`.
  - *Affordable* — `PanelBg` fill, 2 px `Accent` left border, `Ink` label.
    Click → `BoneMarrowManager.UpgradeTower(slotIndex, rowIndex)` (§8).
  - *Can't afford* — button shows `–`, `AccentDim`, not clickable; the
    cost turns `LivesLow`. (`GUI.enabled`-equivalent: `SetEnabled(false)`.)
  - *Maxed* — no button, the word `MAX` in `AccentDim` where the button
    was; the dots are all filled.

**Footer.** A `close ✕` text button (clears `SelectedSlotIndex`). The ATP
balance is not repeated here — it's in the HUD directly above.

The target-field name (`EfferocytosisDebrisPerTick`, etc.) is **not**
shown to the player. It appears only in the debug readout (§3) while this
panel is open, so it is "ready to wire" and visible to the Director
on demand.

### Dismissal, and what happens on other actions

| Action | Result |
|---|---|
| Click `close ✕` | `ClearSelection()`; panel hidden; shop returns to the dock. |
| Press `Esc` | Same as close. |
| Click another placed slot | Selection re-targets; panel repopulates for the new slot; the board highlight moves. No close/reopen animation. |
| Click an empty slot | Selection re-targets; the **tower picker** (§6) replaces the upgrade panel in the dock. |
| Click empty board / a pathogen / anywhere not a slot and not a panel | `ClearSelection()` — the physics raycast misses every `BoneMarrowSlot`, `UiController` sees a click with no slot hit and clears. |
| Press `Space` / click **Start Round** | Round starts. `ClearSelection()` is called as part of the phase change; the panel (and the shop) are buy-phase surfaces and hide for `Active`. |

### Freeze / phase behaviour

**Buy-phase only.** The panel is only shown while
`RoundController.Phase == Building`. Clicking a slot during `Active` does
nothing (the recommendation is *not* to let it pause a running round —
§10). On the `Building → Active` transition the panel and the shop both
hide and `SelectedSlotIndex` is cleared; on `Active → Building` (round
clear) nothing is auto-selected — the player sees the shop.

### The selected-slot highlight on the board

`COMPARTMENT_DESIGN.md` §2.4 left this as a hook: "a `bool selected` on
the slot that rims the `SlotNiche`." Concretely:

- `BoneMarrowManager.Slot` gains `bool Selected`.
  `SetSelected(i, bool)` is driven by `SelectedSlotIndex` changing.
- Each slot `GameObject` gets a **child rim renderer** built at
  `Initialize` time alongside `Visual`: a `SpriteRenderer` using
  `SpriteShapes.KnowledgeRing` (a thin clean ring silhouette that already
  exists), `sortingOrder = 6` (above the slots at 5, below agents at 10),
  tinted `Accent` `#6E8FB0`, `enabled = false`.
- On selection: `rim.enabled = true` and a cheap `Update`-driven alpha
  breathe between 0.55 and 1.0 over ~1.4 s (cosmetic; it can ignore
  `RoundClock.Frozen` since the panel is only up while frozen anyway, but
  gating it costs nothing and keeps the convention).
- Only ever one slot is selected, so exactly one rim is ever on.

No new sprite, no layout change to the marrow column, no change to the
slot's own `Visual.color` (which already carries the kind colour).

---

## 5. The per-kind progenitor upgrade roster

Placeholders, but **ready to wire**: each row names a mechanic that maps
onto what that unit already does in the sim, and the exact field an
effect would write. Every field cited is already **per-tower mutable
state seeded from a per-kind default** (`GAME_DESIGN.md` §6d) — the
Director's rule that "an upgrade is a write to one tower's field and
nothing more" — except where noted as living on `AdaptiveTuning` (global
statics today; see the wiring note under the table).

Cost curve column: `base` is the first level's ATP price; each subsequent
level costs `base · (1 + 0.6 · level)`, reusing
`ShopTuning.PriceGrowthPerLevel` (0.6) exactly as the Sprint 11 shop and
progenitor-upgrade prices already do. `cap` is the level ceiling.

**Recommendation: ship 3 rows per kind** (§10). The tables below give 4
where there's a good fourth, so the Director can pick.

### Macrophage — `UnitProfile` / `UnitLifecycleTuning`

Sim behaviours in play: efferocytosis (clears debris on its own slot),
quiet retirement at a high kill limit, a low contact stress-sense roll,
a 5-fine-tile footprint, slow (`FineTilesPerTick` 1).

| Display name | Effect (player-facing) | Field it writes | Cost curve |
|---|---|---|---|
| **Efferocytic capacity** (scavenger-receptor panel) | Clears debris markedly faster — frees dead ground to regrow. | `EfferocytosisDebrisPerTick` `0.05 → +0.03 / lvl` | base 30, cap 3 |
| **Tissue residency (M2)** | +8 kills before the cell retires. A longer-lived line. | `KillLimit` `20 → +8 / lvl` | base 40, cap 3 |
| **Pseudopod reach** (podosome extension) | The cell touches — and clears — pathogens one tile further out. | `ContactRadiusFineTiles` `2 → 3` | base 45, cap 1 |
| **Inflammasome priming (NLRP3)** *(optional 4th — see note)* | Better odds each tick of sensing an infected cell it's touching and killing it loudly. | `StressSenseChancePerTick` `0.03 → +0.015 / lvl` | base 70, cap 2 |

> **Note on Inflammasome priming.** `GAME_DESIGN.md` §4b makes the low
> innate stress-sense roll *deliberately* bad — the player is meant to
> feel "my macrophages can't touch this" until dedicated sensors (γδ T /
> CTL) are purchasable. A cheap upgrade that buys that away undermines the
> innate↔adaptive bridge. If included at all it should be **expensive and
> low-cap** (as priced above), and the Director should decide whether it's
> in the roster at all (§10).

### Neutrophil — `UnitProfile` / `UnitLifecycleTuning`

Sim behaviours: fast (`FineTilesPerTick` 3), low kill limit (5), **always
degranulates on depletion** — a `3×` collateral burst that damages
whatever host / infected cell it's standing on (this is the fibrosis
feed-in, `GAME_DESIGN.md` §4 / §6d). The Director's stated intent for
neutrophil upgrades: *reduce* degranulation collateral, or make
depletion something the player can raise the ceiling on.

| Display name | Effect (player-facing) | Field it writes | Cost curve |
|---|---|---|---|
| **Controlled degranulation** | The terminal burst does less collateral tissue damage — less scarring from your own defence. | `DegranulationBurstMultiplier` `3 → −0.5 / lvl`, floor 1 | base 40, cap 4 |
| **Extended lifespan (GM-CSF priming)** | +2 kills before the cell degranulates. | `KillLimit` `5 → +2 / lvl` | base 30, cap 4 |
| **Rapid chemokinesis** | The cell covers ground faster on its way to a signal. | `FineTilesPerTick` `3 → +1 / lvl` — *needs the field moved onto `UnitLifecycleTuning`; see wiring note* | base 45, cap 2 |
| **Respiratory burst reach** *(optional 4th)* | ROS diffusion — hits pathogens one tile further out. | `ContactRadiusFineTiles` `2 → 3` | base 45, cap 1 |

### Dendritic cell — `AdaptiveTuning` (+ per-tower cap)

Sim behaviours: paces the tissue band its whole life, samples antigen off
debris (eating a bite of the pile — competes with efferocytosis), carries
it to the node, pairs with helper-T cells to teach knowledge. Cargo is
good for `DcPresentationsPerCargo` (4) pairings.

| Display name | Effect (player-facing) | Field it writes | Cost curve |
|---|---|---|---|
| **Macropinocytosis rate** | Each trip to the node is worth more presentations before the cell has to go back for more antigen. | `AdaptiveTuning.DcPresentationsPerCargo` `4 → +2 / lvl` | base 35, cap 3 |
| **CCR7 expression** | The cell migrates faster — antigen reaches the node sooner. | `AdaptiveTuning.DcFineTilesPerTick` `3 → +1 / lvl` | base 40, cap 3 |
| **Antigen-sparing sampling** | Takes a smaller bite of each debris pile — leaves more for macrophage clearance, competes less. | `AdaptiveTuning.DcDebrisSamplePerBite` `0.34 → −0.08 / lvl`, floor 0.1 | base 30, cap 3 |
| **MHC-II density** *(optional 4th)* | Every successful pairing teaches the adaptive system more. | `AdaptiveTuning.KnowledgePerMatch` `3 → +1.5 / lvl` | base 55, cap 2 |

### Helper-T cell — `AdaptiveTuning` (+ per-tower cap)

Sim behaviours: born with a random 8-bit barcode, wanders the node on the
co-localisation field, freezes for `PairingSeconds` (1.5) when it meets a
DC, teaches iff Hamming distance ≤ `MatchMaxHammingDistance` (2), ages out
at `LymphocyteLifespanSeconds` (20) and is re-emitted with a fresh tag
(barcode turnover, `GAME_DESIGN.md` §5c).

| Display name | Effect (player-facing) | Field it writes | Cost curve |
|---|---|---|---|
| **Clonal expansion** | More helper-T cells resident in the node at once — a busier node teaches faster. | per-tower `MaxActiveChildren` (`AdaptiveCapFor` seed) `8 → +4 / lvl` | base 35, cap 3 |
| **TCR affinity maturation** | A near-miss barcode still teaches — more pairings count. | `AdaptiveTuning.MatchMaxHammingDistance` `2 → +1 / lvl`, cap 4 | base 45, cap 2 |
| **Rapid recirculation** | Shorter helper-T lifespan — the barcode repertoire refreshes faster, so you're less likely to be stuck with no match. | `AdaptiveTuning.LymphocyteLifespanSeconds` `20 → −4 / lvl`, floor 6 | base 30, cap 3 |
| **IL-2 autocrine loop** *(optional 4th)* | Pairings resolve faster — each cell churns through more DCs. | `AdaptiveTuning.PairingSeconds` `1.5 → −0.3 / lvl`, floor 0.4 | base 40, cap 2 |

### Wiring note (for the sprint that makes these real)

- **Innate rows** already have per-tower fields — `GetTuning(i).KillLimit
  += n` and nothing else, exactly as §6d wants.
- **`FineTilesPerTick`** is currently on `UnitProfile` only and read by
  `SearchUnit` from the shared profile. To make "Rapid chemokinesis"
  per-tower it must be copied into `UnitLifecycleTuning` (a one-field
  addition to `FromProfile` / `CopyFromProfile` / `CopyFrom`, mirroring
  the other lifecycle fields) and `SearchUnit` pointed at the tuning
  instance instead of the profile. Additive, mechanical.
- **Adaptive rows** cite `AdaptiveTuning` statics, which are process-global
  — an upgrade there would affect *every* DC/helper-T tower, not just the
  upgraded niche. Two honest options for the wiring sprint: (a) accept
  global-per-kind for adaptive upgrades (simplest; arguably fine since a
  player rarely runs two DC niches), or (b) give the adaptive slot's
  `Slot.Tuning` real per-tower fields for these numbers and have
  `AdaptiveDirector` read them per-agent. This spec doesn't decide it —
  it's a §10 question — but the *rows and their costs are the same either
  way*.

---

## 6. Shop panel + tower picker — migrate now

**Recommendation: migrate the minimal HUD, the debug readout, the upgrade
panel, the buy-phase shop, and the empty-slot tower picker — all of it —
to UI Toolkit in this one sprint. Do not leave the shop and picker in
IMGUI.**

### Why all-in

- **One styling system, once.** The upgrade panel's row component (name +
  effect + dots + cost + buy-state button) *is* the shop's row component
  and *is* the picker's button. Building it for the upgrade panel and then
  writing a second IMGUI version for the shop is pure waste, and
  guarantees a second migration sprint later (`BACKLOG.md` "A real buy UI"
  explicitly wants this consolidated).
- **The screen would otherwise be inconsistent in every playtest.** IMGUI
  `GUI.skin.button` next to a designed UITK panel, in the same buy phase,
  is exactly the "doesn't feel like one product" the Director is reacting
  to.
- **The picker is tiny.** It's four buttons
  (`BoneMarrowManager.DrawBuyButton` ×4). Porting it is an afternoon.
- **Click occlusion forces the shop to move anyway** (§4): the shop is
  left-docked IMGUI today and sits over the marrow colliders. Any version
  that keeps physics-raycast slot clicks needs the shop off the left —
  and if you're moving it, move it into the new system.

### The cost of all-in

A bigger sprint than "just the HUD + upgrade panel." The shop is 6 rows,
one of which (`CytokineSensingUpgrade`) has a live bridge to
`Chemotaxis.SensingUpgradeLevel` that must keep working. The picker needs
the `SelectedSlotIndex` plumbing (which the upgrade panel needs anyway).

**Middle path if scope has to shrink:** HUD + debug toggle + upgrade panel
+ selected-slot highlight this sprint; shop and picker next sprint. They
are the least-watched surfaces (the picker fires once per slot, ever; the
shop is a flat list). But the row component would then be built twice
unless it's written UITK-first from the start. If the middle path is
taken, still write the row component in UITK and leave the shop/picker as
thin IMGUI callers of it — don't write throwaway IMGUI rows.

### Sketch — shop and picker in the same system

Both dock to the **right**, in the same rectangle the upgrade panel uses,
mutually exclusive with it:

```
   Building phase, no slot selected:            Building phase, empty slot selected:

   ┌──────────────────────────────────┐         ┌──────────────────────────────────┐
   │  ATP 214   ROUND 4   LIVES 96    │         │  ATP 214   ROUND 4   LIVES 96    │
   └──────────────────────────────────┘         └──────────────────────────────────┘
   ┌──────────────────────────────────┐         ┌──────────────────────────────────┐
   │  SHOP                            │         │  PLACE PROGENITOR · SLOT 3       │
   │  ──────────────────────────────  │         │  ──────────────────────────────  │
   │  Cytokine sensing +      35 ATP  │         │  Macrophage             40 ATP  │
   │  sharpens every unit's   [ BUY ] │         │  ruffled scavenger      [ BUY ] │
   │  gradient bias · Lv 1            │         │  Neutrophil             15 ATP  │
   │  ──────────────────────────────  │         │  fast, high collateral  [ BUY ] │
   │  Mucus turnover          30 ATP  │         │  Dendritic cell         30 ATP  │
   │  (barrier) · Lv 0        [ BUY ] │         │  antigen shuttle        [ BUY ] │
   │  ──────────────────────────────  │         │  Helper-T cell          25 ATP  │
   │  Host dsRNA sensor       45 ATP  │         │  node · barcode match   [ BUY ] │
   │  … four more rows …              │         │                        close ✕ │
   └──────────────────────────────────┘         └──────────────────────────────────┘
```

- **Shop** — one row per `ShopItem` (`ShopLedger.NextPrice` / `LevelOf` /
  `CanBuy` / `TryBuy`, unchanged). `CytokineSensingUpgrade` row keeps its
  "REAL" affordance — but instead of a text tag, it's the only shop row
  with an `Accent` dot before the name, and the debug readout still shows
  the effective-sharpness multiplier. Buy calls
  `shop.TryBuy(item, wallet)`; the
  `Chemotaxis.SensingUpgradeLevel = shop.LevelOf(...)` bridge moves from
  `HudOverlay.Update` to `UiController.Update` unchanged.
- **Picker** — `Place progenitor` title, four rows
  (`BoneMarrowManager.PriceFor(kind)`), each with a one-line descriptor.
  Adaptive kinds only shown when `adaptive != null` (same rule as
  `BoneMarrowManager.OnGUI` today). Buy calls
  `PlaceTower(SelectedSlotIndex, kind)`. A `close ✕` clears the selection.
- Shop shows when `Building` **and** `SelectedSlotIndex == null`. Picker
  shows when `Building` **and** the selected slot is `Empty`. Upgrade
  panel shows when `Building` **and** the selected slot is `Placed`.

### The world-space `CompartmentLabel`s

"Bone Marrow — click an empty slot to place a tower" and "Lymph Node /
antigen presentation" (`GameBootstrap.BuildBoneMarrowBackdrop` /
`BuildLymphNodeBackdrop`, via `CompartmentLabel.cs` IMGUI).

**Recommendation: keep them as world-anchored labels, trim the copy,
restyle, port last (or leave IMGUI one more sprint).** They are spatial
annotations that point at an organ — folding them into a screen-space
panel loses that. Trim to just **"Bone marrow"** and **"Lymph node"**
(11 px `InkDim`, no panel behind them, a 1 px `Rule` underline): the
selected-slot rim + the picker panel now teach placement, so the
instruction text is redundant. Porting `CompartmentLabel` to a UITK
element positioned with `RuntimePanelUtils.CameraTransformWorldToPanel` is
straightforward but low value; it does not visually clash in the
meantime because it's just thin text with no chrome. Lowest priority in
the commit plan.

---

## 7. UI Toolkit from-code implementation sketch

### Does it need a package?

**No `com.unity.*` package install is required.**
`game/Packages/manifest.json` already lists
`com.unity.modules.uielements: 1.0.0` (line 25) — that is the built-in
runtime UI Toolkit module in Unity 6. `UnityEngine.UIElements.PanelSettings`,
`UIDocument`, `VisualElement`, `Label`, `Button`, `ScrollView`,
`Background.FromSprite`, `RuntimePanelUtils` are all in-engine. This is
already noted in `docs/TEAM_RETRO.md` Sprint 1: "*evaluating UI Toolkit
(`com.unity.modules.uielements`, already present)*". No UI Builder, no
`com.unity.ui` package, no network step.

**The one caveat — a theme style sheet.** `PanelSettings` has a
`themeStyleSheet` field that normally points at a `ThemeStyleSheet`
(`.tss`) asset, and `ThemeStyleSheet` cannot be constructed at runtime
(its importer is editor-only). With `themeStyleSheet` left null you get a
one-time console warning and **no default control theme** — Unity's stock
button chrome, default fonts on unstyled elements, etc. are absent.

For this UI that is *mostly fine*: every element is explicitly styled
against `UiTheme` (see below), and we specifically do **not** want Unity's
default `Button` look. Three ways to handle the warning, for the head to
pick:

1. **Accept it and suppress** — filter that one warning string in a log
   handler, or just live with one line at boot. Zero assets. Recommended
   default.
2. **Ship one tiny asset** — a single `DefaultRuntimeTheme.tss` (literally
   `@import url("unity-theme://default")` or an empty theme). This is one
   deliberate editor step, far smaller than the `com.unity.ugui` decision
   `TEAM_RETRO.md` Sprint 1 describes — but it *is* an asset file, which
   the brief says to avoid if avoidable. Only do this if the missing
   default fonts on any element actually bite.
3. **Assign the packaged default** — some Unity 6 installs expose a
   built-in runtime theme loadable via `Resources`/`AssetDatabase`; not
   reliable to assume. Skip unless (1) and (2) both fail.

Flagging this honestly per the brief: I'm confident **no package** is
needed; I'm ~80% confident option (1) is clean enough that no asset is
needed either. If the head finds unstyled text rendering wrong without a
theme, option (2) is the fallback and it's one small file.

### Construction, in `GameBootstrap`

A new `BuildUiRoot()` called from `Awake()` where `BuildHud(...)` is
called today (replacing it):

```
// GameBootstrap.BuildUiRoot()
var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
panelSettings.name              = "ITD_PanelSettings";
panelSettings.scaleMode         = PanelScaleMode.ConstantPhysicalSize;   // or ConstantPixelSize
panelSettings.referenceResolution = new Vector2Int(1920, 1080);
panelSettings.screenMatchMode   = PanelScreenMatchMode.MatchWidthOrHeight;
panelSettings.match             = 0.5f;
panelSettings.sortingOrder      = 100;      // above sprites; UITK panels are their own layer anyway
panelSettings.clearColor        = false;    // draw over the game, don't clear it
// panelSettings.themeStyleSheet = <null — see caveat above>

var uiGo  = new GameObject("UiRoot");
var doc   = uiGo.AddComponent<UIDocument>();
doc.panelSettings   = panelSettings;
doc.visualTreeAsset = null;                 // tree is built in code, not from UXML

var controller = uiGo.AddComponent<UiController>();
controller.Bind(board, wallet, rounds, pathogenSpawner, boneMarrow,
                knowledge, adaptive, shop, gutInterface, tally,
                macrophageProfile.FineTilesPerTick, neutrophilProfile.FineTilesPerTick);
```

`UiController.OnEnable()` builds `doc.rootVisualElement`:

```
root.style.flexGrow = 1;
root.pickingMode = PickingMode.Ignore;      // the empty root never eats a world click
// dock containers:
_topRight   = Dock(root, Align.FlexEnd,   Justify.FlexStart); // minimal HUD
_rightStack = Dock(root, Align.FlexEnd,   Justify.FlexStart); // upgrade / picker / shop, below the HUD
_bottomLeft = Dock(root, Align.FlexStart, Justify.FlexEnd);   // debug readout
_hud     = new HudView(_topRight);
_upgrade = new UpgradePanelView(_rightStack);
_picker  = new TowerPickerView(_rightStack);
_shop    = new ShopView(_rightStack);
_debug   = new DebugReadoutView(_bottomLeft);
_debug.Root.hidden = true;
```

Only the panels have `pickingMode = Position` (the default); the root and
the dock containers are `Ignore`, so a click that isn't on a panel falls
straight through to the physics raycaster and `BoneMarrowSlot.OnMouseDown`
still works. Verify this early (§9).

### Styling

**Inline `style.*` only** — no `styleSheets.Add`, because a USS *string*
can't be parsed into a `StyleSheet` at runtime without the editor
importer. A shared static:

```
static class UiTheme {
    public static readonly Color PanelBg  = new(0.059f, 0.055f, 0.067f, 0.86f);
    public static readonly Color Ink      = new(0.914f, 0.898f, 0.855f);
    public static readonly Color InkDim   = new(0.545f, 0.522f, 0.478f);
    public static readonly Color Rule     = new(0.914f, 0.898f, 0.855f, 0.14f);
    public static readonly Color Atp      = new(0.796f, 0.722f, 0.471f);
    public static readonly Color LivesOk  = new(0.510f, 0.663f, 0.627f);
    public static readonly Color LivesLow = new(0.753f, 0.361f, 0.263f);
    public static readonly Color Accent   = new(0.431f, 0.561f, 0.690f);
    public static readonly Color Defeat   = new(0.604f, 0.231f, 0.173f);
    public const int S = 4;
    // helpers:
    public static Label  Text(string s, int px, Color c, FontStyle w = FontStyle.Normal) { … }
    public static VisualElement Panel() { … }          // PanelBg, 1px Rule border, 3px radius, padding 3S/4S
    public static VisualElement Divider() { … }        // 1px Rule, margin 2S vertical
    public static VisualElement Row(string name, string effect, int levels, int cap,
                                    int price, System.Action onBuy, bool affordable) { … }
}
```

`UiTheme.Row(...)` is the component shared by the upgrade panel, the shop,
and (a trimmed variant) the picker.

### The view classes

| Class | Owns | Refresh |
|---|---|---|
| `UiController` (`MonoBehaviour`) | the `UIDocument`, all views, the debug key, `Esc`, the `Chemotaxis.SensingUpgradeLevel` bridge, the F9 passthrough | `Update()` polls and calls each visible view's `Refresh()` |
| `HudView` | the top-right stat bar; caches the `Label`s it mutates | `Refresh(wallet, rounds, spawner)` — sets `.text` and the lives colour; rebuilds the lower block only on a phase change |
| `UpgradePanelView` | header (portrait, dots, fielded count), the row list for the selected slot's kind, footer | `SetTarget(boneMarrow, slotIndex)` builds the rows once; `Refresh()` updates each row's buy-enabled / dots / cost colour and the fielded count |
| `TowerPickerView` | four buy rows (+ adaptive two conditionally) | built once on `SetTarget(slotIndex)`; `Refresh()` updates affordability |
| `ShopView` | one `UiTheme.Row` per `ShopItem` | `Refresh()` on `shop.Revision` change or affordability change |
| `DebugReadoutView` | a `ScrollView` of monospace `Label`s | `Refresh()` rebuilds the text block from the same `Build*Line()` logic moved out of `HudOverlay` |

### Reading the model — poll, don't event

There are no events on `AtpWallet` / `RoundController` / `BoneMarrowManager`
/ `ShopLedger`, and adding them is out of scope. `UiController.Update()`
polls every frame; the values are 3–10 cheap int getters. Guard the
expensive rebuilds:

- `ShopView` / `UpgradePanelView` row *structure* is built once per
  `SetTarget`; only per-row `SetEnabled` / dot fills / colour flip each
  frame.
- `ShopLedger.Revision` and a cached `wallet.Balance` / `rounds.Phase` /
  `SelectedSlotIndex` let `UiController` skip work when nothing changed.
- `DebugReadoutView.Refresh()` only runs while `!Root.hidden`.

Selection: `UiController` reads `BoneMarrowManager.SelectedSlotIndex` each
frame; on a change it calls `SetSelected` on the old/new slot (board rim)
and swaps which right-dock view is visible.

Buttons call the **existing real methods** directly —
`boneMarrow.UpgradeTower(i, row)`, `boneMarrow.PlaceTower(i, kind)`,
`shop.TryBuy(item, wallet)`, `rounds.StartRound()`. No new command layer.

### Headless-testability

**None of the rendered UI is headless-testable** — it's `Update()` /
event-driven, `Update()` does not run in Editor batchmode, and no harness
asserts UITK output. Same status as the Sprint 13/15 rendering passes
(`SPRITE_DESIGN.md` §5.2, `COMPARTMENT_DESIGN.md` §6). The **one**
automatable signal: `GameBootstrap.Awake` now also constructs the
`PanelSettings` + `UIDocument` + builds the initial tree, so **"a
batchmode scene load / bootstrap completes with 0 exceptions"** covers "no
view class threw while building" (a null ref in `SetTarget`, a bad
`Background.FromSprite` on a not-yet-generated sprite, etc.). If a
scene-load smoke `-executeMethod` exists it should run after each commit;
if not, one is cheap and worth adding.

---

## 8. Migration / commit plan

Ordered, each independently landable and build-testable. File paths are
under `game/Assets/Scripts/`. Line numbers are from the current tree and
will drift.

Every new `.cs` needs its `.meta` committed alongside
(`AGENT_HANDBOOK.md`).

### Commit 1 — UITK bootstrap, empty root

- **New** `UI/UiTheme.cs` (palette consts + helper stubs), `UI/UiController.cs`
  (builds `PanelSettings` + `UIDocument`, empty docks, reads the debug key
  + `Esc`, moves the `Chemotaxis.SensingUpgradeLevel` bridge and the F9
  passthrough out of `HudOverlay.Update`).
- **`Bootstrap/GameBootstrap.cs`** — `BuildUiRoot()` called where
  `BuildHud(...)` is (L553/L199). Leave `BuildHud` + `HudOverlay` in place
  for now (both HUDs briefly co-exist; the old one gets stripped commit by
  commit).
- Decide the `themeStyleSheet` question here (§7): default is null +
  suppress.
- **Verify:** bootstrap 0 exceptions; a blank panel renders; world clicks
  on marrow slots still open the *old* IMGUI panels (proves
  `pickingMode` is right).

### Commit 2 — `HudView`, delete the old round bar

- **New** `UI/HudView.cs` — the §2 stat bar + phase block + Start button +
  GAME OVER.
- **`Rendering/HudOverlay.cs`** — delete `DrawRoundBar()` (L195–244) and
  its call in `OnGUI` (L87).
- **Verify:** ATP / round / lives update live; `Space` and the button both
  start a round; lives flash on a breach; GAME OVER shows on defeat.

### Commit 3 — `DebugReadoutView`, delete the top-left dump

- **New** `UI/DebugReadoutView.cs` — the §3 corner panel; move
  `BuildPopulationLine` / `BuildPathogenLine` / `BuildInvasionLine` /
  `BuildKnowledgeHeader` / `BuildLadderLine` / `BuildPerformanceLine` and
  the `infoLine` / `toggleLine` / `heatmapLine` strings out of
  `HudOverlay`.
- **`Rendering/HudOverlay.cs`** — delete the big panel block in `OnGUI`
  (L91–121) and `DrawShopPanel` is *not* touched yet (commit 7). After
  this commit `HudOverlay.OnGUI` only still draws the shop.
- **`UI/UiController.cs`** — the debug key toggles `DebugReadoutView.Root.hidden`.
- **Verify:** backtick toggles it; default hidden; every number that was
  on screen before is present when it's shown; `F9` / `C` still work with
  it hidden.

### Commit 4 — `BoneMarrowManager` selection API

- **`Units/BoneMarrowManager.cs`** — add `int? SelectedSlotIndex`,
  `ClearSelection()`, `SetSelected(int,bool)`; `OnSlotClicked` sets
  `SelectedSlotIndex` (keeps `pendingChoiceIndex` / `pendingUpgradeIndex`
  as private derived state for the still-live `OnGUI` panels until commit
  6). Expose `ColorForKind` (make it `public static` or add a
  `public Color GetSlotColor(int)`).
- **Verify:** `Sprint11Verification` / `LifecycleVerification` still green
  (no behaviour change, additive surface).

### Commit 5 — selected-slot highlight on the board

- **`Units/BoneMarrowManager.cs`** — `Slot.Selected`; build a child rim
  `SpriteRenderer` (`SpriteShapes.KnowledgeRing`, `sortingOrder 6`,
  `Accent` tint, disabled) per slot in `Initialize` (L191–215); a tiny
  `Update` alpha-breathe on the enabled one.
- **Verify (eyeball):** selecting a slot (via the still-IMGUI click) rims
  exactly that slot; deselecting clears it.

### Commit 6 — `UpgradePanelView` + `TowerPickerView` + catalog; delete the IMGUI marrow panels

- **New** `UI/ProgenitorUpgradeCatalog.cs` — the §5 per-kind row
  descriptors (`struct Row { string Name; string Effect; string
  FieldLabel; int BasePrice; int Cap; }`, arrays keyed by `UnitKind`).
- **New** `UI/UpgradePanelView.cs`, `UI/TowerPickerView.cs`.
- **`Economy/ShopTuning.cs`** — `ProgenitorUpgradePrice` grows an overload
  `ProgenitorUpgradePrice(int basePrice, int level)` (same
  `1 + 0.6·level` curve) so per-row base prices work; keep the old
  `ProgenitorUpgradePrice(int level)` as `=> ProgenitorUpgradePrice(35,
  level)`.
- **`Units/BoneMarrowManager.cs`** — `Slot.UpgradeLevel` (single int) →
  `Slot.UpgradeLevels` (`int[]` sized to that kind's row count).
  `UpgradeTower(int slot, int row)` spends
  `ShopTuning.ProgenitorUpgradePrice(catalogBase, UpgradeLevels[row])` and
  bumps `UpgradeLevels[row]`. Keep `UpgradeTower(int) => UpgradeTower(i,
  0)` and `GetUpgradeLevel(int) => sum(UpgradeLevels)` so
  `Sprint11Verification` compiles unchanged. **Still no
  `UnitLifecycleTuning` write** — assert that in the harness as today.
  Delete the `pendingChoiceIndex` / `pendingUpgradeIndex` `OnGUI` blocks
  (L555–597) and `DrawBuyButton` (L600–611).
- **Verify:** click placed slot → upgrade panel with the right kind's
  rows; buy states (affordable / can't afford / maxed) correct; click
  empty slot → picker; `Esc` / close / other-slot / Start all dismiss;
  `UpgradeTower` still leaves `GetTuning(i)` untouched (harness).

### Commit 7 — `ShopView`, delete `DrawShopPanel`

- **New** `UI/ShopView.cs` — six `UiTheme.Row`s over `ShopLedger`;
  `CytokineSensingUpgrade` gets the `Accent` dot; buy → `shop.TryBuy`.
- **`Rendering/HudOverlay.cs`** — delete `DrawShopPanel()` (L123–166) and
  its call (L88). `HudOverlay.OnGUI` is now empty → delete the method;
  keep the class only if `Update`'s bridge/F9 haven't fully moved (they
  did in commit 1), else **delete `HudOverlay.cs` entirely** and drop it
  from `BuildHud` / the `HUD` GameObject (keep `CytokineToggle` on that
  GameObject or move it to `UiRoot`).
- **Verify:** all six shop rows, prices, grey-out; cytokine-sensing bridge
  still raises `EffectiveSharpness` (debug readout shows the multiplier).

### Commit 8 — `CompartmentLabel` (lowest priority)

- Either port `Rendering/CompartmentLabel.cs` to a UITK world-anchored
  element (`RuntimePanelUtils.CameraTransformWorldToPanel`) or just trim
  the two label strings in `GameBootstrap.BuildBoneMarrowBackdrop`
  (L427) / `BuildLymphNodeBackdrop` (L444) to "Bone marrow" / "Lymph
  node" and restyle the IMGUI to 11 px `InkDim`.
- **Verify (eyeball):** labels still sit on their organs; no clash with
  the new panels.

### Commit 9 — docs

- **`docs/UI_STYLE_GUIDE.md`** — rewrite the "HUD / panels — IMGUI" section
  to "HUD / panels — UI Toolkit, built from code"; note UITK panels are a
  separate layer above the sprite sorting table (no table change).
- **`docs/INTERFACE.md`** — a "Sprint 16 changes" section:
  `BoneMarrowManager.SelectedSlotIndex` / `ClearSelection` /
  `UpgradeTower(int,int)` / `Slot.UpgradeLevels`,
  `ShopTuning.ProgenitorUpgradePrice(int,int)`, the `UiController` /
  view classes, the `PanelSettings`-from-code note and the theme caveat.
- **`docs/ENGINE_STATUS.md`**, **`docs/CHANGELOG.md`**, **`docs/TEAM_RETRO.md`**
  per the usual sprint close.

### What gets deleted, and when

| From | What | Commit |
|---|---|---|
| `HudOverlay.cs` | `DrawRoundBar()` + call | 2 |
| `HudOverlay.cs` | top-left panel block, `Build*Line()`, info/toggle/heatmap strings | 3 |
| `HudOverlay.cs` | `DrawShopPanel()` + call; then the whole file | 7 |
| `BoneMarrowManager.cs` | `pendingChoiceIndex` / `pendingUpgradeIndex` `OnGUI` blocks, `DrawBuyButton`, `EnsureStyles` (IMGUI) | 6 |
| `BoneMarrowManager.cs` | the per-slot `GUI.Label` "children alive / cap" in `OnGUI` — moves into `UpgradePanelView`'s header and the debug readout | 6 |
| `GameBootstrap.cs` | `BuildHud(...)` → `BuildUiRoot(...)` | 1 (add) / 7 (remove old) |

---

## 9. Verification

**Automatable:** exactly one thing — `GameBootstrap.Awake` builds the
`PanelSettings`, the `UIDocument`, and the initial view tree, so a
**batchmode scene load / bootstrap that completes with 0 exceptions**
proves no view class threw while constructing (null slot, bad
`Background.FromSprite`, catalog index slip). Run it after every commit.
Add a one-line `-executeMethod` scene-load smoke if none exists — it's
cheap and would also have caught the Sprint 4 degenerate-band bug.

The existing headless harnesses (`Sprint11Verification`,
`LifecycleVerification`, `EconomyVerification`) must **stay green with no
edits** — commits 4/6 are additive to `BoneMarrowManager` and keep the
old `UpgradeTower(int)` / `GetUpgradeLevel(int)` signatures as shims, and
`UpgradeTower` still never touches `UnitLifecycleTuning` (the harness
already asserts this — keep that assertion).

**The Director eyeballs (screenshot / live):**

1. The top-left debug dump is **gone**; the only always-on UI is the
   top-right stat bar, and it reads ATP / round / lives at a glance.
2. The stat bar looks *of* the histology plate — flat black glass,
   hairlines, two type sizes — not a game HUD. A screenshot still reads as
   "a stained section."
3. Backtick brings the full debug readout back, bottom-left, with every
   number that used to be on screen; a fresh launch has it hidden.
4. Clicking a placed marrow slot opens the upgrade panel; it names the
   progenitor, shows its portrait and niche level, and lists the per-kind
   rows with effect text, cost, level dots, and correct buy-button states
   (affordable / can't afford / maxed).
5. The selected slot is rimmed on the board; the rim moves when you click
   a different slot; it clears on dismiss.
6. Clicking an empty slot opens the tower picker in the same place;
   `Esc` / close / clicking the board / starting the round all dismiss
   cleanly.
7. The buy-phase shop is in the same right-dock system, same visual
   language as the upgrade panel — no IMGUI buttons anywhere in the buy
   phase.
8. Buying anything still just spends ATP and lights a dot — nothing on the
   board changes (except `Cytokine sensing +`, which still sharpens
   movement — visible via the debug readout's multiplier).
9. World clicks still reach the marrow slots with panels on screen (no
   dead zone under a panel that isn't over a slot).
10. During a running round only the stat bar is up; the panel and shop are
    gone; they come back when the round clears.

---

## 10. Open questions for the Director

1. **Debug-toggle key.** Recommendation: backtick `` ` ``
   (`KeyCode.BackQuote`) — the standard dev-overlay key, unbound here.
   `F3` if you'd rather a function key. Confirm.

2. **Lives — number or pips?** With a 100-life pool, 100 pips is
   unreadable and 10 pips-of-10 is a lie about the granularity.
   Recommendation: a **plain integer**, `LivesOk` teal that turns
   `LivesLow` oxblood below 25%, with a single flash on each decrement.
   Accept, or do you want a bar / segmented gauge?

3. **Upgrade-panel placement — fixed right dock, or floating by the
   slot?** Recommendation: **fixed right dock, under the HUD.** It avoids
   the panel sitting on the marrow colliders, gives a stable place to
   look, and has room for 3–4 rows; the selected-slot rim provides the
   spatial link. Floating-by-the-slot is buildable (anchor via
   `CameraTransformWorldToPanel`) if you want the tighter coupling — but
   then the shop still has to leave the left edge.

4. **Does the shop migrate this sprint?** Recommendation: **yes — migrate
   HUD, debug readout, upgrade panel, shop, and picker all at once.** The
   row component is shared; leaving the shop in IMGUI guarantees a second
   migration and an inconsistent buy phase. The middle path (shop + picker
   next sprint) is viable but only if the row component is still written
   UITK-first.

5. **How many upgrade rows per kind?** The §5 tables give 4 where there's
   a good fourth; recommendation is **3 per kind** for the first pass (the
   panel stays short, the choices stay legible). Which 3 — and is
   **"Inflammasome priming"** (the macrophage stress-sense row) in the
   roster at all, given §4b wants the innate-can't-touch-this pain to be
   *felt* until real sensors are purchasable? Recommendation: leave it out
   of the first pass, or keep it expensive and low-cap.

6. **Does clicking a slot during a running round do anything?**
   Recommendation: **no — the upgrade panel and shop are buy-phase only**,
   consistent with `RoundClock` freezing everything and with the shop's
   existing `Phase == Building` gate. Clicking a slot mid-round just
   doesn't select. Accept, or do you want the panel viewable (read-only,
   or pausing the round) while a round runs?

7. **The `PanelSettings` theme asset.** Runtime UI Toolkit works with no
   theme asset, but logs one warning and ships no default control skin
   (which we mostly don't want anyway). Recommendation: **accept the
   warning, no asset.** Fallback if unstyled text renders wrong: one tiny
   `DefaultRuntimeTheme.tss` (a single small asset file — the only place
   this pass would touch the asset pipeline). Your call on whether the
   fallback is pre-authorised.

8. **Adaptive upgrades — global or per-tower?** The DC / helper-T rows in
   §5 cite `AdaptiveTuning` statics, which are process-global; an upgrade
   there affects every DC/helper-T niche, not just the one you clicked.
   Fine for a placeholder (and rare to run two). Making them truly
   per-tower is a real (but mechanical) change to `AdaptiveDirector` for
   the wiring sprint. Note it now — the rows and prices don't change
   either way.

9. **The `CompartmentLabel`s** ("Bone Marrow — click an empty slot…" /
   "Lymph Node / antigen presentation"). Recommendation: **keep them
   world-anchored, trim to "Bone marrow" / "Lymph node"** (the rim + the
   picker teach placement now), restyle to the new type scale, port to
   UITK last or leave as thin IMGUI text one more sprint. Accept?
