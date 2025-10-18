using UnityEngine;
using SGC2025.Enemy.Interface;

namespace SGC2025.Enemy.Strategy
{
    /// <summary>
    /// 慣性追従型の移動戦略
    /// プレイヤーの方向に徐々に向きを変える
    /// </summary>
    public class InertiaMovementStrategy : IMovementStrategy
    {
        private Vector3 currentDirection = Vector3.down;
        private float turnSpeed = 2f;
        
        public void Move(Transform enemy, Transform target, float speed, float deltaTime)
        {
            if (enemy == null || target == null) return;
            
            // プレイヤーに向かう方向ベクトルを計算
            Vector3 targetDirection = (target.position - enemy.position).normalized;
            
            // 現在の方向から目標方向に徐々に変更（慣性をつける）
            currentDirection = Vector3.Slerp(currentDirection, targetDirection, turnSpeed * deltaTime);
            
            // 慣性方向に移動
            enemy.position += currentDirection * speed * deltaTime;
        }
    }
}