using System.Collections;
using TMPro;
using UnityEngine;
using Tyotyo.Core;
using Tyotyo.Manager;

namespace Tyotyo.UI
{
    /// <summary>
    /// インゲーム中のUI表示を管理
    /// Managerから値を取得して各Viewへ渡す（描画の詳細は各Viewが担当）
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        #region 定数
        private const string START_TEXT = "開始！";
        private const float START_DISPLAY_DURATION = 0.5f;
        private const float COUNTDOWN_DISPLAY_THRESHOLD = 1f;
        private const int COUNTDOWN_MIN_NUMBER = 1;
        private const int COUNTDOWN_MAX_NUMBER = 3;
        private const float PERCENT_MULTIPLIER = 100f;
        #endregion

        #region シリアライズフィールド
        [Header("View")]
        [SerializeField] private GameTimeView gameTimeView;
        [SerializeField] private TerritoryGaugeView territoryGaugeView;

        [Header("カウントダウン設定")]
        [SerializeField, Tooltip("カウントダウン表示用テキスト")]
        private TextMeshProUGUI countdownText;
        [SerializeField, Tooltip("カウントダウンアニメーションの拡大率")]
        private float countdownPulseScale = 1.5f;
        [SerializeField, Tooltip("カウントダウン色")]
        private Color countdownColor = Color.white;
        [SerializeField, Tooltip("START表示色")]
        private Color startColor = Color.green;

        #endregion

        #region プライベートフィールド
        private TMP_FontAsset startTextFont;
        private TMP_FontAsset numberFont;
        private int lastCountdownNumber = -1;
        private int lastTerritoryMilestone = 0;
        #endregion

        #region Unityライフサイクル
        private void Start()
        {
            gameTimeView?.Initialize();
            territoryGaugeView?.Initialize();
            InitializeCountdownDisplay();
            UpdateTerritoryGauge();
        }

        private void Update()
        {
            UpdateCountdownDisplay();
            UpdateTimeView();
            territoryGaugeView?.Tick(Time.deltaTime);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Subscribe<GroundUngreenifiedEvent>(OnGroundUngreenified);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Unsubscribe<GroundUngreenifiedEvent>(OnGroundUngreenified);
        }
        #endregion

        #region イベントハンドラー
        private void OnGroundGreenified(GroundGreenifiedEvent e)
        {
            UpdateTerritoryGauge();
        }

        private void OnGroundUngreenified(GroundUngreenifiedEvent e)
        {
            UpdateTerritoryGauge();
        }
        #endregion

        #region 残り時間
        private void UpdateTimeView()
        {
            if (gameTimeView == null || InGameManager.I == null) return;

            gameTimeView.SetRemainingTime(InGameManager.I.RemainingGameTime);
        }
        #endregion

        #region 緑化度ゲージ
        private void UpdateTerritoryGauge()
        {
            if (territoryGaugeView == null || GroundManager.I == null) return;

            float rate = GroundManager.I.GetGreenificationRate();
            territoryGaugeView.SetRate(rate);

            CheckTerritoryMilestone(rate);
        }

        /// <summary>10%刻みのマイルストーン到達を検知して演出をトリガーする</summary>
        private void CheckTerritoryMilestone(float rate)
        {
            int milestone = Mathf.FloorToInt(rate * PERCENT_MULTIPLIER / 10f);
            if (milestone == lastTerritoryMilestone) return;

            // 緑化率が下がった場合は演出せずに基準だけ下げ、再到達時に再び演出できるようにする
            bool increased = milestone > lastTerritoryMilestone;
            lastTerritoryMilestone = milestone;

            if (increased)
                StartCoroutine(territoryGaugeView.AnimateMilestonePulse());
        }
        #endregion

        #region カウントダウン
        private void InitializeCountdownDisplay()
        {
            if (countdownText != null)
            {
                startTextFont = countdownText.font;
                countdownText.gameObject.SetActive(false);
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
                            if (Tyotyo.Audio.AudioManager.I != null)
                                Tyotyo.Audio.AudioManager.I.PlaySE(Tyotyo.Audio.SEType.CountDown);
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
                        if (Tyotyo.Audio.AudioManager.I != null)
                            Tyotyo.Audio.AudioManager.I.PlaySE(Tyotyo.Audio.SEType.CountDown);
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
        #endregion
    }
}
