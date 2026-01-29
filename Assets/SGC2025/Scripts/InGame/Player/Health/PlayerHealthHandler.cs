using System;
using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーのヘルス管理の実装
    /// </summary>
    public class PlayerHealthHandler : IPlayerHealth
    {
        private const float DEFAULT_MAX_HEALTH = 100f;
        private const float DEFAULT_INVINCIBLE_TIME = 1f;
        
        private float maxHealth;
        private float currentHealth;
        private float invincibleTime;
        private float currentInvincibleTime;
        
        /// <summary>現在のヘルス値</summary>
        public float CurrentHealth => currentHealth;
        
        /// <summary>最大ヘルス値</summary>
        public float MaxHealth => maxHealth;
        
        /// <summary>生存状態</summary>
        public bool IsAlive => currentHealth > 0f;
        
        /// <summary>無敵時間中かどうか</summary>
        public bool IsInvincible => currentInvincibleTime > 0f;
        
        /// <summary>ダメージを受けた際のイベント</summary>
        public event Action<float> OnDamageTaken;
        
        /// <summary>死亡時のイベント</summary>
        public event Action OnPlayerDeath;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxHealth">最大ヘルス値</param>
        /// <param name="invincibleTime">無敵時間</param>
        public PlayerHealthHandler(float maxHealth = DEFAULT_MAX_HEALTH, float invincibleTime = DEFAULT_INVINCIBLE_TIME)
        {
            this.maxHealth = maxHealth;
            this.invincibleTime = invincibleTime;
            this.currentHealth = maxHealth;
            this.currentInvincibleTime = 0f;
        }
        
        /// <summary>
        /// 無敵時間の更新（毎フレーム呼び出す）
        /// </summary>
        /// <param name="deltaTime">フレーム間の時間</param>
        public void UpdateInvincibleTime(float deltaTime)
        {
            if (currentInvincibleTime > 0f)
                currentInvincibleTime -= deltaTime;
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        public void TakeDamage(float damage = 1f)
        {
            if (!IsAlive || IsInvincible) return;
            
            float actualDamage = Mathf.Min(damage, currentHealth);
            currentHealth = Mathf.Max(0f, currentHealth - actualDamage);
            currentInvincibleTime = invincibleTime;
            
            OnDamageTaken?.Invoke(actualDamage);
            if (!IsAlive)
                OnPlayerDeath?.Invoke();
        }
        
        /// <summary>
        /// ヘルスを回復
        /// </summary>
        /// <param name="healAmount">回復量</param>
        public void Heal(float healAmount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        }
    }
}