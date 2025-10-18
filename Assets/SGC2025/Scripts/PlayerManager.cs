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
        [SerializeField] private Transform playerTransform;
        
        /// <summary>
        /// プレイヤーのTransform
        /// </summary>
        public static Transform PlayerTransform => I?.playerTransform;
        
        /// <summary>
        /// プレイヤーの現在位置
        /// </summary>
        public static Vector3 PlayerPosition => PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
        
        protected override void Init()
        {
            base.Init();
            
            // プレイヤーが設定されていない場合、"Player"タグで検索
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log("PlayerManager: Playerタグからプレイヤーを自動検出しました");
                }
                else
                {
                    Debug.LogWarning("PlayerManager: プレイヤーが見つかりません。Inspectorで設定するか、Playerタグをつけてください");
                }
            }
        }
        
        /// <summary>
        /// プレイヤーのTransformを手動で設定
        /// </summary>
        /// <param name="player">プレイヤーのTransform</param>
        public static void SetPlayer(Transform player)
        {
            if (I != null)
            {
                I.playerTransform = player;
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