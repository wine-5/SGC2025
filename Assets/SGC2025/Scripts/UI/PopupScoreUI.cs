using UnityEngine;
using TMPro;

namespace SGC2025
{
    /// <summary>
    /// スコアポップアップUIの表示とアニメーション管理
    /// </summary>
    public class PopupScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        private RectTransform rect;
        private float lifetime = 0.5f; // 0.5秒に短縮してパフォーマンス改善
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

            // シンプルな基本設定
            text.text = $"+{score}";
            text.color = Color.white;  // 白色でシンプルに
            text.fontSize = 48f;       // 適度なサイズ
            
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 100);
            
            startPos = position;
            timer = 0f;
            this.onComplete = onComplete;
            
            gameObject.SetActive(true);
            
            Debug.Log($"[PopupScoreUI] Initialize: text='{text.text}', position={position}, lifetime={lifetime}s");
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            
            // 上に浮き上がる動作
            rect.anchoredPosition = startPos + Vector2.up * floatSpeed * timer;

            // アルファ値をフェードアウト（白色を保持）
            var color = Color.white;
            color.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            text.color = color;

            // 寿命終了で非アクティブ化
            if (timer >= lifetime)
            {
                Debug.Log($"[PopupScoreUI] Lifetime ended: {timer:F2}s/{lifetime}s - calling onComplete");
                gameObject.SetActive(false);
                onComplete?.Invoke(this);
            }
        }
    }
}
