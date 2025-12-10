namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーの武器システムを抽象化するインターフェース
    /// </summary>
    public interface IPlayerWeaponSystem
    {
        /// <summary>手動発射モードかどうか</summary>
        bool IsManualFiring { get; }
        
        /// <summary>
        /// 手動発射モードの設定
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        void SetManualFiring(bool enabled);
        
        /// <summary>
        /// 射撃実行
        /// </summary>
        void FireWeapon();
        
        /// <summary>
        /// 武器システムの初期化
        /// </summary>
        void Initialize();
    }
}