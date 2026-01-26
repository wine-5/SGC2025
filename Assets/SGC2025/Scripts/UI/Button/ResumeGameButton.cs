using UnityEngine;
using UnityEngine.UI;
using SGC2025.Manager;

namespace SGC2025.UI
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
            if (PauseManager.I != null)
                PauseManager.I.ResumeGame();
        }
    }
}
