using UnityEngine;
using System.Collections.Generic;
using Tyotyo.InGame.Enemy;

namespace Tyotyo.InGame.Enemy
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

            [Header("スポーン設定")]
            [Tooltip("敵のスポーン間隔（秒）")]
            public float spawnInterval = 2f;

            [Header("使用する敵生成設定")]
            [Tooltip("このWaveで使用するEnemySpawnConfigSO")]
            public List<EnemySpawnConfigSO> enemyConfigs = new List<EnemySpawnConfigSO>();
        }
        
        [Header("Wave設定リスト")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        
        [Header("全体設定")]
        [Tooltip("最後のWave後もゲームを継続するか")]
        [SerializeField] private bool loopLastWave = true;
        
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