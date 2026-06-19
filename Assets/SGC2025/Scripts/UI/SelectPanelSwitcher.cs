using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SGC2025.Audio;

namespace SGC2025.UI
{
    /// <summary>
    /// セレクト画面で、ボタンを押すと対応するPanelへ右側表示を切り替え、
    /// 右上のタイトルテキストも対応する文字に差し替える
    /// </summary>
    public class SelectPanelSwitcher : MonoBehaviour
    {
        /// <summary>ボタン・表示Panel・タイトル文字列の対応</summary>
        [Serializable]
        private struct ButtonPanel
        {
            public Button button;
            public GameObject panel;
            public string title; // 右上ヘッダーに表示する文字（インスペクターで設定）
        }

        [SerializeField]
        private List<ButtonPanel> buttonPanels = new List<ButtonPanel>();
        [SerializeField]
        private GameObject defaultPanel; // 初期表示するPanel（未指定なら全て非表示）
        [SerializeField]
        private TextMeshProUGUI headerText; // 右上のタイトルテキスト（差し替え先は1つ）

        private void Awake()
        {
            foreach (ButtonPanel pair in buttonPanels)
            {
                if (pair.button == null) continue;

                ButtonPanel captured = pair; // ローカルにコピーしてクロージャの参照ずれを防ぐ
                pair.button.onClick.AddListener(() => OnButtonClicked(captured));
            }
        }

        private void OnEnable()
        {
            ShowDefault();
        }

        /// <summary>
        /// ボタン押下時：クリック音を鳴らして対応Panelとタイトルへ切り替える
        /// </summary>
        private void OnButtonClicked(ButtonPanel selected)
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            ShowPanel(selected.panel);
            SetHeader(selected.title);
        }

        /// <summary>
        /// 初期表示Panelとそのタイトルを表示する
        /// </summary>
        private void ShowDefault()
        {
            ShowPanel(defaultPanel);

            // defaultPanel に対応するタイトルがあればそれを表示
            foreach (ButtonPanel pair in buttonPanels)
            {
                if (pair.panel == defaultPanel)
                {
                    SetHeader(pair.title);
                    return;
                }
            }
            SetHeader(string.Empty);
        }

        /// <summary>
        /// 指定したPanelのみを表示し、他は非表示にする
        /// </summary>
        private void ShowPanel(GameObject target)
        {
            foreach (ButtonPanel pair in buttonPanels)
            {
                if (pair.panel != null)
                    pair.panel.SetActive(pair.panel == target);
            }
        }

        /// <summary>
        /// 右上のタイトルテキストを差し替える
        /// </summary>
        private void SetHeader(string title)
        {
            if (headerText != null)
                headerText.SetText(title);
        }
    }
}
