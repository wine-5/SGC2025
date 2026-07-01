using Tyotyo.Core;
using Tyotyo.UI;
using UnityEngine;

namespace Tyotyo.Manager
{
    /// <summary>
    /// ポーズ機能の管理を行うクラス
    /// 状態管理（TimeScale、UI 表示切り替え）を担当
    /// ボタンイベントは PauseUI が担当
    /// InGameManager 経由で参照する
    /// </summary>
    public class PauseManager : Singleton<PauseManager>
    {
        private bool isPaused;

        public bool IsPaused => isPaused;

        /// <summary>ゲームをポーズする</summary>
        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;

            Time.timeScale = 0f;
            EventBus.Publish(new PausedEvent());
        }

        /// <summary>ポーズを解除する</summary>
        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;

            Time.timeScale = 1f;
            UIFocusHelper.ClearFocus();
            EventBus.Publish(new ResumedEvent());
        }
    }
}