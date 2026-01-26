using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using SGC2025.Player;
using SGC2025.Audio;

namespace SGC2025.Manager
{
    /// <summary>
    /// ゲーム全体の状態管理を行うマネージャー
    /// プレイヤーの死亡処理、ゲームオーバー、ポーズ機能、時間管理などを提供
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("ゲーム設定")]
        [SerializeField] private float gameOverDelay = 2f;

        [Header("時間設定")]
        [SerializeField, Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
        private float startCountDownTime = 3f;

        [SerializeField, Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 600f;
        [Header("ポーズ設定")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject firstPauseButton; // ポーズ時に最初に選択されるボタン

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
        protected override bool UseDontDestroyOnLoad => false;


        protected override void Init()
        {
            base.Init();
            Debug.Log($"[GameManager] Init called - Instance: {GetInstanceID()}");
            PlayerCharacter.OnPlayerDeath += HandlePlayerDeath;
        }

        private void Start()
        {
            Debug.Log($"[GameManager] Start called - Instance: {GetInstanceID()}");
            InitializeGameState();
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
                
            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.InGame);
        }

        protected override void OnDestroy()
        {
            Debug.Log($"[GameManager] OnDestroy called - Instance: {GetInstanceID()}");
            
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;
            
            // Time.timeScaleを確実にリセット（ポーズ中に破棄された場合に備えて）
            Time.timeScale = 1f;
            
            // static eventsをクリア（リスナーが残らないように）
            OnGameOver = null;
            OnGamePause = null;
            OnGameResume = null;
            OnCountDownFinished = null;
            OnGameTimeUp = null;
            
            Debug.Log($"[GameManager] OnDestroy completed");
            
            base.OnDestroy();
        }

        /// <summary>
        /// ゲーム状態を初期化
        /// </summary>
        private void InitializeGameState()
        {
            Debug.Log($"[GameManager] InitializeGameState called - Instance: {GetInstanceID()}");
            
            isGameOver = false;
            isPaused = false;
            isCountDown = true;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            Time.timeScale = 1f;
            
            Debug.Log($"[GameManager] countGameTimer reset to: {countGameTimer}, Time.timeScale: {Time.timeScale}");
            
            if (ScoreManager.I != null)
                ScoreManager.I.ResetScore();
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
            {
                if (isGameOver) return;
                isGameOver = true;
                
                if (AudioManager.I != null)
                {
                    AudioManager.I.StopBGM(true);
                    AudioManager.I.PlaySE(SEType.TimeUp);
                }
                
                OnGameTimeUp?.Invoke();
                Invoke(nameof(LoadResultScene), gameOverDelay);
            }
        }

        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            if (AudioManager.I != null)
                AudioManager.I.StopBGM(true);
            
            OnGameOver?.Invoke();
            Invoke(nameof(LoadResultScene), gameOverDelay);
        }

        private void LoadResultScene()
        {
            // ポーズ中にゲームオーバーになった場合に備えてTime.timeScaleをリセット
            Time.timeScale = 1f;
            
            if (SceneController.I == null) return;

            if (GroundManager.I != null && ScoreManager.I != null)
            {
                float greeningRate = GroundManager.I.GetGreenificationRate();
                ScoreManager.I.SaveGreeningRate(greeningRate);
            }

            SceneController.I.LoadResultScene();
            AudioManager.I.PlayBGM(BGMType.Result);
        }

        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            isPaused = true;
            
            if (pausePanel != null)
                pausePanel.SetActive(true);
            
            Time.timeScale = 0f;
            
            if (firstPauseButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstPauseButton);
            
            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            isPaused = false;
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
            
            Time.timeScale = 1f;
            
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            
            OnGameResume?.Invoke();
        }

    }
}