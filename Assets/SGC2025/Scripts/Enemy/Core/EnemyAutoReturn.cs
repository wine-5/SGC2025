using UnityEngine;
using SGC2025;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動削除を管理するコンポーネント
    /// 一定時間後、または画面外に出たときにプールに返却する
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
        #region 定数
        private const float DEFAULT_PLAYER_CHASER_LIFETIME = 20f;
        private const float DEFAULT_FIXED_DIRECTION_LIFETIME = 30f;
        #endregion
        
        #region フィールド
        private float currentLifeTime;
        private float elapsedTime;
        private bool isInitialized = false;
        #endregion
        
        /// <summary>
        /// 初期化（生成時に呼ばれる）
        /// </summary>
        public void Initialize()
        {
            elapsedTime = 0f;
            isInitialized = true;
            
            // 敵の種類に応じて生存時間を設定
            SetLifeTimeBasedOnEnemyType();
            

        }
        
        /// <summary>
        /// 敵の種類に応じて生存時間を設定
        /// </summary>
        private void SetLifeTimeBasedOnEnemyType()
        {
            var controller = GetComponent<EnemyController>();
            if (controller != null && controller.EnemyData != null)
            {
                // 各敵タイプ固有の生存時間を使用
                currentLifeTime = controller.LifeTime;
            }
            else
            {
                // フォールバック：移動タイプベースの時間設定
                if (controller != null && controller.EnemyData != null)
                {
                    MovementType movementType = controller.EnemyData.MovementType;
                    
                    if (IsPlayerChaserType(movementType))
                    {
                        currentLifeTime = DEFAULT_PLAYER_CHASER_LIFETIME;
                    }
                    else
                    {
                        currentLifeTime = DEFAULT_FIXED_DIRECTION_LIFETIME;
                    }
                }
                else
                {
                    // 最終フォールバック
                    currentLifeTime = DEFAULT_FIXED_DIRECTION_LIFETIME;
                }
            }
        }
        
        /// <summary>
        /// プレイヤー追従型の敵かどうかを判定
        /// </summary>
        private bool IsPlayerChaserType(MovementType movementType)
        {
            return movementType == MovementType.LinearChaser ||
                   movementType == MovementType.InertiaChaser ||
                   movementType == MovementType.PredictiveChaser ||
                   movementType == MovementType.ArcChaser;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // DeltaTimeで経過時間を累積
            elapsedTime += Time.deltaTime;
            
            if (ShouldReturnToPool())
            {
                ReturnToPool();
            }
        }
        
        /// <summary>
        /// プールに返却すべきかチェック
        /// </summary>
        private bool ShouldReturnToPool()
        {
            // 時間経過のみで判定
            return HasLifeTimeExpired();
        }
        
        /// <summary>
        /// 生存時間が経過したかチェック
        /// </summary>
        private bool HasLifeTimeExpired()
        {
            return elapsedTime >= currentLifeTime;
        }

        /// <summary>
        /// プールに返却
        /// </summary>
        private void ReturnToPool()
        {

            
            if (EnemyFactory.I != null)
            {
                EnemyFactory.I.ReturnEnemy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// プールから取得されたときの初期化
        /// </summary>
        private void OnEnable()
        {
            // プールから再取得された場合は再初期化
            if (isInitialized)
            {
                Initialize();
            }
        }
        
    }
}