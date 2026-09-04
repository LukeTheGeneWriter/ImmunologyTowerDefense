# Agent Playtest 01 — Sprint 16 UI pass

Status: **first agentic playtest run**, 2026-09-04. Tester was a Claude Code
session with no prior context on this project, driving the pre-built
Windows executable via PowerShell + screenshot loop (no source read, no
rebuild). This is as much a report on *how well an agent can test this game
at all* as it is a report on the game itself — read the tooling section
before trusting the coverage claims above it.

Build tested: `game/Builds/Windows/ImmunologyTowerDefense.exe` (exe dated
8/18, data folder rebuilt 9/4 11:31 AM). Two launches, logs at
`C:\Users\lukef\AppData\Local\Temp\agenttest.log` (first launch, later
disappeared — see below) and `agenttest2.log` (second launch, the one this
report is based on).

Screenshots referenced below are all under:
`C:\Users\lukef\AppData\Local\Temp\claude\C--Users-lukef-ImmunologyTowerDefense\58bcfcf7-d254-48ec-a099-8a7b14574189\scratchpad\`

---

## What I was able to do

**Driven via real input (confirmed working):**
- Launched the build windowed, moved/foregrounded the window, and
  screenshotted it repeatedly over the course of the session.
- **`Space`** to start a round — confirmed multiple times (`shot13_spacekey.png`,
  `shot22_round2_start.png`), reliably flips `BUY PHASE` → `ROUND IN
  PROGRESS` and back again on its own once the round clears.
- **Backtick (`` ` ``)** to toggle the debug readout — confirmed both
  directions (`shot15_backtick2.png` on, then off again before
  `shot16_sendinput_test.png`). **Confirmed OFF by default** at boot
  (`shot09_restore.png` shows a clean boot state with no debug panel).
- Passive observation of the game running for several real minutes once
  focused — round loop, ATP economy, and pathogen simulation all visibly
  progressing on their own.

**Observed only, not deliberately driven:**
- The tower-picker floating panel and the mid-round shop-collapsed header
  appeared once early in the session (see Finding 4) without a click I can
  attribute with confidence — I'm reporting what was on screen, not
  claiming I clicked it into being.
- Dendritic-cell motion, watched passively across a burst of screenshots
  during a live round.

**Could not drive at all:**
- **Every mouse interaction** — clicking a bone-marrow slot, a shop BUY
  button, the Start Round *button* (as opposed to the key), or the SHOP
  header strip. See "Notes on the tooling" for the full account of what I
  tried (four distinct injection methods, all failing) and why I believe
  this is a tooling limitation, not necessarily a game bug — but I cannot
  rule out the game side, and said so honestly below.

Net effect: I can **speak with confidence** to items 1, 2, and 3 of the
Director's checklist (HUD, Space-to-start, debug toggle default-off), and
to item 6 (DC motion, observed passively). I **cannot** speak with
confidence to items 4 and 5 (buy panels, mid-round buying) — I saw suggestive
evidence once but couldn't reproduce it on demand.

---

## Findings

### 1. Dendritic cell motion looks stuck in place, not paced — likely confirms the known jitter bug

Screenshots: `dc_frame1.png`, `dc_frame3.png`, `dc_frame6.png` (same magenta
star-shaped cell, ~350 ms apart, ~1.75 s span total, during a live round).

Across all three frames the DC sits in the same handful of pixels near the
lumen/base edge (roughly screen coordinates 1043–1057, 620–628 in a
1920-wide capture) — it does not visibly travel any distance up or down the
lane in that window, despite `docs/UI_DESIGN.md`'s referenced Sprint 12/14
work explicitly building a DC that "paces the tissue band its whole life"
with a "full base↔lumen lap...legible inside a ~30s round"
(`GAME_DESIGN.md` §5a). What I actually saw reads as *vibrating in place*
rather than *pacing* — consistent with the "known jitter problem" the brief
asked me to describe in my own words rather than diagnose. I did not
capture a long enough or dense enough burst to characterize the jitter's
amplitude or frequency precisely, and I was not able to place a
purpose-bought Dendritic cell tower (that requires a click — see below), so
this is one incidentally-present DC, not a controlled test.

### 2. The HUD is readable and live — matches the spec's direction well

Screenshot: `shot04_wide.png`, `shot09_restore.png`, `shot23_tab_test.png`.

Top-right panel shows ATP / ROUND / LIVES exactly as `UI_DESIGN.md` §2
mocks it, numerals large and legible against the tissue background, phase
line updates correctly ("BUY PHASE · TIME IS FROZEN" with the round's
tagline in `Building`; "ROUND IN PROGRESS" + "batch N/M · K in play" in
`Active`). I watched ATP, ROUND, and LIVES all change live across several
minutes of passive observation (ATP 100→180→260 as kills paid out and round
lump sums landed; LIVES 100→98→91→90 as breaches happened; ROUND climbed
1→2→3 automatically). No overlap with other panels, no clipping once the
window was fully on-screen. This one item I'm confident is working exactly
as designed.

### 3. Debug readout: correctly off by default, correct content when toggled

Screenshot: `shot15_backtick2.png` (on), `shot09_restore.png` (a later,
independent boot showing it off by default).

Toggling it on showed a bottom-left monospace panel matching
`UI_DESIGN.md` §3's content list near-verbatim: board dims, macrophage/
neutrophil tick speed, the buy blurb, cytokine sensing state, active
units/pathogens counts, adhesions/breaches/excreted/reached-base tally, and
the per-species knowledge ladder with rung checkboxes. It sits bottom-left,
over the marrow column, as specified, and doesn't visually collide with the
top-right HUD or the right-docked shop. Toggling back off worked cleanly.

### 4. A tower-picker panel and the mid-round shop collapse both look right — but I can't confirm how I triggered them

Screenshots: `shot03_full.png`, `shot04_wide.png`.

Early in the session, before I had successfully driven any deliberate
input, I found the game already showing a **"PLACE PROGENITOR · SLOT 3"**
panel floating immediately next to bone-marrow slot 3, with the four
progenitor kinds (Macrophage 40 ATP, Neutrophil 15 ATP, Dendritic cell 30
ATP, Helper-T cell 25 ATP) listed with one-line descriptors and a `close ✕`
— and, simultaneously, a round was **actively running** (`batch 12/16`) with
the shop **collapsed to a header strip** reading "SHOP ▸ (click to buy
mid-round)". Both of these match the spec closely: the picker floats at the
slot rather than docking right (matching the *shipped* behaviour described
in `UI_STYLE_GUIDE.md`, which supersedes the docked-panel alternative
`UI_DESIGN.md` §4 originally proposed), and the shop visibly stays
reachable — collapsed, not gone — during an active round, which is exactly
Sprint 16's headline feature ("Buying is LIVE," `GAME_DESIGN.md` §5d).

I'm reporting this because it's suggestive that the feature works, but I
want to be precise about what I don't know: I did not perform a mouse click
I can point to as the cause. The window had been resized and refocused via
several PowerShell/Win32 calls in the moments before this screenshot, and
it's possible a stray input (or leftover state from before I attached) is
responsible. I was not able to reproduce this deliberately later in the
session despite many attempts (see below), so **treat this as "the feature
appears to exist and look right," not "I verified it interactively."**

### 5. No exceptions in the log; no crash traceable to my input

`agenttest2.log` (the launch this report is based on) stayed at 40 lines
for the whole session — clean boot, `GameBootstrap` diagnostic, then
silence, which is expected (Unity doesn't log per-frame). No stack traces,
no `Exiting batchmode`-style abnormal markers.

The *first* launch (`agenttest.log`) is a separate, minor story: the game
process disappeared partway through my very first round of window-
manipulation calls (before I'd built reliable helpers), and its log shows a
clean shutdown sequence (`CodeReloadManager destroyed`, `Input System...
Shutdown`) with no error — i.e. something asked it to quit, but nothing
crashed. I could not determine what sent the quit signal; my best guess is
an errant click on the window's native close button landed during my early,
uncalibrated coordinate math (see tooling notes), but I did not verify this
and am not confident enough to state it as fact. I relaunched cleanly and
continued on the second process for the rest of the session.

### 6. Nothing else looked visually broken in what I could see

Within the surfaces I could actually observe (HUD, debug readout, shop
list, the one incidental picker panel, the tissue board), I didn't spot
overlapping panels, unreadable text, or panels landing off-screen. The
right-docked shop's five rows (Cytokine sensing, Mucus turnover, Host dsRNA
sensor, Harden vs viral entry, Bacterial resistance, Crypt) were all fully
legible with cost and BUY affordance visible, matching `UI_DESIGN.md` §6's
sketch. I want to flag, though, that this is a *weak* finding — with mouse
input non-functional for me, I never got to see a hover state, a disabled/
unaffordable BUY button, a maxed row, or the upgrade panel's three named
rows at all, all of which are exactly the states most likely to reveal a
layout bug.

---

## What I could not test, and why

- **Clicking a bone-marrow slot** (empty or placed), **any shop BUY
  button**, **the Start Round button**, **the upgrade panel's three rows**,
  or **buying mid-round** — all require a mouse click, and mouse click
  injection did not register with the game in this environment despite
  four different methods (see tooling notes). I could not test item 4 and 5
  of the brief's checklist through deliberate interaction as a result.
- **Placing a Dendritic cell tower on purpose** — this needs a click on an
  empty marrow slot, then a click on "Dendritic cell" in the picker. The
  one DC I observed (Finding 1) was already on the board when I arrived at
  a controllable state; I don't know which slot it came from or when it was
  bought.
- **A controlled, longer observation of DC jitter** — my burst was six
  frames over ~1.75 s; a real characterization (amplitude, whether it's
  screen-space or world-space, whether it correlates with anything) would
  need tens of seconds of dense capture, ideally with the debug readout's
  population/position numbers cross-referenced frame-by-frame.
- **Esc to dismiss a panel**, **the maxed/unaffordable row states**, **the
  upgrade panel's target-field echo in the debug readout** — all downstream
  of the click problem.
- **GAME OVER state** — never got lives to 0; would need either much longer
  passive observation or the ability to intentionally under-defend, which
  isn't really controllable without buy-phase clicks either.

---

## Notes on the tooling

This is the part of the brief I'd weight most heavily, since it's a dry run
of agentic testing itself.

**What worked well:**
- **Launching windowed + `-logFile`** worked exactly as documented.
- **Screenshotting via `System.Drawing`/`CopyFromScreen`** worked reliably
  and read back fine through the `Read` tool — no issues seeing the game
  visually.
- **`SendKeys` for individual key presses** (`Space`, backtick) worked
  reliably once the window had real OS foreground focus. This was the one
  input channel that worked end-to-end.
- **Win32 `FindWindow`-by-title failed** (returned a null handle) but
  **`EnumWindows` filtered by the process's PID** reliably found the real
  window handle, including recovering from a window that `Get-Process
  ...MainWindowHandle` reported as `0`/invisible partway through the
  session (see below).

**What did not work, and cost most of the session:**
- **Mouse click injection never registered with the game**, across four
  distinct methods tried in order: (1) `SetCursorPos` + legacy `mouse_event`
  DOWN/UP at screenshot-pixel coordinates, (2) the same after discovering
  and correcting a **DPI scale mismatch** (this display reports
  `GetSystemMetrics` / `SetCursorPos` in a virtualized ~1707×1067 logical
  space while screen captures come back at physical ~1920×1080 — a ~1.125×
  scale factor I had to derive empirically via `WindowFromPoint`), (3) the
  Win32 `SendInput` API with `MOUSEEVENTF_ABSOLUTE`, correctly scaled, and
  (4) `SendInput` with a stepped sequence of small relative moves toward
  the target before clicking, to rule out a "the game only trusts gradual
  movement" theory. All four left the game in an unchanged state (verified
  by screenshot after each). Hovering a button (no click, just resting the
  cursor over a BUY button for 700 ms) also produced no visible hover
  change, which suggests the game's UI Toolkit event system may not be
  seeing the synthetic pointer at all — as opposed to seeing it but
  rejecting the click specifically. I want to be careful here: **I cannot
  rule out that this is a real bug in how this build reads pointer input**
  (its UI Toolkit `PanelSettings`/`InputSystemUIInputModule` wiring), as
  distinct from a limitation of injecting input from an unattended,
  non-interactive PowerShell session into a foreground game window. The
  fact that keyboard input worked flawlessly through the same window/focus
  state makes me lean towards "synthetic mouse specifically," but this is
  circumstantial, not proven.
- **The window intermittently lost visibility/handle validity.** Partway
  through, `Get-Process ImmunologyTowerDefense | Select MainWindowHandle`
  started returning `0` and `IsWindowVisible` reported `false` for the
  window Win32 could still enumerate by PID, even though the process was
  alive and (per the log) had not restarted. `ShowWindow(hwnd, SW_RESTORE)`
  on the PID-recovered handle fixed it. I don't know what caused the
  window to go invisible — possibly interaction with the other window
  activity on this machine (see next point) — but it's a good example of
  why "the process is running" isn't sufficient to assume "the window is
  interactable."
- **This machine had other active windows competing for focus/z-order
  throughout the session** — at least one other Claude Code terminal
  session was visibly running its own agentic work (editing
  `Rendering/LumenChannelRenderer.cs` and related rendering files) in a
  window that kept regaining foreground focus and covering the game
  window, including at least once fully obscuring it between one of my
  screenshots and the next. `SetWindowPos` with `HWND_TOPMOST` helped keep
  the game visible for screenshots but does **not** grant input focus by
  itself — I still had to `SetForegroundWindow` immediately before every
  keyboard action, and even then, competition from other windows made this
  session noticeably more fragile than a quiet machine would be. **This is
  worth flagging to the Director as an environment factor, not a game
  issue**: testing concurrently with another active agent session sharing
  the same desktop is going to intermittently steal focus.

**What would make this easier next time:**
1. **A documented, known-good mouse-injection method for this specific
   Unity/Input-System configuration** — ideally verified once by a human
   or by the Director, so a future agent isn't rediscovering (or failing to
   discover) it from scratch. If the game's `InputSystemUIInputModule` has
   a setting that specifically ignores synthetic/non-hardware pointer
   events, that's useful to know either way — it would mean mouse-driven
   agentic testing of this build is a dead end regardless of technique, and
   testing plans should route around it (e.g., temporary keyboard-only
   debug shortcuts for the actions a playtest most needs — start round,
   open shop, place a specific tower kind — gated out of release builds).
2. **A dedicated, isolated desktop/VM for agentic playtesting** so window
   focus isn't contended with other concurrent sessions.
3. **Reporting the actual window client resolution in the log** (it
   already logs `Screen 1280x720` — this was very useful for confirming
   what I should be targeting) — more of this kind of ground-truth logging
   would help an agent self-correct coordinate math faster than the
   empirical `WindowFromPoint` trick I ended up using.
4. If future runs need reliable clicking, worth trying **`AutomationId`
   /UI-Automation-tree-based invocation** (Windows UI Automation can often
   invoke a control directly by its accessible name/role rather than by
   coordinate+synthetic-click) as a fifth method not attempted this run,
   *if* UI Toolkit exposes an accessibility tree — it may not.

---

## Summary for the Director

Confirmed working and matching spec: the minimal HUD (readable, live,
correctly laid out), the Space-to-start-round control, and the debug-readout
toggle (backtick, correctly off by default, correct content). Strong
circumstantial evidence — but not a controlled verification — that the
floating tower-picker and the mid-round shop-collapse-to-header-strip both
work and look right. The dendritic-cell jitter bug is very likely still
present: the one DC I watched did not visibly travel over ~1.75 seconds
despite the design intent of a full patrolling sweep, though my sample was
short. No crashes, no exceptions in the log tied to this session's actual
gameplay. The single biggest gap in this report is that I could not click
anything on purpose — every finding about the buy UI (items 4 and 5 of your
checklist) rests on one unexplained early observation rather than something
I drove myself, and I'd treat that gap as the headline result of this dry
run as much as any individual UI finding.

---

# Head's follow-up (2026-09-04, after the run)

The tester's report above is unedited. Two of its open questions are now
closed, and the answers matter more than the findings.

## The clicks were a tooling problem, not a game bug — and the fix is one line

The report's headline gap ("I could not click anything on purpose") had a
mundane cause: **the calling process was not DPI-aware.** This display
reports a virtualised ~1707×1067 logical space to `SetCursorPos` while
screen captures come back at physical 1920×1080, so every click landed
~11% away from where the screenshot said to click. The tester found that
scale factor empirically and corrected for it — but correcting the
*coordinates* while the process stays DPI-unaware fights the same
virtualisation from the other side, which is why all four methods failed
identically, and why hovering produced no hover state either. Nothing was
being clicked; the pointer was elsewhere.

Calling `SetProcessDPIAware()` once, before any `SetCursorPos`, makes
screenshot pixels and cursor coordinates the same thing. With that, the
plain legacy method works first try:

1. Click bone-marrow slot 1 → the slot rims blue and the **PLACE
   PROGENITOR · SLOT 1** picker floats at it (four kinds, prices,
   `close ✕`).
2. Click **BUY** on Macrophage → **ATP 100 → 60**, the slot turns
   macrophage blue, and the panel swaps in place to **PROGENITOR NICHE ·
   SLOT 1** with the portrait, `0 / 10 cells fielded`, and the three real
   roster rows (Efferocytic capacity 30, Tissue residency (M2) 40,
   Pseudopod reach 45) with level dots and buy states.

So checklist items 4 and 5 **are** verified — the picker floats at the
slot, the selection rim works, placing spends ATP, and the panel re-targets
to the tower you just bought without another click. The report's Finding 4
("appears to exist and looks right, but I didn't drive it") was correct to
hedge, and correct in substance.

The method is now written down in `AGENT_HANDBOOK.md` → "Driving the built
game from a shell", so the next agent starts where this one finished.

## The focus contention was my fault, not the machine's

The report flags "another Claude Code session editing rendering files and
stealing focus." That was this session, working on Sprint 17 in the same
repo while the tester played. Dispatching a play-tester and then doing
windowed work on the same desktop is a scheduling mistake; the handbook now
says not to.

## What the run was actually worth

Three things it got that a harness cannot:

- **The DC jitter, confirmed from the outside.** "It sits in the same
  ~15-pixel patch for 1.75 s instead of pacing the lane" is exactly the
  symptom, described without knowing the cause — and it independently
  corroborated the Director's report from a completely cold read of the
  game.
- **A clean audit of the three things it could drive.** HUD live and
  readable, `Space` reliable, debug readout off by default with the right
  content — each tied to a named screenshot.
- **Honest boundaries.** It refused to claim the buy UI worked when it
  could not attribute the panel to a click it made. A report that
  over-claimed there would have been worse than useless, and this is the
  property that decides whether agentic testing is worth doing again.

**Verdict on the dry run: worth repeating**, with the input recipe in hand
and on a quiet desktop. The open question it could not answer — whether
this is *efficient* — is still open: 17 minutes and ~228k tokens bought
three verified checklist items and one confirmed bug, most of the run
having been spent fighting the pointer. The next run should be much
cheaper.
