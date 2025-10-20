using System;
using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーの入力を抽象化するインターフェース
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>移動入力値</summary>
        Vector2 MoveInput { get; }
        
        /// <summary>射撃入力イベント</summary>
        event Action OnShotRequested;
        
        /// <summary>移動入力イベント</summary>
        event Action<Vector2> OnMoveInputChanged;
        
        /// <summary>入力システムの有効化</summary>
        void EnableInput();
        
        /// <summary>入力システムの無効化</summary>
        void DisableInput();
    }
}