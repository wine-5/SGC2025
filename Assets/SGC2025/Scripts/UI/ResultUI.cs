using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using SGC2025.Manager;
using SGC2025.Ranking;
using SGC2025.Core;
#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SGC2025.UI
{
    /// <summary>
    /// リザルト画面UI（スコア表示とカウントアップ演出）
    /// </summary>
    public class ResultUI : UIBase
    {
        private const float SCORE_COUNT_UP_TIME = 0.7f;
        private const float ZERO_WAIT_TIME = 0.0f;
        private const float PERCENT_MULTIPLIER = 100f;
        private const string RANK_SUFFIX = "位";

        [SerializeField]
        private TextMeshProUGUI greeningRateText; // 緑化度（％）表示
        [SerializeField]
        private TextMeshProUGUI rankText; // 獲得した順位表示
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

            EventBus.Subscribe<LeaderboardRankedInEvent>(HandleLeaderboardRankedIn);
        }

        private void OnDestroy()
        {
            if (nameInputUI != null)
                nameInputUI.Submitted -= HandleNameSubmitted;

            EventBus.Unsubscribe<LeaderboardRankedInEvent>(HandleLeaderboardRankedIn);
        }

        /// <summary>
        /// Steam Leaderboard へのランクイン結果を受け取り、順位を表示する
        /// </summary>
        private void HandleLeaderboardRankedIn(LeaderboardRankedInEvent rankedIn)
        {
            if (rankText != null)
                rankText.SetText($"{rankedIn.Rank}{RANK_SUFFIX}");
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

                case ResultPhase.HighScore:
                    float greeningRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;

#if STEAMWORKS_NET
                    if (SteamManager.Initialized)
                    {
                        // Steam有効時はログイン中のユーザー名を取得して自動登録（名前入力はスキップ）
                        string steamName = SteamFriends.GetPersonaName();
                        RankingManager.I.AddScore(steamName, greeningRate);
                        
                        if (rankingUI != null)
                            rankingUI.UpdateScore();

                        ShowEndButtons();
                        return;
                    }
#endif

                    // --- 以下、Steam未接続時のフォールバック処理 ---
                    var rankingManager = RankingManager.I;
                    if (rankingManager != null && rankingManager.IsNewRecord(greeningRate))
                    {
                        if (nameInputUI != null)
                            nameInputUI.gameObject.SetActive(true);
                        else
                            ShowEndButtons();
                    }
                    else
                    {
                        ShowEndButtons();
                    }
                    break;

                case ResultPhase.End:
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
                        float maxRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
                        float currentRate = Mathf.Lerp(0f, maxRate, Mathf.Clamp01(waitTime / SCORE_COUNT_UP_TIME));
                        greeningRateText.SetText($"{currentRate:F1}%");
                    }
                    break;

                case ResultPhase.HighScore:
                    waitTime = ZERO_WAIT_TIME;
                    break;

                case ResultPhase.End:
                    break;

                default:
                    break;
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
            
            UIFocusHelper.SetFocus(firstButtonAfterInput);
        }
    }
}