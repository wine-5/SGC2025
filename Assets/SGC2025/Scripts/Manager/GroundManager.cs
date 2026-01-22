using UnityEngine;
using SGC2025.Enemy;
using SGC2025.Events;

namespace SGC2025
{
    /// <summary>
    /// 地面システムの管理クラス
    /// マップ生成、タイルの緑化処理
    /// </summary>
    public class GroundManager : Singleton<GroundManager>
    {
        private const int DEFAULT_TILE_POINT = 100;
        private const int DEFAULT_HIGH_SCORE_MULTIPLIER = 3;
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
        private Material grassMaterial;
        
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
            
            grassMaterial = Resources.Load<Material>("Materials/grass");
            if (!grassMaterial)
                Debug.LogError("GroundManager: 草マテリアルが見つかりません");
            
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
            if (currentGroundArray == null || grassMaterial == null) return false;
            Vector2Int cellPosition = SearchCellIndex(enemyPosition);
            if (cellPosition.x < 0 || cellPosition.x >= groundData.columns ||
                cellPosition.y < 0 || cellPosition.y >= groundData.rows) return false;
            if (currentGroundArray[cellPosition.x, cellPosition.y].isDrawn) return false;
            
            currentGroundArray[cellPosition.x, cellPosition.y].isDrawn = true;
            currentGroundArray[cellPosition.x, cellPosition.y].renderer.material = grassMaterial;

            int points = currentGroundArray[cellPosition.x, cellPosition.y].point;
            ScoreManager.I?.AddGreenScore(points);
            
            return true;
        }

        private Vector2Int SearchCellIndex(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - currentOriginPosition.x) / groundData.cellWidth);
            int y = Mathf.RoundToInt((position.y - currentOriginPosition.y) / groundData.cellHeight);

            x = Mathf.Clamp(x, 0, groundData.columns - 1);
            y = Mathf.Clamp(y, 0, groundData.rows - 1);
            
            return new Vector2Int(x, y);
        }

        private void SetStageObject()
        {
            currentGroundArray = new GroundData[groundData.columns, groundData.rows];
            
            for (int y = 0; y < groundData.rows; y++)
            {
                for (int x = 0; x < groundData.columns; x++)
                {
                    Vector3 pos = new Vector3(x * groundData.cellWidth, y * groundData.cellHeight, TILE_Z_POSITION);
                    
                    if (groundData.tilePrefab == null)
                    {
                        Debug.LogError("GroundManager: tilePrefabが設定されていません");
                        return;
                    }
                    
                    GameObject tile = Instantiate(groundData.tilePrefab, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";

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
        
        #region Debug Methods
        
        [ContextMenu("Debug Ground Status")]
        public void DebugGroundStatus()
        {
            if (currentGroundArray == null)
            {
                Debug.Log("GroundManager: 地面データが初期化されていません");
                return;
            }
            
            int totalCells = groundData.columns * groundData.rows;
            int drawnCells = 0;
            
            for (int y = 0; y < groundData.rows; y++)
            {
                for (int x = 0; x < groundData.columns; x++)
                {
                    if (currentGroundArray[x, y].isDrawn)
                        drawnCells++;
                }
            }
            
            float percentage = (float)drawnCells / totalCells * 100f;
            Debug.Log($"GroundManager: 緑化状況 {drawnCells}/{totalCells} ({percentage:F1}%)");
        }
        
        public void DebugCellInfo(Vector3 worldPosition)
        {
            if (currentGroundArray == null) return;
            
            Vector2Int cellPos = SearchCellIndex(worldPosition);
            var cellData = currentGroundArray[cellPos.x, cellPos.y];
            
            Debug.Log($"セル情報 - 座標:({cellPos.x}, {cellPos.y}), " +
                     $"ワールド座標:{cellData.worldPos}, " +
                     $"塗られている:{cellData.isDrawn}, " +
                     $"ポイント:{cellData.point}");
        }
        
        [ContextMenu("Test Draw Ground")]
        public void TestDrawGround()
        {
            Vector3 testPosition = new Vector3(
                groundData.columns * groundData.cellWidth * 0.5f, 
                groundData.rows * groundData.cellHeight * 0.5f, 
                TILE_Z_POSITION);
            DrawGround(testPosition);
        }
        
        #endregion
    }
}
