using System;
using System.Collections.Generic;
using UnityEngine;

namespace SGC2025.Item
{
    /// <summary>
    /// 個別のアイテムデータ
    /// </summary>
    [Serializable]
    public class ItemData
    {
        [Header("基本設定")]
        [SerializeField, Tooltip("アイテムの種類")]
        private ItemType itemType;
        
        [SerializeField, Tooltip("アイテム名")]
        private string itemName;
        
        [SerializeField, Tooltip("アイテムの説明")]
        private string description;
        
        [Header("効果設定")]
        [SerializeField, Tooltip("効果の持続時間（秒）")]
        private float duration = 10f;
        
        [SerializeField, Tooltip("効果の強さ（移動速度の倍率 or スコアの倍率）")]
        private float effectValue = 1.5f;
        
        [Header("エフェクト設定")]
        [SerializeField, Tooltip("アイテム適用中に表示されるエフェクトのPrefab")]
        private GameObject effectPrefab;
        
        [Header("生成設定")]
        [SerializeField, Tooltip("アイテムのPrefab")]
        private GameObject itemPrefab;
        
        [SerializeField, Tooltip("生成される確率の重み（大きいほど出やすい）")]
        private int spawnWeight = 1;
        
        // プロパティ
        public ItemType ItemType => itemType;
        public string ItemName => itemName;
        public string Description => description;
        public float Duration => duration;
        public float EffectValue => effectValue;
        public GameObject EffectPrefab => effectPrefab;
        public GameObject ItemPrefab => itemPrefab;
        public int SpawnWeight => spawnWeight;
    }
    
    /// <summary>
    /// アイテムのデータを管理するScriptableObject
    /// 複数のアイテムをまとめて管理
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDataSO", menuName = "SGC2025/Item Data", order = 0)]
    public class ItemDataSO : ScriptableObject
    {
        [SerializeField, Tooltip("管理するアイテムのリスト")]
        private List<ItemData> itemList = new List<ItemData>();
        
        /// <summary>
        /// アイテムリストを取得
        /// </summary>
        public List<ItemData> ItemList => itemList;
        
        /// <summary>
        /// 指定したタイプのアイテムを取得
        /// </summary>
        public ItemData GetItemByType(ItemType type)
        {
            return itemList.Find(item => item.ItemType == type);
        }
        
        /// <summary>
        /// アイテムを追加
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item != null && !itemList.Contains(item))
            {
                itemList.Add(item);
            }
        }
        
        /// <summary>
        /// アイテムを削除
        /// </summary>
        public void RemoveItem(ItemData item)
        {
            if (item != null)
            {
                itemList.Remove(item);
            }
        }
    }
}
