using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// マップ全体の設定を管理するScriptableObject
    /// 全ての地面関連データの単一の真実の源泉
    /// </summary>
    [CreateAssetMenu(fileName = "GroundData", menuName = "SGC2025/Ground Data", order = 0)]
    public class GroundDataSO : ScriptableObject
    {
        [Header("マップサイズ設定")]
        [Tooltip("マップの列数（横幅）")]
        public int columns = 60;
        
        [Tooltip("マップの行数（縦幅）")]
        public int rows = 45;
        
        [Header("タイル設定")]
        [Tooltip("1タイルの幅")]
        public float cellWidth = 1f;
        
        [Tooltip("1タイルの高さ")]
        public float cellHeight = 1f;
        
        [Tooltip("タイルのアスペクト比（幅:高さ）")]
        public Vector2 tileAspect = new Vector2(1f, 1f);
        
        [Tooltip("使用するタイルのプレハブ")]
        public GameObject tilePrefab;
        
        /// <summary>マップの最大インデックス（0ベース）を取得</summary>
        public Vector2Int MapMaxIndex => new Vector2Int(columns - 1, rows - 1);
        
        /// <summary>マップの中心座標（ワールド座標）を取得</summary>
        public Vector3 MapCenterPosition => new Vector3(
            (columns - 1) * cellWidth * 0.5f,
            (rows - 1) * cellHeight * 0.5f,
            0f
        );
        
        /// <summary>マップの物理的なサイズ（ワールド単位）を取得</summary>
        public Vector2 MapWorldSize => new Vector2(
            columns * cellWidth,
            rows * cellHeight
        );
        
        /// <summary>マップの最大座標（ワールド座標）を取得</summary>
        public Vector2 MapMaxWorldPosition => new Vector2(
            (columns - 1) * cellWidth,
            (rows - 1) * cellHeight
        );
        
        /// <summary>エディタ用の検証メソッド</summary>
        private void OnValidate()
        {
            // 負の値を防ぐ
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellWidth = Mathf.Max(0.1f, cellWidth);
            cellHeight = Mathf.Max(0.1f, cellHeight);
        }
    }
}
