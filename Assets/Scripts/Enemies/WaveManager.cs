using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Grid;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public event Action<int, int> OnWaveChanged;
        public event Action<float> OnWaveCountdown;
        public event Action<bool> OnWaveStateChanged;

        [Header("Настройки волн")]
        [SerializeField] private float countdownBetweenWaves = 8f;

        private List<WaveDefinition> _waves = new List<WaveDefinition>();
        private int _currentWaveIndex = -1;
        private List<Enemy> _activeEnemies = new List<Enemy>();
        private bool _isSpawningWave = false;
        private bool _isWaveActive = false;
        private float _countdownTimer = 0f;

        public int CurrentWave => _currentWaveIndex + 1;
        public int TotalWaves => _waves.Count;
        public bool IsWaveInProgress => _isWaveActive || _isSpawningWave;
        public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

        private void Awake()
        {
            Instance = this;
            BuildPredefinedWaves();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            StartCoroutine(WaveProgressionRoutine());
        }

        private void BuildPredefinedWaves()
        {
            _waves.Clear();

            // Волна 1: 5 Пехотинцев (знакомство)
            var w1 = new WaveDefinition { waveName = "Волна 1: Дозорные" };
            w1.groups.Add(new WaveSubGroup { enemyType = EnemyType.Grunt, count = 5, spawnInterval = 1.3f });
            _waves.Add(w1);

            // Волна 2: 4 Пехотинца + 4 Скаута (проверка реакции на скорость)
            var w2 = new WaveDefinition { waveName = "Волна 2: Стремительный рейд" };
            w2.groups.Add(new WaveSubGroup { enemyType = EnemyType.Grunt, count = 4, spawnInterval = 1.2f });
            w2.groups.Add(new WaveSubGroup { enemyType = EnemyType.Scout, count = 4, spawnInterval = 0.9f, delayBeforeGroup = 1.0f });
            _waves.Add(w2);

            // Волна 3: 6 Скаутов + 2 Броневых Голема (требуется замедление и высокий урон)
            var w3 = new WaveDefinition { waveName = "Волна 3: Каменный щит" };
            w3.groups.Add(new WaveSubGroup { enemyType = EnemyType.Scout, count = 6, spawnInterval = 0.8f });
            w3.groups.Add(new WaveSubGroup { enemyType = EnemyType.Golem, count = 2, spawnInterval = 2.5f, delayBeforeGroup = 1.5f });
            _waves.Add(w3);

            // Волна 4: 6 Пехотинцев + 5 Скаутов + 3 Голема
            var w4 = new WaveDefinition { waveName = "Волна 4: Армейский прорыв" };
            w4.groups.Add(new WaveSubGroup { enemyType = EnemyType.Grunt, count = 6, spawnInterval = 1.0f });
            w4.groups.Add(new WaveSubGroup { enemyType = EnemyType.Scout, count = 5, spawnInterval = 0.8f, delayBeforeGroup = 1.0f });
            w4.groups.Add(new WaveSubGroup { enemyType = EnemyType.Golem, count = 3, spawnInterval = 2.0f, delayBeforeGroup = 1.5f });
            _waves.Add(w4);

            // Волна 5: Финальная битва (8 Скаутов + 6 Големов + 10 Пехотинцев)
            var w5 = new WaveDefinition { waveName = "Волна 5: Генеральное наступление" };
            w5.groups.Add(new WaveSubGroup { enemyType = EnemyType.Grunt, count = 8, spawnInterval = 0.9f });
            w5.groups.Add(new WaveSubGroup { enemyType = EnemyType.Scout, count = 8, spawnInterval = 0.7f, delayBeforeGroup = 1.0f });
            w5.groups.Add(new WaveSubGroup { enemyType = EnemyType.Golem, count = 5, spawnInterval = 1.8f, delayBeforeGroup = 1.5f });
            _waves.Add(w5);
        }

        private IEnumerator WaveProgressionRoutine()
        {
            yield return new WaitForSeconds(1.5f); // Короткая пауза в начале игры для осмотра

            while (_currentWaveIndex + 1 < _waves.Count)
            {
                _currentWaveIndex++;
                OnWaveChanged?.Invoke(CurrentWave, TotalWaves);

                // Таймер отсчета перед стартом волны
                _countdownTimer = countdownBetweenWaves;
                while (_countdownTimer > 0)
                {
                    OnWaveCountdown?.Invoke(_countdownTimer);
                    yield return new WaitForSeconds(1f);
                    _countdownTimer -= 1f;
                }
                OnWaveCountdown?.Invoke(0f);

                // Старт волны
                _isWaveActive = true;
                _isSpawningWave = true;
                OnWaveStateChanged?.Invoke(true);

                yield return StartCoroutine(SpawnWaveRoutine(_waves[_currentWaveIndex]));
                _isSpawningWave = false;

                // Ждем, пока все враги волны будут уничтожены или дойдут до базы
                while (_activeEnemies.Count > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                _isWaveActive = false;
                OnWaveStateChanged?.Invoke(false);

                // Награда за успешное завершение волны
                if (GameManager.Instance != null && _waves[_currentWaveIndex].bonusGoldReward > 0)
                {
                    GameManager.Instance.AddGold(_waves[_currentWaveIndex].bonusGoldReward);
                }

                yield return new WaitForSeconds(2.0f);
            }

            // Все волны отбиты - Победа!
            if (GameManager.Instance != null && GameManager.Instance.BaseHealth > 0)
            {
                GameManager.Instance.TriggerVictory();
            }
        }

        private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
        {
            List<Vector3> path = GridManager.Instance.GetEnemyPath();

            foreach (var group in wave.groups)
            {
                if (group.delayBeforeGroup > 0)
                {
                    yield return new WaitForSeconds(group.delayBeforeGroup);
                }

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyType, path);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
        }

        private void SpawnEnemy(EnemyType type, List<Vector3> path)
        {
            GameObject enemyObj = new GameObject($"Enemy_{type}");
            Enemy enemy = enemyObj.AddComponent<Enemy>();
            enemy.Initialize(type, path);

            _activeEnemies.Add(enemy);
        }

        public void UnregisterEnemy(Enemy enemy)
        {
            _activeEnemies.Remove(enemy);
        }

        public void SkipCountdownAndStartWave()
        {
            if (!IsWaveInProgress && _countdownTimer > 0)
            {
                _countdownTimer = 0.1f;
            }
        }

        public Enemy GetTargetForTower(Vector3 towerPos, float range, TargetPriority priority)
        {
            Enemy bestEnemy = null;
            float bestMetric = (priority == TargetPriority.Closest) ? float.MaxValue : float.MinValue;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                Enemy e = _activeEnemies[i];
                if (e == null || e.IsDead) continue;

                float dist = Vector3.Distance(towerPos, e.transform.position);
                if (dist <= range)
                {
                    switch (priority)
                    {
                        case TargetPriority.First:
                            // Ближайший к базе C (наименьшее расстояние до контрольной точки)
                            float distToEnd = e.DistanceToTarget;
                            if (distToEnd < (bestEnemy == null ? float.MaxValue : bestEnemy.DistanceToTarget))
                            {
                                bestEnemy = e;
                            }
                            break;

                        case TargetPriority.Closest:
                            // Ближайший к самой башне
                            if (dist < bestMetric)
                            {
                                bestMetric = dist;
                                bestEnemy = e;
                            }
                            break;

                        case TargetPriority.Strongest:
                            // С наибольшим текущим здоровьем
                            if (e.CurrentHealth > bestMetric)
                            {
                                bestMetric = e.CurrentHealth;
                                bestEnemy = e;
                            }
                            break;
                    }
                }
            }

            return bestEnemy;
        }

        public List<Enemy> GetEnemiesInSplashRadius(Vector3 center, float radius)
        {
            List<Enemy> list = new List<Enemy>();
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                Enemy e = _activeEnemies[i];
                if (e != null && !e.IsDead)
                {
                    if (Vector3.Distance(center, e.transform.position) <= radius)
                    {
                        list.Add(e);
                    }
                }
            }
            return list;
        }
    }
}
