# Session Handoff — 2026-10-25 (Sprint 23 shipped the adaptive roster; next: rounds and pathogens)

A checkpoint written by the head session. **This is a living file:
overwrite it at the next checkpoint rather than accumulating dated
copies.** It is not one of `WORKFLOW.md` §3's canonical docs; if it
contradicts `SPRINT_PLAN.md`, `ENGINE_STATUS.md` or `INTERFACE.md`, those
win.

## First: which machine are you on?

| | Windows | Linux (CachyOS) |
|---|---|---|
| Clone | `C:\Users\lukef\ImmunologyTowerDefense` | `~/Projects/Biotech/ImmunologyTowerDefense` |
| Shell | PowerShell | fish (scripts are bash, run directly) |
| Unity | `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` | `~/Unity/Hub/Editor/6000.6.4f1/Editor/Unity` |
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` (14 harnesses) |
| Build / serve WebGL | `BuildScript.BuildWebGL`; `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/unity.sh webgl`; `tools/serve_webgl.py` → http://localhost:8000 |
| Look at sprites | — | `tools/unity.sh SpriteShapeDump.DumpAll` → `_spritedump/` (now includes the organ canvases) |

**`git pull` before doing anything.** GitHub is the only sync point.
**The Windows machine still needs Unity `6000.6.4f1`** (via the Hub, with
"Web Build Support") before it opens the project. Opening it in 6000.5.8f1
would try to downgrade the project.

## What shipped this session (2026-10-25, Linux machine)

**Sprint 23: four new towers, and only one of them is a killer.**

The Director asked for research first — six parallel passes against PubMed
and Consensus — on the reasoning that getting the cells right lets rounds
be designed against a stable roster. The research changed two towers
before a line was written, and found one live defect.

| Tower | Gated | Its job |
|---|---|---|
| Cytotoxic T | ~10% knowledge | Precise kill — **the host cell survives** |
| NK cell (ILC1) | no | **IFN-γ: upgrades nearby macrophages**; weak kill |
| γδ T (IEL) | no | Wall patrol + **the only tower that repairs** |
| B cell → plasma cells | ~20% knowledge | Drifting **antibodies** that coat pathogens |

Read `GAME_DESIGN.md` §4c first — it is the source of truth for all four.
The evidence is in `docs/CELL_KINETICS_REFERENCE.md`, with the six
unabridged passes in `docs/research/`.

### The ideas the sprint rests on

- **The CTL's mechanic is not damage.** It is `QuietKill`: the enterocyte
  is still standing afterwards. If a later session collapses the quiet
  branch into the loud one, every test about infections clearing still
  passes and the entire reason to buy adaptive immunity silently vanishes.
  `Sprint23Verification.RunQuietKillSparesTheHostCell` guards it.
- **The NK cell is an amplifier, not a competitor.** Building it as a
  cheap CTL was the obvious design and the wrong biology, and it would
  have cheapened the ~10% rung. Do **not** give it a clonal-expansion
  upgrade — human NK proliferation is unchanged during acute infection.
- **Recognition is a visible cumulative meter**, not a hidden roll, and
  cooperation is superadditive (Halle 2016). Same reasoning that made
  Sprint 22's blister work.
- **Innate recognition was ~800× too generous** and is now ~10× *slower*
  than cytotoxic. §4b always said innate immunity is bad at intracellular
  infection; the number had said the opposite since Sprint 6.

### Two seams fixed first, worth not re-breaking

- **`ProfileFor`/`PoolFor` were binary ternaries.** A third tissue kind was
  a *silent wrong answer*, not a compile error. Now a dictionary.
- **`IsAdaptive` conflated two properties** and a CTL is the first cell
  that is both adaptive and tissue-roaming. Split into `IsNodeResident`
  and `IsKnowledgeGated`. Keep them separate.

### A defect found on the way, relevant to the IgA mystery

**The Sprint 22 secretory-IgA rung was never flipped to `Built`.** For a
whole sprint — including the playtest where the Director reported never
perceiving IgA — the HUD labelled a live mechanic "planned". It does not
by itself explain the miss (the mechanic reads `IgaTuning.KnowledgePercent`,
not the flag), but the one piece of UI that would have told him IgA had
arrived was saying the opposite. Add it to `PLAYTEST_LOG.md`'s three
candidate causes.

## Open questions the Director has not ruled on

1. **Per-species recognition.** A CTL currently ignores species below 10%,
   so your CTLs walk past a novel pathogen. Biologically right, harsher.
   `KnowledgeLicence.PerSpeciesRecognition` flips it in one write.
2. **What one round is in biological time.** Two research passes
   disagreed — 1 day vs 2 days. Belongs with round design.
3. **The 8-bit barcode ceilings the species roster at ~8–10** before tags
   start teaching two species at once. This bites *before* new pathogens
   ship. 16 bits at r≤2 recommended. §5c's claim that the Hamming
   threshold is the dial to reach for first is backwards.

## What is queued next

1. **Round design and new pathogens** — the thing this sprint was clearing
   the way for. `BACKLOG.md`'s Sprint 23 section has the corrections that
   should land alongside (clonal expansion is on the wrong knob; the ~30%
   memory rung is specified backwards).
2. **The MHC-I stealth pathogen** — designed, not built. Under real MHC-I
   downmodulation CTL killing goes to *exactly zero*, so it is a sourced
   hard counter only the NK cell answers. Build it as a cliff; there must
   be no graded stealth stat.
3. **B cells → tertiary lymphoid organs** (the Director's idea), with the
   cited drawback: gut TLOs obstruct lymphatic outflow toward the node.
4. Bone marrow second pass, still queued from Sprint 21.

## State of the code

- **16 harnesses, 603 checks**, green as of Sprint 23's final commit.
  `Sprint23Verification` is seeded.
- New roster numbers: `UnitProfile` defaults in `GameBootstrap`,
  cross-kind values in `Units/UnitTuning`, antibody in
  `Adaptive/AntibodyTuning`, licensing in `Adaptive/KnowledgeLicence`.
- `GutWall.Current` and `KnowledgeLicence.Ledger` are new process-global,
  null-safe statics, same pattern as `GutWallFeatures.Current`.
- **Two sprite traps** cost a rebuild each and are written up in
  `BACKLOG.md`: `InnerShade` with a multiplier above 1 is a no-op on these
  white-silhouette sprites, and the γδ cell's first draft read as a
  **face**. Always dump and look before building.

