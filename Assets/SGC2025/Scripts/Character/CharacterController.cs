using UnityEngine;

namespace SGC2025.Character
{
    /// <summary>
    /// キャラクター（プレイヤー）の各Handlerを統合管理する軽量なコントローラー
    /// 各Handler間の連携を調整し、外部からのアクセスポイントを提供
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        [Header("コンポーネント")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private CharacterMovement movement;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private CharacterWeapon weapon;
        [SerializeField] private CharacterCollisionHandler collision;

        /// <summary>入力処理へのアクセス</summary>
        public PlayerInputManager Input => inputManager;
        
        /// <summary>移動処理へのアクセス</summary>
        public CharacterMovement Movement => movement;
        
        /// <summary>ヘルス管理へのアクセス</summary>
        public CharacterHealth Health => health;
        
        /// <summary>武器システムへのアクセス</summary>
        public CharacterWeapon Weapon => weapon;
        
        /// <summary>衝突処理へのアクセス</summary>
        public CharacterCollisionHandler Collision => collision;

        /// <summary>キャラクターが生存しているか</summary>
        public bool IsAlive => health != null && health.IsAlive;

        private void Awake()
        {
            InitializeHandlers();
        }

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
            if (collision == null) collision = GetComponent<CharacterCollisionHandler>();

            // 各コンポーネントの初期化
            inputManager?.Initialize(this);
            movement?.Initialize(this);
            health?.Initialize(this);
            weapon?.Initialize(this);
            collision?.Initialize(this);
        }

        /// <summary>
        /// キャラクターの完全なリセット（ゲーム開始時など）
        /// </summary>
        public void ResetCharacter()
        {
            health?.ResetHealth();
            weapon?.ResetWeapon();
            movement?.ResetPosition();

        }

        /// <summary>
        
        /// </summary>
        public void OnCharacterDeath()
        {
            // 入力を無効化
            inputManager?.SetInputEnabled(false);
            
            // 武器を無効化
            weapon?.SetWeaponEnabled(false);

        }
    }
}