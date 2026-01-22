using UnityEngine;
using UnityEngine.Tilemaps;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成位置を計算するクラス
    /// 四方向からのランダム生成位置を提供
    /// </summary>
    [System.Serializable]
    public class EnemySpawnPositionManager : ISpawnPositionProvider
    {
        [Header("四隅生成設定")]
        [SerializeField] private bool useCornerSpawn = false; // 四隅から生成するかどうか
        [SerializeField] private float cornerOffset = 1f;    // 四隅からの少しのオフセット
        
        [Header("境界線生成設定")]
        [SerializeField] private bool useBoundarySpawn = true; // 境界線全体から生成するかどうか
        [SerializeField] private float boundaryOffset = 1f;   // 境界線からのオフセット
        
        [Header("Canvas自動取得設定")]
        [SerializeField] private bool useCanvasReference = false; // Canvasから四隅を自動計算するか
        [SerializeField] private Canvas targetCanvas;             // 対象のCanvas
        [SerializeField] private Camera referenceCamera;         // ワールド座標変換用のカメラ
        
        [Header("TileMap自動取得設定")]
        [SerializeField] private bool useTileMapReference = false; // TileMapから四隅を自動計算するか
        [SerializeField] private UnityEngine.Tilemaps.Tilemap targetTileMap; // 対象のTileMap
        [SerializeField] private float tileMapPadding = 0.5f;      // TileMap境界からのパディング
        
        [Header("直接座標指定設定")]
        [SerializeField] private bool useDirectCoordinates = true; // 直接座標指定を使用するか
        [SerializeField] private Vector2 gameAreaMin = new Vector2(0, 0);    // ゲームエリアの最小座標
        // gameAreaMaxはGroundManagerから自動取得するため削除
        [SerializeField] private float cornerRandomRange = 3f; // 各角周辺のランダム範囲
        
        /// <summary>ゲームエリアの最大座標（GroundManagerから取得）</summary>
        private Vector2 gameAreaMax => GetGameAreaMaxFromGroundManager();
        
        private bool isInitialized = false;

        /// <summary>
        /// GroundManagerからゲームエリアの最大座標を取得
        /// </summary>
        private Vector2 GetGameAreaMaxFromGroundManager()
        {
            if (SGC2025.GroundManager.I != null)
            {
                var maxIndex = SGC2025.GroundManager.I.MapMaxIndex;
                return new Vector2(maxIndex.x, maxIndex.y);
            }
            
            // GroundManagerが初期化されていない場合のフォールバック
            Debug.LogWarning("[SpawnPositionManager] GroundManagerが初期化されていません。デフォルト値(59, 44)を使用します。");
            return new Vector2(59, 44);
        }
        
        #region ISpawnPositionProviderの実装

        /// <summary>初期化済みかどうか</summary>
        public bool IsInitialized => isInitialized;

        /// <summary>初期化処理</summary>
        public void Initialize()
        {
            isInitialized = true;
        }

        /// <summary>ランダムなスポーン位置を取得</summary>
        public Vector3 GetRandomSpawnPosition()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return GetRandomEdgeSpawnPosition();
        }

        #endregion

        /// <summary>Canvas参照モード取得</summary>
        public bool IsCanvasReferenceMode() => useCanvasReference;
        
        /// <summary>
        /// TileMap参照モードを設定
        /// </summary>
        public void SetTileMapReferenceMode(bool enabled, Tilemap tileMap = null, float padding = 0.5f)
        {
            useTileMapReference = enabled;
            if (tileMap != null) targetTileMap = tileMap;
            tileMapPadding = padding;
            if (enabled)
                useCanvasReference = false;
        }
        
        /// <summary>TileMap参照モード取得</summary>
        public bool IsTileMapReferenceMode() => useTileMapReference;
        
        /// <summary>
        /// 画面の四方向または四隅からランダムに生成位置を取得
        /// </summary>
        public Vector3 GetRandomEdgeSpawnPosition()
        {
            if (useBoundarySpawn)
                return GetRandomBoundaryPosition();
            if (useCornerSpawn)
                return GetRandomCornerSpawnPosition();
                
            // デフォルトは境界線生成
            return GetRandomBoundaryPosition();
        }
        
        /// <summary>
        /// 四隅からランダムに生成位置を取得
        /// </summary>
        public Vector3 GetRandomCornerSpawnPosition()
        {
            int corner = Random.Range(0, 4);
            return corner switch
            {
                0 => GetTopLeftCornerPosition(),
                1 => GetTopRightCornerPosition(),
                2 => GetBottomLeftCornerPosition(),
                3 => GetBottomRightCornerPosition(),
                _ => GetTopLeftCornerPosition()
            };
        }
        
        
        /// <summary>
        /// Canvasの四隅を自動計算して取得
        /// </summary>
        /// <returns>四隅の位置配列 [左上, 右上, 左下, 右下]</returns>
        private Vector3[] GetCanvasCorners()
        {
            if (targetCanvas == null)
            {
                Debug.LogError("[SpawnPositionManager] targetCanvasが設定されていません");
                return new Vector3[4];
            }
            
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            if (canvasRect == null)
            {
                Debug.LogError("[SpawnPositionManager] CanvasにRectTransformが見つかりません");
                return new Vector3[4];
            }
            
            // Canvas RectTransformの四隅をワールド座標で取得
            Vector3[] worldCorners = new Vector3[4];
            canvasRect.GetWorldCorners(worldCorners);
            
            // GetWorldCornersの順序: [左下, 左上, 右上, 右下]
            // 戻り値の順序: [左上, 右上, 左下, 右下]
            return new Vector3[]
            {
                worldCorners[1], // 左上
                worldCorners[2], // 右上
                worldCorners[0], // 左下
                worldCorners[3]  // 右下
            };
        }
        
        /// <summary>
        /// 直接座標指定から四隅の座標を取得
        /// </summary>
        /// <returns>四隅の位置配列 [左上, 右上, 左下, 右下]</returns>
        private Vector3[] GetDirectCoordinateCorners()
        {
            // 四隅の位置を計算 [左上, 右上, 左下, 右下]
            return new Vector3[]
            {
                new Vector3(gameAreaMin.x + cornerOffset, gameAreaMax.y - cornerOffset, 0f), // 左上
                new Vector3(gameAreaMax.x - cornerOffset, gameAreaMax.y - cornerOffset, 0f), // 右上
                new Vector3(gameAreaMin.x + cornerOffset, gameAreaMin.y + cornerOffset, 0f), // 左下
                new Vector3(gameAreaMax.x - cornerOffset, gameAreaMin.y + cornerOffset, 0f)  // 右下
            };
        }

        /// <summary>
        /// 指定した角の周辺からランダム位置を取得
        /// </summary>
        /// <param name="cornerIndex">角のインデックス (0:左上, 1:右上, 2:左下, 3:右下)</param>
        /// <returns>ランダム生成位置</returns>
        private Vector3 GetRandomPositionAroundCorner(int cornerIndex)
        {
            Vector3[] corners = GetDirectCoordinateCorners();
            Vector3 baseCorner = corners[cornerIndex];
            
            // 各角の制約に基づいてランダム範囲を調整
            float randomX, randomY;
            
            switch (cornerIndex)
            {
                case 0: // 左上
                    randomX = Random.Range(baseCorner.x, Mathf.Min(baseCorner.x + cornerRandomRange, gameAreaMax.x));
                    randomY = Random.Range(Mathf.Max(baseCorner.y - cornerRandomRange, gameAreaMin.y), baseCorner.y);
                    break;
                case 1: // 右上
                    randomX = Random.Range(Mathf.Max(baseCorner.x - cornerRandomRange, gameAreaMin.x), baseCorner.x);
                    randomY = Random.Range(Mathf.Max(baseCorner.y - cornerRandomRange, gameAreaMin.y), baseCorner.y);
                    break;
                case 2: // 左下
                    randomX = Random.Range(baseCorner.x, Mathf.Min(baseCorner.x + cornerRandomRange, gameAreaMax.x));
                    randomY = Random.Range(baseCorner.y, Mathf.Min(baseCorner.y + cornerRandomRange, gameAreaMax.y));
                    break;
                case 3: // 右下
                    randomX = Random.Range(Mathf.Max(baseCorner.x - cornerRandomRange, gameAreaMin.x), baseCorner.x);
                    randomY = Random.Range(baseCorner.y, Mathf.Min(baseCorner.y + cornerRandomRange, gameAreaMax.y));
                    break;
                default:
                    randomX = baseCorner.x;
                    randomY = baseCorner.y;
                    break;
            }
            
            return new Vector3(randomX, randomY, 0f);
        }

        /// <summary>
        /// 境界線全体からランダム位置を取得
        /// </summary>
        /// <returns>境界線上のランダム生成位置</returns>
        private Vector3 GetRandomBoundaryPosition()
        {
            int side = Random.Range(0, 4);
            return side switch
            {
                0 => GetRandomTopBoundaryPosition(),
                1 => GetRandomBottomBoundaryPosition(),
                2 => GetRandomLeftBoundaryPosition(),
                3 => GetRandomRightBoundaryPosition(),
                _ => GetRandomTopBoundaryPosition()
            };
        }

        /// <summary>
        /// 上辺からランダム位置を取得
        /// </summary>
        private Vector3 GetRandomTopBoundaryPosition()
        {
            float randomX = Random.Range(gameAreaMin.x + boundaryOffset, gameAreaMax.x - boundaryOffset);
            float y = gameAreaMax.y - boundaryOffset;
            return new Vector3(randomX, y, 0f);
        }

        /// <summary>
        /// 下辺からランダム位置を取得
        /// </summary>
        private Vector3 GetRandomBottomBoundaryPosition()
        {
            float randomX = Random.Range(gameAreaMin.x + boundaryOffset, gameAreaMax.x - boundaryOffset);
            float y = gameAreaMin.y + boundaryOffset;
            return new Vector3(randomX, y, 0f);
        }

        /// <summary>
        /// 左辺からランダム位置を取得
        /// </summary>
        private Vector3 GetRandomLeftBoundaryPosition()
        {
            float x = gameAreaMin.x + boundaryOffset;
            float randomY = Random.Range(gameAreaMin.y + boundaryOffset, gameAreaMax.y - boundaryOffset);
            return new Vector3(x, randomY, 0f);
        }

        /// <summary>
        /// 右辺からランダム位置を取得
        /// </summary>
        private Vector3 GetRandomRightBoundaryPosition()
        {
            float x = gameAreaMax.x - boundaryOffset;
            float randomY = Random.Range(gameAreaMin.y + boundaryOffset, gameAreaMax.y - boundaryOffset);
            return new Vector3(x, randomY, 0f);
        }

        /// <summary>
        /// TileMapの四隅を自動計算して取得
        /// </summary>
        /// <returns>四隅の位置配列 [左上, 右上, 左下, 右下]</returns>
        private Vector3[] GetTileMapCorners()
        {
            if (targetTileMap == null)
            {
                Debug.LogError("[SpawnPositionManager] targetTileMapが設定されていません");
                return new Vector3[4];
            }
            
            // TileMapの境界を取得
            BoundsInt bounds = targetTileMap.cellBounds;
            
            if (bounds.size.x == 0 || bounds.size.y == 0)
            {
                Debug.LogWarning("[SpawnPositionManager] TileMapにタイルが配置されていません");
                return new Vector3[4];
            }
            
            // セル座標をワールド座標に変換
            Vector3 minCellWorld = targetTileMap.CellToWorld(bounds.min);
            Vector3 maxCellWorld = targetTileMap.CellToWorld(bounds.max);
            
            // パディングを適用
            float padding = tileMapPadding;
            
            // 四隅の位置を計算 [左上, 右上, 左下, 右下]
            return new Vector3[]
            {
                new Vector3(minCellWorld.x - padding, maxCellWorld.y + padding, minCellWorld.z), // 左上
                new Vector3(maxCellWorld.x + padding, maxCellWorld.y + padding, minCellWorld.z), // 右上
                new Vector3(minCellWorld.x - padding, minCellWorld.y - padding, minCellWorld.z), // 左下
                new Vector3(maxCellWorld.x + padding, minCellWorld.y - padding, minCellWorld.z)  // 右下
            };
        }
        
        /// <summary>
        /// 左上隅の位置を取得
        /// </summary>
        private Vector3 GetTopLeftCornerPosition()
        {
            if (useDirectCoordinates)
            {
                Vector3 spawnPosition = GetRandomPositionAroundCorner(0); // 左上角周辺
                Debug.Log($"[SpawnPositionManager] 左上角生成: {spawnPosition} (範囲: {cornerRandomRange})");
                return spawnPosition;
            }
            else if (useTileMapReference)
            {
                Vector3[] corners = GetTileMapCorners();
                Vector3 corner = corners[0]; // 左上
                Vector3 spawnPosition = new Vector3(corner.x + cornerOffset, corner.y - cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 左上角生成位置(TileMap): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            else if (useCanvasReference)
            {
                Vector3[] corners = GetCanvasCorners();
                Vector3 corner = corners[0]; // 左上
                Vector3 spawnPosition = new Vector3(corner.x + cornerOffset, corner.y - cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 左上角生成位置(Canvas): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            
            Debug.LogError("[SpawnPositionManager] 左上隅の生成モードが設定されていません");
            return Vector3.zero;
        }
        
        /// <summary>
        /// 右上隅の位置を取得
        /// </summary>
        private Vector3 GetTopRightCornerPosition()
        {
            if (useDirectCoordinates)
            {
                Vector3 spawnPosition = GetRandomPositionAroundCorner(1); // 右上角周辺
                Debug.Log($"[SpawnPositionManager] 右上角生成: {spawnPosition} (範囲: {cornerRandomRange})");
                return spawnPosition;
            }
            else if (useTileMapReference)
            {
                Vector3[] corners = GetTileMapCorners();
                Vector3 corner = corners[1]; // 右上
                Vector3 spawnPosition = new Vector3(corner.x - cornerOffset, corner.y - cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 右上角生成位置(TileMap): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            else if (useCanvasReference)
            {
                Vector3[] corners = GetCanvasCorners();
                Vector3 corner = corners[1]; // 右上
                Vector3 spawnPosition = new Vector3(corner.x - cornerOffset, corner.y - cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 右上角生成位置(Canvas): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            
            Debug.LogError("[SpawnPositionManager] 右上隅の生成モードが設定されていません");
            return Vector3.zero;
        }
        
        /// <summary>
        /// 左下隅の位置を取得
        /// </summary>
        private Vector3 GetBottomLeftCornerPosition()
        {
            if (useDirectCoordinates)
            {
                Vector3 spawnPosition = GetRandomPositionAroundCorner(2); // 左下角周辺
                Debug.Log($"[SpawnPositionManager] 左下角生成: {spawnPosition} (範囲: {cornerRandomRange})");
                return spawnPosition;
            }
            else if (useTileMapReference)
            {
                Vector3[] corners = GetTileMapCorners();
                Vector3 corner = corners[2]; // 左下
                Vector3 spawnPosition = new Vector3(corner.x + cornerOffset, corner.y + cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 左下角生成位置(TileMap): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            else if (useCanvasReference)
            {
                Vector3[] corners = GetCanvasCorners();
                Vector3 corner = corners[2]; // 左下
                Vector3 spawnPosition = new Vector3(corner.x + cornerOffset, corner.y + cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 左下角生成位置(Canvas): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            
            Debug.LogError("[SpawnPositionManager] 左下隅の生成モードが設定されていません");
            return Vector3.zero;
        }
        
        /// <summary>
        /// 右下隅の位置を取得
        /// </summary>
        private Vector3 GetBottomRightCornerPosition()
        {
            if (useDirectCoordinates)
            {
                Vector3 spawnPosition = GetRandomPositionAroundCorner(3); // 右下角周辺
                Debug.Log($"[SpawnPositionManager] 右下角生成: {spawnPosition} (範囲: {cornerRandomRange})");
                return spawnPosition;
            }
            else if (useTileMapReference)
            {
                Vector3[] corners = GetTileMapCorners();
                Vector3 corner = corners[3]; // 右下
                Vector3 spawnPosition = new Vector3(corner.x - cornerOffset, corner.y + cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 右下角生成位置(TileMap): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            else if (useCanvasReference)
            {
                Vector3[] corners = GetCanvasCorners();
                Vector3 corner = corners[3]; // 右下
                Vector3 spawnPosition = new Vector3(corner.x - cornerOffset, corner.y + cornerOffset, 0f);
                Debug.Log($"[SpawnPositionManager] 右下角生成位置(Canvas): {spawnPosition} (基準角: {corner}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
            
            Debug.LogError("[SpawnPositionManager] 右下隅の生成モードが設定されていません");
            return Vector3.zero;
        }
        
        /// <summary>
        /// 逆側のエッジ位置を取得
        /// スポーン位置が端に近い場合、反対側の端の位置を返す
        /// </summary>
        /// <param name="spawnPos">スポーン位置</param>
        /// <returns>逆側のエッジ位置</returns>
        public Vector3 GetOppositeEdgePosition(Vector3 spawnPos)
        {
            float threshold = 2f; // 端判定の閾値
            
            // 上端判定
            if (Mathf.Abs(spawnPos.y - gameAreaMax.y) < threshold)
                return new Vector3(spawnPos.x, gameAreaMin.y, 0f);
            
            // 下端判定
            if (Mathf.Abs(spawnPos.y - gameAreaMin.y) < threshold)
                return new Vector3(spawnPos.x, gameAreaMax.y, 0f);
            
            // 左端判定
            if (Mathf.Abs(spawnPos.x - gameAreaMin.x) < threshold)
                return new Vector3(gameAreaMax.x, spawnPos.y, 0f);
            
            // 右端判定
            if (Mathf.Abs(spawnPos.x - gameAreaMax.x) < threshold)
                return new Vector3(gameAreaMin.x, spawnPos.y, 0f);
            
            // 最も近い端に基づいて反対側を返す
            return GetFallbackOppositePosition(spawnPos);
        }
        
        /// <summary>
        /// フォールバック用の反対側位置計算
        /// </summary>
        private Vector3 GetFallbackOppositePosition(Vector3 spawnPos)
        {
            float distToTop = Mathf.Abs(spawnPos.y - gameAreaMax.y);
            float distToBottom = Mathf.Abs(spawnPos.y - gameAreaMin.y);
            float distToLeft = Mathf.Abs(spawnPos.x - gameAreaMin.x);
            float distToRight = Mathf.Abs(spawnPos.x - gameAreaMax.x);
            
            float minDistance = Mathf.Min(distToTop, distToBottom, distToLeft, distToRight);
            
            if (minDistance == distToTop)
                return new Vector3(spawnPos.x, gameAreaMin.y, 0f);
            if (minDistance == distToBottom)
                return new Vector3(spawnPos.x, gameAreaMax.y, 0f);
            if (minDistance == distToLeft)
                return new Vector3(gameAreaMax.x, spawnPos.y, 0f);
            
            return new Vector3(gameAreaMin.x, spawnPos.y, 0f);
        }

        /// <summary>
        /// 境界線生成モードが有効かどうか
        /// </summary>
        /// <returns>境界線生成モードの状態</returns>
        public bool IsBoundarySpawnMode()
        {
            return useBoundarySpawn;
        }
    }
}