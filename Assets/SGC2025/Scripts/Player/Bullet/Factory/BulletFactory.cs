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
            Debug.Log($"[BulletFactory] CreateBullet呼び出し - 位置: {position}, 方向: {direction}");
            
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
            
            Debug.Log($"[BulletFactory] 使用データ: {dataToUse.BulletName}");
            
            // ObjectPoolから弾を取得
            GameObject bulletObj = objectPool.GetObjectByName(bulletPoolName);
            if (bulletObj == null)
            {
                Debug.LogError($"BulletFactory: ObjectPoolから弾を取得できませんでした。PoolName: {bulletPoolName}");
                return null;
            }
            
            Debug.Log($"[BulletFactory] プールから弾取得: {bulletObj.name}");
            
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
            
            Debug.Log($"[BulletFactory] BulletController取得: {bulletController}");
            
            // 弾を初期化
            bulletController.Initialize(dataToUse, direction);
            
            Debug.Log($"[BulletFactory] 弾初期化完了");
            return bulletController;
        }
        
        public void ReturnBullet(GameObject bullet)
        {
            if (objectPool == null || bullet == null) return;
            
            // BulletControllerがあればリセット
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController?.ResetBullet();
            
            // ObjectPoolに返却
            objectPool.ReturnObject(bullet);
        }
        
        /// <summary>
        /// 円状に弾を発射（指定した方向数で等分）
        /// </summary>
        public void CreateCircularBullets(Vector3 position, int directionCount, BulletDataSO bulletData = null)
        {
            Debug.Log($"[BulletFactory] CreateCircularBullets呼び出し - 位置: {position}, 方向数: {directionCount}");
            
            if (directionCount <= 0) 
            {
                Debug.LogWarning($"[BulletFactory] 無効な方向数: {directionCount}");
                return;
            }
            
            float angleStep = 360f / directionCount;
            Debug.Log($"[BulletFactory] 角度ステップ: {angleStep}度");
            
            for (int i = 0; i < directionCount; i++)
            {
                float angle = angleStep * i;
                float radians = angle * Mathf.Deg2Rad;
                
                // XZ平面での方向ベクトルを計算
                Vector3 direction = new Vector3(
                    Mathf.Cos(radians),
                    0f,
                    Mathf.Sin(radians)
                );
                
                Debug.Log($"[BulletFactory] 弾{i}: 角度{angle}度, 方向{direction}");
                CreateBullet(position, direction, bulletData);
            }
            
            Debug.Log($"[BulletFactory] {directionCount}個の弾作成完了");
        }
    }
}