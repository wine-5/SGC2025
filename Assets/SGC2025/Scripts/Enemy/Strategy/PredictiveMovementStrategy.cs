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
            
            // プレイヤーの速度を計算
            Vector3 currentPlayerPosition = target.position;
            playerVelocity = (currentPlayerPosition - lastPlayerPosition) / deltaTime;
            lastPlayerPosition = currentPlayerPosition;
            
            // プレイヤーの移動先を予測
            Vector3 predictedPosition = currentPlayerPosition + playerVelocity * predictionMultiplier;
            
            // 予測位置に向かって移動
            Vector3 direction = (predictedPosition - enemy.position).normalized;
            enemy.position += direction * speed * deltaTime;
        }
    }
}