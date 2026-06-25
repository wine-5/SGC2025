using UnityEngine;
using Tyotyo.InGame.Player;

namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 敵の移動を管理するクラス（plain C#）
    /// EnemyControllerが所有し、Tick()で毎フレーム駆動される
    /// </summary>
    public class EnemyMovement
    {
        private const float DEFAULT_ARRIVE_THRESHOLD = 0.5f;
        private const float OVERSHOOT_MULTIPLIER = 2f;

        private readonly Transform transform;
        private readonly EnemyController controller;
        private IMovementStrategy movementStrategy;
        private Vector3 moveDirection = Vector3.down;
        private Vector3? targetPosition = null;
        private readonly float arriveThreshold = DEFAULT_ARRIVE_THRESHOLD;
        private Vector3 lastPosition;

        private Transform playerTransform;

        public EnemyMovement(Transform transform, EnemyController controller)
        {
            this.transform = transform;
            this.controller = controller;
            lastPosition = transform.position;
        }

        /// <summary>
        /// 移動戦略を設定（追従型）
        /// </summary>
        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            movementStrategy = strategy;
            targetPosition = null;
        }

        /// <summary>
        /// 目標位置を設定（固定方向移動型）
        /// </summary>
        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = target;
            movementStrategy = null;
            Vector3 direction = target - transform.position;
            direction.z = 0f;
            moveDirection = direction.normalized;
        }

        /// <summary>
        /// 毎フレームの移動処理（EnemyController.Update()から呼ぶ）
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!controller.CanMove) return;

            float speed = controller.MoveSpeed;

            if (targetPosition.HasValue)
            {
                MoveToFixedTarget(speed, deltaTime);
            }
            else if (movementStrategy != null)
            {
                Transform player = GetPlayerTransform();
                if (player != null)
                    movementStrategy.Move(transform, player, speed, deltaTime);
                else
                {
                    Vector3 movement = moveDirection * speed * deltaTime;
                    movement.z = 0f;
                    transform.Translate(movement);
                }
            }
            else
            {
                Vector3 movement = moveDirection * speed * deltaTime;
                movement.z = 0f;
                transform.Translate(movement);
            }
        }

        private Transform GetPlayerTransform()
        {
            // Playerは生成時にPlayerDataProviderへ登録される。
            // 登録前はnullのまま、次フレーム以降に再取得を試みる（取得後はキャッシュ）。
            if (playerTransform != null) return playerTransform;

            if (PlayerDataProvider.I != null && PlayerDataProvider.I.IsPlayerRegistered)
                playerTransform = PlayerDataProvider.I.PlayerTransform;

            return playerTransform;
        }

        private void MoveToFixedTarget(float speed, float deltaTime)
        {
            Vector3 movement = moveDirection * speed * deltaTime;
            movement.z = 0f;

            lastPosition = transform.position;
            transform.position += movement;

            Vector3 currentPos = transform.position;
            Vector3 targetPos = targetPosition.Value;
            currentPos.z = 0f;
            targetPos.z = 0f;

            float distanceToTarget = Vector3.Distance(currentPos, targetPos);

            Vector3 lastPos = lastPosition;
            lastPos.z = 0f;
            float lastDistance = Vector3.Distance(lastPos, targetPos);
            bool overshot = distanceToTarget > lastDistance && lastDistance < arriveThreshold * OVERSHOOT_MULTIPLIER;

            if (distanceToTarget < arriveThreshold || overshot)
                controller.ReturnToPool();
        }
    }
}