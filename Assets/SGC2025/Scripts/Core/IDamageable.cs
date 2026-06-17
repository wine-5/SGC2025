using System;

namespace SGC2025.Core
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// <para>
    /// 仲介者として機能し、攻撃側（BulletController 等）が被ダメージ側（EnemyController 等）に
    /// 直接依存しないようにする。攻撃側はこのインターフェースのみを知れば十分。
    /// </para>
    /// </summary>
    public interface IDamageable
    {
        /// <summary>現在の体力値</summary>
        float CurrentHealth { get; }

        /// <summary>最大体力値</summary>
        float MaxHealth { get; }

        /// <summary>生存状態</summary>
        bool IsAlive { get; }

        /// <summary>ダメージを受けた際のイベント</summary>
        event Action<float> OnDamageTaken;

        /// <summary>死亡時のイベント</summary>
        event Action OnDeath;

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        void TakeDamage(float damage);
    }
}
