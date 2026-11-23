using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Core;
using TowerDefense.Utils;

namespace TowerDefense.Enemies
{
    public class Enemy : MonoBehaviour
    {
        public EnemyType Type { get; private set; }
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public float BaseSpeed { get; private set; }
        public int GoldReward { get; private set; }
        public int BaseDamage { get; private set; }

        public bool IsDead { get; private set; } = false;
        public float DistanceToTarget { get; private set; }

        private List<Vector3> _path;
        private int _currentWaypointIndex = 0;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;

        // Эффект замедления (Frost)
        private bool _isSlowed = false;
        private float _slowTimer = 0f;
        private float _slowFactor = 1f;

        // Полоска здоровья над головой
        private Transform _healthBarBg;
        private Transform _healthBarFill;

        public void Initialize(EnemyType type, List<Vector3> path)
        {
            Type = type;
            _path = path;
            _currentWaypointIndex = 0;

            if (_path != null && _path.Count > 0)
            {
                transform.position = _path[0];
            }

            SetupVisuals();
            ApplyConfigFromBalanceManager();

            CreateHealthBar();

            // Подписка на изменение баланса в реальном времени
            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnEnemyBalanceUpdated += HandleBalanceUpdated;
            }
        }

        private void SetupVisuals()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _spriteRenderer.sprite = SpriteManager.Instance.GetEnemySprite(Type);
            _spriteRenderer.sortingOrder = 5;
        }

        private void ApplyConfigFromBalanceManager()
        {
            if (GameBalanceManager.Instance == null) return;

            EnemyConfig cfg = GameBalanceManager.Instance.GetEnemyConfig(Type);
            if (cfg != null)
            {
                MaxHealth = cfg.maxHealth;
                CurrentHealth = MaxHealth;
                BaseSpeed = cfg.moveSpeed;
                GoldReward = cfg.goldReward;
                BaseDamage = cfg.baseDamage;
                _originalColor = cfg.tintColor;
                _spriteRenderer.color = _originalColor;
                transform.localScale = Vector3.one * cfg.visualScale;
            }
        }

        private void HandleBalanceUpdated(EnemyType type, EnemyConfig cfg)
        {
            if (type == Type && cfg != null)
            {
                float healthRatio = MaxHealth > 0 ? (CurrentHealth / MaxHealth) : 1f;
                MaxHealth = cfg.maxHealth;
                CurrentHealth = MaxHealth * healthRatio;
                BaseSpeed = cfg.moveSpeed;
                GoldReward = cfg.goldReward;
                BaseDamage = cfg.baseDamage;
                _originalColor = cfg.tintColor;
                if (!_isSlowed) _spriteRenderer.color = _originalColor;
                transform.localScale = Vector3.one * cfg.visualScale;
                UpdateHealthBar();
            }
        }

        private void CreateHealthBar()
        {
            // Фоновая темная полоска
            GameObject bgObj = new GameObject("HealthBar_Bg");
            bgObj.transform.SetParent(transform);
            bgObj.transform.localPosition = new Vector3(0, 0.45f, 0);
            bgObj.transform.localScale = new Vector3(0.8f, 0.12f, 1f);

            SpriteRenderer bgRend = bgObj.AddComponent<SpriteRenderer>();
            bgRend.sprite = SpriteManager.Instance.GetSprite("ui_slot_bg");
            bgRend.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bgRend.sortingOrder = 6;
            _healthBarBg = bgObj.transform;

            // Заполняющая полоска HP
            GameObject fillObj = new GameObject("HealthBar_Fill");
            fillObj.transform.SetParent(_healthBarBg);
            fillObj.transform.localPosition = Vector3.zero;
            fillObj.transform.localScale = Vector3.one;

            SpriteRenderer fillRend = fillObj.AddComponent<SpriteRenderer>();
            fillRend.sprite = SpriteManager.Instance.GetSprite("ui_slot_bg");
            fillRend.color = new Color(0.2f, 0.85f, 0.3f, 1f);
            fillRend.sortingOrder = 7;
            _healthBarFill = fillObj.transform;
        }

        private void Update()
        {
            if (IsDead) return;

            UpdateSlowEffect();
            MoveAlongPath();
            UpdateDistanceToTarget();
        }

        private void UpdateSlowEffect()
        {
            if (_isSlowed)
            {
                _slowTimer -= Time.deltaTime;
                if (_slowTimer <= 0)
                {
                    _isSlowed = false;
                    _slowFactor = 1f;
                    _spriteRenderer.color = _originalColor;
                }
            }
        }

        private void MoveAlongPath()
        {
            if (_path == null || _path.Count == 0 || _currentWaypointIndex >= _path.Count)
                return;

            Vector3 targetPos = _path[_currentWaypointIndex];
            float currentSpeed = BaseSpeed * _slowFactor;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= _path.Count)
                {
                    ReachControlPoint();
                }
            }
        }

        private void UpdateDistanceToTarget()
        {
            if (_path != null && _path.Count > 0)
            {
                Vector3 endPos = _path[_path.Count - 1];
                DistanceToTarget = Vector3.Distance(transform.position, endPos);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth -= damage;
            UpdateHealthBar();

            // Эффект вспышки при получении урона
            StartCoroutine(FlashDamage());

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private System.Collections.IEnumerator FlashDamage()
        {
            if (_spriteRenderer != null)
            {
                Color prev = _spriteRenderer.color;
                _spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = _isSlowed ? new Color(0.4f, 0.8f, 1f) : _originalColor;
                }
            }
        }

        public void ApplySlow(float factor, float duration)
        {
            if (IsDead) return;

            _isSlowed = true;
            _slowFactor = Mathf.Clamp(factor, 0.1f, 1f);
            _slowTimer = duration;
            _spriteRenderer.color = new Color(0.4f, 0.8f, 1f); // Ледяной голубой оттенок
        }

        private void UpdateHealthBar()
        {
            if (_healthBarFill != null)
            {
                float ratio = Mathf.Clamp01(CurrentHealth / MaxHealth);
                _healthBarFill.localScale = new Vector3(ratio, 1f, 1f);
                // Сдвиг центра заполнения, чтобы полоска уменьшалась справа налево
                _healthBarFill.localPosition = new Vector3((ratio - 1f) * 0.5f, 0, 0);

                // Окраска: зеленый -> желтый -> красный
                SpriteRenderer rend = _healthBarFill.GetComponent<SpriteRenderer>();
                if (rend != null)
                {
                    if (ratio > 0.5f) rend.color = Color.Lerp(Color.yellow, new Color(0.2f, 0.85f, 0.3f), (ratio - 0.5f) * 2f);
                    else rend.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
                }
            }
        }

        private void ReachControlPoint()
        {
            if (IsDead) return;
            IsDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamageBase(BaseDamage);
            }

            DestroyEnemy();
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(GoldReward);
            }

            DestroyEnemy();
        }

        private void DestroyEnemy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.UnregisterEnemy(this);
            }

            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnEnemyBalanceUpdated -= HandleBalanceUpdated;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (GameBalanceManager.Instance != null)
            {
                GameBalanceManager.Instance.OnEnemyBalanceUpdated -= HandleBalanceUpdated;
            }
        }
    }
}
