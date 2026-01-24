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
        [SerializeField] private float gameOverDelay = 2f;

        [Header("時間設定")]
        [SerializeField, Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
        private float startCountDownTime = 3f;

        [SerializeField, Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 600f;
        [Header("ポーズ設定")]
        [SerializeField] private GameObject pausePanel;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            InitializeGameState();
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
                
            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.InGame);
        }

        protected override void OnDestroy()
        {
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            base.OnDestroy();
        }

        /// <summary>
        /// シーンがロードされたときの処理
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneName.InGame.ToString())
            {
                InitializeGameState();
                
                if (pausePanel != null)
                    pausePanel.SetActive(false);
                    
                if (AudioManager.I != null)
                    AudioManager.I.PlayBGM(BGMType.InGame);
            }
        }

        /// <summary>
        /// ゲーム状態を初期化
        /// </summary>
        private void InitializeGameState()
        {
            isGameOver = false;
            isPaused = false;
            isCountDown = true;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            
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
                Invoke(nameof(LoadGameOverScene), gameOverDelay);
            }
        }

        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            if (AudioManager.I != null)
                AudioManager.I.StopBGM(true);
            
            OnGameOver?.Invoke();
            Invoke(nameof(LoadGameOverScene), gameOverDelay);
        }

        private void LoadGameOverScene()
        {
            if (SceneController.I == null) return;
            SceneController.I.LoadResultScene();
        }

        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }

    }
}