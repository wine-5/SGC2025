using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成設定を管理するScriptableObject
    /// 重み付けによる敵選択と生成確率を設定
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
            public float spawnWeight = 10f;
            
            [Header("出現条件")]
            public int minWaveLevel = 1;
            public int maxWaveLevel = 999;
            
            /// <summary>
            /// 指定ウェーブで出現可能かチェック
            /// </summary>
            public bool CanSpawnAtWave(int waveLevel)
            {
                return waveLevel >= minWaveLevel && waveLevel <= maxWaveLevel;
            }
        }
        
        [Header("敵生成データ")]
        [SerializeField] private List<EnemySpawnData> enemySpawnDataList = new List<EnemySpawnData>();
        
        /// <summary>
        /// 指定されたウェーブレベルで出現可能な敵のリストを取得
        /// </summary>
        /// <param name="waveLevel">ウェーブレベル</param>
        /// <returns>出現可能な敵のリスト</returns>
        public List<EnemySpawnData> GetAvailableEnemiesForWave(int waveLevel)
        {
            return enemySpawnDataList.Where(data => 
                data != null && 
                data.enemyData != null && 
                data.CanSpawnAtWave(waveLevel)
            ).ToList();
        }
        
        /// <summary>
        /// 重み付きランダムで敵を選択
        /// </summary>
        /// <param name="waveLevel">ウェーブレベル</param>
        /// <returns>選択された敵データ</returns>
        public EnemyDataSO SelectRandomEnemyByWeight(int waveLevel)
        {
            var availableEnemies = GetAvailableEnemiesForWave(waveLevel);
            if (availableEnemies.Count == 0)
            {
                Debug.LogWarning($"EnemySpawnConfigSO: ウェーブレベル {waveLevel} で出現可能な敵がいません");
                return null;
            }
            
            return SelectFromList(availableEnemies);
        }
        
        /// <summary>
        /// リストから重み付きランダムで選択
        /// </summary>
        /// <param name="availableEnemies">選択可能な敵のリスト</param>
        /// <returns>選択された敵データ</returns>
        private EnemyDataSO SelectFromList(List<EnemySpawnData> availableEnemies)
        {
            float totalWeight = CalculateTotalWeight(availableEnemies);
            float randomValue = Random.Range(0f, totalWeight);
            
            return FindEnemyByWeight(availableEnemies, randomValue);
        }
        
        /// <summary>
        /// 全体の重みを計算
        /// </summary>
        /// <param name="enemies">敵のリスト</param>
        /// <returns>合計重み</returns>
        private float CalculateTotalWeight(List<EnemySpawnData> enemies)
        {
            return enemies.Sum(enemy => enemy.spawnWeight);
        }
        
        /// <summary>
        /// 重みに基づいて敵を検索
        /// </summary>
        /// <param name="enemies">敵のリスト</param>
        /// <param name="randomValue">ランダム値</param>
        /// <returns>選択された敵データ</returns>
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
        /// <param name="enemyType">敵タイプ</param>
        /// <returns>該当する敵データ</returns>
        public EnemyDataSO GetEnemyData(EnemyType enemyType)
        {
            var spawnData = enemySpawnDataList.FirstOrDefault(data => 
                data != null && 
                data.enemyData != null && 
                data.enemyData.EnemyType == enemyType
            );
            return spawnData?.enemyData;
        }
        
        /// <summary>
        /// 登録されている敵データの数を取得
        /// </summary>
        /// <returns>敵データ数</returns>
        public int GetEnemyDataCount()
        {
            return enemySpawnDataList.Count(data => data != null && data.enemyData != null);
        }
    }
}