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


        /// <summary>現在のシーンをリロード</summary>
        public void ReloadCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadtTitleScene() => LoadScene(SceneName.Title);
        public void LoadtInGameScene() => LoadScene(SceneName.InGame);
        public void LoadtResultScene() => LoadScene(SceneName.Result);
    }
}
