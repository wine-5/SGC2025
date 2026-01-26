using UnityEngine;
using System.Collections.Generic;
using SGC2025.Enemy;

namespace SGC2025
{
    /// <summary>
    /// Waveの設定データを定義するScriptableObject
    /// 時間経過による敵の出現パターンとレベルスケーリングを管理
    /// </summary>
    [CreateAssetMenu(fileName = "New Wave Data", menuName = "SGC2025/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        [System.Serializable]
        public class WaveData
        {
            [Header("Wave基本設定")]
            [Tooltip("このWaveが開始される時間（秒）")]
            public float startTime = 0f;
            
            [Tooltip("Waveレベル（敵の強さに影響）")]
            public int waveLevel = 1;
            
            [Tooltip("Wave名（UI表示用）")]
            public string waveName = "Wave 1";
            
            [Header("スポーン設定")]
            [Tooltip("敵のスポーン間隔（秒）")]
            public float spawnInterval = 2f;
            
            [Tooltip("同時出現可能な敵の最大数")]
            public int maxEnemyCount = 10;
            
            [Header("使用する敵生成設定")]
            [Tooltip("このWaveで使用するEnemySpawnConfigSO")]
            public List<EnemySpawnConfigSO> enemyConfigs = new List<EnemySpawnConfigSO>();
            
            [Header("デバッグ情報")]
            [Tooltip("このWaveのデバッグログを有効にする")]
            public bool enableDebugLog = false;
            
            /// <summary>
            /// 指定時間でこのWaveが開始されるかチェック
            /// </summary>
            public bool ShouldStartAtTime(float currentTime)
            {
                return currentTime >= startTime;
            }
            
            /// <summary>
            /// このWaveが有効な敵設定を持っているかチェック
            /// </summary>
            public bool HasValidEnemyConfigs()
            {
                return enemyConfigs != null && enemyConfigs.Count > 0 && 
                       enemyConfigs.Exists(config => config != null);
            }
        }
        
        [Header("Wave設定リスト")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        
        [Header("全体設定")]
        [Tooltip("最後のWave後もゲームを継続するか")]
        [SerializeField] private bool loopLastWave = true;
        
        [Tooltip("Wave間の最小時間間隔（秒）")]
        [SerializeField] private float minWaveInterval = 30f;
        
        /// <summary>
        /// すべてのWaveデータを取得
        /// </summary>
        public List<WaveData> GetAllWaves()
        {
            return new List<WaveData>(waves);
        }
        
        /// <summary>
        /// 指定時間で開始すべきWaveを取得
        /// </summary>
        public WaveData GetWaveForTime(float currentTime)
        {
            WaveData activeWave = null;
            
            foreach (var wave in waves)
            {
                if (wave.ShouldStartAtTime(currentTime))
                {
                    activeWave = wave;
                }
                else
                {
                    break; // 時間順に並んでいる前提
                }
            }
            
            return activeWave;
        }
        
        /// <summary>
        /// 指定時間でのWaveレベルを取得（WaveManager用）
        /// </summary>
        public int GetWaveLevelAtTime(float gameTime)
        {
            var activeWave = GetWaveForTime(gameTime);
            return activeWave?.waveLevel ?? 1;
        }
        
        /// <summary>
        /// 指定Waveレベルのデータを取得（WaveManager用）
        /// </summary>
        public WaveData GetWaveDataAtLevel(int waveLevel)
        {
            foreach (var wave in waves)
            {
                if (wave.waveLevel == waveLevel)
                {
                    return wave;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 次のWaveレベルを取得（WaveManager用）
        /// </summary>
        public int GetNextWaveLevel(int currentLevel)
        {
            var nextWave = GetNextWaveData(currentLevel);
            return nextWave?.waveLevel ?? currentLevel;
        }
        
        /// <summary>
        /// 次のWaveデータを取得（WaveManager用）
        /// </summary>
        public WaveData GetNextWaveData(int currentLevel)
        {
            // 現在のレベルより高いレベルの最初のWaveを返す
            foreach (var wave in waves)
            {
                if (wave.waveLevel > currentLevel)
                {
                    return wave;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 次のWaveを取得
        /// </summary>
        public WaveData GetNextWave(WaveData currentWave)
        {
            if (currentWave == null) return GetFirstWaveOrNull();
            
            int currentIndex = waves.IndexOf(currentWave);
            if (currentIndex == -1 || currentIndex >= waves.Count - 1)
            {
                return loopLastWave ? waves[waves.Count - 1] : null;
            }
            
            return waves[currentIndex + 1];
        }
        
        /// <summary>
        /// 最初のWaveを取得（nullの場合もある）
        /// </summary>
        public WaveData GetFirstWaveOrNull()
        {
            return waves.Count > 0 ? waves[0] : null;
        }
        
        /// <summary>
        /// 総Wave数を取得
        /// </summary>
        public int GetWaveCount()
        {
            return waves.Count;
        }
        
        /// <summary>
        /// 最後のWave終了時間を取得
        /// </summary>
        public float GetLastWaveTime()
        {
            return waves.Count > 0 ? waves[waves.Count - 1].startTime : 0f;
        }
        
        /// <summary>
        /// バリデーション
        /// </summary>
        private void OnValidate()
        {
            // Wave開始時間の重複チェック
            for (int i = 0; i < waves.Count; i++)
            {
                for (int j = i + 1; j < waves.Count; j++)
                {
                    if (Mathf.Abs(waves[i].startTime - waves[j].startTime) < 0.1f)
                    {
                        Debug.LogWarning($"[WaveConfigSO] Wave {i} と Wave {j} の開始時間が重複しています");
                    }
                }
            }
            
            // 時間順ソート
            waves.Sort((a, b) => a.startTime.CompareTo(b.startTime));
            
            // 最小間隔チェック
            for (int i = 1; i < waves.Count; i++)
            {
                float interval = waves[i].startTime - waves[i - 1].startTime;
                if (interval < minWaveInterval)
                {
                    Debug.LogWarning($"[WaveConfigSO] Wave間隔が短すぎます: {interval}秒 (最小: {minWaveInterval}秒)");
                }
            }
        }
    }
}