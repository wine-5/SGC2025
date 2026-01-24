using UnityEngine;
using TMPro;

namespace SGC2025
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
        private GameObject[] buttons;

        enum ResultPhase
        {
            Init,
            Start,
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

            CreateMenu("UI/RankingCanvas");

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
                        if (RankingManager.I.IsNewRecord(DEFAULT_RECORD_SCORE))
                        {
                            CreateMenu("UI/InputFieldCanvas");
                        }
                        break;
                    }

                case ResultPhase.End:
                    {
                        foreach (GameObject button in buttons)
                        {
                            button.SetActive(true);
                        }
                        break;
                    }

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

        override protected void DestoryChild(UIBase uIBase)
        {
            if (uIBase.gameObject.name.Equals("InputFieldCanvas"))
            {
                currentPhase = ResultPhase.End; // NextPhaseとかつくるべき
                waitTime = ZERO_WAIT_TIME;

                foreach (UIBase child in childrenMenu)
                {
                    RankingUI ranking = child as RankingUI;
                    if (ranking != null)
                    {
                        ranking.UpdateScore();
                        break;
                    }

                }
            }
        }

    }
}
