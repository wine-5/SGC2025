using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SGC2025.Events;
using SGC2025.Manager;

namespace SGC2025.UI
{
    /// <summary>
    /// インゲーム中のUI表示を管理
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        #region 定数
        private const int MAX_POPUP_COUNT = 5;
        private const int INSPECTOR_MAX_POPUP_COUNT = 3;
        private const float GAUGE_ANIMATION_SPEED = 2f;
        private const string START_TEXT = "開始！";
        private const float START_DISPLAY_DURATION = 0.5f;
        private const float COUNTDOWN_DISPLAY_THRESHOLD = 1f;
        private const int COUNTDOWN_MIN_NUMBER = 1;
        private const int COUNTDOWN_MAX_NUMBER = 3;
        private const float TIME_WARNING_THRESHOLD = 10f;
        private const float TIME_BLINK_SPEED = 3f;
        private const float PERCENT_MULTIPLIER = 100f;
        private const float GAUGE_FILL_THRESHOLD = 0.001f;
        #endregion

        #region シリアライズフィールド
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("カウントダウン設定")]
        [SerializeField, Tooltip("カウントダウン表示用テキスト")]
        private TextMeshProUGUI countdownText;
        [SerializeField, Tooltip("カウントダウンアニメーションの拡大率")]
        private float countdownPulseScale = 1.5f;
        [SerializeField, Tooltip("カウントダウンアニメーションの時間（秒）")]
        private float countdownPulseDuration = 0.3f;
        [SerializeField, Tooltip("カウントダウン色")]
        private Color countdownColor = Color.white;
        [SerializeField, Tooltip("START表示色")]
        private Color startColor = Color.green;

        [Header("領地ゲージ設定")]
        [SerializeField] private Image territoryGaugeImage;
        [SerializeField] private TextMeshProUGUI territoryPercentageText;
        [SerializeField] private Color lowTerritoryColor = new Color(0.6f, 1f, 0.6f);
        [SerializeField] private Color highTerritoryColor = new Color(0.2f, 0.8f, 0.2f);

        [Header("スコアアニメーション設定")]
        [SerializeField] private float scorePulseScale = 1.2f;
        [SerializeField] private float scorePulseDuration = 0.2f;
        [SerializeField] private Color scoreFlashColor = Color.yellow;
        [SerializeField] private Color bigScoreFlashColor = Color.cyan;
        [SerializeField] private int bigScoreThreshold = 1000;
        [SerializeField] private Color scoreBoostColor = new Color(1f, 0.6f, 0f);

        [Header("Waveアニメーション設定")]
        [SerializeField] private float wavePulseScale = 1.3f;
        [SerializeField] private float wavePulseDuration = 0.5f;
        [SerializeField] private Color waveChangeColor = new Color(1f, 0.8f, 0f);

        [Header("スコアポップアップ設定")]
        [SerializeField] private RectTransform parentCanvas;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private Vector2 popupSpawnPosition = new Vector2(100, 150);
        #endregion

        #region プライベートフィールド
        private readonly Queue<PopupScoreUI> popupPool = new Queue<PopupScoreUI>();
        private Color originalScoreColor;
        private Vector3 originalScoreScale;
        private Coroutine currentScoreAnimation;
        private float targetGaugeFillAmount = 0f;
        private TMP_FontAsset startTextFont;
        private TMP_FontAsset numberFont;
        private int lastCountdownNumber = -1;
        private Color originalTimeColor;
        private Color timeWarningColor = Color.red;
        private Color originalWaveColor;
        private Vector3 originalWaveScale;
        #endregion

        #region Unityライフサイクル
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
            
            if (timeText != null)
                originalTimeColor = timeText.color;

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
            InitializeTerritoryGauge();
            InitializeCountdownDisplay();
            InitializeWaveDisplay();
        }

        private void Update()
        {
            UpdateCountdownDisplay();
            UpdateTimeText();
            AnimateTerritoryGauge();
        }

        private void OnEnable()
        {
            EnemyEvents.OnEnemyScoreAdded += OnEnemyDestroyed;
            GroundEvents.OnGreenScoreAdded += OnGroundGreenified;
            SGC2025.Item.ItemManager.OnItemEffectActivated += OnItemEffectActivated;
            SGC2025.Item.ItemManager.OnItemEffectExpired += OnItemEffectExpired;
            WaveManager.OnWaveChanged += OnWaveChanged;
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyScoreAdded -= OnEnemyDestroyed;
            GroundEvents.OnGreenScoreAdded -= OnGroundGreenified;
            SGC2025.Item.ItemManager.OnItemEffectActivated -= OnItemEffectActivated;
            SGC2025.Item.ItemManager.OnItemEffectExpired -= OnItemEffectExpired;
            WaveManager.OnWaveChanged -= OnWaveChanged;
        }
        #endregion

        #region イベントハンドラー

        private void OnEnemyDestroyed(int finalScore, Vector3 position)
        {
            // ScoreManagerで既に倍率適用済みの最終スコアを受け取る
            UpdateScoreText(finalScore);
            ShowScorePopupAtInspectorPosition(finalScore);
        }

        private void OnGroundGreenified(Vector3 position, int finalPoints)
        {
            // ScoreManagerで既に倍率適用済みの最終ポイントを受け取る
            UpdateScoreText(finalPoints);
            ShowScorePopupAtInspectorPosition(finalPoints);
            UpdateTerritoryGauge();
        }

        private void OnItemEffectActivated(SGC2025.Item.ItemType itemType, float effectValue, float duration)
        {
            if (itemType == SGC2025.Item.ItemType.ScoreMultiplier && scoreText != null)
                scoreText.color = scoreBoostColor;
        }

        private void OnItemEffectExpired(SGC2025.Item.ItemType itemType)
        {
            if (itemType == SGC2025.Item.ItemType.ScoreMultiplier && scoreText != null)
                scoreText.color = originalScoreColor;
        }

        private void OnWaveChanged(int newWaveLevel)
        {
            UpdateWaveText(newWaveLevel);
            if (waveText != null)
                StartCoroutine(AnimateWaveChange());
        }
        #endregion

        #region 初期化メソッド
        private void InitializeScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = "0";
        }

        private void InitializeWaveDisplay()
        {
            if (waveText != null && WaveManager.I != null)
            {
                originalWaveColor = waveText.color;
                originalWaveScale = waveText.transform.localScale;
                UpdateWaveText(WaveManager.I.CurrentWaveLevel);
            }
        }

        private void UpdateWaveText(int waveLevel)
        {
            if (waveText != null)
                waveText.text = $"Wave {waveLevel}";
        }

        private IEnumerator AnimateWaveChange()
        {
            if (waveText == null) yield break;

            float elapsed = 0f;
            float halfDuration = wavePulseDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                waveText.transform.localScale = Vector3.Lerp(originalWaveScale, originalWaveScale * wavePulseScale, t);
                waveText.color = Color.Lerp(originalWaveColor, waveChangeColor, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                waveText.transform.localScale = Vector3.Lerp(originalWaveScale * wavePulseScale, originalWaveScale, t);
                waveText.color = Color.Lerp(waveChangeColor, originalWaveColor, t);
                yield return null;
            }

            waveText.transform.localScale = originalWaveScale;
            waveText.color = originalWaveColor;
        }

        private void InitializeTerritoryGauge()
        {
            if (territoryGaugeImage != null)
            {
                territoryGaugeImage.fillAmount = 0f;
                territoryGaugeImage.type = Image.Type.Filled;
                territoryGaugeImage.fillMethod = Image.FillMethod.Radial360;
                territoryGaugeImage.fillOrigin = (int)Image.Origin360.Top;
                territoryGaugeImage.fillClockwise = true;
            }

            UpdateTerritoryGauge();
        }

        private void InitializeCountdownDisplay()
        {
            if (countdownText != null)
            {
                startTextFont = countdownText.font;
                countdownText.gameObject.SetActive(false);
            }
        }
        #endregion

        #region スコアシステム

        /// <summary>
        /// 任意座標でスコアポップアップを表示
        /// </summary>
        public void ShowScorePopup(int score, Vector2 position)
        {
            int activeCount = parentCanvas.childCount - popupPool.Count;
            if (activeCount >= MAX_POPUP_COUNT) return;

            PopupScoreUI popup = GetFromPool();
            if (popup == null) return;

            // スコア倍率中かチェック
            bool isBoostActive = SGC2025.Item.ItemManager.I != null &&
                                 SGC2025.Item.ItemManager.I.IsEffectActive(SGC2025.Item.ItemType.ScoreMultiplier);

            popup.Initialize(score, position, ReturnToPool, isBoostActive);
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
            if (popup == null) return;

            // スコア倍率中かチェック
            bool isBoostActive = SGC2025.Item.ItemManager.I != null &&
                                 SGC2025.Item.ItemManager.I.IsEffectActive(SGC2025.Item.ItemType.ScoreMultiplier);

            popup.Initialize(score, spawnPosition, ReturnToPool, isBoostActive);
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

                // スコア倍率中かチェックして色を決定
                bool isBoostActive = SGC2025.Item.ItemManager.I != null &&
                                     SGC2025.Item.ItemManager.I.IsEffectActive(SGC2025.Item.ItemType.ScoreMultiplier);
                scoreText.color = isBoostActive ? scoreBoostColor : originalScoreColor;
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

                // スコア倍率中かチェックして復帰色を決定
                bool isBoostActive = SGC2025.Item.ItemManager.I != null &&
                                     SGC2025.Item.ItemManager.I.IsEffectActive(SGC2025.Item.ItemType.ScoreMultiplier);
                Color targetColor = isBoostActive ? scoreBoostColor : originalScoreColor;
                scoreText.color = Color.Lerp(flashColor, targetColor, t);

                yield return null;
            }

            // 確実に元に戻す（倍率状態も考慮）
            scoreText.transform.localScale = originalScoreScale;
            bool isFinalBoostActive = SGC2025.Item.ItemManager.I != null &&
                                      SGC2025.Item.ItemManager.I.IsEffectActive(SGC2025.Item.ItemType.ScoreMultiplier);
            scoreText.color = isFinalBoostActive ? scoreBoostColor : originalScoreColor;
            currentScoreAnimation = null;
        }

        private void UpdateTimeText()
        {
            if (timeText == null) return;
            if (InGameManager.I != null)
            {
                float remainingTime = InGameManager.I.RemainingGameTime;
                timeText.text = remainingTime.ToString("F1");
                
                // 残り10秒以内で赤色点滅
                if (remainingTime <= TIME_WARNING_THRESHOLD)
                {
                    float blinkValue = Mathf.PingPong(Time.time * TIME_BLINK_SPEED, 1f);
                    timeText.color = Color.Lerp(timeWarningColor, originalTimeColor, blinkValue);
                }
                else
                {
                    // 通常状態は元の色
                    if (timeText.color != originalTimeColor)
                        timeText.color = originalTimeColor;
                }
            }
        }

        private void UpdateTerritoryGauge()
        {
            if (GroundManager.I == null) return;

            float rate = GroundManager.I.GetGreenificationRate();
            targetGaugeFillAmount = rate;

            if (territoryPercentageText != null)
                territoryPercentageText.text = $"{rate * PERCENT_MULTIPLIER:F1}%";
        }

        private void AnimateTerritoryGauge()
        {
            if (territoryGaugeImage == null) return;

            float currentFill = territoryGaugeImage.fillAmount;
            if (Mathf.Abs(currentFill - targetGaugeFillAmount) > GAUGE_FILL_THRESHOLD)
            {
                territoryGaugeImage.fillAmount = Mathf.Lerp(
                    currentFill,
                    targetGaugeFillAmount,
                    Time.deltaTime * GAUGE_ANIMATION_SPEED
                );

                Color gaugeColor = Color.Lerp(lowTerritoryColor, highTerritoryColor, territoryGaugeImage.fillAmount);
                territoryGaugeImage.color = gaugeColor;
            }
        }

        /// <summary>
        /// カウントダウン表示を更新
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            if (InGameManager.I == null || countdownText == null) return;

            if (InGameManager.I.IsCountingDown)
            {
                if (!countdownText.gameObject.activeSelf)
                    countdownText.gameObject.SetActive(true);

                float timer = InGameManager.I.CountDownTimer;

                if (timer > COUNTDOWN_DISPLAY_THRESHOLD)
                {
                    int countNumber = Mathf.FloorToInt(timer);

                    if (countNumber >= COUNTDOWN_MIN_NUMBER && countNumber <= COUNTDOWN_MAX_NUMBER)
                    {
                        countdownText.text = countNumber.ToString();
                        countdownText.color = countdownColor;
                        
                        // 最初の数字表示時に現在のフォントを数字用として保存
                        if (numberFont == null)
                            numberFont = countdownText.font;
                        
                        // 数字表示時は数字用フォントを使用
                        if (countdownText.font != numberFont && numberFont != null)
                            countdownText.font = numberFont;
                        
                        if (countNumber != lastCountdownNumber)
                        {
                            lastCountdownNumber = countNumber;
                            if (SGC2025.Audio.AudioManager.I != null)
                                SGC2025.Audio.AudioManager.I.PlaySE(SGC2025.Audio.SEType.CountDown);
                        }
                    }
                    else
                    {
                        countdownText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    countdownText.text = START_TEXT;
                    countdownText.color = startColor;
                    
                    // START表示時は元のフォント（START用）に戻す
                    if (countdownText.font != startTextFont && startTextFont != null)
                        countdownText.font = startTextFont;
                    
                    if (lastCountdownNumber != 0)
                    {
                        lastCountdownNumber = 0;
                        if (SGC2025.Audio.AudioManager.I != null)
                            SGC2025.Audio.AudioManager.I.PlaySE(SGC2025.Audio.SEType.CountDown);
                    }
                }

                // パルスアニメーション（シンプルなsin波）
                if (countdownText.gameObject.activeSelf)
                {
                    float normalizedTime = 1f - (timer % 1f);
                    float scale = Mathf.Lerp(1f, countdownPulseScale, Mathf.Sin(normalizedTime * Mathf.PI));
                    countdownText.transform.localScale = Vector3.one * scale;
                }
            }
            else
            {
                // カウントダウン終了後、START!を少し表示してから非表示にする
                if (countdownText.gameObject.activeSelf && countdownText.text == START_TEXT)
                {
                    StartCoroutine(HideCountdownAfterDelay(START_DISPLAY_DURATION));
                    lastCountdownNumber = -1; // リセット
                }
            }
        }

        /// <summary>
        /// 遅延してカウントダウンテキストを非表示にする
        /// </summary>
        private IEnumerator HideCountdownAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (countdownText != null && countdownText.gameObject.activeSelf)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}
#endregion