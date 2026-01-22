using UnityEngine;

namespace SGC2025.Events
{
    /// <summary>
    /// 地面関連のイベント定義
    /// 地面の緑化、音、エフェクトなどの疎結合通信を提供
    /// </summary>
    public static class GroundEvents
    {
        /// <summary>地面が緑化されたときのイベント（位置、獲得ポイント）</summary>
        public static event System.Action<Vector3, int> OnGroundGreenified;
        
        /// <summary>
        /// 地面が緑化されたことを通知
        /// </summary>
        /// <param name="position">緑化された位置</param>
        /// <param name="points">獲得したポイント</param>
        public static void TriggerGroundGreenified(Vector3 position, int points)
        {
            OnGroundGreenified?.Invoke(position, points);
        }
        
        /// <summary>
        /// すべてのイベントリスナーをクリア（デバッグ用）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ClearAllEvents()
        {
            OnGroundGreenified = null;
            
            Debug.Log("[GroundEvents] すべてのイベントリスナーをクリアしました");
        }
    }
}
