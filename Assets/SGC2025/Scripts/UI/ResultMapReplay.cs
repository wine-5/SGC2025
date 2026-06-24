using UnityEngine;
using UnityEngine.UI;
using Tyotyo.Manager;

namespace Tyotyo.UI
{
    /// <summary>
    /// リザルト画面で、InGameの塗り順を背景ミニマップとして再生するコンポーネント。
    /// GameManager に記録された緑化セルの順序を、進捗(0〜1)に応じて緑に塗っていく。
    /// 進捗はResultUIのカウントアップと同期させて呼び出す。
    /// </summary>
    public class ResultMapReplay : MonoBehaviour
    {
        [SerializeField] private RawImage mapImage;
        [SerializeField] private Color brownColor = new(0.4f, 0.25f, 0.1f);
        [SerializeField] private Color greenColor = new(0.3f, 0.7f, 0.2f);

        private Texture2D mapTexture;
        private int paintedCount; // すでに塗ったセル数
        private int totalCount;   // 塗る予定の総セル数（記録された順序数）

        /// <summary>
        /// 背景マップを全マス茶色で初期化する。ResultUIのStartから一度だけ呼ぶ。
        /// </summary>
        public void Initialize()
        {
            if (GameManager.I == null) return;

            int columns = GameManager.I.MapColumns;
            int rows = GameManager.I.MapRows;
            if (columns <= 0 || rows <= 0) return;

            mapTexture = new Texture2D(columns, rows, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            // 全マスを茶色で塗りつぶす
            Color[] fill = new Color[columns * rows];
            for (int i = 0; i < fill.Length; i++)
                fill[i] = brownColor;
            mapTexture.SetPixels(fill);
            mapTexture.Apply();

            if (mapImage != null)
                mapImage.texture = mapTexture;

            paintedCount = 0;
            totalCount = GameManager.I.GreenifiedSequence.Count;
        }

        /// <summary>
        /// 進捗(0〜1)に応じて、記録された順に緑化セルを塗る。
        /// 進んだ分だけ追加で塗り、Applyはこの呼び出しごとに1回だけ行う。
        /// </summary>
        public void SetProgress(float t)
        {
            if (mapTexture == null) return;

            int target = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(t) * totalCount), 0, totalCount);
            if (target <= paintedCount) return;

            var sequence = GameManager.I.GreenifiedSequence;
            for (int i = paintedCount; i < target; i++)
            {
                Vector2Int cell = sequence[i];
                if (cell.x >= 0 && cell.x < mapTexture.width && cell.y >= 0 && cell.y < mapTexture.height)
                    mapTexture.SetPixel(cell.x, cell.y, greenColor);
            }

            mapTexture.Apply();
            paintedCount = target;
        }
    }
}
