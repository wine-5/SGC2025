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
                    CusLog.Log("[SteamManager] Steam API successfully initialized.");
                }
                else
                {
                    Initialized = false;
                    CusLog.Error("[SteamManager] Failed to initialize Steam API. Is Steam running?");
                }
            }
            catch (System.DllNotFoundException)
            {
                Initialized = false;
                CusLog.Error("[SteamManager] Steam DLL not found. Running in offline mode.");
            }
#else
            Initialized = false;
            CusLog.Warning("[SteamManager] STEAMWORKS_NET symbol is not defined. Running in offline mode.");
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
                CusLog.Log("[SteamManager] Steam API shutdown.");
            }
#endif
            base.OnDestroy();
        }
    }
}