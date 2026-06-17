using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if STEAMWORKS_NET
using SGC2025.Ranking.Steam;
using SGC2025.Core;
#endif

namespace SGC2025.Ranking
{
    /// <summary>
    /// ランキングデータの保存と取得を管理するクラス
    /// </summary>
    public class RankingManager : Singleton<RankingManager>
    {
        private string filePath;
        private RankingData ranking;
        private const int MAX_RANK = 3;

        override protected void Awake()
        {
            base.Awake();
            filePath = Path.Combine(Application.persistentDataPath, "ranking.json");
            LoadRanking();
        }

        /// <summary>
        /// 新しいスコアを登録して保存する（Steam対応）
        /// </summary>
        /// <param name="playerName">プレイヤー名</param>
        /// <param name="greeningRate">緑化度（％）</param>
        public void AddScore(string playerName, float greeningRate)
        {
#if STEAMWORKS_NET
            if (SteamManager.Initialized)
            {
                // Steam側へ送信するために int 型に変換（小数点以下を四捨五入）
                int scoreAsInt = Mathf.RoundToInt(greeningRate);
                SteamLeaderboardManager.I.UploadScore(scoreAsInt);
                return;
            }
#endif

            // --- 以下、Steam未接続時（ローカル保存）のフォールバック処理 ---
            if (ranking == null)
                ranking = new RankingData();

            if (ranking.scores == null)
                ranking.scores = new List<ScoreData>();

            ranking.scores.Add(new ScoreData(playerName, greeningRate));
            ranking.scores.Sort((a, b) => b.greeningRate.CompareTo(a.greeningRate));

            if (ranking.scores.Count > MAX_RANK)
                ranking.scores = ranking.scores.GetRange(0, MAX_RANK);

            SaveRanking();
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

                if (ranking == null)
                    ranking = new RankingData();

                if (ranking.scores == null)
                    ranking.scores = new List<ScoreData>();
            }
            else
            {
                ranking = new RankingData { scores = new List<ScoreData>() };
            }
        }

        /// <summary>
        /// 現在のランキングを取得
        /// </summary>
        public List<ScoreData> GetRanking()
        {
            if (ranking == null)
                ranking = new RankingData();

            if (ranking.scores == null)
                ranking.scores = new List<ScoreData>();

            return ranking.scores;
        }

        /// <summary>
        /// 新しいスコアがランキングに入ったか判定する
        /// </summary>
        public bool IsNewRecord(float greeningRate)
        {
#if STEAMWORKS_NET
            if (SteamManager.Initialized)
            {
                // Steamの場合は常にオンライン上で自己ベストが更新されるため常に許可してOK
                return true;
            }
#endif

            List<ScoreData> rankingList = GetRanking();
            if (rankingList == null || rankingList.Count == 0) return true;
            if (rankingList.Count < MAX_RANK) return true;

            ScoreData lowestRank = rankingList[MAX_RANK - 1];
            return greeningRate > lowestRank.greeningRate;
        }
    }
}