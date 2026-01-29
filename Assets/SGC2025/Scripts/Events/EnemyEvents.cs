using UnityEngine;

namespace SGC2025.Events
{
    /// <summary>
    /// 敵関連のイベント定義
    /// システム間の疎結合通信を提供
    /// </summary>
    public static class EnemyEvents
    {
        private const int MIN_SCORE = 0;

        public static event System.Action OnEnemyDestroyed;
        public static event System.Action<Vector3> OnEnemyDestroyedAtPosition;
        public static event System.Action<int, Vector3> OnEnemyDestroyedWithScore;
        /// <summary>倍率適用後の最終スコアが加算されたときのイベント（UI表示用）</summary>
        public static event System.Action<int, Vector3> OnEnemyScoreAdded;
        public static event System.Action<GameObject, float> OnEnemyDamageTaken;
        public static event System.Action<GameObject, float, float> OnEnemyHealthChanged;
        public static event System.Action<GameObject> OnEnemySpawned;
        public static event System.Action<GameObject> OnEnemyReturnedToPool;
        
        public static void TriggerEnemyDestroyed(Vector3 position, int score = MIN_SCORE)
        {
            OnEnemyDestroyed?.Invoke();
            OnEnemyDestroyedAtPosition?.Invoke(position);
            if (score > MIN_SCORE)
                OnEnemyDestroyedWithScore?.Invoke(score, position);
        }
        
        /// <summary>
        /// 倍率適用後のスコアが加算されたことを通知（UI表示用）
        /// </summary>
        /// <param name="finalScore">倍率適用後の最終スコア</param>
        /// <param name="position">位置</param>
        public static void TriggerEnemyScoreAdded(int finalScore, Vector3 position)
        {
            OnEnemyScoreAdded?.Invoke(finalScore, position);
        }

        public static void TriggerEnemyDamage(GameObject enemy, float damage, float currentHP, float maxHP)
        {
            OnEnemyDamageTaken?.Invoke(enemy, damage);
            OnEnemyHealthChanged?.Invoke(enemy, currentHP, maxHP);
        }
        
        public static void TriggerEnemySpawned(GameObject enemy)
        {
            OnEnemySpawned?.Invoke(enemy);
        }
        
        public static void TriggerEnemyReturnedToPool(GameObject enemy)
        {
            OnEnemyReturnedToPool?.Invoke(enemy);
        }
    }
}