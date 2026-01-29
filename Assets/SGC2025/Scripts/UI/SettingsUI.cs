using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using SGC2025.Audio;
using SGC2025.Manager;

namespace SGC2025.UI
{
    /// <summary>
    /// 設定画面のUI制御
    /// 音量スライダーとAudioManagerの連携を管理
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        private const float DEFAULT_MASTER_VOLUME = 1f;
        private const float DEFAULT_BGM_VOLUME = 0.7f;
        private const float DEFAULT_SE_VOLUME = 1f;

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;

        [Header("Volume Labels (Optional)")]
        [SerializeField] private TextMeshProUGUI masterVolumeLabel;
        [SerializeField] private TextMeshProUGUI bgmVolumeLabel;
        [SerializeField] private TextMeshProUGUI seVolumeLabel;

        [Header("音量設定のパネル")]
        [SerializeField] private GameObject audioPanel;
        
        [Header("Controller Navigation")]
        [SerializeField] private GameObject firstSelected; // パネルを開いた時に最初に選択されるUI要素
        
        private GameObject lastSelectedBeforeOpen; // パネルを開く前に選択されていた要素

        private void Start()
        {
            InitializeSliders();
            SetupSliderEvents();
            
            // 初期状態でパネルを非表示
            if (audioPanel != null)
                audioPanel.SetActive(false);
        }

        private void OnEnable()
        {
            // UIInputManagerのCancelイベントを購読
            if (UIInputManager.I != null)
            {
                UIInputManager.I.OnCancelPressed += OnCancelPressed;
            }
        }

        private void OnDisable()
        {
            // イベント購読を解除
            if (UIInputManager.I != null)
            {
                UIInputManager.I.OnCancelPressed -= OnCancelPressed;
            }
        }

        /// <summary>
        /// Cancelボタンが押された時の処理（UIInputManagerから通知）
        /// </summary>
        private void OnCancelPressed()
        {
            if (audioPanel != null && audioPanel.activeSelf)
                CloseSettings();
        }

        /// <summary>
        /// スライダーの初期値をAudioManagerから取得
        /// </summary>
        private void InitializeSliders()
        {
            if (AudioManager.I != null)
            {
                if (masterVolumeSlider != null)
                {
                    masterVolumeSlider.value = AudioManager.I.masterVolume;
                    UpdateVolumeLabel(masterVolumeLabel, AudioManager.I.masterVolume);
                }

                if (bgmVolumeSlider != null)
                {
                    bgmVolumeSlider.value = AudioManager.I.bgmVolume;
                    UpdateVolumeLabel(bgmVolumeLabel, AudioManager.I.bgmVolume);
                }

                if (seVolumeSlider != null)
                {
                    seVolumeSlider.value = AudioManager.I.seVolume;
                    UpdateVolumeLabel(seVolumeLabel, AudioManager.I.seVolume);
                }
            }
        }

        /// <summary>
        /// スライダーのイベントを設定
        /// </summary>
        private void SetupSliderEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (seVolumeSlider != null)
                seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }

        /// <summary>
        /// マスター音量変更時の処理
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetMasterVolume(value);
                UpdateVolumeLabel(masterVolumeLabel, value);
            }
        }

        /// <summary>
        /// BGM音量変更時の処理
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetBGMVolume(value);
                UpdateVolumeLabel(bgmVolumeLabel, value);
            }
        }

        /// <summary>
        /// SE音量変更時の処理
        /// </summary>
        private void OnSEVolumeChanged(float value)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetSEVolume(value);
                UpdateVolumeLabel(seVolumeLabel, value);
            }
        }

        /// <summary>
        /// 音量ラベルの表示を更新
        /// </summary>
        private void UpdateVolumeLabel(TextMeshProUGUI label, float volume)
        {
            if (label != null)
                label.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }

        /// <summary>
        /// 設定パネルを開く（ボタンから呼び出し）
        /// </summary>
        public void OpenSettings()
        {
            if (audioPanel != null)
            {
                // ボタンクリック音を再生
                if (AudioManager.I != null)
                    AudioManager.I.PlaySE(SEType.ButtonClick);
                
                // 現在選択されている要素を保存
                lastSelectedBeforeOpen = UIFocusHelper.GetCurrentFocus();
                
                audioPanel.SetActive(true);
                
                // コントローラー用：最初の要素にフォーカスを設定
                UIFocusHelper.SetFocus(firstSelected);
            }
        }

        /// <summary>
        /// 設定パネルを閉じる（ボタンから呼び出し）
        /// </summary>
        public void CloseSettings()
        {
            if (audioPanel != null)
            {
                // ボタンクリック音を再生
                if (AudioManager.I != null)
                    AudioManager.I.PlaySE(SEType.ButtonClick);
                
                audioPanel.SetActive(false);
                
                // コントローラー用：元のボタンに選択を戻す
                UIFocusHelper.RestoreFocus(lastSelectedBeforeOpen);
            }
        }

        /// <summary>
        /// 設定パネルの表示/非表示を切り替え（ボタンから呼び出し）
        /// </summary>
        public void ToggleSettings()
        {
            if (audioPanel != null)
                audioPanel.SetActive(!audioPanel.activeSelf);
        }

        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefault()
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.SetMasterVolume(DEFAULT_MASTER_VOLUME);
                AudioManager.I.SetBGMVolume(DEFAULT_BGM_VOLUME);
                AudioManager.I.SetSEVolume(DEFAULT_SE_VOLUME);
                
                if (masterVolumeSlider != null) masterVolumeSlider.value = DEFAULT_MASTER_VOLUME;
                if (bgmVolumeSlider != null) bgmVolumeSlider.value = DEFAULT_BGM_VOLUME;
                if (seVolumeSlider != null) seVolumeSlider.value = DEFAULT_SE_VOLUME;
            }
        }

        private void OnDestroy()
        {
            // イベントの解除
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

            if (seVolumeSlider != null)
                seVolumeSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        }
    }
}
