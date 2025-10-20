using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動削除を管理するコンポーネント
    /// 一定時間後、または画面外に出たときにプールに返却する
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
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
            var controller = GetComponent<EnemyController>();
            if (controller != null && controller.EnemyData != null)
            {
                currentLifeTime = controller.LifeTime;
                return;
            }
            
            // SOが設定されていない場合の警告
            Debug.LogError($"[EnemyAutoReturn] EnemyDataSOが設定されていません！ GameObject: {gameObject.name}");
            Debug.LogError("[EnemyAutoReturn] EnemyDataSOを正しく設定してください。オブジェクトプールに即座に返却します。");
            
            // 設定不備のため即座にプールに返却
            currentLifeTime = 0f;
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
        
        private bool ShouldReturnToPool()
        {
            // 時間経過のみで判定
            return HasLifeTimeExpired();
        }

        private bool HasLifeTimeExpired()
        {
            return elapsedTime >= currentLifeTime;
        }
        
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