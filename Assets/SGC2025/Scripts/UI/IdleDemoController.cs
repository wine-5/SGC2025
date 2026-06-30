using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Video;

namespace Tyotyo.UI
{
    /// <summary>
    /// タイトル画面で一定時間操作がないとデモ（アイドル時の自動デモ）を開始する。
    /// 背景動画を再生し、中央の「デモプレイ中」テキストを点滅表示する。
    /// 何か操作されると解除してタイトルへ戻す。東京ゲームショー等の無人展示用。
    /// BGMはタイトルのまま流し続けるため、動画はミュート再生する。
    /// </summary>
    public class IdleDemoController : MonoBehaviour
    {
        [Header("アイドル判定")]
        [SerializeField, Tooltip("操作がないままこの秒数が経過したらデモを開始")]
        private float idleTimeout = 30f;

        [SerializeField, Tooltip("マウス移動を入力とみなす閾値（小さいほど敏感。展示でのカーソル振動による誤解除を防ぐ）")]
        private float mouseMoveThreshold = 2f;

        [SerializeField, Tooltip("スティックを入力とみなす閾値（ドリフトによる誤解除を防ぐ）")]
        private float stickThreshold = 0.3f;

        [Header("デモ表示のルート")]
        [SerializeField, Tooltip("デモ中に表示するルート（背景動画＋デモテキストを含む）。OnEnableでTextBlinkingEffectの点滅が自動開始する")]
        private GameObject demoRoot;

        [Header("背景動画")]
        [SerializeField, Tooltip("背景で再生する動画。BGMはタイトルのままにするためミュート・ループ再生する")]
        private VideoPlayer videoPlayer;

        private float idleTimer;
        private bool isDemoActive;

        private void Start()
        {
            if (demoRoot != null)
                demoRoot.SetActive(false);
        }

        private void Update()
        {
            bool inputDetected = DetectAnyInput();

            if (isDemoActive)
            {
                if (inputDetected)
                    StopDemo();
                return;
            }

            if (inputDetected)
            {
                idleTimer = 0f;
                return;
            }

            // ポーズ等でTime.timeScaleが変わっても影響しないよう unscaled を使用
            idleTimer += Time.unscaledDeltaTime;
            if (idleTimer >= idleTimeout)
                StartDemo();
        }

        /// <summary>デモを開始（背景動画＋デモテキスト表示）</summary>
        private void StartDemo()
        {
            isDemoActive = true;
            idleTimer = 0f;

            // ルートを有効化するとTextBlinkingEffectのアルファ点滅がOnEnableで自動開始する
            if (demoRoot != null)
                demoRoot.SetActive(true);

            if (videoPlayer != null)
            {
                videoPlayer.isLooping = true;
                // タイトルBGMを残すため動画はミュート
                SetVideoMuted(videoPlayer);
                videoPlayer.Play();
            }
        }

        /// <summary>デモを解除してタイトルへ戻す</summary>
        private void StopDemo()
        {
            isDemoActive = false;
            idleTimer = 0f;

            if (videoPlayer != null)
                videoPlayer.Stop();

            if (demoRoot != null)
                demoRoot.SetActive(false);
        }

        /// <summary>このフレームに何らかの操作（キー・マウス・ゲームパッド）があったかを判定</summary>
        private bool DetectAnyInput()
        {
            // キーボード：いずれかのキー
            if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
                return true;

            // マウス：ボタン or 一定以上の移動
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed
                    || Mouse.current.rightButton.isPressed
                    || Mouse.current.middleButton.isPressed
                    || Mouse.current.delta.ReadValue().sqrMagnitude > mouseMoveThreshold * mouseMoveThreshold)
                    return true;
            }

            // ゲームパッド：いずれかのボタン or スティック傾倒（ドリフト誤検知は閾値で抑制）
            if (Gamepad.current != null)
            {
                foreach (var control in Gamepad.current.allControls)
                {
                    if (control is ButtonControl button && button.isPressed)
                        return true;
                }

                float threshold = stickThreshold * stickThreshold;
                if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > threshold
                    || Gamepad.current.rightStick.ReadValue().sqrMagnitude > threshold)
                    return true;
            }

            return false;
        }

        /// <summary>動画の全オーディオトラックをミュートする</summary>
        private static void SetVideoMuted(VideoPlayer player)
        {
            for (ushort i = 0; i < player.audioTrackCount; i++)
                player.SetDirectAudioMute(i, true);
        }
    }
}
