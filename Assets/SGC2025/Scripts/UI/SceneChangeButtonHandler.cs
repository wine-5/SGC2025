using UnityEngine;
using MoreHit.Audio;

namespace SGC2025
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField]
        private SceneName targetScene = SceneName.Title;

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// 指定されたシーンに切り替える
        /// </summary>
        public void ChangeScene()
        {
            // ボタンクリックSE再生
            if (AudioManager.I != null)
            {
                AudioManager.I.PlaySE(SeType.Button);
            }
            
            if (SceneController.I != null)
            {
                // targetSceneに応じて適切なメソッドを呼び出す
                switch (targetScene)
                {
                    case SceneName.Title:
                        SceneController.I.ChangeToTitleScene();
                        break;
                    case SceneName.InGame:
                        SceneController.I.ChangeToInGameScene();
                        break;
                    case SceneName.Clear:
                        SceneController.I.ChangeToGameClearScene();
                        break;
                    case SceneName.GameOver:
                        SceneController.I.ChangeToGameOverScene();
                        break;
                    default:
                        SceneController.I.LoadScene(targetScene);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("SceneController instance not found!");
            }
        }
    }
}