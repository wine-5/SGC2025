using UnityEngine;
using UnityEngine.UI;

namespace SGC2025
{
    /// <summary>
    /// Playerがダメージを受けた時の画面フラッシュ演出
    /// 画面端を赤く点滅させる
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFlashEffect : MonoBehaviour
    {
        [Header("フラッシュ設定")]
        [SerializeField, Tooltip("フラッシュの継続時間")]
        private float flashDuration = 0.2f;
        
        [SerializeField, Tooltip("通常時のフラッシュ色")]
        private Color normalFlashColor = new Color(1f, 0f, 0f, 0.3f); // 赤、30%透明度
        
        [SerializeField, Tooltip("低HP時のフラッシュ色")]
        private Color lowHpFlashColor = new Color(1f, 0f, 0f, 0.6f); // 赤、60%透明度
        
        [SerializeField, Tooltip("低HPと判定するHP割合")]
        [Range(0f, 1f)]
        private float lowHpThreshold = 0.3f;
        
        private Image flashImage;
        private float flashTimer;
        private Color currentFlashColor;

        private void Awake()
        {
            flashImage = GetComponent<Image>();
            flashImage.color = Color.clear;
            
            // Playerのダメージイベントを購読
            PlayerCharacter.OnPlayerDamaged += HandlePlayerDamaged;
        }

        private void OnDestroy()
        {
            // イベント購読解除
            PlayerCharacter.OnPlayerDamaged -= HandlePlayerDamaged;
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                float t = flashTimer / flashDuration;
                float alpha = currentFlashColor.a * t;
                Color color = currentFlashColor;
                color.a = alpha;
                flashImage.color = color;
                
                flashTimer -= Time.deltaTime;
            }
            else if (flashImage.color.a > 0f)
                flashImage.color = Color.clear;
        }

        /// <summary>Playerがダメージを受けた時の処理</summary>
        private void HandlePlayerDamaged(float hpRate) => TriggerFlash(hpRate);

        /// <summary>フラッシュ演出をトリガー</summary>
        public void TriggerFlash(float hpRate)
        {
            currentFlashColor = hpRate <= lowHpThreshold ? lowHpFlashColor : normalFlashColor;
            flashTimer = flashDuration;
        }
    }
}
