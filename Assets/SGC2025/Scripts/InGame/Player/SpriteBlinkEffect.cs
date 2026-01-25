using UnityEngine;
using SGC2025.Player;

namespace SGC2025.Effects
{
    /// <summary>
    /// 無敵時間中のスプライト点滅演出
    /// </summary>
    public class SpriteBlinkEffect : MonoBehaviour
    {
        [Header("点滅設定")]
        [SerializeField, Tooltip("点滅の間隔（秒）")]
        private float blinkInterval = 0.1f;
        
        [SerializeField, Tooltip("無敵時間中の透明度")]
        [Range(0f, 1f)]
        private float blinkAlpha = 0.3f;
        
        private SpriteRenderer spriteRenderer;
        private PlayerCharacter player;
        private float blinkTimer;
        private bool isBlinking;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            player = GetComponentInParent<PlayerCharacter>();
            
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
            
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
            if (player == null || spriteRenderer == null) return;
            
            if (player.IsInvincible)
            {
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    isBlinking = !isBlinking;
                    
                    Color color = originalColor;
                    color.a = isBlinking ? blinkAlpha : 1f;
                    spriteRenderer.color = color;
                }
            }
            else if (spriteRenderer.color.a != originalColor.a)
            {
                spriteRenderer.color = originalColor;
                isBlinking = false;
                blinkTimer = 0f;
            }
        }

        /// <summary>Playerがダメージを受けた時の処理</summary>
        private void HandlePlayerDamaged(float hpRate) => blinkTimer = 0f;
    }
}
