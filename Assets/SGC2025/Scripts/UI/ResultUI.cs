using UnityEngine;
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
    /// リザルト画面UI（緑化度・総スコアのカウントアップ演出とランキング登録）
    /// </summary>
    public class ResultUI : UIBase
    {
        private const float SCORE_COUNT_UP_TIME = 0.7f;
        private const float ZERO_WAIT_TIME = 0.0f;
        private const float PERCENT_MULTIPLIER = 100f;
        private const string RANK_SUFFIX = "位";
        private const string GREENING_RANK_LABEL = "緑化度 ";
        private const string TOTAL_RANK_LABEL = "総スコア ";

        [SerializeField]
        private TextMeshProUGUI greeningRateText; // 緑化度（％）表示
        [SerializeField]
        private TextMeshProUGUI rankText; // ランクイン順位表示（緑化度・総スコアをまとめて表示）
        [SerializeField]
        private TextMeshProUGUI totalScoreText; // 総スコア表示

        private int? greeningRank;
        private int? totalRank;
        [SerializeField]
        private GameObject[] buttons;
        [SerializeField]
        private RankingUI rankingUI; // ランキングUI（登録後に更新）
        [SerializeField]
        private NameInputUI nameInputUI; // 名前入力UI（展示用ハイスコア時に表示）
        [SerializeField]
        private GameObject firstButtonAfterInput; // 名前入力後に最初に選択されるボタン

        enum ResultPhase
        {
            Init,
            Start,
            GreeningRate, // 緑化度（％）
            TotalScore,   // 総スコア
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
        /// ランクイン結果を受け取り、緑化度・総スコアの順位を1つのテキストへまとめて表示する
        /// </summary>
        private void HandleLeaderboardRankedIn(LeaderboardRankedInEvent rankedIn)
        {
            if (rankedIn.Type == LeaderboardType.TotalScore)
                totalRank = rankedIn.Rank;
            else
                greeningRank = rankedIn.Rank;

            RefreshRankText();
        }

        /// <summary>
        /// 取得済みの順位を1つのテキストへ反映する（ランクインした種別のみ表示）
        /// </summary>
        private void RefreshRankText()
        {
            if (rankText == null) return;

            string text = string.Empty;

            if (greeningRank.HasValue)
                text += $"{GREENING_RANK_LABEL}{greeningRank.Value}{RANK_SUFFIX}";

            if (totalRank.HasValue)
            {
                if (text.Length > 0) text += "\n";
                text += $"{TOTAL_RANK_LABEL}{totalRank.Value}{RANK_SUFFIX}";
            }

            rankText.SetText(text);
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
                case ResultPhase.GreeningRate:
                    if (greeningRateText != null)
                        greeningRateText.SetText("0.0%");
                    break;

                case ResultPhase.TotalScore:
                    if (totalScoreText != null)
                        totalScoreText.SetText("0");
                    break;

                case ResultPhase.HighScore:
                    RegisterResult();
                    break;

                default:
                    break;
            }
        }

        private void OnPhaseUpdate(float waitTime)
        {
            switch (currentPhase)
            {
                case ResultPhase.GreeningRate:
                    if (greeningRateText != null)
                    {
                        float maxRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
                        float currentRate = Mathf.Lerp(0f, maxRate, Mathf.Clamp01(waitTime / SCORE_COUNT_UP_TIME));
                        greeningRateText.SetText($"{currentRate:F1}%");
                    }
                    break;

                case ResultPhase.TotalScore:
                    if (totalScoreText != null)
                    {
                        int maxScore = GameManager.I.FinalTotalScore;
                        int currentScore = Mathf.RoundToInt(Mathf.Lerp(0f, maxScore, Mathf.Clamp01(waitTime / SCORE_COUNT_UP_TIME)));
                        totalScoreText.SetText(currentScore.ToString());
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 緑化度・総スコアをランキングへ登録する（モードにより Steam 自動登録 / 展示用名前入力を切替）
        /// </summary>
        private void RegisterResult()
        {
            float greeningRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
            int totalScore = GameManager.I.FinalTotalScore;

            if (GameModeConfig.UseSteam)
            {
                // Steam: 両ランキングへ自動登録（順位は送信完了イベントで表示される）
                RankingManager.I.AddResult(GetPlayerName(), greeningRate, totalScore);

                if (rankingUI != null)
                    rankingUI.UpdateScore();

                ShowEndButtons();
                return;
            }

            // 展示用: いずれかのランキングにランクインしていれば名前入力を表示
            RankingManager rankingManager = RankingManager.I;
            bool isNewRecord = rankingManager != null &&
                (rankingManager.IsNewRecord(LeaderboardType.GreeningRate, greeningRate) ||
                 rankingManager.IsNewRecord(LeaderboardType.TotalScore, totalScore));

            if (isNewRecord && nameInputUI != null)
                nameInputUI.gameObject.SetActive(true);
            else
                ShowEndButtons();
        }

        /// <summary>
        /// 登録に使うプレイヤー名を取得する（Steam時はログイン名、それ以外は空）
        /// </summary>
        private string GetPlayerName()
        {
#if STEAMWORKS_NET
            if (SteamManager.Initialized)
                return SteamFriends.GetPersonaName();
#endif
            return string.Empty;
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
