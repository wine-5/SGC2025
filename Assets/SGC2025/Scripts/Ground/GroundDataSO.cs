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
        [Tooltip("使用するタイルのプレハブ")]
        public GameObject tilePrefab;
        
        [Tooltip("緑化後の草タイルのプレハブ")]
        public GameObject grassTilePrefab;
        
        [Tooltip("Prefabから自動的にサイズを取得するか")]
        public bool autoCalculateSize = true;
        
        [Tooltip("1タイルの幅（autoCalculateSizeがfalseの場合のみ使用）")]
        public float cellWidth = 1f;
        
        [Tooltip("1タイルの高さ（autoCalculateSizeがfalseの場合のみ使用）")]
        public float cellHeight = 1f;
        
        [Tooltip("タイルのアスペクト比（幅:高さ）")]
        public Vector2 tileAspect = new Vector2(1f, 1f);
        
        /// <summary>実際のセル幅を取得（自動計算対応）</summary>
        public float ActualCellWidth
        {
            get
            {
                if (autoCalculateSize && tilePrefab != null)
                {
                    return GetPrefabSize().x;
                }
                return cellWidth;
            }
        }
        
        /// <summary>実際のセル高さを取得（自動計算対応）</summary>
        public float ActualCellHeight
        {
            get
            {
                if (autoCalculateSize && tilePrefab != null)
                {
                    return GetPrefabSize().y;
                }
                return cellHeight;
            }
        }
        
        /// <summary>マップの最大インデックス（0ベース）を取得</summary>
        public Vector2Int MapMaxIndex => new Vector2Int(columns - 1, rows - 1);
        
        /// <summary>マップの中心座標（ワールド座標）を取得</summary>
        public Vector3 MapCenterPosition => new Vector3(
            (columns - 1) * ActualCellWidth * 0.5f,
            (rows - 1) * ActualCellHeight * 0.5f,
            0f
        );
        
        /// <summary>マップの物理的なサイズ（ワールド単位）を取得</summary>
        public Vector2 MapWorldSize => new Vector2(
            columns * ActualCellWidth,
            rows * ActualCellHeight
        );
        
        /// <summary>マップの最大座標（ワールド座標）を取得</summary>
        public Vector2 MapMaxWorldPosition => new Vector2(
            (columns - 1) * ActualCellWidth,
            (rows - 1) * ActualCellHeight
        );
        
        /// <summary>
        /// Prefabから実際のサイズを取得
        /// SpriteRenderer、MeshRenderer、Colliderの順で試行
        /// </summary>
        private Vector2 GetPrefabSize()
        {
            if (tilePrefab == null)
            {
                Debug.LogWarning("[GroundDataSO] tilePrefabが設定されていません");
                return new Vector2(cellWidth, cellHeight);
            }
            
            // SpriteRendererから取得を試みる
            var spriteRenderer = tilePrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                // Spriteのピクセルサイズをワールド単位に変換
                float pixelsPerUnit = sprite.pixelsPerUnit;
                Vector2 size = new Vector2(
                    sprite.rect.width / pixelsPerUnit,
                    sprite.rect.height / pixelsPerUnit
                );
                // Prefabのスケールを適用
                size.x *= tilePrefab.transform.localScale.x;
                size.y *= tilePrefab.transform.localScale.y;
                return size;
            }
            
            // MeshRendererから取得を試みる
            var meshRenderer = tilePrefab.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Bounds bounds = meshRenderer.bounds;
                return new Vector2(bounds.size.x, bounds.size.y);
            }
            
            // Colliderから取得を試みる
            var collider2D = tilePrefab.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                Bounds bounds = collider2D.bounds;
                return new Vector2(bounds.size.x, bounds.size.y);
            }
            
            var collider3D = tilePrefab.GetComponent<Collider>();
            if (collider3D != null)
            {
                Bounds bounds = collider3D.bounds;
                return new Vector2(bounds.size.x, bounds.size.y);
            }
            
            // 取得できない場合は設定値を返す
            Debug.LogWarning($"[GroundDataSO] {tilePrefab.name}からサイズを自動取得できませんでした。手動設定値を使用します。");
            return new Vector2(cellWidth, cellHeight);
        }
        
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
