using Tyotyo.InGame.Enemy;

namespace Tyotyo.InGame.Enemy
{
    /// <summary>
    /// 移動タイプに応じて移動戦略を生成するファクトリー
    /// </summary>
    public static class MovementStrategyFactory
    {
        // LinearMovementStrategyは内部状態を持たないため、全敵で1インスタンスを共有してスポーンごとの確保を避ける。
        // 他の戦略（Inertia/Predictive/Arc）は敵ごとの状態を保持するため共有できず、都度生成する。
        private static readonly LinearMovementStrategy SharedLinearStrategy = new LinearMovementStrategy();

        /// <summary>
        /// 移動タイプに応じた移動戦略を作成
        /// </summary>
        /// <param name="movementType">移動タイプ</param>
        /// <returns>移動戦略（固定方向移動の場合はnull）</returns>
        public static IMovementStrategy CreateStrategy(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.FixedDirection => null, // 固定方向移動は戦略なし
                MovementType.LinearChaser => SharedLinearStrategy,
                MovementType.InertiaChaser => new InertiaMovementStrategy(),
                MovementType.PredictiveChaser => new PredictiveMovementStrategy(),
                MovementType.ArcChaser => new ArcMovementStrategy(),
                _ => null
            };
        }
    }
}