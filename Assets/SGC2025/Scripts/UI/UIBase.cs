using UnityEngine;
using Tyotyo.Audio;
using Tyotyo.Manager;

namespace Tyotyo.UI
{
    /// <summary>
    /// UI画面の基底クラス（共通のボタン操作と経過時間管理を提供）
    /// 初期フォーカスは AutoSelectFirst コンポーネントで設定する。
    /// </summary>
    public class UIBase : MonoBehaviour
    {
        protected float waitTime = 0.0f;

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
