using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SGC2025.UI
{
    /// <summary>
    /// 緑化度ゲージ（リング・パーセント表示・ラベル）を描画するView
    /// 緑化率の取得やマイルストーン判定はInGameUIが行い、本クラスは描画のみに専念する
    /// </summary>
    [System.Serializable]
    public class TerritoryGaugeView
    {
        private const float GAUGE_ANIMATION_SPEED = 2f;
        private const float GAUGE_FILL_THRESHOLD = 0.001f;
        private const float PERCENT_MULTIPLIER = 100f;

        [SerializeField, Tooltip("緑化度を表示するリング画像（Filled/Radial360で描画される）")]
        private Image gaugeImage;

        [SerializeField, Tooltip("パーセント数値テキスト")]
        private TextMeshProUGUI percentageText;

        [SerializeField, Tooltip("「緑化度」ラベルテキスト（緑化率に応じて変色）")]
        private TextMeshProUGUI labelText;

        [Header("色設定")]
        [SerializeField, Tooltip("緑化率が低いときのリング色")]
        private Color lowColor = new(0.6f, 1f, 0.6f);

        [SerializeField, Tooltip("緑化率が高いときのリング色")]
        private Color highColor = new(0.2f, 0.8f, 0.2f);

        [SerializeField, Tooltip("緑化率0%時のラベル色")]
        private Color labelLowColor = Color.white;

        [SerializeField, Tooltip("緑化率100%時のラベル色")]
        private Color labelHighColor = new(0.2f, 0.9f, 0.2f);

        [Header("マイルストーン演出")]
        [SerializeField, Tooltip("10%達成ごとのパルス拡大率")]
        private float pulseScale = 1.3f;

        [SerializeField, Tooltip("パルス演出の時間（秒）")]
        private float pulseDuration = 0.4f;

        private float targetFillAmount = 0f;
        private bool isPulsing = false;
        private Vector3 originalScale = Vector3.one;

        /// <summary>リング画像をFilled/Radial360構成に初期化する</summary>
        public void Initialize()
        {
            if (gaugeImage == null) return;

            gaugeImage.fillAmount = 0f;
            gaugeImage.type = Image.Type.Filled;
            gaugeImage.fillMethod = Image.FillMethod.Radial360;
            gaugeImage.fillOrigin = (int)Image.Origin360.Top;
            gaugeImage.fillClockwise = true;
            originalScale = gaugeImage.transform.localScale;
        }

        /// <summary>緑化率（0.0～1.0）を描画に反映する</summary>
        public void SetRate(float rate)
        {
            targetFillAmount = rate;

            if (percentageText != null)
                percentageText.text = $"{rate * PERCENT_MULTIPLIER:F1}%";

            if (labelText != null)
                labelText.color = Color.Lerp(labelLowColor, labelHighColor, rate);
        }

        /// <summary>毎フレーム呼び出し、ゲージの滑らかなアニメーションを進める</summary>
        public void Tick(float deltaTime)
        {
            if (gaugeImage == null) return;

            float currentFill = gaugeImage.fillAmount;
            if (Mathf.Abs(currentFill - targetFillAmount) <= GAUGE_FILL_THRESHOLD) return;

            gaugeImage.fillAmount = Mathf.Lerp(currentFill, targetFillAmount, deltaTime * GAUGE_ANIMATION_SPEED);

            // パルス演出中は色の制御をAnimateMilestonePulseに任せる
            if (!isPulsing)
                gaugeImage.color = Color.Lerp(lowColor, highColor, gaugeImage.fillAmount);
        }

        /// <summary>ゲージを一瞬拡大し白くフラッシュさせるパルス演出（呼び出し側でStartCoroutineする）</summary>
        public IEnumerator AnimateMilestonePulse()
        {
            if (gaugeImage == null || isPulsing) yield break;

            isPulsing = true;
            Transform gaugeTransform = gaugeImage.transform;
            Color baseColor = Color.Lerp(lowColor, highColor, targetFillAmount);
            float halfDuration = pulseDuration * 0.5f;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                gaugeTransform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
                gaugeImage.color = Color.Lerp(baseColor, Color.white, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                gaugeTransform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, t);
                gaugeImage.color = Color.Lerp(Color.white, baseColor, t);
                yield return null;
            }

            gaugeTransform.localScale = originalScale;
            gaugeImage.color = baseColor;
            isPulsing = false;
        }
    }
}
