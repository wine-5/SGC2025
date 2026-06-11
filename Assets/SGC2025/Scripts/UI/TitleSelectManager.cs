using UnityEngine;
using SGC2025.Manager;

namespace SGC2025.UI
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
            {
                UIInputManager.I.OnSubmitPressed += OnSubmitPressed;
            }
        }

        private void OnDestroy()
        {
            if (UIInputManager.I != null)
            {
                UIInputManager.I.OnSubmitPressed -= OnSubmitPressed;
            }
        }

        private void OnSubmitPressed()
        {
            if (!titleScreen.activeSelf) return;
            titleScreen.SetActive(false);
            selectScreen.SetActive(true);
        }
    }
}
