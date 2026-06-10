using System;
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

        [Header("生成設定")]
        [SerializeField, Tooltip("アイテムのPrefab")]
        private GameObject itemPrefab;

        [SerializeField, Tooltip("生成される確率の重み（大きいほど出やすい）")]
        private int spawnWeight = 1;

        public ItemType ItemType => itemType;
        public string ItemName => itemName;
        public string Description => description;
        public float Duration => duration;
        public float EffectValue => effectValue;
        public GameObject ItemPrefab => itemPrefab;
        public int SpawnWeight => spawnWeight;
    }
}
