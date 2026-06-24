using System.Collections.Generic;
using UnityEngine;

namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 複数のEnemySpawnConfigSOを管理し、ランダム選択を行うクラス
    /// </summary>
    [System.Serializable]
    public class EnemySpawnConfigManager
    {
        [Header("敵生成設定")]
        [SerializeField] private List<EnemySpawnConfigSO> spawnConfigs = new List<EnemySpawnConfigSO>();

        // 有効設定はインスペクタで固定されるため、スポーンごとに確保せず一度だけ構築して使い回す
        private readonly List<EnemySpawnConfigSO> validConfigsCache = new List<EnemySpawnConfigSO>();
        private bool validConfigsBuilt;

        /// <summary>
        /// すべての設定が有効かチェック
        /// </summary>
        public bool HasValidConfigs => GetValidConfigs().Count > 0;

        /// <summary>
        /// ランダムに敵データを選択
        /// </summary>
        /// <returns>選択された敵データ</returns>
        public EnemyDataSO SelectRandomEnemyData()
        {
            var validConfigs = GetValidConfigs();
            if (validConfigs.Count == 0)
            {
                Debug.LogError("EnemySpawnConfigManager: 有効な敵生成設定がありません");
                return null;
            }

            // ランダムに設定を選択してから、その設定内で敵を選択
            int configIndex = Random.Range(0, validConfigs.Count);
            var randomConfig = validConfigs[configIndex];
            
            return randomConfig.SelectRandomEnemy();
        }

        /// <summary>
        /// 有効な設定のリストを取得（初回のみ構築し、以降はキャッシュを返す）
        /// </summary>
        private List<EnemySpawnConfigSO> GetValidConfigs()
        {
            if (validConfigsBuilt) return validConfigsCache;

            validConfigsCache.Clear();
            for (int i = 0; i < spawnConfigs.Count; i++)
            {
                var config = spawnConfigs[i];
                if (config != null && config.HasValidEnemies())
                    validConfigsCache.Add(config);
            }
            validConfigsBuilt = true;

            return validConfigsCache;
        }
    }
}
