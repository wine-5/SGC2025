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
        private const int DEFAULT_OBSTACLE_LAYER = -1; // 使用しない（障害物レイヤーが存在しないため）
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
        [SerializeField] private LayerMask obstacleLayer = 0;    // 使用しない
        
        [Header("画面境界設定")]
        [SerializeField] private Transform topBoundary;
        [SerializeField] private Transform bottomBoundary;
        [SerializeField] private Transform leftBoundary;
        [SerializeField] private Transform rightBoundary;
        
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
            
            // 実行時にレイヤー番号を取得して正しく設定
            int actualEnemyLayer = LayerMask.NameToLayer("Enemy");
            
            if (actualEnemyLayer != -1)
            {
                enemyLayer = 1 << actualEnemyLayer;  // Layer 7 (Enemy) = 128
            }
            else
            {
                Debug.LogError("[BulletController] 'Enemy'レイヤーが見つかりません！");
            }
            
            // 障害物レイヤーは使用しないので0に設定
            obstacleLayer = 0;
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
                return;
            }
            
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
        /// 画面境界Transformを設定
        /// </summary>
        public void SetBoundaries(Transform top, Transform bottom, Transform left, Transform right)
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
            
            if (IsInLayerMask(other.gameObject, enemyLayer))
            {
                HandleEnemyCollision(other);
            }
            else if (IsBoundaryObject(other.gameObject))
            {
                HandleBoundaryCollision();
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
                return;
            }
            
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsAlive)
            {
                if (bulletData == null)
                {
                    Debug.LogError("[BulletController] BulletDataSOがnullです！ダメージを与えられません");
                    return;
                }
                
                float damageToApply = bulletData.Damage;
                if (damageToApply <= 0)
                {
                    Debug.LogWarning($"[BulletController] ダメージが0以下です: {damageToApply}");
                    return;
                }
                
                enemy.TakeDamage(damageToApply);
                Deactivate();
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

        private bool IsInLayerMask(GameObject obj, LayerMask layerMask) =>
            (layerMask.value & (1 << obj.layer)) != 0;

        private bool IsBoundaryObject(GameObject obj)
        {
            if (obj == null) return false;
            
            Transform objTransform = obj.transform;
            return objTransform == topBoundary || 
                   objTransform == bottomBoundary || 
                   objTransform == leftBoundary || 
                   objTransform == rightBoundary;
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