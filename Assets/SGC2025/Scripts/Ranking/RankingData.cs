using System;
using System.Collections.Generic;

namespace SGC2025.Ranking
{
    /// <summary>
    /// ランキングデータのコンテナクラス
    /// </summary>
    [Serializable]
    public class RankingData
    {
        public List<ScoreData> scores;
    }
}
