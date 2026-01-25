using UnityEngine;
using System.Collections.Generic;
using SGC2025.Manager;

namespace SGC2025.Item
{
    /// <summary>
    /// アイテムの生成と効果管理を行うマネージャー
    /// </summary>
    public class ItemManager : Singleton<ItemManager>
    {
        private const float MIN_SPAWN_INTERVAL = 3f;
        
        [Header("アイテム抽選設定")]
        [SerializeField, Tooltip("アイテムの抽選を行うセレクター")]
        private ItemSpawnSelector spawnSelector = new ItemSpawnSelector();
        
        [Header("生成設定")]
        [SerializeField, Tooltip("アイテム生成間隔（秒）")]
        private float spawnInterval = 10f;
        
        [SerializeField, Tooltip("生成する高さのオフセット")]
        private float spawnHeightOffset = 0.5f;
        
        [SerializeField, Tooltip("自動生成を有効にする")]
        private bool autoSpawn = true;
        
        private float nextSpawnTime;
        private Dictionary<ItemType, ItemEffect> activeEffects = new Dictionary<ItemType, ItemEffect>();
        
        // イベント
        public static event System.Action<ItemType, float, float> OnItemEffectActivated; // (種類, 効果値, 持続時間)
        public static event System.Action<ItemType> OnItemEffectExpired; // (種類)
        
        protected override bool UseDontDestroyOnLoad => false;
        
        /// <summary>
        /// アイテム効果の状態
        /// </summary>
        private class ItemEffect
        {
            public ItemData data;
            public float startTime;
            public GameObject effectInstance;
        }
        
        protected override void Init()
        {
            base.Init();
            
            if (autoSpawn)
            {
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
        
        private void Update()
        {
            // 自動生成
            if (autoSpawn && Time.time >= nextSpawnTime)
            {
                SpawnRandomItem();
                nextSpawnTime = Time.time + Mathf.Max(spawnInterval, MIN_SPAWN_INTERVAL);
            }
            
            // 効果時間のチェック
            CheckEffectExpiration();
        }
        
        /// <summary>
        /// ランダムにアイテムを生成
        /// </summary>
        public void SpawnRandomItem()
        {
            if (spawnSelector.IsEmpty)
            {
                Debug.LogWarning("[ItemManager] ItemSpawnSelector is empty!");
                return;
            }
            
            // 重み付き抽選
            ItemData selectedItem = spawnSelector.SelectRandom();
            if (selectedItem == null) return;
            
            // ランダムな位置を取得
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // アイテムを生成
            SpawnItem(selectedItem, spawnPosition);
        }
        
        /// <summary>
        /// ランダムな生成位置を取得
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            // GroundManagerからマップのサイズを取得
            if (GroundManager.I != null && GroundManager.I.MapData != null)
            {
                var mapData = GroundManager.I.MapData;
                float mapWidth = mapData.columns * mapData.ActualCellWidth;
                float mapHeight = mapData.rows * mapData.ActualCellHeight;
                
                float randomX = Random.Range(0f, mapWidth);
                float randomY = Random.Range(0f, mapHeight);
                
                return new Vector3(randomX, randomY, 0f) + new Vector3(0f, spawnHeightOffset, 0f);
            }
            
            // マップ情報が取得できない場合のフォールバック
            Debug.LogWarning("[ItemManager] GroundManager or MapData is null. Using default spawn position.");
            return new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f) + spawnHeightOffset,
                0f
            );
        }
        
        /// <summary>
        /// アイテムを生成
        /// </summary>
        private void SpawnItem(ItemData itemData, Vector3 position)
        {
            if (ItemFactory.I == null)
            {
                Debug.LogWarning("[ItemManager] ItemFactory instance is null!");
                return;
            }
            
            GameObject itemObj = ItemFactory.I.SpawnItem(itemData, position);
        }
        
        /// <summary>
        /// アイテムを取得して効果を適用
        /// </summary>
        public void CollectItem(ItemData itemData)
        {
            if (itemData == null) return;
            
            // 既に同じ種類の効果が有効な場合は上書き
            if (activeEffects.ContainsKey(itemData.ItemType))
            {
                RemoveEffect(itemData.ItemType);
            }
            
            // 効果を適用
            ApplyEffect(itemData);
            
            Debug.Log($"[ItemManager] Collected: {itemData.ItemName}");
        }
        
        /// <summary>
        /// アイテム効果を適用
        /// </summary>
        private void ApplyEffect(ItemData itemData)
        {
            var effect = new ItemEffect
            {
                data = itemData,
                startTime = Time.time,
                effectInstance = null // TODO: エフェクト生成処理を後で追加
            };
            
            activeEffects[itemData.ItemType] = effect;
            
            // イベント通知
            OnItemEffectActivated?.Invoke(itemData.ItemType, itemData.EffectValue, itemData.Duration);
            
            // TODO: エフェクトの生成（ObjectPool、EffectFactoryを使用）
            // if (itemData.EffectPrefab != null)
            // {
            //     effect.effectInstance = Instantiate(itemData.EffectPrefab);
            // }
        }
        
        /// <summary>
        /// 効果時間の期限をチェック
        /// </summary>
        private void CheckEffectExpiration()
        {
            var expiredEffects = new List<ItemType>();
            
            foreach (var kvp in activeEffects)
            {
                float elapsedTime = Time.time - kvp.Value.startTime;
                if (elapsedTime >= kvp.Value.data.Duration)
                {
                    expiredEffects.Add(kvp.Key);
                }
            }
            
            foreach (var itemType in expiredEffects)
            {
                RemoveEffect(itemType);
            }
        }
        
        /// <summary>
        /// 効果を解除
        /// </summary>
        private void RemoveEffect(ItemType itemType)
        {
            if (!activeEffects.ContainsKey(itemType)) return;
            
            var effect = activeEffects[itemType];
            
            // エフェクトを削除
            if (effect.effectInstance != null)
            {
                Destroy(effect.effectInstance);
            }
            
            activeEffects.Remove(itemType);
            
            // イベント通知
            OnItemEffectExpired?.Invoke(itemType);
            
            Debug.Log($"[ItemManager] Effect expired: {itemType}");
        }
        
        /// <summary>
        /// 指定した種類のアイテムが有効か確認
        /// </summary>
        public bool IsEffectActive(ItemType itemType)
        {
            return activeEffects.ContainsKey(itemType);
        }
        
        /// <summary>
        /// 指定した種類のアイテムの残り時間を取得
        /// </summary>
        public float GetRemainingTime(ItemType itemType)
        {
            if (!activeEffects.ContainsKey(itemType)) return 0f;
            
            var effect = activeEffects[itemType];
            float elapsedTime = Time.time - effect.startTime;
            return Mathf.Max(0f, effect.data.Duration - elapsedTime);
        }
        
        /// <summary>
        /// 指定した種類のアイテムの効果値を取得
        /// </summary>
        public float GetEffectValue(ItemType itemType)
        {
            if (!activeEffects.ContainsKey(itemType)) return 1f;
            
            return activeEffects[itemType].data.EffectValue;
        }
        
        /// <summary>
        /// 生成間隔を設定
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(interval, MIN_SPAWN_INTERVAL);
        }
        
        /// <summary>
        /// 自動生成の有効/無効を切り替え
        /// </summary>
        public void SetAutoSpawn(bool enabled)
        {
            autoSpawn = enabled;
            if (enabled)
            {
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
    }
}
