using UnityEngine;

namespace SGC2025.Core
{
    // -------------------------------------------------------
    // Player
    // -------------------------------------------------------

    /// <summary>プレイヤーがダメージを受けた（HP割合: 0〜1）</summary>
    public struct PlayerDamagedEvent : IGameEvent
    {
        public float HpRate;
        public PlayerDamagedEvent(float hpRate) => HpRate = hpRate;
    }

    /// <summary>プレイヤーが死亡した</summary>
    public struct PlayerDiedEvent : IGameEvent { }

    // -------------------------------------------------------
    // Enemy
    // -------------------------------------------------------

    /// <summary>敵が撃破された</summary>
    public struct EnemyDestroyedEvent : IGameEvent
    {
        public Vector3 Position;
        public EnemyDestroyedEvent(Vector3 position)
        {
            Position = position;
        }
    }

    /// <summary>敵がダメージを受けた</summary>
    public struct EnemyDamageTakenEvent : IGameEvent
    {
        public GameObject Enemy;
        public float Damage;
        public float CurrentHp;
        public float MaxHp;
        public EnemyDamageTakenEvent(GameObject enemy, float damage, float currentHp, float maxHp)
        {
            Enemy = enemy;
            Damage = damage;
            CurrentHp = currentHp;
            MaxHp = maxHp;
        }
    }

    /// <summary>敵がスポーンされた</summary>
    public struct EnemySpawnedEvent : IGameEvent
    {
        public SGC2025.Enemy.EnemyController Enemy;
        public EnemySpawnedEvent(SGC2025.Enemy.EnemyController enemy) => Enemy = enemy;
    }

    // -------------------------------------------------------
    // Ground
    // -------------------------------------------------------

    /// <summary>タイルが緑化された</summary>
    public struct GroundGreenifiedEvent : IGameEvent
    {
        public Vector3 Position;
        public GroundGreenifiedEvent(Vector3 position)
        {
            Position = position;
        }
    }

    /// <summary>タイルが非緑化された（茶色に戻った）</summary>
    public struct GroundUngreenifiedEvent : IGameEvent
    {
        public Vector3 Position;
        public GroundUngreenifiedEvent(Vector3 position)
        {
            Position = position;
        }
    }

    // -------------------------------------------------------
    // Game Flow
    // -------------------------------------------------------

    /// <summary>ゲームオーバー（プレイヤー死亡）</summary>
    public struct GameOverEvent : IGameEvent { }

    /// <summary>カウントダウン完了・ゲーム開始</summary>
    public struct CountDownFinishedEvent : IGameEvent { }

    /// <summary>制限時間切れ</summary>
    public struct GameTimeUpEvent : IGameEvent { }

    /// <summary>ゲームがポーズされた</summary>
    public struct PausedEvent : IGameEvent { }

    /// <summary>ポーズが解除された</summary>
    public struct ResumedEvent : IGameEvent { }

    /// <summary>ミニマップ拡大開始</summary>
    public struct MiniMapExpandStartedEvent : IGameEvent { }

    /// <summary>ミニマップ拡大終了</summary>
    public struct MiniMapExpandCanceledEvent : IGameEvent { }

    // -------------------------------------------------------
    // Wave
    // -------------------------------------------------------

    /// <summary>Wave レベルが変更された</summary>
    public struct WaveChangedEvent : IGameEvent
    {
        public int WaveLevel;
        public WaveChangedEvent(int waveLevel) => WaveLevel = waveLevel;
    }

    // -------------------------------------------------------
    // Item
    // -------------------------------------------------------

    /// <summary>アイテム効果が開始した</summary>
    public struct ItemEffectActivatedEvent : IGameEvent
    {
        public SGC2025.Item.ItemType ItemType;
        public float EffectValue;
        public float Duration;
        public ItemEffectActivatedEvent(SGC2025.Item.ItemType itemType, float effectValue, float duration)
        {
            ItemType = itemType;
            EffectValue = effectValue;
            Duration = duration;
        }
    }

    /// <summary>アイテム効果が終了した</summary>
    public struct ItemEffectExpiredEvent : IGameEvent
    {
        public SGC2025.Item.ItemType ItemType;
        public ItemEffectExpiredEvent(SGC2025.Item.ItemType itemType) => ItemType = itemType;
    }
}
