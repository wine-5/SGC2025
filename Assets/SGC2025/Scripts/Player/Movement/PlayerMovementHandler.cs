using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーの移動機能の実装
    /// </summary>
    public class PlayerMovementHandler : IPlayerMovement
    {
        private const float DEFAULT_MOVE_SPEED = 5f;
        
        private readonly Rigidbody rigidbody;
        private readonly Transform transform;
        
        /// <summary>移動速度</summary>
        public float MoveSpeed { get; set; } = DEFAULT_MOVE_SPEED;
        
        /// <summary>位置制限の上限</summary>
        public Vector2 PositionLimitHigh { get; set; }
        
        /// <summary>位置制限の下限</summary>
        public Vector2 PositionLimitLow { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rigidbody">移動に使用するRigidbody</param>
        /// <param name="transform">回転に使用するTransform</param>
        public PlayerMovementHandler(Rigidbody rigidbody, Transform transform)
        {
            this.rigidbody = rigidbody;
            this.transform = transform;
        }
        
        /// <summary>
        /// 移動処理を実行
        /// </summary>
        /// <param name="moveInputX">X軸入力</param>
        /// <param name="moveInputY">Y軸入力</param>
        public void SetVelocity(float moveInputX, float moveInputY)
        {
            Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
            rigidbody.linearVelocity = new Vector2(
                moveInputNormalized.x * MoveSpeed, 
                moveInputNormalized.y * MoveSpeed
            );
        }
        
        /// <summary>
        /// プレイヤーの回転処理
        /// </summary>
        /// <param name="moveInput">移動入力</param>
        public void Rotate(Vector2 moveInput)
        {
            // プレイヤーの回転
            if (moveInput != Vector2.zero)
            {
                transform.up = rigidbody.linearVelocity;
            }
        }
    }
}