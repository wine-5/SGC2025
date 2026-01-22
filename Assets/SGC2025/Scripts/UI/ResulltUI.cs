using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace SGC2025
{
    /// <summary>
    /// リザルト画面UI（スコア表示とカウントアップ演出）
    /// </summary>
    public class ResulltUI : UIBase
    {
        private const float SCORE_COUNT_UP_TIME = 0.7f;
        private const int DEFAULT_RECORD_SCORE = 0;

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
                currentPhase++;
                OnPhaseChanged();
                waitTime = 0.0f;

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
                    {
                        int enemyKillScore = CommonDef.currentEnemyScore;
                        enemyScoreText.SetText(enemyKillScore.ToString());
                        break;
                    }

                case ResultPhase.GreeningScore:
                    {
                        int greeningScore = CommonDef.currentGreeningScore;
                        greeningScoreText.SetText( greeningScore.ToString() );
                        break;
                    }

                case ResultPhase.TotalScore:
                    {
                        int totalScoreScore = CommonDef.currentEnemyScore + CommonDef.currentGreeningScore;
                        totalScoreText.SetText(totalScoreScore.ToString());
                        break;
                    }

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
                    {
                        int enemyKillScore = CommonDef.currentEnemyScore;
                        enemyScoreText.SetText(ScoreCountUp(waitTime, enemyKillScore, SCORE_COUNT_UP_TIME).ToString());
                        break;
                    }

                case ResultPhase.GreeningScore:
                    {
                        int greeningScore = CommonDef.currentGreeningScore;
                        greeningScoreText.SetText( ScoreCountUp( waitTime, greeningScore, SCORE_COUNT_UP_TIME ).ToString() );
                        break;
                    }
                case ResultPhase.HighScore:
                    {
                        waitTime = 0.0f;
                        break;
                    }

                case ResultPhase.TotalScore:
                    {
                        int totalScoreScore = CommonDef.currentEnemyScore + CommonDef.currentGreeningScore;
                        totalScoreText.SetText(ScoreCountUp(waitTime, totalScoreScore, SCORE_COUNT_UP_TIME).ToString());
                        break;
                    }

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
                waitTime = 0.0f;

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
