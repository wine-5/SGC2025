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
        
        [Header("基本パラメーター")]
        [SerializeField] private float health = DEFAULT_HEALTH;
        [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
        
        [Header("生存時間設定")]
        [SerializeField] private float lifeTime = DEFAULT_LIFE_TIME;

        [Header("ボス設定")]
        [SerializeField] private bool isBoss = false;

        [Header("緑化設定")]
        [SerializeField, Tooltip("撃破時に緑化する一辺のマス数（通常=1, ボス=3 など）")]
        private int greeningSize = 1;
        [SerializeField, Tooltip("緑化範囲上昇アイテム中に緑化する一辺のマス数（通常=3, ボス=6 など）")]
        private int greeningSizeBoosted = 3;

        // 基本プロパティ
        public EnemyType EnemyType => enemyType;
        public MovementType MovementType => movementType;
        public bool IsBoss => isBoss;
        public int GreeningSize => greeningSize;
        public int GreeningSizeBoosted => greeningSizeBoosted;
        
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
