using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField] private SceneName targetScene = SceneName.Title;

        public void ChangeScene()
        {
            if (SceneController.I == null)
            {
                Debug.LogWarning("SceneController instance not found!");
                return;
            }
            
            SceneController.I.LoadScene(targetScene);
        }
    }
}