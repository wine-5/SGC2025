using UnityEngine;

namespace Tyotyo.Core
{
    /// <summary>
    /// プロジェクト共通のレイヤー名・タグ定数
    /// 文字列・レイヤー番号のハードコードを防ぎ、一元管理する
    /// </summary>
    public static class GameLayers
    {
        // タグ名
        public const string PlayerTag = "Player";

        // レイヤー名
        public const string PlayerLayerName = "Player";
        public const string EnemyLayerName  = "Enemy";

        /// <summary>Player レイヤーのインデックス</summary>
        public static int PlayerLayer => LayerMask.NameToLayer(PlayerLayerName);

        /// <summary>Enemy レイヤーのインデックス</summary>
        public static int EnemyLayer => LayerMask.NameToLayer(EnemyLayerName);
    }
}
