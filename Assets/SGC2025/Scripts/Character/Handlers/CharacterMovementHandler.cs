using UnityEngine;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクターの移動処理を専門に管理するクラス
    /// 盤面外制限、移動速度制御、位置管理を担当
    /// </summary>
    public class CharacterMovement : MonoBehaviour
    {
        #region 定数
        private const float DEFAULT_MOVE_SPEED = 5f;
        private const float DEFAULT_ACCELERATION = 10f;
        private const float DEFAULT_DECELERATION = 10f;
        private const float DEFAULT_PLAY_AREA_WIDTH = 10f;
        private const float DEFAULT_PLAY_AREA_HEIGHT = 8f;
        private const float MOVEMENT_THRESHOLD = 0.01f;
        private const float VELOCITY_THRESHOLD = 0.001f;
        private const float HALF_MULTIPLIER = 0.5f;
        private const string DEBUG_LOG_PREFIX = "[CharacterMovement]";
        #endregion

        #region Inspector設定
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
        [SerializeField] private float acceleration = DEFAULT_ACCELERATION;
        [SerializeField] private float deceleration = DEFAULT_DECELERATION;
        [SerializeField] private bool useSmoothing = true;

        [Header("境界設定")]
        [SerializeField] private Vector2 playAreaSize = new Vector2(DEFAULT_PLAY_AREA_WIDTH, DEFAULT_PLAY_AREA_HEIGHT);
        [SerializeField] private Vector2 playAreaCenter = Vector2.zero;
        [SerializeField] private bool constrainToBounds = true;

        [Header("デバッグ")]
        [SerializeField] private bool showBoundaryGizmos = true;
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private Color boundaryColor = Color.red;
        #endregion

        #region プライベート変数
        private CharacterController characterController;
        private Vector3 currentVelocity;
        private Vector3 targetVelocity;
        private Vector3 initialPosition;
        #endregion

        #region プロパティ
        /// <summary>現在の移動速度</summary>
        public float CurrentSpeed => currentVelocity.magnitude;
        
        /// <summary>最大移動速度</summary>
        public float MaxSpeed => moveSpeed;
        
        /// <summary>現在の速度ベクトル</summary>
        public Vector3 CurrentVelocity => currentVelocity;
        
        /// <summary>プレイエリアの中心</summary>
        public Vector2 PlayAreaCenter => playAreaCenter;
        
        /// <summary>プレイエリアのサイズ</summary>
        public Vector2 PlayAreaSize => playAreaSize;
        
        /// <summary>移動中かどうか</summary>
        public bool IsMoving => currentVelocity.magnitude > MOVEMENT_THRESHOLD;
        #endregion

        #region ハンドラーライフサイクル
        /// <summary>
        /// CharacterControllerによる初期化
        /// </summary>
        /// <param name="controller">親のCharacterController</param>
        public void Initialize(CharacterController controller)
        {
            characterController = controller;
            initialPosition = transform.position;
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Initialized at position {initialPosition}");
            }
        }

        public void OnStart() { }

        public void OnEnable() { }

        public void OnDisable() 
        {
            // 無効化時は移動を停止
            StopMovement();
        }
        #endregion

        #region 更新処理
        private void FixedUpdate()
        {
            if (characterController?.Input == null) return;

            ProcessMovementInput();
            ApplyMovement();
        }

        /// <summary>
        /// 入力に基づいて移動処理を実行
        /// </summary>
        private void ProcessMovementInput()
        {
            Vector2 inputVector = characterController.Input.MovementInput;
            
            // 目標速度を設定
            targetVelocity = new Vector3(inputVector.x, inputVector.y, 0f) * moveSpeed;

            // スムージングの適用
            if (useSmoothing)
            {
                ApplySmoothing();
            }
            else
            {
                currentVelocity = targetVelocity;
            }
        }

        /// <summary>
        /// 加速・減速のスムージングを適用
        /// </summary>
        private void ApplySmoothing()
        {
            float deltaTime = Time.deltaTime;
            
            if (targetVelocity.magnitude > MOVEMENT_THRESHOLD)
            {
                // 加速
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * deltaTime);
            }
            else
            {
                // 減速
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * deltaTime);
            }
        }

        /// <summary>
        /// 実際の移動を適用
        /// </summary>
        private void ApplyMovement()
        {
            if (currentVelocity.magnitude < VELOCITY_THRESHOLD) return;

            Vector3 newPosition = transform.position + currentVelocity * Time.deltaTime;
            
            // 境界制限を適用
            if (constrainToBounds)
            {
                newPosition = ClampToPlayArea(newPosition);
            }

            transform.position = newPosition;
        }
        #endregion

        #region 境界管理
        /// <summary>
        /// 位置をプレイエリア内に制限
        /// </summary>
        /// <param name="position">制限したい位置</param>
        /// <returns>制限された位置</returns>
        private Vector3 ClampToPlayArea(Vector3 position)
        {
            float halfWidth = playAreaSize.x * HALF_MULTIPLIER;
            float halfHeight = playAreaSize.y * HALF_MULTIPLIER;
            
            Vector3 clampedPosition = position;
            clampedPosition.x = Mathf.Clamp(position.x, 
                playAreaCenter.x - halfWidth, 
                playAreaCenter.x + halfWidth);
            clampedPosition.y = Mathf.Clamp(position.y, 
                playAreaCenter.y - halfHeight, 
                playAreaCenter.y + halfHeight);
            
            return clampedPosition;
        }

        /// <summary>
        /// 指定位置がプレイエリア内にあるかチェック
        /// </summary>
        /// <param name="position">チェックしたい位置</param>
        /// <returns>エリア内にある場合true</returns>
        public bool IsInPlayArea(Vector3 position)
        {
            float halfWidth = playAreaSize.x * HALF_MULTIPLIER;
            float halfHeight = playAreaSize.y * HALF_MULTIPLIER;
            
            return position.x >= playAreaCenter.x - halfWidth &&
                   position.x <= playAreaCenter.x + halfWidth &&
                   position.y >= playAreaCenter.y - halfHeight &&
                   position.y <= playAreaCenter.y + halfHeight;
        }

        /// <summary>
        /// プレイエリアの境界との距離を取得
        /// </summary>
        /// <param name="position">基準位置</param>
        /// <returns>境界までの最短距離</returns>
        public float GetDistanceToBoundary(Vector3 position)
        {
            float halfWidth = playAreaSize.x * HALF_MULTIPLIER;
            float halfHeight = playAreaSize.y * HALF_MULTIPLIER;
            
            float distanceToLeft = position.x - (playAreaCenter.x - halfWidth);
            float distanceToRight = (playAreaCenter.x + halfWidth) - position.x;
            float distanceToBottom = position.y - (playAreaCenter.y - halfHeight);
            float distanceToTop = (playAreaCenter.y + halfHeight) - position.y;
            
            return Mathf.Min(distanceToLeft, distanceToRight, distanceToBottom, distanceToTop);
        }
        #endregion

        #region パブリックメソッド
        /// <summary>
        /// 移動速度を設定
        /// </summary>
        /// <param name="speed">新しい移動速度</param>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Move speed set to {moveSpeed}");
            }
        }

        /// <summary>
        /// プレイエリアのサイズを設定
        /// </summary>
        /// <param name="size">新しいプレイエリアサイズ</param>
        public void SetPlayAreaSize(Vector2 size)
        {
            playAreaSize = new Vector2(Mathf.Max(0.1f, size.x), Mathf.Max(0.1f, size.y));

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Play area size set to {playAreaSize}");
            }
        }

        /// <summary>
        /// プレイエリアの中心を設定
        /// </summary>
        /// <param name="center">新しいプレイエリア中心</param>
        public void SetPlayAreaCenter(Vector2 center)
        {
            playAreaCenter = center;

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Play area center set to {playAreaCenter}");
            }
        }

        /// <summary>
        /// キャラクターを指定位置にテレポート
        /// </summary>
        /// <param name="position">テレポート先位置</param>
        /// <param name="resetVelocity">速度をリセットするか</param>
        public void TeleportTo(Vector3 position, bool resetVelocity = true)
        {
            transform.position = constrainToBounds ? ClampToPlayArea(position) : position;
            
            if (resetVelocity)
            {
                currentVelocity = Vector3.zero;
                targetVelocity = Vector3.zero;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Teleported to {transform.position}");
            }
        }

        /// <summary>
        /// 初期位置にリセット
        /// </summary>
        public void ResetPosition()
        {
            TeleportTo(initialPosition, true);
        }

        /// <summary>
        /// 速度を強制停止
        /// </summary>
        public void StopMovement()
        {
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
        }

        /// <summary>
        /// 指定方向に一定の力を加える（ノックバックなど）
        /// </summary>
        /// <param name="force">加える力</param>
        public void AddForce(Vector3 force)
        {
            currentVelocity += force;
        }

        /// <summary>
        /// 境界制限の有効/無効を切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetBoundaryConstraint(bool enabled)
        {
            constrainToBounds = enabled;
        }
        #endregion

        #region Debug
        /// <summary>
        /// デバッグ用ギズモ描画
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (!showBoundaryGizmos) return;

            Gizmos.color = boundaryColor;
            Vector3 center = new Vector3(playAreaCenter.x, playAreaCenter.y, transform.position.z);
            Vector3 size = new Vector3(playAreaSize.x, playAreaSize.y, 0.1f);
            Gizmos.DrawWireCube(center, size);
            
            // 現在の速度ベクトルを表示
            if (Application.isPlaying && IsMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, currentVelocity.normalized);
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawDebugGizmos();
        }
        #endregion
    }
}