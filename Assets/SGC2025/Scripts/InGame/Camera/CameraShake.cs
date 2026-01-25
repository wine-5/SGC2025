using UnityEngine;

namespace SGC2025.Camera
{
    /// <summary>
    /// カメラシェイクの設定とパラメータを管理するクラス
    /// </summary>
    [System.Serializable]
    public class CameraShake
    {
        [Header("シェイク設定")]
        [SerializeField, Tooltip("シェイクの継続時間（秒）")]
        private float duration = 0.2f;
        
        [SerializeField, Tooltip("シェイクの強さ")]
        private float magnitude = 0.1f;
        
        [SerializeField, Tooltip("低HP時のシェイク強度倍率")]
        private float lowHpMultiplier = 2f;
        
        [SerializeField, Tooltip("低HPと判定するHP割合")]
        [Range(0f, 1f)]
        private float lowHpThreshold = 0.3f;

        public float Duration => duration;
        public float Magnitude => magnitude;

        /// <summary>
        /// HP率に応じたシェイクの強さを取得
        /// </summary>
        public float GetMagnitudeByHpRate(float hpRate)
        {
            if (hpRate <= lowHpThreshold)
                return magnitude * lowHpMultiplier;
            return magnitude;
        }
    }
}
