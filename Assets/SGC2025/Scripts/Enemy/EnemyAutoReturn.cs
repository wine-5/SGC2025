using UnityEngine;
using System.Collections;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の自動削除を管理するコンポーネント
    /// 一定時間後、または画面外に出たときにプールに返却する
    /// </summary>
    public class EnemyAutoReturn : MonoBehaviour
    {
        [Header("自動削除設定")]
        [SerializeField] private float lifeTime = 30f; // 生存時間（秒）
        [SerializeField] private float returnBoundary = -15f; // Y座標がこの値以下になったら削除
        
        private float spawnTime;
        private bool isInitialized = false;
        
        /// <summary>
        /// 初期化（生成時に呼ばれる）
        /// </summary>
        public void Initialize()
        {
            spawnTime = Time.time;
            isInitialized = true;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // 時間経過チェック
            if (Time.time - spawnTime >= lifeTime)
            {
                ReturnToPool("時間経過");
                return;
            }
            
            // 画面外チェック（画面下に落ちた場合）
            if (transform.position.y <= returnBoundary)
            {
                ReturnToPool("画面外");
                return;
            }
        }
        
        /// <summary>
        /// プールに返却
        /// </summary>
        private void ReturnToPool(string reason)
        {
            if (EnemyFactory.I != null)
            {
                EnemyFactory.I.ReturnEnemy(gameObject);
            }
            else
            {
                // FactoryがないときFallback
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// プールから取得されたときの初期化
        /// </summary>
        private void OnEnable()
        {
            // プールから再取得された場合は自動で初期化
            if (isInitialized)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// 設定値を変更
        /// </summary>
        public void SetLifeTime(float newLifeTime)
        {
            lifeTime = newLifeTime;
        }
        
        public void SetReturnBoundary(float newBoundary)
        {
            returnBoundary = newBoundary;
        }
    }
}