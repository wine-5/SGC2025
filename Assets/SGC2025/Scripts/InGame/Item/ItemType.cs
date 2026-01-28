namespace SGC2025.Item
{
    /// <summary>
    /// アイテムの種類を定義
    /// </summary>
    public enum ItemType
    {
        /// <summary>移動速度上昇アイテム</summary>
        SpeedBoost,
        
        /// <summary>スコア獲得量アップアイテム</summary>
        ScoreMultiplier,
        
        /// <summary>広範囲緑化アイテム（一定時間、敵撃破時に3x3範囲を緑化）</summary>
        AreaGreenify
    }
}
