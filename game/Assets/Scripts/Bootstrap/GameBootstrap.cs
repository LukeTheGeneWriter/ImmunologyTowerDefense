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
            // At the 0.12s tick, 0.03/tick is ~1-in-4 per second -- a
            // macrophage that parks on an infected cell catches it in a few
            // seconds on average, but a patrol that only brushes past
            // usually misses. Judgment call, mechanics-first.
            StressSenseChancePerTick = 0.03f
        };

        [SerializeField]
        private UnitProfile neutrophilProfile = new UnitProfile
        {
            Kind = UnitKind.Neutrophil,
            DisplayName = "Neutrophil",
            FineTilesPerTick = 3,
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
            // that can stumble onto a stressed cell. Judgment call.
            StressSenseChancePerTick = 0.02f
        };

        /// <summary>Bone marrow slot count -- unchanged from Sprint 2, a
        /// judgment call (see docs/TEAM_RETRO.md).</summary>
        private const int BoneMarrowSlotCount = 5;

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;
        private GutInterface gutInterface;
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
            ImmunologyTD.Rendering.SpriteShapes.Prewarm();
            macrophageProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.Macrophage;
            neutrophilProfile.Shape = ImmunologyTD.Rendering.SpriteShapes.Neutrophil;
            tissueGrid = new TissueGrid(board);
            cytokineField = new CytokineField(board);
            tally = new InvasionTally();
            gutInterface = new GutInterface(board, tissueGrid, tally);

            // Sprint 7: the ATP economy + round loop (GAME_DESIGN.md §5b/§5d/§6c).
            EconomyTuning.ResetToDefaults();
            wallet = new AtpWallet(EconomyTuning.StartingAtp);
            EconomyHooks.PayForKill = () => wallet.Grant(EconomyTuning.AtpPerKill);

            // Sprint 8: the dendritic-cell shuttle + antigen barcode
            // (GAME_DESIGN.md §5a/§5c). Knowledge % persists for the run.
            AdaptiveTuning.ResetToDefaults();
            knowledge = new KnowledgeLedger();

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
            BuildBoardVisual();
            BuildLumenChannel();
            BuildBaseCompartment();
            BuildGutInterfaceVisual();
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
            BuildHud(boneMarrow, rounds, adaptive);

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

            var marrowBackdropSize = new Vector2(
                Mathf.Min(marrowSlotSize * 1.35f, baseRect.width * 0.92f),
                marrowStripHeight + pitch * 0.35f);
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
            mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
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
            const float padding = 1.04f;
            Vector2 center = bounds.center;
            float halfHeight = bounds.height * 0.5f * padding;
            float aspect = cam.aspect > 0 ? cam.aspect : 16f / 9f;
            float halfWidthDrivenSize = bounds.width * 0.5f * padding / aspect;
            cam.orthographicSize = Mathf.Max(halfHeight, halfWidthDrivenSize);
            cam.transform.position = new Vector3(center.x, center.y, -10f);
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

        private void BuildGutInterfaceVisual()
        {
            var go = new GameObject("GutInterfaceRenderer");
            var renderer = go.AddComponent<GutInterfaceRenderer>();
            renderer.Bind(board, gutInterface);
        }

        /// <summary>Sprint 15: the lumen stops being tinted host-cell quads
        /// and becomes an open channel -- a chyme field, a mucus wall layer,
        /// and drifting particulate (docs/COMPARTMENT_DESIGN.md §2.1).</summary>
        private void BuildLumenChannel()
        {
            var go = new GameObject("LumenChannelRenderer");
            go.AddComponent<LumenChannelRenderer>().Bind(board);
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

        /// <summary>Sprint 15: a soft dark-plasma halo behind a base-band
        /// organ so it reads as suspended in the bloodstream rather than
        /// floating on the camera clear colour (docs/COMPARTMENT_DESIGN.md
        /// §2.2). Sits at sortingOrder 1, behind the backdrop at 2.</summary>
        private void BuildOrganHalo(string name, Vector3 center, Vector2 backdropSize)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            go.transform.localScale = new Vector3(backdropSize.x * 1.5f, backdropSize.y * 1.5f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.SpriteShapes.OrganHalo;
            sr.color = new Color(0.20f, 0.06f, 0.09f); // deeper plasma
            sr.sortingOrder = 1;
        }

        private void BuildBoneMarrowBackdrop(Layout layout)
        {
            BuildOrganHalo("BoneMarrowHalo", layout.MarrowBackdropCenter, layout.MarrowBackdropSize);

            var go = new GameObject("BoneMarrowBackdrop");
            go.transform.position = layout.MarrowBackdropCenter;
            go.transform.localScale = new Vector3(layout.MarrowBackdropSize.x, layout.MarrowBackdropSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.SpriteShapes.MarrowRegion; // Sprint 13 texture, Sprint 15 revision
            sr.color = new Color(0.34f, 0.22f, 0.18f); // Sprint 15: red marrow -- belongs to the blood it sits in
            sr.sortingOrder = 2; // Sprint 15: above the organ halo

            var labelGo = new GameObject("BoneMarrowLabel");
            var label = labelGo.AddComponent<CompartmentLabel>();
            var labelPos = layout.MarrowBackdropCenter + new Vector3(0f, layout.MarrowBackdropSize.y * 0.5f + 0.9f, 0f);
            label.Initialize(labelPos, "Bone Marrow -- click an empty slot to place a tower", new Vector2(440, 34));
        }

        private void BuildLymphNodeBackdrop(Layout layout)
        {
            BuildOrganHalo("LymphNodeHalo", layout.LymphCenter, layout.LymphSize);

            var go = new GameObject("LymphNode");
            go.transform.position = layout.LymphCenter;
            go.transform.localScale = new Vector3(layout.LymphSize.x, layout.LymphSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ImmunologyTD.Rendering.SpriteShapes.LymphNodeBean; // Sprint 13 silhouette, Sprint 15 revision
            sr.color = new Color(0.34f, 0.40f, 0.28f); // pale lymphoid green
            sr.sortingOrder = 2; // Sprint 15: above the organ halo

            var labelGo = new GameObject("LymphNodeLabel");
            var label = labelGo.AddComponent<CompartmentLabel>();
            label.Initialize(layout.LymphCenter, "Lymph Node\nantigen presentation", new Vector2(220, 46));
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
            spawner.Initialize(board, tissueGrid, cytokineField, gutInterface, tally, pathogenTemplate);
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
            manager.Initialize(
                board, tissueGrid, cytokineField,
                macrophageProfile, macrophagePool,
                neutrophilProfile, neutrophilPool,
                layout.MarrowSlotPositions, layout.MarrowSlotSize,
                wallet, adaptive);
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

        private void BuildHud(BoneMarrowManager boneMarrow, RoundController rounds, AdaptiveDirector adaptive)
        {
            var hudGo = new GameObject("HUD");
            hudGo.AddComponent<CytokineToggle>();
            var overlay = hudGo.AddComponent<HudOverlay>();
            overlay.Bind(board, macrophageProfile.FineTilesPerTick, neutrophilProfile.FineTilesPerTick,
                boneMarrow, gutInterface, tally, pathogenSpawner, wallet, rounds, knowledge, adaptive, shop);
        }
    }
}
