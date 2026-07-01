#if STEAMWORKS_NET
using System.Collections.Generic;
using Steamworks;
using Tyotyo.Core.Log;
using Tyotyo.Core;
using Tyotyo.Ranking;
using Cysharp.Threading.Tasks;

namespace Tyotyo.Ranking.Steam
{
    /// <summary>
    /// Steam リーダーボード（緑化度・総スコア）の管理を行うシングルトンマネージャー
    /// </summary>
    public class SteamLeaderboardManager : Singleton<SteamLeaderboardManager>
    {
        private const int LEADERBOARD_MAX_ENTRIES = 10;
        private const string LOG_CATEGORY = "Steam";
        private const int OUT_OF_RANK = LEADERBOARD_MAX_ENTRIES + 1; // 本人エントリが取得できない場合の圏外順位

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
        // 本人順位の取得用（グローバル取得と CallResult を共有すると互いに上書きするため別管理）
        private readonly Dictionary<LeaderboardType, CallResult<LeaderboardScoresDownloaded_t>> rankQueryCallResults = new Dictionary<LeaderboardType, CallResult<LeaderboardScoresDownloaded_t>>();
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
            // CusLog.Log(LOG_CATEGORY, "UniTaskで初期化を開始します...");

            await UniTask.Yield();

            if (!SteamManager.Initialized)
            {
                CusLog.Warning(LOG_CATEGORY, "Steamが初期化されていません。テストモードで動作します。");
                return;
            }

            // Steam が完全に初期化されるまで少し待つ
            await UniTask.Delay(1000);

            foreach (LeaderboardType type in AllTypes)
            {
                LeaderboardType captured = type;
                cachedEntries[captured] = new List<SteamLeaderboardEntry>();
                ready[captured] = false;
                findCallResults[captured] = CallResult<LeaderboardFindResult_t>.Create((r, io) => OnLeaderboardFound(captured, r, io));
                downloadCallResults[captured] = CallResult<LeaderboardScoresDownloaded_t>.Create((r, io) => OnLeaderboardEntriesLoaded(captured, r, io));
                rankQueryCallResults[captured] = CallResult<LeaderboardScoresDownloaded_t>.Create((r, io) => OnUserRankLoaded(captured, r, io));
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
            CusLog.Log(LOG_CATEGORY, $"リーダーボードを検索または作成します: {name}");

            SteamAPICall_t hSteamAPICall = SteamUserStats.FindOrCreateLeaderboard(
                name,
                ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
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
                CusLog.Error(LOG_CATEGORY, $"リーダーボードが見つかりませんでした！（{GetLeaderboardName(type)}）");
                return;
            }

            handles[type] = result.m_hSteamLeaderboard;
            ready[type] = true;
            // CusLog.Log(LOG_CATEGORY, $"リーダーボードの準備が完了しました。（{GetLeaderboardName(type)}）");

            // 初期化成功時にトップ10をキャッシュしておく
            FetchLeaderboard(type, LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// スコアを Steam に送信する
        /// </summary>
        public void UploadScore(LeaderboardType type, int score)
        {
            CusLog.Log(LOG_CATEGORY, $"スコア送信を呼び出しました: {GetLeaderboardName(type)} = {score}");

            if (!IsReady(type))
            {
                CusLog.Warning(LOG_CATEGORY, $"リーダーボードが未準備のため送信をスキップしました。（{GetLeaderboardName(type)}）");
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
                CusLog.Error(LOG_CATEGORY, $"Steamへのスコア送信に失敗しました。（{GetLeaderboardName(type)}）");
                return;
            }

            CusLog.Log(LOG_CATEGORY, $"スコア {result.m_nScore} をSteamへ送信しました。スコア更新: {result.m_bScoreChanged}（{GetLeaderboardName(type)}）");

            // 自己ベストが更新された場合は新しいグローバル順位をそのまま通知する。
            // 更新されなかった場合（KeepBestで既存ベストが上回る）は m_nGlobalRankNew が 0 を返すため、
            // 本人エントリを取得し直して現在の実順位を通知する。
            if (result.m_nGlobalRankNew > 0)
                EventBus.Publish(new LeaderboardRankedInEvent(result.m_nGlobalRankNew, result.m_nScore, type));
            else
                FetchUserRank(type);

            FetchLeaderboard(type, LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// 本人エントリ（自己ベスト）を取得し、現在のグローバル順位をリザルト画面へ通知する。
        /// 自己ベスト未更新で順位が返らなかった場合に使用する。
        /// </summary>
        private void FetchUserRank(LeaderboardType type)
        {
            if (!IsReady(type)) return;

            // AroundUser の範囲 [0,0] は本人エントリのみを返す
            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
                handles[type],
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
                0,
                0
            );

            rankQueryCallResults[type].Set(hSteamAPICall);
        }

        /// <summary>
        /// 本人エントリの取得が完了した時に呼ばれるコールバック。実順位をリザルト画面へ通知する。
        /// </summary>
        private void OnUserRankLoaded(LeaderboardType type, LeaderboardScoresDownloaded_t result, bool bIOFailure)
        {
            if (bIOFailure || result.m_cEntryCount == 0)
            {
                // 本人エントリが取得できなければ圏外として通知する
                CusLog.Log(LOG_CATEGORY, $"本人順位の取得: エントリなし → 圏外（{GetLeaderboardName(type)}）");
                EventBus.Publish(new LeaderboardRankedInEvent(OUT_OF_RANK, 0, type));
                return;
            }

            SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, 0, out LeaderboardEntry_t entry, null, 0);
            CusLog.Log(LOG_CATEGORY, $"本人順位の取得: 順位={entry.m_nGlobalRank}, スコア={entry.m_nScore}（{GetLeaderboardName(type)}）");
            EventBus.Publish(new LeaderboardRankedInEvent(entry.m_nGlobalRank, entry.m_nScore, type));
        }

        /// <summary>
        /// グローバルランキングデータを取得する
        /// </summary>
        public void FetchLeaderboard(LeaderboardType type, int count)
        {
            if (!IsReady(type))
            {
                CusLog.Warning(LOG_CATEGORY, $"リーダーボードが未準備のため取得できません。（{GetLeaderboardName(type)}）");
                return;
            }

            // CusLog.Log(LOG_CATEGORY, $"上位 {count} 件を取得します...（{GetLeaderboardName(type)}）");

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
                CusLog.Error(LOG_CATEGORY, $"リーダーボード取得中にIOエラーが発生しました。（{GetLeaderboardName(type)}）");
                return;
            }

            // CusLog.Log(LOG_CATEGORY, $"{result.m_cEntryCount} 件を取得しました。（{GetLeaderboardName(type)}）");

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
