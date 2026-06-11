#if STEAMWORKS_NET
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Polychroma.Core.Log;
using SGC2025.Core;

namespace SGC2025.Ranking.Steam
{
    /// <summary>
    /// Steam リーダーボードの管理を行うシングルトンマネージャー
    /// </summary>
    public class SteamLeaderboardManager : Singleton<SteamLeaderboardManager>
    {
        private const string LEADERBOARD_NAME = "GreenificationRate";
        private const int LEADERBOARD_MAX_ENTRIES = 10;
        private SteamLeaderboard_t leaderboardHandle;
        private bool isLeaderboardReady = false;

        // 取得済みエントリーキャッシュ
        public List<SteamLeaderboardEntry> CachedEntries { get; private set; } = new List<SteamLeaderboardEntry>();

        //Steamworks.NET 非同期通信用の CallResult 定義
        private CallResult<LeaderboardFindResult_t> m_SteamCallResultLeaderboardFind;
        private CallResult<LeaderboardScoresDownloaded_t> m_SteamCallResultLeaderboardEntriesLoaded;
        private CallResult<LeaderboardScoreUploaded_t> m_SteamCallResultLeaderboardScoreUploaded;

        protected override bool UseDontDestroyOnLoad => true;
        protected override bool DestroyTargetGameObject => true;

        /// <summary>
        /// 親クラスの Awake で自動呼び出される Init は空にして無視する
        /// </summary>
        protected override void Init() { }

        /// <summary>
        /// コルーチンとして実行し、1フレーム待つことで SteamManager の初期化完了を確実にする
        /// </summary>
        private System.Collections.IEnumerator Start()
        {
            yield return null;

            CusLog.Log("[SteamLeaderboardManager] Initializing via Coroutine Start...");

            // Steam 初期化確認
            if (!SteamManager.Initialized)
            {
                CusLog.Log("[SteamLeaderboardManager] Steam is NOT initialized. Test mode.");
                yield break;
            }

            // CallResult のインスタンスを作成
            m_SteamCallResultLeaderboardFind = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
            m_SteamCallResultLeaderboardEntriesLoaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardEntriesLoaded);
            m_SteamCallResultLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

            FindOrCreateLeaderboard();
        }

        /// <summary>
        /// リーダーボードのハンドルを Steam 側から探す、なければ作成する
        /// </summary>
        private void FindOrCreateLeaderboard()
        {
            CusLog.Log($"[SteamLeaderboardManager] Finding or Creating Leaderboard: {LEADERBOARD_NAME}");

            SteamAPICall_t hSteamAPICall = SteamUserStats.FindOrCreateLeaderboard(
                LEADERBOARD_NAME,
                ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, // 降順（スコアが高い順）
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric
            );

            m_SteamCallResultLeaderboardFind.Set(hSteamAPICall);
        }

        /// <summary>
        /// リーダーボードが見つかった（または作成された）時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure)
        {
            if (bIOFailure || result.m_bLeaderboardFound == 0)
            {
                CusLog.Log("[SteamLeaderboardManager] Leaderboard NOT found!");
                return;
            }

            leaderboardHandle = result.m_hSteamLeaderboard;
            isLeaderboardReady = true;
            CusLog.Log("[SteamLeaderboardManager] Leaderboard found and ready.");

            // 初期化成功時にトップ10をキャッシュしておく
            FetchLeaderboard(LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// スコアを Steam に送信する
        /// </summary>
        public void UploadScore(int score)
        {
            CusLog.Log($"[SteamLeaderboardManager] UploadScore called: {score}");

            if (!isLeaderboardReady)
            {
                CusLog.Log("[SteamLeaderboardManager] Leaderboard not ready. Upload skipped.");
                return;
            }

            SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(
                leaderboardHandle,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, // 自己ベストを保持
                score,
                null,
                0
            );

            m_SteamCallResultLeaderboardScoreUploaded.Set(hSteamAPICall);
        }

        /// <summary>
        /// スコア送信が完了した時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t result, bool bIOFailure)
        {
            if (bIOFailure || result.m_bSuccess == 0)
            {
                CusLog.Log("[SteamLeaderboardManager] Failed to upload score to Steam.");
                return;
            }

            CusLog.Log($"[SteamLeaderboardManager] Score {result.m_nScore} uploaded to Steam. ScoreChanged: {result.m_bScoreChanged}");

            FetchLeaderboard(LEADERBOARD_MAX_ENTRIES);
        }

        /// <summary>
        /// グローバルランキングデータを取得する
        /// </summary>
        public void FetchLeaderboard(int count)
        {
            if (!isLeaderboardReady)
            {
                CusLog.Log("[SteamLeaderboardManager] Leaderboard not ready. Cannot fetch.");
                return;
            }

            CusLog.Log($"[SteamLeaderboardManager] Fetching top {count} entries...");

            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
                leaderboardHandle,
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
                1,
                count
            );

            m_SteamCallResultLeaderboardEntriesLoaded.Set(hSteamAPICall);
        }

        /// <summary>
        /// ランキングデータのダウンロードが完了した時に呼ばれるコールバック
        /// </summary>
        private void OnLeaderboardEntriesLoaded(LeaderboardScoresDownloaded_t result, bool bIOFailure)
        {
            if (bIOFailure)
            {
                CusLog.Log("[SteamLeaderboardManager] IO Failure while fetching leaderboard entries.");
                return;
            }

            CusLog.Log($"[SteamLeaderboardManager] Fetched {result.m_cEntryCount} entries.");

            CachedEntries.Clear();

            for (int i = 0; i < result.m_cEntryCount; i++)
            {
                LeaderboardEntry_t entry;
                SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, null, 0);

                CachedEntries.Add(new SteamLeaderboardEntry
                {
                    PlayerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser),
                    Score = entry.m_nScore,
                    Rank = entry.m_nGlobalRank
                });

                CusLog.Log($"[SteamLeaderboardManager] Entry {i + 1}: {CachedEntries[i].PlayerName} - {CachedEntries[i].Score} (Rank: {CachedEntries[i].Rank})");
            }
        }
    }
}
#endif