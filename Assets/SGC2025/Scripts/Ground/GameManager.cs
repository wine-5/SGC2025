using UnityEngine;
using UnityEngine.SceneManagement;
using TechC;

namespace SGC2025
{
    /// <summary>
    /// ゲーム全体の状態管理を行うマネージャー
    /// プレイヤーの死亡処理、ゲームオーバー、ポーズ機能、時間管理などを提供
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("ゲーム設定")]
        [SerializeField] private string gameOverSceneName = "Gameover";
        [SerializeField] private float gameOverDelay = 2f;

        [Header("時間設定")]
        [SerializeField, Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
        private float startCountDownTime = 3f;

        [SerializeField, Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 600f;

        private bool isGameOver;
        private bool isPaused;
        private bool isCountDown;
        private float currentCountDownTimer;
        private float countGameTimer;

        public static event System.Action OnGameOver;
        public static event System.Action OnGamePause;
        public static event System.Action OnGameResume;
        public static event System.Action OnCountDownFinished;
        public static event System.Action OnGameTimeUp;

        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;
        public bool IsCountingDown => isCountDown;
        public float GameTimeLimit => gameTimeLimit;
        public float CurrentGameTime => countGameTimer;
        public float RemainingGameTime => gameTimeLimit - countGameTimer;
        public float CountDownTimer => currentCountDownTimer;
        protected override bool UseDontDestroyOnLoad => true;


        protected override void Init()
        {
            base.Init();
            PlayerCharacter.OnPlayerDeath += HandlePlayerDeath;
        }

        protected override void OnDestroy()
        {
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;

            base.OnDestroy();
        }

        private void Update()
        {
            if (isGameOver || isPaused) return;
            UpdateCountDown();
            UpdateGameTimer();
        }

        private void UpdateCountDown()
        {
            if (!isCountDown) return;
            currentCountDownTimer -= Time.deltaTime;
            if (currentCountDownTimer <= 0f)
            {
                isCountDown = false;
                OnCountDownFinished?.Invoke();
            }
        }

        private void UpdateGameTimer()
        {
            if (isCountDown) return;
            countGameTimer += Time.deltaTime;
            if (countGameTimer >= gameTimeLimit)
                OnGameTimeUp?.Invoke();
        }

        public void StartCountDown()
        {
            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        public void ResetGameTimer()
        {
            countGameTimer = 0f;
            isCountDown = false;
        }

        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            isGameOver = true;
            OnGameOver?.Invoke();
            Invoke(nameof(LoadGameOverScene), gameOverDelay);
        }

        private void LoadGameOverScene()
        {
            try
            {
                SceneManager.LoadScene(gameOverSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] シーン遷移に失敗しました: {e.Message}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}