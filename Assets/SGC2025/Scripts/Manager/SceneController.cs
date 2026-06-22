using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SGC2025.Manager
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        TitleSelect,
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

        private bool isTransitioning;

        /// <summary>
        /// 指定されたシーンに切り替え（タイルめくり演出つき）
        /// </summary>
        public void LoadScene(SceneName sceneName)
        {
            // 多重遷移を防止（演出中の連打・重複呼び出し対策）
            if (isTransitioning) return;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        /// <summary>
        /// Resultシーンを読み込む
        /// </summary>
        public void LoadResultScene() => LoadScene(SceneName.Result);

        /// <summary>
        /// Cover（覆う）→ シーン読み込み → Uncover（晴れる）の順で遷移する。
        /// </summary>
        private IEnumerator LoadSceneRoutine(SceneName sceneName)
        {
            isTransitioning = true;

            // タイルで画面を覆う
            yield return SceneTransition.I.PlayCover();

            // 覆っている間にシーンを切り替える（ポーズ起因の timeScale=0 をここで確実に戻す）
            Time.timeScale = 1f;
            var op = SceneManager.LoadSceneAsync(sceneName.ToString());
            while (op != null && !op.isDone)
                yield return null;

            // 覆いを晴らして次シーンを見せる
            yield return SceneTransition.I.PlayUncover();

            isTransitioning = false;
        }
    }
}
