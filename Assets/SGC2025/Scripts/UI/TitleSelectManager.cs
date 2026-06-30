using UnityEngine;
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

        private void Start()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnSubmitPressed += OnSubmitPressed;

            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.Title);
        }

        private void OnDestroy()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnSubmitPressed -= OnSubmitPressed;
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
