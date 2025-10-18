using UnityEngine;

public class Player : MonoBehaviour
{
    // === 静的プレイヤー参照機能 ===
    private static Player instance;
    
    /// <summary>
    /// プレイヤーのインスタンス
    /// </summary>
    public static Player Instance => instance;
    
    /// <summary>
    /// プレイヤーのTransform
    /// </summary>
    public static Transform PlayerTransform => instance?.transform;
    
    /// <summary>
    /// プレイヤーの現在位置
    /// </summary>
    public static Vector3 PlayerPosition => instance != null ? instance.transform.position : Vector3.zero;
    
    /// <summary>
    /// プレイヤーが設定されているかチェック
    /// </summary>
    public static bool IsPlayerSet() => instance != null;

    // === 既存のプレイヤー機能 ===
    public Animator anim {  get; private set; }
    public Rigidbody rb { get; private set; }

    public PlayerInputSet input;
    public StateMachine stateMachine { get; private set; }




    public PlayerIdleState idleState {  get; private set; }
    public PlayerMoveState moveState {  get; private set; }






    [Header("�X�e�[�^�X")]
    //[SerializeField] private int health = 30;

    public float moveSpeed;
    [SerializeField] private float mutekiTime;
    private float nowMutekiTime;

    //[Header("�ړ����x")]
    public Vector2 moveInput {  get; private set; }


    [Space]
    [Header("�ړ�����")]
    [SerializeField] public Vector2 positionLimitHigh;
    [SerializeField] public Vector2 positionLimitLow;





    private void Awake()
    {
        // Singletonの設定
        if (instance == null)
        {
            instance = this;
            Debug.Log("Player: プレイヤーインスタンスを設定しました");
        }
        else if (instance != this)
        {
            Debug.LogWarning("Player: 複数のプレイヤーが存在します。古いインスタンスを破棄します。");
            Destroy(gameObject);
            return;
        }
        
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        stateMachine = new StateMachine();
        input = new PlayerInputSet();

        //�X�e�[�g�� = new �N���X��(this, stateMachine, "animator�Őݒ肵��bool��")
        idleState = new PlayerIdleState(this, stateMachine, "fly");
        moveState = new PlayerMoveState(this, stateMachine, "fly");
    }


    private void OnEnable()
    {
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.UpdateActiveState();

        DecreaseMutekiTime();

        
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


    public void Damage()
    {
        //�_���[�W�̏���

        if (nowMutekiTime > 0f)
            return;

        //�_���[�W�����ǉ�
        Debug.Log("Player damaged");



        nowMutekiTime = mutekiTime;
    }


    private void PlayerActive()
    {
        gameObject.SetActive(true);
    }

    private void PlayerInactive()
    {
        gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // インスタンスをクリア
        if (instance == this)
        {
            instance = null;
            Debug.Log("Player: プレイヤーインスタンスをクリアしました");
        }
    }

}
