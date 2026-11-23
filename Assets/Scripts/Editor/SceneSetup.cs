using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TowerDefense.Core;
using TowerDefense.Grid;
using TowerDefense.Towers;
using TowerDefense.Enemies;
using TowerDefense.UI;
using TowerDefense.Utils;
using TowerDefense.Data;

namespace TowerDefense.Editor
{
    public static class SceneSetup
    {
        [MenuItem("TowerDefense/Setup Scene")]
        public static void SetupScene()
        {
            Debug.Log("[SceneSetup] Starting full scene construction...");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

            // 1. Очистка старых динамических объектов сцены (оставляем или перенастраиваем камеру и свет)
            List<GameObject> rootObjects = new List<GameObject>(scene.GetRootGameObjects());
            foreach (var root in rootObjects)
            {
                if (root.name != "Main Camera" && root.name != "Global Light 2D")
                {
                    Object.DestroyImmediate(root);
                }
            }

            // 2. Настройка Main Camera
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }
            Camera cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            // 3. Настройка Global Light 2D
            GameObject lightObj = GameObject.Find("Global Light 2D");
            if (lightObj == null)
            {
                lightObj = new GameObject("Global Light 2D");
                Light2D l2d = lightObj.AddComponent<Light2D>();
                l2d.lightType = Light2D.LightType.Global;
                l2d.color = Color.white;
                l2d.intensity = 1.0f;
            }

            // 4. Загрузка спрайтов из Assets/Sprites
            Sprite sPath = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tile_path.png");
            Sprite sBuildable = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tile_buildable.png");
            Sprite sDeadzone = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tile_deadzone.png");
            Sprite sCrystal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/base_crystal.png");
            Sprite sArcherBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_archer_base.png");
            Sprite sArcherTurret = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_archer_turret.png");
            Sprite sFrostBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_frost_base.png");
            Sprite sFrostTurret = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_frost_crystal.png");
            Sprite sMortarBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_mortar_base.png");
            Sprite sMortarTurret = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tower_mortar_barrel.png");
            Sprite sRange = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/circle_range.png");
            Sprite sGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_gold.png");
            Sprite sHeart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_heart.png");
            Sprite sSlotBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_slot_bg.png");

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 5. Создание родительского объекта Managers
            GameObject managersObj = new GameObject("Managers");
            SpriteManager spriteMgr = managersObj.AddComponent<SpriteManager>();
            GameBalanceManager balanceMgr = managersObj.AddComponent<GameBalanceManager>();
            GameManager gameMgr = managersObj.AddComponent<GameManager>();
            GridManager gridMgr = managersObj.AddComponent<GridManager>();
            WaveManager waveMgr = managersObj.AddComponent<WaveManager>();
            TowerPlacementController placementCtrl = managersObj.AddComponent<TowerPlacementController>();

            // 6. Создание сетки Grid (16x9 ячеек) прямо на сцене
            GameObject gridHolder = new GameObject("GridCells");
            gridHolder.transform.SetParent(gridMgr.transform);

            Vector3 gridOrigin = new Vector3(
                -(gridMgr.gridWidth * gridMgr.cellSize) / 2f + gridMgr.cellSize / 2f,
                -(gridMgr.gridHeight * gridMgr.cellSize) / 2f + gridMgr.cellSize / 2f,
                0f
            );

            for (int x = 0; x < gridMgr.gridWidth; x++)
            {
                for (int y = 0; y < gridMgr.gridHeight; y++)
                {
                    ZoneType zone = gridMgr.DetermineZone(x, y);
                    Vector3 worldPos = gridOrigin + new Vector3(x * gridMgr.cellSize, y * gridMgr.cellSize, 0f);

                    GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                    cellObj.transform.SetParent(gridHolder.transform);
                    cellObj.transform.position = worldPos;

                    SpriteRenderer tileRend = cellObj.AddComponent<SpriteRenderer>();
                    Sprite cellSprite = sBuildable;
                    if (zone == ZoneType.PathA) cellSprite = sPath;
                    else if (zone == ZoneType.BuildableB) cellSprite = sBuildable;
                    else if (zone == ZoneType.DeadZoneD) cellSprite = sDeadzone;
                    else if (zone == ZoneType.BaseC) cellSprite = sCrystal;

                    tileRend.sprite = cellSprite;
                    tileRend.sortingOrder = 0;

                    GameObject hlObj = new GameObject("Highlight");
                    hlObj.transform.SetParent(cellObj.transform);
                    hlObj.transform.localPosition = Vector3.zero;
                    hlObj.transform.localScale = Vector3.one;
                    SpriteRenderer hlRend = hlObj.AddComponent<SpriteRenderer>();
                    hlRend.sprite = cellSprite;
                    hlRend.sortingOrder = 1;
                    hlRend.color = Color.clear;

                    GridCell cell = cellObj.AddComponent<GridCell>();
                    cell.SetupInEditor(x, y, zone, tileRend, hlRend);
                }
            }

            // 7. Создание объекта BaseCitadel_C на сцене
            GameObject baseCitadel = new GameObject("BaseCitadel_C");
            baseCitadel.transform.position = new Vector3(-7.5f, 0f, 0f);
            baseCitadel.transform.localScale = Vector3.one * 1.5f;
            SpriteRenderer baseRend = baseCitadel.AddComponent<SpriteRenderer>();
            baseRend.sprite = sCrystal;
            baseRend.sortingOrder = 4;
            baseCitadel.AddComponent<PulseAnimation>();

            // 8. Создание PlacementPreview на сцене
            GameObject previewObj = new GameObject("TowerPlacementPreview");
            previewObj.transform.SetParent(placementCtrl.transform);
            previewObj.transform.position = Vector3.zero;

            SpriteRenderer prevBase = previewObj.AddComponent<SpriteRenderer>();
            prevBase.sortingOrder = 10;
            prevBase.color = new Color(1, 1, 1, 0.7f);

            GameObject prevTurretObj = new GameObject("Turret");
            prevTurretObj.transform.SetParent(previewObj.transform);
            prevTurretObj.transform.localPosition = Vector3.zero;
            SpriteRenderer prevTurret = prevTurretObj.AddComponent<SpriteRenderer>();
            prevTurret.sortingOrder = 11;
            prevTurret.color = new Color(1, 1, 1, 0.7f);

            GameObject prevRangeObj = new GameObject("Range");
            prevRangeObj.transform.SetParent(previewObj.transform);
            prevRangeObj.transform.localPosition = Vector3.zero;
            SpriteRenderer prevRange = prevRangeObj.AddComponent<SpriteRenderer>();
            prevRange.sprite = sRange;
            prevRange.sortingOrder = 9;
            prevRange.color = new Color(0.2f, 0.8f, 1f, 0.35f);

            previewObj.SetActive(false);
            placementCtrl.SetupPreviewInEditor(previewObj, prevBase, prevTurret, prevRange);

            // 9. Создание Canvas и полного UI на сцене
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameHUD gameHUD = canvasObj.AddComponent<GameHUD>();
            BalanceEditorUI balanceUI = canvasObj.AddComponent<BalanceEditorUI>();

            // --- TopBar ---
            GameObject topBar = CreateUIObject("TopBar", canvasObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 70));
            Image topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

            GameObject goldObj = CreateUIObject("GoldGroup", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(200, 50));
            CreateUIImage("Icon", goldObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(36, 36), sGold);
            Text goldText = CreateUIText("Text", goldObj.transform, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(-45, 0), defaultFont, "Золото: 300", 22, Color.yellow, TextAnchor.MiddleLeft);

            GameObject hpObj = CreateUIObject("HealthGroup", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(250, 0), new Vector2(200, 50));
            CreateUIImage("Icon", hpObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(36, 36), sHeart);
            Text healthText = CreateUIText("Text", hpObj.transform, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(-45, 0), defaultFont, "База C: 10/10", 22, new Color(1f, 0.35f, 0.45f), TextAnchor.MiddleLeft);

            Text waveText = CreateUIText("WaveText", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, 0), new Vector2(200, 50), defaultFont, "Волна 1 / 5", 24, Color.white, TextAnchor.MiddleCenter);
            Text statusText = CreateUIText("StatusText", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 0), new Vector2(220, 50), defaultFont, "Подготовка...", 20, new Color(0.4f, 0.9f, 1f), TextAnchor.MiddleLeft);

            Button startWaveBtn = CreateUIButton("StartWaveBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-360, 0), new Vector2(140, 46), "Старт волны", defaultFont, 16, new Color(0.2f, 0.65f, 0.35f));
            Button speedBtn = CreateUIButton("SpeedBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(75, 46), "1x", defaultFont, 18, new Color(0.3f, 0.4f, 0.5f));
            Button pauseBtn = CreateUIButton("PauseBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-185, 0), new Vector2(75, 46), "❚❚", defaultFont, 18, new Color(0.35f, 0.35f, 0.45f));
            Button balanceBtn = CreateUIButton("BalanceBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-25, 0), new Vector2(145, 46), "⚙ Баланс", defaultFont, 17, new Color(0.85f, 0.55f, 0.15f));

            // --- ZoneE_Panel ---
            GameObject zoneEPanel = CreateUIObject("ZoneE_Panel", canvasObj.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(620, 125));
            Image zoneEImg = zoneEPanel.AddComponent<Image>();
            zoneEImg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            CreateUIText("ZoneELabel", zoneEPanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(400, 24), defaultFont, "ЗОНА E — ВЫБОР И ПЕРЕТАСКИВАНИЕ БАШЕН В ЗОНУ B", 13, new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter);

            CreateTowerSlot(TowerType.Archer, "Стрелковая", 100, -190, zoneEPanel.transform, defaultFont, sArcherTurret, sGold);
            CreateTowerSlot(TowerType.Frost, "Ледяная", 130, 0, zoneEPanel.transform, defaultFont, sFrostTurret, sGold);
            CreateTowerSlot(TowerType.Mortar, "Мортира", 200, 190, zoneEPanel.transform, defaultFont, sMortarTurret, sGold);

            // --- TowerInfoPanel ---
            GameObject towerInfoPanel = CreateUIObject("TowerInfoPanel", canvasObj.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 150), new Vector2(300, 230));
            Image tiImg = towerInfoPanel.AddComponent<Image>();
            tiImg.color = new Color(0.12f, 0.15f, 0.2f, 0.96f);

            Text towerNameText = CreateUIText("TowerName", towerInfoPanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(280, 30), defaultFont, "Башня (Ур. 1)", 19, Color.white, TextAnchor.MiddleCenter);
            Text towerStatsText = CreateUIText("Stats", towerInfoPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(260, 90), defaultFont, "Урон: 12\nКулдаун: 0.6с (DPS 20)\nДальность: 3.2", 14, new Color(0.85f, 0.9f, 0.95f), TextAnchor.UpperLeft);

            Button upgradeBtn = CreateUIButton("UpgradeBtn", towerInfoPanel.transform, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(12, 12), new Vector2(130, 42), "Улучшить (80)", defaultFont, 14, new Color(0.2f, 0.6f, 0.85f));
            Text upgradeBtnText = upgradeBtn.GetComponentInChildren<Text>();

            Button sellBtn = CreateUIButton("SellBtn", towerInfoPanel.transform, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-12, 12), new Vector2(130, 42), "Продать (70)", defaultFont, 14, new Color(0.8f, 0.3f, 0.25f));
            Text sellBtnText = sellBtn.GetComponentInChildren<Text>();

            // --- EndGamePanel ---
            GameObject endGamePanel = CreateUIObject("EndGamePanel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 260));
            Image egImg = endGamePanel.AddComponent<Image>();
            egImg.color = new Color(0.08f, 0.1f, 0.14f, 0.98f);

            Text endGameTitle = CreateUIText("Title", endGamePanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(420, 50), defaultFont, "ПОБЕДА!", 34, Color.green, TextAnchor.MiddleCenter);
            Text endGameDesc = CreateUIText("Desc", endGamePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(400, 60), defaultFont, "Все 5 волн врагов успешно отбиты!", 18, Color.white, TextAnchor.MiddleCenter);

            Button restartBtn = CreateUIButton("RestartBtn", endGamePanel.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(220, 50), "ИГРАТЬ СНОВА", defaultFont, 18, new Color(0.2f, 0.65f, 0.35f));

            gameHUD.SetupInEditor(
                goldText, healthText, waveText, statusText,
                startWaveBtn, pauseBtn, speedBtn, balanceBtn,
                zoneEPanel,
                towerInfoPanel, towerNameText, towerStatsText, upgradeBtn, upgradeBtnText, sellBtn, sellBtnText,
                endGamePanel, endGameTitle, endGameDesc, restartBtn
            );

            // --- BalanceEditorPanel ---
            GameObject balancePanel = CreateUIObject("BalanceEditorPanel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580, 500));
            Image bpImg = balancePanel.AddComponent<Image>();
            bpImg.color = new Color(0.1f, 0.12f, 0.17f, 0.98f);

            CreateUIText("Title", balancePanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -25), new Vector2(400, 35), defaultFont, "⚙ РЕДАКТОР БАЛАНСА ХАРАКТЕРИСТИК", 20, new Color(1f, 0.8f, 0.3f), TextAnchor.MiddleCenter);
            Button closeBtn = CreateUIButton("CloseBtn", balancePanel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-15, -15), new Vector2(35, 35), "✕", defaultFont, 18, new Color(0.8f, 0.25f, 0.25f));

            GameObject mainTabs = CreateUIObject("MainTabs", balancePanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(400, 40));
            Button tabTowers = CreateUIButton("TabTowers", mainTabs.transform, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-5, 0), "БАШНИ", defaultFont, 16, new Color(0.2f, 0.5f, 0.8f));
            Button tabEnemies = CreateUIButton("TabEnemies", mainTabs.transform, new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(5, 0), new Vector2(-5, 0), "ВРАГИ", defaultFont, 16, new Color(0.25f, 0.3f, 0.38f));

            // Tower Group
            GameObject towerGroup = CreateUIObject("TowerGroup", balancePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(520, 310));
            GameObject subTabsT = CreateUIObject("SubTabsT", towerGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(480, 36));
            Button bArch = CreateUIButton("BtnArcher", subTabsT.transform, new Vector2(0, 0), new Vector2(0.33f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-4, 0), "Стрелковая", defaultFont, 14, new Color(0.2f, 0.6f, 0.85f));
            Button bFrost = CreateUIButton("BtnFrost", subTabsT.transform, new Vector2(0.33f, 0), new Vector2(0.66f, 1), new Vector2(0, 0), new Vector2(2, 0), new Vector2(-4, 0), "Ледяная", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));
            Button bMort = CreateUIButton("BtnMortar", subTabsT.transform, new Vector2(0.66f, 0), new Vector2(1f, 1), new Vector2(0, 0), new Vector2(4, 0), Vector2.zero, "Мортира", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));

            InputField tDmg = balanceUI.CreateInputFieldRow("DmgRow", towerGroup.transform, -65, "Урон за выстрел (Damage):", "12", defaultFont);
            InputField tCd = balanceUI.CreateInputFieldRow("CdRow", towerGroup.transform, -115, "Кулдаун атаки, сек (Cooldown):", "0.6", defaultFont);
            InputField tRange = balanceUI.CreateInputFieldRow("RangeRow", towerGroup.transform, -165, "Дальность стрельбы (Range):", "3.2", defaultFont);
            InputField tCost = balanceUI.CreateInputFieldRow("CostRow", towerGroup.transform, -215, "Стоимость в золоте (Cost):", "100", defaultFont);
            Text tDps = CreateUIText("DpsCalc", towerGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -265), new Vector2(460, 26), defaultFont, "Расчетный DPS: 20.0 ед/сек", 15, Color.yellow, TextAnchor.MiddleCenter);

            // Enemy Group
            GameObject enemyGroup = CreateUIObject("EnemyGroup", balancePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(520, 310));
            GameObject subTabsE = CreateUIObject("SubTabsE", enemyGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(480, 36));
            Button bGrunt = CreateUIButton("BtnGrunt", subTabsE.transform, new Vector2(0, 0), new Vector2(0.33f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-4, 0), "Пехотинец", defaultFont, 14, new Color(0.85f, 0.35f, 0.25f));
            Button bScout = CreateUIButton("BtnScout", subTabsE.transform, new Vector2(0.33f, 0), new Vector2(0.66f, 1), new Vector2(0, 0), new Vector2(2, 0), new Vector2(-4, 0), "Скаут", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));
            Button bGolem = CreateUIButton("BtnGolem", subTabsE.transform, new Vector2(0.66f, 0), new Vector2(1f, 1), new Vector2(0, 0), new Vector2(4, 0), Vector2.zero, "Голем", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));

            InputField eHp = balanceUI.CreateInputFieldRow("HpRow", enemyGroup.transform, -65, "Здоровье (Max HP):", "80", defaultFont);
            InputField eSpd = balanceUI.CreateInputFieldRow("SpeedRow", enemyGroup.transform, -115, "Скорость перемещения (Speed):", "1.5", defaultFont);
            InputField eRew = balanceUI.CreateInputFieldRow("RewardRow", enemyGroup.transform, -165, "Награда за убийство (Gold):", "20", defaultFont);
            InputField eDmg = balanceUI.CreateInputFieldRow("BaseDmgRow", enemyGroup.transform, -215, "Урон по базе C (Damage):", "1", defaultFont);
            enemyGroup.SetActive(false);

            GameObject bottomBar = CreateUIObject("BottomBar", balancePanel.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(500, 48));
            Button applyBtn = CreateUIButton("ApplyBtn", bottomBar.transform, new Vector2(0, 0), new Vector2(0.58f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-8, 0), "✔ ПРИМЕНИТЬ ИЗМЕНЕНИЯ", defaultFont, 16, new Color(0.2f, 0.65f, 0.3f));
            Button resetBtn = CreateUIButton("ResetBtn", bottomBar.transform, new Vector2(0.58f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(8, 0), Vector2.zero, "↺ СБРОСИТЬ", defaultFont, 15, new Color(0.55f, 0.45f, 0.2f));

            balanceUI.SetupInEditor(
                balancePanel, closeBtn, tabTowers, tabEnemies, tabTowers.GetComponent<Image>(), tabEnemies.GetComponent<Image>(),
                towerGroup, bArch, bFrost, bMort, tDmg, tCd, tRange, tCost, tDps,
                enemyGroup, bGrunt, bScout, bGolem, eHp, eSpd, eRew, eDmg,
                applyBtn, resetBtn
            );
            balancePanel.SetActive(false);

            // 10. Создание EventSystem на сцене
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            // 11. Сохранение сцены
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[SceneSetup] Scene setup completed successfully and saved!");
        }

        private static void CreateTowerSlot(TowerType type, string label, int cost, float xOffset, Transform parent, Font font, Sprite turretSp, Sprite goldSp)
        {
            GameObject slotObj = CreateUIObject($"Slot_{type}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, -10), new Vector2(170, 78));
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.22f, 0.3f, 0.9f);

            CreateUIImage("Icon", slotObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(50, 50), turretSp);
            CreateUIText("Name", slotObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(68, -12), new Vector2(-68, 25), font, label, 14, Color.white, TextAnchor.MiddleLeft);

            GameObject costObj = CreateUIObject("Cost", slotObj.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(68, 12), new Vector2(-68, 25));
            CreateUIImage("Coin", costObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(18, 18), goldSp);
            CreateUIText("CostText", costObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(-22, 0), font, $"{cost}", 14, Color.yellow, TextAnchor.MiddleLeft);

            TowerSlotUI slotUI = slotObj.AddComponent<TowerSlotUI>();
            slotUI.Initialize(type);
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return obj;
        }

        private static Text CreateUIText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Font font, string text, int fontSize, Color color, TextAnchor align)
        {
            GameObject obj = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            return t;
        }

        private static Image CreateUIImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite)
        {
            GameObject obj = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return img;
        }

        private static Button CreateUIButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, string label, Font font, int fontSize, Color bgColor)
        {
            GameObject obj = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            CreateUIText("Text", obj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, font, label, fontSize, Color.white, TextAnchor.MiddleCenter);

            return btn;
        }
    }
}
