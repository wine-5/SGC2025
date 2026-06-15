using SGC2025.Audio;
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

        private ItemData itemData;
        private float lifeTime;
        private float spawnTime;
        private ItemFactory factory;

        private void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (lifeTime > 0f && Time.time - spawnTime >= lifeTime)
                ReturnToPool();
        }

        /// <summary>
        /// アイテムデータを設定
        /// </summary>
        public void Initialize(ItemData data, ItemFactory itemFactory)
        {
            itemData = data;
            factory = itemFactory;
            lifeTime = data != null ? data.Duration : 0f;
            spawnTime = Time.time;
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
            if (itemData == null) return;
            
            AudioManager.I.PlaySE(SEType.GetItem);
            if (ItemManager.I != null)
                ItemManager.I.CollectItem(itemData);
        }
        
        /// <summary>
        /// アイテムをプールに返却
        /// </summary>
        private void ReturnToPool()
        {
            // 取得・寿命切れ共通の退場通知（クローンアイテムの生成数管理用）
            if (ItemManager.I != null)
                ItemManager.I.OnItemReturned(itemData);

            if (factory != null)
                factory.ReturnItem(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
