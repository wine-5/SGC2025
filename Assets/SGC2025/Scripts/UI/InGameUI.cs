using UnityEngine;
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

        [Header("スコアポップアップ設定")]
        [SerializeField] private RectTransform parentCanvas;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private int initialPoolSize = 10;

        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();

        private void Awake()
        {
            if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (popupPrefab == null) popupPrefab = Resources.Load<GameObject>("UI/PulsScore");

            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = Instantiate(popupPrefab, parentCanvas);
                var popup = obj.GetComponent<PopupScoreUI>();
                obj.SetActive(false);
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
            UpdateScoreText();
            ShowScorePopup(score, Camera.main.WorldToScreenPoint(position));
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            UpdateScoreText();
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

        private void UpdateScoreText()
        {
            if (scoreText == null) return;
            scoreText.text = ScoreManager.I.GetTotalScore().ToString();
        }

        private void UpdateTimeText()
        {
            if (timeText == null) return;
            timeText.text = ScoreManager.I.GetGameCount().ToString("F1");
        }
    }
}
