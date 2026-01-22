using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using SGC2025.Enemy;
using SGC2025.Events;

namespace SGC2025
{
    public class GroundManager : Singleton<GroundManager>
    {
        // TODO:AddressableかResouseで取得する
        [SerializeField]
        private GameObject defaultTile;

        // TODO:定数にしたい
        [SerializeField]
        private float cellSize;

        // 地面データ
        private struct GroundData
        {
            // グリッド座標 (col,row)
            public Vector2Int gridPos;

            // 配置座標
            public Vector2 worldPos;

            // 塗られたか
            public bool isDrawn;

            // ポイント
            public int point;

            // レンダラー
            public Renderer renderer;
        }

        // 現在の地面の配列
        private GroundData[,] currentGroundArray ;

        // 現在の原点
        private Vector3 currentOriginPosisiton ;

        // マップ情報
        private MapSettings mapSetting;

        // 緑地マテリアル
        private Material grassMaterial;
        
        /// <summary>マップの列数（幅）を取得</summary>
        public int MapColumns => mapSetting?.columns ?? 0;
        
        /// <summary>マップの行数（高さ）を取得</summary>
        public int MapRows => mapSetting?.rows ?? 0;
        
        /// <summary>マップの最大座標を取得（0ベース）</summary>
        public Vector2Int MapMaxIndex => new Vector2Int(MapColumns - 1, MapRows - 1);

        public void Start()
        {
            LoadStage(SceneManager.GetActiveScene().name);
            InitHighObject();
            grassMaterial = Resources.Load<Material>("Materials/grass");
            if (!grassMaterial)
            {
                Debug.LogWarning("GroundManager : 草マテリアルがありません。");
            }
            
            // 敵撃破イベントを購読（EnemyEventsクラス経由）
            EnemyEvents.OnEnemyDestroyedAtPosition += OnEnemyDestroyed;
        }
        
        protected override void OnDestroy()
        {
            // イベントの購読解除
            EnemyEvents.OnEnemyDestroyedAtPosition -= OnEnemyDestroyed;
            base.OnDestroy();
        }
        
        /// <summary>
        /// 敵撃破時の処理
        /// </summary>
        /// <param name="enemyPosition">敵が撃破された位置</param>
        private void OnEnemyDestroyed(Vector3 enemyPosition)
        {
            DrawGround(enemyPosition);
        }

        /// <summary>
        /// 地面を塗る
        /// </summary>
        /// <param name="enemyPosition">敵が倒された位置</param>
        /// <returns>成否</returns>
        public bool DrawGround(Vector3 enemyPosition)
        {
            if (currentGroundArray == null || grassMaterial == null)
            {
                Debug.LogWarning("GroundManager: 初期化が完了していないか、草マテリアルがありません");
                return false;
            }
            
            Vector2Int cellPosition = SearchCellIndex(enemyPosition);
            
            // 範囲チェック
            if (cellPosition.x < 0 || cellPosition.x >= mapSetting.columns ||
                cellPosition.y < 0 || cellPosition.y >= mapSetting.rows)
            {
                Debug.LogWarning($"GroundManager: 位置 {enemyPosition} がマップ範囲外です");
                return false;
            }
            
            // 既に塗られている場合はスキップ
            if (currentGroundArray[cellPosition.x, cellPosition.y].isDrawn)
            {
                Debug.Log($"GroundManager: セル({cellPosition.x}, {cellPosition.y})は既に塗られています");
                return false;
            }
            
            // 地面を塗る
            currentGroundArray[cellPosition.x, cellPosition.y].isDrawn = true;
            currentGroundArray[cellPosition.x, cellPosition.y].renderer.material = grassMaterial;

            // スコア追加
            int points = currentGroundArray[cellPosition.x, cellPosition.y].point;
            if (ScoreManager.I != null)
            {
                ScoreManager.I.AddGreenScore(points);
            }
            
            Debug.Log($"GroundManager: セル({cellPosition.x}, {cellPosition.y})を緑化しました (ポイント: {points})");
            return true;
        }

        /// <summary>
        /// ワールド座標からセルインデックスを取得
        /// </summary>
        /// <param name="position">ワールド座標</param>
        /// <returns>セルインデックス</returns>
        private Vector2Int SearchCellIndex(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - currentOriginPosisiton.x) / cellSize);
            int y = Mathf.RoundToInt((position.y - currentOriginPosisiton.y) / cellSize);

            // 範囲をクランプ
            x = Mathf.Clamp(x, 0, mapSetting.columns - 1);
            y = Mathf.Clamp(y, 0, mapSetting.rows - 1);
            
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// ステージロード
        /// </summary>
        public void LoadStage(string sceneName)
        {
            string path = Path.Combine(Application.dataPath, "Maps", $"{sceneName}_map.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"GroundManager : JSON が見つかりません: {path}");
            }

            string json = File.ReadAllText(path);
            MapSettings temp = ScriptableObject.CreateInstance<MapSettings>();
            JsonUtility.FromJsonOverwrite(json, temp);
            SetStageObject(temp);
        }

        /// <summary>
        /// ステージの配置
        /// </summary>
        private void SetStageObject(MapSettings mapSettings)
        {
            mapSetting = mapSettings;
            currentGroundArray = new GroundData[mapSettings.columns, mapSettings.rows];
            
            Debug.Log($"GroundManager: マップ生成開始 - サイズ: {mapSettings.columns}x{mapSettings.rows}, cellSize: {cellSize}");
            
            for (int y = 0; y < mapSettings.rows; y++)
            {
                for (int x = 0; x < mapSettings.columns; x++)
                {
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0.0f);
                    GameObject tile = Instantiate(defaultTile, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";

                    currentGroundArray[x, y].point = 100; // todo 定数
                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = pos;
                    currentGroundArray[x, y].gridPos = new Vector2Int(x, y);
                    currentGroundArray[x, y].renderer = tile.GetComponent<Renderer>();
                }
            }
            
            // 原点位置を設定
            currentOriginPosisiton = transform.position;
            
            Debug.Log($"GroundManager: マップ生成完了 - タイル数: {mapSettings.columns * mapSettings.rows}");
        }

        /// <summary>
        /// 高得点オブジェクトの初期化
        /// </summary>
        private void InitHighObject()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("HighScoreObject");
            
            foreach(GameObject highScore in objects)
            {
                Vector2Int cellPosition = SearchCellIndex(highScore.transform.position);
                currentGroundArray[cellPosition.x, cellPosition.y].point = currentGroundArray[cellPosition.x, cellPosition.y].point*3;
            }
        }
        
        /// <summary>
        /// 緑化状況をデバッグ表示
        /// </summary>
        [ContextMenu("Debug Ground Status")]
        public void DebugGroundStatus()
        {
            if (currentGroundArray == null)
            {
                Debug.Log("GroundManager: 地面データが初期化されていません");
                return;
            }
            
            int totalCells = mapSetting.columns * mapSetting.rows;
            int drawnCells = 0;
            
            for (int y = 0; y < mapSetting.rows; y++)
            {
                for (int x = 0; x < mapSetting.columns; x++)
                {
                    if (currentGroundArray[x, y].isDrawn)
                    {
                        drawnCells++;
                    }
                }
            }
            
            float percentage = (float)drawnCells / totalCells * 100f;
            Debug.Log($"GroundManager: 緑化状況 {drawnCells}/{totalCells} ({percentage:F1}%)");
        }
        
        /// <summary>
        /// 指定位置のセル情報を取得（デバッグ用）
        /// </summary>
        /// <param name="worldPosition">ワールド座標</param>
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
        
        /// <summary>
        /// 手動で地面を塗る（テスト用）
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        [ContextMenu("Test Draw Ground")]
        public void TestDrawGround()
        {
            // テスト用：中央付近の地面を塗る
            Vector3 testPosition = new Vector3(mapSetting.columns * cellSize * 0.5f, 
                                               mapSetting.rows * cellSize * 0.5f, 0);
            DrawGround(testPosition);
        }
    }
}
