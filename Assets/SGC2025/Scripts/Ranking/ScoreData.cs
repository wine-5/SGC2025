using System;

namespace SGC2025.Ranking
{
    /// <summary>
    /// ランキング1エントリのデータ
    /// </summary>
    [Serializable]
    public struct ScoreData
    {
        public string playerName;
        public float greeningRate;

        public ScoreData(string name, float rate)
        {
            playerName = name;
            greeningRate = rate;
        }
    }
}
