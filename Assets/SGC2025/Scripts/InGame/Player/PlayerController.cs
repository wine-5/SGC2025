using UnityEngine;
using UnityEngine.InputSystem;
using Tyotyo.Core;
using Tyotyo.InGame.Bullet;
using Tyotyo.Audio;
using Tyotyo.Manager;
using Tyotyo.InGame.Item;

namespace Tyotyo.InGame.Player
{
    /// <summary>
    /// プレイヤーキャラクターの管理
    /// </summary>
    public class PlayerController : MonoBehaviour, IDamageable
    {
        #region プロパティ
        public Animator Anim { get; private set; }
        public Vector2 MoveInput { get; private set; }
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

        // 敵側で攻撃力が取得できなかった場合のフォールバックダメージ
        private const float DAMAGE = 10f;
        private float baseMoveSpeed;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float invincibleDuration;
        private float invincibleTimer;

        public bool IsInvincible => invincibleTimer > 0f;
        #endregion

        #region Unityライフサイクル
        private void Awake()
        {
            Anim = GetComponentInChildren<Animator>();
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
            baseMoveSpeed = moveSpeed;
            Anim.SetBool("fly", true);
            if (GroundManager.I != null)
                transform.position = GroundManager.I.GetPlayerSpawnPosition();
            
            if (PlayerDataProvider.I != null)
                PlayerDataProvider.I.RegisterPlayer(transform);
        }

        private void Update()
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            
            HandleMovement();
            DecreaseInvincibleTimer();
            PlayerRotate();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != GameLayers.EnemyLayer) return;

            // 接触した敵のIAttackerから攻撃力を取得し、その分のダメージを受ける
            float damage = other.GetComponentInParent<IAttacker>()?.AttackPower ?? DAMAGE;
            Damage(damage);
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
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && InGameManager.I.IsCountingDown) return;
            MoveInput = Vector2.zero;
        }

        private void OnShotPerformed(InputAction.CallbackContext context)
        {
            if (InGameManager.I != null && (InGameManager.I.IsCountingDown || InGameManager.I.IsPaused)) return;
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
            if (MoveInput == Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (GroundManager.I == null || GroundManager.I.MapData == null)
            {
                rb.linearVelocity = MoveInput.normalized * moveSpeed;
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
                rb.linearVelocity = MoveInput.normalized * moveSpeed;
            }
        }

        private void PlayerRotate()
        {
            if (MoveInput != Vector2.zero)
                transform.up = rb.linearVelocity;
        }
        #endregion

        #region ヘルスシステム
        private void Damage(float damage)
        {
            if (invincibleTimer > 0f) return;
            TakeDamage(damage);
            invincibleTimer = invincibleDuration;
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

        private void DecreaseInvincibleTimer() => invincibleTimer -= Time.deltaTime;
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
            moveSpeed = baseMoveSpeed * multiplier;
        }
        
        /// <summary>
        /// 移動速度を元に戻す
        /// </summary>
        private void ResetSpeed()
        {
            moveSpeed = baseMoveSpeed;
        }
        #endregion
    }
}
