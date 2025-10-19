using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵オブジェクトの状態管理とライフサイクル制御
    /// EnemyDataSOからの設定取得とウェーブレベルスケーリングをサポート
    /// 弾システムとの相互作用を含む包括的な敵制御機能
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        #region フィールド

        [Header("設定データ")]
        [Tooltip("敵の基本データ設定")]
        [SerializeField] private EnemyDataSO enemyData;
        
        [Header("現在状態")]
        [Tooltip("現在のヘルス値")]
        [SerializeField] private float currentHealth;
        
        [Tooltip("現在のウェーブレベル")]
        [SerializeField] private int currentWaveLevel = 1;
        
        // キャッシュされたパラメーター
        private EnemyParameters cachedParameters;
        
        // 状態フラグ
        private bool isInitialized = false;

        #endregion

        #region イベント

        /// <summary>敵が撃破された際に発火されるイベント</summary>
        public static event System.Action OnEnemyDestroyed;
        
        /// <summary>敵が撃破された際に位置情報と共に発火されるイベント</summary>
        public static event System.Action<Vector3> OnEnemyDestroyedAtPosition;
        
        /// <summary>この敵がダメージを受けた際に発火されるイベント</summary>
        public event System.Action<float> OnDamageTaken;
        
        /// <summary>この敵が死亡した際に発火されるイベント</summary>
        public event System.Action OnDeath;

        #endregion

        #region プロパティ - 状態取得

        /// <summary>使用中の敵データ</summary>
        public EnemyDataSO EnemyData => enemyData;
        
        /// <summary>現在のヘルス値</summary>
        public float CurrentHealth => currentHealth;
        
        /// <summary>生存状態</summary>
        public bool IsAlive => currentHealth > 0f && isInitialized;
        
        /// <summary>現在のウェーブレベル</summary>
        public int CurrentWaveLevel => currentWaveLevel;
        
        /// <summary>初期化済みかどうか</summary>
        public bool IsInitialized => isInitialized;

        #endregion

        #region プロパティ - パラメーター取得

        /// <summary>最大ヘルス値（ウェーブレベル適用済み）</summary>
        public float MaxHealth => cachedParameters.health;
        
        /// <summary>移動速度（ウェーブレベル適用済み）</summary>
        public float MoveSpeed => cachedParameters.moveSpeed;
        
        /// <summary>攻撃力（ウェーブレベル適用済み）</summary>
        public float AttackPower => cachedParameters.attackPower;
        
        /// <summary>敵の種類</summary>
        public EnemyType EnemyType => cachedParameters.enemyType;
        
        /// <summary>生存時間（ウェーブレベル適用済み）</summary>
        public float LifeTime => cachedParameters.lifeTime;

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 敵を初期化
        /// </summary>
        /// <param name="data">敵データ</param>
        /// <param name="waveLevel">ウェーブレベル</param>
        public void Initialize(EnemyDataSO data, int waveLevel = 1)
        {
            if (data == null)
            {
                Debug.LogError("[EnemyController] 敵データがnullです");
                return;
            }
            
            enemyData = data;
            currentWaveLevel = Mathf.Max(1, waveLevel);
            cachedParameters = data.GetScaledParameters(currentWaveLevel);
            currentHealth = cachedParameters.health;
            isInitialized = true;
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f) 
            {
                return;
            }
            
            float actualDamage = Mathf.Min(damage, currentHealth);
            currentHealth = Mathf.Max(0f, currentHealth - actualDamage);
            
            OnDamageTaken?.Invoke(actualDamage);
            
            if (!IsAlive)
            {
                HandleDeath();
            }
        }

        /// <summary>
        /// 敵をリセット（オブジェクトプール用）
        /// </summary>
        public void ResetEnemy()
        {
            isInitialized = false;
            currentHealth = 0f;
            currentWaveLevel = 1;
            cachedParameters = default;
            gameObject.SetActive(false);
        }

        #endregion

        #region プライベートメソッド

        private void HandleDeath()
        {
            OnDeath?.Invoke();
            OnEnemyDestroyed?.Invoke();
            
            // 位置情報付きで敵撃破イベントを発火
            OnEnemyDestroyedAtPosition?.Invoke(transform.position);
            
            ProcessDeathEffects();
            DeactivateEnemy();
        }

        private void ProcessDeathEffects()
        {
            // 将来的にパーティクル、サウンド、スコア等の処理を追加
        }

        private void DeactivateEnemy()
        {
            // EnemyFactoryのプールに返却
            if (SGC2025.EnemyFactory.I != null)
            {
                SGC2025.EnemyFactory.I.ReturnEnemy(gameObject);
            }
            else
            {
                // フォールバック: 直接非アクティブ化
                gameObject.SetActive(false);
                Debug.LogWarning("[EnemyController] EnemyFactoryが見つからないため、直接非アクティブ化しました");
            }
        }

        #endregion
    }
}
