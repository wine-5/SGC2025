using UnityEngine;

namespace SGC2025.Item
{
    /// <summary>
    /// アイテムオブジェクトの動作を制御するクラス
    /// フィールドに配置されたアイテムの振る舞いを管理
    /// </summary>
    public class ItemController : MonoBehaviour
    {
        [Header("判定設定")]
        [SerializeField, Tooltip("プレイヤーのレイヤー")]
        private LayerMask playerLayer;
        
        [Header("動作設定")]
        [SerializeField, Tooltip("アイテムの回転速度")]
        private float rotationSpeed = 50f;
        
        [SerializeField, Tooltip("自動消滅までの時間（0で無効）")]
        private float lifeTime = 30f;
        
        private ItemData itemData;
        private float spawnTime;
        
        public ItemData ItemData => itemData;
        
        private void Start()
        {
            spawnTime = Time.time;
        }
        
        private void Update()
        {
            // 回転アニメーション
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            
            // 自動消滅チェック
            if (lifeTime > 0f && Time.time - spawnTime >= lifeTime)
            {
                ReturnToPool();
            }
        }
        
        /// <summary>
        /// アイテムデータを設定
        /// </summary>
        public void Initialize(ItemData data)
        {
            itemData = data;
        }
        
        /// <summary>
        /// プレイヤーとの衝突判定
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Layerで判定
            if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;
            
            // アイテム取得処理
            OnItemCollected();
            
            // アイテムをプールに返却
            ReturnToPool();
        }
        
        /// <summary>
        /// アイテム取得時の処理
        /// </summary>
        private void OnItemCollected()
        {
            if (itemData == null)
            {
                Debug.LogWarning("[ItemController] ItemData is null!");
                return;
            }
            
            // ItemManagerにアイテム取得を通知
            if (ItemManager.I != null)
            {
                ItemManager.I.CollectItem(itemData);
            }
            else
            {
                Debug.LogWarning("[ItemController] ItemManager instance is null!");
            }
        }
        
        /// <summary>
        /// アイテムをプールに返却
        /// </summary>
        private void ReturnToPool()
        {
            if (ItemFactory.I != null)
            {
                ItemFactory.I.ReturnItem(gameObject);
            }
            else
            {
                Debug.LogError("[ItemController] ItemFactory is not available! Cannot return item to pool.");
                gameObject.SetActive(false);
            }
        }
    }
}
