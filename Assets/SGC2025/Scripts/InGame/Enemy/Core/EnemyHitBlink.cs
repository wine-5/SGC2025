using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵が被弾したときにスプライトを短時間点滅させるヒットフィードバック。
    /// 同じ敵の EnemyController.OnDamageTaken を購読して発動する。
    /// プールで再利用されても OnEnable/OnDisable で購読を張り直すため安全。
    /// </summary>
    public class EnemyHitBlink : MonoBehaviour
    {
        [Header("点滅設定")]
        [SerializeField, Tooltip("点滅の合計時間（秒）")]
        private float blinkDuration = 0.15f;

        [SerializeField, Tooltip("点滅の間隔（秒）")]
        private float blinkInterval = 0.05f;

        [SerializeField, Range(0f, 1f), Tooltip("点滅中の透明度")]
        private float blinkAlpha = 0.3f;

        private SpriteRenderer spriteRenderer;
        private EnemyController controller;
        private Color originalColor;
        private float blinkRemaining;
        private float blinkTimer;
        private bool dimmed;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            controller = GetComponentInParent<EnemyController>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            if (controller != null)
                controller.OnDamageTaken += HandleDamageTaken;
            RestoreColor();
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.OnDamageTaken -= HandleDamageTaken;
            RestoreColor();
        }

        private void HandleDamageTaken(float damage)
        {
            // 被弾のたびに点滅をやり直す
            blinkRemaining = blinkDuration;
            blinkTimer = 0f;
        }

        private void Update()
        {
            if (spriteRenderer == null || blinkRemaining <= 0f) return;

            blinkRemaining -= Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                dimmed = !dimmed;

                Color c = dimmed ? new Color(1, 0, 0, blinkAlpha) : originalColor;
                spriteRenderer.color = c;
            }

            if (blinkRemaining <= 0f)
                RestoreColor();
        }

        private void RestoreColor()
        {
            dimmed = false;
            blinkTimer = 0f;
            blinkRemaining = 0f;

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }
}
