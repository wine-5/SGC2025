using UnityEngine;
using UnityEngine.EventSystems;

namespace SGC2025.UI
{
    /// <summary>
    /// ボタンが選択／ホバーされている間だけScaleを変更する（拡大・縮小）
    /// </summary>
    public class ButtonScaleEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField]
        private float selectedScale = 1.1f; // 選択／ホバー中の倍率
        [SerializeField]
        private float scaleSpeed = 12f;     // 補間速度（大きいほど速い）

        private Vector3 baseScale;
        private float targetScale = 1f;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            targetScale = 1f;
            transform.localScale = baseScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                baseScale * targetScale,
                Time.unscaledDeltaTime * scaleSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData) => targetScale = selectedScale;
        public void OnPointerExit(PointerEventData eventData) => targetScale = 1f;
        public void OnSelect(BaseEventData eventData) => targetScale = selectedScale;
        public void OnDeselect(BaseEventData eventData) => targetScale = 1f;
    }
}
