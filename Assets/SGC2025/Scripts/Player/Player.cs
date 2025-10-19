using UnityEngine;
using SGC2025.Player.Bullet;

public class Player : MonoBehaviour
{

    public Animator anim {  get; private set; }
    public Rigidbody rb { get; private set; }

    public PlayerInputSet input;
    public StateMachine stateMachine { get; private set; }


    [Header("武器システム")]
    [SerializeField] private PlayerWeaponSystem weaponSystem;

    public PlayerIdleState idleState {  get; private set; }
    public PlayerMoveState moveState {  get; private set; }






    [Header("�X�e�[�^�X")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float moveSpeed;
    [SerializeField] private float mutekiTime;
    private float nowMutekiTime;
    
    // HPイベント
    public static event System.Action OnPlayerDeath;

    //[Header("�ړ����x")]
    public Vector2 moveInput {  get; private set; }


    [Space]
    [Header("�ړ�����")]
    [SerializeField] public Vector2 positionLimitHigh;
    [SerializeField] public Vector2 positionLimitLow;





    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        stateMachine = new StateMachine();
        input = new PlayerInputSet();

        // PlayerWeaponSystemが設定されていない場合は自動で取得
        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<PlayerWeaponSystem>();
        }

        //ステート名 = new クラス名(this, stateMachine, "animatorで設定したbool名")
        idleState = new PlayerIdleState(this, stateMachine, "fly");
        moveState = new PlayerMoveState(this, stateMachine, "fly");
    }


    private void OnEnable()
    {
        input.Enable();

        // 移動入力の処理
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCanceled;
        
        // 射撃入力の処理
        input.Player.Shot.performed += OnShotPerformed;
    }

    private void OnDisable()
    {
        // 入力イベントの登録解除
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCanceled;
        input.Player.Shot.performed -= OnShotPerformed;
        
        input.Disable();
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
        
        // HP初期化
        currentHealth = maxHealth;
        
        // 手動発射モードを有効にする（スペースキーで弾を発射できるように）
        if (weaponSystem != null)
        {
            weaponSystem.SetManualFiring(true);
            Debug.Log("[Player] 手動発射モードを有効にしました");
        }
    }

    private void Update()
    {
        stateMachine.UpdateActiveState();

        DecreaseMutekiTime();
        PlayerRotate();
    }

    private void PlayerRotate()
    {
        //�v���C���[�̉�]
        if(moveInput != Vector2.zero)
            transform.up = rb.linearVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        //�_���[�W����
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Damage();
        }
    }


    public void SetVelocity(float moveInputX, float moveInputY)
    {
        Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
        rb.linearVelocity = new Vector2(moveInputNormalized.x * moveSpeed, moveInputNormalized.y * moveSpeed);

    }


    //private void PositionLimit()
    //{

    //}

    private void DecreaseMutekiTime()
    {
        nowMutekiTime -= Time.deltaTime;
    }

    /// <summary>
    /// 移動入力開始時の処理
    /// </summary>
    private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"[Player] 移動入力: {moveInput}");
    }

    /// <summary>
    /// 移動入力終了時の処理
    /// </summary>
    private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        Debug.Log("[Player] 移動入力停止");
    }

    /// <summary>
    /// 射撃入力時の処理
    /// </summary>
    private void OnShotPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            weaponSystem.Fire();
            Debug.Log("[Player] 射撃実行");
        }
        else
        {
            Debug.LogWarning("[Player] PlayerWeaponSystemが設定されていません");
        }
    }

    public void Damage()
    {
        
        if (nowMutekiTime > 0f)
            return;

        TakeDamage(10f); // デフォルトダメージ10
        nowMutekiTime = mutekiTime;
    }
    
    /// <summary>
    /// HPBarController用：最大HP取得
    /// </summary>
    public float GetPlayerMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// HPBarController用：現在HP取得
    /// </summary>
    public float GetPlayerCurrentHalth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        Debug.Log($"[Player] ダメージを受けました - HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0f)
        {
            Debug.Log("[Player] プレイヤーが死亡しました");
            OnPlayerDeath?.Invoke();
        }
    }

    //�v���C���[�̗L����
    private void PlayerActive()
    {
        gameObject.SetActive(true);
    }

    //�v���C���[�̔�L����
    private void PlayerInactive()
    {
        gameObject.SetActive(false);
    }

}
