using UnityEngine;
using TechC;
using SGC2025.Enemy;
using System.Collections.Generic;
using System.Linq;

namespace SGC2025
{
    /// <summary>
    /// 敵の生成・プール管理を行うファクトリークラス
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private List<EnemyDataSO> enemyDataList = new List<EnemyDataSO>();
        
        [Header("初期プール設定")]
        [SerializeField] private int initialPoolSize = 10;
        
        protected override void Init()
        {
            base.Init();
            InitializeEnemyPools();
        }
        
        /// <summary>
        /// 敵のプールを初期化
        /// 注意: ObjectPoolのInspectorで事前にプレファブを設定しておく必要があります
        /// </summary>
        private void InitializeEnemyPools()
        {
            if (objectPool == null)
            {
                Debug.LogError("EnemyFactory: ObjectPoolが設定されていません");
                return;
            }
            
            // ObjectPoolの設定は事前にInspectorで行う
            Debug.Log("EnemyFactory: ObjectPoolの初期化完了（プレファブはObjectPool側で設定済み）");
        }
        
        /// <summary>
        /// 敵を生成（EnemyDataSOから）
        /// </summary>
        public GameObject CreateEnemy(EnemyDataSO enemyData, Vector3 position, int waveLevel = 1)
        {
            if (enemyData == null)
            {
                Debug.LogError("EnemyFactory: 無効なEnemyDataSOです");
                return null;
            }
            
            // EnemyTypeの名前でプールから取得
            string poolName = enemyData.EnemyType.ToString();
            GameObject enemyObj = objectPool.GetObjectByName(poolName);
            
            if (enemyObj == null)
            {
                Debug.LogError($"EnemyFactory: {enemyData.EnemyType} の生成に失敗しました（プール名: {poolName}）");
                return null;
            }
            
            // 位置を設定
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = Quaternion.identity;
            
            // EnemyControllerを初期化
            var controller = enemyObj.GetComponent<EnemyController>();
            if (controller != null)
            {
                enemyData.InitializeController(controller, waveLevel);
            }
            else
            {
                Debug.LogError($"EnemyFactory: {enemyData.EnemyType}にEnemyControllerが見つかりません");
            }
            
            return enemyObj;
        }
        
        /// <summary>
        /// 敵を生成（EnemyTypeから）
        /// </summary>
        public GameObject CreateEnemy(EnemyType enemyType, Vector3 position, int waveLevel = 1)
        {
            var enemyData = GetEnemyData(enemyType);
            if (enemyData == null)
            {
                Debug.LogError($"EnemyFactory: EnemyType {enemyType} のデータが見つかりません");
                return null;
            }
            
            return CreateEnemy(enemyData, position, waveLevel);
        }
        
        /// <summary>
        /// ランダムな敵を生成（重み付きランダム）
        /// </summary>
        public GameObject CreateRandomEnemy(Vector3 position, int waveLevel = 1)
        {
            var availableEnemies = GetAvailableEnemiesForWave(waveLevel);
            if (availableEnemies.Count == 0)
            {
                Debug.LogWarning($"EnemyFactory: ウェーブレベル {waveLevel} で出現可能な敵がいません");
                return null;
            }
            
            var selectedEnemy = SelectRandomEnemyByWeight(availableEnemies);
            return CreateEnemy(selectedEnemy, position, waveLevel);
        }
        
        /// <summary>
        /// 敵をプールに返却
        /// </summary>
        public void ReturnEnemy(GameObject enemy)
        {
            if (enemy == null) return;
            
            // 敵の状態をリセット
            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                // 必要に応じて状態リセット処理を追加
            }
            
            objectPool.ReturnObject(enemy);
        }
        
        /// <summary>
        /// 指定されたウェーブレベルで出現可能な敵のリストを取得
        /// </summary>
        private List<EnemyDataSO> GetAvailableEnemiesForWave(int waveLevel)
        {
            return enemyDataList.Where(data => data != null && data.CanSpawnAtWave(waveLevel)).ToList();
        }
        
        /// <summary>
        /// 重み付きランダムで敵を選択
        /// </summary>
        private EnemyDataSO SelectRandomEnemyByWeight(List<EnemyDataSO> availableEnemies)
        {
            float totalWeight = availableEnemies.Sum(enemy => enemy.SpawnWeight);
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var enemy in availableEnemies)
            {
                currentWeight += enemy.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemy;
                }
            }
            
            // フォールバック：最初の敵を返す
            return availableEnemies[0];
        }
        
        /// <summary>
        /// EnemyTypeからEnemyDataSOを取得
        /// </summary>
        private EnemyDataSO GetEnemyData(EnemyType enemyType)
        {
            return enemyDataList.FirstOrDefault(data => data != null && data.EnemyType == enemyType);
        }
    }
}
