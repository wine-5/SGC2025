using UnityEngine;
using UnityEngine.EventSystems;

namespace SGC2025.Manager
{
    /// <summary>
    /// ポーズ機能の管理を行うマネージャー
    /// シーン固有のUI要素を扱うため、DontDestroyOnLoadは使用しない
    /// </summary>
    public class PauseManager : Singleton<PauseManager>
    {
        [Header("ポーズ設定")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject firstPauseButton; // ポーズ時に最初に選択されるボタン

        private bool isPaused;

        public static event System.Action OnPause;
        public static event System.Action OnResume;

        public bool IsPaused => isPaused;
        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            isPaused = false;
        }

        private void Start()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        protected override void OnDestroy()
        {
            // static eventsをクリア
            OnPause = null;
            OnResume = null;
            
            base.OnDestroy();
        }

        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;
            
            if (pausePanel != null)
                pausePanel.SetActive(true);
            else
                Debug.LogWarning("[PauseManager] Cannot pause - PausePanel not assigned");
            
            Time.timeScale = 0f;
            
            if (firstPauseButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstPauseButton);
            
            OnPause?.Invoke();
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
            else
                Debug.LogWarning("[PauseManager] Cannot resume - PausePanel not assigned");
            
            Time.timeScale = 1f;
            
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            
            OnResume?.Invoke();
        }

        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
}