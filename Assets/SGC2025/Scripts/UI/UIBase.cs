using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace SGC2025
{
    public class UIBase : MonoBehaviour
    {
        protected float waitTime = 0.0f;

        virtual public void Start()
        {

        }
        
        virtual public void Update()
        {
            waitTime += Time.unscaledDeltaTime;
        }
        public void OnClickRestart()
        {
            SceneManager.LoadScene("Map_1");
        }

        public void OnClickBackTitle()
        {
            SceneManager.LoadScene("Title");
        }
        
        public void OnClickControllGuide()
        {
            //操作説明用ガイドプレハブ作成
        }

        public void OnClickExit()
        {
            Application.Quit();
        }

        protected int ScoreCountUp(float currentWaitTime, float scoreMaxValue, float waitMaxTime)
        {
            float t = Mathf.Clamp01(currentWaitTime / waitMaxTime);
            return (int)Mathf.Lerp(0f, scoreMaxValue, t);
        }
    }
}
