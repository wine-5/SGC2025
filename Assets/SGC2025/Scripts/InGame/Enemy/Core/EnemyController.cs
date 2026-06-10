using UnityEngine;
using SGC2025.Core;
using SGC2025.Manager;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵オブジェクトの唯一の窓口
    /// 状態管理・移動・ライフタイム管理を統括する
    /// </summary>
    public class EnemyController : MonoBehaviour, IDamageable
    {
        private const int MIN_WAVE_LEVEL = 1;
        private const float MIN_HEALTH = 0f;
        private const float OUT_OF_BOUNDS_MARGIN = 5f;

        [Header("設定データ")]
        [Tooltip("敵の基本データ設定")]
        [SerializeField] private EnemyDataSO enemyData;

        [Tooltip("現在のウェーブレベル（実行時のみ変更）")]
        [SerializeField] private int currentWaveLevel = MIN_WAVE_LEVEL;

        private float currentHp;
        private EnemyParameters cachedParameters;
        private bool isInitialized = false;
        private EnemyMovement movement;
        private float elapsedTime;

        public event System.Action<float> OnDamageTaken;
        public event System.Action OnDeath;

        public EnemyDataSO EnemyData => enemyData;
        public float CurrentHealth => currentHp;
        public bool IsAlive => currentHp > MIN_HEALTH && isInitialized;
        public int CurrentWaveLevel => currentWaveLevel;
        public float MaxHealth => cachedParameters.health;
        public float MoveSpeed => cachedParameters.moveSpeed;
        public EnemyType EnemyType => cachedParameters.enemyType;
        public float LifeTime => cachedParameters.lifeTime;
        public Transform Transform => transform;
        public bool CanMove => IsAlive && isInitialized;

        private void Update()
        {
            if (!isInitialized) return;

            movement.Tick(Time.deltaTime);
            elapsedTime += Time.deltaTime;

            if (ShouldReturn())
                ReturnToPool();
        }

        private void OnEnable()
        {
            // プールから再取得されたとき経過時間をリセット
            if (isInitialized)
                elapsedTime = 0f;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(EnemyDataSO data, int waveLevel = MIN_WAVE_LEVEL)
        {
            if (data == null)
            {
                Debug.LogError("[EnemyController] EnemyDataSOがnullです");
                return;
            }

            enemyData = data;
            currentWaveLevel = Mathf.Max(MIN_WAVE_LEVEL, waveLevel);
            cachedParameters = data.GetScaledParameters(currentWaveLevel);
            currentHp = cachedParameters.health;
            movement = new EnemyMovement(transform, this);
            elapsedTime = 0f;
            isInitialized = true;
        }

        /// <summary>
        /// 移動戦略を設定（追従型）
        /// </summary>
        public void SetMovementStrategy(IMovementStrategy strategy) => movement.SetMovementStrategy(strategy);

        /// <summary>
        /// 目標位置を設定（固定方向移動型）
        /// </summary>
        public void SetTargetPosition(Vector3 target) => movement.SetTargetPosition(target);

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= MIN_HEALTH) return;

            float actualDamage = Mathf.Min(damage, currentHp);
            currentHp = Mathf.Max(MIN_HEALTH, currentHp - actualDamage);

            OnDamageTaken?.Invoke(actualDamage);
            EventBus.Publish(new EnemyDamageTakenEvent(gameObject, actualDamage, currentHp, MaxHealth));

            if (!IsAlive)
                HandleDeath();
        }

        /// <summary>
        /// プールに返却する（ライフタイム切れ・範囲外・目標到達時に呼ばれる）
        /// </summary>
        public void ReturnToPool()
        {
            if (!isInitialized) return;

            isInitialized = false;
            if (EnemyFactory.I != null)
                EnemyFactory.I.ReturnEnemy(gameObject);
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke();
            EventBus.Publish(new EnemyDestroyedEvent(transform.position));
            ReturnToPool();
        }

        #region ライフタイム・範囲外チェック

        private bool ShouldReturn() => HasLifeTimeExpired() || IsOutOfMapBounds();

        private bool HasLifeTimeExpired() => elapsedTime >= LifeTime;

        private bool IsOutOfMapBounds()
        {
            if (GroundManager.I == null || GroundManager.I.MapData == null) return false;

            var mapData = GroundManager.I.MapData;
            Vector3 pos = transform.position;

            return pos.x < -OUT_OF_BOUNDS_MARGIN ||
                   pos.x > mapData.MapMaxWorldPosition.x + OUT_OF_BOUNDS_MARGIN ||
                   pos.y < -OUT_OF_BOUNDS_MARGIN ||
                   pos.y > mapData.MapMaxWorldPosition.y + OUT_OF_BOUNDS_MARGIN;
        }

        #endregion
    }
}