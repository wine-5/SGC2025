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
        private Vector3? targetPosition = null;
        private float arriveThreshold = 0.2f;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
        }
        
        /// <summary>
        /// 目標位置をセット
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
            
            if (targetPosition.HasValue)
            {
                float speed = controller.MoveSpeed;
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
            else
            {
                // 目標位置未設定時は従来通り下方向に移動
                float speed = controller.MoveSpeed;
                transform.Translate(moveDirection * speed * Time.deltaTime);
            }
        }
    }
}