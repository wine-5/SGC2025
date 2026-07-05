using UnityEngine;
using System.Collections.Generic;
using Tyotyo.Core;
using Tyotyo.Manager;

namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 敵の生成を管理するコンポーネント
    /// 指定した間隔で敵をFactoryから生成し、自動管理コンポーネントを追加する
    /// Waveごとにボス数の制限を管理する
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
        private List<GameObject> activeBosses = new List<GameObject>();

        private void Start()
        {
            positionManager.Initialize();

            if (autoStart)
                StartSpawning();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<WaveChangedEvent>(OnWaveChanged);
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WaveChangedEvent>(OnWaveChanged);
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void OnWaveChanged(WaveChangedEvent e)
        {
            SetWaveLevel(e.WaveLevel);
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            // ボスが倒されたときにリストをクリーンアップ
            if (e.IsBoss)
                CleanupDeadBosses();
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
        /// 敵を1体生成（ボス数上限をチェック）
        /// </summary>
        private void SpawnEnemy()
        {
            if (EnemyFactory.I == null) return;

            Vector3 spawnPosition = positionManager.GetRandomSpawnPosition();
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            if (enemy == null) return;

            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null || controller.EnemyData == null) return;

            // ボス上限チェック
            if (controller.EnemyData.IsBoss && !CanSpawnBoss())
            {
                EnemyFactory.I.ReturnEnemy(enemy);
                return;
            }

            // ボスの場合はリストに追加（ReturnToPool時にEventBusで自動削除）
            if (controller.EnemyData.IsBoss)
                activeBosses.Add(enemy);

            // 移動タイプに応じて戦略を設定
            var strategy = MovementStrategyFactory.CreateStrategy(controller.EnemyData.MovementType);
            if (strategy != null)
                controller.SetMovementStrategy(strategy);
            else
                controller.SetTargetPosition(positionManager.GetOppositeEdgePosition(spawnPosition));
        }

        /// <summary>
        /// ボスを生成できるかチェック（Wave設定の上限を確認）
        /// </summary>
        private bool CanSpawnBoss()
        {
            CleanupDeadBosses();

            if (WaveManager.I == null)
                return true;

            var currentWave = WaveManager.I.CurrentWave;
            if (currentWave == null)
                return true;

            return activeBosses.Count < currentWave.maxBossCount;
        }

        /// <summary>
        /// 破棄されたボスをリストから除去
        /// </summary>
        private void CleanupDeadBosses()
        {
            activeBosses.RemoveAll(boss => boss == null || !boss.activeInHierarchy);
        }
    }
}
