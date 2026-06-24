using UnityEngine;

namespace Tyotyo.InGame.Player
{
    /// <summary>
    /// プレイヤーのクローン（自動発射する蝶）を管理する。
    /// あらかじめ子に配置しておいた非アクティブのクローンを、
    /// アイテム取得で先頭から順に1体ずつ有効化する（最大 = 配列の長さ）。
    /// </summary>
    public class PlayerCloneManager : MonoBehaviour
    {
        [SerializeField, Tooltip("子に配置した非アクティブのクローン（上下左右など）。先頭から順に有効化される")]
        private GameObject[] clones;

        /// <summary>有効化済みのクローン数</summary>
        public int ActiveCloneCount { get; private set; }

        /// <summary>クローンの最大数</summary>
        public int MaxCloneCount => clones != null ? clones.Length : 0;

        /// <summary>まだクローンを追加できるか</summary>
        public bool CanAddClone => ActiveCloneCount < MaxCloneCount;

        private void Awake()
        {
            // 初期状態を全て非アクティブに揃える
            if (clones == null) return;

            foreach (var clone in clones)
            {
                if (clone != null)
                    clone.SetActive(false);
            }
            ActiveCloneCount = 0;
        }

        /// <summary>
        /// 次のクローンを1体有効化する。
        /// </summary>
        /// <returns>有効化できたら true、上限到達なら false</returns>
        public bool TryActivateNextClone()
        {
            if (!CanAddClone) return false;

            GameObject clone = clones[ActiveCloneCount];
            if (clone == null) return false;

            clone.SetActive(true);
            ActiveCloneCount++;
            return true;
        }
    }
}
