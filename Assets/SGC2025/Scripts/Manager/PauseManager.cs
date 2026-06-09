using SGC2025.Core;
using SGC2025.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SGC2025.Manager
{
    /// <summary>
    /// ポーズ機能の管理を行うクラス
    /// PauseGame/ResumeGame が呼ばれたら View の切り替えと TimeScale の操作を行う
    /// InGameManager 経由で参照する
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("ポーズ設定")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject firstPauseButton;

        private bool isPaused;

        public bool IsPaused => isPaused;

        private void Start()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        /// <summary>ゲームをポーズする</summary>
        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;

            if (pausePanel != null)
                pausePanel.SetActive(true);
            else
                Debug.LogWarning("[PauseManager] Cannot pause - PausePanel not assigned");

            Time.timeScale = 0f;
            UIFocusHelper.SetFocus(firstPauseButton);
            EventBus.Publish(new PausedEvent());
        }

        /// <summary>ポーズを解除する</summary>
        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;

            if (pausePanel != null)
                pausePanel.SetActive(false);
            else
                Debug.LogWarning("[PauseManager] Cannot resume - PausePanel not assigned");

            Time.timeScale = 1f;
            UIFocusHelper.ClearFocus();
            EventBus.Publish(new ResumedEvent());
        }
    }
}