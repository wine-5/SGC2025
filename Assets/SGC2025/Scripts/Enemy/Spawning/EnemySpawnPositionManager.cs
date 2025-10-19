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
        
        [Header("ランダム生成範囲設定")]
        [SerializeField] private float horizontalSpawnRange = 10f; // 上下からスポーンする際のX軸範囲
        [SerializeField] private float verticalSpawnRange = 8f;     // 左右からスポーンする際のY軸範囲
        
        private float cachedRangeX;
        private float cachedRangeY;

        public void InitRangesFromTransforms()
        {
            // スポーンポイントの検証（エラーのみログ出力）
            ValidateSpawnPoints();
            
            // Inspector設定の範囲値を直接使用
            cachedRangeX = horizontalSpawnRange;
            cachedRangeY = verticalSpawnRange;
            
            #if UNITY_EDITOR
            Debug.Log($"[SpawnPositionManager] 初期化完了 - Range X: {cachedRangeX}, Y: {cachedRangeY}");
            #endif
        }
        
        /// <summary>
        /// スポーンポイントの検証（設定ミスの早期発見用）
        /// </summary>
        private void ValidateSpawnPoints()
        {
            if (topSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Top Spawn Point が設定されていません");
            if (bottomSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Bottom Spawn Point が設定されていません");
            if (leftSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Left Spawn Point が設定されていません");
            if (rightSpawnPoint == null) Debug.LogError("[SpawnPositionManager] Right Spawn Point が設定されていません");
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
            
            Vector3 spawnPos = side switch
            {
                0 => GetTopPosition(),    // 上
                1 => GetBottomPosition(), // 下
                2 => GetLeftPosition(),   // 左
                3 => GetRightPosition(),  // 右
                _ => GetTopPosition()
            };
            
            return spawnPos;
        }
        
        private Vector3 GetTopPosition()
        {
            if (topSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] topSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = topSpawnPoint.position;
            float minX = basePosition.x - cachedRangeX * RANGE_HALF;
            float maxX = basePosition.x + cachedRangeX * RANGE_HALF;
            float randomX = Random.Range(minX, maxX);
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        
        private Vector3 GetBottomPosition()
        {
            if (bottomSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] bottomSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = bottomSpawnPoint.position;
            float minX = basePosition.x - cachedRangeX * RANGE_HALF;
            float maxX = basePosition.x + cachedRangeX * RANGE_HALF;
            float randomX = Random.Range(minX, maxX);
            return new Vector3(randomX, basePosition.y, basePosition.z);
        }
        
        private Vector3 GetLeftPosition()
        {
            if (leftSpawnPoint == null) 
            {  
                Debug.LogError("[SpawnPositionManager] leftSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = leftSpawnPoint.position;
            float minY = basePosition.y - cachedRangeY * RANGE_HALF;
            float maxY = basePosition.y + cachedRangeY * RANGE_HALF;
            float randomY = Random.Range(minY, maxY);
            return new Vector3(basePosition.x, randomY, basePosition.z);
        }
        
        private Vector3 GetRightPosition()
        {
            if (rightSpawnPoint == null) 
            {
                Debug.LogError("[SpawnPositionManager] rightSpawnPoint がnullです");
                return Vector3.zero;
            }
            Vector3 basePosition = rightSpawnPoint.position;
            float minY = basePosition.y - cachedRangeY * RANGE_HALF;
            float maxY = basePosition.y + cachedRangeY * RANGE_HALF;
            float randomY = Random.Range(minY, maxY);
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
            if (topSpawnPoint != null && bottomSpawnPoint != null && Mathf.Abs(spawnPos.y - topSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 上端→下端
                return new Vector3(spawnPos.x, bottomSpawnPoint.position.y, spawnPos.z);
            }
            // 下端判定
            if (bottomSpawnPoint != null && topSpawnPoint != null && Mathf.Abs(spawnPos.y - bottomSpawnPoint.position.y) < cachedRangeY * RANGE_DETECT_RATIO)
            {
                // 下端→上端
                return new Vector3(spawnPos.x, topSpawnPoint.position.y, spawnPos.z);
            }
            // 左端判定
            if (leftSpawnPoint != null && rightSpawnPoint != null && Mathf.Abs(spawnPos.x - leftSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 左端→右端
                return new Vector3(rightSpawnPoint.position.x, spawnPos.y, spawnPos.z);
            }
            // 右端判定
            if (rightSpawnPoint != null && leftSpawnPoint != null && Mathf.Abs(spawnPos.x - rightSpawnPoint.position.x) < cachedRangeX * RANGE_DETECT_RATIO)
            {
                // 右端→左端
                return new Vector3(leftSpawnPoint.position.x, spawnPos.y, spawnPos.z);
            }
            
            // どれにも該当しない場合はフォールバック処理
#if UNITY_EDITOR
            Debug.LogWarning($"[SpawnPositionManager] 端判定に該当しなかったため、フォールバック処理を実行");
#endif
            return GetFallbackOppositePosition(spawnPos);
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