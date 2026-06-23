namespace SGC2025.Ranking.Steam
{
    public struct SteamLeaderboardEntry
    {
        public string PlayerName;   // Steam 表示名
        public int Score;           // 緑化率 × 100（例：8500 = 85.00%）
        public int Rank;            // 順位（1始まり）
        public bool IsCurrentUser;  // ログイン中の本人の記録なら true
    }
}