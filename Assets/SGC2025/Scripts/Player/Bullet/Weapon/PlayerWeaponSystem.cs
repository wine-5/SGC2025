using UnityEngine;
using System.Collections;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// プレイヤーの武器システムを管理するクラス
    /// 敵撃破による強化、自動発射機能を提供
    /// </summary>
    public class PlayerWeaponSystem : MonoBehaviour
    {
        [Header("武器設定")]
        [SerializeField] private WeaponUpgradeDataSO weaponUpgradeData;
        [SerializeField] private BulletDataSO bulletData;
        [SerializeField] private Transform firePoint; // 弾の発射位置
        
        [Header("デバッグ情報")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int enemiesKilled = 0;
        [SerializeField] private float currentFireInterval = 1f;
        [SerializeField] private int currentBulletDirections = 4;
        
        // 内部状態
        private Coroutine autoFireCoroutine;
        private bool isAutoFiring = false;
        private WeaponLevelData currentLevelData;
        
        // イベント
        public static event System.Action<int> OnWeaponLevelUp; // レベルアップ時
        public static event System.Action<int, int> OnEnemyKilled; // 敵撃破時（撃破数, 現在レベル）
        
        // プロパティ
        public int CurrentLevel => currentLevel;
        public int EnemiesKilled => enemiesKilled;
        public bool IsAutoFiring => isAutoFiring;
        public WeaponLevelData CurrentLevelData => currentLevelData;
        
        private void Awake()
        {
            // 発射位置が設定されていない場合は自分の位置を使用
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }
        
        private void Start()
        {
            InitializeWeapon();
            StartAutoFire();
        }
        
        private void OnEnable()
        {
            // 敵撃破イベントの購読
            SGC2025.Enemy.EnemyController.OnEnemyDestroyed += OnEnemyDestroyed;
        }
        
        private void OnDisable()
        {
            // 敵撃破イベントの購読解除
            SGC2025.Enemy.EnemyController.OnEnemyDestroyed -= OnEnemyDestroyed;
            
            // 自動発射停止
            StopAutoFire();
        }
        
        /// <summary>
        /// 武器システムの初期化
        /// </summary>
        private void InitializeWeapon()
        {
            if (weaponUpgradeData == null)
            {
                Debug.LogError("PlayerWeaponSystem: WeaponUpgradeDataSOが設定されていません");
                return;
            }
            
            // 初期レベルの設定
            UpdateWeaponLevel();
        }
        
        /// <summary>
        /// 敵撃破時の処理
        /// </summary>
        private void OnEnemyDestroyed()
        {
            enemiesKilled++;
            
            // レベルアップチェック
            int newLevel = weaponUpgradeData.CalculateLevel(enemiesKilled);
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                UpdateWeaponLevel();
                OnWeaponLevelUp?.Invoke(currentLevel);
            }
            
            // イベント発火
            OnEnemyKilled?.Invoke(enemiesKilled, currentLevel);
        }
        
        /// <summary>
        /// 武器レベルの更新
        /// </summary>
        private void UpdateWeaponLevel()
        {
            if (weaponUpgradeData == null) return;
            
            currentLevelData = weaponUpgradeData.GetLevelData(currentLevel);
            currentFireInterval = Mathf.Max(currentLevelData.fireInterval, weaponUpgradeData.MinFireInterval);
            currentBulletDirections = currentLevelData.bulletDirections;
            
            Debug.Log($"武器レベルアップ! レベル: {currentLevel}, 方向数: {currentBulletDirections}, 発射間隔: {currentFireInterval}秒");
            
            // 自動発射の再開（間隔が変わった場合）
            if (isAutoFiring)
            {
                StopAutoFire();
                StartAutoFire();
            }
        }
        
        /// <summary>
        /// 自動発射開始
        /// </summary>
        public void StartAutoFire()
        {
            if (isAutoFiring) return;
            
            isAutoFiring = true;
            autoFireCoroutine = StartCoroutine(AutoFireRoutine());
        }
        
        /// <summary>
        /// 自動発射停止
        /// </summary>
        public void StopAutoFire()
        {
            if (autoFireCoroutine != null)
            {
                StopCoroutine(autoFireCoroutine);
                autoFireCoroutine = null;
            }
            isAutoFiring = false;
        }
        
        /// <summary>
        /// 自動発射のコルーチン
        /// </summary>
        private IEnumerator AutoFireRoutine()
        {
            while (isAutoFiring)
            {
                Fire();
                yield return new WaitForSeconds(currentFireInterval);
            }
        }
        
        public void Fire()
        {
            Debug.Log("[PlayerWeaponSystem] Fire() 呼び出し");
            
            if (BulletFactory.I == null)
            {
                Debug.LogError("PlayerWeaponSystem: BulletFactoryが見つかりません");
                return;
            }
            
            Debug.Log($"[PlayerWeaponSystem] BulletFactory見つかりました: {BulletFactory.I}");
            
            if (firePoint == null)
            {
                Debug.LogError("PlayerWeaponSystem: 発射位置が設定されていません");
                return;
            }
            
            Debug.Log($"[PlayerWeaponSystem] 発射位置: {firePoint.position}, 方向数: {currentBulletDirections}");
            
            // 円状に弾を発射
            BulletFactory.I.CreateCircularBullets(
                firePoint.position,
                currentBulletDirections,
                bulletData
            );
            
            Debug.Log("[PlayerWeaponSystem] CreateCircularBullets 呼び出し完了");
        }
        
        /// <summary>
        /// 手動で弾を発射（テスト用）
        /// </summary>
        [ContextMenu("Test Fire")]
        public void TestFire()
        {
            Fire();
        }
        
        /// <summary>
        /// 武器レベルをリセット（テスト用）
        /// </summary>
        [ContextMenu("Reset Weapon Level")]
        public void ResetWeaponLevel()
        {
            enemiesKilled = 0;
            currentLevel = 1;
            UpdateWeaponLevel();
        }
        
        /// <summary>
        /// 敵撃破数を増加（テスト用）
        /// </summary>
        [ContextMenu("Add Enemy Kill")]
        public void AddEnemyKill()
        {
            OnEnemyDestroyed();
        }
    }
}