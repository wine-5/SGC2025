namespace SGC2025.Audio
{
    /// <summary>
    /// SEの種類
    /// </summary>
    public enum SEType
    {
        None,
        PlayerShoot,        // 弾を発射したとき
        Grass,          // 地面が緑いろになったとき
        TimeUp,            // TimeOverの音
        ButtonClick,        // ボタンをクリックしたときの音
        PlayerDamage,       // プレイヤーがダメージを受けたとき
        GetItem,
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