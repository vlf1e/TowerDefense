using System;
using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Core;
using TowerDefense.Data;

namespace TowerDefense.UI
{
    public class BalanceEditorUI : MonoBehaviour
    {
        public static BalanceEditorUI Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button tabTowersBtn;
        [SerializeField] private Button tabEnemiesBtn;
        [SerializeField] private Image towerTabBtnImg;
        [SerializeField] private Image enemyTabBtnImg;

        [Header("Tower Controls")]
        [SerializeField] private GameObject towerControlsGroup;
        [SerializeField] private Button subTabArcher;
        [SerializeField] private Button subTabFrost;
        [SerializeField] private Button subTabMortar;
        [SerializeField] private InputField towerDmgField;
        [SerializeField] private InputField towerCooldownField;
        [SerializeField] private InputField towerRangeField;
        [SerializeField] private InputField towerCostField;
        [SerializeField] private Text towerDpsCalcText;

        [Header("Enemy Controls")]
        [SerializeField] private GameObject enemyControlsGroup;
        [SerializeField] private Button subTabGrunt;
        [SerializeField] private Button subTabScout;
        [SerializeField] private Button subTabGolem;
        [SerializeField] private InputField enemyHpField;
        [SerializeField] private InputField enemySpeedField;
        [SerializeField] private InputField enemyRewardField;
        [SerializeField] private InputField enemyBaseDmgField;

        [Header("Action Buttons")]
        [SerializeField] private Button applyBtn;
        [SerializeField] private Button resetBtn;

        private bool _isTowerTab = true;
        private TowerType _selectedTowerType = TowerType.Archer;
        private EnemyType _selectedEnemyType = EnemyType.Grunt;

        public void SetupInEditor(
            GameObject rootPanel, Button closeB, Button tabTowers, Button tabEnemies, Image tTabImg, Image eTabImg,
            GameObject tGroup, Button bArch, Button bFr, Button bMort, InputField tDmg, InputField tCd, InputField tRange, InputField tCost, Text tDps,
            GameObject eGroup, Button bGr, Button bSc, Button bGol, InputField eHp, InputField eSpd, InputField eRew, InputField eDmg,
            Button bApply, Button bReset
        )
        {
            panel = rootPanel;
            closeBtn = closeB;
            tabTowersBtn = tabTowers;
            tabEnemiesBtn = tabEnemies;
            towerTabBtnImg = tTabImg;
            enemyTabBtnImg = eTabImg;

            towerControlsGroup = tGroup;
            subTabArcher = bArch;
            subTabFrost = bFr;
            subTabMortar = bMort;
            towerDmgField = tDmg;
            towerCooldownField = tCd;
            towerRangeField = tRange;
            towerCostField = tCost;
            towerDpsCalcText = tDps;

            enemyControlsGroup = eGroup;
            subTabGrunt = bGr;
            subTabScout = bSc;
            subTabGolem = bGol;
            enemyHpField = eHp;
            enemySpeedField = eSpd;
            enemyRewardField = eRew;
            enemyBaseDmgField = eDmg;

            applyBtn = bApply;
            resetBtn = bReset;
        }

        private void Awake()
        {
            Instance = this;

            if (panel == null)
            {
                BuildUI();
            }
            else
            {
                BindListeners();
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BindListeners()
        {
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => panel.SetActive(false));
            }

            if (tabTowersBtn != null)
            {
                tabTowersBtn.onClick.RemoveAllListeners();
                tabTowersBtn.onClick.AddListener(() => SwitchMainTab(true));
            }

            if (tabEnemiesBtn != null)
            {
                tabEnemiesBtn.onClick.RemoveAllListeners();
                tabEnemiesBtn.onClick.AddListener(() => SwitchMainTab(false));
            }

            if (subTabArcher != null)
            {
                subTabArcher.onClick.RemoveAllListeners();
                subTabArcher.onClick.AddListener(() => SelectTowerSubTab(TowerType.Archer));
            }
            if (subTabFrost != null)
            {
                subTabFrost.onClick.RemoveAllListeners();
                subTabFrost.onClick.AddListener(() => SelectTowerSubTab(TowerType.Frost));
            }
            if (subTabMortar != null)
            {
                subTabMortar.onClick.RemoveAllListeners();
                subTabMortar.onClick.AddListener(() => SelectTowerSubTab(TowerType.Mortar));
            }

            if (subTabGrunt != null)
            {
                subTabGrunt.onClick.RemoveAllListeners();
                subTabGrunt.onClick.AddListener(() => SelectEnemySubTab(EnemyType.Grunt));
            }
            if (subTabScout != null)
            {
                subTabScout.onClick.RemoveAllListeners();
                subTabScout.onClick.AddListener(() => SelectEnemySubTab(EnemyType.Scout));
            }
            if (subTabGolem != null)
            {
                subTabGolem.onClick.RemoveAllListeners();
                subTabGolem.onClick.AddListener(() => SelectEnemySubTab(EnemyType.Golem));
            }

            if (towerDmgField != null)
            {
                towerDmgField.onValueChanged.RemoveAllListeners();
                towerDmgField.onValueChanged.AddListener((s) => RecalculateTowerDPS());
            }
            if (towerCooldownField != null)
            {
                towerCooldownField.onValueChanged.RemoveAllListeners();
                towerCooldownField.onValueChanged.AddListener((s) => RecalculateTowerDPS());
            }

            if (applyBtn != null)
            {
                applyBtn.onClick.RemoveAllListeners();
                applyBtn.onClick.AddListener(OnApplyClicked);
            }
            if (resetBtn != null)
            {
                resetBtn.onClick.RemoveAllListeners();
                resetBtn.onClick.AddListener(OnResetDefaultsClicked);
            }
        }

        private void Start()
        {
            LoadCurrentTowerData();
            LoadCurrentEnemyData();
        }

        public void ToggleVisibility()
        {
            if (panel == null) return;
            bool show = !panel.activeSelf;
            panel.SetActive(show);
            if (show)
            {
                RefreshCurrentView();
            }
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            panel = CreateUIObject("BalanceEditorPanel", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580, 500));
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.17f, 0.98f);

            CreateUIText("Title", panel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -25), new Vector2(400, 35), defaultFont, "⚙ РЕДАКТОР БАЛАНСА ХАРАКТЕРИСТИК", 20, new Color(1f, 0.8f, 0.3f), TextAnchor.MiddleCenter);

            closeBtn = CreateUIButton("CloseBtn", panel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-15, -15), new Vector2(35, 35), "✕", defaultFont, 18, new Color(0.8f, 0.25f, 0.25f));

            GameObject mainTabs = CreateUIObject("MainTabs", panel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(400, 40));
            tabTowersBtn = CreateUIButton("TabTowers", mainTabs.transform, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-5, 0), "БАШНИ", defaultFont, 16, new Color(0.2f, 0.5f, 0.8f));
            tabEnemiesBtn = CreateUIButton("TabEnemies", mainTabs.transform, new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(5, 0), new Vector2(-5, 0), "ВРАГИ", defaultFont, 16, new Color(0.25f, 0.3f, 0.38f));

            towerTabBtnImg = tabTowersBtn.GetComponent<Image>();
            enemyTabBtnImg = tabEnemiesBtn.GetComponent<Image>();

            // Tower Group
            towerControlsGroup = CreateUIObject("TowerGroup", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(520, 310));

            GameObject subTabsT = CreateUIObject("SubTabsT", towerControlsGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(480, 36));
            subTabArcher = CreateUIButton("BtnArcher", subTabsT.transform, new Vector2(0, 0), new Vector2(0.33f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-4, 0), "Стрелковая", defaultFont, 14, new Color(0.2f, 0.6f, 0.85f));
            subTabFrost = CreateUIButton("BtnFrost", subTabsT.transform, new Vector2(0.33f, 0), new Vector2(0.66f, 1), new Vector2(0, 0), new Vector2(2, 0), new Vector2(-4, 0), "Ледяная", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));
            subTabMortar = CreateUIButton("BtnMortar", subTabsT.transform, new Vector2(0.66f, 0), new Vector2(1f, 1), new Vector2(0, 0), new Vector2(4, 0), Vector2.zero, "Мортира", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));

            towerDmgField = CreateInputFieldRow("DmgRow", towerControlsGroup.transform, -65, "Урон за выстрел (Damage):", "12", defaultFont);
            towerCooldownField = CreateInputFieldRow("CdRow", towerControlsGroup.transform, -115, "Кулдаун атаки, сек (Cooldown):", "0.6", defaultFont);
            towerRangeField = CreateInputFieldRow("RangeRow", towerControlsGroup.transform, -165, "Дальность стрельбы (Range):", "3.2", defaultFont);
            towerCostField = CreateInputFieldRow("CostRow", towerControlsGroup.transform, -215, "Стоимость в золоте (Cost):", "100", defaultFont);

            towerDpsCalcText = CreateUIText("DpsCalc", towerControlsGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -265), new Vector2(460, 26), defaultFont, "Расчетный DPS: 20.0 ед/сек", 15, Color.yellow, TextAnchor.MiddleCenter);

            // Enemy Group
            enemyControlsGroup = CreateUIObject("EnemyGroup", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(520, 310));

            GameObject subTabsE = CreateUIObject("SubTabsE", enemyControlsGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(480, 36));
            subTabGrunt = CreateUIButton("BtnGrunt", subTabsE.transform, new Vector2(0, 0), new Vector2(0.33f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-4, 0), "Пехотинец", defaultFont, 14, new Color(0.85f, 0.35f, 0.25f));
            subTabScout = CreateUIButton("BtnScout", subTabsE.transform, new Vector2(0.33f, 0), new Vector2(0.66f, 1), new Vector2(0, 0), new Vector2(2, 0), new Vector2(-4, 0), "Скаут", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));
            subTabGolem = CreateUIButton("BtnGolem", subTabsE.transform, new Vector2(0.66f, 0), new Vector2(1f, 1), new Vector2(0, 0), new Vector2(4, 0), Vector2.zero, "Голем", defaultFont, 14, new Color(0.25f, 0.35f, 0.45f));

            enemyHpField = CreateInputFieldRow("HpRow", enemyControlsGroup.transform, -65, "Здоровье (Max HP):", "80", defaultFont);
            enemySpeedField = CreateInputFieldRow("SpeedRow", enemyControlsGroup.transform, -115, "Скорость перемещения (Speed):", "1.5", defaultFont);
            enemyRewardField = CreateInputFieldRow("RewardRow", enemyControlsGroup.transform, -165, "Награда за убийство (Gold):", "20", defaultFont);
            enemyBaseDmgField = CreateInputFieldRow("BaseDmgRow", enemyControlsGroup.transform, -215, "Урон по базе C (Damage):", "1", defaultFont);

            enemyControlsGroup.SetActive(false);

            GameObject bottomBar = CreateUIObject("BottomBar", panel.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(500, 48));
            applyBtn = CreateUIButton("ApplyBtn", bottomBar.transform, new Vector2(0, 0), new Vector2(0.58f, 1), new Vector2(0, 0), Vector2.zero, new Vector2(-8, 0), "✔ ПРИМЕНИТЬ ИЗМЕНЕНИЯ", defaultFont, 16, new Color(0.2f, 0.65f, 0.3f));
            resetBtn = CreateUIButton("ResetBtn", bottomBar.transform, new Vector2(0.58f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(8, 0), Vector2.zero, "↺ СБРОСИТЬ", defaultFont, 15, new Color(0.55f, 0.45f, 0.2f));

            BindListeners();
        }

        private void SwitchMainTab(bool isTowers)
        {
            _isTowerTab = isTowers;
            if (towerControlsGroup != null) towerControlsGroup.SetActive(isTowers);
            if (enemyControlsGroup != null) enemyControlsGroup.SetActive(!isTowers);

            if (towerTabBtnImg != null) towerTabBtnImg.color = isTowers ? new Color(0.2f, 0.5f, 0.8f) : new Color(0.25f, 0.3f, 0.38f);
            if (enemyTabBtnImg != null) enemyTabBtnImg.color = !isTowers ? new Color(0.2f, 0.5f, 0.8f) : new Color(0.25f, 0.3f, 0.38f);

            RefreshCurrentView();
        }

        private void SelectTowerSubTab(TowerType type)
        {
            _selectedTowerType = type;
            if (subTabArcher != null) subTabArcher.GetComponent<Image>().color = (type == TowerType.Archer) ? new Color(0.2f, 0.6f, 0.85f) : new Color(0.25f, 0.35f, 0.45f);
            if (subTabFrost != null) subTabFrost.GetComponent<Image>().color = (type == TowerType.Frost) ? new Color(0.2f, 0.6f, 0.85f) : new Color(0.25f, 0.35f, 0.45f);
            if (subTabMortar != null) subTabMortar.GetComponent<Image>().color = (type == TowerType.Mortar) ? new Color(0.2f, 0.6f, 0.85f) : new Color(0.25f, 0.35f, 0.45f);
            LoadCurrentTowerData();
        }

        private void SelectEnemySubTab(EnemyType type)
        {
            _selectedEnemyType = type;
            if (subTabGrunt != null) subTabGrunt.GetComponent<Image>().color = (type == EnemyType.Grunt) ? new Color(0.85f, 0.35f, 0.25f) : new Color(0.25f, 0.35f, 0.45f);
            if (subTabScout != null) subTabScout.GetComponent<Image>().color = (type == EnemyType.Scout) ? new Color(0.85f, 0.35f, 0.25f) : new Color(0.25f, 0.35f, 0.45f);
            if (subTabGolem != null) subTabGolem.GetComponent<Image>().color = (type == EnemyType.Golem) ? new Color(0.85f, 0.35f, 0.25f) : new Color(0.25f, 0.35f, 0.45f);
            LoadCurrentEnemyData();
        }

        private void RefreshCurrentView()
        {
            if (_isTowerTab) LoadCurrentTowerData();
            else LoadCurrentEnemyData();
        }

        private void LoadCurrentTowerData()
        {
            if (GameBalanceManager.Instance == null) return;
            TowerConfig cfg = GameBalanceManager.Instance.GetTowerConfig(_selectedTowerType);
            if (cfg != null)
            {
                if (towerDmgField != null) towerDmgField.text = cfg.damage.ToString("F1");
                if (towerCooldownField != null) towerCooldownField.text = cfg.attackCooldown.ToString("F2");
                if (towerRangeField != null) towerRangeField.text = cfg.range.ToString("F1");
                if (towerCostField != null) towerCostField.text = cfg.cost.ToString();
                RecalculateTowerDPS();
            }
        }

        private void LoadCurrentEnemyData()
        {
            if (GameBalanceManager.Instance == null) return;
            EnemyConfig cfg = GameBalanceManager.Instance.GetEnemyConfig(_selectedEnemyType);
            if (cfg != null)
            {
                if (enemyHpField != null) enemyHpField.text = cfg.maxHealth.ToString("F0");
                if (enemySpeedField != null) enemySpeedField.text = cfg.moveSpeed.ToString("F1");
                if (enemyRewardField != null) enemyRewardField.text = cfg.goldReward.ToString();
                if (enemyBaseDmgField != null) enemyBaseDmgField.text = cfg.baseDamage.ToString();
            }
        }

        private void RecalculateTowerDPS()
        {
            if (towerDmgField == null || towerCooldownField == null || towerDpsCalcText == null) return;

            if (float.TryParse(towerDmgField.text, out float dmg) && float.TryParse(towerCooldownField.text, out float cd) && cd > 0.01f)
            {
                float dps = dmg / cd;
                towerDpsCalcText.text = $"Расчетный DPS: {dps:F1} ед/сек";
            }
        }

        private void OnApplyClicked()
        {
            if (GameBalanceManager.Instance == null) return;

            if (_isTowerTab)
            {
                TowerConfig cfg = GameBalanceManager.Instance.GetTowerConfig(_selectedTowerType);
                if (cfg != null)
                {
                    TowerConfig updated = cfg.Clone();
                    if (towerDmgField != null && float.TryParse(towerDmgField.text, out float dmg)) updated.damage = dmg;
                    if (towerCooldownField != null && float.TryParse(towerCooldownField.text, out float cd)) updated.attackCooldown = Mathf.Max(0.1f, cd);
                    if (towerRangeField != null && float.TryParse(towerRangeField.text, out float range)) updated.range = Mathf.Max(1f, range);
                    if (towerCostField != null && int.TryParse(towerCostField.text, out int cost)) updated.cost = Mathf.Max(10, cost);

                    GameBalanceManager.Instance.UpdateTowerConfig(_selectedTowerType, updated);
                }
            }
            else
            {
                EnemyConfig cfg = GameBalanceManager.Instance.GetEnemyConfig(_selectedEnemyType);
                if (cfg != null)
                {
                    EnemyConfig updated = cfg.Clone();
                    if (enemyHpField != null && float.TryParse(enemyHpField.text, out float hp)) updated.maxHealth = Mathf.Max(1f, hp);
                    if (enemySpeedField != null && float.TryParse(enemySpeedField.text, out float spd)) updated.moveSpeed = Mathf.Max(0.2f, spd);
                    if (enemyRewardField != null && int.TryParse(enemyRewardField.text, out int rew)) updated.goldReward = Mathf.Max(1, rew);
                    if (enemyBaseDmgField != null && int.TryParse(enemyBaseDmgField.text, out int bd)) updated.baseDamage = Mathf.Max(1, bd);

                    GameBalanceManager.Instance.UpdateEnemyConfig(_selectedEnemyType, updated);
                }
            }
        }

        private void OnResetDefaultsClicked()
        {
            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.ResetToDefaults();
                RefreshCurrentView();
            }
        }

        public InputField CreateInputFieldRow(string name, Transform parent, float yPos, string label, string defaultVal, Font font)
        {
            GameObject row = CreateUIObject(name, parent, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yPos), new Vector2(480, 36));

            CreateUIText("Label", row.transform, new Vector2(0, 0), new Vector2(0.65f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero, font, label, 15, Color.white, TextAnchor.MiddleLeft);

            GameObject inputObj = CreateUIObject("Input", row.transform, new Vector2(0.68f, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.24f, 0.32f);

            InputField inputField = inputObj.AddComponent<InputField>();

            GameObject textObj = CreateUIObject("Text", inputObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(10, 0), new Vector2(-20, 0));
            Text t = textObj.AddComponent<Text>();
            t.font = font;
            t.fontSize = 15;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;

            inputField.textComponent = t;
            inputField.text = defaultVal;

            return inputField;
        }

        public GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
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

        public Text CreateUIText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Font font, string text, int fontSize, Color color, TextAnchor align)
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

        public Button CreateUIButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, string label, Font font, int fontSize, Color bgColor)
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
