using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// プレイヤーの参照を管理するSingletonクラス
    /// 敵がプレイヤーを追従する際に使用
    /// </summary>
    public class PlayerManager : Singleton<PlayerManager>
    {
        [Header("プレイヤー設定")]
        [SerializeField] private Player player;
        
        /// <summary>
        /// プレイヤーのTransform
        /// </summary>
        public static Transform PlayerTransform => I?.player?.transform;
        
        /// <summary>
        /// プレイヤーの現在位置
        /// </summary>
        public static Vector3 PlayerPosition => PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
        
        /// <summary>
        /// プレイヤーの参照
        /// </summary>
        public static Player PlayerInstance => I?.player;
        
        protected override void Init()
        {
            base.Init();
            
            // プレイヤーが設定されていない場合、"Player"タグで検索
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponent<Player>();
                    if (player != null)
                    {
                        Debug.Log("PlayerManager: Playerタグからプレイヤーを自動検出しました");
                    }
                }
                
                if (player == null)
                {
                    Debug.LogWarning("PlayerManager: プレイヤーが見つかりません。Inspectorで設定するか、Playerタグをつけてください");
                }
            }
        }
        
        /// <summary>
        /// プレイヤーを手動で設定
        /// </summary>
        /// <param name="playerInstance">プレイヤーのインスタンス</param>
        public static void SetPlayer(Player playerInstance)
        {
            if (I != null)
            {
                I.player = playerInstance;
            }
        }
        
        /// <summary>
        /// プレイヤーが設定されているかチェック
        /// </summary>
        /// <returns>設定されている場合true</returns>
        public static bool IsPlayerSet()
        {
            return PlayerTransform != null;
        }
    }
}