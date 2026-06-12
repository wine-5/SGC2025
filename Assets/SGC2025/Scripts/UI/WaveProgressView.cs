using UnityEngine;
using UnityEngine.UI;

namespace SGC2025.UI
{
    /// <summary>
    /// Waveの進行度を時計回りのリングで描画するView
    /// 進行度の計算はWaveManagerが行い、本クラスは描画のみに専念する
    /// </summary>
    [System.Serializable]
    public class WaveProgressView
    {
        [SerializeField, Tooltip("進行度を表示するリング画像（Filled/Radial360で描画される）")]
        private Image progressImage;

        [Header("発光演出設定")]
        [SerializeField, Tooltip("進行度0のときのリング色（暗め）")]
        private Color dimColor = new(0.1f, 0.5f, 0.1f, 1f);

        [SerializeField, Tooltip("進行度1のときのリング色（明るいほどBloomで発光して見える）")]
        private Color brightColor = new(0.7f, 1f, 0.7f, 1f);

        /// <summary>リング画像をFilled/Radial360構成に初期化する</summary>
        public void Initialize()
        {
            if (progressImage == null) return;

            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Radial360;
            progressImage.fillOrigin = (int)Image.Origin360.Top;
            progressImage.fillClockwise = true;
            progressImage.fillAmount = 0f;
            progressImage.color = dimColor;
        }

        /// <summary>進行度（0.0～1.0）を描画に反映する。進行度が高いほどリングが明るくなる</summary>
        public void SetProgress(float normalizedProgress)
        {
            if (progressImage == null) return;

            float progress = Mathf.Clamp01(normalizedProgress);
            progressImage.fillAmount = progress;
            progressImage.color = Color.Lerp(dimColor, brightColor, progress);
        }
    }
}
