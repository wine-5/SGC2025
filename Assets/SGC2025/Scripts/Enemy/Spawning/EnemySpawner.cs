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
        private const float MIN_SPAWN_INTERVAL = 0.1f;
        private const string DEBUG_LOG_PREFIX = "[EnemySpawner]";

        [Header("生成設定")]
        [SerializeField] private float spawnInterval = DEFAULT_SPAWN_INTERVAL;
        [SerializeField] private bool autoStart = true;

        [Header("生成位置管理")]
        [SerializeField] private EnemySpawnPositionManager positionManager = new EnemySpawnPositionManager();
        private ISpawnPositionProvider positionProvider; // インターフェース参照

        [Header("ウェーブ設定")]
        [SerializeField] private int currentWaveLevel = DEFAULT_WAVE_LEVEL;

        private bool isSpawning = false;
        private float nextSpawnTime = 0f;

        private void Start()
        {
            // インターフェース参照を設定
            positionProvider = positionManager;
            positionProvider.Initialize(); // インターフェース経由で初期化

            if (autoStart)
            {
                StartSpawning();
            }
        }

        /// <summary>敵の生成を開始</summary>
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
            spawnInterval = Mathf.Max(MIN_SPAWN_INTERVAL, interval);
        }

        /// <summary>
        /// Waveレベルを設定（WaveManager用）
        /// </summary>
        public void SetWaveLevel(int waveLevel)
        {
            currentWaveLevel = Mathf.Max(DEFAULT_WAVE_LEVEL, waveLevel);
        }

        /// <summary>現在のスポーン間隔を取得</summary>
        public float GetSpawnInterval() => spawnInterval;

        /// <summary>現在のWaveレベルを取得</summary>
        public int GetWaveLevel() => currentWaveLevel;


        private void Update()
        {
            if (!isSpawning) return;

            // カウントダウン中はスポーンしない
            if (SGC2025.GameManager.I != null && SGC2025.GameManager.I.IsCountingDown)
                return;

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
            if (EnemyFactory.I == null) return;
            Vector3 spawnPosition = positionProvider.GetRandomSpawnPosition();
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            if (enemy == null) return;
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
