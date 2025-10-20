using UnityEngine;

namespace SGC2025.Enemy
{
    /// <summary>
    /// スポーン位置提供のインターフェース
    /// </summary>
    public interface ISpawnPositionProvider
    {
        /// <summary>
        /// ランダムなスポーン位置を取得
        /// </summary>
        /// <returns>スポーン位置</returns>
        Vector3 GetRandomSpawnPosition();
        
        /// <summary>
        /// 初期化済みかどうか
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();
    }
}