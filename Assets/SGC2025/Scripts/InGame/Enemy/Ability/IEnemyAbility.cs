namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 敵の特殊挙動（ボス能力など）を表すアビリティ。
    /// EnemyControllerが同一GameObject上のアビリティを収集し、生存中に駆動する。
    /// 実装はMonoBehaviourとしてプレハブにアタッチして使う。
    /// </summary>
    public interface IEnemyAbility
    {
        /// <summary>敵が初期化（スポーン）されたときに呼ばれる</summary>
        void OnSpawn(EnemyController owner);

        /// <summary>生存中、毎フレーム呼ばれる</summary>
        void Tick(float deltaTime);

        /// <summary>プールへの返却・死亡などで非アクティブ化されたときに呼ばれる</summary>
        void OnDespawn();
    }
}
