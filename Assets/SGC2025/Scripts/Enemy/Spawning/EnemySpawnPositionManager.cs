using UnityEngine;
using UnityEngine.Tilemaps;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成位置を計算するクラス
    /// 四方向からのランダム生成位置を提供
    /// </summary>
    [System.Serializable]
    public class EnemySpawnPositionManager
    {
        private const float RANGE_DETECT_RATIO = 0.6f; // 端判定用の比率
        private const float RANGE_HALF = 0.5f;         // 範囲の半分

        [Header("生成位置設定")]
        [SerializeField] private Transform topSpawnPoint;
        [SerializeField] private Transform bottomSpawnPoint;
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;
        
        [Header("ランダム生成範囲設定")]
        [SerializeField] private float horizontalSpawnRange = 10f; // 上下からスポーンする際のX軸範囲
        [SerializeField] private float verticalSpawnRange = 8f;     // 左右からスポーンする際のY軸範囲
        
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
        [SerializeField] private Vector2 gameAreaMax = new Vector2(149, 149); // ゲームエリアの最大座標
        [SerializeField] private float cornerRandomRange = 3f; // 各角周辺のランダム範囲
        
        private float cachedRangeX;
        private float cachedRangeY;

        public void InitRangesFromTransforms()
        {
            // スポーンポイントの検証（エラーのみログ出力）
            ValidateSpawnPoints();
            
            // Inspector設定の範囲値を直接使用
            cachedRangeX = horizontalSpawnRange;
            cachedRangeY = verticalSpawnRange;
            
            Debug.Log($"[SpawnPositionManager] 初期化完了 - 範囲 X:{cachedRangeX}, Y:{cachedRangeY}");
            Debug.Log($"[SpawnPositionManager] 設定 - 境界線:{useBoundarySpawn}, 四隅:{useCornerSpawn}, 直接座標:{useDirectCoordinates}");
            Debug.Log($"[SpawnPositionManager] エリア:{gameAreaMin}~{gameAreaMax}, 境界オフセット:{boundaryOffset}, 角ランダム範囲:{cornerRandomRange}");
            
            // 各スポーンポイントの位置をログ出力
            if (topSpawnPoint != null) Debug.Log($"[SpawnPositionManager] 上スポーンポイント: {topSpawnPoint.position}");
            if (bottomSpawnPoint != null) Debug.Log($"[SpawnPositionManager] 下スポーンポイント: {bottomSpawnPoint.position}");
            if (leftSpawnPoint != null) Debug.Log($"[SpawnPositionManager] 左スポーンポイント: {leftSpawnPoint.position}");
            if (rightSpawnPoint != null) Debug.Log($"[SpawnPositionManager] 右スポーンポイント: {rightSpawnPoint.position}");
        }
        
        /// <summary>
        /// スポーンポイントの検証（設定ミスの早期発見用）
        /// </summary>
        private void ValidateSpawnPoints()
        {
            if (topSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Top Spawn Point が設定されていません");
            if (bottomSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Bottom Spawn Point が設定されていません");
            if (leftSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Left Spawn Point が設定されていません");
            if (rightSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Right Spawn Point が設定されていません");
        }

        /// <summary>
        /// 上から下への生成位置をランダムに取得
        /// </summary>
        /// <returns>生成位置</returns>
        public Vector3 GetRandomTopSpawnPosition()
        {
            if (topSpawnPoint == null)
            {
                Debug.LogWarning("topSpawnPoint が設定されていません");
                return Vector3.zero;
            }
            Vector3 basePosition = topSpawnPoint.position;
            float randomX = Random.Range(
                basePosition.x - cachedRangeX * RANGE_HALF,
                basePosition.x + cachedRangeX * RANGE_HALF
            );
            Vector3 spawnPosition = new Vector3(randomX, basePosition.y, 0f);
            Debug.Log($"[SpawnPositionManager] 上からの生成位置: {spawnPosition} (基準位置: {basePosition}, 範囲: {cachedRangeX})");
            return spawnPosition;
        }
        
        /// <summary>
        /// 四隅生成モードを設定
        /// </summary>
        /// <param name="enabled">四隅生成を有効にするか</param>
        public void SetCornerSpawnMode(bool enabled)
        {
            useCornerSpawn = enabled;
            Debug.Log($"[SpawnPositionManager] 四隅生成モード: {(enabled ? "有効" : "無効")}");
        }
        
        /// <summary>
        /// 現在の生成モードを取得
        /// </summary>
        /// <returns>四隅生成が有効かどうか</returns>
        public bool IsCornerSpawnMode()
        {
            return useCornerSpawn;
        }
        
        /// <summary>
        /// Canvas参照モードを設定
        /// </summary>
        /// <param name="enabled">Canvas参照を有効にするか</param>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="camera">参照カメラ（オプション）</param>
        public void SetCanvasReferenceMode(bool enabled, Canvas canvas = null, Camera camera = null)
        {
            useCanvasReference = enabled;
            if (canvas != null) targetCanvas = canvas;
            if (camera != null) referenceCamera = camera;
            
            // 他の参照モードを無効化
            if (enabled)
            {
                useTileMapReference = false;
            }
            
            Debug.Log($"[SpawnPositionManager] Canvas参照モード: {(enabled ? "有効" : "無効")}");
        }
        
        /// <summary>
        /// Canvas参照モードが有効かどうか
        /// </summary>
        /// <returns>Canvas参照が有効かどうか</returns>
        public bool IsCanvasReferenceMode()
        {
            return useCanvasReference;
        }
        
        /// <summary>
        /// TileMap参照モードを設定
        /// </summary>
        /// <param name="enabled">TileMap参照を有効にするか</param>
        /// <param name="tileMap">対象のTileMap</param>
        /// <param name="padding">境界からのパディング</param>
        public void SetTileMapReferenceMode(bool enabled, Tilemap tileMap = null, float padding = 0.5f)
        {
            useTileMapReference = enabled;
            if (tileMap != null) targetTileMap = tileMap;
            tileMapPadding = padding;
            
            // 他の参照モードを無効化
            if (enabled)
            {
                useCanvasReference = false;
            }
            
            Debug.Log($"[SpawnPositionManager] TileMap参照モード: {(enabled ? "有効" : "無効")}");
        }
        
        /// <summary>
        /// TileMap参照モードが有効かどうか
        /// </summary>
        /// <returns>TileMap参照が有効かどうか</returns>
        public bool IsTileMapReferenceMode()
        {
            return useTileMapReference;
        }
        
        /// <summary>
        /// 画面の四方向または四隅からランダムに生成位置を取得
        /// </summary>
        /// <returns>生成位置</returns>
        public Vector3 GetRandomEdgeSpawnPosition()
        {
            if (useBoundarySpawn)
            {
                Debug.Log("[SpawnPositionManager] 境界線生成モードで生成");
                return GetRandomBoundaryPosition();
            }
            else if (useCornerSpawn)
            {
                Debug.Log("[SpawnPositionManager] 四隅生成モードで生成");
                return GetRandomCornerSpawnPosition();
            }
            else
            {
                // 従来の四方向生成
                int side = Random.Range(0, 4);
                
                Vector3 spawnPos = side switch
                {
                    0 => GetTopPosition(),    // 上
                    1 => GetBottomPosition(), // 下
                    2 => GetLeftPosition(),   // 左
                    3 => GetRightPosition(),  // 右
                    _ => GetTopPosition()
                };
                
                string sideName = side switch
                {
                    0 => "上",
                    1 => "下", 
                    2 => "左",
                    3 => "右",
                    _ => "上"
                };
                
                Debug.Log($"[SpawnPositionManager] エッジ生成位置: {spawnPos} ({sideName}側)");
                return spawnPos;
            }
        }
        
        /// <summary>
        /// 四隅からランダムに生成位置を取得
        /// </summary>
        /// <returns>生成位置</returns>
        public Vector3 GetRandomCornerSpawnPosition()
        {
            // 四隅のどこから生成するかをランダム選択
            int corner = Random.Range(0, 4);
            
            Vector3 spawnPos = corner switch
            {
                0 => GetTopLeftCornerPosition(),     // 左上
                1 => GetTopRightCornerPosition(),    // 右上
                2 => GetBottomLeftCornerPosition(),  // 左下
                3 => GetBottomRightCornerPosition(), // 右下
                _ => GetTopLeftCornerPosition()
            };
            
            string cornerName = corner switch
            {
                0 => "左上",
                1 => "右上", 
                2 => "左下",
                3 => "右下",
                _ => "左上"
            };
            
            Debug.Log($"[SpawnPositionManager] 四隅生成位置: {spawnPos} ({cornerName}角)");
            return spawnPos;
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
            // 四辺のどこから生成するかをランダム選択
            int side = Random.Range(0, 4);
            
            Vector3 spawnPosition;
            string sideName;
            
            switch (side)
            {
                case 0: // 上辺
                    spawnPosition = GetRandomTopBoundaryPosition();
                    sideName = "上辺";
                    break;
                case 1: // 下辺
                    spawnPosition = GetRandomBottomBoundaryPosition();
                    sideName = "下辺";
                    break;
                case 2: // 左辺
                    spawnPosition = GetRandomLeftBoundaryPosition();
                    sideName = "左辺";
                    break;
                case 3: // 右辺
                    spawnPosition = GetRandomRightBoundaryPosition();
                    sideName = "右辺";
                    break;
                default:
                    spawnPosition = GetRandomTopBoundaryPosition();
                    sideName = "上辺";
                    break;
            }
            
            Debug.Log($"[SpawnPositionManager] 境界線生成: {spawnPosition} ({sideName})");
            return spawnPosition;
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
        
        private Vector3 GetTopPosition()
        {
            if (topSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] topSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = topSpawnPoint.position;
            float minX = basePosition.x - cachedRangeX * RANGE_HALF;
            float maxX = basePosition.x + cachedRangeX * RANGE_HALF;
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, basePosition.y, 0f);
            Debug.Log($"[SpawnPositionManager] 上側生成位置: {spawnPosition} (基準: {basePosition}, X範囲: {minX}~{maxX})");
            return spawnPosition;
        }
        
        private Vector3 GetBottomPosition()
        {
            if (bottomSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] bottomSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = bottomSpawnPoint.position;
            float minX = basePosition.x - cachedRangeX * RANGE_HALF;
            float maxX = basePosition.x + cachedRangeX * RANGE_HALF;
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, basePosition.y, 0f);
            Debug.Log($"[SpawnPositionManager] 下側生成位置: {spawnPosition} (基準: {basePosition}, X範囲: {minX}~{maxX})");
            return spawnPosition;
        }
        
        private Vector3 GetLeftPosition()
        {
            if (leftSpawnPoint == null) 
            {  
                Debug.LogError("[SpawnPositionManager] leftSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = leftSpawnPoint.position;
            float minY = basePosition.y - cachedRangeY * RANGE_HALF;
            float maxY = basePosition.y + cachedRangeY * RANGE_HALF;
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(basePosition.x, randomY, 0f);
            Debug.Log($"[SpawnPositionManager] 左側生成位置: {spawnPosition} (基準: {basePosition}, Y範囲: {minY}~{maxY})");
            return spawnPosition;
        }
        
        private Vector3 GetRightPosition()
        {
            if (rightSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] rightSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = rightSpawnPoint.position;
            float minY = basePosition.y - cachedRangeY * RANGE_HALF;
            float maxY = basePosition.y + cachedRangeY * RANGE_HALF;
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(basePosition.x, randomY, 0f);
            Debug.Log($"[SpawnPositionManager] 右側生成位置: {spawnPosition} (基準: {basePosition}, Y範囲: {minY}~{maxY})");
            return spawnPosition;
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
            else
            {
                if (topSpawnPoint == null || leftSpawnPoint == null)
                {
                    Debug.LogError("[SpawnPositionManager] 左上隅の計算に必要なSpawnPointがnullです");
                    return Vector3.zero;
                }
                
                float x = leftSpawnPoint.position.x + cornerOffset;
                float y = topSpawnPoint.position.y - cornerOffset;
                
                Vector3 spawnPosition = new Vector3(x, y, 0f);
                Debug.Log($"[SpawnPositionManager] 左上角生成位置(Transform): {spawnPosition} (左: {leftSpawnPoint.position}, 上: {topSpawnPoint.position}, オフセット: {cornerOffset})");
                return spawnPosition;
            }
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
            else
            {
                if (topSpawnPoint == null || rightSpawnPoint == null)
                {
                    Debug.LogError("[SpawnPositionManager] 右上隅の計算に必要なSpawnPointがnullです");
                    return Vector3.zero;
                }
                
                float x = rightSpawnPoint.position.x - cornerOffset;
                float y = topSpawnPoint.position.y - cornerOffset;
                
                return new Vector3(x, y, 0f);
            }
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
            else
            {
                if (bottomSpawnPoint == null || leftSpawnPoint == null)
                {
                    Debug.LogError("[SpawnPositionManager] 左下隅の計算に必要なSpawnPointがnullです");
                    return Vector3.zero;
                }
                
                float x = leftSpawnPoint.position.x + cornerOffset;
                float y = bottomSpawnPoint.position.y + cornerOffset;
                
                return new Vector3(x, y, 0f);
            }
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
            else
            {
                if (bottomSpawnPoint == null || rightSpawnPoint == null)
                {
                    Debug.LogError("[SpawnPositionManager] 右下隅の計算に必要なSpawnPointがnullです");
                    return Vector3.zero;
                }
                
                float x = rightSpawnPoint.position.x - cornerOffset;
                float y = bottomSpawnPoint.position.y + cornerOffset;
                
                return new Vector3(x, y, 0f);
            }
        }
        
        /// <summary>
        /// すべてのスポーンポイントが設定されているかチェック
        /// </summary>
        /// <returns>設定状況</returns>
        public bool AreAllSpawnPointsSet()
        {
            return topSpawnPoint != null && 
                   bottomSpawnPoint != null && 
                   leftSpawnPoint != null && 
                   rightSpawnPoint != null;
        }
        
        /// <summary>
        /// 設定されていないスポーンポイントの警告を表示
        /// </summary>
        public void LogMissingSpawnPoints()
        {
            if (topSpawnPoint == null) Debug.LogWarning("topSpawnPoint が設定されていません");
            if (bottomSpawnPoint == null) Debug.LogWarning("bottomSpawnPoint が設定されていません");
            if (leftSpawnPoint == null) Debug.LogWarning("leftSpawnPoint が設定されていません");
            if (rightSpawnPoint == null) Debug.LogWarning("rightSpawnPoint が設定されていません");
        }
        
        /// <summary>
        /// 四隅の位置をデバッグ表示
        /// </summary>
        public void DebugCornerPositions()
        {
            if (useCanvasReference)
            {
                if (targetCanvas == null)
                {
                    Debug.LogWarning("[SpawnPositionManager] Canvas参照モードですが、targetCanvasが設定されていません");
                    return;
                }
                
                Debug.Log("=== 四隅の生成位置 (Canvas参照) ===");
                Debug.Log($"対象Canvas: {targetCanvas.name}");
            }
            else
            {
                if (!AreAllSpawnPointsSet())
                {
                    Debug.LogWarning("[SpawnPositionManager] すべてのSpawnPointが設定されていないため、四隅位置を計算できません");
                    return;
                }
                
                Debug.Log("=== 四隅の生成位置 (Transform参照) ===");
            }
            
            Debug.Log($"左上: {GetTopLeftCornerPosition()}");
            Debug.Log($"右上: {GetTopRightCornerPosition()}");
            Debug.Log($"左下: {GetBottomLeftCornerPosition()}");
            Debug.Log($"右下: {GetBottomRightCornerPosition()}");
            Debug.Log($"四隅生成モード: {(useCornerSpawn ? "有効" : "無効")}");
            Debug.Log($"Canvas参照モード: {(useCanvasReference ? "有効" : "無効")}");
        }

        /// <summary>
        /// 逆側のエッジ位置を取得
        /// スポーン位置が端に近い場合、反対側の端の位置を返す
        /// </summary>
        /// <param name="spawnPos">スポーン位置</param>
        /// <returns>逆側のエッジ位置</returns>
        public Vector3 GetOppositeEdgePosition(Vector3 spawnPos)
        {
            // 上端判定
            if (topSpawnPoint != null && bottomSpawnPoint != null && Mathf.Abs(spawnPos.y - topSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 上端→下端
                return new Vector3(spawnPos.x, bottomSpawnPoint.position.y, 0f);
            }
            // 下端判定
            if (bottomSpawnPoint != null && topSpawnPoint != null && Mathf.Abs(spawnPos.y - bottomSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 下端→上端
                return new Vector3(spawnPos.x, topSpawnPoint.position.y, 0f);
            }
            // 左端判定
            if (leftSpawnPoint != null && rightSpawnPoint != null && Mathf.Abs(spawnPos.x - leftSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 左端→右端
                return new Vector3(rightSpawnPoint.position.x, spawnPos.y, 0f);
            }
            // 右端判定
            if (rightSpawnPoint != null && leftSpawnPoint != null && Mathf.Abs(spawnPos.x - rightSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 右端→左端
                return new Vector3(leftSpawnPoint.position.x, spawnPos.y, 0f);
            }
            
            // どれにも該当しない場合はフォールバック処理
#if UNITY_EDITOR
            Debug.LogWarning($"[SpawnPositionManager] 端判定に該当しなかったため、フォールバック処理を実行");
#endif
            return GetFallbackOppositePosition(spawnPos);
        }
        
        /// <summary>
        /// フォールバック用の反対側位置計算
        /// </summary>
        private Vector3 GetFallbackOppositePosition(Vector3 spawnPos)
        {
            // 各端との距離を計算
            float distToTop = topSpawnPoint != null ? Mathf.Abs(spawnPos.y - topSpawnPoint.position.y) : float.MaxValue;
            float distToBottom = bottomSpawnPoint != null ? Mathf.Abs(spawnPos.y - bottomSpawnPoint.position.y) : float.MaxValue;
            float distToLeft = leftSpawnPoint != null ? Mathf.Abs(spawnPos.x - leftSpawnPoint.position.x) : float.MaxValue;
            float distToRight = rightSpawnPoint != null ? Mathf.Abs(spawnPos.x - rightSpawnPoint.position.x) : float.MaxValue;
            
            // 最も近い端を特定
            float minDistance = Mathf.Min(distToTop, distToBottom, distToLeft, distToRight);
            
            if (minDistance == distToTop && topSpawnPoint != null && bottomSpawnPoint != null)
            {
                return new Vector3(spawnPos.x, bottomSpawnPoint.position.y, 0f);
            }
            else if (minDistance == distToBottom && bottomSpawnPoint != null && topSpawnPoint != null)
            {
                return new Vector3(spawnPos.x, topSpawnPoint.position.y, 0f);
            }
            else if (minDistance == distToLeft && leftSpawnPoint != null && rightSpawnPoint != null)
            {
                return new Vector3(rightSpawnPoint.position.x, spawnPos.y, 0f);
            }
            else if (minDistance == distToRight && rightSpawnPoint != null && leftSpawnPoint != null)
            {
                return new Vector3(leftSpawnPoint.position.x, spawnPos.y, 0f);
            }
            
            // 最終フォールバック: 画面外の遠い位置
            return new Vector3(spawnPos.x * -2f, spawnPos.y * -2f, 0f);
        }

        /// <summary>
        /// 境界線生成モードを設定
        /// </summary>
        /// <param name="enabled">境界線生成を有効にするか</param>
        /// <param name="offset">境界線からのオフセット</param>
        public void SetBoundarySpawnMode(bool enabled, float offset = 1f)
        {
            useBoundarySpawn = enabled;
            boundaryOffset = offset;
            
            // 他のモードを無効化
            if (enabled)
            {
                useCornerSpawn = false;
            }
            
            Debug.Log($"[SpawnPositionManager] 境界線生成モード: {(enabled ? "有効" : "無効")}, オフセット: {offset}");
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