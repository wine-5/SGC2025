using UnityEngine;
using SGC2025.Enemy;

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

        private const int DEFAULT_ENEMY_LAYER = 6;
        private const int DEFAULT_OBSTACLE_LAYER = 7;
        private const int DEFAULT_PLAYER_LAYER = 3; // Playerレイヤー
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
            SetVelocity(direction);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 弾を非アクティブ化してプールに返却
        /// </summary>
        public void Deactivate()
        {
            Debug.Log($"[BulletController] Deactivate開始 - isActive:{isActive}");
            isActive = false;
            StopMovement();
            Debug.Log($"[BulletController] ReturnToPool実行前");
            ReturnToPool();
            Debug.Log($"[BulletController] Deactivate完了");
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
            
            // デバッグ情報：Z座標確認
            Debug.Log($"[BulletController] 衝突検知 - 弾Z:{transform.position.z}, 相手Z:{other.transform.position.z}, 相手:{other.name}");
            
            if (IsInLayerMask(other.gameObject, enemyLayer))
            {
                HandleEnemyCollision(other);
            }
            else if (IsInLayerMask(other.gameObject, obstacleLayer))
            {
                HandleObstacleCollision(other);
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
            Debug.Log($"[BulletController] ReturnToPool開始 - BulletFactory.I:{BulletFactory.I != null}");
            
            if (BulletFactory.I != null)
            {
                Debug.Log($"[BulletController] BulletFactory経由でプールに返却");
                BulletFactory.I.ReturnBullet(gameObject);
            }
            else
            {
                Debug.Log($"[BulletController] 直接非アクティブ化");
                gameObject.SetActive(false);
            }
            
            Debug.Log($"[BulletController] ReturnToPool完了");
        }

        #endregion

        #region プライベートメソッド - 衝突ハンドリング

        private void HandleEnemyCollision(Collider other)
        {
            Debug.Log($"[BulletController] 敵との衝突開始 - オブジェクト名:{other.name}");
            
            // Playerレイヤーは除外
            if (other.gameObject.layer == DEFAULT_PLAYER_LAYER)
            {
                Debug.Log($"[BulletController] Playerレイヤーとの衝突は無視します - レイヤー:{other.gameObject.layer}");
                return;
            }
            
            var enemy = other.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogWarning($"[BulletController] EnemyControllerが見つかりません - {other.name}");
                return;
            }
            
            Debug.Log($"[BulletController] Enemy情報 - IsAlive:{enemy.IsAlive}, CurrentHealth:{enemy.CurrentHealth}");
            
            if (enemy.IsAlive)
            {
                Debug.Log($"[BulletController] ダメージ処理開始 - Damage:{bulletData.Damage}");
                enemy.TakeDamage(bulletData.Damage);
                Debug.Log($"[BulletController] ダメージ処理後 - Enemy IsAlive:{enemy.IsAlive}");
                
                Debug.Log($"[BulletController] 弾を非アクティブ化します");
                Deactivate();
                Debug.Log($"[BulletController] 弾の非アクティブ化完了");
            }
            else
            {
                Debug.Log($"[BulletController] 敵は既に死亡しています");
            }
        }

        private void HandleObstacleCollision()
        {
            Debug.Log($"[BulletController] 障害物衝突処理");
            Deactivate();
        }

        private void HandleObstacleCollision(Collider other)
        {
            Debug.Log($"[BulletController] 障害物衝突処理 - オブジェクト名:{other.name}");
            
            // EnemyControllerがある場合は敵として処理
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                Debug.Log($"[BulletController] 障害物レイヤーの敵を発見 - {other.name}");
                HandleEnemyCollision(other);
                return;
            }
            
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
            Debug.Log($"[BulletController] レイヤーチェック - オブジェクト:{obj.name}, レイヤー:{obj.layer}, マスク値:{layerMask.value}, 結果:{result}");
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
    }
}