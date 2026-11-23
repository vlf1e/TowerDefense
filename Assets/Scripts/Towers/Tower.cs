using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Enemies;
using TowerDefense.Grid;
using TowerDefense.Core;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    public class Tower : MonoBehaviour
    {
        public TowerType Type { get; private set; }
        public int Level { get; private set; } = 1;
        public GridCell Cell { get; private set; }

        public float Damage { get; private set; }
        public float AttackCooldown { get; private set; }
        public float Range { get; private set; }
        public int BaseCost { get; private set; }
        public int UpgradeCost { get; private set; }
        public float SplashRadius { get; private set; }
        public float SlowFactor { get; private set; }
        public float SlowDuration { get; private set; }
        public AttackType AttackType { get; private set; }

        public TargetPriority Priority { get; set; } = TargetPriority.First;

        private float _attackTimer = 0f;
        private Transform _turretTransform;
        private Transform _rangeIndicator;
        private Enemy _currentTarget;

        public void Initialize(TowerType type, GridCell cell)
        {
            Type = type;
            Cell = cell;
            Level = 1;

            SetupVisuals();
            ApplyConfigFromBalanceManager();
            CreateRangeIndicator();

            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnTowerBalanceUpdated += HandleBalanceUpdated;
            }
        }

        private void SetupVisuals()
        {
            // Основание башни (Base)
            SpriteRenderer baseRend = GetComponent<SpriteRenderer>();
            if (baseRend == null)
            {
                baseRend = gameObject.AddComponent<SpriteRenderer>();
            }
            baseRend.sprite = SpriteManager.Instance.GetTowerBaseSprite(Type);
            baseRend.sortingOrder = 2;

            // Поворотная турель / кристалл (Turret)
            GameObject turretObj = new GameObject("Turret");
            turretObj.transform.SetParent(transform);
            turretObj.transform.localPosition = Vector3.zero;

            SpriteRenderer turretRend = turretObj.AddComponent<SpriteRenderer>();
            turretRend.sprite = SpriteManager.Instance.GetTowerTurretSprite(Type);
            turretRend.sortingOrder = 3;
            _turretTransform = turretObj.transform;
        }

        private void ApplyConfigFromBalanceManager()
        {
            if (GameBalanceManager.Instance == null) return;

            TowerConfig cfg = GameBalanceManager.Instance.GetTowerConfig(Type);
            if (cfg != null)
            {
                float levelMult = 1f + (Level - 1) * 0.5f;
                Damage = cfg.damage * levelMult;
                AttackCooldown = cfg.attackCooldown;
                Range = cfg.range * (1f + (Level - 1) * 0.1f);
                BaseCost = cfg.cost;
                UpgradeCost = Mathf.RoundToInt(cfg.upgradeCost * levelMult);
                SplashRadius = cfg.splashRadius;
                SlowFactor = cfg.slowFactor;
                SlowDuration = cfg.slowDuration + (Level - 1) * 0.5f;
                AttackType = cfg.attackType;

                UpdateRangeIndicatorScale();
            }
        }

        private void HandleBalanceUpdated(TowerType type, TowerConfig cfg)
        {
            if (type == Type && cfg != null)
            {
                ApplyConfigFromBalanceManager();
            }
        }

        private void CreateRangeIndicator()
        {
            GameObject rangeObj = new GameObject("RangeIndicator");
            rangeObj.transform.SetParent(transform);
            rangeObj.transform.localPosition = Vector3.zero;

            SpriteRenderer rend = rangeObj.AddComponent<SpriteRenderer>();
            rend.sprite = SpriteManager.Instance.GetSprite("circle_range");
            rend.sortingOrder = 1;
            rend.color = new Color(0.2f, 0.7f, 1f, 0.25f);

            _rangeIndicator = rangeObj.transform;
            UpdateRangeIndicatorScale();
            ShowRange(false);
        }

        private void UpdateRangeIndicatorScale()
        {
            if (_rangeIndicator != null)
            {
                // circle_range имеет размер 256x256 при 64 PPU = 4 единицы диаметр.
                // Диаметр нужный = Range * 2. Значит масштаб = (Range * 2) / 4 = Range * 0.5f.
                float scale = Range * 0.5f;
                _rangeIndicator.localScale = new Vector3(scale, scale, 1f);
            }
        }

        public void ShowRange(bool show)
        {
            if (_rangeIndicator != null)
            {
                _rangeIndicator.gameObject.SetActive(show);
            }
        }

        private void Update()
        {
            _attackTimer -= Time.deltaTime;

            AcquireTarget();
            AimAtTarget();

            if (_currentTarget != null && _attackTimer <= 0)
            {
                Shoot();
                _attackTimer = AttackCooldown;
            }
        }

        private void AcquireTarget()
        {
            if (WaveManager.Instance == null) return;

            // Если текущая цель вышла из радиуса или погибла
            if (_currentTarget == null || _currentTarget.IsDead || Vector3.Distance(transform.position, _currentTarget.transform.position) > Range)
            {
                _currentTarget = WaveManager.Instance.GetTargetForTower(transform.position, Range, Priority);
            }
        }

        private void AimAtTarget()
        {
            if (_currentTarget == null || _turretTransform == null) return;

            Vector3 dir = _currentTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _turretTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Shoot()
        {
            if (_currentTarget == null) return;

            GameObject bulletObj = new GameObject($"Bullet_{Type}");
            bulletObj.transform.position = transform.position;

            Projectile proj = bulletObj.AddComponent<Projectile>();
            float speed = (Type == TowerType.Mortar) ? 8f : 14f;

            proj.Initialize(
                Type,
                _currentTarget,
                Damage,
                speed,
                AttackType,
                SlowFactor,
                SlowDuration,
                SplashRadius
            );
        }

        public bool CanUpgrade()
        {
            return Level < 3 && GameManager.Instance != null && GameManager.Instance.CanAfford(UpgradeCost);
        }

        public bool Upgrade()
        {
            if (CanUpgrade())
            {
                if (GameManager.Instance.SpendGold(UpgradeCost))
                {
                    Level++;
                    ApplyConfigFromBalanceManager();

                    // Визуальный эффект улучшения (подсветка/масштаб)
                    transform.localScale = Vector3.one * (1f + (Level - 1) * 0.1f);
                    return true;
                }
            }
            return false;
        }

        public void Sell()
        {
            int refund = Mathf.RoundToInt((BaseCost + (Level - 1) * UpgradeCost) * 0.7f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(refund);
            }

            if (Cell != null)
            {
                Cell.ClearTower();
            }

            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnTowerBalanceUpdated -= HandleBalanceUpdated;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnTowerBalanceUpdated -= HandleBalanceUpdated;
            }
        }
    }
}
