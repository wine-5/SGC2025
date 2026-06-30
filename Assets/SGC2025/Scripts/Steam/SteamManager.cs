using Tyotyo.Core.Log;
#if STEAMWORKS_NET
using Steamworks;
#endif

namespace Tyotyo.Core
{
    /// <summary>
    /// Steam 初期化・管理を行うマネージャー
    /// ゲーム起動時に自動的に Steam を初期化する
    /// </summary>
    public class SteamManager : Singleton<SteamManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        public static bool Initialized { get; private set; }

        private void Start()
        {
#if STEAMWORKS_NET
            try
            {
                if (Steamworks.SteamAPI.Init())
                {
                    Initialized = true;
                    CusLog.Log("[SteamManager] Steam APIの初期化に成功しました。");
                }
                else
                {
                    Initialized = false;
                    CusLog.Error("[SteamManager] Steam APIの初期化に失敗しました。Steamが起動していますか？");
                }
            }
            catch (System.DllNotFoundException)
            {
                Initialized = false;
                CusLog.Error("[SteamManager] Steam DLLが見つかりません。オフラインモードで動作します。");
            }
#else
            Initialized = false;
            CusLog.Warning("[SteamManager] STEAMWORKS_NETシンボルが定義されていません。オフラインモードで動作します。");
#endif
        }

        private void Update()
        {
#if STEAMWORKS_NET
            if (Initialized)
            {
                Steamworks.SteamAPI.RunCallbacks();
            }
#endif
        }

        protected override void OnDestroy()
        {
#if STEAMWORKS_NET
            if (Initialized)
            {
                Steamworks.SteamAPI.Shutdown();
                Initialized = false; 
                CusLog.Log("[SteamManager] Steam APIをシャットダウンしました。");
            }
#endif
            base.OnDestroy();
        }
    }
}