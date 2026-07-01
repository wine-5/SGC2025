using UnityEngine;
using UnityEngine.UI;
using Tyotyo.Manager;
using Tyotyo.Audio;
using Tyotyo.Core;
using Tyotyo.Core.Event;

namespace Tyotyo.UI
{
    /// <summary>
    /// ポーズ画面の UI を管理（ボタンイベント処理）
    /// Manager の分離：PauseManager は状態管理、PauseUI は UI 操作を担当
    /// </summary>
    public class PauseUI : UIBase
    {
        [Header("ポーズパネル")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject firstPauseButton;

        [Header("ポーズボタン")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        protected override void Start()
        {
            base.Start(); // UIBase のリスナー登録処理を実行

            resumeButton?.onClick.AddListener(OnResumePressed);
            quitButton?.onClick.AddListener(OnQuitPressed);

            // PausedEvent に購読して、UI パネルを表示
            EventBus.Subscribe<PausedEvent>(OnPaused);
            EventBus.Subscribe<ResumedEvent>(OnResumed);

            // 初期状態: ポーズパネルは非表示
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy(); // UIBase のリスナー登録解除処理を実行

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumePressed);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitPressed);

            EventBus.Unsubscribe<PausedEvent>(OnPaused);
            EventBus.Unsubscribe<ResumedEvent>(OnResumed);
        }

        /// <summary>ゲームがポーズされたときの UI 表示</summary>
        private void OnPaused(PausedEvent @event)
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);

            UIFocusHelper.SetFocus(firstPauseButton);
        }

        /// <summary>ゲームが再開されたときの UI 表示解除</summary>
        private void OnResumed(ResumedEvent @event)
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        /// <summary>再開ボタンが押された</summary>
        private void OnResumePressed()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.ButtonClick);

            if (PauseManager.I != null)
                PauseManager.I.ResumeGame();
        }

        /// <summary>タイトルに戻るボタンが押された</summary>
        private void OnQuitPressed()
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
    }
}
