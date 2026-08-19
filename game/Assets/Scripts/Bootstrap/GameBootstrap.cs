using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Pooling;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Bootstrap
{
    /// <summary>
    /// Sprint 1 entry point. Builds the whole scene at runtime -- camera,
    /// board visualization, unit/pathogen pools and spawns, HUD -- from a
    /// single GameObject (see Assets/Editor/SceneSetup.cs, which is what
    /// puts this component into Sprint1.unity). Nothing is hand laid out
    /// in the Editor; that keeps the whole sprint reproducible from code,
    /// which matters since this was built without an interactive Editor
    /// session. See docs/INTERFACE.md for the data shapes wired together
    /// here.
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
            Color = new Color(0.30f, 0.40f, 0.80f)
        };

        [SerializeField]
        private UnitProfile neutrophilProfile = new UnitProfile
        {
            Kind = UnitKind.Neutrophil,
            DisplayName = "Neutrophil",
            FineTilesPerTick = 3,
            FootprintFineTiles = 3,
            Color = new Color(0.95f, 0.78f, 0.25f)
        };

        [SerializeField] private int macrophageCount = 4;
        [SerializeField] private int neutrophilCount = 6;

        private BoardConfig board;
        private TissueGrid tissueGrid;
        private CytokineField cytokineField;

        private void Awake()
        {
            board = GetComponent<BoardConfig>();
            tissueGrid = new TissueGrid(board);
            cytokineField = new CytokineField(board);

            BuildCamera();
            BuildBoardVisual();

            var pathogenTemplate = BuildPathogenTemplate();
            BuildPathogenSpawner(pathogenTemplate);

            SpawnUnits(macrophageProfile, macrophageCount);
            SpawnUnits(neutrophilProfile, neutrophilCount);

            BuildHud();
        }

        private void BuildCamera()
        {
            var camGo = new GameObject("MainCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";

            const float padding = 1.15f;
            float halfHeightDrivenSize = board.BoardWorldHeight * 0.5f * padding;
            float aspect = cam.aspect > 0 ? cam.aspect : 16f / 9f;
            float halfWidthDrivenSize = board.BoardWorldWidth * 0.5f * padding / aspect;
            cam.orthographicSize = Mathf.Max(halfHeightDrivenSize, halfWidthDrivenSize);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
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
            boardRenderer.Bind(board, tissueGrid, views);
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

        private void SpawnUnits(UnitProfile profile, int count)
        {
            var template = new GameObject($"{profile.DisplayName}Template");
            template.AddComponent<SpriteRenderer>();
            template.AddComponent<SearchUnit>();
            template.SetActive(false);

            var poolGo = new GameObject($"{profile.DisplayName}Pool");
            var pool = poolGo.AddComponent<PrefabPool>();
            pool.SetPrefab(template);

            // Bone-marrow placement and blood-entry mechanics are out of
            // scope this sprint (see SPRINT_PLAN.md) -- units debug-spawn
            // at randomized fine-grid positions instead of extravasating
            // from blood. Noted in docs/TEAM_RETRO.md so it isn't mistaken
            // for a design decision.
            for (int i = 0; i < count; i++)
            {
                var go = pool.Get();
                var start = new FineCoord(Random.Range(0, board.FineColumns), Random.Range(0, board.FineRows));
                go.GetComponent<SearchUnit>().Initialize(board, tissueGrid, cytokineField, profile, start);
            }
        }

        private void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hudGo.AddComponent<CytokineToggle>();
            var overlay = hudGo.AddComponent<HudOverlay>();
            overlay.Bind(board, macrophageCount, macrophageProfile.FineTilesPerTick, neutrophilCount, neutrophilProfile.FineTilesPerTick);
        }
    }
}
