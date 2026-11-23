using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TowerDefense.Data;
using TowerDefense.Grid;
using TowerDefense.Core;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    public class TowerPlacementController : MonoBehaviour
    {
        public static TowerPlacementController Instance { get; private set; }

        public event Action<Tower> OnTowerSelected;
        public event Action OnTowerDeselected;

        [Header("Scene References")]
        [SerializeField] private GameObject previewObject;
        [SerializeField] private SpriteRenderer previewBaseRend;
        [SerializeField] private SpriteRenderer previewTurretRend;
        [SerializeField] private SpriteRenderer previewRangeRend;

        private Camera _mainCamera;
        private bool _isPlacing = false;
        private TowerType _placingType;
        private GridCell _lastHoveredCell;
        private Tower _selectedTower;
        private float _placementStartTime = 0f;

        public bool IsPlacing => _isPlacing;
        public TowerType PlacingType => _placingType;

        public void SetupPreviewInEditor(GameObject preview, SpriteRenderer baseR, SpriteRenderer turretR, SpriteRenderer rangeR)
        {
            previewObject = preview;
            previewBaseRend = baseR;
            previewTurretRend = turretR;
            previewRangeRend = rangeR;
        }

        private void Awake()
        {
            Instance = this;

            _mainCamera = Camera.main;
            if (previewObject == null)
            {
                CreatePreviewObject();
            }
            else
            {
                previewObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void CreatePreviewObject()
        {
            previewObject = new GameObject("TowerPlacementPreview");
            previewObject.transform.SetParent(transform);

            previewBaseRend = previewObject.AddComponent<SpriteRenderer>();
            previewBaseRend.sortingOrder = 10;
            previewBaseRend.color = new Color(1, 1, 1, 0.7f);

            GameObject turretObj = new GameObject("Turret");
            turretObj.transform.SetParent(previewObject.transform);
            turretObj.transform.localPosition = Vector3.zero;
            previewTurretRend = turretObj.AddComponent<SpriteRenderer>();
            previewTurretRend.sortingOrder = 11;
            previewTurretRend.color = new Color(1, 1, 1, 0.7f);

            GameObject rangeObj = new GameObject("Range");
            rangeObj.transform.SetParent(previewObject.transform);
            rangeObj.transform.localPosition = Vector3.zero;
            previewRangeRend = rangeObj.AddComponent<SpriteRenderer>();
            previewRangeRend.sprite = (SpriteManager.Instance != null) ? SpriteManager.Instance.GetSprite("circle_range") : null;
            previewRangeRend.sortingOrder = 9;
            previewRangeRend.color = new Color(0.2f, 0.8f, 1f, 0.35f);

            previewObject.SetActive(false);
        }

        public void StartPlacement(TowerType type)
        {
            DeselectTower();

            _placingType = type;
            _isPlacing = true;
            _placementStartTime = Time.time;

            TowerConfig cfg = (GameBalanceManager.Instance != null) ? GameBalanceManager.Instance.GetTowerConfig(type) : null;
            if (SpriteManager.Instance != null)
            {
                if (previewBaseRend != null) previewBaseRend.sprite = SpriteManager.Instance.GetTowerBaseSprite(type);
                if (previewTurretRend != null) previewTurretRend.sprite = SpriteManager.Instance.GetTowerTurretSprite(type);
            }

            float range = (cfg != null) ? cfg.range : 3.0f;
            float scale = range * 0.5f;
            if (previewRangeRend != null)
            {
                previewRangeRend.transform.localScale = new Vector3(scale, scale, 1f);
            }

            if (previewObject != null)
            {
                previewObject.SetActive(true);
            }
        }

        public void CancelPlacement()
        {
            _isPlacing = false;
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            if (_lastHoveredCell != null)
            {
                _lastHoveredCell.ResetHighlight();
                _lastHoveredCell = null;
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            Vector3 mousePos = InputHelper.MousePosition;
            float camDist = (_mainCamera != null) ? Mathf.Abs(_mainCamera.transform.position.z) : 10f;
            Vector3 worldPos = (_mainCamera != null) ? _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camDist)) : Vector3.zero;
            worldPos.z = 0f;
            return worldPos;
        }

        private void Update()
        {
            if (_isPlacing)
            {
                HandlePlacementInput();
            }
            else
            {
                HandleSelectionInput();
            }
        }

        private void HandlePlacementInput()
        {
            Vector3 worldPos = GetMouseWorldPosition();

            if (previewObject != null)
            {
                previewObject.transform.position = worldPos;
            }

            GridCell currentCell = (GridManager.Instance != null) ? GridManager.Instance.GetCellAtWorldPosition(worldPos) : null;

            if (currentCell != _lastHoveredCell)
            {
                if (_lastHoveredCell != null) _lastHoveredCell.ResetHighlight();
                _lastHoveredCell = currentCell;
            }

            TowerConfig cfg = (GameBalanceManager.Instance != null) ? GameBalanceManager.Instance.GetTowerConfig(_placingType) : null;
            bool canAfford = cfg != null && GameManager.Instance != null && GameManager.Instance.CanAfford(cfg.cost);

            if (currentCell != null)
            {
                bool canBuildHere = currentCell.CanBuild() && canAfford;
                Color highlightColor = canBuildHere
                    ? new Color(0.2f, 0.9f, 0.3f, 0.45f) // Зеленый: можно строить
                    : new Color(0.95f, 0.2f, 0.2f, 0.45f); // Красный: запрещено строить

                currentCell.SetHighlight(true, highlightColor);
                if (previewRangeRend != null)
                {
                    previewRangeRend.color = canBuildHere
                        ? new Color(0.2f, 0.8f, 1f, 0.35f)
                        : new Color(1f, 0.2f, 0.2f, 0.35f);
                }
            }

            // 1. Завершение Drag & Drop при отпускании левой кнопки мыши
            if (InputHelper.MouseLeftUp)
            {
                // Если мы зажали и потянули (прошло немного времени после нажатия)
                if (Time.time - _placementStartTime > 0.08f)
                {
                    TryPlaceAtMouse();
                    return;
                }
            }

            // 2. Завершение кликом (Click-to-place)
            if (InputHelper.MouseLeftDown)
            {
                // Если кликнули уже после того, как режим включен
                if (Time.time - _placementStartTime > 0.15f)
                {
                    if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                    {
                        TryPlaceAtMouse();
                        return;
                    }
                }
            }

            // 3. Отмена по правой кнопке мыши или Escape
            if (InputHelper.MouseRightDown || InputHelper.EscapeDown)
            {
                CancelPlacement();
            }
        }

        public void TryPlaceAtMouse()
        {
            if (!_isPlacing) return;

            Vector3 worldPos = GetMouseWorldPosition();
            GridCell cell = (GridManager.Instance != null) ? GridManager.Instance.GetCellAtWorldPosition(worldPos) : null;
            TowerConfig cfg = (GameBalanceManager.Instance != null) ? GameBalanceManager.Instance.GetTowerConfig(_placingType) : null;

            if (cell != null && cell.CanBuild() && cfg != null && GameManager.Instance != null && GameManager.Instance.CanAfford(cfg.cost))
            {
                BuildTower(cell, _placingType, cfg.cost);
            }

            CancelPlacement();
        }

        private void BuildTower(GridCell cell, TowerType type, int cost)
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendGold(cost))
            {
                GameObject towerObj = new GameObject($"Tower_{type}");
                towerObj.transform.position = cell.transform.position;

                Tower tower = towerObj.AddComponent<Tower>();
                tower.Initialize(type, cell);
                cell.SetTower(tower);

                SelectTower(tower);
            }
        }

        private void HandleSelectionInput()
        {
            // Горячие клавиши выбора башен (1, 2, 3) и паузы (Пробел)
            if (InputHelper.Key1Down) StartPlacement(TowerType.Archer);
            else if (InputHelper.Key2Down) StartPlacement(TowerType.Frost);
            else if (InputHelper.Key3Down) StartPlacement(TowerType.Mortar);
            else if (InputHelper.SpaceDown && GameManager.Instance != null) GameManager.Instance.TogglePause();

            if (InputHelper.MouseLeftDown)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Vector3 worldPos = GetMouseWorldPosition();
                GridCell cell = (GridManager.Instance != null) ? GridManager.Instance.GetCellAtWorldPosition(worldPos) : null;

                if (cell != null && cell.IsOccupied)
                {
                    SelectTower(cell.PlacedTower);
                }
                else
                {
                    DeselectTower();
                }
            }
            else if (InputHelper.EscapeDown)
            {
                DeselectTower();
            }
        }

        public void SelectTower(Tower tower)
        {
            if (_selectedTower != null && _selectedTower != tower)
            {
                _selectedTower.ShowRange(false);
            }

            _selectedTower = tower;
            if (_selectedTower != null)
            {
                _selectedTower.ShowRange(true);
                OnTowerSelected?.Invoke(_selectedTower);
            }
        }

        public void DeselectTower()
        {
            if (_selectedTower != null)
            {
                _selectedTower.ShowRange(false);
                _selectedTower = null;
                OnTowerDeselected?.Invoke();
            }
        }
    }
}
