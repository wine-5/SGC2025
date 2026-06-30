using UnityEngine;
using Tyotyo.Ranking;

namespace Tyotyo.Core
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
        public int GreeningSize;        // 撃破時に緑化する一辺のマス数（基本）
        public int GreeningSizeBoosted; // 緑化範囲上昇アイテム中の一辺のマス数
        public bool IsBoss;             // ボスかどうか（緑化SEの出し分けに使用）
        public EnemyDestroyedEvent(Vector3 position, int greeningSize, int greeningSizeBoosted, bool isBoss = false)
        {
            Position = position;
            GreeningSize = greeningSize;
            GreeningSizeBoosted = greeningSizeBoosted;
            IsBoss = isBoss;
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
        public Tyotyo.InGame.Enemy.EnemyController Enemy;
        public EnemySpawnedEvent(Tyotyo.InGame.Enemy.EnemyController enemy) => Enemy = enemy;
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

    /// <summary>範囲緑化が成立した（中心位置と一辺のマス数）。エフェクト/SE再生は購読側が担当する</summary>
    public struct GroundAreaGreenifiedEvent : IGameEvent
    {
        public Vector3 CenterPosition;
        public int Size;
        public GroundAreaGreenifiedEvent(Vector3 centerPosition, int size)
        {
            CenterPosition = centerPosition;
            Size = size;
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

    /// <summary>Leaderboard のエントリー取得（更新）が完了した</summary>
    public struct LeaderboardEntriesUpdatedEvent : IGameEvent
    {
        public LeaderboardType Type;
        public LeaderboardEntriesUpdatedEvent(LeaderboardType type) => Type = type;
    }

    /// <summary>Leaderboard にランクインした（どのランキングかは Type で区別）</summary>
    public struct LeaderboardRankedInEvent : IGameEvent
    {
        public int Rank;
        public int Score;
        public LeaderboardType Type;

        public LeaderboardRankedInEvent(int rank, int score, LeaderboardType type)
        {
            Rank = rank;
            Score = score;
            Type = type;
        }
    }
    
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

    /// <summary>アイテムを取得した（種類を問わず発行。取得演出用）</summary>
    public struct ItemCollectedEvent : IGameEvent
    {
        public Tyotyo.InGame.Item.ItemType ItemType;
        public ItemCollectedEvent(Tyotyo.InGame.Item.ItemType itemType) => ItemType = itemType;
    }

    /// <summary>アイテム効果が開始した</summary>
    public struct ItemEffectActivatedEvent : IGameEvent
    {
        public Tyotyo.InGame.Item.ItemType ItemType;
        public float EffectValue;
        public float Duration;
        public ItemEffectActivatedEvent(Tyotyo.InGame.Item.ItemType itemType, float effectValue, float duration)
        {
            ItemType = itemType;
            EffectValue = effectValue;
            Duration = duration;
        }
    }

    /// <summary>アイテム効果が終了した</summary>
    public struct ItemEffectExpiredEvent : IGameEvent
    {
        public Tyotyo.InGame.Item.ItemType ItemType;
        public ItemEffectExpiredEvent(Tyotyo.InGame.Item.ItemType itemType) => ItemType = itemType;
    }
}
