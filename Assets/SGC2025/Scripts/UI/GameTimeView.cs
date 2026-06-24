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

        private Color originalColor = Color.white;

        /// <summary>元の文字色を記憶して初期化する</summary>
        public void Initialize()
        {
            if (timeText == null) return;

            originalColor = timeText.color;
        }

        /// <summary>残り時間（秒）を描画に反映する。閾値以下では警告色で点滅する</summary>
        public void SetRemainingTime(float remainingTime)
        {
            if (timeText == null) return;

            timeText.text = remainingTime.ToString("F1");

            if (remainingTime <= warningThreshold)
            {
                float blinkValue = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                timeText.color = Color.Lerp(warningColor, originalColor, blinkValue);
            }
            else if (timeText.color != originalColor)
            {
                timeText.color = originalColor;
            }
        }
    }
}
