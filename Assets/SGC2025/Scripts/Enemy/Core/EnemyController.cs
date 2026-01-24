using UnityEngine;
using SGC2025.Events;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵オブジェクトの状態管理とライフサイクル制御
    /// EnemyDataSOからの設定取得とウェーブレベルスケーリングをサポート
    /// 弾システムとの相互作用を含む包括的な敵制御機能
    /// </summary>
    public class EnemyController : MonoBehaviour, IDamageable, IEnemyParameters, IMovable
    {
        private const int MIN_WAVE_LEVEL = 1;
        private const float MIN_HEALTH = 0f;

        [Header("設定データ")]
        [Tooltip("敵の基本データ設定")]
        [SerializeField] private EnemyDataSO enemyData;

        private float currentHp;
        
        [Tooltip("現在のウェーブレベル（実行時のみ変更）")]
        [ReadOnly]
        [SerializeField] private int currentWaveLevel = MIN_WAVE_LEVEL;
        
        private EnemyParameters cachedParameters;
        private bool isInitialized = false;

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

        public void Initialize(EnemyDataSO data, int waveLevel = MIN_WAVE_LEVEL)
        {
            if (data == null)
            {
                currentWaveLevel = Mathf.Max(MIN_WAVE_LEVEL, waveLevel);
                isInitialized = true;
                return;
            }
            
            enemyData = data;
            currentWaveLevel = Mathf.Max(MIN_WAVE_LEVEL, waveLevel);
            cachedParameters = data.GetScaledParameters(currentWaveLevel);
            currentHp = cachedParameters.health;
            isInitialized = true;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= MIN_HEALTH) return;
            float actualDamage = Mathf.Min(damage, currentHp);
            currentHp = Mathf.Max(MIN_HEALTH, currentHp - actualDamage);

            OnDamageTaken?.Invoke(actualDamage);
            EnemyEvents.TriggerEnemyDamage(gameObject, actualDamage, currentHp, MaxHealth);
            if (!IsAlive)
                HandleDeath();
        }

        private void HandleDeath()
        {   
            OnDeath?.Invoke();
            int score = SGC2025.ScoreManager.I.EnemyKillPoint;
            EnemyEvents.TriggerEnemyDestroyed(transform.position, score);
            DeactivateEnemy();
        }

        private void DeactivateEnemy()
        {
            if (EnemyFactory.I != null)
                EnemyFactory.I.ReturnEnemy(gameObject);
        }
    }
}