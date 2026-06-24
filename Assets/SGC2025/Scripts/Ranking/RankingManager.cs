using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Tyotyo.Core;
#if STEAMWORKS_NET
using Tyotyo.Ranking.Steam;
#endif

namespace Tyotyo.Ranking
{
    /// <summary>
    /// ランキングデータの保存と取得を管理するクラス（緑化度・総スコアの2系統 / 展示用・Steam用対応）
    /// </summary>
    public class RankingManager : Singleton<RankingManager>
    {
        private const int MAX_RANK = 10;
        private const int GREENING_STEAM_SCALE = 100; // 緑化％をSteam用のintへ変換する倍率（85.00% → 8500）

        private string filePath;
        private RankingData ranking;

        override protected void Awake()
        {
            base.Awake();
            filePath = Path.Combine(Application.persistentDataPath, "ranking.json");
            LoadRanking();
        }

        /// <summary>
        /// 1プレイ分の結果を緑化度・総スコア両ランキングへ登録する
        /// </summary>
        /// <param name="playerName">プレイヤー名（展示用のみ使用）</param>
        /// <param name="greeningRate">緑化度（％）</param>
        /// <param name="totalScore">総スコア</param>
        public void AddResult(string playerName, float greeningRate, int totalScore)
        {
            if (GameModeConfig.UseSteam)
            {
#if STEAMWORKS_NET
                SteamLeaderboardManager.I.UploadScore(LeaderboardType.GreeningRate, Mathf.RoundToInt(greeningRate * GREENING_STEAM_SCALE));
                SteamLeaderboardManager.I.UploadScore(LeaderboardType.TotalScore, totalScore);
#endif
                return;
            }

            // --- 展示用（ローカル保存） ---
            AddLocalScore(LeaderboardType.GreeningRate, playerName, greeningRate);
            AddLocalScore(LeaderboardType.TotalScore, playerName, totalScore);
            SaveRanking();
        }

        /// <summary>
        /// ローカルランキングへスコアを追加し、ランクインしていれば順位を通知する
        /// </summary>
        private void AddLocalScore(LeaderboardType type, string playerName, float score)
        {
            List<ScoreData> list = GetList(type);
            list.Add(new ScoreData(playerName, score));
            list.Sort((a, b) => b.score.CompareTo(a.score));

            if (list.Count > MAX_RANK)
                list.RemoveRange(MAX_RANK, list.Count - MAX_RANK);

            int rank = CalcLocalRank(list, score);
            if (rank <= MAX_RANK)
                EventBus.Publish(new LeaderboardRankedInEvent(rank, Mathf.RoundToInt(score), type));
        }

        /// <summary>
        /// スコアのローカル順位を計算する（同点は同順位）
        /// </summary>
        private int CalcLocalRank(List<ScoreData> list, float score)
        {
            int rank = 1;
            foreach (ScoreData entry in list)
                if (entry.score > score) rank++;

            return rank;
        }

        /// <summary>
        /// ランキングをJSON保存
        /// </summary>
        private void SaveRanking()
        {
            string json = JsonUtility.ToJson(ranking, true);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// ランキングをJSON読込
        /// </summary>
        private void LoadRanking()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                ranking = JsonUtility.FromJson<RankingData>(json);
            }

            EnsureRanking();
        }

        /// <summary>
        /// ranking と各リストの null を補完する
        /// </summary>
        private void EnsureRanking()
        {
            if (ranking == null)
                ranking = new RankingData();

            if (ranking.greeningScores == null)
                ranking.greeningScores = new List<ScoreData>();

            if (ranking.totalScores == null)
                ranking.totalScores = new List<ScoreData>();
        }

        /// <summary>
        /// 種別に対応するローカルスコアリストを取得する
        /// </summary>
        private List<ScoreData> GetList(LeaderboardType type)
        {
            EnsureRanking();
            return type == LeaderboardType.TotalScore ? ranking.totalScores : ranking.greeningScores;
        }

        /// <summary>
        /// 指定種別の現在のローカルランキングを取得する
        /// </summary>
        public List<ScoreData> GetRanking(LeaderboardType type) => GetList(type);

        /// <summary>
        /// 新しいスコアがランキングに入るか判定する
        /// </summary>
        public bool IsNewRecord(LeaderboardType type, float score)
        {
            if (GameModeConfig.UseSteam)
            {
                // Steamは常に自己ベストが更新されるため常に許可
                return true;
            }

            List<ScoreData> list = GetList(type);
            if (list.Count < MAX_RANK) return true;

            return score > list[MAX_RANK - 1].score;
        }
    }
}
