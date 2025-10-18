using SGC2025.Enemy.Interface;
using SGC2025.Enemy.Strategy;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の種類に応じて移動戦略を生成するファクトリー
    /// </summary>
    public static class MovementStrategyFactory
    {
        /// <summary>
        /// 敵の種類に応じた移動戦略を作成
        /// </summary>
        /// <param name="enemyType">敵の種類</param>
        /// <returns>移動戦略</returns>
        public static IMovementStrategy CreateStrategy(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Boy => null,
                EnemyType.LinearChaser => new LinearMovementStrategy(),
                EnemyType.InertiaChaser => new InertiaMovementStrategy(),
                EnemyType.PredictiveChaser => new PredictiveMovementStrategy(),
                EnemyType.ArcChaser => new ArcMovementStrategy(),
                // 従来の敵タイプは戦略なし（固定位置移動を使用）
                _ => null
            };
        }
    }
}