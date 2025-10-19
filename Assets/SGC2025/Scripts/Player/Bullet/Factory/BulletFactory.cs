using UnityEngine;
using TechC;
using SGC2025;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾の生成・管理・プールリングを担当するファクトリークラス
    /// Singletonパターンでシーン内の一元管理を実現
    /// ObjectPoolを利用した効率的なメモリ管理
    /// </summary>
    public class BulletFactory : Singleton<BulletFactory>
    {
        #region 定数

        private const float FULL_CIRCLE_DEGREES = 360f;
        private const float BULLET_Z_POSITION = 0f; // 2D用Z座標統一

        #endregion

        #region フィールド

        [Header("ObjectPool設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("弾設定")]
        [SerializeField] private BulletDataSO defaultBulletData;
        [SerializeField] private string bulletPoolName = "PlayerBullet";
        
        [Header("画面境界設定")]
        [SerializeField] private GameObject topBoundary;
        [SerializeField] private GameObject bottomBoundary;
        [SerializeField] private GameObject leftBoundary;
        [SerializeField] private GameObject rightBoundary;

        #endregion

        #region プロパティ

        protected override bool UseDontDestroyOnLoad => false;

        public BulletDataSO DefaultBulletData => defaultBulletData;

        #endregion

        #region Singletonライフサイクル

        protected override void Init()
        {
            ValidateConfiguration();
        }

        #endregion

        #region パブリックメソッド - 弾生成

        /// <summary>
        /// 単発弾を生成
        /// </summary>
        /// <param name="position">生成位置</param>
        /// <param name="direction">発射方向</param>
        /// <param name="bulletData">弾データ（nullの場合はデフォルト使用）</param>
        /// <returns>生成された弾のコントローラー</returns>
        public BulletController CreateBullet(Vector3 position, Vector3 direction, BulletDataSO bulletData = null)
        {
            if (!ValidatePoolAndData()) return null;
            
            BulletDataSO dataToUse = bulletData ?? defaultBulletData;
            
            GameObject bulletObj = AcquireBulletFromPool();
            if (bulletObj == null) return null;
            
            ConfigureBulletPosition(bulletObj, position);
            
            BulletController controller = SetupBulletController(bulletObj, dataToUse, direction);
            return controller;
        }

        /// <summary>
        /// 円状に等間隔で弾を発射
        /// </summary>
        /// <param name="position">発射位置</param>
        /// <param name="directionCount">方向数</param>
        /// <param name="bulletData">弾データ（nullの場合はデフォルト使用）</param>
        public void CreateCircularBullets(Vector3 position, int directionCount, BulletDataSO bulletData = null)
        {
            if (directionCount <= 0) 
            {
                Debug.LogWarning($"[BulletFactory] 無効な方向数: {directionCount}");
                return;
            }
            
            float angleStep = FULL_CIRCLE_DEGREES / directionCount;
            
            for (int i = 0; i < directionCount; i++)
            {
                Vector3 direction = CalculateCircularDirection(angleStep * i);
                CreateBullet(position, direction, bulletData);
            }
        }

        #endregion

        #region パブリックメソッド - プール管理

        /// <summary>
        /// 弾をObjectPoolに返却
        /// </summary>
        /// <param name="bullet">返却する弾のGameObject</param>
        public void ReturnBullet(GameObject bullet)
        {
            if (!ValidateReturn(bullet)) return;
            
            ResetBulletController(bullet);
            bullet.SetActive(false);
            objectPool.ReturnObject(bullet);
        }

        #endregion

        #region プライベートメソッド - 検証

        private void ValidateConfiguration()
        {
            if (objectPool == null)
            {
                Debug.LogError("[BulletFactory] ObjectPoolが設定されていません");
            }
            
            if (defaultBulletData == null)
            {
                Debug.LogError("[BulletFactory] デフォルトのBulletDataSOが設定されていません");
            }
        }

        private bool ValidatePoolAndData()
        {
            if (objectPool == null)
            {
                Debug.LogError("[BulletFactory] ObjectPoolが設定されていません");
                return false;
            }
            
            if (defaultBulletData == null)
            {
                Debug.LogError("[BulletFactory] デフォルトの弾データがありません");
                return false;
            }
            
            return true;
        }

        private bool ValidateReturn(GameObject bullet)
        {
            if (objectPool == null || bullet == null)
            {
                Debug.LogWarning($"[BulletFactory] 返却失敗: objectPool={objectPool}, bullet={bullet}");
                return false;
            }
            return true;
        }

        #endregion

        #region プライベートメソッド - プール操作

        private GameObject AcquireBulletFromPool()
        {
            GameObject bulletObj = objectPool.GetObjectByName(bulletPoolName);
            if (bulletObj == null)
            {
                Debug.LogError($"[BulletFactory] ObjectPoolから弾を取得できませんでした。PoolName: {bulletPoolName}");
            }
            return bulletObj;
        }

        private void ResetBulletController(GameObject bullet)
        {
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.ResetBullet();
            }
        }

        #endregion

        #region プライベートメソッド - 弾設定

        private void ConfigureBulletPosition(GameObject bulletObj, Vector3 position)
        {
            // Z座標を統一して当たり判定を確実にする
            Vector3 adjustedPosition = new Vector3(position.x, position.y, BULLET_Z_POSITION);
            bulletObj.transform.position = adjustedPosition;
            bulletObj.transform.rotation = Quaternion.identity;
        }

        private BulletController SetupBulletController(GameObject bulletObj, BulletDataSO bulletData, Vector3 direction)
        {
            BulletController bulletController = bulletObj.GetComponent<BulletController>();
            if (bulletController == null)
            {
                Debug.LogError("[BulletFactory] BulletControllerが見つかりません");
                objectPool.ReturnObject(bulletObj);
                return null;
            }
            
            bulletController.SetBoundaries(topBoundary, bottomBoundary, leftBoundary, rightBoundary);
            bulletController.Initialize(bulletData, direction);
            
            return bulletController;
        }

        #endregion

        #region プライベートメソッド - 数学計算

        private Vector3 CalculateCircularDirection(float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            
            return new Vector3(
                Mathf.Cos(radians),
                Mathf.Sin(radians),
                0f
            );
        }

        #endregion
    }
}