using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SGC2025.Events;
using UnityEditor;

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
        [SerializeField] private Vector2 popupSpawnPosition = new Vector2(100, 150); // Inspectorで調整可能

        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();
        private Color originalScoreColor;
        private Vector3 originalScoreScale;
        private Coroutine currentScoreAnimation;

        private void Awake()
        {
            if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (popupPrefab == null) popupPrefab = Resources.Load<GameObject>("UI/PulsScore");

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
            Debug.Log($"[InGameUI] OnEnemyDestroyed: score={score}, position={position}");
            UpdateScoreText(score);
            ShowScorePopupAtInspectorPosition(score);
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            Debug.Log($"[InGameUI] OnGroundGreenified: points={points}, position={position}");
            UpdateScoreText(points);
            ShowScorePopupAtInspectorPosition(points);
        }

        /// <summary>ワールド座標をキャンバス座標に変換</summary>
        private Vector2 WorldToCanvasPoint(Vector3 worldPosition)
        {
            if (Camera.main == null || parentCanvas == null)
            {
                Debug.LogError("[InGameUI] Camera.main or parentCanvas is null");
                return Vector2.zero;
            }
            
            // ワールド座標をスクリーン座標に変換
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Canvas上の座標に変換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas, screenPoint, Camera.main, out Vector2 canvasPoint);
            
            // Canvas座標系の範囲内にクランプ（画面中央寄りの範囲を使用）
            var canvasRect = parentCanvas.rect;
            float margin = 100f; // 画面端からのマージン
            canvasPoint.x = Mathf.Clamp(canvasPoint.x, canvasRect.xMin + margin, canvasRect.xMax - margin);
            canvasPoint.y = Mathf.Clamp(canvasPoint.y, canvasRect.yMin + margin, canvasRect.yMax - margin);
                
            Debug.Log($"[InGameUI] WorldToCanvasPoint: world={worldPosition} -> screen={screenPoint} -> canvas={canvasPoint}");
            Debug.Log($"[InGameUI] Canvas rect: {canvasRect}, clamped range: x({canvasRect.xMin + margin} to {canvasRect.xMax - margin}), y({canvasRect.yMin + margin} to {canvasRect.yMax - margin})");
            return canvasPoint;
        }

        private void Update()
        {
            UpdateTimeText();
        }

        /// <summary>
        /// スコア表示を初期化（0から開始）
        /// </summary>
        private void InitializeScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = "0"; // 明示的に0から開始
                Debug.Log("[InGameUI] Score display initialized to 0");
            }
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
            // パフォーマンス保護：アクティブなポップアップ数を制限
            int activeCount = parentCanvas.childCount - popupPool.Count;
            if (activeCount >= 5) // 最大5個まで同時表示
            {
                Debug.Log($"[InGameUI] Skipping popup - too many active ({activeCount})");
                return;
            }
            
            Debug.Log($"[InGameUI] ShowScorePopup: score={score}, active={activeCount}");
            
            // 右上に移動して確実に見える位置でテスト
            Vector2 testPosition = new Vector2(200, 100); // 右上寄り
            
            PopupScoreUI popup = GetFromPool();
            if (popup == null)
            {
                Debug.LogError("[InGameUI] Failed to get popup from pool");
                return;
            }
            
            // テスト位置で初期化
            popup.Initialize(score, testPosition, ReturnToPool);
            popup.transform.SetAsLastSibling(); // UIで最前面に
            
            Debug.Log($"[InGameUI] Popup created at test position: {testPosition}, active: {popup.gameObject.activeSelf}");
        }

        /// <summary>
        /// Inspector設定位置でスコアポップアップを表示
        /// </summary>
        private void ShowScorePopupAtInspectorPosition(int score)
        {
            // パフォーマンス保護：アクティブなポップアップ数を制限
            // アクティブなポップアップ数を直接カウント
            int activeCount = 0;
            for (int i = 0; i < parentCanvas.childCount; i++)
            {
                var child = parentCanvas.GetChild(i);
                if (child.gameObject.activeSelf && child.GetComponent<PopupScoreUI>() != null)
                {
                    activeCount++;
                }
            }
            
            if (activeCount >= 3) // 一時的に3個までに緊和
            {
                Debug.Log($"[InGameUI] Skipping popup - too many active ({activeCount})");
                return;
            }

            // Inspector設定位置を取得
            Vector2 spawnPosition = GetSpawnPosition();
            Debug.Log($"[InGameUI] ShowScorePopupAtInspectorPosition: score={score}, position={spawnPosition}, activeCount={activeCount}");
            
            PopupScoreUI popup = GetFromPool();
            if (popup == null)
            {
                Debug.LogError("[InGameUI] Failed to get popup from pool");
                return;
            }
            
            // Inspector設定位置で初期化
            popup.Initialize(score, spawnPosition, ReturnToPool);
            popup.transform.SetAsLastSibling(); // UIで最前面に
            
            Debug.Log($"[InGameUI] Popup created at inspector position: {spawnPosition}, active: {popup.gameObject.activeSelf}");
        }

        /// <summary>
        /// Inspector設定のポップアップ位置を取得
        /// </summary>
        private Vector2 GetSpawnPosition()
        {
            // Inspectorで設定した位置を使用
            return popupSpawnPosition;
        }

        private PopupScoreUI GetFromPool()
        {
            PopupScoreUI popup;
            
            if (popupPool.Count > 0)
            {
                popup = popupPool.Dequeue();
                Debug.Log($"[InGameUI] Got popup from pool, remaining: {popupPool.Count}");
            }
            else
            {
                if (popupPrefab == null)
                {
                    Debug.LogError("[InGameUI] popupPrefab is null!");
                    return null;
                }
                
                var obj = Instantiate(popupPrefab, parentCanvas);
                popup = obj.GetComponent<PopupScoreUI>();
                
                if (popup == null)
                {
                    Debug.LogError("[InGameUI] PopupScoreUI component not found on prefab!");
                    Destroy(obj);
                    return null;
                }
                
                obj.SetActive(false);
                Debug.Log($"[InGameUI] Created new popup from prefab (simple)");
            }
            
            return popup;
        }

        private void ReturnToPool(PopupScoreUI popup)
        {
            popup.gameObject.SetActive(false);
            popupPool.Enqueue(popup);
            Debug.Log($"[InGameUI] Popup returned to pool, pool size: {popupPool.Count}");
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText == null) return;
            
            // スコア表示を通常の数字表示に変更（0000形式ではなく、0から始まる普通の数字）
            int totalScore = ScoreManager.I.GetTotalScore();
            scoreText.text = totalScore.ToString();
            
            Debug.Log($"[InGameUI] Score updated: {totalScore}");
            
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
            timeText.text = GameManager.I.RemainingGameTime.ToString("F1");
        }
    }
}
