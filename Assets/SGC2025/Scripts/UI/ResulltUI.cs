using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SGC2025
{
    public class ResulltUI : UIBase
    {
        const float SCORE_COUNT_UP_TIME = 0.7f;

        [SerializeField]
        private TextMeshProUGUI enemyScoreText;
        [SerializeField]
        private TextMeshProUGUI greeningScoreText;

        [SerializeField]
        private TextMeshProUGUI totalScoreText;

        enum ResultPhase
        {
            Init,
            Start,
            EnemyKillScore,
            GreeningScore,
            TotalScore,
            End
        }

        ResultPhase currentPhase = ResultPhase.Init;


        override public void Start()
        {
            base.Start();
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
                    {
                        // 初期化処理
                        break;
                    }

                case ResultPhase.Start:
                    {
                        // リザルト開始処理
                        break;
                    }

                case ResultPhase.EnemyKillScore:
                    {
                        // 敵撃破スコア処理
                        break;
                    }

                case ResultPhase.GreeningScore:
                    {
                        // 緑化スコア処理
                        break;
                    }

                case ResultPhase.TotalScore:
                    {
                        // 合計スコア表示
                        break;
                    }

                case ResultPhase.End:
                    {
                        // リザルト終了処理
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }
        private void OnPhaseUpdate(float waitTime)
        {
            switch (currentPhase)
            {
                case ResultPhase.Init:
                    {
                        // 初期化処理
                        break;
                    }

                case ResultPhase.Start:
                    {
                        // リザルト開始処理
                        break;
                    }

                case ResultPhase.EnemyKillScore:
                    {
                        // 敵撃破スコア処理
                        int enemyKillScore = 0;
                        //ScoreCountUp(waitTime, enemyKillScore, SCORE_COUNT_UP_TIME);

                        enemyScoreText.SetText(ScoreCountUp(waitTime, enemyKillScore, SCORE_COUNT_UP_TIME).ToString());
                        break;
                    }

                case ResultPhase.GreeningScore:
                    {
                        // 緑化スコア処理
                        int greeningScore = 0;
                        ScoreCountUp(waitTime, greeningScore, SCORE_COUNT_UP_TIME);
                        break;
                    }

                case ResultPhase.TotalScore:
                    {
                        // 合計スコア表示
                        int totalScoreScore = 0;
                        ScoreCountUp(waitTime, totalScoreScore, SCORE_COUNT_UP_TIME);
                        break;
                    }

                case ResultPhase.End:
                    {
                        // リザルト終了処理
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

        }


    }
}
