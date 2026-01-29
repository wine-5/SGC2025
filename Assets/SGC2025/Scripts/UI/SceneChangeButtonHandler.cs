using UnityEngine;
using SGC2025.Manager;
using SGC2025.Audio;

namespace SGC2025.UI
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField] private SceneName targetScene = SceneName.Title;

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