using UnityEngine;
using UnityEngine.InputSystem;
using SGC2025.Core;
using SGC2025.Player.Bullet;
using SGC2025.Audio;
using SGC2025.Manager;
using SGC2025.Item;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーキャラクターの管理
    /// </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        #region プロパティ
        public Animator anim { get; private set; }
        private Rigidbody2D rb;
        private PlayerInputSet input;
        private StateMachine stateMachine;
        public PlayerIdleState idleState { get; private set; }
        public PlayerMoveState moveState { get; private set; }
        public Vector2 moveInput { get; private set; }
        #endregion

        #region フィールド
        [Header("武器システム")]
        private PlayerWeaponSystem weaponSystem;

        [Header("ステータス")]
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float damage = 10;
        [SerializeField] private float currentHealth;
        private float baseMovSpeed;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float mutekiTime;
        private float nowMutekiTime;

        public bool IsInvincible => nowMutekiTime > 0f;
        #endregion

        #region Unityライフサイクル
        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody2D>();
            weaponSystem = GetComponent<PlayerWeaponSystem>();
            stateMachine = new StateMachine();
            input = new PlayerInputSet();
            idleState = new PlayerIdleState(this, stateMachine, "fly");
            moveState = new PlayerMoveState(this, stateMachine, "fly");
        }

        private void OnEnable()
        {
            input.Enable();
            input.Player.Movement.performed += OnMovementPerformed;
            input.Player.Movement.canceled += OnMovementCanceled;
            input.Player.Shot.performed += OnShotPerformed;
            input.Player.Pause.performed += OnPausePerformed;
            
            EventBus.Subscribe<ItemEffectActivatedEvent>(OnItemEffectActivatedEvent);
            EventBus.Subscribe<ItemEffectExpiredEvent>(OnItemEffectExpiredEvent);
        }

        private void OnDisable()
        {
            input.Player.Movement.performed -= OnMovementPerformed;
            input.Player.Movement.canceled -= OnMovementCanceled;
            input.Player.Shot.performed -= OnShotPerformed;
            input.Player.Pause.performed -= OnPausePerformed;
            input.Disable();
            
            EventBus.Unsubscribe<ItemEffectActivatedEvent>(OnItemEffectActivatedEvent);
            EventBus.Unsubscribe<ItemEffectExpiredEvent>(OnItemEffectExpiredEvent);
        }

        private void Start()
        {
            stateMachine.Initialize(idleState);
            currentHealth = maxHealth;
            baseMovSpeed = moveSpeed;
            if (GroundManager.I != null)
                transform.position = GroundManager.I.GetPlayerSpawnPosition();
            
            if (PlayerDataProvider.I != null)
                PlayerDataProvider.I.RegisterPlayer(transform);
        }

        private void Update()
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            
            stateMachine.UpdateActiveState();
            DecreaseMutekiTime();
            PlayerRotate();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == GameLayers.EnemyLayer)
                Damage();
        }
        #endregion

        #region 入力処理
        private void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (InGameManager.I == null) return;

            if (InGameManager.I.IsPaused)
                InGameManager.I.Resume();
            else
                InGameManager.I.Pause();
        }

        private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            moveInput = Vector2.zero;
        }

        private void OnShotPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            if (weaponSystem == null) return;
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
        public float GetPlayerCurrentHealth() => currentHealth;

        private void Damage()
        {
            if (nowMutekiTime > 0f) return;
            TakeDamage(damage);
            nowMutekiTime = mutekiTime;
            AudioManager.I?.PlaySE(SEType.PlayerDamage);
        }

        /// <summary>ダメージを受ける</summary>
        private void TakeDamage(float damage)
        {
            if (damage <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            
            float hpRate = maxHealth > 0f ? currentHealth / maxHealth : 0f;
            EventBus.Publish(new PlayerDamagedEvent(hpRate));
            if (currentHealth <= 0f)
                EventBus.Publish(new PlayerDiedEvent());
        }

        private void DecreaseMutekiTime() => nowMutekiTime -= Time.deltaTime;
        #endregion

        #region アイテム効果
        private void OnItemEffectActivatedEvent(ItemEffectActivatedEvent e)
        {
            if (e.ItemType == ItemType.SpeedBoost)
                ApplySpeedBoost(e.EffectValue);
        }

        private void OnItemEffectExpiredEvent(ItemEffectExpiredEvent e)
        {
            if (e.ItemType == ItemType.SpeedBoost)
                ResetSpeed();
        }
        
        /// <summary>
        /// 移動速度上昇を適用
        /// </summary>
        private void ApplySpeedBoost(float multiplier)
        {
            moveSpeed = baseMovSpeed * multiplier;
        }
        
        /// <summary>
        /// 移動速度を元に戻す
        /// </summary>
        private void ResetSpeed()
        {
            moveSpeed = baseMovSpeed;
        }
        #endregion
    }
}
