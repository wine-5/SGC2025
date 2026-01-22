using UnityEngine;
using TechC;
using SGC2025.Enemy;
using System.Collections.Generic;
using System.Linq;

namespace SGC2025
{
    /// <summary>
    /// Waveシステムを管理するマネージャー
    /// 時間経過に応じてWaveレベルを変更し、敵の出現パターンを制御
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        [Header("Wave設定")]
        [SerializeField] private WaveConfigSO waveConfig;
        [SerializeField] private float gameStartTime = 0f;
        
        [Header("デバッグ設定")]
        [SerializeField] private bool enableDebugLog = false;
        
        // 内部状態
        private int currentWaveLevel = 1;
        private float gameElapsedTime = 0f;
        private bool isGameActive = true;
        private WaveConfigSO.WaveData currentWave;
        
        // イベント
        public static event System.Action<int> OnWaveChanged;
        public static event System.Action<WaveConfigSO.WaveData> OnWaveDataChanged;
        
        // プロパティ
        public int CurrentWaveLevel => currentWaveLevel;
        public float GameElapsedTime => gameElapsedTime;
        public WaveConfigSO.WaveData CurrentWave => currentWave;
        public bool IsGameActive => isGameActive;
        
        protected override void Init()
        {
            base.Init();
            
            // ゲーム状態イベントを購読
            GameManager.OnGameOver += StopWaveProgression;
            GameManager.OnGamePause += PauseWaveProgression;
            GameManager.OnGameResume += ResumeWaveProgression;
            
            InitializeWaveSystem();
            
            if (enableDebugLog)
            {
                Debug.Log("[WaveManager] 初期化完了");
            }
        }
        
        protected override void OnDestroy()
        {
            // イベントの購読解除
            GameManager.OnGameOver -= StopWaveProgression;
            GameManager.OnGamePause -= PauseWaveProgression;
            GameManager.OnGameResume -= ResumeWaveProgression;
            
            base.OnDestroy();
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            UpdateGameTime();
            CheckWaveProgression();
        }
        
        /// <summary>
        /// Waveシステムを初期化
        /// </summary>
        private void InitializeWaveSystem()
        {
            if (waveConfig == null)
            {
                Debug.LogError("[WaveManager] WaveConfigSOが設定されていません！");
                return;
            }
            
            gameElapsedTime = gameStartTime;
            currentWaveLevel = 1;
            
            // 最初のWaveデータを設定
            UpdateCurrentWaveData();
            
            if (enableDebugLog)
            {
                Debug.Log($"[WaveManager] Waveシステム初期化 - 開始Wave: {currentWaveLevel}");
            }
        }
        
        /// <summary>
        /// ゲーム時間を更新
        /// </summary>
        private void UpdateGameTime()
        {
            gameElapsedTime += Time.deltaTime;
        }
        
        /// <summary>Wave進行をチェック</summary>
        private void CheckWaveProgression()
        {
            if (waveConfig == null) return;
            int newWaveLevel = waveConfig.GetWaveLevelAtTime(gameElapsedTime);
            if (newWaveLevel != currentWaveLevel) ChangeWave(newWaveLevel);
        }
        
        /// <summary>
        /// Waveを変更
        /// </summary>
        /// <param name="newWaveLevel">新しいWaveレベル</param>
        private void ChangeWave(int newWaveLevel)
        {
            int previousWave = currentWaveLevel;
            currentWaveLevel = newWaveLevel;
            
            UpdateCurrentWaveData();
            
            // イベント発火
            OnWaveChanged?.Invoke(currentWaveLevel);
            OnWaveDataChanged?.Invoke(currentWave);
            
            // EnemySpawnerに新しいWaveレベルを通知
            NotifyEnemySpawners();
            
            if (enableDebugLog)
            {
                Debug.Log($"[WaveManager] Wave変更: {previousWave} → {currentWaveLevel} (時間: {gameElapsedTime:F1}秒)");
            }
        }
        
        /// <summary>
        /// 現在のWaveデータを更新
        /// </summary>
        private void UpdateCurrentWaveData()
        {
            if (waveConfig != null)
            {
                currentWave = waveConfig.GetWaveDataAtLevel(currentWaveLevel);
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
                
                if (currentWave != null)
                {
                    spawner.SetSpawnInterval(currentWave.spawnInterval);
                }
            }
            
            if (enableDebugLog && spawners.Length > 0)
            {
                Debug.Log($"[WaveManager] {spawners.Length}個のEnemySpawnerにWave {currentWaveLevel}を通知");
            }
        }
        
        /// <summary>
        /// Wave進行を停止
        /// </summary>
        private void StopWaveProgression()
        {
            isGameActive = false;
            
            if (enableDebugLog)
            {
                Debug.Log("[WaveManager] Wave進行を停止しました");
            }
        }
        
        /// <summary>
        /// Wave進行を一時停止
        /// </summary>
        private void PauseWaveProgression()
        {
            isGameActive = false;
        }
        
        /// <summary>
        /// Wave進行を再開
        /// </summary>
        private void ResumeWaveProgression()
        {
            isGameActive = true;
        }
        
        /// <summary>
        /// Wave情報を取得（UI表示用）
        /// </summary>
        public string GetWaveInfoText()
        {
            if (currentWave != null)
            {
                return $"Wave {currentWaveLevel} - {currentWave.waveName}";
            }
            return $"Wave {currentWaveLevel}";
        }
        
        /// <summary>
        /// 次のWaveまでの残り時間を取得
        /// </summary>
        public float GetTimeToNextWave()
        {
            if (waveConfig == null) return 0f;
            
            var nextWave = waveConfig.GetNextWaveData(currentWaveLevel);
            if (nextWave != null)
            {
                return Mathf.Max(0f, nextWave.startTime - gameElapsedTime);
            }
            
            return 0f;
        }
    }
}