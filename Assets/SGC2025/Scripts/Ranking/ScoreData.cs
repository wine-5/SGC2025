using System;

namespace SGC2025.Ranking
{
    /// <summary>
    /// ランキング1エントリのデータ。score の意味は LeaderboardType によって異なる
    /// （緑化度ランキングなら緑化％、総スコアランキングなら総スコア）
    /// </summary>
    [Serializable]
    public struct ScoreData
    {
        public string playerName;
        public float score;

        public ScoreData(string name, float score)
        {
            playerName = name;
            this.score = score;
        }
    }
}
