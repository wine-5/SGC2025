using UnityEngine;
using UnityEngine.InputSystem;
using SGC2025.Player.Bullet;

namespace SGC2025
{
    /// <summary>
    /// プレイヤーキャラクターの管理
    /// </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        #region 定数
        private const float DEFAULT_MAX_HEALTH = 100f;
        private const float DEFAULT_DAMAGE = 10f;
        #endregion

        #region プロパティ
        public Animator anim { get; private set; }
        public Rigidbody rb { get; private set; }
        public PlayerInputSet input;
        public StateMachine stateMachine { get; private set; }
        public PlayerIdleState idleState { get; private set; }
        public PlayerMoveState moveState { get; private set; }
        public Vector2 moveInput { get; private set; }
        #endregion

        #region フィールド
        [Header("武器システム")]
        private PlayerWeaponSystem weaponSystem;

        [Header("ステータス")]
        [SerializeField] private float maxHealth = DEFAULT_MAX_HEALTH;
        [SerializeField] private float currentHealth;
        public float moveSpeed;
        [SerializeField] private float mutekiTime;
        private float nowMutekiTime;

        public static event System.Action OnPlayerDeath;
        #endregion

        #region Unityライフサイクル
        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody>();
            weaponSystem = GetComponent<PlayerWeaponSystem>();
            stateMachine = new StateMachine();
            input = new PlayerInputSet();
            if (weaponSystem == null)
                weaponSystem = GetComponent<SGC2025.Player.Bullet.PlayerWeaponSystem>();
            idleState = new PlayerIdleState(this, stateMachine, "fly");
            moveState = new PlayerMoveState(this, stateMachine, "fly");
        }

        private void OnEnable()
        {
            input.Enable();
            input.Player.Movement.performed += OnMovementPerformed;
            input.Player.Movement.canceled += OnMovementCanceled;
            input.Player.Shot.performed += OnShotPerformed;
        }

        private void OnDisable()
        {
            input.Player.Movement.performed -= OnMovementPerformed;
            input.Player.Movement.canceled -= OnMovementCanceled;
            input.Player.Shot.performed -= OnShotPerformed;
            input.Disable();
        }

        private void Start()
        {
            stateMachine.Initialize(idleState);
            currentHealth = maxHealth;
            if (GroundManager.I != null)
                transform.position = GroundManager.I.GetPlayerSpawnPosition();
        }

        private void Update()
        {
            stateMachine.UpdateActiveState();
            DecreaseMutekiTime();
            PlayerRotate();
            HandlePauseInput();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                Damage();
        }
        #endregion

        #region 入力処理
        /// <summary>ポーズ入力処理</summary>
        private void HandlePauseInput()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame != true) return;
            if (GameManager.I == null) return;
            
            if (GameManager.I.IsPaused)
                GameManager.I.ResumeGame();
            else
                GameManager.I.PauseGame();
        }

        private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context) =>
            moveInput = context.ReadValue<Vector2>();

        private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context) =>
            moveInput = Vector2.zero;

        private void OnShotPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (weaponSystem == null) return;
            
            // 弾発射SEを再生
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.PlayerShoot);
            
            weaponSystem.Fire();
        }
        #endregion

        #region 移動処理
        public void SetVelocity(float moveInputX, float moveInputY)
        {
            Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
            rb.linearVelocity = moveInputNormalized * moveSpeed;
        }

        private void PlayerRotate()
        {
            if (moveInput != Vector2.zero)
                transform.up = rb.linearVelocity;
        }
        #endregion

        #region ヘルスシステム
        /// <summary>最大HP取得</summary>
        public float GetPlayerMaxHealth() => maxHealth;

        /// <summary>現在HP取得</summary>
        public float GetPlayerCurrentHalth() => currentHealth;

        public void Damage()
        {
            if (nowMutekiTime > 0f) return;
            TakeDamage(DEFAULT_DAMAGE);
            nowMutekiTime = mutekiTime;
        }

        /// <summary>ダメージを受ける</summary>
        public void TakeDamage(float damage)
        {
            if (damage <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            if (currentHealth <= 0f)
                OnPlayerDeath?.Invoke();
        }

        private void DecreaseMutekiTime() => nowMutekiTime -= Time.deltaTime;
        #endregion
    }
}
