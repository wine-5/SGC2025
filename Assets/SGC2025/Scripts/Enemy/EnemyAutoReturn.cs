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
        private const float DEFAULT_RETURN_BOUNDARY = 15f;
        
        [Header("自動削除設定")]
        [SerializeField] private float lifeTime = DEFAULT_LIFE_TIME;
        [SerializeField] private float returnBoundaryX = DEFAULT_RETURN_BOUNDARY;
        [SerializeField] private float returnBoundaryY = DEFAULT_RETURN_BOUNDARY;
        
        private float spawnTime;
        private bool isInitialized = false;
        
        /// <summary>
        /// 初期化（生成時に呼ばれる）
        /// </summary>
        public void Initialize()
        {
            spawnTime = Time.time;
            isInitialized = true;
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
            return HasLifeTimeExpired() || IsOutOfBounds();
        }
        
        /// <summary>
        /// 生存時間が経過したかチェック
        /// </summary>
        private bool HasLifeTimeExpired()
        {
            return Time.time - spawnTime >= lifeTime;
        }
        
        /// <summary>
        /// 境界外にいるかチェック（四方向対応）
        /// </summary>
        private bool IsOutOfBounds()
        {
            Vector3 pos = transform.position;
            
            // 画面の四方向の境界をチェック
            return pos.y <= -returnBoundaryY ||    // 下
                   pos.y >= returnBoundaryY ||     // 上  
                   pos.x <= -returnBoundaryX ||    // 左
                   pos.x >= returnBoundaryX;       // 右
        }
        
        /// <summary>
        /// プールに返却
        /// </summary>
        private void ReturnToPool()
        {
            if (SGC2025.EnemyFactory.I != null)
            {
                SGC2025.EnemyFactory.I.ReturnEnemy(gameObject);
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