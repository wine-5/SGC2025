namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の移動タイプを定義する列挙型
    /// </summary>
    public enum MovementType
    {
        /// <summary>
        /// 固定方向移動 - 生成位置から反対側に向かって移動
        /// </summary>
        FixedDirection,
        
        /// <summary>
        /// 直線追従 - プレイヤーに向かって直線で移動
        /// </summary>
        LinearChaser,

        /// <summary>
        /// 慣性追従 - プレイヤーの方向に徐々に向きを変える
        /// </summary>
        InertiaChaser,

        /// <summary>
        /// 予測追従 - プレイヤーの移動先を予測して移動
        /// </summary>
        PredictiveChaser,

        /// <summary>
        /// 円弧追従 - プレイヤーの周りを円弧状に移動
        /// </summary>
        ArcChaser
    }
}