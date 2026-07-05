using Tyotyo.Core;
namespace Tyotyo.Manager
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        TitleSelect,
        TitleSelect_Steam,
        TitleSelect_Exhibition,
        InGame,
        Result,
    }

    /// <summary>
    /// シーン遷移を管理するSingletonクラス
    /// Titleシーンで一度生成されれば、他のシーンでも利用可能
    /// 遷移は SceneTransition による「緑化タイルめくり」演出を挟んで行う。
    /// </summary>
    public class SceneController : Singleton<SceneController>
    {
        protected override bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// 指定されたシーンに切り替え（緑化タイルめくり演出つき）。
        /// 実際の「覆う→読み込み→晴れる」フローは SceneTransition が担当する。
        /// </summary>
        public void LoadScene(SceneName sceneName)
        {
            SceneTransition.I.TransitionTo(sceneName.ToString());
        }

        /// <summary>
        /// Resultシーンを読み込む
        /// </summary>
        public void LoadResultScene() => LoadScene(SceneName.Result);
    }
}
