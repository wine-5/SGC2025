using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using SGC2025.Audio;

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
            
            SceneManager.LoadScene("InGame");
        }

        public void OnClickBackTitle()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);
            
            SceneManager.LoadScene("Title");
        }
        
        public void OnClickExit()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);
            
            Application.Quit();
        }

        protected int ScoreCountUp(float currentWaitTime, float scoreMaxValue, float waitMaxTime)
        {
            float a = Mathf.Clamp01(currentWaitTime / waitMaxTime);
            return (int)Mathf.Lerp(0 , scoreMaxValue, a);
        }
    }
}
