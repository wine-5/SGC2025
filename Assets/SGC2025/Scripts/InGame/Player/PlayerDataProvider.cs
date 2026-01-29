using UnityEngine;
using System;

namespace SGC2025.Player
{
    /// <summary>
    /// Playerの参照を提供するプロバイダー
    /// 他のシステムがPlayerに直接アクセスせずに済むようにする
    /// </summary>
    public class PlayerDataProvider : Singleton<PlayerDataProvider>
    {
        private Transform playerTransform;

        public static event Action<Transform> OnPlayerRegistered;
        public static event Action OnPlayerUnregistered;
        
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
            OnPlayerRegistered?.Invoke(playerTransform);
        }
        
        /// <summary>
        /// Player登録を解除
        /// </summary>
        public void UnregisterPlayer()
        {
            playerTransform = null;
            OnPlayerUnregistered?.Invoke();
        }

        protected override void OnDestroy()
        {
            // シーン再読み込み時に static event が残っていると多重購読の原因になるためクリア
            if (I == (this as PlayerDataProvider))
            {
                OnPlayerRegistered = null;
                OnPlayerUnregistered = null;
            }

            base.OnDestroy();
        }
        
        /// <summary>
        /// Playerの位置を取得
        /// </summary>
        public Vector3 GetPlayerPosition()
        {
            if (playerTransform == null) return Vector3.zero;
            return playerTransform.position;
        }
    }
}
