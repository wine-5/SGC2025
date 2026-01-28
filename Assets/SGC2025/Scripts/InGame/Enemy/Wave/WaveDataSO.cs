using UnityEngine;
using System.Collections.Generic;
using SGC2025.Enemy;

namespace SGC2025
{
    /// <summary>
    /// Waveの設定データを定義するScriptableObject
    /// 30秒間隔で自動進行するWaveシステムの設定データ
    /// WaveManagerによって時間ベースで自動的にWaveが変更される
    /// </summary>
    [CreateAssetMenu(fileName = "New Wave Data", menuName = "SGC2025/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        [System.Serializable]
        public class WaveData
        {
            [Header("Wave基本設定")]
            [Tooltip("Wave名（UI表示用）")]
            public string waveName = "Wave 1";
            [Tooltip("Waveレベル（敵の強さに影響）")]
            public int waveLevel = 1;
            [Tooltip("このWaveの説明")]
            public string explain;
            
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

            // 指定レベルのWaveが存在しない場合のフォールバック
            // loopLastWave が有効なら「最後のWave」を返して以降のWaveをループさせる
            if (loopLastWave && waves != null && waves.Count > 0)
            {
                // waves は OnValidate で waveLevel 昇順に並ぶ想定
                var lastWave = waves[waves.Count - 1];
                if (waveLevel > lastWave.waveLevel)
                {
                    return lastWave;
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
        /// 最後のWaveの終了時間を取得（30秒間隔ベース）
        /// </summary>
        public float GetLastWaveTime()
        {
            if (waves.Count == 0) return 0f;
            
            // 最大waveLevelを取得
            int maxWaveLevel = 0;
            foreach (var wave in waves)
            {
                if (wave.waveLevel > maxWaveLevel)
                    maxWaveLevel = wave.waveLevel;
            }
            
            return maxWaveLevel * minWaveInterval;
        }
        
        /// <summary>
        /// バリデーション
        /// </summary>
        private void OnValidate()
        {
            // waveLevelの重複チェック
            for (int i = 0; i < waves.Count; i++)
            {
                for (int j = i + 1; j < waves.Count; j++)
                {
                    if (waves[i].waveLevel == waves[j].waveLevel)
                    {
                        Debug.LogWarning($"[WaveDataSO] Wave {i} と Wave {j} のwaveLevelが重複しています: {waves[i].waveLevel}");
                    }
                }
            }
            
            // waveLevel順ソート
            waves.Sort((a, b) => a.waveLevel.CompareTo(b.waveLevel));
        }
    }
}