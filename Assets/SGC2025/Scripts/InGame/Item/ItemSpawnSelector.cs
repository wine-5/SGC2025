using System;
using System.Linq;
using UnityEngine;

namespace SGC2025.Item
{
    /// <summary>
    /// アイテムの重み付きランダム抽選を行うクラス
    /// </summary>
    [Serializable]
    public class ItemSpawnSelector
    {
        [SerializeField, Tooltip("抽選対象のアイテムデータSO")]
        private ItemDataSO itemDataSO;
        
        /// <summary>
        /// アイテムリストが空かどうか
        /// </summary>
        public bool IsEmpty => itemDataSO == null || itemDataSO.ItemList == null || itemDataSO.ItemList.Count == 0;
        
        /// <summary>
        /// 重み付きランダム抽選を実行
        /// </summary>
        /// <returns>抽選されたアイテムデータ（抽選失敗時はnull）</returns>
        public ItemData SelectRandom()
        {
            if (IsEmpty) return null;
            
            var itemList = itemDataSO.ItemList;
            
            int totalWeight = itemList.Sum(item => item.SpawnWeight);
            if (totalWeight <= 0) return null;
            
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var item in itemList)
            {
                currentWeight += item.SpawnWeight;
                if (randomValue < currentWeight)
                {
                    return item;
                }
            }
            
            return itemList[0];
        }
        
        /// <summary>
        /// ItemDataSOを設定
        /// </summary>
        public void SetItemDataSO(ItemDataSO dataSO)
        {
            itemDataSO = dataSO;
        }
    }
}
