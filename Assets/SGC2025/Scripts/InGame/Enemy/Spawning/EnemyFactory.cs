using System.Collections.Generic;
using UnityEngine;
using Tyotyo.InGame.Enemy;
using Tyotyo.Systems;
using Tyotyo.Core;

namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 敵の生成・プール管理を行うファクトリークラス
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        protected override bool UseDontDestroyOnLoad => false;

        private const int DEFAULT_WAVE_LEVEL = 1;
        private const float SCALE_INCREMENT_PER_WAVE = 0.05f;

        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("敵選択設定")]
        [SerializeField] private EnemySpawnConfigManager spawnConfigManager = new EnemySpawnConfigManager();

        // 各インスタンスのプレハブ元スケールを保存
        private readonly Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

        // EnemyType→プール名（enum.ToString()）をキャッシュし、スポーンごとの文字列確保を避ける
        private readonly Dictionary<EnemyType, string> poolNameCache = new Dictionary<EnemyType, string>();
        
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
        private GameObject CreateEnemy(EnemyDataSO enemyData, Vector3 position, int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            if (enemyData == null) return null;
            
            string poolName = GetPoolName(enemyData.EnemyType);
            GameObject enemyObj = objectPool.GetObjectByName(poolName);
            
            if (enemyObj == null) return null;

            // インスタンスごとに元スケールを登録（初回のみ）
            if (!originalScales.ContainsKey(enemyObj))
                originalScales[enemyObj] = enemyObj.transform.localScale;

            // Waveレベルに応じて元スケールをスケーリング
            float scaleMultiplier = 1f + (SCALE_INCREMENT_PER_WAVE * (waveLevel - 1));
            enemyObj.transform.localScale = originalScales[enemyObj] * scaleMultiplier;
            
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = Quaternion.identity;
            
            var controller = enemyObj.GetComponent<EnemyController>();
            if (controller != null)
                controller.Initialize(enemyData, waveLevel);
            else
                Debug.LogError($"[EnemyFactory] {enemyData.EnemyType}にEnemyControllerが見つかりません");
            
            return enemyObj;
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

        /// <summary>EnemyTypeに対応するプール名を返す（初回のみToStringし、以降はキャッシュ）</summary>
        private string GetPoolName(EnemyType enemyType)
        {
            if (!poolNameCache.TryGetValue(enemyType, out var poolName))
            {
                poolName = enemyType.ToString();
                poolNameCache[enemyType] = poolName;
            }
            return poolName;
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
