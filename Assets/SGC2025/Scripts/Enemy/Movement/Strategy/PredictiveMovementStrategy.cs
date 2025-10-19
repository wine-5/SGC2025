using UnityEngine;
using SGC2025.Enemy.Interface;

namespace SGC2025.Enemy.Strategy
{
    /// <summary>
    /// 予測追従型の移動戦略
    /// プレイヤーの移動先を予測して移動
    /// </summary>
    public class PredictiveMovementStrategy : IMovementStrategy
    {
        private Vector3 lastPlayerPosition;
        private Vector3 playerVelocity;
        private float predictionMultiplier = 2f;
        
        public void Move(Transform enemy, Transform target, float speed, float deltaTime)
        {
            if (enemy == null || target == null) return;
            
            // プレイヤーの速度を計算（XY平面のみ）
            Vector3 currentPlayerPosition = target.position;
            Vector3 positionDiff = currentPlayerPosition - lastPlayerPosition;
            positionDiff.z = 0f; // Z軸の変化を無視
            playerVelocity = positionDiff / deltaTime;
            lastPlayerPosition = currentPlayerPosition;
            
            // プレイヤーの移動先を予測（XY平面のみ）
            Vector3 predictedPosition = currentPlayerPosition + playerVelocity * predictionMultiplier;
            predictedPosition.z = currentPlayerPosition.z; // Z軸は現在位置を維持
            
            // 予測位置に向かって移動（Z軸制限）
            Vector3 direction = (predictedPosition - enemy.position);
            direction.z = 0f; // Z軸の移動を制限
            direction = direction.normalized;
            
            Vector3 newPosition = enemy.position + direction * speed * deltaTime;
            newPosition.z = enemy.position.z; // Z軸位置を維持
            enemy.position = newPosition;
        }
    }
}