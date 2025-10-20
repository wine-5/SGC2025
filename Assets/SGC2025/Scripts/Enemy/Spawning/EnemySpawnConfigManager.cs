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
        /// <param name="waveLevel">ウェーブレベル</param>
        /// <returns>選択された敵データ</returns>
        public EnemyDataSO SelectRandomEnemyData(int waveLevel = 1)
        {
            var validConfigs = GetValidConfigs();
            if (validConfigs.Count == 0)
            {
                Debug.LogError("EnemySpawnConfigManager: 有効な敵生成設定がありません");
                return null;
            }

            // 全ての設定から利用可能な敵を収集
            var allAvailableEnemies = CollectAllAvailableEnemies(validConfigs, waveLevel);
            if (allAvailableEnemies.Count == 0)
            {
                Debug.LogWarning($"EnemySpawnConfigManager: ウェーブレベル {waveLevel} で出現可能な敵がいません");
                return null;
            }

            // 重み付きランダムで選択
            var selectedData = SelectByWeight(allAvailableEnemies);

            return selectedData;
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
                {
                    return enemyData;
                }
            }

            Debug.LogWarning($"EnemySpawnConfigManager: EnemyType {enemyType} のデータが見つかりません");
            return null;
        }

        /// <summary>
        /// 有効な設定のリストを取得
        /// </summary>
        private List<EnemySpawnConfigSO> GetValidConfigs()
        {
            return spawnConfigs.Where(config => config != null).ToList();
        }

        /// <summary>
        /// すべての設定から利用可能な敵を収集
        /// </summary>
        private List<EnemySpawnConfigSO.EnemySpawnData> CollectAllAvailableEnemies(
            List<EnemySpawnConfigSO> configs, int waveLevel)
        {
            var allEnemies = new List<EnemySpawnConfigSO.EnemySpawnData>();

            foreach (var config in configs)
            {
                var availableEnemies = config.GetAvailableEnemiesForWave(waveLevel);
                allEnemies.AddRange(availableEnemies);
            }

            return allEnemies;
        }

        /// <summary>
        /// 重み付きランダムで選択
        /// </summary>
        private EnemyDataSO SelectByWeight(List<EnemySpawnConfigSO.EnemySpawnData> enemies)
        {
            float totalWeight = enemies.Sum(enemy => enemy.spawnWeight);
            float randomValue = Random.Range(0f, totalWeight);

            float currentWeight = 0f;
            foreach (var enemy in enemies)
            {
                currentWeight += enemy.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemy.enemyData;
                }
            }

            // フォールバック：最初の敵を返す
            return enemies[0].enemyData;
        }

    }
}