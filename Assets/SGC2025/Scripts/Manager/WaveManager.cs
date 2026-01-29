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
        private const int MIN_WAVE_LEVEL = 1;
        private const int MAX_WAVE_LEVEL = 10;

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
            
            OnWaveChanged?.Invoke(currentWaveLevel);
            OnWaveDataChanged?.Invoke(currentWave);
            
            NotifyEnemySpawners();
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
        
        private void NotifyEnemySpawners()
        {
            var spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
            
            foreach (var spawner in spawners)
                spawner.SetWaveLevel(currentWaveLevel);
        }
        
        private void StopWaveProgression() => isGameActive = false;
        private void PauseWaveProgression() => isGameActive = false;
        private void ResumeWaveProgression() => isGameActive = true;
    }
}