using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Bootstrap
{
    /// <summary>
    /// Entry point. Builds the whole scene at runtime -- camera, board
    /// visualization, bone marrow / lymph node compartments, unit/pathogen
    /// pools, HUD -- from a single GameObject (see
    /// Assets/Editor/SceneSetup.cs, which is what puts this component into
    /// Sprint1.unity). Nothing is hand laid out in the Editor; see
    /// docs/INTERFACE.md for the data shapes wired together here.
    ///
    /// Sprint 2 change: units no longer debug-spawn at random positions at
    /// startup. GameBootstrap now only builds the (initially empty) unit
    /// pools and hands them to BoneMarrowManager, which is the sole source
    /// of new units from here on (GAME_DESIGN.md section 2a) -- nothing is
    /// on the tissue board until the player places at least one bone
    /// marrow tower. Also builds two new compartments (GAME_DESIGN.md
    /// section 1): the bone marrow (functional placement, see
    /// BoneMarrowManager) and the lymph node (a labeled, reserved,
    /// non-functional placeholder -- SPRINT_PLAN.md).
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
            // Lifecycle defaults, GAME_DESIGN.md section 6d / SPRINT_PLAN.md
            // items 1, 4, 5. Macrophages are the longer-lived, quiet-exit
            // half of the contrast: 20 kills (Director, 2026-08-21 -- four
            // times the neutrophil's), no degranulation, no collateral.
            MaxActiveChildren = 10,
            KillLimit = 20,
            DegranulatesOnDepletion = false,
            DegranulationBurstMultiplier = 0f,
            ContactRadiusFineTiles = 2
        };

        [SerializeField]
        private UnitProfile neutrophilProfile = new UnitProfile
        {
            Kind = UnitKind.Neutrophil,
            DisplayName = "Neutrophil",
            FineTilesPerTick = 3,
            FootprintFineTiles = 3,
            Color = new Color(0.95f, 0.78f, 0.25f),
            // Lifecycle defaults, GAME_DESIGN.md section 6d / SPRINT_PLAN.md
            // items 1, 3, 5. Neutrophils are the short-lived, violent-exit
            // half: 5 kills, then degranulation -- a 3x ContactDamagePerHit
            // burst into whatever occupies their coarse slot.
            MaxActiveChildren = 10,
            KillLimit = 5,
            DegranulatesOnDepletion = true,
            DegranulationBurstMultiplier = 3f,
            ContactRadiusFineTiles = 2
        };

        /// <summary>Bone marrow slot count -- a judgment call (see
        /// docs/TEAM_RETRO.md), "a small number" per SPRINT_PLAN.md.
        /// Chosen so a player can run a mixed macrophage/neutrophil
        /// strategy (2-3 of each) without the strip dominating the
        /// screen.</summary>
        private const int BoneMarrowSlotCount = 5;

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;

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

        private void Awake()
        {
            board = GetComponent<BoardConfig>();
            tissueGrid = new TissueGrid(board);
            cytokineField = new CytokineField(board);

            float coarseCellSize = BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision;
            var layout = BuildLayout(coarseCellSize);

            BuildCamera(layout.Bounds);
            BuildBoardVisual();
            BuildBoneMarrowBackdrop(layout);
            BuildLymphNodeBackdrop(layout);

            var pathogenTemplate = BuildPathogenTemplate();
            BuildPathogenSpawner(pathogenTemplate);

            var macrophagePool = BuildUnitPool(macrophageProfile);
            var neutrophilPool = BuildUnitPool(neutrophilProfile);
            var boneMarrow = BuildBoneMarrowManager(layout, macrophagePool, neutrophilPool);

            // Sprint 3: pooled degranulation burst effect. Built here (not
            // lazily on first use) so the very first neutrophil to deplete
            // renders its burst, and pooled because GAME_DESIGN.md section 8
            // names effects explicitly alongside enemies and projectiles.
            BuildDegranulationFlashPool();

            BuildHud(boneMarrow);

            // Diagnostic only -- lets a headless/scripted verification pass
            // (no interactive Editor session this project, see
            // docs/ENGINE_STATUS.md) recover exact world-space compartment
            // positions and camera framing from Player.log after launching
            // a real build, without needing to guess pixel coordinates for
            // simulated mouse clicks.
            Debug.Log(
                $"[GameBootstrap] Layout diagnostic -- camera pos ({Camera.main.transform.position.x:F3}, {Camera.main.transform.position.y:F3}), " +
                $"orthoSize {Camera.main.orthographicSize:F3}, aspect {Camera.main.aspect:F4}\n" +
                $"  BoneMarrowSlot[0] world ({layout.MarrowSlotPositions[0].x:F3}, {layout.MarrowSlotPositions[0].y:F3}), slotSize {layout.MarrowSlotSize:F3}\n" +
                $"  LymphNode world ({layout.LymphCenter.x:F3}, {layout.LymphCenter.y:F3})");
        }

        private Layout BuildLayout(float coarseCellSize)
        {
            float tissueHalfW = board.BoardWorldWidth * 0.5f;
            float tissueHalfH = board.BoardWorldHeight * 0.5f;

            // Bone marrow: a horizontal strip of slots below the tissue
            // board, adjacent to the blood-side edge -- CoarseCoord's Row
            // convention puts Row 0 (shallowest, nearest the lumen) at the
            // top of the screen (BoardConfig.FineToWorld), so the deepest
            // row -- the blood-adjacent edge new units extravasate from,
            // see BoneMarrowManager.Emit -- renders at the bottom. Placing
            // bone marrow below the board keeps that spatial story
            // legible even though nothing this sprint draws a literal
            // connecting path.
            float marrowSlotSize = coarseCellSize * 0.9f;
            float marrowSlotGap = coarseCellSize * 0.3f;
            float marrowGapFromTissue = coarseCellSize * 1.1f;
            float marrowStripWidth = BoneMarrowSlotCount * marrowSlotSize + (BoneMarrowSlotCount - 1) * marrowSlotGap;
            float marrowCenterY = -tissueHalfH - marrowGapFromTissue - marrowSlotSize * 0.5f;
            float marrowStartX = -marrowStripWidth * 0.5f + marrowSlotSize * 0.5f;

            var slotPositions = new Vector3[BoneMarrowSlotCount];
            for (int i = 0; i < BoneMarrowSlotCount; i++)
            {
                float x = marrowStartX + i * (marrowSlotSize + marrowSlotGap);
                slotPositions[i] = new Vector3(x, marrowCenterY, 0f);
            }

            var marrowBackdropSize = new Vector2(marrowStripWidth + marrowSlotGap, marrowSlotSize + marrowSlotGap);
            var marrowBackdropCenter = new Vector3(0f, marrowCenterY, 0f);

            // Lymph node: a labeled, reserved box to the right of the
            // tissue board -- GAME_DESIGN.md section 1's fourth
            // compartment, not functional this sprint (SPRINT_PLAN.md).
            float lymphGap = coarseCellSize * 1.1f;
            var lymphSize = new Vector2(coarseCellSize * 2.4f, coarseCellSize * 2.0f);
            var lymphCenter = new Vector3(tissueHalfW + lymphGap + lymphSize.x * 0.5f, 0f, 0f);

            float minX = Mathf.Min(-tissueHalfW, marrowBackdropCenter.x - marrowBackdropSize.x * 0.5f);
            float maxX = Mathf.Max(tissueHalfW, lymphCenter.x + lymphSize.x * 0.5f);
            float minY = Mathf.Min(marrowBackdropCenter.y - marrowBackdropSize.y * 0.5f, lymphCenter.y - lymphSize.y * 0.5f);
            float maxY = Mathf.Max(tissueHalfH, lymphCenter.y + lymphSize.y * 0.5f);

            return new Layout
            {
                Bounds = Rect.MinMaxRect(minX, minY, maxX, maxY),
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
            mainCamera.backgroundColor = new Color(0.07f, 0.07f, 0.09f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";

            FitCamera(mainCamera, bounds);
            StartCoroutine(RefitCameraNextFrame(bounds));
        }

        /// <summary>
        /// Fits the camera's orthographic size to <paramref name="bounds"/>
        /// using Camera.aspect at call time. Sprint 2 bug found via build
        /// screenshot (not the headless verification, which can't see
        /// rendering -- see docs/TEAM_RETRO.md): Camera.aspect read during
        /// Awake() -- frame 0, before the actual window size has settled --
        /// does not reliably match the real runtime window's aspect ratio,
        /// which under-sized the fit enough to crop the right edge of the
        /// board and push the lymph node fully off-screen in a real build.
        /// FitCamera is called once immediately (so there's a reasonable
        /// frame-0 fallback) and again one frame later via
        /// RefitCameraNextFrame, by which point Screen.width/height (and
        /// therefore Camera.aspect) reflect the real window.
        /// </summary>
        private static void FitCamera(Camera cam, Rect bounds)
        {
            const float padding = 1.1f;
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

            // Diagnostic only -- see the frame-0 log in Awake() for why this
            // is logged again post-refit: this is the value a scripted
            // verification pass should actually use to compute screen
            // coordinates for simulated clicks.
            Debug.Log(
                $"[GameBootstrap] Post-refit camera diagnostic -- pos ({mainCamera.transform.position.x:F3}, {mainCamera.transform.position.y:F3}), " +
                $"orthoSize {mainCamera.orthographicSize:F3}, aspect {mainCamera.aspect:F4}, Screen {Screen.width}x{Screen.height}");
        }

        private void BuildBoardVisual()
        {
            var container = new GameObject("HostCellGrid").transform;
            var views = new SpriteRenderer[board.Columns, BoardConfig.Rows];
            float size = BoardConfig.FineTileWorldSize * BoardConfig.FineSubdivision * 0.92f;

            for (int col = 0; col < board.Columns; col++)
            {
                for (int row = 0; row < BoardConfig.Rows; row++)
                {
                    var cellGo = new GameObject($"Cell_{col}_{row}");
                    cellGo.transform.SetParent(container, false);
                    cellGo.transform.position = board.CoarseToWorldCenter(new CoarseCoord(col, row));
                    cellGo.transform.localScale = new Vector3(size, size, 1f);
                    var sr = cellGo.AddComponent<SpriteRenderer>();
                    sr.sprite = RuntimeSprites.SquareSprite;
                    sr.sortingOrder = 0;
                    views[col, row] = sr;
                }
            }

            var boardRenderer = gameObject.AddComponent<BoardRenderer>();
            boardRenderer.Bind(board, tissueGrid, cytokineField, views);
        }

        private void BuildBoneMarrowBackdrop(Layout layout)
        {
            var go = new GameObject("BoneMarrowBackdrop");
            go.transform.position = layout.MarrowBackdropCenter;
            go.transform.localScale = new Vector3(layout.MarrowBackdropSize.x, layout.MarrowBackdropSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.color = new Color(0.30f, 0.24f, 0.16f); // dark bone-marrow brown, distinct from tissue's pink
            sr.sortingOrder = 1;

            var labelGo = new GameObject("BoneMarrowLabel");
            var label = labelGo.AddComponent<CompartmentLabel>();
            var labelPos = layout.MarrowBackdropCenter + new Vector3(0f, layout.MarrowBackdropSize.y * 0.5f + 0.35f, 0f);
            label.Initialize(labelPos, "Bone Marrow -- click an empty slot to place a tower", new Vector2(440, 34));
        }

        private void BuildLymphNodeBackdrop(Layout layout)
        {
            var go = new GameObject("LymphNode");
            go.transform.position = layout.LymphCenter;
            go.transform.localScale = new Vector3(layout.LymphSize.x, layout.LymphSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSprites.SquareSprite;
            sr.color = new Color(0.34f, 0.40f, 0.28f); // pale lymphoid green, distinct from tissue/bone marrow
            sr.sortingOrder = 1;

            var labelGo = new GameObject("LymphNodeLabel");
            var label = labelGo.AddComponent<CompartmentLabel>();
            label.Initialize(layout.LymphCenter, "Lymph Node\n(reserved -- not functional yet)", new Vector2(220, 46));
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
            spawner.Initialize(board, tissueGrid, cytokineField, pathogenTemplate);
        }

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

        private BoneMarrowManager BuildBoneMarrowManager(Layout layout, PrefabPool macrophagePool, PrefabPool neutrophilPool)
        {
            var go = new GameObject("BoneMarrowManager");
            var manager = go.AddComponent<BoneMarrowManager>();
            manager.Initialize(
                board, tissueGrid, cytokineField,
                macrophageProfile, macrophagePool,
                neutrophilProfile, neutrophilPool,
                layout.MarrowSlotPositions, layout.MarrowSlotSize);
            return manager;
        }

        /// <summary>Sprint 3: the pooled burst effect a depleting neutrophil
        /// plays (see Rendering/DegranulationFlash.cs). Same
        /// runtime-template + PrefabPool pattern as the unit and pathogen
        /// pools -- no .prefab assets in this project, and no raw
        /// Instantiate/Destroy anywhere (GAME_DESIGN.md section 8).</summary>
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

        private void BuildHud(BoneMarrowManager boneMarrow)
        {
            var hudGo = new GameObject("HUD");
            hudGo.AddComponent<CytokineToggle>();
            var overlay = hudGo.AddComponent<HudOverlay>();
            overlay.Bind(board, macrophageProfile.FineTilesPerTick, neutrophilProfile.FineTilesPerTick, boneMarrow);
        }
    }
}
