using UnityEngine.SceneManagement;

namespace SGC2025
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        Title,
        InGame,
        Result,
    }

    /// <summary>
    /// シーン遷移を管理するSingletonクラス
    /// Titleシーンで一度生成されれば、他のシーンでも利用可能
    /// </summary>
    public class SceneController : Singleton<SceneController>
    {
        protected override bool UseDontDestroyOnLoad => true;

        public SceneName CurrentStage { get; private set; } = SceneName.Title;

        /// <summary>指定されたシーンに切り替え</summary>
        public void LoadScene(SceneName sceneName)
        {
            CurrentStage = sceneName;
            SceneManager.LoadScene(sceneName.ToString());
        }

        /// <summary>次のシーンに進む</summary>
        public void LoadNextScene()
        {
            LoadScene(GetNextScene(CurrentStage));
        }

        private SceneName GetNextScene(SceneName currentScene)
        {
            switch (currentScene)
            {
                case SceneName.Title: return SceneName.InGame;
                case SceneName.InGame: return SceneName.Result;
                case SceneName.Result: return SceneName.Title;
                default: return SceneName.Title;
            }
        }

        /// <summary>ゲームプレイ中かどうかを判定</summary>
        public bool IsInGame() => CurrentStage == SceneName.InGame;
    }
}
