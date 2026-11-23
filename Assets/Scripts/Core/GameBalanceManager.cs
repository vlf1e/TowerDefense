using System;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Data;

namespace TowerDefense.Core
{
    public class GameBalanceManager : MonoBehaviour
    {
        public static GameBalanceManager Instance { get; private set; }

        public event Action<TowerType, TowerConfig> OnTowerBalanceUpdated;
        public event Action<EnemyType, EnemyConfig> OnEnemyBalanceUpdated;
        public event Action OnBalanceResetToDefaults;

        private Dictionary<TowerType, TowerConfig> _defaultTowers = new Dictionary<TowerType, TowerConfig>();
        private Dictionary<TowerType, TowerConfig> _activeTowers = new Dictionary<TowerType, TowerConfig>();

        private Dictionary<EnemyType, EnemyConfig> _defaultEnemies = new Dictionary<EnemyType, EnemyConfig>();
        private Dictionary<EnemyType, EnemyConfig> _activeEnemies = new Dictionary<EnemyType, EnemyConfig>();

        private void Awake()
        {
            Instance = this;
            InitializeDefaultBalances();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeDefaultBalances()
        {
            // === БАШНИ ===
            // 1. Archer Tower (Пулеметная вышка)
            var archer = new TowerConfig
            {
                type = TowerType.Archer,
                towerName = "Стрелковая башня",
                description = "Базовая скорострельная башня. Наносит надежный точечный урон по быстрым и одиночным целям.",
                cost = 100,
                damage = 12f,
                attackCooldown = 0.6f,
                range = 3.2f,
                attackType = AttackType.SingleTarget,
                upgradeCost = 80,
                projectileSpeed = 14f
            };

            // 2. Frost Tower (Ледяная башня)
            var frost = new TowerConfig
            {
                type = TowerType.Frost,
                towerName = "Ледяная башня",
                description = "Контролирует темп боя. Замедляет врагов на 50%, давая соседним башням вдвое больше времени на атаку.",
                cost = 130,
                damage = 6f,
                attackCooldown = 0.8f,
                range = 2.8f,
                attackType = AttackType.Slow,
                slowFactor = 0.5f,
                slowDuration = 2.5f,
                upgradeCost = 100,
                projectileSpeed = 12f
            };

            // 3. Mortar Tower (Осадная мортира)
            var mortar = new TowerConfig
            {
                type = TowerType.Mortar,
                towerName = "Осадная мортира",
                description = "Тяжелая осадная пушка. Стреляет навесными ядрами с мощным уроном по площади (Splash AoE).",
                cost = 200,
                damage = 45f,
                attackCooldown = 2.2f,
                range = 4.5f,
                attackType = AttackType.AreaOfEffect,
                splashRadius = 1.6f,
                upgradeCost = 150,
                projectileSpeed = 7f
            };

            _defaultTowers[TowerType.Archer] = archer;
            _defaultTowers[TowerType.Frost] = frost;
            _defaultTowers[TowerType.Mortar] = mortar;

            _activeTowers[TowerType.Archer] = archer.Clone();
            _activeTowers[TowerType.Frost] = frost.Clone();
            _activeTowers[TowerType.Mortar] = mortar.Clone();

            // === ВРАГИ ===
            // 1. Grunt (Пехотинец)
            var grunt = new EnemyConfig
            {
                type = EnemyType.Grunt,
                enemyName = "Пехотинец",
                description = "Стандартный боец со сбалансированным здоровьем и средней скоростью перемещения.",
                maxHealth = 80f,
                moveSpeed = 1.5f,
                goldReward = 20,
                baseDamage = 1,
                tintColor = new Color(0.95f, 0.25f, 0.25f),
                visualScale = 1.0f
            };

            // 2. Scout (Скаут / Спринтер)
            var scout = new EnemyConfig
            {
                type = EnemyType.Scout,
                enemyName = "Скаут-спринтер",
                description = "Сверхбыстрый разведчик с малым запасом HP. Требует быстрой реакции или замедления ледяной башней.",
                maxHealth = 40f,
                moveSpeed = 3.0f,
                goldReward = 15,
                baseDamage = 1,
                tintColor = new Color(1.0f, 0.85f, 0.15f),
                visualScale = 0.85f
            };

            // 3. Armored Golem (Тяжелый голем / Танк)
            var golem = new EnemyConfig
            {
                type = EnemyType.Golem,
                enemyName = "Броневой Голем",
                description = "Тяжелый каменный танк с огромным запасом здоровья. Медлителен, но наносит 3 ед. урона базе.",
                maxHealth = 240f,
                moveSpeed = 0.8f,
                goldReward = 45,
                baseDamage = 3,
                tintColor = new Color(0.6f, 0.35f, 0.85f),
                visualScale = 1.3f
            };

            _defaultEnemies[EnemyType.Grunt] = grunt;
            _defaultEnemies[EnemyType.Scout] = scout;
            _defaultEnemies[EnemyType.Golem] = golem;

            _activeEnemies[EnemyType.Grunt] = grunt.Clone();
            _activeEnemies[EnemyType.Scout] = scout.Clone();
            _activeEnemies[EnemyType.Golem] = golem.Clone();
        }

        public TowerConfig GetTowerConfig(TowerType type)
        {
            if (_activeTowers.TryGetValue(type, out var config))
                return config;
            return null;
        }

        public EnemyConfig GetEnemyConfig(EnemyType type)
        {
            if (_activeEnemies.TryGetValue(type, out var config))
                return config;
            return null;
        }

        public void UpdateTowerConfig(TowerType type, TowerConfig newConfig)
        {
            if (_activeTowers.ContainsKey(type))
            {
                _activeTowers[type].CopyFrom(newConfig);
                OnTowerBalanceUpdated?.Invoke(type, _activeTowers[type]);
            }
        }

        public void UpdateEnemyConfig(EnemyType type, EnemyConfig newConfig)
        {
            if (_activeEnemies.ContainsKey(type))
            {
                _activeEnemies[type].CopyFrom(newConfig);
                OnEnemyBalanceUpdated?.Invoke(type, _activeEnemies[type]);
            }
        }

        public void ResetToDefaults()
        {
            foreach (var kvp in _defaultTowers)
            {
                _activeTowers[kvp.Key].CopyFrom(kvp.Value);
                OnTowerBalanceUpdated?.Invoke(kvp.Key, _activeTowers[kvp.Key]);
            }

            foreach (var kvp in _defaultEnemies)
            {
                _activeEnemies[kvp.Key].CopyFrom(kvp.Value);
                OnEnemyBalanceUpdated?.Invoke(kvp.Key, _activeEnemies[kvp.Key]);
            }

            OnBalanceResetToDefaults?.Invoke();
        }
    }
}
