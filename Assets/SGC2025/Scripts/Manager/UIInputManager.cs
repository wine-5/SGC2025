using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Tyotyo.Core;

namespace Tyotyo.Manager
{
    /// <summary>
    /// UI操作のデバイスタイプ
    /// </summary>
    public enum InputDeviceType
    {
        Keyboard,  // キーボード・マウス
        Gamepad    // ゲームパッド・コントローラー
    }

    /// <summary>
    /// UI関連の入力を一元管理するマネージャー
    /// InputSystemを使用してキャンセル操作などを管理
    /// シーンごとに生成されるため、DontDestroyOnLoadは使用しない
    /// </summary>
    public class UIInputManager : Singleton<UIInputManager>
    {
        private PlayerInputSet inputActions;
        private InputDeviceType currentDeviceType = InputDeviceType.Gamepad;

        /// <summary>
        /// キャンセル操作が実行された時のイベント
        /// SettingsUI、PauseManagerなど各UIがこれを購読
        /// </summary>
        public event Action OnCancelPressed;

        /// <summary>
        /// 決定操作が実行された時のイベント
        /// タイトル画面など各UIがこれを購読
        /// </summary>
        public event Action OnSubmitPressed;

        /// <summary>
        /// 入力デバイスが切り替わった時のイベント
        /// AutoSelectFirst、Cursor表示制御などがこれを購読
        /// </summary>
        public event Action<InputDeviceType> OnDeviceSwitched;

        protected override bool UseDontDestroyOnLoad => false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Init()
        {
            base.Init();
            
            inputActions = new PlayerInputSet();
            inputActions.Enable();

            // Cancelアクションのイベント登録
            inputActions.Player.Cancel.performed += _ => OnCancelPressed?.Invoke();

            // Shotアクション（決定操作）のイベント登録
            inputActions.Player.Shot.performed += _ =>
            {
                OnSubmitPressed?.Invoke();
                // 選択されているボタンに対して Submit を送信（UI ボタン反応用）
                SendSubmitToSelectedButton();
            };
        }

        private void Update()
        {
            DetectDeviceChange();
        }

        /// <summary>
        /// 現在のアクティブデバイスを判定し、デバイスが切り替わっていればイベントを発火
        /// </summary>
        private void DetectDeviceChange()
        {
            InputDeviceType newDeviceType = GetCurrentDeviceType();

            if (newDeviceType != currentDeviceType)
            {
                currentDeviceType = newDeviceType;
                OnDeviceSwitched?.Invoke(currentDeviceType);
            }
        }

        /// <summary>
        /// 選択されているボタンに対して Submit イベントを送信
        /// </summary>
        private void SendSubmitToSelectedButton()
        {
            if (EventSystem.current == null) return;

            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            if (selectedObject == null) return;

            // 選択されているオブジェクトに Submit イベントを送信
            ExecuteEvents.Execute(selectedObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        /// <summary>
        /// 現在のアクティブデバイスタイプを判定
        /// ゲームパッドが接続されていれば Gamepad、そうでなければ Keyboard
        /// </summary>
        private InputDeviceType GetCurrentDeviceType()
        {
            // ゲームパッドが接続されていれば Gamepad（入力の有無に関わらず）
            if (Gamepad.current != null && Gamepad.current.enabled)
                return InputDeviceType.Gamepad;

            // 接続されていなければ Keyboard（マウス・キーボード）
            return InputDeviceType.Keyboard;
        }

        protected override void OnDestroy()
        {
            if (inputActions != null)
                inputActions.Dispose();

            base.OnDestroy();
        }

    }
}
