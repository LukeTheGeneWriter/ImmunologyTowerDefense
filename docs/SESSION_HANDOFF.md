# Session Handoff — 2026-11-02 (Sprint 32: the permit, the curriculum, turnover)

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
| Run harnesses | `Unity.exe -batchmode -quit -projectPath game -executeMethod <X>`, then grep the log | `tools/unity.sh verify` (23 methods) |
| Build / serve WebGL | `BuildScript.BuildWebGL`; `powershell -ExecutionPolicy Bypass -File tools\serve_webgl.ps1` | `tools/unity.sh webgl`; `tools/serve_webgl.py` → http://localhost:8000 |
| Look at sprites | — | `tools/unity.sh SpriteShapeDump.DumpAll` → `_spritedump/` |

**`git pull` before doing anything.** GitHub is the only sync point.
**The Windows machine still needs Unity `6000.6.4f1`** (via the Hub, with
"Web Build Support") before it opens the project.

## What shipped this session (2026-11-02, Linux machine)

Sprint 32, one sitting, three strands. The Director picked "rounds and new
pathogens" with the edge bug first, and said yes to innate turnover.

| Strand | What |
|---|---|
| A | **The edge bug is fixed.** Per-kind band permit in `Chemotaxis.ChooseNextStep`. |
| B | **Innate cells turn over.** Macrophage kill limit 20 → 5, neutrophil 5 → 3, from a measured distribution. |
| C1 | **Named species can spawn at all** — they never could. Rounds carry a cast of species. |
| C2 | **A round curriculum**: one new organism at a time, organisms recur, wall pressure climbs. |
| C3 | **Astrovirus: not built**, deliberately. Scoped in `BACKLOG.md`. |
| + | **A round-hang fixed before it shipped** — the scaled batch ran into the spawner's 150-pathogen ceiling, and a food round never ends if its batch cannot be emitted. Clamped to 90. |

**23 verify methods, 827 checks, green** (`Sprint32Verification` is 59 of
them). WebGL builds clean.

## The finding that mattered most

**Rotavirus, *Vibrio cholerae* and A/E *E. coli* had never been on the
board.** `PathogenSpawner.SpawnFromFood` passed a `PathogenClass`, and a
bare class resolves to the *generic* species of that class; nothing in
`Assets/Scripts` ever passed a named species except replication, which
inherits its parent's. All of Sprints 24-25 — traits, sprites,
knowledge-HUD rows — was unreachable, while the round taglines promised
*Salmonella*, poliovirus and *Listeria*, none of which are in the roster.

That is the **fifth** instance of one pattern: three upgrade ceilings that
never bound, one barcode guarantee that was mathematically unreachable,
and now this. The signature every time: **a value written on one side of a
boundary and never read as that value on the other.** Worth asking of any
new feature — who reads this, and with what?

## Numbers worth keeping

- Immune cells spent **43% of their steps outside the tissue** under the
  old movement rule (34,402 of 80,000). Now 0. A stray cell homes in 38
  steps worst case, and cells still diffuse (mean depth 2.2 of 13 cells).
- Round 5 now delivers, through the real spawner: **32 A/E *E. coli*, 11
  intracellular bacteria, 9 rotavirus, 8 gut bacteria**.
- Kills per cell with the ceiling lifted: macrophage **median 2** per 45s
  round, **9** per three; neutrophil **3** and **11**.
- The macrophage's old ceiling of 20 retired **0 cells even over three
  rounds**. The neutrophil's 5 retired 7 of 11 over three rounds — so
  that row was never inert, which corrects the standing claim.

## Open, in rough priority order

Full detail in `BACKLOG.md` under "Open after Sprint 32".

1. **The Director's playtest of this build.** Three specific questions:
   does the curriculum read as teaching; is IgA perceptible now that wall
   pressure climbs (the Sprint 22 miss was diagnosed as round design, and
   this is the fix being tested); does the board still feel populated with
   turnover live.
2. **Astrovirus** — the next pathogen, now on working rails. More than a
   roster entry: it needs a non-infecting virus path in `PathogenAgent`,
   a barrier interaction, a sprite pass, and the barcode search re-run for
   the whole seven-species set. **And a design question first:** it
   produces no debris and no stress signal, so no knowledge can ever
   accrue from it — what does its knowledge-HUD row say?
3. **The structural gap behind the dead-upgrade pattern**: no harness can
   read the shipped per-kind unit numbers (they live on `GameBootstrap`'s
   serialized fields; a harness cannot stand that component up), so
   nothing compares an upgrade row's step against the base it applies to.
   A `UnitProfileAudit` on the `UpgradeAudit` precedent closes it.
4. **Degranulation × A/E *E. coli*** — faster neutrophil turnover makes
   more debris, and that organism respires exactly that. Both landed this
   sprint; the compound effect is unmeasured.
5. **`docs/INTERFACE.md` is ten sprints stale** (last per-sprint section
   before Sprint 32: Sprint 21). It is a `WORKFLOW.md` §5.3 checkbox that
   was being ticked without being true.
6. Kill limits on the other four kinds are **unmeasured** (CTL 16, NK 10,
   γδ 12). The board now drifts toward long-lived adaptive and
   short-lived innate cells, which nobody has decided.

## Where things live now

- `Grid/BoardConfig.cs` — `BandPermit`, `BandOfFine`, `BandAxisRange`,
  `FineDistanceToPermittedBand`. Band extents have one home now.
- `Units/Chemotaxis.cs` — `CollectCandidates`, the three-pass permit rule.
- `Rounds/RoundScript.cs` — the curriculum and `SpeciesWeight`.
- `Editor/Sprint32Verification.cs` — 53 checks in three parts.
- `Editor/KillLimitDiagnostic.cs` — now has a distribution mode.

## Traps this session hit

- **A test can fail because the test is wrong.** The first no-excursion
  check also demanded cells cross the whole tissue band in 600 steps; an
  unbiased walk needs ~8,000 to cover 91 fine tiles, so it was testing the
  step budget. Restated as diffusion.
- **Every harness leaks units into `SearchUnit`'s static registry**
  (`ResetForPool` is the only exit), which moved a measurement from 4 to 3
  depending on how many runs preceded it. `Sprint32Verification` drains it
  and asserts run-to-run determinism; the others still leak.
- **A batchmode CRASH can be a transient build-system error.**
  `Sprint24Verification` reported `Internal build system error … binlog`
  then "Scripts have compiler errors" in a run where every other harness
  compiled; it passed on a bare re-run. A real compile error fails every
  method in the run, not one.
- **Two older harnesses owned numbers they had no reason to own** —
  `Sprint18Verification` asserted `20 -> 28` and `5 -> 7` as literals, so
  a turnover change broke tests that were not about turnover. Both now
  read their own fixture base plus the live per-level constant.
