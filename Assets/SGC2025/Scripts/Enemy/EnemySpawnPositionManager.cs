using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の生成位置を計算するクラス
    /// 四方向からのランダム生成位置を提供
    /// </summary>
    [System.Serializable]
    public class EnemySpawnPositionManager
    {
        [Header("生成位置設定")]
        [SerializeField] private Transform topSpawnPoint;
        [SerializeField] private Transform bottomSpawnPoint;
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;
        
        [Header("ランダム範囲")]
        [SerializeField] private float randomRangeX = 5f;  // 上下生成時のX座標ランダム範囲
        [SerializeField] private float randomRangeY = 5f;  // 左右生成時のY座標ランダム範囲
        
        /// <summary>
        /// 上から下への生成位置をランダムに取得
        /// </summary>
        /// <returns>生成位置</returns>
        public Vector3 GetRandomTopSpawnPosition()
        {
            if (topSpawnPoint == null)
            {
                Debug.LogWarning("topSpawnPoint が設定されていません");
                return Vector3.zero;
            }
            
            Vector3 basePosition = topSpawnPoint.position;
            // 上下からの生成：X座標をランダムに
            float randomX = Random.Range(
                basePosition.x - randomRangeX * 0.5f,
                basePosition.x + randomRangeX * 0.5f
            );
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        
        /// <summary>
        /// 画面の四方向からランダムに生成位置を取得
        /// </summary>
        /// <returns>生成位置</returns>
        public Vector3 GetRandomEdgeSpawnPosition()
        {
            // 上下左右のどこから生成するかをランダム選択
            int side = Random.Range(0, 4);
            
            return side switch
            {
                0 => GetTopPosition(),    // 上
                1 => GetBottomPosition(), // 下
                2 => GetLeftPosition(),   // 左
                3 => GetRightPosition(),  // 右
                _ => GetTopPosition()
            };
        }
        
        private Vector3 GetTopPosition()
        {
            if (topSpawnPoint == null) return Vector3.zero;
            
            Vector3 basePosition = topSpawnPoint.position;
            // 上からの生成：X座標をランダムに
            float randomX = Random.Range(
                basePosition.x - randomRangeX * 0.5f,
                basePosition.x + randomRangeX * 0.5f
            );
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        
        private Vector3 GetBottomPosition()
        {
            if (bottomSpawnPoint == null) return Vector3.zero;
            
            Vector3 basePosition = bottomSpawnPoint.position;
            // 下からの生成：X座標をランダムに
            float randomX = Random.Range(
                basePosition.x - randomRangeX * 0.5f,
                basePosition.x + randomRangeX * 0.5f
            );
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        
        private Vector3 GetLeftPosition()
        {
            if (leftSpawnPoint == null) return Vector3.zero;
            
            Vector3 basePosition = leftSpawnPoint.position;
            // 左からの生成：Y座標をランダムに
            float randomY = Random.Range(
                basePosition.y - randomRangeY * 0.5f,
                basePosition.y + randomRangeY * 0.5f
            );
            return new Vector3(basePosition.x, randomY, basePosition.z);
        }
        
        private Vector3 GetRightPosition()
        {
            if (rightSpawnPoint == null) return Vector3.zero;
            
            Vector3 basePosition = rightSpawnPoint.position;
            // 右からの生成：Y座標をランダムに
            float randomY = Random.Range(
                basePosition.y - randomRangeY * 0.5f,
                basePosition.y + randomRangeY * 0.5f
            );
            return new Vector3(basePosition.x, randomY, basePosition.z);
        }
        
        /// <summary>
        /// すべてのスポーンポイントが設定されているかチェック
        /// </summary>
        /// <returns>設定状況</returns>
        public bool AreAllSpawnPointsSet()
        {
            return topSpawnPoint != null && 
                   bottomSpawnPoint != null && 
                   leftSpawnPoint != null && 
                   rightSpawnPoint != null;
        }
        
        /// <summary>
        /// 設定されていないスポーンポイントの警告を表示
        /// </summary>
        public void LogMissingSpawnPoints()
        {
            if (topSpawnPoint == null) Debug.LogWarning("topSpawnPoint が設定されていません");
            if (bottomSpawnPoint == null) Debug.LogWarning("bottomSpawnPoint が設定されていません");
            if (leftSpawnPoint == null) Debug.LogWarning("leftSpawnPoint が設定されていません");
            if (rightSpawnPoint == null) Debug.LogWarning("rightSpawnPoint が設定されていません");
        }
    }
}