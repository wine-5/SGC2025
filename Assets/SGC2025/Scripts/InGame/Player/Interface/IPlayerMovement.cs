using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーの移動機能を抽象化するインターフェース
    /// </summary>
    public interface IPlayerMovement
    {
        /// <summary>移動速度</summary>
        float MoveSpeed { get; set; }
        
        /// <summary>位置制限の上限</summary>
        Vector2 PositionLimitHigh { get; set; }
        
        /// <summary>位置制限の下限</summary>
        Vector2 PositionLimitLow { get; set; }
        
        /// <summary>
        /// 移動処理を実行
        /// </summary>
        /// <param name="moveInputX">X軸入力</param>
        /// <param name="moveInputY">Y軸入力</param>
        void SetVelocity(float moveInputX, float moveInputY);
        
        /// <summary>
        /// プレイヤーの回転処理
        /// </summary>
        /// <param name="moveInput">移動入力</param>
        void Rotate(Vector2 moveInput);
    }
}