using System;
using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// UnityのInput Systemを使用した入力処理の実装
    /// </summary>
    public class UnityInputSystemHandler : IPlayerInput
    {
        private PlayerInputSet inputSet;
        
        /// <summary>移動入力値</summary>
        public Vector2 MoveInput { get; private set; }
        
        /// <summary>射撃入力イベント</summary>
        public event Action OnShotRequested;
        
        /// <summary>移動入力イベント</summary>
        public event Action<Vector2> OnMoveInputChanged;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnityInputSystemHandler()
        {
            inputSet = new PlayerInputSet();
        }
        
        /// <summary>
        /// 入力システムの有効化
        /// </summary>
        public void EnableInput()
        {
            inputSet.Enable();
            
            // 移動入力の処理
            inputSet.Player.Movement.performed += OnMovementPerformed;
            inputSet.Player.Movement.canceled += OnMovementCanceled;
            
            // 射撃入力の処理
            inputSet.Player.Shot.performed += OnShotPerformed;
        }
        
        /// <summary>
        /// 入力システムの無効化
        /// </summary>
        public void DisableInput()
        {
            // 入力イベントの登録解除
            inputSet.Player.Movement.performed -= OnMovementPerformed;
            inputSet.Player.Movement.canceled -= OnMovementCanceled;
            inputSet.Player.Shot.performed -= OnShotPerformed;
            
            inputSet.Disable();
        }
        
        /// <summary>
        /// 移動入力開始時の処理
        /// </summary>
        private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
            OnMoveInputChanged?.Invoke(MoveInput);
        }
        
        /// <summary>
        /// 移動入力終了時の処理
        /// </summary>
        private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            MoveInput = Vector2.zero;
            OnMoveInputChanged?.Invoke(MoveInput);
        }
        
        /// <summary>
        /// 射撃入力時の処理
        /// </summary>
        private void OnShotPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnShotRequested?.Invoke();
        }
    }
}