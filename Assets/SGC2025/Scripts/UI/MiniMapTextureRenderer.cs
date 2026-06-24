using UnityEngine;
using UnityEngine.UI;
using SGC2025.Manager;

namespace SGC2025.UI
{
    [System.Serializable]
    public class MiniMapTextureRenderer
    {
        [SerializeField] private RawImage mapImage;
        [SerializeField] private RawImage expandMapImage;
        [SerializeField] private Color brownColor = new(0.4f, 0.25f, 0.1f);
        [SerializeField] private Color greenColor = new(0.3f, 0.7f, 0.2f);

        private Texture2D mapTexture;
        private bool isDirty;

        public void Initialize()
        {
            if (GroundManager.I?.MapData == null) return;

            var mapData = GroundManager.I.MapData;
            mapTexture = new Texture2D(mapData.columns, mapData.rows, TextureFormat.RGBA32, false);
            mapTexture.filterMode = FilterMode.Point;

            for (int x = 0; x < mapData.columns; x++)
            {
                for (int y = 0; y < mapData.rows; y++)
                {
                    mapTexture.SetPixel(x, y, brownColor);
                }
            }
            mapTexture.Apply();

            if (mapImage != null)
                mapImage.texture = mapTexture;

            if (expandMapImage != null)
                expandMapImage.texture = mapTexture;
        }

        public void UpdateGreenifiedCell(Vector3 worldPos)
        {
            PaintCell(worldPos, greenColor);
        }

        public void UpdateUngreenifiedCell(Vector3 worldPos)
        {
            PaintCell(worldPos, brownColor);
        }

        private void PaintCell(Vector3 worldPos, Color color)
        {
            if (mapTexture == null || GroundManager.I?.MapData == null) return;

            var mapData = GroundManager.I.MapData;
            Vector2Int cellPos = WorldToCellIndex(worldPos);

            if (cellPos.x >= 0 && cellPos.x < mapData.columns &&
                cellPos.y >= 0 && cellPos.y < mapData.rows)
            {
                mapTexture.SetPixel(cellPos.x, cellPos.y, color);
                isDirty = true;
            }
        }

        /// <summary>
        /// このフレームで <see cref="SetPixel"/> による変更があった場合のみ、
        /// テクスチャ全体のGPU再アップロード（<see cref="Texture2D.Apply"/>）を1回だけ行う。
        /// 毎フレーム末尾に呼ぶこと。
        /// </summary>
        public void ApplyIfDirty()
        {
            if (!isDirty || mapTexture == null) return;

            mapTexture.Apply();
            isDirty = false;
        }

        private Vector2Int WorldToCellIndex(Vector3 worldPos)
        {
            if (GroundManager.I?.MapData == null) return Vector2Int.zero;

            var mapData = GroundManager.I.MapData;
            Vector3 origin = GroundManager.I.transform.position;

            int x = Mathf.RoundToInt((worldPos.x - origin.x) / mapData.ActualCellWidth);
            int y = Mathf.RoundToInt((worldPos.y - origin.y) / mapData.ActualCellHeight);

            x = Mathf.Clamp(x, 0, mapData.columns - 1);
            y = Mathf.Clamp(y, 0, mapData.rows - 1);

            return new Vector2Int(x, y);
        }
    }
}
