using UnityEngine;
using TechC;

namespace SGC2025.Item
{
    /// <summary>
    /// アイテムの生成・プール管理を行うファクトリークラス
    /// ObjectPoolのラッパーとして機能
    /// </summary>
    public class ItemFactory : Singleton<ItemFactory>
    {
        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        /// <summary>
        /// アイテムを生成
        /// </summary>
        /// <param name="itemData">生成するアイテムのデータ</param>
        /// <param name="position">生成位置</param>
        /// <returns>生成されたアイテムのGameObject</returns>
        public GameObject SpawnItem(ItemData itemData, Vector3 position)
        {
            if (itemData == null || itemData.ItemPrefab == null)
            {
                Debug.LogWarning("[ItemFactory] ItemData or ItemPrefab is null!");
                return null;
            }
            
            // ObjectPoolから取得
            GameObject itemObj = objectPool.GetObject(itemData.ItemPrefab, position, Quaternion.identity);
            
            if (itemObj == null)
            {
                Debug.LogWarning($"[ItemFactory] Failed to spawn item: {itemData.ItemName}");
                return null;
            }
            
            // ItemControllerの初期化
            ItemController controller = itemObj.GetComponent<ItemController>();
            if (controller != null)
            {
                controller.Initialize(itemData);
            }
            else
            {
                Debug.LogWarning($"[ItemFactory] ItemController not found on {itemData.ItemName}");
            }
            
            return itemObj;
        }
        
        /// <summary>
        /// アイテムをプールに返却
        /// </summary>
        /// <param name="itemObj">返却するアイテムのGameObject</param>
        public void ReturnItem(GameObject itemObj)
        {
            if (itemObj == null) return;
            
            objectPool.ReturnObject(itemObj);
        }
    }
}
