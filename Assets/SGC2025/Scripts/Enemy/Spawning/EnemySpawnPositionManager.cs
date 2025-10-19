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
        private const float RANGE_DETECT_RATIO = 0.6f; // 端判定用の比率
        private const float RANGE_HALF = 0.5f;         // 範囲の半分

        [Header("生成位置設定")]
        [SerializeField] private Transform topSpawnPoint;
        [SerializeField] private Transform bottomSpawnPoint;
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;
        
        private float cachedRangeX;
        private float cachedRangeY;

        public void InitRangesFromTransforms()
        {
            // 上下のランダムX範囲はtop/bottomのlocalScale.xの大きい方
            float topX = topSpawnPoint != null ? topSpawnPoint.localScale.x : 0f;
            float bottomX = bottomSpawnPoint != null ? bottomSpawnPoint.localScale.x : 0f;
            cachedRangeX = Mathf.Max(topX, bottomX);
            // 左右のランダムY範囲はleft/rightのlocalScale.yの大きい方
            float leftY = leftSpawnPoint != null ? leftSpawnPoint.localScale.y : 0f;
            float rightY = rightSpawnPoint != null ? rightSpawnPoint.localScale.y : 0f;
            cachedRangeY = Mathf.Max(leftY, rightY);
        }

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
            float randomX = Random.Range(
                basePosition.x - cachedRangeX * RANGE_HALF,
                basePosition.x + cachedRangeX * RANGE_HALF
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
            float randomX = Random.Range(
                basePosition.x - cachedRangeX * RANGE_HALF,
                basePosition.x + cachedRangeX * RANGE_HALF
            );
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        private Vector3 GetBottomPosition()
        {
            if (bottomSpawnPoint == null) return Vector3.zero;
            Vector3 basePosition = bottomSpawnPoint.position;
            float randomX = Random.Range(
                basePosition.x - cachedRangeX * RANGE_HALF,
                basePosition.x + cachedRangeX * RANGE_HALF
            );
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        private Vector3 GetLeftPosition()
        {
            if (leftSpawnPoint == null) return Vector3.zero;
            Vector3 basePosition = leftSpawnPoint.position;
            float randomY = Random.Range(
                basePosition.y - cachedRangeY * RANGE_HALF,
                basePosition.y + cachedRangeY * RANGE_HALF
            );
            return new Vector3(basePosition.x, randomY, basePosition.z);
        }
        private Vector3 GetRightPosition()
        {
            if (rightSpawnPoint == null) return Vector3.zero;
            Vector3 basePosition = rightSpawnPoint.position;
            float randomY = Random.Range(
                basePosition.y - cachedRangeY * RANGE_HALF,
                basePosition.y + cachedRangeY * RANGE_HALF
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

        /// <summary>
        /// 逆側のエッジ位置を取得
        /// スポーン位置が端に近い場合、反対側の端の位置を返す
        /// </summary>
        /// <param name="spawnPos">スポーン位置</param>
        /// <returns>逆側のエッジ位置</returns>
        public Vector3 GetOppositeEdgePosition(Vector3 spawnPos)
        {
            // 上端判定
            if (topSpawnPoint != null && Mathf.Abs(spawnPos.y - topSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 上端→下端
                Vector3 target = new Vector3(spawnPos.x, bottomSpawnPoint.position.y, spawnPos.z);

                return target;
            }
            // 下端判定
            if (bottomSpawnPoint != null && Mathf.Abs(spawnPos.y - bottomSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 下端→上端
                Vector3 target = new Vector3(spawnPos.x, topSpawnPoint.position.y, spawnPos.z);

                return target;
            }
            // 左端判定
            if (leftSpawnPoint != null && Mathf.Abs(spawnPos.x - leftSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 左端→右端
                Vector3 target = new Vector3(rightSpawnPoint.position.x, spawnPos.y, spawnPos.z);

                return target;
            }
            // 右端判定
            if (rightSpawnPoint != null && Mathf.Abs(spawnPos.x - rightSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 右端→左端
                Vector3 target = new Vector3(leftSpawnPoint.position.x, spawnPos.y, spawnPos.z);

                return target;
            }
            
            // どれにも該当しない場合はフォールバック処理
            // 最も近いエッジを探して、その反対側を返す
            Vector3 fallbackTarget = GetFallbackOppositePosition(spawnPos);
            return fallbackTarget;
        }
        
        /// <summary>
        /// フォールバック用の反対側位置計算
        /// </summary>
        private Vector3 GetFallbackOppositePosition(Vector3 spawnPos)
        {
            // 各端との距離を計算
            float distToTop = topSpawnPoint != null ? Mathf.Abs(spawnPos.y - topSpawnPoint.position.y) : float.MaxValue;
            float distToBottom = bottomSpawnPoint != null ? Mathf.Abs(spawnPos.y - bottomSpawnPoint.position.y) : float.MaxValue;
            float distToLeft = leftSpawnPoint != null ? Mathf.Abs(spawnPos.x - leftSpawnPoint.position.x) : float.MaxValue;
            float distToRight = rightSpawnPoint != null ? Mathf.Abs(spawnPos.x - rightSpawnPoint.position.x) : float.MaxValue;
            
            // 最も近い端を特定
            float minDistance = Mathf.Min(distToTop, distToBottom, distToLeft, distToRight);
            
            if (minDistance == distToTop && topSpawnPoint != null && bottomSpawnPoint != null)
            {
                return new Vector3(spawnPos.x, bottomSpawnPoint.position.y, spawnPos.z);
            }
            else if (minDistance == distToBottom && bottomSpawnPoint != null && topSpawnPoint != null)
            {
                return new Vector3(spawnPos.x, topSpawnPoint.position.y, spawnPos.z);
            }
            else if (minDistance == distToLeft && leftSpawnPoint != null && rightSpawnPoint != null)
            {
                return new Vector3(rightSpawnPoint.position.x, spawnPos.y, spawnPos.z);
            }
            else if (minDistance == distToRight && rightSpawnPoint != null && leftSpawnPoint != null)
            {
                return new Vector3(leftSpawnPoint.position.x, spawnPos.y, spawnPos.z);
            }
            
            // 最終フォールバック: 画面外の遠い位置
            return new Vector3(spawnPos.x * -2f, spawnPos.y * -2f, spawnPos.z);
        }
    }
}