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
        }

        // 現在の地面の配列
        private GroundData[,] currentGroundArray;

        public void Start()
        {
            LoadStage( SceneManager.GetActiveScene().name );
        }

        /// <summary>
        /// 地面を塗る
        /// </summary>
        /// <returns>成否</returns>
        public bool DrawGround()
        {
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
                Debug.LogWarning($"JSON not found: {path}");
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
            currentGroundArray = new GroundData[mapSettings.columns, mapSettings.rows];
            for (int y = 0; y < mapSettings.rows; y++)
            {
                for (int x = 0; x < mapSettings.columns; x++)
                {
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0.0f);
                    GameObject tile = Instantiate(defaultTile, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";

                    currentGroundArray[x, y].point = 100;
                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = pos;
                    currentGroundArray[x, y].gridPos = new Vector2Int( x, y );
                }
            }
        }
    }
}
