using TMPro;
using UnityEngine;

namespace Tyotyo.UI
{
    /// <summary>
    /// 残り時間テキストを描画するView
    /// 残り時間の取得はInGameUIが行い、本クラスは描画のみに専念する
    /// </summary>
    [System.Serializable]
    public class GameTimeView
    {
        [SerializeField, Tooltip("残り時間を表示するテキスト")]
        private TextMeshProUGUI timeText;

        [SerializeField, Tooltip("この残り秒数以下で警告点滅を開始する")]
        private float warningThreshold = 10f;

        [SerializeField, Tooltip("警告点滅の速さ")]
        private float blinkSpeed = 3f;

        [SerializeField, Tooltip("警告時の色")]
        private Color warningColor = Color.red;

        [Header("スケーリング設定")]
        [SerializeField, Tooltip("スケーリング機能を有効にするか")]
        private bool enableScaling = true;

        [SerializeField, Tooltip("最小スケール（縮小時）")]
        private float minScale = 1.0f;

        [SerializeField, Tooltip("最大スケール（拡大時）")]
        private float maxScale = 1.3f;

        [SerializeField, Tooltip("拡大縮小の周期（秒。0.5なら1秒で拡大→縮小が1回完了）")]
        private float pulseCycle = 0.5f;

        private Color originalColor = Color.white;
        private Vector3 originalScale = Vector3.one;
        private RectTransform cachedRectTransform;

        // 直近に表示した残り時間（1/10秒単位）。値が変わった時だけToStringして文字列確保を避ける
        private int lastDisplayedTenths = int.MinValue;

        /// <summary>元の文字色とスケールを記憶して初期化する</summary>
        public void Initialize()
        {
            if (timeText == null) return;

            cachedRectTransform = timeText.gameObject.GetComponent<RectTransform>();
            originalColor = timeText.color;
            originalScale = cachedRectTransform != null ? cachedRectTransform.localScale : Vector3.one;
            lastDisplayedTenths = int.MinValue;
        }

        /// <summary>残り時間（秒）を描画に反映する。閾値以下では警告色で点滅し、スケールも変更する</summary>
        public void SetRemainingTime(float remainingTime)
        {
            if (timeText == null) return;

            int tenths = Mathf.RoundToInt(remainingTime * 10f);
            if (tenths != lastDisplayedTenths)
            {
                lastDisplayedTenths = tenths;
                timeText.text = remainingTime.ToString("F1");
            }

            if (remainingTime <= warningThreshold)
            {
                float blinkValue = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                timeText.color = Color.Lerp(warningColor, originalColor, blinkValue);

                // スケーリング処理：周期的にスケールを変更
                if (enableScaling)
                    ApplyTimeBasedScaling();
            }
            else
            {
                if (timeText.color != originalColor)
                    timeText.color = originalColor;

                // 警告終了時はスケールを元に戻す
                if (enableScaling && cachedRectTransform != null)
                    cachedRectTransform.localScale = originalScale;
            }
        }

        /// <summary>周期的に拡大縮小するテキストスケールを計算・適用</summary>
        private void ApplyTimeBasedScaling()
        {
            if (cachedRectTransform == null) return;

            // PingPong で 0 -> 1 -> 0 を繰り返す（周期 = pulseCycle）
            float pulseValue = Mathf.PingPong(Time.time / pulseCycle, 1f);
            float scale = Mathf.Lerp(minScale, maxScale, pulseValue);

            Vector3 scaledSize = originalScale * scale;
            cachedRectTransform.localScale = scaledSize;
        }
    }
}
