namespace SGC2025.Item
{
    /// <summary>
    /// アイテムの種類を定義
    /// </summary>
    public enum ItemType
    {
        /// <summary>移動速度上昇アイテム</summary>
        SpeedBoost,

        /// <summary>広範囲緑化アイテム（一定時間、敵撃破時に3x3範囲を緑化）</summary>
        AreaGreenify,

        /// <summary>プレイヤークローン追加アイテム（取得でクローンを1体増やす・最大4体・自動発射）</summary>
        PlayerClone
    }
}
