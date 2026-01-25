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
            
            // プレイヤーに向かう方向ベクトルを計算（XY平面のみ）
            Vector3 targetDirection = (target.position - enemy.position);
            targetDirection.z = 0f; // Z軸の移動を制限
            targetDirection = targetDirection.normalized;
            
            // 現在の方向から目標方向に徐々に変更（慣性をつける）
            // Z軸成分を0に制限
            currentDirection.z = 0f;
            currentDirection = Vector3.Slerp(currentDirection, targetDirection, turnSpeed * deltaTime);
            currentDirection.z = 0f; // 補間後もZ軸を0に
            
            // 慣性方向に移動（Z軸位置は維持）
            Vector3 newPosition = enemy.position + currentDirection * speed * deltaTime;
            newPosition.z = enemy.position.z; // Z軸位置を維持
            enemy.position = newPosition;
        }
    }
}