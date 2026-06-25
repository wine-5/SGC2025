using UnityEngine;
using Tyotyo.Core;
using Tyotyo.InGame.Enemy;
using Tyotyo.InGame.Bullet;
using Tyotyo.Manager;

namespace Tyotyo.InGame.Bullet
{
    /// <summary>
    /// 弾の動作とライフサイクルを管理するコントローラー
    /// ObjectPoolパターンによる効率的な再利用をサポート
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        #region 定数

        private const int CIRCLE_SPRITE_SIZE = 64;
        private const float CIRCLE_SPRITE_CENTER_FACTOR = 0.5f;
        private const float CIRCLE_SPRITE_RADIUS_OFFSET = 1f;
        private const float CIRCLE_SPRITE_PIVOT = 0.5f;

        #endregion

        #region フィールド

        [Header("設定データ")]
        [SerializeField] private BulletDataSO bulletData;
        
        [Header("衝突設定")]
        [SerializeField] private LayerMask enemyLayer = 1 << 7;  // Layer 7 (Enemy)


        // キャッシュされたコンポーネント
        private Rigidbody2D cachedRigidbody;
        private SpriteRenderer cachedSpriteRenderer;
        private BulletRotationEffect rotationEffect;
        private BulletFactory factory;

        // 円スプライトは全弾で共通。インスタンスごとに生成せず1回だけ作って共有する
        // （プールで再利用される弾やシーン再読込時のTexture2Dリークを防ぐ）
        private static Sprite sharedCircleSprite;
        
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
            
            // 実行時にレイヤー番号を取得して正しく設定
            int actualEnemyLayer = GameLayers.EnemyLayer;

            if (actualEnemyLayer != -1)
                enemyLayer = 1 << actualEnemyLayer;
        }

        private void Update()
        {
            if (!isActive) return;
            
            UpdateLifeTime();
            CheckBoundary();
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 弾を初期化してアクティブ化
        /// </summary>
        /// <param name="data">弾データ</param>
        /// <param name="direction">発射方向</param>
        public void Initialize(BulletDataSO data, Vector3 direction, BulletFactory bulletFactory)
        {
            bulletData = data;
            factory = bulletFactory;
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
        private void Deactivate()
        {
            if (!isActive)
                return;
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
                cachedRigidbody.linearVelocity = Vector2.zero;
                cachedRigidbody.angularVelocity = 0f;
            }
            
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        #endregion

        #region 衝突処理

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;
            
            if (IsInLayerMask(other.gameObject, enemyLayer))
            {
                HandleEnemyCollision(other);
            }
        }

        #endregion

        #region プライベートメソッド - 初期化

        private void CacheComponents()
        {
            cachedRigidbody = GetComponent<Rigidbody2D>();
            cachedSpriteRenderer = GetComponent<SpriteRenderer>();
            rotationEffect = GetComponent<BulletRotationEffect>();
        }

        private void ConfigurePhysics()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.gravityScale = 0f;
                cachedRigidbody.linearDamping = 0f;
            }

            var collider = GetComponent<Collider2D>();
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
                Vector2 velocity = direction.normalized * bulletData.MoveSpeed;
                cachedRigidbody.linearVelocity = velocity;
            }
        }

        private void StopMovement()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector2.zero;
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
                    cachedSpriteRenderer.sprite = GetOrCreateCircleSprite();
                }
                
                cachedSpriteRenderer.color = Color.white;
            }
        }

        private Sprite GetOrCreateCircleSprite()
        {
            if (sharedCircleSprite != null) return sharedCircleSprite;

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
            sharedCircleSprite = Sprite.Create(texture, rect, pivot);
            return sharedCircleSprite;
        }

        #endregion

        #region プライベートメソッド - プール管理

        private void ReturnToPool()
        {
            if (factory != null)
                factory.ReturnBullet(gameObject);
            else
                gameObject.SetActive(false);
        }

        #endregion

        #region プライベートメソッド - 衝突ハンドリング

        private void HandleEnemyCollision(Collider2D other)
        {
            // プレイヤーオブジェクトは除外（Layer 6はPlayer）
            if (other.name.Contains(GameLayers.PlayerTag) || other.gameObject.layer == GameLayers.PlayerLayer)
            {
                return;
            }
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                if (bulletData == null) return;
                
                float damageToApply = bulletData.Damage;
                if (damageToApply <= 0) return;
                
                damageable.TakeDamage(damageToApply);
                Deactivate();
            }
        }

        #endregion

        #region プライベートメソッド - ユーティリティ

        private bool IsInLayerMask(GameObject obj, LayerMask layerMask) =>
            (layerMask.value & (1 << obj.layer)) != 0;

        /// <summary>
        /// 弾が画面境界を超えたかチェック
        /// </summary>
        private void CheckBoundary()
        {
            if (GroundManager.I == null || GroundManager.I.MapData == null) return;

            // マップのワールド座標範囲外に出たら非アクティブ化（マージン付き）
            const float BOUNDARY_MARGIN = 1f;
            if (GroundManager.I.MapData.IsOutOfBounds(transform.position, BOUNDARY_MARGIN))
                Deactivate();
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