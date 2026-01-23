namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵のパラメーター情報を提供するインターフェース
    /// </summary>
    public interface IEnemyParameters
    {
        /// <summary>移動速度</summary>
        float MoveSpeed { get; }
        
        /// <summary>敵の種類</summary>
        EnemyType EnemyType { get; }
        
        /// <summary>生存時間</summary>
        float LifeTime { get; }
        
        /// <summary>使用中の敵データ</summary>
        EnemyDataSO EnemyData { get; }
        
        /// <summary>現在のウェーブレベル</summary>
        int CurrentWaveLevel { get; }
    }
}