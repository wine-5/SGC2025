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
        [SerializeField] private WaveConfigSO waveConfig;
        [SerializeField] private float gameStartTime = 0f;
        
        private int currentWaveLevel = 1;
        private float gameElapsedTime = 0f;
        private bool isGameActive = true;
        private WaveConfigSO.WaveData currentWave;
        
        public static event Action<int> OnWaveChanged;
        public static event Action<WaveConfigSO.WaveData> OnWaveDataChanged;
        
        public int CurrentWaveLevel => currentWaveLevel;
        public float GameElapsedTime => gameElapsedTime;
        public WaveConfigSO.WaveData CurrentWave => currentWave;
        public bool IsGameActive => isGameActive;
        
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
                Debug.LogError("[WaveManager] WaveConfigSO is not assigned!");
                return;
            }
            
            gameElapsedTime = gameStartTime;
            currentWaveLevel = 1;
            UpdateCurrentWaveData();
        }
        
        private void UpdateGameTime() => gameElapsedTime += Time.deltaTime;
        
        /// <summary>
        /// Wave進行をチェック
        /// </summary>
        private void CheckWaveProgression()
        {
            if (waveConfig == null) return;
            
            int newWaveLevel = waveConfig.GetWaveLevelAtTime(gameElapsedTime);
            if (newWaveLevel != currentWaveLevel)
                ChangeWave(newWaveLevel);
        }
        
        /// <summary>
        /// Waveを変更
        /// </summary>
        private void ChangeWave(int newWaveLevel)
        {
            currentWaveLevel = newWaveLevel;
            UpdateCurrentWaveData();
            
            OnWaveChanged?.Invoke(currentWaveLevel);
            OnWaveDataChanged?.Invoke(currentWave);
            
            NotifyEnemySpawners();
        }
        
        /// <summary>
        /// 現在のWaveデータを更新
        /// </summary>
        private void UpdateCurrentWaveData()
        {
            if (waveConfig == null) return;
            
            currentWave = waveConfig.GetWaveDataAtLevel(currentWaveLevel);
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
                    spawner.SetSpawnInterval(currentWave.spawnInterval);
            }
        }
        
        private void StopWaveProgression() => isGameActive = false;
        
        private void PauseWaveProgression() => isGameActive = false;
        
        private void ResumeWaveProgression() => isGameActive = true;
    }
}