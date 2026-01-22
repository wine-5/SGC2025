using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace SGC2025 
{
    /// <summary>
    /// インゲーム中のUI表示を管理
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        private const int DEFAULT_POOL_SIZE = 10;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        public static InGameUI Instance;

        [Header("スコアポップアップ設定")]
        [SerializeField] private RectTransform parentCanvas;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private int initialPoolSize = 10;

        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();

        private void Awake()
        {
            Instance = this;
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
        private void Update()
        {
            UpdateScoreText();
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
            scoreText.text = (CommonDef.currentEnemyScore + CommonDef.currentGreeningScore).ToString();
        }

        private void UpdateTimeText()
        {
            if (timeText == null) return;
            timeText.text = ScoreManager.I.GetGameCount().ToString("F1");
        }
    }
}
