using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tyotyo.UI
{
    /// <summary>
    /// Wave表示（進行度リング＋ウェーブテキスト）を描画するView
    /// レベルや進行度の計算はWaveManagerが行い、本クラスは描画のみに専念する
    /// </summary>
    [System.Serializable]
    public class WaveView
    {
        [Header("進行度リング")]
        [SerializeField, Tooltip("進行度を表示するリング画像（Filled/Radial360で描画される）")]
        private Image progressImage;

        [SerializeField, Tooltip("進行度0のときのリング色（暗め）")]
        private Color dimColor = new(0.1f, 0.5f, 0.1f, 1f);

        [SerializeField, Tooltip("進行度1のときのリング色（明るいほどBloomで発光して見える）")]
        private Color brightColor = new(0.7f, 1f, 0.7f, 1f);

        [Header("ウェーブテキスト")]
        [SerializeField, Tooltip("ウェーブ番号を表示するテキスト")]
        private TextMeshProUGUI waveText;

        [SerializeField, Tooltip("Wave1時のテキスト色")]
        private Color levelLowColor = Color.white;

        [SerializeField, Tooltip("最大レベル時のテキスト色")]
        private Color levelHighColor = Color.red;

        [SerializeField, Tooltip("このWaveレベルでlevelHighColorに到達する")]
        private int levelColorMaxLevel = 6;

        [Header("Wave切替演出")]
        [SerializeField, Tooltip("切替時のテキスト拡大率")]
        private float pulseScale = 1.3f;

        [SerializeField, Tooltip("切替演出の時間（秒）")]
        private float pulseDuration = 0.5f;

        [SerializeField, Tooltip("切替時のフラッシュ色")]
        private Color pulseColor = new(1f, 0.8f, 0f);

        private Color currentLevelColor = Color.white;
        private Vector3 originalTextScale = Vector3.one;

        /// <summary>リング画像とテキストを初期化する</summary>
        public void Initialize()
        {
            if (progressImage != null)
            {
                progressImage.type = Image.Type.Filled;
                progressImage.fillMethod = Image.FillMethod.Radial360;
                progressImage.fillOrigin = (int)Image.Origin360.Top;
                progressImage.fillClockwise = true;
                progressImage.fillAmount = 0f;
                progressImage.color = dimColor;
            }

            if (waveText != null)
                originalTextScale = waveText.transform.localScale;
        }

        /// <summary>進行度（0.0～1.0）を描画に反映する。進行度が高いほどリングが明るくなる</summary>
        public void SetProgress(float normalizedProgress)
        {
            if (progressImage == null) return;

            float progress = Mathf.Clamp01(normalizedProgress);
            progressImage.fillAmount = progress;
            progressImage.color = Color.Lerp(dimColor, brightColor, progress);
        }

        /// <summary>ウェーブ番号と、レベルに応じたテキスト色を反映する</summary>
        public void SetWaveLevel(int waveLevel)
        {
            if (waveText == null) return;

            waveText.text = $"ウェーブ {waveLevel}";

            float t = Mathf.InverseLerp(1f, levelColorMaxLevel, waveLevel);
            currentLevelColor = Color.Lerp(levelLowColor, levelHighColor, t);
            waveText.color = currentLevelColor;
        }

        /// <summary>Wave切替時のパルス演出（呼び出し側でStartCoroutineする）</summary>
        public IEnumerator AnimateWaveChange()
        {
            if (waveText == null) yield break;

            float halfDuration = pulseDuration * 0.5f;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                waveText.transform.localScale = Vector3.Lerp(originalTextScale, originalTextScale * pulseScale, t);
                waveText.color = Color.Lerp(currentLevelColor, pulseColor, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                waveText.transform.localScale = Vector3.Lerp(originalTextScale * pulseScale, originalTextScale, t);
                waveText.color = Color.Lerp(pulseColor, currentLevelColor, t);
                yield return null;
            }

            waveText.transform.localScale = originalTextScale;
            waveText.color = currentLevelColor;
        }
    }
}
