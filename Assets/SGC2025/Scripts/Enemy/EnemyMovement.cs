using UnityEngine;
using SGC2025.Enemy;
using SGC2025.Enemy.Interface;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の移動を管理するコンポーネント
    /// 移動戦略パターンを使用して異なる移動タイプを実現
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        private EnemyController controller;
        private IMovementStrategy movementStrategy;
        private Vector3 moveDirection = Vector3.down; // デフォルトは下向き
        private Vector3? targetPosition = null;
        private float arriveThreshold = 0.05f;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
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
            // 方向ベクトルを計算
            moveDirection = (target - transform.position).normalized;
        }

        private void Update()
        {
            if (controller == null || !controller.IsAlive) return;
            
            float speed = controller.MoveSpeed;
            
            // 固定目標位置がある場合（画面端への移動）
            if (targetPosition.HasValue)
            {
                MoveToFixedTarget(speed);
            }
            // 移動戦略がある場合（プレイヤー追従）
            else if (movementStrategy != null && Player.Instance != null)
            {
                movementStrategy.Move(transform, Player.PlayerTransform, speed, Time.deltaTime);
            }
            // どちらもない場合はデフォルト移動（下向き）
            else
            {
                transform.Translate(moveDirection * speed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// 固定目標位置への移動処理
        /// </summary>
        private void MoveToFixedTarget(float speed)
        {
            transform.position += moveDirection * speed * Time.deltaTime;

            // 目標位置に到達したらPoolに返却
            if (Vector3.Distance(transform.position, targetPosition.Value) < arriveThreshold)
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
}