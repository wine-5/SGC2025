using System;

namespace SGC2025.Manager
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
        }

        protected override void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Dispose();
            }

            base.OnDestroy();
        }

        /// <summary>
        /// 入力を一時的に無効化
        /// </summary>
        public void DisableInput()
        {
            inputActions?.Disable();
        }

        /// <summary>
        /// 入力を有効化
        /// </summary>
        public void EnableInput()
        {
            inputActions?.Enable();
        }
    }
}
