using UnityEngine;

namespace SGC2025.Events
{
    /// <summary>
    /// プレイヤー関連のイベント定義
    /// UI更新、ゲーム状態変更などの疎結合通信を提供
    /// </summary>
    public static class PlayerEvents
    {
        #region プレイヤー状態イベント
        
        /// <summary>プレイヤーがダメージを受けた際のイベント</summary>
        public static event System.Action<float> OnPlayerDamageTaken;
        
        /// <summary>プレイヤーの体力が変化した際のイベント</summary>
        public static event System.Action<float, float> OnPlayerHealthChanged; // (currentHP, maxHP)
        
        /// <summary>プレイヤーが死亡した際のイベント</summary>
        public static event System.Action OnPlayerDeath;
        
        /// <summary>プレイヤーが復活した際のイベント</summary>
        public static event System.Action OnPlayerRespawned;
        
        #endregion
        
        #region プレイヤー行動イベント
        
        /// <summary>プレイヤーが移動した際のイベント</summary>
        public static event System.Action<Vector2> OnPlayerMoved;
        
        /// <summary>プレイヤーが射撃した際のイベント</summary>
        public static event System.Action<Vector3> OnPlayerShot;
        
        /// <summary>プレイヤーの武器が変更された際のイベント</summary>
        public static event System.Action<int> OnPlayerWeaponChanged; // weaponId
        
        #endregion
        
        #region スコア・ゲーム進行イベント
        
        /// <summary>スコアが変化した際のイベント</summary>
        public static event System.Action<int> OnScoreChanged;
        
        /// <summary>レベルアップした際のイベント</summary>
        public static event System.Action<int> OnPlayerLevelUp;
        
        /// <summary>アイテムを取得した際のイベント</summary>
        public static event System.Action<string> OnItemCollected; // itemType
        
        #endregion
        
        #region イベント発火メソッド
        
        /// <summary>プレイヤーダメージイベントを発火</summary>
        public static void TriggerPlayerDamage(float damage, float currentHP, float maxHP)
        {
            OnPlayerDamageTaken?.Invoke(damage);
            OnPlayerHealthChanged?.Invoke(currentHP, maxHP);
        }
        
        /// <summary>プレイヤー死亡イベントを発火</summary>
        public static void TriggerPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }
        
        /// <summary>プレイヤー行動イベントを発火</summary>
        public static void TriggerPlayerAction(Vector2 moveDirection, Vector3 shotPosition, bool hasFired)
        {
            OnPlayerMoved?.Invoke(moveDirection);
            if (hasFired)
            {
                OnPlayerShot?.Invoke(shotPosition);
            }
        }
        
        /// <summary>スコア変更イベントを発火</summary>
        public static void TriggerScoreChange(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
        }
        
        #endregion
        
        #region デバッグ・清掃メソッド
        
        /// <summary>すべてのイベントリスナーをクリア（デバッグ用）</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ClearAllEvents()
        {
            OnPlayerDamageTaken = null;
            OnPlayerHealthChanged = null;
            OnPlayerDeath = null;
            OnPlayerRespawned = null;
            OnPlayerMoved = null;
            OnPlayerShot = null;
            OnPlayerWeaponChanged = null;
            OnScoreChanged = null;
            OnPlayerLevelUp = null;
            OnItemCollected = null;
            
            Debug.Log("[PlayerEvents] すべてのイベントリスナーをクリアしました");
        }
        
        #endregion
    }
}