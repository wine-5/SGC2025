using UnityEngine;

namespace SGC2025.Events
{
    /// <summary>
    /// 敵関連のイベント定義
    /// システム間の疎結合通信を提供
    /// </summary>
    public static class EnemyEvents
    {
        #region 敵撃破イベント
        
        /// <summary>敵が撃破された際に発火されるイベント</summary>
        public static event System.Action OnEnemyDestroyed;
        
        /// <summary>敵が撃破された際に位置情報と共に発火されるイベント</summary>
        public static event System.Action<Vector3> OnEnemyDestroyedAtPosition;
        
        /// <summary>敵が撃破された際にスコア情報と共に発火されるイベント</summary>
        public static event System.Action<int> OnEnemyDestroyedWithScore;
        
        #endregion
        
        #region 敵ダメージイベント
        
        /// <summary>敵がダメージを受けた際のイベント</summary>
        public static event System.Action<GameObject, float> OnEnemyDamageTaken;
        
        /// <summary>敵の体力が変化した際のイベント</summary>
        public static event System.Action<GameObject, float, float> OnEnemyHealthChanged; // (enemy, currentHP, maxHP)
        
        #endregion
        
        #region 敵生成・管理イベント
        
        /// <summary>敵が生成された際のイベント</summary>
        public static event System.Action<GameObject> OnEnemySpawned;
        
        /// <summary>敵がプールに返却された際のイベント</summary>
        public static event System.Action<GameObject> OnEnemyReturnedToPool;
        
        #endregion
        
        #region イベント発火メソッド
        
        /// <summary>敵撃破イベントを発火</summary>
        public static void TriggerEnemyDestroyed(Vector3 position, int score = 0)
        {
            OnEnemyDestroyed?.Invoke();
            OnEnemyDestroyedAtPosition?.Invoke(position);
            if (score > 0)
            {
                OnEnemyDestroyedWithScore?.Invoke(score);
            }
        }
        
        /// <summary>敵ダメージイベントを発火</summary>
        public static void TriggerEnemyDamage(GameObject enemy, float damage, float currentHP, float maxHP)
        {
            OnEnemyDamageTaken?.Invoke(enemy, damage);
            OnEnemyHealthChanged?.Invoke(enemy, currentHP, maxHP);
        }
        
        /// <summary>敵生成イベントを発火</summary>
        public static void TriggerEnemySpawned(GameObject enemy)
        {
            OnEnemySpawned?.Invoke(enemy);
        }
        
        /// <summary>敵プール返却イベントを発火</summary>
        public static void TriggerEnemyReturnedToPool(GameObject enemy)
        {
            OnEnemyReturnedToPool?.Invoke(enemy);
        }
        
        #endregion
        
        #region デバッグ・清掃メソッド
        
        /// <summary>すべてのイベントリスナーをクリア（デバッグ用）</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ClearAllEvents()
        {
            OnEnemyDestroyed = null;
            OnEnemyDestroyedAtPosition = null;
            OnEnemyDestroyedWithScore = null;
            OnEnemyDamageTaken = null;
            OnEnemyHealthChanged = null;
            OnEnemySpawned = null;
            OnEnemyReturnedToPool = null;
            
            Debug.Log("[EnemyEvents] すべてのイベントリスナーをクリアしました");
        }
        
        #endregion
    }
}