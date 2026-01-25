namespace SGC2025
{
    /// <summary>
    /// SEの種類
    /// </summary>
    public enum SEType
    {
        None,
        PlayerShoot,        // 弾を発射したとき
        PlayerDamage,       // プレイヤーがダメージを受けたとき
        Grass,          // 地面が緑いろになったとき
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