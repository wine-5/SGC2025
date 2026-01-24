using UnityEngine;
using SGC2025.Enemy.Interface;
using SGC2025.Player.Bullet;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の移動を管理するコンポーネント
    /// 移動戦略パターンを使用して異なる移動タイプを実現
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        private const float DEFAULT_ARRIVE_THRESHOLD = 0.5f;
        private const float OVERSHOOT_MULTIPLIER = 2f;
        private const string PLAYER_TAG = "Player";

        private IMovable movableTarget;
        private EnemyController controller;
        private IMovementStrategy movementStrategy;
        private Vector3 moveDirection = Vector3.down;
        private Vector3? targetPosition = null;
        private float arriveThreshold = DEFAULT_ARRIVE_THRESHOLD;
        private Vector3 lastPosition;
        
        private Transform playerTransform;
        private bool playerSearchAttempted = false;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
            movableTarget = controller; // IMovableとして参照
            lastPosition = transform.position;
        }
        
        /// <summary>プレイヤーのTransformを取得</summary>
        private Transform GetPlayerTransform()
        {
            if (playerTransform != null) return playerTransform;
            if (!playerSearchAttempted)
            {
                playerSearchAttempted = true;
                GameObject playerObject = GameObject.FindWithTag(PLAYER_TAG);
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                    return playerTransform;
                }
                var playerWeaponSystem = FindFirstObjectByType<PlayerWeaponSystem>();
                if (playerWeaponSystem != null)
                {
                    playerTransform = playerWeaponSystem.transform;
                    return playerTransform;
                }
                var player = FindFirstObjectByType<SGC2025.PlayerCharacter>();
                if (player != null)
                {
                    playerTransform = player.transform;
                    return playerTransform;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 移動戦略を設定
        /// </summary>
        /// <param name="strategy">移動戦略</param>
        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            movementStrategy = strategy;
        }
        
        /// <summary>
        /// 目標位置をセット（固定位置移動用）
        /// </summary>
        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = target;
            Vector3 direction = target - transform.position;
            direction.z = 0f;
            moveDirection = direction.normalized;
        }

        private void Update()
        {
            if (movableTarget == null || !movableTarget.CanMove) return;
            
            float speed = movableTarget.MoveSpeed;
            
            if (targetPosition.HasValue)
            {
                MoveToFixedTarget(speed);
            }
            else if (movementStrategy != null)
            {
                Transform player = GetPlayerTransform();
                if (player != null)
                    movementStrategy.Move(transform, player, speed, Time.deltaTime);
                else
                {
                    Vector3 movement = moveDirection * speed * Time.deltaTime;
                    movement.z = 0f;
                    transform.Translate(movement);
                }
            }
            else
            {
                Vector3 movement = moveDirection * speed * Time.deltaTime;
                movement.z = 0f;
                transform.Translate(movement);
            }
        }
        
        /// <summary>
        /// 固定目標位置への移動処理
        /// </summary>
        private void MoveToFixedTarget(float speed)
        {
            Vector3 movement = moveDirection * speed * Time.deltaTime;
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
                ReturnToPool();
        }
        
        /// <summary>
        /// プールに返却
        /// </summary>
        private void ReturnToPool()
        {
            if (SGC2025.EnemyFactory.I != null)
            {
                SGC2025.EnemyFactory.I.ReturnEnemy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}