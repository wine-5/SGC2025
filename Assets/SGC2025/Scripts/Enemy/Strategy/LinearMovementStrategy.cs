using UnityEngine;
using SGC2025.Enemy.Interface;

namespace SGC2025.Enemy.Strategy
{
    /// <summary>
    /// 直線追従型の移動戦略
    /// プレイヤーに向かって直線で移動
    /// </summary>
    public class LinearMovementStrategy : IMovementStrategy
    {
        public void Move(Transform enemy, Transform target, float speed, float deltaTime)
        {
            if (enemy == null || target == null) return;
            
            // プレイヤーに向かう方向ベクトルを計算
            Vector3 direction = (target.position - enemy.position).normalized;
            
            // 直線でプレイヤーに向かって移動
            enemy.position += direction * speed * deltaTime;
        }
    }
}