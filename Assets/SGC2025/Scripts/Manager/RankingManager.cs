using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SGC2025.Manager
{
    /// <summary>
    /// スコアデータの構造体
    /// </summary>
    [Serializable]
    public struct ScoreData
    {
        public string playerName;
        public int score;
        public float greeningRate; // 緑化度（独占度）

        public ScoreData(string name, int s, float rate)
        {
            playerName = name;
            score = s;
            greeningRate = rate;
        }
    }

    /// <summary>
    /// ランキングデータのコンテナクラス
    /// </summary>
    [Serializable]
    public class RankingData
    {
        public List<ScoreData> scores;
    }

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
        /// 新しいスコアを登録して保存する
        /// </summary>
        /// <param name="playerName">プレイヤー名</param>
        /// <param name="score">スコア</param>
        /// <param name="greeningRate">緑化度（％）</param>
        public void AddScore(string playerName, int score, float greeningRate)
        {
            if (ranking == null)
            {
                ranking = new RankingData();
            }

            if (ranking.scores == null)
            {
                ranking.scores = new List<ScoreData>();
            }

            ranking.scores.Add(new ScoreData(playerName, score, greeningRate));

            ranking.scores.Sort((a, b) => b.score.CompareTo(a.score));

            if (ranking.scores.Count > MAX_RANK)
            {
                ranking.scores = ranking.scores.GetRange(0, MAX_RANK);
            }

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
                {
                    ranking = new RankingData();
                }

                if (ranking.scores == null)
                {
                    ranking.scores = new List<ScoreData>();
                }
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
            {
                ranking = new RankingData();
            }

            if (ranking.scores == null)
            {
                ranking.scores = new List<ScoreData>();
            }

            return ranking.scores;
        }
        
        /// <summary>
        /// 新しいスコアがランキングに入ったか判定する
        /// </summary>
        public bool IsNewRecord(int score)
        {
            List<ScoreData> rankingList = GetRanking();
            if (rankingList == null || rankingList.Count == 0) return true;

            int lowestScore = rankingList[Mathf.Min(rankingList.Count, MAX_RANK) - 1].score;
            return score > lowestScore || rankingList.Count < MAX_RANK;
        }
    }
}
