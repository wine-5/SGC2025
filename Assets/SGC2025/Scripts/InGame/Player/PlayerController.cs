using UnityEngine;
using UnityEngine.InputSystem;
using SGC2025.Core;
using SGC2025.Bullet;
using SGC2025.Audio;
using SGC2025.Manager;
using SGC2025.Item;

namespace SGC2025.Player
{
    /// <summary>
    /// プレイヤーキャラクターの管理
    /// </summary>
    public class PlayerController : MonoBehaviour, IDamageable
    {
        #region プロパティ
        public Animator anim { get; private set; }
        public Vector2 moveInput { get; private set; }
        public PlayerInputSet PlayerInput { get; private set; }

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f;
        #endregion

        #region IDamageableイベント
        public event System.Action<float> OnDamageTaken;
        public event System.Action OnDeath;
        #endregion

        #region フィールド
        private const float BOUNDARY_MARGIN = 0.5f;

        private Rigidbody2D rb;
        [Header("武器システム")]
        private PlayerWeaponSystem weaponSystem;

        [Header("ステータス")]
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float currentHealth;

        private const float DAMAGE = 10f;
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
            PlayerInput = new PlayerInputSet();
        }

        private void OnEnable()
        {
            PlayerInput.Enable();
            PlayerInput.Player.Movement.performed += OnMovementPerformed;
            PlayerInput.Player.Movement.canceled += OnMovementCanceled;
            PlayerInput.Player.Shot.performed += OnShotPerformed;
            PlayerInput.Player.Pause.performed += OnPausePerformed;
            PlayerInput.Player.MiniMapExpand.started += OnMiniMapExpandStarted;
            PlayerInput.Player.MiniMapExpand.canceled += OnMiniMapExpandCanceled;

            EventBus.Subscribe<ItemEffectActivatedEvent>(OnItemEffectActivatedEvent);
            EventBus.Subscribe<ItemEffectExpiredEvent>(OnItemEffectExpiredEvent);
        }

        private void OnDisable()
        {
            PlayerInput.Player.Movement.performed -= OnMovementPerformed;
            PlayerInput.Player.Movement.canceled -= OnMovementCanceled;
            PlayerInput.Player.Shot.performed -= OnShotPerformed;
            PlayerInput.Player.Pause.performed -= OnPausePerformed;
            PlayerInput.Player.MiniMapExpand.started -= OnMiniMapExpandStarted;
            PlayerInput.Player.MiniMapExpand.canceled -= OnMiniMapExpandCanceled;
            PlayerInput.Disable();

            EventBus.Unsubscribe<ItemEffectActivatedEvent>(OnItemEffectActivatedEvent);
            EventBus.Unsubscribe<ItemEffectExpiredEvent>(OnItemEffectExpiredEvent);
        }

        private void Start()
        {
            currentHealth = maxHealth;
            baseMovSpeed = moveSpeed;
            anim.SetBool("fly", true);
            if (GroundManager.I != null)
                transform.position = GroundManager.I.GetPlayerSpawnPosition();
            
            if (PlayerDataProvider.I != null)
                PlayerDataProvider.I.RegisterPlayer(transform);
        }

        private void Update()
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            
            HandleMovement();
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
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (InGameManager.I == null) return;

            if (InGameManager.I.IsPaused)
                InGameManager.I.Resume();
            else
                InGameManager.I.Pause();
        }

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            moveInput = Vector2.zero;
        }

        private void OnShotPerformed(InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            if (weaponSystem == null) return;
            weaponSystem.Fire();
        }

        private void OnMiniMapExpandStarted(InputAction.CallbackContext context)
        {
            EventBus.Publish(new MiniMapExpandStartedEvent());
        }

        private void OnMiniMapExpandCanceled(InputAction.CallbackContext context)
        {
            EventBus.Publish(new MiniMapExpandCanceledEvent());
        }
        #endregion

        #region 移動処理
        private void HandleMovement()
        {
            if (moveInput == Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (GroundManager.I == null || GroundManager.I.MapData == null)
            {
                rb.linearVelocity = moveInput.normalized * moveSpeed;
                return;
            }

            var mapData = GroundManager.I.MapData;
            Vector2 limitHigh = new Vector2(
                mapData.MapMaxWorldPosition.x - BOUNDARY_MARGIN,
                mapData.MapMaxWorldPosition.y - BOUNDARY_MARGIN
            );
            Vector2 limitLow = new Vector2(BOUNDARY_MARGIN, BOUNDARY_MARGIN);

            Vector3 currentPos = transform.position;
            bool isOutOfBounds = limitHigh.x < currentPos.x || limitHigh.y < currentPos.y ||
                                 limitLow.x > currentPos.x || limitLow.y > currentPos.y;

            if (isOutOfBounds)
            {
                transform.position = new Vector2(
                    Mathf.Clamp(currentPos.x, limitLow.x, limitHigh.x),
                    Mathf.Clamp(currentPos.y, limitLow.y, limitHigh.y)
                );
            }
            else
            {
                rb.linearVelocity = moveInput.normalized * moveSpeed;
            }
        }

        private void PlayerRotate()
        {
            if (moveInput != Vector2.zero)
                transform.up = rb.linearVelocity;
        }
        #endregion

        #region ヘルスシステム
        private void Damage()
        {
            if (nowMutekiTime > 0f) return;
            TakeDamage(DAMAGE);
            nowMutekiTime = mutekiTime;
            AudioManager.I?.PlaySE(SEType.PlayerDamage);
        }

        /// <summary>ダメージを受ける</summary>
        public void TakeDamage(float damage)
        {
            if (damage <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);

            OnDamageTaken?.Invoke(damage);

            float hpRate = maxHealth > 0f ? currentHealth / maxHealth : 0f;
            EventBus.Publish(new PlayerDamagedEvent(hpRate));
            if (currentHealth <= 0f)
            {
                OnDeath?.Invoke();
                EventBus.Publish(new PlayerDiedEvent());
            }
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
