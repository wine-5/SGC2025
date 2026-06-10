using UnityEngine;
using SGC2025.Core;
using SGC2025.Player;
using SGC2025.Audio;

namespace SGC2025.Manager
{
    /// <summary>
    /// ゲーム内の時間管理、カウントダウン、プレイヤー死亡処理を行うマネージャー
    /// シーン固有の機能のため、DontDestroyOnLoadは使用しない
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        [Header("時間設定")]
        [SerializeField, Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
        private float startCountDownTime = 4f;

        [SerializeField, Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 300f;

        [Header("ポーズ設定")]
        [SerializeField] private PauseManager pauseManager;

        private bool isGameOver;
        private bool isCountDown;
        private float currentCountDownTimer;
        private float countGameTimer;

        public bool IsGameOver => isGameOver;
        public bool IsCountingDown => isCountDown;
        public float GameTimeLimit => gameTimeLimit;
        public float CurrentGameTime => countGameTimer;
        public float RemainingGameTime => gameTimeLimit - countGameTimer;
        public float CountDownTimer => currentCountDownTimer;

        /// <summary>ポーズ中か</summary>
        public bool IsPaused => pauseManager != null && pauseManager.IsPaused;
        /// <summary>ゲームをポーズする</summary>
        public void Pause() => pauseManager?.PauseGame();
        /// <summary>ポーズを解除する</summary>
        public void Resume() => pauseManager?.ResumeGame();

        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDeath);
            InitializeGameState();
        }

        private void Start()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.InGame);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(HandlePlayerDeath);
            
            // Time.timeScaleを確実にリセット（ポーズ中に破棄された場合に備えて）
            Time.timeScale = 1f;
            
            base.OnDestroy();
        }

        /// <summary>
        /// ゲーム状態を初期化
        /// </summary>
        private void InitializeGameState()
        {
            isGameOver = false;
            isCountDown = true;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (isGameOver) return;
            
            // ポーズ中は時間を進めない
            if (IsPaused) return;
            
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
                EventBus.Publish(new CountDownFinishedEvent());
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
                
                EventBus.Publish(new GameTimeUpEvent());
                
                if (GameManager.I != null)
                    GameManager.I.LoadResultScene();
            }
        }

        private void HandlePlayerDeath(PlayerDiedEvent e)
        {
            if (isGameOver) return;
            isGameOver = true;
            
            if (AudioManager.I != null)
                AudioManager.I.StopBGM(true);
            
            if (GameManager.I != null)
                GameManager.I.LoadResultScene();
        }
    }
}