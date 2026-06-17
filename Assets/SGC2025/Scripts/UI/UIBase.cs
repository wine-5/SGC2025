using UnityEngine;
using SGC2025.Audio;
using SGC2025.Manager;

namespace SGC2025.UI
{
    /// <summary>
    /// UI画面の基底クラス
    /// </summary>
    public class UIBase : MonoBehaviour
    {
        [SerializeField]
        protected GameObject firstSelect;
        protected float waitTime = 0.0f;

        virtual public void Start()
        {
            UIFocusHelper.SetFocus(firstSelect);
        }
        
        virtual public void Update()
        {
            waitTime += Time.unscaledDeltaTime;
        }
        public void OnClickRestart()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            SceneController.I.LoadScene(SceneName.InGame);
        }

        public void OnClickBackTitle()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            SceneController.I.LoadScene(SceneName.TitleSelect);
        }
        
        public void OnClickExit()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);
            
            Application.Quit();
        }
    }
}
