using UnityEngine;
using UnityEngine.UI;
using SGC2025.Audio;

namespace SGC2025.UI
{
    /// <summary>
    /// ルール説明を、ボタン押下でコントローラー版／キーボード＆マウス版のGameObjectへ切り替える
    /// </summary>
    public class RuleDescriptionSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Button controllerButton;      // コントローラー説明へ切替えるボタン
        [SerializeField]
        private Button keyboardMouseButton;   // キーボード＆マウス説明へ切替えるボタン

        [SerializeField]
        private GameObject controllerPanel;    // コントローラー版の説明（GameObjectごと表示切替）
        [SerializeField]
        private GameObject keyboardMousePanel; // キーボード＆マウス版の説明

        private void Awake()
        {
            if (controllerButton != null)
                controllerButton.onClick.AddListener(ShowController);

            if (keyboardMouseButton != null)
                keyboardMouseButton.onClick.AddListener(ShowKeyboardMouse);
        }

        private void OnEnable()
        {
            // 表示する度に既定（コントローラー版）から開始する
            ShowPanel(controllerPanel, playSE: false);
        }

        /// <summary>コントローラー版の説明へ切り替える（切替ボタンから呼ぶ）</summary>
        public void ShowController() => ShowPanel(controllerPanel, playSE: true);

        /// <summary>キーボード＆マウス版の説明へ切り替える（切替ボタンから呼ぶ）</summary>
        public void ShowKeyboardMouse() => ShowPanel(keyboardMousePanel, playSE: true);

        /// <summary>
        /// 指定した方の説明GameObjectのみを表示し、もう一方を非表示にする
        /// </summary>
        private void ShowPanel(GameObject target, bool playSE)
        {
            if (playSE && AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            if (controllerPanel != null)
                controllerPanel.SetActive(target == controllerPanel);

            if (keyboardMousePanel != null)
                keyboardMousePanel.SetActive(target == keyboardMousePanel);
        }
    }
}
