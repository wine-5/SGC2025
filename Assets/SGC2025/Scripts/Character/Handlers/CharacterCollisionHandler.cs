using UnityEngine;
using System;
using System.Collections.Generic;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクターの衝突処理を専門に管理するクラス
    /// 敵との接触判定、アイテム取得処理を担当
    /// </summary>
    public class CharacterCollisionHandler : MonoBehaviour
    {
        #region 定数
        private const int ALL_LAYERS = -1;
        private const float DEFAULT_KNOCKBACK_FORCE = 2f;
        private const float DEFAULT_KNOCKBACK_DURATION = 0.2f;
        private const float DEFAULT_ITEM_COLLECTION_RADIUS = 0.5f;
        private const float DEFAULT_MAGNET_RANGE = 2f;
        private const float DEFAULT_MAGNET_SPEED = 5f;
        private const float DEFAULT_COLLISION_COOLDOWN = 0.5f;
        private const string DEBUG_LOG_PREFIX = "[CharacterCollisionHandler]";
        #endregion

        #region Inspector設定
        [Header("衝突設定")]
        [SerializeField] private LayerMask enemyLayerMask = ALL_LAYERS;
        [SerializeField] private LayerMask itemLayerMask = ALL_LAYERS;
        [SerializeField] private LayerMask boundaryLayerMask = ALL_LAYERS;
        [SerializeField] private bool enableCollisionDetection = true;

        [Header("衝突応答")]
        [SerializeField] private float knockbackForce = DEFAULT_KNOCKBACK_FORCE;
        [SerializeField] private float knockbackDuration = DEFAULT_KNOCKBACK_DURATION;
        [SerializeField] private bool enableKnockback = true;

        [Header("アイテム収集")]
        [SerializeField] private float itemCollectionRadius = DEFAULT_ITEM_COLLECTION_RADIUS;
        [SerializeField] private bool autoCollectItems = true;
        [SerializeField] private float magnetRange = DEFAULT_MAGNET_RANGE;
        [SerializeField] private float magnetSpeed = DEFAULT_MAGNET_SPEED;

        [Header("デバッグ")]
        [SerializeField] private bool showCollisionGizmos = true;
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private Color enemyCollisionColor = Color.red;
        [SerializeField] private Color itemCollectionColor = Color.green;
        #endregion

        #region プライベート変数
        private CharacterController characterController;
        private Collider2D characterCollider;
        private Rigidbody2D characterRigidbody;
        
        // ノックバック関連
        private bool isKnockedBack = false;
        private float knockbackTimer = 0f;
        private Vector3 knockbackVelocity = Vector3.zero;

        // 衝突追跡（同じオブジェクトとの連続衝突を防ぐ）
        private Dictionary<GameObject, float> lastCollisionTimes = new Dictionary<GameObject, float>();
        private float collisionCooldown = DEFAULT_COLLISION_COOLDOWN;
        #endregion

        #region イベント
        /// <summary>敵との衝突時のイベント</summary>
        public event Action<GameObject> OnEnemyCollision;
        
        /// <summary>アイテム取得時のイベント</summary>
        public event Action<GameObject> OnItemCollected;
        
        /// <summary>境界との衝突時のイベント</summary>
        public event Action<Vector3> OnBoundaryCollision;
        
        /// <summary>ノックバック開始時のイベント</summary>
        public event Action<Vector3> OnKnockbackStarted;
        #endregion

        #region プロパティ
        /// <summary>衝突検出が有効かどうか</summary>
        public bool IsCollisionEnabled => enableCollisionDetection;
        
        /// <summary>ノックバック中かどうか</summary>
        public bool IsKnockedBack => isKnockedBack;
        
        /// <summary>アイテム収集範囲</summary>
        public float ItemCollectionRadius => itemCollectionRadius;
        
        /// <summary>マグネット範囲</summary>
        public float MagnetRange => magnetRange;
        #endregion

        #region ハンドラーライフサイクル
        /// <summary>
        /// CharacterControllerによる初期化
        /// </summary>
        /// <param name="controller">親のCharacterController</param>
        public void Initialize(CharacterController controller)
        {
            characterController = controller;
            characterCollider = GetComponent<Collider2D>();
            characterRigidbody = GetComponent<Rigidbody2D>();

            ValidateComponents();
        }

        /// <summary>
        /// コンポーネントの妥当性チェック
        /// </summary>
        private void ValidateComponents()
        {
            if (characterCollider == null)
            {
                Debug.LogError($"{DEBUG_LOG_PREFIX} Collider2D not found on {gameObject.name}");
                return;
            }

            // Triggerとして設定されていることを確認
            if (!characterCollider.isTrigger)
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} Collider on {gameObject.name} should be set as Trigger");
            }

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Initialized for {gameObject.name}");
            }
        }

        public void OnStart() { }

        public void OnEnable() { }

        public void OnDisable() { }
        #endregion

        #region Update Processing
        private void Update()
        {
            UpdateKnockback();
            UpdateItemMagnetism();
            CleanupCollisionHistory();
        }

        /// <summary>
        /// ノックバック処理の更新
        /// </summary>
        private void UpdateKnockback()
        {
            if (!isKnockedBack) return;

            knockbackTimer -= Time.deltaTime;
            
            if (knockbackTimer <= 0f)
            {
                EndKnockback();
            }
            else
            {
                // ノックバック速度を徐々に減衰
                float normalizedTime = 1f - (knockbackTimer / knockbackDuration);
                float currentKnockbackStrength = Mathf.Lerp(1f, 0f, normalizedTime);
                
                if (characterRigidbody != null)
                {
                    characterRigidbody.linearVelocity = knockbackVelocity * currentKnockbackStrength;
                }
            }
        }

        /// <summary>
        /// アイテムマグネット効果の更新
        /// </summary>
        private void UpdateItemMagnetism()
        {
            if (!autoCollectItems) return;

            // 近くのアイテムを検索してマグネット効果を適用
            Collider2D[] nearbyItems = Physics2D.OverlapCircleAll(
                transform.position, 
                magnetRange, 
                itemLayerMask
            );

            foreach (var itemCollider in nearbyItems)
            {
                if (itemCollider == null || itemCollider.gameObject == gameObject) continue;

                float distance = Vector3.Distance(transform.position, itemCollider.transform.position);
                
                if (distance <= itemCollectionRadius)
                {
                    // 収集範囲内の場合は即座に取得
                    CollectItem(itemCollider.gameObject);
                }
                else if (distance <= magnetRange)
                {
                    // マグネット範囲内の場合はプレイヤーに向けて移動
                    Vector3 direction = (transform.position - itemCollider.transform.position).normalized;
                    itemCollider.transform.position += direction * magnetSpeed * Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 衝突履歴のクリーンアップ
        /// </summary>
        private void CleanupCollisionHistory()
        {
            var keysToRemove = new List<GameObject>();
            foreach (var kvp in lastCollisionTimes)
            {
                if (kvp.Key == null || Time.time - kvp.Value > collisionCooldown * 2f)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                lastCollisionTimes.Remove(key);
            }
        }
        #endregion

        #region 衝突検出
        /// <summary>
        /// Triggerエリアに入った時の処理
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enableCollisionDetection || other == null) return;

            ProcessCollision(other, CollisionType.Enter);
        }

        /// <summary>
        /// Triggerエリア内にいる間の処理
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enableCollisionDetection || other == null) return;

            ProcessCollision(other, CollisionType.Stay);
        }

        /// <summary>
        /// Triggerエリアから出た時の処理
        /// </summary>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enableCollisionDetection || other == null) return;

            ProcessCollision(other, CollisionType.Exit);
        }

        /// <summary>
        /// 衝突処理のメイン関数
        /// </summary>
        /// <param name="other">衝突相手のCollider</param>
        /// <param name="collisionType">衝突の種類</param>
        private void ProcessCollision(Collider2D other, CollisionType collisionType)
        {
            GameObject otherObject = other.gameObject;

            // 連続衝突の防止チェック
            if (collisionType == CollisionType.Enter && IsRecentCollision(otherObject))
            {
                return;
            }

            // 敵との衝突処理
            if (IsInLayerMask(other.gameObject.layer, enemyLayerMask))
            {
                HandleEnemyCollision(otherObject, collisionType);
            }
            // アイテムとの衝突処理
            else if (IsInLayerMask(other.gameObject.layer, itemLayerMask))
            {
                HandleItemCollision(otherObject, collisionType);
            }
            // 境界との衝突処理
            else if (IsInLayerMask(other.gameObject.layer, boundaryLayerMask))
            {
                HandleBoundaryCollision(other.ClosestPoint(transform.position), collisionType);
            }
        }

        /// <summary>
        /// 最近衝突したオブジェクトかどうかをチェック
        /// </summary>
        /// <param name="obj">チェックするオブジェクト</param>
        /// <returns>最近衝突している場合true</returns>
        private bool IsRecentCollision(GameObject obj)
        {
            return lastCollisionTimes.ContainsKey(obj) && 
                   Time.time - lastCollisionTimes[obj] < collisionCooldown;
        }

        /// <summary>
        /// レイヤーマスクに含まれているかチェック
        /// </summary>
        /// <param name="layer">チェックするレイヤー</param>
        /// <param name="layerMask">レイヤーマスク</param>
        /// <returns>含まれている場合true</returns>
        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
        #endregion

        #region 衝突処理
        /// <summary>
        /// 敵との衝突処理
        /// </summary>
        /// <param name="enemy">敵オブジェクト</param>
        /// <param name="collisionType">衝突の種類</param>
        private void HandleEnemyCollision(GameObject enemy, CollisionType collisionType)
        {
            if (collisionType != CollisionType.Enter) return;

            // 衝突履歴を記録
            lastCollisionTimes[enemy] = Time.time;

            // ダメージ処理
            if (characterController?.Health != null)
            {
                characterController.Health.TakeDamage(1);
            }

            // ノックバック処理
            if (enableKnockback)
            {
                ApplyKnockback(enemy.transform.position);
            }

            // イベント通知
            OnEnemyCollision?.Invoke(enemy);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Collided with enemy: {enemy.name}");
            }
        }

        /// <summary>
        /// アイテムとの衝突処理
        /// </summary>
        /// <param name="item">アイテムオブジェクト</param>
        /// <param name="collisionType">衝突の種類</param>
        private void HandleItemCollision(GameObject item, CollisionType collisionType)
        {
            if (collisionType != CollisionType.Enter) return;

            CollectItem(item);
        }

        /// <summary>
        /// 境界との衝突処理
        /// </summary>
        /// <param name="collisionPoint">衝突点</param>
        /// <param name="collisionType">衝突の種類</param>
        private void HandleBoundaryCollision(Vector3 collisionPoint, CollisionType collisionType)
        {
            if (collisionType != CollisionType.Enter) return;

            OnBoundaryCollision?.Invoke(collisionPoint);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Hit boundary at: {collisionPoint}");
            }
        }
        #endregion

        #region アイテム収集
        /// <summary>
        /// アイテムを収集する
        /// </summary>
        /// <param name="item">収集するアイテム</param>
        private void CollectItem(GameObject item)
        {
            if (item == null) return;

            // アイテムの種類に応じた処理
            var itemComponent = item.GetComponent<ICollectible>();
            if (itemComponent != null)
            {
                itemComponent.OnCollected(gameObject);
            }

            // イベント通知
            OnItemCollected?.Invoke(item);

            // アイテムを破棄
            Destroy(item);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Collected item: {item.name}");
            }
        }
        #endregion

        #region ノックバックシステム
        /// <summary>
        /// ノックバックを適用
        /// </summary>
        /// <param name="sourcePosition">ノックバックの発生源位置</param>
        private void ApplyKnockback(Vector3 sourcePosition)
        {
            if (isKnockedBack) return;

            Vector3 knockbackDirection = (transform.position - sourcePosition).normalized;
            knockbackVelocity = knockbackDirection * knockbackForce;
            
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;

            // 移動ハンドラーに一時的な力を加える
            characterController?.Movement?.AddForce(knockbackVelocity);

            OnKnockbackStarted?.Invoke(knockbackDirection);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Knockback applied: {knockbackDirection} * {knockbackForce}");
            }
        }

        /// <summary>
        /// ノックバック終了処理
        /// </summary>
        private void EndKnockback()
        {
            isKnockedBack = false;
            knockbackTimer = 0f;
            knockbackVelocity = Vector3.zero;

            if (characterRigidbody != null)
            {
                characterRigidbody.linearVelocity = Vector2.zero;
            }
        }
        #endregion

        #region パブリックメソッド
        /// <summary>
        /// 衝突検出の有効/無効を切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetCollisionEnabled(bool enabled)
        {
            enableCollisionDetection = enabled;
        }

        /// <summary>
        /// ノックバック設定の変更
        /// </summary>
        /// <param name="force">ノックバック力</param>
        /// <param name="duration">ノックバック時間</param>
        public void SetKnockbackParameters(float force, float duration)
        {
            knockbackForce = Mathf.Max(0f, force);
            knockbackDuration = Mathf.Max(0f, duration);
        }

        /// <summary>
        /// アイテム収集範囲の設定
        /// </summary>
        /// <param name="collectionRadius">収集範囲</param>
        /// <param name="magnetRadius">マグネット範囲</param>
        public void SetItemCollectionRanges(float collectionRadius, float magnetRadius)
        {
            itemCollectionRadius = Mathf.Max(0f, collectionRadius);
            magnetRange = Mathf.Max(0f, magnetRadius);
        }

        /// <summary>
        /// 強制的にノックバックを停止
        /// </summary>
        public void ForceStopKnockback()
        {
            EndKnockback();
        }
        #endregion

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (!showCollisionGizmos) return;

            // アイテム収集範囲の表示
            Gizmos.color = itemCollectionColor;
            Gizmos.DrawWireSphere(transform.position, itemCollectionRadius);

            // マグネット範囲の表示
            Gizmos.color = Color.Lerp(itemCollectionColor, Color.clear, 0.5f);
            Gizmos.DrawWireSphere(transform.position, magnetRange);

            // ノックバック方向の表示
            if (isKnockedBack)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, knockbackVelocity);
            }
        }
        #endregion
    }

    /// <summary>
    /// 衝突の種類
    /// </summary>
    public enum CollisionType
    {
        Enter,
        Stay,
        Exit
    }

    /// <summary>
    /// 収集可能なアイテムのインターフェース
    /// </summary>
    public interface ICollectible
    {
        void OnCollected(GameObject collector);
    }
}