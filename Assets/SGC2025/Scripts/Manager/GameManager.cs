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
        [SerializeField] private float gameOverDelay = 2f; // ゲームオーバーまでの遅延時間
        
        [Header("デバッグ設定")]
        [SerializeField] private bool enableDebugLog = false;
        
        // ゲーム状態
        private bool isGameOver = false;
        private bool isPaused = false;
        
        // イベント
        public static event System.Action OnGameOver;
        public static event System.Action OnGamePause;
        public static event System.Action OnGameResume;
        
        // プロパティ
        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;
        
        protected override void Init()
        {
            base.Init();
            
            // プレイヤーの死亡イベントを購読
            PlayerCharacter.OnPlayerDeath += HandlePlayerDeath;
            
            if (enableDebugLog)
            {
                Debug.Log("[GameManager] 初期化完了");
            }
        }
        
        protected override void OnDestroy()
        {
            // イベントの購読解除
            PlayerCharacter.OnPlayerDeath -= HandlePlayerDeath;
            
            base.OnDestroy();
        }
        
        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
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
            if (enableDebugLog)
            {
                Debug.Log($"[GameManager] {gameOverSceneName}シーンに遷移します");
            }
            
            try
            {
                SceneManager.LoadScene(gameOverSceneName);

                AudioManager.I.PlayBGM("GameOver");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] シーン遷移に失敗しました: {e.Message}");
                // フォールバック：現在のシーンをリロード
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public void PauseGame()
        {
            if (isPaused || isGameOver) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
            
            if (enableDebugLog)
            {
                Debug.Log("[GameManager] ゲームを一時停止しました");
            }
        }
        
        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused || isGameOver) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
            
            if (enableDebugLog)
            {
                Debug.Log("[GameManager] ゲームを再開しました");
            }
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