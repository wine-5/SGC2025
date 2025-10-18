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
            isActive = false;
            
            // 移動を停止
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            
            // オブジェクトを無効化（ObjectPoolが管理）
            gameObject.SetActive(false);
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
            }
        }
        
        /// <summary>
        /// トリガー衝突処理
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            // 敵との衝突チェック
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsAlive)
            {
                // 敵にダメージを与える
                enemy.TakeDamage(bulletData.Damage);
                
                // 弾を無効化
                DeactivateBullet();
                return;
            }
            
            // 壁や障害物との衝突（敵以外）
            if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
            {
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
            
            transform.localScale = Vector3.one;
        }
    }
}