using UnityEngine;
using UnityEngine.UI;
using Tyotyo.Manager;
using Tyotyo.Audio;

namespace Tyotyo.UI
{
    /// <summary>
    /// タイトル画面とセレクト画面の遷移を管理
    /// </summary>
    public class TitleSelectManager : MonoBehaviour
    {
        [SerializeField] private GameObject titleScreen;
        [SerializeField] private GameObject selectScreen;
        [SerializeField] private Button quitButton; // ゲーム終了ボタン

        private void Start()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnSubmitPressed += OnSubmitPressed;

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonPressed);

            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.Title);
        }

        private void OnDestroy()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnSubmitPressed -= OnSubmitPressed;

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitButtonPressed);
        }

        /// <summary>
        /// 終了ボタンが押されたときの処理。ボタン音を鳴らしてゲームを終了する。
        /// </summary>
        private void OnQuitButtonPressed()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            if (GameManager.I != null)
                GameManager.I.QuitGame();
        }

        private void OnSubmitPressed()
        {
            if (!titleScreen.activeSelf) return;

            // タイトルから進むときにボタン音を鳴らす（クリック・コントローラー共通）
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            titleScreen.SetActive(false);
            selectScreen.SetActive(true);
        }
    }
}
