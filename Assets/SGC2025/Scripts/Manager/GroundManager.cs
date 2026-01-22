using UnityEngine;
using SGC2025.Events;

namespace SGC2025
{
    /// <summary>
    /// 地面システムの管理クラス
    /// マップ生成、タイルの緑化処理
    /// </summary>
    public class GroundManager : Singleton<GroundManager>
    {
        private const float TILE_Z_POSITION = 0f;
        
        [Header("地面データ設定")]
        [SerializeField]
        [Tooltip("使用するGroundDataSO（マップ設定の単一の真実の源泉）")]
        private GroundDataSO groundData;

        private struct GroundData
        {
            public Vector2Int gridPos;
            public Vector2 worldPos;
            public bool isDrawn;
            public int point;
            public Renderer renderer;
        }

        private GroundData[,] currentGroundArray;
        private Vector3 currentOriginPosition;
        private GameObject[,] tileObjects;
        
        public GroundDataSO MapData => groundData;
        public int MapColumns => groundData?.columns ?? 0;
        public int MapRows => groundData?.rows ?? 0;
        public Vector2Int MapMaxIndex => new Vector2Int(MapColumns - 1, MapRows - 1);
        
        /// <summary>Playerのスポーン位置を取得（マップの中心）</summary>
        public Vector3 GetPlayerSpawnPosition()
        {
            if (groundData == null)
            {
                Debug.LogError("GroundManager: GroundDataSOが設定されていません");
                return Vector3.zero;
            }
            return groundData.MapCenterPosition;
        }

        public void Start()
        {
            if (groundData == null)
            {
                Debug.LogError("GroundManager: GroundDataSOが設定されていません！");
                return;
            }
            
            SetStageObject();
            InitHighObject();
            EnemyEvents.OnEnemyDestroyedAtPosition += OnEnemyDestroyed;
        }
        
        protected override void OnDestroy()
        {
            EnemyEvents.OnEnemyDestroyedAtPosition -= OnEnemyDestroyed;
            base.OnDestroy();
        }
        
        private void OnEnemyDestroyed(Vector3 enemyPosition) => DrawGround(enemyPosition);

        /// <summary>指定位置の地面を緑化</summary>
        public bool DrawGround(Vector3 enemyPosition)
        {
            if (currentGroundArray == null) return false;
            
            Vector2Int cellPosition = SearchCellIndex(enemyPosition);
            
            if (cellPosition.x < 0 || cellPosition.x >= groundData.columns ||
                cellPosition.y < 0 || cellPosition.y >= groundData.rows) return false;
            
            if (currentGroundArray[cellPosition.x, cellPosition.y].isDrawn) return false;
            
            if (groundData.grassTilePrefab == null)
            {
                Debug.LogWarning("[GroundManager] grassTilePrefabが設定されていません。緑化をスキップします。");
                return false;
            }
            
            if (tileObjects != null && tileObjects[cellPosition.x, cellPosition.y] != null)
                Destroy(tileObjects[cellPosition.x, cellPosition.y]);
            
            Vector3 pos = currentGroundArray[cellPosition.x, cellPosition.y].worldPos;
            GameObject grassTile = Instantiate(groundData.grassTilePrefab, pos, Quaternion.identity, transform);
            grassTile.name = $"GrassTile_{cellPosition.x}_{cellPosition.y}";
            
            AdjustTileScale(grassTile, groundData.ActualCellWidth, groundData.ActualCellHeight);
            
            if (tileObjects != null)
                tileObjects[cellPosition.x, cellPosition.y] = grassTile;
            
            Renderer newRenderer = grassTile.GetComponent<Renderer>();
            currentGroundArray[cellPosition.x, cellPosition.y].renderer = newRenderer;
            currentGroundArray[cellPosition.x, cellPosition.y].isDrawn = true;

            int points = currentGroundArray[cellPosition.x, cellPosition.y].point;
            GroundEvents.TriggerGroundGreenified(pos, points);
            
            return true;
        }

        private Vector2Int SearchCellIndex(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - currentOriginPosition.x) / groundData.ActualCellWidth);
            int y = Mathf.RoundToInt((position.y - currentOriginPosition.y) / groundData.ActualCellHeight);

            x = Mathf.Clamp(x, 0, groundData.columns - 1);
            y = Mathf.Clamp(y, 0, groundData.rows - 1);
            
            return new Vector2Int(x, y);
        }

        private void SetStageObject()
        {
            currentGroundArray = new GroundData[groundData.columns, groundData.rows];
            tileObjects = new GameObject[groundData.columns, groundData.rows];
            
            for (int y = 0; y < groundData.rows; y++)
            {
                for (int x = 0; x < groundData.columns; x++)
                {
                    Vector3 pos = new Vector3(x * groundData.ActualCellWidth, y * groundData.ActualCellHeight, TILE_Z_POSITION);
                    
                    if (groundData.tilePrefab == null)
                    {
                        Debug.LogError("GroundManager: tilePrefabが設定されていません");
                        return;
                    }
                    
                    GameObject tile = Instantiate(groundData.tilePrefab, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";
                    tileObjects[x, y] = tile;

                    if (ScoreManager.I == null)
                    {
                        Debug.LogError("GroundManager: ScoreManagerが見つかりません");
                        return;
                    }
                    
                    currentGroundArray[x, y].point = ScoreManager.I.NormalTilePoint;
                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = pos;
                    currentGroundArray[x, y].gridPos = new Vector2Int(x, y);
                    currentGroundArray[x, y].renderer = tile.GetComponent<Renderer>();
                }
            }
            
            currentOriginPosition = transform.position;
        }

        private void InitHighObject()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("HighScoreObject");
            
            if (ScoreManager.I == null)
            {
                Debug.LogError("GroundManager: ScoreManagerが見つかりません");
                return;
            }
            
            foreach(GameObject highScore in objects)
            {
                Vector2Int cellPosition = SearchCellIndex(highScore.transform.position);
                int multiplier = ScoreManager.I.HighScoreTileMultiplier;
                currentGroundArray[cellPosition.x, cellPosition.y].point *= multiplier;
            }
        }
        
        /// <summary>タイルのスケールをセルサイズに合わせて調整</summary>
        private void AdjustTileScale(GameObject tile, float targetWidth, float targetHeight)
        {
            if (tile == null) return;
            
            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                float pixelsPerUnit = sprite.pixelsPerUnit;
                
                Vector2 spriteSize = new Vector2(
                    sprite.rect.width / pixelsPerUnit,
                    sprite.rect.height / pixelsPerUnit
                );
                
                float scaleX = targetWidth / spriteSize.x;
                float scaleY = targetHeight / spriteSize.y;
                float uniformScale = Mathf.Min(scaleX, scaleY);
                tile.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                
                return;
            }
            
            var meshRenderer = tile.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Bounds bounds = meshRenderer.bounds;
                float scaleX = targetWidth / bounds.size.x;
                float scaleY = targetHeight / bounds.size.y;
                float uniformScale = Mathf.Min(scaleX, scaleY);
                tile.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                return;
            }
            
            Debug.LogWarning($"[GroundManager] {tile.name}のスケールを自動調整できませんでした");
        }
    }
}
