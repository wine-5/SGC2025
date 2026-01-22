using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SGC2025.Events;

namespace SGC2025 
{
    /// <summary>
    /// インゲーム中のUI表示を管理
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("スコアアニメーション設定")]
        [SerializeField] private float scorePulseScale = 1.2f;
        [SerializeField] private float scorePulseDuration = 0.2f;
        [SerializeField] private Color scoreFlashColor = Color.yellow;
        [SerializeField] private Color bigScoreFlashColor = Color.cyan;
        [SerializeField] private int bigScoreThreshold = 1000;

        [Header("スコアポップアップ設定")]
        [SerializeField] private RectTransform parentCanvas;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private int initialPoolSize = 10;

        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();
        private Color originalScoreColor;
        private Vector3 originalScoreScale;
        private Coroutine currentScoreAnimation;

        private void Awake()
        {
            if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (popupPrefab == null) popupPrefab = Resources.Load<GameObject>("UI/PulsScore");

            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = Instantiate(popupPrefab, parentCanvas);
                var popup = obj.GetComponent<PopupScoreUI>();
                obj.SetActive(false);

            if (scoreText != null)
            {
                originalScoreColor = scoreText.color;
                originalScoreScale = scoreText.transform.localScale;
            }
                popupPool.Enqueue(popup);
            }
        }

        private void OnEnable()
        {
            EnemyEvents.OnEnemyDestroyedWithScore += OnEnemyDestroyed;
            GroundEvents.OnGroundGreenified += OnGroundGreenified;
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyDestroyedWithScore -= OnEnemyDestroyed;
            GroundEvents.OnGroundGreenified -= OnGroundGreenified;
        }

        private void OnEnemyDestroyed(int score, Vector3 position)
        {
            UpdateScoreText(score);
            ShowScorePopup(score, Camera.main.WorldToScreenPoint(position));
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            UpdateScoreText(points);
            ShowScorePopup(points, Camera.main.WorldToScreenPoint(position));
        }

        private void Update()
        {
            UpdateTimeText();
        }

        /// <summary>
        /// スコアポップアップを表示（中央付近）
        /// </summary>
        public void ShowScorePopup(int score)
        {
            Vector2 basePos = new Vector2(0, 100);
            ShowScorePopup(score, basePos);
        }

        /// <summary>
        /// 任意座標でスコアポップアップを表示
        /// </summary>
        public void ShowScorePopup(int score, Vector2 position)
        {
            PopupScoreUI popup = GetFromPool();
            popup.Initialize(score, position, ReturnToPool);
        }

        private PopupScoreUI GetFromPool()
        {
            if (popupPool.Count > 0) return popupPool.Dequeue();
            var obj = Instantiate(popupPrefab, parentCanvas);
            var popup = obj.GetComponent<PopupScoreUI>();
            obj.SetActive(false);
            return popup;
        }

        private void ReturnToPool(PopupScoreUI popup)
        {
            popup.gameObject.SetActive(false);
            popupPool.Enqueue(popup);
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText == null) return;
            scoreText.text = ScoreManager.I.GetTotalScore().ToString();
            
            // 既存のアニメーションを停止して即座に新しいものを開始
            if (currentScoreAnimation != null)
            {
                StopCoroutine(currentScoreAnimation);
                scoreText.transform.localScale = originalScoreScale;
                scoreText.color = originalScoreColor;
            }
            
            Color flashColor = score >= bigScoreThreshold ? bigScoreFlashColor : scoreFlashColor;
            currentScoreAnimation = StartCoroutine(ScoreUpdateAnimation(flashColor));
        }

        private IEnumerator ScoreUpdateAnimation(Color flashColor)
        {
            if (scoreText == null) yield break;

            float elapsed = 0f;
            float halfDuration = scorePulseDuration * 0.5f;

            // スケールアップ + カラーフラッシュ
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                scoreText.transform.localScale = Vector3.Lerp(originalScoreScale, originalScoreScale * scorePulseScale, t);
                scoreText.color = Color.Lerp(originalScoreColor, flashColor, t);
                
                yield return null;
            }

            elapsed = 0f;

            // スケールダウン + カラー復帰
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                scoreText.transform.localScale = Vector3.Lerp(originalScoreScale * scorePulseScale, originalScoreScale, t);
                scoreText.color = Color.Lerp(flashColor, originalScoreColor, t);
                
                yield return null;
            }

            // 確実に元に戻す
            scoreText.transform.localScale = originalScoreScale;
            scoreText.color = originalScoreColor;
            currentScoreAnimation = null;
        }

        private void UpdateTimeText()
        {
            if (timeText == null) return;
            timeText.text = ScoreManager.I.GetGameCount().ToString("F1");
        }
    }
}
