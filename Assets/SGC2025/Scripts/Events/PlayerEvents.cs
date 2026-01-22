using UnityEngine;

namespace SGC2025.Events
{
    /// <summary>
    /// プレイヤー関連のイベント定義
    /// UI更新、ゲーム状態変更などの疎結合通信を提供
    /// </summary>
    public static class PlayerEvents
    {
        public static event System.Action<float> OnPlayerDamageTaken;
        public static event System.Action<float, float> OnPlayerHealthChanged;
        public static event System.Action OnPlayerDeath;
        public static event System.Action OnPlayerRespawned;
        public static event System.Action<Vector2> OnPlayerMoved;
        public static event System.Action<Vector3> OnPlayerShot;
        public static event System.Action<int> OnPlayerWeaponChanged;
        public static event System.Action<int> OnScoreChanged;
        public static event System.Action<int> OnPlayerLevelUp;
        public static event System.Action<string> OnItemCollected;
    }
}
