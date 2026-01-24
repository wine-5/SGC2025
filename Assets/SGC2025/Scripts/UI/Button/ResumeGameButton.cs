using UnityEngine;
using UnityEngine.UI;

namespace SGC2025
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
            if (GameManager.I != null)
                GameManager.I.ResumeGame();
        }
    }
}
