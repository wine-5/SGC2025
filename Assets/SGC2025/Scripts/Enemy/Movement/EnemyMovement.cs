using UnityEngine;
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
        private float arriveThreshold = 0.5f; // 到達判定を緩和
        private Vector3 lastPosition; // 前フレームの位置

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
            lastPosition = transform.position;
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
            Vector3 direction = target - transform.position;
            direction.z = 0f; // Z軸を除外
            moveDirection = direction.normalized;
            

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
            else if (movementStrategy != null && global::Player.I != null)
            {
                movementStrategy.Move(transform, global::Player.PlayerTransform, speed, Time.deltaTime);
            }
            // どちらもない場合はデフォルト移動（下向き、XY平面のみ）
            else
            {
                Vector3 movement = moveDirection * speed * Time.deltaTime;
                movement.z = 0f; // Z軸移動を制限
                transform.Translate(movement);
            }
        }
        
        /// <summary>
        /// 固定目標位置への移動処理
        /// </summary>
        private void MoveToFixedTarget(float speed)
        {
            Vector3 movement = moveDirection * speed * Time.deltaTime;
            movement.z = 0f; // Z軸移動を制限
            
            // 前フレームの位置を記録
            lastPosition = transform.position;
            transform.position += movement;

            // 改善された到達判定
            Vector3 currentPos = transform.position;
            Vector3 targetPos = targetPosition.Value;
            currentPos.z = 0f;
            targetPos.z = 0f;
            
            float distanceToTarget = Vector3.Distance(currentPos, targetPos);
            
            // オーバーシュート検出: 目標位置を通り越したかチェック
            Vector3 lastPos = lastPosition;
            lastPos.z = 0f;
            float lastDistance = Vector3.Distance(lastPos, targetPos);
            bool overshot = distanceToTarget > lastDistance && lastDistance < arriveThreshold * 2f;
            
            // 到達判定またはオーバーシュート検出
            if (distanceToTarget < arriveThreshold || overshot)
            {

                ReturnToPool();
            }
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