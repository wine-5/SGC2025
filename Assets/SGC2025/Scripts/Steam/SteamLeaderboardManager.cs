#if STEAMWORKS_NET
using System.Collections.Generic;
using Steamworks;
using Polychroma.Core.Log;
using SGC2025.Core;
using SGC2025.Ranking;
using Cysharp.Threading.Tasks;

namespace SGC2025.Ranking.Steam
{
    /// <summary>
    /// Steam リーダーボード（緑化度・総スコア）の管理を行うシングルトンマネージャー
    /// </summary>
    public class SteamLeaderboardManager : Singleton<SteamLeaderboardManager>
    {
        private const int LEADERBOARD_MAX_ENTRIES = 10;
        private const string LOG_CATEGORY = "Steam";

        private static readonly LeaderboardType[] AllTypes =
        {
            LeaderboardType.GreeningRate,
            LeaderboardType.TotalScore,
        };

        // 種別ごとの状態
        private readonly Dictionary<LeaderboardType, SteamLeaderboard_t> handles = new Dictionary<LeaderboardType, SteamLeaderboard_t>();
        private readonly Dictionary<LeaderboardType, bool> ready = new Dictionary<LeaderboardType, bool>();
        private readonly Dictionary<LeaderboardType, List<SteamLeaderboardEntry>> cachedEntries = new Dictionary<LeaderboardType, List<SteamLeaderboardEntry>>();

        // 種別ごとの CallResult（同時進行で互いに上書きしないよう種別単位で保持）
        private readonly Dictionary<LeaderboardType, CallResult<LeaderboardFindResult_t>> findCallResults = new Dictionary<LeaderboardType, CallResult<LeaderboardFindResult_t>>();
        private readonly Dictionary<LeaderboardType, CallResult<LeaderboardScoresDownloaded_t>> downloadCallResults = new Dictionary<LeaderboardType, CallResult<LeaderboardScoresDownloaded_t>>();
        private readonly Dictionary<LeaderboardType, CallResult<LeaderboardScoreUploaded_t>> uploadCallResults = new Dictionary<LeaderboardType, CallResult<LeaderboardScoreUploaded_t>>();

        protected override bool UseDontDestroyOnLoad => true;
        protected override bool DestroyTargetGameObject => true;

        /// <summary>
        /// 親クラスの Awake で自動呼び出される Init は空にして無視する
        /// </summary>
        protected override void Init() { }

        private void Start()
        {
            InitializeAsync().Forget();
        }

        /// <summary>
        /// Steam Leaderboard 名へ変換する
        /// </summary>
        // 共有テストアプリ(AppID 480)で他開発者と衝突しないよう、プロジェクト固有の名前にする
        private static string GetLeaderboardName(LeaderboardType type) => type switch
        {
            LeaderboardType.TotalScore => "SGC2025_TotalScore",
            _ => "SGC2025_GreeningRate",
        };

        /// <summary>
        /// UniTask による非同期初期化処理
        /// </summary>
        private async UniTaskVoid InitializeAsync()
        {
            CusLog.Log(LOG_CATEGORY, "Initializing via UniTask Start...");

            await UniTask.Yield();

            if (!SteamManager.Initialized)
            {
                CusLog.Warning(LOG_CATEGORY, "Steam is NOT initialized. Test mode.");
                return;
            }

            foreach (LeaderboardType type in AllTypes)
            {
                LeaderboardType captured = type;
                cachedEntries[captured] = new List<SteamLeaderboardEntry>();
                ready[captured] = false;
                findCallResults[captured] = CallResult<LeaderboardFindResult_t>.Create((r, io) => OnLeaderboardFound(captured, r, io));
                downloadCallResults[captured] = CallResult<LeaderboardScoresDownloaded_t>.Create((r, io) => OnLeaderboardEntriesLoaded(captured, r, io));
                uploadCallResults[captured] = CallResult<LeaderboardScoreUploaded_t>.Create((r, io) => OnLeaderboardScoreUploaded(captured, r, io));

                FindOrCreateLeaderboard(captured);
            }
        }

        /// <summary>
        /// リーダーボードのハンドルを Steam 側から探す、なければ作成する
        /// </summary>
        private void FindOrCreateLeaderboard(LeaderboardType type)
        {
            string name = GetLeaderboardName(type);
            CusLog.Log(LOG_CATEGORY, $"Finding or Creating Leaderboard: {name}");

            SteamAPICall_t hSteamAPICall = SteamUserStats.FindOrCreateLeaderboard(
                name,
                ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, // 降順（スコアが高い順）
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric
            );

            findCallResults[type].Set(hSteamAPICall);
        }

        /// <summary>
        /// リーダーボードが見つかった（または作成された）時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardFound(LeaderboardType type, LeaderboardFindResult_t result, bool bIOFailure)
        {
            if (bIOFailure || result.m_bLeaderboardFound == 0)
            {
                CusLog.Error(LOG_CATEGORY, $"Leaderboard NOT found! ({GetLeaderboardName(type)})");
                return;
            }

            handles[type] = result.m_hSteamLeaderboard;
            ready[type] = true;
            CusLog.Log(LOG_CATEGORY, $"Leaderboard found and ready. ({GetLeaderboardName(type)})");

            // 初期化成功時にトップ10をキャッシュしておく
            FetchLeaderboard(type, LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// スコアを Steam に送信する
        /// </summary>
        public void UploadScore(LeaderboardType type, int score)
        {
            CusLog.Log(LOG_CATEGORY, $"UploadScore called: {GetLeaderboardName(type)} = {score}");

            if (!IsReady(type))
            {
                CusLog.Warning(LOG_CATEGORY, $"Leaderboard not ready. Upload skipped. ({GetLeaderboardName(type)})");
                return;
            }

            SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(
                handles[type],
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, // 自己ベストを保持
                score,
                null,
                0
            );

            uploadCallResults[type].Set(hSteamAPICall);
        }

        /// <summary>
        /// スコア送信が完了した時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardScoreUploaded(LeaderboardType type, LeaderboardScoreUploaded_t result, bool bIOFailure)
        {
            if (bIOFailure || result.m_bSuccess == 0)
            {
                CusLog.Error(LOG_CATEGORY, $"Failed to upload score to Steam. ({GetLeaderboardName(type)})");
                return;
            }

            CusLog.Log(LOG_CATEGORY, $"Score {result.m_nScore} uploaded to Steam. ScoreChanged: {result.m_bScoreChanged} ({GetLeaderboardName(type)})");

            // 送信後のグローバル順位をリザルト画面へ通知する
            EventBus.Publish(new LeaderboardRankedInEvent(result.m_nGlobalRankNew, result.m_nScore, type));

            FetchLeaderboard(type, LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// グローバルランキングデータを取得する
        /// </summary>
        public void FetchLeaderboard(LeaderboardType type, int count)
        {
            if (!IsReady(type))
            {
                CusLog.Warning(LOG_CATEGORY, $"Leaderboard not ready. Cannot fetch. ({GetLeaderboardName(type)})");
                return;
            }

            CusLog.Log(LOG_CATEGORY, $"Fetching top {count} entries... ({GetLeaderboardName(type)})");

            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
                handles[type],
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
                1,
                count
            );

            downloadCallResults[type].Set(hSteamAPICall);
        }

        /// <summary>
        /// ランキングデータのダウンロードが完了した時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardEntriesLoaded(LeaderboardType type, LeaderboardScoresDownloaded_t result, bool bIOFailure)
        {
            if (bIOFailure)
            {
                CusLog.Error(LOG_CATEGORY, $"IO Failure while fetching leaderboard entries. ({GetLeaderboardName(type)})");
                return;
            }

            CusLog.Log(LOG_CATEGORY, $"Fetched {result.m_cEntryCount} entries. ({GetLeaderboardName(type)})");

            List<SteamLeaderboardEntry> entries = cachedEntries[type];
            entries.Clear();

            for (int i = 0; i < result.m_cEntryCount; i++)
            {
                SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t entry, null, 0);

                entries.Add(new SteamLeaderboardEntry
                {
                    PlayerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser),
                    Score = entry.m_nScore,
                    Rank = entry.m_nGlobalRank,
                    IsCurrentUser = entry.m_steamIDUser == SteamUser.GetSteamID()
                });
            }

            // 取得完了をUIへ通知して再描画させる
            EventBus.Publish(new LeaderboardEntriesUpdatedEvent(type));
        }

        /// <summary>
        /// 指定種別の取得済みエントリーを取得する
        /// </summary>
        public List<SteamLeaderboardEntry> GetCachedEntries(LeaderboardType type)
            => cachedEntries.TryGetValue(type, out List<SteamLeaderboardEntry> entries) ? entries : null;

        /// <summary>
        /// 指定種別の Leaderboard が利用可能か
        /// </summary>
        private bool IsReady(LeaderboardType type) => ready.TryGetValue(type, out bool isReady) && isReady;
    }
}
#endif
