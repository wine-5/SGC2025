using UnityEngine;
using UnityEngine.SceneManagement;
using TechC;

namespace SGC2025
{
    /// <summary>
    /// ゲーム全体の状態管理を行うマネージャー
    /// プレイヤーの死亡処理、ゲームオーバー、ポーズ機能などを提供
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("ゲーム設定")]
        [SerializeField] private string gameOverSceneName = "Gameover";
        [SerializeField] private float gameOverDelay = 2f;

        private bool isGameOver = false;
        private bool isPaused = false;

        public static event System.Action OnGameOver;
        public static event System.Action OnGamePause;
        public static event System.Action OnGameResume;

        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;
        protected override bool UseDontDestroyOnLoad => true;


        protected override void Init()
        {
            base.Init();
            PlayerCharacter.OnPlayerDeath += HandlePlayerDeath;
        }

        protected override void OnDestroy()
        {
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;

            base.OnDestroy();
        }

        /// <summary>プレイヤー死亡時の処理</summary>
        private void HandlePlayerDeath()
        {
            if (isGameOver) return;
            Debug.Log("[GameManager] プレイヤーが死亡しました。ゲームオーバー処理を開始します");
            isGameOver = true;
            OnGameOver?.Invoke();

            // 遅延後にゲームオーバーシーンに遷移
            Invoke(nameof(LoadGameOverScene), gameOverDelay);
        }

        /// <summary>
        /// ゲームオーバーシーンに遷移
        /// </summary>
        private void LoadGameOverScene()
        {
            try
            {
                SceneManager.LoadScene(gameOverSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] シーン遷移に失敗しました: {e.Message}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>ゲームを一時停止</summary>
        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
        }

        /// <summary>ゲームを再開</summary>
        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// ゲームを強制終了（デバッグ用）
        /// </summary>
        [ContextMenu("Force Game Over")]
        public void ForceGameOver()
        {
            HandlePlayerDeath();
        }

        /// <summary>
        /// 現在のシーンをリロード
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f; // TimeScaleをリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}