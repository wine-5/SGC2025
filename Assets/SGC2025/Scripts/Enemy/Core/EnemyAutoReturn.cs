using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動削除を管理するコンポーネント
    /// 一定時間後、または画面外に出たときにプールに返却する
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
        private const float DEFAULT_LIFE_TIME = 30f;
        private const float CHASER_LIFE_TIME = 20f;  // プレイヤー追従型の生存時間
        
        [Header("自動削除設定")]
        [SerializeField] private float lifeTime = DEFAULT_LIFE_TIME;
        
        private float spawnTime;
        private bool isInitialized = false;
        
        /// <summary>
        /// 初期化（生成時に呼ばれる）
        /// </summary>
        public void Initialize()
        {
            spawnTime = Time.time;
            isInitialized = true;
            
            // 敵の種類に応じて生存時間を調整
            SetLifeTimeBasedOnEnemyType();
        }
        
        /// <summary>
        /// 敵の種類に応じて生存時間を設定
        /// </summary>
        private void SetLifeTimeBasedOnEnemyType()
        {
            var controller = GetComponent<EnemyController>();
            if (controller != null && controller.EnemyData != null)
            {
                MovementType movementType = controller.EnemyData.MovementType;
                
                // プレイヤー追従型の敵は短い生存時間
                if (IsPlayerChaserType(movementType))
                {
                    lifeTime = CHASER_LIFE_TIME;
                }
                else
                {
                    lifeTime = DEFAULT_LIFE_TIME;
                }
            }
        }
        
        /// <summary>
        /// プレイヤー追従型の敵かどうかを判定
        /// </summary>
        private bool IsPlayerChaserType(MovementType movementType)
        {
            return movementType == MovementType.LinearChaser ||
                   movementType == MovementType.InertiaChaser ||
                   movementType == MovementType.PredictiveChaser ||
                   movementType == MovementType.ArcChaser;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            if (ShouldReturnToPool())
            {
                ReturnToPool();
            }
        }
        
        /// <summary>
        /// プールに返却すべきかチェック
        /// </summary>
        private bool ShouldReturnToPool()
        {
            // 時間経過のみで判定（境界チェックは削除）
            return HasLifeTimeExpired();
        }
        
        /// <summary>
        /// 生存時間が経過したかチェック
        /// </summary>
        private bool HasLifeTimeExpired()
        {
            return Time.time - spawnTime >= lifeTime;
        }

        /// <summary>
        /// プールに返却
        /// </summary>
        private void ReturnToPool()
        {
            if (SGC2025.EnemyFactory.I != null)
            {
                EnemyFactory.I.ReturnEnemy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// プールから取得されたときの初期化
        /// </summary>
        private void OnEnable()
        {
            if (isInitialized)
            {
                Initialize();
            }
        }
    }
}