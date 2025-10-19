using UnityEngine;
using SGC2025.Enemy;
using SGC2025.Player.Bullet.Effects;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾の動作とライフサイクルを管理するコントローラー
    /// ObjectPoolパターンによる効率的な再利用をサポート
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class BulletController : MonoBehaviour
    {
        #region 定数

        private const int DEFAULT_ENEMY_LAYER = 7;  // Enemyレイヤー
        private const int DEFAULT_OBSTACLE_LAYER = 8;  // 使用しない（障害物レイヤーが無いため）
        private const int CIRCLE_SPRITE_SIZE = 64;
        private const float CIRCLE_SPRITE_CENTER_FACTOR = 0.5f;
        private const float CIRCLE_SPRITE_RADIUS_OFFSET = 1f;
        private const float CIRCLE_SPRITE_PIVOT = 0.5f;

        #endregion

        #region フィールド

        [Header("設定データ")]
        [SerializeField] private BulletDataSO bulletData;
        
        [Header("衝突設定")]
        [SerializeField] private LayerMask enemyLayer = 1 << DEFAULT_ENEMY_LAYER;
        [SerializeField] private LayerMask obstacleLayer = 1 << DEFAULT_OBSTACLE_LAYER;
        
        [Header("画面境界設定")]
        [SerializeField] private GameObject topBoundary;
        [SerializeField] private GameObject bottomBoundary;
        [SerializeField] private GameObject leftBoundary;
        [SerializeField] private GameObject rightBoundary;
        
        // キャッシュされたコンポーネント
        private Rigidbody cachedRigidbody;
        private SpriteRenderer cachedSpriteRenderer;
        private BulletRotationEffect rotationEffect;
        
        // 弾の状態
        private float remainingLifeTime;
        private bool isActive;

        #endregion

        #region プロパティ

        /// <summary>弾がアクティブかどうか</summary>
        public bool IsActive => isActive;

        #endregion

        #region Unityライフサイクル
        private void Awake()
        {
            CacheComponents();
            ConfigurePhysics();
            
            // レイヤー設定をログ出力
            Debug.Log($"[BulletController] レイヤー設定 - Enemy: {enemyLayer.value} (Layer {DEFAULT_ENEMY_LAYER}), Obstacle: {obstacleLayer.value} (Layer {DEFAULT_OBSTACLE_LAYER})");
            Debug.Log($"[BulletController] 'Enemy'レイヤー番号の確認: {LayerMask.NameToLayer("Enemy")}");
            Debug.Log($"[BulletController] 'Player'レイヤー番号の確認: {LayerMask.NameToLayer("Player")}");
        }

        private void Update()
        {
            if (!isActive) return;
            
            UpdateLifeTime();
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 弾を初期化してアクティブ化
        /// </summary>
        /// <param name="data">弾データ</param>
        /// <param name="direction">発射方向</param>
        public void Initialize(BulletDataSO data, Vector3 direction)
        {
            bulletData = data;
            isActive = true;
            remainingLifeTime = bulletData.LifeTime;
            
            SetupVisuals();
            SetupRotation();
            SetVelocity(direction);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 弾を非アクティブ化してプールに返却
        /// </summary>
        public void Deactivate()
        {
            if (!isActive) 
            {
                Debug.LogWarning("[BulletController] 既に非アクティブな弾に対してDeactivateが呼ばれました");
                return;
            }
            
            Debug.Log("[BulletController] 弾を非アクティブ化し、プールに返却します");
            isActive = false;
            StopMovement();
            ReturnToPool();
        }

        /// <summary>
        /// ObjectPool用のリセット処理
        /// </summary>
        public void ResetBullet()
        {
            isActive = false;
            remainingLifeTime = 0f;
            
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
            
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 画面境界オブジェクトを設定
        /// </summary>
        public void SetBoundaries(GameObject top, GameObject bottom, GameObject left, GameObject right)
        {
            topBoundary = top;
            bottomBoundary = bottom;
            leftBoundary = left;
            rightBoundary = right;
        }

        #endregion

        #region 衝突処理

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            Debug.Log($"[BulletController] 衝突検出 - Object: {other.name}, Layer: {other.gameObject.layer}, LayerName: {LayerMask.LayerToName(other.gameObject.layer)}");
            
            if (IsInLayerMask(other.gameObject, enemyLayer))
            {
                Debug.Log($"[BulletController] 敵レイヤーとの衝突を確認");
                HandleEnemyCollision(other);
            }
            else if (IsInLayerMask(other.gameObject, obstacleLayer))
            {
                Debug.Log($"[BulletController] 障害物レイヤーとの衝突を確認");
                HandleObstacleCollision();
            }
            else if (IsBoundaryObject(other.gameObject))
            {
                Debug.Log($"[BulletController] 境界オブジェクトとの衝突を確認");
                HandleBoundaryCollision();
            }
            else
            {
                Debug.Log($"[BulletController] 未処理のレイヤーとの衝突 - Layer: {other.gameObject.layer}");
            }
        }

        #endregion

        #region プライベートメソッド - 初期化

        private void CacheComponents()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedSpriteRenderer = GetComponent<SpriteRenderer>();
            rotationEffect = GetComponent<BulletRotationEffect>();
        }

        private void ConfigurePhysics()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.useGravity = false;
                cachedRigidbody.linearDamping = 0f;
            }

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        #endregion

        #region プライベートメソッド - 更新処理

        private void UpdateLifeTime()
        {
            remainingLifeTime -= Time.deltaTime;
            if (remainingLifeTime <= 0f)
            {
                Deactivate();
            }
        }

        #endregion

        #region プライベートメソッド - 動作制御

        private void SetVelocity(Vector3 direction)
        {
            if (cachedRigidbody != null && bulletData != null)
            {
                Vector3 velocity = direction.normalized * bulletData.MoveSpeed;
                cachedRigidbody.linearVelocity = velocity;
            }
        }

        private void StopMovement()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
            }
        }

        #endregion

        #region プライベートメソッド - 外観設定

        private void SetupVisuals()
        {
            if (bulletData == null) return;
            
            transform.localScale = Vector3.one * bulletData.BulletSize;
            
            if (cachedSpriteRenderer != null)
            {
                // SpriteRendererに既存のSpriteがない場合のみ、円形スプライトを作成
                if (cachedSpriteRenderer.sprite == null)
                {
                    cachedSpriteRenderer.sprite = CreateCircleSprite();
                }
                
                cachedSpriteRenderer.color = Color.white;
            }
        }

        private Sprite CreateCircleSprite()
        {
            var texture = new Texture2D(CIRCLE_SPRITE_SIZE, CIRCLE_SPRITE_SIZE);
            var colors = new Color[CIRCLE_SPRITE_SIZE * CIRCLE_SPRITE_SIZE];
            
            var center = new Vector2(CIRCLE_SPRITE_SIZE * CIRCLE_SPRITE_CENTER_FACTOR, CIRCLE_SPRITE_SIZE * CIRCLE_SPRITE_CENTER_FACTOR);
            var radius = CIRCLE_SPRITE_SIZE * CIRCLE_SPRITE_CENTER_FACTOR - CIRCLE_SPRITE_RADIUS_OFFSET;
            
            for (int y = 0; y < CIRCLE_SPRITE_SIZE; y++)
            {
                for (int x = 0; x < CIRCLE_SPRITE_SIZE; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        colors[y * CIRCLE_SPRITE_SIZE + x] = Color.white;
                    }
                    else
                    {
                        colors[y * CIRCLE_SPRITE_SIZE + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            var rect = new Rect(0, 0, CIRCLE_SPRITE_SIZE, CIRCLE_SPRITE_SIZE);
            var pivot = new Vector2(CIRCLE_SPRITE_PIVOT, CIRCLE_SPRITE_PIVOT);
            return Sprite.Create(texture, rect, pivot);
        }

        #endregion

        #region プライベートメソッド - プール管理

        private void ReturnToPool()
        {
            if (BulletFactory.I != null)
            {
                BulletFactory.I.ReturnBullet(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region プライベートメソッド - 衝突ハンドリング

        private void HandleEnemyCollision(Collider other)
        {
            // プレイヤーオブジェクトは除外（Layer 6はPlayer）
            if (other.name.Contains("Player") || other.gameObject.layer == 6)
            {
                Debug.Log($"[BulletController] プレイヤーとの衝突を無視します - Object: {other.name}, Layer: {other.gameObject.layer}");
                return;
            }
            
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsAlive)
            {
                Debug.Log($"[BulletController] 弾が敵に衝突 - 敵: {enemy.EnemyType}, ダメージ: {bulletData.Damage}");
                enemy.TakeDamage(bulletData.Damage);
                Debug.Log($"[BulletController] 弾を非アクティブ化します");
                Deactivate();
            }
            else
            {
                Debug.LogWarning($"[BulletController] 敵レイヤーのオブジェクトと衝突しましたが、有効なEnemyControllerが見つかりません - " +
                               $"Object: {other.name}, HasEnemyController: {enemy != null}, IsAlive: {enemy?.IsAlive}");
            }
        }

        private void HandleObstacleCollision()
        {
            Deactivate();
        }

        private void HandleBoundaryCollision()
        {
            Deactivate();
        }

        #endregion

        #region プライベートメソッド - ユーティリティ

        private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            bool result = (layerMask.value & (1 << obj.layer)) != 0;
            Debug.Log($"[BulletController] レイヤーチェック - Object: {obj.name}, Layer: {obj.layer}, LayerMask: {layerMask.value}, Result: {result}");
            return result;
        }

        private bool IsBoundaryObject(GameObject obj)
        {
            if (obj == null) return false;
            
            return obj == topBoundary || 
                   obj == bottomBoundary || 
                   obj == leftBoundary || 
                   obj == rightBoundary;
        }

        #endregion
        
        #region プライベートメソッド - 回転設定

        private void SetupRotation()
        {
            if (rotationEffect != null && bulletData != null)
            {
                if (bulletData.EnableRotation)
                {
                    rotationEffect.SetRotationSpeed(bulletData.RotationSpeed);
                    rotationEffect.SetRotationDirection(bulletData.RotationDirection);
                    rotationEffect.StartRotation();
                }
                else
                {
                    rotationEffect.StopRotation();
                }
            }
        }

        #endregion
    }
}