using UnityEngine;
using System;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクターのHP管理を専門に行うクラス
    /// ダメージ処理、死亡判定、無敵時間・バフ拡張に対応
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        #region 定数
        private const int DEFAULT_MAX_HEALTH = 3;
        private const float DEFAULT_INVINCIBILITY_DURATION = 1f;
        private const float DEFAULT_FLASH_INTERVAL = 0.1f;
        private const string DEBUG_LOG_PREFIX = "[CharacterHealth]";
        #endregion

        #region Inspector設定
        [Header("ヘルス設定")]
        [SerializeField] private int maxHealth = DEFAULT_MAX_HEALTH;
        [SerializeField] private int currentHealth;
        [SerializeField] private bool isDead = false;

        [Header("無敵時間設定")]
        [SerializeField] private float invincibilityDuration = DEFAULT_INVINCIBILITY_DURATION;
        [SerializeField] private bool enableInvincibility = true;
        [SerializeField] private float invincibilityTimer = 0f;

        [Header("視覚効果")]
        [SerializeField] private bool enableFlashing = true;
        [SerializeField] private float flashInterval = DEFAULT_FLASH_INTERVAL;
        [SerializeField] private Color damageFlashColor = Color.red;

        [Header("デバッグ")]
        [SerializeField] private bool showHealthInGizmos = true;
        [SerializeField] private bool enableDebugLogs = false;
        #endregion

        #region プライベート変数
        private CharacterController characterController;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float flashTimer = 0f;
        private bool isFlashing = false;
        #endregion

        #region イベント
        /// <summary>ヘルスが変化したときのイベント</summary>
        public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
        
        /// <summary>死亡時のイベント</summary>
        public event Action OnDeath;
        
        /// <summary>無敵状態変化時のイベント</summary>
        public event Action<bool> OnInvincibilityChanged; // (isInvincible)
        
        /// <summary>HP回復時のイベント</summary>
        public event Action<int> OnHealthRestored; // (restoredAmount)
        #endregion

        #region プロパティ
        /// <summary>現在のヘルス値</summary>
        public int CurrentHealth => currentHealth;
        
        /// <summary>最大HP</summary>
        public int MaxHealth => maxHealth;
        
        /// <summary>死亡しているかどうか</summary>
        public bool IsDead => isDead;
        
        /// <summary>生存しているかどうか</summary>
        public bool IsAlive => !isDead && currentHealth > 0;
        
        /// <summary>無敵状態かどうか</summary>
        public bool IsInvincible => invincibilityTimer > 0f;
        
        /// <summary>HPの割合（0.0f～1.0f）</summary>
        public float HealthRatio => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        
        /// <summary>残り無敵時間</summary>
        public float RemainingInvincibilityTime => Mathf.Max(0f, invincibilityTimer);
        #endregion

        #region ハンドラーライフサイクル
        /// <summary>
        /// CharacterControllerによる初期化
        /// </summary>
        /// <param name="controller">親のCharacterController</param>
        public void Initialize(CharacterController controller)
        {
            characterController = controller;
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            else
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} SpriteRenderer not found. Flashing effects will be disabled.");
            }

            ResetHealth();

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Initialized with {currentHealth}/{maxHealth} HP");
            }
        }

        public void OnStart() { }

        public void OnEnable() { }

        public void OnDisable() 
        {
            // 無効化時に点滅を停止
            StopFlashing();
        }
        #endregion

        #region 更新処理
        private void Update()
        {
            UpdateInvincibility();
            UpdateVisualEffects();
        }

        /// <summary>
        /// 無敵時間の更新
        /// </summary>
        private void UpdateInvincibility()
        {
            if (invincibilityTimer > 0f)
            {
                float previousTimer = invincibilityTimer;
                invincibilityTimer -= Time.deltaTime;
                
                // 無敵状態が終了した時
                if (previousTimer > 0f && invincibilityTimer <= 0f)
                {
                    OnInvincibilityChanged?.Invoke(false);
                    StopFlashing();
                }
            }
        }

        /// <summary>
        /// ビジュアルエフェクトの更新
        /// </summary>
        private void UpdateVisualEffects()
        {
            if (isFlashing && spriteRenderer != null)
            {
                flashTimer += Time.deltaTime;
                
                if (flashTimer >= flashInterval)
                {
                    flashTimer = 0f;
                    spriteRenderer.color = spriteRenderer.color == originalColor ? 
                        damageFlashColor : originalColor;
                }
            }
        }
        #endregion

        #region ヘルス管理
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        /// <param name="ignoreInvincibility">無敵状態を無視するか</param>
        /// <returns>実際にダメージを受けたかどうか</returns>
        public bool TakeDamage(int damage, bool ignoreInvincibility = false)
        {
            // 死亡している場合はダメージを受けない
            if (isDead) return false;
            
            // 無敵状態かつ無視しない場合はダメージを受けない
            if (IsInvincible && !ignoreInvincibility) return false;
            
            // ダメージが0以下の場合は処理しない
            if (damage <= 0) return false;

            int previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // 無敵時間を設定
            if (enableInvincibility && !ignoreInvincibility)
            {
                SetInvincible(invincibilityDuration);
            }

            // ビジュアルエフェクト開始
            StartFlashing();

            // イベント通知
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // 死亡判定
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Took {damage} damage. Health: {currentHealth}/{maxHealth}");
            }

            return true;
        }

        /// <summary>
        /// HPを回復する
        /// </summary>
        /// <param name="amount">回復量</param>
        /// <returns>実際に回復した量</returns>
        public int RestoreHealth(int amount)
        {
            if (isDead || amount <= 0) return 0;

            int previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            int actualRestored = currentHealth - previousHealth;

            if (actualRestored > 0)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                OnHealthRestored?.Invoke(actualRestored);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"{DEBUG_LOG_PREFIX} Restored {actualRestored} health. Health: {currentHealth}/{maxHealth}");
                }
            }

            return actualRestored;
        }

        /// <summary>
        /// HPを完全回復する
        /// </summary>
        public void FullRestore()
        {
            RestoreHealth(maxHealth - currentHealth);
        }

        /// <summary>
        /// 最大HPを設定（現在HPも調整）
        /// </summary>
        /// <param name="newMaxHealth">新しい最大HP</param>
        /// <param name="adjustCurrentHealth">現在HPも比例して調整するか</param>
        public void SetMaxHealth(int newMaxHealth, bool adjustCurrentHealth = false)
        {
            if (newMaxHealth <= 0)
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} Cannot set max health to {newMaxHealth}. Must be positive.");
                return;
            }

            if (adjustCurrentHealth && maxHealth > 0)
            {
                float ratio = HealthRatio;
                maxHealth = newMaxHealth;
                currentHealth = Mathf.RoundToInt(maxHealth * ratio);
            }
            else
            {
                maxHealth = newMaxHealth;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// HPをリセット（最大値に設定）
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            invincibilityTimer = 0f;
            StopFlashing();
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        #endregion

        #region 無敵状態管理
        /// <summary>
        /// 無敵状態を設定
        /// </summary>
        /// <param name="duration">無敵時間</param>
        public void SetInvincible(float duration)
        {
            if (duration <= 0f) return;

            bool wasInvincible = IsInvincible;
            invincibilityTimer = duration;
            
            if (!wasInvincible)
            {
                OnInvincibilityChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// 無敵状態を即座に解除
        /// </summary>
        public void ClearInvincibility()
        {
            if (IsInvincible)
            {
                invincibilityTimer = 0f;
                OnInvincibilityChanged?.Invoke(false);
                StopFlashing();
            }
        }
        #endregion

        #region 死亡管理
        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            invincibilityTimer = 0f;
            StopFlashing();

            // 死亡イベント通知
            OnDeath?.Invoke();
            
            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Character {gameObject.name} has died");
            }
        }

        /// <summary>
        /// 即座に死亡させる
        /// </summary>
        public void InstantKill()
        {
            currentHealth = 0;
            Die();
        }

        /// <summary>
        /// 蘇生処理
        /// </summary>
        /// <param name="healthAmount">蘇生時のHP（-1で最大HP）</param>
        public void Revive(int healthAmount = -1)
        {
            if (!isDead) return;

            isDead = false;
            currentHealth = healthAmount < 0 ? maxHealth : Mathf.Min(healthAmount, maxHealth);
            invincibilityTimer = 0f;
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Character {gameObject.name} revived with {currentHealth} HP");
            }
        }
        #endregion

        #region ビジュアルエフェクト
        /// <summary>
        /// 点滅エフェクト開始
        /// </summary>
        private void StartFlashing()
        {
            if (!enableFlashing || spriteRenderer == null) return;
            
            isFlashing = true;
            flashTimer = 0f;
        }

        /// <summary>
        /// 点滅エフェクト停止
        /// </summary>
        private void StopFlashing()
        {
            if (!isFlashing || spriteRenderer == null) return;
            
            isFlashing = false;
            spriteRenderer.color = originalColor;
        }
        #endregion

        #region Debug
        private void OnDrawGizmos()
        {
            if (!showHealthInGizmos) return;

            // HPバーの描画
            Vector3 position = transform.position + Vector3.up * 1.5f;
            float barWidth = 1f;
            float barHeight = 0.1f;
            
            // 背景（最大HP）
            Gizmos.color = Color.red;
            Gizmos.DrawCube(position, new Vector3(barWidth, barHeight, 0.1f));
            
            // 現在HP
            if (maxHealth > 0)
            {
                float healthRatio = HealthRatio;
                Gizmos.color = Color.green;
                Vector3 healthBarPos = position - Vector3.right * barWidth * (1 - healthRatio) * 0.5f;
                Gizmos.DrawCube(healthBarPos, new Vector3(barWidth * healthRatio, barHeight, 0.1f));
            }

            // 無敵状態の可視化
            if (IsInvincible)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
        }
        #endregion
    }
}