using UnityEngine;
using TMPro;

namespace SGC2025.UI
{
    /// <summary>
    /// スコアポップアップUIの表示とアニメーション管理
    /// </summary>
    public class PopupScoreUI : MonoBehaviour
    {
        [Header("アニメーション設定")]
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private float floatSpeed = 60f;
        
        [Header("テキスト設定")]
        [SerializeField] private float fontSize = 48f;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color boostedColor = new Color(1f, 0.6f, 0f); // オレンジ
        
        private RectTransform rect;
        private float timer = 0f;
        private Vector2 startPos;
        private System.Action<PopupScoreUI> onComplete;
        private bool isBoostActive = false;

        /// <summary>
        /// ポップアップを初期化
        /// </summary>
        public void Initialize(int score, Vector2 position, System.Action<PopupScoreUI> onComplete, bool isBoostActive = false)
        {
            if (text == null)
                text = GetComponent<TextMeshProUGUI>();
            if (rect == null)
                rect = GetComponent<RectTransform>();

            this.isBoostActive = isBoostActive;
            
            text.text = $"+{score}";
            text.fontSize = isBoostActive ? fontSize * 1.2f : fontSize;
            
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 100);
            
            startPos = position;
            timer = 0f;
            this.onComplete = onComplete;
            
            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            
            rect.anchoredPosition = startPos + Vector2.up * floatSpeed * timer;

            var color = isBoostActive ? boostedColor : normalColor;
            color.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            text.color = color;

            if (timer >= lifetime)
            {
                gameObject.SetActive(false);
                onComplete?.Invoke(this);
            }
        }
    }
}