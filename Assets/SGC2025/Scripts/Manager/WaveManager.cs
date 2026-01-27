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
            
            // 初期化時のログ
            string modeText = useTestMode ? $"[テストモード] {testWaveInterval}秒間隔" : $"通常モード {waveInterval}秒間隔";
            Debug.Log($"[WaveManager] Waveシステム初期化: {modeText}");
            
            if (enableVerboseLogging)
            {
                if (waveData == null)
                {
                    Debug.LogError("[WaveManager] WaveDataSOが設定されていません！");
                }
                else
                {
                    Debug.Log($"[WaveManager] WaveDataSOが設定されています: {waveData.name}");
                    Debug.Log($"[WaveManager] 総 Wave数: {waveData.GetWaveCount()}");
                }
                
                LogWaveDataDetails();
            }
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
            Debug.Log($"[WaveManager] {modeText} Wave changed from {previousWave} to {currentWaveLevel} at time {GameElapsedTime:F1}s");
            
            // 詳細ログ（WaveDataの内容確認）
            if (enableVerboseLogging)
            {
                LogWaveDataDetails();
            }
            
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
        
        /// <summary>
        /// 現在のWaveDataの詳細をログ出力
        /// </summary>
        private void LogWaveDataDetails()
        {
            if (currentWave == null)
            {
                Debug.LogWarning($"[WaveManager] Wave Level {currentWaveLevel}: WaveDataがnullです！");
                return;
            }
            
            Debug.Log($"[WaveManager] === Wave Details ===\n" +
                     $"Wave Name: {currentWave.waveName}\n" +
                     $"Wave Level: {currentWave.waveLevel}\n" +
                     $"Spawn Interval: {currentWave.spawnInterval}s\n" +
                     $"Max Enemy Count: {currentWave.maxEnemyCount}\n" +
                     $"Enemy Configs Count: {currentWave.enemyConfigs?.Count ?? 0}");
            
            if (currentWave.enemyConfigs != null && currentWave.enemyConfigs.Count > 0)
            {
                for (int i = 0; i < currentWave.enemyConfigs.Count; i++)
                {
                    var config = currentWave.enemyConfigs[i];
                    if (config != null)
                    {
                        Debug.Log($"[WaveManager] Enemy Config [{i}]: {config.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[WaveManager] Enemy Config [{i}]: NULL");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Enemy Configsが設定されていません！");
            }
            
            Debug.Log($"[WaveManager] Has Valid Enemy Configs: {currentWave.HasValidEnemyConfigs()}");
        }
        
        /// <summary>
        /// テスト用: 現在のWave情報をコンソールに出力
        /// </summary>
        [ContextMenu("Show Current Wave Info")]
        private void ShowCurrentWaveInfo()
        {
            string modeText = useTestMode ? "[テストモード]" : "[通常モード]";
            float interval = useTestMode ? testWaveInterval : waveInterval;
            
            Debug.Log($"[WaveManager] {modeText} \n" +
                     $"Current Game Time: {GameElapsedTime:F1}s\n" +
                     $"Wave Interval: {interval}s\n" +
                     $"Current Wave Level: {currentWaveLevel}\n" +
                     $"Next Wave at: {currentWaveLevel * interval:F1}s");
                     
            LogWaveDataDetails();
        }
        
        private void StopWaveProgression() => isGameActive = false;
        
        private void PauseWaveProgression() => isGameActive = false;
        
        private void ResumeWaveProgression() => isGameActive = true;
    }
}