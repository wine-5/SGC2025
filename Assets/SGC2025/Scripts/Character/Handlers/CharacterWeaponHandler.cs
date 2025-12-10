using UnityEngine;
using System.Collections.Generic;
using System;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクターの武器・射撃システムを専門に管理するクラス
    /// 弾丸生成、連射レート、武器アップグレード拡張に対応
    /// </summary>
    public class CharacterWeapon : MonoBehaviour
    {
        #region 定数
        private const float DEFAULT_FIRE_RATE = 0.3f;
        private const float DEFAULT_BULLET_SPEED = 10f;
        private const float DEFAULT_BULLET_LIFETIME = 5f;
        private const int DEFAULT_BULLET_DAMAGE = 1;
        private const int DEFAULT_WEAPON_LEVEL = 1;
        private const int DEFAULT_MAX_WEAPON_LEVEL = 5;
        private const int DEFAULT_ENEMIES_FOR_UPGRADE = 5;
        private const float DEFAULT_SPREAD_ANGLE = 15f;
        private const int DEFAULT_BULLET_COUNT = 1;
        private const int DEFAULT_MAX_FOLLOWERS = 5;
        private const float DEFAULT_MUZZLE_FLASH_DURATION = 0.1f;
        private const float FOLLOWER_RADIUS = 1.5f;
        private const float FOLLOWER_ROTATION_SPEED = 90f;
        private const int ENEMIES_PER_FOLLOWER = 3;
        private const float FIRE_RATE_IMPROVEMENT = 0.8f;
        private const float BULLET_SPEED_IMPROVEMENT = 1.2f;
        private const string DEBUG_LOG_PREFIX = "[CharacterWeapon]";
        #endregion

        #region Inspector設定
        [Header("基本武器設定")]
        [SerializeField] private GameObject bulletPrefab;  
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float fireRate = DEFAULT_FIRE_RATE;
        [SerializeField] private bool weaponEnabled = true;

        [Header("弾丸設定")]
        [SerializeField] private float bulletSpeed = DEFAULT_BULLET_SPEED;
        [SerializeField] private float bulletLifetime = DEFAULT_BULLET_LIFETIME;
        [SerializeField] private int bulletDamage = DEFAULT_BULLET_DAMAGE;

        [Header("アップグレードシステム")]
        [SerializeField] private int currentWeaponLevel = DEFAULT_WEAPON_LEVEL;
        [SerializeField] private int maxWeaponLevel = DEFAULT_MAX_WEAPON_LEVEL;
        [SerializeField] private int enemiesDefeatedForUpgrade = DEFAULT_ENEMIES_FOR_UPGRADE;
        [SerializeField] private int currentEnemiesDefeated = 0;

        [Header("高度な機能")]
        [SerializeField] private bool enableSpread = false;
        [SerializeField] private float spreadAngle = DEFAULT_SPREAD_ANGLE;
        [SerializeField] private bool enableMultiShot = false;
        [SerializeField] private int bulletCount = DEFAULT_BULLET_COUNT;

        [Header("将来の機能")]
        [SerializeField] private bool enableButterflyFollowers = false;
        [SerializeField] private int maxFollowers = DEFAULT_MAX_FOLLOWERS;
        [SerializeField] private List<GameObject> butterflyFollowers = new List<GameObject>();

        [Header("エフェクト")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private AudioClip shotSound;
        [SerializeField] private float muzzleFlashDuration = DEFAULT_MUZZLE_FLASH_DURATION;

        [Header("デバッグ")]
        [SerializeField] private bool showFireDirection = true;
        [SerializeField] private bool enableDebugLogs = false;
        #endregion

        #region プライベート変数
        private CharacterController characterController;
        private float nextFireTime = 0f;
        private AudioSource audioSource;
        #endregion

        #region イベント
        /// <summary>弾丸発射時のイベント</summary>
        public event Action<Vector3, Vector3> OnWeaponFired; // (position, direction)
        
        /// <summary>武器レベルアップ時のイベント</summary>
        public event Action<int> OnWeaponLevelUp; // (newLevel)
        
        /// <summary>弾丸ヒット時のイベント</summary>
        public event Action<GameObject> OnBulletHit; // (hitTarget)
        #endregion

        #region プロパティ
        /// <summary>現在の武器レベル</summary>
        public int CurrentWeaponLevel => currentWeaponLevel;
        
        /// <summary>射撃可能かどうか</summary>
        public bool CanFire => weaponEnabled && Time.time >= nextFireTime;
        
        /// <summary>武器が有効かどうか</summary>
        public bool IsWeaponEnabled => weaponEnabled;
        
        /// <summary>次のレベルアップまでの敵撃破数</summary>
        public int EnemiesUntilUpgrade => Mathf.Max(0, enemiesDefeatedForUpgrade - currentEnemiesDefeated);
        
        /// <summary>現在の連射レート</summary>
        public float CurrentFireRate => fireRate;
        
        /// <summary>蝶々の追随者数</summary>
        public int FollowerCount => butterflyFollowers.Count;
        #endregion

        #region ハンドラーライフサイクル
        /// <summary>
        /// CharacterControllerによる初期化
        /// </summary>
        /// <param name="controller">親のCharacterController</param>
        public void Initialize(CharacterController controller)
        {
            characterController = controller;
            audioSource = GetComponent<AudioSource>();
            
            ValidateComponents();
            InitializeBulletPool();

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Initialized with {firePoints.Length} fire points");
            }
        }

        /// <summary>
        /// コンポーネントの妥当性チェック
        /// </summary>
        private void ValidateComponents()
        {
            // FirePointが設定されていない場合は自身のTransformを使用
            if (firePoints == null || firePoints.Length == 0)
            {
                firePoints = new Transform[] { transform };
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} No fire points set. Using self transform.");
            }

            // 弾丸プレハブのチェック
            if (bulletPrefab == null)
            {
                Debug.LogError($"{DEBUG_LOG_PREFIX} Bullet prefab is not assigned!");
            }

            // AudioSourceのチェック
            if (audioSource == null && shotSound != null)
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} Shot sound is assigned but AudioSource component is missing.");
            }
        }

        public void OnStart() { }

        public void OnEnable() { }

        public void OnDisable() { }
        #endregion

        #region 更新処理
        private void Update()
        {
            if (characterController?.Input == null) return;

            ProcessWeaponInput();
            UpdateFollowers();
        }

        /// <summary>
        /// 武器入力の処理
        /// </summary>
        private void ProcessWeaponInput()
        {
            if (characterController.Input.IsShotPressed && CanFire)
            {
                FireWeapon();
            }
        }

        /// <summary>
        /// 蝶々フォロワーの更新
        /// </summary>
        private void UpdateFollowers()
        {
            if (!enableButterflyFollowers) return;

            // 蝶々フォロワーの円形移動処理
            for (int i = 0; i < butterflyFollowers.Count; i++)
            {
                if (butterflyFollowers[i] == null) continue;

                float angle = (360f / butterflyFollowers.Count) * i + Time.time * FOLLOWER_ROTATION_SPEED;
                float radius = FOLLOWER_RADIUS;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0f
                );
                
                butterflyFollowers[i].transform.position = transform.position + offset;
            }
        }
        #endregion

        #region 武器システム
        /// <summary>
        /// 武器を発射
        /// </summary>
        public void FireWeapon()
        {
            if (!CanFire || bulletPrefab == null) return;

            nextFireTime = Time.time + fireRate;

            // 射撃方向の決定
            Vector3 fireDirection = GetFireDirection();

            // 弾丸の生成
            if (enableMultiShot)
            {
                FireMultipleBullets(fireDirection);
            }
            else
            {
                FireSingleBullet(fireDirection);
            }

            // エフェクトとサウンド
            PlayMuzzleFlash();
            PlayShotSound();

            // イベント通知
            OnWeaponFired?.Invoke(firePoints[0].position, fireDirection);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Weapon fired at direction: {fireDirection}");
            }
        }

        /// <summary>
        /// 単発弾丸の発射
        /// </summary>
        /// <param name="direction">射撃方向</param>
        private void FireSingleBullet(Vector3 direction)
        {
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    CreateBullet(firePoint.position, direction);
                }
            }
        }

        /// <summary>
        /// 複数弾丸の発射（散弾など）
        /// </summary>
        /// <param name="baseDirection">基準射撃方向</param>
        private void FireMultipleBullets(Vector3 baseDirection)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                Vector3 direction = baseDirection;
                
                // 拡散角度の適用
                if (enableSpread && bulletCount > 1)
                {
                    float angleOffset = spreadAngle * (i - (bulletCount - 1) * 0.5f) / (bulletCount - 1);
                    direction = Quaternion.Euler(0, 0, angleOffset) * baseDirection;
                }

                foreach (Transform firePoint in firePoints)
                {
                    if (firePoint != null)
                    {
                        CreateBullet(firePoint.position, direction);
                    }
                }
            }
        }

        /// <summary>
        /// 弾丸オブジェクトの生成
        /// </summary>
        /// <param name="position">生成位置</param>
        /// <param name="direction">射撃方向</param>
        private void CreateBullet(Vector3 position, Vector3 direction)
        {
            if (bulletPrefab == null) return;

            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));

            if (bullet == null) return;

            // 弾丸の物理設定
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * bulletSpeed;
            }

            // 弾丸コンポーネントの設定
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(bulletDamage, bulletLifetime, this);
            }
            else
            {
                // Bulletコンポーネントがない場合は自動破棄
                Destroy(bullet, bulletLifetime);
            }
        }

        /// <summary>
        /// 射撃方向の取得
        /// </summary>
        /// <returns>射撃方向ベクトル</returns>
        private Vector3 GetFireDirection()
        {
            // 基本的には上方向に射撃
            return Vector3.up;
        }
        #endregion

        #region アップグレードシステム
        /// <summary>
        /// 敵撃破時の処理
        /// </summary>
        public void OnEnemyDefeated()
        {
            currentEnemiesDefeated++;

            if (currentEnemiesDefeated >= enemiesDefeatedForUpgrade && currentWeaponLevel < maxWeaponLevel)
            {
                UpgradeWeapon();
                currentEnemiesDefeated = 0;
            }

            // 蝶々フォロワーの追加判定
            if (enableButterflyFollowers && butterflyFollowers.Count < maxFollowers)
            {
                if (currentEnemiesDefeated % ENEMIES_PER_FOLLOWER == 0)
                {
                    AddButterflyFollower();
                }
            }
        }

        /// <summary>
        /// 武器のレベルアップ
        /// </summary>
        private void UpgradeWeapon()
        {
            currentWeaponLevel++;

            // レベルに応じた能力向上
            switch (currentWeaponLevel)
            {
                case 2:
                    fireRate = Mathf.Max(0.1f, fireRate * FIRE_RATE_IMPROVEMENT);
                    break;
                case 3:
                    enableMultiShot = true;
                    bulletCount = 2;
                    break;
                case 4:
                    bulletCount = 3;
                    enableSpread = true;
                    break;
                case 5:
                    bulletCount = 5;
                    bulletSpeed *= BULLET_SPEED_IMPROVEMENT;
                    break;
            }

            OnWeaponLevelUp?.Invoke(currentWeaponLevel);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Weapon upgraded to level {currentWeaponLevel}");
            }
        }

        /// <summary>
        /// 蝶々フォロワーの追加
        /// </summary>
        private void AddButterflyFollower()
        {
            if (butterflyFollowers.Count >= maxFollowers) return;

            // TODO: 蝶々フォロワーのプレハブから生成
            // GameObject follower = Instantiate(butterflyFollowerPrefab);
            // butterflyFollowers.Add(follower);
            
            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Butterfly follower added. Total: {butterflyFollowers.Count}");
            }
        }
        #endregion

        #region エフェクトとオーディオ
        /// <summary>
        /// マズルフラッシュの再生
        /// </summary>
        private void PlayMuzzleFlash()
        {
            if (muzzleFlashPrefab == null) return;

            foreach (Transform firePoint in firePoints)
            {
                if (firePoint == null) continue;

                GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, muzzleFlashDuration);
            }
        }

        /// <summary>
        /// 射撃音の再生
        /// </summary>
        private void PlayShotSound()
        {
            if (audioSource != null && shotSound != null)
            {
                audioSource.PlayOneShot(shotSound);
            }
        }
        #endregion

        #region オブジェクトプール
        /// <summary>
        /// 弾丸オブジェクトプールの初期化（現在は未実装）
        /// </summary>
        private void InitializeBulletPool()
        {
            // TODO: ObjectPoolクラスが実装されたら対応
        }
        #endregion

        #region パブリックメソッド
        /// <summary>
        /// 武器の有効/無効を切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetWeaponEnabled(bool enabled)
        {
            weaponEnabled = enabled;

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Weapon {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// 武器をリセット（初期状態に戻す）
        /// </summary>
        public void ResetWeapon()
        {
            currentWeaponLevel = DEFAULT_WEAPON_LEVEL;
            currentEnemiesDefeated = 0;
            enableMultiShot = false;
            enableSpread = false;
            bulletCount = DEFAULT_BULLET_COUNT;
            fireRate = DEFAULT_FIRE_RATE;
            bulletSpeed = DEFAULT_BULLET_SPEED;
            
            // フォロワーをクリア
            foreach (var follower in butterflyFollowers)
            {
                if (follower != null)
                {
                    Destroy(follower);
                }
            }
            butterflyFollowers.Clear();

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Weapon reset to default state");
            }
        }

        /// <summary>
        /// 弾丸パラメータの設定
        /// </summary>
        /// <param name="speed">弾丸速度</param>
        /// <param name="damage">弾丸ダメージ</param>
        /// <param name="lifetime">弾丸寿命</param>
        public void SetBulletParameters(float speed, int damage, float lifetime)
        {
            bulletSpeed = Mathf.Max(0f, speed);
            bulletDamage = Mathf.Max(0, damage);
            bulletLifetime = Mathf.Max(0f, lifetime);
        }

        /// <summary>
        /// 連射レートの設定
        /// </summary>
        /// <param name="rate">新しい連射レート</param>
        public void SetFireRate(float rate)
        {
            fireRate = Mathf.Max(0.01f, rate);
        }
        #endregion

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (!showFireDirection) return;

            if (firePoints == null || firePoints.Length == 0) return;

            Gizmos.color = Color.red;
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    Vector3 direction = GetFireDirection();
                    Gizmos.DrawRay(firePoint.position, direction * 2f);
                }
            }

            // 拡散角度の可視化
            if (enableSpread && firePoints.Length > 0 && firePoints[0] != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 baseDirection = GetFireDirection();
                for (int i = 0; i < bulletCount; i++)
                {
                    float angleOffset = spreadAngle * (i - (bulletCount - 1) * 0.5f) / (bulletCount - 1);
                    Vector3 direction = Quaternion.Euler(0, 0, angleOffset) * baseDirection;
                    Gizmos.DrawRay(firePoints[0].position, direction * 1.5f);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 弾丸の基本コンポーネント
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        private int damage;
        private float lifetime;
        private CharacterWeapon weaponHandler;

        /// <summary>
        /// 弾丸の初期化
        /// </summary>
        /// <param name="bulletDamage">ダメージ量</param>
        /// <param name="bulletLifetime">寿命</param>
        /// <param name="handler">武器ハンドラー</param>
        public void Initialize(int bulletDamage, float bulletLifetime, CharacterWeapon handler)
        {
            damage = bulletDamage;
            lifetime = bulletLifetime;
            weaponHandler = handler;
            
            // 寿命タイマー開始
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;

            if (other.CompareTag("Enemy"))
            {
                // 敵にダメージを与える処理
                var enemy = other.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}