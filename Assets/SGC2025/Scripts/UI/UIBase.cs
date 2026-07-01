using UnityEngine;
using UnityEngine.UI;
using Tyotyo.Audio;
using Tyotyo.Manager;
using Tyotyo.Core;

namespace Tyotyo.UI
{
    /// <summary>
    /// UI画面の基底クラス（共通のボタン操作と経過時間管理を提供）
    /// 初期フォーカスは AutoSelectFirst コンポーネントで設定する。
    /// </summary>
    public class UIBase : MonoBehaviour
    {
        [SerializeField] protected Button backToTitleButton;
        [SerializeField] protected Button restartButton;

        protected float waitTime = 0.0f;

        protected virtual void Start()
        {
            if (backToTitleButton != null)
                backToTitleButton.onClick.AddListener(OnClickBackTitle);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnClickRestart);
        }

        protected virtual void OnDestroy()
        {
            if (backToTitleButton != null)
                backToTitleButton.onClick.RemoveListener(OnClickBackTitle);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnClickRestart);
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

            // GameMode に応じて遷移先を決定
            SceneName titleScene = GetTitleSelectScene();
            SceneController.I.LoadScene(titleScene);
        }

        /// <summary>GameMode に応じて適切なタイトル選択シーンを返す</summary>
        private SceneName GetTitleSelectScene()
        {
            if (GameModeConfig.Current == null)
                return SceneName.TitleSelect;

            return GameModeConfig.Current.Mode == GameModeConfig.GameMode.Steam
                ? SceneName.TitleSelect_Steam
                : SceneName.TitleSelect_Exhibition;
        }
        
        public void OnClickExit()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);
            
            Application.Quit();
        }
    }
}
