using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

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
            public uint point;

            // レンダラー
            public Renderer renderer;
        }

        // 現在の地面の配列
        private GroundData[,] currentGroundArray;

        // 現在の原点
        private Vector3 currentOriginPosisiton;

        // マップ情報
        private MapSettings mapSetting;

        // 緑地マテリアル
        private Material grassMaterial;

        public void Start()
        {
            LoadStage( SceneManager.GetActiveScene().name );
            grassMaterial = Resources.Load<Material>("Materials/grass");
            if (!grassMaterial)
            {
                Debug.LogWarning("GroundManager : 草マテリアルがありません。");
            }
        }

        /// <summary>
        /// 地面を塗る
        /// </summary>
        /// <returns>成否</returns>
        public bool DrawGround(Vector3 enemyPosition)
        {
            // todo 関数を分ける
            // --近い座標のものを取得-------------------------------------------
            int x = Mathf.RoundToInt((enemyPosition.x - currentOriginPosisiton.x) / cellSize);
            int y = Mathf.RoundToInt((enemyPosition.y - currentOriginPosisiton.y) / cellSize);

            x = Mathf.Clamp(x, 0, mapSetting.columns - 1);
            y = Mathf.Clamp(y, 0, mapSetting.rows - 1);
            // ---------------------------------------------
            currentGroundArray[x, y].isDrawn = true;
            currentGroundArray[x, y].renderer.material = grassMaterial;

            Debug.Log("GroundManager : Draw" + x + "" + y);
            return false;
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
        }
    }
}
