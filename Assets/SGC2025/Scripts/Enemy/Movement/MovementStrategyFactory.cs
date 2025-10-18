using SGC2025.Enemy.Interface;
using SGC2025.Enemy.Strategy;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 移動タイプに応じて移動戦略を生成するファクトリー
    /// </summary>
    public static class MovementStrategyFactory
    {
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
                MovementType.LinearChaser => new LinearMovementStrategy(),
                MovementType.InertiaChaser => new InertiaMovementStrategy(),
                MovementType.PredictiveChaser => new PredictiveMovementStrategy(),
                MovementType.ArcChaser => new ArcMovementStrategy(),
                _ => null
            };
        }
        
        /// <summary>
        /// 敵の種類に応じた移動戦略を作成（後方互換性のため）
        /// </summary>
        /// <param name="enemyType">敵の種類</param>
        /// <returns>移動戦略</returns>
        [System.Obsolete("Use CreateStrategy(MovementType) instead")]
        public static IMovementStrategy CreateStrategy(EnemyType enemyType)
        {
            // デフォルトは固定方向移動
            return null;
        }
    }
}