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

        /// <summary>
        /// 現在のシーン情報
        /// </summary>
        public SceneName CurrentStage { get; private set; } = SceneName.Title;

        /// <summary>
        /// enumで指定されたシーンに切り替え
        /// </summary>
        /// <param name="sceneName">遷移先のシーン</param>
        public void LoadScene(SceneName sceneName)
        {
            CurrentStage = sceneName;
            string sceneNameStr = sceneName.ToString();
            SceneManager.LoadScene(sceneNameStr);
        }

        /// <summary>
        /// 次のシーンに進む
        /// </summary>
        public void LoadNextScene()
        {
            SceneName nextScene = GetNextScene(CurrentStage);
            LoadScene(nextScene);
        }

        /// <summary>
        /// 指定したシーンの次のシーンを取得
        /// </summary>
        /// <param name="currentScene">現在のシーン</param>
        /// <returns>次のシーン</returns>
        private SceneName GetNextScene(SceneName currentScene)
        {
            switch (currentScene)
            {
                case SceneName.Title:
                    return SceneName.InGame;
                case SceneName.InGame:
                    return SceneName.Result;
                case SceneName.Result:
                    return SceneName.Title;
                default:
                    return SceneName.Title;
            }
        }

        /// <summary>
        /// 現在のシーンがゲームプレイ中かどうかを判定
        /// </summary>
        /// <returns>ゲームプレイ中の場合true</returns>
        public bool IsInGame() => CurrentStage == SceneName.InGame;
    }
}
