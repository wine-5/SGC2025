using System;
using Tyotyo.Core;

namespace Tyotyo.Manager
{
    /// <summary>
    /// UI関連の入力を一元管理するマネージャー
    /// InputSystemを使用してキャンセル操作などを管理
    /// シーンごとに生成されるため、DontDestroyOnLoadは使用しない
    /// </summary>
    public class UIInputManager : Singleton<UIInputManager>
    {
        private PlayerInputSet inputActions;

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
            inputActions.Player.Shot.performed += _ => OnSubmitPressed?.Invoke();
        }

        protected override void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Dispose();
            }

            base.OnDestroy();
        }

    }
}
