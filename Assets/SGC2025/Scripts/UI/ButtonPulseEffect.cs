using UnityEngine;

namespace Tyotyo.UI
{
    /// <summary>
    /// 対象を常に拡大・縮小させ続けて注目を集める演出（パルス）
    /// </summary>
    public class ButtonPulseEffect : MonoBehaviour
    {
        [SerializeField]
        private float minScale = 0.95f;  // 最小倍率
        [SerializeField]
        private float maxScale = 1.05f;  // 最大倍率
        [SerializeField]
        private float pulseSpeed = 3f;   // 拡大縮小の速さ

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            transform.localScale = baseScale;
        }

        private void Update()
        {
            // 0〜1を往復するサイン波からminScale〜maxScaleの倍率を求める
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = baseScale * scale;
        }
    }
}
