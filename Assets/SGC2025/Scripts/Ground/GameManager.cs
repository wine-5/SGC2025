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
        [SerializeField]
        [Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
        private float startCountDownTime = 3f;

        [SerializeField]
        [Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 600.0f;

        private bool isGameOver = false;
        private bool isPaused = false;
        private bool isCountDown = false;
        private float currentCountDownTimer = 0f;
        private float countGameTimer = 0f;

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

        /// <summary>カウントダウンタイマー更新</summary>
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

        /// <summary>ゲームタイマー更新</summary>
        private void UpdateGameTimer()
        {
            if (isCountDown) return;

            countGameTimer += Time.deltaTime;
            if (countGameTimer >= gameTimeLimit)
            {
                OnGameTimeUp?.Invoke();
            }
        }

        /// <summary>カウントダウン開始</summary>
        public void StartCountDown()
        {
            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        /// <summary>ゲームタイマーをリセット</summary>
        public void ResetGameTimer()
        {
            countGameTimer = 0f;
            isCountDown = false;
        }

        /// <summary>プレイヤー死亡時の処理</summary>
        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            Debug.Log("[GameManager] プレイヤーが死亡しました。ゲームオーバー処理を開始します");
            isGameOver = true;
            OnGameOver?.Invoke();

            // 遅延後にゲームオーバーシーンに遷移
            Invoke(nameof(LoadGameOverScene), gameOverDelay);
        }

        /// <summary>
        /// ゲームオーバーシーンに遷移
        /// </summary>
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

        /// <summary>ゲームを一時停止</summary>
        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
        }

        /// <summary>ゲームを再開</summary>
        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// 現在のシーンをリロード
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f; // TimeScaleをリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}