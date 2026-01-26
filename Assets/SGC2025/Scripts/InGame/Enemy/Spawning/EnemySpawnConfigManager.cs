using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 複数のEnemySpawnConfigSOを管理し、ランダム選択を行うクラス
    /// </summary>
    [System.Serializable]
    public class EnemySpawnConfigManager
    {
        [Header("敵生成設定")]
        [SerializeField] private List<EnemySpawnConfigSO> spawnConfigs = new List<EnemySpawnConfigSO>();

        /// <summary>
        /// 登録されている設定の数を取得
        /// </summary>
        public int ConfigCount => spawnConfigs.Count(config => config != null);

        /// <summary>
        /// すべての設定が有効かチェック
        /// </summary>
        public bool HasValidConfigs => ConfigCount > 0;

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
            var randomConfig = validConfigs[Random.Range(0, validConfigs.Count)];
            return randomConfig.SelectRandomEnemy();
        }

        /// <summary>
        /// 特定の敵タイプのデータを取得
        /// </summary>
        /// <param name="enemyType">敵タイプ</param>
        /// <returns>該当する敵データ</returns>
        public EnemyDataSO GetEnemyData(EnemyType enemyType)
        {
            foreach (var config in GetValidConfigs())
            {
                var enemyData = config.GetEnemyData(enemyType);
                if (enemyData != null)
                    return enemyData;
            }

            Debug.LogWarning($"EnemySpawnConfigManager: EnemyType {enemyType} のデータが見つかりません");
            return null;
        }

        /// <summary>
        /// 有効な設定のリストを取得
        /// </summary>
        private List<EnemySpawnConfigSO> GetValidConfigs() => spawnConfigs.Where(config => config != null && config.HasValidEnemies()).ToList();
    }
}
