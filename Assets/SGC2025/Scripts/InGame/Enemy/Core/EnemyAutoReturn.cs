using UnityEngine;
using SGC2025.Manager;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動返却管理
    /// 一定時間経過またはマップ範囲外に出た際にプールに返却
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
        private const float DEFAULT_ELAPSED_TIME = 0f;
        private const float IMMEDIATE_RETURN_TIME = 0f;
        private const float OUT_OF_BOUNDS_MARGIN = 5f;

        private float currentLifeTime;
        private float elapsedTime;
        private bool isInitialized;
        
        public void Initialize()
        {
            elapsedTime = DEFAULT_ELAPSED_TIME;
            isInitialized = true;
            SetLifeTimeBasedOnEnemyType();
        }
        
        private void SetLifeTimeBasedOnEnemyType()
        {
            var controller = GetComponent<EnemyController>();
            if (controller != null && controller.EnemyData != null)
            {
                currentLifeTime = controller.LifeTime;
                return;
            }
            
            currentLifeTime = IMMEDIATE_RETURN_TIME;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            elapsedTime += Time.deltaTime;
            
            if (ShouldReturnToPool())
                ReturnToPool();
        }
        
        private bool ShouldReturnToPool() => HasLifeTimeExpired() || IsOutOfMapBounds();
        
        private bool HasLifeTimeExpired() => elapsedTime >= currentLifeTime;
        
        private bool IsOutOfMapBounds()
        {
            if (GroundManager.I == null || GroundManager.I.MapData == null) return false;
            var mapData = GroundManager.I.MapData;
            Vector3 pos = transform.position;
            
            return pos.x < -OUT_OF_BOUNDS_MARGIN || 
                   pos.x > mapData.MapMaxWorldPosition.x + OUT_OF_BOUNDS_MARGIN ||
                   pos.y < -OUT_OF_BOUNDS_MARGIN || 
                   pos.y > mapData.MapMaxWorldPosition.y + OUT_OF_BOUNDS_MARGIN;
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
            if (isInitialized)
                Initialize();
        }
    }
}