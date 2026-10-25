# Session Handoff — 2026-10-25 (Sprints 23–31: the roster, the pathogens, the upgrades)

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

Nine sprints in one sitting. The game went from four cell types and three
nameless pathogen classes to a full roster with named organisms, working
upgrades and a measured economy.

| Sprint | What |
|---|---|
| 23 | Cytotoxic T, NK/ILC1, γδ IEL, B cell → plasma cells + antibodies |
| 24 | Named pathogen **species**; rotavirus, *Vibrio cholerae*, A/E *E. coli* |
| 25 | Finished the class→species jump; smaller populations; complement to the shop; lumen lane-changing |
| 26 | Greek in the font (ImmunoSans); γδ sprite redrawn |
| 27 | Bone marrow expansion — fatty niches you buy by clicking |
| 28 | Crowd desensitisation; 2×4 marrow; shop to the top left |
| 29 | The upgrade pass — four empty towers filled, four dead shop rows woken, 10-bit barcode, memory T cells |
| 30 | **Viruses can reach the base** — they previously could not, at all |
| 31 | Clumping fixed properly, with measurements |

**21 harnesses, 768 checks, green.** WebGL builds clean.

## The habit that produced most of the value

**Measure before tuning.** Four separate times this session, a
"balance problem" turned out to be a structural one, and tuning would
have made it worse:

- Viruses "too weak" → they had **no path to the base at all** (0/150).
- Clumping "needs more repulsion" → the penalty was distance-blind, and
  100× strength changed nothing.
- The kill-limit upgrade → macrophages average 0.9 kills against a
  ceiling of 20. Inert.
- The 8-bit barcode's separation guarantee → **mathematically
  unreachable**, not merely unmet.

The three diagnostics under `ImmunologyTD/Diagnose` exist for exactly
this. They are not in `verify`; run them by hand when a balance question
comes up.

## Second habit worth keeping: assert the property, not a proxy

Several tests passed for sprints while the thing they described was
false — the barcode separation rule, and Sprint 28's dispersal checks
which a random walk would have satisfied. Where it matters now, the
harness enumerates the real property (every tag in the barcode space;
the penalty's gradient one tile out).

## Open, in rough priority order

Full detail in `BACKLOG.md` under "Open after Sprint 31".

1. **BUG — immune cells walk out of the tissue into the blood** at round
   start. Director-reported, diagnosed, not fixed. Nothing constrains an
   immune cell to the tissue band: `Chemotaxis.ChooseNextStep` tests only
   `InFineBounds`. Careful — the lymph node is *in* the base band, so DCs
   and lymphocytes legitimately go there; the fix must be per-kind.
2. **Kill-limit rows are inert** (measured). Fixing means deciding whether
   innate cells should turn over at all — a Director call, not a number.
3. **Round design and new pathogens** — the thing the last several sprints
   were clearing the way for. The shortlist is ranked and cited in
   `PATHOGEN_ROSTER.md`; astrovirus is the standout (breach with no
   infection and no stress signal — it defeats the whole detection stack
   by not being detectable).
4. **Epithelial extrusion** + the messy-death fork, designed not built.
5. Virus march speed is incubation-bound (~185s) if it still reads slow.

## Where things live now

- `docs/PATHOGEN_ROSTER.md` — named organisms, the shortlist, rejections.
- `docs/CELL_KINETICS_REFERENCE.md` — the numbers behind the roster.
- `docs/research/` — 11 unabridged research passes, ~250 resolved
  citations. Research INPUTS, not design; `GAME_DESIGN.md` wins conflicts.
- `Grid/HostDefence.cs` — the body shop's host-side effects.
- `Units/UnitTuning.cs` — cross-kind numbers (crowding, IFN-γ).
- `Pathogens/PathogenSpecies.cs` — the species roster and its traits.

## Two traps this session fell into twice each

- **A sprite that only gets looked at alone will pass.** The γδ cell
  shipped as a rod-shaped bacterium, and intermediate drafts read as a
  rabbit and a domino. Always dump magnified AND composite against the
  *pathogens*, not just the other towers.
- **An upgrade that writes nothing still charges.** Three dead upgrades
  found so far, all the same shape: a ceiling that never binds. Any new
  cap-style row needs its steady state checked first.
