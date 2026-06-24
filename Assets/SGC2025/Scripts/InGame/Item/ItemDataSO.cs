using System.Collections.Generic;
using UnityEngine;

namespace Tyotyo.InGame.Item
{
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
    }
}
