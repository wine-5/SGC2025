using UnityEngine;
using System.Collections.Generic;
using Tyotyo.Core;
using Tyotyo.Manager;
using Tyotyo.Effect;

namespace Tyotyo.InGame.Item
{
    /// <summary>
    /// アイテムの生成と効果管理を行うマネージャー
    /// </summary>
    public class ItemManager : Singleton<ItemManager>
    {
        private const float MIN_SPAWN_INTERVAL = 0.1f;
        private const float DEFAULT_SPAWN_RANGE = 10f;
        private const float GREENING_PARTICLE_DURATION = 1.5f;
        private const float CLONE_EFFECT_DURATION = 1.5f;
        private const string GAUGE_TAG = "GreenGauge";
        
        [Header("アイテム抽選設定")]
        [SerializeField, Tooltip("アイテムの抽選を行うセレクター")]
        private ItemSpawnSelector spawnSelector = new ItemSpawnSelector();
        
        [Header("生成設定")]
        [SerializeField, Range(0.1f, 60f), Tooltip("アイテム生成間隔（秒）")]
        private float spawnInterval = 10f;
        
        [SerializeField, Tooltip("生成する高さのオフセット")]
        private float spawnHeightOffset = 0.5f;
        
        [SerializeField, Tooltip("自動生成を有効にする")]
        private bool autoSpawn = true;

        [Header("ファクトリー参照")]
        [SerializeField] private ItemFactory itemFactory;

        private float nextSpawnTime;
        private Transform gaugeTarget;
        private int pendingCloneItems; // フィールドに存在する未取得のクローンアイテム数
        private Tyotyo.InGame.Player.PlayerCloneManager cloneManager;
        private Dictionary<ItemType, ItemEffect> activeEffects = new Dictionary<ItemType, ItemEffect>();
        
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
                nextSpawnTime = Time.time + spawnInterval;
            
            // 敵撃破イベントを購読して広範囲緑化効果を適用
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            base.OnDestroy();
        }
        
        /// <summary>
        /// 敵撃破時の処理
        /// </summary>
        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            // 敵撃破時パーティクルエフェクトを敵の位置に生成し、緑化度ゲージへ向かって飛ばす
            // （地面の緑化サイズ判定・実行は GroundManager 側で行う）
            if (EffectFactory.I != null)
                EffectFactory.I.CreateEffect(EffectType.GreeningParticle, e.Position, GREENING_PARTICLE_DURATION);
        }

        /// <summary>緑化範囲上昇アイテムが有効か（GroundManagerが緑化サイズ判定に使用）</summary>
        public bool IsAreaGreenifyActive => IsEffectActive(ItemType.AreaGreenify);

        /// <summary>緑化度ゲージのTransformを取得（キャッシュ）</summary>
        private Transform GetGaugeTarget()
        {
            // TODO: Findをやめて実装する
            if (gaugeTarget == null)
            {
                GameObject gaugeObject = GameObject.FindWithTag(GAUGE_TAG);
                if (gaugeObject != null)
                    gaugeTarget = gaugeObject.transform;
            }
            return gaugeTarget;
        }
        
        private void Update()
        {
            if (autoSpawn && Time.time >= nextSpawnTime)
            {
                SpawnRandomItem();
                nextSpawnTime = Time.time + Mathf.Max(spawnInterval, MIN_SPAWN_INTERVAL);
            }
            
            CheckEffectExpiration();
        }
        
        /// <summary>
        /// ランダムにアイテムを生成
        /// </summary>
        private void SpawnRandomItem()
        {
            if (spawnSelector.IsEmpty) return;

            // クローンアイテムは「残り必要数」だけ出すよう抽選を絞り込む
            ItemData selectedItem = spawnSelector.SelectRandom(CanSpawnItem);
            if (selectedItem == null) return;

            // ランダムな位置を取得
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // アイテムを生成
            SpawnItem(selectedItem, spawnPosition);

            if (selectedItem.ItemType == ItemType.PlayerClone)
                pendingCloneItems++;
        }

        /// <summary>そのアイテムを今生成してよいか（クローンは アクティブ数＋未取得数 が最大未満のときのみ）</summary>
        private bool CanSpawnItem(ItemData item)
        {
            if (item.ItemType != ItemType.PlayerClone) return true;

            var manager = GetCloneManager();
            if (manager == null) return false; // プレイヤー未準備ならクローンは出さない

            return manager.ActiveCloneCount + pendingCloneItems < manager.MaxCloneCount;
        }

        /// <summary>PlayerCloneManagerをPlayerDataProvider経由で取得（キャッシュ）</summary>
        private Tyotyo.InGame.Player.PlayerCloneManager GetCloneManager()
        {
            if (cloneManager == null
                && Tyotyo.InGame.Player.PlayerDataProvider.I != null
                && Tyotyo.InGame.Player.PlayerDataProvider.I.IsPlayerRegistered)
            {
                cloneManager = Tyotyo.InGame.Player.PlayerDataProvider.I.PlayerTransform.GetComponent<Tyotyo.InGame.Player.PlayerCloneManager>();
            }
            return cloneManager;
        }

        /// <summary>フィールドのアイテムがプールへ返却された通知（取得・寿命切れ共通。生成数管理用）</summary>
        public void OnItemReturned(ItemData itemData)
        {
            if (itemData == null) return;

            if (itemData.ItemType == ItemType.PlayerClone)
                pendingCloneItems = Mathf.Max(0, pendingCloneItems - 1);
        }
        
        
        /// <summary>
        /// ランダムな生成位置を取得
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            if (GroundManager.I != null && GroundManager.I.MapData != null)
            {
                var mapData = GroundManager.I.MapData;
                Vector2 maxWorldPos = mapData.MapMaxWorldPosition;
                
                float randomX = Random.Range(0f, maxWorldPos.x);
                float randomY = Random.Range(0f, maxWorldPos.y);
                
                return new Vector3(randomX, randomY, 0f) + new Vector3(0f, spawnHeightOffset, 0f);
            }
            
            return new Vector3(
                Random.Range(-DEFAULT_SPAWN_RANGE, DEFAULT_SPAWN_RANGE),
                Random.Range(-DEFAULT_SPAWN_RANGE, DEFAULT_SPAWN_RANGE) + spawnHeightOffset,
                0f
            );
        }
        
        /// <summary>
        /// アイテムを生成
        /// </summary>
        private void SpawnItem(ItemData itemData, Vector3 position)
        {
            if (itemFactory == null) return;
            itemFactory.SpawnItem(itemData, position);
        }

        /// <summary>
        /// アイテムを取得して効果を適用
        /// </summary>
        public void CollectItem(ItemData itemData)
        {
            if (itemData == null) return;

            // 種類を問わず取得を通知（画面エッジ演出などの取得フィードバック用）
            EventBus.Publish(new ItemCollectedEvent(itemData.ItemType));

            // クローンアイテムは時限効果ではなく、クローンを1体増やす専用処理
            if (itemData.ItemType == ItemType.PlayerClone)
            {
                var manager = GetCloneManager();
                // 実際にクローンが増えたときだけプレイヤーからエフェクトを出す
                if (manager != null && manager.TryActivateNextClone())
                    SpawnPlayerEffect(EffectType.PlayerCloneEffect, CLONE_EFFECT_DURATION);
                return;
            }

            if (activeEffects.ContainsKey(itemData.ItemType))
                RemoveEffect(itemData.ItemType);

            ApplyEffect(itemData);
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
                effectInstance = null
            };
            
            activeEffects[itemData.ItemType] = effect;
            
            EventBus.Publish(new ItemEffectActivatedEvent(itemData.ItemType, itemData.EffectValue, itemData.Duration));
            
            if (Tyotyo.InGame.Player.PlayerDataProvider.I != null && Tyotyo.InGame.Player.PlayerDataProvider.I.IsPlayerRegistered)
            {
                var playerTransform = Tyotyo.InGame.Player.PlayerDataProvider.I.PlayerTransform;
                Vector3 playerPos = playerTransform.position;
                
                // アイテムタイプに応じてエフェクト生成を判定
                switch (itemData.ItemType)
                {
                    case ItemType.SpeedBoost:
                        effect.effectInstance = EffectFactory.I.CreateEffect(EffectType.SpeedBoostEffect, playerPos, itemData.Duration, playerTransform);
                        break;

                    case ItemType.AreaGreenify:
                        // 広範囲緑化アイテムは持続効果（一定時間、敵撃破時に9マス緑化）＋エフェクト生成
                        effect.effectInstance = EffectFactory.I.CreateEffect(EffectType.AreaGreenifyEffect, playerPos, itemData.Duration, playerTransform);
                        break;

                    default:
                        Debug.LogWarning($"[ItemManager] ItemType {itemData.ItemType} のエフェクト処理が未実装です");
                        break;
                }
            }
        }
        
        /// <summary>
        /// プレイヤーの位置からエフェクトを生成する（プレイヤーに追従）。
        /// </summary>
        private GameObject SpawnPlayerEffect(EffectType effectType, float duration)
        {
            if (EffectFactory.I == null
                || Tyotyo.InGame.Player.PlayerDataProvider.I == null
                || !Tyotyo.InGame.Player.PlayerDataProvider.I.IsPlayerRegistered)
                return null;

            var playerTransform = Tyotyo.InGame.Player.PlayerDataProvider.I.PlayerTransform;
            return EffectFactory.I.CreateEffect(effectType, playerTransform.position, duration, playerTransform);
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
            
            if (effect.effectInstance != null && EffectFactory.I != null)
            {
                EffectFactory.I.ReturnEffect(effect.effectInstance);
            }
            
            activeEffects.Remove(itemType);
            
            EventBus.Publish(new ItemEffectExpiredEvent(itemType));
        }
        
        /// <summary>
        /// 指定した種類のアイテムが有効か確認
        /// </summary>
        private bool IsEffectActive(ItemType itemType) => activeEffects.ContainsKey(itemType);
    }
}
