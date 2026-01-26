using UnityEngine;
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
        private float startCountDownTime = 3f;

        [SerializeField, Tooltip("ゲームの制限時間（秒）")]
        private float gameTimeLimit = 300f;

        private bool isGameOver;
        private bool isCountDown;
        private float currentCountDownTimer;
        private float countGameTimer;

        public static event System.Action OnGameOver;
        public static event System.Action OnCountDownFinished;
        public static event System.Action OnGameTimeUp;

        public bool IsGameOver => isGameOver;
        public bool IsCountingDown => isCountDown;
        public float GameTimeLimit => gameTimeLimit;
        public float CurrentGameTime => countGameTimer;
        public float RemainingGameTime => gameTimeLimit - countGameTimer;
        public float CountDownTimer => currentCountDownTimer;
        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            PlayerCharacter.OnPlayerDeath += HandlePlayerDeath;
            InitializeGameState();
        }

        private void Start()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.InGame);
        }

        protected override void OnDestroy()
        {
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;
            
            // Time.timeScaleを確実にリセット（ポーズ中に破棄された場合に備えて）
            Time.timeScale = 1f;
            
            // static eventsをクリア
            OnGameOver = null;
            OnCountDownFinished = null;
            OnGameTimeUp = null;
            
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
            
            if (ScoreManager.I != null)
                ScoreManager.I.ResetScore();
        }

        private void Update()
        {
            if (isGameOver) return;
            
            // ポーズ中は時間を進めない
            if (PauseManager.I != null && PauseManager.I.IsPaused) return;
            
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
                
                // GameManagerに結果シーンへの遷移を依頼
                if (GameManager.I != null)
                    GameManager.I.LoadResultScene();
            }
        }

        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            if (AudioManager.I != null)
                AudioManager.I.StopBGM(true);
            
            OnGameOver?.Invoke();
            
            // GameManagerに結果シーンへの遷移を依頼
            if (GameManager.I != null)
                GameManager.I.LoadResultScene();
        }
    }
}