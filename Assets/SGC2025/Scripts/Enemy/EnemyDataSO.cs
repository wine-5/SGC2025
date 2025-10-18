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
        
        private const float DEFAULT_HEALTH = 100f;
        private const float DEFAULT_MOVE_SPEED = 3f;
        private const float DEFAULT_ATTACK_POWER = 10f;
        
        [Header("基本パラメーター")]
        [SerializeField] private float health = DEFAULT_HEALTH;
        [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
        [SerializeField] private float attackPower = DEFAULT_ATTACK_POWER;
        
        
        // 基本プロパティ
        public EnemyType EnemyType => enemyType;
        
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
                attackPower = this.attackPower
            };
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
    }
}
