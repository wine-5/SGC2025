using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TechC;
using SGC2025.Enemy;

namespace SGC2025
{
    /// <summary>
    /// 敵の生成・プール管理を行うファクトリークラス
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        private const int DEFAULT_WAVE_LEVEL = 1;
        private const float SCALE_INCREMENT_PER_WAVE = 0.05f;

        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("敵選択設定")]
        [SerializeField] private EnemySpawnConfigManager spawnConfigManager = new EnemySpawnConfigManager();
        
        // 各敵タイプのオリジナルスケールを保存
        private Dictionary<EnemyType, Vector3> originalScales = new Dictionary<EnemyType, Vector3>();
        
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
            
            if (!spawnConfigManager.HasValidConfigs)
                Debug.LogError("EnemyFactory: EnemySpawnConfigManagerに有効な設定がありません");
        }
        
        /// <summary>
        /// 敵を生成（EnemyDataSOから）
        /// </summary>
        public GameObject CreateEnemy(EnemyDataSO enemyData, Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (enemyData == null) return null;
            
            string poolName = enemyData.EnemyType.ToString();
            GameObject enemyObj = objectPool.GetObjectByName(poolName);
            
            if (enemyObj == null) return null;
            
            // オリジナルスケールを保存（初回のみ）
            if (!originalScales.ContainsKey(enemyData.EnemyType))
            {
                originalScales[enemyData.EnemyType] = enemyObj.transform.localScale;
            }
            
            // Waveレベルに応じてオリジナルスケールをスケーリング
            float scaleMultiplier = 1f + (SCALE_INCREMENT_PER_WAVE * (waveLevel - 1));
            Vector3 correctScale = originalScales[enemyData.EnemyType] * scaleMultiplier;
            enemyObj.transform.localScale = correctScale;
            
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = Quaternion.identity;
            
            var controller = enemyObj.GetComponent<EnemyController>();
            if (controller != null)
            {
                enemyData.InitializeController(controller, waveLevel);
                

            }
            else
                Debug.LogError($"[EnemyFactory] {enemyData.EnemyType}にEnemyControllerが見つかりません");
            
            return enemyObj;
        }
        
        /// <summary>
        /// 敵を生成（EnemyTypeから）
        /// </summary>
        public GameObject CreateEnemy(EnemyType enemyType, Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (!spawnConfigManager.HasValidConfigs)
            {
                Debug.LogError("[EnemyFactory] EnemySpawnConfigManagerに有効な設定がありません");
                return null;
            }
            
            var enemyData = spawnConfigManager.GetEnemyData(enemyType);
            if (enemyData == null)
            {
                Debug.LogError($"[EnemyFactory] EnemyType {enemyType} のデータが見つかりません");
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
                Debug.LogError("[EnemyFactory] EnemySpawnConfigManagerに有効な設定がありません");
                return null;
            }
            

            var selectedEnemy = spawnConfigManager.SelectRandomEnemyData();
            if (selectedEnemy == null)
            {
                Debug.LogWarning("[EnemyFactory] 選択可能な敵がいません");
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
