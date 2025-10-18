using UnityEngine;
using SGC2025.Enemy;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の基本移動を管理するコンポーネント
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        private EnemyController controller;
        private Vector3 moveDirection = Vector3.down; // デフォルトは下向き
        
        private void Awake()
        {
            controller = GetComponent<EnemyController>();
        }
        
        private void Update()
        {
            if (controller == null || !controller.IsAlive) return;
            
            // 移動処理
            float speed = controller.MoveSpeed;
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }
        
        /// <summary>
        /// 移動方向を設定
        /// </summary>
        public void SetMoveDirection(Vector3 direction)
        {
            moveDirection = direction.normalized;
        }
        
        /// <summary>
        /// プレイヤー追従移動（将来の拡張用）
        /// </summary>
        public void SetTargetPosition(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            moveDirection = direction;
        }
    }
}