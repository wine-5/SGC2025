using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成設定を管理するScriptableObject
    /// 重み付けによる敵選択を設定（Wave制御はWaveDataSOで行う）
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Spawn Config", menuName = "SGC2025/Enemy/Enemy Spawn Config")]
    public class EnemySpawnConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            [Header("敵データ")]
            public EnemyDataSO enemyData;
            
            [Header("生成設定")]
            [Range(0f, 100f)]
            [Tooltip("この敵が選択される重み（高いほど出現しやすい）")]
            public float spawnWeight = 10f;
            
            /// <summary>
            /// 有効な敵データかチェック
            /// </summary>
            public bool IsValid()
            {
                return enemyData != null && spawnWeight > 0f;
            }
        }
        
        [Header("敵生成データ")]
        [SerializeField] private List<EnemySpawnData> enemySpawnDataList = new List<EnemySpawnData>();
        
        /// <summary>
        /// 有効な敵のリストを取得
        /// </summary>
        public List<EnemySpawnData> GetValidEnemies()
        {
            return enemySpawnDataList.Where(data => 
                data != null && data.IsValid()).ToList();
        }
        
        /// <summary>
        /// 重み付けによるランダム敵選択
        /// </summary>
        public EnemyDataSO SelectRandomEnemy()
        {
            var validEnemies = GetValidEnemies();
            if (validEnemies.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnConfigSO] 有効な敵が設定されていません");
                return null;
            }
            
            float totalWeight = GetTotalWeight(validEnemies);
            float randomValue = Random.Range(0f, totalWeight);
            
            return FindEnemyByWeight(validEnemies, randomValue);
        }
        
        /// <summary>
        /// 総重みを計算
        /// </summary>
        private float GetTotalWeight(List<EnemySpawnData> enemies)
        {
            return enemies.Sum(enemy => enemy.spawnWeight);
        }
        
        /// <summary>
        /// 重みに基づいて敵を検索
        /// </summary>
        private EnemyDataSO FindEnemyByWeight(List<EnemySpawnData> enemies, float randomValue)
        {
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
        
        /// <summary>
        /// 特定の敵タイプのデータを取得
        /// </summary>
        public EnemyDataSO GetEnemyData(EnemyType enemyType)
        {
            var spawnData = enemySpawnDataList.FirstOrDefault(data => 
                data != null && 
                data.IsValid() && 
                data.enemyData.EnemyType == enemyType
            );
            return spawnData?.enemyData;
        }
        
        /// <summary>
        /// 登録されている敵データの数を取得
        /// </summary>
        public int GetEnemyDataCount()
        {
            return GetValidEnemies().Count;
        }
        
        /// <summary>
        /// 設定に有効な敵が含まれているかチェック
        /// </summary>
        public bool HasValidEnemies()
        {
            return GetEnemyDataCount() > 0;
        }
    }
}