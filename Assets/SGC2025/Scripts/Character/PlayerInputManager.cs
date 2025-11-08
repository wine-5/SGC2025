using UnityEngine;
using UnityEngine.InputSystem;

namespace SGC2025.Character
{
    /// <summary>
    /// プレイヤーの入力処理を専門に管理するクラス
    /// WASD、矢印キー、コントローラー入力に対応
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        [Header("入力設定")]
        [SerializeField] private bool inputEnabled = true;
        [SerializeField] private float inputDeadzone = 0.1f;

        private CharacterController characterController;
        private PlayerInput playerInput;
        
        // 入力状態の保持
        private Vector2 rawMovementInput;
        private Vector2 processedMovementInput;
        private bool isShotPressed;
        private bool isShotHeld;

        #region プロパティ
        /// <summary>処理済み移動入力（デッドゾーン適用済み）</summary>
        public Vector2 MovementInput => inputEnabled ? processedMovementInput : Vector2.zero;
        
        /// <summary>射撃ボタンが押された瞬間</summary>
        public bool IsShotPressed => inputEnabled && isShotPressed;
        
        /// <summary>射撃ボタンが押し続けられている</summary>
        public bool IsShotHeld => inputEnabled && isShotHeld;
        
        /// <summary>入力が有効かどうか</summary>
        public bool IsInputEnabled => inputEnabled;
        
        /// <summary>移動入力があるかどうか</summary>
        public bool HasMovementInput => MovementInput.magnitude > 0f;
        #endregion

        #region 初期化
        /// <summary>
        /// CharacterControllerによる初期化
        /// </summary>
        /// <param name="controller">親のCharacterController</param>
        public void Initialize(CharacterController controller)
        {
            characterController = controller;
            playerInput = GetComponent<PlayerInput>();
            
            if (playerInput == null)
            {
                Debug.LogError($"[PlayerInputManager] PlayerInput component not found on {gameObject.name}");
                return;
            }

            SetupInputActions();
        }

        public void OnStart() { }

        public void OnEnable()
        {
            EnableInputActions();
        }

        public void OnDisable()
        {
            DisableInputActions();
        }
        #endregion

        #region 入力セットアップ
        /// <summary>
        /// Input Actionsのイベントバインディング設定
        /// </summary>
        private void SetupInputActions()
        {
            if (playerInput?.actions == null) return;

            // 移動入力のバインディング
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed += OnMoveInput;
                moveAction.canceled += OnMoveInput;
            }

            // 射撃入力のバインディング
            var shotAction = playerInput.actions["Shot"];
            if (shotAction != null)
            {
                shotAction.performed += OnShotInput;
                shotAction.canceled += OnShotCanceled;
            }
        }

        /// <summary>
        /// 入力アクションを有効化
        /// </summary>
        private void EnableInputActions()
        {
            playerInput?.ActivateInput();
        }

        /// <summary>
        /// 入力アクションを無効化
        /// </summary>
        private void DisableInputActions()
        {
            playerInput?.DeactivateInput();
        }
        #endregion

        #region 入力コールバック
        /// <summary>
        /// 移動入力コールバック
        /// </summary>
        /// <param name="context">入力コンテキスト</param>
        private void OnMoveInput(InputAction.CallbackContext context)
        {
            rawMovementInput = context.ReadValue<Vector2>();
            processedMovementInput = ApplyDeadzone(rawMovementInput);
        }

        /// <summary>
        /// 射撃入力コールバック（押下時）
        /// </summary>
        /// <param name="context">入力コンテキスト</param>
        private void OnShotInput(InputAction.CallbackContext context)
        {
            if (!inputEnabled) return;
            
            isShotPressed = true;
            isShotHeld = true;
        }

        /// <summary>
        /// 射撃入力コールバック（離上時）
        /// </summary>
        /// <param name="context">入力コンテキスト</param>
        private void OnShotCanceled(InputAction.CallbackContext context)
        {
            isShotHeld = false;
        }
        #endregion

        #region 入力処理
        /// <summary>
        /// デッドゾーンを適用した入力処理
        /// </summary>
        /// <param name="input">生の入力値</param>
        /// <returns>処理済み入力値</returns>
        private Vector2 ApplyDeadzone(Vector2 input)
        {
            if (input.magnitude < inputDeadzone)
            {
                return Vector2.zero;
            }

            // アナログスティックの場合は正規化
            if (input.magnitude > 1f)
            {
                return input.normalized;
            }

            return input;
        }

        /// <summary>
        /// フレーム終了時の入力状態リセット
        /// </summary>
        private void LateUpdate()
        {
            // 押下フラグは1フレームだけ有効
            isShotPressed = false;
        }
        #endregion

        #region 公開メソッド
        /// <summary>
        /// 入力の有効/無効を切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            
            if (!enabled)
            {
                // 入力を無効化する際は全ての入力状態をリセット
                rawMovementInput = Vector2.zero;
                processedMovementInput = Vector2.zero;
                isShotPressed = false;
                isShotHeld = false;
            }
        }
        #endregion
    }
}