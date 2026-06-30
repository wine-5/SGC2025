using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tyotyo.Audio;

namespace Tyotyo.UI
{
    /// <summary>
    /// セレクト画面で、ボタンを押すと対応するPanelへ右側表示を切り替え、
    /// 右上のタイトルテキストも対応する文字に差し替える（切替時にフェード＋スケール演出）
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

        [Header("切替アニメーション")]
        [SerializeField]
        private float fadeDuration = UIPanelAnimator.DefaultFadeDuration; // フェードイン時間（秒）
        [SerializeField]
        private float startScale = UIPanelAnimator.DefaultStartScale;     // 出現開始時のスケール倍率

        private GameObject currentPanel;
        private Coroutine playingRoutine;

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
            if (selected.panel == currentPanel) return; // 同じPanelなら何もしない

            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            ShowPanel(selected.panel, animate: true);
            SetHeader(selected.title);
        }

        /// <summary>
        /// 初期表示Panelとそのタイトルを表示する（演出なし）
        /// </summary>
        private void ShowDefault()
        {
            ShowPanel(defaultPanel, animate: false);

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
        private void ShowPanel(GameObject target, bool animate)
        {
            foreach (ButtonPanel pair in buttonPanels)
            {
                if (pair.panel != null && pair.panel != target)
                    pair.panel.SetActive(false);
            }

            currentPanel = target;
            if (target == null) return;

            target.SetActive(true);

            if (playingRoutine != null) StopCoroutine(playingRoutine);

            if (animate && isActiveAndEnabled)
                playingRoutine = StartCoroutine(PlayShowAnimation(target));
            else
                UIPanelAnimator.Reset(target);
        }

        /// <summary>
        /// フェードイン＋スケールのポップ演出を再生する
        /// </summary>
        private IEnumerator PlayShowAnimation(GameObject target)
        {
            yield return UIPanelAnimator.PlayShow(target, fadeDuration, startScale);
            playingRoutine = null;
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
