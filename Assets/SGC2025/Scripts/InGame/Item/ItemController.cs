using SGC2025.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SGC2025.Item
{
    /// <summary>
    /// アイテムオブジェクトの動作を制御するクラス
    /// フィールドに配置されたアイテムの振る舞いを管理
    /// </summary>
    public class ItemController : MonoBehaviour
    {
        [Header("デバック用")]
        [SerializeField, Tooltip("デバッグ用アイテム生成を有効にする")]
        private bool enableDebugSpawn = false;
        
        [SerializeField, Tooltip("デバッグ用アイテム生成キー")]
        private KeyCode debugSpawnKey = KeyCode.I;
        
        [SerializeField, Tooltip("Playerからの生成距離")]
        private float debugSpawnDistance = 2f;
        
        [SerializeField] private GameObject testItemObj;
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
            // デバッグ用アイテム生成
            if (enableDebugSpawn && IsDebugKeyPressed())
            {
                SpawnDebugItemNearPlayer();
            }
            
            // 回転アニメーション
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            
            if (lifeTime > 0f && Time.time - spawnTime >= lifeTime)
                ReturnToPool();
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
            
            if (ItemManager.I != null)
                ItemManager.I.CollectItem(itemData);
            else
                Debug.LogWarning("[ItemController] ItemManager instance is null!");
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
        
        /// <summary>
        /// デバッグ用：Playerの近くにランダムアイテムを生成
        /// </summary>
        private void SpawnDebugItemNearPlayer()
        {
            if (PlayerDataProvider.I == null)
            {
                Debug.LogWarning("[ItemController] PlayerDataProvider not found!");
                return;
            }
            
            if (ItemManager.I == null)
            {
                Debug.LogWarning("[ItemController] ItemManager not found!");
                return;
            }
            
            Vector3 playerPos = PlayerDataProvider.I.GetPlayerPosition();
            
            // Playerの周りにランダムな位置を生成
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spawnOffset = new Vector3(
                Mathf.Cos(randomAngle) * debugSpawnDistance,
                Mathf.Sin(randomAngle) * debugSpawnDistance,
                0f
            );
            
            Vector3 spawnPosition = playerPos + spawnOffset;
            
            // ItemManagerを使ってアイテムを生成
            ItemManager.I.SpawnRandomItemAt(spawnPosition);
            
            Debug.Log($"[ItemController] Debug item spawned at {spawnPosition} (Player: {playerPos})");
        }
        
        /// <summary>
        /// デバッグキーが押されたかチェック（新Input System対応）
        /// </summary>
        private bool IsDebugKeyPressed()
        {
            if (Keyboard.current == null) return false;
            
            // debugSpawnKeyに応じて適切なキーをチェック
            return debugSpawnKey switch
            {
                KeyCode.I => Keyboard.current.iKey.wasPressedThisFrame,
                KeyCode.O => Keyboard.current.oKey.wasPressedThisFrame,
                KeyCode.P => Keyboard.current.pKey.wasPressedThisFrame,
                KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
                KeyCode.F1 => Keyboard.current.f1Key.wasPressedThisFrame,
                KeyCode.F2 => Keyboard.current.f2Key.wasPressedThisFrame,
                KeyCode.F3 => Keyboard.current.f3Key.wasPressedThisFrame,
                _ => false // サポートされていないキーの場合はfalse
            };
        }
    }
}
