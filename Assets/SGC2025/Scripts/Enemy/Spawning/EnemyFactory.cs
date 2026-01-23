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
        private const int DEFAULT_WAVE_LEVEL = 1;
        private const string DEBUG_LOG_PREFIX = "[EnemyFactory]";

        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("敵選択設定")]
        [SerializeField] private EnemySpawnConfigManager spawnConfigManager = new EnemySpawnConfigManager();
        
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
            if (objectPool == null) return;
            
            // 設定状況を確認
            if (!spawnConfigManager.HasValidConfigs)
            {
                Debug.LogError("EnemyFactory: EnemySpawnConfigManagerに有効な設定がありません");
            }
        }
        
        /// <summary>
        /// 敵を生成（EnemyDataSOから）
        /// </summary>
        public GameObject CreateEnemy(EnemyDataSO enemyData, Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (enemyData == null) return null;
            
            // EnemyTypeの名前でプールから取得
            string poolName = enemyData.EnemyType.ToString();
            GameObject enemyObj = objectPool.GetObjectByName(poolName);
            
            if (enemyObj == null) return null;
            
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
                Debug.LogError($"{DEBUG_LOG_PREFIX} {enemyData.EnemyType}にEnemyControllerが見つかりません");
            }
            
            return enemyObj;
        }
        
        /// <summary>
        /// 敵を生成（EnemyTypeから）
        /// </summary>
        public GameObject CreateEnemy(EnemyType enemyType, Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (!spawnConfigManager.HasValidConfigs)
            {
                Debug.LogError($"{DEBUG_LOG_PREFIX} EnemySpawnConfigManagerに有効な設定がありません");
                return null;
            }
            
            var enemyData = spawnConfigManager.GetEnemyData(enemyType);
            if (enemyData == null)
            {
                Debug.LogError($"{DEBUG_LOG_PREFIX} EnemyType {enemyType} のデータが見つかりません");
                return null;
            }
            
            return CreateEnemy(enemyData, position, waveLevel);
        }
        
        /// <summary>
        /// ランダムな敵を生成（重み付きランダム）
        /// </summary>
        public GameObject CreateRandomEnemy(Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (!spawnConfigManager.HasValidConfigs)
            {
                Debug.LogError($"{DEBUG_LOG_PREFIX} EnemySpawnConfigManagerに有効な設定がありません");
                return null;
            }
            
            var selectedEnemy = spawnConfigManager.SelectRandomEnemyData(waveLevel);
            if (selectedEnemy == null)
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} ウェーブレベル {waveLevel} で選択可能な敵がいません");
                return null;
            }
            
            return CreateEnemy(selectedEnemy, position, waveLevel);
        }
        
        /// <summary>
        /// 敵をプールに返却
        /// </summary>
        public void ReturnEnemy(GameObject enemy)
        {
            if (enemy == null) return;
            
            objectPool.ReturnObject(enemy);
        }
        

    }
}
