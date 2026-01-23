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
        private const int MAX_POPUP_COUNT = 5;
        private const int INSPECTOR_MAX_POPUP_COUNT = 3;
        private const float BOUNDARY_MARGIN = 100f;
        private const int DEFAULT_POPUP_SIZE_WIDTH = 200;
        private const int DEFAULT_POPUP_SIZE_HEIGHT = 100;
        
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
        [SerializeField] private Vector2 popupSpawnPosition = new Vector2(100, 150);

        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();
        private Color originalScoreColor;
        private Vector3 originalScoreScale;
        private Coroutine currentScoreAnimation;

        private void Awake()
        {
            if (parentCanvas == null)
                parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (popupPrefab == null)
                popupPrefab = Resources.Load<GameObject>("UI/PulsScore");

            if (scoreText != null)
            {
                originalScoreColor = scoreText.color;
                originalScoreScale = scoreText.transform.localScale;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = Instantiate(popupPrefab, parentCanvas);
                var popup = obj.GetComponent<PopupScoreUI>();
                obj.SetActive(false);
                popupPool.Enqueue(popup);
            }
        }

        private void Start()
        {
            InitializeScoreDisplay();
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
            ShowScorePopupAtInspectorPosition(score);
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            UpdateScoreText(points);
            ShowScorePopupAtInspectorPosition(points);
        }

        private void Update() => UpdateTimeText();

        /// <summary>
        /// スコア表示を初期化（0から開始）
        /// </summary>
        private void InitializeScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = "0";
        }

        /// <summary>
        /// 任意座標でスコアポップアップを表示
        /// </summary>
        public void ShowScorePopup(int score, Vector2 position)
        {
            int activeCount = parentCanvas.childCount - popupPool.Count;
            if (activeCount >= MAX_POPUP_COUNT) return;
            
            Vector2 testPosition = new Vector2(200, 100);
            
            PopupScoreUI popup = GetFromPool();
            if (popup == null) 
                return;
            
            popup.Initialize(score, testPosition, ReturnToPool);
            popup.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Inspector設定位置でスコアポップアップを表示
        /// </summary>
        private void ShowScorePopupAtInspectorPosition(int score)
        {
            int activeCount = 0;
            for (int i = 0; i < parentCanvas.childCount; i++)
            {
                var child = parentCanvas.GetChild(i);
                if (child.gameObject.activeSelf && child.GetComponent<PopupScoreUI>() != null)
                    activeCount++;
            }
            
            if (activeCount >= INSPECTOR_MAX_POPUP_COUNT) return;

            Vector2 spawnPosition = GetSpawnPosition();
            
            PopupScoreUI popup = GetFromPool();
            if (popup == null) 
                return;
            
            popup.Initialize(score, spawnPosition, ReturnToPool);
            popup.transform.SetAsLastSibling();
        }

        private Vector2 GetSpawnPosition() => popupSpawnPosition;

        private PopupScoreUI GetFromPool()
        {
            PopupScoreUI popup;
            
            if (popupPool.Count > 0)
            {
                popup = popupPool.Dequeue();
            }
            else
            {
                if (popupPrefab == null) return null;
                
                var obj = Instantiate(popupPrefab, parentCanvas);
                popup = obj.GetComponent<PopupScoreUI>();
                
                if (popup == null)
                {
                    Destroy(obj);
                    return null;
                }
                
                obj.SetActive(false);
            }
            
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
            
            int totalScore = ScoreManager.I.GetTotalScore();
            scoreText.text = totalScore.ToString();
            
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
            timeText.text = GameManager.I.RemainingGameTime.ToString("F1");
        }
    }
}
