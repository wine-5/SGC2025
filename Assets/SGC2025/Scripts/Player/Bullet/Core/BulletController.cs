using UnityEngine;
using SGC2025.Enemy;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾の動作を制御するクラス
    /// ObjectPoolで再利用される
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class BulletController : MonoBehaviour
    {
        [Header("設定データ")]
        [SerializeField] private BulletDataSO bulletData;
        
        [Header("衝突設定")]
        [SerializeField] private LayerMask enemyLayer = 1 << 6; // Enemyレイヤー（通常6番）
        [SerializeField] private LayerMask obstacleLayer = 1 << 7; // 障害物レイヤー（通常7番）
        
        // コンポーネント（自動取得）
        private Rigidbody rb;
        private Collider col;
        private SpriteRenderer spriteRenderer;
        
        // 内部状態
        private float currentLifeTime;
        private bool isActive = false;
        
        // プロパティ
        public BulletDataSO BulletData => bulletData;
        public bool IsActive => isActive;
        
        private void Awake()
        {
            // コンポーネントの自動取得
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            Debug.Log($"[BulletController] レイヤー設定確認 - enemyLayer: {enemyLayer.value}, obstacleLayer: {obstacleLayer.value}");
            
            // Rigidbodyの設定
            if (rb != null)
            {
                rb.useGravity = false; // 重力を無効化
                rb.linearDamping = 0f; // 空気抵抗なし
            }
            
            // Colliderの設定
            if (col != null)
            {
                col.isTrigger = true; // トリガーとして設定
            }
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            // 生存時間のカウントダウン
            currentLifeTime -= Time.deltaTime;
            if (currentLifeTime <= 0f)
            {
                DeactivateBullet();
            }
        }
        
        /// <summary>
        /// 弾を初期化して発射
        /// </summary>
        public void Initialize(BulletDataSO data, Vector3 direction)
        {
            Debug.Log($"[BulletController] Initialize呼び出し - データ: {data?.BulletName}, 方向: {direction}");
            
            bulletData = data;
            isActive = true;
            currentLifeTime = bulletData.LifeTime;
            
            Debug.Log($"[BulletController] 設定 - 生存時間: {currentLifeTime}, 速度: {bulletData.MoveSpeed}");
            
            // 見た目の設定
            ApplyVisualSettings();
            
            // 移動の設定
            if (rb != null)
            {
                Vector3 velocity = direction.normalized * bulletData.MoveSpeed;
                rb.linearVelocity = velocity;
                Debug.Log($"[BulletController] Rigidbody速度設定: {velocity}");
            }
            else
            {
                Debug.LogError("[BulletController] Rigidbodyがnullです!");
            }
            
            // オブジェクトを有効化
            gameObject.SetActive(true);
            Debug.Log($"[BulletController] 弾をアクティブ化: {gameObject.name}");
        }
        
        /// <summary>
        /// 弾を無効化（ObjectPoolに戻す）
        /// </summary>
        public void DeactivateBullet()
        {
            Debug.Log($"[BulletController] DeactivateBullet開始: {gameObject.name}");
            
            isActive = false;
            
            // 移動を停止
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            
            // BulletFactoryを通してObjectPoolに返却
            if (BulletFactory.I != null)
            {
                Debug.Log($"[BulletController] BulletFactoryを通して返却: {gameObject.name}");
                BulletFactory.I.ReturnBullet(gameObject);
            }
            else
            {
                Debug.LogWarning($"[BulletController] BulletFactory.Iがnull、直接無効化: {gameObject.name}");
                // フォールバック：直接無効化
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 見た目の設定を適用
        /// </summary>
        private void ApplyVisualSettings()
        {
            if (bulletData == null) return;
            
            // サイズの設定
            transform.localScale = Vector3.one * bulletData.BulletSize;
            
            if (spriteRenderer != null)
            {
                // スプライトの設定
                if (bulletData.BulletSprite != null)
                {
                    spriteRenderer.sprite = bulletData.BulletSprite;
                }
                else
                {
                    // スプライトが設定されていない場合は円形スプライトを作成
                    spriteRenderer.sprite = CreateCircleSprite();
                }
                
                // 弾の色を設定（白色で統一）
                spriteRenderer.color = Color.white;
            }
        }
        
        /// <summary>
        /// 円形のスプライトを作成
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            // 既存の円形スプライトがあるかチェック
            Sprite existingSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            if (existingSprite != null)
            {
                return existingSprite;
            }
            
            // プログラムで円形テクスチャを生成
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        colors[y * size + x] = Color.white;
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// トリガー衝突処理
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            Debug.Log($"[BulletController] 衝突検知: {other.name}, Layer: {other.gameObject.layer}");
            
            int otherLayer = other.gameObject.layer;
            
            // 敵レイヤーとの衝突チェック
            if ((enemyLayer.value & (1 << otherLayer)) != 0)
            {
                Debug.Log($"[BulletController] 敵レイヤーとの衝突確認: {other.name}");
                
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemy.IsAlive)
                {
                    Debug.Log($"[BulletController] 敵にダメージを与える: {bulletData.Damage}");
                    
                    // 敵にダメージを与える
                    enemy.TakeDamage(bulletData.Damage);
                    
                    Debug.Log($"[BulletController] 弾を無効化開始");
                    
                    // 弾を無効化
                    DeactivateBullet();
                    return;
                }
                else
                {
                    Debug.Log($"[BulletController] EnemyControllerが見つからないか死亡済み: enemy={enemy}, IsAlive={enemy?.IsAlive}");
                }
            }
            
            // 障害物レイヤーとの衝突チェック
            if ((obstacleLayer.value & (1 << otherLayer)) != 0)
            {
                Debug.Log($"[BulletController] 障害物レイヤーとの衝突: {other.name}");
                DeactivateBullet();
            }
        }
        
        /// <summary>
        /// ObjectPool用のリセット処理
        /// </summary>
        public void ResetBullet()
        {
            isActive = false;
            currentLifeTime = 0f;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // 回転と初期スケールをリセット
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}