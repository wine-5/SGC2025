using UnityEngine;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクター（プレイヤー）の各Handlerを統合管理する軽量なコントローラー
    /// 各Handler間の連携を調整し、外部からのアクセスポイントを提供
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        [Header("ハンドラー")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private CharacterMovement movement;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private CharacterWeapon weapon;
        [SerializeField] private CharacterCollision collision;

        [Header("設定")]
        [SerializeField] private bool isDebugMode = false;

        #region プロパティ
        /// <summary>入力処理へのアクセス</summary>
        public PlayerInputManager Input => inputManager;
        
        /// <summary>移動処理へのアクセス</summary>
        public CharacterMovement Movement => movement;
        
        /// <summary>ヘルス管理へのアクセス</summary>
        public CharacterHealth Health => health;
        
        /// <summary>武器システムへのアクセス</summary>
        public CharacterWeapon Weapon => weapon;
        
        /// <summary>衝突処理へのアクセス</summary>
        public CharacterCollision Collision => collision;

        /// <summary>キャラクターが生存しているか</summary>
        public bool IsAlive => health != null && health.IsAlive;
        #endregion

        #region Unityコールバック
        private void Awake()
        {
            InitializeHandlers();
        }

        private void Start()
        {
            StartHandlers();
        }

        private void OnEnable()
        {
            EnableHandlers();
        }

        private void OnDisable()
        {
            DisableHandlers();
        }
        #endregion

        #region 初期化
        /// <summary>
        /// 各コンポーネントの初期化と相互参照の設定
        /// </summary>
        private void InitializeHandlers()
        {
            // 各コンポーネントが存在しない場合は自動取得
            if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
            if (movement == null) movement = GetComponent<CharacterMovement>();
            if (health == null) health = GetComponent<CharacterHealth>();
            if (weapon == null) weapon = GetComponent<CharacterWeapon>();
            if (collision == null) collision = GetComponent<CharacterCollision>();

            // 各コンポーネントの初期化
            inputManager?.Initialize(this);
            movement?.Initialize(this);
            health?.Initialize(this);
            weapon?.Initialize(this);
            collision?.Initialize(this);

            if (isDebugMode)
            {
                Debug.Log($"[CharacterController] All components initialized for {gameObject.name}");
            }
        }

        /// <summary>
        /// 各コンポーネントのスタート処理
        /// </summary>
        private void StartHandlers()
        {
            inputManager?.OnStart();
            movement?.OnStart();
            health?.OnStart();
            weapon?.OnStart();
            collision?.OnStart();
        }

        /// <summary>
        /// 各コンポーネントの有効化
        /// </summary>
        private void EnableHandlers()
        {
            inputManager?.OnEnable();
            movement?.OnEnable();
            health?.OnEnable();
            weapon?.OnEnable();
            collision?.OnEnable();
        }

        /// <summary>
        /// 各コンポーネントの無効化
        /// </summary>
        private void DisableHandlers()
        {
            inputManager?.OnDisable();
            movement?.OnDisable();
            health?.OnDisable();
            weapon?.OnDisable();
            collision?.OnDisable();
        }
        #endregion

        #region 公開メソッド
        /// <summary>
        /// キャラクターの完全なリセット（ゲーム開始時など）
        /// </summary>
        public void ResetCharacter()
        {
            health?.ResetHealth();
            weapon?.ResetWeapon();
            movement?.ResetPosition();

            if (isDebugMode)
            {
                Debug.Log($"[CharacterController] Character {gameObject.name} reset");
            }
        }

        /// <summary>
        /// キャラクターの死亡処理
        /// </summary>
        public void OnCharacterDeath()
        {
            // 入力を無効化
            inputManager?.SetInputEnabled(false);
            
            // 武器を無効化
            weapon?.SetWeaponEnabled(false);

            if (isDebugMode)
            {
                Debug.Log($"[CharacterController] Character {gameObject.name} died");
            }
        }

        /// <summary>
        /// スコア加算時の処理（敵撃破時など）
        /// </summary>
        /// <param name="scoreAmount">加算スコア</param>
        public void OnScoreGained(int scoreAmount)
        {
            // 武器のアップグレード処理などを呼び出し
            weapon?.OnEnemyDefeated();

            if (isDebugMode)
            {
                Debug.Log($"[CharacterController] Score gained: {scoreAmount}");
            }
        }
        #endregion

        #region デバッグ
        private void OnDrawGizmosSelected()
        {
            if (isDebugMode && movement != null)
            {
                // 移動範囲の可視化
                movement.DrawDebugGizmos();
            }
        }
        #endregion
    }
}