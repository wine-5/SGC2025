using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SGC2025.Core;
using SGC2025.Manager;

namespace SGC2025.UI
{
    /// <summary>
    /// インゲーム中のUI表示を管理
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        #region 定数
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
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("カウントダウン設定")]
        [SerializeField, Tooltip("カウントダウン表示用テキスト")]
        private TextMeshProUGUI countdownText;
        [SerializeField, Tooltip("カウントダウンアニメーションの拡大率")]
        private float countdownPulseScale = 1.5f;
        [SerializeField, Tooltip("カウントダウン色")]
        private Color countdownColor = Color.white;
        [SerializeField, Tooltip("START表示色")]
        private Color startColor = Color.green;

        [Header("領地ゲージ設定")]
        [SerializeField] private Image territoryGaugeImage;
        [SerializeField] private TextMeshProUGUI territoryPercentageText;
        [SerializeField] private Color lowTerritoryColor = new Color(0.6f, 1f, 0.6f);
        [SerializeField] private Color highTerritoryColor = new Color(0.2f, 0.8f, 0.2f);


        [Header("Waveアニメーション設定")]
        [SerializeField] private float wavePulseScale = 1.3f;
        [SerializeField] private float wavePulseDuration = 0.5f;
        [SerializeField] private Color waveChangeColor = new Color(1f, 0.8f, 0f);
        #endregion

        #region プライベートフィールド
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
            if (timeText != null)
                originalTimeColor = timeText.color;
        }

        private void Start()
        {
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
            EventBus.Subscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Subscribe<WaveChangedEvent>(OnWaveChangedEvent);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Unsubscribe<WaveChangedEvent>(OnWaveChangedEvent);
        }
        #endregion

        #region イベントハンドラー

        private void OnGroundGreenified(GroundGreenifiedEvent e)
        {
            UpdateTerritoryGauge();
        }

        private void OnWaveChangedEvent(WaveChangedEvent e)
        {
            UpdateWaveText(e.WaveLevel);
            if (waveText != null)
                StartCoroutine(AnimateWaveChange());
        }
        #endregion

        #region 初期化メソッド
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
                waveText.text = $"ウェーブ {waveLevel}";
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
