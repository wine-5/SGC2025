using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// Playerの参照を提供するプロバイダー
    /// 他のシステムがPlayerに直接アクセスせずに済むようにする
    /// </summary>
    public class PlayerDataProvider : Singleton<PlayerDataProvider>
    {
        private Transform playerTransform;

        /// <summary>
        /// Playerのトランスフォーム参照
        /// </summary>
        public Transform PlayerTransform => playerTransform;
        
        /// <summary>
        /// Playerが登録されているか
        /// </summary>
        public bool IsPlayerRegistered => playerTransform != null;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        /// <summary>
        /// Playerを登録
        /// </summary>
        /// <param name="player">登録するPlayerのTransform</param>
        public void RegisterPlayer(Transform player)
        {
            if (player == null) return;
            playerTransform = player;
        }
    }
}
