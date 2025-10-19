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

        [Header("生成設定")]
        [SerializeField] private float spawnInterval = DEFAULT_SPAWN_INTERVAL;
        [SerializeField] private bool autoStart = true;

        [Header("生成位置管理")]
        [SerializeField] private EnemySpawnPositionManager positionManager = new EnemySpawnPositionManager();

        [Header("ウェーブ設定")]
        [SerializeField] private int currentWaveLevel = DEFAULT_WAVE_LEVEL;

        private bool isSpawning = false;
        private float nextSpawnTime = 0f;

        private void Start()
        {
            positionManager.InitRangesFromTransforms(); // ← ここで呼び出し
            
            // スポーンポイントの設定を確認
            if (!positionManager.AreAllSpawnPointsSet())
            {
                Debug.LogError("EnemySpawner: スポーンポイントが正しく設定されていません！");
                positionManager.LogMissingSpawnPoints();
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
            spawnInterval = Mathf.Max(0.1f, interval);
        }
        
        /// <summary>
        /// Waveレベルを設定（WaveManager用）
        /// </summary>
        public void SetWaveLevel(int waveLevel)
        {
            currentWaveLevel = Mathf.Max(1, waveLevel);
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
            if (!isSpawning) return;

            // DeltaTimeベースのスポーン判定
            if (Time.time >= nextSpawnTime)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnInterval;
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
            
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            
            if (enemy == null)
            {
                Debug.LogError("EnemySpawner: 敵の生成に失敗しました！");
                return;
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