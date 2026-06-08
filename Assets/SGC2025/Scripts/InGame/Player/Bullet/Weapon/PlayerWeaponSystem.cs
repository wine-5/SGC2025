using UnityEngine;
using SGC2025.Core;
using SGC2025.Audio;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// プレイヤーの武器システムを管理するクラス
    /// 敵撃破による強化、ボタン押下時の発射機能を提供
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
        [SerializeField] private int currentBulletDirections = 4;
        
        private WeaponLevelData currentLevelData;

        public static event System.Action<int> OnWeaponLevelUp;
        public static event System.Action<int, int> OnEnemyKilled;

        private void Awake()
        {
            if (firePoint == null)
                firePoint = transform;
        }
        
        private void Start()
        {
            InitializeWeapon();
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
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
        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            enemiesKilled++;
            
            int newLevel = weaponUpgradeData.CalculateLevel(enemiesKilled);
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                UpdateWeaponLevel();
                OnWeaponLevelUp?.Invoke(currentLevel);
            }
            OnEnemyKilled?.Invoke(enemiesKilled, currentLevel);
        }
        
        /// <summary>
        /// 武器レベルの更新
        /// </summary>
        private void UpdateWeaponLevel()
        {
            if (weaponUpgradeData == null) return;
            
            currentLevelData = weaponUpgradeData.GetLevelData(currentLevel);
            currentBulletDirections = currentLevelData.bulletDirections;
        }
        
        /// <summary>
        /// 弾を発射（ボタン押下時に呼ばれる）
        /// </summary>
        public void Fire()
        {
            if (BulletFactory.I == null || firePoint == null) return;
            
            BulletFactory.I.CreateCircularBullets(
                firePoint.position,
                currentBulletDirections,
                bulletData
            );
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.PlayerShoot);
        }
    }
}