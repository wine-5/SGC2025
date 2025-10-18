using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーのダメージ時のスプライト点滅効果を管理するクラス
/// </summary>
public class PlayerDamageEffect : MonoBehaviour
{
    [Header("コンポーネント設定")]
    [SerializeField] private SpriteRenderer spriteRenderer; // 手動設定可能（空の場合は自動検索）

    [Header("点滅設定")]
    [SerializeField] private float blinkInterval = 0.1f; // 点滅間隔
    [SerializeField] private float minAlpha = 0.3f; // 点滅時の最小透明度
    [SerializeField] private bool useVisibilityBlink = false; // 表示/非表示で点滅（最終手段）
    private Color originalColor;
    private Coroutine blinkCoroutine;
    private bool isBlinking = false;

    public bool IsBlinking => isBlinking;

    private void Awake()
    {
        FindSpriteRenderer();
    }

    private void FindSpriteRenderer()
    {
        // 手動設定がある場合はそれを使用
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            return;
        }

        // 1. 自分自身から検索
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            return;
        }

        // 2. 子階層から検索（最も一般的なケース）
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // 複数のSpriteRendererがある場合の警告
            var allRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (allRenderers.Length > 1)
            {
                Debug.LogWarning($"[PlayerDamageEffect] 複数のSpriteRenderer発見（{allRenderers.Length}個）。手動設定を推奨します。");
            }

            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError($"[PlayerDamageEffect] SpriteRendererが見つかりません - GameObject: {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        // プレイヤーのダメージイベントを購読
        Player.OnPlayerDamaged += OnPlayerDamaged;
    }

    private void OnDisable()
    {
        // プレイヤーのダメージイベントの購読を解除
        Player.OnPlayerDamaged -= OnPlayerDamaged;
    }

    /// <summary>
    /// プレイヤーがダメージを受けた時の処理
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    private void OnPlayerDamaged(float damage)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"[PlayerDamageEffect] SpriteRendererが null です! GameObject: {gameObject.name}");
            return;
        }
        
        // デフォルトで1秒間点滅
        float blinkDuration = 1f;
        StartDamageBlink(blinkDuration);
    }

    /// <summary>
    /// ダメージ点滅を開始
    /// </summary>
    /// <param name="duration">点滅継続時間</param>
    public void StartDamageBlink(float duration)
    {
        if (spriteRenderer == null) 
        {
            Debug.LogError($"[PlayerDamageEffect] StartDamageBlink: SpriteRendererが null です!");
            return;
        }

        // 既存の点滅を停止
        StopDamageBlink();

        // 新しい点滅を開始
        blinkCoroutine = StartCoroutine(BlinkCoroutine(duration));
    }

    /// <summary>
    /// ダメージ点滅を停止
    /// </summary>
    public void StopDamageBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        RestoreOriginalColor();
        isBlinking = false;
    }

    /// <summary>
    /// 点滅コルーチン（透明度ベース）
    /// </summary>
    private IEnumerator BlinkCoroutine(float duration)
    {
        isBlinking = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (useVisibilityBlink)
            {
                // 表示/非表示で点滅（最終手段）
                spriteRenderer.enabled = false;
                yield return new WaitForSeconds(blinkInterval);

                spriteRenderer.enabled = true;
                yield return new WaitForSeconds(blinkInterval);
            }
            else
            {
                // 透明度を下げる（半透明にする）
                Color fadeColor = new Color(originalColor.r, originalColor.g, originalColor.b, minAlpha);
                spriteRenderer.color = fadeColor;
                yield return new WaitForSeconds(blinkInterval);

                // 透明度を元に戻す
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(blinkInterval);
            }

            elapsedTime += blinkInterval * 2f;
        }

        RestoreOriginalColor();
        isBlinking = false;
        blinkCoroutine = null;
    }

    private void RestoreOriginalColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }
    }

    [ContextMenu("Test Damage Blink")]
    public void TestDamageBlink()
    {
        StartDamageBlink(1f);
    }

    [ContextMenu("Force Trigger Damage Event")]
    public void ForceTriggerDamageEvent()
    {
        OnPlayerDamaged(10f); // 10ダメージをシミュレート
    }
}