using Polychroma.Core.Log;
#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SGC2025.Core
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
                }
                else
                {
                    Initialized = false;
                    CusLog.Error("[SteamManager] Failed to initialize Steam API.");
                }
            }
            catch (System.DllNotFoundException)
            {
                Initialized = false;
                CusLog.Error("[SteamManager] Steam DLL not found. Running in offline mode.");
            }
#else
            Initialized = false;
#endif
        }

        protected override void OnDestroy()
        {
#if STEAMWORKS_NET
            if (Initialized)
            {
                Steamworks.SteamAPI.Shutdown();
                CusLog.Log("[SteamManager] Steam API shutdown.");
            }
#endif
            base.OnDestroy();
        }
    }
}
