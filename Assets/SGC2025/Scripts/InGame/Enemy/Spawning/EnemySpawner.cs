using UnityEngine;
using SGC2025.Manager;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成を管理するコンポーネント
    /// 指定した間隔で敵をFactoryから生成し、自動管理コンポーネントを追加する
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private const int DEFAULT_WAVE_LEVEL = 1;
        private const float DEFAULT_SPAWN_INTERVAL = 2f;

        [Header("生成設定")]
        [SerializeField] private bool autoStart = true;

        [Header("生成位置管理")]
        [SerializeField] private EnemySpawnPositionManager positionManager = new EnemySpawnPositionManager();

        [Header("ウェーブ設定")]
        [SerializeField] private int currentWaveLevel = DEFAULT_WAVE_LEVEL;

        private bool isSpawning = false;
        private float nextSpawnTime = 0f;

        private void Start()
        {
            positionManager.Initialize();

            if (autoStart)
                StartSpawning();
        }

        /// <summary>敵の生成を開始</summary>
        private void StartSpawning()
        {
            if (isSpawning) return;
            isSpawning = true;
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
        }

        /// <summary>
        /// Waveレベルを設定（WaveManager用）
        /// </summary>
        public void SetWaveLevel(int waveLevel)
        {
            currentWaveLevel = Mathf.Max(DEFAULT_WAVE_LEVEL, waveLevel);
        }

        /// <summary>現在のスポーン間隔を取得（WaveManagerから）</summary>
        private float GetCurrentSpawnInterval()
        {
            if (WaveManager.I != null)
            {
                var currentWave = WaveManager.I.CurrentWave;
                return currentWave != null ? currentWave.spawnInterval : DEFAULT_SPAWN_INTERVAL;
            }
            return DEFAULT_SPAWN_INTERVAL;
        }

        private void Update()
        {
            if (!isSpawning) return;

            // カウントダウン中はスポーンしない
            if (InGameManager.I != null && InGameManager.I.IsCountingDown)
                return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + GetCurrentSpawnInterval();
            }
        }

        /// <summary>
        /// 敵を1体生成
        /// </summary>
        private void SpawnEnemy()
        {
            if (EnemyFactory.I == null) return;

            Vector3 spawnPosition = positionManager.GetRandomSpawnPosition();
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            if (enemy == null) return;

            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null || controller.EnemyData == null) return;

            // 移動タイプに応じて戦略を設定
            var strategy = MovementStrategyFactory.CreateStrategy(controller.EnemyData.MovementType);
            if (strategy != null)
                controller.SetMovementStrategy(strategy);
            else
                controller.SetTargetPosition(positionManager.GetOppositeEdgePosition(spawnPosition));
        }
    }
}
