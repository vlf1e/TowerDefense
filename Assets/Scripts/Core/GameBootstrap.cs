using UnityEngine;
using UnityEngine.Rendering.Universal;
using TowerDefense.Grid;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using TowerDefense.UI;
using TowerDefense.Utils;

namespace TowerDefense.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            SetupCamera();
            SetupLighting();
            InitializeManagers();
            CreateBaseVisuals();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void SetupLighting()
        {
            // Проверяем наличие глобального 2D света для URP 2D
            Light2D[] existingLights = FindObjectsByType<Light2D>(FindObjectsInactive.Exclude);
            if (existingLights == null || existingLights.Length == 0)
            {
                GameObject lightObj = new GameObject("GlobalLight2D");
                Light2D globalLight = lightObj.AddComponent<Light2D>();
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.color = Color.white;
                globalLight.intensity = 1.0f;
            }
        }

        private void InitializeManagers()
        {
            // 1. SpriteManager
            if (SpriteManager.Instance == null)
            {
                GameObject smObj = new GameObject("SpriteManager");
                smObj.AddComponent<SpriteManager>();
            }

            // 2. GameBalanceManager
            if (GameBalanceManager.Instance == null)
            {
                GameObject bmObj = new GameObject("GameBalanceManager");
                bmObj.AddComponent<GameBalanceManager>();
            }

            // 3. GameManager
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            // 4. GridManager
            if (GridManager.Instance == null)
            {
                GameObject gridObj = new GameObject("GridManager");
                gridObj.AddComponent<GridManager>();
            }

            // 5. WaveManager
            if (WaveManager.Instance == null)
            {
                GameObject wmObj = new GameObject("WaveManager");
                wmObj.AddComponent<WaveManager>();
            }

            // 6. TowerPlacementController
            if (TowerPlacementController.Instance == null)
            {
                GameObject tpcObj = new GameObject("TowerPlacementController");
                tpcObj.AddComponent<TowerPlacementController>();
            }

            // 7. UI Canvas + GameHUD
            if (GameHUD.Instance == null)
            {
                GameObject hudObj = new GameObject("GameHUD");
                hudObj.AddComponent<GameHUD>();
            }

            // 8. BalanceEditorUI
            if (BalanceEditorUI.Instance == null)
            {
                GameObject beObj = new GameObject("BalanceEditorUI");
                beObj.AddComponent<BalanceEditorUI>();
            }
        }

        private void CreateBaseVisuals()
        {
            // Визуальное оформление для контрольной точки C (Цитадель / Кристалл)
            // Координаты базы: (baseColX = 0, pathRowY = 4)
            // Рассчитаем положение: -16/2 + 0.5 = -7.5f по X, -9/2 + 4.5 = 0f по Y
            Vector3 basePos = new Vector3(-7.5f, 0f, 0f);

            GameObject baseObj = new GameObject("BaseCitadel_C");
            baseObj.transform.position = basePos;

            SpriteRenderer rend = baseObj.AddComponent<SpriteRenderer>();
            rend.sprite = SpriteManager.Instance.GetSprite("base_crystal");
            rend.sortingOrder = 4;
            baseObj.transform.localScale = Vector3.one * 1.5f;

            // Легкая пульсация кристалла
            baseObj.AddComponent<PulseAnimation>();
        }
    }
}
