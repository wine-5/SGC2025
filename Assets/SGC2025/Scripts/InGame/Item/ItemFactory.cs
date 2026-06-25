using Tyotyo.Systems;
using UnityEngine;
using Tyotyo.Core.Log;

namespace Tyotyo.InGame.Item
{
    /// <summary>
    /// アイテムの生成・プール管理を行うファクトリークラス
    /// ObjectPoolのラッパーとして機能。ItemManagerが参照を持つ。
    /// </summary>
    public class ItemFactory : MonoBehaviour
    {
        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
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
                CusLog.Warning("ItemFactory", "ItemData or ItemPrefab is null!");
                return null;
            }
            
            // ObjectPoolから取得
            GameObject itemObj = objectPool.GetObject(itemData.ItemPrefab, position, Quaternion.identity);
            
            if (itemObj == null)
            {
                CusLog.Warning("ItemFactory", $"Failed to spawn item: {itemData.ItemName}");
                return null;
            }
            
            // ItemControllerの初期化
            ItemController controller = itemObj.GetComponent<ItemController>();
            if (controller != null)
                controller.Initialize(itemData, this);
            
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
