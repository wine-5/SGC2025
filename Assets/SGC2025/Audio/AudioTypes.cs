namespace SGC2025
{
    /// <summary>
    /// SEの種類
    /// </summary>
    public enum SEType
    {
        None,
        PlayerShoot,        // 弾を発射したとき
        EnemyHit,          // 敵に弾が当たったとき（自然を取り戻すような音）
        TimeUp,            // TimeOverの音
        ButtonClick        // ボタンをクリックしたときの音
    }

    /// <summary>
    /// BGMの種類
    /// </summary>
    public enum BGMType
    {
        None,
        Title,
        InGame,
        Result,
    }
}