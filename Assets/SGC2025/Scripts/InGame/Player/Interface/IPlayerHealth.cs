using System;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーのヘルス管理を抽象化するインターフェース
    /// </summary>
    public interface IPlayerHealth
    {
        /// <summary>現在のヘルス値</summary>
        float CurrentHealth { get; }
        
        /// <summary>最大ヘルス値</summary>
        float MaxHealth { get; }
        
        /// <summary>生存状態</summary>
        bool IsAlive { get; }
        
        /// <summary>無敵時間中かどうか</summary>
        bool IsInvincible { get; }
        
        /// <summary>ダメージを受けた際のイベント</summary>
        event Action<float> OnDamageTaken;
        
        /// <summary>死亡時のイベント</summary>
        event Action OnPlayerDeath;
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        void TakeDamage(float damage = 1f);
        
        /// <summary>
        /// ヘルスを回復
        /// </summary>
        /// <param name="healAmount">回復量</param>
        void Heal(float healAmount);
    }
}