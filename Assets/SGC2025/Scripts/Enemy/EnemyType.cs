namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の種類を定義する列挙型
    /// </summary>
    public enum EnemyType
    {
        /// <summary>
        /// 基本的な敵。虫取り網少年。
        /// </summary>
        Boy,

        /// <summary>
        /// 直線追従型 - プレイヤーに向かって直線で移動
        /// </summary>
        LinearChaser,

        /// <summary>
        /// 慣性追従型 - プレイヤーの方向に徐々に向きを変える
        /// </summary>
        InertiaChaser,

        /// <summary>
        /// 予測追従型 - プレイヤーの移動先を予測して移動
        /// </summary>
        PredictiveChaser,

        /// <summary>
        /// 円弧追従型 - プレイヤーの周りを円弧状に移動
        /// </summary>
        ArcChaser
    }
}
