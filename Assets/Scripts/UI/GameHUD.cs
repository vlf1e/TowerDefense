using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using TowerDefense.Utils;

namespace TowerDefense.UI
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("Top Bar")]
        [SerializeField] private Text goldText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button startWaveBtn;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button speedBtn;
        [SerializeField] private Button balanceEditorBtn;

        [Header("Zone E Panel (Slots)")]
        [SerializeField] private GameObject zoneEPanel;

        [Header("Tower Info Panel")]
        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private Text towerNameText;
        [SerializeField] private Text towerStatsText;
        [SerializeField] private Button upgradeBtn;
        [SerializeField] private Text upgradeBtnText;
        [SerializeField] private Button sellBtn;
        [SerializeField] private Text sellBtnText;

        [Header("End Game Panel")]
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private Text endGameTitle;
        [SerializeField] private Text endGameDesc;
        [SerializeField] private Button restartBtn;

        private Tower _currentSelectedTower;

        public void SetupInEditor(
            Text goldT, Text healthT, Text waveT, Text statusT,
            Button startBtn, Button pauseB, Button speedB, Button balBtn,
            GameObject zoneE,
            GameObject tInfoPanel, Text tName, Text tStats, Button upBtn, Text upText, Button sBtn, Text sText,
            GameObject eGamePanel, Text eTitle, Text eDesc, Button rBtn
        )
        {
            goldText = goldT;
            healthText = healthT;
            waveText = waveT;
            statusText = statusT;
            startWaveBtn = startBtn;
            pauseBtn = pauseB;
            speedBtn = speedB;
            balanceEditorBtn = balBtn;
            zoneEPanel = zoneE;
            towerInfoPanel = tInfoPanel;
            towerNameText = tName;
            towerStatsText = tStats;
            upgradeBtn = upBtn;
            upgradeBtnText = upText;
            sellBtn = sBtn;
            sellBtnText = sText;
            endGamePanel = eGamePanel;
            endGameTitle = eTitle;
            endGameDesc = eDesc;
            restartBtn = rBtn;
        }

        private void Awake()
        {
            Instance = this;

            if (goldText == null)
            {
                BuildHUDUI();
            }
            else
            {
                BindButtonListeners();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BindButtonListeners()
        {
            if (startWaveBtn != null)
            {
                startWaveBtn.onClick.RemoveAllListeners();
                startWaveBtn.onClick.AddListener(() =>
                {
                    if (WaveManager.Instance != null)
                        WaveManager.Instance.SkipCountdownAndStartWave();
                });
            }

            if (speedBtn != null)
            {
                speedBtn.onClick.RemoveAllListeners();
                speedBtn.onClick.AddListener(OnSpeedBtnClicked);
            }

            if (pauseBtn != null)
            {
                pauseBtn.onClick.RemoveAllListeners();
                pauseBtn.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.TogglePause();
                });
            }

            if (balanceEditorBtn != null)
            {
                balanceEditorBtn.onClick.RemoveAllListeners();
                balanceEditorBtn.onClick.AddListener(OnBalanceBtnClicked);
            }

            if (upgradeBtn != null)
            {
                upgradeBtn.onClick.RemoveAllListeners();
                upgradeBtn.onClick.AddListener(OnUpgradeClicked);
            }

            if (sellBtn != null)
            {
                sellBtn.onClick.RemoveAllListeners();
                sellBtn.onClick.AddListener(OnSellClicked);
            }

            if (restartBtn != null)
            {
                restartBtn.onClick.RemoveAllListeners();
                restartBtn.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.RestartGame();
                });
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += UpdateGoldUI;
                GameManager.Instance.OnBaseHealthChanged += UpdateHealthUI;
                GameManager.Instance.OnGameStateChanged += UpdateGameStateUI;
                GameManager.Instance.OnTimeScaleChanged += UpdateTimeScaleUI;

                UpdateGoldUI(GameManager.Instance.CurrentGold);
                UpdateHealthUI(GameManager.Instance.BaseHealth, GameManager.Instance.MaxBaseHealth);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveChanged += UpdateWaveUI;
                WaveManager.Instance.OnWaveCountdown += UpdateCountdownUI;
                WaveManager.Instance.OnWaveStateChanged += UpdateWaveStateUI;
            }

            if (TowerPlacementController.Instance != null)
            {
                TowerPlacementController.Instance.OnTowerSelected += ShowTowerInfo;
                TowerPlacementController.Instance.OnTowerDeselected += HideTowerInfo;
            }

            HideTowerInfo();
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }
        }

        private void BuildHUDUI()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                gameObject.AddComponent<GraphicRaycaster>();
            }

            EventSystem existingEs = FindAnyObjectByType<EventSystem>();
            if (existingEs == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                existingEs = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // TopBar
            GameObject topBar = CreateUIPanel("TopBar", transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 70));
            Image topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

            GameObject goldObj = CreateUIObject("GoldGroup", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(200, 50));
            CreateUIImage("Icon", goldObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(36, 36), SpriteManager.Instance != null ? SpriteManager.Instance.GetSprite("icon_gold") : null);
            goldText = CreateUIText("Text", goldObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(-45, 0), defaultFont, "Золото: 300", 22, Color.yellow, TextAnchor.MiddleLeft);

            GameObject hpObj = CreateUIObject("HealthGroup", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(250, 0), new Vector2(200, 50));
            CreateUIImage("Icon", hpObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(36, 36), SpriteManager.Instance != null ? SpriteManager.Instance.GetSprite("icon_heart") : null);
            healthText = CreateUIText("Text", hpObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(-45, 0), defaultFont, "База C: 10/10", 22, new Color(1f, 0.35f, 0.45f), TextAnchor.MiddleLeft);

            waveText = CreateUIText("WaveText", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, 0), new Vector2(200, 50), defaultFont, "Волна 1 / 5", 24, Color.white, TextAnchor.MiddleCenter);
            statusText = CreateUIText("StatusText", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 0), new Vector2(220, 50), defaultFont, "Подготовка...", 20, new Color(0.4f, 0.9f, 1f), TextAnchor.MiddleLeft);

            startWaveBtn = CreateUIButton("StartWaveBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-360, 0), new Vector2(140, 46), "Старт волны", defaultFont, 16, new Color(0.2f, 0.65f, 0.35f));
            speedBtn = CreateUIButton("SpeedBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(75, 46), "1x", defaultFont, 18, new Color(0.3f, 0.4f, 0.5f));
            pauseBtn = CreateUIButton("PauseBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-185, 0), new Vector2(75, 46), "❚❚", defaultFont, 18, new Color(0.35f, 0.35f, 0.45f));
            balanceEditorBtn = CreateUIButton("BalanceBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-25, 0), new Vector2(145, 46), "⚙ Баланс", defaultFont, 17, new Color(0.85f, 0.55f, 0.15f));

            // Zone E
            zoneEPanel = CreateUIPanel("ZoneE_Panel", transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(620, 125));
            Image zoneEImg = zoneEPanel.AddComponent<Image>();
            zoneEImg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            CreateUIText("ZoneELabel", zoneEPanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(400, 24), defaultFont, "ЗОНА E — ВЫБОР И ПЕРЕТАСКИВАНИЕ БАШЕН В ЗОНУ B", 13, new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter);

            CreateTowerSlot(TowerType.Archer, "Стрелковая", 100, -190, zoneEPanel.transform, defaultFont);
            CreateTowerSlot(TowerType.Frost, "Ледяная", 130, 0, zoneEPanel.transform, defaultFont);
            CreateTowerSlot(TowerType.Mortar, "Мортира", 200, 190, zoneEPanel.transform, defaultFont);

            // TowerInfoPanel
            towerInfoPanel = CreateUIPanel("TowerInfoPanel", transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 150), new Vector2(300, 230));
            Image tiImg = towerInfoPanel.AddComponent<Image>();
            tiImg.color = new Color(0.12f, 0.15f, 0.2f, 0.96f);

            towerNameText = CreateUIText("TowerName", towerInfoPanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(280, 30), defaultFont, "Башня (Ур. 1)", 19, Color.white, TextAnchor.MiddleCenter);
            towerStatsText = CreateUIText("Stats", towerInfoPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(260, 90), defaultFont, "Урон: 12\nКулдаун: 0.6с (DPS 20)\nДальность: 3.2", 14, new Color(0.85f, 0.9f, 0.95f), TextAnchor.UpperLeft);

            upgradeBtn = CreateUIButton("UpgradeBtn", towerInfoPanel.transform, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(12, 12), new Vector2(130, 42), "Улучшить (80)", defaultFont, 14, new Color(0.2f, 0.6f, 0.85f));
            upgradeBtnText = upgradeBtn.GetComponentInChildren<Text>();

            sellBtn = CreateUIButton("SellBtn", towerInfoPanel.transform, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-12, 12), new Vector2(130, 42), "Продать (70)", defaultFont, 14, new Color(0.8f, 0.3f, 0.25f));
            sellBtnText = sellBtn.GetComponentInChildren<Text>();

            // EndGamePanel
            endGamePanel = CreateUIPanel("EndGamePanel", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 260));
            Image egImg = endGamePanel.AddComponent<Image>();
            egImg.color = new Color(0.08f, 0.1f, 0.14f, 0.98f);

            endGameTitle = CreateUIText("Title", endGamePanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(420, 50), defaultFont, "ПОБЕДА!", 34, Color.green, TextAnchor.MiddleCenter);
            endGameDesc = CreateUIText("Desc", endGamePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(400, 60), defaultFont, "Все 5 волн врагов успешно отбиты!", 18, Color.white, TextAnchor.MiddleCenter);

            restartBtn = CreateUIButton("RestartBtn", endGamePanel.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(220, 50), "ИГРАТЬ СНОВА", defaultFont, 18, new Color(0.2f, 0.65f, 0.35f));

            BindButtonListeners();
        }

        public void CreateTowerSlot(TowerType type, string label, int cost, float xOffset, Transform parent, Font font)
        {
            GameObject slotObj = CreateUIObject($"Slot_{type}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, -10), new Vector2(170, 78));
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.22f, 0.3f, 0.9f);

            Sprite turretSp = (SpriteManager.Instance != null) ? SpriteManager.Instance.GetTowerTurretSprite(type) : null;
            CreateUIImage("Icon", slotObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(50, 50), turretSp);

            CreateUIText("Name", slotObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(68, -12), new Vector2(-68, 25), font, label, 14, Color.white, TextAnchor.MiddleLeft);

            GameObject costObj = CreateUIObject("Cost", slotObj.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(68, 12), new Vector2(-68, 25));
            Sprite goldSp = (SpriteManager.Instance != null) ? SpriteManager.Instance.GetSprite("icon_gold") : null;
            CreateUIImage("Coin", costObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(18, 18), goldSp);
            CreateUIText("CostText", costObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(-22, 0), font, $"{cost}", 14, Color.yellow, TextAnchor.MiddleLeft);

            TowerSlotUI slotUI = slotObj.AddComponent<TowerSlotUI>();
            slotUI.Initialize(type);
        }

        private void OnSpeedBtnClicked()
        {
            if (GameManager.Instance == null) return;
            float newScale = (GameManager.Instance.CurrentTimeScale == 1f) ? 2f : 1f;
            GameManager.Instance.SetTimeScale(newScale);
        }

        private void OnBalanceBtnClicked()
        {
            if (BalanceEditorUI.Instance != null)
            {
                BalanceEditorUI.Instance.ToggleVisibility();
            }
        }

        private void OnUpgradeClicked()
        {
            if (_currentSelectedTower != null)
            {
                if (_currentSelectedTower.Upgrade())
                {
                    ShowTowerInfo(_currentSelectedTower);
                }
            }
        }

        private void OnSellClicked()
        {
            if (_currentSelectedTower != null)
            {
                _currentSelectedTower.Sell();
                HideTowerInfo();
            }
        }

        private void UpdateGoldUI(int gold)
        {
            if (goldText != null) goldText.text = $"Золото: {gold}";
        }

        private void UpdateHealthUI(int current, int max)
        {
            if (healthText != null) healthText.text = $"База C: {current}/{max}";
        }

        private void UpdateWaveUI(int wave, int total)
        {
            if (waveText != null) waveText.text = $"Волна {wave} / {total}";
        }

        private void UpdateCountdownUI(float seconds)
        {
            if (statusText != null)
            {
                if (seconds > 0)
                {
                    statusText.text = $"До волны: {Mathf.CeilToInt(seconds)} с";
                    if (startWaveBtn != null) startWaveBtn.gameObject.SetActive(true);
                }
            }
        }

        private void UpdateWaveStateUI(bool isActive)
        {
            if (statusText != null)
            {
                statusText.text = isActive ? "Атака врагов!" : "Волна отбита!";
            }
            if (startWaveBtn != null)
            {
                startWaveBtn.gameObject.SetActive(!isActive);
            }
        }

        private void UpdateGameStateUI(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (endGamePanel != null) endGamePanel.SetActive(true);
                if (endGameTitle != null)
                {
                    endGameTitle.text = "ПОРАЖЕНИЕ";
                    endGameTitle.color = new Color(0.95f, 0.25f, 0.25f);
                }
                if (endGameDesc != null) endGameDesc.text = "Враги прорвали оборону и достигли базы C!";
            }
            else if (state == GameState.Won)
            {
                if (endGamePanel != null) endGamePanel.SetActive(true);
                if (endGameTitle != null)
                {
                    endGameTitle.text = "ПОБЕДА!";
                    endGameTitle.color = new Color(0.25f, 0.95f, 0.35f);
                }
                if (endGameDesc != null) endGameDesc.text = "Поздравляем! Оборона выдержала все волны!";
            }
            else if (state == GameState.Paused)
            {
                if (pauseBtn != null) pauseBtn.GetComponentInChildren<Text>().text = "▶";
            }
            else if (state == GameState.Playing)
            {
                if (pauseBtn != null) pauseBtn.GetComponentInChildren<Text>().text = "❚❚";
            }
        }

        private void UpdateTimeScaleUI(float scale)
        {
            if (speedBtn != null)
            {
                speedBtn.GetComponentInChildren<Text>().text = $"{scale:0}x";
            }
        }

        public void ShowTowerInfo(Tower tower)
        {
            _currentSelectedTower = tower;
            if (towerInfoPanel != null) towerInfoPanel.SetActive(true);

            TowerConfig cfg = (GameBalanceManager.Instance != null) ? GameBalanceManager.Instance.GetTowerConfig(tower.Type) : null;
            string name = (cfg != null) ? cfg.towerName : tower.Type.ToString();

            if (towerNameText != null) towerNameText.text = $"{name} (Ур. {tower.Level})";
            float dps = tower.AttackCooldown > 0 ? (tower.Damage / tower.AttackCooldown) : 0;
            if (towerStatsText != null) towerStatsText.text = $"Урон: {tower.Damage:F0}\nКулдаун: {tower.AttackCooldown:F2}с (DPS {dps:F1})\nРадиус: {tower.Range:F1}\nТип: {tower.AttackType}";

            if (tower.Level < 3)
            {
                if (upgradeBtn != null)
                {
                    upgradeBtn.gameObject.SetActive(true);
                    upgradeBtn.interactable = tower.CanUpgrade();
                }
                if (upgradeBtnText != null) upgradeBtnText.text = $"Улучшить ({tower.UpgradeCost})";
            }
            else
            {
                if (upgradeBtn != null) upgradeBtn.gameObject.SetActive(false);
            }

            int refund = Mathf.RoundToInt((tower.BaseCost + (tower.Level - 1) * tower.UpgradeCost) * 0.7f);
            if (sellBtnText != null) sellBtnText.text = $"Продать (+{refund})";
        }

        public void HideTowerInfo()
        {
            _currentSelectedTower = null;
            if (towerInfoPanel != null)
            {
                towerInfoPanel.SetActive(false);
            }
        }

        private GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
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

        private GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            return CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
        }

        private Text CreateUIText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Font font, string text, int fontSize, Color color, TextAnchor align)
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

        private Image CreateUIImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite)
        {
            GameObject obj = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return img;
        }

        private Button CreateUIButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, string label, Font font, int fontSize, Color bgColor)
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
