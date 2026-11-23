using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Utils;

namespace TowerDefense.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Параметры сетки")]
        public int gridWidth = 16;
        public int gridHeight = 9;
        public float cellSize = 1f;

        [Header("Зонирование")]
        public int pathRowY = 4;           // Горизонтальная линия A движения врагов
        public int baseColX = 0;           // Столбец контрольной точки C слева

        private GridCell[,] _grid;
        private Vector3 _gridOrigin;
        private List<Vector3> _enemyWaypoints = new List<Vector3>();

        private void Awake()
        {
            Instance = this;

            _gridOrigin = new Vector3(
                -(gridWidth * cellSize) / 2f + cellSize / 2f,
                -(gridHeight * cellSize) / 2f + cellSize / 2f,
                0f
            );

            InitializeGrid();
            CalculateEnemyWaypoints();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[gridWidth, gridHeight];

            GridCell[] existingCells = GetComponentsInChildren<GridCell>(true);
            if (existingCells != null && existingCells.Length >= gridWidth * gridHeight)
            {
                foreach (var cell in existingCells)
                {
                    if (cell.GridX >= 0 && cell.GridX < gridWidth && cell.GridY >= 0 && cell.GridY < gridHeight)
                    {
                        _grid[cell.GridX, cell.GridY] = cell;
                    }
                }
            }
            else
            {
                GenerateGrid();
            }
        }

        public void GenerateGrid()
        {
            _grid = new GridCell[gridWidth, gridHeight];

            _gridOrigin = new Vector3(
                -(gridWidth * cellSize) / 2f + cellSize / 2f,
                -(gridHeight * cellSize) / 2f + cellSize / 2f,
                0f
            );

            // Удаляем старые дочерние элементы, если были
            Transform oldHolder = transform.Find("GridCells");
            if (oldHolder != null)
            {
                if (Application.isPlaying) Destroy(oldHolder.gameObject);
                else DestroyImmediate(oldHolder.gameObject);
            }

            GameObject gridHolder = new GameObject("GridCells");
            gridHolder.transform.SetParent(transform);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    ZoneType zone = DetermineZone(x, y);
                    Vector3 worldPos = CellToWorld(x, y);

                    GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                    cellObj.transform.SetParent(gridHolder.transform);
                    cellObj.transform.position = worldPos;

                    GridCell cell = cellObj.AddComponent<GridCell>();
                    Sprite sprite = (SpriteManager.Instance != null) ? SpriteManager.Instance.GetTileSprite(zone) : null;
                    cell.Initialize(x, y, zone, sprite);

                    _grid[x, y] = cell;
                }
            }
        }

        public ZoneType DetermineZone(int x, int y)
        {
            // Зона C (Контрольная точка базы слева)
            if (x == baseColX && y >= 3 && y <= 5)
            {
                return ZoneType.BaseC;
            }

            // Зона A (Линия пути врагов справа налево по Y = pathRowY)
            if (y == pathRowY && x > baseColX)
            {
                return ZoneType.PathA;
            }

            // Зона B (Зона размещения башен сверху и снизу от зоны A)
            // Верхняя полоса: Y = 5, 6; Нижняя полоса: Y = 2, 3
            if ((y == 2 || y == 3 || y == 5 || y == 6) && x > baseColX)
            {
                return ZoneType.BuildableB;
            }

            // Зона D (Зона бездействия / фон вверху и внизу: Y = 0, 1 и Y = 7, 8)
            return ZoneType.DeadZoneD;
        }

        public void CalculateEnemyWaypoints()
        {
            _enemyWaypoints.Clear();
            _gridOrigin = new Vector3(
                -(gridWidth * cellSize) / 2f + cellSize / 2f,
                -(gridHeight * cellSize) / 2f + cellSize / 2f,
                0f
            );

            Vector3 spawnPos = CellToWorld(gridWidth - 1, pathRowY) + new Vector3(cellSize, 0, 0);
            Vector3 endPos = CellToWorld(baseColX, pathRowY);

            _enemyWaypoints.Add(spawnPos);
            _enemyWaypoints.Add(endPos);
        }

        public List<Vector3> GetEnemyPath()
        {
            if (_enemyWaypoints.Count == 0)
            {
                CalculateEnemyWaypoints();
            }
            return _enemyWaypoints;
        }

        public Vector3 CellToWorld(int x, int y)
        {
            return _gridOrigin + new Vector3(x * cellSize, y * cellSize, 0f);
        }

        public bool WorldToCellCoords(Vector3 worldPos, out int cellX, out int cellY)
        {
            Vector3 local = worldPos - _gridOrigin + new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
            cellX = Mathf.FloorToInt(local.x / cellSize);
            cellY = Mathf.FloorToInt(local.y / cellSize);

            return (cellX >= 0 && cellX < gridWidth && cellY >= 0 && cellY < gridHeight);
        }

        public GridCell GetCellAtWorldPosition(Vector3 worldPos)
        {
            if (WorldToCellCoords(worldPos, out int x, out int y))
            {
                if (_grid == null) InitializeGrid();
                return _grid[x, y];
            }
            return null;
        }

        public GridCell GetCell(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                if (_grid == null) InitializeGrid();
                return _grid[x, y];
            }
            return null;
        }

        public void ClearAllHighlights()
        {
            if (_grid == null) return;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_grid[x, y] != null)
                        _grid[x, y].ResetHighlight();
                }
            }
        }
    }
}
