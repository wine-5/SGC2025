using UnityEngine;
using UnityEngine.InputSystem;
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
        public Rigidbody2D rb { get; private set; }
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
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float damage = 10;
        [SerializeField] private float currentHealth;
        private float baseMovSpeed;
        public float moveSpeed;
        [SerializeField] private float mutekiTime;
        private float nowMutekiTime;

        public bool IsInvincible => nowMutekiTime > 0f;

        public static event System.Action OnPlayerDeath;
        public static event System.Action<float> OnPlayerDamaged;
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
            
            // アイテム効果イベントの購読
            ItemManager.OnItemEffectActivated += OnItemEffectActivated;
            ItemManager.OnItemEffectExpired += OnItemEffectExpired;
        }

        private void OnDisable()
        {
            input.Player.Movement.performed -= OnMovementPerformed;
            input.Player.Movement.canceled -= OnMovementCanceled;
            input.Player.Shot.performed -= OnShotPerformed;
            input.Disable();
            
            // アイテム効果イベントの購読解除
            ItemManager.OnItemEffectActivated -= OnItemEffectActivated;
            ItemManager.OnItemEffectExpired -= OnItemEffectExpired;
        }

        private void Start()
        {
            stateMachine.Initialize(idleState);
            currentHealth = maxHealth;
            baseMovSpeed = moveSpeed;
            if (GroundManager.I != null)
                transform.position = GroundManager.I.GetPlayerSpawnPosition();
        }

        private void Update()
        {
            if (GameManager.I != null && GameManager.I.IsCountingDown) return;
            
            stateMachine.UpdateActiveState();
            DecreaseMutekiTime();
            PlayerRotate();
            HandlePauseInput();
        }

        private void OnTriggerEnter2D(Collider2D other)
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

        private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (GameManager.I != null && GameManager.I.IsCountingDown) return;
            
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (GameManager.I != null && GameManager.I.IsCountingDown) return;
            
            moveInput = Vector2.zero;
        }

        private void OnShotPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (GameManager.I != null && GameManager.I.IsCountingDown) return;
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

        public void Damage()
        {
            if (nowMutekiTime > 0f) return;
            TakeDamage(damage);
            nowMutekiTime = mutekiTime;
            
            float hpRate = currentHealth / maxHealth;
            OnPlayerDamaged?.Invoke(hpRate);
            
            AudioManager.I?.PlaySE(SEType.PlayerDamage);
        }

        /// <summary>ダメージを受ける</summary>
        public void TakeDamage(float damage)
        {
            if (damage <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            
            // ダメージイベント発火 (HP割合を渡す)
            float hpRate = maxHealth > 0f ? currentHealth / maxHealth : 0f;
            OnPlayerDamaged?.Invoke(hpRate);
            
            if (currentHealth <= 0f)
                OnPlayerDeath?.Invoke();
        }

        private void DecreaseMutekiTime() => nowMutekiTime -= Time.deltaTime;
        #endregion

        #region アイテム効果
        /// <summary>
        /// アイテム効果が適用された時の処理
        /// </summary>
        private void OnItemEffectActivated(ItemType itemType, float effectValue, float duration)
        {
            switch (itemType)
            {
                case ItemType.SpeedBoost:
                    ApplySpeedBoost(effectValue);
                    break;
                case ItemType.ScoreMultiplier:
                    // ScoreManagerで処理されるため、ここでは何もしない
                    break;
            }
        }
        
        /// <summary>
        /// アイテム効果が切れた時の処理
        /// </summary>
        private void OnItemEffectExpired(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.SpeedBoost:
                    ResetSpeed();
                    break;
                case ItemType.ScoreMultiplier:
                    // ScoreManagerで処理されるため、ここでは何もしない
                    break;
            }
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
