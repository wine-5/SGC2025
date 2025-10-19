using UnityEngine;
using System.Collections;

namespace SGC2025
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("生成設定")]
        [SerializeField] private float spawnInterval = 2f; // 生成間隔（秒）
        [SerializeField] private float spawnHeight = 10f; // 生成Y座標
        [SerializeField] private bool autoStart = true; // 自動開始
        
        [Header("生成範囲")]
        [SerializeField] private float spawnRangeX = 10f; // X軸の生成範囲
        [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero; // 生成エリアの中心
        
        [Header("ウェーブ設定")]
        [SerializeField] private int currentWaveLevel = 1; // 現在のウェーブレベル
        
        private bool isSpawning = false;
        private Coroutine spawnCoroutine;
        
        private void Start()
        {
            if (autoStart)
            {
                StartSpawning();
            }
        }
        
        /// <summary>
        /// 敵の生成を開始
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning) return;
            
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }
        
        /// <summary>
        /// 敵の生成を停止
        /// </summary>
        public void StopSpawning()
        {
            if (!isSpawning) return;
            
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        /// <summary>
        /// 生成間隔を設定
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(0.1f, interval);
        }
        
        /// <summary>
        /// ウェーブレベルを設定
        /// </summary>
        public void SetWaveLevel(int waveLevel)
        {
            currentWaveLevel = Mathf.Max(1, waveLevel);
        }
        
        /// <summary>
        /// 敵生成のコルーチン
        /// </summary>
        private IEnumerator SpawnCoroutine()
        {
            while (isSpawning)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        /// <summary>
        /// 敵を1体生成
        /// </summary>
        private void SpawnEnemy()
        {
            if (EnemyFactory.I == null)
            {
                Debug.LogWarning("EnemyFactory instance not found!");
                return;
            }
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = EnemyFactory.I.CreateRandomEnemy(spawnPosition, currentWaveLevel);
            
            if (enemy != null)
            {
                Debug.Log($"Enemy spawned at {spawnPosition}");
            }
            else
            {
                Debug.LogWarning("Failed to spawn enemy!");
            }
        }
        
        /// <summary>
        /// ランダムな生成位置を取得（上から下）
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            float randomX = Random.Range(
                spawnAreaCenter.x - spawnRangeX * 0.5f,
                spawnAreaCenter.x + spawnRangeX * 0.5f
            );
            
            return new Vector3(randomX, spawnAreaCenter.y + spawnHeight, 0);
        }
        
        /// <summary>
        /// デバッグ用：生成範囲を描画
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            
            // 生成範囲を描画
            Vector3 leftPoint = new Vector3(spawnAreaCenter.x - spawnRangeX * 0.5f, spawnAreaCenter.y + spawnHeight, 0);
            Vector3 rightPoint = new Vector3(spawnAreaCenter.x + spawnRangeX * 0.5f, spawnAreaCenter.y + spawnHeight, 0);
            
            Gizmos.DrawLine(leftPoint, rightPoint);
            Gizmos.DrawWireCube(new Vector3(spawnAreaCenter.x, spawnAreaCenter.y + spawnHeight, 0), new Vector3(spawnRangeX, 0.5f, 0.5f));
        }
    }
}