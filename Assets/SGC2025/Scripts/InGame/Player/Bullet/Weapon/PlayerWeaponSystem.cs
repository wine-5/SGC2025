using UnityEngine;
using System.Collections;
using SGC2025.Events;
using SGC2025.Audio;
using SGC2025.Manager;

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
        
        // 内部状態
        private WeaponLevelData currentLevelData;
        
        // イベント
        public static event System.Action<int> OnWeaponLevelUp; // レベルアップ時
        public static event System.Action<int, int> OnEnemyKilled; // 敵撃破時（撃破数, 現在レベル）
        
        // プロパティ
        public int CurrentLevel => currentLevel;
        public int EnemiesKilled => enemiesKilled;
        public WeaponLevelData CurrentLevelData => currentLevelData;
        
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
            EnemyEvents.OnEnemyDestroyed += OnEnemyDestroyed;
        }
        
        private void OnDisable()
        {
            EnemyEvents.OnEnemyDestroyed -= OnEnemyDestroyed;
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