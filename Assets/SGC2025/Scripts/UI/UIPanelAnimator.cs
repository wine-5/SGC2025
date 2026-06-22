using System.Collections;
using UnityEngine;

namespace SGC2025.UI
{
    /// <summary>
    /// パネル切替時のフェードイン＋スケールのポップ演出を共通化するヘルパー。
    /// セレクト・ランキング・ルール説明の各切替で同じ演出を使い回すために用いる。
    /// </summary>
    public static class UIPanelAnimator
    {
        public const float DefaultFadeDuration = 0.18f; // フェードイン時間（秒）
        public const float DefaultStartScale = 0.92f;   // 出現開始時のスケール倍率

        /// <summary>
        /// 対象GameObjectにフェードイン＋スケールのポップ演出を再生する。
        /// 呼び出し側のMonoBehaviourから StartCoroutine で実行する。
        /// </summary>
        public static IEnumerator PlayShow(GameObject target, float fadeDuration, float startScale)
        {
            if (target == null) yield break;

            CanvasGroup group = GetCanvasGroup(target);
            Transform tf = target.transform;
            float elapsed = 0f;

            group.alpha = 0f;
            tf.localScale = Vector3.one * startScale;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float eased = 1f - (1f - t) * (1f - t); // EaseOutQuad

                group.alpha = eased;
                tf.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);
                yield return null;
            }

            Reset(target);
        }

        /// <summary>
        /// 対象の見た目を通常状態（不透明・等倍）へ戻す。
        /// </summary>
        public static void Reset(GameObject target)
        {
            if (target == null) return;
            GetCanvasGroup(target).alpha = 1f;
            target.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 対象のCanvasGroupを取得（無ければ追加する）。
        /// </summary>
        public static CanvasGroup GetCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null) group = target.AddComponent<CanvasGroup>();
            return group;
        }
    }
}
