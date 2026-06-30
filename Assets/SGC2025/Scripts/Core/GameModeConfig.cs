using UnityEngine;
using Tyotyo.Core.Log;

namespace Tyotyo.Core
{
    /// <summary>
    /// ゲームの動作モード（展示用 / Steam用）を切り替える設定
    /// GameManager の SerializeField から参照を注入する（Resources.Load は使わない）
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "SGC2025/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        private const string LOG_CATEGORY = "GameMode";

        /// <summary>動作モード</summary>
        public enum GameMode
        {
            Exhibition, // 展示用（ローカル保存・名前入力あり）
            Steam,      // Steam用（Leaderboard 自動登録）
        }

        [SerializeField]
        private GameMode mode = GameMode.Exhibition;

        public GameMode Mode => mode;

        /// <summary>現在の設定（GameManager から注入される）</summary>
        public static GameModeConfig Current { get; private set; }

        /// <summary>
        /// 設定を登録する（GameManager の Awake から呼ぶ）
        /// </summary>
        public static void Register(GameModeConfig config)
        {
            Current = config;

            if (Current == null)
                CusLog.Error(LOG_CATEGORY, "GameModeConfig is not assigned to GameManager.");
        }

        /// <summary>
        /// Steam モードかどうか（未設定時は false=展示用として扱う）
        /// </summary>
        public static bool UseSteam => Current is not null && Current.Mode == GameMode.Steam;
    }
}
