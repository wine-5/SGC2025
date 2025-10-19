using UnityEngine;
using SGC2025.Enemy;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成を管理するコンポーネント
    /// 指定した間隔で敵をFactoryから生成し、自動管理コンポーネントを追加する
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private const float DEFAULT_SPAWN_INTERVAL = 2f;
        private const int DEFAULT_WAVE_LEVEL = 1;

                [Header("簡易Wave設定")]
        [SerializeField] private bool useSimpleWaveSystem = true;
        [SerializeField] private float waveUpgradeInterval = 5f; // 5秒ごとにWaveレベルアップ（テスト用）
        [SerializeField] private int maxWaveLevel = 10;
        [SerializeField] private float spawnInterval = DEFAULT_SPAWN_INTERVAL;
        [SerializeField] private bool autoStart = true;

        [Header("生成位置管理")]
        [SerializeField] private EnemySpawnPositionManager positionManager = new EnemySpawnPositionManager();

        [Header("デバッグ設定")]
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private int currentWaveLevel = DEFAULT_WAVE_LEVEL;

        private bool isSpawning = false;
        private float nextSpawnTime = 0f;
        private float gameStartTime = 0f;

        private void Start()
        {
            positionManager.InitRangesFromTransforms(); // ← ここで呼び出し
            
            // スポーンポイントの設定を確認
            if (!positionManager.AreAllSpawnPointsSet())
            {
                Debug.LogError("EnemySpawner: スポーンポイントが正しく設定されていません！");
                positionManager.LogMissingSpawnPoints();
            }
            
            // ゲーム開始時間を記録
            gameStartTime = Time.time;
            
            if (enableDebugLog)
            {
                string waveSystemType = useSimpleWaveSystem ? "簡易Wave制御" : "WaveManager制御";
                Debug.Log($"[EnemySpawner] 初期化完了 - {waveSystemType}, Waveレベル: {currentWaveLevel}, スポーン間隔: {spawnInterval}秒");
            }
            
            if (autoStart)
            {
                StartSpawning();
            }
        }

        /// <summary>
        /// 敵の生成を開始
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning) return;

            isSpawning = true;
            nextSpawnTime = Time.time + spawnInterval;
        }

        /// <summary>
        /// 敵の生成を停止
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }
        
        /// <summary>
        /// スポーン間隔を設定（WaveManager用）
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            float oldInterval = spawnInterval;
            spawnInterval = Mathf.Max(0.1f, interval);
            
            if (enableDebugLog)
            {
                Debug.Log($"[EnemySpawner] スポーン間隔変更: {oldInterval:F1}秒 → {spawnInterval:F1}秒");
            }
        }
        
        /// <summary>
        /// Waveレベルを設定（WaveManager用）
        /// </summary>
        public void SetWaveLevel(int waveLevel)
        {
            int oldLevel = currentWaveLevel;
            currentWaveLevel = Mathf.Max(1, waveLevel);
            
            if (enableDebugLog)
            {
                Debug.Log($"[EnemySpawner] Waveレベル変更: {oldLevel} → {currentWaveLevel}");
            }
        }
        
        /// <summary>
        /// 現在のスポーン間隔を取得
        /// </summary>
        public float GetSpawnInterval()
        {
            return spawnInterval;
        }
        
        /// <summary>
        /// 現在のWaveレベルを取得
        /// </summary>
        public int GetWaveLevel()
        {
            return currentWaveLevel;
        }

        private void Update()
        {
            // 簡易Wave制御システム
            if (useSimpleWaveSystem)
            {
                UpdateSimpleWaveSystem();
            }
            
            if (!isSpawning) return;

            // DeltaTimeベースのスポーン判定
            if (Time.time >= nextSpawnTime)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
        
        /// <summary>
        /// 簡易Wave制御システムの更新
        /// </summary>
        private void UpdateSimpleWaveSystem()
        {
            float gameElapsedTime = Time.time - gameStartTime;
            int calculatedWaveLevel = Mathf.FloorToInt(gameElapsedTime / waveUpgradeInterval) + 1;
            calculatedWaveLevel = Mathf.Min(calculatedWaveLevel, maxWaveLevel);
            
            if (calculatedWaveLevel != currentWaveLevel)
            {
                int previousWave = currentWaveLevel;
                currentWaveLevel = calculatedWaveLevel;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[EnemySpawner] 簡易Wave更新: {previousWave} → {currentWaveLevel} (ゲーム時間: {gameElapsedTime:F1}秒)");
                    Debug.Log($"[EnemySpawner] Wave更新間隔: {waveUpgradeInterval}秒, 最大Wave: {maxWaveLevel}");
                }
                
                // スポーン間隔を調整（オプション）
                AdjustSpawnIntervalForWave();
            }
            
            // 定期的にWave状態をログ出力（1秒間隔）
            if (enableDebugLog && Time.time % 1f < Time.deltaTime)
            {
                Debug.Log($"[EnemySpawner] 現在のWave: {currentWaveLevel}, 経過時間: {gameElapsedTime:F1}秒");
            }
        }
        
        /// <summary>
        /// Waveレベルに応じてスポーン間隔を調整
        /// </summary>
        private void AdjustSpawnIntervalForWave()
        {
            // 例：Waveが上がるにつれてスポーン間隔を短くする
            float baseInterval = 2f;
            float minInterval = 0.5f;
            float newInterval = Mathf.Max(minInterval, baseInterval - (currentWaveLevel - 1) * 0.1f);
            
            if (Mathf.Abs(newInterval - spawnInterval) > 0.01f)
            {
                float oldInterval = spawnInterval;
                spawnInterval = newInterval;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[EnemySpawner] スポーン間隔調整: {oldInterval:F1}秒 → {spawnInterval:F1}秒");
                }
            }
        }

        /// <summary>
        /// 敵を1体生成
        /// </summary>
        private void SpawnEnemy()
        {
            if (EnemyFactory.I == null)
            {
                Debug.LogError("EnemySpawner: EnemyFactory.I がnullです！");  
                return;
            }

            Vector3 spawnPosition = positionManager.GetRandomEdgeSpawnPosition();
            
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("EnemySpawner: スポーン位置が中心(0,0,0)になっています。スポーンポイントの設定を確認してください。");
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"[EnemySpawner] 敵生成試行 - Waveレベル: {currentWaveLevel}, 位置: {spawnPosition}");
            }
            
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            
            if (enemy == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[EnemySpawner] 敵の生成に失敗！Waveレベル {currentWaveLevel} で利用可能な敵がない可能性があります");
                }
                return;
            }

            if (enableDebugLog)
            {
                var controller = enemy.GetComponent<EnemyController>();
                string enemyType = controller?.EnemyData?.EnemyType.ToString() ?? "Unknown";
                Debug.Log($"[EnemySpawner] 敵生成成功: {enemyType} (Wave {currentWaveLevel})");
            }

            if (enemy != null)
            {
                // 移動コンポーネントを取得
                var movement = enemy.GetComponent<EnemyMovement>();

                // 敵の種類を取得（手動アタッチ前提）
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null && controller.EnemyData != null && movement != null)
                {
                    MovementType movementType = controller.EnemyData.MovementType;
                    
                    // 移動タイプに応じて移動戦略を設定
                    var strategy = MovementStrategyFactory.CreateStrategy(movementType);
                    if (strategy != null)
                    {
                        // プレイヤー追従型
                        movement.SetMovementStrategy(strategy);
                    }
                    else
                    {
                        // 固定方向移動型
                        Vector3 targetPosition = positionManager.GetOppositeEdgePosition(spawnPosition);
                        movement.SetTargetPosition(targetPosition);
                    }
                }

                // 自動削除コンポーネントの初期化
                var autoReturn = enemy.GetComponent<EnemyAutoReturn>();
                if (autoReturn != null)
                {
                    autoReturn.Initialize();
                }
            }
        }
    }
}