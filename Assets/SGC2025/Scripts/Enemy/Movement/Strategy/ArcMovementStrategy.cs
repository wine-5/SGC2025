using UnityEngine;
using SGC2025.Enemy.Interface;

namespace SGC2025.Enemy.Strategy
{
    /// <summary>
    /// 円弧追従型の移動戦略
    /// プレイヤーの周りを円弧状に移動
    /// </summary>
    public class ArcMovementStrategy : IMovementStrategy
    {
        private float orbitRadius = 3f;
        private float orbitSpeed = 1f;
        private float currentAngle = 0f;
        
        public void Move(Transform enemy, Transform target, float speed, float deltaTime)
        {
            if (enemy == null || target == null) return;
            
            // 円弧の角度を更新
            currentAngle += orbitSpeed * deltaTime;
            
            // プレイヤーを中心とした円弧上の位置を計算（XY平面のみ）
            Vector3 offset = new Vector3(
                Mathf.Cos(currentAngle) * orbitRadius,
                Mathf.Sin(currentAngle) * orbitRadius,
                0f  // Z軸は固定
            );
            
            Vector3 targetPosition = target.position + offset;
            
            // 目標位置に向かって移動（Z軸は維持）
            Vector3 direction = (targetPosition - enemy.position);
            direction.z = 0f; // Z軸の移動を制限
            direction = direction.normalized;
            
            Vector3 newPosition = enemy.position + direction * speed * deltaTime;
            newPosition.z = enemy.position.z; // Z軸位置を維持
            enemy.position = newPosition;
        }
    }
}