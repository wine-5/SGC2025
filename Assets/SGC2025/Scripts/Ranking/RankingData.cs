using System;
using System.Collections.Generic;

namespace SGC2025.Ranking
{
    /// <summary>
    /// ランキングデータのコンテナクラス（緑化度・総スコアの2系統を保持）
    /// </summary>
    [Serializable]
    public class RankingData
    {
        public List<ScoreData> greeningScores;
        public List<ScoreData> totalScores;
    }
}
