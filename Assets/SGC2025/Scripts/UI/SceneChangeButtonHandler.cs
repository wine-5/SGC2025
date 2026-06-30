using UnityEngine;
using Tyotyo.Manager;
using Tyotyo.Audio;

namespace Tyotyo.UI
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField] private SceneName targetScene = SceneName.TitleSelect;

        public void ChangeScene()
        {
            if (SceneController.I == null) return;
            
            // ボタンクリック音を再生
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);
            
            SceneController.I.LoadScene(targetScene);
        }
    }
}