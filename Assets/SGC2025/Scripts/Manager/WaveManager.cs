using UnityEngine;
using UnityEngine.Serialization;
using SGC2025.Core;
using SGC2025.Enemy;
using SGC2025.UI;

namespace SGC2025.Manager
{
    /// <summary>
    /// Waveシステムを管理するマネージャー
    /// 時間経過に応じてWaveレベルを変更し、敵の出現パターンを制御
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        private const int MIN_WAVE_LEVEL = 1;
        private const int MAX_WAVE_LEVEL = 10;

        [Header("Wave設定")]
        [SerializeField] private WaveDataSO waveData;
        [SerializeField] private float waveInterval = 30f; // 30秒間隔でWave変化
        
        [Header("テスト設定")]
        [Tooltip("テスト用の高速Wave切り替え (デバッグ用)")]
        [SerializeField] private bool useTestMode = false;
        [SerializeField] private float testWaveInterval = 10f;

        [Header("View参照")]
        [Tooltip("Wave表示View（進行度リング・テキストの描画のみを担当）")]
        [SerializeField, FormerlySerializedAs("waveProgressView")]
        private WaveView waveView;

        
        private int currentWaveLevel = 1;
        private bool isGameActive = true;
        private WaveDataSO.WaveData currentWave;
        
        public int CurrentWaveLevel => currentWaveLevel;
        public WaveDataSO.WaveData CurrentWave => currentWave;
        protected override bool UseDontDestroyOnLoad => false; // シーン固有のManager

        protected override void Init()
        {
            base.Init();

            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<PausedEvent>(OnPaused);
            EventBus.Subscribe<ResumedEvent>(OnResumed);

            InitializeWaveSystem();

            waveView?.Initialize();
            waveView?.SetWaveLevel(currentWaveLevel);
        }
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<PausedEvent>(OnPaused);
            EventBus.Unsubscribe<ResumedEvent>(OnResumed);
            
            base.OnDestroy();
        }
        
        private void Update()
        {
            if (!isGameActive) return;

            CheckWaveProgression();
            UpdateProgressView();
        }

        /// <summary>現在のWave内での経過進行度（0.0～1.0）を計算してViewへ渡す</summary>
        private void UpdateProgressView()
        {
            if (waveView == null) return;

            waveView.SetProgress(CalcWaveProgress());
        }

        private float CalcWaveProgress()
        {
            if (InGameManager.I == null) return 0f;

            // 最大Wave到達後はそれ以上変化しないため満タン表示で固定
            if (currentWaveLevel >= MAX_WAVE_LEVEL) return 1f;

            float interval = useTestMode ? testWaveInterval : waveInterval;
            return Mathf.Clamp01(InGameManager.I.CurrentGameTime % interval / interval);
        }
        
        private void InitializeWaveSystem()
        {
            currentWaveLevel = MIN_WAVE_LEVEL;
            UpdateCurrentWaveData();
        }
        
        private void CheckWaveProgression()
        {
            if (InGameManager.I == null) return;
            
            float currentGameTime = InGameManager.I.CurrentGameTime;
            float interval = useTestMode ? testWaveInterval : waveInterval;
            int expectedWaveLevel = Mathf.FloorToInt(currentGameTime / interval) + 1;
            
            expectedWaveLevel = Mathf.Clamp(expectedWaveLevel, MIN_WAVE_LEVEL, MAX_WAVE_LEVEL);
            
            if (expectedWaveLevel != currentWaveLevel)
                ChangeWave(expectedWaveLevel);
        }
        
        private void ChangeWave(int newWaveLevel)
        {
            currentWaveLevel = newWaveLevel;
            UpdateCurrentWaveData();

            if (waveView != null)
            {
                waveView.SetWaveLevel(currentWaveLevel);
                StartCoroutine(waveView.AnimateWaveChange());
            }

            EventBus.Publish(new WaveChangedEvent(currentWaveLevel));
        }
        
        private void UpdateCurrentWaveData()
        {
            if (waveData == null)
            {
                Debug.LogWarning("[WaveManager] WaveData is null - cannot update wave data");
                return;
            }
            
            currentWave = waveData.GetWaveDataAtLevel(currentWaveLevel);
        }
        
        private void OnGameOver(GameOverEvent e) => isGameActive = false;
        private void OnPaused(PausedEvent e) => isGameActive = false;
        private void OnResumed(ResumedEvent e) => isGameActive = true;
    }
}