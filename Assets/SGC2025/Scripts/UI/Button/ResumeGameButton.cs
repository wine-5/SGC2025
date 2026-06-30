using UnityEngine;
using UnityEngine.UI;
using Tyotyo.Manager;
using Tyotyo.Audio;

namespace Tyotyo.UI
{
    /// <summary>
    /// ゲームを再開するボタン
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ResumeGameButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (InGameManager.I != null)
            {
                if (AudioManager.I != null)
                    AudioManager.I.PlaySE(SEType.ButtonClick);

                InGameManager.I.Resume();
            }
        }
    }
}
