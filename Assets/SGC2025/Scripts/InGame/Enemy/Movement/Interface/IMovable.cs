using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 移動可能なオブジェクトのインターフェース
    /// </summary>
    public interface IMovable
    {
        /// <summary>現在の移動速度</summary>
        float MoveSpeed { get; }
        
        /// <summary>オブジェクトのTransform</summary>
        Transform Transform { get; }
        
        /// <summary>移動可能かどうか</summary>
        bool CanMove { get; }
    }
}