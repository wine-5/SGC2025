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

        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("敵選択設定")]
        [SerializeField] private EnemySpawnConfigManager spawnConfigManager = new EnemySpawnConfigManager();
        
        // 各敵タイプのオリジナルスケールを保存
        private Dictionary<EnemyType, Vector3> originalScales = new Dictionary<EnemyType, Vector3>();
        
        // 敵生成統計用カウンター
        private Dictionary<EnemyType, int> enemySpawnCounts = new Dictionary<EnemyType, int>();
        private int totalSpawnCount = 0;
        private Queue<EnemyType> recentSpawns = new Queue<EnemyType>();
        
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
            
            // スケールの現在値をログ
            Vector3 currentScale = enemyObj.transform.localScale;
            
            // オリジナルスケールを保存（初回のみ）
            if (!originalScales.ContainsKey(enemyData.EnemyType))
            {
                // プレファブのオリジナルスケールを保存（実際のプレファブのスケールを使用）
                originalScales[enemyData.EnemyType] = currentScale;
                Debug.Log($"[EnemyFactory] {enemyData.EnemyType} のオリジナルスケール保存: {originalScales[enemyData.EnemyType]}");
            }
            
            Debug.Log($"[EnemyFactory] {enemyData.EnemyType} 生成 - Poolから取得時のスケール: {currentScale}");
            
            // Waveレベルに応じてオリジナルスケールをスケーリング
            float scaleMultiplier = 1f + (0.05f * (waveLevel - 1));
            Vector3 correctScale = originalScales[enemyData.EnemyType] * scaleMultiplier;
            enemyObj.transform.localScale = correctScale;
            Debug.Log($"[EnemyFactory] {enemyData.EnemyType} スケール設定: Wave{waveLevel} (x{scaleMultiplier:F2}) → {correctScale}");
            
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = Quaternion.identity;
            
            var controller = enemyObj.GetComponent<EnemyController>();
            if (controller != null)
            {
                enemyData.InitializeController(controller, waveLevel);
                
                // 初期化後のスケールをログ
                Vector3 afterInitScale = enemyObj.transform.localScale;
                Debug.Log($"[EnemyFactory] {enemyData.EnemyType} 初期化後のスケール: {afterInitScale}");
                
                if (currentScale != afterInitScale)
                {
                    Debug.LogWarning($"[EnemyFactory] スケールが変更されました: {currentScale} -> {afterInitScale}");
                }
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
            
            Debug.Log($"[EnemyFactory] ランダム敵生成開始 - Wave Level: {waveLevel}, Position: {position}");
            
            var selectedEnemy = spawnConfigManager.SelectRandomEnemyData();
            if (selectedEnemy == null)
            {
                Debug.LogWarning("[EnemyFactory] 選択可能な敵がいません");
                return null;
            }
            
            Debug.Log($"[EnemyFactory] 最終選択された敵: {selectedEnemy.EnemyType}");
            
            // 統計情報を更新してログ
            UpdateAndLogSpawnStatistics(selectedEnemy.EnemyType);
            
            return CreateEnemy(selectedEnemy, position, waveLevel);
        }
        
        /// <summary>
        /// 敵生成統計情報を更新してログ出力
        /// </summary>
        private void UpdateAndLogSpawnStatistics(EnemyType enemyType)
        {
            totalSpawnCount++;
            
            // 最近のスポーンを記録（最大10件まで）
            recentSpawns.Enqueue(enemyType);
            if (recentSpawns.Count > 10)
                recentSpawns.Dequeue();
            
            if (!enemySpawnCounts.ContainsKey(enemyType))
                enemySpawnCounts[enemyType] = 0;
            enemySpawnCounts[enemyType]++;
            
            // 5回毎に統計情報を表示（偏りを早期発見のため）
            if (totalSpawnCount % 5 == 0)
            {
                Debug.Log($"===== 敵生成統計 (総数: {totalSpawnCount}) =====");
                foreach (var kvp in enemySpawnCounts.OrderByDescending(x => x.Value))
                {
                    float percentage = (float)kvp.Value / totalSpawnCount * 100f;
                    Debug.Log($"  {kvp.Key}: {kvp.Value}回 ({percentage:F1}%)");
                }
                
                // 最新の体の統計も表示
                Debug.Log($"--- 直近{recentSpawns.Count}体の分析 ---");
                AnalyzeRecentSpawns();
                Debug.Log($"==========================");
            }
        }
        
        /// <summary>
        /// 最近のスポーンパターンを分析
        /// </summary>
        private void AnalyzeRecentSpawns()
        {
            var recentCounts = new Dictionary<EnemyType, int>();
            foreach (var spawn in recentSpawns)
            {
                if (!recentCounts.ContainsKey(spawn))
                    recentCounts[spawn] = 0;
                recentCounts[spawn]++;
            }
            
            foreach (var kvp in recentCounts.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / recentSpawns.Count * 100f;
                Debug.Log($"    直近{recentSpawns.Count}体: {kvp.Key}: {kvp.Value}回 ({percentage:F1}%)");
            }
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
