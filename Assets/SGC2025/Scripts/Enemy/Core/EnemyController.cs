using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の現在状態を管理するクラス
    /// 設定値はEnemyDataSOから参照する
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("設定データ")]
        [SerializeField] private EnemyDataSO enemyData;
        
        [Header("現在状態")]
        [SerializeField] private float currentHealth;
        [SerializeField] private int currentWaveLevel = 1;
        
        // 現在のパラメーター（キャッシュ用）
        private EnemyParameters currentParameters;
        
        // プロパティ（現在状態のみ）
        public EnemyDataSO EnemyData => enemyData;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        
        // 設定値は必要に応じてEnemyDataから取得
        public float MaxHealth => currentParameters.health;
        public float MoveSpeed => currentParameters.moveSpeed;
        public float AttackPower => currentParameters.attackPower;
        public EnemyType EnemyType => currentParameters.enemyType;
        public float LifeTime => currentParameters.lifeTime;
        
        /// <summary>
        /// EnemyDataSOとウェーブレベルを設定して初期化
        /// </summary>
        public void Initialize(EnemyDataSO data, int waveLevel = 1)
        {
            enemyData = data;
            currentWaveLevel = waveLevel;
            currentParameters = data.GetScaledParameters(waveLevel);
            currentHealth = currentParameters.health;
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (!IsAlive)
            {
                OnDeath();
            }
        }
        

        
        /// <summary>
        /// 死亡時の処理
        /// </summary>
        private void OnDeath()
        {
            // 死亡処理はEnemyDeathHandlerに委譲する予定
            gameObject.SetActive(false);
        }
        

    }
}
