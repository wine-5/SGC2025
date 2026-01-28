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
        /// <summary>倍率適用後の最終ポイントが加算されたときのイベント（UI表示用）</summary>
        public static event System.Action<Vector3, int> OnGreenScoreAdded;
        
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
        /// 倍率適用後のポイントが加算されたことを通知（UI表示用）
        /// </summary>
        /// <param name="position">緑化された位置</param>
        /// <param name="finalPoints">倍率適用後の最終ポイント</param>
        public static void TriggerGreenScoreAdded(Vector3 position, int finalPoints)
        {
            OnGreenScoreAdded?.Invoke(position, finalPoints);
        }
    }
}
