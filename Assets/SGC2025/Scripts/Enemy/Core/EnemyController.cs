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
        #region フィールド

        [Header("設定データ")]
        [Tooltip("敵の基本データ設定")]
        [SerializeField] private EnemyDataSO enemyData;

        private float currentHp;
        
        [Tooltip("現在のウェーブレベル（実行時のみ変更）")]
        [ReadOnly]
        [SerializeField] private int currentWaveLevel = 1;
        
        // キャッシュされたパラメーター
        private EnemyParameters cachedParameters;
        
        // 状態フラグ
        private bool isInitialized = false;

        #endregion

        #region IDamageableインターフェース要求イベント

        /// <summary>この敵がダメージを受けた際に発火されるイベント（IDamageable要求）</summary>
        public event System.Action<float> OnDamageTaken;
        
        /// <summary>この敵が死亡した際に発火されるイベント（IDamageable要求）</summary>
        public event System.Action OnDeath;

        #endregion

        #region プロパティ - 状態取得

        /// <summary>使用中の敵データ</summary>
        public EnemyDataSO EnemyData => enemyData;
        
        /// <summary>現在のヘルス値</summary>
        public float CurrentHealth => currentHp;
        
        /// <summary>生存状態</summary>
        public bool IsAlive => currentHp > 0f && isInitialized;
        
        /// <summary>現在のウェーブレベル</summary>
        public int CurrentWaveLevel => currentWaveLevel;
        

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

        #region IMovableの実装

        /// <summary>オブジェクトのTransform</summary>
        public Transform Transform => transform;
        
        /// <summary>移動可能かどうか</summary>
        public bool CanMove => IsAlive && isInitialized;

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

                currentWaveLevel = Mathf.Max(1, waveLevel);
                isInitialized = true;
                return;
            }
            
            enemyData = data;
            currentWaveLevel = Mathf.Max(1, waveLevel);
            cachedParameters = data.GetScaledParameters(currentWaveLevel);
            currentHp = cachedParameters.health;  // SOから正しいHPを設定
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

            float actualDamage = Mathf.Min(damage, currentHp);
            currentHp = Mathf.Max(0f, currentHp - actualDamage);

            // インターフェース要求イベント発火
            OnDamageTaken?.Invoke(actualDamage);

            // 静的イベントクラス経由でダメージイベント発火
            EnemyEvents.TriggerEnemyDamage(gameObject, actualDamage, currentHp, MaxHealth);

            if (!IsAlive)
            {
                HandleDeath();
            }
        }

        #endregion

        #region プライベートメソッド

        private void HandleDeath()
        {
            // インターフェース要求イベント発火
            OnDeath?.Invoke();
            
            // 静的イベントクラス経由で撃破イベント発火（スコアは将来的に追加）
            EnemyEvents.TriggerEnemyDestroyed(transform.position, 0);
            
            DeactivateEnemy();
        }

        private void DeactivateEnemy()
        {
            // EnemyFactoryのプールに返却
            if (EnemyFactory.I != null)
            {
                EnemyFactory.I.ReturnEnemy(gameObject);
            }
        }

        #endregion
    }
}