using UnityEngine;
using TechC;
using SGC2025.Enemy;
using System;

namespace SGC2025.Manager
{
    /// <summary>
    /// Waveシステムを管理するマネージャー
    /// 時間経過に応じてWaveレベルを変更し、敵の出現パターンを制御
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        [Header("Wave設定")]
        [SerializeField] private WaveDataSO waveData;
        [SerializeField] private float waveInterval = 30f; // 30秒間隔でWave変化
        
        [Header("テスト設定")]
        [Tooltip("テスト用の高速Wave切り替え (デバッグ用)")]
        [SerializeField] private bool useTestMode = true;
        [SerializeField] private float testWaveInterval = 10f; // テスト用10秒間隔
        [SerializeField] private bool enableVerboseLogging = true; // 詳細ログ
        
        private int currentWaveLevel = 1;
        private bool isGameActive = true;
        private WaveDataSO.WaveData currentWave;
        
        public static event Action<int> OnWaveChanged;
        public static event Action<WaveDataSO.WaveData> OnWaveDataChanged;
        
        public int CurrentWaveLevel => currentWaveLevel;
        public float GameElapsedTime => InGameManager.I != null ? InGameManager.I.CurrentGameTime : 0f;
        public WaveDataSO.WaveData CurrentWave => currentWave;
        public bool IsGameActive => isGameActive;
        protected override bool UseDontDestroyOnLoad => false; // シーン固有のManager

        /// <summary>
        /// 現在のWaveデータを取得（EnemySpawner用）
        /// </summary>
        /// <returns>現在のWaveData、存在しない場合はnull</returns>
        public WaveDataSO.WaveData CurrentWave1 => currentWave;

        protected override void Init()
        {
            base.Init();
            
            // InGameManagerからゲームオーバーイベントを購諭
            InGameManager.OnGameOver += StopWaveProgression;
            
            // PauseManagerからポーズイベントを購諭
            PauseManager.OnPause += PauseWaveProgression;
            PauseManager.OnResume += ResumeWaveProgression;
            
            InitializeWaveSystem();
        }
        
        protected override void OnDestroy()
        {
            // InGameManagerからイベントの購諭を解除
            InGameManager.OnGameOver -= StopWaveProgression;
            
            // PauseManagerからイベントの購諭を解除
            PauseManager.OnPause -= PauseWaveProgression;
            PauseManager.OnResume -= ResumeWaveProgression;
            
            base.OnDestroy();
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            CheckWaveProgression();
        }
        
        /// <summary>
        /// Waveシステムを初期化
        /// </summary>
        private void InitializeWaveSystem()
        {
            currentWaveLevel = 1;
            UpdateCurrentWaveData();
            
            // Waveシステム初期化完了
            

        }
        
        /// <summary>
        /// Wave進行をチェック（設定間隔で計算）
        /// </summary>
        private void CheckWaveProgression()
        {
            if (InGameManager.I == null) return;
            
            float currentGameTime = InGameManager.I.CurrentGameTime;
            float interval = useTestMode ? testWaveInterval : waveInterval;
            int expectedWaveLevel = Mathf.FloorToInt(currentGameTime / interval) + 1;
            
            // 最大10Waveまで
            expectedWaveLevel = Mathf.Clamp(expectedWaveLevel, 1, 10);
            
            if (expectedWaveLevel != currentWaveLevel)
            {
                ChangeWave(expectedWaveLevel);
            }
        }
        
        /// <summary>
        /// Waveを変更
        /// </summary>
        private void ChangeWave(int newWaveLevel)
        {
            int previousWave = currentWaveLevel;
            currentWaveLevel = newWaveLevel;
            UpdateCurrentWaveData();
            
            // 基本ログ
            string modeText = useTestMode ? "[テストモード]" : "";
            // Wave変更ロジック（ログなし）
            
            OnWaveChanged?.Invoke(currentWaveLevel);
            OnWaveDataChanged?.Invoke(currentWave);
            
            NotifyEnemySpawners();
        }
        
        /// <summary>
        /// 現在のWaveデータを更新
        /// </summary>
        private void UpdateCurrentWaveData()
        {
            if (waveData == null)
            {
                Debug.LogWarning("[WaveManager] WaveData is null - cannot update wave data");
                return;
            }
            
            currentWave = waveData.GetWaveDataAtLevel(currentWaveLevel);
            
            if (enableVerboseLogging && currentWave == null)
            {
                Debug.LogWarning($"[WaveManager] Wave Level {currentWaveLevel}に対応するWaveDataが見つかりません");
            }
        }
        
        /// <summary>
        /// すべてのEnemySpawnerに現在のWaveレベルを通知
        /// </summary>
        private void NotifyEnemySpawners()
        {
            var spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
            
            foreach (var spawner in spawners)
            {
                spawner.SetWaveLevel(currentWaveLevel);
            }
        }
        

        
            private void StopWaveProgression() => isGameActive = false;
        
        private void PauseWaveProgression() => isGameActive = false;
        
        private void ResumeWaveProgression() => isGameActive = true;
    }
}