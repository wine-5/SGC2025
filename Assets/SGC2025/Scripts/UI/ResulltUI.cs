using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using SGC2025.Manager;

namespace SGC2025.UI
{
    /// <summary>
    /// リザルト画面UI（スコア表示とカウントアップ演出）
    /// </summary>
    public class ResulltUI : UIBase
    {
        private const float SCORE_COUNT_UP_TIME = 0.7f;
        private const int DEFAULT_RECORD_SCORE = 0;
        private const float ZERO_WAIT_TIME = 0.0f;

        [SerializeField]
        private TextMeshProUGUI enemyScoreText;
        [SerializeField]
        private TextMeshProUGUI greeningScoreText;
        [SerializeField]
        private TextMeshProUGUI totalScoreText;
        [SerializeField]
        private TextMeshProUGUI greeningRateText; // 緑化度（％）表示
        [SerializeField]
        private GameObject[] buttons;
        [SerializeField]
        private RankingUI rankingUI; // ランキングUI（名前入力後に更新）
        [SerializeField]
        private NameInputUI nameInputUI; // 名前入力UI（ハイスコア時に表示）
        [SerializeField]
        private GameObject firstButtonAfterInput; // 名前入力後に最初に選択されるボタン

        enum ResultPhase
        {
            Init,
            Start,
            GreeningRate, // 緑化度（％）
            EnemyKillScore,
            GreeningScore,
            TotalScore,
            HighScore,
            End
        }

        ResultPhase currentPhase = ResultPhase.Init;

        override public void Start()
        {
            base.Start();

            if (nameInputUI != null)
            {
                nameInputUI.Submitted -= HandleNameSubmitted;
                nameInputUI.Submitted += HandleNameSubmitted;
            }
        }

        private void OnDestroy()
        {
            if (nameInputUI != null)
                nameInputUI.Submitted -= HandleNameSubmitted;
        }

        private void HandleNameSubmitted()
        {
            if (rankingUI != null)
                rankingUI.UpdateScore();

            ShowEndButtons();
        }

        override public void Update()
        {
            base.Update();

            if (waitTime >= SCORE_COUNT_UP_TIME)
            {
                OnPhaseUpdate(SCORE_COUNT_UP_TIME); // 最終値を表示
                currentPhase++;
                OnPhaseChanged();
                waitTime = ZERO_WAIT_TIME;
            }
            else
            {
                OnPhaseUpdate(waitTime);
            }
        }

        private void OnPhaseChanged()
        {
            switch (currentPhase)
            {
                case ResultPhase.Init:
                    break;

                case ResultPhase.Start:
                    break;

                case ResultPhase.GreeningRate:
                    if (greeningRateText != null)
                        greeningRateText.SetText("0.0%");
                    break;

                case ResultPhase.EnemyKillScore:
                    enemyScoreText.SetText("0");
                    break;

                case ResultPhase.GreeningScore:
                    greeningScoreText.SetText("0");
                    break;

                case ResultPhase.TotalScore:
                    totalScoreText.SetText("0");
                    break;

                case ResultPhase.HighScore:
                    {
                        int totalScore = ScoreManager.I != null ? ScoreManager.I.GetTotalScore() : 0;

                        var rankingManager = RankingManager.I;
                        if (rankingManager != null && rankingManager.IsNewRecord(totalScore))
                        {
                            if (nameInputUI != null)
                            {
                                nameInputUI.gameObject.SetActive(true);
                            }
                            else
                            {
                                ShowEndButtons();
                            }
                        }
                        else
                        {
                            ShowEndButtons();
                        }
                        break;
                    }

                case ResultPhase.End:
                    // ボタンはShowEndButtons()で既に表示済み
                    break;

                default:
                    break;
            }
        }
        private void OnPhaseUpdate(float waitTime)
        {
            switch (currentPhase)
            {
                case ResultPhase.Init:
                    break;

                case ResultPhase.Start:
                    break;

                case ResultPhase.GreeningRate:
                    if (greeningRateText != null)
                    {
                        float maxRate = ScoreManager.I != null ? ScoreManager.I.GetGreeningRate() * 100f : 0f;
                        float currentRate = Mathf.Lerp(0f, maxRate, Mathf.Clamp01(waitTime / SCORE_COUNT_UP_TIME));
                        greeningRateText.SetText($"{currentRate:F1}%");
                    }
                    break;

                case ResultPhase.EnemyKillScore:
                    enemyScoreText.SetText(ScoreCountUp(waitTime, ScoreManager.I.GetEnemyScore(), SCORE_COUNT_UP_TIME).ToString());
                    break;

                case ResultPhase.GreeningScore:
                    greeningScoreText.SetText(ScoreCountUp(waitTime, ScoreManager.I.GetGreenScore(), SCORE_COUNT_UP_TIME).ToString());
                    break;
                    
                case ResultPhase.HighScore:
                    waitTime = ZERO_WAIT_TIME;
                    break;

                case ResultPhase.TotalScore:
                    totalScoreText.SetText(ScoreCountUp(waitTime, ScoreManager.I.GetTotalScore(), SCORE_COUNT_UP_TIME).ToString());
                    break;

                case ResultPhase.End:
                    break;

                default:
                    {
                        break;
                    }
            }

        }

        /// <summary>
        /// Endボタンを表示して最初のボタンにフォーカスを当てる
        /// </summary>
        private void ShowEndButtons()
        {
            currentPhase = ResultPhase.End;
            waitTime = ZERO_WAIT_TIME;
            
            foreach (GameObject button in buttons)
                button.SetActive(true);
            
            if (firstButtonAfterInput != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstButtonAfterInput);
        }



    }
}
