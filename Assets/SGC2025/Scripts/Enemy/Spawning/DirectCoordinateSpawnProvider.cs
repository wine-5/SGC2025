using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 直接座標指定によるスポーン位置提供
    /// </summary>
    [System.Serializable]
    public class DirectCoordinateSpawnProvider : ISpawnPositionProvider
    {
        [Header("直接座標指定設定")]
        [SerializeField] private Vector2 gameAreaMin = new Vector2(0, 0);
        [SerializeField] private Vector2 gameAreaMax = new Vector2(149, 149);
        [SerializeField] private float boundaryOffset = 1f;
        [SerializeField] private bool useCornerSpawn = false;
        [SerializeField] private float cornerRandomRange = 3f;
        
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        /// <summary>
        /// ランダムなスポーン位置を取得
        /// </summary>
        public Vector3 GetRandomSpawnPosition()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            
            if (useCornerSpawn)
            {
                return GetCornerSpawnPosition();
            }
            
            return GetBoundarySpawnPosition();
        }
        
        /// <summary>
        /// 四隅からのスポーン位置を取得
        /// </summary>
        private Vector3 GetCornerSpawnPosition()
        {
            int corner = Random.Range(0, 4);
            Vector2 basePosition;
            
            switch (corner)
            {
                case 0: // 左上
                    basePosition = new Vector2(gameAreaMin.x, gameAreaMax.y);
                    break;
                case 1: // 右上
                    basePosition = new Vector2(gameAreaMax.x, gameAreaMax.y);
                    break;
                case 2: // 左下
                    basePosition = new Vector2(gameAreaMin.x, gameAreaMin.y);
                    break;
                default: // 右下
                    basePosition = new Vector2(gameAreaMax.x, gameAreaMin.y);
                    break;
            }
            
            // ランダムオフセットを追加
            Vector2 randomOffset = new Vector2(
                Random.Range(-cornerRandomRange, cornerRandomRange),
                Random.Range(-cornerRandomRange, cornerRandomRange)
            );
            
            return basePosition + randomOffset;
        }
        
        /// <summary>
        /// 境界線からのスポーン位置を取得
        /// </summary>
        private Vector3 GetBoundarySpawnPosition()
        {
            int side = Random.Range(0, 4);
            
            switch (side)
            {
                case 0: // 上
                    return new Vector3(
                        Random.Range(gameAreaMin.x, gameAreaMax.x),
                        gameAreaMax.y + boundaryOffset,
                        0f
                    );
                case 1: // 下
                    return new Vector3(
                        Random.Range(gameAreaMin.x, gameAreaMax.x),
                        gameAreaMin.y - boundaryOffset,
                        0f
                    );
                case 2: // 左
                    return new Vector3(
                        gameAreaMin.x - boundaryOffset,
                        Random.Range(gameAreaMin.y, gameAreaMax.y),
                        0f
                    );
                default: // 右
                    return new Vector3(
                        gameAreaMax.x + boundaryOffset,
                        Random.Range(gameAreaMin.y, gameAreaMax.y),
                        0f
                    );
            }
        }
    }
}