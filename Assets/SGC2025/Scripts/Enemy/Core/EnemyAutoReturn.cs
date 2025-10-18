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
        [Header("自動削除設定")]
        [SerializeField] private float fixedDirectionLifeTime = 30f;    // 固定方向移動の生存時間
        [SerializeField] private float playerChaserLifeTime = 20f;      // プレイヤー追従型の生存時間
        [SerializeField] private bool useCustomLifeTime = false;        // カスタム時間を使用するか
        [SerializeField] private float customLifeTime = 15f;            // カスタム生存時間
        
        [Header("デバッグ情報")]
        [SerializeField] private float remainingTime = 0f;              // 残り時間（読み取り専用）
        
        private float currentLifeTime;
        private float elapsedTime;
        private bool isInitialized = false;
        
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
            // カスタム時間を使用する場合
            if (useCustomLifeTime)
            {
                currentLifeTime = customLifeTime;
                return;
            }
            
            var controller = GetComponent<EnemyController>();
            if (controller != null && controller.EnemyData != null)
            {
                // 各敵タイプ固有の生存時間を使用（推奨）
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
                        currentLifeTime = playerChaserLifeTime;
                    }
                    else
                    {
                        currentLifeTime = fixedDirectionLifeTime;
                    }
                }
                else
                {
                    // 最終フォールバック
                    currentLifeTime = fixedDirectionLifeTime;
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
            
            // デバッグ用の残り時間を更新
            remainingTime = Mathf.Max(0f, currentLifeTime - elapsedTime);
            

            
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
        
        /// <summary>
        /// 手動でライフタイムをリセット
        /// </summary>
        public void ResetLifeTime()
        {
            elapsedTime = 0f;
            remainingTime = currentLifeTime;
        }
        
        /// <summary>
        /// カスタムライフタイムを設定
        /// </summary>
        public void SetCustomLifeTime(float customTime)
        {
            useCustomLifeTime = true;
            customLifeTime = customTime;
            currentLifeTime = customTime;
            elapsedTime = 0f;
            remainingTime = currentLifeTime;
        }
    }
}