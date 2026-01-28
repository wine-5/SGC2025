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
        
        [Header("移動設定")]
        [SerializeField] private MovementType movementType = MovementType.FixedDirection;
        
        private const float DEFAULT_HEALTH = 100f;
        private const float DEFAULT_MOVE_SPEED = 3f;
        private const float DEFAULT_LIFE_TIME = 30f;
        private const int DEFAULT_WAVE_LEVEL = 1;
        
        [Header("基本パラメーター")]
        [SerializeField] private float health = DEFAULT_HEALTH;
        [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
        
        [Header("生存時間設定")]
        [SerializeField] private float lifeTime = DEFAULT_LIFE_TIME;
        
        [Header("スケール設定")]
        [SerializeField] private Vector3 baseScale = Vector3.one;
        [SerializeField] private float scaleGrowthRate = 0.05f; // Waveごとのスケール上昇率
        
        
        // 基本プロパティ
        public EnemyType EnemyType => enemyType;
        public MovementType MovementType => movementType;
        public float LifeTime => lifeTime;
        public Vector3 BaseScale => baseScale;
        
        /// <summary>
        /// Waveレベルに応じてスケーリングされたパラメーターを取得
        /// </summary>
        public EnemyParameters GetScaledParameters(int waveLevel)
        {
            // Waveレベルに応じてパラメーターをスケーリング
            float waveMultiplier = 1f + (0.1f * (waveLevel - 1)); // 10%ずつ上昇
            return new EnemyParameters
            {
                enemyType = this.enemyType,
                health = this.health * waveMultiplier,
                moveSpeed = this.moveSpeed * Mathf.Min(waveMultiplier, 2f), // 移動速度は最大2倍まで
                lifeTime = this.lifeTime
            };
        }
        
        /// <summary>
        /// Waveレベルに応じたスケールを取得
        /// </summary>
        public Vector3 GetScale(int waveLevel = DEFAULT_WAVE_LEVEL)
        {
            float scaleMultiplier = 1f + (scaleGrowthRate * (waveLevel - 1));
            return baseScale * scaleMultiplier;
        }
        
        /// <summary>
        /// EnemyControllerを初期化
        /// </summary>
        public void InitializeController(EnemyController controller, int waveLevel = DEFAULT_WAVE_LEVEL)
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
        public float lifeTime;
    }
}
