using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動削除を管理するコンポーネント
    /// 一定時間後、または画面外に出たときにプールに返却する
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
        private const float DEFAULT_ELAPSED_TIME = 0f;
        private const float IMMEDIATE_RETURN_TIME = 0f;
        private const string DEBUG_LOG_PREFIX = "[EnemyAutoReturn]";

        private float currentLifeTime;
        private float elapsedTime;
        private bool isInitialized = false;
        
        /// <summary>
        /// 初期化（生成時に呼ばれる）
        /// </summary>
        public void Initialize()
        {
            elapsedTime = DEFAULT_ELAPSED_TIME;
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
            Debug.LogError($"{DEBUG_LOG_PREFIX} EnemyDataSOが設定されていません！ GameObject: {gameObject.name}");
            Debug.LogError($"{DEBUG_LOG_PREFIX} EnemyDataSOを正しく設定してください。オブジェクトプールに即座に返却します。");
            
            currentLifeTime = IMMEDIATE_RETURN_TIME;
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
            // 時間経過またはマップ範囲外で判定
            return HasLifeTimeExpired() || IsOutOfMapBounds();
        }

        private bool HasLifeTimeExpired()
        {
            return elapsedTime >= currentLifeTime;
        }
        
        /// <summary>
        /// マップ範囲外に出たかをチェック
        /// </summary>
        private bool IsOutOfMapBounds()
        {
            if (GroundManager.I == null || GroundManager.I.MapData == null)
            {
                return false; // GroundManagerがない場合は時間でのみ判定
            }
            
            var mapData = GroundManager.I.MapData;
            Vector3 pos = transform.position;
            
            // マップの範囲外チェック（少し余裕を持たせる）
            float margin = 5f; // 完全に見えなくなってから返却
            return pos.x < -margin || 
                   pos.x > mapData.MapMaxWorldPosition.x + margin ||
                   pos.y < -margin || 
                   pos.y > mapData.MapMaxWorldPosition.y + margin;
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