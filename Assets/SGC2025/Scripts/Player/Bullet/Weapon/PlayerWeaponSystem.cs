using UnityEngine;
using System.Collections;
using SGC2025.Events;

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
        
        [Header("デバッグ設定")]
        [SerializeField] private bool debugManualFiring = false; // デバッグ用：手動発射モード
        
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
            
            // デバッグフラグをチェックして自動発射を決定
            if (!debugManualFiring)
            {
                StartAutoFire();
            }
        }
        
        private void OnEnable()
        {
            // 敵撃破イベントの購読（EnemyEventsクラス経由）
            EnemyEvents.OnEnemyDestroyed += OnEnemyDestroyed;
        }
        
        private void OnDisable()
        {
            // 敵撃破イベントの購読解除
            EnemyEvents.OnEnemyDestroyed -= OnEnemyDestroyed;
            
            // 自動発射停止
            StopAutoFire();
        }
        
        /// <summary>
        /// 武器システムの初期化
        /// </summary>
        private void InitializeWeapon()
        {
            if (weaponUpgradeData == null) return;
            
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
            
            // 自動発射の再開（間隔が変わった場合）- デバッグフラグもチェック
            if (isAutoFiring && !debugManualFiring)
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
            if (BulletFactory.I == null || firePoint == null) return;
            
            // 円状に弾を発射
            BulletFactory.I.CreateCircularBullets(
                firePoint.position,
                currentBulletDirections,
                bulletData
            );
        }
    }
}