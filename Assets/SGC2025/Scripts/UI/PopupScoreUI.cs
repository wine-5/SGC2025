using UnityEngine;
using TMPro;

namespace SGC2025
{
    public class PopupScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        private RectTransform rect;
        private float lifetime = 1.0f;
        private float floatSpeed = 60f;
        private float timer = 0f;
        private Vector2 startPos;
        private System.Action<PopupScoreUI> onComplete;

        public void Initialize(int score, Vector2 position, System.Action<PopupScoreUI> onComplete)
        {
            if (text == null)
                text = GetComponent<TextMeshProUGUI>();
            if (rect == null)
                rect = GetComponent<RectTransform>();

            text.text = $"+{score}";
            rect.anchoredPosition = position;
            startPos = position;
            timer = 0f;

            this.onComplete = onComplete;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;

            // 上方向に移動
            rect.anchoredPosition = startPos + Vector2.up * floatSpeed * timer;

            // フェードアウト
            var color = text.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            text.color = color;

            if (timer > lifetime)
            {
                gameObject.SetActive(false);
                onComplete?.Invoke(this); // プールに返却
            }
        }

    }
}
