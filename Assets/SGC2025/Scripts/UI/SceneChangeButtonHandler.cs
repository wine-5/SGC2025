using UnityEngine;
using SGC2025.Manager;

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
            
            SceneController.I.LoadScene(targetScene);
        }
    }
}