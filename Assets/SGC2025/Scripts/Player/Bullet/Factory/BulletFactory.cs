using UnityEngine;
using TechC;
using SGC2025;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾の生成と管理を行うファクトリークラス
    /// ObjectPoolを使用して弾を効率的に管理
    /// </summary>
    public class BulletFactory : Singleton<BulletFactory>
    {
        [Header("ObjectPool設定")]
        [SerializeField] private ObjectPool objectPool;
        
        [Header("弾の設定")]
        [SerializeField] private BulletDataSO defaultBulletData;
        [SerializeField] private string bulletPoolName = "PlayerBullet";
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Init()
        {
            if (objectPool == null)
            {
                Debug.LogError("BulletFactory: ObjectPoolが設定されていません");
                return;
            }
            
            if (defaultBulletData == null)
            {
                Debug.LogError("BulletFactory: デフォルトのBulletDataSOが設定されていません");
                return;
            }
        }
        
        public BulletController CreateBullet(Vector3 position, Vector3 direction, BulletDataSO bulletData = null)
        {
            if (objectPool == null)
            {
                Debug.LogError("BulletFactory: ObjectPoolが設定されていません");
                return null;
            }
            
            // 弾データの決定
            BulletDataSO dataToUse = bulletData ?? defaultBulletData;
            if (dataToUse == null)
            {
                Debug.LogError("BulletFactory: 使用する弾データがありません");
                return null;
            }
            
            // ObjectPoolから弾を取得
            GameObject bulletObj = objectPool.GetObjectByName(bulletPoolName);
            if (bulletObj == null)
            {
                Debug.LogError($"BulletFactory: ObjectPoolから弾を取得できませんでした。PoolName: {bulletPoolName}");
                return null;
            }
            
            // 位置と回転を設定
            bulletObj.transform.position = position;
            bulletObj.transform.rotation = Quaternion.LookRotation(direction);
            
            // BulletControllerを取得
            BulletController bulletController = bulletObj.GetComponent<BulletController>();
            if (bulletController == null)
            {
                Debug.LogError("BulletFactory: BulletControllerが見つかりません");
                objectPool.ReturnObject(bulletObj);
                return null;
            }
            
            // 弾を初期化
            bulletController.Initialize(dataToUse, direction);
            
            return bulletController;
        }
        
        /// <summary>
        /// 弾をObjectPoolに返却
        /// </summary>
        public void ReturnBullet(GameObject bullet)
        {
            Debug.Log($"[BulletFactory] ReturnBullet開始: {bullet?.name}");
            
            if (objectPool == null || bullet == null) 
            {
                Debug.LogWarning($"[BulletFactory] 返却失敗: objectPool={objectPool}, bullet={bullet}");
                return;
            }
            
            // BulletControllerがあればリセット
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                Debug.Log($"[BulletFactory] BulletControllerリセット");
                bulletController.ResetBullet();
            }
            
            // オブジェクトを無効化してからObjectPoolに返却
            bullet.SetActive(false);
            Debug.Log($"[BulletFactory] ObjectPoolに返却: {bullet.name}");
            objectPool.ReturnObject(bullet);
            Debug.Log($"[BulletFactory] 返却完了");
        }
        
        /// <summary>
        /// 円状に弾を発射（指定した方向数で等分）
        /// </summary>
        public void CreateCircularBullets(Vector3 position, int directionCount, BulletDataSO bulletData = null)
        {
            if (directionCount <= 0) 
            {
                Debug.LogWarning($"[BulletFactory] 無効な方向数: {directionCount}");
                return;
            }
            
            float angleStep = 360f / directionCount;
            
            for (int i = 0; i < directionCount; i++)
            {
                float angle = angleStep * i;
                float radians = angle * Mathf.Deg2Rad;
                
                // XY平面での方向ベクトルを計算（2D用）
                Vector3 direction = new Vector3(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians),
                    0f
                );
                
                CreateBullet(position, direction, bulletData);
            }
        }
    }
}