namespace Tyotyo.Core
{
    /// <summary>
    /// 接触などで相手に攻撃力を与えられるオブジェクトのインターフェース。
    /// <para>
    /// 「ダメージを受けられる（<see cref="IDamageable"/>）」とは別概念として切り出す。
    /// 接触ダメージを与える側（敵など）のみが実装する。弾で攻撃するプレイヤーは実装しない。
    /// </para>
    /// </summary>
    public interface IAttacker
    {
        /// <summary>接触などで相手に与える攻撃力</summary>
        float AttackPower { get; }
    }
}
