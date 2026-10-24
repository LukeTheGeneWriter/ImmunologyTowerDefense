using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;
using ImmunologyTD.Economy;
using ImmunologyTD.Rounds;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.Bootstrap
{
    /// <summary>
    /// Entry point. Builds the whole scene at runtime -- camera, the coarse
    /// cell grid, the gut wall, the base-band compartments, unit/pathogen
    /// pools, HUD -- from a single GameObject (see
    /// Assets/Editor/SceneSetup.cs). Nothing is hand laid out in the Editor;
    /// see docs/INTERFACE.md for the data shapes wired together here.
    ///
    /// **Sprint 4 rebuilt the layout around Map 01 (GAME_DESIGN.md section
    /// 1a).** Bone marrow and the lymph node were free-floating strips below
    /// and to the right of a 30x5 tissue board; they now sit INSIDE the base
    /// band of a 100x40 map, which is the whole point of item 8 -- the
    /// player's production and the endzone are the same compartment, and it
    /// should look that way. Camera framing is correspondingly simpler:
    /// every compartment is inside the board now, so the camera fits the
    /// board and nothing else.
    /// </summary>
    [RequireComponent(typeof(BoardConfig))]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Units -- each type's own configurable fine-tiles-per-tick speed")]
        [SerializeField]
        private UnitProfile macrophageProfile = new UnitProfile
        {
            Kind = UnitKind.Macrophage,
            DisplayName = "Macrophage",
            FineTilesPerTick = 1,
            FootprintFineTiles = 5,
            Color = new Color(0.30f, 0.40f, 0.80f),
            MaxActiveChildren = 10,
            KillLimit = 20,
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 2,
            // ~2.5s to eat a full debris pile it stands on -- roughly 20x
            // faster than the 60s self-dissipation, so efferocytosis is the
            // better answer without self-dissipation being a dead option.
            // Judgment call, mechanics-first (see docs/TEAM_RETRO.md).
            EfferocytosisDebrisPerTick = 0.05f,
            // GAME_DESIGN.md §4b: generic stress sensing, deliberately weak.
            //
            // **Sprint 23 cut this 0.03 -> 0.003** (Director, 2026-10-25).
            // 0.03/tick meant a mean 4.0s of contact to clear an infected
            // cell -- while a real CTL, which actually HAS MHC-I-restricted
            // recognition, kills with p=0.15 per ~9-minute contact (Halle
            // 2016, Immunity, 10.1016/j.immuni.2016.01.010, two-photon in
            // vivo). Our macrophage, which has no such recognition at all,
            // was clearing infections ~800x faster than a real CTL. §4b says
            // in plain language that established intracellular infection is
            // "the thing innate immunity is *bad at*, on purpose"; the number
            // never implemented the sentence. 0.003 = a mean ~40s of contact.
            StressSenseChancePerTick = 0.003f
        };

        [SerializeField]
        private UnitProfile neutrophilProfile = new UnitProfile
        {
            Kind = UnitKind.Neutrophil,
            DisplayName = "Neutrophil",
            // Sprint 23: 3 -> 5. Measured interstitial neutrophil crawling is
            // ~7.3 um/min (Zinselmeyer 2008) against a tissue macrophage's
            // ~1 um/min (van den Bos 2020) -- a 7-10x gap where the game was
            // showing 3x. 5 is the ceiling of the useful integer ladder.
            FineTilesPerTick = 5,
            FootprintFineTiles = 3,
            Color = new Color(0.93f, 0.74f, 0.30f), // Sprint 13: nudged amber -> gold, away from hazard-orange

            MaxActiveChildren = 10,
            KillLimit = 5,
            DegranulatesOnDepletion = true,
            DegranulationBurstMultiplier = 3f,
            ContactRadiusFineTiles = 2,
            EfferocytosisDebrisPerTick = 0f, // neutrophils do not clear debris -- macrophage's job (section 1c)
            // Slightly weaker than the macrophage's -- a neutrophil is a
            // blunter instrument -- but non-zero: it is still an innate cell
            // that can stumble onto a stressed cell.
            // Sprint 23: 0.02 -> 0.002, same rescale as the macrophage above.
            StressSenseChancePerTick = 0.002f
        };

        // ---------------------------------------------------------------
        // Sprint 23 -- the adaptive and unconventional roster.
        // Every number below is sourced in docs/CELL_KINETICS_REFERENCE.md.
        // Speeds come from that doc's ratio table, which normalises measured
        // two-photon velocities against the macrophage at 1.
        // ---------------------------------------------------------------

        /// <summary>The ~10% knowledge rung made flesh. Its mechanic is NOT
        /// damage -- it is <see cref="UnitProfile.QuietKill"/>: the enterocyte
        /// survives. Licensed per species by the knowledge ledger.</summary>
        [SerializeField]
        private UnitProfile cytotoxicTProfile = new UnitProfile
        {
            Kind = UnitKind.CytotoxicT,
            DisplayName = "Cytotoxic T",
            // Effector CD8 in mouse small-intestinal villi: 6.5 um/min
            // (Torres 2023 eLife, 10.7554/eLife.84916) ~= 6x the macrophage;
            // compressed to 3 because the integer ladder tops out at 5 and
            // the neutrophil needs the top rung.
            FineTilesPerTick = 3,
            FootprintFineTiles = 3,
            Color = new Color(0.78f, 0.26f, 0.32f),
            MaxActiveChildren = 8,
            KillLimit = 16,
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 2,
            EfferocytosisDebrisPerTick = 0f,
            // 10x the (rebalanced) macrophage: a mean ~4s of contact. The
            // Director's call was a 10-15x gap, explicitly not more -- past
            // ~20x the innate roll stops reading as a mechanic at all.
            StressSenseChancePerTick = 0.03f,
            QuietKill = true,
        };

        /// <summary>"NK cell (ILC1)". **Primarily a macrophage amplifier.**
        /// Intestinal NK/ILC1s are published as less cytotoxic and higher
        /// IFN-gamma than blood NK cells, and ILC1s lack perforin and
        /// granzyme B outright -- so the weak kill is the side job and the
        /// aura is the tower.</summary>
        [SerializeField]
        private UnitProfile naturalKillerProfile = new UnitProfile
        {
            Kind = UnitKind.NaturalKiller,
            DisplayName = "NK cell (ILC1)",
            FineTilesPerTick = 3, // 6-7 um/min, Garrod 2007 PNAS
            FootprintFineTiles = 3,
            Color = new Color(0.46f, 0.30f, 0.62f),
            MaxActiveChildren = 6,
            KillLimit = 10,
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 2,
            EfferocytosisDebrisPerTick = 0f,
            // A THIRD of the CTL's, and quiet. Needs no knowledge, but it is
            // blind to an infected cell that keeps its MHC-I up -- which is
            // the CTL's job and, later, the stealth pathogen's counter.
            StressSenseChancePerTick = 0.01f,
            QuietKill = true,
            // The real tower: upgrade the macrophages around it.
            IfnGammaAuraChancePerTick = 0.02f,
            IfnGammaAuraCoarseRadius = 3,
        };

        /// <summary>Intraepithelial gamma-delta T cell. **Not DPS.** It
        /// patrols the wall, intercepts adherents before they get in, and
        /// repairs the barrier. Needs no priming -- the one T cell that can
        /// exist before the lymph node is online (GAME_DESIGN.md §4).</summary>
        [SerializeField]
        private UnitProfile gammaDeltaTProfile = new UnitProfile
        {
            Kind = UnitKind.GammaDeltaT,
            DisplayName = "gd T (IEL)",
            // Deliberately 1, against the real ratio: it is tissue-RESIDENT,
            // already at the wall, so search speed is beside the point and a
            // high number would read as "another fast hunter".
            FineTilesPerTick = 1,
            FootprintFineTiles = 3,
            Color = new Color(0.20f, 0.58f, 0.46f),
            MaxActiveChildren = 6,
            KillLimit = 12,
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 3, // it reaches along the epithelium
            EfferocytosisDebrisPerTick = 0f,
            // Modest, and quiet -- it is a surveillance cell, and Edelblum
            // 2015 shows its value is interception and barrier integrity,
            // not kill throughput.
            StressSenseChancePerTick = 0.008f,
            QuietKill = true,
            // Its real job: hold the wall and mend it.
            BarrierPatrolRadiusCells = 2.0f,
            BarrierRepairIntervalSeconds = 3.5f,
        };

        /// <summary>Sessile antibody factory emitted by the B-cell tower.
        /// Motility is inversely related to antibody-secretion capacity
        /// (Rakhymzhan 2024), so a working plasma cell does not move.</summary>
        [SerializeField]
        private UnitProfile plasmaCellProfile = new UnitProfile
        {
            Kind = UnitKind.PlasmaCell,
            DisplayName = "Plasma cell",
            FineTilesPerTick = 0, // sessile
            FootprintFineTiles = 3,
            Color = new Color(0.92f, 0.55f, 0.20f),
            MaxActiveChildren = 5,
            KillLimit = 9999, // it never kills; depletion is by lifespan
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 1,
            EfferocytosisDebrisPerTick = 0f,
            StressSenseChancePerTick = 0f, // no recognition of its own
            AntibodySecretionIntervalSeconds = 1.1f, // its entire job
        };

        /// <summary>Bone marrow slot count -- unchanged from Sprint 2, a
        /// judgment call (see docs/TEAM_RETRO.md).</summary>
        private const int BoneMarrowSlotCount = 5;

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private GutWall gutWall;
        private InvasionTally tally;

        private struct Layout
        {
            public Rect Bounds;
            public Vector3[] MarrowSlotPositions;
            public float MarrowSlotSize;
            public Vector3 MarrowBackdropCenter;
            public Vector2 MarrowBackdropSize;
            public Vector3 LymphCenter;
            public Vector2 LymphSize;
        }

        /// <summary>
        /// Shouts if the band layout is degenerate. This is here because it
        /// already bit once: the scene asset still carried Sprint 1's
        /// serialized `columns: 30`, which silently overrode Map 01's
        /// default of 100. Since BaseBandCells and LumenBandCells are
        /// clamped against the axis length, the shortfall landed entirely on
        /// the tissue band -- 25 base + 5 lumen + **0 tissue** -- so
        /// pathogens piled up on a gut wall with nothing behind it and
        /// nothing could ever enter tissue. The game ran, rendered, and
        /// reported no errors; the only tell was the HUD's band readout.
        ///
        /// A silent zero-width playfield is worth an error in the log.
        /// </summary>
        private void WarnOnDegenerateBands()
        {
            if (board.TissueBandCells <= 0)
            {
                Debug.LogError(
                    $"[GameBootstrap] Tissue band is {board.TissueBandCells} cells wide -- there is no playfield. " +
                    $"Axis length {board.AxisLength} cannot hold base {board.BaseBandCells} + lumen {board.LumenBandCells}. " +
                    "Check BoardConfig's serialized columns/rows on the scene object (GAME_DESIGN.md section 1a: Map 01 is 100x40).");
            }
            else if (board.LumenBandCells <= 0 || board.BaseBandCells <= 0)
            {
                Debug.LogError(
                    $"[GameBootstrap] Degenerate band layout: base {board.BaseBandCells}, tissue {board.TissueBandCells}, " +
                    $"lumen {board.LumenBandCells} on an axis of {board.AxisLength}.");
            }
        }

        private AtpWallet wallet;
        private KnowledgeLedger knowledge;
        private ShopLedger shop;

        private void Awake()
        {
            board = GetComponent<BoardConfig>();
            WarnOnDegenerateBands();

            // Sprint 13: the procedural shape sprites for each unit kind
            // (runtime-generated, so not settable in the serialized profile
            // initializer -- see UnitProfile.Shape).
            // Sprint 15: generate every shape up front (BACKLOG item) -- the
            // compartment renderers below add ~9 more rasters, and one known
            // boot cost beats a first-of-kind hitch mid-round.
            // Unit colours come from the palette (Sprint 20), overriding
            // whatever the scene serialised -- a stale serialized field beats
            // a C# default.
            macrophageProfile.Color = ImmunologyTD.Rendering.Palette.Macrophage;
            neutrophilProfile.Color = ImmunologyTD.Rendering.Palette.Neutrophil;
            ImmunologyTD.Rendering.SpriteShapes.Prewarm();
            macrophageProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.Macrophage;
            neutrophilProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.Neutrophil;
            // Sprint 23 roster.
            cytotoxicTProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.CytotoxicT;
            naturalKillerProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.NaturalKiller;
            gammaDeltaTProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.GammaDeltaT;
            plasmaCellProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.PlasmaCell;
            tissueGrid = new TissueGrid(board);
            // Sprint 19: the Peyer's patch + crypts. Created before the lumen
            // renderer binds (it leaves the patch lanes bare of villi).
            GutWallTuning.ResetToDefaults();
            ImmunologyTD.Pathogens.Peristalsis.ResetToDefaults();
            GutWallFeatures.Current = new GutWallFeatures(board);
            cytokineField = new CytokineField(board);
            tally = new InvasionTally();
            gutWall = new GutWall(board, tissueGrid, tally);
            GutWall.Current = gutWall; // Sprint 23: gamma-delta IELs reach the wall through this

            // Sprint 7: the ATP economy + round loop (GAME_DESIGN.md §5b/§5d/§6c).
            EconomyTuning.ResetToDefaults();
            wallet = new AtpWallet(EconomyTuning.StartingAtp);
            EconomyHooks.PayForKill = () => wallet.Grant(EconomyTuning.AtpPerKill);

            // Sprint 8: the dendritic-cell shuttle + antigen barcode
            // (GAME_DESIGN.md §5a/§5c). Knowledge % persists for the run.
            AdaptiveTuning.ResetToDefaults();
            knowledge = new KnowledgeLedger();
            // Sprint 23: the first thing in the game that reads knowledge and
            // changes BEHAVIOUR rather than just the HUD readout. Process-
            // global and null-safe, so harnesses that never set it keep
            // licensing everything exactly as before.
            ImmunologyTD.Adaptive.KnowledgeLicence.Ledger = knowledge;
            // Secretory IgA is the only capability that acts in the lumen, so
            // the wall needs to see knowledge directly (KnowledgeLadder's 70%
            // rung).
            gutWall.BindKnowledge(knowledge);

            // Sprint 11: the placeholder buy-phase shop.
            ShopTuning.ResetToDefaults();
            shop = new ShopLedger();

            // Sprint 5: the host layer heals on its own clock, independent
            // of whether anything is invading (GAME_DESIGN.md section 1c).
            gameObject.AddComponent<TissueDriver>().Bind(tissueGrid);

            // Sprint 9: the buy phase freezes time. RoundClockDriver advances
            // the sim clock only while a round is Active (RoundController sets
            // RoundClock.Frozen); every Update()-driven system checks it.
            RoundClock.Reset();
            gameObject.AddComponent<RoundClockDriver>();

            var layout = BuildLayout();

            BuildCamera(layout.Bounds);
            BuildPlateFrame(layout.Bounds);
            BuildBoardVisual();
            BuildLumenChannel();
            BuildTissueUnderlay();
            BuildBaseCompartment();
            BuildGutWallVisual();
            BuildBoneMarrowBackdrop(layout);
            BuildLymphNodeBackdrop(layout);

            var pathogenTemplate = BuildPathogenTemplate();
            BuildPathogenSpawner(pathogenTemplate);

            var macrophagePool = BuildUnitPool(macrophageProfile);
            var neutrophilPool = BuildUnitPool(neutrophilProfile);
            var adaptive = BuildAdaptiveDirector(layout);
            var boneMarrow = BuildBoneMarrowManager(layout, macrophagePool, neutrophilPool, adaptive);

            var rounds = BuildRoundController(boneMarrow);

            BuildDegranulationFlashPool();
            BuildAntibodyPool();
            BuildUiRoot(layout, boneMarrow, rounds, adaptive);

            Debug.Log(
                $"[GameBootstrap] Map 01 -- {board.Columns}x{board.Rows} coarse cells; " +
                $"base axis 0..{board.BaseBandCells - 1}, tissue {board.TissueBaseEdgeAxisIndex}..{board.TissueLumenEdgeAxisIndex}, " +
                $"lumen {board.LumenNearWallAxisIndex}..{board.AxisLength - 1}; " +
                $"base end {board.BaseEnd}, threat axis {board.ThreatAxis}\n" +
                $"  camera pos ({Camera.main.transform.position.x:F3}, {Camera.main.transform.position.y:F3}), " +
                $"orthoSize {Camera.main.orthographicSize:F3}, aspect {Camera.main.aspect:F4}\n" +
                $"  BoneMarrowSlot[0] world ({layout.MarrowSlotPositions[0].x:F3}, {layout.MarrowSlotPositions[0].y:F3}), slotSize {layout.MarrowSlotSize:F3}\n" +
                $"  LymphNode world ({layout.LymphCenter.x:F3}, {layout.LymphCenter.y:F3})");
        }

        /// <summary>
        /// Places the two base-band compartments. Everything is derived from
        /// BoardConfig.BandWorldRect(BoardBand.Base), so the layout follows
        /// the band wherever config puts it rather than assuming "left."
        ///
        /// Bone marrow is a COLUMN of slots rather than Sprint 2's
        /// horizontal strip: the base band is tall and narrow (25 x 40 cells
        /// on Map 01), and stacking the slots vertically both fits that
        /// shape and lines each tower up with the lanes its cells will walk
        /// out into.
        /// </summary>
        private Layout BuildLayout()
        {
            var baseRect = board.BandWorldRect(BoardBand.Base);

            // SPRINT_PLAN.md item 6: the 25x10 resize left this sized for the
            // old 100x40 proportions -- the marrow strip alone was taller
            // than the whole base band, so it spilled into the tissue band
            // and overlapped the lymph node. Everything below is derived as a
            // FRACTION of baseRect with explicit non-overlapping vertical
            // budgets, so a later map resize cannot reproduce the spill:
            //   4% top margin | marrow (62%) | 4% gap | lymph (30%) | (nothing)
            float innerTop = baseRect.yMax - baseRect.height * 0.04f;
            float marrowRegionH = baseRect.height * 0.62f;
            float lymphRegionH = baseRect.height * 0.30f;

            float marrowCenterX = baseRect.center.x;
            float pitch = marrowRegionH / BoneMarrowSlotCount;
            // Square slots: cap the size against the band's WIDTH too, so a
            // narrow base band shrinks the slots rather than overflowing
            // sideways. Whatever the pitch does not spend on the slot is the
            // gap between slots.
            float marrowSlotSize = Mathf.Min(pitch * 0.80f, baseRect.width * 0.62f);

            float topSlotY = innerTop - marrowSlotSize * 0.5f;
            var slotPositions = new Vector3[BoneMarrowSlotCount];
            for (int i = 0; i < BoneMarrowSlotCount; i++)
            {
                slotPositions[i] = new Vector3(marrowCenterX, topSlotY - i * pitch, 0f);
            }

            float marrowStripHeight = (BoneMarrowSlotCount - 1) * pitch + marrowSlotSize;
            float marrowCenterY = topSlotY - (BoneMarrowSlotCount - 1) * pitch * 0.5f;

            // Sprint 21 (spec §1.2): a wider block of bone (side lanes of
            // spongy holes either side of the pads). Height keeps 0.30 pitch
            // of margin (the spec's 0.15 let the end cavities break through).
            var marrowBackdropSize = new Vector2(
                Mathf.Min(marrowSlotSize * 2.9f, baseRect.width * 0.50f),
                marrowStripHeight + pitch * 0.30f);
            var marrowBackdropCenter = new Vector3(marrowCenterX, marrowCenterY, 0f);

            var lymphSize = new Vector2(baseRect.width * 0.82f, lymphRegionH);
            var lymphCenter = new Vector3(
                marrowCenterX,
                baseRect.yMin + baseRect.height * 0.04f + lymphRegionH * 0.5f,
                0f);

            // Every compartment is inside the board now, so the camera fits
            // the board itself -- no more union-of-scattered-strips bounds.
            var bounds = Rect.MinMaxRect(
                -board.BoardWorldWidth * 0.5f, -board.BoardWorldHeight * 0.5f,
                board.BoardWorldWidth * 0.5f, board.BoardWorldHeight * 0.5f);

            return new Layout
            {
                Bounds = bounds,
                MarrowSlotPositions = slotPositions,
                MarrowSlotSize = marrowSlotSize,
                MarrowBackdropCenter = marrowBackdropCenter,
                MarrowBackdropSize = marrowBackdropSize,
                LymphCenter = lymphCenter,
                LymphSize = lymphSize,
            };
        }

        private Camera mainCamera;

        private void BuildCamera(Rect bounds)
        {
            var camGo = new GameObject("MainCamera");
            mainCamera = camGo.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.backgroundColor = ImmunologyTD.Rendering.Palette.CameraClear;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";

            FitCamera(mainCamera, bounds);
            StartCoroutine(RefitCameraNextFrame(bounds));
        }

        /// <summary>
        /// Fits the camera's orthographic size to <paramref name="bounds"/>.
        /// Called once immediately and again a frame later, because
        /// Camera.aspect read during Awake() (frame 0) does not reliably
        /// match the real runtime window -- see docs/ENGINE_STATUS.md for
        /// the Sprint 2 story behind this.
        ///
        /// Padding is tighter than Sprint 3's 1.1: the field is 2.5:1 and
        /// nearly fills a 16:9 frame on its own, so generous padding wastes
        /// pixels the 40 lanes need.
        /// </summary>
        private static void FitCamera(Camera cam, Rect bounds)
        {
            Vector2 center = bounds.center;
            float aspect = cam.aspect > 0 ? cam.aspect : 16f / 9f;
            cam.orthographicSize = CameraHalfHeight(bounds, aspect);
            cam.transform.position = new Vector3(center.x, center.y, -10f);
        }

        /// <summary>Sprint 21: the strip above and below the board
        /// (each at least this fraction of the screen height) is where all
        /// the permanent UI lives (COSMETIC_PASS_SPEC.md §0).</summary>
        public const float UiGutterFraction = 0.15f;

        /// <summary>Orthographic half-height that fits the board with 4%
        /// padding AND keeps a <see cref="UiGutterFraction"/> strip above and
        /// below it. At 16:9 the 2.5:1 board already leaves ~15.8%, so this
        /// is a no-op there; at 21:9 it used to leave 5% and the HUD and
        /// shop would have sat on the board. Pure, so a harness checks it.</summary>
        public static float CameraHalfHeight(Rect bounds, float aspect)
        {
            const float padding = 1.04f;
            float byHeight = bounds.height * 0.5f * padding;
            float byGutter = bounds.height * 0.5f / (1f - 2f * UiGutterFraction);
            float byWidth = bounds.width * 0.5f * padding / Mathf.Max(0.01f, aspect);
            return Mathf.Max(byHeight, Mathf.Max(byGutter, byWidth));
        }

        private System.Collections.IEnumerator RefitCameraNextFrame(Rect bounds)
        {
            yield return null;
            if (mainCamera == null) yield break;
            FitCamera(mainCamera, bounds);
            Debug.Log(
                $"[GameBootstrap] Post-refit camera diagnostic -- pos ({mainCamera.transform.position.x:F3}, {mainCamera.transform.position.y:F3}), " +
                $"orthoSize {mainCamera.orthographicSize:F3}, aspect {mainCamera.aspect:F4}, Screen {Screen.width}x{Screen.height}");
        }

        /// <summary>One SpriteRenderer per coarse cell -- 4,000 of them on
        /// Map 01, up from 150. SPRINT_PLAN.md item 2 names this as the
        /// sprint's main performance risk; measured frame cost is in
        /// docs/ENGINE_STATUS.md.</summary>
        private void BuildBoardVisual()
        {
            var container = new GameObject("HostCellGrid").transform;
            var views = new SpriteRenderer[board.Columns, board.Rows];
            float size = board.CoarseCellWorldSize * 0.94f;

            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    var coord = new CoarseCoord(col, row);
                    // Sprint 15: only the tissue band gets a per-cell
                    // SpriteRenderer now -- the base and lumen bands are drawn
                    // by BaseCompartmentRenderer / LumenChannelRenderer, which
                    // removes ~half the grid's renderers (and BoardRenderer
                    // stops recolouring them every 0.15 s). Host cells only
                    // ever exist in the tissue band (TissueGrid.IsHostGround),
                    // so nothing on the host layer is lost.
                    if (board.BandOf(coord) != BoardBand.Tissue) { views[col, row] = null; continue; }

                    var cellGo = new GameObject($"Cell_{col}_{row}");
                    cellGo.transform.SetParent(container, false);
                    cellGo.transform.position = board.CoarseToWorldCenter(coord);
                    cellGo.transform.localScale = new Vector3(size, size, 1f);
                    var sr = cellGo.AddComponent<SpriteRenderer>();
                    sr.sprite = ImmunologyTD.Rendering.SpriteShapes.HostCell; // Sprint 13 -- BoardRenderer overrides per host state
                    sr.sortingOrder = 0;
                    views[col, row] = sr;
                }
            }

            var boardRenderer = gameObject.AddComponent<BoardRenderer>();
            boardRenderer.Bind(board, tissueGrid, cytokineField, views);
        }

        private void BuildGutWallVisual()
        {
            var go = new GameObject("GutWallRenderer");
            var renderer = go.AddComponent<GutWallRenderer>();
            renderer.Bind(board, gutWall);
        }

        /// <summary>Sprint 21 (COSMETIC_PASS_SPEC.md §3): the board as a
        /// textbook figure plate -- one 9-sliced renderer whose window is
        /// the board, with a dark ink edge, a baked drop shadow, and
        /// desk-coloured corners that round off the board's square ones. It
        /// sits above everything on the board (order 40, over the FX at 30)
        /// so a unit crossing a corner is clipped by the frame, which is the
        /// accepted cost.</summary>
        private void BuildPlateFrame(Rect bounds)
        {
            var go = new GameObject("PlateFrame");
            go.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.SpriteShapes.PlateFrame;
            sr.drawMode = SpriteDrawMode.Sliced;
            float m = ImmunologyTD.Rendering.SpriteShapes.PlateMarginWorld;
            sr.size = new Vector2(bounds.width + 2f * m, bounds.height + 2f * m);
            sr.color = ImmunologyTD.Rendering.Palette.CameraClear;
            sr.sortingOrder = 40;
        }

        /// <summary>Sprint 20 (ART_DIRECTION.md §3.2): a flat
        /// quad under the tissue grid, so the gaps between the rounded host
        /// cells read as pink mortar rather than the camera clear showing
        /// through as black lines. +1 renderer.</summary>
        private void BuildTissueUnderlay()
        {
            var rect = board.BandWorldRect(BoardBand.Tissue);
            if (rect.width <= 0f) return;
            var go = new GameObject("TissueUnderlay");
            go.transform.position = new Vector3(rect.center.x, rect.center.y, 0f);
            go.transform.localScale = new Vector3(rect.width, rect.height, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.RuntimeSprites.SquareSprite;
            sr.color = ImmunologyTD.Rendering.Palette.TissueUnderlay;
            sr.sortingOrder = -1;
        }

        /// <summary>Sprint 15: the lumen stops being tinted host-cell quads
        /// and becomes an open channel -- a chyme field, a mucus wall layer,
        /// and drifting particulate (docs/COMPARTMENT_DESIGN.md §2.1).</summary>
        private void BuildLumenChannel()
        {
            var go = new GameObject("LumenChannelRenderer");
            go.AddComponent<LumenChannelRenderer>().Bind(board);
            // Sprint 19: Peyer's patch dome / M cells / follicle, and crypts.
            new GameObject("GutWallFeatureRenderer").AddComponent<GutWallFeatureRenderer>()
                .Bind(board, GutWallFeatures.Current);
        }

        /// <summary>Sprint 15: the base band becomes the bloodstream --
        /// plasma field, endothelial vessel wall, erythrocyte streamers,
        /// marrow birth-puffs, and an acute breach flash
        /// (docs/COMPARTMENT_DESIGN.md §2.2).</summary>
        private void BuildBaseCompartment()
        {
            var go = new GameObject("BaseCompartmentRenderer");
            go.AddComponent<BaseCompartmentRenderer>().Bind(board);
        }

        /// <summary>Sprint 21 (COSMETIC_PASS_SPEC.md §1.1): a cartoon drop
        /// shadow -- the organ's own silhouette, offset down-right, under it.
        /// Replaces Sprint 15's dark halo ring.</summary>
        private static void BuildOrganLayer(string name, Vector3 center, Sprite sprite, Color tint, int order)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = order;
        }

        private static readonly Vector3 ShadowOffset = new Vector3(0.06f, -0.09f, 0f);

        /// <summary>Sprint 21 (spec §1.2): spongy bone -- cream struts round
        /// salmon marrow spaces -- with a rounded cavity behind each of the
        /// five steel build pads (BoneMarrowManager draws the pads).</summary>
        private void BuildBoneMarrowBackdrop(Layout layout)
        {
            var pads = new Vector2[layout.MarrowSlotPositions.Length];
            for (int i = 0; i < pads.Length; i++)
                pads[i] = layout.MarrowSlotPositions[i] - layout.MarrowBackdropCenter;
            float pitch = pads.Length > 1 ? Mathf.Abs(layout.MarrowSlotPositions[0].y - layout.MarrowSlotPositions[1].y) : 0f;
            var m = ImmunologyTD.Rendering.OrganShapes.BuildMarrow(layout.MarrowBackdropSize, pads, layout.MarrowSlotSize, pitch);
            var c = layout.MarrowBackdropCenter;
            BuildOrganLayer("BoneMarrowShadow", c + ShadowOffset, m.Shadow, ImmunologyTD.Rendering.Palette.OrganShadow, 1);
            BuildOrganLayer("BoneMarrowSpace", c, m.Space, ImmunologyTD.Rendering.Palette.MarrowSpace, 2);
            BuildOrganLayer("BoneMarrowBone", c, m.Bone, ImmunologyTD.Rendering.Palette.Bone, 3);
        }

        /// <summary>Sprint 21 (spec §1.3): capsule, follicles with
        /// germinal centres, paracortex, medullary cords to a hilum on the
        /// far side, and afferent lymphatics plugging in from the tissue.
        /// The body is a rounded rect that contains the helper-T arena (86%
        /// of LymphSize) -- the old bean ellipse let T cells walk off it.</summary>
        private void BuildLymphNodeBackdrop(Layout layout)
        {
            var baseRect = board.BandWorldRect(BoardBand.Base);
            var tissueRect = board.BandWorldRect(BoardBand.Tissue);
            bool tissueOnRight = tissueRect.center.x >= baseRect.center.x;
            var n = ImmunologyTD.Rendering.OrganShapes.BuildNode(layout.LymphSize, tissueOnRight);
            var c = layout.LymphCenter;
            BuildOrganLayer("LymphNodeShadow", c + ShadowOffset, n.Shadow, ImmunologyTD.Rendering.Palette.OrganShadow, 1);
            BuildOrganLayer("LymphNodeVessels", c, n.Vessels, ImmunologyTD.Rendering.Palette.Lymph, 1);
            BuildOrganLayer("LymphNode", c, n.Body, ImmunologyTD.Rendering.Palette.NodeBody, 2);
            BuildOrganLayer("LymphNodeFollicles", c, n.Follicles, ImmunologyTD.Rendering.Palette.NodeFollicle, 3);
        }

        private GameObject BuildPathogenTemplate()
        {
            var template = new GameObject("PathogenTemplate");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<PathogenAgent>();
            template.SetActive(false);
            return template;
        }

        private void BuildPathogenSpawner(GameObject pathogenTemplate)
        {
            var spawnerGo = new GameObject("PathogenSpawner");
            var spawner = spawnerGo.AddComponent<PathogenSpawner>();
            spawner.Initialize(board, tissueGrid, cytokineField, gutWall, tally, pathogenTemplate);
            pathogenSpawner = spawner;
        }

        private PathogenSpawner pathogenSpawner;

        private PrefabPool BuildUnitPool(UnitProfile profile)
        {
            var template = new GameObject($"{profile.DisplayName}Template");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<SearchUnit>();
            template.SetActive(false);

            var poolGo = new GameObject($"{profile.DisplayName}Pool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            return pool;
        }

        private BoneMarrowManager BuildBoneMarrowManager(
            Layout layout, PrefabPool macrophagePool, PrefabPool neutrophilPool, AdaptiveDirector adaptive)
        {
            var go = new GameObject("BoneMarrowManager");
            var manager = go.AddComponent<BoneMarrowManager>();

            // Sprint 23: the four new kinds are all TISSUE-roaming, so they
            // register here rather than through the AdaptiveDirector -- a
            // cytotoxic T cell is adaptive but it does its work on the board,
            // not in the lymph node. See BoneMarrowManager.IsNodeResident vs
            // IsKnowledgeGated for why that distinction needed two predicates.
            var extras = new[]
            {
                new BoneMarrowManager.TissueKindRegistration
                    { Profile = cytotoxicTProfile, Pool = BuildUnitPool(cytotoxicTProfile) },
                new BoneMarrowManager.TissueKindRegistration
                    { Profile = naturalKillerProfile, Pool = BuildUnitPool(naturalKillerProfile) },
                new BoneMarrowManager.TissueKindRegistration
                    { Profile = gammaDeltaTProfile, Pool = BuildUnitPool(gammaDeltaTProfile) },
                new BoneMarrowManager.TissueKindRegistration
                    { Profile = plasmaCellProfile, Pool = BuildUnitPool(plasmaCellProfile) },
            };

            manager.Initialize(
                board, tissueGrid, cytokineField,
                macrophageProfile, macrophagePool,
                neutrophilProfile, neutrophilPool,
                layout.MarrowSlotPositions, layout.MarrowSlotSize,
                wallet, adaptive, extras);
            return manager;
        }

        /// <summary>Sprint 8: the lymph node arena + the DC/helper-T pools +
        /// the AdaptiveDirector that emits and ticks them (GAME_DESIGN.md
        /// §5a/§5c). The node draws its agents inside the lymph backdrop
        /// rect, inset a little so nothing sits on the border.</summary>
        private AdaptiveDirector BuildAdaptiveDirector(Layout layout)
        {
            var rect = new Rect(
                layout.LymphCenter.x - layout.LymphSize.x * 0.43f,
                layout.LymphCenter.y - layout.LymphSize.y * 0.43f,
                layout.LymphSize.x * 0.86f,
                layout.LymphSize.y * 0.86f);
            var node = new LymphNode(knowledge, rect);

            // Sprint 15: the co-localisation field made visible.
            new GameObject("LymphNodeFieldRenderer")
                .AddComponent<ImmunologyTD.Rendering.LymphNodeFieldRenderer>().Bind(node);

            var dcPool = BuildAgentPool<DendriticCell>("DendriticCell");
            var lymphocytePool = BuildAgentPool<Lymphocyte>("Lymphocyte");

            var go = new GameObject("AdaptiveDirector");
            var director = go.AddComponent<AdaptiveDirector>();
            director.Initialize(node, lymphocytePool, dcPool, board, tissueGrid, cytokineField);
            return director;
        }

        private PrefabPool BuildAgentPool<T>(string label) where T : Component
        {
            var template = new GameObject($"{label}Template");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<T>();
            template.SetActive(false);

            var poolGo = new GameObject($"{label}Pool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            return pool;
        }

        private RoundController BuildRoundController(BoneMarrowManager boneMarrow)
        {
            var go = new GameObject("RoundController");
            var rounds = go.AddComponent<RoundController>();
            rounds.Initialize(wallet, pathogenSpawner, tally, boneMarrow);
            return rounds;
        }

        /// <summary>Sprint 23: the plasma cell's antibody pool. Pooled from
        /// first implementation per GAME_DESIGN.md §8's non-negotiable
        /// performance requirement -- several plasma cells secreting ~1/s
        /// with a 4.5s lifespan puts a few dozen in flight at once, and
        /// these are the smallest, most numerous objects on the board.</summary>
        private void BuildAntibodyPool()
        {
            var template = new GameObject("AntibodyTemplate");
            var tsr = template.AddComponent<SpriteRenderer>();
            tsr.sortingOrder = 12; // above host cells, below the HUD
            template.AddComponent<ImmunologyTD.Adaptive.Antibody>();
            template.SetActive(false);

            var poolGo = new GameObject("AntibodyPool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            ImmunologyTD.Adaptive.Antibody.Configure(pool);
        }

        private void BuildDegranulationFlashPool()
        {
            var template = new GameObject("DegranulationFlashTemplate");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<DegranulationFlash>();
            template.SetActive(false);

            var poolGo = new GameObject("DegranulationFlashPool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);
            DegranulationFlash.Configure(pool);
        }

        /// <summary>Sprint 16: the UI Toolkit front end, replacing Sprint
        /// 1-15's IMGUI HudOverlay (deleted). PanelSettings is a
        /// ScriptableObject created here rather than an asset, for the same
        /// reason the scene itself is built from code -- nothing in this
        /// project is hand-authored in the Editor.
        ///
        /// `themeStyleSheet` is left null on purpose (docs/UI_DESIGN.md §7):
        /// a .tss can only be produced by an editor importer, and the
        /// default control chrome it would bring is not wanted. The cost is
        /// one boot warning; every element is explicitly styled, font
        /// included, so nothing renders unstyled without it.</summary>
        private void BuildUiRoot(Layout layout, BoneMarrowManager boneMarrow, RoundController rounds, AdaptiveDirector adaptive)
        {
            // The one UI asset this project ships (Assets/Editor/UiAssetSetup.cs
            // creates it, by script, not by hand). A PanelSettings created
            // at runtime has no text settings, and in a *player build* UI
            // Toolkit's text shaper then throws a NullReferenceException on
            // every label every frame -- invisible in the Editor and in
            // batchmode, which is why the headless launch of the build is
            // part of the definition of done.
            var panelSettings = Resources.Load<UnityEngine.UIElements.PanelSettings>("ITD_PanelSettings");
            if (panelSettings == null)
            {
                Debug.LogWarning(
                    "[GameBootstrap] Resources/ITD_PanelSettings not found -- falling back to a runtime-created " +
                    "PanelSettings. The UI will render but text may throw in a player build. " +
                    "Run UiAssetSetup.CreatePanelSettings to restore the asset.");
                panelSettings = ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
                panelSettings.name = "ITD_PanelSettings";
                panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;
                panelSettings.sortingOrder = 100;
                panelSettings.clearColor = false;
            }

            var uiGo = new GameObject("UiRoot");
            uiGo.AddComponent<CytokineToggle>();   // the C debug toggle keeps its home on the UI object
            var doc = uiGo.AddComponent<UnityEngine.UIElements.UIDocument>();
            doc.panelSettings = panelSettings;

            var ui = uiGo.AddComponent<ImmunologyTD.UI.UiController>();
            ui.Bind(board, wallet, rounds, pathogenSpawner, boneMarrow, knowledge, adaptive, shop,
                gutWall, tally,
                macrophageProfile.FineTilesPerTick, neutrophilProfile.FineTilesPerTick);
            // Built explicitly rather than left to OnEnable: AddComponent
            // fires OnEnable before Bind has run, and in Editor batchmode it
            // does not fire at all -- which is exactly where the headless
            // bootstrap smoke needs the view tree to be constructed.
            ui.Build();
            ui.SetLymphNode(layout.LymphCenter, BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision);

            // The compartment headings, world-anchored (UI_DESIGN.md §6).
            // They used to be IMGUI CompartmentLabels created next to each
            // backdrop; they are UITK elements now, so they are made here
            // where the panel exists rather than there where the sprite is.
            // Sprint 21: tags pinned to the organs' edges -- the marrow's top,
            // the node's bottom (COSMETIC_PASS_SPEC.md §1.4).
            ui.AddWorldLabel(
                layout.MarrowBackdropCenter + new Vector3(0f, layout.MarrowBackdropSize.y * 0.5f, 0f),
                "Bone marrow");
            ui.AddWorldLabel(
                layout.LymphCenter - new Vector3(0f, layout.LymphSize.y * 0.5f, 0f),
                "Lymph node");
        }
    }
}
