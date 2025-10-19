using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵のパラメーターと出現設定を管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "SGC2025/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("基本情報")]
        [SerializeField] private EnemyType enemyType;
        
        [Header("基本パラメーター")]
        [SerializeField] private float health = 100f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float attackPower = 10f;
        
        [Header("AI設定")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1.5f;
        
        [Header("出現設定")]
        [SerializeField, Range(0f, 100f)] private float spawnWeight = 10f;
        [SerializeField] private int minWaveLevel = 1;
        
        [Header("スケーリング設定")]
        [SerializeField] private float healthScaling = 1.2f;
        [SerializeField] private float speedScaling = 1.1f;
        [SerializeField] private float attackScaling = 1.15f;
        
        // 必要最小限のプロパティ
        public EnemyType EnemyType => enemyType;
        public float SpawnWeight => spawnWeight;
        
        /// <summary>
        /// ウェーブレベルに応じてスケーリングされたパラメーターを取得
        /// </summary>
        public EnemyParameters GetScaledParameters(int waveLevel)
        {
            return new EnemyParameters
            {
                enemyType = this.enemyType,
                health = this.health * Mathf.Pow(healthScaling, waveLevel - 1),
                moveSpeed = this.moveSpeed * Mathf.Pow(speedScaling, waveLevel - 1),
                attackPower = this.attackPower * Mathf.Pow(attackScaling, waveLevel - 1),
                detectionRange = this.detectionRange,
                attackRange = this.attackRange
            };
        }
        
        /// <summary>
        /// 指定されたウェーブレベルで出現可能かチェック
        /// </summary>
        public bool CanSpawnAtWave(int waveLevel)
        {
            return waveLevel >= minWaveLevel;
        }
        
        /// <summary>
        /// EnemyControllerを初期化
        /// </summary>
        public void InitializeController(EnemyController controller, int waveLevel = 1)
        {
            controller.Initialize(this, waveLevel);
        }
    }
    
    /// <summary>
    /// スケーリングされた敵のパラメーター
    /// </summary>
    [System.Serializable]
    public struct EnemyParameters
    {
        public EnemyType enemyType;
        public float health;
        public float moveSpeed;
        public float attackPower;
        public float detectionRange;
        public float attackRange;
    }
}
