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
        #region 定数
        private const float DEFAULT_INPUT_DEADZONE = 0.1f;
        private const float MAX_INPUT_MAGNITUDE = 1f;
        private const string DEBUG_LOG_PREFIX = "[PlayerInputManager]";
        #endregion

        #region Inspector設定
        [Header("入力設定")]
        [SerializeField] private bool inputEnabled = true;
        [SerializeField] private float inputDeadzone = DEFAULT_INPUT_DEADZONE;

        [Header("デバッグ")]
        [SerializeField] private bool enableDebugLogs = false;
        #endregion

        #region プライベート変数
        private CharacterController characterController;
        private PlayerInput playerInput;
        
        // 入力状態の保持
        private Vector2 rawMovementInput;
        private Vector2 processedMovementInput;
        private bool isShotPressed;
        #endregion

        #region プロパティ
        /// <summary>処理済み移動入力（デッドゾーン適用済み）</summary>
        public Vector2 MovementInput => inputEnabled ? processedMovementInput : Vector2.zero;
        
        /// <summary>射撃ボタンが押された瞬間</summary>
        public bool IsShotPressed => inputEnabled && isShotPressed;

        /// <summary>入力が有効かどうか</summary>
        public bool IsInputEnabled => inputEnabled;
        #endregion

        #region ハンドラーライフサイクル
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
                Debug.LogError($"{DEBUG_LOG_PREFIX} PlayerInput component not found on {gameObject.name}");
                return;
            }

            SetupInputActions();

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Initialized for {gameObject.name}");
            }
        }

        public void OnEnable()
        {
            EnableInputActions();
        }

        public void OnDisable()
        {
            DisableInputActions();
        }

        private void OnDestroy()
        {
            // イベントのクリーンアップ
            CleanupInputActions();
        }
        #endregion

        #region 入力アクション設定
        /// <summary>
        /// Input Actionsのイベントバインディング設定
        /// </summary>
        private void SetupInputActions()
        {
            if (playerInput?.actions == null)
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} PlayerInput actions are not set up");
                return;
            }

            // 移動入力のバインディング
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed += OnMoveInput;
                moveAction.canceled += OnMoveInput;
            }
            else
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} 'Move' action not found in input actions");
            }

            // 射撃入力のバインディング
            var shotAction = playerInput.actions["Shot"];
            if (shotAction != null)
            {
                shotAction.performed += OnShotInput;
            }
            else
            {
                Debug.LogWarning($"{DEBUG_LOG_PREFIX} 'Shot' action not found in input actions");
            }
        }

        /// <summary>
        /// Input Actionsのイベントバインディング解除
        /// </summary>
        private void CleanupInputActions()
        {
            if (playerInput?.actions == null) return;

            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed -= OnMoveInput;
                moveAction.canceled -= OnMoveInput;
            }

            var shotAction = playerInput.actions["Shot"];
            if (shotAction != null)
            {
                shotAction.performed -= OnShotInput;
            }
        }

        /// <summary>
        /// 入力アクションを有効化
        /// </summary>
        private void EnableInputActions()
        {
            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
        }

        /// <summary>
        /// 入力アクションを無効化
        /// </summary>
        private void DisableInputActions()
        {
            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }

            // 入力状態をリセット
            ResetInputState();
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

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Move input: {processedMovementInput}");
            }
        }

        /// <summary>
        /// 射撃入力コールバック（押下時）
        /// </summary>
        /// <param name="context">入力コンテキスト</param>
        private void OnShotInput(InputAction.CallbackContext context)
        {
            if (!inputEnabled) return;
            
            isShotPressed = true;

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Shot pressed");
            }
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
            if (input.magnitude > MAX_INPUT_MAGNITUDE)
            {
                return input.normalized;
            }

            return input;
        }

        /// <summary>
        /// 入力状態をリセット
        /// </summary>
        private void ResetInputState()
        {
            rawMovementInput = Vector2.zero;
            processedMovementInput = Vector2.zero;
            isShotPressed = false;
        }
        #endregion

        #region フレーム更新
        /// <summary>
        /// フレーム終了時の入力状態リセット
        /// </summary>
        private void LateUpdate()
        {
            // 押下フラグは1フレームだけ有効
            isShotPressed = false;
        }
        #endregion

        #region パブリックメソッド
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
                ResetInputState();
            }

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Input {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// デッドゾーンの設定
        /// </summary>
        /// <param name="deadzone">新しいデッドゾーン値</param>
        public void SetDeadzone(float deadzone)
        {
            inputDeadzone = Mathf.Clamp01(deadzone);
        }
        #endregion
    }
}