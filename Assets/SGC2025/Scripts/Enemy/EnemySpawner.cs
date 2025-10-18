using UnityEngine;
using System.Collections;
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

        [Header("敵生成設定")]
        [SerializeField] private EnemySpawnConfigSO spawnConfig;

        [Header("生成位置管理")]
        [SerializeField] private EnemySpawnPositionManager positionManager = new EnemySpawnPositionManager();

        [Header("ウェーブ設定")]
        [SerializeField] private int currentWaveLevel = DEFAULT_WAVE_LEVEL;

        private bool isSpawning = false;
        private Coroutine spawnCoroutine;

        private void Start()
        {
            positionManager.InitRangesFromTransforms(); // ← ここで呼び出し
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
            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }

        /// <summary>
        /// 敵の生成を停止
        /// </summary>
        public void StopSpawning()
        {
            if (!isSpawning) return;

            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }



        /// <summary>
        /// 敵生成のコルーチン
        /// </summary>
        private IEnumerator SpawnCoroutine()
        {
            while (isSpawning)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// 敵を1体生成
        /// </summary>
        private void SpawnEnemy()
        {
            if (EnemyFactory.I == null)
            {
                return;
            }

            Vector3 spawnPosition = positionManager.GetRandomEdgeSpawnPosition();
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);

            if (enemy != null)
            {
                // 移動コンポーネントを取得/追加
                var movement = enemy.GetComponent<EnemyMovement>();
                if (movement == null)
                {
                    movement = enemy.AddComponent<EnemyMovement>();
                }

                // 敵の種類を取得
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null && controller.EnemyData != null)
                {
                    EnemyType enemyType = controller.EnemyData.EnemyType;
                    
                    // 敵の種類に応じて移動戦略を設定
                    var strategy = MovementStrategyFactory.CreateStrategy(enemyType);
                    if (strategy != null)
                    {
                        // プレイヤー追従型
                        movement.SetMovementStrategy(strategy);
                    }
                    else
                    {
                        // 従来の固定位置移動型
                        Vector3 targetPosition = positionManager.GetOppositeEdgePosition(spawnPosition);
                        movement.SetTargetPosition(targetPosition);
                    }
                }

                // 敵に自動削除コンポーネントを追加
                var autoReturn = enemy.GetComponent<EnemyAutoReturn>();
                if (autoReturn == null)
                {
                    autoReturn = enemy.AddComponent<EnemyAutoReturn>();
                }
                autoReturn.Initialize();
            }
        }

        /// <summary>
        /// 生成位置の反対側の座標を計算（中心対称）
        /// </summary>
        private Vector3 GetOppositePosition(Vector3 spawnPos)
        {
            // 画面の中心を原点(0,0,0)と仮定
            return -spawnPos;
        }


    }
}