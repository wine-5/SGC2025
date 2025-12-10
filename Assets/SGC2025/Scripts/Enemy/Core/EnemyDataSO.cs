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
        private const float DEFAULT_ATTACK_POWER = 10f;
        private const float DEFAULT_LIFE_TIME = 30f;
        private const int DEFAULT_WAVE_LEVEL = 1;
        
        [Header("基本パラメーター")]
        [SerializeField] private float health = DEFAULT_HEALTH;
        [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
        [SerializeField] private float attackPower = DEFAULT_ATTACK_POWER;
        
        [Header("生存時間設定")]
        [SerializeField] private float lifeTime = DEFAULT_LIFE_TIME;
        
        
        // 基本プロパティ
        public EnemyType EnemyType => enemyType;
        public MovementType MovementType => movementType;
        public float LifeTime => lifeTime;
        
        /// <summary>
        /// ウェーブレベルに応じてスケーリングされたパラメーターを取得
        /// </summary>
        public EnemyParameters GetScaledParameters(int waveLevel)
        {
            return new EnemyParameters
            {
                enemyType = this.enemyType,
                health = this.health,
                moveSpeed = this.moveSpeed,
                attackPower = this.attackPower,
                lifeTime = this.lifeTime
            };
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
        public float attackPower;
        public float lifeTime;
    }
}
