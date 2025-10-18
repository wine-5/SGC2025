using UnityEngine;

namespace SGC2025.Enemy.Interface
{
    /// <summary>
    /// 敵の移動戦略を定義するインターフェース
    /// </summary>
    public interface IMovementStrategy
    {
        /// <summary>
        /// 敵を移動させる
        /// </summary>
        /// <param name="enemy">敵のTransform</param>
        /// <param name="target">ターゲット（プレイヤー）のTransform</param>
        /// <param name="speed">移動速度</param>
        /// <param name="deltaTime">フレーム間の時間</param>
        void Move(Transform enemy, Transform target, float speed, float deltaTime);
    }
}