using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefense.Data;

namespace TowerDefense.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action<int, int> OnBaseHealthChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<float> OnTimeScaleChanged;

        [Header("Начальные ресурсы")]
        [SerializeField] private int initialGold = 300;
        [SerializeField] private int initialBaseHealth = 10;

        public int CurrentGold { get; private set; }
        public int BaseHealth { get; private set; }
        public int MaxBaseHealth => initialBaseHealth;
        public GameState CurrentState { get; private set; } = GameState.Playing;
        public float CurrentTimeScale { get; private set; } = 1f;

        private float _previousTimeScale = 1f;

        private void Awake()
        {
            Instance = this;

            Time.timeScale = 1f;
            CurrentGold = initialGold;
            BaseHealth = initialBaseHealth;
            CurrentState = GameState.Playing;
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
            OnGoldChanged?.Invoke(CurrentGold);
            OnBaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public bool CanAfford(int cost)
        {
            return CurrentGold >= cost;
        }

        public bool SpendGold(int amount)
        {
            if (CanAfford(amount))
            {
                CurrentGold -= amount;
                OnGoldChanged?.Invoke(CurrentGold);
                return true;
            }
            return false;
        }

        public void AddGold(int amount)
        {
            CurrentGold += amount;
            OnGoldChanged?.Invoke(CurrentGold);
        }

        public void DamageBase(int damage)
        {
            if (CurrentState != GameState.Playing) return;

            BaseHealth = Mathf.Max(0, BaseHealth - damage);
            OnBaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);

            if (BaseHealth <= 0)
            {
                GameOver();
            }
        }

        public void TriggerVictory()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Won;
                OnGameStateChanged?.Invoke(CurrentState);
            }
        }

        public void GameOver()
        {
            CurrentState = GameState.GameOver;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public void SetTimeScale(float scale)
        {
            CurrentTimeScale = scale;
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = scale;
            }
            OnTimeScaleChanged?.Invoke(scale);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                OnGameStateChanged?.Invoke(CurrentState);
            }
            else if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = _previousTimeScale > 0 ? _previousTimeScale : 1f;
                OnGameStateChanged?.Invoke(CurrentState);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "SampleScene";
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
