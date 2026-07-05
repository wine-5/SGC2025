using UnityEngine;
using Tyotyo.Manager;
using Tyotyo.Ranking;
using Tyotyo.Core;
#if STEAMWORKS_NET
using Steamworks;
#endif

namespace Tyotyo.UI
{
    /// <summary>
    /// リザルト画面UI（緑化度・総スコアのカウントアップ演出とランキング登録）
    /// </summary>
    public class ResultUI : UIBase
    {
        private const float ZERO_WAIT_TIME = 0.0f;
        private const float PERCENT_MULTIPLIER = 100f;
        private const string RANK_SUFFIX = "位";
        private const string OUT_OF_RANK_TEXT = "圏外"; // TOP10圏外（死亡や低スコア含む）の順位表示
        private const int MAX_RANK = 10;               // ランキングに掲載される最大順位

        [Header("結果の各行（ラベル＋数値のセット）")]
        [SerializeField]
        private ResultStatRow greeningRateRow; // 緑化度（％）
        [SerializeField]
        private ResultStatRow totalScoreRow;   // 総スコア
        [SerializeField]
        private ResultStatRow greeningRankRow;  // 緑化度ランキング順位
        [SerializeField]
        private ResultStatRow totalRankRow;     // 総スコアランキング順位

        private int? greeningRank;
        private int? totalRank;
        // 順位が確定したか（登録処理を開始した時点でtrue）。確定後はランクイン外を「圏外」と表示する
        private bool ranksFinalized;
        [SerializeField]
        private GameObject[] buttons;
        [SerializeField]
        private RankingUI rankingUI; // ランキングUI（登録後に更新）
        [SerializeField]
        private NameInputUI nameInputUI; // 名前入力UI（展示用ハイスコア時に表示）
        [SerializeField]
        private GameObject firstButtonAfterInput; // 名前入力後に最初に選択されるボタン

        [Header("演出設定")]
        [SerializeField, Tooltip("緑化度・総スコアのカウントアップ（＝マップ再生）にかける秒数")]
        private float scoreCountUpTime = 2f;
        [SerializeField, Tooltip("カウントアップ開始前の前置き秒数（Init/Start各フェーズ）。0ですぐ開始")]
        private float startDelay = 0.2f;
        [SerializeField, Tooltip("背景に塗り直しを再生するマップ（緑化度カウントアップに同期）")]
        private ResultMapReplay mapReplay;

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

        protected override void Start()
        {
            base.Start(); // UIBase のリスナー登録処理を実行

            if (nameInputUI != null)
            {
                nameInputUI.Submitted -= HandleNameSubmitted;
                nameInputUI.Submitted += HandleNameSubmitted;
            }

            EventBus.Subscribe<LeaderboardRankedInEvent>(HandleLeaderboardRankedIn);

            if (mapReplay != null)
                mapReplay.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy(); // UIBase のリスナー登録解除処理を実行

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
        /// 取得済みの順位を各行へ反映する。
        /// TOP10入りは「N位」、確定後にランクイン外（死亡や低スコア）なら「圏外」と表示する。
        /// </summary>
        private void RefreshRankText()
        {
            UpdateRankRow(greeningRankRow, greeningRank);
            UpdateRankRow(totalRankRow, totalRank);
        }

        /// <summary>
        /// 1つの順位行を更新する。ランクイン（1〜MAX_RANK位）なら順位、確定後の圏外なら「圏外」を表示する。
        /// 順位未確定かつ未ランクインの場合は何もしない（カウントアップ中に上書きしないため）。
        /// </summary>
        private void UpdateRankRow(ResultStatRow row, int? rank)
        {
            if (row == null) return;

            if (rank.HasValue && rank.Value >= 1 && rank.Value <= MAX_RANK)
                row.SetValue($"{rank.Value}{RANK_SUFFIX}");
            else if (ranksFinalized)
                row.SetValue(OUT_OF_RANK_TEXT);
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

            if (waitTime >= GetPhaseDuration(currentPhase))
            {
                OnPhaseUpdate(scoreCountUpTime); // 最終値を表示
                currentPhase++;
                OnPhaseChanged();
                waitTime = ZERO_WAIT_TIME;
            }
            else
            {
                OnPhaseUpdate(waitTime);
            }
        }

        /// <summary>
        /// フェーズごとの所要時間を返す。
        /// カウントアップ系は演出時間、それ以外（前置き）は短いstartDelayを使う。
        /// </summary>
        private float GetPhaseDuration(ResultPhase phase)
        {
            switch (phase)
            {
                case ResultPhase.GreeningRate:
                case ResultPhase.TotalScore:
                    return scoreCountUpTime;
                default:
                    return startDelay;
            }
        }

        private void OnPhaseChanged()
        {
            switch (currentPhase)
            {
                case ResultPhase.GreeningRate:
                    greeningRateRow?.SetValue("0.0%");
                    break;

                case ResultPhase.TotalScore:
                    totalScoreRow?.SetValue("0");
                    break;

                case ResultPhase.HighScore:
                    RegisterResult();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// カウントアップ進捗(0〜1)を返す。scoreCountUpTimeが0以下でも
        /// 0除算（NaN）にならないよう、その場合は即完了(1)とみなす。
        /// </summary>
        private float CountUpProgress(float waitTime)
        {
            if (scoreCountUpTime <= 0f) return 1f;
            return Mathf.Clamp01(waitTime / scoreCountUpTime);
        }

        private void OnPhaseUpdate(float waitTime)
        {
            switch (currentPhase)
            {
                case ResultPhase.GreeningRate:
                    float greeningProgress = CountUpProgress(waitTime);

                    if (greeningRateRow != null)
                    {
                        float maxRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
                        float currentRate = Mathf.Lerp(0f, maxRate, greeningProgress);
                        greeningRateRow.SetValue($"{currentRate:F1}%");
                    }

                    // ％の進捗に同期して背景マップへ塗り直しを再生する
                    if (mapReplay != null)
                        mapReplay.SetProgress(greeningProgress);
                    break;

                case ResultPhase.TotalScore:
                    if (totalScoreRow != null)
                    {
                        int maxScore = GameManager.I.FinalTotalScore;
                        int currentScore = Mathf.RoundToInt(Mathf.Lerp(0f, maxScore, CountUpProgress(waitTime)));
                        totalScoreRow.SetValue(currentScore.ToString());
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
            // ここで順位を確定する。ランクインした種別は登録処理（イベント）で「N位」へ上書きされ、
            // ランクインしなかった種別（死亡・低スコア含む）は「圏外」として表示される。
            ranksFinalized = true;

            float greeningRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
            int totalScore = GameManager.I.FinalTotalScore;

            if (GameModeConfig.UseSteam)
            {
                // Steam: 両ランキングへ自動登録（順位は送信完了イベントで表示される）
                RefreshRankText();
                RankingManager.I.AddResult(GetPlayerName(), greeningRate, totalScore);

                if (rankingUI != null)
                    rankingUI.UpdateScore();

                ShowEndButtons();
                return;
            }

            // 展示用: 名前入力（＝AddResult）は後段で行われるため、登録前の想定順位を先に表示する。
            // これをしないと、名前入力中は順位が未確定のまま「圏外」と誤表示されてしまう。
            RankingManager rankingManager = RankingManager.I;
            if (rankingManager != null)
            {
                greeningRank = rankingManager.GetProspectiveRank(LeaderboardType.GreeningRate, greeningRate);
                totalRank = rankingManager.GetProspectiveRank(LeaderboardType.TotalScore, totalScore);
            }
            RefreshRankText();

            // 展示用: いずれかのランキングにランクインしていれば名前入力を表示
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
